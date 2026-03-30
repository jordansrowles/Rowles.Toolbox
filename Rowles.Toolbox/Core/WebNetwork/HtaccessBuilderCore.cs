using System.Text;

namespace Rowles.Toolbox.Core.WebNetwork;

public static class HtaccessBuilderCore
{
    public static string BuildOutput(HtaccessOptions opts)
    {
        StringBuilder sb = new();
        bool needsSpacer = false;

        // 1. HTTPS Enforcement
        if (opts.EnableHttps)
        {
            AppendSpacer(sb, ref needsSpacer);
            sb.AppendLine("# ---- HTTPS Enforcement ----");
            sb.AppendLine("# Redirect all HTTP traffic to HTTPS");
            sb.AppendLine("<IfModule mod_rewrite.c>");
            sb.AppendLine("    RewriteEngine On");
            sb.AppendLine("    RewriteCond %{HTTPS} !=on");
            sb.AppendLine("    RewriteRule ^(.*)$ https://%{HTTP_HOST}%{REQUEST_URI} [L,R=301]");
            sb.AppendLine("</IfModule>");
            needsSpacer = true;
        }

        // 2. WWW Redirect
        if (opts.EnableWww)
        {
            AppendSpacer(sb, ref needsSpacer);
            if (opts.WwwMode == "add")
            {
                sb.AppendLine("# ---- Add WWW Prefix ----");
                sb.AppendLine("# Redirect non-www to www");
                sb.AppendLine("<IfModule mod_rewrite.c>");
                sb.AppendLine("    RewriteEngine On");
                sb.AppendLine("    RewriteCond %{HTTP_HOST} !^www\\. [NC]");
                if (!string.IsNullOrWhiteSpace(opts.WwwDomain))
                {
                    sb.AppendLine($"    RewriteRule ^(.*)$ https://www.{opts.WwwDomain.Trim().TrimStart('.')}/$1 [L,R=301]");
                }
                else
                {
                    sb.AppendLine("    RewriteRule ^(.*)$ https://www.%{HTTP_HOST}/$1 [L,R=301]");
                }
                sb.AppendLine("</IfModule>");
            }
            else
            {
                sb.AppendLine("# ---- Remove WWW Prefix ----");
                sb.AppendLine("# Redirect www to non-www");
                sb.AppendLine("<IfModule mod_rewrite.c>");
                sb.AppendLine("    RewriteEngine On");
                sb.AppendLine("    RewriteCond %{HTTP_HOST} ^www\\.(.+)$ [NC]");
                if (!string.IsNullOrWhiteSpace(opts.WwwDomain))
                {
                    sb.AppendLine($"    RewriteRule ^(.*)$ https://{opts.WwwDomain.Trim().TrimStart('.')}/$1 [L,R=301]");
                }
                else
                {
                    sb.AppendLine("    RewriteRule ^(.*)$ https://%1/$1 [L,R=301]");
                }
                sb.AppendLine("</IfModule>");
            }
            needsSpacer = true;
        }

        // 3. Custom Error Pages
        if (opts.EnableErrorPages)
        {
            AppendSpacer(sb, ref needsSpacer);
            sb.AppendLine("# ---- Custom Error Pages ----");
            sb.AppendLine("# Define custom pages for common HTTP error codes");
            if (!string.IsNullOrWhiteSpace(opts.Error404))
                sb.AppendLine($"ErrorDocument 404 {opts.Error404.Trim()}");
            if (!string.IsNullOrWhiteSpace(opts.Error403))
                sb.AppendLine($"ErrorDocument 403 {opts.Error403.Trim()}");
            if (!string.IsNullOrWhiteSpace(opts.Error500))
                sb.AppendLine($"ErrorDocument 500 {opts.Error500.Trim()}");
            needsSpacer = true;
        }

        // 4. Directory Listing
        if (opts.EnableDirListing)
        {
            AppendSpacer(sb, ref needsSpacer);
            sb.AppendLine("# ---- Disable Directory Listing ----");
            sb.AppendLine("# Prevent visitors from seeing directory contents");
            sb.AppendLine("Options -Indexes");
            needsSpacer = true;
        }

        // 5. Compression
        if (opts.EnableCompression)
        {
            List<CompressType> enabledTypes = opts.CompressTypes.FindAll(ct => ct.Enabled);
            if (enabledTypes.Count > 0)
            {
                AppendSpacer(sb, ref needsSpacer);
                sb.AppendLine("# ---- Gzip Compression ----");
                sb.AppendLine("# Compress responses to reduce transfer size");
                sb.AppendLine("<IfModule mod_deflate.c>");
                foreach (CompressType ct in enabledTypes)
                {
                    sb.AppendLine($"    AddOutputFilterByType DEFLATE {ct.MimeType}");
                }
                sb.AppendLine("</IfModule>");
                needsSpacer = true;
            }
        }

        // 6. Browser Caching
        if (opts.EnableCaching)
        {
            AppendSpacer(sb, ref needsSpacer);
            sb.AppendLine("# ---- Browser Caching ----");
            sb.AppendLine("# Set expiry headers for static assets to improve load times");
            sb.AppendLine("<IfModule mod_expires.c>");
            sb.AppendLine("    ExpiresActive On");
            sb.AppendLine($"    ExpiresDefault \"access plus {opts.CacheHtml}\"");
            sb.AppendLine();
            sb.AppendLine("    # Images");
            sb.AppendLine($"    ExpiresByType image/jpeg \"access plus {opts.CacheImages}\"");
            sb.AppendLine($"    ExpiresByType image/png \"access plus {opts.CacheImages}\"");
            sb.AppendLine($"    ExpiresByType image/gif \"access plus {opts.CacheImages}\"");
            sb.AppendLine($"    ExpiresByType image/webp \"access plus {opts.CacheImages}\"");
            sb.AppendLine($"    ExpiresByType image/svg+xml \"access plus {opts.CacheImages}\"");
            sb.AppendLine($"    ExpiresByType image/x-icon \"access plus {opts.CacheImages}\"");
            sb.AppendLine();
            sb.AppendLine("    # CSS and JavaScript");
            sb.AppendLine($"    ExpiresByType text/css \"access plus {opts.CacheCssJs}\"");
            sb.AppendLine($"    ExpiresByType application/javascript \"access plus {opts.CacheCssJs}\"");
            sb.AppendLine();
            sb.AppendLine("    # HTML");
            sb.AppendLine($"    ExpiresByType text/html \"access plus {opts.CacheHtml}\"");
            sb.AppendLine("</IfModule>");
            needsSpacer = true;
        }

        // 7. Security Headers
        if (opts.EnableSecHeaders)
        {
            bool hasAnyHeader = opts.SecXContentType || opts.SecXFrame || opts.SecXss
                || opts.SecReferrer || opts.SecCsp || opts.SecPermissions;
            if (hasAnyHeader)
            {
                AppendSpacer(sb, ref needsSpacer);
                sb.AppendLine("# ---- Security Headers ----");
                sb.AppendLine("# Harden the site by setting security-related HTTP headers");
                sb.AppendLine("<IfModule mod_headers.c>");
                if (opts.SecXContentType)
                    sb.AppendLine("    Header always set X-Content-Type-Options \"nosniff\"");
                if (opts.SecXFrame)
                    sb.AppendLine($"    Header always set X-Frame-Options \"{opts.SecXFrameValue}\"");
                if (opts.SecXss)
                    sb.AppendLine("    Header always set X-XSS-Protection \"1; mode=block\"");
                if (opts.SecReferrer)
                    sb.AppendLine($"    Header always set Referrer-Policy \"{opts.SecReferrerValue}\"");
                if (opts.SecCsp && !string.IsNullOrWhiteSpace(opts.SecCspValue))
                    sb.AppendLine($"    Header always set Content-Security-Policy \"{opts.SecCspValue.Trim()}\"");
                if (opts.SecPermissions && !string.IsNullOrWhiteSpace(opts.SecPermissionsValue))
                    sb.AppendLine($"    Header always set Permissions-Policy \"{opts.SecPermissionsValue.Trim()}\"");
                sb.AppendLine("</IfModule>");
                needsSpacer = true;
            }
        }

        // 8. Hotlink Protection
        if (opts.EnableHotlink)
        {
            AppendSpacer(sb, ref needsSpacer);
            string domain = !string.IsNullOrWhiteSpace(opts.HotlinkDomain)
                ? opts.HotlinkDomain.Trim()
                : "example.com";
            string extensions = !string.IsNullOrWhiteSpace(opts.HotlinkExtensions)
                ? opts.HotlinkExtensions.Trim()
                : "jpg|jpeg|png|gif|webp|svg";
            sb.AppendLine("# ---- Hotlink Protection ----");
            sb.AppendLine("# Prevent other sites from embedding your images directly");
            sb.AppendLine("<IfModule mod_rewrite.c>");
            sb.AppendLine("    RewriteEngine On");
            sb.AppendLine($"    RewriteCond %{{HTTP_REFERER}} !^$");
            sb.AppendLine($"    RewriteCond %{{HTTP_REFERER}} !^https?://(www\\.)?{EscapeDots(domain)} [NC]");
            sb.AppendLine($"    RewriteRule \\.({extensions})$ - [F,L]");
            sb.AppendLine("</IfModule>");
            needsSpacer = true;
        }

        // 9. IP Block/Allow
        if (opts.EnableIpRules && !string.IsNullOrWhiteSpace(opts.IpList))
        {
            string[] ips = opts.IpList.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                AppendSpacer(sb, ref needsSpacer);
                if (opts.IpMode == "block")
                {
                    sb.AppendLine("# ---- IP Blocking ----");
                    sb.AppendLine("# Deny access from specific IP addresses");
                    sb.AppendLine("<RequireAll>");
                    sb.AppendLine("    Require all granted");
                    foreach (string ip in ips)
                    {
                        string trimmed = ip.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                            sb.AppendLine($"    Require not ip {trimmed}");
                    }
                    sb.AppendLine("</RequireAll>");
                }
                else
                {
                    sb.AppendLine("# ---- IP Allowlist ----");
                    sb.AppendLine("# Allow access only from specific IP addresses");
                    sb.AppendLine("<RequireAny>");
                    foreach (string ip in ips)
                    {
                        string trimmed = ip.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                            sb.AppendLine($"    Require ip {trimmed}");
                    }
                    sb.AppendLine("</RequireAny>");
                }
                needsSpacer = true;
            }
        }

