using DartTournamentPlaner.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Views;  // NEU: Für die neuen Dialog-Views
using DartTournamentPlaner.Services.License;
using System;
using System.Windows;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Lizenzierter Hub Service - Wrapper um HubIntegrationService
/// Prüfgt die Lizenz vor Hub-Operationen
/// </summary>
public class LicensedHubService
{
    private readonly HubIntegrationService _innerHubService;
    private readonly LicenseFeatureService? _licenseFeatureService;
    private readonly LocalizationService? _localizationService;
    private readonly Services.License.LicenseManager? _licenseManager;

    // Delegiere Properties
    public bool IsRegisteredWithHub => _innerHubService.IsRegisteredWithHub;
    public bool IsSyncing => _innerHubService.IsSyncing;
    public DateTime? LastSyncTime => _innerHubService.LastSyncTime;
    public ITournamentHubService? TournamentHubService => _innerHubService.TournamentHubService;
    public HubConnectionState CurrentConnectionState => _innerHubService.CurrentConnectionState;
    public bool IsWebSocketConnected => _innerHubService.IsWebSocketConnected;
    
    /// <summary>
    /// ✅ NEU: Direkter Zugriff auf den inneren HubIntegrationService für QR-Code Generierung
    /// </summary>
    public HubIntegrationService InnerHubService => _innerHubService;

    // Events
    public event Action<HubMatchUpdateEventArgs>? MatchResultReceived
    {
        add => _innerHubService.MatchResultReceived += value;
        remove => _innerHubService.MatchResultReceived -= value;
    }

    public event Action<bool>? HubStatusChanged
    {
        add => _innerHubService.HubStatusChanged += value;
        remove => _innerHubService.HubStatusChanged -= value;
    }
    
    // ✅ NEW: Detailed connection state event
    public event Action<HubConnectionState>? HubConnectionStateChanged
    {
        add => _innerHubService.HubConnectionStateChanged += value;
        remove => _innerHubService.HubConnectionStateChanged -= value;
    }

    public event Action? DataChanged
    {
        add => _innerHubService.DataChanged += value;
        remove => _innerHubService.DataChanged -= value;
    }
    
    // ✅ NEW: Tournament needs resync event
    public event Func<Task>? TournamentNeedsResync
    {
        add => _innerHubService.TournamentNeedsResync += value;
        remove => _innerHubService.TournamentNeedsResync -= value;
    }
    
    // ✅ NEW: PowerScoring event
    public event EventHandler<PowerScore.PowerScoringHubMessage>? PowerScoringMessageReceived
    {
        add => _innerHubService.PowerScoringMessageReceived += value;
        remove => _innerHubService.PowerScoringMessageReceived -= value;
    }

    public LicensedHubService(
        HubIntegrationService innerHubService, 
        LicenseFeatureService? licenseFeatureService, 
        LocalizationService? localizationService,
        Services.License.LicenseManager? licenseManager)
    {
        _innerHubService = innerHubService;
        _licenseFeatureService = licenseFeatureService;
        _localizationService = localizationService;
        _licenseManager = licenseManager;
    }

    /// <summary>
    /// Prüft ob Hub-Connection-Lizenz vorhanden ist
    /// </summary>
    private bool HasHubConnectionLicense()
    {
        try
        {
            if (_licenseFeatureService == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ LicenseFeatureService not available for Hub - allowing access");
                return true; // Fallback: erlaubt Zugriff wenn Service nicht verfügbar
            }

            var status = _licenseFeatureService.CurrentStatus;
            var hasHubConnection = _licenseFeatureService.HasFeature(Models.License.LicenseFeatures.HUB_CONNECTION);

            System.Diagnostics.Debug.WriteLine($"🔍 Hub License Check:");
            System.Diagnostics.Debug.WriteLine($"   - Status.IsLicensed: {status?.IsLicensed ?? false}");
            System.Diagnostics.Debug.WriteLine($"   - Status.IsValid: {status?.IsValid ?? false}");
            System.Diagnostics.Debug.WriteLine($"   - HasFeature(HUB_CONNECTION): {hasHubConnection}");

            if (status?.ActiveFeatures != null && status.ActiveFeatures.Any())
            {
                System.Diagnostics.Debug.WriteLine($"   - Active Features: {string.Join(", ", status.ActiveFeatures)}");
            }

            var result = status?.IsLicensed == true && hasHubConnection;
            System.Diagnostics.Debug.WriteLine($"🔍 Hub License Check Result: {result}");
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error checking Hub license: {ex.Message}");
            return true; // Fallback: erlaubt Zugriff bei Fehlern
        }
    }

