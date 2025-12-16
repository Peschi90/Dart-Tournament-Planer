using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Dialog für die Bestätigung der direkten KO-Phasen-Generierung
/// Zeigt Spieleranzahl, Modus und Warnung an
/// </summary>
public partial class GenerateKnockoutDialog : Window
{
    private readonly LocalizationService? _localizationService;
    
    public int PlayerCount { get; set; }
    public string KnockoutMode { get; set; } = string.Empty;
    
    public GenerateKnockoutDialog(int playerCount, string knockoutMode, LocalizationService? localizationService = null)
    {
        InitializeComponent();
        _localizationService = localizationService;
        
        PlayerCount = playerCount;
        KnockoutMode = knockoutMode;
        
        UpdateTranslations();
        UpdateContent();
    }
    
    private void UpdateTranslations()
    {
        if (_localizationService == null) return;
        
        try
        {
            // Title
            Title = _localizationService.GetString("GenerateKnockout") ?? "KO-Phase generieren";
            TitleTextBlock.Text = Title;
            
            // Question
            QuestionTextBlock.Text = _localizationService.GetString("ConfirmDirectKnockoutQuestion") 
                ?? "Möchten Sie die KO-Phase direkt starten?";
            
            // Labels
            PlayersLabel.Text = _localizationService.GetString("Players") ?? "Spieler:";
            ModeLabel.Text = _localizationService.GetString("KnockoutMode") ?? "Modus:";
            NoGroupPhaseWarningText.Text = _localizationService.GetString("NoGroupPhase") ?? "Keine Gruppenphase";
            
            // Info
            InfoHeaderText.Text = _localizationService.GetString("Information") ?? "Information";
            InfoDetailsText.Text = _localizationService.GetString("DirectKnockoutInfo") 
                ?? "Das Turnier beginnt direkt mit dem K.O.-System. Alle Spieler werden automatisch ins Bracket eingefügt.";
            
            // Buttons
            CancelButton.Content = _localizationService.GetString("Cancel") ?? "✗ Abbrechen";
            ContinueButton.Content = _localizationService.GetString("Continue") ?? "✓ Fortfahren";
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating translations in GenerateKnockoutDialog: {ex.Message}");
        }
    }
    
    private void UpdateContent()
    {
        PlayersValueText.Text = PlayerCount.ToString();
        ModeValueText.Text = KnockoutMode;
    }
    
    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
