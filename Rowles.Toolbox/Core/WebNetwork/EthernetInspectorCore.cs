namespace Rowles.Toolbox.Core.WebNetwork;

public static class EthernetInspectorCore
{
    public static readonly Dictionary<string, string> OuiVendors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["00000C"] = "Cisco Systems",
        ["000C29"] = "VMware",
        ["005056"] = "VMware",
        ["00155D"] = "Microsoft (Hyper-V)",
        ["0050F2"] = "Microsoft",
        ["001B21"] = "Intel Corporate",
        ["3C22FB"] = "Apple",
        ["A483E7"] = "Apple",
        ["080027"] = "Oracle (VirtualBox)",
        ["001C42"] = "Parallels",
        ["B827EB"] = "Raspberry Pi Foundation",
        ["DCA632"] = "Raspberry Pi Trading",
        ["D4BED9"] = "Dell",
        ["002590"] = "Super Micro Computer",
        ["F0DEF1"] = "Google",
    };

    public static readonly Dictionary<int, string> EtherTypes = new()
    {
        [0x0800] = "IPv4",
        [0x0806] = "ARP",
        [0x8100] = "802.1Q VLAN",
        [0x86DD] = "IPv6",
        [0x8863] = "PPPoE Discovery",
        [0x8864] = "PPPoE Session",
        [0x0842] = "Wake-on-LAN",
        [0x88CC] = "LLDP",
        [0x88A8] = "802.1ad (Q-in-Q)",
        [0x8847] = "MPLS Unicast",
        [0x8848] = "MPLS Multicast",
        [0x88F7] = "PTP (IEEE 1588)",
    };

    public static (ParsedFrame? Frame, List<HexSpan>? HexSpans, string? Error) ParseFrame(
        string hexInput, bool includeFcs)
    {
        string cleaned = CleanHexInput(hexInput);
        if (string.IsNullOrEmpty(cleaned))
            return (null, null, "Please enter hex bytes of an Ethernet II frame.");

        if (cleaned.Length % 2 != 0)
            return (null, null, "Hex input must contain an even number of hex characters.");

        byte[] bytes;
        try
        {
            bytes = HexToBytes(cleaned);
        }
        catch
        {
            return (null, null, "Invalid hex characters detected.");
        }

        if (bytes.Length < 14)
            return (null, null, "Frame too short. Minimum 14 bytes required (6 dst + 6 src + 2 EtherType).");

        string dstMac = FormatMac(bytes, 0);
        string srcMac = FormatMac(bytes, 6);

        bool isBroadcast = bytes[0] == 0xFF && bytes[1] == 0xFF && bytes[2] == 0xFF
                           && bytes[3] == 0xFF && bytes[4] == 0xFF && bytes[5] == 0xFF;
        bool isMulticast = !isBroadcast && (bytes[0] & 0x01) != 0;

        string dstOui = $"{bytes[0]:X2}{bytes[1]:X2}{bytes[2]:X2}";
        string srcOui = $"{bytes[6]:X2}{bytes[7]:X2}{bytes[8]:X2}";
        string? dstVendor = OuiVendors.GetValueOrDefault(dstOui);
        string? srcVendor = OuiVendors.GetValueOrDefault(srcOui);

        int etherType = (bytes[12] << 8) | bytes[13];
        string etherTypeName = LookupEtherType(etherType);

        bool hasVlan = etherType == 0x8100;
        int vlanPcp = 0;
        int vlanDei = 0;
        int vlanVid = 0;
        int realEtherType = etherType;
        string realEtherTypeName = etherTypeName;
        int payloadOffset = 14;

        if (hasVlan)
        {
            if (bytes.Length < 18)
                return (null, null, "VLAN-tagged frame too short. Need at least 18 bytes for 802.1Q.");

            int tci = (bytes[14] << 8) | bytes[15];
            vlanPcp = (tci >> 13) & 0x07;
            vlanDei = (tci >> 12) & 0x01;
            vlanVid = tci & 0x0FFF;
            realEtherType = (bytes[16] << 8) | bytes[17];
            realEtherTypeName = LookupEtherType(realEtherType);
            payloadOffset = 18;
        }

        bool hasFcs = false;
        string fcs = string.Empty;
        int fcsStart = bytes.Length;

        if (includeFcs && bytes.Length >= payloadOffset + 4)
        {
            hasFcs = true;
            fcsStart = bytes.Length - 4;
            fcs = $"0x{bytes[fcsStart]:X2}{bytes[fcsStart + 1]:X2}{bytes[fcsStart + 2]:X2}{bytes[fcsStart + 3]:X2}";
        }

        int payloadLength = fcsStart - payloadOffset;
        if (payloadLength < 0) payloadLength = 0;

        List<FrameField> fields = BuildFieldsList(
            dstMac, srcMac, isBroadcast, isMulticast,
            dstVendor, srcVendor, etherType, etherTypeName,
            hasVlan, vlanPcp, vlanDei, vlanVid,
            realEtherType, realEtherTypeName,
            payloadOffset, payloadLength, bytes.Length, hasFcs, fcs, fcsStart);

        int previewLen = Math.Min(64, payloadLength);
        string payloadHex = string.Empty;
        string payloadAscii = string.Empty;

        if (previewLen > 0)
        {
            string[] hexParts = new string[previewLen];
            char[] asciiChars = new char[previewLen];

            for (int i = 0; i < previewLen; i++)
            {
                byte b = bytes[payloadOffset + i];
                hexParts[i] = b.ToString("X2");
                asciiChars[i] = b >= 0x20 && b < 0x7F ? (char)b : '.';
            }

            payloadHex = string.Join(" ", hexParts);
            payloadAscii = new string(asciiChars);
        }

        ParsedFrame frame = new ParsedFrame
        {
            DstMac = dstMac,
            SrcMac = srcMac,
            EtherType = etherType,
            EtherTypeName = etherTypeName,
            HasVlanTag = hasVlan,
            VlanPcp = vlanPcp,
            VlanDei = vlanDei,
            VlanVid = vlanVid,
            RealEtherType = realEtherType,
            RealEtherTypeName = realEtherTypeName,
            PayloadOffset = payloadOffset,
            TotalLength = bytes.Length,
            PayloadLength = payloadLength,
            IsBroadcast = isBroadcast,
            IsMulticast = isMulticast,
            DstVendor = dstVendor,
            SrcVendor = srcVendor,
            PayloadHex = payloadHex,
            PayloadAscii = payloadAscii,
            HasFcs = hasFcs,
            Fcs = fcs,
            Fields = fields,
        };

        List<HexSpan> hexSpans = BuildHexSpans(bytes, hasVlan, hasFcs, fcsStart);

        return (frame, hexSpans, null);
    }

    public static string CleanHexInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        string upper = input.ToUpperInvariant();
        char[] chars = upper.Where(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')).ToArray();
        return new string(chars);
    }

    public static byte[] HexToBytes(string hex)
    {
        byte[] result = new byte[hex.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return result;
    }

    public static string FormatMac(byte[] bytes, int offset)
    {
        return $"{bytes[offset]:X2}:{bytes[offset + 1]:X2}:{bytes[offset + 2]:X2}:{bytes[offset + 3]:X2}:{bytes[offset + 4]:X2}:{bytes[offset + 5]:X2}";
    }

    public static string LookupEtherType(int etherType)
    {
        return EtherTypes.GetValueOrDefault(etherType, "Unknown");
    }

    public static List<FrameField> BuildFieldsList(
        string dstMac, string srcMac, bool isBroadcast, bool isMulticast,
        string? dstVendor, string? srcVendor, int etherType, string etherTypeName,
        bool hasVlan, int vlanPcp, int vlanDei, int vlanVid,
        int realEtherType, string realEtherTypeName,
        int payloadOffset, int payloadLength, int totalLength, bool hasFcs, string fcs, int fcsStart)
    {
        List<FrameField> fields = [];

        string dstNotes = isBroadcast ? "Broadcast" : isMulticast ? "Multicast" : "Unicast";
        if (!string.IsNullOrEmpty(dstVendor))
            dstNotes += $" | OUI: {dstVendor}";

        fields.Add(new FrameField
        {
            FieldName = "Destination MAC",
            Value = dstMac,
            ByteRange = "0 .. 5",
            Notes = dstNotes,
        });

        string srcNotes = string.IsNullOrEmpty(srcVendor) ? "" : $"OUI: {srcVendor}";
        fields.Add(new FrameField
        {
            FieldName = "Source MAC",
            Value = srcMac,
            ByteRange = "6 .. 11",
            Notes = srcNotes,
        });

        if (hasVlan)
        {
            fields.Add(new FrameField
            {
                FieldName = "TPID",
                Value = $"0x{etherType:X4}",
                ByteRange = "12 .. 13",
                Notes = "802.1Q VLAN Tag Protocol Identifier",
            });
            fields.Add(new FrameField
            {
                FieldName = "PCP (Priority)",
                Value = vlanPcp.ToString(),
                ByteRange = "14 [bits 15..13]",
                Notes = "Priority Code Point (0 to 7)",
            });
            fields.Add(new FrameField
            {
                FieldName = "DEI",
                Value = vlanDei.ToString(),
                ByteRange = "14 [bit 12]",
                Notes = "Drop Eligible Indicator",
            });
            fields.Add(new FrameField
            {
                FieldName = "VLAN ID",
                Value = vlanVid.ToString(),
                ByteRange = "14 .. 15 [bits 11..0]",
                Notes = "VLAN Identifier (0 to 4095)",
            });
            fields.Add(new FrameField
            {
                FieldName = "EtherType",
                Value = $"0x{realEtherType:X4}",
                ByteRange = "16 .. 17",
                Notes = realEtherTypeName,
            });
        }
        else
        {
            fields.Add(new FrameField
            {
                FieldName = "EtherType",
                Value = $"0x{etherType:X4}",
                ByteRange = "12 .. 13",
                Notes = etherTypeName,
            });
        }

        fields.Add(new FrameField
        {
            FieldName = "Payload Offset",
            Value = $"byte {payloadOffset}",
            ByteRange = "",
            Notes = $"{payloadLength} bytes of payload data",
        });

        fields.Add(new FrameField
        {
            FieldName = "Frame Length",
            Value = $"{totalLength} bytes",
            ByteRange = "",
            Notes = hasFcs ? "Includes 4-byte FCS" : "Without FCS",
        });

        if (hasFcs)
        {
            fields.Add(new FrameField
            {
                FieldName = "FCS",
                Value = fcs,
                ByteRange = $"{fcsStart} .. {fcsStart + 3}",
                Notes = "Frame Check Sequence",
            });
        }

        return fields;
    }

    public static List<HexSpan> BuildHexSpans(byte[] bytes, bool hasVlan, bool hasFcs, int fcsStart)
    {
        List<HexSpan> spans = [];
        List<(int Start, int End, string CssClass)> regions = [];

        int len = bytes.Length;

        regions.Add((0, Math.Min(6, len), "text-blue-600 dark:text-blue-400 font-semibold"));

        if (len > 6)
            regions.Add((6, Math.Min(12, len), "text-green-600 dark:text-green-400 font-semibold"));

        if (len > 12)
            regions.Add((12, Math.Min(14, len), "text-purple-600 dark:text-purple-400 font-semibold"));

        if (hasVlan)
        {
            if (len > 14)
                regions.Add((14, Math.Min(16, len), "text-yellow-600 dark:text-yellow-400 font-semibold"));

            if (len > 16)
                regions.Add((16, Math.Min(18, len), "text-purple-600 dark:text-purple-400 font-semibold"));

            int payloadEnd = hasFcs ? fcsStart : len;
            if (18 < payloadEnd)
                regions.Add((18, payloadEnd, "text-gray-600 dark:text-gray-400"));
        }
        else
        {
            int payloadEnd = hasFcs ? fcsStart : len;
            if (14 < payloadEnd)
                regions.Add((14, payloadEnd, "text-gray-600 dark:text-gray-400"));
        }

        if (hasFcs && fcsStart < len)
            regions.Add((fcsStart, len, "text-red-500 dark:text-red-400 font-semibold"));

        foreach ((int start, int end, string cssClass) in regions)
        {
            string[] parts = new string[end - start];
            for (int i = start; i < end; i++)
            {
                parts[i - start] = bytes[i].ToString("X2");
            }

            if (parts.Length > 0)
                spans.Add(new HexSpan { CssClass = cssClass, Content = string.Join(" ", parts) });
        }

        return spans;
    }

    public static string BuildCopyText(ParsedFrame frame)
    {
        List<string> lines = [];
        lines.Add("Ethernet Frame Inspector Results");
        lines.Add(new string('=', 44));

        foreach (FrameField field in frame.Fields)
        {
            string line = $"{field.FieldName}: {field.Value}";
            if (!string.IsNullOrEmpty(field.ByteRange))
                line += $"  [Bytes {field.ByteRange}]";
            if (!string.IsNullOrEmpty(field.Notes))
                line += $"  ({field.Notes})";
            lines.Add(line);
        }

        if (!string.IsNullOrEmpty(frame.PayloadHex))
        {
            lines.Add("");
            lines.Add("Payload (hex):   " + frame.PayloadHex);
            lines.Add("Payload (ASCII): " + frame.PayloadAscii);
        }

        return string.Join(Environment.NewLine, lines);
    }

    public sealed class ParsedFrame
    {
        public string DstMac { get; init; } = string.Empty;
        public string SrcMac { get; init; } = string.Empty;
        public int EtherType { get; init; }
        public string EtherTypeName { get; init; } = string.Empty;
        public bool HasVlanTag { get; init; }
        public int VlanPcp { get; init; }
        public int VlanDei { get; init; }
        public int VlanVid { get; init; }
        public int RealEtherType { get; init; }
        public string RealEtherTypeName { get; init; } = string.Empty;
        public int PayloadOffset { get; init; }
        public int TotalLength { get; init; }
        public int PayloadLength { get; init; }
        public bool IsBroadcast { get; init; }
        public bool IsMulticast { get; init; }
        public string? DstVendor { get; init; }
        public string? SrcVendor { get; init; }
        public string PayloadHex { get; init; } = string.Empty;
        public string PayloadAscii { get; init; } = string.Empty;
        public bool HasFcs { get; init; }
        public string Fcs { get; init; } = string.Empty;
        public List<FrameField> Fields { get; init; } = [];
    }

    public sealed class FrameField
    {
        public string FieldName { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string ByteRange { get; init; } = string.Empty;
        public string Notes { get; init; } = string.Empty;
    }

    public sealed class HexSpan
    {
        public string CssClass { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
    }
}
