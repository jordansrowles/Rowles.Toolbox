using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Rowles.Toolbox.Core.Image;

public static class SvgViewerCore
{
    public sealed record SvgInfo(string? Width, string? Height, string? ViewBox, int ElementCount);

    public static readonly string[] Backgrounds = ["White", "Dark", "Checkerboard"];

    public static readonly (string Label, string Value)[] ZoomLevels =
    [
        ("Fit", "fit"),
        ("50%", "50%"),
        ("100%", "100%"),
        ("200%", "200%")
    ];

    public static string GetBackgroundClass(string selectedBackground) => selectedBackground switch
    {
        "Dark" => "bg-gray-900",
        "Checkerboard" => "bg-[length:20px_20px] bg-[image:linear-gradient(45deg,#ccc_25%,transparent_25%),linear-gradient(-45deg,#ccc_25%,transparent_25%),linear-gradient(45deg,transparent_75%,#ccc_75%),linear-gradient(-45deg,transparent_75%,#ccc_75%)] bg-[position:0_0,0_10px,10px_-10px,-10px_0]",
        _ => "bg-white"
    };

    public static string GetZoomStyle(string zoom) => zoom switch
    {
        "fit" => "max-width: 100%; height: auto;",
        "50%" => "transform: scale(0.5); transform-origin: top left;",
        "200%" => "transform: scale(2); transform-origin: top left;",
        _ => ""
    };

    public static SvgInfo ParseSvgInfo(string svgCode)
    {
        string? width = null;
        string? height = null;
        string? viewBox = null;
        int elementCount = 0;

        if (string.IsNullOrWhiteSpace(svgCode))
            return new SvgInfo(width, height, viewBox, elementCount);

        try
        {
            XDocument doc = XDocument.Parse(svgCode);
            XElement? root = doc.Root;
            if (root is null)
                return new SvgInfo(width, height, viewBox, elementCount);

            width = root.Attribute("width")?.Value;
            height = root.Attribute("height")?.Value;
            viewBox = root.Attribute("viewBox")?.Value;
            elementCount = root.DescendantsAndSelf().Count();
        }
        catch
        {
            // Invalid XML — try regex fallback for basic info
            Match widthMatch = Regex.Match(svgCode, @"width\s*=\s*""([^""]+)""");
            if (widthMatch.Success) width = widthMatch.Groups[1].Value;

            Match heightMatch = Regex.Match(svgCode, @"height\s*=\s*""([^""]+)""");
            if (heightMatch.Success) height = heightMatch.Groups[1].Value;

            Match viewBoxMatch = Regex.Match(svgCode, @"viewBox\s*=\s*""([^""]+)""");
            if (viewBoxMatch.Success) viewBox = viewBoxMatch.Groups[1].Value;
        }

        return new SvgInfo(width, height, viewBox, elementCount);
    }

    public static string PrettifySvg(string svgCode)
    {
        if (string.IsNullOrWhiteSpace(svgCode)) return svgCode;
        try
        {
            XDocument doc = XDocument.Parse(svgCode);
            return doc.ToString();
        }
        catch
        {
            return svgCode;
        }
    }

    public static (int Width, int Height) GetRenderDimensions(string? svgWidth, string? svgHeight, string? svgViewBox)
    {
        int renderWidth = 800;
        int renderHeight = 600;

        if (int.TryParse(svgWidth?.Replace("px", ""), out int pw)) renderWidth = pw;
        if (int.TryParse(svgHeight?.Replace("px", ""), out int ph)) renderHeight = ph;

        if (svgViewBox is not null)
        {
            string[] parts = svgViewBox.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                if (int.TryParse(parts[2], out int vw)) renderWidth = vw;
                if (int.TryParse(parts[3], out int vh)) renderHeight = vh;
            }
        }

        return (renderWidth, renderHeight);
    }
}
