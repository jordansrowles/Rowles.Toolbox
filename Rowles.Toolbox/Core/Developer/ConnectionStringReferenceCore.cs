namespace Rowles.Toolbox.Core.Developer;

public static class ConnectionStringReferenceCore
{
    public sealed record DbType(string Name, string Icon, int DefaultPort, string Template, string Example, List<DbParam> Parameters);
    public sealed record DbParam(string Name, string Description, string DefaultValue, bool Required);

    public static readonly List<DbType> DbTypes =
    [
        new DbType(
            "SQL Server", "database", 1433,
            "Server={Server};Database={Database};User Id={User Id};Password={Password};Encrypt={Encrypt};TrustServerCertificate={TrustServerCertificate};Connection Timeout={Connection Timeout}",
            "Server=localhost,1433;Database=MyAppDb;User Id=sa;Password=YourStrong!Pass;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30",
            new List<DbParam>
            {
                new("Server", "Hostname or IP, optionally with port (host,port)", "localhost", true),
                new("Database", "Name of the database to connect to", "", true),
                new("User Id", "SQL Server login username", "sa", true),
                new("Password", "SQL Server login password", "", true),
                new("Encrypt", "Whether to encrypt the connection (True/False/Strict)", "True", false),
                new("TrustServerCertificate", "Trust the server certificate without validation", "False", false),
                new("Connection Timeout", "Seconds to wait before timing out a connection attempt", "30", false),
                new("MultipleActiveResultSets", "Enable MARS for multiple concurrent queries", "False", false),
                new("Application Name", "Application name sent to SQL Server for tracking", "", false),
                new("Integrated Security", "Use Windows authentication instead of SQL auth (True/SSPI)", "False", false),
            }),
        new DbType(
            "PostgreSQL", "elephant", 5432,
            "Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};SSL Mode={SSL Mode};Timeout={Timeout}",
            "Host=localhost;Port=5432;Database=myapp;Username=postgres;Password=secret;SSL Mode=Prefer;Timeout=30",
            new List<DbParam>
            {
                new("Host", "Hostname or IP address of the PostgreSQL server", "localhost", true),
                new("Port", "TCP port number", "5432", true),
                new("Database", "Name of the database", "", true),
                new("Username", "PostgreSQL user", "postgres", true),
                new("Password", "PostgreSQL password", "", true),
                new("SSL Mode", "SSL connection mode (Disable/Prefer/Require/VerifyCA/VerifyFull)", "Prefer", false),
                new("Timeout", "Seconds before connection timeout", "30", false),
                new("Command Timeout", "Default timeout for commands in seconds", "30", false),
                new("Pooling", "Enable connection pooling", "True", false),
                new("Maximum Pool Size", "Maximum connections in the pool", "100", false),
                new("Search Path", "Schema search path", "public", false),
            }),
        new DbType(
            "MySQL", "brand-mysql", 3306,
            "Server={Server};Port={Port};Database={Database};User={User};Password={Password};SslMode={SslMode};Connection Timeout={Connection Timeout}",
            "Server=localhost;Port=3306;Database=myapp;User=root;Password=secret;SslMode=Preferred;Connection Timeout=30",
            new List<DbParam>
            {
                new("Server", "Hostname or IP address of the MySQL server", "localhost", true),
                new("Port", "TCP port number", "3306", true),
                new("Database", "Name of the database", "", true),
                new("User", "MySQL username", "root", true),
                new("Password", "MySQL password", "", true),
                new("SslMode", "SSL connection mode (None/Preferred/Required/VerifyCA/VerifyFull)", "Preferred", false),
                new("Connection Timeout", "Seconds before connection timeout", "30", false),
                new("CharSet", "Character set for the connection", "utf8mb4", false),
                new("AllowPublicKeyRetrieval", "Allow retrieval of RSA public key from server", "False", false),
                new("Convert Zero Datetime", "Convert zero datetime values instead of throwing", "True", false),
            }),
        new DbType(
            "SQLite", "file-database", 0,
            "Data Source={Data Source};Mode={Mode};Cache={Cache};Password={Password}",
            "Data Source=app.db;Mode=ReadWriteCreate;Cache=Shared",
            new List<DbParam>
            {
                new("Data Source", "Path to the SQLite database file", "app.db", true),
                new("Mode", "File open mode (ReadWriteCreate/ReadWrite/ReadOnly/Memory)", "ReadWriteCreate", false),
                new("Cache", "Cache sharing mode (Default/Private/Shared)", "Default", false),
                new("Password", "Encryption password (requires SQLCipher or similar)", "", false),
                new("Foreign Keys", "Enforce foreign key constraints", "True", false),
                new("Pooling", "Enable connection pooling", "True", false),
                new("Default Timeout", "Default command timeout in seconds", "30", false),
            }),
        new DbType(
            "Oracle", "database", 1521,
            "Data Source={Host}:{Port}/{Service Name};User Id={User Id};Password={Password};Connection Timeout={Connection Timeout}",
            "Data Source=localhost:1521/ORCL;User Id=system;Password=oracle;Connection Timeout=30",
            new List<DbParam>
            {
                new("Host", "Hostname or IP address of the Oracle server", "localhost", true),
                new("Port", "TCP port number", "1521", true),
                new("Service Name", "Oracle service name or SID", "ORCL", true),
                new("User Id", "Oracle username", "system", true),
                new("Password", "Oracle password", "", true),
                new("Connection Timeout", "Seconds before connection timeout", "30", false),
                new("Min Pool Size", "Minimum number of connections in the pool", "1", false),
                new("Max Pool Size", "Maximum number of connections in the pool", "100", false),
                new("Pooling", "Enable connection pooling", "True", false),
                new("Statement Cache Size", "Number of statements to cache", "0", false),
            }),
        new DbType(
            "MongoDB", "leaf", 27017,
            "mongodb://{Username}:{Password}@{Host}:{Port}/{Database}?authSource={AuthSource}&retryWrites={RetryWrites}&w={WriteConcern}",
            "mongodb://admin:secret@localhost:27017/myapp?authSource=admin&retryWrites=true&w=majority",
            new List<DbParam>
            {
                new("Host", "Hostname or IP address of the MongoDB server", "localhost", true),
                new("Port", "TCP port number", "27017", true),
                new("Database", "Default database name", "", true),
                new("Username", "MongoDB username", "admin", false),
                new("Password", "MongoDB password", "", false),
                new("AuthSource", "Authentication database", "admin", false),
                new("RetryWrites", "Enable retryable writes", "true", false),
                new("WriteConcern", "Write concern level (1/majority)", "majority", false),
                new("ReplicaSet", "Name of the replica set", "", false),
                new("TLS", "Enable TLS/SSL (true/false)", "false", false),
            }),
        new DbType(
            "Redis", "flame", 6379,
            "{Host}:{Port},password={Password},ssl={SSL},abortConnect={AbortConnect},defaultDatabase={DefaultDatabase},connectTimeout={ConnectTimeout}",
            "localhost:6379,password=secret,ssl=False,abortConnect=False,defaultDatabase=0,connectTimeout=5000",
            new List<DbParam>
            {
                new("Host", "Hostname or IP address of the Redis server", "localhost", true),
                new("Port", "TCP port number", "6379", true),
                new("Password", "Redis authentication password", "", false),
                new("SSL", "Enable SSL/TLS connection", "False", false),
                new("AbortConnect", "Abort if connection cannot be established immediately", "False", false),
                new("DefaultDatabase", "Default database index (0-15)", "0", false),
                new("ConnectTimeout", "Connection timeout in milliseconds", "5000", false),
                new("SyncTimeout", "Synchronous operation timeout in milliseconds", "5000", false),
                new("AllowAdmin", "Enable administrative commands", "False", false),
            }),
        new DbType(
            "Azure SQL", "cloud-computing", 1433,
            "Server=tcp:{Server}.database.windows.net,1433;Database={Database};User ID={User ID};Password={Password};Encrypt=True;TrustServerCertificate=False;Connection Timeout={Connection Timeout}",
            "Server=tcp:myserver.database.windows.net,1433;Database=MyAppDb;User ID=sqladmin;Password=YourStrong!Pass;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30",
            new List<DbParam>
            {
                new("Server", "Azure SQL server name (without .database.windows.net)", "myserver", true),
                new("Database", "Name of the Azure SQL database", "", true),
                new("User ID", "Azure SQL login username", "", true),
                new("Password", "Azure SQL login password", "", true),
                new("Connection Timeout", "Seconds before connection timeout", "30", false),
                new("Encrypt", "Encryption is mandatory for Azure SQL", "True", false),
                new("TrustServerCertificate", "Must be False for Azure SQL", "False", false),
                new("MultipleActiveResultSets", "Enable MARS", "False", false),
                new("Authentication", "Auth type (SQL Password/Active Directory Password/Active Directory Integrated/Active Directory Interactive)", "SQL Password", false),
            }),
        new DbType(
            "CosmosDB", "universe", 443,
            "AccountEndpoint={AccountEndpoint};AccountKey={AccountKey};Database={Database}",
            "AccountEndpoint=https://myaccount.documents.azure.com:443/;AccountKey=base64key==;Database=MyDb",
            new List<DbParam>
            {
                new("AccountEndpoint", "CosmosDB account URI (https://{name}.documents.azure.com:443/)", "", true),
                new("AccountKey", "Primary or secondary access key (base64)", "", true),
                new("Database", "Default database name", "", true),
                new("MaxRetryAttemptsOnRateLimitedRequests", "Max retries on 429 responses", "9", false),
                new("MaxRetryWaitTimeOnRateLimitedRequests", "Max wait in seconds for retries", "30", false),
                new("ConnectionMode", "Connection mode (Direct/Gateway)", "Direct", false),
                new("ApplicationRegion", "Preferred Azure region for the client", "", false),
            }),
        new DbType(
            "RabbitMQ", "arrow-right-circle", 5672,
            "amqp://{Username}:{Password}@{Host}:{Port}/{VirtualHost}",
            "amqp://guest:guest@localhost:5672/%2F",
            new List<DbParam>
            {
                new("Host", "Hostname or IP address of the RabbitMQ server", "localhost", true),
                new("Port", "AMQP port number", "5672", true),
                new("Username", "RabbitMQ username", "guest", true),
                new("Password", "RabbitMQ password", "guest", true),
                new("VirtualHost", "Virtual host (use %2F for default /)", "%2F", false),
                new("RequestedHeartbeat", "Heartbeat interval in seconds", "60", false),
                new("PrefetchCount", "Number of messages to prefetch", "0", false),
                new("SSL Enabled", "Enable SSL/TLS connection", "False", false),
            }),
        new DbType(
            "Elasticsearch", "search", 9200,
            "{Scheme}://{Username}:{Password}@{Host}:{Port}",
            "https://elastic:changeme@localhost:9200",
            new List<DbParam>
            {
                new("Scheme", "Protocol scheme (http/https)", "https", true),
                new("Host", "Hostname or IP address of the Elasticsearch node", "localhost", true),
                new("Port", "HTTP port number", "9200", true),
                new("Username", "Elasticsearch username", "elastic", false),
                new("Password", "Elasticsearch password", "", false),
                new("CertificateFingerprint", "SHA-256 fingerprint for certificate pinning", "", false),
                new("DefaultIndex", "Default index for operations", "", false),
                new("RequestTimeout", "Request timeout in seconds", "60", false),
            }),
    ];

