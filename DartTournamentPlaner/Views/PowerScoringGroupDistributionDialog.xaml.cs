using System.Text;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models.PowerScore;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.PowerScore;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Dialog zur Anzeige und Verwaltung der Gruppeneinteilung nach PowerScoring
/// </summary>
public partial class PowerScoringGroupDistributionDialog : Window
{
    private readonly PowerScoringService _powerScoringService;
    private readonly LocalizationService _localizationService;
    private Dictionary<int, List<PowerScoringPlayer>> _currentDistribution = new();

    public PowerScoringGroupDistributionDialog(
        PowerScoringService powerScoringService, 
        LocalizationService localizationService)
    {
        InitializeComponent();
        
        _powerScoringService = powerScoringService;
        _localizationService = localizationService;

        DistributeToGroups();
    }

    private void GroupCountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DistributeToGroups();
    }

    private void RedistributeButton_Click(object sender, RoutedEventArgs e)
    {
        DistributeToGroups();
    }

    private void DistributeToGroups()
    {
        if (GroupCountComboBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content.ToString(), out int groupCount))
        {
            _currentDistribution = _powerScoringService.DistributePlayersToGroups(groupCount);
            DisplayGroups();
        }
    }

    private void DisplayGroups()
    {
        var groupDisplayData = _currentDistribution
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => new
            {
                GroupName = GetGroupName(kvp.Key),
                Players = kvp.Value
            })
            .ToList();

        GroupsDisplay.ItemsSource = groupDisplayData;
        
        System.Diagnostics.Debug.WriteLine($"📋 Gruppeneinteilung angezeigt: {_currentDistribution.Count} Gruppen");
    }

    private string GetGroupName(int groupNumber)
    {
        return groupNumber switch
        {
            1 => "🏆 Platin",
            2 => "🥇 Gold",
            3 => "🥈 Silber",
            4 => "🥉 Bronze",
            _ => $"Gruppe {groupNumber}"
        };
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== PowerScoring Gruppeneinteilung ===");
        sb.AppendLine();

        foreach (var group in _currentDistribution.OrderBy(kvp => kvp.Key))
        {
            sb.AppendLine($"{GetGroupName(group.Key)}:");
            sb.AppendLine(new string('-', 40));
            
            foreach (var player in group.Value)
            {
                sb.AppendLine($"  • {player.Name} - Ø {player.AverageScore:F2} (Gesamt: {player.TotalScore})");
            }
            
            sb.AppendLine();
        }

        try
        {
            Clipboard.SetText(sb.ToString());
            PowerScoringConfirmDialog.ShowSuccess(
                "Erfolg",
                "Gruppeneinteilung wurde in die Zwischenablage kopiert.",
                this);
        }
        catch (Exception ex)
        {
            PowerScoringConfirmDialog.ShowError(
                "Fehler",
                $"Fehler beim Kopieren: {ex.Message}",
                this);
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
