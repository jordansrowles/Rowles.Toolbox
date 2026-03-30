using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.Developer;

public static class BenchmarkFormatterCore
{
    public enum OutputTab { Preview, Markdown, HTML }

    public sealed record ParseResult(
        List<string> Columns,
        List<Dictionary<string, string>> Rows,
        int FastestIndex,
        int SlowestIndex,
        double SpeedRatio,
        string? MeanColumnName,
        string? ErrorMessage);

    public static readonly string[] TimeColumns = ["Mean", "Median", "Error", "StdDev", "Min", "Max"];

    public static ParseResult Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new ParseResult([], [], -1, -1, 0, null, null);
        }

        string[] lines = input.Split(["\r\n", "\n"], StringSplitOptions.None);

        int headerLineIndex = -1;
        int separatorLineIndex = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (trimmed.StartsWith('|') && trimmed.EndsWith('|'))
            {
                if (IsSeparatorLine(trimmed))
                {
                    separatorLineIndex = i;
                    if (i > 0)
                    {
                        headerLineIndex = i - 1;
                    }
                    break;
                }
            }
        }

        if (headerLineIndex < 0 || separatorLineIndex < 0)
        {
            return new ParseResult([], [], -1, -1, 0, null,
                "Could not find a valid BenchmarkDotNet table. Expected a header row followed by a separator line (e.g. |---|---|).");
        }

        List<string> columns = ParseRow(lines[headerLineIndex]);
        if (columns.Count < 2)
        {
            return new ParseResult([], [], -1, -1, 0, null,
                "Header row must contain at least 2 columns.");
        }

        List<Dictionary<string, string>> rows = [];
        for (int i = separatorLineIndex + 1; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || !trimmed.StartsWith('|') || !trimmed.EndsWith('|'))
            {
                continue;
            }

            if (IsSeparatorLine(trimmed))
            {
                continue;
            }

            List<string> values = ParseRow(trimmed);
            Dictionary<string, string> rowDict = new(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < columns.Count && c < values.Count; c++)
            {
                rowDict[columns[c]] = values[c];
            }

            if (rowDict.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                rows.Add(rowDict);
            }
        }

        if (rows.Count == 0)
        {
            return new ParseResult(columns, [], -1, -1, 0, null,
                "No data rows found after the separator line.");
        }

        string? meanColumnName = FindMeanColumn(columns);
        (int fastestIndex, int slowestIndex, double speedRatio) = CalculateFastestSlowest(rows, meanColumnName);

        return new ParseResult(columns, rows, fastestIndex, slowestIndex, speedRatio, meanColumnName, null);
    }

    public static bool IsSeparatorLine(string line)
    {
        string inner = line.Trim().Trim('|');
        return Regex.IsMatch(inner, @"^[\s\-:|]+$") && inner.Contains('-');
    }

    public static List<string> ParseRow(string line)
    {
        string trimmed = line.Trim();
        if (trimmed.StartsWith('|'))
        {
            trimmed = trimmed[1..];
        }
        if (trimmed.EndsWith('|'))
        {
            trimmed = trimmed[..^1];
        }

        string[] cells = trimmed.Split('|');
        List<string> result = [];
        foreach (string cell in cells)
        {
            result.Add(cell.Trim());
        }
        return result;
    }

    public static string? FindMeanColumn(List<string> columns)
    {
        foreach (string candidate in TimeColumns)
        {
            if (columns.Contains(candidate, StringComparer.OrdinalIgnoreCase))
            {
                return columns.First(c => string.Equals(c, candidate, StringComparison.OrdinalIgnoreCase));
            }
        }
        return null;
    }

    private static (int FastestIndex, int SlowestIndex, double SpeedRatio) CalculateFastestSlowest(
        List<Dictionary<string, string>> rows,
        string? meanColumnName)
    {
        if (meanColumnName is null || rows.Count == 0)
        {
            return (-1, -1, 0);
        }

        double minNanos = double.MaxValue;
        double maxNanos = double.MinValue;
        int fastestIndex = -1;
        int slowestIndex = -1;

        for (int i = 0; i < rows.Count; i++)
        {
            double nanos = ParseToNanoseconds(GetCellValue(rows[i], meanColumnName));
            if (nanos < 0)
            {
                continue;
            }

            if (nanos < minNanos)
            {
                minNanos = nanos;
                fastestIndex = i;
            }
            if (nanos > maxNanos)
            {
                maxNanos = nanos;
                slowestIndex = i;
            }
        }

        double speedRatio = 0;
        if (minNanos > 0 && maxNanos > 0 && fastestIndex != slowestIndex)
        {
            speedRatio = maxNanos / minNanos;
        }

        return (fastestIndex, slowestIndex, speedRatio);
    }

    public static double ParseToNanoseconds(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "-" || value == "NA")
        {
            return -1;
        }

        string cleaned = value.Trim().Replace(",", "");
        Match match = Regex.Match(cleaned, @"^([\d.]+)\s*(ns|us|μs|ms|s)$", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            if (double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out double raw))
            {
                return raw;
            }
            return -1;
        }

        double number = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        string unit = match.Groups[2].Value.ToLowerInvariant();

        return unit switch
        {
            "ns" => number,
            "us" or "μs" => number * 1_000,
            "ms" => number * 1_000_000,
            "s" => number * 1_000_000_000,
            _ => number
        };
    }

    public static double ParseToBytes(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "-" || value == "NA")
        {
            return -1;
        }

        string cleaned = value.Trim().Replace(",", "");
        Match match = Regex.Match(cleaned, @"^([\d.]+)\s*(B|KB|MB|GB)$", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            if (double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out double raw))
            {
                return raw;
            }
            return -1;
        }

        double number = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        string unit = match.Groups[2].Value.ToUpperInvariant();

        return unit switch
        {
            "B" => number,
            "KB" => number * 1_024,
            "MB" => number * 1_024 * 1_024,
            "GB" => number * 1_024 * 1_024 * 1_024,
            _ => number
        };
    }

    public static string GetCellValue(Dictionary<string, string> row, string column)
    {
        return row.TryGetValue(column, out string? val) ? val : string.Empty;
    }

    public static string GetRelativePerformance(
        Dictionary<string, string> row,
        string? meanColumnName,
        int fastestIndex,
        List<Dictionary<string, string>> allRows)
    {
        if (meanColumnName is null || fastestIndex < 0)
        {
            return "-";
        }

        double current = ParseToNanoseconds(GetCellValue(row, meanColumnName));
        double fastest = ParseToNanoseconds(GetCellValue(allRows[fastestIndex], meanColumnName));

        if (current < 0 || fastest <= 0)
        {
            return "-";
        }

        if (Math.Abs(current - fastest) < 0.001)
        {
            return "baseline";
        }

        double ratio = current / fastest;
        return $"{ratio:F2}x slower";
    }

    public static List<Dictionary<string, string>> SortRows(
        List<Dictionary<string, string>> rows,
        List<string> columns,
        int columnIndex,
        bool ascending)
    {
        if (columnIndex < 0 || columnIndex >= columns.Count)
        {
            return [.. rows];
        }

        string columnName = columns[columnIndex];
        bool isTimeColumn = TimeColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase);
        bool isAllocColumn = string.Equals(columnName, "Allocated", StringComparison.OrdinalIgnoreCase);

        List<Dictionary<string, string>> sorted = [.. rows.OrderBy(row =>
        {
            string cellValue = GetCellValue(row, columnName);

            if (isTimeColumn)
            {
                double nanos = ParseToNanoseconds(cellValue);
                return nanos < 0 ? double.MaxValue : nanos;
            }

            if (isAllocColumn)
            {
                double bytes = ParseToBytes(cellValue);
                return bytes < 0 ? double.MaxValue : bytes;
            }

            if (double.TryParse(cellValue.Replace(",", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out double numericValue))
            {
                return numericValue;
            }

            return double.MaxValue;
        })];

        if (!ascending)
        {
            sorted.Reverse();
        }

        return sorted;
    }

    public static string GenerateMarkdown(
        List<string> columns,
        List<Dictionary<string, string>> sortedRows,
        string? meanColumnName,
        int fastestIndex,
        List<Dictionary<string, string>> allRows)
    {
        StringBuilder sb = new();
        List<string> headers = [.. columns];
        if (meanColumnName is not null)
        {
            headers.Add("Relative");
        }

        List<int> widths = [];
        foreach (string header in headers)
        {
            widths.Add(header.Length);
        }

        foreach (Dictionary<string, string> row in sortedRows)
        {
            for (int c = 0; c < columns.Count; c++)
            {
                int len = GetCellValue(row, columns[c]).Length;
                if (len > widths[c])
                {
                    widths[c] = len;
                }
            }
            if (meanColumnName is not null)
            {
                int relLen = GetRelativePerformance(row, meanColumnName, fastestIndex, allRows).Length;
                if (relLen > widths[^1])
                {
                    widths[^1] = relLen;
                }
            }
        }

        sb.Append('|');
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append($" {headers[i].PadRight(widths[i])} |");
        }
        sb.AppendLine();

        sb.Append('|');
        for (int i = 0; i < headers.Count; i++)
        {
            sb.Append($" {new string('-', widths[i])} |");
        }
        sb.AppendLine();

        foreach (Dictionary<string, string> row in sortedRows)
        {
            sb.Append('|');
            for (int c = 0; c < columns.Count; c++)
            {
                sb.Append($" {GetCellValue(row, columns[c]).PadRight(widths[c])} |");
            }
            if (meanColumnName is not null)
            {
                sb.Append($" {GetRelativePerformance(row, meanColumnName, fastestIndex, allRows).PadRight(widths[^1])} |");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string GenerateHtml(
        List<string> columns,
        List<Dictionary<string, string>> sortedRows,
        string? meanColumnName,
        int fastestIndex,
        int slowestIndex,
        List<Dictionary<string, string>> allRows)
    {
        StringBuilder sb = new();
        sb.AppendLine("<table>");
        sb.AppendLine("  <thead>");
        sb.AppendLine("    <tr>");
        foreach (string col in columns)
        {
            sb.AppendLine($"      <th>{System.Net.WebUtility.HtmlEncode(col)}</th>");
        }
        if (meanColumnName is not null)
        {
            sb.AppendLine("      <th>Relative</th>");
        }
        sb.AppendLine("    </tr>");
        sb.AppendLine("  </thead>");
        sb.AppendLine("  <tbody>");

        for (int ri = 0; ri < sortedRows.Count; ri++)
        {
            Dictionary<string, string> row = sortedRows[ri];
            int originalIndex = allRows.IndexOf(row);
            string style = originalIndex == fastestIndex
                ? " style=\"background-color: #dcfce7;\""
                : originalIndex == slowestIndex && allRows.Count > 1
                    ? " style=\"background-color: #fee2e2;\""
                    : "";

            sb.AppendLine($"    <tr{style}>");
            foreach (string col in columns)
            {
                sb.AppendLine($"      <td>{System.Net.WebUtility.HtmlEncode(GetCellValue(row, col))}</td>");
            }
            if (meanColumnName is not null)
            {
                sb.AppendLine($"      <td>{System.Net.WebUtility.HtmlEncode(GetRelativePerformance(row, meanColumnName, fastestIndex, allRows))}</td>");
            }
            sb.AppendLine("    </tr>");
        }

        sb.AppendLine("  </tbody>");
        sb.AppendLine("</table>");

        return sb.ToString();
    }
}
