namespace Rowles.Toolbox.Core.Developer;

public static class ErrorCodeReferenceCore
{
    public sealed record ErrorCode(string Code, string Name, string Description, string Category);
    public sealed record GrpcCode(string Code, string Name, string Description, string HttpEquivalent);

    public static bool MatchesSearch(ErrorCode code, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        string trimmed = query.Trim();
        return code.Code.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
            || code.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
            || code.Description.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
            || code.Category.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
    }

    public static bool MatchesGrpcSearch(GrpcCode code, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        string trimmed = query.Trim();
        return code.Code.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
            || code.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
            || code.Description.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
            || code.HttpEquivalent.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
    }

    public static string GetHttpCodeColor(string category) => category switch
    {
        "1xx Informational" => "text-blue-600 dark:text-blue-400",
        "2xx Success" => "text-green-600 dark:text-green-400",
        "3xx Redirection" => "text-yellow-600 dark:text-yellow-400",
        "4xx Client Error" => "text-orange-600 dark:text-orange-400",
        "5xx Server Error" => "text-red-600 dark:text-red-400",
        _ => "text-gray-600 dark:text-gray-400"
    };

    public static string GetHttpBadgeColor(string category) => category switch
    {
        "1xx Informational" => "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300",
        "2xx Success" => "bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300",
        "3xx Redirection" => "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-700 dark:text-yellow-300",
        "4xx Client Error" => "bg-orange-100 dark:bg-orange-900/30 text-orange-700 dark:text-orange-300",
        "5xx Server Error" => "bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300",
        _ => "bg-gray-100 dark:bg-gray-900/30 text-gray-700 dark:text-gray-300"
    };

