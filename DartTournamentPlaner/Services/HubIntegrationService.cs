using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Reflection;
using System.Text.Json;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Services;

/// <summary>
/// ✅ NEW: Hub Connection State Enum
/// Unterscheidet zwischen verschiedenen Verbindungszuständen
/// </summary>
public enum HubConnectionState
{
    /// <summary>Keine Verbindung zum Hub</summary>
    Disconnected,
    
    /// <summary>WebSocket verbunden, aber kein Tournament registriert</summary>
    WebSocketReady,
    
    /// <summary>WebSocket verbunden UND Tournament registriert</summary>
    TournamentRegistered,
    
    /// <summary>Verbindung wird aufgebaut</summary>
    Connecting,
    
    /// <summary>Verbindungsfehler</summary>
    Error
}

/// <summary>
/// Service für die Tournament Hub Integration und WebSocket-Kommunikation
/// Verwaltet die bidirektionale Kommunikation mit dem Tournament Hub
/// </summary>
public class HubIntegrationService : IDisposable
{
    private readonly ITournamentHubService _tournamentHubService;
    private readonly ConfigService _configService;
    private readonly LocalizationService _localizationService;
    private readonly IApiIntegrationService _apiService;
    private readonly Dispatcher _dispatcher;
    
    // Hub Status
    private DispatcherTimer _hubHeartbeatTimer;
    private DispatcherTimer _hubSyncTimer;
    private string? _currentTournamentId;
    private bool _isRegisteredWithHub = false;
    private bool _isWebSocketConnected = false; // ✅ NEW: Track WebSocket connection separately
    private DateTime _lastSyncTime = DateTime.MinValue;
    private bool _isSyncingWithHub = false;

    // Hub Debug Console - KORRIGIERT: Static instance für globalen Zugriff
    private static HubDebugWindow? _globalHubDebugWindow;

    // Events
    public event Action<HubMatchUpdateEventArgs>? MatchResultReceived;
    public event Action<bool>? HubStatusChanged;
    public event Action<HubConnectionState>? HubConnectionStateChanged; // ✅ NEW: Detailed state event
    public event Action? DataChanged;
    
    // ✅ NEW: Event fired when tournament needs to be re-synced after reconnect
    public event Func<Task>? TournamentNeedsResync;
    
    // ✅ NEW: PowerScoring Events
    public event EventHandler<PowerScore.PowerScoringHubMessage>? PowerScoringMessageReceived;

    public HubIntegrationService(
        ConfigService configService,
        LocalizationService localizationService,
        IApiIntegrationService apiService,
        Dispatcher dispatcher)
    {
        _configService = configService;
        _localizationService = localizationService;
        _apiService = apiService;
        _dispatcher = dispatcher;
        
        _tournamentHubService = new TournamentHubService(_configService);
        InitializeHubDebugConsole();
        InitializeTimers();
    }

    public string GetCurrentTournamentId() => _currentTournamentId ?? string.Empty;
    public bool IsRegisteredWithHub => _isRegisteredWithHub;
    public ITournamentHubService TournamentHubService => _tournamentHubService;

    // KORRIGIERT: Statische Referenz auf Debug Console für globalen Zugriff
    public static HubDebugWindow? GlobalDebugWindow => _globalHubDebugWindow;

