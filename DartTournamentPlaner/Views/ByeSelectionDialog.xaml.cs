using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Dialog zur Auswahl des Freilos-Gewinners
/// </summary>
public partial class ByeSelectionDialog : Window
{
    public Player? SelectedPlayer { get; private set; }

    public ByeSelectionDialog(KnockoutMatch match, LocalizationService? localizationService)
    {
        InitializeComponent();
        Title = localizationService?.GetString("GiveBye") ?? "Give Bye";
        
        var message = localizationService?.GetString("SelectByeWinner") ?? "Select the player who should receive the bye:";
        MessageTextBlock.Text = message;
        
        var cancelText = localizationService?.GetString("Cancel") ?? "Cancel";
        CancelButton.Content = cancelText;
        
        if (match.Player1 != null)
        {
            Player1Button.Content = match.Player1.Name;
            Player1Button.Tag = match.Player1;
        }
        else
        {
            Player1Button.Visibility = Visibility.Collapsed;
        }
        
        if (match.Player2 != null)
        {
            Player2Button.Content = match.Player2.Name;
            Player2Button.Tag = match.Player2;
        }
        else
        {
            Player2Button.Visibility = Visibility.Collapsed;
        }
    }

    private void Player1Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Player player)
        {
            SelectedPlayer = player;
            DialogResult = true;
            Close();
        }
    }

    private void Player2Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Player player)
        {
            SelectedPlayer = player;
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}