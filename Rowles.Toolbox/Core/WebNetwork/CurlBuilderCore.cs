namespace Rowles.Toolbox.Core.WebNetwork;

public static class CurlBuilderCore
{
    public sealed class CurlHeader
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public sealed record CurlOptions(
        string Method,
        string Url,
        List<CurlHeader> Headers,
        string BodyContent,
        string BodyMode,
        string AuthType,
        string BasicUser,
        string BasicPass,
        string BearerToken,
        string ApiKeyHeader,
        string ApiKeyValue,
        bool Verbose,
        bool Insecure,
        bool FollowRedirects,
        bool Silent,
        bool HeadOnly,
        bool Compressed,
        bool Http2,
        bool Http11,
        bool OutputFile,
        string OutputFilename,
        bool ConnectTimeout,
        int ConnectTimeoutSeconds,
        bool MaxTime,
        int MaxTimeSeconds,
        bool MultiLine);

    public static string BuildCurlCommand(CurlOptions options)
    {
        List<string> parts = new();
        parts.Add("curl");

        if (options.Method != "GET")
            parts.Add($"-X {options.Method}");

        if (options.Verbose) parts.Add("-v");
        if (options.Silent) parts.Add("-s");
        if (options.Insecure) parts.Add("-k");
        if (options.FollowRedirects) parts.Add("-L");
        if (options.HeadOnly) parts.Add("-I");
        if (options.Compressed) parts.Add("--compressed");
        if (options.Http2) parts.Add("--http2");
        if (options.Http11) parts.Add("--http1.1");
        if (options.ConnectTimeout) parts.Add($"--connect-timeout {options.ConnectTimeoutSeconds}");
        if (options.MaxTime) parts.Add($"--max-time {options.MaxTimeSeconds}");
        if (options.OutputFile && !string.IsNullOrWhiteSpace(options.OutputFilename))
            parts.Add($"-o \"{options.OutputFilename}\"");

        if (options.AuthType == "basic"
            && (!string.IsNullOrWhiteSpace(options.BasicUser) || !string.IsNullOrWhiteSpace(options.BasicPass)))
        {
            parts.Add($"-u \"{options.BasicUser}:{options.BasicPass}\"");
        }
        else if (options.AuthType == "bearer" && !string.IsNullOrWhiteSpace(options.BearerToken))
        {
            parts.Add($"-H \"Authorization: Bearer {options.BearerToken}\"");
        }
        else if (options.AuthType == "apikey" && !string.IsNullOrWhiteSpace(options.ApiKeyValue))
        {
            parts.Add($"-H \"{options.ApiKeyHeader}: {options.ApiKeyValue}\"");
        }

        foreach (CurlHeader header in options.Headers)
        {
            if (!string.IsNullOrWhiteSpace(header.Key))
                parts.Add($"-H \"{header.Key}: {header.Value}\"");
        }

        bool hasBody = !string.IsNullOrWhiteSpace(options.BodyContent)
                       && options.Method != "GET"
                       && options.Method != "HEAD"
                       && options.Method != "OPTIONS";
        if (hasBody)
        {
            if (options.BodyMode == "form")
            {
                string[] lines = options.BodyContent.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    parts.Add($"-F \"{line.Trim()}\"");
                }
            }
            else
            {
                string escapedBody = options.BodyContent.Replace("'", "'\\''");
                parts.Add($"-d '{escapedBody}'");
            }
        }

        string urlPart = !string.IsNullOrWhiteSpace(options.Url) ? $"\"{options.Url}\"" : "\"https://example.com\"";
        parts.Add(urlPart);

        if (options.MultiLine)
            return string.Join(" \\\n  ", parts);
        else
            return string.Join(" ", parts);
    }

    public static string BuildPowerShellCommand(CurlOptions options)
    {
        List<string> parts = new();
        parts.Add("Invoke-RestMethod");
        parts.Add($"-Method {options.Method}");

        string urlValue = !string.IsNullOrWhiteSpace(options.Url) ? options.Url : "https://example.com";
        parts.Add($"-Uri \"{urlValue}\"");

        List<CurlHeader> allHeaders = new(
            options.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)));

        if (options.AuthType == "bearer" && !string.IsNullOrWhiteSpace(options.BearerToken))
            allHeaders.Add(new CurlHeader { Key = "Authorization", Value = $"Bearer {options.BearerToken}" });
        else if (options.AuthType == "apikey" && !string.IsNullOrWhiteSpace(options.ApiKeyValue))
            allHeaders.Add(new CurlHeader { Key = options.ApiKeyHeader, Value = options.ApiKeyValue });

        if (allHeaders.Count > 0)
        {
            if (options.MultiLine)
            {
                string headerLines = string.Join("\n",
                    allHeaders.Select(h => $"    \"{h.Key}\" = \"{h.Value}\""));
                parts.Add($"-Headers @{{\n{headerLines}\n  }}");
            }
            else
            {
                string headerInline = string.Join("; ",
                    allHeaders.Select(h => $"\"{h.Key}\" = \"{h.Value}\""));
                parts.Add($"-Headers @{{{headerInline}}}");
            }
        }

        if (options.AuthType == "basic")
            parts.Add("-Credential (Get-Credential)");

        bool hasBody = !string.IsNullOrWhiteSpace(options.BodyContent)
                       && options.Method != "GET"
                       && options.Method != "HEAD"
                       && options.Method != "OPTIONS";
        if (hasBody)
        {
            string escapedBody = options.BodyContent.Replace("'", "''");
            parts.Add($"-Body '{escapedBody}'");
        }

        if (options.Insecure) parts.Add("-SkipCertificateCheck");
        if (options.FollowRedirects) parts.Add("-MaximumRedirection 10");
        if (options.MaxTime) parts.Add($"-TimeoutSec {options.MaxTimeSeconds}");
        if (options.OutputFile && !string.IsNullOrWhiteSpace(options.OutputFilename))
            parts.Add($"-OutFile \"{options.OutputFilename}\"");

        if (options.MultiLine)
            return string.Join(" `\n  ", parts);
        else
            return string.Join(" ", parts);
    }
}
