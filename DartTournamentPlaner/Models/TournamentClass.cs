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
                    Status = MatchStatus.Bye,
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
            
            // Propagiere nur initiale Bye-Matches
            PropagateInitialByeMatches(phase.WinnerBracket, null);
            
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

            // Generate winner bracket
            var winnerBracketMatches = GenerateWinnerBracket(players, 1);
            phase.WinnerBracket = new ObservableCollection<KnockoutMatch>(winnerBracketMatches);

            // Generate loser bracket
            var loserBracketMatches = GenerateLoserBracket(winnerBracketMatches, players);
            phase.LoserBracket = new ObservableCollection<KnockoutMatch>(loserBracketMatches);

            // Generate grand final
            if (winnerBracketMatches.Any() && loserBracketMatches.Any())
            {
                GenerateGrandFinal(phase);
            }

            // Propagate initial bye matches
            PropagateInitialByeMatches(phase.WinnerBracket, phase.LoserBracket);
            PropagateInitialByeMatches(phase.LoserBracket, null);

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

    private List<KnockoutMatch> GenerateWinnerBracket(List<Player> players, int startId)
    {
        var matches = new List<KnockoutMatch>();
        var shuffledPlayers = players.OrderBy(x => Guid.NewGuid()).ToList();
        int matchId = startId;

        // Calculate bracket size
        int playersCount = shuffledPlayers.Count;
        int nextPowerOf2 = 1;
        while (nextPowerOf2 < playersCount) nextPowerOf2 *= 2;

        int byesNeeded = nextPowerOf2 - playersCount;
        int firstRoundMatches = (playersCount - byesNeeded) / 2;

        KnockoutRound startingRound = GetStartingRound(nextPowerOf2);
        var currentRoundMatches = new List<KnockoutMatch>();

        int playerIndex = 0;

        // Create first round actual matches
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
            currentRoundMatches.Add(match);
            matches.Add(match);
        }

        // Create bye matches
        for (int i = 0; i < byesNeeded; i++)
        {
            var byeMatch = new KnockoutMatch
            {
                Id = matchId++,
                Player1 = shuffledPlayers[playerIndex++],
                Player2 = null,
                Winner = shuffledPlayers[playerIndex - 1],
                Status = MatchStatus.Bye,
                BracketType = BracketType.Winner,
                Round = startingRound,
                Position = firstRoundMatches + i
            };
            currentRoundMatches.Add(byeMatch);
            matches.Add(byeMatch);
        }

        // Generate subsequent rounds
        var currentRound = startingRound;
        while (currentRoundMatches.Count > 1)
        {
            var nextRoundMatches = new List<KnockoutMatch>();
            var nextRound = GetNextRound(currentRound, currentRoundMatches.Count);

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
                nextRoundMatches.Add(match);
                matches.Add(match);
            }

            currentRoundMatches = nextRoundMatches;
            currentRound = nextRound;
        }

        return matches;
    }

    private List<KnockoutMatch> GenerateLoserBracket(List<KnockoutMatch> winnerMatches, List<Player> allPlayers)
    {
        var loserMatches = new List<KnockoutMatch>();
        int matchId = winnerMatches.Max(m => m.Id) + 1;

        // Get group phase losers if enabled
        var groupPhaseLosers = new List<Player>();
        if (GameRules.IncludeGroupPhaseLosersBracket)
        {
            var groupPhase = Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
            var qualifiedPlayers = groupPhase.GetQualifiedPlayers(GameRules.QualifyingPlayersPerGroup);
            var allGroupPlayers = groupPhase.Groups.SelectMany(g => g.Players).ToList();
            groupPhaseLosers = allGroupPlayers.Where(p => !qualifiedPlayers.Contains(p)).ToList();
        }

        // Start with group phase losers
        List<KnockoutMatch> currentRound = new List<KnockoutMatch>();
        KnockoutRound currentLoserRound = KnockoutRound.LoserRound1;

        if (groupPhaseLosers.Any())
        {
            int position = 0;
            for (int i = 0; i < groupPhaseLosers.Count - 1; i += 2)
            {
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = groupPhaseLosers[i],
                    Player2 = groupPhaseLosers[i + 1],
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++
                };
                currentRound.Add(match);
                loserMatches.Add(match);
            }

            // Handle odd number with bye
            if (groupPhaseLosers.Count % 2 == 1)
            {
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = groupPhaseLosers.Last(),
                    Player2 = null,
                    Winner = groupPhaseLosers.Last(),
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++
                };
                currentRound.Add(byeMatch);
                loserMatches.Add(byeMatch);
            }
        }

        // Integrate winner bracket losers
        var winnerRounds = winnerMatches.GroupBy(m => m.Round).OrderBy(g => g.Key).ToList();
        
        foreach (var winnerRound in winnerRounds.Take(winnerRounds.Count - 1)) // Skip final
        {
            currentLoserRound = GetNextLoserRound(currentLoserRound);
            var nextRound = new List<KnockoutMatch>();
            int position = 0;

            var loserProducingMatches = winnerRound.Where(m => m.Status != MatchStatus.Bye).ToList();
            var allParticipants = new List<object>();
            allParticipants.AddRange(currentRound.Cast<object>());
            allParticipants.AddRange(loserProducingMatches.Cast<object>());

            for (int i = 0; i < allParticipants.Count - 1; i += 2)
            {
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++
                };

                SetLoserBracketParticipant(match, allParticipants[i], true);
                SetLoserBracketParticipant(match, allParticipants[i + 1], false);

                nextRound.Add(match);
                loserMatches.Add(match);
            }

            // Handle odd number
            if (allParticipants.Count % 2 == 1)
            {
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    Player2 = null,
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++
                };

                SetLoserBracketParticipant(byeMatch, allParticipants.Last(), true);
                if (allParticipants.Last() is Player player)
                {
                    byeMatch.Winner = player;
                }

                nextRound.Add(byeMatch);
                loserMatches.Add(byeMatch);
            }

            currentRound = nextRound;
        }

        // Continue loser bracket until final
        while (currentRound.Count > 1)
        {
            currentLoserRound = GetNextLoserRound(currentLoserRound);
            var nextRound = new List<KnockoutMatch>();
            int position = 0;

            for (int i = 0; i < currentRound.Count - 1; i += 2)
            {
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    SourceMatch1 = currentRound[i],
                    SourceMatch2 = currentRound[i + 1],
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    Player1FromWinner = true,
                    Player2FromWinner = true
                };
                nextRound.Add(match);
                loserMatches.Add(match);
            }

            if (currentRound.Count % 2 == 1)
            {
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    SourceMatch1 = currentRound.Last(),
                    SourceMatch2 = null,
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    Player1FromWinner = true
                };
                nextRound.Add(byeMatch);
                loserMatches.Add(byeMatch);
            }

            currentRound = nextRound;
        }

        return loserMatches;
    }

    private void SetLoserBracketParticipant(KnockoutMatch loserMatch, object participant, bool isPlayer1)
    {
        if (participant is Player player)
        {
            if (isPlayer1)
                loserMatch.Player1 = player;
            else
                loserMatch.Player2 = player;
        }
        else if (participant is KnockoutMatch sourceMatch)
        {
            if (isPlayer1)
            {
                loserMatch.SourceMatch1 = sourceMatch;
                loserMatch.Player1FromWinner = sourceMatch.BracketType != BracketType.Winner;
            }
            else
            {
                loserMatch.SourceMatch2 = sourceMatch;
                loserMatch.Player2FromWinner = sourceMatch.BracketType != BracketType.Winner;
            }
        }
    }

    private void GenerateGrandFinal(TournamentPhase phase)
    {
        var winnerFinal = phase.WinnerBracket.LastOrDefault(m => m.Round != KnockoutRound.GrandFinal);
        var loserFinal = phase.LoserBracket.LastOrDefault();

        if (winnerFinal == null || loserFinal == null) return;

        int matchId = Math.Max(
            phase.WinnerBracket.Max(m => m.Id),
            phase.LoserBracket.Max(m => m.Id)
        ) + 1;

        var grandFinal = new KnockoutMatch
        {
            Id = matchId,
            BracketType = BracketType.Winner,
            Round = KnockoutRound.GrandFinal,
            Position = 0,
            SourceMatch1 = winnerFinal,
            SourceMatch2 = loserFinal,
            Player1FromWinner = true,
            Player2FromWinner = true
        };

        var winnerMatches = phase.WinnerBracket.ToList();
        winnerMatches.Add(grandFinal);
        phase.WinnerBracket = new ObservableCollection<KnockoutMatch>(winnerMatches);
    }

    private KnockoutRound GetNextLoserRound(KnockoutRound currentRound)
    {
        return currentRound switch
        {
            KnockoutRound.LoserRound1 => KnockoutRound.LoserRound2,
            KnockoutRound.LoserRound2 => KnockoutRound.LoserRound3,
            KnockoutRound.LoserRound3 => KnockoutRound.LoserRound4,
            KnockoutRound.LoserRound4 => KnockoutRound.LoserRound5,
            KnockoutRound.LoserRound5 => KnockoutRound.LoserRound6,
            KnockoutRound.LoserRound6 => KnockoutRound.LoserRound7,
            KnockoutRound.LoserRound7 => KnockoutRound.LoserRound8,
            KnockoutRound.LoserRound8 => KnockoutRound.LoserRound9,
            KnockoutRound.LoserRound9 => KnockoutRound.LoserRound10,
            KnockoutRound.LoserRound10 => KnockoutRound.LoserRound11,
            KnockoutRound.LoserRound11 => KnockoutRound.LoserRound12,
            KnockoutRound.LoserRound12 => KnockoutRound.LoserFinal,
            _ => KnockoutRound.LoserFinal
        };
    }

    #region Helper Methods

    private static int GetNextPowerOfTwo(int n)
    {
        if (n <= 0) return 1;
        int power = 1;
        while (power < n) power *= 2;
        return power;
    }

    private static KnockoutRound GetStartingRound(int bracketSize)
    {
        return bracketSize switch
        {
            <= 2 => KnockoutRound.Final,
            <= 4 => KnockoutRound.Semifinal,
            <= 8 => KnockoutRound.Quarterfinal,
            <= 16 => KnockoutRound.Best16,
            <= 32 => KnockoutRound.Best32,
            <= 64 => KnockoutRound.Best64,
            _ => KnockoutRound.Best64
        };
    }

    private static KnockoutRound GetNextRound(KnockoutRound currentRound, int currentMatchCount)
    {
        return currentMatchCount switch
        {
            <= 2 => KnockoutRound.Final,
            <= 4 => KnockoutRound.Semifinal,
            <= 8 => KnockoutRound.Quarterfinal,
            <= 16 => KnockoutRound.Best16,
            <= 32 => KnockoutRound.Best32,
            <= 64 => KnockoutRound.Best64,
            _ => currentRound switch
            {
                KnockoutRound.Best64 => KnockoutRound.Best32,
                KnockoutRound.Best32 => KnockoutRound.Best16,
                KnockoutRound.Best16 => KnockoutRound.Quarterfinal,
                KnockoutRound.Quarterfinal => KnockoutRound.Semifinal,
                KnockoutRound.Semifinal => KnockoutRound.Final,
                _ => KnockoutRound.Final
            }
        };
    }

    #endregion

    #region Propagation Methods - VOLLSTÄNDIGE FREILOS-LÖSUNG

    /// <summary>
    /// HAUPTMETHODE: Propagiert initiale Bye-Matches und aktiviert automatische Freilos-Erkennung
    /// </summary>
    private void PropagateInitialByeMatches(ObservableCollection<KnockoutMatch> bracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        System.Diagnostics.Debug.WriteLine($"=== PropagateInitialByeMatches START ===");
        
        var byeMatches = bracket.Where(m => m.Status == MatchStatus.Bye && m.Winner != null).ToList();
        System.Diagnostics.Debug.WriteLine($"  Found {byeMatches.Count} initial bye matches to propagate");

        foreach (var byeMatch in byeMatches)
        {
            System.Diagnostics.Debug.WriteLine($"  Propagating initial bye match {byeMatch.Id} (winner: {byeMatch.Winner?.Name})");
            PropagateMatchResultWithAutomaticByes(byeMatch, bracket, otherBracket);
        }

        System.Diagnostics.Debug.WriteLine($"=== PropagateInitialByeMatches END ===");
    }

    /// <summary>
    /// KERNMETHODE: Propagiert Match-Ergebnisse und überprüft automatisch auf neue Freilose
    /// </summary>
    private void PropagateMatchResultWithAutomaticByes(KnockoutMatch completedMatch, ObservableCollection<KnockoutMatch> sameBracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        if (completedMatch.Winner == null) return;

        System.Diagnostics.Debug.WriteLine($"    Propagating match {completedMatch.Id} (winner: {completedMatch.Winner.Name})");

        // 1. Propagiere Gewinner in das gleiche Bracket
        var sameBracketDependents = sameBracket.Where(m => 
            (m.SourceMatch1?.Id == completedMatch.Id && m.Player1FromWinner) ||
            (m.SourceMatch2?.Id == completedMatch.Id && m.Player2FromWinner)).ToList();

        foreach (var dependent in sameBracketDependents)
        {
            if (dependent.SourceMatch1?.Id == completedMatch.Id && dependent.Player1FromWinner)
            {
                dependent.Player1 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player1 in match {dependent.Id}");
            }
            
            if (dependent.SourceMatch2?.Id == completedMatch.Id && dependent.Player2FromWinner)
            {
                dependent.Player2 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player2 in match {dependent.Id}");
            }
        }

        // 2. Propagiere Verlierer ins andere Bracket (Winner ? Loser)
        if (otherBracket != null && completedMatch.BracketType == BracketType.Winner)
        {
            var loser = GetMatchLoser(completedMatch);
            if (loser != null)
            {
                System.Diagnostics.Debug.WriteLine($"      WB Match {completedMatch.Id}: Loser {loser.Name} goes to LB");
                
                var crossBracketDependents = otherBracket.Where(m => 
                    (m.SourceMatch1?.Id == completedMatch.Id && !m.Player1FromWinner) ||
                    (m.SourceMatch2?.Id == completedMatch.Id && !m.Player2FromWinner)).ToList();

                foreach (var dependent in crossBracketDependents)
                {
                    if (dependent.SourceMatch1?.Id == completedMatch.Id && !dependent.Player1FromWinner)
                    {
                        dependent.Player1 = loser;
                        System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player1 in LB match {dependent.Id}");
                    }
                    
                    if (dependent.SourceMatch2?.Id == completedMatch.Id && !dependent.Player2FromWinner)
                    {
                        dependent.Player2 = loser;
                        System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player2 in LB match {dependent.Id}");
                    }
                }
            }
        }

        // 3. KRITISCH: Prüfe nach der Propagation auf neue automatische Freilose
        CheckAndHandleAutomaticByes(sameBracket, otherBracket);
        if (otherBracket != null)
        {
            CheckAndHandleAutomaticByes(otherBracket, sameBracket);
        }
    }

    /// <summary>
    /// Direkte Match-Propagation ohne weitere Bye-Checks (verhindert Rekursion)
    /// </summary>
    private void PropagateMatchResultDirectly(KnockoutMatch completedMatch, ObservableCollection<KnockoutMatch> sameBracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        if (completedMatch.Winner == null) return;

        // Propagiere nur die direkten Abhängigkeiten
        var sameBracketDependents = sameBracket.Where(m => 
            (m.SourceMatch1?.Id == completedMatch.Id && m.Player1FromWinner) ||
            (m.SourceMatch2?.Id == completedMatch.Id && m.Player2FromWinner)).ToList();

        foreach (var dependent in sameBracketDependents)
        {
            if (dependent.SourceMatch1?.Id == completedMatch.Id && dependent.Player1FromWinner)
            {
                dependent.Player1 = completedMatch.Winner;
            }
            
            if (dependent.SourceMatch2?.Id == completedMatch.Id && dependent.Player2FromWinner)
            {
                dependent.Player2 = completedMatch.Winner;
            }
        }

        // Propagiere Verlierer ins andere Bracket
        if (otherBracket != null && completedMatch.BracketType == BracketType.Winner)
        {
            var loser = GetMatchLoser(completedMatch);
            if (loser != null)
            {
                var crossBracketDependents = otherBracket.Where(m => 
                    (m.SourceMatch1?.Id == completedMatch.Id && !m.Player1FromWinner) ||
                    (m.SourceMatch2?.Id == completedMatch.Id && !m.Player2FromWinner)).ToList();

                foreach (var dependent in crossBracketDependents)
                {
                    if (dependent.SourceMatch1?.Id == completedMatch.Id && !dependent.Player1FromWinner)
                    {
                        dependent.Player1 = loser;
                    }
                    
                    if (dependent.SourceMatch2?.Id == completedMatch.Id && !dependent.Player2FromWinner)
                    {
                        dependent.Player2 = loser;
                    }
                }
            }
        }
    }

    /// <summary>
    /// VERBESSERTE LÖSUNG: Überprüft ALLE Arten von automatischen Freilosen
    /// WICHTIG: Nur für Freilos-Szenarien, nicht für normale Match-Ergebnisse
    /// </summary>
    private void CheckAndHandleAutomaticByes(ObservableCollection<KnockoutMatch> bracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        System.Diagnostics.Debug.WriteLine($"      === ENHANCED: CheckAndHandleAutomaticByes START ===");
        
        bool foundNewByes;
        int iterations = 0;
        const int maxIterations = 15; // Erhöht für komplexere Szenarien

        do
        {
            foundNewByes = false;
            iterations++;
            
            System.Diagnostics.Debug.WriteLine($"        Iteration {iterations}: Checking for automatic byes...");

            // FALL 1: Neue Freilose durch Spieler-Hinzufügung (aber nicht bei bereits beendeten Matches)
            var newByeMatches = bracket.Where(m => 
                m.Status == MatchStatus.NotStarted && 
                ShouldBeAutomaticBye(m))
                .ToList();

            // FALL 2: Ursprünglich Bye-markierte Matches mit neuen Spielern (aber keine finished matches)
            var convertedMatches = bracket.Where(m => 
                m.Status == MatchStatus.Bye && 
                m.Winner == null && 
                ShouldConvertByeToAutomatic(m))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"        Found {newByeMatches.Count} new byes, {convertedMatches.Count} converted byes");

            // Behandle neue automatische Freilose
            foreach (var match in newByeMatches)
            {
                var automaticWinner = DetermineAutomaticWinner(match);
                
                if (automaticWinner != null)
                {
                    System.Diagnostics.Debug.WriteLine($"        NEW AUTO BYE: Match {match.Id} - {automaticWinner.Name} advances automatically");
                    
                    match.Status = MatchStatus.Bye;
                    match.Winner = automaticWinner;
                    foundNewByes = true;
                    
                    // Verwende direkte Propagation ohne weitere Bye-Checks
                    PropagateMatchResultDirectly(match, bracket, otherBracket);
                }
            }

            // Behandle konvertierte Bye-Matches
            foreach (var match in convertedMatches)
            {
                var automaticWinner = DetermineAutomaticWinner(match);
                
                if (automaticWinner != null)
                {
                    System.Diagnostics.Debug.WriteLine($"        CONVERTED BYE: Match {match.Id} - {automaticWinner.Name} now gets automatic bye");
                    
                    match.Winner = automaticWinner; // Status bleibt Bye
                    foundNewByes = true;
                    
                    // Verwende direkte Propagation ohne weitere Bye-Checks
                    PropagateMatchResultDirectly(match, bracket, otherBracket);
                }
            }
            
        } while (foundNewByes && iterations < maxIterations);

        if (iterations >= maxIterations)
        {
            System.Diagnostics.Debug.WriteLine($"        WARNING: Reached maximum iterations ({maxIterations}) in enhanced bye check");
        }
        
        System.Diagnostics.Debug.WriteLine($"      === ENHANCED: CheckAndHandleAutomaticByes END ===");
    }

    /// <summary>
    /// NEU: Bestimmt ob ein Match automatisch als Bye behandelt werden sollte
    /// WICHTIG: Nur für NotStarted Matches, nicht für bereits fertige Matches
    /// </summary>
    private bool ShouldBeAutomaticBye(KnockoutMatch match)
    {
        // SICHERHEITSCHECK: Keine Automatic-Byes für bereits beendete oder laufende Matches
        if (match.Status != MatchStatus.NotStarted)
        {
            System.Diagnostics.Debug.WriteLine($"        ShouldBeAutomaticBye: Match {match.Id} not NotStarted, skipping");
            return false;
        }

        // Hilfsfunktion: Prüft ob ein Player gültig ist (nicht null und nicht "TBD")
        bool IsValidPlayer(Player? player)
        {
            return player != null && 
                   !string.IsNullOrEmpty(player.Name) && 
                   !player.Name.Equals("TBD", StringComparison.OrdinalIgnoreCase) &&
                   !player.Name.Equals("To Be Determined", StringComparison.OrdinalIgnoreCase);
        }

        System.Diagnostics.Debug.WriteLine($"        ShouldBeAutomaticBye: Match {match.Id}");
        System.Diagnostics.Debug.WriteLine($"          Player1: {match.Player1?.Name ?? "null"}, Valid: {IsValidPlayer(match.Player1)}");
        System.Diagnostics.Debug.WriteLine($"          Player2: {match.Player2?.Name ?? "null"}, Valid: {IsValidPlayer(match.Player2)}");
        System.Diagnostics.Debug.WriteLine($"          SourceMatch1: {match.SourceMatch1?.Id ?? 0}, SourceMatch2: {match.SourceMatch2?.Id ?? 0}");

        // Fall 1: Ein gültiger Spieler vorhanden, kein zweiter erwartet
        if (IsValidPlayer(match.Player1) && match.Player2 == null && match.SourceMatch2 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player1, no Player2 expected");
            return true;
        }
            
        // Fall 2: Ein gültiger Spieler vorhanden, kein erster erwartet  
        if (match.Player1 == null && IsValidPlayer(match.Player2) && match.SourceMatch1 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player2, no Player1 expected");
            return true;
        }

        // Fall 3: Ein gültiger Spieler + ein TBD Spieler, aber keine wartenden Matches
        if (IsValidPlayer(match.Player1) && match.Player2 != null && !IsValidPlayer(match.Player2) && match.SourceMatch2 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player1, Player2 is TBD, no SourceMatch2");
            return true;
        }

        if (match.Player1 != null && !IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2) && match.SourceMatch1 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player2, Player1 is TBD, no SourceMatch1");
            return true;
        }
            
        System.Diagnostics.Debug.WriteLine($"          -> Should NOT be bye");
        return false;
    }

    /// <summary>
    /// NEU: Bestimmt ob ein ursprüngliches Bye-Match zu einem automatischen Bye konvertiert werden sollte
    /// </summary>
    private bool ShouldConvertByeToAutomatic(KnockoutMatch match)
    {
        // Bereits als Bye markiert, aber ohne Gewinner und jetzt mit Spieler(n)
        return (match.Player1 != null || match.Player2 != null);
    }

    /// <summary>
    /// VERBESSERT: Bestimmt den automatischen Gewinner eines Matches (filtert TBD aus)
    /// </summary>
    private Player? DetermineAutomaticWinner(KnockoutMatch match)
    {
        // Hilfsfunktion: Prüft ob ein Player gültig ist (nicht null und nicht "TBD")
        bool IsValidPlayer(Player? player)
        {
            return player != null && 
                   !string.IsNullOrEmpty(player.Name) && 
                   !player.Name.Equals("TBD", StringComparison.OrdinalIgnoreCase) &&
                   !player.Name.Equals("To Be Determined", StringComparison.OrdinalIgnoreCase);
        }

        System.Diagnostics.Debug.WriteLine($"        DetermineAutomaticWinner: Match {match.Id}");
        System.Diagnostics.Debug.WriteLine($"          Player1: {match.Player1?.Name ?? "null"}");
        System.Diagnostics.Debug.WriteLine($"          Player2: {match.Player2?.Name ?? "null"}");

        // Fall 1: Nur Player1 ist ein gültiger Spieler
        if (IsValidPlayer(match.Player1) && !IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Player1 {match.Player1.Name} wins automatically (Player2 is TBD/null)");
            return match.Player1;
        }
            
        // Fall 2: Nur Player2 ist ein gültiger Spieler
        if (!IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Player2 {match.Player2.Name} wins automatically (Player1 is TBD/null)");
            return match.Player2;
        }
            
        // Fall 3: Beide Spieler sind gültig - kein automatisches Bye
        if (IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Both players valid, no automatic bye");
            return null;
        }

        // Fall 4: Beide Spieler sind TBD/null - kein automatisches Bye
        System.Diagnostics.Debug.WriteLine($"          -> Both players TBD/null, no automatic bye");
        return null;
    }

    /// <summary>
    /// ÖFFENTLICHE METHODE: Manueller Refresh aller Freilose
    /// </summary>
    public void RefreshAllByeMatches()
    {
        System.Diagnostics.Debug.WriteLine($"=== PUBLIC RefreshAllByeMatches START ===");
        
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return;
        }

        var winnerBracket = CurrentPhase.WinnerBracket;
        var loserBracket = CurrentPhase.LoserBracket;
        
        CheckAndHandleAutomaticByes(winnerBracket, loserBracket);
        if (loserBracket.Any())
        {
            CheckAndHandleAutomaticByes(loserBracket, winnerBracket);
        }
        
        // Trigger UI refresh nach Bye-Check
        TriggerUIRefresh();
        
        System.Diagnostics.Debug.WriteLine($"=== PUBLIC RefreshAllByeMatches END ===");
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE für manuelle Freilos-Vergabe
    /// </summary>
    /// <param name="match">Das Match, in dem ein Freilos vergeben werden soll</param>
    /// <param name="byeWinner">Der Spieler, der das Freilos bekommen soll (null = automatisch bestimmen)</param>
    /// <returns>True wenn erfolgreich, False wenn nicht möglich</returns>
    public bool GiveManualBye(KnockoutMatch match, Player? byeWinner = null)
    {
        System.Diagnostics.Debug.WriteLine($"=== GiveManualBye START for match {match.Id} ===");
        
        try
        {
            if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in knockout phase - cannot give bye");
                return false;
            }

            // Validation: Match must not be finished
            if (match.Status == MatchStatus.Finished)
            {
                System.Diagnostics.Debug.WriteLine($"  Match {match.Id} already finished - cannot give bye");
                return false;
            }

            // Determine bye winner
            Player? winner = byeWinner;
            
            if (winner == null)
            {
                // Automatic determination
                if (match.Player1 != null && match.Player2 == null)
                    winner = match.Player1;
                else if (match.Player1 == null && match.Player2 != null)
                    winner = match.Player2;
                else if (match.Player1 != null && match.Player2 != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Both players present in match {match.Id} - manual bye winner required");
                    return false; // Beide Spieler vorhanden - explizite Auswahl nötig
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  No players in match {match.Id} - cannot give bye");
                    return false; // Keine Spieler vorhanden
                }
            }
            else
            {
                // Validate that the specified winner is actually in the match
                if (match.Player1?.Id != winner.Id && match.Player2?.Id != winner.Id)
                {
                    System.Diagnostics.Debug.WriteLine($"  Specified winner {winner.Name} not in match {match.Id}");
                    return false;
                }
            }

            if (winner == null)
            {
                System.Diagnostics.Debug.WriteLine($"  Could not determine winner for bye in match {match.Id}");
                return false;
            }

            // Apply the bye
            match.Status = MatchStatus.Bye;
            match.Winner = winner;
            match.Loser = null; // Freilos hat keinen Verlierer
            match.Player1Sets = 0;
            match.Player2Sets = 0;
            match.Player1Legs = 0;
            match.Player2Legs = 0;
            match.EndTime = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"  Manual bye given to {winner.Name} in match {match.Id}");

            // Propagate the bye result
            var winnerBracket = CurrentPhase.WinnerBracket;
            var loserBracket = CurrentPhase.LoserBracket;

            if (match.BracketType == BracketType.Winner)
            {
                PropagateMatchResultWithAutomaticByes(match, winnerBracket, loserBracket);
            }
            else
            {
                PropagateMatchResultWithAutomaticByes(match, loserBracket, null);
            }

            System.Diagnostics.Debug.WriteLine($"=== GiveManualBye SUCCESS for match {match.Id} ===");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GiveManualBye ERROR: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE zum Rückgängigmachen eines Freiloses
    /// </summary>
    /// <param name="match">Das Match, dessen Freilos rückgängig gemacht werden soll</param>
    /// <returns>True wenn erfolgreich, False wenn nicht möglich</returns>
    public bool UndoBye(KnockoutMatch match)
    {
        System.Diagnostics.Debug.WriteLine($"=== UndoBye START for match {match.Id} ===");
        
        try
        {
            if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in knockout phase - cannot undo bye");
                return false;
            }

            // Validation: Match must be a bye
            if (match.Status != MatchStatus.Bye)
            {
                System.Diagnostics.Debug.WriteLine($"  Match {match.Id} is not a bye - cannot undo");
                return false;
            }

            // Check if there are dependent matches that would be affected
            var allMatches = CurrentPhase.WinnerBracket.Concat(CurrentPhase.LoserBracket).ToList();
            var dependentMatches = allMatches.Where(m => 
                (m.SourceMatch1?.Id == match.Id) || (m.SourceMatch2?.Id == match.Id)).ToList();

            bool hasProgressedDependents = dependentMatches.Any(m => 
                m.Status == MatchStatus.Finished || 
                (m.Status == MatchStatus.Bye && m.Winner != null));

            if (hasProgressedDependents)
            {
                System.Diagnostics.Debug.WriteLine($"  Cannot undo bye for match {match.Id} - dependent matches have progressed");
                return false;
            }

            // Reset the match
            match.Status = MatchStatus.NotStarted;
            match.Winner = null;
            match.Loser = null;
            match.Player1Sets = 0;
            match.Player2Sets = 0;
            match.Player1Legs = 0;
            match.Player2Legs = 0;
            match.EndTime = null;

            // Clear dependent matches
            foreach (var dependent in dependentMatches)
            {
                if (dependent.SourceMatch1?.Id == match.Id)
                {
                    dependent.Player1 = null;
                }
                if (dependent.SourceMatch2?.Id == match.Id)
                {
                    dependent.Player2 = null;
                }
            }

            System.Diagnostics.Debug.WriteLine($"  Bye undone for match {match.Id}");
            
            // Re-check for automatic byes after the change
            RefreshAllByeMatches();

            System.Diagnostics.Debug.WriteLine($"=== UndoBye SUCCESS for match {match.Id} ===");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UndoBye ERROR: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE zur Validierung von Freilos-Operationen
    /// </summary>
    /// <param name="match">Das zu validierende Match</param>
    /// <returns>Validierungsresultat</returns>
    public ByeValidationResult ValidateByeOperation(KnockoutMatch match)
    {
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return new ByeValidationResult(false, "Nicht in K.O.-Phase", false, false);
        }

        bool canGiveBye = false;
        bool canUndoBye = false;
        string message = "";

        if (match.Status == MatchStatus.Bye)
        {
            // Check if bye can be undone
            var allMatches = CurrentPhase.WinnerBracket.Concat(CurrentPhase.LoserBracket).ToList();
            var dependentMatches = allMatches.Where(m => 
                (m.SourceMatch1?.Id == match.Id) || (m.SourceMatch2?.Id == match.Id)).ToList();

            bool hasProgressedDependents = dependentMatches.Any(m => 
                m.Status == MatchStatus.Finished || 
                (m.Status == MatchStatus.Bye && m.Winner != null));

            canUndoBye = !hasProgressedDependents;
            message = canUndoBye ? "Freilos kann rückgängig gemacht werden" : "Freilos kann nicht rückgängig gemacht werden - nachfolgende Matches bereits gespielt";
        }
        else if (match.Status == MatchStatus.NotStarted)
        {
            // Check if bye can be given
            bool hasPlayers = match.Player1 != null || match.Player2 != null;
            canGiveBye = hasPlayers;
            message = hasPlayers ? "Freilos kann vergeben werden" : "Keine Spieler im Match - Freilos nicht möglich";
        }
        else if (match.Status == MatchStatus.Finished)
        {
            message = "Match bereits beendet - keine Freilos-Operationen möglich";
        }
        else
        {
            message = "Match läuft - keine Freilos-Operationen möglich";
        }

        return new ByeValidationResult(true, message, canGiveBye, canUndoBye);
    }

    /// <summary>
    /// BEISPIEL: Status-Überprüfung für UI-Buttons
    /// </summary>
    /// <param name="matchId">ID des Matches</param>
    /// <returns>UI-Status für Freilos-Buttons</returns>
    public MatchByeUIStatus GetMatchByeUIStatus(int matchId)
    {
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return new MatchByeUIStatus(false, false, "Nicht in K.O.-Phase");
        }

        var allMatches = CurrentPhase.WinnerBracket.Concat(CurrentPhase.LoserBracket).ToList();
        var match = allMatches.FirstOrDefault(m => m.Id == matchId);
        
        if (match == null)
        {
            return new MatchByeUIStatus(false, false, "Match nicht gefunden");
        }

        var validation = ValidateByeOperation(match);
        
        return new MatchByeUIStatus(
            validation.CanGiveBye,
            validation.CanUndoBye,
            validation.Message
        );
    }

    private Player? GetMatchLoser(KnockoutMatch match)
    {
        if (match.Winner == null || match.Status == MatchStatus.Bye) 
            return null;

        if (match.Player1 != null && match.Player2 != null)
        {
            return match.Winner.Id == match.Player1.Id ? match.Player2 : match.Player1;
        }

        return null;
    }

    #endregion

    #region Öffentliche Beispiel-Methoden für UI-Integration

    /// <summary>
    /// BEISPIEL: Wie die UI die neuen Freilos-Funktionen verwenden kann
    /// </summary>
    /// <param name="matchId">ID des Matches</param>
    /// <param name="winnerName">Name des Spielers, der das Freilos bekommen soll (optional)</param>
    public void HandleByeAction(int matchId, string? winnerName = null)
    {
        System.Diagnostics.Debug.WriteLine($"=== EXAMPLE: HandleByeAction for match {matchId} ===");
        
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            System.Diagnostics.Debug.WriteLine("  Not in knockout phase");
            return;
        }

        // Finde das Match
        var allMatches = CurrentPhase.WinnerBracket.Concat(CurrentPhase.LoserBracket).ToList();
        var match = allMatches.FirstOrDefault(m => m.Id == matchId);
        
        if (match == null)
        {
            System.Diagnostics.Debug.WriteLine($"  Match {matchId} not found");
            return;
        }

        // Validiere die Operation
        var validation = ValidateByeOperation(match);
        System.Diagnostics.Debug.WriteLine($"  Validation: {validation.Message}");
        
        if (!validation.IsValid)
        {
            System.Diagnostics.Debug.WriteLine($"  Validation failed");
            return;
        }

        // Führe entsprechende Aktion aus
        if (match.Status == MatchStatus.Bye && validation.CanUndoBye)
        {
            bool success = UndoBye(match);
            System.Diagnostics.Debug.WriteLine($"  Undo bye result: {success}");
        }
        else if (match.Status == MatchStatus.NotStarted && validation.CanGiveBye)
        {
            Player? winner = null;
            
            if (!string.IsNullOrEmpty(winnerName))
            {
                winner = match.Player1?.Name == winnerName ? match.Player1 : 
                        match.Player2?.Name == winnerName ? match.Player2 : null;
            }
            
            bool success = GiveManualBye(match, winner);
            System.Diagnostics.Debug.WriteLine($"  Give bye result: {success}");
        }
        
        System.Diagnostics.Debug.WriteLine($"=== EXAMPLE: HandleByeAction END ===");
    }

    /// <summary>
    /// BEISPIEL: Automatische Überprüfung nach Spieler-Hinzufügung
    /// Sollte aufgerufen werden, wenn ein Spieler in ein Match gesetzt wird
    /// </summary>
    /// <param name="matchId">ID des Matches, das geändert wurde</param>
    public void OnPlayerAddedToMatch(int matchId)
    {
        System.Diagnostics.Debug.WriteLine($"=== EXAMPLE: OnPlayerAddedToMatch for match {matchId} ===");
        
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return;
        }

        // Trigger eine vollständige Überprüfung aller Freilose
        RefreshAllByeMatches();
        
        System.Diagnostics.Debug.WriteLine($"=== EXAMPLE: OnPlayerAddedToMatch END ===");
    }

    #endregion

    /// <summary>
    /// NEU: Löst ein UI-Refresh-Event aus, um ViewModels zu aktualisieren
    /// </summary>
    public void TriggerUIRefresh()
    {
        System.Diagnostics.Debug.WriteLine($"TriggerUIRefresh: Firing UIRefreshRequested event");
        UIRefreshRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// NEU: Event für UI-Refreshs nach automatischen Freilos-Änderungen
    /// </summary>
    public event EventHandler? UIRefreshRequested;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{Name} - {GameRules}";
    }

    /// <summary>
    /// ÖFFENTLICHE METHODE: Für normale Match-Ergebnis-Progression (nicht für Freilose)
    /// Diese Methode sollte verwendet werden, wenn ein Match über die UI beendet wird
    /// </summary>
    /// <param name="completedMatch">Das beendete Match</param>
    /// <returns>True wenn erfolgreich</returns>
    public bool ProcessMatchResult(KnockoutMatch completedMatch)
    {
        System.Diagnostics.Debug.WriteLine($"=== ProcessMatchResult START for match {completedMatch.Id} ===");
        
        try
        {
            if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in knockout phase - cannot process match result");
                return false;
            }

            if (completedMatch.Winner == null || completedMatch.Status != MatchStatus.Finished)
            {
                System.Diagnostics.Debug.WriteLine($"  Match {completedMatch.Id} not finished or no winner - cannot process");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"  Processing match {completedMatch.Id} - Winner: {completedMatch.Winner.Name}");

            // Setze den Verlierer
            if (completedMatch.Player1 != null && completedMatch.Player2 != null)
            {
                completedMatch.Loser = completedMatch.Winner.Id == completedMatch.Player1.Id ? completedMatch.Player2 : completedMatch.Player1;
            }

            // Use the standard progression logic first
            var allMatches = CurrentPhase.WinnerBracket.Concat(CurrentPhase.LoserBracket).ToList();
            
            System.Diagnostics.Debug.WriteLine($"  Using KnockoutMatch.UpdateNextRoundFromCompletedMatch");
            KnockoutMatch.UpdateNextRoundFromCompletedMatch(completedMatch, allMatches);

            // If this was a winner bracket match, also update the loser bracket
            if (completedMatch.BracketType == BracketType.Winner)
            {
                System.Diagnostics.Debug.WriteLine($"  Winner bracket match - updating loser bracket");
                KnockoutMatch.UpdateLoserBracketFromWinnerMatch(completedMatch, CurrentPhase.LoserBracket);
            }

            // Then check for automatic byes that might result from the progression
            var winnerBracket = CurrentPhase.WinnerBracket;
            var loserBracket = CurrentPhase.LoserBracket;
            
            System.Diagnostics.Debug.WriteLine($"  Checking for automatic byes after progression");
            CheckAndHandleAutomaticByes(winnerBracket, loserBracket.Any() ? loserBracket : null);
            if (loserBracket.Any())
            {
                CheckAndHandleAutomaticByes(loserBracket, winnerBracket);
            }

            // Trigger UI refresh
            TriggerUIRefresh();

            System.Diagnostics.Debug.WriteLine($"=== ProcessMatchResult SUCCESS for match {completedMatch.Id} ===");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProcessMatchResult ERROR: {ex.Message}");
            return false;
        }
    }
}

/// <summary>
/// Ergebnis der Validierung von Freilos-Operationen
/// </summary>
public class ByeValidationResult
{
    public bool IsValid { get; }
    public string Message { get; }
    public bool CanGiveBye { get; }
    public bool CanUndoBye { get; }

    public ByeValidationResult(bool isValid, string message, bool canGiveBye, bool canUndoBye)
    {
        IsValid = isValid;
        Message = message;
        CanGiveBye = canGiveBye;
        CanUndoBye = canUndoBye;
    }
}

/// <summary>
/// UI-Status für Freilos-Buttons
/// </summary>
public class MatchByeUIStatus
{
    public bool ShowGiveByeButton { get; }
    public bool ShowUndoByeButton { get; }
    public string StatusMessage { get; }

    public MatchByeUIStatus(bool showGiveByeButton, bool showUndoByeButton, string statusMessage)
    {
        ShowGiveByeButton = showGiveByeButton;
        ShowUndoByeButton = showUndoByeButton;
        StatusMessage = statusMessage;
    }
}
