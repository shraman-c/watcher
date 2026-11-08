using System.Text.Json;
using Microsoft.Extensions.Logging;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;

namespace Watcher.Infrastructure.Configuration;

public class LegacyConfigMigrator : ILegacyConfigMigrator
{
    private readonly IFolderRepository _folderRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly ILogger<LegacyConfigMigrator>? _logger;

    public LegacyConfigMigrator(
        IFolderRepository folderRepository,
        IRuleRepository ruleRepository,
        ILogger<LegacyConfigMigrator>? logger = null)
    {
        _folderRepository = folderRepository;
        _ruleRepository = ruleRepository;
        _logger = logger;
    }

    public async Task MigrateIfNecessaryAsync()
    {
        var existingFolders = await _folderRepository.GetAllAsync();
        if (existingFolders.Count > 0)
        {
            _logger?.LogInformation("Database already contains watched folders. Skipping legacy migration.");
            return;
        }

        // Try user home directory first, then current directory
        var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var homeConfigPath = Path.Combine(userHome, ".watcher_config.json");
        var localSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        if (!File.Exists(localSettingsPath))
        {
            localSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
        }

        string? configToMigrate = null;
        if (File.Exists(homeConfigPath))
        {
            configToMigrate = homeConfigPath;
        }
        else if (File.Exists(localSettingsPath))
        {
            configToMigrate = localSettingsPath;
        }

        if (string.IsNullOrEmpty(configToMigrate))
        {
            _logger?.LogInformation("No legacy configuration file found to migrate.");
            return;
        }

        _logger?.LogInformation("Found legacy configuration at '{Path}'. Migrating to Watcher 2.0 database...", configToMigrate);

        try
        {
            var content = await File.ReadAllTextAsync(configToMigrate);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            // 1. Migrate folders
            if (root.TryGetProperty("folders", out var foldersElement) && foldersElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var folderEl in foldersElement.EnumerateArray())
                {
                    string path = string.Empty;
                    string unknown = "Other";
                    bool quiet = false;

                    if (folderEl.ValueKind == JsonValueKind.String)
                    {
                        path = folderEl.GetString() ?? string.Empty;
                    }
                    else if (folderEl.ValueKind == JsonValueKind.Object)
                    {
                        if (folderEl.TryGetProperty("path", out var pProp)) path = pProp.GetString() ?? "";
                        if (folderEl.TryGetProperty("unknown", out var uProp)) unknown = uProp.GetString() ?? "Other";
                        if (folderEl.TryGetProperty("quiet", out var qProp)) quiet = qProp.GetBoolean();
                    }

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        var normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        var existing = await _folderRepository.GetByPathAsync(normalizedPath);
                        if (existing == null)
                        {
                            await _folderRepository.AddAsync(new WatchedFolder
                            {
                                Path = normalizedPath,
                                UnknownTarget = string.IsNullOrWhiteSpace(unknown) ? "Other" : unknown,
                                Quiet = quiet,
                                Enabled = true,
                                Recursive = false
                            });
                            _logger?.LogInformation("Migrated legacy watched folder: '{Path}'", normalizedPath);
                        }
                    }
                }
            }

            // 2. Migrate rules
            if (root.TryGetProperty("rules_text", out var rulesTextElement))
            {
                var rulesText = rulesTextElement.GetString();
                if (!string.IsNullOrWhiteSpace(rulesText))
                {
                    await MigrateRulesTextAsync(rulesText);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to migrate legacy configuration from '{Path}'", configToMigrate);
        }
    }

    private async Task MigrateRulesTextAsync(string rulesText)
    {
        var lines = rulesText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var existingRules = await _ruleRepository.GetAllAsync();
        var existingCategories = await _ruleRepository.GetCategoriesAsync();
        int priority = existingRules.Count > 0 ? existingRules.Max(r => r.Priority) + 1 : 10;

        foreach (var line in lines)
        {
            var parts = line.Split('=', 2);
            if (parts.Length != 2) continue;

            var categoryName = parts[0].Trim();
            var extensions = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim().TrimStart('.').ToLowerInvariant())
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .ToList();

            if (string.IsNullOrEmpty(categoryName) || extensions.Count == 0) continue;

            var category = existingCategories.FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));
            if (category == null)
            {
                category = await _ruleRepository.AddCategoryAsync(new Category
                {
                    Name = categoryName,
                    DefaultFolder = categoryName,
                    Extensions = extensions,
                    Color = "#6C9BFF",
                    IconName = "Folder",
                    IsSystem = false
                });
            }

            var rule = existingRules.FirstOrDefault(r => string.Equals(r.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase));
            if (rule != null)
            {
                rule.Extensions = extensions;
                await _ruleRepository.UpdateAsync(rule);
            }
            else
            {
                await _ruleRepository.AddAsync(new OrganizationRule
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Extensions = extensions,
                    TargetFolder = category.DefaultFolder,
                    Priority = priority++,
                    Enabled = true,
                    IsCustom = !category.IsSystem
                });
            }
        }

        _logger?.LogInformation("Successfully migrated legacy organization rules.");
    }
}

