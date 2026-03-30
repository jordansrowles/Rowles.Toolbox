namespace Rowles.Toolbox.Core.Text;

public static class ReverseTextCore
{
    public enum ReverseMode { EntireString, EachLine, WordsInLine }

    public static string Reverse(string input, ReverseMode mode)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        return mode switch
        {
            ReverseMode.EntireString => new string(input.Reverse().ToArray()),
            ReverseMode.EachLine => ReverseEachLine(input),
            ReverseMode.WordsInLine => ReverseWordsInEachLine(input),
            _ => input
        };
    }

    public static string ReverseEachLine(string input)
    {
        string[] lines = input.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = new string(lines[i].Reverse().ToArray());
        }
        return string.Join('\n', lines);
    }

    public static string ReverseWordsInEachLine(string input)
    {
        string[] lines = input.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(' ');
            Array.Reverse(words);
            lines[i] = string.Join(' ', words);
        }
        return string.Join('\n', lines);
    }

    public static string GetModeLabel(ReverseMode mode) => mode switch
    {
        ReverseMode.EntireString => "Reverse entire string",
        ReverseMode.EachLine => "Reverse each line",
        ReverseMode.WordsInLine => "Reverse words in each line",
        _ => mode.ToString()
    };
}
