using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Rowles.Toolbox.Core.DataFormats;

public static class YamlJsonCore
{
    // -- YAML Parser --

    private record struct YLine(int Indent, string Text);

    public static object? ParseYamlDocument(string yaml)
    {
        List<YLine> lines = TokenizeYaml(yaml);
        if (lines.Count == 0) return new Dictionary<string, object?>();
        int pos = 0;
        return ParseBlock(lines, ref pos, 0);
    }

    public static string UnindentSample(string text)
    {
        string[] lines = text.Split('\n');
        int minIndent = int.MaxValue;
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            int spaces = line.Length - line.TrimStart().Length;
            if (spaces < minIndent) minIndent = spaces;
        }
        if (minIndent == int.MaxValue) minIndent = 0;
        StringBuilder sb = new();
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            sb.AppendLine(line.Length >= minIndent ? line[minIndent..] : line);
        }
        return sb.ToString().Trim();
    }

    private static List<YLine> TokenizeYaml(string yaml)
    {
        List<YLine> result = [];
        foreach (string raw in yaml.Split('\n'))
        {
            string line = raw.TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line)) continue;
            int indent = line.Length - line.TrimStart().Length;
            string trimmed = line.TrimStart();
            if (trimmed.StartsWith('#')) continue;
            int commentIdx = FindInlineComment(trimmed);
            if (commentIdx >= 0) trimmed = trimmed[..commentIdx].TrimEnd();
            if (string.IsNullOrEmpty(trimmed)) continue;
            result.Add(new YLine(indent, trimmed));
        }
        return result;
    }

    private static int FindInlineComment(string text)
    {
        bool inSingle = false;
        bool inDouble = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\'' && !inDouble) inSingle = !inSingle;
            else if (c == '"' && !inSingle) inDouble = !inDouble;
            else if (c == '#' && !inSingle && !inDouble && i > 0 && text[i - 1] == ' ')
                return i - 1;
        }
        return -1;
    }

    private static object? ParseBlock(List<YLine> lines, ref int pos, int minIndent)
    {
        if (pos >= lines.Count || lines[pos].Indent < minIndent) return null;

        YLine first = lines[pos];

        if (first.Text.StartsWith("- "))
            return ParseSequence(lines, ref pos, first.Indent);

        if (first.Text.StartsWith('['))
        {
            object? result = ParseFlowSequence(first.Text);
            pos++;
            return result;
        }
        if (first.Text.StartsWith('{'))
        {
            object? result = ParseFlowMapping(first.Text);
            pos++;
            return result;
        }

        int colon = FindMappingColon(first.Text);
        if (colon > 0)
            return ParseMapping(lines, ref pos, first.Indent);

        pos++;
        return ParseScalar(first.Text);
    }

    private static Dictionary<string, object?> ParseMapping(List<YLine> lines, ref int pos, int indent)
    {
        Dictionary<string, object?> map = new();
        while (pos < lines.Count && lines[pos].Indent == indent)
        {
            string content = lines[pos].Text;
            if (content.StartsWith("- ")) break;
            int colon = FindMappingColon(content);
            if (colon <= 0) break;

            string key = UnquoteString(content[..colon].Trim());
            string valStr = content[(colon + 1)..].TrimStart();

            if (string.IsNullOrEmpty(valStr))
            {
                pos++;
                if (pos < lines.Count && lines[pos].Indent > indent)
                    map[key] = ParseBlock(lines, ref pos, lines[pos].Indent);
                else
                    map[key] = null;
            }
            else if (valStr.StartsWith('['))
            {
                map[key] = ParseFlowSequence(valStr);
                pos++;
            }
            else if (valStr.StartsWith('{'))
            {
                map[key] = ParseFlowMapping(valStr);
                pos++;
            }
            else
            {
                map[key] = ParseScalar(valStr);
                pos++;
            }
        }
        return map;
    }

    private static List<object?> ParseSequence(List<YLine> lines, ref int pos, int indent)
    {
        List<object?> list = [];
        while (pos < lines.Count && lines[pos].Indent == indent && lines[pos].Text.StartsWith("- "))
        {
            string rest = lines[pos].Text.Length > 2 ? lines[pos].Text[2..] : string.Empty;
            int virtualIndent = indent + 2;

            if (string.IsNullOrWhiteSpace(rest))
            {
                pos++;
                if (pos < lines.Count && lines[pos].Indent >= virtualIndent)
                    list.Add(ParseBlock(lines, ref pos, virtualIndent));
                else
                    list.Add(null);
            }
            else
            {
                rest = rest.TrimStart();
                if (rest.StartsWith('['))
                {
                    list.Add(ParseFlowSequence(rest));
                    pos++;
                }
                else if (rest.StartsWith('{'))
                {
                    list.Add(ParseFlowMapping(rest));
                    pos++;
                }
                else
                {
                    int colon = FindMappingColon(rest);
                    if (colon > 0)
                    {
                        list.Add(ParseInlineMapping(lines, ref pos, rest, indent, virtualIndent));
                    }
                    else
                    {
                        list.Add(ParseScalar(rest));
                        pos++;
                    }
                }
            }
        }
        return list;
    }

    private static Dictionary<string, object?> ParseInlineMapping(List<YLine> lines, ref int pos, string firstContent, int dashIndent, int virtualIndent)
    {
        Dictionary<string, object?> map = new();
        ParseKeyValue(map, lines, ref pos, firstContent, virtualIndent);

        while (pos < lines.Count && lines[pos].Indent == virtualIndent)
        {
            string content = lines[pos].Text;
            if (content.StartsWith("- ")) break;
            int colon = FindMappingColon(content);
            if (colon <= 0) break;
            ParseKeyValue(map, lines, ref pos, content, virtualIndent);
        }
        return map;
    }

    private static void ParseKeyValue(Dictionary<string, object?> map, List<YLine> lines, ref int pos, string content, int virtualIndent)
    {
        int colon = FindMappingColon(content);
        if (colon <= 0) { pos++; return; }

        string key = UnquoteString(content[..colon].Trim());
        string valStr = content[(colon + 1)..].TrimStart();

        if (string.IsNullOrEmpty(valStr))
        {
            pos++;
            if (pos < lines.Count && lines[pos].Indent > virtualIndent)
                map[key] = ParseBlock(lines, ref pos, lines[pos].Indent);
            else
                map[key] = null;
        }
        else if (valStr.StartsWith('['))
        {
            map[key] = ParseFlowSequence(valStr);
            pos++;
        }
        else if (valStr.StartsWith('{'))
        {
            map[key] = ParseFlowMapping(valStr);
            pos++;
        }
        else
        {
            map[key] = ParseScalar(valStr);
            pos++;
        }
    }

    private static int FindMappingColon(string text)
    {
        bool inSingle = false;
        bool inDouble = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\'' && !inDouble) inSingle = !inSingle;
            else if (c == '"' && !inSingle) inDouble = !inDouble;
            else if (c == ':' && !inSingle && !inDouble && (i + 1 >= text.Length || text[i + 1] == ' '))
                return i;
        }
        return -1;
    }

    private static List<object?> ParseFlowSequence(string text)
    {
        text = text.Trim();
        if (!text.StartsWith('[') || !text.EndsWith(']'))
            throw new FormatException("Invalid flow sequence.");
        string inner = text[1..^1].Trim();
        if (string.IsNullOrEmpty(inner)) return [];
        List<object?> result = [];
        foreach (string item in SplitFlowItems(inner))
        {
            string t = item.Trim();
            if (t.StartsWith('[')) result.Add(ParseFlowSequence(t));
            else if (t.StartsWith('{')) result.Add(ParseFlowMapping(t));
            else result.Add(ParseScalar(t));
        }
        return result;
    }

    private static Dictionary<string, object?> ParseFlowMapping(string text)
    {
        text = text.Trim();
        if (!text.StartsWith('{') || !text.EndsWith('}'))
            throw new FormatException("Invalid flow mapping.");
        string inner = text[1..^1].Trim();
        if (string.IsNullOrEmpty(inner)) return new();
        Dictionary<string, object?> result = new();
        foreach (string item in SplitFlowItems(inner))
        {
            int colon = item.IndexOf(':');
            if (colon <= 0) continue;
            string key = UnquoteString(item[..colon].Trim());
            string val = item[(colon + 1)..].Trim();
            if (val.StartsWith('[')) result[key] = ParseFlowSequence(val);
            else if (val.StartsWith('{')) result[key] = ParseFlowMapping(val);
            else result[key] = ParseScalar(val);
        }
        return result;
    }

    private static List<string> SplitFlowItems(string text)
    {
        List<string> items = [];
        int depth = 0;
        int start = 0;
        bool inSingle = false;
        bool inDouble = false;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\'' && !inDouble) inSingle = !inSingle;
            else if (c == '"' && !inSingle) inDouble = !inDouble;
            else if (!inSingle && !inDouble)
            {
                if (c is '[' or '{') depth++;
                else if (c is ']' or '}') depth--;
                else if (c == ',' && depth == 0)
                {
                    items.Add(text[start..i]);
                    start = i + 1;
                }
            }
        }
        if (start < text.Length) items.Add(text[start..]);
        return items;
    }

    private static object? ParseScalar(string text)
    {
        text = text.Trim();
        if (string.IsNullOrEmpty(text) || text is "null" or "Null" or "NULL" or "~") return null;
        if ((text.StartsWith('"') && text.EndsWith('"')) || (text.StartsWith('\'') && text.EndsWith('\'')))
            return text[1..^1];
        if (text is "true" or "True" or "TRUE" or "yes" or "Yes" or "YES") return true;
        if (text is "false" or "False" or "FALSE" or "no" or "No" or "NO") return false;
        if (long.TryParse(text, CultureInfo.InvariantCulture, out long l)) return l;
        if (double.TryParse(text, CultureInfo.InvariantCulture, out double d)) return d;
        return text;
    }

    private static string UnquoteString(string text)
    {
        if (text.Length >= 2 &&
            ((text.StartsWith('"') && text.EndsWith('"')) || (text.StartsWith('\'') && text.EndsWith('\''))))
            return text[1..^1];
        return text;
    }

    // -- JSON to YAML Generator --

    public static void WriteYamlElement(StringBuilder sb, JsonElement el, int indent)
    {
        string pad = new(' ', indent);
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                bool anyProp = false;
                foreach (JsonProperty prop in el.EnumerateObject())
                {
                    anyProp = true;
                    sb.Append(pad);
                    sb.Append(NeedsYamlQuoting(prop.Name) ? $"\"{prop.Name}\"" : prop.Name);
                    sb.Append(':');
                    if (prop.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        sb.AppendLine();
                        WriteYamlElement(sb, prop.Value, indent + 2);
                    }
                    else
                    {
                        sb.Append(' ');
                        WriteYamlScalar(sb, prop.Value);
                        sb.AppendLine();
                    }
                }
                if (!anyProp) { sb.Append(pad); sb.AppendLine("{}"); }
                break;

            case JsonValueKind.Array:
                bool anyItem = false;
                foreach (JsonElement item in el.EnumerateArray())
                {
                    anyItem = true;
                    sb.Append(pad);
                    sb.Append("- ");
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        bool first = true;
                        foreach (JsonProperty prop in item.EnumerateObject())
                        {
                            if (!first) { sb.Append(pad); sb.Append("  "); }
                            first = false;
                            sb.Append(NeedsYamlQuoting(prop.Name) ? $"\"{prop.Name}\"" : prop.Name);
                            sb.Append(':');
                            if (prop.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                            {
                                sb.AppendLine();
                                WriteYamlElement(sb, prop.Value, indent + 4);
                            }
                            else
                            {
                                sb.Append(' ');
                                WriteYamlScalar(sb, prop.Value);
                                sb.AppendLine();
                            }
                        }
                        if (first) sb.AppendLine("{}");
                    }
                    else if (item.ValueKind == JsonValueKind.Array)
                    {
                        sb.AppendLine();
                        WriteYamlElement(sb, item, indent + 2);
                    }
                    else
                    {
                        WriteYamlScalar(sb, item);
                        sb.AppendLine();
                    }
                }
                if (!anyItem) { sb.Append(pad); sb.AppendLine("[]"); }
                break;

            default:
                sb.Append(pad);
                WriteYamlScalar(sb, el);
                sb.AppendLine();
                break;
        }
    }

    private static void WriteYamlScalar(StringBuilder sb, JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.String:
                string s = el.GetString() ?? "";
                sb.Append(NeedsYamlQuoting(s) ? $"\"{s}\"" : s);
                break;
            case JsonValueKind.Number:
                sb.Append(el.GetRawText());
                break;
            case JsonValueKind.True:
                sb.Append("true");
                break;
            case JsonValueKind.False:
                sb.Append("false");
                break;
            case JsonValueKind.Null:
                sb.Append("null");
                break;
            default:
                sb.Append(el.GetRawText());
                break;
        }
    }

    private static bool NeedsYamlQuoting(string value)
    {
        if (string.IsNullOrEmpty(value)) return true;
        if (value.Contains(':') || value.Contains('#') || value.Contains('{') || value.Contains('}') ||
            value.Contains('[') || value.Contains(']') || value.Contains(',') || value.Contains('&') ||
            value.Contains('*') || value.Contains('!') || value.Contains('|') || value.Contains('>') ||
            value.Contains('\'') || value.Contains('"') || value.Contains('%') || value.Contains('@'))
            return true;
        if (value is "true" or "false" or "null" or "yes" or "no" or "True" or "False" or "Null" or "Yes" or "No")
            return true;
        if (double.TryParse(value, CultureInfo.InvariantCulture, out _)) return true;
        return false;
    }
}
