[← Return to index](README.md)

# Getting started

A tour of the main window so you know where everything is. If you haven't installed it yet, start by
[Installation](installation.md).

## The first time

When you open Trading Hub for the first time, the **Trading** section appears with a chart of `SIM-BTC` in 5 minutes, in
footprint mode, with Big Trades and Delta indicators. That instrument comes out of the **Simulator**, a simulated market
that works without internet and is used to practice.

Everything you trade goes to the **simulated account**. Nothing moves real money until you enable it by hand, connection by
connection, in [Connections](connections.md).

These connections are already created:

| Connection | At startup | What it provides |
|---|---|---|
| Simulator | enabled | Simulated SIM-BTC, SIM-ETH, SIM-ES, and SIM-NQ |
| Binance Futures | enabled | Public crypto data, no account |
| Binance Spot | enabled | Public crypto data, no account |
| Bybit | enabled | Public USDT perpetual data |
| Rithmic | disabled | CME futures (need your credentials) |

## The main window

### Sections, in the title bar

Navigation is integrated into the macOS title bar, next to the window buttons:

| Section | Shortcut | Purpose |
|---|---|---|
| Trading | ⌘1 | Charts, DOM, trade tape, and Chart Trader. |
| Account | ⌘5 | Balance, positions, orders, executions, trades and statistics. |
| Backtesting | ⌘2 | Strategies in C#, backtests, optimization and the chat with Claude. |
| Live | ⌘7 | Automatic running strategies, portfolios and the “Stop All” button. |
| Data | ⌘3 | Saved history, downloads, import files, Databento and replay. |
| Connections | ⌘4 | Markets, brokers and accounts. |
| Settings | ⌘, | Appearance, time zone, shortcuts, sleep mode and folders. |

In narrow windows (less than 1,360 pixels wide) the bar shows only icons; the name appears on hover.
Drag the bar to move the window, and double-click it to maximize.

### To the right of the bar

- **Simulated 0.00 · 10,000.00**: the result of the day and the balance of the simulated account. Click to go to Account.
- **Sun or moon**: go to light or dark mode.
- **Connection pill**: the active chart connection and its status. Click it to open a menu with “Reconnect now” and
  “Connections…”.

| State | Meaning |
|---|---|
| connected | Data arrives. |
| connecting… / reconnecting… (attempt N) | Connecting; after a drop, missing data backfills automatically. |
| no data N min ago | The connection is still up, but no data has arrived for a while. |
| market closed · opens (day and time) | Futures after hours: this is not a drop; data resumes automatically. |
| error / disconnected | Check the connection in Connections. |

If the Mac slept, Trading Hub reconnects and backfills missing data when it wakes.

### The status bar

At the bottom. On the left is the latest message: orders sent, history uploads, price alerts, and
errors (hover to see the full text). On the right is the active chart summary: “N trades · N candles ·
from dd/MM HH:mm”, or “Loading history N%” while loading.

## The Trading section

![The Trading section with two charts, each Chart Trader, and the right panel with the DOM and trade tape](img/trading.png)

1. **Chart bar**, above: instrument, history, periods, chart type, indicators, footprint, colors,
   strategies, templates, design, panels, separate window and new tab. Everything is explained in [Charts](charts.md).
2. **Tabs**: the strip appears when there is more than one. Each tab can have 1 to 6 charts.
3. **The chart**, with its drawing bar on the left and its own Chart Trader on the right (collapse it with the
   small arrow).
4. **Right panel**: Chart Trader, DOM, and trade tape, stacked vertically. Change the width by dragging the
   edge, and the height of each section with the dividers. Hide a section with its close button or detach it into its own window.
