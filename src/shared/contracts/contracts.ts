/**
 * Shared Type Contracts between Frontend (React/TS) and Desktop Backend (C#/.NET)
 */

export interface BridgeRequest<T = any> {
  id: string;
  action: string;
  payload?: T;
  timestamp: string;
}

export interface BridgeError {
  code: string;
  message: string;
  details?: string;
}

export interface BridgeResponse<T = any> {
  id: string;
  success: boolean;
  data?: T;
  error?: BridgeError;
}

export interface BridgeEvent<T = any> {
  event: string;
  payload: T;
  timestamp: string;
}

export interface WatchedFolder {
  id: string;
  path: string;
  unknownTarget: string;
  quiet: boolean;
  enabled: boolean;
  recursive: boolean;
  filesOrganizedCount: number;
  status: 'active' | 'paused' | 'error' | 'not_found';
  createdAt: string;
  updatedAt: string;
}

export interface OrganizationRule {
  id: string;
  categoryId: string;
  categoryName: string;
  extensions: string[];
  targetFolder: string;
  priority: number;
  enabled: boolean;
  isCustom: boolean;
}

export interface Category {
  id: string;
  name: string;
  defaultFolder: string;
  extensions: string[];
  color: string;
  iconName: string;
  isSystem: boolean;
}

export type OperationStatus = 'SUCCESS' | 'FAILED' | 'SKIPPED';

export interface ActivityLogItem {
  id: string;
  fileName: string;
  extension: string;
  category: string;
  sourcePath: string;
  destinationPath: string;
  timestamp: string;
  status: OperationStatus;
  errorMessage?: string;
  fileSizeBytes?: number;
  canUndo: boolean;
}

export interface ActivityOverTimeItem {
  date: string;
  total: number;
  success: number;
  failed: number;
}

export interface CategoryStatItem {
  category: string;
  count: number;
  percentage: number;
}

export interface FolderStatItem {
  folderId: string;
  path: string;
  count: number;
}

export interface DashboardStatistics {
  activeFoldersCount: number;
  totalFoldersCount: number;
  totalFilesOrganized: number;
  filesOrganizedToday: number;
  successfulOperations: number;
  failedOperations: number;
  categoriesCount: number;
  isMonitoringActive: boolean;
  uptimeSeconds: number;
  activityOverTime: ActivityOverTimeItem[];
  categoryBreakdown: CategoryStatItem[];
  folderBreakdown: FolderStatItem[];
}

export type CollisionBehavior = 'rename' | 'skip' | 'replace';
export type AppTheme = 'system' | 'dark' | 'light';

export interface ApplicationSettings {
  // General
  launchOnStartup: boolean;
  minimizeToTray: boolean;
  closeToTray: boolean;
  startMonitoringOnLaunch: boolean;

  // Appearance
  theme: AppTheme;
  compactMode: boolean;

  // Notifications
  notifyOnSuccess: boolean;
  notifyOnError: boolean;
  notifyOnMonitoringStatus: boolean;

  // Organization
  collisionBehavior: CollisionBehavior;
  defaultUnknownTarget: string;
  readinessTimeoutMs: number;
  readinessPollIntervalMs: number;
  recursiveDefault: boolean;

  // Advanced
  logLevel: 'DEBUG' | 'INFO' | 'WARN' | 'ERROR';
  autoCleanupDatabaseDays: number;
}

