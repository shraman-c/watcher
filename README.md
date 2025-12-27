<div align="center">

<img width="200" height="200" alt="binoculars-design-vector-icon-sy" src="https://github.com/user-attachments/assets/53c6e78d-25da-4727-8fb9-eb77ab3514d6" />

# 📂 Watcher — Auto-Organize New Files by Type

[![Python](https://img.shields.io/badge/Python-3.9+-3776AB?style=for-the-badge&logo=python&logoColor=white)](https://www.python.org/)
[![Watchdog](https://img.shields.io/badge/Watchdog-4.0+-orange?style=for-the-badge)](https://pypi.org/project/watchdog/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20macOS%20%7C%20Linux-lightgrey?style=for-the-badge)](https://github.com)
[![Download](https://img.shields.io/badge/Download-watcher.exe-blue?style=for-the-badge&logo=windows)](watcher.exe)

**Automatically organize your downloads and other folders by file type in real-time!**

💡 **Quick Start:** Download [watcher.exe](watcher.exe) and run it — no installation needed!

[Features](#-features) • [Quick Start](#-quick-start) • [Installation](#-installation) • [Usage](#-usage) • [Configuration](#-configuration) • [FAQ](#-faq)

</div>

---

## 📋 Table of Contents

- [📂 Watcher — Auto-Organize New Files by Type](#-watcher--auto-organize-new-files-by-type)
  - [📋 Table of Contents](#-table-of-contents)
  - [✨ Features](#-features)
  - [⚡ Quick Start](#-quick-start)
    - [🪟 Windows Users (Easiest)](#-windows-users-easiest)
  - [🚀 Installation](#-installation)
    - [For Python Users](#for-python-users)
      - [Prerequisites](#prerequisites)
    - [Steps](#steps)
  - [💻 Usage](#-usage)
    - [GUI Mode](#gui-mode)
    - [CLI Mode](#cli-mode)
  - [⚙️ Configuration](#️-configuration)
    - [File Organization Rules](#file-organization-rules)
    - [Custom Icon Setup](#custom-icon-setup)
  - [📦 Packaging \& Deployment](#-packaging--deployment)
    - [🪟 Windows Executable](#-windows-executable)
    - [🍎 macOS Application Bundle](#-macos-application-bundle)
    - [🐧 Linux Binary](#-linux-binary)
  - [🔧 Advanced Setup](#-advanced-setup)
    - [🪟 Windows: Auto-start on Login](#-windows-auto-start-on-login)
    - [🍎 macOS: Auto-start on Login](#-macos-auto-start-on-login)
    - [🐧 Linux: Auto-start on Login](#-linux-auto-start-on-login)
  - [❓ FAQ](#-faq)
  - [🤝 Contributing](#-contributing)

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

## ⚡ Quick Start

### 🪟 Windows Users (Easiest)

**No Python installation required!**

1. **Download** [watcher.exe](watcher.exe) from this repository
2. **Double-click** to run the application
3. **Add folders** to watch and click "Start Watchers"
4. **Done!** Your files will be automatically organized

**Optional:** Set up auto-start on login (see [Advanced Setup](#-advanced-setup))

---

## 🚀 Installation

### For Python Users

#### Prerequisites

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

**✨ Built-in Icon:** The app comes with an embedded base64-encoded icon in [gui.py](gui.py#L22) (`ICON_BASE64_DEFAULT`), so it works out of the box!

The app searches for an icon in this **priority order**:

1. **Embedded base64** in [gui.py](gui.py) (`ICON_BASE64_DEFAULT` variable) ⭐ *Default*
2. **Environment variable** `WATCHER_ICON_BASE64`
3. **`icon_base64` field** in [settings.json](settings.json)
4. **Base64 file:** [watcher-icon.b64](watcher-icon.b64) (base64-encoded PNG)
5. **Image files:** [watcher-icon.ico](watcher-icon.ico) or [watcher-icon.png](watcher-icon.png)

---

**🔧 Customize the Embedded Icon:**

1. **Create or find your icon** (PNG format recommended, 256x256 or 512x512)

2. **Convert to base64:**

**PowerShell:**
```powershell
# Generate base64 string (prints to console)
[Convert]::ToBase64String([IO.File]::ReadAllBytes("your-icon.png"))

# Or save to file
[Convert]::ToBase64String([IO.File]::ReadAllBytes("your-icon.png")) | Out-File -Encoding ascii watcher-icon.b64
```

**Python:**
```bash
# Generate base64 string (prints to console)
python -c "import base64; print(base64.b64encode(open('your-icon.png','rb').read()).decode())"

# Or save to file
python -c "import base64;print(base64.b64encode(open('your-icon.png','rb').read()).decode())" > watcher-icon.b64
```

**Linux/macOS:**
```bash
base64 your-icon.png
```

3. **Update [gui.py](gui.py#L22):**

Find the `ICON_BASE64_DEFAULT` variable (around line 22) and replace the long string with your base64 output:

```python
ICON_BASE64_DEFAULT = "iVBORw0KGgoAAAANS... YOUR BASE64 STRING HERE ..."
```

4. **Rebuild your executable** if you've already packaged the app

---

**💡 Alternative Methods (Without Editing Code):**

- **Environment Variable:** `set WATCHER_ICON_BASE64=your_base64_string` (Windows) or `export WATCHER_ICON_BASE64=your_base64_string` (Linux/macOS)
- **Settings File:** Add `"icon_base64": "your_base64_string"` to [settings.json](settings.json)
- **File:** Save base64 string to `watcher-icon.b64` or place `watcher-icon.png`/`watcher-icon.ico` in the same folder

</details>

---

## 📦 Packaging & Deployment

### 🪟 Windows Executable

<details open>
<summary><b>🎨 Method 1: auto-py-to-exe (GUI - Easiest)</b></summary>

**auto-py-to-exe** provides a graphical interface for PyInstaller with live preview:

```bash
# Install auto-py-to-exe
pip install auto-py-to-exe

# Launch the GUI
auto-py-to-exe
```

**Configuration in auto-py-to-exe:**

1. **Script Location:** Browse and select `gui.py`
2. **Onefile:** Select **"One File"**
3. **Console Window:** Select **"Window Based (hide the console)"**
4. **Icon:** *(Optional)* Browse to `watcher-icon.ico` if you have one
   - **Note:** The app already has an embedded base64 icon in `ICON_BASE64_DEFAULT`, so this is optional
5. **Additional Files:** None needed (icon is embedded)
6. **Advanced:**
   - Name: `watcher`
7. Click **"CONVERT .PY TO .EXE"**

Your executable will be in the `output/` folder.

**📌 About the Embedded Icon:**
The app includes a built-in icon as a base64 string in [gui.py](gui.py) (`ICON_BASE64_DEFAULT` variable). This means:
- ✅ No external icon file needed
- ✅ Works even without specifying an icon in auto-py-to-exe
- ✅ Icon displays in the app window automatically
- 💡 To customize: Replace the `ICON_BASE64_DEFAULT` value in [gui.py](gui.py) with your own base64-encoded PNG

**Generate your own base64 icon:**
```powershell
# PowerShell - creates base64 string
[Convert]::ToBase64String([IO.File]::ReadAllBytes("your-icon.png"))
```
Copy the output and paste it into `ICON_BASE64_DEFAULT` in [gui.py](gui.py#L22)

**For auto-start:** Copy the `.exe` to your Windows Startup folder (see [Advanced Setup](#-advanced-setup))

</details>

<details>
<summary><b>🔨 Method 2: PyInstaller (Command Line)</b></summary>

Create a single-file executable using **PyInstaller**:

```bash
# Install PyInstaller
pip install pyinstaller

# Basic build (uses embedded icon)
pyinstaller --noconfirm --noconsole --onefile gui.py --name watcher

# With external icon file (optional)
pyinstaller --noconfirm --noconsole --onefile --icon=watcher-icon.ico gui.py --name watcher
```

Your executable will be in the `dist/` folder.

**Note:** The embedded base64 icon in the script will be used for the app window regardless of whether you specify `--icon` (which only affects the .exe file icon in Windows Explorer).

**For auto-start:** Copy `dist/watcher.exe` to your Windows Startup folder (see [Advanced Setup](#-advanced-setup))

</details>

---

### 🍎 macOS Application Bundle

<details>
<summary><b>Build macOS .app with PyInstaller</b></summary>

Create a native macOS application:

```bash
# Install PyInstaller
pip install pyinstaller

# Build the .app bundle
pyinstaller --noconfirm --noconsole --onefile --name watcher gui.py

# Optional: Add an icon (use .icns format for macOS)
pyinstaller --noconfirm --noconsole --onefile --icon=watcher-icon.icns --name watcher gui.py
```

The app bundle will be in `dist/watcher.app`

**Create .icns from PNG:**
```bash
mkdir watcher.iconset
sips -z 16 16 watcher-icon.png --out watcher.iconset/icon_16x16.png
sips -z 32 32 watcher-icon.png --out watcher.iconset/icon_16x16@2x.png
sips -z 32 32 watcher-icon.png --out watcher.iconset/icon_32x32.png
sips -z 64 64 watcher-icon.png --out watcher.iconset/icon_32x32@2x.png
sips -z 128 128 watcher-icon.png --out watcher.iconset/icon_128x128.png
sips -z 256 256 watcher-icon.png --out watcher.iconset/icon_128x128@2x.png
sips -z 256 256 watcher-icon.png --out watcher.iconset/icon_256x256.png
sips -z 512 512 watcher-icon.png --out watcher.iconset/icon_256x256@2x.png
sips -z 512 512 watcher-icon.png --out watcher.iconset/icon_512x512.png
cp watcher-icon.png watcher.iconset/icon_512x512@2x.png
iconutil -c icns watcher.iconset
```

**Move to Applications:**
```bash
mv dist/watcher.app /Applications/
```

</details>

---

### 🐧 Linux Binary

<details>
<summary><b>Build Linux executable with PyInstaller</b></summary>

Create a standalone binary for Linux:

```bash
# Install PyInstaller
pip install pyinstaller

# Build the binary
pyinstaller --noconfirm --noconsole --onefile gui.py --name watcher
```

The executable will be in `dist/watcher`

**Make it globally accessible:**
```bash
# Move to /usr/local/bin
sudo mv dist/watcher /usr/local/bin/

# Or create a symlink
sudo ln -s $(pwd)/dist/watcher /usr/local/bin/watcher
```

**Create a .desktop launcher:**

Create `~/.local/share/applications/watcher.desktop`:

```ini
[Desktop Entry]
Type=Application
Name=File Watcher
Comment=Auto-organize files by type
Exec=/usr/local/bin/watcher
Icon=/path/to/watcher-icon.png
Terminal=false
Categories=Utility;FileTools;
StartupNotify=true
```

Update desktop database:
```bash
update-desktop-database ~/.local/share/applications/
```

</details>

---

## 🔧 Advanced Setup

### 🪟 Windows: Auto-start on Login

<details open>
<summary><b>Option 1: Task Scheduler (Python Script)</b></summary>

1. Open **Task Scheduler** (press `Win + R`, type `taskschd.msc`)
2. Click **"Create Task..."** in the right panel
3. **General tab:**
   - Name: `Watcher File Organizer`
   - Description: `Automatically organize downloads by file type`
   - Select: **"Run whether user is logged on or not"**
4. **Triggers tab:**
   - Click **"New..."**
   - Begin the task: **"At log on"**
   - Specific user: Select your username
5. **Actions tab:**
   - Click **"New..."**
   - Action: **"Start a program"**
   - Program/script: `python.exe` (or full path: `C:\Python39\python.exe`)
   - Add arguments: `"C:\path\to\watcher\gui.py"`
   - Start in: `C:\path\to\watcher`
6. Click **OK** to save

</details>

<details>
<summary><b>Option 2: Startup Folder (Executable)</b></summary>

Best for packaged `.exe` files:

1. Build the executable (see [Packaging & Deployment](#-packaging--deployment))
2. Press `Win + R` and type: `shell:startup`
3. Copy `watcher.exe` into the Startup folder
4. Create a shortcut (optional) for easier management

**Alternative:** Create a shortcut in the Startup folder:
```powershell
# PowerShell command to create shortcut
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup\watcher.lnk")
$Shortcut.TargetPath = "C:\path\to\watcher\dist\watcher.exe"
$Shortcut.WorkingDirectory = "C:\path\to\watcher"
$Shortcut.Save()
```

</details>

<details>
<summary><b>Option 3: Registry Run Key</b></summary>

Add to Windows Registry for auto-start:

```powershell
# Add to registry (run as Administrator)
New-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "Watcher" -Value "C:\path\to\watcher\dist\watcher.exe" -PropertyType String -Force
```

To remove:
```powershell
Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "Watcher"
```

</details>

---

### 🍎 macOS: Auto-start on Login

<details open>
<summary><b>Option 1: Login Items (Recommended)</b></summary>

**For .app bundle:**

1. Build the macOS app (see [Packaging & Deployment](#-packaging--deployment))
2. Move `watcher.app` to `/Applications/`
3. Open **System Settings** (or System Preferences)
4. Go to **General** → **Login Items** (macOS Ventura+) or **Users & Groups** → **Login Items** (older macOS)
5. Click the **"+"** button
6. Select `/Applications/watcher.app`
7. The app will now start automatically at login

**For Python script:**

Add to Login Items using AppleScript:

```bash
osascript -e 'tell application "System Events" to make login item at end with properties {path:"/Applications/watcher.app", hidden:false}'
```

</details>

<details>
<summary><b>Option 2: LaunchAgent (Advanced)</b></summary>

Create a LaunchAgent plist file for more control:

Create `~/Library/LaunchAgents/com.user.watcher.plist`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.user.watcher</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/python3</string>
        <string>/Users/YOUR_USERNAME/path/to/watcher/gui.py</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <false/>
    <key>StandardOutPath</key>
    <string>/tmp/watcher.log</string>
    <key>StandardErrorPath</key>
    <string>/tmp/watcher.error.log</string>
</dict>
</plist>
```

Load the LaunchAgent:

```bash
# Load the agent
launchctl load ~/Library/LaunchAgents/com.user.watcher.plist

# Start immediately
launchctl start com.user.watcher

# Check status
launchctl list | grep watcher

# Unload (to stop auto-start)
launchctl unload ~/Library/LaunchAgents/com.user.watcher.plist
```

**For executable binary:**

Replace the `ProgramArguments` section with:
```xml
<key>ProgramArguments</key>
<array>
    <string>/Applications/watcher.app/Contents/MacOS/watcher</string>
</array>
```

</details>

<details>
<summary><b>Option 3: Automator Application</b></summary>

Create an Automator app that launches at login:

1. Open **Automator**
2. Create a new **Application**
3. Add **"Run Shell Script"** action
4. Enter the script:
   ```bash
   /usr/local/bin/python3 /path/to/watcher/gui.py
   ```
5. Save as `Watcher.app`
6. Add to Login Items (Option 1 above)

</details>

---

### 🐧 Linux: Auto-start on Login

<details open>
<summary><b>Option 1: systemd User Service (Recommended)</b></summary>

Create a systemd user service for automatic startup:

Create `~/.config/systemd/user/watcher.service`:

```ini
[Unit]
Description=File Watcher Auto-Organizer
After=graphical-session.target

[Service]
Type=simple
ExecStart=/usr/bin/python3 /home/YOUR_USERNAME/path/to/watcher/gui.py
Restart=on-failure
RestartSec=10
Environment="DISPLAY=:0"
Environment="XAUTHORITY=/home/YOUR_USERNAME/.Xauthority"

[Install]
WantedBy=default.target
```

**For executable binary:**
```ini
ExecStart=/usr/local/bin/watcher
```

**Enable and manage the service:**

```bash
# Reload systemd user daemon
systemctl --user daemon-reload

# Enable auto-start at login
systemctl --user enable watcher.service

# Start immediately
systemctl --user start watcher.service

# Check status
systemctl --user status watcher.service

# View logs
journalctl --user -u watcher.service -f

# Stop the service
systemctl --user stop watcher.service

# Disable auto-start
systemctl --user disable watcher.service
```

</details>

<details>
<summary><b>Option 2: XDG Autostart (.desktop file)</b></summary>

Create an autostart entry for desktop environments:

Create `~/.config/autostart/watcher.desktop`:

```ini
[Desktop Entry]
Type=Application
Name=File Watcher
Comment=Auto-organize files by type
Exec=/usr/bin/python3 /home/YOUR_USERNAME/path/to/watcher/gui.py
Icon=/home/YOUR_USERNAME/path/to/watcher/watcher-icon.png
Terminal=false
Categories=Utility;FileTools;
X-GNOME-Autostart-enabled=true
StartupNotify=false
```

**For executable binary:**
```ini
Exec=/usr/local/bin/watcher
```

Make it executable:
```bash
chmod +x ~/.config/autostart/watcher.desktop
```

The app will now start automatically when you log in to your desktop environment.

</details>

<details>
<summary><b>Option 3: Cron @reboot</b></summary>

Use cron to start at boot (less reliable for GUI apps):

```bash
# Edit crontab
crontab -e

# Add this line
@reboot DISPLAY=:0 /usr/bin/python3 /home/YOUR_USERNAME/path/to/watcher/gui.py &
```

**Note:** This method may have issues with GUI applications depending on your display manager.

</details>

<details>
<summary><b>Option 4: Shell Profile (.bashrc/.zshrc)</b></summary>

Start when opening a terminal (not recommended for GUI apps):

Add to `~/.bashrc` or `~/.zshrc`:

```bash
# Auto-start watcher if not already running
if ! pgrep -f "gui.py" > /dev/null; then
    /usr/bin/python3 /path/to/watcher/gui.py &
fi
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



<div align="center">

**Made with ❤️ by [ShramanC](https://github.com/shraman-c)**

Built for developers who hate messy download folders

⭐ Star this repo if you find it useful!

---

*Watcher v1.0.0 — Automate Organization*

*© 2025 Shraman Chaudhuri. All rights reserved.*

*This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.*
</div>

