using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services.HubWebSocket;

/// <summary>
/// Verwaltet WebSocket-Verbindungen zum Tournament Hub
/// </summary>
public class WebSocketConnectionManager : IDisposable
{
    private readonly string _hubUrl;
    private readonly Action<string, string> _debugLog;
    private readonly Func<string, Task> _messageProcessor;

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private System.Timers.Timer? _heartbeatTimer;
    private bool _isConnected = false;
    private string? _connectedEndpoint;
    private bool _isDisposed = false;
    private bool _reconnectScheduled = false; // ✅ NEW: Track if reconnect is already scheduled

    // Events
    public event Action<bool, string>? ConnectionStatusChanged;

    public bool IsConnected => _isConnected;
    public string? ConnectedEndpoint => _connectedEndpoint;

    public WebSocketConnectionManager(string hubUrl, Action<string, string> debugLog, Func<string, Task> messageProcessor)
    {
        _hubUrl = hubUrl;
        _debugLog = debugLog;
        _messageProcessor = messageProcessor;
    }

    /// <summary>
    /// Initialisiert WebSocket-Verbindung zum Hub
    /// </summary>
    public async Task<bool> InitializeAsync()
    {
        try
        {
            _debugLog("🔌 [WS-CONNECTION] ===== INITIALIZING WEBSOCKET =====", "WEBSOCKET");
            _debugLog($"🔌 [WS-CONNECTION] Hub URL: {_hubUrl}", "INFO");
            _debugLog($"🔌 [WS-CONNECTION] Current _isConnected: {_isConnected}", "INFO");
            _debugLog($"🔌 [WS-CONNECTION] Current WebSocket State: {_webSocket?.State.ToString() ?? "null"}", "INFO");

            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                _debugLog("✅ [WS-CONNECTION] WebSocket already connected, skipping initialization", "SUCCESS");
                return true;
            }

            // Dispose existing WebSocket if any
            _debugLog("🧹 [WS-CONNECTION] Cleaning up previous WebSocket connection...", "WEBSOCKET");
            await CloseAsync();

            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            // Configure WebSocket
            _webSocket.Options.SetRequestHeader("User-Agent", "DartTournamentPlaner-WebSocket/1.0");

            // SSL WebSocket-Endpunkte für Tournament Hub
            string[] possibleEndpoints = {
                $"{_hubUrl.Replace("https://", "wss://").Replace("http://", "ws://")}/ws",     // ✅ Konfigurierbare URL mit /ws
                $"{_hubUrl.Replace("https://", "wss://").Replace("http://", "ws://")}/socket.io/?EIO=4&transport=websocket"  // ✅ Socket.IO Fallback
            };
            
            _debugLog($"🔍 [WS-CONNECTION] Will try {possibleEndpoints.Length} endpoints", "INFO");

            bool connected = false;
            Exception? lastException = null;

            foreach (var endpoint in possibleEndpoints)
            {
                try
                {
                    _debugLog($"🔌 [WS-CONNECTION] Trying endpoint: {endpoint}", "WEBSOCKET");

                    // Reset WebSocket for each attempt
                    _webSocket?.Dispose();
                    _webSocket = new ClientWebSocket();
                    _webSocket.Options.SetRequestHeader("User-Agent", "DartTournamentPlaner-WebSocket/1.0");

                    // SSL-spezifische Konfiguration
                    if (endpoint.StartsWith("wss://"))
                    {
                        _webSocket.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            _debugLog($"🔒 [WS-CONNECTION] SSL Certificate validation: {sslPolicyErrors}", "WEBSOCKET");
                            return true; // Für Productions-SSL: Akzeptiere auch selbst-signierte Zertifikate
                        };
                    }

                    _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

                    var uri = new Uri(endpoint);
                    var connectionTimeout = endpoint.StartsWith("wss://") ? 30 : 15;
                    var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(connectionTimeout));
                    
                    _debugLog($"⏰ [WS-CONNECTION] Attempting connection with {connectionTimeout}s timeout...", "WEBSOCKET");

                    await _webSocket.ConnectAsync(uri, timeoutCts.Token);

                    if (_webSocket.State == WebSocketState.Open)
                    {
                        _debugLog($"✅ [WS-CONNECTION] Connected successfully to: {endpoint}", "SUCCESS");
                        _connectedEndpoint = endpoint;
                        connected = true;
                        break;
                    }
                    else
                    {
                        _debugLog($"⚠️ [WS-CONNECTION] Connection attempt completed but state is: {_webSocket.State}", "WARNING");
                    }
                }
                catch (Exception ex)
                {
                    _debugLog($"❌ [WS-CONNECTION] Failed to connect to {endpoint}: {ex.Message}", "ERROR");
                    lastException = ex;
                    continue;
                }
            }

            if (!connected)
            {
                _debugLog($"❌ [WS-CONNECTION] All WebSocket connection attempts failed", "ERROR");
                _debugLog($"❌ [WS-CONNECTION] Last error: {lastException?.Message}", "ERROR");

                _isConnected = false;
                _debugLog($"🔔 [WS-CONNECTION] Firing ConnectionStatusChanged event: false", "WEBSOCKET");
                ConnectionStatusChanged?.Invoke(false, $"Connection failed: {lastException?.Message}");
                
                // ✅ CRITICAL FIX: Schedule another reconnect attempt after failed connection
                _debugLog($"🔄 [WS-CONNECTION] Connection failed, scheduling retry...", "WEBSOCKET");
                ScheduleReconnect(10); // Warte 10 Sekunden nach fehlgeschlagenem Versuch
                
                return false;
            }

            _isConnected = true;
            _reconnectScheduled = false; // ✅ FIX: Reset reconnect flag on successful connection
            _debugLog($"✅ [WS-CONNECTION] Setting _isConnected = true", "SUCCESS");
            _debugLog($"🔔 [WS-CONNECTION] Firing ConnectionStatusChanged event: true", "WEBSOCKET");
            ConnectionStatusChanged?.Invoke(true, $"WebSocket Connected ({_connectedEndpoint})");

            // Start listening for messages and heartbeat
            _debugLog($"👂 [WS-CONNECTION] Starting message listener...", "WEBSOCKET");
            _ = Task.Run(ListenForMessages);
            
            _debugLog($"💓 [WS-CONNECTION] Starting heartbeat...", "WEBSOCKET");
            StartHeartbeat();

            _debugLog("✅ [WS-CONNECTION] WebSocket connection established successfully", "SUCCESS");
            _debugLog("✅ [WS-CONNECTION] ===== INITIALIZATION COMPLETE =====", "SUCCESS");
            return true;
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error initializing WebSocket: {ex.Message}", "ERROR");
            _debugLog($"❌ [WS-CONNECTION] Stack trace: {ex.StackTrace}", "ERROR");
            _isConnected = false;
            _debugLog($"🔔 [WS-CONNECTION] Firing ConnectionStatusChanged event: false (exception)", "WEBSOCKET");
            ConnectionStatusChanged?.Invoke(false, $"Connection Error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Lauscht auf WebSocket-Nachrichten
    /// </summary>
    private async Task ListenForMessages()
    {
        try
        {
            _debugLog("👂 [WS-CONNECTION] Starting to listen for WebSocket messages...", "WEBSOCKET");
            var buffer = new byte[8192];

            while (_webSocket?.State == WebSocketState.Open && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
                    using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, timeoutCts.Token);

                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), combinedCts.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _debugLog($"📥 [WS-CONNECTION] Raw WebSocket message received ({result.Count} bytes): {messageJson}", "WEBSOCKET");

                        await _messageProcessor(messageJson);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _debugLog($"🔌 [WS-CONNECTION] WebSocket close message received: {result.CloseStatus} - {result.CloseStatusDescription}", "WEBSOCKET");
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    _debugLog("⏰ [WS-CONNECTION] WebSocket receive operation cancelled or timed out", "WARNING");
                    break;
                }
                catch (WebSocketException wsEx)
                {
                    _debugLog($"🔌 [WS-CONNECTION] WebSocket exception: {wsEx.Message}", "ERROR");
                    break;
                }
            }

            _debugLog($"👂 [WS-CONNECTION] Stopped listening. WebSocket state: {_webSocket?.State}", "WEBSOCKET");
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error listening to WebSocket: {ex.Message}", "ERROR");
        }
        finally
        {
            _isConnected = false;
            _debugLog($"🔌 [WS-CONNECTION] Setting _isConnected = false", "WEBSOCKET");
            _debugLog($"🔔 [WS-CONNECTION] Firing ConnectionStatusChanged event: false", "WEBSOCKET");
            ConnectionStatusChanged?.Invoke(false, "WebSocket Disconnected");

            // ✅ CRITICAL FIX: Schedule reconnect only if not disposed
            if (!_isDisposed && _cancellationTokenSource?.Token.IsCancellationRequested != true)
            {
                ScheduleReconnect(5);
            }
            else
            {
                _debugLog($"⚠️ [WS-CONNECTION] Not scheduling reconnect - disposed or cancelled", "WARNING");
            }
        }
    }

    /// <summary>
    /// Startet Heartbeat-Timer
    /// </summary>
    private void StartHeartbeat()
    {
        _heartbeatTimer = new System.Timers.Timer(30000); // 30 Sekunden
        _heartbeatTimer.Elapsed += async (sender, e) => await SendHeartbeat();
        _heartbeatTimer.Start();

        _debugLog("💓 [WS-CONNECTION] Heartbeat timer started (30s interval)", "WEBSOCKET");
    }

    /// <summary>
    /// Sendet Heartbeat-Nachricht
    /// </summary>
    private async Task SendHeartbeat()
    {
        try
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                var heartbeatData = new
                {
                    type = "heartbeat",
                    timestamp = DateTime.Now.ToString("o"),
                    clientType = "Tournament Planner"
                };

                await SendMessageAsync("heartbeat", heartbeatData);
                _debugLog("💓 [WS-CONNECTION] Heartbeat sent successfully", "WEBSOCKET");
            }
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error sending heartbeat: {ex.Message}", "ERROR");
        }
    }

    /// <summary>
    /// Sendet WebSocket-Nachricht
    /// </summary>
    public async Task<bool> SendMessageAsync(string messageType, object data)
    {
        try
        {
            if (_webSocket?.State != WebSocketState.Open)
            {
                _debugLog($"❌ [WS-CONNECTION] Cannot send message '{messageType}' - WebSocket not open (State: {_webSocket?.State})", "ERROR");
                return false;
            }

            var message = new
            {
                type = messageType,
                data = data,
                timestamp = DateTime.Now.ToString("o")
            };

            var messageJson = System.Text.Json.JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            // ✅ ERWEITERT: Detailliertes Logging vor dem Senden
            _debugLog($"📤 [WS-CONNECTION] ===== SENDING MESSAGE =====", "WEBSOCKET");
            _debugLog($"📤 [WS-CONNECTION] Type: {messageType}", "WEBSOCKET");
            _debugLog($"📤 [WS-CONNECTION] Data: {System.Text.Json.JsonSerializer.Serialize(data)}", "WEBSOCKET");
            _debugLog($"📤 [WS-CONNECTION] Full JSON: {messageJson}", "WEBSOCKET");
            _debugLog($"📤 [WS-CONNECTION] Message size: {messageBytes.Length} bytes", "WEBSOCKET");
            _debugLog($"📤 [WS-CONNECTION] WebSocket state: {_webSocket.State}", "WEBSOCKET");

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);

            _debugLog($"✅ [WS-CONNECTION] Message '{messageType}' sent successfully", "SUCCESS");
            _debugLog($"📤 [WS-CONNECTION] ================================", "WEBSOCKET");
            
            return true;
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error sending WebSocket message '{messageType}': {ex.Message}", "ERROR");
            _debugLog($"❌ [WS-CONNECTION] Stack trace: {ex.StackTrace}", "ERROR");
            return false;
        }
    }

    public async Task<bool> SendRawMessageAsync(object message)
    {
        try
        {
            if (_webSocket?.State != WebSocketState.Open)
            {
                _debugLog($"❌ [WS-CONNECTION] Cannot send raw message - WebSocket not open (State: {_webSocket?.State})", "ERROR");
                return false;
            }

            var messageJson = System.Text.Json.JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);
            var messageType = message.GetType().GetProperty("Type")?.GetValue(message)?.ToString() ?? "unknown";

            _debugLog($"📤 [WS-CONNECTION] ===== SENDING RAW MESSAGE =====", "WEBSOCKET");
            _debugLog($"📤 [WS-CONNECTION] Type: {messageType}", "WEBSOCKET");
            _debugLog($"📤 [WS-CONNECTION] Full JSON: {messageJson}", "WEBSOCKET");
            _debugLog($"📤 [WS-CONNECTION] Message size: {messageBytes.Length} bytes", "WEBSOCKET");

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);

            _debugLog($"✅ [WS-CONNECTION] Raw message '{messageType}' sent successfully", "SUCCESS");
            _debugLog($"📤 [WS-CONNECTION] ================================", "WEBSOCKET");

            return true;
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error sending raw WebSocket message: {ex.Message}", "ERROR");
            return false;
        }
    }

    public Task<bool> SendPlannerRegistrationAsync(PlannerTournamentRegistrationRequest request)
    {
        return SendRawMessageAsync(request);
    }

    public Task<bool> SendPlannerFetchTournamentsAsync(PlannerFetchTournamentsRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.LicenseKey))
        {
            _debugLog("❌ [WS-CONNECTION] Invalid fetch request - license key missing", "ERROR");
            return Task.FromResult(false);
        }

        _debugLog($"📄 [WS-CONNECTION] Sending planner-fetch-tournaments for license: {request.LicenseKey}, days: {request.Days}", "WEBSOCKET");
        return SendRawMessageAsync(request);
    }

    /// <summary>
    /// Abonniert Tournament-Updates
    /// </summary>
    public async Task<bool> SubscribeToTournamentAsync(string tournamentId)
    {
        _debugLog($"📡 [WS-CONNECTION] ===== SUBSCRIBING TO TOURNAMENT =====", "WEBSOCKET");
        _debugLog($"📡 [WS-CONNECTION] Tournament ID: {tournamentId}", "WEBSOCKET");
        _debugLog($"📡 [WS-CONNECTION] WebSocket State: {_webSocket?.State}", "WEBSOCKET");
        _debugLog($"📡 [WS-CONNECTION] Is Connected: {_isConnected}", "WEBSOCKET");
        _debugLog($"📡 [WS-CONNECTION] Connected Endpoint: {_connectedEndpoint}", "WEBSOCKET");
        
        var result = await SendMessageAsync("subscribe-tournament", tournamentId);
        
        _debugLog($"📡 [WS-CONNECTION] Subscribe result: {result}", result ? "SUCCESS" : "ERROR");
        _debugLog($"📡 [WS-CONNECTION] ================================", "WEBSOCKET");
        
        return result;
    }

    /// <summary>
    /// Deabonniert Tournament-Updates
    /// </summary>
    public async Task<bool> UnsubscribeFromTournamentAsync(string tournamentId)
    {
        _debugLog($"📡 [WS-CONNECTION] ===== UNSUBSCRIBING FROM TOURNAMENT =====", "WEBSOCKET");
        _debugLog($"📡 [WS-CONNECTION] Tournament ID: {tournamentId}", "WEBSOCKET");
        
        var result = await SendMessageAsync("unsubscribe-tournament", tournamentId);
        
        _debugLog($"📡 [WS-CONNECTION] Unsubscribe result: {result}", result ? "SUCCESS" : "ERROR");
        _debugLog($"📡 [WS-CONNECTION] ================================", "WEBSOCKET");
        
        return result;
    }

    /// <summary>
    /// Registriert als Planner
    /// </summary>
    public async Task<bool> RegisterAsPlannerAsync(string tournamentId, object plannerInfo)
    {
        _debugLog($"📋 [WS-CONNECTION] ===== REGISTERING AS PLANNER =====", "WEBSOCKET");
        _debugLog($"📋 [WS-CONNECTION] Tournament ID: {tournamentId}", "WEBSOCKET");
        _debugLog($"📋 [WS-CONNECTION] Planner Info: {System.Text.Json.JsonSerializer.Serialize(plannerInfo)}", "WEBSOCKET");
        
        var data = new { tournamentId = tournamentId, plannerInfo = plannerInfo };
        var result = await SendMessageAsync("register-planner", data);
        
        _debugLog($"📋 [WS-CONNECTION] Register result: {result}", result ? "SUCCESS" : "ERROR");
        _debugLog($"📋 [WS-CONNECTION] ================================", "WEBSOCKET");
        
        return result;
    }

    /// <summary>
    /// Schließt WebSocket-Verbindung
    /// </summary>
    public async Task CloseAsync()
    {
        try
        {
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;

            if (_webSocket != null)
            {
                // ✅ FIX: Safe cancellation with null check
                try
                {
                    _cancellationTokenSource?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    _debugLog("⚠️ [WS-CONNECTION] CancellationTokenSource already disposed", "WARNING");
                }

                if (_webSocket.State == WebSocketState.Open)
                {
                    _debugLog("🔌 [WS-CONNECTION] Closing WebSocket gracefully...", "WEBSOCKET");
                    try
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                    }
                    catch (Exception closeEx)
                    {
                        _debugLog($"⚠️ [WS-CONNECTION] Error during graceful close: {closeEx.Message}", "WARNING");
                    }
                }
                else
                {
                    _debugLog($"🔌 [WS-CONNECTION] WebSocket already in state: {_webSocket.State}", "WEBSOCKET");
                }

                try
                {
                    _webSocket.Dispose();
                }
                catch (Exception disposeEx)
                {
                    _debugLog($"⚠️ [WS-CONNECTION] Error disposing WebSocket: {disposeEx.Message}", "WARNING");
                }
                _webSocket = null;
            }

            // ✅ FIX: Safe disposal with null check
            try
            {
                _cancellationTokenSource?.Dispose();
            }
            catch (ObjectDisposedException)
            {
                _debugLog("⚠️ [WS-CONNECTION] CancellationTokenSource already disposed during disposal", "WARNING");
            }
            _cancellationTokenSource = null;

            _isConnected = false;
            _debugLog("🔔 [WS-CONNECTION] Firing ConnectionStatusChanged event: false (close)", "WEBSOCKET");
            ConnectionStatusChanged?.Invoke(false, "WebSocket Closed");
            _debugLog("🔌 [WS-CONNECTION] WebSocket connection closed", "WEBSOCKET");
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error closing WebSocket: {ex.Message}", "ERROR");
            _debugLog($"❌ [WS-CONNECTION] Stack trace: {ex.StackTrace}", "ERROR");
        }
    }

    /// <summary>
    /// ✅ NEW: Schedule an auto-reconnect attempt
    /// Can be called from anywhere to trigger a reconnect
    /// </summary>
    public void ScheduleReconnect(int delaySeconds = 5)
    {
        // Don't schedule if already disposed or if cancellation was requested
        if (_isDisposed || _cancellationTokenSource?.Token.IsCancellationRequested == true)
        {
            _debugLog($"⚠️ [WS-CONNECTION] Cannot schedule reconnect - disposed or cancelled", "WARNING");
            return;
        }

        // ✅ FIX: Prevent duplicate reconnect scheduling
        if (_reconnectScheduled)
        {
            _debugLog($"ℹ️ [WS-CONNECTION] Reconnect already scheduled, skipping duplicate", "INFO");
            return;
        }

        _reconnectScheduled = true;
        _debugLog($"🔄 [WS-CONNECTION] Scheduling auto-reconnect in {delaySeconds} seconds...", "WEBSOCKET");
        
        _ = Task.Run(async () =>
        {
            try
            {
                _debugLog($"⏰ [WS-CONNECTION] Waiting {delaySeconds} seconds before reconnect...", "WEBSOCKET");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                
                if (_isDisposed || _cancellationTokenSource?.Token.IsCancellationRequested == true)
                {
                    _debugLog($"⚠️ [WS-CONNECTION] Auto-reconnect cancelled (Token was cancelled)", "WARNING");
                    _reconnectScheduled = false;
                    return;
                }
                
                _debugLog($"🔄 [WS-CONNECTION] Starting auto-reconnect now...", "WEBSOCKET");
                _reconnectScheduled = false; // Reset flag before attempting reconnect
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                _debugLog($"❌ [WS-CONNECTION] Error in auto-reconnect: {ex.Message}", "ERROR");
                _reconnectScheduled = false; // Reset flag on error
            }
        });
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _heartbeatTimer?.Stop();
            _heartbeatTimer?.Dispose();
            _cancellationTokenSource?.Cancel();
            _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None);
            _webSocket?.Dispose();
            _cancellationTokenSource?.Dispose();
            _isDisposed = true;
        }
    }
}