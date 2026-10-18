[← Return to index](README.md)

# Live

The **Live** section (⌘7) gathers every strategy running in real time, whether launched from here, from
Backtesting, or from a chart. It has the tabs **Strategies**, **Market**, **Portfolios**, **Trades**, and
**Proposals**, plus the buttons **New live strategy** and **Stop all** at the top. Always test on the simulated
account first.

## Accounts: simulated, test, and real

- The **simulated account** is always available. Its orders fill against the book tail and the real market tape.
- Binance testnet, Tradovate demo, and Rithmic practice accounts also do not trade real money.
- **Real** accounts appear only if you enabled “Allow real orders” on that connection and are shown in red with
  “— REAL” (see [Connections](connections.md#real-accounts-and-test-accounts)).
- When you manually start or resume a strategy on a real account, Trading Hub asks for confirmation once. After that it sends
  orders without confirming each one: safety comes from size and risk limits. Automatic resume
  on open does not ask; that is why it is off for real accounts.

## Launch a strategy

### From Backtesting or Live

1. In **Backtesting → Backtest**, “Take live” card, click **Go live…** (carries the strategy and its
   parameters), or under **Live** click **New live strategy**.
2. **What and where**: the strategy, instrument, account, and candles (M1, M2, M3, M5, M10, M15, M30, H1, H4, or the
   period from your backtest).
3. **Parameters**: taken from the last configuration in your backtest.
4. **Market condition**: which market to open trades in (range, mixed, uptrend, downtrend) and with
   what volatility (low, normal, high compared with the last month). This is the strategy's context filter.
5. **Risk**: size (×), daily maximum loss, and maximum drawdown (in money; 0 = no limit), plus whether hitting a
   limit also closes the position.
6. **Portfolio**: one of yours or none (goes to automatic).
7. **Go live**.

The time zone and session start come from your backtest: they define the “day” for the daily maximum loss. Before trading, the
strategy builds its candles and indicators from the history already loaded for that instrument (the connection's history
hours, or whatever a chart loaded), without sending orders, and its market context is filled with the last month by
hour. If it needs more candles than are loaded, its indicators take time to become ready. The same strategy cannot
run twice on the same instrument and account. For futures, a notice shows how many contracts and which contract each order
sends.

### From a chart

On the chart bar, the robot button lists your strategies (you can also use **Put on active chart**
from Backtesting). The strategy starts **visual only**: it marks where it would have entered and exited over the chart
history (a backtest without commissions or slippage) and also runs live on its own simulated account, “Visual”, which does not
touch yours. In its configuration (from the same menu or the chart legend) you choose **Run live with an
account**, the account, and **Apply**. From then on it appears in Live with the origin "Chart" and is managed like the others. If
you change the chart instrument, executing strategies pause; if you remove it from the chart, it stops and its position remains
as is. More in [Charts](charts.md#strategies-on-the-chart).

## Size and smart contract

- **Size** multiplies the strategy's orders (1 = as programmed; 0 = opens nothing new). The strategy
  still sees its position in its own units: each order sends the difference between what the
  current size calls for and what it holds, so an exit closes exactly what it opened even if you changed size in between.
- It rounds down to the instrument's step, but if the strategy wants to be in the market it sends at least one
  unit.
- **Smart contract** (in Settings, enabled): for futures with micros (NQ → MNQ, ES → MES, YM → MYM, RTY → M2K,
  GC → MGC, CL → MCL, among others), if the size requests a fraction of the mini, orders go to the micro with the same
  exposure: ×0.5 of 1 NQ is 5 MNQ. Whole minis use the mini. With an open position it stays in its contract and
  switches when flat (it restarts itself with the same accounting).
- The table, detail panel, menu bar, and chart legend always show how many contracts and which contract each order
  sends, for example “5 MNQ · micro”.

## Risk limits and pause

| Limit | When reached |
|---|---|
| Daily maximum loss | When closed trades for the session day plus open P&L reaches −limit, it cancels pending entries and opens nothing else until the next session (“Stopped for today”). |
| Maximum drawdown | When the drop from the peak of its curve (including open P&L) reaches the limit, it stops. |

With “When a limit is reached, also close the position” (on by default), it also closes whatever is open. Each
hit limit leaves a notice above the table and in the status bar. **Pause** is different: it opens no new
positions, cancels pending entries, and keeps managing any open position until it closes.

## Strategies: table and details

At the top: **Result**, **Today**, how many are **Live**, **Closed trades** (click to see all), and **Pending
changes**. The table shows strategy, status (including “paused by context” when the filter will not let it open),
size and contract, position, today, net, trades, profit factor, current drawdown, and limits. Clicking one opens its
details:

- **Actions**: Pause or Resume, Close position, Stop, Stop and close, Restart (or Resume on the
  chart), View on chart, View trades, Start from scratch (clears history, only when flat), and Delete.
- **Risk**: size and limits, with **Apply risk** (the new size applies from the next order).
- **Market condition**: applied immediately, without restarting, and shows whether the current market would let it open.
- **Parameters**: **Apply and restart** (reloads history; if it has a position, it keeps managing it).
- **Portfolio** and **Activity** (what it is doing and what it writes with `Log`).

Each strategy keeps its own accounting, even when it shares the account with others or with your manual trades. If it
restarts with a position in its accounting and the account has it, it keeps managing it and re-adds the
stop and target (without duplicating them if they were already at the broker); if the account does not have it, it closes it in its accounting
at the latest price. With an external broker, it waits up to 45 seconds for the position report before deciding.

**Stop all** is the emergency button: after confirmation, it stops every live strategy (in this
section and on charts), cancels their orders, and closes their positions at market.

## Trades

Every closed live trade: strategy, market, side, entry and exit with price, duration, quantity,
points, result, MAE, MFE, and “Capture” (how much of the best moment it captured). Filter by strategy, sort
(recent, old, best or worst result, worst MAE), and click one to see it on the chart. Keep in mind that each
strategy's accounting records each fill at the latest market price at that moment (not the price reported by the
broker) and does not deduct commissions.

## Portfolios

- The **automatic** portfolio ("Automatic · all live") gathers strategies that run without a portfolio of yours. With **New
  portfolio** you create others with a name, reference capital (25,000 by default), and strategies; each strategy belongs to
  only one of your portfolios.
- Each shows result (closed and open), trades, profit factor, maximum drawdown, Sharpe (with more than one
  week of data), average correlation between its strategies, the combined curve, one table per strategy, and the
  daily-result correlation matrix (red: they win and lose on the same days; green: they offset each other).
- Buttons: **Add new strategy**, **Start all** (confirms if there are real accounts), **Stop all** (positions
  remain as they are), **Edit**, and **Delete** (its strategies remain and move to automatic).

## The risk agent

Each portfolio can have a risk agent: Claude reviews the result, recent streak, drawdown against its
history, daily-result volatility, and the correlation of each strategy, then allocates risk by proposing the
size of each one and, if needed, its daily maximum loss and maximum drawdown.

| Option | Default |
|---|---|
| Mode | **Propose**: you approve each change. **Automatic**: applies on its own, and only if no strategy in the portfolio trades a real account |
| Daily risk (% of capital) and maximum portfolio drawdown (%) | 2% and 10% |
| Minimum and maximum size per strategy | ×0.25 and ×3 (0 so it opens nothing new) |
| Review every (min) | 60 (minimum 10), if there were at least 3 new trades, and every 8 hours even with none |
| Model and effort | Opus 5.5 with Extra effort; also Sonnet 5.5, Haiku 5.5, or Fable 5.1, from Low to Maximum |
| Instructions | Free text: “prioritize NQ”, “never more than ×2 in crypto”… |

**Review now** runs it immediately. It performs at most 8 automatic reviews per day, each lasting 12 minutes with
a US$2 usage cap (if your Claude Code supports that option); it has no tools, only the numbers Trading Hub passes
to it. What it proposes stays within your limits and appears below the portfolio and in **Proposals**. The agent
card shows its last review, the summary, and accumulated estimated cost. How to connect your Claude account:
[Claude](claude.md).

## Proposals

Suggested size changes wait for your approval, one by one or with **Approve all**; those from real accounts require
confirmation and apply from the next order. They come from three sources:

- **Suggest with rules**: with 10 or more trades and looking at the profit factor of the last 20, it proposes pausing (×0) if
  it is below 0.7 with drawdown near its worst and at least 15 recent trades, halving if below
  0.9 or the drawdown is near its worst, and raising 25% (up to ×3) if it is 1.5 or more with positive expectancy and low
  drawdown.
- **Ask Claude for analysis**: opens the laboratory in Backtesting and asks it to review the live portfolio and propose.
- Each portfolio's **risk agent**.

## Market

For each market of the running strategies: its **condition** (range, mixed, bullish trend, or bearish trend, with the
last 24 hours' efficiency and how long it has lasted), its **volatility** (percentile of the last month), a strip with the
last 48 hours, which strategies trade there, and whether their filter lets them open. At the end, a table shows which
conditions each strategy trades, with the current one marked. It recalculates every 30 seconds from hourly closes of the
last month, using the same measure as the context filters.

## Closing and reopening the window

- **Menu bar**: with the option enabled (on by default), closing the window leaves Trading Hub running in the
  Mac menu bar with strategies working. Its menu shows live strategies and open positions,
  and the panel has more detail. To quit completely: ⌘Q or “Quit Trading Hub”. See
  [Getting started](getting-started.md#the-icon-in-the-menu-bar).
- **Automatic resume**: a few seconds after opening, strategies that were running when you closed the app (or when the Mac shut down)
  start again on their own, with their parameters, size, and limits; paused strategies come back paused. Strategies on
  real accounts wait for you to resume them unless you enable that option too. A notice summarizes what resumed.
- **No-sleep mode**: while live strategies or the risk agent are working, the Mac does not sleep from
  inactivity, and if you enable it, it keeps running with the lid closed. See [Settings](settings.md).
