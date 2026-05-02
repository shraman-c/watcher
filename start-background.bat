@echo off
REM Start Watcher in background mode (minimized to system tray with auto-start)
echo Starting Watcher in background mode...
start "" "%~dp0Watcher-v1.00001a-bino.exe" --background
echo Watcher is now running in the system tray.
timeout /t 2 >nul
