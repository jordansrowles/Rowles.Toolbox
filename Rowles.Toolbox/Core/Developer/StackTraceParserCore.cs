using System.Text;
using System.Text.RegularExpressions;

namespace Rowles.Toolbox.Core.Developer;

public static class StackTraceParserCore
{
    public sealed record StackFrame(
        string Namespace,
        string Class,
        string Method,
        string Parameters,
        string File,
        int Line,
        int Column);

    public sealed record ExceptionInfo(
        string Type,
        string Message,
        bool IsInner,
        string InnerLabel);

    public enum TraceLanguage { Unknown, DotNet, Java, NodeJs, Python }

    // -- Regex patterns --

    private static readonly Regex s_dotnetFrame = new(
        @"^\s+at\s+(.+?)(?:\s+in\s+(.+?):line\s+(\d+))?\s*$",
        RegexOptions.Compiled);

    private static readonly Regex s_dotnetMethodSplit = new(
        @"^(?:(.+)\.)?(\w+)\.(\w+|\.<\w+>[\w`<>]*)\(([^)]*)\)$",
        RegexOptions.Compiled);

    private static readonly Regex s_javaFrame = new(
        @"^\s+at\s+([\w\.$]+)\.([\w<>]+)\(([^)]*)\)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex s_nodeFrame = new(
        @"^\s+at\s+(?:(.+?)\s+\()?(.+?):(\d+):(\d+)\)?\s*$",
        RegexOptions.Compiled);

    private static readonly Regex s_pythonFrame = new(
        @"^\s+File\s+""(.+?)"",\s+line\s+(\d+),\s+in\s+(.+)\s*$",
        RegexOptions.Compiled);

    private static readonly Regex s_dotnetException = new(
        @"^([\w\.]+(?:Exception|Error|Failure))\s*:\s*(.*)$",
        RegexOptions.Compiled);

    private static readonly Regex s_javaException = new(
        @"^([\w\.]+(?:Exception|Error|Throwable))\s*:\s*(.*)$",
        RegexOptions.Compiled);

    private static readonly Regex s_pythonException = new(
        @"^([\w\.]*(?:Error|Exception|Warning))\s*:\s*(.*)$",
        RegexOptions.Compiled);

    private static readonly Regex s_causedBy = new(
        @"^Caused by:\s*([\w\.]+(?:Exception|Error|Throwable))\s*:\s*(.*)$",
        RegexOptions.Compiled);

    private static readonly Regex s_dotnetInner = new(
        @"^\s*--->\s*([\w\.]+(?:Exception|Error))\s*:\s*(.*)$",
        RegexOptions.Compiled);

    private static readonly string[] s_noisePatterns =
    [
        "System.Runtime",
        "System.Threading",
        "Microsoft.AspNetCore.Diagnostics",
        "Microsoft.AspNetCore.Server.Kestrel",
        "Microsoft.AspNetCore.Hosting",
        "Microsoft.Extensions.Hosting",
        "java.lang.reflect",
        "sun.reflect",
        "org.springframework.cglib",
        "com.sun.proxy",
        "node:internal",
        "node:events"
    ];

    // -- Detection --

    public static TraceLanguage DetectLanguage(string input)
    {
        string[] lines = input.Split('\n');

        int dotnetScore = 0;
        int javaScore = 0;
        int nodeScore = 0;
        int pythonScore = 0;

        foreach (string line in lines)
        {
            if (s_dotnetFrame.IsMatch(line))
            {
                dotnetScore++;
            }

            if (s_javaFrame.IsMatch(line))
            {
                javaScore++;
            }

            if (s_nodeFrame.IsMatch(line))
            {
                nodeScore++;
            }

            if (s_pythonFrame.IsMatch(line))
            {
                pythonScore++;
            }
        }

        if (dotnetScore > 0 && dotnetScore >= javaScore)
        {
            return TraceLanguage.DotNet;
        }

        int max = Math.Max(Math.Max(dotnetScore, javaScore), Math.Max(nodeScore, pythonScore));

        if (max == 0)
        {
            return TraceLanguage.Unknown;
        }

        if (max == javaScore)
        {
            return TraceLanguage.Java;
        }

        if (max == nodeScore)
        {
            return TraceLanguage.NodeJs;
        }

        if (max == pythonScore)
        {
            return TraceLanguage.Python;
        }

        return TraceLanguage.DotNet;
    }

    // -- Exception extraction --

