using System.Windows;
using System.Windows.Media;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views.Common;

public partial class StyledInfoDialog : Window
{
    public StyledInfoDialog(string title, string message, LocalizationService localization, bool isError = false, bool isSuccess = false)
    {
        InitializeComponent();
        TitleBlock.Text = title;
        MessageBlock.Text = message;
        OkButton.Content = localization.GetString("OK") ?? "OK";

        if (isError)
        {
            HeaderBorder.Background = (Brush)FindResource("ErrorGradient");
            TitleBlock.Text = "⚠️ " + TitleBlock.Text;
        }
        else if (isSuccess)
        {
            HeaderBorder.Background = (Brush)FindResource("SuccessGradient");
            TitleBlock.Text = "✅ " + TitleBlock.Text;
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    public static void Show(string title, string message, LocalizationService localization, bool isError = false, bool isSuccess = false, Window? owner = null)
    {
        var dlg = new StyledInfoDialog(title, message, localization, isError, isSuccess)
        {
            Owner = owner
        };
        dlg.ShowDialog();
    }
}
