using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.WinForms;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;
using Watcher.Infrastructure.Windows;

namespace Watcher.Desktop.Services;

public class BridgeService
{
    private readonly IFileMonitoringService _monitoringService;
    private readonly IFileOrganizationService _organizationService;
    private readonly IFolderRepository _folderRepository;
    private readonly IRuleRepository _ruleRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IRuleEngine _ruleEngine;
    private readonly IWindowsNotificationService _notificationService;
    private readonly ILogger<BridgeService> _logger;

    private WebView2? _webView;
    private readonly SynchronizationContext? _uiContext;

    public BridgeService(
        IFileMonitoringService monitoringService,
        IFileOrganizationService organizationService,
        IFolderRepository folderRepository,
        IRuleRepository ruleRepository,
        IActivityRepository activityRepository,
        ISettingsRepository settingsRepository,
        IRuleEngine ruleEngine,
        IWindowsNotificationService notificationService,
        ILogger<BridgeService> logger)
    {
        _monitoringService = monitoringService;
        _organizationService = organizationService;
        _folderRepository = folderRepository;
        _ruleRepository = ruleRepository;
        _activityRepository = activityRepository;
        _settingsRepository = settingsRepository;
        _ruleEngine = ruleEngine;
        _notificationService = notificationService;
        _logger = logger;
        _uiContext = SynchronizationContext.Current;

        // Wire backend events to frontend notifications
        _organizationService.FileOrganized += OnBackendFileOrganized;
        _monitoringService.GlobalMonitoringChanged += OnGlobalMonitoringChanged;
        _monitoringService.FolderStatusChanged += OnFolderStatusChanged;
    }

    public void RegisterWebView(WebView2 webView)
    {
        _webView = webView;
        _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
    }

