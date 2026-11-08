namespace Watcher.Core.Interfaces;

public interface IInfiniteLoopGuard
{
    /// <summary>
    /// Checks whether targetDirectory would create a cyclic monitoring loop inside any active watched folder.
    /// </summary>
    bool IsCyclicPath(string sourceFolder, string targetDirectory);

    /// <summary>
    /// Determines whether a path is an excluded sub-destination directory inside watched folders.
    /// </summary>
    bool IsSubDestinationPath(string path, IEnumerable<string> knownSubTargets);
}

