# Watcher 2.0 — Build and Packaging Guide

This guide covers building, testing, and packaging Watcher 2.0 for development and production distribution on Windows x64.

---

## 1. Prerequisites

- **Operating System**: Windows 10 (19041+) or Windows 11
- **.NET SDK**: .NET 9.0 SDK installed
- **Node.js**: v18+ and npm
- **Edge WebView2 Runtime**: Pre-installed on Windows 10/11 (or install Evergreen Runtime)
- **Windows SDK** *(optional, only needed for generating signed .msix packages; otherwise zip distribution is used)*

---

## 2. Fast Build Pipeline

The project includes an end-to-end automated build script:

```powershell
.\scripts\build.ps1
```

This script:
1. Compiles the frontend (`npm --prefix src/frontend run build`).
2. Runs all 21 automated unit tests across Core and Infrastructure.
3. Publishes a production release build to `dist/Watcher-v2.0.0-win-x64/`.

---

## 3. Manual Build Steps

### Step 1: Build the React Presentation Layer
```powershell
cd src/frontend
npm install
npm run build
cd ../..
```
*Output: `src/frontend/dist/` (HTML, JS, and CSS bundle).*

### Step 2: Run Automated Tests
```powershell
dotnet test Watcher.sln -c Release
```
*Verifies all 21 unit tests pass before publishing.*

### Step 3: Publish the Native Desktop Application
```powershell
dotnet publish src/desktop/Watcher.Desktop/Watcher.Desktop.csproj -c Release -r win-x64 --self-contained false -o dist/Watcher-v2.0.0-win-x64
```
*Output: `dist/Watcher-v2.0.0-win-x64/Watcher.exe` along with embedded `frontend/` assets and native SQLite binaries.*

---

## 4. Packaging for Distribution

Run the packaging script:

```powershell
.\scripts\create-msix.ps1
```

- If `makeappx.exe` is detected in your Windows SDK path, it builds an installable **`Watcher-v2.0.0.msix`** package using `scripts/AppxManifest.xml`.
- If the Windows SDK is not installed, it automatically generates a standalone zip package: **`dist/Package/Watcher-v2.0.0-win-x64.zip`**.

---

## 5. Offline Operation Guarantee

Watcher 2.0 is fully offline-capable:
- The production build embeds the compiled React frontend into the application directory.
- The WebView2 host navigates to virtual host `https://app.local/index.html` mapped directly to the local folder.
- No active internet connection or dev server is required to run the installed application.

