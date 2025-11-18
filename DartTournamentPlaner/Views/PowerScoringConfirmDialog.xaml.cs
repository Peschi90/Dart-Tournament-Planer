using System.Windows;
using System.Windows.Media;

namespace DartTournamentPlaner.Views;

public enum DialogType
{
    Question,
    Information,
    Warning,
    Error,
    Success
}

public partial class PowerScoringConfirmDialog : Window
{
    public bool Result { get; private set; }

    public PowerScoringConfirmDialog(string title, string message, DialogType type = DialogType.Question)
    {
        InitializeComponent();
        
        TitleText.Text = title;
        MessageText.Text = message;
        
        // ✅ NEU: Setze dynamische Farben basierend auf Typ
        SetDialogStyle(type);
        
        // Buttons anpassen
        if (type == DialogType.Information || type == DialogType.Success || type == DialogType.Error || type == DialogType.Warning)
        {
            // Nur OK Button
            NoButton.Visibility = Visibility.Collapsed;
            YesButton.Content = "✅ OK";
        }
    }
    
    /// <summary>
    /// ✅ NEU: Setzt Style basierend auf Dialog-Typ
    /// </summary>
    private void SetDialogStyle(DialogType type)
    {
        LinearGradientBrush gradient;
        string icon;
        
        switch (type)
        {
            case DialogType.Success:
                gradient = (LinearGradientBrush)Resources["SuccessGradient"];
                icon = "✅";
                break;
                
            case DialogType.Warning:
                gradient = (LinearGradientBrush)Resources["WarningGradient"];
                icon = "⚠️";
                break;
                
            case DialogType.Error:
                gradient = (LinearGradientBrush)Resources["ErrorGradient"];
                icon = "❌";
                break;
                
            case DialogType.Information:
                gradient = (LinearGradientBrush)Resources["InfoGradient"];
                icon = "ℹ️";
                break;
                
            default: // Question
                gradient = (LinearGradientBrush)Resources["QuestionGradient"];
                icon = "❓";
                break;
        }
        
        HeaderBorder.Background = gradient;
        IconText.Text = icon;
        
        // Aktualisiere Primary Button Farbe
        YesButton.Background = gradient;
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }

    // Question Dialog (Ja/Nein)
    public static bool ShowQuestion(string title, string message, Window? owner = null)
    {
        var dialog = new PowerScoringConfirmDialog(title, message, DialogType.Question);
        if (owner != null && owner.IsLoaded) dialog.Owner = owner;
        dialog.ShowDialog();
        return dialog.Result;
    }
    
    // Information Dialog (OK)
    public static void ShowInformation(string title, string message, Window? owner = null)
    {
        var dialog = new PowerScoringConfirmDialog(title, message, DialogType.Information);
        if (owner != null && owner.IsLoaded) dialog.Owner = owner;
        dialog.ShowDialog();
    }
    
    // Warning Dialog (OK)
    public static void ShowWarning(string title, string message, Window? owner = null)
    {
        var dialog = new PowerScoringConfirmDialog(title, message, DialogType.Warning);
        if (owner != null && owner.IsLoaded) dialog.Owner = owner;
        dialog.ShowDialog();
    }
    
    // Error Dialog (OK)
    public static void ShowError(string title, string message, Window? owner = null)
    {
        var dialog = new PowerScoringConfirmDialog(title, message, DialogType.Error);
        if (owner != null && owner.IsLoaded) dialog.Owner = owner;
        dialog.ShowDialog();
    }
    
    // Success Dialog (OK)
    public static void ShowSuccess(string title, string message, Window? owner = null)
    {
        var dialog = new PowerScoringConfirmDialog(title, message, DialogType.Success);
        if (owner != null && owner.IsLoaded) dialog.Owner = owner;
        dialog.ShowDialog();
    }
}
