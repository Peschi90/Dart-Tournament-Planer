using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using DartTournamentPlaner.Models;
using System.Net.WebSockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Event-Argumente für Match-Updates vom Hub
/// </summary>
public class HubMatchUpdateEventArgs : EventArgs
{
    public int MatchId { get; set; }
    public int ClassId { get; set; }
    public int Player1Sets { get; set; }
    public int Player2Sets { get; set; }
    public int Player1Legs { get; set; }
    public int Player2Legs { get; set; }
    public string Status { get; set; } = "NotStarted";
    public string? Notes { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Source { get; set; } = "hub";
    
    // 🚨 HINZUGEFÜGT: Group-Information für eindeutige Match-Identifikation
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public string? MatchType { get; set; }
}

/// <summary>
/// Service für die Kommunikation mit dem Tournament Hub
/// </summary>
public interface ITournamentHubService
{
    string HubUrl { get; set; }
    
    // Registration & Basic Operations
    Task<bool> RegisterWithHubAsync(string tournamentId, string name, string description);
    Task<bool> UnregisterFromHubAsync(string tournamentId);
    Task<bool> SendHeartbeatAsync(string tournamentId, int activeMatches, int totalPlayers);
    Task<bool> SyncTournamentWithClassesAsync(string tournamentId, string name, TournamentData data);
    
    // WebSocket Operations
    Task<bool> InitializeWebSocketAsync();
    Task<bool> SubscribeToTournamentAsync(string tournamentId);
    Task<bool> UnsubscribeFromTournamentAsync(string tournamentId);
    Task<bool> RegisterAsPlannerAsync(string tournamentId, object plannerInfo);
    Task CloseWebSocketAsync();
    
    // Events for WebSocket communication
    event Action<HubMatchUpdateEventArgs> OnMatchResultReceivedFromHub;
    event Action<string, object> OnTournamentUpdateReceived;
    event Action<bool, string> OnConnectionStatusChanged;
    
    // Utility
    string GetJoinUrl(string tournamentId);
}

/// <summary>
/// Implementierung des Tournament Hub Service mit WebSocket Support
/// </summary>
public class TournamentHubService : ITournamentHubService, IDisposable
{
    private readonly ConfigService _configService;
    private readonly HttpClient _httpClient;

    // Hub configuration
    public string HubUrl { get; set; }
    private readonly Dictionary<string, string> _hubData = new Dictionary<string, string>();
    
    // Disposal
    private bool _isDisposed = false;
    
    // Heartbeat Timer
    private System.Timers.Timer? _heartbeatTimer;

    // WebSocket connection state
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _webSocketCancellation;
    private bool _isWebSocketConnected = false;
    private string? _connectedEndpoint;
    private string? _currentTournamentId;

    // WebSocket events
    public event Action<HubMatchUpdateEventArgs>? OnMatchResultReceivedFromHub;
    public event Action<string, object>? OnTournamentUpdateReceived;
    public event Action<bool, string>? OnConnectionStatusChanged;

    // Hub Debug Window Support
    private Views.HubDebugWindow? _debugWindow;

    public TournamentHubService(ConfigService configService)
    {
        _configService = configService;
        HubUrl = "https://dtp.i3ull3t.de:9443";
        
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DartTournamentPlaner/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        DebugLog($"🎯 TournamentHubService initialized with Hub URL: {HubUrl}");
    }

    /// <summary>
    /// Setzt das HubDebugWindow für erweiterte Logging-Ausgaben
    /// </summary>
    public void SetDebugWindow(Views.HubDebugWindow debugWindow)
    {
        _debugWindow = debugWindow;
        DebugLog("🔧 Debug Window connected to TournamentHubService", "SUCCESS");
        
        // Aktualisiere Verbindungsstatus im Debug Window
        if (_isWebSocketConnected)
        {
            _debugWindow?.UpdateConnectionStatus(true, $"WebSocket Connected ({_connectedEndpoint})");
        }
        else
        {
            _debugWindow?.UpdateConnectionStatus(false, "WebSocket Disconnected");
        }
    }

    /// <summary>
    /// Entfernt das HubDebugWindow 
    /// </summary>
    public void RemoveDebugWindow()
    {
        if (_debugWindow != null)
        {
            DebugLog("🔧 Debug Window disconnected from TournamentHubService", "WARNING");
            _debugWindow = null;
        }
    }

    /// <summary>
    /// Unified Debug Logging - sendet an Visual Studio Debug und HubDebugWindow
    /// </summary>
    private void DebugLog(string message, string category = "INFO")
    {
        // Immer an Visual Studio Debug senden
        System.Diagnostics.Debug.WriteLine(message);
        
        // Auch an HubDebugWindow senden wenn verfügbar
        try
        {
            _debugWindow?.AddDebugMessage(message, category);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error sending to HubDebugWindow: {ex.Message}");
        }
    }

    // =====================================
    // WEBSOCKET IMPLEMENTATION
    // =====================================

    /// <summary>
    /// Initialisiert WebSocket-Verbindung zum Hub
    /// </summary>
    public async Task<bool> InitializeWebSocketAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔌 [PLANNER-WS] ===== INITIALIZING WEBSOCKET =====");
            System.Diagnostics.Debug.WriteLine($"🔌 [PLANNER-WS] Hub URL: {HubUrl}");
            
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                System.Diagnostics.Debug.WriteLine("🔌 [PLANNER-WS] WebSocket already connected, skipping initialization");
                return true;
            }
            
            // Dispose existing WebSocket if any
            await CloseWebSocketAsync();
            
            _webSocket = new ClientWebSocket();
            _webSocketCancellation = new CancellationTokenSource();
            
            // Configure WebSocket
            _webSocket.Options.SetRequestHeader("User-Agent", "DartTournamentPlaner-WebSocket/1.0");
            
            // Korrekte SSL WebSocket-Endpunkte für Tournament Hub
            var wsUrl = HubUrl.Replace("https://", "wss://").Replace("http://", "ws://");
            
            // Teste SSL und HTTP WebSocket-Endpunkte in korrekter Reihenfolge
            string[] possibleEndpoints = {
                $"wss://dtp.i3ull3t.de:9444/ws",     // SSL WebSocket (bevorzugt)
                $"ws://dtp.i3ull3t.de:9445/ws",      // HTTP WebSocket (fallback)
                $"{wsUrl}/socket.io/?EIO=4&transport=websocket"  // Socket.IO fallback
            };
            
            bool connected = false;
            Exception lastException = null;
            
