using Microsoft.AspNetCore.SignalR;
using DartTournamentPlaner.API.Services;
using DartTournamentPlaner.API.Models;

namespace DartTournamentPlaner.API.Hubs;

/// <summary>
/// SignalR Hub für Real-Time Tournament Updates
/// </summary>
public class TournamentHub : Hub
{
    private readonly ITournamentSyncService _syncService;

    public TournamentHub(ITournamentSyncService syncService)
    {
        _syncService = syncService;
    }

    public async Task JoinTournament(dynamic data)
    {
        string tournamentId = data.tournamentId;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tournament_{tournamentId}");
        
        // Sende Tournament-Daten an den neuen Client
        if (_syncService.IsApiRunning)
        {
            var tournamentData = _syncService.GetCurrentTournamentData();
            if (tournamentData != null)
            {
                await Clients.Caller.SendAsync("tournament-joined", new { 
                    success = true, 
                    tournament = new { 
                        id = tournamentData.GetHashCode().ToString(),
                        name = $"Tournament {DateTime.Now:yyyy-MM-dd HH:mm}"
                    }
                });
            }
        }
    }

    public async Task LeaveTournamentGroup(string tournamentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tournament_{tournamentId}");
        await Clients.Group($"tournament_{tournamentId}").SendAsync("UserLeft", Context.ConnectionId);
    }

