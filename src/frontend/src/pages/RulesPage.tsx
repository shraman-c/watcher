import React, { useState } from 'react';
import {
  Sliders,
  Plus,
  Trash2,
  RotateCcw,
  Tag,
  CheckCircle2,
  Folder,
  ArrowRight,
  Search,
  Sparkles,
  AlertCircle,
  X,
} from 'lucide-react';
import { useMonitoringStore } from '../stores/useMonitoringStore';
import { useAppStore } from '../stores/useAppStore';
import { Modal } from '../components/Modal';
import { OrganizationRule, Category } from '../../../shared/contracts/contracts';

export const RulesPage: React.FC = () => {
  const { rules, categories, saveRule, deleteRule, resetRulesToDefault } = useMonitoringStore();
  const { showToast } = useAppStore();

  const [testFileName, setTestFileName] = useState('annual_report.pdf');
  const [selectedRule, setSelectedRule] = useState<OrganizationRule | null>(null);
  const [newExtensionInput, setNewExtensionInput] = useState('');

  // Add Category / Rule Modal State
  const [isAddRuleOpen, setIsAddRuleOpen] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');
  const [newTargetFolder, setNewTargetFolder] = useState('');
  const [newExtensionsText, setNewExtensionsText] = useState('');

  // Live File Match Simulator
  const simulateMatch = (fileName: string) => {
    const ext = fileName.includes('.') ? fileName.split('.').pop()?.toLowerCase() || '' : '';
    if (!ext) return { category: 'None', folder: 'Other', matchedRule: null };

    const activeRules = rules.filter((r) => r.enabled).sort((a, b) => a.priority - b.priority);

    for (const rule of activeRules) {
      if (rule.extensions.some((e) => e.toLowerCase() === ext)) {
        return {
          category: rule.categoryName,
          folder: rule.targetFolder || rule.categoryName,
          matchedRule: rule,
        };
      }
    }

    return { category: 'Other', folder: 'Other', matchedRule: null };
  };

  const currentMatch = simulateMatch(testFileName);

  const handleToggleRule = async (rule: OrganizationRule) => {
    await saveRule({ ...rule, enabled: !rule.enabled });
    showToast(`Rule '${rule.categoryName}' ${rule.enabled ? 'disabled' : 'enabled'}`, 'info');
  };

  const handleRemoveExtension = async (rule: OrganizationRule, extToRemove: string) => {
    const updated = {
      ...rule,
      extensions: rule.extensions.filter((e) => e.toLowerCase() !== extToRemove.toLowerCase()),
    };
    await saveRule(updated);
  };

  const handleAddExtension = async (rule: OrganizationRule) => {
    const clean = newExtensionInput.trim().replace(/^\./, '').toLowerCase();
    if (!clean) return;

    if (rule.extensions.some((e) => e.toLowerCase() === clean)) {
      showToast(`Extension '.${clean}' already exists in this rule`, 'error');
      return;
    }

    const updated = {
      ...rule,
      extensions: [...rule.extensions, clean],
    };

    await saveRule(updated);
    setNewExtensionInput('');
    if (selectedRule?.id === rule.id) {
      setSelectedRule(updated);
    }
  };

  const handleCreateRule = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCategoryName.trim()) {
      showToast('Category name is required', 'error');
      return;
    }

    const exts = newExtensionsText
      .split(',')
      .map((e) => e.trim().replace(/^\./, '').toLowerCase())
      .filter((e) => e.length > 0);

    const newRule: OrganizationRule = {
      id: `rule-${Date.now()}`,
      categoryId: `cat-${Date.now()}`,
      categoryName: newCategoryName.trim(),
      targetFolder: newTargetFolder.trim() || newCategoryName.trim(),
      extensions: exts,
      priority: rules.length + 1,
      enabled: true,
      isCustom: true,
    };

    const ok = await saveRule(newRule);
    if (ok) {
      showToast(`Created rule: ${newCategoryName}`, 'success');
      setNewCategoryName('');
      setNewTargetFolder('');
      setNewExtensionsText('');
      setIsAddRuleOpen(false);
    }
  };

  const handleResetDefaults = async () => {
    if (window.confirm('Reset all organization rules and extensions back to default settings?')) {
      await resetRulesToDefault();
      showToast('Reset rules to default configuration', 'success');
    }
  };

  return (
    <div className="p-6 space-y-6 max-w-7xl mx-auto">
      {/* Top Banner & Simulator */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card flex flex-col justify-between">
          <div>
            <div className="flex items-center justify-between mb-2">
              <h2 className="text-base font-bold text-[#F3F5F8]">File Categorization Engine</h2>
              <div className="flex items-center gap-2">
                <button
                  onClick={handleResetDefaults}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold bg-[#272C36] text-[#A0A8B5] hover:text-[#F3F5F8] border border-[#2B313E] transition-colors"
                  title="Reload factory standard rules"
                >
                  <RotateCcw size={13} />
                  <span>Restore Defaults</span>
                </button>
                <button
                  onClick={() => setIsAddRuleOpen(true)}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-semibold bg-[#6C9BFF] text-white hover:bg-[#5287F6] transition-colors shadow-subtle"
                >
                  <Plus size={14} />
                  <span>New Rule</span>
                </button>
              </div>
            </div>
            <p className="text-xs text-[#A0A8B5] leading-relaxed">
              When new files are saved, Watcher reads their extension, matches against priority rules, and moves them to corresponding destination subfolders.
            </p>
          </div>
        </div>

        {/* Live File Match Simulator Widget */}
        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card">
          <div className="flex items-center gap-1.5 text-xs font-semibold text-[#6C9BFF] uppercase tracking-wider mb-2">
            <Sparkles size={14} />
            <span>Rule Match Preview</span>
          </div>

          <div className="space-y-3">
            <input
              type="text"
              value={testFileName}
              onChange={(e) => setTestFileName(e.target.value)}
              placeholder="Type filename e.g. code.ts, video.mp4"
              className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl px-3 py-1.5 text-xs text-[#F3F5F8] placeholder-[#677182] focus:outline-none focus:border-[#6C9BFF]"
            />

            <div className="p-3 bg-[#181B22] border border-[#2B313E] rounded-xl flex items-center justify-between text-xs">
              <span className="text-[#A0A8B5]">Destination:</span>
              <div className="flex items-center gap-1.5 font-semibold text-[#35B77A]">
                <Folder size={14} />
                <span>{currentMatch.folder}/</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Rules Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {rules.map((rule) => {
          return (
            <div
              key={rule.id}
              className={`bg-[#20242D] border rounded-2xl p-5 shadow-card flex flex-col justify-between transition-all ${
                rule.enabled ? 'border-[#2B313E] hover:border-[#6C9BFF]/40' : 'border-[#2B313E]/50 opacity-70'
              }`}
            >
              <div>
                {/* Header */}
                <div className="flex items-start justify-between gap-2 mb-3">
                  <div>
                    <div className="flex items-center gap-2">
                      <h4 className="text-sm font-bold text-[#F3F5F8]">{rule.categoryName}</h4>
                      {rule.isCustom && (
                        <span className="text-[10px] px-1.5 py-0.2 rounded bg-[#6C9BFF]/15 text-[#6C9BFF] font-medium">
                          Custom
                        </span>
                      )}
                    </div>
                    <span className="text-[11px] text-[#A0A8B5] flex items-center gap-1 mt-0.5">
                      <Folder size={12} />
                      <span>Destination: <strong>{rule.targetFolder || rule.categoryName}/</strong></span>
                    </span>
                  </div>

                  <button
                    onClick={() => handleToggleRule(rule)}
                    className={`text-xs px-2 py-0.5 rounded-full font-semibold border transition-colors ${
                      rule.enabled
                        ? 'bg-[#35B77A]/15 text-[#35B77A] border-[#35B77A]/30'
                        : 'bg-[#272C36] text-[#A0A8B5] border-[#2B313E]'
                    }`}
                  >
                    {rule.enabled ? 'Active' : 'Disabled'}
                  </button>
                </div>

                {/* Extensions Chips */}
                <div className="flex flex-wrap gap-1.5 my-3 min-h-[52px]">
                  {rule.extensions.map((ext) => (
                    <span
                      key={ext}
                      className="inline-flex items-center gap-1 px-2 py-0.5 rounded-lg text-xs bg-[#272C36] text-[#F3F5F8] border border-[#2B313E] group"
                    >
                      <span className="font-mono text-[#6C9BFF]">.{ext}</span>
                      <button
                        onClick={() => handleRemoveExtension(rule, ext)}
                        className="text-[#677182] hover:text-[#E36F75] transition-colors ml-0.5"
                        title={`Remove .${ext}`}
                      >
                        <X size={12} />
                      </button>
                    </span>
                  ))}
                </div>
              </div>

              {/* Add Extension Row & Actions */}
              <div className="pt-3 border-t border-[#2B313E]/60 flex items-center justify-between gap-2">
                <div className="flex items-center gap-1 flex-1">
                  <input
                    type="text"
                    placeholder="+ ext"
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        setNewExtensionInput((e.target as HTMLInputElement).value);
                        handleAddExtension(rule);
                        (e.target as HTMLInputElement).value = '';
                      }
                    }}
                    onChange={(e) => setNewExtensionInput(e.target.value)}
                    className="w-20 bg-[#181B22] border border-[#2B313E] rounded-lg px-2 py-1 text-xs text-[#F3F5F8] placeholder-[#677182] focus:outline-none focus:border-[#6C9BFF]"
                  />
                  <button
                    onClick={() => handleAddExtension(rule)}
                    className="px-2 py-1 rounded-lg text-xs font-semibold bg-[#272C36] hover:bg-[#2f3542] text-[#F3F5F8] border border-[#2B313E] transition-colors"
                  >
                    Add
                  </button>
                </div>

                {rule.isCustom && (
                  <button
                    onClick={() => deleteRule(rule.id)}
                    className="p-1 rounded-lg text-[#A0A8B5] hover:text-[#E36F75] hover:bg-[#E36F75]/10 transition-colors"
                    title="Delete rule"
                  >
                    <Trash2 size={15} />
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* Add Rule Modal */}
      <Modal isOpen={isAddRuleOpen} onClose={() => setIsAddRuleOpen(false)} title="Create Organization Rule">
        <form onSubmit={handleCreateRule} className="space-y-4">
          <div>
            <label className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider block mb-1">
              Category Name
            </label>
            <input
              type="text"
              value={newCategoryName}
              onChange={(e) => setNewCategoryName(e.target.value)}
              placeholder="e.g. Design Assets, Invoices, Ebooks"
              className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl px-3.5 py-2 text-xs text-[#F3F5F8] placeholder-[#677182] focus:outline-none focus:border-[#6C9BFF]"
            />
          </div>

          <div>
            <label className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider block mb-1">
              Target Destination Subfolder
            </label>
            <input
              type="text"
              value={newTargetFolder}
              onChange={(e) => setNewTargetFolder(e.target.value)}
              placeholder="e.g. Design Assets (leave empty to use category name)"
              className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl px-3.5 py-2 text-xs text-[#F3F5F8] placeholder-[#677182] focus:outline-none focus:border-[#6C9BFF]"
            />
          </div>

          <div>
            <label className="text-xs font-semibold text-[#A0A8B5] uppercase tracking-wider block mb-1">
              Extensions (comma-separated)
            </label>
            <input
              type="text"
              value={newExtensionsText}
              onChange={(e) => setNewExtensionsText(e.target.value)}
              placeholder="e.g. figma, sketch, ai, psd, xd"
              className="w-full bg-[#181B22] border border-[#2B313E] rounded-xl px-3.5 py-2 text-xs text-[#F3F5F8] placeholder-[#677182] focus:outline-none focus:border-[#6C9BFF]"
            />
          </div>

          <div className="flex justify-end gap-2.5 pt-3">
            <button
              type="button"
              onClick={() => setIsAddRuleOpen(false)}
              className="px-4 py-2 rounded-xl text-xs font-semibold text-[#A0A8B5] hover:text-[#F3F5F8] hover:bg-[#272C36] transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 rounded-xl text-xs font-semibold bg-[#6C9BFF] text-white hover:bg-[#5287F6] transition-colors shadow-subtle"
            >
              Create Rule
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

