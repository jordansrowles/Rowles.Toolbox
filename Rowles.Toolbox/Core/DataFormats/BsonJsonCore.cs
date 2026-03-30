using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Rowles.Toolbox.Core.DataFormats;

public static class BsonJsonCore
{
    public sealed class BsonStats
    {
        public int DocumentSize { get; set; }
        public int FieldCount { get; set; }
        public int NestingDepth { get; set; }
    }

    public sealed class BsonToJsonResult
    {
        public string? Json { get; set; }
        public List<string> TypeBadges { get; set; } = [];
        public BsonStats? Stats { get; set; }
        public string? Error { get; set; }
    }

    public sealed class JsonToBsonResult
    {
        public string? BsonHex { get; set; }
        public List<string> TypeBadges { get; set; } = [];
        public BsonStats? Stats { get; set; }
        public string? Error { get; set; }
    }

    // BSON type constants
    private const byte BsonDouble = 0x01;
    private const byte BsonString = 0x02;
    private const byte BsonDocument = 0x03;
    private const byte BsonArray = 0x04;
    private const byte BsonBinary = 0x05;
    private const byte BsonObjectId = 0x07;
    private const byte BsonBoolean = 0x08;
    private const byte BsonDateTime = 0x09;
    private const byte BsonNull = 0x0A;
    private const byte BsonInt32 = 0x10;
    private const byte BsonInt64 = 0x12;

    private static readonly Dictionary<byte, string> TypeNames = new()
    {
        [BsonDouble] = "double",
        [BsonString] = "string",
        [BsonDocument] = "document",
        [BsonArray] = "array",
        [BsonBinary] = "binary",
        [BsonObjectId] = "ObjectId",
        [BsonBoolean] = "boolean",
        [BsonDateTime] = "datetime",
        [BsonNull] = "null",
        [BsonInt32] = "int32",
        [BsonInt64] = "int64",
    };

    public static BsonToJsonResult ConvertBsonToJson(string bsonText)
    {
        if (string.IsNullOrWhiteSpace(bsonText))
            return new BsonToJsonResult { Error = "BSON input is empty." };

        try
        {
            byte[] bytes = ParseInputBytes(bsonText.Trim());
            HashSet<byte> seenTypes = new();
            int fieldCount = 0;
            int maxDepth = 0;
            object? doc = ReadDocument(bytes, 0, out _, seenTypes, ref fieldCount, 1, ref maxDepth);
            JsonSerializerOptions opts = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(doc, opts);

            List<string> badges = seenTypes
                .Where(t => TypeNames.ContainsKey(t))
                .OrderBy(t => t)
                .Select(t => TypeNames[t])
                .ToList();

            return new BsonToJsonResult
            {
                Json = json,
                TypeBadges = badges,
                Stats = new BsonStats
                {
                    DocumentSize = bytes.Length,
                    FieldCount = fieldCount,
                    NestingDepth = maxDepth,
                }
            };
        }
        catch (Exception ex)
        {
            return new BsonToJsonResult { Error = $"BSON parse error: {ex.Message}" };
        }
    }

