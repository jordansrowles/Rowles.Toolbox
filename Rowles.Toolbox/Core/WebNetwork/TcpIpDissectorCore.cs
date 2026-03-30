namespace Rowles.Toolbox.Core.WebNetwork;

public static class TcpIpDissectorCore
{
    public const string TcpSynSample =
        "45 00 00 3c 1c 46 40 00 40 06 b1 e6 ac 10 0a 63 ac 10 0a 0c " +
        "00 50 e6 24 00 00 00 00 a0 02 72 10 00 00 00 00 02 04 05 b4 " +
        "04 02 08 0a 00 9c 27 24 00 00 00 00 01 03 03 07";

    public const string UdpDnsSample =
        "45 00 00 3c 00 00 40 00 40 11 00 00 c0 a8 01 64 08 08 08 08 " +
        "00 35 e1 58 00 28 00 00";

    public const string TcpAckSample =
        "45 00 00 28 ab cd 40 00 80 06 00 00 c0 a8 00 01 c0 a8 00 02 " +
        "c0 01 00 50 00 00 00 01 00 00 00 02 50 10 10 00 00 00 00 00";

    public static (DissectionResult? Result, string? Error) Dissect(string hexInput)
    {
        string cleaned = CleanHexInput(hexInput);
        if (string.IsNullOrEmpty(cleaned))
            return (null, "Please enter hex bytes to parse.");

        if (cleaned.Length % 2 != 0)
            return (null, "Invalid hex string: odd number of characters.");

        byte[] bytes;
        try
        {
            bytes = HexToBytes(cleaned);
        }
        catch
        {
            return (null, "Invalid hex characters detected.");
        }

        if (bytes.Length < 20)
            return (null, "Input too short for an IPv4 header (minimum 20 bytes required).");

        int version = (bytes[0] >> 4) & 0x0F;
        if (version != 4)
            return (null, $"Not an IPv4 packet. Version field is {version}, expected 4.");

        int ihl = bytes[0] & 0x0F;
        int ipHeaderLength = ihl * 4;
        if (ipHeaderLength < 20 || ipHeaderLength > bytes.Length)
            return (null, $"Invalid IHL value: {ihl} (computed header length {ipHeaderLength} bytes exceeds data).");

        List<HeaderField> ipv4Fields = ParseIpv4Header(bytes, ihl, ipHeaderLength);

        int protocol = bytes[9];
        List<HeaderField> transportFields = new List<HeaderField>();
        int transportHeaderLen = 0;
        int transportOffset = ipHeaderLength;

        if (protocol == 6 && bytes.Length >= transportOffset + 20)
        {
            transportFields = ParseTcpHeader(bytes, transportOffset);
            int dataOffset = (bytes[transportOffset + 12] >> 4) & 0x0F;
            transportHeaderLen = dataOffset * 4;
        }
        else if (protocol == 17 && bytes.Length >= transportOffset + 8)
        {
            transportFields = ParseUdpHeader(bytes, transportOffset);
            transportHeaderLen = 8;
        }

        DissectionResult result = new DissectionResult
        {
            Ipv4Fields = ipv4Fields,
            TransportFields = transportFields,
            Protocol = protocol == 6 ? "TCP" : protocol == 17 ? "UDP" : "Other",
            RawBytes = bytes,
            Ipv4HeaderLength = ipHeaderLength,
            TransportHeaderLength = transportHeaderLen,
        };

        return (result, null);
    }

    public static string CleanHexInput(string input)
    {
        string text = input.Replace("0x", "").Replace("0X", "");
        char[] hexChars = text.Where(c => Uri.IsHexDigit(c)).ToArray();
        return new string(hexChars);
    }