    public static readonly List<ErrorCode> HttpCodes =
    [
        // 1xx Informational
        new("100", "Continue", "The server has received the request headers and the client should proceed to send the request body.", "1xx Informational"),
        new("101", "Switching Protocols", "The server is switching protocols as requested by the client via the Upgrade header.", "1xx Informational"),
        new("102", "Processing", "The server has received and is processing the request, but no response is available yet.", "1xx Informational"),
        new("103", "Early Hints", "Used to return some response headers before the final HTTP message, allowing preloading of resources.", "1xx Informational"),

        // 2xx Success
        new("200", "OK", "The request has succeeded. The meaning depends on the HTTP method used.", "2xx Success"),
        new("201", "Created", "The request has been fulfilled and a new resource has been created.", "2xx Success"),
        new("202", "Accepted", "The request has been accepted for processing, but processing has not been completed.", "2xx Success"),
        new("204", "No Content", "The server successfully processed the request but is not returning any content.", "2xx Success"),
        new("206", "Partial Content", "The server is delivering only part of the resource due to a range header sent by the client.", "2xx Success"),
        new("207", "Multi-Status", "A Multi-Status response conveys information about multiple resources in situations where multiple status codes might be appropriate.", "2xx Success"),

        // 3xx Redirection
        new("301", "Moved Permanently", "The resource has been permanently moved to a new URL. Future requests should use the new URL.", "3xx Redirection"),
        new("302", "Found", "The resource resides temporarily under a different URL. The client should continue to use the original URL.", "3xx Redirection"),
        new("303", "See Other", "The response can be found under a different URL using a GET method.", "3xx Redirection"),
        new("304", "Not Modified", "The resource has not been modified since the last request. The client can use its cached version.", "3xx Redirection"),
        new("307", "Temporary Redirect", "The resource resides temporarily under a different URL. The request method must not change.", "3xx Redirection"),
        new("308", "Permanent Redirect", "The resource has been permanently moved. The request method must not change.", "3xx Redirection"),

        // 4xx Client Error
        new("400", "Bad Request", "The server cannot process the request due to malformed syntax or invalid parameters.", "4xx Client Error"),
        new("401", "Unauthorized", "Authentication is required and has failed or has not been provided.", "4xx Client Error"),
        new("403", "Forbidden", "The server understood the request but refuses to authorize it.", "4xx Client Error"),
        new("404", "Not Found", "The requested resource could not be found on the server.", "4xx Client Error"),
        new("405", "Method Not Allowed", "The request method is not supported for the requested resource.", "4xx Client Error"),
        new("406", "Not Acceptable", "The resource is not available in a format that would be acceptable according to the Accept headers.", "4xx Client Error"),
        new("408", "Request Timeout", "The server timed out waiting for the request from the client.", "4xx Client Error"),
        new("409", "Conflict", "The request could not be completed due to a conflict with the current state of the target resource.", "4xx Client Error"),
        new("410", "Gone", "The resource is no longer available and no forwarding address is known.", "4xx Client Error"),
        new("411", "Length Required", "The server requires a Content-Length header to be included in the request.", "4xx Client Error"),
        new("412", "Precondition Failed", "One or more conditions given in the request header fields evaluated to false.", "4xx Client Error"),
        new("413", "Content Too Large", "The request payload is larger than the server is willing or able to process.", "4xx Client Error"),
        new("414", "URI Too Long", "The request URI is longer than the server is willing to interpret.", "4xx Client Error"),
        new("415", "Unsupported Media Type", "The media format of the requested data is not supported by the server.", "4xx Client Error"),
        new("416", "Range Not Satisfiable", "The range specified in the Range header cannot be fulfilled.", "4xx Client Error"),
        new("418", "I'm a Teapot", "The server refuses to brew coffee because it is, permanently, a teapot (RFC 2324).", "4xx Client Error"),
        new("422", "Unprocessable Content", "The request was well-formed but the server was unable to process the contained instructions.", "4xx Client Error"),
        new("425", "Too Early", "The server is unwilling to process a request that might be replayed.", "4xx Client Error"),
        new("426", "Upgrade Required", "The server refuses to perform the request using the current protocol and requires an upgrade.", "4xx Client Error"),
        new("428", "Precondition Required", "The origin server requires the request to be conditional to prevent lost updates.", "4xx Client Error"),
        new("429", "Too Many Requests", "The user has sent too many requests in a given amount of time (rate limiting).", "4xx Client Error"),
        new("431", "Request Header Fields Too Large", "The server is unwilling to process the request because its header fields are too large.", "4xx Client Error"),
        new("451", "Unavailable For Legal Reasons", "The resource is unavailable due to legal demands such as censorship or government-mandated blocks.", "4xx Client Error"),

        // 5xx Server Error
        new("500", "Internal Server Error", "The server encountered an unexpected condition that prevented it from fulfilling the request.", "5xx Server Error"),
        new("501", "Not Implemented", "The server does not support the functionality required to fulfill the request.", "5xx Server Error"),
        new("502", "Bad Gateway", "The server received an invalid response from an upstream server while acting as a gateway.", "5xx Server Error"),
        new("503", "Service Unavailable", "The server is temporarily unable to handle the request due to overload or maintenance.", "5xx Server Error"),
        new("504", "Gateway Timeout", "The server did not receive a timely response from an upstream server.", "5xx Server Error"),
        new("505", "HTTP Version Not Supported", "The server does not support the HTTP version used in the request.", "5xx Server Error"),
        new("507", "Insufficient Storage", "The server is unable to store the representation needed to complete the request.", "5xx Server Error"),
        new("508", "Loop Detected", "The server detected an infinite loop while processing the request.", "5xx Server Error"),
        new("510", "Not Extended", "Further extensions to the request are required for the server to fulfill it.", "5xx Server Error"),
        new("511", "Network Authentication Required", "The client needs to authenticate to gain network access (e.g. captive portal).", "5xx Server Error"),
    ];

