using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using DartTournamentPlaner.Helpers;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;

namespace DartTournamentPlaner;

/// <summary>
/// Code-Behind-Klasse für das Hauptfenster der Dart Tournament Planer Anwendung
/// Verwaltet die vier Turnierklassen (Platin, Gold, Silber, Bronze) und koordiniert
/// alle Services wie Konfiguration, Lokalisierung und Datenverwaltung
/// </summary>
public partial class MainWindow : Window
{
    // Service-Instanzen für die gesamte Anwendung
    private readonly ConfigService _configService;
    private readonly LocalizationService _localizationService;
    private readonly DataService _dataService;
    private readonly IApiIntegrationService _apiService;
    
    // Tournament Hub Integration
    private ITournamentHubService _tournamentHubService;
    private System.Windows.Threading.DispatcherTimer _hubHeartbeatTimer;
    private System.Windows.Threading.DispatcherTimer _hubSyncTimer;
    private string _currentTournamentId;
    private bool _isRegisteredWithHub = false;
    private DateTime _lastSyncTime = DateTime.MinValue;
    private bool _isSyncingWithHub = false;

    // Hub Debug Console - NEU
    private HubDebugWindow _hubDebugWindow;

    // Auto-Save System
    private readonly DispatcherTimer _autoSaveTimer = new DispatcherTimer();
    private bool _hasUnsavedChanges = false;

    // Tracking-Set um doppelte Event-Handler-Registrierungen zu vermeiden
    private readonly HashSet<TournamentClass> _subscribedTournaments = new HashSet<TournamentClass>();

    /// <summary>
    /// Konstruktor des Hauptfensters
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        
        // Services aus App.xaml.cs holen
        _configService = App.ConfigService ?? throw new InvalidOperationException("ConfigService not initialized");
        _localizationService = App.LocalizationService ?? throw new InvalidOperationException("LocalizationService not initialized");
        _dataService = App.DataService ?? throw new InvalidOperationException("DataService not initialized");
        _apiService = App.ApiIntegrationService ?? throw new InvalidOperationException("ApiIntegrationService not initialized");

        // Tournament Hub Service initialisieren
        _tournamentHubService = new TournamentHubService(_configService);
        
        // Hub Debug Console initialisieren
        InitializeHubDebugConsole();
        
        // Initialisierung in logischer Reihenfolge
        InitializeTournamentClasses();
        InitializeServices();
        InitializeAutoSave();
        InitializeApiService();
        InitializeTournamentHub();
        
