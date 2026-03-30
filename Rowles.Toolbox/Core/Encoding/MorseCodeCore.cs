namespace Rowles.Toolbox.Core.Encoding;

public static class MorseCodeCore
{
    public sealed record MorseEntry(string Character, string Code);

    private static readonly Dictionary<char, string> s_morseMap = new()
    {
        ['A'] = ".-",     ['B'] = "-...",   ['C'] = "-.-.",   ['D'] = "-..",
        ['E'] = ".",      ['F'] = "..-.",   ['G'] = "--.",    ['H'] = "....",
        ['I'] = "..",     ['J'] = ".---",   ['K'] = "-.-",    ['L'] = ".-..",
        ['M'] = "--",     ['N'] = "-.",     ['O'] = "---",    ['P'] = ".--.",
        ['Q'] = "--.-",   ['R'] = ".-.",    ['S'] = "...",    ['T'] = "-",
        ['U'] = "..-",    ['V'] = "...-",   ['W'] = ".--",    ['X'] = "-..-",
        ['Y'] = "-.--",   ['Z'] = "--..",
        ['0'] = "-----",  ['1'] = ".----",  ['2'] = "..---",  ['3'] = "...--",
        ['4'] = "....-",  ['5'] = ".....",  ['6'] = "-....",  ['7'] = "--...",
        ['8'] = "---..",   ['9'] = "----.",
        ['.'] = ".-.-.-", [','] = "--..--", ['?'] = "..--..", ['\''] = ".----.",
        ['!'] = "-.-.--", ['/'] = "-..-.",  ['('] = "-.--.",  [')'] = "-.--.-",
        ['&'] = ".-...",  [':'] = "---...", [';'] = "-.-.-.", ['='] = "-...-",
        ['+'] = ".-.-.",  ['-'] = "-....-", ['_'] = "..--.-", ['"'] = ".-..-.",
        ['@'] = ".--.-."
    };

    private static readonly Dictionary<string, char> s_reverseMorseMap = BuildReverseMap();

    private static Dictionary<string, char> BuildReverseMap()
    {
        Dictionary<string, char> reverse = new();
        foreach (KeyValuePair<char, string> kv in s_morseMap)
        {
            reverse[kv.Value] = kv.Key;
        }
        return reverse;
    }

    public static (string MorseText, string? Error) EncodeText(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return (string.Empty, null);

        string? error = null;
        string upper = plainText.ToUpperInvariant();
        List<string> morseWords = new();
        string[] words = upper.Split(' ');

        foreach (string word in words)
        {
            List<string> morseLetters = new();
            foreach (char ch in word)
            {
                if (s_morseMap.TryGetValue(ch, out string? morse))
                {
                    morseLetters.Add(morse);
                }
                else if (!char.IsWhiteSpace(ch))
                {
                    error = $"Unsupported character skipped: '{ch}'";
                }
            }
            if (morseLetters.Count > 0)
            {
                morseWords.Add(string.Join(" ", morseLetters));
            }
        }

        return (string.Join(" / ", morseWords), error);
    }

    public static (string PlainText, string? Error) DecodeMorse(string morseText)
    {
        if (string.IsNullOrWhiteSpace(morseText))
            return (string.Empty, null);

        string? error = null;
        string[] words = morseText.Split(" / ");
        List<string> decodedWords = new();

        foreach (string word in words)
        {
            string[] letters = word.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            System.Text.StringBuilder sb = new();

            foreach (string letter in letters)
            {
                if (s_reverseMorseMap.TryGetValue(letter, out char ch))
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append('?');
                    error = $"Unknown Morse sequence: \"{letter}\"";
                }
            }

            decodedWords.Add(sb.ToString());
        }

        return (string.Join(" ", decodedWords), error);
    }

    public static MorseEntry[] GetLetterEntries() =>
    [
        new("A", ".-"),    new("B", "-..."),  new("C", "-.-."),  new("D", "-.."),
        new("E", "."),     new("F", "..-."),  new("G", "--."),   new("H", "...."),
        new("I", ".."),    new("J", ".---"),  new("K", "-.-"),   new("L", ".-.."),
        new("M", "--"),    new("N", "-."),    new("O", "---"),   new("P", ".--."),
        new("Q", "--.-"),  new("R", ".-."),   new("S", "..."),   new("T", "-"),
        new("U", "..-"),   new("V", "...-"),  new("W", ".--"),   new("X", "-..-"),
        new("Y", "-.--"),  new("Z", "--..")
    ];

    public static MorseEntry[] GetDigitEntries() =>
    [
        new("0", "-----"), new("1", ".----"), new("2", "..---"), new("3", "...--"),
        new("4", "....-"), new("5", "....."), new("6", "-...."), new("7", "--..."),
        new("8", "---.."), new("9", "----.")
    ];

    public static MorseEntry[] GetPunctuationEntries() =>
    [
        new(".",  ".-.-.-"),  new(",",  "--..--"),  new("?",  "..--.."),
        new("'",  ".----."),  new("!",  "-.-.--"),  new("/",  "-..-."),
        new("(",  "-.--."),   new(")",  "-.--.-"),  new("&",  ".-..."),
        new(":",  "---..."),  new(";",  "-.-.-."),  new("=",  "-...-"),
        new("+",  ".-.-."),   new("-",  "-....-"),  new("_",  "..--.-"),
        new("\"", ".-..-."),  new("@",  ".--.-.")
    ];
}
