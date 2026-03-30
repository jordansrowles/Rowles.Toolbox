namespace Rowles.Toolbox.Core.WebNetwork;

public static class RegexTesterCore
{
    public sealed record RegexPreset(string Name, string Pattern);

    public static readonly List<RegexPreset> Presets =
    [
        new("Email", @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}"),
        new("URL", @"https?://[^\s/$.?#].[^\s]*"),
        new("IPv4 Address", @"\b(?:\d{1,3}\.){3}\d{1,3}\b"),
        new("Phone (US)", @"\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}"),
        new("HTML Tag", @"<([a-zA-Z][a-zA-Z0-9]*)\b[^>]*>.*?</\1>"),
        new("Hex Colour", @"#(?:[0-9a-fA-F]{3}){1,2}\b"),
        new("Date (YYYY-MM-DD)", @"\d{4}-(?:0[1-9]|1[0-2])-(?:0[1-9]|[12]\d|3[01])"),
        new("Time (HH:MM:SS)", @"(?:[01]\d|2[0-3]):[0-5]\d(?::[0-5]\d)?"),
        new("UUID", @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"),
        new("MAC Address", @"(?:[0-9a-fA-F]{2}[:-]){5}[0-9a-fA-F]{2}"),
        new("Credit Card", @"\b\d{4}[- ]?\d{4}[- ]?\d{4}[- ]?\d{4}\b"),
        new("Postcode (UK)", @"[A-Z]{1,2}\d[A-Z\d]?\s*\d[A-Z]{2}"),
    ];
}
