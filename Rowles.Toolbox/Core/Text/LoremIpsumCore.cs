namespace Rowles.Toolbox.Core.Text;

public static class LoremIpsumCore
{
    public enum FormatType { Paragraphs, Sentences, Words }

    public static readonly string[] LoremParagraphs =
    [
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
        "Curabitur pretium tincidunt lacus. Nulla gravida orci a odio. Nullam varius, turpis et commodo pharetra, est eros bibendum elit, nec luctus magna felis sollicitudin mauris. Integer in mauris eu nibh euismod gravida. Duis ac tellus et risus vulputate vehicula. Donec lobortis risus a elit. Etiam tempor. Ut ullamcorper, ligula ut dictum pharetra, nisi nunc fringilla magna, in commodo elit erat nec turpis. Ut pharetra augue nec augue.",
        "Praesent dapibus, neque id cursus faucibus, tortor neque egestas augue, eu vulputate magna eros eu erat. Aliquam erat volutpat. Nam dui mi, tincidunt quis, accumsan porttitor, facilisis luctus, metus. Phasellus ultrices nulla quis nibh. Quisque a lectus. Donec consectetuer ligula vulputate sem tristique cursus. Nam nulla quam, gravida non, commodo a, sodales sit amet, nisi.",
        "Pellentesque fermentum dolor. Aliquam quam lectus, facilisis auctor, ultrices ut, elementum vulputate, nunc. Sed adipiscing ornare risus. Morbi est est, blandit sit amet, sagittis vel, euismod vel, velit. Pellentesque egestas sem. Suspendisse commodo ullamcorper magna. Ut nulla. Vivamus bibendum, nulla ut congue fringilla, lorem ipsum ultricies risus, ut rutrum velit tortor vel purus.",
        "Fusce mauris. Vestibulum luctus nibh at lectus. Sed bibendum, nulla a faucibus semper, leo velit ultricies tellus, ac venenatis arcu wisi vel nisl. Vestibulum diam. Aliquam pellentesque, augue quis sagittis posuere, turpis lacus congue quam, in hendrerit risus eros eget felis. Maecenas eget erat in sapien mattis facilisis. Vel nisi. In hac habitasse platea dictumst.",
        "Aenean ut eros et nisl sagittis vestibulum. Nullam nulla eros, ultricies sit amet, nonummy id, imperdiet feugiat, pede. Sed lectus. Donec mollis hendrerit risus. Phasellus nec sem in justo pellentesque facilisis. Etiam imperdiet imperdiet orci. Nunc nec neque. Phasellus leo dolor, tempus non, auctor et, hendrerit quis, nisi. Curabitur ligula sapien, tincidunt non, euismod vitae, posuere imperdiet, leo.",
        "Morbi mattis ullamcorper velit. Phasellus gravida semper nisi. Nullam vel sem. Pellentesque libero tortor, tincidunt et, tincidunt eget, semper nec, quam. Sed hendrerit. Morbi ac felis. Nunc egestas, augue at pellentesque laoreet, felis eros vehicula leo, at malesuada velit leo quis pede. Donec interdum, metus et hendrerit aliquet, dolor diam sagittis ligula, eget egestas libero turpis vel mi."
    ];

    public static readonly string[] LoremSentences = BuildSentences();
    public static readonly string[] LoremWords = BuildWords();

    private static string[] BuildSentences()
    {
        List<string> sentences = new();
        foreach (string paragraph in LoremParagraphs)
        {
            string[] parts = paragraph.Split(". ", StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                string sentence = trimmed.EndsWith('.') ? trimmed : trimmed + ".";
                sentences.Add(sentence);
            }
        }
        return sentences.ToArray();
    }

    private static string[] BuildWords()
    {
        List<string> words = new();
        foreach (string paragraph in LoremParagraphs)
        {
            string[] parts = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in parts)
            {
                string cleaned = word.Trim('.', ',');
                if (!string.IsNullOrEmpty(cleaned)) words.Add(cleaned.ToLowerInvariant());
            }
        }
        return words.ToArray();
    }

    public static string Generate(FormatType format, int count)
    {
        int safeCount = Math.Max(1, Math.Min(count, 999));
        return format switch
        {
            FormatType.Paragraphs => GenerateParagraphs(safeCount),
            FormatType.Sentences => GenerateSentences(safeCount),
            FormatType.Words => GenerateWords(safeCount),
            _ => string.Empty
        };
    }

    public static string GenerateParagraphs(int count)
    {
        string[] result = new string[count];
        for (int i = 0; i < count; i++)
            result[i] = LoremParagraphs[i % LoremParagraphs.Length];
        return string.Join("\n\n", result);
    }

    public static string GenerateSentences(int count)
    {
        string[] result = new string[count];
        for (int i = 0; i < count; i++)
            result[i] = LoremSentences[i % LoremSentences.Length];
        return string.Join(" ", result);
    }

    public static string GenerateWords(int count)
    {
        string[] result = new string[count];
        for (int i = 0; i < count; i++)
            result[i] = LoremWords[i % LoremWords.Length];
        return string.Join(" ", result);
    }
}
