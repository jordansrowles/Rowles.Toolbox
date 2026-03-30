using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace Rowles.Toolbox.Core.DataFormats;

public static class MessagePackVisualizerCore
{
    public sealed class MsgPackNode
    {
        public string Format { get; set; } = "";
        public string Category { get; set; } = "";
        public string DisplayValue { get; set; } = "";
        public List<MsgPackNode>? ArrayItems { get; set; }
        public List<(MsgPackNode Key, MsgPackNode Value)>? MapEntries { get; set; }
        public sbyte ExtTypeCode { get; set; }
        public int Offset { get; set; }
        public int ByteLength { get; set; }
    }

    public sealed class TreeLine
    {
        public int Depth { get; set; }
        public string Format { get; set; } = "";
        public string Category { get; set; } = "";
        public string Prefix { get; set; } = "";
        public string DisplayValue { get; set; } = "";
        public int Offset { get; set; }
        public int ByteLength { get; set; }
    }

    public static readonly Dictionary<string, string> Samples = new()
    {
        ["simple"] = "82A46E616D65A5416C696365A36167651E",
        ["nested"] = "9301920203A568656C6C6F",
        ["mixed"] = "84A6616374697665C3A573636F7265CB40091EB851EB851FA47461677392A161A162A464617461C0",
    };

    // -- Hex parsing --

    public static byte[] ParseHex(string input)
    {
        var clean = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (IsHexChar(c))
                clean.Append(c);
        }

        string hex = clean.ToString();
        if (hex.Length == 0)
            throw new FormatException("No valid hex digits found in input.");
        if (hex.Length % 2 != 0)
            throw new FormatException("Hex string must contain an even number of hex digits.");