        UpdateTranslations();
        LoadData();
    }

    /// <summary>
    /// Initialisiert die vier Turnierklassen
    /// </summary>
    private void InitializeTournamentClasses()
    {
        PlatinTab.TournamentClass = new TournamentClass { Id = 1, Name = "Platin" };
        GoldTab.TournamentClass = new TournamentClass { Id = 2, Name = "Gold" };
        SilberTab.TournamentClass = new TournamentClass { Id = 3, Name = "Silber" };
        BronzeTab.TournamentClass = new TournamentClass { Id = 4, Name = "Bronze" };

        PlatinTab.TournamentClass.EnsureGroupPhaseExists();
        GoldTab.TournamentClass.EnsureGroupPhaseExists();
        SilberTab.TournamentClass.EnsureGroupPhaseExists();
        BronzeTab.TournamentClass.EnsureGroupPhaseExists();

        SubscribeToChanges(PlatinTab.TournamentClass);
        SubscribeToChanges(GoldTab.TournamentClass);
        SubscribeToChanges(SilberTab.TournamentClass);
        SubscribeToChanges(BronzeTab.TournamentClass);

        PlatinTab.DataChanged += (s, e) => MarkAsChanged();
        GoldTab.DataChanged += (s, e) => MarkAsChanged();
        SilberTab.DataChanged += (s, e) => MarkAsChanged();
        BronzeTab.DataChanged += (s, e) => MarkAsChanged();
    } 

    /// <summary>
    /// Abonniert alle relevanten Events einer TournamentClass für automatisches Speichern
    /// </summary>
    private void SubscribeToChanges(TournamentClass tournamentClass)
    {
        if (_subscribedTournaments.Contains(tournamentClass))
        {
            return;
        }
        
        try
        {
            tournamentClass.Groups.CollectionChanged += (s, e) => MarkAsChanged();
            tournamentClass.DataChangedEvent += (s, e) => MarkAsChanged();
            tournamentClass.GameRules.PropertyChanged += (s, e) => MarkAsChanged();
            
            _subscribedTournaments.Add(tournamentClass);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: ERROR for {tournamentClass.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialisiert Service-Events und Callbacks
    /// </summary>
    private void InitializeServices()
    {
        _localizationService.PropertyChanged += (s, e) => UpdateTranslations();
        
        _configService.LanguageChanged += (s, language) => 
        {
            _localizationService.SetLanguage(language);
            
            Dispatcher.BeginInvoke(() =>
            {
                UpdateLanguageStatus();
                UpdateTranslations();
                ForceUIUpdate();
            }, System.Windows.Threading.DispatcherPriority.Render);
        };
    }

    /// <summary>
    /// Erzwingt ein sofortiges UI-Update für alle Komponenten
    /// </summary>
    private void ForceUIUpdate()
    {
        try
        {
            UpdateTranslations();
            UpdateLanguageStatus();
            UpdateStatusBar();
            
            PlatinTab?.UpdateTranslations();
            GoldTab?.UpdateTranslations();
            SilberTab?.UpdateTranslations();
            BronzeTab?.UpdateTranslations();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow: ForceUIUpdate ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialisiert das automatische Speichersystem
    /// </summary>
    private void InitializeAutoSave()
    {
        _autoSaveTimer.Tick += AutoSave_Tick;
        UpdateAutoSaveTimer();
        
        _configService.Config.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AppConfig.AutoSave) || e.PropertyName == nameof(AppConfig.AutoSaveInterval))
            {
                UpdateAutoSaveTimer();
            }
        };
    }

    /// <summary>
    /// Initialisiert den API-Service
    /// </summary>
    private void InitializeApiService()
    {
        try
        {
            _apiService.MatchResultUpdated += OnApiMatchResultUpdated;
            UpdateApiStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing API service: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialisiert das Tournament Hub System
    /// </summary>
    private void InitializeTournamentHub()
    {
        try
        {
            _hubHeartbeatTimer = new DispatcherTimer();
            _hubHeartbeatTimer.Interval = TimeSpan.FromMinutes(2);
            _hubHeartbeatTimer.Tick += HubHeartbeatTimer_Tick;
            
            _hubSyncTimer = new DispatcherTimer();
            _hubSyncTimer.Interval = TimeSpan.FromSeconds(30);
            _hubSyncTimer.Tick += HubSyncTimer_Tick;
            
            // Initialisiere WebSocket-Verbindung zum Hub
            InitializeHubWebSocketConnection();
            
            UpdateHubStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Tournament Hub service: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialisiert die WebSocket-Verbindung zum Tournament Hub
    /// </summary>
    private async void InitializeHubWebSocketConnection()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔌 [PLANNER] Initializing WebSocket connection to Tournament Hub...");
            _hubDebugWindow?.AddDebugMessage("🔌 Initialisiere WebSocket-Verbindung zum Tournament Hub", "WEBSOCKET");
            _hubDebugWindow?.AddDebugMessage($"🔗 Hub URL: {_tournamentHubService.HubUrl}", "INFO");
            
            // Update Status sofort
            _hubDebugWindow?.UpdateStatus("Verbinde mit Hub...");
            
            // Initialisiere WebSocket für Tournament Updates
            var success = await _tournamentHubService.InitializeWebSocketAsync();
            
            if (success)
            {
                // Abonniere Hub-Events
                _tournamentHubService.OnMatchResultReceivedFromHub += OnHubMatchResultReceived;
                _tournamentHubService.OnTournamentUpdateReceived += OnHubTournamentUpdateReceived;
                _tournamentHubService.OnConnectionStatusChanged += OnHubConnectionStatusChanged;
                
                System.Diagnostics.Debug.WriteLine("✅ [PLANNER] WebSocket connection to Tournament Hub established");
                _hubDebugWindow?.AddDebugMessage("✅ WebSocket-Verbindung zum Tournament Hub hergestellt", "SUCCESS");
                _hubDebugWindow?.UpdateStatus("WebSocket-Verbindung erfolgreich hergestellt");
                
                // Update Status in UI
                Dispatcher.Invoke(() =>
                {
                    UpdateHubStatus();
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ [PLANNER] WebSocket connection to Tournament Hub failed");
                _hubDebugWindow?.AddDebugMessage("⚠️ WebSocket-Verbindung zum Tournament Hub fehlgeschlagen", "ERROR");
                _hubDebugWindow?.UpdateStatus("WebSocket-Verbindung fehlgeschlagen");
                
                // Retry nach 5 Sekunden
                Task.Run(async () =>
                {
                    await Task.Delay(5000);
                    _hubDebugWindow?.AddDebugMessage("🔄 Versuche WebSocket-Reconnect...", "WEBSOCKET");
                    await Task.Run(InitializeHubWebSocketConnection);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Error initializing Hub WebSocket: {ex.Message}");
            _hubDebugWindow?.AddDebugMessage($"❌ Fehler bei WebSocket-Initialisierung: {ex.Message}", "ERROR");
            _hubDebugWindow?.UpdateStatus($"WebSocket-Fehler: {ex.Message}");
        }
    }

    /// <summary>
    /// Event-Handler für Hub-Verbindungsstatus-Änderungen
    /// </summary>
    private void OnHubConnectionStatusChanged(bool isConnected, string status)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔌 Hub connection status changed: {isConnected} - {status}");
                _hubDebugWindow?.AddDebugMessage($"🔌 Hub-Verbindungsstatus geändert: {isConnected} - {status}", "WEBSOCKET");
                _hubDebugWindow?.UpdateConnectionStatus(isConnected, status);
                
                var wasConnected = _isRegisteredWithHub;
                _isRegisteredWithHub = isConnected;
                UpdateHubStatus();
                
                // VERHINDERE HÄUFIGES RE-SUBSCRIBE: Nur bei tatsächlicher Statusänderung
                if (isConnected && !wasConnected && !string.IsNullOrEmpty(_currentTournamentId))
                {
                    _hubDebugWindow?.AddDebugMessage($"🔄 Statusänderung erkannt - führe Re-Subscribe durch für Tournament: {_currentTournamentId}", "SYNC");
                    
                    // Verzögere Re-Subscribe um mehrfache Aufrufe zu vermeiden
                    Task.Run(async () =>
                    {
                        await Task.Delay(2000); // 2 Sekunden warten
                        
                        // Prüfe erneut ob Verbindung noch besteht
                        if (_isRegisteredWithHub && !string.IsNullOrEmpty(_currentTournamentId))
                        {
                            _hubDebugWindow?.AddDebugMessage($"📡 Führe verzögertes Re-Subscribe durch...", "SYNC");
                            await SubscribeToTournamentUpdates(_currentTournamentId);
                        }
                        else
                        {
                            _hubDebugWindow?.AddDebugMessage($"⏸️ Re-Subscribe übersprungen - Verbindung nicht mehr aktiv", "INFO");
                        }
                    });
                }
                else if (isConnected && wasConnected)
                {
                    _hubDebugWindow?.AddDebugMessage($"ℹ️ Verbindung bereits aktiv - Re-Subscribe übersprungen", "INFO");
                }
                else if (!isConnected)
                {
                    _hubDebugWindow?.AddDebugMessage($"📴 Verbindung getrennt - warte auf Wiederverbindung", "WARNING");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error handling Hub connection status change: {ex.Message}");
                _hubDebugWindow?.AddDebugMessage($"❌ Fehler beim Verarbeiten der Verbindungsstatusänderung: {ex.Message}", "ERROR");
            }
        });
    }

    /// <summary>
    /// Event-Handler für Tournament Updates vom Hub
    /// </summary>
    private void OnHubTournamentUpdateReceived(string tournamentId, object updateData)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📥 Received tournament update from Hub: {tournamentId}");
                
                if (tournamentId == _currentTournamentId)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Processing tournament update for current tournament");
                    
                    // Triggere UI-Refresh für alle Tournament Tabs
                    RefreshAllTournamentTabs();
                    
                    // Update last sync time
                    _lastSyncTime = DateTime.Now;
                    UpdateHubStatus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error processing Hub tournament update: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Abonniert Tournament Updates vom Hub via WebSocket
    /// </summary>
    private async Task SubscribeToTournamentUpdates(string tournamentId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🎯 [PLANNER] ===== SUBSCRIBING TO TOURNAMENT =====");
            System.Diagnostics.Debug.WriteLine($"🎯 [PLANNER] Tournament ID: {tournamentId}");
            
            _hubDebugWindow?.AddDebugMessage("===== SUBSCRIBING TOURNAMENT =====", "TOURNAMENT");
            _hubDebugWindow?.AddDebugMessage($"Tournament ID: {tournamentId}", "TOURNAMENT");
            
            var success = await _tournamentHubService.SubscribeToTournamentAsync(tournamentId);
            
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Successfully subscribed to tournament updates: {tournamentId}");
                _hubDebugWindow?.AddDebugMessage($"✅ Erfolgreich zu Tournament Updates subscribed: {tournamentId}", "SUCCESS");
                
                // Warte kurz, dann registriere als Tournament Planner Client
                await Task.Delay(1000);
                
                var plannerInfo = new
                {
                    ClientType = "Tournament Planner",
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    ClientId = Environment.MachineName,
                    ConnectedAt = DateTime.Now,
                    TournamentId = tournamentId
                };
                
                System.Diagnostics.Debug.WriteLine($"🏁 [PLANNER] Registering as Tournament Planner with info: {JsonSerializer.Serialize(plannerInfo)}");
                _hubDebugWindow?.AddDebugMessage($"🏁 Registriere als Tournament Planner Client", "TOURNAMENT");
                _hubDebugWindow?.AddDebugMessage($"Client Info: {JsonSerializer.Serialize(plannerInfo)}", "INFO");
                
                var plannerSuccess = await _tournamentHubService.RegisterAsPlannerAsync(tournamentId, plannerInfo);
                
                if (plannerSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Successfully registered as Tournament Planner: {tournamentId}");
                    _hubDebugWindow?.AddDebugMessage($"✅ Erfolgreich als Tournament Planner registriert: {tournamentId}", "SUCCESS");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Failed to register as Tournament Planner: {tournamentId}");
                    _hubDebugWindow?.AddDebugMessage($"⚠️ Fehler bei Tournament Planner Registrierung: {tournamentId}", "WARNING");
                }
                
                _lastSyncTime = DateTime.Now;
                UpdateHubStatus();
                
                System.Diagnostics.Debug.WriteLine($"🎯 [PLANNER] ===== SUBSCRIPTION COMPLETE =====");
                _hubDebugWindow?.AddDebugMessage("===== SUBSCRIPTION COMPLETE =====", "TOURNAMENT");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Failed to subscribe to tournament updates: {tournamentId}");
                _hubDebugWindow?.AddDebugMessage($"❌ Fehler beim Subscriben zu Tournament Updates: {tournamentId}", "ERROR");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Error subscribing to tournament updates: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Stack trace: {ex.StackTrace}");
            
            _hubDebugWindow?.AddDebugMessage($"❌ Fehler beim Subscriben: {ex.Message}", "ERROR");
        }
    }

    /// <summary>
    /// Entfernt Tournament Update Subscription
    /// </summary>
    private async Task UnsubscribeFromTournamentUpdates(string tournamentId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📴 Unsubscribing from tournament updates: {tournamentId}");
            
            await _tournamentHubService.UnsubscribeFromTournamentAsync(tournamentId);
            
            System.Diagnostics.Debug.WriteLine($"✅ Successfully unsubscribed from tournament updates: {tournamentId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error unsubscribing from tournament updates: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert alle Tournament Tabs (UI Refresh)
    /// </summary>
    private void RefreshAllTournamentTabs()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 Refreshing all tournament tabs...");
            
            PlatinTab?.Dispatcher.BeginInvoke(() =>
            {
                PlatinTab?.TournamentClass?.TriggerUIRefresh();
            });
            
            GoldTab?.Dispatcher.BeginInvoke(() =>
            {
                GoldTab?.TournamentClass?.TriggerUIRefresh();
            });
            
            SilberTab?.Dispatcher.BeginInvoke(() =>
            {
                SilberTab?.TournamentClass?.TriggerUIRefresh();
            });
            
            BronzeTab?.Dispatcher.BeginInvoke(() =>
            {
                BronzeTab?.TournamentClass?.TriggerUIRefresh();
            });
            
            System.Diagnostics.Debug.WriteLine("✅ All tournament tabs refreshed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error refreshing tournament tabs: {ex.Message}");
        }
    }

    /// <summary>
    /// Event-Handler für Heartbeat-Timer
    /// </summary>
    private async void HubHeartbeatTimer_Tick(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentTournamentId))
        {
            try
            {
                var activeMatches = GetActiveMatchesCount();
                var totalPlayers = GetTotalPlayersCount();
                
                await _tournamentHubService.SendHeartbeatAsync(
                    _currentTournamentId, 
                    activeMatches, 
                    totalPlayers
                );
                
                System.Diagnostics.Debug.WriteLine($"💓 Heartbeat sent for tournament: {_currentTournamentId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Heartbeat error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Event-Handler für Sync-Timer - automatische Synchronisation
    /// </summary>
    private async void HubSyncTimer_Tick(object? sender, EventArgs e)
    {
        if (_isRegisteredWithHub && !_isSyncingWithHub)
        {
            try
            {
                _isSyncingWithHub = true;
                UpdateHubStatus();
                
                await SyncFullTournamentWithHub();
                _lastSyncTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Auto-sync error: {ex.Message}");
            }
            finally
            {
                _isSyncingWithHub = false;
                UpdateHubStatus();
            }
        }
    }

    /// <summary>
    /// Registriert das aktuelle Turnier beim Tournament Hub
    /// </summary>
    private async Task RegisterTournamentWithHub()
    {
        try
        {
            _currentTournamentId = $"TOURNAMENT_{DateTime.Now:yyyyMMdd_HHmmss}";
            
            var success = await _tournamentHubService.RegisterWithHubAsync(
                _currentTournamentId,
                $"Dart Turnier {DateTime.Now:dd.MM.yyyy}",
                "Live Dart Tournament von Tournament Planner"
            );
            
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"🎯 Tournament registered with Hub: {_currentTournamentId}");
                
                _isRegisteredWithHub = true;
                
                // Startet Heartbeat und Sync Timer
                _hubHeartbeatTimer?.Start();
                _hubSyncTimer?.Start();
                
                // API Integration benachrichtigen
                if (_apiService is HttpApiIntegrationService httpApiService)
                {
                    httpApiService.SetCurrentTournamentId(_currentTournamentId);
                }
                
                // WICHTIG: WebSocket Subscription für Tournament Updates
                await SubscribeToTournamentUpdates(_currentTournamentId);
                
                // Initial sync
                await SyncFullTournamentWithHub();
                UpdateHubStatus();
                
                var joinUrl = _tournamentHubService.GetJoinUrl(_currentTournamentId);
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = $"🎯 Tournament erfolgreich beim Hub registriert!\n\n" +
                             $"Tournament ID: {_currentTournamentId}\n" +
                             $"Join URL: {joinUrl}\n\n" +
                             $"Diese URL können Sie an Spieler senden, damit sie ihre Match-Ergebnisse über das Web eingeben können.\n\n" +
                             $"✅ Automatische Synchronisation ist aktiv.\n" +
                             $"🔌 WebSocket-Verbindung für Live-Updates etabliert.";
                
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                System.Windows.Clipboard.SetText(joinUrl);
            }
            else
            {
                var title = _localizationService.GetString("Error") ?? "Fehler";
                var message = "❌ Tournament konnte nicht beim Hub registriert werden.\n\n" +
                             "Mögliche Ursachen:\n" +
                             "• Tournament Hub ist nicht erreichbar\n" +
                             "• Netzwerkverbindung unterbrochen\n" +
                             "• Hub-Server ist offline\n\n" +
                             "Überprüfen Sie Ihre Internetverbindung und versuchen Sie es erneut.";
                
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                _currentTournamentId = string.Empty;
                _isRegisteredWithHub = false;
                UpdateHubStatus();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ RegisterTournamentWithHub: Exception: {ex.Message}");
            
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"❌ Unerwarteter Fehler bei der Hub-Registrierung:\n\n{ex.Message}\n\n" +
                         $"Bitte versuchen Sie es erneut oder kontaktieren Sie den Support.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            
            _currentTournamentId = string.Empty;
            _isRegisteredWithHub = false;
            UpdateHubStatus();
        }
    }

    /// <summary>
    /// Initialisiert die bidirektionale Kommunikation mit dem Tournament Hub
    /// </summary>
    private async Task InitializeHubCommunication()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentTournamentId))
                return;

            // WebSocket-Verbindung ist bereits über InitializeHubWebSocketConnection() etabliert
            System.Diagnostics.Debug.WriteLine($"✅ Hub communication initialized for tournament: {_currentTournamentId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Hub communication initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Event-Handler für Match-Updates die vom Hub empfangen werden
    /// 🚨 KORRIGIERT: Berücksichtigt Group-Information für eindeutige Match-Identifikation
    /// </summary>
    private void OnHubMatchResultReceived(HubMatchUpdateEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                // Erweiterte Debug-Ausgabe für Match Results
                var isMatchResult = e.Source?.Contains("match-result") == true;
                var logCategory = isMatchResult ? "MATCH_RESULT" : "MATCH";
                
                System.Diagnostics.Debug.WriteLine("📥 [PLANNER] ===== MATCH UPDATE FROM HUB =====");
                System.Diagnostics.Debug.WriteLine($"📥 [PLANNER] Received match update from Hub: Match {e.MatchId} in class {e.ClassId}");
                System.Diagnostics.Debug.WriteLine($"📊 [PLANNER] Result: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs, Status: {e.Status}");
                System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Source: {e.Source}, UpdatedAt: {e.UpdatedAt}");
                System.Diagnostics.Debug.WriteLine($"📋 [PLANNER] Group Info: GroupName='{e.GroupName}', GroupId={e.GroupId}, MatchType='{e.MatchType}'");
                
                // DEBUG CONSOLE LOGGING - mit farblicher Hervorhebung
                if (isMatchResult)
                {
                    _hubDebugWindow?.AddDebugMessage("🏆 ===== MATCH-ERGEBNIS EMPFANGEN =====", "MATCH_RESULT");
                    _hubDebugWindow?.AddDebugMessage($"📥 Match-Ergebnis: Match {e.MatchId} in Klasse {e.ClassId}", "MATCH_RESULT");
                    _hubDebugWindow?.AddDebugMessage($"📊 Endergebnis: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs", "MATCH_RESULT");
                    _hubDebugWindow?.AddDebugMessage($"🔍 Status: {e.Status}, Quelle: {e.Source}", "MATCH_RESULT");
                    _hubDebugWindow?.AddDebugMessage($"📋 Group Info: '{e.GroupName}' (ID: {e.GroupId})", "MATCH_RESULT");
                }
                else
                {
                    _hubDebugWindow?.AddDebugMessage("===== MATCH UPDATE FROM HUB =====", "MATCH");
                    _hubDebugWindow?.AddDebugMessage($"📥 Match Update empfangen: Match {e.MatchId} in Klasse {e.ClassId}", "MATCH");
                    _hubDebugWindow?.AddDebugMessage($"📊 Ergebnis: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs", "MATCH");
                    _hubDebugWindow?.AddDebugMessage($"🔍 Status: {e.Status}, Quelle: {e.Source}", "MATCH");
                    _hubDebugWindow?.AddDebugMessage($"📋 Group Info: '{e.GroupName}' (ID: {e.GroupId})", "MATCH");
                }
                
                // Finde die entsprechende Tournament-Klasse
                var tournamentClass = GetTournamentClassById(e.ClassId);
                if (tournamentClass == null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Tournament class {e.ClassId} not found");
                    _hubDebugWindow?.AddDebugMessage($"⚠️ Tournament-Klasse {e.ClassId} nicht gefunden", "WARNING");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found tournament class: {tournamentClass.Name}");
                _hubDebugWindow?.AddDebugMessage($"✅ Tournament-Klasse gefunden: {tournamentClass.Name}", "SUCCESS");

                // Finde das entsprechende Match
                Match? targetMatch = null;
                Group? targetGroup = null;

                // 🚨 KORRIGIERT: Verwende Group-spezifische Suche falls Group-Information verfügbar ist
                if (!string.IsNullOrEmpty(e.GroupName) && e.MatchType == "Group")
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Group-spezifische Suche: Match {e.MatchId} in '{e.GroupName}'");
                    _hubDebugWindow?.AddDebugMessage($"🔍 Group-spezifische Suche: Match {e.MatchId} in '{e.GroupName}'", "SEARCH");
                    
                    // Suche die SPEZIFISCHE Gruppe
                    targetGroup = tournamentClass.Groups
                        .FirstOrDefault(g => g.Name.Equals(e.GroupName, StringComparison.OrdinalIgnoreCase));
                    
                    if (targetGroup != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"📋 [PLANNER] Found target group: {targetGroup.Name}");
                        _hubDebugWindow?.AddDebugMessage($"📋 Zielgruppe gefunden: {targetGroup.Name}", "SUCCESS");
                        
                        // Suche das Match NUR in der spezifischen Gruppe
                        targetMatch = targetGroup.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                        
                        if (targetMatch != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found match {e.MatchId} in specific group '{targetGroup.Name}'");
                            _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} in spezifischer Gruppe '{targetGroup.Name}' gefunden", "MATCH_RESULT");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Match {e.MatchId} not found in group '{targetGroup.Name}'");
                            _hubDebugWindow?.AddDebugMessage($"❌ Match {e.MatchId} nicht in Gruppe '{targetGroup.Name}' gefunden", "WARNING");
                            
                            var availableMatches = string.Join(", ", targetGroup.Matches.Select(m => m.Id));
                            System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Available matches in group: {availableMatches}");
                            _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Matches in Gruppe: {availableMatches}", "INFO");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Group '{e.GroupName}' not found in class {tournamentClass.Name}");
                        _hubDebugWindow?.AddDebugMessage($"❌ Gruppe '{e.GroupName}' nicht in Klasse {tournamentClass.Name} gefunden", "WARNING");
                        
                        var availableGroups = string.Join(", ", tournamentClass.Groups.Select(g => g.Name));
                        System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Available groups: {availableGroups}");
                        _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Gruppen: {availableGroups}", "INFO");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] No group info available - using fallback search in all groups");
                    _hubDebugWindow?.AddDebugMessage($"🔍 Keine Group-Info - verwende Fallback-Suche in allen Gruppen", "SEARCH");
                    
                    // FALLBACK: Suche in allen Gruppen (alte Logik)
                    foreach (var group in tournamentClass.Groups)
                    {
                        targetMatch = group.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                        if (targetMatch != null)
                        {
                            targetGroup = group;
                            System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Found match {e.MatchId} in group '{group.Name}' via fallback search");
                            _hubDebugWindow?.AddDebugMessage($"⚠️ Match {e.MatchId} in Gruppe '{group.Name}' über Fallback-Suche gefunden", "WARNING");
                            break;
                        }
                    }
                }

                // Suche in Finals falls nicht in Gruppen gefunden
                if (targetMatch == null && tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🏆 [PLANNER] Searching in Finals for match {e.MatchId}");
                    _hubDebugWindow?.AddDebugMessage($"🏆 Suche in Finals nach Match {e.MatchId}", "SEARCH");
                    
                    targetMatch = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                    if (targetMatch != null)
                    {
                        targetGroup = tournamentClass.CurrentPhase.FinalsGroup;
                        System.Diagnostics.Debug.WriteLine($"🏆 [PLANNER] Found match {e.MatchId} in Finals");
                        _hubDebugWindow?.AddDebugMessage($"🏆 Match {e.MatchId} in Finals gefunden", "SUCCESS");
                    }
                }

                if (targetMatch == null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Match {e.MatchId} not found in class {e.ClassId}");
                    System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Available matches in class: {string.Join(", ", tournamentClass.Groups.SelectMany(g => g.Matches).Select(m => m.Id))}");
                    
                    _hubDebugWindow?.AddDebugMessage($"⚠️ Match {e.MatchId} nicht gefunden in Klasse {e.ClassId}", "WARNING");
                    _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Matches: {string.Join(", ", tournamentClass.Groups.SelectMany(g => g.Matches).Select(m => m.Id))}", "INFO");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found target match: {targetMatch.Id} ({targetMatch.Player1?.Name} vs {targetMatch.Player2?.Name}) in group '{targetGroup?.Name}'");
                
                if (isMatchResult)
                {
                    _hubDebugWindow?.AddDebugMessage($"✅ Ziel-Match gefunden: {targetMatch.Id} ({targetMatch.Player1?.Name} vs {targetMatch.Player2?.Name}) in Gruppe '{targetGroup?.Name}'", "MATCH_RESULT");
                }
                else
                {
                    _hubDebugWindow?.AddDebugMessage($"✅ Ziel-Match gefunden: {targetMatch.Id} ({targetMatch.Player1?.Name} vs {targetMatch.Player2?.Name}) in Gruppe '{targetGroup?.Name}'", "SUCCESS");
                }

                // Aktualisiere das Match mit den Hub-Daten
                var wasUpdated = UpdateMatchWithHubData(targetMatch, e);
                
                if (wasUpdated)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Match {e.MatchId} updated successfully from Hub");
                    
                    // Triggere UI-Updates
                    tournamentClass.TriggerUIRefresh();
                    MarkAsChanged();
                    
                    // Zeige Benachrichtigung
                    var playerNames = $"{targetMatch.Player1?.Name ?? "Player 1"} vs {targetMatch.Player2?.Name ?? "Player 2"}";
                    var resultInfo = $"{e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs";
                    var groupInfo = targetGroup?.Name != null ? $" in Gruppe '{targetGroup.Name}'" : "";
                    
                    System.Diagnostics.Debug.WriteLine($"🎯 [PLANNER] {playerNames}: {resultInfo}{groupInfo}");
                    System.Diagnostics.Debug.WriteLine($"🔄 [PLANNER] UI refresh triggered for {tournamentClass.Name}");
                    
                    if (isMatchResult)
                    {
                        _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} erfolgreich vom Hub aktualisiert{groupInfo}", "MATCH_RESULT");
                        _hubDebugWindow?.AddDebugMessage($"🎯 {playerNames}: {resultInfo}", "MATCH_RESULT");
                        _hubDebugWindow?.AddDebugMessage($"🔄 UI-Aktualisierung ausgelöst für {tournamentClass.Name}", "MATCH_RESULT");
                    }
                    else
                    {
                        _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} erfolgreich vom Hub aktualisiert{groupInfo}", "SUCCESS");
                        _hubDebugWindow?.AddDebugMessage($"🎯 {playerNames}: {resultInfo}", "MATCH");
                        _hubDebugWindow?.AddDebugMessage($"🔄 UI-Aktualisierung ausgelöst für {tournamentClass.Name}", "SYNC");
                    }
                    
                    // Optional: Zeige Toast-Benachrichtigung
                    ShowToastNotification($"Match Update", $"{playerNames}{groupInfo}\n{resultInfo}", "Hub Update empfangen");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ℹ️ [PLANNER] No changes detected for match {e.MatchId}");
                    _hubDebugWindow?.AddDebugMessage($"ℹ️ Keine Änderungen für Match {e.MatchId} erkannt", "INFO");
                }
                
                if (isMatchResult)
                {
                    _hubDebugWindow?.AddDebugMessage("🏆 ===== MATCH-ERGEBNIS VERARBEITUNG ABGESCHLOSSEN =====", "MATCH_RESULT");
                }
                else
                {
                    _hubDebugWindow?.AddDebugMessage("===== MATCH UPDATE ENDE =====", "MATCH");
                }
                
                System.Diagnostics.Debug.WriteLine("📥 [PLANNER] ===== END MATCH UPDATE =====");
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Error processing Hub match update: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Stack trace: {ex.StackTrace}");
                
                _hubDebugWindow?.AddDebugMessage($"❌ Fehler beim Verarbeiten des Hub Match Updates: {ex.Message}", "ERROR");
            }
        });
    }

    /// <summary>
    /// Aktualisiert ein Match mit Daten vom Hub
    /// </summary>
    private bool UpdateMatchWithHubData(Match match, HubMatchUpdateEventArgs hubData)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔧 UpdateMatchWithHubData called for Match {hubData.MatchId}");
            System.Diagnostics.Debug.WriteLine($"   Current: {match.Player1Sets}-{match.Player2Sets} Sets, {match.Player1Legs}-{match.Player2Legs} Legs, Status: {match.Status}");
            System.Diagnostics.Debug.WriteLine($"   Hub Data: {hubData.Player1Sets}-{hubData.Player2Sets} Sets, {hubData.Player1Legs}-{hubData.Player2Legs} Legs, Status: {hubData.Status}");
            
            // Prüfe ob es tatsächlich Änderungen gibt
            if (match.Player1Sets == hubData.Player1Sets &&
                match.Player2Sets == hubData.Player2Sets &&
                match.Player1Legs == hubData.Player1Legs &&
                match.Player2Legs == hubData.Player2Legs &&
                match.Status.ToString() == hubData.Status)
            {
                System.Diagnostics.Debug.WriteLine($"   No changes detected, skipping update");
                return false; // Keine Änderungen
            }

            // Aktualisiere Match-Daten
            match.Player1Sets = hubData.Player1Sets;
            match.Player2Sets = hubData.Player2Sets;
            match.Player1Legs = hubData.Player1Legs;
            match.Player2Legs = hubData.Player2Legs;
            match.Notes = hubData.Notes ?? match.Notes;

            // Aktualisiere Status
            if (Enum.TryParse<MatchStatus>(hubData.Status, out var newStatus))
            {
                match.Status = newStatus;
                System.Diagnostics.Debug.WriteLine($"   Status updated to: {newStatus}");
            }

            // Setze End-Zeit wenn abgeschlossen
            if (match.Status == MatchStatus.Finished && match.EndTime == null)
            {
                match.EndTime = DateTime.Now;
                System.Diagnostics.Debug.WriteLine($"   End time set to: {match.EndTime}");
            }

            // Bestimme Gewinner
            match.DetermineWinner();
            
            System.Diagnostics.Debug.WriteLine($"   Winner determined: {match.Winner?.Name ?? "None"}");
            System.Diagnostics.Debug.WriteLine($"   ✅ Match updated successfully");

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error updating match with hub data: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Zeigt eine Toast-Benachrichtigung (optional)
    /// </summary>
    private void ShowToastNotification(string title, string message, string source)
    {
        // Für jetzt nur Debug-Output, später kann echte Toast-Notification implementiert werden
        System.Diagnostics.Debug.WriteLine($"🔔 {title}: {message} (Source: {source})");
        
        // TODO: Implementiere echte Toast-Notification mit Windows-System oder eigenem Control
    }

    /// <summary>
    /// Synchronisiert das komplette Tournament mit allen Klassen
    /// </summary>
    private async Task SyncFullTournamentWithHub()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentTournamentId)) return;
            
            var tournamentData = new TournamentData
            {
                TournamentClasses = new List<TournamentClass>
                {
                    PlatinTab.TournamentClass,
                    GoldTab.TournamentClass,
                    SilberTab.TournamentClass,
                    BronzeTab.TournamentClass
                }
            };

            var success = await _tournamentHubService.SyncTournamentWithClassesAsync(
                _currentTournamentId, 
                $"Dart Turnier {DateTime.Now:dd.MM.yyyy}",
                tournamentData
            );
            
            if (success)
            {
                var totalMatches = tournamentData.TournamentClasses
                    .Sum(tc => tc.Groups.Sum(g => g.Matches.Count));
                              
                System.Diagnostics.Debug.WriteLine($"✅ Full tournament sync completed: {totalMatches} matches, 4 classes synced to Hub");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Full tournament sync error: {ex.Message}");
        }
    }

    /// <summary>
    /// Entregistriert das Turnier vom Tournament Hub
    /// </summary>
    private async Task UnregisterTournamentFromHub()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentTournamentId)) return;
            
            // Stop timers
            _hubHeartbeatTimer?.Stop();
            _hubSyncTimer?.Stop();
            
            // Unsubscribe von Tournament Updates
            await UnsubscribeFromTournamentUpdates(_currentTournamentId);
            
            // Cleanup WebSocket Event-Handler
            _tournamentHubService.OnMatchResultReceivedFromHub -= OnHubMatchResultReceived;
            _tournamentHubService.OnTournamentUpdateReceived -= OnHubTournamentUpdateReceived;
            _tournamentHubService.OnConnectionStatusChanged -= OnHubConnectionStatusChanged;
            
            // Unregister from Hub
            await _tournamentHubService.UnregisterFromHubAsync(_currentTournamentId);
            
            _currentTournamentId = string.Empty;
            _isRegisteredWithHub = false;
            _lastSyncTime = DateTime.MinValue;
            
            UpdateHubStatus();
            
            System.Diagnostics.Debug.WriteLine($"📴 Tournament unregistered and WebSocket cleanup completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Hub unregistration error: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert den visuellen Hub-Status in der Statusleiste
    /// </summary>
    private void UpdateHubStatus()
    {
        try
        {
            Dispatcher.Invoke(() =>
            {
                if (_isRegisteredWithHub)
                {
                    HubStatusIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113));
                    HubStatusText.Text = $"Hub: Verbunden ({_currentTournamentId})";
                    
                    if (_isSyncingWithHub)
                    {
                        HubSyncStatus.Text = "🔄 Synchronisiert...";
                        HubSyncStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                    }
                    else if (_lastSyncTime != DateTime.MinValue)
                    {
                        var timeSinceSync = DateTime.Now - _lastSyncTime;
                        if (timeSinceSync.TotalMinutes < 2)
                        {
                            HubSyncStatus.Text = "✅ WebSocket Live";
                            HubSyncStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113));
                        }
                        else
                        {
                            HubSyncStatus.Text = $"⏱️ Sync vor {timeSinceSync.Minutes}min";
                            HubSyncStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(241, 196, 15));
                        }
                    }
                    else
                    {
                        HubSyncStatus.Text = "🔌 WebSocket aktiv";
                        HubSyncStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(52, 152, 219));
                    }
                }
                else
                {
                    HubStatusIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
                    HubStatusText.Text = "Hub: Getrennt";
                    HubSyncStatus.Text = "(WebSocket inaktiv)";
                    HubSyncStatus.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(149, 165, 166));
                }

                UpdateApiStatus();
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating hub status: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert den Sprachindikator in der Statusleiste
    /// </summary>
    private void UpdateLanguageStatus()
    {
        try 
        {
            LanguageStatusBlock.Text = _localizationService.CurrentLanguage.ToUpper();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating language status: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert die Statusleiste mit Save-Status
    /// </summary>
    private void UpdateStatusBar()
    {
        try
        {
            StatusTextBlock.Text = _hasUnsavedChanges ? 
                _localizationService.GetString("HasUnsavedChanges") : 
                "WebSocket-Hub Integration aktiviert";
            
            LastSavedBlock.Text = _hasUnsavedChanges ? 
                _localizationService.GetString("NotSaved") : 
                _localizationService.GetString("Saved");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating status bar: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert den API-Status in der Benutzeroberfläche
    /// </summary>
    private void UpdateApiStatus()
    {
        try
        {
            var isRunning = _apiService.IsApiRunning;
            var statusText = isRunning ? 
                (_localizationService.GetString("APIRunning") ?? "API läuft") : 
                (_localizationService.GetString("APIStopped") ?? "API gestoppt");
            
            ApiStatusMenuItem.Header = $"📊 {statusText}";
            
            Dispatcher.Invoke(() =>
            {
                if (isRunning)
                {
                    ApiStatusIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(46, 204, 113));
                    ApiStatusText.Text = "API: Läuft";
                }
                else
                {
                    ApiStatusIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
                    ApiStatusText.Text = "API: Gestoppt";
                }
            });
            
            StartApiMenuItem.IsEnabled = !isRunning;
            StopApiMenuItem.IsEnabled = isRunning;
            OpenApiDocsMenuItem.IsEnabled = isRunning;
            
            if (isRunning && _apiService.ApiUrl != null)
            {
                ApiStatusMenuItem.Header += $" ({_apiService.ApiUrl})";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateApiStatus: Error: {ex.Message}");
            ApiStatusMenuItem.Header = "📊 " + (_localizationService.GetString("APIError") ?? "API Fehler");
            
            Dispatcher.Invoke(() =>
            {
                ApiStatusIndicator.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(231, 76, 60));
                ApiStatusText.Text = "API: Fehler";
            });
            
            StartApiMenuItem.IsEnabled = true;
            StopApiMenuItem.IsEnabled = false;
            OpenApiDocsMenuItem.IsEnabled = false;
        }
    }
    
    /// <summary>
    /// Lädt gespeicherte Turnierdaten
    /// </summary>
    private async void LoadData()
    {
        try
        {
            var data = await _dataService.LoadTournamentDataAsync();
            if (data.TournamentClasses.Count >= 4)
            {
                UnsubscribeFromChanges(PlatinTab.TournamentClass);
                UnsubscribeFromChanges(GoldTab.TournamentClass);
                UnsubscribeFromChanges(SilberTab.TournamentClass);
                UnsubscribeFromChanges(BronzeTab.TournamentClass);
                
                PlatinTab.TournamentClass = data.TournamentClasses[0];
                GoldTab.TournamentClass = data.TournamentClasses[1];
                SilberTab.TournamentClass = data.TournamentClasses[2];
                BronzeTab.TournamentClass = data.TournamentClasses[3];

                PlatinTab.TournamentClass.Name = "Platin";
                GoldTab.TournamentClass.Name = "Gold";
                SilberTab.TournamentClass.Name = "Silber";
                BronzeTab.TournamentClass.Name = "Bronze";

                PlatinTab.TournamentClass.Id = 1;
                GoldTab.TournamentClass.Id = 2;
                SilberTab.TournamentClass.Id = 3;
                BronzeTab.TournamentClass.Id = 4;

                SubscribeToChanges(PlatinTab.TournamentClass);
                SubscribeToChanges(GoldTab.TournamentClass);
                SubscribeToChanges(SilberTab.TournamentClass);
                SubscribeToChanges(BronzeTab.TournamentClass);
                
                _hasUnsavedChanges = false;
                UpdateStatusBar();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadData: ERROR: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Entfernt Event-Handler-Registrierungen für eine TournamentClass
    /// </summary>
    private void UnsubscribeFromChanges(TournamentClass? tournamentClass)
    {
        if (tournamentClass != null && _subscribedTournaments.Contains(tournamentClass))
        {
            _subscribedTournaments.Remove(tournamentClass);
        }
    }

    /// <summary>
    /// Hilfsmethode um TournamentClass anhand der ID zu finden
    /// </summary>
    private TournamentClass? GetTournamentClassById(int classId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Looking for tournament class with ID: {classId}");
            
            // Durchsuche alle Tournament-Klassen
            var tournamentClasses = new[] { PlatinTab.TournamentClass, GoldTab.TournamentClass, SilberTab.TournamentClass, BronzeTab.TournamentClass };
            
            for (int i = 0; i < tournamentClasses.Length; i++)
            {
                var tc = tournamentClasses[i];
                if (tc != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   Checking class: {tc.Name} (ID: {tc.Id})");
                    if (tc.Id == classId)
                    {
                        System.Diagnostics.Debug.WriteLine($"   ✅ Found matching class: {tc.Name}");
                        return tc;
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"   ❌ No tournament class found with ID: {classId}");
            System.Diagnostics.Debug.WriteLine($"   Available classes: {string.Join(", ", tournamentClasses.Where(tc => tc != null).Select(tc => $"{tc.Name}(ID:{tc.Id})"))}");
            
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error getting tournament class by ID {classId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Aktualisiert die Auto-Save Timer-Konfiguration
    /// </summary>
    private void UpdateAutoSaveTimer()
    {
        _autoSaveTimer.Stop();
        if (_configService.Config.AutoSave)
        {
            _autoSaveTimer.Interval = TimeSpan.FromMinutes(_configService.Config.AutoSaveInterval);
            _autoSaveTimer.Start();
        }
    }

    /// <summary>
    /// Event-Handler für Auto-Save Timer
    /// </summary>
    private async void AutoSave_Tick(object? sender, EventArgs e)
    {
        if (_hasUnsavedChanges)
        {
            await SaveDataInternal();
            UpdateAutoSaveTimer();
        }
    }

    /// <summary>
    /// Interne Methode zum Speichern der Turnierdaten
    /// </summary>
    private async Task SaveDataInternal()
    {
        try
        {
            var data = new TournamentData
            {
                TournamentClasses = new List<TournamentClass>
                {
                    PlatinTab.TournamentClass,
                    GoldTab.TournamentClass,
                    SilberTab.TournamentClass,
                    BronzeTab.TournamentClass
                }
            };

            await _dataService.SaveTournamentDataAsync(data);
            _hasUnsavedChanges = false;
            UpdateStatusBar();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveDataInternal: ERROR: {ex.Message}");
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("ErrorSavingData")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    // EVENT HANDLERS

    private void New_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService.GetString("NewTournament");
        var message = _localizationService.GetString("CreateNewTournament");
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            InitializeTournamentClasses();
            _hasUnsavedChanges = false;
            UpdateStatusBar();
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Tournament files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            var title = _localizationService.GetString("Information");
            var message = _localizationService.GetString("CustomFileNotImplemented");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await SaveDataInternal();
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Tournament files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            var title = _localizationService.GetString("Information");
            var message = _localizationService.GetString("CustomFileSaveNotImplemented");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_configService, _localizationService);
        settingsWindow.Owner = this;
        
        if (settingsWindow.ShowDialog() == true)
        {
            UpdateAutoSaveTimer();
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService.GetString("About");
        var message = _localizationService.GetString("AboutText");
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var helpWindow = new HelpWindow(_localizationService);
            helpWindow.Owner = this;
            helpWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("ErrorOpeningHelp")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Exit_Click(object sender, RoutedEventArgs e)
    {
        await HandleAppExit();
    }

    protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        await HandleAppExit();
    }

    private async Task HandleAppExit()
    {
        if (_hasUnsavedChanges)
        {
            var title = _localizationService.GetString("UnsavedChanges");
            var message = _localizationService.GetString("SaveBeforeExit");
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        await SaveDataInternal();
                        Application.Current.Shutdown();
                    }
                    catch
                    {
                        return;
                    }
                    break;
                case MessageBoxResult.No:
                    Application.Current.Shutdown();
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }
        else
        {
            Application.Current.Shutdown();
        }
    }

    private void OverviewMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var tournamentClasses = new List<TournamentClass>
            {
                PlatinTab.TournamentClass,
                GoldTab.TournamentClass,
                SilberTab.TournamentClass,
                BronzeTab.TournamentClass
            };

            var overviewWindow = new TournamentOverviewWindow(tournamentClasses, _localizationService);
            overviewWindow.Show();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("ErrorOpeningOverview")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BugReport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var bugReportDialog = new BugReportDialog(_localizationService);
            bugReportDialog.Owner = this;
            bugReportDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("Error")}: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Donation_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var donationDialog = new DonationDialog(_localizationService);
            donationDialog.Owner = this;
            donationDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("Error")}: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var allTournamentClasses = new List<TournamentClass>
            {
                PlatinTab.TournamentClass,
                GoldTab.TournamentClass,
                SilberTab.TournamentClass,
                BronzeTab.TournamentClass
            };

            TournamentClass? selectedTournamentClass = PlatinTab.TournamentClass;
            PrintHelper.ShowPrintDialog(allTournamentClasses, selectedTournamentClass, this, _localizationService);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler"; 
            var message = $"Fehler beim Öffnen des Druckdialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // API EVENT HANDLERS

    private async void StartApi_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var tournamentData = new TournamentData
            {
                TournamentClasses = new List<TournamentClass>
                {
                    PlatinTab.TournamentClass,
                    GoldTab.TournamentClass,
                    SilberTab.TournamentClass,
                    BronzeTab.TournamentClass
                }
            };

            var success = await _apiService.StartApiAsync(tournamentData, 5000);
            
            if (success)
            {
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = _localizationService.GetString("APIStarted") ?? 
                             $"API wurde erfolgreich gestartet!\n\nURL: {_apiService.ApiUrl}";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var title = _localizationService.GetString("Error") ?? "Fehler";
                var message = _localizationService.GetString("APIStartError") ?? 
                             "API konnte nicht gestartet werden.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            UpdateApiStatus();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Starten der API: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void StopApi_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var success = await _apiService.StopApiAsync();
            
            if (success)
            {
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = _localizationService.GetString("APIStopped") ?? "API wurde erfolgreich gestoppt.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
            UpdateApiStatus();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Stoppen der API: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenApiDocs_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_apiService.IsApiRunning)
            {
                var title = _localizationService.GetString("Information") ?? "Information";
                var message = "API ist nicht gestartet. Starten Sie die API zuerst.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_apiService.ApiUrl != null)
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _apiService.ApiUrl,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Browsers: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // TOURNAMENT HUB EVENT HANDLERS

    private async void RegisterWithHub_Click(object sender, RoutedEventArgs e)
    {
        await RegisterTournamentWithHub();
    }

    private async void UnregisterFromHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_isRegisteredWithHub)
            {
                var infoTitle = _localizationService.GetString("Information") ?? "Information";
                var infoMessage = "Kein Tournament beim Hub registriert.";
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmTitle = "Tournament entregistrieren";
            var confirmMessage = $"Tournament '{_currentTournamentId}' wirklich vom Hub entregistrieren?";
            
            var result = MessageBox.Show(confirmMessage, confirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                await UnregisterTournamentFromHub();
                
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = "Tournament erfolgreich vom Hub entregistriert.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Entregistrieren: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ManualSyncWithHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_isRegisteredWithHub)
            {
                var infoTitle = _localizationService.GetString("Information") ?? "Information";
                var infoMessage = "Kein Tournament beim Hub registriert.";
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isSyncingWithHub = true;
            UpdateHubStatus();
            
            try 
            {
                await SyncFullTournamentWithHub();
                _lastSyncTime = DateTime.Now;
                
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = "Tournament erfolgreich mit Hub synchronisiert!";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                var title = _localizationService.GetString("Error") ?? "Fehler";
                var message = "Fehler beim Synchronisieren mit Hub.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim manuellen Sync: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _isSyncingWithHub = false;
            UpdateHubStatus();
        }
    }

    private void ShowJoinUrl_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentTournamentId))
            {
                var infoTitle = _localizationService.GetString("Information") ?? "Information";
                var infoMessage = "Kein Tournament beim Hub registriert.";
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var joinUrl = _tournamentHubService.GetJoinUrl(_currentTournamentId);
            
            var dialogTitle = "Tournament Join URL";
            var dialogMessage = $"Tournament ID: {_currentTournamentId}\n\n" +
                               $"Join URL:\n{joinUrl}\n\n" +
                               $"Diese URL können Sie an Spieler senden.";
            
            MessageBox.Show(dialogMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            System.Windows.Clipboard.SetText(joinUrl);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Anzeigen der Join-URL: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void HubSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentHubUrl = _tournamentHubService.HubUrl;
            
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Geben Sie die Tournament Hub URL ein:",
                "Hub-Einstellungen",
                currentHubUrl
            );
            
            if (!string.IsNullOrWhiteSpace(input) && input != currentHubUrl)
            {
                _tournamentHubService.HubUrl = input.Trim();
                
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = $"Hub-URL aktualisiert:\n{input}";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler bei den Hub-Einstellungen: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnApiMatchResultUpdated(object? sender, MatchResultUpdateEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                var tournamentClass = GetTournamentClassById(e.ClassId);
                if (tournamentClass != null)
                {
                    tournamentClass.TriggerUIRefresh();
                    MarkAsChanged();
                }
                
                UpdateStatusBar();
                UpdateHubStatus();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing API match result update: {ex.Message}");
            }
        });
    }

    // HELPER METHODS

    /// <summary>
    /// Markiert die Anwendung als "geändert" und löst Auto-Save-Logik aus
    /// </summary>
    private void MarkAsChanged()
    {
        _hasUnsavedChanges = true;
        UpdateStatusBar();
        
        if (_configService.Config.AutoSave)
        {
            _autoSaveTimer.Stop();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(2);
            _autoSaveTimer.Start();
        }
        
        // Triggere Hub-Synchronisation wenn registriert
        if (_isRegisteredWithHub && !_isSyncingWithHub)
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                if (!_isSyncingWithHub)
                {
                    try
                    {
                        _isSyncingWithHub = true;
                        UpdateHubStatus();
                        
                        await SyncFullTournamentWithHub();
                        _lastSyncTime = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Auto-sync with Hub failed: {ex.Message}");
                    }
                    finally
                    {
                        _isSyncingWithHub = false;
                        Dispatcher.Invoke(UpdateHubStatus);
                    }
                }
            });
        }
    }

    /// <summary>
    /// Aktualisiert alle übersetzten Texte in der Benutzeroberfläche
    /// </summary>
    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("AppTitle");
        
        // Menü-Einträge aktualisieren
        FileMenuItem.Header = _localizationService.GetString("File");
        NewMenuItem.Header = _localizationService.GetString("New");
        OpenMenuItem.Header = _localizationService.GetString("Open");
        SaveMenuItem.Header = _localizationService.GetString("Save");
        SaveAsMenuItem.Header = _localizationService.GetString("SaveAs");
        PrintMenuItem.Header = "🖨️ " + (_localizationService.GetString("Print") ?? "Drucken");
        ExitMenuItem.Header = _localizationService.GetString("Exit");
        ViewMenuItem.Header = _localizationService.GetString("View");
        OverviewModeMenuItem.Header = _localizationService.GetString("TournamentOverview");
        
        // API Menü-Einträge aktualisieren
        ApiMenuItem.Header = "🌐 " + (_localizationService.GetString("API") ?? "API");
        StartApiMenuItem.Header = "▶️ " + (_localizationService.GetString("StartAPI") ?? "API starten");
        StopApiMenuItem.Header = "⏹️ " + (_localizationService.GetString("StopAPI") ?? "API stoppen");
        OpenApiDocsMenuItem.Header = "📖 " + (_localizationService.GetString("APIDocumentation") ?? "API Dokumentation");
        
        // Tournament Hub Menü-Einträge
        TournamentHubMenuItem.Header = "🎯 " + (_localizationService.GetString("TournamentHub") ?? "Tournament Hub");
        RegisterWithHubMenuItem.Header = "🏁 " + (_localizationService.GetString("RegisterWithHub") ?? "Bei Hub registrieren");
        UnregisterFromHubMenuItem.Header = "📴 " + (_localizationService.GetString("UnregisterFromHub") ?? "Vom Hub entregistrieren");
        ShowJoinUrlMenuItem.Header = "📱 " + (_localizationService.GetString("ShowJoinURL") ?? "Join-URL anzeigen");
        ManualSyncMenuItem.Header = "🔄 " + (_localizationService.GetString("ManualSync") ?? "Manuell synchronisieren");
        HubSettingsMenuItem.Header = "⚙️ " + (_localizationService.GetString("HubSettings") ?? "Hub-Einstellungen");
        
        SettingsMenuItem.Header = _localizationService.GetString("Settings");
        HelpMenuItem.Header = _localizationService.GetString("Help");
        HelpContentMenuItem.Header = "📖 " + _localizationService.GetString("Help");
        BugReportMenuItem.Header = _localizationService.GetString("BugReport");
        AboutMenuItem.Header = _localizationService.GetString("About");

        // Tab-Header aktualisieren
        var platinTextBlock = FindTextBlockInHeader(PlatinTabItem);
        if (platinTextBlock != null) platinTextBlock.Text = _localizationService.GetString("Platinum");
        
        var goldTextBlock = FindTextBlockInHeader(GoldTabItem);
        if (goldTextBlock != null) goldTextBlock.Text = _localizationService.GetString("Gold");
        
        var silverTextBlock = FindTextBlockInHeader(SilverTabItem);
        if (silverTextBlock != null) silverTextBlock.Text = _localizationService.GetString("Silver");
        
        var bronzeTextBlock = FindTextBlockInHeader(BronzeTabItem);
        if (bronzeTextBlock != null) bronzeTextBlock.Text = _localizationService.GetString("Bronze");

        // Spenden-Button aktualisieren
        DonationButton.Content = _localizationService.GetString("Donate");
        DonationButton.ToolTip = _localizationService.GetString("DonateTooltip");

        // Statusleiste und Sprachindikator aktualisieren
        UpdateLanguageStatus();
        UpdateStatusBar();
        UpdateApiStatus();
        
        // Child-Controls zur Übersetzungsaktualisierung auffordern
        PlatinTab?.Dispatcher.BeginInvoke(() => PlatinTab?.UpdateTranslations());
        GoldTab?.Dispatcher.BeginInvoke(() => GoldTab?.UpdateTranslations());
        SilberTab?.Dispatcher.BeginInvoke(() => SilberTab?.UpdateTranslations());
        BronzeTab?.Dispatcher.BeginInvoke(() => BronzeTab?.UpdateTranslations());
    }

    /// <summary>
    /// Hilfsmethode: Findet das TextBlock-Element im Header eines TabItems
    /// </summary>
    private TextBlock? FindTextBlockInHeader(TabItem tabItem)
    {
        if (tabItem.Header is StackPanel stackPanel)
        {
            return stackPanel.Children.OfType<TextBlock>().FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Hilfsmethode: Zählt aktive Matches
    /// </summary>
    private int GetActiveMatchesCount()
    {
        var count = 0;
        count += PlatinTab.TournamentClass?.Groups?.Sum(g => g.Matches?.Count(m => m.Status == MatchStatus.InProgress) ?? 0) ?? 0;
        count += GoldTab.TournamentClass?.Groups?.Sum(g => g.Matches?.Count(m => m.Status == MatchStatus.InProgress) ?? 0) ?? 0;
        count += SilberTab.TournamentClass?.Groups?.Sum(g => g.Matches?.Count(m => m.Status == MatchStatus.InProgress) ?? 0) ?? 0;
        count += BronzeTab.TournamentClass?.Groups?.Sum(g => g.Matches?.Count(m => m.Status == MatchStatus.InProgress) ?? 0) ?? 0;
        return count;
    }

    /// <summary>
    /// Hilfsmethode: Zählt Gesamtspieler
    /// </summary>
    private int GetTotalPlayersCount()
    {
        var count = 0;
        count += PlatinTab.TournamentClass?.Groups?.Sum(g => g.Players?.Count ?? 0) ?? 0;
        count += GoldTab.TournamentClass?.Groups?.Sum(g => g.Players?.Count ?? 0) ?? 0;
        count += SilberTab.TournamentClass?.Groups?.Sum(g => g.Players?.Count ?? 0) ?? 0;
        count += BronzeTab.TournamentClass?.Groups?.Sum(g => g.Players?.Count ?? 0) ?? 0;
        return count;
    }

    /// <summary>
    /// Initialisiert die Hub Debug Console
    /// </summary>
    private void InitializeHubDebugConsole()
    {
        try
        {
            _hubDebugWindow = new HubDebugWindow();
            _hubDebugWindow.AddDebugMessage("🎯 Hub Debug Console initialisiert", "SUCCESS");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing Hub Debug Console: {ex.Message}");
        }
    }

    /// <summary>
    /// Zeigt oder versteckt die Hub Debug Console
    /// </summary>
    private void ToggleHubDebugConsole()
    {
        try
        {
            if (_hubDebugWindow.IsVisible)
            {
                _hubDebugWindow.Hide();
            }
            else
            {
                _hubDebugWindow.Show();
                _hubDebugWindow.Owner = this;
                _hubDebugWindow.AddDebugMessage("🔍 Debug Console geöffnet", "INFO");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error toggling Hub Debug Console: {ex.Message}");
        }
    }

    /// <summary>
    /// Event-Handler für Hub Status Click - öffnet Debug Console
    /// </summary>
    private void HubStatus_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            ToggleHubDebugConsole();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening Hub Debug Console: {ex.Message}");
            MessageBox.Show($"Fehler beim Öffnen der Debug Console: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}