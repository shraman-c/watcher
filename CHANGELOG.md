# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

- **System Tray Integration** – Run Watcher in the background
  - Minimize to system tray on window close
  - System tray menu with Show/Hide, Start/Stop Watchers, and Exit options
  - Right-click tray icon for quick access to controls
  - Windows notification area support

- **Startup Background App** – Auto-launch on Windows startup
  - `--minimized` – Start with window hidden to system tray
  - `--background` – Start in background mode with auto-start watchers
  - Runs completely in background without console or window (compiled exe)
  - GUI "Startup Options" menu for easy configuration
    - Add to Startup – Register app in Windows startup registry
    - Remove from Startup – Unregister from Windows startup
    - Check Startup Status – Verify current startup configuration
  - Automatic watcher activation in background mode

- **Enhanced Dependency Management**
  - Auto-install pystray for system tray functionality
  - Auto-install Pillow for icon processing
  - Graceful fallback if system tray unavailable

- **Helper Scripts**
  - `start-background.bat` – Quick launcher for background mode on Windows
  - Double-click to start Watcher silently in system tray

### Changed

- Window close button now minimizes to tray (if available) instead of exiting
- Exit button and tray menu "Exit" option properly terminate the application

---

## [1.0.0.1 Alpha] - 2025-12-27

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
