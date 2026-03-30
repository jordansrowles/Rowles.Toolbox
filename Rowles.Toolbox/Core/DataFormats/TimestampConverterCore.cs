namespace Rowles.Toolbox.Core.DataFormats;

public static class TimestampConverterCore
{
    public static string GetRelativeTime(DateTimeOffset date)
    {
        TimeSpan diff = DateTimeOffset.UtcNow - date;
        bool isFuture = diff.TotalSeconds < 0;
        TimeSpan absDiff = isFuture ? diff.Negate() : diff;

        string relative;
        if (absDiff.TotalSeconds < 60)
            relative = $"{(int)absDiff.TotalSeconds} second{((int)absDiff.TotalSeconds == 1 ? "" : "s")}";
        else if (absDiff.TotalMinutes < 60)
            relative = $"{(int)absDiff.TotalMinutes} minute{((int)absDiff.TotalMinutes == 1 ? "" : "s")}";
        else if (absDiff.TotalHours < 24)
            relative = $"{(int)absDiff.TotalHours} hour{((int)absDiff.TotalHours == 1 ? "" : "s")}";
        else if (absDiff.TotalDays < 30)
            relative = $"{(int)absDiff.TotalDays} day{((int)absDiff.TotalDays == 1 ? "" : "s")}";
        else if (absDiff.TotalDays < 365)
            relative = $"{(int)(absDiff.TotalDays / 30)} month{((int)(absDiff.TotalDays / 30) == 1 ? "" : "s")}";
        else
            relative = $"{(int)(absDiff.TotalDays / 365)} year{((int)(absDiff.TotalDays / 365) == 1 ? "" : "s")}";

        return isFuture ? $"in {relative}" : $"{relative} ago";
    }

    public static List<(string Label, long Ts)> GetReferenceTimestamps()
    {
        return
        [
            ("Unix Epoch", 0),
            ("Y2K", 946684800),
            ("2010-01-01", 1262304000),
            ("2020-01-01", 1577836800),
            ("2025-01-01", 1735689600),
            ("2030-01-01", 1893456000),
            ("Max 32-bit", 2147483647),
            ("2038 Problem", 2147483648)
        ];
    }
}
