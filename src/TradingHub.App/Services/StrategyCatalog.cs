using TradingHub.Scripting;
using TradingHub.Strategies;

namespace TradingHub.App.Services;

/// <summary>Una estrategia en disco (un archivo .cs) y su última compilación.</summary>
public sealed class StrategyFile
{
    public required string Path { get; init; }
    public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);
    public Type? Type { get; set; }
    public ScriptCompileResult? LastCompile { get; set; }
    public DateTime CompiledFileTime { get; set; }
    public string DisplayName => Type != null ? Strategy.DisplayName(Type) : FileName;

    /// <summary>Id estable para el historial (el nombre de la clase).</summary>
    public string Id => Type?.FullName ?? FileName;

    public Strategy Create() => (Strategy)Activator.CreateInstance(Type ?? throw new InvalidOperationException("The strategy is not compiled."))!;
}

/// <summary>User strategies: files in ~/Documents/Trading Hub/Strategies, compiled with Roslyn.</summary>
public sealed class StrategyCatalog
{
    private readonly ScriptCompiler _compiler = new();

    public StrategyCatalog()
    {
        _ = typeof(Strategy); // asegura que TradingHub.Strategies esté cargado para referenciarlo al compilar
        // TRADINGHUB_BACKTESTS_DIR: historial alternativo (pruebas)
        StrategyStore.Root = Environment.GetEnvironmentVariable("TRADINGHUB_BACKTESTS_DIR") is { Length: > 0 } dir ? dir : AppPaths.Backtests;
        Directory.CreateDirectory(AppPaths.Strategies);
        if (!Directory.EnumerateFiles(AppPaths.Strategies, "*.cs").Any())
        {
            foreach (var t in StrategyTemplates.All.Where(t => t.Example))
                File.WriteAllText(System.IO.Path.Combine(AppPaths.Strategies, t.FileName + ".cs"), t.Code(t.FileName).Replace("__NAME__", t.Name));
        }

        Refresh();
    }

    public List<StrategyFile> Files { get; } = new();

    public event Action? Changed;

    public void Refresh()
    {
        var existing = Files.ToDictionary(f => f.Path);
        Files.Clear();
        var extra = Enumerable.Empty<string>();
        foreach (var path in Directory.EnumerateFiles(AppPaths.Strategies, "*.cs").Concat(extra).OrderBy(p => p))
            Files.Add(existing.TryGetValue(path, out var f) ? f : new StrategyFile { Path = path });
        Changed?.Invoke();
    }

    /// <summary>Compila un archivo (si cambió desde la última vez o si se fuerza).</summary>
    public ScriptCompileResult Compile(StrategyFile file, bool force = false)
    {
        var stamp = File.GetLastWriteTimeUtc(file.Path);
        if (!force && file.LastCompile != null && stamp == file.CompiledFileTime) return file.LastCompile;
        var res = _compiler.Compile(new[] { file.Path }, "Strategy_" + file.FileName);
        file.LastCompile = res;
        file.CompiledFileTime = stamp;
        if (res.Success && res.Assembly != null)
        {
            var types = res.Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(Strategy).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null).ToList();
            file.Type = types.FirstOrDefault();
            if (file.Type == null)
            {
                var diag = new ScriptDiagnostic(file.Path, 1, 1, "error", "TH01", "The file does not have any public class that inherits from Strategy with a parameterless constructor.");
                file.LastCompile = new ScriptCompileResult { Success = false, Diagnostics = res.Diagnostics.Append(diag).ToList(), Elapsed = res.Elapsed };
            }
        }
        else
        {
            file.Type = null;
        }

        Changed?.Invoke();
        return file.LastCompile;
    }

    /// <summary>Crea un archivo nuevo desde una plantilla y devuelve su ruta.</summary>
    public StrategyFile Create(string name, StrategyTemplate template)
    {
        string className = new string(name.Where(char.IsLetterOrDigit).ToArray());
        if (className.Length == 0 || char.IsDigit(className[0])) className = "Strategy" + className;
        var path = System.IO.Path.Combine(AppPaths.Strategies, className + ".cs");
        for (int i = 2; File.Exists(path); i++) path = System.IO.Path.Combine(AppPaths.Strategies, $"{className}{i}.cs");
        className = System.IO.Path.GetFileNameWithoutExtension(path);
        File.WriteAllText(path, template.Code(className).Replace("__NAME__", name.Replace("\"", "'")));
        Refresh();
        return Files.First(f => f.Path == path);
    }

    public void Delete(StrategyFile file)
    {
        if (File.Exists(file.Path)) File.Delete(file.Path);
        Refresh();
    }
}