    public static JsonToBsonResult ConvertJsonToBson(string jsonText)
    {
        if (string.IsNullOrWhiteSpace(jsonText))
            return new JsonToBsonResult { Error = "JSON input is empty." };

        try
        {
            using JsonDocument doc = JsonDocument.Parse(jsonText);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return new JsonToBsonResult { Error = "BSON requires a top-level object (not an array or primitive)." };

            HashSet<byte> seenTypes = new();
            int fieldCount = 0;
            int maxDepth = 0;
            byte[] bson = WriteDocument(doc.RootElement, seenTypes, ref fieldCount, 1, ref maxDepth);

            StringBuilder hex = new(bson.Length * 3);
            for (int i = 0; i < bson.Length; i++)
            {
                if (i > 0) hex.Append(' ');
                hex.Append(bson[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            List<string> badges = seenTypes
                .Where(t => TypeNames.ContainsKey(t))
                .OrderBy(t => t)
                .Select(t => TypeNames[t])
                .ToList();

            return new JsonToBsonResult
            {
                BsonHex = hex.ToString(),
                TypeBadges = badges,
                Stats = new BsonStats
                {
                    DocumentSize = bson.Length,
                    FieldCount = fieldCount,
                    NestingDepth = maxDepth,
                }
            };
        }
        catch (JsonException ex)
        {
            return new JsonToBsonResult { Error = $"JSON parse error: {ex.Message}" };
        }
        catch (Exception ex)
        {
            return new JsonToBsonResult { Error = $"BSON encode error: {ex.Message}" };
        }
    }

    public static string BuildMongoSampleHex()
    {
        using MemoryStream ms = new();
        ms.Write(new byte[4], 0, 4);

        // _id: ObjectId (0x07)
        ms.WriteByte(BsonObjectId);
        WriteCStringToStream(ms, "_id");
        ms.Write(new byte[] { 0x50, 0x7F, 0x1F, 0x77, 0xBC, 0xF8, 0x6C, 0xD7, 0x99, 0x43, 0x90, 0x11 }, 0, 12);

        // name: "Sample" (0x02)
        ms.WriteByte(BsonString);
        WriteCStringToStream(ms, "name");
        byte[] nameVal = System.Text.Encoding.UTF8.GetBytes("Sample");
        ms.Write(BitConverter.GetBytes(nameVal.Length + 1), 0, 4);
        ms.Write(nameVal, 0, nameVal.Length);
        ms.WriteByte(0x00);

        // createdAt: DateTime (0x09)
        ms.WriteByte(BsonDateTime);
        WriteCStringToStream(ms, "createdAt");
        ms.Write(BitConverter.GetBytes(1673784000000L), 0, 8);

        // active: true (0x08)
        ms.WriteByte(BsonBoolean);
        WriteCStringToStream(ms, "active");
        ms.WriteByte(0x01);

        // count: 100 (0x10)
        ms.WriteByte(BsonInt32);
        WriteCStringToStream(ms, "count");
        ms.Write(BitConverter.GetBytes(100), 0, 4);

        // rating: 4.8 (0x01)
        ms.WriteByte(BsonDouble);
        WriteCStringToStream(ms, "rating");
        ms.Write(BitConverter.GetBytes(4.8), 0, 8);

        ms.WriteByte(0x00); // terminator

        byte[] result = ms.ToArray();
        byte[] sizeBytes = BitConverter.GetBytes(result.Length);
        Array.Copy(sizeBytes, 0, result, 0, 4);

        StringBuilder hex = new(result.Length * 3);
        for (int i = 0; i < result.Length; i++)
        {
            if (i > 0) hex.Append(' ');
            hex.Append(result[i].ToString("X2", CultureInfo.InvariantCulture));
        }
        return hex.ToString();
    }

    // -- Input parsing --

    private static byte[] ParseInputBytes(string input)
    {
        string stripped = input.Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");

        if (stripped.Length >= 2 && stripped.Length % 2 == 0 && IsAllHex(stripped))
        {
            return HexToBytes(stripped);
        }

        try
        {
            return Convert.FromBase64String(stripped);
        }
        catch
        {
            throw new FormatException("Input is not valid hex or Base64.");
        }
    }

    private static bool IsAllHex(string s)
    {
        foreach (char c in s)
        {
            if (!Uri.IsHexDigit(c)) return false;
        }
        return true;
    }

    private static byte[] HexToBytes(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = byte.Parse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
        return bytes;
    }

    // -- BSON reading --

    private static Dictionary<string, object?> ReadDocument(
        byte[] data, int offset, out int bytesRead,
        HashSet<byte> seenTypes, ref int fieldCount, int depth, ref int maxDepth)
    {
        if (depth > maxDepth) maxDepth = depth;

        if (offset + 4 > data.Length)
            throw new FormatException("Unexpected end of data reading document size.");

        int docSize = BitConverter.ToInt32(data, offset);
        if (docSize < 5 || offset + docSize > data.Length)
            throw new FormatException($"Invalid document size: {docSize} at offset {offset}.");

        Dictionary<string, object?> result = new();
        int pos = offset + 4;
        int docEnd = offset + docSize - 1;

        while (pos < docEnd)
        {
            if (pos >= data.Length)
                throw new FormatException("Unexpected end of data reading element type.");

            byte elementType = data[pos];
            pos++;

            if (elementType == 0x00) break;

            string name = ReadCString(data, pos, out int nameLen);
            pos += nameLen;
            fieldCount++;

            seenTypes.Add(elementType);

            object? value = ReadValue(data, pos, elementType, out int valueLen, seenTypes, ref fieldCount, depth, ref maxDepth);
            pos += valueLen;

            result[name] = value;
        }

        if (pos < data.Length && data[pos] == 0x00) pos++;

        bytesRead = docSize;
        return result;
    }

    private static List<object?> ReadArray(
        byte[] data, int offset, out int bytesRead,
        HashSet<byte> seenTypes, ref int fieldCount, int depth, ref int maxDepth)
    {
        if (depth > maxDepth) maxDepth = depth;

        if (offset + 4 > data.Length)
            throw new FormatException("Unexpected end of data reading array size.");

        int docSize = BitConverter.ToInt32(data, offset);
        if (docSize < 5 || offset + docSize > data.Length)
            throw new FormatException($"Invalid array size: {docSize} at offset {offset}.");

        List<object?> result = new();
        int pos = offset + 4;
        int docEnd = offset + docSize - 1;

        while (pos < docEnd)
        {
            if (pos >= data.Length)
                throw new FormatException("Unexpected end of data reading array element type.");

            byte elementType = data[pos];
            pos++;

            if (elementType == 0x00) break;

            ReadCString(data, pos, out int nameLen);
            pos += nameLen;
            fieldCount++;

            seenTypes.Add(elementType);

            object? value = ReadValue(data, pos, elementType, out int valueLen, seenTypes, ref fieldCount, depth, ref maxDepth);
            pos += valueLen;

            result.Add(value);
        }

        if (pos < data.Length && data[pos] == 0x00) pos++;

        bytesRead = docSize;
        return result;
    }

    private static object? ReadValue(
        byte[] data, int offset, byte type, out int bytesRead,
        HashSet<byte> seenTypes, ref int fieldCount, int depth, ref int maxDepth)
    {
        switch (type)
        {
            case BsonDouble:
                EnsureBytes(data, offset, 8);
                bytesRead = 8;
                return BitConverter.ToDouble(data, offset);

            case BsonString:
            {
                EnsureBytes(data, offset, 4);
                int strLen = BitConverter.ToInt32(data, offset);
                if (strLen < 1) throw new FormatException("Invalid BSON string length.");
                EnsureBytes(data, offset + 4, strLen);
                string s = System.Text.Encoding.UTF8.GetString(data, offset + 4, strLen - 1);
                bytesRead = 4 + strLen;
                return s;
            }

            case BsonDocument:
            {
                var doc = ReadDocument(data, offset, out bytesRead, seenTypes, ref fieldCount, depth + 1, ref maxDepth);
                return doc;
            }

            case BsonArray:
            {
                var arr = ReadArray(data, offset, out bytesRead, seenTypes, ref fieldCount, depth + 1, ref maxDepth);
                return arr;
            }

            case BsonBinary:
            {
                EnsureBytes(data, offset, 5);
                int binLen = BitConverter.ToInt32(data, offset);
                EnsureBytes(data, offset + 5, binLen);
                byte[] binData = new byte[binLen];
                Array.Copy(data, offset + 5, binData, 0, binLen);
                bytesRead = 5 + binLen;
                return Convert.ToBase64String(binData);
            }

            case BsonObjectId:
            {
                EnsureBytes(data, offset, 12);
                StringBuilder sb = new(24);
                for (int i = 0; i < 12; i++)
                    sb.Append(data[offset + i].ToString("x2", CultureInfo.InvariantCulture));
                bytesRead = 12;
                return sb.ToString();
            }

            case BsonBoolean:
                EnsureBytes(data, offset, 1);
                bytesRead = 1;
                return data[offset] != 0;

            case BsonDateTime:
            {
                EnsureBytes(data, offset, 8);
                long ms = BitConverter.ToInt64(data, offset);
                DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(ms);
                bytesRead = 8;
                return dto.ToString("o", CultureInfo.InvariantCulture);
            }

            case BsonNull:
                bytesRead = 0;
                return null;

            case BsonInt32:
                EnsureBytes(data, offset, 4);
                bytesRead = 4;
                return BitConverter.ToInt32(data, offset);

            case BsonInt64:
                EnsureBytes(data, offset, 8);
                bytesRead = 8;
                return BitConverter.ToInt64(data, offset);

            default:
                throw new FormatException($"Unsupported BSON type: 0x{type:X2}.");
        }
    }

    private static string ReadCString(byte[] data, int offset, out int bytesRead)
    {
        int end = Array.IndexOf(data, (byte)0x00, offset);
        if (end < 0)
            throw new FormatException("Unterminated cstring in BSON data.");
        int len = end - offset;
        string s = System.Text.Encoding.UTF8.GetString(data, offset, len);
        bytesRead = len + 1;
        return s;
    }

    private static void EnsureBytes(byte[] data, int offset, int count)
    {
        if (offset + count > data.Length)
            throw new FormatException($"Unexpected end of data at offset {offset}, need {count} bytes.");
    }

    // -- BSON writing --

    private static byte[] WriteDocument(JsonElement element, HashSet<byte> seenTypes, ref int fieldCount, int depth, ref int maxDepth)
    {
        if (depth > maxDepth) maxDepth = depth;

        using MemoryStream ms = new();
        ms.Write(new byte[4], 0, 4);

        foreach (JsonProperty prop in element.EnumerateObject())
        {
            fieldCount++;
            WriteElement(ms, prop.Name, prop.Value, seenTypes, ref fieldCount, depth, ref maxDepth);
        }

        ms.WriteByte(0x00);

        byte[] result = ms.ToArray();
        byte[] sizeBytes = BitConverter.GetBytes(result.Length);
        Array.Copy(sizeBytes, 0, result, 0, 4);
        return result;
    }

    private static byte[] WriteArrayDocument(JsonElement element, HashSet<byte> seenTypes, ref int fieldCount, int depth, ref int maxDepth)
    {
        if (depth > maxDepth) maxDepth = depth;

        using MemoryStream ms = new();
        ms.Write(new byte[4], 0, 4);

        int idx = 0;
        foreach (JsonElement item in element.EnumerateArray())
        {
            fieldCount++;
            WriteElement(ms, idx.ToString(CultureInfo.InvariantCulture), item, seenTypes, ref fieldCount, depth, ref maxDepth);
            idx++;
        }

        ms.WriteByte(0x00);

        byte[] result = ms.ToArray();
        byte[] sizeBytes = BitConverter.GetBytes(result.Length);
        Array.Copy(sizeBytes, 0, result, 0, 4);
        return result;
    }

    private static void WriteElement(MemoryStream ms, string name, JsonElement value, HashSet<byte> seenTypes, ref int fieldCount, int depth, ref int maxDepth)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(name);

        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                seenTypes.Add(BsonDocument);
                ms.WriteByte(BsonDocument);
                ms.Write(nameBytes, 0, nameBytes.Length);
                ms.WriteByte(0x00);
                byte[] subdoc = WriteDocument(value, seenTypes, ref fieldCount, depth + 1, ref maxDepth);
                ms.Write(subdoc, 0, subdoc.Length);
                break;

            case JsonValueKind.Array:
                seenTypes.Add(BsonArray);
                ms.WriteByte(BsonArray);
                ms.Write(nameBytes, 0, nameBytes.Length);
                ms.WriteByte(0x00);
                byte[] arr = WriteArrayDocument(value, seenTypes, ref fieldCount, depth + 1, ref maxDepth);
                ms.Write(arr, 0, arr.Length);
                break;

            case JsonValueKind.String:
            {
                string str = value.GetString() ?? string.Empty;
                seenTypes.Add(BsonString);
                ms.WriteByte(BsonString);
                ms.Write(nameBytes, 0, nameBytes.Length);
                ms.WriteByte(0x00);
                byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
                byte[] strLen = BitConverter.GetBytes(strBytes.Length + 1);
                ms.Write(strLen, 0, 4);
                ms.Write(strBytes, 0, strBytes.Length);
                ms.WriteByte(0x00);
                break;
            }

            case JsonValueKind.Number:
            {
                if (value.TryGetInt32(out int i32))
                {
                    seenTypes.Add(BsonInt32);
                    ms.WriteByte(BsonInt32);
                    ms.Write(nameBytes, 0, nameBytes.Length);
                    ms.WriteByte(0x00);
                    ms.Write(BitConverter.GetBytes(i32), 0, 4);
                }
                else if (value.TryGetInt64(out long i64))
                {
                    seenTypes.Add(BsonInt64);
                    ms.WriteByte(BsonInt64);
                    ms.Write(nameBytes, 0, nameBytes.Length);
                    ms.WriteByte(0x00);
                    ms.Write(BitConverter.GetBytes(i64), 0, 8);
                }
                else
                {
                    double d = value.GetDouble();
                    seenTypes.Add(BsonDouble);
                    ms.WriteByte(BsonDouble);
                    ms.Write(nameBytes, 0, nameBytes.Length);
                    ms.WriteByte(0x00);
                    ms.Write(BitConverter.GetBytes(d), 0, 8);
                }
                break;
            }

            case JsonValueKind.True:
            case JsonValueKind.False:
                seenTypes.Add(BsonBoolean);
                ms.WriteByte(BsonBoolean);
                ms.Write(nameBytes, 0, nameBytes.Length);
                ms.WriteByte(0x00);
                ms.WriteByte(value.ValueKind == JsonValueKind.True ? (byte)0x01 : (byte)0x00);
                break;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                seenTypes.Add(BsonNull);
                ms.WriteByte(BsonNull);
                ms.Write(nameBytes, 0, nameBytes.Length);
                ms.WriteByte(0x00);
                break;
        }
    }

    private static void WriteCStringToStream(MemoryStream ms, string value)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
        ms.Write(bytes, 0, bytes.Length);
        ms.WriteByte(0x00);
    }
}
