using System.Text;

namespace Rowles.Toolbox.Core.Encoding;

public static class TwosComplementCore
{
    public static ulong MaskToWidth(ulong value, int width)
    {
        if (width == 64) return value;
        return value & ((1UL << width) - 1);
    }

    public static long SignedFromBits(ulong pattern, int width)
    {
        if (width == 64)
            return (long)pattern;

        ulong signBit = 1UL << (width - 1);
        if ((pattern & signBit) != 0)
        {
            ulong signExtended = pattern | (~0UL << width);
            return (long)signExtended;
        }
        return (long)pattern;
    }

    public static long SignedMinVal(int width)
    {
        if (width == 64) return long.MinValue;
        return -(1L << (width - 1));
    }

    public static long SignedMaxVal(int width)
    {
        if (width == 64) return long.MaxValue;
        return (1L << (width - 1)) - 1;
    }

    public static ulong UnsignedMaxVal(int width)
    {
        if (width == 64) return ulong.MaxValue;
        return (1UL << width) - 1;
    }

    public static string PadBinary(ulong pattern, int bitWidth)
    {
        StringBuilder sb = new();
        for (int i = bitWidth - 1; i >= 0; i--)
        {
            sb.Append(((pattern >> i) & 1) == 1 ? '1' : '0');
        }
        return sb.ToString();
    }

    public static string FormatBinaryGrouped(ulong pattern, int bitWidth)
    {
        string raw = PadBinary(pattern, bitWidth);
        StringBuilder sb = new();
        for (int i = 0; i < raw.Length; i++)
        {
            if (i > 0 && i % 4 == 0)
                sb.Append(' ');
            sb.Append(raw[i]);
        }
        return sb.ToString();
    }

    public static string ComputeHex(ulong pattern, int bitWidth)
    {
        int hexDigits = bitWidth / 4;
        return pattern.ToString("X" + hexDigits);
    }

    public static string ComputeOctal(ulong pattern)
    {
        if (pattern == 0) return "0";
        StringBuilder sb = new();
        ulong val = pattern;
        while (val > 0)
        {
            sb.Insert(0, (char)('0' + (int)(val & 7)));
            val >>= 3;
        }
        return sb.ToString();
    }

    public static ulong ComputeNegationPattern(ulong pattern, int bitWidth)
    {
        ulong inverted = MaskToWidth(~pattern, bitWidth);
        return MaskToWidth(inverted + 1, bitWidth);
    }

    public static ulong ComputeNotPattern(ulong pattern, int bitWidth) =>
        MaskToWidth(~pattern, bitWidth);

    public static string AbsoluteValueDecimal(ulong pattern, int bitWidth)
    {
        long signed = SignedFromBits(pattern, bitWidth);
        if (signed == long.MinValue)
            return ((ulong)long.MaxValue + 1UL).ToString();
        return Math.Abs(signed).ToString();
    }
}
