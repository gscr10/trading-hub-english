using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using TradingHub.App.Services;
using TradingHub.App.Windows;
using TradingHub.Strategies;

namespace TradingHub.App.Views;

/// <summary>
/// En vivo: todo lo que corre con dinero (o en simulada) en tiempo real, en un solo lugar. Las estrategias lanzadas desde
/// esta sección o desde un gráfico, sus portafolios con estadísticas y correlación,
/// el agente de riesgo de cada portafolio, todas las operaciones cerradas con su detalle y las propuestas de tamaño.
/// </summary>
internal sealed partial class LiveView : UserControl, IPage
{
    private static readonly CultureInfo Es = new("en-US");
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private readonly AppServices _svc;
    private readonly Action<string> _status;
    private readonly MainWindow _window;
    private readonly Ui.Segmented<string> _tabs;
    private readonly Dictionary<string, Control> _pages = new();
    private readonly Panel _host = new();
    private string _tab = "strategies";
    private int _tick;

    // estrategias
    private readonly WrapPanel _kpis = new() { ItemSpacing = 10, LineSpacing = 10 };
    private readonly StackPanel _rows = new() { Spacing = 0 };
    private readonly Border _notice = new() { IsVisible = false };
    private readonly TextBlock _noticeText = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
    private (DateTime Time, string Text)? _shownNotice, _dismissedNotice;
    private string _rowsKey = "";

    // operaciones y propuestas
    private LiveTradesPanel? _trades;
    private readonly StackPanel _proposals = new() { Spacing = 10 };
    private DateTime _proposalsStamp;

    public LiveView(AppServices svc, Action<string> status, MainWindow window)
    {
        _svc = svc;
        _status = status;
        _window = window;
        _tabs = new Ui.Segmented<string>(new[]
        {
            ("strategies", "Strategies", (string?)"All live strategies: those in this section and those on the charts"),
            ("market", "Market", (string?)"Range or trend (uptrend or downtrend) and volatility of each market, and what strategies operate with that"),
            ("portfolios", "Portfolios", (string?)"Strategies that run together: statistics, correlation and risk agent"),
            ("trades", "Trades", (string?)"All trades closed live with MAE, MFE and all the details"),
            ("proposals", "Proposals", (string?)"Suggested size changes (rules, Claude, or Risk Agent): apply only if you approve"),
        });
        _tabs.SelectionChanged += ShowTab;
        _pages["strategies"] = BuildStrategiesPage();
        _pages["market"] = BuildMarketPage();
        _pages["portfolios"] = BuildPortfoliosPage();
        _pages["trades"] = new Border();
        _pages["proposals"] = BuildProposalsPage();
        foreach (var p in _pages.Values) _host.Children.Add(p);
        Content = Build();
        ShowTab("strategies");
        _tabs.Select("strategies");
    }

    public void OnShown() => Refresh(force: true);

    public void OnTimer()
    {
        if (!IsVisible) return;
        if (++_tick % 8 != 0) return; // ~2 s
        Refresh(force: false);
    }

    private Control Build()
    {
        var title = Ui.H(10, Ui.Icon(MaterialIconKind.AccessPoint, 22, Ui.Accent), Ui.H2("Live"));
        var launch = Ui.Button("New live strategy", MaterialIconKind.Play, () => OpenLaunch(), "primary");
        ToolTip.SetTip(launch, "Launch a real-time strategy on an account (simulated, testnet or real)");
        var kill = Ui.Button("Stop everything", MaterialIconKind.OctagonOutline, () => _ = KillAllAsync(), "danger");
        ToolTip.SetTip(kill, "Kill switch: stops all live strategies, cancels your orders and closes your positions");
        var header = new DockPanel { Margin = new Thickness(24, 12, 24, 12) };
        var right = Ui.H(12, _tabs, kill, launch);
        DockPanel.SetDock(right, Dock.Right);
        header.Children.Add(right);
        header.Children.Add(title);
        var top = new Border { Child = header, Background = Ui.Bg1, BorderBrush = Ui.Line, BorderThickness = new Thickness(0, 0, 0, 1) };
        var root = new DockPanel();
        DockPanel.SetDock(top, Dock.Top);
        root.Children.Add(top);
        root.Children.Add(_host);
        return root;
    }

