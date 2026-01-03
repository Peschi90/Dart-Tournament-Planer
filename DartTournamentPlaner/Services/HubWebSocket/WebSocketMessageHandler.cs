using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services.PowerScore;

namespace DartTournamentPlaner.Services.HubWebSocket;

/// <summary>
/// Verarbeitet eingehende WebSocket-Nachrichten vom Tournament Hub
/// ERWEITERT: Unterstützt Match-Start, Leg-Complete und Match-Progress Events
/// </summary>
public class WebSocketMessageHandler
{
    private readonly Action<string, string> _debugLog;
    private readonly Func<JsonElement, Task> _matchUpdateAcknowledgment;
    private readonly Func<string, Task> _errorAcknowledgment;

    // Events
    public event Action<HubMatchUpdateEventArgs>? MatchResultReceived;
  public event Action<string, object>? TournamentUpdateReceived;
    
    // ? NEU: Zusätzliche Events für Live-Updates
    public event Action<HubMatchUpdateEventArgs>? MatchStarted;
    public event Action<HubMatchUpdateEventArgs>? LegCompleted;
    public event Action<HubMatchUpdateEventArgs>? MatchProgressUpdated;

    // ? NEU: Planner Tournament Fetch Events
    public event Action<PlannerTournamentsResponse>? PlannerTournamentsReceived;
    public event Action<PlannerFetchErrorResponse>? PlannerFetchFailed;

    /// <summary>
    /// ? NEU: Event für PowerScoring Messages
    /// </summary>
    public event EventHandler<PowerScoringHubMessage>? PowerScoringMessageReceived;

    // Aggregation for planner chunking
    private readonly Dictionary<string, PlannerChunkBuffer> _plannerChunkBuffers = new();

    public WebSocketMessageHandler(
        Action<string, string> debugLog, 
        Func<JsonElement, Task> matchUpdateAcknowledgment,
        Func<string, Task> errorAcknowledgment)
    {
        _debugLog = debugLog;
        _matchUpdateAcknowledgment = matchUpdateAcknowledgment;
        _errorAcknowledgment = errorAcknowledgment;
    }

    /// <summary>
    /// Verarbeitet WebSocket-Nachrichten
    /// </summary>
    public async Task ProcessWebSocketMessage(string messageJson)
    {
        try
        {
            // ? ERWEITERT: Logge rohe Nachricht
            _debugLog($"?? [WS-MESSAGE] ===== RAW MESSAGE RECEIVED =====", "WEBSOCKET");
            _debugLog($"?? [WS-MESSAGE] Length: {messageJson.Length} characters", "WEBSOCKET");
            _debugLog($"?? [WS-MESSAGE] Content: {messageJson}", "WEBSOCKET");

            var message = JsonSerializer.Deserialize<JsonElement>(messageJson);

            if (message.TryGetProperty("type", out var typeElement))
            {
                var messageType = typeElement.GetString();
                
                _debugLog($"?? [WS-MESSAGE] Type: {messageType}", "WEBSOCKET");
                _debugLog($"?? [WS-MESSAGE] ================================", "WEBSOCKET");

                switch (messageType)
                {
                    case "welcome":
                        HandleWelcomeMessage(message);
                        break;
                    case "subscription-confirmed":
                        HandleSubscriptionConfirmed(message);
                        break;
                    case "planner-registration-confirmed":
                        HandlePlannerRegistrationConfirmed(message);
                        break;
                    case "planner-tournaments-data":
                        HandlePlannerTournamentsData(message);
                        break;
                    case "planner-tournaments-data-chunk":
                        HandlePlannerTournamentsChunk(message);
                        break;
                    case "planner-tournaments-data-end":
                        HandlePlannerTournamentsEnd(message);
                        break;
                    case "planner-fetch-error":
                        HandlePlannerFetchError(message);
                        break;
                    case "tournament-match-updated":
                        await HandleTournamentMatchUpdate(message);
                        break;
                    // ? NEU: Match-Start Handler
                    case "match-started":
                        await HandleMatchStarted(message);
                        break;
                    // ? NEU: Leg-Completed Handler
                    case "leg-completed":
                        await HandleLegCompleted(message);
                        break;
                    // ? NEU: Match-Progress Handler
                    case "match-in-progress":
                        await HandleMatchProgress(message);
                        break;
                    // ? NEU: PowerScoring Messages
                    case "power-scoring-update":
                        await HandlePowerScoringUpdate(message);
                        break;
                    case "power-scoring-progress":
                        await HandlePowerScoringProgress(message);
                        break;
                    case "power-scoring-result":
                        await HandlePowerScoringResult(message);
                        break;
                    case "heartbeat-ack":
                        HandleHeartbeatAck(message);
                        break;
                    case "error":
                        HandleErrorMessage(message);
                        break;
                    default:
                        _debugLog($"?? [WS-MESSAGE] Unknown message type: {messageType}", "WARNING");
                        _debugLog($"?? [WS-MESSAGE] Full message: {messageJson}", "WARNING");
                        break;
                }
            }
            else
            {
                _debugLog($"?? [WS-MESSAGE] Message has no 'type' property", "WARNING");
                _debugLog($"?? [WS-MESSAGE] Full message: {messageJson}", "WARNING");
            }
        }
        catch (JsonException jsonEx)
        {
            _debugLog($"? [WS-MESSAGE] JSON parsing error: {jsonEx.Message}", "ERROR");
            _debugLog($"? [WS-MESSAGE] Invalid JSON: {messageJson}", "ERROR");
        }
        catch (Exception ex)
        {
            _debugLog($"? [WS-MESSAGE] Error processing WebSocket message: {ex.Message}", "ERROR");
            _debugLog($"? [WS-MESSAGE] Stack trace: {ex.StackTrace}", "ERROR");
        }
    }

    /// <summary>
    /// Behandelt Welcome-Nachrichten
    /// </summary>
    private void HandleWelcomeMessage(JsonElement message)
    {
        var clientId = message.TryGetProperty("clientId", out var clientIdElement) ? clientIdElement.GetString() : "unknown";
        _debugLog($"?? [WS-MESSAGE] Welcome message received (ID: {clientId})", "WEBSOCKET");
    }

    /// <summary>
    /// Behandelt Fetch-Ergebnisse für Planner-Turniere
    /// </summary>
    private void HandlePlannerTournamentsData(JsonElement message)
    {
        try
        {
            var response = JsonSerializer.Deserialize<PlannerTournamentsResponse>(message.GetRawText());
            if (response != null)
            {
                _debugLog($"?? [WS-MESSAGE] Received planner tournaments data: {response.Tournaments?.Count ?? 0} tournaments", "SUCCESS");
                PlannerTournamentsReceived?.Invoke(response);

                // Reset potential chunk buffer for this license/days
                var key = BuildPlannerChunkKey(response.LicenseKey, response.Days);
                _plannerChunkBuffers.Remove(key);
            }
            else
            {
                _debugLog("? [WS-MESSAGE] Planner tournaments data could not be parsed", "ERROR");
            }
        }
        catch (Exception ex)
        {
            _debugLog($"? [WS-MESSAGE] Error handling planner tournaments data: {ex.Message}", "ERROR");
        }
    }

