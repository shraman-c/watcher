using Microsoft.Extensions.Logging;
using Watcher.Core.Interfaces;

namespace Watcher.Core.Organization;

public class FileReadinessDetector : IFileReadinessDetector
{
    private readonly ILogger<FileReadinessDetector>? _logger;

    private static readonly HashSet<string> IncompleteExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".crdownload",
        ".part",
        ".partial",
        ".tmp",
        ".temp",
        ".download",
        ".opdownload",
        ".aria2"
    };

    public FileReadinessDetector(ILogger<FileReadinessDetector>? logger = null)
    {
        _logger = logger;
    }

    public bool IsTemporaryOrIncompleteFile(string filePath)
    {
        var name = Path.GetFileName(filePath);
        if (string.IsNullOrWhiteSpace(name))
            return true;

        // Microsoft Office temporary files
        if (name.StartsWith("~$", StringComparison.OrdinalIgnoreCase))
            return true;

        // Hidden / OS temp files
        if (name.StartsWith(".~", StringComparison.OrdinalIgnoreCase))
            return true;

        var ext = Path.GetExtension(filePath);
        return IncompleteExtensions.Contains(ext);
    }

    public async Task<bool> WaitUntilReadyAsync(
        string filePath,
        int timeoutMs = 15000,
        int pollIntervalMs = 400,
        CancellationToken cancellationToken = default)
    {
        if (IsTemporaryOrIncompleteFile(filePath))
        {
            _logger?.LogDebug("File '{FilePath}' is recognized as incomplete or temporary. Skipping.", filePath);
            return false;
        }

        var startTime = DateTime.UtcNow;
        long lastSize = -1;
        int consecutiveStableChecks = 0;
        const int requiredStableChecks = 2;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(filePath))
            {
                // File may have been removed or renamed while downloading
                return false;
            }

            try
            {
                var fileInfo = new FileInfo(filePath);
                long currentSize = fileInfo.Length;

                // Test exclusive read access to verify file is not locked by another process
                using (var stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (currentSize == lastSize && currentSize >= 0)
                    {
                        consecutiveStableChecks++;
                        if (consecutiveStableChecks >= requiredStableChecks)
                        {
                            _logger?.LogDebug("File '{FilePath}' is stable and ready (size: {Size} bytes).", filePath, currentSize);
                            return true;
                        }
                    }
                    else
                    {
                        consecutiveStableChecks = 0;
                    }

                    lastSize = currentSize;
                }
            }
            catch (IOException)
            {
                // File is currently locked by downloading process or writer
                consecutiveStableChecks = 0;
            }
            catch (UnauthorizedAccessException)
            {
                // Permission delay
                consecutiveStableChecks = 0;
            }

            try
            {
                await Task.Delay(pollIntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        _logger?.LogWarning("Timeout waiting for file '{FilePath}' to become ready.", filePath);
        return false;
    }
}

