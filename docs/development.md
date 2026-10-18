[← Return to index](README.md)

# Development

For anyone who wants to work on the code. Trading Hub is C# on .NET 10, with Avalonia 11.3 for the UI and SkiaSharp 2.88
for chart rendering. Interface texts and code comments are in English.

## The solution

`TradingHub.slnx` contains ten projects:

| Project | What does it do |
|---|---|
| `src/TradingHub.Core` | Market and data: instruments, trades, candles, tick files, CSV import, history sources (combined Rithmic and Databento), connections (Binance, Bybit, Rithmic, Tradovate, OANDA, Alpaca, cTrader, Databento, Interactive Brokers test, simulator, and replay), and trading (accounts, simulated account, stops and targets, history). No external packages. |
| `src/TradingHub.Sdk` | The indicator API, shaped like ATAS: `Indicator`, series, drawing, attributes, and texts. |
| `src/TradingHub.Charting` | The Skia chart engine: model, candles, footprint, DOM, trade tape, drawings, and themes. |
| `src/TradingHub.Scripting` | Compiles C# in memory with Roslyn (indicators and strategies) and adapts ATAS namespaces. |
| `src/TradingHub.Strategies` | Strategy API, backtest engine, optimization, smart walk-forward, robustness and validation, portfolios, and live execution with accounting. Depends only on Core. |
| `src/TradingHub.Indicators.Native` | Included indicators. |
| `src/TradingHub.Indicators.Technical` | Home for ported technical indicators; the public version includes only the editor types they use. |
| `src/TradingHub.App` | The app (`TradingHub` executable): `Views` (sections), `Windows`, `Controls`, `Services` (connections and Keychain, Claude Code, Live, risk agent, market condition, menu bar, sleep mode...), `Lab` (MCP server and reports), and `Styles`. With `--mcp` it runs headless as a laboratory server. |
| `tests/TradingHub.Tests` | Tests, using xUnit. |
| `tools/TradingHub.UiSnapshot` | Headless app with scenes that verify and capture the UI. |

Avalonia, SkiaSharp, framework, and product versions are in `Directory.Build.props`. The
visible name comes from `TradingHub.Common.Product.Name`, and folders come from `Product` and `AppPaths`.

