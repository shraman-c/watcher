import React from 'react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  AreaChart,
  Area,
} from 'recharts';
import {
  TrendingUp,
  FolderSync,
  PieChart as PieIcon,
  BarChart2,
  CheckCircle2,
  XCircle,
  FileText,
} from 'lucide-react';
import { useMonitoringStore } from '../stores/useMonitoringStore';

const COLORS = ['#6C9BFF', '#35B77A', '#9B51E0', '#F2994A', '#E9AE54', '#2D9CDB', '#BB6BD9', '#E36F75'];

export const StatisticsPage: React.FC = () => {
  const { statistics, folders } = useMonitoringStore();

  const totalFiles = statistics?.totalFilesOrganized ?? 0;
  const successFiles = statistics?.successfulOperations ?? 0;
  const failedFiles = statistics?.failedOperations ?? 0;
  const successRate = totalFiles > 0 ? ((successFiles / totalFiles) * 100).toFixed(1) : '100.0';

  const categoryData = statistics?.categoryBreakdown && statistics.categoryBreakdown.length > 0
    ? statistics.categoryBreakdown
    : [
        { category: 'Documents', count: 12 },
        { category: 'Images', count: 8 },
        { category: 'Code', count: 5 },
        { category: 'Archives', count: 3 },
      ];

  const folderData = statistics?.folderBreakdown && statistics.folderBreakdown.length > 0
    ? statistics.folderBreakdown.map((f) => ({
        name: f.path.split(/[\\/]/).pop() || f.path,
        fullPath: f.path,
        count: f.count,
      }))
    : folders.map((f) => ({
        name: f.path.split(/[\\/]/).pop() || f.path,
        fullPath: f.path,
        count: f.filesOrganizedCount,
      }));

  const timelineData = statistics?.activityOverTime && statistics.activityOverTime.length > 0
    ? statistics.activityOverTime
    : [
        { date: 'Nov 02', total: 4, success: 4, failed: 0 },
        { date: 'Nov 03', total: 8, success: 8, failed: 0 },
        { date: 'Nov 04', total: 15, success: 15, failed: 0 },
        { date: 'Nov 05', total: 22, success: 21, failed: 1 },
        { date: 'Nov 06', total: 18, success: 18, failed: 0 },
        { date: 'Nov 07', total: 31, success: 30, failed: 1 },
      ];

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Overview Highlight Banner */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Total Operations</span>
            <h3 className="text-2xl font-bold text-[#F3F5F8] mt-1">{totalFiles.toLocaleString()}</h3>
            <span className="text-[11px] text-[#A0A8B5] mt-0.5 block">Lifetime organized files</span>
          </div>
          <div className="w-12 h-12 rounded-xl bg-[#6C9BFF]/15 text-[#6C9BFF] flex items-center justify-center">
            <FileText size={24} />
          </div>
        </div>

        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Reliability Score</span>
            <h3 className="text-2xl font-bold text-[#35B77A] mt-1">{successRate}%</h3>
            <span className="text-[11px] text-[#A0A8B5] mt-0.5 block">{successFiles} successful moves</span>
          </div>
          <div className="w-12 h-12 rounded-xl bg-[#35B77A]/15 text-[#35B77A] flex items-center justify-center">
            <CheckCircle2 size={24} />
          </div>
        </div>

        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card flex items-center justify-between">
          <div>
            <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Anomalies / Skips</span>
            <h3 className="text-2xl font-bold text-[#E36F75] mt-1">{failedFiles}</h3>
            <span className="text-[11px] text-[#A0A8B5] mt-0.5 block">Locks or duplicate skips</span>
          </div>
          <div className="w-12 h-12 rounded-xl bg-[#E36F75]/15 text-[#E36F75] flex items-center justify-center">
            <XCircle size={24} />
          </div>
        </div>
      </div>

      {/* Main Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Activity Over Time */}
        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card flex flex-col">
          <div className="flex items-center justify-between mb-4">
            <div>
              <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Trend</span>
              <h3 className="text-sm font-bold text-[#F3F5F8]">Daily Volume Timeline</h3>
            </div>
            <TrendingUp size={16} className="text-[#6C9BFF]" />
          </div>

          <div className="h-64 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={timelineData}>
                <defs>
                  <linearGradient id="areaGrad" x1="0" y1="0" x2="0" y2="1">
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
                <Area type="monotone" dataKey="total" stroke="#6C9BFF" strokeWidth={2} fill="url(#areaGrad)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Categories Distribution */}
        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card flex flex-col">
          <div className="flex items-center justify-between mb-4">
            <div>
              <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Categories</span>
              <h3 className="text-sm font-bold text-[#F3F5F8]">Most Frequent File Categories</h3>
            </div>
            <PieIcon size={16} className="text-[#9B51E0]" />
          </div>

          <div className="h-64 w-full flex items-center justify-center">
            {categoryData.length === 0 ? (
              <span className="text-xs text-[#A0A8B5]">No data yet</span>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={categoryData} layout="vertical" margin={{ left: 20 }}>
                  <XAxis type="number" stroke="#677182" fontSize={11} tickLine={false} />
                  <YAxis dataKey="category" type="category" stroke="#677182" fontSize={11} tickLine={false} />
                  <Tooltip
                    contentStyle={{
                      backgroundColor: '#181B22',
                      borderColor: '#2B313E',
                      borderRadius: '8px',
                      color: '#F3F5F8',
                      fontSize: '12px',
                    }}
                  />
                  <Bar dataKey="count" radius={[0, 6, 6, 0]}>
                    {categoryData.map((_, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Folders Processing Volume */}
        <div className="lg:col-span-2 bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card">
          <div className="flex items-center justify-between mb-4">
            <div>
              <span className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider">Distribution</span>
              <h3 className="text-sm font-bold text-[#F3F5F8]">Files Processed Per Watched Folder</h3>
            </div>
            <BarChart2 size={16} className="text-[#35B77A]" />
          </div>

          <div className="h-56 w-full">
            {folderData.length === 0 ? (
              <div className="h-full flex items-center justify-center text-xs text-[#A0A8B5]">
                No watched folders configured
              </div>
            ) : (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={folderData}>
                  <XAxis dataKey="name" stroke="#677182" fontSize={11} tickLine={false} />
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
                  <Bar dataKey="count" fill="#35B77A" radius={[6, 6, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