    public static readonly List<GrpcCode> GrpcCodes =
    [
        new("0", "OK", "The operation completed successfully.", "200 OK"),
        new("1", "Cancelled", "The operation was cancelled, typically by the caller.", "499 Client Closed Request"),
        new("2", "Unknown", "An unknown error occurred. This may be returned when a status value is received from another address space.", "500 Internal Server Error"),
        new("3", "InvalidArgument", "The client specified an invalid argument. Unlike FailedPrecondition, this indicates arguments that are problematic regardless of system state.", "400 Bad Request"),
        new("4", "DeadlineExceeded", "The deadline expired before the operation could complete. Even if the operation has completed successfully, it may have exceeded the deadline.", "504 Gateway Timeout"),
        new("5", "NotFound", "The requested entity was not found.", "404 Not Found"),
        new("6", "AlreadyExists", "The entity that the client attempted to create already exists.", "409 Conflict"),
        new("7", "PermissionDenied", "The caller does not have permission to execute the specified operation. Must not be used for rejections caused by exhausting resources.", "403 Forbidden"),
        new("8", "ResourceExhausted", "Some resource has been exhausted, such as a per-user quota, or the entire file system is out of space.", "429 Too Many Requests"),
        new("9", "FailedPrecondition", "The operation was rejected because the system is not in a state required for the operation. Use InvalidArgument for argument validation.", "400 Bad Request"),
        new("10", "Aborted", "The operation was aborted, typically due to a concurrency issue such as a sequencer check failure or transaction abort.", "409 Conflict"),
        new("11", "OutOfRange", "The operation was attempted past the valid range, e.g. reading past end of file.", "400 Bad Request"),
        new("12", "Unimplemented", "The operation is not implemented or not supported/enabled in this service.", "501 Not Implemented"),
        new("13", "Internal", "An internal error occurred. This means that some invariant expected by the underlying system has been broken.", "500 Internal Server Error"),
        new("14", "Unavailable", "The service is currently unavailable. This is most likely a transient condition that can be corrected by retrying.", "503 Service Unavailable"),
        new("15", "DataLoss", "Unrecoverable data loss or corruption.", "500 Internal Server Error"),
        new("16", "Unauthenticated", "The request does not have valid authentication credentials for the operation.", "401 Unauthorized"),
    ];

    public static readonly List<ErrorCode> Win32Codes =
    [
        new("0", "ERROR_SUCCESS", "The operation completed successfully.", "Win32"),
        new("1", "ERROR_INVALID_FUNCTION", "Incorrect function. The function is not recognized or is not valid in the current context.", "Win32"),
        new("2", "ERROR_FILE_NOT_FOUND", "The system cannot find the file specified.", "Win32"),
        new("3", "ERROR_PATH_NOT_FOUND", "The system cannot find the path specified.", "Win32"),
        new("5", "ERROR_ACCESS_DENIED", "Access is denied. The process does not have the required permissions.", "Win32"),
        new("6", "ERROR_INVALID_HANDLE", "The handle is invalid or has been closed.", "Win32"),
        new("8", "ERROR_NOT_ENOUGH_MEMORY", "Not enough memory resources are available to process this command.", "Win32"),
        new("13", "ERROR_INVALID_DATA", "The data is invalid or corrupted.", "Win32"),
        new("14", "ERROR_OUTOFMEMORY", "Not enough storage is available to complete this operation.", "Win32"),
        new("15", "ERROR_INVALID_DRIVE", "The system cannot find the drive specified.", "Win32"),
        new("18", "ERROR_NO_MORE_FILES", "There are no more files to enumerate.", "Win32"),
        new("19", "ERROR_WRITE_PROTECT", "The media is write protected.", "Win32"),
        new("24", "ERROR_BAD_LENGTH", "The program issued a command but the command length is incorrect.", "Win32"),
        new("32", "ERROR_SHARING_VIOLATION", "The process cannot access the file because it is being used by another process.", "Win32"),
        new("33", "ERROR_LOCK_VIOLATION", "The process cannot access the file because another process has locked a portion of the file.", "Win32"),
        new("38", "ERROR_HANDLE_EOF", "Reached the end of the file.", "Win32"),
        new("80", "ERROR_FILE_EXISTS", "The file already exists.", "Win32"),
        new("87", "ERROR_INVALID_PARAMETER", "The parameter is incorrect.", "Win32"),
        new("109", "ERROR_BROKEN_PIPE", "The pipe has been ended. The connection to the pipe server has been lost.", "Win32"),
        new("110", "ERROR_OPEN_FAILED", "The system cannot open the device or file specified.", "Win32"),
        new("111", "ERROR_BUFFER_OVERFLOW", "The file name is too long. The buffer provided is too small.", "Win32"),
        new("112", "ERROR_DISK_FULL", "There is not enough space on the disk.", "Win32"),
        new("122", "ERROR_INSUFFICIENT_BUFFER", "The data area passed to a system call is too small.", "Win32"),
        new("123", "ERROR_INVALID_NAME", "The filename, directory name, or volume label syntax is incorrect.", "Win32"),
        new("126", "ERROR_MOD_NOT_FOUND", "The specified module could not be found.", "Win32"),
        new("127", "ERROR_PROC_NOT_FOUND", "The specified procedure could not be found in the module.", "Win32"),
        new("183", "ERROR_ALREADY_EXISTS", "Cannot create a file or directory when it already exists.", "Win32"),
        new("193", "ERROR_BAD_EXE_FORMAT", "The file is not a valid Win32 application or has an invalid executable format.", "Win32"),
        new("203", "ERROR_ENVVAR_NOT_FOUND", "The system could not find the environment variable specified.", "Win32"),
        new("1223", "ERROR_CANCELLED", "The operation was cancelled by the user.", "Win32"),
    ];

