namespace Rowles.Toolbox.Core.Text;

public static class MergeSplitCore
{
    public static string GetDelimiterValue(string selectedDelimiter, string customDelimiter)
    {
        return selectedDelimiter switch
        {
            "Newline" => "\n",
            "Comma" => ",",
            "Tab" => "\t",
            "Space" => " ",
            "Custom" => customDelimiter,
            _ => "\n"
        };
    }

    public static string Merge(IEnumerable<string> inputs, string delimiter)
    {
        List<string> nonEmpty = inputs.Where(s => !string.IsNullOrEmpty(s)).ToList();
        if (nonEmpty.Count == 0) return string.Empty;
        return string.Join(delimiter, nonEmpty);
    }

    public static List<string> Split(string input, string delimiter)
    {
        if (string.IsNullOrEmpty(input)) return [];
        string[] parts = input.Split(delimiter);
        return parts.ToList();
    }
}
