namespace Rowles.Toolbox.Core.File;

public static class CsvViewerCore
{
    public const int MaxDisplayRows = 1000;

    public static List<string> ParseLine(string line, char delimiter)
    {
        List<string> fields = [];
        bool inQuotes = false;
        System.Text.StringBuilder current = new();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == delimiter)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    public static char DetectDelimiter(string content)
    {
        string[] lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        int linesToCheck = Math.Min(5, lines.Length);

        char[] candidates = [',', '\t', '|', ';'];
        int bestScore = 0;
        char bestDelimiter = ',';

        foreach (char delim in candidates)
        {
            int totalCount = 0;
            bool consistent = true;
            int firstCount = -1;

            for (int i = 0; i < linesToCheck; i++)
            {
                int count = CountUnquotedOccurrences(lines[i], delim);
                totalCount += count;

                if (i == 0)
                {
                    firstCount = count;
                }
                else if (count != firstCount)
                {
                    consistent = false;
                }
            }

            int score = totalCount * (consistent ? 10 : 1);
            if (score > bestScore)
            {
                bestScore = score;
                bestDelimiter = delim;
            }
        }

        return bestDelimiter;
    }

    public static int CountUnquotedOccurrences(string line, char target)
    {
        int count = 0;
        bool inQuotes = false;
        foreach (char c in line)
        {
            if (c == '"') inQuotes = !inQuotes;
            else if (c == target && !inQuotes) count++;
        }
        return count;
    }

    public static (List<string> Headers, List<List<string>> Rows, int TotalRowCount, char DetectedDelimiter, string? Error)
        ParseCsv(string content, string customDelimiter)
    {
        List<string> headers = [];
        List<List<string>> rows = [];

        char delimiter = !string.IsNullOrEmpty(customDelimiter)
            ? customDelimiter[0]
            : DetectDelimiter(content);

        string[] lines = content.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);

        int headerIndex = 0;
        while (headerIndex < lines.Length && string.IsNullOrWhiteSpace(lines[headerIndex]))
        {
            headerIndex++;
        }

        if (headerIndex >= lines.Length)
        {
            return (headers, rows, 0, delimiter, "File appears to be empty.");
        }

        headers = ParseLine(lines[headerIndex], delimiter);
        int totalRowCount = 0;

        for (int i = headerIndex + 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            totalRowCount++;
            if (rows.Count < MaxDisplayRows)
            {
                rows.Add(ParseLine(lines[i], delimiter));
            }
        }

        return (headers, rows, totalRowCount, delimiter, null);
    }

    public static List<List<string>> GetDisplayedRows(List<List<string>> rows, List<string> headers,
        string searchQuery, int sortColumnIndex, bool sortAscending)
    {
        IEnumerable<List<string>> result = rows;

        if (!string.IsNullOrEmpty(searchQuery))
        {
            result = result.Where(row => row.Any(cell =>
                cell.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)));
        }

        if (sortColumnIndex >= 0 && sortColumnIndex < headers.Count)
        {
            int col = sortColumnIndex;
            result = sortAscending
                ? result.OrderBy(r => col < r.Count ? r[col] : "", StringComparer.OrdinalIgnoreCase)
                : result.OrderByDescending(r => col < r.Count ? r[col] : "", StringComparer.OrdinalIgnoreCase);
        }

        return result.Take(MaxDisplayRows).ToList();
    }

    public static string DelimiterLabel(char detectedDelimiter) => detectedDelimiter switch
    {
        ',' => "Comma (,)",
        '\t' => "Tab (⇥)",
        '|' => "Pipe (|)",
        ';' => "Semicolon (;)",
        _ => $"'{detectedDelimiter}'"
    };
}
