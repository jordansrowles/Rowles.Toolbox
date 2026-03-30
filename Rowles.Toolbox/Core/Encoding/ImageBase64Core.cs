namespace Rowles.Toolbox.Core.Encoding;

public static class ImageBase64Core
{
    public static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1_048_576 => $"{bytes / 1024.0:F1} KB",
        < 1_073_741_824 => $"{bytes / 1_048_576.0:F1} MB",
        _ => $"{bytes / 1_073_741_824.0:F2} GB"
    };

    public static (string DataUri, long EstimatedSize) NormaliseDataUri(string pastedUri)
    {
        string uri = pastedUri.Trim();

        if (!uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            uri = $"data:image/png;base64,{uri}";
        }

        long size = 0;
        int commaIndex = uri.IndexOf(',');
        if (commaIndex >= 0)
        {
            string base64Part = uri[(commaIndex + 1)..];
            size = (long)(base64Part.Length * 3.0 / 4.0);
        }

        return (uri, size);
    }
}
