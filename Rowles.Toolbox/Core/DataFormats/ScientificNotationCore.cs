using System.Globalization;
using System.Text;

namespace Rowles.Toolbox.Core.DataFormats;

public static class ScientificNotationCore
{
    public static string FormatCoefficient(double coeff, int sigFigs)
    {
        string format = $"G{sigFigs}";
        return coeff.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string ToSuperscript(int number)
    {
        string digits = number.ToString();
        StringBuilder sb = new();
        foreach (char c in digits)
        {
            sb.Append(c switch
            {
                '-' => '\u207B',
                '0' => '\u2070',
                '1' => '\u00B9',
                '2' => '\u00B2',
                '3' => '\u00B3',
                '4' => '\u2074',
                '5' => '\u2075',
                '6' => '\u2076',
                '7' => '\u2077',
                '8' => '\u2078',
                '9' => '\u2079',
                _ => c
            });
        }
        return sb.ToString();
    }

    public static string FormatDecimalExpansion(double value)
    {
        if (value == 0) return "0";

        double absValue = Math.Abs(value);
        string sign = value < 0 ? "-" : "";

        if (absValue >= 1e15 || absValue < 1e-15)
        {
            string full = absValue.ToString("R", CultureInfo.InvariantCulture);
            if (full.Contains('E', StringComparison.OrdinalIgnoreCase))
            {
                int eIdx = full.IndexOf('E', StringComparison.OrdinalIgnoreCase);
                string mantissa = full[..eIdx];
                int exp = int.Parse(full[(eIdx + 1)..], CultureInfo.InvariantCulture);
                return sign + ExpandMantissa(mantissa, exp);
            }
            return sign + full;
        }

        string result = absValue.ToString("R", CultureInfo.InvariantCulture);
        return sign + result;
    }

    public static string ExpandMantissa(string mantissa, int exponent)
    {
        bool hasDot = mantissa.Contains('.');
        string intPart;
        string fracPart;
        if (hasDot)
        {
            string[] parts = mantissa.Split('.');
            intPart = parts[0];
            fracPart = parts[1];
        }
        else
        {
            intPart = mantissa;
            fracPart = "";
        }

        string allDigits = intPart + fracPart;
        int currentDecimalPos = intPart.Length;
        int newDecimalPos = currentDecimalPos + exponent;

        if (newDecimalPos >= allDigits.Length)
        {
            return allDigits + new string('0', newDecimalPos - allDigits.Length);
        }
        else if (newDecimalPos <= 0)
        {
            return "0." + new string('0', -newDecimalPos) + allDigits;
        }
        else
        {
            return allDigits[..newDecimalPos] + "." + allDigits[newDecimalPos..];
        }
    }

    public static string? GetSiPrefix(int exponent)
    {
        return exponent switch
        {
            30 => "quetta (Q)",
            27 => "ronna (R)",
            24 => "yotta (Y)",
            21 => "zetta (Z)",
            18 => "exa (E)",
            15 => "peta (P)",
            12 => "tera (T)",
            9 => "giga (G)",
            6 => "mega (M)",
            3 => "kilo (k)",
            2 => "hecto (h)",
            1 => "deca (da)",
            0 => null,
            -1 => "deci (d)",
            -2 => "centi (c)",
            -3 => "milli (m)",
            -6 => "micro (\u03BC)",
            -9 => "nano (n)",
            -12 => "pico (p)",
            -15 => "femto (f)",
            -18 => "atto (a)",
            -21 => "zepto (z)",
            -24 => "yocto (y)",
            -27 => "ronto (r)",
            -30 => "quecto (q)",
            _ => null
        };
    }
}
