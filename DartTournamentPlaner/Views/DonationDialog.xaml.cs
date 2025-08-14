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
        Title = _localizationService.GetString("SupportDevelopment");
        
        // Content
        TitleTextBlock.Text = _localizationService.GetString("SupportDevelopment");
        MessageTextBlock.Text = _localizationService.GetString("DonationMessage");
        
        // Buttons
        DonateButton.Content = "?? " + _localizationService.GetString("OpenDonationPage");
        CloseButton.Content = _localizationService.GetString("Close");
    }

    private void DonateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // PayPal donation link (replace with your actual PayPal donation URL)
            var donationUrl = "https://www.paypal.com/donate/?hosted_button_id=YOUR_PAYPAL_BUTTON_ID";
            
            // Alternative: Ko-fi, GitHub Sponsors, or other donation platform
            // var donationUrl = "https://ko-fi.com/yourusername";
            // var donationUrl = "https://github.com/sponsors/yourusername";
            
            Process.Start(new ProcessStartInfo
            {
                FileName = donationUrl,
                UseShellExecute = true
            });

            // Show thank you message
            var title = _localizationService.GetString("Information");
            var message = _localizationService.GetString("ThankYouSupport");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
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