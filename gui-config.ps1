<#
.SYNOPSIS
  GUI for configuring and testing watch-organizer parameters.

.DESCRIPTION
  Windows Forms interface to set path, customize rules, and test the file organizer
  without touching the command line.
#>

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Capture script directory at top level
$scriptDir = Split-Path -Parent $PSCommandPath

# Create form
$form = New-Object System.Windows.Forms.Form
$form.Text = "Watch Organizer - GUI Config"
$form.Width = 700
$form.Height = 750
$form.StartPosition = "CenterScreen"
$form.MaximizeBox = $false
$form.BackColor = [System.Drawing.Color]::White

# Helper function
function Add-Label($text, $y, $bold = $false) {
  $label = New-Object System.Windows.Forms.Label
  $label.Text = $text
  $label.Location = New-Object System.Drawing.Point(20, $y)
  $label.AutoSize = $true
  if ($bold) { $label.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold) }
  else { $label.Font = New-Object System.Drawing.Font("Segoe UI", 9) }
  $form.Controls.Add($label)
  return $label
}

function Add-TextBox($y, $width = 300) {
  $tb = New-Object System.Windows.Forms.TextBox
  $tb.Location = New-Object System.Drawing.Point(200, $y)
  $tb.Width = $width
  $tb.Font = New-Object System.Drawing.Font("Segoe UI", 9)
  $form.Controls.Add($tb)
  return $tb
}

function Add-Button($text, $y, $onClick) {
  $btn = New-Object System.Windows.Forms.Button
  $btn.Text = $text
  $btn.Location = New-Object System.Drawing.Point(550, $y)
  $btn.Width = 120
  $btn.Height = 25
  $btn.Font = New-Object System.Drawing.Font("Segoe UI", 9)
  $btn.Add_Click($onClick)
  $form.Controls.Add($btn)
  return $btn
}

function Add-CheckBox($text, $y) {
  $cb = New-Object System.Windows.Forms.CheckBox
  $cb.Text = $text
  $cb.Location = New-Object System.Drawing.Point(200, $y)
  $cb.AutoSize = $true
  $cb.Font = New-Object System.Drawing.Font("Segoe UI", 9)
  $form.Controls.Add($cb)
  return $cb
}

# ========== PATH SECTION ==========
Add-Label "Watch Directory:" 20 $true

$pathBox = Add-TextBox 50 400
$pathBox.Text = "$env:USERPROFILE\Downloads"

Add-Button "Browse..." 45 {
  $browserDialog = New-Object System.Windows.Forms.FolderBrowserDialog
  $browserDialog.Description = "Select directory to watch"
  if ($browserDialog.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
    $pathBox.Text = $browserDialog.SelectedPath
  }
}

# ========== UNKNOWN TARGET SECTION ==========
Add-Label "Unknown File Target Folder:" 90 $true

$unknownBox = Add-TextBox 120 150
$unknownBox.Text = "Other"

# ========== QUIET MODE SECTION ==========
$quietCheck = Add-CheckBox "Quiet Mode (logs errors only)" 160

# ========== CUSTOM RULES SECTION ==========
Add-Label "Custom File Type Rules:" 200 $true
Add-Label "(Optional - leave blank to use defaults)" 225

$rulesBox = New-Object System.Windows.Forms.TextBox
$rulesBox.Multiline = $true
$rulesBox.ScrollBars = "Vertical"
$rulesBox.Location = New-Object System.Drawing.Point(20, 250)
$rulesBox.Width = 650
$rulesBox.Height = 120
$rulesBox.Font = New-Object System.Drawing.Font("Courier New", 8)
$rulesBox.Text = @"
Images = jpg,jpeg,png,gif,bmp
Videos = mp4,mov,mkv,avi
Audio = mp3,wav,flac,aac
Documents = pdf,doc,docx,txt
"@
$form.Controls.Add($rulesBox)

Add-Label "Format: Category = ext1,ext2,ext3 (one per line)" 375

# ========== COMMAND PREVIEW ==========
Add-Label "Command Preview:" 400 $true

$previewBox = New-Object System.Windows.Forms.TextBox
$previewBox.Multiline = $true
$previewBox.ScrollBars = "Vertical"
$previewBox.ReadOnly = $true
$previewBox.Location = New-Object System.Drawing.Point(20, 425)
$previewBox.Width = 650
$previewBox.Height = 80
$previewBox.Font = New-Object System.Drawing.Font("Courier New", 8)
$previewBox.BackColor = [System.Drawing.Color]::LightGray
$form.Controls.Add($previewBox)

# ========== BUTTON PANEL ==========
$startBtn = New-Object System.Windows.Forms.Button
$startBtn.Text = "Start Watcher"
$startBtn.Location = New-Object System.Drawing.Point(20, 520)
$startBtn.Width = 140
$startBtn.Height = 35
$startBtn.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$startBtn.BackColor = [System.Drawing.Color]::LimeGreen
$startBtn.ForeColor = [System.Drawing.Color]::White
$form.Controls.Add($startBtn)

$testBtn = New-Object System.Windows.Forms.Button
$testBtn.Text = "Test Sample Files"
$testBtn.Location = New-Object System.Drawing.Point(170, 520)
$testBtn.Width = 140
$testBtn.Height = 35
$testBtn.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$testBtn.BackColor = [System.Drawing.Color]::DodgerBlue
$testBtn.ForeColor = [System.Drawing.Color]::White
$form.Controls.Add($testBtn)

