using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Rowles.Toolbox.Core.Text;

public static class DiffViewerCore
{
    private const int MaxDiffLines = 3000;

    public enum ChangeType { Equal, Added, Removed, Modified }

    public sealed record DiffLine(ChangeType Type, string Content, int? LineA, int? LineB);

    public sealed record SideBySidePair(DiffLine? Left, DiffLine? Right);

    public sealed record StructuralChange(string Path, ChangeType Type, string? ValueA, string? ValueB);

    public sealed record DiffStats(int Additions, int Removals, int Modifications, int Unchanged);

    // ══════════════════════════════════════════════════════════════
    //  Text Line Diff (LCS-based)
    // ══════════════════════════════════════════════════════════════

    public static List<DiffLine> ComputeLineDiff(string textA, string textB)
    {
        string[] a = SplitLines(textA);
        string[] b = SplitLines(textB);

        if (a.Length > MaxDiffLines || b.Length > MaxDiffLines)
            throw new InvalidOperationException(
                $"Input exceeds {MaxDiffLines} lines per side. Please reduce the input size.");

        int m = a.Length, n = b.Length;
        int[,] dp = new int[m + 1, n + 1];
        for (int i = 1; i <= m; i++)
            for (int j = 1; j <= n; j++)
                dp[i, j] = a[i - 1] == b[j - 1]
                    ? dp[i - 1, j - 1] + 1
                    : Math.Max(dp[i - 1, j], dp[i, j - 1]);

        List<DiffLine> result = [];
        int x = m, y = n;
        while (x > 0 || y > 0)
        {
            if (x > 0 && y > 0 && a[x - 1] == b[y - 1])
            {
                result.Add(new DiffLine(ChangeType.Equal, a[x - 1], x, y));
                x--;
                y--;
            }
            else if (y > 0 && (x == 0 || dp[x, y - 1] >= dp[x - 1, y]))
            {
                result.Add(new DiffLine(ChangeType.Added, b[y - 1], null, y));
                y--;
            }
            else
            {
                result.Add(new DiffLine(ChangeType.Removed, a[x - 1], x, null));
                x--;
            }
        }

        result.Reverse();
        return result;
    }

    public static List<SideBySidePair> ToSideBySide(List<DiffLine> diff)
    {
        List<SideBySidePair> pairs = [];
        int i = 0;
        while (i < diff.Count)
        {
            if (diff[i].Type == ChangeType.Equal)
            {
                pairs.Add(new SideBySidePair(diff[i], diff[i]));
                i++;
            }
            else
            {
                List<DiffLine> removed = [];
                List<DiffLine> added = [];
                while (i < diff.Count && diff[i].Type == ChangeType.Removed)
                    removed.Add(diff[i++]);
                while (i < diff.Count && diff[i].Type == ChangeType.Added)
                    added.Add(diff[i++]);

                int max = Math.Max(removed.Count, added.Count);
                for (int j = 0; j < max; j++)
                    pairs.Add(new SideBySidePair(
                        j < removed.Count ? removed[j] : null,
                        j < added.Count ? added[j] : null));
            }
        }

        return pairs;
    }

    public static DiffStats GetLineDiffStats(List<DiffLine> diff) =>
        new(diff.Count(d => d.Type == ChangeType.Added),
            diff.Count(d => d.Type == ChangeType.Removed),
            0,
            diff.Count(d => d.Type == ChangeType.Equal));

    public static DiffStats GetStructuralDiffStats(List<StructuralChange> changes) =>
        new(changes.Count(c => c.Type == ChangeType.Added),
            changes.Count(c => c.Type == ChangeType.Removed),
            changes.Count(c => c.Type == ChangeType.Modified),
            0);

    private static string[] SplitLines(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        if (lines.Length > 0 && lines[^1] == "") return lines[..^1];
        return lines;
    }

    // ══════════════════════════════════════════════════════════════
    //  JSON Structural Diff
    // ══════════════════════════════════════════════════════════════

    public static List<StructuralChange> ComputeJsonDiff(string jsonA, string jsonB)
    {
        List<StructuralChange> changes = [];
        try
        {
            using JsonDocument docA = JsonDocument.Parse(jsonA);
            using JsonDocument docB = JsonDocument.Parse(jsonB);
            WalkJson(docA.RootElement, docB.RootElement, "$", changes);
        }
        catch (JsonException ex)
        {
            changes.Add(new StructuralChange("Parse error", ChangeType.Removed, ex.Message, null));
        }

        return changes;
    }

