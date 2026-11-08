using Watcher.Core.Models;
using Watcher.Infrastructure.Configuration;
using Watcher.Infrastructure.Database;
using Watcher.Infrastructure.Repositories;
using Xunit;

namespace Watcher.Infrastructure.Tests;

public class DatabaseAndMigrationTests : IDisposable
{
    private readonly string _tempDbPath;
    private readonly DatabaseContext _context;

    public DatabaseAndMigrationTests()
    {
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"watcher_test_{Guid.NewGuid():N}.db");
        _context = new DatabaseContext(_tempDbPath);
    }

    [Fact]
    public async Task FolderRepository_CrudOperations_WorkCorrectly()
    {
        var repo = new FolderRepository(_context);
        var folder = new WatchedFolder
        {
            Path = @"C:\TestWatch",
            UnknownTarget = "Uncategorized",
            Quiet = true,
            Enabled = true,
            Recursive = false
        };

        // Add
        var added = await repo.AddAsync(folder);
        Assert.NotNull(added);

        // Get
        var fetched = await repo.GetByIdAsync(added.Id);
        Assert.NotNull(fetched);
        Assert.Equal(@"C:\TestWatch", fetched.Path);
        Assert.True(fetched.Quiet);

        // Update
        fetched.Quiet = false;
        await repo.UpdateAsync(fetched);
        var updated = await repo.GetByIdAsync(added.Id);
        Assert.False(updated!.Quiet);

        // Increment count
        await repo.IncrementOrganizedCountAsync(added.Id);
        var incremented = await repo.GetByIdAsync(added.Id);
        Assert.Equal(1, incremented!.FilesOrganizedCount);

        // Delete
        bool deleted = await repo.DeleteAsync(added.Id);
        Assert.True(deleted);
        Assert.Null(await repo.GetByIdAsync(added.Id));
    }

    [Fact]
    public async Task ActivityRepository_AddAndStatistics_WorkCorrectly()
    {
        var repo = new ActivityRepository(_context);

        var log1 = new ActivityLogItem
        {
            FileName = "photo.jpg",
            Extension = "jpg",
            Category = "Images",
            SourcePath = @"C:\In\photo.jpg",
            DestinationPath = @"C:\In\Images\photo.jpg",
            Status = OperationStatus.SUCCESS,
            Timestamp = DateTime.UtcNow
        };

        var log2 = new ActivityLogItem
        {
            FileName = "corrupt.bin",
            Extension = "bin",
            Category = "Other",
            SourcePath = @"C:\In\corrupt.bin",
            DestinationPath = @"C:\In\Other\corrupt.bin",
            Status = OperationStatus.FAILED,
            ErrorMessage = "Drive locked",
            Timestamp = DateTime.UtcNow
        };

        await repo.AddAsync(log1);
        await repo.AddAsync(log2);

        var recent = await repo.GetRecentAsync(10);
        Assert.Equal(2, recent.Count);

        var querySuccess = await repo.QueryAsync(status: OperationStatus.SUCCESS);
        Assert.Single(querySuccess);

        var querySearch = await repo.QueryAsync(search: "photo");
        Assert.Single(querySearch);

        var stats = await repo.GetDashboardStatisticsAsync();
        Assert.Equal(1, stats.SuccessfulOperations);
        Assert.Equal(1, stats.FailedOperations);
    }

    [Fact]
    public async Task SettingsRepository_PersistsAndReadsSettings()
    {
        var repo = new SettingsRepository(_context);
        var settings = await repo.GetSettingsAsync();

        settings.CompactMode = true;
        settings.CollisionBehavior = CollisionBehavior.Replace;
        await repo.SaveSettingsAsync(settings);

        var reloaded = await repo.GetSettingsAsync();
        Assert.True(reloaded.CompactMode);
        Assert.Equal(CollisionBehavior.Replace, reloaded.CollisionBehavior);
    }

    public void Dispose()
    {
        if (File.Exists(_tempDbPath))
        {
            try { File.Delete(_tempDbPath); } catch { }
        }
    }
}

