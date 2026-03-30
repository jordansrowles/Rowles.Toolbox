namespace Rowles.Toolbox.Core.Generators;

public static class MarkdownTableGeneratorCore
{
    public static string[] ParseDelimitedLine(string line, char delimiter)
    {
        List<string> fields = [];
        bool inQuotes = false;
        System.Text.StringBuilder current = new();

        foreach (char ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (ch == delimiter && !inQuotes)
            {
                fields.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }
        fields.Add(current.ToString().Trim());
        return [.. fields];
    }

    public static string GenerateMarkdown(int cols, int rows, Func<int, int, string> getCell, Func<int, string> getAlign)
    {
        System.Text.StringBuilder sb = new();

        // Compute column widths
        int[] widths = new int[cols];
        for (int c = 0; c < cols; c++)
        {
            string header = getCell(0, c);
            widths[c] = Math.Max(3, string.IsNullOrEmpty(header) ? $"Header {c + 1}".Length : header.Length);
            for (int r = 1; r <= rows; r++)
            {
                widths[c] = Math.Max(widths[c], getCell(r, c).Length);
            }
        }

        // Header row
        sb.Append('|');
        for (int c = 0; c < cols; c++)
        {
            string header = getCell(0, c);
            if (string.IsNullOrEmpty(header)) header = $"Header {c + 1}";
            sb.Append($" {header.PadRight(widths[c])} |");
        }
        sb.AppendLine();

        // Separator row
        sb.Append('|');
        for (int c = 0; c < cols; c++)
        {
            string align = getAlign(c);
            string sep = align switch
            {
                "center" => $":{new string('-', widths[c])}:",
                "right" => $"{new string('-', widths[c])}:",
                _ => $"{new string('-', widths[c] + 1)}-"
            };
            sb.Append($" {sep} |");
        }
        sb.AppendLine();

        // Data rows
        for (int r = 1; r <= rows; r++)
        {
            sb.Append('|');
            for (int c = 0; c < cols; c++)
            {
                string cell = getCell(r, c);
                sb.Append($" {cell.PadRight(widths[c])} |");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
