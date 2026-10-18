[← Return to index](README.md)

# Strategies in C#

A Trading Hub strategy is a C# class that inherits from `Strategy`. The same class, unchanged, runs in the
backtest, optimization, walk-forward, chart, and live account. Here is the complete API
and two examples that compile as-is.

## Where they live and how they are compiled

- Each strategy is a `.cs` file in `~/Documents/Trading Hub/Strategies`. Open the folder from the strategies menu in
  **Backtesting** (“Open strategies folder”), the **Code** menu, or **Settings → Data**.
- With an empty folder, Trading Hub creates four examples: *Moving average crossover*, *Range breakout*, *Delta absorption*,
  and *VWAP reversion*. The app compiles them with Roslyn in memory: you need nothing else.
- They compile when you click **Compile** (⌘B) or **Compile and test**, when you open them in the editor, when Claude finishes
  editing them, and when you are about to use one that has not been compiled yet. **Save** (⌘S) only writes the file: if
  you changed the code, compile before running a backtest, because the latest compiled version is used.
- A public, non-abstract class is needed that inherits from `Strategy`, with a constructor without parameters and
  `using TradingHub.Strategies;` (`System`, `System.Linq`, and `System.Collections.Generic` are already included). Use one per file,
  with the same name: backtest history is stored under the class name, so renaming it starts a new history.
- Compilation applies the same adaptations as for indicators (ATAS namespaces and the `TRADINGHUB` and
  `CROSS_PLATFORM` symbols): see [Indicators](indicators.md).

## Parameters

- `[Strategy("Display name", Description = "…")]` on the class: the name and description shown by the app.
- `[Parameter("Name", Description = "…")]` on a public property with `get` and `set`: appears in the
  backtest, live, and chart parameter forms. Types: `int`, `long`, `double`, `decimal`, `bool`, enums, and `string`.
- `[Optimize(min, max, step)]` additionally includes the parameter in optimization and walk-forward (you can still select
  any numeric parameter manually).

All strategies come with these management parameters, which you can set in the constructor:

| Property | In the app | Default | Description |
|---|---|---|---|
| `Quantity` | Quantity | 1 | Quantity of each order if you do not pass another |
| `StopLossTicks` | Stop loss (ticks) | 0 | Automatic stop at N ticks from the average price |
| `TakeProfitTicks` | Take profit (ticks) | 0 | Automatic target at N ticks |
| `TrailingStopTicks` | Trailing stop (ticks) | 0 | Stop that accompanies the price at N ticks |
| `RegimeFilter` | Regime filter | All | Only enters under certain market conditions |
| `MinVolatilityRank`, `MaxVolatilityRank` | Minimum/maximum volatility (percentile) | 0 and 100 | Only enters within that volatility range |

## Life cycle

| Method | When it is called |
|---|---|
| `OnInit()` | On start. Create indicators here (and set `UseFootprint = true` if you use volume by price). |
| `OnBar(Bar bar)` | When each candle closes. This is the usual place to make decisions. |
| `OnTick(Trade trade)` | On each market trade. Slower. Requires `using TradingHub.Market;` (`Trade` has `Time`, `Price`, `Volume`, and `Side`). |
| `OnOrderFilled(StrategyOrder order)` | When an order fills. In backtests it also reports automatic stops and targets; live, only orders sent by the strategy. |
| `OnSessionStart(DateOnly session)` | At the start of each new session (not called for the first session in the data). |
| `OnStop()` | At the end of the backtest or when stopping the strategy. |

For each candle, market context and indicators update first, then `OnBar` is called. A candle
closes on the first trade of the next one, and a market order sent in `OnBar` fills on that
trade (from `OnTick`, on the following trade). Local time (`LocalTime`, `TimeBetween`, each session day) comes from
the backtest’s **time zone and session start**; live, from the strategy’s last backtest; on a chart,
from **Settings → Chart time zone** with the session at 0:00 (if “This Mac time” is selected, UTC is used).

## Candle and market data

- `Bars[0]` is the last closed candle and `Bars[1]` the previous one (`Bars.Count`; `Bars.At(i)` counts from the first one);
  `Bar` is the last closed one, `FormingBar` the one that is forming and `CurrentBar` its number, from 0.
- Each `Bar` brings `Time` (opening, UTC), `CloseTime` (your last trade, UTC), `Open`, `High`, `Low`, `Close`,
  `Volume`, `BuyVolume` and `SellVolume` (aggressors against the ask and against the bid), `Delta`, `MaxDelta`, `MinDelta`,
  `Trades`, `IsBullish`, `IsBearish`, `Range`, `Body`, `Typical` and `Index`. `PocPrice` (the price with the most volume) and
  `Levels` (buyer and seller volume of each price) need `UseFootprint = true` in `OnInit`.
