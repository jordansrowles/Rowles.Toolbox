namespace Rowles.Toolbox.Core.Generators;

public static class CssFilterBuilderCore
{
    public sealed record FilterPreset(
        string Name,
        double Blur,
        int Brightness,
        int Contrast,
        int Saturate,
        int Grayscale,
        int Sepia,
        int Invert,
        int HueRotate,
        int Opacity,
        bool DropShadowEnabled,
        int ShadowX,
        int ShadowY,
        int ShadowBlur,
        string ShadowColour
    );

    public static string FormatDecimal(double value)
    {
        return value == Math.Floor(value)
            ? ((int)value).ToString()
            : value.ToString("0.#");
    }

    public static string BuildFilterValue(
        bool blurEnabled, double blur,
        bool brightnessEnabled, int brightness,
        bool contrastEnabled, int contrast,
        bool saturateEnabled, int saturate,
        bool grayscaleEnabled, int grayscale,
        bool sepiaEnabled, int sepia,
        bool invertEnabled, int invert,
        bool hueRotateEnabled, int hueRotate,
        bool opacityEnabled, int opacity,
        bool dropShadowEnabled, int shadowX, int shadowY, int shadowBlur, string shadowColour)
    {
        List<string> parts = new();

        if (blurEnabled && blur != 0)
            parts.Add($"blur({FormatDecimal(blur)}px)");
        if (brightnessEnabled && brightness != 100)
            parts.Add($"brightness({brightness}%)");
        if (contrastEnabled && contrast != 100)
            parts.Add($"contrast({contrast}%)");
        if (saturateEnabled && saturate != 100)
            parts.Add($"saturate({saturate}%)");
        if (grayscaleEnabled && grayscale != 0)
            parts.Add($"grayscale({grayscale}%)");
        if (sepiaEnabled && sepia != 0)
            parts.Add($"sepia({sepia}%)");
        if (invertEnabled && invert != 0)
            parts.Add($"invert({invert}%)");
        if (hueRotateEnabled && hueRotate != 0)
            parts.Add($"hue-rotate({hueRotate}deg)");
        if (opacityEnabled && opacity != 100)
            parts.Add($"opacity({opacity}%)");
        if (dropShadowEnabled && (shadowX != 0 || shadowY != 0 || shadowBlur != 0))
            parts.Add($"drop-shadow({shadowX}px {shadowY}px {shadowBlur}px {shadowColour})");

        return parts.Count > 0 ? string.Join(" ", parts) : "none";
    }

    public static string GetFilterStyle(string filterValue)
    {
        return filterValue == "none" ? string.Empty : $"filter: {filterValue};";
    }

    public static string GetOutputText(string filterValue, bool showWebkitPrefix, bool showBackdropFilter)
    {
        System.Text.StringBuilder sb = new();

        sb.AppendLine($"filter: {filterValue};");

        if (showWebkitPrefix)
            sb.AppendLine($"-webkit-filter: {filterValue};");

        if (showBackdropFilter)
        {
            sb.AppendLine($"backdrop-filter: {filterValue};");
            if (showWebkitPrefix)
                sb.AppendLine($"-webkit-backdrop-filter: {filterValue};");
        }

        return sb.ToString().TrimEnd();
    }
}
