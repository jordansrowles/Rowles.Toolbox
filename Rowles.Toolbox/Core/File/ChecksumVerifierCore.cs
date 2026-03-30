using System.Security.Cryptography;

#pragma warning disable CA1416 // Validate platform compatibility

namespace Rowles.Toolbox.Core.File;

public static class ChecksumVerifierCore
{
    public enum Mode { File, Text }

    public static Dictionary<string, string> ComputeHashes(byte[] data)
    {
        Dictionary<string, string> hashes = new();
        hashes["MD5"] = TryHash(() => MD5.HashData(data));
        hashes["SHA-1"] = TryHash(() => SHA1.HashData(data));
        hashes["SHA-256"] = TryHash(() => SHA256.HashData(data));
        hashes["SHA-512"] = TryHash(() => SHA512.HashData(data));
        hashes["CRC32"] = ComputeCrc32(data).ToString("x8");
        return hashes;
    }

    public static string TryHash(Func<byte[]> hashFunc)
    {
        try
        {
            return Convert.ToHexStringLower(hashFunc());
        }
        catch (CryptographicException)
        {
            return "(not available in WASM)";
        }
    }

    public static uint ComputeCrc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in data)
        {
            crc ^= b;
            for (int bit = 0; bit < 8; bit++)
            {
                if ((crc & 1) != 0)
                    crc = (crc >> 1) ^ 0xEDB88320;
                else
                    crc >>= 1;
            }
        }
        return ~crc;
    }

    public static string DetectAlgorithm(string hash)
    {
        string trimmed = hash.Trim();
        return trimmed.Length switch
        {
            8 => "CRC32",
            32 => "MD5",
            40 => "SHA-1",
            64 => "SHA-256",
            128 => "SHA-512",
            _ => "SHA-256"
        };
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
