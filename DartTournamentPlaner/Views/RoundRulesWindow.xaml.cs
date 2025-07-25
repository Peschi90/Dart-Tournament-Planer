using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class RoundRulesWindow : Window
{
    private readonly GameRules _gameRules;
    private readonly LocalizationService _localizationService;
    private readonly Dictionary<KnockoutRound, (TextBox SetsBox, TextBox LegsBox, TextBox LegsPerSetBox)> _winnerBracketControls;
    private readonly Dictionary<KnockoutRound, (TextBox SetsBox, TextBox LegsBox, TextBox LegsPerSetBox)> _loserBracketControls;

    // Event für Datenänderungen
    public event EventHandler? DataChanged;

    public RoundRulesWindow(GameRules gameRules, LocalizationService localizationService)
    {
        _gameRules = gameRules;
        _localizationService = localizationService;
        _winnerBracketControls = new Dictionary<KnockoutRound, (TextBox, TextBox, TextBox)>();
        _loserBracketControls = new Dictionary<KnockoutRound, (TextBox, TextBox, TextBox)>();
        
        InitializeComponent();
        UpdateTranslations();
        CreateRoundControls();
        LoadCurrentValues();
    }

    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("RoundRulesConfiguration");
        TitleBlock.Text = _localizationService.GetString("RoundRulesConfiguration");
        
        WinnerBracketTab.Header = _localizationService.GetString("WinnerBracketRules");
        LoserBracketTab.Header = _localizationService.GetString("LoserBracketRules");
        
        WinnerBracketHeaderText.Text = _localizationService.GetString("WinnerBracketRules");
        LoserBracketHeaderText.Text = _localizationService.GetString("LoserBracketRules");
        
        ResetToDefaultButton.Content = _localizationService.GetString("ResetToDefault");
        SaveButton.Content = _localizationService.GetString("Save");
        CancelButton.Content = _localizationService.GetString("Cancel");
    }

    private void CreateRoundControls()
    {
        CreateWinnerBracketControls();
        CreateLoserBracketControls();
    }

    private void CreateWinnerBracketControls()
    {
        var winnerRounds = new[]
        {
            KnockoutRound.Best64,
            KnockoutRound.Best32,
            KnockoutRound.Best16,
            KnockoutRound.Quarterfinal,
            KnockoutRound.Semifinal,
            KnockoutRound.Final,
            KnockoutRound.GrandFinal
        };

        int row = 1; // Start after headers
        foreach (var round in winnerRounds)
        {
            WinnerBracketGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Round name
            var roundName = GetRoundDisplayName(round);
            var nameBlock = new TextBlock 
            { 
                Text = roundName,
                FontWeight = FontWeights.Medium
            };
            Grid.SetRow(nameBlock, row);
            Grid.SetColumn(nameBlock, 0);
            WinnerBracketGrid.Children.Add(nameBlock);
            
            // Sets TextBox
            var setsBox = new TextBox();
            Grid.SetRow(setsBox, row);
            Grid.SetColumn(setsBox, 1);
            WinnerBracketGrid.Children.Add(setsBox);
            
            // Legs TextBox
            var legsBox = new TextBox();
            Grid.SetRow(legsBox, row);
            Grid.SetColumn(legsBox, 2);
            WinnerBracketGrid.Children.Add(legsBox);
            
            // Legs per Set TextBox
            var legsPerSetBox = new TextBox();
            Grid.SetRow(legsPerSetBox, row);
            Grid.SetColumn(legsPerSetBox, 3);
            WinnerBracketGrid.Children.Add(legsPerSetBox);
            
            _winnerBracketControls[round] = (setsBox, legsBox, legsPerSetBox);
            row++;
        }
    }

    private void CreateLoserBracketControls()
    {
        var loserRounds = new[]
        {
            KnockoutRound.LoserRound1,
            KnockoutRound.LoserRound2,
            KnockoutRound.LoserRound3,
            KnockoutRound.LoserRound4,
            KnockoutRound.LoserRound5,
            KnockoutRound.LoserRound6,
            KnockoutRound.LoserRound7,
            KnockoutRound.LoserRound8,
            KnockoutRound.LoserRound9,
            KnockoutRound.LoserRound10,
            KnockoutRound.LoserRound11,
            KnockoutRound.LoserRound12,
            KnockoutRound.LoserFinal
        };

        int row = 1; // Start after headers
        foreach (var round in loserRounds)
        {
            LoserBracketGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Round name
            var roundName = GetRoundDisplayName(round);
            var nameBlock = new TextBlock 
            { 
                Text = roundName,
                FontWeight = FontWeights.Medium
            };
            Grid.SetRow(nameBlock, row);
            Grid.SetColumn(nameBlock, 0);
            LoserBracketGrid.Children.Add(nameBlock);
            
            // Sets TextBox
            var setsBox = new TextBox();
            Grid.SetRow(setsBox, row);
            Grid.SetColumn(setsBox, 1);
            LoserBracketGrid.Children.Add(setsBox);
            
            // Legs TextBox
            var legsBox = new TextBox();
            Grid.SetRow(legsBox, row);
            Grid.SetColumn(legsBox, 2);
            LoserBracketGrid.Children.Add(legsBox);
            
            // Legs per Set TextBox
            var legsPerSetBox = new TextBox();
            Grid.SetRow(legsPerSetBox, row);
            Grid.SetColumn(legsPerSetBox, 3);
            LoserBracketGrid.Children.Add(legsPerSetBox);
            
            _loserBracketControls[round] = (setsBox, legsBox, legsPerSetBox);
            row++;
        }
    }

    private string GetRoundDisplayName(KnockoutRound round)
    {
        return round switch
        {
            KnockoutRound.Best64 => _localizationService.GetString("Best64"),
            KnockoutRound.Best32 => _localizationService.GetString("Best32"),
            KnockoutRound.Best16 => _localizationService.GetString("Best16") + " (Achtelfinale)",
            KnockoutRound.Quarterfinal => _localizationService.GetString("Quarterfinal"),
            KnockoutRound.Semifinal => _localizationService.GetString("Semifinal"),
            KnockoutRound.Final => _localizationService.GetString("Final"),
            KnockoutRound.GrandFinal => _localizationService.GetString("GrandFinal"),
            KnockoutRound.LoserRound1 => "LR1",
            KnockoutRound.LoserRound2 => "LR2",
            KnockoutRound.LoserRound3 => "LR3",
            KnockoutRound.LoserRound4 => "LR4",
            KnockoutRound.LoserRound5 => "LR5",
            KnockoutRound.LoserRound6 => "LR6",
            KnockoutRound.LoserRound7 => "LR7",
            KnockoutRound.LoserRound8 => "LR8",
            KnockoutRound.LoserRound9 => "LR9",
            KnockoutRound.LoserRound10 => "LR10",
            KnockoutRound.LoserRound11 => "LR11",
            KnockoutRound.LoserRound12 => "LR12",
            KnockoutRound.LoserFinal => _localizationService.GetString("LoserBracket") + " " + _localizationService.GetString("Final"),
            _ => round.ToString()
        };
    }

    private void LoadCurrentValues()
    {
        // Load Winner Bracket values
        foreach (var kvp in _winnerBracketControls)
        {
            var rules = _gameRules.GetRulesForRound(kvp.Key);
            kvp.Value.SetsBox.Text = rules.SetsToWin.ToString();
            kvp.Value.LegsBox.Text = rules.LegsToWin.ToString();
            kvp.Value.LegsPerSetBox.Text = rules.LegsPerSet.ToString();
        }

        // Load Loser Bracket values
        foreach (var kvp in _loserBracketControls)
        {
            var rules = _gameRules.GetRulesForRound(kvp.Key);
            kvp.Value.SetsBox.Text = rules.SetsToWin.ToString();
            kvp.Value.LegsBox.Text = rules.LegsToWin.ToString();
            kvp.Value.LegsPerSetBox.Text = rules.LegsPerSet.ToString();
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("RoundRulesWindow.SaveButton_Click: START");
            
            // Save Winner Bracket rules
            foreach (var kvp in _winnerBracketControls)
            {
                if (int.TryParse(kvp.Value.SetsBox.Text, out var sets) && sets >= 0 &&  // Erlaube 0 für Sets
                    int.TryParse(kvp.Value.LegsBox.Text, out var legs) && legs > 0 &&
                    int.TryParse(kvp.Value.LegsPerSetBox.Text, out var legsPerSet) && legsPerSet > 0)
                {
                    _gameRules.SetRulesForRound(kvp.Key, sets, legs, legsPerSet);
                }
            }

            // Save Loser Bracket rules
            foreach (var kvp in _loserBracketControls)
            {
                if (int.TryParse(kvp.Value.SetsBox.Text, out var sets) && sets >= 0 &&  // Erlaube 0 für Sets
                    int.TryParse(kvp.Value.LegsBox.Text, out var legs) && legs > 0 &&
                    int.TryParse(kvp.Value.LegsPerSetBox.Text, out var legsPerSet) && legsPerSet > 0)
                {
                    _gameRules.SetRulesForRound(kvp.Key, sets, legs, legsPerSet);
                }
            }

            System.Diagnostics.Debug.WriteLine("RoundRulesWindow.SaveButton_Click: Rules saved, triggering DataChanged event");
            
            // Trigger data changed event
            DataChanged?.Invoke(this, EventArgs.Empty);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving round rules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            _localizationService.GetString("ResetToDefault") + "?",
            _localizationService.GetString("ResetToDefault"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _gameRules.ResetKnockoutRulesToDefault();
            LoadCurrentValues();
        }
    }
}