namespace Rowles.Toolbox.Core.Text;

public static class TextCounterCore
{
    public static int CountCharacters(string text) => text.Length;

    public static int CountCharactersNoSpaces(string text) => text.Count(c => !char.IsWhiteSpace(c));

    public static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split((char[])[' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
    }

    public static int CountSentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        int count = 0;
        foreach (char c in text)
        {
            if (c is '.' or '!' or '?') count++;
        }
        return count == 0 ? 1 : count;
    }

    public static int CountParagraphs(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        string[] parts = text.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries);
        return parts.Count(p => !string.IsNullOrWhiteSpace(p));
    }

    public static int CountLines(string text)
    {
        if (text.Length == 0) return 0;
        return text.Split('\n').Length;
    }

    public static string CalculateReadingTime(int wordCount)
    {
        if (wordCount == 0) return "0 sec";
        double minutes = wordCount / 200.0;
        if (minutes < 1) return $"{(int)Math.Ceiling(minutes * 60)} sec";
        int mins = (int)Math.Floor(minutes);
        int secs = (int)Math.Round((minutes - mins) * 60);
        return secs > 0 ? $"{mins} min {secs} sec" : $"{mins} min";
    }

    public static int CountFilteredChar(string text, string filterChar)
    {
        if (string.IsNullOrEmpty(filterChar) || text.Length == 0) return 0;
        char target = filterChar[0];
        return text.Count(c => c == target);
    }
}
