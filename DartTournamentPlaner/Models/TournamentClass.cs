using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

public class TournamentClass : INotifyPropertyChanged
{
    private int _id;
    private string _name = "Platin";
    private GameRules _gameRules = new GameRules();
    private TournamentPhase? _currentPhase;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public GameRules GameRules
    {
        get => _gameRules;
        set
        {
            _gameRules = value;
            OnPropertyChanged();
        }
    }

    public TournamentPhase? CurrentPhase
    {
        get => _currentPhase;
        set
        {
            _currentPhase = value;
            OnPropertyChanged();
        }
    }

    // Legacy support - Groups from the current or group phase
    public ObservableCollection<Group> Groups 
    { 
        get 
        {
            System.Diagnostics.Debug.WriteLine($"TournamentClass.Groups getter called for {Name}");
            
            if (CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Current phase is GroupPhase, returning {CurrentPhase.Groups.Count} groups");
                return CurrentPhase.Groups;
            }
            
            // If we're in later phases, return the groups from the group phase
            var groupPhase = Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
            var count = groupPhase?.Groups?.Count ?? 0;
            System.Diagnostics.Debug.WriteLine($"  Not in GroupPhase, found GroupPhase with {count} groups");
            
            return groupPhase?.Groups ?? new ObservableCollection<Group>();
        }
    }

    // All tournament phases
    public ObservableCollection<TournamentPhase> Phases { get; set; } = new ObservableCollection<TournamentPhase>();

    public TournamentClass()
    {
        System.Diagnostics.Debug.WriteLine($"=== TournamentClass Constructor START ===");
        
        // Initialize with Group Phase
        var groupPhase = new TournamentPhase
        {
            Name = "Gruppenphase",
            PhaseType = TournamentPhaseType.GroupPhase,
            IsActive = true
        };
        
        System.Diagnostics.Debug.WriteLine($"TournamentClass Constructor: Created new TournamentPhase with {groupPhase.Groups.Count} groups");
        
        Phases.Add(groupPhase);
        CurrentPhase = groupPhase;
        
        System.Diagnostics.Debug.WriteLine($"TournamentClass Constructor: Set CurrentPhase, Groups count = {Groups.Count}");
        System.Diagnostics.Debug.WriteLine($"=== TournamentClass Constructor END ===");
    }

