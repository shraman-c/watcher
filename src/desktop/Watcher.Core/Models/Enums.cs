namespace Watcher.Core.Models;

public enum OperationStatus
{
    SUCCESS,
    FAILED,
    SKIPPED
}

public enum CollisionBehavior
{
    Rename,
    Skip,
    Replace
}

public enum AppTheme
{
    System,
    Dark,
    Light
}

public enum FolderStatus
{
    Active,
    Paused,
    Error,
    NotFound
}

