<#
.SYNOPSIS
    Packages Watcher 2.0 into an installable MSIX package or zip distribution.
#>

param(
    [string]$DistPath = "$PSScriptRoot\..\dist\Watcher-v2.0.0-win-x64",
    [string]$OutputDir = "$PSScriptRoot\..\dist\Package"
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

if (-not (Test-Path $DistPath)) {
    Write-Error "Published distribution directory not found at $DistPath. Run scripts\build.ps1 first."
}

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Write-Host "Packaging Watcher 2.0 from $DistPath to $OutputDir..." -ForegroundColor Cyan

# Copy AppxManifest into output staging
Copy-Item "$PSScriptRoot\AppxManifest.xml" "$DistPath\AppxManifest.xml" -Force

# Check if makeappx is available from Windows SDK
$makeAppxCmd = Get-Command makeappx.exe -ErrorAction SilentlyContinue
$makeAppx = $null
if ($makeAppxCmd) {
    $makeAppx = $makeAppxCmd.Source
} else {
    $sdkPaths = @(
        "C:\Program Files (x86)\Windows Kits\10\bin\10.0.*\x64\makeappx.exe",
        "C:\Program Files\Windows Kits\10\bin\10.0.*\x64\makeappx.exe"
    )
    $resolved = Resolve-Path $sdkPaths -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($resolved) {
        $makeAppx = $resolved.Path
    }
}

if ($makeAppx) {
    Write-Host "Found makeappx.exe at $makeAppx. Generating MSIX..." -ForegroundColor Yellow
    $msixOut = "$OutputDir\Watcher-v2.0.0.msix"
    & $makeAppx pack /d $DistPath /p $msixOut /o
    Write-Host "Created MSIX package at: $msixOut" -ForegroundColor Green
} else {
    Write-Host "makeappx.exe not in PATH (Windows SDK not installed). Creating standalone zip package..." -ForegroundColor Yellow
    $zipOut = "$OutputDir\Watcher-v2.0.0-win-x64.zip"
    if (Test-Path $zipOut) { Remove-Item $zipOut -Force }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($DistPath, $zipOut, [System.IO.Compression.CompressionLevel]::Optimal, $false)
    Write-Host "Created standalone distribution zip at: $zipOut" -ForegroundColor Green
}
