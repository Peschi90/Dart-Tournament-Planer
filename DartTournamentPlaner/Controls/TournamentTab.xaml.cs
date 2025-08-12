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
    private DispatcherTimer? _refreshTimer;

    public TournamentClass TournamentClass
    {
        get => _tournamentClass;
        set
        {
            // Unsubscribe from old tournament class events
            if (_tournamentClass != null)
            {
                _tournamentClass.UIRefreshRequested -= OnTournamentUIRefreshRequested;
            }

            _tournamentClass = value;
            
            // Subscribe to new tournament class events
            if (_tournamentClass != null)
            {
                _tournamentClass.UIRefreshRequested += OnTournamentUIRefreshRequested;
            }

            OnPropertyChanged();
            UpdateUI();
            
            // Ensure knockout view is refreshed if we're in knockout phase
            if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                // Use Dispatcher to ensure UI is ready
                Dispatcher.BeginInvoke(() =>
                {
                    RefreshKnockoutView();
                    UpdateMatchesView();
                }, DispatcherPriority.Loaded);
            }
        }
    }

    /// <summary>
    /// Event-Handler für UI-Refresh-Anfragen vom TournamentClass
    /// </summary>
    private void OnTournamentUIRefreshRequested(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"OnTournamentUIRefreshRequested: Refreshing knockout view");
        
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SelectedGroup setter START ===");
                System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: Current = {_selectedGroup?.Name ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: New = {value?.Name ?? "null"}");
                
                // Prevent infinite recursion if the same group is selected
                if (_selectedGroup == value) 
                {
                    System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: Same group, returning");
                    return;
                }
                
                // Unsubscribe from previous group's events
                if (_selectedGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: Unsubscribing from previous group events");
                    // Unsubscribe from match property changes
                    foreach (var match in _selectedGroup.Matches)
                    {
                        match.PropertyChanged -= Match_PropertyChanged;
                    }
                    // Unsubscribe from collection changes
                    _selectedGroup.Matches.CollectionChanged -= Matches_CollectionChanged;
                }

                _selectedGroup = value;
                System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: Set _selectedGroup to {value?.Name ?? "null"}");
                OnPropertyChanged();
                
                // Subscribe to new group's events
                if (_selectedGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: Subscribing to new group events");
                    // Subscribe to existing match property changes
                    foreach (var match in _selectedGroup.Matches)
                    {
                        match.PropertyChanged += Match_PropertyChanged;
                    }
                    
                    // Subscribe to collection changes to handle new matches
                    _selectedGroup.Matches.CollectionChanged += Matches_CollectionChanged;
                }
                
                // Update UI - simplified approach
                System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: Calling UpdatePlayersView");
                UpdatePlayersView();
                
                System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: Calling UpdateMatchesView");
                UpdateMatchesView();
                
                System.Diagnostics.Debug.WriteLine($"=== SelectedGroup setter END ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: CRITICAL ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"SelectedGroup setter: Stack trace: {ex.StackTrace}");
                _selectedGroup = null;
                OnPropertyChanged();
                ClearViewsSafely();
            }
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
            _localizationService.PropertyChanged += (s, e) => UpdateTranslations();
            UpdateTranslations();
        }
    }

    private void UpdateTranslations()
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
        
        // Update DataGrid columns
        UpdateDataGridHeaders();
        
        UpdatePlayersView(); // This will update the players header text
        UpdatePhaseDisplay();
        
        // Ensure knockout phase is properly loaded if we're in that phase
        if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            RefreshKnockoutView();
        }
    }

    private void UpdateDataGridHeaders()
    {
        if (_localizationService == null) return;

        // Update Matches DataGrid headers
        if (MatchesDataGrid.Columns.Count >= 3)
        {
            MatchesDataGrid.Columns[0].Header = _localizationService.GetString("Match") ?? "Match";
            MatchesDataGrid.Columns[1].Header = _localizationService.GetString("Result");
            MatchesDataGrid.Columns[2].Header = _localizationService.GetString("Status") ?? "Status";
        }

        // Update Finals Matches DataGrid headers
        if (FinalsMatchesDataGrid.Columns.Count >= 3)
        {
            FinalsMatchesDataGrid.Columns[0].Header = _localizationService.GetString("Match") ?? "Match";
            FinalsMatchesDataGrid.Columns[1].Header = _localizationService.GetString("Result");
            FinalsMatchesDataGrid.Columns[2].Header = _localizationService.GetString("Status") ?? "Status";
        }

        // Update Knockout Matches DataGrid headers
        if (KnockoutMatchesDataGrid.Columns.Count >= 4)
        {
            KnockoutMatchesDataGrid.Columns[0].Header = _localizationService.GetString("Round") ?? "Runde";
            KnockoutMatchesDataGrid.Columns[1].Header = _localizationService.GetString("Match") ?? "Match";
            KnockoutMatchesDataGrid.Columns[2].Header = _localizationService.GetString("Result");
            KnockoutMatchesDataGrid.Columns[3].Header = _localizationService.GetString("Status") ?? "Status";
        }

        // Update Standings DataGrid headers
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
        // Prevent duplicate items by only setting if different
        if (GroupsListBox.ItemsSource != TournamentClass.Groups)
        {
            GroupsListBox.ItemsSource = TournamentClass.Groups;
        }
        
        if (GroupPhaseGroupsList.ItemsSource != TournamentClass.Groups)
        {
            GroupPhaseGroupsList.ItemsSource = TournamentClass.Groups;
        }
        
        UpdateNextIds();
        UpdatePhaseDisplay();
    }

    private void UpdateNextIds()
    {
        // Update next group ID
        if (TournamentClass.Groups.Count > 0)
        {
            _nextGroupId = TournamentClass.Groups.Max(g => g.Id) + 1;
        }

        // Update next player ID
        var allPlayers = TournamentClass.Groups.SelectMany(g => g.Players);
        if (allPlayers.Any())
        {
            _nextPlayerId = allPlayers.Max(p => p.Id) + 1;
        }
    }

    private void UpdatePlayersView()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Starting - Current phase = {TournamentClass?.CurrentPhase?.PhaseType}");
            
            // Check current phase and show appropriate content
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                // Original group phase logic
                if (SelectedGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Group phase with selected group = {SelectedGroup.Name}");
                    
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
                            
                            System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Successfully updated UI for group {SelectedGroup.Name}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView for selected group: {ex.Message}");
                        }
                    }, DispatcherPriority.DataBind);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Group phase with no selected group");
                    
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
                            PlayersHeaderText.Text = $"Finalisten ({finalsGroup.Players.Count} Spieler):";
                            

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
                        PlayersHeaderText.Text = $"KO-Teilnehmer ({qualifiedPlayers.Count} Spieler):";
                        
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
            
            UpdatePhaseDisplay(); // Update phase info whenever players view changes
            System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private void UpdatePhaseDisplay()
    {
        if (TournamentClass?.CurrentPhase == null) return;

        try
        {
            System.Diagnostics.Debug.WriteLine($"=== UpdatePhaseDisplay START ===");
            
            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: Current phase = {TournamentClass.CurrentPhase.PhaseType}");

            // Update current phase text
            var phaseText = TournamentClass.CurrentPhase.PhaseType switch
            {
                TournamentPhaseType.GroupPhase => _localizationService?.GetString("GroupPhase") ?? "Gruppenphase",
                TournamentPhaseType.RoundRobinFinals => _localizationService?.GetString("FinalsPhase") ?? "Finalrunde",
                TournamentPhaseType.KnockoutPhase => _localizationService?.GetString("KnockoutPhase") ?? "KO-Phase",
                _ => "Unbekannte Phase"
            };

            CurrentPhaseText.Text = $"Aktuelle Phase: {phaseText}";
            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: Set phase text to '{phaseText}'");

            // Update tab visibility based on rules
            var hasPostPhase = TournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None;
            var hasRoundRobinFinals = TournamentClass.GameRules.PostGroupPhaseMode == PostGroupPhaseMode.RoundRobinFinals;
            var hasKnockout = TournamentClass.GameRules.PostGroupPhaseMode == PostGroupPhaseMode.KnockoutBracket;
            var hasDoubleElimination = TournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination;

            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: PostGroupPhaseMode = {TournamentClass.GameRules.PostGroupPhaseMode}");
            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: hasPostPhase = {hasPostPhase}, hasKnockout = {hasKnockout}");

            // Show/hide tabs based on configuration
            FinalsTabItem.Visibility = hasRoundRobinFinals ? Visibility.Visible : Visibility.Collapsed;
            KnockoutTabItem.Visibility = hasKnockout ? Visibility.Visible : Visibility.Collapsed;
            LoserBracketTab.Visibility = hasKnockout && hasDoubleElimination ? Visibility.Visible : Visibility.Collapsed;
            LoserBracketTreeTab.Visibility = hasKnockout && hasDoubleElimination ? Visibility.Visible : Visibility.Collapsed;

            // Check if we can advance to next phase
            bool canAdvance = false;
            try
            {
                canAdvance = TournamentClass.CanProceedToNextPhase() && 
                           TournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None;
                System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: CanProceedToNextPhase = {TournamentClass.CanProceedToNextPhase()}");
                System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: canAdvance = {canAdvance}");
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

            // Show reset tournament button only if there are matches or we're not in group phase
            var hasGeneratedMatches = TournamentClass.Groups.Any(g => g.MatchesGenerated && g.Matches.Count > 0);
            var isInAdvancedPhase = TournamentClass.CurrentPhase?.PhaseType != TournamentPhaseType.GroupPhase;
            ResetTournamentButton.IsEnabled = hasGeneratedMatches || isInAdvancedPhase;

            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: hasGeneratedMatches = {hasGeneratedMatches}, isInAdvancedPhase = {isInAdvancedPhase}");

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
                        AdvanceToNextPhaseButton.Content = $"🏆 {nextPhaseText} starten";
                        System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: Set next phase button text to '{nextPhaseText} starten'");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: ERROR in GetNextPhase: {ex.Message}");
                }
            }

            // Update tournament overview
            UpdateTournamentOverview();
            
            System.Diagnostics.Debug.WriteLine($"=== UpdatePhaseDisplay END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: Stack trace: {ex.StackTrace}");
        }
    }

    private void UpdateTournamentOverview()
    {
        if (TournamentClass == null) return;

        var overview = $"🏆 Turnier: {TournamentClass.Name}\n\n";
        overview += $"🎯 Aktuelle Phase: {TournamentClass.CurrentPhase?.Name}\n";
        overview += $"👥 Gruppen: {TournamentClass.Groups.Count}\n";
        overview += $"🎮 Spieler gesamt: {TournamentClass.Groups.SelectMany(g => g.Players).Count()}\n\n";
        
        overview += $"📋 Spielregeln:\n{TournamentClass.GameRules}\n\n";

        if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
        {
            var finishedGroups = TournamentClass.Groups.Count(g => g.MatchesGenerated && g.Matches.All(m => m.Status == MatchStatus.Finished || m.IsBye));
            overview += $"✅ Abgeschlossene Gruppen: {finishedGroups}/{TournamentClass.Groups.Count}\n";
        }
        else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
            var qualifiedCount = TournamentClass.CurrentPhase.QualifiedPlayers.Count;
            overview += $"🏅 Qualifizierte Spieler: {qualifiedCount}\n";
        }
        else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            var totalMatches = TournamentClass.CurrentPhase.WinnerBracket.Count;
            var finishedMatches = TournamentClass.CurrentPhase.WinnerBracket.Count(m => m.Status == MatchStatus.Finished);
            overview += $"⚔️ KO-Spiele: {finishedMatches}/{totalMatches} beendet\n";
        }

        TournamentOverviewText.Text = overview;
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
            
            // Create a dynamic view of matches with correct round names AND bye button status
            var winnerBracketMatches = TournamentClass.CurrentPhase.WinnerBracket.Select(match =>
                new KnockoutMatchViewModel(match, TournamentClass)).ToList();
            
            KnockoutMatchesDataGrid.ItemsSource = winnerBracketMatches;
            
            if (TournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                var loserBracketMatches = TournamentClass.CurrentPhase.LoserBracket.Select(match =>
                    new KnockoutMatchViewModel(match, TournamentClass)).ToList();
                
                LoserBracketDataGrid.ItemsSource = loserBracketMatches;
                
                // Show loser bracket tabs for double elimination
                LoserBracketTab.Visibility = Visibility.Visible;
                LoserBracketTreeTab.Visibility = Visibility.Visible;
            }
            else
            {
                // Hide loser bracket tabs for single elimination
                LoserBracketTab.Visibility = Visibility.Collapsed;
                LoserBracketTreeTab.Visibility = Visibility.Collapsed;
            }
            
            // Update bracket visualization
            DrawBracketTree();
            DrawLoserBracketTree();
        }
    }

    private void DrawBracketTree()
    {
        if (TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase || BracketCanvas == null)
            return;

        BracketCanvas.Children.Clear();
        
        var matches = TournamentClass.CurrentPhase.WinnerBracket.OrderBy(m => m.Round).ThenBy(m => m.Position).ToList();
        if (!matches.Any()) return;

        // Calculate bracket dimensions
        var rounds = matches.GroupBy(m => m.Round).Count();
        var maxMatchesInRound = matches.GroupBy(m => m.Round).Max(g => g.Count());
        
        double matchWidth = 200;
        double matchHeight = 70;
        double roundSpacing = 250;
        double baseMatchSpacing = 80; // Base spacing for early rounds
        
        // Calculate canvas size - needs to be larger for proper tree spacing
        double canvasWidth = rounds * roundSpacing + matchWidth;
        double canvasHeight = Math.Max(600, maxMatchesInRound * baseMatchSpacing * 4); // More height for tree spacing
        
        BracketCanvas.Width = Math.Max(800, canvasWidth);
        BracketCanvas.Height = Math.Max(600, canvasHeight);

        // Group matches by round
        var matchesByRound = matches.GroupBy(m => m.Round).OrderBy(g => g.Key).ToList();
        
        // Calculate Y positions for all rounds first (tree-like spacing)
        var roundPositions = CalculateTreePositions(matchesByRound.ToArray(), canvasHeight, baseMatchSpacing);
        
        for (int roundIndex = 0; roundIndex < matchesByRound.Count; roundIndex++)
        {
            var roundMatches = matchesByRound[roundIndex].OrderBy(m => m.Position).ToList();
            double x = roundIndex * roundSpacing + 50;
            
            // Get pre-calculated positions for this round
            var yPositions = roundPositions[roundIndex];
            
            for (int matchIndex = 0; matchIndex < roundMatches.Count; matchIndex++)
            {
                var match = roundMatches[matchIndex];
                double y = yPositions[matchIndex];
                
                // Draw match box
                var matchBorder = new Border
                {
                    Width = matchWidth,
                    Height = matchHeight,
                    Background = GetMatchBackground(match.Status),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5)
                };
                
                var matchStackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                // Match title
                var titleText = new TextBlock
                {
                    Text = match.GetDynamicRoundDisplay(TournamentClass.CurrentPhase.QualifiedPlayers.Count, _localizationService),
                    FontWeight = FontWeights.Bold,
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.DarkBlue
                };
                matchStackPanel.Children.Add(titleText);
                
                // Player names and score
                var player1Text = new TextBlock
                {
                    Text = match.Player1?.Name ?? "TBD",
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                
                var scoreText = new TextBlock
                {
                    Text = match.ScoreDisplay,
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };
                
                var player2Text = new TextBlock
                {
                    Text = match.Player2?.Name ?? "TBD",
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                
                matchStackPanel.Children.Add(player1Text);
                matchStackPanel.Children.Add(scoreText);
                matchStackPanel.Children.Add(player2Text);
                
                matchBorder.Child = matchStackPanel;
                
                // Position the match
                Canvas.SetLeft(matchBorder, x);
                Canvas.SetTop(matchBorder, y);
                
                // Add click event for match editing - Support both single and double click
                matchBorder.MouseLeftButtonDown += (sender, e) =>
                {
                    if (match.Player1 != null && match.Player2 != null)
                    {
                        KnockoutMatchesDataGrid.SelectedItem = new KnockoutMatchViewModel(match, TournamentClass);
                        
                        // Handle double click for result entry
                        if (e.ClickCount == 2)
                        {
                            OpenMatchResultWindow(match);
                        }
                    }
                };
                
                // Add right-click context menu
                matchBorder.MouseRightButtonDown += (sender, e) =>
                {
                    CreateAndShowKnockoutMatchContextMenu(match, matchBorder, e);
                };
                
                matchBorder.Cursor = Cursors.Hand;
                BracketCanvas.Children.Add(matchBorder);
                
                // Draw connection lines to next round (except for final)
                if (roundIndex < matchesByRound.Count - 1)
                {
                    var nextRoundPositions = roundPositions[roundIndex + 1];
                    DrawTreeConnectionLine(x + matchWidth, y + matchHeight / 2, 
                                         x + roundSpacing, nextRoundPositions, 
                                         matchIndex, roundMatches.Count);
                }
            }
        }
    }

    /// <summary>
    /// Calculates Y positions for all rounds to create a proper bracket structure
    /// where each match is positioned exactly in the middle between its two source matches
    /// </summary>
    /// <param name="matchesByRound">Matches grouped by round</param>
    /// <param name="canvasHeight">Total canvas height</param>
    /// <param name="baseSpacing">Base spacing for the first round</param>
    /// <returns>Dictionary of round positions</returns>
    private Dictionary<int, List<double>> CalculateTreePositions(
        IGrouping<KnockoutRound, KnockoutMatch>[] matchesByRound, 
        double canvasHeight, 
        double baseSpacing)
    {
        var positions = new Dictionary<int, List<double>>();
        
        // First round: start at the top with minimal margin
        var firstRound = matchesByRound[0].OrderBy(m => m.Position).ToList();
        var firstRoundPositions = new List<double>();
        
        // Start near the top with a small margin (instead of centering)
        double topMargin = 50; // Small margin from the top
        double startY = topMargin;
        
        for (int i = 0; i < firstRound.Count; i++)
        {
            firstRoundPositions.Add(startY + i * baseSpacing);
        }
        positions[0] = firstRoundPositions;
        
        System.Diagnostics.Debug.WriteLine($"First round: {firstRound.Count} matches, starting at Y={startY}, spacing: {baseSpacing}px");
        for (int i = 0; i < firstRoundPositions.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"  Match {i}: Y = {firstRoundPositions[i]}");
        }
        
        // Calculate subsequent rounds: each match positioned exactly in the middle of its two source matches
        for (int roundIndex = 1; roundIndex < matchesByRound.Length; roundIndex++)
        {
            var currentRound = matchesByRound[roundIndex].OrderBy(m => m.Position).ToList();
            var currentRoundPositions = new List<double>();
            var previousRoundPositions = positions[roundIndex - 1];
            
            System.Diagnostics.Debug.WriteLine($"Round {roundIndex + 1}: {currentRound.Count} matches");
            
            for (int matchIndex = 0; matchIndex < currentRound.Count; matchIndex++)
            {
                // Each match gets its position from the middle of two previous matches
                int sourceIndex1 = matchIndex * 2;      // First source match
                int sourceIndex2 = matchIndex * 2 + 1;  // Second source match
                
                double y1 = sourceIndex1 < previousRoundPositions.Count ? previousRoundPositions[sourceIndex1] : startY;
                double y2 = sourceIndex2 < previousRoundPositions.Count ? previousRoundPositions[sourceIndex2] : y1;
                
                // Position exactly in the middle between the two source matches
                double middleY = (y1 + y2) / 2;
                currentRoundPositions.Add(middleY);
                
                System.Diagnostics.Debug.WriteLine($"  Match {matchIndex}: Y = {middleY} (between {y1} and {y2})");
            }
            
            positions[roundIndex] = currentRoundPositions;
        }
        
        return positions;
    }

    /// <summary>
    /// Draws connection lines for proper bracket structure
    /// Each line connects two matches to their common target match in the next round
    /// </summary>
    private void DrawTreeConnectionLine(double startX, double startY, double endX, 
                                      List<double> nextRoundPositions, 
                                      int matchIndex, int totalMatches)
    {
        // Draw horizontal line from match to connection point
        var horizontalLine = new Line
        {
            X1 = startX,
            Y1 = startY,
            X2 = startX + 80,
            Y2 = startY,
            Stroke = Brushes.Gray,
            StrokeThickness = 2
        };
        BracketCanvas.Children.Add(horizontalLine);
        
        // Draw bracket connections: every pair of matches connects to one match in next round
        int targetMatchIndex = matchIndex / 2;
        
        if (targetMatchIndex < nextRoundPositions.Count)
        {
            double targetY = nextRoundPositions[targetMatchIndex];
            double connectionX = startX + 80;
            
            // Draw vertical line from current match to target Y level
            var verticalLine = new Line
            {
                X1 = connectionX,
                Y1 = startY,
                X2 = connectionX,
                Y2 = targetY,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            BracketCanvas.Children.Add(verticalLine);
            
            // For even-indexed matches (first of each pair), draw the horizontal connection to next round
            if (matchIndex % 2 == 0)
            {
                var toNextRoundLine = new Line
                {
                    X1 = connectionX,
                    Y1 = targetY,
                    X2 = endX,
                    Y2 = targetY,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 2
                };
                BracketCanvas.Children.Add(toNextRoundLine);
                
                System.Diagnostics.Debug.WriteLine($"Connected matches {matchIndex} and {matchIndex + 1} to target {targetMatchIndex} at Y={targetY}");
            }
        }
    }
    private void DrawLoserBracketTree()
    {
        if (TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase || LoserBracketCanvas == null)
            return;

        LoserBracketCanvas.Children.Clear();

        var matches = TournamentClass.CurrentPhase.LoserBracket.OrderBy(m => m.Round).ThenBy(m => m.Position).ToList();
        if (!matches.Any()) return;

        // Calculate bracket dimensions
        var rounds = matches.GroupBy(m => m.Round).Count();
        var maxMatchesInRound = matches.GroupBy(m => m.Round).Max(g => g.Count());

        double matchWidth = 200;
        double matchHeight = 70;
        double roundSpacing = 250;
        double baseMatchSpacing = 80;

        // Calculate canvas size - larger for proper tree spacing
        double canvasWidth = rounds * roundSpacing + matchWidth;
        double canvasHeight = Math.Max(600, maxMatchesInRound * baseMatchSpacing * 4);

        LoserBracketCanvas.Width = Math.Max(800, canvasWidth);
        LoserBracketCanvas.Height = Math.Max(600, canvasHeight);

        // Group matches by round
        var matchesByRound = matches.GroupBy(m => m.Round).OrderBy(g => g.Key).ToList();

        // Calculate Y positions for all rounds first (tree-like spacing)
        var roundPositions = CalculateLoserTreePositions(matchesByRound.ToArray(), canvasHeight, baseMatchSpacing);

        for (int roundIndex = 0; roundIndex < matchesByRound.Count; roundIndex++)
        {
            var roundMatches = matchesByRound[roundIndex].OrderBy(m => m.Position).ToList();
            double x = roundIndex * roundSpacing + 50;

            // Get pre-calculated positions for this round
            var yPositions = roundPositions[roundIndex];

            for (int matchIndex = 0; matchIndex < roundMatches.Count; matchIndex++)
            {
                var match = roundMatches[matchIndex];
                double y = yPositions[matchIndex];

                // Draw match box with loser bracket styling
                var matchBorder = new Border
                {
                    Width = matchWidth,
                    Height = matchHeight,
                    Background = GetLoserMatchBackground(match.Status),
                    BorderBrush = Brushes.DarkRed,
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(5)
                };

                var matchStackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Match title with loser bracket indication
                var titleText = new TextBlock
                {
                    Text = match.GetDynamicRoundDisplay(TournamentClass.CurrentPhase.QualifiedPlayers.Count, _localizationService),
                    FontWeight = FontWeights.Bold,
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = Brushes.DarkRed
                };
                matchStackPanel.Children.Add(titleText);

                // Player names and score
                var player1Text = new TextBlock
                {
                    Text = match.Player1?.Name ?? "TBD",
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var scoreText = new TextBlock
                {
                    Text = match.ScoreDisplay,
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold
                };

                var player2Text = new TextBlock
                {
                    Text = match.Player2?.Name ?? "TBD",
                    FontSize = 11,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                matchStackPanel.Children.Add(player1Text);
                matchStackPanel.Children.Add(scoreText);
                matchStackPanel.Children.Add(player2Text);

                matchBorder.Child = matchStackPanel;

                // Position the match
                Canvas.SetLeft(matchBorder, x);
                Canvas.SetTop(matchBorder, y);

                // Add click event for match editing - Support both single and double click
                matchBorder.MouseLeftButtonDown += (sender, e) =>
                {
                    if (match.Player1 != null && match.Player2 != null)
                    {
                        LoserBracketDataGrid.SelectedItem = new KnockoutMatchViewModel(match, TournamentClass);

                        // Handle double click for result entry
                        if (e.ClickCount == 2)
                        {
                            OpenMatchResultWindow(match);
                        }
                    }
                };

                // Add right-click context menu
                matchBorder.MouseRightButtonDown += (sender, e) =>
                {
                    CreateAndShowKnockoutMatchContextMenu(match, matchBorder, e);
                };

                matchBorder.Cursor = Cursors.Hand;
                LoserBracketCanvas.Children.Add(matchBorder);

                // Draw connection lines to next round (except for final)
                if (roundIndex < matchesByRound.Count - 1)
                {
                    var nextRoundPositions = roundPositions[roundIndex + 1];
                    DrawLoserTreeConnectionLine(x + matchWidth, y + matchHeight / 2,
                                           x + roundSpacing, nextRoundPositions,
                                           matchIndex, roundMatches.Count);
                }
            }
        }
    }
    /// <summary>
    /// Calculates Y positions for loser bracket rounds using improved bracket structure
    /// </summary>
    private Dictionary<int, List<double>> CalculateLoserTreePositions(
        IGrouping<KnockoutRound, KnockoutMatch>[] matchesByRound, 
        double canvasHeight, 
        double baseSpacing)
    {
        var positions = new Dictionary<int, List<double>>();
        
        // First round: start with more spacing to avoid overlaps
        var firstRound = matchesByRound[0].OrderBy(m => m.Position).ToList();
        var firstRoundPositions = new List<double>();
        
        // Start near the top with appropriate spacing
        double topMargin = 50;
        double startY = topMargin;
        
        // VERBESSERTE SPACING-LOGIK: Mehr Platz zwischen Matches
        double adjustedSpacing = Math.Max(baseSpacing, 100); // Mindestens 100px Abstand
        
        // If there are many matches, reduce spacing but keep minimum
        if (firstRound.Count > 8)
        {
            adjustedSpacing = Math.Max(80, (canvasHeight - 200) / firstRound.Count);
        }
        
        System.Diagnostics.Debug.WriteLine($"LoserBracket First round: {firstRound.Count} matches, spacing: {adjustedSpacing}px");
        
        for (int i = 0; i < firstRound.Count; i++)
        {
            double yPosition = startY + i * adjustedSpacing;
            firstRoundPositions.Add(yPosition);
            System.Diagnostics.Debug.WriteLine($"  LB Match {i} (ID:{firstRound[i].Id}): Y = {yPosition}");
        }
        positions[0] = firstRoundPositions;
        
        // Calculate subsequent rounds using improved bracket logic
        for (int roundIndex = 1; roundIndex < matchesByRound.Length; roundIndex++)
        {
            var currentRound = matchesByRound[roundIndex].OrderBy(m => m.Position).ToList();
            var currentRoundPositions = new List<double>();
            var previousRoundPositions = positions[roundIndex - 1];
            
            System.Diagnostics.Debug.WriteLine($"LoserBracket Round {roundIndex + 1}: {currentRound.Count} matches");
            
            for (int matchIndex = 0; matchIndex < currentRound.Count; matchIndex++)
            {
                double yPosition;
                
                if (currentRound.Count == 1 && roundIndex == matchesByRound.Length - 1)
                {
                    // Final match - center it well
                    if (previousRoundPositions.Count > 0)
                    {
                        yPosition = (previousRoundPositions.Min() + previousRoundPositions.Max()) / 2;
                    }
                    else
                    {
                        yPosition = startY + 300; // Fallback position
                    }
                }
                else
                {
                    // Standard bracket logic with safety checks
                    int sourceIndex1 = matchIndex * 2;
                    int sourceIndex2 = matchIndex * 2 + 1;
                    
                    double y1 = sourceIndex1 < previousRoundPositions.Count ? 
                               previousRoundPositions[sourceIndex1] : startY;
                    double y2 = sourceIndex2 < previousRoundPositions.Count ? 
                               previousRoundPositions[sourceIndex2] : y1;
                    
                    // Position in the middle with minimum spacing
                    yPosition = (y1 + y2) / 2;
                    
                    // ANTI-OVERLAP PROTECTION: Ensure minimum distance between matches
                    if (matchIndex > 0 && currentRoundPositions.Count > 0)
                    {
                        double lastPosition = currentRoundPositions.Last();
                        double minDistance = 80; // Minimum distance between matches
                        
                        if (yPosition - lastPosition < minDistance)
                        {
                            yPosition = lastPosition + minDistance;
                            System.Diagnostics.Debug.WriteLine($"  LB Adjusted position for match {matchIndex} to avoid overlap: {yPosition}");
                        }
                    }
                }
                
                currentRoundPositions.Add(yPosition);
                System.Diagnostics.Debug.WriteLine($"  LB Match {matchIndex} (ID:{currentRound[matchIndex].Id}): Y = {yPosition}");
            }
            
            positions[roundIndex] = currentRoundPositions;
        }
        
        return positions;
    }

    /// <summary>
    /// Draws connection lines for loser bracket using bracket structure
    /// </summary>
    private void DrawLoserTreeConnectionLine(double startX, double startY, double endX, 
                                           List<double> nextRoundPositions, 
                                           int matchIndex, int totalMatches)
    {
        // Draw horizontal line from match to connection point (red style for loser bracket)
        var horizontalLine = new Line
        {
            X1 = startX,
            Y1 = startY,
            X2 = startX + 80,
            Y2 = startY,
            Stroke = Brushes.DarkRed,
            StrokeThickness = 2
        };
        LoserBracketCanvas.Children.Add(horizontalLine);
        
        // Draw bracket connections for loser bracket
        int targetMatchIndex = matchIndex / 2;
        
        if (targetMatchIndex < nextRoundPositions.Count)
        {
            double targetY = nextRoundPositions[targetMatchIndex];
            double connectionX = startX + 80;
            
            // Draw vertical line from current match to target Y level (red style)
            var verticalLine = new Line
            {
                X1 = connectionX,
                Y1 = startY,
                X2 = connectionX,
                Y2 = targetY,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };
            LoserBracketCanvas.Children.Add(verticalLine);
            
            // For even-indexed matches, draw the horizontal connection to next round (red style)
            if (matchIndex % 2 == 0)
            {
                var toNextRoundLine = new Line
                {
                    X1 = connectionX,
                    Y1 = targetY,
                    X2 = endX,
                    Y2 = targetY,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 2
                };
                LoserBracketCanvas.Children.Add(toNextRoundLine);
            }
        }
    }

    private void ConfigureRulesButton_Click(object sender, RoutedEventArgs e)
    {
        if (_localizationService == null) return;

        var rulesWindow = new GameRulesWindow(TournamentClass.GameRules, _localizationService);
        rulesWindow.Owner = Window.GetWindow(this);
        
        // Subscribe to data changes
        rulesWindow.DataChanged += (s, args) =>
        {
            System.Diagnostics.Debug.WriteLine("TournamentTab: GameRulesWindow DataChanged received, triggering OnDataChanged...");
            OnDataChanged();
        };
        
        if (rulesWindow.ShowDialog() == true)
        {
            OnDataChanged();
        }
    }

    private void AddGroupButton_Click(object sender, RoutedEventArgs e)
    {
        var defaultName = _localizationService?.GetString("Group", _nextGroupId) ?? $"Gruppe {_nextGroupId}";
        var title = _localizationService?.GetString("NewGroup") ?? "Neue Gruppe";
        var prompt = _localizationService?.GetString("GroupName") ?? "Geben Sie den Namen der neuen Gruppe ein:";

        // Simple dialog alternative
        string? groupName = ShowInputDialog(prompt, title, defaultName);
        
        if (!string.IsNullOrWhiteSpace(groupName))
        {
            var group = new Group { Id = _nextGroupId++, Name = groupName.Trim() };
            TournamentClass.Groups.Add(group);
            
            // Subscribe to player changes in the new group
            group.Players.CollectionChanged += (s, e) => OnDataChanged();
            group.Matches.CollectionChanged += (s, e) => OnDataChanged();
        }
    }

    /// <summary>
    /// Handles removing a group from the tournament
    /// </summary>
    private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
    {
        if (GroupsListBox.SelectedItem is Group selectedGroup)
        {
            var title = _localizationService?.GetString("RemoveGroupTitle") ?? "Gruppe entfernen";
            var confirmMessage = _localizationService?.GetString("RemoveGroupConfirm", selectedGroup.Name) ?? 
                         $"Möchten Sie die Gruppe '{selectedGroup.Name}' wirklich entfernen?\nAlle Spieler in dieser Gruppe werden ebenfalls entfernt.";

            // Add warning about tournament reset if in advanced phase
            if (TournamentClass.CurrentPhase?.PhaseType != TournamentPhaseType.GroupPhase)
            {
                confirmMessage += "\n\n" + (_localizationService?.GetString("TournamentResetWarning") ?? "⚠ WARNUNG: Das Turnier wird auf die Gruppenphase zurückgesetzt!");
            }

            var result = MessageBox.Show(confirmMessage, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                TournamentClass.Groups.Remove(selectedGroup);
                if (SelectedGroup == selectedGroup)
                {
                    SelectedGroup = null;
                }
                
                // Reset tournament to group phase
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

    /// <summary>
    /// Gets the background color for a match based on its status
    /// </summary>
    private Brush GetMatchBackground(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.NotStarted => Brushes.LightGray,
            MatchStatus.InProgress => Brushes.LightYellow,
            MatchStatus.Finished => Brushes.LightGreen,
            MatchStatus.Bye => Brushes.LightBlue,
            _ => Brushes.White
        };
    }

    /// <summary>
    /// Gets the background color for a loser bracket match based on its status
    /// </summary>
    private Brush GetLoserMatchBackground(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.NotStarted => Brushes.LightCoral,
            MatchStatus.InProgress => Brushes.Orange,
            MatchStatus.Finished => Brushes.LightPink,
            MatchStatus.Bye => Brushes.LightCyan,
            _ => Brushes.White
        };
    }

    /// <summary>
    /// Resets the tournament to group phase
    /// </summary>
    private void ResetToGroupPhase()
    {
        // Clear all advanced phases
        var phasesToRemove = TournamentClass.Phases
            .Where(p => p.PhaseType != TournamentPhaseType.GroupPhase)
            .ToList();
        
        foreach (var phase in phasesToRemove)
        {
            TournamentClass.Phases.Remove(phase);
        }
        
        // Reset to group phase
        var groupPhase = TournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        if (groupPhase != null)
        {
            groupPhase.IsActive = true;
            groupPhase.IsCompleted = false;
            TournamentClass.CurrentPhase = groupPhase;
        }
        
        // Update UI
        UpdatePhaseDisplay();
        UpdateMatchesView();
        
        // Switch back to setup tab
        MainTabControl.SelectedItem = SetupTabItem;
    }

    /// <summary>
    /// Checks if there are subsequent matches that depend on this match result
    /// </summary>
    private bool HasSubsequentMatches(KnockoutMatch match)
    {
        if (TournamentClass?.CurrentPhase == null) return false;

        var allMatches = TournamentClass.CurrentPhase.WinnerBracket.Concat(TournamentClass.CurrentPhase.LoserBracket);
        
        return allMatches.Any(m => 
            (m.SourceMatch1 == match || m.SourceMatch2 == match) && 
            m.Status != MatchStatus.NotStarted);
    }

    /// <summary>
    /// Updates knockout progression after a match is completed
    /// </summary>
    private void UpdateKnockoutProgression(KnockoutMatch completedMatch)
    {
        if (TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase || completedMatch.Winner == null)
            return;

        System.Diagnostics.Debug.WriteLine($"=== UpdateKnockoutProgression START for match {completedMatch.Id} ===");
        System.Diagnostics.Debug.WriteLine($"  Winner: {completedMatch.Winner.Name}");
        System.Diagnostics.Debug.WriteLine($"  Status: {completedMatch.Status}");
        System.Diagnostics.Debug.WriteLine($"  BracketType: {completedMatch.BracketType}");

        // Use the new ProcessMatchResult method which handles progression correctly
        bool success = TournamentClass.ProcessMatchResult(completedMatch);
        
        if (success)
        {
            System.Diagnostics.Debug.WriteLine($"  Match result processed successfully");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"  Failed to process match result");
        }

        System.Diagnostics.Debug.WriteLine($"=== UpdateKnockoutProgression END ===");
    }

    /// <summary>
    /// Handles adding a player
    /// </summary>
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

    /// <summary>
    /// Handles generating matches for the selected group
    /// </summary>
    private void GenerateMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup?.Players.Count < 2)
        {
            MessageBox.Show("Mindestens 2 Spieler sind erforderlich.", "Information", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            SelectedGroup.GenerateRoundRobinMatches();
            
            var message = _localizationService?.GetString("MatchesGenerated") ?? "Spiele wurden erfolgreich generiert!";
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            
            UpdateMatchesView();
            UpdatePlayersView(); // Update button states
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Generieren der Spiele: {ex.Message}", "Fehler", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles resetting matches for the selected group
    /// </summary>
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
            UpdatePlayersView(); // Update button states
            DataChanged?.Invoke(this, EventArgs.Empty);
            
            var successMessage = _localizationService?.GetString("MatchesReset") ?? "Spiele wurden zurückgesetzt!";
            MessageBox.Show(successMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Handles removing a player  
    /// </summary>
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

    /// <summary>
    /// Simple input dialog alternative
    /// </summary>
    private string? ShowInputDialog(string prompt, string title, string defaultValue = "")
    {
        // For now, use a simple approach - in a real implementation you might create a proper dialog
        return defaultValue; // Placeholder - returns default value for now
    }

    /// <summary>
    /// Event handler for match property changes
    /// </summary>
    private void Match_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Match.Status) or nameof(Match.Winner) or nameof(Match.Player1Sets) or nameof(Match.Player2Sets))
        {
            // Throttled UI update to prevent performance issues
            ThrottledUpdateMatchesView();
        }
    }

    /// <summary>
    /// Event handler for matches collection changes
    /// </summary>
    private void Matches_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Subscribe to new matches
        if (e.NewItems != null)
        {
            foreach (Match match in e.NewItems)
            {
                match.PropertyChanged += Match_PropertyChanged;
            }
        }

        // Unsubscribe from removed matches
        if (e.OldItems != null)
        {
            foreach (Match match in e.OldItems)
            {
                match.PropertyChanged -= Match_PropertyChanged;
            }
        }

        // Update UI
        ThrottledUpdateMatchesView();
    }

    /// <summary>
    /// Throttled update to prevent excessive UI updates
    /// </summary>
    private void ThrottledUpdateMatchesView()
    {
        _refreshTimer?.Stop();
        _refreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _refreshTimer.Tick += (s, e) =>
        {
            _refreshTimer.Stop();
            _refreshTimer = null;
            UpdateMatchesView();
        };
        _refreshTimer.Start();
    }

    /// <summary>
    /// Updates the matches view based on current phase and selected group
    /// </summary>
    private void UpdateMatchesView()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: Starting - Current phase = {TournamentClass?.CurrentPhase?.PhaseType}");

            // Clear all match views first
            MatchesDataGrid.ItemsSource = null;
            StandingsDataGrid.ItemsSource = null;

            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                // Group phase logic
                if (SelectedGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: Updating for group {SelectedGroup.Name}");
                    
                    // Update matches
                    MatchesDataGrid.ItemsSource = SelectedGroup.Matches;
                    
                    // Update standings if there are players
                    if (SelectedGroup.Players.Count > 0)
                    {
                        var standings = SelectedGroup.GetStandings();
                        StandingsDataGrid.ItemsSource = standings;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: No group selected");
                }
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                // Finals phase
                var finalsGroup = TournamentClass.CurrentPhase.FinalsGroup;
                if (finalsGroup != null)
                {
                    MatchesDataGrid.ItemsSource = finalsGroup.Matches;
                    var standings = finalsGroup.GetStandings();
                    StandingsDataGrid.ItemsSource = standings;
                }
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                // Knockout phase - matches are handled in separate grids
                System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: Knockout phase - using separate grids");
            }

            System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: Completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in UpdateMatchesView: {ex.Message}");
        }
    }

    /// <summary>
    /// Safely clears all views when an error occurs
    /// </summary>
    private void ClearViewsSafely()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ClearViewsSafely: Starting");

            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    PlayersListBox.ItemsSource = null;
                    MatchesDataGrid.ItemsSource = null;
                    StandingsDataGrid.ItemsSource = null;
                    
                    PlayersHeaderText.Text = "Spieler: (Fehler)";
                    
                    PlayerNameTextBox.IsEnabled = false;
                    AddPlayerButton.IsEnabled = false;
                    GenerateMatchesButton.IsEnabled = false;
                    ResetMatchesButton.IsEnabled = false;
                    
                    System.Diagnostics.Debug.WriteLine("ClearViewsSafely: Views cleared");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in ClearViewsSafely: {ex.Message}");
                }
            }, DispatcherPriority.DataBind);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Critical error in ClearViewsSafely: {ex.Message}");
        }
    }

    /// <summary>
    /// Opens the match result window for a knockout match
    /// </summary>
    private void OpenMatchResultWindow(KnockoutMatch match)
    {
        if (match.Player1 == null || match.Player2 == null || _localizationService == null) return;

        // Convert KnockoutMatch to Match for the result window
        var tempMatch = new Match
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
            Notes = match.Notes
        };

        var resultWindow = new MatchResultWindow(tempMatch, TournamentClass.GameRules, _localizationService);
        resultWindow.Owner = Window.GetWindow(this);

        if (resultWindow.ShowDialog() == true)
        {
            // Copy results back to KnockoutMatch
            var resultMatch = resultWindow.InternalMatch;
            match.Player1Sets = resultMatch.Player1Sets;
            match.Player2Sets = resultMatch.Player2Sets;
            match.Player1Legs = resultMatch.Player1Legs;
            match.Player2Legs = resultMatch.Player2Legs;
            match.Winner = resultMatch.Winner;
            match.Loser = match.Winner == resultMatch.Player1 ? resultMatch.Player2 : resultMatch.Player1;
            match.Status = resultMatch.Status;
            match.Notes = resultMatch.Notes;
            match.EndTime = DateTime.Now;

            // Update next round matches if needed
            UpdateKnockoutProgression(match);
            RefreshKnockoutView();
            OnDataChanged();
        }
    }

    /// <summary>
    /// Creates and shows context menu for knockout matches
    /// </summary>
    private void CreateAndShowKnockoutMatchContextMenu(KnockoutMatch match, Border matchBorder, MouseButtonEventArgs e)
    {
        var contextMenu = new ContextMenu();

        if (match.Player1 != null && match.Player2 != null && match.Status == MatchStatus.NotStarted)
        {
            // Enter result menu item
            var enterResultItem = new MenuItem
            {
                Header = _localizationService?.GetString("EnterResult") ?? "Ergebnis eingeben"
            };
            enterResultItem.Click += (s, args) => OpenMatchResultWindow(match);
            contextMenu.Items.Add(enterResultItem);

            // Bye options if applicable
            var giveByeItem = new MenuItem
            {
                Header = _localizationService?.GetString("SelectByeWinner") ?? "Freilos-Gewinner wählen"
            };
            
            var player1ByeItem = new MenuItem
            {
                Header = _localizationService?.GetString("GiveByeToPlayer", match.Player1.Name) ?? $"Freilos an {match.Player1.Name}"
            };
            player1ByeItem.Click += (s, args) => GiveByeToPlayer(match, match.Player1);
            
            var player2ByeItem = new MenuItem
            {
                Header = _localizationService?.GetString("GiveByeToPlayer", match.Player2.Name) ?? $"Freilos an {match.Player2.Name}"
            };
            player2ByeItem.Click += (s, args) => GiveByeToPlayer(match, match.Player2);

            giveByeItem.Items.Add(player1ByeItem);
            giveByeItem.Items.Add(player2ByeItem);
            contextMenu.Items.Add(giveByeItem);
        }
        else if (match.Status == MatchStatus.Finished && match.Winner != null)
        {
            // Remove bye option if it's a bye
            if (match.Player2 == null || match.Player1 == null)
            {
                var removeByeItem = new MenuItem
                {
                    Header = _localizationService?.GetString("RemoveBye") ?? "Freilos entfernen"
                };
                removeByeItem.Click += (s, args) => RemoveByeFromMatch(match);
                contextMenu.Items.Add(removeByeItem);
            }
        }

        if (contextMenu.Items.Count > 0)
        {
            contextMenu.PlacementTarget = matchBorder;
            contextMenu.IsOpen = true;
        }

        e.Handled = true;
    }

    /// <summary>
    /// Gives a bye to the specified player
    /// </summary>
    private void GiveByeToPlayer(KnockoutMatch match, Player player)
    {
        try
        {
            match.Winner = player;
            match.Loser = player == match.Player1 ? match.Player2 : match.Player1;
            match.Status = MatchStatus.Finished;
            match.EndTime = DateTime.Now;
            match.Notes = _localizationService?.GetString("AutomaticByeDetected", player.Name) ?? $"Automatisches Freilos für {player.Name}";

            UpdateKnockoutProgression(match);
            RefreshKnockoutView();
            OnDataChanged();

            var message = _localizationService?.GetString("ByeGiven") ?? "Freilos wurde vergeben";
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            var errorMessage = _localizationService?.GetString("ByeOperationFailed") ?? "Freilos-Operation fehlgeschlagen";
            MessageBox.Show($"{errorMessage}: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Removes a bye from a match
    /// </summary>
    private void RemoveByeFromMatch(KnockoutMatch match)
    {
        try
        {
            // Check if subsequent matches have been played
            if (HasSubsequentMatches(match))
            {
                var cannotUndoMessage = _localizationService?.GetString("ByeCannotBeUndone") ?? "Freilos kann nicht rückgängig gemacht werden - nachfolgende Matches bereits gespielt";
                MessageBox.Show(cannotUndoMessage, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            match.Winner = null;
            match.Loser = null;
            match.Status = MatchStatus.NotStarted;
            match.EndTime = null;
            match.Notes = "";
            match.Player1Sets = 0;
            match.Player2Sets = 0;
            match.Player1Legs = 0;
            match.Player2Legs = 0;

            RefreshKnockoutView();
            DataChanged?.Invoke(this, EventArgs.Empty);

            var successMessage = _localizationService?.GetString("ByeUndone") ?? "Freilos wurde rückgängig gemacht";
            MessageBox.Show(successMessage, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            var errorMessage = _localizationService?.GetString("ByeOperationFailed") ?? "Freilos-Operation fehlgeschlagen";
            MessageBox.Show($"{errorMessage}: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles knockout phase reset
    /// </summary>
    private void ResetKnockoutButton_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService?.GetString("ResetKnockoutTitle") ?? "KO-Phase zurücksetzen";
        var message = _localizationService?.GetString("ResetKnockoutConfirm") ?? 
                     "Möchten Sie die KO-Phase wirklich zurücksetzen?\n\n⚠ Alle KO-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird zur Gruppenphase zurückgesetzt.";

        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            ResetToGroupPhase();
            
            var successMessage = _localizationService?.GetString("ResetKnockoutComplete") ?? "KO-Phase wurde erfolgreich zurückgesetzt.";
            MessageBox.Show(successMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
            
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Triggers the data changed event
    /// </summary>
    protected virtual void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Handles tab control selection changes
    /// </summary>
    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // When switching to knockout tab, ensure the view is refreshed
        if (e.AddedItems.Count > 0 && e.AddedItems[0] == KnockoutTabItem)
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                // Validierung hinzufügen bevor K.O.-Tab angezeigt wird
                var qualifiedParticipants = TournamentClass.CurrentPhase.QualifiedPlayers?.Count ?? 0;
                
                if (!KnockoutMatch.CanStartKnockoutPhase(qualifiedParticipants))
                {
                    string? error = KnockoutMatch.ValidateKnockoutPhaseStart(qualifiedParticipants);
                    MessageBox.Show(error ?? "K.O.-Phase noch nicht aktiv", 
                                   "K.O.-Phase Warnung", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Zurück zum Gruppen-Tab
                    MainTabControl.SelectedItem = GroupPhaseTabItem;
                    return;
                }
                
                // Use a small delay to ensure UI is fully loaded
                Dispatcher.BeginInvoke(() =>
                {
                    RefreshKnockoutView();
                }, DispatcherPriority.Loaded);
            }
            else
            {
                // Nicht in K.O.-Phase - zurück zur Gruppenphase
                MessageBox.Show("K.O.-Phase ist noch nicht aktiv.", "Information", 
                               MessageBoxButton.OK, MessageBoxImage.Information);
                MainTabControl.SelectedItem = GroupPhaseTabItem;
            }
        }
        // When switching to finals tab, ensure the view is refreshed
        else if (e.AddedItems.Count > 0 && e.AddedItems[0] == FinalsTabItem)
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                RefreshFinalsView();
            }
            else
            {
                // Nicht in Finalrunde - zurück zur Gruppenphase
                MessageBox.Show("Finalrunde ist noch nicht aktiv.", "Information", 
                               MessageBoxButton.OK, MessageBoxImage.Information);
                MainTabControl.SelectedItem = GroupPhaseTabItem;
            }
        }
    }

    /// <summary>
    /// Handles tournament reset button click
    /// </summary>
    private void ResetTournamentButton_Click(object sender, RoutedEventArgs e)
    {
        // Disable button to prevent multiple clicks
        ResetTournamentButton.IsEnabled = false;
        
        try
        {
            var title = _localizationService?.GetString("ResetTournamentTitle") ?? "Turnier komplett zurücksetzen";
            var message = _localizationService?.GetString("ResetTournamentConfirm") ?? 
                         "Möchten Sie das gesamte Turnier wirklich zurücksetzen?\n\n⚠ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Clear all matches from all groups
                foreach (var group in TournamentClass.Groups)
                {
                    group.Matches.Clear();
                    group.MatchesGenerated = false;
                }
                
                // Clear all advanced phases (keep only group phase)
                var phasesToRemove = TournamentClass.Phases
                    .Where(p => p.PhaseType != TournamentPhaseType.GroupPhase)
                    .ToList();
                
                foreach (var phase in phasesToRemove)
                {
                    TournamentClass.Phases.Remove(phase);
                }
                
                // Reset group phase to initial state
                var groupPhase = TournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
                if (groupPhase != null)
                {
                    groupPhase.IsActive = true;
                    groupPhase.IsCompleted = false;
                    TournamentClass.CurrentPhase = groupPhase;
                }
                
                // Clear selected group and player
                SelectedGroup = null;
                SelectedPlayer = null;
                
                // Clear the bracket canvas visually
                if (BracketCanvas != null)
                {
                    BracketCanvas.Children.Clear();
                }
                
                // Clear loser bracket canvas visually
                if (LoserBracketCanvas != null)
                {
                    LoserBracketCanvas.Children.Clear();
                }
                
                // Clear knockout view data sources
                KnockoutParticipantsListBox.ItemsSource = null;
                KnockoutMatchesDataGrid.ItemsSource = null;
                LoserBracketDataGrid.ItemsSource = null;
                
                // Update UI completely
                UpdateUI();
                UpdatePlayersView();
                UpdateMatchesView();
                
                // Switch to setup tab
                MainTabControl.SelectedItem = SetupTabItem;
                
                // Notify about changes
                OnDataChanged();
                
                // Show success message
                var successMessage = _localizationService?.GetString("TournamentResetComplete") ?? "Turnier wurde erfolgreich zurückgesetzt.";
                MessageBox.Show(successMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Zurücksetzen des Turniers: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            // Re-enable button after operation (will be controlled by UpdatePhaseDisplay)
            UpdatePhaseDisplay();
        }
    }

    /// <summary>
    /// Handles groups list box selection changes
    /// </summary>
    private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Group selectedGroup)
        {
            SelectedGroup = selectedGroup;
        }
    }

    /// <summary>
    /// Handles players list box selection changes
    /// </summary>
    private void PlayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Player selectedPlayer)
        {
            SelectedPlayer = selectedPlayer;
        }
    }

    /// <summary>
    /// Handles matches data grid mouse double click
    /// </summary>
    private void MatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (MatchesDataGrid.SelectedItem is Match selectedMatch && !selectedMatch.IsBye && _localizationService != null)
        {
            var resultWindow = new MatchResultWindow(selectedMatch, TournamentClass.GameRules, _localizationService);
            resultWindow.Owner = Window.GetWindow(this);
            
            if (resultWindow.ShowDialog() == true)
            {
                // SOFORTIGE UI-AKTUALISIERUNG NACH SPIELERGEBNIS-EINGABE
                System.Diagnostics.Debug.WriteLine("Match result saved - forcing immediate UI update");
                
                // Stoppe den Timer falls er läuft, um Throttling zu umgehen
                _refreshTimer?.Stop();
                _refreshTimer = null;
                
                // Sofortige Aktualisierung mit höchster Priorität
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Force refresh der DataGrids
                        if (MatchesDataGrid.ItemsSource != null)
                        {
                            MatchesDataGrid.Items.Refresh();
                        }
                        
                        if (StandingsDataGrid.ItemsSource != null)
                        {
                            // Neuberechnung der Standings
                            if (SelectedGroup?.Players.Count > 0)
                            {
                                var standings = SelectedGroup.GetStandings();
                                StandingsDataGrid.ItemsSource = standings;
                            }
                            StandingsDataGrid.Items.Refresh();
                        }
                        
                        // Update der Phasen-Anzeige
                        UpdatePhaseDisplay();
                        
                        System.Diagnostics.Debug.WriteLine("Immediate UI update completed");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in immediate UI update: {ex.Message}");
                    }
                }, DispatcherPriority.Render); // Höchste Priorität für sofortige Anzeige
                
                OnDataChanged();
            }
        }
    }

    /// <summary>
    /// Handles finals matches data grid mouse double click
    /// </summary>
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

    /// <summary>
    /// Handles knockout matches data grid mouse double click
    /// </summary>
    private void KnockoutMatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        KnockoutMatch? selectedMatch = null;
        
        // Handle both direct KnockoutMatch and KnockoutMatchViewModel
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
            // Convert KnockoutMatch to Match for the result window
            var tempMatch = new Match
            {
                Id = selectedMatch.Id,
                Player1 = selectedMatch.Player1,
                Player2 = selectedMatch.Player2,
                Player1Sets = selectedMatch.Player1Sets,
                Player2Sets = selectedMatch.Player2Sets,
                Player1Legs = selectedMatch.Player1Legs,
                Player2Legs = selectedMatch.Player2Legs,
                Winner = selectedMatch.Winner,
                Status = selectedMatch.Status,
                Notes = selectedMatch.Notes
            };

            var resultWindow = new MatchResultWindow(tempMatch, TournamentClass.GameRules, _localizationService);
            resultWindow.Owner = Window.GetWindow(this);
            
            if (resultWindow.ShowDialog() == true)
            {
                // Copy results back to KnockoutMatch
                var match = resultWindow.InternalMatch;
                selectedMatch.Player1Sets = match.Player1Sets;
                selectedMatch.Player2Sets = match.Player2Sets;
                selectedMatch.Player1Legs = match.Player1Legs;
                selectedMatch.Player2Legs = match.Player2Legs;
                selectedMatch.Winner = match.Winner;
                selectedMatch.Loser = match.Winner == match.Player1 ? match.Player2 : match.Player1;
                selectedMatch.Status = match.Status;
                selectedMatch.Notes = match.Notes;
                selectedMatch.EndTime = DateTime.Now;

                // Update next round matches if needed
                UpdateKnockoutProgression(selectedMatch);
                RefreshKnockoutView();
                OnDataChanged();
            }
        }
    }

    /// <summary>
    /// Handles loser bracket data grid mouse double click
    /// </summary>
    private void LoserBracketDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Same logic as KnockoutMatchesDataGrid_MouseDoubleClick
        KnockoutMatch? selectedMatch = null;
        
        // Handle both direct KnockoutMatch and KnockoutMatchViewModel
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
            KnockoutMatchesDataGrid.SelectedItem = selectedMatch;
            KnockoutMatchesDataGrid_MouseDoubleClick(sender, e);
        }
    }

    /// <summary>
    /// Handles the Enter key press in the player name textbox
    /// </summary>
    private void PlayerNameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddPlayerButton_Click(sender, new RoutedEventArgs());
        }
    }

    /// <summary>
    /// Handles group selection changes in the group phase tab
    /// </summary>
    private void GroupPhaseGroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Group selectedGroup)
        {
            SelectedGroup = selectedGroup;
        }
    }

    /// <summary>
    /// Handles advancing to the next phase
    /// </summary>
    private void AdvanceToNextPhaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!TournamentClass.CanProceedToNextPhase())
            {
                var cannotAdvanceMessage = _localizationService?.GetString("CannotAdvancePhase") ?? "Alle Spiele der aktuellen Phase müssen beendet sein";
                MessageBox.Show(cannotAdvanceMessage, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Use AdvanceToNextPhase instead of ProceedToNextPhase
            TournamentClass.AdvanceToNextPhase();
            
            UpdateUI();
            UpdatePlayersView();
            UpdateMatchesView();
            
            // Switch to appropriate tab
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
            MessageBox.Show($"Fehler beim Wechsel zur nächsten Phase: {ex.Message}", "Fehler", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Handles giving a bye to a player
    /// </summary>
    private void GiveByeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is KnockoutMatchViewModel viewModel)
        {
            var match = viewModel.Match;
            
            // Check if both players are present - if so, show selection dialog
            if (match.Player1 != null && match.Player2 != null)
            {
                try
                {
                    var byeSelectionDialog = new ByeSelectionDialog(match, _localizationService);
                    byeSelectionDialog.Owner = Window.GetWindow(this);
                    
                    if (byeSelectionDialog.ShowDialog() == true)
                    {
                        var selectedWinner = byeSelectionDialog.SelectedPlayer;
                        bool success = TournamentClass.GiveManualBye(match, selectedWinner);
                        
                        if (success)
                        {
                            RefreshKnockoutView();
                            OnDataChanged();
                            
                            var successMsg = _localizationService?.GetString("ByeGiven") ?? "Freilos wurde vergeben";
                            MessageBox.Show(successMsg, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            var errorMsg = _localizationService?.GetString("ByeOperationFailed") ?? "Freilos-Operation fehlgeschlagen";
                            MessageBox.Show(errorMsg, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = _localizationService?.GetString("ByeOperationFailed") ?? "Freilos-Operation fehlgeschlagen";
                    MessageBox.Show($"{errorMsg}: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Automatic bye (only one player present or determinable)
                bool success = TournamentClass.GiveManualBye(match);
                
                if (success)
                {
                    RefreshKnockoutView();
                    OnDataChanged();
                    
                    var successMsg = _localizationService?.GetString("ByeGiven") ?? "Freilos wurde vergeben";
                    MessageBox.Show(successMsg, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errorMsg = _localizationService?.GetString("ByeOperationFailed") ?? "Freilos-Operation fehlgeschlagen";
                    MessageBox.Show(errorMsg, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// Handles undoing a bye
    /// </summary>
    private void UndoByeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is KnockoutMatchViewModel viewModel)
        {
            var match = viewModel.Match;
            
            var confirmTitle = _localizationService?.GetString("UndoBye") ?? "Freilos rückgängig machen";
            var confirmMessage = _localizationService?.GetString("UndoByeConfirm") ?? "Möchten Sie das Freilos wirklich rückgängig machen?";
            
            var result = MessageBox.Show(confirmMessage, confirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                bool success = TournamentClass.UndoBye(match);
                
                if (success)
                {
                    RefreshKnockoutView();
                    OnDataChanged();
                    
                    var successMsg = _localizationService?.GetString("ByeUndone") ?? "Freilos wurde rückgängig gemacht";
                    MessageBox.Show(successMsg, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errorMsg = _localizationService?.GetString("ByeCannotBeUndone") ?? "Freilos kann nicht rückgängig gemacht werden";
                    MessageBox.Show(errorMsg, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}