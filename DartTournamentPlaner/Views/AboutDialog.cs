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
        
        // Content translations
        TitleTextBlock.Text = _localizationService.GetString("AppTitle") ?? "Dart Tournament Planner";
        
        DescriptionTextBlock.Text = _localizationService.GetString("AboutDescription") ?? 
            "A modern and user-friendly application for organizing dart tournaments. Supports various tournament formats, player management, and comprehensive statistics.";
        
        LicenseTextBlock.Text = _localizationService.GetString("OpenSource") ?? "Open Source";
        
        CreditsTextBlock.Text = _localizationService.GetString("AboutCredits") ?? 
            "Thanks to all contributors, testers, and the dart community for their valuable feedback and support in making this application better.";
        
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