    private void InitializeHubDebugConsole()
    {
        try
        {
            // Verwende globale Instanz wenn bereits vorhanden
            if (_globalHubDebugWindow == null)
            {
                _globalHubDebugWindow = new HubDebugWindow();
                _globalHubDebugWindow.AddDebugMessage("🎯 Hub Debug Console initialisiert", "SUCCESS");
                
                System.Diagnostics.Debug.WriteLine("✅ Global Hub Debug Console created");
            }
            else
            {
                _globalHubDebugWindow.AddDebugMessage("🔄 Hub Integration Service reconnected", "INFO");
                System.Diagnostics.Debug.WriteLine("✅ Using existing Global Hub Debug Console");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error initializing Hub Debug Console: {ex.Message}");
        }
    }

    private void InitializeTimers()
    {
        _hubHeartbeatTimer = new DispatcherTimer();
        _hubHeartbeatTimer.Interval = TimeSpan.FromMinutes(2);
        _hubHeartbeatTimer.Tick += HubHeartbeatTimer_Tick;
        
        _hubSyncTimer = new DispatcherTimer();
        _hubSyncTimer.Interval = TimeSpan.FromSeconds(30);
        _hubSyncTimer.Tick += HubSyncTimer_Tick;
    }

    public async Task InitializeAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔌 [HUB] Initializing WebSocket connection to Tournament Hub...");
            _globalHubDebugWindow?.AddDebugMessage("🔌 Initialisiere WebSocket-Verbindung zum Tournament Hub", "WEBSOCKET");
            _globalHubDebugWindow?.AddDebugMessage($"🔗 Hub URL: {_tournamentHubService.HubUrl}", "INFO");
            
            var success = await _tournamentHubService.InitializeWebSocketAsync();
            
            if (success)
            {
                _tournamentHubService.OnMatchResultReceivedFromHub += OnHubMatchResultReceived;
                _tournamentHubService.OnTournamentUpdateReceived += OnHubTournamentUpdateReceived;
                _tournamentHubService.OnConnectionStatusChanged += OnHubConnectionStatusChanged;
                
                // ✅ CRITICAL FIX: Subscribe to NEW live-update events!
                _tournamentHubService.OnMatchStarted += OnHubMatchStarted;
                _tournamentHubService.OnLegCompleted += OnHubLegCompleted;
                _tournamentHubService.OnMatchProgressUpdated += OnHubMatchProgressUpdated;
                
                // ✅ NEW: Subscribe to PowerScoring events
                _tournamentHubService.OnPowerScoringMessageReceived += OnHubPowerScoringMessageReceived;
       
                System.Diagnostics.Debug.WriteLine("✅ [HUB] WebSocket connection established");
                System.Diagnostics.Debug.WriteLine("✅ [HUB] Live-update event handlers subscribed");
                System.Diagnostics.Debug.WriteLine("✅ [HUB] PowerScoring event handlers subscribed");
                _globalHubDebugWindow?.AddDebugMessage("✅ WebSocket-Verbindung hergestellt", "SUCCESS");
                _globalHubDebugWindow?.AddDebugMessage("✅ Live-Update Events subscribed", "SUCCESS");
                _globalHubDebugWindow?.UpdateStatus("WebSocket-Verbindung erfolgreich hergestellt");
                
                // ✅ CRITICAL FIX: Update connection status in debug window immediately
                _globalHubDebugWindow?.UpdateConnectionStatus(true, "WebSocket Connected");
                
                // ✅ CRITICAL FIX: Track WebSocket connection state
                _isWebSocketConnected = true;
                
                // ✅ CRITICAL FIX: Notify about state change (WebSocket Ready)
                NotifyConnectionStateChanged();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ [HUB] WebSocket connection failed");
                _globalHubDebugWindow?.AddDebugMessage("⚠️ WebSocket-Verbindung fehlgeschlagen", "ERROR");
                _globalHubDebugWindow?.UpdateStatus("WebSocket-Verbindung fehlgeschlagen");
                
                // ✅ CRITICAL FIX: Update connection status in debug window immediately
                _globalHubDebugWindow?.UpdateConnectionStatus(false, "WebSocket Connection Failed");
                
                // ✅ CRITICAL FIX: Track WebSocket connection state
                _isWebSocketConnected = false;
                
                // ✅ CRITICAL FIX: Notify about state change (Disconnected)
                NotifyConnectionStateChanged();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [HUB] Error initializing: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler bei Initialisierung: {ex.Message}", "ERROR");
            _globalHubDebugWindow?.UpdateStatus($"WebSocket-Fehler: {ex.Message}");
            
            // ✅ CRITICAL FIX: Update connection status in debug window immediately
            _globalHubDebugWindow?.UpdateConnectionStatus(false, $"WebSocket Error: {ex.Message}");
            
            // ✅ CRITICAL FIX: Track WebSocket connection state
            _isWebSocketConnected = false;
            
            // ✅ CRITICAL FIX: Notify about state change (Disconnected)
            NotifyConnectionStateChanged();
        }
    }

