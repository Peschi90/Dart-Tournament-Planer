using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class BugReportDialog : Window
{
    private readonly LocalizationService _localizationService;

    public BugReportDialog(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        InitializeComponent();
        LoadSystemInformation();
        UpdateTranslations();
        
        // Subscribe to language changes
        _localizationService.PropertyChanged += (s, e) => UpdateTranslations();
    }

    private void LoadSystemInformation()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "Unknown";
            var osVersion = Environment.OSVersion.ToString();
            var framework = RuntimeInformation.FrameworkDescription;
            var architecture = RuntimeInformation.OSArchitecture.ToString();
            var processorCount = Environment.ProcessorCount;
            var workingSet = Environment.WorkingSet / (1024 * 1024); // MB

            var systemInfo = $"Application Version: {version}\n" +
                           $"Operating System: {osVersion}\n" +
                           $"Framework: {framework}\n" +
                           $"Architecture: {architecture}\n" +
                           $"Processor Cores: {processorCount}\n" +
                           $"Memory Usage: {workingSet} MB\n" +
                           $"Current Language: {_localizationService.CurrentLanguage}\n" +
                           $"Report Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            SystemInfoTextBlock.Text = systemInfo;
        }
        catch (Exception ex)
        {
            SystemInfoTextBlock.Text = $"Error loading system information: {ex.Message}";
        }
    }

    private void UpdateTranslations()
    {
        // Window title
        Title = _localizationService.GetString("BugReportTitle");
        
        // Header
        TitleTextBlock.Text = _localizationService.GetString("BugReport");
        DescriptionTextBlock.Text = _localizationService.GetString("BugReportDescription");
        
        // Tab headers
        DescriptionTab.Header = "📝 " + _localizationService.GetString("BugReportDescription").Replace(":", "");
        StepsTab.Header = "📋 " + _localizationService.GetString("BugReportSteps").Replace(":", "");
        ExpectedTab.Header = "✅ " + _localizationService.GetString("BugReportExpected").Replace(":", "");
        ActualTab.Header = "❌ " + _localizationService.GetString("BugReportActual").Replace(":", "");
        SystemTab.Header = "💻 " + _localizationService.GetString("BugReportSystemInfo").Replace(":", "");
        
        // Content headers
        StepsHeaderTextBlock.Text = _localizationService.GetString("BugReportSteps");
        ExpectedHeaderTextBlock.Text = _localizationService.GetString("BugReportExpected");
        ActualHeaderTextBlock.Text = _localizationService.GetString("BugReportActual");
        SystemHeaderTextBlock.Text = _localizationService.GetString("BugReportSystemInfo");
        
        // Buttons
        EmailButton.Content = "📧 " + _localizationService.GetString("BugReportSubmitEmail");
        GitHubButton.Content = "📋 " + _localizationService.GetString("BugReportSubmitGitHub");
        CancelButton.Content = _localizationService.GetString("Cancel");
    }

    private void EmailButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var subject = _localizationService.GetString("BugReportEmailSubject");
            var body = GenerateBugReportText();
            
            // Encode for URL
            var encodedSubject = Uri.EscapeDataString(subject);
            var encodedBody = Uri.EscapeDataString(body);
            
            // Create mailto link (with fallback email)
            var mailtoLink = $"mailto:support@darttournamentplanner.com?subject={encodedSubject}&body={encodedBody}";
            
            // Try to open default email client
            Process.Start(new ProcessStartInfo
            {
                FileName = mailtoLink,
                UseShellExecute = true
            });

            ShowSuccessMessage();
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    private void GitHubButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var bugReportText = GenerateBugReportText();
            var encodedBody = Uri.EscapeDataString(bugReportText);
            
            // GitHub Issues URL (replace with actual repository URL)
            var githubUrl = "https://github.com/Peschi90/Dart-Tournament-Planer/issues/new" +
                           $"?title=Bug%20Report&body={encodedBody}";
            // https://github.com/Peschi90/Dart-Turnament-Planer/issues/new
            
            Process.Start(new ProcessStartInfo
            {
                FileName = githubUrl,
                UseShellExecute = true
            });

            ShowSuccessMessage();
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private string GenerateBugReportText()
    {
        var report = $"## {_localizationService.GetString("BugReportTitle")}\n\n";
        
        if (!string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
        {
            report += $"### {_localizationService.GetString("BugReportDescription")}\n";
            report += $"{DescriptionTextBox.Text}\n\n";
        }
        
        if (!string.IsNullOrWhiteSpace(StepsTextBox.Text))
        {
            report += $"### {_localizationService.GetString("BugReportSteps")}\n";
            report += $"{StepsTextBox.Text}\n\n";
        }
        
        if (!string.IsNullOrWhiteSpace(ExpectedTextBox.Text))
        {
            report += $"### {_localizationService.GetString("BugReportExpected")}\n";
            report += $"{ExpectedTextBox.Text}\n\n";
        }
        
        if (!string.IsNullOrWhiteSpace(ActualTextBox.Text))
        {
            report += $"### {_localizationService.GetString("BugReportActual")}\n";
            report += $"{ActualTextBox.Text}\n\n";
        }
        
        report += $"### {_localizationService.GetString("BugReportSystemInfo")}\n";
        report += $"```\n{SystemInfoTextBlock.Text}\n```\n";
        
        return report;
    }

    private void ShowSuccessMessage()
    {
        var title = _localizationService.GetString("Information");
        var message = _localizationService.GetString("BugReportSent");
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ShowErrorMessage(string error)
    {
        var title = _localizationService.GetString("Error");
        var message = $"{_localizationService.GetString("ErrorSendingBugReport")} {error}";
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}