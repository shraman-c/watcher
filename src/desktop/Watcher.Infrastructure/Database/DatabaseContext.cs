using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Watcher.Infrastructure.Database;

public class DatabaseContext
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseContext>? _logger;
    private static readonly ConcurrentDictionary<string, bool> _initializedDatabases = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _initLock = new();

    public DatabaseContext(string? dbPath = null, ILogger<DatabaseContext>? logger = null)
    {
        _logger = logger;
        if (string.IsNullOrEmpty(dbPath))
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var folder = Path.Combine(appData, "Watcher");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            dbPath = Path.Combine(folder, "watcher.db");
        }

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        };
        _connectionString = builder.ToString();

        EnsureInitialized();
    }

    public SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode = WAL; PRAGMA foreign_keys = ON; PRAGMA synchronous = NORMAL;";
        pragma.ExecuteNonQuery();

        return conn;
    }

    private void EnsureInitialized()
    {
        if (_initializedDatabases.ContainsKey(_connectionString))
            return;

        lock (_initLock)
        {
            if (_initializedDatabases.ContainsKey(_connectionString))
                return;

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS WatchedFolders (
                    Id TEXT PRIMARY KEY,
                    Path TEXT NOT NULL UNIQUE,
                    UnknownTarget TEXT NOT NULL DEFAULT 'Other',
                    Quiet INTEGER NOT NULL DEFAULT 0,
                    Enabled INTEGER NOT NULL DEFAULT 1,
                    Recursive INTEGER NOT NULL DEFAULT 0,
                    FilesOrganizedCount INTEGER NOT NULL DEFAULT 0,
                    Status TEXT NOT NULL DEFAULT 'Active',
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Categories (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL UNIQUE,
                    DefaultFolder TEXT NOT NULL,
                    ExtensionsJson TEXT NOT NULL,
                    Color TEXT NOT NULL,
                    IconName TEXT NOT NULL,
                    IsSystem INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS OrganizationRules (
                    Id TEXT PRIMARY KEY,
                    CategoryId TEXT NOT NULL,
                    CategoryName TEXT NOT NULL,
                    ExtensionsJson TEXT NOT NULL,
                    TargetFolder TEXT NOT NULL,
                    Priority INTEGER NOT NULL DEFAULT 0,
                    Enabled INTEGER NOT NULL DEFAULT 1,
                    IsCustom INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS ActivityLogs (
                    Id TEXT PRIMARY KEY,
                    FileName TEXT NOT NULL,
                    Extension TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    SourcePath TEXT NOT NULL,
                    DestinationPath TEXT NOT NULL,
                    Timestamp TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    ErrorMessage TEXT,
                    FileSizeBytes INTEGER,
                    CanUndo INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS ApplicationSettings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS IX_ActivityLogs_Timestamp ON ActivityLogs(Timestamp DESC);
                CREATE INDEX IF NOT EXISTS IX_ActivityLogs_Category ON ActivityLogs(Category);
                CREATE INDEX IF NOT EXISTS IX_ActivityLogs_Status ON ActivityLogs(Status);
                CREATE INDEX IF NOT EXISTS IX_OrganizationRules_Priority ON OrganizationRules(Priority);
            ";
            cmd.ExecuteNonQuery();
            _initializedDatabases[_connectionString] = true;
            _logger?.LogInformation("Database initialized successfully for '{Conn}'.", _connectionString);
        }
    }
}
