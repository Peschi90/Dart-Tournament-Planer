using System;
using System.Windows;
using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Models.HubSync;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Services.PowerScore;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Verantwortlich für die Initialisierung aller Services im MainWindow
/// Trennt die Service-Setup-Logik vom MainWindow für bessere Wartbarkeit
/// </summary>
public class MainWindowServiceInitializer
{
    private readonly MainWindow _mainWindow;
    private readonly Dispatcher _dispatcher;

    // Services
    public ConfigService ConfigService { get; }
    public LocalizationService LocalizationService { get; }
    public IApiIntegrationService ApiService { get; }
    public TournamentManagementService TournamentService { get; }
    public LicensedHubService HubService { get; }
    public HubMatchProcessingService HubMatchProcessor { get; }
    public MainWindowUIHelper UiHelper { get; }
    public LicenseManager LicenseManager { get; }
    public LicenseFeatureService LicenseFeatureService { get; }
    public PowerScoringService PowerScoringService { get; }
    public UserAuthService UserAuthService { get; }

    public MainWindowServiceInitializer(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _dispatcher = mainWindow.Dispatcher;

        // Services aus App.xaml.cs holen
        ConfigService = App.ConfigService ?? throw new InvalidOperationException("ConfigService not initialized");
        LocalizationService = App.LocalizationService ?? throw new InvalidOperationException("LocalizationService not initialized");
        UserAuthService = App.UserAuthService ?? throw new InvalidOperationException("UserAuthService not initialized");

        // License Services initialisieren
        LicenseManager = new Services.License.LicenseManager();
        LicenseFeatureService = new LicenseFeatureService(LicenseManager);

        // API Service mit Lizenzierung
        var originalApiService = App.ApiIntegrationService ?? throw new InvalidOperationException("ApiIntegrationService not initialized");
        ApiService = new LicensedApiIntegrationService(LicenseFeatureService, LocalizationService);

        // Tournament Service
        TournamentService = new TournamentManagementService(LocalizationService, App.DataService!);

        // Hub Services
        var innerHubService = new HubIntegrationService(ConfigService, LocalizationService, ApiService, _dispatcher, LicenseManager);
        HubService = new LicensedHubService(innerHubService, LicenseFeatureService, LocalizationService, LicenseManager);

        // Hub Match Processor
        HubMatchProcessor = new HubMatchProcessingService(TournamentService.GetTournamentClassById);

        // UI Helper
        UiHelper = new MainWindowUIHelper(LocalizationService, _dispatcher);

        // PowerScoring Service
        PowerScoringService = new PowerScoringService();
    }

