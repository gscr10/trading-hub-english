[← Return to index](README.md)

# Connections

In **Connections** (⌘4) you configure where the market data comes from and what accounts you trade with. On the left is the
connection list; on the right, details for the selected connection.

In the list, each connection shows a summary ("public data, no account", "data + testnet account", the user and the
Rithmic system…) and a colored dot: gray if disabled, green or red depending on the last test result,
and neutral if you have not tested it yet.

## Add and test a connection

1. Click **New connection** at the bottom of the list and choose the type.
2. Fill in the requested information (it varies by type; see below).
3. Click **Test connection**. It saves first, then tests; the result appears above the form.
4. If you change anything, click **Save**.

Above the form are also:

- the **name** of the connection (you can change it) and its type;
- the **On / Off** switch: a disabled connection does not appear in the instrument browser or provide
  trading accounts;
- **Delete**, which removes the connection and its Keychain secrets. Recorded data is not touched. The Simulator,
  Binance, Bybit, and Rithmic connection types can only be removed when more than one of the same type exists; otherwise, disable them.

Almost all types also have **History when opening a chart** (from 30 minutes to 7 days): how much history is required
when opening an instrument. Data already recorded or downloaded in [Data](data.md) loads instantly; the rest
is requested from the connection.

## Where are your keys

Passwords, API key secrets, tokens, and the Databento API key are stored in the **macOS Keychain**,
never in files. In **Keychain Access** they appear as passwords named "Trading Hub". Once
saved, the field no longer shows the value: it displays “•••••••• (saved to Keychain)”. To replace it, enter the
new value and save; if you leave it empty, the existing value remains.

In `connections.json`, inside the data folder, only data that is not secret remains: the name, the user, the
server, the public identifier of the API key (for example, the Binance API key or the Alpaca Key ID) and the
options.

## What each type gives

| Connection | What it provides | What you need | Order routing |
|---|---|---|---|
| Simulator | Simulated market without internet | Nothing | Simulated account |
| Binance Futures / Binance Spot | Free Crypto Data and History | Nothing for data; API key and secret to trade | Testnet or real |
| Bybit | Perpetuals data in USDT | Nothing | No (data only) |
| Rithmic | CME futures: data, history, and orders | User, password, system, and server | Paper or real |
| AMP Futures | Same as Rithmic | The Rithmic credentials AMP sends you | Practice or real |
| Tradovate | CME futures: trading accounts and data | Username, password, CID, and API key secret | Demo or real |
| cTrader | CFDs and forex from brokers with cTrader | An Open API application, plus authorization for your account | No (data only) |
| OANDA | Indices, gold and forex CFDs | An API token | No (data only) |
| Alpaca | US Stocks and ETFs | API key ID and secret key | No (data only) |
| Databento | CME futures history since 2010 | API key (pay as you go) | No (history only) |
| Interactive Brokers | For now, only connection test | TWS or IB Gateway with API enabled | No |

Any connection with data is used to trade with the **simulated account**.

## Simulator

Simulated market with realistic book and trades, ideal for trying offline. Includes SIM-BTC, SIM-ETH, SIM-ES, and
SIM-NQ. No configuration needed.

## Binance Futures and Binance Spot

Binance market data is public: without an account you already have charts, DOM, tape and history. To trade, the
section **Trading account** asks for:

1. Choose **Testnet (test money)** or **Real account**. It comes on testnet.
2. Paste the **API key** and **API secret**. Create the key with trading permission only, without withdrawals. The “How
   do I create an API key?” link opens the testnet page or your real account page, depending on your choice.
3. For a real account, check **Allow REAL orders from Trading Hub**. Without it, the account remains "real
   (read only)” and cannot be traded.
4. Optional: **Ask for confirmation before each real order** (checked) and **Maximum quantity per order** (0 = no
   limit).

## Bybit

Public data of linear perpetuals in USDT. Trading with a Bybit account is not yet available.

## Rithmic

Direct connection through R|Protocol, the official Rithmic API: R|Trader Pro does not need to be open. It is created
disabled; enable it when you enter your data.

- **User** and **Password** (the eye button shows what you type). They are given to you by your broker or your prop firm.
- **Server**: when you open the section, Trading Hub queries Rithmic servers and lists each system/region
  combination with its latency in milliseconds; the fastest appear in green. The search box filters (paper, 01,
  Frankfurt, your prop firm name…) and the reload button requests the list again. Colo 75 only works from the
  CME data center.
- **Use a server not in the list**: enter the address (`wss://…`) and system manually.
- **Aggregated quotes**: Rithmic aggregates book updates that arrive at the same instant. Below, the app
  tells you whether the chosen system provides them; otherwise it connects without them.
- **Connection timeout (ms)**: 10,000 by default.
- **Advanced**: the **Application Name** (some systems require an approved one) and the **Products for the
  instrument search** (ES, NQ, MES, MNQ, CL…). For each product, the front-month contract is looked up, for example ES → ESZ6.

Your Rithmic accounts appear in the Chart Trader of Rithmic charts and in Account when the connection is
active and the order session connects. Rithmic allows only one session per user: if you sign in with the same
user from another program, one of the two sessions is disconnected.

