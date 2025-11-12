using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
            var message = JsonSerializer.Deserialize<JsonElement>(messageJson);

   if (message.TryGetProperty("type", out var typeElement))
        {
      var messageType = typeElement.GetString();

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
        case "heartbeat-ack":
        HandleHeartbeatAck(message);
    break;
       case "error":
     HandleErrorMessage(message);
        break;
                default:
_debugLog($"? [WS-MESSAGE] Unknown message type: {messageType}", "WARNING");
         break;
           }
   }
        }
    catch (Exception ex)
        {
       _debugLog($"? [WS-MESSAGE] Error processing WebSocket message: {ex.Message}", "ERROR");
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
  var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdElement) ? tournamentIdElement.GetString() : "unknown";
        _debugLog($"? [WS-MESSAGE] Subscribed to {tournamentId}", "SUCCESS");
    }

    /// <summary>
    /// Behandelt Planner-Registration-Confirmationen
    /// </summary>
    private void HandlePlannerRegistrationConfirmed(JsonElement message)
    {
     var success = message.TryGetProperty("success", out var successElement) && successElement.GetBoolean();
        var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdElement) ? tournamentIdElement.GetString() : "unknown";

    if (success)
        {
         _debugLog($"? [WS-MESSAGE] Registered as Planner for {tournamentId}", "SUCCESS");
     }
   else
        {
     _debugLog("? [WS-MESSAGE] Planner registration failed", "ERROR");
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
}
