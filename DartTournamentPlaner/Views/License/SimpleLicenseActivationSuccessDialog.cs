using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DartTournamentPlaner.Helpers;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Vereinfachter Erfolg-Dialog für die Lizenzaktivierung
/// Fallback-Version ohne komplexe Styling-Features
/// </summary>
public class SimpleLicenseActivationSuccessDialog : Window
{
    private readonly LocalizationService _localizationService;
    private readonly Models.License.LicenseValidationResult _result;

    public SimpleLicenseActivationSuccessDialog(LocalizationService localizationService, Models.License.LicenseValidationResult result)
    {
        _localizationService = localizationService;
        _result = result;
        
        InitializeSimpleDialog();
        LoadLicenseData();
    }

    private void InitializeSimpleDialog()
    {
        // Basic Window Properties
        Title = _localizationService.GetString("LicenseActivatedSuccessfully") ?? "Lizenz erfolgreich aktiviert!";
        Width = 500;
        Height = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Background = Brushes.White;
        
        var mainPanel = new StackPanel
        {
            Margin = new Thickness(20),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        // Success Icon and Title
        var titlePanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        
        var successIcon = new TextBlock
        {
            Text = "??",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        titlePanel.Children.Add(successIcon);
        
        var titleText = new TextBlock
        {
            Text = _localizationService.GetString("LicenseActivatedSuccessfully") ?? "Lizenz erfolgreich aktiviert!",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 5)
        };
        titlePanel.Children.Add(titleText);
        
        var subtitleText = new TextBlock
        {
            Text = _localizationService.GetString("LicenseActivationSuccessSubtitle") ?? "Alle Premium-Features sind jetzt verfügbar",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray,
            TextWrapping = TextWrapping.Wrap
        };
        titlePanel.Children.Add(subtitleText);
        
        mainPanel.Children.Add(titlePanel);
        
        // License Information
        if (_result?.Data != null)
        {
            var infoPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 20)
            };
            
            var infoText = new TextBlock
            {
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18
            };
            
            var infoBuilder = new System.Text.StringBuilder();
            infoBuilder.AppendLine($"?? Kunde: {_result.Data.CustomerName ?? "Unbekannt"}");
            infoBuilder.AppendLine($"?? Produkt: {_result.Data.ProductName ?? "Dart Tournament Planner"}");
            
            if (_result.Data.ExpiresAt.HasValue)
            {
                infoBuilder.AppendLine($"?? Gültig bis: {_result.Data.ExpiresAt.Value:dd.MM.yyyy HH:mm}");
            }
            else
            {
                infoBuilder.AppendLine("?? Gültig bis: Unbegrenzt");
            }
            
            if (_result.Data.RemainingActivations.HasValue)
            {
                infoBuilder.AppendLine($"?? Verbleibende Aktivierungen: {_result.Data.RemainingActivations}");
            }
            
            if (_result.Data.Features != null && _result.Data.Features.Length > 0)
            {
                infoBuilder.AppendLine("\n?? Aktivierte Features:");
                foreach (var feature in _result.Data.Features)
                {
                    infoBuilder.AppendLine($"  • {GetFeatureDisplayName(feature)}");
                }
            }
            
            infoText.Text = infoBuilder.ToString();
            infoPanel.Children.Add(infoText);
            
            // Warning for few remaining activations
            if (_result.Data.RemainingActivations.HasValue && _result.Data.RemainingActivations.Value <= 2)
            {
                var warningText = new TextBlock
                {
                    Text = _result.Data.RemainingActivations.Value == 0 ?
                        "?? Dies war Ihre letzte verfügbare Aktivierung für diese Lizenz." :
                        $"?? Sie haben nur noch {_result.Data.RemainingActivations.Value} Aktivierung(en) übrig.",
                    FontSize = 11,
                    Foreground = Brushes.Orange,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                infoPanel.Children.Add(warningText);
            }
            
            mainPanel.Children.Add(infoPanel);
        }
        
        // Continue Button
        var continueButton = new Button
        {
            Content = _localizationService.GetString("Continue") ?? "Weiter",
            Width = 120,
            Height = 35,
            FontSize = 14,
            FontWeight = FontWeights.Medium,
            Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(0, 20, 0, 0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        continueButton.Click += ContinueButton_Click;
        
        mainPanel.Children.Add(continueButton);
        
        Content = mainPanel;
    }

    private void LoadLicenseData()
    {
        // Data is already loaded in InitializeSimpleDialog()
        // This method is kept for consistency with the main dialog
    }

    private static string GetFeatureDisplayName(string featureId)
    {
        return featureId switch
        {
            "tournament_management" => "Turnier-Management",
            "player_tracking" => "Spieler-Verfolgung", 
            "statistics" => "Erweiterte Statistiken",
            "api_access" => "API-Zugang",
            "hub_integration" => "Hub-Integration",
            "enhanced_printing" => "Erweiterte Druckfunktionen",
            "multi_tournament" => "Multi-Turnier Support",
            "advanced_reporting" => "Erweiterte Berichte",
            "custom_themes" => "Benutzerdefinierte Themes",
            "premium_support" => "Premium-Support",
            _ => featureId
        };
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DialogResult = true;
        }
        catch (InvalidOperationException)
        {
            System.Diagnostics.Debug.WriteLine("Could not set DialogResult in simple dialog - closing normally");
        }
        
        Close();
    }

    /// <summary>
    /// Zeigt den einfachen Dialog als Modal Window an
    /// </summary>
    public static void ShowDialog(Window owner, LocalizationService localizationService, Models.License.LicenseValidationResult result)
    {
        try
        {
            var dialog = new SimpleLicenseActivationSuccessDialog(localizationService, result);
            
            if (owner != null && owner.IsLoaded)
            {
                dialog.Owner = owner;
            }
            
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing simple license success dialog: {ex.Message}");
            
            // Final fallback zu MessageBox
            TournamentDialogHelper.ShowInformation(
                "Lizenz erfolgreich aktiviert!\n\nAlle Premium-Features sind jetzt verfuegbar.",
                "Erfolgreich",
                localizationService,
                owner);
        }
    }
}