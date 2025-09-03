using System;
using System.Threading.Tasks;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Vereinfachte Lizenz-Aktivierung Dialog mit Server-Integration
/// </summary>
public class SimpleLicenseActivationDialog
{
    private readonly LocalizationService _localizationService;
    private readonly Services.License.LicenseManager _licenseManager;
    
    public SimpleLicenseActivationDialog(LocalizationService localizationService, Services.License.LicenseManager licenseManager)
    {
        _localizationService = localizationService;
        _licenseManager = licenseManager;
    }
    
    public async Task<bool> ShowDialogAsync()
    {
        try
        {
            var title = _localizationService.GetString("ActivateLicense") ?? "Lizenz aktivieren";
            var message = _localizationService.GetString("LicenseActivationMessage") ?? 
                "Geben Sie Ihren Lizenzschlüssel ein:\n\nFormat: BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3";
            
            // Hardware-ID anzeigen für Support-Zwecke
            var hardwareId = Services.License.LicenseManager.GenerateHardwareId();
            var fullMessage = $"{message}\n\nIhre Hardware-ID (für Support): {hardwareId}";
            
            var licenseKey = Microsoft.VisualBasic.Interaction.InputBox(
                fullMessage, title, ""
            );
            
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                // Normalisiere den Lizenzschlüssel
                var normalizedKey = licenseKey.Trim().ToUpperInvariant();
                
                // Format-Validierung
                if (!IsValidLicenseKeyFormat(normalizedKey))
                {
                    MessageBox.Show("Ungültiges Lizenzschlüssel-Format.\n\nErwartet: BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3\n(8 Blöcke mit je 4 Hex-Zeichen)", 
                        "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                
                // Server-Validierung und Aktivierung
                var progressWindow = new LicenseActivationProgressWindow(_localizationService);
                progressWindow.Show();
                
                try
                {
                    var result = await _licenseManager.ActivateLicenseAsync(normalizedKey);
                    progressWindow.Close();
                    
                    if (result.IsValid)
                    {
                        var successTitle = _localizationService.GetString("Success") ?? "Erfolgreich";
                        var successMessage = BuildSuccessMessage(result);
                        
                        // Warnung bei wenigen Aktivierungen
                        if (result.ShowActivationWarning)
                        {
                            var warningTitle = _localizationService.GetString("Warning") ?? "Warnung";
                            var warningMessage = result.Data?.RemainingActivations == 0 ?
                                "Dies ist Ihre letzte verfügbare Aktivierung für diese Lizenz. Kontaktieren Sie den Support, falls Sie die Software auf zusätzlichen Computern installieren müssen." :
                                $"Sie haben noch {result.Data?.RemainingActivations} Aktivierung(en) für diese Lizenz übrig.";
                                                    }
                        
                        MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                        return true;
                    }
                    else
                    {
                        var errorMessage = GetUserFriendlyErrorMessage(result);
                        MessageBox.Show(errorMessage, "Aktivierung fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                finally
                {
                    progressWindow?.Close();
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Aktivieren der Lizenz: {ex.Message}", 
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
    
    private string BuildSuccessMessage(Models.License.LicenseValidationResult result)
    {
        var message = "✅ Lizenz erfolgreich aktiviert!\n\n";
        
        if (result.Data != null)
        {
            message += $"📄 Kunde: {result.Data.CustomerName}\n";
            message += $"📦 Produkt: {result.Data.ProductName}\n";
            
            if (result.Data.ExpiresAt.HasValue)
            {
                message += $"📅 Gültig bis: {result.Data.ExpiresAt.Value:dd.MM.yyyy HH:mm}\n";
            }
            else
            {
                message += "📅 Gültig bis: Unbegrenzt\n";
            }
            
            if (result.Data.RemainingActivations.HasValue)
            {
                message += $"🔄 Verbleibende Aktivierungen: {result.Data.RemainingActivations}\n";
            }
            
            if (result.Data.Features != null && result.Data.Features.Length > 0)
            {
                message += "\n🚀 Aktivierte Features:\n";
                foreach (var feature in result.Data.Features)
                {
                    message += $"  • {GetFeatureDisplayName(feature)}\n";
                }
            }
        }
        
        return message;
    }
    
    private string GetUserFriendlyErrorMessage(Models.License.LicenseValidationResult result)
    {
        return result.ErrorType switch
        {
            Models.License.LicenseErrorType.LicenseNotFound => 
                "❌ Lizenzschlüssel nicht gefunden oder ungültig.\n\nBitte überprüfen Sie Ihren Lizenzschlüssel.",
            
            Models.License.LicenseErrorType.LicenseExpired => 
                "⏰ Diese Lizenz ist abgelaufen.\n\nBitte kontaktieren Sie den Support für eine Erneuerung.",
            
            Models.License.LicenseErrorType.LicenseInactive => 
                "🚫 Diese Lizenz ist inaktiv.\n\nBitte kontaktieren Sie den Support.",
            
            Models.License.LicenseErrorType.MaxActivationsReached => 
                "🔒 Das Aktivierungslimit für diese Lizenz wurde erreicht.\n\nBitte kontaktieren Sie den Support, um das Limit zurückzusetzen.",
            
            Models.License.LicenseErrorType.InvalidFormat => 
                "📝 Ungültiges Lizenzschlüssel-Format.\n\nBitte überprüfen Sie die Eingabe.",
            
            Models.License.LicenseErrorType.NetworkError => 
                "🌐 Netzwerkfehler beim Kontaktieren des Lizenzservers.\n\nBitte überprüfen Sie Ihre Internetverbindung und versuchen Sie es erneut.",
            
            Models.License.LicenseErrorType.ServerError => 
                "⚠️ Server-Fehler beim Validieren der Lizenz.\n\nBitte versuchen Sie es später erneut.",
            
            _ => result.Message ?? "❌ Unbekannter Fehler bei der Lizenzaktivierung."
        };
    }
    
    private static string GetFeatureDisplayName(string featureId)
    {
        return featureId switch
        {
            "tournament_management" => "Turnier-Management",
            "player_tracking" => "Spieler-Verfolgung",
            "statistics" => "Statistiken",
            "api_access" => "API-Zugang",
            "hub_integration" => "Hub-Integration",
            "multi_tournament" => "Multi-Turnier",
            "advanced_reporting" => "Erweiterte Berichte",
            "custom_themes" => "Benutzerdefinierte Themes",
            "premium_support" => "Premium-Support",
            _ => featureId
        };
    }
    
    private static bool IsValidLicenseKeyFormat(string licenseKey)
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        // Entferne nur Leerzeichen für die Validierung
        var cleaned = licenseKey.Replace(" ", "").ToUpperInvariant();
        
        // Format 1: Mit Bindestrichen - BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3
        var patternWithDashes = @"^[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}$";
        if (System.Text.RegularExpressions.Regex.IsMatch(cleaned, patternWithDashes))
        {
            return true;
        }
        
        // Format 2: Ohne Bindestriche - BDF6192DE8BE4178B160C6C360180FE3 (32 Zeichen)
        if (cleaned.Length == 32)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^[A-F0-9]{32}$");
        }
        
        return false;
    }
}

/// <summary>
/// Einfaches Progress-Fenster für Lizenz-Aktivierung
/// </summary>
public class LicenseActivationProgressWindow : Window
{
    private readonly LocalizationService _localizationService;
    
    public LicenseActivationProgressWindow(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        InitializeWindow();
    }
    
    private void InitializeWindow()
    {
        Title = _localizationService.GetString("ActivatingLicense") ?? "Lizenz wird aktiviert...";
        Width = 400;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        
        var stackPanel = new System.Windows.Controls.StackPanel
        {
            Margin = new Thickness(20),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        var progressBar = new System.Windows.Controls.ProgressBar
        {
            Height = 20,
            IsIndeterminate = true,
            Margin = new Thickness(0, 0, 0, 15)
        };
        
        var textBlock = new System.Windows.Controls.TextBlock
        {
            Text = _localizationService.GetString("ValidatingLicense") ?? "Lizenz wird validiert...",
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 14
        };
        
        stackPanel.Children.Add(progressBar);
        stackPanel.Children.Add(textBlock);
        
        Content = stackPanel;
    }
}