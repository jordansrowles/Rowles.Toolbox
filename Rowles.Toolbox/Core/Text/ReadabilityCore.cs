using System.Globalization;

namespace Rowles.Toolbox.Core.Text;

public static class ReadabilityCore
{
    public sealed record ReadabilityLevel(string Label, int Min, int Max, string Range);

    public static readonly ReadabilityLevel[] ReadabilityLevels =
    [
        new("Very Easy (5th Grade)", 90, 100, "90\u2013100"),
        new("Easy (6th Grade)", 80, 89, "80\u201389"),
        new("Fairly Easy (7th Grade)", 70, 79, "70\u201379"),
        new("Standard (8th\u20139th Grade)", 60, 69, "60\u201369"),
        new("Fairly Difficult (10th\u201312th Grade)", 50, 59, "50\u201359"),
        new("Difficult (College)", 30, 49, "30\u201349"),
        new("Very Difficult (Professional)", 0, 29, "0\u201329"),
    ];

    public static string[] GetWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        return text.Split((char[])[' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
    }

    public static int CountSentences(string text)
    {
        int count = 0;
        foreach (char c in text)
        {
            if (c is '.' or '!' or '?') count++;
        }
        return Math.Max(count, 1);
    }

    public static int CountSyllables(string word)
    {
        string lower = word.ToLowerInvariant().Trim();
        char[] letters = lower.Where(char.IsLetter).ToArray();
        if (letters.Length == 0) return 1;

        string cleaned = new string(letters);
        int count = 0;
        bool previousWasVowel = false;

        for (int i = 0; i < cleaned.Length; i++)
        {
            bool isVowel = cleaned[i] is 'a' or 'e' or 'i' or 'o' or 'u' or 'y';
            if (isVowel && !previousWasVowel) count++;
            previousWasVowel = isVowel;
        }

        if (cleaned.Length > 2 && cleaned[^1] == 'e') count--;
        return Math.Max(count, 1);
    }

    public static int TotalSyllables(string[] words)
    {
        int total = 0;
        foreach (string word in words) total += CountSyllables(word);
        return total;
    }

    public static string CalculateReadingTime(int wordCount)
    {
        if (wordCount == 0) return "0s";
        double minutes = (double)wordCount / 200.0;
        if (minutes < 1.0)
        {
            int seconds = (int)Math.Ceiling(minutes * 60.0);
            return $"{seconds}s";
        }
        int wholeMinutes = (int)Math.Ceiling(minutes);
        return $"{wholeMinutes} min";
    }

    public static double CalculateFleschReadingEase(int wordCount, int sentenceCount, int syllableCount)
    {
        if (wordCount == 0) return 0.0;
        double score = 206.835
            - 1.015 * ((double)wordCount / sentenceCount)
            - 84.6 * ((double)syllableCount / wordCount);
        return Math.Clamp(score, 0.0, 100.0);
    }

    public static double CalculateFleschKincaidGrade(int wordCount, int sentenceCount, int syllableCount)
    {
        if (wordCount == 0) return 0.0;
        double grade = 0.39 * ((double)wordCount / sentenceCount)
            + 11.8 * ((double)syllableCount / wordCount)
            - 15.59;
        return Math.Max(grade, 0.0);
    }

    public static string GetReadingEaseLabel(double score)
    {
        foreach (ReadabilityLevel level in ReadabilityLevels)
        {
            if (score >= level.Min && score <= level.Max) return level.Label;
        }
        return "Very Difficult (Professional)";
    }

    public static string GetGradeLevelLabel(double grade)
    {
        return grade switch
        {
            <= 5.0 => "Elementary School",
            <= 8.0 => "Middle School",
            <= 12.0 => "High School",
            <= 16.0 => "College",
            _ => "Graduate / Professional"
        };
    }
}
