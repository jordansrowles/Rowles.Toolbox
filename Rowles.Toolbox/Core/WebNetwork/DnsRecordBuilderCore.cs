namespace Rowles.Toolbox.Core.WebNetwork;

public static class DnsRecordBuilderCore
{
    // ════════════════════════════════════════════════════════════════════
    //  SPF
    // ════════════════════════════════════════════════════════════════════

    public static readonly HashSet<string> LookupMechanisms =
        ["include", "a", "mx", "redirect", "exists"];

    public static readonly List<SpfPreset> SpfPresets =
    [
        new("Google Workspace", ["_spf.google.com"]),
        new("Microsoft 365", ["spf.protection.outlook.com"]),
        new("Mailchimp", ["servers.mcsv.net"]),
        new("SendGrid", ["sendgrid.net"]),
        new("Amazon SES", ["amazonses.com"]),
    ];

    public static bool IsSpfModifier(string type) =>
        type is "redirect" or "exp";

    public static string SpfPlaceholder(string type) => type switch
    {
        "ip4" => "203.0.113.0/24",
        "ip6" => "2001:db8::/32",
        "a" or "mx" => "mail.example.com (optional)",
        "include" => "_spf.google.com",
        "exists" => "%{i}.spf.example.com",
        "redirect" or "exp" => "_spf.example.com",
        _ => ""
    };

    public static string BuildSpfRecord(List<SpfMechanism> mechanisms, string allQualifier)
    {
        List<string> parts = new List<string> { "v=spf1" };
        foreach (SpfMechanism m in mechanisms)
        {
            if (string.IsNullOrWhiteSpace(m.Value) && m.Type is not ("a" or "mx"))
                continue;

            if (IsSpfModifier(m.Type))
            {
                if (!string.IsNullOrWhiteSpace(m.Value))
                    parts.Add($"{m.Type}={m.Value}");
            }
            else
            {
                string qualifier = m.Qualifier == "+" ? "" : m.Qualifier;
                string value = string.IsNullOrWhiteSpace(m.Value) ? "" : $":{m.Value}";
                parts.Add($"{qualifier}{m.Type}{value}");
            }
        }
        parts.Add($"{allQualifier}all");
        return string.Join(" ", parts);
    }

    public static int CountSpfLookups(List<SpfMechanism> mechanisms) =>
        mechanisms.Count(m => LookupMechanisms.Contains(m.Type) && (!string.IsNullOrWhiteSpace(m.Value) || m.Type is "a" or "mx"));

    // ════════════════════════════════════════════════════════════════════
    //  DKIM
    // ════════════════════════════════════════════════════════════════════

    public static readonly Dictionary<string, string> DkimTagLabels = new()
    {
        ["v"] = "Version",
        ["k"] = "Key type",
        ["p"] = "Public key",
        ["t"] = "Flags",
        ["h"] = "Hash algorithms",
        ["g"] = "Granularity",
        ["n"] = "Notes",
        ["s"] = "Service type",
    };

    public static readonly List<DkimSample> DkimSamples =
    [
        new("RSA 2048", "v=DKIM1; k=rsa; p=MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA2a2rwplBQLgVM1ODUQE+TiIlGnOvMcbx0GjTRpUX/sDOB5KVQG0IfMz1FG0fQsVbFOqkZMGHsL4aUGjkVI3bKfD4sP8KE3MthgEBajHxiMMCj01MhzCzP/H0+bUMBPsnpbMFjzqWsl1SeKaGCKlQSoMT8nMAcPBpRwTFf7s8kJExVJGFNFMwjcXEA5yzFwF0sxfKDHH31gKOcOHql8KfGDmtK2SKiMPCKS9Nc/b2B4MHBo25GBsBpbqmIZ+niEcvVUmG5pw/nKVGS1K3hFj3E0cIalJFRQb/7xBJ6fazLkW+/XZXxGKjVikSTNXmKvT8KQVj06kE6RLsWgDHVJhJwIDAQAB"),
        new("Revoked Key", "v=DKIM1; k=rsa; p="),
        new("Ed25519", "v=DKIM1; k=ed25519; t=y; p=LS0tLS1CRUdJTi4uLg=="),
    ];

