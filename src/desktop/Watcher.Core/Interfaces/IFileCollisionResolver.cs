using Watcher.Core.Models;

namespace Watcher.Core.Interfaces;

public interface IFileCollisionResolver
{
    /// <summary>
    /// Resolves the destination path in accordance with the specified collision behavior.
    /// Returns null if the operation should be skipped.
    /// </summary>
    string? ResolveDestination(string targetDirectory, string fileName, CollisionBehavior behavior);
}

