using DartTournamentPlaner.Controls;
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

    public event Action? DataChanged
    {
        add => _innerHubService.DataChanged += value;
        remove => _innerHubService.DataChanged -= value;
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
    public async Task<bool> RegisterTournamentAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔗 LicensedHubService: RegisterTournamentAsync requested");
        
        if (!HasHubConnectionLicense())
        {
            System.Diagnostics.Debug.WriteLine("❌ Hub registration denied - no valid license");
            ShowHubLicenseRequired();
            return false;
        }

        System.Diagnostics.Debug.WriteLine("✅ Hub license verified - registering tournament...");
        
        try
        {
            bool registrationResult = await _innerHubService.RegisterTournamentAsync();
            
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
                    return await RegisterTournamentAsync();
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
                System.Diagnostics.Debug.WriteLine("🔄 User requested retry after exception - attempting registration again");
                return await RegisterTournamentAsync();
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

    public async Task UnregisterTournamentAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔗 LicensedHubService: UnregisterTournamentAsync requested");
        
        // Unregister ist immer erlaubt - auch ohne Lizenz
        await _innerHubService.UnregisterTournamentAsync();
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

    // Unlizenzierte Methoden (immer verfügbar)
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
}