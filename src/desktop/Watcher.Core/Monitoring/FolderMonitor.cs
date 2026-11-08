using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;

namespace Watcher.Core.Monitoring;

public class FolderMonitor : IDisposable
{
    private readonly WatchedFolder _folder;
    private readonly IFileOrganizationService _organizationService;
    private readonly IFileReadinessDetector _readinessDetector;
    private readonly ILogger? _logger;

    private FileSystemWatcher? _watcher;
    private readonly Channel<string> _eventChannel;
    private readonly ConcurrentDictionary<string, DateTime> _recentEvents = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource? _cts;
    private Task? _processingTask;
    private bool _isPaused = false;
    private bool _isDisposed = false;

    public string FolderId => _folder.Id;
    public string FolderPath => _folder.Path;
    public bool IsActive => _watcher != null && _watcher.EnableRaisingEvents;

    public event EventHandler<FolderStatusChangedEventArgs>? StatusChanged;

    public FolderMonitor(
        WatchedFolder folder,
        IFileOrganizationService organizationService,
        IFileReadinessDetector readinessDetector,
        ILogger? logger = null)
    {
        _folder = folder;
        _organizationService = organizationService;
        _readinessDetector = readinessDetector;
        _logger = logger;
        _eventChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public Task StartAsync()
    {
        if (!Directory.Exists(_folder.Path))
        {
            _logger?.LogWarning("Watched directory '{Path}' does not exist.", _folder.Path);
            StatusChanged?.Invoke(this, new FolderStatusChangedEventArgs
            {
                FolderId = _folder.Id,
                FolderPath = _folder.Path,
                Status = FolderStatus.NotFound,
                ErrorMessage = "Directory does not exist or drive is disconnected."
            });
            return Task.CompletedTask;
        }

        try
        {
            _cts = new CancellationTokenSource();
            _processingTask = Task.Run(() => ProcessEventsAsync(_cts.Token));

            _watcher = new FileSystemWatcher(_folder.Path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                IncludeSubdirectories = _folder.Recursive,
                InternalBufferSize = 65536 // 64 KB buffer to prevent overflow
            };

            _watcher.Created += OnFileSystemEvent;
            _watcher.Renamed += OnRenamedEvent;
            _watcher.Error += OnWatcherError;

            _watcher.EnableRaisingEvents = true;

            StatusChanged?.Invoke(this, new FolderStatusChangedEventArgs
            {
                FolderId = _folder.Id,
                FolderPath = _folder.Path,
                Status = FolderStatus.Active
            });

            _logger?.LogInformation("Started monitoring folder: '{Path}' (Recursive: {Rec})", _folder.Path, _folder.Recursive);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start FileSystemWatcher for '{Path}'", _folder.Path);
            StatusChanged?.Invoke(this, new FolderStatusChangedEventArgs
            {
                FolderId = _folder.Id,
                FolderPath = _folder.Path,
                Status = FolderStatus.Error,
                ErrorMessage = ex.Message
            });
        }

        return Task.CompletedTask;
    }

    public void Pause()
    {
        _isPaused = true;
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
        }
    }

    public void Resume()
    {
        _isPaused = false;
        if (_watcher != null && Directory.Exists(_folder.Path))
        {
            _watcher.EnableRaisingEvents = true;
        }
    }

    public async Task StopAsync()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnFileSystemEvent;
            _watcher.Renamed -= OnRenamedEvent;
            _watcher.Error -= OnWatcherError;
            _watcher.Dispose();
            _watcher = null;
        }

        if (_cts != null)
        {
            _cts.Cancel();
            if (_processingTask != null)
            {
                try
                {
                    await _processingTask;
                }
                catch (OperationCanceledException) { }
            }
            _cts.Dispose();
            _cts = null;
        }

        StatusChanged?.Invoke(this, new FolderStatusChangedEventArgs
        {
            FolderId = _folder.Id,
            FolderPath = _folder.Path,
            Status = FolderStatus.Paused
        });
    }

    private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
    {
        QueueFile(e.FullPath);
    }

    private void OnRenamedEvent(object sender, RenamedEventArgs e)
    {
        QueueFile(e.FullPath);
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _logger?.LogError(e.GetException(), "FileSystemWatcher buffer overflow or system error for '{Path}'", _folder.Path);
        StatusChanged?.Invoke(this, new FolderStatusChangedEventArgs
        {
            FolderId = _folder.Id,
            FolderPath = _folder.Path,
            Status = FolderStatus.Error,
            ErrorMessage = "Internal buffer overflow or drive connection issue."
        });
    }

    private void QueueFile(string fullPath)
    {
        if (_isPaused || _isDisposed)
            return;

        try
        {
            // Ignore directories
            if (Directory.Exists(fullPath))
                return;

            // Ignore temporary extensions
            if (_readinessDetector.IsTemporaryOrIncompleteFile(fullPath))
                return;

            // Debounce duplicate events within 1 second
            var now = DateTime.UtcNow;
            if (_recentEvents.TryGetValue(fullPath, out var lastTime) && (now - lastTime).TotalMilliseconds < 1000)
            {
                return;
            }
            _recentEvents[fullPath] = now;

            // Clean up old debounced entries
            if (_recentEvents.Count > 500)
            {
                var staleKeys = _recentEvents.Where(kvp => (now - kvp.Value).TotalSeconds > 10).Select(k => k.Key).ToList();
                foreach (var k in staleKeys)
                {
                    _recentEvents.TryRemove(k, out _);
                }
            }

            _eventChannel.Writer.TryWrite(fullPath);
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Error enqueueing file event for '{Path}'", fullPath);
        }
    }

    private async Task ProcessEventsAsync(CancellationToken cancellationToken)
    {
        var reader = _eventChannel.Reader;

        while (!cancellationToken.IsCancellationRequested && await reader.WaitToReadAsync(cancellationToken))
        {
            while (reader.TryRead(out var filePath))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await _organizationService.OrganizeFileAsync(
                        filePath,
                        _folder.Path,
                        _folder.UnknownTarget,
                        _folder.Quiet,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Unexpected error processing queued file '{File}'", filePath);
                }
            }
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _ = StopAsync();
        }
    }
}

