import React from 'react';
import {
  Play,
  Pause,
  Sun,
  Moon,
  FolderPlus,
  FlaskConical,
  Bell,
  RefreshCw,
} from 'lucide-react';
import { useAppStore } from '../stores/useAppStore';
import { useMonitoringStore } from '../stores/useMonitoringStore';

interface HeaderProps {
  onAddFolderClick?: () => void;
}

export const Header: React.FC<HeaderProps> = ({ onAddFolderClick }) => {
  const { currentPage, theme, setTheme, showToast } = useAppStore();
  const {
    isMonitoringActive,
    startMonitoring,
    pauseMonitoring,
    createSampleFiles,
    fetchInitialData,
    isLoading,
  } = useMonitoringStore();

  const pageTitles: Record<string, { title: string; subtitle: string }> = {
    overview: { title: 'Overview', subtitle: 'Real-time monitoring metrics and quick actions' },
    folders: { title: 'Watched Folders', subtitle: 'Manage monitored directories and their targets' },
    rules: { title: 'Organization Rules', subtitle: 'Configure file extensions and destination mappings' },
    activity: { title: 'Activity History', subtitle: 'Searchable log of all organized files and operations' },
    statistics: { title: 'Statistics & Analytics', subtitle: 'Visual insights on organization performance' },
    settings: { title: 'Settings', subtitle: 'Application behavior, notifications, and persistence' },
    about: { title: 'About Watcher', subtitle: 'Architecture, version, and developer details' },
  };

  const currentInfo = pageTitles[currentPage] || { title: 'Watcher', subtitle: '' };

  const handleToggleMonitoring = async () => {
    if (isMonitoringActive) {
      await pauseMonitoring();
      showToast('Monitoring paused', 'info');
    } else {
      await startMonitoring();
      showToast('Monitoring started', 'success');
    }
  };

  const handleGenerateSamples = async () => {
    const res = await createSampleFiles();
    if (res && res.created > 0) {
      showToast(`Created ${res.created} sample files in watched folder!`, 'success');
    } else {
      showToast('Please add or enable a watched folder first', 'error');
    }
  };

  return (
    <header className="h-16 border-b border-[#2B313E] bg-[#111318]/90 backdrop-blur-md px-6 flex items-center justify-between select-none z-10 sticky top-0">
      {/* Title & Subtitle */}
      <div>
        <h1 className="text-lg font-bold text-[#F3F5F8] tracking-tight">{currentInfo.title}</h1>
        <p className="text-xs text-[#A0A8B5] hidden sm:block">{currentInfo.subtitle}</p>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2.5">
        {/* Quick Sample Files Generator */}
        <button
          onClick={handleGenerateSamples}
          className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-[#20242D] text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] border border-[#2B313E] transition-all"
          title="Create test sample files in watched folder to verify rules"
        >
          <FlaskConical size={14} className="text-[#6C9BFF]" />
          <span className="hidden md:inline">Test Files</span>
        </button>

        {/* Add Folder Quick Action */}
        {onAddFolderClick && (
          <button
            onClick={onAddFolderClick}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-[#20242D] text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] border border-[#2B313E] transition-all"
          >
            <FolderPlus size={14} className="text-[#35B77A]" />
            <span className="hidden md:inline">Add Folder</span>
          </button>
        )}

        {/* Global Monitoring Play/Pause Button */}
        <button
          onClick={handleToggleMonitoring}
          className={`flex items-center gap-1.5 px-3.5 py-1.5 rounded-lg text-xs font-semibold transition-all shadow-subtle ${
            isMonitoringActive
              ? 'bg-[#E9AE54]/15 text-[#E9AE54] border border-[#E9AE54]/30 hover:bg-[#E9AE54]/25'
              : 'bg-[#35B77A] text-white hover:bg-[#2fa36d]'
          }`}
        >
          {isMonitoringActive ? (
            <>
              <Pause size={14} />
              <span>Pause</span>
            </>
          ) : (
            <>
              <Play size={14} />
              <span>Start</span>
            </>
          )}
        </button>

        <div className="h-4 w-[1px] bg-[#2B313E] mx-1" />

        {/* Refresh */}
        <button
          onClick={() => fetchInitialData()}
          disabled={isLoading}
          className="w-8 h-8 rounded-lg flex items-center justify-center text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#20242D] transition-colors"
          title="Refresh application data"
        >
          <RefreshCw size={15} className={isLoading ? 'animate-spin text-[#6C9BFF]' : ''} />
        </button>

        {/* Theme Toggle */}
        <button
          onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
          className="w-8 h-8 rounded-lg flex items-center justify-center text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#20242D] transition-colors"
          title={`Switch to ${theme === 'dark' ? 'Light' : 'Dark'} theme`}
        >
          {theme === 'dark' ? <Sun size={16} /> : <Moon size={16} />}
        </button>
      </div>
    </header>
  );
};