    private void HandlePlannerTournamentsChunk(JsonElement message)
    {
        try
        {
            var licenseKey = message.TryGetProperty("licenseKey", out var licenseEl) ? licenseEl.GetString() : string.Empty;
            var days = message.TryGetProperty("days", out var daysEl) && daysEl.ValueKind == JsonValueKind.Number ? daysEl.GetInt32() : 14;
            var index = message.TryGetProperty("index", out var idxEl) && idxEl.ValueKind == JsonValueKind.Number ? idxEl.GetInt32() : -1;
            var total = message.TryGetProperty("total", out var totalEl) && totalEl.ValueKind == JsonValueKind.Number ? totalEl.GetInt32() : 0;

            if (index < 0 || total <= 0)
            {
                _debugLog($"? [WS-MESSAGE] Invalid planner chunk index/total: index={index}, total={total}", "ERROR");
                return;
            }

            var key = BuildPlannerChunkKey(licenseKey, days);
            if (!_plannerChunkBuffers.TryGetValue(key, out var buffer))
            {
                buffer = new PlannerChunkBuffer(total);
                _plannerChunkBuffers[key] = buffer;
                _debugLog($"?? [WS-MESSAGE] Init planner chunk buffer: total={total}, key={key}", "INFO");
            }
            else if (buffer.Total != total)
            {
                _debugLog($"? [WS-MESSAGE] Planner chunk total mismatch: existing={buffer.Total}, incoming={total}", "WARNING");
                buffer.Total = total;
            }

            if (index >= buffer.Total)
            {
                _debugLog($"? [WS-MESSAGE] Planner chunk index out of range: {index} >= {buffer.Total}", "ERROR");
                return;
            }

            if (message.TryGetProperty("tournament", out var tournamentElement))
            {
                var tournament = JsonSerializer.Deserialize<PlannerTournamentSummary>(tournamentElement.GetRawText());
                buffer.Store(index, tournament);
                _debugLog($"?? [WS-MESSAGE] Planner chunk stored index={index}/{buffer.Total - 1}", "INFO");
            }
            else
            {
                _debugLog("? [WS-MESSAGE] Planner chunk missing tournament payload", "ERROR");
            }
        }
        catch (Exception ex)
        {
            _debugLog($"? [WS-MESSAGE] Error handling planner chunk: {ex.Message}", "ERROR");
        }
    }

    private void HandlePlannerTournamentsEnd(JsonElement message)
    {
        try
        {
            var licenseKey = message.TryGetProperty("licenseKey", out var licenseEl) ? licenseEl.GetString() : string.Empty;
            var days = message.TryGetProperty("days", out var daysEl) && daysEl.ValueKind == JsonValueKind.Number ? daysEl.GetInt32() : 14;
            var total = message.TryGetProperty("total", out var totalEl) && totalEl.ValueKind == JsonValueKind.Number ? totalEl.GetInt32() : 0;

            var key = BuildPlannerChunkKey(licenseKey, days);
            if (!_plannerChunkBuffers.TryGetValue(key, out var buffer))
            {
                _debugLog($"? [WS-MESSAGE] Planner end received but no buffer for key={key}", "WARNING");
                return;
            }

            if (total > 0 && buffer.Total != total)
            {
                _debugLog($"? [WS-MESSAGE] Planner end total mismatch: buffer={buffer.Total}, end={total}", "WARNING");
                buffer.Total = total;
            }

            if (!buffer.IsComplete)
            {
                _debugLog($"? [WS-MESSAGE] Planner chunks incomplete: {buffer.Received}/{buffer.Total}", "ERROR");
                _plannerChunkBuffers.Remove(key);
                return;
            }

            var response = new PlannerTournamentsResponse
            {
                LicenseKey = licenseKey,
                Days = days,
                Tournaments = buffer.GetTournaments()
            };

            _plannerChunkBuffers.Remove(key);
            _debugLog($"?? [WS-MESSAGE] Planner chunks assembled: {response.Tournaments.Count} tournaments", "SUCCESS");
            PlannerTournamentsReceived?.Invoke(response);
        }
        catch (Exception ex)
        {
            _debugLog($"? [WS-MESSAGE] Error handling planner chunk end: {ex.Message}", "ERROR");
        }
    }

    private static string BuildPlannerChunkKey(string? licenseKey, int days) => $"{licenseKey ?? ""}__{days}";

    private class PlannerChunkBuffer
    {
        private readonly List<PlannerTournamentSummary?> _items;

        public int Total { get; set; }
        public int Received { get; private set; }

        public PlannerChunkBuffer(int total)
        {
            Total = Math.Max(1, total);
            _items = Enumerable.Repeat<PlannerTournamentSummary?>(null, Total).ToList();
            Received = 0;
        }

        public void Store(int index, PlannerTournamentSummary? item)
        {
            if (index < 0 || index >= Total) return;
            if (_items[index] == null)
            {
                Received++;
            }
            _items[index] = item;
        }

        public bool IsComplete => Received == Total && _items.All(x => x != null);

        public List<PlannerTournamentSummary> GetTournaments()
        {
            return _items.Where(x => x != null).Select(x => x!).ToList();
        }
    }

    /// <summary>
    /// Behandelt Error-Nachrichten
  /// </summary>
    private void HandleErrorMessage(JsonElement message)
    {
        var error = message.TryGetProperty("error", out var errorElement) ? errorElement.GetString() : "Unknown error";
        _debugLog($"? [WS-MESSAGE] Server Error: {error}", "ERROR");
    }

    /// <summary>
    /// Behandelt Heartbeat-Acknowledgments
    /// </summary>
    private void HandleHeartbeatAck(JsonElement message)
    {
        _debugLog("?? [WS-MESSAGE] Heartbeat acknowledged", "WEBSOCKET");
    }

    /// <summary>
    /// Behandelt Subscription-Confirmations
    /// </summary>
    private void HandleSubscriptionConfirmed(JsonElement message)
    {
        try
        {
            var tournamentId = message.TryGetProperty("tournamentId", out var idElement) ? idElement.GetString() : "unknown";
            _debugLog($"?? [WS-MESSAGE] Subscription confirmed for tournament: {tournamentId}", "WEBSOCKET");

            TournamentUpdateReceived?.Invoke(tournamentId ?? "unknown", message);
        }
        catch (Exception ex)
        {
            _debugLog($"? [WS-MESSAGE] Error handling subscription confirmation: {ex.Message}", "ERROR");
        }
    }

