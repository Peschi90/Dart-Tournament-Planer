using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace DartTournamentPlaner.Models;

public enum GameMode
{
    Points501,
    Points401,
    Points301
}

public enum FinishMode
{
    SingleOut,
    DoubleOut
}

public enum PostGroupPhaseMode
{
    None,                    // Nur Gruppenphase
    RoundRobinFinals,       // Finalrunde: Beste X Spieler im Round Robin
    KnockoutBracket          // KO-System mit Single/Double Elimination
}

public enum KnockoutMode 
{
    SingleElimination,       // Einfaches KO-System
    DoubleElimination       // Doppeltes KO-System (Winner + Loser Bracket)
}

/// <summary>
/// Defines sets and legs requirements for a specific knockout round
/// </summary>
public class RoundRules : INotifyPropertyChanged
{
    private int _setsToWin;
    private int _legsToWin;
    private int _legsPerSet;

    public RoundRules(int setsToWin = 3, int legsToWin = 3, int legsPerSet = 3)
    {
        _setsToWin = Math.Max(0, setsToWin);  // Erlaubt 0 für "keine Sets"
        _legsToWin = Math.Max(1, legsToWin);  // Legs müssen mindestens 1 sein
        _legsPerSet = Math.Max(1, legsPerSet); // LegsPerSet müssen mindestens 1 sein
    }

    public int SetsToWin
    {
        get => _setsToWin;
        set
        {
            _setsToWin = Math.Max(0, value);  // Erlaubt 0 für "keine Sets"
            OnPropertyChanged();
        }
    }

    public int LegsToWin
    {
        get => _legsToWin;
        set
        {
            _legsToWin = Math.Max(1, value);
            OnPropertyChanged();
        }
    }

    public int LegsPerSet
    {
        get => _legsPerSet;
        set
        {
            _legsPerSet = Math.Max(1, value);
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"Best of {SetsToWin} Sets ({LegsPerSet} Legs per Set) or First to {LegsToWin} Legs";
    }
}

public class GameRules : INotifyPropertyChanged
{
    private GameMode _gameMode = GameMode.Points501;
    private FinishMode _finishMode = FinishMode.DoubleOut;
    private int _legsToWin = 3;
    private bool _playWithSets = false;
    private int _setsToWin = 3;
    private int _legsPerSet = 3;
    
    // Post-Group Phase Settings
    private PostGroupPhaseMode _postGroupPhaseMode = PostGroupPhaseMode.None;
    private int _qualifyingPlayersPerGroup = 2;
    private KnockoutMode _knockoutMode = KnockoutMode.SingleElimination;
    private bool _includeGroupPhaseLosersBracket = false;

    // Round-specific rules for knockout phases
    private Dictionary<KnockoutRound, RoundRules> _knockoutRoundRules;

    public GameRules()
    {
        _knockoutRoundRules = new Dictionary<KnockoutRound, RoundRules>();
        InitializeDefaultKnockoutRules();
    }

