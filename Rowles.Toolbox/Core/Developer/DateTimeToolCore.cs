using System.Globalization;

namespace Rowles.Toolbox.Core.Developer;

public static class DateTimeToolCore
{
    public sealed record ParseTestResult(string Format, bool Success, string? ParsedValue);

    public static readonly string[] CultureCodes =
    [
        "en-GB", "en-US", "fr-FR", "de-DE", "ja-JP",
        "zh-CN", "ar-SA", "es-ES", "pt-BR", "ko-KR",
        "hi-IN", "ru-RU", "it-IT", "nl-NL", "sv-SE",
        "da-DK", "nb-NO", "fi-FI", "pl-PL", "tr-TR"
    ];

    public static readonly (string Specifier, string Name)[] StandardFormats =
    [
        ("d", "Short date"),
        ("D", "Long date"),
        ("f", "Full (short time)"),
        ("F", "Full (long time)"),
        ("g", "General (short time)"),
        ("G", "General (long time)"),
        ("o", "Round-trip"),
        ("R", "RFC1123"),
        ("s", "Sortable"),
        ("t", "Short time"),
        ("T", "Long time"),
        ("u", "Universal sortable"),
        ("U", "Universal full")
    ];

    public static readonly (string Format, string Description)[] CustomFormats =
    [
        ("yyyy-MM-dd", "ISO date"),
        ("HH:mm:ss", "24-hour time"),
        ("hh:mm:ss tt", "12-hour time"),
        ("yyyy-MM-dd HH:mm:ss", "Date and time"),
        ("yyyy-MM-ddTHH:mm:ssK", "ISO 8601 with offset"),
        ("ddd, dd MMM yyyy", "RFC-style date"),
        ("MMMM dd, yyyy", "Long month day year"),
        ("dd/MM/yyyy", "UK date"),
        ("MM/dd/yyyy", "US date"),
        ("yyyyMMddHHmmss", "Compact sortable"),
        ("yyyy-MM-dd HH:mm:ss.fff", "With milliseconds"),
        ("dddd", "Day name"),
        ("MMMM", "Month name"),
        ("zzz", "UTC offset")
    ];

    public static readonly string[] ParseTestFormats =
    [
        "yyyy-MM-dd",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.fffffffK",
        "yyyy-MM-ddTHH:mm:ssK",
        "dd/MM/yyyy",
        "MM/dd/yyyy",
        "dd-MM-yyyy",
        "MM-dd-yyyy",
        "dd/MM/yyyy HH:mm:ss",
        "MM/dd/yyyy HH:mm:ss",
        "yyyy/MM/dd",
        "yyyyMMdd",
        "yyyyMMddHHmmss",
        "ddd, dd MMM yyyy HH:mm:ss",
        "dd MMM yyyy",
        "MMM dd, yyyy",
        "MMMM dd, yyyy",
        "d MMMM yyyy",
        "HH:mm:ss",
        "hh:mm:ss tt",
        "yyyy-MM-dd HH:mm:ss.fff"
    ];

    public static string FormatExample(DateTime reference, string format)
    {
        try
        {
            return reference.ToString(format, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return "(invalid)";
        }
    }

    public static string FormatOffsetDifference(TimeSpan diff)
    {
        if (diff == TimeSpan.Zero)
        {
            return "±0:00";
        }

        string sign = diff > TimeSpan.Zero ? "+" : "−";
        TimeSpan abs = diff.Duration();
        return $"{sign}{(int)abs.TotalHours}:{abs.Minutes:D2}";
    }
}
