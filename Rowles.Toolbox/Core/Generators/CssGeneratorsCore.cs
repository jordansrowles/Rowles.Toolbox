namespace Rowles.Toolbox.Core.Generators;

public static class CssGeneratorsCore
{
    public sealed class GradStop
    {
        public string Colour { get; set; } = "#000000";
        public int Position { get; set; }
    }

    public static string HexToRgba(string hex, double opacity)
    {
        hex = hex.TrimStart('#');
        if (hex.Length < 6) hex = hex.PadRight(6, '0');
        int r = Convert.ToInt32(hex[..2], 16);
        int g = Convert.ToInt32(hex[2..4], 16);
        int b = Convert.ToInt32(hex[4..6], 16);
        return $"rgba({r}, {g}, {b}, {opacity:F2})";
    }

    public static string BuildShadowCss(int shadowX, int shadowY, int shadowBlur, int shadowSpread,
        string shadowColour, int shadowOpacity, bool shadowInset)
    {
        string inset = shadowInset ? "inset " : "";
        string rgba = HexToRgba(shadowColour, shadowOpacity / 100.0);
        return $"box-shadow: {inset}{shadowX}px {shadowY}px {shadowBlur}px {shadowSpread}px {rgba};";
    }

    public static string BuildBorderCss(int borderWidth, string borderStyle, string borderColour,
        int radiusTL, int radiusTR, int radiusBR, int radiusBL)
    {
        string border = $"border: {borderWidth}px {borderStyle} {borderColour};";
        string radius = $"border-radius: {radiusTL}px {radiusTR}px {radiusBR}px {radiusBL}px;";
        return $"{border} {radius}";
    }

    public static string BuildGradientCss(bool gradRadial, int gradAngle, List<GradStop> gradStops)
    {
        string stops = string.Join(", ", gradStops.Select(s => $"{s.Colour} {s.Position}%"));
        return gradRadial
            ? $"background: radial-gradient(circle, {stops});"
            : $"background: linear-gradient({gradAngle}deg, {stops});";
    }
}
