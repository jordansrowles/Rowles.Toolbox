using System.Text;
using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.WebNetwork;

public static class UserAgentParserCore
{
    public sealed class ParsedUserAgent
    {
        public string BrowserName { get; set; } = "Unknown";
        public string BrowserVersion { get; set; } = "Unknown";
        public string Engine { get; set; } = "Unknown";
        public string EngineVersion { get; set; } = "Unknown";
        public string OsName { get; set; } = "Unknown";
        public string OsVersion { get; set; } = "Unknown";
        public string DeviceType { get; set; } = "Unknown";
        public string Architecture { get; set; } = "Unknown";
        public bool IsBot { get; set; }
        public string BotName { get; set; } = string.Empty;
        public string RawBreakdown { get; set; } = string.Empty;
    }

    public sealed record UaExample(string Name, string Icon, string Value);

    public static readonly List<UaExample> Examples =
    [
        new("Chrome (Windows)", "ti-brand-chrome",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"),
        new("Firefox (Linux)", "ti-brand-firefox",
            "Mozilla/5.0 (X11; Linux x86_64; rv:132.0) Gecko/20100101 Firefox/132.0"),
        new("Safari (macOS)", "ti-brand-safari",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_1) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15"),
        new("Edge (Windows)", "ti-brand-edge",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0"),
        new("Chrome (Android)", "ti-device-mobile",
            "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Mobile Safari/537.36"),
        new("Safari (iPhone)", "ti-device-mobile",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Mobile/15E148 Safari/604.1"),
        new("Googlebot", "ti-robot",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)"),
        new("Bingbot", "ti-robot",
            "Mozilla/5.0 (compatible; bingbot/2.0; +http://www.bing.com/bingbot.htm)"),
        new("cURL", "ti-terminal",
            "curl/8.4.0"),
        new("Opera (Windows)", "ti-brand-opera",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 OPR/114.0.0.0"),
    ];

    public static ParsedUserAgent Parse(string userAgent)
    {
        string ua = userAgent.Trim();
        ParsedUserAgent result = new();

        // Bot detection
        Match botMatch = Regex.Match(ua, @"(Googlebot|bingbot|Baiduspider|YandexBot|DuckDuckBot|Slurp|facebookexternalhit|LinkedInBot|Twitterbot|applebot|AhrefsBot|SemrushBot|MJ12bot|DotBot|PetalBot|GPTBot)[/\s]?(\S*)?", RegexOptions.IgnoreCase);
        if (botMatch.Success)
        {
            result.IsBot = true;
            result.BotName = botMatch.Groups[1].Value;
            result.BrowserName = botMatch.Groups[1].Value;
            result.BrowserVersion = botMatch.Groups[2].Success ? botMatch.Groups[2].Value : "Unknown";
            result.DeviceType = "Bot";
        }

        if (ua.StartsWith("curl/", StringComparison.OrdinalIgnoreCase))
        {
            result.BrowserName = "cURL";
            result.BrowserVersion = ua["curl/".Length..];
            result.DeviceType = "CLI";
            result.Engine = "None";
            BuildRawBreakdown(result, ua);
            return result;
        }

        // OS detection
        if (Regex.Match(ua, @"Windows NT (\d+\.\d+)") is { Success: true } winMatch)
        {
            result.OsName = "Windows";
            result.OsVersion = winMatch.Groups[1].Value switch
            {
                "10.0" => "10/11",
                "6.3" => "8.1",
                "6.2" => "8",
                "6.1" => "7",
                string v => v,
            };
            result.Architecture = ua.Contains("Win64") || ua.Contains("x64") ? "x86_64" : "x86";
        }
        else if (Regex.Match(ua, @"Mac OS X (\d+[._]\d+(?:[._]\d+)?)") is { Success: true } macMatch)
        {
            result.OsName = "macOS";
            result.OsVersion = macMatch.Groups[1].Value.Replace('_', '.');
            result.Architecture = ua.Contains("arm64") || ua.Contains("Apple") ? "ARM64" : "x86_64";
        }
        else if (Regex.Match(ua, @"iPhone OS (\d+[._]\d+)") is { Success: true } iosMatch)
        {
            result.OsName = "iOS";
            result.OsVersion = iosMatch.Groups[1].Value.Replace('_', '.');
        }
        else if (Regex.Match(ua, @"Android (\d+(?:\.\d+)*)") is { Success: true } androidMatch)
        {
            result.OsName = "Android";
            result.OsVersion = androidMatch.Groups[1].Value;
        }
        else if (ua.Contains("Linux", StringComparison.OrdinalIgnoreCase))
        {
            result.OsName = "Linux";
            result.Architecture = ua.Contains("x86_64") ? "x86_64" : ua.Contains("aarch64") ? "ARM64" : "Unknown";
        }
        else if (ua.Contains("CrOS", StringComparison.OrdinalIgnoreCase))
        {
            result.OsName = "Chrome OS";
        }

        // Device type
        if (!result.IsBot)
        {
            if (ua.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
                ua.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
                ua.Contains("Android", StringComparison.OrdinalIgnoreCase) && !ua.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
            {
                result.DeviceType = ua.Contains("Tablet", StringComparison.OrdinalIgnoreCase) || ua.Contains("iPad", StringComparison.OrdinalIgnoreCase) ? "Tablet" : "Mobile";
            }
            else if (ua.Contains("iPad", StringComparison.OrdinalIgnoreCase))
            {
                result.DeviceType = "Tablet";
            }
            else
            {
                result.DeviceType = "Desktop";
            }
        }

        // Browser detection (order matters)
        if (!result.IsBot)
        {
            if (Regex.Match(ua, @"Edg[e/](\d+[\.\d]*)") is { Success: true } edgeMatch)
            {
                result.BrowserName = "Microsoft Edge";
                result.BrowserVersion = edgeMatch.Groups[1].Value;
            }
            else if (Regex.Match(ua, @"OPR/(\d+[\.\d]*)") is { Success: true } operaMatch)
            {
                result.BrowserName = "Opera";
                result.BrowserVersion = operaMatch.Groups[1].Value;
            }
            else if (Regex.Match(ua, @"Vivaldi/(\d+[\.\d]*)") is { Success: true } vivaldiMatch)
            {
                result.BrowserName = "Vivaldi";
                result.BrowserVersion = vivaldiMatch.Groups[1].Value;
            }
            else if (Regex.Match(ua, @"SamsungBrowser/(\d+[\.\d]*)") is { Success: true } samsungMatch)
            {
                result.BrowserName = "Samsung Internet";
                result.BrowserVersion = samsungMatch.Groups[1].Value;
            }
            else if (Regex.Match(ua, @"YaBrowser/(\d+[\.\d]*)") is { Success: true } yandexMatch)
            {
                result.BrowserName = "Yandex Browser";
                result.BrowserVersion = yandexMatch.Groups[1].Value;
            }
            else if (ua.Contains("Chrome/") && Regex.Match(ua, @"Chrome/(\d+[\.\d]*)") is { Success: true } chromeMatch)
            {
                result.BrowserName = "Google Chrome";
                result.BrowserVersion = chromeMatch.Groups[1].Value;
            }
            else if (Regex.Match(ua, @"Firefox/(\d+[\.\d]*)") is { Success: true } ffMatch)
            {
                result.BrowserName = "Mozilla Firefox";
                result.BrowserVersion = ffMatch.Groups[1].Value;
            }
            else if (Regex.Match(ua, @"Version/(\d+[\.\d]*)\s+Safari") is { Success: true } safariMatch)
            {
                result.BrowserName = "Apple Safari";
                result.BrowserVersion = safariMatch.Groups[1].Value;
            }
            else if (Regex.Match(ua, @"MSIE\s+(\d+[\.\d]*)") is { Success: true } ieMatch)
            {
                result.BrowserName = "Internet Explorer";
                result.BrowserVersion = ieMatch.Groups[1].Value;
            }
            else if (ua.Contains("Trident/"))
            {
                result.BrowserName = "Internet Explorer";
                Match rvMatch = Regex.Match(ua, @"rv:(\d+[\.\d]*)");
                result.BrowserVersion = rvMatch.Success ? rvMatch.Groups[1].Value : "11";
            }
        }

        // Engine detection
        if (ua.Contains("AppleWebKit/"))
        {
            Match webkitVersion = Regex.Match(ua, @"AppleWebKit/(\d+[\.\d]*)");
            result.EngineVersion = webkitVersion.Success ? webkitVersion.Groups[1].Value : "Unknown";
            result.Engine = ua.Contains("Chrome/") ? "Blink" : "WebKit";
        }
        else if (ua.Contains("Gecko/"))
        {
            result.Engine = "Gecko";
            Match geckoVersion = Regex.Match(ua, @"rv:(\d+[\.\d]*)");
            result.EngineVersion = geckoVersion.Success ? geckoVersion.Groups[1].Value : "Unknown";
        }
        else if (ua.Contains("Trident/"))
        {
            result.Engine = "Trident";
            Match tridentVersion = Regex.Match(ua, @"Trident/(\d+[\.\d]*)");
            result.EngineVersion = tridentVersion.Success ? tridentVersion.Groups[1].Value : "Unknown";
        }

        BuildRawBreakdown(result, ua);
        return result;
    }

    private static void BuildRawBreakdown(ParsedUserAgent parsed, string ua)
    {
        StringBuilder sb = new();
        sb.AppendLine($"Browser:      {parsed.BrowserName} {parsed.BrowserVersion}");
        sb.AppendLine($"Engine:       {parsed.Engine} {parsed.EngineVersion}");
        sb.AppendLine($"OS:           {parsed.OsName} {parsed.OsVersion}");
        sb.AppendLine($"Device:       {parsed.DeviceType}");
        sb.AppendLine($"Architecture: {parsed.Architecture}");
        if (parsed.IsBot) sb.AppendLine($"Bot:          {parsed.BotName}");
        sb.AppendLine();
        sb.AppendLine("--- Tokens ---");

        MatchCollection tokens = Regex.Matches(ua, @"(\S+/\S+|\([^)]+\))");
        foreach (Match token in tokens)
        {
            sb.AppendLine(token.Value);
        }

        parsed.RawBreakdown = sb.ToString();
    }
}
