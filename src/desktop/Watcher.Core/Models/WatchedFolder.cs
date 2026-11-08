namespace Watcher.Core.Models;

public class WatchedFolder
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Path { get; set; } = string.Empty;
    public string UnknownTarget { get; set; } = "Other";
    public bool Quiet { get; set; } = false;
    public bool Enabled { get; set; } = true;
    public bool Recursive { get; set; } = false;
    public int FilesOrganizedCount { get; set; } = 0;
    public FolderStatus Status { get; set; } = FolderStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