    public bool CanProceedToNextPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase START ===");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: TournamentClass = {Name}");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: CurrentPhase = {CurrentPhase?.PhaseType}");            
            var result = CurrentPhase?.CanProceedToNextPhase() ?? false;
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Result = {result}");
            System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase END ===");
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    public TournamentPhase? GetNextPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GetNextPhase START ===");
            
            if (CurrentPhase == null) 
            {
                System.Diagnostics.Debug.WriteLine($"GetNextPhase: CurrentPhase is null");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"GetNextPhase: Current phase = {CurrentPhase.PhaseType}");
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: PostGroupPhaseMode = {GameRules.PostGroupPhaseMode}");

            TournamentPhase? nextPhase = CurrentPhase.PhaseType switch
            {
                TournamentPhaseType.GroupPhase => GameRules.PostGroupPhaseMode switch
                {
                    PostGroupPhaseMode.RoundRobinFinals => CreateRoundRobinFinalsPhase(),
                    PostGroupPhaseMode.KnockoutBracket => CreateKnockoutPhase(),
                    _ => null
                },

                TournamentPhaseType.RoundRobinFinals => GameRules.PostGroupPhaseMode == PostGroupPhaseMode.KnockoutBracket 
                    ? CreateKnockoutPhase() 
                    : null,

                TournamentPhaseType.KnockoutPhase => null, // Tournament ends

                _ => null
            };
            
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: Next phase = {nextPhase?.PhaseType}");
            System.Diagnostics.Debug.WriteLine($"=== GetNextPhase END ===");
            
            return nextPhase;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public void AdvanceToNextPhase()
    {
        if (!CanProceedToNextPhase()) return;

        var nextPhase = GetNextPhase();
        if (nextPhase == null) return;

        // Mark current phase as completed
        if (CurrentPhase != null)
        {
            CurrentPhase.IsActive = false;
            CurrentPhase.IsCompleted = true;
        }

        // Add and activate next phase
        Phases.Add(nextPhase);
        CurrentPhase = nextPhase;
        nextPhase.IsActive = true;
    }

    private TournamentPhase CreateRoundRobinFinalsPhase()
    {
        var finalsPhase = new TournamentPhase
        {
            Name = "Finalrunde",
            PhaseType = TournamentPhaseType.RoundRobinFinals
        };

        // Get qualified players from group phase
        var groupPhase = Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        var qualifiedPlayers = groupPhase.GetQualifiedPlayers(GameRules.QualifyingPlayersPerGroup);

        // Create finals group
        var finalsGroup = new Group
        {
            Id = 999, // Special ID for finals
            Name = "Finalrunde",
            MatchesGenerated = false
        };

        foreach (var player in qualifiedPlayers)
        {
            finalsGroup.Players.Add(player);
        }

        finalsPhase.FinalsGroup = finalsGroup;
        finalsPhase.QualifiedPlayers = new ObservableCollection<Player>(qualifiedPlayers);

        return finalsPhase;
    }

    private TournamentPhase CreateKnockoutPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateKnockoutPhase START ===");
            
            var knockoutPhase = new TournamentPhase
            {
                Name = GameRules.KnockoutMode == KnockoutMode.DoubleElimination ? "KO-Phase (Double Elimination)" : "KO-Phase (Single Elimination)",
                PhaseType = TournamentPhaseType.KnockoutPhase
            };

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Created phase with name '{knockoutPhase.Name}'");

            // Get qualified players from previous phase
            List<Player> qualifiedPlayers;
            if (CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Getting players from RoundRobinFinals");
                qualifiedPlayers = CurrentPhase.GetQualifiedPlayers(int.MaxValue); // All players from finals
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Getting players from GroupPhase");
                var groupPhase = Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
                qualifiedPlayers = groupPhase.GetQualifiedPlayers(GameRules.QualifyingPlayersPerGroup);
            }

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Found {qualifiedPlayers.Count} qualified players");
            foreach (var player in qualifiedPlayers)
            {
                System.Diagnostics.Debug.WriteLine($"  - {player.Name}");
            }

            // WICHTIGE VALIDIERUNG: Prüfen ob genügend Spieler vorhanden
            if (qualifiedPlayers.Count <= 1)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: ERROR - Not enough players ({qualifiedPlayers.Count})");
                throw new InvalidOperationException($"Nicht genügend Spieler für K.O.-Phase: {qualifiedPlayers.Count}");
            }

            knockoutPhase.QualifiedPlayers = new ObservableCollection<Player>(qualifiedPlayers);

            // Generate bracket
            if (GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generating double elimination bracket");
                GenerateDoubleEliminationBracket(knockoutPhase, qualifiedPlayers);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generating single elimination bracket");
                GenerateSingleEliminationBracket(knockoutPhase, qualifiedPlayers);
            }

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generated {knockoutPhase.WinnerBracket.Count} winner bracket matches");
            System.Diagnostics.Debug.WriteLine($"=== CreateKnockoutPhase END ===");

            return knockoutPhase;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void GenerateSingleEliminationBracket(TournamentPhase phase, List<Player> players)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GenerateSingleEliminationBracket START ===");
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: {players.Count} players");
            
            var shuffledPlayers = players.OrderBy(x => Guid.NewGuid()).ToList(); // Random seeding
            var matches = new List<KnockoutMatch>();
            int matchId = 1;

            // Calculate rounds needed
            int playersCount = shuffledPlayers.Count;
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: playersCount = {playersCount}");
            
            int nextPowerOf2 = 1;
            while (nextPowerOf2 < playersCount)
            {
                nextPowerOf2 *= 2;
            }
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: nextPowerOf2 = {nextPowerOf2}");

            // Add byes if needed
            int byesNeeded = nextPowerOf2 - playersCount;
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: byesNeeded = {byesNeeded}");
            
            for (int i = 0; i < byesNeeded; i++)
            {
                shuffledPlayers.Add(null!); // null represents a bye
            }

            // Calculate total rounds needed for this tournament
            var totalRounds = (int)Math.Ceiling(Math.Log2(playersCount));
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: totalRounds = {totalRounds}");
            
            // Determine starting round based on player count and German terminology
            KnockoutRound startingRound = GetStartingRound(playersCount);
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: startingRound = {startingRound}");

            // Generate first round
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Generating first round with {shuffledPlayers.Count} participants");
            var currentRoundMatches = new List<KnockoutMatch>();
            for (int i = 0; i < shuffledPlayers.Count; i += 2)
            {
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = shuffledPlayers[i],
                    Player2 = i + 1 < shuffledPlayers.Count ? shuffledPlayers[i + 1] : null,
                    BracketType = BracketType.Winner,
                    Round = startingRound,
                    Position = i / 2
                };

                // Handle byes
                if (match.Player2 == null)
                {
                    match.Winner = match.Player1;
                    match.Status = MatchStatus.Finished;
                    System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: {match.Player1?.Name} gets bye");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}");
                }

                currentRoundMatches.Add(match);
                matches.Add(match);
            }
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Generated {currentRoundMatches.Count} first round matches");

            // Generate subsequent rounds
            var currentRound = startingRound;
            int roundCounter = 2;
            while (currentRoundMatches.Count > 1)
            {
                System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Generating round {roundCounter} with {currentRoundMatches.Count} current matches");
                
                var nextRoundMatches = new List<KnockoutMatch>();
                
                // Calculate next round BEFORE creating matches
                var nextRound = GetNextRound(currentRound, currentRoundMatches.Count);
                System.Diagnostics.Debug.WriteLine($"  Next round will be: {nextRound}");
                
                for (int i = 0; i < currentRoundMatches.Count; i += 2)
                {
                    var match = new KnockoutMatch
                    {
                        Id = matchId++,
                        SourceMatch1 = currentRoundMatches[i],
                        SourceMatch2 = i + 1 < currentRoundMatches.Count ? currentRoundMatches[i + 1] : null,
                        BracketType = BracketType.Winner,
                        Round = nextRound,
                        Position = i / 2,
                        Player1FromWinner = true,
                        Player2FromWinner = true
                    };

                    System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: Winner of {match.SourceMatch1?.Id} vs Winner of {match.SourceMatch2?.Id ?? 0}");
                    nextRoundMatches.Add(match);
                    matches.Add(match);
                }

                currentRoundMatches = nextRoundMatches;
                currentRound = nextRound;
                roundCounter++;
                
                System.Diagnostics.Debug.WriteLine($"  Generated {nextRoundMatches.Count} matches for round {roundCounter - 1}");
            }

            phase.WinnerBracket = new ObservableCollection<KnockoutMatch>(matches);
            
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Total matches generated: {matches.Count}");
            System.Diagnostics.Debug.WriteLine($"=== GenerateSingleEliminationBracket END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Determines the starting round based on player count and German tournament terminology
    /// </summary>
    private static KnockoutRound GetStartingRound(int playerCount)
    {
        // For 6 players: Quarterfinal (4 matches) → Semifinal (2 matches) → Final (1 match)
        // For 4 players: Semifinal (2 matches) → Final (1 match)
        // For 8 players: Quarterfinal (4 matches) → Semifinal (2 matches) → Final (1 match)
        
        // Calculate bracket size (next power of 2)
        int bracketSize = 1;
        while (bracketSize < playerCount)
        {
            bracketSize *= 2; // FIXED: War bracketSize *= 1 → Endlosschleife!
        }

        return bracketSize switch
        {
            64 => KnockoutRound.Best64,      // 64+ → "Beste 64"
            32 => KnockoutRound.Best32,         // 32 → "Beste 32" 
            16 => KnockoutRound.Best16,   // 16 → "Achtelfinale"
            8 => KnockoutRound.Quarterfinal,    // 8 → "Viertelfinale" (bei 5-8 Spielern)
            4 => KnockoutRound.Semifinal,       // 4 → "Halbfinale" (bei 3-4 Spielern)
            2 => KnockoutRound.Final,           // 2 → "Finale"
            _ => KnockoutRound.Final
        };
    }

    /// <summary>
    /// Determines the next round based on current round and remaining match count
    /// </summary>
    private static KnockoutRound GetNextRound(KnockoutRound currentRound, int currentMatchCount)
    {
        return currentRound switch
        {
            KnockoutRound.Best64 => currentMatchCount switch
            {
                16 => KnockoutRound.Best32,        // Beste 32 → Beste 16
                8 => KnockoutRound.Best16,         // Beste 16 → Achtelfinale
                4 => KnockoutRound.Quarterfinal,   // → Viertelfinale
                2 => KnockoutRound.Semifinal,      // → Halbfinale  
                1 => KnockoutRound.Final,          // → Finale
                _ => KnockoutRound.Best32
            },
            KnockoutRound.Best32 => currentMatchCount switch
            {
                8 => KnockoutRound.Best16,         // Beste 16 → Achtelfinale
                4 => KnockoutRound.Quarterfinal,   // → Viertelfinale
                2 => KnockoutRound.Semifinal,      // → Halbfinale  
                1 => KnockoutRound.Final,          // → Finale
                _ => KnockoutRound.Best16
            },
            KnockoutRound.Best16 => currentMatchCount switch
            {
                4 => KnockoutRound.Quarterfinal,   // → Viertelfinale
                2 => KnockoutRound.Semifinal,      // → Halbfinale  
                1 => KnockoutRound.Final,          // → Finale
                _ => KnockoutRound.Quarterfinal
            },
            KnockoutRound.Quarterfinal => currentMatchCount switch
            {
                2 => KnockoutRound.Semifinal,      // Viertelfinale → Halbfinale
                1 => KnockoutRound.Final,          // Viertelfinale → Finale (falls nur 2 Matches)
                _ => KnockoutRound.Semifinal
            },
            KnockoutRound.Semifinal => KnockoutRound.Final, // Halbfinale → Finale
            _ => KnockoutRound.Final
        };
    }

    private void GenerateDoubleEliminationBracket(TournamentPhase phase, List<Player> players)
    {
        // First generate single elimination winner bracket
        GenerateSingleEliminationBracket(phase, players);

        // Then generate loser bracket
        var loserMatches = new List<KnockoutMatch>();
        int matchId = phase.WinnerBracket.Count + 1;

        // TODO: Implement full double elimination loser bracket logic
        // This is quite complex and would need careful bracket structure

        phase.LoserBracket = new ObservableCollection<KnockoutMatch>(loserMatches);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{Name} - {GameRules}";
    }
}