using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services.HubSync;

/// <summary>
/// Synchronisiert Tournament-Daten mit dem Hub
/// </summary>
public class TournamentDataSyncService
{
    private readonly HttpClient _httpClient;
    private readonly string _hubUrl;
    private readonly Action<string, string> _debugLog;

    public TournamentDataSyncService(HttpClient httpClient, string hubUrl, Action<string, string> debugLog)
    {
        _httpClient = httpClient;
        _hubUrl = hubUrl;
        _debugLog = debugLog;
    }

    /// <summary>
    /// Synchronisiert Tournament-Daten mit allen Match-Typen
    /// </summary>
    public async Task<bool> SyncTournamentWithClassesAsync(string tournamentId, string tournamentName, TournamentData tournamentData)
    {
        try
        {
            _debugLog($"🔄 [SYNC] Starting full tournament sync with ALL match types: {tournamentId}", "SYNC");

            var allMatches = new List<object>();
            var tournamentClasses = new List<object>();
            var gameRulesArray = new List<object>();

            foreach (var tournamentClass in tournamentData.TournamentClasses)
            {
                await ProcessTournamentClass(tournamentClass, allMatches, tournamentClasses, gameRulesArray, tournamentId);
            }

            var syncData = new
            {
                tournamentId,
                name = tournamentName,
                classes = tournamentClasses,
                matches = allMatches,
                gameRules = gameRulesArray,
                syncedAt = DateTime.UtcNow,
                matchTypeStats = new
                {
                    totalMatches = allMatches.Count,
                    groupMatches = allMatches.Count(m => ((dynamic)m).matchType.ToString() == "Group"),
                    finalsMatches = allMatches.Count(m => ((dynamic)m).matchType.ToString() == "Finals"),
                    winnerBracketMatches = allMatches.Count(m => ((dynamic)m).matchType.ToString().StartsWith("Knockout-WB")),
                    loserBracketMatches = allMatches.Count(m => ((dynamic)m).matchType.ToString().StartsWith("Knockout-LB"))
                }
            };

            _debugLog($"🎯 [SYNC] Tournament sync data prepared:", "SYNC");
            _debugLog($"   Classes: {tournamentClasses.Count}", "SYNC");
            _debugLog($"   Total Matches: {allMatches.Count}", "SYNC");
            _debugLog($"   Game Rules: {gameRulesArray.Count}", "SYNC");

            var json = JsonSerializer.Serialize(syncData, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var syncClient = new HttpClient();
            syncClient.Timeout = TimeSpan.FromSeconds(60);
            var response = await syncClient.PostAsync($"{_hubUrl}/api/tournaments/{tournamentId}/sync-full", content);

            if (response.IsSuccessStatusCode)
            {
                _debugLog($"✅ [SYNC] Tournament sync successful with ALL match types:", "SUCCESS");
                _debugLog($"   📊 Synced: {syncData.matchTypeStats.totalMatches} total matches", "SUCCESS");
                _debugLog($"   📊 Game Rules: {gameRulesArray.Count} rules synced", "SUCCESS");
                return true;
            }
            else
            {
                _debugLog($"❌ [SYNC] Tournament sync failed: {response.StatusCode}", "ERROR");
                var errorContent = await response.Content.ReadAsStringAsync();
                _debugLog($"❌ [SYNC] Error response: {errorContent}", "ERROR");
                return false;
            }
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [SYNC] Tournament sync error: {ex.Message}", "ERROR");
            return false;
        }
    }

    /// <summary>
    /// Verarbeitet eine Tournament-Klasse für die Synchronisation
    /// </summary>
    private async Task ProcessTournamentClass(TournamentClass tournamentClass, List<object> allMatches, 
        List<object> tournamentClasses, List<object> gameRulesArray, string tournamentId)
    {
        _debugLog($"🎮 [SYNC] Processing class {tournamentClass.Name}", "SYNC");
        _debugLog($"🎮 [SYNC] Base GameRules: Sets={tournamentClass.GameRules.SetsToWin}, Legs={tournamentClass.GameRules.LegsToWin}, LegsPerSet={tournamentClass.GameRules.LegsPerSet}", "SYNC");
        _debugLog($"🎮 [SYNC] KnockoutRoundRules count: {tournamentClass.GameRules.KnockoutRoundRules.Count}", "SYNC");

        // Debug: Ausgabe aller KnockoutRoundRules
        //foreach (var roundRule in tournamentClass.GameRules.KnockoutRoundRules)
        //{
        //    _debugLog($"   Round {roundRule.Key}: Sets={roundRule.Value.SetsToWin}, Legs={roundRule.Value.LegsToWin}, LegsPerSet={roundRule.Value.LegsPerSet}", "SYNC");
        //}

        // Zähle alle Match-Typen
        int groupMatches = tournamentClass.Groups.Sum(g => g.Matches.Count);
        int finalsMatches = tournamentClass.CurrentPhase?.FinalsGroup?.Matches.Count ?? 0;
        int winnerBracketMatches = tournamentClass.CurrentPhase?.WinnerBracket?.Count ?? 0;
        int loserBracketMatches = tournamentClass.CurrentPhase?.LoserBracket?.Count ?? 0;
        int totalMatches = groupMatches + finalsMatches + winnerBracketMatches + loserBracketMatches;

        _debugLog($"   📊 Match Count: Groups={groupMatches}, Finals={finalsMatches}, Winner={winnerBracketMatches}, Loser={loserBracketMatches}, Total={totalMatches}", "SYNC");

        tournamentClasses.Add(new
        {
            id = tournamentClass.Id,
            name = tournamentClass.Name,
            playerCount = tournamentClass.Groups.Sum(g => g.Players.Count),
            groupCount = tournamentClass.Groups.Count,
            matchCount = totalMatches,
            phase = GetTournamentPhase(tournamentClass)
        });

        // Game Rules hinzufügen
        gameRulesArray.Add(CreateGameRulesObject(tournamentClass));

        // 1. Gruppenphasen-Matches verarbeiten
        await ProcessGroupMatches(tournamentClass, allMatches, tournamentId);

        // 2. Finals-Matches verarbeiten
        await ProcessFinalsMatches(tournamentClass, allMatches, tournamentId, gameRulesArray);

        // 3. Winner Bracket Matches verarbeiten
        await ProcessWinnerBracketMatches(tournamentClass, allMatches, tournamentId, gameRulesArray);

        // 4. Loser Bracket Matches verarbeiten
        await ProcessLoserBracketMatches(tournamentClass, allMatches, tournamentId, gameRulesArray);

        _debugLog($"✅ [SYNC] Processed class {tournamentClass.Name}: {totalMatches} matches, {gameRulesArray.Count} game rules", "SUCCESS");
    }

    /// <summary>
    /// Verarbeitet Gruppenphasen-Matches
    /// </summary>
    private async Task ProcessGroupMatches(TournamentClass tournamentClass, List<object> allMatches, string tournamentId)
    {
        foreach (var group in tournamentClass.Groups)
        {
            foreach (var match in group.Matches)
            {
                allMatches.Add(new
                {
                    id = match.UniqueId,
                    matchId = match.Id,
                    uniqueId = match.UniqueId,
                    hubIdentifier = match.GetHubIdentifier(tournamentId),
                    player1 = match.Player1?.Name ?? "Unbekannt",
                    player2 = match.Player2?.Name ?? "Unbekannt",
                    player1Sets = match.Player1Sets,
                    player2Sets = match.Player2Sets,
                    player1Legs = match.Player1Legs,
                    player2Legs = match.Player2Legs,
                    status = GetMatchStatus(match),
                    winner = GetWinner(match),
                    notes = match.Notes ?? "",
                    classId = tournamentClass.Id,
                    className = tournamentClass.Name,
                    groupId = group.Id,
                    groupName = group.Name,
                    matchType = "Group",
                    uuidSystem = new
                    {
                        hasValidUuid = match.HasValidUniqueId(),
                        hubReady = match.HasValidUniqueId() && !string.IsNullOrEmpty(tournamentId),
                        identificationMethods = new[] { "uuid", "numericId", "hubIdentifier" },
                        preferredAccess = match.HasValidUniqueId() ? "uuid" : "numericId"
                    },
                    // ✅ NEU: Game Rules Information auch für Group Matches
                    gameRulesId = 1,
                    gameRulesUsed = new
                    {
                        id = 1,
                        name = "Standard Regel",
                        gamePoints = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode),
                        startingScore = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode),
                        gameMode = tournamentClass.GameRules.GameMode.ToString(),
                        finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                        doubleOut = tournamentClass.GameRules.FinishMode == FinishMode.DoubleOut,
                        singleOut = tournamentClass.GameRules.FinishMode == FinishMode.SingleOut,
                        setsToWin = tournamentClass.GameRules.SetsToWin,
                        legsToWin = tournamentClass.GameRules.LegsToWin,
                        legsPerSet = tournamentClass.GameRules.LegsPerSet,
                        maxSets = Math.Max(tournamentClass.GameRules.SetsToWin * 2 - 1, 5),
                        maxLegsPerSet = tournamentClass.GameRules.LegsPerSet,
                        playWithSets = tournamentClass.GameRules.PlayWithSets,
                        classId = tournamentClass.Id,
                        className = tournamentClass.Name,
                        matchType = "Group",
                        isDefault = true,
                        // ✅ NEU: Dart Scoring spezifische Daten
                        dartScoringReady = true,
                        gameTypeString = GetGameTypeString(tournamentClass.GameRules.GameMode),
                        finishTypeString = GetFinishTypeString(tournamentClass.GameRules.FinishMode)
                    },
                    createdAt = match.CreatedAt,
                    startedAt = match.StartTime,
                    finishedAt = match.EndTime,
                    syncedAt = DateTime.UtcNow
                });
            }
        }
    }

    /// <summary>
    /// Verarbeitet Finals-Matches
    /// </summary>
    private async Task ProcessFinalsMatches(TournamentClass tournamentClass, List<object> allMatches, 
        string tournamentId, List<object> gameRulesArray)
    {
        if (tournamentClass.CurrentPhase?.FinalsGroup == null) return;

        _debugLog($"🏆 [SYNC] Processing Finals matches for {tournamentClass.Name}: {tournamentClass.CurrentPhase.FinalsGroup.Matches.Count} matches", "SYNC");

        // Finals-spezifische Game Rules hinzufügen
        var finalsGameRules = CreateFinalsGameRulesObject(tournamentClass);
        gameRulesArray.Add(finalsGameRules);

        _debugLog($"🏆 [SYNC] Finals GameRules: Sets={tournamentClass.GameRules.SetsToWin}, Legs={tournamentClass.GameRules.LegsToWin}, LegsPerSet={tournamentClass.GameRules.LegsPerSet}", "SYNC");
        _debugLog($"🏆 [SYNC] Finals GameRules ID: {finalsGameRules.GetType().GetProperty("id")?.GetValue(finalsGameRules)}", "SYNC");

        foreach (var match in tournamentClass.CurrentPhase.FinalsGroup.Matches)
        {
            allMatches.Add(new
            {
                id = match.UniqueId,
                matchId = match.Id,
                uniqueId = match.UniqueId,
                hubIdentifier = match.GetHubIdentifier(tournamentId),
                player1 = match.Player1?.Name ?? "TBD",
                player2 = match.Player2?.Name ?? "TBD",
                player1Sets = match.Player1Sets,
                player2Sets = match.Player2Sets,
                player1Legs = match.Player1Legs,
                player2Legs = match.Player2Legs,
                status = GetMatchStatus(match),
                winner = GetWinner(match),
                notes = match.Notes ?? "",
                classId = tournamentClass.Id,
                className = tournamentClass.Name,
                matchType = "Finals",
                groupId = (int?)null,
                groupName = "Finals",
                uuidSystem = new
                {
                    hasValidUuid = match.HasValidUniqueId(),
                    hubReady = match.HasValidUniqueId() && !string.IsNullOrEmpty(tournamentId),
                    identificationMethods = new[] { "uuid", "numericId", "hubIdentifier" },
                    preferredAccess = match.HasValidUniqueId() ? "uuid" : "numericId"
                },
                // ✅ KORRIGIERT: Game Rules Information hinzufügen
                gameRulesId = $"{tournamentClass.Id}_Finals",
                gameRulesUsed = new
                {
                    id = $"{tournamentClass.Id}_Finals",
                    name = $"{tournamentClass.Name} Finalrunde",
                    gamePoints = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode),
                    startingScore = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode),
                    gameMode = tournamentClass.GameRules.GameMode.ToString(),
                    finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                    doubleOut = tournamentClass.GameRules.FinishMode == FinishMode.DoubleOut,
                    singleOut = tournamentClass.GameRules.FinishMode == FinishMode.SingleOut,
                    setsToWin = tournamentClass.GameRules.SetsToWin,
                    legsToWin = tournamentClass.GameRules.LegsToWin,
                    legsPerSet = tournamentClass.GameRules.LegsPerSet,
                    maxSets = Math.Max(tournamentClass.GameRules.SetsToWin * 2 - 1, 5),
                    maxLegsPerSet = tournamentClass.GameRules.LegsPerSet,
                    playWithSets = tournamentClass.GameRules.PlayWithSets,
                    classId = tournamentClass.Id,
                    className = tournamentClass.Name,
                    matchType = "Finals",
                    isDefault = false,
                    dartScoringReady = true,
                    gameTypeString = GetGameTypeString(tournamentClass.GameRules.GameMode),
                    finishTypeString = GetFinishTypeString(tournamentClass.GameRules.FinishMode),
                    // ✅ NEU: Finals-spezifische Metadaten
                    finalsSpecific = new
                    {
                        isFinalsMatch = true,
                        usesBaseRules = true,
                        escalated = false
                    }
                },
                // Zeitstempel
                createdAt = match.CreatedAt,
                startedAt = match.StartTime,
                finishedAt = match.EndTime,
                syncedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Verarbeitet Winner Bracket Matches
    /// </summary>
    private async Task ProcessWinnerBracketMatches(TournamentClass tournamentClass, List<object> allMatches, 
        string tournamentId, List<object> gameRulesArray)
    {
        if (tournamentClass.CurrentPhase?.WinnerBracket == null) return;

        _debugLog($"⚡ [SYNC] Processing Winner Bracket matches for {tournamentClass.Name}: {tournamentClass.CurrentPhase.WinnerBracket.Count} matches", "SYNC");

        // Gruppiere Winner Bracket Matches nach Runden
        var winnerRounds = tournamentClass.CurrentPhase.WinnerBracket
            .GroupBy(m => m.Round)
            .ToList();

        foreach (var roundGroup in winnerRounds)
        {
            var round = roundGroup.Key;
            var matchCount = roundGroup.Count();

            // ✅ KORRIGIERT: Round-spezifische Game Rules hinzufügen
            var roundGameRules = CreateWinnerBracketGameRulesObject(tournamentClass, round);
            gameRulesArray.Add(roundGameRules);

            foreach (var knockoutMatch in roundGroup)
            {
                var (setsToWin, legsToWin) = GetEscalatedRulesForWinnerBracket(round, tournamentClass.GameRules);
                var legsPerSet = GetLegsPerSetForRound(round, tournamentClass.GameRules);
                var gameRuleId = $"{tournamentClass.Id}_WB_{round}";

                //_debugLog($"🎮 [SYNC] Winner Bracket Match {knockoutMatch.Id} ({round}):", "SYNC");
                //_debugLog($"   Sets/Legs/LegsPerSet: {setsToWin}/{legsToWin}/{legsPerSet}", "SYNC");
                //_debugLog($"   Game Rule ID: {gameRuleId}", "SYNC");

                allMatches.Add(new
                {
                    id = knockoutMatch.UniqueId,
                    matchId = knockoutMatch.Id,
                    uniqueId = knockoutMatch.UniqueId,
                    hubIdentifier = knockoutMatch.GetHubIdentifier(tournamentId),
                    player1 = knockoutMatch.Player1?.Name ?? "TBD",
                    player2 = knockoutMatch.Player2?.Name ?? "TBD",
                    player1Sets = knockoutMatch.Player1Sets,
                    player2Sets = knockoutMatch.Player2Sets,
                    player1Legs = knockoutMatch.Player1Legs,
                    player2Legs = knockoutMatch.Player2Legs,
                    status = GetKnockoutMatchStatus(knockoutMatch),
                    winner = GetKnockoutWinner(knockoutMatch),
                    notes = knockoutMatch.Notes ?? "",
                    classId = tournamentClass.Id,
                    className = tournamentClass.Name,
                    matchType = $"Knockout-{knockoutMatch.BracketType}-{knockoutMatch.Round}",
                    groupId = (int?)null,
                    groupName = $"Winner Bracket - {knockoutMatch.Round}",
                    round = knockoutMatch.Round,
                    position = knockoutMatch.Position,
                    uuidSystem = new
                    {
                        hasValidUuid = knockoutMatch.HasValidUniqueId(),
                        hubReady = knockoutMatch.HasValidUniqueId() && !string.IsNullOrEmpty(tournamentId),
                        identificationMethods = new[] { "uuid", "numericId", "hubIdentifier" },
                        preferredAccess = knockoutMatch.HasValidUniqueId() ? "uuid" : "numericId"
                    },
                    // ✅ KORRIGIERT: Game Rules Information hinzufügen
                    gameRulesId = gameRuleId,
                    gameRulesUsed = new
                    {
                        id = gameRuleId,
                        name = $"{tournamentClass.Name} {GetWinnerBracketRoundName(round)}",
                        gamePoints = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode),
                        startingScore = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode),
                        gameMode = tournamentClass.GameRules.GameMode.ToString(),
                        finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                        doubleOut = tournamentClass.GameRules.FinishMode == FinishMode.DoubleOut,
                        singleOut = tournamentClass.GameRules.FinishMode == FinishMode.SingleOut,
                        setsToWin = setsToWin,
                        legsToWin = legsToWin,
                        legsPerSet = legsPerSet,
                        maxSets = Math.Max(setsToWin * 2 - 1, 5),
                        maxLegsPerSet = legsPerSet,
                        playWithSets = tournamentClass.GameRules.PlayWithSets || setsToWin > 1,
                        classId = tournamentClass.Id,
                        className = tournamentClass.Name,
                        matchType = $"Knockout-WB-{round}",
                        round = round.ToString(),
                        isDefault = false,
                        dartScoringReady = true,
                        gameTypeString = GetGameTypeString(tournamentClass.GameRules.GameMode),
                        finishTypeString = GetFinishTypeString(tournamentClass.GameRules.FinishMode),
                        // ✅ NEU: Round-spezifische Metadaten
                        roundSpecific = new
                        {
                            originalSetsToWin = tournamentClass.GameRules.SetsToWin,
                            originalLegsToWin = tournamentClass.GameRules.LegsToWin,
                            originalLegsPerSet = tournamentClass.GameRules.LegsPerSet,
                            escalated = setsToWin != tournamentClass.GameRules.SetsToWin || legsToWin != tournamentClass.GameRules.LegsToWin,
                            hasRoundRules = tournamentClass.GameRules.KnockoutRoundRules.ContainsKey(round),
                            roundName = GetWinnerBracketRoundName(round)
                        }
                    },
                    // Zeitstempel
                    createdAt = knockoutMatch.CreatedAt,
                    startedAt = knockoutMatch.StartedAt,
                    finishedAt = knockoutMatch.FinishedAt,
                    syncedAt = DateTime.UtcNow
                });
            }
        }
    }

    /// <summary>
    /// Verarbeitet Loser Bracket Matches
    /// </summary>
    private async Task ProcessLoserBracketMatches(TournamentClass tournamentClass, List<object> allMatches, 
        string tournamentId, List<object> gameRulesArray)
    {
        if (tournamentClass.CurrentPhase?.LoserBracket == null) return;

        _debugLog($"🔄 [SYNC] Processing Loser Bracket matches for {tournamentClass.Name}: {tournamentClass.CurrentPhase.LoserBracket.Count} matches", "SYNC");

        var loserRounds = tournamentClass.CurrentPhase.LoserBracket
            .GroupBy(m => m.Round)
            .ToList();

        foreach (var roundGroup in loserRounds)
        {
            var round = roundGroup.Key;

            // ✅ KORRIGIERT: Round-spezifische Game Rules hinzufügen
            var roundGameRules = CreateLoserBracketGameRulesObject(tournamentClass, round);
            gameRulesArray.Add(roundGameRules);

            foreach (var knockoutMatch in roundGroup)
            {
                var (setsToWin, legsToWin) = GetLoserBracketRules(round, tournamentClass.GameRules);
                var legsPerSet = GetLegsPerSetForRound(round, tournamentClass.GameRules);
                var gameRuleId = $"{tournamentClass.Id}_LB_{round}";

                //_debugLog($"🔄 [SYNC] Loser Bracket Match {knockoutMatch.Id} ({round}):", "SYNC");
                //_debugLog($"   Sets/Legs/LegsPerSet: {setsToWin}/{legsToWin}/{legsPerSet}", "SYNC");
                //_debugLog($"   Game Rule ID: {gameRuleId}", "SYNC");

                allMatches.Add(new
                {
                    id = knockoutMatch.UniqueId,
                    matchId = knockoutMatch.Id,
                    uniqueId = knockoutMatch.UniqueId,
                    hubIdentifier = knockoutMatch.GetHubIdentifier(tournamentId),
                    player1 = knockoutMatch.Player1?.Name ?? "TBD",
                    player2 = knockoutMatch.Player2?.Name ?? "TBD",
                    player1Sets = knockoutMatch.Player1Sets,
                    player2Sets = knockoutMatch.Player2Sets,
                    player1Legs = knockoutMatch.Player1Legs,
                    player2Legs = knockoutMatch.Player2Legs,
                    status = GetKnockoutMatchStatus(knockoutMatch),
                    winner = GetKnockoutWinner(knockoutMatch),
                    notes = knockoutMatch.Notes ?? "",
                    classId = tournamentClass.Id,
                    className = tournamentClass.Name,
                    matchType = $"Knockout-LB-{knockoutMatch.Round}",
                    groupId = (int?)null,
                    groupName = $"Loser Bracket - {knockoutMatch.Round}",
                    round = knockoutMatch.Round,
                    position = knockoutMatch.Position,
                    uuidSystem = new
                    {
                        hasValidUuid = knockoutMatch.HasValidUniqueId(),
                        hubReady = knockoutMatch.HasValidUniqueId() && !string.IsNullOrEmpty(tournamentId),
                        identificationMethods = new[] { "uuid", "numericId", "hubIdentifier" },
                        preferredAccess = knockoutMatch.HasValidUniqueId() ? "uuid" : "numericId"
                    },
                    // ✅ KORRIGIERT: Game Rules Information hinzufügen
                    gameRulesId = gameRuleId,
                    gameRulesUsed = new
                    {
                        id = gameRuleId,
                        name = $"{tournamentClass.Name} {GetLoserBracketRoundName(round)}",
                        gamePoints = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode),
                        startingScore = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode),
                        gameMode = tournamentClass.GameRules.GameMode.ToString(),
                        finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                        doubleOut = tournamentClass.GameRules.FinishMode == FinishMode.DoubleOut,
                        singleOut = tournamentClass.GameRules.FinishMode == FinishMode.SingleOut,
                        setsToWin = setsToWin,
                        legsToWin = legsToWin,
                        legsPerSet = legsPerSet,
                        maxSets = Math.Max(setsToWin * 2 - 1, 5),
                        maxLegsPerSet = legsPerSet,
                        playWithSets = tournamentClass.GameRules.PlayWithSets || setsToWin > 1,
                        classId = tournamentClass.Id,
                        className = tournamentClass.Name,
                        matchType = $"Knockout-LB-{round}",
                        round = round.ToString(),
                        isDefault = false,
                        dartScoringReady = true,
                        gameTypeString = GetGameTypeString(tournamentClass.GameRules.GameMode),
                        finishTypeString = GetFinishTypeString(tournamentClass.GameRules.FinishMode),
                        // ✅ NEU: Round-spezifische Metadaten
                        roundSpecific = new
                        {
                            originalSetsToWin = tournamentClass.GameRules.SetsToWin,
                            originalLegsToWin = tournamentClass.GameRules.LegsToWin,
                            originalLegsPerSet = tournamentClass.GameRules.LegsPerSet,
                            escalated = setsToWin != tournamentClass.GameRules.SetsToWin || legsToWin != tournamentClass.GameRules.LegsToWin,
                            hasRoundRules = tournamentClass.GameRules.KnockoutRoundRules.ContainsKey(round),
                            roundName = GetLoserBracketRoundName(round),
                            bracketType = "Loser"
                        }
                    },
                    // Zeitstempel
                    createdAt = knockoutMatch.CreatedAt,
                    startedAt = knockoutMatch.StartedAt,
                    finishedAt = knockoutMatch.FinishedAt,
                    syncedAt = DateTime.UtcNow
                });
            }
        }
    }

    // Helper-Methoden
    private string GetTournamentPhase(TournamentClass tournamentClass)
    {
        if (tournamentClass.CurrentPhase?.WinnerBracket?.Any() == true || tournamentClass.CurrentPhase?.LoserBracket?.Any() == true)
        {
            return "Knockout";
        }
        else if (tournamentClass.CurrentPhase?.FinalsGroup?.Matches?.Any() == true)
        {
            return "Finals";
        }
        else
        {
            return "GroupPhase";
        }
    }

    private object CreateGameRulesObject(TournamentClass tournamentClass)
    {
        // ✅ KORRIGIERT: Korrekte GameMode-Mapping für JavaScript/HTML Frontend
        var gamePoints = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode);
        var isDoubleOut = tournamentClass.GameRules.FinishMode == FinishMode.DoubleOut;
        
        return new
        {
            id = 1,
            name = "Standard Regel",
            // ✅ NEU: Korrekte Frontend-Mapping-Eigenschaften
            gamePoints = gamePoints,
            startingScore = gamePoints,  // Zusätzliches Mapping für Frontend
            gameMode = tournamentClass.GameRules.GameMode.ToString(),
            finishMode = tournamentClass.GameRules.FinishMode.ToString(),
            doubleOut = isDoubleOut,  // Boolean für Frontend
            singleOut = !isDoubleOut, // Inverse für Kompatibilität
            setsToWin = tournamentClass.GameRules.SetsToWin,
            legsToWin = tournamentClass.GameRules.LegsToWin,
            legsPerSet = tournamentClass.GameRules.LegsPerSet,
            maxSets = Math.Max(tournamentClass.GameRules.SetsToWin * 2 - 1, 5),
            maxLegsPerSet = tournamentClass.GameRules.LegsPerSet,
            playWithSets = tournamentClass.GameRules.PlayWithSets,
            classId = tournamentClass.Id,
            className = tournamentClass.Name,
            matchType = "Group",
            isDefault = true,
            // ✅ NEU: Frontend-Kompatibilität für Dart Scoring
            dartScoringCompatibility = new
            {
                gamePointsNumber = gamePoints,
                gameTypeString = GetGameTypeString(tournamentClass.GameRules.GameMode),
                finishTypeString = GetFinishTypeString(tournamentClass.GameRules.FinishMode),
                requiresDoubleOut = isDoubleOut,
                formatDescription = GetFormatDescription(tournamentClass.GameRules)
            }
        };
    }

    private object CreateFinalsGameRulesObject(TournamentClass tournamentClass)
    {
        var gamePoints = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode);
        var isDoubleOut = tournamentClass.GameRules.FinishMode == FinishMode.DoubleOut;
        
        return new
        {
            id = $"{tournamentClass.Id}_Finals",
            name = $"{tournamentClass.Name} Finalrunde",
            gamePoints = gamePoints,
            startingScore = gamePoints,
            gameMode = tournamentClass.GameRules.GameMode.ToString(),
            finishMode = tournamentClass.GameRules.FinishMode.ToString(),
            doubleOut = isDoubleOut,
            singleOut = !isDoubleOut,
            setsToWin = tournamentClass.GameRules.SetsToWin,
            legsToWin = tournamentClass.GameRules.LegsToWin,
            legsPerSet = tournamentClass.GameRules.LegsPerSet,
            maxSets = Math.Max(tournamentClass.GameRules.SetsToWin * 2 - 1, 5),
            maxLegsPerSet = tournamentClass.GameRules.LegsPerSet,
            playWithSets = tournamentClass.GameRules.PlayWithSets,
            classId = tournamentClass.Id,
            className = tournamentClass.Name,
            matchType = "Finals",
            isDefault = false,
            dartScoringCompatibility = new
            {
                gamePointsNumber = gamePoints,
                gameTypeString = GetGameTypeString(tournamentClass.GameRules.GameMode),
                finishTypeString = GetFinishTypeString(tournamentClass.GameRules.FinishMode),
                requiresDoubleOut = isDoubleOut,
                formatDescription = GetFormatDescription(tournamentClass.GameRules)
            }
        };
    }

    private object CreateWinnerBracketGameRulesObject(TournamentClass tournamentClass, KnockoutRound round)
    {
        var (setsToWin, legsToWin) = GetEscalatedRulesForWinnerBracket(round, tournamentClass.GameRules);
        var legsPerSet = GetLegsPerSetForRound(round, tournamentClass.GameRules);
        var gamePoints = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode);
        var isDoubleOut = tournamentClass.GameRules.FinishMode == FinishMode.DoubleOut;

        return new
        {
            id = $"{tournamentClass.Id}_WB_{round}",
            name = $"{tournamentClass.Name} {GetWinnerBracketRoundName(round)}",
            gamePoints = gamePoints,
            startingScore = gamePoints,
            gameMode = tournamentClass.GameRules.GameMode.ToString(),
            finishMode = tournamentClass.GameRules.FinishMode.ToString(),
            doubleOut = isDoubleOut,
            singleOut = !isDoubleOut,
            setsToWin = setsToWin,
            legsToWin = legsToWin,
            legsPerSet = legsPerSet,
            playWithSets = tournamentClass.GameRules.PlayWithSets || setsToWin > 1,
            classId = tournamentClass.Id,
            className = tournamentClass.Name,
            matchType = $"Knockout-WB-{round}",
            round = round.ToString(),
            isDefault = false,
            dartScoringCompatibility = new
            {
                gamePointsNumber = gamePoints,
                gameTypeString = GetGameTypeString(tournamentClass.GameRules.GameMode),
                finishTypeString = GetFinishTypeString(tournamentClass.GameRules.FinishMode),
                requiresDoubleOut = isDoubleOut,
                formatDescription = $"{GetGameTypeString(tournamentClass.GameRules.GameMode)} {GetFinishTypeString(tournamentClass.GameRules.FinishMode)}, First to {setsToWin} Sets ({legsPerSet} Legs per Set)"
            }
        };
    }

    private object CreateLoserBracketGameRulesObject(TournamentClass tournamentClass, KnockoutRound round)
    {
        var (setsToWin, legsToWin) = GetLoserBracketRules(round, tournamentClass.GameRules);
        var legsPerSet = GetLegsPerSetForRound(round, tournamentClass.GameRules);
        var gamePoints = GetGamePointsFromGameMode(tournamentClass.GameRules.GameMode);
        var isDoubleOut = tournamentClass.GameRules.FinishMode == FinishMode.DoubleOut;

        return new
        {
            id = $"{tournamentClass.Id}_LB_{round}",
            name = $"{tournamentClass.Name} {GetLoserBracketRoundName(round)}",
            gamePoints = gamePoints,
            startingScore = gamePoints,
            gameMode = tournamentClass.GameRules.GameMode.ToString(),
            finishMode = tournamentClass.GameRules.FinishMode.ToString(),
            doubleOut = isDoubleOut,
            singleOut = !isDoubleOut,
            setsToWin = setsToWin,
            legsToWin = legsToWin,
            legsPerSet = legsPerSet,
            playWithSets = tournamentClass.GameRules.PlayWithSets || setsToWin > 1,
            classId = tournamentClass.Id,
            className = tournamentClass.Name,
            matchType = $"Knockout-LB-{round}",
            round = round.ToString(),
            isDefault = false,
            dartScoringCompatibility = new
            {
                gamePointsNumber = gamePoints,
                gameTypeString = GetGameTypeString(tournamentClass.GameRules.GameMode),
                finishTypeString = GetFinishTypeString(tournamentClass.GameRules.FinishMode),
                requiresDoubleOut = isDoubleOut,
                formatDescription = $"{GetGameTypeString(tournamentClass.GameRules.GameMode)} {GetFinishTypeString(tournamentClass.GameRules.FinishMode)}, First to {setsToWin} Sets ({legsPerSet} Legs per Set)"
            }
        };
    }

    // Status-Helper
    private string GetMatchStatus(Match match) =>
        match.Status switch
        {
            MatchStatus.NotStarted => "NotStarted",
            MatchStatus.InProgress => "InProgress",
            MatchStatus.Finished => "Finished",
            MatchStatus.Bye => "Finished",
            _ => "NotStarted"
        };

    private string? GetWinner(Match match) =>
        match.Status != MatchStatus.Finished && match.Status != MatchStatus.Bye ? null : match.Winner?.Name;

    private string GetKnockoutMatchStatus(KnockoutMatch match) =>
        match.Status switch
        {
            MatchStatus.NotStarted => "NotStarted",
            MatchStatus.InProgress => "InProgress",
            MatchStatus.Finished => "Finished",
            MatchStatus.Bye => "Finished",
            _ => "NotStarted"
        };

    private string? GetKnockoutWinner(KnockoutMatch match) =>
        match.Status != MatchStatus.Finished && match.Status != MatchStatus.Bye ? null : match.Winner?.Name;

    // Game Rules Berechnung
    private (int setsToWin, int legsToWin) GetEscalatedRulesForWinnerBracket(KnockoutRound round, GameRules baseRules)
    {
        if (baseRules.KnockoutRoundRules.TryGetValue(round, out var roundRules))
        {
            return (roundRules.SetsToWin, roundRules.LegsToWin);
        }

        return round switch
        {
            KnockoutRound.Best64 => (Math.Max(2, baseRules.SetsToWin - 1), Math.Max(3, baseRules.LegsToWin)),
            KnockoutRound.Best32 => (Math.Max(2, baseRules.SetsToWin - 1), Math.Max(3, baseRules.LegsToWin)),
            KnockoutRound.Best16 => (baseRules.SetsToWin, baseRules.LegsToWin),
            KnockoutRound.Quarterfinal => (baseRules.SetsToWin, baseRules.LegsToWin),
            KnockoutRound.Semifinal => (baseRules.SetsToWin, Math.Min(baseRules.LegsToWin + 1, 6)),
            KnockoutRound.Final => (Math.Min(baseRules.SetsToWin + 1, 5), Math.Min(baseRules.LegsToWin + 1, 6)),
            _ => (baseRules.SetsToWin, baseRules.LegsToWin)
        };
    }

    private (int setsToWin, int legsToWin) GetLoserBracketRules(KnockoutRound round, GameRules baseRules)
    {
        if (baseRules.KnockoutRoundRules.TryGetValue(round, out var roundRules))
        {
            return (roundRules.SetsToWin, roundRules.LegsToWin);
        }

        return round switch
        {
            KnockoutRound.LoserFinal => (baseRules.SetsToWin, Math.Min(baseRules.LegsToWin + 1, 5)),
            _ => (Math.Max(2, baseRules.SetsToWin - 1), baseRules.LegsToWin)
        };
    }

    private int GetLegsPerSetForRound(KnockoutRound round, GameRules baseRules)
    {
        if (baseRules.KnockoutRoundRules.TryGetValue(round, out var roundRules))
        {
            return roundRules.LegsPerSet;
        }

        return baseRules.LegsPerSet;
    }

    private string GetWinnerBracketRoundName(KnockoutRound round) =>
        round switch
        {
            KnockoutRound.Best64 => "K.O. Beste 64",
            KnockoutRound.Best32 => "K.O. Beste 32",
            KnockoutRound.Best16 => "K.O. Beste 16",
            KnockoutRound.Quarterfinal => "K.O. Viertelfinale",
            KnockoutRound.Semifinal => "K.O. Halbfinale",
            KnockoutRound.Final => "K.O. Finale",
            _ => $"K.O. Winner {round}"
        };

    private string GetLoserBracketRoundName(KnockoutRound round) =>
        round switch
        {
            KnockoutRound.LoserFinal => "K.O. Loser Final",
            _ => $"K.O. Loser Runde {(int)round}"
        };

    // ✅ NEU: GameMode-zu-Punkte Mapping für Frontend-Kompatibilität
    private int GetGamePointsFromGameMode(GameMode gameMode) =>
        gameMode switch
        {
            GameMode.Points301 => 301,
            GameMode.Points401 => 401,
            GameMode.Points501 => 501,
            _ => 501 // Default fallback
        };

    // ✅ NEU: GameMode-zu-String Mapping für Frontend
    private string GetGameTypeString(GameMode gameMode) =>
        gameMode switch
        {
            GameMode.Points301 => "301",
            GameMode.Points401 => "401", 
            GameMode.Points501 => "501",
            _ => "501"
        };

    // ✅ NEU: FinishMode-zu-String Mapping für Frontend
    private string GetFinishTypeString(FinishMode finishMode) =>
        finishMode switch
        {
            FinishMode.SingleOut => "SingleOut",
            FinishMode.DoubleOut => "DoubleOut",
            _ => "DoubleOut"
        };

    // ✅ NEU: Format-Beschreibung für Benutzeroberfläche
    private string GetFormatDescription(GameRules gameRules)
    {
        var gameType = GetGameTypeString(gameRules.GameMode);
        var finishType = GetFinishTypeString(gameRules.FinishMode);
        var setsFormat = gameRules.PlayWithSets ? 
            $"First to {gameRules.SetsToWin} Sets ({gameRules.LegsPerSet} Legs per Set)" :
            $"First to {gameRules.LegsToWin} Legs";
        
        return $"{gameType} {finishType}, {setsFormat}";
    }
}