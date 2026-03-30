namespace Rowles.Toolbox.Core.Colour;

public static class ColourConverterCore
{
    public static int ParseInt(object? value, int fallback)
    {
        string raw = value?.ToString() ?? string.Empty;
        return int.TryParse(raw, out int result) ? result : fallback;
    }

    public static bool IsValidHex(string hex) =>
        hex.All(c => "0123456789abcdefABCDEF".Contains(c));

    public static bool IsValidHexColour(string hex) =>
        (hex.Length == 6 || hex.Length == 3) && IsValidHex(hex);

    public static string NormaliseHex(string hex)
    {
        hex = hex.ToLowerInvariant();
        if (hex.Length == 3)
            return $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        return hex;
    }

    public static string RgbToHex(int r, int g, int b) =>
        $"{r:x2}{g:x2}{b:x2}";

    public static (int R, int G, int B) HexToRgb(string hex) =>
        (Convert.ToInt32(hex[..2], 16), Convert.ToInt32(hex[2..4], 16), Convert.ToInt32(hex[4..6], 16));

    public static (int H, int S, int L) RgbToHsl(int r, int g, int b)
    {
        double rn = r / 255.0, gn = g / 255.0, bn = b / 255.0;
        double max = Math.Max(rn, Math.Max(gn, bn));
        double min = Math.Min(rn, Math.Min(gn, bn));
        double l = (max + min) / 2.0;

        if (Math.Abs(max - min) < 0.0001)
            return (0, 0, (int)Math.Round(l * 100));

        double d = max - min;
        double s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

        double h;
        if (Math.Abs(max - rn) < 0.0001)
            h = (gn - bn) / d + (gn < bn ? 6 : 0);
        else if (Math.Abs(max - gn) < 0.0001)
            h = (bn - rn) / d + 2;
        else
            h = (rn - gn) / d + 4;

        h *= 60;
        return ((int)Math.Round(h), (int)Math.Round(s * 100), (int)Math.Round(l * 100));
    }

    public static (int R, int G, int B) HslToRgb(int h, int s, int l)
    {
        double hd = h / 360.0, sd = s / 100.0, ld = l / 100.0;

        if (Math.Abs(sd) < 0.0001)
        {
            int grey = (int)Math.Round(ld * 255);
            return (grey, grey, grey);
        }

        double q = ld < 0.5 ? ld * (1 + sd) : ld + sd - ld * sd;
        double p = 2 * ld - q;

        double r = HueToRgb(p, q, hd + 1.0 / 3.0);
        double g = HueToRgb(p, q, hd);
        double b = HueToRgb(p, q, hd - 1.0 / 3.0);

        return ((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
    }

    public static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
        return p;
    }

    public static (int H, int S, int V) RgbToHsv(int r, int g, int b)
    {
        double rn = r / 255.0, gn = g / 255.0, bn = b / 255.0;
        double max = Math.Max(rn, Math.Max(gn, bn));
        double min = Math.Min(rn, Math.Min(gn, bn));
        double d = max - min;

        double h;
        if (Math.Abs(d) < 0.0001)
            h = 0;
        else if (Math.Abs(max - rn) < 0.0001)
            h = ((gn - bn) / d + (gn < bn ? 6 : 0)) * 60;
        else if (Math.Abs(max - gn) < 0.0001)
            h = ((bn - rn) / d + 2) * 60;
        else
            h = ((rn - gn) / d + 4) * 60;

        double s = Math.Abs(max) < 0.0001 ? 0 : d / max;
        return ((int)Math.Round(h), (int)Math.Round(s * 100), (int)Math.Round(max * 100));
    }

    public static (int R, int G, int B) HsvToRgb(int h, int s, int v)
    {
        double sd = s / 100.0, vd = v / 100.0;
        double c = vd * sd;
        double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
        double m = vd - c;

        double r, g, b;
        int sector = h / 60 % 6;
        (r, g, b) = sector switch
        {
            0 => (c, x, 0.0),
            1 => (x, c, 0.0),
            2 => (0.0, c, x),
            3 => (0.0, x, c),
            4 => (x, 0.0, c),
            _ => (c, 0.0, x)
        };

        return ((int)Math.Round((r + m) * 255), (int)Math.Round((g + m) * 255), (int)Math.Round((b + m) * 255));
    }

    public static (int C, int M, int Y, int K) RgbToCmyk(int r, int g, int b)
    {
        if (r == 0 && g == 0 && b == 0)
            return (0, 0, 0, 100);

        double rn = r / 255.0, gn = g / 255.0, bn = b / 255.0;
        double k = 1 - Math.Max(rn, Math.Max(gn, bn));
        double c = (1 - rn - k) / (1 - k);
        double m = (1 - gn - k) / (1 - k);
        double y = (1 - bn - k) / (1 - k);
        return ((int)Math.Round(c * 100), (int)Math.Round(m * 100), (int)Math.Round(y * 100), (int)Math.Round(k * 100));
    }

    public static (int R, int G, int B) CmykToRgb(int c, int m, int y, int k)
    {
        double cd = c / 100.0, md = m / 100.0, yd = y / 100.0, kd = k / 100.0;
        int r = (int)Math.Round(255 * (1 - cd) * (1 - kd));
        int g = (int)Math.Round(255 * (1 - md) * (1 - kd));
        int b = (int)Math.Round(255 * (1 - yd) * (1 - kd));
        return (r, g, b);
    }

    public static string HslToHex(int h, int s, int l)
    {
        (int r, int g, int b) = HslToRgb(h, s, l);
        return RgbToHex(r, g, b);
    }

    public static bool IsLight(string hex)
    {
        (int r, int g, int b) = HexToRgb(hex);
        double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
        return luminance > 0.5;
    }
}
