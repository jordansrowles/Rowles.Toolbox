namespace Rowles.Toolbox.Core.WebNetwork;

public static class HttpHeaderBuilderCore
{
    public sealed class HeaderEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public sealed record HeaderPreset(string Name, string[] SuggestedValues);

    public static readonly List<HeaderPreset> CommonHeaders =
    [
        new("Accept", ["application/json", "text/html", "text/plain", "application/xml", "*/*", "text/css", "image/webp"]),
        new("Accept-Encoding", ["gzip, deflate, br", "gzip, deflate", "identity", "br"]),
        new("Accept-Language", ["en-GB,en;q=0.9", "en-US,en;q=0.9", "fr-FR,fr;q=0.9,en;q=0.8", "*"]),
        new("Authorization", ["Bearer <token>", "Basic <base64>", "Digest <credentials>"]),
        new("Cache-Control", ["no-cache", "no-store", "max-age=3600", "public, max-age=86400", "private, no-cache", "must-revalidate", "no-store, no-cache, must-revalidate"]),
        new("Content-Type", ["application/json", "application/x-www-form-urlencoded", "multipart/form-data", "text/html; charset=utf-8", "text/plain", "application/xml", "application/octet-stream"]),
        new("Content-Length", []),
        new("Cookie", []),
        new("Host", []),
        new("Origin", []),
        new("Referer", []),
        new("User-Agent", ["Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"]),
        new("X-Forwarded-For", []),
        new("X-Forwarded-Proto", ["https", "http"]),
        new("X-Request-ID", []),
        new("Access-Control-Allow-Origin", ["*", "https://example.com"]),
        new("Access-Control-Allow-Methods", ["GET, POST, PUT, DELETE, OPTIONS", "GET, POST", "*"]),
        new("Access-Control-Allow-Headers", ["Content-Type, Authorization", "*"]),
        new("Content-Disposition", ["attachment; filename=\"file.pdf\"", "inline"]),
        new("Content-Encoding", ["gzip", "deflate", "br", "identity"]),
        new("ETag", []),
        new("Expires", ["Thu, 01 Dec 2025 16:00:00 GMT", "0", "-1"]),
        new("If-None-Match", []),
        new("If-Modified-Since", []),
        new("Location", []),
        new("Pragma", ["no-cache"]),
        new("Set-Cookie", []),
        new("Strict-Transport-Security", ["max-age=31536000; includeSubDomains", "max-age=63072000; includeSubDomains; preload"]),
        new("X-Content-Type-Options", ["nosniff"]),
        new("X-Frame-Options", ["DENY", "SAMEORIGIN"]),
        new("X-XSS-Protection", ["1; mode=block", "0"]),
        new("Content-Security-Policy", ["default-src 'self'", "default-src 'self'; script-src 'self' 'unsafe-inline'"]),
        new("Retry-After", ["120", "Fri, 31 Dec 2025 23:59:59 GMT"]),
        new("WWW-Authenticate", ["Bearer realm=\"api\"", "Basic realm=\"Access\""]),
    ];

    public static List<HeaderEntry> ParseRawHeaders(string rawInput)
    {
        List<HeaderEntry> results = [];
        if (string.IsNullOrWhiteSpace(rawInput)) return results;

        string[] lines = rawInput.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                string name = line[..colonIndex].Trim();
                string value = line[(colonIndex + 1)..].Trim();
                results.Add(new HeaderEntry { Name = name, Value = value });
            }
        }

        return results;
    }

    public static string BuildRawOutput(List<HeaderEntry> headers)
    {
        if (headers.Count == 0) return "(empty)";
        return string.Join(Environment.NewLine, headers.Select(h => $"{h.Name}: {h.Value}"));
    }
}
