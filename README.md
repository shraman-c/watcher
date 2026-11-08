# 📂 Watcher 2.0 — Modern Windows Desktop File Organizer

[![Platform: Windows x64](https://img.shields.io/badge/Platform-Windows%20x64-0078D6?style=for-the-badge&logo=windows)](https://github.com/shraman-c/watcher)
[![C# / .NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-61DAFB?style=for-the-badge&logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?style=for-the-badge&logo=typescript)](https://www.typescriptlang.org/)
[![Vite](https://img.shields.io/badge/Vite-5-646CFF?style=for-the-badge&logo=vite)](https://vitejs.dev/)
[![TailwindCSS](https://img.shields.io/badge/TailwindCSS-3.4-38B2AC?style=for-the-badge&logo=tailwind-css)](https://tailwindcss.com/)
[![SQLite](https://img.shields.io/badge/SQLite-WAL%20Mode-003B57?style=for-the-badge&logo=sqlite)](https://www.sqlite.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-35B77A?style=for-the-badge)](LICENSE)

**Watcher 2.0** is an enterprise-grade, high-performance Windows desktop application that automatically monitors your Downloads and other folders, detects when files have finished downloading or copying, and organizes them by file type into clean categories in real-time.

Completely re-engineered from the original Python/PyQt6 prototype into a native Windows application with **WebView2**, **C#/.NET 9**, **React 18**, and **SQLite**.

---

## ✨ Features & Enhancements

### 🎨 Fluent Design 2.0 User Interface
- **Dark Charcoal Theme**: Modern Windows 11 Fluent aesthetic (`#111318` background, `#181B22` sidebar, `#20242D` cards, `#6C9BFF` accent).
- **Responsive Layout**: Collapsible sidebar, animated page transitions with Framer Motion, and consistent typography.
- **Visual Analytics**: Interactive Recharts graphs showing files organized over time and category volume distribution.
- **Searchable Activity Table**: Powered by TanStack Table with search, category filtering, status filtering, and pagination.

### ⚙️ Native Windows Shell Integration
- **System Tray Icon**: Complete notification-area integration (`NotifyIcon`) with quick menu (Open, Start, Pause, Resume, Exit).
- **Close to Tray**: Keeps running in the background when closing the main window without interrupting active monitoring.
- **Windows Notifications**: Native balloons alerting you when files are organized or if folder access issues occur.
- **Single Instance Guard**: Prevents duplicate processes; activating a second instance restores and focuses the existing window.
- **Windows Startup**: Optional auto-launch on Windows login via registry integration.
- **Native Folder Browser**: Uses Windows native folder dialogs through C# rather than browser prompts.

### 🛡️ Industrial-Strength Monitoring Engine
- **Multi-Directory Monitoring**: Simultaneously watches multiple distinct drives and folders using `System.IO.FileSystemWatcher`.
- **File Readiness Detection**: Verifies that new downloads or large file copies (e.g. videos, zip files) have finished writing and unlocked before attempting to move them.
- **Temporary File Filtering**: Automatically ignores transient files (`.crdownload`, `.part`, `.tmp`, `~$*`) until fully finalized.
- **Collision Strategies**: Supports automatic numbering (`file (1).ext`), replace, or skip to ensure user files are never silently overwritten.
- **Infinite Loop Guard**: Prevents recursive loop cycles when destination directories reside inside monitored paths.
- **One-Click Undo**: Every file movement is logged with full source/destination paths, allowing instant revert if needed.

### 🔄 Automatic Watcher 1.0 Data Migration
- Seamlessly reads existing user configuration (`~/.watcher_config.json` and `./settings.json`).
- Automatically migrates existing watched folders and custom rule text into SQLite without modifying or deleting original files.

---

## 🏗️ Architecture & Monorepo Structure

```
Watcher/
├── src/
│   ├── frontend/                         # React 18 + TypeScript + Vite + TailwindCSS
│   │   ├── src/
│   │   │   ├── components/              # Sidebar, Header, MetricCard, StatusBadge, Modal
│   │   │   ├── pages/                   # Overview, WatchedFolders, Rules, Activity, Stats, Settings, About
│   │   │   ├── stores/                  # Zustand stores (useAppStore, useMonitoringStore)
│   │   │   ├── services/                # Strongly-typed WebView2 bridge client (bridgeClient.ts)
│   │   │   └── styles/                  # Tailwind & Fluent CSS
│   │   ├── vite.config.ts
│   │   └── package.json
│   │
│   ├── desktop/
│   │   ├── Watcher.Desktop/             # WinUI/WinForms WebView2 host, System Tray, Program.cs
│   │   ├── Watcher.Core/                # Domain models, FileReadinessDetector, RuleEngine, FolderMonitor
│   │   └── Watcher.Infrastructure/      # SQLite DatabaseContext, Repositories, LegacyConfigMigrator
│   │
│   └── shared/
│       └── contracts/                   # Shared DTO and RPC protocol interfaces
│
├── tests/
│   ├── Watcher.Core.Tests/              # 18 Unit tests for readiness, collision, rules, loop guard
│   └── Watcher.Infrastructure.Tests/    # SQLite persistence & settings repository tests
│
├── scripts/
│   ├── build.ps1                        # Monorepo build and packaging pipeline
│   ├── create-msix.ps1                  # MSIX / zip packaging automation
│   └── AppxManifest.xml                 # Windows App package manifest
├── dist/                                # Published binaries & packages
└── Watcher.sln                          # .NET Solution linking all projects
```

---

## 🚀 Quick Start & Development

### Prerequisites
- Windows 10 (Build 19041+) or Windows 11
- [.NET 9.0 SDK](https://dotnet.microsoft.com/)
- [Node.js v18+](https://nodejs.org/)
- Microsoft Edge WebView2 Runtime (pre-installed on modern Windows 10/11)

### 1. Run Automated Tests
```powershell
dotnet test Watcher.sln
```
*All 21 unit tests across Core and Infrastructure pass with 0 errors.*

### 2. Frontend Development Server
```powershell
cd src/frontend
npm install
npm run dev
```

### 3. Build & Package Production Release
Run the automated build script from PowerShell:
```powershell
.\scripts\build.ps1
```
This performs:
1. Production bundle of the React/TypeScript frontend (`tsc && vite build`).
2. Verification via automated unit tests.
3. Native Windows x64 release compilation (`dotnet publish -r win-x64`).
4. Embeds the production frontend directly into the application output folder at `dist/Watcher-v2.0.0-win-x64/`.

To package into a distribution zip / MSIX:
```powershell
.\scripts\create-msix.ps1
```

---

## 📂 Default Organization Categories

| Category | Default Extensions | Target Folder |
| :--- | :--- | :--- |
| **Documents** | `pdf, doc, docx, txt, rtf, odt` | `Documents/` |
| **Spreadsheets** | `xls, xlsx, csv` | `Spreadsheets/` |
| **Images** | `jpg, jpeg, png, gif, bmp, svg, webp` | `Images/` |
| **Videos** | `mp4, mkv, avi, mov, webm` | `Videos/` |
| **Audio** | `mp3, wav, flac, aac, ogg` | `Audio/` |
| **Archives** | `zip, rar, 7z, tar, gz` | `Archives/` |
| **Executables** | `exe, msi, msix` | `Executables/` |
| **Code** | `py, js, ts, html, css, cpp, c, java, cs` | `Code/` |
| **Fonts** | `ttf, otf, woff, woff2` | `Fonts/` |
| **Other** | *(Any unrecognized extension)* | `Other/` |

*All categories and extensions can be added, customized, renamed, or deleted in the **Organization Rules** tab.*

---

## 📄 License

Watcher 2.0 is licensed under the [MIT License](LICENSE).
Copyright (c) 2026 Shraman Chaudhuri.
