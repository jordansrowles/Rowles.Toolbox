using System.Globalization;

namespace Rowles.Toolbox.Core.DataFormats;

public static class MemorySizeFormatterCore
{
    public sealed record SizeUnit(string Name, string Symbol, double Factor);

    public static readonly SizeUnit[] DecimalUnits =
    [
        new("Bytes", "B", 1),
        new("Kilobytes", "KB", 1_000),
        new("Megabytes", "MB", 1_000_000),
        new("Gigabytes", "GB", 1_000_000_000),
        new("Terabytes", "TB", 1_000_000_000_000),
        new("Petabytes", "PB", 1_000_000_000_000_000),
    ];

    public static readonly SizeUnit[] BinaryUnits =
    [
        new("Bytes", "B", 1),
        new("Kibibytes", "KiB", 1024),
        new("Mebibytes", "MiB", 1_048_576),
        new("Gibibytes", "GiB", 1_073_741_824),
        new("Tebibytes", "TiB", 1_099_511_627_776),
        new("Pebibytes", "PiB", 1_125_899_906_842_624),
    ];

    public static readonly SizeUnit[] AllUnits =
    [
        new("Bytes", "B", 1),
        new("Kilobytes", "KB", 1_000),
        new("Megabytes", "MB", 1_000_000),
        new("Gigabytes", "GB", 1_000_000_000),
        new("Terabytes", "TB", 1_000_000_000_000),
        new("Kibibytes", "KiB", 1024),
        new("Mebibytes", "MiB", 1_048_576),
        new("Gibibytes", "GiB", 1_073_741_824),
        new("Tebibytes", "TiB", 1_099_511_627_776),
    ];

    public static string FormatNumber(double value)
    {
        if (value == 0) return "0";
        if (Math.Abs(value) >= 1)
            return value.ToString("N6", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
        return value.ToString("G10", CultureInfo.InvariantCulture);
    }

    public static string HumanReadable(double bytes, bool binary)
    {
        string[] suffixes = binary
            ? ["B", "KiB", "MiB", "GiB", "TiB", "PiB"]
            : ["B", "KB", "MB", "GB", "TB", "PB"];
        double divisor = binary ? 1024 : 1000;
        int index = 0;
        double size = bytes;
        while (size >= divisor && index < suffixes.Length - 1)
        {
            size /= divisor;
            index++;
        }
        return $"{size:0.##} {suffixes[index]}";
    }
}
