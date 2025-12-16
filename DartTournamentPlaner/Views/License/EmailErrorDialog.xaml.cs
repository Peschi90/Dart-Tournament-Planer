using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Dialog für fehlgeschlagene E-Mail-Sendung im modernen Design
/// </summary>
public partial class EmailErrorDialog : Window
{
    public EmailErrorDialog(LocalizationService localizationService, string? errorDetails = null)
    {
        InitializeComponent();
        ApplyTranslations(localizationService);
        
        // Zeige Fehlerdetails falls vorhanden
        if (!string.IsNullOrWhiteSpace(errorDetails))
        {
            ErrorDetailsPanel.Visibility = Visibility.Visible;
            ErrorDetailsText.Text = errorDetails;
        }
    }

    private void ApplyTranslations(LocalizationService localizationService)
    {
        Title = localizationService.GetString("AutomaticSendFailed") ?? "Automatischer Versand fehlgeschlagen";
        TitleText.Text = localizationService.GetString("AutomaticSendFailed") ?? "Automatischer Versand fehlgeschlagen";
        MessageText.Text = localizationService.GetString("FallbackToMailClient") ?? 
            "Die E-Mail konnte nicht automatisch versendet werden.\n\n" +
            "Möchten Sie Ihren E-Mail-Client öffnen, um die Anfrage manuell zu versenden?";
        YesButton.Content = localizationService.GetString("OpenEmailClient") ?? "E-Mail-Client öffnen";
        NoButton.Content = localizationService.GetString("Cancel") ?? "Abbrechen";
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// Zeigt den Fehler-Dialog an
    /// </summary>
    public static bool? ShowDialog(Window owner, LocalizationService localizationService, string? errorDetails = null)
    {
        var dialog = new EmailErrorDialog(localizationService, errorDetails)
        {
            Owner = owner
        };
        return dialog.ShowDialog();
    }
}
