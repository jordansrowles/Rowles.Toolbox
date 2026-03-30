using System.Text;

namespace Rowles.Toolbox.Core.DataFormats;

public static class ProtobufInspectorCore
{
    public enum InputFormat { Hex, Base64 }

    public sealed class FlatRow
    {
        public ProtoField Field { get; set; } = default!;
        public int Depth { get; set; }
    }

    public sealed class ProtoField
    {
        public int FieldNumber { get; set; }
        public int WireType { get; set; }
        public string WireTypeName { get; set; } = string.Empty;
        public ulong RawVarint { get; set; }
        public byte[] RawBytes { get; set; } = [];
        public List<ProtoField>? Children { get; set; }
        public string? StringValue { get; set; }
        public string DisplayValue { get; set; } = string.Empty;
        public List<string> Interpretations { get; set; } = [];
    }

    public static List<FlatRow> FlattenFields(List<ProtoField> fields, int depth)
    {
        var rows = new List<FlatRow>();
        foreach (var field in fields)
        {
            rows.Add(new FlatRow { Field = field, Depth = depth });
            if (field.Children is not null && field.Children.Count > 0)
            {
                rows.AddRange(FlattenFields(field.Children, depth + 1));
            }
        }
        return rows;
    }

    public static byte[] ParseInputBytes(string input, InputFormat inputFormat)
    {
        string cleaned = input.Trim().Replace(" ", "").Replace("-", "").Replace("\n", "").Replace("\r", "");
        if (inputFormat == InputFormat.Base64)
        {
            return Convert.FromBase64String(cleaned);
        }

        if (cleaned.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            cleaned = cleaned[2..];

        if (cleaned.Length % 2 != 0)
            throw new FormatException("Hex string must have an even number of characters.");

        byte[] result = new byte[cleaned.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
        }
        return result;
    }

    public static List<ProtoField> DecodeMessage(byte[] data, int offset, int limit, int depth)
    {
        if (depth > 10)
            throw new InvalidOperationException("Maximum nesting depth exceeded.");

        var fields = new List<ProtoField>();
        int pos = offset;

        while (pos < limit)
        {
            if (!TryReadVarint(data, ref pos, limit, out ulong tag))
                break;

            int fieldNumber = (int)(tag >> 3);
            int wireType = (int)(tag & 0x07);

            if (fieldNumber == 0)
                throw new InvalidOperationException("Invalid field number 0.");

            var field = new ProtoField
            {
                FieldNumber = fieldNumber,
                WireType = wireType,
                WireTypeName = GetWireTypeName(wireType)
            };

            switch (wireType)
            {
                case 0: // Varint
                    if (!TryReadVarint(data, ref pos, limit, out ulong varint))
                        throw new InvalidOperationException($"Truncated varint at field {fieldNumber}.");
                    field.RawVarint = varint;
                    field.DisplayValue = varint.ToString();
                    field.Interpretations = GetVarintInterpretations(varint);
                    break;

                case 1: // 64-bit fixed
                    if (pos + 8 > limit)
                        throw new InvalidOperationException($"Truncated 64-bit field {fieldNumber}.");
                    field.RawBytes = new byte[8];
                    Array.Copy(data, pos, field.RawBytes, 0, 8);
                    ulong fixed64 = BitConverter.ToUInt64(data, pos);
                    double dbl = BitConverter.ToDouble(data, pos);
                    pos += 8;
                    field.DisplayValue = $"0x{fixed64:X16}";
                    field.Interpretations =
                    [
                        $"fixed64: {fixed64}",
                        $"sfixed64: {(long)fixed64}",
                        $"double: {dbl:G17}"
                    ];
                    break;

                case 2: // Length-delimited
                    if (!TryReadVarint(data, ref pos, limit, out ulong length))
                        throw new InvalidOperationException($"Truncated length at field {fieldNumber}.");
                    int len = (int)length;
                    if (pos + len > limit)
                        throw new InvalidOperationException($"Length-delimited field {fieldNumber} exceeds data bounds.");
                    field.RawBytes = new byte[len];
                    Array.Copy(data, pos, field.RawBytes, 0, len);

                    string? strVal = TryDecodeUtf8(field.RawBytes);
                    field.StringValue = strVal;

                    List<ProtoField>? nested = TryDecodeNestedMessage(data, pos, len, depth + 1);
                    if (nested is not null && nested.Count > 0)
                    {
                        field.Children = nested;
                    }

                    string hexStr = BytesToHex(field.RawBytes);
                    field.DisplayValue = strVal is not null && IsPrintable(strVal)
                        ? $"\"{strVal}\""
                        : $"[{len} bytes] {hexStr}";

                    field.Interpretations = [];
                    if (strVal is not null && IsPrintable(strVal))
                        field.Interpretations.Add($"string: \"{strVal}\"");
                    if (nested is not null && nested.Count > 0)
                        field.Interpretations.Add($"embedded message: {nested.Count} field(s)");
                    field.Interpretations.Add($"bytes: {hexStr}");

                    pos += len;
                    break;

                case 5: // 32-bit fixed
                    if (pos + 4 > limit)
                        throw new InvalidOperationException($"Truncated 32-bit field {fieldNumber}.");
                    field.RawBytes = new byte[4];
                    Array.Copy(data, pos, field.RawBytes, 0, 4);
                    uint fixed32 = BitConverter.ToUInt32(data, pos);
                    float flt = BitConverter.ToSingle(data, pos);
                    pos += 4;
                    field.DisplayValue = $"0x{fixed32:X8}";
                    field.Interpretations =
                    [
                        $"fixed32: {fixed32}",
                        $"sfixed32: {(int)fixed32}",
                        $"float: {flt:G9}"
                    ];
                    break;

                default:
                    throw new InvalidOperationException($"Unknown wire type {wireType} at field {fieldNumber}.");
            }

            fields.Add(field);
        }

        return fields;
    }

    public static string BuildJson(List<ProtoField> fields, int indent)
    {
        var sb = new StringBuilder();
        string pad = new(' ', indent * 2);
        string innerPad = new(' ', (indent + 1) * 2);

        sb.AppendLine($"{pad}{{");

        for (int i = 0; i < fields.Count; i++)
        {
            var f = fields[i];
            string comma = i < fields.Count - 1 ? "," : "";

            if (f.Children is not null && f.Children.Count > 0)
            {
                sb.Append($"{innerPad}\"field_{f.FieldNumber}\": ");
                string nestedJson = BuildJson(f.Children, indent + 1);
                sb.Append(nestedJson.TrimStart());
                sb.AppendLine(comma);
            }
            else if (f.WireType == 0)
            {
                sb.AppendLine($"{innerPad}\"field_{f.FieldNumber}\": {f.RawVarint}{comma}");
            }
            else if (f.WireType == 2 && f.StringValue is not null && IsPrintable(f.StringValue))
            {
                string escaped = f.StringValue
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t");
                sb.AppendLine($"{innerPad}\"field_{f.FieldNumber}\": \"{escaped}\"{comma}");
            }
            else if (f.WireType == 1)
            {
                double dblVal = BitConverter.ToDouble(f.RawBytes, 0);
                sb.AppendLine($"{innerPad}\"field_{f.FieldNumber}\": {dblVal:G17}{comma}");
            }
            else if (f.WireType == 5)
            {
                float fltVal = BitConverter.ToSingle(f.RawBytes, 0);
                sb.AppendLine($"{innerPad}\"field_{f.FieldNumber}\": {fltVal:G9}{comma}");
            }
            else
            {
                sb.AppendLine($"{innerPad}\"field_{f.FieldNumber}\": \"{BytesToHex(f.RawBytes)}\"{comma}");
            }
        }

        sb.Append($"{pad}}}");
        return sb.ToString();
    }

    // -- Varint reader (LEB128) --
    private static bool TryReadVarint(byte[] data, ref int pos, int limit, out ulong value)
    {
        value = 0;
        int shift = 0;
        while (pos < limit)
        {
            byte b = data[pos++];
            value |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0)
                return true;
            shift += 7;
            if (shift >= 64)
                throw new InvalidOperationException("Varint too long.");
        }
        return false;
    }

