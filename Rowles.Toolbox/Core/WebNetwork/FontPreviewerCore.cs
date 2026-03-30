namespace Rowles.Toolbox.Core.WebNetwork;

public static class FontPreviewerCore
{
    public sealed record FontEntry(string Name, string Category, string Fallback, int[] Weights, bool IsSystem);

    public static string CategoryBadgeClass(string category) => category switch
    {
        "Sans-Serif" => "bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300",
        "Serif" => "bg-amber-100 dark:bg-amber-900/40 text-amber-700 dark:text-amber-300",
        "Monospace" => "bg-green-100 dark:bg-green-900/40 text-green-700 dark:text-green-300",
        "Display" => "bg-purple-100 dark:bg-purple-900/40 text-purple-700 dark:text-purple-300",
        "System" => "bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300",
        _ => "bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300",
    };

    public static string GetGoogleFontsUrl(FontEntry font)
    {
        string encodedName = font.Name.Replace(" ", "+");
        string weightList = string.Join(";", font.Weights);
        return $"https://fonts.googleapis.com/css2?family={encodedName}:wght@{weightList}&display=swap";
    }

    public static string GetCssDeclaration(FontEntry font)
    {
        return $"font-family: '{font.Name}', {font.Fallback};";
    }

    public static readonly List<FontEntry> AllFonts =
    [
        // Sans-Serif Google Fonts
        new("Inter", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Roboto", "Sans-Serif", "sans-serif", [100, 300, 400, 500, 700, 900], false),
        new("Open Sans", "Sans-Serif", "sans-serif", [300, 400, 500, 600, 700, 800], false),
        new("Lato", "Sans-Serif", "sans-serif", [100, 300, 400, 700, 900], false),
        new("Montserrat", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Poppins", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Raleway", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Nunito", "Sans-Serif", "sans-serif", [200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Nunito Sans", "Sans-Serif", "sans-serif", [200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Rubik", "Sans-Serif", "sans-serif", [300, 400, 500, 600, 700, 800, 900], false),
        new("Work Sans", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Noto Sans", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("DM Sans", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Plus Jakarta Sans", "Sans-Serif", "sans-serif", [200, 300, 400, 500, 600, 700, 800], false),
        new("Space Grotesk", "Sans-Serif", "sans-serif", [300, 400, 500, 600, 700], false),
        new("Outfit", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Manrope", "Sans-Serif", "sans-serif", [200, 300, 400, 500, 600, 700, 800], false),
        new("PT Sans", "Sans-Serif", "sans-serif", [400, 700], false),
        new("Ubuntu", "Sans-Serif", "sans-serif", [300, 400, 500, 700], false),
        new("Oswald", "Sans-Serif", "sans-serif", [200, 300, 400, 500, 600, 700], false),
        new("Mukta", "Sans-Serif", "sans-serif", [200, 300, 400, 500, 600, 700, 800], false),
        new("Quicksand", "Sans-Serif", "sans-serif", [300, 400, 500, 600, 700], false),
        new("Cabin", "Sans-Serif", "sans-serif", [400, 500, 600, 700], false),
        new("Barlow", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Figtree", "Sans-Serif", "sans-serif", [300, 400, 500, 600, 700, 800, 900], false),
        new("Geist", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Lexend", "Sans-Serif", "sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),

        // Serif Google Fonts
        new("Playfair Display", "Serif", "serif", [400, 500, 600, 700, 800, 900], false),
        new("Merriweather", "Serif", "serif", [300, 400, 700, 900], false),
        new("Lora", "Serif", "serif", [400, 500, 600, 700], false),
        new("PT Serif", "Serif", "serif", [400, 700], false),
        new("Noto Serif", "Serif", "serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Libre Baskerville", "Serif", "serif", [400, 700], false),
        new("EB Garamond", "Serif", "serif", [400, 500, 600, 700, 800], false),
        new("Cormorant Garamond", "Serif", "serif", [300, 400, 500, 600, 700], false),
        new("Bitter", "Serif", "serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),
        new("Crimson Text", "Serif", "serif", [400, 600, 700], false),
        new("DM Serif Display", "Serif", "serif", [400], false),
        new("Fraunces", "Serif", "serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], false),

        // Monospace Google Fonts
        new("Source Code Pro", "Monospace", "monospace", [200, 300, 400, 500, 600, 700, 800, 900], false),
        new("JetBrains Mono", "Monospace", "monospace", [100, 200, 300, 400, 500, 600, 700, 800], false),
        new("Fira Code", "Monospace", "monospace", [300, 400, 500, 600, 700], false),
        new("Roboto Mono", "Monospace", "monospace", [100, 200, 300, 400, 500, 600, 700], false),
        new("IBM Plex Mono", "Monospace", "monospace", [100, 200, 300, 400, 500, 600, 700], false),
        new("Ubuntu Mono", "Monospace", "monospace", [400, 700], false),
        new("Space Mono", "Monospace", "monospace", [400, 700], false),
        new("Inconsolata", "Monospace", "monospace", [200, 300, 400, 500, 600, 700, 800, 900], false),

        // Display Google Fonts
        new("Bebas Neue", "Display", "sans-serif", [400], false),
        new("Lobster", "Display", "cursive", [400], false),
        new("Pacifico", "Display", "cursive", [400], false),
        new("Dancing Script", "Display", "cursive", [400, 500, 600, 700], false),
        new("Satisfy", "Display", "cursive", [400], false),
        new("Permanent Marker", "Display", "cursive", [400], false),
        new("Abril Fatface", "Display", "serif", [400], false),
        new("Righteous", "Display", "sans-serif", [400], false),
        new("Comfortaa", "Display", "sans-serif", [300, 400, 500, 600, 700], false),

        // System Font Stacks
        new("System UI", "System", "system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif", [100, 200, 300, 400, 500, 600, 700, 800, 900], true),
        new("Serif Stack", "System", "Georgia, 'Times New Roman', Times, serif", [400, 700], true),
        new("Sans-Serif Stack", "System", "'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif", [400, 700], true),
        new("Monospace Stack", "System", "SFMono-Regular, Menlo, Consolas, 'Courier New', monospace", [400, 700], true),
        new("Cursive Stack", "System", "'Brush Script MT', 'Segoe Script', cursive", [400], true),
        new("Fantasy Stack", "System", "Copperplate, Papyrus, fantasy", [400], true),
    ];
}
