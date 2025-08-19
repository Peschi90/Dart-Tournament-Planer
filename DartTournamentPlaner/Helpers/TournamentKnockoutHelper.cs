using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für Knockout-spezifische Operationen im TournamentTab
/// Enthält Methoden für Freilos-Vergabe und Match-Result-Handling
/// </summary>
public static class TournamentKnockoutHelper
{
    /// <summary>
    /// Behandelt die Vergabe eines manuellen Freiloses
    /// </summary>
    /// <param name="tournamentClass">Die TournamentClass</param>
    /// <param name="match">Das Match für das Freilos</param>
    /// <param name="byeWinner">Der Spieler der das Freilos bekommen soll (null = automatisch bestimmen)</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn erfolgreich, sonst False</returns>
    public static bool HandleKnockoutBye(
        TournamentClass tournamentClass, 
        KnockoutMatch match, 
        Player? byeWinner = null,
        LocalizationService? localizationService = null)
    {
        try
        {
            bool success = tournamentClass.GiveManualBye(match, byeWinner);
            
            if (success)
            {
                var winnerName = byeWinner?.Name ?? "automatic winner";
                System.Diagnostics.Debug.WriteLine($"Successfully gave bye to {winnerName} in match {match.Id}");
            }
            else
            {
                var errorMessage = localizationService?.GetString("Error") ?? "Fehler";
                System.Windows.MessageBox.Show("Freilos konnte nicht vergeben werden.", errorMessage, 
                              System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }

            return success;
        }
        catch (Exception ex)
        {
            var errorTitle = localizationService?.GetString("Error") ?? "Fehler";
            System.Windows.MessageBox.Show($"Fehler beim Vergeben des Freiloses: {ex.Message}", errorTitle, 
                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// Behandelt das Rückgängigmachen eines Freiloses
    /// </summary>
    /// <param name="tournamentClass">Die TournamentClass</param>
    /// <param name="match">Das Match dessen Freilos rückgängig gemacht werden soll</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn erfolgreich, sonst False</returns>
    public static bool HandleUndoKnockoutBye(
        TournamentClass tournamentClass, 
        KnockoutMatch match,
        LocalizationService? localizationService = null)
    {
        try
        {
            bool success = tournamentClass.UndoBye(match);
            
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"Successfully undid bye for match {match.Id}");
            }
            else
            {
                var errorMessage = localizationService?.GetString("Error") ?? "Fehler";
                System.Windows.MessageBox.Show("Freilos konnte nicht rückgängig gemacht werden.", errorMessage, 
                              System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }

            return success;
        }
        catch (Exception ex)
        {
            var errorTitle = localizationService?.GetString("Error") ?? "Fehler";
            System.Windows.MessageBox.Show($"Fehler beim Rückgängigmachen des Freiloses: {ex.Message}", errorTitle, 
                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// Öffnet das Match-Result-Fenster für ein Knockout-Match
    /// </summary>
    /// <param name="knockoutMatch">Das Knockout-Match</param>
    /// <param name="gameRules">Die Spielregeln</param>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn das Ergebnis gespeichert wurde, sonst False</returns>
    public static bool OpenMatchResultWindow(
        KnockoutMatch knockoutMatch,
        GameRules gameRules,
        System.Windows.Window? owner = null,
        LocalizationService? localizationService = null)
    {
        if (localizationService == null) return false;

        try
        {
            // Erstelle dummy RoundRules basierend auf GameRules
            var roundRules = new RoundRules
            {
                LegsToWin = gameRules.LegsToWin,
                SetsToWin = gameRules.PlayWithSets ? gameRules.SetsToWin : 0,
                LegsPerSet = gameRules.LegsPerSet
            };
            
            var resultWindow = new MatchResultWindow(knockoutMatch, roundRules, gameRules, localizationService);
            if (owner != null)
            {
                resultWindow.Owner = owner;
            }
            
            return resultWindow.ShowDialog() == true;
        }
        catch (Exception ex)
        {
            var errorTitle = localizationService?.GetString("Error") ?? "Fehler";
            System.Windows.MessageBox.Show($"Fehler beim Öffnen des Match-Ergebnisfensters: {ex.Message}", errorTitle, 
                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// Behandelt die Auswahl des Freilos-Gewinners zwischen zwei Spielern
    /// </summary>
    /// <param name="match">Das Match mit beiden verfügbaren Spielern</param>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>Der ausgewählte Spieler oder null wenn abgebrochen</returns>
    public static Player? SelectByeWinner(
        KnockoutMatch match,
        System.Windows.Window? owner = null,
        LocalizationService? localizationService = null)
    {
        if (match.Player1 == null || match.Player2 == null)
        {
            return match.Player1 ?? match.Player2; // Return the available player
        }

        var selectedPlayerIndex = TournamentDialogHelper.ShowPlayerSelectionDialog(
            owner, 
            match.Player1?.Name ?? "TBD", 
            match.Player2?.Name ?? "TBD", 
            localizationService);

        return selectedPlayerIndex switch
        {
            1 => match.Player1,
            2 => match.Player2,
            _ => null
        };
    }

    /// <summary>
    /// Behandelt die Freilos-Vergabe basierend auf der Anzahl verfügbarer Spieler
    /// </summary>
    /// <param name="tournamentClass">Die TournamentClass</param>
    /// <param name="match">Das Match</param>
    /// <param name="owner">Das übergeordnete Fenster</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>True wenn erfolgreich, sonst False</returns>
    public static bool ProcessByeSelection(
        TournamentClass tournamentClass,
        KnockoutMatch match,
        System.Windows.Window? owner = null,
        LocalizationService? localizationService = null)
    {
        Player? selectedWinner = null;

        // Determine bye winner based on available players
        if (match.Player1 != null && match.Player2 == null)
        {
            selectedWinner = match.Player1; // Automatic bye for single player
        }
        else if (match.Player1 == null && match.Player2 != null)
        {
            selectedWinner = match.Player2; // Automatic bye for single player
        }
        else if (match.Player1 != null && match.Player2 != null)
        {
            // Both players available - show selection dialog
            selectedWinner = SelectByeWinner(match, owner, localizationService);
            if (selectedWinner == null)
            {
                return false; // User cancelled selection
            }
        }
        // If both players are null, selectedWinner remains null (automatic bye)

        return HandleKnockoutBye(tournamentClass, match, selectedWinner, localizationService);
    }

    /// <summary>
    /// Validiert ob eine Freilos-Operation für ein Match möglich ist
    /// </summary>
    /// <param name="match">Das zu validierende Match</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>Tuple mit Validierungsergebnis und Fehlermeldung</returns>
    public static (bool IsValid, string? ErrorMessage) ValidateByeOperation(
        KnockoutMatch match,
        LocalizationService? localizationService = null)
    {
        if (match.Status == MatchStatus.Finished)
        {
            return (false, "Match ist bereits beendet.");
        }

        if (match.Status == MatchStatus.InProgress)
        {
            return (false, "Match ist bereits in Bearbeitung.");
        }

        if (match.Player1 == null && match.Player2 == null)
        {
            return (false, "Keine Spieler für Freilos verfügbar.");
        }

        return (true, null);
    }

    /// <summary>
    /// Ermittelt den UI-Status für Freilos-Buttons eines Matches
    /// </summary>
    /// <param name="match">Das Match</param>
    /// <returns>Status-Objekt mit Button-Zuständen</returns>
    public static MatchByeUIStatus GetMatchByeUIStatus(KnockoutMatch match)
    {
        var canGiveBye = match.Status == MatchStatus.NotStarted && 
                        (match.Player1 != null || match.Player2 != null);
        
        var canUndoBye = match.Status == MatchStatus.Bye;
        
        var hasAutomaticBye = match.Status == MatchStatus.Bye && 
                             (match.Player1 == null || match.Player2 == null);

        return new MatchByeUIStatus
        {
            CanGiveBye = canGiveBye,
            CanUndoBye = canUndoBye,
            HasAutomaticBye = hasAutomaticBye,
            ShowByeButton = canGiveBye,
            ShowUndoButton = canUndoBye
        };
    }

    /// <summary>
    /// Status-Klasse für UI-Buttons bei Freilos-Operationen
    /// </summary>
    public class MatchByeUIStatus
    {
        public bool CanGiveBye { get; set; }
        public bool CanUndoBye { get; set; }
        public bool HasAutomaticBye { get; set; }
        public bool ShowByeButton { get; set; }
        public bool ShowUndoButton { get; set; }
    }

    /// <summary>
    /// Zeichnet einen statischen Turnierbaum wenn die interaktive Version nicht verfügbar ist
    /// </summary>
    /// <param name="canvas">Canvas für die Zeichnung</param>
    /// <param name="matches">Die zu zeichnenden Matches</param>
    /// <param name="isLoserBracket">Ob es sich um das Loser Bracket handelt</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    public static void DrawStaticBracketTree(
        System.Windows.Controls.Canvas canvas, 
        List<KnockoutMatch> matches,
        bool isLoserBracket,
        LocalizationService? localizationService = null)
    {
        canvas.Background = System.Windows.Media.Brushes.White;

        if (matches.Count == 0)
        {
            var message = isLoserBracket 
                ? localizationService?.GetString("NoLoserBracketGames") ?? "Keine Loser Bracket Spiele vorhanden"
                : localizationService?.GetString("NoWinnerBracketGames") ?? "Keine Winner Bracket Spiele vorhanden";
            
            TournamentUIHelper.DrawEmptyBracketMessage(canvas, message, isLoserBracket, localizationService);
            return;
        }

        // Add title
        var titleText = new System.Windows.Controls.TextBlock
        {
            Text = isLoserBracket ? "🥈 " + (localizationService?.GetString("LoserBracketTree") ?? "Loser Bracket")
                                  : "🏆 " + (localizationService?.GetString("WinnerBracketTree") ?? "Winner Bracket"),
            FontSize = 24,
            FontWeight = System.Windows.FontWeights.Bold,
            Foreground = isLoserBracket 
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 92, 92))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 139, 34)),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        
        System.Windows.Controls.Canvas.SetLeft(titleText, 20);
        System.Windows.Controls.Canvas.SetTop(titleText, 10);
        canvas.Children.Add(titleText);

        // Simple message for now
        var infoText = new System.Windows.Controls.TextBlock
        {
            Text = localizationService?.GetString("InteractiveTournamentTree") ?? "Interaktiver Turnierbaum wird über TournamentClass erstellt",
            FontSize = 14,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.DarkGray,
            Margin = new System.Windows.Thickness(0, 50, 0, 0)
        };
        
        System.Windows.Controls.Canvas.SetLeft(infoText, 200);
        System.Windows.Controls.Canvas.SetTop(infoText, 60);
        canvas.Children.Add(infoText);

        // Adjust canvas size
        canvas.Width = Math.Max(1000, 800);
        canvas.Height = Math.Max(700, 600);
        canvas.MinWidth = canvas.Width;
        canvas.MinHeight = canvas.Height;
    }

    /// <summary>
    /// Setzt ein Turnier auf die Gruppenphase zurück
    /// </summary>
    /// <param name="tournamentClass">Die zurückzusetzende TournamentClass</param>
    public static void ResetToGroupPhase(TournamentClass tournamentClass)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ResetToGroupPhase: Starting tournament reset");
            
            // 1. WICHTIG: Lösche alle fortgeschrittenen Phasen vollständig
            var phasesToRemove = tournamentClass.Phases
                .Where(p => p.PhaseType != TournamentPhaseType.GroupPhase)
                .ToList();
            
            System.Diagnostics.Debug.WriteLine($"ResetToGroupPhase: Removing {phasesToRemove.Count} non-group phases");
            foreach (var phase in phasesToRemove)
            {
                System.Diagnostics.Debug.WriteLine($"  - Removing phase: {phase.PhaseType}");
                
                // Explizit alle KO-Daten löschen
                if (phase.PhaseType == TournamentPhaseType.KnockoutPhase)
                {
                    if (phase.WinnerBracket != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"    - Clearing {phase.WinnerBracket.Count} Winner Bracket matches");
                        phase.WinnerBracket.Clear();
                    }
                    
                    if (phase.LoserBracket != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"    - Clearing {phase.LoserBracket.Count} Loser Bracket matches");
                        phase.LoserBracket.Clear();
                    }
                    
                    if (phase.QualifiedPlayers != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"    - Clearing {phase.QualifiedPlayers.Count} qualified players");
                        phase.QualifiedPlayers.Clear();
                    }
                }
                
                // Explizit alle Finals-Daten löschen
                if (phase.PhaseType == TournamentPhaseType.RoundRobinFinals)
                {
                    if (phase.FinalsGroup != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"    - Clearing finals group with {phase.FinalsGroup.Players.Count} players");
                        phase.FinalsGroup.Players.Clear();
                        phase.FinalsGroup.Matches.Clear();
                    }
                    
                    if (phase.QualifiedPlayers != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"    - Clearing {phase.QualifiedPlayers.Count} qualified players");
                        phase.QualifiedPlayers.Clear();
                    }
                }
                
                tournamentClass.Phases.Remove(phase);
            }
            
