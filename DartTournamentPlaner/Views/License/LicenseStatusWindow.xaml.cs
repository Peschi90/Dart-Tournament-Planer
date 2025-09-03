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
/// Lizenz-Status Übersichtsfenster im modernen Design
/// </summary>
public partial class LicenseStatusWindow : Window
{
    private readonly Services.License.LicenseManager _licenseManager;
    private readonly LicenseFeatureService _licenseFeatureService;
    private readonly LocalizationService _localizationService;
    private LicenseStatus _currentStatus;
    private LicenseValidationResult? _lastValidationResult;
    
    public LicenseStatusWindow(Services.License.LicenseManager licenseManager, 
                              LicenseFeatureService licenseFeatureService,
                              LocalizationService localizationService)
    {
        _licenseManager = licenseManager;
        _licenseFeatureService = licenseFeatureService;
        _localizationService = localizationService;
        
        InitializeComponent();
        InitializeWindow();
        _ = LoadLicenseStatusAsync();
    }
    
    private void InitializeWindow()
    {
        Title = _localizationService.GetString("LicenseStatus") ?? "License Status";
        
        // Apply translations
        TitleText.Text = (_localizationService.GetString("LicenseStatus") ?? "License Status");
        SubtitleText.Text = _localizationService.GetString("LicenseStatusSubtitle") ?? 
            "Overview of your current license status and available features";
        
        RefreshButton.Content = (_localizationService.GetString("Refresh") ?? "Refresh");
        DetailsButton.Content = (_localizationService.GetString("ShowDetails") ?? "Show Details");
        CloseButton.Content = _localizationService.GetString("Close") ?? "Close";
        
        // Load system information
        LoadSystemInformation();
    }
    
    private void LoadSystemInformation()
    {
        SystemHardwareIdText.Text = Services.License.LicenseManager.GenerateHardwareId();
        SystemVersionText.Text = _localizationService.GetApplicationVersion();
    }
    
    private async Task LoadLicenseStatusAsync()
    {
        try
        {
            // Status vom Feature Service holen
            _currentStatus = _licenseFeatureService.CurrentStatus;
            
            if (_currentStatus.IsLicensed)
            {
                // Frische Daten vom Server holen (nicht blockierend)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var validationResult = await _licenseManager.ValidateLicenseAsync();
                        _lastValidationResult = validationResult;
                        
                        Dispatcher.Invoke(() =>
                        {
                            if (validationResult.IsValid && validationResult.Data != null)
                            {
                                UpdateLicensedDisplay(validationResult.Data, validationResult.Offline);
                            }
                        });
                    }
                    catch
                    {
                        // Verwende gecatche Daten bei Fehlern
                    }
                });
                
