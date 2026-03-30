using System.Text.Json;
using System.Text.Json.Nodes;

namespace Rowles.Toolbox.Core.WebNetwork;

public static class ManifestBuilderCore
{
    public static readonly string[] DisplayOptions =
        ["fullscreen", "standalone", "minimal-ui", "browser"];

    public static readonly string[] OrientationOptions =
        ["any", "natural", "portrait", "landscape", "portrait-primary", "portrait-secondary", "landscape-primary", "landscape-secondary"];

    public static readonly string[] DirOptions = ["auto", "ltr", "rtl"];

    public static readonly string[] IconSizeOptions =
        ["48x48", "72x72", "96x96", "128x128", "144x144", "152x152", "192x192", "384x384", "512x512"];

    public static readonly string[] IconTypeOptions =
        ["image/png", "image/svg+xml", "image/webp"];

    public static readonly string[] IconPurposeOptions =
        ["any", "maskable", "monochrome"];

    public static string GenerateJson(ManifestOptions opts)
    {
        JsonObject manifest = new JsonObject();

        if (!string.IsNullOrWhiteSpace(opts.Name))
            manifest["name"] = opts.Name;

        if (!string.IsNullOrWhiteSpace(opts.ShortName))
            manifest["short_name"] = opts.ShortName;

        if (!string.IsNullOrWhiteSpace(opts.Description))
            manifest["description"] = opts.Description;

        manifest["start_url"] = opts.StartUrl;
        manifest["scope"] = opts.Scope;
        manifest["display"] = opts.Display;
        manifest["orientation"] = opts.Orientation;
        manifest["theme_color"] = opts.ThemeColor;
        manifest["background_color"] = opts.BackgroundColor;

        if (!string.IsNullOrWhiteSpace(opts.Lang))
            manifest["lang"] = opts.Lang;

        if (opts.Dir != "auto")
            manifest["dir"] = opts.Dir;

        if (opts.Categories.Count > 0)
        {
            JsonArray categoriesArray = new JsonArray();
            foreach (string cat in opts.Categories)
            {
                categoriesArray.Add(cat);
            }
            manifest["categories"] = categoriesArray;
        }

        if (opts.Icons.Count > 0)
        {
            JsonArray iconsArray = new JsonArray();
            foreach (IconEntry icon in opts.Icons)
            {
                JsonObject iconObj = new JsonObject
                {
                    ["src"] = icon.Src,
                    ["sizes"] = icon.Sizes,
                    ["type"] = icon.Type,
                    ["purpose"] = icon.Purpose
                };
                iconsArray.Add(iconObj);
            }
            manifest["icons"] = iconsArray;
        }

        if (opts.Screenshots.Count > 0)
        {
            JsonArray screenshotsArray = new JsonArray();
            foreach (ScreenshotEntry screenshot in opts.Screenshots)
            {
                JsonObject ssObj = new JsonObject
                {
                    ["src"] = screenshot.Src,
                    ["sizes"] = screenshot.Sizes,
                    ["type"] = screenshot.Type
                };
                screenshotsArray.Add(ssObj);
            }
            manifest["screenshots"] = screenshotsArray;
        }

        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        return manifest.ToJsonString(options);
    }

    public static List<string> GetValidationWarnings(ManifestOptions opts)
    {
        List<string> result = [];

        if (string.IsNullOrWhiteSpace(opts.Name))
            result.Add("\"name\" is required.");

        if (string.IsNullOrWhiteSpace(opts.ShortName))
            result.Add("\"short_name\" is required.");

        if (opts.Icons.Count == 0)
            result.Add("No icons defined. PWAs require at least one icon.");

        bool has192 = false;
        bool has512 = false;
        foreach (IconEntry icon in opts.Icons)
        {
            if (icon.Sizes == "192x192") has192 = true;
            if (icon.Sizes == "512x512") has512 = true;
            if (string.IsNullOrWhiteSpace(icon.Src))
                result.Add($"Icon ({icon.Sizes}) is missing a source URL.");
        }

        if (opts.Icons.Count > 0 && !has192)
            result.Add("Missing a 192x192 icon (required for Add to Home Screen).");

        if (opts.Icons.Count > 0 && !has512)
            result.Add("Missing a 512x512 icon (required for splash screens).");

        foreach (ScreenshotEntry screenshot in opts.Screenshots)
        {
            if (string.IsNullOrWhiteSpace(screenshot.Src))
                result.Add("A screenshot is missing a source URL.");
            if (string.IsNullOrWhiteSpace(screenshot.Sizes))
                result.Add("A screenshot is missing sizes.");
        }

        if (!opts.ThemeColor.StartsWith('#') || (opts.ThemeColor.Length != 4 && opts.ThemeColor.Length != 7))
            result.Add("\"theme_color\" should be a valid hex colour (e.g. #ffffff).");

        if (!opts.BackgroundColor.StartsWith('#') || (opts.BackgroundColor.Length != 4 && opts.BackgroundColor.Length != 7))
            result.Add("\"background_color\" should be a valid hex colour (e.g. #ffffff).");

        return result;
    }

    // ── Inner types ──

    public sealed class ManifestOptions
    {
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StartUrl { get; set; } = "/";
        public string Scope { get; set; } = "/";
        public string Display { get; set; } = "standalone";
        public string Orientation { get; set; } = "any";
        public string ThemeColor { get; set; } = "#ffffff";
        public string BackgroundColor { get; set; } = "#ffffff";
        public string Lang { get; set; } = "en";
        public string Dir { get; set; } = "auto";
        public List<string> Categories { get; set; } = [];
        public List<IconEntry> Icons { get; set; } = [];
        public List<ScreenshotEntry> Screenshots { get; set; } = [];
    }

    public sealed class IconEntry
    {
        public string Src { get; set; } = string.Empty;
        public string Sizes { get; set; } = "192x192";
        public string Type { get; set; } = "image/png";
        public string Purpose { get; set; } = "any";
    }

    public sealed class ScreenshotEntry
    {
        public string Src { get; set; } = string.Empty;
        public string Sizes { get; set; } = "1280x720";
        public string Type { get; set; } = "image/png";
    }
}