    /// <summary>
    /// Behandelt Planner-Registration-Confirmationen
    /// </summary>
    private void HandlePlannerRegistrationConfirmed(JsonElement message)
    {
        try
        {
            var success = message.TryGetProperty("success", out var successElement) && successElement.GetBoolean();
            var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdElement) ? tournamentIdElement.GetString() : "unknown";

            // ? ERWEITERT: Detailliertes Logging mit vollständiger Message
            _debugLog($"?? [WS-MESSAGE] ===== PLANNER REGISTRATION =====", "INFO");
            _debugLog($"?? [WS-MESSAGE] Success: {success}", "INFO");
            _debugLog($"?? [WS-MESSAGE] Tournament ID: {tournamentId}", "INFO");
            _debugLog($"?? [WS-MESSAGE] Full message: {message.ToString()}", "INFO");
            
            if (success)
            {
                _debugLog($"? [WS-MESSAGE] Successfully registered as Planner for {tournamentId}", "SUCCESS");
            }
            else
            {
                _debugLog($"? [WS-MESSAGE] Planner registration failed for {tournamentId}", "ERROR");
            }
            
            _debugLog($"?? [WS-MESSAGE] ================================", "INFO");
        }
        catch (Exception ex)
        {
            _debugLog($"? [WS-MESSAGE] Error handling planner registration: {ex.Message}", "ERROR");
        }
    }

 /// <summary>
    /// Behandelt Tournament Match Updates
    /// </summary>
    private async Task HandleTournamentMatchUpdate(JsonElement message)
    {
        try
        {
_debugLog("?? [WS-MESSAGE] ===== MATCH UPDATE RECEIVED =====", "MATCH_RESULT");
    _debugLog($"?? [WS-MESSAGE] Raw tournament match update message:", "MATCH_RESULT");
    _debugLog($"?? [WS-MESSAGE] {JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true })}", "MATCH_RESULT");

    // Sende sofortige Empfangsbestätigung an Server
      await _matchUpdateAcknowledgment(message);

        var isMatchResult = message.TryGetProperty("matchResultHighlight", out var highlightElement) && highlightElement.GetBoolean();
     var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdEl) ? tournamentIdEl.GetString() : "unknown";

  _debugLog($"?? [WS-MESSAGE] Tournament Match Update for: {tournamentId}", "MATCH_RESULT");
     _debugLog($"?? [WS-MESSAGE] Match Result Highlight: {isMatchResult}", "MATCH_RESULT");

            // Extrahiere Match Update Daten
 var matchUpdate = ExtractMatchUpdateData(message, isMatchResult);
       if (matchUpdate != null)
            {
        _debugLog("?? [WS-MESSAGE] TRIGGERING OnMatchResultReceivedFromHub EVENT", "MATCH_RESULT");
      _debugLog($"?? [WS-MESSAGE] Event subscribers count: {MatchResultReceived?.GetInvocationList()?.Length ?? 0}", "MATCH_RESULT");

  MatchResultReceived?.Invoke(matchUpdate);

     _debugLog("? [WS-MESSAGE] Match update event triggered successfully!", "SUCCESS");
         _debugLog("?? [WS-MESSAGE] Match update forwarded to Tournament Planner UI", "SUCCESS");
            }
            else
{
          _debugLog("?? [WS-MESSAGE] No match update data found in message", "WARNING");
      _debugLog($"?? [WS-MESSAGE] Available properties: {string.Join(", ", GetJsonProperties(message))}", "WARNING");
      }

            _debugLog("?? [WS-MESSAGE] ===== MATCH UPDATE PROCESSING COMPLETE =====", "MATCH_RESULT");
        }
        catch (Exception ex)
    {
       _debugLog($"? [WS-MESSAGE] Error handling tournament match update: {ex.Message}", "ERROR");
     _debugLog($"? [WS-MESSAGE] Stack trace: {ex.StackTrace}", "ERROR");

            // Sende Fehler-Acknowledgment
            await _errorAcknowledgment(ex.Message);
        }
    }

    /// <summary>
    /// ? NEU: Behandelt Match-Start Nachrichten
    /// </summary>
    private async Task HandleMatchStarted(JsonElement message)
    {
        try
     {
   _debugLog("?? [MATCH-START] Processing match started event...", "WEBSOCKET");

 if (!message.TryGetProperty("data", out var data))
     {
        _debugLog("? [MATCH-START] No data property found", "ERROR");
        return;
   }

     var matchUpdate = ParseMatchUpdateData(data);
          if (matchUpdate != null)
   {
 // ? WICHTIG: Status überschreiben BEVOR Events gefeuert werden
           matchUpdate.IsMatchStarted = true;
       matchUpdate.IsMatchCompleted = false;
        matchUpdate.Status = "InProgress";  // ? Überschreibt den falschen Status vom Hub
       matchUpdate.MatchStartTime = DateTime.UtcNow;
            matchUpdate.Source = "match-started";  // ? CRITICAL FIX: Setze Source explizit!
     
       // Extrahiere Match-Start-Zeit wenn vorhanden
 if (data.TryGetProperty("startedAt", out var startedAtElement))
       {
  if (DateTime.TryParse(startedAtElement.GetString(), out var startedAt))
           {
  matchUpdate.MatchStartTime = startedAt;
      }
    }

      _debugLog($"? [MATCH-START] {matchUpdate.GetMatchIdentificationSummary()} started", "WEBSOCKET");
     _debugLog($"   ?? Status correctly set to: {matchUpdate.Status}", "WEBSOCKET");
            _debugLog($"   ?? Source correctly set to: {matchUpdate.Source}", "WEBSOCKET");

    // ? CRITICAL FIX: Nur das spezifische Event feuern, NICHT MatchResultReceived!
        // MatchResultReceived wird nur für die alten "tournament-match-updated" Messages verwendet
        MatchStarted?.Invoke(matchUpdate);

     // Acknowledgment senden
   await _matchUpdateAcknowledgment(data);
  }
    }
        catch (Exception ex)
  {
   _debugLog($"? [MATCH-START] Error: {ex.Message}", "ERROR");
     await _errorAcknowledgment(ex.Message);
   }
    }

    /// <summary>
    /// ? NEU: Behandelt Leg-Completed Nachrichten
    /// </summary>
    private async Task HandleLegCompleted(JsonElement message)
    {
   try
        {
   _debugLog("?? [LEG-COMPLETE] Processing leg completed event...", "WEBSOCKET");

    if (!message.TryGetProperty("data", out var data))
   {
     _debugLog("? [LEG-COMPLETE] No data property found", "ERROR");
    return;
            }

 var matchUpdate = ParseMatchUpdateData(data);
      if (matchUpdate != null)
            {
    // ? WICHTIG: Setze Flags NACH dem Parsing, um Hub-Status zu überschreiben
    matchUpdate.IsLegUpdate = true;
     matchUpdate.IsMatchStarted = true;
    matchUpdate.IsMatchCompleted = false;  // ? NEU: Explizit auf false setzen
  matchUpdate.Status = "InProgress";  // ? Überschreibt den Status vom Hub
       matchUpdate.Source = "leg-completed";  // ? CRITICAL FIX: Setze Source explizit!
      
         // Extrahiere Leg-Informationen
            if (data.TryGetProperty("currentLeg", out var currentLegElement))
{
      matchUpdate.CurrentLeg = currentLegElement.GetInt32();
    }
    
    if (data.TryGetProperty("totalLegs", out var totalLegsElement))
       {
        matchUpdate.TotalLegs = totalLegsElement.GetInt32();
   }

    // Extrahiere detaillierte Leg-Ergebnisse
     if (data.TryGetProperty("legResults", out var legResultsElement))
   {
   matchUpdate.LegResults = ParseLegResults(legResultsElement);
       }
    
    // Extrahiere aktuelle Leg-Dauer
    if (data.TryGetProperty("currentLegDuration", out var legDurationElement))
     {
if (TimeSpan.TryParse(legDurationElement.GetString(), out var legDuration))
 {
      matchUpdate.CurrentLegDuration = legDuration;
     }
       }

    _debugLog($"? [LEG-COMPLETE] {matchUpdate.GetMatchIdentificationSummary()} - Leg {matchUpdate.CurrentLeg}/{matchUpdate.TotalLegs} completed", "WEBSOCKET");
     _debugLog($"   ?? Score: {matchUpdate.Player1Legs}-{matchUpdate.Player2Legs}", "WEBSOCKET");
 _debugLog($"   ? Status forced to: {matchUpdate.Status} (IsMatchCompleted={matchUpdate.IsMatchCompleted})", "WEBSOCKET");
      _debugLog($"   ?? Source correctly set to: {matchUpdate.Source}", "WEBSOCKET");  // ? NEU: Log Source

  // ? CRITICAL FIX: Nur das spezifische Event feuern, NICHT MatchResultReceived!
        LegCompleted?.Invoke(matchUpdate);

         // Acknowledgment senden
  await _matchUpdateAcknowledgment(data);
     }
        }
      catch (Exception ex)
        {
    _debugLog($"? [LEG-COMPLETE] Error: {ex.Message}", "ERROR");
     await _errorAcknowledgment(ex.Message);
        }
    }

    /// <summary>
    /// ? NEU: Behandelt Match-Progress Nachrichten (optionale Zwischenstände)
    /// </summary>
    private async Task HandleMatchProgress(JsonElement message)
    {
      try
 {
  _debugLog("?? [MATCH-PROGRESS] Processing match progress event...", "WEBSOCKET");

            if (!message.TryGetProperty("data", out var data))
            {
          _debugLog("? [MATCH-PROGRESS] No data property found", "ERROR");
                return;
      }

            var matchUpdate = ParseMatchUpdateData(data);
            if (matchUpdate != null)
            {
       matchUpdate.IsMatchStarted = true;
       matchUpdate.Status = "InProgress";
          matchUpdate.Source = "match-progress";  // ? CRITICAL FIX: Setze Source explizit!
 
 // Extrahiere aktuelle Leg-Scores (falls verfügbar)
     if (data.TryGetProperty("currentPlayer1LegScore", out var p1ScoreElement))
    {
        matchUpdate.CurrentPlayer1LegScore = p1ScoreElement.GetInt32();
     }
        
        if (data.TryGetProperty("currentPlayer2LegScore", out var p2ScoreElement))
      {
     matchUpdate.CurrentPlayer2LegScore = p2ScoreElement.GetInt32();
         }
 
  // Extrahiere Match-Dauer
     if (data.TryGetProperty("matchDuration", out var durationElement))
 {
    if (TimeSpan.TryParse(durationElement.GetString(), out var duration))
    {
   matchUpdate.MatchDuration = duration;
      }
      }

       _debugLog($"? [MATCH-PROGRESS] {matchUpdate.GetMatchIdentificationSummary()} - Progress update", "WEBSOCKET");
         _debugLog($"   ?? Source correctly set to: {matchUpdate.Source}", "WEBSOCKET");// ? NEU: Log Source

     // ? CRITICAL FIX: Nur das spezifische Event feuern, NICHT MatchResultReceived!
     MatchProgressUpdated?.Invoke(matchUpdate);
         
 // Acknowledgment senden
       await _matchUpdateAcknowledgment(data);
      }
  }
        catch (Exception ex)
      {
     _debugLog($"? [MATCH-PROGRESS] Error: {ex.Message}", "ERROR");
       await _errorAcknowledgment(ex.Message);
    }
    }

    /// <summary>
    /// ? NEU: Parst Match-Update Daten für neue Message Types
 /// </summary>
    private HubMatchUpdateEventArgs? ParseMatchUpdateData(JsonElement data)
    {
        try
        {
        var matchUpdate = new HubMatchUpdateEventArgs();

  // Match-ID Extraktion (UUID oder numerisch)
         if (data.TryGetProperty("matchId", out var matchIdElement))
    {
if (matchIdElement.ValueKind == JsonValueKind.String)
       {
    var matchIdString = matchIdElement.GetString();
           matchUpdate.OriginalMatchIdString = matchIdString;
         
  if (matchIdString?.Length > 10)
              {
          matchUpdate.MatchUuid = matchIdString;
 matchUpdate.MatchId = 0;
        }
   else if (int.TryParse(matchIdString, out var numericMatchId))
   {
       matchUpdate.MatchId = numericMatchId;
          }
      }
           else if (matchIdElement.ValueKind == JsonValueKind.Number)
         {
    matchUpdate.MatchId = matchIdElement.GetInt32();
      }
            }
 
            // ? CRITICAL FIX: Extrahiere uniqueId als separates Property!
   // Der Hub sendet die UUID als "uniqueId", nicht als Teil von "matchId"
if (data.TryGetProperty("uniqueId", out var uniqueIdElement))
    {
     var uniqueId = uniqueIdElement.GetString();
                if (!string.IsNullOrEmpty(uniqueId))
      {
 matchUpdate.MatchUuid = uniqueId;
   _debugLog($"? [PARSE] UUID extracted from uniqueId property: {uniqueId}", "SUCCESS");
                }
            }
  
  // Class-ID
      if (data.TryGetProperty("classId", out var classIdElement))
         {
     matchUpdate.ClassId = classIdElement.GetInt32();
            }
            
            // Scores
   if (data.TryGetProperty("player1Sets", out var p1Sets))
     matchUpdate.Player1Sets = p1Sets.GetInt32();
            if (data.TryGetProperty("player2Sets", out var p2Sets))
    matchUpdate.Player2Sets = p2Sets.GetInt32();
            if (data.TryGetProperty("player1Legs", out var p1Legs))
     matchUpdate.Player1Legs = p1Legs.GetInt32();
            if (data.TryGetProperty("player2Legs", out var p2Legs))
                matchUpdate.Player2Legs = p2Legs.GetInt32();
       
// Status
   if (data.TryGetProperty("status", out var statusElement))
  {
      matchUpdate.Status = statusElement.GetString() ?? "NotStarted";
        }
  
  // ? Group-Informationen (mit Null-Handling für KO-Matches)
            if (data.TryGetProperty("groupId", out var groupIdElement))
     {
          // ? Prüfe ob der Wert null ist (bei KO-Matches)
       if (groupIdElement.ValueKind != JsonValueKind.Null)
       {
  matchUpdate.GroupId = groupIdElement.GetInt32();
       }
else
      {
   matchUpdate.GroupId = null;
                }
        }
  if (data.TryGetProperty("groupName", out var groupNameElement))
      {
          matchUpdate.GroupName = groupNameElement.GetString();
  }
   if (data.TryGetProperty("matchType", out var matchTypeElement))
   {
             matchUpdate.MatchType = matchTypeElement.GetString();
       }

     // Timestamp
  matchUpdate.UpdatedAt = DateTime.UtcNow;

            return matchUpdate;
   }
        catch (Exception ex)
        {
            _debugLog($"? [PARSE] Error parsing match data: {ex.Message}", "ERROR");
       return null;
        }
    }

 /// <summary>
    /// ? NEU: Parst Leg-Ergebnisse aus JSON
    /// </summary>
    private List<LegResult> ParseLegResults(JsonElement legResultsElement)
    {
   var legResults = new List<LegResult>();
        
        try
        {
    if (legResultsElement.ValueKind == JsonValueKind.Array)
      {
       foreach (var legElement in legResultsElement.EnumerateArray())
  {
            var legResult = new LegResult
            {
          LegNumber = legElement.TryGetProperty("legNumber", out var legNum) ? legNum.GetInt32() : 0,
        Winner = legElement.TryGetProperty("winner", out var winner) ? winner.GetString() : null,
          Player1Score = legElement.TryGetProperty("player1Score", out var p1Score) ? p1Score.GetInt32() : 0,
     Player2Score = legElement.TryGetProperty("player2Score", out var p2Score) ? p2Score.GetInt32() : 0,
    Player1Darts = legElement.TryGetProperty("player1Darts", out var p1Darts) ? p1Darts.GetInt32() : 0,
        Player2Darts = legElement.TryGetProperty("player2Darts", out var p2Darts) ? p2Darts.GetInt32() : 0
                  };
                
      // Dauer parsen
       if (legElement.TryGetProperty("duration", out var durationElement))
        {
                 if (TimeSpan.TryParse(durationElement.GetString(), out var duration))
   {
             legResult.Duration = duration;
      }
       }
       
         // CompletedAt parsen
          if (legElement.TryGetProperty("completedAt", out var completedAtElement))
                    {
         if (DateTime.TryParse(completedAtElement.GetString(), out var completedAt))
   {
     legResult.CompletedAt = completedAt;
       }
 }
           
        // Optionale Statistiken (mit korrektem Typ-Handling)
             // ? Averages sind Doubles, nicht Ints!
     if (legElement.TryGetProperty("player1Average", out var p1Avg))
             {
      try
           {
    legResult.Player1Average = p1Avg.ValueKind == JsonValueKind.Number
       ? (int)p1Avg.GetDouble()  // Konvertiere Double zu Int
         : 0;
                }
   catch { legResult.Player1Average = 0; }
         }
             
       if (legElement.TryGetProperty("player2Average", out var p2Avg))
    {
             try
         {
           legResult.Player2Average = p2Avg.ValueKind == JsonValueKind.Number
         ? (int)p2Avg.GetDouble()  // Konvertiere Double zu Int
          : 0;
       }
      catch { legResult.Player2Average = 0; }
 }
    
        // Highest Scores und Checkouts sind Integers
    if (legElement.TryGetProperty("player1HighestScore", out var p1High))
        legResult.Player1HighestScore = p1High.GetInt32();
     if (legElement.TryGetProperty("player2HighestScore", out var p2High))
                 legResult.Player2HighestScore = p2High.GetInt32();
            
      // ? Checkouts können null sein (wenn Spieler nicht ausgecheckt hat)
      if (legElement.TryGetProperty("player1Checkout", out var p1Checkout))
 {
          if (p1Checkout.ValueKind != JsonValueKind.Null)
           legResult.Player1Checkout = p1Checkout.GetInt32();
                }
 if (legElement.TryGetProperty("player2Checkout", out var p2Checkout))
                {
 if (p2Checkout.ValueKind != JsonValueKind.Null)
                 legResult.Player2Checkout = p2Checkout.GetInt32();
                }

     legResults.Add(legResult);
                }
        }
        }
        catch (Exception ex)
    {
            _debugLog($"?? [LEG-RESULTS] Error parsing leg results: {ex.Message}", "WARNING");
        }
        
        return legResults;
    }

    /// <summary>
    /// Extrahiert Match Update Daten aus der Nachricht (für tournament-match-updated)
    /// </summary>
    private HubMatchUpdateEventArgs? ExtractMatchUpdateData(JsonElement message, bool isMatchResult)
    {
   // Mehrere Wege um Match Update zu extrahieren
        JsonElement matchUpdateElement = default;
   bool hasMatchUpdate = false;

        // Priorität 1: matchUpdate Property (direkt)
        if (message.TryGetProperty("matchUpdate", out matchUpdateElement))
        {
   hasMatchUpdate = true;
            _debugLog("? [WS-MESSAGE] Found matchUpdate property", "MATCH_RESULT");
        }
        // Priorität 2: Fallback zu result Property
        else if (message.TryGetProperty("result", out matchUpdateElement))
        {
        hasMatchUpdate = true;
            _debugLog("? [WS-MESSAGE] Using result property as matchUpdate fallback", "MATCH_RESULT");
        }
     // Priorität 3: Direkte Match-Daten im Root
        else if (message.TryGetProperty("matchId", out var _))
  {
    matchUpdateElement = message;
       hasMatchUpdate = true;
   _debugLog("? [WS-MESSAGE] Using root message as matchUpdate", "MATCH_RESULT");
        }

        if (!hasMatchUpdate) return null;

   // UUID-aware Match-ID Verarbeitung
        string? matchIdString = null;
  string? matchUuid = null;
  int numericMatchId = 0;

     // 1. Versuche Match ID zu extrahieren
     if (matchUpdateElement.TryGetProperty("matchId", out var matchIdEl))
   {
  matchIdString = matchIdEl.GetString() ?? matchIdEl.ToString();
 }
else if (message.TryGetProperty("matchId", out var rootMatchIdEl))
 {
      matchIdString = rootMatchIdEl.GetString() ?? rootMatchIdEl.ToString();
        }

  // 2. Versuche UUID separat zu extrahieren
      if (matchUpdateElement.TryGetProperty("uniqueId", out var uuidEl))
  {
          matchUuid = uuidEl.GetString();
        }
        else if (matchUpdateElement.TryGetProperty("result", out var resultEl) &&
      resultEl.TryGetProperty("uniqueId", out var resultUuidEl))
        {
   matchUuid = resultUuidEl.GetString();
        }

      // 3. Versuche numerische ID zu extrahieren
        if (matchUpdateElement.TryGetProperty("numericMatchId", out var numericIdEl) &&
            numericIdEl.TryGetInt32(out var numericId))
   {
   numericMatchId = numericId;
        }
        else if (matchUpdateElement.TryGetProperty("result", out var resultElement) &&
         resultElement.TryGetProperty("numericMatchId", out var resultNumericIdEl))
   {
         if (resultNumericIdEl.ValueKind == JsonValueKind.Number &&
         resultNumericIdEl.TryGetInt32(out var resultNumericId))
  {
         numericMatchId = resultNumericId;
          }
        else if (resultNumericIdEl.ValueKind == JsonValueKind.String)
     {
     var numericString = resultNumericIdEl.GetString();
     if (int.TryParse(numericString, out var parsedId))
            {
        numericMatchId = parsedId;
            }
            }
        }

        // 4. Fallback: Versuche Match ID als Numeric zu parsen
 if (numericMatchId == 0 && matchIdString != null)
        {
   if (int.TryParse(matchIdString, out var parsedMatchId))
            {
   numericMatchId = parsedMatchId;
  _debugLog($"? [WS-MESSAGE] Parsed match ID as numeric: {numericMatchId}", "MATCH_RESULT");
 }
          else if (matchIdString.Contains("-") && matchIdString.Length > 30)
     {
      // Das ist wahrscheinlich eine UUID
     matchUuid = matchIdString;
           _debugLog($"? [WS-MESSAGE] Detected match ID as UUID: {matchUuid}", "MATCH_RESULT");
   }
            else
            {
   _debugLog($"?? [WS-MESSAGE] Could not parse match ID: '{matchIdString}'", "WARNING");
    }
        }

        _debugLog($"?? [WS-MESSAGE] Match identification extracted:", "MATCH_RESULT");
        _debugLog($"   Original Match ID String: {matchIdString}", "MATCH_RESULT");
        _debugLog($"   UUID: {matchUuid ?? "none"}", "MATCH_RESULT");
        _debugLog($" Numeric ID: {numericMatchId}", "MATCH_RESULT");

        // Erweiterte Result-Extraktion
   var result = matchUpdateElement.TryGetProperty("result", out var resultEl2) ? resultEl2 : matchUpdateElement;

        // Detaillierte Score-Extraktion
        var player1Sets = ExtractIntValue(result, "player1Sets", "Player1Sets") ?? 0;
        var player2Sets = ExtractIntValue(result, "player2Sets", "Player2Sets") ?? 0;
  var player1Legs = ExtractIntValue(result, "player1Legs", "Player1Legs") ?? 0;
        var player2Legs = ExtractIntValue(result, "player2Legs", "Player2Legs") ?? 0;
   var notes = ExtractStringValue(result, "notes", "Notes") ?? "";
        var status = ExtractStringValue(result, "status", "Status") ?? "Finished";

    // ? NEU: Stelle sicher, dass die komplette Nachricht mit statistics in Notes gespeichert wird
        // Das PlayerStatisticsManager benötigt Zugriff auf die kompletten Daten
        var completeNotes = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = false });
        _debugLog($"?? [WS-MESSAGE] Complete message saved to Notes for statistics processing", "MATCH_RESULT");

      // Erweiterte Class-ID Extraktion
    var classId = ExtractIntValue(result, "classId", "ClassId") ??
 ExtractIntValue(matchUpdateElement, "classId", "ClassId") ??
       ExtractIntValue(message, "classId", "ClassId") ?? 1;

        // Group-Information Extraktion
        var groupId = ExtractIntValue(result, "groupId", "GroupId") ??
             ExtractIntValue(matchUpdateElement, "groupId", "GroupId") ??
        ExtractIntValue(message, "groupId", "GroupId");

 var groupName = ExtractStringValue(result, "groupName", "GroupName") ??
     ExtractStringValue(matchUpdateElement, "groupName", "GroupName") ??
            ExtractStringValue(message, "groupName", "GroupName");

        var matchType = ExtractStringValue(result, "matchType", "MatchType") ??
 ExtractStringValue(matchUpdateElement, "matchType", "MatchType") ??
   ExtractStringValue(message, "matchType", "MatchType") ?? "Group";

  _debugLog($"?? [WS-MESSAGE] Extracted Match Data:", "MATCH_RESULT");
        _debugLog($"   UUID: {matchUuid ?? "none"}", "MATCH_RESULT");
        _debugLog($"   Numeric ID: {numericMatchId}", "MATCH_RESULT");
        _debugLog($"   Player 1: {player1Sets} Sets, {player1Legs} Legs", "MATCH_RESULT");
        _debugLog($"   Player 2: {player2Sets} Sets, {player2Legs} Legs", "MATCH_RESULT");
  _debugLog($"   Status: {status}", "MATCH_RESULT");
