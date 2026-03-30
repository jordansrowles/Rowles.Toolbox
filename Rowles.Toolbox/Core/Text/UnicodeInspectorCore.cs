using System.Globalization;
using System.Text;

namespace Rowles.Toolbox.Core.Text;

public static class UnicodeInspectorCore
{
    public static List<string> GetTextElements(string input)
    {
        List<string> elements = new();
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(input);
        while (enumerator.MoveNext())
        {
            string textElement = enumerator.GetTextElement();
            elements.Add(textElement);
        }
        return elements;
    }

    public static int GetCodepoint(string textElement)
    {
        if (char.IsSurrogatePair(textElement, 0))
        {
            return char.ConvertToUtf32(textElement, 0);
        }
        return textElement[0];
    }

    public static string FormatCodepoint(int codepoint)
    {
        return codepoint > 0xFFFF
            ? $"U+{codepoint:X5}"
            : $"U+{codepoint:X4}";
    }

    public static string FormatUtf8Bytes(string textElement)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(textElement);
        StringBuilder sb = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(bytes[i].ToString("X2"));
        }
        return sb.ToString();
    }

    public static UnicodeCategory GetCategory(string textElement)
    {
        if (char.IsSurrogatePair(textElement, 0))
        {
            int codepoint = char.ConvertToUtf32(textElement, 0);
            return CharUnicodeInfo.GetUnicodeCategory(codepoint);
        }
        return char.GetUnicodeCategory(textElement[0]);
    }

    public static string GetCategoryName(UnicodeCategory category)
    {
        return category switch
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
            UnicodeCategory.InitialQuotePunctuation => "Initial Quote Punctuation",
            UnicodeCategory.FinalQuotePunctuation => "Final Quote Punctuation",
            UnicodeCategory.OtherPunctuation => "Other Punctuation",
            UnicodeCategory.MathSymbol => "Math Symbol",
            UnicodeCategory.CurrencySymbol => "Currency Symbol",
            UnicodeCategory.ModifierSymbol => "Modifier Symbol",
            UnicodeCategory.OtherSymbol => "Other Symbol",
            UnicodeCategory.OtherNotAssigned => "Not Assigned",
            _ => category.ToString()
        };
    }
}
