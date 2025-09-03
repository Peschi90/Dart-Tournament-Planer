using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Models.License;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Modernes Lizenz-Info Fenster im Design der Anwendung
/// </summary>
public partial class LicenseInfoWindow : Window
{
    private readonly Services.License.LicenseManager _licenseManager;
    private readonly LicenseFeatureService _licenseFeatureService;
    private readonly LocalizationService _localizationService;
    private LicenseStatus _currentStatus;
    
    public LicenseInfoWindow(Services.License.LicenseManager licenseManager, 
                            LicenseFeatureService licenseFeatureService,
                            LocalizationService localizationService)
    {
        _licenseManager = licenseManager;
        _licenseFeatureService = licenseFeatureService;
        _localizationService = localizationService;
        
        InitializeComponent();
        InitializeWindow();
        _ = LoadLicenseInformationAsync();
    }
    
    private void InitializeWindow()
    {
        Title = _localizationService.GetString("LicenseInfo") ?? "Lizenz-Informationen";
        
        // Übersetzungen anwenden
        TitleText.Text = "🔑 " + (_localizationService.GetString("LicenseInfo") ?? "Lizenz-Informationen");
        SubtitleText.Text = _localizationService.GetString("LicenseInfoSubtitle") ?? 
            "Aktuelle Lizenz- und Feature-Informationen";
        
        RefreshButton.Content = "🔄 " + (_localizationService.GetString("Refresh") ?? "Aktualisieren");
        RemoveLicenseButton.Content = "🗑️ " + (_localizationService.GetString("RemoveLicense") ?? "Lizenz entfernen");
        CloseButton.Content = _localizationService.GetString("Close") ?? "Schließen";
        
        // System-Informationen laden
        LoadSystemInformation();
    }
    
    private void LoadSystemInformation()
    {
        HardwareIdText.Text = Services.License.LicenseManager.GenerateHardwareId();
        ClientVersionText.Text = _localizationService.GetApplicationVersion();
    }
    
    private async Task LoadLicenseInformationAsync()
    {
        try
        {
            // Status vom Feature Service holen
            _currentStatus = _licenseFeatureService.CurrentStatus;
            
            if (_currentStatus.IsLicensed)
            {
                // Frische Daten vom Server holen
                var validationResult = await _licenseManager.ValidateLicenseAsync();
                if (validationResult.IsValid && validationResult.Data != null)
                {
                    UpdateLicenseDisplay(validationResult.Data, validationResult.Offline);
                }
                else
                {
                    ShowUnlicensedState();
                }
            }
            else
            {
                ShowUnlicensedState();
            }
        }
        catch (Exception ex)
        {
            ShowErrorState($"Fehler beim Laden der Lizenz-Informationen: {ex.Message}");
        }
    }
    
