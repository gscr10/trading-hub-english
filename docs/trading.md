[← Return to index](README.md)

# Trading

You trade from the **Trading** section with the Chart Trader, the DOM, the chart, or keyboard shortcuts. Each
chart trades through the account you choose; the default is the **simulated account**.

## Choose account

Each chart remembers its account. Choose it in the Chart Trader under **Account**. The simulated account is always
available; the others depend on the chart instrument and your [connections](connections.md):

| Chart | Accounts that appear |
|---|---|
| Any | Simulated account |
| Binance Futures or Spot | Your Binance account for that connection: “(testnet)” or “— REAL” |
| Rithmic Futures | Your Rithmic accounts: “(practice)” or “— REAL” |
| Tradovate CME futures | Your Tradovate accounts: “(demo)” or “— REAL” |

Rithmic accounts appear when your order session connects; until then, the Chart Trader explains
why they are not available yet. With a live account chosen, the Chart Trader displays “REAL ACCOUNT” in orange.

## The Chart Trader

![The Chart Trader on the chart and, to the right, the panel with the DOM, its order buttons, and the trade tape](img/chart-trader.png)

It's in two places that show the same thing:

- **In each chart**, on the right. Collapse it with the small arrow or ⌥T; each chart trades only its own.
- **In the right panel**, above the DOM. It follows the active chart (“Trade on the active chart:…”). If you detach it
  into a separate window, it returns to the panel when you close that window.

From top to bottom:

| Part | Action |
|---|---|
| Account | Where the orders on this chart go. |
| Cross trading | Route orders to another instrument (see [Cross trading](#cross-trading)). |
| Quantity | With quick buttons: 1, 2, 5 and 10 if the instrument is traded in whole units; fractions in crypto. |
| Buy MKT / Sell MKT | To market. |
| Buy Ask / Sell Ask | Limit order at the ask (the price appears below each button). Buying at the ask fills immediately at a capped price; selling at the ask waits on the offer. |
| Buy Bid / Sell Bid | Limit order at the bid. Buying at the bid waits on the bid; selling at the bid fills immediately at a capped price. |
| Canc. buys / Canc. sells | Cancel orders on that side. |
| Cancel all / Revert | Cancel all orders / reverse position. |
| Breakeven | Move the stop to the entry price. |
| Close 25% / 50% / 75% | Close that portion at market; the stop and targets adjust automatically. |
| Close position | Close everything to the market and cancel orders. |
| Result | Open PnL and position (“LONG 2 @ price”, “Flat · result of the day”). |
| Automatic SL/TP | Stops and targets (see [Stops and targets](#stops-and-targets)). |

The result and position appear below the buttons, so updates do not shift the buttons.

## Order types

| Type | How to send | In the simulated account |
|---|---|---|
| Market | MKT, Close, Revert, partials | Fill at the best bid or ask from the book (or at the latest price if there is no book). |
| Limit | Ask/Bid, click on DOM, right-click on chart | Fill if price trades through your price, or if more volume trades at your price than was ahead of you in the queue. |
| Stop | Click on the DOM beyond the price (see chart below), right-click on the chart | Triggers when price trades and fills at market. |

Stop loss and targets can only reduce the position, never turn it around.

## Trade with the DOM

| Action | Result |
|---|---|
| Click the Bid (or Buy Orders) column | Limit buy at that price; if at or above the ask, a buy stop. |
| Click the Ask (or Sell Orders) column | Limit sell at that price; if at or below the bid, a sell stop. |
| Right-click or ⌥-click | Cancel your orders at that price. |
| Drag one of your orders | Move it to another price. |
| Wheel / center button | Move the ladder / recenter on price. |

The quantity is taken from the active chart. If you enable **Double-click to trade** in Configure DOM, a single click no longer
sends orders (cancel still works with one click). The question mark icon in the DOM summarizes these gestures.

Below the DOM are the **order buttons** (shown or hidden from the header): quick quantity buttons,
Buy MKT, Sell MKT, C. Bid, C. Ask, V. Bid, V. Ask, Cancel, Revert, and Close. They trade the active chart.

## Trade from the chart

- **Right-click** a price: “Buy limit” and “Sell stop” if below the last price, “Sell limit” and
  “Buy stop” if above, using the Chart Trader quantity. Also “Cancel all orders”.
- **Drag** an order line to move it in price.
- **Right-click one of your orders**: cancel it; on a target, “Target amount” (25, 50, 75, or 100% of the
  position); on the stop, “Move stop to breakeven”; on a pending entry order, “Attach stop and targets from
  template”.
- **With an open position**, right-click offers “Close half to market”, “Close all to market”, “Move stop to
  breakeven”, and “Set stops and template targets”.

## Stops and targets

The **Auto SL / TP** section of the Chart Trader saves one template per instrument (the same one in the
chart and the one on the right):

1. Activate the **Automatic SL / TP** switch.
2. Set the **SL** in ticks from the entry.
3. Set the **TP** in ticks. With **+ target** you can add up to 3 partial targets, each with its percentage of the
   position (100% is distributed automatically; the × removes each one).
4. **Breakeven at TP1** (checked) moves the stop to the entry when the first target is filled.

With the template active, opening a position sets the stop and targets automatically in OCO: when one closes the
position, the rest is canceled. On a limit or stop entry order, they are attached and sent when it fills.
If you increase the position, they cover the entire open position; if you close part of it, they adjust to the remainder.

**Apply to position** replaces the stop and targets of the open position with those of the template.

You can also place them manually on the chart: drag the SL/TP buttons on the position or a pending order, or
drag from the position line (toward profit to create a target; toward loss, a stop).

## Cross trading

You watch one instrument and send orders to another, for example watch ES and execute in MES. In the Chart Trader, click
**Cross trading**, search for the instrument (same-underlying instruments appear first), and choose it; “None” returns to
trading the chart instrument. Orders, stops, and targets go to the chosen instrument with prices unchanged. If the
two prices differ, the Chart Trader warns “careful: different prices”.

## Shortcuts for trading

In **Settings → Keyboard shortcuts**, **Trade** group, you can assign: buy and sell to market, buy limit on the
bid, sell limit on the ask, close the position, reverse it, cancel all orders, close 25% or 50%,
stop to breakeven, set stop and template targets, and move to the next or previous quick amount button.
They are unassigned by default. They work on the active chart and do not fire while you type in a text field (unless
the shortcut itself uses ⌘, ⌥, ⌃, or a function key).

## The Account section

**Account** (⌘5) displays one account at a time. The button above opens the list, grouped by connection: the Account
simulated and your Rithmic and Tradovate accounts.

Cards: **Balance**, **Daily result**, **Open PnL**, **Realized today**, **Total result** (simulated account) or
**Available to trade** (broker account), and **Active orders**. For broker accounts, the balance is reported by
Rithmic or Tradovate.

| Tab | Content |
|---|---|
| Positions | Contract, side, quantity, average price, open PnL, made today and “Close” button. |
| Orders | Today’s orders, active or completed, with status and a “Cancel” button. |
| Executions | Each execution with time, contract, side, quantity, price and commission. |
| Trades | Each closed trade: entry, exit, duration and result. |
| Statistics | Today, 7 days, 30 days or all: balance curve, net result, winning %, profit factor, maximum drawdown, expectation, streaks, longs and shorts, and the result of each day. |

Position and order rows keep their place: new rows are added below, so a click does not land on the neighboring row.
History is saved on your Mac and persists when you close the app.

In the simulated account you can change the **Starting balance** and use **Reset simulated account**: it returns to the starting
balance and begins a new history (the previous one is saved separately). To reset, you must have no open positions
or orders.

## Security

- **Nothing real by default.** Each connection has a real-money checkbox, off initially. Without it, the real
  account does not appear for trading (see [Connections](connections.md#real-accounts-and-test-accounts)).
- **No confirmations on test accounts.** On the simulated account and on the demo, practice or
  testnet, each click sends the order instantly.
- **Confirmation on real accounts.** By default, each order to a real account opens the “Confirm REAL order”
  dialog with the details and account. The same happens when moving an order, closing, reversing, canceling from the buttons, or
  using partials or breakeven. To disable it, both options must be off: the connection option (“Ask for
  confirmation before each real order” on Binance, “Confirm each real order” on Rithmic) and **Settings → Trading →
  Confirm real orders**. The Tradovate connection has no such checkbox, so its real orders always ask for
  confirmation. In Account, “Close” and “Cancel” on a real account also ask for confirmation.
- **Maximum quantity per order.** On live accounts, an order larger than the connection maximum is blocked and
  the status bar says “Order blocked”.
- **Strategy limits.** Automated strategies have a daily maximum loss and maximum drawdown
  (0 = no limit), and **Live** has **Stop all**, which stops everything, cancels orders, and closes
  positions.
- **Clear warnings.** Real accounts are marked in orange with “— REAL” or “REAL ACCOUNT” in the Chart Trader and
  Account.
