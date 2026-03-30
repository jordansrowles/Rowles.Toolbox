using System.Text;

namespace Rowles.Toolbox.Core.Encoding;

public static class UrlEncoderCore
{
    private static readonly HashSet<char> s_urlStructureChars =
        [':', '/', '?', '#', '[', ']', '@', '!', '$', '&', '\'', '(', ')', '*', '+', ',', ';', '='];

    public static string EncodeComponent(string input) =>
        Uri.EscapeDataString(input);

    public static string EncodeFullUrl(string input)
    {
        StringBuilder sb = new(input.Length * 2);
        foreach (char c in input)
        {
            if (s_urlStructureChars.Contains(c) || char.IsLetterOrDigit(c) || c is '-' or '.' or '_' or '~')
            {
                sb.Append(c);
            }
            else
            {
                foreach (byte b in System.Text.Encoding.UTF8.GetBytes(c.ToString()))
                {
                    sb.Append('%');
                    sb.Append(b.ToString("X2"));
                }
            }
        }
        return sb.ToString();
    }

    public static string Decode(string input) =>
        Uri.UnescapeDataString(input);
}