    private void UpdateLicenseDisplay(LicenseData licenseData, bool isOffline)
    {
        // Status Badge
        UpdateStatusBadge(licenseData, isOffline);
        
        // Lizenz-Status
        LicenseStatusText.Text = isOffline ? "✅ Gültig (Offline)" : "✅ Gültig";
        LicenseStatusText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
        
        LicenseTypeText.Text = licenseData.ProductName ?? "Standard";
        
        // Gültigkeit
        if (licenseData.ExpiresAt.HasValue)
        {
            ExpiryDateText.Text = licenseData.ExpiresAt.Value.ToString("dd.MM.yyyy HH:mm");
            
            // Warnung bei bald ablaufender Lizenz
            var daysUntilExpiry = (licenseData.ExpiresAt.Value - DateTime.Now).Days;
            if (daysUntilExpiry <= 30)
            {
                ExpiryDateText.Foreground = new SolidColorBrush(Color.FromRgb(245, 124, 0)); // Orange
                if (daysUntilExpiry <= 7)
                {
                    ExpiryDateText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                }
            }
        }
        else
        {
            ExpiryDateText.Text = "Unbegrenzt";
            ExpiryDateText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
        }
        
        LastValidationText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        if (isOffline)
        {
            LastValidationText.Text += " (Offline-Modus)";
            LastValidationText.Foreground = new SolidColorBrush(Color.FromRgb(245, 124, 0)); // Orange
        }
        
        // Lizenz-Details
        CustomerNameText.Text = licenseData.CustomerName ?? "Nicht verfügbar";
        ProductNameText.Text = licenseData.ProductName ?? "Dart Tournament Planner";
        LicenseKeyText.Text = FormatLicenseKey(licenseData.LicenseKey);
        
        // Aktivierungen
        if (licenseData.RemainingActivations.HasValue && licenseData.MaxActivations.HasValue)
        {
            var used = licenseData.MaxActivations.Value - licenseData.RemainingActivations.Value;
            ActivationsText.Text = $"{used}/{licenseData.MaxActivations} verwendet ({licenseData.RemainingActivations} verbleibend)";
            
            // Warnung bei wenigen Aktivierungen
            if (licenseData.RemainingActivations <= 1)
            {
                ActivationsText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
            }
            else if (licenseData.RemainingActivations <= 2)
            {
                ActivationsText.Foreground = new SolidColorBrush(Color.FromRgb(245, 124, 0)); // Orange
            }
        }
        else
        {
            ActivationsText.Text = "Unbegrenzt";
        }
        
        // Server-Version
        ServerVersionText.Text = licenseData.ServerVersion ?? "Nicht verfügbar";
        
        // Features anzeigen
        LoadFeatures(licenseData.Features);
        
        // Lizenz entfernen Button anzeigen
        RemoveLicenseButton.Visibility = Visibility.Visible;
    }
    
