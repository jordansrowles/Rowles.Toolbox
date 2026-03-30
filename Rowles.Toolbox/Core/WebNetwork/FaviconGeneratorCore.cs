namespace Rowles.Toolbox.Core.WebNetwork;

public static class FaviconGeneratorCore
{
    public sealed record FaviconSpec(int Size, string Filename, string Label);

    public sealed class FaviconEntry
    {
        public int Size { get; init; }
        public string Filename { get; init; } = string.Empty;
        public string DataUrl { get; init; } = string.Empty;
        public long ByteSize { get; init; }
    }

    public static readonly string CheckerPattern = Uri.EscapeDataString(
        "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"10\" height=\"10\"><rect width=\"5\" height=\"5\" fill=\"%23ccc\"/><rect x=\"5\" y=\"5\" width=\"5\" height=\"5\" fill=\"%23ccc\"/></svg>");

    public static readonly FaviconSpec[] FaviconSpecs =
    [
        new(16, "favicon-16x16.png", "Browser tab"),
        new(32, "favicon-32x32.png", "Browser tab HiDPI"),
        new(48, "favicon-48x48.png", "Windows shortcut"),
        new(64, "favicon-64x64.png", "Windows shortcut HiDPI"),
        new(128, "favicon-128x128.png", "Chrome Web Store"),
        new(180, "apple-touch-icon.png", "iOS home screen"),
        new(192, "android-chrome-192x192.png", "Android / PWA"),
        new(512, "android-chrome-512x512.png", "Android / PWA splash"),
    ];

    public static readonly string HtmlSnippet = string.Join(Environment.NewLine,
    [
        "<link rel=\"icon\" type=\"image/png\" sizes=\"16x16\" href=\"/favicon-16x16.png\">",
        "<link rel=\"icon\" type=\"image/png\" sizes=\"32x32\" href=\"/favicon-32x32.png\">",
        "<link rel=\"icon\" type=\"image/png\" sizes=\"48x48\" href=\"/favicon-48x48.png\">",
        "<link rel=\"icon\" type=\"image/png\" sizes=\"64x64\" href=\"/favicon-64x64.png\">",
        "<link rel=\"icon\" type=\"image/png\" sizes=\"128x128\" href=\"/favicon-128x128.png\">",
        "<link rel=\"apple-touch-icon\" sizes=\"180x180\" href=\"/apple-touch-icon.png\">",
        "<link rel=\"icon\" type=\"image/png\" sizes=\"192x192\" href=\"/android-chrome-192x192.png\">",
        "<link rel=\"icon\" type=\"image/png\" sizes=\"512x512\" href=\"/android-chrome-512x512.png\">",
    ]);

    public static string SizeColorClass(int size) => size switch
    {
        16 or 32 => "bg-blue-400",
        48 or 64 => "bg-purple-400",
        128 => "bg-amber-400",
        180 => "bg-pink-400",
        192 or 512 => "bg-green-400",
        _ => "bg-gray-400",
    };

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}
