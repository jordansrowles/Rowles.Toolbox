using System.Text;

namespace Rowles.Toolbox.Core.Security;

public static class CipherPlaygroundCore
{
    // Caesar / ROT13
    public static string CaesarShift(string text, int shift)
    {
        StringBuilder sb = new(text.Length);
        foreach (char c in text)
        {
            if (c is >= 'A' and <= 'Z')
                sb.Append((char)('A' + (c - 'A' + shift) % 26));
            else if (c is >= 'a' and <= 'z')
                sb.Append((char)('a' + (c - 'a' + shift) % 26));
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    public static string Rot47(string text)
    {
        StringBuilder sb = new(text.Length);
        foreach (char c in text)
        {
            if (c >= 33 && c <= 126)
                sb.Append((char)(33 + (c - 33 + 47) % 94));
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    // Vigenere
    public static string VigenereApply(string input, string keyLetters, bool encode)
    {
        StringBuilder sb = new(input.Length);
        int ki = 0;
        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                int @base = char.IsUpper(c) ? 'A' : 'a';
                int shift = keyLetters[ki % keyLetters.Length] - 'A';
                if (!encode) shift = 26 - shift;
                sb.Append((char)(@base + (c - @base + shift) % 26));
                ki++;
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    public static string VigenereExpandKey(string input, string keyLetters)
    {
        if (keyLetters.Length == 0 || string.IsNullOrEmpty(input)) return string.Empty;

        StringBuilder sb = new(input.Length);
        int ki = 0;
        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                sb.Append(keyLetters[ki % keyLetters.Length]);
                ki++;
            }
            else
            {
                sb.Append(' ');
            }
        }
        return sb.ToString();
    }

    // Atbash
    public static string Atbash(string input)
    {
        StringBuilder sb = new(input.Length);
        foreach (char c in input)
        {
            if (c is >= 'A' and <= 'Z')
                sb.Append((char)('Z' - (c - 'A')));
            else if (c is >= 'a' and <= 'z')
                sb.Append((char)('z' - (c - 'a')));
            else
                sb.Append(c);
        }
        return sb.ToString();
    }

    // XOR
    public static (string? Result, string? Error) XorApply(string input, string key, bool inputHex, bool outputBase64)
    {
        if (string.IsNullOrEmpty(input)) return (null, "Please enter some input.");
        if (string.IsNullOrEmpty(key)) return (null, "Please enter a key.");

        try
        {
            byte[] inputBytes;
            if (inputHex)
            {
                string hex = input.Replace(" ", "").Replace("-", "");
                if (hex.Length % 2 != 0) return (null, "Hex input must have an even number of characters.");
                inputBytes = Convert.FromHexString(hex);
            }
            else
            {
                inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            }

            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[inputBytes.Length];

            for (int i = 0; i < inputBytes.Length; i++)
                result[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);

            string output = outputBase64
                ? Convert.ToBase64String(result)
                : Convert.ToHexString(result).ToLowerInvariant();

            return (output, null);
        }
        catch (FormatException)
        {
            return (null, "Invalid hex input.");
        }
    }

    // Random bytes formatting
    public static string FormatHex(byte[] bytes, bool spaceSeparated, bool upperCase)
    {
        string hex = upperCase
            ? Convert.ToHexString(bytes)
            : Convert.ToHexString(bytes).ToLowerInvariant();

        if (!spaceSeparated) return hex;

        return string.Join(' ', Enumerable.Range(0, bytes.Length).Select(i => hex.Substring(i * 2, 2)));
    }

    public static string FormatCSharpLiteral(byte[] bytes) =>
        $"new byte[] {{ {string.Join(", ", bytes.Select(b => $"0x{b:X2}"))} }}";

    public static string FormatDecimalArray(byte[] bytes) =>
        $"[{string.Join(", ", bytes)}]";
}
