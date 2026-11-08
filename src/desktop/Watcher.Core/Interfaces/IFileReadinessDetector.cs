namespace Watcher.Core.Interfaces;

public interface IFileReadinessDetector
{
    /// <summary>
    /// Determines whether a file is ready to be moved (writes completed, size stable, not locked, not temporary).
    /// </summary>
    Task<bool> WaitUntilReadyAsync(string filePath, int timeoutMs = 15000, int pollIntervalMs = 400, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file is a transient/temporary file (e.g. .crdownload, .tmp, .part, ~$*) that should be ignored until finalized.
    /// </summary>
    bool IsTemporaryOrIncompleteFile(string filePath);
}

