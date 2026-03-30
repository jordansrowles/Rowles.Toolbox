namespace Rowles.Toolbox.Core.Image;

public static class ColourExtractorCore
{
    public static readonly (string Label, string Value)[] SortOptions =
    [
        ("Frequency", "frequency"),
        ("Hue", "hue"),
        ("Lightness", "lightness")
    ];

    public static string FormatRgb(ExtractedColour c) => $"rgb({c.R}, {c.G}, {c.B})";

    public static double GetHue(int r, int g, int b)
    {
        double rn = r / 255.0;
        double gn = g / 255.0;
        double bn = b / 255.0;
        double max = Math.Max(rn, Math.Max(gn, bn));
        double min = Math.Min(rn, Math.Min(gn, bn));
        double delta = max - min;

        if (delta == 0) return 0;

        double hue;
        if (max == rn) hue = 60 * (((gn - bn) / delta) % 6);
        else if (max == gn) hue = 60 * (((bn - rn) / delta) + 2);
        else hue = 60 * (((rn - gn) / delta) + 4);

        if (hue < 0) hue += 360;
        return hue;
    }

    public static double GetLightness(int r, int g, int b)
    {
        double rn = r / 255.0;
        double gn = g / 255.0;
        double bn = b / 255.0;
        double max = Math.Max(rn, Math.Max(gn, bn));
        double min = Math.Min(rn, Math.Min(gn, bn));
        return (max + min) / 2.0;
    }

    public static IReadOnlyList<ExtractedColour> GetSortedColours(List<ExtractedColour> colours, string sortBy) => sortBy switch
    {
        "hue" => [.. colours.OrderBy(c => GetHue(c.R, c.G, c.B))],
        "lightness" => [.. colours.OrderBy(c => GetLightness(c.R, c.G, c.B))],
        _ => colours
    };

    public sealed class ExtractedColour
    {
        public int R { get; init; }
        public int G { get; init; }
        public int B { get; init; }
        public string Hex { get; init; } = string.Empty;
        public double Percentage { get; init; }
    }
}