    public static byte[] HexToBytes(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    public static string FormatHex(byte[] bytes, int start, int length)
    {
        string[] parts = new string[length];
        for (int i = 0; i < length; i++)
        {
            parts[i] = bytes[start + i].ToString("X2");
        }
        return string.Join(" ", parts);
    }

    public static int BoolToFlag(bool value) => value ? 1 : 0;

    public static string GetProtocolName(int protocol) => protocol switch
    {
        1 => "ICMP",
        2 => "IGMP",
        6 => "TCP",
        17 => "UDP",
        41 => "IPv6",
        47 => "GRE",
        50 => "ESP",
        51 => "AH",
        58 => "ICMPv6",
        89 => "OSPF",
        132 => "SCTP",
        _ => $"Unknown ({protocol})",
    };

    public static string GetProtocolDescription(int protocol) => protocol switch
    {
        1 => "Internet Control Message Protocol",
        2 => "Internet Group Management Protocol",
        6 => "Transmission Control Protocol",
        17 => "User Datagram Protocol",
        41 => "IPv6 Encapsulation",
        47 => "Generic Routing Encapsulation",
        50 => "Encapsulating Security Payload",
        51 => "Authentication Header",
        58 => "ICMP for IPv6",
        89 => "Open Shortest Path First",
        132 => "Stream Control Transmission Protocol",
        _ => $"Protocol number {protocol}",
    };

    public static List<HeaderField> ParseIpv4Header(byte[] bytes, int ihl, int headerLength)
    {
        List<HeaderField> fields = new List<HeaderField>();

        int version = (bytes[0] >> 4) & 0x0F;
        int dscp = (bytes[1] >> 2) & 0x3F;
        int ecn = bytes[1] & 0x03;
        int totalLength = (bytes[2] << 8) | bytes[3];
        int identification = (bytes[4] << 8) | bytes[5];
        int flagsAndOffset = (bytes[6] << 8) | bytes[7];
        bool df = (flagsAndOffset & 0x4000) != 0;
        bool mf = (flagsAndOffset & 0x2000) != 0;
        int fragmentOffset = flagsAndOffset & 0x1FFF;
        int ttl = bytes[8];
        int protocol = bytes[9];
        int checksum = (bytes[10] << 8) | bytes[11];
        string srcIp = $"{bytes[12]}.{bytes[13]}.{bytes[14]}.{bytes[15]}";
        string dstIp = $"{bytes[16]}.{bytes[17]}.{bytes[18]}.{bytes[19]}";

        fields.Add(new HeaderField
        {
            Name = "Version",
            Value = version.ToString(),
            HexBytes = FormatHex(bytes, 0, 1),
            Description = "IPv4 (upper nibble of byte 0)",
        });

        fields.Add(new HeaderField
        {
            Name = "IHL",
            Value = $"{ihl} ({headerLength} bytes)",
            HexBytes = FormatHex(bytes, 0, 1),
            Description = "Internet Header Length in 32-bit words (lower nibble)",
        });

        fields.Add(new HeaderField
        {
            Name = "DSCP",
            Value = dscp.ToString(),
            HexBytes = FormatHex(bytes, 1, 1),
            Description = "Differentiated Services Code Point (upper 6 bits)",
        });

        fields.Add(new HeaderField
        {
            Name = "ECN",
            Value = ecn.ToString(),
            HexBytes = FormatHex(bytes, 1, 1),
            Description = "Explicit Congestion Notification (lower 2 bits)",
        });

        fields.Add(new HeaderField
        {
            Name = "Total Length",
            Value = $"{totalLength} bytes",
            HexBytes = FormatHex(bytes, 2, 2),
            Description = "Total packet length including header and data",
        });

        fields.Add(new HeaderField
        {
            Name = "Identification",
            Value = $"0x{identification:X4} ({identification})",
            HexBytes = FormatHex(bytes, 4, 2),
            Description = "Unique fragment identifier for reassembly",
        });

        List<string> flagNames = new List<string>();
        if (df) flagNames.Add("DF");
        if (mf) flagNames.Add("MF");
        string flagsDisplay = flagNames.Count > 0 ? string.Join(", ", flagNames) : "None";

        fields.Add(new HeaderField
        {
            Name = "Flags",
            Value = flagsDisplay,
            HexBytes = FormatHex(bytes, 6, 2),
            Description = $"DF={BoolToFlag(df)}, MF={BoolToFlag(mf)}",
        });

        fields.Add(new HeaderField
        {
            Name = "Fragment Offset",
            Value = fragmentOffset.ToString(),
            HexBytes = FormatHex(bytes, 6, 2),
            Description = "Fragment offset in 8-byte blocks",
        });

        fields.Add(new HeaderField
        {
            Name = "TTL",
            Value = ttl.ToString(),
            HexBytes = FormatHex(bytes, 8, 1),
            Description = "Time to Live (hop limit)",
        });

        fields.Add(new HeaderField
        {
            Name = "Protocol",
            Value = $"{protocol} ({GetProtocolName(protocol)})",
            HexBytes = FormatHex(bytes, 9, 1),
            Description = GetProtocolDescription(protocol),
        });

        fields.Add(new HeaderField
        {
            Name = "Header Checksum",
            Value = $"0x{checksum:X4}",
            HexBytes = FormatHex(bytes, 10, 2),
            Description = "Error-checking for the header",
        });

        fields.Add(new HeaderField
        {
            Name = "Source IP",
            Value = srcIp,
            HexBytes = FormatHex(bytes, 12, 4),
            Description = "Source IP address",
        });

        fields.Add(new HeaderField
        {
            Name = "Destination IP",
            Value = dstIp,
            HexBytes = FormatHex(bytes, 16, 4),
            Description = "Destination IP address",
        });

        return fields;
    }

    public static List<HeaderField> ParseTcpHeader(byte[] bytes, int offset)
    {
        List<HeaderField> fields = new List<HeaderField>();

        int srcPort = (bytes[offset] << 8) | bytes[offset + 1];
        int dstPort = (bytes[offset + 2] << 8) | bytes[offset + 3];
        uint seqNum = (uint)((bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | bytes[offset + 7]);
        uint ackNum = (uint)((bytes[offset + 8] << 24) | (bytes[offset + 9] << 16) | (bytes[offset + 10] << 8) | bytes[offset + 11]);
        int dataOffset = (bytes[offset + 12] >> 4) & 0x0F;
        int flagsByte = bytes[offset + 13];

        bool urg = (flagsByte & 0x20) != 0;
        bool ack = (flagsByte & 0x10) != 0;
        bool psh = (flagsByte & 0x08) != 0;
        bool rst = (flagsByte & 0x04) != 0;
        bool syn = (flagsByte & 0x02) != 0;
        bool fin = (flagsByte & 0x01) != 0;

        int windowSize = (bytes[offset + 14] << 8) | bytes[offset + 15];
        int checksum = (bytes[offset + 16] << 8) | bytes[offset + 17];
        int urgentPtr = (bytes[offset + 18] << 8) | bytes[offset + 19];

        fields.Add(new HeaderField
        {
            Name = "Source Port",
            Value = srcPort.ToString(),
            HexBytes = FormatHex(bytes, offset, 2),
            Description = $"Port {srcPort}",
        });

        fields.Add(new HeaderField
        {
            Name = "Destination Port",
            Value = dstPort.ToString(),
            HexBytes = FormatHex(bytes, offset + 2, 2),
            Description = $"Port {dstPort}",
        });

        fields.Add(new HeaderField
        {
            Name = "Sequence Number",
            Value = seqNum.ToString(),
            HexBytes = FormatHex(bytes, offset + 4, 4),
            Description = "Sequence number of the first data byte",
        });

        fields.Add(new HeaderField
        {
            Name = "Acknowledgment Number",
            Value = ackNum.ToString(),
            HexBytes = FormatHex(bytes, offset + 8, 4),
            Description = "Next expected byte from sender",
        });

        fields.Add(new HeaderField
        {
            Name = "Data Offset",
            Value = $"{dataOffset} ({dataOffset * 4} bytes)",
            HexBytes = FormatHex(bytes, offset + 12, 1),
            Description = "TCP header length in 32-bit words (upper nibble)",
        });

        List<string> activeFlags = new List<string>();
        if (urg) activeFlags.Add("URG");
        if (ack) activeFlags.Add("ACK");
        if (psh) activeFlags.Add("PSH");
        if (rst) activeFlags.Add("RST");
        if (syn) activeFlags.Add("SYN");
        if (fin) activeFlags.Add("FIN");
        string flagsValue = activeFlags.Count > 0 ? string.Join(", ", activeFlags) : "None";

        fields.Add(new HeaderField
        {
            Name = "Flags",
            Value = flagsValue,
            HexBytes = FormatHex(bytes, offset + 13, 1),
            Description = $"URG={BoolToFlag(urg)} ACK={BoolToFlag(ack)} PSH={BoolToFlag(psh)} RST={BoolToFlag(rst)} SYN={BoolToFlag(syn)} FIN={BoolToFlag(fin)}",
        });

        fields.Add(new HeaderField
        {
            Name = "Window Size",
            Value = windowSize.ToString(),
            HexBytes = FormatHex(bytes, offset + 14, 2),
            Description = "Receive window size in bytes",
        });

        fields.Add(new HeaderField
        {
            Name = "Checksum",
            Value = $"0x{checksum:X4}",
            HexBytes = FormatHex(bytes, offset + 16, 2),
            Description = "TCP segment checksum",
        });

        fields.Add(new HeaderField
        {
            Name = "Urgent Pointer",
            Value = urgentPtr.ToString(),
            HexBytes = FormatHex(bytes, offset + 18, 2),
            Description = "Offset of urgent data (valid when URG flag set)",
        });

        return fields;
    }

    public static List<HeaderField> ParseUdpHeader(byte[] bytes, int offset)
    {
        List<HeaderField> fields = new List<HeaderField>();

        int srcPort = (bytes[offset] << 8) | bytes[offset + 1];
        int dstPort = (bytes[offset + 2] << 8) | bytes[offset + 3];
        int length = (bytes[offset + 4] << 8) | bytes[offset + 5];
        int checksum = (bytes[offset + 6] << 8) | bytes[offset + 7];

        fields.Add(new HeaderField
        {
            Name = "Source Port",
            Value = srcPort.ToString(),
            HexBytes = FormatHex(bytes, offset, 2),
            Description = $"Port {srcPort}",
        });

        fields.Add(new HeaderField
        {
            Name = "Destination Port",
            Value = dstPort.ToString(),
            HexBytes = FormatHex(bytes, offset + 2, 2),
            Description = $"Port {dstPort}",
        });

        fields.Add(new HeaderField
        {
            Name = "Length",
            Value = $"{length} bytes",
            HexBytes = FormatHex(bytes, offset + 4, 2),
            Description = "Length of UDP header and data",
        });

        fields.Add(new HeaderField
        {
            Name = "Checksum",
            Value = $"0x{checksum:X4}",
            HexBytes = FormatHex(bytes, offset + 6, 2),
            Description = "UDP datagram checksum",
        });

        return fields;
    }

    public sealed class HeaderField
    {
        public string Name { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string HexBytes { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }

    public sealed class DissectionResult
    {
        public List<HeaderField> Ipv4Fields { get; init; } = new List<HeaderField>();
        public List<HeaderField> TransportFields { get; init; } = new List<HeaderField>();
        public string Protocol { get; init; } = string.Empty;
        public byte[] RawBytes { get; init; } = Array.Empty<byte>();
        public int Ipv4HeaderLength { get; init; }
        public int TransportHeaderLength { get; init; }
    }
}
