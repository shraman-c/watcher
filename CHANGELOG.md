# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.0.0] - 2026-11-08

### 🚀 Complete Platform Re-engineering
- **Native Windows Desktop Architecture**: Fully rewritten from Python/PyQt6 into a high-performance Windows desktop application utilizing **Microsoft.Web.WebView2**, **C#/.NET 9**, **React 18**, and **SQLite**.
- **Modular Monorepo Structure**:
  - `src/frontend`: React 18, TypeScript, Vite, TailwindCSS, Framer Motion, Zustand, TanStack Table, Recharts.
  - `src/desktop/Watcher.Desktop`: Desktop host, WebView2 bridge router, System Tray icon, single-instance mutex.
  - `src/desktop/Watcher.Core`: Domain models, rule evaluation engine, file readiness detector, collision resolver, loop guard.
  - `src/desktop/Watcher.Infrastructure`: SQLite database context with WAL mode, repositories, Windows notifications, and startup integration.
  - `src/shared/contracts`: Shared TypeScript and C# data contracts and bridge protocol interfaces.

### ✨ Added
- **Fluent Design 2.0 User Interface**:
  - Windows 11 Fluent dark charcoal aesthetic (`#111318` background, `#181B22` sidebar, `#20242D` cards, `#6C9BFF` accent).
  - Light theme toggle with design token transitions.
  - Collapsible sidebar with real-time engine activity status.
  - Framer Motion page navigation and animated dialog modals.
- **Overview Dashboard**:
  - 6 real-time metrics (Active Folders, Total Files, Files Today, Categories, Success Rate, Lock/Skip Issues).
  - Engine status card with live Start/Pause controls.
  - Quick action buttons (Add Folder, Edit Rules, Seed Test Files, Activity History).
  - Interactive Recharts volume timeline over time.
  - Real-time recent file organization feed.
- **Watched Folders Management**:
  - Native Windows folder selection dialog powered by C# backend (`FolderBrowserDialog`).
  - Folder cards displaying full path, status, fallback target, recursive mode, and quiet mode.
  - Individual folder enable/disable toggling and removal with confirmation dialogs.
  - Organized file counter tracking per directory.
- **Organization Rules & Match Simulator**:
  - Full rule editor supporting custom categories and priorities.
  - Interactive extension chips with instant addition and removal.
  - Live File Match Preview simulator to test any filename against active rules in real-time.
  - Out-of-the-box expanded categories (Documents, Spreadsheets, Images, Videos, Audio, Archives, Executables, Code, Fonts).
  - One-click Restore Defaults.
- **Searchable Activity History**:
  - Search by filename, source, or target destination.
  - Category and status filters with pagination.
  - "Reveal in File Explorer" button (`explorer.exe /select,"path"`).
  - Clipboard file path copy with feedback.
  - **Instant Undo**: Move any organized file back to its original location with database status update.
  - Clear history with confirmation.
- **Visual Analytics**:
  - Daily organization volume timeline.
  - Category volume horizontal bar chart.
  - Per-folder workload distribution chart.