    private async void OnWebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
    {
        string rawMessage = e.TryGetWebMessageAsString();
        if (string.IsNullOrWhiteSpace(rawMessage)) return;

        try
        {
            using var doc = JsonDocument.Parse(rawMessage);
            var root = doc.RootElement;

            string requestId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
            string action = root.TryGetProperty("action", out var actionProp) ? actionProp.GetString() ?? "" : "";
            JsonElement payload = root.TryGetProperty("payload", out var payloadProp) ? payloadProp : default;

            var response = await HandleActionAsync(action, payload);
            SendResponse(requestId, response.Success, response.Data, response.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling bridge web message: {Msg}", rawMessage);
        }
    }

    private async Task<(bool Success, object? Data, string? Error)> HandleActionAsync(string action, JsonElement payload)
    {
        try
        {
            switch (action)
            {
                case "watcher.getStatus":
                    return (true, new
                    {
                        isRunning = _monitoringService.IsRunning,
                        folderStatuses = _monitoringService.FolderStatuses
                    }, null);

                case "watcher.getFolders":
                    var folders = await _folderRepository.GetAllAsync();
                    return (true, folders, null);

                case "watcher.addFolder":
                    var newFolder = JsonSerializer.Deserialize<WatchedFolder>(payload.GetRawText(), JsonOptions);
                    if (newFolder == null || string.IsNullOrWhiteSpace(newFolder.Path))
                        return (false, null, "Folder path is required.");

                    var normPath = Path.GetFullPath(newFolder.Path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (!Directory.Exists(normPath))
                        return (false, null, $"Directory '{normPath}' does not exist.");

                    var existing = await _folderRepository.GetByPathAsync(normPath);
                    if (existing != null)
                        return (false, null, "Folder is already being watched.");

                    newFolder.Path = normPath;
                    newFolder.Status = FolderStatus.Active;
                    var added = await _folderRepository.AddAsync(newFolder);
                    await _monitoringService.ReloadFoldersAsync();
                    return (true, added, null);

                case "watcher.updateFolder":
                    var updateFolder = JsonSerializer.Deserialize<WatchedFolder>(payload.GetRawText(), JsonOptions);
                    if (updateFolder == null) return (false, null, "Invalid folder payload.");
                    var updated = await _folderRepository.UpdateAsync(updateFolder);
                    await _monitoringService.ReloadFoldersAsync();
                    return (true, updated, null);

                case "watcher.removeFolder":
                    string removeId = payload.GetString() ?? (payload.TryGetProperty("id", out var rId) ? rId.GetString() ?? "" : "");
                    if (string.IsNullOrEmpty(removeId)) return (false, null, "Folder ID is required.");
                    await _monitoringService.StopFolderAsync(removeId);
                    bool deleted = await _folderRepository.DeleteAsync(removeId);
                    return (true, deleted, null);

                case "watcher.openNativeFolderPicker":
                    string? selectedPath = null;
                    if (_uiContext != null)
                    {
                        var tcs = new TaskCompletionSource<string?>();
                        _uiContext.Post(_ =>
                        {
                            using var dialog = new FolderBrowserDialog
                            {
                                Description = "Select folder to monitor with Watcher",
                                UseDescriptionForTitle = true,
                                ShowNewFolderButton = true
                            };
                            if (dialog.ShowDialog() == DialogResult.OK)
                            {
                                tcs.SetResult(dialog.SelectedPath);
                            }
                            else
                            {
                                tcs.SetResult(null);
                            }
                        }, null);
                        selectedPath = await tcs.Task;
                    }
                    return (true, new { path = selectedPath }, null);

                case "watcher.startMonitoring":
                    await _monitoringService.StartAllAsync();
                    return (true, true, null);

                case "watcher.stopMonitoring":
                    await _monitoringService.StopAllAsync();
                    return (true, true, null);

                case "watcher.pauseMonitoring":
                    await _monitoringService.PauseAsync();
                    return (true, true, null);

                case "watcher.resumeMonitoring":
                    await _monitoringService.ResumeAsync();
                    return (true, true, null);

                case "watcher.startFolder":
                    string sId = payload.GetString() ?? "";
                    await _monitoringService.StartFolderAsync(sId);
                    return (true, true, null);

                case "watcher.stopFolder":
                    string pId = payload.GetString() ?? "";
                    await _monitoringService.StopFolderAsync(pId);
                    return (true, true, null);

                case "watcher.getRules":
                    var rules = await _ruleRepository.GetAllAsync();
                    return (true, rules, null);

                case "watcher.saveRule":
                    var ruleToSave = JsonSerializer.Deserialize<OrganizationRule>(payload.GetRawText(), JsonOptions);
                    if (ruleToSave == null) return (false, null, "Invalid rule payload.");
                    var existingRule = await _ruleRepository.GetByIdAsync(ruleToSave.Id);
                    if (existingRule == null)
                    {
                        await _ruleRepository.AddAsync(ruleToSave);
                    }
                    else
                    {
                        await _ruleRepository.UpdateAsync(ruleToSave);
                    }
                    var allRules = await _ruleRepository.GetAllAsync();
                    _ruleEngine.RefreshRules(allRules);
                    return (true, ruleToSave, null);

                case "watcher.deleteRule":
                    string ruleId = payload.GetString() ?? "";
                    bool delRule = await _ruleRepository.DeleteAsync(ruleId);
                    var refreshedRules = await _ruleRepository.GetAllAsync();
                    _ruleEngine.RefreshRules(refreshedRules);
                    return (true, delRule, null);

                case "watcher.resetRulesToDefault":
                    var defaultRules = Core.Rules.DefaultCategories.GetDefaultRules();
                    await _ruleRepository.SaveAllAsync(defaultRules);
                    _ruleEngine.RefreshRules(defaultRules);
                    return (true, defaultRules, null);

                case "watcher.getCategories":
                    var categories = await _ruleRepository.GetCategoriesAsync();
                    return (true, categories, null);

                case "watcher.saveCategory":
                    var catToSave = JsonSerializer.Deserialize<Category>(payload.GetRawText(), JsonOptions);
                    if (catToSave == null) return (false, null, "Invalid category.");
                    var existingCat = await _ruleRepository.GetCategoryByIdAsync(catToSave.Id);
                    if (existingCat == null)
                    {
                        await _ruleRepository.AddCategoryAsync(catToSave);
                    }
                    else
                    {
                        await _ruleRepository.UpdateCategoryAsync(catToSave);
                    }
                    return (true, catToSave, null);

                case "watcher.deleteCategory":
                    string catId = payload.GetString() ?? "";
                    bool delCat = await _ruleRepository.DeleteCategoryAsync(catId);
                    return (true, delCat, null);

                case "watcher.getActivity":
                    string? search = payload.TryGetProperty("search", out var sp) ? sp.GetString() : null;
                    string? category = payload.TryGetProperty("category", out var cp) ? cp.GetString() : null;
                    int page = payload.TryGetProperty("page", out var pgProp) ? pgProp.GetInt32() : 1;
                    int pageSize = payload.TryGetProperty("pageSize", out var pzProp) ? pzProp.GetInt32() : 20;

                    var items = await _activityRepository.QueryAsync(search, category, null, null, null, page, pageSize);
                    int total = await _activityRepository.GetTotalCountAsync(search, category, null, null, null);
                    return (true, new { items, total, page, pageSize }, null);

                case "watcher.clearActivity":
                    await _activityRepository.ClearAllAsync();
                    return (true, true, null);

                case "watcher.undoActivity":
                    string logId = payload.GetString() ?? "";
                    bool undone = await _organizationService.UndoOperationAsync(logId);
                    return (true, undone, null);

                case "watcher.openFileInExplorer":
                    string filePath = payload.GetString() ?? "";
                    if (File.Exists(filePath) || Directory.Exists(filePath))
                    {
                        Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                        return (true, true, null);
                    }
                    var dir = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    {
                        Process.Start("explorer.exe", $"\"{dir}\"");
                        return (true, true, null);
                    }
                    return (false, null, "File or directory does not exist.");

                case "watcher.getStatistics":
                    var stats = await _activityRepository.GetDashboardStatisticsAsync();
                    stats.IsMonitoringActive = _monitoringService.IsRunning;
                    return (true, stats, null);

                case "watcher.getSettings":
                    var settings = await _settingsRepository.GetSettingsAsync();
                    return (true, settings, null);

                case "watcher.saveSettings":
                    var newSettings = JsonSerializer.Deserialize<ApplicationSettings>(payload.GetRawText(), JsonOptions);
                    if (newSettings == null) return (false, null, "Invalid settings.");
                    await _settingsRepository.SaveSettingsAsync(newSettings);
                    WindowsStartupManager.SetLaunchOnStartup(newSettings.LaunchOnStartup);
                    return (true, newSettings, null);

                case "watcher.createSampleFiles":
                    string targetFolder = payload.GetString() ?? "";
                    if (!Directory.Exists(targetFolder))
                    {
                        var first = (await _folderRepository.GetAllAsync()).FirstOrDefault();
                        if (first != null && Directory.Exists(first.Path))
                        {
                            targetFolder = first.Path;
                        }
                    }

                    if (!Directory.Exists(targetFolder))
                        return (false, null, "No valid target folder found.");

                    var samples = new[]
                    {
                        "sample_report.pdf",
                        "family_photo.jpg",
                        "presentation.pptx",
                        "script.py",
                        "budget.xlsx",
                        "recording.mp3",
                        "archive_data.zip"
                    };

                    int created = 0;
                    foreach (var s in samples)
                    {
                        var p = Path.Combine(targetFolder, s);
                        if (!File.Exists(p))
                        {
                            await File.WriteAllTextAsync(p, $"Sample test content generated by Watcher at {DateTime.Now:O}");
                            created++;
                        }
                    }
                    return (true, new { created, folder = targetFolder }, null);

                default:
                    return (false, null, $"Unknown action: '{action}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing action '{Action}'", action);
            return (false, null, ex.Message);
        }
    }

    private void SendResponse(string requestId, bool success, object? data, string? error)
    {
        if (_webView == null || _webView.CoreWebView2 == null) return;

        var responseObj = new
        {
            id = requestId,
            success = success,
            data = data,
            error = error != null ? new { code = "ERROR", message = error } : null
        };

        var json = JsonSerializer.Serialize(responseObj, JsonOptions);
        _uiContext?.Post(_ =>
        {
            try { _webView?.CoreWebView2?.PostWebMessageAsJson(json); } catch { }
        }, null);
    }

    public void SendEvent(string eventName, object payload)
    {
        if (_webView == null || _webView.CoreWebView2 == null) return;

        var eventObj = new
        {
            @event = eventName,
            payload = payload,
            timestamp = DateTime.UtcNow.ToString("o")
        };

        var json = JsonSerializer.Serialize(eventObj, JsonOptions);
        _uiContext?.Post(_ =>
        {
            try { _webView?.CoreWebView2?.PostWebMessageAsJson(json); } catch { }
        }, null);
    }

    private void OnBackendFileOrganized(object? sender, FileOrganizedEventArgs e)
    {
        SendEvent("FileOrganized", e.LogItem);

        _settingsRepository.GetSettingsAsync().ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully && t.Result.NotifyOnSuccess)
            {
                _notificationService.ShowNotification(
                    "File Organized",
                    $"{e.LogItem.FileName} -> {e.LogItem.Category}");
            }
        });
    }

    private void OnGlobalMonitoringChanged(object? sender, bool isRunning)
    {
        SendEvent(isRunning ? "MonitoringStarted" : "MonitoringStopped", new { isRunning });

        _settingsRepository.GetSettingsAsync().ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully && t.Result.NotifyOnMonitoringStatus)
            {
                _notificationService.ShowNotification(
                    "Monitoring Status",
                    isRunning ? "Watcher is actively organizing files." : "Monitoring has stopped.");
            }
        });
    }

    private void OnFolderStatusChanged(object? sender, FolderStatusChangedEventArgs e)
    {
        SendEvent("FolderStatusChanged", e);
        if (e.Status == FolderStatus.Error || e.Status == FolderStatus.NotFound)
        {
            _notificationService.ShowNotification(
                "Watched Folder Issue",
                $"Folder '{e.FolderPath}': {e.ErrorMessage ?? "Unavailable"}",
                ToolTipIcon.Warning);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}

