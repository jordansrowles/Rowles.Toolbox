using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.Developer;

public static class WebLintersCore
{
    public sealed record LintResult(string Severity, int Line, string Rule, string Message);

    public static List<LintResult> LintHtml(string input)
    {
        List<LintResult> results = new();
        string[] lines = input.Split('\n');
        string lower = input.ToLowerInvariant();

        if (!Regex.IsMatch(lower, @"<!doctype\s+html\s*>"))
            results.Add(new("error", 1, "missing-doctype", "Missing <!DOCTYPE html> declaration"));

        if (Regex.IsMatch(lower, @"<html(?:\s[^>]*)?>") && !Regex.IsMatch(lower, @"<html\s[^>]*lang\s*="))
            results.Add(new("warning", FindLine(lines, "<html"), "html-lang", "The <html> element should have a lang attribute"));

        if (!lower.Contains("<title"))
            results.Add(new("warning", 1, "missing-title", "Missing <title> tag in the document"));

        if (!Regex.IsMatch(lower, @"<meta\s[^>]*charset\s*="))
            results.Add(new("warning", 1, "missing-charset", "Missing <meta charset=\"...\"> declaration"));

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNum = i + 1;
            string line = lines[i];
            string lineLower = line.ToLowerInvariant();

            if (Regex.IsMatch(lineLower, @"<img\s") && !Regex.IsMatch(lineLower, @"\balt\s*="))
                results.Add(new("error", lineNum, "img-alt", "Image is missing an alt attribute"));

            if (Regex.IsMatch(lineLower, @"href\s*=\s*(""|')\1"))
                results.Add(new("warning", lineNum, "empty-href", "Empty href attribute detected"));

            if (Regex.IsMatch(lineLower, @"src\s*=\s*(""|')\1"))
                results.Add(new("warning", lineNum, "empty-src", "Empty src attribute detected"));

            if (Regex.IsMatch(lineLower, @"\bstyle\s*=\s*""[^""]+""") || Regex.IsMatch(lineLower, @"\bstyle\s*=\s*'[^']+'"))
                results.Add(new("warning", lineNum, "inline-style", "Inline styles detected — consider using CSS classes"));

            foreach (string tag in new[] { "center", "font", "marquee", "blink", "big", "strike" })
            {
                if (Regex.IsMatch(lineLower, $@"<{tag}[\s>]"))
                    results.Add(new("error", lineNum, "deprecated-tag", $"Deprecated <{tag}> tag — use CSS instead"));
            }
        }

        MatchCollection idMatches = Regex.Matches(input, @"\bid\s*=\s*[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        Dictionary<string, int> seenIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in idMatches)
        {
            string id = m.Groups[1].Value;
            if (seenIds.TryGetValue(id, out int firstLine))
                results.Add(new("error", FindLineOfMatch(lines, m.Index), "duplicate-id", $"Duplicate id \"{id}\" (first seen at line {firstLine})"));
            else
                seenIds[id] = FindLineOfMatch(lines, m.Index);
        }

        CheckUnclosedTags(lines, results);

        return results;
    }

    public static List<LintResult> LintCss(string input)
    {
        List<LintResult> results = new();
        string[] lines = input.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNum = i + 1;
            string line = lines[i];
            string trimmed = line.Trim();

            if (trimmed.Contains("!important"))
                results.Add(new("warning", lineNum, "no-important", "Avoid !important — it makes styles harder to override"));

            if (Regex.IsMatch(trimmed, @"(?<![a-zA-Z0-9\-_])\*\s*\{"))
                results.Add(new("warning", lineNum, "no-universal-selector", "Universal selector (*) can impact performance"));

            foreach (Match m in Regex.Matches(trimmed, @"#([a-fA-F0-9]+)\b"))
            {
                string hex = m.Groups[1].Value;
                if (hex.Length is not (3 or 4 or 6 or 8))
                    results.Add(new("error", lineNum, "invalid-hex", $"Invalid hex colour value: #{hex}"));
            }
            if (Regex.IsMatch(trimmed, @"#[^a-fA-F0-9;}\s][a-zA-Z0-9]*"))
                results.Add(new("error", lineNum, "invalid-hex", "Hex colour contains invalid characters"));

            if (Regex.IsMatch(trimmed, @"\bfloat\s*:\s*(left|right|none)", RegexOptions.IgnoreCase))
                results.Add(new("info", lineNum, "no-float", "Consider using flexbox or grid instead of float"));

            if (Regex.IsMatch(trimmed, @"(?<!\d)0(px|em|rem|%|pt|cm|mm|in|vh|vw)\b"))
                results.Add(new("info", lineNum, "zero-units", "Zero values don't need units — use 0 instead of 0px etc."));

            if (Regex.IsMatch(trimmed, @"-(webkit|moz|ms|o)-"))
                results.Add(new("info", lineNum, "vendor-prefix", "Vendor prefixes detected — consider using autoprefixer"));

            if (Regex.IsMatch(trimmed, @"^[a-zA-Z\-]+\s*:.*[^;{}/\*]$") && !trimmed.EndsWith("{") && !trimmed.EndsWith("}"))
                results.Add(new("warning", lineNum, "missing-semicolon", "Possible missing semicolon at end of declaration"));
        }

