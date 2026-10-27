# Changelog

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-10-27

### Added

- Fullscreen, always-on-top screensaver window on the primary display.
- Keeps Windows awake (display, screensaver and sleep) while running.
- Dims the built-in display to the lowest non-zero brightness; software overlay fallback.
- Muted, looping bouncing DVD logo video via WebView2 and the YouTube IFrame Player.
- Mouse activity temporarily restores brightness without closing.
- ESC exits and restores the original brightness.
- `--video <id|url>` and `--wake-seconds <n>` command-line options.
- `build.ps1`, `install.ps1` and `uninstall.ps1` scripts.
- CI build and automated release packaging.

[Unreleased]: https://github.com/davi-domingues/bouncing-dvd-screensaver/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/davi-domingues/bouncing-dvd-screensaver/releases/tag/v1.0.0
