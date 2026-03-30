namespace Rowles.Toolbox.Core.Encoding;

public static class Ieee754InspectorCore
{
    public static void LoadBitsFromInt(int raw, int[] bits)
    {
        for (int i = 0; i < 32; i++)
        {
            bits[i] = (raw >> (31 - i)) & 1;
        }
    }

    public static void LoadBitsFromLong(long raw, int[] bits, int count)
    {
        for (int i = 0; i < count; i++)
        {
            bits[i] = (int)((raw >> (count - 1 - i)) & 1);
        }
    }

    public static long BitsToLong(int[] bits)
    {
        long result = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i] == 1)
                result |= (1L << (bits.Length - 1 - i));
        }
        return result;
    }

    public static int BitsToInt(int[] bits)
    {
        int result = 0;
        for (int i = 0; i < 32; i++)
        {
            if (bits[i] == 1)
                result |= (1 << (31 - i));
        }
        return result;
    }

    public static bool IsMantissaZero(int[] bits, int exponentBits, int totalBits)
    {
        for (int i = 1 + exponentBits; i < totalBits; i++)
        {
            if (bits[i] != 0)
                return false;
        }
        return true;
    }

    public static string FormatFloat(float val)
    {
        if (float.IsNaN(val)) return "NaN";
        if (float.IsPositiveInfinity(val)) return "Infinity";
        if (float.IsNegativeInfinity(val)) return "-Infinity";
        if (IsNegativeZero(val)) return "-0";
        return val.ToString("G9", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string FormatDouble(double val)
    {
        if (double.IsNaN(val)) return "NaN";
        if (double.IsPositiveInfinity(val)) return "Infinity";
        if (double.IsNegativeInfinity(val)) return "-Infinity";
        if (IsNegativeZeroDouble(val)) return "-0";
        return val.ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static bool IsNegativeZero(float val) =>
        val == 0f && BitConverter.SingleToInt32Bits(val) != 0;

    public static bool IsNegativeZeroDouble(double val) =>
        val == 0d && BitConverter.DoubleToInt64Bits(val) != 0;

    public static string FormatBitsGrouped(long raw, int count)
    {
        System.Text.StringBuilder sb = new();
        for (int i = count - 1; i >= 0; i--)
        {
            sb.Append((raw >> i) & 1);
            if (i > 0 && i % 4 == 0)
                sb.Append(' ');
        }
        return sb.ToString();
    }

    public static string FormatBitsGrouped(int raw, int count) =>
        FormatBitsGrouped((long)(uint)raw, count);

    public static string GetAllBitsString(int[] bits)
    {
        System.Text.StringBuilder sb = new();
        foreach (int b in bits)
        {
            sb.Append(b);
        }
        return sb.ToString();
    }
}
