using System.Globalization;

namespace Rowles.Toolbox.Core.MathConverters;

public static class KinematicsSolverCore
{
    public sealed record SolverStep(string Equation, string Substituted, double Value, string Unit);

    public static bool TryParse(string raw, out double result) =>
        double.TryParse(
            raw.Trim(),
            NumberStyles.Float | NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out result);

    public static bool IsKnown(string raw) =>
        !string.IsNullOrWhiteSpace(raw) && TryParse(raw, out _);

    public static string Fmt(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return "\u2014";
        if (value == 0.0) return "0";
        double abs = Math.Abs(value);
        if (abs < 0.0001 || abs >= 1e7)
            return value.ToString("G5", CultureInfo.InvariantCulture);
        return value.ToString("G7", CultureInfo.InvariantCulture);
    }

    public static string FmtDisplay(double? v) =>
        v.HasValue ? Fmt(v.Value) : "\u2014";

    public static string BadgeCss(string raw) => IsKnown(raw)
        ? "inline-block px-1.5 py-0.5 text-xs font-medium rounded bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300"
        : "inline-block px-1.5 py-0.5 text-xs font-medium rounded bg-gray-100 text-gray-400 dark:bg-gray-800 dark:text-gray-500";

    public static string BadgeLabel(string raw) => IsKnown(raw) ? "known" : "?";

    public static string SourceCss(string input, double? solved)
    {
        if (IsKnown(input))
            return "inline-block px-1.5 py-0.5 text-xs font-medium rounded bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300";
        if (solved.HasValue)
            return "inline-block px-1.5 py-0.5 text-xs font-medium rounded bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300";
        return "inline-block px-1.5 py-0.5 text-xs font-medium rounded bg-gray-100 text-gray-400 dark:bg-gray-800 dark:text-gray-500";
    }

    public static string SourceLabel(string input, double? solved)
    {
        if (IsKnown(input))  return "input";
        if (solved.HasValue) return "solved";
        return "\u2014";
    }

    public static string SolvedValueCss(double? v) => v.HasValue
        ? "text-gray-900 dark:text-gray-100"
        : "text-gray-300 dark:text-gray-600";

    public static string PresetHint(string preset) => preset switch
    {
        "freefall"   => "Enter s (height fallen) or t (fall time) to complete the problem.",
        "braking"    => "Enter s (stopping distance) or t (braking time) to complete the problem.",
        "projectile" => "Enter s (horizontal range) \u2014 speed is constant since a = 0.",
        _            => string.Empty
    };

    /// <summary>
    /// Returns the smallest non-negative finite root from a pair of quadratic roots.
    /// Returns <see cref="double.NaN"/> if neither root qualifies.
    /// </summary>
    public static double PickPositiveRoot(double r1, double r2)
    {
        bool r1Ok = r1 >= 0.0 && double.IsFinite(r1);
        bool r2Ok = r2 >= 0.0 && double.IsFinite(r2);
        if (r1Ok && r2Ok) return Math.Min(r1, r2);
        if (r1Ok)         return r1;
        if (r2Ok)         return r2;
        return double.NaN;
    }
}
