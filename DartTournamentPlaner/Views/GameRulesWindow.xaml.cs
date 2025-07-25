using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class GameRulesWindow : Window
{
    private readonly GameRules _gameRules;
    private readonly LocalizationService _localizationService;

    // Event für Datenänderungen
    public event EventHandler? DataChanged;

    public GameRulesWindow(GameRules gameRules, LocalizationService localizationService)
    {
        _gameRules = gameRules;
        _localizationService = localizationService;
        
        InitializeComponent();
        InitializeUI();
        UpdateTranslations();
        
        _gameRules.PropertyChanged += GameRules_PropertyChanged;
    }

    private void InitializeUI()
    {
        // Set initial values
        GameModeComboBox.SelectedValue = _gameRules.GameMode.ToString();
        FinishModeComboBox.SelectedValue = _gameRules.FinishMode.ToString();
        LegsToWinTextBox.Text = _gameRules.LegsToWin.ToString();
        PlayWithSetsCheckBox.IsChecked = _gameRules.PlayWithSets;
        SetsToWinTextBox.Text = _gameRules.SetsToWin.ToString();
        LegsPerSetTextBox.Text = _gameRules.LegsPerSet.ToString();
        
        // Set post-group phase values
        PostGroupPhaseModeComboBox.SelectedValue = _gameRules.PostGroupPhaseMode.ToString();
        QualifyingPlayersTextBox.Text = _gameRules.QualifyingPlayersPerGroup.ToString();
        KnockoutModeComboBox.SelectedValue = _gameRules.KnockoutMode.ToString();
        IncludeGroupPhaseLosersBracketCheckBox.IsChecked = _gameRules.IncludeGroupPhaseLosersBracket;
        
        UpdateVisibility();
        UpdatePreview();
    }

    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("GameRules");
        TitleBlock.Text = _localizationService.GetString("GameRules");
        
        GameModeLabel.Text = _localizationService.GetString("GameMode") + ":";
        FinishModeLabel.Text = _localizationService.GetString("FinishMode") + ":";
        LegsToWinLabel.Text = _localizationService.GetString("LegsToWin") + ":";
        PlayWithSetsLabel.Text = _localizationService.GetString("PlayWithSets") + ":";
        SetsToWinLabel.Text = _localizationService.GetString("SetsToWin") + ":";
        LegsPerSetLabel.Text = _localizationService.GetString("LegsPerSet") + ":";
        
        // Post-group phase translations
        PostGroupPhaseModeLabel.Text = _localizationService.GetString("PostGroupPhaseMode") + ":";
        QualifyingPlayersLabel.Text = _localizationService.GetString("QualifyingPlayersPerGroup") + ":";
        KnockoutModeLabel.Text = _localizationService.GetString("KnockoutMode") + ":";
        IncludeGroupPhaseLosersBracketCheckBox.Content = _localizationService.GetString("IncludeGroupPhaseLosersBracket");
        
        PreviewLabel.Text = _localizationService.GetString("RulesPreview") + ":";
        SaveButton.Content = _localizationService.GetString("Save");
        CancelButton.Content = _localizationService.GetString("Cancel");
        ConfigureRoundRulesButton.Content = _localizationService.GetString("ConfigureRoundRules");
        
        UpdateComboBoxContents();
    }

    private void UpdateComboBoxContents()
    {
        // Update game mode combo box
        foreach (ComboBoxItem item in GameModeComboBox.Items)
        {
            var tag = item.Tag?.ToString();
            item.Content = tag switch
            {
                "Points501" => _localizationService.GetString("Points501"),
                "Points401" => _localizationService.GetString("Points401"),
                "Points301" => _localizationService.GetString("Points301"),
                _ => item.Content
            };
        }

        // Update finish mode combo box
        foreach (ComboBoxItem item in FinishModeComboBox.Items)
        {
            var tag = item.Tag?.ToString();
            item.Content = tag switch
            {
                "SingleOut" => _localizationService.GetString("SingleOut"),
                "DoubleOut" => _localizationService.GetString("DoubleOut"),
                _ => item.Content
            };
        }

        // Update post-group phase mode combo box
        foreach (ComboBoxItem item in PostGroupPhaseModeComboBox.Items)
        {
            var tag = item.Tag?.ToString();
            item.Content = tag switch
            {
                "None" => _localizationService.GetString("PostGroupPhaseNone"),
                "RoundRobinFinals" => _localizationService.GetString("PostGroupPhaseRoundRobin"),
                "KnockoutBracket" => _localizationService.GetString("PostGroupPhaseKnockout"),
                _ => item.Content
            };
        }

        // Update knockout mode combo box
        foreach (ComboBoxItem item in KnockoutModeComboBox.Items)
        {
            var tag = item.Tag?.ToString();
            item.Content = tag switch
            {
                "SingleElimination" => _localizationService.GetString("SingleElimination"),
                "DoubleElimination" => _localizationService.GetString("DoubleElimination"),
                _ => item.Content
            };
        }
    }

    private void UpdateVisibility()
    {
        // Show/hide sets configuration
        SetsConfigGrid.Visibility = PlayWithSetsCheckBox.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        
        // Show/hide knockout configuration
        var showKnockout = PostGroupPhaseModeComboBox.SelectedValue?.ToString() == "KnockoutBracket";
        KnockoutConfigGrid.Visibility = showKnockout ? Visibility.Visible : Visibility.Collapsed;
        
        // Show/hide loser bracket configuration
        var showLoserBracket = showKnockout && KnockoutModeComboBox.SelectedValue?.ToString() == "DoubleElimination";
        LoserBracketConfigPanel.Visibility = showLoserBracket ? Visibility.Visible : Visibility.Collapsed;
        
        // Show/hide qualifying players (only for post-group phases)
        var showQualifying = PostGroupPhaseModeComboBox.SelectedValue?.ToString() != "None";
        QualifyingPlayersLabel.Visibility = showQualifying ? Visibility.Visible : Visibility.Collapsed;
        QualifyingPlayersTextBox.Visibility = showQualifying ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdatePreview()
    {
        PreviewText.Text = _gameRules.ToString();
    }

    private void GameRules_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdatePreview();
    }

    private void PlayWithSetsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        UpdateVisibility();
    }

    private void PlayWithSetsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        UpdateVisibility();
    }

    private void PostGroupPhaseModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateVisibility();
    }

    private void KnockoutModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateVisibility();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("GameRulesWindow.SaveButton_Click: START");
            
            // Validate and save basic rules
            if (Enum.TryParse<GameMode>(GameModeComboBox.SelectedValue?.ToString(), out var gameMode))
                _gameRules.GameMode = gameMode;

            if (Enum.TryParse<FinishMode>(FinishModeComboBox.SelectedValue?.ToString(), out var finishMode))
                _gameRules.FinishMode = finishMode;

            if (int.TryParse(LegsToWinTextBox.Text, out var legsToWin) && legsToWin > 0)
                _gameRules.LegsToWin = legsToWin;

            _gameRules.PlayWithSets = PlayWithSetsCheckBox.IsChecked == true;

            if (_gameRules.PlayWithSets)
            {
                if (int.TryParse(SetsToWinTextBox.Text, out var setsToWin) && setsToWin > 0)
                    _gameRules.SetsToWin = setsToWin;

                if (int.TryParse(LegsPerSetTextBox.Text, out var legsPerSet) && legsPerSet > 0)
                    _gameRules.LegsPerSet = legsPerSet;
            }

            // Save post-group phase settings
            if (Enum.TryParse<PostGroupPhaseMode>(PostGroupPhaseModeComboBox.SelectedValue?.ToString(), out var postGroupMode))
                _gameRules.PostGroupPhaseMode = postGroupMode;

            if (int.TryParse(QualifyingPlayersTextBox.Text, out var qualifyingPlayers) && qualifyingPlayers > 0)
                _gameRules.QualifyingPlayersPerGroup = qualifyingPlayers;

            if (Enum.TryParse<KnockoutMode>(KnockoutModeComboBox.SelectedValue?.ToString(), out var knockoutMode))
                _gameRules.KnockoutMode = knockoutMode;

            _gameRules.IncludeGroupPhaseLosersBracket = IncludeGroupPhaseLosersBracketCheckBox.IsChecked == true;

            System.Diagnostics.Debug.WriteLine("GameRulesWindow.SaveButton_Click: Rules saved, triggering DataChanged event");
            
            // Trigger data changed event
            DataChanged?.Invoke(this, EventArgs.Empty);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving rules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void ConfigureRoundRulesButton_Click(object sender, RoutedEventArgs e)
    {
        var roundRulesWindow = new RoundRulesWindow(_gameRules, _localizationService);
        roundRulesWindow.Owner = this;
        
        // Subscribe to RoundRulesWindow data changes
        roundRulesWindow.DataChanged += (s, args) =>
        {
            System.Diagnostics.Debug.WriteLine("GameRulesWindow: RoundRulesWindow DataChanged received, propagating...");
            DataChanged?.Invoke(this, EventArgs.Empty);
        };
        
        roundRulesWindow.ShowDialog();
    }
}