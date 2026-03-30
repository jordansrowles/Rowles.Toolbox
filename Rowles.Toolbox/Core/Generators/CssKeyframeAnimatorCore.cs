namespace Rowles.Toolbox.Core.Generators;

public static class CssKeyframeAnimatorCore
{
    public sealed class Keyframe
    {
        public int Percent { get; set; }
        public double TranslateX { get; set; }
        public double TranslateY { get; set; }
        public double Rotate { get; set; }
        public double Scale { get; set; } = 1.0;
        public double SkewX { get; set; }
        public double SkewY { get; set; }
        public double Opacity { get; set; } = 1.0;
        public string Background { get; set; } = "#3b82f6";
        public string Color { get; set; } = "#ffffff";
        public int BorderRadius { get; set; } = 8;
        public int Width { get; set; } = 80;
        public int Height { get; set; } = 60;
        public int Blur { get; set; }
        public int Brightness { get; set; } = 100;
        public int HueRotate { get; set; }
        public string Easing { get; set; } = "ease";
        public double Bx1 { get; set; } = 0.25;
        public double By1 { get; set; } = 0.1;
        public double Bx2 { get; set; } = 0.25;
        public double By2 { get; set; } = 1.0;
    }

    public sealed class AnimPreset
    {
        public string Name { get; init; } = "";
        public string Icon { get; init; } = "ti-sparkles";
        public double Duration { get; init; } = 1.0;
        public bool Infinite { get; init; } = true;
        public int Iterations { get; init; } = 1;
        public string Direction { get; init; } = "normal";
        public string FillMode { get; init; } = "both";
        public required Func<List<Keyframe>> BuildKeyframes { get; init; }
    }

    public static string BuildTransform(Keyframe kf)
    {
        List<string> parts = new();
        if (kf.TranslateX != 0 || kf.TranslateY != 0)
            parts.Add($"translate({kf.TranslateX}px, {kf.TranslateY}px)");
        if (kf.Rotate != 0)
            parts.Add($"rotate({kf.Rotate}deg)");
        if (Math.Abs(kf.Scale - 1.0) > 0.001)
            parts.Add($"scale({kf.Scale:F2})");
        if (kf.SkewX != 0)
            parts.Add($"skewX({kf.SkewX}deg)");
        if (kf.SkewY != 0)
            parts.Add($"skewY({kf.SkewY}deg)");
        return string.Join(" ", parts);
    }

    public static string BuildFilter(Keyframe kf)
    {
        List<string> parts = new();
        if (kf.Blur > 0) parts.Add($"blur({kf.Blur}px)");
        if (kf.Brightness != 100) parts.Add($"brightness({kf.Brightness}%)");
        if (kf.HueRotate != 0) parts.Add($"hue-rotate({kf.HueRotate}deg)");
        return string.Join(" ", parts);
    }

    public static string SummarizeKeyframe(Keyframe kf)
    {
        List<string> parts = new();
        if (kf.TranslateX != 0 || kf.TranslateY != 0) parts.Add($"translate({kf.TranslateX},{kf.TranslateY})");
        if (kf.Rotate != 0) parts.Add($"rotate({kf.Rotate})");
        if (Math.Abs(kf.Scale - 1.0) > 0.001) parts.Add($"scale({kf.Scale:F2})");
        if (Math.Abs(kf.Opacity - 1.0) > 0.001) parts.Add($"opacity({kf.Opacity:F2})");
        if (kf.Blur > 0) parts.Add($"blur({kf.Blur}px)");
        return parts.Count > 0 ? string.Join(" ", parts) : "no changes";
    }

    public static string GenerateCss(string animName, double duration, double delay,
        bool infiniteLoop, int iterations, string direction, string fillMode, List<Keyframe> keyframes)
    {
        System.Text.StringBuilder sb = new();

        sb.AppendLine($"@keyframes {animName} {{");
        foreach (Keyframe kf in keyframes)
        {
            sb.AppendLine($"  {kf.Percent}% {{");
            string transform = BuildTransform(kf);
            if (!string.IsNullOrEmpty(transform))
                sb.AppendLine($"    transform: {transform};");
            if (Math.Abs(kf.Opacity - 1.0) > 0.001)
                sb.AppendLine($"    opacity: {kf.Opacity:F2};");
            if (kf.Background != "#3b82f6")
                sb.AppendLine($"    background: {kf.Background};");
            if (kf.Color != "#ffffff")
                sb.AppendLine($"    color: {kf.Color};");
            if (kf.BorderRadius != 8)
                sb.AppendLine($"    border-radius: {kf.BorderRadius}px;");
            if (kf.Width != 80)
                sb.AppendLine($"    width: {kf.Width}px;");
            if (kf.Height != 60)
                sb.AppendLine($"    height: {kf.Height}px;");
            string filter = BuildFilter(kf);
            if (!string.IsNullOrEmpty(filter))
                sb.AppendLine($"    filter: {filter};");
            if (kf.Easing != "ease" && kf.Percent < 100)
            {
                string easingVal = kf.Easing == "cubic-bezier"
                    ? $"cubic-bezier({kf.Bx1:F2}, {kf.By1:F2}, {kf.Bx2:F2}, {kf.By2:F2})"
                    : kf.Easing;
                sb.AppendLine($"    animation-timing-function: {easingVal};");
            }
            sb.AppendLine("  }");
        }
        sb.AppendLine("}");
        sb.AppendLine();

        string iterStr = infiniteLoop ? "infinite" : iterations.ToString();
        sb.AppendLine(".animated-element {");
        sb.AppendLine($"  animation: {animName} {duration}s {delay}s {iterStr} {direction} {fillMode};");
        sb.AppendLine("}");

        return sb.ToString();
    }

    public static string GeneratePreviewKeyframes(string previewName, List<Keyframe> keyframes)
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine($"@keyframes {previewName} {{");
        foreach (Keyframe kf in keyframes)
        {
            sb.AppendLine($"  {kf.Percent}% {{");
            string transform = BuildTransform(kf);
            if (!string.IsNullOrEmpty(transform))
                sb.AppendLine($"    transform: {transform};");
            if (Math.Abs(kf.Opacity - 1.0) > 0.001)
                sb.AppendLine($"    opacity: {kf.Opacity:F2};");
            if (kf.Background != "#3b82f6")
                sb.AppendLine($"    background: {kf.Background};");
            if (kf.Color != "#ffffff")
                sb.AppendLine($"    color: {kf.Color};");
            if (kf.BorderRadius != 8)
                sb.AppendLine($"    border-radius: {kf.BorderRadius}px;");
            if (kf.Width != 80)
                sb.AppendLine($"    width: {kf.Width}px;");
            if (kf.Height != 60)
                sb.AppendLine($"    height: {kf.Height}px;");
            string filter = BuildFilter(kf);
            if (!string.IsNullOrEmpty(filter))
                sb.AppendLine($"    filter: {filter};");
            if (kf.Easing != "ease" && kf.Percent < 100)
            {
                string easingVal = kf.Easing == "cubic-bezier"
                    ? $"cubic-bezier({kf.Bx1:F2}, {kf.By1:F2}, {kf.Bx2:F2}, {kf.By2:F2})"
                    : kf.Easing;
                sb.AppendLine($"    animation-timing-function: {easingVal};");
            }
            sb.AppendLine("  }");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }
}