            foreach (var endpoint in possibleEndpoints)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"🔌 [PLANNER-WS] Trying endpoint: {endpoint}");
                    
                    // Reset WebSocket for each attempt
                    _webSocket?.Dispose();
                    _webSocket = new ClientWebSocket();
                    _webSocket.Options.SetRequestHeader("User-Agent", "DartTournamentPlaner-WebSocket/1.0");
                    
                    // SSL-spezifische Konfiguration
                    if (endpoint.StartsWith("wss://"))
                    {
                        // SSL WebSocket Optionen - robuster für Productions-SSL
                        _webSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"🔒 [PLANNER-WS] SSL Certificate validation:");
                            System.Diagnostics.Debug.WriteLine($"🔒 [PLANNER-WS]   Subject: {certificate?.Subject}");
                            System.Diagnostics.Debug.WriteLine($"🔒 [PLANNER-WS]   Issuer: {certificate?.Issuer}");
                            System.Diagnostics.Debug.WriteLine($"🔒 [PLANNER-WS]   Valid from: {certificate?.GetEffectiveDateString()}");
                            System.Diagnostics.Debug.WriteLine($"🔒 [PLANNER-WS]   Valid to: {certificate?.GetExpirationDateString()}");
                            System.Diagnostics.Debug.WriteLine($"🔒 [PLANNER-WS]   SSL Policy Errors: {sslPolicyErrors}");
                            
                            // Für Productions-SSL: Akzeptiere auch selbst-signierte Zertifikate
                            return true;
                        };
                        
                        // SSL-spezifische WebSocket-Optionen
                        _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                    }
                    
                    // Allgemeine WebSocket-Konfiguration für Stabilität
                    _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                    
                    var uri = new Uri(endpoint);
                    
                    // Längerer Timeout für SSL-Verbindungen
                    var connectionTimeout = endpoint.StartsWith("wss://") ? 30 : 15;
                    var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(connectionTimeout));
                    
                    System.Diagnostics.Debug.WriteLine($"🔌 [PLANNER-WS] Attempting connection with {connectionTimeout}s timeout...");
                    
                    await _webSocket.ConnectAsync(uri, timeoutCts.Token);
                    
                    if (_webSocket.State == WebSocketState.Open)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ [PLANNER-WS] Connected successfully to: {endpoint}");
                        System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER-WS] WebSocket SubProtocol: {_webSocket.SubProtocol}");
                        System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER-WS] WebSocket State: {_webSocket.State}");
                        System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER-WS] Keep-Alive Interval: {_webSocket.Options.KeepAliveInterval}");
                        
                        _connectedEndpoint = endpoint;
                        connected = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [PLANNER-WS] Failed to connect to {endpoint}: {ex.Message}");
                    lastException = ex;
                    continue;
                }
            }
            
            if (!connected)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [PLANNER-WS] All WebSocket connection attempts failed");
                System.Diagnostics.Debug.WriteLine($"❌ [PLANNER-WS] Last error: {lastException?.Message}");
                
                _isWebSocketConnected = false;
                OnConnectionStatusChanged?.Invoke(false, $"Connection failed: {lastException?.Message}");
                
                return false;
            }
            
            System.Diagnostics.Debug.WriteLine($"✅ [PLANNER-WS] WebSocket state: {_webSocket.State}");
            System.Diagnostics.Debug.WriteLine($"🔗 [PLANNER-WS] Connected endpoint: {_connectedEndpoint}");
            
            _isWebSocketConnected = true;
            OnConnectionStatusChanged?.Invoke(true, $"WebSocket Connected ({_connectedEndpoint})");
            
            // Start listening for messages
            _ = Task.Run(ListenForWebSocketMessages);
            
            System.Diagnostics.Debug.WriteLine("✅ [PLANNER-WS] WebSocket connection established successfully");
            System.Diagnostics.Debug.WriteLine("🔌 [PLANNER-WS] ===== WEBSOCKET INITIALIZATION COMPLETE =====");
            
            return true;
            
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER-WS] Error initializing WebSocket: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER-WS] Stack trace: {ex.StackTrace}");
            _isWebSocketConnected = false;
            OnConnectionStatusChanged?.Invoke(false, $"Connection Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Lauscht auf WebSocket-Nachrichten mit Keep-Alive
    /// </summary>
    private async Task ListenForWebSocketMessages()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("👂 [PLANNER-WS] Starting to listen for WebSocket messages...");
            var buffer = new byte[8192];
            
            // Start Heartbeat-Timer für Keep-Alive
            _heartbeatTimer = new System.Timers.Timer(30000); // 30 Sekunden
            _heartbeatTimer.Elapsed += async (sender, e) => await SendHeartbeat();
            _heartbeatTimer.Start();
            
            System.Diagnostics.Debug.WriteLine("💓 [PLANNER-WS] Heartbeat timer started (30s interval)");
            
            while (_webSocket?.State == WebSocketState.Open && !_webSocketCancellation.Token.IsCancellationRequested)
            {
                try
                {
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
                    using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_webSocketCancellation.Token, timeoutCts.Token);
                    
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), combinedCts.Token);
                    
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        System.Diagnostics.Debug.WriteLine($"📥 [PLANNER-WS] Raw WebSocket message received ({result.Count} bytes): {messageJson}");
                        
                        await ProcessWebSocketMessage(messageJson);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔌 [PLANNER-WS] WebSocket close message received: {result.CloseStatus} - {result.CloseStatusDescription}");
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("⏰ [PLANNER-WS] WebSocket receive operation cancelled or timed out");
                    break;
                }
                catch (WebSocketException wsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"🔌 [PLANNER-WS] WebSocket exception: {wsEx.Message}");
                    break;
                }
            }
            
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            
            System.Diagnostics.Debug.WriteLine($"👂 [PLANNER-WS] Stopped listening. WebSocket state: {_webSocket?.State}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER-WS] Error listening to WebSocket: {ex.Message}");
        }
        finally
        {
            _isWebSocketConnected = false;
            OnConnectionStatusChanged?.Invoke(false, "WebSocket Disconnected");
            
            // Automatischer Reconnect nach 5 Sekunden
            if (!_webSocketCancellation.Token.IsCancellationRequested)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    if (!_webSocketCancellation.Token.IsCancellationRequested)
                    {
                        await InitializeWebSocketAsync();
                    }
                });
            }
        }
    }

    /// <summary>
    /// WebSocket Message Processing
    /// </summary>
    private async Task ProcessWebSocketMessage(string messageJson)
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
                        System.Diagnostics.Debug.WriteLine($"❓ [PLANNER-WS] Unknown message type: {messageType}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER-WS] Error processing WebSocket message: {ex.Message}");
        }
    }

    // WebSocket Message Handlers (vereinfacht)
    private void HandleWelcomeMessage(JsonElement message)
    {
        var clientId = message.TryGetProperty("clientId", out var clientIdElement) ? clientIdElement.GetString() : "unknown";
        OnConnectionStatusChanged?.Invoke(true, $"WebSocket Connected (ID: {clientId})");
        _debugWindow?.UpdateConnectionStatus(true, $"WebSocket Connected (ID: {clientId})");
    }

    private void HandleErrorMessage(JsonElement message)
    {
        var error = message.TryGetProperty("error", out var errorElement) ? errorElement.GetString() : "Unknown error";
        OnConnectionStatusChanged?.Invoke(false, $"Server Error: {error}");
        _debugWindow?.UpdateConnectionStatus(false, $"Server Error: {error}");
    }

    private void HandleHeartbeatAck(JsonElement message)
    {
        DebugLog($"💓 [PLANNER-WS] Heartbeat acknowledged", "WEBSOCKET");
    }

    private void HandleSubscriptionConfirmed(JsonElement message)
    {
        var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdElement) ? tournamentIdElement.GetString() : "unknown";
        OnConnectionStatusChanged?.Invoke(true, $"Subscribed to {tournamentId}");
    }

    private void HandlePlannerRegistrationConfirmed(JsonElement message)
    {
        var success = message.TryGetProperty("success", out var successElement) && successElement.GetBoolean();
        var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdElement) ? tournamentIdElement.GetString() : "unknown";
        
        if (success)
        {
            OnConnectionStatusChanged?.Invoke(true, $"Registered as Planner for {tournamentId}");
        }
        else
        {
            OnConnectionStatusChanged?.Invoke(false, "Planner registration failed");
        }
    }

    private async Task HandleTournamentMatchUpdate(JsonElement message)
    {
        try
        {
            DebugLog($"📥 [PLANNER-WS] ===== MATCH UPDATE RECEIVED =====", "MATCH_RESULT");
            DebugLog($"📥 [PLANNER-WS] Raw tournament match update message:", "MATCH_RESULT");
            DebugLog($"📥 [PLANNER-WS] {JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true })}", "MATCH_RESULT");
            
            // Sende sofortige Empfangsbestätigung an Server
            await SendMatchUpdateAcknowledgment(message);
            
            var isMatchResult = message.TryGetProperty("matchResultHighlight", out var highlightElement) && highlightElement.GetBoolean();
            var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdEl) ? tournamentIdEl.GetString() : "unknown";
            
            DebugLog($"🏆 [PLANNER-WS] Tournament Match Update for: {tournamentId}", "MATCH_RESULT");
            DebugLog($"🎯 [PLANNER-WS] Match Result Highlight: {isMatchResult}", "MATCH_RESULT");
            
            // ERWEITERT: Mehrere Wege um Match Update zu extrahieren
            JsonElement matchUpdateElement = default;
            bool hasMatchUpdate = false;
            
            // Priorität 1: matchUpdate Property (direkt)
            if (message.TryGetProperty("matchUpdate", out matchUpdateElement))
            {
                hasMatchUpdate = true;
                DebugLog($"✅ [PLANNER-WS] Found matchUpdate property", "MATCH_RESULT");
            }
            // Priorität 2: Fallback zu result Property
            else if (message.TryGetProperty("result", out matchUpdateElement))
            {
                hasMatchUpdate = true;
                DebugLog($"✅ [PLANNER-WS] Using result property as matchUpdate fallback", "MATCH_RESULT");
            }
            // Priorität 3: Direkte Match-Daten im Root
            else if (message.TryGetProperty("matchId", out var _))
            {
                matchUpdateElement = message;
                hasMatchUpdate = true;
                DebugLog($"✅ [PLANNER-WS] Using root message as matchUpdate", "MATCH_RESULT");
            }
            
            if (hasMatchUpdate)
            {
                // ROBUSTE Match-ID Extraktion mit String-Support
                var matchId = 0;
                if (matchUpdateElement.TryGetProperty("matchId", out var matchIdEl))
                {
                    var extractedMatchId = ExtractIntValue(JsonDocument.Parse($"{{\"matchId\":{JsonSerializer.Serialize(matchIdEl)}}}").RootElement, "matchId");
                    matchId = extractedMatchId ?? 0;
                }
                else if (message.TryGetProperty("matchId", out var rootMatchIdEl))
                {
                    var extractedMatchId = ExtractIntValue(JsonDocument.Parse($"{{\"matchId\":{JsonSerializer.Serialize(rootMatchIdEl)}}}").RootElement, "matchId");
                    matchId = extractedMatchId ?? 0;
                }
                
                DebugLog($"🎯 [PLANNER-WS] Processing Match ID: {matchId}", "MATCH_RESULT");
                
                // Erweiterte Result-Extraktion
                var result = matchUpdateElement.TryGetProperty("result", out var resultEl) ? resultEl : matchUpdateElement;
                
                // Detaillierte Score-Extraktion mit Debugging
                var player1Sets = ExtractIntValue(result, "player1Sets", "Player1Sets") ?? 0;
                var player2Sets = ExtractIntValue(result, "player2Sets", "Player2Sets") ?? 0;
                var player1Legs = ExtractIntValue(result, "player1Legs", "Player1Legs") ?? 0;
                var player2Legs = ExtractIntValue(result, "player2Legs", "Player2Legs") ?? 0;
                var notes = ExtractStringValue(result, "notes", "Notes") ?? "";
                var status = ExtractStringValue(result, "status", "Status") ?? "Finished";
                
                DebugLog($"📊 [PLANNER-WS] Extracted Match Data:", "MATCH_RESULT");
                DebugLog($"   Match ID: {matchId}", "MATCH_RESULT");
                DebugLog($"   Player 1: {player1Sets} Sets, {player1Legs} Legs", "MATCH_RESULT");
                DebugLog($"   Player 2: {player2Sets} Sets, {player2Legs} Legs", "MATCH_RESULT");
                DebugLog($"   Status: {status}", "MATCH_RESULT");
                DebugLog($"   Notes: {notes}", "MATCH_RESULT");
                DebugLog($"   Source: {(isMatchResult ? "hub-match-result" : "hub-websocket-direct")}", "MATCH_RESULT");
                
                // Erweiterte Class-ID Extraktion
                var classId = ExtractIntValue(result, "classId", "ClassId") ?? 
                              ExtractIntValue(matchUpdateElement, "classId", "ClassId") ?? 
                              ExtractIntValue(message, "classId", "ClassId") ?? 1;
                
                DebugLog($"📚 [PLANNER-WS] Tournament Class ID: {classId}", "MATCH_RESULT");
                
                // 🚨 ERWEITERT: Verbesserte Group-Information Extraktion mit Round-Informationen
                var groupId = ExtractIntValue(result, "groupId", "GroupId") ?? 
                              ExtractIntValue(matchUpdateElement, "groupId", "GroupId") ?? 
                              ExtractIntValue(message, "groupId", "GroupId");
                
                var groupName = ExtractStringValue(result, "groupName", "GroupName") ?? 
                                ExtractStringValue(matchUpdateElement, "groupName", "GroupName") ?? 
                                ExtractStringValue(message, "groupName", "GroupName");
                
                var matchType = ExtractStringValue(result, "matchType", "MatchType") ?? 
                                ExtractStringValue(matchUpdateElement, "matchType", "MatchType") ?? 
                                ExtractStringValue(message, "matchType", "MatchType") ?? "Group";
                
                // 🚨 NEUE: Round-Information für KO-Matches extrahieren
                var round = ExtractStringValue(result, "round", "Round") ?? 
                           ExtractStringValue(matchUpdateElement, "round", "Round") ?? 
                           ExtractStringValue(message, "round", "Round");
                
                var position = ExtractIntValue(result, "position", "Position") ?? 
                              ExtractIntValue(matchUpdateElement, "position", "Position") ?? 
                              ExtractIntValue(message, "position", "Position");
                
                DebugLog($"📋 [PLANNER-WS] Match Identification:", "MATCH_RESULT");
                DebugLog($"   Group ID: {groupId?.ToString() ?? "None"}", "MATCH_RESULT");
                DebugLog($"   Group Name: {groupName ?? "None"}", "MATCH_RESULT");
                DebugLog($"   Match Type: {matchType}", "MATCH_RESULT");
                DebugLog($"   Round: {round ?? "None"}", "MATCH_RESULT");
                DebugLog($"   Position: {position?.ToString() ?? "None"}", "MATCH_RESULT");
                
                var matchUpdate = new HubMatchUpdateEventArgs
                {
                    MatchId = matchId,
                    ClassId = classId,
                    Player1Sets = player1Sets,
                    Player2Sets = player2Sets,
                    Player1Legs = player1Legs,
                    Player2Legs = player2Legs,
                    Status = status,
                    Notes = notes,
                    UpdatedAt = DateTime.Now,
                    Source = isMatchResult ? "hub-match-result" : "hub-websocket-direct",
                    // 🚨 ERWEITERT: Verbesserte Match-Identifikation
                    GroupId = groupId,
                    GroupName = groupName,
                    MatchType = matchType
                };
                
                DebugLog($"🎯 [PLANNER-WS] TRIGGERING OnMatchResultReceivedFromHub EVENT", "MATCH_RESULT");
                DebugLog($"🔔 [PLANNER-WS] Event subscribers count: {OnMatchResultReceivedFromHub?.GetInvocationList()?.Length ?? 0}", "MATCH_RESULT");
                
                // Triggere Event für UI-Update mit erweiterten Informationen
                OnMatchResultReceivedFromHub?.Invoke(matchUpdate);
                
                DebugLog($"✅ [PLANNER-WS] Match update event triggered successfully!", "SUCCESS");
                DebugLog($"📤 [PLANNER-WS] Match update forwarded to Tournament Planner UI", "SUCCESS");
                
            }
            else
            {
                DebugLog($"⚠️ [PLANNER-WS] No match update data found in message", "WARNING");
                DebugLog($"⚠️ [PLANNER-WS] Available properties: {string.Join(", ", GetJsonProperties(message))}", "WARNING");
            }
            
            DebugLog($"📥 [PLANNER-WS] ===== MATCH UPDATE PROCESSING COMPLETE =====", "MATCH_RESULT");
            
        }
        catch (Exception ex)
        {
            DebugLog($"❌ [PLANNER-WS] Error handling tournament match update: {ex.Message}", "ERROR");
            DebugLog($"❌ [PLANNER-WS] Stack trace: {ex.StackTrace}", "ERROR");
            
            // Sende Fehler-Acknowledgment
            await SendErrorAcknowledgment(ex.Message);
        }
    }

    // Helper-Methoden für erweiterte Datenextraktion
    private int? ExtractIntValue(JsonElement element, params string[] propertyNames)
    {
        foreach (var propName in propertyNames)
        {
            if (element.TryGetProperty(propName, out var propElement))
            {
                // ERWEITERT: Unterstützt sowohl Int32 als auch String-zu-Int Konvertierung
                if (propElement.ValueKind == JsonValueKind.Number && propElement.TryGetInt32(out var intValue))
                {
                    DebugLog($"🔍 [PLANNER-WS] Extracted {propName} as number: {intValue}");
                    return intValue;
                }
                else if (propElement.ValueKind == JsonValueKind.String)
                {
                    var stringValue = propElement.GetString();
                    if (int.TryParse(stringValue, out var parsedInt))
                    {
                        DebugLog($"🔍 [PLANNER-WS] Extracted {propName} as string->int: '{stringValue}' -> {parsedInt}");
                        return parsedInt;
                    }
                    else
                    {
                        DebugLog($"⚠️ [PLANNER-WS] Could not parse string to int for {propName}: '{stringValue}'", "WARNING");
                    }
                }
                else
                {
                    DebugLog($"⚠️ [PLANNER-WS] Unexpected JsonValueKind for {propName}: {propElement.ValueKind}", "WARNING");
                }
            }
        }
        DebugLog($"⚠️ [PLANNER-WS] Could not extract int from properties: {string.Join(", ", propertyNames)}", "WARNING");
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
                    DebugLog($"🔍 [PLANNER-WS] Extracted {propName}: {stringValue}");
                    return stringValue;
                }
            }
        }
        DebugLog($"⚠️ [PLANNER-WS] Could not extract string from properties: {string.Join(", ", propertyNames)}", "WARNING");
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

    // NEUE FUNKTION: Empfangsbestätigung an Hub senden
    private async Task SendMatchUpdateAcknowledgment(JsonElement message)
    {
        try
        {
            var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdEl) ? tournamentIdEl.GetString() : _currentTournamentId;
            var matchId = "unknown";
            
            // Versuche Match ID zu extrahieren (mit String-Support)
            if (message.TryGetProperty("matchUpdate", out var matchUpdateEl))
            {
                if (matchUpdateEl.TryGetProperty("matchId", out var matchIdEl))
                {
                    matchId = matchIdEl.ToString();
                }
            }
            else if (message.TryGetProperty("matchId", out var directMatchIdEl))
            {
                matchId = directMatchIdEl.ToString();
            }
            
            var acknowledgment = new
            {
                type = "match-update-acknowledged",
                tournamentId = tournamentId,
                matchId = matchId,
                timestamp = DateTime.Now.ToString("o"),
                clientType = "Tournament Planner",
                plannerVersion = "1.0",
                receivedAt = DateTime.Now.ToString("o"),
                status = "received_and_processed"
            };
            
            DebugLog($"📤 [PLANNER-WS] Sending match update acknowledgment:", "WEBSOCKET");
            DebugLog($"📤 [PLANNER-WS] Tournament: {tournamentId}, Match: {matchId}", "WEBSOCKET");
            
            await SendWebSocketMessage("match-update-acknowledged", acknowledgment);
            
            DebugLog($"✅ [PLANNER-WS] Match update acknowledgment sent successfully", "SUCCESS");
        }
        catch (Exception ex)
        {
            DebugLog($"❌ [PLANNER-WS] Failed to send match update acknowledgment: {ex.Message}", "ERROR");
        }
    }

    // NEUE FUNKTION: Fehler-Acknowledgment senden
    private async Task SendErrorAcknowledgment(string errorMessage)
    {
        try
        {
            var errorAck = new
            {
                type = "match-update-error",
                tournamentId = _currentTournamentId,
                error = errorMessage,
                timestamp = DateTime.Now.ToString("o"),
                clientType = "Tournament Planner",
                status = "processing_failed"
            };
            
            await SendWebSocketMessage("match-update-error", errorAck);
            DebugLog($"📤 [PLANNER-WS] Error acknowledgment sent: {errorMessage}", "ERROR");
        }
        catch (Exception ex)
        {
            DebugLog($"❌ [PLANNER-WS] Failed to send error acknowledgment: {ex.Message}", "ERROR");
        }
    }

    // WebSocket Operations
    private async Task<bool> SendWebSocketMessage(string messageType, object data)
    {
        try
        {
            if (_webSocket?.State != WebSocketState.Open) return false;
            
            var message = new
            {
                type = messageType,
                data = data,
                timestamp = DateTime.Now.ToString("o")
            };
            
            var messageJson = JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            
            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _webSocketCancellation.Token);
            
            return true;
        }
        catch (Exception ex)
        {
            DebugLog($"❌ [PLANNER-WS] Error sending WebSocket message: {ex.Message}", "ERROR");
            return false;
        }
    }

    public async Task<bool> SubscribeToTournamentAsync(string tournamentId)
    {
        _currentTournamentId = tournamentId;
        return await SendWebSocketMessage("subscribe-tournament", tournamentId);
    }

    public async Task<bool> UnsubscribeFromTournamentAsync(string tournamentId)
    {
        return await SendWebSocketMessage("unsubscribe-tournament", tournamentId);
    }

    public async Task<bool> RegisterAsPlannerAsync(string tournamentId, object plannerInfo)
    {
        var data = new { tournamentId = tournamentId, plannerInfo = plannerInfo };
        return await SendWebSocketMessage("register-planner", data);
    }

    private async Task SendHeartbeat()
    {
        try
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                await SendWebSocketMessage("heartbeat", new { 
                    timestamp = DateTime.Now.ToString("o"),
                    clientType = "Tournament Planner",
                    tournamentId = _currentTournamentId
                });
                DebugLog($"💓 [PLANNER-WS] Heartbeat sent successfully", "WEBSOCKET");
            }
        }
        catch (Exception ex)
        {
            DebugLog($"❌ [PLANNER-WS] Error sending heartbeat: {ex.Message}", "ERROR");
        }
    }

    public async Task CloseWebSocketAsync()
    {
        try
        {
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            
            if (_webSocket != null)
            {
                _webSocketCancellation?.Cancel();
                
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                }
                
                _webSocket.Dispose();
                _webSocket = null;
            }
            
            _webSocketCancellation?.Dispose();
            _webSocketCancellation = null;
            
            _isWebSocketConnected = false;
            OnConnectionStatusChanged?.Invoke(false, "WebSocket Closed");
            DebugLog("🔌 [PLANNER-WS] WebSocket connection closed", "WEBSOCKET");
        }
        catch (Exception ex)
        {
            DebugLog($"❌ Error closing WebSocket: {ex.Message}", "ERROR");
        }
    }

    // =====================================
    // HTTP API METHODS 
    // =====================================

    public async Task<bool> RegisterWithHubAsync(string tournamentId, string tournamentName, string description = null)
    {
        try
        {
            if (string.IsNullOrEmpty(tournamentId)) return false;

            string apiEndpoint = await DiscoverApiEndpoint();
            var registrationData = new
            {
                tournamentId = tournamentId,
                name = tournamentName,
                description = description ?? $"Dart Tournament: {tournamentName}",
                apiEndpoint = apiEndpoint,
                location = Environment.MachineName,
                apiKey = GenerateApiKey(tournamentId)
            };

            var json = JsonSerializer.Serialize(registrationData, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{HubUrl}/api/tournaments/register", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var jsonDocument = JsonSerializer.Deserialize<JsonElement>(responseText);
                    if (jsonDocument.TryGetProperty("success", out var successElement) && successElement.GetBoolean())
                    {
                        if (jsonDocument.TryGetProperty("data", out var dataElement))
                        {
                            if (dataElement.TryGetProperty("joinUrl", out var joinUrlElement))
                            {
                                _hubData[$"TournamentHub_Registration_{tournamentId}_JoinUrl"] = joinUrlElement.GetString() ?? "";
                            }
                            if (dataElement.TryGetProperty("websocketUrl", out var wsUrlElement))
                            {
                                _hubData[$"TournamentHub_Registration_{tournamentId}_WebsocketUrl"] = wsUrlElement.GetString() ?? "";
                            }
                        }
                        
                        _currentTournamentId = tournamentId;
                        return true;
                    }
                }
                catch (JsonException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [API] JSON parsing error: {ex.Message}");
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [API] Hub registration error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendHeartbeatAsync(string tournamentId, int activeMatches, int totalPlayers)
    {
        try
        {
            var heartbeatData = new { status = "active", activeMatches, totalPlayers };
            var json = JsonSerializer.Serialize(heartbeatData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{HubUrl}/api/tournaments/{tournamentId}/heartbeat", content);
            
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UnregisterFromHubAsync(string tournamentId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{HubUrl}/api/tournaments/{tournamentId}");
            if (response.IsSuccessStatusCode)
            {
                _hubData.Remove($"TournamentHub_Registration_{tournamentId}_JoinUrl");
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SyncTournamentWithClassesAsync(string tournamentId, string tournamentName, TournamentData tournamentData)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔄 [API] Starting full tournament sync with ALL match types: {tournamentId}");
            
            var allMatches = new List<object>();
            var tournamentClasses = new List<object>();
            var gameRulesArray = new List<object>();

            foreach (var tournamentClass in tournamentData.TournamentClasses)
            {
                System.Diagnostics.Debug.WriteLine($"🎮 [API] Processing class {tournamentClass.Name} with Game Rules:");
                System.Diagnostics.Debug.WriteLine($"   Game Mode: {tournamentClass.GameRules.GameMode}");
                System.Diagnostics.Debug.WriteLine($"   Finish Mode: {tournamentClass.GameRules.FinishMode}"); 
                System.Diagnostics.Debug.WriteLine($"   Legs to Win: {tournamentClass.GameRules.LegsToWin}");
                System.Diagnostics.Debug.WriteLine($"   Sets to Win: {tournamentClass.GameRules.SetsToWin}");
                System.Diagnostics.Debug.WriteLine($"   Play With Sets (Original): {tournamentClass.GameRules.PlayWithSets}");
                System.Diagnostics.Debug.WriteLine($"   Legs per Set: {tournamentClass.GameRules.LegsPerSet}");
                
                // 🚨 KORRIGIERT: Debug-Ausgabe für PlayWithSets-Logik
                var correctedPlayWithSets = tournamentClass.GameRules.PlayWithSets || tournamentClass.GameRules.SetsToWin > 1;
                System.Diagnostics.Debug.WriteLine($"   🔧 CORRECTED Play With Sets: {correctedPlayWithSets} (Original: {tournamentClass.GameRules.PlayWithSets}, SetsToWin: {tournamentClass.GameRules.SetsToWin})");                
                
                // ERWEITERT: Zähle alle Match-Typen
                int groupMatches = tournamentClass.Groups.Sum(g => g.Matches.Count);
                int finalsMatches = tournamentClass.CurrentPhase?.FinalsGroup?.Matches.Count ?? 0;
                int winnerBracketMatches = tournamentClass.CurrentPhase?.WinnerBracket?.Count ?? 0;
                int loserBracketMatches = tournamentClass.CurrentPhase?.LoserBracket?.Count ?? 0;
                int totalMatches = groupMatches + finalsMatches + winnerBracketMatches + loserBracketMatches;
                
                System.Diagnostics.Debug.WriteLine($"   📊 Match Count: Groups={groupMatches}, Finals={finalsMatches}, Winner={winnerBracketMatches}, Loser={loserBracketMatches}, Total={totalMatches}");
                
                tournamentClasses.Add(new
                {
                    id = tournamentClass.Id,
                    name = tournamentClass.Name,
                    playerCount = tournamentClass.Groups.Sum(g => g.Players.Count),
                    groupCount = tournamentClass.Groups.Count,
                    matchCount = totalMatches, // KORRIGIERT: Alle Match-Typen einbeziehen
                    phase = GetTournamentPhase(tournamentClass) // NEUE: Aktuelle Phase
                });

                // KORRIGIERT: Game Rules für jede Klasse hinzufügen
                gameRulesArray.Add(new
                {
                    id = tournamentClass.Id,
                    name = $"{tournamentClass.Name} Regel",
                    gamePoints = tournamentClass.GameRules.GamePoints,
                    gameMode = tournamentClass.GameRules.GameMode.ToString(),
                    finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                    setsToWin = tournamentClass.GameRules.SetsToWin,
                    legsToWin = tournamentClass.GameRules.LegsToWin,
                    legsPerSet = tournamentClass.GameRules.LegsPerSet,
                    maxSets = Math.Max(tournamentClass.GameRules.SetsToWin * 2 - 1, 5),
                    maxLegsPerSet = tournamentClass.GameRules.LegsPerSet,
                    playWithSets = tournamentClass.GameRules.PlayWithSets, // 🚨 KORRIGIERT: Exakter Wert vom Planer ohne zusätzliche Logik
                    classId = tournamentClass.Id,
                    className = tournamentClass.Name,
                    matchType = "Group", // Standard für Gruppenphase
                    isDefault = true
                });

                // 🎮 ERWEITERT: Rundenspezifische Game Rules für verschiedene Phasen
                
                // Finals-spezifische Game Rules (falls vorhanden)
                if (tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🏆 [API] Adding Finals-specific GameRules for {tournamentClass.Name}");
                    System.Diagnostics.Debug.WriteLine($"🏆 [API] Finals Rules - PlayWithSets: {tournamentClass.GameRules.PlayWithSets}, SetsToWin: {tournamentClass.GameRules.SetsToWin}");
                    
                    gameRulesArray.Add(new
                    {
                        id = $"{tournamentClass.Id}_Finals",
                        name = $"{tournamentClass.Name} Finalrunde",
                        gamePoints = tournamentClass.GameRules.GamePoints,
                        gameMode = tournamentClass.GameRules.GameMode.ToString(),
                        finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                        setsToWin = tournamentClass.GameRules.SetsToWin, // 🚨 KORRIGIERT: Exakter Wert vom Planer
                        legsToWin = tournamentClass.GameRules.LegsToWin, // 🚨 KORRIGIERT: Exakter Wert vom Planer
                        legsPerSet = tournamentClass.GameRules.LegsPerSet, // 🚨 KORRIGIERT: Exakter Wert vom Planer
                        maxSets = Math.Max(tournamentClass.GameRules.SetsToWin * 2 - 1, 5),
                        maxLegsPerSet = tournamentClass.GameRules.LegsPerSet, // 🚨 KORRIGIERT: Exakter Wert vom Planer
                        playWithSets = tournamentClass.GameRules.PlayWithSets, // 🚨 KORRIGIERT: Exakter Wert vom Planer ohne Logik-Modifikation
                        classId = tournamentClass.Id,
                        className = tournamentClass.Name,
                        matchType = "Finals",
                        isDefault = false
                    });
                }

                // Winner Bracket spezifische Game Rules mit rundenabhängiger Eskalation
                if (tournamentClass.CurrentPhase?.WinnerBracket != null)
                {
                    // Gruppiere Winner Bracket Matches nach Runden
                    var winnerRounds = tournamentClass.CurrentPhase.WinnerBracket
                        .GroupBy(m => m.Round)
                        .ToList();

                    foreach (var roundGroup in winnerRounds)
                    {
                        var round = roundGroup.Key;
                        var matchCount = roundGroup.Count();
                        
                        // Ermittle eskalierte Regeln basierend auf der Runde
                        var (setsToWin, legsToWin) = GetEscalatedRulesForWinnerBracket(round, tournamentClass.GameRules);
                        
                        // 🚨 KORRIGIERT: Verwende auch legsPerSet aus den Round Rules wenn verfügbar
                        int legsPerSet = tournamentClass.GameRules.LegsPerSet; // Default
                        if (tournamentClass.GameRules.KnockoutRoundRules.TryGetValue(round, out var roundRules))
                        {
                            legsPerSet = roundRules.LegsPerSet;
                            System.Diagnostics.Debug.WriteLine($"🎮 [RULES] Using round-specific legsPerSet for round {round}: {legsPerSet}");
                        }
                        else
                        {
                            legsPerSet = Math.Max(tournamentClass.GameRules.LegsPerSet, legsToWin + 2);
                            System.Diagnostics.Debug.WriteLine($"⚠️ [RULES] Using calculated legsPerSet for round {round}: {legsPerSet}");
                        }
                        
                        gameRulesArray.Add(new
                        {
                            id = $"{tournamentClass.Id}_WB_{round}",
                            name = $"{tournamentClass.Name} {GetWinnerBracketRoundName(round)}",
                            gamePoints = tournamentClass.GameRules.GamePoints,
                            gameMode = tournamentClass.GameRules.GameMode.ToString(),
                            finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                            setsToWin = setsToWin,
                            legsToWin = legsToWin,
                            legsPerSet = legsPerSet, // 🚨 KORRIGIERT: Verwende Round-spezifischen Wert
                            // 🚨 KORRIGIERT: PlayWithSets sollte true sein wenn setsToWin > 1 ODER wenn PlayWithSets explizit true ist
                            playWithSets = tournamentClass.GameRules.PlayWithSets || setsToWin > 1,
                            classId = tournamentClass.Id,
                            className = tournamentClass.Name,
                            matchType = $"Knockout-WB-{round}",
                            round = round.ToString(),
                            isDefault = false,
                            matchCount = matchCount
                        });
                    }
                }

                // Loser Bracket spezifische Game Rules (generell schneller)
                if (tournamentClass.CurrentPhase?.LoserBracket != null)
                {
                    var loserRounds = tournamentClass.CurrentPhase.LoserBracket
                        .GroupBy(m => m.Round)
                        .ToList();

                    foreach (var roundGroup in loserRounds)
                    {
                        var round = roundGroup.Key;
                        var matchCount = roundGroup.Count();
                        
                        // Loser Bracket hat generell kürzere Spiele
                        var (setsToWin, legsToWin) = GetLoserBracketRules(round, tournamentClass.GameRules);
                        
                        // 🚨 KORRIGIERT: Verwende auch legsPerSet aus den Round Rules wenn verfügbar
                        int legsPerSet = tournamentClass.GameRules.LegsPerSet; // Default
                        if (tournamentClass.GameRules.KnockoutRoundRules.TryGetValue(round, out var roundRules))
                        {
                            legsPerSet = roundRules.LegsPerSet;
                            System.Diagnostics.Debug.WriteLine($"🎮 [MATCH] Using round-specific legsPerSet for match {round}: {legsPerSet}");
                        }
                        else
                        {
                            legsPerSet = Math.Max(3, legsToWin + 1);
                            System.Diagnostics.Debug.WriteLine($"⚠️ [MATCH] Using calculated legsPerSet for match {round}: {legsPerSet}");
                        }
                        
                        gameRulesArray.Add(new
                        {
                            id = $"{tournamentClass.Id}_LB_{round}",
                            name = $"{tournamentClass.Name} {GetLoserBracketRoundName(round)}",
                            gamePoints = tournamentClass.GameRules.GamePoints,
                            gameMode = tournamentClass.GameRules.GameMode.ToString(),
                            finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                            setsToWin = setsToWin,
                            legsToWin = legsToWin,
                            legsPerSet = legsPerSet, // 🚨 KORRIGIERT: Verwende Round-spezifischen Wert
                            // 🚨 KORRIGIERT: PlayWithSets sollte true sein wenn setsToWin > 1 ODER wenn PlayWithSets explizit true ist
                            playWithSets = tournamentClass.GameRules.PlayWithSets || setsToWin > 1,
                            classId = tournamentClass.Id,
                            className = tournamentClass.Name,
                            matchType = $"Knockout-LB-{round}",
                            round = round.ToString(),
                            isDefault = false,
                            matchCount = matchCount
                        });
                    }
                }

                // 1. GRUPPENPHASEN-MATCHES (wie bisher)
                foreach (var group in tournamentClass.Groups)
                {
                    foreach (var match in group.Matches)
                    {
                        allMatches.Add(new
                        {
                            id = match.Id,
                            matchId = match.Id,
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
                            matchType = "Group",
                            groupId = group.Id,
                            groupName = group.Name,
                            gameRulesId = tournamentClass.Id,
                            gameRulesUsed = new
                            {
                                id = tournamentClass.Id,
                                name = $"{tournamentClass.Name} Regel",
                                gamePoints = tournamentClass.GameRules.GamePoints,
                                gameMode = tournamentClass.GameRules.GameMode.ToString(),
                                finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                                setsToWin = tournamentClass.GameRules.SetsToWin,
                                legsToWin = tournamentClass.GameRules.LegsToWin,
                                legsPerSet = tournamentClass.GameRules.LegsPerSet,
                                maxSets = Math.Max(tournamentClass.GameRules.SetsToWin * 2 - 1, 5), // 🚨 HINZUGEFÜGT
                                maxLegsPerSet = tournamentClass.GameRules.LegsPerSet, // 🚨 HINZUGEFÜGT
                                playWithSets = tournamentClass.GameRules.PlayWithSets, // 🚨 KORRIGIERT: Exakter Wert vom Planer ohne zusätzliche Logik
                                matchType = "Group",
                                classId = tournamentClass.Id, // 🚨 HINZUGEFÜGT
                                className = tournamentClass.Name // 🚨 HINZUGEFÜGT
                            }
                        });
                    }
                }

                // 2. NEUE: FINALRUNDEN-MATCHES
                if (tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🏆 [API] Processing Finals matches for {tournamentClass.Name}: {tournamentClass.CurrentPhase.FinalsGroup.Matches.Count} matches");
                    System.Diagnostics.Debug.WriteLine($"🏆 [API] Finals GameRules - PlayWithSets: {tournamentClass.GameRules.PlayWithSets}, SetsToWin: {tournamentClass.GameRules.SetsToWin}");
                    System.Diagnostics.Debug.WriteLine($"🏆 [API] Finals GameRules - LegsToWin: {tournamentClass.GameRules.LegsToWin}, LegsPerSet: {tournamentClass.GameRules.LegsPerSet}");
                    
                    foreach (var match in tournamentClass.CurrentPhase.FinalsGroup.Matches)
                    {
                        // 🚨 KORRIGIERT: Verwende die exakten GameRules-Werte vom Planer ohne Modifikation
                        var finalsPlayWithSets = tournamentClass.GameRules.PlayWithSets;
                        var finalsSetsToWin = tournamentClass.GameRules.SetsToWin;
                        var finalsLegsToWin = tournamentClass.GameRules.LegsToWin;
                        var finalsLegsPerSet = tournamentClass.GameRules.LegsPerSet;
                        
                        System.Diagnostics.Debug.WriteLine($"🏆 [API] Finals Match {match.Id}: PlayWithSets={finalsPlayWithSets}, UsesSets={match.UsesSets}");
                        
                        allMatches.Add(new
                        {
                            id = match.Id,
                            matchId = match.Id,
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
                            matchType = "Finals", // WICHTIG: Finals Match-Type
                            groupId = (int?)null, // Finals haben keine Gruppe
                            groupName = "Finals", // WICHTIG: Eindeutige Group-Name für Finals
                            gameRulesId = $"{tournamentClass.Id}_Finals",
                            gameRulesUsed = new
                            {
                                id = $"{tournamentClass.Id}_Finals",
                                name = $"{tournamentClass.Name} Finals Regel",
                                gamePoints = tournamentClass.GameRules.GamePoints,
                                gameMode = tournamentClass.GameRules.GameMode.ToString(),
                                finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                                setsToWin = finalsSetsToWin, // 🚨 KORRIGIERT: Exakter Wert vom Planer
                                legsToWin = finalsLegsToWin, // 🚨 KORRIGIERT: Exakter Wert vom Planer
                                legsPerSet = finalsLegsPerSet, // 🚨 KORRIGIERT: Exakter Wert vom Planer
                                maxSets = Math.Max(finalsSetsToWin * 2 - 1, 5), // Berechnet basierend auf exakten Werten
                                maxLegsPerSet = finalsLegsPerSet, // 🚨 KORRIGIERT: Exakter Wert vom Planer
                                playWithSets = finalsPlayWithSets, // 🚨 KORRIGIERT: Exakter Wert vom Planer ohne zusätzliche Logik
                                matchType = "Finals",
                                classId = tournamentClass.Id,
                                className = tournamentClass.Name,
                                isDefault = false
                            }
                        });
                    }
                }

                // 3. KORRIGIERT: WINNER BRACKET MATCHES
                if (tournamentClass.CurrentPhase?.WinnerBracket != null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚡ [API] Processing Winner Bracket matches for {tournamentClass.Name}: {tournamentClass.CurrentPhase.WinnerBracket.Count} matches");
                    
                    foreach (var knockoutMatch in tournamentClass.CurrentPhase.WinnerBracket)
                    {
                        // 🎮 ERWEITERT: Ermittle rundenspezifische Game Rules
                        var (setsToWin, legsToWin) = GetEscalatedRulesForWinnerBracket(knockoutMatch.Round, tournamentClass.GameRules);
                        
                        // 🚨 KORRIGIERT: Verwende auch legsPerSet aus den Round Rules wenn verfügbar
                        int legsPerSet = tournamentClass.GameRules.LegsPerSet; // Default
                        if (tournamentClass.GameRules.KnockoutRoundRules.TryGetValue(knockoutMatch.Round, out var roundRules))
                        {
                            legsPerSet = roundRules.LegsPerSet;
                            System.Diagnostics.Debug.WriteLine($"🎮 [MATCH] Using round-specific legsPerSet for match {knockoutMatch.Id}, round {knockoutMatch.Round}: {legsPerSet}");
                        }
                        else
                        {
                            legsPerSet = Math.Max(tournamentClass.GameRules.LegsPerSet, legsToWin + 2);
                            System.Diagnostics.Debug.WriteLine($"⚠️ [MATCH] Using calculated legsPerSet for match {knockoutMatch.Id}, round {knockoutMatch.Round}: {legsPerSet}");
                        }
                        
                        var gameRuleId = $"{tournamentClass.Id}_WB_{knockoutMatch.Round}";
                        var roundName = GetWinnerBracketRoundName(knockoutMatch.Round);
                        var matchType = GetWinnerBracketMatchType(knockoutMatch);
                        
                        // 🚨 DEBUG: KO Game Rules Übertragung
                        System.Diagnostics.Debug.WriteLine($"🎮 [API] Winner Bracket Match {knockoutMatch.Id}:");
                        System.Diagnostics.Debug.WriteLine($"   Round: {knockoutMatch.Round}");
                        System.Diagnostics.Debug.WriteLine($"   Game Rule ID: {gameRuleId}");
                        System.Diagnostics.Debug.WriteLine($"   Match Type: {matchType}");
                        System.Diagnostics.Debug.WriteLine($"   Sets/Legs/LegsPerSet: {setsToWin}/{legsToWin}/{legsPerSet}");
                        System.Diagnostics.Debug.WriteLine($"   Rule Name: {roundName}");
                        
                        allMatches.Add(new
                        {
                            id = knockoutMatch.Id,
                            matchId = knockoutMatch.Id,
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
                            matchType = matchType, // WICHTIG: Spezifischer Winner Bracket Match-Type
                            groupId = (int?)null, // KO-Matches haben keine traditionelle Gruppe
                            groupName = $"Winner Bracket - {knockoutMatch.Round}", // WICHTIG: Eindeutige Group-Name für Winner Bracket
                            round = knockoutMatch.Round,
                            position = knockoutMatch.Position,
                            gameRulesId = gameRuleId,
                            // 🚨 KORRIGIERT: Verbesserte gameRulesUsed mit allen notwendigen Properties für Web Interface
                            gameRulesUsed = new
                            {
                                id = gameRuleId,
                                name = $"{tournamentClass.Name} {roundName}",
                                gamePoints = tournamentClass.GameRules.GamePoints,
                                gameMode = tournamentClass.GameRules.GameMode.ToString(),
                                finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                                setsToWin = setsToWin,
                                legsToWin = legsToWin,
                                legsPerSet = legsPerSet, // 🚨 KORRIGIERT: Verwende Round-spezifischen Wert
                                maxSets = Math.Max(setsToWin * 2 - 1, 5), // 🚨 HINZUGEFÜGT: maxSets für Web Interface
                                maxLegsPerSet = legsPerSet, // 🚨 KORRIGIERT: Verwende Round-spezifischen Wert
                                // 🚨 KORRIGIERT: PlayWithSets sollte true sein wenn setsToWin > 1 ODER wenn PlayWithSets explizit true ist
                                playWithSets = tournamentClass.GameRules.PlayWithSets || setsToWin > 1,
                                matchType = matchType,
                                round = knockoutMatch.Round.ToString(),
                                classId = tournamentClass.Id, // 🚨 HINZUGEFÜGT: für Web Interface Matching
                                className = tournamentClass.Name, // 🚨 HINZUGEFÜGT
                                isDefault = false
                            }
                        });
                    }
                }

                // 4. KORRIGIERT: LOSER BRACKET MATCHES
                if (tournamentClass.CurrentPhase?.LoserBracket != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 [API] Processing Loser Bracket matches for {tournamentClass.Name}: {tournamentClass.CurrentPhase.LoserBracket.Count} matches");
                    
                    foreach (var knockoutMatch in tournamentClass.CurrentPhase.LoserBracket)
                    {
                        // 🎮 ERWEITERT: Ermittle Loser Bracket spezifische Game Rules
                        var (setsToWin, legsToWin) = GetLoserBracketRules(knockoutMatch.Round, tournamentClass.GameRules);
                        
                        // 🚨 KORRIGIERT: Verwende auch legsPerSet aus den Round Rules wenn verfügbar
                        int legsPerSet = tournamentClass.GameRules.LegsPerSet; // Default
                        if (tournamentClass.GameRules.KnockoutRoundRules.TryGetValue(knockoutMatch.Round, out var roundRules))
                        {
                            legsPerSet = roundRules.LegsPerSet;
                            System.Diagnostics.Debug.WriteLine($"🎮 [MATCH] Using round-specific legsPerSet for match {knockoutMatch.Id}, round {knockoutMatch.Round}: {legsPerSet}");
                        }
                        else
                        {
                            legsPerSet = Math.Max(3, legsToWin + 1);
                            System.Diagnostics.Debug.WriteLine($"⚠️ [MATCH] Using calculated legsPerSet for match {knockoutMatch.Id}, round {knockoutMatch.Round}: {legsPerSet}");
                        }
                        
                        var gameRuleId = $"{tournamentClass.Id}_LB_{knockoutMatch.Round}";
                        var roundName = GetLoserBracketRoundName(knockoutMatch.Round);
                        var matchType = GetLoserBracketMatchType(knockoutMatch);
                        
                        // 🚨 DEBUG: KO Game Rules Übertragung
                        System.Diagnostics.Debug.WriteLine($"🎮 [API] Loser Bracket Match {knockoutMatch.Id}:");
                        System.Diagnostics.Debug.WriteLine($"   Round: {knockoutMatch.Round}");
                        System.Diagnostics.Debug.WriteLine($"   Game Rule ID: {gameRuleId}");
                        System.Diagnostics.Debug.WriteLine($"   Match Type: {matchType}");
                        System.Diagnostics.Debug.WriteLine($"   Sets/Legs/LegsPerSet: {setsToWin}/{legsToWin}/{legsPerSet}");
                        System.Diagnostics.Debug.WriteLine($"   Rule Name: {roundName}");
                        
                        allMatches.Add(new
                        {
                            id = knockoutMatch.Id,
                            matchId = knockoutMatch.Id,
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
                            matchType = matchType, // WICHTIG: Spezifischer Loser Bracket Match-Type
                            groupId = (int?)null, // KO-Matches haben keine traditionelle Gruppe
                            groupName = $"Loser Bracket - {knockoutMatch.Round}", // WICHTIG: Eindeutige Group-Name für Loser Bracket
                            round = knockoutMatch.Round,
                            position = knockoutMatch.Position,
                            gameRulesId = gameRuleId,
                            // 🚨 KORRIGIERT: Verbesserte gameRulesUsed mit allen notwendigen Properties für Web Interface
                            gameRulesUsed = new
                            {
                                id = gameRuleId,
                                name = $"{tournamentClass.Name} {roundName}",
                                gamePoints = tournamentClass.GameRules.GamePoints,
                                gameMode = tournamentClass.GameRules.GameMode.ToString(),
                                finishMode = tournamentClass.GameRules.FinishMode.ToString(),
                                setsToWin = setsToWin,
                                legsToWin = legsToWin,
                                legsPerSet = legsPerSet, // 🚨 KORRIGIERT: Verwende Round-spezifischen Wert
                                maxSets = Math.Max(setsToWin * 2 - 1, 3), // 🚨 HINZUGEFÜGT: maxSets für Web Interface (kürzer für Loser)
                                maxLegsPerSet = legsPerSet, // 🚨 KORRIGIERT: Verwende Round-spezifischen Wert
                                // 🚨 KORRIGIERT: PlayWithSets sollte true sein wenn setsToWin > 1 ODER wenn PlayWithSets explizit true ist
                                playWithSets = tournamentClass.GameRules.PlayWithSets || setsToWin > 1,
                                matchType = matchType,
                                round = knockoutMatch.Round.ToString(),
                                classId = tournamentClass.Id, // 🚨 HINZUGEFÜGT: für Web Interface Matching
                                className = tournamentClass.Name, // 🚨 HINZUGEFÜGT
                                isDefault = false
                            }
                        });
                    }
                }
            }

            var syncData = new
            {
                tournamentId,
                name = tournamentName,
                classes = tournamentClasses,
                matches = allMatches,
                gameRules = gameRulesArray,
                syncedAt = DateTime.UtcNow,
                // NEUE: Zusätzliche Match-Typ Statistiken
                matchTypeStats = new
                {
                    totalMatches = allMatches.Count,
                    groupMatches = allMatches.Count(m => ((dynamic)m).matchType.ToString() == "Group"),
                    finalsMatches = allMatches.Count(m => ((dynamic)m).matchType.ToString() == "Finals"),
                    winnerBracketMatches = allMatches.Count(m => ((dynamic)m).matchType.ToString().StartsWith("Knockout-WB")),
                    loserBracketMatches = allMatches.Count(m => ((dynamic)m).matchType.ToString().StartsWith("Knockout-LB"))
                }
            };

            System.Diagnostics.Debug.WriteLine($"🎯 [API] Tournament sync data prepared:");
            System.Diagnostics.Debug.WriteLine($"   Classes: {tournamentClasses.Count}");
            System.Diagnostics.Debug.WriteLine($"   Total Matches: {allMatches.Count}");
            System.Diagnostics.Debug.WriteLine($"     - Group Matches: {syncData.matchTypeStats.groupMatches}");
            System.Diagnostics.Debug.WriteLine($"     - Finals Matches: {syncData.matchTypeStats.finalsMatches}");
            System.Diagnostics.Debug.WriteLine($"     - Winner Bracket Matches: {syncData.matchTypeStats.winnerBracketMatches}");
            System.Diagnostics.Debug.WriteLine($"     - Loser Bracket Matches: {syncData.matchTypeStats.loserBracketMatches}");
            System.Diagnostics.Debug.WriteLine($"   Game Rules: {gameRulesArray.Count}");

            var json = JsonSerializer.Serialize(syncData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var syncClient = new HttpClient();
            syncClient.Timeout = TimeSpan.FromSeconds(60);
            var response = await syncClient.PostAsync($"{HubUrl}/api/tournaments/{tournamentId}/sync-full", content);
            
            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"✅ [API] Tournament sync successful with ALL match types:");
                System.Diagnostics.Debug.WriteLine($"   📊 Synced: {syncData.matchTypeStats.totalMatches} total matches");
                System.Diagnostics.Debug.WriteLine($"   📊 Game Rules: {gameRulesArray.Count} rules synced");
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ [API] Tournament sync failed: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"❌ [API] Error response: {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [API] Tournament sync error: {ex.Message}");
            return false;
        }
    }

    // NEUE HELPER-METHODEN für verschiedene Match-Typen

    /// <summary>
    /// Bestimmt die aktuelle Tournament-Phase
    /// </summary>
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

    /// <summary>
    /// Ermittelt den Status eines Match
    /// </summary>
    private string GetMatchStatus(Match match)
    {
        return match.Status switch
        {
            MatchStatus.NotStarted => "NotStarted",
            MatchStatus.InProgress => "InProgress", 
            MatchStatus.Finished => "Finished",
            MatchStatus.Bye => "Finished",
            _ => "NotStarted"
        };
    }

    /// <summary>
    /// Ermittelt den Gewinner eines Match
    /// </summary>
    private string GetWinner(Match match)
    {
        if (match.Status != MatchStatus.Finished && match.Status != MatchStatus.Bye)
            return null;
        return match.Winner?.Name;
    }

    /// <summary>
    /// Ermittelt den Status eines KnockoutMatch
    /// </summary>
    private string GetKnockoutMatchStatus(KnockoutMatch match)
    {
        return match.Status switch
        {
            MatchStatus.NotStarted => "NotStarted",
            MatchStatus.InProgress => "InProgress", 
            MatchStatus.Finished => "Finished",
            MatchStatus.Bye => "Finished",
            _ => "NotStarted"
        };
    }

    /// <summary>
    /// Ermittelt den Gewinner eines KnockoutMatch
    /// </summary>
    private string GetKnockoutWinner(KnockoutMatch match)
    {
        if (match.Status != MatchStatus.Finished && match.Status != MatchStatus.Bye)
            return null;
        return match.Winner?.Name;
    }

    /// <summary>
    /// Ermittelt den spezifischen Match-Type für Winner Bracket Matches
    /// </summary>
    private string GetWinnerBracketMatchType(KnockoutMatch match)
    {
        // Verwende eine vereinfachte Rundenbeschreibung basierend auf der Round-Eigenschaft
        return match.Round switch
        {
            KnockoutRound.Final => "Knockout-WB-Final",
            KnockoutRound.Semifinal => "Knockout-WB-Semifinal", 
            KnockoutRound.Quarterfinal => "Knockout-WB-Quarterfinal",
            KnockoutRound.Best16 => "Knockout-WB-Best16",
            KnockoutRound.Best32 => "Knockout-WB-Best32",
            KnockoutRound.Best64 => "Knockout-WB-Best64",
            _ => $"Knockout-WB-{match.Round}"
        };
    }

    /// <summary>
    /// Ermittelt den spezifischen Match-Type für Loser Bracket Matches
    /// </summary>
    private string GetLoserBracketMatchType(KnockoutMatch match)
    {
        // Für Loser Bracket verwenden wir eine einfachere Struktur
        return match.Round switch
        {
            KnockoutRound.LoserFinal => "Knockout-LB-LoserFinal",
            _ => $"Knockout-LB-LoserRound{(int)match.Round}"
        };
    }

    // 🎮 NEUE HELPER-METHODEN für rundenspezifische Game Rules

    /// <summary>
    /// Ermittelt eskalierte Regeln für Winner Bracket basierend auf der Runde
    /// 🚨 KORRIGIERT: Verwendet jetzt die tatsächlichen Round Rules aus GameRules.KnockoutRoundRules
    /// </summary>
    private (int setsToWin, int legsToWin) GetEscalatedRulesForWinnerBracket(KnockoutRound round, GameRules baseRules)
    {
        // 🚨 KORRIGIERT: Verwende die tatsächlichen Round Rules statt eigene Berechnung
        if (baseRules.KnockoutRoundRules.TryGetValue(round, out var roundRules))
        {
            System.Diagnostics.Debug.WriteLine($"🎮 [RULES] Using round-specific rules for {round}: Sets={roundRules.SetsToWin}, Legs={roundRules.LegsToWin}");
            return (roundRules.SetsToWin, roundRules.LegsToWin);
        }
        
        // Fallback: Alte Logik wenn keine rundenspezifischen Regeln vorhanden sind
        System.Diagnostics.Debug.WriteLine($"⚠️ [RULES] No round-specific rules found for {round}, using fallback logic");
        return round switch
        {
            KnockoutRound.Best64 => (Math.Max(2, baseRules.SetsToWin - 1), Math.Max(3, baseRules.LegsToWin)), // Schnelle frühe Runden
            KnockoutRound.Best32 => (Math.Max(2, baseRules.SetsToWin - 1), Math.Max(3, baseRules.LegsToWin)),
            KnockoutRound.Best16 => (baseRules.SetsToWin, baseRules.LegsToWin), // Standard
            KnockoutRound.Quarterfinal => (baseRules.SetsToWin, baseRules.LegsToWin),
            KnockoutRound.Semifinal => (baseRules.SetsToWin, Math.Min(baseRules.LegsToWin + 1, 6)), // Längere wichtige Spiele
            KnockoutRound.Final => (Math.Min(baseRules.SetsToWin + 1, 5), Math.Min(baseRules.LegsToWin + 1, 6)),
            _ => (baseRules.SetsToWin, baseRules.LegsToWin) // Fallback
        };
    }

    /// <summary>
    /// Ermittelt Loser Bracket Regeln (generell schneller als Winner Bracket)
    /// 🚨 KORRIGIERT: Verwendet jetzt die tatsächlichen Round Rules aus GameRules.KnockoutRoundRules
    /// </summary>
    private (int setsToWin, int legsToWin) GetLoserBracketRules(KnockoutRound round, GameRules baseRules)
    {
        // 🚨 KORRIGIERT: Verwende die tatsächlichen Round Rules statt eigene Berechnung
        if (baseRules.KnockoutRoundRules.TryGetValue(round, out var roundRules))
        {
            System.Diagnostics.Debug.WriteLine($"🎮 [RULES] Using round-specific rules for {round}: Sets={roundRules.SetsToWin}, Legs={roundRules.LegsToWin}");
            return (roundRules.SetsToWin, roundRules.LegsToWin);
        }
        
        // Fallback: Alte Logik wenn keine rundenspezifischen Regeln vorhanden sind
        System.Diagnostics.Debug.WriteLine($"⚠️ [RULES] No round-specific rules found for {round}, using fallback logic");
        return round switch
        {
            KnockoutRound.LoserFinal => (baseRules.SetsToWin, Math.Min(baseRules.LegsToWin + 1, 5)), // Loser Final ist wichtig
            _ => (Math.Max(2, baseRules.SetsToWin - 1), baseRules.LegsToWin) // Alle anderen Loser Rounds sind schneller
        };
    }

    /// <summary>
    /// Ermittelt benutzerfreundliche Namen für Winner Bracket Runden
    /// </summary>
    private string GetWinnerBracketRoundName(KnockoutRound round)
    {
        return round switch
        {
            KnockoutRound.Best64 => "K.O. Beste 64",
            KnockoutRound.Best32 => "K.O. Beste 32",
            KnockoutRound.Best16 => "K.O. Beste 16",
            KnockoutRound.Quarterfinal => "K.O. Viertelfinale",
            KnockoutRound.Semifinal => "K.O. Halbfinale",
            KnockoutRound.Final => "K.O. Finale",
            _ => $"K.O. Winner {round}"
        };
    }

    /// <summary>
    /// Ermittelt benutzerfreundliche Namen für Loser Bracket Runden
    /// </summary>
    private string GetLoserBracketRoundName(KnockoutRound round)
    {
        return round switch
        {
            KnockoutRound.LoserFinal => "K.O. Loser Final",
            _ => $"K.O. Loser Runde {(int)round}"
        };
    }

    /// <summary>
    /// Entdeckt den API-Endpunkt
    /// </summary>
    private async Task<string> DiscoverApiEndpoint()
    {
        string apiEndpoint = "http://localhost:5000";
        
        try
        {
            using var testClient = new HttpClient();
            testClient.Timeout = TimeSpan.FromSeconds(3);
            var testResponse = await testClient.GetAsync("http://localhost:5000/health");
            if (!testResponse.IsSuccessStatusCode)
            {
                var alternativePorts = new[] { 5001, 5002, 8080 };
                foreach (var port in alternativePorts)
                {
                    var altUrl = $"http://localhost:{port}/health";
                    var altResponse = await testClient.GetAsync(altUrl);
                    if (altResponse.IsSuccessStatusCode)
                    {
                        apiEndpoint = $"http://localhost:{port}";
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [API] Error discovering API endpoint: {ex.Message}");
        }
        
        return apiEndpoint;
    }

    /// <summary>
    /// Generiert einen API-Schlüssel
    /// </summary>
    private string GenerateApiKey(string tournamentId)
    {
        var combined = $"{tournamentId}_{Environment.MachineName}_{DateTime.UtcNow.Ticks}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(combined))[..20];
    }

    public string GetJoinUrl(string tournamentId)
    {
        if (_hubData.TryGetValue($"TournamentHub_Registration_{tournamentId}_JoinUrl", out var joinUrl))
        {
            return joinUrl;
        }
        
        return $"{HubUrl}/join/{tournamentId}";
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
            
            _webSocketCancellation?.Cancel();
            _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None);
            _webSocket?.Dispose();
            _webSocketCancellation?.Dispose();
            _httpClient?.Dispose();
            _isDisposed = true;
        }
    }
}