using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Helpers;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Vereinfachte Lizenz-Aktivierung Dialog mit modernen UI-Komponenten
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
        return await ShowDialogAsync(null);
    }
    
    public async Task<bool> ShowDialogAsync(Window? owner)
    {
        try
        {
            // Verwende die modernen Dialog-Klassen falls verfügbar
            try 
            {
                var parentWindow = owner ?? Application.Current.MainWindow;
                if (parentWindow != null)
                {
                    return await LicenseActivationDialog.ShowDialogAsync(parentWindow, _localizationService, _licenseManager);
                }
                else
                {
                    // Kein Parent Window verfügbar, verwende Legacy-Implementierung
                    return await ShowLegacyDialogAsync(null);
                }
            }
            catch (Exception dialogEx)
            {
                System.Diagnostics.Debug.WriteLine($"Modern dialog failed: {dialogEx.Message}");
                // Fallback zu Legacy-Implementierung bei Fehlern mit dem modernen Dialog
                return await ShowLegacyDialogAsync(owner);
            }
        }
        catch (Exception ex)
        {
            Helpers.TournamentDialogHelper.ShowError($"Fehler beim Aktivieren der Lizenz: {ex.Message}", 
                "Fehler", _localizationService, owner);
            return false;
        }
    }

    /// <summary>
    /// Einfachster Fallback mit BasicInputBox
    /// </summary>
    private async Task<bool> ShowBasicInputDialogAsync(Window? owner)
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
                    Helpers.TournamentDialogHelper.ShowError("Ungültiges Lizenzschlüssel-Format.\n\nErwartet: BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3\n(8 Blöcke mit je 4 Hex-Zeichen)", 
                        "Fehler", _localizationService, owner);
                    return false;
                }
                
                // Server-Validierung und Aktivierung
                var result = await _licenseManager.ActivateLicenseAsync(normalizedKey);
                
                if (result.IsValid)
                {
                    ShowSuccessMessage(result);
                    return true;
                }
                else
                {
                    var errorMessage = GetUserFriendlyErrorMessage(result);
                    Helpers.TournamentDialogHelper.ShowError(errorMessage, "Aktivierung fehlgeschlagen", _localizationService, owner);
                    return false;
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            Helpers.TournamentDialogHelper.ShowError($"Fehler beim Aktivieren der Lizenz: {ex.Message}", 
                "Fehler", _localizationService, owner);
            return false;
        }
    }

    private void ShowSuccessMessage(Models.License.LicenseValidationResult result)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🎉 Attempting to show success dialog...");
            
            // Versuche den modernen Success-Dialog zu verwenden
            if (Application.Current.MainWindow != null)
            {
                System.Diagnostics.Debug.WriteLine("🎯 Using modern success dialog...");
                LicenseActivationSuccessDialog.ShowDialog(Application.Current.MainWindow, _localizationService, result);
                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Modern dialog failed: {ex.Message}");
            
            // Fallback zum einfachen Dialog
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Trying simple success dialog fallback...");
                SimpleLicenseActivationSuccessDialog.ShowDialog(Application.Current.MainWindow, _localizationService, result);
                return;
            }
            catch (Exception simpleEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Simple dialog also failed: {simpleEx.Message}");
            }
        }

        // Final Fallback zu einfacher MessageBox
        System.Diagnostics.Debug.WriteLine("🔄 Using MessageBox as final fallback...");
        var message = BuildSuccessMessage(result);
        TournamentDialogHelper.ShowInformation(message,
            _localizationService.GetString("LicenseActivatedSuccessfully") ?? "Lizenz erfolgreich aktiviert!",
            _localizationService,
            Application.Current?.MainWindow);
    }
    
    /// <summary>
    /// Fallback-Implementierung mit einfachen MessageBoxes (Legacy)
    /// </summary>
    private async Task<bool> ShowLegacyDialogAsync(Window? owner)
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
                    TournamentDialogHelper.ShowError("Ungültiges Lizenzschlüssel-Format.\n\nErwartet: BDF6-192D-E8BE-4178-B160-C6C3-6018-0FE3\n(8 Blöcke mit je 4 Hex-Zeichen)",
                        "Fehler", _localizationService, owner);
                    return false;
                }
                
                // Server-Validierung und Aktivierung
                var progressWindow = new LicenseActivationProgressWindow(_localizationService);
                if (owner != null)
                    progressWindow.Owner = owner;
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
                            

                            TournamentDialogHelper.ShowWarning(warningMessage, warningTitle, _localizationService, owner);
                        }
                        TournamentDialogHelper.ShowInformation(successMessage, successTitle, _localizationService, owner);
                        return true;
                    }
                    else
                    {
                        var errorMessage = GetUserFriendlyErrorMessage(result);
                        // Use modern dialog helper instead of MessageBox for consistent design
                        Helpers.TournamentDialogHelper.ShowError(errorMessage, "Aktivierung fehlgeschlagen", _localizationService, owner);
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
            Helpers.TournamentDialogHelper.ShowError($"Fehler beim Aktivieren der Lizenz: {ex.Message}", 
                "Fehler", _localizationService, owner);
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
            "hub_integration" => "Hub-Integrationen",
            _ => featureId // Für unbekannte Features einfach die ID zurückgeben
        };
    }
    
    private static bool IsValidLicenseKeyFormat(string licenseKey)
    {
        // Einfachste Validierung: 8 Blöcke mit je 4 Hex-Zeichen
        var blocks = licenseKey.Split('-');
        if (blocks.Length != 8) return false;
        
        foreach (var block in blocks)
        {
            if (block.Length != 4 || !IsHexadecimal(block))
                return false;
        }
        
        return true;
    }
    
    private static bool IsHexadecimal(string input)
    {
        foreach (char c in input)
        {
            if (!IsHexDigit(c))
                return false;
        }
        return true;
    }
    
    private static bool IsHexDigit(char c)
    {
        return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
    }
}

