using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services.HubWebSocket;
using DartTournamentPlaner.Services.HubSync;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Event-Argumente für Match-Updates vom Hub
/// ERWEITERT: Unterstützt jetzt auch Leg-Updates und Match-Start
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
    
    // Group-Information für eindeutige Match-Identifikation
  public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public string? MatchType { get; set; }
    
    // UUID-Support für erweiterte Match-Identifikation
    public string? MatchUuid { get; set; }
    public string? OriginalMatchIdString { get; set; }
    
    // ✨ NEU: Leg-Update Support
    public bool IsLegUpdate { get; set; }
    public bool IsMatchStarted { get; set; }
    public bool IsMatchCompleted { get; set; }
    public int CurrentLeg { get; set; }
    public int TotalLegs { get; set; }
    
    // ✨ NEU: Leg-spezifische Details
    public List<LegResult>? LegResults { get; set; }
    public int? CurrentPlayer1LegScore { get; set; }
    public int? CurrentPlayer2LegScore { get; set; }
 
    // ✨ NEU: Match-Timing
    public DateTime? MatchStartTime { get; set; }
    public TimeSpan? MatchDuration { get; set; }
    public TimeSpan? CurrentLegDuration { get; set; }
    
    /// <summary>
    /// Gibt zurück, ob eine UUID verfügbar ist
    /// </summary>
    public bool HasUuid => !string.IsNullOrEmpty(MatchUuid);
    
    /// <summary>
    /// Gibt die bevorzugte Match-Identifikation zurück (UUID wenn verfügbar, sonst numerische ID)
    /// </summary>
    public string GetPreferredIdentifier() => HasUuid ? MatchUuid! : MatchId.ToString();
    
    /// <summary>
    /// Gibt eine vollständige Match-Identifikation zurück
    /// </summary>
    public string GetMatchIdentificationSummary() => 
        $"Match {(HasUuid ? $"UUID: {MatchUuid}" : $"ID: {MatchId}")} " +
  $"({MatchType ?? "Unknown"}, Class: {ClassId}, Group: {GroupName ?? GroupId?.ToString() ?? "None"})";
    
    /// <summary>
    /// ✨ NEU: Gibt eine lesbare Status-Beschreibung zurück
/// </summary>
  public string GetStatusDescription()
    {
        if (IsMatchStarted && !IsMatchCompleted)
    return $"In Progress - Leg {CurrentLeg}/{TotalLegs}";
        if (IsMatchCompleted)
 return "Completed";
        return "Not Started";
    }
}

/// <summary>
/// ✨ NEU: Leg-Ergebnis für detaillierte Tracking
/// </summary>
public class LegResult
{
    public int LegNumber { get; set; }
    public string? Winner { get; set; } // "Player1" oder "Player2"
    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public int Player1Darts { get; set; }
    public int Player2Darts { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime CompletedAt { get; set; }
    
    // Optionale detaillierte Statistiken
    public int? Player1Average { get; set; }
    public int? Player2Average { get; set; }
    public int? Player1HighestScore { get; set; }
    public int? Player2HighestScore { get; set; }
    public int? Player1Checkout { get; set; }
    public int? Player2Checkout { get; set; }
}

/// <summary>
/// Service für die Kommunikation mit dem Tournament Hub
/// REFACTORED: Hauptklasse delegiert an spezialisierte Manager
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
 
    // ✅ NEU: WebSocket Status
    bool IsWebSocketConnected { get; }
    
    // Events for WebSocket communication
    event Action<HubMatchUpdateEventArgs> OnMatchResultReceivedFromHub;
    event Action<string, object> OnTournamentUpdateReceived;
    event Action<bool, string> OnConnectionStatusChanged;
    
 // ✨ NEU: Events für Live-Updates
    event Action<HubMatchUpdateEventArgs> OnMatchStarted;
    event Action<HubMatchUpdateEventArgs> OnLegCompleted;
    event Action<HubMatchUpdateEventArgs> OnMatchProgressUpdated;
    
    // ✅ NEW: Event für PowerScoring Messages (delegiert an WebSocketMessageHandler)
    event EventHandler<PowerScore.PowerScoringHubMessage> OnPowerScoringMessageReceived;
    
    // Utility
  string GetJoinUrl(string tournamentId);
}

/// <summary>
/// Implementierung des Tournament Hub Service mit modularer Architektur
/// REFACTORED: Verwendet spezialisierte Manager für verschiedene Aufgaben
/// </summary>
public class TournamentHubService : ITournamentHubService, IDisposable
{
    private readonly ConfigService _configService;
    private readonly HttpClient _httpClient;

    // Hub configuration
    public string HubUrl { get; set; }
    private readonly Dictionary<string, string> _hubData = new Dictionary<string, string>();