    public static string BuildConnectionString(List<DbType> dbTypes, string dbName, Dictionary<string, string> values)
    {
        DbType? db = dbTypes.Find(d => d.Name == dbName);
        if (db is null)
            return string.Empty;

        string result = db.Template;
        foreach (DbParam param in db.Parameters)
        {
            string value = values.TryGetValue(param.Name, out string? v) ? v : string.Empty;
            if (string.IsNullOrEmpty(value))
                value = param.DefaultValue;
            result = result.Replace($"{{{param.Name}}}", value);
        }

        return result;
    }

    public static string GetAppSettingsSnippet(string connectionString)
    {
        return $$"""
{
  "ConnectionStrings": {
    "DefaultConnection": "{{connectionString}}"
  }
}
""";
    }

    public static string GetCSharpSnippet(string connectionString, string? dbName)
    {
        string provider = dbName switch
        {
            "SQL Server" or "Azure SQL" => "UseSqlServer",
            "PostgreSQL" => "UseNpgsql",
            "MySQL" => "UseMySql",
            "SQLite" => "UseSqlite",
            "Oracle" => "UseOracle",
            "CosmosDB" => "UseCosmos",
            _ => "// Configure your provider"
        };

        if (dbName is "MongoDB" or "Redis" or "RabbitMQ" or "Elasticsearch")
        {
            return dbName switch
            {
                "MongoDB" => $"""
string connectionString = "{connectionString}";
MongoClient client = new MongoClient(connectionString);
IMongoDatabase database = client.GetDatabase("mydb");
""",
                "Redis" => $"""
string connectionString = "{connectionString}";
ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
IDatabase db = redis.GetDatabase();
""",
                "RabbitMQ" => $$"""
ConnectionFactory factory = new ConnectionFactory
{
    Uri = new Uri("{{connectionString}}")
};
using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();
""",
                "Elasticsearch" => $"""
string connectionString = "{connectionString}";
ElasticsearchClientSettings settings = new(new Uri(connectionString));
ElasticsearchClient client = new(settings);
""",
                _ => string.Empty
            };
        }

        return $"""
builder.Services.AddDbContext<AppDbContext>(options =>
    options.{provider}(
        builder.Configuration.GetConnectionString("DefaultConnection")));
""";
    }
}