- `Price` (last price), `Time` (market time, UTC), `LocalTime` (same time in the session time zone),
  `TickSize`, `Instrument` (`Symbol`, `TickSize`, `PointValue`…), `Period`, and `IsBacktest`.
- Position: `Position` (positive = long, negative = short), `IsFlat`, `IsLong`, `IsShort`, `AveragePrice`, `OpenPnL`,
  `NetProfit` (realized result) and `ActiveOrders`.

## Orders

| Method | Description |
|---|---|
| `EnterLong(qty?, name?)` / `EnterShort(…)` | Enter at market. If on the other side, reverse the position; if already on that side, do nothing. |
| `Buy(qty?, name?)` / `Sell(…)` | Market order. |
| `BuyLimit(price, qty?, name?)` / `SellLimit(…)` | Limit order. |
| `BuyStop(price, qty?, name?)` / `SellStop(…)` | Stop order: triggers when price is reached, then goes to market. |
| `ClosePosition(name?)` | Cancel pending orders and close the position at market. |
| `CancelAll()` · `Cancel(order)` · `Modify(order, price)` | Cancel all pending orders, cancel one, or change its price (limits and stops). |

Without a quantity, `Quantity` is used, and prices are rounded to the tick. Each method returns a `StrategyOrder` with `Id`,
`Side`, `Type`, `Quantity`, `Price`, `Name`, `State` (`Active`, `Filled`, or `Canceled`), `IsActive`, `FillPrice`,
`FillTime`, and `IsProtective` (stop or automatic target). The closing order’s name appears in “Exit by” in
backtests and in the order comment live. How fills are simulated is covered in [Backtesting](backtesting.md).

## Automatic stop, target and trailing

With `StopLossTicks` or `TakeProfitTicks` greater than 0, each time the position changes, a stop and a target are placed
at that distance from the average price, linked as OCO (if one fills, the other is canceled). `TrailingStopTicks` makes
the stop follow price when it moves in your favor, without retreating; if `StopLossTicks` is 0, the stop starts at that distance.
`ClosePosition` and `CancelAll` do not touch these orders: they cancel when the position reaches zero. Live, they are
real account orders (OCO is managed by Trading Hub), and trailing moves the stop at most once per second.

## Indicators

They are created in `OnInit` and saved in fields; Trading Hub updates them only before each `OnBar`.

| Method | Description |
|---|---|
| `Sma(n, source)` / `Ema(n, source)` | Simple / exponential moving average (EMA seeds with the SMA) |
| `Rsi(n = 14, source)` · `Atr(n = 14)` · `StdDev(n, source)` | RSI (0–100) · average true range in price · standard deviation |
| `Highest(n, source = High)` / `Lowest(n, source = Low)` | High/low of the last n candles, including the latest |
| `Bollinger(n = 20, k = 2, source)` | Middle band with `Upper`, `Lower`, `UpperAgo(i)`, and `LowerAgo(i)` |
| `Macd(12, 26, 9)` | MACD with `Signal` and `Histogram` |
| `Vwap()` · `CumulativeDelta(resetEachSession = true)` | Session VWAP · cumulative delta |
| `Custom("name", bars => value)` | Custom series calculated on each candle |

`source` is a `PriceSource`: `Close` (default), `Open`, `High`, `Low`, `Typical`, `Median`, `Volume`, or `Delta`.
Each indicator is a series: `ind[0]` is the latest closed candle’s value and `ind[1]` the previous one; it also has
`Value`, `Count`, and `IsReady` (enough candles are available), and can be used directly as `decimal`. Because `Highest` and
`Lowest` include the latest candle, compare the close with `[1]` for a breakout. Prices and volumes are `decimal`:
cast `double` values with `(decimal)`.

## Helpers

- `CrossAbove(a, b)` / `CrossBelow(a, b)`: series `a` crossed above/below `b` (series or number) on the
  latest candle. `Ticks(n)`: n ticks in price. `Round(price)`: round to the nearest tick.
- `TimeBetween(from, to)`: whether session local time is between two `TimeSpan` values (if `from` is later, it crosses
  midnight). `Log("text")`: writes to the backtest **Log** (up to 5,000 lines, UTC) and live **Activity**.

## Drawings on the chart

They appear when the strategy is placed on a chart, over history and live. Times are UTC (like `bar.Time`), and
colors are `"#RRGGBB"`, `"#AARRGGBB"`, or a name (`green`, `red`, `blue`, `cyan`, `yellow`, `orange`, `violet`,
`pink`, `gray`, `white`); without a color, the strategy color is used. Repeating an id replaces the drawing (useful to stretch a
range candle by candle). Limits are 3,000 drawings (oldest are removed) and 20 `Plot` lines.

