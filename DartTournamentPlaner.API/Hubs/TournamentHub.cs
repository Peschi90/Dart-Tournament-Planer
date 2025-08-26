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
    /// ?? NEUE METHODE: Empfängt Match-Ergebnisse MIT GROUP-INFORMATION
    /// </summary>
    [HubMethodName("submit-match-result")]
    public async Task SubmitMatchResult(dynamic data)
    {
        try
        {
            string tournamentId = data.tournamentId;
            int matchId = data.matchId;
            var result = data.result;
            
            // ?? WICHTIG: Extrahiere Group-Information aus den empfangenen Daten
            int? classId = data.classId;
            string? className = data.className;
            int? groupId = data.groupId;
            string? groupName = data.groupName;
            string? matchType = data.matchType ?? "Group";

            Console.WriteLine($"?? [TOURNAMENT_HUB] Received match result submission:");
            Console.WriteLine($"   Tournament: {tournamentId}");
            Console.WriteLine($"   Match ID: {matchId}");
            Console.WriteLine($"   Class: {className} (ID: {classId})");
            Console.WriteLine($"   Group: {groupName} (ID: {groupId})");
            Console.WriteLine($"   Match Type: {matchType}");
            Console.WriteLine($"   Score: {result.player1Sets}-{result.player2Sets} Sets, {result.player1Legs}-{result.player2Legs} Legs");

            if (!_syncService.IsApiRunning)
            {
                await Clients.Caller.SendAsync("planner-match-error", new { 
                    matchId = matchId, 
                    error = "Keine Verbindung zum Tournament Planner" 
                });
                return;
            }

            // Erstelle MatchResultDto MIT Group-Information
            var matchResult = new MatchResultDto
            {
                MatchId = matchId,
                Player1Sets = (int)(result.player1Sets ?? 0),
                Player2Sets = (int)(result.player2Sets ?? 0),
                Player1Legs = (int)(result.player1Legs ?? 0),
                Player2Legs = (int)(result.player2Legs ?? 0),
                Notes = result.notes?.ToString() ?? "",
                
                // ?? KRITISCH: Group-Information hinzufügen
                ClassId = classId,
                ClassName = className?.ToString(),
                GroupId = groupId,
                GroupName = groupName?.ToString(),
                MatchType = matchType?.ToString()
            };

            Console.WriteLine($"?? [TOURNAMENT_HUB] Processing match result with group info:");
            Console.WriteLine($"   Target Group: '{matchResult.GroupName}' (ID: {matchResult.GroupId})");
            Console.WriteLine($"   Target Class: '{matchResult.ClassName}' (ID: {matchResult.ClassId})");

            // Verarbeite das Update über den Sync Service MIT Group-Information
            if (classId.HasValue)
            {
                _syncService.ProcessMatchResultUpdate(matchId, classId.Value, matchResult);
                
                // Bestätige dem Client das erfolgreiche Update
                await Clients.Caller.SendAsync("planner-match-acknowledged", new { 
                    matchId = matchId,
                    plannerInfo = new {
                        plannerCount = 1,
                        groupName = groupName,
                        className = className,
                        timestamp = DateTime.Now
                    }
                });

                Console.WriteLine($"? [TOURNAMENT_HUB] Successfully processed match {matchId} for group '{groupName}'");
            }
            else
            {
                Console.WriteLine($"? [TOURNAMENT_HUB] Missing class ID for match {matchId}");
                await Clients.Caller.SendAsync("planner-match-error", new { 
                    matchId = matchId, 
                    error = "Fehlende Klassen-Information" 
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [TOURNAMENT_HUB] Error processing match result: {ex.Message}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            
            await Clients.Caller.SendAsync("planner-match-error", new { 
                matchId = data?.matchId, 
                error = ex.Message 
            });
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"?? [TOURNAMENT_HUB] Client connected: {Context.ConnectionId}");
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"?? [TOURNAMENT_HUB] Client disconnected: {Context.ConnectionId}");
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}