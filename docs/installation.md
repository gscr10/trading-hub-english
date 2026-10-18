[← Return to index](README.md)

# Installation

Trading Hub is compiled on your own Mac from the code in the repository. A script creates the “Trading Hub.app” app,
signs it, and copies it to Applications. The first build takes a few minutes because it downloads the required packages.

## Requirements

| What | Detail |
|---|---|
| Mac | Apple Silicon (M1 or later). For now the app is built only for `arm64`. |
| macOS | 12 or newer. |
| .NET 10 SDK | Only needed to build. Installs into `~/.dotnet` without an administrator password. |
| Internet | The first build downloads the NuGet packages. |
| Claude Code | Optional. Only for AI functions (see [Claude Code](#claude-code-optional)). |

The app that is installed is self-contained: to use it you do not need to have .NET installed.

To check your Mac, open Terminal and run:

```bash
uname -m                 # must print arm64
sw_vers -productVersion  # must be 12 or newer
```

If `uname -m` prints `x86_64`, your Mac is Intel-based and is currently not supported.

## Option 1: Ask Claude Code

If you use Claude Code, it can do the installation for you. The repository includes a `CLAUDE.md` file with the steps to follow.

1. Install Claude Code and sign in with your Claude account.
2. Open a Terminal in the repository folder and run `claude`.
3. Type: "Install Trading Hub."

Claude Code will check that your Mac is compatible, install the .NET 10 SDK into `~/.dotnet` if missing, run
`./build-app.sh --install`, verify that the app can find Claude Code, and open it. It will not ask for personal
information or keys: neither is needed to install. If Applications already contains a different app named "Trading Hub", it will
ask before replacing it.

## Option 2: manual installation

1. Check if you already have the SDK:
   ```bash
   ~/.dotnet/dotnet --list-sdks
   ```
If you see a line starting with `10.`, go to step 3.
2. Install the .NET 10 SDK into `~/.dotnet`:
   ```bash
   curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
   bash /tmp/dotnet-install.sh --channel 10.0
   ```
Then run `~/.dotnet/dotnet --list-sdks` again to confirm.
3. From the repository folder, compile and install:
   ```bash
   ./build-app.sh --install
   ```
4. Open the app from Applications or with:
   ```bash
   open "/Applications/Trading Hub.app"
   ```

There is no need to add `~/.dotnet` to `PATH`: the script adds it only while it runs.

### What `build-app.sh` does

- Publishes the app in Release mode for `osx-arm64`, self-contained.
- Builds the package in a temporary folder outside the Desktop: iCloud adds file attributes that break
  the signature.
- Signs the app and leaves a copy in `dist/Trading Hub.app` inside the repository.
- With `--install`, deletes `/Applications/Trading Hub.app` if it exists, copies the new build, and verifies the signature. On success
  it ends with “Installed in /Applications/Trading Hub.app”.

Without `--install` it only builds `dist/Trading Hub.app` and does not touch Applications.

## Ad hoc signature

If you do not have a developer certificate, the script signs the app "ad hoc" (it appears as "Signed ad hoc").
On the Mac where you built it, it opens without warnings.

- If you copy the app to another Mac, open it the first time with right-click → **Open**.
- If you have a signing certificate, create a file `build.local.env` in the repository folder. The script reads only that file,
  and git ignores it:
  ```bash
  SIGN_IDENTITY="certificate name"
  BUNDLE_ID="com.your-domain.tradinghub"
  ```
`SIGN_IDENTITY` must match a name from `security find-identity -v -p codesigning`. If it does not match,
  it falls back to an ad hoc signature. With a stable certificate, macOS remembers the permissions you granted between updates.

## Claude Code (optional)

Trading Hub does not use Claude API keys: it runs the Claude Code CLI you already have installed, using your session. Claude
usage counts against your normal plan limit. It is used in two places:

- **Backtesting → Claude**: a chat that writes and fixes strategies.
- **Live → Portfolios**: the “Ask Claude for analysis” button, which proposes size changes that you approve.

Without Claude Code the rest of the platform works the same (charting, trading, data, backtesting and live strategies).

The app looks for the CLI in these places and uses the newest version it finds:

- `~/.local/bin/claude`, `~/.claude/local/claude`, `/opt/homebrew/bin/claude`, `/usr/local/bin/claude`,
  `~/.npm-global/bin/claude`;
- folders in `PATH`;
- the copy that comes with Claude's desktop app.

If `command -v claude` points somewhere else, create a link:

```bash
mkdir -p ~/.local/bin && ln -sf "$(command -v claude)" ~/.local/bin/claude
```

You can also indicate a specific CLI with the `TRADINGHUB_CLAUDE_CLI` environment variable. To sign in, run
`claude` in Terminal and use `/login`. If Claude Code asks you to sign in while using the Backtesting chat, the app
displays the “Sign in to Claude Code” button.

## Where your data is stored

| Location | Contents |
|---|---|
| `~/Library/Application Support/Trading Hub` | Settings, connections (no secrets), workspace, trading history, drawings, templates, account history, backtests and live strategies. |
| `~/Documents/Trading Hub/Strategies` | Your C# strategies. The `Reports` subfolder stores Markdown reports. |
| `~/Documents/Trading Hub/Indicators` | Your indicators in C#. |
| macOS Keychain | Connection passwords, secrets, and tokens under the name "Trading Hub". |

Inside the data folder:

| File or folder | Content |
|---|---|
| `settings.json` | General settings, shortcuts, DOM and stop/target templates. |
| `connections.json` | Configured connections, without passwords or secrets. |
| `workspace.json` | Tabs, charts, layouts and open panels. |
| `Ticks/` | Trading history: one file per connection, instrument and day. |
| `Klines/` | Candles downloaded from connections (saved by month), to load long history quickly. |
| `Drawings/` | Drawings of each instrument. |
| `ChartTemplates/`, `IndicatorTemplates/`, `FootprintTemplates/` | Chart, indicator and footprint templates. |
| `Accounts/` | Executions for each account (the history of the Account section). |
| `Backtests/` and `Live/` | Backtesting runs and status of live strategies. |
| `crash.log` | Unexpected errors, in case you have to report a problem. |

On first launch, the `Documents` folders are created. An example indicator appears in `Indicators`
(`DeltaPercentExample.cs`), and example strategies are written to `Strategies` when that folder is empty.

To open the folders from the app: **File → Open data folder**, **Code → Open indicators folder**,
**Code → Open strategies folder**, or the “Open” buttons in **Settings → Data**.

## Update

1. Quit Trading Hub completely with ⌘Q or with “Quit Trading Hub” from its icon in the menu bar. Closing the
   window is not enough: by default, the app keeps running in the menu bar.
2. Bring the new version:
   ```bash
   git pull
   ```
If you downloaded the code as a ZIP, download the new one and replace the folder (save your `build.local.env` if you had one).
3. Compile and install:
   ```bash
   ./build-app.sh --install
   ```

You can also open the folder in Claude Code and ask it to “update Trading Hub”. Your data, connections, keys, and
strategies remain unchanged because they live outside the repository folder.

## Uninstall

1. Quit Trading Hub completely (⌘Q or “Quit Trading Hub”).
2. If you want to remove keys from the Keychain through the app, use the "Delete" button for each connection in **Connections**:
   it erases its secrets. Built-in connections cannot always be removed; their secrets are deleted in step 4.
3. Delete the app:
   ```bash
   rm -rf "/Applications/Trading Hub.app"
   ```
4. Delete the secrets left in the Keychain: open **Keychain Access**, search for “Trading Hub” and delete those
   items. From Terminal, this command deletes them one by one until none remain:
   ```bash
   while security delete-generic-password -s "Trading Hub" >/dev/null 2>&1; do :; done
   ```
5. If you are not going to use it again, delete the data folder (history, accounts, settings). Cannot be undone:
   ```bash
   rm -rf ~/Library/Application\ Support/Trading\ Hub
   ```
6. Your strategies and indicators are in `~/Documents/Trading Hub`. Delete them only if you do not want to keep them.
7. Optional: the repository folder (with `dist/`), the SDK in `~/.dotnet` if not used by another program, and the link
   `~/.local/bin/claude` if you created it for Trading Hub only.

## If something goes wrong

- **`dotnet` not found**: The script uses `~/.dotnet` alone. Check that `~/.dotnet/dotnet --list-sdks` shows
  a version 10.
- **The build stops with an error**: read the first error shown in the Terminal. Check your internet connection (packages are
  downloaded on the first build) and that the installed SDK is version 10.
- **The app closes or stops responding**: check `crash.log` in the data folder.
