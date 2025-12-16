using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using DartTournamentPlaner.Helpers;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Event Handlers für TournamentTab - ausgelagert für bessere Übersichtlichkeit
/// </summary>
public class TournamentTabEventHandlers : IDisposable
{
    private readonly TournamentClass _tournamentClass;
    private readonly LocalizationService _localizationService;
    private readonly Func<Group?> _getSelectedGroup;
    private readonly Func<Player?> _getSelectedPlayer;
    private readonly Action<string> _setNewPlayerName;
    private readonly Func<string> _getNewPlayerName;
    private readonly Action _onDataChanged;
    private readonly Action _updateNextIds;
    private readonly Action _updatePlayersView;
    private readonly Action _updateMatchesView;
    private readonly Action _refreshFinalsView;
    private readonly Action _refreshKnockoutView;
    private readonly Func<Window> _getWindow;
    private readonly Func<HubIntegrationService?> _getHubService;
    private readonly Action<TournamentPhaseType> _switchToPhaseTab;
    private readonly Action? _updateUI; // ✅ NEU: Für vollständige UI-Updates

    public TournamentTabEventHandlers(
        TournamentClass tournamentClass,
        LocalizationService localizationService,
        Func<Group?> getSelectedGroup,
        Func<Player?> getSelectedPlayer,
        Action<string> setNewPlayerName,
        Func<string> getNewPlayerName,
        Action onDataChanged,
        Action updateNextIds,
        Action updatePlayersView,
        Action updateMatchesView,
        Action refreshFinalsView,
        Action refreshKnockoutView,
        Func<Window> getWindow,
        Func<HubIntegrationService?> getHubService = null,
        Action<TournamentPhaseType>? switchToPhaseTab = null,
        Action? updateUI = null) // ✅ NEU
    {
        _tournamentClass = tournamentClass;
        _localizationService = localizationService;
        _getSelectedGroup = getSelectedGroup;
        _getSelectedPlayer = getSelectedPlayer;
        _setNewPlayerName = setNewPlayerName;
        _getNewPlayerName = getNewPlayerName;
        _onDataChanged = onDataChanged;
        _updateNextIds = updateNextIds;
        _updatePlayersView = updatePlayersView;
        _updateMatchesView = updateMatchesView;
        _refreshFinalsView = refreshFinalsView;
        _refreshKnockoutView = refreshKnockoutView;
        _getWindow = getWindow;
        _getHubService = getHubService ?? (() => null);
        _switchToPhaseTab = switchToPhaseTab ?? ((phaseType) => { }); // ✅ Default no-op
        _updateUI = updateUI; // ✅ NEU
    }

    public void ConfigureRulesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var gameRulesWindow = new Views.GameRulesWindow(_tournamentClass.GameRules, _localizationService);
            gameRulesWindow.Owner = _getWindow();
            
            gameRulesWindow.DataChanged += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine("ConfigureRulesButton_Click: GameRulesWindow DataChanged received");
                
                // Alle bestehenden Matches in allen Gruppen aktualisieren
                foreach (var group in _tournamentClass.Groups)
                {
                    if (group.MatchesGenerated && group.Matches.Count > 0)
                    {
                        group.UpdateMatchDisplaySettings(_tournamentClass.GameRules);
                    }
                }
                
                _onDataChanged();
                
                // ✅ NEU: Trigger vollständiges UI-Update nach Regel-Änderungen
                _updateUI?.Invoke();
                
