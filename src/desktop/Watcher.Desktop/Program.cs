using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Watcher.Core.Interfaces;
using Watcher.Core.Monitoring;
using Watcher.Core.Organization;
using Watcher.Core.Rules;
using Watcher.Desktop.Services;
using Watcher.Infrastructure.Configuration;
using Watcher.Infrastructure.Database;
using Watcher.Infrastructure.Repositories;
using Watcher.Infrastructure.Windows;

namespace Watcher.Desktop;

static class Program
{
    private const string MutexName = @"Global\WatcherAppSingleInstanceMutex";
    private const int WM_SHOWME = 0x8001;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int RegisterWindowMessage(string lpString);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private static readonly IntPtr HWND_BROADCAST = new(0xffff);

    [STAThread]
    static void Main(string[] args)
    {
        using var mutex = new Mutex(true, MutexName, out bool isOnlyInstance);

        if (!isOnlyInstance)
        {
            // Another instance is already running; notify it to restore and bring to focus
            int msg = RegisterWindowMessage("WM_SHOWME_WATCHER");
            PostMessage(HWND_BROADCAST, msg, IntPtr.Zero, IntPtr.Zero);
            return;
        }

        ApplicationConfiguration.Initialize();

        var services = ConfigureServices();
        using var serviceProvider = services.BuildServiceProvider();

        // Run legacy configuration migration if needed
        var migrator = serviceProvider.GetRequiredService<ILegacyConfigMigrator>();
        migrator.MigrateIfNecessaryAsync().GetAwaiter().GetResult();

        var mainForm = new MainForm(serviceProvider);
        Application.Run(mainForm);
    }

    private static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Database & Repositories
        services.AddSingleton<DatabaseContext>();
        services.AddSingleton<IFolderRepository, FolderRepository>();
        services.AddSingleton<IRuleRepository, RuleRepository>();
        services.AddSingleton<IActivityRepository, ActivityRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();

        // Core Domain
        services.AddSingleton<IRuleEngine, RuleEngine>();
        services.AddSingleton<IFileReadinessDetector, FileReadinessDetector>();
        services.AddSingleton<IFileCollisionResolver, FileCollisionResolver>();
        services.AddSingleton<IInfiniteLoopGuard, InfiniteLoopGuard>();
        services.AddSingleton<IFileOrganizationService, FileOrganizationService>();
        services.AddSingleton<IFileMonitoringService, MonitoringManager>();

        // Infrastructure & Windows Integration
        services.AddSingleton<ILegacyConfigMigrator, LegacyConfigMigrator>();
        services.AddSingleton<IWindowsNotificationService, WindowsNotificationService>();

        // Desktop Bridge & Forms
        services.AddSingleton<BridgeService>();
        services.AddTransient<MainForm>();

        return services;
    }
}
