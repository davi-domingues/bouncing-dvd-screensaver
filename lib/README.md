# Vendored WebView2 SDK binaries

These files come unmodified from the official NuGet package
[Microsoft.Web.WebView2 1.0.2903.40](https://www.nuget.org/packages/Microsoft.Web.WebView2/1.0.2903.40)
and are redistributed under its BSD-style license (`WebView2-LICENSE.txt`).

| File | Package path | SHA-256 |
|---|---|---|
| `Microsoft.Web.WebView2.Core.dll` | `lib/net462/` | `88A3B62F45225A811CDB85DF6DFD95C2BFF9A0E43E3B04F813B125EACA56CC9F` |
| `Microsoft.Web.WebView2.WinForms.dll` | `lib/net462/` | `29FC954C3FDE1749817D158F3DD5EBE9EFD3B3CE9708D86092FB1E9F023A1CB0` |
| `WebView2Loader.dll` | `runtimes/win-x64/native/` | `462B36FD1BE6CA9F7563466A89E57C41EF4A4DEF3E0A84FA885D203AEA4A3AAF` |
| `WebView2-LICENSE.txt` | `LICENSE.txt` | `0AF8F1B807512AAE39C2AC1AA4D0CAE65CABECB6FD554B8439A5162A0D6ECA55` |

Package SHA-256 (`.nupkg`): `EF128016DD1E51C59178C827ED5B8AA3322C57AFA8675D930F8109505542AD74`.

They are vendored so the build works offline with only the tools built into
Windows. The CI workflow re-downloads the package and verifies these hashes on
every run.

To verify locally:

```powershell
Get-FileHash .\lib\* -Algorithm SHA256
```
