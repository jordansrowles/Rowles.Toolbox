using System.Text;
using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.WebNetwork;

public static class SvgSpriteBuilderCore
{
    public sealed record SpriteIcon
    {
        public string Id { get; init; } = string.Empty;
        public string ViewBox { get; init; } = string.Empty;
        public string InnerContent { get; init; } = string.Empty;
        public string OriginalSvg { get; init; } = string.Empty;
    }

    public static string BuildSpriteSheet(List<SpriteIcon> icons)
    {
        if (icons.Count == 0) return string.Empty;

        StringBuilder sb = new();
        sb.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" style=\"display: none;\">");
        foreach (SpriteIcon icon in icons)
        {
            string viewBoxAttr = string.IsNullOrEmpty(icon.ViewBox) ? "" : $" viewBox=\"{icon.ViewBox}\"";
            sb.AppendLine($"  <symbol id=\"{icon.Id}\"{viewBoxAttr}>");
            foreach (string line in icon.InnerContent.Split('\n'))
            {
                string trimmed = line.TrimEnd('\r');
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    sb.AppendLine($"    {trimmed.Trim()}");
                }
            }
            sb.AppendLine("  </symbol>");
        }
        sb.Append("</svg>");
        return sb.ToString();
    }

    public static string BuildUsageExamples(List<SpriteIcon> icons)
    {
        StringBuilder sb = new();
        foreach (SpriteIcon icon in icons)
        {
            sb.AppendLine($"<svg class=\"icon\" width=\"24\" height=\"24\"><use href=\"#{icon.Id}\"></use></svg>");
        }
        return sb.ToString().TrimEnd();
    }

    public static List<string> SplitSvgBlocks(string input)
    {
        List<string> blocks = new();
        MatchCollection matches = Regex.Matches(input, @"<svg[\s\S]*?<\/svg>", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            blocks.Add(match.Value);
        }
        return blocks;
    }

    public static SpriteIcon? ParseSvgBlock(string svgMarkup, ref int nextId)
    {
        Match openTagMatch = Regex.Match(svgMarkup, @"<svg\b([^>]*)>", RegexOptions.IgnoreCase);
        if (!openTagMatch.Success) return null;

        string attributes = openTagMatch.Groups[1].Value;

        string viewBox = string.Empty;
        Match viewBoxMatch = Regex.Match(attributes, @"viewBox\s*=\s*""([^""]*)""|viewBox\s*=\s*'([^']*)'", RegexOptions.IgnoreCase);
        if (viewBoxMatch.Success)
        {
            viewBox = viewBoxMatch.Groups[1].Success ? viewBoxMatch.Groups[1].Value : viewBoxMatch.Groups[2].Value;
        }

        string existingId = string.Empty;
        Match idMatch = Regex.Match(attributes, @"\bid\s*=\s*""([^""]*)""|id\s*=\s*'([^']*)'", RegexOptions.IgnoreCase);
        if (idMatch.Success)
        {
            existingId = idMatch.Groups[1].Success ? idMatch.Groups[1].Value : idMatch.Groups[2].Value;
        }

        int openTagEnd = openTagMatch.Index + openTagMatch.Length;
        int closeTagStart = svgMarkup.LastIndexOf("</svg>", StringComparison.OrdinalIgnoreCase);
        if (closeTagStart < 0) closeTagStart = svgMarkup.LastIndexOf("</SVG>", StringComparison.OrdinalIgnoreCase);
        if (closeTagStart < 0 || closeTagStart <= openTagEnd) return null;

        string innerContent = svgMarkup.Substring(openTagEnd, closeTagStart - openTagEnd).Trim();
        string symbolId = !string.IsNullOrWhiteSpace(existingId) ? existingId : $"icon-{nextId}";
        nextId++;

        string normalizedSvg = NormalizeSvgForPreview(svgMarkup);

        return new SpriteIcon
        {
            Id = symbolId,
            ViewBox = viewBox,
            InnerContent = innerContent,
            OriginalSvg = normalizedSvg
        };
    }

    public static string NormalizeSvgForPreview(string svgMarkup)
    {
        string normalized = Regex.Replace(
            svgMarkup,
            @"(<svg\b[^>]*?)(?:\s+(?:width|height)\s*=\s*(?:""[^""]*""|'[^']*'))",
            "$1",
            RegexOptions.IgnoreCase);

        normalized = Regex.Replace(
            normalized,
            @"(<svg\b[^>]*?)(?:\s+(?:width|height)\s*=\s*(?:""[^""]*""|'[^']*'))",
            "$1",
            RegexOptions.IgnoreCase);

        normalized = Regex.Replace(
            normalized,
            @"<svg\b",
            "<svg width=\"48\" height=\"48\"",
            RegexOptions.IgnoreCase);

        return normalized;
    }
}
