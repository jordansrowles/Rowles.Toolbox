namespace Rowles.Toolbox.Core.Encoding;

public static class Base64ToolCore
{
    public static (string Base64, int ByteLength) Encode(string plainText)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return (Convert.ToBase64String(bytes), bytes.Length);
    }

    public static (string PlainText, int ByteLength) Decode(string base64Text)
    {
        byte[] bytes = Convert.FromBase64String(base64Text);
        return (System.Text.Encoding.UTF8.GetString(bytes), bytes.Length);
    }
}
