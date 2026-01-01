using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DartTournamentPlaner.Helpers;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class RoundRulesWindow : Window
{
    private readonly GameRules _gameRules;
    private readonly LocalizationService _localizationService;
    private readonly Dictionary<KnockoutRound, (TextBox SetsBox, TextBox LegsBox, TextBox LegsPerSetBox)> _winnerBracketControls;
    private readonly Dictionary<KnockoutRound, (TextBox SetsBox, TextBox LegsBox, TextBox LegsPerSetBox)> _loserBracketControls;
    private readonly Dictionary<RoundRobinFinalsRound, (TextBox SetsBox, TextBox LegsBox, TextBox LegsPerSetBox)> _roundRobinFinalsControls;

    // Event für Datenänderungen
    public event EventHandler? DataChanged;

    public RoundRulesWindow(GameRules gameRules, LocalizationService localizationService)
    {
        _gameRules = gameRules;
        _localizationService = localizationService;
        _winnerBracketControls = new Dictionary<KnockoutRound, (TextBox, TextBox, TextBox)>();
        _loserBracketControls = new Dictionary<KnockoutRound, (TextBox, TextBox, TextBox)>();
        _roundRobinFinalsControls = new Dictionary<RoundRobinFinalsRound, (TextBox, TextBox, TextBox)>();
        
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
        RoundRobinFinalsTab.Header = _localizationService.GetString("RoundRobinFinalsRules");
        
        WinnerBracketHeaderText.Text = _localizationService.GetString("WinnerBracketRules");
        LoserBracketHeaderText.Text = _localizationService.GetString("LoserBracketRules");
        RoundRobinFinalsHeaderText.Text = _localizationService.GetString("RoundRobinFinalsRules");
        
        ResetToDefaultButton.Content = _localizationService.GetString("ResetToDefault");
        SaveButton.Content = _localizationService.GetString("Save");
        CancelButton.Content = _localizationService.GetString("Cancel");
    }

    private void CreateRoundControls()
    {
        CreateWinnerBracketControls();
        CreateLoserBracketControls();
        CreateRoundRobinFinalsControls();
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
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 8, 5, 8),
                Foreground = System.Windows.Media.Brushes.DarkSlateGray
            };
            Grid.SetRow(nameBlock, row);
            Grid.SetColumn(nameBlock, 0);
            WinnerBracketGrid.Children.Add(nameBlock);
            
            // Sets TextBox
            var setsBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
            Grid.SetRow(setsBox, row);
            Grid.SetColumn(setsBox, 1);
            WinnerBracketGrid.Children.Add(setsBox);
            
            // Legs TextBox
            var legsBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
            Grid.SetRow(legsBox, row);
            Grid.SetColumn(legsBox, 2);
            WinnerBracketGrid.Children.Add(legsBox);
            
            // Legs per Set TextBox
            var legsPerSetBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
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
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 8, 5, 8),
                Foreground = System.Windows.Media.Brushes.DarkSlateGray
            };
            Grid.SetRow(nameBlock, row);
            Grid.SetColumn(nameBlock, 0);
            LoserBracketGrid.Children.Add(nameBlock);
            
            // Sets TextBox
            var setsBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
            Grid.SetRow(setsBox, row);
            Grid.SetColumn(setsBox, 1);
            LoserBracketGrid.Children.Add(setsBox);
            
            // Legs TextBox
            var legsBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
            Grid.SetRow(legsBox, row);
            Grid.SetColumn(legsBox, 2);
            LoserBracketGrid.Children.Add(legsBox);
            
            // Legs per Set TextBox
            var legsPerSetBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
            Grid.SetRow(legsPerSetBox, row);
            Grid.SetColumn(legsPerSetBox, 3);
            LoserBracketGrid.Children.Add(legsPerSetBox);
            
            _loserBracketControls[round] = (setsBox, legsBox, legsPerSetBox);
            row++;
        }
    }

    private void CreateRoundRobinFinalsControls()
    {
        var finalsRounds = new[]
        {
            RoundRobinFinalsRound.Finals
        };

        int row = 1; // Start after headers
        foreach (var round in finalsRounds)
        {
            RoundRobinFinalsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            // Round name
            var roundName = GetRoundRobinFinalsDisplayName(round);
            var nameBlock = new TextBlock 
            { 
                Text = roundName,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 8, 5, 8),
                Foreground = System.Windows.Media.Brushes.DarkSlateGray
            };
            Grid.SetRow(nameBlock, row);
            Grid.SetColumn(nameBlock, 0);
            RoundRobinFinalsGrid.Children.Add(nameBlock);
            
            // Sets TextBox
            var setsBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
            Grid.SetRow(setsBox, row);
            Grid.SetColumn(setsBox, 1);
            RoundRobinFinalsGrid.Children.Add(setsBox);
            
            // Legs TextBox
            var legsBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
            Grid.SetRow(legsBox, row);
            Grid.SetColumn(legsBox, 2);
            RoundRobinFinalsGrid.Children.Add(legsBox);
            
            // Legs per Set TextBox
            var legsPerSetBox = new TextBox { Margin = new Thickness(5, 4, 5, 4) };
            Grid.SetRow(legsPerSetBox, row);
            Grid.SetColumn(legsPerSetBox, 3);
            RoundRobinFinalsGrid.Children.Add(legsPerSetBox);
            
            _roundRobinFinalsControls[round] = (setsBox, legsBox, legsPerSetBox);
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
            KnockoutRound.Final => _localizationService.GetString("Final") + " (Winner Bracket)",
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
            KnockoutRound.LoserFinal => _localizationService.GetString("Final") + " (Loser Bracket)",
            _ => round.ToString()
        };
    }

    private string GetRoundRobinFinalsDisplayName(RoundRobinFinalsRound round)
    {
        return round switch
        {
            RoundRobinFinalsRound.Finals => _localizationService.GetString("RoundRobinFinals"),
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

        // Load Round Robin Finals values
        foreach (var kvp in _roundRobinFinalsControls)
        {
            var rules = _gameRules.GetRulesForRoundRobinFinals(kvp.Key);
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

            // Save Round Robin Finals rules
            foreach (var kvp in _roundRobinFinalsControls)
            {
                if (int.TryParse(kvp.Value.SetsBox.Text, out var sets) && sets >= 0 &&  // Erlaube 0 für Sets
                    int.TryParse(kvp.Value.LegsBox.Text, out var legs) && legs > 0 &&
                    int.TryParse(kvp.Value.LegsPerSetBox.Text, out var legsPerSet) && legsPerSet > 0)
                {
                    _gameRules.SetRulesForRoundRobinFinals(kvp.Key, sets, legs, legsPerSet);
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
            TournamentDialogHelper.ShowError($"Error saving round rules: {ex.Message}", "Error", _localizationService, this);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
    {
        var result = TournamentDialogHelper.ShowConfirmation(
            this,
            _localizationService.GetString("ResetToDefault"),
            _localizationService.GetString("ResetToDefault") + "?",
            "??",
            true,
            _localizationService);

        if (result)
        {
            _gameRules.ResetKnockoutRulesToDefault();
            _gameRules.ResetRoundRobinFinalsRulesToDefault();
            LoadCurrentValues();
        }
    }

    /// <summary>
    /// Event-Handler für das Verschieben des Fensters über den Header
    /// </summary>
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}