    private void UpdateStatusBadge(LicenseData licenseData, bool isOffline)
    {
        if (licenseData.ExpiresAt.HasValue && licenseData.ExpiresAt.Value < DateTime.Now)
        {
            // Abgelaufen
            StatusIcon.Text = "⏰";
            StatusText.Text = "Abgelaufen";
            StatusBadge.Background = new SolidColorBrush(Color.FromRgb(254, 226, 226)); // Light Red
            StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38)); // Red
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
        }
        else if (isOffline)
        {
            // Offline aber gültig
            StatusIcon.Text = "📶";
            StatusText.Text = "Offline-Modus";
            StatusBadge.Background = new SolidColorBrush(Color.FromRgb(255, 237, 213)); // Light Orange
            StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(245, 124, 0)); // Orange
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(245, 124, 0));
        }
        else
        {
            // Gültig und online
            StatusIcon.Text = "✅";
            StatusText.Text = "Aktiv";
            StatusBadge.Background = new SolidColorBrush(Color.FromRgb(220, 252, 231)); // Light Green
            StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
            StatusText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94));
        }
    }
    
    private void ShowUnlicensedState()
    {
        // Status Badge
        StatusIcon.Text = "📴";
        StatusText.Text = "Unlizenziert";
        StatusBadge.Background = new SolidColorBrush(Color.FromRgb(243, 244, 246)); // Light Gray
        StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); // Gray
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
        
        // Status
        LicenseStatusText.Text = "❌ Unlizenziert";
        LicenseStatusText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); // Gray
        
        LicenseTypeText.Text = "Free Version";
        ExpiryDateText.Text = "Nicht anwendbar";
        LastValidationText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        
        // Details
        CustomerNameText.Text = "Nicht lizenziert";
        ProductNameText.Text = "Dart Tournament Planner (Free)";
        LicenseKeyText.Text = "Keine Lizenz aktiviert";
        ActivationsText.Text = "Nicht anwendbar";
        ServerVersionText.Text = "Nicht verfügbar";
        
        // Core-Features anzeigen
        LoadCoreFeatures();
        
        RemoveLicenseButton.Visibility = Visibility.Collapsed;
    }
    
    private void ShowErrorState(string errorMessage)
    {
        StatusIcon.Text = "⚠️";
        StatusText.Text = "Fehler";
        StatusBadge.Background = new SolidColorBrush(Color.FromRgb(254, 226, 226)); // Light Red
        StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
        StatusText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
        
        LicenseStatusText.Text = "❌ Fehler beim Laden";
        LicenseStatusText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
        
        MessageBox.Show(errorMessage, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    private void LoadFeatures(string[]? features)
    {
        FeaturesPanel.Children.Clear();
        
        if (features != null && features.Length > 0)
        {
            NoFeaturesText.Visibility = Visibility.Collapsed;
            
            foreach (var feature in features)
            {
                var badge = CreateFeatureBadge(GetFeatureDisplayName(feature), true);
                FeaturesPanel.Children.Add(badge);
            }
        }
        else
        {
            LoadCoreFeatures();
        }
    }
    
    private void LoadCoreFeatures()
    {
        FeaturesPanel.Children.Clear();
        NoFeaturesText.Visibility = Visibility.Collapsed;
        
        // Alle Core-Features anzeigen
        var coreFeatures = new[]
        {
            "Turnier-Management",
            "Spieler-Verfolgung", 
            "Statistiken",
            "API-Zugang",
            "Hub-Integration",
            "Multi-Turnier"
        };
        
        foreach (var feature in coreFeatures)
        {
            var badge = CreateFeatureBadge(feature, false);
            FeaturesPanel.Children.Add(badge);
        }
    }
    
    private Border CreateFeatureBadge(string featureName, bool isPremium)
    {
        var border = new Border
        {
            Background = isPremium ? 
                new SolidColorBrush(Color.FromRgb(239, 246, 255)) : // Blue for premium
                new SolidColorBrush(Color.FromRgb(240, 253, 244)),  // Green for core
            BorderBrush = isPremium ?
                new SolidColorBrush(Color.FromRgb(219, 234, 254)) : // Blue border
                new SolidColorBrush(Color.FromRgb(187, 247, 208)),  // Green border
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(4, 4, 4, 4)
        };
        
        var textBlock = new TextBlock
        {
            Text = $"{(isPremium ? "⭐" : "✅")} {featureName}",
            FontSize = 13,
            FontWeight = FontWeights.Medium,
            Foreground = isPremium ?
                new SolidColorBrush(Color.FromRgb(30, 64, 175)) : // Blue text
                new SolidColorBrush(Color.FromRgb(22, 101, 52))   // Green text
        };
        
        border.Child = textBlock;
        return border;
    }
    
    private static string FormatLicenseKey(string? licenseKey)
    {
        if (string.IsNullOrEmpty(licenseKey))
            return "Nicht verfügbar";
            
        // Zeige nur die ersten und letzten Segmente
        if (licenseKey.Length > 20)
        {
            var parts = licenseKey.Split('-');
            if (parts.Length >= 9) // DART + 8 Segmente
            {
                return $"{parts[0]}-{parts[1]}-****-****-****-****-{parts[7]}-{parts[8]}";
            }
        }
        
        return licenseKey;
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
            "cloud_sync" => "Cloud-Synchronisation",
            "tournament_templates" => "Turnier-Vorlagen",
            "advanced_statistics" => "Erweiterte Statistiken",
            _ => featureId
        };
    }
    
    // Event Handlers
    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        RefreshButton.Content = "🔄 Wird aktualisiert...";
        
        try
        {
            await LoadLicenseInformationAsync();
        }
        finally
        {
            RefreshButton.IsEnabled = true;
            RefreshButton.Content = "🔄 " + (_localizationService.GetString("Refresh") ?? "Aktualisieren");
        }
    }
    
    private void RemoveLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService.GetString("RemoveLicense") ?? "Lizenz entfernen";
        var message = "Möchten Sie die Lizenz wirklich entfernen?\n\nDie Anwendung wird nach dem Neustart als unlizenziert ausgeführt.";
        
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _licenseManager.RemoveLicense();
                ShowUnlicensedState();
                
                MessageBox.Show("Lizenz wurde erfolgreich entfernt.", "Erfolg", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Entfernen der Lizenz: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}