        // 10. Custom Redirects
        if (opts.EnableRedirects && opts.RedirectRules.Count > 0)
        {
            List<RedirectRule> validRules = opts.RedirectRules.FindAll(
                r => !string.IsNullOrWhiteSpace(r.FromPath) && !string.IsNullOrWhiteSpace(r.ToUrl));
            if (validRules.Count > 0)
            {
                AppendSpacer(sb, ref needsSpacer);
                sb.AppendLine("# ---- Custom Redirects ----");
                sb.AppendLine("# Redirect old URLs to new locations");
                foreach (RedirectRule rule in validRules)
                {
                    sb.AppendLine($"Redirect {rule.StatusCode} {rule.FromPath.Trim()} {rule.ToUrl.Trim()}");
                }
                needsSpacer = true;
            }
        }

        // 11. CORS Headers
        if (opts.EnableCors)
        {
            AppendSpacer(sb, ref needsSpacer);
            string origin = !string.IsNullOrWhiteSpace(opts.CorsOrigin)
                ? opts.CorsOrigin.Trim()
                : "*";
            sb.AppendLine("# ---- CORS Headers ----");
            sb.AppendLine("# Allow cross-origin requests from specified origins");
            sb.AppendLine("<IfModule mod_headers.c>");
            sb.AppendLine($"    Header set Access-Control-Allow-Origin \"{origin}\"");
            if (opts.CorsMethods && !string.IsNullOrWhiteSpace(opts.CorsMethodsValue))
                sb.AppendLine($"    Header set Access-Control-Allow-Methods \"{opts.CorsMethodsValue.Trim()}\"");
            if (opts.CorsHeaders && !string.IsNullOrWhiteSpace(opts.CorsHeadersValue))
                sb.AppendLine($"    Header set Access-Control-Allow-Headers \"{opts.CorsHeadersValue.Trim()}\"");
            sb.AppendLine("</IfModule>");
            needsSpacer = true;
        }

