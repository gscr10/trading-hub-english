# Port Trading Hub to Windows with Claude

Trading Hub is built in C#/.NET 10 with Avalonia and SkiaSharp, which work the same on Windows. Most of the
code (data, feeds, charts, backtesting, strategies, indicators) does not depend on the operating system. What is
macOS-specific lives in a few well-identified places, and this guide leaves everything ready for **Claude Code** to do the
work on a Windows PC: the inventory of what needs to change, the staged plan, and the complete prompt.

> Requirements on PC: Windows 10/11 64-bit (x64 or ARM64), [Git](https://git-scm.com), the
> [.NET 10 SDK](https://dotnet.microsoft.com/download), and [Claude Code](https://claude.com/claude-code) with your session
> signed in (`npm install -g @anthropic-ai/claude-code` or the official installer).

## What depends on macOS (inventory)

| Where | What it does on macOS | What to do on Windows |
|---|---|---|
| `build-app.sh` | Publish for `osx-arm64`, build `Trading Hub.app`, sign it with `codesign`, and copy it with `ditto` | A `build-windows.ps1` that publishes self-contained `win-x64` (and `win-arm64`) with an `.ico` icon; optionally an installer (Inno Setup or MSIX) |
| `src/TradingHub.App/Services/Connections.cs` (class `Keychain`) | Stores keys in Keychain with `/usr/bin/security` | Windows Credential Manager (`CredWrite`/`CredRead`/`CredDelete` from `advapi32.dll`) or DPAPI (`ProtectedData`) with one encrypted file per user. Keep the same `Get`/`Set` interface |
| `src/TradingHub.App/Services/SleepGuard.cs` | Prevents the Mac from sleeping (IOKit `IOPMAssertion`) and, with the lid closed, uses `pmset` with administrator permission via `osascript` | `SetThreadExecutionState(ES_CONTINUOUS \| ES_SYSTEM_REQUIRED)` from `kernel32.dll`. Hide the closed-lid option or replace it with an explanation of Windows power settings |
| `src/TradingHub.App/Program.cs` | `CGGetActiveDisplayList` (wait for a screen before starting) | Skip that wait on Windows (`OperatingSystem.IsMacOS()`) |
| `src/TradingHub.App/Services/MenuBar.cs` | Menu bar icon (Avalonia `TrayIcon`) and mouse position via `objc_msgSend` | `TrayIcon` works in the Windows tray; mouse position comes from `GetCursorPos` (`user32.dll`), and the panel opens above the taskbar |
| `src/TradingHub.App/Windows/MainWindow.cs` (`Shell.OpenUrl`, `OpenFolder`…) and `Views/StrategiesView.Reports.cs` | Opens links, folders, and reports with `open` | `Process.Start(new ProcessStartInfo(path) { UseShellExecute = true })`; to reveal a file, `explorer.exe /select,"path"` |
| `src/TradingHub.App/Views/TradingView.cs` | Sounds with `afplay` | `System.Media.SoundPlayer` (WAV) or an Avalonia player; keep the same sound files |
| `src/TradingHub.App/Services/ClaudeCode.cs` | Finds the `claude` CLI in Mac paths, builds `PATH` with `/usr/local/bin`, `/opt/homebrew/bin`…, and opens Terminal with `osascript` for `/login` | Look for `claude.exe`/`claude.cmd` in `%USERPROFILE%\.local\bin`, `%APPDATA%\npm`, `%LOCALAPPDATA%\Programs`, and `PATH` (with `where claude`); open `cmd.exe /k claude /login` or Windows Terminal (`wt.exe`) |
| `MainWindow` (title bar) | Navigation sits inside the macOS title bar (`ExtendClientAreaToDecorationsHint`), with traffic lights on the left | On Windows the window buttons are on the right: leave space on the right instead of the left, and test drag and maximize |
| Shortcuts (`KeyModifiers.Meta`, ~30 uses) | ⌘ | `Ctrl` on Windows: a `Keys.Command` helper that returns `Meta` on Mac and `Control` on Windows, plus help text that says "Ctrl" |
| Fonts (`"Menlo, monospace"`, ~12 uses) | Mac monospaced font | `"Cascadia Mono, Consolas, monospace"` on Windows (a shared `Ui.MonoFont`) |
| `TradingHub.Common.Product` | Data in `~/Library/Application Support/Trading Hub` and files in `~/Documents/Trading Hub` | It already uses `Environment.SpecialFolder`: on Windows, `%APPDATA%\Trading Hub` and `Documents\Trading Hub` remain unchanged. Review comments and UI text that mention “Mac” |
| UI text | Mentions “the Mac”, “macOS Keychain”, “menu bar”, ⌘Q | Platform-specific text (`OperatingSystem.IsWindows()`): "PC", "Credential Manager", "system tray", Alt+F4 |

Everything else (Rithmic, Tradovate, Binance, Bybit, OANDA, Alpaca, cTrader, Databento, the backtest engine, the
walk-forward, Skia charts, the Roslyn strategy compiler, and the MCP laboratory) is cross-platform.

## Staged plan

1. **Build and run.** `dotnet build TradingHub.slnx` and `dotnet test tests/TradingHub.Tests` on Windows. Isolate Mac-specific
   code behind `OperatingSystem.IsMacOS()` or interfaces with two implementations (for example `ISecretStore`,
   `ISleepGuard`, `IShell`), without breaking the Mac version.
2. **Secure keys.** The Windows keystore, with tests that do not touch the user's real credentials
   (as on Mac, an environment variable points to a test file).
3. **System integrations.** Sleep, system tray, opening links and folders, sounds, Claude Code.
4. **Interface.** Title bar, Ctrl shortcuts, fonts, platform-specific text. Review each section with the headless
   scenes (`dotnet run --project tools/TradingHub.UiSnapshot -- <folder> 4 <scene>`).
5. **Packaging.** `build-windows.ps1` with self-contained publishing and an icon, and optionally an installer.
6. **Verification.** Unit tests, scenes, and a manual test with the simulated account and free Binance data.

## The prompt for Claude Code

Clone the repository to the Windows PC, open a terminal in the folder, run `claude`, and paste this:

```
I want to port Trading Hub to Windows without breaking the macOS version. It is a desktop app in C#/.NET 10 with
Avalonia and SkiaSharp (TradingHub.slnx solution). First read CLAUDE.md, README.md, docs/development.md, and
PORT_TO_WINDOWS.md (it contains the inventory of macOS-specific parts and the staged plan).

Rules:
- The macOS version must keep building and working the same. Separate platform-specific code with
  OperatingSystem.IsMacOS()/IsWindows() or, better, small interfaces (ISecretStore, ISleepGuard, IShell, ISound)
  and one implementation per platform. Do not copy whole files.
- Never store keys, tokens, or passwords in text files: on Windows use Credential Manager
  (advapi32 CredWrite/CredRead/CredDelete) or DPAPI. Tests must not touch real credentials: use an
  environment variable (like TRADINGHUB_KEYCHAIN_FILE on Mac) to point at a test store.
- Tools and tests never use the user's real data: headless scenes already use temporary folders
  (TRADINGHUB_DATA_DIR, TRADINGHUB_STRATEGIES_DIR, TRADINGHUB_INDICATORS_DIR); keep it that way.
- Do not add confirmations that block clicks on trading buttons; safety comes from risk limits.
- The interface is in English: new text must be in English too.

Work in stages, and at the end of each one build (dotnet build TradingHub.slnx), run the tests
(dotnet test tests/TradingHub.Tests), and tell me what you changed:
1. Make it build and start on Windows (Program.cs without the macOS screen wait, paths, fonts).
2. Windows key store behind the same interface as Keychain (Connections.cs).
3. Integrations: SleepGuard with SetThreadExecutionState, system tray (TrayIcon + GetCursorPos to position
   the panel), opening links/folders/reports with UseShellExecute, sounds without afplay, and Claude Code (find claude.exe
   or claude.cmd with `where`, %USERPROFILE%\.local\bin, and %APPDATA%\npm; open /login in cmd or Windows Terminal).
4. Interface: title bar with Windows buttons on the right, Ctrl shortcuts (a shared helper instead of
   KeyModifiers.Meta), and platform-specific text (Mac/PC, Keychain/Credential Manager, menu bar/system tray).
5. A build-windows.ps1 that publishes self-contained for win-x64 and win-arm64 with the icon, leaving the app ready in
   a dist\Trading Hub folder. If possible, an optional Inno Setup script.
6. Final verification: unit tests, the relevant headless scenes
   (dotnet run --project tools/TradingHub.UiSnapshot -- screenshots 4 trading conexiones panel footprint mercado),
   and a list of what should be tested manually with the simulated account.

If something cannot work the same as on Mac (for example, keeping the lid closed), hide that option on Windows and
explain it in the Settings text. Show real errors if something fails; do not continue as if it had worked.
```

## After

- If you want to share the result, open a *pull request* in the repository with the changes split by stage.
- Keep tests green on both platforms: every change to one must not break the other.
- Linux follows the same path (Secret Service/libsecret for keys, `systemd-inhibit` for sleep,
  `xdg-open` for links).
