Auto-Organize New Files by Type (Windows PowerShell)

Overview
- Watches a folder and, when a new file is created, moves it into a subfolder based on its file type (extension). Unknown types go to "Other". It does not scan existing files; only reacts to new ones.

Quick Start
- Requirements: Windows PowerShell 5+.
- Script: see [watch-organizer.ps1](watch-organizer.ps1).

Run
1) Open PowerShell in this folder.
2) Start the watcher for a target directory (replace the path):

```
powershell -ExecutionPolicy Bypass -File .\watch-organizer.ps1 -Path "C:\Downloads"
```

- Optional flags:
  - `-Quiet:$true` — reduce logs to errors only.
  - `-MoveUnknownTo "Misc"` — change where unknown extensions go.

**Important:** Only the top-level of the target directory is monitored. Subfolders and their structures are never scanned or modified, preserving existing project layouts.
Customize Categories
- By default, categories include Images, Videos, Audio, Documents, Archives, Code, Installers, Fonts.
- You can override with a custom hashtable:

```
$rules = @{ 
	Images = @('jpg','jpeg','png');
	Code   = @('py','js','ts');
	Archives = @('zip','rar','7z');
}
powershell -ExecutionPolicy Bypass -File .\watch-organizer.ps1 -Path "C:\Downloads" -Rules $rules -MoveUnknownTo "Other"
```

How It Works
- Uses `FileSystemWatcher` to listen for `Created` events **in the top-level directory only**.
- Does not scan or recurse into existing subdirectories (preserves project structures).
- Waits briefly until the new file is ready (not locked) before moving.
- Ensures destination folders exist; avoids name collisions by appending a counter.

Stop the Watcher
- Press `Ctrl+C` in the PowerShell window to stop.

Run as Background Job (optional)
- You can start it in a separate PowerShell window:

```
Start-Process powershell -ArgumentList '-NoExit','-ExecutionPolicy','Bypass','-File','"' + (Resolve-Path .\watch-organizer.ps1).Path + '"','-Path','"C:\Downloads"'
```

Build as Native Windows EXE
- To create a standalone .exe (no PowerShell required):
  1. Run the build script:
  ```
  powershell -ExecutionPolicy Bypass -File .\build-exe.ps1
  ```
  2. This generates `watch-organizer.exe` in the repo folder.
  3. Run the EXE directly:
  ```
  .\watch-organizer.exe -Path "C:\Downloads"
  ```
- The EXE can be moved anywhere and run without PowerShell or execution policy restrictions.

GUI Configuration Tool (Cross-Platform Web UI)
  For an interactive, no-command-line way to set up and test:
  ```
  powershell -ExecutionPolicy Bypass -File .\web-server.ps1
  ```
  A browser window opens automatically with a beautiful, responsive UI.
  Features:
  - Path selection (type or paste)
  - Configurable unknown-file folder
  - Quiet mode toggle
  - Custom file type rules (edit inline)
  - Command preview
  - Test sample files button
  - Direct watcher launch
  
  **Works on Windows, Linux, and macOS** (with PowerShell 7+)

Notes
- Only files are moved; new directories are ignored.
- If a file is created without an extension, it goes to the `Other` (or your chosen) folder.
# organize

