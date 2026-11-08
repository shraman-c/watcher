using Watcher.Core.Models;

namespace Watcher.Core.Rules;

public static class DefaultCategories
{
    public static List<Category> GetDefaults()
    {
        return new List<Category>
        {
            new()
            {
                Id = "cat-documents",
                Name = "Documents",
                DefaultFolder = "Documents",
                Color = "#4A90E2",
                IconName = "FileText",
                IsSystem = true,
                Extensions = new() { "pdf", "doc", "docx", "txt", "rtf", "odt" }
            },
            new()
            {
                Id = "cat-spreadsheets",
                Name = "Spreadsheets",
                DefaultFolder = "Spreadsheets",
                Color = "#35B77A",
                IconName = "Table",
                IsSystem = true,
                Extensions = new() { "xls", "xlsx", "csv" }
            },
            new()
            {
                Id = "cat-images",
                Name = "Images",
                DefaultFolder = "Images",
                Color = "#9B51E0",
                IconName = "Image",
                IsSystem = true,
                Extensions = new() { "jpg", "jpeg", "png", "gif", "bmp", "svg", "webp" }
            },
            new()
            {
                Id = "cat-videos",
                Name = "Videos",
                DefaultFolder = "Videos",
                Color = "#E36F75",
                IconName = "Video",
                IsSystem = true,
                Extensions = new() { "mp4", "mkv", "avi", "mov", "webm" }
            },
            new()
            {
                Id = "cat-audio",
                Name = "Audio",
                DefaultFolder = "Audio",
                Color = "#F2994A",
                IconName = "Music",
                IsSystem = true,
                Extensions = new() { "mp3", "wav", "flac", "aac", "ogg" }
            },
            new()
            {
                Id = "cat-archives",
                Name = "Archives",
                DefaultFolder = "Archives",
                Color = "#E9AE54",
                IconName = "Archive",
                IsSystem = true,
                Extensions = new() { "zip", "rar", "7z", "tar", "gz" }
            },
            new()
            {
                Id = "cat-executables",
                Name = "Executables",
                DefaultFolder = "Executables",
                Color = "#6C9BFF",
                IconName = "Terminal",
                IsSystem = true,
                Extensions = new() { "exe", "msi", "msix" }
            },
            new()
            {
                Id = "cat-code",
                Name = "Code",
                DefaultFolder = "Code",
                Color = "#2D9CDB",
                IconName = "Code",
                IsSystem = true,
                Extensions = new() { "py", "js", "ts", "html", "css", "cpp", "c", "java", "cs" }
            },
            new()
            {
                Id = "cat-fonts",
                Name = "Fonts",
                DefaultFolder = "Fonts",
                Color = "#BB6BD9",
                IconName = "Type",
                IsSystem = true,
                Extensions = new() { "ttf", "otf", "woff", "woff2" }
            }
        };
    }

    public static List<OrganizationRule> GetDefaultRules()
    {
        var categories = GetDefaults();
        var rules = new List<OrganizationRule>();
        int priority = 10;

        foreach (var cat in categories)
        {
            rules.Add(new OrganizationRule
            {
                Id = $"rule-{cat.Name.ToLowerInvariant()}",
                CategoryId = cat.Id,
                CategoryName = cat.Name,
                Extensions = new List<string>(cat.Extensions),
                TargetFolder = cat.DefaultFolder,
                Priority = priority++,
                Enabled = true,
                IsCustom = false
            });
        }

        return rules;
    }
}

