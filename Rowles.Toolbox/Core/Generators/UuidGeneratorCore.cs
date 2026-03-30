namespace Rowles.Toolbox.Core.Generators;

public static class UuidGeneratorCore
{
    public sealed record UuidResult(string Formatted, DateTimeOffset? Timestamp);

    public static Guid CreateUuidV7()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        long unixMs = now.ToUnixTimeMilliseconds();

        Span<byte> bytes = stackalloc byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);

        // First 48 bits: Unix timestamp in milliseconds (big-endian)
        bytes[0] = (byte)((unixMs >> 40) & 0xFF);
        bytes[1] = (byte)((unixMs >> 32) & 0xFF);
        bytes[2] = (byte)((unixMs >> 24) & 0xFF);
        bytes[3] = (byte)((unixMs >> 16) & 0xFF);
        bytes[4] = (byte)((unixMs >> 8) & 0xFF);
        bytes[5] = (byte)(unixMs & 0xFF);

        // Version nibble: 0111 (7) in bits 48-51
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);

        // Variant bits: 10xx in bits 64-65
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        // Guid constructor expects specific byte ordering on little-endian systems.
        if (BitConverter.IsLittleEndian)
        {
            (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
            (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
            (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
            (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
        }

        return new Guid(bytes);
    }

    public static DateTimeOffset ExtractV7Timestamp(Guid guid)
    {
        string hex = guid.ToString("N");
        long unixMs = Convert.ToInt64(hex[..12], 16);
        return DateTimeOffset.FromUnixTimeMilliseconds(unixMs);
    }

    public static string FormatGuid(Guid guid, string format) => format switch
    {
        "Upper" => guid.ToString("D").ToUpperInvariant(),
        "NoHyphens" => guid.ToString("N"),
        "UpperNoHyphens" => guid.ToString("N").ToUpperInvariant(),
        "Braces" => guid.ToString("B"),
        "Urn" => $"urn:uuid:{guid.ToString("D")}",
        _ => guid.ToString("D")
    };
}