| Method | Draw |
|---|---|
| `DrawBox(id, from, price1, to, price2, color, fillOpacity = 15, text)` | Rectangle |
| `DrawLine(id, t1, p1, t2, p2, color, width, style)` | Line (`LineStyle.Solid`, `Dashed`, or `Dotted`) |
| `DrawHorizontalLine(id, price, from, color, width, style, text)` | Level extending to the right edge |
| `DrawArrowUp` / `DrawArrowDown(id, time, price, color, text)` | Arrow with the tip at the price |
| `DrawMarker(id, time, price, shape, color, text)` | `MarkerShape.Circle`, `Square`, `Diamond`, `Cross`, `ArrowUp`, or `ArrowDown` |
| `DrawText(id, time, price, text, color)` | Text |
| `Plot("name", value, color, width, style)` | Continuous line candle by candle; a `null` value breaks it |
| `RemoveDrawing(id)` / `ClearDrawings()` | Delete one or all |

## Market context and context filter

Each strategy measures, only with already closed candles, how the market is doing: with the hourly closings of the last 24
hours or, if your candles are one hour or longer, with your last 25 candles.

| Property | Description |
|---|---|
| `MarketTrend` | Trend efficiency: \|net movement\| / total run (0 = pure range, 1 = clean trend; random gives ~0.2) |
| `Regime` | `Range` (up to 0.15), `Mixed`, `Trend` (from 0.35) or `Unknown` without data |
| `MarketDirection` · `MarketChange` | 1 if it rose and −1 if it fell (bullish or bearish) · net movement in % |
| `MarketVolatility` · `VolatilityRank` | Standard deviation of hourly returns, in % · its percentile over the last 30 days (0 = lowest) |
| `ContextAllowsEntries` | Whether the context filter currently allows entries |

They can be `null` while history is missing (7 closes; the percentile requires at least one day). Backtests and live
strategies start with saved hourly closes from the previous month (live also uses exchange candles),
so context usually exists from the first candle. The **context filter** requires no code: it uses the parameters
`RegimeFilter`, `MinVolatilityRank`, and `MaxVolatilityRank`. `RegimeFilter` supports any combination of range, mixed, bullish, and bearish trend: `All`,
`RangeOnly`, `NoTrend`, `TrendOnly`, `NoRange`, `UpTrendOnly`, `DownTrendOnly`, `MixedOnly`, `RangeOrUpTrend`,
`RangeOrDownTrend`, `MixedOrUpTrend`, `MixedOrDownTrend`, `NoDownTrend`, `NoUpTrend`, and `NoMixed`. Outside that context
the strategy does not open or increase positions: only the portion of an order that closes the open position is allowed, and the rest is
canceled (“Context filter: not opening trades now” in the log). Exits always pass, and without data to
judge, the filter does not block. It can be changed live without restarting: see [Live](live.md).

## Example 1: moving average crossover with a time window

It only trades during the session and holds no position outside it.

```csharp
using TradingHub.Strategies;

[Strategy("Scheduled moving average crossover", Description = "Long when the fast average crosses above the slow one, short on the opposite cross, only within the time window.")]
public class ScheduledMovingAverageCrossover : Strategy
{
    [Parameter("Fast average"), Optimize(5, 30, 5)] public int Fast { get; set; } = 10;
    [Parameter("Slow average"), Optimize(20, 100, 10)] public int Slow { get; set; } = 40;
    [Parameter("From (local time)")] public int FromHour { get; set; } = 9;
    [Parameter("To (local time)")] public int ToHour { get; set; } = 16;

    private Ema _fast = null!, _slow = null!;

    // automatic stop 40 ticks from the average price and target at 80, as OCO
    public ScheduledMovingAverageCrossover() { StopLossTicks = 40; TakeProfitTicks = 80; }

    protected override void OnInit() { _fast = Ema(Fast); _slow = Ema(Slow); }

    protected override void OnBar(Bar bar)
    {
        if (!_slow.IsReady) return;
        Plot("Fast", _fast[0], "cyan"); // both averages on the chart, candle by candle
        Plot("Slow", _slow[0], "orange");

        // outside the window, stay flat and open nothing
        if (!TimeBetween(new TimeSpan(FromHour, 0, 0), new TimeSpan(ToHour, 0, 0)))
        {
            if (!IsFlat) ClosePosition("Window end");
        }
        else if (CrossAbove(_fast, _slow)) EnterLong(); // if short, the position flips
        else if (CrossBelow(_fast, _slow)) EnterShort();
    }
}
```

## Example 2: opening range breakout

It measures the range of the first minutes after the open and trades it with a stop order on each side. Try it on an
index future with the **New York** session settings (open at 9:30).

