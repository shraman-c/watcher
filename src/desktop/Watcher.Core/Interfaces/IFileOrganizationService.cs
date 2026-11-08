using Watcher.Core.Models;

namespace Watcher.Core.Interfaces;

public class FileOrganizedEventArgs : EventArgs
{
    public ActivityLogItem LogItem { get; set; } = null!;
}

public interface IFileOrganizationService
{
    event EventHandler<FileOrganizedEventArgs>? FileOrganized;

    Task<ActivityLogItem> OrganizeFileAsync(
        string filePath,
        string sourceWatchedFolder,
        string fallbackUnknownTarget,
        bool quiet,
        CancellationToken cancellationToken = default);

    Task<int> OrganizeExistingFilesAsync(
        string folderPath,
        string fallbackUnknownTarget,
        bool quiet,
        CancellationToken cancellationToken = default);

    Task<bool> UndoOperationAsync(string activityLogId, CancellationToken cancellationToken = default);
}

