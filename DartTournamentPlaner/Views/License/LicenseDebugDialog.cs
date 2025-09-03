using System;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Einfacher Debug-Dialog für Lizenz-Informationen
/// </summary>
public partial class LicenseDebugDialog : Window
{
    private readonly LicenseFeatureService _licenseFeatureService;
    private readonly LicenseManager _licenseManager;
    private readonly LocalizationService _localizationService;

    public LicenseDebugDialog(LicenseFeatureService licenseFeatureService, LicenseManager licenseManager, LocalizationService localizationService)
    {
        _licenseFeatureService = licenseFeatureService;
        _licenseManager = licenseManager;
        _localizationService = localizationService;
        
        InitializeComponent();
        LoadDebugInfo();
    }

    private void LoadDebugInfo()
    {
        Title = "License Debug Information";
        
        var info = new System.Text.StringBuilder();
        
        try
        {
            var status = _licenseFeatureService.CurrentStatus;
            var hasLicense = _licenseManager.HasLicense();
            var hasEnhancedPrinting = _licenseFeatureService.HasFeature(Models.License.LicenseFeatures.ENHANCED_PRINTING);
            
            info.AppendLine("=== LICENSE DEBUG INFO ===");
            info.AppendLine($"License Manager HasLicense(): {hasLicense}");
            info.AppendLine($"Current Status IsLicensed: {status?.IsLicensed ?? false}");
            info.AppendLine($"Current Status IsValid: {status?.IsValid ?? false}");
            info.AppendLine($"Current Status IsExpired: {status?.IsExpired ?? false}");
            info.AppendLine($"Current Status IsOffline: {status?.IsOffline ?? false}");
            info.AppendLine($"Customer Name: {status?.CustomerName ?? "N/A"}");
            info.AppendLine($"Product Name: {status?.ProductName ?? "N/A"}");
            info.AppendLine($"Status Message: {status?.StatusMessage ?? "N/A"}");
            info.AppendLine();
            
            info.AppendLine("=== FEATURE CHECK ===");
            info.AppendLine($"HasFeature(ENHANCED_PRINTING): {hasEnhancedPrinting}");
            info.AppendLine($"Active Features Count: {status?.ActiveFeatures?.Count ?? 0}");
            
            if (status?.ActiveFeatures != null && status.ActiveFeatures.Count > 0)
            {
                info.AppendLine("Active Features:");
                foreach (var feature in status.ActiveFeatures)
                {
                    info.AppendLine($"  - {feature}");
                }
            }
            
            info.AppendLine();
            info.AppendLine("=== PRINT ACCESS LOGIC ===");
            var printAllowed = status?.IsLicensed == true && hasEnhancedPrinting;
            info.AppendLine($"Print Access Allowed: {printAllowed}");
            info.AppendLine($"Logic: IsLicensed({status?.IsLicensed ?? false}) AND HasEnhancedPrinting({hasEnhancedPrinting}) = {printAllowed}");
            
        }
        catch (Exception ex)
        {
            info.AppendLine($"ERROR: {ex.Message}");
            info.AppendLine($"Stack Trace: {ex.StackTrace}");
        }
        
        DebugTextBox.Text = info.ToString();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadDebugInfo();
    }

    /// <summary>
    /// Statische Methode um den Debug-Dialog anzuzeigen
    /// </summary>
    public static void ShowDebugDialog(Window? owner, LicenseFeatureService licenseFeatureService, LicenseManager licenseManager, LocalizationService localizationService)
    {
        var dialog = new LicenseDebugDialog(licenseFeatureService, licenseManager, localizationService)
        {
            Owner = owner
        };
        dialog.ShowDialog();
    }
}