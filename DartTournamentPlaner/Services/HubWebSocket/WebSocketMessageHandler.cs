using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DartTournamentPlaner.Services.HubWebSocket;

/// <summary>
/// Verarbeitet eingehende WebSocket-Nachrichten vom Tournament Hub
/// </summary>
public class WebSocketMessageHandler
{
    private readonly Action<string, string> _debugLog;
    private readonly Func<JsonElement, Task> _matchUpdateAcknowledgment;
    private readonly Func<string, Task> _errorAcknowledgment;

    // Events
    public event Action<HubMatchUpdateEventArgs>? MatchResultReceived;
    public event Action<string, object>? TournamentUpdateReceived;

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
                    case "heartbeat-ack":
                        HandleHeartbeatAck(message);
                        break;
                    case "error":
                        HandleErrorMessage(message);
                        break;
                    default:
                        _debugLog($"❌ [WS-MESSAGE] Unknown message type: {messageType}", "WARNING");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-MESSAGE] Error processing WebSocket message: {ex.Message}", "ERROR");
        }
    }

    /// <summary>
    /// Behandelt Welcome-Nachrichten
    /// </summary>
    private void HandleWelcomeMessage(JsonElement message)
    {
        var clientId = message.TryGetProperty("clientId", out var clientIdElement) ? clientIdElement.GetString() : "unknown";
        _debugLog($"👋 [WS-MESSAGE] Welcome message received (ID: {clientId})", "WEBSOCKET");
    }

    /// <summary>
    /// Behandelt Error-Nachrichten
    /// </summary>
    private void HandleErrorMessage(JsonElement message)
    {
        var error = message.TryGetProperty("error", out var errorElement) ? errorElement.GetString() : "Unknown error";
        _debugLog($"❌ [WS-MESSAGE] Server Error: {error}", "ERROR");
    }

    /// <summary>
    /// Behandelt Heartbeat-Acknowledgments
    /// </summary>
    private void HandleHeartbeatAck(JsonElement message)
    {
        _debugLog("💓 [WS-MESSAGE] Heartbeat acknowledged", "WEBSOCKET");
    }

    /// <summary>
    /// Behandelt Subscription-Confirmations
    /// </summary>
    private void HandleSubscriptionConfirmed(JsonElement message)
    {
        var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdElement) ? tournamentIdElement.GetString() : "unknown";
        _debugLog($"✅ [WS-MESSAGE] Subscribed to {tournamentId}", "SUCCESS");
    }

    /// <summary>
    /// Behandelt Planner-Registration-Confirmations
    /// </summary>
    private void HandlePlannerRegistrationConfirmed(JsonElement message)
    {
        var success = message.TryGetProperty("success", out var successElement) && successElement.GetBoolean();
        var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdElement) ? tournamentIdElement.GetString() : "unknown";

        if (success)
        {
            _debugLog($"✅ [WS-MESSAGE] Registered as Planner for {tournamentId}", "SUCCESS");
        }
        else
        {
            _debugLog("❌ [WS-MESSAGE] Planner registration failed", "ERROR");
        }
    }

    /// <summary>
    /// Behandelt Tournament Match Updates
    /// </summary>
    private async Task HandleTournamentMatchUpdate(JsonElement message)
    {
        try
        {
            _debugLog("📥 [WS-MESSAGE] ===== MATCH UPDATE RECEIVED =====", "MATCH_RESULT");
            _debugLog($"📥 [WS-MESSAGE] Raw tournament match update message:", "MATCH_RESULT");
            _debugLog($"📥 [WS-MESSAGE] {JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true })}", "MATCH_RESULT");

            // Sende sofortige Empfangsbestätigung an Server
            await _matchUpdateAcknowledgment(message);

            var isMatchResult = message.TryGetProperty("matchResultHighlight", out var highlightElement) && highlightElement.GetBoolean();
            var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdEl) ? tournamentIdEl.GetString() : "unknown";

            _debugLog($"🏆 [WS-MESSAGE] Tournament Match Update for: {tournamentId}", "MATCH_RESULT");
            _debugLog($"🎯 [WS-MESSAGE] Match Result Highlight: {isMatchResult}", "MATCH_RESULT");

            // Extrahiere Match Update Daten
            var matchUpdate = ExtractMatchUpdateData(message, isMatchResult);
            if (matchUpdate != null)
            {
                _debugLog("🎯 [WS-MESSAGE] TRIGGERING OnMatchResultReceivedFromHub EVENT", "MATCH_RESULT");
                _debugLog($"🔔 [WS-MESSAGE] Event subscribers count: {MatchResultReceived?.GetInvocationList()?.Length ?? 0}", "MATCH_RESULT");

                MatchResultReceived?.Invoke(matchUpdate);

                _debugLog("✅ [WS-MESSAGE] Match update event triggered successfully!", "SUCCESS");
                _debugLog("📤 [WS-MESSAGE] Match update forwarded to Tournament Planner UI", "SUCCESS");
            }
            else
            {
                _debugLog("⚠️ [WS-MESSAGE] No match update data found in message", "WARNING");
                _debugLog($"⚠️ [WS-MESSAGE] Available properties: {string.Join(", ", GetJsonProperties(message))}", "WARNING");
            }

            _debugLog("📥 [WS-MESSAGE] ===== MATCH UPDATE PROCESSING COMPLETE =====", "MATCH_RESULT");
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-MESSAGE] Error handling tournament match update: {ex.Message}", "ERROR");
            _debugLog($"❌ [WS-MESSAGE] Stack trace: {ex.StackTrace}", "ERROR");

            // Sende Fehler-Acknowledgment
            await _errorAcknowledgment(ex.Message);
        }
    }

    /// <summary>
    /// Extrahiert Match Update Daten aus der Nachricht
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
            _debugLog("✅ [WS-MESSAGE] Found matchUpdate property", "MATCH_RESULT");
        }
        // Priorität 2: Fallback zu result Property
        else if (message.TryGetProperty("result", out matchUpdateElement))
        {
            hasMatchUpdate = true;
            _debugLog("✅ [WS-MESSAGE] Using result property as matchUpdate fallback", "MATCH_RESULT");
        }
        // Priorität 3: Direkte Match-Daten im Root
        else if (message.TryGetProperty("matchId", out var _))
        {
            matchUpdateElement = message;
            hasMatchUpdate = true;
            _debugLog("✅ [WS-MESSAGE] Using root message as matchUpdate", "MATCH_RESULT");
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
                _debugLog($"✅ [WS-MESSAGE] Parsed match ID as numeric: {numericMatchId}", "MATCH_RESULT");
            }
            else if (matchIdString.Contains("-") && matchIdString.Length > 30)
            {
                // Das ist wahrscheinlich eine UUID
                matchUuid = matchIdString;
                _debugLog($"✅ [WS-MESSAGE] Detected match ID as UUID: {matchUuid}", "MATCH_RESULT");
            }
            else
            {
                _debugLog($"⚠️ [WS-MESSAGE] Could not parse match ID: '{matchIdString}'", "WARNING");
            }
        }

        _debugLog($"🔍 [WS-MESSAGE] Match identification extracted:", "MATCH_RESULT");
        _debugLog($"   Original Match ID String: {matchIdString}", "MATCH_RESULT");
        _debugLog($"   UUID: {matchUuid ?? "none"}", "MATCH_RESULT");
        _debugLog($"   Numeric ID: {numericMatchId}", "MATCH_RESULT");

        // Erweiterte Result-Extraktion
        var result = matchUpdateElement.TryGetProperty("result", out var resultEl2) ? resultEl2 : matchUpdateElement;

        // Detaillierte Score-Extraktion
        var player1Sets = ExtractIntValue(result, "player1Sets", "Player1Sets") ?? 0;
        var player2Sets = ExtractIntValue(result, "player2Sets", "Player2Sets") ?? 0;
        var player1Legs = ExtractIntValue(result, "player1Legs", "Player1Legs") ?? 0;
        var player2Legs = ExtractIntValue(result, "player2Legs", "Player2Legs") ?? 0;
        var notes = ExtractStringValue(result, "notes", "Notes") ?? "";
        var status = ExtractStringValue(result, "status", "Status") ?? "Finished";

        // ✅ NEU: Stelle sicher, dass die komplette Nachricht mit statistics in Notes gespeichert wird
        // Das PlayerStatisticsManager benötigt Zugriff auf die kompletten Daten
        var completeNotes = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = false });
        _debugLog($"📊 [WS-MESSAGE] Complete message saved to Notes for statistics processing", "MATCH_RESULT");

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

        _debugLog($"📊 [WS-MESSAGE] Extracted Match Data:", "MATCH_RESULT");
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
            Notes = completeNotes, // ✅ NEU: Komplette Nachricht für Statistik-Verarbeitung
            UpdatedAt = DateTime.Now,
            Source = isMatchResult ? "hub-match-result" : "websocket-direct", // ✅ KORRIGIERT
            GroupId = groupId,
            GroupName = groupName,
            MatchType = matchType,
            MatchUuid = matchUuid,
            OriginalMatchIdString = matchIdString
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