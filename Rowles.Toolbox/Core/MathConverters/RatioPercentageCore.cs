namespace Rowles.Toolbox.Core.MathConverters;

public static class RatioPercentageCore
{
    public static string FormatResult(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return "—";
        double abs = Math.Abs(value);
        if (abs == 0) return "0";
        if (abs < 0.01) return value.ToString("G6");
        if (abs < 10000) return value.ToString("0.####");
        return value.ToString("N2");
    }
}
