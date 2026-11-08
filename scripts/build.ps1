<#
.SYNOPSIS
    Build and Package Watcher 2.0 for Windows x64.
.DESCRIPTION
    1. Installs frontend dependencies and builds Vite/React production assets.
    2. Runs all .NET Core and Infrastructure test suites.
    3. Publishes native Windows x64 release build to dist/Watcher-v2.0.0-win-x64.
#>

param(
    [switch]$SkipTests = $false,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$rootDir = Resolve-Path "$PSScriptRoot\.."

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Watcher 2.0 - Production Build Pipeline " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Ensure custom .NET and NuGet locations if present
if (Test-Path "$rootDir\.dotnet_home") {
    $env:DOTNET_CLI_HOME = "$rootDir\.dotnet_home"
}
if (Test-Path "$rootDir\.nuget\packages") {
    $env:NUGET_PACKAGES = "$rootDir\.nuget\packages"
}

$dotnetCmd = "dotnet"
if (Test-Path "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe") {
    $dotnetCmd = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
}

# 1. Build React/TypeScript Frontend
Write-Host "`n[1/3] Building Frontend (React 18 + TypeScript + Vite + TailwindCSS)..." -ForegroundColor Yellow
Push-Location "$rootDir\src\frontend"
try {
    npm run build
}
finally {
    Pop-Location
}

# 2. Run Automated Unit Tests
if (-not $SkipTests) {
    Write-Host "`n[2/3] Executing Automated Test Suites..." -ForegroundColor Yellow
    & $dotnetCmd test "$rootDir\tests\Watcher.Core.Tests\Watcher.Core.Tests.csproj" -c $Configuration --no-restore
    & $dotnetCmd test "$rootDir\tests\Watcher.Infrastructure.Tests\Watcher.Infrastructure.Tests.csproj" -c $Configuration --no-restore
} else {
    Write-Host "`n[2/3] Skipping tests (-SkipTests specified)." -ForegroundColor DarkGray
}

# 3. Publish Desktop Host Application
Write-Host "`n[3/3] Publishing Watcher 2.0 Desktop for win-x64..." -ForegroundColor Yellow
$outDir = "$rootDir\dist\Watcher-v2.0.0-win-x64"
& $dotnetCmd publish "$rootDir\src\desktop\Watcher.Desktop\Watcher.Desktop.csproj" -c $Configuration -r win-x64 --self-contained false -o $outDir

Write-Host "`n==========================================" -ForegroundColor Green
Write-Host "  Build Completed Successfully!           " -ForegroundColor Green
Write-Host "  Output Directory: $outDir" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

