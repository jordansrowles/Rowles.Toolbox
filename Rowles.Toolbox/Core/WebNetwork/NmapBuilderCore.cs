namespace Rowles.Toolbox.Core.WebNetwork;

public static class NmapBuilderCore
{
    public sealed record ScanTypeOption(string Flag, string Name, string Description);
    public sealed record TimingOption(int Level, string Name, string Description);
    public sealed record FlagInfo(string Flag, string Description);

    public sealed class NmapPreset
    {
        public string Name { get; init; } = "";
        public string Icon { get; init; } = "";
        public string ScanType { get; init; } = "-sS";
        public int TimingLevel { get; init; } = 3;
        public string PortMode { get; init; } = "common";
        public bool VersionDetection { get; init; }
        public bool OsDetection { get; init; }
        public bool Aggressive { get; init; }
        public bool ScriptEnabled { get; init; }
        public string[] ScriptCategories { get; init; } = [];
        public bool Verbose { get; init; }
        public bool SkipHostDiscovery { get; init; }
        public bool OpenOnly { get; init; }
        public bool NoDns { get; init; }
    }

    public sealed record NmapOptions(
        string Target,
        string PortMode,
        string SpecificPorts,
        string ScanType,
        bool VersionDetection,
        int VersionIntensity,
        bool OsDetection,
        bool Aggressive,
        int TimingLevel,
        string MinRate,
        string MaxRate,
        string OutputFormat,
        string OutputFilename,
        bool ScriptEnabled,
        List<string> SelectedCategories,
        string CustomScript,
        bool Verbose,
        bool VeryVerbose,
        bool SkipHostDiscovery,
        bool OpenOnly,
        bool Ipv6,
        bool Reason,
        bool Traceroute,
        bool NoDns);

    public static readonly List<ScanTypeOption> ScanTypes =
    [
        new("-sS", "SYN Scan", "Half-open scan; fast and stealthy, requires root/admin privileges"),
        new("-sT", "TCP Connect", "Full TCP handshake; works without elevated privileges"),
        new("-sU", "UDP Scan", "Scan UDP ports; slower but finds UDP services like DNS, SNMP"),
        new("-sA", "ACK Scan", "Map firewall rules; determines if ports are filtered or unfiltered"),
        new("-sN", "Null Scan", "Send packets with no flags; may bypass stateless firewalls"),
        new("-sF", "FIN Scan", "Send only FIN flag; stealthy against some firewalls"),
        new("-sX", "Xmas Scan", "Set FIN, PSH, URG flags; stealthy against certain firewalls"),
        new("-sn", "Ping Scan", "Host discovery only; no port scanning performed"),
    ];

    public static readonly List<TimingOption> TimingOptions =
    [
        new(0, "Paranoid", "Serialised scanning with 5-minute waits; extreme IDS evasion"),
        new(1, "Sneaky", "15-second intervals between probes; IDS evasion"),
        new(2, "Polite", "0.4-second intervals; uses less bandwidth"),
        new(3, "Normal", "Default parallel scanning; balanced speed"),
        new(4, "Aggressive", "Assumes fast, reliable network; may miss ports on slow links"),
        new(5, "Insane", "Very aggressive; high risk of missing ports or crashing targets"),
    ];

    public static readonly string[] ScriptCategoryNames = ["default", "vuln", "auth", "discovery", "safe"];

    public static readonly List<NmapPreset> Presets =
    [
        new() { Name = "Quick Scan", Icon = "ti-bolt", ScanType = "-sS", TimingLevel = 4, PortMode = "top100", VersionDetection = true, OpenOnly = true },
        new() { Name = "Intense Scan", Icon = "ti-flame", ScanType = "-sS", TimingLevel = 4, Aggressive = true, Verbose = true },
        new() { Name = "Stealth Scan", Icon = "ti-eye-off", ScanType = "-sS", TimingLevel = 1, SkipHostDiscovery = true, NoDns = true },
        new() { Name = "UDP Scan", Icon = "ti-arrows-up-down", ScanType = "-sU", PortMode = "top100", VersionDetection = true },
        new() { Name = "Vuln Scan", Icon = "ti-bug", VersionDetection = true, ScriptEnabled = true, ScriptCategories = ["vuln"] },
    ];

    public static string GetVersionIntensityLabel(int intensity) => intensity switch
    {
        0 => "Light",
        1 or 2 => "Low",
        3 or 4 => "Medium",
        5 or 6 => "Standard",
        7 => "Default",
        8 => "High",
        9 => "All probes",
        _ => ""
    };

