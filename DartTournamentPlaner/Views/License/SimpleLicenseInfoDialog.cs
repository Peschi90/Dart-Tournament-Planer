using System;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Wrapper für das moderne Lizenz-Info Fenster
/// </summary>
public class SimpleLicenseInfoDialog
{
    private readonly Services.License.LicenseManager _licenseManager;
    private readonly LicenseFeatureService _licenseFeatureService;
    private readonly LocalizationService _localizationService;
    
    public SimpleLicenseInfoDialog(Services.License.LicenseManager licenseManager, 
                                  LicenseFeatureService licenseFeatureService,
                                  LocalizationService localizationService)
    {
        _licenseManager = licenseManager;
        _licenseFeatureService = licenseFeatureService;
        _localizationService = localizationService;
    }
    
    public void ShowDialog()
    {
        try
        {
            var window = new LicenseInfoWindow(_licenseManager, _licenseFeatureService, _localizationService);
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Fehler beim Öffnen der Lizenz-Informationen: {ex.Message}", 
                "Fehler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}