using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace Rowles.Toolbox.Core.Inspection;

public static class JsonPathTesterCore
{
    public const string DefaultJson = "{\n  \"store\": {\n    \"book\": [\n      {\n        \"category\": \"reference\",\n        \"author\": \"Nigel Rees\",\n        \"title\": \"Sayings of the Century\",\n        \"price\": 8.95\n      },\n      {\n        \"category\": \"fiction\",\n        \"author\": \"Evelyn Waugh\",\n        \"title\": \"Sword of Honour\",\n        \"price\": 12.99\n      },\n      {\n        \"category\": \"fiction\",\n        \"author\": \"Herman Melville\",\n        \"title\": \"Moby Dick\",\n        \"isbn\": \"0-553-21311-3\",\n        \"price\": 8.99\n      },\n      {\n        \"category\": \"fiction\",\n        \"author\": \"J. R. R. Tolkien\",\n        \"title\": \"The Lord of the Rings\",\n        \"isbn\": \"0-395-19395-8\",\n        \"price\": 22.99\n      }\n    ],\n    \"bicycle\": {\n      \"color\": \"red\",\n      \"price\": 19.95\n    }\n  }\n}";

    public const string DefaultPath = "$..book[*].author";

    public static readonly JsonSerializerOptions IndentedOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static readonly List<JsonPathExample> Examples =
    [
        new("$", "Root element"),
        new("$.store.book", "Child access"),
        new("$.store.book[0]", "First array element"),
        new("$.store.book[*]", "All books"),
        new("$.store.book[-1]", "Last element"),
        new("$.store.book[0:2]", "Slice (first two)"),
        new("$..author", "All authors (recursive)"),
        new("$..book[?(@.isbn)]", "Books with isbn"),
        new("$..book[?(@.price<10)]", "Price filter (basic)"),
    ];

    public sealed record JsonPathResult(string Path, string Value, JsonValueKind ValueKind);
    public sealed record JsonPathExample(string Expression, string Description);

    public abstract record PathToken;
    public sealed record RootToken : PathToken;
    public sealed record PropertyToken(string Name) : PathToken;
    public sealed record RecursiveDescentToken(string Name) : PathToken;
    public sealed record ArrayIndexToken(int Index) : PathToken;
    public sealed record WildcardToken : PathToken;
    public sealed record SliceToken(int? Start, int? End) : PathToken;
    public sealed record FilterExistsToken(string Property) : PathToken;
    public sealed record FilterValueToken(string Property, string Operator, string RawValue) : PathToken;

    public static List<JsonPathResult> EvaluateJsonPath(JsonElement root, string path)
    {
        List<PathToken> tokens = TokenizePath(path);
        List<(string Path, JsonElement Element)> current = [("$", root)];

        foreach (PathToken token in tokens)
        {
            List<(string Path, JsonElement Element)> next = [];

            foreach ((string currentPath, JsonElement element) in current)
            {
                switch (token)
                {
                    case RootToken:
                        next.Add(("$", root));
                        break;

                    case PropertyToken prop:
                        if (element.ValueKind == JsonValueKind.Object &&
                            element.TryGetProperty(prop.Name, out JsonElement propValue))
                        {
                            next.Add(($"{currentPath}.{prop.Name}", propValue));
                        }
                        break;

                    case RecursiveDescentToken rd:
                        FindRecursive(element, rd.Name, currentPath, next);
                        break;

                    case ArrayIndexToken idx:
                        if (element.ValueKind == JsonValueKind.Array)
                        {
                            int arrayLen = element.GetArrayLength();
                            int actualIndex = idx.Index < 0 ? arrayLen + idx.Index : idx.Index;
                            if (actualIndex >= 0 && actualIndex < arrayLen)
                            {
                                next.Add(($"{currentPath}[{actualIndex}]", element[actualIndex]));
                            }
                        }
                        break;

                    case WildcardToken:
                        if (element.ValueKind == JsonValueKind.Array)
                        {
                            int arrIdx = 0;
                            foreach (JsonElement item in element.EnumerateArray())
                            {
                                next.Add(($"{currentPath}[{arrIdx}]", item));
                                arrIdx++;
                            }
                        }
                        else if (element.ValueKind == JsonValueKind.Object)
                        {
                            foreach (JsonProperty p in element.EnumerateObject())
                            {
                                next.Add(($"{currentPath}.{p.Name}", p.Value));
                            }
                        }
                        break;

                    case SliceToken slice:
                        if (element.ValueKind == JsonValueKind.Array)
                        {
                            int len = element.GetArrayLength();
                            int start = slice.Start ?? 0;
                            int end = slice.End ?? len;
                            if (start < 0) start = len + start;
                            if (end < 0) end = len + end;
                            start = Math.Clamp(start, 0, len);
                            end = Math.Clamp(end, 0, len);
                            for (int si = start; si < end; si++)
                            {
                                next.Add(($"{currentPath}[{si}]", element[si]));
                            }
                        }
                        break;

                    case FilterExistsToken filter:
                        if (element.ValueKind == JsonValueKind.Array)
                        {
                            int fi = 0;
                            foreach (JsonElement item in element.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Object &&
                                    item.TryGetProperty(filter.Property, out _))
                                {
                                    next.Add(($"{currentPath}[{fi}]", item));
                                }
                                fi++;
                            }
                        }
                        break;

                    case FilterValueToken filter:
                        if (element.ValueKind == JsonValueKind.Array)
                        {
                            int fvi = 0;
                            foreach (JsonElement item in element.EnumerateArray())
                            {
                                if (item.ValueKind == JsonValueKind.Object &&
                                    item.TryGetProperty(filter.Property, out JsonElement filterVal) &&
                                    MatchesFilter(filterVal, filter.Operator, filter.RawValue))
                                {
                                    next.Add(($"{currentPath}[{fvi}]", item));
                                }
                                fvi++;
                            }
                        }
                        break;
                }
            }

            current = next;
        }

