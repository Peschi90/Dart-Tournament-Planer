using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
        ResetFinalsButton.Content = "⚠ " + _localizationService.GetString("ResetFinalsPhase");
        GroupsHeaderText.Text = _localizationService.GetString("Groups");
        MatchesHeaderText.Text = _localizationService.GetString("Matches");
        StandingsHeaderText.Text = _localizationService.GetString("Standings");
        GamesTabItem.Header = _localizationService.GetString("Matches");
        TableTabItem.Header = _localizationService.GetString("Standings");
        TournamentOverviewHeader.Text = _localizationService.GetString("TournamentOverview");



        //Gruppenphasen Tab
        SelectGroupText.Text = _localizationService.GetString("SelectGroup");

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

            var currentPhaseLabel = _localizationService?.GetString("CurrentPhase") ?? "Aktuelle Phase";
            CurrentPhaseText.Text = $"{currentPhaseLabel}: {phaseText}";

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
            
            // WICHTIG: Turnierbäume neu zeichnen
            DrawBracketTree();
            DrawLoserBracketTree();
            
            System.Diagnostics.Debug.WriteLine("RefreshKnockoutView: Tournament trees have been redrawn");
        }
    }

    private void DrawBracketTree()
    {
        if (BracketCanvas == null || TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            return;

        BracketCanvas.Children.Clear();

        try
        {
            var winnerBracketContent = TournamentClass.CreateTournamentTreeView(BracketCanvas, false, _localizationService);
            // Der Content wird direkt in den Canvas eingefügt von der TournamentClass
            System.Diagnostics.Debug.WriteLine("DrawBracketTree: Interactive Winner Bracket tree created successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DrawBracketTree: Error creating interactive tree: {ex.Message}");
            // Fallback zu alter Implementation
            DrawStaticBracketTree(false);
        }
    }

    private void DrawLoserBracketTree()
    {
        if (LoserBracketCanvas == null || TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            return;

        LoserBracketCanvas.Children.Clear();

        if (TournamentClass.GameRules.KnockoutMode != KnockoutMode.DoubleElimination)
        {
            DrawEmptyBracketMessage(LoserBracketCanvas, 
                _localizationService?.GetString("NoLoserBracketSingleElimination") ?? "Kein Loser Bracket (Single Elimination)", 
                true);
            return;
        }

        try
        {
            var loserBracketContent = TournamentClass.CreateTournamentTreeView(LoserBracketCanvas, true, _localizationService);
            // Der Content wird direkt in den Canvas eingefügt von der TournamentClass
            System.Diagnostics.Debug.WriteLine("DrawLoserBracketTree: Interactive Loser Bracket tree created successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DrawLoserBracketTree: Error creating interactive tree: {ex.Message}");
            // Fallback zu alter Implementation
            DrawStaticBracketTree(true);
        }
    }

    // Fallback-Methoden für den Fall, dass die interaktiven Methoden nicht verfügbar sind
    private void DrawStaticBracketTree(bool isLoserBracket)
    {
        var canvas = isLoserBracket ? LoserBracketCanvas : BracketCanvas;
        if (canvas == null) return;

        var matches = isLoserBracket 
            ? TournamentClass.CurrentPhase.LoserBracket.ToList()
            : TournamentClass.CurrentPhase.WinnerBracket.ToList();

        if (matches.Count == 0)
        {
            var message = isLoserBracket 
                ? _localizationService?.GetString("NoLoserBracketGames") ?? "Keine Loser Bracket Spiele vorhanden"
                : _localizationService?.GetString("NoWinnerBracketGames") ?? "Keine Winner Bracket Spiele vorhanden";
            
            DrawEmptyBracketMessage(canvas, message, isLoserBracket);
            return;
        }

        DrawKnockoutBracket(canvas, matches, isLoserBracket);
    }

    private void DrawEmptyBracketMessage(Canvas canvas, string message, bool isLoserBracket)
    {
        canvas.MinWidth = 800;
        canvas.MinHeight = 600;
        canvas.Background = System.Windows.Media.Brushes.White; // Weißer Hintergrund anstatt Gradient

        var messagePanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var icon = new TextBlock
        {
            Text = isLoserBracket ? "🥈" : "🏆",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var messageText = new TextBlock
        {
            Text = message,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.DarkGray
        };

        var subText = new TextBlock
        {
            Text = _localizationService?.GetString("TournamentTreeWillShow") ?? "Der Turnierbaum wird angezeigt sobald die KO-Phase beginnt",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Gray,
            Margin = new Thickness(0, 10, 0, 0)
        };

        messagePanel.Children.Add(icon);
        messagePanel.Children.Add(messageText);
        messagePanel.Children.Add(subText);

        Canvas.SetLeft(messagePanel, 250);
        Canvas.SetTop(messagePanel, 200);
        canvas.Children.Add(messagePanel);
    }

    private void DrawKnockoutBracket(Canvas canvas, List<KnockoutMatch> matches, bool isLoserBracket)
    {
        canvas.Background = System.Windows.Media.Brushes.White; // Weißer Hintergrund anstatt Gradient

        // Add title
        var titleText = new TextBlock
        {
            Text = isLoserBracket ? "🥈 " + (_localizationService?.GetString("LoserBracketTree") ?? "Loser Bracket") 
                                  : "🏆 " + (_localizationService?.GetString("WinnerBracketTree") ?? "Winner Bracket"),
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = isLoserBracket 
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 92, 92))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 139, 34)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        Canvas.SetLeft(titleText, 20);
        Canvas.SetTop(titleText, 10);
        canvas.Children.Add(titleText);

        // Simple message for now
        var infoText = new TextBlock
        {
            Text = _localizationService?.GetString("InteractiveTournamentTree") ?? "Interaktiver Turnierbaum wird über TournamentClass erstellt",
            FontSize = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.DarkGray,
            Margin = new Thickness(0, 50, 0, 0)
        };
        
        Canvas.SetLeft(infoText, 200);
        Canvas.SetTop(infoText, 60);
        canvas.Children.Add(infoText);

        // Adjust canvas size
        canvas.Width = Math.Max(1000, 800);
        canvas.Height = Math.Max(700, 600);
        canvas.MinWidth = canvas.Width;
        canvas.MinHeight = canvas.Height;
    }

    private Border CreateKnockoutMatchControl(KnockoutMatch match, double width, double height)
    {
        var border = new Border
        {
            Width = width,
            Height = height,
            BorderBrush = System.Windows.Media.Brushes.DarkSlateGray,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(3),
            Effect = new DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Gray,
                Direction = 315,
                ShadowDepth = 3,
                Opacity = 0.5
            }
        };

        // Set background color and border based on match status
        switch (match.Status)
        {
            case MatchStatus.NotStarted:
                border.Background = System.Windows.Media.Brushes.WhiteSmoke;
                border.BorderBrush = System.Windows.Media.Brushes.Silver;
                break;
            case MatchStatus.InProgress:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 200));
                border.BorderBrush = System.Windows.Media.Brushes.Orange;
                break;
            case MatchStatus.Finished:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 255, 200));
                border.BorderBrush = System.Windows.Media.Brushes.Green;
                break;
            case MatchStatus.Bye:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 220, 255));
                border.BorderBrush = System.Windows.Media.Brushes.RoyalBlue;
                break;
            default:
                border.Background = System.Windows.Media.Brushes.White;
                break;
        }

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Match ID/Position indicator
        var matchIdText = new TextBlock
        {
            Text = $"#{match.Id}",
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 2)
        };

        // Player names with winner highlighting
        var tbdText = _localizationService?.GetString("TBD") ?? "TBD";
        var vsText = _localizationService?.GetString("Versus") ?? "vs";
        
        var player1Text = new TextBlock
        {
            Text = match.Player1?.Name ?? tbdText,
            FontSize = 11,
            FontWeight = match.Winner?.Id == match.Player1?.Id ? FontWeights.Bold : FontWeights.Normal,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player1?.Id 
                ? System.Windows.Media.Brushes.DarkGreen 
                : System.Windows.Media.Brushes.Black
        };

        var vsTextBlock = new TextBlock
        {
            Text = vsText,
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 2),
            FontStyle = FontStyles.Italic,
            Foreground = System.Windows.Media.Brushes.Gray
        };

        var player2Text = new TextBlock
        {
            Text = match.Player2?.Name ?? tbdText,
            FontSize = 11,
            FontWeight = match.Winner?.Id == match.Player2?.Id ? FontWeights.Bold : FontWeights.Normal,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player2?.Id 
                ? System.Windows.Media.Brushes.DarkGreen 
                : System.Windows.Media.Brushes.Black
        };

        // Score display with better styling
        var scoreText = new TextBlock
        {
            Text = match.Status == MatchStatus.NotStarted ? "--" : match.ScoreDisplay,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.DarkBlue,
            Margin = new Thickness(0, 3, 0, 0)
        };

        stackPanel.Children.Add(matchIdText);
        stackPanel.Children.Add(player1Text);
        stackPanel.Children.Add(vsTextBlock);
        stackPanel.Children.Add(player2Text);
        stackPanel.Children.Add(scoreText);

        border.Child = stackPanel;

        // Enhanced tooltip with more match details
        var tooltipText = $"{_localizationService?.GetString("Match") ?? "Match"} {match.Id} - {match.RoundDisplay}\n" +
                         $"{_localizationService?.GetString("Status")}: {match.StatusDisplay}\n" +
                         $"{_localizationService?.GetString("Player")} 1: {match.Player1?.Name ?? tbdText}\n" +
                         $"{_localizationService?.GetString("Player")} 2: {match.Player2?.Name ?? tbdText}";
        
        if (match.Status == MatchStatus.Finished && match.Winner != null)
        {
            tooltipText += $"\n🏆 {_localizationService?.GetString("Winner")}: {match.Winner.Name}";
        }

        border.ToolTip = tooltipText;

        return border;
    }

    private void DrawBracketConnectionLine(Canvas canvas, double x1, double y1, double x2, double y2)
    {
        var line = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 180)),
            StrokeThickness = 2,
            Opacity = 0.7
        };

        // Create subtle dashed line effect
        line.StrokeDashArray = new System.Windows.Media.DoubleCollection(new double[] { 5, 3 });

        // Add subtle glow effect
        line.Effect = new DropShadowEffect
        {
            Color = System.Windows.Media.Colors.SteelBlue,
            Direction = 0,
            ShadowDepth = 0,
            BlurRadius = 2,
            Opacity = 0.3
        };

        canvas.Children.Add(line);
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
            
            // NEU: Nach jedem Match automatisch Gruppenstatus prüfen (mit LoadingSpinner)
            if (sender is Match match && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                Task.Run(() => CheckAllGroupsCompletion());
            }
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

    private void RefreshUIButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("RefreshUIButton_Click: Refreshing UI manually");
            
            // Update all views
            UpdateUI();
            UpdatePlayersView();
            UpdateMatchesView();
            UpdatePhaseDisplay();
            
            // Refresh knockout view if in knockout phase
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                RefreshKnockoutView();
            }
            
            // Show toast notification
            var successMessage = _localizationService?.GetString("UIRefreshed") ?? "Benutzeroberfläche wurde aktualisiert";
            var title = _localizationService?.GetString("Information") ?? "Information";
            ShowToastNotification(title, successMessage);
            
            System.Diagnostics.Debug.WriteLine("RefreshUIButton_Click: UI refresh completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshUIButton_Click: ERROR: {ex.Message}");
            
            var errorMessage = $"{_localizationService?.GetString("ErrorRefreshing") ?? "Fehler beim Aktualisieren:"} {ex.Message}";
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetFinalsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _localizationService?.GetString("ResetFinalsPhase") ?? "Finalrunde zurücksetzen";
            var message = _localizationService?.GetString("ResetFinalsConfirm") ?? 
                         "Möchten Sie wirklich die Finalrunde zurücksetzen?\n\n⚠ Alle Finalspiele werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Reset to group phase
                ResetToGroupPhase();
                
                var successMessage = _localizationService?.GetString("ResetFinalsComplete") ?? "Die Finalrunde wurde erfolgreich zurückgesetzt.";
                var successTitle = _localizationService?.GetString("Information") ?? "Information";
                MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            var errorMessage = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen:"} {ex.Message}";
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetKnockoutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _localizationService?.GetString("ResetKnockoutPhase") ?? "KO-Phase zurücksetzen";
            var message = _localizationService?.GetString("ResetKnockoutConfirm") ?? 
                         "Möchten Sie wirklich die K.-o.-Phase zurücksetzen?\n\n⚠ Alle K.-o.-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Reset to group phase
                ResetToGroupPhase();
                
                var successMessage = _localizationService?.GetString("ResetKnockoutComplete") ?? "Die K.-o.-Phase wurde erfolgreich zurückgesetzt.";
                var successTitle = _localizationService?.GetString("Information") ?? "Information";
                MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            var errorMessage = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen:"} {ex.Message}";
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UndoByeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is KnockoutMatchViewModel viewModel)
        {
            HandleUndoKnockoutBye(viewModel.Match);
        }
        else if (sender is Button button2 && button2.Tag is KnockoutMatch match)
        {
            HandleUndoKnockoutBye(match);
        }
    }

    private async void MatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                
                // NEU: Automatische Gruppenstatusüberprüfung nach Match-Ergebnis (mit LoadingSpinner)
                Task.Run(() => CheckAllGroupsCompletion());
                
                // WICHTIG: Explizit als geändert markieren für Speicher-Status
                OnDataChanged();
            }
        }
    }

    // Missing event handlers

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Handle tab control selection changes if needed
        UpdatePhaseDisplay();
    }

    private void AdvanceToNextPhaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!TournamentClass.CanProceedToNextPhase())
            {
                var message = _localizationService?.GetString("CannotAdvancePhase") ?? "Alle Spiele der aktuellen Phase müssen abgeschlossen sein";
                var title = _localizationService?.GetString("Warning") ?? "Warnung";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TournamentClass.AdvanceToNextPhase();
            UpdateUI();
            UpdatePlayersView();
            UpdateMatchesView();
            
            // Switch to appropriate tab
            if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                MainTabControl.SelectedItem = FinalsTabItem;
            }
            else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                MainTabControl.SelectedItem = KnockoutTabItem;
            }
            
            OnDataChanged();
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            var errorMessage = $"{_localizationService?.GetString("ErrorAdvancingPhase") ?? "Fehler beim Wechsel in die nächste Phase:"} {ex.Message}";
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null) return;

        try
        {
            var title = _localizationService?.GetString("ResetMatchesTitle") ?? "Spiele zurücksetzen";
            var message = _localizationService?.GetString("ResetMatchesConfirm", SelectedGroup.Name) ?? 
                         $"Möchten Sie alle Spiele für Gruppe '{SelectedGroup.Name}' wirklich zurücksetzen?\nAlle Ergebnisse gehen verloren!";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                SelectedGroup.ResetMatches();
                UpdateMatchesView();
                OnDataChanged();

                var successMessage = _localizationService?.GetString("MatchesResetSuccess") ?? "Spiele wurden zurückgesetzt!";
                var successTitle = _localizationService?.GetString("Information") ?? "Information";
                MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show($"Fehler beim Zurücksetzen der Spiele: {ex.Message}", errorTitle, 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetTournamentButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _localizationService?.GetString("ResetTournament") ?? "Turnier zurücksetzen";
            var message = _localizationService?.GetString("ResetTournamentConfirm") ?? 
                         "Möchten Sie wirklich das gesamte Turnier zurücksetzen?\n\n⚠ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ResetToGroupPhase();
                
                var successMessage = _localizationService?.GetString("TournamentResetComplete") ?? "Das Turnier wurde erfolgreich zurückgesetzt.";
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
    }

    private void AddGroupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _localizationService?.GetString("NewGroup") ?? "Neue Gruppe";
            var prompt = _localizationService?.GetString("GroupName") ?? "Geben Sie den Namen der neuen Gruppe ein:";
            var defaultName = $"Group {_nextGroupId}";
            
            var name = ShowInputDialog(prompt, title, defaultName);
            
            if (!string.IsNullOrWhiteSpace(name))
            {
                var group = new Group(_nextGroupId, name.Trim());
                TournamentClass.Groups.Add(group);
                
                UpdateNextIds();
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show($"Fehler beim Hinzufügen der Gruppe: {ex.Message}", errorTitle, 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
    {
        if (GroupsListBox.SelectedItem is not Group selectedGroup)
        {
            var title = _localizationService?.GetString("NoGroupSelectedTitle") ?? "Keine Gruppe ausgewählt";
            var message = _localizationService?.GetString("NoGroupSelected") ?? "Bitte wählen Sie eine Gruppe aus, die entfernt werden soll.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var title = _localizationService?.GetString("RemoveGroupTitle") ?? "Gruppe entfernen";
            var message = _localizationService?.GetString("RemoveGroupConfirm", selectedGroup.Name) ?? 
                         $"Möchten Sie die Gruppe '{selectedGroup.Name}' wirklich entfernen?\nAlle Spieler in dieser Gruppe werden ebenfalls entfernt.";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                TournamentClass.Groups.Remove(selectedGroup);
                SelectedGroup = null;
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show($"Fehler beim Entfernen der Gruppe: {ex.Message}", errorTitle, 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedGroup = GroupsListBox.SelectedItem as Group;
        RemoveGroupButton.IsEnabled = SelectedGroup != null;
    }

    private void PlayerNameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && AddPlayerButton.IsEnabled)
        {
            AddPlayerButton_Click(sender, new RoutedEventArgs());
        }
    }

    private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null)
        {
            var message = _localizationService?.GetString("SelectGroupFirst") ?? "Bitte wählen Sie zuerst eine Gruppe aus.";
            var title = _localizationService?.GetString("NoGroupSelectedTitle") ?? "Keine Gruppe ausgewählt";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPlayerName))
        {
            var message = _localizationService?.GetString("EnterPlayerName") ?? "Bitte geben Sie einen Spielernamen ein.";
            var title = _localizationService?.GetString("NoNameEntered") ?? "Kein Name eingegeben";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var player = new Player(_nextPlayerId, NewPlayerName.Trim());
            SelectedGroup.Players.Add(player);
            
            NewPlayerName = string.Empty;
            UpdateNextIds();
            UpdatePlayersView();
            OnDataChanged();
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show($"Fehler beim Hinzufügen des Spielers: {ex.Message}", errorTitle, 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
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

        try
        {
            var title = _localizationService?.GetString("RemovePlayerTitle") ?? "Spieler entfernen";
            var message = _localizationService?.GetString("RemovePlayerConfirm", SelectedPlayer.Name) ?? 
                         $"Möchten Sie den Spieler '{SelectedPlayer.Name}' wirklich entfernen?";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                SelectedGroup?.Players.Remove(SelectedPlayer);
                SelectedPlayer = null;
                UpdatePlayersView();
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show($"Fehler beim Entfernen des Spielers: {ex.Message}", errorTitle, 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GenerateMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null) return;

        try
        {
            if (SelectedGroup.Players.Count < 2)
            {
                var message = _localizationService?.GetString("MinimumTwoPlayers") ?? "Mindestens 2 Spieler erforderlich.";
                var title = _localizationService?.GetString("Warning") ?? "Warnung";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedGroup.GenerateRoundRobinMatches();
            UpdateMatchesView();
            UpdatePlayersView();
            OnDataChanged();

            var successMessage = _localizationService?.GetString("MatchesGeneratedSuccess") ?? "Spiele wurden erfolgreich erstellt!";
            var successTitle = _localizationService?.GetString("Information") ?? "Information";
            MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            var errorMessage = $"{_localizationService?.GetString("ErrorGeneratingMatches") ?? "Fehler beim Erstellen der Spiele:"} {ex.Message}";
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PlayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedPlayer = PlayersListBox.SelectedItem as Player;
    }

    private void GroupPhaseGroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedGroup = GroupPhaseGroupsList.SelectedItem as Group;
        UpdateMatchesView();
    }

    private void FinalsMatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FinalsMatchesDataGrid.SelectedItem is Match selectedMatch && !selectedMatch.IsBye && _localizationService != null)
        {
            var resultWindow = new MatchResultWindow(selectedMatch, TournamentClass.GameRules, _localizationService);
            resultWindow.Owner = Window.GetWindow(this);
            
            if (resultWindow.ShowDialog() == true)
            {
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.ScoreDisplay));
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.StatusDisplay));
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.WinnerDisplay));
                
                RefreshFinalsView();
                
                // Auto-check finals completion
                Task.Run(() => CheckFinalsCompletion());
                
                OnDataChanged();
            }
        }
    }

    private void KnockoutMatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (KnockoutMatchesDataGrid.SelectedItem is KnockoutMatchViewModel viewModel)
        {
            OpenMatchResultWindow(viewModel.Match);
        }
    }

    private void KnockoutDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Handle right-click context menu for knockout matches if needed
        // This can be implemented later for additional functionality
    }

    private void LoserBracketDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LoserBracketDataGrid.SelectedItem is KnockoutMatchViewModel viewModel)
        {
            OpenMatchResultWindow(viewModel.Match);
        }
    }

    private void GiveByeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is KnockoutMatchViewModel viewModel)
        {
            var match = viewModel.Match;
            
            // Check if only one player is available for automatic bye
            if (match.Player1 != null && match.Player2 == null)
            {
                HandleKnockoutBye(match, match.Player1);
            }
            else if (match.Player1 == null && match.Player2 != null)
            {
                HandleKnockoutBye(match, match.Player2);
            }
            else if (match.Player1 != null && match.Player2 != null)
            {
                // Both players available - show selection dialog
                var title = _localizationService?.GetString("GiveBye") ?? "Freilos vergeben";
                var message = _localizationService?.GetString("SelectByeWinner") ?? "Wählen Sie den Spieler, der das Freilos erhalten soll:";
                
                var dialog = new Window
                {
                    Title = title,
                    Width = 350,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this)
                };
                
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                
                var label = new Label { Content = message, Margin = new Thickness(15) };
                Grid.SetRow(label, 0);
                
                var playerPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(15)
                };
                
                Player? selectedPlayer = null;
                
                var player1Button = new Button
                {
                    Content = match.Player1.Name,
                    Width = 120,
                    Height = 35,
                    Margin = new Thickness(10)
                };
                player1Button.Click += (s, e) => { selectedPlayer = match.Player1; dialog.DialogResult = true; };
                
                var player2Button = new Button
                {
                    Content = match.Player2.Name,
                    Width = 120,
                    Height = 35,
                    Margin = new Thickness(10)
                };
                player2Button.Click += (s, e) => { selectedPlayer = match.Player2; dialog.DialogResult = true; };
                
                playerPanel.Children.Add(player1Button);
                playerPanel.Children.Add(player2Button);
                Grid.SetRow(playerPanel, 1);
                
                var cancelButton = new Button
                {
                    Content = _localizationService?.GetString("Cancel") ?? "Abbrechen",
                    Width = 80,
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(15),
                    IsCancel = true
                };
                Grid.SetRow(cancelButton, 2);
                
                grid.Children.Add(label);
                grid.Children.Add(playerPanel);
                grid.Children.Add(cancelButton);

                dialog.Content = grid;
                
                if (dialog.ShowDialog() == true && selectedPlayer != null)
                {
                    HandleKnockoutBye(match, selectedPlayer);
                }
            }
            else
            {
                // No players available
                HandleKnockoutBye(match, null);
            }
        }
        else if (sender is Button button2 && button2.Tag is KnockoutMatch match)
        {
            HandleKnockoutBye(match, null);
        }
    }

    // ...existing methods remain unchanged...

    private void ResetToGroupPhase()
    {
        // Remove all phases except group phase
        var phasesToRemove = TournamentClass.Phases
            .Where(p => p.PhaseType != TournamentPhaseType.GroupPhase)
            .ToList();
        
        foreach (var phase in phasesToRemove)
        {
            TournamentClass.Phases.Remove(phase);
        }
        
        // Reset group phase to active
        var groupPhase = TournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        if (groupPhase != null)
        {
            groupPhase.IsActive = true;
            groupPhase.IsCompleted = false;
            TournamentClass.CurrentPhase = groupPhase;
        }
        
        // Clear bracket canvases
        if (BracketCanvas != null)
        {
            BracketCanvas.Children.Clear();
        }
        
        if (LoserBracketCanvas != null)
        {
            LoserBracketCanvas.Children.Clear();
        }
        
        // Clear knockout/finals data grids
        KnockoutParticipantsListBox.ItemsSource = null;
        KnockoutMatchesDataGrid.ItemsSource = null;
        LoserBracketDataGrid.ItemsSource = null;
        FinalistsListBox.ItemsSource = null;
        FinalsMatchesDataGrid.ItemsSource = null;
        FinalsStandingsDataGrid.ItemsSource = null;
        
        // Update UI
        UpdateUI();
        UpdatePlayersView();
        UpdateMatchesView();
        
        // Switch to setup tab
        MainTabControl.SelectedItem = SetupTabItem;
        
        // Mark as changed
        OnDataChanged();
    }

    // Helper method to open match result window
    private void OpenMatchResultWindow(KnockoutMatch knockoutMatch)
    {
        if (_localizationService == null) return;

        try
        {
            // Erstelle dummy RoundRules basierend auf GameRules
            var roundRules = new RoundRules
            {
                LegsToWin = TournamentClass.GameRules.LegsToWin,
                SetsToWin = TournamentClass.GameRules.PlayWithSets ? TournamentClass.GameRules.SetsToWin : 0,
                LegsPerSet = TournamentClass.GameRules.LegsPerSet
            };
            
            var resultWindow = new MatchResultWindow(knockoutMatch, roundRules, TournamentClass.GameRules, _localizationService);
            resultWindow.Owner = Window.GetWindow(this);
            
            if (resultWindow.ShowDialog() == true)
            {
                RefreshKnockoutView();
                
                // WICHTIG: Explizit als geändert markieren für Speicher-Status
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show($"Fehler beim Öffnen des Match-Ergebnisfensters: {ex.Message}", errorTitle, 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// NEU: Zeigt eine Benachrichtigung wenn alle Gruppen abgeschlossen sind
    /// </summary>
    private void ShowGroupPhaseCompletionNotification()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ShowGroupPhaseCompletionNotification: All groups completed!");
            
            // Verwende einen Timer um zu vermeiden, dass die Nachricht mehrmals angezeigt wird
            if (_lastCompletionNotification.HasValue && 
                DateTime.Now - _lastCompletionNotification.Value < TimeSpan.FromSeconds(5))
            {
                System.Diagnostics.Debug.WriteLine("  Skipping notification - too recent");
                return;
            }
            
            _lastCompletionNotification = DateTime.Now;
            
            var title = _localizationService?.GetString("Information") ?? "Information";
            var message = _localizationService?.GetString("AllGroupsCompleted") ?? 
                         "🎉 Alle Gruppen sind abgeschlossen!\n\nSie können jetzt zur nächsten Phase wechseln.";
            
            // Zeige Toast-Benachrichtigung statt störendem MessageBox
            ShowToastNotification(title, message);
            
            System.Diagnostics.Debug.WriteLine("  Completion notification shown");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowGroupPhaseCompletionNotification: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// NEU: Zeigt eine Benachrichtigung wenn die Finals abgeschlossen sind
    /// </summary>
    private void ShowFinalsCompletionNotification()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ShowFinalsCompletionNotification: Finals completed!");
            
            // Verwende einen Timer um zu vermeiden, dass die Nachricht mehrmals angezeigt wird
            if (_lastCompletionNotification.HasValue && 
                DateTime.Now - _lastCompletionNotification.Value < TimeSpan.FromSeconds(5))
            {
                System.Diagnostics.Debug.WriteLine("  Skipping notification - too recent");
                return;
            }
            
            _lastCompletionNotification = DateTime.Now;
            
            var title = _localizationService?.GetString("Information") ?? "Information";
            var message = _localizationService?.GetString("FinalsCompleted") ?? 
                         "🏆 Die Finalrunde ist abgeschlossen!\n\nAlle Spiele wurden beendet. Das Turnier ist komplett!";
            
            // Zeige Toast-Benachrichtigung
            ShowToastNotification(title, message);
            
            System.Diagnostics.Debug.WriteLine("  Finals completion notification shown");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowFinalsCompletionNotification: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// NEU: Zeigt eine dezente Toast-Benachrichtigung
    /// </summary>
    private void ShowToastNotification(string title, string message)
    {
        try
        {
            // Finde das parent window
            var parentWindow = Window.GetWindow(this);
            if (parentWindow == null) return;
            
            // Erstelle ein einfaches Toast-Panel
            var toastPanel = new Border
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(230, 76, 175, 80)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                MaxWidth = 400,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20),
                Effect = new DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    Direction = 270,
                    ShadowDepth = 4,
                    BlurRadius = 8,
                    Opacity = 0.3
                }
            };
            
            var stackPanel = new StackPanel();
            
            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 4)
            };
            
            var messageBlock = new TextBlock
            {
                Text = message,
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };
            
            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(messageBlock);
            toastPanel.Child = stackPanel;
            
            // Finde oder erstelle ein Grid für Overlays
            var mainGrid = parentWindow.Content as Grid;
            if (mainGrid == null)
            {
                // Fallback zu MessageBox wenn kein Grid gefunden
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Füge Toast zum Grid hinzu
            Grid.SetRowSpan(toastPanel, mainGrid.RowDefinitions.Count > 0 ? mainGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(toastPanel, mainGrid.ColumnDefinitions.Count > 0 ? mainGrid.ColumnDefinitions.Count : 1);
            mainGrid.Children.Add(toastPanel);
            
            // Auto-remove nach 4 Sekunden
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                mainGrid.Children.Remove(toastPanel);
            };
            timer.Start();
            
            System.Diagnostics.Debug.WriteLine("Toast notification displayed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowToastNotification: ERROR: {ex.Message}");
            // Fallback zu normalem MessageBox
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private DateTime? _lastCompletionNotification;

    // Helper method für knockout bye handling
    private void HandleUndoKnockoutBye(KnockoutMatch match)
    {
        try
        {
            bool success = TournamentClass.UndoBye(match);
            
            if (success)
            {
                RefreshKnockoutView();
                
                // WICHTIG: Explizit als geändert markieren für Speicher-Status
                OnDataChanged();
                
                System.Diagnostics.Debug.WriteLine($"Successfully undid bye for match {match.Id}");
            }
            else
            {
                var errorMessage = _localizationService?.GetString("Error") ?? "Fehler";
                MessageBox.Show("Freilos konnte nicht rückgängig gemacht werden.", errorMessage, 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show($"Fehler beim Rückgängigmachen des Freiloses: {ex.Message}", errorTitle, 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // NEW: Handle knockout bye operations
    private void HandleKnockoutBye(KnockoutMatch match, Player? byeWinner)
    {
        try
        {
            bool success = TournamentClass.GiveManualBye(match, byeWinner);
            
            if (success)
            {
                RefreshKnockoutView();
                
                // WICHTIG: Explizit als geändert markieren für Speicher-Status
                OnDataChanged();
                
                var winnerName = byeWinner?.Name ?? "automatic winner";
                System.Diagnostics.Debug.WriteLine($"Successfully gave bye to {winnerName} in match {match.Id}");
            }
            else
            {
                var errorMessage = _localizationService?.GetString("Error") ?? "Fehler";
                MessageBox.Show("Freilos konnte nicht vergeben werden.", errorMessage, 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService?.GetString("Error") ?? "Fehler";
            MessageBox.Show($"Fehler beim Vergeben des Freiloses: {ex.Message}", errorTitle, 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string? ShowInputDialog(string prompt, string title, string defaultValue = "")
    {
        var dialog = new Window
        {
            Title = title,
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.CanResize
        };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var label = new Label
        {
            Content = prompt,
            Margin = new Thickness(15),
            FontWeight = FontWeights.Medium
        };
        Grid.SetRow(label, 0);

        var textBox = new TextBox
        {
            Text = defaultValue,
            Margin = new Thickness(15, 5, 15, 15),
            MinHeight = 30,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(textBox, 1);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(15),
            Height = 40
        };

        var okButton = new Button
        {
            Content = _localizationService?.GetString("OK") ?? "OK",
            Width = 90,
            Height = 32,
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true
        };

        var cancelButton = new Button
        {
            Content = _localizationService?.GetString("Cancel") ?? "Abbrechen",
            Width = 90,
            Height = 32,
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

        dialog.Loaded += (s, e) => { textBox.Focus(); textBox.SelectAll(); };

        return dialog.ShowDialog() == true ? textBox.Text?.Trim() : null;
    }

    // Helper methods
    private async Task CheckAllGroupsCompletion()
    {
        // LoadingSpinner anzeigen
        var parentWindow = Window.GetWindow(this);
        var mainGrid = parentWindow?.Content as Grid;
        LoadingSpinner? spinner = null;
        
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CheckAllGroupsCompletion START ===");
            
            // Zeige LoadingSpinner falls Main Grid verfügbar
            if (mainGrid != null)
            {
                var loadingText = _localizationService?.GetString("CheckingGroupStatus") ?? "Überprüfe Gruppenstatus...";
                spinner = mainGrid.ShowLoadingSpinner(loadingText);
            }
            
            // Kleine Verzögerung damit der Spinner sichtbar wird
            await Task.Delay(100);
            
            var completedGroups = 0;
            var totalGroups = TournamentClass.Groups.Count;
            
            // Update progress
            if (spinner != null)
            {
                await spinner.Dispatcher.BeginInvoke(() =>
                {
                    var progressText = _localizationService?.GetString("ProcessingMatches") ?? "Verarbeite Spiele...";
                    spinner.ProgressText = progressText;
                });
            }
            
            foreach (var group in TournamentClass.Groups)
            {
                var status = group.CheckCompletionStatus();
                System.Diagnostics.Debug.WriteLine($"  Group {group.Name}: {status}");
                
                if (status.IsComplete)
                {
                    completedGroups++;
                }
                
                // NEU: Automatische Match-Status-Reparatur falls nötig
                if (!status.IsComplete && group.MatchesGenerated)
                {
                    group.RepairMatchStatuses();
                    
                    // Nach der Reparatur nochmal prüfen
                    var repairedStatus = group.CheckCompletionStatus();
                    System.Diagnostics.Debug.WriteLine($"  Group {group.Name} after repair: {repairedStatus}");
                    
                    if (repairedStatus.IsComplete)
                    {
                        completedGroups++;
                    }
                }
                
                // Kleine Pause für UI Responsiveness
                await Task.Delay(50);
            }
            
            // Update progress
            if (spinner != null)
            {
                await spinner.Dispatcher.BeginInvoke(() =>
                {
                    var progressText = _localizationService?.GetString("CheckingCompletion") ?? "Überprüfe Abschluss...";
                    spinner.ProgressText = progressText;
                });
            }
            
            await Task.Delay(100); // Kurze Verzögerung für visuelle Klarheit
            
            System.Diagnostics.Debug.WriteLine($"CheckAllGroupsCompletion: {completedGroups}/{totalGroups} groups completed");
            
            // Update UI to reflect completion status
            UpdatePhaseDisplay();
            UpdateTournamentOverview();
            
            // NEU: Zeige Benachrichtigung wenn alle Gruppen abgeschlossen sind
            if (completedGroups == totalGroups && totalGroups > 0 && 
                TournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None)
            {
                ShowGroupPhaseCompletionNotification();
            }
            
            System.Diagnostics.Debug.WriteLine("=== CheckAllGroupsCompletion END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckAllGroupsCompletion: ERROR: {ex.Message}");
        }
        finally
        {
            // LoadingSpinner verstecken
            if (mainGrid != null && spinner != null)
            {
                // Kleine Verzögerung damit User den Spinner sehen kann
                await Task.Delay(200);
                mainGrid.HideLoadingSpinner(spinner);
            }
        }
    }

    /// <summary>
    /// NEU: Überprüft automatisch die Finals-Phase auf Vollständigkeit
    /// </summary>
    private async Task CheckFinalsCompletion()
    {
        // LoadingSpinner anzeigen
        var parentWindow = Window.GetWindow(this);
        var mainGrid = parentWindow?.Content as Grid;
        LoadingSpinner? spinner = null;
        
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CheckFinalsCompletion START ===");
            
            if (TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine("  Not in finals phase");
                return;
            }
            
            // Zeige LoadingSpinner falls Main Grid verfügbar
            if (mainGrid != null)
            {
                var loadingText = _localizationService?.GetString("CheckingFinalsStatus") ?? "Überprüfe Finalrunden-Status...";
                spinner = mainGrid.ShowLoadingSpinner(loadingText);
            }
            
            // Kleine Verzögerung damit der Spinner sichtbar wird
            await Task.Delay(100);
            
            var finalsGroup = TournamentClass.CurrentPhase.FinalsGroup;
            if (finalsGroup == null)
            {
                System.Diagnostics.Debug.WriteLine("  No finals group found");
                return;
            }
            
            // Update progress
            if (spinner != null)
            {
                await spinner.Dispatcher.BeginInvoke(() =>
                {
                    var progressText = _localizationService?.GetString("ProcessingFinalsMatches") ?? "Verarbeite Finals-Spiele...";
                    spinner.ProgressText = progressText;
                });
            }
            
            var status = finalsGroup.CheckCompletionStatus();
            System.Diagnostics.Debug.WriteLine($"  Finals group: {status}");
            
            // Auto-repair if needed
            if (!status.IsComplete && finalsGroup.MatchesGenerated)
            {
                finalsGroup.RepairMatchStatuses();
                await Task.Delay(100);
                status = finalsGroup.CheckCompletionStatus();
                System.Diagnostics.Debug.WriteLine($"  Finals group after repair: {status}");
            }
            
            // Update progress
            if (spinner != null)
            {
                await spinner.Dispatcher.BeginInvoke(() =>
                {
                    var progressText = _localizationService?.GetString("CheckingCompletion") ?? "Überprüfe Abschluss...";
                    spinner.ProgressText = progressText;
                });
            }
            
            await Task.Delay(100);
            
            // Update UI
            UpdatePhaseDisplay();
            UpdateTournamentOverview();
            
            // Show notification if finals are complete
            if (status.IsComplete)
            {
                ShowFinalsCompletionNotification();
            }
            
            System.Diagnostics.Debug.WriteLine("=== CheckFinalsCompletion END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckFinalsCompletion: ERROR: {ex.Message}");
        }
        finally
        {
            // LoadingSpinner verstecken
            if (mainGrid != null && spinner != null)
            {
                // Kleine Verzögerung damit User den Spinner sehen kann
                await Task.Delay(200);
                mainGrid.HideLoadingSpinner(spinner);
            }
        }
    }
}

public static class UIElementExtensions
{
    public static T? FindAncestor<T>(this DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T ancestor)
            {
                return ancestor;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}