## Build and run

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet build TradingHub.slnx          # everything
dotnet test tests/TradingHub.Tests    # tests
./build-app.sh                        # builds dist/Trading Hub.app (ad hoc signed)
./build-app.sh --install              # and copies it to /Applications
```

`build-app.sh` publishes Release for `osx-arm64`, self-contained, and builds the bundle outside the Desktop (iCloud can
add attributes that break the signature). An untracked `build.local.env` file can define `BUNDLE_ID` and
`SIGN_IDENTITY`; `TH_OUT` and `TH_STAGE` change the output and staging folders. More in
[Installation](installation.md).

To run the app from the code without touching your data or your Keychain:

```bash
TRADINGHUB_DATA_DIR=/tmp/th-dev TRADINGHUB_KEYCHAIN_FILE=/tmp/th-dev/llavero.json dotnet run --project src/TradingHub.App
```

With `TRADINGHUB_DATA_DIR`, strategies and indicators also go to subfolders of that folder, and the
menu bar icon and sleep mode do not touch the Mac's power settings. Never run a second copy against the
real data while the installed app is open: two processes writing the same files will corrupt them.

## Tests

`dotnet test tests/TradingHub.Tests` runs everything. Coverage includes the backtest engine and fills (`StrategyTests`), stops,
targets, and OCO (`BracketTests`), market context and filter (`ContextTests`), robustness and validation
(`RobustnessTests`), smart walk-forward (`SmartWalkForwardTests`), live execution (`LiveRunnerTests`,
`LivePortfolioTests`), portfolios and reports (`PortfolioTests`), data (`CoreTests`, `DataIntegrityTests`,
`HistorySourcesTests`, `LongHistoryTests`, `CsvImportTests`), accounts (`AccountTests`), drawings and legend
(`DrawingAndLegendTests`), and connections (`RithmicTests`, `BrokerConnectionTests`, `DataSourceTests`,
`BinanceSignatureTests`).

Tests use temporary folders and fake servers with real message formats from each service
(`FakeRithmicServer`, and `FakeHttpServer` with `FakeBrokers` for OANDA, Alpaca, Databento, cTrader, and Tradovate): they never
reach a real account. To run a group: `dotnet test tests/TradingHub.Tests --filter FullyQualifiedName~SmartWalkForward`.

## Headless scenes

```bash
dotnet run --project tools/TradingHub.UiSnapshot -- <folder> 4 <scene> [<scene>...] [--open connection:SYMBOL] [--size 1600x950]
```

`<folder>` is where PNG screenshots are saved, and `4` is the number of seconds to wait for the app to load before the scenes
(no scenes: capture Trading). Scenes with checks print `OK` or `FAIL` for each one, and the process exits
with the failure count as its exit code.

| Scene | What does it do |
|---|---|
| `trading`, `accounts`, `strategies`, `live`, `data`, `connections`, `settings` | Open that section and capture |
| `connections` | Tradovate, cTrader, OANDA, Alpaca, and Databento against fake servers: registration, test, download in Data, Databento cost, Tradovate accounts, and OANDA live prices |
| `panel` | The right Trading panel (Chart Trader, DOM, and trade tape), DOM buttons, and detached windows |
| `footprint` | Footprint ladder with its own colors, and the footprint editor |
| `market` | Backtest by market condition, and the run filtered by condition |
| `theme` | Dark and light mode, with chart settings |
| `choice`, `picker`, `corners` | History button on the bar, instrument selector, and a rounded-corner diagnostic |
| `data+rithmic`, `data+import` | Data with Rithmic (fake server), and importing a test CSV |
| `accounts+demo` | Accounts section with orders in the simulated account and in fake Rithmic |
| `connections+rithmic`, `connections+amp` | Rithmic or AMP profile with its server list |
| `trading+…` | `layout2`, `layout2r`, `layout3`, `layout4`, `layout6`, `history<hours>[+period]`, `historyall`, `jitter`, `float`, `brackets`, `drawings`, `drawsettings`, `links`, `toolbar`, `trader`, `accounts`, `crossdemo`, `strategy`, `domfloat`, `domsettings` |
| `strategies+…` | `backtest`, `optimize`, and `wfo` (run and capture), `code`, `history`, `reports`, `portfolio`, `claudefloat`, `claude`, `claudelab`; append `+File` to choose that strategy |

How they are isolated from your data:

- Sets `TRADINGHUB_SLEEP_GUARD=off` and a temporary `TRADINGHUB_KEYCHAIN_FILE` with fake secrets: it never reads Keychain.
- Copies your strategies and indicators to temporary folders (`TRADINGHUB_STRATEGIES_DIR`, `TRADINGHUB_INDICATORS_DIR`).
- Uses a temporary data folder (`TRADINGHUB_DATA_DIR`) with copies of `settings.json`, `workspace.json`,
  `connections.json`, and downloaded data (`Ticks`, `Klines`, `Drawings`, except Rithmic), never the originals.
  Connections lose their API keys, real orders are disabled, and Rithmic points at a fake server with user
  `demo`; account history starts empty.
- Two do not run at the same time: a lock on the temporary folder makes the next one wait (up to 20 minutes).
- If you already defined any of those variables, yours are respected.

Caution: `strategies+claude` and `strategies+claudelab` send a real request to your Claude Code (consuming your limit),
unless `TRADINGHUB_CLAUDE_CLI` points to another executable. For diagnostics: `TRADINGHUB_SNAP_INDICATORS="Name|Other"`
adds those indicators to the active chart, `TRADINGHUB_SCRIPTS_DEBUG=1` shows how custom indicators were compiled,
and `TRADINGHUB_SNAP_DEBUG=1` shows connection status.

## Environment variables

| Variable | What changes |
|---|---|
| `TRADINGHUB_DATA_DIR` | The data folder (instead of `~/Library/Application Support/Trading Hub`) |
| `TRADINGHUB_STRATEGIES_DIR`, `TRADINGHUB_INDICATORS_DIR` | The strategies and indicators folders |
| `TRADINGHUB_KEYCHAIN_FILE` | A JSON file with secrets, instead of Keychain |
| `TRADINGHUB_BACKTESTS_DIR`, `TRADINGHUB_LIVE_DIR` | Backtest history and live portfolio |
| `TRADINGHUB_SLEEP_GUARD=off` | Does not touch the Mac's power settings |
| `TRADINGHUB_CLAUDE_CLI` | Path to the Claude Code CLI |
| `TRADINGHUB_MCP_COMMAND` | The executable the laboratory launches (by default, the same app) |
| `TRADINGHUB_OANDA_REST`, `TRADINGHUB_OANDA_STREAM`, `TRADINGHUB_ALPACA_DATA`, `TRADINGHUB_ALPACA_TRADING`, `TRADINGHUB_ALPACA_STREAM`, `TRADINGHUB_TRADOVATE_REST`, `TRADINGHUB_TRADOVATE_WS`, `TRADINGHUB_TRADOVATE_MD`, `TRADINGHUB_CTRADER_URL`, `TRADINGHUB_DATABENTO_URL` | Point each connection at another server (used by scenes with fake servers) |

## Add a connection

1. In `src/TradingHub.Core/Feeds/<Name>/`, a class implementing `IMarketDataFeed`: `GetInstrumentsAsync`,
   `LoadTradesAsync` (batch history), and `Subscribe`, which delivers trades, depth, and state to an
   `IMarketDataSink`. Depending on the service, add `IKlineHistory` (candles for long history), `IDayDownloader`
   (full-day downloads in Data), `ISessionAwareFeed` (markets that close), or `ITickerStatsProvider` (price and
   24-hour change in the selector).
2. If it can be traded, an account implementing `ITradingAccount` (market, limit, stop, cancel, modify, close,
   reverse, position, and `IsReal`).
3. In `src/TradingHub.App/Services/Connections.cs`: a new value in `ConnectionKind`, its `MarketDataId`, non-secret
   data in `ConnectionProfile`, the feed in `ConnectionManager.GetFeed`, and the test in `TestAsync`. Secrets go through
   `SetSecret` and `GetSecret` (Keychain). Add a `TRADINGHUB_…` variable to change the server address in tests.
4. The form in `src/TradingHub.App/Views/ConnectionsView.cs` and, if it trades, its accounts in `AppServices.AccountsFor`
   and `AccountFor`: real accounts only with “Allow real orders”.
5. A fake server in `tests/TradingHub.Tests` (start from `FakeHttpServer`) with its tests.

## Add an indicator or lab tool

- **Included indicator**: a class in `src/TradingHub.Indicators.Native` inheriting from `Indicator`, with a parameterless
  constructor, `[DisplayName]`, and `[Category(IndicatorCategories.…)]`. The app scans that assembly at startup. For your
  own, no code changes are needed: put it in the indicators folder (see [Indicators](indicators.md)).
- **Laboratory tool**: its schema in `LabServer.Tools()`, its case in `LabServer.CallAsync`
  (`src/TradingHub.App/Lab/LabServer.cs`), and its name in `ClaudeCodeBridge.LabTools`
  (`src/TradingHub.App/Services/ClaudeCode.cs`), which is the allowlist of tools.

## Validation from code

`ValidationGate.Evaluate` (in `TradingHub.Strategies`) applies the [Backtesting](backtesting.md) rules to a
walk-forward. Examples are in `tests/TradingHub.Tests/RobustnessTests.cs`.

```csharp
var gate = ValidationGate.Evaluate(new GateInput
{
    WalkForward = walkForward,   // output of Optimizer.WalkForward
    Optimization = optimization, // optional: for plateau and robustness
    Trials = 12,                 // configurations tested, for deflated Sharpe
    TrialVariance = 0.01,        // variance of their per-trade Sharpe
    MinTrades = 30,
});
Console.WriteLine($"{gate.StatusText} · deflated Sharpe {gate.Deflated:P0}");
foreach (var c in gate.Checks) Console.WriteLine($"{c.Status} · {c.Name}: {c.Text}");
```

`LiveStrategyRunner.Guard` also accepts an `ILiveOrderGuard`, a limiter that reviews every order that opens or increases a
position before sending it (closing orders always pass); the app ships without one, but tests show how to use it.

## Rules

- **Never write passwords, tokens, or keys into repository files.** The app stores them in Keychain
  (`Keychain` in `Connections.cs`); test tools use `TRADINGHUB_KEYCHAIN_FILE` with fake data.
- **Never run tools against real data** (the data folder and `~/Documents/Trading Hub`): use the
  variables above. Two processes writing the same files corrupt them.
- **No confirmations that block trading buttons.** Safety comes from risk limits (size, daily loss,
  drawdown, Stop all) and from enabling real money per connection, not from obstacles between the click
  and the order. Trading buttons must not move either: the `trading+jitter` scene measures whether the bar changes
  height or whether the buy button moves when the day's change updates.
- Always test with the simulated account, and with fake servers when changing a connection.
- Interface texts and code comments are in English.
