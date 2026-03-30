namespace Rowles.Toolbox.Core.Image;

public static class ExifInspectorCore
{
    public sealed class ExifRawTag
    {
        public ushort TagId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string TypeName { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
    }

    public sealed class ExifParseResult
    {
        public string? ErrorMessage { get; init; }
        public Dictionary<string, string> CameraInfo { get; init; } = new();
        public Dictionary<string, string> GpsInfo { get; init; } = new();
        public List<ExifRawTag> RawTags { get; init; } = [];
    }

    public static readonly Dictionary<ushort, string> KnownTags = new()
    {
        [0x010F] = "Make",
        [0x0110] = "Model",
        [0x0112] = "Orientation",
        [0x011A] = "XResolution",
        [0x011B] = "YResolution",
        [0x0131] = "Software",
        [0x0132] = "DateTime",
        [0x013B] = "Artist",
        [0x8769] = "ExifIFDPointer",
        [0x8825] = "GPSInfoIFDPointer",
        [0x9000] = "ExifVersion",
        [0x9003] = "DateTimeOriginal",
        [0x9004] = "DateTimeDigitized",
        [0x829A] = "ExposureTime",
        [0x829D] = "FNumber",
        [0x8827] = "ISOSpeedRatings",
        [0x920A] = "FocalLength",
        [0xA405] = "FocalLengthIn35mmFilm",
        [0xA002] = "PixelXDimension",
        [0xA003] = "PixelYDimension",
        [0xA001] = "ColorSpace",
        [0x9209] = "Flash",
        [0xA434] = "LensModel"
    };

    public static readonly Dictionary<ushort, string> GpsTags = new()
    {
        [0x0001] = "GPSLatitudeRef",
        [0x0002] = "GPSLatitude",
        [0x0003] = "GPSLongitudeRef",
        [0x0004] = "GPSLongitude",
        [0x0005] = "GPSAltitudeRef",
        [0x0006] = "GPSAltitude"
    };

    public static ExifParseResult ParseExifData(ReadOnlySpan<byte> data)
    {
        Dictionary<string, string> cameraInfo = new();
        Dictionary<string, string> gpsInfo = new();
        List<ExifRawTag> rawTags = [];

        if (data.Length < 4)
        {
            return new ExifParseResult { ErrorMessage = "File too small to contain EXIF data." };
        }

        if (data[0] != 0xFF || data[1] != 0xD8)
        {
            return new ExifParseResult { ErrorMessage = "Not a valid JPEG file." };
        }

        int offset = 2;
        while (offset < data.Length - 4)
        {
            if (data[offset] != 0xFF)
            {
                offset++;
                continue;
            }

            byte marker = data[offset + 1];
            if (marker == 0xE1)
            {
                int segmentLength = (data[offset + 2] << 8) | data[offset + 3];
                int segmentStart = offset + 4;

                if (segmentStart + 6 <= data.Length &&
                    data[segmentStart] == 0x45 && data[segmentStart + 1] == 0x78 &&
                    data[segmentStart + 2] == 0x69 && data[segmentStart + 3] == 0x66 &&
                    data[segmentStart + 4] == 0x00 && data[segmentStart + 5] == 0x00)
                {
                    int tiffStart = segmentStart + 6;
                    ReadOnlySpan<byte> tiffData = data[tiffStart..Math.Min(data.Length, offset + 2 + segmentLength)];
                    ParseTiff(tiffData, cameraInfo, gpsInfo, rawTags);
                    return new ExifParseResult
                    {
                        CameraInfo = cameraInfo,
                        GpsInfo = gpsInfo,
                        RawTags = rawTags
                    };
                }
            }

            if (offset + 3 < data.Length)
            {
                int length = (data[offset + 2] << 8) | data[offset + 3];
                offset += 2 + length;
            }
            else
            {
                break;
            }
        }

        return new ExifParseResult { ErrorMessage = "No EXIF data found in this file." };
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    public static int GetTypeSize(ushort typeId) => typeId switch
    {
        1 => 1,
        2 => 1,
        3 => 2,
        4 => 4,
        5 => 8,
        6 => 1,
        7 => 1,
        8 => 2,
        9 => 4,
        10 => 8,
        11 => 4,
        12 => 8,
        _ => 1
    };

    public static string GetTypeName(ushort typeId) => typeId switch
    {
        1 => "BYTE",
        2 => "ASCII",
        3 => "SHORT",
        4 => "LONG",
        5 => "RATIONAL",
        6 => "SBYTE",
        7 => "UNDEFINED",
        8 => "SSHORT",
        9 => "SLONG",
        10 => "SRATIONAL",
        11 => "FLOAT",
        12 => "DOUBLE",
        _ => $"UNKNOWN({typeId})"
    };

    public static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset, bool littleEndian)
    {
        if (offset + 2 > data.Length) return 0;
        return littleEndian
            ? (ushort)(data[offset] | (data[offset + 1] << 8))
            : (ushort)((data[offset] << 8) | data[offset + 1]);
    }

