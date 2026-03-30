using System.Globalization;

namespace Rowles.Toolbox.Core.Image;

public static class SvgPathInspectorCore
{
    public sealed record PathCommand(char Letter, string Name, bool IsAbsolute, List<double> Parameters);

    public sealed record ReferenceEntry(string Command, string Name, string Syntax);

    public static readonly Dictionary<string, string> SamplePaths = new()
    {
        ["Triangle"] = "M 50 10 L 90 90 L 10 90 Z",
        ["Circle"] = "M 50 0 C 77.6 0 100 22.4 100 50 C 100 77.6 77.6 100 50 100 C 22.4 100 0 77.6 0 50 C 0 22.4 22.4 0 50 0 Z",
        ["Heart"] = "M 50 30 C 50 20 40 0 20 0 C 0 0 0 20 0 30 C 0 60 50 90 50 90 C 50 90 100 60 100 30 C 100 20 100 0 80 0 C 60 0 50 20 50 30 Z",
        ["Star"] = "M 50 5 L 63 38 L 98 38 L 70 59 L 80 95 L 50 72 L 20 95 L 30 59 L 2 38 L 37 38 Z"
    };

    public static readonly List<ReferenceEntry> ReferenceData = new()
    {
        new("M / m", "MoveTo", "x y"),
        new("L / l", "LineTo", "x y"),
        new("H / h", "Horizontal LineTo", "x"),
        new("V / v", "Vertical LineTo", "y"),
        new("C / c", "CurveTo (cubic)", "x1 y1 x2 y2 x y"),
        new("S / s", "Smooth CurveTo", "x2 y2 x y"),
        new("Q / q", "CurveTo (quadratic)", "x1 y1 x y"),
        new("T / t", "Smooth QuadTo", "x y"),
        new("A / a", "Arc", "rx ry rotation large-arc sweep x y"),
        new("Z / z", "ClosePath", "(none)")
    };

    public static readonly Dictionary<char, string> CommandNames = new()
    {
        ['M'] = "MoveTo", ['m'] = "MoveTo",
        ['L'] = "LineTo", ['l'] = "LineTo",
        ['H'] = "Horizontal LineTo", ['h'] = "Horizontal LineTo",
        ['V'] = "Vertical LineTo", ['v'] = "Vertical LineTo",
        ['C'] = "CurveTo (cubic)", ['c'] = "CurveTo (cubic)",
        ['S'] = "Smooth CurveTo", ['s'] = "Smooth CurveTo",
        ['Q'] = "CurveTo (quadratic)", ['q'] = "CurveTo (quadratic)",
        ['T'] = "Smooth QuadTo", ['t'] = "Smooth QuadTo",
        ['A'] = "Arc", ['a'] = "Arc",
        ['Z'] = "ClosePath", ['z'] = "ClosePath"
    };

    public static readonly Dictionary<char, int> ParamCounts = new()
    {
        ['M'] = 2, ['m'] = 2,
        ['L'] = 2, ['l'] = 2,
        ['H'] = 1, ['h'] = 1,
        ['V'] = 1, ['v'] = 1,
        ['C'] = 6, ['c'] = 6,
        ['S'] = 4, ['s'] = 4,
        ['Q'] = 4, ['q'] = 4,
        ['T'] = 2, ['t'] = 2,
        ['A'] = 7, ['a'] = 7,
        ['Z'] = 0, ['z'] = 0
    };

    public static readonly Dictionary<char, char> ImplicitCommand = new()
    {
        ['M'] = 'L', ['m'] = 'l'
    };

