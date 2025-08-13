using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.Specialized;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.ViewModels;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Controls;

public partial class TournamentTab : UserControl, INotifyPropertyChanged
{
    private TournamentClass _tournamentClass;
    private Group? _selectedGroup;
    private Player? _selectedPlayer;
    private string _newPlayerName = string.Empty;
    private int _nextGroupId = 1;
    private int _nextPlayerId = 1;
    private LocalizationService? _localizationService;

    public TournamentClass TournamentClass
    {
        get => _tournamentClass;
        set
        {
            if (_tournamentClass == value) return;

            if (_tournamentClass != null)
            {
                _tournamentClass.UIRefreshRequested -= OnTournamentUIRefreshRequested;
            }

            _tournamentClass = value;
            
            if (_tournamentClass != null)
            {
                _tournamentClass.UIRefreshRequested += OnTournamentUIRefreshRequested;
            }

            OnPropertyChanged();
            UpdateUI();
            
            if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    RefreshKnockoutView();
                }, DispatcherPriority.Loaded);
            }
        }
    }

    private void OnTournamentUIRefreshRequested(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                RefreshKnockoutView();
                UpdateMatchesView();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnTournamentUIRefreshRequested: {ex.Message}");
            }
        }, DispatcherPriority.DataBind);
    }

    public Group? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (_selectedGroup == value) return;
            
            if (_selectedGroup != null)
            {
                foreach (var match in _selectedGroup.Matches)
                {
                    match.PropertyChanged -= Match_PropertyChanged;
                }
                _selectedGroup.Matches.CollectionChanged -= Matches_CollectionChanged;
            }

            _selectedGroup = value;
            OnPropertyChanged();
            
            if (_selectedGroup != null)
            {
                foreach (var match in _selectedGroup.Matches)
                {
                    match.PropertyChanged += Match_PropertyChanged;
                }
                _selectedGroup.Matches.CollectionChanged += Matches_CollectionChanged;
            }
                
            UpdatePlayersView();
            UpdateMatchesView();
        }
    }

    public Player? SelectedPlayer
    {
        get => _selectedPlayer;
        set
        {
            _selectedPlayer = value;
            OnPropertyChanged();
            RemovePlayerButton.IsEnabled = value != null;
        }
    }

    public string NewPlayerName
    {
        get => _newPlayerName;
        set
        {
            _newPlayerName = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? DataChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    public TournamentTab()
    {
        InitializeComponent();
        DataContext = this;
        _tournamentClass = new TournamentClass();
        
        Loaded += TournamentTab_Loaded;
    }

    private void TournamentTab_Loaded(object sender, RoutedEventArgs e)
    {
        _localizationService = App.LocalizationService;
        if (_localizationService != null)
        {
            _localizationService.PropertyChanged += (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine($"TournamentTab: LocalizationService PropertyChanged - {e.PropertyName}");
                
                // Update translations immediately on the UI thread
                if (Dispatcher.CheckAccess())
                {
                    UpdateTranslations();
                }
                else
                {
                    Dispatcher.BeginInvoke(() => UpdateTranslations(), System.Windows.Threading.DispatcherPriority.Render);
                }
            };
            UpdateTranslations();
        }
    }

    public void UpdateTranslations()
    {
        if (_localizationService == null) return;

        // Update tab headers
        SetupTabItem.Header = _localizationService.GetString("SetupTab");
        GroupPhaseTabItem.Header = _localizationService.GetString("GroupPhaseTab");
        FinalsTabItem.Header = _localizationService.GetString("FinalsTab");
        KnockoutTabItem.Header = _localizationService.GetString("KnockoutTab");

        ConfigureRulesButton.Content = _localizationService.GetString("ConfigureRules");
        AddGroupButton.Content = _localizationService.GetString("AddGroup");
        RemoveGroupButton.Content = _localizationService.GetString("RemoveGroup");
        AddPlayerButton.Content = _localizationService.GetString("AddPlayer");
        RemovePlayerButton.Content = _localizationService.GetString("RemovePlayer");
        GenerateMatchesButton.Content = _localizationService.GetString("GenerateMatches");
        ResetMatchesButton.Content = "⚠ " + _localizationService.GetString("ResetMatches");
        AdvanceToNextPhaseButton.Content = "🏆 " + _localizationService.GetString("AdvanceToNextPhase");
        ResetTournamentButton.Content = "🔄 " + _localizationService.GetString("ResetTournament");
        ResetKnockoutButton.Content = "⚠ " + _localizationService.GetString("ResetKnockoutPhase");
        GroupsHeaderText.Text = _localizationService.GetString("Groups");
        MatchesHeaderText.Text = _localizationService.GetString("Matches");
        StandingsHeaderText.Text = _localizationService.GetString("Standings");
        
        if (RefreshUIButton != null)
        {
            RefreshUIButton.Content = "🔄 " + _localizationService.GetString("RefreshUI");
            RefreshUIButton.ToolTip = _localizationService.GetString("RefreshUITooltip");
        }
        
        // Update DataGrid columns
        UpdateDataGridHeaders();
        
        // Update players view (this will also update phase display)
        UpdatePlayersView();
        
        // Ensure knockout phase is properly loaded if we're in that phase
        if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            RefreshKnockoutView();
        }
    }

    private void UpdateDataGridHeaders()
    {
        if (_localizationService == null) return;

        if (MatchesDataGrid.Columns.Count >= 3)
        {
            MatchesDataGrid.Columns[0].Header = _localizationService.GetString("Match") ?? "Match";
            MatchesDataGrid.Columns[1].Header = _localizationService.GetString("Result");
            MatchesDataGrid.Columns[2].Header = _localizationService.GetString("Status") ?? "Status";
        }

        if (FinalsMatchesDataGrid.Columns.Count >= 3)
        {
            FinalsMatchesDataGrid.Columns[0].Header = _localizationService.GetString("Match") ?? "Match";
            FinalsMatchesDataGrid.Columns[1].Header = _localizationService.GetString("Result");
            FinalsMatchesDataGrid.Columns[2].Header = _localizationService.GetString("Status") ?? "Status";
        }

        if (KnockoutMatchesDataGrid.Columns.Count >= 4)
        {
            KnockoutMatchesDataGrid.Columns[0].Header = _localizationService.GetString("Round") ?? "Runde";
            KnockoutMatchesDataGrid.Columns[1].Header = _localizationService.GetString("Match") ?? "Match";
            KnockoutMatchesDataGrid.Columns[2].Header = _localizationService.GetString("Result");
            KnockoutMatchesDataGrid.Columns[3].Header = _localizationService.GetString("Status") ?? "Status";
        }

        if (StandingsDataGrid.Columns.Count >= 6)
        {
            StandingsDataGrid.Columns[0].Header = _localizationService.GetString("Position") ?? "Pos";
            StandingsDataGrid.Columns[1].Header = _localizationService.GetString("Player");
            StandingsDataGrid.Columns[2].Header = _localizationService.GetString("Score");
            StandingsDataGrid.Columns[3].Header = "W-D-L";
            StandingsDataGrid.Columns[4].Header = _localizationService.GetString("Sets");
            StandingsDataGrid.Columns[5].Header = _localizationService.GetString("Legs");
        }
    }

    private void UpdateUI()
    {
        if (TournamentClass?.Groups != null)
        {
            if (GroupsListBox.ItemsSource != TournamentClass.Groups)
            {
                GroupsListBox.ItemsSource = TournamentClass.Groups;
            }
            
            if (GroupPhaseGroupsList.ItemsSource != TournamentClass.Groups)
            {
                GroupPhaseGroupsList.ItemsSource = TournamentClass.Groups;
            }
        }
        
        RemoveGroupButton.IsEnabled = GroupsListBox.SelectedItem != null;
        UpdateNextIds();
        UpdatePhaseDisplay();
    }

    private void UpdateNextIds()
    {
        if (TournamentClass.Groups.Count > 0)
        {
            var maxId = TournamentClass.Groups.Max(g => g.Id);
            _nextGroupId = maxId + 1;
        }
        else
        {
            _nextGroupId = 1;
        }

        var allPlayers = TournamentClass.Groups.SelectMany(g => g.Players);
        if (allPlayers.Any())
        {
            var maxPlayerId = allPlayers.Max(p => p.Id);
            _nextPlayerId = maxPlayerId + 1;
        }
        else
        {
            _nextPlayerId = 1;
        }
    }

    private void UpdatePlayersView()
    {
        try
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                if (SelectedGroup != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            PlayersListBox.ItemsSource = SelectedGroup.Players;
                            if (_localizationService != null)
                            {
                                PlayersHeaderText.Text = _localizationService.GetString("PlayersInGroup", SelectedGroup.Name);
                            }
                            else
                            {
                                PlayersHeaderText.Text = $"Spieler in {SelectedGroup.Name}:";
                            }

                            PlayerNameTextBox.IsEnabled = true;
                            AddPlayerButton.IsEnabled = true;
                            GenerateMatchesButton.IsEnabled = SelectedGroup.Players.Count >= 2;
                            ResetMatchesButton.IsEnabled = SelectedGroup.MatchesGenerated && SelectedGroup.Matches.Count > 0;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView for selected group: {ex.Message}");
                        }
                    }, DispatcherPriority.DataBind);
                }
                else
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            PlayersListBox.ItemsSource = null;
                            if (_localizationService != null)
                            {
                                PlayersHeaderText.Text = _localizationService.GetString("NoGroupSelectedPlayers");
                            }
                            else
                            {
                                PlayersHeaderText.Text = "Spieler: (Keine Gruppe ausgewählt)";
                            }

                            PlayerNameTextBox.IsEnabled = false;
                            AddPlayerButton.IsEnabled = false;
                            GenerateMatchesButton.IsEnabled = false;
                            ResetMatchesButton.IsEnabled = false;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView for no group: {ex.Message}");
                        }
                    }, DispatcherPriority.DataBind);
                }
                SelectedPlayer = null;
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Finals phase");
                
                // Finals phase - show qualified players
                var finalsGroup = TournamentClass.CurrentPhase.FinalsGroup;
                if (finalsGroup != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            PlayersListBox.ItemsSource = finalsGroup.Players;
                            PlayersHeaderText.Text = _localizationService?.GetString("FinalistsCount", finalsGroup.Players.Count) ?? $"Finalisten ({finalsGroup.Players.Count} Spieler):";

                            // Disable player management in finals
                            PlayerNameTextBox.IsEnabled = false;
                            AddPlayerButton.IsEnabled = false;
                            GenerateMatchesButton.IsEnabled = !finalsGroup.MatchesGenerated;
                            ResetMatchesButton.IsEnabled = finalsGroup.MatchesGenerated;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView for finals: {ex.Message}");
                        }
                    }, DispatcherPriority.DataBind);
                }
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Knockout phase");
                
                // SICHERHEITSCHECK: Prüfe ob QualifiedPlayers verfügbar sind
                var qualifiedPlayers = TournamentClass.CurrentPhase.QualifiedPlayers;
                if (qualifiedPlayers == null)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: QualifiedPlayers is null!");
                    
                    Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            PlayersListBox.ItemsSource = null;
                            PlayersHeaderText.Text = "KO-Teilnehmer: (Fehler - keine Daten)";

                            // Disable all player management in knockout
                            PlayerNameTextBox.IsEnabled = false;
                            AddPlayerButton.IsEnabled = false;
                            GenerateMatchesButton.IsEnabled = false;
                            ResetMatchesButton.IsEnabled = false;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView for knockout (null check): {ex.Message}");
                        }
                    }, DispatcherPriority.DataBind);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Knockout phase with {qualifiedPlayers.Count} players");
                
                // Knockout phase - show qualified players
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        PlayersListBox.ItemsSource = qualifiedPlayers;
                        PlayersHeaderText.Text = _localizationService?.GetString("KnockoutParticipantsCount", qualifiedPlayers.Count) ?? $"KO-Teilnehmer ({qualifiedPlayers.Count} Spieler):";
                        
                        // Disable all player management in knockout
                        PlayerNameTextBox.IsEnabled = false;
                        AddPlayerButton.IsEnabled = false;
                        GenerateMatchesButton.IsEnabled = false;
                        ResetMatchesButton.IsEnabled = false;
                        
                        System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Successfully updated UI for knockout phase");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView for knockout: {ex.Message}");
                    }
                }, DispatcherPriority.DataBind);
            }
            
            // Update phase info whenever players view changes
            UpdatePhaseDisplay();
            System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView: {ex.Message}");
        }
    }

    private void UpdatePhaseDisplay()
    {
        if (TournamentClass?.CurrentPhase == null) return;

        try
        {
            var phaseText = TournamentClass.CurrentPhase.PhaseType switch
            {
                TournamentPhaseType.GroupPhase => _localizationService?.GetString("GroupPhase") ?? "Gruppenphase",
                TournamentPhaseType.RoundRobinFinals => _localizationService?.GetString("FinalsPhase") ?? "Finalrunde",
                TournamentPhaseType.KnockoutPhase => _localizationService?.GetString("KnockoutPhase") ?? "KO-Phase",
                _ => "Unbekannte Phase"
            };

            CurrentPhaseText.Text = $"Aktuelle Phase: {phaseText}";

            var hasPostPhase = TournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None;
            var hasRoundRobinFinals = TournamentClass.GameRules.PostGroupPhaseMode == PostGroupPhaseMode.RoundRobinFinals;
            var hasKnockout = TournamentClass.GameRules.PostGroupPhaseMode == PostGroupPhaseMode.KnockoutBracket;
            var hasDoubleElimination = TournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination;

            FinalsTabItem.Visibility = hasRoundRobinFinals ? Visibility.Visible : Visibility.Collapsed;
            KnockoutTabItem.Visibility = hasKnockout ? Visibility.Visible : Visibility.Collapsed;
            LoserBracketTab.Visibility = hasKnockout && hasDoubleElimination ? Visibility.Visible : Visibility.Collapsed;
            LoserBracketTreeTab.Visibility = hasKnockout && hasDoubleElimination ? Visibility.Visible :Visibility.Collapsed;

            bool canAdvance = false;
            try
            {
                canAdvance = TournamentClass.CanProceedToNextPhase() && 
                           TournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: ERROR in CanProceedToNextPhase: {ex.Message}");
                canAdvance = false;
            }

            AdvanceToNextPhaseButton.IsEnabled = canAdvance;
            AdvanceToNextPhaseButton.Visibility = TournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            var hasGeneratedMatches = TournamentClass.Groups.Any(g => g.MatchesGenerated && g.Matches.Count > 0);
            var isInAdvancedPhase = TournamentClass.CurrentPhase?.PhaseType != TournamentPhaseType.GroupPhase;
            ResetTournamentButton.IsEnabled = hasGeneratedMatches || isInAdvancedPhase;

            // Show what next phase would be
            if (canAdvance)
            {
                try
                {
                    var nextPhase = TournamentClass.GetNextPhase();
                    System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: Next phase = {nextPhase?.PhaseType}");                    
                    if (nextPhase != null)
                    {
                        var nextPhaseText = nextPhase.PhaseType switch
                        {
                            TournamentPhaseType.RoundRobinFinals => _localizationService?.GetString("FinalsPhase") ?? "Finalrunde",
                            TournamentPhaseType.KnockoutPhase => _localizationService?.GetString("KnockoutPhase") ?? "KO-Phase",
                            _ => "Nächste Phase"
                        };
                        AdvanceToNextPhaseButton.Content = $"🏆 {_localizationService?.GetString("NextPhaseStart", nextPhaseText) ?? $"{nextPhaseText} starten"}";
                        System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: Set next phase button text to '{nextPhaseText} starten'");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: ERROR in GetNextPhase: {ex.Message}");
                }
            }

            UpdateTournamentOverview();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: CRITICAL ERROR: {ex.Message}");
        }
    }

    private void UpdateTournamentOverview()
    {
        if (TournamentClass == null) return;

        var overview = $"{_localizationService?.GetString("TournamentName") ?? "🏆 Turnier:"} {TournamentClass.Name}\n\n";
        overview += $"{_localizationService?.GetString("CurrentPhase") ?? "🎯 Aktuelle Phase:"} {TournamentClass.CurrentPhase?.Name}\n";
        overview += $"{_localizationService?.GetString("GroupsCount") ?? "👥 Gruppen:"} {TournamentClass.Groups.Count}\n";
        overview += $"{_localizationService?.GetString("PlayersTotal") ?? "🎮 Spieler gesamt:"} {TournamentClass.Groups.SelectMany(g => g.Players).Count()}\n\n";
        
        overview += $"{_localizationService?.GetString("GameRulesColon") ?? "📋 Spielregeln:"}\n{TournamentClass.GameRules}\n\n";

        if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
        {
            var finishedGroups = TournamentClass.Groups.Count(g => g.MatchesGenerated && g.Matches.All(m => m.Status == MatchStatus.Finished || m.IsBye));
            overview += $"{_localizationService?.GetString("CompletedGroups") ?? "✅ Abgeschlossene Gruppen:"} {finishedGroups}/{TournamentClass.Groups.Count}\n";
        }
        else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
            var qualifiedCount = TournamentClass.CurrentPhase.QualifiedPlayers.Count;
            overview += $"{_localizationService?.GetString("QualifiedPlayers") ?? "🏅 Qualifizierte Spieler:"} {qualifiedCount}\n";
        }
        else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            var totalMatches = TournamentClass.CurrentPhase.WinnerBracket.Count;
            var finishedMatches = TournamentClass.CurrentPhase.WinnerBracket.Count(m => m.Status == MatchStatus.Finished);
            overview += $"{_localizationService?.GetString("KnockoutMatches") ?? "⚔️ KO-Spiele:"} {finishedMatches}/{totalMatches} {_localizationService?.GetString("Completed") ?? "beendet"}\n";
        }

        TournamentOverviewText.Text = overview;
    }

    private void UpdateMatchesView()
    {
        try
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                if (SelectedGroup != null)
                {
                    MatchesDataGrid.ItemsSource = SelectedGroup.Matches;
                    var standings = SelectedGroup.GetStandings();
                    StandingsDataGrid.ItemsSource = standings;
                }
                else
                {
                    MatchesDataGrid.ItemsSource = null;
                    StandingsDataGrid.ItemsSource = null;
                }
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                RefreshFinalsView();
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                RefreshKnockoutView();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: ERROR: {ex.Message}");
        }
    }

    private void RefreshFinalsView()
    {
        if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
            var finalsGroup = TournamentClass.CurrentPhase.FinalsGroup;
            if (finalsGroup != null)
            {
                FinalistsListBox.ItemsSource = finalsGroup.Players;
                FinalsMatchesDataGrid.ItemsSource = finalsGroup.Matches;
                var standings = finalsGroup.GetStandings();
                FinalsStandingsDataGrid.ItemsSource = standings;
            }
        }
    }

    private void RefreshKnockoutView()
    {
        if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            KnockoutParticipantsListBox.ItemsSource = TournamentClass.CurrentPhase.QualifiedPlayers;
            
            var winnerBracketMatches = TournamentClass.CurrentPhase.WinnerBracket.Select(match =>
                new KnockoutMatchViewModel(match, TournamentClass)).ToList();
            
            KnockoutMatchesDataGrid.ItemsSource = winnerBracketMatches;
            
            if (TournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                var loserBracketMatches = TournamentClass.CurrentPhase.LoserBracket.Select(match =>
                    new KnockoutMatchViewModel(match, TournamentClass)).ToList();
                
                LoserBracketDataGrid.ItemsSource = loserBracketMatches;
                
                LoserBracketTab.Visibility = Visibility.Visible;
                LoserBracketTreeTab.Visibility = Visibility.Visible;
            }
            else
            {
                LoserBracketTab.Visibility = Visibility.Collapsed;
                LoserBracketTreeTab.Visibility = Visibility.Collapsed;
            }
            
            DrawBracketTree();
            DrawLoserBracketTree();
        }
    }

    private void DrawBracketTree()
    {
        // Placeholder - komplexere Implementierung würde hier folgen
        if (BracketCanvas != null)
        {
            BracketCanvas.Children.Clear();
        }
    }

    private void DrawLoserBracketTree()
    {
        // Placeholder - komplexere Implementierung würde hier folgen
        if (LoserBracketCanvas != null)
        {
            LoserBracketCanvas.Children.Clear();
        }
    }

    private void ClearViewsSafely()
    {
        try
        {
            PlayersListBox.ItemsSource = null;
            MatchesDataGrid.ItemsSource = null;
            StandingsDataGrid.ItemsSource = null;
            
            if (_localizationService != null)
            {
                PlayersHeaderText.Text = _localizationService.GetString("NoGroupSelectedPlayers");
            }
            else
            {
                PlayersHeaderText.Text = "Spieler: (Keine Gruppe ausgewählt)";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClearViewsSafely: ERROR: {ex.Message}");
        }
    }

    private void Match_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Match.Status) or nameof(Match.Winner))
        {
            UpdateMatchesView();
        }
    }

    private void Matches_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (Match match in e.NewItems)
            {
                match.PropertyChanged += Match_PropertyChanged;
            }
        }
        
        if (e.OldItems != null)
        {
            foreach (Match match in e.OldItems)
            {
                match.PropertyChanged -= Match_PropertyChanged;
            }
        }
        
        UpdateMatchesView();
    }

    // Event Handlers
    private void ConfigureRulesButton_Click(object sender, RoutedEventArgs e)
    {
        if (_localizationService == null) return;

        try
        {
            var gameRulesWindow = new Views.GameRulesWindow(TournamentClass.GameRules, _localizationService);
            gameRulesWindow.Owner = Window.GetWindow(this);
            
            gameRulesWindow.DataChanged += (s, args) =>
            {
                foreach (var group in TournamentClass.Groups)
                {
                    if (group.MatchesGenerated && group.Matches.Count > 0)
                    {
                        group.UpdateMatchDisplaySettings(TournamentClass.GameRules);
                    }
                }
                
                OnDataChanged();
            };
            
            gameRulesWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Öffnen der Spielregeln: {ex.Message}", "Fehler", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddGroupButton_Click(object sender, RoutedEventArgs e)
    {
        var defaultName = _localizationService?.GetString("Group", _nextGroupId) ?? $"Gruppe {_nextGroupId}";
        var title = _localizationService?.GetString("NewGroup") ?? "Neue Gruppe";
        var prompt = _localizationService?.GetString("GroupName") ?? "Geben Sie den Namen der neuen Gruppe ein:";

        string? groupName = ShowInputDialog(prompt, title, defaultName);
        
        if (!string.IsNullOrWhiteSpace(groupName))
        {
            while (TournamentClass.Groups.Any(g => g.Id == _nextGroupId))
            {
                _nextGroupId++;
            }
            
            var group = new Group { Id = _nextGroupId, Name = groupName.Trim() };
            TournamentClass.Groups.Add(group);
            _nextGroupId++;
            
            group.Players.CollectionChanged += (s, e) => OnDataChanged();
            group.Matches.CollectionChanged += (s, e) => OnDataChanged();
            
            OnDataChanged();
        }
    }

    private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
    {
        if (GroupsListBox.SelectedItem is Group selectedGroup)
        {
            var title = _localizationService?.GetString("RemoveGroupTitle") ?? "Gruppe entfernen";
            var confirmMessage = _localizationService?.GetString("RemoveGroupConfirm", selectedGroup.Name) ?? 
                         $"Möchten Sie die Gruppe '{selectedGroup.Name}' wirklich entfernen?\nAlle Spieler in dieser Gruppe werden ebenfalls entfernt.";

            var result = MessageBox.Show(confirmMessage, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                TournamentClass.Groups.Remove(selectedGroup);
                if (SelectedGroup == selectedGroup)
                {
                    SelectedGroup = null;
                }
                
                ResetToGroupPhase();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            var title = _localizationService?.GetString("NoGroupSelectedTitle") ?? "Keine Gruppe ausgewählt";
            var noGroupMessage = _localizationService?.GetString("NoGroupSelected") ?? "Bitte wählen Sie eine Gruppe aus, die entfernt werden soll.";
            MessageBox.Show(noGroupMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null)
        {
            var title = _localizationService?.GetString("NoGroupSelectedTitle") ?? "Keine Gruppe ausgewählt";
            var message = _localizationService?.GetString("SelectGroupFirst") ?? "Bitte wählen Sie zuerst eine Gruppe aus.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPlayerName))
        {
            var title = _localizationService?.GetString("NoNameEntered") ?? "Kein Name eingegeben";
            var message = _localizationService?.GetString("EnterPlayerName") ?? "Bitte geben Sie einen Spielernamen ein.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var player = new Player { Id = _nextPlayerId++, Name = NewPlayerName.Trim() };
        SelectedGroup.Players.Add(player);
        NewPlayerName = string.Empty;
        
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RemovePlayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPlayer == null)
        {
            var title = _localizationService?.GetString("NoPlayerSelectedTitle") ?? "Kein Spieler ausgewählt";
            var message = _localizationService?.GetString("NoPlayerSelected") ?? "Bitte wählen Sie einen Spieler aus, der entfernt werden soll.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (SelectedGroup == null) return;

        var title2 = _localizationService?.GetString("RemovePlayerTitle") ?? "Spieler entfernen";
        var message2 = _localizationService?.GetString("RemovePlayerConfirm", SelectedPlayer.Name) ?? 
                      $"Möchten Sie den Spieler '{SelectedPlayer.Name}' wirklich entfernen?";

        var result = MessageBox.Show(message2, title2, MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            SelectedGroup.Players.Remove(SelectedPlayer);
            SelectedPlayer = null;
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GenerateMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup?.Players.Count < 2)
        {
            var title = _localizationService?.GetString("Information") ?? "Information";
            var message = _localizationService?.GetString("MinimumTwoPlayers") ?? "Mindestens 2 Spieler sind erforderlich.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            SelectedGroup.GenerateRoundRobinMatches(TournamentClass.GameRules);
            
            var message = _localizationService?.GetString("MatchesGeneratedSuccess") ?? "Spiele wurden erfolgreich generiert!";
            var title = _localizationService?.GetString("Information") ?? "Information";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            
            UpdateMatchesView();
            UpdatePlayersView();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            var title = _localizationService?.GetString("Error") ?? "Fehler";
            var message = $"{_localizationService?.GetString("ErrorGeneratingMatches") ?? "Fehler beim Generieren der Spiele:"} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null) return;

        var title = _localizationService?.GetString("ResetMatchesTitle") ?? "Spiele zurücksetzen";
        var message = _localizationService?.GetString("ResetMatchesConfirm", SelectedGroup.Name) ?? 
                     $"Möchten Sie alle Spiele für Gruppe '{SelectedGroup.Name}' wirklich zurücksetzen?\nAlle Ergebnisse gehen verloren!";

        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            SelectedGroup.Matches.Clear();
            SelectedGroup.MatchesGenerated = false;
            
            UpdateMatchesView();
            UpdatePlayersView();
            DataChanged?.Invoke(this, EventArgs.Empty);
            
            var successMessage = _localizationService?.GetString("MatchesResetSuccess") ?? "Spiele wurden zurückgesetzt!";
            var successTitle = _localizationService?.GetString("Information") ?? "Information";
            MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void AdvanceToNextPhaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!TournamentClass.CanProceedToNextPhase())
            {
                var cannotAdvanceMessage = _localizationService?.GetString("CannotAdvancePhase") ?? "Alle Spiele der aktuellen Phase müssen beendet sein";
                var title = _localizationService?.GetString("Information") ?? "Information";
                MessageBox.Show(cannotAdvanceMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            TournamentClass.AdvanceToNextPhase();
            
            UpdateUI();
            UpdatePlayersView();
            UpdateMatchesView();
            
            if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                MainTabControl.SelectedItem = FinalsTabItem;
                RefreshFinalsView();
            }
            else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                MainTabControl.SelectedItem = KnockoutTabItem;
                RefreshKnockoutView();
            }
            
            OnDataChanged();
        }
        catch (Exception ex)
        {
            var title = _localizationService?.GetString("Error") ?? "Fehler";
            var message = $"{_localizationService?.GetString("ErrorAdvancingPhase") ?? "Fehler beim Wechsel zur nächsten Phase:"} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetTournamentButton_Click(object sender, RoutedEventArgs e)
    {
        ResetTournamentButton.IsEnabled = false;
        
        try
        {
            var title = _localizationService?.GetString("ResetTournamentTitle") ?? "Turnier komplett zurücksetzen";
            var message = _localizationService?.GetString("ResetTournamentConfirm") ?? 
                         "Möchten Sie das gesamte Turnier wirklich zurücksetzen?\n\n⚠ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var group in TournamentClass.Groups)
                {
                    group.Matches.Clear();
                    group.MatchesGenerated = false;
                }
                
                var phasesToRemove = TournamentClass.Phases
                    .Where(p => p.PhaseType != TournamentPhaseType.GroupPhase)
                    .ToList();
                
                foreach ( var phase in phasesToRemove)
                {
                    TournamentClass.Phases.Remove(phase);
                }
                
                var groupPhase = TournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
                if (groupPhase != null)
                {
                    groupPhase.IsActive = true;
                    groupPhase.IsCompleted = false;
                    TournamentClass.CurrentPhase = groupPhase;
                }
                
                SelectedGroup = null;
                SelectedPlayer = null;
                
                if (BracketCanvas != null)
                {
                    BracketCanvas.Children.Clear();
                }
                
                if (LoserBracketCanvas != null)
                {
                    LoserBracketCanvas.Children.Clear();
                }
                
                KnockoutParticipantsListBox.ItemsSource = null;
                KnockoutMatchesDataGrid.ItemsSource = null;
                LoserBracketDataGrid.ItemsSource = null;
                
                UpdateUI();
                UpdatePlayersView();
                UpdateMatchesView();
                
                MainTabControl.SelectedItem = SetupTabItem;
                
                OnDataChanged();
                
                var successMessage = _localizationService?.GetString("TournamentResetComplete") ?? "Turnier wurde erfolgreich zurückgesetzt.";
                var successTitle = _localizationService?.GetString("Information") ?? "Information";
                MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            var errorMessage = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen des Turniers:"} {ex.Message}";
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            ResetTournamentButton.IsEnabled = true;
        }
    }

    // List box event handlers
    private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Group selectedGroup)
        {
            SelectedGroup = selectedGroup;
            RemoveGroupButton.IsEnabled = selectedGroup != null;
        }
        else
        {
            RemoveGroupButton.IsEnabled = false;
        }
    }

    private void GroupPhaseGroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Group selectedGroup)
        {
            SelectedGroup = selectedGroup;
        }
    }

    private void PlayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Player selectedPlayer)
        {
            SelectedPlayer = selectedPlayer;
        }
    }

    private void PlayerNameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddPlayerButton_Click(sender, new RoutedEventArgs());
        }
    }

    // DataGrid event handlers
    private void MatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (MatchesDataGrid.SelectedItem is Match selectedMatch && !selectedMatch.IsBye && _localizationService != null)
        {
            var resultWindow = new MatchResultWindow(selectedMatch, TournamentClass.GameRules, _localizationService);
            resultWindow.Owner = Window.GetWindow(this);
            
            if (resultWindow.ShowDialog() == true)
            {
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.ScoreDisplay));
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.StatusDisplay));
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.WinnerDisplay));
                
                UpdateMatchesView();
                OnDataChanged();
            }
        }
    }

    private void FinalsMatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FinalsMatchesDataGrid.SelectedItem is Match selectedMatch && !selectedMatch.IsBye && _localizationService != null)
        {
            var resultWindow = new MatchResultWindow(selectedMatch, TournamentClass.GameRules, _localizationService);
            resultWindow.Owner = Window.GetWindow(this);
            
            if (resultWindow.ShowDialog() == true)
            {
                RefreshFinalsView();
                OnDataChanged();
            }
        }
    }

    private void KnockoutMatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        KnockoutMatch? selectedMatch = null;
        
        if (KnockoutMatchesDataGrid.SelectedItem is KnockoutMatchViewModel viewModel)
        {
            selectedMatch = viewModel.Match;
        }
        else if (KnockoutMatchesDataGrid.SelectedItem is KnockoutMatch directMatch)
        {
            selectedMatch = directMatch;
        }
        
        if (selectedMatch != null && _localizationService != null)
        {
            OpenMatchResultWindow(selectedMatch);
        }
    }

    private void LoserBracketDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        KnockoutMatch? selectedMatch = null;
        
        if (LoserBracketDataGrid.SelectedItem is KnockoutMatchViewModel viewModel)
        {
            selectedMatch = viewModel.Match;
        }
        else if (LoserBracketDataGrid.SelectedItem is KnockoutMatch directMatch)
        {
            selectedMatch = directMatch;
        }
        
        if (selectedMatch != null && _localizationService != null)
        {
            OpenMatchResultWindow(selectedMatch);
        }
    }

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] == KnockoutTabItem)
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                var qualifiedParticipants = TournamentClass.CurrentPhase.QualifiedPlayers?.Count ?? 0;
                
                if (qualifiedParticipants < 2)
                {
                    var koUsrWarnMessage = _localizationService?.GetString("KOPhaseNotEnoughUserMSB") ?? "Nicht genügend Teilnehmer für K.O.-Phase (mindestens 2 erforderlich)";
                    var koUsrWarnTitle = _localizationService?.GetString("KOPhaseUsrWarnTitel") ?? "K.O.-Phase Warnung";
                    MessageBox.Show(koUsrWarnMessage, koUsrWarnTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    //MessageBox.Show("Nicht genügend Teilnehmer für K.O.-Phase (mindestens 2 erforderlich)", 
                                   //"K.O.-Phase Warnung", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    MainTabControl.SelectedItem = GroupPhaseTabItem;
                    return;
                }
                
                Dispatcher.BeginInvoke(() => RefreshKnockoutView(), DispatcherPriority.Loaded);
            }
            else
            {
                var koInfoMessage = _localizationService?.GetString("KOPhaseActiveMSB") ?? "K.O.-Phase ist noch nicht aktiv.";
                var koInfoTitle = _localizationService?.GetString("Information") ?? "Information";
                MessageBox.Show(koInfoMessage, koInfoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                //MessageBox.Show("K.O.-Phase ist noch nicht aktiv.", "Information", 
                               //MessageBoxButton.OK, MessageBoxImage.Information);
                MainTabControl.SelectedItem = GroupPhaseTabItem;
            }
        }
        else if (e.AddedItems.Count > 0 && e.AddedItems[0] == FinalsTabItem)
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                RefreshFinalsView();
            }
            else
            {
                MessageBox.Show("Finalrunde ist noch nicht aktiv.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                MainTabControl.SelectedItem = GroupPhaseTabItem;
            }
        }
    }

    // Helper methods
    private void ResetToGroupPhase()
    {
        var phasesToRemove = TournamentClass.Phases
            .Where(p => p.PhaseType != TournamentPhaseType.GroupPhase)
            .ToList();
        
        foreach (var phase in phasesToRemove)
        {
            TournamentClass.Phases.Remove(phase);
        }
        
        var groupPhase = TournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        if (groupPhase != null)
        {
            groupPhase.IsActive = true;
            groupPhase.IsCompleted = false;
            TournamentClass.CurrentPhase = groupPhase;
        }
        
        UpdatePhaseDisplay();
        UpdateMatchesView();
        
        MainTabControl.SelectedItem = SetupTabItem;
    }

    private void OpenMatchResultWindow(KnockoutMatch match)
    {
        if (match.Player1 == null || match.Player2 == null || match.Status == MatchStatus.Bye)
        {
            return;
        }

        try
        {
            // Convert KnockoutMatch to Match for the result window
            var matchForWindow = new Match
            {
                Id = match.Id,
                Player1 = match.Player1,
                Player2 = match.Player2,
                Player1Sets = match.Player1Sets,
                Player2Sets = match.Player2Sets,
                Player1Legs = match.Player1Legs,
                Player2Legs = match.Player2Legs,
                Winner = match.Winner,
                Status = match.Status,
                Notes = match.Notes,
                StartTime = match.StartTime,
                EndTime = match.EndTime
            };
            
            var resultWindow = new MatchResultWindow(matchForWindow, TournamentClass.GameRules, _localizationService!);
            resultWindow.Owner = Window.GetWindow(this);
            
            if (resultWindow.ShowDialog() == true)
            {
                var internalMatch = resultWindow.InternalMatch;
                match.Player1Sets = internalMatch.Player1Sets;
                match.Player2Sets = internalMatch.Player2Sets;
                match.Player1Legs = internalMatch.Player1Legs;
                match.Player2Legs = internalMatch.Player2Legs;
                match.Winner = internalMatch.Winner;
                match.Status = internalMatch.Status;
                match.Notes = internalMatch.Notes;
                match.StartTime = internalMatch.StartTime;
                match.EndTime = DateTime.Now;
                
                UpdateKnockoutProgression(match);
                RefreshKnockoutView();
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService?.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Ergebnis-Fensters: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateKnockoutProgression(KnockoutMatch completedMatch)
    {
        if (TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase || completedMatch.Winner == null)
        {
            return;
        }

        bool success = TournamentClass.ProcessMatchResult(completedMatch);
        
        if (!success)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to process match result - no progression occurred");
        }
    }

    private string? ShowInputDialog(string prompt, string title, string defaultValue = "")
    {
        var dialog = new Window
        {
            Title = title,
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var label = new Label { Content = prompt, Margin = new Thickness(10) };
        Grid.SetRow(label, 0);
        
        var textBox = new TextBox { Text = defaultValue, Margin = new Thickness(10), Height = 25 };
        Grid.SetRow(textBox, 1);
        
        var buttonPanel = new StackPanel 
        { 
            Orientation = Orientation.Horizontal, 
            HorizontalAlignment = HorizontalAlignment.Right, 
            Margin = new Thickness(10) 
        };
        
        var okButton = new Button 
        { 
            Content = _localizationService?.GetString("OK") ?? "OK", 
            Width = 70, 
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true 
        };
        var cancelButton = new Button 
        { 
            Content = _localizationService?.GetString("Cancel") ?? "Cancel", 
            Width = 70,
            IsCancel = true 
        };
        
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        Grid.SetRow(buttonPanel, 2);
        
        grid.Children.Add(label);
        grid.Children.Add(textBox);
        grid.Children.Add(buttonPanel);
        
        dialog.Content = grid;
        
        okButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
        cancelButton.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };
        
        if (Window.GetWindow(this) is Window parentWindow)
        {
            dialog.Owner = parentWindow;
        }
        
        textBox.Focus();
        textBox.SelectAll();
        
        return dialog.ShowDialog() == true ? textBox.Text : null;
    }

    // Button click handlers for bye functionality (placeholders)
    private void GiveByeButton_Click(object sender, RoutedEventArgs e)
    {
        // Bye functionality would be implemented here
        MessageBox.Show("Give Bye functionality not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void UndoByeButton_Click(object sender, RoutedEventArgs e)
    {
        // Undo bye functionality would be implemented here  
        MessageBox.Show("Undo Bye functionality not implemented yet", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResetKnockoutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _localizationService?.GetString("ResetKnockoutTitle") ?? "KO-Phase zurücksetzen";
            var message = _localizationService?.GetString("ResetKnockoutConfirm") ?? 
                         "Möchten Sie die KO-Phase wirklich zurücksetzen?\n\n⚠ Alle KO-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird zur Gruppenphase zurückgesetzt.";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                var knockoutPhase = TournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.KnockoutPhase);
                if (knockoutPhase != null)
                {
                    TournamentClass.Phases.Remove(knockoutPhase);
                }

                var groupPhase = TournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
                if (groupPhase != null)
                {
                    groupPhase.IsActive = true;
                    groupPhase.IsCompleted = false;
                    TournamentClass.CurrentPhase = groupPhase;
                }

                UpdateUI();
                UpdatePlayersView();
                UpdateMatchesView();

                MainTabControl.SelectedItem = GroupPhaseTabItem;
                OnDataChanged();

                var successMessage = _localizationService?.GetString("ResetKnockoutComplete") ?? "KO-Phase wurde erfolgreich zurückgesetzt.";
                var successTitle = _localizationService?.GetString("Information") ?? "Information";
                MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService?.GetString("Error") ?? "Fehler";
            var message = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen:"} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RefreshUIButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateUI();
            UpdatePlayersView();
            UpdateMatchesView();
            
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                RefreshKnockoutView();
            }
            
            var message = _localizationService?.GetString("UIRefreshed") ?? "Benutzeroberfläche wurde aktualisiert";
            MessageBox.Show(message, "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            var title = _localizationService?.GetString("Error") ?? "Fehler";
            var message = $"{_localizationService?.GetString("ErrorRefreshing") ?? "Fehler beim Aktualisieren:"} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}