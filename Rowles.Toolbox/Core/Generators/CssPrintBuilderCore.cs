namespace Rowles.Toolbox.Core.Generators;

public static class CssPrintBuilderCore
{
    public sealed class BreakRule
    {
        public string Selector { get; set; } = string.Empty;
        public string Before { get; set; } = "auto";
        public string Inside { get; set; } = "auto";
        public string After { get; set; } = "auto";
    }

    public sealed class HideEntry
    {
        public string Selector { get; set; } = string.Empty;
        public bool Hidden { get; set; }
    }

    public static string Escape(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    public static string GenerateCss(
        string pageSize, bool landscape, int customWidthMm, int customHeightMm,
        bool sameAllMargins, int marginTop, int marginRight, int marginBottom, int marginLeft,
        List<BreakRule> breakRules, List<HideEntry> hideSelectors,
        double bodyFontSize, string fontSizeUnit, double bodyLineHeight,
        bool showUrls, bool forceBlackText, bool removeBackgrounds, bool enablePrintOnly)
    {
        System.Text.StringBuilder sb = new();

        sb.AppendLine("@page {");
        string sizeValue = pageSize switch
        {
            "Custom" => $"{customWidthMm}mm {customHeightMm}mm",
            _ => pageSize
        };
        string orientation = landscape ? " landscape" : "";
        sb.AppendLine($"  size: {sizeValue}{orientation};");
        if (sameAllMargins)
        {
            sb.AppendLine($"  margin: {marginTop}mm;");
        }
        else
        {
            sb.AppendLine($"  margin: {marginTop}mm {marginRight}mm {marginBottom}mm {marginLeft}mm;");
        }
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("@media print {");

        List<string> hiddenSelectors = hideSelectors
            .Where(h => h.Hidden)
            .Select(h => h.Selector)
            .ToList();

        if (hiddenSelectors.Count > 0)
        {
            string joined = string.Join(",\n  ", hiddenSelectors);
            sb.AppendLine($"  {joined} {{");
            sb.AppendLine("    display: none !important;");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        foreach (BreakRule rule in breakRules)
        {
            List<string> declarations = new();
            if (rule.Before != "auto")
                declarations.Add($"    break-before: {rule.Before};");
            if (rule.Inside != "auto")
                declarations.Add($"    break-inside: {rule.Inside};");
            if (rule.After != "auto")
                declarations.Add($"    break-after: {rule.After};");

            if (declarations.Count > 0)
            {
                sb.AppendLine($"  {rule.Selector} {{");
                foreach (string decl in declarations)
                    sb.AppendLine(decl);
                sb.AppendLine("  }");
                sb.AppendLine();
            }
        }

        if (bodyFontSize > 0 || bodyLineHeight > 0)
        {
            sb.AppendLine("  body {");
            if (bodyFontSize > 0)
                sb.AppendLine($"    font-size: {bodyFontSize}{fontSizeUnit};");
            if (bodyLineHeight > 0)
                sb.AppendLine($"    line-height: {bodyLineHeight};");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        if (showUrls)
        {
            sb.AppendLine("  a[href]:after {");
            sb.AppendLine("    content: \" (\" attr(href) \")\";");
            sb.AppendLine("    font-size: 0.8em;");
            sb.AppendLine("    color: #555;");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        if (forceBlackText)
        {
            sb.AppendLine("  * {");
            sb.AppendLine("    color: #000 !important;");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        if (removeBackgrounds)
        {
            sb.AppendLine("  * {");
            sb.AppendLine("    background: transparent !important;");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        if (enablePrintOnly)
        {
            sb.AppendLine();
            sb.AppendLine("@media screen {");
            sb.AppendLine("  .print-only {");
            sb.AppendLine("    display: none;");
            sb.AppendLine("  }");
            sb.AppendLine("}");
        }

        return sb.ToString().TrimEnd();
    }

    public static string GenerateOptimisedCss(
        string pageSize, bool landscape, int customWidthMm, int customHeightMm,
        bool sameAllMargins, int marginTop, int marginRight, int marginBottom, int marginLeft,
        List<BreakRule> breakRules, List<HideEntry> hideSelectors,
        double bodyFontSize, string fontSizeUnit, double bodyLineHeight,
        bool showUrls, bool forceBlackText, bool removeBackgrounds, bool enablePrintOnly)
    {
        System.Text.StringBuilder sb = new();

        sb.AppendLine("@page {");
        string sizeValue = pageSize switch
        {
            "Custom" => $"{customWidthMm}mm {customHeightMm}mm",
            _ => pageSize
        };
        string orientation = landscape ? " landscape" : "";
        sb.AppendLine($"  size: {sizeValue}{orientation};");
        if (sameAllMargins)
            sb.AppendLine($"  margin: {marginTop}mm;");
        else
            sb.AppendLine($"  margin: {marginTop}mm {marginRight}mm {marginBottom}mm {marginLeft}mm;");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("@media print {");

        List<string> hiddenSelectors = hideSelectors
            .Where(h => h.Hidden)
            .Select(h => h.Selector)
            .ToList();
        if (hiddenSelectors.Count > 0)
        {
            string joined = string.Join(",\n  ", hiddenSelectors);
            sb.AppendLine($"  {joined} {{");
            sb.AppendLine("    display: none !important;");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        foreach (BreakRule rule in breakRules)
        {
            List<string> declarations = new();
            if (rule.Before != "auto")
                declarations.Add($"    break-before: {rule.Before};");
            if (rule.Inside != "auto")
                declarations.Add($"    break-inside: {rule.Inside};");
            if (rule.After != "auto")
                declarations.Add($"    break-after: {rule.After};");

            if (declarations.Count > 0)
            {
                sb.AppendLine($"  {rule.Selector} {{");
                foreach (string decl in declarations)
                    sb.AppendLine(decl);
                sb.AppendLine("  }");
                sb.AppendLine();
            }
        }

        if (bodyFontSize > 0 || bodyLineHeight > 0)
        {
            sb.AppendLine("  body {");
            if (bodyFontSize > 0)
                sb.AppendLine($"    font-size: {bodyFontSize}{fontSizeUnit};");
            if (bodyLineHeight > 0)
                sb.AppendLine($"    line-height: {bodyLineHeight};");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        if (showUrls)
        {
            sb.AppendLine("  a[href]:after {");
            sb.AppendLine("    content: \" (\" attr(href) \")\";");
            sb.AppendLine("    font-size: 0.8em;");
            sb.AppendLine("    color: #555;");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        List<string> wildcardDecls = new();
        if (forceBlackText)
            wildcardDecls.Add("    color: #000 !important;");
        if (removeBackgrounds)
            wildcardDecls.Add("    background: transparent !important;");
        if (wildcardDecls.Count > 0)
        {
            sb.AppendLine("  * {");
            foreach (string decl in wildcardDecls)
                sb.AppendLine(decl);
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        sb.AppendLine("}");

        if (enablePrintOnly)
        {
            sb.AppendLine();
            sb.AppendLine("@media screen {");
            sb.AppendLine("  .print-only {");
            sb.AppendLine("    display: none;");
            sb.AppendLine("  }");
            sb.AppendLine("}");
        }

        return sb.ToString().TrimEnd();
    }

    public static string HighlightCss(string css)
    {
        System.Text.StringBuilder sb = new();

        foreach (string rawLine in css.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            string trimmed = line.TrimStart();

            if (trimmed.StartsWith('@'))
            {
                int braceIdx = line.IndexOf('{');
                if (braceIdx >= 0)
                {
                    string keyword = line[..braceIdx].TrimEnd();
                    sb.AppendLine($"<span style=\"color:#3b82f6\">{Escape(keyword)}</span> <span style=\"color:#6b7280\">{{</span>");
                }
                else
                {
                    sb.AppendLine($"<span style=\"color:#3b82f6\">{Escape(line)}</span>");
                }
            }
            else if (trimmed == "}")
            {
                sb.AppendLine($"<span style=\"color:#6b7280\">{Escape(line)}</span>");
            }
            else if (trimmed.EndsWith('{'))
            {
                string selector = line[..line.LastIndexOf('{')].TrimEnd();
                string indent = line[..^line.TrimStart().Length];
                sb.AppendLine($"{Escape(indent)}<span style=\"color:#ea580c\">{Escape(selector.TrimStart())}</span> <span style=\"color:#6b7280\">{{</span>");
            }
            else if (trimmed.Contains(':') && trimmed.EndsWith(';'))
            {
                int colonIdx = trimmed.IndexOf(':');
                string prop = trimmed[..colonIdx];
                string val = trimmed[(colonIdx + 1)..].TrimEnd(';').Trim();
                string indent = line[..^line.TrimStart().Length];
                sb.AppendLine($"{Escape(indent)}<span style=\"color:#3b82f6\">{Escape(prop)}</span>: <span style=\"color:#16a34a\">{Escape(val)}</span>;");
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine(Escape(line));
            }
        }

        return sb.ToString().TrimEnd();
    }
}
