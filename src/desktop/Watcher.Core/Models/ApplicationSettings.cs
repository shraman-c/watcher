namespace Watcher.Core.Models;

public class ApplicationSettings
{
    // General
    public bool LaunchOnStartup { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool CloseToTray { get; set; } = true;
    public bool StartMonitoringOnLaunch { get; set; } = true;

    // Appearance
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public bool CompactMode { get; set; } = false;

    // Notifications
    public bool NotifyOnSuccess { get; set; } = true;
    public bool NotifyOnError { get; set; } = true;
    public bool NotifyOnMonitoringStatus { get; set; } = true;

    // Organization
    public CollisionBehavior CollisionBehavior { get; set; } = CollisionBehavior.Rename;
    public string DefaultUnknownTarget { get; set; } = "Other";
    public int ReadinessTimeoutMs { get; set; } = 15000;
    public int ReadinessPollIntervalMs { get; set; } = 400;
    public bool RecursiveDefault { get; set; } = false;

    // Advanced
    public string LogLevel { get; set; } = "INFO";
    public int AutoCleanupDatabaseDays { get; set; } = 30;
}

