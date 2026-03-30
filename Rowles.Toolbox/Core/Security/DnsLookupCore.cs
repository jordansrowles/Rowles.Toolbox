namespace Rowles.Toolbox.Core.Security;

public static class DnsLookupCore
{
    public static string FormatTtl(int ttl) => ttl switch
    {
        < 60 => $"{ttl}s",
        < 3600 => $"{ttl / 60}m",
        < 86400 => $"{ttl / 3600}h",
        _ => $"{ttl / 86400}d"
    };

    public static string RecordTypeColor(string type) => type switch
    {
        "A" => "text-blue-600 dark:text-blue-400",
        "AAAA" => "text-violet-600 dark:text-violet-400",
        "MX" => "text-orange-600 dark:text-orange-400",
        "TXT" => "text-green-600 dark:text-green-400",
        "CNAME" => "text-pink-600 dark:text-pink-400",
        "NS" => "text-amber-600 dark:text-amber-400",
        "SOA" => "text-gray-600 dark:text-gray-400",
        _ => "text-gray-600 dark:text-gray-400"
    };

    public static int DnsTypeCode(string type) => type switch
    {
        "A" => 1,
        "NS" => 2,
        "CNAME" => 5,
        "SOA" => 6,
        "MX" => 15,
        "TXT" => 16,
        "AAAA" => 28,
        _ => 255
    };

    public static string DnsStatusName(int status) => status switch
    {
        0 => "NOERROR",
        1 => "FORMERR",
        2 => "SERVFAIL",
        3 => "NXDOMAIN",
        4 => "NOTIMP",
        5 => "REFUSED",
        _ => "UNKNOWN"
    };
}
