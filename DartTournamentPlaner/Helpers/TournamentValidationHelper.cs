using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Controls;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für asynchrone Validierungs- und Status-Checking-Operationen im TournamentTab
/// Enthält Methoden zur Überprüfung von Turnierfortschritt und automatischen Benachrichtigungen
/// </summary>
public static class TournamentValidationHelper
{
    private static DateTime? _lastCompletionNotification;

    /// <summary>
    /// Überprüft asynchron ob alle Gruppen abgeschlossen sind und zeigt entsprechende Benachrichtigungen
    /// </summary>
    /// <param name="tournamentClass">Die zu überprüfende TournamentClass</param>
    /// <param name="parentWindow">Das übergeordnete Fenster für LoadingSpinner</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    public static async Task CheckAllGroupsCompletion(
        TournamentClass tournamentClass, 
        System.Windows.Window? parentWindow = null,
        LocalizationService? localizationService = null)
    {
        // LoadingSpinner anzeigen
        var mainGrid = parentWindow?.Content as System.Windows.Controls.Grid;
        LoadingSpinner? spinner = null;
        
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CheckAllGroupsCompletion START ===");
            
            // Zeige LoadingSpinner falls Main Grid verfügbar
            if (mainGrid != null)
            {
                var loadingText = localizationService?.GetString("CheckingGroupStatus") ?? "Überprüfe Gruppenstatus...";
                spinner = mainGrid.ShowLoadingSpinner(loadingText);
            }
            
            // Kleine Verzögerung damit der Spinner sichtbar wird
            await Task.Delay(100);
            
            var completedGroups = 0;
            var totalGroups = tournamentClass.Groups.Count;
            
            // Update progress
            if (spinner != null)
            {
                await spinner.Dispatcher.BeginInvoke(() =>
                {
                    var progressText = localizationService?.GetString("ProcessingMatches") ?? "Verarbeite Spiele...";
                    spinner.ProgressText = progressText;
                });
            }
            
            foreach (var group in tournamentClass.Groups)
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
                    var progressText = localizationService?.GetString("CheckingCompletion") ?? "Überprüfe Abschluss...";
                    spinner.ProgressText = progressText;
                });
            }
            
            await Task.Delay(100); // Kurze Verzögerung für visuelle Klarheit
            
            System.Diagnostics.Debug.WriteLine($"CheckAllGroupsCompletion: {completedGroups}/{totalGroups} groups completed");
            
            // NEU: Zeige Benachrichtigung wenn alle Gruppen abgeschlossen sind
            if (completedGroups == totalGroups && totalGroups > 0 && 
                tournamentClass.GameRules.PostGroupPhaseMode != PostGroupPhaseMode.None)
            {
                ShowGroupPhaseCompletionNotification(mainGrid, localizationService);
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
    /// Überprüft asynchron die Finals-Phase auf Vollständigkeit
    /// </summary>
    /// <param name="tournamentClass">Die zu überprüfende TournamentClass</param>
    /// <param name="parentWindow">Das übergeordnete Fenster für LoadingSpinner</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    public static async Task CheckFinalsCompletion(
        TournamentClass tournamentClass, 
        System.Windows.Window? parentWindow = null,
        LocalizationService? localizationService = null)
    {
        // LoadingSpinner anzeigen
        var mainGrid = parentWindow?.Content as System.Windows.Controls.Grid;
        LoadingSpinner? spinner = null;
        
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CheckFinalsCompletion START ===");
            
            if (tournamentClass?.CurrentPhase?.PhaseType != TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine("  Not in finals phase");
                return;
            }
            
            // Zeige LoadingSpinner falls Main Grid verfügbar
            if (mainGrid != null)
            {
                var loadingText = localizationService?.GetString("CheckingFinalsStatus") ?? "Überprüfe Finalrunden-Status...";
                spinner = mainGrid.ShowLoadingSpinner(loadingText);
            }
            
            // Kleine Verzögerung damit der Spinner sichtbar wird
            await Task.Delay(100);
            
            var finalsGroup = tournamentClass.CurrentPhase.FinalsGroup;
            if (finalsGroup == null)
            {
                return;
            }
            
            // Update progress
            if (spinner != null)
            {
                await spinner.Dispatcher.BeginInvoke(() =>
                {
                    var progressText = localizationService?.GetString("ProcessingFinalsMatches") ?? "Verarbeite Finals-Spiele...";
                    spinner.ProgressText = progressText;
                });
            }
            
            var status = finalsGroup.CheckCompletionStatus();
            
            // Auto-repair if needed
            if (!status.IsComplete && finalsGroup.MatchesGenerated)
            {
                finalsGroup.RepairMatchStatuses();
                await Task.Delay(100);
                status = finalsGroup.CheckCompletionStatus();
            }
            
            // Update progress
            if (spinner != null)
            {
                await spinner.Dispatcher.BeginInvoke(() =>
                {
                    var progressText = localizationService?.GetString("CheckingCompletion") ?? "Überprüfe Abschluss...";
                    spinner.ProgressText = progressText;
                });
            }
            
            await Task.Delay(100);
            
            // Show notification if finals are complete
            if (status.IsComplete)
            {
                ShowFinalsCompletionNotification(mainGrid, localizationService);
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

    /// <summary>
    /// Zeigt eine Benachrichtigung wenn alle Gruppen abgeschlossen sind
    /// </summary>
    /// <param name="parentGrid">Das übergeordnete Grid für Toast-Benachrichtigungen</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    private static void ShowGroupPhaseCompletionNotification(
        System.Windows.Controls.Grid? parentGrid, 
        LocalizationService? localizationService = null)
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
            
            var title = localizationService?.GetString("Information") ?? "Information";
            var message = localizationService?.GetString("AllGroupsCompleted") ?? 
                         "🎉 Alle Gruppen sind abgeschlossen!\n\nSie können jetzt zur nächsten Phase wechseln.";
            
            // Zeige Toast-Benachrichtigung
            if (parentGrid != null)
            {
                TournamentUIHelper.ShowToastNotification(parentGrid, title, message);
            }
            else
            {
                // Fallback to MessageBox
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            
            System.Diagnostics.Debug.WriteLine("  Completion notification shown");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowGroupPhaseCompletionNotification: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Zeigt eine Benachrichtigung wenn die Finals abgeschlossen sind
    /// </summary>
    /// <param name="parentGrid">Das übergeordnete Grid für Toast-Benachrichtigungen</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    private static void ShowFinalsCompletionNotification(
        System.Windows.Controls.Grid? parentGrid,
        LocalizationService? localizationService = null)
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
            
            var title = localizationService?.GetString("Information") ?? "Information";
            var message = localizationService?.GetString("FinalsCompleted") ?? 
                         "🏆 Die Finalrunde ist abgeschlossen!\n\nAlle Spiele wurden beendet. Das Turnier ist komplett!";
            
            // Zeige Toast-Benachrichtigung
            if (parentGrid != null)
            {
                TournamentUIHelper.ShowToastNotification(parentGrid, title, message);
            }
            else
            {
                // Fallback to MessageBox
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            
            System.Diagnostics.Debug.WriteLine("  Finals completion notification shown");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowFinalsCompletionNotification: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Validiert ob zur nächsten Phase gewechselt werden kann
    /// </summary>
    /// <param name="tournamentClass">Die zu validierende TournamentClass</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn Wechsel möglich, sonst False mit Fehlermeldung</returns>
    public static (bool CanAdvance, string? ErrorMessage) ValidatePhaseAdvancement(
        TournamentClass tournamentClass,
        LocalizationService? localizationService = null)
    {
        try
        {
            if (!tournamentClass.CanProceedToNextPhase())
            {
                var message = localizationService?.GetString("CannotAdvancePhase") ?? 
                             "Alle Spiele der aktuellen Phase müssen abgeschlossen sein";
                return (false, message);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            var errorMessage = localizationService?.GetString("ErrorAdvancingPhase") ?? 
                              $"Fehler beim Wechsel in die nächste Phase: {ex.Message}";
            return (false, errorMessage);
        }
    }

    /// <summary>
    /// Validiert ob Matches für eine Gruppe generiert werden können
    /// </summary>
    /// <param name="group">Die zu validierende Gruppe</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn Generation möglich, sonst False mit Fehlermeldung</returns>
    public static (bool CanGenerate, string? ErrorMessage) ValidateMatchGeneration(
        Group group,
        LocalizationService? localizationService = null)
    {
        if (group.Players.Count < 2)
        {
            var message = localizationService?.GetString("MinimumTwoPlayers") ?? 
                         "Mindestens 2 Spieler erforderlich.";
            return (false, message);
        }

        return (true, null);
    }

    /// <summary>
    /// Validiert Spieler-Input für das Hinzufügen eines neuen Spielers
    /// </summary>
    /// <param name="playerName">Der zu validierende Spielername</param>
    /// <param name="group">Die Gruppe in die der Spieler hinzugefügt werden soll</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn gültig, sonst False mit Fehlermeldung</returns>
    public static (bool IsValid, string? ErrorMessage) ValidatePlayerInput(
        string? playerName,
        Group? group,
        LocalizationService? localizationService = null)
    {
        if (group == null)
        {
            var message = localizationService?.GetString("SelectGroupFirst") ?? 
                         "Bitte wählen Sie zuerst eine Gruppe aus.";
            return (false, message);
        }

        if (string.IsNullOrWhiteSpace(playerName))
        {
            var message = localizationService?.GetString("EnterPlayerName") ?? 
                         "Bitte geben Sie einen Spielernamen ein.";
            return (false, message);
        }

        // Check for duplicate names within the group
        if (group.Players.Any(p => p.Name.Equals(playerName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return (false, "Ein Spieler mit diesem Namen existiert bereits in der Gruppe.");
        }

        return (true, null);
    }

    /// <summary>
    /// Validiert Gruppen-Input für das Hinzufügen einer neuen Gruppe
    /// </summary>
    /// <param name="groupName">Der zu validierende Gruppenname</param>
    /// <param name="existingGroups">Bereits existierende Gruppen</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn gültig, sonst False mit Fehlermeldung</returns>
    public static (bool IsValid, string? ErrorMessage) ValidateGroupInput(
        string? groupName,
        IEnumerable<Group> existingGroups,
        LocalizationService? localizationService = null)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            var message = localizationService?.GetString("EnterGroupName") ?? 
                         "Bitte geben Sie einen Gruppennamen ein.";
            return (false, message);
        }

        // Check for duplicate group names
        if (existingGroups.Any(g => g.Name.Equals(groupName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return (false, "Eine Gruppe mit diesem Namen existiert bereits.");
        }

        return (true, null);
    }
}