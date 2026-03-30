namespace Rowles.Toolbox.Core.WebNetwork;

public static class RobotsTxtBuilderCore
{
    public static readonly string[] CommonAgents =
    [
        "*",
        "Googlebot",
        "Googlebot-Image",
        "Bingbot",
        "Slurp",
        "DuckDuckBot",
        "Baiduspider",
        "YandexBot",
        "facebot",
        "ia_archiver",
        "GPTBot",
        "ChatGPT-User",
        "CCBot",
        "Google-Extended",
        "anthropic-ai",
        "ClaudeBot",
        "Bytespider",
        "Applebot",
        "Twitterbot",
        "LinkedInBot"
    ];

    public static readonly string[] AiBotAgents =
    [
        "GPTBot",
        "ChatGPT-User",
        "CCBot",
        "Google-Extended",
        "anthropic-ai",
        "ClaudeBot",
        "Bytespider",
        "Applebot-Extended",
        "FacebookBot",
        "PerplexityBot",
        "Omgilibot",
        "Diffbot",
        "cohere-ai"
    ];

    public static readonly string[] CommonPaths =
    [
        "/admin/",
        "/api/",
        "/private/",
        "/tmp/",
        "/wp-admin/",
        "/cgi-bin/",
        "/search",
        "/*.pdf$",
        "/*.js$",
        "/*.css$"
    ];

    public static readonly HashSet<string> KnownDirectives = new(StringComparer.OrdinalIgnoreCase)
    {
        "user-agent",
        "disallow",
        "allow",
        "sitemap",
        "crawl-delay",
        "host",
        "clean-param",
        "request-rate",
        "visit-time",
        "noindex"
    };

    public static string GetDefaultDirective() => "Disallow";
    public static string GetAllowDirective() => "Allow";
    public static string GetDisallowDirective() => "Disallow";

