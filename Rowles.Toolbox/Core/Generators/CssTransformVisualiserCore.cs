using System.Globalization;

namespace Rowles.Toolbox.Core.Generators;

public static class CssTransformVisualiserCore
{
    public sealed record TransformPreset(
        string Name,
        int TranslateX = 0,
        int TranslateY = 0,
        int TranslateZ = 0,
        int RotateX = 0,
        int RotateY = 0,
        int RotateZ = 0,
        double ScaleX = 1.0,
        double ScaleY = 1.0,
        int SkewX = 0,
        int SkewY = 0,
        int Perspective = 800
    );

    public static string FmtDouble(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    public static List<string> GetActiveTransformParts(
        int translateX, int translateY, int translateZ,
        int rotateX, int rotateY, int rotateZ,
        double scaleX, double scaleY, int skewX, int skewY)
    {
        List<string> parts = new();
        if (translateX != 0) parts.Add($"translateX({translateX}px)");
        if (translateY != 0) parts.Add($"translateY({translateY}px)");
        if (translateZ != 0) parts.Add($"translateZ({translateZ}px)");
        if (rotateX != 0) parts.Add($"rotateX({rotateX}deg)");
        if (rotateY != 0) parts.Add($"rotateY({rotateY}deg)");
        if (rotateZ != 0) parts.Add($"rotateZ({rotateZ}deg)");
        if (Math.Abs(scaleX - 1.0) > 0.001) parts.Add($"scaleX({FmtDouble(scaleX)})");
        if (Math.Abs(scaleY - 1.0) > 0.001) parts.Add($"scaleY({FmtDouble(scaleY)})");
        if (skewX != 0) parts.Add($"skewX({skewX}deg)");
        if (skewY != 0) parts.Add($"skewY({skewY}deg)");
        return parts;
    }

    public static string GetTransformOrigin(string originX, string originY, int customOriginX, int customOriginY)
    {
        string x = originX == "custom" ? $"{customOriginX}%" : originX;
        string y = originY == "custom" ? $"{customOriginY}%" : originY;
        return $"{x} {y}";
    }

    public static bool IsOriginDefault(string originX, string originY) =>
        originX == "center" && originY == "center";

    public static int GetOriginXPercent(string originX, int customOriginX) => originX switch
    {
        "left" => 0,
        "center" => 50,
        "right" => 100,
        "custom" => customOriginX,
        _ => 50
    };

    public static int GetOriginYPercent(string originY, int customOriginY) => originY switch
    {
        "top" => 0,
        "center" => 50,
        "bottom" => 100,
        "custom" => customOriginY,
        _ => 50
    };

    public static string GetCssOutput(List<string> activeParts, int perspective,
        string originX, string originY, int customOriginX, int customOriginY, bool backfaceHidden)
    {
        bool hasParentStyles = perspective != 800;
        bool hasTransform = activeParts.Count > 0;
        bool hasOrigin = !IsOriginDefault(originX, originY);
        bool hasBackface = backfaceHidden;
        bool hasElementStyles = hasTransform || hasOrigin || hasBackface;

        if (!hasParentStyles && !hasElementStyles)
            return "/* All values at defaults \u2014 no CSS needed */";

        List<string> lines = new();

        if (hasTransform)
        {
            lines.Add("/* Individual transform functions */");
            foreach (string part in activeParts)
            {
                lines.Add($"/*   {part} */");
            }
            lines.Add("");
        }

        if (hasParentStyles)
        {
            lines.Add("/* Parent container */");
            lines.Add(".parent {");
            lines.Add($"  perspective: {perspective}px;");
            lines.Add("}");
        }

        if (hasElementStyles)
        {
            if (hasParentStyles) lines.Add("");
            lines.Add("/* Element */");
            lines.Add(".element {");
            if (hasTransform)
                lines.Add($"  transform: {string.Join(" ", activeParts)};");
            if (hasOrigin)
                lines.Add($"  transform-origin: {GetTransformOrigin(originX, originY, customOriginX, customOriginY)};");
            if (hasBackface)
                lines.Add("  backface-visibility: hidden;");
            lines.Add("}");
        }

        return string.Join("\n", lines);
    }
}
