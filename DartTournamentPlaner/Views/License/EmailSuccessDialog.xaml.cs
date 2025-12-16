using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views.License;

/// <summary>
/// Dialog für erfolgreiche E-Mail-Sendung im modernen Design
/// </summary>
public partial class EmailSuccessDialog : Window
{
    public EmailSuccessDialog(LocalizationService localizationService)
    {
        InitializeComponent();
        ApplyTranslations(localizationService);
    }

    private void ApplyTranslations(LocalizationService localizationService)
    {
        Title = localizationService.GetString("Success") ?? "Erfolg";
        TitleText.Text = localizationService.GetString("EmailSentSuccessTitle") ?? "E-Mail erfolgreich versendet!";
        OkButton.Content = "OK";
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// Zeigt den Erfolgs-Dialog an
    /// </summary>
    public static bool? ShowDialog(Window owner, LocalizationService localizationService)
    {
        var dialog = new EmailSuccessDialog(localizationService)
        {
            Owner = owner
        };
        return dialog.ShowDialog();
    }
}