    private static void WalkJson(JsonElement a, JsonElement b, string path, List<StructuralChange> c)
    {
        if (a.ValueKind != b.ValueKind)
        {
            c.Add(new StructuralChange(path, ChangeType.Modified, FmtJson(a), FmtJson(b)));
            return;
        }

        switch (a.ValueKind)
        {
            case JsonValueKind.Object:
                HashSet<string> keys = [];
                foreach (JsonProperty p in a.EnumerateObject()) keys.Add(p.Name);
                foreach (JsonProperty p in b.EnumerateObject()) keys.Add(p.Name);
                foreach (string k in keys.OrderBy(k => k))
                {
                    string cp = $"{path}.{k}";
                    bool inA = a.TryGetProperty(k, out JsonElement vA);
                    bool inB = b.TryGetProperty(k, out JsonElement vB);
                    if (inA && inB) WalkJson(vA, vB, cp, c);
                    else if (inA) c.Add(new StructuralChange(cp, ChangeType.Removed, FmtJson(vA), null));
                    else c.Add(new StructuralChange(cp, ChangeType.Added, null, FmtJson(vB)));
                }
                break;

            case JsonValueKind.Array:
                int lenA = a.GetArrayLength(), lenB = b.GetArrayLength();
                for (int i = 0; i < Math.Max(lenA, lenB); i++)
                {
                    string cp = $"{path}[{i}]";
                    if (i < lenA && i < lenB) WalkJson(a[i], b[i], cp, c);
                    else if (i < lenA) c.Add(new StructuralChange(cp, ChangeType.Removed, FmtJson(a[i]), null));
                    else c.Add(new StructuralChange(cp, ChangeType.Added, null, FmtJson(b[i])));
                }
                break;

            default:
                if (a.GetRawText() != b.GetRawText())
                    c.Add(new StructuralChange(path, ChangeType.Modified, FmtJson(a), FmtJson(b)));
                break;
        }
    }

    private static string FmtJson(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => $"\"{el.GetString()}\"",
        JsonValueKind.Object or JsonValueKind.Array =>
            el.GetRawText().Length > 60
                ? string.Concat(el.GetRawText().AsSpan(0, 57), "...")
                : el.GetRawText(),
        _ => el.GetRawText()
    };

    // ══════════════════════════════════════════════════════════════
    //  YAML Diff (parse to JSON, then structural diff)
    // ══════════════════════════════════════════════════════════════

    public static List<StructuralChange> ComputeYamlDiff(string yamlA, string yamlB)
    {
        try
        {
            object? objA = DataFormats.YamlJsonCore.ParseYamlDocument(yamlA);
            object? objB = DataFormats.YamlJsonCore.ParseYamlDocument(yamlB);
            string jA = JsonSerializer.Serialize(objA);
            string jB = JsonSerializer.Serialize(objB);
            return ComputeJsonDiff(jA, jB);
        }
        catch (Exception ex)
        {
            return [new StructuralChange("Parse error", ChangeType.Removed, ex.Message, null)];
        }
    }

    // ══════════════════════════════════════════════════════════════
    //  XML Node-Level Diff
    // ══════════════════════════════════════════════════════════════

    public static List<StructuralChange> ComputeXmlDiff(string xmlA, string xmlB)
    {
        List<StructuralChange> changes = [];
        try
        {
            XDocument dA = XDocument.Parse(xmlA);
            XDocument dB = XDocument.Parse(xmlB);
            if (dA.Root is not null && dB.Root is not null)
                WalkXml(dA.Root, dB.Root, "", changes);
        }
        catch (Exception ex)
        {
            changes.Add(new StructuralChange("Parse error", ChangeType.Removed, ex.Message, null));
        }

        return changes;
    }

    private static void WalkXml(XElement a, XElement b, string parentPath, List<StructuralChange> c)
    {
        string path = string.IsNullOrEmpty(parentPath)
            ? a.Name.LocalName
            : $"{parentPath}/{a.Name.LocalName}";

        if (a.Name != b.Name)
        {
            c.Add(new StructuralChange(path, ChangeType.Modified,
                $"<{a.Name.LocalName}>", $"<{b.Name.LocalName}>"));
            return;
        }

        // Attributes
        HashSet<string> attrs = [];
        foreach (XAttribute at in a.Attributes()) attrs.Add(at.Name.LocalName);
        foreach (XAttribute at in b.Attributes()) attrs.Add(at.Name.LocalName);
        foreach (string attr in attrs.OrderBy(x => x))
        {
            string ap = $"{path}[{attr}]";
            string? vA = a.Attribute(attr)?.Value;
            string? vB = b.Attribute(attr)?.Value;
            if (vA is not null && vB is not null && vA != vB)
                c.Add(new StructuralChange(ap, ChangeType.Modified, vA, vB));
            else if (vA is not null && vB is null)
                c.Add(new StructuralChange(ap, ChangeType.Removed, vA, null));
            else if (vA is null && vB is not null)
                c.Add(new StructuralChange(ap, ChangeType.Added, null, vB));
        }

        // Direct text content
        string tA = string.Join("", a.Nodes().OfType<XText>().Select(t => t.Value.Trim()));
        string tB = string.Join("", b.Nodes().OfType<XText>().Select(t => t.Value.Trim()));
        if (tA != tB)
        {
            if (!string.IsNullOrEmpty(tA) && !string.IsNullOrEmpty(tB))
                c.Add(new StructuralChange($"{path}/text()", ChangeType.Modified, tA, tB));
            else if (!string.IsNullOrEmpty(tA))
                c.Add(new StructuralChange($"{path}/text()", ChangeType.Removed, tA, null));
            else
                c.Add(new StructuralChange($"{path}/text()", ChangeType.Added, null, tB));
        }

        // Child elements (positional matching)
        List<XElement> chA = [.. a.Elements()];
        List<XElement> chB = [.. b.Elements()];
        for (int i = 0; i < Math.Max(chA.Count, chB.Count); i++)
        {
            if (i < chA.Count && i < chB.Count)
                WalkXml(chA[i], chB[i], path, c);
            else if (i < chA.Count)
                c.Add(new StructuralChange($"{path}/{chA[i].Name.LocalName}[{i}]",
                    ChangeType.Removed, Trunc(chA[i].ToString()), null));
            else
                c.Add(new StructuralChange($"{path}/{chB[i].Name.LocalName}[{i}]",
                    ChangeType.Added, null, Trunc(chB[i].ToString())));
        }
    }

