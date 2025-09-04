using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Views;  // Add this import
using System.Diagnostics;
using System.Windows;  // Add this import

namespace DartTournamentPlaner.Services;

/// <summary>
/// Lizenzierter API Service - Wrapper um HttpApiIntegrationService
/// Dieser Service prüft die Lizenz vor allen API-Operationen
/// </summary>
public class LicensedApiIntegrationService : IApiIntegrationService
{
    private readonly HttpApiIntegrationService _innerApiService;
    private readonly LicenseFeatureService? _licenseFeatureService;
    private readonly Services.LocalizationService? _localizationService;

    public bool IsApiRunning => _innerApiService.IsApiRunning;
    public string? ApiUrl => _innerApiService.ApiUrl;

    public event EventHandler<MatchResultUpdateEventArgs>? MatchResultUpdated
    {
        add => _innerApiService.MatchResultUpdated += value;
        remove => _innerApiService.MatchResultUpdated -= value;
    }

    public LicensedApiIntegrationService(LicenseFeatureService? licenseFeatureService = null, Services.LocalizationService? localizationService = null)
    {
        _innerApiService = new HttpApiIntegrationService();
        _licenseFeatureService = licenseFeatureService;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Prüft ob API-Lizenz vorhanden ist
    /// </summary>
    private bool HasApiLicense()
    {
        try
        {
            if (_licenseFeatureService == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ LicenseFeatureService not available - allowing API access");
                return true; // Fallback: erlaubt Zugriff wenn Service nicht verfügbar
            }

            var status = _licenseFeatureService.CurrentStatus;
            var hasApiConnection = _licenseFeatureService.HasFeature(Models.License.LicenseFeatures.API_CONNECTION);

            System.Diagnostics.Debug.WriteLine($"🔍 API License Check:");
            System.Diagnostics.Debug.WriteLine($"   - Status.IsLicensed: {status?.IsLicensed ?? false}");
            System.Diagnostics.Debug.WriteLine($"   - Status.IsValid: {status?.IsValid ?? false}");
            System.Diagnostics.Debug.WriteLine($"   - HasFeature(API_CONNECTION): {hasApiConnection}");

            if (status?.ActiveFeatures != null && status.ActiveFeatures.Any())
            {
                System.Diagnostics.Debug.WriteLine($"   - Active Features: {string.Join(", ", status.ActiveFeatures)}");
            }

            var result = status?.IsLicensed == true && hasApiConnection;
            System.Diagnostics.Debug.WriteLine($"🔍 API License Check Result: {result}");
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error checking API license: {ex.Message}");
            return true; // Fallback: erlaubt Zugriff bei Fehlern
        }
    }

    /// <summary>
    /// Shows license required dialog for API access
    /// </summary>
    private void ShowApiLicenseRequired()
    {
        System.Diagnostics.Debug.WriteLine("🔒 API access denied - license required");
        
        var title = _localizationService?.GetString("ApiLicenseRequired") ?? "API-Lizenz erforderlich";
        var message = _localizationService?.GetString("ApiLicenseRequiredMessage") ?? 
            "Die API-Funktionalität erfordert eine gültige Lizenz mit dem 'API Connection' Feature.\n\n" +
            "Die API-Dokumentation ist weiterhin frei verfügbar.\n" +
            "Bitte aktivieren Sie eine entsprechende Lizenz über das Lizenz-Menü um die API zu starten.";
        
        // Use new ApiErrorDialog instead of MessageBox
        ApiErrorDialog.ShowSimpleApiError(
            Application.Current.MainWindow,
            message,
            _localizationService);
    }

    /// <summary>
    /// Shows an API error using the modern error dialog
    /// </summary>
    private bool ShowApiError(string message, string? errorCode = null, Exception? exception = null)
    {
        if (exception != null)
        {
            return ApiErrorDialog.ShowExceptionApiError(
                Application.Current.MainWindow,
                message,
                exception,
                _localizationService);
        }
        else
        {
            return ApiErrorDialog.ShowSimpleApiError(
                Application.Current.MainWindow,
                message,
                _localizationService);
        }
    }

    public async Task<bool> StartApiAsync(TournamentData tournamentData, int port = 5000)
    {
        System.Diagnostics.Debug.WriteLine($"🚀 LicensedApiIntegrationService: StartApiAsync requested on port {port}");
        
        if (!HasApiLicense())
        {
            System.Diagnostics.Debug.WriteLine("❌ API start denied - no valid license");
            ShowApiLicenseRequired();
            return false;
        }

        System.Diagnostics.Debug.WriteLine("✅ API license verified - starting API...");
        
        try
        {
            return await _innerApiService.StartApiAsync(tournamentData, port);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ API start failed with exception: {ex.Message}");
            
            var errorMessage = _localizationService?.GetString("ApiStartError") ?? 
                "Fehler beim Starten der API. Überprüfen Sie, ob der Port bereits verwendet wird.";
            
            var retry = ShowApiError(errorMessage, "API_START_ERROR", ex);
            
            if (retry)
            {
                System.Diagnostics.Debug.WriteLine("🔄 User requested retry - attempting API start again");
                return await StartApiAsync(tournamentData, port);
            }
            
            return false;
        }
    }

    public async Task<bool> StopApiAsync()
    {
        System.Diagnostics.Debug.WriteLine("🛑 LicensedApiIntegrationService: StopApiAsync requested");
        
        try
        {
            return await _innerApiService.StopApiAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ API stop failed with exception: {ex.Message}");
            
            var errorMessage = _localizationService?.GetString("ApiStopError") ?? 
                "Fehler beim Stoppen der API.";
            
            ShowApiError(errorMessage, "API_STOP_ERROR", ex);
            return false;
        }
    }

    public async Task<bool> UpdateTournamentDataAsync(TournamentData tournamentData)
    {
        System.Diagnostics.Debug.WriteLine("📊 LicensedApiIntegrationService: UpdateTournamentDataAsync requested");
        
        if (!HasApiLicense())
        {
            System.Diagnostics.Debug.WriteLine("❌ API update denied - no valid license");
            return false;
        }

        try
        {
            return await _innerApiService.UpdateTournamentDataAsync(tournamentData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ API update failed with exception: {ex.Message}");
            
            var errorMessage = _localizationService?.GetString("ApiUpdateError") ?? 
                "Fehler beim Aktualisieren der Turnierdaten über die API.";
            
            var retry = ShowApiError(errorMessage, "API_UPDATE_ERROR", ex);
            
            if (retry)
            {
                System.Diagnostics.Debug.WriteLine("🔄 User requested retry - attempting API update again");
                return await UpdateTournamentDataAsync(tournamentData);
            }
            
            return false;
        }
    }

    public void SetCurrentTournamentId(string tournamentId)
    {
        System.Diagnostics.Debug.WriteLine($"🎯 LicensedApiIntegrationService: SetCurrentTournamentId to {tournamentId}");
        _innerApiService.SetCurrentTournamentId(tournamentId);
    }

    public void OpenApiDocumentation()
    {
        System.Diagnostics.Debug.WriteLine("📚 LicensedApiIntegrationService: OpenApiDocumentation requested");
        
        // NEU: API Dokumentation ist IMMER verfügbar - keine Lizenzprüfung nötig
        System.Diagnostics.Debug.WriteLine("✅ API documentation access granted - no license required");

        if (IsApiRunning && ApiUrl != null)
        {
            System.Diagnostics.Debug.WriteLine($"✅ Opening API documentation at {ApiUrl}");
            _innerApiService.OpenApiDocumentation();
        }
        else
        {
            // NEU: Auch ohne laufende API - öffne statische API Dokumentation URL
            System.Diagnostics.Debug.WriteLine("ℹ️ API is not running - opening static API documentation");
            
            var staticApiUrl = "http://localhost:5000"; // Standard API URL
            
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = staticApiUrl,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
                
                System.Diagnostics.Debug.WriteLine($"✅ Static API documentation opened at {staticApiUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error opening static API documentation: {ex.Message}");
                
                var title = _localizationService?.GetString("Information") ?? "Information";
                var message = _localizationService?.GetString("ApiDocumentationInfo") ?? 
                    "API Dokumentation:\n\n" +
                    "Die API-Dokumentation ist verfügbar wenn die API läuft.\n" +
                    "Standard URL: http://localhost:5000\n\n" +
                    "Um die API zu starten, verwenden Sie das API-Menü.";
                
                // Use modern dialog for information message too
                ApiErrorDialog.ShowSimpleApiError(
                    Application.Current.MainWindow,
                    message,
                    _localizationService);
            }
        }
    }

    public void Dispose()
    {
        _innerApiService?.Dispose();
    }
}