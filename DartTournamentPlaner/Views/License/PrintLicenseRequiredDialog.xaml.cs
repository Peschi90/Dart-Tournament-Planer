using System;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Dialog der angezeigt wird, wenn Print-Funktionalität ohne entsprechende Lizenz aufgerufen wird
/// </summary>
public partial class PrintLicenseRequiredDialog : Window
{
    private readonly LocalizationService _localizationService;
    private readonly LicenseManager _licenseManager;

    public PrintLicenseRequiredDialog(LocalizationService localizationService, LicenseManager licenseManager)
    {
        _localizationService = localizationService;
        _licenseManager = licenseManager;
        
        InitializeComponent();
        InitializeWindow();
    }

    private void InitializeWindow()
    {
        Title = _localizationService.GetString("PrintLicenseRequired") ?? "Drucklizenz erforderlich";
        
        // Übersetzungen anwenden
        TitleText.Text = _localizationService.GetString("PrintLicenseRequiredTitle") ?? "Drucklizenz erforderlich";
        MessageText.Text = _localizationService.GetString("PrintLicenseRequiredMessage") ?? 
            "Die Druckfunktionalität erfordert eine gültige Lizenz mit dem 'Enhanced Printing' Feature.";

        BenefitsTitle.Text = _localizationService.GetString("EnhancedPrintingBenefitsTitle") ?? "🎯 Enhanced Printing beinhaltet:";
        
        var benefitsText = _localizationService.GetString("EnhancedPrintingBenefits") ??
            "• Professionelle Turnierberichte\n" +
            "• Spielergebnisse und Turnierbäume\n" +
            "• Export zu PDF Funktionalität";
        
        BenefitsList.Text = benefitsText;

        ActionText.Text = _localizationService.GetString("PrintLicenseActionText") ?? 
            "Möchten Sie eine Lizenz mit dem Enhanced Printing Feature anfordern?";
        
        // Buttons
        CancelButton.Content = _localizationService.GetString("Cancel") ?? "Abbrechen";
        RequestLicenseButton.Content = "🛒 " + (_localizationService.GetString("RequestLicense") ?? "Lizenz anfordern");
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void RequestLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Schließe aktuellen Dialog
            DialogResult = true;
            Close();

            // Öffne Purchase License Dialog
            var purchaseDialog = new PurchaseLicenseDialog(_localizationService, _licenseManager);
            if (Owner != null)
            {
                purchaseDialog.Owner = Owner;
            }

            // Pre-select Enhanced Printing Feature
            purchaseDialog.Loaded += (s, args) =>
            {
                try
                {
                    // Fokussiere auf das Print Feature
                    purchaseDialog.FocusOnPrintFeature();
                    System.Diagnostics.Debug.WriteLine("✅ Print feature pre-selected in Purchase Dialog");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error pre-selecting print feature: {ex.Message}");
                }
            };

            purchaseDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Lizenzanfrage-Dialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Statische Methode um den Dialog anzuzeigen
    /// </summary>
    /// <param name="owner">Parent Window</param>
    /// <param name="localizationService">Lokalisierungsservice</param>
    /// <param name="licenseManager">Lizenzmanager</param>
    /// <returns>True wenn Benutzer Lizenz anfordern möchte</returns>
    public static bool ShowDialog(Window? owner, LocalizationService localizationService, LicenseManager licenseManager)
    {
        var dialog = new PrintLicenseRequiredDialog(localizationService, licenseManager)
        {
            Owner = owner
        };

        return dialog.ShowDialog() == true;
    }
}