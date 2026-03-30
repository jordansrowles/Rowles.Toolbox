using System.Globalization;

namespace Rowles.Toolbox.Core.Developer;

public static class CultureFormatterCore
{
    public static readonly string[] CultureCodes =
    [
        "en-GB", "en-US", "fr-FR", "de-DE", "ja-JP",
        "zh-CN", "ar-SA", "es-ES", "pt-BR", "ko-KR",
        "hi-IN", "ru-RU", "it-IT", "nl-NL", "sv-SE",
        "da-DK", "nb-NO", "fi-FI", "pl-PL", "tr-TR"
    ];

    public static readonly (string Specifier, string Name)[] NumberFormats =
    [
        ("N",  "Number"),
        ("N0", "Number (0 dp)"),
        ("N2", "Number (2 dp)"),
        ("C",  "Currency"),
        ("C2", "Currency (2 dp)"),
        ("P",  "Percent"),
        ("P2", "Percent (2 dp)"),
        ("E",  "Scientific"),
        ("E2", "Scientific (2 dp)"),
        ("F",  "Fixed-point"),
        ("F2", "Fixed-point (2 dp)"),
        ("G",  "General"),
        ("X",  "Hexadecimal")
    ];

    public static readonly (string Specifier, string Name)[] DateFormats =
    [
        ("d", "Short date"),
        ("D", "Long date"),
        ("f", "Full (short time)"),
        ("F", "Full (long time)"),
        ("g", "General (short time)"),
        ("G", "General (long time)"),
        ("M", "Month/day"),
        ("Y", "Year/month"),
        ("o", "Round-trip"),
        ("R", "RFC1123"),
        ("s", "Sortable"),
        ("t", "Short time"),
        ("T", "Long time"),
        ("u", "Universal sortable")
    ];

    public static readonly (string Specifier, string Name)[] CurrencyFormats =
    [
        ("C",  "Currency (default)"),
        ("C0", "Currency (0 dp)"),
        ("C2", "Currency (2 dp)"),
        ("C4", "Currency (4 dp)"),
        ("N2", "Number (2 dp)"),
        ("F2", "Fixed-point (2 dp)")
    ];

    public static readonly (string Specifier, string Name)[] ComparisonFormats =
    [
        ("N2",  "Number"),
        ("C",   "Currency"),
        ("P",   "Percent"),
        ("d",   "Short date"),
        ("D",   "Long date"),
        ("T",   "Long time"),
        ("G",   "General date"),
        ("Y",   "Year/month"),
        ("M",   "Month/day")
    ];

    public static string FormatNumber(string specifier, CultureInfo culture, double numberInput)
    {
        if (string.Equals(specifier, "X", StringComparison.OrdinalIgnoreCase))
        {
            long intValue = (long)numberInput;
            return intValue.ToString(specifier, culture);
        }

        return numberInput.ToString(specifier, culture);
    }

    public static string FormatDate(string specifier, CultureInfo culture, DateTime dateInput)
    {
        return dateInput.ToString(specifier, culture);
    }

    public static string FormatCurrency(string specifier, CultureInfo culture, double currencyInput)
    {
        return currencyInput.ToString(specifier, culture);
    }

    public static string FormatForComparison(
        string specifier,
        string name,
        CultureInfo culture,
        double numberInput,
        DateTime dateInput,
        double currencyInput)
    {
        bool isDateFormat = name.Contains("date", StringComparison.OrdinalIgnoreCase)
                         || name.Contains("time", StringComparison.OrdinalIgnoreCase)
                         || name.Contains("month", StringComparison.OrdinalIgnoreCase)
                         || name.Contains("year", StringComparison.OrdinalIgnoreCase);

        if (isDateFormat)
        {
            return dateInput.ToString(specifier, culture);
        }

        if (string.Equals(name, "Currency", StringComparison.OrdinalIgnoreCase))
        {
            return currencyInput.ToString(specifier, culture);
        }

        return numberInput.ToString(specifier, culture);
    }

    public static string EscapeWhitespace(string value)
    {
        if (string.Equals(value, "\u00A0", StringComparison.Ordinal))
        {
            return "NBSP";
        }

        if (string.Equals(value, "\u202F", StringComparison.Ordinal))
        {
            return "NNBSP";
        }

        if (string.IsNullOrEmpty(value))
        {
            return "(none)";
        }

        return value;
    }
}
