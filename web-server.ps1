<#
.SYNOPSIS
  Cross-platform web server GUI for watch-organizer.
  
.DESCRIPTION
  Launches a local HTTP server serving an HTML5 interface for configuring
  and testing the file organizer. Works on Windows, Linux, and macOS with PowerShell 7+.

.NOTES
  Requires: PowerShell 7+ for cross-platform support.
  Opens browser automatically at http://localhost:5000
#>

[CmdletBinding()]
param(
  [int]$Port = 5000
)

$ErrorActionPreference = 'Stop'
$scriptDir = Split-Path -Parent $PSCommandPath

# Helper functions
function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Cyan }
function Write-Err($msg) { Write-Host "[ERROR] $msg" -ForegroundColor Red }
function Write-Success($msg) { Write-Host "[SUCCESS] $msg" -ForegroundColor Green }

# Load HTML
$htmlPath = Join-Path $scriptDir "gui-web.html"
if (-not (Test-Path -LiteralPath $htmlPath)) {
  Write-Err "gui-web.html not found at $htmlPath"
  exit 1
}
$htmlContent = Get-Content -LiteralPath $htmlPath -Raw

# Create HTTP listener
$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://localhost:$Port/")

try {
  $listener.Start()
  Write-Success "Web server started at http://localhost:$Port"
  
  # Open browser
  $url = "http://localhost:$Port"
  if ($PSVersionTable.Platform -eq 'Win32NT') {
    Start-Process $url
  } elseif ($PSVersionTable.Platform -eq 'Linux') {
    if (Get-Command xdg-open -ErrorAction SilentlyContinue) {
      & xdg-open $url &
    } else {
      Write-Info "Open browser and go to: $url"
    }
  } elseif ($PSVersionTable.Platform -eq 'Darwin') {
    & open $url &
  }
  
  Write-Info "Press Ctrl+C to stop the server."
  
  # Request loop
  while ($true) {
    $context = $listener.GetContext()
    $request = $context.Request
    $response = $context.Response
    
    # CORS headers
    $response.Headers.Add('Access-Control-Allow-Origin', '*')
    $response.Headers.Add('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
    $response.Headers.Add('Access-Control-Allow-Headers', 'Content-Type')
    
    # Route: GET /
    if ($request.HttpMethod -eq 'GET' -and $request.Url.LocalPath -eq '/') {
      $response.ContentType = 'text/html; charset=utf-8'
      $buffer = [System.Text.Encoding]::UTF8.GetBytes($htmlContent)
      $response.ContentLength64 = $buffer.Length
      $response.OutputStream.Write($buffer, 0, $buffer.Length)
      $response.Close()
      continue
    }
    
    # Route: POST /api/start-watcher
    if ($request.HttpMethod -eq 'POST' -and $request.Url.LocalPath -eq '/api/start-watcher') {
      $body = $null
      $reader = New-Object System.IO.StreamReader($request.InputStream)
      $body = $reader.ReadToEnd()
      $reader.Close()
      
      try {
        $data = $body | ConvertFrom-Json
        $path = $data.path
        $unknown = $data.unknown
        $quiet = $data.quiet
        $rules = $data.rules
        
        # Validate path
        if (-not (Test-Path -LiteralPath $path -PathType Container)) {
          throw "Path does not exist or is not a directory"
        }
        
        # Build watcher command
        $watcherScript = Join-Path $scriptDir "watch-organizer.ps1"
        $args = @("-Path", "`"$path`"", "-MoveUnknownTo", "`"$unknown`"")
        if ($quiet) { $args += "-Quiet:$true" }
        
        # Launch watcher in new window
        Start-Process powershell -ArgumentList "-NoExit", "-ExecutionPolicy", "Bypass", "-File", $watcherScript, @args
        
        # Send success response
        $response.ContentType = 'application/json; charset=utf-8'
        $json = @{ success = $true; message = "Watcher started" } | ConvertTo-Json
        $buffer = [System.Text.Encoding]::UTF8.GetBytes($json)
        $response.ContentLength64 = $buffer.Length
        $response.OutputStream.Write($buffer, 0, $buffer.Length)
      } catch {
        $response.ContentType = 'application/json; charset=utf-8'
        $json = @{ success = $false; error = $_.Exception.Message } | ConvertTo-Json
        $buffer = [System.Text.Encoding]::UTF8.GetBytes($json)
        $response.ContentLength64 = $buffer.Length
        $response.OutputStream.Write($buffer, 0, $buffer.Length)
      }
      $response.Close()
      continue
    }
    
    # Route: POST /api/create-test-files
    if ($request.HttpMethod -eq 'POST' -and $request.Url.LocalPath -eq '/api/create-test-files') {
      $body = $null
      $reader = New-Object System.IO.StreamReader($request.InputStream)
      $body = $reader.ReadToEnd()
      $reader.Close()
      
      try {
        $data = $body | ConvertFrom-Json
        $path = $data.path
        
        # Validate path
        if (-not (Test-Path -LiteralPath $path -PathType Container)) {
          throw "Path does not exist"
        }
        
        $testDir = Join-Path $path "_test_organize_samples"
        
        # Clean up old test dir
        if (Test-Path -LiteralPath $testDir) {
          Remove-Item -LiteralPath $testDir -Recurse -Force
        }
        
        # Create test dir
        New-Item -ItemType Directory -Path $testDir | Out-Null
        
        # Create sample files
        @{
          "sample.jpg"    = "fake image data"
          "document.pdf"  = "fake pdf"
          "video.mp4"     = "fake video"
          "song.mp3"      = "fake audio"
          "archive.zip"   = "fake zip"
          "code.py"       = "print('hello')"
          "unknown.xyz"   = "unknown type"
        }.GetEnumerator() | ForEach-Object {
          [System.IO.File]::WriteAllText((Join-Path $testDir $_.Key), $_.Value)
        }
        
        # Send success response
        $response.ContentType = 'application/json; charset=utf-8'
        $json = @{ success = $true; testDir = $testDir } | ConvertTo-Json
        $buffer = [System.Text.Encoding]::UTF8.GetBytes($json)
        $response.ContentLength64 = $buffer.Length
        $response.OutputStream.Write($buffer, 0, $buffer.Length)
      } catch {
        $response.ContentType = 'application/json; charset=utf-8'
        $json = @{ success = $false; error = $_.Exception.Message } | ConvertTo-Json
        $buffer = [System.Text.Encoding]::UTF8.GetBytes($json)
        $response.ContentLength64 = $buffer.Length
        $response.OutputStream.Write($buffer, 0, $buffer.Length)
      }
      $response.Close()
      continue
    }
    
    # Route: POST /api/shutdown
    if ($request.HttpMethod -eq 'POST' -and $request.Url.LocalPath -eq '/api/shutdown') {
      $response.ContentType = 'application/json; charset=utf-8'
      $json = @{ success = $true } | ConvertTo-Json
      $buffer = [System.Text.Encoding]::UTF8.GetBytes($json)
      $response.ContentLength64 = $buffer.Length
      $response.OutputStream.Write($buffer, 0, $buffer.Length)
      $response.Close()
      Write-Info "Shutdown requested."
      break
    }
    
    # Route: OPTIONS (CORS preflight)
    if ($request.HttpMethod -eq 'OPTIONS') {
      $response.StatusCode = 200
      $response.Close()
      continue
    }
    
    # 404
    $response.StatusCode = 404
    $response.Close()
  }
} catch {
  Write-Err "Server error: $_"
} finally {
  if ($listener) {
    $listener.Stop()
    $listener.Close()
  }
}
