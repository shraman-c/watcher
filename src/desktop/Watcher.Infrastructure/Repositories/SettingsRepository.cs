using System.Text.Json;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;
using Watcher.Infrastructure.Database;

namespace Watcher.Infrastructure.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly DatabaseContext _context;
    private const string SettingsKey = "AppSettings";
    private ApplicationSettings? _cachedSettings;

    public SettingsRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<ApplicationSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null) return _cachedSettings;

        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Value FROM ApplicationSettings WHERE Key = @Key;";
        cmd.Parameters.AddWithValue("@Key", SettingsKey);

        var result = await cmd.ExecuteScalarAsync();
        if (result != null && result != DBNull.Value)
        {
            try
            {
                var json = result.ToString();
                if (!string.IsNullOrEmpty(json))
                {
                    _cachedSettings = JsonSerializer.Deserialize<ApplicationSettings>(json);
                    if (_cachedSettings != null) return _cachedSettings;
                }
            }
            catch { }
        }

        _cachedSettings = new ApplicationSettings();
        await SaveSettingsAsync(_cachedSettings);
        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(ApplicationSettings settings)
    {
        _cachedSettings = settings;
        using var conn = _context.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ApplicationSettings (Key, Value)
            VALUES (@Key, @Value)
            ON CONFLICT(Key) DO UPDATE SET Value = @Value;
        ";
        cmd.Parameters.AddWithValue("@Key", SettingsKey);
        cmd.Parameters.AddWithValue("@Value", JsonSerializer.Serialize(settings));

        await cmd.ExecuteNonQueryAsync();
    }
}

