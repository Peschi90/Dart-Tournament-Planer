using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Generiert K.O.-Turnierbäume für Single- und Double-Elimination-Modi
/// Verantwortlich für die komplexe Logik der Bracket-Generierung
/// </summary>
public class KnockoutBracketGenerator
{
    private readonly TournamentClass _tournament;

    public KnockoutBracketGenerator(TournamentClass tournament)
    {
        _tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
    }

    public void GenerateSingleEliminationBracket(TournamentPhase phase, List<Player> players)
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
                // WICHTIG: Bestimme rundenspezifische Regeln
                var roundRules = _tournament.GameRules.GetRulesForRound(startingRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                    Player1 = shuffledPlayers[playerIndex++],
                    Player2 = shuffledPlayers[playerIndex++],
                    BracketType = BracketType.Winner,
                    Round = startingRound,
                    Position = i,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                    CreatedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                match.EnsureUniqueId();

                System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}, Round: {startingRound}, UsesSets: {match.UsesSets} (SetsToWin: {roundRules.SetsToWin}), UUID: {match.UniqueId}");
                currentRoundMatches.Add(match);
                matches.Add(match);
            }

            // Then, create "bye matches" for remaining players
            for (int i = 0; i < byesNeeded; i++)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln auch für Bye-Matches
                var roundRules = _tournament.GameRules.GetRulesForRound(startingRound);
                
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                    Player1 = shuffledPlayers[playerIndex++],
                    Player2 = null, // Bye
                    Winner = shuffledPlayers[playerIndex - 1], // Player with bye automatically wins
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Winner,
                    Round = startingRound,
                    Position = firstRoundMatches + i,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets auch für Bye-Matches
                    CreatedAt = DateTime.Now,
                    FinishedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                byeMatch.EnsureUniqueId();

                System.Diagnostics.Debug.WriteLine($"  Match {byeMatch.Id}: {byeMatch.Player1?.Name} gets bye, Round: {startingRound}, UsesSets: {byeMatch.UsesSets} (SetsToWin: {roundRules.SetsToWin}), UUID: {byeMatch.UniqueId}");
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
                    // WICHTIG: Bestimme rundenspezifische Regeln für jede Runde
                    var roundRules = _tournament.GameRules.GetRulesForRound(nextRound);
                    
                    var match = new KnockoutMatch
                    {
                        Id = matchId++,
                        // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                        SourceMatch1 = currentRoundMatches[i],
                        SourceMatch2 = i + 1 < currentRoundMatches.Count ? currentRoundMatches[i + 1] : null,
                        BracketType = BracketType.Winner,
                        Round = nextRound,
                        Position = i / 2,
                        Player1FromWinner = true,
                        Player2FromWinner = true,
                        UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                        CreatedAt = DateTime.Now
                    };

                    // Stelle sicher, dass UUID gültig ist
                    match.EnsureUniqueId();

                    System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: Winner of {match.SourceMatch1?.Id} vs Winner of {match.SourceMatch2?.Id ?? 0}, Round: {nextRound}, UsesSets: {match.UsesSets} (SetsToWin: {roundRules.SetsToWin}), UUID: {match.UniqueId}");
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
            var byeMatchManager = new ByeMatchManager(_tournament);
            byeMatchManager.PropagateInitialByeMatches(phase.WinnerBracket, null);
            
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

    public void GenerateDoubleEliminationBracket(TournamentPhase phase, List<Player> players)
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
            var byeMatchManager = new ByeMatchManager(_tournament);
            byeMatchManager.PropagateInitialByeMatches(phase.WinnerBracket, phase.LoserBracket);
            byeMatchManager.PropagateInitialByeMatches(phase.LoserBracket, null);

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
            // WICHTIG: Bestimme rundenspezifische Regeln
            var roundRules = _tournament.GameRules.GetRulesForRound(startingRound);
            
            var match = new KnockoutMatch
            {
                Id = matchId++,
                // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                Player1 = shuffledPlayers[playerIndex++],
                Player2 = shuffledPlayers[playerIndex++],
                BracketType = BracketType.Winner,
                Round = startingRound,
                Position = i,
                UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                CreatedAt = DateTime.Now
            };

            // Stelle sicher, dass UUID gültig ist
            match.EnsureUniqueId();

            System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}, Round: {startingRound}, UsesSets: {match.UsesSets} (SetsToWin: {roundRules.SetsToWin}), UUID: {match.UniqueId}");
            currentRoundMatches.Add(match);
            matches.Add(match);
        }

        // Create bye matches
        for (int i = 0; i < byesNeeded; i++)
        {
            // WICHTIG: Bestimme rundenspezifische Regeln auch für Bye-Matches
            var roundRules = _tournament.GameRules.GetRulesForRound(startingRound);
            
            var byeMatch = new KnockoutMatch
            {
                Id = matchId++,
                // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                Player1 = shuffledPlayers[playerIndex++],
                Player2 = null,
                Winner = shuffledPlayers[playerIndex - 1],
                Status = MatchStatus.Bye,
                BracketType = BracketType.Winner,
                Round = startingRound,
                Position = firstRoundMatches + i,
                UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets auch für Bye-Matches
                CreatedAt = DateTime.Now,
                FinishedAt = DateTime.Now
            };

            // Stelle sicher, dass UUID gültig ist
            byeMatch.EnsureUniqueId();

            System.Diagnostics.Debug.WriteLine($"  Match {byeMatch.Id}: {byeMatch.Player1?.Name} gets bye, Round: {startingRound}, UsesSets: {byeMatch.UsesSets} (SetsToWin: {roundRules.SetsToWin}), UUID: {byeMatch.UniqueId}");
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
                // WICHTIG: Bestimme rundenspezifische Regeln für jede Runde
                var roundRules = _tournament.GameRules.GetRulesForRound(nextRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    SourceMatch1 = currentRoundMatches[i],
                    SourceMatch2 = i + 1 < currentRoundMatches.Count ? currentRoundMatches[i + 1] : null,
                    BracketType = BracketType.Winner,
                    Round = nextRound,
                    Position = i / 2,
                    Player1FromWinner = true,
                    Player2FromWinner = true,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                    CreatedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                match.EnsureUniqueId();

                System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: Winner of {match.SourceMatch1?.Id} vs Winner of {match.SourceMatch2?.Id ?? 0}, Round: {nextRound}, UsesSets: {match.UsesSets} (SetsToWin: {roundRules.SetsToWin}), UUID: {match.UniqueId}");
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
        if (_tournament.GameRules.IncludeGroupPhaseLosersBracket)
        {
            var groupPhase = _tournament.Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
            var qualifiedPlayers = groupPhase.GetQualifiedPlayers(_tournament.GameRules.QualifyingPlayersPerGroup);
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
                // WICHTIG: Bestimme rundenspezifische Regeln für Loser Bracket
                var roundRules = _tournament.GameRules.GetRulesForRound(currentLoserRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                    Player1 = groupPhaseLosers[i],
                    Player2 = groupPhaseLosers[i + 1],
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                    CreatedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                match.EnsureUniqueId();

                System.Diagnostics.Debug.WriteLine($"  Loser Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}, Round: {currentLoserRound}, UsesSets: {match.UsesSets}, UUID: {match.UniqueId}");
                currentRound.Add(match);
                loserMatches.Add(match);
            }

            // Handle odd number with bye
            if (groupPhaseLosers.Count % 2 == 1)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln auch für Bye-Matches
                var roundRules = _tournament.GameRules.GetRulesForRound(currentLoserRound);
                
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                    Player1 = groupPhaseLosers.Last(),
                    Player2 = null,
                    Winner = groupPhaseLosers.Last(),
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets auch für Bye-Matches
                    CreatedAt = DateTime.Now,
                    FinishedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                byeMatch.EnsureUniqueId();

                System.Diagnostics.Debug.WriteLine($"  Loser Bye Match {byeMatch.Id}: {byeMatch.Player1?.Name} gets bye, Round: {currentLoserRound}, UsesSets: {byeMatch.UsesSets}, UUID: {byeMatch.UniqueId}");
                currentRound.Add(byeMatch);
                loserMatches.Add(byeMatch);
            }
        }

        // Integrate winner bracket losers
        var winnerRounds = winnerMatches.GroupBy(m => m.Round).OrderBy(g => g.Key).ToList();
        
        foreach (var winnerRound in winnerRounds.Take(winnerRounds.Count - 1)) // Skip final
        {
            // *** KORREKTUR: Nur advance wenn bereits Matches im currentRound sind ***
            if (currentRound.Any())
            {
                currentLoserRound = GetNextLoserRound(currentLoserRound);
            }
            
            var nextRound = new List<KnockoutMatch>();
            int position = 0;

            var loserProducingMatches = winnerRound.Where(m => m.Status != MatchStatus.Bye).ToList();
            var allParticipants = new List<object>();
            allParticipants.AddRange(currentRound.Cast<object>());
            allParticipants.AddRange(loserProducingMatches.Cast<object>());

            for (int i = 0; i < allParticipants.Count - 1; i += 2)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln für jede Loser Bracket Runde
                var roundRules = _tournament.GameRules.GetRulesForRound(currentLoserRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                    CreatedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                match.EnsureUniqueId();

                SetLoserBracketParticipant(match, allParticipants[i], true);
                SetLoserBracketParticipant(match, allParticipants[i + 1], false);

                System.Diagnostics.Debug.WriteLine($"  Loser Mixed Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}, Round: {currentLoserRound}, UsesSets: {match.UsesSets}, UUID: {match.UniqueId}");
                nextRound.Add(match);
                loserMatches.Add(match);
            }

            // Handle odd number
            if (allParticipants.Count % 2 == 1)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln auch für Bye-Matches
                var roundRules = _tournament.GameRules.GetRulesForRound(currentLoserRound);
                
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                    Player2 = null,
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets auch für Bye-Matches
                    CreatedAt = DateTime.Now,
                    FinishedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                byeMatch.EnsureUniqueId();

                SetLoserBracketParticipant(byeMatch, allParticipants.Last(), true);
                if (allParticipants.Last() is Player player)
                {
                    byeMatch.Winner = player;
                }

                System.Diagnostics.Debug.WriteLine($"  Loser Bye Match (odd) {byeMatch.Id}: {byeMatch.Player1?.Name} gets bye, Round: {currentLoserRound}, UsesSets: {byeMatch.UsesSets}, UUID: {byeMatch.UniqueId}");
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
                // WICHTIG: Bestimme rundenspezifische Regeln für jede weitere Runde
                var roundRules = _tournament.GameRules.GetRulesForRound(currentLoserRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    SourceMatch1 = currentRound[i],
                    SourceMatch2 = currentRound[i + 1],
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    Player1FromWinner = true,
                    Player2FromWinner = true,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                    CreatedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                match.EnsureUniqueId();

                System.Diagnostics.Debug.WriteLine($"  Loser Continue Match {match.Id}: Winner of {match.SourceMatch1?.Id} vs Winner of {match.SourceMatch2?.Id}, Round: {currentLoserRound}, UsesSets: {match.UsesSets}, UUID: {match.UniqueId}");
                nextRound.Add(match);
                loserMatches.Add(match);
            }

            // Handle odd number
            if (currentRound.Count % 2 == 1)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln auch für finale Bye-Matches
                var roundRules = _tournament.GameRules.GetRulesForRound(currentLoserRound);
                
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
                    SourceMatch1 = currentRound.Last(),
                    SourceMatch2 = null,
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    Player1FromWinner = true,
                    UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets auch für finale Bye-Matches
                    CreatedAt = DateTime.Now,
                    FinishedAt = DateTime.Now
                };

                // Stelle sicher, dass UUID gültig ist
                byeMatch.EnsureUniqueId();

                System.Diagnostics.Debug.WriteLine($"  Loser Final Bye Match {byeMatch.Id}: Winner of {byeMatch.SourceMatch1?.Id} gets bye, Round: {currentLoserRound}, UsesSets: {byeMatch.UsesSets}, UUID: {byeMatch.UniqueId}");
                nextRound.Add(byeMatch);
                loserMatches.Add(byeMatch);
            }

            currentRound = nextRound;
        }

        // *** KORREKTUR: Stelle sicher dass das letzte Match als LoserFinal markiert wird ***
        if (currentRound.Count == 1)
        {
            var finalMatch = currentRound[0];
            System.Diagnostics.Debug.WriteLine($"GenerateLoserBracket: Correcting final match {finalMatch.Id} from {finalMatch.Round} to LoserFinal");
            finalMatch.Round = KnockoutRound.LoserFinal;
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
        var loserFinal = phase.LoserBracket.LastOrDefault(m => m.Round == KnockoutRound.LoserFinal);

        System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: winnerFinal = {winnerFinal?.Id} (Round: {winnerFinal?.Round})");
        System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: loserFinal = {loserFinal?.Id} (Round: {loserFinal?.Round})");

        if (winnerFinal == null || loserFinal == null) 
        {
            System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Cannot create Grand Final - missing source matches");
            return;
        }

        int matchId = Math.Max(
            phase.WinnerBracket.Max(m => m.Id),
            phase.LoserBracket.Max(m => m.Id)
        ) + 1;

        // WICHTIG: Bestimme rundenspezifische Regeln auch für Grand Final
        var roundRules = _tournament.GameRules.GetRulesForRound(KnockoutRound.GrandFinal);

        var grandFinal = new KnockoutMatch
        {
            Id = matchId,
            // WICHTIG: UUID wird automatisch im KnockoutMatch-Konstruktor erstellt
            BracketType = BracketType.Winner,
            Round = KnockoutRound.GrandFinal,
            Position = 0,
            SourceMatch1 = winnerFinal,
            SourceMatch2 = loserFinal,
            Player1FromWinner = true,
            Player2FromWinner = true,
            UsesSets = roundRules.SetsToWin > 0, // WICHTIG: Setze UsesSets auch für Grand Final
            CreatedAt = DateTime.Now
        };

        // Stelle sicher, dass UUID gültig ist
        grandFinal.EnsureUniqueId();

        System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Created Grand Final match {grandFinal.Id} with sources WB:{winnerFinal.Id} and LB:{loserFinal.Id}, UUID: {grandFinal.UniqueId}");

        var winnerMatches = phase.WinnerBracket.ToList();
        winnerMatches.Add(grandFinal);
        phase.WinnerBracket = new ObservableCollection<KnockoutMatch>(winnerMatches);
        
        System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Grand Final added to Winner Bracket, total matches = {phase.WinnerBracket.Count}");
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
}