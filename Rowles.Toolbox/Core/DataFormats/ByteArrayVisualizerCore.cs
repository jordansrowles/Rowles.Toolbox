using System.Globalization;

namespace Rowles.Toolbox.Core.DataFormats;

public static class ByteArrayVisualizerCore
{
    public sealed class ByteStats
    {
        public int NullCount { get; set; }
        public int PrintableCount { get; set; }
        public int NonPrintableCount { get; set; }
        public int HighByteCount { get; set; }
    }

    public sealed class ParseResult
    {
        public byte[]? Bytes { get; set; }
        public string? DetectedFormat { get; set; }
        public string? Error { get; set; }
    }

    public static ParseResult ParseInput(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return new ParseResult();

        string input = rawInput.Trim();

        if (TryParseCSharpByteArray(input, out byte[]? result))
            return new ParseResult { Bytes = result, DetectedFormat = "C# byte[]" };

        if (TryParseHexString(input, out result))
            return new ParseResult { Bytes = result, DetectedFormat = "Hex string" };

        if (TryParseDecimalCsv(input, out result))
            return new ParseResult { Bytes = result, DetectedFormat = "Decimal CSV" };

        if (TryParseBase64(input, out result))
            return new ParseResult { Bytes = result, DetectedFormat = "Base64" };

        return new ParseResult
        {
            Error = "Could not detect input format. Supported: C# byte[] literal, hex string, comma-separated decimals, or Base64."
        };
    }

    public static ByteStats ComputeStats(byte[] bytes)
    {
        ByteStats stats = new();
        foreach (byte b in bytes)
        {
            if (b == 0) stats.NullCount++;
            if (b >= 0x20 && b <= 0x7E) stats.PrintableCount++;
            else stats.NonPrintableCount++;
            if (b > 0x7F) stats.HighByteCount++;
        }
        return stats;
    }

    public static string BuildHexDump(byte[] bytes)
    {
        var sb = new System.Text.StringBuilder();
        for (int row = 0; row < (bytes.Length + 15) / 16; row++)
        {
            int offset = row * 16;
            int count = Math.Min(16, bytes.Length - offset);

            sb.Append(offset.ToString("X8"));
            sb.Append("  ");

            for (int i = 0; i < 16; i++)
            {
                if (i == 8) sb.Append(' ');
                if (i < count)
                {
                    sb.Append(bytes[offset + i].ToString("X2"));
                    sb.Append(' ');
                }
                else
                {
                    sb.Append("   ");
                }
            }

            sb.Append(" |");
            for (int i = 0; i < count; i++)
            {
                byte b = bytes[offset + i];
                sb.Append(b >= 32 && b <= 126 ? (char)b : '.');
            }
            sb.Append('|');
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string BuildCSharpLiteral(byte[] bytes)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("byte[] arr = new byte[] { ");

        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            if (i > 0 && i % 16 == 0) sb.AppendLine().Append("    ");
            sb.Append("0x");
            sb.Append(bytes[i].ToString("X2"));
        }

        sb.Append(" };");
        return sb.ToString();
    }

    public static string BuildDecimalCsv(byte[] bytes)
    {
        return string.Join(", ", bytes.Select(b => b.ToString()));
    }

    public static string BuildBinaryRepresentation(byte[] bytes)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(' ');
                if (i % 8 == 0) sb.AppendLine();
            }
            sb.Append(Convert.ToString(bytes[i], 2).PadLeft(8, '0'));
        }

        return sb.ToString();
    }

    public static string BuildUtf8String(byte[] bytes)
    {
        try
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return "(invalid UTF-8 sequence)";
        }
    }

    public static string GetByteColor(byte b)
    {
        if (b >= 0x20 && b <= 0x7E)
            return "text-green-700 dark:text-green-400";
        if (b <= 0x1F || b == 0x7F)
            return "text-red-600 dark:text-red-400";
        return "text-blue-600 dark:text-blue-400";
    }

    // -- Format Parsers --

    private static bool TryParseCSharpByteArray(string input, out byte[]? result)
    {
        result = null;

        string pattern = input;

        int braceStart = pattern.IndexOf('{');
        int braceEnd = pattern.LastIndexOf('}');
        int bracketStart = pattern.IndexOf('[');
        int bracketEnd = pattern.LastIndexOf(']');

        if (braceStart >= 0 && braceEnd > braceStart)
        {
            pattern = pattern.Substring(braceStart + 1, braceEnd - braceStart - 1);
        }
        else if (bracketStart >= 0 && bracketEnd > bracketStart
                 && pattern.Contains("0x", StringComparison.OrdinalIgnoreCase))
        {
            pattern = pattern.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
        }
        else if (!pattern.Contains("0x", StringComparison.OrdinalIgnoreCase)
                 && !pattern.Contains("0X", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!pattern.Contains("0x", StringComparison.OrdinalIgnoreCase))
            return false;

        string[] parts = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return false;

        var bytes = new List<byte>(parts.Length);
        foreach (string part in parts)
        {
            string val = part.TrimEnd(';');
            if (val.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || val.StartsWith("0X"))
            {
                if (byte.TryParse(val.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                    bytes.Add(b);
                else
                    return false;
            }
            else if (byte.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte b))
            {
                bytes.Add(b);
            }
            else
            {
                return false;
            }
        }

        result = bytes.ToArray();
        return result.Length > 0;
    }

    private static bool TryParseHexString(string input, out byte[]? result)
    {
        result = null;

        string hex = input
            .Replace("0x", "", StringComparison.OrdinalIgnoreCase)
            .Replace("0X", "")
            .Replace("-", "")
            .Replace(":", "")
            .Replace(" ", "")
            .Replace("\t", "")
            .Replace("\n", "")
            .Replace("\r", "");

        if (hex.Length == 0 || hex.Length % 2 != 0)
            return false;

        if (!IsHexString(hex))
            return false;

        bool hasHexLetter = false;
        foreach (char c in hex)
        {
            if ((c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
            {
                hasHexLetter = true;
                break;
            }
        }

        if (!hasHexLetter && hex.Length < 4)
            return false;

        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            if (!byte.TryParse(hex.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i]))
                return false;
        }

        result = bytes;
        return true;
    }

    private static bool IsHexString(string s)
    {
        foreach (char c in s)
        {
            if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                return false;
        }
        return true;
    }

    private static bool TryParseDecimalCsv(string input, out byte[]? result)
    {
        result = null;

        if (!input.Contains(','))
            return false;

        string[] parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return false;

        var bytes = new List<byte>(parts.Length);
        foreach (string part in parts)
        {
            string val = part.Trim(' ', '\t', '\n', '\r', '[', ']', '{', '}');
            if (string.IsNullOrEmpty(val))
                continue;
            if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) && n >= 0 && n <= 255)
                bytes.Add((byte)n);
            else
                return false;
        }

        result = bytes.ToArray();
        return result.Length > 0;
    }

    private static bool TryParseBase64(string input, out byte[]? result)
    {
        result = null;

        string trimmed = input.Trim();

        if (trimmed.Length < 4)
            return false;

        try
        {
            result = Convert.FromBase64String(trimmed);
            return result.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}
