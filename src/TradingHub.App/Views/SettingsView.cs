using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using TradingHub.App.Services;
using TradingHub.App.Windows;

namespace TradingHub.App.Views;

/// <summary>Ajustes generales de la plataforma.</summary>
internal sealed class SettingsView : UserControl, IPage
{
    private readonly AppServices _svc;
    private readonly Action<string> _status;

    public SettingsView(AppServices svc, Action<string> status)
    {
        _svc = svc;
        _status = status;
        // el margen va en el contenido: con Padding en el ScrollViewer el final quedaba tapado y no se podía bajar
        var content = Build();
        content.Margin = new Thickness(28, 24, 28, 32);
        Content = new ScrollViewer { Content = content };
    }

    public void OnShown()
    {
    }

    private int _tick;

    public void OnTimer()
    {
        if (IsVisible && ++_tick % 8 == 0) RefreshSleep(); // ~2 s
    }

    private Control Build()
    {
        var s = _svc.Settings;

        // zona horaria: las habituales primero
        var tz = new ComboBox { MinWidth = 320 };
        var common = new[] { "America/Argentina/Buenos_Aires", "America/New_York", "America/Chicago", "Europe/London", "Europe/Madrid", "UTC" };
        tz.Items.Add(new ComboBoxItem { Content = $"Time for this Mac ({TimeZoneInfo.Local.Id})", Tag = "" });
        foreach (var id in common)
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var z)) tz.Items.Add(new ComboBoxItem { Content = Label(z), Tag = z.Id });
        tz.Items.Add(new Separator());
        foreach (var z in TimeZoneInfo.GetSystemTimeZones().Where(z => !common.Contains(z.Id)).OrderBy(z => z.BaseUtcOffset).ThenBy(z => z.Id))
            tz.Items.Add(new ComboBoxItem { Content = Label(z), Tag = z.Id });
        tz.SelectedItem = tz.Items.OfType<ComboBoxItem>().FirstOrDefault(i => (string)i.Tag! == (s.TimeZoneId ?? "")) ?? tz.Items[0];
        tz.SelectionChanged += (_, _) =>
        {
            if (tz.SelectedItem is not ComboBoxItem { Tag: string id }) return;
            s.TimeZoneId = id == "" ? null : id;
            s.Save();
            if (TopLevel.GetTopLevel(this) is MainWindow w)
            {
                foreach (var t in w.Trading.Tabs)
                {
                    if (t.Model == null) continue;
                    t.Model.Settings.TimeZoneId = s.TimeZoneId;
                    t.Model.Invalidate();
                }
            }

            _status("Updated time zone");
        };

        var confirm = Switch(s.ConfirmRealOrders, v => s.ConfirmRealOrders = v);
        var sounds = Switch(s.SoundAlerts, v => s.SoundAlerts = v);
        var record = Switch(s.RecordLiveData, v =>
        {
            s.RecordLiveData = v;
            _svc.Hub.RecordLive = v;
        });

        var appearance = new Ui.Segmented<string>(new[] { (AppTheme.Dark, "Dark", (string?)"Black"), (AppTheme.Light, "Light", "White") });
        appearance.Select(AppTheme.IsLight ? AppTheme.Light : AppTheme.Dark);
        appearance.SelectionChanged += mode =>
        {
            s.ThemeMode = mode;
            s.Save();
            AppTheme.Apply(mode);
        };
        var general = Section("General", MaterialIconKind.TuneVariant,
            Setting("Appearance", "Black or white throughout the platform (also with the sun/moon button above). The colors of each charts are maintained.", appearance),
            Setting("Chart time zone", "Time axis, tape and session hours.", tz),
            Setting("Alert sounds", "Price alerts, Big Trades and indicator alerts.", sounds)
            );

        var trading = Section("Trade", MaterialIconKind.SwapVertical,
            Setting("Confirm real orders", "Ask for confirmation before sending orders to real accounts (in addition to what is configured in each connection).", confirm),
            Setting("Simulated account", $"Beginning balance {_svc.Portfolio.BeginBalance:N2}. Simulator orders are filled with book queue and real tape.", new TextBlock()));

        var data = Section("Data", MaterialIconKind.DatabaseOutline,
            Setting("Record what comes live", "Save each trade received to instantly reopen charts, replay and backtest.", record),
            Setting("Data folder", AppPaths.Root, Ui.Button("Open", MaterialIconKind.FolderOpenOutline, () => Shell.OpenFolder(AppPaths.Root))),
            Setting("Custom indicators (C#)", AppPaths.UserIndicators, Ui.Button("Open", MaterialIconKind.FolderOpenOutline, () => Shell.OpenFolder(AppPaths.UserIndicators))),
            Setting("Strategies (C#)", AppPaths.Strategies, Ui.Button("Open", MaterialIconKind.FolderOpenOutline, () => Shell.OpenFolder(AppPaths.Strategies))));

        var sleep = BuildSleep();
        var liveStart = BuildLiveStart();
        var hotkeys = BuildHotkeys();

        var logo = new Image { Source = AppIcon.Bitmap, Width = 56, Height = 56 };
        var about = Ui.Card(Ui.H(16, logo, Ui.V(4,
            new TextBlock { Text = TradingHub.Common.Product.Name, Classes = { "h1" } },
            Ui.Muted($"Version {typeof(App).Assembly.GetName().Version?.ToString(3)} · .NET {Environment.Version} · Avalonia + SkiaSharp"),
            Ui.Faint("Personal order flow platform: footprint, DOM, ribbon, indicators in C#, strategies and backtesting."),
            Creator())));

        return new StackPanel
        {
            Spacing = 18,
            MaxWidth = 860,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { Ui.V(4, Ui.H1("Settings"), Ui.Muted("General Trading Hub Preferences")), general, trading, liveStart, sleep, hotkeys, data, about },
        };
    }

    // ===================================================================== estrategias en vivo al abrir

    private Control BuildLiveStart()
    {
        var s = _svc.Settings;
        var auto = Switch(s.AutoResumeLive, v => s.AutoResumeLive = v);
        var real = Switch(s.AutoResumeLiveReal, v => s.AutoResumeLiveReal = v);
        real.IsEnabled = s.AutoResumeLive;
        auto.IsCheckedChanged += (_, _) => real.IsEnabled = auto.IsChecked == true;
        var smart = Switch(s.SmartContracts, v => s.SmartContracts = v);
        var menuBar = Switch(s.KeepInMenuBar, v =>
        {
            s.KeepInMenuBar = v;
            Services.MenuBar.Current?.Refresh();
        });
        return Section("Live strategies", MaterialIconKind.PlayCircleOutline,
            Setting("Stay in menu bar when closing window", "Closing the window does not close Trading Hub: it is at the top right, in the menu bar, with the live strategies working. Tapping its icon opens a panel with strategies and positions in real time. To close it completely: ⌘Q or “Exit Trading Hub” in that panel.", menuBar),
            Setting("Resume them on their own when opening Trading Hub", "Those that were running when Trading Hub was closed (or the Mac was turned off) start again on their own a few seconds after opening, with their same parameters, size and limits; paused strategies come back paused. Off: they wait for you to resume them from Live.", auto),
            Setting("Also those of real accounts", "Off (recommended): those that trade real money wait for you to restart them. On: they start on their own like the others.", real),
            Setting("Choose the contract only (minis or micros)", "If the size of a futures strategy calls for a fraction of the mini (for example 1 NQ × 0.5), the orders go to the micro with the same exposure (5 MNQ), looking at the same chart. With whole minis use the mini. With an open position it remains in the same contract and changes when it flattens.", smart));
    }

    // ===================================================================== modo sin dormir

    private readonly TextBlock _sleepState = new() { Classes = { "faint", "small" }, TextWrapping = TextWrapping.Wrap };

    /// <summary>Quién lo hizo, con enlace a su perfil.</summary>
    private static Control Creator()
    {
        var handle = new TextBlock
        {
            Text = "@ralphtradesz",
            Foreground = Ui.Accent,
            FontWeight = FontWeight.SemiBold,
            TextDecorations = TextDecorations.Underline,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
        };
        handle.PointerPressed += (_, _) => Shell.OpenUrl("https://x.com/ralphtradesz");
        ToolTip.SetTip(handle, "x.com/ralphtradesz");
        return Ui.H(6, new TextBlock { Text = "Created by", Classes = { "faint", "small" }, VerticalAlignment = VerticalAlignment.Center }, handle);
    }

    private Control BuildSleep()
    {
        var s = _svc.Settings;
        var awake = Switch(s.KeepAwake, v => s.KeepAwake = v);
        var power = Switch(s.LidClosedOnlyOnPower, v => s.LidClosedOnlyOnPower = v);
        var lid = Switch(s.KeepAwakeLidClosed, v =>
        {
            s.KeepAwakeLidClosed = v;
            _status(v ? "Lid closed: when something is running, macOS asks you for the password once (it is undone only when you close Trading Hub)" : "Lid closed off: closing the lid puts your Mac to sleep as usual");
        });
        power.IsEnabled = s.KeepAwakeLidClosed;
        lid.IsCheckedChanged += (_, _) => power.IsEnabled = lid.IsChecked == true;
        RefreshSleep();
        return Section("Sleepless mode", MaterialIconKind.SleepOff,
            Setting("Keep your Mac awake", "As long as there are live strategies or Claude agents working, the Mac does not go to sleep due to inactivity (the screen can be turned off). No password.", awake),
            Setting("Keep the lid closed", "When you close the lid, macOS sleeps everything. With this activated, when something is running Trading Hub asks you for the administrator password once and the Mac continues to work with the lid closed as long as Trading Hub is open. It is undone only when Trading Hub is closed (even if it is slammed). With the lid closed, the Mac does not ventilate the same: do not store it in a bag like that.", lid),
            Setting("Only with charger connected", "On battery, your Mac goes to sleep when you close the lid even if it's on (recommended).", power),
            _sleepState);
    }

    private void RefreshSleep()
    {
        var g = _svc.Sleep;
        string reason = _svc.AwakeReason() ?? "there is nothing running";
        _sleepState.Text = (g.Awake ? $"Now: wake up ({g.Reason})." : $"Now: you can sleep ({reason}).") +
                           (g.LidHelperRunning ? "Lid closed: active." : _svc.Settings.KeepAwakeLidClosed ? "Lid closed: activates when something is running." : "") +
                           (g.LidError != null ? " " + g.LidError : "");
    }

    // ===================================================================== atajos de teclado

    private readonly Dictionary<string, Button> _hotkeyButtons = new();

    private Control BuildHotkeys()
    {
        var s = _svc.Settings;
        var list = new StackPanel { Spacing = 6 };
        foreach (var group in Hotkeys.All.GroupBy(h => h.Group))
        {
            list.Children.Add(new TextBlock { Text = group.Key.ToUpperInvariant(), Classes = { "h3" }, Margin = new Thickness(0, list.Children.Count > 0 ? 12 : 0, 0, 2) });
            foreach (var h in group)
            {
                var def = h;
                var key = new Button { MinWidth = 150, HorizontalContentAlignment = HorizontalAlignment.Center, Padding = new Thickness(10, 4) };
                ToolTip.SetTip(key, "Click and press the combination. Esc cancels · ⌫ leaves it unassigned.");
                _hotkeyButtons[def.Id] = key;
                bool capturing = false;
                key.Click += (_, _) =>
                {
                    capturing = true;
                    key.Content = "Press a key...";
                    key.Classes.Add("primary");
                    key.Focus();
                };
                key.LostFocus += (_, _) =>
                {
                    if (!capturing) return;
                    capturing = false;
                    key.Classes.Remove("primary");
                    RefreshHotkey(def.Id);
                };
                key.AddHandler(KeyDownEvent, (_, e) =>
                {
                    if (!capturing) return;
                    e.Handled = true;
                    if (e.Key == Avalonia.Input.Key.Escape)
                    {
                        capturing = false;
                        key.Classes.Remove("primary");
                        RefreshHotkey(def.Id);
                        return;
                    }

                    if (e.Key is Avalonia.Input.Key.Back or Avalonia.Input.Key.Delete)
                    {
                        s.Hotkeys[def.Id] = "";
                    }
                    else
                    {
                        var gesture = Hotkeys.FromKey(e.Key, e.KeyModifiers);
                        if (gesture == null) return; // solo un modificador: sigue esperando
                        // si otra acción ya usaba ese atajo, se le saca
                        foreach (var other in Hotkeys.All.Where(o => o.Id != def.Id && Hotkeys.GestureOf(s, o.Id) == gesture))
                        {
                            s.Hotkeys[other.Id] = "";
                            RefreshHotkey(other.Id);
                            _status($"{Hotkeys.Pretty(gesture)} was in “{other.Name}”: it was changed to “{def.Name}”");
                        }

                        s.Hotkeys[def.Id] = gesture;
                    }

                    capturing = false;
                    key.Classes.Remove("primary");
                    s.Save();
                    RefreshHotkey(def.Id);
                }, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                RefreshHotkey(def.Id);

                var row = new DockPanel();
                DockPanel.SetDock(key, Dock.Right);
                row.Children.Add(key);
                row.Children.Add(new TextBlock { Text = def.Name, VerticalAlignment = VerticalAlignment.Center });
                list.Children.Add(row);
            }
        }

        var reset = Ui.Button("Factory reset", MaterialIconKind.Restore, () =>
        {
            s.Hotkeys.Clear();
            s.Save();
            foreach (var h in Hotkeys.All) RefreshHotkey(h.Id);
            _status("Shortcuts reset");
        });
        reset.HorizontalAlignment = HorizontalAlignment.Left;
        var note = new TextBlock
        {
            Text = "They work in Trading and in separate chart windows, on the active chart. The trading ones come unassigned; in real accounts they still ask for confirmation if that is enabled.",
            Classes = { "muted", "small" },
            TextWrapping = TextWrapping.Wrap,
        };
        return Ui.Card(Ui.V(16, Ui.H(10, Ui.Icon(MaterialIconKind.KeyboardOutline, 18, Ui.Accent), Ui.H2("Keyboard shortcuts")), note, list, reset), 20);
    }

    private void RefreshHotkey(string id)
    {
        if (_hotkeyButtons.TryGetValue(id, out var b)) b.Content = Hotkeys.Pretty(Hotkeys.GestureOf(_svc.Settings, id));
    }

    private static string Label(TimeZoneInfo z)
    {
        var off = z.BaseUtcOffset;
        return $"(UTC{(off < TimeSpan.Zero ? "-" : "+")}{off:hh\\:mm}) {z.Id.Replace('_', ' ')}";
    }

    private ToggleSwitch Switch(bool value, Action<bool> set)
    {
        var t = new ToggleSwitch { IsChecked = value, OnContent = "", OffContent = "" };
        t.IsCheckedChanged += (_, _) =>
        {
            set(t.IsChecked == true);
            _svc.Settings.Save();
        };
        return t;
    }

    private static Control Section(string title, MaterialIconKind icon, params Control[] rows)
    {
        var list = new StackPanel { Spacing = 0 };
        for (int i = 0; i < rows.Length; i++)
        {
            if (i > 0) list.Children.Add(new Border { Height = 1, Margin = new Thickness(0, 14) }.Classes("divider"));
            list.Children.Add(rows[i]);
        }

        return Ui.Card(Ui.V(16, Ui.H(10, Ui.Icon(icon, 18, Ui.Accent), Ui.H2(title)), list), 20);
    }

    private static Control Setting(string title, string description, Control control)
    {
        var text = Ui.V(3, new TextBlock { Text = title, FontWeight = FontWeight.Medium }, new TextBlock { Text = description, Classes = { "muted", "small" }, TextWrapping = TextWrapping.Wrap });
        var row = new DockPanel();
        control.VerticalAlignment = VerticalAlignment.Center;
        control.Margin = new Thickness(24, 0, 0, 0);
        DockPanel.SetDock(control, Dock.Right);
        row.Children.Add(control);
        row.Children.Add(text);
        return row;
    }
}
