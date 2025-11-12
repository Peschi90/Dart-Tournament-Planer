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
    private DateTime _lastSyncTime = DateTime.MinValue;
    private bool _isSyncingWithHub = false;

    // Hub Debug Console - KORRIGIERT: Static instance für globalen Zugriff
    private static HubDebugWindow? _globalHubDebugWindow;

    // Events
    public event Action<HubMatchUpdateEventArgs>? MatchResultReceived;
    public event Action<bool>? HubStatusChanged;
    public event Action? DataChanged;

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
       
        System.Diagnostics.Debug.WriteLine("✅ [HUB] WebSocket connection established");
          System.Diagnostics.Debug.WriteLine("✅ [HUB] Live-update event handlers subscribed");
     _globalHubDebugWindow?.AddDebugMessage("✅ WebSocket-Verbindung hergestellt", "SUCCESS");
                _globalHubDebugWindow?.AddDebugMessage("✅ Live-Update Events subscribed", "SUCCESS");
    _globalHubDebugWindow?.UpdateStatus("WebSocket-Verbindung erfolgreich hergestellt");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ [HUB] WebSocket connection failed");
                _globalHubDebugWindow?.AddDebugMessage("⚠️ WebSocket-Verbindung fehlgeschlagen", "ERROR");
                _globalHubDebugWindow?.UpdateStatus("WebSocket-Verbindung fehlgeschlagen");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [HUB] Error initializing: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler bei Initialisierung: {ex.Message}", "ERROR");
            _globalHubDebugWindow?.UpdateStatus($"WebSocket-Fehler: {ex.Message}");
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
                HubStatusChanged?.Invoke(true);
                
                return true;
            }
            
            _currentTournamentId = string.Empty;
            _isRegisteredWithHub = false;
            _globalHubDebugWindow?.AddDebugMessage("❌ Tournament-Registrierung fehlgeschlagen", "ERROR");
            HubStatusChanged?.Invoke(false);
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ RegisterTournament error: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler bei Tournament-Registrierung: {ex.Message}", "ERROR");
            _currentTournamentId = string.Empty;
            _isRegisteredWithHub = false;
            HubStatusChanged?.Invoke(false);
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
 
        // Stelle sicher dass die ID nicht leer ist und ein Präfix hat
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            // Fallback: Generiere automatische ID
            sanitized = $"TOURNAMENT_{DateTime.Now:yyyyMMdd_HHmmss}";
            System.Diagnostics.Debug.WriteLine($"⚠️ Empty custom ID provided, generated fallback: {sanitized}");
        }
        else if (!sanitized.StartsWith("TOURNAMENT_", StringComparison.OrdinalIgnoreCase))
        {
            // Füge Präfix hinzu wenn nicht vorhanden
            sanitized = $"TOURNAMENT_{sanitized}";
            System.Diagnostics.Debug.WriteLine($"🔧 Added TOURNAMENT_ prefix: {sanitized}");
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
            HubStatusChanged?.Invoke(false);
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
        _tournamentHubService.HubUrl = newUrl.Trim();
    }

    public DateTime LastSyncTime => _lastSyncTime;
    public bool IsSyncing => _isSyncingWithHub;

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
            var wasConnected = _isRegisteredWithHub;
            _isRegisteredWithHub = isConnected;
            
            _globalHubDebugWindow?.AddDebugMessage($"🔌 Hub-Verbindungsstatus geändert: {isConnected} - {status}", "WEBSOCKET");
            _globalHubDebugWindow?.UpdateConnectionStatus(isConnected, status);
            
            HubStatusChanged?.Invoke(isConnected);
            
            if (isConnected && !wasConnected && !string.IsNullOrEmpty(_currentTournamentId))
            {
                _globalHubDebugWindow?.AddDebugMessage($"🔄 Re-Subscribe nach Reconnect für Tournament: {_currentTournamentId}", "SYNC");
                
                Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    if (_isRegisteredWithHub && !string.IsNullOrEmpty(_currentTournamentId))
                    {
                        await SubscribeToTournamentUpdates(_currentTournamentId);
                    }
                });
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
            
            var success = await _tournamentHubService.SubscribeToTournamentAsync(tournamentId);
            
            if (success)
            {
                _globalHubDebugWindow?.AddDebugMessage($"✅ Erfolgreich zu Tournament Updates subscribed: {tournamentId}", "SUCCESS");
                
                await Task.Delay(1000);
                
                var plannerInfo = new
                {
                    ClientType = "Tournament Planner",
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    ClientId = Environment.MachineName,
                    ConnectedAt = DateTime.Now,
                    TournamentId = tournamentId
                };
                
                _globalHubDebugWindow?.AddDebugMessage($"🏁 Registriere als Tournament Planner Client", "TOURNAMENT");
                
                await _tournamentHubService.RegisterAsPlannerAsync(tournamentId, plannerInfo);
                _lastSyncTime = DateTime.Now;
                
                _globalHubDebugWindow?.AddDebugMessage("===== SUBSCRIPTION COMPLETE =====", "TOURNAMENT");
            }
            else
            {
                _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler beim Subscriben zu Tournament Updates: {tournamentId}", "ERROR");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Subscribe error: {ex.Message}");
            _globalHubDebugWindow?.AddDebugMessage($"❌ Fehler beim Subscriben: {ex.Message}", "ERROR");
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

    public void Dispose()
    {
        _hubHeartbeatTimer?.Stop();
        _hubSyncTimer?.Stop();
        
        // Debug Console nicht schließen, da sie global verwendet wird
        _globalHubDebugWindow?.AddDebugMessage("🔄 Hub Integration Service disposed", "INFO");
    }
}