    public async Task<bool> RegisterTournamentAsync(string? customTournamentId = null)
    {
        try
        {
            // ⭐ ERWEITERT: Verwende custom ID oder generiere eine neue
            if (!string.IsNullOrWhiteSpace(customTournamentId))
            {
                // Validiere und säubere die custom ID
                _currentTournamentId = SanitizeTournamentId(customTournamentId);
                System.Diagnostics.Debug.WriteLine($"🎯 Using custom Tournament ID: {_currentTournamentId}");
                _globalHubDebugWindow?.AddDebugMessage($"🎯 Verwende benutzerdefinierte Tournament-ID: {_currentTournamentId}", "TOURNAMENT");
            }
            else
            {
                // Generiere automatische ID (wie bisher)
                _currentTournamentId = $"TOURNAMENT_{DateTime.Now:yyyyMMdd_HHmmss}";
                System.Diagnostics.Debug.WriteLine($"🎯 Generated automatic Tournament ID: {_currentTournamentId}");
                _globalHubDebugWindow?.AddDebugMessage($"🎯 Automatisch generierte Tournament-ID: {_currentTournamentId}", "TOURNAMENT");
            }
    
            _globalHubDebugWindow?.AddDebugMessage($"🎯 Registriere Tournament: {_currentTournamentId}", "TOURNAMENT");
    
            var success = await _tournamentHubService.RegisterWithHubAsync(
                _currentTournamentId,
                $"Dart Turnier {DateTime.Now:dd.MM.yyyy}",
                "Live Dart Tournament von Tournament Planner"
            );
            
            if (success)
            {
                _isRegisteredWithHub = true;
                _hubHeartbeatTimer?.Start();
                _hubSyncTimer?.Start();
                
                System.Diagnostics.Debug.WriteLine($"✅ Tournament registered: {_currentTournamentId}");
                _globalHubDebugWindow?.AddDebugMessage($"✅ Tournament erfolgreich registriert: {_currentTournamentId}", "SUCCESS");
                
                // API Integration benachrichtigen
                if (_apiService is HttpApiIntegrationService httpApiService)
                {
                    httpApiService.SetCurrentTournamentId(_currentTournamentId);
                    _globalHubDebugWindow?.AddDebugMessage("🔗 API Integration benachrichtigt", "INFO");
                }
        
                await SubscribeToTournamentUpdates(_currentTournamentId);
                
                // ✅ CRITICAL FIX: Notify about state change (Tournament Registered)
                NotifyConnectionStateChanged();
                
                return true;
            }
            
            _currentTournamentId = string.Empty;
            _isRegisteredWithHub = false;
            _globalHubDebugWindow?.AddDebugMessage("❌ Tournament-Registrierung fehlgeschlagen", "ERROR");
            
            // ✅ CRITICAL FIX: Notify about state change (WebSocket Ready or Disconnected)
            NotifyConnectionStateChanged();
            
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ RegisterTournament error: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler bei Tournament-Registrierung: {ex.Message}", "ERROR");
            _currentTournamentId = string.Empty;
            _isRegisteredWithHub = false;
            
            // ✅ CRITICAL FIX: Notify about state change (WebSocket Ready or Disconnected)
            NotifyConnectionStateChanged();
            
            return false;
        }
    }

    /// <summary>
    /// ⭐ NEU: Validiert und säubert eine Tournament-ID
    /// Entfernt ungültige Zeichen und stellt sicher dass die ID Hub-kompatibel ist
    /// </summary>
    private string SanitizeTournamentId(string rawId)
    {
        // Entferne führende/nachfolgende Leerzeichen
        var sanitized = rawId.Trim();
        
        // Ersetze Leerzeichen durch Unterstriche
        sanitized = sanitized.Replace(" ", "_");
        
        // Entferne alle Zeichen außer: A-Z, a-z, 0-9, _, -
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[^a-zA-Z0-9_\-]", "");
 
        // Stelle sicher dass die ID nicht leer ist
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            // Fallback: Generiere automatische ID
            sanitized = $"TOURNAMENT_{DateTime.Now:yyyyMMdd_HHmmss}";
            System.Diagnostics.Debug.WriteLine($"⚠️ Empty custom ID provided, generated fallback: {sanitized}");
        }
        
        System.Diagnostics.Debug.WriteLine($"🔧 Sanitized Tournament ID: '{rawId}' -> '{sanitized}'");
        
