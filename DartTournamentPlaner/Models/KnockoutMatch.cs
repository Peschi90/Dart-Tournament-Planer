using System.ComponentModel;
using System.Runtime.CompilerServices;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Models;

public enum BracketType
{
    Winner,
    Loser
}

public enum KnockoutRound
{
    // Winner Bracket Rounds
    Best64,
    Best32,
    Best16,
    Quarterfinal,
    Semifinal,
    Final,
    GrandFinal,
    
    // Loser Bracket Rounds (numbered system)
    LoserRound1,
    LoserRound2,
    LoserRound3,
    LoserRound4,
    LoserRound5,
    LoserRound6,
    LoserRound7,
    LoserRound8,
    LoserRound9,
    LoserRound10,
    LoserRound11,
    LoserRound12,
    LoserFinal
}

public class KnockoutMatch : INotifyPropertyChanged
{
    private int _id;
    private Player? _player1;
    private Player? _player2;
    private Player? _winner;
    private Player? _loser;
    private int _player1Sets = 0;
    private int _player2Sets = 0;
    private int _player1Legs = 0;
    private int _player2Legs = 0;
    private MatchStatus _status = MatchStatus.NotStarted;
    private BracketType _bracketType = BracketType.Winner;
    private KnockoutRound _round = KnockoutRound.Best64;
    private int _position = 0;
    private string _notes = string.Empty;
    private DateTime? _startTime;
    private DateTime? _endTime;

