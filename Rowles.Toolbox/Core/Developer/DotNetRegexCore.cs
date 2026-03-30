using System.Text;
using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.Developer;

public static class DotNetRegexCore
{
    public static readonly (string Name, string Pattern, string Description)[] CommonPatterns =
    [
        ("Email", @"^[\w\.-]+@[\w\.-]+\.\w{2,}$", "Basic email address validation"),
        ("URL", @"https?://[\w\-]+(\.[\w\-]+)+([\w\-.,@?^=%&:/~+#]*[\w\-@?^=%&/~+#])?", "HTTP/HTTPS URL"),
        ("IPv4 Address", @"\b(?:(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(?:25[0-5]|2[0-4]\d|[01]?\d\d?)\b", "Valid IPv4 address (0.0.0.0 – 255.255.255.255)"),
        ("IPv6 Address", @"([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}", "Simple IPv6 address (full form)"),
        ("Phone (UK)", @"(?:\+44|0)\s?\d{4}\s?\d{6}", "UK phone number (+44 or 0 prefix)"),
        ("Phone (US)", @"\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}", "US phone number with optional formatting"),
        ("Date (yyyy-MM-dd)", @"\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])", "ISO 8601 date format"),
        ("Date (dd/MM/yyyy)", @"(0[1-9]|[12]\d|3[01])/(0[1-9]|1[0-2])/\d{4}", "UK date format"),
        ("Time (HH:mm:ss)", @"([01]\d|2[0-3]):[0-5]\d(:[0-5]\d)?", "24-hour time with optional seconds"),
        ("Hex Colour", @"#([0-9a-fA-F]{3}){1,2}\b", "CSS hex colour (#RGB or #RRGGBB)"),
        ("GUID", @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}", "Standard GUID/UUID format"),
        ("Postcode (UK)", @"[A-Z]{1,2}\d[A-Z\d]?\s?\d[A-Z]{2}", "UK postcode (e.g. SW1A 1AA)"),
        ("ZIP Code (US)", @"\d{5}(-\d{4})?", "US ZIP code with optional +4"),
        ("HTML Tag", @"<([a-zA-Z][\w]*)\b[^>]*>(.*?)</\1>", "Matching HTML open/close tag pair"),
        ("Whitespace Trim", @"^\s+|\s+$", "Leading and trailing whitespace"),
        ("Integer", @"-?\d+", "Signed integer"),
        ("Decimal Number", @"-?\d+\.\d+", "Signed decimal number"),
        ("C# Identifier", @"@?[_\p{L}][\p{L}\p{Nd}_]*", "Valid C# identifier (including verbatim @)"),
    ];

    public static readonly (string Syntax, string Meaning)[] SyntaxReference =
    [
        (@".", "Any character (except \\n unless Singleline)"),
        (@"\d", "Digit [0-9]"),
        (@"\D", "Non-digit"),
        (@"\w", "Word character [a-zA-Z0-9_]"),
        (@"\W", "Non-word character"),
        (@"\s", "Whitespace"),
        (@"\S", "Non-whitespace"),
        (@"\b", "Word boundary"),
        (@"^", "Start of string (or line if Multiline)"),
        (@"$", "End of string (or line if Multiline)"),
        (@"*", "Zero or more (greedy)"),
        (@"+", "One or more (greedy)"),
        (@"?", "Zero or one (greedy)"),
        (@"*?", "Zero or more (lazy)"),
        (@"+?", "One or more (lazy)"),
        (@"{n}", "Exactly n times"),
        (@"{n,m}", "Between n and m times"),
        (@"(…)", "Capturing group"),
        (@"(?<name>…)", "Named capturing group"),
        (@"(?:…)", "Non-capturing group"),
        (@"(?=…)", "Positive lookahead"),
        (@"(?!…)", "Negative lookahead"),
        (@"(?<=…)", "Positive lookbehind"),
        (@"(?<!…)", "Negative lookbehind"),
        (@"[abc]", "Character class"),
        (@"[^abc]", "Negated character class"),
        (@"|", "Alternation (OR)"),
        (@"\1", "Backreference to group 1"),
        (@"\k<name>", "Backreference to named group"),
    ];

    public static RegexOptions BuildOptions(
        bool ignoreCase,
        bool multiline,
        bool singleline,
        bool explicitCapture,
        bool ignorePatternWhitespace,
        bool compiled,
        bool rightToLeft,
        bool nonBacktracking)
    {
        RegexOptions options = RegexOptions.None;
        if (ignoreCase) options |= RegexOptions.IgnoreCase;
        if (multiline) options |= RegexOptions.Multiline;
        if (singleline) options |= RegexOptions.Singleline;
        if (explicitCapture) options |= RegexOptions.ExplicitCapture;
        if (ignorePatternWhitespace) options |= RegexOptions.IgnorePatternWhitespace;
        if (compiled) options |= RegexOptions.Compiled;
        if (rightToLeft) options |= RegexOptions.RightToLeft;
        if (nonBacktracking) options |= RegexOptions.NonBacktracking;
        return options;
    }

    public static string HighlightMatches(string testString, MatchCollection matches)
    {
        if (matches.Count == 0)
        {
            return System.Net.WebUtility.HtmlEncode(testString);
        }

        StringBuilder sb = new();
        int lastIndex = 0;

        foreach (Match match in matches)
        {
            if (match.Index > lastIndex)
            {
                sb.Append(System.Net.WebUtility.HtmlEncode(testString[lastIndex..match.Index]));
            }

            sb.Append("<mark class=\"bg-yellow-300 dark:bg-yellow-600 text-gray-900 dark:text-white rounded px-0.5\">");
            sb.Append(System.Net.WebUtility.HtmlEncode(match.Value));
            sb.Append("</mark>");

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < testString.Length)
        {
            sb.Append(System.Net.WebUtility.HtmlEncode(testString[lastIndex..]));
        }

        return sb.ToString();
    }
}
