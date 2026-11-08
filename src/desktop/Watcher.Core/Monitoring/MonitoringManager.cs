using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;

namespace Watcher.Core.Monitoring;

public class MonitoringManager : IFileMonitoringService, IDisposable
{
    private readonly IFolderRepository _folderRepository;
    private readonly IFileOrganizationService _organizationService;
    private readonly IFileReadinessDetector _readinessDetector;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MonitoringManager> _logger;

    private readonly ConcurrentDictionary<string, FolderMonitor> _monitors = new();
    private readonly ConcurrentDictionary<string, FolderStatus> _statuses = new();
    private bool _isRunning = false;
    private bool _isDisposed = false;

    public event EventHandler<FolderStatusChangedEventArgs>? FolderStatusChanged;
    public event EventHandler<bool>? GlobalMonitoringChanged;

    public bool IsRunning => _isRunning;
    public IReadOnlyDictionary<string, FolderStatus> FolderStatuses => _statuses;

    public MonitoringManager(
        IFolderRepository folderRepository,
        IFileOrganizationService organizationService,
        IFileReadinessDetector readinessDetector,
        ILoggerFactory loggerFactory)
    {
        _folderRepository = folderRepository;
        _organizationService = organizationService;
        _readinessDetector = readinessDetector;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MonitoringManager>();
    }

    public async Task StartAllAsync()
    {
        _logger.LogInformation("Starting monitoring on all enabled folders...");
        var folders = await _folderRepository.GetAllAsync();

        foreach (var folder in folders.Where(f => f.Enabled))
        {
            await StartFolderInternalAsync(folder);
        }

        _isRunning = true;
        GlobalMonitoringChanged?.Invoke(this, true);
    }

    public async Task StopAllAsync()
    {
        _logger.LogInformation("Stopping all active folder monitors...");
        foreach (var kvp in _monitors)
        {
            await kvp.Value.StopAsync();
            kvp.Value.Dispose();
        }

        _monitors.Clear();
        _isRunning = false;
        GlobalMonitoringChanged?.Invoke(this, false);
    }

    public Task PauseAsync()
    {
        _logger.LogInformation("Pausing all folder monitors...");
        foreach (var monitor in _monitors.Values)
        {
            monitor.Pause();
        }
        _isRunning = false;
        GlobalMonitoringChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        _logger.LogInformation("Resuming all folder monitors...");
        foreach (var monitor in _monitors.Values)
        {
            monitor.Resume();
        }
        _isRunning = true;
        GlobalMonitoringChanged?.Invoke(this, true);
        return Task.CompletedTask;
    }

    public async Task StartFolderAsync(string folderId)
    {
        var folder = await _folderRepository.GetByIdAsync(folderId);
        if (folder != null)
        {
            folder.Enabled = true;
            await _folderRepository.UpdateAsync(folder);
            await StartFolderInternalAsync(folder);
        }
    }

    public async Task StopFolderAsync(string folderId)
    {
        if (_monitors.TryRemove(folderId, out var monitor))
        {
            await monitor.StopAsync();
            monitor.Dispose();
            _statuses[folderId] = FolderStatus.Paused;

            var folder = await _folderRepository.GetByIdAsync(folderId);
            if (folder != null)
            {
                folder.Enabled = false;
                await _folderRepository.UpdateAsync(folder);
            }
        }
    }

    public async Task ReloadFoldersAsync()
    {
        _logger.LogInformation("Reloading folder monitor configuration...");
        var folders = await _folderRepository.GetAllAsync();
        var folderIds = folders.Select(f => f.Id).ToHashSet();

        // Stop removed folders
        var toRemove = _monitors.Keys.Where(id => !folderIds.Contains(id)).ToList();
        foreach (var id in toRemove)
        {
            await StopFolderAsync(id);
        }

        // Start / restart enabled folders
        foreach (var folder in folders)
        {
            if (folder.Enabled && !_monitors.ContainsKey(folder.Id))
            {
                await StartFolderInternalAsync(folder);
            }
            else if (!folder.Enabled && _monitors.ContainsKey(folder.Id))
            {
                await StopFolderAsync(folder.Id);
            }
        }
    }

    private async Task StartFolderInternalAsync(WatchedFolder folder)
    {
        if (_monitors.ContainsKey(folder.Id))
        {
            return;
        }

        // First, startup cleanup: organize any pre-existing files in top-level directory
        _ = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("Scanning existing files in '{Path}'...", folder.Path);
                int organized = await _organizationService.OrganizeExistingFilesAsync(
                    folder.Path,
                    folder.UnknownTarget,
                    folder.Quiet);
                if (organized > 0)
                {
                    _logger.LogInformation("Startup cleanup organized {Count} existing files in '{Path}'.", organized, folder.Path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error organizing existing files in '{Path}'", folder.Path);
            }
        });

        var monitorLogger = _loggerFactory.CreateLogger<FolderMonitor>();
        var monitor = new FolderMonitor(folder, _organizationService, _readinessDetector, monitorLogger);

        monitor.StatusChanged += (s, e) =>
        {
            _statuses[e.FolderId] = e.Status;
            FolderStatusChanged?.Invoke(this, e);
        };

        _monitors[folder.Id] = monitor;
        await monitor.StartAsync();
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _ = StopAllAsync();
        }
    }
}

