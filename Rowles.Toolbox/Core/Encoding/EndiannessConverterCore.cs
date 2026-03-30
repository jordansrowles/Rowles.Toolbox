namespace Rowles.Toolbox.Core.Encoding;

public static class EndiannessConverterCore
{
    public static ulong ParseInput(string input)
    {
        string trimmed = input.Trim().Replace("_", "");

        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return ulong.Parse(trimmed[2..], System.Globalization.NumberStyles.HexNumber);
        }

        if (trimmed.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.ToUInt64(trimmed[2..], 2);
        }

        return ulong.Parse(trimmed);
    }

    public static void ValidateWidth(ulong value, int bitWidth)
    {
        ulong maxValue = bitWidth switch
        {
            16 => ushort.MaxValue,
            32 => uint.MaxValue,
            _ => ulong.MaxValue
        };

        if (value > maxValue)
        {
            throw new OverflowException(
                $"Value {value:N0} exceeds {bitWidth}-bit range (max {maxValue:N0}). Select a wider bit width.");
        }
    }

    public static byte[] ToBigEndianBytes(ulong value, int bitWidth)
    {
        int byteCount = bitWidth / 8;
        byte[] bytes = new byte[byteCount];
        for (int i = 0; i < byteCount; i++)
        {
            int shift = (bitWidth - 8) - (i * 8);
            bytes[i] = (byte)(value >> shift);
        }
        return bytes;
    }

    public static byte[] ToLittleEndianBytes(byte[] bigEndianBytes)
    {
        byte[] result = new byte[bigEndianBytes.Length];
        Array.Copy(bigEndianBytes, result, bigEndianBytes.Length);
        Array.Reverse(result);
        return result;
    }

    public static ulong ComputeSwappedValue(byte[] littleEndianBytes)
    {
        ulong result = 0;
        for (int i = 0; i < littleEndianBytes.Length; i++)
        {
            result = (result << 8) | littleEndianBytes[i];
        }
        return result;
    }

    public static string FormatHexValue(ulong value, int bitWidth)
    {
        int hexDigits = bitWidth / 4;
        return "0x" + value.ToString("X" + hexDigits.ToString());
    }

    public static string FormatBinaryGrouped(byte[] bytes)
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(Convert.ToString(bytes[i], 2).PadLeft(8, '0'));
        }
        return sb.ToString();
    }

    public static string FormatByteString(byte[] bytes) =>
        string.Join(" ", Array.ConvertAll(bytes, b => b.ToString("X2")));
}
