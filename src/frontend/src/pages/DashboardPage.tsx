import React, { useEffect, useState } from 'react';
import {
  FolderSync,
  Files,
  CalendarCheck,
  Tag,
  CheckCircle2,
  AlertTriangle,
  Play,
  Pause,
  FolderPlus,
  Sliders,
  History,
  FlaskConical,
  ExternalLink,
  ArrowRight,
  TrendingUp,
} from 'lucide-react';
import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from 'recharts';
import { useMonitoringStore } from '../stores/useMonitoringStore';
import { useAppStore } from '../stores/useAppStore';
import { MetricCard } from '../components/MetricCard';
import { StatusBadge } from '../components/StatusBadge';
import { ActivityLogItem } from '../../../shared/contracts/contracts';
import { bridge } from '../services/bridgeClient';

interface DashboardPageProps {
  onOpenAddFolder: () => void;
}

export const DashboardPage: React.FC<DashboardPageProps> = ({ onOpenAddFolder }) => {
  const { setCurrentPage, showToast } = useAppStore();
  const {
    isMonitoringActive,
    folders,
    statistics,
    startMonitoring,
    pauseMonitoring,
    createSampleFiles,
  } = useMonitoringStore();

  const [recentLogs, setRecentLogs] = useState<ActivityLogItem[]>([]);

  useEffect(() => {
    const fetchRecent = async () => {
      try {
        const res = await bridge.call<{ items: ActivityLogItem[] }>('watcher.getActivity', {
          page: 1,
          pageSize: 6,
        });
        setRecentLogs(res?.items || []);
      } catch { }
    };
    fetchRecent();

    const unsub = bridge.on('FileOrganized', (log: ActivityLogItem) => {
      setRecentLogs((prev) => [log, ...prev.slice(0, 5)]);
    });

    return () => unsub();
  }, []);

  const totalFolders = folders.length;
  const activeFolders = folders.filter((f) => f.enabled).length;
  const totalOrganized = statistics?.totalFilesOrganized ?? 0;
  const organizedToday = statistics?.filesOrganizedToday ?? 0;
  const categoriesCount = statistics?.categoriesCount ?? 8;
  const successCount = statistics?.successfulOperations ?? 0;
  const failedCount = statistics?.failedOperations ?? 0;
  const successRate = totalOrganized > 0 ? Math.round((successCount / totalOrganized) * 100) : 100;

  const chartData = statistics?.activityOverTime && statistics.activityOverTime.length > 0
    ? statistics.activityOverTime
    : [
        { date: 'Mon', total: 0, success: 0 },
        { date: 'Tue', total: 0, success: 0 },
        { date: 'Wed', total: 0, success: 0 },
        { date: 'Thu', total: 0, success: 0 },
        { date: 'Fri', total: 0, success: 0 },
        { date: 'Sat', total: 0, success: 0 },
        { date: 'Today', total: organizedToday, success: organizedToday },
      ];

  const handleCreateTestFiles = async () => {
    const res = await createSampleFiles();
    if (res && res.created > 0) {
      showToast(`Created ${res.created} sample files in ${res.folder}!`, 'success');
    } else {
      showToast('Please add a watched folder first', 'error');
    }
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Metric Cards Row */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3.5">
        <MetricCard
          title="Active Folders"
          value={`${activeFolders}/${totalFolders}`}
          subtitle="Monitored in real-time"
          icon={FolderSync}
          iconColor="#6C9BFF"
          iconBg="rgba(108, 155, 255, 0.12)"
        />
        <MetricCard
          title="Total Files"
          value={totalOrganized.toLocaleString()}
          subtitle="Organized to date"
          icon={Files}
          iconColor="#35B77A"
          iconBg="rgba(53, 183, 122, 0.12)"
        />
        <MetricCard
          title="Organized Today"
          value={organizedToday.toLocaleString()}
          subtitle="Past 24 hours"
          icon={CalendarCheck}
          iconColor="#9B51E0"
          iconBg="rgba(155, 81, 224, 0.12)"
        />
        <MetricCard
          title="Categories"
          value={categoriesCount}
          subtitle="Rule sets active"
          icon={Tag}
          iconColor="#2D9CDB"
          iconBg="rgba(45, 156, 219, 0.12)"
        />
        <MetricCard
          title="Success Rate"
          value={`${successRate}%`}
          subtitle={`${successCount} operations`}
          icon={CheckCircle2}
          iconColor="#35B77A"
          iconBg="rgba(53, 183, 122, 0.12)"
        />
        <MetricCard
          title="Issues"
          value={failedCount}
          subtitle="Lock/skip events"
          icon={AlertTriangle}
          iconColor={failedCount > 0 ? '#E36F75' : '#A0A8B5'}
          iconBg={failedCount > 0 ? 'rgba(227, 111, 117, 0.12)' : 'rgba(160, 168, 181, 0.12)'}
        />
      </div>

      {/* Main Grid: Status Banner & Chart */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Status Card & Quick Actions (1 column) */}
        <div className="space-y-6">
          {/* Status Box */}
          <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card relative overflow-hidden">
            <div className="flex items-center justify-between mb-4">
              <div>
                <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Engine Status</span>
                <h3 className="text-lg font-bold text-[#F3F5F8] mt-0.5">
                  {isMonitoringActive ? 'Actively Monitoring' : 'Monitoring Paused'}
                </h3>
              </div>
              <div
                className={`w-3.5 h-3.5 rounded-full ${
                  isMonitoringActive ? 'bg-[#35B77A] shadow-[0_0_12px_#35B77A]' : 'bg-[#E9AE54]'
                }`}
              />
            </div>

            <p className="text-xs text-[#A0A8B5] leading-relaxed mb-5">
              {isMonitoringActive
                ? `System.IO.FileSystemWatcher is tracking ${activeFolders} location(s). Newly created files are validated for write readiness before moving.`
                : 'Background filesystem events are paused. Click Start Monitoring to resume real-time organization.'}
            </p>

            <div className="flex items-center gap-2.5">
              <button
                onClick={isMonitoringActive ? pauseMonitoring : startMonitoring}
                className={`flex-1 flex items-center justify-center gap-2 py-2.5 px-4 rounded-xl text-xs font-semibold transition-all shadow-subtle ${
                  isMonitoringActive
                    ? 'bg-[#272C36] text-[#E9AE54] border border-[#E9AE54]/30 hover:bg-[#2e3442]'
                    : 'bg-[#6C9BFF] text-white hover:bg-[#5287F6]'
                }`}
              >
                {isMonitoringActive ? (
                  <>
                    <Pause size={15} />
                    <span>Pause Monitoring</span>
                  </>
                ) : (
                  <>
                    <Play size={15} />
                    <span>Start Monitoring</span>
                  </>
                )}
              </button>
            </div>
          </div>

          {/* Quick Actions Card */}
          <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card">
            <h4 className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider mb-3">
              Quick Actions
            </h4>
            <div className="grid grid-cols-2 gap-2.5">
              <button
                onClick={onOpenAddFolder}
                className="flex items-center gap-2 p-3 rounded-xl bg-[#272C36] hover:bg-[#2f3542] text-left transition-colors border border-[#2B313E]"
              >
                <div className="w-8 h-8 rounded-lg bg-[#35B77A]/15 text-[#35B77A] flex items-center justify-center flex-shrink-0">
                  <FolderPlus size={16} />
                </div>
                <div>
                  <span className="text-xs font-semibold text-[#F3F5F8] block">Add Folder</span>
                  <span className="text-[10px] text-[#A0A8B5]">Watch directory</span>
                </div>
              </button>

              <button
                onClick={() => setCurrentPage('rules')}
                className="flex items-center gap-2 p-3 rounded-xl bg-[#272C36] hover:bg-[#2f3542] text-left transition-colors border border-[#2B313E]"
              >
                <div className="w-8 h-8 rounded-lg bg-[#6C9BFF]/15 text-[#6C9BFF] flex items-center justify-center flex-shrink-0">
                  <Sliders size={16} />
                </div>
                <div>
                  <span className="text-xs font-semibold text-[#F3F5F8] block">Edit Rules</span>
                  <span className="text-[10px] text-[#A0A8B5]">Customize types</span>
                </div>
              </button>

              <button
                onClick={handleCreateTestFiles}
                className="flex items-center gap-2 p-3 rounded-xl bg-[#272C36] hover:bg-[#2f3542] text-left transition-colors border border-[#2B313E]"
              >
                <div className="w-8 h-8 rounded-lg bg-[#E9AE54]/15 text-[#E9AE54] flex items-center justify-center flex-shrink-0">
                  <FlaskConical size={16} />
                </div>
                <div>
                  <span className="text-xs font-semibold text-[#F3F5F8] block">Seed Files</span>
                  <span className="text-[10px] text-[#A0A8B5]">Test rules</span>
                </div>
              </button>

              <button
                onClick={() => setCurrentPage('activity')}
                className="flex items-center gap-2 p-3 rounded-xl bg-[#272C36] hover:bg-[#2f3542] text-left transition-colors border border-[#2B313E]"
              >
                <div className="w-8 h-8 rounded-lg bg-[#9B51E0]/15 text-[#9B51E0] flex items-center justify-center flex-shrink-0">
                  <History size={16} />
                </div>
                <div>
                  <span className="text-xs font-semibold text-[#F3F5F8] block">History</span>
                  <span className="text-[10px] text-[#A0A8B5]">Audit operations</span>
                </div>
              </button>
            </div>
          </div>
        </div>

        {/* Activity Over Time Chart (2 columns) */}
        <div className="lg:col-span-2 bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card flex flex-col">
          <div className="flex items-center justify-between mb-4">
            <div>
              <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Trend Analysis</span>
              <h3 className="text-base font-bold text-[#F3F5F8] mt-0.5">Files Organized Over Time</h3>
            </div>
            <div className="flex items-center gap-2 text-xs text-[#A0A8B5]">
              <span className="inline-flex items-center gap-1">
                <span className="w-2.5 h-2.5 rounded-full bg-[#6C9BFF]" />
                <span>Files</span>
              </span>
            </div>
          </div>

          <div className="flex-1 w-full min-h-[220px]">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={chartData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                <defs>
                  <linearGradient id="colorTotal" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#6C9BFF" stopOpacity={0.4} />
                    <stop offset="95%" stopColor="#6C9BFF" stopOpacity={0.0} />
                  </linearGradient>
                </defs>
                <XAxis dataKey="date" stroke="#677182" fontSize={11} tickLine={false} />
                <YAxis stroke="#677182" fontSize={11} tickLine={false} allowDecimals={false} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#181B22',
                    borderColor: '#2B313E',
                    borderRadius: '8px',
                    color: '#F3F5F8',
                    fontSize: '12px',
                  }}
                />
                <Area
                  type="monotone"
                  dataKey="total"
                  stroke="#6C9BFF"
                  strokeWidth={2}
                  fillOpacity={1}
                  fill="url(#colorTotal)"
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      {/* Recent Activity Table */}
      <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card">
        <div className="flex items-center justify-between mb-4">
          <div>
            <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Live Feed</span>
            <h3 className="text-base font-bold text-[#F3F5F8] mt-0.5">Recent Activity</h3>
          </div>
          <button
            onClick={() => setCurrentPage('activity')}
            className="flex items-center gap-1 text-xs font-semibold text-[#6C9BFF] hover:text-[#5287F6] transition-colors"
          >
            <span>View all</span>
            <ArrowRight size={13} />
          </button>
        </div>

        {recentLogs.length === 0 ? (
          <div className="py-12 text-center">
            <Files size={36} className="mx-auto text-[#677182] mb-3 stroke-[1.5]" />
            <p className="text-sm font-semibold text-[#F3F5F8]">No files organized yet</p>
            <p className="text-xs text-[#A0A8B5] mt-1 max-w-sm mx-auto">
              Add a watched folder or generate test sample files to see real-time organization in action.
            </p>
            <button
              onClick={handleCreateTestFiles}
              className="mt-4 px-4 py-2 rounded-xl text-xs font-semibold bg-[#6C9BFF] text-white hover:bg-[#5287F6] transition-colors shadow-subtle inline-flex items-center gap-2"
            >
              <FlaskConical size={14} />
              <span>Create Test Files Now</span>
            </button>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead>
                <tr className="border-b border-[#2B313E] text-[#A0A8B5] font-semibold">
                  <th className="pb-3 pl-1">File Name</th>
                  <th className="pb-3">Category</th>
                  <th className="pb-3 hidden sm:table-cell">Target Path</th>
                  <th className="pb-3">Timestamp</th>
                  <th className="pb-3 text-right pr-1">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[#2B313E]/50">
                {recentLogs.map((log) => (
                  <tr key={log.id} className="hover:bg-[#272C36]/50 transition-colors">
                    <td className="py-3 pl-1 font-medium text-[#F3F5F8] truncate max-w-[200px]">
                      {log.fileName}
                    </td>
                    <td className="py-3">
                      <span className="px-2 py-0.5 rounded-md bg-[#272C36] text-[#6C9BFF] font-medium border border-[#2B313E]">
                        {log.category || 'Other'}
                      </span>
                    </td>
                    <td className="py-3 text-[#A0A8B5] truncate max-w-[260px] hidden sm:table-cell">
                      {log.destinationPath || '-'}
                    </td>
                    <td className="py-3 text-[#A0A8B5] whitespace-nowrap">
                      {new Date(log.timestamp).toLocaleTimeString()}
                    </td>
                    <td className="py-3 text-right pr-1">
                      <StatusBadge status={log.status} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

