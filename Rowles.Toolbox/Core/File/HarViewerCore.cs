namespace Rowles.Toolbox.Core.File;

public static class HarViewerCore
{
    public sealed class HarEntry
    {
        public string Method { get; set; } = "";
        public string Url { get; set; } = "";
        public int Status { get; set; }
        public long ResponseSize { get; set; }
        public double Time { get; set; }
        public string ContentType { get; set; } = "";
        public double StartedMs { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; } = new();
        public Dictionary<string, string> ResponseHeaders { get; set; } = new();
        public Dictionary<string, double> Timings { get; set; } = new();
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024):F1} MB";
    }

    public static string TruncateUrl(string url)
    {
        if (url.Length <= 50) return url;
        try
        {
            Uri uri = new(url);
            string path = uri.AbsolutePath;
            return path.Length > 40 ? $"…{path[^40..]}" : path;
        }
        catch
        {
            return url[..47] + "…";
        }
    }

    public static string MethodColor(string method) => method.ToUpperInvariant() switch
    {
        "GET" => "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400",
        "POST" => "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
        "PUT" => "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400",
        "PATCH" => "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400",
        "DELETE" => "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
        _ => "bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300",
    };

    public static string StatusColor(int status) => status switch
    {
        >= 200 and < 300 => "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400",
        >= 300 and < 400 => "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400",
        >= 400 and < 500 => "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
        >= 500 => "bg-red-200 text-red-800 dark:bg-red-900/50 dark:text-red-300",
        _ => "bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300",
    };

    public static string StatusBarColor(int status) => status switch
    {
        >= 200 and < 300 => "bg-green-500",
        >= 300 and < 400 => "bg-amber-500",
        >= 400 and < 500 => "bg-red-400",
        >= 500 => "bg-red-600",
        _ => "bg-gray-400",
    };

    public static List<HarEntry> FilterEntries(List<HarEntry> entries, string statusFilter,
        string contentTypeFilter, string urlFilter, string sortColumn, bool sortAscending)
    {
        IEnumerable<HarEntry> result = entries;

        if (!string.IsNullOrEmpty(statusFilter))
        {
            result = statusFilter switch
            {
                "2xx" => result.Where(e => e.Status >= 200 && e.Status < 300),
                "3xx" => result.Where(e => e.Status >= 300 && e.Status < 400),
                "4xx" => result.Where(e => e.Status >= 400 && e.Status < 500),
                "5xx" => result.Where(e => e.Status >= 500),
                _ => result
            };
        }

        if (!string.IsNullOrEmpty(contentTypeFilter))
        {
            result = result.Where(e => string.Equals(e.ContentType, contentTypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(urlFilter))
        {
            result = result.Where(e => e.Url.Contains(urlFilter, StringComparison.OrdinalIgnoreCase));
        }

        IOrderedEnumerable<HarEntry> sorted = sortColumn switch
        {
            "method" => sortAscending ? result.OrderBy(e => e.Method) : result.OrderByDescending(e => e.Method),
            "url" => sortAscending ? result.OrderBy(e => e.Url) : result.OrderByDescending(e => e.Url),
            "status" => sortAscending ? result.OrderBy(e => e.Status) : result.OrderByDescending(e => e.Status),
            "size" => sortAscending ? result.OrderBy(e => e.ResponseSize) : result.OrderByDescending(e => e.ResponseSize),
            "type" => sortAscending ? result.OrderBy(e => e.ContentType) : result.OrderByDescending(e => e.ContentType),
            _ => sortAscending ? result.OrderBy(e => e.Time) : result.OrderByDescending(e => e.Time),
        };

        return sorted.ToList();
    }
}
