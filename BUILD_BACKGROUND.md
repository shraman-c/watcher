# Building Watcher with Background Mode

This guide explains how to rebuild the Watcher executable with the new background functionality.

## Prerequisites

Make sure you have all dependencies installed:

```bash
pip install -r requirements.txt
```

This will install:
- watchdog >= 4.0.0
- pystray >= 0.19.0
- Pillow >= 10.0.0

## Building the Executable

### Using PyInstaller with the spec file:

```bash
pyinstaller Watcher-v1.00001a-bino.spec
```

The executable will be created in the `dist/` folder.

## Testing Background Mode

### 1. Test from Python (before building):

```bash
# Start minimized to tray
python gui.py --minimized

# Start in background with auto-start watchers
python gui.py --background
```

### 2. Test the compiled exe:

```cmd
# Normal mode (GUI visible)
Watcher-v1.00001a-bino.exe

# Minimized to tray
Watcher-v1.00001a-bino.exe --minimized

# Background mode (auto-start watchers)
Watcher-v1.00001a-bino.exe --background
```

### 3. Test startup integration:

1. Run the exe normally
2. Click "Startup Options" menu
3. Select "Add to Startup"
4. Check "Startup Options" → "Check Startup Status"
5. Log out and log back in - app should start automatically in background

## Features

- ✅ No console window appears (console=False in spec)
- ✅ System tray icon with menu
- ✅ Minimize to tray on window close
- ✅ Auto-start watchers in background mode
- ✅ Windows startup registry integration
- ✅ Completely silent operation

## Quick Start Helper

For end users, you can use the provided batch file:

```cmd
start-background.bat
```

This automatically launches Watcher in background mode.

## Troubleshooting

### "pystray not found" error:
```bash
pip install pystray>=0.19.0 Pillow>=10.0.0
```

### System tray icon not showing:
- Make sure pystray and Pillow are installed
- Check Windows notification area settings
- The app falls back gracefully if tray unavailable

### Startup not working:
- Run as Administrator if registry access denied
- Check registry at: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- Verify the exe path is correct in registry
