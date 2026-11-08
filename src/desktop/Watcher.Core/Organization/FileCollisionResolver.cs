using Watcher.Core.Interfaces;
using Watcher.Core.Models;

namespace Watcher.Core.Organization;

public class FileCollisionResolver : IFileCollisionResolver
{
    public string? ResolveDestination(string targetDirectory, string fileName, CollisionBehavior behavior)
    {
        var primaryPath = Path.Combine(targetDirectory, fileName);

        if (!File.Exists(primaryPath))
        {
            return primaryPath;
        }

        switch (behavior)
        {
            case CollisionBehavior.Replace:
                return primaryPath;

            case CollisionBehavior.Skip:
                return null;

            case CollisionBehavior.Rename:
            default:
                var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var ext = Path.GetExtension(fileName);

                for (int i = 1; i <= 10000; i++)
                {
                    var newName = $"{nameWithoutExt} ({i}){ext}";
                    var candidatePath = Path.Combine(targetDirectory, newName);
                    if (!File.Exists(candidatePath))
                    {
                        return candidatePath;
                    }
                }

                // If somehow 10,000 exist, append timestamp
                var timestampName = $"{nameWithoutExt}_{DateTime.UtcNow.Ticks}{ext}";
                return Path.Combine(targetDirectory, timestampName);
        }
    }
}