    public GameMode GameMode
    {
        get => _gameMode;
        set
        {
            _gameMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GamePoints)); // Notify that GamePoints changed
        }
    }

    /// <summary>
    /// Returns the numeric game points value based on the selected GameMode
    /// For API compatibility
    /// </summary>
    [JsonIgnore]
    public int GamePoints
    {
        get => GameMode switch
        {
            GameMode.Points301 => 301,
            GameMode.Points401 => 401,
            GameMode.Points501 => 501,
            _ => 501
        };
    }

    public FinishMode FinishMode
    {
        get => _finishMode;
        set
        {
            _finishMode = value;
            OnPropertyChanged();
        }
    }

    public int LegsToWin
    {
        get => _legsToWin;
        set
        {
            _legsToWin = value;
            OnPropertyChanged();
        }
    }

    public bool PlayWithSets
    {
        get => _playWithSets;
        set
        {
            _playWithSets = value;
            OnPropertyChanged();
        }
    }

    public int SetsToWin
    {
        get => _setsToWin;
        set
        {
            _setsToWin = value;
            OnPropertyChanged();
        }
    }

    public int LegsPerSet
    {
        get => _legsPerSet;
        set
        {
            _legsPerSet = value;
            OnPropertyChanged();
        }
    }

    // Post-Group Phase Properties
    public PostGroupPhaseMode PostGroupPhaseMode
    {
        get => _postGroupPhaseMode;
        set
        {
            _postGroupPhaseMode = value;
            OnPropertyChanged();
        }
    }

    public int QualifyingPlayersPerGroup
    {
        get => _qualifyingPlayersPerGroup;
        set
        {
            _qualifyingPlayersPerGroup = Math.Max(1, value);
            OnPropertyChanged();
        }
    }

    public KnockoutMode KnockoutMode
    {
        get => _knockoutMode;
        set
        {
            _knockoutMode = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeGroupPhaseLosersBracket
    {
        get => _includeGroupPhaseLosersBracket;
        set
        {
            _includeGroupPhaseLosersBracket = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets round-specific rules for knockout rounds
    /// </summary>
    [JsonIgnore]
    public Dictionary<KnockoutRound, RoundRules> KnockoutRoundRules 
    { 
        get 
        {
            // Ensure the dictionary is initialized
            if (_knockoutRoundRules == null)
            {
                _knockoutRoundRules = new Dictionary<KnockoutRound, RoundRules>();
                InitializeDefaultKnockoutRules();
            }
            return _knockoutRoundRules;
        }
    }

    /// <summary>
    /// Gets the rules for a specific knockout round, falls back to default group phase rules if not found
    /// </summary>
    /// <param name="round">The knockout round</param>
    /// <returns>RoundRules for the specified round</returns>
    public RoundRules GetRulesForRound(KnockoutRound round)
    {
        if (_knockoutRoundRules.TryGetValue(round, out var rules))
        {
            return rules;
        }

        // Fallback to default group phase rules
        return new RoundRules(SetsToWin, LegsToWin, LegsPerSet);
    }

    /// <summary>
    /// Sets the rules for a specific knockout round
    /// </summary>
    /// <param name="round">The knockout round</param>
    /// <param name="setsToWin">Sets needed to win</param>
    /// <param name="legsToWin">Legs needed to win (if not playing with sets)</param>
    /// <param name="legsPerSet">Legs per set</param>
    public void SetRulesForRound(KnockoutRound round, int setsToWin, int legsToWin, int legsPerSet)
    {
        //System.Diagnostics.Debug.WriteLine($"SetRulesForRound: {round} -> Sets={setsToWin}, Legs={legsToWin}, LegsPerSet={legsPerSet}");
        
        if (_knockoutRoundRules.ContainsKey(round))
        {
            // Entferne Event-Handler vom alten RoundRules
            if (_knockoutRoundRules[round] != null)
            {
                _knockoutRoundRules[round].PropertyChanged -= OnRoundRulesPropertyChanged;
            }
            
            _knockoutRoundRules[round].SetsToWin = setsToWin;
            _knockoutRoundRules[round].LegsToWin = legsToWin;
            _knockoutRoundRules[round].LegsPerSet = legsPerSet;
            
            // Event-Handler für PropertyChanged hinzufügen
            _knockoutRoundRules[round].PropertyChanged += OnRoundRulesPropertyChanged;
        }
        else
        {
            var newRoundRules = new RoundRules(setsToWin, legsToWin, legsPerSet);
            newRoundRules.PropertyChanged += OnRoundRulesPropertyChanged;
            _knockoutRoundRules[round] = newRoundRules;
        }
        
        //System.Diagnostics.Debug.WriteLine($"SetRulesForRound: After setting -> Sets={_knockoutRoundRules[round].SetsToWin}, Legs={_knockoutRoundRules[round].LegsToWin}");
        
        OnPropertyChanged(nameof(KnockoutRoundRules));
        OnPropertyChanged(nameof(SerializableKnockoutRoundRules));
    }

    /// <summary>
    /// Event handler for when individual round rules change
    /// </summary>
    private void OnRoundRulesPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        //System.Diagnostics.Debug.WriteLine($"OnRoundRulesPropertyChanged: {e.PropertyName}");
        
        // Propagate property change to GameRules level
        OnPropertyChanged(nameof(KnockoutRoundRules));
        OnPropertyChanged(nameof(SerializableKnockoutRoundRules));
    }

    /// <summary>
    /// Initializes default knockout round rules based on current game rules
    /// </summary>
    private void InitializeDefaultKnockoutRules()
    {
        //System.Diagnostics.Debug.WriteLine("InitializeDefaultKnockoutRules: START");
        
        _knockoutRoundRules = new Dictionary<KnockoutRound, RoundRules>();

        // Winner Bracket rounds - Standardmäßig ohne Sets (SetsToWin = 0)
        _knockoutRoundRules[KnockoutRound.Best64] = new RoundRules(0, 2, 3);        // Early rounds: shorter
        _knockoutRoundRules[KnockoutRound.Best32] = new RoundRules(0, 2, 3);
        _knockoutRoundRules[KnockoutRound.Best16] = new RoundRules(0, 2, 3);        // Achtelfinale: longer
        _knockoutRoundRules[KnockoutRound.Quarterfinal] = new RoundRules(0, 2, 3);  // Viertelfinale
        _knockoutRoundRules[KnockoutRound.Semifinal] = new RoundRules(0, 4, 3);     // Halbfinale: longer
        _knockoutRoundRules[KnockoutRound.Final] = new RoundRules(0, 6, 3);         // Finale: longest
        _knockoutRoundRules[KnockoutRound.GrandFinal] = new RoundRules(0, 8, 3);    // Grand Final: sehr lang

        // Loser Bracket rounds (usually shorter than corresponding winner rounds)
        _knockoutRoundRules[KnockoutRound.LoserRound1] = new RoundRules(0, 2, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound2] = new RoundRules(0, 2, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound3] = new RoundRules(0, 2, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound4] = new RoundRules(0, 3, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound5] = new RoundRules(0, 3, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound6] = new RoundRules(0, 3, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound7] = new RoundRules(0, 3, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound8] = new RoundRules(0, 4, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound9] = new RoundRules(0, 4, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound10] = new RoundRules(0, 4, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound11] = new RoundRules(0, 4, 3);
        _knockoutRoundRules[KnockoutRound.LoserRound12] = new RoundRules(0, 4, 3);
        _knockoutRoundRules[KnockoutRound.LoserFinal] = new RoundRules(0, 6, 3);    // Loser Final: lang

        // Debug: Verify values after creation
        foreach (var rule in _knockoutRoundRules)
        {
            //System.Diagnostics.Debug.WriteLine($"  Default {rule.Key}: Sets={rule.Value.SetsToWin}, Legs={rule.Value.LegsToWin}, LegsPerSet={rule.Value.LegsPerSet}");
        }

        // Füge Event-Handler für alle RoundRules hinzu
        foreach (var roundRule in _knockoutRoundRules.Values)
        {
            roundRule.PropertyChanged += OnRoundRulesPropertyChanged;
        }
        
        //System.Diagnostics.Debug.WriteLine("InitializeDefaultKnockoutRules: END");
    }

    /// <summary>
    /// Resets all knockout round rules to default based on current base game rules
    /// </summary>
    public void ResetKnockoutRulesToDefault()
    {
        InitializeDefaultKnockoutRules();
        OnPropertyChanged(nameof(KnockoutRoundRules));
    }

    /// <summary>
    /// Serializable version of knockout round rules for JSON persistence
    /// </summary>
    [JsonProperty("KnockoutRoundRules")]
    public SerializableRoundRules SerializableKnockoutRoundRules
    {
        get 
        { 
            var result = new SerializableRoundRules(KnockoutRoundRules);
            //System.Diagnostics.Debug.WriteLine($"SerializableKnockoutRoundRules GET: {result.Rules.Count} rules");
            foreach (var rule in result.Rules)
            {
                var roundRules = rule.Value;
                System.Diagnostics.Debug.WriteLine($"  {rule.Key}: Sets={roundRules.SetsToWin}, Legs={roundRules.LegsToWin}, LegsPerSet={roundRules.LegsPerSet}");
            }
            return result;
        }
        set
        {
            //System.Diagnostics.Debug.WriteLine($"SerializableKnockoutRoundRules SET: {value?.Rules?.Count ?? 0} rules");
            
            if (value != null)
            {
                // Entferne alte Event-Handler
                if (_knockoutRoundRules != null)
                {
                    foreach (var roundRule in _knockoutRoundRules.Values)
                    {
                        if (roundRule != null)
                        {
                            roundRule.PropertyChanged -= OnRoundRulesPropertyChanged;
                        }
                    }
                }

                _knockoutRoundRules = value.ToDictionary();
                
                // Debug: Log loaded rules
                foreach (var rule in _knockoutRoundRules)
                {
                    //System.Diagnostics.Debug.WriteLine($"  Loaded {rule.Key}: Sets={rule.Value.SetsToWin}, Legs={rule.Value.LegsToWin}, LegsPerSet={rule.Value.LegsPerSet}");
                }
                
                // Füge Event-Handler für alle neuen RoundRules hinzu
                foreach (var roundRule in _knockoutRoundRules.Values)
                {
                    if (roundRule != null)
                    {
                        roundRule.PropertyChanged += OnRoundRulesPropertyChanged;
                    }
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine("SerializableKnockoutRoundRules SET: value is null, initializing defaults");
                _knockoutRoundRules = new Dictionary<KnockoutRound, RoundRules>();
                InitializeDefaultKnockoutRules();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        var mode = GameMode switch
        {
            GameMode.Points501 => "501",
            GameMode.Points401 => "401",
            GameMode.Points301 => "301",
            _ => "501"
        };

        var finish = FinishMode switch
        {
            FinishMode.SingleOut => "Single Out",
            FinishMode.DoubleOut => "Double Out",
            _ => "Double Out"
        };

        var baseRules = PlayWithSets 
            ? $"{mode} {finish}, First to {SetsToWin} Sets ({LegsPerSet} Legs per Set)"
            : $"{mode} {finish}, First to {LegsToWin} Legs";

        var postGroupInfo = PostGroupPhaseMode switch
        {
            PostGroupPhaseMode.RoundRobinFinals => $" + Finals: Top {QualifyingPlayersPerGroup} per Group (Round Robin)",
            PostGroupPhaseMode.KnockoutBracket => KnockoutMode == KnockoutMode.DoubleElimination 
                ? $" + Finals: Top {QualifyingPlayersPerGroup} per Group (Double Elimination)"
                : $" + Finals: Top {QualifyingPlayersPerGroup} per Group (Single Elimination)",
            _ => ""
        };

        return baseRules + postGroupInfo;
    }
}

/// <summary>
/// Serializable wrapper for round rules to ensure proper JSON serialization
/// </summary>
[JsonObject]
public class SerializableRoundRules
{
    [JsonProperty]
    public Dictionary<string, RoundRules> Rules { get; set; } = new Dictionary<string, RoundRules>();

    public SerializableRoundRules() { }

    public SerializableRoundRules(Dictionary<KnockoutRound, RoundRules> rules)
    {
        Rules = new Dictionary<string, RoundRules>();
        foreach (var kvp in rules)
        {
            Rules[kvp.Key.ToString()] = kvp.Value;
            //System.Diagnostics.Debug.WriteLine($"SerializableRoundRules Constructor: {kvp.Key} -> Sets={kvp.Value.SetsToWin}, Legs={kvp.Value.LegsToWin}, LegsPerSet={kvp.Value.LegsPerSet}");
        }
    }

    public Dictionary<KnockoutRound, RoundRules> ToDictionary()
    {
        var result = new Dictionary<KnockoutRound, RoundRules>();
        foreach (var kvp in Rules)
        {
            if (Enum.TryParse<KnockoutRound>(kvp.Key, out var round))
            {
                result[round] = kvp.Value;
                //System.Diagnostics.Debug.WriteLine($"SerializableRoundRules ToDictionary: {round} -> Sets={kvp.Value.SetsToWin}, Legs={kvp.Value.LegsToWin}, LegsPerSet={kvp.Value.LegsPerSet}");
            }
        }
        return result;
    }
}