    public static readonly List<ErrorCode> HresultCodes =
    [
        new("0x00000000", "S_OK", "Operation successful. No error occurred.", "HRESULT"),
        new("0x00000001", "S_FALSE", "Operation successful but returned a boolean false result or no data.", "HRESULT"),
        new("0x80004001", "E_NOTIMPL", "Not implemented. The method or operation is not supported.", "HRESULT"),
        new("0x80004002", "E_NOINTERFACE", "No such interface supported. The queried COM interface is not available.", "HRESULT"),
        new("0x80004003", "E_POINTER", "Invalid pointer. A null or invalid pointer was passed.", "HRESULT"),
        new("0x80004004", "E_ABORT", "Operation aborted. The operation was terminated before completion.", "HRESULT"),
        new("0x80004005", "E_FAIL", "Unspecified failure. A general error occurred with no more specific code.", "HRESULT"),
        new("0x8000FFFF", "E_UNEXPECTED", "Unexpected failure. The operation encountered an unexpected condition.", "HRESULT"),
        new("0x80070005", "E_ACCESSDENIED", "General access denied error. The caller does not have the required permission.", "HRESULT"),
        new("0x80070006", "E_HANDLE", "Invalid handle. The handle provided is not valid or has been closed.", "HRESULT"),
        new("0x8007000E", "E_OUTOFMEMORY", "Out of memory. There is insufficient memory to complete the operation.", "HRESULT"),
        new("0x80070057", "E_INVALIDARG", "One or more arguments are not valid.", "HRESULT"),
        new("0x8000000A", "E_PENDING", "The data necessary to complete the operation is not yet available.", "HRESULT"),
        new("0x800401F0", "CO_E_NOTINITIALIZED", "CoInitialize has not been called. COM must be initialized before use.", "HRESULT"),
        new("0x80010106", "RPC_E_CHANGED_MODE", "Cannot change thread mode after it is set. CoInitializeEx was called with a different concurrency model.", "HRESULT"),
        new("0x80010119", "RPC_E_TOO_LATE", "It is too late to perform the requested operation. The RPC connection has progressed past the required state.", "HRESULT"),
        new("0x80040110", "CLASS_E_NOAGGREGATION", "Class does not support aggregation or the controlling IUnknown was non-null.", "HRESULT"),
        new("0x80040154", "REGDB_E_CLASSNOTREG", "Class not registered. The CLSID is not found in the registry.", "HRESULT"),
        new("0x80028CA0", "TYPE_E_TYPEMISMATCH", "Type mismatch. The value could not be converted to the expected type.", "HRESULT"),
        new("0x80131500", "COR_E_EXCEPTION", "A .NET CLR exception occurred. The base HRESULT for managed exceptions.", "HRESULT"),
    ];
}
