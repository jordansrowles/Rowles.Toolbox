using System.Security.Cryptography;

namespace Rowles.Toolbox.Core.Encoding;

public static class HashGeneratorCore
{
    public static uint ComputeCrc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }
        return ~crc;
    }

    public static string FormatHash(byte[] hash, bool uppercase) =>
        uppercase ? Convert.ToHexString(hash) : Convert.ToHexString(hash).ToLowerInvariant();

    public static string TryHash(Func<byte[]> hashFunc, bool uppercase)
    {
        try
        {
            return FormatHash(hashFunc(), uppercase);
        }
        catch (CryptographicException)
        {
            return "(not available in WASM)";
        }
    }

#pragma warning disable CA5350, CA5351, CA1416
    public static (string Crc32, string Md5, string Sha1, string Sha256, string Sha384, string Sha512) ComputeAllHashes(byte[] data, bool uppercase)
    {
        string crc32 = ComputeCrc32(data).ToString(uppercase ? "X8" : "x8");
        string md5 = TryHash(() => MD5.HashData(data), uppercase);
        string sha1 = TryHash(() => SHA1.HashData(data), uppercase);
        string sha256 = TryHash(() => SHA256.HashData(data), uppercase);
        string sha384 = TryHash(() => SHA384.HashData(data), uppercase);
        string sha512 = TryHash(() => SHA512.HashData(data), uppercase);
        return (crc32, md5, sha1, sha256, sha384, sha512);
    }
#pragma warning restore CA5350, CA5351, CA1416

    public static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1_048_576 => $"{bytes / 1024.0:F1} KB",
        < 1_073_741_824 => $"{bytes / 1_048_576.0:F1} MB",
        _ => $"{bytes / 1_073_741_824.0:F2} GB"
    };
}