    public static List<ExceptionInfo> ExtractExceptions(string[] lines, TraceLanguage language)
    {
        List<ExceptionInfo> results = [];
        int innerCount = 0;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            Match dotnetInner = s_dotnetInner.Match(line);
            if (dotnetInner.Success)
            {
                innerCount++;
                results.Add(new ExceptionInfo(
                    dotnetInner.Groups[1].Value,
                    dotnetInner.Groups[2].Value,
                    true,
                    $"Inner Exception #{innerCount}"));
                continue;
            }

            Match causedByMatch = s_causedBy.Match(trimmed);
            if (causedByMatch.Success)
            {
                innerCount++;
                results.Add(new ExceptionInfo(
                    causedByMatch.Groups[1].Value,
                    causedByMatch.Groups[2].Value,
                    true,
                    $"Caused by #{innerCount}"));
                continue;
            }

            if (language is TraceLanguage.DotNet or TraceLanguage.Java)
            {
                Regex exRegex = language == TraceLanguage.DotNet
                    ? s_dotnetException
                    : s_javaException;

                Match exMatch = exRegex.Match(trimmed);
                if (exMatch.Success && !results.Exists(e => e.Type == exMatch.Groups[1].Value))
                {
                    results.Add(new ExceptionInfo(
                        exMatch.Groups[1].Value,
                        exMatch.Groups[2].Value,
                        false,
                        string.Empty));
                }
            }

            if (language == TraceLanguage.Python)
            {
                Match pyMatch = s_pythonException.Match(trimmed);
                if (pyMatch.Success && !results.Exists(e => e.Type == pyMatch.Groups[1].Value))
                {
                    results.Add(new ExceptionInfo(
                        pyMatch.Groups[1].Value,
                        pyMatch.Groups[2].Value,
                        false,
                        string.Empty));
                }
            }
        }

