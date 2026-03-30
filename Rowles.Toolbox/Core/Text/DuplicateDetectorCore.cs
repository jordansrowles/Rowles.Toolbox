namespace Rowles.Toolbox.Core.Text;

public static class DuplicateDetectorCore
{
    public enum DetectionMode { Lines, Sentences }

    public sealed record DuplicateEntry(string Text, int Count);

    public sealed record AnalysisResult(
        int TotalCount,
        int UniqueCount,
        List<DuplicateEntry> Duplicates,
        string DeduplicatedOutput);

    public static string[] SplitLines(string text)
    {
        return text.Split(["\r\n", "\n"], StringSplitOptions.None)
                   .Where(line => !string.IsNullOrWhiteSpace(line))
                   .Select(line => line.Trim())
                   .ToArray();
    }

    public static string[] SplitSentences(string text)
    {
        return text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => s.Trim())
                   .Where(s => s.Length > 0)
                   .ToArray();
    }

    public static AnalysisResult Analyze(string input, DetectionMode mode, bool caseSensitive, bool removeDuplicates)
    {
        string[] items = mode == DetectionMode.Lines
            ? SplitLines(input)
            : SplitSentences(input);

        StringComparer comparer = caseSensitive
            ? StringComparer.Ordinal
            : StringComparer.OrdinalIgnoreCase;

        Dictionary<string, int> counts = new(comparer);
        List<string> orderedKeys = [];

        foreach (string item in items)
        {
            if (counts.TryGetValue(item, out int existing))
            {
                counts[item] = existing + 1;
            }
            else
            {
                counts[item] = 1;
                orderedKeys.Add(item);
            }
        }

        List<DuplicateEntry> duplicates = [];
        foreach (string key in orderedKeys)
        {
            int count = counts[key];
            if (count > 1)
            {
                duplicates.Add(new DuplicateEntry(key, count));
            }
        }

        string deduplicatedOutput = string.Empty;
        if (removeDuplicates)
        {
            HashSet<string> seen = new(comparer);
            List<string> deduped = [];
            foreach (string item in items)
            {
                if (seen.Add(item)) deduped.Add(item);
            }
            string separator = mode == DetectionMode.Lines ? Environment.NewLine : ". ";
            deduplicatedOutput = string.Join(separator, deduped);
        }

        return new AnalysisResult(items.Length, counts.Count, duplicates, deduplicatedOutput);
    }
}
