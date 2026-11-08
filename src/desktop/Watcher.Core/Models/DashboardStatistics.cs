namespace Watcher.Core.Models;

public class ActivityOverTimeItem
{
    public string Date { get; set; } = string.Empty;
    public int Total { get; set; } = 0;
    public int Success { get; set; } = 0;
    public int Failed { get; set; } = 0;
}

public class CategoryStatItem
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; } = 0;
    public double Percentage { get; set; } = 0;
}

public class FolderStatItem
{
    public string FolderId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int Count { get; set; } = 0;
}

public class DashboardStatistics
{
    public int ActiveFoldersCount { get; set; }
    public int TotalFoldersCount { get; set; }
    public int TotalFilesOrganized { get; set; }
    public int FilesOrganizedToday { get; set; }
    public int SuccessfulOperations { get; set; }
    public int FailedOperations { get; set; }
    public int CategoriesCount { get; set; }
    public bool IsMonitoringActive { get; set; }
    public long UptimeSeconds { get; set; }
    public List<ActivityOverTimeItem> ActivityOverTime { get; set; } = new();
    public List<CategoryStatItem> CategoryBreakdown { get; set; } = new();
    public List<FolderStatItem> FolderBreakdown { get; set; } = new();
}