        // 12. PHP Settings
        if (opts.EnablePhp)
        {
            bool hasAnyPhp = opts.PhpDisplayErrors || opts.PhpMaxExec || opts.PhpUploadSize || opts.PhpMemory;
            if (hasAnyPhp)
            {
                AppendSpacer(sb, ref needsSpacer);
                sb.AppendLine("# ---- PHP Settings ----");
                sb.AppendLine("# Override PHP configuration values");
                if (opts.PhpDisplayErrors)
                    sb.AppendLine($"php_flag display_errors {opts.PhpDisplayErrorsValue}");
                if (opts.PhpMaxExec)
                    sb.AppendLine($"php_value max_execution_time {opts.PhpMaxExecValue}");
                if (opts.PhpUploadSize && !string.IsNullOrWhiteSpace(opts.PhpUploadSizeValue))
                    sb.AppendLine($"php_value upload_max_filesize {opts.PhpUploadSizeValue.Trim()}");
                if (opts.PhpMemory && !string.IsNullOrWhiteSpace(opts.PhpMemoryValue))
                    sb.AppendLine($"php_value memory_limit {opts.PhpMemoryValue.Trim()}");
                needsSpacer = true;
            }
        }

        // 13. File Access Restriction
        if (opts.EnableFileBlock)
        {
            List<BlockedFileType> enabledBlocks = opts.BlockedFileTypes.FindAll(ft => ft.Enabled);
            if (enabledBlocks.Count > 0)
            {
                AppendSpacer(sb, ref needsSpacer);
                sb.AppendLine("# ---- File Access Restriction ----");
                sb.AppendLine("# Block access to sensitive files and directories");
                foreach (BlockedFileType ft in enabledBlocks)
                {
                    string pattern = ft.Pattern.Trim();
                    if (pattern.Contains('.') && !pattern.Contains('*'))
                    {
                        if (pattern.StartsWith('.'))
                        {
                            string escaped = pattern.Replace(".", "\\.");
                            sb.AppendLine($"<FilesMatch \"^{escaped}\">");
                            sb.AppendLine("    Require all denied");
                            sb.AppendLine("</FilesMatch>");
                        }
                        else
                        {
                            sb.AppendLine($"<Files \"{pattern}\">");
                            sb.AppendLine("    Require all denied");
                            sb.AppendLine("</Files>");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"<Files \"{pattern}\">");
                        sb.AppendLine("    Require all denied");
                        sb.AppendLine("</Files>");
                    }
                }
            }
        }

