using System;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Helpers
{
    /// <summary>
    /// Helper-Klasse für Druckoperationen
    /// Vereinfacht die Integration des Drucksystems in die Hauptanwendung
    /// </summary>
    public static class PrintHelper
    {
        /// <summary>
        /// Öffnet den Druckdialog für eine Turnierklasse
        /// </summary>
        /// <param name="tournamentClass">Die zu druckende Turnierklasse</param>
        /// <param name="owner">Das Besitzerfenster für den Dialog</param>
        /// <param name="localizationService">Service für Übersetzungen</param>
        /// <returns>True wenn erfolgreich gedruckt wurde</returns>
        public static bool ShowPrintDialog(TournamentClass tournamentClass, Window? owner = null, LocalizationService? localizationService = null)
        {
            return ShowPrintDialog(new List<TournamentClass> { tournamentClass }, tournamentClass, owner, localizationService);
        }

        /// <summary>
        /// Öffnet den Druckdialog für eine oder mehrere Turnierklassen
        /// </summary>
        /// <param name="tournamentClasses">Die verfügbaren Turnierklassen</param>
        /// <param name="selectedTournamentClass">Die initial ausgewählte Turnierklasse</param>
        /// <param name="owner">Das Besitzerfenster für den Dialog</param>
        /// <param name="localizationService">Service für Übersetzungen</param>
        /// <returns>True wenn erfolgreich gedruckt wurde</returns>
        public static bool ShowPrintDialog(List<TournamentClass> tournamentClasses, TournamentClass? selectedTournamentClass = null, Window? owner = null, LocalizationService? localizationService = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ShowPrintDialog: Starting for {tournamentClasses?.Count ?? 0} tournament classes");
                
                if (tournamentClasses == null || !tournamentClasses.Any())
                {
                    System.Diagnostics.Debug.WriteLine("ShowPrintDialog: No tournament classes provided");
                    MessageBox.Show("Keine Turnierklassen zum Drucken verfügbar.", "Druckfehler", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                // Prüfe ob mindestens eine Klasse druckbare Inhalte hat
                var printableClasses = tournamentClasses.Where(HasPrintableContent).ToList();
                if (!printableClasses.Any())
                {
                    System.Diagnostics.Debug.WriteLine("ShowPrintDialog: No printable content in any tournament class");
                    var message = localizationService?.GetString("NoPrintableContentInAnyClass") ?? 
                                "Keine der Turnierklassen enthält druckbare Inhalte.\n\n" +
                                "Um Statistiken drucken zu können, müssen Sie:\n" +
                                "• Gruppen erstellen und Spieler hinzufügen\n" +
                                "• Spiele generieren und Ergebnisse eingeben\n" +
                                "• Oder das Turnier in weitere Phasen (Finals/KO) vorantreiben";
                    
                    MessageBox.Show(message, "Keine druckbaren Inhalte", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                // Wähle die ausgewählte Klasse oder die erste druckbare
                var initialSelection = selectedTournamentClass != null && HasPrintableContent(selectedTournamentClass) 
                    ? selectedTournamentClass 
                    : printableClasses.First();

                System.Diagnostics.Debug.WriteLine($"ShowPrintDialog: Opening print dialog with {printableClasses.Count} printable classes, initial selection: {initialSelection.Name}");

                // Öffne Druckdialog
                var printDialog = new TournamentPrintDialog(tournamentClasses, initialSelection, localizationService);
                
                if (owner != null)
                {
                    printDialog.Owner = owner;
                }

                var result = printDialog.ShowDialog();
                
                if (result == true && printDialog.PrintConfirmed)
                {
                    System.Diagnostics.Debug.WriteLine($"ShowPrintDialog: Print confirmed, executing print...");
                    // Führe den tatsächlichen Druckvorgang aus
                    return ExecutePrint(initialSelection, printDialog.PrintOptions, localizationService);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ShowPrintDialog: Print cancelled by user");
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowPrintDialog: ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"ShowPrintDialog: Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Fehler beim Öffnen des Druckdialogs: {ex.Message}", "Fehler", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Führt den Druckvorgang mit den angegebenen Optionen aus
        /// </summary>
        /// <param name="tournamentClass">Die zu druckende Turnierklasse</param>
        /// <param name="printOptions">Die Druckoptionen</param>
        /// <param name="localizationService">Service für Übersetzungen</param>
        /// <returns>True wenn erfolgreich gedruckt wurde</returns>
        private static bool ExecutePrint(TournamentClass tournamentClass, TournamentPrintOptions printOptions, LocalizationService? localizationService)
        {
            try
            {
                var printService = new PrintService(localizationService);
                var success = printService.PrintTournamentStatistics(tournamentClass, printOptions);

                if (success)
                {
                    var message = localizationService?.GetString("PrintSuccessful") ?? "Turnierstatistiken wurden erfolgreich gedruckt.";
                    MessageBox.Show(message, "Drucken erfolgreich", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }

                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintHelper.ExecutePrint: ERROR: {ex.Message}");
                MessageBox.Show($"Fehler beim Drucken: {ex.Message}", "Druckfehler", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Prüft ob eine Turnierklasse druckbare Inhalte hat
        /// </summary>
        /// <param name="tournamentClass">Die zu prüfende Turnierklasse</param>
        /// <returns>True wenn druckbare Inhalte vorhanden sind</returns>
        public static bool HasPrintableContent(TournamentClass tournamentClass)
        {
            if (tournamentClass == null) 
            {
                System.Diagnostics.Debug.WriteLine("HasPrintableContent: TournamentClass is null");
                return false;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"HasPrintableContent: Checking content for {tournamentClass.Name}");

                // 1. Es gibt Gruppen mit Spielern
                var hasGroupsWithPlayers = tournamentClass.Groups?.Any(g => g?.Players?.Any() == true) ?? false;
                System.Diagnostics.Debug.WriteLine($"HasPrintableContent: HasGroupsWithPlayers = {hasGroupsWithPlayers}");
                
                if (hasGroupsWithPlayers)
                {
                    return true;
                }

                // 2. Es gibt eine Finals-Phase mit Inhalten
                var hasFinalsPhaseWithContent = false;
                if (tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
                {
                    hasFinalsPhaseWithContent = tournamentClass.CurrentPhase.FinalsGroup?.Players?.Any() == true;
                }
                else
                {
                    // Prüfe abgeschlossene Finals-Phasen
                    hasFinalsPhaseWithContent = tournamentClass.Phases?.Any(p => 
                        p.PhaseType == TournamentPhaseType.RoundRobinFinals && 
                        p.FinalsGroup?.Players?.Any() == true) ?? false;
                }
                
                System.Diagnostics.Debug.WriteLine($"HasPrintableContent: HasFinalsPhaseWithContent = {hasFinalsPhaseWithContent}");
                if (hasFinalsPhaseWithContent)
                {
                    return true;
                }

                // 3. Es gibt eine KO-Phase mit Inhalten
                var hasKnockoutPhaseWithContent = false;
                if (tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
                {
                    hasKnockoutPhaseWithContent = 
                        (tournamentClass.CurrentPhase.WinnerBracket?.Any() == true) ||
                        (tournamentClass.CurrentPhase.LoserBracket?.Any() == true) ||
                        (tournamentClass.CurrentPhase.QualifiedPlayers?.Any() == true);
                }
                else
                {
                    // Prüfe abgeschlossene KO-Phasen
                    hasKnockoutPhaseWithContent = tournamentClass.Phases?.Any(p => 
                        p.PhaseType == TournamentPhaseType.KnockoutPhase && 
                        ((p.WinnerBracket?.Any() == true) || 
                         (p.LoserBracket?.Any() == true) ||
                         (p.QualifiedPlayers?.Any() == true))) ?? false;
                }
                
                System.Diagnostics.Debug.WriteLine($"HasPrintableContent: HasKnockoutPhaseWithContent = {hasKnockoutPhaseWithContent}");
                if (hasKnockoutPhaseWithContent)
                {
                    return true;
                }

                // 4. Auch ohne spezifische Inhalte kann eine Übersichtsseite gedruckt werden, 
                // aber nur wenn das Turnier zumindest initialisiert wurde (Phases vorhanden)
                var hasAnyPhases = tournamentClass.Phases?.Any() ?? false;
                var hasGameRules = tournamentClass.GameRules != null;
                var hasTournamentData = hasAnyPhases && hasGameRules;
                
                System.Diagnostics.Debug.WriteLine($"HasPrintableContent: HasTournamentData = {hasTournamentData} (Phases: {hasAnyPhases}, GameRules: {hasGameRules})");
                
                return hasTournamentData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HasPrintableContent: ERROR: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Erstellt eine schnelle Druckvorschau für eine Turnierklasse
        /// </summary>
        /// <param name="tournamentClass">Die Turnierklasse</param>
        /// <param name="owner">Besitzerfenster</param>
        /// <param name="localizationService">Lokalisierungsservice</param>
        /// <returns>True wenn Vorschau angezeigt wurde</returns>
        public static bool ShowQuickPreview(TournamentClass tournamentClass, Window? owner = null, LocalizationService? localizationService = null)
        {
            try
            {
                if (tournamentClass == null || !HasPrintableContent(tournamentClass))
                {
                    return false;
                }

                var printService = new PrintService(localizationService);
                var options = CreateDefaultPrintOptions(tournamentClass);
                var preview = printService.CreatePrintPreview(tournamentClass, options);
                
                if (preview != null)
                {
                    var previewWindow = new Window
                    {
                        Title = $"Schnellvorschau - {tournamentClass.Name}",
                        Content = preview,
                        WindowState = WindowState.Maximized,
                        Owner = owner
                    };
                    
                    previewWindow.ShowDialog();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintHelper.ShowQuickPreview: ERROR: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Erstellt Standard-Druckoptionen für eine Turnierklasse
        /// </summary>
        /// <param name="tournamentClass">Die Turnierklasse</param>
        /// <returns>Standard-Druckoptionen</returns>
        private static TournamentPrintOptions CreateDefaultPrintOptions(TournamentClass tournamentClass)
        {
            var hasGroups = tournamentClass.Groups.Any();
            var hasFinalsPhase = tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals;
            var hasKnockoutPhase = tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase;
            var hasLoserBracket = hasKnockoutPhase && tournamentClass.GetLoserBracketMatches().Any();

            return new TournamentPrintOptions
            {
                ShowPrintDialog = false, // Für Schnellvorschau
                IncludeOverview = true,
                IncludeGroupPhase = hasGroups,
                SelectedGroups = new List<int>(), // Leer = alle Gruppen
                IncludeFinalsPhase = hasFinalsPhase,
                IncludeKnockoutPhase = hasKnockoutPhase,
                IncludeWinnerBracket = hasKnockoutPhase,
                IncludeLoserBracket = hasLoserBracket,
                IncludeKnockoutParticipants = hasKnockoutPhase,
                Title = "",
                Subtitle = ""
            };
        }

        /// <summary>
        /// Schnelldruck mit Standard-Einstellungen (ohne Dialog)
        /// </summary>
        /// <param name="tournamentClass">Die zu druckende Turnierklasse</param>
        /// <param name="localizationService">Lokalisierungsservice</param>
        /// <returns>True wenn erfolgreich gedruckt</returns>
        public static bool QuickPrint(TournamentClass tournamentClass, LocalizationService? localizationService = null)
        {
            try
            {
                if (tournamentClass == null || !HasPrintableContent(tournamentClass))
                {
                    return false;
                }

                var printService = new PrintService(localizationService);
                var options = CreateDefaultPrintOptions(tournamentClass);
                options.ShowPrintDialog = true; // Zeige System-Druckdialog
                
                return printService.PrintTournamentStatistics(tournamentClass, options);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintHelper.QuickPrint: ERROR: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generiert eine Zusammenfassung der druckbaren Inhalte
        /// </summary>
        /// <param name="tournamentClass">Die Turnierklasse</param>
        /// <param name="localizationService">Lokalisierungsservice</param>
        /// <returns>Textuelle Beschreibung der Inhalte</returns>
        public static string GetPrintableSummary(TournamentClass tournamentClass, LocalizationService? localizationService = null)
        {
            if (tournamentClass == null)
            {
                return "Keine Daten verfügbar";
            }

            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Turnierklasse: {tournamentClass.Name}");
            
            // Gruppen
            if (tournamentClass.Groups.Any())
            {
                var totalPlayers = tournamentClass.Groups.SelectMany(g => g.Players).Count();
                var totalMatches = tournamentClass.Groups.SelectMany(g => g.Matches).Count();
                summary.AppendLine($"Gruppen: {tournamentClass.Groups.Count} ({totalPlayers} Spieler, {totalMatches} Spiele)");
            }

            // Aktuelle Phase
            if (tournamentClass.CurrentPhase != null)
            {
                summary.AppendLine($"Phase: {tournamentClass.CurrentPhase.Name}");
                
                if (tournamentClass.CurrentPhase.PhaseType == TournamentPhaseType.RoundRobinFinals)
                {
                    var finalistsCount = tournamentClass.CurrentPhase.QualifiedPlayers?.Count ?? 0;
                    summary.AppendLine($"Finalisten: {finalistsCount}");
                }
                else if (tournamentClass.CurrentPhase.PhaseType == TournamentPhaseType.KnockoutPhase)
                {
                    var winnerMatches = tournamentClass.GetWinnerBracketMatches().Count;
                    var loserMatches = tournamentClass.GetLoserBracketMatches().Count;
                    summary.AppendLine($"Winner Bracket: {winnerMatches} Spiele");
                    if (loserMatches > 0)
                    {
                        summary.AppendLine($"Loser Bracket: {loserMatches} Spiele");
                    }
                }
            }

            return summary.ToString();
        }
    }
} 