## AMP Futures

AMP uses Rithmic: it sends you a Rithmic username and password. The connection is created disabled, with the
Chicago server and the “Rithmic Paper Trading” system (the demo). For a real account, choose “Rithmic 01” on the
nearest server. The rest is the same as Rithmic.

## Tradovate

Direct connection to the official Tradovate API.

1. Choose **Demo (simulated by Tradovate)** or **Real account**. It defaults to demo.
2. Enter your Tradovate **User** and **Password**.
3. Enter the **CID** and **Secret** for your API key. The “Where do I get the API key? (Settings → API Access)” link opens
   Tradovate.
4. Select the **Products for the instrument search** (the front-month contract is used for each one).
5. To trade a live account, check **Allow orders on Tradovate live accounts**. Optional: **Maximum quantity
   in order**.

Your accounts appear in Chart Trader and Account, and can be traded from CME futures charts on
Tradovate or Rithmic. Tradovate only provides market data through the API if your account has CME market data enabled
for API access (a separate license); without it, you can still trade while watching charts from another connection. Tradovate allows
only one data session per user: if you open Tradovate Trader with the same user, it can disconnect the Trading Hub session.

## cTrader

Free with any broker that has cTrader and a demo account: CFDs on indices, forex, gold and oil, with history
tick by tick.

1. **Your application (one time only)**: create an application on openapi.ctrader.com (you log in with your cTrader ID) and add it
   the redirect address shown on screen (`http://127.0.0.1:53682/ctrader`; the “Copy” button copies it).
   Then paste the **Client ID** and the **Secret** here.
2. **Authorize your account**: Choose **Demo account** or **Live account** and tap **Connect with cTrader**. It opens the
   browser; sign in with your cTrader ID and authorize. You have 5 minutes. You can also paste an access token (from
   your application’s “Playground”).
3. **Account number**: if the token has multiple accounts, the login of the one you want to use (0 = the first).

Trading Hub stores the token in the Keychain and renews it automatically. CFDs have no real trades: each quote
(bid) counts as a trade of volume 1, so footprint and delta are approximate.

## OANDA

Free with a practice account: Indices CFDs (NAS100_USD, SPX500_USD, US30_USD, DE30_EUR…), gold, oil and
forex, with years of history in 5-second candles.

- **Practice account (free)** or **Real account**.
- **Token**: you generate it in “Manage API Access” (there are links to create the account and the token).
- **Account**: the number 101-001-…; empty uses the first of the token.

Trading Hub uses the token for data only; it does not send orders to OANDA. As in cTrader, each quote (average price)
counts as a trade of volume 1.

## Alpaca

With a paper account (free, available in any country) you get tick-by-tick history for all US stock markets
since 2016, except the last 15 minutes. Use it to study indices through SPY, QQQ, IWM, and DIA.

- **API key ID** and **Secret key** of your practice account.
- **Live data**: IEX (free, one exchange) or SIP (all exchanges, paid plan).

Keys are used for data only; no orders are sent to Alpaca.

## Databento

Tick-by-tick history of CME futures (NQ, ES, MNQ, MES, CL, GC…) since 2010, with the real aggressor side of each
trade. Pricing is pay-as-you-go; new accounts include free credits.

- Only request the **API key** (`db-…`). There are links to create the account and see prices.
- **Test connection** shows the available date range and the cost of one day of NQ tick data. Nothing is downloaded.
- It provides no live data and does not appear in the instrument finder: use it from **Data → CME futures with
  Databento**. See [Data](data.md#databento).

## Interactive Brokers

Open TWS or IB Gateway and enable the API (Configure → API → Settings). Complete **Host**, **Port** (regular: 7497
TWS paper, 7496 real TWS, 4002 Gateway paper), and **Client ID**. For now Trading Hub only verifies the connection: it provides no
data or order routing.

## Real accounts and test accounts

All trading accounts appear in the Chart Trader **Account** selector. The **simulated account** is always
available. The rest depend on the connection:

| Connection | Paper account | Real account |
|---|---|---|
| Binance | Testnet: trades directly | With “Allow REAL orders from Trading Hub” |
| Rithmic and AMP | “Rithmic Paper Trading” and “Rithmic Test” systems: trade directly | Any other system (Rithmic 01, prop firms…), with “Allow orders in real accounts” |
| Tradovate | Demo: trades directly | With “Allow orders on real Tradovate accounts” |

Without that authorization, a live account does not appear in the Chart Trader selector. Rithmic and Tradovate live accounts remain
**read only** in the Account section: you can see balance, positions, and history, but cannot send them
orders. Real accounts are marked with “— REAL” in the selector and the notice “REAL ACCOUNT”.

### Enable real money

1. Try everything first with the simulated account or with the broker's test account.
2. On connection, go to the real account: on Binance, with the switch **Testnet / Real account**; on Tradovate, with
   **Demo / Real account**; in Rithmic and AMP, choosing the real system in **Server**.
3. Check the real orders box for that connection.
4. Set a **Maximum amount per order**: Trading Hub blocks larger orders.
5. Save and test the connection.

How real orders are confirmed and which limits apply is covered in [Trading](trading.md#security).
