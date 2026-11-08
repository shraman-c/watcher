import {
  BridgeRequest,
  BridgeResponse,
  BridgeEvent,
  WatchedFolder,
  OrganizationRule,
  Category,
  ActivityLogItem,
  DashboardStatistics,
  ApplicationSettings,
} from '../../../shared/contracts/contracts';

type EventHandler = (payload: any) => void;

class BridgeClient {
  private isWebView: boolean = false;
  private pendingRequests: Map<string, { resolve: (data: any) => void; reject: (err: any) => void }> = new Map();
  private eventHandlers: Map<string, Set<EventHandler>> = new Map();

  constructor() {
    this.isWebView = typeof window !== 'undefined' && !!(window as any).chrome?.webview;

    if (this.isWebView) {
      (window as any).chrome.webview.addEventListener('message', (event: any) => {
        this.handleIncomingMessage(event.data);
      });
    } else {
      console.warn('Running outside WebView2 host. Bridge will simulate responses for preview.');
    }
  }

  public get isConnected(): boolean {
    return this.isWebView;
  }

  public on(eventName: string, handler: EventHandler): () => void {
    if (!this.eventHandlers.has(eventName)) {
      this.eventHandlers.set(eventName, new Set());
    }
    this.eventHandlers.get(eventName)!.add(handler);

    return () => {
      this.eventHandlers.get(eventName)?.delete(handler);
    };
  }

  private handleIncomingMessage(raw: any) {
    let message: any = raw;
    if (typeof raw === 'string') {
      try {
        message = JSON.parse(raw);
      } catch {
        return;
      }
    }

    // Check if this is an Event from backend
    if (message.event) {
      const handlers = this.eventHandlers.get(message.event);
      if (handlers) {
        handlers.forEach((h) => h(message.payload));
      }
      return;
    }

    // Check if this is a Response to a Request
    if (message.id && this.pendingRequests.has(message.id)) {
      const { resolve, reject } = this.pendingRequests.get(message.id)!;
      this.pendingRequests.delete(message.id);

      if (message.success) {
        resolve(message.data);
      } else {
        reject(new Error(message.error?.message || 'Bridge request failed'));
      }
    }
  }

  public async call<T = any>(action: string, payload?: any): Promise<T> {
    const id = `req_${Date.now()}_${Math.random().toString(36).substring(2, 9)}`;

    if (!this.isWebView) {
      return this.mockCall<T>(action, payload);
    }

    const request: BridgeRequest = {
      id,
      action,
      payload,
      timestamp: new Date().toISOString(),
    };

    return new Promise<T>((resolve, reject) => {
      const timeout = setTimeout(() => {
        if (this.pendingRequests.has(id)) {
          this.pendingRequests.delete(id);
          reject(new Error(`Bridge timeout for action: ${action}`));
        }
      }, 20000);

      this.pendingRequests.set(id, {
        resolve: (data) => {
          clearTimeout(timeout);
          resolve(data);
        },
        reject: (err) => {
          clearTimeout(timeout);
          reject(err);
        },
      });

      (window as any).chrome.webview.postMessage(JSON.stringify(request));
    });
  }

  // Fallback mock responses when running `npm run dev` directly in browser without WebView2 host
  private async mockCall<T = any>(action: string, payload?: any): Promise<T> {
    await new Promise((r) => setTimeout(r, 120));

    switch (action) {
      case 'watcher.getStatus':
        return { isRunning: true, folderStatuses: {} } as any;

      case 'watcher.getFolders':
        return [
          {
            id: 'mock-1',
            path: 'C:\\Users\\shramanc\\Downloads',
            unknownTarget: 'Other',
            quiet: false,
            enabled: true,
            recursive: false,
            filesOrganizedCount: 42,
            status: 'active',
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
          },
        ] as any;

      case 'watcher.getRules':
        return [
          { id: '1', categoryId: 'cat-doc', categoryName: 'Documents', extensions: ['pdf', 'docx', 'txt'], targetFolder: 'Documents', priority: 1, enabled: true, isCustom: false },
          { id: '2', categoryId: 'cat-img', categoryName: 'Images', extensions: ['jpg', 'png', 'svg'], targetFolder: 'Images', priority: 2, enabled: true, isCustom: false },
        ] as any;

      case 'watcher.getStatistics':
        return {
          activeFoldersCount: 1,
          totalFoldersCount: 1,
          totalFilesOrganized: 128,
          filesOrganizedToday: 14,
          successfulOperations: 125,
          failedOperations: 3,
          categoriesCount: 8,
          isMonitoringActive: true,
          uptimeSeconds: 3600,
          activityOverTime: [
            { date: '2026-11-02', total: 10, success: 10, failed: 0 },
            { date: '2026-11-03', total: 22, success: 22, failed: 0 },
            { date: '2026-11-04', total: 18, success: 17, failed: 1 },
            { date: '2026-11-05', total: 35, success: 34, failed: 1 },
            { date: '2026-11-06', total: 29, success: 28, failed: 1 },
            { date: '2026-11-07', total: 14, success: 14, failed: 0 },
          ],
          categoryBreakdown: [
            { category: 'Documents', count: 45, percentage: 36 },
            { category: 'Images', count: 35, percentage: 28 },
            { category: 'Code', count: 25, percentage: 20 },
            { category: 'Archives', count: 20, percentage: 16 },
          ],
          folderBreakdown: [
            { folderId: 'mock-1', path: 'C:\\Users\\shramanc\\Downloads', count: 128 },
          ],
        } as any;

      case 'watcher.getActivity':
        return {
          items: [
            {
              id: 'log-1',
              fileName: 'contract.pdf',
              extension: 'pdf',
              category: 'Documents',
              sourcePath: 'C:\\Users\\shramanc\\Downloads\\contract.pdf',
              destinationPath: 'C:\\Users\\shramanc\\Downloads\\Documents\\contract.pdf',
              timestamp: new Date().toISOString(),
              status: 'SUCCESS',
              fileSizeBytes: 245000,
              canUndo: true,
            },
          ],
          total: 1,
          page: 1,
          pageSize: 20,
        } as any;

      case 'watcher.getSettings':
        return {
          launchOnStartup: false,
          minimizeToTray: true,
          closeToTray: true,
          startMonitoringOnLaunch: true,
          theme: 'dark',
          compactMode: false,
          notifyOnSuccess: true,
          notifyOnError: true,
          notifyOnMonitoringStatus: true,
          collisionBehavior: 'rename',
          defaultUnknownTarget: 'Other',
          readinessTimeoutMs: 15000,
          readinessPollIntervalMs: 400,
          recursiveDefault: false,
          logLevel: 'INFO',
          autoCleanupDatabaseDays: 30,
        } as any;

      case 'watcher.openNativeFolderPicker':
        return { path: 'C:\\Users\\shramanc\\Downloads' } as any;

      default:
        return true as any;
    }
  }
}

export const bridge = new BridgeClient();