    // Specialized Managers
    private readonly WebSocketConnectionManager _connectionManager;
    private readonly WebSocketMessageHandler _messageHandler;
    private readonly TournamentDataSyncService _syncService;

    // Current state
    private string? _currentTournamentId;
    private bool _isDisposed = false;

    // Events (delegated from managers)
    public event Action<HubMatchUpdateEventArgs>? OnMatchResultReceivedFromHub;
    public event Action<string, object>? OnTournamentUpdateReceived;
    public event Action<bool, string>? OnConnectionStatusChanged;
  
    // ✨ NEU: Events für Live-Updates (delegiert an WebSocketMessageHandler)
    public event Action<HubMatchUpdateEventArgs>? OnMatchStarted
    {
        add => _messageHandler.MatchStarted += value;
        remove => _messageHandler.MatchStarted -= value;
    }
    
    public event Action<HubMatchUpdateEventArgs>? OnLegCompleted
    {
        add => _messageHandler.LegCompleted += value;
        remove => _messageHandler.LegCompleted -= value;
    }
  
    public event Action<HubMatchUpdateEventArgs>? OnMatchProgressUpdated
 {
        add => _messageHandler.MatchProgressUpdated += value;
        remove => _messageHandler.MatchProgressUpdated -= value;
    }

    // ✅ NEW: Event für PowerScoring Messages (delegiert an WebSocketMessageHandler)
    public event EventHandler<PowerScore.PowerScoringHubMessage>? OnPowerScoringMessageReceived
    {
        add => _messageHandler.PowerScoringMessageReceived += value;
        remove => _messageHandler.PowerScoringMessageReceived -= value;
    }

    // Hub Debug Window Support
    private Views.HubDebugWindow? _debugWindow;

    public TournamentHubService(ConfigService configService)
    {
        _configService = configService;
        // ✅ FIXED: Lade Hub-URL aus Config statt fest zu kodieren
        HubUrl = _configService.Config.HubUrl ?? "https://dtp.i3ull3t.de";

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DartTournamentPlaner/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Initialize specialized managers
        _connectionManager = new WebSocketConnectionManager(HubUrl, DebugLog, ProcessWebSocketMessage);
        _messageHandler = new WebSocketMessageHandler(DebugLog, SendMatchUpdateAcknowledgment, SendErrorAcknowledgment);
        _syncService = new TournamentDataSyncService(_httpClient, HubUrl, DebugLog);

        // Wire up events
        _connectionManager.ConnectionStatusChanged += OnConnectionStatusChangedInternal;
        _messageHandler.MatchResultReceived += OnMatchResultReceivedInternal;
        _messageHandler.TournamentUpdateReceived += OnTournamentUpdateReceivedInternal;

        DebugLog($"🎯 TournamentHubService initialized with modular architecture", "SUCCESS");
        DebugLog($"🔗 Hub URL: {HubUrl}", "INFO");
    }

    #region Debug Window Management

