[← Return to index](README.md)

# Data

The **Data** section (⌘3) shows the history saved on your Mac. That history is used when opening charts (instant load), in replay, and in backtests.

The screen has three parts: on the left, **Saved** (your instruments); in the middle, the **days** for the
selected instrument; on the right, **Download history**, **CME futures with Databento**, and **Import files**.
Above you see the total ("N instruments · size"), a button to update the list and **Open folder**.

![The Data section: what has been saved, the days of an instrument and the cards to download history and Databento](img/data.png)

## Where does the history come from

- **What you watch live is recorded automatically**, including data from Rithmic. Controlled by **Settings → Data → Record what
  arrives live** (enabled by default).
- **Downloads** for full days, from public files or your connections’ APIs.
- **Imported files** (CSV or TXT) for CFDs, forex, and other markets.
- **Databento**, for years of CME futures.

Everything remains as trades, in a file per connection, instrument and day (UTC).

## What was saved

In **Saved**, each instrument shows its connection, number of days, and disk usage. Select one to see its days in the middle,
newest first, with a bar proportional to size (roughly the number of trades) and each day’s disk usage. Above,
you see the full range (“dd/MM/yyyy → dd/MM/yyyy”) and the total size.

To choose multiple days, use ⌘ or ⇧ when clicking.

### Delete days

1. Select the instrument in **Saved**.
2. Select one or more days.
3. Click **Delete** and confirm. Those days are removed from that instrument and cannot be undone.

## Replay

Replay plays saved days as if they were live and lets you trade with the simulated account.

1. Select the instrument and a day. If you select several, replay runs from the first to the last.
2. Click **Play** (or double-click a day).
3. A new tab opens in Trading, marked with ▶, starting at the first trade of the first selected day and
   ending at the end of the last. It uses the active chart period and copies its indicators; earlier history
   loads from the same files.

The replay bar appears below:

| Control | Action |
|---|---|
| Play/pause | Start or stop playback. |
| 1× to 300× | Speed: 1×, 2×, 5×, 10×, 30×, 60× or 300× real time. |
| Clock and progress | Current replay time; “Finished” at the end. |
| +5 min, +30 min, +1 h | Jump forward. |

Drawings made during replay are not saved, and the replay is not kept in the workspace. Trades you make
do go into the simulated account history.

## Download history

Download complete days for an instrument.

1. Under **Instrument**, choose the instrument (the active chart is preselected if it can be downloaded).
2. Under **How much (UTC days)**, click a shortcut (Yesterday, 7 days, 30 days, 90 days, 6 months, 1 year, or 2 years) or change
   **From** and **To**. Today is not downloaded because it is not over yet.
3. Read the summary below the buttons: how many days and where they come from.
4. Click **Download**. You can see the current day, progress, and remaining time. **Cancel** stops the download.

| Source | What is downloaded |
|---|---|
| Binance Futures and Spot | Binance public daily archives, no account required. |
| Bybit | Bybit public daily archives, no account. |
| Rithmic | Tick by tick, using the contract traded each day (for example ESU6, then ESZ6). Rithmic only keeps data from the latest expiration: it does not provide history for expired contracts. |
| Tradovate | Tradovate ticks (API-enabled CME data required). |
| cTrader | Broker bid ticks through the Open API. |
| OANDA | 5-second candles converted into trades (CFDs have no real trades). |
| Alpaca | Trades from all US stock exchanges (SIP), free since 2016. |

- Days that are already complete are not downloaded again: if you cancel, the next run resumes where it stopped.
- Because of API limits (Rithmic, Tradovate, cTrader, OANDA, and Alpaca), Saturdays are not requested, and the downloaded day replaces data
  recorded live.
- An index such as ES has more than a million trades per day: long ranges take time.
- If a day has no data, the final summary counts it separately (holidays, weekends, or files not yet
  published). Rithmic also reports expired contracts that it cannot return.
- Connections that do not provide history still record data when you watch them live.

## Import files

