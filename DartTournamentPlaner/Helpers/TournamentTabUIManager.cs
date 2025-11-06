using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.ViewModels;
using DartTournamentPlaner.Helpers;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// UI Manager für TournamentTab - verwaltet alle UI-Updates und -Displays
/// </summary>
public class TournamentTabUIManager : IDisposable
{
    private readonly TournamentClass _tournamentClass;
    private readonly LocalizationService _localizationService;
    private readonly Dispatcher _dispatcher;
    private bool _disposed = false;

    // UI Elements - werden vom TournamentTab gesetzt
    public ListBox? GroupsListBox { get; set; }
    public ListBox? GroupPhaseGroupsList { get; set; }
    public ListBox? PlayersListBox { get; set; }
    public ListBox? KnockoutParticipantsListBox { get; set; }
    public ListBox? FinalistsListBox { get; set; }
    public DataGrid? MatchesDataGrid { get; set; }
    public DataGrid? StandingsDataGrid { get; set; }
    public DataGrid? KnockoutMatchesDataGrid { get; set; }
    public DataGrid? LoserBracketDataGrid { get; set; }
    public DataGrid? FinalsMatchesDataGrid { get; set; }
    public DataGrid? FinalsStandingsDataGrid { get; set; }
    public TextBlock? PlayersHeaderText { get; set; }
    public TextBlock? CurrentPhaseText { get; set; }
    public TextBlock? TournamentOverviewText { get; set; }
    public Button? GenerateMatchesButton { get; set; }
    public Button? ResetMatchesButton { get; set; }
    public Button? AdvanceToNextPhaseButton { get; set; }
    public Button? ResetTournamentButton { get; set; }
    public Button? AddPlayerButton { get; set; }
    public Button? RemoveGroupButton { get; set; }
    public TextBox? PlayerNameTextBox { get; set; }
    public TabItem? FinalsTabItem { get; set; }
    public TabItem? KnockoutTabItem { get; set; }
    public TabItem? LoserBracketTab { get; set; }
    public TabItem? LoserBracketTreeTab { get; set; }
    public Canvas? BracketCanvas { get; set; }
    public Canvas? LoserBracketCanvas { get; set; }

    public TournamentTabUIManager(TournamentClass tournamentClass, LocalizationService localizationService, Dispatcher dispatcher)
    {
        _tournamentClass = tournamentClass;
        _localizationService = localizationService;
        _dispatcher = dispatcher;
        
        // Theme-Change Event abonnieren
        if (App.ThemeService != null)
        {
            App.ThemeService.ThemeChanged += OnThemeChanged;
            System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Subscribed to ThemeChanged event");
        }
    }

    /// <summary>
    /// Event-Handler für Theme-Änderungen
    /// </summary>
    private void OnThemeChanged(object? sender, string newTheme)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Theme changed to: {newTheme} - refreshing tournament trees");
            
