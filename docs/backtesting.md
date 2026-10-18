[← Return to index](README.md)

# Backtesting

The **Backtesting** section (⌘2) brings together all testing tools. At the top you choose the strategy; below are the tabs
**Code**, **Backtest**, **Optimization**, **Walk-forward**, **History**, **Portfolio** and **Reports**. How to
write a strategy is in [Strategies](strategies.md); what happens in real time, in [Live](live.md).

## Choose data

The backtest walks through the saved real trades one by one: data recorded while you had charts open and
data you downloaded or imported in [Data](data.md). With nothing saved, the list is empty.

- **Instrument (saved data)** shows `SYMBOL · source · N days`, with the range and disk usage below.
- Futures that also have [Databento](data.md#databento) history appear three times:

| Entry | What it uses |
|---|---|
  | **Rithmic + Databento** | Combined: Rithmic takes precedence, and Databento fills only minutes where Rithmic has no trades |
  | **Only Rithmic** | Only data recorded or downloaded from Rithmic |
  | **Databento** | Only data downloaded from Databento |

The same applies to Tradovate (“Tradovate + Databento”, “Only Tradovate”).
- **From** and **To** (the last day is complete), with the quick ranges **1 week**, **1 month**, **3 months**,
  **6 months**, **1 year** and **Everything**, counted backwards from the last saved day (Everything starts on the first).
- When changing instruments the range goes to the last 7 days saved, and the commission % to 0.04 if it is crypto (Binance
  or Bybit) or 0 in the rest.

**Candle period**: a code such as `M5`. Buttons are available for `M1`, `M5`, `M15`, `H1`, `H4`, `D1`, `Tick500`, and
`Range20`. Case does not matter, and chart-bar formats are accepted: `d1`, `D`,
and `1D` are equivalent to `Daily`, as are `5m` and `M5`, `1h` and `H1`, or `1w` and `W1`.

| Code | Candle type |
|---|---|
| `S30`, `M5`, `H1`, `D1` or `Daily`, `W1` or `Weekly`, `Mo1` or `Monthly` | Time-based (seconds, minutes, hours, days, weeks, months) |
| `Tick500` | Every 500 trades |
| `Volume1000` | Every 1,000 volume |
| `Range20` | Every time the range exceeds 20 ticks |
| `Delta300` | When the candle delta reaches ±300 |

Renko is not available in backtests.

## Execution: costs and rules

| Field | Default | Description |
|---|---|---|
| Initial capital | 10,000 | Base of return %, drawdown % and Sharpe |
| Commission % per side | 0.04 in crypto, 0 elsewhere | Percentage of notional (quantity × price × point value) on each entry and exit |
| Fixed commission per unit | 0 | Per contract or unit and per side; added to the percentage (use this for futures) |
| Slippage (ticks) | 1 | Ticks against market orders and stops |
| Limit orders | Only if price trades through | The alternative is “Touches the price (optimistic)” |
| Session time zone and start (h) | UTC, 0 | Define `LocalTime`, `TimeBetween`, each session day (VWAP, cumulative delta, `OnSessionStart`), and hourly statistics |

Available zones are UTC, Buenos Aires, New York, Chicago (CME), and London. The **Parameters** card contains the strategy
parameters and, separately, position-management settings. Your configuration is saved per strategy: these parameters
are used when the strategy is placed on a chart, and [Live](live.md) also uses the candles and session.

### How orders are filled

| Order | Fills |
|---|---|
| Market | With the next market trade, plus or minus slippage. If sent from `OnBar`, with the first trade of the next candle. |
| Limit | At the limit price when a trade crosses it (or touches it, in optimistic mode). If the price was already through the limit when sent, it fills like a market order, never worse than the limit. |
| Stop | When a trade reaches the price, at that trade’s price plus or minus slippage (worse with a gap). |
| Automatic stop and target | As a stop and a limit order, linked as OCO. |

No order fills on the trade that generated it. At the end of the range, any open position closes at the last
price (“End of backtest”). Results are in the instrument currency: price difference × quantity × point
value (for NQ, 20 per point). Market context starts with the previous saved month, as if live.

## Results

Each backtest is stored in **History** with the exact code used. At the top you see **Net result** (and % of
capital), **Profit factor**, **Win rate**, **Max drawdown** (in % and with recovery factor), **Sharpe** (and
Sortino), and **Expectancy**, plus the equity curve. Below are five tabs:

- **Trades**: number, side, entry and exit prices (in your Mac’s time zone), quantity, net result, MAE and
  MFE (maximum adverse and favorable excursion), candles, and “Exit by” (the order that closed the trade).
- **Metrics**: 28 figures, including commissions, return, average per trade, streaks, average candles, time in
  the market, long and short results, simulated ticks, and candle count.
- **By market**, **Monte Carlo**, and **Log** (what the strategy wrote with `Log`, in UTC, up to
  5,000 lines), explained below.

| Metric | How to calculate |
|---|---|
| Profit factor | Gross profit / gross loss (∞ if no losses) |
| Winners | Trades with net result greater than 0, over the total |
| Expectancy | % winners × average win + % losers × average loss |
| Maximum Drawdown | Greater fall from a peak of the capital curve (with the open, at the close of each candle); % is about that peak |
| Recovery factor | Net result / maximum drawdown |
| Sharpe and Sortino | Average of daily returns / their deviation (Sortino only counts negative days), × √252, or × √365 in crypto |

### By market

Each trade records the market condition at entry, using the same strategy-context measures: range,
mixed, bullish or bearish trend (efficiency over the last 24 hours), and low, normal, or high volatility (up to the
33rd percentile of the last month, between 33 and 67, or above 67). The table shows trades, net, profit factor, win %,
and average for each condition, and marks “small sample” when there are very few.

Below, choose conditions and click **Run again only in these conditions**. This is not a slice of the previous backtest:
it is a new run with the context filter, where the strategy decides on each candle using only what was known at that moment
(exits always execute), just like live. The two runs are compared above. Be careful: if you choose conditions
based on these same results, the filter is fitted to that data; validate it with walk-forward or on another period.

### Monte Carlo

**Simulate 1,000 random orderings** reshuffles trades 1,000 times and measures the drawdown of each path: typical
(median), 1-in-20 (95th percentile), extreme (99th percentile), and worst observed, plus the **risk of
ruin** (paths with a drawdown of at least 50% of capital). The chart shows 40 of those paths.

## History

Filter by **All**, **Backtests**, **Optimizations**, **Walk-forward**, or the star (favorites). Each run includes
its metrics, curve, parameters, and settings, with an editable note. Buttons: star, delete, **View
code** (the exact version), **Report** (builds the professional report in Reports), and **Use parameters** (copies them to the
backtest and runs it). Saved in `~/Library/Application Support/Trading Hub/Backtests`, one folder per strategy.

## Optimization

1. Under **What to test**, select the parameters to sweep with their minimum, maximum, and step (those with `[Optimize]` are already
   selected). The combination count is shown: above 3,000 it warns you, and above 20,000 it will not run.
2. **Sort by**: Return / drawdown (default), Net result, Profit factor (capped at 100), Sharpe,
   Sortino, Recovery factor, Expectancy, or win %.
3. **Minimum trades** (10): combinations with fewer trades receive no metric, to avoid rewarding luck.
4. **Optimize**. Uses the data and execution settings from the Backtest tab and tests all combinations (grid),
   parallelized on all cores but one.

**Optimize the timeframe.** At the end of **What to test** is **Candle period**: check it and select the periods
to test (`M1`, `M5`, `M15`, `M30`, `H1`, `H4`, and `D1`). Each parameter combination is tested on each selected
period: tick-by-tick history loads only once, and candles are built per period. Each table row
shows its period, and **Use in Backtest** copies it too. In walk-forward the period is chosen per window as
one more parameter, and the overfitting measurement counts each period as one more trial.

The table shows up to 500 rows sorted by metric. **Use in Backtest** (or double-click) copies the parameters and
runs the backtest. The best row is often a lucky result: validate it with walk-forward. Optimizations and
walk-forward runs are also stored in History.

## Walk-forward

Optimize in a window (in sample), test the chosen in the following days, which the optimization did not see (out
sample), move forward and repeat. What counts is the chained out-of-sample result.

- **In sample** (14 days) and **Out of sample** (7 days). **Rolling**: the optimization window has a fixed
  length and moves forward. **Anchored**: always starts on the first day and grows. Before running, you can see how many windows will result.
- Uses parameters, metrics, and minimum trades from Optimization, plus data and costs from Backtest.
- **Parameter search**: **Automatic** (grid up to 300 combinations, Bayesian beyond that), **Full
  grid**, or **Bayesian**. Bayesian evaluates 80 combinations per window.

Results: **Out of sample** (net, trades, and profit factor), **WF efficiency**, **Winning windows**,
**Out-of-sample drawdown**, **Overfitting**, and **Paths (CPCV)**, plus the out-of-sample curve with each window marked (V1, V2…) and
a table per window with dates, chosen parameters, in-sample and out-of-sample net, and efficiency.

### How it works inside

1. **One run per combination.** Each combination runs only once over the full history, with indicators
   preloaded and market context as live, and each window is measured by trimming that result (trades that
   entered the window and equity hour by hour). No window uses later data.
2. **Tick compression.** Consecutive trades at the same price, side, and second are merged into
   one with the combined volume. Time or range candles remain identical (open, high, low, close, volume,
   delta, and footprint), and orders fill at the same prices with far fewer events. Used in optimization and
   walk-forward only for time or range candles when the strategy does not use `OnTick`; the only change is
   `Bar.Trades`, the number of trades per candle.
3. **Grid or Bayesian.** The grid tests everything and shows the full map. Bayesian (TPE, *Tree-structured Parzen
   Estimator*) starts with well-distributed points (a Latin hypercube of 8 to 30, a quarter of the budget), then
   proposes batches: it separates the best 20% of measured trials, estimates for each parameter which values are
   frequent among good trials (`l(x)`) and bad trials (`g(x)`), and tests where `l(x)/g(x)` is highest.
4. **Plateau, not peak.** Each window chooses by neighborhood average: for each combination,
   `score = (sum of its metric and neighboring grid steps + median) / (neighbors + 2)`. The median pulls
   toward the middle those with few measured neighbors, and a missing neighbor counts worse than the worst. A combination
   that wins alone among losing neighbors is almost always luck; an entire winning region is not.
5. **Efficiency.** `out-of-sample daily profit / in-sample daily profit`, summed across all windows
   (and the same per window). Above 0.5 is a good sign; 0 if it did not win in sample.
6. **PBO with CSCV** (probability of backtest overfitting using combinatorially symmetric cross-validation). Using the
   daily results of all combinations, history is split into 10 blocks (fewer if there are few days), and
all ways of choosing half as “in-sample” (252) are tried: each time, the best-Sharpe combination there is selected and its
   rank `r` on the other half is recorded (1 = worst of `N`). With `ω = r / (N + 1)`, it is in the bottom half when
   `ln(ω / (1 − ω)) ≤ 0`; **PBO** is the proportion of times this happens. Trading Hub also shows how often the
   selected combination **wins** on the other half (**Overfit** card: green below 20% losses, red from
   50%), because a high PBO when everything wins is a plateau, not a problem. Requires at least 4 combinations and 20 days.
7. **CPCV with embargo** (purged combinatorial cross-validation). History is split into 8 consecutive groups (fewer if
   there are few days); for each pair of test groups (28 pairs), the combination is chosen by plateau using the remaining
   days, removing those within the **embargo** of the test groups (typical trade duration,
   1 to 5 days), then measured on those two groups. Since each group is tested in 7 pairs, 7 complete
   out-of-sample paths emerge, each with net, Sharpe, and drawdown. The
   **Paths (CPCV)** card is green if at least 70% win and the median is positive, and red if the median does not win.

## Sharpe deflation and validation

Trying many variants makes the best one look good by chance. The **Deflated Sharpe** measures the probability that
the edge is real after discounting how many trials were run. Given the per-trade out-of-sample Sharpe
(`SR` = mean / standard deviation of net result), skewness `γ3`, kurtosis `γ4` (3 for a normal distribution), and `n` trades:

```
PSR(SR*) = Φ( (SR − SR*) · √(n − 1) / √(1 − γ3·SR + (γ4 − 1)/4 · SR²) )
SR*      = √(max(V, 1/n)) · E[max Z_N],   E[max Z_N] ≈ (1 − γ)·Φ⁻¹(1 − 1/N) + γ·Φ⁻¹(1 − 1/(N·e)),   γ ≈ 0.5772
```

`N` is the number of configurations tested, `V` the variance of their Sharpe, and `Φ` the cumulative normal: `SR*` is the
Sharpe that the best of `N` strategies with no edge would show by luck.

Validation uses fixed rules (`ValidationGate` in `TradingHub.Strategies`) that judge only out-of-sample results:

| Test | Pass | Doubtful | Fail |
|---|---|---|---|
| Sample | ≥ 30 trades | | fewer |
| Result | net > 0 | | net ≤ 0 |
| Profit factor | ≥ 1.2 | ≥ 1.0 | < 1.0 |
| Walk-forward efficiency | ≥ 0.5 | ≥ 0.3 | < 0.3 |
| Consistency (winning windows) | ≥ 60% | ≥ 40% | < 40% |
| Windows | ≥ 4 | fewer | |
| Stability (dispersion of chosen parameters / range width) | ≤ 0.35 | higher | |
| Plateau (winning neighbors of the best) | ≥ 50% | lower | |
| Robustness (winning combinations) | ≥ 25% | lower | |
| Overfitting (chosen combination loses on the other half) | < 20% | < 50% | ≥ 50% |
| CPCV paths | ≥ 70% win | lower | median ≤ 0 |
| Deflated Sharpe | ≥ 90% | ≥ 50% | < 50% |
| Decay (30+ trades) | holds | last third earns less than half the first, or t ≤ −1.5 | first won, last loses, and t ≤ −2 |
| Regimes (groups with at least 8 trades and 15%) | wins in all | loses in some | |

With any “fail” the verdict is **fails**; with any doubtful result, **doubtful**; otherwise **pass**. If it wins out of
sample and only fails checks that depend on trade count, the verdict is **not enough sample**, with how many times
more trades would be needed (projecting the deflated Sharpe with the same edge, up to 8×).

In this version, the Walk-forward tab shows efficiency, winning windows, overfitting, and paths (the
last two using the thresholds in this table). Full validation, including deflated Sharpe, decay, and
regimes, is in the library, covered by tests, for use from code (see [Development](development.md)).

## Portfolio

Combine saved backtests from multiple strategies or instruments: **Add** (choose the run), **Equal** weights,
**Risk parity** (each weight is inverse to the volatility of its daily result), or **Manual**, plus capital. It shows
overall metrics, the curve, the daily-result correlation matrix (ideally below
0.5), and how much drawdown decreases versus the sum of individual drawdowns. **Suggest combination** searches the latest
backtests across all your strategies for the combination with correlation below 0.5 that most improves Sharpe (up to 5).
**Report** builds the portfolio report.

## Reports

Markdown reports with all metrics, tables, and charts: build them with **Report** from History or Portfolio,
or with Claude in the lab (including its verdict and analysis). They are saved in `~/Documents/Trading Hub/Strategies/Reports`,
open as a sheet, and **Export PDF / HTML** opens them in the browser for printing or saving as PDF. See
[Claude](claude.md).
