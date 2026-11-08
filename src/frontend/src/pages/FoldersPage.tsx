import React, { useState } from 'react';
import {
  FolderPlus,
  Trash2,
  FolderOpen,
  CheckCircle2,
  AlertCircle,
  ToggleLeft,
  ToggleRight,
  Settings2,
  HardDrive,
  Volume2,
  VolumeX,
  Layers,
} from 'lucide-react';
import { useMonitoringStore } from '../stores/useMonitoringStore';
import { useAppStore } from '../stores/useAppStore';
import { StatusBadge } from '../components/StatusBadge';
import { Modal } from '../components/Modal';
import { WatchedFolder } from '../../../shared/contracts/contracts';

interface FoldersPageProps {
  isAddModalOpen: boolean;
  setIsAddModalOpen: (open: boolean) => void;
}

export const FoldersPage: React.FC<FoldersPageProps> = ({
  isAddModalOpen,
  setIsAddModalOpen,
}) => {
  const {
    folders,
    addFolder,
    updateFolder,
    removeFolder,
    openNativeFolderPicker,
    startMonitoring,
    isMonitoringActive,
  } = useMonitoringStore();
  const { showToast } = useAppStore();

  const [pathInput, setPathInput] = useState('');
  const [unknownInput, setUnknownInput] = useState('Other');
  const [quietInput, setQuietInput] = useState(false);
  const [recursiveInput, setRecursiveInput] = useState(false);

  // Edit Modal State
  const [editingFolder, setEditingFolder] = useState<WatchedFolder | null>(null);

  // Delete Confirm State
  const [deletingFolder, setDeletingFolder] = useState<WatchedFolder | null>(null);

  const handleBrowseFolder = async () => {
    const selected = await openNativeFolderPicker();
    if (selected) {
      setPathInput(selected);
    }
  };

  const handleCreateFolder = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!pathInput.trim()) {
      showToast('Folder path cannot be empty', 'error');
      return;
    }

    const success = await addFolder(pathInput.trim(), unknownInput.trim() || 'Other', quietInput, recursiveInput);
    if (success) {
      showToast(`Added watched folder: ${pathInput}`, 'success');
      setPathInput('');
      setUnknownInput('Other');
      setQuietInput(false);
      setRecursiveInput(false);
      setIsAddModalOpen(false);
    } else {
      showToast('Could not add folder. Please verify the path exists and is not already added.', 'error');
    }
  };

  const handleSaveEdit = async () => {
    if (!editingFolder) return;
    const ok = await updateFolder(editingFolder);
    if (ok) {
      showToast('Folder settings updated successfully', 'success');
      setEditingFolder(null);
    }
  };

  const handleConfirmDelete = async () => {
    if (!deletingFolder) return;
    const ok = await removeFolder(deletingFolder.id);
    if (ok) {
      showToast('Watched folder removed', 'info');
      setDeletingFolder(null);
    }
  };

  const handleToggleEnabled = async (folder: WatchedFolder) => {
    await updateFolder({
      ...folder,
      enabled: !folder.enabled,
    });
    showToast(`Folder ${folder.enabled ? 'paused' : 'enabled'}: ${folder.path}`, 'info');
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Top Banner */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card">
        <div>
          <h2 className="text-base font-bold text-[#F3F5F8]">Monitored Locations</h2>
          <p className="text-xs text-[#A0A8B5] mt-0.5">
            Watcher scans these directories in real-time and sorts new files into categorized folders.
          </p>
        </div>
        <button
          onClick={() => setIsAddModalOpen(true)}
          className="flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl text-xs font-semibold bg-[#6C9BFF] text-white hover:bg-[#5287F6] transition-all shadow-subtle flex-shrink-0"
        >
          <FolderPlus size={16} />
          <span>Add Monitored Folder</span>
        </button>
      </div>

      {/* Folders List */}
      {folders.length === 0 ? (
        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-12 text-center shadow-card">
          <HardDrive size={40} className="mx-auto text-[#677182] mb-3 stroke-[1.5]" />
          <h3 className="text-sm font-bold text-[#F3F5F8]">No Watched Folders Configured</h3>
          <p className="text-xs text-[#A0A8B5] mt-1 max-w-md mx-auto">
            Get started by adding your Downloads directory or any desktop folder you want to automatically keep clean.
          </p>
          <button
            onClick={() => setIsAddModalOpen(true)}
            className="mt-4 px-4 py-2 rounded-xl text-xs font-semibold bg-[#6C9BFF] text-white hover:bg-[#5287F6] transition-colors shadow-subtle inline-flex items-center gap-2"
          >
            <FolderPlus size={15} />
            <span>Add Watched Folder Now</span>
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {folders.map((folder) => {
            return (
              <div
                key={folder.id}
                className={`bg-[#20242D] border rounded-2xl p-5 shadow-card transition-all ${
                  folder.enabled ? 'border-[#2B313E] hover:border-[#6C9BFF]/40' : 'border-[#2B313E]/50 opacity-75'
                }`}
              >
                {/* Header & Status */}
                <div className="flex items-start justify-between gap-3 mb-3">
                  <div className="flex items-center gap-2.5 min-w-0">
                    <div className="w-9 h-9 rounded-xl bg-[#6C9BFF]/15 text-[#6C9BFF] flex items-center justify-center flex-shrink-0">
                      <FolderOpen size={18} />
                    </div>
                    <div className="min-w-0">
                      <h4 className="text-sm font-bold text-[#F3F5F8] truncate" title={folder.path}>
                        {folder.path}
                      </h4>
                      <span className="text-[11px] text-[#A0A8B5]">
                        {folder.filesOrganizedCount} file(s) organized
                      </span>
                    </div>
                  </div>

                  <StatusBadge
                    status={folder.enabled && isMonitoringActive ? 'active' : 'paused'}
                    label={folder.enabled ? (isMonitoringActive ? 'Monitoring' : 'Ready') : 'Disabled'}
                  />
                </div>

                {/* Properties Badge Row */}
                <div className="flex flex-wrap items-center gap-2 pt-2 border-t border-[#2B313E]/60 text-[11px] text-[#A0A8B5]">
                  <span className="bg-[#272C36] px-2 py-0.5 rounded border border-[#2B313E]">
                    Fallback: <strong className="text-[#F3F5F8]">{folder.unknownTarget}</strong>
                  </span>
                  <span className="bg-[#272C36] px-2 py-0.5 rounded border border-[#2B313E] flex items-center gap-1">
                    {folder.recursive ? <Layers size={12} className="text-[#6C9BFF]" /> : null}
                    <span>{folder.recursive ? 'Recursive' : 'Top-Level Only'}</span>
                  </span>
                  <span className="bg-[#272C36] px-2 py-0.5 rounded border border-[#2B313E] flex items-center gap-1">
                    {folder.quiet ? <VolumeX size={12} /> : <Volume2 size={12} />}
                    <span>{folder.quiet ? 'Quiet' : 'Verbose'}</span>
                  </span>
                </div>

                {/* Footer Controls */}
                <div className="flex items-center justify-between mt-4 pt-3 border-t border-[#2B313E]/60">
                  <button
                    onClick={() => handleToggleEnabled(folder)}
                    className="flex items-center gap-1.5 text-xs font-semibold text-[#A0A8B5] hover:text-[#F3F5F8] transition-colors"
                  >
                    {folder.enabled ? (
                      <>
                        <ToggleRight size={20} className="text-[#35B77A]" />
                        <span>Enabled</span>
                      </>
                    ) : (
                      <>
                        <ToggleLeft size={20} className="text-[#A0A8B5]" />
                        <span>Disabled</span>
                      </>
                    )}
                  </button>

                  <div className="flex items-center gap-1.5">
                    <button
                      onClick={() => setEditingFolder({ ...folder })}
                      className="p-1.5 rounded-lg text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] transition-colors"
                      title="Edit folder configuration"
                    >
                      <Settings2 size={16} />
                    </button>
                    <button
                      onClick={() => setDeletingFolder(folder)}
                      className="p-1.5 rounded-lg text-[#A0A8B5] hover:text-[#E36F75] hover:bg-[#E36F75]/10 transition-colors"
                      title="Remove from watched folders"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Add Folder Modal */}
      <Modal isOpen={isAddModalOpen} onClose={() => setIsAddModalOpen(false)} title="Add Watched Folder">
        <form onSubmit={handleCreateFolder} className="space-y-4">
          <div>
            <label className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider block mb-1.5">
              Folder Path
            </label>
            <div className="flex gap-2">
              <input
                type="text"
                value={pathInput}
                onChange={(e) => setPathInput(e.target.value)}
                placeholder="e.g. D:\Downloads or C:\Users\name\Desktop"
                className="flex-1 bg-[#181B22] border border-[#2B313E] rounded-xl px-3.5 py-2 text-xs text-[#F3F5F8] placeholder-[#677182] focus:outline-none focus:border-[#6C9BFF]"
              />
              <button
                type="button"
                onClick={handleBrowseFolder}
                className="px-3.5 py-2 rounded-xl text-xs font-semibold bg-[#272C36] text-[#F3F5F8] hover:bg-[#2f3542] border border-[#2B313E] transition-colors flex-shrink-0"
              >
                Browse...
              </button>
            </div>
            <p className="text-[11px] text-[#A0A8B5] mt-1">
              Select any local or secondary drive folder to monitor automatically.
            </p>
          </div>

          <div>
            <label className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider block mb-1.5">
              Unrecognized File Target
            </label>
            <input
              type="text"
              value={unknownInput}
              onChange={(e) => setUnknownInput(e.target.value)}
              placeholder="Other"
              className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl px-3.5 py-2 text-xs text-[#F3F5F8] placeholder-[#677182] focus:outline-none focus:border-[#6C9BFF]"
            />
            <p className="text-[11px] text-[#A0A8B5] mt-1">
              Folder name where files with unmatched extensions will be routed (default: Other).
            </p>
          </div>

          <div className="space-y-2 pt-2 border-t border-[#2B313E]">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={recursiveInput}
                onChange={(e) => setRecursiveInput(e.target.checked)}
                className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#181B22]"
              />
              <span className="text-xs text-[#F3F5F8]">Enable recursive subfolder monitoring</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={quietInput}
                onChange={(e) => setQuietInput(e.target.checked)}
                className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#181B22]"
              />
              <span className="text-xs text-[#F3F5F8]">Quiet mode (suppress desktop notifications for this folder)</span>
            </label>
          </div>

          <div className="flex justify-end gap-2.5 pt-4">
            <button
              type="button"
              onClick={() => setIsAddModalOpen(false)}
              className="px-4 py-2 rounded-xl text-xs font-semibold text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 rounded-xl text-xs font-semibold bg-[#6C9BFF] text-white hover:bg-[#5287F6] transition-colors shadow-subtle"
            >
              Add Folder
            </button>
          </div>
        </form>
      </Modal>

      {/* Edit Folder Modal */}
      <Modal isOpen={!!editingFolder} onClose={() => setEditingFolder(null)} title="Configure Watched Folder">
        {editingFolder && (
          <div className="space-y-4">
            <div>
              <label className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider block mb-1">
                Folder Path
              </label>
              <input
                type="text"
                disabled
                value={editingFolder.path}
                className="w-full bg-[#181B22]/50 border border-[#2B313E] rounded-xl px-3.5 py-2 text-xs text-[#A0A8B5] cursor-not-allowed"
              />
            </div>

            <div>
              <label className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider block mb-1">
                Fallback Unknown Target
              </label>
              <input
                type="text"
                value={editingFolder.unknownTarget}
                onChange={(e) => setEditingFolder({ ...editingFolder, unknownTarget: e.target.value })}
                className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl px-3.5 py-2 text-xs text-[#F3F5F8] focus:outline-none focus:border-[#6C9BFF]"
              />
            </div>

            <div className="space-y-2 pt-2 border-t border-[#2B313E]">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={editingFolder.recursive}
                  onChange={(e) => setEditingFolder({ ...editingFolder, recursive: e.target.checked })}
                  className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#181B22]"
                />
                <span className="text-xs text-[#F3F5F8]">Monitor subdirectories recursively</span>
              </label>
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={editingFolder.quiet}
                  onChange={(e) => setEditingFolder({ ...editingFolder, quiet: e.target.checked })}
                  className="rounded border-[#2B313E] text-[#6C9BFF] focus:ring-0 bg-[#181B22]"
                />
                <span className="text-xs text-[#F3F5F8]">Quiet mode (reduce notifications)</span>
              </label>
            </div>

            <div className="flex justify-end gap-2.5 pt-4">
              <button
                type="button"
                onClick={() => setEditingFolder(null)}
                className="px-4 py-2 rounded-xl text-xs font-semibold text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] transition-colors"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleSaveEdit}
                className="px-4 py-2 rounded-xl text-xs font-semibold bg-[#6C9BFF] text-white hover:bg-[#5287F6] transition-colors shadow-subtle"
              >
                Save Changes
              </button>
            </div>
          </div>
        )}
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal isOpen={!!deletingFolder} onClose={() => setDeletingFolder(null)} title="Remove Watched Folder">
        {deletingFolder && (
          <div className="space-y-4">
            <p className="text-xs text-[#F3F5F8]">
              Are you sure you want to stop monitoring this location?
            </p>
            <div className="p-3 bg-[#181B22] border border-[#2B313E] rounded-xl text-xs text-[#A0A8B5] font-mono break-all">
              {deletingFolder.path}
            </div>
            <p className="text-[11px] text-[#A0A8B5]">
              No files inside the directory will be deleted or moved. Watcher will simply cease monitoring it.
            </p>

            <div className="flex justify-end gap-2.5 pt-3">
              <button
                type="button"
                onClick={() => setDeletingFolder(null)}
                className="px-4 py-2 rounded-xl text-xs font-semibold text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] transition-colors"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleConfirmDelete}
                className="px-4 py-2 rounded-xl text-xs font-semibold bg-[#E36F75] text-white hover:bg-[#c9595f] transition-colors shadow-subtle"
              >
                Remove
              </button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};

