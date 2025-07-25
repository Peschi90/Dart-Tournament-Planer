using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Shapes;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using Microsoft.VisualBasic;

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
            _tournamentClass = value;
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
        overview += $"📋 Aktuelle Phase: {TournamentClass.CurrentPhase?.Name}\n";
        overview += $"👥 Gruppen: {TournamentClass.Groups.Count}\n";
        overview += $"🎯 Spieler gesamt: {TournamentClass.Groups.SelectMany(g => g.Players).Count()}\n\n";
        
        overview += $"⚙️ Spielregeln:\n{TournamentClass.GameRules}\n\n";

        if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
        {
            var finishedGroups = TournamentClass.Groups.Count(g => g.MatchesGenerated && g.Matches.All(m => m.Status == MatchStatus.Finished || m.IsBye));
            overview += $"✅ Abgeschlossene Gruppen: {finishedGroups}/{TournamentClass.Groups.Count}\n";
        }
        else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
            var qualifiedCount = TournamentClass.CurrentPhase.QualifiedPlayers.Count;
            overview += $"🏆 Qualifizierte Spieler: {qualifiedCount}\n";
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
            
            // Create a dynamic view of matches with correct round names
            var winnerBracketMatches = TournamentClass.CurrentPhase.WinnerBracket.Select(match =>
                new KnockoutMatchViewModel(match, TournamentClass.CurrentPhase.QualifiedPlayers.Count, _localizationService)).ToList();
            
            KnockoutMatchesDataGrid.ItemsSource = winnerBracketMatches;
            
            if (TournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                var loserBracketMatches = TournamentClass.CurrentPhase.LoserBracket.Select(match =>
                    new KnockoutMatchViewModel(match, TournamentClass.CurrentPhase.QualifiedPlayers.Count, _localizationService)).ToList();
                
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
        double matchSpacing = 80;
        
        // Calculate canvas size
        double canvasWidth = rounds * roundSpacing + matchWidth;
        double canvasHeight = maxMatchesInRound * matchSpacing + matchHeight;
        
        BracketCanvas.Width = Math.Max(800, canvasWidth);
        BracketCanvas.Height = Math.Max(600, canvasHeight);

        // Group matches by round
        var matchesByRound = matches.GroupBy(m => m.Round).OrderBy(g => g.Key).ToList();
        
        for (int roundIndex = 0; roundIndex < matchesByRound.Count; roundIndex++)
        {
            var roundMatches = matchesByRound[roundIndex].OrderBy(m => m.Position).ToList();
            double x = roundIndex * roundSpacing + 50;
            
            for (int matchIndex = 0; matchIndex < roundMatches.Count; matchIndex++)
            {
                var match = roundMatches[matchIndex];
                
                // Calculate Y position (center matches vertically)
                double totalRoundHeight = roundMatches.Count * matchSpacing;
                double startY = (canvasHeight - totalRoundHeight) / 2;
                double y = startY + matchIndex * matchSpacing;
                
                // Drew match box
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
                        KnockoutMatchesDataGrid.SelectedItem = new KnockoutMatchViewModel(match, TournamentClass.CurrentPhase.QualifiedPlayers.Count, _localizationService);
                        
                        // Handle double click for result entry
                        if (e.ClickCount == 2)
                        {
                            OpenMatchResultWindow(match);
                        }
                    }
                };
                
                matchBorder.Cursor = Cursors.Hand;
                BracketCanvas.Children.Add(matchBorder);
                
                // Draw connection lines to next round (except for final)
                if (roundIndex < matchesByRound.Count - 1)
                {
                    DrawConnectionLine(x + matchWidth, y + matchHeight / 2, 
                                     x + roundSpacing, y + matchHeight / 2, 
                                     matchIndex, roundMatches.Count);
                }
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
        double matchSpacing = 80;
        
        // Calculate canvas size
        double canvasWidth = rounds * roundSpacing + matchWidth;
        double canvasHeight = maxMatchesInRound * matchSpacing + matchHeight;
        
        LoserBracketCanvas.Width = Math.Max(800, canvasWidth);
        LoserBracketCanvas.Height = Math.Max(600, canvasHeight);

        // Group matches by round
        var matchesByRound = matches.GroupBy(m => m.Round).OrderBy(g => g.Key).ToList();
        
        for (int roundIndex = 0; roundIndex < matchesByRound.Count; roundIndex++)
        {
            var roundMatches = matchesByRound[roundIndex].OrderBy(m => m.Position).ToList();
            double x = roundIndex * roundSpacing + 50;
            
            for (int matchIndex = 0; matchIndex < roundMatches.Count; matchIndex++)
            {
                var match = roundMatches[matchIndex];
                
                // Calculate Y position (center matches vertically)
                double totalRoundHeight = roundMatches.Count * matchSpacing;
                double startY = (canvasHeight - totalRoundHeight) / 2;
                double y = startY + matchIndex * matchSpacing;
                
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
                        LoserBracketDataGrid.SelectedItem = new KnockoutMatchViewModel(match, TournamentClass.CurrentPhase.QualifiedPlayers.Count, _localizationService);
                        
                        // Handle double click for result entry
                        if (e.ClickCount == 2)
                        {
                            OpenMatchResultWindow(match);
                        }
                    }
                };
                
                matchBorder.Cursor = Cursors.Hand;
                LoserBracketCanvas.Children.Add(matchBorder);
                
                // Draw connection lines to next round (except for final)
                if (roundIndex < matchesByRound.Count - 1)
                {
                    DrawLoserConnectionLine(x + matchWidth, y + matchHeight / 2, 
                                           x + roundSpacing, y + matchHeight / 2, 
                                           matchIndex, roundMatches.Count);
                }
            }
        }
    }

    private Brush GetMatchBackground(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.NotStarted => Brushes.LightGray,
            MatchStatus.InProgress => Brushes.LightYellow,
            MatchStatus.Finished => Brushes.LightGreen,
            _ => Brushes.White
        };
    }

    private Brush GetLoserMatchBackground(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.NotStarted => Brushes.LightCoral,
            MatchStatus.InProgress => Brushes.Orange,
            MatchStatus.Finished => Brushes.LightPink,
            _ => Brushes.White
        };
    }

    private void DrawConnectionLine(double startX, double startY, double endX, double endY, int matchIndex, int totalMatches)
    {
        // Draw horizontal line from match to middle point
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
        
        // For pairs of matches, draw vertical connector and line to next round
        if (matchIndex % 2 == 0 && matchIndex + 1 < totalMatches)
        {
            // Calculate positions for vertical connector
            double verticalX = startX + 80;
            double currentY = startY;
            double nextMatchY = startY + 80; // Next match down
            double midY = (currentY + nextMatchY) / 2;
            
            // Vertical line connecting two matches
            var verticalLine = new Line
            {
                X1 = verticalX,
                Y1 = currentY,
                X2 = verticalX,
                Y2 = nextMatchY,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            BracketCanvas.Children.Add(verticalLine);
            
            // Horizontal line to next round
            var toNextRoundLine = new Line
            {
                X1 = verticalX,
                Y1 = midY,
                X2 = endX,
                Y2 = midY,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            BracketCanvas.Children.Add(toNextRoundLine);
        }
    }

    private void DrawLoserConnectionLine(double startX, double startY, double endX, double endY, int matchIndex, int totalMatches)
    {
        // Draw horizontal line from match to middle point (red style for loser bracket)
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
        
        // For pairs of matches, draw vertical connector and line to next round
        if (matchIndex % 2 == 0 && matchIndex + 1 < totalMatches)
        {
            // Calculate positions for vertical connector
            double verticalX = startX + 80;
            double currentY = startY;
            double nextMatchY = startY + 80; // Next match down
            double midY = (currentY + nextMatchY) / 2;
            
            // Vertical line connecting two matches (red style)
            var verticalLine = new Line
            {
                X1 = verticalX,
                Y1 = currentY,
                X2 = verticalX,
                Y2 = nextMatchY,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };
            LoserBracketCanvas.Children.Add(verticalLine);
            
            // Horizontal line to next round (red style)
            var toNextRoundLine = new Line
            {
                X1 = verticalX,
                Y1 = midY,
                X2 = endX,
                Y2 = midY,
                Stroke = Brushes.DarkRed,
                StrokeThickness = 2
            };
            LoserBracketCanvas.Children.Add(toNextRoundLine);
        }
    }

    private void ConfigureRulesButton_Click(object sender, RoutedEventArgs e)
    {
        if (_localizationService == null) return;

        var rulesWindow = new GameRulesWindow(TournamentClass.GameRules, _localizationService);
        rulesWindow.Owner = Window.GetWindow(this);
        
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

        var groupName = Interaction.InputBox(prompt, title, defaultName);
        
        if (!string.IsNullOrWhiteSpace(groupName))
        {
            var group = new Group { Id = _nextGroupId++, Name = groupName.Trim() };
            TournamentClass.Groups.Add(group);
            
            // Subscribe to player changes in the new group
            group.Players.CollectionChanged += (s, e) => OnDataChanged();
            group.Matches.CollectionChanged += (s, e) => OnDataChanged();
        }
    }

    private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
    {
        if (GroupsListBox.SelectedItem is Group selectedGroup)
        {
            var title = _localizationService?.GetString("RemoveGroupTitle") ?? "Gruppe entfernen";
            var message = _localizationService?.GetString("RemoveGroupConfirm", selectedGroup.Name) ?? 
                         $"Möchten Sie die Gruppe '{selectedGroup.Name}' wirklich entfernen?\nAlle Spieler in dieser Gruppe werden ebenfalls entfernt.";

            // Add warning about tournament reset if in advanced phase
            if (TournamentClass.CurrentPhase?.PhaseType != TournamentPhaseType.GroupPhase)
            {
                message += "\n\n" + (_localizationService?.GetString("TournamentResetWarning") ?? "⚠️ WARNUNG: Das Turnier wird auf die Gruppenphase zurückgesetzt!");
            }

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                TournamentClass.Groups.Remove(selectedGroup);
                if (SelectedGroup == selectedGroup)
                {
                    SelectedGroup = null;
                }
                
                // Reset tournament to group phase
                ResetToGroupPhase();
                
                OnDataChanged();
            }
        }
        else
        {
            var title = _localizationService?.GetString("NoGroupSelectedTitle") ?? "Keine Gruppe ausgewählt";
            var message = _localizationService?.GetString("NoGroupSelected") ?? "Bitte wählen Sie eine Gruppe aus, die entfernt werden soll.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

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

    private void ResetTournamentButton_Click(object sender, RoutedEventArgs e)
    {
        // Disable button to prevent multiple clicks
        ResetTournamentButton.IsEnabled = false;
        
        try
        {
            var title = _localizationService?.GetString("ResetTournamentTitle") ?? "Turnier komplett zurücksetzen";
            var message = _localizationService?.GetString("ResetTournamentConfirm") ?? 
                         "Möchten Sie das gesamte Turnier wirklich zurücksetzen?\n\n⚠️ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.";

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

    private void MatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (MatchesDataGrid.SelectedItem is Match selectedMatch && !selectedMatch.IsBye && _localizationService != null)
        {
            var resultWindow = new MatchResultWindow(selectedMatch, TournamentClass.GameRules, _localizationService);
            resultWindow.Owner = Window.GetWindow(this);
            
            if (resultWindow.ShowDialog() == true)
            {
                // SOFORTIGE UI-Aktualisierung nach Spielergebnis-Eingabe
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
                selectedMatch.Player1Sets = tempMatch.Player1Sets;
                selectedMatch.Player2Sets = tempMatch.Player2Sets;
                selectedMatch.Player1Legs = tempMatch.Player1Legs;
                selectedMatch.Player2Legs = tempMatch.Player2Legs;
                selectedMatch.Winner = tempMatch.Winner;
                selectedMatch.Loser = tempMatch.Winner == tempMatch.Player1 ? tempMatch.Player2 : tempMatch.Player1;
                selectedMatch.Status = tempMatch.Status;
                selectedMatch.Notes = tempMatch.Notes;
                selectedMatch.EndTime = DateTime.Now;

                // Update next round matches if needed
                UpdateKnockoutProgression(selectedMatch);
                RefreshKnockoutView();
                OnDataChanged();
            }
        }
    }

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

    private void UpdateKnockoutProgression(KnockoutMatch completedMatch)
    {
        if (TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase || completedMatch.Winner == null)
            return;

        // Collect all matches for the update
        var allMatches = new List<KnockoutMatch>();
        allMatches.AddRange(TournamentClass.CurrentPhase.WinnerBracket);
        allMatches.AddRange(TournamentClass.CurrentPhase.LoserBracket);

        // Update next round matches using the new helper method
        KnockoutMatch.UpdateNextRoundFromCompletedMatch(completedMatch, allMatches);

        // If this was a winner bracket match, also update the loser bracket
        if (completedMatch.BracketType == BracketType.Winner)
        {
            KnockoutMatch.UpdateLoserBracketFromWinnerMatch(completedMatch, TournamentClass.CurrentPhase.LoserBracket);
        }
    }

    /// <summary>
    /// Opens the match result window for a knockout match
    /// </summary>
    /// <param name="selectedMatch">The knockout match to edit</param>
    private void OpenMatchResultWindow(KnockoutMatch selectedMatch)
    {
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
                selectedMatch.Player1Sets = tempMatch.Player1Sets;
                selectedMatch.Player2Sets = tempMatch.Player2Sets;
                selectedMatch.Player1Legs = tempMatch.Player1Legs;
                selectedMatch.Player2Legs = tempMatch.Player2Legs;
                selectedMatch.Winner = tempMatch.Winner;
                selectedMatch.Loser = tempMatch.Winner == tempMatch.Player1 ? tempMatch.Player2 : tempMatch.Player1;
                selectedMatch.Status = tempMatch.Status;
                selectedMatch.Notes = tempMatch.Notes;
                selectedMatch.EndTime = DateTime.Now;

                // Update next round matches if needed
                UpdateKnockoutProgression(selectedMatch);
                RefreshKnockoutView();
                OnDataChanged();
            }
        }
    }

    private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedGroup = GroupsListBox.SelectedItem as Group;
        RemoveGroupButton.IsEnabled = SelectedGroup != null;
        // Entferne automatischen Tab-Wechsel - User bleibt auf aktuellem Tab
    }

    private void PlayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedPlayer = PlayersListBox.SelectedItem as Player;
    }

    private void GroupPhaseGroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var newSelectedGroup = GroupPhaseGroupsList.SelectedItem as Group;
            
            // UMFASSENDE Debug-Ausgaben
            System.Diagnostics.Debug.WriteLine($"=== GroupPhaseGroupsList_SelectionChanged START ===");
            System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: newSelectedGroup = {newSelectedGroup?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: currentSelectedGroup = {SelectedGroup?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: TournamentClass = {TournamentClass?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: CurrentPhase = {TournamentClass?.CurrentPhase?.PhaseType}");
            
            // Prevent recursion and duplicate selection changes
            if (newSelectedGroup == SelectedGroup) 
            {
                System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Same group selected, returning");
                return;
            }

            // Check current phase state BEFORE doing anything
            if (TournamentClass?.CurrentPhase != null)
            {
                System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Current phase type = {TournamentClass.CurrentPhase.PhaseType}");
                
                // Check if all groups are complete
                bool allGroupsComplete = CheckAllGroupsComplete();
                System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: All groups complete = {allGroupsComplete}");
                
                if (allGroupsComplete)
                {
                    System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: All groups complete - checking advance conditions");
                    
                    try
                    {
                        bool canAdvance = TournamentClass.CanProceedToNextPhase();
                        System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: CanProceedToNextPhase = {canAdvance}");
                        
                        if (canAdvance && TournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None)
                        {
                            System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Should be able to advance to next phase");
                            System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: PostGroupPhaseMode = {TournamentClass.GameRules.PostGroupPhaseMode}");
                        }
                    }
                    catch (Exception advanceEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: ERROR checking CanProceedToNextPhase: {advanceEx.Message}");
                    }
                }
            }
            
            // WICHTIG: Validierung hinzufügen bevor GetDynamicRoundDisplay aufgerufen werden könnte
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: We are in KO phase - validating");
                
                var qualifiedParticipants = TournamentClass.CurrentPhase.QualifiedPlayers?.Count ?? 0;
                System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: QualifiedPlayers count = {qualifiedParticipants}");
                
                // Validierung mit den neuen KnockoutMatch-Methoden
                string? validationError = KnockoutMatch.ValidateKnockoutPhaseStart(qualifiedParticipants);
                if (validationError != null)
                {
                    System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Validation failed: {validationError}");
                    
                    // Fehlermeldung anzeigen und Wechsel verhindern
                    MessageBox.Show(validationError, "K.O.-Phase Fehler", 
                                   MessageBoxButton.OK, MessageBoxImage.Warning);
                    
                    // Zurück zur Gruppenphase wechseln
                    ResetToGroupPhase();
                    return;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: KO phase validation passed");
                }
            }
            
            // ZUSÄTZLICHER SCHUTZ: Auch in anderen Phasen auf Null-Werte prüfen
            if (newSelectedGroup != null && TournamentClass?.CurrentPhase != null)
            {
                var currentPhase = TournamentClass.CurrentPhase.PhaseType;
                System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Current phase = {currentPhase}");
                
                // Vermeide Zugriff auf K.O.-Daten in anderen Phasen
                if (currentPhase != TournamentPhaseType.GroupPhase && 
                    currentPhase != TournamentPhaseType.RoundRobinFinals)
                {
                    System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Preventing group selection in KO phase ({currentPhase})");
                    return;
                }
            }
            
            // Safely set the selected group
            System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Setting SelectedGroup to {newSelectedGroup?.Name ?? "null"}");
            SelectedGroup = newSelectedGroup;
            System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Successfully set SelectedGroup to {newSelectedGroup?.Name ?? "null"}");
            System.Diagnostics.Debug.WriteLine($"=== GroupPhaseGroupsList_SelectionChanged END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== GroupPhaseGroupsList_SelectionChanged CRITICAL ERROR ===");
            System.Diagnostics.Debug.WriteLine($"Critical error in GroupPhaseGroupsList_SelectionChanged: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Bei kritischem Fehler: Zur Gruppenphase zurückkehren
            try
            {
                ResetToGroupPhase();
                System.Diagnostics.Debug.WriteLine($"GroupPhaseGroupsList_SelectionChanged: Reset to group phase completed");
            }
            catch (Exception resetEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error during reset: {resetEx.Message}");
            }
            
            // Show error but don't crash
            MessageBox.Show($"Unerwarteter Fehler beim Gruppenwechsel: {ex.Message}\n\nDas Turnier wurde zur Gruppenphase zurückgesetzt.", 
                           "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        // Kein automatischer Tab-Wechsel zur KO-Phase
    }
    
    private bool CheckAllGroupsComplete()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== CheckAllGroupsComplete START ===");
            
            if (TournamentClass?.Groups == null || TournamentClass.Groups.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"CheckAllGroupsComplete: No groups found");
                return false;
            }
            
            foreach (var group in TournamentClass.Groups)
            {
                System.Diagnostics.Debug.WriteLine($"CheckAllGroupsComplete: Group '{group.Name}' - MatchesGenerated: {group.MatchesGenerated}, Matches: {group.Matches.Count}");
                
                if (!group.MatchesGenerated || group.Matches.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"CheckAllGroupsComplete: Group '{group.Name}' has no matches generated");
                    return false;
                }
                
                var finishedMatches = group.Matches.Count(m => m.Status == MatchStatus.Finished || m.IsBye);
                var totalMatches = group.Matches.Count;
                
                System.Diagnostics.Debug.WriteLine($"CheckAllGroupsComplete: Group '{group.Name}' - Finished: {finishedMatches}/{totalMatches}");
                
                if (finishedMatches < totalMatches)
                {
                    System.Diagnostics.Debug.WriteLine($"CheckAllGroupsComplete: Group '{group.Name}' is not complete");
                    return false;
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"CheckAllGroupsComplete: All groups are complete");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckAllGroupsComplete: ERROR: {ex.Message}");
            return false;
        }
    }

    private void Match_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            // React to changes in match properties like ScoreDisplay, StatusDisplay, etc.
            if (e.PropertyName == nameof(Match.ScoreDisplay) || 
                e.PropertyName == nameof(Match.StatusDisplay) || 
                e.PropertyName == nameof(Match.Winner) ||
                e.PropertyName == nameof(Match.Status) ||
                e.PropertyName == nameof(Match.Player1Sets) ||
                e.PropertyName == nameof(Match.Player2Sets) ||
                e.PropertyName == nameof(Match.Player1Legs) ||
                e.PropertyName == nameof(Match.Player2Legs))
            {
                // THROTTLING: Verwende Timer, um mehrfache Aufrufe zu begrenzen
                if (_refreshTimer == null)
                {
                    _refreshTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(100) // 100ms Verzögerung
                    };
                    _refreshTimer.Tick += (s, args) =>
                    {
                        _refreshTimer.Stop();
                        _refreshTimer = null;
                        
                        try
                        {
                            RefreshMatchesView();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error refreshing matches view in timer: {ex.Message}");
                        }
                    };
                }
                
                _refreshTimer.Stop();
                _refreshTimer.Start();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Match_PropertyChanged: {ex.Message}");
        }
    }
    
    private DispatcherTimer? _refreshTimer;

    private void Matches_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        try
        {
            // Subscribe to PropertyChanged events for newly added matches
            if (e.NewItems != null)
            {
                foreach (Match match in e.NewItems)
                {
                    match.PropertyChanged -= Match_PropertyChanged; // Prevent double subscription
                    match.PropertyChanged += Match_PropertyChanged;
                }
            }
            
            // Unsubscribe from PropertyChanged events for removed matches
            if (e.OldItems != null)
            {
                foreach (Match match in e.OldItems)
                {
                    match.PropertyChanged -= Match_PropertyChanged;
                }
            }
            
            // Refresh views when collection changes
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    RefreshMatchesView();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error refreshing matches view in collection changed: {ex.Message}");
                }
            }, DispatcherPriority.DataBind);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Matches_CollectionChanged: {ex.Message}");
        }
    }

    private void RefreshMatchesView()
    {
        try
        {
            if (SelectedGroup != null)
            {
                // Debug output
                System.Diagnostics.Debug.WriteLine($"RefreshMatchesView for group: {SelectedGroup.Name}");
                System.Diagnostics.Debug.WriteLine($"  Matches count: {SelectedGroup.Matches.Count}");
                System.Diagnostics.Debug.WriteLine($"  Players count: {SelectedGroup.Players.Count}");
                
                // Check if we're already on UI thread
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    // We're on UI thread - execute directly
                    try
                    {
                        // VEREINFACHT: Nur einmal setzen, nicht null und wieder setzen
                        MatchesDataGrid.ItemsSource = SelectedGroup.Matches;
                        
                        // Calculate fresh standings - with null check
                        if (SelectedGroup.Players.Count > 0)
                        {
                            var standings = SelectedGroup.GetStandings();
                            

                            // Update standings display
                            StandingsDataGrid.ItemsSource = standings;
                        }
                        else
                        {
                            StandingsDataGrid.ItemsSource = null;
                        }
                        
                        // ENTFERNT: Items.Refresh() - kann zu Endlosschleifen führen
                        // MatchesDataGrid.Items.Refresh();
                        // StandingsDataGrid.Items.Refresh();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in RefreshMatchesView direct execution: {ex.Message}");
                        ClearViewsSafely();
                    }
                }
                else
                {
                    // We're on background thread - use Dispatcher
                    Dispatcher.BeginInvoke(() =>
                    {
                        RefreshMatchesView(); // Recursive call on UI thread
                    }, DispatcherPriority.DataBind);
                }
            }
            else
            {
                ClearViewsSafely();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in RefreshMatchesView: {ex.Message}");
            ClearViewsSafely();
        }
    }
    
    private void ClearViewsSafely()
    {
        try
        {
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    MatchesDataGrid.ItemsSource = null;
                    StandingsDataGrid.ItemsSource = null;
                    PlayersListBox.ItemsSource = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error clearing views safely: {ex.Message}");
                }
            }, DispatcherPriority.DataBind);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Critical error in ClearViewsSafely: {ex.Message}");
        }
    }

    private void PlayerNameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddPlayerButton_Click(sender, new RoutedEventArgs());
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
        PlayerNameTextBox.Focus();
        
        // Update button states
        GenerateMatchesButton.IsEnabled = SelectedGroup.Players.Count >= 2;
        
        OnDataChanged();
    }

    private void RemovePlayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null)
        {
            var title = _localizationService?.GetString("NoGroupSelectedTitle") ?? "Keine Gruppe ausgewählt";
            var message = _localizationService?.GetString("SelectGroupFirst") ?? "Bitte wählen Sie zuerst eine Gruppe aus.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (SelectedPlayer != null)
        {
            var title = _localizationService?.GetString("RemovePlayerTitle") ?? "Spieler entfernen";
            var message = _localizationService?.GetString("RemovePlayerConfirm", SelectedPlayer.Name) ?? 
                         $"Möchten Sie den Spieler '{SelectedPlayer.Name}' wirklich entfernen?";

            // Add warning about tournament reset if in advanced phase
            if (TournamentClass.CurrentPhase?.PhaseType != TournamentPhaseType.GroupPhase)
            {
                message += "\n\n" + (_localizationService?.GetString("TournamentResetWarning") ?? "⚠️ WARNUNG: Das Turnier wird auf die Gruppenphase zurückgesetzt!");
            }

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SelectedGroup.Players.Remove(SelectedPlayer);
                
                // Regenerate matches if they were already generated
                if (SelectedGroup.MatchesGenerated)
                {
                    SelectedGroup.GenerateRoundRobinMatches();
                    UpdateMatchesView();
                }
                
                // Reset tournament to group phase if we were in advanced phase
                if (TournamentClass.CurrentPhase?.PhaseType != TournamentPhaseType.GroupPhase)
                {
                    ResetToGroupPhase();
                }
                
                // Update button states
                UpdatePlayersView(); // This will update all button states including reset button
                OnDataChanged();
            }
        }
        else
        {
            var title = _localizationService?.GetString("NoPlayerSelectedTitle") ?? "Kein Spieler ausgewählt";
            var message = _localizationService?.GetString("NoPlayerSelected") ?? "Bitte wählen Sie einen Spieler aus, der entfernt werden soll.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void GenerateMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        // Check if we're in group phase with a selected group
        if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
        {
            if (SelectedGroup == null || SelectedGroup.Players.Count < 2)
            {
                MessageBox.Show("Mindestens 2 Spieler sind erforderlich um Spiele zu generieren.", "Nicht genügend Spieler", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var title = _localizationService?.GetString("GenerateMatches") ?? "Spiele generieren";
            var message = $"Möchten Sie die Spiele für Gruppe '{SelectedGroup.Name}' generieren?\n" +
                         $"Spielmodus: {TournamentClass.GameRules}\n" +
                         $"Anzahl Spiele: {SelectedGroup.Players.Count * (SelectedGroup.Players.Count - 1) / 2}";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SelectedGroup.GenerateRoundRobinMatches();
                UpdateMatchesView();
                UpdatePlayersView(); // Update to enable reset button
                OnDataChanged();
                
                var successMessage = _localizationService?.GetString("MatchesGenerated") ?? 
                                    $"Spiele wurden erfolgreich generiert! Anzahl: {SelectedGroup.Matches.Count}";
                MessageBox.Show(successMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        // Check if we're in finals phase
        else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
            var finalsGroup = TournamentClass.CurrentPhase.FinalsGroup;
            if (finalsGroup == null || finalsGroup.Players.Count < 2)
            {
                MessageBox.Show("Nicht genügend qualifizierte Spieler für die Finalrunde.", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var title = _localizationService?.GetString("GenerateMatches") ?? "Spiele generieren";
            var message = $"Möchten Sie die Spiele für die Finalrunde generieren?\n" +
                         $"Teilnehmer: {finalsGroup.Players.Count}\n" +
                         $"Anzahl Spiele: {finalsGroup.Players.Count * (finalsGroup.Players.Count - 1) / 2}";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                finalsGroup.GenerateRoundRobinMatches();
                UpdateMatchesView();
                OnDataChanged();
                
                var successMessage = $"Finalrunde wurde erfolgreich generiert! Anzahl Spiele: {finalsGroup.Matches.Count}";
                MessageBox.Show(successMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        // Knockout phase matches are automatically generated
        else if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            MessageBox.Show("KO-Spiele wurden bereits automatisch generiert.", "Information", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void ResetMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null || !SelectedGroup.MatchesGenerated)
        {
            return;
        }

        var title = _localizationService?.GetString("ResetMatchesTitle") ?? "Spiele zurücksetzen";
        var message = _localizationService?.GetString("ResetMatchesConfirm", SelectedGroup.Name) ?? 
                     $"Möchten Sie alle Spiele für Gruppe '{SelectedGroup.Name}' wirklich zurücksetzen?\nAlle Ergebnisse gehen verloren!";

        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            // Clear all matches
            SelectedGroup.Matches.Clear();
            SelectedGroup.MatchesGenerated = false;
            
            // Update UI
            UpdateMatchesView();
            UpdatePlayersView(); // Update to disable reset button
            OnDataChanged();
            
            var successMessage = _localizationService?.GetString("MatchesReset") ?? "Spiele wurden zurückgesetzt!";
            MessageBox.Show(successMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void AdvanceToNextPhaseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TournamentClass.CanProceedToNextPhase())
        {
            var cannotAdvanceTitle = _localizationService?.GetString("CannotAdvancePhase") ?? "Kann nicht zur nächsten Phase";
            var cannotAdvanceMessage = _localizationService?.GetString("CannotAdvancePhase") ?? "Alle Spiele der aktuellen Phase müssen beendet sein.";
            MessageBox.Show(cannotAdvanceMessage, cannotAdvanceTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var nextPhase = TournamentClass.GetNextPhase();
        if (nextPhase == null)
        {
            MessageBox.Show("Keine weitere Phase verfügbar.", "Phase", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var qualifiedPlayersCount = TournamentClass.CurrentPhase?.GetQualifiedPlayers(TournamentClass.GameRules.QualifyingPlayersPerGroup).Count ?? 0;

        // Validierung hinzufügen für K.O.-Phase
        if (nextPhase.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            string? validationError = KnockoutMatch.ValidateKnockoutPhaseStart(qualifiedPlayersCount);
            if (validationError != null)
            {
                MessageBox.Show(validationError, "K.O.-Phase nicht möglich", 
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        var nextPhaseText = nextPhase.PhaseType switch
        {
            TournamentPhaseType.RoundRobinFinals => _localizationService?.GetString("FinalsPhase") ?? "Finalrunde",
            TournamentPhaseType.KnockoutPhase => _localizationService?.GetString("KnockoutPhase") ?? "KO-Phase",
            _ => "Nächste Phase"
        };

        var advanceTitle = _localizationService?.GetString("AdvanceToNextPhase") ?? "Nächste Phase starten";
        
        var advanceMessage = $"Möchten Sie zur {nextPhaseText} wechseln?\n\n" +
                           $"Qualifizierte Spieler: {qualifiedPlayersCount}\n" +
                           $"Modus: {TournamentClass.GameRules.PostGroupPhaseMode}";

        if (TournamentClass.GameRules.PostGroupPhaseMode == PostGroupPhaseMode.KnockoutBracket)
        {
            advanceMessage += $"\nKO-System: {TournamentClass.GameRules.KnockoutMode}";
        }

        var result = MessageBox.Show(advanceMessage, advanceTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                TournamentClass.AdvanceToNextPhase();
                
                // Update UI to show new phase
                UpdatePhaseDisplay();
                UpdateMatchesView();
                OnDataChanged();
                
                // Show new phase info
                var successTitle = _localizationService?.GetString("PhaseCompleted") ?? "Phase abgeschlossen";
                var successMessage = $"{nextPhaseText} wurde erfolgreich gestartet!\n" +
                                   $"Qualifizierte Spieler: {qualifiedPlayersCount}";

                if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
                {
                    successMessage += "\n\nGenerieren Sie jetzt die Spiele für die Finalrunde.";
                }

                MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch ( Exception ex)
            {
                MessageBox.Show($"Fehler beim Wechsel zur nächsten Phase: {ex.Message}", "Fehler", 
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ResetKnockoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (TournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return;
        }

        var title = _localizationService?.GetString("ResetKnockoutTitle") ?? "KO-Phase zurücksetzen";
        var message = _localizationService?.GetString("ResetKnockoutConfirm") ?? 
                     "Möchten Sie die KO-Phase wirklich zurücksetzen?\n\n⚠️ Alle KO-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird zur Gruppenphase zurückgesetzt.";

        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
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
                
                // Update UI
                UpdatePhaseDisplay();
                UpdateMatchesView();
                UpdatePlayersView();
                
                // Switch to group phase tab
                MainTabControl.SelectedItem = GroupPhaseTabItem;
                
                // Notify about changes
                OnDataChanged();
                
                // Show success message
                var successMessage = _localizationService?.GetString("ResetKnockoutComplete") ?? "KO-Phase wurde erfolgreich zurückgesetzt.";
                MessageBox.Show(successMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Zurücksetzen der KO-Phase: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OnDataChanged()
    {
        // Notify parent window that data has changed
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateMatchesView()
    {
        try
        {
            if (SelectedGroup != null)
            {
                // Use Dispatcher for thread safety
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        MatchesDataGrid.ItemsSource = SelectedGroup.Matches;
                        
                        // Update standings with null check
                        if (SelectedGroup.Players.Count > 0)
                        {
                            var standings = SelectedGroup.GetStandings();
                            StandingsDataGrid.ItemsSource = standings;
                        }
                        else
                        {
                            StandingsDataGrid.ItemsSource = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in UpdateMatchesView inner block: {ex.Message}");
                        ClearViewsSafely();
                    }
                }, DispatcherPriority.DataBind);
            }
            else
            {
                ClearViewsSafely();
            }

            // Update other views based on current phase
            RefreshFinalsView();
            RefreshKnockoutView();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in UpdateMatchesView: {ex.Message}");
            ClearViewsSafely();
        }
    }

    public event EventHandler? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// ViewModel for displaying knockout matches with dynamic round names
/// </summary>
public class KnockoutMatchViewModel
{
    private readonly KnockoutMatch _match;
    private readonly int _totalParticipants;
    private readonly LocalizationService? _localizationService;

    public KnockoutMatchViewModel(KnockoutMatch match, int totalParticipants, LocalizationService? localizationService)
    {
        _match = match;
        _totalParticipants = totalParticipants;
        _localizationService = localizationService;
    }

    public KnockoutMatch Match => _match;
    public string DisplayName => _match.DisplayName;
    public string ScoreDisplay => _match.ScoreDisplay;
    public string StatusDisplay => _match.StatusDisplay;
    
    public string RoundDisplay 
    { 
        get
        {
            // Zusätzliche Validierung im ViewModel
            if (!KnockoutMatch.CanStartKnockoutPhase(_totalParticipants))
            {
                return "Fehler: Ungültige Teilnehmeranzahl";
            }
            
            return _match.GetDynamicRoundDisplay(_totalParticipants, _localizationService);
        }
    }
}