    public static uint ReadUInt32(ReadOnlySpan<byte> data, int offset, bool littleEndian)
    {
        if (offset + 4 > data.Length) return 0;
        return littleEndian
            ? (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24))
            : (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
    }

    public static string ReadTagValue(ReadOnlySpan<byte> tiff, ushort typeId, uint count, ReadOnlySpan<byte> valueData, uint valueOffset, bool littleEndian)
    {
        if (valueData.IsEmpty) return "(data out of range)";

        return typeId switch
        {
            2 => ReadAscii(tiff, count, valueData, valueOffset),
            3 => count == 1 ? ReadUInt16(valueData, 0, littleEndian).ToString() : ReadMultipleShorts(valueData, count, littleEndian),
            4 => count == 1 ? ReadUInt32(valueData, 0, littleEndian).ToString() : ReadMultipleLongs(valueData, count, littleEndian),
            5 => ReadRationals(tiff, count, valueOffset, littleEndian),
            7 => $"({count} bytes)",
            10 => ReadSignedRationals(tiff, count, valueOffset, littleEndian),
            _ => $"type={typeId}, count={count}"
        };
    }

    public static string ReadAscii(ReadOnlySpan<byte> tiff, uint count, ReadOnlySpan<byte> valueData, uint valueOffset)
    {
        ReadOnlySpan<byte> source = count <= 4 ? valueData : (valueOffset + count <= (uint)tiff.Length ? tiff.Slice((int)valueOffset, (int)count) : valueData);
        int len = (int)Math.Min(count, (uint)source.Length);
        while (len > 0 && source[len - 1] == 0) len--;
        return System.Text.Encoding.ASCII.GetString(source[..len]);
    }

    public static string ReadMultipleShorts(ReadOnlySpan<byte> data, uint count, bool littleEndian)
    {
        List<string> values = [];
        for (int i = 0; i < (int)count && i * 2 + 2 <= data.Length; i++)
        {
            values.Add(ReadUInt16(data, i * 2, littleEndian).ToString());
        }
        return string.Join(", ", values);
    }

    public static string ReadMultipleLongs(ReadOnlySpan<byte> data, uint count, bool littleEndian)
    {
        List<string> values = [];
        for (int i = 0; i < (int)count && i * 4 + 4 <= data.Length; i++)
        {
            values.Add(ReadUInt32(data, i * 4, littleEndian).ToString());
        }
        return string.Join(", ", values);
    }

    public static string ReadRationals(ReadOnlySpan<byte> tiff, uint count, uint valueOffset, bool littleEndian)
    {
        List<string> values = [];
        for (int i = 0; i < (int)count; i++)
        {
            int off = (int)valueOffset + i * 8;
            if (off + 8 > tiff.Length) break;
            uint numerator = ReadUInt32(tiff, off, littleEndian);
            uint denominator = ReadUInt32(tiff, off + 4, littleEndian);
            if (denominator == 0)
            {
                values.Add($"{numerator}/0");
            }
            else
            {
                double val = (double)numerator / denominator;
                values.Add(val == Math.Floor(val) ? $"{(int)val}" : $"{val:F2}");
            }
        }
        return string.Join(", ", values);
    }

    public static string ReadSignedRationals(ReadOnlySpan<byte> tiff, uint count, uint valueOffset, bool littleEndian)
    {
        List<string> values = [];
        for (int i = 0; i < (int)count; i++)
        {
            int off = (int)valueOffset + i * 8;
            if (off + 8 > tiff.Length) break;
            int numerator = (int)ReadUInt32(tiff, off, littleEndian);
            int denominator = (int)ReadUInt32(tiff, off + 4, littleEndian);
            if (denominator == 0)
            {
                values.Add($"{numerator}/0");
            }
            else
            {
                double val = (double)numerator / denominator;
                values.Add(val == Math.Floor(val) ? $"{(int)val}" : $"{val:F2}");
            }
        }
        return string.Join(", ", values);
    }

