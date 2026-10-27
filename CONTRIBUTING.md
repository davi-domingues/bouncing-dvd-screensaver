# Contributing

Thanks for your interest in improving Bouncing DVD Screensaver!

## Reporting bugs and requesting features

Open an [issue](https://github.com/davi-domingues/bouncing-dvd-screensaver/issues/new/choose) using one
of the templates. For bugs, include your Windows version, whether you use a laptop panel or an
external monitor, and the exact steps to reproduce.

## Development setup

Requirements: Windows 10/11 x64 with .NET Framework 4.6.2+ and the WebView2 Runtime.

```powershell
git clone https://github.com/davi-domingues/bouncing-dvd-screensaver.git
cd bouncing-dvd-screensaver
powershell -ExecutionPolicy Bypass -File .\build.ps1
.\dist\bouncing-dvd-screensaver.exe
```

Project layout:

| Path | Purpose |
|---|---|
| `src/BouncingDvdScreensaver.cs` | Window, brightness, keep-awake, options |
| `src/web/index.html` | YouTube player page and input capture |
| `lib/` | Vendored WebView2 SDK binaries (see `lib/README.md`) |
| `build.ps1` / `install.ps1` / `uninstall.ps1` | Build and per-user install scripts |

## Guidelines

- Keep the code compatible with the C# 5 compiler that ships with .NET Framework 4.x
  (no `$""` interpolation, `?.`, `nameof`, expression-bodied members, etc.). The build uses
  `/warnaserror+`, so warnings fail it.
- Keep it dependency-free: no new NuGet packages or binaries without discussion in an issue.
- Follow `.editorconfig` (4 spaces in C#, 2 spaces in PowerShell/HTML/YAML, LF line endings).
- Manually test before opening a PR: brightness dims, mouse wakes, ESC exits and restores brightness.
- Update `README.md` and `CHANGELOG.md` (under `Unreleased`) when behavior changes.

## Pull requests

1. Fork the repository and create a branch from `main`.
2. Make focused commits with clear messages.
3. Make sure the **Build** workflow passes.
4. Open a pull request and fill in the template.

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE).
