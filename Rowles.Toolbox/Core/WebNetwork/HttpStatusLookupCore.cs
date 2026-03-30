namespace Rowles.Toolbox.Core.WebNetwork;

public static class HttpStatusLookupCore
{
    public sealed record HttpStatus(int Code, string Name, string Description, string Category);
    public sealed record MimeEntry(string Type, string Description, string? Extensions);

    public static string StatusBadgeClass(int code) => code switch
    {
        < 200 => "bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300",
        < 300 => "bg-green-100 dark:bg-green-900 text-green-700 dark:text-green-300",
        < 400 => "bg-yellow-100 dark:bg-yellow-900 text-yellow-700 dark:text-yellow-300",
        < 500 => "bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-300",
        _ => "bg-purple-100 dark:bg-purple-900 text-purple-700 dark:text-purple-300",
    };

    public static readonly List<HttpStatus> AllStatuses =
    [
        // 1xx Informational
        new(100, "Continue", "Server received request headers; client should proceed to send body.", "1xx Informational"),
        new(101, "Switching Protocols", "Server is switching protocols as requested by the client.", "1xx Informational"),
        new(102, "Processing", "Server has received and is processing the request (WebDAV).", "1xx Informational"),
        new(103, "Early Hints", "Used to return response headers before the final HTTP message.", "1xx Informational"),

        // 2xx Success
        new(200, "OK", "The request has succeeded.", "2xx Success"),
        new(201, "Created", "The request has been fulfilled and a new resource has been created.", "2xx Success"),
        new(202, "Accepted", "The request has been accepted for processing, but not completed.", "2xx Success"),
        new(203, "Non-Authoritative Information", "The returned information is from a local or third-party copy.", "2xx Success"),
        new(204, "No Content", "The server successfully processed the request but returns no content.", "2xx Success"),
        new(205, "Reset Content", "The server processed the request; the client should reset the document view.", "2xx Success"),
        new(206, "Partial Content", "The server is delivering part of the resource due to a range header.", "2xx Success"),
        new(207, "Multi-Status", "The message body contains multiple status codes (WebDAV).", "2xx Success"),
        new(208, "Already Reported", "The members have already been enumerated in a prior response (WebDAV).", "2xx Success"),
        new(226, "IM Used", "The server has fulfilled a GET request using instance manipulations.", "2xx Success"),

        // 3xx Redirection
        new(300, "Multiple Choices", "The request has more than one possible response.", "3xx Redirection"),
        new(301, "Moved Permanently", "The resource has been permanently moved to a new URL.", "3xx Redirection"),
        new(302, "Found", "The resource is temporarily at a different URL.", "3xx Redirection"),
        new(303, "See Other", "The response can be found under a different URL using a GET request.", "3xx Redirection"),
        new(304, "Not Modified", "The resource has not been modified since the last request.", "3xx Redirection"),
        new(305, "Use Proxy", "The requested resource must be accessed through a proxy (deprecated).", "3xx Redirection"),
        new(307, "Temporary Redirect", "The resource is temporarily at a different URL; method preserved.", "3xx Redirection"),
        new(308, "Permanent Redirect", "The resource has permanently moved; method preserved.", "3xx Redirection"),

        // 4xx Client Error
        new(400, "Bad Request", "The server cannot process the request due to a client error.", "4xx Client Error"),
        new(401, "Unauthorized", "Authentication is required to access the resource.", "4xx Client Error"),
        new(402, "Payment Required", "Reserved for future use; sometimes used for digital payment systems.", "4xx Client Error"),
        new(403, "Forbidden", "The server understood the request but refuses to authorise it.", "4xx Client Error"),
        new(404, "Not Found", "The requested resource could not be found.", "4xx Client Error"),
        new(405, "Method Not Allowed", "The request method is not supported for the resource.", "4xx Client Error"),
        new(406, "Not Acceptable", "The resource cannot generate content acceptable per the Accept headers.", "4xx Client Error"),
        new(407, "Proxy Authentication Required", "Authentication with the proxy is required.", "4xx Client Error"),
        new(408, "Request Timeout", "The server timed out waiting for the request.", "4xx Client Error"),
        new(409, "Conflict", "The request conflicts with the current state of the resource.", "4xx Client Error"),
        new(410, "Gone", "The resource is no longer available and will not be available again.", "4xx Client Error"),
        new(411, "Length Required", "The request did not specify the Content-Length required by the server.", "4xx Client Error"),
        new(412, "Precondition Failed", "One or more preconditions in the request headers evaluated to false.", "4xx Client Error"),
        new(413, "Payload Too Large", "The request entity is larger than the server is willing to process.", "4xx Client Error"),
        new(414, "URI Too Long", "The URI provided was too long for the server to process.", "4xx Client Error"),
        new(415, "Unsupported Media Type", "The request entity has a media type the server does not support.", "4xx Client Error"),
        new(416, "Range Not Satisfiable", "The range specified in the Range header cannot be fulfilled.", "4xx Client Error"),
        new(417, "Expectation Failed", "The expectation given in the Expect header could not be met.", "4xx Client Error"),
        new(418, "I'm a Teapot", "The server refuses to brew coffee because it is, permanently, a teapot.", "4xx Client Error"),
        new(421, "Misdirected Request", "The request was directed at a server unable to produce a response.", "4xx Client Error"),
        new(422, "Unprocessable Content", "The request was well-formed but contained semantic errors.", "4xx Client Error"),
        new(423, "Locked", "The resource being accessed is locked (WebDAV).", "4xx Client Error"),
        new(424, "Failed Dependency", "The request failed due to failure of a previous request (WebDAV).", "4xx Client Error"),
        new(425, "Too Early", "The server is unwilling to process a request that might be replayed.", "4xx Client Error"),
        new(426, "Upgrade Required", "The server refuses the request using the current protocol.", "4xx Client Error"),
        new(428, "Precondition Required", "The server requires the request to be conditional.", "4xx Client Error"),
        new(429, "Too Many Requests", "The user has sent too many requests in a given amount of time.", "4xx Client Error"),
        new(431, "Request Header Fields Too Large", "The server refuses the request because headers are too large.", "4xx Client Error"),
        new(451, "Unavailable For Legal Reasons", "The resource is unavailable due to legal demands.", "4xx Client Error"),

        // 5xx Server Error
        new(500, "Internal Server Error", "The server encountered an unexpected condition.", "5xx Server Error"),
        new(501, "Not Implemented", "The server does not support the functionality required to fulfil the request.", "5xx Server Error"),
        new(502, "Bad Gateway", "The server received an invalid response from an upstream server.", "5xx Server Error"),
        new(503, "Service Unavailable", "The server is currently unable to handle the request.", "5xx Server Error"),
        new(504, "Gateway Timeout", "The server did not receive a timely response from an upstream server.", "5xx Server Error"),
        new(505, "HTTP Version Not Supported", "The server does not support the HTTP version used in the request.", "5xx Server Error"),
        new(506, "Variant Also Negotiates", "The server has an internal configuration error.", "5xx Server Error"),
        new(507, "Insufficient Storage", "The server cannot store the representation needed (WebDAV).", "5xx Server Error"),
        new(508, "Loop Detected", "The server detected an infinite loop while processing the request (WebDAV).", "5xx Server Error"),
        new(510, "Not Extended", "Further extensions to the request are required for the server to fulfil it.", "5xx Server Error"),
        new(511, "Network Authentication Required", "The client needs to authenticate to gain network access.", "5xx Server Error"),
    ];