_debugLog($"   Class ID: {classId}", "MATCH_RESULT");
        _debugLog($"   Group: {groupName ?? "None"} (ID: {groupId})", "MATCH_RESULT");

     // Erstelle HubMatchUpdateEventArgs
 return new HubMatchUpdateEventArgs
        {
   MatchId = numericMatchId,
          ClassId = classId,
  Player1Sets = player1Sets,
    Player2Sets = player2Sets,
         Player1Legs = player1Legs,
    Player2Legs = player2Legs,
            Status = status,
   Notes = completeNotes, // ? NEU: Komplette Nachricht für Statistik-Verarbeitung
 UpdatedAt = DateTime.Now,
       Source = isMatchResult ? "hub-match-result" : "websocket-direct",
          GroupId = groupId,
  GroupName = groupName,
  MatchType = matchType,
  MatchUuid = matchUuid,
      OriginalMatchIdString = matchIdString,
        // ? NEU: Setze Flags basierend auf Status
      IsMatchStarted = status == "InProgress" || status == "Finished",
       IsMatchCompleted = status == "Finished"
        };
    }

    // Helper-Methoden für Datenextraktion
  private int? ExtractIntValue(JsonElement element, params string[] propertyNames)
    {
   foreach (var propName in propertyNames)
        {
    if (element.TryGetProperty(propName, out var propElement))
    {
        if (propElement.ValueKind == JsonValueKind.Number && propElement.TryGetInt32(out var intValue))
    {
       return intValue;
   }
   else if (propElement.ValueKind == JsonValueKind.String)
            {
             var stringValue = propElement.GetString();
        if (int.TryParse(stringValue, out var parsedInt))
  {
       return parsedInt;
               }
}
       }
        }
      return null;
    }

  private string? ExtractStringValue(JsonElement element, params string[] propertyNames)
    {
        foreach (var propName in propertyNames)
        {
  if (element.TryGetProperty(propName, out var propElement))
            {
           var stringValue = propElement.GetString();
          if (!string.IsNullOrEmpty(stringValue))
             {
   return stringValue;
      }
            }
        }
        return null;
    }

    private List<string> GetJsonProperties(JsonElement element)
    {
   var properties = new List<string>();
   if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
      {
            properties.Add(property.Name);
            }
    }
        return properties;
    }

    /// <summary>
    /// ? NEU: Verarbeitet PowerScoring-Update Messages
    /// </summary>
    private async Task HandlePowerScoringUpdate(JsonElement message)
    {
        try
        {
            _debugLog("?? [POWERSCORING] Processing power-scoring-update message", "POWERSCORING");
            
            var powerScoringMessage = ParsePowerScoringMessage(message, "power-scoring-update");
            
            if (powerScoringMessage != null)
            {
                _debugLog($"?? [POWERSCORING] Update for player {powerScoringMessage.PlayerName}: " +
                         $"Total={powerScoringMessage.TotalScore}, Avg={powerScoringMessage.Average:F2}",
                         "POWERSCORING");

                PowerScoringMessageReceived?.Invoke(this, powerScoringMessage);
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _debugLog($"? [POWERSCORING] Error processing update: {ex.Message}", "ERROR");
        }
    }

    /// <summary>
    /// ? NEU: Verarbeitet PowerScoring-Progress Messages (Live-Updates während des Scorings)
    /// </summary>
    private async Task HandlePowerScoringProgress(JsonElement message)
    {
        try
        {
            _debugLog("?? [POWERSCORING] Processing power-scoring-progress message", "POWERSCORING");
            
            var powerScoringMessage = ParsePowerScoringProgressMessage(message);
            
            if (powerScoringMessage is not null)
            {
                _debugLog($"?? [POWERSCORING] Progress for player {powerScoringMessage.PlayerName}: " +
                         $"Round {powerScoringMessage.Rounds}, Total={powerScoringMessage.TotalScore}, Avg={powerScoringMessage.Average:F2}",
                         "POWERSCORING");

                PowerScoringMessageReceived?.Invoke(this, powerScoringMessage);
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _debugLog($"? [POWERSCORING] Error processing progress: {ex.Message}", "ERROR");
        }
    }

    /// <summary>
    /// ? NEU: Verarbeitet PowerScoring-Result Messages (Finale Ergebnisse)
    /// </summary>
    private async Task HandlePowerScoringResult(JsonElement message)
    {
        try
        {
            _debugLog("? [POWERSCORING] Processing power-scoring-result message", "POWERSCORING");
            
            var powerScoringMessage = ParsePowerScoringMessage(message, "power-scoring-result");
            
            if (powerScoringMessage is not null)
            {
                _debugLog($"? [POWERSCORING] Result for player {powerScoringMessage.PlayerName}: " +
                         $"Total={powerScoringMessage.TotalScore}, Avg={powerScoringMessage.Average:F2}",
                         "POWERSCORING");

                PowerScoringMessageReceived?.Invoke(this, powerScoringMessage);
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _debugLog($"? [POWERSCORING] Error processing result: {ex.Message}", "ERROR");
        }
    }

    /// <summary>
    /// ? NEU: Parst PowerScoring Message JSON
    /// </summary>
    private PowerScoringHubMessage? ParsePowerScoringMessage(JsonElement message, string messageType)
    {
        try
        {
            var powerScoringMessage = new PowerScoringHubMessage
            {
                Type = message.TryGetProperty("type", out var type) ? type.GetString() ?? "" : "",
                TournamentId = message.TryGetProperty("tournamentId", out var tid) ? tid.GetString() ?? "" : "",
                ParticipantId = message.TryGetProperty("participantId", out var pid) ? pid.GetString() ?? "" : "",
                PlayerName = message.TryGetProperty("playerName", out var pn) ? pn.GetString() ?? "" : "",
                Rounds = message.TryGetProperty("rounds", out var r) ? r.GetInt32() : 0,
                TotalScore = message.TryGetProperty("totalScore", out var ts) ? ts.GetInt32() : 0,
                Average = message.TryGetProperty("average", out var avg) ? avg.GetDouble() : 0.0,
                HighestThrow = message.TryGetProperty("highestThrow", out var ht) ? ht.GetInt32() : 0,
                TotalDarts = message.TryGetProperty("totalDarts", out var td) ? td.GetInt32() : 0,
                SessionStartTime = message.TryGetProperty("sessionStartTime", out var sst) 
                    ? DateTime.Parse(sst.GetString() ?? DateTime.Now.ToString()) 
                    : null,
                CompletionTime = message.TryGetProperty("completionTime", out var ct) 
                    ? DateTime.Parse(ct.GetString() ?? DateTime.Now.ToString()) 
                    : null,
                SubmittedVia = message.TryGetProperty("submittedVia", out var sv) ? sv.GetString() : null,
                SubmittedAt = message.TryGetProperty("submittedAt", out var sa) 
                    ? DateTime.Parse(sa.GetString() ?? DateTime.Now.ToString()) 
                    : null,
                Timestamp = message.TryGetProperty("timestamp", out var timestamp) 
                    ? DateTime.Parse(timestamp.GetString() ?? DateTime.Now.ToString()) 
                    : DateTime.Now
            };

            // Parse ThrowHistory
            if (message.TryGetProperty("throwHistory", out var historyArray) && 
                historyArray.ValueKind == JsonValueKind.Array)
            {
                powerScoringMessage.ThrowHistory = new List<ThrowRound>();
                
                foreach (var roundItem in historyArray.EnumerateArray())
                {
                    var throwRound = new ThrowRound
                    {
                        Round = roundItem.TryGetProperty("round", out var round) ? round.GetInt32() : 0,
                        Total = roundItem.TryGetProperty("total", out var total) ? total.GetInt32() : 0,
                        Timestamp = roundItem.TryGetProperty("timestamp", out var roundTs) 
                            ? DateTime.Parse(roundTs.GetString() ?? DateTime.Now.ToString()) 
                            : DateTime.Now
                    };
                    
                    // Parse Darts array
                    if (roundItem.TryGetProperty("darts", out var dartsArray) && 
                        dartsArray.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var dartItem in dartsArray.EnumerateArray())
                        {
                            var dart = new DartThrow
                            {
                                Number = GetDartNumber(dartItem),
                                Multiplier = dartItem.TryGetProperty("multiplier", out var mult) ? mult.GetInt32() : 1,
                                Score = dartItem.TryGetProperty("score", out var score) ? score.GetInt32() : 0,
                                DisplayValue = dartItem.TryGetProperty("displayValue", out var dv) ? dv.GetString() ?? "" : ""
                            };
                            throwRound.Darts.Add(dart);
                        }
                    }
                    
                    powerScoringMessage.ThrowHistory.Add(throwRound);
                }
                
                _debugLog($"?? [POWERSCORING] Parsed {powerScoringMessage.ThrowHistory.Count} throw rounds with detailed dart info", "POWERSCORING");
            }

            return powerScoringMessage;
        }
        catch (Exception ex)
        {
            _debugLog($"? [POWERSCORING] Error parsing message: {ex.Message}", "ERROR");
            return null;
        }
    }
    
    /// <summary>
    /// Helper zum Parsen von Dart-Number (kann String wie "bullseye" oder Zahl wie 20 sein)
    /// </summary>
    private static string GetDartNumber(JsonElement dartItem)
    {
        if (dartItem.TryGetProperty("number", out var numberProp))
        {
            // Prüfe ob es ein String ist
            if (numberProp.ValueKind == JsonValueKind.String)
            {
                return numberProp.GetString() ?? "0";
            }
            // Prüfe ob es eine Zahl ist
            else if (numberProp.ValueKind == JsonValueKind.Number)
            {
                return numberProp.GetInt32().ToString();
            }
        }
        
        return "0";
    }
    
    /// <summary>
    /// ? NEU: Parst power-scoring-progress Messages (Live-Updates)
    /// Format: {"type":"power-scoring-progress","currentRound":2,"totalRounds":8,"totalScore":60,"lastThrow":{...}}
    /// </summary>
    private PowerScoringHubMessage? ParsePowerScoringProgressMessage(JsonElement message)
    {
        try
        {
            var progressMessage = new PowerScoringHubMessage
            {
                Type = "power-scoring-progress",
                TournamentId = message.TryGetProperty("tournamentId", out var tid) ? tid.GetString() ?? "" : "",
                ParticipantId = message.TryGetProperty("participantId", out var pid) ? pid.GetString() ?? "" : "",
                PlayerName = message.TryGetProperty("playerName", out var pn) ? pn.GetString() ?? "" : "",
                Rounds = message.TryGetProperty("currentRound", out var cr) ? cr.GetInt32() : 0,  // ? FIX: currentRound statt totalRounds
                TotalScore = message.TryGetProperty("totalScore", out var ts) ? ts.GetInt32() : 0,
                TotalDarts = message.TryGetProperty("totalDarts", out var td) ? td.GetInt32() : 0,
                HighestThrow = message.TryGetProperty("highestThrow", out var ht) ? ht.GetInt32() : 0,
                Timestamp = message.TryGetProperty("timestamp", out var timestamp) 
                    ? DateTime.Parse(timestamp.GetString() ?? DateTime.Now.ToString()) 
                    : DateTime.Now
            };
            
            // Parse average (kann String oder Number sein)
            if (message.TryGetProperty("average", out var avgProp))
            {
                if (avgProp.ValueKind == JsonValueKind.String)
                {
                    // ? FIX: Verwende InvariantCulture für Punkt als Dezimaltrennzeichen
                    if (double.TryParse(avgProp.GetString(), System.Globalization.NumberStyles.Float, 
                        System.Globalization.CultureInfo.InvariantCulture, out var avgValue))
                    {
                        progressMessage.Average = avgValue;
                    }
                }
                else if (avgProp.ValueKind == JsonValueKind.Number)
                {
                    progressMessage.Average = avgProp.GetDouble();
                }
            }
            
            // Parse lastThrow (die letzte geworfene Runde)
            if (message.TryGetProperty("lastThrow", out var dartsElement))
            {
                var throwRound = new ThrowRound
                {
                    Round = dartsElement.TryGetProperty("round", out var round) ? round.GetInt32() : 0,
                    Total = dartsElement.TryGetProperty("total", out var total) ? total.GetInt32() : 0,
                    Timestamp = dartsElement.TryGetProperty("timestamp", out var roundTs) 
                        ? DateTime.Parse(roundTs.GetString() ?? DateTime.Now.ToString()) 
                        : DateTime.Now
                };
                
                // Parse Darts
                if (dartsElement.TryGetProperty("darts", out var dartsArray) && 
                    dartsArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var dartItem in dartsArray.EnumerateArray())
                    {
                        var dart = new DartThrow
                        {
                            Number = GetDartNumber(dartItem),
                            Multiplier = dartItem.TryGetProperty("multiplier", out var mult) ? mult.GetInt32() : 1,
                            Score = dartItem.TryGetProperty("score", out var score) ? score.GetInt32() : 0,
                            DisplayValue = dartItem.TryGetProperty("displayValue", out var dv) ? dv.GetString() ?? "" : ""
                        };
                        throwRound.Darts.Add(dart);
                    }
                }
                
                progressMessage.ThrowHistory.Add(throwRound);
            }
            
            _debugLog($"?? [POWERSCORING] Parsed progress: Round {progressMessage.Rounds}, Score {progressMessage.TotalScore}", "POWERSCORING");
            
            return progressMessage;
        }
        catch (Exception ex)
        {
            _debugLog($"? [POWERSCORING] Error parsing progress message: {ex.Message}", "ERROR");
            return null;
        }
    }
    
    private void HandlePlannerFetchError(JsonElement message)
    {
        try
        {
            var error = JsonSerializer.Deserialize<PlannerFetchErrorResponse>(message.GetRawText());
            if (error != null)
            {
                _debugLog($"? [WS-MESSAGE] Planner fetch error: {error.Error}", "ERROR");
                PlannerFetchFailed?.Invoke(error);
            }
            else
            {
                _debugLog("? [WS-MESSAGE] Planner fetch error could not be parsed", "ERROR");
            }
        }
        catch (Exception ex)
        {
            _debugLog($"? [WS-MESSAGE] Error handling planner fetch error: {ex.Message}", "ERROR");
        }
    }
}
