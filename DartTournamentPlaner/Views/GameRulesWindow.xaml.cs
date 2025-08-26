using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        System.Diagnostics.Debug.WriteLine("InitializeUI: START");
        
        // Set initial values for game mode
        var gameModeTag = _gameRules.GameMode.ToString();
        foreach (ComboBoxItem item in GameModeComboBox.Items)
        {
            if (item.Tag?.ToString() == gameModeTag)
            {
                GameModeComboBox.SelectedItem = item;
                System.Diagnostics.Debug.WriteLine($"InitializeUI: Set GameMode to {gameModeTag}");
                break;
            }
        }
        
        // Set initial values for finish mode
        var finishModeTag = _gameRules.FinishMode.ToString();
        foreach (ComboBoxItem item in FinishModeComboBox.Items)
        {
            if (item.Tag?.ToString() == finishModeTag)
            {
                FinishModeComboBox.SelectedItem = item;
                System.Diagnostics.Debug.WriteLine($"InitializeUI: Set FinishMode to {finishModeTag}");
                break;
            }
        }
        
        LegsToWinTextBox.Text = _gameRules.LegsToWin.ToString();
        PlayWithSetsCheckBox.IsChecked = _gameRules.PlayWithSets;
        SetsToWinTextBox.Text = _gameRules.SetsToWin.ToString();
        LegsPerSetTextBox.Text = _gameRules.LegsPerSet.ToString();
        
        // KORRIGIERT: Set post-group phase values mit korrekter Tag-Behandlung
        var postGroupPhaseTag = _gameRules.PostGroupPhaseMode.ToString();
        System.Diagnostics.Debug.WriteLine($"InitializeUI: Looking for PostGroupPhaseMode tag: {postGroupPhaseTag}");
        
        foreach (ComboBoxItem item in PostGroupPhaseModeComboBox.Items)
        {
            System.Diagnostics.Debug.WriteLine($"  Checking item with tag: {item.Tag}");
            if (item.Tag?.ToString() == postGroupPhaseTag)
            {
                PostGroupPhaseModeComboBox.SelectedItem = item;
                System.Diagnostics.Debug.WriteLine($"InitializeUI: Set PostGroupPhaseMode to {postGroupPhaseTag}");
                break;
            }
        }
        
        QualifyingPlayersTextBox.Text = _gameRules.QualifyingPlayersPerGroup.ToString();
        
        // KORRIGIERT: Set knockout mode mit korrekter Tag-Behandlung
        var knockoutModeTag = _gameRules.KnockoutMode.ToString();
        System.Diagnostics.Debug.WriteLine($"InitializeUI: Looking for KnockoutMode tag: {knockoutModeTag}");
        
        foreach (ComboBoxItem item in KnockoutModeComboBox.Items)
        {
            System.Diagnostics.Debug.WriteLine($"  Checking item with tag: {item.Tag}");
            if (item.Tag?.ToString() == knockoutModeTag)
            {
                KnockoutModeComboBox.SelectedItem = item;
                System.Diagnostics.Debug.WriteLine($"InitializeUI: Set KnockoutMode to {knockoutModeTag}");
                break;
            }
        }
        
        IncludeGroupPhaseLosersBracketCheckBox.IsChecked = _gameRules.IncludeGroupPhaseLosersBracket;
        
        System.Diagnostics.Debug.WriteLine("InitializeUI: Calling UpdateVisibility and UpdatePreview");
        UpdateVisibility();
        UpdatePreview();
        
        System.Diagnostics.Debug.WriteLine("InitializeUI: END");
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
        
        // Button translations
        SaveButton.Content = _localizationService.GetString("Save");
        CancelButton.Content = _localizationService.GetString("Cancel");

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
        // Show/hide qualifying players (only for post-group phases)
        var showQualifying = PostGroupPhaseModeComboBox.SelectedValue?.ToString() != "None";
        QualifyingPlayersLabel.Visibility = showQualifying ? Visibility.Visible : Visibility.Collapsed;
        QualifyingPlayersTextBox.Visibility = showQualifying ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdatePreview()
    {
        // Update preview text - use the new TextBlock name
        RulesPreviewTextBlock.Text = _gameRules.ToString();
    }

    private void GameRules_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        UpdatePreview();
    }

    // Event Handlers
    private void GameModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // KORRIGIERT: Verwende das Tag des ausgewählten ComboBoxItems
        if (GameModeComboBox.SelectedItem is ComboBoxItem selectedItem &&
            Enum.TryParse<GameMode>(selectedItem.Tag?.ToString(), out var mode))
        {
            System.Diagnostics.Debug.WriteLine($"GameModeComboBox_SelectionChanged: Setting GameMode to {mode}");
            _gameRules.GameMode = mode;
        }
        UpdatePreview();
    }

    private void FinishModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // KORRIGIERT: Verwende das Tag des ausgewählten ComboBoxItems
        if (FinishModeComboBox.SelectedItem is ComboBoxItem selectedItem &&
            Enum.TryParse<FinishMode>(selectedItem.Tag?.ToString(), out var mode))
        {
            System.Diagnostics.Debug.WriteLine($"FinishModeComboBox_SelectionChanged: Setting FinishMode to {mode}");
            _gameRules.FinishMode = mode;
        }
        UpdatePreview();
    }

    private void LegsToWinTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(LegsToWinTextBox.Text, out var legsToWin) && legsToWin > 0)
        {
            _gameRules.LegsToWin = legsToWin;
        }
    }

    private void PlayWithSetsCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        _gameRules.PlayWithSets = true;
        UpdateVisibility();
    }

    private void PlayWithSetsCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        _gameRules.PlayWithSets = false;
        UpdateVisibility();
    }

    private void SetsToWinTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(SetsToWinTextBox.Text, out var setsToWin) && setsToWin > 0)
        {
            _gameRules.SetsToWin = setsToWin;
        }
    }

    private void LegsPerSetTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(LegsPerSetTextBox.Text, out var legsPerSet) && legsPerSet > 0)
        {
            _gameRules.LegsPerSet = legsPerSet;
        }
    }

    private void PostGroupPhaseModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // KORRIGIERT: Verwende das Tag des ausgewählten ComboBoxItems
        if (PostGroupPhaseModeComboBox.SelectedItem is ComboBoxItem selectedItem &&
            Enum.TryParse<PostGroupPhaseMode>(selectedItem.Tag?.ToString(), out var mode))
        {
            System.Diagnostics.Debug.WriteLine($"PostGroupPhaseModeComboBox_SelectionChanged: Setting PostGroupPhaseMode to {mode} (from tag: {selectedItem.Tag})");
            _gameRules.PostGroupPhaseMode = mode;
            
            // WICHTIG: Sofort DataChanged Event feuern um sicherzustellen dass die Änderung propagiert wird
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"PostGroupPhaseModeComboBox_SelectionChanged: ERROR - Could not parse selection!");
            System.Diagnostics.Debug.WriteLine($"  SelectedItem: {PostGroupPhaseModeComboBox.SelectedItem}");
            if (PostGroupPhaseModeComboBox.SelectedItem is ComboBoxItem item)
            {
                System.Diagnostics.Debug.WriteLine($"  Item.Tag: {item.Tag}");
            }
        }
        UpdateVisibility();
    }

    private void QualifyingPlayersTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(QualifyingPlayersTextBox.Text, out var players) && players > 0)
        {
            System.Diagnostics.Debug.WriteLine($"QualifyingPlayersTextBox_TextChanged: Setting QualifyingPlayersPerGroup to {players}");
            _gameRules.QualifyingPlayersPerGroup = players;
            
            // WICHTIG: Sofort DataChanged Event feuern
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void KnockoutModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // KORRIGIERT: Verwende das Tag des ausgewählten ComboBoxItems
        if (KnockoutModeComboBox.SelectedItem is ComboBoxItem selectedItem &&
            Enum.TryParse<KnockoutMode>(selectedItem.Tag?.ToString(), out var mode))
        {
            System.Diagnostics.Debug.WriteLine($"KnockoutModeComboBox_SelectionChanged: Setting KnockoutMode to {mode} (from tag: {selectedItem.Tag})");
            _gameRules.KnockoutMode = mode;
            
            // WICHTIG: Sofort DataChanged Event feuern
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"KnockoutModeComboBox_SelectionChanged: ERROR - Could not parse selection!");
        }
        UpdateVisibility();
    }

    private void IncludeGroupPhaseLosersBracketCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("IncludeGroupPhaseLosersBracketCheckBox_Checked: Setting to true");
        _gameRules.IncludeGroupPhaseLosersBracket = true;
        
        // WICHTIG: Sofort DataChanged Event feuern
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void IncludeGroupPhaseLosersBracketCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("IncludeGroupPhaseLosersBracketCheckBox_Unchecked: Setting to false");
        _gameRules.IncludeGroupPhaseLosersBracket = false;
        
        // WICHTIG: Sofort DataChanged Event feuern
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("GameRulesWindow.SaveButton_Click: START");
            
            // Validate and save basic rules
            if (GameModeComboBox.SelectedItem is ComboBoxItem gameModeItem && 
                Enum.TryParse<GameMode>(gameModeItem.Tag?.ToString(), out var gameMode))
            {
                System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting GameMode to {gameMode}");
                _gameRules.GameMode = gameMode;
            }

            if (FinishModeComboBox.SelectedItem is ComboBoxItem finishModeItem && 
                Enum.TryParse<FinishMode>(finishModeItem.Tag?.ToString(), out var finishMode))
            {
                System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting FinishMode to {finishMode}");
                _gameRules.FinishMode = finishMode;
            }

            if (int.TryParse(LegsToWinTextBox.Text, out var legsToWin) && legsToWin > 0)
            {
                System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting LegsToWin to {legsToWin}");
                _gameRules.LegsToWin = legsToWin;
            }

            _gameRules.PlayWithSets = PlayWithSetsCheckBox.IsChecked == true;
            System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting PlayWithSets to {_gameRules.PlayWithSets}");

            if (_gameRules.PlayWithSets)
            {
                if (int.TryParse(SetsToWinTextBox.Text, out var setsToWin) && setsToWin > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting SetsToWin to {setsToWin}");
                    _gameRules.SetsToWin = setsToWin;
                }

                if (int.TryParse(LegsPerSetTextBox.Text, out var legsPerSet) && legsPerSet > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting LegsPerSet to {legsPerSet}");
                    _gameRules.LegsPerSet = legsPerSet;
                }
            }

            // KORRIGIERT: Korrekte Behandlung der PostGroupPhaseMode ComboBox
            if (PostGroupPhaseModeComboBox.SelectedItem is ComboBoxItem postGroupModeItem && 
                Enum.TryParse<PostGroupPhaseMode>(postGroupModeItem.Tag?.ToString(), out var postGroupMode))
            {
                System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting PostGroupPhaseMode to {postGroupMode} (from tag: {postGroupModeItem.Tag})");
                _gameRules.PostGroupPhaseMode = postGroupMode;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"SaveButton_Click: ERROR - Failed to parse PostGroupPhaseMode!");
                System.Diagnostics.Debug.WriteLine($"  SelectedItem: {PostGroupPhaseModeComboBox.SelectedItem}");
                System.Diagnostics.Debug.WriteLine($"  SelectedValue: {PostGroupPhaseModeComboBox.SelectedValue}");
                if (PostGroupPhaseModeComboBox.SelectedItem is ComboBoxItem item)
                {
                    System.Diagnostics.Debug.WriteLine($"  Item.Tag: {item.Tag}");
                    System.Diagnostics.Debug.WriteLine($"  Item.Content: {item.Content}");
                }
            }

            if (int.TryParse(QualifyingPlayersTextBox.Text, out var qualifyingPlayers) && qualifyingPlayers > 0)
            {
                System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting QualifyingPlayersPerGroup to {qualifyingPlayers}");
                _gameRules.QualifyingPlayersPerGroup = qualifyingPlayers;
            }

            // KORRIGIERT: Korrekte Behandlung der KnockoutMode ComboBox
            if (KnockoutModeComboBox.SelectedItem is ComboBoxItem knockoutModeItem && 
                Enum.TryParse<KnockoutMode>(knockoutModeItem.Tag?.ToString(), out var knockoutMode))
            {
                System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting KnockoutMode to {knockoutMode} (from tag: {knockoutModeItem.Tag})");
                _gameRules.KnockoutMode = knockoutMode;
            }

            _gameRules.IncludeGroupPhaseLosersBracket = IncludeGroupPhaseLosersBracketCheckBox.IsChecked == true;
            System.Diagnostics.Debug.WriteLine($"SaveButton_Click: Setting IncludeGroupPhaseLosersBracket to {_gameRules.IncludeGroupPhaseLosersBracket}");

            System.Diagnostics.Debug.WriteLine("GameRulesWindow.SaveButton_Click: Rules saved, triggering DataChanged event");
            System.Diagnostics.Debug.WriteLine($"Final values: PostGroupPhaseMode = {_gameRules.PostGroupPhaseMode}, KnockoutMode = {_gameRules.KnockoutMode}");
            
            // Trigger data changed event
            DataChanged?.Invoke(this, EventArgs.Empty);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveButton_Click: CRITICAL ERROR: {ex.Message}");
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