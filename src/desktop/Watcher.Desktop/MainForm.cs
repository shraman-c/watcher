using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Watcher.Core.Interfaces;
using Watcher.Core.Models;
using Watcher.Desktop.Services;
using Watcher.Infrastructure.Windows;

namespace Watcher.Desktop;

public partial class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainForm> _logger;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IFileMonitoringService _monitoringService;
    private readonly BridgeService _bridgeService;
    private readonly WindowsNotificationService _notificationService;

    private WebView2 _webView = null!;
    private NotifyIcon _trayIcon = null!;
    private ContextMenuStrip _trayMenu = null!;
    private bool _isRealClose = false;

    // Windows DWM Dark Mode APIs
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public MainForm(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<MainForm>>();
        _settingsRepository = serviceProvider.GetRequiredService<ISettingsRepository>();
        _monitoringService = serviceProvider.GetRequiredService<IFileMonitoringService>();
        _bridgeService = serviceProvider.GetRequiredService<BridgeService>();
        _notificationService = (WindowsNotificationService)serviceProvider.GetRequiredService<IWindowsNotificationService>();

        InitializeComponent();
        ApplyDarkTitleBar();
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();

        this.Text = "Watcher";
        this.Size = new Size(1280, 840);
        this.MinimumSize = new Size(960, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(17, 19, 24); // #111318

        // Icon
        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "app.ico");
        if (File.Exists(iconPath))
        {
            this.Icon = new Icon(iconPath);
        }

        // Initialize WebView2
        _webView = new WebView2
        {
            Dock = DockStyle.Fill
        };
        this.Controls.Add(_webView);

        // System Tray Icon & Menu
        _trayMenu = new ContextMenuStrip();
        var openItem = new ToolStripMenuItem("Open Watcher", null, (s, e) => RestoreWindow());
        var startItem = new ToolStripMenuItem("Start Monitoring", null, async (s, e) => await _monitoringService.StartAllAsync());
        var pauseItem = new ToolStripMenuItem("Pause Monitoring", null, async (s, e) => await _monitoringService.PauseAsync());
        var resumeItem = new ToolStripMenuItem("Resume Monitoring", null, async (s, e) => await _monitoringService.ResumeAsync());
        var exitItem = new ToolStripMenuItem("Exit", null, (s, e) => ExitApplication());

        _trayMenu.Items.AddRange(new ToolStripItem[] {
            openItem,
            new ToolStripSeparator(),
            startItem,
            pauseItem,
            resumeItem,
            new ToolStripSeparator(),
            exitItem
        });

        _trayIcon = new NotifyIcon
        {
            Text = "Watcher - Automated File Organizer",
            Icon = this.Icon ?? SystemIcons.Application,
            ContextMenuStrip = _trayMenu,
            Visible = true
        };
        _trayIcon.DoubleClick += (s, e) => RestoreWindow();
        _notificationService.RegisterNotifyIcon(_trayIcon);

        this.FormClosing += OnFormClosing;
        this.Resize += OnFormResize;

        this.ResumeLayout(false);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await InitializeWebViewAsync();

        // Check if start monitoring on launch is enabled
        var settings = await _settingsRepository.GetSettingsAsync();
        if (settings.StartMonitoringOnLaunch)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                await _monitoringService.StartAllAsync();
            });
        }
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var userDataFolder = Path.Combine(localAppData, "Watcher", "WebView2Data");
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

            await _webView.EnsureCoreWebView2Async(environment);

            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            _webView.CoreWebView2.Settings.IsZoomControlEnabled = false;

            // Register bridge
            _bridgeService.RegisterWebView(_webView);

            // Determine if frontend production dist exists or use dev server
            var distFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frontend");
            if (!Directory.Exists(distFolder))
            {
                distFolder = Path.Combine(Directory.GetCurrentDirectory(), "src", "frontend", "dist");
            }

            if (Directory.Exists(distFolder) && File.Exists(Path.Combine(distFolder, "index.html")))
            {
                _logger.LogInformation("Serving frontend from: '{Dist}'", distFolder);
                _webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app.local",
                    distFolder,
                    CoreWebView2HostResourceAccessKind.Allow);

                _webView.CoreWebView2.Navigate("https://app.local/index.html");
            }
            else
            {
                // Fallback to Vite Dev Server during development
                _logger.LogInformation("Dist folder not found. Navigating to Vite dev server at http://localhost:5173");
                _webView.CoreWebView2.Navigate("http://localhost:5173");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize WebView2");
            MessageBox.Show($"Failed to initialize WebView2:\n{ex.Message}", "Watcher Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void RestoreWindow()
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.WindowState = FormWindowState.Normal;
        }
        this.Show();
        this.BringToFront();
        this.Activate();
    }

    private void ExitApplication()
    {
        _isRealClose = true;
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    private async void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_isRealClose)
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            if (settings.CloseToTray)
            {
                e.Cancel = true;
                this.Hide();
                _notificationService.ShowNotification("Watcher Running in Background", "Watcher is continuing to monitor files in your system tray.");
                return;
            }
        }

        await _monitoringService.StopAllAsync();
    }

    private async void OnFormResize(object? sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            var settings = await _settingsRepository.GetSettingsAsync();
            if (settings.MinimizeToTray)
            {
                this.Hide();
            }
        }
    }

    private void ApplyDarkTitleBar()
    {
        try
        {
            int darkMode = 1;
            DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
        }
        catch { }
    }
}

