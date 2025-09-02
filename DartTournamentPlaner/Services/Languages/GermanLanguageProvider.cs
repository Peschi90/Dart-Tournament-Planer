using System;
using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages;

/// <summary>
/// Deutsche Übersetzungen für den Dart Turnier Planer
/// </summary>
public class GermanLanguageProvider : ILanguageProvider
{
    public string LanguageCode => "de";
    public string DisplayName => "Deutsch";

    /// <summary>
    /// Ermittelt die aktuelle Assembly-Version der Anwendung
    /// </summary>
    /// <returns>Versionsnummer als String (z.B. "1.2.3")</returns>
    private string GetCurrentAssemblyVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            
            if (version != null)
            {
                // Format: Major.Minor.Build (ohne Revision für bessere Lesbarkeit)
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            
            return "1.0.0"; // Fallback
        }
        catch
        {
            return "1.0.0"; // Fallback bei Fehlern
        }
    }

    /// <summary>
    /// Generiert dynamischen About-Text mit aktueller Assembly-Version
    /// </summary>
    /// <returns>About-Text mit aktueller Versionsnummer</returns>
    private string GetDynamicAboutText()
    {
        var currentVersion = GetCurrentAssemblyVersion();
        return $"Dart Turnier Planer v{currentVersion}\n\nEine moderne Turnierverwaltungssoftware.\n\n© 2025 by I3uLL3t";
    }

    public Dictionary<string, string> GetTranslations()
    {
        return new Dictionary<string, string>
        {
            // Anwendungseinstellungen
            ["Settings"] = "Einstellungen",
            ["Language"] = "Sprache",
            ["Theme"] = "Design",
            ["AutoSave"] = "Automatisches Speichern",
            ["AutoSaveInterval"] = "Speicherintervall (Minuten)",
            ["Save"] = "Speichern",
            ["Cancel"] = "Abbrechen",

            // Menü-Einträge
            ["File"] = "Datei",
            ["New"] = "Neu",
            ["Open"] = "Öffnen",
            ["SaveAs"] = "Speichern unter",
            ["Print"] = "Drucken",
            ["Exit"] = "Beenden",
            ["Edit"] = "Bearbeiten",
            ["View"] = "Ansicht",
            ["Help"] = "Hilfe",
            ["About"] = "Über",

            // Status-Anzeigen
            ["HasUnsavedChanges"] = "Geändert",
            ["NotSaved"] = "Nicht gespeichert",
            ["Saved"] = "Gespeichert",
            ["Ready"] = "Bereit",

            // Allgemeine UI-Elemente
            ["Close"] = "Schließen",
            ["OK"] = "OK",
            ["Start"] = "Start",
            ["Stop"] = "Stop",
            ["Player"] = "Spieler",
            ["Match"] = "Spiel",
            ["Result"] = "Ergebnis",
            ["Status"] = "Status",
            ["Position"] = "Platz",
            ["Winner"] = "Sieger",
            ["Information"] = "Information",
            ["Warning"] = "Warnung",
            ["Error"] = "Fehler",

            // Hilfesystem
            ["HelpTitle"] = "Hilfe - Dart Turnier Planer",
            ["HelpGeneral"] = "Allgemeine Bedienung",
            ["HelpTournamentSetup"] = "Turnier-Setup",
            ["HelpGroupManagement"] = "Gruppenverwaltung",
            ["HelpGameRules"] = "Spielregeln",
            ["HelpMatches"] = "Spiele & Ergebnisse",
            ["HelpTournamentPhases"] = "Turnierphasen",
            ["HelpMenus"] = "Menüs & Funktionen",
            ["HelpTips"] = "Tipps & Tricks",

            // Ausführliche Hilfe-Inhalte
            ["HelpGeneralContent"] = "Der Dart Turnier Planer hilft Ihnen bei der Verwaltung von Dart-Turnieren mit bis zu 4 verschiedenen Klassen (Platin, Gold, Silber, Bronze).\n\n" +
                "• Verwenden Sie die Tabs oben, um zwischen den Klassen zu wechseln\n" +
                "• Alle Änderungen werden automatisch gespeichert (wenn aktiviert)\n" +
                "• Die Statusleiste zeigt den aktuellen Speicherstatus an\n" +
                "• Sprache kann in den Einstellungen geändert werden",

            ["HelpTournamentSetupContent"] = "So richten Sie ein neues Turnier ein:\n\n" +
                "1. Wählen Sie eine Turnierklasse (Platin, Gold, Silber, Bronze)\n" +
                "2. Klicken Sie auf 'Gruppe hinzufügen' um Gruppen zu erstellen\n" +
                "3. Fügen Sie Spieler zu den Gruppen hinzu\n" +
                "4. Konfigurieren Sie die Spielregeln über den 'Regeln konfigurieren' Button\n" +
                "5. Stellen Sie den Modus nach der Gruppenphase ein (Nur Gruppen, Finalrunde, KO-System)\n\n" +
                "Tipp: Mindestens 2 Spieler pro Gruppe sind erforderlich für die Spielgenerierung.",

            ["HelpGroupManagementContent"] = "Gruppenverwaltung:\n\n" +
                "• 'Gruppe hinzufügen': Erstellt eine neue Gruppe\n" +
                "• 'Gruppe entfernen': Löscht die ausgewählte Gruppe (Warnung erscheint)\n" +
                "• 'Spieler hinzufügen': Fügt einen Spieler zur ausgewählten Gruppe hinzu\n" +
                "• 'Spieler entfernen': Entfernt den ausgewählten Spieler\n\n" +
                "Die Spielerliste zeigt alle Spieler der aktuell ausgewählten Gruppe.\n" +
                "Gruppen können beliebig benannt werden und sollten aussagekräftige Namen haben.",

            ["HelpGameRulesContent"] = "Spielregeln konfigurieren:\n\n" +
                "• Spielmodus: 301, 401 oder 501 Punkte\n" +
                "• Finish-Modus: Single Out oder Double Out\n" +
                "• Legs zum Sieg: Anzahl der Legs für einen Sieg\n" +
                "• Mit Sets spielen: Aktiviert das Set-System\n" +
                "• Sets zum Sieg: Anzahl der Sets für einen Turniersieg\n" +
                "• Legs pro Set: Anzahl der Legs pro Set\n\n" +
                "Für verschiedene Turnierrunden können unterschiedliche Regeln festgelegt werden.",

            ["HelpMatchesContent"] = "Spiele verwalten:\n\n" +
                "• 'Spiele generieren': Erstellt alle Spiele für die Gruppe (Round-Robin)\n" +
                "• Klicken Sie auf ein Spiel, um das Ergebnis einzugeben\n" +
                "• Status: Nicht gestartet (grau), Läuft (gelb), Beendet (grün)\n" +
                "• Rechtsklick auf Spiele für weitere Optionen (Freilos, etc.)\n" +
                "• 'Spiele zurücksetzen': Löscht alle Ergebnisse der Gruppe\n\n" +
                "Die Tabelle zeigt automatisch die aktuelle Platzierung aller Spieler.",

            ["HelpTournamentPhasesContent"] = "Turnierphasen:\n\n" +
                "1. Gruppenphase: Round-Robin innerhalb jeder Gruppe\n" +
                "2. Nach der Gruppenphase (optional):\n" +
                "   • Nur Gruppenphase: Turnier endet nach den Gruppen\n" +
                "   • Finalrunde: Top-Spieler spielen Round-Robin\n" +
                "   • KO-System: Einzel- oder Doppel-Eliminierung\n\n" +
                "Der 'Nächste Phase starten' Button wird verfügbar, wenn alle Spiele beendet sind.\n" +
                "KO-System kann Winner Bracket und Loser Bracket haben.",

            ["HelpMenusContent"] = "Menü-Funktionen:\n\n" +
                "Datei:\n• Neu: Erstellt ein leeres Turnier\n• Öffnen/Speichern: Lädt/Speichert Turnierdaten\n• Beenden: Schließt die Anwendung\n\n" +
                "Ansicht:\n• Turnier-Übersicht: Zeigt eine Vollbild-Ansicht aller Klassen\n\n" +
                "Einstellungen:\n• Sprache, Design und Auto-Speicher-Einstellungen\n\n" +
                "Hilfe:\n• Diese Hilfe-Seite\n• Über-Dialog mit Versionsinformationen",

            ["HelpTipsContent"] = "Tipps & Tricks:\n\n" +
                "• Verwenden Sie aussagekräftige Gruppennamen (z.B. 'Gruppe A', 'Anfänger')\n" +
                "• Aktivieren Sie Auto-Speichern in den Einstellungen\n" +
                "• Die Turnier-Übersicht eignet sich perfekt für Beamer-Präsentationen\n" +
                "• Rechtsklick auf Spiele zeigt zusätzliche Optionen\n" +
                "• Bei ungerader Spielerzahl wird automatisch ein Freilos vergeben\n" +
                "• Sets und Legs werden automatisch validiert\n" +
                "• Verschiedene Regeln für verschiedene Turnierrunden möglich\n" +
                "• Export/Import von Turnierdaten über das Datei-Menü",

            // Turnierübersicht-spezifische Übersetzungen
            ["TournamentOverview"] = "🏆 Turnier-Übersicht",
            ["OverviewMode"] = "Übersichtsmodus",
            ["Configure"] = "Konfigurieren",
            ["ManualMode"] = "Manueller Modus",
            ["AutoCyclingActive"] = "Automatischer Wechsel aktiv",
            ["CyclingStopped"] = "Automatischer Wechsel gestoppt",
            ["ManualControl"] = "Manuelle Steuerung",
            ["Showing"] = "Anzeigen",

            // Spielstatus
            ["Unknown"] = "Unbekannt",

            // Weitere Turnierübersicht Begriffe
            ["StartCycling"] = "Start",
            ["StopCycling"] = "Stopp",
            ["WinnerBracketMatches"] = "Spiele im Gewinnerbaum",
            ["WinnerBracketTree"] = "Gewinnerbaum",
            ["LoserBracketMatches"] = "Spiele im Verliererbaum",
            ["LoserBracketTree"] = "Verliererbaum",
            ["RoundColumn"] = "Runde",
            ["PositionShort"] = "Pos",
            ["PointsShort"] = "Pkt",
            ["WinDrawLoss"] = "S-U-N",
            ["NoLoserBracketMatches"] = "Keine Spiele im Verliererbaum verfügbar",
            ["NoWinnerBracketMatches"] = "Keine Spiele im Gewinnerbaum verfügbar",
            ["TournamentTreeWillShow"] = "Der Turnierbaum wird angezeigt, sobald die K.-o.-Phase beginnt",

            // Zusätzliche Gruppenphasenbegriffe
            ["SelectGroup"] = "Gruppe auswählen",
            ["NoGroupSelected"] = "Keine Gruppe ausgewählt",

            // Übersichtskonfigurationsdialog
            ["OverviewConfiguration"] = "Übersichtskonfiguration",
            ["TournamentOverviewConfiguration"] = "Turnierübersichtskonfiguration",
            ["TimeBetweenClasses"] = "Zeit zwischen Turnierklassen:",
            ["TimeBetweenSubTabs"] = "Zeit zwischen Unterreitern:",
            ["Seconds"] = "Sekunden",
            ["ShowOnlyActiveClassesText"] = "Nur Klassen mit aktiven Gruppen anzeigen",
            ["OverviewInfoText"] = "Live-Turnierdarstellung für alle Klassen mit automatischem Wechsel",
            ["InvalidClassInterval"] = "Ungültiges Klassenintervall. Bitte eine Zahl ≥ 1 eingeben.",
            ["InvalidSubTabInterval"] = "Ungültiges Unterreiterintervall. Bitte eine Zahl ≥ 1 eingeben.",

            // Turnierübersicht Texte
            ["TournamentName"] = "🎯 Turnier:",
            ["CurrentPhase"] = "📋 Aktuelle Phase:",
            ["GroupsCount"] = "👥 Gruppen:",
            ["PlayersTotal"] = "👤 Gesamtspieler:",
            ["GameRulesColon"] = "📜 Spielregeln:",
            ["CompletedGroups"] = "✅ Abgeschlossene Gruppen:",
            ["QualifiedPlayers"] = "🏅 Qualifizierte Spieler:",
            ["KnockoutMatches"] = "⚔️ K.-o.-Spiele:",
            ["Completed"] = "abgeschlossen",

            // Weitere fest codierte Texte
            ["Finalists"] = "Finalisten",
            ["KnockoutParticipants"] = "K.-o.-Teilnehmer",
            ["PlayersText"] = "Spieler",
            ["OverviewModeTitle"] = "Turnierübersichtsmodus",
            ["NewTournament"] = "Neues Turnier",
            ["CreateNewTournament"] = "Neues Turnier erstellen? Ungespeicherte Änderungen gehen verloren.",
            ["UnsavedChanges"] = "Ungespeicherte Änderungen",
            ["SaveBeforeExit"] = "Es gibt ungespeicherte Änderungen. Möchten Sie vor dem Beenden speichern?",
            ["CustomFileNotImplemented"] = "Benutzerdefiniertes Laden von Dateien noch nicht implementiert.",
            ["CustomFileSaveNotImplemented"] = "Benutzerdefiniertes Speichern von Dateien noch nicht implementiert.",
            ["ErrorOpeningHelp"] = "Fehler beim Öffnen der Hilfe:",
            ["ErrorOpeningOverview"] = "Fehler beim Öffnen der Turnierübersicht:",
            ["AboutText"] = GetDynamicAboutText(),
            ["ErrorSavingData"] = "Fehler beim Speichern der Daten:",

            // Nachrichten für Turnier-Tab
            ["MinimumTwoPlayers"] = "Mindestens 2 Spieler erforderlich.",
            ["ErrorGeneratingMatches"] = "Fehler beim Erstellen der Spiele:",
            ["MatchesGeneratedSuccess"] = "Spiele wurden erfolgreich erstellt!",
            ["MatchesResetSuccess"] = "Spiele wurden zurückgesetzt!",
            ["ResetTournamentConfirm"] = "Möchten Sie wirklich das gesamte Turnier zurücksetzen?\n\n⚠️ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.",
            ["TournamentResetComplete"] = "Das Turnier wurde erfolgreich zurückgesetzt.",
            ["ResetKnockoutConfirm"] = "Möchten Sie wirklich die K.-o.-Phase zurücksetzen?\n\n⚠️ Alle K.-o.-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.",
            ["ResetKnockoutComplete"] = "Die K.-o.-Phase wurde erfolgreich zurückgesetzt.",
            ["ResetFinalsConfirm"] = "Möchten Sie wirklich die Finalrunde zurücksetzen?\n\n⚠️ Alle Finalspiele werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.",
            ["ResetFinalsComplete"] = "Die Finalrunde wurde erfolgreich zurückgesetzt.",
            ["ErrorResettingTournament"] = "Fehler beim Zurücksetzen des Turniers:",
            ["CannotAdvancePhase"] = "Alle Spiele der aktuellen Phase müssen abgeschlossen sein",
            ["ErrorAdvancingPhase"] = "Fehler beim Wechsel in die nächste Phase:",
            ["UIRefreshed"] = "Benutzeroberfläche wurde aktualisiert",
            ["ErrorRefreshing"] = "Fehler beim Aktualisieren:",
            ["KOPhaseActiveMSB"] = "K.-o.-Phase ist nicht aktiv",
            ["KOPhaseNotEnoughUserMSB"] = "Nicht genügend Teilnehmer für die K.-o.-Phase (mindestens 2 erforderlich)",

            // Meldungstitel
            ["KOPhaseUsrWarnTitel"] = "K.-o.-Phasen-Warnung",

            // Tab-Kopfzeilen für Spieleransicht
            ["FinalistsCount"] = "Finalisten ({0} Spieler):",
            ["KnockoutParticipantsCount"] = "K.-o.-Teilnehmer ({0} Spieler):",

            // Weitere Phasen-Texte
            ["NextPhaseStart"] = "Start {0}",

            // Match-Ergebnisfenster
            ["EnterMatchResult"] = "Match-Ergebnis eingeben",
            ["SaveResult"] = "Ergebnis speichern",
            ["Notes"] = "Notizen",
            ["InvalidNumbers"] = "Ungültige Zahlen",
            ["NegativeValues"] = "Negative Werte sind nicht erlaubt",
            ["InvalidSetCount"] = "Ungültige Satzanzahl. Maximum: {0}, Gesamt: {1}",
            ["BothPlayersWon"] = "Beide Spieler können nicht gleichzeitig gewinnen",
            ["MatchIncomplete"] = "Das Spiel ist noch nicht abgeschlossen",
            ["InsufficientLegsForSet"] = "{0} hat nicht genügend Legs für die gewonnenen Sätze. Minimum: {1}",
            ["ExcessiveLegs"] = "Zu viele Legs für die Satzkombination {0}:{1}. Maximum: {2}",
            ["LegsExceedSetRequirement"] = "{0} hat mehr Legs als für die Sätze erforderlich",
            ["InvalidLegCount"] = "Ungültige Leg-Anzahl. Maximum: {0}, Gesamt: {1}",
            ["SaveBlocked"] = "Speichern blockiert",
            ["ValidationError"] = "Validierungsfehler",
            ["NoWinnerFound"] = "Kein Gewinner gefunden",
            ["GiveBye"] = "Freilos vergeben",
            ["SelectByeWinner"] = "Wählen Sie den Spieler, der das Freilos erhalten soll:",

            // Eingabedialog
            ["InputDialog"] = "Eingabe",
            ["EnterName"] = "Name eingeben:",

            // Spenden- und Bug-Report-Funktionen
            ["Donate"] = "💝",
            ["DonateTooltip"] = "Support the development of this project",
            ["BugReport"] = "🐛 Fehler melden",
            ["BugReportTooltip"] = "Report bugs or suggest improvements",
            ["BugReportTitle"] = "Bug Report",
            ["BugReportDescription"] = "Describe the problem or your improvement idea:",
            ["BugReportEmailSubject"] = "Dart Tournament Planner - Bug Report",
            ["BugReportSteps"] = "Steps to reproduce:",
            ["BugReportExpected"] = "Expected behavior:",
            ["BugReportActual"] = "Actual behavior:",
            ["BugReportSystemInfo"] = "System Information:",
            ["BugReportVersion"] = "Version:",
            ["BugReportOS"] = "Operating System:",
            ["BugReportSubmitEmail"] = "Send via Email",
            ["BugReportSubmitGitHub"] = "Open on GitHub",
            ["ThankYouSupport"] = "Thank you for your support!",
            ["BugReportSent"] = "Bug report has been successfully sent. Thank you!",
            ["ErrorSendingBugReport"] = "Error sending bug report:",
            ["SupportDevelopment"] = "Support Development",
            ["DonationMessage"] = "Do you like this Dart Tournament Planner?\n\nSupport further development with a small donation.\nEvery contribution helps with improving and maintaining the software.",
            ["OpenDonationPage"] = "Open Donation Page",

            // Loading Spinner / Lade-Anzeige
            ["Loading"] = "Wird geladen...",
            ["CheckingGroupStatus"] = "Überprüfe Gruppenstatus...",
            ["ProcessingMatches"] = "Verarbeite Spiele...",
            ["CheckingCompletion"] = "Überprüfe Abschluss...",

            // Startup und Update-Funktionen
            ["StartingApplication"] = "Starte Anwendung...",
            ["AppSubtitle"] = "Moderne Turnierverwaltung",
            ["CheckingForUpdates"] = "Suche nach Updates...",
            ["ConnectingToGitHub"] = "Verbinde mit GitHub...",
            ["AnalyzingReleases"] = "Analysiere Releases...",
            ["UpdateAvailable"] = "Update verfügbar",
            ["WhatsNew"] = "Was ist neu:",
            ["RemindLater"] = "Später erinnern",
            ["SkipVersion"] = "Version überspringen",
            ["DownloadUpdate"] = "Jetzt herunterladen",
            ["DownloadAndInstall"] = "Herunterladen & Installieren",
            ["DownloadingUpdate"] = "Update wird heruntergeladen",
            ["PreparingDownload"] = "Bereite Download vor...",
            ["DownloadingSetup"] = "Lade Setup herunter...",
            ["DownloadCompleted"] = "Download abgeschlossen, prüfe Datei...",
            ["PreparingInstallation"] = "Bereite Installation vor...",
            ["StartingInstallation"] = "Starte Installation...",
            ["InstallationStarted"] = "Installation gestartet",
            ["InstallationCancelled"] = "Installation abgebrochen",
            ["ErrorStartingSetup"] = "Fehler beim Starten",
            ["AdminRightsRequired"] = "Administratorrechte erforderlich",
            ["NoUpdateAvailable"] = "Keine Updates verfügbar",

            // PRINT SERVICE ÜBERSETZUNGEN
            ["PrintError"] = "Druckfehler",
            ["ErrorCreatingDocument"] = "Fehler beim Erstellen des Druckdokuments.",
            ["ErrorPrinting"] = "Fehler beim Drucken: {0}",
            ["TournamentStatistics"] = "Turnierstatistiken - {0}",
            ["TournamentOverviewPrint"] = "Turnierübersicht - {0}",
            ["CreatedWith"] = "Erstellt mit Dart Tournament Planner",
            ["GameRulesLabel"] = "Spielregeln: {0}",
            ["CurrentPhaseLabel"] = "Aktuelle Phase: {0}",
            ["NotStarted"] = "Nicht begonnen",
            ["GroupsPlayersLabel"] = "Gruppen: {0}, Spieler gesamt: {1}",
            ["GroupsOverview"] = "Gruppen-Übersicht",
            ["PlayersCount"] = "Spieler: {0}",
            ["MatchesStatus"] = "{0} von {1} Spielen beendet",
            ["Table"] = "Tabelle",
            ["NoStandingsAvailable"] = "Noch keine Tabelle verfügbar.",
            ["MatchResults"] = "Spielergebnisse",
            ["NoMatchesAvailable"] = "Keine Spiele vorhanden.",
            ["WinnerBracketHeader"] = "{0} - Winner Bracket",
            ["LoserBracketHeader"] = "{0} - Loser Bracket",
            ["WinnerBracketMatches"] = "Winner Bracket - Spiele",
            ["LoserBracketMatches"] = "Loser Bracket - Spiele",
            ["NoWinnerBracketGames"] = "Keine Winner Bracket Spiele vorhanden.",
            ["NoLoserBracketGames"] = "Keine Loser Bracket Spiele vorhanden.",
            ["FinalsRound"] = "Finalrunde",

            // Tabellen-Header
            ["Position"] = "Pos",
            ["PlayerHeader"] = "Spieler",
            ["MatchesPlayedShort"] = "Sp",
            ["WinsShort"] = "S",
            ["DrawsShort"] = "U",
            ["LossesShort"] = "N",
            ["PointsHeader"] = "Pkt",
            ["SetsHeader"] = "Sets",
            ["MatchNumber"] = "Nr",
            ["MatchHeader"] = "Spiel",
            ["StatusHeader"] = "Status",
            ["ResultHeader"] = "Ergebnis",
            ["WinnerHeader"] = "Gewinner",
            ["RoundHeader"] = "Runde",

            // Match Status Texte
            ["ByeStatus"] = "FREILOS",
            ["FinishedStatus"] = "BEENDET",
            ["InProgressStatus"] = "LÄUFT",
            ["PendingStatus"] = "AUSSTEHEND",
            ["ByeGame"] = "{0} (Freilos)",
            ["VersusGame"] = "{0} vs {1}",
            ["Draw"] = "Unentschieden",

            // PRINT DIALOG ÜBERSETZUNGEN
            ["PrintTournamentStatistics"] = "Turnierstatistiken drucken",
            ["TournamentStatisticsIcon"] = "📊 Statistiken",
            ["TournamentClass"] = "🏆 Turnierklasse:",
            ["SelectTournamentClass"] = "Turnierklasse: {0} ({1} Gruppen, {2} Spieler)",
            ["EmptyTournamentClass"] = "❌ {0} (leer)",
            ["ActiveTournamentClass"] = "✅ {0}",
            ["GeneralOptions"] = "⚙️ Allgemeine Optionen",
            ["TournamentOverviewOption"] = "Turnierübersicht",
            ["TitleOptional"] = "📝 Titel (optional):",
            ["SubtitleOptional"] = "📄 Untertitel (optional):",
            ["GroupPhaseSection"] = "👥 Gruppenphase",
            ["IncludeGroupPhase"] = "Gruppenphase einschließen",
            ["SelectGroups"] = "Gruppen auswählen:",
            ["AllGroups"] = "Alle Gruppen",
            ["GroupWithPlayers"] = "{0} ({1} Spieler)",
            ["FinalsSection"] = "🏅 Finalrunde",
            ["IncludeFinals"] = "Finalrunde einschließen",
            ["KnockoutSection"] = "⚔️ KO-Phase",
            ["IncludeKnockout"] = "KO-Phase einschließen",
            ["WinnerBracket"] = "Winner Bracket",
            ["LoserBracket"] = "Loser Bracket",
            ["ParticipantsList"] = "Teilnehmer-Liste",
            ["PreviewSection"] = "👁️ Vorschau",
            ["PreviewPlaceholder"] = "📋 Vorschau wird hier angezeigt...",
            ["UpdatePreview"] = "🔄 Vorschau aktualisieren",
            ["PrintButton"] = "🖨️ Drucken",
            ["CancelButton"] = "❌ Abbrechen",
            ["PrintPreviewTitle"] = "Druckvorschau - {0}",
            ["NoContentSelected"] = "Keine Inhalte zum Anzeigen ausgewählt.",
            ["PreviewTitle"] = "Vorschau",
            ["PreviewError"] = "Fehler bei der Vorschau: {0}",
            ["PrintPreparationError"] = "Fehler bei der Druckvorbereitung: {0}",
            ["NoContentToPrint"] = "⚠️ Keine Inhalte zum Drucken ausgewählt",
            ["PreviewError2"] = "❌ Fehler bei der Vorschau: {0}",
            ["PreviewGenerationError"] = "⚠️ Fehler bei der Generierung der Vorschau-Informationen: {0}",
            ["SelectAtLeastOne"] = "Bitte wählen Sie mindestens eine Druckoption aus.",
            ["NoSelection"] = "Keine Auswahl",
            ["NoGroupsAvailable"] = "Die ausgewählte Turnierklasse enthält keine Gruppen zum Drucken.",
            ["NoGroupsAvailableTitle"] = "Keine Gruppen verfügbar",
            ["SelectAtLeastOneGroup"] = "Bitte wählen Sie mindestens eine Gruppe aus.",
            ["InvalidGroupSelection"] = "Die ausgewählten Gruppen sind nicht mehr verfügbar.",
            ["InvalidGroupSelectionTitle"] = "Ungültige Gruppenauswahl",
            ["NoFinalsAvailable"] = "Die ausgewählte Turnierklasse hat keine Finalrunde zum Drucken.",
            ["NoFinalsAvailableTitle"] = "Keine Finalrunde verfügbar",
            ["SelectAtLeastOneKO"] = "Bitte wählen Sie mindestens eine KO-Option aus.",
            ["NoKOOptionSelected"] = "Keine KO-Option ausgewählt",
            ["NoKnockoutAvailable"] = "Die ausgewählte Turnierklasse hat keine KO-Phase zum Drucken.",
            ["NoKnockoutAvailableTitle"] = "Keine KO-Phase verfügbar",
            ["ValidationError"] = "Fehler bei der Validierung: {0}",
            ["ValidationErrorTitle"] = "Validierungsfehler",

            // Preview-Inhalte
            ["PageOverview"] = "📄 Seite {0}: Turnierübersicht",
            ["OverviewContent1"] = "   • Allgemeine Turnierinformationen",
            ["OverviewContent2"] = "   • Spielregeln und Phasen-Status",
            ["OverviewContent3"] = "   • Gruppen-Übersicht",
            ["PageGroupPhase"] = "📄 Seite {0}: Gruppenphase - {1}",
            ["GroupPlayers"] = "   • {0} Spieler",
            ["GroupMatches"] = "   • {0} Spiele",
            ["GroupContent"] = "   • Tabelle und Ergebnisse",
            ["PageFinals"] = "📄 Seite {0}: Finalrunde",
            ["FinalsContent1"] = "   • Qualifizierte Finalisten",
            ["FinalsContent2"] = "   • Finals-Tabelle",
            ["FinalsContent3"] = "   • Finals-Spiele",
            ["PageWinnerBracket"] = "📄 Seite {0}: Winner Bracket",
            ["WinnerBracketMatches"] = "   • {0} KO-Spiele",
            ["PageLoserBracket"] = "📄 Seite {0}: Loser Bracket",
            ["LoserBracketMatches"] = "   • {0} LB-Spiele",
            ["PageKnockoutParticipants"] = "📄 Seite {0}: KO-Teilnehmer",
            ["KnockoutParticipantsContent"] = "   • {0} qualifizierte Spieler",

            // Kontext-Menü spezifische Übersetzungen
            ["EditResult"] = "Ergebnis bearbeiten",
            ["AutomaticBye"] = "Automatisches Freilos",
            ["UndoByeShort"] = "Freilos rückgängig machen",
            ["NoActionsAvailable"] = "Keine Aktionen verfügbar",
            ["ByeToPlayer"] = "Freilos an {0}",
            
            // Hauptfenster
            ["AppTitle"] = "Dart Turnier Planer",
            ["Platinum"] = "Platin",
            ["Gold"] = "Gold",
            ["Silver"] = "Silber",
            ["Bronze"] = "Bronze",
            
            // Turnier-Tab Übersetzungen
            ["SetupTab"] = "Turnier-Setup",
            ["GroupPhaseTab"] = "Gruppenphase",
            ["FinalsTab"] = "Finalrunde",
            ["KnockoutTab"] = "KO-Runde",
            ["Groups"] = "Gruppen:",
            ["Players"] = "Spieler:",
            ["AddGroup"] = "Gruppe hinzufügen",
            ["RemoveGroup"] = "Gruppe entfernen",
            ["AddPlayer"] = "Spieler hinzufügen",
            ["RemovePlayer"] = "Spieler entfernen",
            ["NewGroup"] = "Neue Gruppe",
            ["GroupName"] = "Geben Sie den Namen der neuen Gruppe ein:",
            ["RemoveGroupConfirm"] = "Möchten Sie die Gruppe '{0}' wirklich entfernen?\nAlle Spieler in dieser Gruppe werden ebenfalls entfernt.",
            ["RemoveGroupTitle"] = "Gruppe entfernen",
            ["RemovePlayerConfirm"] = "Möchten Sie den Spieler '{0}' wirklich entfernen?",
            ["RemovePlayerTitle"] = "Spieler entfernen",
            ["NoGroupSelected"] = "Bitte wählen Sie eine Gruppe aus, die entfernt werden soll.",
            ["NoGroupSelectedTitle"] = "Keine Gruppe ausgewählt",
            ["NoPlayerSelected"] = "Bitte wählen Sie einen Spieler aus, der entfernt werden soll.",
            ["NoPlayerSelectedTitle"] = "Kein Spieler ausgewählt",
            ["SelectGroupFirst"] = "Bitte wählen Sie zuerst eine Gruppe aus.",
            ["EnterPlayerName"] = "Bitte geben Sie einen Spielernamen ein.",
            ["NoNameEntered"] = "Kein Name eingegeben",
            ["PlayersInGroup"] = "Spieler in {0}:",
            ["NoGroupSelectedPlayers"] = "Spieler: (Keine Gruppe ausgewählt)",
            ["Group"] = "Gruppe {0}",
            ["AdvanceToNextPhase"] = "Nächste Phase starten",
            ["ResetTournament"] = "Turnier zurücksetzen",
            ["ResetKnockoutPhase"] = "KO-Phase zurücksetzen",
            ["ResetFinalsPhase"] = "Finalrunde zurücksetzen",
            ["RefreshUI"] = "UI aktualisieren",
            ["RefreshUITooltip"] = "Aktualisiert die Benutzeroberfläche",
            
            // Turnierprozessphasen
            ["GroupPhase"] = "Gruppenphase",
            ["FinalsPhase"] = "Finalrunde",
            ["KnockoutPhase"] = "KO-Phase",
            
            // Spielregeln
            ["GameRules"] = "Spielregeln",
            ["GameMode"] = "Spielmodus",
            ["Points501"] = "501 Punkte",
            ["Points401"] = "401 Punkte",
            ["Points301"] = "301 Punkte",
            ["FinishMode"] = "Finish-Modus",
            ["SingleOut"] = "Single Out",
            ["DoubleOut"] = "Double Out",
            ["LegsToWin"] = "Legs zum Sieg",
            ["PlayWithSets"] = "Mit Sets spielen",
            ["SetsToWin"] = "Sets zum Sieg",
            ["LegsPerSet"] = "Legs pro Set",
            ["ConfigureRules"] = "Regeln konfigurieren",
            ["RulesPreview"] = "Regelvorschau",
            ["AfterGroupPhaseHeader"] = "Nach der Gruppenphase",

            // Einstellungen nach der Gruppenphase
            ["PostGroupPhase"] = "Nach der Gruppenphase",
            ["PostGroupPhaseMode"] = "Modus nach Gruppenphase",
            ["PostGroupPhaseNone"] = "Nur Gruppenphase",
            ["PostGroupPhaseRoundRobin"] = "Finalrunde (Round Robin)",
            ["PostGroupPhaseKnockout"] = "KO-System",
            ["QualifyingPlayersPerGroup"] = "Qualifizierte pro Gruppe",
            ["KnockoutMode"] = "KO-Modus", 
            ["SingleElimination"] = "Einfaches KO",
            ["DoubleElimination"] = "Doppeltes KO (Winner + Loser Bracket)",
            ["IncludeGroupPhaseLosersBracket"] = "Gruppenphase-Verlierer ins Loser Bracket",
            
            // Rundenspezifische Regeln
            ["RoundSpecificRules"] = "Rundenspezifische Regeln",
            ["ConfigureRoundRules"] = "Rundenregeln konfigurieren",
            ["WinnerBracketRules"] = "Winner Bracket Regeln",
            ["LoserBracketRules"] = "Loser Bracket Regeln",
            ["RoundRulesFor"] = "Regeln für {0}",
            ["DefaultRules"] = "Standard-Regeln",
            ["ResetToDefault"] = "Auf Standard zurücksetzen",
            ["RoundRulesConfiguration"] = "Rundenregeln-Konfiguration",
            ["Best64Rules"] = "Beste 64 Regeln",
            ["Best32Rules"] = "Beste 32 Regeln",
            ["Best16Rules"] = "Beste 16 Regeln",
            ["QuarterfinalRules"] = "Viertelfinale Regeln",
            ["SemifinalRules"] = "Halbfinale Regeln",
            ["FinalRules"] = "Finale Regeln",
            ["GrandFinalRules"] = "Grand Final Regeln",
            
            // Individual round names for GetRoundDisplayName
            ["Best64"] = "Beste 64",
            ["Best32"] = "Beste 32", 
            ["Best16"] = "Beste 16",
            ["Quarterfinal"] = "Viertelfinale",
            ["Semifinal"] = "Halbfinale",
            ["Final"] = "Finale",
            ["GrandFinal"] = "Grand Final",
            ["LoserBracket"] = "Loser Bracket",

            // Spiele und Match-Management
            ["Matches"] = "Spiele:",
            ["Standings"] = "Tabelle:",
            ["GenerateMatches"] = "Spiele generieren",
            ["MatchesGenerated"] = "Spiele wurden erfolgreich generiert!",
            ["ResetMatches"] = "Spiele zurücksetzen",
            ["ResetMatchesConfirm"] = "Möchten Sie alle Spiele für Gruppe '{0}' wirklich zurücksetzen?\nAlle Ergebnisse gehen verloren!",
            ["ResetMatchesTitle"] = "Spiele zurücksetzen",
            ["MatchesReset"] = "Spiele wurden zurückgesetzt!",
            ["EnterResult"] = "Ergebnis eingeben",
            ["MatchNotStarted"] = "Nicht gestartet",
            ["MatchInProgress"] = "Läuft",
            ["MatchFinished"] = "Beendet",
            ["MatchBye"] = "Freilos",
            ["Round"] = "Runde",
            ["Sets"] = "Sets",
            ["Legs"] = "Legs",
            ["Score"] = "Punktestand",
            ["SubmitResult"] = "Ergebnis bestätigen",
            ["ResultSubmitted"] = "Ergebnis erfolgreich übermittelt!",
            ["Player1"] = "Spieler 1",
            ["Player2"] = "Spieler 2",
            ["Loser"] = "Verlierer",
            ["MatchCancelled"] = "Spiel wurde abgebrochen",
            ["CancelMatch"] = "Spiel abbrechen",
            ["MatchCancelledConfirm"] = "Möchten Sie das Spiel wirklich abbrechen?",
            ["MatchCancelledTitle"] = "Spiel abbrechen",
            ["NotImplemented"] = "Nicht implementiert",
            ["FeatureComingSoon"] = "Diese Funktion wird bald verfügbar sein.",

            // API-Funktionen
            ["API"] = "API",
            ["StartAPI"] = "API starten",
            ["StopAPI"] = "API stoppen", 
            ["APIDocumentation"] = "API Dokumentation",
            ["APIStatus"] = "API Status",
            ["APIRunning"] = "API läuft",
            ["APIStopped"] = "API gestoppt",
            ["APIError"] = "API Fehler",
            ["APIStarted"] = "API wurde erfolgreich gestartet!\n\nURL: {0}\nDokumentation: {0}",
            ["APIStartError"] = "API konnte nicht gestartet werden. Überprüfen Sie, ob Port 5000 verfügbar ist.",
            ["APIStopError"] = "Fehler beim Stoppen der API.",
            ["APINotRunning"] = "API ist nicht gestartet. Starten Sie die API zuerst.",
            ["APIURLNotAvailable"] = "API-URL ist nicht verfügbar.",
            ["ErrorOpeningBrowser"] = "Fehler beim Öffnen des Browsers",
            ["Success"] = "Erfolgreich",

            // ========================================
            // TOURNAMENT HUB ÜBERSETZUNGEN - NEU
            // ========================================

            // Tournament Hub Menü
            ["TournamentHub"] = "Tournament Hub",
            ["RegisterWithHub"] = "Bei Hub registrieren",
            ["UnregisterFromHub"] = "Vom Hub entregistrieren",
            ["ShowJoinUrl"] = "Join-URL anzeigen",
            ["ManualSync"] = "Manuell synchronisieren",
            ["HubSettings"] = "Hub-Einstellungen",

            // Hub Status Übersetzungen
            ["HubStatus"] = "Hub Status",
            ["HubConnected"] = "Verbunden",
            ["HubDisconnected"] = "Getrennt",
            ["HubConnecting"] = "Verbinde...",
            ["HubReconnecting"] = "Wiederverbindung...",
            ["HubError"] = "Fehler",
            ["HubSyncing"] = "Synchronisiere...",
            ["HubSyncComplete"] = "Sync abgeschlossen",
            ["HubWebSocket"] = "WebSocket",
            ["HubHTTP"] = "HTTP",

            // Hub Registrierung und Verwaltung
            ["RegisterTournamentTitle"] = "Tournament beim Hub registrieren",
            ["RegisterTournamentSuccess"] = "🎯 Tournament erfolgreich beim Hub registriert!\n\nTournament ID: {0}\nJoin URL: {1}\n\nDiese URL können Sie an Spieler senden.",
            ["RegisterTournamentError"] = "❌ Tournament konnte nicht beim Hub registriert werden.",
            ["UnregisterTournamentTitle"] = "Tournament entregistrieren",
            ["UnregisterTournamentConfirm"] = "Tournament '{0}' wirklich vom Hub entregistrieren?",
            ["UnregisterTournamentSuccess"] = "Tournament erfolgreich vom Hub entregistriert.",
            ["UnregisterTournamentError"] = "Fehler beim Entregistrieren: {0}",
            ["NoTournamentRegistered"] = "Kein Tournament beim Hub registriert.",

            // Hub Synchronisation
            ["SyncWithHub"] = "Mit Hub synchronisieren",
            ["SyncSuccess"] = "Tournament erfolgreich mit Hub synchronisiert!",
            ["SyncError"] = "Fehler beim Synchronisieren mit Hub.",
            ["ManualSyncError"] = "Fehler beim manuellen Sync: {0}",
            ["AutoSyncEnabled"] = "Automatische Synchronisation aktiviert",
            ["AutoSyncDisabled"] = "Automatische Synchronisation deaktiviert",

            // Join URL Funktionen
            ["JoinUrlTitle"] = "Tournament Join URL",
            ["JoinUrlMessage"] = "Tournament ID: {0}\n\nJoin URL:\n{1}\n\nDiese URL können Sie an Spieler senden.",
            ["JoinUrlError"] = "Fehler beim Anzeigen der Join-URL: {0}",
            ["JoinUrlCopied"] = "Join-URL wurde in die Zwischenablage kopiert",

            // Hub Einstellungen
            ["HubSettingsTitle"] = "Hub-Einstellungen",
            ["HubSettingsPrompt"] = "Geben Sie die Tournament Hub URL ein:",
            ["HubUrlUpdated"] = "Hub-URL aktualisiert:\n{0}",
            ["HubSettingsError"] = "Fehler bei den Hub-Einstellungen: {0}",
            ["InvalidHubUrl"] = "Ungültige Hub-URL. Bitte geben Sie eine vollständige URL ein.",

            // WebSocket Verbindung
            ["WebSocketConnecting"] = "WebSocket-Verbindung wird hergestellt...",
            ["WebSocketConnected"] = "WebSocket-Verbindung hergestellt",
            ["WebSocketDisconnected"] = "WebSocket-Verbindung getrennt",
            ["WebSocketError"] = "WebSocket-Fehler: {0}",
            ["WebSocketReconnecting"] = "WebSocket-Wiederverbindung...",
            ["WebSocketReconnected"] = "WebSocket-Wiederverbindung erfolgreich",
            ["WebSocketMaxRetriesReached"] = "Maximale WebSocket-Verbindungsversuche erreicht",

            // Match Updates vom Hub
            ["MatchUpdateReceived"] = "Match-Update erhalten",
            ["MatchUpdateProcessed"] = "Match {0} erfolgreich aktualisiert",
            ["MatchUpdateError"] = "Fehler beim Verarbeiten des Match-Updates: {0}",
            ["MatchResultFromHub"] = "Match-Ergebnis vom Hub empfangen",
            ["InvalidMatchUpdate"] = "Ungültiges Match-Update erhalten",

            // Tournament Data Sync
            ["TournamentDataSyncing"] = "Tournament-Daten werden synchronisiert...",
            ["TournamentDataSynced"] = "Tournament-Daten erfolgreich synchronisiert",
            ["TournamentDataSyncError"] = "Fehler bei der Tournament-Daten-Synchronisation: {0}",
            ["SendingTournamentData"] = "Tournament-Daten werden gesendet...",
            ["TournamentDataSent"] = "Tournament-Daten erfolgreich gesendet",

            // Hub Debug Console
            ["HubDebugConsole"] = "Tournament Hub Debug Console",
            ["DebugConsoleTitle"] = "Hub Debug Console",
            ["DebugConsoleReady"] = "Ready for debugging...",
            ["DebugConsoleStarted"] = "Tournament Hub Debug Console gestartet",
            ["DebugConsoleClear"] = "Debug Console löschen",
            ["DebugConsoleClearConfirm"] = "Möchten Sie alle Debug-Nachrichten löschen?",
            ["DebugConsoleCleared"] = "Debug Console geleert",
            ["DebugConsoleSave"] = "Debug Log speichern",
            ["DebugConsoleSaved"] = "Debug Log gespeichert unter: {0}",
            ["DebugConsoleSaveError"] = "Fehler beim Speichern: {0}",
            ["DebugConsoleClose"] = "Schließen",
            ["AutoScrollEnabled"] = "Auto-Scroll aktiviert",
            ["AutoScrollDisabled"] = "Auto-Scroll deaktiviert",
            ["MessagesCount"] = "Nachrichten: {0}",

            // Hub Connection Status Details
            ["ConnectionStatusUpdated"] = "Verbindungsstatus aktualisiert",
            ["HubServiceStatus"] = "Hub Service Status",
            ["LastSyncTime"] = "Letzte Synchronisation: {0}",
            ["NextSyncIn"] = "Nächste Synchronisation in: {0}",
            ["ConnectionQuality"] = "Verbindungsqualität",
            ["ConnectionStable"] = "Stabil",
            ["ConnectionUnstable"] = "Instabil",
            ["ConnectionPoor"] = "Schlecht",

            // Hub Heartbeat
            ["HeartbeatSent"] = "Heartbeat gesendet",
            ["HeartbeatReceived"] = "Heartbeat empfangen",
            ["HeartbeatError"] = "Heartbeat-Fehler: {0}",
            ["HeartbeatTimeout"] = "Heartbeat-Timeout",

            // Tournament ID und Client Info
            ["TournamentId"] = "Tournament ID",
            ["ClientType"] = "Client-Typ",
            ["TournamentPlanner"] = "Tournament Planner",
            ["ClientVersion"] = "Client-Version",
            ["ConnectedAt"] = "Verbunden seit",
            ["ClientId"] = "Client-ID",

            // Subscription Management
            ["SubscribingToTournament"] = "Abonniere Tournament-Updates...",
            ["SubscribedToTournament"] = "Tournament-Updates erfolgreich abonniert",
            ["UnsubscribingFromTournament"] = "Tournament-Updates-Abonnement kündigen...",
            ["UnsubscribedFromTournament"] = "Tournament-Updates-Abonnement erfolgreich gekündigt",
            ["SubscriptionError"] = "Abonnement-Fehler: {0}",

            // Hub Service Messages
            ["HubServiceStarted"] = "Hub Service gestartet",
            ["HubServiceStopped"] = "Hub Service gestoppt",
            ["HubServiceInitialized"] = "Hub Service initialisiert",
            ["HubServiceError"] = "Hub Service Fehler: {0}",
            ["HubServiceRestarting"] = "Hub Service wird neugestartet...",

            // Network und Connection
            ["NetworkError"] = "Netzwerkfehler: {0}",
            ["ConnectionTimeout"] = "Verbindungs-Timeout",
            ["ConnectionRefused"] = "Verbindung verweigert",
            ["ServerNotReachable"] = "Server nicht erreichbar",
            ["InternetConnectionRequired"] = "Internetverbindung erforderlich",

            // Tournament Hub URL Validation
            ["ValidatingHubUrl"] = "Hub-URL wird validiert...",
            ["HubUrlValid"] = "Hub-URL ist gültig",
            ["HubUrlInvalid"] = "Hub-URL ist ungültig",
            ["HubUrlNotReachable"] = "Hub-URL nicht erreichbar",
            ["DefaultHubUrl"] = "Standard-Hub-URL verwenden",

            // Status Bar Messages für Hub
            ["HubStatusConnected"] = "Hub: Verbunden",
            ["HubStatusDisconnected"] = "Hub: Getrennt",
            ["HubStatusConnecting"] = "Hub: Verbinde...",
            ["HubStatusError"] = "Hub: Fehler",
            ["HubStatusSyncing"] = "Hub: Sync...",
            ["HubStatusReady"] = "Hub: Bereit",

            // Tournament Registration Details
            ["GeneratingTournamentId"] = "Tournament-ID wird generiert...",
            ["TournamentIdGenerated"] = "Tournament-ID generiert: {0}",
            ["RegisteringWithServer"] = "Registrierung beim Server...",
            ["ServerRegistrationComplete"] = "Server-Registrierung abgeschlossen",
            ["ObtainingJoinUrl"] = "Join-URL wird abgerufen...",
            ["JoinUrlObtained"] = "Join-URL erhalten: {0}",

            // Error Categories für Debug Console
            ["InfoMessage"] = "Info",
            ["WarningMessage"] = "Warnung",
            ["ErrorMessage"] = "Fehler",
            ["SuccessMessage"] = "Erfolg",
            ["WebSocketMessage"] = "WebSocket",
            ["SyncMessage"] = "Sync",
            ["TournamentMessage"] = "Tournament",
            ["MatchMessage"] = "Match",
            ["MatchResultMessage"] = "Match-Ergebnis",

            // Advanced Hub Features
            ["HubStatistics"] = "Hub-Statistiken",
            ["ConnectedClients"] = "Verbundene Clients: {0}",
            ["ActiveTournaments"] = "Aktive Tournaments: {0}",
            ["TotalMatches"] = "Gesamte Matches: {0}",
            ["DataTransferred"] = "Übertragene Daten: {0}",
            ["UptimeInfo"] = "Betriebszeit: {0}",

            // Hub Configuration
            ["HubConfiguration"] = "Hub-Konfiguration",
            ["AutoReconnect"] = "Automatische Wiederverbindung",
            ["ReconnectInterval"] = "Wiederverbindungsintervall",
            ["MaxReconnectAttempts"] = "Maximale Wiederverbindungsversuche",
            ["SyncInterval"] = "Synchronisationsintervall",
            ["HeartbeatInterval"] = "Heartbeat-Intervall",

            // Feature Flags und Capabilities
            ["HubFeatures"] = "Hub-Features",
            ["RealTimeUpdates"] = "Echtzeit-Updates",
            ["MatchStreaming"] = "Match-Streaming",
            ["StatisticsSync"] = "Statistik-Synchronisation",
            ["MultiDeviceSupport"] = "Multi-Device-Unterstützung",
            ["OfflineMode"] = "Offline-Modus",

            // User Experience Messages
            ["PleaseWait"] = "Bitte warten...",
            ["ProcessingRequest"] = "Anfrage wird verarbeitet...",
            ["AlmostDone"] = "Fast fertig...",
            ["OperationCompleted"] = "Vorgang abgeschlossen",
            ["OperationCancelled"] = "Vorgang abgebrochen",
            ["TryAgain"] = "Erneut versuchen",
            ["CheckConnection"] = "Verbindung prüfen",

            // Tournament Hub Service Lifecycle
            ["ServiceInitializing"] = "Service wird initialisiert...",
            ["ServiceReady"] = "Service bereit",
            ["ServiceShuttingDown"] = "Service wird heruntergefahren...",
            ["ServiceShutdown"] = "Service heruntergefahren",
            ["ServiceRestarted"] = "Service neugestartet",
            ["ServiceHealthy"] = "Service ist gesund",
            ["ServiceUnhealthy"] = "Service ist nicht gesund",

            // ========================================
            // KO-TAB ÜBERSETZUNGEN - NEU
            // ========================================

            // KO-Tab Header und Navigation
            ["KOTab"] = "KO-Runde",
            ["StatisticsTab"] = "Statistiken",
            ["KOPhaseTab"] = "KO-Phase",
            ["WinnerBracketTab"] = "Winner Bracket",
            ["LoserBracketTab"] = "Loser Bracket",
            ["KOParticipantsTab"] = "KO-Teilnehmer",
            ["BracketOverviewTab"] = "Bracket-Übersicht",
            ["TreeViewTab"] = "Turnierturm",

            // KO-Phase Status und Meldungen
            ["KOPhaseNotActive"] = "KO-Phase ist nicht aktiv",
            ["KOPhaseWaitingForGroupCompletion"] = "Warten auf Abschluss der Gruppenphase",
            ["KOPhaseReady"] = "KO-Phase bereit",
            ["KOPhaseInProgress"] = "KO-Phase läuft",
            ["KOPhaseComplete"] = "KO-Phase abgeschlossen",
            ["GenerateKOBracket"] = "KO-Bracket generieren",
            ["KOBracketGenerated"] = "KO-Bracket wurde erfolgreich generiert!",
            ["ErrorGeneratingKOBracket"] = "Fehler beim Generieren des KO-Brackets:",
            ["NoQualifiedPlayersForKO"] = "Keine qualifizierten Spieler für die KO-Phase gefunden.",
            ["InsufficientPlayersForKO"] = "Nicht genügend Spieler für die KO-Phase (mindestens 2 erforderlich).",
            
            // KO-Match Status und Aktionen
            ["KOMatchPending"] = "Ausstehend",
            ["KOMatchInProgress"] = "Läuft",
            ["KOMatchFinished"] = "Beendet",
            ["KOMatchBye"] = "Freilos",
            ["NextRound"] = "Nächste Runde",
            ["AdvanceWinner"] = "Sieger weiterleiten",
            ["EliminateLoser"] = "Verlierer eliminieren",
            ["WinnerAdvancesTo"] = "Sieger qualifiziert sich für: {0}",
            ["LoserMovesToLB"] = "Verlierer ins Loser Bracket: {0}",
            ["WaitingForPreviousMatch"] = "Warten auf vorheriges Spiel",
            
            // KO-Bracket Struktur
            ["Round1"] = "1. Runde",
            ["Round2"] = "2. Runde", 
            ["Round3"] = "3. Runde",
            ["Quarterfinals"] = "Viertelfinale",
            ["Semifinals"] = "Halbfinale",
            ["Finals"] = "Finale",
            ["GrandFinals"] = "Grand Final",
            ["ThirdPlacePlayoff"] = "Spiel um Platz 3",
            ["WinnerBracketFinal"] = "Winner Bracket Finale",
            ["LoserBracketFinal"] = "Loser Bracket Finale",
            
            // KO-Teilnehmer und Seeding
            ["QualifiedFromGroup"] = "Qualifiziert aus Gruppe {0}",
            ["SeedPosition"] = "Setzlistenplatz {0}",
            ["HighestSeed"] = "Höchste Setzung",
            ["LowestSeed"] = "Niedrigste Setzung",
            ["RandomSeeding"] = "Zufällige Setzung",
            ["GroupWinners"] = "Gruppensieger",
            ["GroupRunners"] = "Gruppenzweite",
            ["BestThirds"] = "Beste Drittplatzierte",
            
            // KO-Bracket Actions und Buttons
            ["ResetKOBracket"] = "KO-Bracket zurücksetzen",
            ["ResetKOBracketConfirm"] = "Möchten Sie das KO-Bracket wirklich zurücksetzen?\n\n⚠️ Alle KO-Spiele werden gelöscht!\nDie qualifizierten Spieler bleiben erhalten.",
            ["KOBracketReset"] = "KO-Bracket wurde zurückgesetzt!",
            ["ExpandAllMatches"] = "Alle Spiele aufklappen",
            ["CollapseAllMatches"] = "Alle Spiele einklappen",
            ["ShowBracketTree"] = "Bracket-Baum anzeigen",
            ["ShowMatchList"] = "Spiele-Liste anzeigen",
            ["ExportBracket"] = "Bracket exportieren",
            ["PrintBracket"] = "Bracket drucken",
            
            // Double Elimination spezifische Begriffe
            ["WinnerBracket"] = "Winner Bracket",
            ["LoserBracket"] = "Loser Bracket",
            ["WinnerBracketMatches"] = "Winner Bracket Spiele",
            ["LoserBracketMatches"] = "Loser Bracket Spiele",
            ["UpperBracket"] = "Oberes Bracket",
            ["LowerBracket"] = "Unteres Bracket",
            ["ConsolationBracket"] = "Trostbracket",
            ["EliminationMatch"] = "Eliminierungsspiel",
            ["ConsolationMatch"] = "Trostspiel",
            
            // KO-Match Informationen
            ["MatchDuration"] = "Spieldauer: {0}",
            ["MatchStarted"] = "Spiel gestartet: {0}",
            ["MatchFinished"] = "Spiel beendet: {0}",
            ["ElapsedTime"] = "Vergangene Zeit: {0}",
            ["EstimatedDuration"] = "Geschätzte Dauer: {0}",
            ["QualificationPath"] = "Qualifikationsweg: {0}",
            
            // ========================================
            // STATISTIKEN-TAB ÜBERSETZUNGEN - NEU
            // ========================================
            
            // Statistiken Tab Header
            ["PlayerStatistics"] = "Spieler-Statistiken",
            ["TournamentStatistics"] = "Turnier-Statistiken",
            ["StatisticsOverview"] = "Statistik-Übersicht",
            ["PlayerRankings"] = "Spieler-Rangliste",
            ["PerformanceAnalysis"] = "Leistungsanalyse",
            ["StatisticsSummary"] = "Statistik-Zusammenfassung",
            ["DetailedStats"] = "Detaillierte Statistiken",
            
            // Statistik-Kategorien
            ["MatchStatistics"] = "Match-Statistiken",
            ["ScoreStatistics"] = "Punkte-Statistiken", 
            ["AccuracyStatistics"] = "Genauigkeits-Statistiken",
            ["FinishStatistics"] = "Finish-Statistiken",
            ["ConsistencyStats"] = "Konsistenz-Statistiken",
            ["ProgressionStats"] = "Fortschritts-Statistiken",
            
            // Spieler-Statistik Werte
            ["TotalMatches"] = "Gesamt Spiele",
            ["MatchesWon"] = "Siege",
            ["MatchesLost"] = "Niederlagen",
            ["MatchWinRate"] = "Sieg-Quote",
            ["OverallAverage"] = "Gesamt-Average",
            ["TournamentAverage"] = "Turnier-Average",
            ["BestAverage"] = "Bester Average",
            ["WorstAverage"] = "Schlechtester Average",
            ["HighestLegAverage"] = "Höchster Leg-Average",
            ["AverageScorePerDart"] = "Durchschnitt pro Pfeil",
            
            // Finish-Statistiken
            ["TotalCheckouts"] = "Gesamt Checkouts",
            ["CheckoutRate"] = "Checkout-Quote",
            ["HighFinishes"] = "High Finishes",
            ["TotalHighFinishes"] = "Gesamt High Finishes",
            ["HighFinishScores"] = "HF Scores",
            ["HighestFinish"] = "Höchstes Finish",
            ["HighestFinishScore"] = "Höchstes Finish",
            ["AverageCheckout"] = "Durchschnittliches Checkout",
            ["CheckoutAccuracy"] = "Checkout-Genauigkeit",
            ["FewestDartsToFinish"] = "Wenigste Pfeile zum Finish",
            ["AverageDartsPerCheckout"] = "∅ Pfeile pro Checkout",
            ["FastestCheckout"] = "Schnellstes Checkout",
            
            // Score-Statistiken 
            ["TotalMaximums"] = "180er",
            ["MaximumsPerGame"] = "180er pro Spiel",
            ["Score26"] = "26er",
            ["TotalScore26"] = "26er",
            ["Score26PerGame"] = "26er pro Spiel",
            ["HighScores"] = "Hohe Scores",
            ["ScoreDistribution"] = "Score-Verteilung",
            ["Above100Average"] = "Über 100 Average",
            ["Above80Average"] = "Über 80 Average",
            ["Above60Average"] = "Über 60 Average",
            
            // ✅ NEU: Erweiterte Effizienz-Statistiken
            ["FastestMatch"] = "Schnellstes Match",
            ["FewestThrowsInMatch"] = "Wenigste Würfe",
            ["FastestMatchTooltip"] = "Kürzeste Spieldauer über alle Matches (MM:SS)",
            ["FewestThrowsTooltip"] = "Wenigste Würfe in einem Match (beste Wurf-Effizienz)",
            
            // Zeit-basierte Statistiken
            ["LastMatchDate"] = "Letztes Spiel",
            ["FirstMatchDate"] = "Erstes Spiel",
            ["TotalPlayingTime"] = "Gesamt Spielzeit",
            ["AverageMatchDuration"] = "Durchschnittliche Spieldauer",
            ["LongestMatch"] = "Längstes Spiel",
            ["ShortestMatch"] = "Kürzestes Spiel",
            ["PlayingDays"] = "Spieltage",
            ["MatchesPerDay"] = "Spiele pro Tag",
            
            // Turnier-Kontext Statistiken
            ["GroupPhaseStats"] = "Gruppenphase-Statistiken",
            ["FinalsStats"] = "Final-Statistiken", 
            ["KOPhaseStats"] = "KO-Phasen-Statistiken",
            ["OverallTournamentStats"] = "Gesamt-Turnier-Statistiken",
            ["PhaseComparison"] = "Phasen-Vergleich",
            ["PerformanceByPhase"] = "Leistung nach Phase",
            
            // Statistik-Sortierung und Filter
            ["SortBy"] = "Sortieren nach",
            ["SortByName"] = "Name",
            ["SortByAverage"] = "Average",
            ["SortByMatches"] = "Spiele",
            ["SortByWinRate"] = "Sieg-Quote",
            ["SortByMaximums"] = "180er",
            ["SortByHighFinishes"] = "High Finishes",
            ["SortByCheckouts"] = "Checkouts",
            ["FilterPlayers"] = "Spieler filtern",
            ["ShowAllPlayers"] = "Alle Spieler anzeigen",
            ["ShowTopPlayers"] = "Top-Spieler anzeigen",
            ["MinimumMatches"] = "Mindest-Spielanzahl",
            
            // Statistik-Aktionen
            ["RefreshStatistics"] = "Statistiken aktualisieren",
            ["ExportStatistics"] = "Statistiken exportieren",
            ["PrintStatistics"] = "Statistiken drucken",
            ["ResetStatistics"] = "Statistiken zurücksetzen",
            ["SaveStatistics"] = "Statistiken speichern",
            ["LoadStatistics"] = "Statistiken laden",
            ["CompareToAverage"] = "Mit Durchschnitt vergleichen",
            
            // Statistik-Meldungen
            ["NoStatisticsAvailable"] = "Keine Statistiken verfügbar",
            ["StatisticsLoading"] = "Statistiken werden geladen...",
            ["StatisticsUpdated"] = "Statistiken aktualisiert",
            ["ErrorLoadingStatistics"] = "Fehler beim Laden der Statistiken: {0}",
            ["StatisticsNotEnabled"] = "Statistiken sind nicht aktiviert",
            ["InsufficientDataForStats"] = "Ungenügend Daten für Statistiken",
            
            // Detail-Ansichten
            ["PlayerDetails"] = "Spieler-Details für {0}",
            ["MatchHistory"] = "Spiel-Historie",
            ["ScoreHistory"] = "Score-Historie",
            ["PerformanceTrend"] = "Leistungstrend",
            ["StrengthsWeaknesses"] = "Stärken & Schwächen",
            ["RecentPerformance"] = "Aktuelle Leistung",
            ["CareerHighlights"] = "Karriere-Höhepunkte",
            
            // Vergleichs-Features
            ["ComparePlayer"] = "Spieler vergleichen",
            ["PlayerComparison"] = "Spieler-Vergleich",
            ["CompareWith"] = "Vergleichen mit",
            ["ComparisonResult"] = "Vergleichsergebnis",
            ["BetterThan"] = "Besser als {0}",
            ["WorseThan"] = "Schlechter als {0}",
            ["SimilarTo"] = "Ähnlich zu {0}",
            
            // Ranking und Positionen
            ["CurrentRank"] = "Aktuelle Position",
            ["RankByAverage"] = "Position nach Average",
            ["RankByWinRate"] = "Position nach Sieg-Quote",
            ["RankByMatches"] = "Position nach Spielen",
            ["TopPerformer"] = "Top-Performer",
            ["RankingChange"] = "Positionsänderung",
            ["MovedUp"] = "Aufgestiegen um {0}",
            ["MovedDown"] = "Abgestiegen um {0}",
            ["NoChange"] = "Keine Änderung",
            
            // Statistik-Tooltips und Hilfe
            ["AverageTooltip"] = "Durchschnittliche Punkte pro 3 Pfeile",
            ["WinRateTooltip"] = "Prozentsatz der gewonnenen Spiele",
            ["MaximumsTooltip"] = "Anzahl der 180-Punkte-Würfe",
            ["HighFinishesTooltip"] = "Checkouts über 100 Punkte",
            ["CheckoutRateTooltip"] = "Prozentsatz erfolgreicher Checkout-Versuche",
            ["ConsistencyTooltip"] = "Gleichmäßigkeit der Leistung",
            ["FastestMatchTooltip"] = "Kürzeste Spieldauer über alle Matches (MM:SS)",
            ["FewestThrowsTooltip"] = "Wenigste Würfe in einem Match (beste Wurf-Effizienz)",
            
            // Erweiterte Statistik-Features
            ["TrendAnalysis"] = "Trend-Analyse",
            ["PerformanceGraph"] = "Leistungsdiagramm",
            ["StatisticalSignificance"] = "Statistische Signifikanz",
            ["ConfidenceInterval"] = "Konfidenzintervall",
            ["StandardDeviation"] = "Standardabweichung",
            ["Correlation"] = "Korrelation",
            ["Regression"] = "Regression",
            ["PredictiveAnalysis"] = "Vorhersage-Analyse",
            
            // Debug und Entwickler-Info
            ["DebugStatistics"] = "Debug-Statistiken",
            ["StatisticsDebugInfo"] = "Statistik Debug-Info",
            ["DataIntegrity"] = "Daten-Integrität",
            ["ValidationStatus"] = "Validierungs-Status",
            ["LastUpdate"] = "Letzte Aktualisierung",
            ["DataSource"] = "Datenquelle",
            ["RecordCount"] = "Anzahl Datensätze",

            // ========================================
            // WEBSOCKET STATISTIK-EXTRAKTION - NEU
            // ========================================
            
            // Statistik-Extraktion Meldungen
            ["ProcessingMatchUpdate"] = "Verarbeite Match-Update für Klasse {0}",
            ["SkippingNonMatchResult"] = "Überspringe Nicht-Match-Ergebnis Update: {0}",
            ["ProcessingTopLevelStats"] = "Verarbeite Top-Level-Spieler-Statistiken für {0} vs {1}",
            ["ProcessingSimpleStats"] = "Verarbeite einfache Spieler-Statistiken für {0} vs {1}",
            ["ProcessingEnhancedStats"] = "Verarbeite erweiterte Dart-Statistiken für {0} vs {1}",
            ["FallbackNotesExtraction"] = "Fallback auf Notes-basierte Statistik-Extraktion",
            ["ErrorProcessingMatchResult"] = "Fehler beim Verarbeiten des Match-Ergebnisses: {0}",
            
            // JSON-Parsing Meldungen
            ["ProcessingJSONFromNotes"] = "Verarbeite JSON aus Notes-Feld für Top-Level-Statistiken",
            ["NoJSONDataFound"] = "Keine JSON-Daten in Notes für Top-Level-Statistiken gefunden",
            ["AvailableTopLevelProperties"] = "Verfügbare Top-Level-Properties: {0}",
            ["NoTopLevelStatsFound"] = "Keine Top-Level-Statistiken in JSON-Struktur gefunden",
            ["FoundTopLevelStats"] = "Top-Level-Statistiken in JSON gefunden",
            
            // Spieler-Daten Extraktion
            ["ExtractedPlayer1"] = "Player1 extrahiert: Avg {0}, 180er: {1}, HF: {2}, 26er: {3}",
            ["ExtractedPlayer2"] = "Player2 extrahiert: Avg {0}, 180er: {1}, HF: {2}, 26er: {3}",
            ["MatchDuration"] = "Match-Dauer: {0}",
            ["ParsedDuration"] = "Dauer geparst: {0} Minuten",
            ["MatchFormat"] = "Match-Format: {0}",
            
            // Spielernamen Extraktion
            ["FoundPlayer1NameFromResult"] = "player1Name aus matchUpdate.result gefunden: {0}",
            ["FoundPlayer2NameFromResult"] = "player2Name aus matchUpdate.result gefunden: {0}",
            ["UsingFallbackPlayerNames"] = "Verwende Fallback-Spielernamen-Extraktion",
            ["FallbackPlayerNames"] = "Fallback-Spielernamen: {0}, {1}",
            ["FinalExtractedStats"] = "Final extrahierte Statistiken: {0} vs {1}",
            ["ErrorParsingTopLevelStats"] = "Fehler beim Parsen der Top-Level-Spieler-Statistiken: {0}",
            
            // High Finish Scores Extraktion
            ["ExtractedHighFinishScores"] = "High Finish Scores extrahiert: [{0}]",
            ["NoPlayerNameFound"] = "Kein Spielername für Player {0} gefunden, verwende Fallback",
            ["ErrorExtractingPlayerName"] = "Fehler beim Extrahieren des Spielernamens: {0}",
            ["FoundPlayerNameInResult"] = "player{0}Name in matchUpdate.result gefunden: {1}",
            
            // Statistik-Verarbeitung Erfolg
            ["SuccessfullyProcessedSimpleStats"] = "Einfache Statistiken erfolgreich verarbeitet für {0} und {1}",
            ["SuccessfullyProcessedEnhancedStats"] = "Erweiterte Statistiken erfolgreich verarbeitet für {0} und {1}",
            ["ErrorProcessingSimpleStats"] = "Fehler beim Verarbeiten einfacher Statistiken: {0}",
            ["ErrorProcessingEnhancedStats"] = "Fehler beim Verarbeiten erweiterter Statistiken: {0}",
            
            // Gewinner-Bestimmung
            ["WinnerDetermined"] = "Gewinner bestimmt: {0} (Sets: {1}-{2}, Legs: {3}-{4})",
            ["ErrorDeterminingWinner"] = "Fehler beim Bestimmen des Gewinners:",
            
            // Erweiterte Statistik-Extraktion - NEU
            ["ExtractedEnhancedDetails"] = "{0} high finish details extrahiert",
            ["MatchDurationMs"] = "Match-Dauer: {0}ms = {1}",
            ["MatchDurationString"] = "Match-Dauer String: {0}",
            ["ExtractedStartTime"] = "Startzeit: {0}",
            ["ExtractedEndTime"] = "Endzeit: {0}",
            ["ExtractedTotalThrows"] = "Gesamtwürfe: {0}",
            ["ExtractedCheckouts"] = "Checkouts extrahiert: {0}",
            ["ExtractedTotalScore"] = "Gesamtpunktzahl extrahiert: {0}",
            ["ExtractedHighFinishDetails"] = "{0} High Finish Details extrahiert",
            ["GameRulesExtracted"] = "Spielregeln extrahiert: {0}, Double Out: {1}, Startpunktzahl: {2}",
            ["VersionInfoExtracted"] = "Version: {0}, Eingereicht via: {1}",
            ["DurationFormatted"] = "Dauer formatiert: {0}ms = {1}",
            
            // Match-Dauer Formatierung
            ["DurationSeconds"] = "{0} Sekunden",
            ["DurationMinutes"] = "{0:D2}:{1:D2} Minuten",
            ["DurationHours"] = "{0}:{1:D2}:{2:D2} Stunden",
            
            // Erweiterte Player-Statistiken
            ["PlayerStatsValidation"] = "Spieler-Daten Validierung: {0} (Avg: {1}, Throws: {2}, Score: {3}, Checkouts: {4})",
            ["DetailedStatsMerge"] = "Detaillierte Statistiken zusammengeführt: Checkouts: {0}, TotalThrows: {1}, TotalScore: {2}",
            ["RealDataUsage"] = "Verwende echte Daten: Throws: {0}, Score: {1}, Checkouts: {2}",
            
            // High Finish Details Verarbeitung
            ["HighFinishDetailsParsed"] = "High Finish Details geparst: Finish {0}, Darts: [{1}], Zeitstempel: {2}",
            ["HighFinishScoresExtracted"] = "High Finish Scores extrahiert: [{0}]",
            ["CheckoutDetailsCreated"] = "Checkout Details erstellt: {0} Checkouts",
            
            // Match-Metadaten
            ["MatchMetadataExtracted"] = "Match-Metadaten extrahiert: Format {0}, Start {1}, Ende {2}, Dauer {3}ms",
            ["GameModeDetected"] = "Spielmodus erkannt: {0}",
            ["SubmissionInfoExtracted"] = "Übertragungsinfo extrahiert: {0} v{1}",
            
            // Statistik-Berechnung
            ["StatisticsCalculated"] = "Statistiken berechnet für {0}: Avg {1}, {2} Würfe, {3} Punkte",
            ["PerformanceMetrics"] = "Leistungsmetriken: Durchschnitt pro Wurf: {0:F1}, HF-Rate: {1:F2}, Maximum-Rate: {2:F2}",
            ["DetailListsSizes"] = "Detail-Listen Größen: {0} HF, {1} Max, {2} Score26, {3} Checkouts",

            // Direkte WebSocket-Extraktion - NEU
            ["DirectWebSocketExtraction"] = "Direkte WebSocket-Statistik-Extraktion",
            ["NoValidJSONInWebSocket"] = "Keine gültigen JSON-Daten in WebSocket-Nachricht gefunden",
            ["ProcessingDirectWebSocketStats"] = "Verarbeite direkte WebSocket-Statistiken",
            ["NoDirectStatsFound"] = "Keine direkten Statistiken in WebSocket-Nachricht gefunden",
            ["FoundDirectStatsInWebSocket"] = "Direkte Statistiken in WebSocket-Nachricht gefunden",
            ["NoValidDirectWebSocketData"] = "Keine gültigen direkten WebSocket-Statistikdaten gefunden",
            ["DirectWebSocketExtractionSuccess"] = "Direkte WebSocket-Extraktion erfolgreich: {0} vs {1}",
            ["ErrorParsingDirectWebSocketStats"] = "Fehler beim Parsen direkter WebSocket-Statistiken: {0}",
            ["DirectWebSocketPlayer1"] = "Direkter WebSocket Player1: {0} - Avg {1}, 180s: {2}, HF: {3}, 26s: {4}, Checkouts: {5}",
            ["DirectWebSocketPlayer2"] = "Direkter WebSocket Player2: {0} - Avg {1}, 180s: {2}, HF: {3}, 26s: {4}, Checkouts: {5}",
            ["DirectWebSocketMatchDuration"] = "Direkte WebSocket Match-Dauer: {0}ms = {1}",
            ["ProcessingDirectWebSocketStatsFor"] = "Verarbeite direkte WebSocket-Statistiken für {0} vs {1}"
        };
    }
}