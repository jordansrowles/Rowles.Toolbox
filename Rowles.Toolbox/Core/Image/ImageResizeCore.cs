namespace Rowles.Toolbox.Core.Image;

public static class ImageResizeCore
{
    public static readonly (int Width, int Height)[] Presets =
    [
        (16, 16), (32, 32), (64, 64), (128, 128),
        (256, 256), (512, 512), (1024, 1024),
        (1920, 1080), (3840, 2160)
    ];

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    public static int CalculateHeightFromWidth(int width, double aspectRatio)
    {
        return (int)Math.Round(width / aspectRatio);
    }

    public static int CalculateWidthFromHeight(int height, double aspectRatio)
    {
        return (int)Math.Round(height * aspectRatio);
    }

    public static string GetOutputExtension(string outputFormat)
    {
        return outputFormat == "jpeg" ? "jpg" : outputFormat;
    }
}
