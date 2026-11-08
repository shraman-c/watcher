using Watcher.Core.Models;

namespace Watcher.Core.Interfaces;

public interface IFolderRepository
{
    Task<List<WatchedFolder>> GetAllAsync();
    Task<WatchedFolder?> GetByIdAsync(string id);
    Task<WatchedFolder?> GetByPathAsync(string path);
    Task<WatchedFolder> AddAsync(WatchedFolder folder);
    Task<WatchedFolder> UpdateAsync(WatchedFolder folder);
    Task<bool> DeleteAsync(string id);
    Task IncrementOrganizedCountAsync(string folderId);
}

public interface IRuleRepository
{
    Task<List<OrganizationRule>> GetAllAsync();
    Task<OrganizationRule?> GetByIdAsync(string id);
    Task<OrganizationRule> AddAsync(OrganizationRule rule);
    Task<OrganizationRule> UpdateAsync(OrganizationRule rule);
    Task<bool> DeleteAsync(string id);
    Task SaveAllAsync(IEnumerable<OrganizationRule> rules);

    Task<List<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(string id);
    Task<Category> AddCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task<bool> DeleteCategoryAsync(string id);
}

public interface IActivityRepository
{
    Task<ActivityLogItem> AddAsync(ActivityLogItem item);
    Task<List<ActivityLogItem>> GetRecentAsync(int limit = 50);
    Task<List<ActivityLogItem>> QueryAsync(
        string? search = null,
        string? category = null,
        OperationStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20);
    Task<int> GetTotalCountAsync(
        string? search = null,
        string? category = null,
        OperationStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    Task<ActivityLogItem?> GetByIdAsync(string id);
    Task<bool> MarkUndoneAsync(string id);
    Task ClearAllAsync();
    Task<DashboardStatistics> GetDashboardStatisticsAsync();
}

public interface ISettingsRepository
{
    Task<ApplicationSettings> GetSettingsAsync();
    Task SaveSettingsAsync(ApplicationSettings settings);
}

public interface ILegacyConfigMigrator
{
    Task MigrateIfNecessaryAsync();
}