    private static string GetWireTypeName(int wireType) => wireType switch
    {
        0 => "Varint (0)",
        1 => "64-bit (1)",
        2 => "Length-delimited (2)",
        5 => "32-bit (5)",
        _ => $"Unknown ({wireType})"
    };

    private static List<string> GetVarintInterpretations(ulong raw)
    {
        var interps = new List<string>
        {
            $"uint64: {raw}"
        };

        long signed = (long)raw;
        interps.Add($"int64: {signed}");

        if (raw <= uint.MaxValue)
            interps.Add($"uint32: {(uint)raw}");

        int int32val = (int)(uint)raw;
        interps.Add($"int32: {int32val}");

        // ZigZag decode for sint32/sint64
        long zigzag64 = (long)((raw >> 1) ^ (~(raw & 1) + 1));
        interps.Add($"sint64: {zigzag64}");
        interps.Add($"sint32: {(int)zigzag64}");

        if (raw == 0 || raw == 1)
            interps.Add($"bool: {(raw == 1 ? "true" : "false")}");

        return interps;
    }

    private static string? TryDecodeUtf8(byte[] bytes)
    {
        try
        {
            string s = System.Text.Encoding.UTF8.GetString(bytes);
            byte[] roundTrip = System.Text.Encoding.UTF8.GetBytes(s);
            if (roundTrip.Length != bytes.Length)
                return null;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (roundTrip[i] != bytes[i]) return null;
            }
            return s;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsPrintable(string s)
    {
        foreach (char c in s)
        {
            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
                return false;
        }
        return true;
    }

    private static List<ProtoField>? TryDecodeNestedMessage(byte[] data, int offset, int length, int depth)
    {
        try
        {
            var nested = DecodeMessage(data, offset, offset + length, depth);
            if (nested.Count > 0)
                return nested;
        }
        catch
        {
            // Not a valid nested message
        }
        return null;
    }

    private static string BytesToHex(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