        return sb.ToString().TrimEnd();
    }

    public static int CountActiveSnippets(HtaccessOptions opts)
    {
        int count = 0;
        if (opts.EnableHttps) count++;
        if (opts.EnableWww) count++;
        if (opts.EnableErrorPages) count++;
        if (opts.EnableDirListing) count++;
        if (opts.EnableCompression) count++;
        if (opts.EnableCaching) count++;
        if (opts.EnableSecHeaders) count++;
        if (opts.EnableHotlink) count++;
        if (opts.EnableIpRules) count++;
        if (opts.EnableRedirects && opts.RedirectRules.Count > 0) count++;
        if (opts.EnableCors) count++;
        if (opts.EnablePhp) count++;
        if (opts.EnableFileBlock) count++;
        return count;
    }

    public static string EscapeDots(string domain)
    {
        return domain.Replace(".", "\\.");
    }

    public static void AppendSpacer(StringBuilder sb, ref bool needsSpacer)
    {
        if (needsSpacer)
        {
            sb.AppendLine();
        }
    }

    // ── Inner types ──

    public sealed class HtaccessOptions
    {
        // 1. HTTPS
        public bool EnableHttps { get; set; }

        // 2. WWW
        public bool EnableWww { get; set; }
        public string WwwMode { get; set; } = "remove";
        public string WwwDomain { get; set; } = string.Empty;

        // 3. Error Pages
        public bool EnableErrorPages { get; set; }
        public string Error404 { get; set; } = "/errors/404.html";
        public string Error403 { get; set; } = "/errors/403.html";
        public string Error500 { get; set; } = "/errors/500.html";

        // 4. Directory Listing
        public bool EnableDirListing { get; set; }

        // 5. Compression
        public bool EnableCompression { get; set; }
        public List<CompressType> CompressTypes { get; set; } = [];

        // 6. Browser Caching
        public bool EnableCaching { get; set; }
        public string CacheImages { get; set; } = "1 month";
        public string CacheCssJs { get; set; } = "1 week";
        public string CacheHtml { get; set; } = "1 hour";

        // 7. Security Headers
        public bool EnableSecHeaders { get; set; }
        public bool SecXContentType { get; set; }
        public bool SecXFrame { get; set; }
        public string SecXFrameValue { get; set; } = "DENY";
        public bool SecXss { get; set; }
        public bool SecReferrer { get; set; }
        public string SecReferrerValue { get; set; } = "strict-origin-when-cross-origin";
        public bool SecCsp { get; set; }
        public string SecCspValue { get; set; } = string.Empty;
        public bool SecPermissions { get; set; }
        public string SecPermissionsValue { get; set; } = string.Empty;

        // 8. Hotlink Protection
        public bool EnableHotlink { get; set; }
        public string HotlinkDomain { get; set; } = string.Empty;
        public string HotlinkExtensions { get; set; } = "jpg|jpeg|png|gif|webp|svg";

        // 9. IP Rules
        public bool EnableIpRules { get; set; }
        public string IpMode { get; set; } = "block";
        public string IpList { get; set; } = string.Empty;

        // 10. Redirects
        public bool EnableRedirects { get; set; }
        public List<RedirectRule> RedirectRules { get; set; } = [];

        // 11. CORS
        public bool EnableCors { get; set; }
        public string CorsOrigin { get; set; } = "*";
        public bool CorsMethods { get; set; }
        public string CorsMethodsValue { get; set; } = string.Empty;
        public bool CorsHeaders { get; set; }
        public string CorsHeadersValue { get; set; } = string.Empty;

        // 12. PHP
        public bool EnablePhp { get; set; }
        public bool PhpDisplayErrors { get; set; }
        public string PhpDisplayErrorsValue { get; set; } = "Off";
        public bool PhpMaxExec { get; set; }
        public int PhpMaxExecValue { get; set; } = 30;
        public bool PhpUploadSize { get; set; }
        public string PhpUploadSizeValue { get; set; } = "64M";
        public bool PhpMemory { get; set; }
        public string PhpMemoryValue { get; set; } = "256M";

        // 13. File Access
        public bool EnableFileBlock { get; set; }
        public List<BlockedFileType> BlockedFileTypes { get; set; } = [];
    }

    public sealed class CompressType
    {
        public string MimeType { get; init; } = string.Empty;
        public bool Enabled { get; set; }
    }

    public sealed class BlockedFileType
    {
        public string Pattern { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public bool Enabled { get; set; }
    }

    public sealed class RedirectRule
    {
        public string StatusCode { get; set; } = "301";
        public string FromPath { get; set; } = string.Empty;
        public string ToUrl { get; set; } = string.Empty;
    }
}