            _dispatcher.BeginInvoke(() =>
            {
                try
                {
                    // Refresh die Tournament Trees wenn wir in der KO-Phase sind
                    if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
                    {
                        System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Refreshing tournament trees for KO phase");
                        DrawBracketTree();
                        DrawLoserBracketTree();
                    }
                    else
                    {
                        // Auch für andere Phasen die Canvas-Hintergründe aktualisieren
                        UpdateCanvasBackgrounds();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Error in OnThemeChanged dispatcher action: {ex.Message}");
                }
            }, DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Error in OnThemeChanged: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert die Canvas-Hintergründe basierend auf dem aktuellen Theme
    /// </summary>
    private void UpdateCanvasBackgrounds()
    {
        try
        {
            if (BracketCanvas != null)
            {
                BracketCanvas.Background = GetThemeBrush("BackgroundBrush", Brushes.White);
                System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Updated BracketCanvas background");
            }
            
            if (LoserBracketCanvas != null)
            {
                LoserBracketCanvas.Background = GetThemeBrush("BackgroundBrush", Brushes.White);
                System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Updated LoserBracketCanvas background");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Error updating canvas backgrounds: {ex.Message}");
        }
    }

    public void UpdateUI()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[UI-MANAGER] UpdateUI() called for {_tournamentClass.Name}");
            
            // ✅ KORRIGIERT: Verwende korrekte Methoden
            UpdatePhaseDisplay();
            UpdateTournamentOverview();
            UpdateButtonStates(); // ✅ WICHTIG: Button-States aktualisieren
            
            if (GroupsListBox?.SelectedItem is Group selectedGroup)
            {
                UpdatePlayersView(selectedGroup);
                UpdateMatchesView(selectedGroup);
            }
            
            // ✅ Weitere UI-Updates falls verfügbar
            if (GroupsListBox != null)
            {
                GroupsListBox.ItemsSource = _tournamentClass.Groups;
            }
            if (GroupPhaseGroupsList != null)
            {
                GroupPhaseGroupsList.ItemsSource = _tournamentClass.Groups;
            }
            
            System.Diagnostics.Debug.WriteLine($"[UI-MANAGER] UpdateUI() completed for {_tournamentClass.Name}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UI-MANAGER] UpdateUI() ERROR: {ex.Message}");
        }
    }

