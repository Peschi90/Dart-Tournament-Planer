using System.Windows;
using System.Windows.Controls;
using System.Linq;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.PowerScore;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Bestätigungs-Dialog für die Erstellung eines Turniers aus PowerScoring Distribution
/// 
/// VERANTWORTLICHKEIT:
/// - Zeigt Vorschau der zu erstellenden Turnier-Struktur
/// - Warnt vor Überschreiben eines bestehenden Turniers
/// - Sammelt User-Bestätigung
/// 
/// NICHT VERANTWORTLICH FÜR:
/// - Tatsächliches Erstellen des Turniers (-> TournamentManagementService)
/// - Konvertierung der Daten (-> PowerScoringToTournamentService)
/// </summary>
public partial class PowerScoringCreateTournamentDialog : Window
{
    public bool UserConfirmed { get; private set; }
    
    private readonly TournamentPreview _preview;
    private readonly LocalizationService _localizationService;
    private readonly bool _hasPendingTournament;

    public PowerScoringCreateTournamentDialog(
        TournamentPreview preview,
        bool hasPendingTournament,
        LocalizationService localizationService)
    {
        InitializeComponent();
        
        _preview = preview;
        _hasPendingTournament = hasPendingTournament;
        _localizationService = localizationService;
        
        LoadPreview();
        UpdateTranslations();
    }
    
    private void LoadPreview()
    {
        // Summary
        SummaryText.Text = _preview.GetSummary();
        
        // Warnung wenn bestehendes Turnier
        if (_hasPendingTournament)
        {
            WarningPanel.Visibility = Visibility.Visible;
        }
        
        // Details
        var detailsText = "";
        foreach (var classPreview in _preview.Classes)
        {
            detailsText += $"\n🏆 {classPreview.ClassName}\n";
            detailsText += $"   Gruppen: {classPreview.GroupCount}, Spieler: {classPreview.TotalPlayers}\n";
            
            foreach (var group in classPreview.Groups)
            {
                detailsText += $"   • Gruppe {group.GroupNumber}: {group.PlayerCount} Spieler\n";
                foreach (var player in group.Players.Take(3)) // Nur erste 3 zeigen
                {
                    detailsText += $"      - {player}\n";
                }
                if (group.Players.Count > 3)
                {
                    detailsText += $"      ... und {group.Players.Count - 3} weitere\n";
                }
            }
        }
        
        DetailsText.Text = detailsText.Trim();
    }
    
    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("PowerScoring_CreateTournament_Title");
        
        // Buttons
        CancelButton.Content = _localizationService.GetString("Cancel");
        CreateButton.Content = _localizationService.GetString("PowerScoring_CreateTournament_Create");
    }
    
    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        UserConfirmed = true;
        Close();
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        UserConfirmed = false;
        Close();
    }
    
    public static bool? ShowDialog(
        TournamentPreview preview,
        bool hasPendingTournament,
        LocalizationService localizationService,
        Window? owner = null)
    {
        var dialog = new PowerScoringCreateTournamentDialog(preview, hasPendingTournament, localizationService);
        if (owner != null && owner.IsLoaded)
        {
            dialog.Owner = owner;
        }
        dialog.ShowDialog();
        return dialog.UserConfirmed;
    }
}
