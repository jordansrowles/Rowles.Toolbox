using System.Numerics;
using System.Text;

namespace Rowles.Toolbox.Core.DataFormats;

public static class NumberBaseConverterCore
{
    public static BigInteger ParseBigInteger(string text, int fromBase)
    {
        BigInteger result = BigInteger.Zero;
        foreach (char c in text)
        {
            int digit = CharToDigit(c);
            if (digit < 0 || digit >= fromBase)
                throw new FormatException($"Character '{c}' is not valid for base {fromBase}.");
            result = result * fromBase + digit;
        }
        return result;
    }

    public static string ConvertFromBigInteger(BigInteger value, int toBase)
    {
        if (value == 0) return "0";
        StringBuilder sb = new();
        BigInteger current = value;
        while (current > 0)
        {
            int remainder = (int)(current % toBase);
            sb.Insert(0, DigitToChar(remainder));
            current /= toBase;
        }
        return sb.ToString();
    }

    public static int CharToDigit(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'a' && c <= 'z') return c - 'a' + 10;
        if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
        return -1;
    }

    public static char DigitToChar(int digit)
    {
        if (digit < 10) return (char)('0' + digit);
        return (char)('a' + digit - 10);
    }

    public static string GetBaseName(int b) => b switch
    {
        2 => "BIN",
        8 => "OCT",
        10 => "DEC",
        16 => "HEX",
        _ => $"B{b}"
    };

    public static string FormatBaseValue(string value, int b)
    {
        if (b == 2 && value.Length > 4)
        {
            string clean = value.TrimStart('-');
            string prefix = value.StartsWith('-') ? "-" : "";
            int padLen = (4 - clean.Length % 4) % 4;
            clean = new string('0', padLen) + clean;
            StringBuilder sb = new();
            for (int i = 0; i < clean.Length; i++)
            {
                if (i > 0 && i % 4 == 0) sb.Append(' ');
                sb.Append(clean[i]);
            }
            return prefix + sb.ToString();
        }
        if (b == 16 && value.Length > 2)
        {
            string clean = value.TrimStart('-');
            string prefix = value.StartsWith('-') ? "-" : "";
            int padLen = (2 - clean.Length % 2) % 2;
            clean = new string('0', padLen) + clean;
            StringBuilder sb = new();
            for (int i = 0; i < clean.Length; i++)
            {
                if (i > 0 && i % 2 == 0) sb.Append(' ');
                sb.Append(clean[i]);
            }
            return prefix + sb.ToString();
        }
        return value;
    }
}