    public static DkimDecodeResult DecodeDkim(string input)
    {
        List<string> errors = new List<string>();
        List<DkimTag> tags = new List<DkimTag>();
        int? keySizeBits = null;
        bool isRevoked = false;

        if (string.IsNullOrWhiteSpace(input))
        {
            return new DkimDecodeResult
            {
                IsValid = false,
                Errors = ["No input provided."]
            };
        }

        string raw = input.Trim().TrimEnd(';');
        string[] pairs = raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Dictionary<string, string> parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (string pair in pairs)
        {
            int eqIdx = pair.IndexOf('=');
            if (eqIdx <= 0) continue;
            string key = pair[..eqIdx].Trim().ToLowerInvariant();
            string val = pair[(eqIdx + 1)..].Trim();
            parsed[key] = val;
        }

        // Required: v should be DKIM1
        if (parsed.TryGetValue("v", out string? version))
        {
            if (!version.Equals("DKIM1", StringComparison.OrdinalIgnoreCase))
                errors.Add($"Version should be DKIM1, found '{version}'.");
        }
        else
        {
            errors.Add("Missing v= tag (version). Should be DKIM1.");
        }

        // Required: p (public key)
        if (parsed.TryGetValue("p", out string? publicKey))
        {
            if (string.IsNullOrWhiteSpace(publicKey))
            {
                isRevoked = true;
            }
            else
            {
                keySizeBits = TryGetRsaKeySize(publicKey);
            }
        }
        else
        {
            errors.Add("Missing p= tag (public key). This is required.");
        }

        // Build tag list
        foreach (string tagKey in new[] { "v", "k", "p", "t", "h", "g", "n", "s" })
        {
            if (!parsed.TryGetValue(tagKey, out string? tagVal)) continue;

            string label = DkimTagLabels.GetValueOrDefault(tagKey, tagKey);
            string display = tagKey == "p" && tagVal.Length > 60
                ? $"{tagVal[..30]}...{tagVal[^30..]}"
                : tagVal;

            if (tagKey == "p" && string.IsNullOrWhiteSpace(tagVal))
                display = "(empty — key revoked)";

            tags.Add(new DkimTag(tagKey, label, tagVal, display));
        }

        // Include any unknown tags
        foreach (KeyValuePair<string, string> kvp in parsed)
        {
            if (DkimTagLabels.ContainsKey(kvp.Key)) continue;
            tags.Add(new DkimTag(kvp.Key, "Unknown tag", kvp.Value, kvp.Value));
        }

        return new DkimDecodeResult
        {
            IsValid = errors.Count == 0 && !isRevoked,
            IsRevoked = isRevoked,
            KeySizeBits = keySizeBits,
            Errors = errors,
            Tags = tags,
        };
    }

    public static int? TryGetRsaKeySize(string base64Key)
    {
        try
        {
            byte[] keyBytes = Convert.FromBase64String(base64Key);
#pragma warning disable CA1416 // Validate platform compatibility
            using System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create();
#pragma warning restore CA1416
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
            return rsa.KeySize;
        }
        catch
        {
            return null;
        }
    }

    // ════════════════════════════════════════════════════════════════════
    //  DMARC
    // ════════════════════════════════════════════════════════════════════

    public static string BuildDmarcRecord(DmarcOptions opts)
    {
        List<string> parts = new List<string> { "v=DMARC1", $"p={opts.Policy}" };

        if (!string.IsNullOrWhiteSpace(opts.Sp))
            parts.Add($"sp={opts.Sp}");

        if (opts.Pct != 100)
            parts.Add($"pct={opts.Pct}");

        if (!string.IsNullOrWhiteSpace(opts.Rua))
            parts.Add($"rua={opts.Rua}");

        if (!string.IsNullOrWhiteSpace(opts.Ruf))
            parts.Add($"ruf={opts.Ruf}");

        if (opts.Adkim != "r")
            parts.Add($"adkim={opts.Adkim}");

        if (opts.Aspf != "r")
            parts.Add($"aspf={opts.Aspf}");

        if (opts.Ri != 86400)
            parts.Add($"ri={opts.Ri}");

        if (opts.Fo != "0")
            parts.Add($"fo={opts.Fo}");

        return string.Join("; ", parts) + ";";
    }

    public static List<string> GetDmarcWarnings(DmarcOptions opts)
    {
        List<string> warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(opts.Rua))
            warnings.Add("No rua specified — you won't receive aggregate reports.");

        if (opts.Policy == "none")
            warnings.Add("Policy is 'none' — messages will not be quarantined or rejected. Good for monitoring only.");

        if (!string.IsNullOrWhiteSpace(opts.Rua) && !opts.Rua.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            warnings.Add("rua should start with mailto: (e.g. mailto:dmarc@example.com).");

        if (!string.IsNullOrWhiteSpace(opts.Ruf) && !opts.Ruf.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            warnings.Add("ruf should start with mailto: (e.g. mailto:forensic@example.com).");

        if (opts.Pct < 100 && opts.Policy == "none")
            warnings.Add("pct has no effect when policy is 'none'.");

        return warnings;
    }

    // ── Inner types ──

    public sealed record SpfMechanism
    {
        public string Qualifier { get; set; } = "+";
        public string Type { get; set; } = "include";
        public string Value { get; set; } = "";
    }

    public sealed record SpfPreset(string Name, string[] Includes);

    public sealed record DkimTag(string Key, string Label, string RawValue, string DisplayValue);

    public sealed class DkimDecodeResult
    {
        public bool IsValid { get; init; }
        public bool IsRevoked { get; init; }
        public int? KeySizeBits { get; init; }
        public List<string> Errors { get; init; } = [];
        public List<DkimTag> Tags { get; init; } = [];
    }

    public sealed record DkimSample(string Name, string Value);

    public sealed class DmarcOptions
    {
        public string Policy { get; set; } = "none";
        public string Sp { get; set; } = "";
        public int Pct { get; set; } = 100;
        public string Rua { get; set; } = "";
        public string Ruf { get; set; } = "";
        public string Adkim { get; set; } = "r";
        public string Aspf { get; set; } = "r";
        public int Ri { get; set; } = 86400;
        public string Fo { get; set; } = "0";
    }
}