            // 2. Stelle sicher dass eine GroupPhase existiert
            var groupPhase = tournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
            if (groupPhase == null)
            {
                System.Diagnostics.Debug.WriteLine("ResetToGroupPhase: Creating new GroupPhase");
                groupPhase = new TournamentPhase
                {
                    PhaseType = TournamentPhaseType.GroupPhase,
                    Name = "Gruppenphase",
                    IsActive = true,
                    IsCompleted = false,
                    Groups = tournamentClass.Groups // Behalte bestehende Gruppen
                };
                tournamentClass.Phases.Add(groupPhase);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ResetToGroupPhase: Using existing GroupPhase");
                // Setze GroupPhase-Status zurück
                groupPhase.IsActive = true;
                groupPhase.IsCompleted = false;
                groupPhase.Groups = tournamentClass.Groups; // Stelle sicher dass Groups verlinkt sind
            }
            
            // 3. Reset zu group phase
            tournamentClass.CurrentPhase = groupPhase;
            
            // 4. WICHTIG: Trigge UI-Refresh um alle Views zu aktualisieren
            tournamentClass.TriggerUIRefresh();
            
            System.Diagnostics.Debug.WriteLine($"ResetToGroupPhase: Successfully reset tournament to group phase with {tournamentClass.Groups.Count} groups");
            System.Diagnostics.Debug.WriteLine($"ResetToGroupPhase: Total phases remaining: {tournamentClass.Phases.Count}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ResetToGroupPhase: ERROR: {ex.Message}");
            throw; // Re-throw to be handled by caller
        }
    }
}