using Watcher.Core.Interfaces;

namespace Watcher.Core.Organization;

public class InfiniteLoopGuard : IInfiniteLoopGuard
{
    public bool IsCyclicPath(string sourceFolder, string targetDirectory)
    {
        var cleanSource = Normalize(sourceFolder);
        var cleanTarget = Normalize(targetDirectory);

        return string.Equals(cleanSource, cleanTarget, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsSubDestinationPath(string path, IEnumerable<string> knownSubTargets)
    {
        var normalizedPath = Normalize(path);
        var folderName = Path.GetFileName(normalizedPath);

        foreach (var sub in knownSubTargets)
        {
            var cleanSub = sub.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.Equals(folderName, cleanSub, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (normalizedPath.EndsWith(Path.DirectorySeparatorChar + cleanSub, StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.EndsWith(Path.AltDirectorySeparatorChar + cleanSub, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string path)
    {
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
