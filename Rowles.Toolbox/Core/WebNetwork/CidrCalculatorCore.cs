using System.Net;
using System.Net.Sockets;

namespace Rowles.Toolbox.Core.WebNetwork;

public static class CidrCalculatorCore
{
    public sealed class CidrResult
    {
        public string Cidr { get; init; } = string.Empty;
        public string NetworkAddress { get; init; } = string.Empty;
        public string BroadcastAddress { get; init; } = string.Empty;
        public string SubnetMask { get; init; } = string.Empty;
        public string WildcardMask { get; init; } = string.Empty;
        public string FirstUsableHost { get; init; } = string.Empty;
        public string LastUsableHost { get; init; } = string.Empty;
        public long TotalAddresses { get; init; }
        public long UsableHosts { get; init; }
        public string IpClass { get; init; } = string.Empty;
        public bool IsPrivate { get; init; }
        public string IpBinary { get; init; } = string.Empty;
        public string MaskBinary { get; init; } = string.Empty;
        public string NetworkBinary { get; init; } = string.Empty;
        public uint NetworkUint { get; init; }
        public uint BroadcastUint { get; init; }
        public List<string> Subnets { get; init; } = [];
    }

    public static readonly string[] Examples = ["10.0.0.0/8", "172.16.0.0/12", "192.168.1.0/24", "192.168.0.0/22", "10.10.0.0/16"];

    public static readonly (int Prefix, string Mask, long Hosts)[] CidrReference =
    [
        (8, "255.0.0.0", 16777214),
        (12, "255.240.0.0", 1048574),
        (16, "255.255.0.0", 65534),
        (20, "255.255.240.0", 4094),
        (22, "255.255.252.0", 1022),
        (24, "255.255.255.0", 254),
        (25, "255.255.255.128", 126),
        (26, "255.255.255.192", 62),
        (27, "255.255.255.224", 30),
        (28, "255.255.255.240", 14),
        (29, "255.255.255.248", 6),
        (30, "255.255.255.252", 2),
        (31, "255.255.255.254", 0),
        (32, "255.255.255.255", 1),
    ];

    public static (CidrResult? Result, string? Error) Calculate(string input)
    {
        input = input.Trim();
        if (string.IsNullOrEmpty(input))
        {
            return (null, "Please enter a CIDR notation (e.g. 192.168.1.0/24).");
        }

        string[] parts = input.Split('/');
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out IPAddress? ipAddress) || !int.TryParse(parts[1], out int prefix))
        {
            return (null, "Invalid CIDR format. Expected format: IP/prefix (e.g. 192.168.1.0/24).");
        }

        if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            return (null, "Only IPv4 addresses are supported.");
        }

        if (prefix < 0 || prefix > 32)
        {
            return (null, "Prefix length must be between 0 and 32.");
        }

        byte[] ipBytes = ipAddress.GetAddressBytes();
        uint ip = (uint)(ipBytes[0] << 24 | ipBytes[1] << 16 | ipBytes[2] << 8 | ipBytes[3]);

        uint mask = prefix == 0 ? 0 : uint.MaxValue << (32 - prefix);
        uint network = ip & mask;
        uint broadcast = network | ~mask;

        uint firstHost = prefix >= 31 ? network : network + 1;
        uint lastHost = prefix >= 31 ? broadcast : broadcast - 1;
        long totalAddresses = (long)1 << (32 - prefix);
        long usableHosts = prefix >= 31 ? (prefix == 32 ? 1 : 0) : totalAddresses - 2;

        byte firstOctet = ipBytes[0];
        string ipClass = firstOctet switch
        {
            < 128 => "A",
            < 192 => "B",
            < 224 => "C",
            < 240 => "D (Multicast)",
            _ => "E (Reserved)",
        };

        bool isPrivate = (firstOctet == 10) ||
                         (firstOctet == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) ||
                         (firstOctet == 192 && ipBytes[1] == 168) ||
                         (firstOctet == 127);

        List<string> subnets = [];
        if (prefix < 24 && prefix >= 8)
        {
            int subnetCount = 1 << (24 - prefix);
            int limit = Math.Min(subnetCount, 256);
            for (int i = 0; i < limit; i++)
            {
                uint subNet = network + ((uint)i << 8);
                subnets.Add($"{UintToIp(subNet)}/24");
            }
            if (subnetCount > 256)
                subnets.Add($"… and {subnetCount - 256} more");
        }

        CidrResult result = new()
        {
            Cidr = $"{UintToIp(network)}/{prefix}",
            NetworkAddress = UintToIp(network),
            BroadcastAddress = UintToIp(broadcast),
            SubnetMask = UintToIp(mask),
            WildcardMask = UintToIp(~mask),
            FirstUsableHost = UintToIp(firstHost),
            LastUsableHost = UintToIp(lastHost),
            TotalAddresses = totalAddresses,
            UsableHosts = usableHosts,
            IpClass = ipClass,
            IsPrivate = isPrivate,
            IpBinary = FormatBinary(ip),
            MaskBinary = FormatBinary(mask),
            NetworkBinary = FormatBinary(network),
            NetworkUint = network,
            BroadcastUint = broadcast,
            Subnets = subnets,
        };

        return (result, null);
    }

    public static bool CheckIpInRange(CidrResult result, string ipText)
    {
        if (string.IsNullOrWhiteSpace(ipText)) return false;

        if (!IPAddress.TryParse(ipText.Trim(), out IPAddress? checkAddr) ||
            checkAddr.AddressFamily != AddressFamily.InterNetwork)
            return false;

        byte[] bytes = checkAddr.GetAddressBytes();
        uint checkUint = (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
        return checkUint >= result.NetworkUint && checkUint <= result.BroadcastUint;
    }

    public static string UintToIp(uint value) =>
        $"{(value >> 24) & 0xFF}.{(value >> 16) & 0xFF}.{(value >> 8) & 0xFF}.{value & 0xFF}";

    public static string FormatBinary(uint value)
    {
        string binary = Convert.ToString(value, 2).PadLeft(32, '0');
        return $"{binary[..8]}.{binary[8..16]}.{binary[16..24]}.{binary[24..]}";
    }
}
