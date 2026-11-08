using Watcher.Core.Models;
using Watcher.Core.Organization;
using Xunit;

namespace Watcher.Core.Tests;

public class FileCollisionResolverTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileCollisionResolver _resolver;

    public FileCollisionResolverTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "WatcherTests_Collision_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _resolver = new FileCollisionResolver();
    }

    [Fact]
    public void ResolveDestination_FileDoesNotExist_ReturnsDirectPath()
    {
        var dest = _resolver.ResolveDestination(_tempDir, "newfile.txt", CollisionBehavior.Rename);
        Assert.Equal(Path.Combine(_tempDir, "newfile.txt"), dest);
    }

    [Fact]
    public void ResolveDestination_FileExists_RenameBehavior_GeneratesNumberedSuffix()
    {
        var initialPath = Path.Combine(_tempDir, "data.csv");
        File.WriteAllText(initialPath, "test");

        var dest = _resolver.ResolveDestination(_tempDir, "data.csv", CollisionBehavior.Rename);
        Assert.Equal(Path.Combine(_tempDir, "data (1).csv"), dest);

        // If data (1).csv exists as well:
        File.WriteAllText(dest!, "test 1");
        var dest2 = _resolver.ResolveDestination(_tempDir, "data.csv", CollisionBehavior.Rename);
        Assert.Equal(Path.Combine(_tempDir, "data (2).csv"), dest2);
    }

    [Fact]
    public void ResolveDestination_FileExists_SkipBehavior_ReturnsNull()
    {
        var initialPath = Path.Combine(_tempDir, "photo.jpg");
        File.WriteAllText(initialPath, "test");

        var dest = _resolver.ResolveDestination(_tempDir, "photo.jpg", CollisionBehavior.Skip);
        Assert.Null(dest);
    }

    [Fact]
    public void ResolveDestination_FileExists_ReplaceBehavior_ReturnsSamePath()
    {
        var initialPath = Path.Combine(_tempDir, "report.pdf");
        File.WriteAllText(initialPath, "test");

        var dest = _resolver.ResolveDestination(_tempDir, "report.pdf", CollisionBehavior.Replace);
        Assert.Equal(initialPath, dest);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); } catch { }
        }
    }
}

