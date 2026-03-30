namespace Rowles.Toolbox.Core.Colour;

public static class WcagContrastCore
{
    public static readonly (string Level, string Size, double Threshold)[] WcagCriteria =
    [
        ("AA", "Normal text", 4.5),
        ("AA", "Large text", 3.0),
        ("AAA", "Normal text", 7.0),
        ("AAA", "Large text", 4.5)
    ];

    public static string FormatRatio(double ratio) =>
        $"{ratio:F2}:1";

    public static double SrgbLinearise(int channel)
    {
        double srgb = channel / 255.0;
        return srgb <= 0.04045
            ? srgb / 12.92
            : Math.Pow((srgb + 0.055) / 1.055, 2.4);
    }

    public static double RelativeLuminance(string hex)
    {
        (int r, int g, int b) = ColourConverterCore.HexToRgb(hex);
        return 0.2126 * SrgbLinearise(r) + 0.7152 * SrgbLinearise(g) + 0.0722 * SrgbLinearise(b);
    }

    public static double CalculateContrastRatio(string hex1, string hex2)
    {
        double l1 = RelativeLuminance(hex1);
        double l2 = RelativeLuminance(hex2);
        double lighter = Math.Max(l1, l2);
        double darker = Math.Min(l1, l2);
        return (lighter + 0.05) / (darker + 0.05);
    }

    public static string? FindAccessibleColour(string fgHex, string bgHex, double targetRatio)
    {
        (int r, int g, int b) = ColourConverterCore.HexToRgb(fgHex);
        double bgLum = RelativeLuminance(bgHex);
        bool darken = bgLum > 0.5;

        for (int step = 1; step <= 255; step++)
        {
            int nr, ng, nb;
            if (darken)
            {
                nr = Math.Max(0, r - step);
                ng = Math.Max(0, g - step);
                nb = Math.Max(0, b - step);
            }
            else
            {
                nr = Math.Min(255, r + step);
                ng = Math.Min(255, g + step);
                nb = Math.Min(255, b + step);
            }

            string candidate = $"{nr:x2}{ng:x2}{nb:x2}";
            double ratio = CalculateContrastRatio(candidate, bgHex);
            if (ratio >= targetRatio)
                return candidate;
        }

        return null;
    }
}