/// <summary>
/// Einfaches Progress-Fenster für Lizenz-Aktivierung (Legacy Fallback)
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
        Height = 180;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Background = (System.Windows.Media.Brush)Application.Current.Resources["BackgroundBrush"];
        Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextBrush"];
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;

        var shell = new Border
        {
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            BorderBrush = (System.Windows.Media.Brush)Application.Current.Resources["BorderBrush"],
            Background = (System.Windows.Media.Brush)Application.Current.Resources["SurfaceBrush"],
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Black,
                BlurRadius = 16,
                Direction = 270,
                ShadowDepth = 6,
                Opacity = 0.15
            }
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new Border
        {
            Background = (System.Windows.Media.Brush)Application.Current.Resources["DialogPrimaryGradient"],
            CornerRadius = new CornerRadius(12, 12, 0, 0),
            Padding = new Thickness(16, 12, 16, 12)
        };
        var headerText = new System.Windows.Controls.TextBlock
        {
            Text = _localizationService.GetString("ActivatingLicense") ?? "Lizenz wird aktiviert...",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextBrush"]
        };
        header.Child = headerText;
        grid.Children.Add(header);

        var stackPanel = new System.Windows.Controls.StackPanel
        {
            Margin = new Thickness(20),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(stackPanel, 1);

        var progressBar = new System.Windows.Controls.ProgressBar
        {
            Height = 20,
            IsIndeterminate = true,
            Margin = new Thickness(0, 10, 0, 15)
        };

        var textBlock = new System.Windows.Controls.TextBlock
        {
            Text = _localizationService.GetString("ValidatingLicense") ?? "Lizenz wird validiert...",
            HorizontalAlignment = HorizontalAlignment.Center,
            FontSize = 14,
            Foreground = (System.Windows.Media.Brush)Application.Current.Resources["TextBrush"]
        };

        stackPanel.Children.Add(progressBar);
        stackPanel.Children.Add(textBlock);

        grid.Children.Add(stackPanel);
        shell.Child = grid;
        Content = shell;
    }
}