/// <summary>Plantilla de estrategia.</summary>
public sealed record StrategyTemplate(string Name, string Description, string FileName, bool Example, Func<string, string> Code);

/// <summary>Plantillas incluidas (también se usan como ejemplos la primera vez).</summary>
public static class StrategyTemplates
{
    public static readonly StrategyTemplate[] All =
    {
        new("Moving average crossover", "Long when the fast average crosses above the slow average, short on the opposite cross. A good starting point.", "MovingAverageCross", true, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NAME__", Description = "Crossover of two exponential moving averages with stop and target in ticks.")]
            public class {{c}} : Strategy
            {
                [Parameter("Fast average"), Optimize(5, 30, 5)]
                public int Fast { get; set; } = 10;

                [Parameter("Slow average"), Optimize(20, 100, 10)]
                public int Slow { get; set; } = 40;

                private Ema _fast = null!, _slow = null!;

                protected override void OnInit()
                {
                    _fast = Ema(Fast);
                    _slow = Ema(Slow);
                }

                protected override void OnBar(Bar bar)
                {
                    if (!_slow.IsReady) return;

                    if (CrossAbove(_fast, _slow)) EnterLong();
                    else if (CrossBelow(_fast, _slow)) EnterShort();
                }
            }
            """),

        new("Range breakout", "Buy the breakout of the high of N candles and sell the breakout of the low, with stop by ATR.", "RangeBreakout", true, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NAME__", Description = "Donchian channel breakout with ATR exit.")]
            public class {{c}} : Strategy
            {
                [Parameter("Channel candles"), Optimize(10, 60, 10)]
                public int Channel { get; set; } = 20;

                [Parameter("ATR multiple for stop"), Optimize(1, 4, 0.5)]
                public double AtrMultiple { get; set; } = 2;

                private Highest _highest = null!;
                private Lowest _lowest = null!;
                private Atr _atr = null!;
                private decimal _stop;

                protected override void OnInit()
                {
                    _highest = Highest(Channel);
                    _lowest = Lowest(Channel);
                    _atr = Atr(14);
                }

                protected override void OnBar(Bar bar)
                {
                    if (Bars.Count <= Channel || !_atr.IsReady) return;
                    decimal ceiling = _highest[1], floor = _lowest[1];
                    decimal distance = _atr.Value * (decimal)AtrMultiple;

                    if (IsFlat)
                    {
                        if (bar.Close > ceiling) { EnterLong(); _stop = bar.Close - distance; }
                        else if (bar.Close < floor) { EnterShort(); _stop = bar.Close + distance; }
                    }
                    else if (IsLong)
                    {
                        _stop = Math.Max(_stop, bar.Close - distance);
                        if (bar.Close < _stop) ClosePosition("ATR stop");
                    }
                    else
                    {
                        _stop = Math.Min(_stop, bar.Close + distance);
                        if (bar.Close > _stop) ClosePosition("ATR stop");
                    }
                }
            }
            """),

        new("Delta absorption", "Order flow: candle with a very negative delta that still closes above (absorption of sells) → buy, and vice versa.", "DeltaAbsorption", true, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NAME__", Description = "Looks for absorption: heavy aggressive volume against the move that fails to move price.")]
            public class {{c}} : Strategy
            {
                [Parameter("Candles for volume average")]
                public int Average { get; set; } = 20;

                [Parameter("Minimum volume (x average)"), Optimize(1.0, 3.0, 0.5)]
                public double MinVolume { get; set; } = 1.5;

                [Parameter("Minimum delta (% of volume)"), Optimize(10, 40, 10)]
                public int MinDelta { get; set; } = 20;

                [Parameter("Maximum candles in trade")]
                public int MaxCandles { get; set; } = 6;

                private Sma _volume = null!;
                private int _entryBar;

                public {{c}}()
                {
                    StopLossTicks = 40;
                    TakeProfitTicks = 80;
                }

                protected override void OnInit()
                {
                    UseFootprint = true; // volume by price in each candle (bar.Levels, bar.PocPrice)
                    _volume = Sma(Average, PriceSource.Volume);
                }

                protected override void OnBar(Bar bar)
                {
                    if (!_volume.IsReady) return;

                    if (!IsFlat && CurrentBar - _entryBar >= MaxCandles)
                    {
                        ClosePosition("Time");
                        return;
                    }

                    bool highVolume = bar.Volume >= _volume[1] * (decimal)MinVolume;
                    decimal deltaPct = bar.Volume > 0 ? 100 * bar.Delta / bar.Volume : 0;

                    // heavy aggressive selling but the candle closes above its POC: someone absorbed
                    if (IsFlat && highVolume && deltaPct <= -MinDelta && bar.Close > bar.PocPrice && bar.IsBullish)
                    {
                        EnterLong();
                        _entryBar = CurrentBar;
                    }
                    else if (IsFlat && highVolume && deltaPct >= MinDelta && bar.Close < bar.PocPrice && bar.IsBearish)
                    {
                        EnterShort();
                        _entryBar = CurrentBar;
                    }
                }
            }
            """),

        new("Reversion to VWAP", "Trade the return to the session's VWAP when the price moves too far away, only on a schedule.", "VwapReversion", true, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NAME__", Description = "Sells extensions above VWAP and buys below; exits at VWAP.")]
            public class {{c}} : Strategy
            {
                [Parameter("Distance (x ATR)"), Optimize(1.0, 3.0, 0.5)]
                public double Distance { get; set; } = 2;

                [Parameter("From (local time)")]
                public int FromHour { get; set; } = 10;

                [Parameter("To (local time)")]
                public int ToHour { get; set; } = 20;

                private Vwap _vwap = null!;
                private Atr _atr = null!;

                public {{c}}()
                {
                    StopLossTicks = 60;
                }

                protected override void OnInit()
                {
                    _vwap = Vwap();
                    _atr = Atr(20);
                }

                protected override void OnBar(Bar bar)
                {
                    if (!_atr.IsReady) return;
                    bool inSession = TimeBetween(TimeSpan.FromHours(FromHour), TimeSpan.FromHours(ToHour));
                    decimal band = _atr.Value * (decimal)Distance;

                    if (IsLong && bar.Close >= _vwap.Value) ClosePosition("VWAP");
                    else if (IsShort && bar.Close <= _vwap.Value) ClosePosition("VWAP");
                    else if (IsFlat && inSession)
                    {
                        if (bar.Close < _vwap.Value - band) EnterLong();
                        else if (bar.Close > _vwap.Value + band) EnterShort();
                    }

                    // outside the session it does not stay positioned
                    if (!inSession && !IsFlat) ClosePosition("End of session");
                }
            }
            """),

        new("Empty", "Minimal skeleton with available methods.", "MyStrategy", false, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NAME__", Description = "")]
            public class {{c}} : Strategy
            {
                // [Parameter("Name"), Optimize(min, max, step)] so it appears in optimization
                [Parameter("Period"), Optimize(10, 50, 10)]
                public int Period { get; set; } = 20;

                private Sma _average = null!;

                protected override void OnInit()
                {
                    // indicators: Sma, Ema, Rsi, Atr, Vwap, Bollinger, Macd, Highest, Lowest, CumulativeDelta…
                    _average = Sma(Period);
                }

                protected override void OnBar(Bar bar)
                {
                    // bar = just-closed candle (Open, High, Low, Close, Volume, BuyVolume, SellVolume, Delta…)
                    // orders: EnterLong(), EnterShort(), Buy(), Sell(), BuyLimit(price), SellStop(price), ClosePosition()
                    // automatic stops: StopLossTicks, TakeProfitTicks, TrailingStopTicks
                }
            }
            """),
    };
}