For CFDs (US100, US500, DAX), forex, gold, and other markets without a connection. Download the CSV from Dukascopy or
HistData, or export it from MetaTrader or TradingView, and Trading Hub converts it into history for charts, replay, and
backtests. The format is detected automatically: bid/ask ticks, candles, or any CSV with date and price or OHLC.

1. Download the files. The **Dukascopy** and **HistData** buttons open their download pages.
2. Under **Instrument name**, type the symbol or leave it empty to infer it from the file name.
3. Under **File time**, choose the time zone used by the file:

| Option | For |
|---|---|
   | UTC | Dukascopy and TradingView |
   | New York fixed, −5 | HistData |
   | UTC+2 | MetaTrader in winter |
   | UTC+3 | MetaTrader in summer |

4. Click **Import files…** and choose one or more CSV or TXT files.
5. When finished, the summary reports how many trades and days were imported, the range, and the detected price step. The
   instrument appears in **Saved** under the “imported” connection and in backtests.

The name is inferred like this: USATECH, NAS100, NSX, US100, USTEC, or NDX → US100; USA500, SPX, or US500 → US500; USA30, US30,
DJI, or WS30 → US30; DEU, GER, or DAX → GER40; XAU or GOLD → XAUUSD; and pairs EURUSD, GBPUSD, USDJPY, AUDUSD, USDCAD,
USDCHF, NZDUSD, XAGUSD, and BTCUSD by name. If nothing is recognized, it uses the first word in the file.

CFDs have no real volume: each quote counts as one trade (tick volume), and the side is inferred from whether the
price moved up or down. Used for price, candles, and price backtests; footprint and delta are approximate. Candles
become synthetic trades.

## Databento

Databento sells tick-by-tick CME futures history since 2010, with the real aggressor side of each trade:
data Rithmic does not keep. It is pay-as-you-go (new accounts include free credits), so Trading Hub always
calculates the cost first and downloads only after you confirm.

First, add the **Databento** connection with your API key in [Connections](connections.md#databento). The
**Connections** button on the card takes you there.

1. Under **Continuous contract**, choose the market: NQ, ES, MNQ, MES, YM, RTY, CL, GC, MCL, MGC, ZN, or 6E. It uses the
   continuous volume contract, which rolls like the charts.
2. Choose **From** and **To** (initially, the last three months).
3. Choose whether to check **Also download days Rithmic already has**:
   - **unchecked** (cheaper): days Rithmic already has are skipped, and Databento fills only the
     missing ones;
   - **checked**: download the entire range to get the full Databento history.
4. Click **Calculate cost**. It reports how many days will be downloaded, the range, and the approximate cost in dollars from your
   credits.
5. Click **Download** and confirm in the “Download Databento history” window. If credits are insufficient, Databento
   charges the difference to your payment method on Databento.

**Download** is enabled only after calculating the cost. If you change the market, dates, or checkbox, you must
recalculate. Days you already have from Databento are not charged again, Saturdays are skipped, and
the range ends yesterday. Days without trades (holidays) are counted separately. **Cancel** stops the download; data already
downloaded is saved.

The same card shows what you have already downloaded (“You already have Databento:…”, with days and range for each market).

### Stored separately

Databento data is stored separately as “Databento” in the **Saved** list, without mixing with what you recorded or downloaded from
Rithmic. In **Backtesting**, under “Instrument (saved data)”, a future with history from both sources
appears three times (each option ends with its number of days):

| Option | What it uses |
|---|---|
| NQ · Rithmic + Databento | Both combined: Rithmic takes precedence, and Databento fills minutes where Rithmic has no trades (for example, hours when the app was closed and not recording). |
| NQ · Only Rithmic | Rithmic data only. |
| NQ · Databento | Databento data only. |

The same applies to Tradovate: “Tradovate + Databento”, “Only Tradovate”, and “Databento”. With the combined option, the
backtest summary reports how many days each source contributes.

## Where are the files

History lives in `~/Library/Application Support/Trading Hub/Ticks`, with one folder per connection (“rithmic”,
“databento”, “imported”…) and one per instrument, with a file per day. **Open folder** reveals it in Finder.
See also [Installation](installation.md#where-your-data-is-stored).
