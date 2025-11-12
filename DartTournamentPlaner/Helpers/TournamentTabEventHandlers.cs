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
        Func<HubIntegrationService?> getHubService = null)
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
        if (match.IsBye) return;

    // ✅ FIXED: HubService UND Tournament-ID holen über TournamentManagementService
        var hubService = _getHubService();
        string? tournamentId = null;
    
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
   
 var resultWindow = new MatchResultWindow(match, _tournamentClass.GameRules, _localizationService, hubService, tournamentId);
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
  
 System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] KnockoutMatch {match.Id} result copied back from MatchResultWindow");
    
   await SendKnockoutMatchResultToHub(match, _tournamentClass, bracketType);
     _refreshKnockoutView();
       _onDataChanged();
      }
        else
        {
   System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTabEventHandlers] MatchResultWindow was cancelled for KnockoutMatch {match.Id}");
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