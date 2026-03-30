namespace Rowles.Toolbox.Core.WebNetwork;

public static class ScreenSizesCore
{
    public sealed record DeviceEntry(string Name, string Category, int Width, int Height, string AspectRatio, int Ppi, string Dpr);
    public sealed record BreakpointEntry(string Prefix, int MinWidth, string Css, string Description);

    public static string CategoryIcon(string category) => category switch
    {
        "Phone" => "ti-device-mobile",
        "Tablet" => "ti-device-tablet",
        "Laptop" => "ti-device-laptop",
        "Desktop" => "ti-device-desktop",
        "Watch" => "ti-device-watch",
        _ => "ti-device-desktop",
    };

    public static string GetBreakpointName(int viewportWidth)
    {
        if (viewportWidth >= 1536) return "2xl";
        if (viewportWidth >= 1280) return "xl";
        if (viewportWidth >= 1024) return "lg";
        if (viewportWidth >= 768) return "md";
        if (viewportWidth >= 640) return "sm";
        return "base";
    }

    public static readonly List<BreakpointEntry> Breakpoints =
    [
        new("sm", 640, "@media (min-width: 640px)", "Small phones landscape, large phones"),
        new("md", 768, "@media (min-width: 768px)", "Tablets portrait"),
        new("lg", 1024, "@media (min-width: 1024px)", "Tablets landscape, small laptops"),
        new("xl", 1280, "@media (min-width: 1280px)", "Laptops, small desktops"),
        new("2xl", 1536, "@media (min-width: 1536px)", "Large desktops, wide monitors"),
    ];

    public static readonly List<DeviceEntry> AllDevices =
    [
        // Phones
        new("iPhone 16 Pro Max", "Phone", 440, 956, "19.5:9", 460, "3x"),
        new("iPhone 16 Pro", "Phone", 402, 874, "19.5:9", 460, "3x"),
        new("iPhone 16", "Phone", 393, 852, "19.5:9", 460, "3x"),
        new("iPhone 15 Pro Max", "Phone", 430, 932, "19.5:9", 460, "3x"),
        new("iPhone 15 Pro", "Phone", 393, 852, "19.5:9", 460, "3x"),
        new("iPhone 15", "Phone", 393, 852, "19.5:9", 460, "3x"),
        new("iPhone 14 Pro Max", "Phone", 430, 932, "19.5:9", 460, "3x"),
        new("iPhone 14 Pro", "Phone", 393, 852, "19.5:9", 460, "3x"),
        new("iPhone 14", "Phone", 390, 844, "19.5:9", 460, "3x"),
        new("iPhone SE (3rd gen)", "Phone", 375, 667, "16:9", 326, "2x"),
        new("Samsung Galaxy S24 Ultra", "Phone", 412, 915, "19.5:9", 505, "3.75x"),
        new("Samsung Galaxy S24+", "Phone", 412, 915, "19.5:9", 390, "3x"),
        new("Samsung Galaxy S24", "Phone", 360, 780, "19.5:9", 416, "3x"),
        new("Samsung Galaxy A54", "Phone", 360, 800, "20:9", 401, "3x"),
        new("Samsung Galaxy Z Fold5", "Phone", 344, 882, "23.1:9", 426, "3x"),
        new("Samsung Galaxy Z Flip5", "Phone", 412, 915, "22:9", 426, "3x"),
        new("Google Pixel 8 Pro", "Phone", 412, 915, "20:9", 489, "3.5x"),
        new("Google Pixel 8", "Phone", 412, 915, "20:9", 428, "2.625x"),
        new("Google Pixel 7a", "Phone", 412, 892, "20:9", 429, "2.625x"),
        new("OnePlus 12", "Phone", 412, 915, "20:9", 450, "3.5x"),

        // Tablets
        new("iPad Pro 12.9\" (M2)", "Tablet", 1024, 1366, "4:3", 264, "2x"),
        new("iPad Pro 11\" (M2)", "Tablet", 834, 1194, "4.3:3", 264, "2x"),
        new("iPad Air (M2)", "Tablet", 820, 1180, "4.3:3", 264, "2x"),
        new("iPad (10th gen)", "Tablet", 820, 1180, "4.3:3", 264, "2x"),
        new("iPad Mini (6th gen)", "Tablet", 744, 1133, "4.3:3", 326, "2x"),
        new("Samsung Galaxy Tab S9 Ultra", "Tablet", 960, 1440, "16:10", 298, "2x"),
        new("Samsung Galaxy Tab S9+", "Tablet", 834, 1280, "16:10", 287, "2x"),
        new("Samsung Galaxy Tab S9", "Tablet", 753, 1128, "16:10", 274, "2x"),
        new("Surface Pro 9", "Tablet", 912, 1368, "3:2", 267, "2x"),
        new("Kindle Fire HD 10", "Tablet", 600, 960, "16:10", 149, "1x"),

        // Laptops
        new("MacBook Pro 16\" (M3)", "Laptop", 1728, 1117, "16:10", 254, "2x"),
        new("MacBook Pro 14\" (M3)", "Laptop", 1512, 982, "16:10", 254, "2x"),
        new("MacBook Air 15\" (M3)", "Laptop", 1710, 1112, "16:10", 224, "2x"),
        new("MacBook Air 13\" (M3)", "Laptop", 1470, 956, "16:10", 224, "2x"),
        new("Surface Laptop 5 15\"", "Laptop", 1536, 1024, "3:2", 201, "1.5x"),
        new("Surface Laptop 5 13.5\"", "Laptop", 1504, 1000, "3:2", 201, "1.5x"),
        new("Dell XPS 13", "Laptop", 1920, 1200, "16:10", 166, "1x"),
        new("Dell XPS 15", "Laptop", 1920, 1200, "16:10", 145, "1x"),
        new("ThinkPad X1 Carbon", "Laptop", 1920, 1200, "16:10", 157, "1x"),
        new("Chromebook (typical)", "Laptop", 1366, 768, "16:9", 135, "1x"),

        // Desktops
        new("HD (720p)", "Desktop", 1280, 720, "16:9", 0, "1x"),
        new("Full HD (1080p)", "Desktop", 1920, 1080, "16:9", 0, "1x"),
        new("QHD (1440p)", "Desktop", 2560, 1440, "16:9", 0, "1x"),
        new("4K UHD", "Desktop", 3840, 2160, "16:9", 0, "1x"),
        new("5K", "Desktop", 5120, 2880, "16:9", 0, "2x"),
        new("Ultrawide FHD", "Desktop", 2560, 1080, "21:9", 0, "1x"),
        new("Ultrawide QHD", "Desktop", 3440, 1440, "21:9", 0, "1x"),
        new("Super Ultrawide", "Desktop", 5120, 1440, "32:9", 0, "1x"),
        new("Apple Studio Display", "Desktop", 5120, 2880, "16:9", 218, "2x"),
        new("iMac 24\" (M3)", "Desktop", 4480, 2520, "16:9", 218, "2x"),

        // Watches
        new("Apple Watch Ultra 2", "Watch", 205, 251, "~5:4", 326, "2x"),
        new("Apple Watch Series 9 45mm", "Watch", 198, 242, "~5:4", 326, "2x"),
        new("Apple Watch Series 9 41mm", "Watch", 176, 215, "~5:4", 326, "2x"),
        new("Apple Watch SE 44mm", "Watch", 184, 224, "~5:4", 326, "2x"),
        new("Samsung Galaxy Watch6 44mm", "Watch", 192, 192, "1:1", 330, "2x"),
        new("Samsung Galaxy Watch6 40mm", "Watch", 176, 176, "1:1", 330, "2x"),
    ];
}
