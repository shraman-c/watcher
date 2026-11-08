using System.Text.Json;
using Microsoft.Data.Sqlite;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;
using Watcher.Core.Rules;
using Watcher.Infrastructure.Database;

namespace Watcher.Infrastructure.Repositories;

public class RuleRepository : IRuleRepository
{
    private readonly DatabaseContext _context;

    public RuleRepository(DatabaseContext context)
    {
        _context = context;
        EnsureDefaultsAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureDefaultsAsync()
    {
        var existingCategories = await GetCategoriesAsync();
        if (existingCategories.Count == 0)
        {
            var defaultCategories = DefaultCategories.GetDefaults();
            foreach (var cat in defaultCategories)
            {
                await AddCategoryAsync(cat);
            }

            var defaultRules = DefaultCategories.GetDefaultRules();
            foreach (var rule in defaultRules)
            {
                await AddAsync(rule);
            }
        }
    }

    public async Task<List<OrganizationRule>> GetAllAsync()
    {
        var list = new List<OrganizationRule>();
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, CategoryId, CategoryName, ExtensionsJson, TargetFolder, Priority, Enabled, IsCustom FROM OrganizationRules ORDER BY Priority ASC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new OrganizationRule
            {
                Id = reader.GetString(0),
                CategoryId = reader.GetString(1),
                CategoryName = reader.GetString(2),
                Extensions = JsonSerializer.Deserialize<List<string>>(reader.GetString(3)) ?? new(),
                TargetFolder = reader.GetString(4),
                Priority = reader.GetInt32(5),
                Enabled = reader.GetInt32(6) == 1,
                IsCustom = reader.GetInt32(7) == 1
            });
        }