    private static void ParseTiff(ReadOnlySpan<byte> tiff, Dictionary<string, string> cameraInfo, Dictionary<string, string> gpsInfo, List<ExifRawTag> rawTags)
    {
        if (tiff.Length < 8) return;

        bool littleEndian = tiff[0] == 0x49 && tiff[1] == 0x49;

        ushort magic = ReadUInt16(tiff, 2, littleEndian);
        if (magic != 42) return;

        int ifdOffset = (int)ReadUInt32(tiff, 4, littleEndian);
        ParseIfd(tiff, ifdOffset, littleEndian, isGps: false, cameraInfo, gpsInfo, rawTags);
    }

    private static void ParseIfd(ReadOnlySpan<byte> tiff, int ifdOffset, bool littleEndian, bool isGps,
        Dictionary<string, string> cameraInfo, Dictionary<string, string> gpsInfo, List<ExifRawTag> rawTags)
    {
        if (ifdOffset + 2 > tiff.Length) return;

        ushort entryCount = ReadUInt16(tiff, ifdOffset, littleEndian);
        int pos = ifdOffset + 2;

        for (int i = 0; i < entryCount && pos + 12 <= tiff.Length; i++)
        {
            ushort tagId = ReadUInt16(tiff, pos, littleEndian);
            ushort typeId = ReadUInt16(tiff, pos + 2, littleEndian);
            uint count = ReadUInt32(tiff, pos + 4, littleEndian);
            uint valueOffset = ReadUInt32(tiff, pos + 8, littleEndian);

            int dataSize = GetTypeSize(typeId) * (int)count;
            ReadOnlySpan<byte> valueData = dataSize <= 4
                ? tiff.Slice(pos + 8, Math.Min(4, tiff.Length - pos - 8))
                : (valueOffset + dataSize <= tiff.Length ? tiff.Slice((int)valueOffset, dataSize) : ReadOnlySpan<byte>.Empty);

            if (tagId == 0x8769 && !isGps)
            {
                int exifIfdOffset = (int)valueOffset;
                ParseIfd(tiff, exifIfdOffset, littleEndian, isGps: false, cameraInfo, gpsInfo, rawTags);
                pos += 12;
                continue;
            }

            if (tagId == 0x8825 && !isGps)
            {
                int gpsIfdOffset = (int)valueOffset;
                ParseIfd(tiff, gpsIfdOffset, littleEndian, isGps: true, cameraInfo, gpsInfo, rawTags);
                pos += 12;
                continue;
            }

            string value = ReadTagValue(tiff, typeId, count, valueData, valueOffset, littleEndian);
            string tagName = isGps
                ? (GpsTags.TryGetValue(tagId, out string? gn) ? gn : $"GPS_0x{tagId:X4}")
                : (KnownTags.TryGetValue(tagId, out string? tn) ? tn : $"Tag_0x{tagId:X4}");

            if (isGps)
            {
                gpsInfo[tagName] = value;
            }
            else
            {
                AddCameraInfo(tagId, value, cameraInfo);
            }

            rawTags.Add(new ExifRawTag
            {
                TagId = tagId,
                Name = tagName,
                TypeName = GetTypeName(typeId),
                Value = value
            });

            pos += 12;
        }
    }

    private static void AddCameraInfo(ushort tagId, string value, Dictionary<string, string> cameraInfo)
    {
        string? displayName = tagId switch
        {
            0x010F => "Camera Make",
            0x0110 => "Camera Model",
            0x0132 => "Date/Time",
            0x9003 => "Date Taken",
            0x829A => "Exposure Time",
            0x829D => "F-Number",
            0x8827 => "ISO",
            0x920A => "Focal Length",
            0xA405 => "Focal Length (35mm)",
            0xA002 => "Image Width",
            0xA003 => "Image Height",
            0x0131 => "Software",
            0x0112 => "Orientation",
            0xA434 => "Lens Model",
            0x9209 => "Flash",
            _ => null
        };

        if (displayName is not null)
        {
            cameraInfo[displayName] = value;
        }
    }
}
