import React from 'react';
import {
  LayoutDashboard,
  FolderSync,
  Sliders,
  History,
  BarChart3,
  Settings,
  Info,
  ChevronLeft,
  ChevronRight,
  ShieldCheck,
  Eye,
  LucideIcon,
} from 'lucide-react';
import { useAppStore, NavigationPage } from '../stores/useAppStore';
import { useMonitoringStore } from '../stores/useMonitoringStore';

interface NavItem {
  id: NavigationPage;
  label: string;
  icon: LucideIcon;
}

const navItems: NavItem[] = [
  { id: 'overview', label: 'Overview', icon: LayoutDashboard },
  { id: 'folders', label: 'Watched Folders', icon: FolderSync },
  { id: 'rules', label: 'Organization Rules', icon: Sliders },
  { id: 'activity', label: 'Activity History', icon: History },
  { id: 'statistics', label: 'Statistics', icon: BarChart3 },
  { id: 'settings', label: 'Settings', icon: Settings },
  { id: 'about', label: 'About', icon: Info },
];

export const Sidebar: React.FC = () => {
  const { currentPage, setCurrentPage, isSidebarCollapsed, toggleSidebar } = useAppStore();
  const { isMonitoringActive, folders } = useMonitoringStore();

  const activeFoldersCount = folders.filter((f) => f.enabled).length;

  return (
    <aside
      className={`h-screen bg-[#181B22] border-r border-[#2B313E] flex flex-col transition-all duration-300 relative select-none z-20 ${
        isSidebarCollapsed ? 'w-20' : 'w-64'
      }`}
    >
      {/* Brand Header */}
      <div className="h-16 flex items-center justify-between px-4 border-b border-[#2B313E]/60">
        <div className="flex items-center gap-3 overflow-hidden">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-[#6C9BFF] to-[#3B66FF] flex items-center justify-center shadow-[0_4px_14px_rgba(108,155,255,0.35)] flex-shrink-0">
            <Eye size={22} className="text-white" />
          </div>
          {!isSidebarCollapsed && (
            <div className="flex flex-col min-w-0">
              <div className="flex items-center gap-2">
                <span className="font-bold text-base text-[#F3F5F8] tracking-tight">Watcher</span>
                <span className="text-[10px] font-semibold px-1.5 py-0.2 bg-[#6C9BFF]/20 text-[#6C9BFF] rounded">
                  2.0
                </span>
              </div>
              <span className="text-xs text-[#A0A8B5] truncate">Smart File Organizer</span>
            </div>
          )}
        </div>

        <button
          onClick={toggleSidebar}
          className="w-7 h-7 rounded-lg flex items-center justify-center text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#20242D] transition-colors"
          title={isSidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
        >
          {isSidebarCollapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
        </button>
      </div>

      {/* Nav Items */}
      <nav className="flex-1 py-4 px-3 space-y-1.5 overflow-y-auto">
        {navItems.map((item) => {
          const Icon = item.icon;
          const isActive = currentPage === item.id;

          return (
            <button
              key={item.id}
              onClick={() => setCurrentPage(item.id)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-150 ${
                isActive
                  ? 'bg-[#6C9BFF] text-white shadow-[0_4px_16px_rgba(108,155,255,0.3)] font-semibold'
                  : 'text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#20242D]'
              }`}
              title={isSidebarCollapsed ? item.label : undefined}
            >
              <Icon size={19} className={`flex-shrink-0 ${isActive ? 'text-white' : 'text-[#A0A8B5]'}`} />
              {!isSidebarCollapsed && <span className="truncate">{item.label}</span>}
            </button>
          );
        })}
      </nav>

      {/* Monitoring Status Badge / Widget at Bottom */}
      <div className="p-3 border-t border-[#2B313E]/60 bg-[#14171E]/60">
        {!isSidebarCollapsed ? (
          <div className="bg-[#20242D] border border-[#2B313E] rounded-xl p-3">
            <div className="flex items-center justify-between mb-1.5">
              <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Engine</span>
              <div className="flex items-center gap-1.5">
                <span
                  className={`w-2 h-2 rounded-full ${
                    isMonitoringActive ? 'bg-[#35B77A] shadow-[0_0_8px_#35B77A]' : 'bg-[#E9AE54]'
                  }`}
                />
                <span className="text-xs font-medium text-[#F3F5F8]">
                  {isMonitoringActive ? 'Active' : 'Paused'}
                </span>
              </div>
            </div>
            <p className="text-[11px] text-[#A0A8B5] leading-tight">
              {isMonitoringActive
                ? `Monitoring ${activeFoldersCount} watched folder(s)`
                : 'Background monitoring is stopped'}
            </p>
          </div>
        ) : (
          <div className="flex justify-center" title={isMonitoringActive ? 'Monitoring Active' : 'Monitoring Paused'}>
            <div
              className={`w-3 h-3 rounded-full ${
                isMonitoringActive ? 'bg-[#35B77A] shadow-[0_0_8px_#35B77A]' : 'bg-[#E9AE54]'
              }`}
            />
          </div>
        )}
      </div>
    </aside>
  );
};
