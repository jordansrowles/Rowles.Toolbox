using System.Globalization;

namespace Rowles.Toolbox.Core.MathConverters;

public static class ElectricalCalculatorCore
{
    public enum CalcTab { OhmsLaw, Resistor, Capacitance, Inductance }

    public sealed record BandInfo(string Name, string Hex);
    public sealed record MultInfo(string Name, string Hex, double Factor);
    public sealed record TolInfo(string Name, string Hex, double Percent);
    public sealed record TauEntry(double TimeConstants, double ChargePercent, double DischargePercent);

    public static readonly BandInfo[] DigitBands = new BandInfo[]
    {
        new("Black",  "#000000"),
        new("Brown",  "#8B4513"),
        new("Red",    "#FF0000"),
        new("Orange", "#FF8C00"),
        new("Yellow", "#FFD700"),
        new("Green",  "#008000"),
        new("Blue",   "#0000FF"),
        new("Violet", "#8B00FF"),
        new("Grey",   "#808080"),
        new("White",  "#FFFFFF"),
    };

    public static readonly MultInfo[] MultBands = new MultInfo[]
    {
        new("Black",  "#000000", 1),
        new("Brown",  "#8B4513", 10),
        new("Red",    "#FF0000", 100),
        new("Orange", "#FF8C00", 1e3),
        new("Yellow", "#FFD700", 1e4),
        new("Green",  "#008000", 1e5),
        new("Blue",   "#0000FF", 1e6),
        new("Violet", "#8B00FF", 1e7),
        new("Grey",   "#808080", 1e8),
        new("White",  "#FFFFFF", 1e9),
        new("Gold",   "#DAA520", 0.1),
        new("Silver", "#C0C0C0", 0.01),
    };

    public static readonly TolInfo[] TolBands = new TolInfo[]
    {
        new("Brown",  "#8B4513", 1),
        new("Red",    "#FF0000", 2),
        new("Green",  "#008000", 0.5),
        new("Blue",   "#0000FF", 0.25),
        new("Violet", "#8B00FF", 0.1),
        new("Grey",   "#808080", 0.05),
        new("Gold",   "#DAA520", 5),
        new("Silver", "#C0C0C0", 10),
    };

    public static readonly TauEntry[] TauTable = new TauEntry[]
    {
        new(1.0, 63.2, 36.8),
        new(2.0, 86.5, 13.5),
        new(3.0, 95.0, 5.0),
        new(4.0, 98.2, 1.8),
        new(5.0, 99.3, 0.7),
    };

    public static bool TryParseSI(string raw, out double result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        string trimmed = raw.Trim();
        if (double.TryParse(trimmed, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result))
            return true;
        if (trimmed.Length < 2) return false;
        char suffix = trimmed[trimmed.Length - 1];
        string numPart = trimmed.Substring(0, trimmed.Length - 1);
        double multiplier = suffix switch
        {
            'p' => 1e-12,
            'n' => 1e-9,
            'u' => 1e-6,
            'm' => 1e-3,
            'k' or 'K' => 1e3,
            'M' => 1e6,
            'G' => 1e9,
            'T' => 1e12,
            _ => 0
        };
        if (multiplier == 0) return false;
        if (!double.TryParse(numPart, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out double val))
            return false;
        result = val * multiplier;
        return true;
    }

    public static string FormatSI(double value, string unit)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) return "N/A";
        if (value == 0) return "0 " + unit;
        double abs = Math.Abs(value);
        if (abs >= 1e12) return (value / 1e12).ToString("G4", CultureInfo.InvariantCulture) + " T" + unit;
        if (abs >= 1e9)  return (value / 1e9).ToString("G4", CultureInfo.InvariantCulture) + " G" + unit;
        if (abs >= 1e6)  return (value / 1e6).ToString("G4", CultureInfo.InvariantCulture) + " M" + unit;
        if (abs >= 1e3)  return (value / 1e3).ToString("G4", CultureInfo.InvariantCulture) + " k" + unit;
        if (abs >= 1)    return value.ToString("G4", CultureInfo.InvariantCulture) + " " + unit;
        if (abs >= 1e-3) return (value * 1e3).ToString("G4", CultureInfo.InvariantCulture) + " m" + unit;
        if (abs >= 1e-6) return (value * 1e6).ToString("G4", CultureInfo.InvariantCulture) + " \u03BC" + unit;
        if (abs >= 1e-9) return (value * 1e9).ToString("G4", CultureInfo.InvariantCulture) + " n" + unit;
        if (abs >= 1e-12) return (value * 1e12).ToString("G4", CultureInfo.InvariantCulture) + " p" + unit;
        return value.ToString("G4", CultureInfo.InvariantCulture) + " " + unit;
    }

    public static string FormatMult(double f)
    {
        if (f >= 1) return f.ToString("G0", CultureInfo.InvariantCulture);
        return f.ToString("G2", CultureInfo.InvariantCulture);
    }

    public static string TabIcon(CalcTab tab) => tab switch
    {
        CalcTab.OhmsLaw => "ti-bolt",
        CalcTab.Resistor => "ti-plug",
        CalcTab.Capacitance => "ti-battery-charging",
        CalcTab.Inductance => "ti-wave-sine",
        _ => "ti-bolt"
    };

    public static string TabLabel(CalcTab tab) => tab switch
    {
        CalcTab.OhmsLaw => "Ohm's Law",
        CalcTab.Resistor => "Resistors",
        CalcTab.Capacitance => "Capacitance",
        CalcTab.Inductance => "Inductance",
        _ => tab.ToString()
    };

    public static double ComputeSum(List<string> items)
    {
        double total = 0;
        foreach (string s in items)
        {
            if (TryParseSI(s, out double val) && val > 0) total += val;
        }
        return total;
    }

    public static double ComputeReciprocalSum(List<string> items)
    {
        double recip = 0;
        int count = 0;
        foreach (string s in items)
        {
            if (TryParseSI(s, out double val) && val > 0)
            {
                recip += 1.0 / val;
                count++;
            }
        }
        return count >= 2 ? 1.0 / recip : 0;
    }

    public static int ValidItemCount(List<string> items)
    {
        int c = 0;
        foreach (string s in items)
        {
            if (TryParseSI(s, out double val) && val > 0) c++;
        }
        return c;
    }
}
