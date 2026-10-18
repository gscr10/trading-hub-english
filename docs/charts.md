[← Return to index](README.md)

# Charts

All this happens in the **Trading** section (⌘1). Each chart has its drawing bar on the left and its Chart
Trader on the right; the bar above acts on the active chart (the last one you clicked on).

## The chart bar

| Control | Action |
|---|---|
| Link color | Join the chart to a color group (see [Link charts by color](#link-charts-by-color)). |
| Instrument (⌘K) | Shows the symbol, price and variation since the start of the session. Click to choose another. |
| History | How much history to load. |
| Periods | Favorites at a glance and a selector with all of them. |
| Footprint template and ticks per row | Only in footprint mode. |
| Chart type | Candles, Bars, Line, Heikin Ashi, or Footprint. |
| Indicators | Open the indicators window (⌘I). |
| Icons | Edit footprint, Chart settings, Strategies on the chart and Chart templates. |
| Design | How many charts to see in the tab. |
| Panels | Show or hide the right panel, Chart Trader, DOM, and trade tape; also opens “Configure DOM…”. |
| Separate window and **+** | Detach the chart into a window (⇧⌘D) or open a new tab (⌘T). |

If the window is narrow, the bar is arranged in two lines.

## Instrument and history

The instrument selector searches active connections and shows symbol, description, price, 24-hour change, and volume.
Enter opens it in the active chart, ⌘Enter opens it in a new tab; the star marks favorites.

The history button offers 30 minutes to 5 years, “All history”, and “Since a date…”. Recent data
loads tick by tick, with footprint. Older data arrives as connection candles (1 minute; ranges over 400
days use 5 or 15 minutes), and those candles have no footprint. Data downloaded in [Data](data.md) is used tick by tick.
When loading finishes, the status bar reports how much footprint data is available.

## Periods

The favorite periods are on the bar (default: 1m, 5m, 15m, 1h, 4h and D). The selector next to it has all:

| Group | Periods |
|---|---|
| Seconds | 5s, 10s, 15s, 30s |
| Minutes | 1m, 2m, 3m, 5m, 10m, 15m, 30m |
| Hours | 1h, 2h, 4h, 6h, 12h |
| Days | D (daily), W (weekly), MO (monthly) |
| Ticks | 100, 250, 500 and 1000 ticks: each candle has that number of trades |
| Volume | Vol 100, 500 and 1000: each candle brings together that volume |
| Range and others | Range 10 and 20 (candles with an N-tick range), Delta 50 and 100 (candle closes when delta reaches N), Renko 10 |

- **Right-click** a period in the selector to add or remove it from the bar. Periods in the bar are
  highlighted.
- **Custom**: type a code in the field below and press Enter. Examples: `M3`, `H2`, `S20`, `Tick500`,
  `Volume100`, `Range20`, `Delta75`, `Renko5`.

## Chart Types

**Candles**, **Bars**, **Line**, **Heikin Ashi**, and **Footprint**. Switching to footprint widens the candles to
fit the numbers.

## Footprint

![Footprint chart with Delta indicator below, Chart Trader and right panel](img/footprint.png)

In footprint mode two more controls appear in the bar:

- **Footprint template**: how each candle is built. Includes “Bid x Ask (classic)”, “Volume + profile ladder”,
  “Delta ladder + volume profile”, “Double ladder (bid | ask)”, “Double delta ladder”, “Volume
  (cells)”, “Delta (cells)”, “Bid/Ask histogram”, “Volume profile per candle”, and “Bid x Ask + Profile”. Yours
  appear with a star. The last option, “Edit columns…”, opens the editor.
- **Ticks per row**: Auto (adapts to zoom) or from 1 to 1000 ticks.

### Footprint editor

Open it with ⇧⌘F, the columns icon in the bar, or right-click → “Edit footprint…”. Changes appear on the
chart immediately.

1. Choose a starting point under **From a template**.
2. In **Columns** (left to right within each candle), add components: Ladder, Profile,
   Cells, Space, and Candle (which adds a “Thin candle”). Reorder them with the arrows and remove them with the trash icon.
3. Select a column and adjust it on the right:
   - **Type**: Thin candle, Colored cells, Ladder (numbers), Profile (histogram), or Space.
   - **Data**: Volume, Delta, Bid (aggressive sales), Ask (aggressive purchases), Bid x Ask or Trades.
   - **Relative Width**, **Bar Direction** (in profiles), **Highlight** (strong delta, imbalances
     diagonals, POC of the candle or high volume), **Cell colors** (proportional to volume, by delta,
     bid/ask, heatmap, solid or no background), text alignment and numbers on bars.
   - **Background colors** and **Text colors** specific to that column (automatic = those of the theme).
4. In **General options**: per-candle totals, box at the POC, mark imbalances, proportion and minimum
   imbalance volume (3× by default), strong delta threshold, minimum volume to highlight, and whether
   colors are proportional to each candle or to everything visible.
5. Enter a **Template name** and click **Save as template** to use it on other charts.

## Move around the chart

| Action | Result |
|---|---|
| Wheel | Horizontal zoom around the mouse |
| ⇧ + wheel, or swipe sideways | Move through time |
| Wheel on the price axis, or ⌘ + wheel | Vertical zoom |
| Drag price axis / double click | Change scale / return to auto scale |
| Drag time axis / double-click | Change candle width / return to latest |
| Bottom-right button | Appears after scrolling back: return to the latest data |

The [Getting started](getting-started.md#keyboard-shortcuts) shortcuts also work, along with “Auto scaling” (⌥⌘A)
and “Go to last price” (⌘End) in the Chart menu.

## Multiple charts

- **Layouts**: one chart; two (side by side or stacked); three columns; one large plus two on the right;
  one large on top plus two below; four (2 × 2); or six (3 × 2). New slots copy the active chart
  with other periods. If you choose a smaller layout, the remaining charts are saved in the tab.
- With multiple charts, each has a bar with its link color, title, and buttons to **maximize it**
  (it fills the tab until restored), **detach it into a separate window**, and **close it**.
- **Separate windows**: the chart moves out with its own bar, drawings, and Chart Trader, and you can move it to another
  monitor. In that window, ⌘W closes it, ⌘I opens indicators, and ⌘K opens the instrument search. “Return to the main window”
  puts it back in a tab.
- Right-click also offers “Move this chart to a new tab”.

### Link charts by color

Charts with the same color (red, green, blue, yellow, violet, or orange) share the instrument, period, and
cursor crosshair. The color menu lets you choose what is synchronized; it applies to all groups. When a chart joins a group, it
takes the instrument and period from the charts already there.

## Chart settings

The palette icon (or “Chart settings…” on right-click) opens that chart’s colors in
tabs: **Background**, **Candles**, **Axes**, **Price**, **Footprint**, and **Trades**. It also controls the grid,
candle data, last-price line, session starts, and the thickness and style of entry/exit lines.
Each color has a palette, code, and opacity; a dot marks the ones you changed. “Reset all”
restores the defaults, and “Apply to all charts” copies the appearance to the others.

Right-click also controls the **Chart legend** (instrument name, candle values, and
indicator names) and **Mark my trades**: a triangle on each execution and a line from entry to
exit for each trade, including previous days.

## Drawings

The left bar has: cursor (Esc); lines (horizontal, horizontal ray, vertical, trend, ray, and
extended); rectangle or zone; Fibonacci retracement; long or short position; ruler; and text. The small arrow under a
group shows its variants. The last button deletes all drawings on the instrument without asking.

- With ⇧ pressed, the point sticks to the high, low, open or close of the candle.
- Drawings appear on all charts of the same instrument and are saved automatically.
- **Double-click** a drawing to open its panel: line (color, thickness, style), fill, text, extend, Fibonacci
  levels, exact prices, and visibility (hide, show only in one period, lock). “Use as
  default” saves the style for future drawings of that type; “Duplicate” and “Delete” are also available.
- The **long or short position** tool starts with a 1:2 stop and target; with a quantity entered, it shows potential profit or
  loss and marks whether candles reach the target or stop.

**Price alerts**: right-click → “Price alert on…” creates a dotted horizontal line. On a horizontal line
or horizontal ray you can enable “Alert when price hits”. When price crosses it, it rings (if
alert sounds are enabled in Settings), shows a status-bar warning, and disables the alert.

## Indicators

The **Indicators** window (⌘I) has three columns:

1. **Library**, with search. To add one, select it and click “Add to chart” or double-click it.
2. **On chart**: show or hide, switch between the price chart and its own panel, and remove.
3. **Settings** for the selected indicator, with **Templates**: save your settings, apply a saved template, and choose
   which templates new indicators start with.

Included: Moving Average, Bollinger Bands, VWAP with deviations, Volume, Delta, Cumulative Delta (CVD), Big Trades
(large trades), Volume Profile per session, RSI, and MACD. In the chart legend each indicator has buttons
to hide it, open its settings, and remove it; double-clicking the name also opens its settings.

Your own C# indicators are written in **Code → Indicator Editor…** (⇧⌘E) or with the key icon in the
library. They live in `~/Documents/Trading Hub/Indicators`, compile on save, and appear first, in the
“My indicators” group (marked with a star).

## Chart templates

The bookmarks icon (or “Chart templates” on right-click) lets you:

- **Save this chart as a template…**: save the style, footprint, colors, visible elements, and
  indicators; this does not include the instrument or period.
- Apply a template to the chart.
- **For new tabs**: copy the active chart or start with a template.
- **Delete a template**.

## DOM and trade tape

The **DOM** is in the right panel and follows the active chart. By default it shows buy orders, bid, price, ask,
sell orders, and session volume. In its header you can group prices (×1 to ×1000 ticks), show order
buttons, center on price, configure it, and detach it into a window. **Configure DOM** lets you choose columns (including
session delta and volume traded at bid and ask), their order and width, row height, font, automatic
centering, “Double-click to trade”, the size from which the book is highlighted, book bars, and colors.

The **trade tape** lists trades as they arrive; executions from the same aggressive order
(same millisecond and side) are combined into one row. Use the wheel to scroll back; the list stays put. Click
the header to return to the latest. How to trade with the DOM is covered in [Trading](trading.md#trade-with-the-dom).

## Volume profile

- The **Volume profile per session** indicator draws the profile per session, per week or the entire visible history,
  volume, delta or bid/ask, with POC, value area (70% by default) and VAH/VAL lines.
- In the footprint, columns of type **Profile** show the profile of each candle.

## Replay

From [Data](data.md#replay) you can replay saved days in a new tab, with speed control, pause, and skips, and
trade with the simulated account as if it were live.

## Strategies on the chart

The robot icon adds one of your Backtesting strategies to the chart (see how to write them in
[Strategies](strategies.md)). It marks entries and exits, both historical and live, and can stay as a visual
overlay or execute on an account. Add or remove it from the chart legend.

Below the settings, **In chart history** shows how it performed over the loaded chart data:

- **Real result, prominently.** Calculated with that market’s commission and slippage, the same as the
  backtest.
- **A mini equity curve** with its drawdown.
- **Statistics:** trades, win %, profit factor, drawdown, average per trade, average win and average loss,
  biggest gain and biggest loss, streaks, Sharpe, Sortino, recovery, exposure, longs and shorts,
  commissions, and candles per trade.
- **What commission and slippage were used.**
