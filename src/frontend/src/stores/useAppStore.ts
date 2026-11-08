import { create } from 'zustand';
import { AppTheme } from '../../../shared/contracts/contracts';

export type NavigationPage =
  | 'overview'
  | 'folders'
  | 'rules'
  | 'activity'
  | 'statistics'
  | 'settings'
  | 'about';

interface AppStore {
  currentPage: NavigationPage;
  theme: AppTheme;
  isSidebarCollapsed: boolean;
  toastMessage: { text: string; type: 'success' | 'error' | 'info' } | null;

  setCurrentPage: (page: NavigationPage) => void;
  setTheme: (theme: AppTheme) => void;
  toggleSidebar: () => void;
  setSidebarCollapsed: (collapsed: boolean) => void;
  showToast: (text: string, type?: 'success' | 'error' | 'info') => void;
  clearToast: () => void;
}

export const useAppStore = create<AppStore>((set) => ({
  currentPage: 'overview',
  theme: 'dark',
  isSidebarCollapsed: false,
  toastMessage: null,

  setCurrentPage: (page) => set({ currentPage: page }),
  setTheme: (theme) => {
    set({ theme });
    if (theme === 'dark') {
      document.documentElement.classList.add('dark');
      document.documentElement.classList.remove('light');
    } else if (theme === 'light') {
      document.documentElement.classList.add('light');
      document.documentElement.classList.remove('dark');
    }
  },
  toggleSidebar: () => set((state) => ({ isSidebarCollapsed: !state.isSidebarCollapsed })),
  setSidebarCollapsed: (collapsed) => set({ isSidebarCollapsed: collapsed }),
  showToast: (text, type = 'info') => {
    set({ toastMessage: { text, type } });
    setTimeout(() => {
      set((state) => (state.toastMessage?.text === text ? { toastMessage: null } : state));
    }, 4000);
  },
  clearToast: () => set({ toastMessage: null }),
}));

