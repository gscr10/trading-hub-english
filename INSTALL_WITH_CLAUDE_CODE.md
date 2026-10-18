# Install Trading Hub with Claude Code

The easiest way to install Trading Hub is to ask your own Claude Code: it compiles the app on your Mac, installs it
in Applications, and leaves it connected to your Claude subscription. You do not need to know how to program.

## Before you start

- A Mac with Apple Silicon (M1 or newer) and macOS 12 or newer.
- [Claude Code](https://claude.com/claude-code) installed and signed in (Pro or Max plan).

## Steps

1. Download the project. With Git:
   ```bash
   git clone https://github.com/ralphtradesz/trading-hub.git
   ```
Or from GitHub: **Code → Download ZIP** and unzip it.
2. Open a Terminal in that folder and start Claude Code:
   ```bash
   cd trading-hub
   claude
   ```
3. Copy and paste this message:

---

```
Install Trading Hub on my Mac following the instructions in CLAUDE.md in this folder:
check that my Mac is compatible, install the .NET 10 SDK if missing (in ~/.dotnet, no sudo),
build and install the app with ./build-app.sh --install, verify that the app can find my Claude Code
(create the link in ~/.local/bin if needed), open it, and tell me in a few lines how to get started.
If Applications already contains another app named “Trading Hub”, ask me before replacing it.
```

---

## What will happen

- Claude Code checks your Mac (Apple Silicon, macOS 12 or newer).
- If you do not have the .NET 10 SDK, it downloads it from Microsoft's official site to `~/.dotnet` (it does not touch the rest of the system or
  ask for your password).
- It builds Trading Hub (a few minutes the first time) and installs it at `/Applications/Trading Hub.app`.
- It verifies that the app finds your Claude Code: because it uses your existing installation and session, AI features
  (the Backtesting chat, the laboratory, and the Live risk agent) stay connected to **your** subscription, without
  API keys.
- It opens the app. You start with a simulated account: nothing trades real money until you enable it in Connections.

## Update

Download the new version (`git pull` or the new ZIP) and ask Claude Code: "update Trading Hub". Your data, connections,
and strategies stay as they were (they live outside this folder).

## If something goes wrong

Ask Claude Code to show you the error and resolve it. The most common problems:

- **The app cannot find Claude Code**: ask it to create the link in `~/.local/bin/claude`.
- **macOS says it cannot verify the app**: the app is signed ad hoc on your Mac when it is compiled; if you copied it from another
  computer, compile it again on yours.
- **The build fails**: confirm that `~/.dotnet/dotnet --list-sdks` shows a version 10.

More details in [docs/installation.md](docs/installation.md).
