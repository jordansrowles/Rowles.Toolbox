namespace Rowles.Toolbox.Core.Image;

public static class WatermarkCheckCore
{
    public static string BuildDataUrl(string contentType, string base64)
    {
        return string.Concat("data:", contentType, ";base64,", base64);
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return bytes + " B";
        if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F1") + " KB";
        return (bytes / (1024.0 * 1024.0)).ToString("F1") + " MB";
    }
}
