import { create } from 'zustand';
import {
  WatchedFolder,
  OrganizationRule,
  Category,
  ActivityLogItem,
  DashboardStatistics,
  ApplicationSettings,
} from '../../../shared/contracts/contracts';
import { bridge } from '../services/bridgeClient';

interface MonitoringStore {
  isMonitoringActive: boolean;
  folders: WatchedFolder[];
  rules: OrganizationRule[];
  categories: Category[];
  statistics: DashboardStatistics | null;
  settings: ApplicationSettings | null;
  isLoading: boolean;
  error: string | null;

  fetchInitialData: () => Promise<void>;
  fetchFolders: () => Promise<void>;
  fetchRules: () => Promise<void>;
  fetchStatistics: () => Promise<void>;
  fetchSettings: () => Promise<void>;

  startMonitoring: () => Promise<void>;
  stopMonitoring: () => Promise<void>;
  pauseMonitoring: () => Promise<void>;
  resumeMonitoring: () => Promise<void>;

  addFolder: (path: string, unknownTarget?: string, quiet?: boolean, recursive?: boolean) => Promise<boolean>;
  updateFolder: (folder: WatchedFolder) => Promise<boolean>;
  removeFolder: (id: string) => Promise<boolean>;
  openNativeFolderPicker: () => Promise<string | null>;

  saveRule: (rule: OrganizationRule) => Promise<boolean>;
  deleteRule: (id: string) => Promise<boolean>;
  resetRulesToDefault: () => Promise<boolean>;

  saveSettings: (settings: ApplicationSettings) => Promise<boolean>;
  createSampleFiles: (folderPath?: string) => Promise<{ created: number; folder: string } | null>;
}

export const useMonitoringStore = create<MonitoringStore>((set, get) => ({
  isMonitoringActive: false,
  folders: [],
  rules: [],
  categories: [],
  statistics: null,
  settings: null,
  isLoading: false,
  error: null,

  fetchInitialData: async () => {
    set({ isLoading: true, error: null });
    try {
      const results = await Promise.allSettled([
        bridge.call<{ isRunning: boolean }>('watcher.getStatus'),
        bridge.call<WatchedFolder[]>('watcher.getFolders'),
        bridge.call<OrganizationRule[]>('watcher.getRules'),
        bridge.call<Category[]>('watcher.getCategories'),
        bridge.call<DashboardStatistics>('watcher.getStatistics'),
        bridge.call<ApplicationSettings>('watcher.getSettings'),
      ]);

      const status = results[0].status === 'fulfilled' ? results[0].value : null;
      const folders = results[1].status === 'fulfilled' ? results[1].value : [];
      const rules = results[2].status === 'fulfilled' ? results[2].value : [];
      const categories = results[3].status === 'fulfilled' ? results[3].value : [];
      const stats = results[4].status === 'fulfilled' ? results[4].value : null;
      const settings = results[5].status === 'fulfilled' ? results[5].value : null;

      set({
        isMonitoringActive: status?.isRunning ?? false,
        folders: folders || [],
        rules: rules || [],
        categories: categories || [],
        statistics: stats || null,
        settings: settings || null,
        isLoading: false,
      });

      // Register event listeners
      bridge.on('FileOrganized', () => {
        get().fetchStatistics();
        get().fetchFolders();
      });

      bridge.on('MonitoringStarted', () => set({ isMonitoringActive: true }));
      bridge.on('MonitoringStopped', () => set({ isMonitoringActive: false }));
    } catch (err: any) {
      set({ error: err.message, isLoading: false });
    }
  },

  fetchFolders: async () => {
    try {
      const folders = await bridge.call<WatchedFolder[]>('watcher.getFolders');
      set({ folders: folders || [] });
    } catch { }
  },

  fetchRules: async () => {
    try {
      const [rules, categories] = await Promise.all([
        bridge.call<OrganizationRule[]>('watcher.getRules'),
        bridge.call<Category[]>('watcher.getCategories'),
      ]);
      set({ rules: rules || [], categories: categories || [] });
    } catch { }
  },

  fetchStatistics: async () => {
    try {
      const stats = await bridge.call<DashboardStatistics>('watcher.getStatistics');
      set({ statistics: stats });
    } catch { }
  },

  fetchSettings: async () => {
    try {
      const settings = await bridge.call<ApplicationSettings>('watcher.getSettings');
      set({ settings });
    } catch { }
  },

  startMonitoring: async () => {
    try {
      await bridge.call('watcher.startMonitoring');
      set({ isMonitoringActive: true });
      get().fetchFolders();
    } catch (err: any) {
      set({ error: err.message });
    }
  },

  stopMonitoring: async () => {
    try {
      await bridge.call('watcher.stopMonitoring');
      set({ isMonitoringActive: false });
      get().fetchFolders();
    } catch (err: any) {
      set({ error: err.message });
    }
  },

  pauseMonitoring: async () => {
    try {
      await bridge.call('watcher.pauseMonitoring');
      set({ isMonitoringActive: false });
    } catch (err: any) {
      set({ error: err.message });
    }
  },

  resumeMonitoring: async () => {
    try {
      await bridge.call('watcher.resumeMonitoring');
      set({ isMonitoringActive: true });
    } catch (err: any) {
      set({ error: err.message });
    }
  },

  addFolder: async (path, unknownTarget = 'Other', quiet = false, recursive = false) => {
    try {
      const newFolder: Partial<WatchedFolder> = {
        path,
        unknownTarget,
        quiet,
        recursive,
        enabled: true,
      };
      await bridge.call('watcher.addFolder', newFolder);
      await get().fetchFolders();
      await get().fetchStatistics();
      return true;
    } catch (err: any) {
      set({ error: err.message });
      return false;
    }
  },

  updateFolder: async (folder) => {
    try {
      await bridge.call('watcher.updateFolder', folder);
      await get().fetchFolders();
      return true;
    } catch (err: any) {
      set({ error: err.message });
      return false;
    }
  },

  removeFolder: async (id) => {
    try {
      await bridge.call('watcher.removeFolder', id);
      await get().fetchFolders();
      await get().fetchStatistics();
      return true;
    } catch (err: any) {
      set({ error: err.message });
      return false;
    }
  },

  openNativeFolderPicker: async () => {
    try {
      const res = await bridge.call<{ path: string | null }>('watcher.openNativeFolderPicker');
      return res?.path ?? null;
    } catch {
      return null;
    }
  },

  saveRule: async (rule) => {
    try {
      await bridge.call('watcher.saveRule', rule);
      await get().fetchRules();
      return true;
    } catch (err: any) {
      set({ error: err.message });
      return false;
    }
  },

  deleteRule: async (id) => {
    try {
      await bridge.call('watcher.deleteRule', id);
      await get().fetchRules();
      return true;
    } catch (err: any) {
      set({ error: err.message });
      return false;
    }
  },

  resetRulesToDefault: async () => {
    try {
      await bridge.call('watcher.resetRulesToDefault');
      await get().fetchRules();
      return true;
    } catch (err: any) {
      set({ error: err.message });
      return false;
    }
  },

  saveSettings: async (settings) => {
    try {
      await bridge.call('watcher.saveSettings', settings);
      set({ settings });
      return true;
    } catch (err: any) {
      set({ error: err.message });
      return false;
    }
  },

  createSampleFiles: async (folderPath) => {
    try {
      const res = await bridge.call<{ created: number; folder: string }>('watcher.createSampleFiles', folderPath || '');
      await get().fetchStatistics();
      return res;
    } catch {
      return null;
    }
  },
}));

