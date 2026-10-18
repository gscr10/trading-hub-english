[← Return to index](README.md)

# Indicators

Chart indicators are added from the **Indicators** window (**Indicators** button on the chart bar,
or **Chart → Indicators…**, ⌘I). On the left is the **Library**, with search by name or category;
**Add to chart** (or double-click) adds one, and selecting an indicator already on the chart edits its
properties on the right. **Templates** save an indicator's settings, apply a saved one, or choose which template
new indicators start from. The rest of chart management is in [Charts](charts.md#indicators).

Strategy indicators (`Sma`, `Ema`, `Rsi`…) use a different, simpler API: see [Strategies](strategies.md).

## Included

| Indicator | Category | Main options |
|---|---|---|
| Moving average | Trend | Period, type (simple, exponential, or weighted), and source |
| Bollinger Bands | Trend | Period and number of deviations |
| VWAP with bands | Volume / Order flow | Restarts each session, week, or month; up to three volume-weighted standard deviation bands |
| Volume | Volume / Order flow | Colors; colored by delta or by candle direction |
| Delta | Volume / Order flow | Histogram or candles with maximum and minimum delta |
| Cumulative Delta (CVD) | Volume / Order flow | In candles; whether it restarts each session |
| Big Trades (large trades) | Volume / Order flow | Bubbles on large aggressive orders (merges executions of the same order even if it sweeps several levels). Fixed or adaptive threshold (percentile of recent orders), price placement, colors, and sizes |
| Volume profile per session | Volume / Order flow | Per session, week, or entire visible history; volume, delta, or bid/ask; value area (%), POC, VAH, and VAL |
| RSI | Oscillators | Period |
| MACD | Oscillators | Fast and slow averages, and signal |

They live in the `TradingHub.Indicators.Native` project and appear in the library grouped by category.

### Technical indicators

The `TradingHub.Indicators.Technical` project is designed for the ATAS technical indicator collection (about
250 indicators). Its code is public but has no open license, so it is **not published** in this repository: the project
only includes the minimum needed to compile them (the editor types they use). You can add them in a minute, as explained in
[Add ATAS indicators](#add-atas-indicators), and Trading Hub loads them automatically: they appear in the
library with the others, by category.

## Custom indicators

You write yours in C# and Trading Hub compiles them itself.

- They go in `~/Documents/Trading Hub/Indicators` (subfolders are allowed; `bin` and `obj` are ignored). With the folder
  empty, Trading Hub creates an example: `DeltaPercentExample.cs` (“Example: delta % per candle”).
- Trading Hub watches the folder: when you save, create, delete, or rename a file, it recompiles everything after 0.6
  seconds without changes. You can also force it with **Code → Recompile indicators**.
- All files compile together. On error, the last good build stays active and the
  status bar shows the first error; on success, it reports "Custom indicators compiled in N ms."
- They appear at the top of the library, in the "My indicators" group (with a star) and labeled
  "own". If you change the code of one already on a chart, remove it and add it again to use the new
  version.

### The editor

**Code → Indicator editor...** (⇧⌘E), or the code button in the Indicators window, opens an editor with the file
list on the left. **New** creates an indicator from a template (it asks for the class name), and **Save and
compile** (⌘S or ⌘B) saves and recompiles it; errors are listed below, and clicking one jumps to the line. The folder
button opens it in Finder.

### Example

```csharp
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using TradingHub.Indicators;

[DisplayName("Average range")]
[Category("My indicators")]
public class AverageRange : Indicator
{
    private readonly ValueDataSeries _series = new("Range", "Average range") { Color = Color.Orange, Width = 2 };
    private int _period = 14;

    [Display(Name = "Period", GroupName = "Settings")]
    [Range(1, 1000)]
    public int Period
    {
        get => _period;
        set { _period = value; RecalculateValues(); }
    }

    public AverageRange()
    {
        Panel = IndicatorDataProvider.NewPanel; // in its own panel, below price
        DataSeries[0] = _series;
    }

    protected override void OnCalculate(int bar, decimal value)
    {
        if (bar < Period - 1) return; // not enough candles yet
        decimal sum = 0;
        for (int i = 0; i < Period; i++)
        {
            var candle = GetCandle(bar - i);
            sum += candle.High - candle.Low;
        }

        _series[bar] = sum / Period;
    }
}
```

### The API at a glance

- A public class inheriting from `Indicator`, with a parameterless constructor, `[DisplayName("…")]`, and `[Category("…")]`.
  `System`, `System.Linq`, and `System.Collections.Generic` are already included; add `System.ComponentModel`,
  `System.ComponentModel.DataAnnotations`, `System.Drawing`, and `TradingHub.Indicators` (plus `TradingHub.Indicators.Drawing`,
  `TradingHub.Rendering.Context`, or `TradingHub.Rendering.Tools` if you draw).
- **Series**: `ValueDataSeries` (line or histogram with `VisualType = VisualMode.Histogram`), `RangeDataSeries`
  (bands, with `new RangeValue { Upper = a, Lower = b }`), and `PaintbarsDataSeries` (paints the candles). The main series goes in
  `DataSeries[0]`, and the others go through `DataSeries.Add(…)`.
- **Panel**: price panel by default; `Panel = IndicatorDataProvider.NewPanel` puts it in its own panel.
- **Parameters**: public properties with `[Display(Name = "…", GroupName = "…")]` and, optionally, `[Range(min, max)]`;
  in `set`, store the value and call `RecalculateValues()`.
- **Calculation**: `OnCalculate(int bar, decimal value)` is called for each candle (`bar` from 0, `value` the close).
  `GetCandle(bar)` provides `Open`, `High`, `Low`, `Close`, `Volume`, `Ask`, `Bid`, `Delta`, `MaxDelta`, `MinDelta`, and `Time`;
  `CurrentBar` is the candle count. Handle negative indices at the start.
- **Custom drawing** (optional): in the constructor, set `EnableCustomDrawing = true;` and
  `SubscribeToDrawingEvents(DrawingLayouts.Final);`, then override `OnRender(RenderContext context, DrawingLayouts layout)`
  using `ChartInfo.GetXByBar(bar)`, `ChartInfo.GetYByPrice(price)`, and the `context` methods (`DrawLine`,
  `FillRectangle`, `DrawString`…). Visible candles range from `FirstVisibleBarNumber` to `LastVisibleBarNumber`.

Claude can also write indicators: ask in the Backtesting panel and it leaves them in this folder (see
[Claude](claude.md)).

## Extra DLLs

If you have indicators already compiled into a DLL (classes inheriting from `TradingHub.Indicators.Indicator`), copy it to
`~/Library/Application Support/Trading Hub/Indicators` or to an `Indicators` folder next to the app executable.
They load when you open Trading Hub and appear in their category (from their `[Category]`, or "Other") labeled "extra".
One that fails to load leaves a warning and does not stop the rest.

## Add ATAS indicators

ATAS publishes its indicator code at [AtasPlatform/Indicators](https://github.com/AtasPlatform/Indicators).
There are two ways to use them in Trading Hub.

**All at once**, from inside the app (from the repository folder):

```bash
git clone https://github.com/AtasPlatform/Indicators.git third_party/ATAS-Indicators
python3 tools/port_atas.py
./build-app.sh --install
```

`tools/port_atas.py` copies the indicators from the `Technical` folder to `src/TradingHub.Indicators.Technical/Atas`,
changes ATAS namespaces to Trading Hub namespaces (table below) without touching the logic, and applies a few
fixes so they compile and do not divide by zero. It skips anything that depends on ATAS internals (its editor
windows, the heatmap, and DOM v3). To exclude others, pass them comma-separated in
`TRADINGHUB_PORT_EXCLUDE=File1.cs,File2.cs`. To update when ATAS publishes changes:

```bash
git -C third_party/ATAS-Indicators pull
python3 tools/port_atas.py
./build-app.sh --install
```

`third_party/` and `src/TradingHub.Indicators.Technical/Atas/` are in `.gitignore`: the ATAS code stays on your
copy and is not uploaded if you fork.

**One by one**, without compiling the app: copy the `.cs` into `~/Documents/Trading Hub/Indicators` and compile it as a
custom indicator (with the same namespace adaptation). Indicators that use other ATAS indicators require copying
those as well: for example, ATAS's `CCI.cs` and `Momentum.cs` use their `SMA.cs`. The same applies to C# indicators
from any other public code source written for the ATAS API.

These indicators belong to their authors: use them in compliance with each source's license.

## ATAS API support

Trading Hub's indicator API has the same shape as ATAS (`Indicator`, `OnCalculate`, `DataSeries`,
`OnRender`, `GetCandle`, `ChartInfo`…), so you can paste indicators written for ATAS. Before compiling, Trading Hub
changes only the namespaces:

| In ATAS code | Compiles as |
|---|---|
| `ATAS.Indicators` · `ATAS.Indicators.Drawing` · `ATAS.Indicators.Technical` | `TradingHub.Indicators` · `TradingHub.Indicators.Drawing` · `TradingHub.Indicators.Technical` |
| `ATAS.DataFeedsCore` | `TradingHub.Trading` |
| `OFT.Rendering` and its `Context`, `Context.GDIPlus`, `Settings`, `Tools`, `Control` | `TradingHub.Rendering` and equivalents |
| `OFT.Attributes` · `OFT.Attributes.Editors` · `OFT.Localization` | `TradingHub.Attributes` · `TradingHub.Attributes.Editors` · `TradingHub.Localization` |
| `Utils.Common` · `Utils.Common.Logging` · `Utils.Common.Collections` | `TradingHub.Common` and equivalents |

It also provides the aliases `CrossColor` and `CrossColors` (= `System.Drawing.Color`), `CrossPen`, `CrossPens`,
`CrossSolidBrush`, `CrossKey`, and `CrossKeyEventArgs`, plus the preprocessor symbols `TRADINGHUB` and `CROSS_PLATFORM`.
With `#if TRADINGHUB … #else … #endif`, the same file can contain code compiled only here and code compiled only in ATAS.
The adaptation also applies to strategies. Not everything in ATAS has an equivalent: if something does not compile, the error
message says what is missing.
