namespace Rowles.Toolbox.Core.Image;

public static class WebPConverterCore
{
    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    public static double CalculateSavings(long originalSize, long convertedSize)
    {
        return originalSize > 0 ? (1 - (convertedSize / (double)originalSize)) * 100 : 0;
    }

    public static double CalculateSizePercentage(long originalSize, long convertedSize)
    {
        return originalSize > 0 ? (convertedSize * 100.0 / originalSize) : 0;
    }
}
