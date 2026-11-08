using Watcher.Core.Models;

namespace Watcher.Core.Interfaces;

public class FolderStatusChangedEventArgs : EventArgs
{
    public string FolderId { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public FolderStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface IFileMonitoringService
{
    event EventHandler<FolderStatusChangedEventArgs>? FolderStatusChanged;
    event EventHandler<bool>? GlobalMonitoringChanged;

    bool IsRunning { get; }
    IReadOnlyDictionary<string, FolderStatus> FolderStatuses { get; }

    Task StartAllAsync();
    Task StopAllAsync();
    Task PauseAsync();
    Task ResumeAsync();

    Task StartFolderAsync(string folderId);
    Task StopFolderAsync(string folderId);

    Task ReloadFoldersAsync();
}

