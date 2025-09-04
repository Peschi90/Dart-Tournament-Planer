using System.Diagnostics;
using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class DonationDialog : Window
{
    private readonly LocalizationService _localizationService;

    public DonationDialog(LocalizationService localizationService)
    {
        _localizationService = localizationService;
        InitializeComponent();
        UpdateTranslations();
        
        // Subscribe to language changes
        _localizationService.PropertyChanged += (s, e) => UpdateTranslations();
    }

    private void UpdateTranslations()
    {
        // Window title
        Title = _localizationService.GetString("SupportDevelopmentTitle");
        
        // Header content
        TitleTextBlock.Text = _localizationService.GetString("SupportDevelopmentTitle");
        
        // Main message
        MessageTextBlock.Text = _localizationService.GetString("DonationDialogMessage");
        
        // Support benefits section
        SupportTitleText.Text = _localizationService.GetString("WithYourSupportYouEnable");
        Benefit1Text.Text = _localizationService.GetString("RegularUpdatesAndNewFeatures");
        Benefit2Text.Text = _localizationService.GetString("FastBugFixes");
        Benefit3Text.Text = _localizationService.GetString("ContinuousSupport");
        Benefit4Text.Text = _localizationService.GetString("FreeUseForEveryone");
        
        // Buttons
        DonateButton.Content = _localizationService.GetString("OpenDonationPageButton");
        CloseButton.Content = _localizationService.GetString("LaterButton");
    }

    private void DonateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // PayPal donation link (replace with your actual PayPal donation URL)
            var donationUrl = "https://www.paypal.com/paypalme/I3ull3t";
            
            // Alternative: Ko-fi, GitHub Sponsors, or other donation platform
            // var donationUrl = "https://ko-fi.com/yourusername";
            // var donationUrl = "https://github.com/sponsors/yourusername";
            
            Process.Start(new ProcessStartInfo
            {
                FileName = donationUrl,
                UseShellExecute = true
            });

            // Show thank you message (optional)
            //var title = _localizationService.GetString("Information");
            //var message = _localizationService.GetString("ThankYouSupport");
            //MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            
            DialogResult = true;
            Close();
        }
        catch (System.Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("Error")}: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}