        foreach (Match m in Regex.Matches(input, @"[^}]+\{\s*\}", RegexOptions.Multiline))
        {
            int line = FindLineOfMatch(lines, m.Index);
            results.Add(new("warning", line, "empty-ruleset", "Empty CSS rule set — remove or add declarations"));
        }

        CheckDuplicateCssProperties(lines, results);

        return results;
    }

    public static List<LintResult> LintJs(string input)
    {
        List<LintResult> results = new();
        string[] lines = input.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNum = i + 1;
            string line = lines[i];
            string trimmed = line.Trim();

            if (trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("*"))
                continue;

            if (Regex.IsMatch(trimmed, @"\bvar\s+"))
                results.Add(new("warning", lineNum, "no-var", "Use let or const instead of var"));

            if (Regex.IsMatch(trimmed, @"(?<!=)={2}(?!=)"))
                results.Add(new("warning", lineNum, "eqeqeq", "Use === instead of == for strict equality"));
            if (Regex.IsMatch(trimmed, @"(?<!!)!={1}(?!=)"))
                results.Add(new("warning", lineNum, "eqeqeq", "Use !== instead of != for strict inequality"));

            if (Regex.IsMatch(trimmed, @"\bconsole\.(log|warn|error|info|debug)\s*\("))
                results.Add(new("warning", lineNum, "no-console", "Remove console statements before production"));

            if (Regex.IsMatch(trimmed, @"\beval\s*\("))
                results.Add(new("error", lineNum, "no-eval", "eval() is dangerous — it executes arbitrary code"));

            if (Regex.IsMatch(trimmed, @"\bwith\s*\("))
                results.Add(new("error", lineNum, "no-with", "with statement is deprecated and error-prone"));

            if (Regex.IsMatch(trimmed, @"\b(alert|confirm|prompt)\s*\("))
                results.Add(new("warning", lineNum, "no-alert", "Avoid browser dialog functions (alert/confirm/prompt)"));

            if (Regex.IsMatch(trimmed, @"\b(if|while)\s*\([^=]*(?<![=!<>])=(?!=)[^=]"))
                results.Add(new("warning", lineNum, "no-cond-assign", "Possible accidental assignment in conditional — did you mean === ?"));

            if (Regex.IsMatch(trimmed, @"\bdebugger\b"))
                results.Add(new("warning", lineNum, "no-debugger", "Remove debugger statements before production"));

            if (!string.IsNullOrWhiteSpace(trimmed)
                && !trimmed.EndsWith(";")
                && !trimmed.EndsWith("{")
                && !trimmed.EndsWith("}")
                && !trimmed.EndsWith(",")
                && !trimmed.EndsWith("(")
                && !trimmed.EndsWith(")")
                && !trimmed.StartsWith("//")
                && !trimmed.StartsWith("/*")
                && !trimmed.StartsWith("*")
                && !Regex.IsMatch(trimmed, @"^(if|else|for|while|do|switch|try|catch|finally|function|class|=>)\b")
                && !Regex.IsMatch(trimmed, @"^\s*\}?\s*(else|catch|finally)")
                && trimmed.Length > 2)
                results.Add(new("info", lineNum, "missing-semicolon", "Possible missing semicolon"));
        }

        foreach (Match m in Regex.Matches(input, @"catch\s*\([^)]*\)\s*\{\s*\}", RegexOptions.Multiline))
        {
            int line = FindLineOfMatch(lines, m.Index);
            results.Add(new("warning", line, "no-empty-catch", "Empty catch block — errors are silently swallowed"));
        }

        return results;
    }

    public static void CheckUnclosedTags(string[] lines, List<LintResult> results)
    {
        HashSet<string> voidElements = new(StringComparer.OrdinalIgnoreCase)
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input",
            "link", "meta", "param", "source", "track", "wbr"
        };

        Dictionary<string, int> openCounts = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> closeCounts = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, int> firstOpen = new(StringComparer.OrdinalIgnoreCase);

        string joined = string.Join('\n', lines);
        MatchCollection opens = Regex.Matches(joined, @"<([a-zA-Z][a-zA-Z0-9]*)\b[^/>]*(?<!/)>", RegexOptions.IgnoreCase);
        MatchCollection closes = Regex.Matches(joined, @"</([a-zA-Z][a-zA-Z0-9]*)\s*>", RegexOptions.IgnoreCase);

        foreach (Match m in opens)
        {
            string tag = m.Groups[1].Value.ToLowerInvariant();
            if (voidElements.Contains(tag)) continue;
            openCounts[tag] = openCounts.GetValueOrDefault(tag) + 1;
            if (!firstOpen.ContainsKey(tag))
                firstOpen[tag] = FindLineOfMatch(lines, m.Index);
        }

        foreach (Match m in closes)
        {
            string tag = m.Groups[1].Value.ToLowerInvariant();
            closeCounts[tag] = closeCounts.GetValueOrDefault(tag) + 1;
        }

        foreach ((string tag, int count) in openCounts)
        {
            int closed = closeCounts.GetValueOrDefault(tag);
            if (count > closed)
                results.Add(new("error", firstOpen.GetValueOrDefault(tag, 1), "unclosed-tag", $"Tag <{tag}> opened {count} time(s) but only closed {closed} time(s)"));
        }
    }

    public static void CheckDuplicateCssProperties(string[] lines, List<LintResult> results)
    {
        List<string> currentProps = new();
        int blockStart = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();

            if (trimmed.Contains('{'))
            {
                currentProps.Clear();
                blockStart = i + 1;
            }
            else if (trimmed.Contains('}'))
            {
                currentProps.Clear();
            }
            else if (blockStart >= 0)
            {
                Match propMatch = Regex.Match(trimmed, @"^([a-zA-Z\-]+)\s*:");
                if (propMatch.Success)
                {
                    string prop = propMatch.Groups[1].Value.ToLowerInvariant();
                    if (currentProps.Contains(prop))
                        results.Add(new("warning", i + 1, "duplicate-property", $"Duplicate property \"{prop}\" in the same rule block"));
                    else
                        currentProps.Add(prop);
                }
            }
        }
    }

    public static int FindLine(string[] lines, string search)
    {
        for (int i = 0; i < lines.Length; i++)
            if (lines[i].Contains(search, StringComparison.OrdinalIgnoreCase))
                return i + 1;
        return 1;
    }

    public static int FindLineOfMatch(string[] lines, int charIndex)
    {
        int count = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            count += lines[i].Length + 1;
            if (count > charIndex)
                return i + 1;
        }
        return lines.Length;
    }

    public static string GetSeverityIcon(string severity) => severity switch
    {
        "error" => "ti-alert-circle",
        "warning" => "ti-alert-triangle",
        _ => "ti-info-circle"
    };

    public static string GetSeverityColor(string severity) => severity switch
    {
        "error" => "text-red-600 dark:text-red-400",
        "warning" => "text-amber-600 dark:text-amber-400",
        _ => "text-blue-600 dark:text-blue-400"
    };

    public static string GetResultBorderClass(string severity) => severity switch
    {
        "error" => "border-red-200 dark:border-red-800 bg-red-50/50 dark:bg-red-900/10",
        "warning" => "border-amber-200 dark:border-amber-800 bg-amber-50/50 dark:bg-amber-900/10",
        _ => "border-blue-200 dark:border-blue-800 bg-blue-50/50 dark:bg-blue-900/10"
    };

    public static string GetPlaceholder(string tab) => tab switch
    {
        "html" => "Paste your HTML here…",
        "css" => "Paste your CSS here…",
        "js" => "Paste your JavaScript here…",
        _ => "Paste code here…"
    };

    public static readonly string SampleHtml = """
        <html>
        <head>
            <title></title>
        </head>
        <body>
            <center>
                <h1 style="color: red;">Welcome</h1>
            </center>
            <img src="">
            <img src="photo.jpg">
            <a href="">Click here</a>
            <div id="main">Main content</div>
            <div id="main">Duplicate id</div>
            <font size="3">Old school text</font>
            <marquee>Scrolling text!</marquee>
            <p>Unclosed paragraph
        </body>
        </html>
        """;

    public static readonly string SampleCss = """
        * {
            margin: 0px;
            padding: 0px;
        }

        .header {
            color: #ff0;
            background: #xyz;
            float: left;
            -webkit-transform: rotate(45deg);
            -moz-transform: rotate(45deg);
            margin: 0rem;
        }

        .empty-rule {}

        .important-stuff {
            color: red !important;
            display: flex;
            color: blue;
            padding: 10px
        }
        """;

    public static readonly string SampleJs = """
        var name = "world";
        var count = 0;

        function greet(x) {
            if (x = "hello") {
                console.log("greeting: " + x);
                alert("Hello!");
            }

            if (name == "world") {
                eval("count++");
            }

            with (document) {
                write("bad practice");
            }

            try {
                JSON.parse(x);
            } catch (e) {}

            debugger
            var result = confirm("Continue?")
        }
        """;
}