        return results;
    }

    // -- Parsing --

    public static List<StackFrame> ParseDotNet(string[] lines, bool stripPaths)
    {
        List<StackFrame> frames = [];

        foreach (string line in lines)
        {
            Match match = s_dotnetFrame.Match(line);
            if (!match.Success)
            {
                continue;
            }

            string fullMethod = match.Groups[1].Value;
            string file = match.Groups[2].Success ? match.Groups[2].Value : string.Empty;
            int lineNum = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

            if (stripPaths && !string.IsNullOrEmpty(file))
            {
                file = Path.GetFileName(file);
            }

            Match methodMatch = s_dotnetMethodSplit.Match(fullMethod);
            if (methodMatch.Success)
            {
                frames.Add(new StackFrame(
                    methodMatch.Groups[1].Value,
                    methodMatch.Groups[2].Value,
                    methodMatch.Groups[3].Value,
                    methodMatch.Groups[4].Value,
                    file,
                    lineNum,
                    0));
            }
            else
            {
                frames.Add(new StackFrame(
                    string.Empty,
                    string.Empty,
                    fullMethod,
                    string.Empty,
                    file,
                    lineNum,
                    0));
            }
        }

        return frames;
    }

    public static List<StackFrame> ParseJava(string[] lines)
    {
        List<StackFrame> frames = [];

        foreach (string line in lines)
        {
            Match match = s_javaFrame.Match(line);
            if (!match.Success)
            {
                continue;
            }

            string fullQualified = match.Groups[1].Value;
            string method = match.Groups[2].Value;
            string fileInfo = match.Groups[3].Value;

            string file = string.Empty;
            int lineNum = 0;

            if (fileInfo.Contains(':'))
            {
                string[] parts = fileInfo.Split(':');
                file = parts[0];
                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out lineNum);
                }
            }
            else
            {
                file = fileInfo;
            }

            int lastDot = fullQualified.LastIndexOf('.');
            string package = lastDot >= 0 ? fullQualified[..lastDot] : string.Empty;
            string className = lastDot >= 0 ? fullQualified[(lastDot + 1)..] : fullQualified;

            frames.Add(new StackFrame(
                package,
                className,
                method,
                string.Empty,
                file,
                lineNum,
                0));
        }

        return frames;
    }

    public static List<StackFrame> ParseNodeJs(string[] lines, bool stripPaths)
    {
        List<StackFrame> frames = [];

        foreach (string line in lines)
        {
            Match match = s_nodeFrame.Match(line);
            if (!match.Success)
            {
                continue;
            }

            string function = match.Groups[1].Success ? match.Groups[1].Value : "(anonymous)";
            string file = match.Groups[2].Value;
            int lineNum = int.TryParse(match.Groups[3].Value, out int ln) ? ln : 0;
            int column = int.TryParse(match.Groups[4].Value, out int col) ? col : 0;

            if (stripPaths && !string.IsNullOrEmpty(file))
            {
                file = Path.GetFileName(file);
            }

            frames.Add(new StackFrame(
                string.Empty,
                string.Empty,
                function,
                string.Empty,
                file,
                lineNum,
                column));
        }

        return frames;
    }

    public static List<StackFrame> ParsePython(string[] lines, bool stripPaths)
    {
        List<StackFrame> frames = [];

        for (int i = 0; i < lines.Length; i++)
        {
            Match match = s_pythonFrame.Match(lines[i]);
            if (!match.Success)
            {
                continue;
            }

            string file = match.Groups[1].Value;
            int lineNum = int.TryParse(match.Groups[2].Value, out int ln) ? ln : 0;
            string function = match.Groups[3].Value;

            if (stripPaths && !string.IsNullOrEmpty(file))
            {
                file = Path.GetFileName(file);
            }

            string code = string.Empty;
            if (i + 1 < lines.Length)
            {
                string nextLine = lines[i + 1].Trim();
                if (!string.IsNullOrWhiteSpace(nextLine)
                    && !s_pythonFrame.IsMatch(lines[i + 1])
                    && !s_pythonException.IsMatch(nextLine))
                {
                    code = nextLine;
                }
            }

            frames.Add(new StackFrame(
                string.Empty,
                string.Empty,
                function,
                code,
                file,
                lineNum,
                0));
        }

        return frames;
    }

    // -- Noise filtering --

    public static bool IsNoise(StackFrame frame)
    {
        string combined = $"{frame.Namespace}.{frame.Class}.{frame.Method} {frame.File}";
        foreach (string pattern in s_noisePatterns)
        {
            if (combined.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // -- Prettify --

    public static string BuildPrettified(
        List<StackFrame> frames,
        List<ExceptionInfo> exceptionInfos,
        TraceLanguage language)
    {
        if (frames.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder sb = new();

        foreach (ExceptionInfo ex in exceptionInfos)
        {
            if (ex.IsInner)
            {
                sb.AppendLine($"  [{ex.InnerLabel}]");
            }

            sb.Append(ex.Type);
            if (!string.IsNullOrWhiteSpace(ex.Message))
            {
                sb.Append(": ").Append(ex.Message);
            }

            sb.AppendLine();
        }

        if (exceptionInfos.Count > 0)
        {
            sb.AppendLine();
        }

        switch (language)
        {
            case TraceLanguage.DotNet:
                foreach (StackFrame f in frames)
                {
                    sb.Append("  at ");
                    if (!string.IsNullOrEmpty(f.Namespace))
                    {
                        sb.Append(f.Namespace).Append('.');
                    }

                    if (!string.IsNullOrEmpty(f.Class))
                    {
                        sb.Append(f.Class).Append('.');
                    }

                    sb.Append(f.Method).Append('(').Append(f.Parameters).Append(')');

                    if (!string.IsNullOrEmpty(f.File))
                    {
                        sb.Append(" in ").Append(f.File);
                        if (f.Line > 0)
                        {
                            sb.Append(":line ").Append(f.Line);
                        }
                    }

                    sb.AppendLine();
                }

                break;

            case TraceLanguage.Java:
                foreach (StackFrame f in frames)
                {
                    sb.Append("  at ");
                    if (!string.IsNullOrEmpty(f.Namespace))
                    {
                        sb.Append(f.Namespace).Append('.');
                    }

                    sb.Append(f.Class).Append('.').Append(f.Method).Append('(');
                    sb.Append(f.File);
                    if (f.Line > 0)
                    {
                        sb.Append(':').Append(f.Line);
                    }

                    sb.Append(')');
                    sb.AppendLine();
                }

                break;

            case TraceLanguage.NodeJs:
                foreach (StackFrame f in frames)
                {
                    sb.Append("  at ");
                    if (f.Method != "(anonymous)")
                    {
                        sb.Append(f.Method).Append(" (");
                    }

                    sb.Append(f.File);
                    if (f.Line > 0)
                    {
                        sb.Append(':').Append(f.Line);
                    }

                    if (f.Column > 0)
                    {
                        sb.Append(':').Append(f.Column);
                    }

                    if (f.Method != "(anonymous)")
                    {
                        sb.Append(')');
                    }

                    sb.AppendLine();
                }

                break;

            case TraceLanguage.Python:
                foreach (StackFrame f in frames)
                {
                    sb.Append("  File \"").Append(f.File).Append("\", line ").Append(f.Line);
                    sb.Append(", in ").Append(f.Method);
                    sb.AppendLine();
                    if (!string.IsNullOrWhiteSpace(f.Parameters))
                    {
                        sb.Append("    ").AppendLine(f.Parameters);
                    }
                }

                break;
        }

        return sb.ToString().TrimEnd();
    }

    // -- Frame formatting --

    public static string FormatFrame(StackFrame frame, TraceLanguage language)
    {
        return language switch
        {
            TraceLanguage.DotNet => FormatDotNetFrame(frame),
            TraceLanguage.Java => FormatJavaFrame(frame),
            TraceLanguage.NodeJs => FormatNodeFrame(frame),
            TraceLanguage.Python => FormatPythonFrame(frame),
            _ => string.Empty
        };
    }

    public static string FormatDotNetFrame(StackFrame f)
    {
        StringBuilder sb = new();
        sb.Append("at ");
        if (!string.IsNullOrEmpty(f.Namespace))
        {
            sb.Append(f.Namespace).Append('.');
        }

        if (!string.IsNullOrEmpty(f.Class))
        {
            sb.Append(f.Class).Append('.');
        }

        sb.Append(f.Method).Append('(').Append(f.Parameters).Append(')');
        if (!string.IsNullOrEmpty(f.File))
        {
            sb.Append(" in ").Append(f.File);
            if (f.Line > 0)
            {
                sb.Append(":line ").Append(f.Line);
            }
        }

        return sb.ToString();
    }

    public static string FormatJavaFrame(StackFrame f)
    {
        StringBuilder sb = new();
        sb.Append("at ");
        if (!string.IsNullOrEmpty(f.Namespace))
        {
            sb.Append(f.Namespace).Append('.');
        }

        sb.Append(f.Class).Append('.').Append(f.Method).Append('(');
        sb.Append(f.File);
        if (f.Line > 0)
        {
            sb.Append(':').Append(f.Line);
        }

        sb.Append(')');
        return sb.ToString();
    }

    public static string FormatNodeFrame(StackFrame f)
    {
        StringBuilder sb = new();
        sb.Append("at ");
        if (f.Method != "(anonymous)")
        {
            sb.Append(f.Method).Append(" (");
        }

        sb.Append(f.File);
        if (f.Line > 0)
        {
            sb.Append(':').Append(f.Line);
        }

        if (f.Column > 0)
        {
            sb.Append(':').Append(f.Column);
        }

        if (f.Method != "(anonymous)")
        {
            sb.Append(')');
        }

        return sb.ToString();
    }

    public static string FormatPythonFrame(StackFrame f)
    {
        StringBuilder sb = new();
        sb.Append("File \"").Append(f.File).Append("\", line ").Append(f.Line);
        sb.Append(", in ").Append(f.Method);
        if (!string.IsNullOrWhiteSpace(f.Parameters))
        {
            sb.Append(" → ").Append(f.Parameters);
        }

        return sb.ToString();
    }

    // -- Badge helpers --

    public static string LanguageBadgeClass(TraceLanguage lang) => lang switch
    {
        TraceLanguage.DotNet => "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300",
        TraceLanguage.Java => "bg-orange-100 dark:bg-orange-900 text-orange-700 dark:text-orange-300",
        TraceLanguage.NodeJs => "bg-green-100 dark:bg-green-900 text-green-700 dark:text-green-300",
        TraceLanguage.Python => "bg-yellow-100 dark:bg-yellow-900 text-yellow-700 dark:text-yellow-300",
        _ => "bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400"
    };

    public static string LanguageIcon(TraceLanguage lang) => lang switch
    {
        TraceLanguage.DotNet => "ti-brand-visual-studio",
        TraceLanguage.Java => "ti-coffee",
        TraceLanguage.NodeJs => "ti-brand-nodejs",
        TraceLanguage.Python => "ti-brand-python",
        _ => "ti-code"
    };

    public static string LanguageLabel(TraceLanguage lang) => lang switch
    {
        TraceLanguage.DotNet => ".NET",
        TraceLanguage.Java => "Java",
        TraceLanguage.NodeJs => "Node.js",
        TraceLanguage.Python => "Python",
        _ => "Unknown"
    };
}
