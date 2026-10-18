[← Return to index](README.md)

# Privacy and security

Trading Hub runs entirely on your Mac: the app, your data, your strategies, backtests, and connection keys.
It has no servers of its own, asks you to create no account, and sends no telemetry or usage statistics. It connects only to
what you configure, and only when you use it.

## What stays on your Mac

| Location | Contents |
|---|---|
| `~/Library/Application Support/Trading Hub` | Settings, connections (no secrets), workspace, tick history, candles, drawings, templates, account history, backtests, live strategies, and their portfolios |
| `~/Documents/Trading Hub` | Your strategies (with reports in `Strategies/Reports`) and your indicators |
| macOS Keychain | Passwords, secrets, tokens, and the Databento API key, under the name “Trading Hub” |

Details for each file are in [Installation](installation.md#where-your-data-is-stored). Claude-related data also lives in the
data folder: conversations (`Chats`), chart images the laboratory reviewed (`Laboratory captures`, the latest 60), and the
laboratory configuration (`claude-lab-mcp.json`). Unexpected errors are logged to `crash.log`, which never leaves your Mac.

## Your passwords

- Passwords, API key secrets, tokens, and the Databento API key go to **macOS Keychain**, never to files.
  Trading Hub reads and writes them with the macOS `security` tool.
- `connections.json` stores only non-secret data: name, user, server, the public part of the API key
  (the Binance API key or the Alpaca Key ID), and the options. See [Connections](connections.md#where-are-your-keys).
- Deleting a connection also deletes its secrets from Keychain.
- Keys are not in files Claude can read, and no laboratory tool reads them.

## What connects to the internet and when

| What | When | With whom |
|---|---|---|
| Market data | When you open a chart or instrument from an enabled connection | The exchange or broker for that connection (Binance, Bybit, Rithmic, Tradovate, OANDA, Alpaca, cTrader) |
| History | When you download days in Data, or Claude calls `download_data` | Binance and Bybit public files, or your connection server |
| Exchange candle history | To load long history on a chart and for market context on days without saved ticks | The open market exchange; for crypto, Binance or Bybit public candles without opening a session |
| Orders | When you or a strategy trades in a broker account | That broker. The simulated account sends nothing out |
| Databento | When testing the connection (checks the available range and the cost of one NQ day), clicking **Calculate cost**, and **Download**, which first shows the cost and asks for confirmation | Databento, with your API key |
| Claude Code | When you use the wizard, the laboratory, or the risk agent | Anthropic, with your account, through the Claude Code CLI |
| Interactive Brokers | Only when you test the connection | Your TWS or IB Gateway (by default, on your own Mac) |
| Links | Only when you click them (signup pages for each service, cTrader authorization, author credit) | They open in your browser |

Nothing else: no update checks or error reports. Building the app requires internet access and NuGet packages,
but that happens during installation, not during use.

## Claude

Claude's work goes through your Claude Code and your account: what you write to it, the files it reads (your strategies and
indicators), and what its tools return (metrics, trades, data summaries, chart images, and, if you ask, your live
account status) travel to Anthropic. Trading Hub starts it inside the strategies folder, with permission to edit only there,
in the indicators folder, and in the captures folder, using file tools (no terminal or web) and without your other MCP servers.
The risk agent has no tools at all: it receives the portfolio numbers and returns a proposal. What Claude Code does with
that data depends on your account and Anthropic's policies. See [Claude](claude.md).

## Real money

- Trading Hub starts with the **simulated account**. Real orders are disabled on each connection until you enable
  “Allow real orders” (see [Connections](connections.md#real-accounts-and-test-accounts)).
- Manual orders to real accounts ask for confirmation if you left that setting on (see [Trading](trading.md#security)), and
  manually starting or resuming a strategy on a live account is confirmed once. Auto-resume on open does not ask:
  that is why it is off for real accounts.
- Live strategies have size, daily maximum loss, and maximum drawdown limits, and **Stop all** stops every one
  (see [Live](live.md)). The risk agent never applies changes unless the portfolio contains a real account.

## Permissions macOS may request

- **Administrator password**: only if you enable "Keep the lid closed" in Settings, so the Mac does not sleep
  with the lid closed while Trading Hub is open. It is reverted automatically when you close the app.
- **Control Terminal**: if you click “Sign in to Claude Code”, Trading Hub opens Terminal with `claude /login`.
- The app is signed "ad hoc" when compiled on your Mac (see [Installation](installation.md#ad-hoc-signature)).

## Developer tools do not touch your data

Tests and headless scenes use temporary folders (`TRADINGHUB_DATA_DIR`, `TRADINGHUB_STRATEGIES_DIR`,
`TRADINGHUB_INDICATORS_DIR`) and a fake key file (`TRADINGHUB_KEYCHAIN_FILE`) instead of Keychain. The
scenes copy your configuration and history to a temporary folder, remove API keys, disable real orders, and
point Rithmic at a fake server. See [Development](development.md).

## Delete everything

1. Quit Trading Hub completely (⌘Q or “Quit Trading Hub” in the menu bar).
2. Delete `~/Library/Application Support/Trading Hub` and, if you want, `~/Documents/Trading Hub`.
3. In **Keychain Access**, search for “Trading Hub” and delete those passwords.
4. Delete the app from Applications (see [Installation](installation.md#uninstall)).
