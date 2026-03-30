using System.Text;
using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.DataFormats;

public static class HtmlToMarkdownCore
{
    public static string HtmlToMd(string html)
    {
        string result = html;

        // Normalize line endings
        result = result.Replace("\r\n", "\n").Replace("\r", "\n");

        // Process tables before other block elements
        result = ConvertTables(result);

        // Headings h1-h6
        for (int i = 6; i >= 1; i--)
        {
            string hashes = new('#', i);
            result = Regex.Replace(result, $@"<h{i}[^>]*>(.*?)</h{i}>", m =>
                $"\n{hashes} {CleanInline(m.Groups[1].Value).Trim()}\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        // Blockquotes
        result = Regex.Replace(result, @"<blockquote[^>]*>(.*?)</blockquote>", m =>
        {
            string inner = CleanInline(StripTag(m.Groups[1].Value, "p")).Trim();
            string[] lines = inner.Split('\n');
            return "\n" + string.Join("\n", lines.Select(l => "> " + l.Trim())) + "\n";
        }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Code blocks (pre > code)
        result = Regex.Replace(result, @"<pre[^>]*>\s*<code[^>]*>(.*?)</code>\s*</pre>", m =>
            $"\n```\n{DecodeHtmlEntities(m.Groups[1].Value.Trim())}\n```\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Standalone pre
        result = Regex.Replace(result, @"<pre[^>]*>(.*?)</pre>", m =>
            $"\n```\n{DecodeHtmlEntities(m.Groups[1].Value.Trim())}\n```\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Horizontal rules
        result = Regex.Replace(result, @"<hr\s*/?>", "\n---\n", RegexOptions.IgnoreCase);

        // Images (before links to avoid nested match issues)
        result = Regex.Replace(result, @"<img\s[^>]*src=[""']([^""']*)[""'][^>]*alt=[""']([^""']*)[""'][^>]*/?>", "![$2]($1)", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<img\s[^>]*alt=[""']([^""']*)[""'][^>]*src=[""']([^""']*)[""'][^>]*/?>", "![$1]($2)", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<img\s[^>]*src=[""']([^""']*)[""'][^>]*/?>", "![]($1)", RegexOptions.IgnoreCase);

        // Ordered lists
        result = Regex.Replace(result, @"<ol[^>]*>(.*?)</ol>", m =>
        {
            string inner = m.Groups[1].Value;
            MatchCollection items = Regex.Matches(inner, @"<li[^>]*>(.*?)</li>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            StringBuilder sb = new();
            sb.AppendLine();
            int num = 1;
            foreach (Match item in items)
            {
                sb.AppendLine($"{num}. {CleanInline(item.Groups[1].Value).Trim()}");
                num++;
            }
            return sb.ToString();
        }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Unordered lists
        result = Regex.Replace(result, @"<ul[^>]*>(.*?)</ul>", m =>
        {
            string inner = m.Groups[1].Value;
            MatchCollection items = Regex.Matches(inner, @"<li[^>]*>(.*?)</li>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            StringBuilder sb = new();
            sb.AppendLine();
            foreach (Match item in items)
            {
                sb.AppendLine($"- {CleanInline(item.Groups[1].Value).Trim()}");
            }
            return sb.ToString();
        }, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Paragraphs
        result = Regex.Replace(result, @"<p[^>]*>(.*?)</p>", m =>
            $"\n{CleanInline(m.Groups[1].Value).Trim()}\n", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Inline formatting
        result = CleanInline(result);

        // Remove remaining HTML tags
        result = Regex.Replace(result, @"<[^>]+>", "");

        // Decode HTML entities
        result = DecodeHtmlEntities(result);

        // Clean up excessive blank lines
        result = Regex.Replace(result, @"\n{3,}", "\n\n");

        return result.Trim();
    }

    private static string CleanInline(string html)
    {
        string result = html;

        // Bold: <strong>, <b>
        result = Regex.Replace(result, @"<(?:strong|b)[^>]*>(.*?)</(?:strong|b)>", "**$1**", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Italic: <em>, <i>
        result = Regex.Replace(result, @"<(?:em|i)[^>]*>(.*?)</(?:em|i)>", "*$1*", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Inline code: <code> (not inside pre)
        result = Regex.Replace(result, @"<code[^>]*>(.*?)</code>", "`$1`", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Links: <a href="...">text</a>
        result = Regex.Replace(result, @"<a\s[^>]*href=[""']([^""']*)[""'][^>]*>(.*?)</a>", "[$2]($1)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        // Line breaks
        result = Regex.Replace(result, @"<br\s*/?>", "  \n", RegexOptions.IgnoreCase);

        // Strikethrough
        result = Regex.Replace(result, @"<(?:del|s|strike)[^>]*>(.*?)</(?:del|s|strike)>", "~~$1~~", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return result;
    }

    private static string ConvertTables(string html)
    {
        return Regex.Replace(html, @"<table[^>]*>(.*?)</table>", m =>
        {
            string tableHtml = m.Groups[1].Value;
            List<List<string>> rows = [];

            MatchCollection rowMatches = Regex.Matches(tableHtml, @"<tr[^>]*>(.*?)</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            foreach (Match rowMatch in rowMatches)
            {
                List<string> cells = [];
                MatchCollection cellMatches = Regex.Matches(rowMatch.Groups[1].Value, @"<(?:td|th)[^>]*>(.*?)</(?:td|th)>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                foreach (Match cellMatch in cellMatches)
                {
                    cells.Add(CleanInline(cellMatch.Groups[1].Value).Trim());
                }
                if (cells.Count > 0)
                    rows.Add(cells);
            }

            if (rows.Count == 0) return m.Value;

            int maxCols = rows.Max(r => r.Count);
            foreach (List<string> row in rows)
            {
                while (row.Count < maxCols) row.Add("");
            }

            int[] widths = new int[maxCols];
            for (int col = 0; col < maxCols; col++)
            {
                widths[col] = rows.Max(r => r[col].Length);
                if (widths[col] < 3) widths[col] = 3;
            }

            StringBuilder sb = new();
            sb.AppendLine();

            // Header row
            sb.Append('|');
            for (int col = 0; col < maxCols; col++)
            {
                sb.Append($" {rows[0][col].PadRight(widths[col])} |");
            }
            sb.AppendLine();

            // Separator
            sb.Append('|');
            for (int col = 0; col < maxCols; col++)
            {
                sb.Append($" {new string('-', widths[col])} |");
            }
            sb.AppendLine();

            // Data rows
            for (int row = 1; row < rows.Count; row++)
            {
                sb.Append('|');
                for (int col = 0; col < maxCols; col++)
                {
                    sb.Append($" {rows[row][col].PadRight(widths[col])} |");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }, RegexOptions.Singleline | RegexOptions.IgnoreCase);
    }

    private static string StripTag(string html, string tag)
    {
        return Regex.Replace(html, $@"</?{tag}[^>]*>", "", RegexOptions.IgnoreCase);
    }

    private static string DecodeHtmlEntities(string text)
    {
        string result = text;
        result = result.Replace("&amp;", "&");
        result = result.Replace("&lt;", "<");
        result = result.Replace("&gt;", ">");
        result = result.Replace("&quot;", "\"");
        result = result.Replace("&#39;", "'");
        result = result.Replace("&apos;", "'");
        result = result.Replace("&nbsp;", " ");
        result = result.Replace("&ndash;", "\u2013");
        result = result.Replace("&mdash;", "\u2014");
        result = result.Replace("&lsquo;", "\u2018");
        result = result.Replace("&rsquo;", "\u2019");
        result = result.Replace("&ldquo;", "\u201c");
        result = result.Replace("&rdquo;", "\u201d");
        result = result.Replace("&hellip;", "\u2026");
        result = result.Replace("&copy;", "\u00a9");
        // Numeric entities
        result = Regex.Replace(result, @"&#(\d+);", m =>
        {
            if (int.TryParse(m.Groups[1].Value, out int code) && code is >= 0 and <= 0x10FFFF)
                return char.ConvertFromUtf32(code);
            return m.Value;
        });
        result = Regex.Replace(result, @"&#x([0-9a-fA-F]+);", m =>
        {
            if (int.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out int code) && code is >= 0 and <= 0x10FFFF)
                return char.ConvertFromUtf32(code);
            return m.Value;
        });
        return result;
    }
}
