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

interface ErrorBoundaryProps {
  children: React.ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

class PageErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Page render error caught:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="p-8 max-w-xl mx-auto my-12 bg-[#20242D] border border-[#E36F75]/40 rounded-2xl text-center shadow-card">
          <div className="w-12 h-12 rounded-full bg-[#E36F75]/20 text-[#E36F75] mx-auto flex items-center justify-center mb-4 text-xl">
            ⚠️
          </div>
          <h3 className="text-lg font-bold text-[#F3F5F8]">Error Displaying Page</h3>
          <p className="text-xs text-[#A0A8B5] mt-2 mb-5 leading-relaxed">
            {this.state.error?.message || 'An unexpected rendering error occurred in this view.'}
          </p>
          <button
            onClick={() => this.setState({ hasError: false, error: null })}
            className="px-5 py-2.5 bg-[#6C9BFF] text-white rounded-xl text-xs font-semibold hover:bg-[#5287F6] transition-colors shadow-subtle"
          >
            Retry View
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}

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
              initial={{ opacity: 0, y: 4 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -4 }}
              transition={{ duration: 0.12, ease: 'easeOut' }}
              className="h-full"
            >
              <PageErrorBoundary key={currentPage}>
                {renderCurrentPage()}
              </PageErrorBoundary>
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
