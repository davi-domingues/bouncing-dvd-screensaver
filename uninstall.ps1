<#
.SYNOPSIS
  Removes Bouncing DVD Screensaver installed by install.ps1 for the current user.
#>
[CmdletBinding()]
param(
  # Also delete the WebView2 cache in %LOCALAPPDATA%\BouncingDvdScreensaver.
  [switch]$Purge
)

$ErrorActionPreference = 'Stop'
$target = Join-Path $env:LOCALAPPDATA 'Programs\BouncingDvdScreensaver'

Get-Process -Name 'bouncing-dvd-screensaver' -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500

$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
$entries = @($userPath -split ';' | Where-Object { $_ -and $_ -ne $target })
[Environment]::SetEnvironmentVariable('Path', ($entries -join ';'), 'User')

$shortcut = Join-Path ([Environment]::GetFolderPath('Desktop')) 'Bouncing DVD Screensaver.lnk'
Remove-Item $shortcut -Force -ErrorAction SilentlyContinue
Remove-Item $target -Recurse -Force -ErrorAction SilentlyContinue

if ($Purge) {
  Remove-Item (Join-Path $env:LOCALAPPDATA 'BouncingDvdScreensaver') -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host 'Bouncing DVD Screensaver was uninstalled.'