    /// <summary>
    /// 🚨 ERWEITERTE METHODE: Empfängt Match-Ergebnisse MIT ALLEN MATCH-TYPES
    /// </summary>
    [HubMethodName("submit-match-result")]
    public async Task SubmitMatchResult(dynamic data)
    {
        try
        {
            string tournamentId = data.tournamentId;
            int matchId = data.matchId;
            var result = data.result;
            
            // 🚨 ERWEITERT: Extrahiere ALLE Match-Informationen aus den empfangenen Daten
            int? classId = data.classId;
            string? className = data.className;
            int? groupId = data.groupId;
            string? groupName = data.groupName;
            string? matchType = data.matchType ?? "Group"; // NEUE: Match-Type Information

            Console.WriteLine($"🎯 [TOURNAMENT_HUB] Received match result submission:");
            Console.WriteLine($"   Tournament: {tournamentId}");
            Console.WriteLine($"   Match ID: {matchId}");
            Console.WriteLine($"   🎮 Match Type: {matchType}"); // NEUE: Match-Type Logging
            Console.WriteLine($"   📚 Class: {className} (ID: {classId})");
            Console.WriteLine($"   📋 Group: {groupName} (ID: {groupId})");

            // NEUE: Match-Type spezifische Validierung
            var validMatchTypes = new[] { 
                "Group", "Finals", 
                "Knockout-WB-Best64", "Knockout-WB-Best32", "Knockout-WB-Best16", 
                "Knockout-WB-Quarterfinal", "Knockout-WB-Semifinal", "Knockout-WB-Final", "Knockout-WB-GrandFinal",
                "Knockout-LB-LoserRound1", "Knockout-LB-LoserRound2", "Knockout-LB-LoserRound3", 
                "Knockout-LB-LoserRound4", "Knockout-LB-LoserRound5", "Knockout-LB-LoserRound6", "Knockout-LB-LoserFinal"
            };

            if (!validMatchTypes.Contains(matchType))
            {
                Console.WriteLine($"⚠️ [TOURNAMENT_HUB] Unknown match type: {matchType} - proceeding with caution");
            }

            // ERWEITERT: Detaillierte Result-Information mit Match-Type
            Console.WriteLine($"📊 [TOURNAMENT_HUB] Match Result Details:");
            Console.WriteLine($"   Player 1: {result.player1Sets ?? 0} Sets, {result.player1Legs ?? 0} Legs");
            Console.WriteLine($"   Player 2: {result.player2Sets ?? 0} Sets, {result.player2Legs ?? 0} Legs");
            Console.WriteLine($"   Status: {result.status ?? "Finished"}");
            Console.WriteLine($"   Notes: {result.notes ?? "None"}");
            Console.WriteLine($"   🎮 Match Context: {className}/{matchType}/{groupName ?? "NoGroup"}");

            // ERWEITERTE Match-Type spezifische Verarbeitung
            var matchTypeCategory = GetMatchTypeCategory(matchType);
            Console.WriteLine($"🏷️ [TOURNAMENT_HUB] Match Category: {matchTypeCategory}");

            // Erstelle erweiterte Match-Result mit ALLEN Informationen
            var extendedResult = new
            {
                matchId = matchId,
                classId = classId,
                className = className,
                groupId = groupId,
                groupName = groupName,
                matchType = matchType, // NEUE: Match-Type Information
                matchTypeCategory = matchTypeCategory, // NEUE: Match-Type Kategorie
                player1Sets = result.player1Sets ?? 0,
                player2Sets = result.player2Sets ?? 0,
                player1Legs = result.player1Legs ?? 0,
                player2Legs = result.player2Legs ?? 0,
                status = result.status ?? "Finished",
                notes = result.notes ?? "",
                submittedAt = DateTime.UtcNow,
                // NEUE: Match-Type spezifische Metadaten
                tournamentPhase = GetTournamentPhaseFromMatchType(matchType),
                isKnockoutMatch = matchType.StartsWith("Knockout"),
                isFinalMatch = matchType == "Finals" || matchType.Contains("Final"),
                bracketType = GetBracketTypeFromMatchType(matchType)
            };

            Console.WriteLine($"📋 [TOURNAMENT_HUB] Extended result prepared:");
            Console.WriteLine($"   Tournament Phase: {extendedResult.tournamentPhase}");
            Console.WriteLine($"   Is Knockout: {extendedResult.isKnockoutMatch}");
            Console.WriteLine($"   Is Final: {extendedResult.isFinalMatch}");
            Console.WriteLine($"   Bracket Type: {extendedResult.bracketType}");

            // Sende an Sync-Service (erweitert)
            if (_syncService?.IsApiRunning == true)
            {
                try
                {
                    var matchResultDto = new MatchResultDto
                    {
                        Player1Sets = extendedResult.player1Sets,
                        Player2Sets = extendedResult.player2Sets,
                        Player1Legs = extendedResult.player1Legs,
                        Player2Legs = extendedResult.player2Legs,
                        Notes = extendedResult.notes
                    };

                    Console.WriteLine($"🔄 [TOURNAMENT_HUB] Forwarding {matchType} match {matchId} to sync service...");
                    _syncService.UpdateMatchResult(matchId, classId ?? 1, matchResultDto);
                    Console.WriteLine($"✅ [TOURNAMENT_HUB] {matchType} match result forwarded to sync service");
                }
                catch (Exception syncEx)
                {
                    Console.WriteLine($"❌ [TOURNAMENT_HUB] Error forwarding {matchType} match to sync service: {syncEx.Message}");
                }
            }

            // ERWEITERTE Broadcast-Nachricht mit ALLEN Match-Informationen
            var broadcastMessage = new
            {
                type = "tournament-match-updated",
                tournamentId = tournamentId,
                matchId = matchId,
                result = extendedResult,
                // NEUE: Match-Type spezifische Broadcast-Informationen
                matchType = matchType,
                matchTypeCategory = matchTypeCategory,
                className = className,
                groupName = groupName,
                updateContext = new
                {
                    source = "web-interface",
                    timestamp = DateTime.UtcNow,
                    phase = extendedResult.tournamentPhase,
                    bracket = extendedResult.bracketType
                },
                // NEUE: Client-spezifische Informationen für verschiedene Match-Types
                displayInfo = new
                {
                    matchTitle = GetMatchDisplayTitle(matchType, className, groupName),
                    phaseIcon = GetPhaseIcon(matchType),
                    bracketIcon = GetBracketIcon(matchType),
                    statusColor = GetMatchTypeColor(matchType)
                }
            };

            Console.WriteLine($"📡 [TOURNAMENT_HUB] Broadcasting {matchType} match update to tournament group...");
            Console.WriteLine($"🎯 [TOURNAMENT_HUB] Match Display Title: {broadcastMessage.displayInfo.matchTitle}");

            // Sende an ALLE Tournament-Teilnehmer
            await Clients.Group($"tournament_{tournamentId}").SendAsync("tournament-match-updated", broadcastMessage);

            // ERWEITERTE Planner-Benachrichtigung mit Match-Type Informationen
            var plannerMessage = new
            {
                type = "planner-match-acknowledged",
                tournamentId = tournamentId,
                matchId = matchId,
                matchType = matchType, // NEUE: Match-Type für Planner
                classId = classId,
                className = className,
                groupName = groupName,
                timestamp = DateTime.UtcNow,
                plannerInfo = new
                {
                    plannerCount = 1,
                    acknowledgedAt = DateTime.UtcNow,
                    matchContext = $"{className}/{matchType}/{groupName ?? "NoGroup"}",
                    phaseInfo = extendedResult.tournamentPhase
                }
            };

            Console.WriteLine($"📬 [TOURNAMENT_HUB] Sending planner acknowledgment for {matchType} match...");
            await Clients.Caller.SendAsync("planner-match-acknowledged", plannerMessage);

            // NEUE: Success Response mit erweiterten Informationen
            var successResponse = new
            {
                success = true,
                message = $"Match result received and processed for {matchType} match",
                matchId = matchId,
                matchType = matchType,
                tournamentPhase = extendedResult.tournamentPhase,
                processed = new
                {
                    syncedToPlanner = _syncService?.IsApiRunning == true,
                    broadcastSent = true,
                    timestamp = DateTime.UtcNow
                }
            };

            await Clients.Caller.SendAsync("result-submitted", successResponse);

            Console.WriteLine($"✅ [TOURNAMENT_HUB] {matchType} match result processing complete!");
            Console.WriteLine($"🎯 [TOURNAMENT_HUB] Match {matchId} in {className}/{matchType} successfully processed");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [TOURNAMENT_HUB] Error processing match result: {ex.Message}");
            Console.WriteLine($"❌ [TOURNAMENT_HUB] Stack trace: {ex.StackTrace}");

            var errorResponse = new
            {
                success = false,
                error = ex.Message,
                timestamp = DateTime.UtcNow
            };

            await Clients.Caller.SendAsync("error", errorResponse);
        }
    }

