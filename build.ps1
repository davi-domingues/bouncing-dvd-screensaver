<#
.SYNOPSIS
  Builds bouncing-dvd-screensaver.exe into .\dist using the C# compiler that ships with
  .NET Framework 4.x. No .NET SDK, Visual Studio or network access required.

.DESCRIPTION
  Compiles src\BouncingDvdScreensaver.cs against the WebView2 SDK binaries vendored in .\lib
  (Microsoft.Web.WebView2 1.0.2903.40, see lib\README.md) and assembles the
  distributable folder in .\dist.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$lib  = Join-Path $root 'lib'
$dist = Join-Path $root 'dist'
$csc  = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$dlls = @('Microsoft.Web.WebView2.Core.dll', 'Microsoft.Web.WebView2.WinForms.dll', 'WebView2Loader.dll')

if (-not (Test-Path $csc)) { throw "C# compiler not found at $csc. .NET Framework 4.x (64-bit) is required." }
foreach ($d in $dlls) {
  if (-not (Test-Path (Join-Path $lib $d))) { throw "Missing lib\$d. See lib\README.md." }
}

if (Test-Path $dist) { Remove-Item $dist -Recurse -Force }
New-Item -ItemType Directory $dist | Out-Null

& $csc /nologo /target:winexe /platform:x64 /optimize+ /warnaserror+ `
  "/out:$(Join-Path $dist 'bouncing-dvd-screensaver.exe')" `
  "/r:$(Join-Path $lib 'Microsoft.Web.WebView2.Core.dll')" `
  "/r:$(Join-Path $lib 'Microsoft.Web.WebView2.WinForms.dll')" `
  /r:System.Management.dll /r:System.Windows.Forms.dll /r:System.Drawing.dll `
  (Join-Path $root 'src\BouncingDvdScreensaver.cs')
if ($LASTEXITCODE -ne 0) { throw "Compilation failed (exit code $LASTEXITCODE)." }

foreach ($d in $dlls) { Copy-Item (Join-Path $lib $d) $dist }
Copy-Item (Join-Path $lib 'WebView2-LICENSE.txt') $dist
Copy-Item (Join-Path $root 'src\web') $dist -Recurse
foreach ($f in 'LICENSE', 'THIRD_PARTY_NOTICES.md', 'install.ps1', 'uninstall.ps1') {
  if (Test-Path (Join-Path $root $f)) { Copy-Item (Join-Path $root $f) $dist }
}

Write-Host "Build succeeded: $(Join-Path $dist 'bouncing-dvd-screensaver.exe')"
