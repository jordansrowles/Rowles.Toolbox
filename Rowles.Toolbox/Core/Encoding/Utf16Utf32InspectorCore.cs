using System.Globalization;
using System.Text;

namespace Rowles.Toolbox.Core.Encoding;

public static class Utf16Utf32InspectorCore
{
    public sealed class CodePointInfo
    {
        public int Index { get; set; }
        public string Display { get; set; } = string.Empty;
        public int CodePoint { get; set; }
        public string CodePointHex { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Utf8Hex { get; set; } = string.Empty;
        public int Utf8ByteCount { get; set; }
        public string Utf16BeHex { get; set; } = string.Empty;
        public string Utf16LeHex { get; set; } = string.Empty;
        public int Utf16ByteCount { get; set; }
        public string Utf32BeHex { get; set; } = string.Empty;
        public string Utf32LeHex { get; set; } = string.Empty;
        public int Utf32ByteCount { get; set; }
        public bool HasSurrogates { get; set; }
        public int HighSurrogate { get; set; }
        public int LowSurrogate { get; set; }
    }

    private static readonly UTF32Encoding Utf32BeEncoding = new(bigEndian: true, byteOrderMark: false);

    public static List<CodePointInfo> Analyze(string inputText)
    {
        List<CodePointInfo> inspections = new();
        if (string.IsNullOrEmpty(inputText))
            return inspections;

        int charIndex = 0;
        int ordinal = 0;
        while (charIndex < inputText.Length)
        {
            ordinal++;
            int codePoint;
            string display;

            if (char.IsHighSurrogate(inputText[charIndex]) &&
                charIndex + 1 < inputText.Length &&
                char.IsLowSurrogate(inputText[charIndex + 1]))
            {
                codePoint = char.ConvertToUtf32(inputText[charIndex], inputText[charIndex + 1]);
                display = inputText.Substring(charIndex, 2);
                charIndex += 2;
            }
            else
            {
                codePoint = inputText[charIndex];
                display = inputText[charIndex].ToString();
                charIndex += 1;
            }

            string charStr = char.ConvertFromUtf32(codePoint);

            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(charStr);
            byte[] utf16BeBytes = System.Text.Encoding.BigEndianUnicode.GetBytes(charStr);
            byte[] utf16LeBytes = System.Text.Encoding.Unicode.GetBytes(charStr);
            byte[] utf32BeBytes = Utf32BeEncoding.GetBytes(charStr);
            byte[] utf32LeBytes = System.Text.Encoding.UTF32.GetBytes(charStr);

            bool hasSurrogates = codePoint >= 0x10000;
            int highSurrogate = 0;
            int lowSurrogate = 0;
            if (hasSurrogates)
            {
                int offset = codePoint - 0x10000;
                highSurrogate = 0xD800 + (offset >> 10);
                lowSurrogate = 0xDC00 + (offset & 0x3FF);
            }

            UnicodeCategory category = GetUnicodeCategory(codePoint);

            inspections.Add(new CodePointInfo
            {
                Index = ordinal,
                Display = display,
                CodePoint = codePoint,
                CodePointHex = FormatCodePoint(codePoint),
                CategoryName = GetCategoryName(category),
                Utf8Hex = FormatBytesHex(utf8Bytes),
                Utf8ByteCount = utf8Bytes.Length,
                Utf16BeHex = FormatBytesHex(utf16BeBytes),
                Utf16LeHex = FormatBytesHex(utf16LeBytes),
                Utf16ByteCount = utf16BeBytes.Length,
                Utf32BeHex = FormatBytesHex(utf32BeBytes),
                Utf32LeHex = FormatBytesHex(utf32LeBytes),
                Utf32ByteCount = utf32BeBytes.Length,
                HasSurrogates = hasSurrogates,
                HighSurrogate = highSurrogate,
                LowSurrogate = lowSurrogate
            });
        }

        return inspections;
    }

    public static string FormatCodePoint(int codePoint) =>
        codePoint > 0xFFFF ? $"U+{codePoint:X5}" : $"U+{codePoint:X4}";

    public static string FormatBytesHex(byte[] bytes)
    {
        StringBuilder sb = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(bytes[i].ToString("X2"));
        }
        return sb.ToString();
    }

    public static UnicodeCategory GetUnicodeCategory(int codePoint)
    {
        if (codePoint <= 0xFFFF)
            return char.GetUnicodeCategory((char)codePoint);
        string charStr = char.ConvertFromUtf32(codePoint);
        return CharUnicodeInfo.GetUnicodeCategory(charStr, 0);
    }

    public static string GetCategoryName(UnicodeCategory category) => category switch
    {
        UnicodeCategory.UppercaseLetter => "Uppercase Letter",
        UnicodeCategory.LowercaseLetter => "Lowercase Letter",
        UnicodeCategory.TitlecaseLetter => "Titlecase Letter",
        UnicodeCategory.ModifierLetter => "Modifier Letter",
        UnicodeCategory.OtherLetter => "Other Letter",
        UnicodeCategory.NonSpacingMark => "Non-Spacing Mark",
        UnicodeCategory.SpacingCombiningMark => "Spacing Combining Mark",
        UnicodeCategory.EnclosingMark => "Enclosing Mark",
        UnicodeCategory.DecimalDigitNumber => "Decimal Digit",
        UnicodeCategory.LetterNumber => "Letter Number",
        UnicodeCategory.OtherNumber => "Other Number",
        UnicodeCategory.SpaceSeparator => "Space Separator",
        UnicodeCategory.LineSeparator => "Line Separator",
        UnicodeCategory.ParagraphSeparator => "Paragraph Separator",
        UnicodeCategory.Control => "Control Character",
        UnicodeCategory.Format => "Format Character",
        UnicodeCategory.Surrogate => "Surrogate",
        UnicodeCategory.PrivateUse => "Private Use",
        UnicodeCategory.ConnectorPunctuation => "Connector Punctuation",
        UnicodeCategory.DashPunctuation => "Dash Punctuation",
        UnicodeCategory.OpenPunctuation => "Open Punctuation",
        UnicodeCategory.ClosePunctuation => "Close Punctuation",
        UnicodeCategory.InitialQuotePunctuation => "Initial Quote",
        UnicodeCategory.FinalQuotePunctuation => "Final Quote",
        UnicodeCategory.OtherPunctuation => "Other Punctuation",
        UnicodeCategory.MathSymbol => "Math Symbol",
        UnicodeCategory.CurrencySymbol => "Currency Symbol",
        UnicodeCategory.ModifierSymbol => "Modifier Symbol",
        UnicodeCategory.OtherSymbol => "Other Symbol",
        UnicodeCategory.OtherNotAssigned => "Not Assigned",
        _ => category.ToString()
    };

    public static int GetTextElementCount(string inputText)
    {
        StringInfo.GetTextElementEnumerator(inputText);
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(inputText);
        int count = 0;
        while (enumerator.MoveNext()) count++;
        return count;
    }

    public static int GetTotalBytes(List<CodePointInfo> inspections, Func<CodePointInfo, int> selector)
    {
        int total = 0;
        foreach (CodePointInfo item in inspections) total += selector(item);
        return total;
    }

    public static int GetSurrogatePairCount(List<CodePointInfo> inspections)
    {
        int count = 0;
        foreach (CodePointInfo item in inspections)
            if (item.HasSurrogates) count++;
        return count;
    }

    public static bool HasAnySurrogates(List<CodePointInfo> inspections)
    {
        foreach (CodePointInfo item in inspections)
            if (item.HasSurrogates) return true;
        return false;
    }
}
