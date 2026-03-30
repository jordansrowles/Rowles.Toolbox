using System.Text;

namespace Rowles.Toolbox.Core.Colour;

public static class PaletteGeneratorCore
{
    public sealed record PaletteResult(
        List<string> Shades,
        List<string> Tints,
        List<string> Tones,
        string Complementary,
        string[] Analogous,
        string[] Triadic,
        string[] SplitComplementary);

    public static PaletteResult GeneratePalette(string baseHex)
    {
        (int r, int g, int b) = ColourConverterCore.HexToRgb(baseHex);
        (int h, int s, int l) = ColourConverterCore.RgbToHsl(r, g, b);

        List<string> shades = [];
        for (int i = 9; i >= 1; i--)
        {
            int newL = (int)Math.Round(l * (i / 10.0));
            shades.Add(ColourConverterCore.HslToHex(h, s, newL));
        }

        List<string> tints = [];
        for (int i = 1; i <= 9; i++)
        {
            int newL = l + (int)Math.Round((100 - l) * (i / 10.0));
            tints.Add(ColourConverterCore.HslToHex(h, s, Math.Min(newL, 100)));
        }

        List<string> tones = [];
        for (int i = 1; i <= 9; i++)
        {
            int newS = (int)Math.Round(s * (1 - i / 10.0));
            tones.Add(ColourConverterCore.HslToHex(h, newS, l));
        }

        string complementary = ColourConverterCore.HslToHex((h + 180) % 360, s, l);
        string[] analogous = [ColourConverterCore.HslToHex((h + 330) % 360, s, l), ColourConverterCore.HslToHex((h + 30) % 360, s, l)];
        string[] triadic = [ColourConverterCore.HslToHex((h + 120) % 360, s, l), ColourConverterCore.HslToHex((h + 240) % 360, s, l)];
        string[] splitComplementary = [ColourConverterCore.HslToHex((h + 150) % 360, s, l), ColourConverterCore.HslToHex((h + 210) % 360, s, l)];

        return new PaletteResult(shades, tints, tones, complementary, analogous, triadic, splitComplementary);
    }

    public static string GenerateCssExport(string baseHex, PaletteResult palette)
    {
        StringBuilder sb = new();
        sb.AppendLine(":root {");
        sb.AppendLine($"  --colour-base: #{baseHex};");
        for (int i = 0; i < palette.Shades.Count; i++)
            sb.AppendLine($"  --colour-shade-{i + 1}: #{palette.Shades[i]};");
        for (int i = 0; i < palette.Tints.Count; i++)
            sb.AppendLine($"  --colour-tint-{i + 1}: #{palette.Tints[i]};");
        for (int i = 0; i < palette.Tones.Count; i++)
            sb.AppendLine($"  --colour-tone-{i + 1}: #{palette.Tones[i]};");
        sb.AppendLine($"  --colour-complementary: #{palette.Complementary};");
        sb.AppendLine($"  --colour-analogous-1: #{palette.Analogous[0]};");
        sb.AppendLine($"  --colour-analogous-2: #{palette.Analogous[1]};");
        sb.AppendLine($"  --colour-triadic-1: #{palette.Triadic[0]};");
        sb.AppendLine($"  --colour-triadic-2: #{palette.Triadic[1]};");
        sb.AppendLine($"  --colour-split-comp-1: #{palette.SplitComplementary[0]};");
        sb.AppendLine($"  --colour-split-comp-2: #{palette.SplitComplementary[1]};");
        sb.AppendLine("}");
        return sb.ToString();
    }
}
