<#
.SYNOPSIS
  Converts watch-organizer.ps1 to a standalone watch-organizer.exe using ps2exe.

.DESCRIPTION
  Downloads ps2exe (if needed) and compiles the PowerShell watcher script into a native Windows EXE.
  The resulting EXE can be run directly without PowerShell or execution policy restrictions.

.NOTES
  Requires .NET Framework 4.5 or higher (included on most Windows systems).
#>

[CmdletBinding()]
param(
  [string]$OutputPath = ".\watch-organizer.exe",
  [string]$Ps2ExeUrl = "https://github.com/MScholtes/PS2EXE/releases/download/v0.5.0.16/ps2exe.exe"
)

function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Err($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }
function Write-Success($msg) { Write-Host "[SUCCESS] $msg" -ForegroundColor Green }

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent (Resolve-Path $MyInvocation.MyCommandName)
$ps2ExePath = Join-Path $scriptRoot "ps2exe.exe"
$scriptToConvert = Join-Path $scriptRoot "watch-organizer.ps1"

# Validate script exists
if (-not (Test-Path -LiteralPath $scriptToConvert)) {
  Write-Err "Script not found: $scriptToConvert"
  exit 1
}

Write-Info "Converting: $scriptToConvert -> $OutputPath"

# Download ps2exe if not present
if (-not (Test-Path -LiteralPath $ps2ExePath)) {
  Write-Info "Downloading ps2exe from GitHub..."
  try {
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
    Invoke-WebRequest -Uri $Ps2ExeUrl -OutFile $ps2ExePath -ErrorAction Stop
    Write-Success "ps2exe downloaded."
  } catch {
    Write-Err "Failed to download ps2exe: $_"
    exit 1
  }
}

# Run ps2exe
try {
  Write-Info "Running ps2exe compiler..."
  & $ps2ExePath -InputFile $scriptToConvert -OutputFile $OutputPath -iconFile $null -consoleMode $true
  
  if (Test-Path -LiteralPath $OutputPath) {
    $size = (Get-Item -LiteralPath $OutputPath).Length / 1MB
    Write-Success "EXE created: $OutputPath ($([math]::Round($size, 2)) MB)"
    Write-Info "You can now run: $OutputPath -Path 'C:\Downloads'"
  } else {
    Write-Err "EXE was not created."
    exit 1
  }
} catch {
  Write-Err "Compilation failed: $_"
  exit 1
}
