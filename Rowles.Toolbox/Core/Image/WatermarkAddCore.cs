namespace Rowles.Toolbox.Core.Image;

public static class WatermarkAddCore
{
    public static string GetOutputFileName(string originalFileName)
    {
        return System.IO.Path.GetFileNameWithoutExtension(originalFileName) + "_watermarked.png";
    }
}
