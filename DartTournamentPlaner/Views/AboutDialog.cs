using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class AboutDialog : Window
{
    private readonly LocalizationService _localizationService;

    public AboutDialog(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        InitializeComponent();
        LoadApplicationInformation();
        UpdateTranslations();
        
        // Subscribe to language changes
        _localizationService.PropertyChanged += (s, e) => UpdateTranslations();
    }

    private void LoadApplicationInformation()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
            var framework = RuntimeInformation.FrameworkDescription;
            
            VersionTextBlock.Text = $"Version {version}";
            FrameworkTextBlock.Text = $".NET 9 / WPF ({RuntimeInformation.OSArchitecture})";
            
            // Load company/developer info from assembly
            var companyAttribute = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (companyAttribute != null && !string.IsNullOrEmpty(companyAttribute.Company))
            {
                DeveloperTextBlock.Text = companyAttribute.Company;
            }
            
            // Add click handler for website
            WebsiteTextBlock.MouseLeftButtonUp += (s, e) =>
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = "https://github.com/Peschi90/Dart-Turnament-Planer",
                        UseShellExecute = true
                    };
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    var title = _localizationService.GetString("Error") ?? "Error";
                    var message = $"Could not open website: {ex.Message}";
                    MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading application information: {ex.Message}");
        }
    }

    private void UpdateTranslations()
    {
        // Window title
        Title = _localizationService.GetString("About") ?? "About Dart Tournament Planner";
        
        // Header
        TitleTextBlock.Text = _localizationService.GetString("AppTitle") ?? "Dart Tournament Planner";
        
        // Description Card
        DescriptionTextBlock.Text = _localizationService.GetString("AboutDescription") ?? 
            "A comprehensive and modern application for organizing and managing dart tournaments. Supports multiple tournament formats including group stages, finals, and knockout phases.";
        
        // Features Card
        FeaturesHeaderTextBlock.Text = _localizationService.GetString("Features") ?? "Features";
        FeaturesTextBlock.Text = _localizationService.GetString("FeatureList") ?? 
            "• Multiple tournament formats (Group, Finals, Knockout)\n" +
            "• Real-time match tracking and score management\n" +
            "• Comprehensive player statistics\n" +
            "• WebSocket-based Tournament Hub integration\n" +
            "• QR code generation for mobile access\n" +
            "• Professional print functionality\n" +
            "• Multi-language support (English/German)\n" +
            "• Dark/Light theme support\n" +
            "• Auto-save and data persistence";
        
        // Info Card Labels
        AppInfoHeaderTextBlock.Text = _localizationService.GetString("AppInformation") ?? "Application Information";
        DeveloperLabelTextBlock.Text = _localizationService.GetString("Developer") ?? "Developer:";
        FrameworkLabelTextBlock.Text = _localizationService.GetString("Framework") ?? "Framework:";
        LicenseLabelTextBlock.Text = _localizationService.GetString("License") ?? "License:";
        WebsiteLabelTextBlock.Text = _localizationService.GetString("Website") ?? "Website:";
        
        // Info Card Values
        DeveloperTextBlock.Text = _localizationService.GetString("DeveloperName") ?? "Marcel Peschka";
        LicenseTextBlock.Text = _localizationService.GetString("LicenseType") ?? "Open Source (MIT License)";
        WebsiteTextBlock.Text = _localizationService.GetString("WebsiteUrl") ?? "https://github.com/Peschi90/Dart-Turnament-Planer";
        
        // Credits Card
        ThanksHeaderTextBlock.Text = _localizationService.GetString("SpecialThanks") ?? "Special Thanks";
        CreditsTextBlock.Text = _localizationService.GetString("AboutCredits") ?? 
            "Special thanks to all contributors, testers, and the dart community for their valuable feedback and support. This project is continuously improved through community engagement and feedback.";
        
        // Button
        CloseButton.Content = _localizationService.GetString("Close") ?? "Close";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Shows the About dialog as a modal dialog
    /// </summary>
    public static void ShowDialog(Window owner, LocalizationService localizationService)
    {
        try
        {
            var dialog = new AboutDialog(localizationService);
            
            if (owner != null)
            {
                dialog.Owner = owner;
            }
            
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing About dialog: {ex.Message}");
            
            // Fallback to simple MessageBox
            var title = localizationService.GetString("About") ?? "About";
            var message = localizationService.GetString("AboutText") ?? 
                "Dart Tournament Planner - A modern application for organizing dart tournaments.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}