    public static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength) return value;
        return value[..maxLength] + "...";
    }

    public static string GenerateOutput(
        List<RuleGroup> ruleGroups,
        List<string> sitemapUrls,
        string crawlDelay)
    {
        List<string> lines = new List<string>();

        for (int i = 0; i < ruleGroups.Count; i++)
        {
            RuleGroup ruleGroup = ruleGroups[i];
            if (i > 0)
            {
                lines.Add(string.Empty);
            }

            string agent = string.IsNullOrWhiteSpace(ruleGroup.UserAgent) ? "*" : ruleGroup.UserAgent.Trim();
            lines.Add($"User-agent: {agent}");

            if (!string.IsNullOrWhiteSpace(crawlDelay))
            {
                lines.Add($"Crawl-delay: {crawlDelay.Trim()}");
            }

            foreach (PathRule rule in ruleGroup.Rules)
            {
                string path = string.IsNullOrWhiteSpace(rule.Path) ? "/" : rule.Path.Trim();
                lines.Add($"{rule.Directive}: {path}");
            }

            if (ruleGroup.Rules.Count == 0)
            {
                lines.Add("Disallow:");
            }
        }

        foreach (string url in sitemapUrls)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                lines.Add(string.Empty);
                lines.Add($"Sitemap: {url.Trim()}");
            }
        }

        return lines.Count > 0 ? string.Join("\n", lines) : "# Empty robots.txt";
    }

    public static ValidationResult Validate(string input)
    {
        List<ValidationMessage> messages = new List<ValidationMessage>();
        List<ParsedRuleGroup> parsedGroups = new List<ParsedRuleGroup>();
        List<string> parsedSitemaps = new List<string>();

        if (string.IsNullOrWhiteSpace(input))
        {
            messages.Add(new ValidationMessage
            {
                Severity = MessageSeverity.Error,
                Text = "Input is empty.",
                Line = 0
            });
            return new ValidationResult(messages, parsedGroups, parsedSitemaps);
        }

        string[] lines = input.Split(["\r\n", "\n", "\r"], StringSplitOptions.None);

        ParsedRuleGroup? currentParsedGroup = null;
        HashSet<string> seenAgents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        HashSet<string> currentGroupRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool hasAnyUserAgent = false;
        bool hasRulesBeforeAgent = false;

        for (int lineNum = 0; lineNum < lines.Length; lineNum++)
        {
            int displayLine = lineNum + 1;
            string rawLine = lines[lineNum];
            string trimmed = rawLine.Trim();

            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            int colonIdx = trimmed.IndexOf(':');
            if (colonIdx < 0)
            {
                messages.Add(new ValidationMessage
                {
                    Severity = MessageSeverity.Error,
                    Text = $"Invalid syntax: missing colon separator in \"{Truncate(trimmed, 60)}\".",
                    Line = displayLine
                });
                continue;
            }

            string directive = trimmed[..colonIdx].Trim();
            string directiveValue = trimmed[(colonIdx + 1)..].Trim();

            if (!KnownDirectives.Contains(directive))
            {
                messages.Add(new ValidationMessage
                {
                    Severity = MessageSeverity.Warning,
                    Text = $"Unknown directive \"{directive}\". It may be ignored by crawlers.",
                    Line = displayLine
                });
            }

            if (string.Equals(directive, "User-agent", StringComparison.OrdinalIgnoreCase))
            {
                hasAnyUserAgent = true;

                if (string.IsNullOrWhiteSpace(directiveValue))
                {
                    messages.Add(new ValidationMessage
                    {
                        Severity = MessageSeverity.Error,
                        Text = "Empty User-agent value.",
                        Line = displayLine
                    });
                }

                if (seenAgents.Contains(directiveValue) && !string.IsNullOrWhiteSpace(directiveValue))
                {
                    messages.Add(new ValidationMessage
                    {
                        Severity = MessageSeverity.Warning,
                        Text = $"Duplicate User-agent \"{directiveValue}\". Later rules may override earlier ones.",
                        Line = displayLine
                    });
                }
                seenAgents.Add(directiveValue);

                currentParsedGroup = new ParsedRuleGroup { UserAgent = directiveValue };
                currentGroupRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                parsedGroups.Add(currentParsedGroup);
            }
            else if (string.Equals(directive, "Disallow", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(directive, "Allow", StringComparison.OrdinalIgnoreCase))
            {
                if (currentParsedGroup is null)
                {
                    if (!hasRulesBeforeAgent)
                    {
                        messages.Add(new ValidationMessage
                        {
                            Severity = MessageSeverity.Error,
                            Text = $"{directive} rule found before any User-agent declaration.",
                            Line = displayLine
                        });
                        hasRulesBeforeAgent = true;
                    }
                    continue;
                }

                string ruleKey = $"{directive}:{directiveValue}";
                if (currentGroupRules.Contains(ruleKey))
                {
                    messages.Add(new ValidationMessage
                    {
                        Severity = MessageSeverity.Warning,
                        Text = $"Duplicate rule \"{directive}: {directiveValue}\" for agent \"{currentParsedGroup.UserAgent}\".",
                        Line = displayLine
                    });
                }
                currentGroupRules.Add(ruleKey);

                if (!string.IsNullOrEmpty(directiveValue) && !directiveValue.StartsWith('/') && !directiveValue.StartsWith('*'))
                {
                    messages.Add(new ValidationMessage
                    {
                        Severity = MessageSeverity.Warning,
                        Text = $"Path \"{directiveValue}\" should start with \"/\" or \"*\".",
                        Line = displayLine
                    });
                }

                currentParsedGroup.Rules.Add(new PathRule
                {
                    Directive = directive.Substring(0, 1).ToUpperInvariant() + directive.Substring(1).ToLowerInvariant(),
                    Path = directiveValue
                });
            }
            else if (string.Equals(directive, "Sitemap", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(directiveValue))
                {
                    messages.Add(new ValidationMessage
                    {
                        Severity = MessageSeverity.Warning,
                        Text = "Empty Sitemap value.",
                        Line = displayLine
                    });
                }
                else
                {
                    if (!directiveValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                     && !directiveValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        messages.Add(new ValidationMessage
                        {
                            Severity = MessageSeverity.Warning,
                            Text = $"Sitemap URL \"{Truncate(directiveValue, 60)}\" should be an absolute URL starting with http:// or https://.",
                            Line = displayLine
                        });
                    }
                    parsedSitemaps.Add(directiveValue);
                }
            }
            else if (string.Equals(directive, "Crawl-delay", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(directiveValue, out int delayVal) || delayVal < 0)
                {
                    messages.Add(new ValidationMessage
                    {
                        Severity = MessageSeverity.Warning,
                        Text = $"Crawl-delay value \"{directiveValue}\" should be a non-negative integer.",
                        Line = displayLine
                    });
                }
                if (currentParsedGroup is not null)
                {
                    currentParsedGroup.CrawlDelay = directiveValue;
                }
            }
        }

        if (!hasAnyUserAgent)
        {
            messages.Add(new ValidationMessage
            {
                Severity = MessageSeverity.Error,
                Text = "No User-agent directive found. A valid robots.txt requires at least one.",
                Line = 0
            });
        }

        return new ValidationResult(messages, parsedGroups, parsedSitemaps);
    }

    // ── Inner types ──

    public enum MessageSeverity { Warning, Error }

    public sealed class ValidationMessage
    {
        public MessageSeverity Severity { get; init; }
        public string Text { get; init; } = string.Empty;
        public int Line { get; init; }
    }

    public sealed class RuleGroup
    {
        public string UserAgent { get; set; } = "*";
        public List<PathRule> Rules { get; } = [];
    }

    public sealed class PathRule
    {
        public string Directive { get; set; } = "Disallow";
        public string Path { get; set; } = "/";
    }

    public sealed class ParsedRuleGroup
    {
        public string UserAgent { get; set; } = string.Empty;
        public List<PathRule> Rules { get; } = [];
        public string? CrawlDelay { get; set; }
    }

    public sealed record ValidationResult(
        List<ValidationMessage> Messages,
        List<ParsedRuleGroup> ParsedGroups,
        List<string> ParsedSitemaps);
}
