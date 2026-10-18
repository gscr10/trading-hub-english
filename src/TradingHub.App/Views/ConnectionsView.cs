using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Material.Icons;
using TradingHub.App.Controls;
using TradingHub.App.Services;
using TradingHub.App.Windows;
using TradingHub.Feeds.Rithmic;

namespace TradingHub.App.Views;

/// <summary>
/// Conexiones: perfiles de datos y cuentas (Rithmic, Binance, Bybit, Interactive Brokers y simulador).
/// Las contraseñas y API secrets se guardan en el Llavero de macOS, nunca en archivos.
/// </summary>
internal sealed class ConnectionsView : UserControl, IPage
{
    private readonly AppServices _svc;
    private readonly Action<string> _status;
    private readonly ListBox _list = new();
    private readonly ContentControl _detail = new();
    private readonly Dictionary<string, (bool Ok, string Message, DateTime When)> _results = new();
    private ConnectionProfile? _selected;
    private CancellationTokenSource? _testCts;

    public ConnectionsView(AppServices svc, Action<string> status)
    {
        _svc = svc;
        _status = status;
        Content = Build();
        FillList();
        _svc.Connections.Changed += FillList;
    }

    public void OnShown() => FillList();

    public void OnTimer()
    {
    }

    private static MaterialIconKind IconOf(ConnectionKind k) => k switch
    {
        ConnectionKind.Simulator => MaterialIconKind.FlaskOutline,
        ConnectionKind.BinanceFutures or ConnectionKind.BinanceSpot => MaterialIconKind.Bitcoin,
        ConnectionKind.Bybit => MaterialIconKind.CurrencyUsd,
        ConnectionKind.Rithmic => MaterialIconKind.ChartTimelineVariant,
        ConnectionKind.AmpFutures => MaterialIconKind.ChartBellCurveCumulative,
        ConnectionKind.Tradovate => MaterialIconKind.ChartLineVariant,
        ConnectionKind.CTrader => MaterialIconKind.ChartAreaspline,
        ConnectionKind.Oanda => MaterialIconKind.Earth,
        ConnectionKind.Alpaca => MaterialIconKind.Finance,
        ConnectionKind.Databento => MaterialIconKind.DatabaseArrowDownOutline,
        _ => MaterialIconKind.Bank,
    };

