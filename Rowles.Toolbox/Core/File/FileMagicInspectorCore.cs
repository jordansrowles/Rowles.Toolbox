namespace Rowles.Toolbox.Core.File;

public static class FileMagicInspectorCore
{
    public sealed class FileSignature
    {
        public string Name { get; init; } = "";
        public string Extensions { get; init; } = "";
        public byte[] Bytes { get; init; } = [];
        public int Offset { get; init; }
        public bool[]? Mask { get; init; }
        public string HexDisplay => string.Join(" ", Bytes.Select(b => b.ToString("X2")));
    }

    public static readonly FileSignature[] Signatures =
    [
        new() { Name = "PDF", Extensions = ".pdf", Bytes = [0x25, 0x50, 0x44, 0x46] },
        new() { Name = "PNG", Extensions = ".png", Bytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A] },
        new() { Name = "JPEG", Extensions = ".jpg, .jpeg", Bytes = [0xFF, 0xD8, 0xFF] },
        new() { Name = "GIF", Extensions = ".gif", Bytes = [0x47, 0x49, 0x46, 0x38] },
        new() { Name = "BMP", Extensions = ".bmp", Bytes = [0x42, 0x4D] },
        new() { Name = "WEBP", Extensions = ".webp", Bytes = [0x52, 0x49, 0x46, 0x46] },
        new() { Name = "ICO", Extensions = ".ico", Bytes = [0x00, 0x00, 0x01, 0x00] },
        new() { Name = "PSD", Extensions = ".psd", Bytes = [0x38, 0x42, 0x50, 0x53] },
        new() { Name = "ZIP", Extensions = ".zip", Bytes = [0x50, 0x4B, 0x03, 0x04] },
        new() { Name = "GZIP", Extensions = ".gz, .gzip", Bytes = [0x1F, 0x8B] },
        new() { Name = "7Z", Extensions = ".7z", Bytes = [0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C] },
        new() { Name = "RAR", Extensions = ".rar", Bytes = [0x52, 0x61, 0x72, 0x21] },
        new() { Name = "EXE/DLL (PE)", Extensions = ".exe, .dll", Bytes = [0x4D, 0x5A] },
        new() { Name = "ELF", Extensions = "", Bytes = [0x7F, 0x45, 0x4C, 0x46] },
        new() { Name = "Mach-O (32-bit)", Extensions = "", Bytes = [0xFE, 0xED, 0xFA, 0xCE] },
        new() { Name = "Mach-O (64-bit)", Extensions = "", Bytes = [0xCF, 0xFA, 0xED, 0xFE] },
        new() { Name = "MP3 (ID3)", Extensions = ".mp3", Bytes = [0x49, 0x44, 0x33] },
        new() { Name = "MP3 (frame sync)", Extensions = ".mp3", Bytes = [0xFF, 0xFB] },
        new() { Name = "WAV", Extensions = ".wav", Bytes = [0x52, 0x49, 0x46, 0x46] },
        new() { Name = "AVI", Extensions = ".avi", Bytes = [0x52, 0x49, 0x46, 0x46] },
        new() { Name = "MP4", Extensions = ".mp4, .m4a, .m4v",
                Bytes = [0x66, 0x74, 0x79, 0x70], Offset = 4 },
        new() { Name = "WASM", Extensions = ".wasm", Bytes = [0x00, 0x61, 0x73, 0x6D] },
        new() { Name = "SQLite", Extensions = ".db, .sqlite, .sqlite3",
                Bytes = [0x53, 0x51, 0x4C, 0x69, 0x74, 0x65, 0x20, 0x66, 0x6F, 0x72, 0x6D, 0x61, 0x74] },
    ];

    public static bool MatchesSignature(byte[] fileBytes, FileSignature sig)
    {
        if (fileBytes.Length < sig.Bytes.Length) return false;

        for (int i = 0; i < sig.Bytes.Length; i++)
        {
            if (sig.Mask is not null && !sig.Mask[i]) continue;
            if (fileBytes[i + sig.Offset] != sig.Bytes[i]) return false;
        }

        return true;
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }

    public static (string? DetectedType, string? ExpectedExtensions, bool ExtensionMatches, int MatchLength) DetectFileType(
        byte[] magicBytes, string fileExtension)
    {
        if (magicBytes.Length == 0)
            return (null, null, false, 0);

        foreach (FileSignature sig in Signatures)
        {
            if (MatchesSignature(magicBytes, sig))
            {
                string[] exts = sig.Extensions.Split(", ");
                bool extensionMatches = exts.Any(ext =>
                    string.Equals(ext, fileExtension, StringComparison.OrdinalIgnoreCase));

                if (sig.Name == "ZIP" && (fileExtension is ".docx" or ".xlsx" or ".pptx" or ".jar" or ".apk" or ".odt" or ".ods"))
                {
                    extensionMatches = true;
                }

                return (sig.Name, sig.Extensions, extensionMatches, sig.Bytes.Length);
            }
        }

        return (null, null, false, 0);
    }
}
