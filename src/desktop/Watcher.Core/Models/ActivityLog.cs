namespace Watcher.Core.Models;

public class ActivityLogItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public OperationStatus Status { get; set; } = OperationStatus.SUCCESS;
    public string? ErrorMessage { get; set; }
    public long? FileSizeBytes { get; set; }
    public bool CanUndo { get; set; } = true;
}