    // For determining where players come from
    private KnockoutMatch? _sourceMatch1; // First source match
    private KnockoutMatch? _sourceMatch2; // Second source match
    private bool _player1FromWinner = true; // True if Player1 comes from winner of sourceMatch1
    private bool _player2FromWinner = true; // True if Player2 comes from winner of sourceMatch2

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged();
        }
    }

    public Player? Player1
    {
        get => _player1;
        set
        {
            _player1 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public Player? Player2
    {
        get => _player2;
        set
        {
            _player2 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public Player? Winner
    {
        get => _winner;
        set
        {
            _winner = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WinnerDisplay));
        }
    }

    public Player? Loser
    {
        get => _loser;
        set
        {
            _loser = value;
            OnPropertyChanged();
        }
    }

    public int Player1Sets
    {
        get => _player1Sets;
        set
        {
            _player1Sets = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    public int Player2Sets
    {
        get => _player2Sets;
        set
        {
            _player2Sets = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    public int Player1Legs
    {
        get => _player1Legs;
        set
        {
            _player1Legs = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    public int Player2Legs
    {
        get => _player2Legs;
        set
        {
            _player2Legs = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    public MatchStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusDisplay));
        }
    }

    public BracketType BracketType
    {
        get => _bracketType;
        set
        {
            _bracketType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BracketDisplay));
        }
    }

    public KnockoutRound Round
    {
        get => _round;
        set
        {
            _round = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RoundDisplay));
        }
    }

    public int Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPropertyChanged();
        }
    }

    public string Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged();
        }
    }

    public DateTime? StartTime
    {
        get => _startTime;
        set
        {
            _startTime = value;
            OnPropertyChanged();
        }
    }

    public DateTime? EndTime
    {
        get => _endTime;
        set
        {
            _endTime = value;
            OnPropertyChanged();
        }
    }

    // Source matches for bracket progression
    public KnockoutMatch? SourceMatch1
    {
        get => _sourceMatch1;
        set
        {
            _sourceMatch1 = value;
            OnPropertyChanged();
        }
    }

    public KnockoutMatch? SourceMatch2
    {
        get => _sourceMatch2;
        set
        {
            _sourceMatch2 = value;
            OnPropertyChanged();
        }
    }

    public bool Player1FromWinner
    {
        get => _player1FromWinner;
        set
        {
            _player1FromWinner = value;
            OnPropertyChanged();
        }
    }

    public bool Player2FromWinner
    {
        get => _player2FromWinner;
        set
        {
            _player2FromWinner = value;
            OnPropertyChanged();
        }
    }

    // Display Properties
    public string DisplayName => $"{Player1?.Name ?? "TBD"} vs {Player2?.Name ?? "TBD"}";

    public string ScoreDisplay
    {
        get
        {
            if (Status == MatchStatus.NotStarted) return "-:-";
            
            return Player1Sets > 0 || Player2Sets > 0 
                ? $"{Player1Sets}:{Player2Sets} ({Player1Legs}:{Player2Legs})"
                : $"{Player1Legs}:{Player2Legs}";
        }
    }

    public string StatusDisplay => Status switch
    {
        MatchStatus.NotStarted => "Nicht gestartet",
        MatchStatus.InProgress => "Läuft",
        MatchStatus.Finished => "Beendet",
        _ => "Unbekannt"
    };

    public string WinnerDisplay => Winner?.Name ?? "";

    public string BracketDisplay => BracketType switch
    {
        BracketType.Winner => "Winner Bracket",
        BracketType.Loser => "Loser Bracket",
        _ => ""
    };

    public string RoundDisplay => Round switch
    {
        KnockoutRound.Best64 => "Beste 64",
        KnockoutRound.Best32 => "Beste 32",
        KnockoutRound.Best16 => "Beste 16",
        KnockoutRound.Quarterfinal => "Viertelfinale",
        KnockoutRound.Semifinal => "Halbfinale",
        KnockoutRound.Final => "Finale",
        KnockoutRound.GrandFinal => "Grand Final",
        KnockoutRound.LoserRound1 => "LR1",
        KnockoutRound.LoserRound2 => "LR2",
        KnockoutRound.LoserRound3 => "LR3",
        KnockoutRound.LoserRound4 => "LR4",
        KnockoutRound.LoserRound5 => "LR5",
        KnockoutRound.LoserRound6 => "LR6",
        KnockoutRound.LoserRound7 => "LR7",
        KnockoutRound.LoserRound8 => "LR8",
        KnockoutRound.LoserRound9 => "LR9",
        KnockoutRound.LoserRound10 => "LR10",
        KnockoutRound.LoserRound11 => "LR11",
        KnockoutRound.LoserRound12 => "LR12",
        KnockoutRound.LoserFinal => "LF",
        _ => ""
    };

    /// <summary>
    /// Gets the dynamic round display name based on tournament context and bracket type
    /// </summary>
    /// <param name="totalParticipants">Total number of participants</param>
    /// <param name="localizationService">Localization service</param>
    /// <returns>Dynamic round name</returns>
    public string GetDynamicRoundDisplay(int totalParticipants, LocalizationService? localizationService = null)
    {
        // Umfassende Eingabevalidierung mit zusätzlichen Checks
        if (totalParticipants <= 0)
        {
            System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Invalid totalParticipants = {totalParticipants}");
            return "Keine Teilnehmer definiert";
        }

        if (totalParticipants == 1)
        {
            System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Only one participant = {totalParticipants}");
            return "Nur ein Teilnehmer - Keine K.O.-Phase möglich";
        }

        // Zusätzliche Sicherheitsüberprüfung für extrem große Zahlen
        if (totalParticipants > 1024)
        {
            System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Too many participants = {totalParticipants}");
            return "Zu viele Teilnehmer";
        }

        try
        {
            // Handle Loser Bracket rounds separately
            if (BracketType == BracketType.Loser)
            {
                return Round switch
                {
                    KnockoutRound.LoserRound1 => "Loser Runde 1",
                    KnockoutRound.LoserRound2 => "Loser Runde 2",
                    KnockoutRound.LoserRound3 => "Loser Runde 3",
                    KnockoutRound.LoserRound4 => "Loser Runde 4",
                    KnockoutRound.LoserRound5 => "Loser Runde 5",
                    KnockoutRound.LoserRound6 => "Loser Runde 6",
                    KnockoutRound.LoserRound7 => "Loser Runde 7",
                    KnockoutRound.LoserRound8 => "Loser Runde 8",
                    KnockoutRound.LoserRound9 => "Loser Runde 9",
                    KnockoutRound.LoserRound10 => "Loser Runde 10",
                    KnockoutRound.LoserRound11 => "Loser Runde 11",
                    KnockoutRound.LoserRound12 => "Loser Runde 12",
                    KnockoutRound.LoserFinal => "Loser Finale",
                    _ => RoundDisplay
                };
            }

            // Handle Winner Bracket rounds with dynamic naming
            int bracketSize = 1;
            int iterations = 0;
            
            // Sicherheitscheck gegen Endlosschleife
            while (bracketSize < totalParticipants && iterations < 20)
            {
                bracketSize *= 2;
                iterations++;
            }
            
            // Fallback wenn die Schleife zu lange läuft
            if (iterations >= 20)
            {
                System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Infinite loop prevented for totalParticipants = {totalParticipants}");
                return "Fehler: Berechnung fehlgeschlagen";
            }

            // Special handling for Grand Final
            if (Round == KnockoutRound.GrandFinal)
            {
                return localizationService?.GetString("GrandFinal") ?? "Grand Final";
            }

            // Calculate which "logical round" this enum represents in the context of this tournament
            int logicalRound = Round switch
            {
                KnockoutRound.Best64 => GetBest64LogicalRound(bracketSize),
                KnockoutRound.Best32 => GetBest32LogicalRound(bracketSize),
                KnockoutRound.Best16 => GetBest16LogicalRound(bracketSize),
                KnockoutRound.Quarterfinal => GetQuarterfinalLogicalRound(bracketSize),
                KnockoutRound.Semifinal => GetSemifinalLogicalRound(bracketSize),
                KnockoutRound.Final => GetFinalLogicalRound(bracketSize),
                _ => 1
            };

            return GetKnockoutRoundName(bracketSize, logicalRound, localizationService);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Exception = {ex.Message}");
            return "Fehler bei Rundenberechnung";
        }
    }
    private static int GetBest64LogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 5); // Best64 is 4rd last round
    }
    private static int GetBest32LogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 4); // Best32 is 4rd last round
    }
    private static int GetBest16LogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 3); // Best16 is 4rd last round
    }
    private static int GetQuarterfinalLogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 2); // Quarterfinal is 3rd last round
    }
    
    private static int GetSemifinalLogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 1); // Semifinal is 2nd last round
    }
    
    private static int GetFinalLogicalRound(int bracketSize)
    {
        return (int)Math.Log2(bracketSize); // Final is last round
    }

    public void SetResult(int player1Sets, int player2Sets, int player1Legs, int player2Legs)
    {
        Player1Sets = player1Sets;
        Player2Sets = player2Sets;
        Player1Legs = player1Legs;
        Player2Legs = player2Legs;

        // Determine winner and loser
        if (player1Sets > player2Sets || (player1Sets == player2Sets && player1Legs > player2Legs))
        {
            Winner = Player1;
            Loser = Player2;
        }
        else if (player2Sets > player1Sets || (player1Sets == player2Sets && player2Legs > player1Legs))
        {
            Winner = Player2;
            Loser = Player1;
        }

        Status = MatchStatus.Finished;
        EndTime = DateTime.Now;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets the correct German knockout round name based on total participants and current round
    /// </summary>
    /// <param name="totalParticipants">Total number of participants in knockout phase</param>
    /// <param name="currentRound">Current round number (1-based)</param>
    /// <param name="localizationService">Localization service for translations</param>
    /// <returns>Correct round name in current language</returns>
    public static string GetKnockoutRoundName(int totalParticipants, int currentRound, LocalizationService? localizationService = null)
    {
        // Eingabevalidierung hinzufügen
        if (totalParticipants <= 0)
        {
            return "Fehler: Keine Teilnehmer";
        }

        if (currentRound <= 0)
        {
            return "Fehler: Ungültige Runde";
        }

        // Calculate total rounds needed
        var totalRounds = (int)Math.Ceiling(Math.Log2(totalParticipants));
        
        // Validierung der Rundenzahl
        if (currentRound > totalRounds)
        {
            return "Fehler: Runde außerhalb des Turniers";
        }

        // Calculate remaining players at start of current round
        var playersInRound = totalParticipants / (int)Math.Pow(2, currentRound - 1);

        // Determine round name based on players in round
        string roundKey = playersInRound switch
        {
            64 => "Best64",       // Beste 64
            32 => "Best32",       // Beste 32
            16 => "Best16",         // Beste 16
            8 => "Best8",         // Viertelfinale  
            4 => "Best4",         // Halbfinale
            2 => "Final",         // Finale
            _ => "LastOfRound"    // Runde X
        };

        // Special case: if it's the final match
        if (currentRound == totalRounds)
        {
            roundKey = "Final";
        }

        // Get translated name
        if (localizationService != null)
        {
            if (roundKey == "LastOfRound")
            {
                return localizationService.GetString(roundKey, currentRound);
            }
            return localizationService.GetString(roundKey);
        }

        // Fallback to German names
        return roundKey switch
        {
            "Best64" => "Beste 64",
            "Best32" => "Beste 32",
            "Best16" => "Beste 16",
            "Best8" => "Viertelfinale",
            "Best4" => "Halbfinale",
            "Final" => "Finale",
            _ => $"Runde {currentRound}"
        };
    }

    /// <summary>
    /// Überprüft, ob die K.O.-Phase mit der angegebenen Teilnehmeranzahl gestartet werden kann
    /// </summary>
    /// <param name="totalParticipants">Anzahl der Teilnehmer</param>
    /// <returns>True wenn K.O.-Phase möglich ist, sonst false</returns>
    public static bool CanStartKnockoutPhase(int totalParticipants)
    {
        return totalParticipants > 1;
    }

    /// <summary>
    /// Gibt eine Fehlermeldung zurück, falls die K.O.-Phase nicht gestartet werden kann
    /// </summary>
    /// <param name="totalParticipants">Anzahl der Teilnehmer</param>
    /// <returns>Fehlermeldung oder null wenn alles in Ordnung ist</returns>
    public static string? ValidateKnockoutPhaseStart(int totalParticipants)
    {
        if (totalParticipants <= 0)
            return "Keine Teilnehmer für K.O.-Phase qualifiziert";
        
        if (totalParticipants == 1)
            return "Nur ein Teilnehmer qualifiziert - K.O.-Phase nicht möglich";
        
        return null; // Alles in Ordnung
    }

    /// <summary>
    /// Updates loser bracket matches with eliminated players from winner bracket
    /// This should be called when a winner bracket match is completed
    /// </summary>
    /// <param name="completedWinnerMatch">The completed winner bracket match</param>
    /// <param name="loserBracket">The loser bracket matches</param>
    public static void UpdateLoserBracketFromWinnerMatch(KnockoutMatch completedWinnerMatch, IEnumerable<KnockoutMatch> loserBracket)
    {
        if (completedWinnerMatch.BracketType != BracketType.Winner || completedWinnerMatch.Loser == null)
            return;

        // Find loser bracket matches that should receive this eliminated player
        var targetLoserMatches = loserBracket
            .Where(lm => lm.SourceMatch1 == completedWinnerMatch || lm.SourceMatch2 == completedWinnerMatch)
            .ToList();

        foreach (var loserMatch in targetLoserMatches)
        {
            if (loserMatch.SourceMatch1 == completedWinnerMatch && !loserMatch.Player1FromWinner)
            {
                loserMatch.Player1 = completedWinnerMatch.Loser;
            }
            else if (loserMatch.SourceMatch2 == completedWinnerMatch && !loserMatch.Player2FromWinner)
            {
                loserMatch.Player2 = completedWinnerMatch.Loser;
            }
        }
    }

    /// <summary>
    /// Updates next round matches with winners from completed matches (both brackets)
    /// </summary>
    /// <param name="completedMatch">The completed match</param>
    /// <param name="allMatches">All matches in the tournament</param>
    public static void UpdateNextRoundFromCompletedMatch(KnockoutMatch completedMatch, IEnumerable<KnockoutMatch> allMatches)
    {
        if (completedMatch.Winner == null) return;

        var nextRoundMatches = allMatches
            .Where(m => m.SourceMatch1 == completedMatch || m.SourceMatch2 == completedMatch)
            .ToList();

        foreach (var nextMatch in nextRoundMatches)
        {
            if (nextMatch.SourceMatch1 == completedMatch && nextMatch.Player1FromWinner)
            {
                nextMatch.Player1 = completedMatch.Winner;
            }
            else if (nextMatch.SourceMatch2 == completedMatch && nextMatch.Player2FromWinner)
            {
                nextMatch.Player2 = completedMatch.Winner;
            }
        }
    }
}

