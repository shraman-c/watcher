# -*- mode: python ; coding: utf-8 -*-

# Version information for Windows executable
version_info = (
    'FileVersion', (1, 0, 0, 0),
    'ProductVersion', (1, 0, 0, 0),
    'FileDescription', 'File Watcher - Automatic File Organizer',
    'CompanyName', 'Open Source',
    'ProductName', 'Watcher',
    'LegalCopyright', 'Copyright © 2025. Licensed under MIT License.',
    'OriginalFilename', 'watcher.exe',
    'InternalName', 'Watcher',
    'Comments', 'Automatically organize your downloads and folders by file type in real-time. Easy to use, smart, and efficient.',
)

a = Analysis(
    ['gui.py'],
    pathex=[],
    binaries=[],
    datas=[],
    hiddenimports=[],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
    optimize=0,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='watcher',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=False,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    icon=['watcher-icon.ico'],
    version='version_info.txt',
)
