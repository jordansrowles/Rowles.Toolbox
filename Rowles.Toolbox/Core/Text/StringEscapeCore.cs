using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Rowles.Toolbox.Core.Text;

public static class StringEscapeCore
{
    public static string Escape(string input, string mode)
    {
        return mode switch
        {
            "Json" => EscapeJson(input),
            "XmlHtml" => HttpUtility.HtmlEncode(input),
            "CSharp" => EscapeCSharp(input),
            "Regex" => Regex.Escape(input),
            "Url" => Uri.EscapeDataString(input),
            _ => input
        };
    }

    public static string Unescape(string input, string mode)
    {
        return mode switch
        {
            "Json" => UnescapeJson(input),
            "XmlHtml" => HttpUtility.HtmlDecode(input),
            "CSharp" => UnescapeCSharp(input),
            "Regex" => Regex.Unescape(input),
            "Url" => Uri.UnescapeDataString(input),
            _ => input
        };
    }

    public static string EscapeJson(string input)
    {
        string serialized = JsonSerializer.Serialize(input);
        return serialized[1..^1];
    }

    public static string UnescapeJson(string input)
    {
        string json = $"\"{input}\"";
        string? result = JsonSerializer.Deserialize<string>(json);
        return result ?? string.Empty;
    }

    public static string EscapeCSharp(string input)
    {
        StringBuilder sb = new(input.Length);
        foreach (char c in input)
        {
            string escaped = c switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\0' => "\\0",
                '\a' => "\\a",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\v' => "\\v",
                _ => c.ToString()
            };
            sb.Append(escaped);
        }
        return sb.ToString();
    }

    public static string UnescapeCSharp(string input)
    {
        StringBuilder sb = new(input.Length);
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '\\' && i + 1 < input.Length)
            {
                char next = input[i + 1];
                string unescaped = next switch
                {
                    '\\' => "\\",
                    '"' => "\"",
                    '0' => "\0",
                    'a' => "\a",
                    'b' => "\b",
                    'f' => "\f",
                    'n' => "\n",
                    'r' => "\r",
                    't' => "\t",
                    'v' => "\v",
                    _ => $"\\{next}"
                };
                sb.Append(unescaped);
                i++;
            }
            else
            {
                sb.Append(input[i]);
            }
        }
        return sb.ToString();
    }
}
