import React, { useState, useEffect } from 'react';
import {
  Save,
  RotateCcw,
  Sliders,
  Bell,
  Sun,
  ShieldCheck,
  HardDrive,
  Check,
} from 'lucide-react';
import { useMonitoringStore } from '../stores/useMonitoringStore';
import { useAppStore } from '../stores/useAppStore';
import { ApplicationSettings, CollisionBehavior, AppTheme } from '../../../shared/contracts/contracts';

export const SettingsPage: React.FC = () => {
  const { settings, saveSettings } = useMonitoringStore();
  const { setTheme, showToast } = useAppStore();

  const [localSettings, setLocalSettings] = useState<ApplicationSettings | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (settings) {
      setLocalSettings({ ...settings });
    }
  }, [settings]);

  if (!localSettings) {
    return (
      <div className="p-6 text-center text-xs text-[#A0A8B5]">
        Loading application settings...
      </div>
    );
  }

  const handleSave = async () => {
    setIsSaving(true);
    const ok = await saveSettings(localSettings);
    setIsSaving(false);
    if (ok) {
      setTheme(localSettings.theme);
      showToast('Settings saved successfully', 'success');
    } else {
      showToast('Failed to save settings', 'error');
    }
  };

  const handleReset = () => {
    const defaultSettings: ApplicationSettings = {
      launchOnStartup: false,
      minimizeToTray: true,
      closeToTray: true,
      startMonitoringOnLaunch: true,
      theme: 'dark',
      compactMode: false,
      notifyOnSuccess: true,
      notifyOnError: true,
      notifyOnMonitoringStatus: true,
      collisionBehavior: 'rename',
      defaultUnknownTarget: 'Other',
      readinessTimeoutMs: 15000,
      readinessPollIntervalMs: 400,
      recursiveDefault: false,
      logLevel: 'INFO',
      autoCleanupDatabaseDays: 30,
    };
    setLocalSettings(defaultSettings);
    showToast('Reset settings to defaults (click Save to persist)', 'info');
  };

  return (
    <div className="p-6 space-y-6 max-w-4xl mx-auto">
      {/* Top Banner */}
      <div className="flex items-center justify-between bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card">
        <div>
          <h2 className="text-base font-bold text-[#F3F5F8]">Application Preferences</h2>
          <p className="text-xs text-[#A0A8B5] mt-0.5">
            Configure background engine lifecycle, notification events, and file organization behavior.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={handleReset}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold bg-[#272C36] text-[#A0A8B5] hover:text-[#F3F5F8] border border-[#2B313E] transition-colors"
          >
            <RotateCcw size={13} />
            <span>Defaults</span>
          </button>
          <button
            onClick={handleSave}
            disabled={isSaving}
            className="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-semibold bg-[#6C9BFF] text-white hover:bg-[#5287F6] transition-all shadow-subtle"
          >
            <Save size={14} />
            <span>{isSaving ? 'Saving...' : 'Save Settings'}</span>
          </button>
        </div>
      </div>

      {/* General Section */}
      <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card space-y-4">
        <h3 className="text-xs font-semibold text-[#6C9BFF] uppercase tracking-wider flex items-center gap-2">
          <ShieldCheck size={15} />
          <span>General & Windows Lifecycle</span>
        </h3>

        <div className="space-y-3">
          <label className="flex items-center justify-between p-3 rounded-xl bg-[#181B22] border border-[#2B313E] cursor-pointer">
            <div>
              <span className="text-xs font-semibold text-[#F3F5F8] block">Launch on Windows Startup</span>
              <span className="text-[11px] text-[#A0A8B5]">
                Start Watcher silently in background tray when Windows logs in
              </span>
            </div>
            <input
              type="checkbox"
              checked={localSettings.launchOnStartup}
              onChange={(e) => setLocalSettings({ ...localSettings, launchOnStartup: e.target.checked })}
              className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#20242D] w-4 h-4"
            />
          </label>

          <label className="flex items-center justify-between p-3 rounded-xl bg-[#181B22] border border-[#2B313E] cursor-pointer">
            <div>
              <span className="text-xs font-semibold text-[#F3F5F8] block">Close to System Tray</span>
              <span className="text-[11px] text-[#A0A8B5]">
                Keep monitoring files in background when closing window instead of quitting
              </span>
            </div>
            <input
              type="checkbox"
              checked={localSettings.closeToTray}
              onChange={(e) => setLocalSettings({ ...localSettings, closeToTray: e.target.checked })}
              className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#20242D] w-4 h-4"
            />
          </label>

          <label className="flex items-center justify-between p-3 rounded-xl bg-[#181B22] border border-[#2B313E] cursor-pointer">
            <div>
              <span className="text-xs font-semibold text-[#F3F5F8] block">Minimize to Tray</span>
              <span className="text-[11px] text-[#A0A8B5]">
                Hide taskbar item when window is minimized
              </span>
            </div>
            <input
              type="checkbox"
              checked={localSettings.minimizeToTray}
              onChange={(e) => setLocalSettings({ ...localSettings, minimizeToTray: e.target.checked })}
              className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#20242D] w-4 h-4"
            />
          </label>

          <label className="flex items-center justify-between p-3 rounded-xl bg-[#181B22] border border-[#2B313E] cursor-pointer">
            <div>
              <span className="text-xs font-semibold text-[#F3F5F8] block">Start Monitoring Automatically</span>
              <span className="text-[11px] text-[#A0A8B5]">
                Resume watching enabled folders immediately upon application launch
              </span>
            </div>
            <input
              type="checkbox"
              checked={localSettings.startMonitoringOnLaunch}
              onChange={(e) => setLocalSettings({ ...localSettings, startMonitoringOnLaunch: e.target.checked })}
              className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#20242D] w-4 h-4"
            />
          </label>
        </div>
      </div>

      {/* Notifications Section */}
      <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card space-y-4">
        <h3 className="text-xs font-semibold text-[#35B77A] uppercase tracking-wider flex items-center gap-2">
          <Bell size={15} />
          <span>Desktop Notifications</span>
        </h3>

        <div className="space-y-3">
          <label className="flex items-center justify-between p-3 rounded-xl bg-[#181B22] border border-[#2B313E] cursor-pointer">
            <div>
              <span className="text-xs font-semibold text-[#F3F5F8] block">Notify on File Organized</span>
              <span className="text-[11px] text-[#A0A8B5]">
                Show Windows notification balloon when a new file is sorted
              </span>
            </div>
            <input
              type="checkbox"
              checked={localSettings.notifyOnSuccess}
              onChange={(e) => setLocalSettings({ ...localSettings, notifyOnSuccess: e.target.checked })}
              className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#20242D] w-4 h-4"
            />
          </label>

          <label className="flex items-center justify-between p-3 rounded-xl bg-[#181B22] border border-[#2B313E] cursor-pointer">
            <div>
              <span className="text-xs font-semibold text-[#F3F5F8] block">Notify on File Lock / Error</span>
              <span className="text-[11px] text-[#A0A8B5]">
                Show alert if a drive is disconnected or permission is denied
              </span>
            </div>
            <input
              type="checkbox"
              checked={localSettings.notifyOnError}
              onChange={(e) => setLocalSettings({ ...localSettings, notifyOnError: e.target.checked })}
              className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#20242D] w-4 h-4"
            />
          </label>
        </div>
      </div>

      {/* Organization Rules & Collision */}
      <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card space-y-4">
        <h3 className="text-xs font-semibold text-[#E9AE54] uppercase tracking-wider flex items-center gap-2">
          <Sliders size={15} />
          <span>File Safety & Collision Rules</span>
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="text-xs font-semibold text-[#A0A8B5] block mb-1.5">
              File Name Collision Strategy
            </label>
            <select
              value={localSettings.collisionBehavior}
              onChange={(e) =>
                setLocalSettings({ ...localSettings, collisionBehavior: e.target.value as CollisionBehavior })
              }
              className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl px-3 py-2 text-xs text-[#F3F5F8] focus:outline-none focus:border-[#6C9BFF]"
            >
              <option value="rename">Rename (append numbered suffix: file (1).ext)</option>
              <option value="skip">Skip (leave existing file in place untouched)</option>
              <option value="replace">Replace (overwrite destination file)</option>
            </select>
            <p className="text-[11px] text-[#A0A8B5] mt-1">
              Determines what happens when a file with identical name already exists in target folder.
            </p>
          </div>

          <div>
            <label className="text-xs font-semibold text-[#A0A8B5] block mb-1.5">
              Readiness Timeout (milliseconds)
            </label>
            <input
              type="number"
              value={localSettings.readinessTimeoutMs}
              onChange={(e) =>
                setLocalSettings({ ...localSettings, readinessTimeoutMs: parseInt(e.target.value) || 15000 })
              }
              step="1000"
              min="2000"
              max="60000"
              className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl px-3 py-2 text-xs text-[#F3F5F8] focus:outline-none focus:border-[#6C9BFF]"
            />
            <p className="text-[11px] text-[#A0A8B5] mt-1">
              Duration to wait for ongoing downloads/writes to stabilize before moving (default: 15,000ms).
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

