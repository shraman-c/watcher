using Microsoft.Data.Sqlite;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;
using Watcher.Infrastructure.Database;

namespace Watcher.Infrastructure.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly DatabaseContext _context;

    public ActivityRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<ActivityLogItem> AddAsync(ActivityLogItem item)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ActivityLogs (Id, FileName, Extension, Category, SourcePath, DestinationPath, Timestamp, Status, ErrorMessage, FileSizeBytes, CanUndo)
            VALUES (@Id, @FileName, @Extension, @Category, @SourcePath, @DestinationPath, @Timestamp, @Status, @ErrorMessage, @FileSizeBytes, @CanUndo);
        ";
        cmd.Parameters.AddWithValue("@Id", item.Id);
        cmd.Parameters.AddWithValue("@FileName", item.FileName);
        cmd.Parameters.AddWithValue("@Extension", item.Extension);
        cmd.Parameters.AddWithValue("@Category", item.Category);
        cmd.Parameters.AddWithValue("@SourcePath", item.SourcePath);
        cmd.Parameters.AddWithValue("@DestinationPath", item.DestinationPath);
        cmd.Parameters.AddWithValue("@Timestamp", item.Timestamp.ToString("o"));
        cmd.Parameters.AddWithValue("@Status", item.Status.ToString());
        cmd.Parameters.AddWithValue("@ErrorMessage", (object?)item.ErrorMessage ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FileSizeBytes", (object?)item.FileSizeBytes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CanUndo", item.CanUndo ? 1 : 0);

        await cmd.ExecuteNonQueryAsync();
        return item;
    }

    public async Task<List<ActivityLogItem>> GetRecentAsync(int limit = 50)
    {
        var list = new List<ActivityLogItem>();
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FileName, Extension, Category, SourcePath, DestinationPath, Timestamp, Status, ErrorMessage, FileSizeBytes, CanUndo FROM ActivityLogs ORDER BY Timestamp DESC LIMIT @Limit;";
        cmd.Parameters.AddWithValue("@Limit", limit);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadActivity(reader));
        }

        return list;
    }

    public async Task<List<ActivityLogItem>> QueryAsync(
        string? search = null,
        string? category = null,
        OperationStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20)
    {
        var list = new List<ActivityLogItem>();
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();

        var (whereSql, parameters) = BuildFilter(search, category, status, fromDate, toDate);
        int offset = Math.Max(0, (page - 1) * pageSize);

        cmd.CommandText = $@"
            SELECT Id, FileName, Extension, Category, SourcePath, DestinationPath, Timestamp, Status, ErrorMessage, FileSizeBytes, CanUndo
            FROM ActivityLogs
            {whereSql}
            ORDER BY Timestamp DESC
            LIMIT @Limit OFFSET @Offset;
        ";
        foreach (var p in parameters) cmd.Parameters.Add(p);
        cmd.Parameters.AddWithValue("@Limit", pageSize);
        cmd.Parameters.AddWithValue("@Offset", offset);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadActivity(reader));
        }

        return list;
    }

    public async Task<int> GetTotalCountAsync(
        string? search = null,
        string? category = null,
        OperationStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();

        var (whereSql, parameters) = BuildFilter(search, category, status, fromDate, toDate);
        cmd.CommandText = $"SELECT COUNT(*) FROM ActivityLogs {whereSql};";
        foreach (var p in parameters) cmd.Parameters.Add(p);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<ActivityLogItem?> GetByIdAsync(string id)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FileName, Extension, Category, SourcePath, DestinationPath, Timestamp, Status, ErrorMessage, FileSizeBytes, CanUndo FROM ActivityLogs WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadActivity(reader);
        }

        return null;
    }

    public async Task<bool> MarkUndoneAsync(string id)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE ActivityLogs SET CanUndo = 0, Status = 'SKIPPED', ErrorMessage = 'Operation undone by user' WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task ClearAllAsync()
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ActivityLogs;";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<DashboardStatistics> GetDashboardStatisticsAsync()
    {
        var stats = new DashboardStatistics();
        using var conn = _context.CreateConnection();

        // 1. Total counts
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT
                    COUNT(*),
                    TOTAL(CASE WHEN Status = 'SUCCESS' THEN 1 ELSE 0 END),
                    TOTAL(CASE WHEN Status = 'FAILED' THEN 1 ELSE 0 END),
                    TOTAL(CASE WHEN date(Timestamp) = date('now') AND Status = 'SUCCESS' THEN 1 ELSE 0 END)
                FROM ActivityLogs;
            ";
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                stats.TotalFilesOrganized = Convert.ToInt32(reader.GetDouble(1));
                stats.SuccessfulOperations = Convert.ToInt32(reader.GetDouble(1));
                stats.FailedOperations = Convert.ToInt32(reader.GetDouble(2));
                stats.FilesOrganizedToday = Convert.ToInt32(reader.GetDouble(3));
            }
        }

        // 2. Folder counts
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*), TOTAL(CASE WHEN Enabled = 1 THEN 1 ELSE 0 END) FROM WatchedFolders;";
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                stats.TotalFoldersCount = reader.GetInt32(0);
                stats.ActiveFoldersCount = Convert.ToInt32(reader.GetDouble(1));
            }
        }

        // 3. Categories count
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM Categories;";
            var res = await cmd.ExecuteScalarAsync();
            stats.CategoriesCount = Convert.ToInt32(res);
        }

        // 4. Activity over past 7 days
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT
                    date(Timestamp) as LogDate,
                    COUNT(*) as Total,
                    TOTAL(CASE WHEN Status = 'SUCCESS' THEN 1 ELSE 0 END) as Success,
                    TOTAL(CASE WHEN Status = 'FAILED' THEN 1 ELSE 0 END) as Failed
                FROM ActivityLogs
                WHERE date(Timestamp) >= date('now', '-7 days')
                GROUP BY date(Timestamp)
                ORDER BY LogDate ASC;
            ";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                stats.ActivityOverTime.Add(new ActivityOverTimeItem
                {
                    Date = reader.GetString(0),
                    Total = reader.GetInt32(1),
                    Success = Convert.ToInt32(reader.GetDouble(2)),
                    Failed = Convert.ToInt32(reader.GetDouble(3))
                });
            }
        }

        // 5. Category breakdown
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                SELECT Category, COUNT(*) as CatCount
                FROM ActivityLogs
                WHERE Status = 'SUCCESS' AND Category != ''
                GROUP BY Category
                ORDER BY CatCount DESC
                LIMIT 8;
            ";
            using var reader = await cmd.ExecuteReaderAsync();
            int totalSuccess = stats.SuccessfulOperations > 0 ? stats.SuccessfulOperations : 1;
            while (await reader.ReadAsync())
            {
                int count = reader.GetInt32(1);
                stats.CategoryBreakdown.Add(new CategoryStatItem
                {
                    Category = reader.GetString(0),
                    Count = count,
                    Percentage = Math.Round((double)count / totalSuccess * 100, 1)
                });
            }
        }

        // 6. Folders breakdown
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT Id, Path, FilesOrganizedCount FROM WatchedFolders ORDER BY FilesOrganizedCount DESC LIMIT 5;";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                stats.FolderBreakdown.Add(new FolderStatItem
                {
                    FolderId = reader.GetString(0),
                    Path = reader.GetString(1),
                    Count = reader.GetInt32(2)
                });
            }
        }

        return stats;
    }

    private static (string WhereSql, List<SqliteParameter> Parameters) BuildFilter(
        string? search,
        string? category,
        OperationStatus? status,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var clauses = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            clauses.Add("(FileName LIKE @Search OR SourcePath LIKE @Search OR DestinationPath LIKE @Search)");
            parameters.Add(new SqliteParameter("@Search", $"%{search.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
        {
            clauses.Add("Category = @Category");
            parameters.Add(new SqliteParameter("@Category", category.Trim()));
        }

        if (status.HasValue)
        {
            clauses.Add("Status = @Status");
            parameters.Add(new SqliteParameter("@Status", status.Value.ToString()));
        }

        if (fromDate.HasValue)
        {
            clauses.Add("Timestamp >= @FromDate");
            parameters.Add(new SqliteParameter("@FromDate", fromDate.Value.ToString("o")));
        }

        if (toDate.HasValue)
        {
            clauses.Add("Timestamp <= @ToDate");
            parameters.Add(new SqliteParameter("@ToDate", toDate.Value.ToString("o")));
        }

        var whereSql = clauses.Count > 0 ? "WHERE " + string.Join(" AND ", clauses) : string.Empty;
        return (whereSql, parameters);
    }

    private static ActivityLogItem ReadActivity(SqliteDataReader reader)
    {
        return new ActivityLogItem
        {
            Id = reader.GetString(0),
            FileName = reader.GetString(1),
            Extension = reader.GetString(2),
            Category = reader.GetString(3),
            SourcePath = reader.GetString(4),
            DestinationPath = reader.GetString(5),
            Timestamp = DateTime.Parse(reader.GetString(6)),
            Status = Enum.TryParse<OperationStatus>(reader.GetString(7), out var st) ? st : OperationStatus.SUCCESS,
            ErrorMessage = reader.IsDBNull(8) ? null : reader.GetString(8),
            FileSizeBytes = reader.IsDBNull(9) ? null : reader.GetInt64(9),
            CanUndo = reader.GetInt32(10) == 1
        };
    }
}

