using System.Globalization;

namespace Rowles.Toolbox.Core.Encoding;

public static class VarintInspectorCore
{
    public sealed class VarintByteInfo
    {
        public int ByteIndex { get; set; }
        public byte RawValue { get; set; }
        public bool HasContinuation { get; set; }
        public byte SevenBitPayload { get; set; }
        public ulong RunningTotal { get; set; }
    }

    public static List<byte> EncodeVarintBytes(ulong value)
    {
        List<byte> result = new();
        do
        {
            byte b = (byte)(value & 0x7F);
            value >>= 7;
            if (value != 0)
                b |= 0x80;
            result.Add(b);
        }
        while (value != 0);
        return result;
    }

    public static ulong ZigZagEncode(long value) =>
        (ulong)((value << 1) ^ (value >> 63));

    public static long ZigZagDecode(ulong value) =>
        (long)(value >> 1) ^ -((long)(value & 1));

    public static string FormatBinary7(byte value) =>
        Convert.ToString(value & 0x7F, 2).PadLeft(7, '0');

    public static string FormatSavings(int encodingBytes, int baseline)
    {
        int saved = baseline - encodingBytes;
        if (saved == 0)
            return "baseline";
        if (saved > 0)
            return $"-{saved} bytes ({saved * 100 / baseline}%)";
        return $"+{-saved} bytes";
    }

    public static List<VarintByteInfo> BuildByteInfoList(List<byte> bytes)
    {
        List<VarintByteInfo> infos = new();
        ulong running = 0;
        for (int i = 0; i < bytes.Count; i++)
        {
            byte raw = bytes[i];
            bool continuation = (raw & 0x80) != 0;
            byte dataBits = (byte)(raw & 0x7F);
            running |= (ulong)dataBits << (i * 7);

            infos.Add(new VarintByteInfo
            {
                ByteIndex = i,
                RawValue = raw,
                HasContinuation = continuation,
                SevenBitPayload = dataBits,
                RunningTotal = running
            });
        }
        return infos;
    }

    public static (List<byte>? Bytes, string? Error) ParseHexInput(string input)
    {
        string cleaned = input.Trim()
            .Replace(",", " ")
            .Replace("0x", string.Empty, StringComparison.OrdinalIgnoreCase);

        string[] parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return (null, "Enter hex bytes separated by spaces (e.g. AC 02).");

        if (parts.Length > 10)
            return (null, "A varint can be at most 10 bytes for a 64-bit value.");

        List<byte> bytes = new();
        foreach (string part in parts)
        {
            if (!byte.TryParse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b))
                return (null, $"Invalid hex byte: '{part}'.");
            bytes.Add(b);
        }

        for (int i = 0; i < bytes.Count - 1; i++)
        {
            if ((bytes[i] & 0x80) == 0)
                return (null, $"Byte {i} (0x{bytes[i]:X2}) has MSB=0 but is not the last byte. Varint terminates early.");
        }

        if ((bytes[^1] & 0x80) != 0)
            return (null, "Last byte has MSB=1. The varint is incomplete (expecting more bytes).");

        return (bytes, null);
    }

    public static ulong DecodeVarint(List<byte> bytes)
    {
        ulong decoded = 0;
        for (int i = 0; i < bytes.Count; i++)
        {
            ulong payload = (ulong)(bytes[i] & 0x7F);
            decoded |= payload << (i * 7);
        }
        return decoded;
    }

    public static string GetConcatenatedBits(List<VarintByteInfo> byteInfos)
    {
        List<string> groups = new();
        for (int i = byteInfos.Count - 1; i >= 0; i--)
        {
            groups.Add(FormatBinary7(byteInfos[i].SevenBitPayload));
        }
        return string.Join(" ", groups);
    }
}