    public static List<object> Tokenize(string input)
    {
        List<object> tokens = new();
        int i = 0;
        while (i < input.Length)
        {
            char c = input[i];

            if (char.IsWhiteSpace(c) || c == ',')
            {
                i++;
                continue;
            }

            if (IsCommandLetter(c))
            {
                tokens.Add(c);
                i++;
                continue;
            }

            // Parse number (handles negative, decimal, and scientific notation)
            if (c == '-' || c == '+' || c == '.' || char.IsDigit(c))
            {
                int start = i;
                if (c == '-' || c == '+') i++;
                bool hasDot = false;
                while (i < input.Length)
                {
                    char ch = input[i];
                    if (char.IsDigit(ch))
                    {
                        i++;
                    }
                    else if (ch == '.' && !hasDot)
                    {
                        hasDot = true;
                        i++;
                    }
                    else if (ch == 'e' || ch == 'E')
                    {
                        i++;
                        if (i < input.Length && (input[i] == '+' || input[i] == '-')) i++;
                        while (i < input.Length && char.IsDigit(input[i])) i++;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }

                string numStr = input[start..i];
                if (double.TryParse(numStr, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double val))
                {
                    tokens.Add(val);
                }
                continue;
            }

            // Skip unknown characters
            i++;
        }
        return tokens;
    }

    public static List<PathCommand> BuildCommands(List<object> tokens)
    {
        List<PathCommand> commands = new();
        int i = 0;
        char currentCmd = '\0';

        while (i < tokens.Count)
        {
            if (tokens[i] is char letter)
            {
                currentCmd = letter;
                i++;

                int paramCount = ParamCounts.GetValueOrDefault(currentCmd, 0);
                if (paramCount == 0)
                {
                    // Z/z — no parameters
                    commands.Add(new PathCommand(
                        currentCmd,
                        CommandNames.GetValueOrDefault(currentCmd, "Unknown"),
                        char.IsUpper(currentCmd),
                        new List<double>()));
                    continue;
                }

                // Read first group
                List<double> firstParams = ReadNumbers(tokens, ref i, paramCount);
                commands.Add(new PathCommand(
                    currentCmd,
                    CommandNames.GetValueOrDefault(currentCmd, "Unknown"),
                    char.IsUpper(currentCmd),
                    firstParams));

                // Read repeated implicit groups
                char repeatCmd = ImplicitCommand.GetValueOrDefault(currentCmd, currentCmd);
                int repeatParamCount = ParamCounts.GetValueOrDefault(repeatCmd, paramCount);
                while (i < tokens.Count && tokens[i] is double)
                {
                    List<double> repeatParams = ReadNumbers(tokens, ref i, repeatParamCount);
                    if (repeatParams.Count == 0) break;
                    commands.Add(new PathCommand(
                        repeatCmd,
                        CommandNames.GetValueOrDefault(repeatCmd, "Unknown"),
                        char.IsUpper(repeatCmd),
                        repeatParams));
                }
            }
            else
            {
                // Stray number without a command — skip
                i++;
            }
        }

        return commands;
    }

    public static List<double> ReadNumbers(List<object> tokens, ref int index, int count)
    {
        List<double> result = new(count);
        for (int j = 0; j < count && index < tokens.Count; j++)
        {
            if (tokens[index] is double d)
            {
                result.Add(d);
                index++;
            }
            else
            {
                break;
            }
        }
        return result;
    }

    public static bool IsCommandLetter(char c)
    {
        return c is 'M' or 'm' or 'L' or 'l' or 'H' or 'h' or 'V' or 'v'
            or 'C' or 'c' or 'S' or 's' or 'Q' or 'q' or 'T' or 't'
            or 'A' or 'a' or 'Z' or 'z';
    }

    public static (double MinX, double MinY, double MaxX, double MaxY) ComputeBoundingBox(List<PathCommand> commands)
    {
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;
        double curX = 0, curY = 0;
        bool hasPoints = false;

        void Track(double x, double y)
        {
            hasPoints = true;
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
        }

        foreach (PathCommand cmd in commands)
        {
            List<double> p = cmd.Parameters;
            char upper = char.ToUpper(cmd.Letter);
            bool abs = cmd.IsAbsolute;

            switch (upper)
            {
                case 'M':
                case 'L':
                case 'T':
                    if (p.Count >= 2)
                    {
                        double x = abs ? p[0] : curX + p[0];
                        double y = abs ? p[1] : curY + p[1];
                        Track(x, y);
                        curX = x; curY = y;
                    }
                    break;

                case 'H':
                    if (p.Count >= 1)
                    {
                        double x = abs ? p[0] : curX + p[0];
                        Track(x, curY);
                        curX = x;
                    }
                    break;

                case 'V':
                    if (p.Count >= 1)
                    {
                        double y = abs ? p[0] : curY + p[0];
                        Track(curX, y);
                        curY = y;
                    }
                    break;

                case 'C':
                    if (p.Count >= 6)
                    {
                        for (int j = 0; j < p.Count - 1; j += 2)
                        {
                            double x = abs ? p[j] : curX + p[j];
                            double y = abs ? p[j + 1] : curY + p[j + 1];
                            Track(x, y);
                        }
                        curX = abs ? p[4] : curX + p[4];
                        curY = abs ? p[5] : curY + p[5];
                    }
                    break;

                case 'S':
                    if (p.Count >= 4)
                    {
                        for (int j = 0; j < p.Count - 1; j += 2)
                        {
                            double x = abs ? p[j] : curX + p[j];
                            double y = abs ? p[j + 1] : curY + p[j + 1];
                            Track(x, y);
                        }
                        curX = abs ? p[2] : curX + p[2];
                        curY = abs ? p[3] : curY + p[3];
                    }
                    break;

                case 'Q':
                    if (p.Count >= 4)
                    {
                        for (int j = 0; j < p.Count - 1; j += 2)
                        {
                            double x = abs ? p[j] : curX + p[j];
                            double y = abs ? p[j + 1] : curY + p[j + 1];
                            Track(x, y);
                        }
                        curX = abs ? p[2] : curX + p[2];
                        curY = abs ? p[3] : curY + p[3];
                    }
                    break;

                case 'A':
                    if (p.Count >= 7)
                    {
                        double x = abs ? p[5] : curX + p[5];
                        double y = abs ? p[6] : curY + p[6];
                        Track(x, y);
                        curX = x; curY = y;
                    }
                    break;

                case 'Z':
                    break;
            }
        }

        if (!hasPoints) return (0, 0, 200, 200);
        return (minX, minY, maxX, maxY);
    }

    public static string ComputeViewBox(List<PathCommand> commands)
    {
        (double minX, double minY, double maxX, double maxY) = ComputeBoundingBox(commands);
        double w = maxX - minX;
        double h = maxY - minY;
        if (w < 1) w = 1;
        if (h < 1) h = 1;

        double padX = w * 0.1;
        double padY = h * 0.1;
        double pad = Math.Max(padX, padY);
        if (pad < 5) pad = 5;

        return FormattableString.Invariant(
            $"{minX - pad:F2} {minY - pad:F2} {w + pad * 2:F2} {h + pad * 2:F2}");
    }

    public static string FormatParameters(List<double> parameters)
    {
        if (parameters.Count == 0) return "\u2014";
        return string.Join(", ", parameters.Select(
            p => p.ToString("G", CultureInfo.InvariantCulture)));
    }

    public static int CalculateTotalCoordinatePairs(List<PathCommand> commands)
    {
        int pairs = 0;
        foreach (PathCommand cmd in commands)
        {
            char upper = char.ToUpper(cmd.Letter);
            pairs += upper switch
            {
                'H' or 'V' => cmd.Parameters.Count,
                'A' => cmd.Parameters.Count / 7,
                'Z' => 0,
                _ => cmd.Parameters.Count / 2
            };
        }
        return pairs;
    }

    public static string FormatBoundingBox(List<PathCommand> commands)
    {
        if (commands.Count == 0) return "\u2014";
        (double minX, double minY, double maxX, double maxY) = ComputeBoundingBox(commands);
        return FormattableString.Invariant($"({minX:F1}, {minY:F1}) \u2192 ({maxX:F1}, {maxY:F1})");
    }

    public static string FormatSize(List<PathCommand> commands)
    {
        if (commands.Count == 0) return "\u2014";
        (double minX, double minY, double maxX, double maxY) = ComputeBoundingBox(commands);
        double w = maxX - minX;
        double h = maxY - minY;
        return FormattableString.Invariant($"{w:F1} \u00d7 {h:F1}");
    }

    public static (List<PathCommand> Commands, string? ErrorMessage) ParsePathData(string pathInput)
    {
        if (string.IsNullOrWhiteSpace(pathInput))
            return (new List<PathCommand>(), "Path input is empty.");

        try
        {
            List<object> tokens = Tokenize(pathInput);
            List<PathCommand> commands = BuildCommands(tokens);
            return (commands, null);
        }
        catch (Exception ex)
        {
            return (new List<PathCommand>(), "Parse error: " + ex.Message);
        }
    }
}