```csharp
using TradingHub.Strategies;

[Strategy("Opening range breakout", Description = "Measures the range of the first minutes and enters with stop orders when it breaks. Closes everything at the closing time.")]
public class OpeningRangeBreakout : Strategy
{
    [Parameter("Opening hour (local)")] public int OpenHour { get; set; } = 9;
    [Parameter("Opening minute")] public int OpenMinute { get; set; } = 30;
    [Parameter("Range minutes"), Optimize(15, 60, 15)] public int RangeMinutes { get; set; } = 30;
    [Parameter("Closing hour (local)")] public int CloseHour { get; set; } = 16;
    [Parameter("Margin beyond the range (ticks)"), Optimize(0, 8, 2)] public int Margin { get; set; } = 2;

    private DateOnly _day;
    private DateTime _rangeStart;
    private decimal _top, _bottom;
    private bool _hasRange, _ordersPlaced;
    private StrategyOrder? _buy, _sell;

    // without StopLossTicks, the stop starts 60 ticks away and then trails the price
    public OpeningRangeBreakout() => TrailingStopTicks = 60;

    protected override void OnBar(Bar bar)
    {
        // candle open in local time: Time is UTC and LocalTime is the same instant in the session zone
        DateTime opened = bar.Time + (LocalTime - Time);
        var open = new TimeSpan(OpenHour, OpenMinute, 0);
        var rangeEnd = open + new TimeSpan(0, RangeMinutes, 0);
        var close = new TimeSpan(CloseHour, 0, 0);
        var now = LocalTime.TimeOfDay;

        var day = DateOnly.FromDateTime(opened);
        if (day != _day) // new day: nothing from the previous day remains
        {
            CancelAll();
            if (!IsFlat) ClosePosition("Day close");
            (_day, _top, _bottom, _hasRange, _ordersPlaced) = (day, decimal.MinValue, decimal.MaxValue, false, false);
        }

        // 1) the range: candles that open between the opening and the end of the range
        if (opened.TimeOfDay >= open && opened.TimeOfDay < rangeEnd)
        {
            if (!_hasRange) _rangeStart = bar.Time;
            _hasRange = true;
            _top = Math.Max(_top, bar.High);
            _bottom = Math.Min(_bottom, bar.Low);
            DrawBox($"range-{day:yyyyMMdd}", _rangeStart, _top, bar.CloseTime, _bottom, "blue", 15, "range");
        }

        // 2) once the range is closed, one stop order on each side (only once per day)
        if (_hasRange && !_ordersPlaced && now >= rangeEnd && now < close && IsFlat)
        {
            _buy = BuyStop(_top + Ticks(Margin), name: "Break up");
            _sell = SellStop(_bottom - Ticks(Margin), name: "Break down");
            _ordersPlaced = true;
            Log($"Day range: {_bottom} to {_top}");
        }

        bool levels = _ordersPlaced && now < close; // range levels while the day lasts
        Plot("Top", levels ? _top : (decimal?)null, "green");
        Plot("Bottom", levels ? _bottom : (decimal?)null, "red");

        // 3) nothing stays open at the closing time
        if (now >= close) { CancelAll(); if (!IsFlat) ClosePosition("Day close"); }
    }

    protected override void OnOrderFilled(StrategyOrder order)
    {
        if (order != _buy && order != _sell) return;
        var other = order == _buy ? _sell : _buy; // one side filled: cancel the other
        if (other is { IsActive: true }) Cancel(other);
        if (order.Side == OrderSide.Buy) DrawArrowUp($"in-{order.Id}", order.FillTime, order.FillPrice, "green", "break");
        else DrawArrowDown($"in-{order.Id}", order.FillTime, order.FillPrice, "red", "break");
    }
}
```

`bar.Time + (LocalTime - Time)` shifts the candle open to session local time, so the range does not depend on
when the first trade of the next candle arrives. The two stop orders wait; when one fills, `OnOrderFilled` cancels
the other. Trailing and the end-of-day close protect the position.

## The editor (Backtesting → Code)

At the top left, choose a strategy or create one with **New strategy...** (templates: Moving average crossover,
Range breakout, Delta absorption, VWAP reversion, or Blank). Next to it you see “Compiled · N ms” or the error count.
The list has search and a **Favorites** section at the top: the star on each strategy adds or removes it without
closing the list.
The editor highlights C# and lists errors below (click one to jump to the line). Buttons: **Save** (⌘S), **Compile**
(⌘B), **Compile and test** (compiles and runs the backtest), **Reference** (API summary), and a checkbox that clears the
file and keeps your history. Claude's panel is on the right; what it writes appears in the editor.

## Let Claude write it

In Claude's panel, describe what you want, for example “a breakout of the first 30 minutes with ATR stops”.
Claude Code creates or edits the file in the strategies folder; Trading Hub compiles it when done and, if it
compiles, offers **Test in backtest**; if not, **Ask it to fix the errors**. With **Laboratory** enabled, it also runs
backtests, optimizations, and walk-forward on its own and leaves a report. Details are in [Claude](claude.md).
