import React, { useEffect, useState } from 'react';
import {
  Search,
  Filter,
  Trash2,
  Undo2,
  ExternalLink,
  Copy,
  Check,
  ChevronLeft,
  ChevronRight,
  FileQuestion,
  RotateCcw,
} from 'lucide-react';
import { useMonitoringStore } from '../stores/useMonitoringStore';
import { useAppStore } from '../stores/useAppStore';
import { StatusBadge } from '../components/StatusBadge';
import { ActivityLogItem, OperationStatus } from '../../../shared/contracts/contracts';
import { bridge } from '../services/bridgeClient';

export const ActivityPage: React.FC = () => {
  const { rules } = useMonitoringStore();
  const { showToast } = useAppStore();

  const [logs, setLogs] = useState<ActivityLogItem[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const pageSize = 15;

  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('All');
  const [copiedId, setCopiedId] = useState<string | null>(null);
  const [isClearing, setIsClearing] = useState(false);

  const fetchLogs = async () => {
    try {
      const res = await bridge.call<{ items: ActivityLogItem[]; total: number }>('watcher.getActivity', {
        search: searchQuery.trim() || undefined,
        category: selectedCategory !== 'All' ? selectedCategory : undefined,
        page,
        pageSize,
      });
      setLogs(res?.items || []);
      setTotalCount(res?.total || 0);
    } catch { }
  };

  useEffect(() => {
    fetchLogs();
  }, [page, selectedCategory]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchLogs();
  };

  const handleOpenExplorer = async (destPath: string) => {
    if (!destPath) return;
    try {
      await bridge.call('watcher.openFileInExplorer', destPath);
    } catch {
      showToast('Could not open location in File Explorer', 'error');
    }
  };

  const handleCopyPath = (path: string, id: string) => {
    navigator.clipboard.writeText(path);
    setCopiedId(id);
    showToast('Copied path to clipboard', 'info');
    setTimeout(() => setCopiedId(null), 2000);
  };

  const handleUndo = async (logItem: ActivityLogItem) => {
    try {
      const success = await bridge.call<boolean>('watcher.undoActivity', logItem.id);
      if (success) {
        showToast(`Undid organization for ${logItem.fileName}`, 'success');
        fetchLogs();
      } else {
        showToast('Cannot undo: file was moved or no longer exists at destination', 'error');
      }
    } catch {
      showToast('Undo operation failed', 'error');
    }
  };

  const handleClearHistory = async () => {
    if (window.confirm('Clear all activity logs from database?')) {
      await bridge.call('watcher.clearActivity');
      showToast('Activity log history cleared', 'info');
      fetchLogs();
    }
  };

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Search & Filter Header */}
      <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-4 shadow-card flex flex-col md:flex-row md:items-center justify-between gap-4">
        {/* Search */}
        <form onSubmit={handleSearchSubmit} className="flex-1 max-w-md relative">
          <Search size={15} className="absolute left-3.5 top-1/2 -translate-y-1/2 text-[#677182]" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Search by filename, source, or destination..."
            className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl pl-9 pr-3.5 py-2 text-xs text-[#F3F5F8] placeholder-[#677182] focus:outline-none focus:border-[#6C9BFF]"
          />
        </form>

        {/* Filters */}
        <div className="flex items-center gap-2.5">
          <div className="flex items-center gap-1.5 text-xs text-[#A0A8B5]">
            <Filter size={14} />
            <span>Category:</span>
          </div>
          <select
            value={selectedCategory}
            onChange={(e) => {
              setSelectedCategory(e.target.value);
              setPage(1);
            }}
            className="bg-[#181B22] border border-[#2B313E] rounded-xl px-3 py-1.5 text-xs text-[#F3F5F8] focus:outline-none focus:border-[#6C9BFF]"
          >
            <option value="All">All Categories</option>
            {rules.map((r) => (
              <option key={r.id} value={r.categoryName}>
                {r.categoryName}
              </option>
            ))}
            <option value="Other">Other</option>
          </select>

          <button
            onClick={handleClearHistory}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold bg-[#272C36] text-[#A0A8B5] hover:text-[#E36F75] hover:bg-[#E36F75]/10 border border-[#2B313E] transition-colors"
            title="Clear all logs"
          >
            <Trash2 size={13} />
            <span>Clear History</span>
          </button>
        </div>
      </div>

      {/* Activity Table */}
      <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl shadow-card overflow-hidden">
        {logs.length === 0 ? (
          <div className="py-16 text-center">
            <FileQuestion size={40} className="mx-auto text-[#677182] mb-3 stroke-[1.5]" />
            <p className="text-sm font-semibold text-[#F3F5F8]">No Activity Records Found</p>
            <p className="text-xs text-[#A0A8B5] mt-1 max-w-sm mx-auto">
              Organized files will be automatically logged here with undo capabilities.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead>
                <tr className="bg-[#181B22]/70 border-b border-[#2B313E] text-[#A0A8B5] font-semibold">
                  <th className="py-3 px-4">File Name</th>
                  <th className="py-3 px-4">Category</th>
                  <th className="py-3 px-4 hidden lg:table-cell">Source Directory</th>
                  <th className="py-3 px-4">Destination Path</th>
                  <th className="py-3 px-4 whitespace-nowrap">Timestamp</th>
                  <th className="py-3 px-4">Status</th>
                  <th className="py-3 px-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-[#2B313E]/50">
                {logs.map((log) => (
                  <tr key={log.id} className="hover:bg-[#272C36]/50 transition-colors">
                    <td className="py-3 px-4 font-medium text-[#F3F5F8] max-w-[200px] truncate" title={log.fileName}>
                      {log.fileName}
                    </td>
                    <td className="py-3 px-4">
                      <span className="px-2 py-0.5 rounded-md bg-[#272C36] text-[#6C9BFF] font-medium border border-[#2B313E]">
                        {log.category || 'Other'}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-[#A0A8B5] max-w-[180px] truncate hidden lg:table-cell" title={log.sourcePath}>
                      {log.sourcePath}
                    </td>
                    <td className="py-3 px-4 text-[#A0A8B5] max-w-[240px] truncate font-mono text-[11px]" title={log.destinationPath}>
                      {log.destinationPath || '-'}
                    </td>
                    <td className="py-3 px-4 text-[#A0A8B5] whitespace-nowrap">
                      {new Date(log.timestamp).toLocaleString()}
                    </td>
                    <td className="py-3 px-4 whitespace-nowrap">
                      <StatusBadge status={log.status} />
                    </td>
                    <td className="py-3 px-4 text-right whitespace-nowrap">
                      <div className="flex items-center justify-end gap-1.5">
                        {log.destinationPath && (
                          <button
                            onClick={() => handleOpenExplorer(log.destinationPath)}
                            className="p-1.5 rounded-lg text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] transition-colors"
                            title="Reveal in Windows Explorer"
                          >
                            <ExternalLink size={14} />
                          </button>
                        )}
                        {log.destinationPath && (
                          <button
                            onClick={() => handleCopyPath(log.destinationPath, log.id)}
                            className="p-1.5 rounded-lg text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] transition-colors"
                            title="Copy file path"
                          >
                            {copiedId === log.id ? <Check size={14} className="text-[#35B77A]" /> : <Copy size={14} />}
                          </button>
                        )}
                        {log.canUndo && log.status === 'SUCCESS' && (
                          <button
                            onClick={() => handleUndo(log)}
                            className="p-1.5 rounded-lg text-[#A0A8B5] hover:text-[#E9AE54] hover:bg-[#E9AE54]/10 transition-colors"
                            title="Undo file movement"
                          >
                            <Undo2 size={14} />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination Controls */}
        <div className="p-4 border-t border-[#2B313E] flex items-center justify-between text-xs text-[#A0A8B5]">
          <span>
            Showing {logs.length} of {totalCount} total entries
          </span>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page <= 1}
              className="p-1.5 rounded-lg border border-[#2B313E] text-[#F3F5F8] hover:bg-[#272C36] disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronLeft size={15} />
            </button>
            <span className="font-semibold text-[#F3F5F8]">
              Page {page} of {totalPages}
            </span>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page >= totalPages}
              className="p-1.5 rounded-lg border border-[#2B313E] text-[#F3F5F8] hover:bg-[#272C36] disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
            >
              <ChevronRight size={15} />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