        byte[] result = new byte[hex.Length / 2];
        for (int i = 0; i < result.Length; i++)
            result[i] = byte.Parse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return result;
    }

    private static bool IsHexChar(char c) =>
        (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');

    // -- MessagePack decoder --

    private const int MaxDepth = 100;

    private static void EnsureBytes(byte[] data, int offset, int count)
    {
        if (offset + count > data.Length)
            throw new FormatException(
                $"Unexpected end of data at offset 0x{offset:X}: need {count} byte(s) but only {data.Length - offset} remaining.");
    }

    public static MsgPackNode Decode(byte[] data, ref int offset, int depth)
    {
        if (depth > MaxDepth)
            throw new FormatException($"Maximum nesting depth ({MaxDepth}) exceeded at offset 0x{offset:X}.");

        EnsureBytes(data, offset, 1);
        int start = offset;
        byte b = data[offset++];

        // -- Positive fixint: 0x00-0x7f --
        if (b <= 0x7f)
        {
            return new MsgPackNode
            {
                Format = "positive fixint", Category = "int",
                DisplayValue = b.ToString(),
                Offset = start, ByteLength = 1
            };
        }

        // -- Fixmap: 0x80-0x8f --
        if (b <= 0x8f)
        {
            int count = b & 0x0f;
            var entries = new List<(MsgPackNode Key, MsgPackNode Value)>(count);
            for (int i = 0; i < count; i++)
            {
                var key = Decode(data, ref offset, depth + 1);
                var val = Decode(data, ref offset, depth + 1);
                entries.Add((key, val));
            }
            return new MsgPackNode
            {
                Format = "fixmap", Category = "map",
                DisplayValue = $"{count} {(count == 1 ? "entry" : "entries")}",
                MapEntries = entries,
                Offset = start, ByteLength = offset - start
            };
        }

        // -- Fixarray: 0x90-0x9f --
        if (b <= 0x9f)
        {
            int count = b & 0x0f;
            var items = new List<MsgPackNode>(count);
            for (int i = 0; i < count; i++)
                items.Add(Decode(data, ref offset, depth + 1));
            return new MsgPackNode
            {
                Format = "fixarray", Category = "array",
                DisplayValue = $"{count} {(count == 1 ? "item" : "items")}",
                ArrayItems = items,
                Offset = start, ByteLength = offset - start
            };
        }

        // -- Fixstr: 0xa0-0xbf --
        if (b <= 0xbf)
        {
            int len = b & 0x1f;
            EnsureBytes(data, offset, len);
            string s = System.Text.Encoding.UTF8.GetString(data, offset, len);
            offset += len;
            return new MsgPackNode
            {
                Format = "fixstr", Category = "str",
                DisplayValue = s,
                Offset = start, ByteLength = offset - start
            };
        }

        // -- Format codes 0xc0-0xff --
        switch (b)
        {
            case 0xc0:
                return new MsgPackNode
                {
                    Format = "nil", Category = "nil",
                    DisplayValue = "null",
                    Offset = start, ByteLength = 1
                };

            case 0xc1:
                throw new FormatException(
                    $"Invalid format byte 0xC1 at offset 0x{start:X}. This code is reserved and must not appear in valid MessagePack data.");

            case 0xc2:
                return new MsgPackNode
                {
                    Format = "false", Category = "bool",
                    DisplayValue = "false",
                    Offset = start, ByteLength = 1
                };

            case 0xc3:
                return new MsgPackNode
                {
                    Format = "true", Category = "bool",
                    DisplayValue = "true",
                    Offset = start, ByteLength = 1
                };

            // -- bin 8/16/32 --
            case 0xc4:
            {
                EnsureBytes(data, offset, 1);
                int len = data[offset++];
                EnsureBytes(data, offset, len);
                string hex = FormatBinHex(data, offset, len);
                offset += len;
                return new MsgPackNode { Format = "bin 8", Category = "bin", DisplayValue = hex, Offset = start, ByteLength = offset - start };
            }
            case 0xc5:
            {
                EnsureBytes(data, offset, 2);
                int len = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
                offset += 2;
                EnsureBytes(data, offset, len);
                string hex = FormatBinHex(data, offset, len);
                offset += len;
                return new MsgPackNode { Format = "bin 16", Category = "bin", DisplayValue = hex, Offset = start, ByteLength = offset - start };
            }
            case 0xc6:
            {
                EnsureBytes(data, offset, 4);
                uint len = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
                offset += 4;
                if (len > int.MaxValue) throw new FormatException("bin 32 length exceeds maximum.");
                EnsureBytes(data, offset, (int)len);
                string hex = FormatBinHex(data, offset, (int)len);
                offset += (int)len;
                return new MsgPackNode { Format = "bin 32", Category = "bin", DisplayValue = hex, Offset = start, ByteLength = offset - start };
            }

            // -- ext 8/16/32 --
            case 0xc7:
            {
                EnsureBytes(data, offset, 2);
                int len = data[offset++];
                sbyte extType = (sbyte)data[offset++];
                EnsureBytes(data, offset, len);
                string hex = FormatBinHex(data, offset, len);
                offset += len;
                return new MsgPackNode { Format = "ext 8", Category = "ext", DisplayValue = hex, ExtTypeCode = extType, Offset = start, ByteLength = offset - start };
            }
            case 0xc8:
            {
                EnsureBytes(data, offset, 3);
                int len = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
                offset += 2;
                sbyte extType = (sbyte)data[offset++];
                EnsureBytes(data, offset, len);
                string hex = FormatBinHex(data, offset, len);
                offset += len;
                return new MsgPackNode { Format = "ext 16", Category = "ext", DisplayValue = hex, ExtTypeCode = extType, Offset = start, ByteLength = offset - start };
            }
            case 0xc9:
            {
                EnsureBytes(data, offset, 5);
                uint len = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
                offset += 4;
                sbyte extType = (sbyte)data[offset++];
                if (len > int.MaxValue) throw new FormatException("ext 32 length exceeds maximum.");
                EnsureBytes(data, offset, (int)len);
                string hex = FormatBinHex(data, offset, (int)len);
                offset += (int)len;
                return new MsgPackNode { Format = "ext 32", Category = "ext", DisplayValue = hex, ExtTypeCode = extType, Offset = start, ByteLength = offset - start };
            }

            // -- float 32/64 --
            case 0xca:
            {
                EnsureBytes(data, offset, 4);
                float val = BinaryPrimitives.ReadSingleBigEndian(data.AsSpan(offset));
                offset += 4;
                return new MsgPackNode { Format = "float 32", Category = "float", DisplayValue = val.ToString("G", CultureInfo.InvariantCulture), Offset = start, ByteLength = 5 };
            }
            case 0xcb:
            {
                EnsureBytes(data, offset, 8);
                double val = BinaryPrimitives.ReadDoubleBigEndian(data.AsSpan(offset));
                offset += 8;
                return new MsgPackNode { Format = "float 64", Category = "float", DisplayValue = val.ToString("G", CultureInfo.InvariantCulture), Offset = start, ByteLength = 9 };
            }

            // -- uint 8/16/32/64 --
            case 0xcc:
            {
                EnsureBytes(data, offset, 1);
                byte val = data[offset++];
                return new MsgPackNode { Format = "uint 8", Category = "int", DisplayValue = val.ToString(), Offset = start, ByteLength = 2 };
            }
            case 0xcd:
            {
                EnsureBytes(data, offset, 2);
                ushort val = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
                offset += 2;
                return new MsgPackNode { Format = "uint 16", Category = "int", DisplayValue = val.ToString(), Offset = start, ByteLength = 3 };
            }
            case 0xce:
            {
                EnsureBytes(data, offset, 4);
                uint val = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
                offset += 4;
                return new MsgPackNode { Format = "uint 32", Category = "int", DisplayValue = val.ToString(), Offset = start, ByteLength = 5 };
            }
            case 0xcf:
            {
                EnsureBytes(data, offset, 8);
                ulong val = BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(offset));
                offset += 8;
                return new MsgPackNode { Format = "uint 64", Category = "int", DisplayValue = val.ToString(), Offset = start, ByteLength = 9 };
            }

            // -- int 8/16/32/64 --
            case 0xd0:
            {
                EnsureBytes(data, offset, 1);
                sbyte val = (sbyte)data[offset++];
                return new MsgPackNode { Format = "int 8", Category = "int", DisplayValue = val.ToString(), Offset = start, ByteLength = 2 };
            }
            case 0xd1:
            {
                EnsureBytes(data, offset, 2);
                short val = BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(offset));
                offset += 2;
                return new MsgPackNode { Format = "int 16", Category = "int", DisplayValue = val.ToString(), Offset = start, ByteLength = 3 };
            }
            case 0xd2:
            {
                EnsureBytes(data, offset, 4);
                int val = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offset));
                offset += 4;
                return new MsgPackNode { Format = "int 32", Category = "int", DisplayValue = val.ToString(), Offset = start, ByteLength = 5 };
            }
            case 0xd3:
            {
                EnsureBytes(data, offset, 8);
                long val = BinaryPrimitives.ReadInt64BigEndian(data.AsSpan(offset));
                offset += 8;
                return new MsgPackNode { Format = "int 64", Category = "int", DisplayValue = val.ToString(), Offset = start, ByteLength = 9 };
            }

            // -- fixext 1/2/4/8/16 --
            case 0xd4:
            {
                EnsureBytes(data, offset, 2);
                sbyte extType = (sbyte)data[offset++];
                string hex = data[offset].ToString("X2");
                offset += 1;
                return new MsgPackNode { Format = "fixext 1", Category = "ext", DisplayValue = hex, ExtTypeCode = extType, Offset = start, ByteLength = 3 };
            }
            case 0xd5:
            {
                EnsureBytes(data, offset, 3);
                sbyte extType = (sbyte)data[offset++];
                string hex = BitConverter.ToString(data, offset, 2).Replace("-", " ");
                offset += 2;
                return new MsgPackNode { Format = "fixext 2", Category = "ext", DisplayValue = hex, ExtTypeCode = extType, Offset = start, ByteLength = 4 };
            }
            case 0xd6:
            {
                EnsureBytes(data, offset, 5);
                sbyte extType = (sbyte)data[offset++];
                string hex = BitConverter.ToString(data, offset, 4).Replace("-", " ");
                offset += 4;
                return new MsgPackNode { Format = "fixext 4", Category = "ext", DisplayValue = hex, ExtTypeCode = extType, Offset = start, ByteLength = 6 };
            }
            case 0xd7:
            {
                EnsureBytes(data, offset, 9);
                sbyte extType = (sbyte)data[offset++];
                string hex = BitConverter.ToString(data, offset, 8).Replace("-", " ");
                offset += 8;
                return new MsgPackNode { Format = "fixext 8", Category = "ext", DisplayValue = hex, ExtTypeCode = extType, Offset = start, ByteLength = 10 };
            }
            case 0xd8:
            {
                EnsureBytes(data, offset, 17);
                sbyte extType = (sbyte)data[offset++];
                string hex = BitConverter.ToString(data, offset, 16).Replace("-", " ");
                offset += 16;
                return new MsgPackNode { Format = "fixext 16", Category = "ext", DisplayValue = hex, ExtTypeCode = extType, Offset = start, ByteLength = 18 };
            }

            // -- str 8/16/32 --
            case 0xd9:
            {
                EnsureBytes(data, offset, 1);
                int len = data[offset++];
                EnsureBytes(data, offset, len);
                string s = System.Text.Encoding.UTF8.GetString(data, offset, len);
                offset += len;
                return new MsgPackNode { Format = "str 8", Category = "str", DisplayValue = s, Offset = start, ByteLength = offset - start };
            }
            case 0xda:
            {
                EnsureBytes(data, offset, 2);
                int len = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
                offset += 2;
                EnsureBytes(data, offset, len);
                string s = System.Text.Encoding.UTF8.GetString(data, offset, len);
                offset += len;
                return new MsgPackNode { Format = "str 16", Category = "str", DisplayValue = s, Offset = start, ByteLength = offset - start };
            }
            case 0xdb:
            {
                EnsureBytes(data, offset, 4);
                uint len = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
                offset += 4;
                if (len > int.MaxValue) throw new FormatException("str 32 length exceeds maximum.");
                EnsureBytes(data, offset, (int)len);
                string s = System.Text.Encoding.UTF8.GetString(data, offset, (int)len);
                offset += (int)len;
                return new MsgPackNode { Format = "str 32", Category = "str", DisplayValue = s, Offset = start, ByteLength = offset - start };
            }

            // -- array 16/32 --
            case 0xdc:
            {
                EnsureBytes(data, offset, 2);
                int count = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
                offset += 2;
                var items = new List<MsgPackNode>(Math.Min(count, 1024));
                for (int i = 0; i < count; i++)
                    items.Add(Decode(data, ref offset, depth + 1));
                return new MsgPackNode
                {
                    Format = "array 16", Category = "array",
                    DisplayValue = $"{count} {(count == 1 ? "item" : "items")}",
                    ArrayItems = items, Offset = start, ByteLength = offset - start
                };
            }
            case 0xdd:
            {
                EnsureBytes(data, offset, 4);
                uint count = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
                offset += 4;
                if (count > int.MaxValue) throw new FormatException("array 32 count exceeds maximum.");
                var items = new List<MsgPackNode>(Math.Min((int)count, 1024));
                for (uint i = 0; i < count; i++)
                    items.Add(Decode(data, ref offset, depth + 1));
                return new MsgPackNode
                {
                    Format = "array 32", Category = "array",
                    DisplayValue = $"{count} {(count == 1 ? "item" : "items")}",
                    ArrayItems = items, Offset = start, ByteLength = offset - start
                };
            }

            // -- map 16/32 --
            case 0xde:
            {
                EnsureBytes(data, offset, 2);
                int count = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
                offset += 2;
                var entries = new List<(MsgPackNode Key, MsgPackNode Value)>(Math.Min(count, 1024));
                for (int i = 0; i < count; i++)
                {
                    var key = Decode(data, ref offset, depth + 1);
                    var val = Decode(data, ref offset, depth + 1);
                    entries.Add((key, val));
                }
                return new MsgPackNode
                {
                    Format = "map 16", Category = "map",
                    DisplayValue = $"{count} {(count == 1 ? "entry" : "entries")}",
                    MapEntries = entries, Offset = start, ByteLength = offset - start
                };
            }
            case 0xdf:
            {
                EnsureBytes(data, offset, 4);
                uint count = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
                offset += 4;
                if (count > int.MaxValue) throw new FormatException("map 32 count exceeds maximum.");
                var entries = new List<(MsgPackNode Key, MsgPackNode Value)>(Math.Min((int)count, 1024));
                for (uint i = 0; i < count; i++)
                {
                    var key = Decode(data, ref offset, depth + 1);
                    var val = Decode(data, ref offset, depth + 1);
                    entries.Add((key, val));
                }
                return new MsgPackNode
                {
                    Format = "map 32", Category = "map",
                    DisplayValue = $"{count} {(count == 1 ? "entry" : "entries")}",
                    MapEntries = entries, Offset = start, ByteLength = offset - start
                };
            }

            // -- Negative fixint: 0xe0-0xff --
            default:
            {
                sbyte val = (sbyte)b;
                return new MsgPackNode
                {
                    Format = "negative fixint", Category = "int",
                    DisplayValue = val.ToString(),
                    Offset = start, ByteLength = 1
                };
            }
        }
    }

    private static string FormatBinHex(byte[] data, int offset, int len)
    {
        if (len == 0) return "(empty)";
        if (len > 256) return $"({len} bytes)";
        return BitConverter.ToString(data, offset, len).Replace("-", " ");
    }

    // -- Tree flattening --

    public static void Flatten(MsgPackNode node, int depth, string prefix, List<TreeLine> lines)
    {
        string display = node.DisplayValue;
        if (node.Category == "str")
            display = $"\"{display}\"";
        else if (node.Category == "ext")
            display = $"type={node.ExtTypeCode}: {display}";

        lines.Add(new TreeLine
        {
            Depth = depth,
            Format = node.Format,
            Category = node.Category,
            Prefix = prefix,
            DisplayValue = display,
            Offset = node.Offset,
            ByteLength = node.ByteLength
        });

        if (node.ArrayItems is not null)
        {
            for (int i = 0; i < node.ArrayItems.Count; i++)
                Flatten(node.ArrayItems[i], depth + 1, $"[{i}]", lines);
        }

        if (node.MapEntries is not null)
        {
            foreach (var (key, value) in node.MapEntries)
            {
                string keyLabel = key.Category == "str"
                    ? $"\"{key.DisplayValue}\""
                    : key.DisplayValue;
                Flatten(value, depth + 1, $"{keyLabel} =>", lines);
            }
        }
    }

    // -- JSON generation --

    public static void AppendJson(MsgPackNode node, StringBuilder sb, int indent)
    {
        string pad = new(' ', indent * 2);
        string inner = new(' ', (indent + 1) * 2);

        switch (node.Category)
        {
            case "nil":
                sb.Append("null");
                break;

            case "bool":
                sb.Append(node.DisplayValue);
                break;

            case "int":
            case "float":
                sb.Append(node.DisplayValue);
                break;

            case "str":
                sb.Append('"');
                AppendJsonEscaped(sb, node.DisplayValue);
                sb.Append('"');
                break;

            case "bin":
                sb.Append("\"<bin: ");
                sb.Append(node.DisplayValue);
                sb.Append(">\"");
                break;

            case "ext":
                sb.Append("{\"__ext_type\": ");
                sb.Append(node.ExtTypeCode);
                sb.Append(", \"__ext_data\": \"");
                sb.Append(node.DisplayValue);
                sb.Append("\"}");
                break;

            case "array":
                if (node.ArrayItems is null || node.ArrayItems.Count == 0)
                {
                    sb.Append("[]");
                }
                else
                {
                    sb.Append("[\n");
                    for (int i = 0; i < node.ArrayItems.Count; i++)
                    {
                        sb.Append(inner);
                        AppendJson(node.ArrayItems[i], sb, indent + 1);
                        if (i < node.ArrayItems.Count - 1) sb.Append(',');
                        sb.Append('\n');
                    }
                    sb.Append(pad);
                    sb.Append(']');
                }
                break;

            case "map":
                if (node.MapEntries is null || node.MapEntries.Count == 0)
                {
                    sb.Append("{}");
                }
                else
                {
                    sb.Append("{\n");
                    for (int i = 0; i < node.MapEntries.Count; i++)
                    {
                        var (key, val) = node.MapEntries[i];
                        sb.Append(inner);
                        if (key.Category == "str")
                        {
                            sb.Append('"');
                            AppendJsonEscaped(sb, key.DisplayValue);
                            sb.Append('"');
                        }
                        else
                        {
                            sb.Append('"');
                            sb.Append(key.DisplayValue);
                            sb.Append('"');
                        }
                        sb.Append(": ");
                        AppendJson(val, sb, indent + 1);
                        if (i < node.MapEntries.Count - 1) sb.Append(',');
                        sb.Append('\n');
                    }
                    sb.Append(pad);
                    sb.Append('}');
                }
                break;

            default:
                sb.Append('"');
                sb.Append(node.DisplayValue);
                sb.Append('"');
                break;
        }
    }

    private static void AppendJsonEscaped(StringBuilder sb, string s)
    {
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                default:
                    if (c < 0x20)
                        sb.Append($"\\u{(int)c:X4}");
                    else
                        sb.Append(c);
                    break;
            }
        }
    }

    // -- UI helpers --

    public static string GetBadgeColor(string category) => category switch
    {
        "nil" => "bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-300",
        "bool" => "bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300",
        "int" => "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300",
        "float" => "bg-teal-100 dark:bg-teal-900/30 text-teal-700 dark:text-teal-300",
        "str" => "bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300",
        "bin" => "bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-300",
        "array" => "bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300",
        "map" => "bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300",
        "ext" => "bg-pink-100 dark:bg-pink-900/30 text-pink-700 dark:text-pink-300",
        _ => "bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400"
    };

    public static string FormatByteRange(int offset, int length)
    {
        if (length <= 1)
            return $"0x{offset:X2}";
        return $"0x{offset:X2}..0x{offset + length - 1:X2} ({length}B)";
    }

    public static string GetByteColor(byte b)
    {
        if (b >= 0x20 && b <= 0x7E)
            return "text-green-700 dark:text-green-400";
        if (b <= 0x1F || b == 0x7F)
            return "text-red-600 dark:text-red-400";
        return "text-blue-600 dark:text-blue-400";
    }
}
