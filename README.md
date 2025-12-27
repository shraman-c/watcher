<div align="center">

# 📂 Watcher — Auto-Organize New Files by Type

[![Python](https://img.shields.io/badge/Python-3.9+-3776AB?style=for-the-badge&logo=python&logoColor=white)](https://www.python.org/)
[![Watchdog](https://img.shields.io/badge/Watchdog-4.0+-orange?style=for-the-badge)](https://pypi.org/project/watchdog/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey?style=for-the-badge)](https://github.com)

**Automatically organize your downloads and other folders by file type in real-time!**

[Features](#-features) • [Installation](#-installation) • [Usage](#-usage) • [Configuration](#-configuration) • [FAQ](#-faq)

</div>

---

## 📋 Table of Contents

- [📂 Watcher — Auto-Organize New Files by Type](#-watcher--auto-organize-new-files-by-type)
  - [📋 Table of Contents](#-table-of-contents)
  - [✨ Features](#-features)
  - [🚀 Installation](#-installation)
    - [Prerequisites](#prerequisites)
    - [Steps](#steps)
  - [💻 Usage](#-usage)
    - [GUI Mode](#gui-mode)
    - [CLI Mode](#cli-mode)
  - [⚙️ Configuration](#️-configuration)
    - [File Organization Rules](#file-organization-rules)
    - [Custom Icon Setup](#custom-icon-setup)
  - [📦 Packaging \& Deployment](#-packaging--deployment)
  - [🔧 Advanced Setup](#-advanced-setup)
  - [❓ FAQ](#-faq)
  - [🤝 Contributing](#-contributing)
  - [📄 License](#-license)

---

## ✨ Features

<details open>
<summary><b>Click to expand/collapse feature list</b></summary>

✅ **Real-time File Monitoring** — Automatically organizes files as they're created  
✅ **Smart Categorization** — Sorts by extension into configurable categories  
✅ **GUI & CLI Support** — Use the modern tkinter interface or command-line  
✅ **Multi-folder Management** — Watch multiple directories simultaneously  
✅ **Per-folder Settings** — Customize behavior for each watched location  
✅ **Safe & Non-recursive** — Only watches top-level; never touches subfolders  
✅ **Zero Configuration** — Works out of the box with sensible defaults  
✅ **Portable & Lightweight** — Package into a single executable  
✅ **Cross-platform** — Windows, macOS, and Linux support

</details>

---

## 🚀 Installation

### Prerequisites

- **Python 3.9+** (recommended)
- **pip** package manager

### Steps

1. **Clone or download this repository**

```bash
git clone https://github.com/yourusername/watcher.git
cd watcher
```

2. **Install dependencies**

```bash
pip install -r requirements.txt
```

<details>
<summary>What gets installed?</summary>

- `watchdog>=4.0.0` — File system monitoring library

</details>

---

## 💻 Usage

### GUI Mode

**Launch the graphical interface:**

```bash
python gui.py
```

<details>
<summary><b>📸 GUI Features Preview</b></summary>

The GUI provides:
- 🗂️ **Folders Table** — Manage multiple watched directories
- ✏️ **Inline Rules Editor** — Customize file type categorization
- ⚙️ **Per-folder Settings** — Unknown file target, quiet mode
- 💾 **Auto-save** — Changes persist to [settings.json](settings.json)
- ▶️ **Start/Stop Controls** — Toggle watchers on demand
- 🧪 **Test File Creator** — Generate sample files for testing
- 📊 **Status Bar** — Real-time running/idle state indicator

</details>

### CLI Mode

**Run in command-line mode with options:**

```bash
python gui.py --path C:/Downloads --path D:/Inbox --unknown Other --quiet
```

<details open>
<summary><b>Available CLI Options</b></summary>

| Option | Description | Example |
|--------|-------------|---------|
| `--path` | Folder to watch (repeat for multiple) | `--path C:/Downloads` |
| `--unknown` | Target folder for unrecognized extensions | `--unknown Miscellaneous` |
| `--quiet` | Reduce logging to errors only | `--quiet` |
| `--rules` | JSON mapping of categories to extensions | `--rules '{"Images":["jpg","png"]}'` |
| `--create-test` | Create sample files for testing | `--create-test` |

</details>

**Example: Watch multiple folders with custom rules**

```bash
python gui.py --path ~/Downloads --path ~/Desktop --unknown Other --rules '{"Photos":["jpg","png","heic"],"Spreadsheets":["xlsx","csv"]}'
```

---

## ⚙️ Configuration

### File Organization Rules

<details>
<summary><b>📝 Rules Format</b></summary>

You can define rules in two formats:

**1. Simple Line Format** (recommended)
```
Images = jpg,png,webp,gif
Documents = pdf,docx,txt
Videos = mp4,mov,avi
Archives = zip,rar,7z
```

**2. JSON Format**
```json
{
  "Images": ["jpg", "png", "webp", "gif"],
  "Documents": ["pdf", "docx", "txt"],
  "Videos": ["mp4", "mov", "avi"],
  "Archives": ["zip", "rar", "7z"]
}
```

</details>

<details open>
<summary><b>🎯 Built-in Default Categories</b></summary>

| Category | Extensions |
|----------|------------|
| **Images** | jpg, jpeg, png, gif, bmp, tiff, webp, svg |
| **Videos** | mp4, mov, mkv, avi, wmv, flv, webm, m4v |
| **Audio** | mp3, wav, flac, aac, ogg, m4a, wma |
| **Documents** | pdf, doc, docx, xls, xlsx, ppt, pptx, txt, rtf |
| **Archives** | zip, rar, 7z, tar, gz, bz2 |
| **Code** | py, js, ts, java, cs, cpp, html, css, json, sql |
| **Installers** | exe, msi, msix, apk, dmg, pkg |
| **Fonts** | ttf, otf, woff, woff2 |

</details>

### Custom Icon Setup

<details>
<summary><b>🎨 Icon Configuration (click to expand)</b></summary>

The app searches for an icon in this priority order:

1. Embedded base64 in [gui.py](gui.py) (`ICON_BASE64_DEFAULT` variable)
2. Environment variable `WATCHER_ICON_BASE64`
3. `icon_base64` field in [settings.json](settings.json)
4. [watcher-icon.b64](watcher-icon.b64) (base64-encoded PNG)
5. [watcher-icon.ico](watcher-icon.ico) or [watcher-icon.png](watcher-icon.png)

**Generate base64 from an image:**

**PowerShell:**
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("watcher-icon.png")) | Out-File -Encoding ascii watcher-icon.b64
```

**Python:**
```bash
python -c "import base64;print(base64.b64encode(open('watcher-icon.png','rb').read()).decode())" > watcher-icon.b64
```

</details>

---

## 📦 Packaging & Deployment

<details>
<summary><b>🔨 Build a Standalone Executable</b></summary>

Create a single-file executable using **PyInstaller**:

```bash
# Install PyInstaller
pip install pyinstaller

# Build the executable
pyinstaller --noconfirm --noconsole --onefile gui.py --name watcher
```

Your executable will be in the `dist/` folder.

</details>

---

## 🔧 Advanced Setup

<details>
<summary><b>🪟 Windows: Auto-start on Login</b></summary>

**Option 1: Task Scheduler**

1. Open **Task Scheduler**
2. Create a new task:
   - **Trigger:** At log on
   - **Action:** Start a program
   - **Program:** `python.exe`
   - **Arguments:** `"C:\path\to\watcher\gui.py"`

**Option 2: Startup Folder** (if using packaged .exe)

1. Press `Win + R` and type `shell:startup`
2. Place your `watcher.exe` in the startup folder

</details>

<details>
<summary><b>🐧 Linux/macOS: Run as System Service</b></summary>

Create a systemd service file (`~/.config/systemd/user/watcher.service`):

```ini
[Unit]
Description=File Watcher Auto-Organizer

[Service]
ExecStart=/usr/bin/python3 /path/to/watcher/gui.py
Restart=always

[Install]
WantedBy=default.target
```

Enable and start:
```bash
systemctl --user enable watcher
systemctl --user start watcher
```

</details>

---

## ❓ FAQ

<details>
<summary><b>Q: Will this move files in subfolders?</b></summary>

No. Watcher only monitors and organizes files created at the **top level** of watched folders. Subfolders are never touched.

</details>

<details>
<summary><b>Q: What happens to files without extensions?</b></summary>

Files without extensions are moved to the "Other" folder (or whatever you've configured as the unknown target).

</details>

<details>
<summary><b>Q: Can I watch network drives?</b></summary>

Yes, but performance may vary depending on your network connection and the file system monitoring capabilities of the remote drive.

</details>

<details>
<summary><b>Q: How do I stop the watcher?</b></summary>

In GUI mode, click the "Stop" button. In CLI mode, press `Ctrl+C` in the terminal.

</details>

<details>
<summary><b>Q: Can I exclude certain file types?</b></summary>

Currently, all files are processed. You can customize rules to direct unwanted types to a specific folder instead.

</details>

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. 🍴 Fork the repository
2. 🌱 Create a feature branch (`git checkout -b feature/amazing-feature`)
3. 💾 Commit your changes (`git commit -m 'Add amazing feature'`)
4. 📤 Push to the branch (`git push origin feature/amazing-feature`)
5. 🎉 Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

<div align="center">

**Made with ❤️ by developers who hate messy download folders**

⭐ Star this repo if you find it useful!

</div>

