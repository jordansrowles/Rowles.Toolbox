namespace Rowles.Toolbox.Shared;

public static class ToolRegistry
{
    public static IReadOnlyList<ToolSection> Sections { get; } =
    [
        // ── TEXT ──
        new ToolSection
        {
            Name = "Text",
            Icon = "ti-typography",
            IconColorClass = "text-blue-500",
            HoverColorClass = "hover:text-blue-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "Analyse", Icon = "ti-chart-dots",
                    Tools =
                    [
                        new ToolItem { Name = "Text Counter", Route = "tools/text/counter", Icon = "ti-123", Tags = ["count", "words", "characters", "lines", "length"] },
                        new ToolItem { Name = "Readability", Route = "tools/text/readability", Icon = "ti-book", Description = "Flesch-Kincaid score & reading time", Tags = ["readability", "flesch", "kincaid", "reading", "grade"] },
                        new ToolItem { Name = "Levenshtein", Route = "tools/text/levenshtein", Icon = "ti-arrows-diff", Description = "edit distance between two strings", Tags = ["levenshtein", "distance", "diff", "compare", "similarity"] },
                        new ToolItem { Name = "Duplicates", Route = "tools/text/duplicates", Icon = "ti-copy", Description = "find repeated sentences or lines", Tags = ["duplicate", "repeat", "dedupe", "unique"] },
                        new ToolItem { Name = "Diff Viewer", Route = "tools/text/diff", Icon = "ti-file-diff", Description = "side-by-side and structural diffs for text, JSON, YAML, XML, CSS", Tags = ["diff", "compare", "side-by-side", "unified", "json", "yaml", "xml", "css"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Transform", Icon = "ti-arrows-exchange",
                    Tools =
                    [
                        new ToolItem { Name = "Escape / Unescape", Route = "tools/text/escape", Icon = "ti-code", Description = "JSON strings, regex, HTML entities", Tags = ["escape", "unescape", "json", "regex", "html", "entities", "encode"] },
                        new ToolItem { Name = "Reverse Text", Route = "tools/text/reverse", Icon = "ti-switch-horizontal", Tags = ["reverse", "mirror", "flip", "backwards"] },
                        new ToolItem { Name = "Find & Replace", Route = "tools/text/find-replace", Icon = "ti-replace", Tags = ["find", "replace", "search", "substitute", "regex"] },
                        new ToolItem { Name = "Merge & Split", Route = "tools/text/merge-split", Icon = "ti-columns", Description = "join or divide text by delimiter", Tags = ["merge", "split", "join", "delimiter", "concat", "separate"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Generate", Icon = "ti-wand",
                    Tools =
                    [
                        new ToolItem { Name = "Lorem Ipsum", Route = "tools/text/lorem-ipsum", Icon = "ti-file-text", Tags = ["lorem", "ipsum", "placeholder", "dummy", "filler"] },
                        new ToolItem { Name = "Unicode Inspector", Route = "tools/text/unicode", Icon = "ti-abc", Description = "codepoint, name, block", Tags = ["unicode", "codepoint", "character", "block", "utf8", "emoji"] },
                    ]
                },
            ]
        },

        // ── STRUCTURED DATA ──
        new ToolSection
        {
            Name = "Structured Data",
            Icon = "ti-database",
            IconColorClass = "text-purple-500",
            HoverColorClass = "hover:text-purple-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "Formats", Icon = "ti-file-code",
                    Tools =
                    [
                        new ToolItem { Name = "YAML \u2194 JSON", Route = "tools/data/yaml-json", Icon = "ti-arrows-exchange", Tags = ["yaml", "json", "convert", "serialize", "deserialize"] },
                        new ToolItem { Name = "Prettify / Minify", Route = "tools/data/prettify", Icon = "ti-indent-increase", Tags = ["prettify", "minify", "format", "json", "xml", "html", "indent"] },
                        new ToolItem { Name = "Query \u2192 JSON", Route = "tools/data/query-params", Icon = "ti-filter", Description = "URL query string to JSON object", Tags = ["query", "params", "url", "json", "querystring"] },
                        new ToolItem { Name = "HTML \u2192 Markdown", Route = "tools/data/html-markdown", Icon = "ti-markdown", Tags = ["html", "markdown", "convert", "md"] },
                        new ToolItem { Name = "Markdown Table", Route = "tools/generators/markdown-table", Icon = "ti-table", Tags = ["markdown", "table", "generator", "md"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Binary Formats", Icon = "ti-binary",
                    Tools =
                    [
                        new ToolItem { Name = "Byte Array", Route = "tools/data/byte-array", Icon = "ti-brackets", Description = "inspect raw bytes as hex/decimal/binary", Tags = ["byte", "array", "hex", "decimal", "binary", "raw"] },
                        new ToolItem { Name = "MessagePack", Route = "tools/data/messagepack", Icon = "ti-package", Description = "compact binary format visualiser", Tags = ["messagepack", "msgpack", "binary", "serialize"] },
                        new ToolItem { Name = "Protobuf", Route = "tools/data/protobuf", Icon = "ti-schema", Description = "Protocol Buffers JSON \u2194 proto inspector", Tags = ["protobuf", "protocol", "buffers", "grpc", "proto", "binary"] },
                        new ToolItem { Name = "BSON \u2194 JSON", Route = "tools/data/bson", Icon = "ti-file-database", Description = "MongoDB document format converter", Tags = ["bson", "json", "mongodb", "document", "binary"] },
                    ]
                },
            ]
        },

        // ── NUMBERS & MATHS ──
        new ToolSection
        {
            Name = "Numbers & Maths",
            Icon = "ti-calculator",
            IconColorClass = "text-teal-500",
            HoverColorClass = "hover:text-teal-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "Conversions", Icon = "ti-arrows-exchange",
                    Tools =
                    [
                        new ToolItem { Name = "Number Base", Route = "tools/data/number-base", Icon = "ti-binary", Description = "hex/decimal/octal/binary", Tags = ["number", "base", "hex", "decimal", "octal", "binary", "radix"] },
                        new ToolItem { Name = "Memory Size", Route = "tools/data/memory-size", Icon = "ti-device-sd-card", Description = "KB/MB/GB/GiB conversions", Tags = ["memory", "size", "kb", "mb", "gb", "gib", "bytes", "storage"] },
                        new ToolItem { Name = "Scientific Notation", Route = "tools/data/scientific", Icon = "ti-math-function", Tags = ["scientific", "notation", "exponent", "mantissa", "number"] },
                        new ToolItem { Name = "Timestamp \u2194 Date", Route = "tools/data/timestamp", Icon = "ti-clock", Tags = ["timestamp", "date", "unix", "epoch", "time", "convert"] },
                        new ToolItem { Name = "Hex Viewer", Route = "tools/data/hex-viewer", Icon = "ti-scan", Description = "binary data as hex/ASCII dump", Tags = ["hex", "viewer", "dump", "binary", "ascii", "hexdump"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Calculators", Icon = "ti-calculator",
                    Tools =
                    [
                        new ToolItem { Name = "Unit Converter", Route = "tools/math/units", Icon = "ti-ruler-measure", Tags = ["unit", "converter", "metric", "imperial", "length", "weight", "temperature"] },
                        new ToolItem { Name = "Ratio / Percentage", Route = "tools/math/ratio", Icon = "ti-percentage", Tags = ["ratio", "percentage", "percent", "fraction", "proportion"] },
                        new ToolItem { Name = "Time Tools", Route = "tools/math/time", Icon = "ti-hourglass", Description = "duration formatter, how-long-since", Tags = ["time", "duration", "elapsed", "countdown", "difference"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Engineering", Icon = "ti-bolt",
                    Tools =
                    [
                        new ToolItem { Name = "Electrical", Route = "tools/math/electrical", Icon = "ti-bolt", Description = "Ohm's Law, resistance, capacitance, inductance", Tags = ["electrical", "ohm", "resistance", "capacitance", "inductance", "voltage", "current", "resistor"] },
                        new ToolItem { Name = "Wave Properties", Route = "tools/math/waves", Icon = "ti-wave-sine", Description = "frequency, wavelength, Doppler, harmonics", Tags = ["wave", "frequency", "wavelength", "doppler", "harmonics", "sound", "electromagnetic"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Science", Icon = "ti-flask",
                    Tools =
                    [
                        new ToolItem { Name = "Projectile Motion", Route = "tools/math/projectile", Icon = "ti-arrow-up-right", Description = "range, max height, time of flight", Tags = ["projectile", "motion", "trajectory", "physics", "gravity", "ballistic"] },
                        new ToolItem { Name = "Kinematics Solver", Route = "tools/math/kinematics", Icon = "ti-wave-sine", Description = "solve SUVAT equations for any unknowns", Tags = ["kinematics", "suvat", "velocity", "acceleration", "displacement", "physics"] },
                        new ToolItem { Name = "Molar Mass", Route = "tools/math/molar-mass", Icon = "ti-atom", Description = "parse formula, element breakdown, mass", Tags = ["molar", "mass", "chemistry", "element", "compound", "molecular", "weight"] },
                    ]
                },
            ]
        },

        // ── ENCODING & CRYPTO ──
        new ToolSection
        {
            Name = "Encoding & Crypto",
            Icon = "ti-lock",
            IconColorClass = "text-emerald-500",
            HoverColorClass = "hover:text-emerald-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "Encoding", Icon = "ti-transform",
                    Tools =
                    [
                        new ToolItem { Name = "Base64", Route = "tools/encoding/base64", Icon = "ti-transform", Tags = ["base64", "encode", "decode", "binary", "text"] },
                        new ToolItem { Name = "URL Encode", Route = "tools/encoding/url", Icon = "ti-link", Tags = ["url", "encode", "decode", "percent", "uri", "escape"] },
                        new ToolItem { Name = "Image Base64", Route = "tools/encoding/image-base64", Icon = "ti-photo-code", Tags = ["image", "base64", "data", "uri", "png", "jpg", "svg"] },
                        new ToolItem { Name = "Morse Code", Route = "tools/encoding/morse", Icon = "ti-antenna", Description = "encode/decode with audio playback", Tags = ["morse", "code", "audio", "dot", "dash", "telegraph"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Binary Inspection", Icon = "ti-code-dots",
                    Tools =
                    [
                        new ToolItem { Name = "Endianness", Route = "tools/encoding/endianness", Icon = "ti-arrows-exchange", Description = "swap byte order, visualize LE/BE", Tags = ["endian", "endianness", "byte", "order", "little", "big", "swap"] },
                        new ToolItem { Name = "Varint Inspector", Route = "tools/encoding/varint", Icon = "ti-binary", Description = "protobuf-style variable-length integers", Tags = ["varint", "variable", "length", "integer", "protobuf", "leb128"] },
                        new ToolItem { Name = "UTF-16/UTF-32", Route = "tools/encoding/utf16-utf32", Icon = "ti-letter-case", Description = "byte sequences, surrogates, BOM", Tags = ["utf16", "utf32", "unicode", "surrogate", "bom", "encoding"] },
                        new ToolItem { Name = "IEEE 754", Route = "tools/encoding/ieee754", Icon = "ti-binary-tree", Description = "float/double binary representation", Tags = ["ieee", "754", "float", "double", "binary", "mantissa", "exponent", "sign"] },
                        new ToolItem { Name = "Two's Complement", Route = "tools/encoding/twos-complement", Icon = "ti-plus-minus", Description = "signed/unsigned binary conversion", Tags = ["twos", "complement", "signed", "unsigned", "binary", "integer", "negative"] },
                        new ToolItem { Name = "Binary Diff", Route = "tools/encoding/binary-diff", Icon = "ti-file-diff", Description = "byte-level file comparison", Tags = ["binary", "diff", "compare", "file", "byte", "hex"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Cryptography", Icon = "ti-key",
                    Tools =
                    [
                        new ToolItem { Name = "Hash Generator", Route = "tools/encoding/hash", Icon = "ti-hash", Tags = ["hash", "md5", "sha", "sha256", "sha512", "checksum", "digest"] },
                        new ToolItem { Name = "JWT Decoder", Route = "tools/encoding/jwt", Icon = "ti-certificate", Description = "inspect header, payload, signature", Tags = ["jwt", "json", "web", "token", "decode", "bearer", "oauth"] },
                        new ToolItem { Name = "Cipher Playground", Route = "tools/security/cipher", Icon = "ti-lock-code", Description = "ROT13, Vigenere, AES-GCM, HMAC", Tags = ["cipher", "rot13", "caesar", "vigenere", "atbash", "xor", "aes", "hmac", "encrypt", "decrypt"] },
                        new ToolItem { Name = "Password Tools", Route = "tools/security/password", Icon = "ti-password", Description = "generator & strength analyser", Tags = ["password", "generate", "strength", "entropy", "secure", "random"] },
                    ]
                },
            ]
        },

        // ── DESIGN & COLOUR ──
        new ToolSection
        {
            Name = "Design & Colour",
            Icon = "ti-color-swatch",
            IconColorClass = "text-pink-500",
            HoverColorClass = "hover:text-pink-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "Colour", Icon = "ti-palette",
                    Tools =
                    [
                        new ToolItem { Name = "Colour Converter", Route = "tools/colour/converter", Icon = "ti-palette", Description = "HEX/RGB/HSL/CMYK/LAB/LCH", Tags = ["colour", "color", "hex", "rgb", "hsl", "cmyk", "lab", "lch", "convert"] },
                        new ToolItem { Name = "WCAG Contrast", Route = "tools/colour/contrast", Icon = "ti-contrast", Description = "accessibility ratio AA/AAA", Tags = ["wcag", "contrast", "accessibility", "a11y", "ratio", "colour", "color"] },
                        new ToolItem { Name = "Palette Generator", Route = "tools/colour/palette", Icon = "ti-color-filter", Description = "variants, complements, gradients", Tags = ["palette", "colour", "color", "complement", "gradient", "scheme", "harmony"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Generators", Icon = "ti-sparkles",
                    Tools =
                    [
                        new ToolItem { Name = "CSS Generators", Route = "tools/generators/css", Icon = "ti-brush", Description = "box-shadow, border, gradient", Tags = ["css", "generator", "shadow", "border", "gradient", "radius"] },
                        new ToolItem { Name = "CSS Print", Route = "tools/generators/css-print", Icon = "ti-printer", Description = "@media print page breaks & layout", Tags = ["css", "print", "media", "page", "break", "layout"] },
                        new ToolItem { Name = "CSS Counters", Route = "tools/generators/css-counters", Icon = "ti-list-numbers", Description = "counter-increment / counter-reset schemes", Tags = ["css", "counter", "increment", "reset", "list", "numbering"] },
                        new ToolItem { Name = "CSS Filter", Route = "tools/generators/css-filter", Icon = "ti-photo-edit", Description = "blur/brightness/contrast/hue chains", Tags = ["css", "filter", "blur", "brightness", "contrast", "hue", "saturate"] },
                        new ToolItem { Name = "CSS Transform", Route = "tools/generators/css-transform", Icon = "ti-3d-cube-sphere", Description = "rotate, scale, skew, translate, perspective", Tags = ["css", "transform", "rotate", "scale", "skew", "translate", "3d", "perspective"] },
                        new ToolItem { Name = "Keyframe Animator", Route = "tools/generators/css-keyframes", Icon = "ti-movie", Description = "visual @keyframes builder with timeline", Tags = ["css", "keyframe", "animation", "timeline", "transition", "animate"] },
                        new ToolItem { Name = "SVG Gradient", Route = "tools/generators/svg-gradient", Icon = "ti-vector", Description = "visual editor with SVG output", Tags = ["svg", "gradient", "linear", "radial", "color", "vector"] },
                        new ToolItem { Name = "UUID Generator", Route = "tools/generators/uuid", Icon = "ti-fingerprint", Tags = ["uuid", "guid", "unique", "identifier", "random", "v4"] },
                        new ToolItem { Name = "Cron Builder", Route = "tools/generators/cron", Icon = "ti-clock-hour-3", Description = "visual builder, next-10-runs preview", Tags = ["cron", "schedule", "job", "timer", "recurring", "expression"] },
                    ]
                },
            ]
        },

        // ── IMAGE & MEDIA ──
        new ToolSection
        {
            Name = "Image & Media",
            Icon = "ti-photo",
            IconColorClass = "text-red-500",
            HoverColorClass = "hover:text-red-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "Edit", Icon = "ti-pencil",
                    Tools =
                    [
                        new ToolItem { Name = "Resize / Crop", Route = "tools/image/resize", Icon = "ti-resize", Tags = ["resize", "crop", "image", "scale", "dimensions"] },
                        new ToolItem { Name = "WebP Converter", Route = "tools/image/webp", Icon = "ti-file-type-jpg", Tags = ["webp", "convert", "image", "jpg", "png", "compress"] },
                        new ToolItem { Name = "Add Watermark", Route = "tools/image/watermark-add", Icon = "ti-droplet", Tags = ["watermark", "image", "overlay", "text", "stamp"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Inspect", Icon = "ti-eye",
                    Tools =
                    [
                        new ToolItem { Name = "EXIF Inspector", Route = "tools/image/exif", Icon = "ti-info-square", Tags = ["exif", "metadata", "photo", "camera", "gps", "image"] },
                        new ToolItem { Name = "Format Inspector", Route = "tools/image/format-inspector", Icon = "ti-file-analytics", Description = "dimensions, colour depth, ICC", Tags = ["format", "image", "dimensions", "colour", "icc", "profile"] },
                        new ToolItem { Name = "Watermark Check", Route = "tools/image/watermark-check", Icon = "ti-eye-check", Description = "detect invisible steganographic marks", Tags = ["watermark", "steganography", "detect", "hidden", "invisible"] },
                        new ToolItem { Name = "Extract Palette", Route = "tools/image/colour-extract", Icon = "ti-color-picker", Tags = ["palette", "extract", "colour", "color", "image", "dominant"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Vector", Icon = "ti-vector",
                    Tools =
                    [
                        new ToolItem { Name = "SVG Viewer", Route = "tools/image/svg-viewer", Icon = "ti-svg", Tags = ["svg", "viewer", "vector", "preview"] },
                        new ToolItem { Name = "SVG Path", Route = "tools/image/svg-path", Icon = "ti-vector-spline", Description = "decode path command sequences", Tags = ["svg", "path", "command", "bezier", "arc", "vector"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Generate", Icon = "ti-camera",
                    Tools =
                    [
                        new ToolItem { Name = "Code Screenshot", Route = "tools/image/code-screenshot", Icon = "ti-camera-code", Description = "syntax-highlighted code as image", Tags = ["code", "screenshot", "image", "syntax", "highlight", "carbon"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Audio", Icon = "ti-music", IsPlanned = true,
                    Tools = []
                },
            ]
        },

        // ── DEVELOPER ──
        new ToolSection
        {
            Name = "Developer",
            Icon = "ti-terminal",
            IconColorClass = "text-indigo-500",
            HoverColorClass = "hover:text-indigo-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "General", Icon = "ti-tools", IsCollapsible = true,
                    Tools =
                    [
                        new ToolItem { Name = "Conn Strings", Route = "tools/dev/connection-string", Icon = "ti-plug-connected", Description = "format reference for common databases", Tags = ["connection", "string", "database", "sql", "postgres", "mysql", "redis"] },
                        new ToolItem { Name = "Big O", Route = "tools/dev/big-o", Icon = "ti-chart-line", Description = "algorithm complexity cheat sheet", Tags = ["big", "complexity", "algorithm", "time", "space", "notation"] },
                        new ToolItem { Name = "Error Codes", Route = "tools/dev/error-codes", Icon = "ti-alert-circle", Description = "HTTP, gRPC, Win32, HRESULT lookup", Tags = ["error", "code", "http", "grpc", "win32", "hresult", "status"] },
                        new ToolItem { Name = "Stack Trace", Route = "tools/dev/stack-trace", Icon = "ti-stack", Description = "parse & format .NET/Java/Node/Python", Tags = ["stack", "trace", "exception", "error", "parse", "format", "debug"] },
                        new ToolItem { Name = "Mermaid Renderer", Route = "tools/dev/mermaid", Icon = "ti-chart-dots-3", Description = "render Mermaid.js diagrams live", Tags = ["mermaid", "diagram", "flowchart", "sequence", "er", "gantt", "class", "state", "pie", "chart", "uml"] },
                        new ToolItem { Name = "PlantUML Preview", Route = "tools/dev/plantuml", Icon = "ti-plant", Description = "render PlantUML via public server", Tags = ["plantuml", "uml", "diagram", "sequence", "class", "activity", "component", "state", "use case"] },
                    ]
                },
                new ToolGroup
                {
                    Name = ".NET", Icon = "ti-hexagon-letter-c", IsCollapsible = true,
                    Tools =
                    [
                        new ToolItem { Name = ".NET Regex", Route = "tools/dev/dotnet-regex", Icon = "ti-regex", Tags = ["dotnet", "regex", "regular", "expression", "pattern", "match", "csharp"] },
                        new ToolItem { Name = "DateTime", Route = "tools/dev/datetime", Icon = "ti-calendar-time", Description = "DateTimeOffset formats playground", Tags = ["datetime", "date", "time", "format", "parse", "dotnet", "csharp"] },
                        new ToolItem { Name = "Culture Format", Route = "tools/dev/culture", Icon = "ti-language", Description = "test ToString() with any locale", Tags = ["culture", "locale", "format", "tostring", "globalization", "i18n"] },
                        new ToolItem { Name = "BDN Formatter", Route = "tools/dev/benchmark", Icon = "ti-chart-bar", Description = "BenchmarkDotNet output to markdown table", Tags = ["benchmark", "benchmarkdotnet", "bdn", "performance", "markdown", "table"] },
                        new ToolItem { Name = "LINQ Tester", Route = "tools/dev/linq", Icon = "ti-list-search", Description = "pipeline builder with preset data sources", Tags = ["linq", "query", "lambda", "enumerable", "pipeline", "dotnet", "csharp"] },
                        new ToolItem { Name = "Expression Tree", Route = "tools/dev/expression-tree", Icon = "ti-git-fork", Description = "visualise Linq.Expressions AST nodes", Tags = ["expression", "tree", "ast", "linq", "lambda", "node", "visitor"] },
                        new ToolItem { Name = "Func Builder", Route = "tools/dev/func-builder", Icon = "ti-function", Description = "compose Func/Action/Predicate signatures", Tags = ["func", "action", "predicate", "delegate", "signature", "generic", "dotnet"] },
                        new ToolItem { Name = "String Interpolation", Route = "tools/dev/string-interpolation", Icon = "ti-curly-loop", Description = "preview string.Format with live substitution", Tags = ["string", "format", "interpolation", "placeholder", "composite", "csharp", "dotnet"] },
                        new ToolItem { Name = "Struct Layout", Route = "tools/dev/struct-layout", Icon = "ti-layout-grid", Description = "visualise memory layout, padding, alignment", Tags = ["struct", "layout", "memory", "padding", "alignment", "sequential", "pack", "marshal"] },
                        new ToolItem { Name = "Type Hierarchy", Route = "tools/dev/type-hierarchy", Icon = "ti-sitemap", Description = "BCL inheritance chains and interfaces", Tags = ["type", "hierarchy", "inheritance", "interface", "bcl", "class", "struct", "dotnet"] },
                        new ToolItem { Name = "Span Slice Calc", Route = "tools/dev/span-slice", Icon = "ti-slice", Description = "validate Span/Memory slice bounds", Tags = ["span", "slice", "memory", "range", "index", "bounds", "array", "segment"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Web", Icon = "ti-world", IsCollapsible = true,
                    Tools =
                    [
                        new ToolItem { Name = "Regex Tester", Route = "tools/web/regex", Icon = "ti-regex", Tags = ["regex", "regular", "expression", "pattern", "match", "test", "javascript"] },
                        new ToolItem { Name = "HTTP Status", Route = "tools/web/http-status", Icon = "ti-http-get", Tags = ["http", "status", "code", "response", "rest", "api"] },
                        new ToolItem { Name = "HTTP Header Builder", Route = "tools/web/http-header", Icon = "ti-clipboard-list", Tags = ["http", "header", "request", "response", "content-type", "authorization"] },
                        new ToolItem { Name = "User Agent", Route = "tools/web/user-agent", Icon = "ti-device-desktop-analytics", Description = "parse UA string to browser/OS/device", Tags = ["user", "agent", "browser", "os", "device", "parse", "ua"] },
                        new ToolItem { Name = "Screen Sizes", Route = "tools/web/screen-sizes", Icon = "ti-devices", Description = "viewport dimensions reference", Tags = ["screen", "size", "viewport", "responsive", "breakpoint", "device", "resolution"] },
                        new ToolItem { Name = "Web Linters", Route = "tools/dev/web-linters", Icon = "ti-bug", Description = "pattern-based HTML/CSS/JS checks", Tags = ["lint", "linter", "html", "css", "javascript", "validate", "check"] },
                        new ToolItem { Name = "Web Units", Route = "tools/math/web-units", Icon = "ti-dimensions", Description = "px/em/rem/vw/vh conversions", Tags = ["web", "units", "px", "em", "rem", "vw", "vh", "convert", "css"] },
                        new ToolItem { Name = "DOM Tree", Route = "tools/inspect/dom", Icon = "ti-hierarchy", Description = "visualise HTML document structure", Tags = ["dom", "tree", "html", "document", "structure", "element", "node"] },
                        new ToolItem { Name = "XPath Tester", Route = "tools/inspect/xpath", Icon = "ti-route", Tags = ["xpath", "xml", "query", "selector", "node", "test"] },
                        new ToolItem { Name = "JSONPath", Route = "tools/inspect/jsonpath", Icon = "ti-braces", Description = "query JSON with JSONPath expressions", Tags = ["jsonpath", "json", "query", "path", "expression", "filter"] },
                        new ToolItem { Name = "Flexbox", Route = "tools/inspect/flexbox", Icon = "ti-layout", Description = "visualise CSS flex container behaviour", Tags = ["flexbox", "flex", "css", "layout", "container", "align", "justify"] },
                        new ToolItem { Name = "SVG Sprite Builder", Route = "tools/web/svg-sprite", Icon = "ti-icons", Description = "merge SVGs into a symbol-based sprite sheet", Tags = ["svg", "sprite", "symbol", "icon", "merge", "sheet", "vector"] },
                        new ToolItem { Name = "Device Fingerprint", Route = "tools/web/fingerprint", Icon = "ti-fingerprint", Description = "browser fingerprint breakdown", Tags = ["fingerprint", "browser", "device", "canvas", "webgl"] },
                        new ToolItem { Name = "HTML Entities", Route = "tools/web/html-entities", Icon = "ti-code", Description = "searchable named entity reference", Tags = ["html", "entity", "character", "amp", "nbsp", "symbol", "unicode", "reference"] },
                        new ToolItem { Name = "Font Previewer", Route = "tools/web/fonts", Icon = "ti-typography", Description = "preview Google Fonts and system fonts", Tags = ["font", "google", "preview", "typeface", "typography", "web", "css"] },
                        new ToolItem { Name = "Favicon Generator", Route = "tools/web/favicon", Icon = "ti-photo-heart", Description = "generate full favicon set from an image", Tags = ["favicon", "icon", "apple-touch", "android", "pwa", "generate", "image", "resize"] },
                        new ToolItem { Name = "robots.txt Builder", Route = "tools/web/robots-txt", Icon = "ti-robot", Description = "build and validate robots.txt", Tags = ["robots", "txt", "crawler", "seo", "sitemap", "ai", "bot", "block"] },
                        new ToolItem { Name = "Manifest.json Builder", Route = "tools/web/manifest", Icon = "ti-app-window", Description = "build PWA manifest.json visually", Tags = ["manifest", "pwa", "progressive", "web", "app", "json", "icons"] },
                        new ToolItem { Name = ".htaccess Builder", Route = "tools/web/htaccess", Icon = "ti-server-cog", Description = "Apache redirect, security & caching rules", Tags = ["htaccess", "apache", "redirect", "https", "security", "headers", "compression", "cache"] },
                    ]
                },
            ]
        },

        // ── NETWORK & SECURITY ──
        new ToolSection
        {
            Name = "Network & Security",
            Icon = "ti-shield",
            IconColorClass = "text-orange-500",
            HoverColorClass = "hover:text-orange-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "Network", Icon = "ti-network",
                    Tools =
                    [
                        new ToolItem { Name = "CIDR / Subnet", Route = "tools/web/cidr", Icon = "ti-network", Description = "IP range, netmask, broadcast address", Tags = ["cidr", "subnet", "ip", "network", "netmask", "broadcast", "ipv4"] },
                        new ToolItem { Name = "Port Reference", Route = "tools/web/port-reference", Icon = "ti-plug", Description = "common TCP/UDP ports lookup", Tags = ["port", "tcp", "udp", "reference", "service", "well-known"] },
                        new ToolItem { Name = "DNS Lookup", Route = "tools/security/dns", Icon = "ti-world-search", Description = "A/AAAA/MX/TXT/CNAME via DoH", Tags = ["dns", "lookup", "doh", "domain", "record", "mx", "txt", "cname"] },
                        new ToolItem { Name = "TCP/IP Dissector", Route = "tools/web/tcp-dissector", Icon = "ti-file-analytics", Description = "parse IPv4/TCP/UDP headers from hex", Tags = ["tcp", "ip", "dissector", "packet", "header", "hex", "protocol"] },
                        new ToolItem { Name = "Ethernet Inspector", Route = "tools/web/ethernet-inspector", Icon = "ti-cpu", Description = "decode Ethernet II frames", Tags = ["ethernet", "frame", "mac", "ethertype", "layer2", "network"] },
                        new ToolItem { Name = "curl Builder", Route = "tools/web/curl-builder", Icon = "ti-terminal", Description = "visual HTTP request composer", Tags = ["curl", "http", "request", "api", "command", "builder"] },
                        new ToolItem { Name = "Nmap Builder", Route = "tools/web/nmap-builder", Icon = "ti-radar", Description = "compose scan commands with flag explanations", Tags = ["nmap", "scan", "port", "security", "network", "command"] },
                        new ToolItem { Name = "SPF / DKIM / DMARC", Route = "tools/web/dns-records", Icon = "ti-mail", Description = "email authentication DNS record builder", Tags = ["spf", "dkim", "dmarc", "email", "dns", "authentication", "smtp"] },
                        new ToolItem { Name = "Whois", Icon = "ti-search", IsPlanned = true, Description = "domain registration lookup", Tags = ["whois", "domain", "registration", "registrar"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Policies & Headers", Icon = "ti-shield-lock", IsPlanned = true,
                    Tools =
                    [
                        new ToolItem { Name = "CORS Header Builder", Icon = "ti-shield", IsPlanned = true, Tags = ["cors", "header", "origin", "cross-origin", "access-control"] },
                        new ToolItem { Name = "CSP Builder", Icon = "ti-shield-check", IsPlanned = true, Tags = ["csp", "content", "security", "policy", "header"] },
                    ]
                },
            ]
        },

        // ── FILE ──
        new ToolSection
        {
            Name = "File",
            Icon = "ti-folder",
            IconColorClass = "text-cyan-500",
            HoverColorClass = "hover:text-cyan-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "View", Icon = "ti-eye",
                    Tools =
                    [
                        new ToolItem { Name = "HAR Viewer", Route = "tools/file/har", Icon = "ti-timeline", Description = "inspect HTTP Archive network traces", Tags = ["har", "http", "archive", "network", "trace", "waterfall"] },
                        new ToolItem { Name = "CSV Viewer", Route = "tools/file/csv", Icon = "ti-file-spreadsheet", Tags = ["csv", "viewer", "spreadsheet", "table", "data"] },
                    ]
                },
                new ToolGroup
                {
                    Name = "Inspect", Icon = "ti-microscope",
                    Tools =
                    [
                        new ToolItem { Name = "File Signature", Route = "tools/file/magic", Icon = "ti-file-search", Description = "identify file type from magic bytes", Tags = ["file", "signature", "magic", "bytes", "type", "identify", "header"] },
                        new ToolItem { Name = "Checksum", Route = "tools/file/checksum", Icon = "ti-shield-check", Description = "MD5/SHA file integrity verifier", Tags = ["checksum", "md5", "sha", "hash", "integrity", "verify", "file"] },
                        new ToolItem { Name = "Encoding / BOM", Route = "tools/file/encoding", Icon = "ti-file-code", Description = "detect byte-order marks & text encoding", Tags = ["encoding", "bom", "byte", "order", "mark", "utf8", "ascii", "detect"] },
                        new ToolItem { Name = "File Size Chunks", Icon = "ti-puzzle", IsPlanned = true, Tags = ["file", "size", "chunk", "split"] },
                        new ToolItem { Name = "CRC32", Icon = "ti-hash", IsPlanned = true, Tags = ["crc32", "checksum", "hash", "error", "detection"] },
                        new ToolItem { Name = "Byte Range Calculator", Icon = "ti-ruler", IsPlanned = true, Tags = ["byte", "range", "offset", "slice", "calculate"] },
                    ]
                },
            ]
        },

        // ── MISC ──
        new ToolSection
        {
            Name = "Misc",
            Icon = "ti-sparkles",
            IconColorClass = "text-gray-400",
            HoverColorClass = "hover:text-gray-600",
            Groups =
            [
                new ToolGroup
                {
                    Name = "Fun & Games", Icon = "ti-dice", IsPlanned = true,
                    Tools = []
                },
            ]
        },
    ];

    /// <summary>Flat list of every tool (active + planned) for search.</summary>
    public static IReadOnlyList<(ToolSection Section, ToolGroup Group, ToolItem Tool)> AllTools { get; } =
        Sections
            .SelectMany(s => s.Groups.SelectMany(g => g.Tools.Select(t => (s, g, t))))
            .ToList();

    /// <summary>Count of active (non-planned) tools.</summary>
    public static int ActiveToolCount { get; } =
        AllTools.Count(x => !x.Tool.IsPlanned);
}
