using Watcher.Core.Organization;
using Xunit;

namespace Watcher.Core.Tests;

public class FileReadinessDetectorTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileReadinessDetector _detector;

    public FileReadinessDetectorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "WatcherTests_Readiness_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _detector = new FileReadinessDetector();
    }

    [Theory]
    [InlineData("test.crdownload")]
    [InlineData("archive.part")]
    [InlineData("tempfile.tmp")]
    [InlineData("~$Document.docx")]
    [InlineData(".~lock.file")]
    public void IsTemporaryOrIncompleteFile_RecognizesIncomplete(string filename)
    {
        var filePath = Path.Combine(_tempDir, filename);
        Assert.True(_detector.IsTemporaryOrIncompleteFile(filePath));
    }

    [Fact]
    public async Task WaitUntilReadyAsync_StableFile_ReturnsTrue()
    {
        var filePath = Path.Combine(_tempDir, "ready_file.txt");
        await File.WriteAllTextAsync(filePath, "Hello world");

        bool ready = await _detector.WaitUntilReadyAsync(filePath, timeoutMs: 2000, pollIntervalMs: 50);
        Assert.True(ready);
    }

    [Fact]
    public async Task WaitUntilReadyAsync_NonExistentFile_ReturnsFalse()
    {
        var filePath = Path.Combine(_tempDir, "ghost_file.txt");
        bool ready = await _detector.WaitUntilReadyAsync(filePath, timeoutMs: 200, pollIntervalMs: 50);
        Assert.False(ready);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); } catch { }
        }
    }
}

