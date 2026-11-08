using Microsoft.Data.Sqlite;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;
using Watcher.Infrastructure.Database;

namespace Watcher.Infrastructure.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly DatabaseContext _context;

    public FolderRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<WatchedFolder>> GetAllAsync()
    {
        var list = new List<WatchedFolder>();
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Path, UnknownTarget, Quiet, Enabled, Recursive, FilesOrganizedCount, Status, CreatedAt, UpdatedAt FROM WatchedFolders ORDER BY CreatedAt ASC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(ReadFolder(reader));
        }

        return list;
    }

    public async Task<WatchedFolder?> GetByIdAsync(string id)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Path, UnknownTarget, Quiet, Enabled, Recursive, FilesOrganizedCount, Status, CreatedAt, UpdatedAt FROM WatchedFolders WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadFolder(reader);
        }

        return null;
    }

    public async Task<WatchedFolder?> GetByPathAsync(string path)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Path, UnknownTarget, Quiet, Enabled, Recursive, FilesOrganizedCount, Status, CreatedAt, UpdatedAt FROM WatchedFolders WHERE LOWER(Path) = LOWER(@Path);";
        cmd.Parameters.AddWithValue("@Path", path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return ReadFolder(reader);
        }

        return null;
    }

    public async Task<WatchedFolder> AddAsync(WatchedFolder folder)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO WatchedFolders (Id, Path, UnknownTarget, Quiet, Enabled, Recursive, FilesOrganizedCount, Status, CreatedAt, UpdatedAt)
            VALUES (@Id, @Path, @UnknownTarget, @Quiet, @Enabled, @Recursive, @FilesOrganizedCount, @Status, @CreatedAt, @UpdatedAt);
        ";
        cmd.Parameters.AddWithValue("@Id", folder.Id);
        cmd.Parameters.AddWithValue("@Path", folder.Path);
        cmd.Parameters.AddWithValue("@UnknownTarget", folder.UnknownTarget);
        cmd.Parameters.AddWithValue("@Quiet", folder.Quiet ? 1 : 0);
        cmd.Parameters.AddWithValue("@Enabled", folder.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@Recursive", folder.Recursive ? 1 : 0);
        cmd.Parameters.AddWithValue("@FilesOrganizedCount", folder.FilesOrganizedCount);
        cmd.Parameters.AddWithValue("@Status", folder.Status.ToString());
        cmd.Parameters.AddWithValue("@CreatedAt", folder.CreatedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@UpdatedAt", folder.UpdatedAt.ToString("o"));

        await cmd.ExecuteNonQueryAsync();
        return folder;
    }

    public async Task<WatchedFolder> UpdateAsync(WatchedFolder folder)
    {
        folder.UpdatedAt = DateTime.UtcNow;
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE WatchedFolders
            SET Path = @Path, UnknownTarget = @UnknownTarget, Quiet = @Quiet, Enabled = @Enabled,
                Recursive = @Recursive, FilesOrganizedCount = @FilesOrganizedCount, Status = @Status, UpdatedAt = @UpdatedAt
            WHERE Id = @Id;
        ";
        cmd.Parameters.AddWithValue("@Id", folder.Id);
        cmd.Parameters.AddWithValue("@Path", folder.Path);
        cmd.Parameters.AddWithValue("@UnknownTarget", folder.UnknownTarget);
        cmd.Parameters.AddWithValue("@Quiet", folder.Quiet ? 1 : 0);
        cmd.Parameters.AddWithValue("@Enabled", folder.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@Recursive", folder.Recursive ? 1 : 0);
        cmd.Parameters.AddWithValue("@FilesOrganizedCount", folder.FilesOrganizedCount);
        cmd.Parameters.AddWithValue("@Status", folder.Status.ToString());
        cmd.Parameters.AddWithValue("@UpdatedAt", folder.UpdatedAt.ToString("o"));

        await cmd.ExecuteNonQueryAsync();
        return folder;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM WatchedFolders WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task IncrementOrganizedCountAsync(string folderId)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE WatchedFolders SET FilesOrganizedCount = FilesOrganizedCount + 1, UpdatedAt = @UpdatedAt WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", folderId);
        cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("o"));

        await cmd.ExecuteNonQueryAsync();
    }

    private static WatchedFolder ReadFolder(SqliteDataReader reader)
    {
        return new WatchedFolder
        {
            Id = reader.GetString(0),
            Path = reader.GetString(1),
            UnknownTarget = reader.GetString(2),
            Quiet = reader.GetInt32(3) == 1,
            Enabled = reader.GetInt32(4) == 1,
            Recursive = reader.GetInt32(5) == 1,
            FilesOrganizedCount = reader.GetInt32(6),
            Status = Enum.TryParse<FolderStatus>(reader.GetString(7), out var st) ? st : FolderStatus.Active,
            CreatedAt = DateTime.Parse(reader.GetString(8)),
            UpdatedAt = DateTime.Parse(reader.GetString(9))
        };
    }
}

