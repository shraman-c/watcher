Watcher (Python) — Auto-Organize New Files by Type

Overview
- Event-driven organizer that moves newly created files into subfolders based on their file type (extension).
- Watches only the top-level of selected folders; subfolders are never scanned or modified.
- Unknown types go to a configurable folder (default: "Other").

Install
- Python 3.9+ recommended.
- Install dependencies:
  - `pip install -r requirements.txt`

CLI Usage
- Start watchers for one or more folders from the integrated file [gui.py](gui.py)
  - `python gui.py --path C:/Downloads --path D:/Inbox --unknown Other --quiet`
  - Options:
    - `--path`: repeat for multiple folders
    - `--unknown`: target folder for unknown extensions
    - `--quiet`: reduce logs to errors only
    - `--rules`: JSON text mapping categories to extensions (optional)
    - `--create-test`: create sample files in each folder

GUI Usage
- Run the GUI: see [gui.py](gui.py)
  - `python gui.py`
  - Features:
    - Manage multiple folders (add/update/remove)
    - Per-folder settings: unknown target, quiet mode
    - Rules editor (JSON or simple lines: `Category = ext1,ext2`)
    - Auto-saves to [settings.json](settings.json)
    - Start watchers with one click
    - Create test files for quick verification

Rules Format
- JSON example:
  - `{ "Images": ["jpg","png"], "Documents": ["pdf","docx"] }`
- Simple lines:
  - `Images = jpg,png,webp`  (one category per line)
- If omitted, built-in defaults are used (Images, Videos, Audio, Documents, Archives, Code, Installers, Fonts).

Icon Options
- The GUI icon can be sourced in this priority order:
  - Embedded base64 in [gui.py](gui.py) (set `ICON_BASE64_DEFAULT`)
  - Env var `WATCHER_ICON_BASE64`
  - `icon_base64` field in [settings.json](settings.json)
  - File [watcher-icon.b64](watcher-icon.b64) (base64 PNG)
  - [watcher-icon.ico](watcher-icon.ico) or [watcher-icon.png](watcher-icon.png)

Generate Base64 Icon
- PowerShell (Windows):
  - `[Convert]::ToBase64String([IO.File]::ReadAllBytes("watcher-icon.png")) | Out-File -Encoding ascii watcher-icon.b64`
- Python:
  - `python -c "import base64;print(base64.b64encode(open('watcher-icon.png','rb').read()).decode())" > watcher-icon.b64`
- Paste the base64 string into the `ICON_BASE64_DEFAULT` block in [gui.py](gui.py), or place it in [watcher-icon.b64](watcher-icon.b64), or set `WATCHER_ICON_BASE64`.

Startup (Windows)
- Create a Task Scheduler task to run `python gui.py` at logon, or package an EXE (see below) and run that.

Packaging
- Optional: build a standalone executable with PyInstaller:
  - `pip install pyinstaller`
  - `pyinstaller --noconfirm --noconsole --onefile gui.py --name watcher`
  - Place icon file(s) alongside the executable or embed base64 as needed.

Notes
- Only new files in the top-level of each watched folder are acted upon.
- The app never scans existing files or subfolders.

Notes
- Only files are moved; new directories are ignored.
- If a file is created without an extension, it goes to the `Other` (or your chosen) folder.

Python (Cross-Platform) Version
- Requirements: Python 3.9+ and `pip`.
- Install dependencies:
```
pip install -r requirements.txt
```

Run (CLI)
```
python organizer.py --path "C:\\Downloads" "D:\\Incoming" --move-unknown-to Other --quiet
```
- Optional: pass custom rules as JSON:
```
python organizer.py --path "C:\\Downloads" --rules-json '{"Images":["jpg","png"],"Archives":["zip","7z"]}'
```
- Create sample files to test:
```
python organizer.py --path "C:\\Downloads" "D:\\Incoming" --create-test-files
```

Cross-Platform GUI (Tkinter)
- Launch the GUI:
```
python gui.py
```
- Features:
  - Multiple folders list (add/remove/update)
  - Per-folder unknown target and quiet toggle
  - Path selection (browse)
  - Custom file type rules (inline, shared)
  - Test sample files for selected folder
  - Remembers settings in settings.json
  - Start watchers (top-level only)
 - App Icon:
  - Provide a base64 PNG in one of these ways:
    - File: place `watcher-icon.b64` next to `gui.py`.
    - Settings: add `"icon_base64": "<your_base64_png>"` to `settings.json`.
    - Env var: set `WATCHER_ICON_BASE64` to your base64 string.
  - Alternatively, drop `watcher-icon.ico` or `watcher-icon.png` in the same folder.

Build Executables
- Windows (PyInstaller):
```
pip install pyinstaller
pyinstaller --onefile organizer.py
pyinstaller --onefile --windowed gui.py
```
- macOS:
```
pip install pyinstaller
pyinstaller --onefile organizer.py
pyinstaller --onefile --windowed gui.py
```
- Linux:
```
pip install pyinstaller
pyinstaller --onefile organizer.py
```
# organize

