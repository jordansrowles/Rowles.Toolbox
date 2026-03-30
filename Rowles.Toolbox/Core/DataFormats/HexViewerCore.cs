namespace Rowles.Toolbox.Core.DataFormats;

public static class HexViewerCore
{
    public static string GetByteColor(byte b)
    {
        if (b >= 0x20 && b <= 0x7E)
            return "text-green-700 dark:text-green-400";
        if (b <= 0x1F || b == 0x7F)
            return "text-red-600 dark:text-red-400";
        return "text-blue-600 dark:text-blue-400";
    }
}