- **System Tray & Windows Lifecycle**:
  - Native Windows notification area (`NotifyIcon`) with custom tray menu (Open, Start, Pause, Resume, Exit).
  - "Close to Tray" keeps monitoring active in background when window is closed.
  - "Minimize to Tray" hides taskbar item when minimized.
  - Windows Balloon Notifications for organized files and drive warnings.
  - Single-instance mutex (`Global\WatcherAppSingleInstanceMutex`) restoring existing window on re-launch.
  - "Launch on Windows Startup" registry integration (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
- **File Readiness & Safety Engine**:
  - `FileReadinessDetector`: Verifies write completion and lock release before moving; eliminates download corruption.
  - Ignored temporary formats: `.crdownload`, `.part`, `.tmp`, `.partial`, `.download`, `~$*`.
  - Configurable collision behavior (*Rename with numbered suffix*, *Skip*, or *Replace*).
  - `InfiniteLoopGuard`: Detects and prevents circular movement into monitored parent folders.
- **SQLite Persistence & Migration**:
  - SQLite database initialized in `%LOCALAPPDATA%\Watcher\watcher.db` with WAL mode and indexing.
  - `LegacyConfigMigrator`: Automatically imports legacy v1 configuration from `~/.watcher_config.json` and `./settings.json` on first run without modifying or deleting original files.
- **Testing & Packaging**:
  - 21 automated unit tests across `Watcher.Core.Tests` and `Watcher.Infrastructure.Tests`.
  - PowerShell build script (`scripts/build.ps1`).
  - MSIX and standalone distribution packaging script (`scripts/create-msix.ps1`).

---

## [1.1.0] - 2026-05-28

### Added

- **GUI Application** – Modern tkinter-based graphical interface for managing watched folders
  - Folders table with inline editing and deletion
  - Real-time status indicator (Running/Idle)
  - Start/Stop watchers controls
  - Test file creator for validation
  
- **File Organization** – Smart categorization engine
  - Default rules for 8+ file types (Images, Videos, Audio, Documents, Archives, Code, Installers, Fonts)
  - Custom rules editor with line-based format
  - Per-folder settings (unknown file handling, quiet mode)
  - Safe non-recursive watching (top-level files only)

- **CLI Support** – Command-line interface for automation
  - `--path` – Watch multiple directories
  - `--unknown` – Set default folder for unrecognized file types
  - `--quiet` – Reduce logging to errors only
  - `--rules` – Custom JSON-based categorization rules
  - `--create-test` – Generate sample files for testing

- **Configuration** – Persistent settings management
  - `settings.json` – Auto-saved folder list and rules
  - Per-folder metadata (quiet mode, unknown file target)
  - JSON format for programmatic access

- **Icon Support** – Multi-layer icon system
  - Embedded base64 PNG icon (no external files needed)
  - Environment variable fallback (`WATCHER_ICON_BASE64`)
  - Settings file fallback (`icon_base64` field)
  - `.ico` and `.png` file support
  - Custom base64 file support (`.b64`)

- **Dependency Management** – Auto-install on startup
  - Automatic watchdog installation if missing
  - Requirements file validation
  - Graceful error handling with user feedback

- **Windows Packaging** – Standalone executable
  - PyInstaller-based single-file exe packaging
  - Embedded version metadata (Company, Description, Version)
  - Multi-size icon embedding (16x16 to 256x256)
  - Portable distribution with no installation required

- **File Monitoring** – Watchdog-based real-time detection
  - Asynchronous file system event handling
  - Multi-folder concurrent watching
  - Safe file move operations with error recovery
  - Graceful watcher shutdown on interrupt

### Fixed

- Missing `watchdog` module errors – now auto-installs on first run
- Icon display inconsistencies – converted PNG to BMP-only multi-size ICO format
- Windows Explorer icon caching issues – fresh builds with new icon paths

### Changed

- **Window Title** – Updated to include version: `Watcher - Automate Organization v1.00001a`
- **Metadata** – EXE properties now reflect:
  - Company: ShramanC
  - Description: Watcher - Automate Organization
  - Version: 1.0.0.1 Alpha

### Technical Details

- **Dependencies**
  - watchdog ≥4.0.0 (file system monitoring)
  - pystray ≥0.19.0 (system tray integration)
  - Pillow ≥10.0.0 (icon processing)
  - tkinter (GUI; bundled with Python)
  - PyInstaller (packaging; dev only)

- **Platform Support**
  - ✅ Windows (primary, tested)
  - 🔄 macOS (supported via code, not tested)
  - 🔄 Linux (supported via code, not tested)

- **Python Version**
  - Minimum: Python 3.9+
  - Tested with: Python 3.14.2

### Known Issues

- Windows Explorer icon cache may persist old icons
  - **Workaround**: Rename exe file or delete `%LocalAppData%\IconCache.db`
- Large folder structures with many files may cause initial lag during watching
  - **Note**: App uses non-recursive watching by design; subfolders unaffected

### Installation & Usage

**Windows (Easiest)**
1. Download `Watcher-v1.00001a-bino.exe`
2. Double-click to run (no installation needed)
3. Add folders and click "Start Watchers"

**Run in Background (No Window)**
```cmd
Watcher-v1.00001a-bino.exe --background
```
App runs silently in system tray with watchers started automatically.

**From Source**
```bash
git clone https://github.com/yourusername/watcher.git
cd watcher
pip install -r requirements.txt
python gui.py
```

**CLI Mode**
```bash
python gui.py --path C:/Downloads --path D:/Inbox --unknown Other --quiet
```

**Background Mode (Auto-start watchers)**
```bash
python gui.py --background
```

**Minimized to Tray**
```bash
python gui.py --minimized
```

---

## Future Roadmap (Planned)

- [ ] macOS `.app` bundle packaging
- [ ] Linux AppImage / snap support
- [ ] Advanced rules (regex, file size filtering)
- [ ] Scheduled watching (enable/disable on time)
- [ ] Logging to file with rotation
- [ ] Undo/rollback for moved files
- [ ] Watch depth configuration (enable subfolders if needed)
- [ ] macOS and Linux system tray support

---

## Migration Log

### Icon Development
- Initial `.png` icon design (binoculars, teal and white)
- Converted to `.ico` multi-size format (16x16, 32x32, 48x48, 64x64, 128x128, 256x256)
- Generated BMP-only variant (`watcher-icon-fixed.ico`) to avoid alpha channel issues in Windows Explorer
- Created base64-encoded PNG for runtime GUI embedding

### Build Process
- Initial PyInstaller setup with single-file exe
- Version metadata via `version_info.txt`
- Icon embedding verified in exe properties
- Build output: `Watcher-v1.00001a-bino.exe` (primary) in project root

---

**For detailed usage and configuration, see [README.md](README.md)**
