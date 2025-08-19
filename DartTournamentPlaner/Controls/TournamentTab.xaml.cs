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
using DartTournamentPlaner.Helpers; // NEU: Helper-Klassen

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
                
                // KRITISCHER FIX: Validiere und repariere Phasen nach dem Laden
                System.Diagnostics.Debug.WriteLine($"TournamentClass.set: Validating and repairing phases for {_tournamentClass.Name}");
                _tournamentClass.ValidateAndRepairPhases();
            }

            OnPropertyChanged();
            UpdateUI();
            
            // WICHTIG: Spezifische View-Updates je nach aktueller Phase
            if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    RefreshKnockoutView();
                }, DispatcherPriority.Loaded);
            }
            else if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine($"TournamentClass.set: Tournament is in Finals phase, scheduling Finals view refresh");
                Dispatcher.BeginInvoke(() =>
                {
                    RefreshFinalsView();
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
                
                // WICHTIG: Validiere Finals-Phase Integrität vor UI-Update
                TournamentClass.EnsureFinalsPhaseIntegrity();
                
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
                else
                {
                    System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Finals phase but no FinalsGroup found after validation!");
                    
                    // Fallback: Zeige QualifiedPlayers wenn verfügbar
                    var qualifiedPlayers = TournamentClass.CurrentPhase.QualifiedPlayers;
                    if (qualifiedPlayers?.Count > 0)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            PlayersListBox.ItemsSource = qualifiedPlayers;
                            PlayersHeaderText.Text = _localizationService?.GetString("FinalistsCount", qualifiedPlayers.Count) ?? $"Finalisten ({qualifiedPlayers.Count} Spieler):";
                            
                            PlayerNameTextBox.IsEnabled = false;
                            AddPlayerButton.IsEnabled = false;
                            GenerateMatchesButton.IsEnabled = false;
                            ResetMatchesButton.IsEnabled = false;
                        }, DispatcherPriority.DataBind);
                    }
                }
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Knockout phase");
                
                // SICHERHEITSCHECK: Prüfe ob QualifiedPlayers verfügbar sind
                var qualifiedPlayers = TournamentClass.CurrentPhase.QualifiedPlayers;
                if (qualifiedPlayers == null)
                {
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
                        
                        }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView for knockout: {ex.Message}");
                    }
                }, DispatcherPriority.DataBind);
            }
            else
            {
                // NEU: FALLBACK für unbekannte oder resetzte Phasen - explizit leeren
                System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: Unknown or reset phase - clearing all views");
                
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        // Alle Player-Listen leeren
                        PlayersListBox.ItemsSource = null;
                        KnockoutParticipantsListBox.ItemsSource = null;
                        FinalistsListBox.ItemsSource = null;
                        
                        PlayersHeaderText.Text = _localizationService?.GetString("NoGroupSelectedPlayers") ?? "Spieler: (Keine Gruppe ausgewählt)";
                        
                        // Player management aktivieren (für Gruppenphase)
                        PlayerNameTextBox.IsEnabled = false; // Aktiviert sich wenn Gruppe gewählt wird
                        AddPlayerButton.IsEnabled = false;
                        GenerateMatchesButton.IsEnabled = false;
                        ResetMatchesButton.IsEnabled = false;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in UpdatePlayersView fallback: {ex.Message}");
                    }
                }, DispatcherPriority.DataBind);
            }
            
            // Update phase info whenever players view changes
            UpdatePhaseDisplay();
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
            System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: START - Current phase: {TournamentClass?.CurrentPhase?.PhaseType}");
            
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: GroupPhase");
                
                // Knockout/Finals DataGrids explizit leeren
                KnockoutMatchesDataGrid.ItemsSource = null;
                LoserBracketDataGrid.ItemsSource = null;
                KnockoutParticipantsListBox.ItemsSource = null;
                FinalsMatchesDataGrid.ItemsSource = null;
                FinalsStandingsDataGrid.ItemsSource = null;
                FinalistsListBox.ItemsSource = null;
                
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
                System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: RoundRobinFinals");
                
                // Group DataGrids explizit leeren
                MatchesDataGrid.ItemsSource = null;
                StandingsDataGrid.ItemsSource = null;
                // Knockout DataGrids explizit leeren
                KnockoutMatchesDataGrid.ItemsSource = null;
                LoserBracketDataGrid.ItemsSource = null;
                KnockoutParticipantsListBox.ItemsSource = null;
                
                // WICHTIG: RefreshFinalsView in einem separaten Thread ausführen um Deadlocks zu vermeiden
                Dispatcher.BeginInvoke(() =>
                {
                    RefreshFinalsView();
                }, DispatcherPriority.Loaded);
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: KnockoutPhase");
                
                // Group DataGrids explizit leeren
                MatchesDataGrid.ItemsSource = null;
                StandingsDataGrid.ItemsSource = null;
                // Finals DataGrids explizit leeren
                FinalsMatchesDataGrid.ItemsSource = null;
                FinalsStandingsDataGrid.ItemsSource = null;
                FinalistsListBox.ItemsSource = null;
                
                // WICHTIG: RefreshKnockoutView in einem separaten Thread ausführen um Deadlocks zu vermeiden  
                Dispatcher.BeginInvoke(() =>
                {
                    RefreshKnockoutView();
                }, DispatcherPriority.Loaded);
            }
            else
            {
                // NEU: Fallback - alle DataGrids explizit leeren
                System.Diagnostics.Debug.WriteLine("UpdateMatchesView: Unknown phase - clearing all DataGrids");
                
                MatchesDataGrid.ItemsSource = null;
                StandingsDataGrid.ItemsSource = null;
                KnockoutMatchesDataGrid.ItemsSource = null;
                LoserBracketDataGrid.ItemsSource = null;
                KnockoutParticipantsListBox.ItemsSource = null;
                FinalsMatchesDataGrid.ItemsSource = null;
                FinalsStandingsDataGrid.ItemsSource = null;
                FinalistsListBox.ItemsSource = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: ERROR: {ex.Message}");
        }
    }

    private void RefreshFinalsView()
    {
        System.Diagnostics.Debug.WriteLine("RefreshFinalsView: START");
        
        try
        {
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: In RoundRobinFinals phase");
                
                var finalsGroup = TournamentClass.CurrentPhase.FinalsGroup;
                if (finalsGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: FinalsGroup found with {finalsGroup.Players.Count} players and {finalsGroup.Matches.Count} matches");
                    
                    // WICHTIG: Stelle sicher dass Matches generiert sind
                    if (!finalsGroup.MatchesGenerated && finalsGroup.Players.Count >= 2)
                    {
                        System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: Generating Finals matches for {finalsGroup.Players.Count} players");
                        finalsGroup.GenerateRoundRobinMatches(TournamentClass.GameRules);
                    }
                    
                    // UI Components aktualisieren
                    Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: Updating UI components");
                            
                            FinalistsListBox.ItemsSource = finalsGroup.Players;
                            FinalsMatchesDataGrid.ItemsSource = finalsGroup.Matches;
                            
                            var standings = finalsGroup.GetStandings();
                            FinalsStandingsDataGrid.ItemsSource = standings;
                            

                            System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: UI updated - Players: {finalsGroup.Players.Count}, Matches: {finalsGroup.Matches.Count}, Standings: {standings.Count}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: ERROR updating UI: {ex.Message}");
                        }
                    }, DispatcherPriority.DataBind);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: ERROR - FinalsGroup is null!");
                    
                    // Versuche die FinalsGroup zu erstellen falls sie fehlt
                    if (TournamentClass.CurrentPhase.QualifiedPlayers?.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: Attempting to create missing FinalsGroup with {TournamentClass.CurrentPhase.QualifiedPlayers.Count} qualified players");
                        
                        var recreatedGroup = new Group
                        {
                            Id = 999,
                            Name = "Finalrunde",
                            MatchesGenerated = false
                        };
                        
                        foreach (var player in TournamentClass.CurrentPhase.QualifiedPlayers)
                        {
                            recreatedGroup.Players.Add(player);
                        }
                        
                        TournamentClass.CurrentPhase.FinalsGroup = recreatedGroup;
                        
                        // Rekursiver Aufruf nach der Reparatur
                        RefreshFinalsView();
                        return;
                    }
                    
                    // Fallback: UI leeren
                    Dispatcher.BeginInvoke(() =>
                    {
                        FinalistsListBox.ItemsSource = null;
                        FinalsMatchesDataGrid.ItemsSource = null;
                        FinalsStandingsDataGrid.ItemsSource = null;
                    }, DispatcherPriority.DataBind);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: Not in RoundRobinFinals phase (current: {TournamentClass?.CurrentPhase?.PhaseType})");
                
                // Nicht in Finals Phase - UI leeren
                Dispatcher.BeginInvoke(() =>
                {
                    FinalistsListBox.ItemsSource = null;
                    FinalsMatchesDataGrid.ItemsSource = null;
                    FinalsStandingsDataGrid.ItemsSource = null;
                }, DispatcherPriority.DataBind);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: Stack trace: {ex.StackTrace}");
        }
        
        System.Diagnostics.Debug.WriteLine("RefreshFinalsView: END");
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
            
            TournamentUIHelper.DrawEmptyBracketMessage(canvas, message, isLoserBracket, _localizationService);
            return;
        }

        TournamentKnockoutHelper.DrawStaticBracketTree(canvas, matches, isLoserBracket, _localizationService);
    }

    private void DrawEmptyBracketMessage(Canvas canvas, string message, bool isLoserBracket)
    {
        TournamentUIHelper.DrawEmptyBracketMessage(canvas, message, isLoserBracket, _localizationService);
    }

    private void DrawKnockoutBracket(Canvas canvas, List<KnockoutMatch> matches, bool isLoserBracket)
    {
        TournamentKnockoutHelper.DrawStaticBracketTree(canvas, matches, isLoserBracket, _localizationService);
    }

    private Border CreateKnockoutMatchControl(KnockoutMatch match, double width, double height)
    {
        return TournamentUIHelper.CreateKnockoutMatchControl(match, width, height, _localizationService);
    }

    private void DrawBracketConnectionLine(Canvas canvas, double x1, double y1, double x2, double y2)
    {
        TournamentUIHelper.DrawBracketConnectionLine(canvas, x1, y1, x2, y2);
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
            
            // Use validation helper for automatic group status checking
            if (sender is Match match && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                var parentWindow = Window.GetWindow(this);
                Task.Run(() => TournamentValidationHelper.CheckAllGroupsCompletion(TournamentClass, parentWindow, _localizationService));
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

    /// <summary>
    /// Hilfsmethode: Findet das TextBlock-Element im Header eines TabItems
    /// Tab-Header bestehen aus StackPanels mit Icons und TextBlocks
    /// </summary>
    /// <param name="tabItem">Das TabItem dessen Header durchsucht werden soll</param>
    /// <returns>Das TextBlock-Element oder null wenn nicht gefunden</returns>
    private TextBlock? FindTextBlockInHeader(TabItem tabItem)
    {
        return TournamentUIHelper.FindTextBlockInHeader(tabItem);
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

    // Event-Handler für Auto-Save Timer
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
            
            // Show toast notification using helper
            var parentGrid = Window.GetWindow(this)?.Content as Grid;
            if (parentGrid != null)
            {
                var successMessage = _localizationService?.GetString("UIRefreshed") ?? "Benutzeroberfläche wurde aktualisiert";
                var title = _localizationService?.GetString("Information") ?? "Information";
                TournamentUIHelper.ShowToastNotification(parentGrid, title, successMessage);
            }
            
            System.Diagnostics.Debug.WriteLine("RefreshUIButton_Click: UI refresh completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshUIButton_Click: ERROR: {ex.Message}");
        
            var errorMessage = $"{_localizationService?.GetString("ErrorRefreshing") ?? "Fehler beim Aktualisieren:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void ResetFinalsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (TournamentDialogHelper.ShowResetFinalsConfirmation(Window.GetWindow(this), _localizationService))
            {
                // Reset to group phase using helper
                TournamentKnockoutHelper.ResetToGroupPhase(TournamentClass);
                
                // WICHTIG: Canvas-Elemente und DataGrids explizit leeren
                ClearKnockoutCanvases();
                
                // ALLE UI-Komponenten aktualisieren
                UpdateUI();
                UpdatePlayersView();
                UpdateMatchesView();
                UpdatePhaseDisplay();
                
                // Finals-spezifische DataGrids explizit leeren
                FinalsMatchesDataGrid.ItemsSource = null;
                FinalsStandingsDataGrid.ItemsSource = null;
                FinalistsListBox.ItemsSource = null;
                
                // Finals-Tab ausblenden
                FinalsTabItem.Visibility = Visibility.Collapsed;
                
                // Automatisch zum Setup-Tab wechseln
                if (MainTabControl != null && SetupTabItem != null)
                {
                    MainTabControl.SelectedItem = SetupTabItem;
                }
                
                var successMessage = _localizationService?.GetString("ResetFinalsComplete") ?? "Die Finalrunde wurde erfolgreich zurückgesetzt.";
                TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService, Window.GetWindow(this));
                
                // Trigger data changed event
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void ResetKnockoutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (TournamentDialogHelper.ShowResetKnockoutConfirmation(Window.GetWindow(this), _localizationService))
            {
                // Reset to group phase using helper
                TournamentKnockoutHelper.ResetToGroupPhase(TournamentClass);
                
                // WICHTIG: Canvas-Elemente explizit leeren
                ClearKnockoutCanvases();
                
                // ALLE UI-Komponenten aktualisieren
                UpdateUI();
                UpdatePlayersView();
                UpdateMatchesView();  // Leert KO-DataGrids
                UpdatePhaseDisplay();
                
                // Knockout-spezifische DataGrids explizit leeren
                KnockoutMatchesDataGrid.ItemsSource = null;
                LoserBracketDataGrid.ItemsSource = null;
                KnockoutParticipantsListBox.ItemsSource = null;
                
                // Knockout-Tabs ausblenden
                KnockoutTabItem.Visibility = Visibility.Collapsed;
                LoserBracketTab.Visibility = Visibility.Collapsed;
                LoserBracketTreeTab.Visibility = Visibility.Collapsed;
                
                // Automatisch zum Setup-Tab wechseln
                if (MainTabControl != null && SetupTabItem != null)
                {
                    MainTabControl.SelectedItem = SetupTabItem;
                }
                
                var successMessage = _localizationService?.GetString("ResetKnockoutComplete") ?? "Die K.-o.-Phase wurde erfolgreich zurücksetz.";
                TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService, Window.GetWindow(this));
                
                // Trigger data changed event
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
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
                
                // Use validation helper for automatic group status checking
                var parentWindow = Window.GetWindow(this);
                Task.Run(() => TournamentValidationHelper.CheckAllGroupsCompletion(TournamentClass, parentWindow, _localizationService));
                
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
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.ScoreDisplay));
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.StatusDisplay));
                selectedMatch.ForcePropertyChanged(nameof(selectedMatch.WinnerDisplay));
                
                RefreshFinalsView();
                
                // Auto-check finals completion using helper
                var parentWindow = Window.GetWindow(this);
                Task.Run(() => TournamentValidationHelper.CheckFinalsCompletion(TournamentClass, parentWindow, _localizationService));
                
                OnDataChanged();
            }
        }
    }

    private void KnockoutMatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (KnockoutMatchesDataGrid.SelectedItem is KnockoutMatchViewModel viewModel)
        {
            if (TournamentKnockoutHelper.OpenMatchResultWindow(viewModel.Match, TournamentClass.GameRules, Window.GetWindow(this), _localizationService))
            {
                RefreshKnockoutView();
                OnDataChanged();
            }
        }
    }

    private void LoserBracketDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (LoserBracketDataGrid.SelectedItem is KnockoutMatchViewModel viewModel)
        {
            if (TournamentKnockoutHelper.OpenMatchResultWindow(viewModel.Match, TournamentClass.GameRules, Window.GetWindow(this), _localizationService))
            {
                RefreshKnockoutView();
                OnDataChanged();
            }
        }
    }

    private void GiveByeButton_Click(object sender, RoutedEventArgs e)
    {
        KnockoutMatch? match = null;

        // Extract match from button tag
        if (sender is Button button)
        {
            if (button.Tag is KnockoutMatchViewModel viewModel)
            {
                match = viewModel.Match;
            }
            else if (button.Tag is KnockoutMatch knockoutMatch)
            {
                match = knockoutMatch;
            }
        }

        if (match != null)
        {
            if (TournamentKnockoutHelper.ProcessByeSelection(TournamentClass, match, Window.GetWindow(this), _localizationService))
            {
                RefreshKnockoutView();
                OnDataChanged();
            }
        }
    }

    private void UndoByeButton_Click(object sender, RoutedEventArgs e)
    {
        KnockoutMatch? match = null;

        // Extract match from button tag
        if (sender is Button button)
        {
            if (button.Tag is KnockoutMatchViewModel viewModel)
            {
                match = viewModel.Match;
            }
            else if (button.Tag is KnockoutMatch knockoutMatch)
            {
                match = knockoutMatch;
            }
        }

        if (match != null)
        {
            if (TournamentKnockoutHelper.HandleUndoKnockoutBye(TournamentClass, match, _localizationService))
            {
                RefreshKnockoutView();
                OnDataChanged();
            }
        }
    }

    private DateTime? _lastCompletionNotification;

    // Helper methods for knockout bye handling - now in TournamentKnockoutHelper
    // Helper methods for dialogs - now in TournamentDialogHelper  
    // Helper methods for validation - now in TournamentValidationHelper
    
    // Old complex methods removed - now using helper classes

    // Missing Event Handlers
    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"MainTabControl_SelectionChanged: START");
        
        if (e.Source == MainTabControl && MainTabControl.SelectedItem is TabItem selectedTab)
        {
            System.Diagnostics.Debug.WriteLine($"MainTabControl_SelectionChanged: Selected tab: {selectedTab.Name}");
            
            try
            {
                // Handle specific tab selections to ensure proper data loading
                if (selectedTab.Name == "FinalsTabItem" && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
                {
                    System.Diagnostics.Debug.WriteLine($"MainTabControl_SelectionChanged: Refreshing Finals view on tab selection");
                    
                    // WICHTIG: Finals View explizit refreshen wenn Tab besucht wird
                    Dispatcher.BeginInvoke(() =>
                    {
                        RefreshFinalsView();
                    }, DispatcherPriority.DataBind);
                }
                else if (selectedTab.Name == "KnockoutTabItem" && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
                {
                    System.Diagnostics.Debug.WriteLine($"MainTabControl_SelectionChanged: Refreshing Knockout view on tab selection");
                    
                    // WICHTIG: Knockout View explizit refreshen wenn Tab besucht wird
                    Dispatcher.BeginInvoke(() =>
                    {
                        RefreshKnockoutView();
                    }, DispatcherPriority.DataBind);
                }
                else if (selectedTab.Name == "GroupPhaseTabItem" && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
                {
                    System.Diagnostics.Debug.WriteLine($"MainTabControl_SelectionChanged: Refreshing Group phase view on tab selection");
                    
                    // Gruppenphasen-spezifische Aktualisierung
                    UpdateMatchesView();
                }
                
                // Allgemeine Phase Display Aktualisierung
                UpdatePhaseDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainTabControl_SelectionChanged: ERROR: {ex.Message}");
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"MainTabControl_SelectionChanged: END");
    }

    private void AdvanceToNextPhaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var validation = TournamentValidationHelper.ValidatePhaseAdvancement(TournamentClass, _localizationService);
            if (!validation.CanAdvance)
            {
                TournamentDialogHelper.ShowWarning(validation.ErrorMessage!, null, _localizationService);
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
            var errorMessage = $"{_localizationService?.GetString("ErrorAdvancingPhase") ?? "Fehler beim Wechsel in die nächste Phase:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void ResetMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null) return;

        try
        {
            if (TournamentDialogHelper.ShowResetMatchesConfirmation(Window.GetWindow(this), SelectedGroup.Name, _localizationService))
            {
                SelectedGroup.ResetMatches();
                UpdateMatchesView();
                OnDataChanged();

                var successMessage = _localizationService?.GetString("MatchesResetSuccess") ?? "Spiele wurden zurückgesetzt!";
                TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Zurücksetzen der Spiele: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void ResetTournamentButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (TournamentDialogHelper.ShowResetTournamentConfirmation(Window.GetWindow(this), _localizationService))
            {
                TournamentKnockoutHelper.ResetToGroupPhase(TournamentClass);
                
                var successMessage = _localizationService?.GetString("TournamentResetComplete") ?? "Das Turnier wurde erfolgreich zurückgesetzt.";
                TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen des Turniers:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void AddGroupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _localizationService?.GetString("NewGroup") ?? "Neue Gruppe";
            var prompt = _localizationService?.GetString("GroupName") ?? "Geben Sie den Namen der neuen Gruppe ein:";
            var defaultName = $"Group {_nextGroupId}";
            
            var name = TournamentDialogHelper.ShowInputDialog(Window.GetWindow(this), prompt, title, defaultName, _localizationService);
            
            if (!string.IsNullOrWhiteSpace(name))
            {
                var validation = TournamentValidationHelper.ValidateGroupInput(name, TournamentClass.Groups, _localizationService);
                if (!validation.IsValid)
                {
                    TournamentDialogHelper.ShowWarning(validation.ErrorMessage!, null, _localizationService);
                    return;
                }

                var group = new Group(_nextGroupId, name.Trim());
                TournamentClass.Groups.Add(group);
                
                UpdateNextIds();
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Hinzufügen der Gruppe: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
    {
        if (GroupsListBox.SelectedItem is not Group selectedGroup)
        {
            var message = _localizationService?.GetString("NoGroupSelected") ?? "Bitte wählen Sie eine Gruppe aus, die entfernt werden soll.";
            TournamentDialogHelper.ShowInformation(message, null, _localizationService);
            return;
        }

        try
        {
            if (TournamentDialogHelper.ShowRemoveGroupConfirmation(Window.GetWindow(this), selectedGroup.Name, _localizationService))
            {
                TournamentClass.Groups.Remove(selectedGroup);
                SelectedGroup = null;
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Entfernen der Gruppe: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
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
        var validation = TournamentValidationHelper.ValidatePlayerInput(NewPlayerName, SelectedGroup, _localizationService);
        if (!validation.IsValid)
        {
            TournamentDialogHelper.ShowInformation(validation.ErrorMessage!, null, _localizationService);
            return;
        }

        try
        {
            var player = new Player(_nextPlayerId, NewPlayerName.Trim());
            SelectedGroup!.Players.Add(player);
            
            NewPlayerName = string.Empty;
            UpdateNextIds();
            UpdatePlayersView();
            OnDataChanged();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Hinzufügen des Spielers: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void RemovePlayerButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPlayer == null)
        {
            var message = _localizationService?.GetString("NoPlayerSelected") ?? "Bitte wählen Sie einen Spieler aus, der entfernt werden soll.";
            TournamentDialogHelper.ShowInformation(message, null, _localizationService);
            return;
        }

        try
        {
            if (TournamentDialogHelper.ShowRemovePlayerConfirmation(Window.GetWindow(this), SelectedPlayer.Name, _localizationService))
            {
                SelectedGroup?.Players.Remove(SelectedPlayer);
                SelectedPlayer = null;
                UpdatePlayersView();
                OnDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Entfernen des Spielers: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void GenerateMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedGroup == null) return;

        try
        {
            var validation = TournamentValidationHelper.ValidateMatchGeneration(SelectedGroup, _localizationService);
            if (!validation.CanGenerate)
            {
                TournamentDialogHelper.ShowWarning(validation.ErrorMessage!, null, _localizationService);
                return;
            }

            SelectedGroup.GenerateRoundRobinMatches();
            UpdateMatchesView();
            UpdatePlayersView();
            OnDataChanged();

            var successMessage = _localizationService?.GetString("MatchesGeneratedSuccess") ?? "Spiele wurden erfolgreich erstellt!";
            TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService);
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService?.GetString("ErrorGeneratingMatches") ?? "Fehler beim Erstellen der Spiele:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
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

    private void KnockoutDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Handle right-click context menu for knockout matches if needed
        // This can be implemented later for additional functionality
        // Currently just a placeholder to satisfy XAML bindings
    }

    /// <summary>
    /// Leert alle Knockout-spezifischen Canvas-Elemente
    /// </summary>
    private void ClearKnockoutCanvases()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ClearKnockoutCanvases: Clearing all knockout-related canvases");
            
            // Winner Bracket Canvas leeren
            if (BracketCanvas != null)
            {
                System.Diagnostics.Debug.WriteLine($"  - Clearing BracketCanvas with {BracketCanvas.Children.Count} children");
                BracketCanvas.Children.Clear();
                BracketCanvas.Background = System.Windows.Media.Brushes.White;
            }
            
            // Loser Bracket Canvas leeren
            if (LoserBracketCanvas != null)
            {
                System.Diagnostics.Debug.WriteLine($"  - Clearing LoserBracketCanvas with {LoserBracketCanvas.Children.Count} children");
                LoserBracketCanvas.Children.Clear();
                LoserBracketCanvas.Background = System.Windows.Media.Brushes.White;
            }
            
            System.Diagnostics.Debug.WriteLine("ClearKnockoutCanvases: Successfully cleared all canvases");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClearKnockoutCanvases: ERROR: {ex.Message}");
        }
    }
}