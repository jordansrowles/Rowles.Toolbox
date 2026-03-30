namespace Rowles.Toolbox.Core.MathConverters;

public static class TimeToolsCore
{
    public enum TimeTab { Duration, AddSubtract, TimeSince, TimeUntil, DurationFormat, WorkingDays, WeekNumber }

    public static List<(string label, string value)> GetDurationParts(TimeSpan span)
    {
        TimeSpan abs = span.Duration();
        return
        [
            ("Years", ((int)(abs.TotalDays / 365.25)).ToString()),
            ("Months", ((int)(abs.TotalDays / 30.4375)).ToString()),
            ("Weeks", ((int)(abs.TotalDays / 7)).ToString()),
            ("Days", ((int)abs.TotalDays).ToString()),
            ("Hours", ((int)abs.TotalHours).ToString("N0")),
            ("Minutes", ((int)abs.TotalMinutes).ToString("N0"))
        ];
    }

    public static string FormatTotalDuration(TimeSpan span)
    {
        TimeSpan abs = span.Duration();
        int totalDays = (int)abs.TotalDays;
        int years = totalDays / 365;
        int remainingDays = totalDays % 365;
        int months = remainingDays / 30;
        int days = remainingDays % 30;

        List<string> parts = [];
        if (years > 0) parts.Add($"{years} year{(years != 1 ? "s" : "")}");
        if (months > 0) parts.Add($"{months} month{(months != 1 ? "s" : "")}");
        if (days > 0) parts.Add($"{days} day{(days != 1 ? "s" : "")}");
        if (abs.Hours > 0) parts.Add($"{abs.Hours} hour{(abs.Hours != 1 ? "s" : "")}");
        if (abs.Minutes > 0) parts.Add($"{abs.Minutes} minute{(abs.Minutes != 1 ? "s" : "")}");
        if (abs.Seconds > 0) parts.Add($"{abs.Seconds} second{(abs.Seconds != 1 ? "s" : "")}");

        return parts.Count > 0 ? string.Join(", ", parts) : "0 seconds";
    }

    public static string FormatSeconds(long totalSeconds)
    {
        long abs = Math.Abs(totalSeconds);
        long days = abs / 86400;
        long hours = (abs % 86400) / 3600;
        long minutes = (abs % 3600) / 60;
        long seconds = abs % 60;

        List<string> parts = [];
        if (days > 0) parts.Add($"{days}d");
        if (hours > 0) parts.Add($"{hours}h");
        if (minutes > 0) parts.Add($"{minutes}m");
        parts.Add($"{seconds}s");

        return string.Join(" ", parts);
    }

    public static long ParseDurationToSeconds(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;

        long total = 0;
        string normalized = input.Trim().ToLowerInvariant();

        int i = 0;
        while (i < normalized.Length)
        {
            while (i < normalized.Length && char.IsWhiteSpace(normalized[i])) i++;
            if (i >= normalized.Length) break;

            int numStart = i;
            while (i < normalized.Length && (char.IsDigit(normalized[i]) || normalized[i] == '.')) i++;
            if (i == numStart) { i++; continue; }

            string numStr = normalized[numStart..i];
            if (!double.TryParse(numStr, System.Globalization.CultureInfo.InvariantCulture, out double num)) continue;

            while (i < normalized.Length && char.IsWhiteSpace(normalized[i])) i++;

            if (i < normalized.Length)
            {
                char unit = normalized[i];
                switch (unit)
                {
                    case 'd': total += (long)(num * 86400); i++; break;
                    case 'h': total += (long)(num * 3600); i++; break;
                    case 'm': total += (long)(num * 60); i++; break;
                    case 's': total += (long)num; i++; break;
                    default: total += (long)num; break;
                }
            }
            else
            {
                total += (long)num;
            }
        }

        return total;
    }

    public static (int total, int working, int weekend) CountWorkingDays(DateTimeOffset start, DateTimeOffset end)
    {
        if (end < start) return (0, 0, 0);

        int total = 0;
        int working = 0;
        int weekend = 0;

        DateTimeOffset current = start.Date;
        DateTimeOffset endDate = end.Date;

        while (current <= endDate)
        {
            total++;
            if (current.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                weekend++;
            else
                working++;
            current = current.AddDays(1);
        }

        return (total, working, weekend);
    }

    public static string TabIcon(TimeTab tab) => tab switch
    {
        TimeTab.Duration => "ti-arrows-horizontal",
        TimeTab.AddSubtract => "ti-plus-minus",
        TimeTab.TimeSince => "ti-history",
        TimeTab.TimeUntil => "ti-clock-forward",
        TimeTab.DurationFormat => "ti-clock",
        TimeTab.WorkingDays => "ti-briefcase",
        TimeTab.WeekNumber => "ti-calendar",
        _ => "ti-clock"
    };

    public static string TabLabel(TimeTab tab) => tab switch
    {
        TimeTab.Duration => "Duration",
        TimeTab.AddSubtract => "Add/Subtract",
        TimeTab.TimeSince => "Time Since",
        TimeTab.TimeUntil => "Time Until",
        TimeTab.DurationFormat => "Format",
        TimeTab.WorkingDays => "Working Days",
        TimeTab.WeekNumber => "Week Number",
        _ => tab.ToString()
    };
}
