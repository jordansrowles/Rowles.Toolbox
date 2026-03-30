using System.Text.Json;

namespace Rowles.Toolbox.Core.Encoding;

public static class JwtDecoderCore
{
    private static readonly JsonSerializerOptions s_indentedOptions = new() { WriteIndented = true };

    public static string Base64UrlDecodeToString(string input)
    {
        string padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        byte[] bytes = Convert.FromBase64String(padded);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    public static string PrettyPrintJson(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc.RootElement, s_indentedOptions);
    }

    public static (DateTimeOffset? Expiry, DateTimeOffset? IssuedAt, bool IsExpired) ParseClaims(string payloadJson)
    {
        DateTimeOffset? expiry = null;
        DateTimeOffset? issuedAt = null;
        bool isExpired = false;

        using JsonDocument doc = JsonDocument.Parse(payloadJson);
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("exp", out JsonElement expEl) && expEl.TryGetInt64(out long expUnix))
        {
            expiry = DateTimeOffset.FromUnixTimeSeconds(expUnix);
            isExpired = expiry < DateTimeOffset.UtcNow;
        }

        if (root.TryGetProperty("iat", out JsonElement iatEl) && iatEl.TryGetInt64(out long iatUnix))
        {
            issuedAt = DateTimeOffset.FromUnixTimeSeconds(iatUnix);
        }

        return (expiry, issuedAt, isExpired);
    }
}
