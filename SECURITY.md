# Security policy

## Supported versions

Only the latest release receives fixes.

## Reporting a vulnerability

Please **do not** open a public issue for security problems. Report them privately through
[GitHub Security Advisories](https://github.com/davi-domingues/bouncing-dvd-screensaver/security/advisories/new).

Include a description, steps to reproduce and the affected version. You can expect an initial
response within 7 days.

## Scope notes

- The app loads a local page and the YouTube IFrame Player inside WebView2; DevTools and context
  menus are disabled.
- It does not listen on any network port, collect telemetry or require administrator rights.
- Release binaries are built by GitHub Actions from the tagged commit. The vendored WebView2 DLLs
  are verified against their published NuGet package in CI (see `lib/README.md`).
