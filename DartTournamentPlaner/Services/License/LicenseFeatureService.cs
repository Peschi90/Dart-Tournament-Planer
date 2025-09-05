using System;
using System.Linq;
using System.Threading.Tasks;
using DartTournamentPlaner.Models.License;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Services.License;

/// <summary>
/// Service f�r Feature-Management basierend auf Lizenzierung
/// Alle bestehenden Funktionen bleiben verf�gbar!
/// </summary>
public class LicenseFeatureService
{
    private readonly Services.License.LicenseManager _licenseManager;
    private LicenseStatus? _currentStatus;
    
    public event EventHandler<LicenseStatus>? LicenseStatusChanged;
    
    public LicenseFeatureService(Services.License.LicenseManager licenseManager)
    {
        _licenseManager = licenseManager;
        _licenseManager.LicenseStatusChanged += OnLicenseStatusChanged;
    }
    
    /// <summary>
    /// Aktueller Lizenz-Status
    /// </summary>
    public LicenseStatus CurrentStatus => _currentStatus ?? CreateUnlicensedStatus();
    
    /// <summary>
    /// Initialisiert den Service und l�dt den aktuellen Status
    /// ? ERWEITERT: F�hrt echte Server-Validierung durch f�r kompletten Feature-Abgleich
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"?? LicenseFeatureService: Starting initialization with server validation...");
            
            // ? KRITISCH: Erzwinge Server-Validierung f�r aktuelle Features
            // Dies �berschreibt die Cached-Logik im LicenseManager
            var validationResult = await ValidateLicenseWithServerRefreshAsync();
            
            // Status aktualisieren
            UpdateStatus(validationResult);
            
            System.Diagnostics.Debug.WriteLine($"? LicenseFeatureService: Initialization completed. " +
                $"Licensed: {_currentStatus?.IsLicensed}, Features: {_currentStatus?.ActiveFeatures.Count ?? 0}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? LicenseFeatureService: Initialization failed: {ex.Message}");
            
            // Fallback auf gecachte Daten
            try
            {
                var fallbackResult = await _licenseManager.ValidateLicenseAsync();
                UpdateStatus(fallbackResult);
            }
            catch
            {
                // Letzter Fallback: Unlizenziert
                _currentStatus = CreateUnlicensedStatus();
            }
        }
    }
    
    /// <summary>
    /// ? NEU: Erzwingt Server-Validierung durch direkten Aufruf (umgeht Cache)
    /// Diese Methode stellt sicher, dass bei einem Refresh echte Server-Kommunikation stattfindet
    /// </summary>
    private async Task<LicenseValidationResult> ValidateLicenseWithServerRefreshAsync()
    {
        System.Diagnostics.Debug.WriteLine($"?? LicenseFeatureService: Forcing server validation...");
        
        // ? VERBESSERT: Verwende die neue ForceServerValidationAsync Methode
        var result = await _licenseManager.ForceServerValidationAsync();
        
        System.Diagnostics.Debug.WriteLine($"?? Server validation result: IsValid={result.IsValid}, " +
            $"Offline={result.Offline}, Features={result.Data?.Features?.Length ?? 0}");
        
        return result;
    }
    
    /// <summary>
    /// Pr�ft ob ein Feature verf�gbar ist
    /// WICHTIG: Alle Features sind immer verf�gbar! Dies ist nur f�r zuk�nftige Premium-Features
    /// </summary>
    public bool IsFeatureEnabled(string feature)
    {
        // ALLE BESTEHENDEN FUNKTIONEN BLEIBEN VERF�GBAR!
        // Diese Methode ist f�r zuk�nftige Premium-Features reserviert
        
        // Core-Features sind immer verf�gbar
        if (IsCoreFeature(feature))
            return true;
        
        // Premium-Features ben�tigen g�ltige Lizenz
        if (!_currentStatus?.IsValid == true)
            return false;
        
        return _currentStatus.ActiveFeatures.Contains(feature);
    }
    
    /// <summary>
    /// Alias f�r IsFeatureEnabled - pr�ft ob ein Feature verf�gbar ist
    /// </summary>
    public bool HasFeature(string feature)
    {
        return IsFeatureEnabled(feature);
    }
    
    /// <summary>
    /// Bestimmt ob ein Feature ein Core-Feature ist (immer verf�gbar)
    /// </summary>
    private static bool IsCoreFeature(string feature)
    {
        // ALLE AKTUELLEN FEATURES SIND CORE-FEATURES!
        return feature switch
        {
            LicenseFeatures.TOURNAMENT_MANAGEMENT => true,
            LicenseFeatures.PLAYER_TRACKING => true,
            LicenseFeatures.STATISTICS => false,  // Premium-Feature
            LicenseFeatures.API_ACCESS => false,  // Deprecated - Premium-Feature
            LicenseFeatures.API_CONNECTION => true,  // Core-Feature (API Start/Stop)
            LicenseFeatures.HUB_INTEGRATION => true,  // Deprecated - Core-Feature
            LicenseFeatures.HUB_CONNECTION => false,  // NEU: Premium-Feature
            LicenseFeatures.MULTI_TOURNAMENT => true,
            LicenseFeatures.ENHANCED_PRINTING => false,  // Premium-Feature
            LicenseFeatures.TOURNAMENT_OVERVIEW => false,  // NEU: Premium-Feature
            _ => false // Nur zuk�nftige Features k�nnen Premium sein
        };
    }
    
    /// <summary>
    /// Zeigt eine Feature-nicht-verf�gbar-Meldung (f�r zuk�nftige Premium-Features)
    /// </summary>
    public void ShowFeatureNotAvailableMessage(string featureName)
    {
        // Aktuell nicht verwendet, da alle Features verf�gbar sind
        var message = $"Das Feature '{featureName}' erfordert eine g�ltige Lizenz.";
        System.Windows.MessageBox.Show(message, "Premium-Feature", 
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }
    
    /// <summary>
    /// Event-Handler f�r Lizenz-Status-�nderungen
    /// </summary>
    private void OnLicenseStatusChanged(object? sender, LicenseStatusChangedEventArgs e)
    {
        UpdateStatus(e.ValidationResult);
    }
    
    /// <summary>
    /// Aktualisiert den internen Status
    /// </summary>
    private void UpdateStatus(LicenseValidationResult validationResult)
    {
        _currentStatus = CreateStatusFromValidationResult(validationResult);
        LicenseStatusChanged?.Invoke(this, _currentStatus);
    }
    
    /// <summary>
    /// Erstellt LicenseStatus aus ValidationResult
    /// </summary>
    private static LicenseStatus CreateStatusFromValidationResult(LicenseValidationResult result)
    {
        var status = new LicenseStatus
        {
            IsLicensed = result.IsValid,
            IsValid = result.IsValid,
            IsOffline = result.Offline,
            StatusMessage = result.Message
        };
        
        if (result.Data != null)
        {
            status.CustomerName = result.Data.CustomerName;
            status.ProductName = result.Data.ProductName;
            status.ExpiresAt = result.Data.ExpiresAt;
            status.RemainingActivations = result.Data.RemainingActivations;
            status.IsExpired = result.Data.ExpiresAt < DateTime.Now;
            
            if (result.Data.Features != null)
            {
                status.ActiveFeatures.AddRange(result.Data.Features);
            }
        }
        
        return status;
    }
    
    /// <summary>
    /// Erstellt Status f�r unlizenzierte Version
    /// </summary>
    private static LicenseStatus CreateUnlicensedStatus()
    {
        return new LicenseStatus
        {
            IsLicensed = false,
            IsValid = false,
            StatusMessage = "Unlicensed version - all core features available"
        };
    }
    
    /// <summary>
    /// Holt verf�gbare Features als benutzerfreundliche Namen
    /// </summary>
    public string[] GetAvailableFeatureNames()
    {
        if (_currentStatus?.ActiveFeatures == null || _currentStatus.ActiveFeatures.Count == 0)
        {
            return new[] { "Alle Core-Features verf�gbar" };
        }
        
        return _currentStatus.ActiveFeatures
            .Select(GetFeatureDisplayName)
            .ToArray();
    }
    
    /// <summary>
    /// Konvertiert Feature-ID zu Anzeigename
    /// </summary>
    private static string GetFeatureDisplayName(string featureId)
    {
        return featureId switch
        {
            LicenseFeatures.TOURNAMENT_MANAGEMENT => "Turnier-Management",
            LicenseFeatures.PLAYER_TRACKING => "Spieler-Verfolgung",
            LicenseFeatures.STATISTICS => "Advanced Statistics",
            LicenseFeatures.API_ACCESS => "API-Zugang (Legacy)",  // Deprecated
            LicenseFeatures.API_CONNECTION => "API-Verbindung",
            LicenseFeatures.HUB_INTEGRATION => "Hub-Integration (Legacy)",  // Deprecated
            LicenseFeatures.HUB_CONNECTION => "Hub-Verbindung",  // NEU
            LicenseFeatures.MULTI_TOURNAMENT => "Multi-Turnier",
            LicenseFeatures.ADVANCED_REPORTING => "Erweiterte Berichte",
            LicenseFeatures.ENHANCED_PRINTING => "Enhanced Printing",
            LicenseFeatures.CUSTOM_THEMES => "Benutzerdefinierte Themes",
            LicenseFeatures.PREMIUM_SUPPORT => "Premium-Support",
            LicenseFeatures.TOURNAMENT_OVERVIEW => "Tournament Overview",  // NEU
            _ => featureId
        };
    }
}