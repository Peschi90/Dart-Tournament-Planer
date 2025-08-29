using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            _debugLog("🔌 [WS-CONNECTION] Initializing WebSocket connection...", "WEBSOCKET");

            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                _debugLog("🔌 [WS-CONNECTION] WebSocket already connected, skipping initialization", "WEBSOCKET");
                return true;
            }

            // Dispose existing WebSocket if any
            await CloseAsync();

            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            // Configure WebSocket
            _webSocket.Options.SetRequestHeader("User-Agent", "DartTournamentPlaner-WebSocket/1.0");

            // SSL WebSocket-Endpunkte für Tournament Hub
            string[] possibleEndpoints = {
                "wss://dtp.i3ull3t.de:9444/ws",     // SSL WebSocket (bevorzugt)
                "ws://dtp.i3ull3t.de:9445/ws",      // HTTP WebSocket (fallback)
                $"{_hubUrl.Replace("https://", "wss://").Replace("http://", "ws://")}/socket.io/?EIO=4&transport=websocket"
            };

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

                    await _webSocket.ConnectAsync(uri, timeoutCts.Token);

                    if (_webSocket.State == WebSocketState.Open)
                    {
                        _debugLog($"✅ [WS-CONNECTION] Connected successfully to: {endpoint}", "SUCCESS");
                        _connectedEndpoint = endpoint;
                        connected = true;
                        break;
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
                ConnectionStatusChanged?.Invoke(false, $"Connection failed: {lastException?.Message}");
                return false;
            }

            _isConnected = true;
            ConnectionStatusChanged?.Invoke(true, $"WebSocket Connected ({_connectedEndpoint})");

            // Start listening for messages and heartbeat
            _ = Task.Run(ListenForMessages);
            StartHeartbeat();

            _debugLog("✅ [WS-CONNECTION] WebSocket connection established successfully", "SUCCESS");
            return true;
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error initializing WebSocket: {ex.Message}", "ERROR");
            _isConnected = false;
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
            ConnectionStatusChanged?.Invoke(false, "WebSocket Disconnected");

            // Automatischer Reconnect nach 5 Sekunden
            if (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    if (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await InitializeAsync();
                    }
                });
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
            if (_webSocket?.State != WebSocketState.Open) return false;

            var message = new
            {
                type = messageType,
                data = data,
                timestamp = DateTime.Now.ToString("o")
            };

            var messageJson = System.Text.Json.JsonSerializer.Serialize(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageJson);

            await _webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);

            return true;
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error sending WebSocket message: {ex.Message}", "ERROR");
            return false;
        }
    }

    /// <summary>
    /// Abonniert Tournament-Updates
    /// </summary>
    public async Task<bool> SubscribeToTournamentAsync(string tournamentId)
    {
        _debugLog($"📡 [WS-CONNECTION] Subscribing to tournament: {tournamentId}", "WEBSOCKET");
        return await SendMessageAsync("subscribe-tournament", tournamentId);
    }

    /// <summary>
    /// Deabonniert Tournament-Updates
    /// </summary>
    public async Task<bool> UnsubscribeFromTournamentAsync(string tournamentId)
    {
        _debugLog($"📡 [WS-CONNECTION] Unsubscribing from tournament: {tournamentId}", "WEBSOCKET");
        return await SendMessageAsync("unsubscribe-tournament", tournamentId);
    }

    /// <summary>
    /// Registriert als Planner
    /// </summary>
    public async Task<bool> RegisterAsPlannerAsync(string tournamentId, object plannerInfo)
    {
        _debugLog($"📋 [WS-CONNECTION] Registering as planner for: {tournamentId}", "WEBSOCKET");
        var data = new { tournamentId = tournamentId, plannerInfo = plannerInfo };
        return await SendMessageAsync("register-planner", data);
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
                _cancellationTokenSource?.Cancel();

                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                }

                _webSocket.Dispose();
                _webSocket = null;
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _isConnected = false;
            ConnectionStatusChanged?.Invoke(false, "WebSocket Closed");
            _debugLog("🔌 [WS-CONNECTION] WebSocket connection closed", "WEBSOCKET");
        }
        catch (Exception ex)
        {
            _debugLog($"❌ [WS-CONNECTION] Error closing WebSocket: {ex.Message}", "ERROR");
        }
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