                // Sofort mit aktuellen Daten anzeigen
                var cachedData = _licenseManager.GetCurrentLicenseInfo();
                if (cachedData != null)
                {
                    UpdateLicensedDisplay(cachedData, true);
                }
                else
                {
                    ShowUnlicensedDisplay();
                }
            }
            else
            {
                ShowUnlicensedDisplay();
            }
        }
        catch (Exception ex)
        {
            ShowErrorDisplay($"Error loading license status: {ex.Message}");
        }
    }
    
    private void UpdateLicensedDisplay(LicenseData licenseData, bool isOffline)
    {
        // Status Icon und Border
        StatusIcon.Text = "✅";
        StatusIconBorder.Background = new SolidColorBrush(Color.FromRgb(220, 252, 231)); // Light Green
        StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
        
        // Status Information
        if (licenseData.ExpiresAt.HasValue && licenseData.ExpiresAt.Value < DateTime.Now)
        {
            // Expired
            StatusTitle.Text = "License Expired";
            StatusTitle.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
            StatusDescription.Text = $"Your license expired on {licenseData.ExpiresAt.Value:dd.MM.yyyy}. " +
                "Please contact support for renewal.";
            PrimaryActionButton.Content = "Renew License";
        }
        else if (isOffline)
        {
            StatusTitle.Text = "Licensed (Offline)";
            StatusTitle.Foreground = new SolidColorBrush(Color.FromRgb(245, 124, 0)); // Orange
            StatusDescription.Text = "Your license is valid but validated offline. " +
                "An internet connection is required for full validation.";
            PrimaryActionButton.Content = "Validate Online";
        }
        else
        {
            StatusTitle.Text = "Fully Licensed";
            StatusTitle.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
            StatusDescription.Text = $"Your license is active and valid. " +
                $"You have access to all premium features of {licenseData.ProductName ?? "Dart Tournament Planner"}.";
            PrimaryActionButton.Content = "Show Details";
        }
        
        // Quick Info anzeigen
        QuickInfoGrid.Visibility = Visibility.Visible;
        QuickCustomerText.Text = licenseData.CustomerName ?? "Not available";
        
        if (licenseData.ExpiresAt.HasValue)
        {
            QuickExpiryText.Text = licenseData.ExpiresAt.Value.ToString("dd.MM.yyyy");
            
            // Warnung bei bald ablaufender Lizenz
            var daysUntilExpiry = (licenseData.ExpiresAt.Value - DateTime.Now).Days;
            if (daysUntilExpiry <= 30 && daysUntilExpiry >= 0)
            {
                QuickExpiryText.Foreground = new SolidColorBrush(Color.FromRgb(245, 124, 0)); // Orange
                if (daysUntilExpiry <= 7)
                {
                    QuickExpiryText.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                }
            }
        }
        else
        {
            QuickExpiryText.Text = "Unlimited";
            QuickExpiryText.Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)); // Green
        }
        
        QuickFeaturesText.Text = licenseData.Features?.Length.ToString() ?? "All Core Features";
        
        // Features anzeigen
        LoadFeatures(licenseData.Features, true);
    }
    
    private void ShowUnlicensedDisplay()
    {
        // Status Icon und Border
        StatusIcon.Text = "🆓";
        StatusIconBorder.Background = new SolidColorBrush(Color.FromRgb(243, 244, 246)); // Light Gray
        StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); // Gray
        
        // Status Information
        StatusTitle.Text = "Free Version";
        StatusTitle.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); // Gray
        StatusDescription.Text = "You are using the free version of Dart Tournament Planner. " +
            "All core features are available! Activate a license for additional premium features.";
        PrimaryActionButton.Content = "Activate License";
        
        // Quick Info verstecken
        QuickInfoGrid.Visibility = Visibility.Collapsed;
        
        // Core-Features anzeigen
        LoadFeatures(null, false);
    }
    
    private void ShowErrorDisplay(string errorMessage)
    {
        StatusIcon.Text = "⚠️";
        StatusIconBorder.Background = new SolidColorBrush(Color.FromRgb(254, 226, 226)); // Light Red
        StatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38)); // Red
        
        StatusTitle.Text = "Error Loading";
        StatusTitle.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
        StatusDescription.Text = errorMessage;
        PrimaryActionButton.Content = "Try Again";
        
        QuickInfoGrid.Visibility = Visibility.Collapsed;
        
        MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    private void LoadFeatures(string[]? features, bool isLicensed)
    {
        FeaturesPanel.Children.Clear();
        
        if (isLicensed && features != null && features.Length > 0)
        {
            // Premium-Features anzeigen
            foreach (var feature in features)
            {
                var badge = CreateFeatureBadge(GetFeatureDisplayName(feature), true);
                FeaturesPanel.Children.Add(badge);
            }
        }
        else
        {
            // Core features display
            var coreFeatures = new[]
            {
                "Tournament Management",
                "Player Tracking", 
                "Statistics",
                "API Access",
                "Hub Integration",
                "Multi Tournament"
            };
            
            foreach (var feature in coreFeatures)
            {
                var badge = CreateFeatureBadge(feature, false);
                FeaturesPanel.Children.Add(badge);
            }
            
            // Hint for additional premium features
            if (!isLicensed)
            {
                var premiumHintBadge = CreateFeatureBadge("+ Premium Features available", null);
                FeaturesPanel.Children.Add(premiumHintBadge);
            }
        }
    }
    
    private Border CreateFeatureBadge(string featureName, bool? isPremium)
    {
        var border = new Border
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16, 8, 16, 8),
            Margin = new Thickness(6, 6, 6, 6)
        };
        
        var textBlock = new TextBlock
        {
            FontSize = 13,
            FontWeight = FontWeights.Medium
        };
        
        if (isPremium == true)
        {
            // Premium-Features
            border.Background = new SolidColorBrush(Color.FromRgb(239, 246, 255)); // Blue
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(219, 234, 254));
            border.BorderThickness = new Thickness(1);
            textBlock.Text = $"⭐ {featureName}";
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(30, 64, 175)); // Blue
        }
        else if (isPremium == false)
        {
            // Core-Features
            border.Background = new SolidColorBrush(Color.FromRgb(240, 253, 244)); // Green
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(187, 247, 208));
            border.BorderThickness = new Thickness(1);
            textBlock.Text = $"✅ {featureName}";
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52)); // Green
        }
        else
        {
            // Premium-Hinweis
            border.Background = new SolidColorBrush(Color.FromRgb(255, 247, 237)); // Orange
            border.BorderBrush = new SolidColorBrush(Color.FromRgb(254, 215, 170));
            border.BorderThickness = new Thickness(2);
            textBlock.Text = $"🚀 {featureName}";
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(154, 52, 18)); // Orange
            textBlock.FontStyle = FontStyles.Italic;
        }
        
        border.Child = textBlock;
        return border;
    }
    
    private static string GetFeatureDisplayName(string featureId)
    {
        return featureId switch
        {
            "tournament_management" => "Tournament Management",
            "player_tracking" => "Player Tracking",
            "statistics" => "Statistics", 
            "api_access" => "API Access",
            "hub_integration" => "Hub Integration",
            "multi_tournament" => "Multi Tournament",
            "advanced_reporting" => "Advanced Reporting",
            "custom_themes" => "Custom Themes",
            "premium_support" => "Premium Support",
            "cloud_sync" => "Cloud Synchronization",
            "tournament_templates" => "Tournament Templates",
            "advanced_statistics" => "Advanced Statistics",
            _ => featureId
        };
    }
    
    // Event Handlers
    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshButton.IsEnabled = false;
        RefreshButton.Content = "Updating...";
        
        try
        {
            await _licenseFeatureService.InitializeAsync();
            await LoadLicenseStatusAsync();
        }
        finally
        {
            RefreshButton.IsEnabled = true;
            RefreshButton.Content = (_localizationService.GetString("Refresh") ?? "Refresh");
        }
    }
    
    private void ShowDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var detailsDialog = new SimpleLicenseInfoDialog(_licenseManager, _licenseFeatureService, _localizationService);
            detailsDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening detailed view: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private async void PrimaryAction_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var buttonContent = PrimaryActionButton.Content.ToString();
            
            if (buttonContent?.Contains("Activate") == true)
            {
                // Activate license
                var activationDialog = new SimpleLicenseActivationDialog(_localizationService, _licenseManager);
                if (await activationDialog.ShowDialogAsync())
                {
                    await LoadLicenseStatusAsync();
                }
            }
            else if (buttonContent?.Contains("Details") == true)
            {
                // Show details
                ShowDetailsButton_Click(sender, e);
            }
            else if (buttonContent?.Contains("Validate") == true)
            {
                // Online validate
                RefreshButton_Click(sender, e);
            }
            else if (buttonContent?.Contains("Renew") == true)
            {
                // Go to license purchase page
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://dart-tournament-planner.com/license",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
            }
            else if (buttonContent?.Contains("Try") == true)
            {
                // Try again
                RefreshButton_Click(sender, e);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error performing action: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}