    public static readonly List<MimeEntry> AllMimeTypes =
    [
        // Application
        new("application/json", "JSON data", ".json"),
        new("application/xml", "XML data", ".xml"),
        new("application/javascript", "JavaScript", ".js, .mjs"),
        new("application/pdf", "PDF document", ".pdf"),
        new("application/zip", "ZIP archive", ".zip"),
        new("application/gzip", "Gzip archive", ".gz"),
        new("application/x-tar", "Tar archive", ".tar"),
        new("application/x-7z-compressed", "7-Zip archive", ".7z"),
        new("application/octet-stream", "Binary data (unknown type)", null),
        new("application/x-www-form-urlencoded", "URL-encoded form data", null),
        new("application/ld+json", "JSON-LD linked data", ".jsonld"),
        new("application/graphql+json", "GraphQL query", null),
        new("application/wasm", "WebAssembly", ".wasm"),
        new("application/sql", "SQL query", ".sql"),
        new("application/rtf", "Rich Text Format", ".rtf"),
        new("application/xhtml+xml", "XHTML document", ".xhtml"),
        new("application/vnd.ms-excel", "Excel spreadsheet (legacy)", ".xls"),
        new("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Excel spreadsheet", ".xlsx"),
        new("application/vnd.ms-powerpoint", "PowerPoint (legacy)", ".ppt"),
        new("application/vnd.openxmlformats-officedocument.presentationml.presentation", "PowerPoint", ".pptx"),
        new("application/msword", "Word document (legacy)", ".doc"),
        new("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Word document", ".docx"),
        new("application/x-yaml", "YAML data", ".yaml, .yml"),

        // Text
        new("text/html", "HTML document", ".html, .htm"),
        new("text/css", "CSS stylesheet", ".css"),
        new("text/plain", "Plain text", ".txt"),
        new("text/csv", "Comma-separated values", ".csv"),
        new("text/markdown", "Markdown", ".md"),
        new("text/xml", "XML document", ".xml"),
        new("text/javascript", "JavaScript (obsolete MIME)", ".js"),
        new("text/calendar", "iCalendar data", ".ics"),
        new("text/x-csharp", "C# source code", ".cs"),

        // Image
        new("image/png", "PNG image", ".png"),
        new("image/jpeg", "JPEG image", ".jpg, .jpeg"),
        new("image/gif", "GIF image", ".gif"),
        new("image/svg+xml", "SVG vector image", ".svg"),
        new("image/webp", "WebP image", ".webp"),
        new("image/avif", "AVIF image", ".avif"),
        new("image/x-icon", "ICO icon", ".ico"),
        new("image/tiff", "TIFF image", ".tiff, .tif"),
        new("image/bmp", "BMP image", ".bmp"),

        // Audio
        new("audio/mpeg", "MP3 audio", ".mp3"),
        new("audio/ogg", "OGG audio", ".ogg"),
        new("audio/wav", "WAV audio", ".wav"),
        new("audio/webm", "WebM audio", ".weba"),
        new("audio/flac", "FLAC audio", ".flac"),
        new("audio/aac", "AAC audio", ".aac"),

        // Video
        new("video/mp4", "MP4 video", ".mp4"),
        new("video/webm", "WebM video", ".webm"),
        new("video/ogg", "OGG video", ".ogv"),
        new("video/mpeg", "MPEG video", ".mpeg"),
        new("video/quicktime", "QuickTime video", ".mov"),
        new("video/x-matroska", "Matroska video", ".mkv"),

        // Font
        new("font/woff", "WOFF font", ".woff"),
        new("font/woff2", "WOFF2 font", ".woff2"),
        new("font/ttf", "TrueType font", ".ttf"),
        new("font/otf", "OpenType font", ".otf"),

        // Multipart
        new("multipart/form-data", "Form data with file uploads", null),
        new("multipart/byteranges", "Partial content with byte ranges", null),
    ];
}
