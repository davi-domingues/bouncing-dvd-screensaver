# Bouncing DVD Screensaver

[![Build](https://github.com/davi-domingues/bouncing-dvd-screensaver/actions/workflows/build.yml/badge.svg)](https://github.com/davi-domingues/bouncing-dvd-screensaver/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/v/release/davi-domingues/bouncing-dvd-screensaver)](https://github.com/davi-domingues/bouncing-dvd-screensaver/releases/latest)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
![Platform: Windows](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011%20x64-0078D6)

A fullscreen, dimmed **bouncing DVD logo** screensaver for Windows that **keeps your PC awake**,
so your AI coding agents, long builds and overnight jobs keep running while you step away.

Windows will happily turn off the display, start the lock screen or put the machine to sleep
in the middle of a long agent session. Bouncing DVD Screensaver prevents that, dims the screen
as low as it can go (without ever turning it off), and plays the classic bouncing DVD logo
so you can see at a glance that the machine is still alive.

## Features

- **Keeps Windows awake**: blocks display-off, the Windows screensaver and system sleep while it runs.
- **Minimum brightness, never black**: lowers the built-in panel to its lowest non-zero level.
- **Bouncing DVD logo**: plays [this video](https://www.youtube.com/watch?v=5mGuCdlCcNM) fullscreen, muted, in an endless loop.
- **Peek without closing**: moving or clicking the mouse restores brightness for a few seconds.
- **ESC to exit**: restores your original brightness and ends the process.
- **Configurable**: play any YouTube video and change the wake duration from the command line.
- **Tiny and dependency-free build**: a single C# file compiled with the compiler built into Windows.

## Quick start

1. Download `bouncing-dvd-screensaver-vX.Y.Z-win-x64.zip` from the [latest release](https://github.com/davi-domingues/bouncing-dvd-screensaver/releases/latest).
2. Extract it anywhere.
3. Double-click `bouncing-dvd-screensaver.exe`.

Press **ESC** to exit.

To get a Desktop shortcut and the `bouncing-dvd-screensaver` command in your terminal, run the
installer from the extracted folder:

```powershell
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

## Installation

### From a release (recommended)

```powershell
# After extracting the release zip:
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

This installs for the current user only (no admin rights) into
`%LOCALAPPDATA%\Programs\BouncingDvdScreensaver`, adds it to your user `PATH` and creates a
**Bouncing DVD Screensaver** shortcut on the Desktop. Use `-NoShortcut` or `-NoPath` to skip either step.

### From source

```powershell
git clone https://github.com/davi-domingues/bouncing-dvd-screensaver.git
cd bouncing-dvd-screensaver
powershell -ExecutionPolicy Bypass -File .\build.ps1    # outputs to .\dist
powershell -ExecutionPolicy Bypass -File .\install.ps1
```

The build uses `csc.exe` from .NET Framework 4.x, which ships with Windows. No .NET SDK,
Visual Studio or network access is needed: the WebView2 SDK binaries are vendored in
[`lib/`](lib/README.md) with their SHA-256 hashes.

### Uninstall

```powershell
powershell -ExecutionPolicy Bypass -File "$env:LOCALAPPDATA\Programs\BouncingDvdScreensaver\uninstall.ps1"
```

Add `-Purge` to also delete the WebView2 cache in `%LOCALAPPDATA%\BouncingDvdScreensaver`.

## Usage

Start it from the Desktop shortcut, by double-clicking the executable, or from a terminal:

```powershell
bouncing-dvd-screensaver
```

| Action | Result |
|---|---|
| Move, click or scroll the mouse | Brightness goes back to normal for a few seconds; the screensaver stays open |
| Press **ESC** | Restores brightness, closes the window and ends the process |

### Options

```text
bouncing-dvd-screensaver [--video <id|url>] [--wake-seconds <n>]

  --video <id|url>     YouTube video ID or URL to play (default: 5mGuCdlCcNM)
  --wake-seconds <n>   Seconds of full brightness after mouse activity, 1-3600 (default: 5)
  --help               Show help
```

Examples:

```powershell
bouncing-dvd-screensaver --wake-seconds 15
bouncing-dvd-screensaver --video https://www.youtube.com/watch?v=VIDEO_ID
```

Only one instance runs at a time; launching it again while it is open does nothing.

## How it works

| Concern | Mechanism |
|---|---|
| Stay awake | [`SetThreadExecutionState`](https://learn.microsoft.com/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate) with `ES_DISPLAY_REQUIRED \| ES_SYSTEM_REQUIRED`, released on exit |
| Brightness | WMI `WmiMonitorBrightness` / `WmiSetBrightness`, using the lowest level above 0 reported by the panel |
| Fallback dimming | An 85% black overlay when hardware brightness is not available (e.g. external monitors) |
| Video | [WebView2](https://developer.microsoft.com/microsoft-edge/webview2/) hosting the YouTube IFrame Player (muted, no controls, looping) |
| Exit | ESC handled by the window, the page and a global hotkey; brightness is restored on exit and on unexpected errors |

## Requirements

- Windows 10 or 11, **64-bit** (x64).
- .NET Framework 4.6.2 or later (built into up-to-date Windows 10 and Windows 11).
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (preinstalled on Windows 11; may need to be installed on older Windows 10).
- Internet access to YouTube.

## Known limitations

- **Windows only.** It relies on Win32, WMI and WebView2.
- **Corporate inactivity lock.** A Group Policy / Intune "lock after N minutes of inactivity" (`InactivityTimeoutSecs`) counts only real keyboard/mouse input and will still lock the session. This tool does not, and will not, fake user input to get around it.
- **External monitors** usually do not expose brightness through WMI; they are dimmed with a software overlay instead of the backlight.
- **Primary display only.** Other monitors are left as they are.
- **Force-killing the process** (e.g. Task Manager) skips cleanup, so brightness stays at the minimum. Restore it from the Windows quick settings or run the app again and press ESC.
- **Adaptive brightness** or a power-plan change while running can override the brightness.
- **Offline or blocked YouTube** shows a black screen; the keep-awake and brightness features still work.
- **Unsigned executable.** SmartScreen or application-control policies may warn about or block it.

## FAQ

**Does it prevent my agents from being interrupted by sleep?**
Yes. While it runs, Windows will not turn off the display, start its own screensaver or go to sleep due to inactivity. Closing it releases the request.

**Does it play sound?**
No. The player is muted and the web view itself is muted.

**Is my data sent anywhere?**
Only what any YouTube embed sends to YouTube. The player is loaded from `youtube-nocookie.com`. The app has no telemetry.

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) and the [Code of Conduct](CODE_OF_CONDUCT.md).
For security issues, see [SECURITY.md](SECURITY.md).

## Disclaimer

This project is not affiliated with or endorsed by YouTube, Google, Microsoft or the DVD Format/Logo
Licensing Corporation. The default video, "Bouncing DVD Logo Screensaver 4K 60fps - 10 hours NO LOOP",
is by [Raúl Blanco](https://www.youtube.com/@RaulWhite) and is streamed from YouTube, not distributed with this project.

## License

[MIT](LICENSE) © 2026 Davi Domingues. Third-party components are listed in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
