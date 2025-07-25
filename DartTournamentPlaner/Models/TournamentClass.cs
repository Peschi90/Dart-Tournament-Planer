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

            // Calculate bracket structure
            int playersCount = shuffledPlayers.Count;
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: playersCount = {playersCount}");
            
            // Find next power of 2
            int nextPowerOf2 = 1;
            while (nextPowerOf2 < playersCount)
            {
                nextPowerOf2 *= 2;
            }
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: nextPowerOf2 = {nextPowerOf2}");

            // Calculate byes and actual matches in first round
            int byesNeeded = nextPowerOf2 - playersCount;
            int playersWithoutBye = playersCount - byesNeeded;
            int firstRoundMatches = playersWithoutBye / 2;
            
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: byesNeeded = {byesNeeded}");
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: playersWithoutBye = {playersWithoutBye}");
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: firstRoundMatches = {firstRoundMatches}");

            // Determine starting round based on final bracket size
            KnockoutRound startingRound = GetStartingRound(nextPowerOf2);
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: startingRound = {startingRound}");

            // Create first round matches
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Generating first round with {firstRoundMatches} actual matches");
            var currentRoundMatches = new List<KnockoutMatch>();
            
            // First, create matches with actual players (no byes)
            int playerIndex = 0;
            for (int i = 0; i < firstRoundMatches; i++)
            {
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = shuffledPlayers[playerIndex++],
                    Player2 = shuffledPlayers[playerIndex++],
                    BracketType = BracketType.Winner,
                    Round = startingRound,
                    Position = i
                };

                System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}");
                currentRoundMatches.Add(match);
                matches.Add(match);
            }

            // Then, create "bye matches" for remaining players
            for (int i = 0; i < byesNeeded; i++)
            {
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = shuffledPlayers[playerIndex++],
                    Player2 = null, // Bye
                    Winner = shuffledPlayers[playerIndex - 1], // Player with bye automatically wins
                    Status = MatchStatus.Finished,
                    BracketType = BracketType.Winner,
                    Round = startingRound,
                    Position = firstRoundMatches + i
                };

                System.Diagnostics.Debug.WriteLine($"  Match {byeMatch.Id}: {byeMatch.Player1?.Name} gets bye");
                currentRoundMatches.Add(byeMatch);
                matches.Add(byeMatch);
            }
            
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Generated {currentRoundMatches.Count} first round matches ({firstRoundMatches} actual + {byesNeeded} byes)");

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
            
            // WICHTIG: Nach der Generierung alle abgeschlossenen Matches (Bye-Matches) verarbeiten
            // und deren Gewinner automatisch in die nächsten Runden übertragen
            PropagateCompletedMatches(phase.WinnerBracket);
            
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

    private void GenerateDoubleEliminationBracket(TournamentPhase phase, List<Player> players)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GenerateDoubleEliminationBracket START ===");
            System.Diagnostics.Debug.WriteLine($"GenerateDoubleEliminationBracket: {players.Count} players");

            // First generate single elimination winner bracket
            GenerateSingleEliminationBracket(phase, players);

            // Generate the loser bracket
            GenerateLoserBracket(phase);

            // Connect brackets for grand final if needed
            if (phase.WinnerBracket.Any() && phase.LoserBracket.Any())
            {
                GenerateGrandFinal(phase);
            }

            // WICHTIG: Nach der kompletten Generierung alle Brackets propagieren
            // Das Winner Bracket wurde bereits in GenerateSingleEliminationBracket propagiert
            // Jetzt das Loser Bracket propagieren
            if (phase.LoserBracket.Any())
            {
                PropagateCompletedMatches(phase.LoserBracket);
            }

            System.Diagnostics.Debug.WriteLine($"GenerateDoubleEliminationBracket: Generated {phase.WinnerBracket.Count} winner matches and {phase.LoserBracket.Count} loser matches");
            System.Diagnostics.Debug.WriteLine($"=== GenerateDoubleEliminationBracket END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateDoubleEliminationBracket: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GenerateDoubleEliminationBracket: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// Propagates winners from completed matches (especially bye matches) to subsequent rounds
    /// </summary>
    private void PropagateCompletedMatches(ObservableCollection<KnockoutMatch> matches)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== PropagateCompletedMatches START ===");
            
            var allMatches = matches.ToList();
            var completedMatches = allMatches.Where(m => m.Status == MatchStatus.Finished && m.Winner != null).ToList();
            
            System.Diagnostics.Debug.WriteLine($"PropagateCompletedMatches: Found {completedMatches.Count} completed matches to propagate");
            
            foreach (var completedMatch in completedMatches)
            {
                System.Diagnostics.Debug.WriteLine($"PropagateCompletedMatches: Processing completed match {completedMatch.Id} (Winner: {completedMatch.Winner?.Name})");
                
                // Find matches that reference this completed match as a source
                var dependentMatches = allMatches.Where(m => 
                    m.SourceMatch1 == completedMatch || m.SourceMatch2 == completedMatch).ToList();
                
                foreach (var dependentMatch in dependentMatches)
                {
                    if (dependentMatch.SourceMatch1 == completedMatch && dependentMatch.Player1FromWinner)
                    {
                        dependentMatch.Player1 = completedMatch.Winner;
                        System.Diagnostics.Debug.WriteLine($"  Set Player1 of match {dependentMatch.Id} to {completedMatch.Winner?.Name}");
                    }
                    else if (dependentMatch.SourceMatch2 == completedMatch && dependentMatch.Player2FromWinner)
                    {
                        dependentMatch.Player2 = completedMatch.Winner;
                        System.Diagnostics.Debug.WriteLine($"  Set Player2 of match {dependentMatch.Id} to {completedMatch.Winner?.Name}");
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"=== PropagateCompletedMatches END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PropagateCompletedMatches: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"PropagateCompletedMatches: Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Determines the starting round based on bracket size and German tournament terminology
    /// </summary>
    private static KnockoutRound GetStartingRound(int bracketSize)
    {
        // bracketSize is already a power of 2 representing the full bracket size
        // For 11 players: bracketSize = 16 → "Beste 16" 
        // For 6 players: bracketSize = 8 → "Viertelfinale"
        // For 4 players: bracketSize = 4 → "Halbfinale"
        
        return bracketSize switch
        {
            128 => KnockoutRound.Best64,      // 128 → "Beste 64"
            64 => KnockoutRound.Best32,       // 64 → "Beste 32" 
            32 => KnockoutRound.Best16,       // 32 → "Beste 16"
            16 => KnockoutRound.Best16,       // 16 → "Beste 16" (Achtelfinale bei 9-16 Spielern)
            8 => KnockoutRound.Quarterfinal,  // 8 → "Viertelfinale" (bei 5-8 Spielern)
            4 => KnockoutRound.Semifinal,     // 4 → "Halbfinale" (bei 3-4 Spielern)
            2 => KnockoutRound.Final,         // 2 → "Finale"
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

    private void GenerateLoserBracket(TournamentPhase phase)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GenerateLoserBracket START ===");

            var loserMatches = new List<KnockoutMatch>();
            int matchId = phase.WinnerBracket.Count + 1;

            // Get winner bracket matches grouped by round
            var winnerMatchesByRound = phase.WinnerBracket
                .GroupBy(m => m.Round)
                .OrderBy(g => g.Key)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"GenerateLoserBracket: Winner bracket has {winnerMatchesByRound.Count} rounds");

            if (winnerMatchesByRound.Count < 2)
            {
                System.Diagnostics.Debug.WriteLine($"GenerateLoserBracket: Not enough winner rounds for loser bracket");
                phase.LoserBracket = new ObservableCollection<KnockoutMatch>();
                return;
            }

            // LOSER BRACKET STRUCTURE:
            // Potential LR0: Group phase losers (if IncludeGroupPhaseLosersBracket is enabled)
            // LR1: Losers from WR1 play each other
            // LR2: Winners from LR1 vs Losers from WR2
            // LR3: Winners from LR2 play each other  
            // LR4: Winners from LR3 vs Losers from WR3
            // ... and so on until loser bracket final

            var loserRoundEnums = new[]
            {
                KnockoutRound.LoserRound1, KnockoutRound.LoserRound2, KnockoutRound.LoserRound3,
                KnockoutRound.LoserRound4, KnockoutRound.LoserRound5, KnockoutRound.LoserRound6,
                KnockoutRound.LoserRound7, KnockoutRound.LoserRound8, KnockoutRound.LoserRound9,
                KnockoutRound.LoserRound10, KnockoutRound.LoserRound11, KnockoutRound.LoserRound12,
                KnockoutRound.LoserFinal
            };

            var loserRoundCounter = 0;
            var previousLoserMatches = new List<KnockoutMatch>();

            // Check if we should include group phase losers
            if (GameRules.IncludeGroupPhaseLosersBracket)
            {
                System.Diagnostics.Debug.WriteLine($"  Including group phase losers in loser bracket");
                
                // Get group phase losers (players who didn't qualify for knockout)
                var groupPhase = Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
                var qualifiedPlayers = groupPhase.GetQualifiedPlayers(GameRules.QualifyingPlayersPerGroup);
                var allGroupPlayers = groupPhase.Groups.SelectMany(g => g.Players).ToList();
                var groupPhaseLosers = allGroupPlayers.Where(p => !qualifiedPlayers.Contains(p)).ToList();
                
                System.Diagnostics.Debug.WriteLine($"  Found {groupPhaseLosers.Count} group phase losers");
                
                if (groupPhaseLosers.Count > 1)
                {
                    // Create initial loser bracket matches for group phase losers
                    var currentLoserRoundMatches = new List<KnockoutMatch>();
                    
                    // Pair up group phase losers
                    for (int i = 0; i < groupPhaseLosers.Count; i += 2)
                    {
                        if (loserRoundCounter >= loserRoundEnums.Length) break;
                        
                        var loserMatch = new KnockoutMatch
                        {
                            Id = matchId++,
                            BracketType = BracketType.Loser,
                            Round = loserRoundEnums[loserRoundCounter],
                            Position = i / 2,
                            Player1 = groupPhaseLosers[i],
                            Player2 = i + 1 < groupPhaseLosers.Count ? groupPhaseLosers[i + 1] : null,
                            Player1FromWinner = false, // Direct assignment from group phase
                            Player2FromWinner = false
                        };

                        // Handle bye if odd number of group phase losers
                        if (loserMatch.Player2 == null)
                        {
                            loserMatch.Winner = loserMatch.Player1;
                            loserMatch.Status = MatchStatus.Finished;
                            System.Diagnostics.Debug.WriteLine($"    LR{loserRoundCounter + 1} Match {loserMatch.Id}: {loserMatch.Player1?.Name} gets bye");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"    LR{loserRoundCounter + 1} Match {loserMatch.Id}: {loserMatch.Player1?.Name} vs {loserMatch.Player2?.Name}");
                        }

                        currentLoserRoundMatches.Add(loserMatch);
                        loserMatches.Add(loserMatch);
                    }
                    
                    previousLoserMatches = currentLoserRoundMatches;
                    loserRoundCounter++;
                }
            }

            // Process each winner bracket round (except the final)
            for (int wrRoundIndex = 0; wrRoundIndex < winnerMatchesByRound.Count - 1; wrRoundIndex++)
            {
                var winnerRound = winnerMatchesByRound[wrRoundIndex];
                var winnerMatches = winnerRound.ToList();

                System.Diagnostics.Debug.WriteLine($"GenerateLoserBracket: Processing Winner Round {wrRoundIndex + 1} with {winnerMatches.Count} matches");

                if (wrRoundIndex == 0 && !GameRules.IncludeGroupPhaseLosersBracket)
                {
                    // LR1: First eliminated players from WR1 play against each other (only if no group phase losers)
                    System.Diagnostics.Debug.WriteLine($"  Creating LR{loserRoundCounter + 1}: Losers from WR1 vs each other");
                    
                    var currentLoserRoundMatches = new List<KnockoutMatch>();
                    
                    // Pair up losers from first winner round
                    for (int i = 0; i < winnerMatches.Count; i += 2)
                    {
                        if (loserRoundCounter >= loserRoundEnums.Length) break;
                        
                        var loserMatch = new KnockoutMatch
                        {
                            Id = matchId++,
                            BracketType = BracketType.Loser,
                            Round = loserRoundEnums[loserRoundCounter],
                            Position = i / 2,
                            SourceMatch1 = winnerMatches[i],
                            SourceMatch2 = i + 1 < winnerMatches.Count ? winnerMatches[i + 1] : null,
                            Player1FromWinner = false, // Loser from source match
                            Player2FromWinner = false
                        };

                        currentLoserRoundMatches.Add(loserMatch);
                        loserMatches.Add(loserMatch);
                        
                        System.Diagnostics.Debug.WriteLine($"    LR{loserRoundCounter + 1} Match {loserMatch.Id}: Loser of W{winnerMatches[i].Id} vs Loser of W{winnerMatches[i + 1]?.Id ?? 0}");
                    }
                    
                    previousLoserMatches = currentLoserRoundMatches;
                    loserRoundCounter++;
                }
                else
                {
                    // Subsequent rounds: Two different patterns alternating
                    
                    // Pattern 1: Winners from previous loser round vs Losers from current winner round
                    if (loserRoundCounter < loserRoundEnums.Length)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Creating LR{loserRoundCounter + 1}: Winners from LR{loserRoundCounter} vs Losers from WR{wrRoundIndex + 1}");
                        
                        var currentLoserRoundMatches = new List<KnockoutMatch>();
                        
                        // Match winners from previous loser round with losers from current winner round
                        var previousLoserWinners = previousLoserMatches.ToList();
                        var currentWinnerLosers = winnerMatches.ToList();
                        
                        int maxMatches = Math.Max(previousLoserWinners.Count, currentWinnerLosers.Count);
                        
                        for (int i = 0; i < maxMatches && i < Math.Min(previousLoserWinners.Count, currentWinnerLosers.Count); i++)
                        {
                            var previousLoserMatch = previousLoserWinners[i];
                            var currentWinnerMatch = currentWinnerLosers[i];
                            
                            var loserMatch = new KnockoutMatch
                            {
                                Id = matchId++,
                                BracketType = BracketType.Loser,
                                Round = loserRoundEnums[loserRoundCounter],
                                Position = i,
                                SourceMatch1 = previousLoserMatch,  // Winner from previous loser round
                                SourceMatch2 = currentWinnerMatch, // Loser from current winner round
                                Player1FromWinner = true,  // Winner from loser match
                                Player2FromWinner = false  // Loser from winner match
                            };

                            currentLoserRoundMatches.Add(loserMatch);
                            loserMatches.Add(loserMatch);
                            
                            System.Diagnostics.Debug.WriteLine($"    LR{loserRoundCounter + 1} Match {loserMatch.Id}: Winner of L{previousLoserMatch.Id} vs Loser of W{currentWinnerMatch.Id}");
                        }
                        
                        previousLoserMatches = currentLoserRoundMatches;
                        loserRoundCounter++;
                    }
                    
                    // Pattern 2: Winners from current loser round play each other (consolidation)
                    if (previousLoserMatches.Count > 1 && loserRoundCounter < loserRoundEnums.Length)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Creating LR{loserRoundCounter + 1}: Consolidation round - Winners from LR{loserRoundCounter} vs each other");
                        
                        var consolidationMatches = new List<KnockoutMatch>();
                        
                        for (int i = 0; i < previousLoserMatches.Count; i += 2)
                        {
                            var loserMatch = new KnockoutMatch
                            {
                                Id = matchId++,
                                BracketType = BracketType.Loser,
                                Round = loserRoundEnums[loserRoundCounter],
                                Position = i / 2,
                                SourceMatch1 = previousLoserMatches[i],
                                SourceMatch2 = i + 1 < previousLoserMatches.Count ? previousLoserMatches[i + 1] : null,
                                Player1FromWinner = true, // Winner from previous loser match
                                Player2FromWinner = true  // Winner from previous loser match
                            };

                            consolidationMatches.Add(loserMatch);
                            loserMatches.Add(loserMatch);
                            
                            System.Diagnostics.Debug.WriteLine($"    LR{loserRoundCounter + 1} Match {loserMatch.Id}: Winner of L{previousLoserMatches[i].Id} vs Winner of L{previousLoserMatches[i + 1]?.Id ?? 0}");
                        }
                        
                        previousLoserMatches = consolidationMatches;
                        loserRoundCounter++;
                    }
                }
            }

            // Create loser bracket final if we have more than one match remaining
            if (previousLoserMatches.Count > 1 && loserRoundCounter < loserRoundEnums.Length)
            {
                System.Diagnostics.Debug.WriteLine($"  Creating Loser Final: Final consolidation");
                
                for (int i = 0; i < previousLoserMatches.Count; i += 2)
                {
                    var loserFinalMatch = new KnockoutMatch
                    {
                        Id = matchId++,
                        BracketType = BracketType.Loser,
                        Round = KnockoutRound.LoserFinal,
                        Position = i / 2,
                        SourceMatch1 = previousLoserMatches[i],
                        SourceMatch2 = i + 1 < previousLoserMatches.Count ? previousLoserMatches[i + 1] : null,
                        Player1FromWinner = true,
                        Player2FromWinner = true
                    };

                    loserMatches.Add(loserFinalMatch);
                    System.Diagnostics.Debug.WriteLine($"    Loser Final Match {loserFinalMatch.Id}: Winner of L{previousLoserMatches[i].Id} vs Winner of L{previousLoserMatches[i + 1]?.Id ?? 0}");
                }
            }

            phase.LoserBracket = new ObservableCollection<KnockoutMatch>(loserMatches);
            
            System.Diagnostics.Debug.WriteLine($"GenerateLoserBracket: Generated {loserMatches.Count} loser bracket matches across {loserRoundCounter} rounds");
            System.Diagnostics.Debug.WriteLine($"=== GenerateLoserBracket END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateLoserBracket: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GenerateLoserBracket: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void GenerateGrandFinal(TournamentPhase phase)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GenerateGrandFinal START ===");

            // Get the winner bracket final match
            var winnerFinal = phase.WinnerBracket.LastOrDefault();
            var loserFinal = phase.LoserBracket.LastOrDefault();

            if (winnerFinal == null || loserFinal == null)
            {
                System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Missing winner or loser final");
                return;
            }

            int matchId = Math.Max(
                phase.WinnerBracket.Max(m => m.Id),
                phase.LoserBracket.Max(m => m.Id)
            ) + 1;

            // Create grand final match
            var grandFinal = new KnockoutMatch
            {
                Id = matchId,
                BracketType = BracketType.Winner, // Grand final is technically part of winner bracket
                Round = KnockoutRound.GrandFinal,
                Position = 0,
                SourceMatch1 = winnerFinal,  // Winner bracket champion
                SourceMatch2 = loserFinal,   // Loser bracket champion
                Player1FromWinner = true,
                Player2FromWinner = true
            };

            // Add grand final to winner bracket (it's the ultimate winner match)
            var winnerMatches = phase.WinnerBracket.ToList();
            winnerMatches.Add(grandFinal);
            phase.WinnerBracket = new ObservableCollection<KnockoutMatch>(winnerMatches);

            System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Created Grand Final match {grandFinal.Id}");
            System.Diagnostics.Debug.WriteLine($"=== GenerateGrandFinal END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Stack trace: {ex.StackTrace}");
            throw;
        }
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