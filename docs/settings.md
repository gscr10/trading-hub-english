[← Return to index](README.md)

# Settings

**Settings** (⌘,) holds general Trading Hub preferences. Each change is saved instantly. Connection-specific settings are in
[Connections](connections.md), chart details in [Charts](charts.md), each instrument's stop and targets in
[Trading](trading.md), and each portfolio's risk agent in [Live](live.md).

## General

| Option | Default | Description |
|---|---|---|
| Appearance | Dark | **Dark** (black) or **Light** (white) across the whole platform. Also toggled with the sun/moon button on the top bar. Individual chart colors are preserved. |
| Chart time zone | This Mac's time | Time for the time axis, tape, and sessions. Common zones are listed first (Buenos Aires, New York, Chicago, London, Madrid, and UTC), followed by all others. It is also the zone for strategies placed on a chart: with “This Mac's time”, they use UTC. |
| Alert sounds | Enabled | Price alerts, Big Trades, and indicator alerts play a sound. |

## Trading

| Option | Default | Description |
|---|---|---|
| Confirm real orders | Enabled | Before sending a manual order (Chart Trader, DOM, or shortcuts) to a live account, ask for confirmation. This is added to each connection's option: an order goes out without asking only if both are off. Does not affect live strategies. |
| Simulated account | — | Shows the starting balance (changed in the **Account** section). Simulator orders fill against book queue and the real tape. |

## Live strategies

| Option | Default | Description |
|---|---|---|
| Stay in menu bar when closing window | Enabled | Closing the window does not quit Trading Hub: it stays in the Mac menu bar with live strategies running, and its icon opens a panel with strategies and positions in real time. To quit completely, press ⌘Q or choose “Quit Trading Hub” in that panel. |
| Resume them automatically when opening Trading Hub | Enabled | Strategies that were running when Trading Hub closed (or the Mac shut down) start again a few seconds after opening, with their parameters, size, and limits; paused strategies come back paused. Off, they wait for you to resume them from Live. |
| Also those on real accounts | Off | Off (recommended): strategies trading real money wait for you to restart them. It can only be enabled if the previous option is enabled. |
| Choose the contract automatically (minis or micros) | Enabled | If a futures strategy's size calls for a fraction of the mini (for example 1 NQ × 0.5), orders go to the micro with the same exposure (5 MNQ), on the same chart. Whole minis use the mini. With an open position it stays in the same contract and switches when flat. |

## No-sleep mode

| Option | Default | Description |
|---|---|---|
| Keep your Mac awake | Enabled | While live strategies or the risk agent are working, the Mac does not sleep from inactivity (the screen may turn off). It asks for no password and is released only when nothing is left running. |
| Continue with the lid closed | Off | When you close the lid, macOS sleeps everything. With this enabled, when something is running, Trading Hub asks for the administrator password (once each time you open the app) and the Mac keeps working with the lid closed while Trading Hub is open. It reverts when you close Trading Hub, even if it closes abruptly. |
| Only with charger connected | Enabled | On battery, the Mac sleeps when you close the lid even if the previous option is enabled. It can only be changed if the previous option is enabled. |

Status is shown below: whether the Mac is awake and why, and whether closed-lid mode is active. With the lid closed
the Mac does not ventilate the same way: do not keep it in a bag like that.

Internally, the first option uses a macOS power assertion, like a video app. Closed-lid mode runs a
helper with administrator permissions that leaves `pmset disablesleep` at 1 while a warning file exists in the data
folder and Trading Hub is open (with “Charger only”, only while plugged in), then sets it back to 0 when done.

## Keyboard shortcuts

They work in the Trading section and on charts in separate windows, with the active chart receiving them. While you type in a
text field, only shortcuts using ⌃, ⌥, ⌘, or a function key respond.

| Group | Actions (with their factory shortcut) |
|---|---|
| Trading | Buy at market, Sell at market, Buy limit on bid, Sell limit on ask, Close position, Flip position, Cancel all orders, Close 25%, Close 50%, Stop at breakeven, Set stop and template targets, Size: next preset, and Size: previous preset. **They ship unassigned** on purpose. |
| Chart | Period 1 minute (⌥1), 5 minutes (⌥2), 15 minutes (⌥3), 1 hour (⌥4), 4 hours (⌥5), and daily (⌥6); candles, footprint, and Heikin Ashi style; auto-scale; go to last price; draw horizontal line (⌥H); cursor (release tool). |
| Windows | One-chart layout, four-chart layout, next (⌃Tab) and previous (⌃⇧Tab) tab, detach the chart into a window, show or hide the DOM, the chart's Chart Trader (⌥T), and the right panel. |

To change one, click its button and press the combination: Esc cancels and ⌫ leaves it unassigned. If another action
already uses that combination, it is removed and you are told. **Reset to defaults** restores everything. On real
accounts, trading shortcuts still ask for confirmation if “Confirm real orders” is enabled.

## Data

| Option | Description |
|---|---|
| Record what comes live | Enabled by default. Saves every incoming trade so charts, replay, and backtests reopen instantly. |
| Data folder | `~/Library/Application Support/Trading Hub`, with **Open** |
| Custom indicators (C#) | `~/Documents/Trading Hub/Indicators`, with **Open** |
| Strategies (C#) | `~/Documents/Trading Hub/Strategies`, with **Open** |

More about what is saved in [Data](data.md) and [Privacy](privacy.md).

## About

At the bottom are the Trading Hub version, the .NET version, and the author's credit, with a link to their profile.

## Where settings are saved

Settings go to `settings.json` in the data folder. That file also stores choices made elsewhere in
the app: window size, favorite instruments and periods, the right Trading panel, DOM settings,
each instrument's stop and targets, the default style of each drawing, the templates used to start
new charts and indicators, and Claude's chat window position. If the file becomes corrupted, Trading Hub
starts with factory settings.
