namespace Rowles.Toolbox.Core.Generators;

public static class SvgGradientGeneratorCore
{
    public sealed class ColourStop
    {
        public string Colour { get; set; } = "#000000";
        public int Offset { get; set; }
    }

    public sealed record GradientPreset(string Name, (string Colour, int Offset)[] Stops);

    public static string GenerateSvg(bool isRadial, int x1, int y1, int x2, int y2, int cx, int cy, int r, List<ColourStop> stops)
    {
        System.Text.StringBuilder sb = new();
        sb.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" width="100%" height="200" viewBox="0 0 400 200">""");
        sb.AppendLine("  <defs>");

        if (isRadial)
        {
            sb.AppendLine($"""    <radialGradient id="grad" cx="{cx}%" cy="{cy}%" r="{r}%">""");
        }
        else
        {
            sb.AppendLine($"""    <linearGradient id="grad" x1="{x1}%" y1="{y1}%" x2="{x2}%" y2="{y2}%">""");
        }

        foreach (ColourStop stop in stops)
        {
            sb.AppendLine($"""      <stop offset="{stop.Offset}%" stop-color="{stop.Colour}" />""");
        }

        sb.AppendLine(isRadial ? "    </radialGradient>" : "    </linearGradient>");
        sb.AppendLine("  </defs>");
        sb.AppendLine("""  <rect width="400" height="200" rx="8" fill="url(#grad)" />""");
        sb.AppendLine("</svg>");
        return sb.ToString();
    }
}
