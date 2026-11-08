import React, { useEffect, useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAppStore } from './stores/useAppStore';
import { useMonitoringStore } from './stores/useMonitoringStore';
import { Sidebar } from './components/Sidebar';
import { Header } from './components/Header';
import { DashboardPage } from './pages/DashboardPage';
import { FoldersPage } from './pages/FoldersPage';
import { RulesPage } from './pages/RulesPage';
import { ActivityPage } from './pages/ActivityPage';
import { StatisticsPage } from './pages/StatisticsPage';
import { SettingsPage } from './pages/SettingsPage';
import { AboutPage } from './pages/AboutPage';

export const App: React.FC = () => {
  const { currentPage, theme, toastMessage, clearToast } = useAppStore();
  const { fetchInitialData } = useMonitoringStore();

  const [isAddFolderModalOpen, setIsAddFolderModalOpen] = useState(false);

  useEffect(() => {
    // Initial fetch of folders, rules, stats, settings
    fetchInitialData();
  }, []);

  const renderCurrentPage = () => {
    switch (currentPage) {
      case 'overview':
        return <DashboardPage onOpenAddFolder={() => setIsAddFolderModalOpen(true)} />;
      case 'folders':
        return (
          <FoldersPage
            isAddModalOpen={isAddFolderModalOpen}
            setIsAddModalOpen={setIsAddFolderModalOpen}
          />
        );
      case 'rules':
        return <RulesPage />;
      case 'activity':
        return <ActivityPage />;
      case 'statistics':
        return <StatisticsPage />;
      case 'settings':
        return <SettingsPage />;
      case 'about':
        return <AboutPage />;
      default:
        return <DashboardPage onOpenAddFolder={() => setIsAddFolderModalOpen(true)} />;
    }
  };

  return (
    <div className={`flex h-screen w-screen overflow-hidden ${theme === 'dark' ? 'bg-[#111318] text-[#F3F5F8]' : 'bg-[#F8FAFC] text-[#0F172A]'}`}>
      {/* Sidebar Navigation */}
      <Sidebar />

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <Header onAddFolderClick={() => setIsAddFolderModalOpen(true)} />

        <main className="flex-1 overflow-y-auto">
          <AnimatePresence mode="wait">
            <motion.div
              key={currentPage}
              initial={{ opacity: 0, y: 6 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -6 }}
              transition={{ duration: 0.15, ease: 'easeOut' }}
              className="h-full"
            >
              {renderCurrentPage()}
            </motion.div>
          </AnimatePresence>
        </main>
      </div>

      {/* Toast Notification Pill */}
      <AnimatePresence>
        {toastMessage && (
          <motion.div
            initial={{ opacity: 0, y: 20, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            exit={{ opacity: 0, y: 20, scale: 0.95 }}
            onClick={clearToast}
            className={`fixed bottom-5 right-6 z-50 flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-semibold shadow-flyout border cursor-pointer select-none ${
              toastMessage.type === 'success'
                ? 'bg-[#20242D] border-[#35B77A]/40 text-[#35B77A]'
                : toastMessage.type === 'error'
                ? 'bg-[#20242D] border-[#E36F75]/40 text-[#E36F75]'
                : 'bg-[#20242D] border-[#6C9BFF]/40 text-[#6C9BFF]'
            }`}
          >
            <span
              className={`w-2 h-2 rounded-full ${
                toastMessage.type === 'success'
                  ? 'bg-[#35B77A]'
                  : toastMessage.type === 'error'
                  ? 'bg-[#E36F75]'
                  : 'bg-[#6C9BFF]'
              }`}
            />
            <span>{toastMessage.text}</span>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};
export default App;

