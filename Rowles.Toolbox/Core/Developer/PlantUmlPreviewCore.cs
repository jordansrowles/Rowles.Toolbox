using System.IO.Compression;
using System.Text;

namespace Rowles.Toolbox.Core.Developer;

public static class PlantUmlPreviewCore
{
    public sealed record DiagramPreset(string Label, string Icon, string Code);

    public static readonly string ServerBaseUrl = "https://www.plantuml.com/plantuml";

    public static readonly List<DiagramPreset> Presets =
    [
        new("Sequence", "arrows-exchange",
            "@startuml\nAlice -> Bob: Authentication Request\nBob --> Alice: Authentication Response\nAlice -> Bob: Another request\nBob --> Alice: Another response\n@enduml"),

        new("Class", "hierarchy-3",
            "@startuml\nclass Animal {\n  +String name\n  +int age\n  +makeSound()\n}\nclass Dog extends Animal {\n  +fetch()\n}\nclass Cat extends Animal {\n  +purr()\n}\n@enduml"),

        new("Activity", "arrows-split",
            "@startuml\nstart\n:Read input;\nif (Valid?) then (yes)\n  :Process data;\n  :Save result;\nelse (no)\n  :Show error;\nendif\nstop\n@enduml"),

        new("Component", "packages",
            "@startuml\npackage \"Frontend\" {\n  [Web App]\n  [Mobile App]\n}\npackage \"Backend\" {\n  [API Gateway]\n  [Auth Service]\n  [Data Service]\n}\ndatabase \"PostgreSQL\" as db\n[Web App] --> [API Gateway]\n[Mobile App] --> [API Gateway]\n[API Gateway] --> [Auth Service]\n[API Gateway] --> [Data Service]\n[Data Service] --> db\n@enduml"),

        new("Use Case", "users",
            "@startuml\nleft to right direction\nactor User\nactor Admin\nrectangle System {\n  User --> (Login)\n  User --> (View Dashboard)\n  Admin --> (Manage Users)\n  Admin --> (View Reports)\n  (Login) .> (Authenticate) : include\n}\n@enduml"),

        new("State", "git-fork",
            "@startuml\n[*] --> Draft\nDraft --> Review : submit\nReview --> Approved : approve\nReview --> Draft : reject\nApproved --> Published : publish\nPublished --> [*]\n@enduml"),
    ];

    public static string EncodePlantUml(string text)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(text);
        using MemoryStream output = new();
        using (DeflateStream deflate = new(output, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            deflate.Write(data, 0, data.Length);
        }
        byte[] compressed = output.ToArray();
        return Encode64(compressed);
    }

    public static string BuildDiagramUrl(string input, string outputFormat)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string encoded = EncodePlantUml(input);
        return $"{ServerBaseUrl}/{outputFormat}/{encoded}";
    }

    private static string Encode64(byte[] data)
    {
        StringBuilder result = new();
        for (int i = 0; i < data.Length; i += 3)
        {
            int b1 = data[i];
            int b2 = (i + 1 < data.Length) ? data[i + 1] : 0;
            int b3 = (i + 2 < data.Length) ? data[i + 2] : 0;
            result.Append(Encode6bit(b1 >> 2));
            result.Append(Encode6bit(((b1 & 0x3) << 4) | (b2 >> 4)));
            result.Append(Encode6bit(((b2 & 0xF) << 2) | (b3 >> 6)));
            result.Append(Encode6bit(b3 & 0x3F));
        }
        return result.ToString();
    }

    private static char Encode6bit(int b)
    {
        if (b < 10) return (char)(48 + b);
        b -= 10;
        if (b < 26) return (char)(65 + b);
        b -= 26;
        if (b < 26) return (char)(97 + b);
        b -= 26;
        if (b == 0) return '-';
        if (b == 1) return '_';
        return '?';
    }
}