    /// <summary>
    /// Zeigt Hub License Required Dialog - NEU: Modernized mit eigenständigem Dialog
    /// </summary>
    private void ShowHubLicenseRequired()
    {
        System.Diagnostics.Debug.WriteLine("🔒 Hub connection denied - license required - showing modern dialog");
        
        try
        {
            // NEU: Verwende das neue HubLicenseRequiredDialog
            HubLicenseRequiredDialog.ShowLicenseRequiredDialog(
                Application.Current.MainWindow,
                _localizationService,
                _licenseManager
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error showing Hub license dialog: {ex.Message}");
        }
    }

    // NEU: Verbesserte lizenzierte Hub-Methoden mit modernen Dialogen
    
    /// <summary>
    /// ⭐ ERWEITERT: Registriert Tournament mit optionaler custom ID
    /// </summary>
    /// <param name="customTournamentId">Optional: Benutzerdefinierte Tournament-ID</param>
  public async Task<bool> RegisterTournamentAsync(string? customTournamentId = null)
    {
   System.Diagnostics.Debug.WriteLine($"🔗 LicensedHubService: RegisterTournamentAsync requested with custom ID: {customTournamentId ?? "null"}");
        
        if (!HasHubConnectionLicense())
        {
          System.Diagnostics.Debug.WriteLine("❌ Hub registration denied - no valid license");
      ShowHubLicenseRequired();
            return false;
 }

        System.Diagnostics.Debug.WriteLine("✅ Hub license verified - registering tournament...");
        
        try
  {
      // ⭐ ERWEITERT: Übergebe custom ID an inneren Service
            bool registrationResult = await _innerHubService.RegisterTournamentAsync(customTournamentId);
            
    if (registrationResult)
        {
           // Erfolgreiche Registrierung - zeige Success Dialog
   System.Diagnostics.Debug.WriteLine("✅ Tournament registration successful - showing success dialog");
     
  var tournamentId = _innerHubService.GetCurrentTournamentId();
      var joinUrl = _innerHubService.GetJoinUrl();
   
            ShowHubRegistrationSuccess(tournamentId, joinUrl);
                
         return true;
      }
else
            {
            // Registrierung fehlgeschlagen - zeige Error Dialog
     System.Diagnostics.Debug.WriteLine("❌ Tournament registration failed - showing error dialog");
     
          var shouldRetry = ShowHubRegistrationError(
     "Die Turnier-Registrierung ist fehlgeschlagen. Bitte prüfen Sie Ihre Internetverbindung und Hub-Einstellungen.",
     "HUB_REGISTRATION_FAILED",
        null);
      
       if (shouldRetry)
           {
  System.Diagnostics.Debug.WriteLine("🔄 User requested retry - attempting registration again");
         return await RegisterTournamentAsync(customTournamentId);  // ⭐ Übergebe custom ID beim Retry
    }
        
                return false;
            }
   }
        catch (Exception ex)
      {
     System.Diagnostics.Debug.WriteLine($"❌ Exception during tournament registration: {ex.Message}");
    
       // Exception - zeige Error Dialog mit Details
            var shouldRetry = ShowHubRegistrationError(
  ex.Message,
          "EXCEPTION",
   ex.StackTrace);
       
            if (shouldRetry)
         {
       return await RegisterTournamentAsync(customTournamentId);  // ⭐ Übergebe custom TournamentId beim Retry
            }
     
         return false;
        }
    }
    /// <summary>
    /// Zeigt den Hub Registration Success Dialog
    /// </summary>
    private void ShowHubRegistrationSuccess(string tournamentId, string joinUrl)
    {
        try
        {
            // NEU: Verwende den neuen PrintDialog-Stil Dialog
            HubRegistrationSuccessDialog.ShowSuccessDialog(
                Application.Current.MainWindow,
                tournamentId,
                joinUrl,
                _localizationService
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error showing hub registration success dialog: {ex.Message}");
        }
    }

    /// <summary>
    /// Zeigt den Hub Registration Error Dialog
    /// </summary>
    private bool ShowHubRegistrationError(string errorMessage, string? errorCode = null, string? technicalDetails = null)
    {
        try
        {
            // NEU: Verwende den neuen PrintDialog-Stil Dialog
            return HubRegistrationErrorDialog.ShowErrorDialog(
                Application.Current.MainWindow,
                errorMessage,
                errorCode,
                technicalDetails,
                _localizationService
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error showing hub registration error dialog: {ex.Message}");
            return false;
        }
    }

    // Unlizenzierte Methoden (immer verfügbar)
    public string GenerateNewTournamentId()
    {
        return _innerHubService.GenerateNewTournamentId();
    }

    public async Task<bool> UnregisterTournamentAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔗 LicensedHubService: UnregisterTournamentAsync requested");
        
        // Unregister ist immer erlaubt - auch ohne Lizenz
        return await _innerHubService.UnregisterTournamentAsync();
    }

    public async Task<bool> SyncTournamentAsync(Models.TournamentData tournamentData)
    {
        System.Diagnostics.Debug.WriteLine("🔗 LicensedHubService: SyncTournamentAsync requested");
        
        if (!HasHubConnectionLicense())
        {
            System.Diagnostics.Debug.WriteLine("❌ Hub sync denied - no valid license");
            return false;
        }

        return await _innerHubService.SyncTournamentAsync(tournamentData);
    }

    public async Task<PlannerTournamentsResponse?> FetchPlannerTournamentsAsync(int days = 14)
    {
        if (!HasHubConnectionLicense())
        {
            ShowHubLicenseRequired();
            return null;
        }

        return await _innerHubService.FetchPlannerTournamentsAsync(days);
    }

    public async Task InitializeAsync()
    {
        await _innerHubService.InitializeAsync();
    }

    public string GetCurrentTournamentId()
    {
        return _innerHubService.GetCurrentTournamentId();
    }

    public string GetJoinUrl()
    {
        return _innerHubService.GetJoinUrl();
    }

    public void UpdateHubUrl(string newHubUrl)
    {
        _innerHubService.UpdateHubUrl(newHubUrl);
    }

    public void ShowDebugConsole()
    {
        _innerHubService.ShowDebugConsole();
    }
}