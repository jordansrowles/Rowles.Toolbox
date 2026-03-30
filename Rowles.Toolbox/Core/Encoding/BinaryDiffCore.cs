namespace Rowles.Toolbox.Core.Encoding;

public static class BinaryDiffCore
{
    public sealed class DiffEntry
    {
        public int Offset { get; init; }
        public byte? ByteA { get; init; }
        public byte? ByteB { get; init; }
    }

    public sealed class DiffResult
    {
        public int SizeA { get; init; }
        public int SizeB { get; init; }
        public int SizeDifference { get; init; }
        public int DifferingByteCount { get; init; }
        public double MatchPercentage { get; init; }
        public int FirstDiffOffset { get; init; }
        public List<DiffEntry> Differences { get; init; } = new();
    }

    public static (DiffResult Result, HashSet<int> DiffOffsets) PerformDiff(byte[] bytesA, byte[] bytesB, int maxDisplayDiffs)
    {
        int sizeA = bytesA.Length;
        int sizeB = bytesB.Length;
        int maxLen = Math.Max(sizeA, sizeB);
        int differingCount = 0;
        int firstDiff = -1;
        List<DiffEntry> diffs = new();
        HashSet<int> diffOffsets = new();

        for (int i = 0; i < maxLen; i++)
        {
            byte? byteA = i < sizeA ? bytesA[i] : null;
            byte? byteB = i < sizeB ? bytesB[i] : null;
            bool isDifferent = byteA != byteB;

            if (isDifferent)
            {
                differingCount++;
                if (firstDiff < 0) firstDiff = i;
                diffOffsets.Add(i);

                if (diffs.Count < maxDisplayDiffs)
                {
                    diffs.Add(new DiffEntry { Offset = i, ByteA = byteA, ByteB = byteB });
                }
            }
        }

        double matchPct = maxLen > 0
            ? ((double)(maxLen - differingCount) / maxLen) * 100.0
            : 100.0;

        DiffResult result = new()
        {
            SizeA = sizeA,
            SizeB = sizeB,
            SizeDifference = sizeA - sizeB,
            DifferingByteCount = differingCount,
            MatchPercentage = matchPct,
            FirstDiffOffset = firstDiff,
            Differences = diffs
        };

        return (result, diffOffsets);
    }

    public static byte[]? ParseHexString(string hex, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(hex))
        {
            error = "Both hex strings must be provided.";
            return null;
        }

        try
        {
            string cleaned = hex.Replace(" ", "").Replace("-", "")
                .Replace("\n", "").Replace("\r", "").Replace("\t", "")
                .Replace("0x", "").Replace("0X", "").Replace(",", "");

            if (cleaned.Length % 2 != 0)
            {
                error = "Hex string must have an even number of characters.";
                return null;
            }

            byte[] result = new byte[cleaned.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToByte(cleaned.Substring(i * 2, 2), 16);
            }
            return result;
        }
        catch (FormatException)
        {
            error = "Invalid hex string. Use characters 0-9 and A-F only.";
            return null;
        }
    }

    public static string FormatByteCellHex(byte? b) =>
        b.HasValue ? b.Value.ToString("X2") : "--";

    public static string FormatByteAscii(byte? b)
    {
        if (!b.HasValue) return " ";
        return ToAsciiChar(b.Value);
    }

    public static string ToAsciiChar(byte b) =>
        b >= 32 && b <= 126 ? ((char)b).ToString() : ".";

    public static string FormatSize(long bytes) => bytes switch
    {
        < 1024L => $"{bytes} B",
        < 1_048_576L => $"{bytes / 1024.0:F1} KB",
        < 1_073_741_824L => $"{bytes / 1_048_576.0:F1} MB",
        _ => $"{bytes / 1_073_741_824.0:F2} GB"
    };

    public static string FormatSizeDifference(int diff)
    {
        if (diff == 0) return "0 B";
        string sign = diff > 0 ? "+" : "";
        return $"{sign}{FormatSize(Math.Abs(diff))}";
    }
}
