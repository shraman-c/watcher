using Watcher.Core.Organization;
using Xunit;

namespace Watcher.Core.Tests;

public class InfiniteLoopGuardTests
{
    private readonly InfiniteLoopGuard _guard = new();

    [Fact]
    public void IsCyclicPath_SameFolder_ReturnsTrue()
    {
        Assert.True(_guard.IsCyclicPath(@"C:\Downloads", @"C:\Downloads\"));
        Assert.True(_guard.IsCyclicPath(@"C:/Downloads/", @"C:\Downloads"));
    }

    [Fact]
    public void IsCyclicPath_Subfolder_ReturnsFalse()
    {
        Assert.False(_guard.IsCyclicPath(@"C:\Downloads", @"C:\Downloads\Images"));
    }

    [Fact]
    public void IsSubDestinationPath_MatchesTargetFolderNames()
    {
        var targets = new[] { "Images", "Documents", "Other" };

        Assert.True(_guard.IsSubDestinationPath(@"C:\Downloads\Images", targets));
        Assert.True(_guard.IsSubDestinationPath(@"C:\Downloads\Documents\", targets));
        Assert.False(_guard.IsSubDestinationPath(@"C:\Downloads\MyProject", targets));
    }
}