    private Control Build()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("320,*") };

        // ---- lista
        var add = new Button { Content = Ui.H(6, Ui.Icon(MaterialIconKind.Plus, 16), new TextBlock { Text = "New connection" }), HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center };
        var menu = new MenuFlyout();
        foreach (var kind in Enum.GetValues<ConnectionKind>())
        {
            // qué da cada una, para elegir sin tener que saberlo
            string? hint = kind switch
            {
                ConnectionKind.Tradovate => "futures: trading accounts and data",
                ConnectionKind.CTrader => "CFDs and forex from IC Markets, Pepperstone, FxPro…",
                ConnectionKind.Oanda => "Indices, gold and forex CFDs (free practice)",
                ConnectionKind.Alpaca => "US stocks and ETFs (SPY, QQQ) for free",
                ConnectionKind.Databento => "CME futures history since 2010",
                _ => null,
            };
            object itemHeader = hint == null ? Ui.EnumName(kind) : Ui.V(1, new TextBlock { Text = Ui.EnumName(kind) }, new TextBlock { Text = hint, Classes = { "faint", "small" } });
            var mi = new MenuItem { Header = itemHeader, Icon = Ui.Icon(IconOf(kind), 16) };
            mi.Click += (_, _) => AddProfile(kind);
            menu.Items.Add(mi);
        }

        add.Flyout = menu;
        var header = Ui.V(4, Ui.H1("Connections"), Ui.Muted("Market data and trading accounts"));
        header.Margin = new Thickness(0, 0, 0, 16);
        var left = new DockPanel { Margin = new Thickness(20, 20, 12, 20) };
        DockPanel.SetDock(header, Dock.Top);
        left.Children.Add(header);
        DockPanel.SetDock(add, Dock.Bottom);
        add.Margin = new Thickness(0, 12, 0, 0);
        left.Children.Add(add);
        _list.SelectionChanged += (_, _) =>
        {
            if (_list.SelectedItem is ListBoxItem { Tag: ConnectionProfile p }) Select(p);
        };
        _list.Background = Brushes.Transparent;
        left.Children.Add(_list);
        grid.Children.Add(new Border { Child = left, Background = Ui.Bg1, BorderBrush = Ui.Line, BorderThickness = new Thickness(0, 0, 1, 0) });

        // ---- detalle
        _detail.Margin = new Thickness(28, 24, 28, 32);
        var scroll = new ScrollViewer { Content = _detail };
        Grid.SetColumn(scroll, 1);
        grid.Children.Add(scroll);
        return grid;
    }

    private void FillList()
    {
        var keep = _selected?.Id;
        _list.Items.Clear();
        foreach (var p in _svc.Connections.Profiles)
        {
            var dot = new Avalonia.Controls.Shapes.Ellipse { Width = 8, Height = 8, VerticalAlignment = VerticalAlignment.Center };
            dot.Fill = !p.Enabled ? Ui.Text2 : _results.TryGetValue(p.Id, out var r) ? (r.Ok ? Ui.Up : Ui.Down) : Ui.Line;
            ToolTip.SetTip(dot, !p.Enabled ? "Disabled" : _results.TryGetValue(p.Id, out var r2) ? r2.Message : "Untested");
            var icon = new Border { Width = 34, Height = 34, CornerRadius = new CornerRadius(8), Background = Ui.Bg3, Child = Ui.Icon(IconOf(p.Kind), 18, p.Enabled ? Ui.Accent : Ui.Text2) };
            string sub = Describe(p);
            var text = Ui.V(1, new TextBlock { Text = p.Name, FontWeight = FontWeight.SemiBold, Opacity = p.Enabled ? 1 : 0.6 }, new TextBlock { Text = sub, Classes = { "muted", "small" } });
            var row = new DockPanel { Margin = new Thickness(2, 4) };
            DockPanel.SetDock(icon, Dock.Left);
            icon.Margin = new Thickness(0, 0, 12, 0);
            DockPanel.SetDock(dot, Dock.Right);
            row.Children.Add(icon);
            row.Children.Add(dot);
            row.Children.Add(text);
            _list.Items.Add(new ListBoxItem { Content = row, Tag = p });
        }

        var sel = _list.Items.OfType<ListBoxItem>().FirstOrDefault(i => ((ConnectionProfile)i.Tag!).Id == keep) ?? _list.Items.OfType<ListBoxItem>().FirstOrDefault();
        _list.SelectedItem = sel;
    }

    /// <summary>Resumen de una conexión para la lista.</summary>
    private string Describe(ConnectionProfile p)
    {
        string prefix = p.Name == Ui.EnumName(p.Kind) ? "" : Ui.EnumName(p.Kind) + " · ";
        if (!p.Enabled) return prefix + "disabled";
        string detail = p.Kind switch
        {
            ConnectionKind.Simulator => "simulated market",
            ConnectionKind.BinanceFutures or ConnectionKind.BinanceSpot => string.IsNullOrWhiteSpace(p.ApiKey)
                ? "public data no account"
                : p.Testnet ? "data + testnet account" : p.AllowRealOrders ? "data + REAL account" : "data + real account (read only)",
            ConnectionKind.Bybit => "public data",
            ConnectionKind.Rithmic or ConnectionKind.AmpFutures => string.IsNullOrWhiteSpace(p.RithmicUser) ? "no user configured" : $"{p.RithmicUser} · {p.RithmicSystem}",
            ConnectionKind.Tradovate => string.IsNullOrWhiteSpace(p.TradovateUser) ? "no user configured" : $"{p.TradovateUser} {(p.Testnet ? "demo" : p.AllowRealOrders ? "REAL" : "real (read only)")}",
            ConnectionKind.CTrader => string.IsNullOrWhiteSpace(p.CTraderClientId) ? "no app configured" : $"{(p.Testnet ? "demo" : "real")}{(p.CTraderLogin > 0 ? "· account" + p.CTraderLogin : "")}",
            ConnectionKind.Oanda => (p.Testnet ? "practice" : "real") + (string.IsNullOrWhiteSpace(p.OandaAccount) ? "" : " · " + p.OandaAccount),
            ConnectionKind.Alpaca => string.IsNullOrWhiteSpace(p.ApiKey) ? "without keys" : $"SIP history live {p.AlpacaLiveFeed.ToUpperInvariant()}",
            ConnectionKind.Databento => "CME futures history (pay as you go)",
            _ => $"{p.IbHost}:{p.IbPort}",
        };
        return prefix + detail;
    }

    private void AddProfile(ConnectionKind kind)
    {
        int n = _svc.Connections.Profiles.Count(p => p.Kind == kind) + 1;
        var p = new ConnectionProfile { Kind = kind, Name = Ui.EnumName(kind) + (n > 1 ? $" {n}" : ""), Enabled = kind is not (ConnectionKind.Rithmic or ConnectionKind.AmpFutures) };
        if (kind is ConnectionKind.CTrader) p.Name = n > 1 ? $"cTrader {n}" : "cTrader";
        if (kind is ConnectionKind.Alpaca) p.Name = n > 1 ? $"Alpaca {n}" : "Alpaca";
        if (kind is ConnectionKind.Databento) p.Name = "Databento";
        // el primero de cada tipo usa el id de sus datos (así los archivos de ticks quedan con un nombre claro)
        string defaultId = ConnectionProfile.DefaultId(kind);
        if (kind is ConnectionKind.Tradovate or ConnectionKind.CTrader or ConnectionKind.Oanda or ConnectionKind.Alpaca or ConnectionKind.Databento
            && _svc.Connections.Profiles.All(x => x.Id != defaultId))
            p.Id = defaultId;
        if (kind == ConnectionKind.Simulator) p.HistoryHours = 6;
        if (kind == ConnectionKind.AmpFutures)
        {
            // AMP da cuentas de Rithmic: servidor de producción (Chicago) y el sistema de la demo por defecto
            p.RithmicUri = RithmicGateways.ChicagoUri;
            p.RithmicSystem = "Rithmic Paper Trading";
        }
        _svc.Connections.Profiles.Add(p);
        _svc.Connections.Save();
        _selected = p;
        FillList();
    }

    // ===================================================================== detalle

    private void Select(ConnectionProfile p)
    {
        _selected = p;
        _detail.Content = BuildDetail(p);
    }

    private Control BuildDetail(ConnectionProfile p)
    {
        var name = new TextBox { Text = p.Name, FontSize = 18, FontWeight = FontWeight.SemiBold, MinWidth = 170, MaxWidth = 300 };
        var enabled = new ToggleSwitch { IsChecked = p.Enabled, OnContent = "Activated", OffContent = "Disabled" };
        var history = new ComboBox { MinWidth = 160 };
        foreach (var (h, label) in new[] { (0.5, "30 minutes"), (1.0, "1 hour"), (1.5, "1.5 hours"), (3.0, "3 hours"), (6.0, "6 hours"), (12.0, "12 hours"), (24.0, "1 day"), (72.0, "3 days"), (168.0, "7 days") })
            history.Items.Add(new ComboBoxItem { Content = label, Tag = h });
        history.SelectedItem = history.Items.OfType<ComboBoxItem>().OrderBy(i => Math.Abs((double)i.Tag! - p.HistoryHours)).First();

        var resultIcon = new ContentControl();
        var resultText = new TextBlock { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
        var resultBox = new Border { Child = Ui.H(10, resultIcon, resultText), Padding = new Thickness(12, 10), CornerRadius = new CornerRadius(8), Background = Ui.Bg2, IsVisible = false };
        void ShowResult(bool ok, string msg, bool running = false)
        {
            resultBox.IsVisible = true;
            resultIcon.Content = running ? Ui.Icon(MaterialIconKind.ProgressClock, 18, Ui.Text1) : Ui.Icon(ok ? MaterialIconKind.CheckCircle : MaterialIconKind.CloseCircle, 18, ok ? Ui.Up : Ui.Down);
            resultText.Text = msg;
        }

        if (_results.TryGetValue(p.Id, out var last)) ShowResult(last.Ok, $"{last.Message}  ({Ui.Ago(last.When)})");

        // campos según el tipo (se escriben en el perfil al guardar)
        var apply = new List<Action>();
        var sections = new StackPanel { Spacing = 16 };

        var dataSection = Ui.V(14,
            Ui.H2("Market data"),
            Ui.Field("History when opening a chart", history, "What was recorded or downloaded in Data loads instantly; the rest is requested from the connection."));
        sections.Children.Add(Ui.Card(dataSection));
        apply.Add(() => p.HistoryHours = (double)((ComboBoxItem)history.SelectedItem!).Tag!);

        switch (p.Kind)
        {
            case ConnectionKind.Simulator:
                dataSection.Children.Add(Note(MaterialIconKind.InformationOutline, "Simulated market with realistic book and trades, ideal for trying offline. No configuration needed."));
                break;
            case ConnectionKind.BinanceFutures:
            case ConnectionKind.BinanceSpot:
                dataSection.Children.Add(Note(MaterialIconKind.InformationOutline, "Binance market data is public: no account required to view charts, DOM, and tape."));
                sections.Children.Add(Ui.Card(BinanceAccount(p, apply)));
                break;
            case ConnectionKind.Bybit:
                dataSection.Children.Add(Note(MaterialIconKind.InformationOutline, "Bybit linear perpetual (USDT) public data. Trading with Bybit account is not yet available."));
                break;
            case ConnectionKind.Rithmic:
            case ConnectionKind.AmpFutures:
                sections.Children.Add(Ui.Card(RithmicSection(p, apply)));
                break;
            case ConnectionKind.InteractiveBrokers:
                sections.Children.Add(Ui.Card(IbSection(p, apply)));
                break;
            case ConnectionKind.Tradovate:
                sections.Children.Add(Ui.Card(TradovateSection(p, apply)));
                break;
            case ConnectionKind.CTrader:
                dataSection.Children.Add(Note(MaterialIconKind.InformationOutline, "CFDs do not have real trades: each quote (bid) counts as a volume 1 trade. It is used for price, candles, replay and backtests; the footprint and delta are approximate."));
                sections.Children.Add(Ui.Card(CTraderSection(p, apply)));
                break;
            case ConnectionKind.Oanda:
                dataSection.Children.Add(Note(MaterialIconKind.InformationOutline, "CFDs do not have real trades: each quote (average price) counts as a volume 1 trade. It is used for price, candles, replay and backtests; the footprint and delta are approximate."));
                sections.Children.Add(Ui.Card(OandaSection(p, apply)));
                break;
            case ConnectionKind.Alpaca:
                sections.Children.Add(Ui.Card(AlpacaSection(p, apply)));
                break;
            case ConnectionKind.Databento:
                dataSection.IsVisible = false;
                sections.Children.Add(Ui.Card(DatabentoSection(p, apply)));
                break;
        }

        // ---- encabezado y acciones
        var test = Ui.Button("Test connection", MaterialIconKind.LanConnect, () => { });
        var save = Ui.Button("Save", MaterialIconKind.ContentSaveOutline, () => { }, "primary");
        var delete = Ui.Button("Delete", MaterialIconKind.DeleteOutline, () => { }, "ghost");
        delete.IsVisible = _svc.Connections.Profiles.Count(x => x.Kind == p.Kind) > 1 || p.Kind is ConnectionKind.InteractiveBrokers or ConnectionKind.AmpFutures
            or ConnectionKind.Tradovate or ConnectionKind.CTrader or ConnectionKind.Oanda or ConnectionKind.Alpaca or ConnectionKind.Databento;

        void Save()
        {
            p.Name = string.IsNullOrWhiteSpace(name.Text) ? Ui.EnumName(p.Kind) : name.Text.Trim();
            p.Enabled = enabled.IsChecked == true;
            foreach (var a in apply) a();
            _svc.Connections.Save();
            _svc.Connections.Invalidate(p);
            InstrumentPicker.ClearCache(p.Id);
            _status($"\"{p.Name}\" connection saved");
        }

        save.Click += (_, _) => Save();
        test.Click += async (_, _) =>
        {
            Save();
            _testCts?.Cancel();
            _testCts = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            test.IsEnabled = false;
            ShowResult(true, "Testing…", running: true);
            try
            {
                var (ok, msg) = await _svc.Connections.TestAsync(p, _testCts.Token);
                _results[p.Id] = (ok, msg, DateTime.UtcNow);
                ShowResult(ok, msg);
                FillList();
            }
            finally
            {
                test.IsEnabled = true;
            }
        };
        delete.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is not Window w) return;
            if (!await ConfirmDialog.ShowAsync(w, "Delete connection", $"\"{p.Name}\" and its secrets are deleted from the Keychain. The recorded data is not touched.", "Delete", danger: true)) return;
            _svc.Connections.ClearSecrets(p);
            _svc.Connections.Invalidate(p);
            _svc.Connections.Profiles.Remove(p);
            _svc.Connections.Save();
            _selected = null;
            FillList();
        };

        var kindBadge = new Border { Child = Ui.H(6, Ui.Icon(IconOf(p.Kind), 14, Ui.Accent), new TextBlock { Text = Ui.EnumName(p.Kind), FontSize = 12 }), Classes = { "pill" } };
        var titleRow = new DockPanel();
        // el interruptor va con las acciones: en ventanas angostas no se superpone con «Eliminar»
        enabled.VerticalAlignment = VerticalAlignment.Center;
        enabled.Margin = new Thickness(0, 0, 8, 0);
        var actions = Ui.H(8, enabled, delete, test, save);
        DockPanel.SetDock(actions, Dock.Right);
        titleRow.Children.Add(actions);
        var left = Ui.H(12, name, kindBadge);
        left.ClipToBounds = true;
        titleRow.Children.Add(left);

        return new StackPanel { Spacing = 16, MaxWidth = 860, HorizontalAlignment = HorizontalAlignment.Left, Children = { titleRow, resultBox, sections } };
    }

    private static Control Note(MaterialIconKind icon, string text)
    {
        var t = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Classes = { "muted", "small" }, VerticalAlignment = VerticalAlignment.Center };
        var d = new DockPanel();
        var i = Ui.Icon(icon, 16, Ui.Text2);
        i.Margin = new Thickness(0, 0, 8, 0);
        i.VerticalAlignment = VerticalAlignment.Top;
        DockPanel.SetDock(i, Dock.Left);
        d.Children.Add(i);
        d.Children.Add(t);
        return d;
    }

    private static Grid TwoColumns(params Control[] fields)
    {
        var g = new Grid { ColumnDefinitions = new ColumnDefinitions("*,16,*") };
        int row = 0;
        for (int i = 0; i < fields.Length; i++)
        {
            if (i % 2 == 0)
            {
                g.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                if (i > 0) g.RowDefinitions.Add(new RowDefinition(new GridLength(14)));
                row = g.RowDefinitions.Count - 1;
            }

            Grid.SetRow(fields[i], row);
            Grid.SetColumn(fields[i], i % 2 == 0 ? 0 : 2);
            g.Children.Add(fields[i]);
        }

        return g;
    }

    private Control BinanceAccount(ConnectionProfile p, List<Action> apply)
    {
        var testnet = new ToggleSwitch { IsChecked = p.Testnet, OnContent = "Testnet (test money)", OffContent = "Live account" };
        var key = new TextBox { Text = p.ApiKey, Watermark = "API key" };
        var secret = new TextBox { PasswordChar = '•', Watermark = string.IsNullOrEmpty(_svc.Connections.GetSecret(p)) ? "API secret" : "•••••••• (saved in Keychain)" };
        var allowReal = new CheckBox { Content = "Allow REAL orders from Trading Hub", IsChecked = p.AllowRealOrders };
        var confirm = new CheckBox { Content = "Ask for confirmation before each real order", IsChecked = p.ConfirmOrders };
        var maxQty = new NumericUpDown { Value = p.MaxOrderQuantity, Minimum = 0, Increment = 0.001m, FormatString = "0.#####", Watermark = "no limit" };
        var warn = Note(MaterialIconKind.AlertOutline, "With a real account the orders move real money. By default Trading Hub only operates on testnet; activate real orders only if you know what you are doing.");
        void UpdateEnabled()
        {
            bool real = testnet.IsChecked != true;
            allowReal.IsEnabled = real;
            confirm.IsEnabled = real && allowReal.IsChecked == true;
            maxQty.IsEnabled = real && allowReal.IsChecked == true;
            warn.IsVisible = real;
        }

        testnet.IsCheckedChanged += (_, _) => UpdateEnabled();
        allowReal.IsCheckedChanged += (_, _) => UpdateEnabled();
        UpdateEnabled();

        apply.Add(() =>
        {
            p.Testnet = testnet.IsChecked == true;
            p.ApiKey = (key.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(secret.Text)) _svc.Connections.SetSecret(p, secret.Text.Trim());
            secret.Text = "";
            p.AllowRealOrders = !p.Testnet && allowReal.IsChecked == true;
            p.ConfirmOrders = confirm.IsChecked != false;
            p.MaxOrderQuantity = maxQty.Value ?? 0;
        });

        var help = new Button { Content = "How do I create an API key?", Classes = { "ghost" }, Padding = new Thickness(0), FontSize = 12, Foreground = Ui.Focus };
        help.Click += (_, _) => Shell.Open(testnet.IsChecked == true
            ? (p.Kind == ConnectionKind.BinanceFutures ? "https://testnet.binancefuture.com" : "https://testnet.binance.vision")
            : "https://www.binance.com/es/my/settings/api-management");

        return Ui.V(14,
            Ui.H2("Trading account"),
            Note(MaterialIconKind.ShieldKeyOutline, "Create an API key with trading permission only (no withdrawals). The secret is saved in the macOS Keychain."),
            testnet,
            TwoColumns(Ui.Field("API key", key), Ui.Field("API secret", secret)),
            help,
            Ui.Divider(),
            allowReal,
            confirm,
            Ui.Field("Maximum quantity per order (0 = no limit)", maxQty),
            warn);
    }

    private static string RithmicCacheFile => Path.Combine(AppPaths.Root, "rithmic-servidores.json");

    private static List<RithmicServer> LoadRithmicCache()
    {
        try
        {
            if (File.Exists(RithmicCacheFile))
                return System.Text.Json.JsonSerializer.Deserialize<List<RithmicServer>>(File.ReadAllText(RithmicCacheFile)) ?? new();
        }
        catch
        {
            // caché dañada: se vuelve a pedir
        }

        return new List<RithmicServer>();
    }

    /// <summary>Rithmic como en ATAS: usuario, contraseña, un solo «Servidor» (sistema. región), cotizaciones agregadas y tiempo de espera.</summary>
    private Control RithmicSection(ConnectionProfile p, List<Action> apply)
    {
        var servers = LoadRithmicCache();
        var search = new TextBox { Watermark = "Search server (paper, 01, Apex, Frankfurt, Sao Paulo…)" };
        var server = new ComboBox { HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Left, MaxDropDownHeight = 420, PlaceholderText = "Choose the server" };
        var info = new TextBlock { Classes = { "faint", "small" }, TextWrapping = TextWrapping.Wrap };
        var refresh = Ui.IconButton(MaterialIconKind.Refresh, "Request the list again from all servers", () => { }, 16);
        refresh.Margin = new Thickness(6, 0, 0, 0);
        string selSystem = p.RithmicSystem, selUri = p.RithmicUri;

        // servidor a mano (por si Rithmic agrega uno nuevo)
        var customUri = new TextBox { Text = p.RithmicUri, Watermark = "wss://server:443" };
        var customSystem = new TextBox { Text = p.RithmicSystem, Watermark = "System (e.g. Rithmic 01)" };
        var customToggle = new CheckBox { Content = "Use a server that is not on the list" };
        var customPanel = TwoColumns(Ui.Field("Address", customUri), Ui.Field("System", customSystem));
        customPanel.IsVisible = false;
        customToggle.IsCheckedChanged += (_, _) => customPanel.IsVisible = customToggle.IsChecked == true;

        void Fill()
        {
            var words = (search.Text ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var list = servers.Where(sv => words.All(w => sv.Display.Contains(w, StringComparison.OrdinalIgnoreCase))).ToList();
            int fastest = servers.Count > 0 ? servers.Min(x => x.LatencyMs) : 0;
            server.Items.Clear();
            ComboBoxItem? selected = null;
            foreach (var sv in list)
            {
                var item = new ComboBoxItem
                {
                    Tag = sv,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Content = new DockPanel
                    {
                        Children =
                        {
                            new TextBlock { Text = $"{sv.LatencyMs} ms", Classes = { "faint", "small" }, Foreground = sv.LatencyMs <= fastest * 1.3 ? Ui.Up : null, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) }.Also(t => DockPanel.SetDock(t, Dock.Right)),
                            new TextBlock { Text = sv.Display, VerticalAlignment = VerticalAlignment.Center },
                        },
                    },
                };
                server.Items.Add(item);
                if (sv.System == selSystem && sv.Uri == selUri) selected = item;
            }

            if (selected == null && words.Length == 0)
            {
                // lo guardado no está en la lista (o todavía no llegó): se muestra igual
                var cur = new RithmicServer(selSystem, RithmicGateways.NameOf(selUri), selUri, 0);
                selected = new ComboBoxItem { Tag = cur, Content = new TextBlock { Text = cur.Display } };
                server.Items.Insert(0, selected);
            }

            server.SelectedItem = selected;
        }

        var aggNote = new TextBlock { Classes = { "faint", "small" }, TextWrapping = TextWrapping.Wrap };
        void UpdateAggregated()
        {
            var known = servers.FirstOrDefault(x => x.System == selSystem && x.Uri == selUri);
            aggNote.Text = known == null ? ""
                : known.Aggregated ? "This system offers aggregate quotes."
                : "This system does not offer aggregate quotes: you connect without them (asking for them still causes Rithmic to reject the login).";
        }

        server.SelectionChanged += (_, _) =>
        {
            if (server.SelectedItem is ComboBoxItem { Tag: RithmicServer sv })
            {
                selSystem = sv.System;
                selUri = sv.Uri;
                UpdateAggregated();
            }
        };
        search.TextChanged += (_, _) => Fill();

        async Task DiscoverAsync()
        {
            info.Text = "Consulting the Rithmic servers…";
            refresh.IsEnabled = false;
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                var found = await RithmicDirectory.DiscoverAsync(cts.Token);
                if (found.Count > 0)
                {
                    servers = found;
                    try
                    {
                        Directory.CreateDirectory(AppPaths.Root);
                        File.WriteAllText(RithmicCacheFile, System.Text.Json.JsonSerializer.Serialize(found));
                    }
                    catch
                    {
                        // sin caché no pasa nada
                    }
                }

                Fill();
                UpdateAggregated();
                var best = servers.GroupBy(x => x.Gateway).Select(g => g.First()).OrderBy(x => x.LatencyMs).FirstOrDefault();
                info.Text = servers.Count == 0
                    ? "No server responded (no internet?)."
                    : $"{servers.Count} combinations on {servers.Select(x => x.Uri).Distinct().Count()} servers. The fastest from here: {best?.Gateway} ({best?.LatencyMs} ms). Colo 75 only works from the CME data center.";
            }
            catch (Exception ex)
            {
                info.Text = "Could not consult:" + ex.Message;
            }
            finally
            {
                refresh.IsEnabled = true;
            }
        }

        refresh.Click += async (_, _) => await DiscoverAsync();
        var serverRow = new DockPanel();
        DockPanel.SetDock(refresh, Dock.Right);
        serverRow.Children.Add(refresh);
        serverRow.Children.Add(server);

        var user = new TextBox { Text = p.RithmicUser, Watermark = "user (Login)" };
        var pass = new TextBox { PasswordChar = '•', RevealPassword = false, Watermark = string.IsNullOrEmpty(_svc.Connections.GetSecret(p)) ? "password" : "•••••••• (saved in Keychain)" };
        var reveal = new ToggleButton { Content = Ui.Icon(MaterialIconKind.EyeOutline, 15), Padding = new Thickness(6, 2), MinHeight = 0, VerticalAlignment = VerticalAlignment.Stretch };
        ToolTip.SetTip(reveal, "Show what you write");
        reveal.IsCheckedChanged += (_, _) => pass.RevealPassword = reveal.IsChecked == true;
        var passRow = new DockPanel();
        DockPanel.SetDock(reveal, Dock.Right);
        reveal.Margin = new Thickness(6, 0, 0, 0);
        passRow.Children.Add(reveal);
        passRow.Children.Add(pass);

        var aggregated = new CheckBox { Content = "Added Quotes", IsChecked = p.RithmicAggregatedQuotes };
        ToolTip.SetTip(aggregated, "Rithmic bundles book updates that arrive at the same instant (fewer messages, same price). Same as the ATAS option.");
        var timeout = new NumericUpDown { Value = p.RithmicTimeoutMs, Minimum = 2000, Maximum = 120000, Increment = 1000, FormatString = "0", Width = 140 };
        var app = new TextBox { Text = p.RithmicAppName, Watermark = "TradingHub" };
        var products = new TextBox { Text = string.Join(", ", p.RithmicProducts), Watermark = "ES, NQ, MES, MNQ, CL…" };

        // órdenes: las cuentas de práctica operan siempre; las reales (Rithmic 01, prop firms) solo si se habilita acá
        var allowReal = new CheckBox { Content = "Allow orders in real accounts (Rithmic 01, prop firms…)", IsChecked = p.AllowRealOrders };
        var confirm = new CheckBox { Content = "Confirm each real order", IsChecked = p.ConfirmOrders };
        var maxQty = new NumericUpDown { Value = p.MaxOrderQuantity, Minimum = 0, Maximum = 10000, Increment = 1, FormatString = "0", Width = 140 };

        apply.Add(() =>
        {
            if (customToggle.IsChecked == true && !string.IsNullOrWhiteSpace(customUri.Text))
            {
                p.RithmicUri = customUri.Text.Trim();
                if (!string.IsNullOrWhiteSpace(customSystem.Text)) p.RithmicSystem = customSystem.Text.Trim();
            }
            else
            {
                p.RithmicUri = selUri;
                p.RithmicSystem = selSystem;
            }

            p.RithmicUser = (user.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(pass.Text)) _svc.Connections.SetSecret(p, pass.Text);
            pass.Text = "";
            p.RithmicAggregatedQuotes = aggregated.IsChecked == true;
            p.RithmicTimeoutMs = (int)(timeout.Value ?? 10000);
            p.RithmicAppName = string.IsNullOrWhiteSpace(app.Text) ? "TradingHub" : app.Text.Trim();
            p.RithmicProducts = (products.Text ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => x.ToUpperInvariant()).Distinct().ToList();
            p.AllowRealOrders = allowReal.IsChecked == true;
            p.ConfirmOrders = confirm.IsChecked == true;
            p.MaxOrderQuantity = maxQty.Value ?? 0;
        });

        Fill();
        Dispatcher.UIThread.Post(() => _ = DiscoverAsync(), DispatcherPriority.Background);
        return Ui.V(14,
            Ui.H2(p.Kind == ConnectionKind.AmpFutures ? "AMP Futures Account" : "Rithmic Account"),
            Note(MaterialIconKind.InformationOutline, p.Kind == ConnectionKind.AmpFutures
                ? "AMP Futures operates on Rithmic: AMP sends you Rithmic username and password. For the demo choose “Rithmic Paper Trading” and for the real account “Rithmic 01”, on the nearest server. Trading Hub connects directly through R|Protocol (you do not need to have R|Trader Pro open)."
                : "Direct connection through R|Protocol, the official Rithmic API (R|Trader Pro is not necessary). Choose the server as in ATAS: system (Paper Trading, Rithmic 01, your prop firm) and region."),
            Ui.H3("Connection"),
            TwoColumns(Ui.Field("User", user), Ui.Field("Password", passRow)),
            Ui.Field("Server", Ui.V(8, search, serverRow, info)),
            customToggle,
            customPanel,
            Ui.H3("Market data"),
            TwoColumns(Ui.V(4, aggregated, aggNote), Ui.Field("Connection timeout (ms)", timeout)),
            Ui.H3("Trade"),
            Note(MaterialIconKind.ShieldCheckOutline, "Your Rithmic accounts appear in the Chart Trader (“Account”) when the connection is active. The practice ones (Paper Trading) operate directly; the real ones need this authorization and ask for confirmation."),
            allowReal,
            TwoColumns(confirm, Ui.Field("Maximum quantity per order (0 = no limit)", maxQty)),
            Ui.H3("Advanced"),
            TwoColumns(Ui.Field("Application name", app, "Some systems require an approved app name."), Ui.Field("Products for the search engine", products, "The contract of the month for each product is searched (e.g. ES → ESZ6).")));
    }

    /// <summary>Campo de secreto: no muestra lo guardado, solo avisa que está en el Llavero.</summary>
    private TextBox SecretBox(ConnectionProfile p, string? name, string watermark) =>
        new() { PasswordChar = '•', Watermark = string.IsNullOrEmpty(name == null ? _svc.Connections.GetSecret(p) : _svc.Connections.GetSecret(p, name)) ? watermark : "•••••••• (saved in Keychain)" };

    private static Button Link(string text, string url)
    {
        var b = new Button { Content = text, Classes = { "ghost" }, Padding = new Thickness(0), FontSize = 12, Foreground = Ui.Focus };
        b.Click += (_, _) => Shell.Open(url);
        ToolTip.SetTip(b, url);
        return b;
    }

    /// <summary>Tradovate: usuario, contraseña y API key (CID + secret); demo o real; productos.</summary>
    private Control TradovateSection(ConnectionProfile p, List<Action> apply)
    {
        var demo = new ToggleSwitch { IsChecked = p.Testnet, OnContent = "Demo (simulated from Tradovate)", OffContent = "Live account" };
        var user = new TextBox { Text = p.TradovateUser, Watermark = "Tradovate user" };
        var pass = SecretBox(p, null, "password");
        var cid = new TextBox { Text = p.TradovateCid, Watermark = "CID (number)" };
        var sec = SecretBox(p, "apisecret", "API key secret");
        var products = new TextBox { Text = string.Join(", ", p.TradovateProducts), Watermark = "ES, NQ, MES, MNQ, CL…" };
        var allowReal = new CheckBox { Content = "Allow orders on real Tradovate accounts", IsChecked = p.AllowRealOrders };
        var confirm = new CheckBox { Content = "Ask for confirmation before each real order", IsChecked = p.ConfirmOrders };
        var maxQty = new NumericUpDown { Value = p.MaxOrderQuantity, Minimum = 0, Maximum = 10000, Increment = 1, FormatString = "0", Width = 140 };
        void Update()
        {
            allowReal.IsEnabled = demo.IsChecked != true;
            confirm.IsEnabled = demo.IsChecked != true && allowReal.IsChecked == true;
        }

        allowReal.IsCheckedChanged += (_, _) => Update();
        demo.IsCheckedChanged += (_, _) => Update();
        Update();
        apply.Add(() =>
        {
            p.Testnet = demo.IsChecked == true;
            p.TradovateUser = (user.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(pass.Text)) _svc.Connections.SetSecret(p, pass.Text);
            pass.Text = "";
            p.TradovateCid = (cid.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(sec.Text)) _svc.Connections.SetSecret(p, "apisecret", sec.Text.Trim());
            sec.Text = "";
            p.TradovateProducts = (products.Text ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => x.ToUpperInvariant()).Distinct().ToList();
            p.AllowRealOrders = !p.Testnet && allowReal.IsChecked == true;
            p.ConfirmOrders = confirm.IsChecked != false;
            p.MaxOrderQuantity = maxQty.Value ?? 0;
        });
        return Ui.V(14,
            Ui.H2("Tradovate Account"),
            Note(MaterialIconKind.InformationOutline, "Direct connection to the official Tradovate API. Your accounts appear in Chart Trader and Account, and can be traded from any CME futures chart (also a Rithmic one). The demos trade directly; the real ones need authorization from below."),
            Note(MaterialIconKind.AlertOutline, "Market data by API: Tradovate only provides this if your account has CME data for API enabled (a separate license from CME). Without that you can still trade, with Rithmic charts or another connection."),
            demo,
            TwoColumns(Ui.Field("User", user), Ui.Field("Password", pass)),
            TwoColumns(Ui.Field("API key CID", cid), Ui.Field("API key secret", sec)),
            Ui.H(16, Link("Where do I get the API key? (Settings → API Access)", "https://trader.tradovate.com/"), Link("API documentation", "https://partner.tradovate.com")),
            Ui.Field("Products for the search engine", products, "The contract of the month for each one is used (ES → ESZ6)."),
            Ui.Divider(),
            allowReal,
            confirm,
            Ui.Field("Maximum quantity per order (0 = no limit)", maxQty),
            Note(MaterialIconKind.ShieldCheckOutline, "Tradovate leaves only one data session per user: if you open Tradovate Trader with the same user, it can cut off the Trading Hub session (and vice versa)."));
    }

    /// <summary>cTrader: aplicación de la Open API (Client ID + Secret) y token de la cuenta (OAuth o Playground).</summary>
    private Control CTraderSection(ConnectionProfile p, List<Action> apply)
    {
        const string redirect = "http://127.0.0.1:53682/ctrader";
        var demo = new ToggleSwitch { IsChecked = p.Testnet, OnContent = "Demo account", OffContent = "Live account" };
        var clientId = new TextBox { Text = p.CTraderClientId, Watermark = "Client ID of your application" };
        var secret = SecretBox(p, null, "Secret of your application");
        var token = SecretBox(p, "token", "access token (or tap “Connect with cTrader”)");
        var login = new NumericUpDown { Value = p.CTraderLogin, Minimum = 0, Maximum = 999999999999, FormatString = "0", Watermark = "the first", HorizontalAlignment = HorizontalAlignment.Stretch };
        var oauthStatus = new TextBlock { Classes = { "faint", "small" }, TextWrapping = TextWrapping.Wrap };
        var connect = Ui.Button("Connect with cTrader", MaterialIconKind.Login, () => { }, "primary");
        connect.Click += async (_, _) =>
        {
            string id = (clientId.Text ?? "").Trim();
            string sec = !string.IsNullOrWhiteSpace(secret.Text) ? secret.Text.Trim() : _svc.Connections.GetSecret(p);
            if (id.Length == 0 || sec.Length == 0)
            {
                oauthStatus.Text = "First complete the Client ID and Secret of your application.";
                return;
            }

            connect.IsEnabled = false;
            oauthStatus.Text = "The browser opened: log in with your cTrader ID and authorize Trading Hub. Waiting…";
            try
            {
                var (access, refresh) = await CTraderOAuthAsync(id, sec, redirect);
                p.CTraderClientId = id;
                _svc.Connections.SetSecret(p, sec);
                _svc.Connections.SetSecret(p, "token", access);
                if (refresh.Length > 0) _svc.Connections.SetSecret(p, "refresh", refresh);
                _svc.Connections.Save();
                _svc.Connections.Invalidate(p);
                token.Watermark = "•••••••• (saved in Keychain)";
                oauthStatus.Text = "Ready: Trading Hub is authorized. Tap “Test Connection.”";
            }
            catch (Exception ex)
            {
                oauthStatus.Text = "Could not authorize:" + ex.Message;
            }
            finally
            {
                connect.IsEnabled = true;
            }
        };
        var copy = Ui.Button("Copy", MaterialIconKind.ContentCopy, () =>
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } cb) _ = cb.SetTextAsync(redirect);
        }, "ghost");
        apply.Add(() =>
        {
            p.Testnet = demo.IsChecked == true;
            p.CTraderClientId = (clientId.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(secret.Text)) _svc.Connections.SetSecret(p, secret.Text.Trim());
            secret.Text = "";
            if (!string.IsNullOrWhiteSpace(token.Text)) _svc.Connections.SetSecret(p, "token", token.Text.Trim());
            token.Text = "";
            p.CTraderLogin = (long)(login.Value ?? 0);
        });
        return Ui.V(14,
            Ui.H2("cTrader Open API"),
            Note(MaterialIconKind.InformationOutline, "Free with any broker that has cTrader (IC Markets, Pepperstone, FxPro, Fusion Markets...) and a demo account: CFDs on indices (US100/NAS100, US500, GER40...), forex, gold and oil, with tick-by-tick history for years."),
            Ui.H3("1 · Your application (one time only)"),
            Note(MaterialIconKind.ShieldKeyOutline, "Create an app at openapi.ctrader.com (you log in with your cTrader ID) and add this redirection address. Then copy the Client ID and Secret here."),
            Ui.H(8, new TextBlock { Text = redirect, FontFamily = new FontFamily("Menlo, monospace"), VerticalAlignment = VerticalAlignment.Center }, copy, Link("Open openapi.ctrader.com", "https://openapi.ctrader.com/apps")),
            TwoColumns(Ui.Field("Client ID", clientId), Ui.Field("Secret", secret)),
            Ui.H3("2 · Authorize your account"),
            Ui.H(10, connect, demo),
            oauthStatus,
            Ui.Field("Or paste an access token (the one from your app's “Playground”)", token),
            Ui.Field("Account number (cTrader login)", login, "If the token has multiple accounts, the one you want to use. 0 = the first demo/real."));
    }

    /// <summary>OAuth de cTrader: abre el navegador y espera el código en la dirección local de redirección.</summary>
    private static async Task<(string Access, string Refresh)> CTraderOAuthAsync(string clientId, string secret, string redirect)
    {
        var uri = new Uri(redirect);
        using var listener = new System.Net.HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{uri.Port}/");
        listener.Start();
        Shell.Open(TradingHub.Feeds.CTrader.CTraderAuth.AuthorizeUrl(clientId, redirect));
        var wait = listener.GetContextAsync();
        if (await Task.WhenAny(wait, Task.Delay(TimeSpan.FromMinutes(5))) != wait) throw new TimeoutException("authorization did not arrive (5 minutes)");
        var ctx = await wait;
        string? code = ctx.Request.QueryString["code"];
        string? error = ctx.Request.QueryString["error"] ?? ctx.Request.QueryString["error_description"];
        var html = System.Text.Encoding.UTF8.GetBytes(code != null
            ? "<html><body style='font-family:-apple-system;padding:40px'><h2>Done</h2><p>Trading Hub is authorized. You can now close this tab and return to Trading Hub.</p></body></html>"
            : "<html><body style='font-family:-apple-system;padding:40px'><h2>Authorization not granted</h2><p>Return to Trading Hub and try again.</p></body></html>");
        ctx.Response.ContentType = "text/html; charset=utf-8";
        await ctx.Response.OutputStream.WriteAsync(html);
        ctx.Response.Close();
        if (code == null) throw new InvalidOperationException(error ?? "cTrader did not send the code");
        var (access, refresh, _) = await TradingHub.Feeds.CTrader.CTraderAuth.ExchangeAsync(clientId, secret, code, redirect, CancellationToken.None);
        return (access, refresh);
    }

    /// <summary>OANDA: token personal y cuenta de práctica (gratis) o real.</summary>
    private Control OandaSection(ConnectionProfile p, List<Action> apply)
    {
        var practice = new ToggleSwitch { IsChecked = p.Testnet, OnContent = "Practice account (free)", OffContent = "Live account" };
        var token = SecretBox(p, null, "API token (Manage API Access)");
        var account = new TextBox { Text = p.OandaAccount, Watermark = "101-001-… (empty = the first)" };
        apply.Add(() =>
        {
            p.Testnet = practice.IsChecked == true;
            if (!string.IsNullOrWhiteSpace(token.Text)) _svc.Connections.SetSecret(p, token.Text.Trim());
            token.Text = "";
            p.OandaAccount = (account.Text ?? "").Trim();
        });
        return Ui.V(14,
            Ui.H2("OANDA (API v20)"),
            Note(MaterialIconKind.InformationOutline, "Free with a practice account: Index CFDs (NAS100_USD = Nasdaq 100, SPX500_USD = S&P 500, US30_USD, DE30_EUR...), gold, oil and forex, with years of history in 5-second candles."),
            practice,
            TwoColumns(Ui.Field("Token", token), Ui.Field("Account", account)),
            Ui.H(16, Link("Create practice account", "https://www.oanda.com/demo-account/"), Link("Generate the token (Manage API Access)", "https://www.oanda.com/demo-account/tpa/personal_token")),
            Note(MaterialIconKind.ShieldKeyOutline, "The token is saved in the Keychain. Trading Hub uses it for data only (it does not send orders to OANDA)."));
    }

    /// <summary>Alpaca: claves de la cuenta de práctica (historia SIP desde 2016 gratis).</summary>
    private Control AlpacaSection(ConnectionProfile p, List<Action> apply)
    {
        var key = new TextBox { Text = p.ApiKey, Watermark = "API key ID" };
        var secret = SecretBox(p, null, "secret key");
        var live = new ComboBox { MinWidth = 260 };
        live.Items.Add(new ComboBoxItem { Content = "IEX (free, one bag)", Tag = "iex" });
        live.Items.Add(new ComboBoxItem { Content = "SIP (all bags, paid plan)", Tag = "sip" });
        live.SelectedItem = live.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (string)i.Tag! == p.AlpacaLiveFeed) ?? live.Items[0];
        apply.Add(() =>
        {
            p.ApiKey = (key.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(secret.Text)) _svc.Connections.SetSecret(p, secret.Text.Trim());
            secret.Text = "";
            p.AlpacaLiveFeed = (live.SelectedItem as ComboBoxItem)?.Tag as string ?? "iex";
        });
        return Ui.V(14,
            Ui.H2("Alpaca (US Stocks and ETFs)"),
            Note(MaterialIconKind.InformationOutline, "With the practice account (free, from any country) the tick-by-tick history of ALL US stocks since 2016 is free (except the last 15 minutes). SPY = S&P 500, QQQ = Nasdaq 100, IWM = Russell 2000, DIA = Dow: real trades to study the indices."),
            TwoColumns(Ui.Field("API key ID", key), Ui.Field("Secret key", secret)),
            Ui.Field("Live data", live),
            Ui.H(16, Link("Create practice account", "https://app.alpaca.markets/signup"), Link("Where are the keys", "https://app.alpaca.markets/paper/dashboard/overview")),
            Note(MaterialIconKind.ShieldKeyOutline, "Trading Hub uses the keys for data only (it does not send orders to Alpaca)."));
    }

    /// <summary>Databento: API key (historia de futuros de CME desde 2010, pago por uso con créditos de regalo).</summary>
    private Control DatabentoSection(ConnectionProfile p, List<Action> apply)
    {
        var key = SecretBox(p, null, "API key (db-…)");
        apply.Add(() =>
        {
            if (!string.IsNullOrWhiteSpace(key.Text)) _svc.Connections.SetSecret(p, key.Text.Trim());
            key.Text = "";
        });
        return Ui.V(14,
            Ui.H2("Databento"),
            Note(MaterialIconKind.InformationOutline, "Tick-by-tick history of CME futures (NQ, ES, MNQ, MES, CL, GC…) since 2010, with the real aggressor side of each trade: what Rithmic does not save. It is pay per use; new accounts come with US$125 of free credits."),
            Ui.Field("API key", key),
            Ui.H(16, Link("Create account (with gift credits)", "https://databento.com/signup"), Link("Prices", "https://databento.com/pricing")),
            Note(MaterialIconKind.ShieldCheckOutline, "It is used from Data → “CME Futures with Databento”: Trading Hub calculates the cost before and only goes down if you confirm. It is saved separately (“Databento” in Data) and in the backtests you choose only Databento, only Rithmic or both combined (Rithmic first and Databento completes what is missing)."));
    }

    private Control IbSection(ConnectionProfile p, List<Action> apply)
    {
        var host = new TextBox { Text = p.IbHost };
        var port = new NumericUpDown { Value = p.IbPort, Minimum = 1, Maximum = 65535, FormatString = "0" };
        var client = new NumericUpDown { Value = p.IbClientId, Minimum = 0, Maximum = 999, FormatString = "0" };
        apply.Add(() =>
        {
            p.IbHost = string.IsNullOrWhiteSpace(host.Text) ? "127.0.0.1" : host.Text.Trim();
            p.IbPort = (int)(port.Value ?? 7497);
            p.IbClientId = (int)(client.Value ?? 17);
        });
        return Ui.V(14,
            Ui.H2("Interactive Brokers (TWS / IB Gateway)"),
            Note(MaterialIconKind.InformationOutline, "Open TWS or IB Gateway and enable the API (Configure → API → Settings). Typical ports: 7497 TWS paper, 7496 real TWS, 4002 Gateway paper. For now Trading Hub only verifies the connection."),
            TwoColumns(Ui.Field("Host", host), Ui.Field("Port", port)),
            Ui.Field("Client ID", client));
    }
}