    public static string BuildCommand(NmapOptions options)
    {
        List<string> parts = ["nmap"];

        parts.Add(options.ScanType);

        switch (options.PortMode)
        {
            case "top100":
                parts.Add("--top-ports 100");
                break;
            case "top1000":
                parts.Add("--top-ports 1000");
                break;
            case "specific" when !string.IsNullOrWhiteSpace(options.SpecificPorts):
                parts.Add($"-p {options.SpecificPorts.Trim()}");
                break;
            case "all":
                parts.Add("-p-");
                break;
        }

        if (options.Aggressive)
        {
            parts.Add("-A");
        }
        else
        {
            if (options.VersionDetection)
                parts.Add("-sV");
            if (options.OsDetection)
                parts.Add("-O");
        }

        if (options.VersionDetection && options.VersionIntensity != 7)
            parts.Add($"--version-intensity {options.VersionIntensity}");

        parts.Add($"-T{options.TimingLevel}");

        if (!string.IsNullOrWhiteSpace(options.MinRate) && int.TryParse(options.MinRate, out int minVal) && minVal > 0)
            parts.Add($"--min-rate {minVal}");

        if (!string.IsNullOrWhiteSpace(options.MaxRate) && int.TryParse(options.MaxRate, out int maxVal) && maxVal > 0)
            parts.Add($"--max-rate {maxVal}");

        if (options.ScriptEnabled)
        {
            List<string> scripts = new List<string>(options.SelectedCategories);
            if (!string.IsNullOrWhiteSpace(options.CustomScript))
                scripts.Add(options.CustomScript.Trim());
            if (scripts.Count > 0)
                parts.Add($"--script {string.Join(",", scripts)}");
        }

        if (options.VeryVerbose)
            parts.Add("-vv");
        else if (options.Verbose)
            parts.Add("-v");

        if (options.SkipHostDiscovery) parts.Add("-Pn");
        if (options.OpenOnly) parts.Add("--open");
        if (options.Ipv6) parts.Add("-6");
        if (options.Reason) parts.Add("--reason");
        if (options.Traceroute && !options.Aggressive) parts.Add("--traceroute");
        if (options.NoDns) parts.Add("-n");

        if (!string.IsNullOrEmpty(options.OutputFormat))
        {
            string fname = string.IsNullOrWhiteSpace(options.OutputFilename) ? "scan_output" : options.OutputFilename.Trim();
            parts.Add($"{options.OutputFormat} {fname}");
        }

        if (!string.IsNullOrWhiteSpace(options.Target))
            parts.Add(options.Target.Trim());

        return string.Join(" ", parts);
    }

    public static List<FlagInfo> BuildExplanations(NmapOptions options)
    {
        List<FlagInfo> list = [];

        ScanTypeOption? scan = ScanTypes.FirstOrDefault(s => s.Flag == options.ScanType);
        if (scan is not null)
            list.Add(new FlagInfo(scan.Flag, $"{scan.Name}: {scan.Description}"));

        switch (options.PortMode)
        {
            case "top100":
                list.Add(new FlagInfo("--top-ports 100", "Scan the 100 most commonly used ports"));
                break;
            case "top1000":
                list.Add(new FlagInfo("--top-ports 1000", "Scan the 1000 most commonly used ports"));
                break;
            case "specific" when !string.IsNullOrWhiteSpace(options.SpecificPorts):
                list.Add(new FlagInfo($"-p {options.SpecificPorts.Trim()}", "Scan only the specified port(s)"));
                break;
            case "all":
                list.Add(new FlagInfo("-p-", "Scan all 65535 TCP/UDP ports"));
                break;
        }

        if (options.Aggressive)
        {
            list.Add(new FlagInfo("-A", "Aggressive mode: enables OS detection, version detection, script scanning, and traceroute"));
        }
        else
        {
            if (options.VersionDetection)
                list.Add(new FlagInfo("-sV", "Probe open ports to determine service and version information"));
            if (options.OsDetection)
                list.Add(new FlagInfo("-O", "Attempt to identify the target operating system"));
        }

        if (options.VersionDetection && options.VersionIntensity != 7)
            list.Add(new FlagInfo($"--version-intensity {options.VersionIntensity}", "Set version detection probe intensity (0=light, 9=try every probe)"));

        TimingOption? timing = TimingOptions.FirstOrDefault(t => t.Level == options.TimingLevel);
        if (timing is not null)
            list.Add(new FlagInfo($"-T{timing.Level}", $"{timing.Name}: {timing.Description}"));

        if (!string.IsNullOrWhiteSpace(options.MinRate) && int.TryParse(options.MinRate, out int mv) && mv > 0)
            list.Add(new FlagInfo($"--min-rate {mv}", $"Send at least {mv} packets per second"));
        if (!string.IsNullOrWhiteSpace(options.MaxRate) && int.TryParse(options.MaxRate, out int xv) && xv > 0)
            list.Add(new FlagInfo($"--max-rate {xv}", $"Send no more than {xv} packets per second"));

        if (options.ScriptEnabled)
        {
            List<string> scripts = new List<string>(options.SelectedCategories);
            if (!string.IsNullOrWhiteSpace(options.CustomScript))
                scripts.Add(options.CustomScript.Trim());
            if (scripts.Count > 0)
                list.Add(new FlagInfo($"--script {string.Join(",", scripts)}", "Run specified NSE scripts against discovered services"));
        }

        if (options.VeryVerbose)
            list.Add(new FlagInfo("-vv", "Very verbose output with extra detail"));
        else if (options.Verbose)
            list.Add(new FlagInfo("-v", "Increase verbosity level"));

        if (options.SkipHostDiscovery)
            list.Add(new FlagInfo("-Pn", "Skip host discovery, treat all hosts as online"));
        if (options.OpenOnly)
            list.Add(new FlagInfo("--open", "Show only open (or possibly open) ports"));
        if (options.Ipv6)
            list.Add(new FlagInfo("-6", "Enable IPv6 scanning"));
        if (options.Reason)
            list.Add(new FlagInfo("--reason", "Display the reason a port is in a particular state"));
        if (options.Traceroute && !options.Aggressive)
            list.Add(new FlagInfo("--traceroute", "Trace the network path to each host"));
        if (options.NoDns)
            list.Add(new FlagInfo("-n", "Disable DNS resolution (faster)"));

        if (!string.IsNullOrEmpty(options.OutputFormat))
        {
            string fname = string.IsNullOrWhiteSpace(options.OutputFilename) ? "scan_output" : options.OutputFilename.Trim();
            string desc = options.OutputFormat switch
            {
                "-oN" => "Save results in normal human-readable format",
                "-oX" => "Save results in XML format (for tool integration)",
                "-oG" => "Save results in greppable format (for scripting)",
                "-oA" => "Save results in all formats (normal, XML, greppable)",
                _ => "Save scan results to file"
            };
            list.Add(new FlagInfo($"{options.OutputFormat} {fname}", desc));
        }

        return list;
    }
}
