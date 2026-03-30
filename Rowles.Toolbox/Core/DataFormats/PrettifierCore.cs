using System.Text;

namespace Rowles.Toolbox.Core.DataFormats;

public static class PrettifierCore
{
    public static string ReindentJson(string json, int spaces)
    {
        StringBuilder sb = new();
        int currentIndent = 0;
        bool inString = false;
        bool escaped = false;
        string indentStr = new(' ', spaces);

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            if (escaped) { sb.Append(c); escaped = false; continue; }
            if (c == '\\' && inString) { sb.Append(c); escaped = true; continue; }
            if (c == '"') { inString = !inString; sb.Append(c); continue; }
            if (inString) { sb.Append(c); continue; }

            switch (c)
            {
                case '{' or '[':
                    sb.Append(c);
                    if (i + 1 < json.Length && json[i + 1] is not '}' and not ']')
                    {
                        currentIndent++;
                        sb.AppendLine();
                        for (int j = 0; j < currentIndent; j++) sb.Append(indentStr);
                    }
                    break;
                case '}' or ']':
                    currentIndent--;
                    sb.AppendLine();
                    for (int j = 0; j < currentIndent; j++) sb.Append(indentStr);
                    sb.Append(c);
                    break;
                case ',':
                    sb.Append(c);
                    sb.AppendLine();
                    for (int j = 0; j < currentIndent; j++) sb.Append(indentStr);
                    break;
                case ':':
                    sb.Append(": ");
                    break;
                case '\n' or '\r' or ' ':
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }
}