        List<JsonPathResult> results = [];
        foreach ((string p, JsonElement el) in current)
        {
            results.Add(new JsonPathResult(p, FormatValue(el), el.ValueKind));
        }
        return results;
    }

    private static void FindRecursive(JsonElement element, string propertyName, string currentPath,
        List<(string Path, JsonElement Element)> results)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty prop in element.EnumerateObject())
            {
                if (prop.Name == propertyName)
                {
                    results.Add(($"{currentPath}.{prop.Name}", prop.Value));
                }
                FindRecursive(prop.Value, propertyName, $"{currentPath}.{prop.Name}", results);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            int idx = 0;
            foreach (JsonElement item in element.EnumerateArray())
            {
                FindRecursive(item, propertyName, $"{currentPath}[{idx}]", results);
                idx++;
            }
        }
    }

    private static bool MatchesFilter(JsonElement value, string op, string rawValue)
    {
        if (value.ValueKind == JsonValueKind.Number &&
            double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double numCompare))
        {
            double numValue = value.GetDouble();
            return op switch
            {
                "==" => Math.Abs(numValue - numCompare) < 0.0001,
                "!=" => Math.Abs(numValue - numCompare) >= 0.0001,
                "<" => numValue < numCompare,
                ">" => numValue > numCompare,
                "<=" => numValue <= numCompare,
                ">=" => numValue >= numCompare,
                _ => false
            };
        }

        string strValue = value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : value.GetRawText();

        return op switch
        {
            "==" => string.Equals(strValue, rawValue, StringComparison.Ordinal),
            "!=" => !string.Equals(strValue, rawValue, StringComparison.Ordinal),
            _ => false
        };
    }

    public static List<PathToken> TokenizePath(string path)
    {
        List<PathToken> tokens = [];
        int pos = 0;

        SkipWhitespace(path, ref pos);

        if (pos < path.Length && path[pos] == '$')
        {
            tokens.Add(new RootToken());
            pos++;
        }

        while (pos < path.Length)
        {
            if (path[pos] == '.')
            {
                pos++;
                if (pos < path.Length && path[pos] == '.')
                {
                    pos++;
                    string name = ReadIdentifier(path, ref pos);
                    if (name.Length > 0)
                    {
                        tokens.Add(new RecursiveDescentToken(name));
                    }
                }
                else
                {
                    string name = ReadIdentifier(path, ref pos);
                    if (name.Length > 0)
                    {
                        tokens.Add(new PropertyToken(name));
                    }
                }
            }
            else if (path[pos] == '[')
            {
                pos++;
                SkipWhitespace(path, ref pos);

                if (pos < path.Length && path[pos] == '*')
                {
                    tokens.Add(new WildcardToken());
                    pos++;
                    SkipWhitespace(path, ref pos);
                    if (pos < path.Length && path[pos] == ']') pos++;
                }
                else if (pos < path.Length && path[pos] == '?')
                {
                    pos++;
                    SkipWhitespace(path, ref pos);
                    if (pos < path.Length && path[pos] == '(') pos++;
                    SkipWhitespace(path, ref pos);
                    if (pos < path.Length && path[pos] == '@') pos++;
                    if (pos < path.Length && path[pos] == '.') pos++;

                    string propName = ReadIdentifier(path, ref pos);
                    SkipWhitespace(path, ref pos);

                    if (pos < path.Length && (path[pos] == ')' || path[pos] == ']'))
                    {
                        tokens.Add(new FilterExistsToken(propName));
                    }
                    else
                    {
                        string op = ReadOperator(path, ref pos);
                        SkipWhitespace(path, ref pos);
                        string filterValue = ReadFilterValue(path, ref pos);
                        tokens.Add(new FilterValueToken(propName, op, filterValue));
                    }

                    while (pos < path.Length && path[pos] != ']') pos++;
                    if (pos < path.Length) pos++;
                }
                else if (pos < path.Length && (path[pos] == '\'' || path[pos] == '"'))
                {
                    char quote = path[pos];
                    pos++;
                    int start = pos;
                    while (pos < path.Length && path[pos] != quote) pos++;
                    string name = path[start..pos];
                    if (pos < path.Length) pos++;
                    SkipWhitespace(path, ref pos);
                    if (pos < path.Length && path[pos] == ']') pos++;
                    tokens.Add(new PropertyToken(name));
                }
                else
                {
                    int start = pos;
                    while (pos < path.Length && path[pos] != ']') pos++;
                    string content = path[start..pos].Trim();
                    if (pos < path.Length) pos++;

                    if (content.Contains(':'))
                    {
                        string[] parts = content.Split(':');
                        int? sliceStart = parts[0].Trim().Length > 0
                            ? int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture)
                            : null;
                        int? sliceEnd = parts.Length > 1 && parts[1].Trim().Length > 0
                            ? int.Parse(parts[1].Trim(), CultureInfo.InvariantCulture)
                            : null;
                        tokens.Add(new SliceToken(sliceStart, sliceEnd));
                    }
                    else if (int.TryParse(content, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index))
                    {
                        tokens.Add(new ArrayIndexToken(index));
                    }
                    else
                    {
                        throw new FormatException($"Invalid bracket content: '{content}'");
                    }
                }
            }
            else
            {
                pos++;
            }
        }

        return tokens;
    }

    private static string ReadIdentifier(string path, ref int pos)
    {
        int start = pos;
        while (pos < path.Length && (char.IsLetterOrDigit(path[pos]) || path[pos] == '_' || path[pos] == '-'))
        {
            pos++;
        }
        return path[start..pos];
    }

    private static string ReadOperator(string path, ref int pos)
    {
        int start = pos;
        while (pos < path.Length && (path[pos] == '=' || path[pos] == '!' || path[pos] == '<' || path[pos] == '>'))
        {
            pos++;
        }
        return path[start..pos];
    }

    private static string ReadFilterValue(string path, ref int pos)
    {
        SkipWhitespace(path, ref pos);

        if (pos < path.Length && (path[pos] == '\'' || path[pos] == '"'))
        {
            char quote = path[pos];
            pos++;
            int start = pos;
            while (pos < path.Length && path[pos] != quote) pos++;
            string filterVal = path[start..pos];
            if (pos < path.Length) pos++;
            return filterVal;
        }

        int valStart = pos;
        while (pos < path.Length && path[pos] != ')' && path[pos] != ']')
        {
            pos++;
        }
        return path[valStart..pos].Trim();
    }

    private static void SkipWhitespace(string path, ref int pos)
    {
        while (pos < path.Length && char.IsWhiteSpace(path[pos])) pos++;
    }

    public static MarkupString SyntaxHighlightJson(string json)
    {
        StringBuilder sb = new();
        Stack<bool> contextStack = new();
        bool expectingKey = false;
        int i = 0;

        while (i < json.Length)
        {
            char c = json[i];

            if (c == '"')
            {
                int start = i;
                i++;
                while (i < json.Length)
                {
                    if (json[i] == '\\')
                    {
                        i += 2;
                    }
                    else if (json[i] == '"')
                    {
                        i++;
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }

                string encoded = System.Net.WebUtility.HtmlEncode(json[start..i]);

                if (expectingKey)
                {
                    sb.Append("<span class=\"text-blue-600 dark:text-blue-400\">");
                    sb.Append(encoded);
                    sb.Append("</span>");
                    expectingKey = false;
                }
                else
                {
                    sb.Append("<span class=\"text-green-600 dark:text-green-400\">");
                    sb.Append(encoded);
                    sb.Append("</span>");
                }
            }
            else if (c == '{')
            {
                sb.Append("<span class=\"text-gray-700 dark:text-gray-300\">{</span>");
                contextStack.Push(true);
                expectingKey = true;
                i++;
            }
            else if (c == '}')
            {
                sb.Append("<span class=\"text-gray-700 dark:text-gray-300\">}</span>");
                if (contextStack.Count > 0) contextStack.Pop();
                expectingKey = false;
                i++;
            }
            else if (c == '[')
            {
                sb.Append("<span class=\"text-gray-700 dark:text-gray-300\">[</span>");
                contextStack.Push(false);
                expectingKey = false;
                i++;
            }
            else if (c == ']')
            {
                sb.Append("<span class=\"text-gray-700 dark:text-gray-300\">]</span>");
                if (contextStack.Count > 0) contextStack.Pop();
                expectingKey = false;
                i++;
            }
            else if (c == ':')
            {
                sb.Append("<span class=\"text-gray-700 dark:text-gray-300\">:</span>");
                expectingKey = false;
                i++;
            }
            else if (c == ',')
            {
                sb.Append("<span class=\"text-gray-700 dark:text-gray-300\">,</span>");
                expectingKey = contextStack.Count > 0 && contextStack.Peek();
                i++;
            }
            else if (char.IsDigit(c) || (c == '-' && i + 1 < json.Length && char.IsDigit(json[i + 1])))
            {
                int numStart = i;
                if (json[i] == '-') i++;
                while (i < json.Length && char.IsDigit(json[i])) i++;
                if (i < json.Length && json[i] == '.')
                {
                    i++;
                    while (i < json.Length && char.IsDigit(json[i])) i++;
                }
                if (i < json.Length && (json[i] == 'e' || json[i] == 'E'))
                {
                    i++;
                    if (i < json.Length && (json[i] == '+' || json[i] == '-')) i++;
                    while (i < json.Length && char.IsDigit(json[i])) i++;
                }
                sb.Append("<span class=\"text-orange-600 dark:text-orange-400\">");
                sb.Append(json[numStart..i]);
                sb.Append("</span>");
            }
            else if (i + 4 <= json.Length && json[i] == 't' && json[i + 1] == 'r' && json[i + 2] == 'u' && json[i + 3] == 'e')
            {
                sb.Append("<span class=\"text-purple-600 dark:text-purple-400\">true</span>");
                i += 4;
            }
            else if (i + 5 <= json.Length && json[i] == 'f' && json[i + 1] == 'a' && json[i + 2] == 'l' && json[i + 3] == 's' && json[i + 4] == 'e')
            {
                sb.Append("<span class=\"text-purple-600 dark:text-purple-400\">false</span>");
                i += 5;
            }
            else if (i + 4 <= json.Length && json[i] == 'n' && json[i + 1] == 'u' && json[i + 2] == 'l' && json[i + 3] == 'l')
            {
                sb.Append("<span class=\"text-gray-500\">null</span>");
                i += 4;
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }

        return new MarkupString(sb.ToString());
    }

    public static string FormatValue(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
        {
            return JsonSerializer.Serialize(element, IndentedOptions);
        }
        return element.GetRawText();
    }

    public static string GetTypeLabel(JsonValueKind kind)
    {
        return kind switch
        {
            JsonValueKind.Object => "Object",
            JsonValueKind.Array => "Array",
            JsonValueKind.String => "String",
            JsonValueKind.Number => "Number",
            JsonValueKind.True or JsonValueKind.False => "Boolean",
            JsonValueKind.Null => "Null",
            _ => "Unknown"
        };
    }

    public static string GetTypeBadgeCss(JsonValueKind kind)
    {
        return kind switch
        {
            JsonValueKind.Object => "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300",
            JsonValueKind.Array => "bg-purple-100 dark:bg-purple-900 text-purple-700 dark:text-purple-300",
            JsonValueKind.String => "bg-green-100 dark:bg-green-900 text-green-700 dark:text-green-300",
            JsonValueKind.Number => "bg-orange-100 dark:bg-orange-900 text-orange-700 dark:text-orange-300",
            JsonValueKind.True or JsonValueKind.False => "bg-indigo-100 dark:bg-indigo-900 text-indigo-700 dark:text-indigo-300",
            JsonValueKind.Null => "bg-gray-100 dark:bg-gray-800 text-gray-500",
            _ => "bg-gray-100 dark:bg-gray-800 text-gray-500"
        };
    }
}
