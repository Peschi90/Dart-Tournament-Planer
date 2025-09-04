using System;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Dialog der angezeigt wird, wenn Tournament Overview Funktionalitaet ohne entsprechende Lizenz aufgerufen wird
/// </summary>
public partial class TournamentOverviewLicenseRequiredDialog : Window
{
    private readonly LocalizationService _localizationService;
    private readonly LicenseManager _licenseManager;

    public TournamentOverviewLicenseRequiredDialog(LocalizationService localizationService, LicenseManager licenseManager)
    {
        _localizationService = localizationService;
        _licenseManager = licenseManager;
        
        InitializeComponent();
        UpdateTranslations();
        
        // Subscribe to language changes
        _localizationService.PropertyChanged += (s, e) => UpdateTranslations();
    }

    private void UpdateTranslations()
    {
        // Window title
        Title = _localizationService.GetString("TournamentOverviewLicenseRequiredTitle");
        
        // Header texts
        TitleText.Text = _localizationService.GetString("TournamentOverviewLicenseRequiredTitle");
        SubtitleText.Text = _localizationService.GetString("TournamentOverviewLicenseRequiredSubtitle");
        
        // Message content
        MessageText.Text = _localizationService.GetString("TournamentOverviewLicenseMessage");
        BenefitsTitle.Text = _localizationService.GetString("TournamentOverviewBenefitsTitle");
        BenefitsList.Text = _localizationService.GetString("TournamentOverviewBenefits");
        ActionText.Text = _localizationService.GetString("TournamentOverviewActionText");
        
        // Buttons
        CancelButton.Content = _localizationService.GetString("Cancel");
        RequestLicenseButton.Content = _localizationService.GetString("RequestTournamentOverviewLicense");
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
            // Close current dialog
            DialogResult = true;
            Close();

            // Open Purchase License Dialog
            var purchaseDialog = new PurchaseLicenseDialog(_localizationService, _licenseManager);
            if (Owner != null)
            {
                purchaseDialog.Owner = Owner;
            }

            // Pre-select Tournament Overview Feature
            purchaseDialog.Loaded += (s, args) =>
            {
                try
                {
                    // Focus on Tournament Overview Feature
                    purchaseDialog.FocusOnTournamentOverviewFeature();
                    System.Diagnostics.Debug.WriteLine("Tournament Overview feature pre-selected in Purchase Dialog");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error pre-selecting tournament overview feature: {ex.Message}");
                }
            };

            purchaseDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("Error")}: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Statische Methode um den Dialog anzuzeigen
    /// </summary>
    /// <param name="owner">Parent Window</param>
    /// <param name="localizationService">Lokalisierungsservice</param>
    /// <param name="licenseManager">Lizenzmanager</param>
    /// <returns>True wenn Benutzer Lizenz anfordern moechte</returns>
    public static bool ShowDialog(Window? owner, LocalizationService localizationService, LicenseManager licenseManager)
    {
        var dialog = new TournamentOverviewLicenseRequiredDialog(localizationService, licenseManager)
        {
            Owner = owner
        };

        return dialog.ShowDialog() == true;
    }
}