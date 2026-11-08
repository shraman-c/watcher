using System.Windows.Forms;

namespace Watcher.Infrastructure.Windows;

public interface IWindowsNotificationService
{
    void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int timeoutMs = 3000);
}

public class WindowsNotificationService : IWindowsNotificationService
{
    private NotifyIcon? _notifyIcon;

    public void RegisterNotifyIcon(NotifyIcon notifyIcon)
    {
        _notifyIcon = notifyIcon;
    }

    public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int timeoutMs = 3000)
    {
        if (_notifyIcon != null && _notifyIcon.Visible)
        {
            _notifyIcon.ShowBalloonTip(timeoutMs, title, message, icon);
        }
    }
}

