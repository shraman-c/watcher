<#
.SYNOPSIS
  Watches a folder and, on new file creation, moves files into subfolders based on file type.

.DESCRIPTION
  Uses FileSystemWatcher to react only to Created events. Each new file is categorized by extension
  and moved into a subfolder under the watched path (e.g., Images, Documents, Audio, etc.).
  Unknown types go to an 'Other' folder. Customize categories via -Rules.

.PARAMETER Path
  The directory to watch for new files.

.PARAMETER MoveUnknownTo
  Target folder name for unknown file types. Defaults to 'Other'.

.PARAMETER Quiet
  Suppress informational logs; only errors will be printed.

.PARAMETER Rules
  Hashtable mapping category names to arrays of extensions (without dot). Case-insensitive.

.EXAMPLE
  ./watch-organizer.ps1 -Path "C:\Downloads"

.EXAMPLE
  ./watch-organizer.ps1 -Path "C:\Incoming" -Quiet

.EXAMPLE
  $custom = @{ Code = @('py','js','ts','cs','java'); Archives = @('zip','rar','7z'); }
  ./watch-organizer.ps1 -Path "C:\Downloads" -Rules $custom -MoveUnknownTo "Misc"

.NOTES
  Press Ctrl+C to stop. The script keeps running by waiting for events.
#>

[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)]
  [ValidateScript({ Test-Path $_ -PathType Container })]
  [string]$Path,

  [string]$MoveUnknownTo = 'Other',

  [bool]$Quiet = $false,

  [hashtable]$Rules = @{ 
    'Images'   = @('jpg','jpeg','png','gif','bmp','tiff','webp','svg');
    'Videos'   = @('mp4','mov','mkv','avi','wmv','flv','webm','m4v');
    'Audio'    = @('mp3','wav','flac','aac','ogg','m4a','wma');
    'Documents'= @('pdf','doc','docx','xls','xlsx','ppt','pptx','txt','rtf');
    'Archives' = @('zip','rar','7z','tar','gz','bz2');
    'Code'     = @('py','js','ts','tsx','jsx','java','cs','cpp','c','h','go','rs','rb','php','html','css','json','yaml','yml','xml','sql','sh','ps1','bat');
    'Installers' = @('exe','msi','msix','apk','dmg','pkg');
    'Fonts'    = @('ttf','otf','woff','woff2');
  }
)

# Normalize rules: lower-case extensions, ensure arrays
$normalizedRules = @{}
foreach ($cat in $Rules.Keys) {
  $exts = @()
  foreach ($e in $Rules[$cat]) { if ($null -ne $e) { $exts += $e.ToString().ToLowerInvariant().TrimStart('.') } }
  $normalizedRules[$cat] = $exts
}

function Write-Info($msg) { if (-not $Quiet) { Write-Host "[INFO] $msg" -ForegroundColor Cyan } }
function Write-Err($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }

function Get-CategoryForExtension([string]$ext) {
  foreach ($cat in $normalizedRules.Keys) {
    if ($normalizedRules[$cat] -contains $ext) { return $cat }
  }
  return $MoveUnknownTo
}

function Ensure-Directory([string]$dir) {
  if (-not (Test-Path -LiteralPath $dir -PathType Container)) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
  }
}

function Wait-ForFileReady([string]$filePath, [int]$maxAttempts = 20, [int]$delayMs = 250) {
  for ($i=0; $i -lt $maxAttempts; $i++) {
    try {
      $fs = [System.IO.File]::Open($filePath,[System.IO.FileMode]::Open,[System.IO.FileAccess]::Read,[System.IO.FileShare]::Read)
      $fs.Close()
      return $true
    } catch {
      Start-Sleep -Milliseconds $delayMs
    }
  }
  return $false
}

function Get-UniqueDestinationPath([string]$destDir, [string]$fileName) {
  $base = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
  $ext = [System.IO.Path]::GetExtension($fileName)
  $candidate = Join-Path $destDir $fileName
  $counter = 1
  while (Test-Path -LiteralPath $candidate) {
    $candidate = Join-Path $destDir ("{0} ({1}){2}" -f $base, $counter, $ext)
    $counter++
  }
  return $candidate
}

# Prepare watcher (top-level only; does not recurse into subfolders)
$fsw = New-Object System.IO.FileSystemWatcher
$fsw.Path = (Resolve-Path -LiteralPath $Path).Path
$fsw.Filter = '*.*'
$fsw.IncludeSubdirectories = $false
$fsw.NotifyFilter = [System.IO.NotifyFilters]'FileName, Size, CreationTime'
$fsw.EnableRaisingEvents = $true

Write-Info "Watching '$($fsw.Path)' (top-level only). Press Ctrl+C to stop."

# Event: Created
$action = {
  param($sender, $eventArgs)
  $fullPath = $eventArgs.FullPath
  $name = [System.IO.Path]::GetFileName($fullPath)

  $context = $event.MessageData
  $watchPath = $context.Path
  $unknownTarget = $context.MoveUnknownTo
  $quietMode = [bool]$context.Quiet
  $rules = $context.Rules

  function Local-WriteInfo($msg) { if (-not $quietMode) { Write-Host "[INFO] $msg" -ForegroundColor Cyan } }
  function Local-WriteErr($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }

  # Ignore directories
  if (Test-Path -LiteralPath $fullPath -PathType Container) { return }

  # Wait until file is ready
  if (-not (Wait-ForFileReady -filePath $fullPath)) {
    Local-WriteErr "File not ready after retries: $name"
    return
  }

  $ext = [System.IO.Path]::GetExtension($name).ToLowerInvariant().TrimStart('.')
  if ([string]::IsNullOrWhiteSpace($ext)) { $category = $unknownTarget }
  else {
    $category = $unknownTarget
    foreach ($cat in $rules.Keys) {
      if ($rules[$cat] -contains $ext) { $category = $cat; break }
    }
  }
  $destDir = Join-Path $watchPath $category
  Ensure-Directory -dir $destDir

  $destPath = Get-UniqueDestinationPath -destDir $destDir -fileName $name

  try {
    Move-Item -LiteralPath $fullPath -Destination $destPath
    Local-WriteInfo "Moved: $name -> $category"
  } catch {
    Local-WriteErr "Failed to move '$name': $($_.Exception.Message)"
  }
}

# Register event handler with context
$subscription = Register-ObjectEvent -InputObject $fsw -EventName Created -SourceIdentifier 'OrganizeFileCreated' -MessageData @{ Path = $fsw.Path; MoveUnknownTo = $MoveUnknownTo; Quiet = $Quiet; Rules = $normalizedRules } -Action $action

try {
  while ($true) { Wait-Event -Timeout 5 | Out-Null }
} finally {
  if ($subscription) { Unregister-Event -SourceIdentifier 'OrganizeFileCreated' -ErrorAction SilentlyContinue }
  if ($fsw) { $fsw.EnableRaisingEvents = $false; $fsw.Dispose() }
}