                System.Diagnostics.Debug.WriteLine("ConfigureRulesButton_Click: UI update triggered");
            };
            
            gameRulesWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Öffnen der Spielregeln: {ex.Message}", "Fehler", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void AddGroupButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _localizationService.GetString("NewGroup") ?? "Neue Gruppe";
            var prompt = _localizationService.GetString("GroupName") ?? "Geben Sie den Namen der neuen Gruppe ein:";
            var defaultName = $"Group {GetNextGroupId()}";
            
            var name = TournamentDialogHelper.ShowInputDialog(_getWindow(), prompt, title, defaultName, _localizationService);
            
            if (!string.IsNullOrWhiteSpace(name))
            {
                var validation = TournamentValidationHelper.ValidateGroupInput(name, _tournamentClass.Groups, _localizationService);
                if (!validation.IsValid)
                {
                    TournamentDialogHelper.ShowWarning(validation.ErrorMessage!, null, _localizationService);
                    return;
                }

                var group = new Group(GetNextGroupId(), name.Trim());
                _tournamentClass.Groups.Add(group);
                
                _updateNextIds();
                _onDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Hinzufügen der Gruppe: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    public void RemoveGroupButton_Click(object sender, RoutedEventArgs e, ListBox groupsListBox)
    {
        if (groupsListBox.SelectedItem is not Group selectedGroup)
        {
            var message = _localizationService.GetString("NoGroupSelected") ?? "Bitte wählen Sie eine Gruppe aus, die entfernt werden soll.";
            TournamentDialogHelper.ShowInformation(message, null, _localizationService);
            return;
        }

        try
        {
            if (TournamentDialogHelper.ShowRemoveGroupConfirmation(_getWindow(), selectedGroup.Name, _localizationService))
            {
                _tournamentClass.Groups.Remove(selectedGroup);
                _onDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Entfernen der Gruppe: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    public void AddPlayerButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedGroup = _getSelectedGroup();
        var newPlayerName = _getNewPlayerName();
        
        var validation = TournamentValidationHelper.ValidatePlayerInput(newPlayerName, selectedGroup, _localizationService);
        if (!validation.IsValid)
        {
            TournamentDialogHelper.ShowInformation(validation.ErrorMessage!, null, _localizationService);
            return;
        }

        try
        {
            var player = new Player(GetNextPlayerId(), newPlayerName.Trim());
            selectedGroup!.Players.Add(player);
            
            _setNewPlayerName(string.Empty);
            _updateNextIds();
            _updatePlayersView();
            _onDataChanged();
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Hinzufügen des Spielers: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    public void RemovePlayerButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedPlayer = _getSelectedPlayer();
        var selectedGroup = _getSelectedGroup();
        
        if (selectedPlayer == null)
        {
            var message = _localizationService.GetString("NoPlayerSelected") ?? "Bitte wählen Sie einen Spieler aus, der entfernt werden soll.";
            TournamentDialogHelper.ShowInformation(message, null, _localizationService);
            return;
        }

        try
        {
            if (TournamentDialogHelper.ShowRemovePlayerConfirmation(_getWindow(), selectedPlayer.Name, _localizationService))
            {
                selectedGroup?.Players.Remove(selectedPlayer);
                _updatePlayersView();
                _onDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Fehler beim Entfernen des Spielers: {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    public void GenerateMatchesButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedGroup = _getSelectedGroup();
        
        // ✅ NEU: Prüfe ob Gruppenphase übersprungen werden soll
        if (_tournamentClass.GameRules.SkipGroupPhase)
        {
            GenerateDirectKnockoutMatches();
            return;
        }
        
        // Normale Gruppenphase-Generierung
        if (selectedGroup == null) return;

        try
        {
            var validation = TournamentValidationHelper.ValidateMatchGeneration(selectedGroup, _localizationService);
            if (!validation.CanGenerate)
            {
                TournamentDialogHelper.ShowWarning(validation.ErrorMessage!, null, _localizationService);
                return;
            }

            selectedGroup.GenerateRoundRobinMatches();
            _updateMatchesView();
            _updatePlayersView();
            _onDataChanged();

            var successMessage = _localizationService.GetString("MatchesGeneratedSuccess") ?? "Spiele wurden erfolgreich erstellt!";
            TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService);
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService.GetString("ErrorGeneratingMatches") ?? "Fehler beim Erstellen der Spiele:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }
    
    /// <summary>
    /// ✅ NEU: Generiert direkt KO-Phase ohne Gruppenphase
    /// Sammelt alle Spieler aus allen Gruppen und erstellt das Bracket
    /// </summary>
    private void GenerateDirectKnockoutMatches()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("GenerateDirectKnockoutMatches: START");
            
            // Sammle alle Spieler aus allen Gruppen
            var allPlayers = _tournamentClass.Groups
                .SelectMany(g => g.Players)
                .ToList();
            
            System.Diagnostics.Debug.WriteLine($"GenerateDirectKnockoutMatches: Found {allPlayers.Count} players");
            
            // Validierung
            if (allPlayers.Count < 2)
            {
                var message = _localizationService.GetString("NotEnoughPlayersForKnockout") 
                    ?? "Es werden mindestens 2 Spieler für die KO-Phase benötigt!";
                TournamentDialogHelper.ShowWarning(message, null, _localizationService);
                return;
            }
            
            // Bestätigung vom Benutzer
            var knockoutMode = _tournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination 
                ? (_localizationService.GetString("DoubleElimination") ?? "Doppeltes K.O.")
                : (_localizationService.GetString("SingleElimination") ?? "Einfaches K.O.");
            
            // ✅ NEU: Verwende benutzerdefinierten Dialog statt MessageBox
            var dialog = new GenerateKnockoutDialog(allPlayers.Count, knockoutMode, _localizationService);
            dialog.Owner = _getWindow();
            
            var result = dialog.ShowDialog();
            
            if (result != true)
            {
                System.Diagnostics.Debug.WriteLine("GenerateDirectKnockoutMatches: User cancelled");
                return;
            }
            
            // Erstelle direkte KO-Phase
            var knockoutPhase = _tournamentClass.GetPhaseManager().CreateDirectKnockoutPhase(allPlayers);
            
            // ✅ WICHTIG: Phases und CurrentPhase korrekt setzen
            _tournamentClass.Phases.Clear();
            _tournamentClass.Phases.Add(knockoutPhase);
            _tournamentClass.CurrentPhase = knockoutPhase;
            knockoutPhase.IsActive = true;
            knockoutPhase.IsCompleted = false;
            
            System.Diagnostics.Debug.WriteLine($"GenerateDirectKnockoutMatches: Created KO phase with {knockoutPhase.WinnerBracket.Count} winner matches");
            System.Diagnostics.Debug.WriteLine($"GenerateDirectKnockoutMatches: Phase is now current: {_tournamentClass.CurrentPhase?.PhaseType}");
            
            // ✅ WICHTIG: Erst Daten ändern markieren
            _onDataChanged();
            
            // ✅ WICHTIG: UI über Dispatcher aktualisieren um Race Conditions zu vermeiden
            var mainWindow = _getWindow();
            if (mainWindow != null)
            {
                mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("GenerateDirectKnockoutMatches: Starting UI updates...");
                        
                        // 1. Wechsle zum KO-Tab über Callback
                        System.Diagnostics.Debug.WriteLine("GenerateDirectKnockoutMatches: Calling _switchToPhaseTab...");
                        _switchToPhaseTab(TournamentPhaseType.KnockoutPhase);
                        
                        // 2. Warte kurz damit Tab-Wechsel abgeschlossen ist
                        System.Threading.Thread.Sleep(200);
                        
                        // 3. Refreshe KO View
                        System.Diagnostics.Debug.WriteLine("GenerateDirectKnockoutMatches: Calling _refreshKnockoutView...");
                        _refreshKnockoutView();
                        
                        // 4. Update andere Views
                        _updatePlayersView();
                        _updateMatchesView();
                        
                        // 5. Force nochmal ein KO View Refresh nach weiterer kurzer Wartezeit
                        System.Threading.Thread.Sleep(100);
                        _refreshKnockoutView();
                        
                        System.Diagnostics.Debug.WriteLine("GenerateDirectKnockoutMatches: UI updates complete");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"GenerateDirectKnockoutMatches: UI update error: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
            
            var successMessage = string.Format(
                _localizationService.GetString("DirectKnockoutGeneratedSuccess") 
                    ?? "KO-Phase wurde erfolgreich erstellt!\n\n" +
                       "• {0} Matches im Winner Bracket\n" +
                       "• Modus: {1}",
                knockoutPhase.WinnerBracket.Count,
                knockoutMode
            );
            
            // Erfolgsmeldung auch über Dispatcher
            if (mainWindow != null)
            {
                mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService);
                }), System.Windows.Threading.DispatcherPriority.Normal);
            }
            
            System.Diagnostics.Debug.WriteLine("GenerateDirectKnockoutMatches: END - Success");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateDirectKnockoutMatches: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            var errorMessage = $"{_localizationService.GetString("ErrorGeneratingKnockout") ?? "Fehler beim Erstellen der KO-Phase:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    public void ResetMatchesButton_Click(object sender, RoutedEventArgs e)
    {
// ✅ GEÄNDERT: Kontextabhängiger Reset basierend auf aktiver Phase
        try
        {
   // Bestimme die Phase und erstelle passende Bestätigungsnachricht
  string phaseDescription = _tournamentClass.CurrentPhase?.PhaseType switch
     {
  TournamentPhaseType.KnockoutPhase => _localizationService.GetString("KnockoutPhase") ?? "K.O.-Phase",
        TournamentPhaseType.RoundRobinFinals => _localizationService.GetString("Finals") ?? "Finalrunde",
  _ => _localizationService.GetString("GroupPhase") ?? "Gruppenphase"
            };
   
         var confirmMessage = string.Format(
   _localizationService.GetString("ConfirmResetMatchResults") ?? "Möchten Sie alle Match-Ergebnisse in der {0} zurücksetzen?\n\n⚠ Nur die Ergebnisse werden gelöscht, die Matches bleiben erhalten!",
     phaseDescription
   );
       
     var title = _localizationService.GetString("ResetMatches") ?? "Match-Ergebnisse zurücksetzen";
   
    // Verwende den modernen Bestätigungsdialog direkt
  var dialog = new Window
      {
  Title = title,
           Width = 450,
       Height = 280,
  WindowStartupLocation = WindowStartupLocation.CenterOwner,
          ResizeMode = ResizeMode.NoResize,
      WindowStyle = WindowStyle.None,
  AllowsTransparency = true,
   Background = System.Windows.Media.Brushes.Transparent,
   ShowInTaskbar = false
     };

   if (_getWindow() != null)
      {
  dialog.Owner = _getWindow();
        }

  // Einfacher Bestätigungsdialog - Hole Bestätigung
    var result = MessageBox.Show(
    confirmMessage,
      title,
 MessageBoxButton.YesNo,
           MessageBoxImage.Warning,
         MessageBoxResult.No
        );
    
      if (result == MessageBoxResult.Yes)
      {
 System.Diagnostics.Debug.WriteLine($"ResetMatchesButton_Click: Resetting matches for phase {_tournamentClass.CurrentPhase?.PhaseType}");
       
      // Verwende die neue kontextabhängige Reset-Methode
 _tournamentClass.ResetCurrentPhaseMatchResults();
      
        // UI aktualisieren
   _updateMatchesView();
    
       // Spezifische View-Updates je nach Phase
    if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
       {
          _refreshKnockoutView();
      }
  else if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
     {
  _refreshFinalsView();
 }
  
    _onDataChanged();

     var successMessage = string.Format(
       _localizationService.GetString("MatchResultsResetSuccess") ?? "Match-Ergebnisse in der {0} wurden zurückgesetzt!",
        phaseDescription
    );
          TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService);
    }
        }
   catch (Exception ex)
        {
     var errorMessage = $"Fehler beim Zurücksetzen der Match-Ergebnisse: {ex.Message}";
 TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
   }
    }

    public void AdvanceToNextPhaseButton_Click(object sender, RoutedEventArgs e, Action<TournamentPhaseType> switchToTab)
    {
        try
        {
            var validation = TournamentValidationHelper.ValidatePhaseAdvancement(_tournamentClass, _localizationService);
            if (!validation.CanAdvance)
            {
                TournamentDialogHelper.ShowWarning(validation.ErrorMessage!, null, _localizationService);
                return;
            }

            _tournamentClass.AdvanceToNextPhase();
            
            // Switch to appropriate tab
            if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                switchToTab(TournamentPhaseType.RoundRobinFinals);
            }
            else if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                switchToTab(TournamentPhaseType.KnockoutPhase);
            }
            
            _onDataChanged();
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService.GetString("ErrorAdvancingPhase") ?? "Fehler beim Wechsel in die nächste Phase:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    public void ResetTournamentButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (TournamentDialogHelper.ShowResetTournamentConfirmation(_getWindow(), _localizationService))
            {
                TournamentKnockoutHelper.ResetToGroupPhase(_tournamentClass);
                
                // ✅ NEU: Explizit alle UI-Views aktualisieren nach dem Reset
                _updatePlayersView();
                _updateMatchesView();
                
                var successMessage = _localizationService.GetString("TournamentResetComplete") ?? "Das Turnier wurde erfolgreich zurückgesetzt.";
                TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService);
                
                _onDataChanged();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen des Turniers:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    public async Task HandleMatchDoubleClick(Match match, string matchType)
    {
        // ✅ FIXED: HubService UND Tournament-ID holen über TournamentManagementService
        var hubService = _getHubService();
        string? tournamentId = null;
        
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] HandleMatchDoubleClick called");
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] Match Type: {matchType}");
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] HubService available: {hubService != null}");
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] HubService registered: {hubService?.IsRegisteredWithHub}");
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] Match UUID: {match.UniqueId}");
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] Match ID: {match.Id}");
        
        try
        {
            // ⭐ KORRIGIERT: Hole Tournament-ID über TournamentManagementService
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var tournamentServiceField = mainWindow.GetType()
                    .GetField("_tournamentService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (tournamentServiceField?.GetValue(mainWindow) is TournamentManagementService tournamentService)
                {
                    var tournamentData = tournamentService.GetTournamentData();
                    tournamentId = tournamentData?.TournamentId;
                    
                    System.Diagnostics.Debug.WriteLine($"🎯 [EventHandlers-HandleMatchDoubleClick] Tournament ID from TournamentService: {tournamentId ?? "null"}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ [EventHandlers-HandleMatchDoubleClick] Could not get TournamentManagementService");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ [EventHandlers-HandleMatchDoubleClick] Could not get Tournament ID: {ex.Message}");
        }
        
        // ✅ NEU: Für Finals-Matches verwende rundenspezifische Regeln
        MatchResultWindow resultWindow;
        
        if (matchType == "Finals")
        {
            // Verwende Round Robin Finals Regeln
            var finalsRules = _tournamentClass.GameRules.GetRulesForRoundRobinFinals(RoundRobinFinalsRound.Finals);
            
            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] Creating MatchResultWindow for Finals Match {match.Id}");
            System.Diagnostics.Debug.WriteLine($"   📊 Finals Rules: SetsToWin={finalsRules.SetsToWin}, LegsToWin={finalsRules.LegsToWin}, LegsPerSet={finalsRules.LegsPerSet}");
            System.Diagnostics.Debug.WriteLine($"   📊 Base GameRules: SetsToWin={_tournamentClass.GameRules.SetsToWin}, LegsToWin={_tournamentClass.GameRules.LegsToWin}");
            
            // Erstelle temporäre GameRules mit Finals-spezifischen Werten
            var finalsGameRules = new GameRules
            {
                GameMode = _tournamentClass.GameRules.GameMode,
                FinishMode = _tournamentClass.GameRules.FinishMode,
                PlayWithSets = finalsRules.SetsToWin > 0,
                SetsToWin = finalsRules.SetsToWin,
                LegsToWin = finalsRules.LegsToWin,
                LegsPerSet = finalsRules.LegsPerSet
            };
            
            resultWindow = new MatchResultWindow(match, finalsGameRules, _localizationService, hubService, tournamentId);
        }
        else
        {
            // Für Gruppenphase-Matches verwende Standard-Regeln
            resultWindow = new MatchResultWindow(match, _tournamentClass.GameRules, _localizationService, hubService, tournamentId);
        }
        
        resultWindow.Owner = _getWindow();
        
        if (resultWindow.ShowDialog() == true)
        {
            match.ForcePropertyChanged(nameof(match.ScoreDisplay));
            match.ForcePropertyChanged(nameof(match.StatusDisplay));
            match.ForcePropertyChanged(nameof(match.WinnerDisplay));
            
            // Send to hub if needed
            await SendMatchResultToHub(match, _tournamentClass, matchType);
            
            if (matchType == "Finals")
            {
                _refreshFinalsView();
                // Auto-check finals completion
                var parentWindow = _getWindow();
                Task.Run(() => TournamentValidationHelper.CheckFinalsCompletion(_tournamentClass, parentWindow, _localizationService));
            }
            else
            {
                _updateMatchesView();
                // Auto-check group completion
                var parentWindow = _getWindow();
                Task.Run(() => TournamentValidationHelper.CheckAllGroupsCompletion(_tournamentClass, parentWindow, _localizationService));
            }
            
            _onDataChanged();
        }
    }

    public async Task HandleKnockoutMatchDoubleClick(KnockoutMatch match, string bracketType)
    {
  // ✅ FIXED: HubService UND Tournament-ID holen über TournamentManagementService
        var hubService = _getHubService();
        string? tournamentId = null;
   
  System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] HandleKnockoutMatchDoubleClick called");
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] HubService available: {hubService != null}");
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] HubService registered: {hubService?.IsRegisteredWithHub}");
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] Match UUID: {match.UniqueId}");
  System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] Match ID: {match.Id}");
     System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] Bracket Type: {bracketType}");
     
    try
        {
     // ⭐ KORRIGIERT: Hole Tournament-ID über TournamentManagementService
            if (Application.Current.MainWindow is MainWindow mainWindow)
 {
     var tournamentServiceField = mainWindow.GetType()
    .GetField("_tournamentService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      
if (tournamentServiceField?.GetValue(mainWindow) is TournamentManagementService tournamentService)
   {
    var tournamentData = tournamentService.GetTournamentData();
       tournamentId = tournamentData?.TournamentId;
    
    System.Diagnostics.Debug.WriteLine($"🎯 [EventHandlers-HandleKnockoutMatch] Tournament ID from TournamentService: {tournamentId ?? "null"}");
    }
      else
      {
          System.Diagnostics.Debug.WriteLine($"⚠️ [EventHandlers-HandleKnockoutMatch] Could not get TournamentManagementService");
}
   }
  }
        catch (Exception ex)
   {
 System.Diagnostics.Debug.WriteLine($"⚠️ [EventHandlers-HandleKnockoutMatch] Could not get Tournament ID: {ex.Message}");
     }
   
        // ✅ IMPROVED: Detaillierte Prüfung der HubService-Kette
        if (hubService == null)
        {
System.Diagnostics.Debug.WriteLine($"❌ [TournamentTabEventHandlers] HubService is null - trying to debug the getHubService callback");
    
        try
 {
      var testHub = _getHubService();
           System.Diagnostics.Debug.WriteLine($"❌ [TournamentTabEventHandlers] getHubService() second call result: {testHub != null}");
       }
            catch (Exception ex)
    {
 System.Diagnostics.Debug.WriteLine($"❌ [TournamentTabEventHandlers] getHubService() callback failed: {ex.Message}");
      }
  }

      // ✅ SIMPLIFIED: Verwende die bewährte Konvertierungs-Methode aber mit korrektem HubService
        // ✅ FIXED: Verwende den KnockoutMatch-Constructor mit rundenspezifischen RoundRules!
        var roundRules = _tournamentClass.GameRules.GetRulesForRound(match.Round);
        
  System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] Creating MatchResultWindow for KnockoutMatch {match.Id}");
    System.Diagnostics.Debug.WriteLine($"   📊 Match Round: {match.Round}");
  System.Diagnostics.Debug.WriteLine($"   📊 Round Rules: SetsToWin={roundRules.SetsToWin}, LegsToWin={roundRules.LegsToWin}, LegsPerSet={roundRules.LegsPerSet}");
        System.Diagnostics.Debug.WriteLine($"   📊 Base GameRules: SetsToWin={_tournamentClass.GameRules.SetsToWin}, LegsToWin={_tournamentClass.GameRules.LegsToWin}");
        System.Diagnostics.Debug.WriteLine($"   🌐 HubService: {hubService != null}, TournamentId: {tournamentId ?? "null"}");
    System.Diagnostics.Debug.WriteLine($"   🆔 Match UUID: {match.UniqueId}");
        
   // ✅ WICHTIG: Verwende den speziellen KnockoutMatch-Constructor mit RoundRules!
  var resultWindow = new MatchResultWindow(match, roundRules, _tournamentClass.GameRules, _localizationService, hubService, tournamentId);
        resultWindow.Owner = _getWindow();

        if (resultWindow.ShowDialog() == true)
  {
 // Kopiere Ergebnisse zurück zum KnockoutMatch
  var internalMatch = resultWindow.InternalMatch;
 match.Player1Sets = internalMatch.Player1Sets;
     match.Player2Sets = internalMatch.Player2Sets;
  match.Player1Legs = internalMatch.Player1Legs;
     match.Player2Legs = internalMatch.Player2Legs;
   match.Status = internalMatch.Status;
         match.Winner = internalMatch.Winner;
    match.Notes = internalMatch.Notes;
       match.EndTime = internalMatch.EndTime;
  
 // ✅ WICHTIG: Setze auch den Verlierer!
            if (match.Winner != null)
            {
                if (match.Player1 == match.Winner)
                {
                    match.Loser = match.Player2;
                }
                else if (match.Player2 == match.Winner)
                {
                    match.Loser = match.Player1;
                }
                
                System.Diagnostics.Debug.WriteLine($"🎯 Match {match.Id}: Winner = {match.Winner.Name}, Loser = {match.Loser?.Name ?? "none"}");
            }
            
            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] KnockoutMatch {match.Id} result copied back from MatchResultWindow");
            
            // ✅ WICHTIG: Propagiere Gewinner/Verlierer in nächste Runde!
            PropagateKnockoutMatchResult(match);
            
            // ✅ NEU: Prüfe und handle automatische Byes nach der Propagierung!
            if (_tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                // Erstelle temporären ByeMatchManager für Bye-Prüfung
                var byeManager = new ByeMatchManager(_tournamentClass);
                byeManager.CheckAndHandleAutomaticByes(
                    _tournamentClass.CurrentPhase.WinnerBracket, 
                    _tournamentClass.CurrentPhase.LoserBracket);
                
                if (_tournamentClass.CurrentPhase.LoserBracket != null)
                {
                    byeManager.CheckAndHandleAutomaticByes(
                        _tournamentClass.CurrentPhase.LoserBracket, 
                        _tournamentClass.CurrentPhase.WinnerBracket);
                }
                
                System.Diagnostics.Debug.WriteLine("✅ Checked for automatic byes after match result");
            }
            
            await SendKnockoutMatchResultToHub(match, _tournamentClass, bracketType);
            _refreshKnockoutView();
            _onDataChanged();
        }
        else
        {
   System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] MatchResultWindow was cancelled for KnockoutMatch {match.Id}");
        }
  }
    /// <summary>
    /// ✅ NEU: Propagiert Gewinner und Verlierer eines KnockoutMatches in die nächsten Runden
    /// ✅ ERWEITERT: Unterstützt auch Bye-Matches
    /// ✅ ERWEITERT: Behandelt Grand Final speziell (LF Winner → Grand Final)
    /// </summary>
    private void PropagateKnockoutMatchResult(KnockoutMatch match)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔄 [PropagateKnockoutMatchResult] Starting for match {match.Id}, Winner: {match.Winner?.Name}, Loser: {match.Loser?.Name}, Status: {match.Status}, Round: {match.Round}");
            
            var currentPhase = _tournamentClass.CurrentPhase;
            if (currentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine("❌ Not in KnockoutPhase!");
                return;
            }
            
            // ✅ WICHTIG: Für Bye-Matches müssen wir auch propagieren!
            // Bei Bye hat der Gewinner bereits gewonnen ohne zu spielen
            if (match.Status == MatchStatus.Bye && match.Winner == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Bye match but no winner set yet - skipping propagation");
                return;
            }
            
            // ✅ SPEZIALFALL: Loser Final Winner geht ins Grand Final!
            if (match.Round == KnockoutRound.LoserFinal && match.Winner != null)
            {
                System.Diagnostics.Debug.WriteLine($"🏆 [SPECIAL] Loser Final Winner {match.Winner.Name} should go to Grand Final!");
                
                // Finde das Grand Final (immer im Winner Bracket)
                var grandFinal = currentPhase.WinnerBracket.FirstOrDefault(m => m.Round == KnockoutRound.GrandFinal);
                if (grandFinal != null)
                {
                    // Loser Final Winner ist immer Player2 im Grand Final (kommt vom Loser Bracket)
                    // Winner Bracket Final Winner ist Player1
                    if (grandFinal.SourceMatch2 == match || grandFinal.Player2 == null)
                    {
                        grandFinal.Player2 = match.Winner;
                        System.Diagnostics.Debug.WriteLine($"✅✅✅ Set {match.Winner.Name} as Player2 in Grand Final (from Loser Final)");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Grand Final not found!");
                }
            }
            
            // Finde alle Matches die von diesem Match abhängen
            // Winner Bracket: Suche nach Matches die SourceMatch1 oder SourceMatch2 = match haben
            foreach (var nextMatch in currentPhase.WinnerBracket)
            {
                bool updated = false;
                
                if (nextMatch.SourceMatch1 == match)
                {
                    // Setze Player1 des nächsten Matches
                    if (nextMatch.Player1FromWinner && match.Winner != null)
                    {
                        nextMatch.Player1 = match.Winner;
                        updated = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Set Player1 of match {nextMatch.Id} to {match.Winner.Name} (from Winner)");
                    }
                    else if (!nextMatch.Player1FromWinner && match.Loser != null)
                    {
                        nextMatch.Player1 = match.Loser;
                        updated = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Set Player1 of match {nextMatch.Id} to {match.Loser.Name} (from Loser)");
                    }
                }
                
                if (nextMatch.SourceMatch2 == match)
                {
                    // Setze Player2 des nächsten Matches
                    if (nextMatch.Player2FromWinner && match.Winner != null)
                    {
                        nextMatch.Player2 = match.Winner;
                        updated = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Set Player2 of match {nextMatch.Id} to {match.Winner.Name} (from Winner)");
                    }
                    else if (!nextMatch.Player2FromWinner && match.Loser != null)
                    {
                        nextMatch.Player2 = match.Loser;
                        updated = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Set Player2 of match {nextMatch.Id} to {match.Loser.Name} (from Loser)");
                    }
                }
                
                if (updated)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 Updated match {nextMatch.Id}: {nextMatch.Player1?.Name ?? "TBD"} vs {nextMatch.Player2?.Name ?? "TBD"}");
                }
            }
            
            // Loser Bracket: Suche nach Matches die SourceMatch1 oder SourceMatch2 = match haben
            if (currentPhase.LoserBracket != null)
            {
                foreach (var nextMatch in currentPhase.LoserBracket)
                {
                    bool updated = false;
                    
                    if (nextMatch.SourceMatch1 == match)
                    {
                        // Setze Player1 des nächsten Matches
                        if (nextMatch.Player1FromWinner && match.Winner != null)
                        {
                            nextMatch.Player1 = match.Winner;
                            updated = true;
                            System.Diagnostics.Debug.WriteLine($"✅ Set Player1 of LB match {nextMatch.Id} to {match.Winner.Name} (from Winner)");
                        }
                        else if (!nextMatch.Player1FromWinner && match.Loser != null)
                        {
                            nextMatch.Player1 = match.Loser;
                            updated = true;
                            System.Diagnostics.Debug.WriteLine($"✅ Set Player1 of LB match {nextMatch.Id} to {match.Loser.Name} (from Loser)");
                        }
                    }
                    
                    if (nextMatch.SourceMatch2 == match)
                    {
                        // Setze Player2 des nächsten Matches
                        if (nextMatch.Player2FromWinner && match.Winner != null)
                        {
                            nextMatch.Player2 = match.Winner;
                            updated = true;
                            System.Diagnostics.Debug.WriteLine($"✅ Set Player2 of LB match {nextMatch.Id} to {match.Winner.Name} (from Winner)");
                        }
                        else if (!nextMatch.Player2FromWinner && match.Loser != null)
                        {
                            nextMatch.Player2 = match.Loser;
                            updated = true;
                            System.Diagnostics.Debug.WriteLine($"✅ Set Player2 of LB match {nextMatch.Id} to {match.Loser.Name} (from Loser)");
                        }
                    }
                    
                    if (updated)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔄 Updated LB match {nextMatch.Id}: {nextMatch.Player1?.Name ?? "TBD"} vs {nextMatch.Player2?.Name ?? "TBD"}");
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"✅ [PropagateKnockoutMatchResult] Completed for match {match.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [PropagateKnockoutMatchResult] ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    // Vereinfachte RoundRules Klasse für interne Verwendung
    private class SimpleRoundRules
    {
        public int SetsToWin { get; set; }
        public int LegsToWin { get; set; }
        public int LegsPerSet { get; set; }
    }

    // Helper methods
    private int GetNextGroupId()
    {
        return _tournamentClass.Groups.Count > 0 ? _tournamentClass.Groups.Max(g => g.Id) + 1 : 1;
    }

    private int GetNextPlayerId()
    {
        var allPlayers = _tournamentClass.Groups.SelectMany(g => g.Players);
        return allPlayers.Any() ? allPlayers.Max(p => p.Id) + 1 : 1;
    }

    private async Task SendMatchResultToHub(Match match, TournamentClass tournamentClass, string matchType)
    {
        try
        {
            // Hub integration logic here - placeholder for now
            System.Diagnostics.Debug.WriteLine($"🚀 [HUB_SEND] Sending {matchType} match result to Hub: {match.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [HUB_SEND] Error: {ex.Message}");
        }
    }

    private async Task SendKnockoutMatchResultToHub(KnockoutMatch match, TournamentClass tournamentClass, string bracketType)
    {
        try
        {
            // Hub integration logic here - placeholder for now
            System.Diagnostics.Debug.WriteLine($"🚀 [HUB_SEND] Sending {bracketType} knockout match result to Hub: {match.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [HUB_SEND] Error: {ex.Message}");
        }
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
        if (disposing)
        {
            // Hier könnten Event-Subscriptions abgemeldet werden, falls vorhanden
            // Derzeit keine expliziten Events zu cleanen
            System.Diagnostics.Debug.WriteLine($"[TournamentTabEventHandlers] Disposed successfully");
        }
    }
}