using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.Text;

public static class FindReplaceCore
{
    public static string BuildPattern(string findText, bool useRegex, bool wholeWord)
    {
        string pattern = useRegex ? findText : Regex.Escape(findText);
        if (wholeWord)
        {
            pattern = $@"\b{pattern}\b";
        }
        return pattern;
    }

    public static int CountMatches(string source, string findText, bool caseSensitive, bool useRegex, bool wholeWord, out string? error)
    {
        error = null;
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(findText)) return 0;

        string pattern = BuildPattern(findText, useRegex, wholeWord);

        try
        {
            RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            Regex regex = new Regex(pattern, options);
            MatchCollection matches = regex.Matches(source);
            return matches.Count;
        }
        catch (RegexParseException ex)
        {
            error = $"Invalid regex: {ex.Message}";
            return 0;
        }
    }

    public static string ReplaceAll(string source, string findText, string replaceText, bool caseSensitive, bool useRegex, bool wholeWord, out string? error)
    {
        error = null;
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(findText)) return source;

        string pattern = BuildPattern(findText, useRegex, wholeWord);

        try
        {
            RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            Regex regex = new Regex(pattern, options);
            return regex.Replace(source, replaceText);
        }
        catch (RegexParseException ex)
        {
            error = $"Invalid regex: {ex.Message}";
            return string.Empty;
        }
    }
}
