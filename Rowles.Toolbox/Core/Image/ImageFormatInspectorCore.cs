namespace Rowles.Toolbox.Core.Image;

public static class ImageFormatInspectorCore
{
    public sealed class ImageInfo
    {
        public string FormatName { get; set; } = "Unknown";
        public string MagicBytesHex { get; set; } = "";
        public string SubFormat { get; set; } = "";
        public string MimeType { get; set; } = "application/octet-stream";
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitsPerPixel { get; set; }
        public string ColourType { get; set; } = "";
        public bool HasIccProfile { get; set; }
        public bool IsAnimated { get; set; }
    }

    public static ImageInfo InspectImage(byte[] data)
    {
        ImageInfo result = new();

        if (data.Length < 4)
        {
            result.FormatName = "Unknown";
            result.MagicBytesHex = BytesToHex(data, 0, data.Length);
            return result;
        }

        // JPEG: FF D8 FF
        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            result.FormatName = "JPEG";
            result.MimeType = "image/jpeg";
            result.MagicBytesHex = BytesToHex(data, 0, 3);
            ParseJpeg(data, result);
            return result;
        }

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (data.Length >= 8 &&
            data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
            data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
        {
            result.FormatName = "PNG";
            result.MimeType = "image/png";
            result.MagicBytesHex = BytesToHex(data, 0, 8);
            ParsePng(data, result);
            return result;
        }

        // GIF: 47 49 46 38
        if (data.Length >= 6 &&
            data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
        {
            result.FormatName = "GIF";
            result.MimeType = "image/gif";
            result.MagicBytesHex = BytesToHex(data, 0, 6);
            char version = (char)data[4];
            result.SubFormat = version == '9' ? "GIF89a" : "GIF87a";
            ParseGif(data, result);
            return result;
        }

        // BMP: 42 4D
        if (data.Length >= 2 && data[0] == 0x42 && data[1] == 0x4D)
        {
            result.FormatName = "BMP";
            result.MimeType = "image/bmp";
            result.MagicBytesHex = BytesToHex(data, 0, 2);
            ParseBmp(data, result);
            return result;
        }

        // WebP: RIFF....WEBP
        if (data.Length >= 12 &&
            data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
            data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
        {
            result.FormatName = "WebP";
            result.MimeType = "image/webp";
            result.MagicBytesHex = BytesToHex(data, 0, 4) + " ... " + BytesToHex(data, 8, 4);
            ParseWebP(data, result);
            return result;
        }

        // TIFF little-endian: 49 49 2A 00
        if (data.Length >= 4 &&
            data[0] == 0x49 && data[1] == 0x49 && data[2] == 0x2A && data[3] == 0x00)
        {
            result.FormatName = "TIFF";
            result.MimeType = "image/tiff";
            result.MagicBytesHex = BytesToHex(data, 0, 4);
            result.SubFormat = "Little-endian";
            return result;
        }

        // TIFF big-endian: 4D 4D 00 2A
        if (data.Length >= 4 &&
            data[0] == 0x4D && data[1] == 0x4D && data[2] == 0x00 && data[3] == 0x2A)
        {
            result.FormatName = "TIFF";
            result.MimeType = "image/tiff";
            result.MagicBytesHex = BytesToHex(data, 0, 4);
            result.SubFormat = "Big-endian";
            return result;
        }

        // ICO: 00 00 01 00
        if (data.Length >= 4 &&
            data[0] == 0x00 && data[1] == 0x00 && data[2] == 0x01 && data[3] == 0x00)
        {
            result.FormatName = "ICO";
            result.MimeType = "image/x-icon";
            result.MagicBytesHex = BytesToHex(data, 0, 4);
            ParseIco(data, result);
            return result;
        }

        // AVIF / HEIC: ftyp box
        if (TryParseFtyp(data, result))
        {
            return result;
        }

        result.FormatName = "Unknown";
        result.MimeType = "application/octet-stream";
        result.MagicBytesHex = BytesToHex(data, 0, Math.Min(8, data.Length));
        return result;
    }

    public static string BytesToHex(byte[] data, int offset, int count)
    {
        int end = Math.Min(offset + count, data.Length);
        List<string> parts = new();
        for (int i = offset; i < end; i++)
        {
            parts.Add(data[i].ToString("X2"));
        }
        return string.Join(" ", parts);
    }

    public static bool MatchesAscii(byte[] data, int offset, string text)
    {
        if (offset + text.Length > data.Length) return false;
        for (int i = 0; i < text.Length; i++)
        {
            if (data[offset + i] != (byte)text[i]) return false;
        }
        return true;
    }

    public static int ReadInt32BE(byte[] data, int offset)
    {
        if (offset + 4 > data.Length) return 0;
        return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
    }

    public static int ReadInt32LE(byte[] data, int offset)
    {
        if (offset + 4 > data.Length) return 0;
        return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
    }

    public static string GetChunkType(byte[] data, int offset)
    {
        if (offset + 4 > data.Length) return "";
        return "" + (char)data[offset] + (char)data[offset + 1] + (char)data[offset + 2] + (char)data[offset + 3];
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return bytes + " B";
        if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F2") + " KB";
        return (bytes / (1024.0 * 1024.0)).ToString("F2") + " MB";
    }

    public static string GetAspectRatio(int width, int height)
    {
        if (width <= 0 || height <= 0) return "N/A";
        int gcd = Gcd(width, height);
        int rw = width / gcd;
        int rh = height / gcd;
        if (rw > 50 || rh > 50)
        {
            double ratio = (double)width / height;
            if (Math.Abs(ratio - 16.0 / 9) < 0.02) return "16:9";
            if (Math.Abs(ratio - 4.0 / 3) < 0.02) return "4:3";
            if (Math.Abs(ratio - 3.0 / 2) < 0.02) return "3:2";
            if (Math.Abs(ratio - 21.0 / 9) < 0.02) return "21:9";
            if (Math.Abs(ratio - 1.0) < 0.02) return "1:1";
            return rw + ":" + rh;
        }
        return rw + ":" + rh;
    }

    public static string GetMegapixels(int width, int height)
    {
        if (width <= 0 || height <= 0) return "N/A";
        double mp = (double)width * height / 1_000_000.0;
        return mp.ToString("F2") + " MP";
    }

    public static int Gcd(int a, int b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    private static void ParseJpeg(byte[] data, ImageInfo result)
    {
        int offset = 2;
        while (offset < data.Length - 1)
        {
            if (data[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            byte marker = data[offset + 1];

            if ((marker == 0xC0 || marker == 0xC1 || marker == 0xC2) && offset + 9 < data.Length)
            {
                int precision = data[offset + 4];
                result.Height = (data[offset + 5] << 8) | data[offset + 6];
                result.Width = (data[offset + 7] << 8) | data[offset + 8];
                int components = data[offset + 9];
                result.BitsPerPixel = precision * components;
                result.ColourType = components switch
                {
                    1 => "Grayscale",
                    3 => "YCbCr (RGB)",
                    4 => "CMYK",
                    _ => components + " components"
                };
                if (marker == 0xC2)
                {
                    result.SubFormat = "Progressive JPEG";
                }
                break;
            }

            if (marker == 0xE2 && offset + 4 < data.Length)
            {
                int segLen = (data[offset + 2] << 8) | data[offset + 3];
                if (offset + 16 < data.Length)
                {
                    if (MatchesAscii(data, offset + 4, "ICC_PROFILE"))
                    {
                        result.HasIccProfile = true;
                    }
                }
                offset += 2 + segLen;
                continue;
            }

            if (marker == 0xD8 || marker == 0xD9)
            {
                offset += 2;
            }
            else if (offset + 3 < data.Length)
            {
                int segLen = (data[offset + 2] << 8) | data[offset + 3];
                offset += 2 + segLen;
            }
            else
            {
                break;
            }
        }

        if (!result.HasIccProfile)
        {
            ScanJpegForIcc(data, result);
        }
    }

    private static void ScanJpegForIcc(byte[] data, ImageInfo result)
    {
        int offset = 2;
        while (offset < data.Length - 1)
        {
            if (data[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            byte marker = data[offset + 1];
            if (marker == 0xE2 && offset + 16 < data.Length)
            {
                if (MatchesAscii(data, offset + 4, "ICC_PROFILE"))
                {
                    result.HasIccProfile = true;
                    return;
                }
            }

            if (marker == 0xD8 || marker == 0xD9)
            {
                offset += 2;
            }
            else if (offset + 3 < data.Length)
            {
                int segLen = (data[offset + 2] << 8) | data[offset + 3];
                offset += 2 + segLen;
            }
            else
            {
                break;
            }
        }
    }

    private static void ParsePng(byte[] data, ImageInfo result)
    {
        if (data.Length >= 29)
        {
            result.Width = ReadInt32BE(data, 16);
            result.Height = ReadInt32BE(data, 20);
            int bitDepth = data[24];
            int colourType = data[25];

            result.ColourType = colourType switch
            {
                0 => "Grayscale",
                2 => "RGB",
                3 => "Indexed",
                4 => "Grayscale + Alpha",
                6 => "RGBA",
                _ => "Type " + colourType
            };

            int channels = colourType switch
            {
                0 => 1,
                2 => 3,
                3 => 1,
                4 => 2,
                6 => 4,
                _ => 1
            };

            result.BitsPerPixel = bitDepth * channels;
        }

        ScanPngChunks(data, result);
    }

    private static void ScanPngChunks(byte[] data, ImageInfo result)
    {
        int offset = 8;
        while (offset + 12 <= data.Length)
        {
            int chunkLen = ReadInt32BE(data, offset);
            if (chunkLen < 0) break;

            string chunkType = GetChunkType(data, offset + 4);

            if (chunkType == "iCCP")
            {
                result.HasIccProfile = true;
            }
            else if (chunkType == "acTL")
            {
                result.IsAnimated = true;
            }

            offset += 12 + chunkLen;
        }
    }

    private static void ParseGif(byte[] data, ImageInfo result)
    {
        if (data.Length >= 10)
        {
            result.Width = data[6] | (data[7] << 8);
            result.Height = data[8] | (data[9] << 8);

            byte packed = data[10];
            bool hasGct = (packed & 0x80) != 0;
            int colourResolution = ((packed >> 4) & 0x07) + 1;
            result.BitsPerPixel = colourResolution;
            result.ColourType = hasGct ? "Indexed (Global Colour Table)" : "Indexed";

            int imageCount = 0;
            int gctSize = 0;
            if (hasGct)
            {
                gctSize = 3 * (1 << ((packed & 0x07) + 1));
            }

            int offset = 13 + gctSize;
            while (offset < data.Length)
            {
                byte b = data[offset];
                if (b == 0x2C)
                {
                    imageCount++;
                    if (imageCount > 1)
                    {
                        result.IsAnimated = true;
                        break;
                    }
                    if (offset + 10 > data.Length) break;
                    byte lpacked = data[offset + 9];
                    bool hasLct = (lpacked & 0x80) != 0;
                    int lctSize = hasLct ? 3 * (1 << ((lpacked & 0x07) + 1)) : 0;
                    offset += 10 + lctSize;
                    if (offset >= data.Length) break;
                    offset++;
                    offset = SkipGifSubBlocks(data, offset);
                }
                else if (b == 0x21)
                {
                    if (offset + 2 >= data.Length) break;
                    offset += 2;
                    offset = SkipGifSubBlocks(data, offset);
                }
                else if (b == 0x3B)
                {
                    break;
                }
                else
                {
                    offset++;
                }
            }
        }
    }

    private static int SkipGifSubBlocks(byte[] data, int offset)
    {
        while (offset < data.Length)
        {
            int blockSize = data[offset];
            if (blockSize == 0)
            {
                return offset + 1;
            }
            offset += 1 + blockSize;
        }
        return offset;
    }

    private static void ParseBmp(byte[] data, ImageInfo result)
    {
        if (data.Length >= 30)
        {
            int width = ReadInt32LE(data, 18);
            int height = ReadInt32LE(data, 22);
            result.Width = width;
            result.Height = Math.Abs(height);
            result.BitsPerPixel = data[28] | (data[29] << 8);

            result.ColourType = result.BitsPerPixel switch
            {
                1 => "Monochrome",
                4 => "16-colour Indexed",
                8 => "256-colour Indexed",
                16 => "RGB (16-bit)",
                24 => "RGB",
                32 => "RGBA",
                _ => result.BitsPerPixel + "-bit"
            };

            if (height < 0)
            {
                result.SubFormat = "Top-down DIB";
            }
        }
    }

    private static void ParseWebP(byte[] data, ImageInfo result)
    {
        if (data.Length < 16) return;

        string fourcc = GetChunkType(data, 12);

        if (fourcc == "VP8 " && data.Length >= 30)
        {
            result.SubFormat = "VP8 (Lossy)";
            int vpOffset = 20;
            for (int i = vpOffset; i < Math.Min(data.Length - 6, vpOffset + 20); i++)
            {
                if (data[i] == 0x9D && data[i + 1] == 0x01 && data[i + 2] == 0x2A)
                {
                    result.Width = (data[i + 3] | (data[i + 4] << 8)) & 0x3FFF;
                    result.Height = (data[i + 5] | (data[i + 6] << 8)) & 0x3FFF;
                    result.BitsPerPixel = 24;
                    result.ColourType = "YUV (RGB)";
                    return;
                }
            }
        }
        else if (fourcc == "VP8L" && data.Length >= 25)
        {
            result.SubFormat = "VP8L (Lossless)";
            int vpOffset = 21;
            if (vpOffset + 4 <= data.Length && data[vpOffset] == 0x2F)
            {
                uint bits = (uint)(data[vpOffset + 1] | (data[vpOffset + 2] << 8) |
                            (data[vpOffset + 3] << 16) | (data[vpOffset + 4] << 24));
                result.Width = (int)(bits & 0x3FFF) + 1;
                result.Height = (int)((bits >> 14) & 0x3FFF) + 1;
                bool hasAlpha = ((bits >> 28) & 1) == 1;
                result.BitsPerPixel = hasAlpha ? 32 : 24;
                result.ColourType = hasAlpha ? "RGBA" : "RGB";
            }
        }
        else if (fourcc == "VP8X" && data.Length >= 30)
        {
            result.SubFormat = "VP8X (Extended)";
            int vpOffset = 20;
            if (vpOffset + 10 <= data.Length)
            {
                byte flags = data[vpOffset];
                bool hasAlpha = (flags & 0x10) != 0;
                bool hasAnim = (flags & 0x02) != 0;

                result.Width = (data[vpOffset + 4] | (data[vpOffset + 5] << 8) | (data[vpOffset + 6] << 16)) + 1;
                result.Height = (data[vpOffset + 7] | (data[vpOffset + 8] << 8) | (data[vpOffset + 9] << 16)) + 1;
                result.BitsPerPixel = hasAlpha ? 32 : 24;
                result.ColourType = hasAlpha ? "RGBA" : "RGB";
                result.IsAnimated = hasAnim;
            }
        }
    }

    private static void ParseIco(byte[] data, ImageInfo result)
    {
        if (data.Length >= 6)
        {
            int imageCount = data[4] | (data[5] << 8);
            result.SubFormat = imageCount + " image" + (imageCount != 1 ? "s" : "");

            if (data.Length >= 22)
            {
                int w = data[6];
                int h = data[7];
                result.Width = w == 0 ? 256 : w;
                result.Height = h == 0 ? 256 : h;
                int bpp = data[12] | (data[13] << 8);
                if (bpp > 0)
                {
                    result.BitsPerPixel = bpp;
                }
                result.ColourType = "Indexed/RGB (ICO)";
            }
        }
    }

    private static bool TryParseFtyp(byte[] data, ImageInfo result)
    {
        if (data.Length < 12) return false;

        if (data[4] != 0x66 || data[5] != 0x74 || data[6] != 0x79 || data[7] != 0x70)
            return false;

        string brand = "" + (char)data[8] + (char)data[9] + (char)data[10] + (char)data[11];

        if (brand == "avif" || brand == "avis")
        {
            result.FormatName = "AVIF";
            result.MimeType = "image/avif";
            result.MagicBytesHex = BytesToHex(data, 4, 4) + " " + BytesToHex(data, 8, 4);
            result.SubFormat = "Brand: " + brand.Trim();
            if (brand == "avis")
            {
                result.IsAnimated = true;
            }
            return true;
        }

        if (brand == "heic" || brand == "heix" || brand == "hevc" || brand == "mif1")
        {
            result.FormatName = "HEIC";
            result.MimeType = "image/heic";
            result.MagicBytesHex = BytesToHex(data, 4, 4) + " " + BytesToHex(data, 8, 4);
            result.SubFormat = "Brand: " + brand.Trim();
            return true;
        }

        return false;
    }
}
