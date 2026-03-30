namespace Rowles.Toolbox.Core.MathConverters;

public static class WebUnitConverterCore
{
    public static readonly (string name, int width)[] Breakpoints =
    [
        ("xs", 320),
        ("sm", 640),
        ("md", 768),
        ("lg", 1024),
        ("xl", 1280),
        ("2xl", 1536)
    ];

    public static string Format(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return "—";
        double abs = Math.Abs(value);
        if (abs == 0) return "0";
        if (abs < 0.001) return value.ToString("G6");
        if (abs < 1) return value.ToString("0.####");
        if (abs < 10000) return value.ToString("0.####");
        return value.ToString("N2");
    }
}
