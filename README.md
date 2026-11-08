<div align="center">

# 📂 Watcher 2.0 — Modern Windows Desktop File Organizer

[![Platform: Windows x64](https://img.shields.io/badge/Platform-Windows%20x64-0078D6?style=for-the-badge&logo=windows)](https://github.com/shraman-c/watcher)
[![C# / .NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18-61DAFB?style=for-the-badge&logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?style=for-the-badge&logo=typescript)](https://www.typescriptlang.org/)
[![Vite](https://img.shields.io/badge/Vite-5-646CFF?style=for-the-badge&logo=vite)](https://vitejs.dev/)
[![TailwindCSS](https://img.shields.io/badge/TailwindCSS-3.4-38B2AC?style=for-the-badge&logo=tailwind-css)](https://tailwindcss.com/)
[![SQLite](https://img.shields.io/badge/SQLite-WAL%20Mode-003B57?style=for-the-badge&logo=sqlite)](https://www.sqlite.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-35B77A?style=for-the-badge)](LICENSE)

**An intelligent, high-performance desktop file organization utility for Windows.**  
Watcher monitors designated directories in real-time, waits for downloads and copies to finish safely, and automatically sorts files into organized categories.

[Features](#-features--capabilities) • [Comparison (1.0 vs 2.0)](#-watcher-10-vs-watcher-20) • [Architecture](#-architecture) • [Getting Started](#-getting-started) • [Documentation](#-documentation-index)

</div>

---

## 📋 Table of Contents
- [✨ Features \& Capabilities](#-features--capabilities)
- [⚖️ Watcher 1.0 vs Watcher 2.0](#️-watcher-10-vs-watcher-20)
- [🏛️ Architecture](#️-architecture)
- [🖥️ Application Views](#️-application-views)
- [📂 Default Categories](#-default-categories)
- [🚀 Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Download \& Run](#download--run)
  - [Building from Source](#building-from-source)
- [🧪 Automated Testing](#-automated-testing)
- [📚 Documentation Index](#-documentation-index)
- [📄 License](#-license)

---

## ✨ Features & Capabilities

### 🎨 Windows 11 Fluent 2.0 UI
- **Dark Charcoal Theme**: Modern dark aesthetic (`#111318` background, `#181B22` sidebar, `#20242D` cards, `#6C9BFF` accent).
- **Responsive Layout**: Collapsible sidebar, animated page transitions with Framer Motion, and design tokens for dark and light modes.
- **Visual Analytics**: Interactive Recharts area, bar, and distribution charts powered by real SQLite queries.
- **Searchable Activity Table**: Powered by TanStack Table with search, category filtering, status filtering, and pagination.

### ⚙️ Native Windows Shell Integration
- **System Tray Icon (`NotifyIcon`)**: Runs quietly in the notification area with a context menu to *Open*, *Start*, *Pause*, *Resume*, and *Exit*.
- **Close to Tray**: Closing the main window continues background monitoring without interruption.
- **Native Notifications**: Windows balloon alerts when files are organized or when directory access warnings occur.
- **Single-Instance Mutex**: Prevents duplicate instances; launching another instance activates and focuses the existing window.
- **Windows Startup**: Optional auto-launch on login via `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- **Native Folder Browser**: Uses Windows native folder dialogs through C# rather than browser prompts.

### 🛡️ Safety & Reliability Engine
- **File Readiness Detection**: Verifies that new downloads or large file copies (e.g. videos, zip files) have finished writing and unlocked before attempting to move them.
- **Temporary File Filtering**: Automatically ignores transient files (`.crdownload`, `.part`, `.tmp`, `~$*`) until fully finalized.
- **Collision Strategies**: Supports automatic numbering (`file (1).ext`), replace, or skip to ensure user files are never silently overwritten.
- **Infinite Loop Guard**: Prevents recursive loop cycles when destination directories reside inside monitored paths.
- **One-Click Undo**: Every file movement is logged with full source/destination paths, allowing instant revert if needed.

### 🔄 Zero-Data-Loss Migration
- Automatically migrates existing user configuration from `~/.watcher_config.json` and `./settings.json` into SQLite without modifying or deleting original files.

---

## ⚖️ Watcher 1.0 vs Watcher 2.0

| Feature | Watcher 1.0 (Original) | Watcher 2.0 (Modernized) |
| :--- | :--- | :--- |
| **Technology Stack** | Python 3, PyQt6, Watchdog | **C#/.NET 9, Microsoft WebView2, React 18, TypeScript, SQLite** |
| **User Interface** | Monolithic Qt widgets, static CSS | **Fluent Design 2.0, TailwindCSS, Framer Motion, TanStack Table, Recharts** |
| **File Readiness Check** | ❌ None (moved immediately upon creation, crashing on active downloads) | ✅ **Bounded size stability detector, lock check, and incomplete file filter** |
| **File Collision Handling** | ❌ None (`file.rename` failed or overwrote) | ✅ **Configurable: Numbered suffix `file (1).ext`, Skip, or Replace** |
| **Infinite Loop Guard** | ❌ Basic top-level only | ✅ **Strict cyclic path validation and sub-destination exclusion checks** |
| **Activity History** | ⚠️ Volatile (in-memory `QTextEdit`, lost on close) | ✅ **Persistent SQLite database with search, pagination, and filters** |
| **Undo Capability** | ❌ None | ✅ **One-click instant Undo for any organized file** |
| **System Tray & Lifecycle** | ⚠️ Simulated batch files (`start-background.bat`) | ✅ **Native `NotifyIcon`, Close-to-Tray, Minimize-to-Tray, Single Instance Mutex** |
| **Architecture** | 885-line single file `gui.py` | **Clean modular monorepo (`Watcher.Core`, `Watcher.Infrastructure`, `Watcher.Desktop`, `frontend`)** |
| **Test Coverage** | None | ✅ **21 Automated unit tests across Core & Infrastructure** |

---

## 🏛️ Architecture

Watcher 2.0 uses a modular monorepo architecture:

```
Watcher/
├── src/
│   ├── frontend/                         # React 18 + TypeScript + Vite + TailwindCSS
│   │   ├── src/
│   │   │   ├── components/              # Sidebar, Header, MetricCard, StatusBadge, Modal
│   │   │   ├── pages/                   # Overview, Folders, Rules, Activity, Statistics, Settings, About
│   │   │   ├── stores/                  # Zustand stores (useAppStore, useMonitoringStore)
│   │   │   ├── services/                # Typed WebView2 RPC bridge client (bridgeClient.ts)
│   │   │   └── styles/                  # Tailwind & Fluent CSS design tokens
│   │   ├── vite.config.ts
│   │   └── package.json
│   │
│   ├── desktop/
│   │   ├── Watcher.Desktop/             # WebView2 host, System Tray, Single-Instance Mutex, Program.cs
│   │   ├── Watcher.Core/                # Domain models, FileReadinessDetector, RuleEngine, FolderMonitor
│   │   └── Watcher.Infrastructure/      # SQLite DatabaseContext (WAL), Repositories, LegacyConfigMigrator
│   │
│   └── shared/
│       └── contracts/                   # Shared TypeScript / C# DTOs and bridge protocol contracts
│
├── tests/
│   ├── Watcher.Core.Tests/              # Unit tests for readiness, collision, rules, loop guard
│   └── Watcher.Infrastructure.Tests/    # SQLite persistence & migration tests
│
├── scripts/
│   ├── build.ps1                        # Monorepo build and packaging pipeline
│   ├── create-msix.ps1                  # MSIX / zip packaging automation
│   └── AppxManifest.xml                 # Windows App package manifest
├── dist/                                # Published binaries & packages
└── Watcher.sln                          # .NET Solution linking all projects
```

---

## 🖥️ Application Views

- **Overview Dashboard**: High-level telemetry displaying active watched folders, total files organized, files organized today, categorization reliability rate, an interactive Recharts volume timeline, and live activity feed.
- **Watched Folders**: Add folders via the native Windows folder picker, configure recursive scanning, customize fallback unknown targets, enable quiet mode, and track per-folder organized counts.
- **Organization Rules & Match Simulator**: Manage categories and extensions with interactive chips. Includes a **Live File Match Simulator** that tests any filename against priority rules in real-time.
- **Activity History**: Searchable database of all file movements with filters by category and status. Includes **"Reveal in File Explorer"** and **One-Click Undo**.
- **Statistics & Analytics**: Visual insights showing daily volume trends, category breakdowns, and per-folder workloads.
- **Settings**: Control Windows startup, tray behavior (*Close to Tray / Minimize to Tray*), desktop notification alerts, collision strategies (*Rename / Skip / Replace*), and readiness timeouts.
- **About**: System diagnostics, version details, and architecture information.

---

## 📂 Default Categories

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

## 🚀 Getting Started

### Prerequisites
- **Operating System**: Windows 10 (Build 19041+) or Windows 11 (x64)
- **.NET Runtime**: .NET 9.0 (included in Windows or installed via Microsoft)
- **Edge WebView2 Runtime**: Pre-installed on Windows 10/11

### Download & Run
1. Download the latest standalone release: **`dist/Package/Watcher-v2.0.0-win-x64.zip`**
2. Extract the archive to any folder (e.g. `C:\Program Files\Watcher` or your user folder).
3. Double-click **`Watcher.exe`** to launch.
4. *No installation or development servers required. Runs completely offline.*

### Building from Source

#### Automated Pipeline
Run the automated PowerShell build script:
```powershell
.\scripts\build.ps1
```

#### Manual Build
```powershell
# 1. Build Frontend
cd src/frontend
npm install
npm run build
cd ../..

# 2. Run Tests
dotnet test Watcher.sln -c Release

# 3. Publish Native Desktop Application
dotnet publish src/desktop/Watcher.Desktop/Watcher.Desktop.csproj -c Release -r win-x64 --self-contained false -o dist/Watcher-v2.0.0-win-x64

# 4. Package distribution bundle
.\scripts\create-msix.ps1
```

---

## 🧪 Automated Testing

Watcher 2.0 includes automated unit test suites targeting `Watcher.Core` and `Watcher.Infrastructure`:

```powershell
dotnet test Watcher.sln -c Release
```

```text
Test run for Watcher.Core.Tests.dll (.NETCoreApp,Version=v9.0)
Passed!  - Failed: 0, Passed: 18, Skipped: 0, Total: 18, Duration: 1 s

Test run for Watcher.Infrastructure.Tests.dll (.NETCoreApp,Version=v9.0)
Passed!  - Failed: 0, Passed:  3, Skipped: 0, Total:  3, Duration: 10 s

Total: 21 Passed, 0 Failed (100% Pass Rate)
```

---

## 📚 Documentation Index

- [Architecture Reference](docs/ARCHITECTURE.md) — Comprehensive architectural breakdown, threat model, and sequence flows.
- [Migration Guide](docs/MIGRATION_GUIDE.md) — Detailed guide on zero-data-loss configuration migration from Watcher 1.0.
- [Build and Packaging Guide](docs/BUILD_AND_PACKAGING.md) — Complete compilation, offline bundling, and MSIX packaging documentation.
- [Changelog](CHANGELOG.md) — Chronological history of all releases and changes.

---

## 📄 License

Watcher 2.0 is open-source software licensed under the [MIT License](LICENSE).  
Copyright (c) 2026 Shraman Chaudhuri.