$previewBtn = New-Object System.Windows.Forms.Button
$previewBtn.Text = "Update Preview"
$previewBtn.Location = New-Object System.Drawing.Point(320, 520)
$previewBtn.Width = 140
$previewBtn.Height = 35
$previewBtn.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$previewBtn.BackColor = [System.Drawing.Color]::Orange
$previewBtn.ForeColor = [System.Drawing.Color]::White
$form.Controls.Add($previewBtn)

$exitBtn = New-Object System.Windows.Forms.Button
$exitBtn.Text = "Exit"
$exitBtn.Location = New-Object System.Drawing.Point(530, 520)
$exitBtn.Width = 140
$exitBtn.Height = 35
$exitBtn.Font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
$exitBtn.BackColor = [System.Drawing.Color]::Gray
$exitBtn.ForeColor = [System.Drawing.Color]::White
$form.Controls.Add($exitBtn)

# ========== STATUS BAR ==========
$statusBar = New-Object System.Windows.Forms.Label
$statusBar.Text = "Ready"
$statusBar.Location = New-Object System.Drawing.Point(20, 570)
$statusBar.Width = 650
$statusBar.Height = 50
$statusBar.BackColor = [System.Drawing.Color]::WhiteSmoke
$statusBar.Padding = New-Object System.Windows.Forms.Padding(5)
$statusBar.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
$statusBar.Font = New-Object System.Drawing.Font("Segoe UI", 8)
$form.Controls.Add($statusBar)

# ========== FUNCTIONS ==========
function Update-Preview {
  $path = $pathBox.Text.Trim()
  $unknown = $unknownBox.Text.Trim()
  $quiet = if ($quietCheck.Checked) { " -Quiet:$true" } else { "" }
  
  if ($rulesBox.Text.Trim()) {
    $cmdPreview = "powershell -ExecutionPolicy Bypass -File .\watch-organizer.ps1 `
  -Path ""$path""``
  -MoveUnknownTo ""$unknown""$quiet``
  -Rules (custom hashtable from input)"
  } else {
    $cmdPreview = "powershell -ExecutionPolicy Bypass -File .\watch-organizer.ps1 `
  -Path ""$path""``
  -MoveUnknownTo ""$unknown""$quiet"
  }
  
  $previewBox.Text = $cmdPreview
}

function Validate-Path {
  $p = $pathBox.Text.Trim()
  if (-not $p) {
    [System.Windows.Forms.MessageBox]::Show("Path is required.", "Error", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
    return $false
  }
  if (-not (Test-Path -LiteralPath $p -PathType Container)) {
    [System.Windows.Forms.MessageBox]::Show("Path does not exist or is not a directory.", "Error", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
    return $false
  }
  return $true
}

function Get-CustomRules {
  $rulesText = $rulesBox.Text.Trim()
  if (-not $rulesText) { return $null }
  
  $rules = @{}
  foreach ($line in $rulesText -split "`n") {
    $line = $line.Trim()
    if (-not $line -or $line.StartsWith("#")) { continue }
    
    $parts = $line -split "="
    if ($parts.Count -ne 2) { continue }
    
    $cat = $parts[0].Trim()
    $exts = @($parts[1].Trim() -split "," | ForEach-Object { $_.Trim() })
    $rules[$cat] = $exts
  }
  return if ($rules.Count -gt 0) { $rules } else { $null }
}

# ========== EVENT HANDLERS ==========
$previewBtn.Add_Click({
  Update-Preview
})

$startBtn.Add_Click({
  if (-not (Validate-Path)) { return }
  
  $path = $pathBox.Text.Trim()
  $unknown = $unknownBox.Text.Trim()
  $quiet = $quietCheck.Checked
  $customRules = Get-CustomRules
  
  $watcherScript = Join-Path $scriptDir "watch-organizer.ps1"
  
  if (-not (Test-Path -LiteralPath $watcherScript)) {
    $statusBar.Text = "ERROR: watch-organizer.ps1 not found in script folder."
    return
  }
  
  # Build argument list
  $args = @("-Path", "`"$path`"", "-MoveUnknownTo", "`"$unknown`"")
  if ($quiet) { $args += "-Quiet:$true" }
  if ($customRules) {
    $rulesJson = $customRules | ConvertTo-Json
    $args += "-Rules", $rulesJson
  }
  
  try {
    Start-Process powershell -ArgumentList (('-ExecutionPolicy Bypass -File "' + $watcherScript + '" ' + ($args -join ' ')) -split ' ') -NoNewWindow:$false
    $statusBar.Text = "[OK] Watcher started. Window will show logs. Press Ctrl+C to stop."
  } catch {
    $statusBar.Text = "ERROR: Failed to start watcher: $_"
  }
})

$testBtn.Add_Click({
  if (-not (Validate-Path)) { return }
  
  $path = $pathBox.Text.Trim()
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
  
  $statusBar.Text = "[OK] Test files created in: $testDir`nThey will be organized when watcher runs."
  
  [System.Windows.Forms.MessageBox]::Show(
    "Test files created in:`n`n$testDir`n`nSample files: jpg, pdf, mp4, mp3, zip, py, xyz`n`nStart the watcher to see them organize!",
    "Test Files Ready",
    [System.Windows.Forms.MessageBoxButtons]::OK,
    [System.Windows.Forms.MessageBoxIcon]::Information
  )
})

$exitBtn.Add_Click({
  $form.Close()
})

# ========== INITIALIZE ==========
Update-Preview
$form.ShowDialog() | Out-Null
