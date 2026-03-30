namespace Rowles.Toolbox.Core.Image;

public static class CodeScreenshotCore
{
    public sealed record BackgroundPreset(string Name, string Value);

    public static readonly BackgroundPreset[] BackgroundPresets =
    [
        new("Sunset", "#f093fb → #f5576c"),
        new("Ocean", "#667eea → #764ba2"),
        new("Forest", "#11998e → #38ef7d"),
        new("Night", "#0f0c29 → #302b63 → #24243e"),
        new("Plain", "#1e1e1e"),
        new("None", "transparent"),
    ];

    public static string SanitiseFilename(string name)
    {
        char[] invalid = System.IO.Path.GetInvalidFileNameChars();
        System.Text.StringBuilder sb = new(name.Length);
        foreach (char c in name)
        {
            sb.Append(Array.IndexOf(invalid, c) >= 0 ? '_' : c);
        }
        return sb.ToString().Trim();
    }
}
