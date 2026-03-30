namespace Rowles.Toolbox.Core.File;

public static class FileEncodingDetectorCore
{
    public static (string Encoding, bool HasBom, string BomHex, int BomLength, string Confidence) DetectEncoding(byte[] fileData)
    {
        if (fileData.Length == 0)
        {
            return ("Empty file", false, string.Empty, 0, "N/A");
        }

        // Check for BOMs (order matters: UTF-32 before UTF-16 since UTF-32 LE starts with FF FE too)
        if (fileData.Length >= 4 && fileData[0] == 0xFF && fileData[1] == 0xFE && fileData[2] == 0x00 && fileData[3] == 0x00)
        {
            return CreateBomResult("UTF-32 LE", 4, fileData);
        }
        if (fileData.Length >= 4 && fileData[0] == 0x00 && fileData[1] == 0x00 && fileData[2] == 0xFE && fileData[3] == 0xFF)
        {
            return CreateBomResult("UTF-32 BE", 4, fileData);
        }
        if (fileData.Length >= 3 && fileData[0] == 0xEF && fileData[1] == 0xBB && fileData[2] == 0xBF)
        {
            return CreateBomResult("UTF-8", 3, fileData);
        }
        if (fileData.Length >= 2 && fileData[0] == 0xFF && fileData[1] == 0xFE)
        {
            return CreateBomResult("UTF-16 LE", 2, fileData);
        }
        if (fileData.Length >= 2 && fileData[0] == 0xFE && fileData[1] == 0xFF)
        {
            return CreateBomResult("UTF-16 BE", 2, fileData);
        }

        // No BOM — heuristic detection
        if (IsValidUtf8(fileData))
        {
            bool hasMultibyte = HasMultibyteUtf8(fileData);
            string encoding = "UTF-8 (no BOM)";
            string confidence = hasMultibyte ? "High" : "Medium";
            return (encoding, false, string.Empty, 0, confidence);
        }
        else
        {
            bool hasHighBytes = fileData.Any(b => b > 127);
            string encoding = hasHighBytes ? "Windows-1252 / ISO-8859-1" : "ASCII";
            string confidence = hasHighBytes ? "Medium" : "High";
            return (encoding, false, string.Empty, 0, confidence);
        }
    }

    private static (string Encoding, bool HasBom, string BomHex, int BomLength, string Confidence) CreateBomResult(
        string encoding, int bomLen, byte[] fileData)
    {
        string bomHex = string.Join(" ", fileData.Take(bomLen).Select(b => b.ToString("X2")));
        return (encoding, true, bomHex, bomLen, "High");
    }

    public static bool IsValidUtf8(byte[] data)
    {
        int i = 0;
        while (i < data.Length)
        {
            byte b = data[i];

            int seqLen;
            if (b <= 0x7F) { seqLen = 1; }
            else if ((b & 0xE0) == 0xC0) { seqLen = 2; }
            else if ((b & 0xF0) == 0xE0) { seqLen = 3; }
            else if ((b & 0xF8) == 0xF0) { seqLen = 4; }
            else { return false; }

            if (i + seqLen > data.Length) return false;

            for (int j = 1; j < seqLen; j++)
            {
                if ((data[i + j] & 0xC0) != 0x80) return false;
            }

            i += seqLen;
        }
        return true;
    }

    public static bool HasMultibyteUtf8(byte[] data)
    {
        return data.Any(b => b > 0x7F);
    }

    public static (int CrlfCount, int LfCount, int CrCount, string LineEndings) CountLineEndings(byte[] fileData)
    {
        int crlfCount = 0;
        int lfCount = 0;
        int crCount = 0;

        for (int i = 0; i < fileData.Length; i++)
        {
            if (fileData[i] == 0x0D)
            {
                if (i + 1 < fileData.Length && fileData[i + 1] == 0x0A)
                {
                    crlfCount++;
                    i++;
                }
                else
                {
                    crCount++;
                }
            }
            else if (fileData[i] == 0x0A)
            {
                lfCount++;
            }
        }

        int total = crlfCount + lfCount + crCount;
        string lineEndings;
        if (total == 0)
        {
            lineEndings = "None (single line)";
        }
        else if (crlfCount > 0 && lfCount == 0 && crCount == 0)
        {
            lineEndings = "CRLF (Windows)";
        }
        else if (lfCount > 0 && crlfCount == 0 && crCount == 0)
        {
            lineEndings = "LF (Unix/macOS)";
        }
        else if (crCount > 0 && crlfCount == 0 && lfCount == 0)
        {
            lineEndings = "CR (Classic Mac)";
        }
        else
        {
            lineEndings = "Mixed";
        }

        return (crlfCount, lfCount, crCount, lineEndings);
    }

    public static byte[] InspectRange(byte[] fileData, int rangeStart, int rangeEnd)
    {
        int start = Math.Clamp(rangeStart, 0, fileData.Length - 1);
        int end = Math.Clamp(rangeEnd, start, fileData.Length - 1);
        return fileData[start..(end + 1)];
    }

    public static string ConfidenceColor(string confidence) => confidence switch
    {
        "High" => "text-green-600 dark:text-green-400",
        "Medium" => "text-amber-600 dark:text-amber-400",
        "Low" => "text-red-600 dark:text-red-400",
        _ => "text-gray-500"
    };

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }

    public static string BytesToAscii(byte[] data)
    {
        char[] chars = new char[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            char c = (char)data[i];
            chars[i] = c >= 32 && c < 127 ? c : '.';
        }
        return new string(chars);
    }
}
