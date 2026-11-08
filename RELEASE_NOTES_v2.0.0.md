# 🚀 Watcher Version 2.0.0 Release Notes

**Release Date:** November 8, 2026  
**Tag:** `v2.0.0`  
**Distribution Archive:** `dist/Package/Watcher-v2.0.0-win-x64.zip` (2.47 MB)  
**Target Platform:** Windows 10 (Build 19041+) and Windows 11 (x64)

---

## 🌟 Executive Summary

Watcher 2.0 is a complete reimagining and architectural overhaul of the Watcher desktop application. It transforms the legacy single-file Python/PyQt6 prototype into a native Windows desktop utility combining high-performance C#/.NET 9 backend monitoring, SQLite ACID persistence with Write-Ahead Logging (WAL), and a modern Windows 11 Fluent 2.0 React interface hosted inside Microsoft Edge WebView2.

---

## 📦 What's New in Version 2.0.0

### 1. 🎨 Fluent Design 2.0 User Interface
- **Windows 11 Fluent Charcoal Theme:** Sleek dark mode (`#111318` canvas, `#181B22` sidebar, `#20242D` cards, `#6C9BFF` accent) with smooth transitions, design tokens, and a clean light theme toggle.
- **Collapsible Sidebar:** Sidebar with live monitoring state indicator and badges.
- **Overview Dashboard:** Live telemetry featuring 6 real-time metrics, interactive Recharts organization timeline, and real-time recent movement stream.
- **Watched Folders Management:** Folder cards showing full normalized paths, recursive toggle, quiet mode, and organized file counters. Native Windows folder browser dialogs powered by the C# backend.
- **Organization Rules & Match Simulator:** Full rule editor with interactive extension chips, category priority re-ordering, and a **Live File Match Simulator** to test filenames before moving files.
- **Searchable Activity History:** Instant search, category filters, status filters, pagination, "Reveal in File Explorer" (`explorer.exe /select,"<path>"`), and **One-Click Instant Undo**.
- **Visual Analytics:** Interactive daily volume trend charts, category distribution bars, and per-folder workload shares.

### 2. 🛡️ Robust File Monitoring Engine (`Watcher.Core`)
- **File Readiness Detection:** Multi-stage size stability polling and lock verification ensure files are not moved until downloads (.crdownload, .part) or large writes have completely finished.
- **Temporary File Blacklist:** Automatically suppresses transient downloading and cache files (`.crdownload`, `.part`, `.tmp`, `~$*`).
- **File Collision Strategies:** User-configurable collision resolution supporting numbered renaming (e.g., `report (1).pdf`), explicit overwrite, or safe skipping.
- **Infinite Loop Guard:** Validates destination subdirectories against watched paths to prevent recursive organization cycles.
- **Asynchronous Event Pipeline:** Event debouncing and background processing over bounded `System.Threading.Channels` prevents UI lockups and filesystem buffer overflows.

### 3. ⚙️ Deep Windows Shell Integration (`Watcher.Desktop` & `Watcher.Infrastructure`)
- **System Tray Icon (`NotifyIcon`):** Runs quietly in the notification area with a context menu to *Open*, *Start*, *Pause*, *Resume*, and *Exit*.
- **Close to Tray / Minimize to Tray:** Configurable window behaviors for uninterrupted background operation.
- **Windows Notifications:** Native balloon alerts for organized files and drive accessibility warnings.
- **Single-Instance Mutex:** Prevents duplicate running instances; launching another instance brings the existing window to the foreground.
- **Launch on Windows Startup:** Registry integration (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`) with background launch mode.

### 4. 🔄 Zero-Data-Loss Migration Policy
- Automatically imports legacy user folders and custom rule text from `~/.watcher_config.json` and `./settings.json` on first run without modifying or deleting original files.
- Safe backups (`*.v1.bak`) preserved.

### 5. 🧪 Quality Assurance & Test Coverage
- **100% Passing Automated Tests:** 21 automated unit tests across `Watcher.Core.Tests` and `Watcher.Infrastructure.Tests`.
- **Offline Packaging:** All frontend assets, SQLite native binaries (`e_sqlite3.dll`), and WebView2 loaders are bundled directly into the standalone distribution archive.

---

## 📂 Release Assets

| Asset Name | Architecture | Type | Size | Description |
| :--- | :--- | :--- | :--- | :--- |
| **`Watcher-v2.0.0-win-x64.zip`** | Windows x64 | Standalone Portable Archive | ~2.47 MB | Complete portable release — extract and run `Watcher.exe` |
| **`AppxManifest.xml`** | Windows x64 | MSIX Packaging Manifest | ~1.4 KB | Windows App SDK / Store deployment manifest |

---

## 🚀 Installation & Getting Started

1. Download **`Watcher-v2.0.0-win-x64.zip`** from `dist/Package/`.
2. Extract the contents to your preferred folder (e.g., `C:\Program Files\Watcher` or `%USERPROFILE%\AppData\Local\Programs\Watcher`).
3. Run **`Watcher.exe`**.
4. The application will initialize your local SQLite database at `%LOCALAPPDATA%\Watcher\watcher.db` and import any existing Watcher 1.0 configurations automatically.

---

## 📚 Complete Documentation

- [README.md](README.md) — Main repository documentation and quick start.
- [Changelog](CHANGELOG.md) — Version-by-version change log.
- [Architecture Guide](docs/ARCHITECTURE.md) — System architecture, sequence diagrams, and threat model.
- [Migration Guide](docs/MIGRATION_GUIDE.md) — Zero-data-loss configuration upgrade from Watcher 1.0.
- [Build and Packaging Guide](docs/BUILD_AND_PACKAGING.md) — Local builds, testing, and distribution packaging.
