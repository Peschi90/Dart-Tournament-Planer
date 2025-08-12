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
            // WICHTIG: Verhindere doppelte Zuweisungen um UI-Duplikate zu vermeiden
            if (_tournamentClass == value)
            {
                System.Diagnostics.Debug.WriteLine($"TournamentClass setter: Same object reference, skipping UI update");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== TournamentClass setter START ===");
            System.Diagnostics.Debug.WriteLine($"TournamentClass setter: Changing from {_tournamentClass?.Name ?? "null"} to {value?.Name ?? "null"}");
            
            // Unsubscribe from old tournament class events
            if (_tournamentClass != null)
            {
                _tournamentClass.UIRefreshRequested -= OnTournamentUIRefreshRequested;
                System.Diagnostics.Debug.WriteLine($"TournamentClass setter: Unsubscribed from UIRefreshRequested for {_tournamentClass.Name}");
            }

            _tournamentClass = value;
            
            // Subscribe to new tournament class events
            if (_tournamentClass != null)
            {
                _tournamentClass.UIRefreshRequested += OnTournamentUIRefreshRequested;
                System.Diagnostics.Debug.WriteLine($"TournamentClass setter: Subscribed to UIRefreshRequested for {_tournamentClass.Name}");
            }

            OnPropertyChanged();
            
            // WICHTIG: UI-Update nur einmal, unabhängig von der Phase
            System.Diagnostics.Debug.WriteLine($"TournamentClass setter: Calling UpdateUI for {_tournamentClass?.Name}");
            UpdateUI();
            
            // Phase-spezifische Updates nur wenn nötig und ohne mehrfachen UpdateUI-Aufruf
            if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                // Use Dispatcher to ensure UI is ready for knockout-specific refreshes
                Dispatcher.BeginInvoke(() =>
                {
                    RefreshKnockoutView();
                    // UpdateMatchesView() wird bereits in UpdateUI() aufgerufen, nicht nochmal hier!
                }, DispatcherPriority.Loaded);
            }
            
            System.Diagnostics.Debug.WriteLine($"=== TournamentClass setter END ===");
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
                    
                    // WICHTIG: Stelle sicher, dass alle Match-Events korrekt funktionieren
                    // Dies ist besonders wichtig nach dem Laden von JSON-Daten
                    _selectedGroup.EnsureMatchEventSubscriptions();
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
        
        // NEW: Add Refresh Button
        if (RefreshUIButton != null)
        {
            RefreshUIButton.Content = "🔄 " + _localizationService.GetString("RefreshUI");
            RefreshUIButton.ToolTip = _localizationService.GetString("RefreshUITooltip");
        }
        
        // Update DataGrid columns
        UpdateDataGridHeaders();
        
        // WICHTIG: Nur UpdatePlayersView aufrufen - das ruft am Ende UpdatePhaseDisplay auf
        // Dadurch vermeiden wir doppelte UpdatePhaseDisplay-Aufrufe
        UpdatePlayersView(); // This will update the players header text AND call UpdatePhaseDisplay()
        
        // ENTFERNT: UpdatePhaseDisplay() wird bereits in UpdatePlayersView() aufgerufen
        
        // Ensure knockout phase is properly loaded if we're in that phase
        // OHNE zusätzliche UI-Updates um Duplikate zu vermeiden
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
        System.Diagnostics.Debug.WriteLine($"=== UpdateUI START ===");
        System.Diagnostics.Debug.WriteLine($"UpdateUI: Starting for TournamentClass {TournamentClass?.Name}");
        System.Diagnostics.Debug.WriteLine($"UpdateUI: Groups count = {TournamentClass?.Groups?.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"UpdateUI: Current phase = {TournamentClass?.CurrentPhase?.PhaseType}");
        
        if (TournamentClass?.Groups != null)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateUI: Groups in TournamentClass.Groups:");
            foreach (var group in TournamentClass.Groups)
            {
                System.Diagnostics.Debug.WriteLine($"  Group: {group.Name} (ID: {group.Id})");
            }
        }
        
        // Debug: Check current ItemsSource status
        System.Diagnostics.Debug.WriteLine($"UpdateUI: GroupsListBox.ItemsSource == TournamentClass.Groups: {GroupsListBox.ItemsSource == TournamentClass.Groups}");
        System.Diagnostics.Debug.WriteLine($"UpdateUI: GroupPhaseGroupsList.ItemsSource == TournamentClass.Groups: {GroupPhaseGroupsList.ItemsSource == TournamentClass.Groups}");
        
        // Debug: Check what's currently in the ListBox
        if (GroupsListBox.ItemsSource != null)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateUI: Current GroupsListBox.ItemsSource contains:");
            foreach (var item in GroupsListBox.ItemsSource)
            {
                if (item is Group g)
                {
                    System.Diagnostics.Debug.WriteLine($"  Current Group: {g.Name} (ID: {g.Id})");
                }
            }
        }
        
        // Prevent duplicate items by only setting if different
        if (GroupsListBox.ItemsSource != TournamentClass.Groups)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateUI: Setting GroupsListBox.ItemsSource to Groups collection with {TournamentClass.Groups.Count} items");
            GroupsListBox.ItemsSource = TournamentClass.Groups;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"UpdateUI: GroupsListBox.ItemsSource already set correctly - skipping");
        }
        
        if (GroupPhaseGroupsList.ItemsSource != TournamentClass.Groups)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateUI: Setting GroupPhaseGroupsList.ItemsSource to Groups collection with {TournamentClass.Groups.Count} items");
            GroupPhaseGroupsList.ItemsSource = TournamentClass.Groups;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"UpdateUI: GroupPhaseGroupsList.ItemsSource already set correctly - skipping");
        }
        
        // WICHTIG: Button-Status basierend auf aktueller Selektion aktualisieren
        RemoveGroupButton.IsEnabled = GroupsListBox.SelectedItem != null;
        
        UpdateNextIds();
        UpdatePhaseDisplay();
        
        System.Diagnostics.Debug.WriteLine($"UpdateUI: Completed for TournamentClass {TournamentClass?.Name}");
        System.Diagnostics.Debug.WriteLine($"=== UpdateUI END ===");
    }

    private void UpdateNextIds()
    {
        System.Diagnostics.Debug.WriteLine($"=== UpdateNextIds START ===");
        System.Diagnostics.Debug.WriteLine($"UpdateNextIds: TournamentClass.Groups.Count = {TournamentClass.Groups.Count}");
        
        if (TournamentClass.Groups != null && TournamentClass.Groups.Any())
        {
            foreach (var group in TournamentClass.Groups)
            {
                System.Diagnostics.Debug.WriteLine($"  Existing Group: ID={group.Id}, Name='{group.Name}'");
            }
        }
        
        // Update next group ID
        if (TournamentClass.Groups.Count > 0)
        {
            var maxId = TournamentClass.Groups.Max(g => g.Id);
            _nextGroupId = maxId + 1;
            System.Diagnostics.Debug.WriteLine($"UpdateNextIds: Max existing Group ID = {maxId}, _nextGroupId set to {_nextGroupId}");
        }
        else
        {
            _nextGroupId = 1;
            System.Diagnostics.Debug.WriteLine($"UpdateNextIds: No groups exist, _nextGroupId set to {_nextGroupId}");
        }

        // Update next player ID
        var allPlayers = TournamentClass.Groups.SelectMany(g => g.Players);
        if (allPlayers.Any())
        {
            var maxPlayerId = allPlayers.Max(p => p.Id);
            _nextPlayerId = maxPlayerId + 1;
            System.Diagnostics.Debug.WriteLine($"UpdateNextIds: Max existing Player ID = {maxPlayerId}, _nextPlayerId set to {_nextPlayerId}");
        }
        else
        {
            _nextPlayerId = 1;
            System.Diagnostics.Debug.WriteLine($"UpdateNextIds: No players exist, _nextPlayerId set to {_nextPlayerId}");
        }
        
        System.Diagnostics.Debug.WriteLine($"=== UpdateNextIds END ===");
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
            
            // WICHTIG: UpdatePhaseDisplay nur am Ende aufrufen, um doppelte Aufrufe zu vermeiden
            // Wird sowohl von UpdateTranslations als auch anderen Methoden aufgerufen
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
            LoserBracketTreeTab.Visibility = hasKnockout && hasDoubleElimination ? Visibility.Visible :Visibility.Collapsed;

            // Check if we can advance to next phase
            bool canAdvance = false;
            try
            {
                canAdvance = TournamentClass.CanProceedToNextPhase() && 
                           TournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None;
                System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: CanProceedToNextPhase = {TournamentClass.CanProceedToNextPhase()}");
                System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: PostGroupPhaseMode = {TournamentClass.GameRules.PostGroupPhaseMode}");
                System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: canAdvance = {canAdvance}");
                
                // ERWEITERTE DEBUG-INFO: Detaillierte Gruppenstatusüberprüfung
                if (TournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: Detailed group status check:");
                    foreach (var group in TournamentClass.Groups)
                    {
                        var status = group.CheckCompletionStatus();
                        System.Diagnostics.Debug.WriteLine($"  - Group '{group.Name}': {status}");
                    }
                }
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
        
        // Calculate subsequent rounds: each match is positioned exactly in the middle of its two source matches
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
    /// where each match is positioned exactly in the middle between its two source matches
    /// </summary>
    /// <param name="matchesByRound">Matches grouped by round</param>
    /// <param name="canvasHeight">Total canvas height</param>
    /// <param name="baseSpacing">Base spacing for the first round</param>
    /// <returns>Dictionary of round positions</returns>
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
        
        // WICHTIG: VERBESSERTE SPACING-LOGIK: Mehr Platz zwischen Matches
        double adjustedSpacing = Math.Max(80, 100); // Mindestens 100px Abstand
        
        // If there are many matches, reduce spacing but keep minimum
        if (firstRound.Count > 8)
        {
            adjustedSpacing = Math.Max(80, (canvasHeight - 200) / firstRound.Count);
        }
        
        System.Diagnostics.Debug.WriteLine($"First round: {firstRound.Count} matches, starting at Y={startY}, spacing: {adjustedSpacing}px");
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

        try
        {
            var gameRulesWindow = new Views.GameRulesWindow(TournamentClass.GameRules, _localizationService);
            gameRulesWindow.Owner = Window.GetWindow(this);
            
            // Subscribe to data changes from the GameRulesWindow
            gameRulesWindow.DataChanged += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine("ConfigureRulesButton_Click: GameRulesWindow DataChanged received");
                
                // WICHTIG: Update existing matches when rules change
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
        System.Diagnostics.Debug.WriteLine($"=== AddGroupButton_Click START ===");
        System.Diagnostics.Debug.WriteLine($"AddGroupButton_Click: Current _nextGroupId = {_nextGroupId}");

        var defaultName = _localizationService?.GetString("Group", _nextGroupId) ?? $"Gruppe {_nextGroupId}";
        var title = _localizationService?.GetString("NewGroup") ?? "Neue Gruppe";
        var prompt = _localizationService?.GetString("GroupName") ?? "Geben Sie den Namen der neuen Gruppe ein:";

        System.Diagnostics.Debug.WriteLine($"AddGroupButton_Click: Default name = '{defaultName}'");

        // Simple dialog alternative
        string? groupName = ShowInputDialog(prompt, title, defaultName);
        
        System.Diagnostics.Debug.WriteLine($"AddGroupButton_Click: User entered name = '{groupName ?? "null"}'");
        
        if (!string.IsNullOrWhiteSpace(groupName))
        {
            // SICHERHEITSCHECK: Stelle sicher, dass die ID wirklich einzigartig ist
            while (TournamentClass.Groups.Any(g => g.Id == _nextGroupId))
            {
                System.Diagnostics.Debug.WriteLine($"AddGroupButton_Click: WARNING! ID {_nextGroupId} already exists, incrementing...");
                _nextGroupId++;
            }
            
            var group = new Group { Id = _nextGroupId, Name = groupName.Trim() };
            System.Diagnostics.Debug.WriteLine($"AddGroupButton_Click: Creating group with ID={group.Id}, Name='{group.Name}'");
            
            TournamentClass.Groups.Add(group);
            System.Diagnostics.Debug.WriteLine($"AddGroupButton_Click: Added group to collection. Total groups now: {TournamentClass.Groups.Count}");
            
            // WICHTIG: Increment AFTER adding the group
            _nextGroupId++;
            System.Diagnostics.Debug.WriteLine($"AddGroupButton_Click: Incremented _nextGroupId to {_nextGroupId} for next group");
            
            // Subscribe to player changes in the new group
            group.Players.CollectionChanged += (s, e) => OnDataChanged();
            group.Matches.CollectionChanged += (s, e) => OnDataChanged();
            
            // Trigger data changed
            OnDataChanged();
        }
        
        System.Diagnostics.Debug.WriteLine($"=== AddGroupButton_Click END ===");
    }

    private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"RemoveGroupButton_Click: SelectedItem = {GroupsListBox.SelectedItem}");
        
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
                
                // Reset tournament to group phase
                ResetToGroupPhase();
                
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"RemoveGroupButton_Click: No group selected");
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
        {
            System.Diagnostics.Debug.WriteLine($"UpdateKnockoutProgression: Skipping - invalid conditions");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"=== UpdateKnockoutProgression START for match {completedMatch.Id} ===");
        System.Diagnostics.Debug.WriteLine($"  Winner: {completedMatch.Winner.Name}");
        System.Diagnostics.Debug.WriteLine($"  Status: {completedMatch.Status}");
        System.Diagnostics.Debug.WriteLine($"  BracketType: {completedMatch.BracketType}");

        // Use the new ProcessMatchResult method which handles progression correctly
        bool success = TournamentClass.ProcessMatchResult(completedMatch);
        
        if (success)
        {
            System.Diagnostics.Debug.WriteLine($"  Match result processed successfully - progression completed");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"  Failed to process match result - no progression occurred");
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
            // WICHTIG: Übergebe GameRules an GenerateRoundRobinMatches
            SelectedGroup.GenerateRoundRobinMatches(TournamentClass.GameRules);
            
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
    /// Handles groups list box selection changes
    /// </summary>
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
                // WICHTIG: Stelle sicher, dass das Match seine UI-Properties aktualisiert
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.ScoreDisplay));
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.StatusDisplay));
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.WinnerDisplay));
                
                UpdateMatchesView();
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

    /// <summary>
    /// Handles loser bracket data grid mouse double click
    /// </summary>
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

    /// <summary>
    /// Handles giving a bye to a player
    /// </summary>
    private void GiveByeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is KnockoutMatchViewModel viewModel)
        {
            var match = viewModel.Match;
            
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

    /// <summary>
    /// Handles tab control selection changes
    /// </summary>
    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] == KnockoutTabItem)
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                var qualifiedParticipants = TournamentClass.CurrentPhase.QualifiedPlayers?.Count ?? 0;
                
                if (qualifiedParticipants < 2)
                {
                    MessageBox.Show("Nicht genügend Teilnehmer für K.O.-Phase (mindestens 2 erforderlich)", 
                                   "K.O.-Phase Warnung", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    MainTabControl.SelectedItem = GroupPhaseTabItem;
                    return;
                }
                
                Dispatcher.BeginInvoke(() => RefreshKnockoutView(), DispatcherPriority.Loaded);
            }
            else
            {
                MessageBox.Show("K.O.-Phase ist noch nicht aktiv.", "Information", 
                               MessageBoxButton.OK, MessageBoxImage.Information);
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

    /// <summary>
    /// Handles tournament reset button click
    /// </summary>
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
                MessageBox.Show(successMessage, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Zurücksetzen des Turniers: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            UpdatePhaseDisplay();
        }
    }

    /// <summary>
    /// Simple input dialog alternative
    /// </summary>
    private string? ShowInputDialog(string prompt, string title, string defaultValue = "")
    {
        return string.IsNullOrWhiteSpace(defaultValue) ? null : defaultValue.Trim();
    }

    /// <summary>
    /// Event handler for match property changes
    /// </summary>
    private void Match_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Match.Status) || e.PropertyName == nameof(Match.Winner))
        {
            UpdateMatchesView();
        }
    }

    /// <summary>
    /// Event handler for matches collection changes
    /// </summary>
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

    /// <summary>
    /// Updates the matches view
    /// </summary>
    private void UpdateMatchesView()
    {
        try
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                if (SelectedGroup != null)
                {
                    // WICHTIG: Stelle sicher, dass alle Match-Events funktionieren
                    SelectedGroup.EnsureMatchEventSubscriptions();
                    
                    MatchesDataGrid.ItemsSource = null; // Clear first
                    MatchesDataGrid.ItemsSource = SelectedGroup.Matches; // Then reassign
                    
                    if (SelectedGroup.Players.Count > 0)
                    {
                        var standings = SelectedGroup.GetStandings();
                        StandingsDataGrid.ItemsSource = null; // Clear first
                        StandingsDataGrid.ItemsSource = standings; // Then reassign
                    }
                    else
                    {
                        StandingsDataGrid.ItemsSource = null;
                    }
                }
                else
                {
                    MatchesDataGrid.ItemsSource = null;
                    StandingsDataGrid.ItemsSource = null;
                }
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                var finalsGroup = TournamentClass.CurrentPhase.FinalsGroup;
                if (finalsGroup != null)
                {
                    finalsGroup.EnsureMatchEventSubscriptions();
                    FinalsMatchesDataGrid.ItemsSource = null;
                    FinalsMatchesDataGrid.ItemsSource = finalsGroup.Matches;
                    FinalsStandingsDataGrid.ItemsSource = null;
                    FinalsStandingsDataGrid.ItemsSource = finalsGroup.GetStandings();
                }
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

    /// <summary>
    /// Safely clears all views
    /// </summary>
    private void ClearViewsSafely()
    {
        try
        {
            Dispatcher.BeginInvoke(() =>
            {
                PlayersListBox.ItemsSource = null;
                MatchesDataGrid.ItemsSource = null;
                StandingsDataGrid.ItemsSource = null;
                PlayersHeaderText.Text = _localizationService?.GetString("NoGroupSelectedPlayers") ?? "Spieler: (Keine Gruppe ausgewählt)";
            }, DispatcherPriority.DataBind);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClearViewsSafely: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Opens match result window for knockout matches
    /// </summary>
    private void OpenMatchResultWindow(KnockoutMatch match)
    {
        if (_localizationService == null) return;

        // WICHTIG: Verwende rundenspezifische Regeln für KO-Matches
        var roundRules = TournamentClass.GameRules.GetRulesForRound(match.Round);
        
        System.Diagnostics.Debug.WriteLine($"OpenMatchResultWindow: Opening KO match {match.Id}");
        System.Diagnostics.Debug.WriteLine($"  Round: {match.Round}");
        System.Diagnostics.Debug.WriteLine($"  Round rules - SetsToWin: {roundRules.SetsToWin}, LegsToWin: {roundRules.LegsToWin}, LegsPerSet: {roundRules.LegsPerSet}");

        // Verwende den speziellen Konstruktor für KnockoutMatch mit rundenspezifischen Regeln
        var resultWindow = new MatchResultWindow(match, roundRules, TournamentClass.GameRules, _localizationService);
        resultWindow.Owner = Window.GetWindow(this);

        if (resultWindow.ShowDialog() == true)
        {
            // Kopiere die Ergebnisse zurück zum KnockoutMatch
            var resultMatch = resultWindow.InternalMatch;
            match.Player1Sets = resultMatch.Player1Sets;
            match.Player2Sets = resultMatch.Player2Sets;
            match.Player1Legs = resultMatch.Player1Legs;
            match.Player2Legs = resultMatch.Player2Legs;
            match.Winner = resultMatch.Winner;
            match.Loser = resultMatch.Winner == resultMatch.Player1 ? resultMatch.Player2 : resultMatch.Player1;
            match.Status = resultMatch.Status;
            match.Notes = resultMatch.Notes;
            match.EndTime = DateTime.Now;
            
            // WICHTIG: Setze UsesSets basierend auf den rundenspezifischen Regeln
            match.UsesSets = roundRules.SetsToWin > 0;

            System.Diagnostics.Debug.WriteLine($"OpenMatchResultWindow: Result saved - UsesSets: {match.UsesSets}");
            System.Diagnostics.Debug.WriteLine($"  Final score: {match.Player1Sets}:{match.Player2Sets} sets, {match.Player1Legs}:{match.Player2Legs} legs");
            System.Diagnostics.Debug.WriteLine($"  Winner: {match.Winner?.Name ?? "none"}");

            UpdateKnockoutProgression(match);
            RefreshKnockoutView();
            OnDataChanged();
        }
    }

    /// <summary>
    /// Creates and shows context menu for knockout matches
    /// </summary>
    private void CreateAndShowKnockoutMatchContextMenu(KnockoutMatch match, FrameworkElement element, MouseButtonEventArgs e)
    {
        var contextMenu = new ContextMenu();

        bool hasPlayer1 = match.Player1 != null;
        bool hasPlayer2 = match.Player2 != null;
        bool isFinished = match.Status == MatchStatus.Finished;
        bool isBye = match.Status == MatchStatus.Bye;

        if (isFinished)
        {
            var editMenuItem = new MenuItem
            {
                Header = _localizationService?.GetString("EditResult") ?? "Ergebnis bearbeiten"
            };
            editMenuItem.Click += (s, args) => OpenMatchResultWindow(match);
            contextMenu.Items.Add(editMenuItem);
        }
        else if (hasPlayer1 && hasPlayer2 && !isBye)
        {
            var enterResultMenuItem = new MenuItem
            {
                Header = _localizationService?.GetString("EnterResult") ?? "Ergebnis eingeben"
            };
            enterResultMenuItem.Click += (s, args) => OpenMatchResultWindow(match);
            contextMenu.Items.Add(enterResultMenuItem);

            contextMenu.Items.Add(new Separator());

            var giveByeMenuItem = new MenuItem
            {
                Header = _localizationService?.GetString("SelectByeWinner") ?? "Freilos-Gewinner wählen"
            };
            giveByeMenuItem.Click += (s, args) =>
            {
                var byeDialog = new ByeSelectionDialog(match, _localizationService);
                byeDialog.Owner = Window.GetWindow(this);
                if (byeDialog.ShowDialog() == true)
                {
                    bool success = TournamentClass.GiveManualBye(match, byeDialog.SelectedPlayer);
                    if (success)
                    {
                        RefreshKnockoutView();
                        OnDataChanged();
                    }
                }
            };
            contextMenu.Items.Add(giveByeMenuItem);
        }
        else if (isBye)
        {
            var undoByeMenuItem = new MenuItem
            {
                Header = _localizationService?.GetString("RemoveBye") ?? "Freilos entfernen"
            };
            undoByeMenuItem.Click += (s, args) =>
            {
                bool success = TournamentClass.UndoBye(match);
                if (success)
                {
                    RefreshKnockoutView();
                    OnDataChanged();
                }
            };
            contextMenu.Items.Add(undoByeMenuItem);
        }
        else if ((hasPlayer1 && !hasPlayer2) || (!hasPlayer1 && hasPlayer2))
        {
            var player = hasPlayer1 ? match.Player1 : match.Player2;
            var giveAutoByeMenuItem = new MenuItem
            {
                Header = _localizationService?.GetString("GiveAutoByeToPlayer", player?.Name ?? "Player") ?? $"Automatisches Freilos an {player?.Name}"
            };
            giveAutoByeMenuItem.Click += (s, args) =>
            {
                bool success = TournamentClass.GiveManualBye(match);
                if (success)
                {
                    RefreshKnockoutView();
                    OnDataChanged();
                }
            };
            contextMenu.Items.Add(giveAutoByeMenuItem);
        }
        else
        {
            var noActionMenuItem = new MenuItem
            {
                Header = _localizationService?.GetString("NoActionPossibleBothTBD") ?? "Keine Aktion möglich (beide Spieler TBD)",
                IsEnabled = false
            };
            contextMenu.Items.Add(noActionMenuItem);
        }

        if (contextMenu.Items.Count > 0)
        {
            contextMenu.PlacementTarget = element;
            contextMenu.Placement = PlacementMode.Mouse;
            contextMenu.IsOpen = true;
        }

        e.Handled = true;
    }

    /// <summary>
    /// Handles the reset knockout phase button click
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
        }
    }

    /// <summary>
    /// NEUE METHODE: Handhabt den Refresh UI Button-Klick
    /// </summary>
    private void RefreshUIButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== RefreshUIButton_Click START ===");
            
            // Detaillierte Gruppenstatusüberprüfung und Reparatur
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshUI: Checking and repairing group phase status");
                
                foreach (var group in TournamentClass.Groups)
                {
                    System.Diagnostics.Debug.WriteLine($"RefreshUI: Processing group '{group.Name}'");
                    
                    // 1. Status vor Reparatur
                    var statusBefore = group.CheckCompletionStatus();
                    System.Diagnostics.Debug.WriteLine($"  Status before repair: {statusBefore}");
                    
                    // 2. Versuche automatische Reparatur
                    group.RepairMatchStatuses();
                    
                    // 3. Status nach Reparatur
                    var statusAfter = group.CheckCompletionStatus();
                    System.Diagnostics.Debug.WriteLine($"  Status after repair: {statusAfter}");
                    
                    // 4. Stelle sicher, dass alle Match-Events funktionieren
                    group.EnsureMatchEventSubscriptions();
                }
            }
            
            // Vollständiges UI-Update
            System.Diagnostics.Debug.WriteLine($"RefreshUI: Performing complete UI update");
            UpdateUI();
            UpdatePlayersView();
            UpdateMatchesView();
            UpdatePhaseDisplay();
            
            // Stelle sicher, dass die Phasenlogik korrekt funktioniert
            try
            {
                bool canAdvance = TournamentClass?.CanProceedToNextPhase() ?? false;
                System.Diagnostics.Debug.WriteLine($"RefreshUI: CanProceedToNextPhase = {canAdvance}");
                
                // Update des Advance-Buttons
                if (AdvanceToNextPhaseButton != null)
                {
                    var hasPostPhase = TournamentClass?.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None;
                    AdvanceToNextPhaseButton.IsEnabled = canAdvance && hasPostPhase;
                    AdvanceToNextPhaseButton.Visibility = hasPostPhase ? Visibility.Visible : Visibility.Collapsed;
                    
                    System.Diagnostics.Debug.WriteLine($"RefreshUI: AdvanceButton enabled = {AdvanceToNextPhaseButton.IsEnabled}, visible = {AdvanceToNextPhaseButton.Visibility}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshUI: Error checking advance status: {ex.Message}");
            }
            
            // Bestätigungsnachricht mit Details
            var message = "";
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                var groupStatuses = TournamentClass.Groups.Select(g => g.CheckCompletionStatus()).ToList();
                var completeGroups = groupStatuses.Count(s => s.IsComplete);
                var totalGroups = groupStatuses.Count;
                
                message = $"UI aktualisiert!\n\nGruppen-Status:\n";
                message += $"- {completeGroups}/{totalGroups} Gruppen abgeschlossen\n";
                
                foreach (var group in TournamentClass.Groups)
                {
                    var status = group.CheckCompletionStatus();
                    message += $"- {group.Name}: {status.FinishedMatches}/{status.TotalMatches} Spiele\n";
                }
                
                var canAdvanceNow = TournamentClass.CanProceedToNextPhase();
                message += $"\nNächste Phase verfügbar: {(canAdvanceNow ? "JA" : "NEIN")}";
            }
            else
            {
                message = _localizationService?.GetString("UIRefreshed") ?? "Benutzeroberfläche wurde aktualisiert";
            }
            
            MessageBox.Show(message, "Refresh", MessageBoxButton.OK, MessageBoxImage.Information);
            
            System.Diagnostics.Debug.WriteLine($"=== RefreshUIButton_Click END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshUIButton_Click: ERROR: {ex.Message}");
            MessageBox.Show($"Fehler beim Aktualisieren: {ex.Message}", "Fehler", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public event EventHandler? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}