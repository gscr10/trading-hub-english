[← Return to index](README.md)

# Claude in Trading Hub

Trading Hub uses **Claude Code**, the Claude CLI, with your own session and your Claude subscription. There are no API
keys or separate accounts, and its work counts against your Claude Code usage limit. Without Claude Code the platform
works the same; you simply lose the assistant, the laboratory, and the risk agent.

| Where | What it does |
|---|---|
| **Backtesting → Code**, Claude's panel | Write, explain, and fix strategies and indicators |
| The same panel with **Laboratory** | Also runs backtests, optimizations, walk-forward, and Monte Carlo, then leaves a report with a verdict |
| **Live → Portfolios** | The risk agent for each portfolio (see [Live](live.md#the-risk-agent)) |
| **Live → Proposals** | “Ask Claude for analysis”: the laboratory reviews the live portfolio and proposes sizes |

## Finding Claude Code

Trading Hub looks for the `claude` executable in `/usr/local/bin`, `/opt/homebrew/bin`, `~/.claude/local`, `~/.local/bin`,
`~/.npm-global/bin`, every folder in your `PATH`, and the copy bundled with the Claude desktop app
(`~/Library/Application Support/Claude/claude-code/…`). If it finds several, it uses the newest version. You can point it at
another path with `TRADINGHUB_CLAUDE_CLI`. Installation instructions are in
[Installation](installation.md#claude-code-optional).

- The Claude panel shows "Claude Code *version* · your subscription" at the top, or a warning if it was not found.
- If Claude Code asks you to log in, **Sign in to Claude Code** appears: it opens Terminal with `claude /login`
  so you can sign in with your account. Then repeat the request.
- If your version is too old for the model, the risk agent warns you: update it (the Claude app updates itself,
  or run `claude update`).

## Model, effort, and permissions

- The assistant and the laboratory use **Opus 5.5** (`claude-opus-5-5`) with **xhigh** effort ("Extra"). If your Claude
  Code does not recognize the effort option, it is ignored. The risk agent starts the same way and is configured per portfolio (Opus
  5.5, Sonnet 5.5, Haiku 5.5, or Fable 5.1; Low, Medium, High, Extra, or Maximum effort).
- It runs headless (`claude -p`) inside the strategies folder, with permission to edit files there, in the
  indicators folder, and in the laboratory captures folder. It has only file tools (`Read`, `Write`,
  `Edit`, `MultiEdit`, `Glob`, `Grep`): no terminal or web access.
- It does not load your other MCP servers (`--strict-mcp-config`): in laboratory mode, only Trading Hub. It also does not inherit
  `CLAUDE*` or `ANTHROPIC*` variables from another session: it uses your normal login.
- The risk agent runs with tighter restrictions: no tools, no configuration or hooks when your version supports
  that, and no session saving. It receives only the portfolio numbers and returns a proposal.

## The strategy assistant

1. Open **Backtesting → Code**. Claude's panel is to the right of the editor; the window button detaches it
   into a separate window (and docks it again).
2. Enter the request and click **Send** (⌘Enter). There are starter suggestions: create a breakout of the first
   30 minutes, explain the open strategy, add a time filter, add an ATR stop...
3. While it works you see what it writes and touches ("Reading", "Writing", or "Editing", with the file); the editor
   stays read-only, and changes to the open file appear live. **Stop** cancels it.
4. When it finishes: if it created a new strategy, that file opens automatically; Trading Hub compiles it and offers **Test in backtest** or,
   on errors, **Ask it to fix them**. If it touched indicators, it compiles them and reports whether they are already in "My indicators"
   or what is wrong.

With each request, Trading Hub sends the open file, the indicators folder, and the current compilation errors.
Its instructions include the complete strategy and indicator API (from [Strategies](strategies.md)
and [Indicators](indicators.md)) plus these rules: C# code that compiles without external packages, no files, network,
threads, or seedless `Random`; short comments in English; and a 2–5 line explanation at the end.

Conversations are saved in `~/Library/Application Support/Trading Hub/Chats`. The history button resumes
one where it left off (Claude continues with everything it already knew); **New conversation** starts from scratch.

## The laboratory

With **Lab** enabled, Claude Code receives Trading Hub tools as an MCP server: the same
app executable launched with `--mcp`, headless, speaking JSON-RPC over stdin and stdout. The
configuration lives in `~/Library/Application Support/Trading Hub/claude-lab-mcp.json` (server `tradinghub`). It only
enables these tools, and each can take up to an hour (a long optimization):

| Tool | What it does |
|---|---|
| `list_strategies` | Lists strategies with compilation status, parameters (including `[Optimize]` ranges), and warm-up: `OnInit` indicators and candles that take time to become ready |
| `compile_strategy` | Compiles a strategy and returns errors (line and message), parameters, and warm-up |
| `chart_snapshot` | Runs the strategy over a range and saves a chart image with entries, exits, and drawings, plus candles, trades, and recording in local time |
| `list_indicators` · `compile_indicator` | Lists your indicators and compiles one, with errors |
| `list_data` | Tick data saved by instrument (the only data backtests use) |
| `download_data` | Downloads full days from Binance Futures, Binance Spot, and Bybit public archives (up to 62 days per request) |
| `run_backtest` | Tick-by-tick backtest with commissions and slippage; optionally with Monte Carlo |
| `run_optimization` | Grid optimization (up to 5,000 combinations by default); returns the best ones, the % of winning combinations, and the plateau-recommended one |
| `run_walk_forward` | Smart walk-forward (grid or Bayesian, plateau, PBO, and CPCV paths); can also just estimate how long it will take |
| `list_runs` | Saved runs for a strategy |
| `analyze_portfolio` · `suggest_portfolio` | Combine runs into a portfolio with their correlation, or find the least correlated combination that most improves Sharpe |
| `live_portfolio` | The live portfolio, read-only: each strategy with its size, position, result, profit factor, and drawdown |
| `propose_sizing` | Leave size proposals pending; nothing is applied |
| `save_report` | Create and save the professional report |

Everything it runs stays in the strategy history with the note “Claude's Laboratory”, just as if you had run it
yourself. Its instructions specify a method: review the strategy and data (and download 30–90 days for crypto),
base backtest, optimization with reasonable ranges and at least 30 trades per combination, walk-forward with the
optimization window 3–4 times the test window, Monte Carlo on what has been validated, improve the code if it has clear
flaws (always comparing against the previous version), give a verdict, and finish with `save_report`. It cannot invent
numbers: everything it writes must come from a tool. The verdicts are:

| Verdict | When |
|---|---|
| **winner** | Out-of-sample win, walk-forward efficiency ≥ 0.5, out-of-sample profit factor ≥ 1.2, at least 30 trades, reasonable Monte Carlo p95 drawdown, and stable windows |
| **promising** | Positive results but with a small sample or some instability |
| **loser** | Loses out of sample or only wins when overfit |
| **discarded** | Does not compile, does not trade, or the idea does not make sense with that data |

For the live portfolio, use `live_portfolio` and `propose_sizing`: more size (up to ×3) for those that perform, less or pause
for those degrading, always with a reason and without drawing conclusions from fewer than 10 trades. Proposals
wait for your approval in **Live → Proposals**.

## Reports

`save_report` asks Trading Hub to assemble the report with all metrics, tables, and charts from the runs
(executive summary, strategy, data and assumptions, performance, monthly, trade analysis, drawdowns,
optimization with each parameter's sensitivity, walk-forward, Monte Carlo, portfolio, and glossary). Claude provides the
verdict, summary, analysis, risks, and next steps.

- They are saved in `~/Documents/Trading Hub/Strategies/Reports`: one `.md` per report and a `…_files` folder with the
  charts.
- They appear in **Backtesting → Reports** as a sheet. **Export PDF / HTML** builds an HTML file with the charts embedded and
  opens it in the browser, where you can print it or save it as PDF; **Show in Finder** opens the folder.
- The verdict also appears next to each strategy in the strategy menu.
- Without Claude, **Report** in History or Portfolio creates the same report without the written analysis.

## What happens to Claude data

What you write, the files it reads (your strategies and indicators), and what the tools return (metrics,
trades, data summaries, chart images, and, on request, live portfolio status) travel to
Anthropic through your Claude Code, under your account. Connection keys are in Keychain, and no
tool exposes them. More in [Privacy](privacy.md).