    private void ShowTab(string id)
    {
        _tab = id;
        if (id == "trades" && _trades == null)
        {
            _trades = new LiveTradesPanel(_svc, _status) { Margin = new Thickness(24, 18) };
            _host.Children.Remove(_pages["trades"]);
            _pages["trades"] = _trades;
            _host.Children.Add(_trades);
        }

        foreach (var (k, c) in _pages) c.IsVisible = k == id;
        Refresh(force: true);
    }

    internal void Select(string tab)
    {
        _tabs.Select(tab);
        ShowTab(tab);
    }

    private void Refresh(bool force)
    {
        try
        {
            switch (_tab)
            {
                case "strategies":
                    RefreshStrategies(force);
                    break;
                case "market":
                    RefreshMarket(force);
                    break;
                case "portfolios":
                    RefreshPortfolios(force);
                    break;
                case "trades":
                    if (force) _trades?.Refresh();
                    break;
                case "proposals":
                    if (force || LiveStore.ProposalsStamp != _proposalsStamp) RefreshProposals();
                    break;
            }
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex);
        }
    }

    private async Task KillAllAsync()
    {
        if (!await ConfirmDialog.ShowAsync(_window, "Stop everything",
                "ALL live strategies (those in this section and those on the charts) are stopped, their orders are canceled and their market positions are closed.",
                "Stop everything", danger: true))
            return;
        int n = await _svc.Live.KillAllAsync("You activated it from Live");
        _status($"You stopped everything: {n} strategy(s) stopped and their positions closed");
        Refresh(force: true);
    }

    internal void OpenLaunch(string? strategyId = null, IReadOnlyDictionary<string, object?>? parameters = null, string? portfolioId = null)
    {
        var w = new LiveLaunchWindow(_svc, _status, strategyId, parameters, portfolioId);
        w.Closed += (_, _) => Refresh(force: true);
        w.Show(_window);
    }

    private void OpenManage(LiveEntry e)
    {
        var w = new LiveStrategyWindow(_svc, _status, e);
        w.Closed += (_, _) => Refresh(force: true);
        w.Show(_window);
    }

    private void OpenTrades(string? key = null, string? title = null, IEnumerable<string>? scope = null) => new LiveTradesWindow(_svc, _status, key, title, scope).Show(_window);

    private static string Money(decimal v) => v.ToString("+#,##0.00;-#,##0.00;0.00", Es);

    private static Control Kpi(string label, string value, string sub, IBrush? color = null, Action? click = null, string? tip = null)
    {
        var card = Ui.Card(Ui.V(3,
            new TextBlock { Text = label, Classes = { "faint", "small" } },
            new TextBlock { Text = value, FontSize = 20, FontWeight = FontWeight.Bold, Foreground = color ?? Ui.Text0 },
            new TextBlock { Text = sub, Classes = { "faint", "small" } }), 12).Also(b => b.MinWidth = 170);
        if (tip != null) ToolTip.SetTip(card, tip);
        if (click == null) return card;
        card.Cursor = new Cursor(StandardCursorType.Hand);
        card.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(card).Properties.IsLeftButtonPressed) click();
        };
        return card;
    }

    // ===================================================================== estrategias

    private Control BuildStrategiesPage()
    {
        var close = Ui.IconButton(MaterialIconKind.Close, "Hide", () =>
        {
            _notice.IsVisible = false;
            _dismissedNotice = _shownNotice; // no vuelve hasta que llegue un aviso nuevo
        }, 14);
        _notice.Child = Ui.Card(new DockPanel
        {
            Children =
            {
                close.Also(b => DockPanel.SetDock(b, Dock.Right)),
                Ui.H(10, Ui.Icon(MaterialIconKind.AlertOutline, 18, Ui.Warn), _noticeText),
            },
        }, 12);

        var intro = Ui.Faint("Everything that runs in real time, whether launched from here or from a chart. Tap one to manage it: risk, parameters, pause, stop or delete.");
        intro.TextWrapping = TextWrapping.Wrap;
        var root = Ui.V(16, intro, _notice, _kpis, Ui.Card(_rows, 10));
        root.Margin = new Thickness(24, 18, 24, 28);
        return new ScrollViewer { Content = root };
    }

    private const string RowColumns = "2.6*,1.1*,0.9*,1*,1*,1*,0.6*,0.7*,1*,1.3*,28";

    private void RefreshStrategies(bool force)
    {
        var entries = _svc.Live.Entries();
        var running = entries.Where(e => e.Running).ToList();

        // avisos recientes (límites tocados, agente de riesgo)
        var last = _svc.Live.Notices.LastOrDefault();
        if (last.Text != null && (DateTime.Now - last.Time).TotalMinutes < 30)
        {
            // se muestra cuando llega uno nuevo; si lo cerraste, queda cerrado
            if (_shownNotice != last) _notice.IsVisible = _dismissedNotice != last;
            _shownNotice = last;
            _noticeText.Text = $"{last.Time:HH:mm} · {last.Text}";
        }
        else _notice.IsVisible = false;

        _kpis.Children.Clear();
        decimal net = entries.Sum(e => e.Net), open = running.Sum(e => e.OpenPnL);
        decimal today = running.Sum(e => e.Runner!.TodayPnL);
        int trades = entries.Sum(e => e.Ledger.Trades.Count);
        int paused = running.Count(e => e.Paused || e.Runner!.DailyLimitActive);
        int pending = LiveStore.LoadProposals().Sum(p => p.Items.Count(i => i.Status == "pending"));
        _kpis.Children.Add(Kpi("Result", Money(net), $"open {Money(open)}", net >= 0 ? Ui.Up : Ui.Down));
        _kpis.Children.Add(Kpi("Today", Money(today), "closed + open of those that run", today >= 0 ? Ui.Up : Ui.Down));
        _kpis.Children.Add(Kpi("Live", running.Count.ToString(Inv), paused > 0 ? $"{paused} pause(s) or braking(s)" : $"{entries.Count} in total", running.Count > 0 ? Ui.Accent : null));
        _kpis.Children.Add(Kpi("Closed trades", trades.ToString(Inv), "Tap to see all of them in detail", null, () => OpenTrades(), "All trades with MAE, MFE, size and more; tap one to see it on the charts"));
        _kpis.Children.Add(Kpi("Pending changes", pending.ToString(Inv), "they wait for your approval", pending > 0 ? Ui.Warn : null, pending > 0 ? () => Select("proposals") : null));

        // la tabla solo se rehace si cambió algo visible
        string key = string.Join("|", entries.Select(e => $"{e.Key}:{e.StateLabel}:{OutOfContext(e)}:{e.Ledger.Fills.Count}:{e.Ledger.Position}:{(e.Runner?.SizeMultiplier ?? e.Info.Size)}:{_svc.Live.PlanOf(e).Text}:{Math.Round(e.OpenPnL, 0)}:{(e.Runner?.MaxDailyLoss ?? e.Info.MaxDailyLoss)}:{(e.Runner?.MaxDrawdown ?? e.Info.MaxDrawdown)}"));
        if (!force && key == _rowsKey) return;
        _rowsKey = key;
        _rows.Children.Clear();
        var head = new Grid { ColumnDefinitions = new ColumnDefinitions(RowColumns), Margin = new Thickness(10, 4, 10, 8) };
        string[] titles = { "Strategy", "Status", "Size", "Position", "Today", "Net", "Ops", "PF", "Current DD", "Limits (day / DD)", "" };
        for (int i = 0; i < titles.Length; i++)
        {
            var t = new TextBlock { Text = titles[i], Classes = { "h3" }, HorizontalAlignment = i >= 2 && i <= 8 ? HorizontalAlignment.Right : HorizontalAlignment.Left, Margin = new Thickness(6, 0) };
            Grid.SetColumn(t, i);
            head.Children.Add(t);
        }

        _rows.Children.Add(head);
        if (entries.Count == 0)
        {
            var empty = Ui.V(10,
                new TextBlock { Text = "There are no live strategies yet.", Classes = { "muted" } },
                Ui.Faint("Launch one with “New Live Strategy” or run it from a chart (chart strategies menu)."));
            empty.Margin = new Thickness(10, 8, 10, 12);
            _rows.Children.Add(empty);
            return;
        }

        foreach (var e in entries.OrderByDescending(e => e.Running).ThenBy(e => e.Locked).ThenByDescending(e => e.Net))
            _rows.Children.Add(BuildRow(e));
    }

    private Control BuildRow(LiveEntry e)
    {
        var info = e.Info;
        var r = e.Runner;
        var stats = LiveStore.StatsOf(info, e.Ledger, 25_000m);
        var g = new Grid { ColumnDefinitions = new ColumnDefinitions(RowColumns), Margin = new Thickness(10, 0) };
        IBrush dotColor = e.Running ? (e.Paused || r!.DailyLimitActive ? Ui.Warn : Ui.Up) : e.WaitingResume ? Ui.Warn : Ui.Text2;
        var dot = new Avalonia.Controls.Shapes.Ellipse { Width = 8, Height = 8, Fill = dotColor, VerticalAlignment = VerticalAlignment.Center };
        var badge = new Border
        {
            Child = Ui.H(4, Ui.Icon(e.Locked ? MaterialIconKind.ShieldLockOutline : e.Source == "chart" ? MaterialIconKind.ChartBoxOutline : MaterialIconKind.AccessPoint, 11, Ui.Text2),
                new TextBlock { Text = e.SourceLabel, FontSize = 10.5, Foreground = Ui.Text2 }),
            Padding = new Thickness(6, 1),
            CornerRadius = new CornerRadius(8),
            Background = Ui.Bg2,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var name = Ui.V(2,
            Ui.H(7, dot, new TextBlock { Text = info.Strategy, FontWeight = FontWeight.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis }, badge),
            new TextBlock { Text = $"{info.Symbol} {info.Period} · {info.Account}{(info.IsReal ? " — REAL" : "")}", Classes = { "faint", "small" }, TextTrimming = TextTrimming.CharacterEllipsis }.Also(t =>
            {
                if (info.IsReal) t.Foreground = Ui.Warn;
            }));
        decimal size = r?.SizeMultiplier ?? info.Size;
        decimal pos = e.Ledger.Position;
        decimal dl = r?.MaxDailyLoss ?? info.MaxDailyLoss, dd = r?.MaxDrawdown ?? info.MaxDrawdown;
        var m = stats.Metrics;
        var plan = _svc.Live.PlanOf(e);
        string posSymbol = e.Ledger.OpenSymbol ?? plan.Symbol;
        Control[] cells =
        {
            name,
            StateCell(e, dotColor),
            SizeCell(e, size, plan),
            Num(pos == 0 ? "—" : $"{(pos > 0 ? "L" : "S")} {Math.Abs(pos).ToString("0.####", Es)} {posSymbol}", pos > 0 ? Ui.Up : pos < 0 ? Ui.Down : null),
            Num(r != null && e.Running ? Money(r.TodayPnL) : "—", r != null && e.Running ? (r.TodayPnL >= 0 ? Ui.Up : Ui.Down) : null),
            Num(Money(e.Net), e.Net >= 0 ? Ui.Up : Ui.Down),
            Num(m.TotalTrades.ToString(Inv)),
            Num(m.TotalTrades > 0 ? (m.ProfitFactor >= 999 ? "∞" : m.ProfitFactor.ToString("0.00", Es)) : "—"),
            Num((r?.CurrentDrawdown ?? stats.CurrentDrawdown).ToString("#,##0.00", Es)),
            new TextBlock { Text = $"{(dl > 0 ? dl.ToString("#,##0", Es) : "—")} / {(dd > 0 ? dd.ToString("#,##0", Es) : "—")}", FontSize = 12, Foreground = Ui.Text1 },
            Ui.Icon(MaterialIconKind.ChevronRight, 16, Ui.Text2),
        };
        for (int c = 0; c < cells.Length; c++)
        {
            cells[c].Margin = new Thickness(6, 9);
            cells[c].VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(cells[c], c);
            g.Children.Add(cells[c]);
        }

        var row = new Border { Child = g, BorderBrush = Ui.Line, BorderThickness = new Thickness(0, 1, 0, 0), Background = Brushes.Transparent, Cursor = new Cursor(StandardCursorType.Hand), CornerRadius = new CornerRadius(6) };
        row.PointerEntered += (_, _) => row.Background = Ui.Bg2;
        row.PointerExited += (_, _) => row.Background = Brushes.Transparent;
        row.PointerPressed += (_, ev) =>
        {
            if (ev.GetCurrentPoint(row).Properties.IsLeftButtonPressed) OpenManage(e);
        };
        ToolTip.SetTip(row, "Manage: risk, parameters, pause, stop or delete");
        return row;
    }

    /// <summary>Corriendo pero con su filtro de contexto cerrado: no abre nada nuevo hasta que el mercado sea el suyo.</summary>
    private static bool OutOfContext(LiveEntry e) =>
        e is { Running: true, Paused: false } && !e.Runner!.DailyLimitActive && HasFilter(e.Runner.StrategyInstance) && !e.Runner.StrategyInstance.ContextAllowsEntries;

    private Control StateCell(LiveEntry e, IBrush dotColor)
    {
        var state = new TextBlock { Text = e.StateLabel, Foreground = dotColor, FontSize = 12, TextWrapping = TextWrapping.Wrap };
        if (!OutOfContext(e)) return state;
        var s = e.Runner!.StrategyInstance;
        var note = new TextBlock { Text = "paused for context", Foreground = Ui.Warn, FontSize = 11, TextWrapping = TextWrapping.Wrap };
        var cell = Ui.V(1, state, note);
        ToolTip.SetTip(cell, $"Operates at: {FilterText(s)}. The market is now in a {MarketContextTracker.Label(s.Regime, s.MarketDirection).ToLowerInvariant()}" +
                             (s.VolatilityRank is { } vr ? $" with volatility in the {vr:0} percentile" : "") + ". It opens nothing new until the market matches its conditions (see the details in Market).");
        cell.Cursor = new Cursor(StandardCursorType.Hand);
        cell.PointerPressed += (_, ev) =>
        {
            ev.Handled = true;
            Select("market");
        };
        return cell;
    }

    private static TextBlock Num(string text, IBrush? color = null) =>
        new() { Text = text, HorizontalAlignment = HorizontalAlignment.Right, Foreground = color ?? Ui.Text0, FontFamily = new FontFamily("Menlo, SF Mono"), FontSize = 12.5 };

    /// <summary>El tamaño y, abajo, cuántos contratos y de cuál manda cada orden (en el color de acento si es el micro).</summary>
    private static Control SizeCell(LiveEntry e, decimal size, OrderPlan plan)
    {
        var top = Num((e.Locked ? "🔒 " : "") + (size == 0 ? "at zero" : "×" + size.ToString("0.##", Es)), size == 0 ? Ui.Warn : null);
        var contract = new TextBlock
        {
            Text = plan.Units <= 0 ? "does not open" : plan.Micro ? $"{plan.Text} · micro" : plan.Text,
            HorizontalAlignment = HorizontalAlignment.Right,
            FontSize = 11,
            FontWeight = plan.Micro ? FontWeight.SemiBold : FontWeight.Normal,
            Foreground = plan.Micro ? Ui.Accent : Ui.Text2,
        };
        var cell = Ui.V(1, top, contract);
        ToolTip.SetTip(cell, plan.Units <= 0 ? "Size at zero: does not open new trades"
            : $"Each order sends {plan.Text}{(plan.Detail.Length > 0 ? $" ({plan.Detail})" : "")}");
        return cell;
    }

    // ===================================================================== propuestas

    private Control BuildProposalsPage()
    {
        var rules = Ui.Button("Suggest with rules", MaterialIconKind.ScaleBalance, SuggestByRules);
        ToolTip.SetTip(rules, "It proposes increasing the size of those that perform (high recent PF, low drawdown) and lowering or pausing those that do not. Nothing is applied without your approval.");
        var claude = Ui.Button("Ask Claude for analysis", MaterialIconKind.Creation, AskClaude, "primary");
        ToolTip.SetTip(claude, "Claude analyzes the portfolio live in the Backtesting laboratory and leaves proposals that you approve one by one");
        var head = new DockPanel();
        var buttons = Ui.H(8, rules, claude);
        DockPanel.SetDock(buttons, Dock.Right);
        head.Children.Add(buttons);
        head.Children.Add(Ui.V(4, Ui.H2("Size proposals"),
            Ui.Faint("From the rules, from Claude or from the risk agent of each portfolio. Nothing changes until you approve it (except a portfolio with the agent on automatic, which only operates simulated accounts).").Also(t => t.TextWrapping = TextWrapping.Wrap)));
        var root = Ui.V(16, head, _proposals);
        root.Margin = new Thickness(24, 18, 24, 28);
        return new ScrollViewer { Content = root };
    }

    private void SuggestByRules()
    {
        var entries = _svc.Live.Entries().Where(e => !e.Locked).ToList();
        var stats = entries.Select(e => LiveStore.StatsOf(e.Info, e.Ledger, 10_000)).ToList();
        foreach (var s in stats) s.Info.Running = entries.First(e => e.Key == s.Info.Key).Running;
        var p = LiveSizing.SuggestByRules(stats);
        if (p.Items.Count == 0)
        {
            _status(p.Summary);
            return;
        }

        LiveStore.AddProposal(p);
        RefreshProposals();
        _status("There are suggested changes: review them and approve the ones you want");
    }

    private void AskClaude()
    {
        _window.Navigate("strategies");
        if (_window.Page("strategies") is StrategiesView sv) sv.AskClaudeForSizing();
    }

    private void RefreshProposals()
    {
        _proposalsStamp = LiveStore.ProposalsStamp;
        _proposals.Children.Clear();
        var list = LiveStore.LoadProposals().OrderByDescending(p => p.CreatedUtc).ToList();
        if (list.Count == 0)
        {
            _proposals.Children.Add(new TextBlock { Text = "There are no proposals. Use “Suggest with rules”, “Ask Claude for analysis” or activate the risk agent of a portfolio.", Classes = { "muted", "small" }, TextWrapping = TextWrapping.Wrap });
            return;
        }

        foreach (var p in list.Take(20)) _proposals.Children.Add(ProposalCard(p, RefreshProposals));
    }

    /// <summary>Tarjeta de una propuesta con aprobar / rechazar cada cambio.</summary>
    internal Control ProposalCard(SizingProposal p, Action refresh)
    {
        var proposal = p;
        string who = p.Source switch
        {
            "claude" => "Claude's proposal",
            "agente" => "Risk Agent " + p.PortfolioName,
            "agente (automático)" => "Risk Agent " + p.PortfolioName + " (applied only)",
            "agent" => $"Risk Agent {p.PortfolioName}",
            "agent (automatic)" => $"Risk Agent {p.PortfolioName} (applied only)",
            _ => "Rules proposal",
        };
        var title = Ui.H(8,
            Ui.Icon(p.Source.StartsWith("agent", StringComparison.Ordinal) || p.Source.StartsWith("agente", StringComparison.Ordinal) ? MaterialIconKind.ShieldHalfFull : p.Source == "claude" ? MaterialIconKind.Creation : MaterialIconKind.ScaleBalance, 16, Ui.Accent),
            new TextBlock { Text = who, FontWeight = FontWeight.SemiBold },
            new TextBlock { Text = p.CreatedUtc.ToLocalTime().ToString("MM/dd HH:mm", Es), Classes = { "faint", "small" }, VerticalAlignment = VerticalAlignment.Center });
        var box = Ui.V(10, title);
        if (!string.IsNullOrWhiteSpace(p.Summary)) box.Children.Add(new TextBlock { Text = p.Summary, Classes = { "muted", "small" }, TextWrapping = TextWrapping.Wrap });
        foreach (var item in p.Items)
        {
            var it = item;
            string change = $"{it.Strategy}: ×{it.CurrentSize.ToString("0.##", Es)} → {(it.ProposedSize == 0 ? "at zero" : "×" + it.ProposedSize.ToString("0.##", Es))}";
            if (it.ProposedMaxDailyLoss is { } dl) change += $"· loss of day {(it.CurrentMaxDailyLoss > 0 ? it.CurrentMaxDailyLoss.ToString("#,##0", Es) : "no limit")} → {(dl > 0 ? dl.ToString("#,##0", Es) : "no limit")}";
            if (it.ProposedMaxDrawdown is { } dd) change += $"· drawdown {(it.CurrentMaxDrawdown > 0 ? it.CurrentMaxDrawdown.ToString("#,##0", Es) : "no limit")} → {(dd > 0 ? dd.ToString("#,##0", Es) : "no limit")}";
            var changeText = new TextBlock
            {
                Text = change,
                FontWeight = FontWeight.SemiBold,
                TextWrapping = TextWrapping.Wrap,
                Foreground = it.ProposedSize > it.CurrentSize ? Ui.Up : it.ProposedSize < it.CurrentSize ? Ui.Down : Ui.Text0,
            };
            var reason = new TextBlock { Text = it.Reason, Classes = { "small" }, TextWrapping = TextWrapping.Wrap, Foreground = Ui.Text1 };
            Control actions;
            if (it.Status == "pending")
            {
                var approve = Ui.Button("Approve", MaterialIconKind.Check, () => _ = DecideAsync(proposal, it, true, refresh), "primary");
                var reject = Ui.Button("Reject", MaterialIconKind.Close, () => _ = DecideAsync(proposal, it, false, refresh));
                actions = Ui.H(6, reject, approve);
            }
            else actions = new TextBlock { Text = it.Status == "approved" ? "Approved and applied" : "Rejected", Foreground = it.Status == "approved" ? Ui.Up : Ui.Text2, Classes = { "small" }, VerticalAlignment = VerticalAlignment.Center };

            var line = new DockPanel();
            DockPanel.SetDock(actions, Dock.Right);
            line.Children.Add(actions);
            line.Children.Add(Ui.V(3, changeText, reason).Also(v => v.Margin = new Thickness(0, 0, 10, 0)));
            box.Children.Add(new Border { Child = line, Padding = new Thickness(12, 8), Background = Ui.Bg2, CornerRadius = new CornerRadius(8) });
        }

        if (p.Items.Count(i => i.Status == "pending") > 1)
        {
            var all = Ui.Button("Approve all", MaterialIconKind.CheckAll, () => _ = ApproveAllAsync(proposal, refresh));
            all.HorizontalAlignment = HorizontalAlignment.Right;
            box.Children.Add(all);
        }

        return Ui.Card(box, 14);
    }

    private async Task ApproveAllAsync(SizingProposal p, Action refresh)
    {
        var pending = p.Items.Where(i => i.Status == "pending").ToList();
        var entries = _svc.Live.Entries();
        if (pending.Any(i => entries.FirstOrDefault(e => e.Key == i.Key)?.Info.IsReal == true) &&
            !await ConfirmDialog.ShowAsync(_window, "Approve changes to REAL accounts", $"There are {pending.Count} changes and some are from a real account. They apply to future orders.", "Approve all", danger: true))
            return;
        foreach (var it in pending) await DecideAsync(p, it, true, null, confirmed: true);
        refresh();
    }

    /// <summary>Aprobar aplica el cambio (con confirmación si es una cuenta real); rechazar solo lo registra.</summary>
    private async Task DecideAsync(SizingProposal p, SizingItem item, bool approve, Action? refresh, bool confirmed = false)
    {
        if (approve)
        {
            var e = _svc.Live.Entries().FirstOrDefault(x => x.Key == item.Key);
            if (e == null)
            {
                _status($"{item.Strategy} is no longer on Live: it is registered as rejected");
                approve = false;
            }
            else
            {
                if (!confirmed && e.Info.IsReal &&
                    !await ConfirmDialog.ShowAsync(_window, "Approve change in REAL account", $"{item.Strategy}: ×{item.CurrentSize:0.##} → ×{item.ProposedSize:0.##}.\n{item.Reason}", "Approve and apply", danger: true))
                    return;
                _svc.Live.ApplyRisk(e, item.ProposedSize, item.ProposedMaxDailyLoss ?? (e.Runner?.MaxDailyLoss ?? e.Info.MaxDailyLoss),
                    item.ProposedMaxDrawdown ?? (e.Runner?.MaxDrawdown ?? e.Info.MaxDrawdown), e.Runner?.CloseOnLimit ?? e.Info.CloseOnLimit);
            }
        }

        item.Status = approve ? "approved" : "rejected";
        var list = LiveStore.LoadProposals();
        var stored = list.FirstOrDefault(x => x.Id == p.Id);
        var storedItem = stored?.Items.FirstOrDefault(i => i.Key == item.Key && i.ProposedSize == item.ProposedSize && i.Status == "pending");
        if (storedItem != null) storedItem.Status = item.Status;
        LiveStore.SaveProposals(list);
        _status(approve ? $"Applied: {item.Strategy} now with size ×{item.ProposedSize:0.##}" : $"Rejected: {item.Strategy}");
        refresh?.Invoke();
    }
}
