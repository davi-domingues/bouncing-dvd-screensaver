<#
.SYNOPSIS
  Installs Bouncing DVD Screensaver for the current user (no admin rights needed).

.DESCRIPTION
  Copies the app to %LOCALAPPDATA%\Programs\BouncingDvdScreensaver, adds that folder to the
  user PATH (so "bouncing-dvd-screensaver" works in any new terminal) and creates
  a Desktop shortcut.

  Works from a source checkout (builds first if needed) or from an extracted
  release zip (where the executable sits next to this script).
#>
[CmdletBinding()]
param(
  [switch]$NoShortcut,
  [switch]$NoPath
)

$ErrorActionPreference = 'Stop'
$appName = 'BouncingDvdScreensaver'
$exeName = 'bouncing-dvd-screensaver.exe'
$target  = Join-Path $env:LOCALAPPDATA "Programs\$appName"

# Locate the files to install: release zip layout first, then source checkout.
if (Test-Path (Join-Path $PSScriptRoot $exeName)) {
  $source = $PSScriptRoot
} else {
  $source = Join-Path $PSScriptRoot 'dist'
  if (-not (Test-Path (Join-Path $source $exeName))) { & (Join-Path $PSScriptRoot 'build.ps1') }
}
if ((Resolve-Path $source).Path -eq $target) { throw "Already running from the install folder: $target" }

Get-Process -Name ([IO.Path]::GetFileNameWithoutExtension($exeName)) -ErrorAction SilentlyContinue | Stop-Process -Force
if (Test-Path $target) { Remove-Item $target -Recurse -Force }
New-Item -ItemType Directory -Force $target | Out-Null
Copy-Item (Join-Path $source '*') $target -Recurse -Force
Get-ChildItem $target -Recurse | Unblock-File

if (-not $NoPath) {
  $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
  $entries = @($userPath -split ';' | Where-Object { $_ })
  if ($entries -notcontains $target) {
    [Environment]::SetEnvironmentVariable('Path', (($entries + $target) -join ';'), 'User')
    Write-Host "Added $target to the user PATH. Open a new terminal to use 'bouncing-dvd-screensaver'."
  }
}

if (-not $NoShortcut) {
  $shell = New-Object -ComObject WScript.Shell
  $lnk = $shell.CreateShortcut((Join-Path ([Environment]::GetFolderPath('Desktop')) 'Bouncing DVD Screensaver.lnk'))
  $lnk.TargetPath = Join-Path $target $exeName
  $lnk.WorkingDirectory = $target
  $lnk.IconLocation = "$env:WINDIR\System32\imageres.dll,-27"
  $lnk.Description = 'Bouncing DVD Screensaver - keeps your PC awake (ESC to exit)'
  $lnk.Save()
  Write-Host 'Created Desktop shortcut "Bouncing DVD Screensaver".'
}

Write-Host "Installed to $target"