    private static string Trunc(string s) =>
        s.Length > 80 ? string.Concat(s.AsSpan(0, 77), "...") : s;

    // ══════════════════════════════════════════════════════════════
    //  CSS Diff (grouped by selector, then by property)
    // ══════════════════════════════════════════════════════════════

    public static List<StructuralChange> ComputeCssDiff(string cssA, string cssB)
    {
        List<StructuralChange> changes = [];
        try
        {
            Dictionary<string, string> rulesA = ParseCssRules(cssA);
            Dictionary<string, string> rulesB = ParseCssRules(cssB);
            HashSet<string> selectors = [.. rulesA.Keys, .. rulesB.Keys];

            foreach (string sel in selectors.OrderBy(s => s))
            {
                bool inA = rulesA.TryGetValue(sel, out string? bodyA);
                bool inB = rulesB.TryGetValue(sel, out string? bodyB);

                if (inA && inB)
                {
                    Dictionary<string, string> pA = ParseProps(bodyA!);
                    Dictionary<string, string> pB = ParseProps(bodyB!);
                    HashSet<string> props = [.. pA.Keys, .. pB.Keys];
                    foreach (string prop in props.OrderBy(p => p))
                    {
                        string pp = $"{sel} > {prop}";
                        bool hA = pA.TryGetValue(prop, out string? pvA);
                        bool hB = pB.TryGetValue(prop, out string? pvB);
                        if (hA && hB && pvA != pvB)
                            changes.Add(new StructuralChange(pp, ChangeType.Modified, pvA, pvB));
                        else if (hA && !hB)
                            changes.Add(new StructuralChange(pp, ChangeType.Removed, pvA, null));
                        else if (!hA && hB)
                            changes.Add(new StructuralChange(pp, ChangeType.Added, null, pvB));
                    }
                }
                else if (inA)
                    changes.Add(new StructuralChange(sel, ChangeType.Removed, Trunc(bodyA!), null));
                else
                    changes.Add(new StructuralChange(sel, ChangeType.Added, null, Trunc(bodyB!)));
            }
        }
        catch (Exception ex)
        {
            changes.Add(new StructuralChange("Parse error", ChangeType.Removed, ex.Message, null));
        }

        return changes;
    }

    private static Dictionary<string, string> ParseCssRules(string css)
    {
        Dictionary<string, string> rules = new(StringComparer.Ordinal);
        css = Regex.Replace(css, @"/\*.*?\*/", "", RegexOptions.Singleline);
        int i = 0;
        while (i < css.Length)
        {
            int open = css.IndexOf('{', i);
            if (open < 0) break;
            string selector = css[i..open].Trim();
            if (string.IsNullOrEmpty(selector)) { i = open + 1; continue; }

            int depth = 1, j = open + 1;
            while (j < css.Length && depth > 0)
            {
                if (css[j] == '{') depth++;
                else if (css[j] == '}') depth--;
                j++;
            }

            rules[selector] = css[(open + 1)..(j - 1)].Trim();
            i = j;
        }

        return rules;
    }

    private static Dictionary<string, string> ParseProps(string body)
    {
        Dictionary<string, string> props = new(StringComparer.OrdinalIgnoreCase);
        foreach (string decl in body.Split(';',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int colon = decl.IndexOf(':');
            if (colon < 0) continue;
            string name = decl[..colon].Trim();
            string val = decl[(colon + 1)..].Trim();
            if (!string.IsNullOrEmpty(name)) props[name] = val;
        }

        return props;
    }
}
