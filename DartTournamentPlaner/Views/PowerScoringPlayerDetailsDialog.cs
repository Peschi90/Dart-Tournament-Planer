using System.Windows;
using DartTournamentPlaner.Models.PowerScore;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Modern Dialog für PowerScoring Player Details
/// </summary>
public partial class PowerScoringPlayerDetailsDialog : Window
{
    public PowerScoringPlayerDetailsDialog(PowerScoringPlayer player)
    {
        InitializeComponent();
        
        LoadPlayerData(player);
    }
    
    private void LoadPlayerData(PowerScoringPlayer player)
    {
        // Title
        TitleText.Text = $"{player.Name} - Details";
        
        // Statistics
        TotalScoreText.Text = player.TotalScore.ToString();
        AverageText.Text = player.AverageScore.ToString("F2");
        HighestThrowText.Text = player.HighestThrow.ToString();
        TotalDartsText.Text = player.TotalDarts.ToString();
        
        // Session Info
        RoundsText.Text = player.History.Count.ToString();
        
        if (player.SessionStartTime.HasValue && player.CompletionTime.HasValue)
        {
            var duration = (player.CompletionTime.Value - player.SessionStartTime.Value).TotalSeconds;
            DurationText.Text = $"{duration:F1}s";
        }
        else
        {
            DurationText.Text = "N/A";
        }
        
        SubmittedViaText.Text = string.IsNullOrEmpty(player.SubmittedVia) ? "N/A" : player.SubmittedVia;
        
        // Throw History
        var historyItems = new List<ThrowHistoryItem>();
        
        foreach (var round in player.History)
        {
            string throws = "";
            
            if (round.Darts != null && round.Darts.Count > 0)
            {
                // Detaillierte Dart-Infos
                throws = string.Join(", ", round.Darts.Select(d => d.DisplayValue));
            }
            else if (round.Throw1 > 0 || round.Throw2 > 0 || round.Throw3 > 0)
            {
                // Fallback auf einfache Scores
                throws = $"{round.Throw1} + {round.Throw2} + {round.Throw3}";
            }
            else
            {
                throws = "No data";
            }
            
            historyItems.Add(new ThrowHistoryItem
            {
                RoundLabel = $"R{round.Round}",
                Throws = throws,
                TotalLabel = $"= {round.Total}",
                TimeLabel = round.Timestamp?.ToString("HH:mm:ss") ?? ""
            });
        }
        
        HistoryItemsControl.ItemsSource = historyItems;
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    /// <summary>
    /// Zeigt den Dialog für einen Spieler
    /// </summary>
    public static void Show(PowerScoringPlayer player, Window? owner = null)
    {
        var dialog = new PowerScoringPlayerDetailsDialog(player);
        if (owner != null && owner.IsLoaded)
        {
            dialog.Owner = owner;
        }
        dialog.ShowDialog();
    }
}

/// <summary>
/// View Model für Throw History Items
/// </summary>
public class ThrowHistoryItem
{
    public string RoundLabel { get; set; } = "";
    public string Throws { get; set; } = "";
    public string TotalLabel { get; set; } = "";
    public string TimeLabel { get; set; } = "";
}
