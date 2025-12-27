# Watcher — Auto‑Organize New Files by Type

Overview
- Moves newly created files into subfolders based on extension.
- Watches only the top‑level of selected folders; never recurses subfolders.
- Unknown types go to a configurable folder (default: "Other").

Install
- Python 3.9+ recommended.
- Install dependencies: see [requirements.txt](requirements.txt)
  - `pip install -r requirements.txt`

Quick Start (GUI)
- Run the app: see [gui.py](gui.py)
  - `python gui.py`
- Features:
  - Modern ttk UI with folders table and inline rules editor
  - Manage multiple folders (add/update/remove)
  - Per‑folder settings: unknown target, quiet mode
  - Auto‑saves to [settings.json](settings.json)
  - Start/Stop watchers and create test files
  - Status bar with running/idle state

Quick Start (CLI)
- Use the integrated CLI from [gui.py](gui.py):
  - `python gui.py --path C:/Downloads --path D:/Inbox --unknown Other --quiet`
- Options:
  - `--path`: repeat for multiple folders
  - `--unknown`: target folder for unknown extensions
  - `--quiet`: reduce logs to errors only
  - `--rules`: JSON text mapping categories to extensions
  - `--create-test`: create sample files in each folder

Rules
- JSON example: `{ "Images": ["jpg","png"], "Documents": ["pdf","docx"] }`
- Simple lines: `Images = jpg,png,webp` (one category per line)
- Built‑in defaults include Images, Videos, Audio, Documents, Archives, Code, Installers, Fonts.

Icon
- Icon priority:
  - Embedded base64 in [gui.py](gui.py) (`ICON_BASE64_DEFAULT`)
  - Env var `WATCHER_ICON_BASE64`
  - `icon_base64` in [settings.json](settings.json)
  - [watcher-icon.b64](watcher-icon.b64) (base64 PNG), then [watcher-icon.ico](watcher-icon.ico) / [watcher-icon.png](watcher-icon.png)
- Generate base64:
  - PowerShell: `[Convert]::ToBase64String([IO.File]::ReadAllBytes("watcher-icon.png")) | Out-File -Encoding ascii watcher-icon.b64`
  - Python: `python -c "import base64;print(base64.b64encode(open('watcher-icon.png','rb').read()).decode())" > watcher-icon.b64`
- Paste into the block in [gui.py](gui.py) or use `WATCHER_ICON_BASE64` or [watcher-icon.b64](watcher-icon.b64).

Startup (Windows)
- Create a Task Scheduler task to run `python gui.py` at logon, or package an EXE (below) and run that.

Packaging
- Build a standalone executable with PyInstaller:
  - `pip install pyinstaller`
  - `pyinstaller --noconfirm --noconsole --onefile gui.py --name watcher`

Notes
- Acts only on new files in the top‑level of watched folders.
- Ignores directories; files without extension go to "Other" (or your chosen folder).

