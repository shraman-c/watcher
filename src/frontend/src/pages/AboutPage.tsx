import React from 'react';
import { Eye, ShieldCheck, Cpu, Database, HardDrive, CheckCircle2, Code2 } from 'lucide-react';

export const AboutPage: React.FC = () => {
  return (
    <div className="p-6 space-y-6 max-w-4xl mx-auto">
      {/* Brand Header Card */}
      <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-8 shadow-card flex flex-col sm:flex-row items-center sm:items-start gap-6 text-center sm:text-left">
        <div className="w-20 h-20 rounded-2xl bg-gradient-to-br from-[#6C9BFF] to-[#3B66FF] flex items-center justify-center shadow-[0_8px_24px_rgba(108,155,255,0.4)] flex-shrink-0">
          <Eye size={44} className="text-white" />
        </div>
        <div className="space-y-2">
          <div className="flex flex-wrap items-center justify-center sm:justify-start gap-2.5">
            <h2 className="text-2xl font-bold text-[#F3F5F8] tracking-tight">Watcher</h2>
            <span className="px-2 py-0.5 rounded-full text-xs font-semibold bg-[#6C9BFF]/15 text-[#6C9BFF] border border-[#6C9BFF]/30">
              v2.0.0 Windows Edition
            </span>
          </div>
          <p className="text-xs text-[#A0A8B5] leading-relaxed max-w-xl">
            A high-performance Windows desktop file organization utility built with React, TypeScript, C#, .NET,
            and SQLite. Watcher continuously monitors directories, detects file write completion, and organizes
            files safely without blocking your workflow.
          </p>
          <div className="flex flex-wrap items-center justify-center sm:justify-start gap-3 pt-2 text-[11px] text-[#A0A8B5]">
            <span>Developed by <strong>Shraman Chaudhuri</strong></span>
            <span>•</span>
            <span>License: <strong>MIT</strong></span>
            <span>•</span>
            <span>Platform: <strong>Windows x64</strong></span>
          </div>
        </div>
      </div>

      {/* Architecture Highlights */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card space-y-3">
          <div className="flex items-center gap-2 text-xs font-semibold text-[#6C9BFF] uppercase tracking-wider">
            <Cpu size={16} />
            <span>Desktop & Shell Engine</span>
          </div>
          <ul className="text-xs text-[#A0A8B5] space-y-2">
            <li className="flex items-start gap-2">
              <CheckCircle2 size={14} className="text-[#35B77A] flex-shrink-0 mt-0.5" />
              <span><strong>C# / .NET Host:</strong> Native Windows execution with System.IO.FileSystemWatcher.</span>
            </li>
            <li className="flex items-start gap-2">
              <CheckCircle2 size={14} className="text-[#35B77A] flex-shrink-0 mt-0.5" />
              <span><strong>Microsoft WebView2:</strong> Evergreen chromium shell running React 18 / Vite.</span>
            </li>
            <li className="flex items-start gap-2">
              <CheckCircle2 size={14} className="text-[#35B77A] flex-shrink-0 mt-0.5" />
              <span><strong>System Tray & Mutex:</strong> Background minimization and single-instance guards.</span>
            </li>
          </ul>
        </div>

        <div className="bg-[#20242D] border border-[#2B313E] rounded-2xl p-5 shadow-card space-y-3">
          <div className="flex items-center gap-2 text-xs font-semibold text-[#35B77A] uppercase tracking-wider">
            <ShieldCheck size={16} />
            <span>Safety & Reliability</span>
          </div>
          <ul className="text-xs text-[#A0A8B5] space-y-2">
            <li className="flex items-start gap-2">
              <CheckCircle2 size={14} className="text-[#35B77A] flex-shrink-0 mt-0.5" />
              <span><strong>File Readiness Detection:</strong> Waits for writes, lock release, and download completion.</span>
            </li>
            <li className="flex items-start gap-2">
              <CheckCircle2 size={14} className="text-[#35B77A] flex-shrink-0 mt-0.5" />
              <span><strong>Collision Prevention:</strong> Configurable renaming, replacement, or skip behavior.</span>
            </li>
            <li className="flex items-start gap-2">
              <CheckCircle2 size={14} className="text-[#35B77A] flex-shrink-0 mt-0.5" />
              <span><strong>SQLite Persistence:</strong> Full activity history with instant Undo support.</span>
            </li>
          </ul>
        </div>
      </div>
    </div>
  );
};