    /// <summary>
    /// Konfiguriert alle Services und ihre Event-Handler
    /// </summary>
    public void ConfigureServices(
        Action markAsChanged,
        Action updateTranslations,
        Action forceUIUpdate,
        Action configureTournamentTabs,
        Func<HubMatchUpdateEventArgs, Task> onHubMatchResultReceived,
        Func<HubConnectionState, Task> onHubConnectionStateChanged,
        Func<Task> onTournamentNeedsResync,
        Func<object?, MatchResultUpdateEventArgs, Task> onApiMatchResultUpdated,
        Func<object?, Models.License.LicenseStatus, Task> onLicenseStatusChanged,
        Func<HubMatchUpdateEventArgs, Task> onHubMatchStarted,
        Func<HubMatchUpdateEventArgs, Task> onHubLegCompleted,
        Func<HubMatchUpdateEventArgs, Task> onHubMatchProgressUpdated)
    {
        // Localization Service Events
        LocalizationService.PropertyChanged += (s, e) => updateTranslations();

        // Config Service Events
        ConfigService.LanguageChanged += (s, language) =>
        {
            LocalizationService.SetLanguage(language);
            _dispatcher.BeginInvoke(() =>
            {
                UiHelper.UpdateLanguageStatus();
                updateTranslations();
                forceUIUpdate();
            }, DispatcherPriority.Render);
        };

        // Tournament Service Events
        TournamentService.DataChanged += markAsChanged;

        // Hub Service Events
        HubService.MatchResultReceived += async (args) => await onHubMatchResultReceived(args);
        HubService.HubConnectionStateChanged += async (state) => await onHubConnectionStateChanged(state);
        HubService.TournamentNeedsResync += async () => await onTournamentNeedsResync();
        HubService.DataChanged += markAsChanged;
        HubService.TournamentSyncPayloadReceived += payload => _dispatcher.BeginInvoke(() => OnTournamentSyncPayloadReceived?.Invoke(payload));

        // Hub Live Events
        if (HubService.TournamentHubService != null)
        {
            HubService.TournamentHubService.OnMatchStarted += async (args) => await onHubMatchStarted(args);
            HubService.TournamentHubService.OnLegCompleted += async (args) => await onHubLegCompleted(args);
            HubService.TournamentHubService.OnMatchProgressUpdated += async (args) => await onHubMatchProgressUpdated(args);
            System.Diagnostics.Debug.WriteLine("✅ [ServiceInitializer] Live-Update Event-Handler registriert");
        }

        // License Service Events
        LicenseFeatureService.LicenseStatusChanged += async (sender, status) => await onLicenseStatusChanged(sender, status);

        // API Service Events
        ApiService.MatchResultUpdated += async (sender, e) => await onApiMatchResultUpdated(sender, e);

        // Auth Events -> Planner Auto-Registration beim Hub
        UserAuthService.CurrentUserChanged += async (sender, user) => await RegisterPlannerPresenceAsync(user);

        // Wenn Benutzer abgemeldet, Hub informieren
        UserAuthService.CurrentUserChanged += async (sender, user) =>
        {
            if (user == null)
            {
                await UnregisterPlannerPresenceAsync("user-logout");
            }
        };

        // Falls Session bereits vorhanden ist, Planner direkt registrieren
        if (UserAuthService.CurrentUser != null)
        {
            _ = RegisterPlannerPresenceAsync(UserAuthService.CurrentUser);
        }

        System.Diagnostics.Debug.WriteLine("✅ [ServiceInitializer] All services configured");
    }

    /// <summary>
    /// Initialisiert das Lizenz-System asynchron
    /// </summary>
    public async Task InitializeLicenseSystemAsync()
    {
        try
        {
            await LicenseFeatureService.InitializeAsync();
            System.Diagnostics.Debug.WriteLine("✅ [ServiceInitializer] License system initialized");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [ServiceInitializer] License system initialization error: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialisiert den Hub Service asynchron
    /// </summary>
    public async Task InitializeHubAsync()
    {
        try
        {
            await HubService.InitializeAsync();
            await RegisterPlannerPresenceAsync(UserAuthService.CurrentUser);
            System.Diagnostics.Debug.WriteLine("✅ [ServiceInitializer] Hub service initialized");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [ServiceInitializer] Hub initialization error: {ex.Message}");
        }
    }

    private async Task RegisterPlannerPresenceAsync(AuthenticatedUser? user)
    {
        try
        {
            await HubService.RegisterPlannerPresenceAsync(user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [ServiceInitializer] Planner presence registration failed: {ex.Message}");
        }
    }

    private async Task UnregisterPlannerPresenceAsync(string reason)
    {
        try
        {
            await HubService.UnregisterPlannerPresenceAsync(reason);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [ServiceInitializer] Planner presence unregister failed: {ex.Message}");
        }
    }

    public event Action<HubTournamentSyncPayload>? OnTournamentSyncPayloadReceived;

    public HubTournamentSyncPayload? GetLastTournamentSyncPayload() => HubService.GetLastTournamentSyncPayload();
    public IReadOnlyList<HubTournamentSyncPayload> GetStoredTournamentSyncMessages() => HubService.GetStoredTournamentSyncMessages();
}