    /// <summary>
    /// Setzt das HubDebugWindow für erweiterte Logging-Ausgaben
    /// </summary>
    public void SetDebugWindow(Views.HubDebugWindow debugWindow)
    {
        _debugWindow = debugWindow;
        DebugLog("🔧 Debug Window connected to TournamentHubService", "SUCCESS");

        // Update connection status in debug window
        if (_connectionManager.IsConnected)
        {
            _debugWindow?.UpdateConnectionStatus(true, "WebSocket Connected");
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
    /// ✅ NEU: Gibt zurück ob WebSocket verbunden ist
    /// </summary>
    public bool IsWebSocketConnected => _connectionManager?.IsConnected ?? false;

    /// <summary>
    /// Unified Debug Logging - sendet an Visual Studio Debug und HubDebugWindow
    /// </summary>
    private void DebugLog(string message, string category = "INFO")
    {
        // Always send to Visual Studio Debug
        System.Diagnostics.Debug.WriteLine(message);

        // Also send to HubDebugWindow if available
        try
        {
            _debugWindow?.AddDebugMessage(message, category);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error sending to HubDebugWindow: {ex.Message}");
        }
    }

    #endregion

    #region WebSocket Operations (Delegated to Managers)

    /// <summary>
    /// Initialisiert WebSocket-Verbindung (delegiert an ConnectionManager)
    /// </summary>
    public async Task<bool> InitializeWebSocketAsync()
    {
        DebugLog("🔌 [HUB-SERVICE] Initializing WebSocket via ConnectionManager...", "WEBSOCKET");
        return await _connectionManager.InitializeAsync();
    }

    /// <summary>
    /// Verarbeitet WebSocket-Nachrichten (delegiert an MessageHandler)
    /// </summary>
    private async Task ProcessWebSocketMessage(string messageJson)
    {
        await _messageHandler.ProcessWebSocketMessage(messageJson);
    }

    /// <summary>
    /// Abonniert Tournament-Updates
    /// </summary>
    public async Task<bool> SubscribeToTournamentAsync(string tournamentId)
    {
        _currentTournamentId = tournamentId;
        DebugLog($"📡 [HUB-SERVICE] Subscribing to tournament: {tournamentId}", "WEBSOCKET");
        return await _connectionManager.SubscribeToTournamentAsync(tournamentId);
    }

    /// <summary>
    /// Deabonniert Tournament-Updates
    /// </summary>
    public async Task<bool> UnsubscribeFromTournamentAsync(string tournamentId)
    {
        DebugLog($"📡 [HUB-SERVICE] Unsubscribing from tournament: {tournamentId}", "WEBSOCKET");
        return await _connectionManager.UnsubscribeFromTournamentAsync(tournamentId);
    }

    /// <summary>
    /// Registriert als Planner
    /// </summary>
    public async Task<bool> RegisterAsPlannerAsync(string tournamentId, object plannerInfo)
    {
        DebugLog($"📋 [HUB-SERVICE] Registering as planner for: {tournamentId}", "WEBSOCKET");
        return await _connectionManager.RegisterAsPlannerAsync(tournamentId, plannerInfo);
    }

    /// <summary>
    /// Schließt WebSocket-Verbindung
    /// </summary>
    public async Task CloseWebSocketAsync()
    {
        DebugLog("🔌 [HUB-SERVICE] Closing WebSocket connection...", "WEBSOCKET");
        await _connectionManager.CloseAsync();
    }

    #endregion

    #region HTTP API Operations

    /// <summary>
    /// Registriert Tournament beim Hub
    /// </summary>
    public async Task<bool> RegisterWithHubAsync(string tournamentId, string tournamentName, string description = null)
    {
        try
        {
            if (string.IsNullOrEmpty(tournamentId)) return false;

            DebugLog($"📤 [HUB-SERVICE] Registering tournament with Hub: {tournamentId}", "API");

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
                ProcessRegistrationResponse(tournamentId, responseText);
                _currentTournamentId = tournamentId;

                DebugLog($"✅ [HUB-SERVICE] Tournament registered successfully: {tournamentId}", "SUCCESS");
                return true;
            }

            DebugLog($"❌ [HUB-SERVICE] Tournament registration failed: {response.StatusCode}", "ERROR");
            return false;
        }
        catch (Exception ex)
        {
            DebugLog($"❌ [HUB-SERVICE] Hub registration error: {ex.Message}", "ERROR");
            return false;
        }
    }

    /// <summary>
    /// Sendet Heartbeat an Hub
    /// </summary>
    public async Task<bool> SendHeartbeatAsync(string tournamentId, int activeMatches, int totalPlayers)
    {
        try
        {
            var heartbeatData = new { status = "active", activeMatches, totalPlayers };
            var json = JsonSerializer.Serialize(heartbeatData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{HubUrl}/api/tournaments/{tournamentId}/heartbeat", content);

            if (response.IsSuccessStatusCode)
            {
                DebugLog($"💓 [HUB-SERVICE] Heartbeat sent successfully for: {tournamentId}", "API");
            }

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deregistriert Tournament vom Hub
    /// </summary>
    public async Task<bool> UnregisterFromHubAsync(string tournamentId)
    {
        try
        {
            DebugLog($"📤 [HUB-SERVICE] Unregistering tournament: {tournamentId}", "API");

            var response = await _httpClient.DeleteAsync($"{HubUrl}/api/tournaments/{tournamentId}");
            if (response.IsSuccessStatusCode)
            {
                _hubData.Remove($"TournamentHub_Registration_{tournamentId}_JoinUrl");
                DebugLog($"✅ [HUB-SERVICE] Tournament unregistered successfully: {tournamentId}", "SUCCESS");
                return true;
            }

            DebugLog($"❌ [HUB-SERVICE] Tournament unregistration failed: {response.StatusCode}", "ERROR");
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Synchronisiert Tournament-Daten (delegiert an SyncService)
    /// </summary>
    public async Task<bool> SyncTournamentWithClassesAsync(string tournamentId, string tournamentName, TournamentData tournamentData)
    {
        DebugLog($"🔄 [HUB-SERVICE] Starting tournament sync via SyncService...", "SYNC");
        return await _syncService.SyncTournamentWithClassesAsync(tournamentId, tournamentName, tournamentData);
    }

    #endregion

    #region Event Handling (Internal)

    private void OnConnectionStatusChangedInternal(bool isConnected, string status)
    {
        DebugLog($"🔌 [HUB-SERVICE] Connection status changed: {isConnected} - {status}", "WEBSOCKET");
        OnConnectionStatusChanged?.Invoke(isConnected, status);
    }

    private void OnMatchResultReceivedInternal(HubMatchUpdateEventArgs eventArgs)
    {
        DebugLog($"📥 [HUB-SERVICE] Match result received: {eventArgs.GetMatchIdentificationSummary()}", "MATCH_RESULT");
        OnMatchResultReceivedFromHub?.Invoke(eventArgs);
    }

    private void OnTournamentUpdateReceivedInternal(string tournamentId, object updateData)
    {
        DebugLog($"📥 [HUB-SERVICE] Tournament update received for: {tournamentId}", "TOURNAMENT");
        OnTournamentUpdateReceived?.Invoke(tournamentId, updateData);
    }

    #endregion

    #region WebSocket Message Acknowledgments

    private async Task SendMatchUpdateAcknowledgment(JsonElement message)
    {
        try
        {
            var tournamentId = message.TryGetProperty("tournamentId", out var tournamentIdEl) ? tournamentIdEl.GetString() : _currentTournamentId;
            var matchId = "unknown";

            // Versuche Match ID zu extrahieren
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

            DebugLog($"📤 [HUB-SERVICE] Sending match update acknowledgment", "WEBSOCKET");
            await _connectionManager.SendMessageAsync("match-update-acknowledged", acknowledgment);
            DebugLog($"✅ [HUB-SERVICE] Match update acknowledgment sent successfully", "SUCCESS");
        }
        catch (Exception ex)
        {
            DebugLog($"❌ [HUB-SERVICE] Failed to send match update acknowledgment: {ex.Message}", "ERROR");
        }
    }

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

            await _connectionManager.SendMessageAsync("match-update-error", errorAck);
            DebugLog($"📤 [HUB-SERVICE] Error acknowledgment sent: {errorMessage}", "ERROR");
        }
        catch (Exception ex)
        {
            DebugLog($"❌ [HUB-SERVICE] Failed to send error acknowledgment: {ex.Message}", "ERROR");
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gibt Join-URL zurück
    /// </summary>
    public string GetJoinUrl(string tournamentId)
    {
        // ✅ FIXED: Verwende IMMER die konfigurierte HubUrl statt der vom Server gelieferten
        // Dies stellt sicher, dass die Join-URL der konfigurierten Domain/Port entspricht
        return $"{HubUrl}/join/{tournamentId}";
        
        /* ALTE LOGIK (verwendet Server-URL mit potentiell falschem Port):
        if (_hubData.TryGetValue($"TournamentHub_Registration_{tournamentId}_JoinUrl", out var joinUrl))
        {
            return joinUrl;
        }
        return $"{HubUrl}/join/{tournamentId}";
        */
    }

    /// <summary>
    /// Entdeckt API-Endpunkt
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
            DebugLog($"❌ [HUB-SERVICE] Error discovering API endpoint: {ex.Message}", "ERROR");
        }

        return apiEndpoint;
    }

    /// <summary>
    /// Generiert API-Schlüssel
    /// </summary>
    private string GenerateApiKey(string tournamentId)
    {
        var combined = $"{tournamentId}_{Environment.MachineName}_{DateTime.UtcNow.Ticks}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(combined))[..20];
    }

    /// <summary>
    /// Verarbeitet Registration Response
    /// </summary>
    private void ProcessRegistrationResponse(string tournamentId, string responseText)
    {
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
            }
        }
        catch (JsonException ex)
        {
            DebugLog($"❌ [HUB-SERVICE] JSON parsing error in registration response: {ex.Message}", "ERROR");
        }
    }

    #endregion

    #region Legacy Compatibility Methods

    // These methods maintain compatibility with existing code that uses the old interface
    public event Action<HubMatchUpdateEventArgs>? MatchResultReceived
    {
        add => OnMatchResultReceivedFromHub += value;
        remove => OnMatchResultReceivedFromHub -= value;
    }

    public event Action<string, object>? TournamentUpdateReceived
    {
        add => OnTournamentUpdateReceived += value;
        remove => OnTournamentUpdateReceived -= value;
    }

    public event Action<bool, string>? ConnectionStatusChanged
    {
        add => OnConnectionStatusChanged += value;
        remove => OnConnectionStatusChanged -= value;
    }

    public async Task<bool> DisconnectAsync()
    {
        await CloseWebSocketAsync();
        return true;
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _connectionManager?.Dispose();
            _httpClient?.Dispose();
            _isDisposed = true;

            DebugLog("🗑️ [HUB-SERVICE] TournamentHubService disposed", "INFO");
        }
    }

    #endregion
}