        return list;
    }

    public async Task<OrganizationRule?> GetByIdAsync(string id)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, CategoryId, CategoryName, ExtensionsJson, TargetFolder, Priority, Enabled, IsCustom FROM OrganizationRules WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new OrganizationRule
            {
                Id = reader.GetString(0),
                CategoryId = reader.GetString(1),
                CategoryName = reader.GetString(2),
                Extensions = JsonSerializer.Deserialize<List<string>>(reader.GetString(3)) ?? new(),
                TargetFolder = reader.GetString(4),
                Priority = reader.GetInt32(5),
                Enabled = reader.GetInt32(6) == 1,
                IsCustom = reader.GetInt32(7) == 1
            };
        }

        return null;
    }

    public async Task<OrganizationRule> AddAsync(OrganizationRule rule)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO OrganizationRules (Id, CategoryId, CategoryName, ExtensionsJson, TargetFolder, Priority, Enabled, IsCustom)
            VALUES (@Id, @CategoryId, @CategoryName, @ExtensionsJson, @TargetFolder, @Priority, @Enabled, @IsCustom);
        ";
        cmd.Parameters.AddWithValue("@Id", rule.Id);
        cmd.Parameters.AddWithValue("@CategoryId", rule.CategoryId);
        cmd.Parameters.AddWithValue("@CategoryName", rule.CategoryName);
        cmd.Parameters.AddWithValue("@ExtensionsJson", JsonSerializer.Serialize(rule.Extensions));
        cmd.Parameters.AddWithValue("@TargetFolder", rule.TargetFolder);
        cmd.Parameters.AddWithValue("@Priority", rule.Priority);
        cmd.Parameters.AddWithValue("@Enabled", rule.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@IsCustom", rule.IsCustom ? 1 : 0);

        await cmd.ExecuteNonQueryAsync();
        return rule;
    }

    public async Task<OrganizationRule> UpdateAsync(OrganizationRule rule)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE OrganizationRules
            SET CategoryId = @CategoryId, CategoryName = @CategoryName, ExtensionsJson = @ExtensionsJson,
                TargetFolder = @TargetFolder, Priority = @Priority, Enabled = @Enabled, IsCustom = @IsCustom
            WHERE Id = @Id;
        ";
        cmd.Parameters.AddWithValue("@Id", rule.Id);
        cmd.Parameters.AddWithValue("@CategoryId", rule.CategoryId);
        cmd.Parameters.AddWithValue("@CategoryName", rule.CategoryName);
        cmd.Parameters.AddWithValue("@ExtensionsJson", JsonSerializer.Serialize(rule.Extensions));
        cmd.Parameters.AddWithValue("@TargetFolder", rule.TargetFolder);
        cmd.Parameters.AddWithValue("@Priority", rule.Priority);
        cmd.Parameters.AddWithValue("@Enabled", rule.Enabled ? 1 : 0);
        cmd.Parameters.AddWithValue("@IsCustom", rule.IsCustom ? 1 : 0);

        await cmd.ExecuteNonQueryAsync();
        return rule;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM OrganizationRules WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task SaveAllAsync(IEnumerable<OrganizationRule> rules)
    {
        using var conn = _context.CreateConnection();
        using var tx = conn.BeginTransaction();

        using var clearCmd = conn.CreateCommand();
        clearCmd.Transaction = tx;
        clearCmd.CommandText = "DELETE FROM OrganizationRules;";
        await clearCmd.ExecuteNonQueryAsync();

        foreach (var rule in rules)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO OrganizationRules (Id, CategoryId, CategoryName, ExtensionsJson, TargetFolder, Priority, Enabled, IsCustom)
                VALUES (@Id, @CategoryId, @CategoryName, @ExtensionsJson, @TargetFolder, @Priority, @Enabled, @IsCustom);
            ";
            cmd.Parameters.AddWithValue("@Id", rule.Id);
            cmd.Parameters.AddWithValue("@CategoryId", rule.CategoryId);
            cmd.Parameters.AddWithValue("@CategoryName", rule.CategoryName);
            cmd.Parameters.AddWithValue("@ExtensionsJson", JsonSerializer.Serialize(rule.Extensions));
            cmd.Parameters.AddWithValue("@TargetFolder", rule.TargetFolder);
            cmd.Parameters.AddWithValue("@Priority", rule.Priority);
            cmd.Parameters.AddWithValue("@Enabled", rule.Enabled ? 1 : 0);
            cmd.Parameters.AddWithValue("@IsCustom", rule.IsCustom ? 1 : 0);

            await cmd.ExecuteNonQueryAsync();
        }

        tx.Commit();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var list = new List<Category>();
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, DefaultFolder, ExtensionsJson, Color, IconName, IsSystem FROM Categories ORDER BY Name ASC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Category
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                DefaultFolder = reader.GetString(2),
                Extensions = JsonSerializer.Deserialize<List<string>>(reader.GetString(3)) ?? new(),
                Color = reader.GetString(4),
                IconName = reader.GetString(5),
                IsSystem = reader.GetInt32(6) == 1
            });
        }

        return list;
    }

    public async Task<Category?> GetCategoryByIdAsync(string id)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, DefaultFolder, ExtensionsJson, Color, IconName, IsSystem FROM Categories WHERE Id = @Id;";
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Category
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                DefaultFolder = reader.GetString(2),
                Extensions = JsonSerializer.Deserialize<List<string>>(reader.GetString(3)) ?? new(),
                Color = reader.GetString(4),
                IconName = reader.GetString(5),
                IsSystem = reader.GetInt32(6) == 1
            };
        }

        return null;
    }

    public async Task<Category> AddCategoryAsync(Category category)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Categories (Id, Name, DefaultFolder, ExtensionsJson, Color, IconName, IsSystem)
            VALUES (@Id, @Name, @DefaultFolder, @ExtensionsJson, @Color, @IconName, @IsSystem);
        ";
        cmd.Parameters.AddWithValue("@Id", category.Id);
        cmd.Parameters.AddWithValue("@Name", category.Name);
        cmd.Parameters.AddWithValue("@DefaultFolder", category.DefaultFolder);
        cmd.Parameters.AddWithValue("@ExtensionsJson", JsonSerializer.Serialize(category.Extensions));
        cmd.Parameters.AddWithValue("@Color", category.Color);
        cmd.Parameters.AddWithValue("@IconName", category.IconName);
        cmd.Parameters.AddWithValue("@IsSystem", category.IsSystem ? 1 : 0);

        await cmd.ExecuteNonQueryAsync();
        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Categories
            SET Name = @Name, DefaultFolder = @DefaultFolder, ExtensionsJson = @ExtensionsJson,
                Color = @Color, IconName = @IconName, IsSystem = @IsSystem
            WHERE Id = @Id;
        ";
        cmd.Parameters.AddWithValue("@Id", category.Id);
        cmd.Parameters.AddWithValue("@Name", category.Name);
        cmd.Parameters.AddWithValue("@DefaultFolder", category.DefaultFolder);
        cmd.Parameters.AddWithValue("@ExtensionsJson", JsonSerializer.Serialize(category.Extensions));
        cmd.Parameters.AddWithValue("@Color", category.Color);
        cmd.Parameters.AddWithValue("@IconName", category.IconName);
        cmd.Parameters.AddWithValue("@IsSystem", category.IsSystem ? 1 : 0);

        await cmd.ExecuteNonQueryAsync();
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(string id)
    {
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Categories WHERE Id = @Id AND IsSystem = 0;";
        cmd.Parameters.AddWithValue("@Id", id);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }
}