    // NEUE HILFSMETHODEN für Match-Type spezifische Behandlung

    /// <summary>
    /// Bestimmt die Match-Type Kategorie
    /// </summary>
    private string GetMatchTypeCategory(string matchType)
    {
        if (string.IsNullOrEmpty(matchType)) return "Group";
        
        if (matchType == "Group") return "GroupPhase";
        if (matchType == "Finals") return "Finals";
        if (matchType.StartsWith("Knockout-WB")) return "WinnerBracket";
        if (matchType.StartsWith("Knockout-LB")) return "LoserBracket";
        
        return "Unknown";
    }

    /// <summary>
    /// Bestimmt die Tournament-Phase basierend auf Match-Type
    /// </summary>
    private string GetTournamentPhaseFromMatchType(string matchType)
    {
        if (string.IsNullOrEmpty(matchType)) return "GroupPhase";
        
        return matchType switch
        {
            "Group" => "GroupPhase",
            "Finals" => "Finals",
            var mt when mt.StartsWith("Knockout") => "KnockoutPhase",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Bestimmt den Bracket-Type basierend auf Match-Type
    /// </summary>
    private string GetBracketTypeFromMatchType(string matchType)
    {
        if (string.IsNullOrEmpty(matchType)) return "None";
        
        if (matchType.StartsWith("Knockout-WB")) return "WinnerBracket";
        if (matchType.StartsWith("Knockout-LB")) return "LoserBracket";
        if (matchType == "Finals") return "Finals";
        if (matchType == "Group") return "Group";
        
        return "Unknown";
    }

    /// <summary>
    /// Erstellt Match-Display-Titel basierend auf Match-Type
    /// </summary>
    private string GetMatchDisplayTitle(string matchType, string className, string groupName)
    {
        var typeDescription = matchType switch
        {
            "Group" => "Gruppen-Match",
            "Finals" => "Finalrunden-Match",
            "Knockout-WB-Best64" => "Winner Bracket - Beste 64",
            "Knockout-WB-Best32" => "Winner Bracket - Beste 32",
            "Knockout-WB-Best16" => "Winner Bracket - Beste 16",
            "Knockout-WB-Quarterfinal" => "Winner Bracket - Viertelfinale",
            "Knockout-WB-Semifinal" => "Winner Bracket - Halbfinale",
            "Knockout-WB-Final" => "Winner Bracket - Finale",
            "Knockout-WB-GrandFinal" => "Winner Bracket - Grand Final",
            "Knockout-LB-LoserRound1" => "Loser Bracket - Runde 1",
            "Knockout-LB-LoserRound2" => "Loser Bracket - Runde 2",
            "Knockout-LB-LoserRound3" => "Loser Bracket - Runde 3",
            "Knockout-LB-LoserRound4" => "Loser Bracket - Runde 4",
            "Knockout-LB-LoserRound5" => "Loser Bracket - Runde 5",
            "Knockout-LB-LoserRound6" => "Loser Bracket - Runde 6",
            "Knockout-LB-LoserFinal" => "Loser Bracket - Final",
            _ => matchType
        };

        return $"{className} - {typeDescription}" + (groupName != null && !groupName.StartsWith("Winner") && !groupName.StartsWith("Loser") && groupName != "Finals" ? $" ({groupName})" : "");
    }

    /// <summary>
    /// Gibt das entsprechende Icon für die Tournament-Phase zurück
    /// </summary>
    private string GetPhaseIcon(string matchType)
    {
        return matchType switch
        {
            "Group" => "🔸",
            "Finals" => "🏆",
            var mt when mt.StartsWith("Knockout-WB") => "⚡",
            var mt when mt.StartsWith("Knockout-LB") => "🔄",
            _ => "🎯"
        };
    }

    /// <summary>
    /// Gibt das entsprechende Icon für den Bracket-Type zurück
    /// </summary>
    private string GetBracketIcon(string matchType)
    {
        return matchType switch
        {
            "Group" => "👥",
            "Finals" => "🏅",
            var mt when mt.StartsWith("Knockout-WB") => "🏆",
            var mt when mt.StartsWith("Knockout-LB") => "🔄",
            _ => "⚽"
        };
    }

    /// <summary>
    /// Gibt die entsprechende Farbe für den Match-Type zurück
    /// </summary>
    private string GetMatchTypeColor(string matchType)
    {
        return matchType switch
        {
            "Group" => "#4299e1", // Blau für Gruppen
            "Finals" => "#d69e2e", // Gold für Finals
            var mt when mt.StartsWith("Knockout-WB") => "#38a169", // Grün für Winner Bracket
            var mt when mt.StartsWith("Knockout-LB") => "#e53e3e", // Rot für Loser Bracket
            _ => "#718096" // Grau für Unknown
        };
    }
}