    public void UpdatePlayersView(Group? selectedGroup)
    {
        try
        {
            if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                UpdateGroupPhasePlayersView(selectedGroup);
            }
            else if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                UpdateFinalsPlayersView();
            }
            else if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                UpdateKnockoutPlayersView();
            }
            else
            {
                ClearPlayersView();
            }
            
            UpdatePhaseDisplay();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdatePlayersView: ERROR: {ex.Message}");
        }
    }

    private void UpdateGroupPhasePlayersView(Group? selectedGroup)
    {
        if (selectedGroup != null)
        {
            _dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (PlayersListBox != null)
                        PlayersListBox.ItemsSource = selectedGroup.Players;
                    
                    if (PlayersHeaderText != null)
                    {
                        PlayersHeaderText.Text = _localizationService != null
                            ? _localizationService.GetString("PlayersInGroup", selectedGroup.Name)
                            : $"Spieler in {selectedGroup.Name}:";
                    }

                    UpdatePlayerManagementButtons(true, selectedGroup);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateGroupPhasePlayersView: ERROR: {ex.Message}");
                }
            }, DispatcherPriority.DataBind);
        }
        else
        {
            _dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (PlayersListBox != null)
                        PlayersListBox.ItemsSource = null;
                    
                    if (PlayersHeaderText != null)
                    {
                        PlayersHeaderText.Text = _localizationService?.GetString("NoGroupSelectedPlayers")
                            ?? "Spieler: (Keine Gruppe ausgewählt)";
                    }

                    UpdatePlayerManagementButtons(false, null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateGroupPhasePlayersView (no group): ERROR: {ex.Message}");
                }
            }, DispatcherPriority.DataBind);
        }
    }

    private void UpdateFinalsPlayersView()
    {
        _tournamentClass.EnsureFinalsPhaseIntegrity();
        
        var finalsGroup = _tournamentClass.CurrentPhase?.FinalsGroup;
        if (finalsGroup != null)
        {
            _dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (PlayersListBox != null)
                        PlayersListBox.ItemsSource = finalsGroup.Players;
                    
                    if (PlayersHeaderText != null)
                    {
                        PlayersHeaderText.Text = _localizationService?.GetString("FinalistsCount", finalsGroup.Players.Count) 
                            ?? $"Finalisten ({finalsGroup.Players.Count} Spieler):";
                    }

                    // Disable player management in finals
                    UpdatePlayerManagementButtons(false, null, false);
                    
                    if (GenerateMatchesButton != null)
                        GenerateMatchesButton.IsEnabled = !finalsGroup.MatchesGenerated;
                    if (ResetMatchesButton != null)
                        ResetMatchesButton.IsEnabled = finalsGroup.MatchesGenerated;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateFinalsPlayersView: ERROR: {ex.Message}");
                }
            }, DispatcherPriority.DataBind);
        }
    }

    private void UpdateKnockoutPlayersView()
    {
        var qualifiedPlayers = _tournamentClass.CurrentPhase?.QualifiedPlayers;
        if (qualifiedPlayers != null)
        {
            _dispatcher.BeginInvoke(() =>
            {
                try
                {
                    if (PlayersListBox != null)
                        PlayersListBox.ItemsSource = qualifiedPlayers;
                    
                    if (PlayersHeaderText != null)
                    {
                        PlayersHeaderText.Text = _localizationService?.GetString("KnockoutParticipantsCount", qualifiedPlayers.Count) 
                            ?? $"KO-Teilnehmer ({qualifiedPlayers.Count} Spieler):";
                    }

                    // Disable all player management in knockout
                    UpdatePlayerManagementButtons(false, null, false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UpdateKnockoutPlayersView: ERROR: {ex.Message}");
                }
            }, DispatcherPriority.DataBind);
        }
    }

    private void ClearPlayersView()
    {
        _dispatcher.BeginInvoke(() =>
        {
            try
            {
                if (PlayersListBox != null)
                    PlayersListBox.ItemsSource = null;
                if (KnockoutParticipantsListBox != null)
                    KnockoutParticipantsListBox.ItemsSource = null;
                if (FinalistsListBox != null)
                    FinalistsListBox.ItemsSource = null;
                
                if (PlayersHeaderText != null)
                {
                    PlayersHeaderText.Text = _localizationService?.GetString("NoGroupSelectedPlayers") 
                        ?? "Spieler: (Keine Gruppe ausgewählt)";
                }
                
                UpdatePlayerManagementButtons(false, null, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ClearPlayersView: ERROR: {ex.Message}");
            }
        }, DispatcherPriority.DataBind);
    }

    public void UpdateMatchesView(Group? selectedGroup)
    {
        try
        {
            if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                UpdateGroupPhaseMatchesView(selectedGroup);
            }
            else if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                UpdateFinalsMatchesView();
            }
            else if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                UpdateKnockoutMatchesView();
            }
            else
            {
                ClearAllMatchViews();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateMatchesView: ERROR: {ex.Message}");
        }
    }

    private void UpdateGroupPhaseMatchesView(Group? selectedGroup)
    {
        // Clear other views
        ClearKnockoutViews();
        ClearFinalsViews();
        
        if (selectedGroup != null)
        {
            if (MatchesDataGrid != null)
                MatchesDataGrid.ItemsSource = selectedGroup.Matches;
            
            if (StandingsDataGrid != null)
            {
                var standings = selectedGroup.GetStandings();
                StandingsDataGrid.ItemsSource = standings;
            }
        }
        else
        {
            if (MatchesDataGrid != null)
                MatchesDataGrid.ItemsSource = null;
            if (StandingsDataGrid != null)
                StandingsDataGrid.ItemsSource = null;
        }
    }

    private void UpdateFinalsMatchesView()
    {
        // Clear other views
        ClearGroupViews();
        ClearKnockoutViews();
        
        _dispatcher.BeginInvoke(() =>
        {
            RefreshFinalsView();
        }, DispatcherPriority.Loaded);
    }

    private void UpdateKnockoutMatchesView()
    {
        // Clear other views
        ClearGroupViews();
        ClearFinalsViews();
        
        _dispatcher.BeginInvoke(() =>
        {
            RefreshKnockoutView();
        }, DispatcherPriority.Loaded);
    }

    public void RefreshFinalsView()
    {
        try
        {
            if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                var finalsGroup = _tournamentClass.CurrentPhase.FinalsGroup;
                if (finalsGroup != null)
                {
                    // Generate matches if not done
                    if (!finalsGroup.MatchesGenerated && finalsGroup.Players.Count >= 2)
                    {
                        finalsGroup.GenerateRoundRobinMatches(_tournamentClass.GameRules);
                    }
                    
                    _dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            if (FinalistsListBox != null)
                                FinalistsListBox.ItemsSource = finalsGroup.Players;
                            if (FinalsMatchesDataGrid != null)
                                FinalsMatchesDataGrid.ItemsSource = finalsGroup.Matches;
                            
                            if (FinalsStandingsDataGrid != null)
                            {
                                var standings = finalsGroup.GetStandings();
                                FinalsStandingsDataGrid.ItemsSource = standings;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"RefreshFinalsView UI update: ERROR: {ex.Message}");
                        }
                    }, DispatcherPriority.DataBind);
                }
                else
                {
                    ClearFinalsViews();
                }
            }
            else
            {
                ClearFinalsViews();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshFinalsView: ERROR: {ex.Message}");
        }
    }

    public void RefreshKnockoutView()
    {
        if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            if (KnockoutParticipantsListBox != null)
                KnockoutParticipantsListBox.ItemsSource = _tournamentClass.CurrentPhase.QualifiedPlayers;
            
            var winnerBracketMatches = _tournamentClass.CurrentPhase.WinnerBracket.Select(match =>
                new KnockoutMatchViewModel(match, _tournamentClass)).ToList();
            
            if (KnockoutMatchesDataGrid != null)
                KnockoutMatchesDataGrid.ItemsSource = winnerBracketMatches;
            
            if (_tournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                var loserBracketMatches = _tournamentClass.CurrentPhase.LoserBracket.Select(match =>
                    new KnockoutMatchViewModel(match, _tournamentClass)).ToList();
                
                if (LoserBracketDataGrid != null)
                    LoserBracketDataGrid.ItemsSource = loserBracketMatches;
                
                if (LoserBracketTab != null)
                    LoserBracketTab.Visibility = Visibility.Visible;
                if (LoserBracketTreeTab != null)
                    LoserBracketTreeTab.Visibility = Visibility.Visible;
            }
            else
            {
                if (LoserBracketTab != null)
                    LoserBracketTab.Visibility = Visibility.Collapsed;
                if (LoserBracketTreeTab != null)
                    LoserBracketTreeTab.Visibility = Visibility.Collapsed;
            }
            
            // Draw tournament trees
            DrawBracketTree();
            DrawLoserBracketTree();
        }
    }

    public void UpdatePhaseDisplay()
    {
        if (_tournamentClass?.CurrentPhase == null) return;

        try
        {
            var phaseText = _tournamentClass.CurrentPhase.PhaseType switch
            {
                TournamentPhaseType.GroupPhase => _localizationService?.GetString("GroupPhase") ?? "Gruppenphase",
                TournamentPhaseType.RoundRobinFinals => _localizationService?.GetString("FinalsPhase") ?? "Finalrunde",
                TournamentPhaseType.KnockoutPhase => _localizationService?.GetString("KnockoutPhase") ?? "KO-Phase",
                _ => "Unbekannte Phase"
            };

            var currentPhaseLabel = _localizationService?.GetString("CurrentPhase") ?? "Aktuelle Phase";
            if (CurrentPhaseText != null)
                CurrentPhaseText.Text = $"{currentPhaseLabel}: {phaseText}";

            UpdateTabVisibility();
            UpdateAdvanceButton();
            UpdateTournamentOverview();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdatePhaseDisplay: ERROR: {ex.Message}");
        }
    }

    private void UpdateTabVisibility()
    {
        var hasRoundRobinFinals = _tournamentClass.GameRules.PostGroupPhaseMode == PostGroupPhaseMode.RoundRobinFinals;
        var hasKnockout = _tournamentClass.GameRules.PostGroupPhaseMode == PostGroupPhaseMode.KnockoutBracket;
        var hasDoubleElimination = _tournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination;

        if (FinalsTabItem != null)
            FinalsTabItem.Visibility = hasRoundRobinFinals ? Visibility.Visible : Visibility.Collapsed;
        if (KnockoutTabItem != null)
            KnockoutTabItem.Visibility = hasKnockout ? Visibility.Visible : Visibility.Collapsed;
        if (LoserBracketTab != null)
            LoserBracketTab.Visibility = hasKnockout && hasDoubleElimination ? Visibility.Visible : Visibility.Collapsed;
        if (LoserBracketTreeTab != null)
            LoserBracketTreeTab.Visibility = hasKnockout && hasDoubleElimination ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateAdvanceButton()
    {
        bool canAdvance = false;
        try
        {
            canAdvance = _tournamentClass.CanProceedToNextPhase() && 
                       _tournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateAdvanceButton: ERROR: {ex.Message}");
        }

        if (AdvanceToNextPhaseButton != null)
        {
            AdvanceToNextPhaseButton.IsEnabled = canAdvance;
            AdvanceToNextPhaseButton.Visibility = _tournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None 
                ? Visibility.Visible 
                : Visibility.Collapsed;

            // Show what next phase would be
            if (canAdvance)
            {
                try
                {
                    var nextPhase = _tournamentClass.GetNextPhase();
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
                    System.Diagnostics.Debug.WriteLine($"UpdateAdvanceButton next phase: ERROR: {ex.Message}");
                }
            }
        }
    }

    private void UpdateTournamentOverview()
    {
        if (_tournamentClass == null || TournamentOverviewText == null) return;

        var overview = $"{_localizationService?.GetString("TournamentName") ?? "🏆 Turnier:"} {_tournamentClass.Name}\n\n";
        overview += $"{_localizationService?.GetString("CurrentPhase") ?? "🎯 Aktuelle Phase:"} {_tournamentClass.CurrentPhase?.Name}\n";
        overview += $"{_localizationService?.GetString("GroupsCount") ?? "👥 Gruppen:"} {_tournamentClass.Groups.Count}\n";
        overview += $"{_localizationService?.GetString("PlayersTotal") ?? "🎮 Spieler gesamt:"} {_tournamentClass.Groups.SelectMany(g => g.Players).Count()}\n\n";
        
        overview += $"{_localizationService?.GetString("GameRulesColon") ?? "📋 Spielregeln:"}\n{_tournamentClass.GameRules}\n\n";

        if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
        {
            var finishedGroups = _tournamentClass.Groups.Count(g => g.MatchesGenerated && g.Matches.All(m => m.Status == MatchStatus.Finished || m.IsBye));
            overview += $"{_localizationService?.GetString("CompletedGroups") ?? "✅ Abgeschlossene Gruppen:"} {finishedGroups}/{_tournamentClass.Groups.Count}\n";
        }
        else if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
            var qualifiedCount = _tournamentClass.CurrentPhase.QualifiedPlayers.Count;
            overview += $"{_localizationService?.GetString("QualifiedPlayers") ?? "🏅 Qualifizierte Spieler:"} {qualifiedCount}\n";
        }
        else if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            var totalMatches = _tournamentClass.CurrentPhase.WinnerBracket.Count;
            var finishedMatches = _tournamentClass.CurrentPhase.WinnerBracket.Count(m => m.Status == MatchStatus.Finished);
            overview += $"{_localizationService?.GetString("KnockoutMatches") ?? "⚔️ KO-Spiele:"} {finishedMatches}/{totalMatches} {_localizationService?.GetString("Completed") ?? "beendet"}\n";
        }

        TournamentOverviewText.Text = overview;
    }

    private void UpdateButtonStates()
    {
        if (RemoveGroupButton != null)
            RemoveGroupButton.IsEnabled = GroupsListBox?.SelectedItem != null;

        // ✅ KORRIGIERT: Reset-Buttons sollten immer aktiviert sein, wenn es Gruppen oder Daten gibt
        var hasGroups = _tournamentClass.Groups.Count > 0;
        var hasGeneratedMatches = _tournamentClass.Groups.Any(g => g.MatchesGenerated && g.Matches.Count > 0);
        var isInAdvancedPhase = _tournamentClass.CurrentPhase?.PhaseType != TournamentPhaseType.GroupPhase;
        var hasAnyData = hasGroups || hasGeneratedMatches || isInAdvancedPhase;
        
        // ✅ Reset Tournament Button: Aktiviert wenn es irgendwelche Daten gibt
        if (ResetTournamentButton != null)
            ResetTournamentButton.IsEnabled = hasAnyData;
            
        // ✅ ERWEITERTE LOGIK: Reset Matches Button basierend auf aktueller Phase
        bool canResetMatches = false;
        
        if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
        {
            // Gruppenphase: Nur wenn Gruppe ausgewählt und Matches generiert
            var selectedGroup = GroupsListBox?.SelectedItem as Group;
            canResetMatches = selectedGroup != null && selectedGroup.MatchesGenerated && selectedGroup.Matches.Count > 0;
        }
        else if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            // K.O.-Phase: Wenn Winner oder Loser Bracket Matches vorhanden sind
            var hasWinnerBracketMatches = _tournamentClass.CurrentPhase.WinnerBracket?.Count > 0;
            var hasLoserBracketMatches = _tournamentClass.CurrentPhase.LoserBracket?.Count > 0;
            canResetMatches = hasWinnerBracketMatches || hasLoserBracketMatches;
        }
        else if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
            // Finals-Phase: Wenn Finals-Gruppe Matches hat
            var finalsGroup = _tournamentClass.CurrentPhase.FinalsGroup;
            canResetMatches = finalsGroup != null && finalsGroup.MatchesGenerated && finalsGroup.Matches.Count > 0;
        }

        if (ResetMatchesButton != null)
            ResetMatchesButton.IsEnabled = canResetMatches;
            
        System.Diagnostics.Debug.WriteLine($"[UI-MANAGER] UpdateButtonStates: Phase={_tournamentClass.CurrentPhase?.PhaseType}, canResetMatches={canResetMatches}");
        System.Diagnostics.Debug.WriteLine($"[UI-MANAGER] ResetTournamentButton.IsEnabled = {ResetTournamentButton?.IsEnabled}");
        System.Diagnostics.Debug.WriteLine($"[UI-MANAGER] ResetMatchesButton.IsEnabled = {ResetMatchesButton?.IsEnabled}");
    }

    private void UpdatePlayerManagementButtons(bool enabled, Group? selectedGroup, bool allowGeneration = true)
    {
        if (PlayerNameTextBox != null)
            PlayerNameTextBox.IsEnabled = enabled;
        if (AddPlayerButton != null)
            AddPlayerButton.IsEnabled = enabled;
        
        if (allowGeneration && GenerateMatchesButton != null)
            GenerateMatchesButton.IsEnabled = enabled && selectedGroup?.Players.Count >= 2;
        if (allowGeneration && ResetMatchesButton != null)
            ResetMatchesButton.IsEnabled = enabled && selectedGroup != null && selectedGroup.MatchesGenerated && selectedGroup.Matches.Count > 0;
    }

    private void ClearAllMatchViews()
    {
        ClearGroupViews();
        ClearKnockoutViews();
        ClearFinalsViews();
    }

    private void ClearGroupViews()
    {
        if (MatchesDataGrid != null)
            MatchesDataGrid.ItemsSource = null;
        if (StandingsDataGrid != null)
            StandingsDataGrid.ItemsSource = null;
    }

    private void ClearKnockoutViews()
    {
        if (KnockoutMatchesDataGrid != null)
            KnockoutMatchesDataGrid.ItemsSource = null;
        if (LoserBracketDataGrid != null)
            LoserBracketDataGrid.ItemsSource = null;
        if (KnockoutParticipantsListBox != null)
            KnockoutParticipantsListBox.ItemsSource = null;
    }

    private void ClearFinalsViews()
    {
        if (FinalsMatchesDataGrid != null)
            FinalsMatchesDataGrid.ItemsSource = null;
        if (FinalsStandingsDataGrid != null)
            FinalsStandingsDataGrid.ItemsSource = null;
        if (FinalistsListBox != null)
            FinalistsListBox.ItemsSource = null;
    }

    private void DrawBracketTree()
    {
        if (BracketCanvas == null || _tournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            return;

        BracketCanvas.Children.Clear();

        try
        {
            var winnerBracketContent = _tournamentClass.CreateTournamentTreeView(BracketCanvas, false, _localizationService);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DrawBracketTree: Error: {ex.Message}");
            DrawStaticBracketTree(false);
        }
    }

    private void DrawLoserBracketTree()
    {
        if (LoserBracketCanvas == null || _tournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            return;

        LoserBracketCanvas.Children.Clear();

        if (_tournamentClass.GameRules.KnockoutMode != KnockoutMode.DoubleElimination)
        {
            TournamentUIHelper.DrawEmptyBracketMessage(LoserBracketCanvas, 
                _localizationService?.GetString("NoLoserBracketSingleElimination") ?? "Kein Loser Bracket (Single Elimination)", 
                true, _localizationService);
            return;
        }

        try
        {
            var loserBracketContent = _tournamentClass.CreateTournamentTreeView(LoserBracketCanvas, true, _localizationService);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DrawLoserBracketTree: Error: {ex.Message}");
            DrawStaticBracketTree(true);
        }
    }

    private void DrawStaticBracketTree(bool isLoserBracket)
    {
        var canvas = isLoserBracket ? LoserBracketCanvas : BracketCanvas;
        if (canvas == null) return;

        var matches = isLoserBracket 
            ? _tournamentClass.CurrentPhase?.LoserBracket.ToList()
            : _tournamentClass.CurrentPhase?.WinnerBracket.ToList();

        if (matches == null || matches.Count == 0)
        {
            var message = isLoserBracket 
                ? _localizationService?.GetString("NoLoserBracketGames") ?? "Keine Loser Bracket Spiele vorhanden"
                : _localizationService?.GetString("NoWinnerBracketGames") ?? "Keine Winner Bracket Spiele vorhanden";
            
            TournamentUIHelper.DrawEmptyBracketMessage(canvas, message, isLoserBracket, _localizationService);
            return;
        }

        TournamentKnockoutHelper.DrawStaticBracketTree(canvas, matches, isLoserBracket, _localizationService);
    }

    public void ClearKnockoutCanvases()
    {
        try
        {
            if (BracketCanvas != null)
            {
                BracketCanvas.Children.Clear();
                // Theme-bewusster Hintergrund statt fest auf Weiß
                BracketCanvas.Background = GetThemeBrush("BackgroundBrush", Brushes.White);
            }
            
            if (LoserBracketCanvas != null)
            {
                LoserBracketCanvas.Children.Clear();
                // Theme-bewusster Hintergrund statt fest auf Weiß
                LoserBracketCanvas.Background = GetThemeBrush("BackgroundBrush", Brushes.White);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ClearKnockoutCanvases: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Hilfsmethode um Theme-Ressourcen zu holen
    /// </summary>
    private object? GetThemeResource(string resourceKey)
    {
        try
        {
            return Application.Current?.Resources[resourceKey];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Hilfsmethode um Brush aus Theme-Ressourcen zu holen
    /// </summary>
    private Brush GetThemeBrush(string resourceKey, Brush fallback)
    {
        return GetThemeResource(resourceKey) as Brush ?? fallback;
    }

    /// <summary>
    /// Dispose-Pattern für ordnungsgemäße Ressourcenverwaltung
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Geschützte Dispose-Methode
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                // Theme-Change Event abmelden
                if (App.ThemeService != null)
                {
                    App.ThemeService.ThemeChanged -= OnThemeChanged;
                    System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Unsubscribed from ThemeChanged event");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TournamentTabUIManager] Error during dispose: {ex.Message}");
            }
            
            _disposed = true;
        }
    }
}