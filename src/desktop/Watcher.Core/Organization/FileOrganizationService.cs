using Microsoft.Extensions.Logging;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;

namespace Watcher.Core.Organization;

public class FileOrganizationService : IFileOrganizationService
{
    private readonly IRuleEngine _ruleEngine;
    private readonly IFileReadinessDetector _readinessDetector;
    private readonly IFileCollisionResolver _collisionResolver;
    private readonly IInfiniteLoopGuard _loopGuard;
    private readonly IActivityRepository _activityRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ILogger<FileOrganizationService>? _logger;

    public event EventHandler<FileOrganizedEventArgs>? FileOrganized;

    public FileOrganizationService(
        IRuleEngine ruleEngine,
        IFileReadinessDetector readinessDetector,
        IFileCollisionResolver collisionResolver,
        IInfiniteLoopGuard loopGuard,
        IActivityRepository activityRepository,
        IFolderRepository folderRepository,
        ISettingsRepository settingsRepository,
        ILogger<FileOrganizationService>? logger = null)
    {
        _ruleEngine = ruleEngine;
        _readinessDetector = readinessDetector;
        _collisionResolver = collisionResolver;
        _loopGuard = loopGuard;
        _activityRepository = activityRepository;
        _folderRepository = folderRepository;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    public async Task<ActivityLogItem> OrganizeFileAsync(
        string filePath,
        string sourceWatchedFolder,
        string fallbackUnknownTarget,
        bool quiet,
        CancellationToken cancellationToken = default)
    {
        var fileName = Path.GetFileName(filePath);
        var ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();

        var log = new ActivityLogItem
        {
            FileName = fileName,
            Extension = ext,
            SourcePath = filePath,
            Timestamp = DateTime.UtcNow,
            CanUndo = false
        };

        if (!File.Exists(filePath))
        {
            log.Status = OperationStatus.SKIPPED;
            log.ErrorMessage = "File no longer exists.";
            return log;
        }

        if (_readinessDetector.IsTemporaryOrIncompleteFile(filePath))
        {
            log.Status = OperationStatus.SKIPPED;
            log.ErrorMessage = "Temporary or incomplete download file.";
            return log;
        }

        var settings = await _settingsRepository.GetSettingsAsync();

        // 1. Wait for file readiness (writes complete, size stable, un-locked)
        bool ready = await _readinessDetector.WaitUntilReadyAsync(
            filePath,
            settings.ReadinessTimeoutMs,
            settings.ReadinessPollIntervalMs,
            cancellationToken);

        if (!ready)
        {
            log.Status = OperationStatus.FAILED;
            log.ErrorMessage = "File readiness check timed out or file locked.";
            await _activityRepository.AddAsync(log);
            return log;
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            log.FileSizeBytes = fileInfo.Length;

            // 2. Rule evaluation
            var (category, targetFolder) = _ruleEngine.EvaluateFile(fileName, fallbackUnknownTarget);
            log.Category = category;

            // Target directory determination (relative to watched folder or absolute)
            string targetDir = Path.IsPathRooted(targetFolder)
                ? targetFolder
                : Path.Combine(sourceWatchedFolder, targetFolder);

            // 3. Infinite loop prevention
            if (_loopGuard.IsCyclicPath(sourceWatchedFolder, targetDir))
            {
                log.Status = OperationStatus.SKIPPED;
                log.ErrorMessage = "Target folder is identical to source folder.";
                return log;
            }

            // Ensure destination directory exists
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // 4. Collision resolution
            string? destinationPath = _collisionResolver.ResolveDestination(targetDir, fileName, settings.CollisionBehavior);
            if (string.IsNullOrEmpty(destinationPath))
            {
                log.Status = OperationStatus.SKIPPED;
                log.ErrorMessage = "File already exists at destination and collision rule is 'Skip'.";
                await _activityRepository.AddAsync(log);
                return log;
            }

            log.DestinationPath = destinationPath;

            if (string.Equals(filePath, destinationPath, StringComparison.OrdinalIgnoreCase))
            {
                log.Status = OperationStatus.SKIPPED;
                log.ErrorMessage = "File is already in target destination.";
                return log;
            }

            // 5. Safe file movement
            bool overwrite = settings.CollisionBehavior == CollisionBehavior.Replace;
            MoveFileSafely(filePath, destinationPath, overwrite);

            log.Status = OperationStatus.SUCCESS;
            log.CanUndo = true;

            // Update stats & folder counter
            var folder = await _folderRepository.GetByPathAsync(sourceWatchedFolder);
            if (folder != null)
            {
                await _folderRepository.IncrementOrganizedCountAsync(folder.Id);
            }

            await _activityRepository.AddAsync(log);

            _logger?.LogInformation("Organized: '{File}' -> '{Dest}' ({Category})", fileName, destinationPath, category);

            // Raise notification event
            FileOrganized?.Invoke(this, new FileOrganizedEventArgs { LogItem = log });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to organize file '{File}'", filePath);
            log.Status = OperationStatus.FAILED;
            log.ErrorMessage = ex.Message;
            await _activityRepository.AddAsync(log);
        }

        return log;
    }

    public async Task<int> OrganizeExistingFilesAsync(
        string folderPath,
        string fallbackUnknownTarget,
        bool quiet,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(folderPath))
            return 0;

        int processed = 0;
        try
        {
            var files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var result = await OrganizeFileAsync(file, folderPath, fallbackUnknownTarget, quiet, cancellationToken);
                if (result.Status == OperationStatus.SUCCESS)
                {
                    processed++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error scanning existing files in '{Folder}'", folderPath);
        }

        return processed;
    }

    public async Task<bool> UndoOperationAsync(string activityLogId, CancellationToken cancellationToken = default)
    {
        var log = await _activityRepository.GetByIdAsync(activityLogId);
        if (log == null || !log.CanUndo || string.IsNullOrEmpty(log.DestinationPath) || string.IsNullOrEmpty(log.SourcePath))
        {
            return false;
        }

        if (!File.Exists(log.DestinationPath))
        {
            return false;
        }

        try
        {
            var originalDir = Path.GetDirectoryName(log.SourcePath);
            if (!string.IsNullOrEmpty(originalDir) && !Directory.Exists(originalDir))
            {
                Directory.CreateDirectory(originalDir);
            }

            MoveFileSafely(log.DestinationPath, log.SourcePath, overwrite: false);
            await _activityRepository.MarkUndoneAsync(activityLogId);
            _logger?.LogInformation("Undid organization for '{File}' back to '{Source}'", log.FileName, log.SourcePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to undo file organization for log ID '{Id}'", activityLogId);
            return false;
        }
    }

    private static void MoveFileSafely(string source, string destination, bool overwrite)
    {
        if (overwrite && File.Exists(destination))
        {
            File.Delete(destination);
        }

        try
        {
            File.Move(source, destination);
        }
        catch (IOException)
        {
            // If cross-volume or standard move fails, copy then delete
            File.Copy(source, destination, overwrite);
            File.Delete(source);
        }
    }
}

