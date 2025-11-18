using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Dialog der angezeigt wird wenn PowerScoring ohne gültige Lizenz verwendet werden soll
/// </summary>
public partial class PowerScoringLicenseRequiredDialog : Window
{
    private readonly LocalizationService _localizationService;
    private readonly LicenseManager _licenseManager;

    public bool RequestedLicense { get; private set; }

    public PowerScoringLicenseRequiredDialog(
        LocalizationService localizationService,
        LicenseManager licenseManager)
    {
        InitializeComponent();
        
        _localizationService = localizationService;
        _licenseManager = licenseManager;
        
        UpdateTranslations();
    }

    private void UpdateTranslations()
    {
        // Title
        Title = _localizationService.GetString("PowerScoringLicenseRequired_Title") 
            ?? "PowerScoring License Required";
        TitleText.Text = Title;

        // Main Message
        MessageText.Text = _localizationService.GetString("PowerScoringLicenseRequired_Message") 
            ?? "PowerScoring is a premium feature that helps you organize players based on their skill level.";

        // Benefits Title
        BenefitsTitle.Text = _localizationService.GetString("PowerScoringLicenseRequired_BenefitsTitle") 
            ?? "PowerScoring includes:";

        // Benefits List
        var benefit1 = _localizationService.GetString("PowerScoringLicenseRequired_Benefit1") 
            ?? "- Systematic player score capture";
        var benefit2 = _localizationService.GetString("PowerScoringLicenseRequired_Benefit2") 
            ?? "- Automatic ranking creation";
        var benefit3 = _localizationService.GetString("PowerScoringLicenseRequired_Benefit3") 
            ?? "- Optimal group distribution based on skill level";
        var benefit4 = _localizationService.GetString("PowerScoringLicenseRequired_Benefit4") 
            ?? "- Flexible scoring rules (1x3, 8x3, 10x3, 15x3 throws)";
        var benefit5 = _localizationService.GetString("PowerScoringLicenseRequired_Benefit5") 
            ?? "- Snake-draft group assignment";

        BenefitsList.Text = $"{benefit1}\n{benefit2}\n{benefit3}\n{benefit4}\n{benefit5}";

        // Action Text
        ActionText.Text = _localizationService.GetString("PowerScoringLicenseRequired_ActionText") 
            ?? "Would you like to request a license with the PowerScoring feature?";

        // Buttons
        CancelButton.Content = _localizationService.GetString("Cancel") ?? "Cancel";
        RequestLicenseButton.Content = _localizationService.GetString("RequestLicense") ?? "Request License";
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        RequestedLicense = false;
        DialogResult = false;
        Close();
    }

    private void RequestLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        RequestedLicense = true;
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// Zeigt den Dialog und gibt zurück ob der Benutzer eine Lizenz anfordern möchte
    /// </summary>
    public static bool ShowDialog(
        Window owner,
        LocalizationService localizationService,
        LicenseManager licenseManager)
    {
        var dialog = new PowerScoringLicenseRequiredDialog(localizationService, licenseManager)
        {
            Owner = owner
        };

        var result = dialog.ShowDialog();
        return result == true && dialog.RequestedLicense;
    }
}