5. **Replay bar**, at the bottom: appears when the active chart is in replay (see [Data](data.md#replay)).

To start trading, see [Trading](trading.md).

## The simulated account

- Starts with a balance of 10,000. You can change it in **Account → Initial balance**.
- Works with any market: the Simulator or real data from Binance, Bybit, Rithmic and other connections.
- Fills orders against the real market: market orders at the best bid or ask; limit orders when the price trades through
  your price or when more volume trades at your price than what was ahead of you in the queue when you sent it; stop orders
  when the trigger price trades, filling at market.
- Balance and history persist: they do not reset when you close the app. To start over, use “Reset
  simulated account” in Account.

## Open a chart

1. In Trading, click the instrument selector (the first button on the chart bar) or tap ⌘K.
2. Choose the connection above, or **Favorites** to see only starred instruments.
3. Type to search (BTC, ETH, NQ…). Use the arrow keys to move through the list.
4. Enter or double-click opens it in the active chart. ⌘Enter opens it in a new tab.
5. The star in each row marks it as a favorite.

When a chart opens, recent history loads (how much depends on the connection). To go further back, use the history button
in the bar. A new tab opens with ⌘T or the **+** button: it copies the active chart unless you have
chosen a template for new tabs. ⌘W closes the active chart; at least one tab always remains.

To view real markets with your accounts, add them to [Connections](connections.md).

## The Mac menu

With Trading Hub at the forefront, the macOS menu bar has these menus:

| Menu | Options |
|---|---|
| File | New chart (⌘T), Close chart (⌘W), Search instrument… (⌘K), Save workspace (⌘S), Open data folder |
| Chart | Indicators… (⌘I), Edit footprint… (⇧⌘F), Auto scaling (⌥⌘A), Go to last price (⌘End), Design, Output charts to a window (⇧⌘D), Panels, Configure DOM… |
| Go | The seven sections, with their shortcuts |
| Code | Indicator Editor… (⇧⌘E), Open indicators folder, Recompile indicators, Backtesting of strategies…, Live strategies…, Open strategy folder |

The workspace (tabs, charts, layouts, and panels) is saved on close and restored on launch.

## The icon in the menu bar

Trading Hub leaves an icon at the top right, in the macOS menu bar. Tapping it opens a menu with:

- the connection of the active chart;
- **Live strategies (N)**, with the size, position and result of each one;
- **Open positions (N)**;
- “View the live dashboard…”, “Open Trading Hub”, and “Quit Trading Hub”.

The live dashboard shows strategies and positions in real time, with the same buttons to open or exit.

**Closing is not quitting.** The red window button hides the window and Trading Hub keeps running in the menu bar, with
strategies active. The icon and this behavior are controlled by **Settings → Live strategies → Stay in the
menu bar when closing the window**, which is enabled by default. If you turn it off, the icon disappears and closing the window quits
the app. To quit completely, use ⌘Q or “Quit Trading Hub”. If the window is hidden, click the Dock icon to
reopen it.

## Keyboard shortcuts

In addition to the menu shortcuts, **Settings → Keyboard shortcuts** has configurable shortcuts. They work in Trading and in
detached chart windows, on the active chart. To change one, click its button and press the
combination; Esc cancels and ⌫ leaves it unassigned. “Reset to defaults” restores the originals.

| Factory shortcut | Action |
|---|---|
| ⌥1 to ⌥6 | Period of 1 minute, 5 minutes, 15 minutes, 1 hour, 4 hours and daily |
| ⌥H | Draw a horizontal line |
| ⌥T | Show or hide the Chart Trader from the chart |
| ⌃Tab / ⌃⇧Tab | Next/previous tab |

Trading shortcuts (buy, sell, close, reverse, cancel, partial, breakeven, and others) are unassigned by
default: everyone chooses their own. See [Trading](trading.md#shortcuts-for-trading).

On the chart they also work:

| Key | Action |
|---|---|
| Esc | Return to the cursor, cancel dragging an order or unhighlight a trade |
| ⌫ or Del | Delete selected drawings |
| Home or End | Jump back to the latest data with auto scaling |
| ← / → | Move the chart |
| + / − | Make the candles larger or smaller |

## Settings that should be looked at

- **General**: appearance (dark or light), charts time zone and alert sounds.
- **Trading**: “Confirm real orders” (see [Trading](trading.md#security)).
- **Live strategies**: stay in the menu bar, resume strategies on launch, and choose the contract automatically
  (minis or micros).
- **Sleepless mode**: the Mac does not sleep while live strategies are running; optionally, keep running with the lid closed.
- **Data**: “Record what comes in live” and access to folders.

## What's next

- [Connections](connections.md): add markets and brokers.
- [Charts](charts.md): periods, footprint, drawings, indicators and templates.
- [Trading](trading.md): Chart Trader, DOM, orders, stops and targets, and the Account section.
- [Data](data.md): saved history, downloads, import, Databento and replay.
