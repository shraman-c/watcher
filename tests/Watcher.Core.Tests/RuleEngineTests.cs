using Watcher.Core.Models;
using Watcher.Core.Rules;
using Xunit;

namespace Watcher.Core.Tests;

public class RuleEngineTests
{
    [Fact]
    public void EvaluateFile_DefaultRules_MatchesCorrectCategory()
    {
        var engine = new RuleEngine();

        var (catDoc, targetDoc) = engine.EvaluateFile("resume.pdf", "Other");
        Assert.Equal("Documents", catDoc);
        Assert.Equal("Documents", targetDoc);

        var (catImg, targetImg) = engine.EvaluateFile("photo.PNG", "Other");
        Assert.Equal("Images", catImg);

        var (catCode, targetCode) = engine.EvaluateFile("script.py", "Other");
        Assert.Equal("Code", catCode);

        var (catSheet, targetSheet) = engine.EvaluateFile("budget.xlsx", "Other");
        Assert.Equal("Spreadsheets", catSheet);
    }

    [Fact]
    public void EvaluateFile_UnknownExtension_FallsBackToUnknownTarget()
    {
        var engine = new RuleEngine();

        var (cat, target) = engine.EvaluateFile("strange_file.xyz123", "Unsorted");
        Assert.Equal("Unsorted", cat);
        Assert.Equal("Unsorted", target);
    }

    [Fact]
    public void RefreshRules_CustomPriority_RespectsPriority()
    {
        var rules = new List<OrganizationRule>
        {
            new()
            {
                Id = "1",
                CategoryName = "WorkDocs",
                Extensions = new() { "pdf" },
                TargetFolder = "Work",
                Priority = 1,
                Enabled = true
            },
            new()
            {
                Id = "2",
                CategoryName = "GeneralDocs",
                Extensions = new() { "pdf", "txt" },
                TargetFolder = "General",
                Priority = 2,
                Enabled = true
            }
        };

        var engine = new RuleEngine(rules);
        var (category, target) = engine.EvaluateFile("invoice.pdf", "Other");

        Assert.Equal("WorkDocs", category);
        Assert.Equal("Work", target);
    }

    [Fact]
    public void EvaluateFile_NoExtension_ReturnsFallback()
    {
        var engine = new RuleEngine();
        var (cat, target) = engine.EvaluateFile("Makefile", "Other");
        Assert.Equal("Other", cat);
    }
}