        return sanitized;
    }

    /// <summary>
    /// ⭐ NEU: Generiert eine neue Tournament-ID (für UI-Button)
    /// </summary>
    public string GenerateNewTournamentId()
    {
        return $"TOURNAMENT_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    public async Task<bool> UnregisterTournamentAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentTournamentId)) return true;
            
            _globalHubDebugWindow?.AddDebugMessage($"📴 Entregistriere Tournament: {_currentTournamentId}", "TOURNAMENT");
            
            _hubHeartbeatTimer?.Stop();
            _hubSyncTimer?.Stop();
            
            await UnsubscribeFromTournamentUpdates(_currentTournamentId);
            
            _tournamentHubService.OnMatchResultReceivedFromHub -= OnHubMatchResultReceived;
            _tournamentHubService.OnTournamentUpdateReceived -= OnHubTournamentUpdateReceived;
            _tournamentHubService.OnConnectionStatusChanged -= OnHubConnectionStatusChanged;
            
            await _tournamentHubService.UnregisterFromHubAsync(_currentTournamentId);
            
            _currentTournamentId = string.Empty;
            _isRegisteredWithHub = false;
            _lastSyncTime = DateTime.MinValue;
            
            _globalHubDebugWindow?.AddDebugMessage("✅ Tournament erfolgreich entregistriert", "SUCCESS");
            
            // ✅ CRITICAL FIX: Notify about state change (WebSocket Ready if still connected)
            NotifyConnectionStateChanged();
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ UnregisterTournament error: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler beim Tournament entregistrieren: {ex.Message}", "ERROR");
            return false;
        }
    }

    public async Task<bool> SyncTournamentAsync(TournamentData tournamentData)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentTournamentId)) return false;
            
            _globalHubDebugWindow?.AddDebugMessage($"🔄 Synchronisiere Tournament-Daten...", "SYNC");
            
            var success = await _tournamentHubService.SyncTournamentWithClassesAsync(
                _currentTournamentId, 
                $"Dart Turnier {DateTime.Now:dd.MM.yyyy}",
                tournamentData
            );
            
            if (success)
            {
                _lastSyncTime = DateTime.Now;
                var totalMatches = tournamentData.TournamentClasses.Sum(tc => tc.Groups.Sum(g => g.Matches.Count));
                _globalHubDebugWindow?.AddDebugMessage($"✅ Sync erfolgreich: {totalMatches} Matches, {tournamentData.TournamentClasses.Count} Klassen", "SUCCESS");
                HubStatusChanged?.Invoke(true);
            }
            else
            {
                _globalHubDebugWindow?.AddDebugMessage("❌ Tournament-Synchronisation fehlgeschlagen", "ERROR");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ SyncTournament error: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler beim Synchronisieren: {ex.Message}", "ERROR");
            return false;
        }
    }

    public void ShowDebugConsole()
    {
        try
        {
            if (_globalHubDebugWindow?.IsVisible == true)
            {
                _globalHubDebugWindow.Hide();
                _globalHubDebugWindow.AddDebugMessage("🔍 Debug Console versteckt", "INFO");
            }
            else
            {
                _globalHubDebugWindow?.Show();
                _globalHubDebugWindow?.AddDebugMessage("🔍 Debug Console geöffnet", "INFO");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error toggling Hub Debug Console: {ex.Message}");
        }
    }

    public string GetJoinUrl()
    {
        return _tournamentHubService.GetJoinUrl(_currentTournamentId ?? "");
    }

    public void UpdateHubUrl(string newUrl)
    {
        _globalHubDebugWindow?.AddDebugMessage($"🔗 Hub-URL aktualisiert: {newUrl}", "INFO");
        
        // ✅ FIXED: Speichere die neue URL in der Config
        var sanitizedUrl = newUrl.Trim();
        _configService.Config.HubUrl = sanitizedUrl;
        
        // Speichere Config asynchron
        Task.Run(async () => await _configService.SaveConfigAsync());
        
        // Aktualisiere die URL im TournamentHubService
        _tournamentHubService.HubUrl = sanitizedUrl;
        
        System.Diagnostics.Debug.WriteLine($"✅ [HUB-INTEGRATION] Hub-URL updated and saved to config: {sanitizedUrl}");
    }

    public DateTime LastSyncTime => _lastSyncTime;
    public bool IsSyncing => _isSyncingWithHub;

    // ✅ NEW: Helper method to notify about connection state changes
    private void NotifyConnectionStateChanged()
    {
        var currentState = GetCurrentConnectionState();
        
        System.Diagnostics.Debug.WriteLine($"🔔 [HUB-STATE] Connection state: {currentState}");
        System.Diagnostics.Debug.WriteLine($"   WebSocket Connected: {_isWebSocketConnected}");
        System.Diagnostics.Debug.WriteLine($"   Tournament Registered: {_isRegisteredWithHub}");
        System.Diagnostics.Debug.WriteLine($"   Tournament ID: {_currentTournamentId ?? "null"}");
        
        // Fire detailed state event
        HubConnectionStateChanged?.Invoke(currentState);
        
        // Fire legacy bool event for backwards compatibility
        HubStatusChanged?.Invoke(_isRegisteredWithHub);
    }
    
    // ✅ NEW: Calculate current connection state
    private HubConnectionState GetCurrentConnectionState()
    {
        if (!_isWebSocketConnected)
        {
            return HubConnectionState.Disconnected;
        }
        
        if (_isRegisteredWithHub && !string.IsNullOrEmpty(_currentTournamentId))
        {
            return HubConnectionState.TournamentRegistered;
        }
        
        return HubConnectionState.WebSocketReady;
    }
    
    // ✅ NEW: Public property to expose connection state
    public HubConnectionState CurrentConnectionState => GetCurrentConnectionState();
    public bool IsWebSocketConnected => _isWebSocketConnected;

    // Private Event Handlers

    private void OnHubMatchResultReceived(HubMatchUpdateEventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"📥 [HUB] Match result received: {e.MatchId}");
            
            var isMatchResult = e.Source?.Contains("match-result") == true;
            var category = isMatchResult ? "MATCH_RESULT" : "MATCH";
            
            _globalHubDebugWindow?.AddDebugMessage($"📥 Match Update empfangen: Match {e.MatchId} in Klasse {e.ClassId}", category);
            _globalHubDebugWindow?.AddDebugMessage($"📊 Ergebnis: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs", category);
            _globalHubDebugWindow?.AddDebugMessage($"🔍 Status: {e.Status}, Quelle: {e.Source}", category);
            _globalHubDebugWindow?.AddDebugMessage($"📋 Group Info: '{e.GroupName}' (ID: {e.GroupId})", category);
            
            MatchResultReceived?.Invoke(e);
        });
    }

    // ✅ NEW: Handler für Match-Started Events
    private void OnHubMatchStarted(HubMatchUpdateEventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"🎬 [HUB-INTEGRATION] Match-Started Event empfangen: {e.MatchId}");
            _globalHubDebugWindow?.AddDebugMessage($"🎬 Match gestartet: Match {e.MatchId} in Klasse {e.ClassId}", "MATCH");
            _globalHubDebugWindow?.AddDebugMessage($"📊 Status: {e.Status}, Source: {e.Source}", "MATCH");
        });
    }

    // ✅ NEW: Handler für Leg-Completed Events
    private void OnHubLegCompleted(HubMatchUpdateEventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"🎯 [HUB-INTEGRATION] Leg-Completed Event empfangen: {e.MatchId} - Leg {e.CurrentLeg}/{e.TotalLegs}");
            _globalHubDebugWindow?.AddDebugMessage($"🎯 Leg abgeschlossen: Match {e.MatchId}, Leg {e.CurrentLeg}/{e.TotalLegs}", "MATCH");
            _globalHubDebugWindow?.AddDebugMessage($"📊 Score: {e.Player1Legs}-{e.Player2Legs}", "MATCH");
        });
    }

    // ✅ NEW: Handler für Match-Progress Events
    private void OnHubMatchProgressUpdated(HubMatchUpdateEventArgs e)
    {
        _dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"📈 [HUB-INTEGRATION] Match-Progress Event empfangen: {e.MatchId}");
            _globalHubDebugWindow?.AddDebugMessage($"📈 Match-Progress: Match {e.MatchId}", "MATCH");
        });
    }

    private void OnHubTournamentUpdateReceived(string tournamentId, object updateData)
    {
        _dispatcher.Invoke(() =>
        {
            if (tournamentId == _currentTournamentId)
            {
                _globalHubDebugWindow?.AddDebugMessage($"📥 Tournament Update empfangen für: {tournamentId}", "TOURNAMENT");
                _lastSyncTime = DateTime.Now;
                HubStatusChanged?.Invoke(true);
            }
        });
    }

    private void OnHubConnectionStatusChanged(bool isConnected, string status)
    {
        _dispatcher.Invoke(() =>
        {
            // ✅ CRITICAL FIX: Track WebSocket connection state separately
            var wasWebSocketConnected = _isWebSocketConnected;
            _isWebSocketConnected = isConnected;
            
            _globalHubDebugWindow?.AddDebugMessage($"🔌 Hub-Verbindungsstatus geändert: {isConnected} - {status}", "WEBSOCKET");
            _globalHubDebugWindow?.UpdateConnectionStatus(isConnected, status);
            
            System.Diagnostics.Debug.WriteLine($"🔔 [HUB-CONNECTION] WebSocket status changed:");
            System.Diagnostics.Debug.WriteLine($"   Was Connected: {wasWebSocketConnected}");
            System.Diagnostics.Debug.WriteLine($"   Now Connected: {_isWebSocketConnected}");
            System.Diagnostics.Debug.WriteLine($"   Tournament Registered: {_isRegisteredWithHub}");
            System.Diagnostics.Debug.WriteLine($"   Tournament ID: {_currentTournamentId ?? "null"}");
            
            // ✅ FIX: Notify about state change (this will calculate correct state based on WebSocket + Registration)
            NotifyConnectionStateChanged();
            
            // ✅ CRITICAL FIX: Auto-Reconnect, Re-Register und Re-Subscribe
            if (isConnected && !wasWebSocketConnected)
            {
                _globalHubDebugWindow?.AddDebugMessage($"🔄 WebSocket wiederhergestellt!", "SUCCESS");
                System.Diagnostics.Debug.WriteLine($"🔄 [HUB-CONNECTION] WebSocket reconnected!");
                
                // ✅ FIX: Wenn Tournament registriert war, RE-REGISTER und RE-SUBSCRIBE
                if (_isRegisteredWithHub && !string.IsNullOrEmpty(_currentTournamentId))
                {
                    _globalHubDebugWindow?.AddDebugMessage($"🔄 Starting full tournament re-registration after reconnect...", "SYNC");
                    System.Diagnostics.Debug.WriteLine($"🔄 [HUB-CONNECTION] Starting full tournament re-registration: {_currentTournamentId}");
                    
                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(2000); // Warte 2 Sekunden damit WebSocket stabil ist
                            
                            if (!_isWebSocketConnected)
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ [HUB-CONNECTION] WebSocket disconnected again, aborting re-registration");
                                return;
                            }
                            
                            var savedTournamentId = _currentTournamentId;
                            
                            if (string.IsNullOrEmpty(savedTournamentId))
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ [HUB-CONNECTION] Tournament ID lost, cannot re-register");
                                return;
                            }
                            
                            // ✅ STEP 1: Re-register tournament with Hub HTTP API
                            System.Diagnostics.Debug.WriteLine($"🔄 [HUB-CONNECTION] Step 1: Re-registering tournament via HTTP API...");
                            _globalHubDebugWindow?.AddDebugMessage($"🔄 Step 1: Re-registering tournament {savedTournamentId} via HTTP API", "TOURNAMENT");
                            
                            var reRegisterSuccess = await _tournamentHubService.RegisterWithHubAsync(
                                savedTournamentId,
                                $"Dart Turnier {DateTime.Now:dd.MM.yyyy}",
                                "Re-registered after reconnect"
                            );
                            
                            if (!reRegisterSuccess)
                            {
                                System.Diagnostics.Debug.WriteLine($"❌ [HUB-CONNECTION] HTTP re-registration failed!");
                                _globalHubDebugWindow?.AddDebugMessage($"❌ HTTP re-registration failed", "ERROR");
                                return;
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"✅ [HUB-CONNECTION] HTTP re-registration successful");
                            _globalHubDebugWindow?.AddDebugMessage($"✅ HTTP re-registration successful", "SUCCESS");
                            
                            // ✅ STEP 2: Re-subscribe via WebSocket
                            System.Diagnostics.Debug.WriteLine($"🔄 [HUB-CONNECTION] Step 2: Re-subscribing via WebSocket...");
                            _globalHubDebugWindow?.AddDebugMessage($"🔄 Step 2: Re-subscribing via WebSocket", "WEBSOCKET");
                            
                            await SubscribeToTournamentUpdates(savedTournamentId);
                            
                            // ✅ STEP 3: Sync tournament data (fire event for MainWindow to handle)
                            System.Diagnostics.Debug.WriteLine($"🔄 [HUB-CONNECTION] Step 3: Syncing tournament data...");
                            _globalHubDebugWindow?.AddDebugMessage($"🔄 Step 3: Syncing tournament data", "SYNC");
                            
                            // Fire the resync event and wait for it to complete
                            if (TournamentNeedsResync != null)
                            {
                                try
                                {
                                    await TournamentNeedsResync.Invoke();
                                    System.Diagnostics.Debug.WriteLine($"✅ [HUB-CONNECTION] Tournament data sync completed");
                                    _globalHubDebugWindow?.AddDebugMessage($"✅ Tournament data sync completed", "SUCCESS");
                                }
                                catch (Exception syncEx)
                                {
                                    System.Diagnostics.Debug.WriteLine($"⚠️ [HUB-CONNECTION] Tournament data sync failed: {syncEx.Message}");
                                    _globalHubDebugWindow?.AddDebugMessage($"⚠️ Tournament data sync failed: {syncEx.Message}", "WARNING");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"⚠️ [HUB-CONNECTION] No TournamentNeedsResync handler registered");
                                _globalHubDebugWindow?.AddDebugMessage($"⚠️ No sync handler registered", "WARNING");
                            }
                            
                            // ✅ STEP 4: Update UI
                            _dispatcher.Invoke(() =>
                            {
                                _globalHubDebugWindow?.AddDebugMessage($"✅ Full re-registration complete! Tournament fully reconnected", "SUCCESS");
                                System.Diagnostics.Debug.WriteLine($"✅ [HUB-CONNECTION] Full re-registration complete!");
                                
                                // Restart timers
                                _hubHeartbeatTimer?.Start();
                                _hubSyncTimer?.Start();
                                
                                NotifyConnectionStateChanged();
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ [HUB-CONNECTION] Error in re-registration: {ex.Message}");
                            _globalHubDebugWindow?.AddDebugMessage($"❌ Error in re-registration: {ex.Message}", "ERROR");
                        }
                    });
                }
                else
                {
                    _globalHubDebugWindow?.AddDebugMessage($"ℹ️ WebSocket verbunden, aber kein Tournament registriert", "INFO");
                    System.Diagnostics.Debug.WriteLine($"ℹ️ [HUB-CONNECTION] WebSocket ready but no tournament registered");
                }
            }
            else if (!isConnected && wasWebSocketConnected)
            {
                // ✅ FIX: Bei Disconnect, stoppe Timer aber behalte Registration-Status
                _hubHeartbeatTimer?.Stop();
                _hubSyncTimer?.Stop();
                
                _globalHubDebugWindow?.AddDebugMessage($"⚠️ WebSocket-Verbindung verloren! Tournament bleibt registriert für Auto-Reconnect", "WARNING");
                System.Diagnostics.Debug.WriteLine($"⚠️ [HUB-CONNECTION] WebSocket connection lost!");
                
                if (_isRegisteredWithHub)
                {
                    _globalHubDebugWindow?.AddDebugMessage($"ℹ️ Tournament-ID {_currentTournamentId} bleibt für Auto-Reconnect gespeichert", "INFO");
                    System.Diagnostics.Debug.WriteLine($"ℹ️ [HUB-CONNECTION] Tournament ID preserved for auto-reconnect: {_currentTournamentId}");
                }
                
                // ✅ REMOVED: Don't trigger reconnect here, WebSocketConnectionManager handles it
                // The reconnect is already scheduled in ListenForMessages finally block
                // and in InitializeAsync after failed connection attempts
                _globalHubDebugWindow?.AddDebugMessage($"ℹ️ Automatic reconnect is handled by WebSocketConnectionManager", "INFO");
                System.Diagnostics.Debug.WriteLine($"ℹ️ [HUB-CONNECTION] WebSocketConnectionManager will handle reconnect");
            }
        });
    }

    private async void HubHeartbeatTimer_Tick(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentTournamentId))
        {
            try
            {
                await _tournamentHubService.SendHeartbeatAsync(_currentTournamentId, 0, 0);
                _globalHubDebugWindow?.AddDebugMessage($"💓 Heartbeat gesendet für Tournament: {_currentTournamentId}", "INFO");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Heartbeat error: {ex.Message}");
                _globalHubDebugWindow?.AddDebugMessage($"❌ Heartbeat-Fehler: {ex.Message}", "ERROR");
            }
        }
    }

    private async void HubSyncTimer_Tick(object? sender, EventArgs e)
    {
        if (_isRegisteredWithHub && !_isSyncingWithHub)
        {
            // Auto-sync würde hier ausgeführt, aber wir brauchen die Tournament-Daten vom MainWindow
            _globalHubDebugWindow?.AddDebugMessage("🔄 Auto-Sync Timer ausgelöst", "SYNC");
        }
    }

    private async Task SubscribeToTournamentUpdates(string tournamentId)
    {
        try
        {
            _globalHubDebugWindow?.AddDebugMessage("===== SUBSCRIBING TOURNAMENT =====", "TOURNAMENT");
            _globalHubDebugWindow?.AddDebugMessage($"Tournament ID: {tournamentId}", "TOURNAMENT");
            
            // ✅ ERWEITERT: Überprüfe WebSocket-Status vor Subscribe
            var isConnected = _tournamentHubService.IsWebSocketConnected;
            
            _globalHubDebugWindow?.AddDebugMessage($"🔍 WebSocket Connected: {isConnected}", "INFO");
            _globalHubDebugWindow?.AddDebugMessage($"🔍 Hub URL: {_tournamentHubService.HubUrl}", "INFO");
            
            if (!isConnected)
            {
                _globalHubDebugWindow?.AddDebugMessage("⚠️ WARNING: WebSocket not connected before subscribe!", "WARNING");
                _globalHubDebugWindow?.AddDebugMessage("⚠️ Attempting to initialize WebSocket first...", "WARNING");
                
                var initSuccess = await _tournamentHubService.InitializeWebSocketAsync();
                if (!initSuccess)
                {
                    _globalHubDebugWindow?.AddDebugMessage("❌ Failed to initialize WebSocket before subscribe", "ERROR");
                    return;
                }
                
                _globalHubDebugWindow?.AddDebugMessage("✅ WebSocket initialized successfully", "SUCCESS");
                await Task.Delay(1000); // Warte auf Verbindung
            }
            
            var success = await _tournamentHubService.SubscribeToTournamentAsync(tournamentId);
            
            if (success)
            {
                _globalHubDebugWindow?.AddDebugMessage($"✅ Subscribe call successful for: {tournamentId}", "SUCCESS");
                
                // ✅ NEU: Warte länger auf Bestätigung
                _globalHubDebugWindow?.AddDebugMessage("⏳ Waiting 2 seconds for subscription confirmation...", "INFO");
                await Task.Delay(2000);
                
                var plannerInfo = new
                {
                    ClientType = "Tournament Planner",
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    ClientId = Environment.MachineName,
                    ConnectedAt = DateTime.Now,
                    TournamentId = tournamentId
                };
                
                _globalHubDebugWindow?.AddDebugMessage($"🏁 Registering as Tournament Planner Client", "TOURNAMENT");
                _globalHubDebugWindow?.AddDebugMessage($"🏁 Planner Info: {System.Text.Json.JsonSerializer.Serialize(plannerInfo)}", "INFO");
                
                var registerSuccess = await _tournamentHubService.RegisterAsPlannerAsync(tournamentId, plannerInfo);
                
                if (registerSuccess)
                {
                    _globalHubDebugWindow?.AddDebugMessage($"✅ Planner registration call successful", "SUCCESS");
                    _lastSyncTime = DateTime.Now;
                }
                else
                {
                    _globalHubDebugWindow?.AddDebugMessage($"❌ Planner registration call failed", "ERROR");
                }
                
                // ✅ NEU: Warte auf Registrierungs-Bestätigung
                _globalHubDebugWindow?.AddDebugMessage("⏳ Waiting 1 second for planner registration confirmation...", "INFO");
                await Task.Delay(1000);
                
                _globalHubDebugWindow?.AddDebugMessage("===== SUBSCRIPTION COMPLETE =====", "TOURNAMENT");
            }
            else
            {
                _globalHubDebugWindow?.AddDebugMessage($"❌ Subscribe call failed for: {tournamentId}", "ERROR");
                _globalHubDebugWindow?.AddDebugMessage($"❌ Possible reasons:", "ERROR");
                _globalHubDebugWindow?.AddDebugMessage($"   - WebSocket not connected", "ERROR");
                _globalHubDebugWindow?.AddDebugMessage($"   - Tournament ID invalid: '{tournamentId}'", "ERROR");
                _globalHubDebugWindow?.AddDebugMessage($"   - Hub server not responding", "ERROR");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Subscribe error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Exception during subscribe: {ex.Message}", "ERROR");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Stack trace: {ex.StackTrace}", "ERROR");
        }
    }

    private async Task UnsubscribeFromTournamentUpdates(string tournamentId)
    {
        try
        {
            _globalHubDebugWindow?.AddDebugMessage($"📴 Unsubscribe von Tournament Updates: {tournamentId}", "TOURNAMENT");
            await _tournamentHubService.UnsubscribeFromTournamentAsync(tournamentId);
            _globalHubDebugWindow?.AddDebugMessage($"✅ Erfolgreich unsubscribed: {tournamentId}", "SUCCESS");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Unsubscribe error: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler beim Unsubscribe: {ex.Message}", "ERROR");
        }
    }

    /// <summary>
    /// ✅ NEW: Handler für PowerScoring Messages vom Hub
    /// </summary>
    private void OnHubPowerScoringMessageReceived(object? sender, PowerScore.PowerScoringHubMessage e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📊 [POWERSCORING] Message received: Type={e.Type}, Player={e.PlayerName}, Score={e.TotalScore}");
            _globalHubDebugWindow?.AddDebugMessage(
                $"📊 PowerScoring {e.Type}: {e.PlayerName} - Total={e.TotalScore}, Avg={e.Average:F2}", 
                "POWERSCORING");
            
            // Leite Event weiter an UI/Service
            _dispatcher.Invoke(() =>
            {
                PowerScoringMessageReceived?.Invoke(this, e);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [POWERSCORING] Error handling message: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ PowerScoring Error: {ex.Message}", "ERROR");
        }
    }

    public void Dispose()
    {
        _hubHeartbeatTimer?.Stop();
        _hubSyncTimer?.Stop();
        
        // Debug Console nicht schließen, da sie global verwendet wird
        _globalHubDebugWindow?.AddDebugMessage("🔄 Hub Integration Service disposed", "INFO");
    }
}