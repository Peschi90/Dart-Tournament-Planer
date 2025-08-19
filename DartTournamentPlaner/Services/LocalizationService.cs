using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Service zur Verwaltung der Lokalisierung/Übersetzung der Anwendung
/// Unterstützt mehrere Sprachen (derzeit Deutsch und Englisch)
/// Implementiert INotifyPropertyChanged für UI-Updates bei Sprachwechsel
/// </summary>
public class LocalizationService : INotifyPropertyChanged
{
    // Dictionary mit allen verfügbaren Übersetzungen
    // Erste Ebene: Sprachcodes (de, en)
    // Zweite Ebene: Übersetzungsschlüssel -> Übersetzter Text
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    
    // Aktuelle Sprache (Standard: Deutsch)
    private string _currentLanguage = "de";

    /// <summary>
    /// Initialisiert den LocalizationService mit allen verfügbaren Übersetzungen
    /// Lädt deutsche und englische Sprachressourcen
    /// </summary>
    public LocalizationService()
    {
        // Initialisierung aller Übersetzungen für unterstützte Sprachen
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            // Deutsche Übersetzungen
            ["de"] = new Dictionary<string, string>
            {
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
                ["Score"] = "Punkte",
                
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
                ["TournamentOverview"] = "📺 Turnier-Übersicht",
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
                ["TournamentName"] = "🏆 Turnier:",
                ["CurrentPhase"] = "🎯 Aktuelle Phase:",
                ["GroupsCount"] = "👥 Gruppen:",
                ["PlayersTotal"] = "🎮 Gesamtspieler:",
                ["GameRulesColon"] = "📋 Spielregeln:",
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
                ["AboutText"] = "Dart Tournament Planner v1.0\n\nEine moderne Turnierverwaltungssoftware.",
                ["ErrorSavingData"] = "Fehler beim Speichern der Daten:",

                // Nachrichten für Turnier-Tab
                ["MinimumTwoPlayers"] = "Mindestens 2 Spieler erforderlich.",
                ["ErrorGeneratingMatches"] = "Fehler beim Erstellen der Spiele:",
                ["MatchesGeneratedSuccess"] = "Spiele wurden erfolgreich erstellt!",
                ["MatchesResetSuccess"] = "Spiele wurden zurückgesetzt!",
                ["ResetTournamentConfirm"] = "Möchten Sie wirklich das gesamte Turnier zurücksetzen?\n\n⚠ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.",
                ["TournamentResetComplete"] = "Das Turnier wurde erfolgreich zurückgesetzt.",
                ["ResetKnockoutConfirm"] = "Möchten Sie wirklich die K.-o.-Phase zurücksetzen?\n\n⚠ Alle K.-o.-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.",
                ["ResetKnockoutComplete"] = "Die K.-o.-Phase wurde erfolgreich zurückgesetzt.",
                ["ResetFinalsConfirm"] = "Möchten Sie wirklich die Finalrunde zurücksetzen?\n\n⚠ Alle Finalspiele werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.",
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
                ["BugReport"] = "🐛 Report Bug",
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
            },
            // Englische Übersetzungen
            ["en"] = new Dictionary<string, string>
            {
                // Kontext-Menü spezifische Übersetzungen
                ["EditResult"] = "Edit Result",
                ["AutomaticBye"] = "Automatic Bye",
                ["UndoByeShort"] = "Undo Bye",
                ["NoActionsAvailable"] = "No actions available",
                ["ByeToPlayer"] = "Bye to {0}",
                
                // Hauptfenster
                ["AppTitle"] = "Dart Tournament Planner",
                ["Platinum"] = "Platinum",
                ["Gold"] = "Gold",
                ["Silver"] = "Silver",
                ["Bronze"] = "Bronze",
                
                // Turnier-Tab Übersetzungen
                ["SetupTab"] = "Tournament Setup",
                ["GroupPhaseTab"] = "Group Phase",
                ["FinalsTab"] = "Final Round",
                ["KnockoutTab"] = "KO Round",
                ["Groups"] = "Groups:",
                ["Players"] = "Players:",
                ["AddGroup"] = "Add Group",
                ["RemoveGroup"] = "Remove Group",
                ["AddPlayer"] = "Add Player",
                ["RemovePlayer"] = "Remove Player",
                ["NewGroup"] = "New Group",
                ["GroupName"] = "Enter the name of the new group:",
                ["RemoveGroupConfirm"] = "Do you really want to remove the group '{0}'?\nAll players in this group will also be removed.",
                ["RemoveGroupTitle"] = "Remove Group",
                ["RemovePlayerConfirm"] = "Do you really want to remove the player '{0}'?",
                ["RemovePlayerTitle"] = "Remove Player",
                ["NoGroupSelected"] = "Please select a group to remove.",
                ["NoGroupSelectedTitle"] = "No Group Selected",
                ["NoPlayerSelected"] = "Please select a player to remove.",
                ["NoPlayerSelectedTitle"] = "No Player Selected",
                ["SelectGroupFirst"] = "Please select a group first.",
                ["EnterPlayerName"] = "Please enter a player name.",
                ["NoNameEntered"] = "No name entered",
                ["PlayersInGroup"] = "Players in {0}:",
                ["NoGroupSelectedPlayers"] = "Players: (No group selected)",
                ["Group"] = "Group {0}",
                ["AdvanceToNextPhase"] = "Advance to Next Phase",
                ["ResetTournament"] = "Reset Tournament",
                ["ResetKnockoutPhase"] = "Reset KO Phase",
                ["ResetFinalsPhase"] = "Reset Finals",
                ["RefreshUI"] = "Refresh UI",
                ["RefreshUITooltip"] = "Refreshes the user interface",
                
                // Turnierprozessphasen
                ["GroupPhase"] = "Group Phase",
                ["FinalsPhase"] = "Finals",
                ["KnockoutPhase"] = "KO Phase",
                
                // Spielregeln
                ["GameRules"] = "Game Rules",
                ["GameMode"] = "Game Mode",
                ["Points501"] = "501 Points",
                ["Points401"] = "401 Points",
                ["Points301"] = "301 Points",
                ["FinishMode"] = "Finish Mode",
                ["SingleOut"] = "Single Out",
                ["DoubleOut"] = "Double Out",
                ["LegsToWin"] = "Legs to Win",
                ["PlayWithSets"] = "Play with Sets",
                ["SetsToWin"] = "Sets to Win",
                ["LegsPerSet"] = "Legs per Set",
                ["ConfigureRules"] = "Configure Rules",
                ["RulesPreview"] = "Rules Preview",
                ["AfterGroupPhaseHeader"] = "After Group Phase",

                // Einstellungen nach der Gruppenphase
                ["PostGroupPhase"] = "After Group Phase",
                ["PostGroupPhaseMode"] = "Mode after group phase",
                ["PostGroupPhaseNone"] = "Group phase only",
                ["PostGroupPhaseRoundRobin"] = "Finals (Round Robin)",
                ["PostGroupPhaseKnockout"] = "KO System",
                ["QualifyingPlayersPerGroup"] = "Qualifiers per group",
                ["KnockoutMode"] = "KO Mode", 
                ["SingleElimination"] = "Single Elimination",
                ["DoubleElimination"] = "Double Elimination (Winner + Loser Bracket)",
                ["IncludeGroupPhaseLosersBracket"] = "Include group phase losers in loser bracket",
                
                // Rundenspezifische Regeln
                ["RoundSpecificRules"] = "Round Specific Rules",
                ["ConfigureRoundRules"] = "Configure Round Rules",
                ["WinnerBracketRules"] = "Winner Bracket Rules",
                ["LoserBracketRules"] = "Loser Bracket Rules",
                ["RoundRulesFor"] = "Rules for {0}",
                ["DefaultRules"] = "Default Rules",
                ["ResetToDefault"] = "Reset to Default",
                ["RoundRulesConfiguration"] = "Round Rules Configuration",
                ["Best64Rules"] = "Best 64 Rules",
                ["Best32Rules"] = "Best 32 Rules",
                ["Best16Rules"] = "Best 16 Rules",
                ["QuarterfinalRules"] = "Quarterfinal Rules",
                ["SemifinalRules"] = "Semifinal Rules",
                ["FinalRules"] = "Final Rules",
                ["GrandFinalRules"] = "Grand Final Rules",
                
                // Individual round names for GetRoundDisplayName
                ["Best64"] = "Best 64",
                ["Best32"] = "Best 32", 
                ["Best16"] = "Best 16",
                ["Quarterfinal"] = "Quarterfinal",
                ["Semifinal"] = "Semifinal",
                ["Final"] = "Final",
                ["GrandFinal"] = "Grand Final",
                ["LoserBracket"] = "Loser Bracket",

                // Spiele und Match-Management
                ["Matches"] = "Matches:",
                ["Standings"] = "Standings:",
                ["GenerateMatches"] = "Generate Matches",
                ["MatchesGenerated"] = "Matches have been successfully generated!",
                ["ResetMatches"] = "Reset Matches",
                ["ResetMatchesConfirm"] = "Do you really want to reset all matches for group '{0}'?\nAll results will be lost!",
                ["ResetMatchesTitle"] = "Reset Matches",
                ["MatchesReset"] = "Matches have been reset!",
                ["EnterResult"] = "Enter Result",
                ["MatchNotStarted"] = "Not Started",
                ["MatchInProgress"] = "In Progress",
                ["MatchFinished"] = "Finished",
                ["MatchBye"] = "Bye",
                ["Round"] = "Round",
                ["Sets"] = "Sets",
                ["Legs"] = "Legs",
                ["Score"] = "Score",
                
                // Anwendungseinstellungen
                ["Settings"] = "Settings",
                ["Language"] = "Language",
                ["Theme"] = "Theme",
                ["AutoSave"] = "Auto Save",
                ["AutoSaveInterval"] = "Save Interval (Minutes)",
                ["Save"] = "Save",
                ["Cancel"] = "Cancel",
                
                // Menü-Einträge
                ["File"] = "File",
                ["New"] = "New",
                ["Open"] = "Open",
                ["SaveAs"] = "Save As",
                ["Exit"] = "Exit",
                ["Edit"] = "Edit",
                ["View"] = "View",
                ["Help"] = "Help",
                ["About"] = "About",
                
                // Status-Anzeigen
                ["HasUnsavedChanges"] = "Modified",
                ["NotSaved"] = "Not Saved",
                ["Saved"] = "Saved",
                ["Ready"] = "Ready",
                
                // Allgemeine UI-Elemente
                ["Close"] = "Close",
                ["OK"] = "OK",
                ["Start"] = "Start",
                ["Stop"] = "Stop",
                ["Player"] = "Player",
                ["Match"] = "Match",
                ["Result"] = "Result",
                ["Status"] = "Status",
                ["Position"] = "Position",
                ["Winner"] = "Winner",
                ["Information"] = "Information",
                ["Warning"] = "Warning",
                ["Error"] = "Error",
                
                // Hilfesystem
                ["HelpTitle"] = "Help - Dart Tournament Planner",
                ["HelpGeneral"] = "General Usage",
                ["HelpTournamentSetup"] = "Tournament Setup",
                ["HelpGroupManagement"] = "Group Management", 
                ["HelpGameRules"] = "Game Rules",
                ["HelpMatches"] = "Matches & Results",
                ["HelpTournamentPhases"] = "Tournament Phases",
                ["HelpMenus"] = "Menus & Features",
                ["HelpTips"] = "Tips & Tricks",
                
                // Ausführliche Hilfe-Inhalte
                ["HelpGeneralContent"] = "The Dart Tournament Planner helps you manage dart tournaments with up to 4 different classes (Platinum, Gold, Silver, Bronze).\n\n" +
                    "• Use the tabs above to switch between classes\n" +
                    "• All changes are automatically saved (if enabled)\n" +
                    "• The status bar shows the current save status\n" +
                    "• Language can be changed in the settings",
                
                ["HelpTournamentSetupContent"] = "To set up a new tournament:\n\n" +
                    "1. Select a tournament class (Platinum, Gold, Silver, Bronze)\n" +
                    "2. Click 'Add Group' to create groups\n" +
                    "3. Add players to the groups\n" +
                    "4. Configure the game rules using the 'Configure Rules' button\n" +
                    "5. Set the mode after the group phase (Group phase only, Finals, KO system)\n\n" +
                    "Tip: At least 2 players per group are required for match generation.",
                
                ["HelpGroupManagementContent"] = "Group Management:\n\n" +
                    "• 'Add Group': Creates a new group\n" +
                    "• 'Remove Group': Deletes the selected group (warning appears)\n" +
                    "• 'Add Player': Adds a player to the selected group\n" +
                    "• 'Remove Player': Removes the selected player\n\n" +
                    "The player list shows all players in the currently selected group.\n" +
                    "Groups can be named arbitrarily and should have meaningful names.",
                
                ["HelpGameRulesContent"] = "Configure game rules:\n\n" +
                    "• Game mode: 301, 401, or 501 points\n" +
                    "• Finish mode: Single Out or Double Out\n" +
                    "• Legs to win: Number of legs for a win\n" +
                    "• Play with sets: Activates the set system\n" +
                    "• Sets to win: Number of sets for a tournament win\n" +
                    "• Legs per set: Number of legs per set\n\n" +
                    "Different rules can be set for different tournament rounds.",
                
                ["HelpMatchesContent"] = "Manage matches:\n\n" +
                    "• 'Generate Matches': Creates all matches for the group (Round-Robin)\n" +
                    "• Click on a match to enter the result\n" +
                    "• Status: Not started (gray), In progress (yellow), Finished (green)\n" +
                    "• Right-click on matches for more options (Bye, etc.)\n" +
                    "• 'Reset Matches': Deletes all results for the group\n\n" +
                    "The standings automatically show the current ranking of all players.",
                
                ["HelpTournamentPhasesContent"] = "Tournament phases:\n\n" +
                    "1. Group Phase: Round-Robin within each group\n" +
                    "2. After the group phase (optional):\n" +
                    "   • Group phase only: Tournament ends after the groups\n" +
                    "   • Finals: Top players play Round-Robin\n" +
                    "   • KO System: Single or double elimination\n\n" +
                    "The 'Advance to Next Phase' button becomes available when all matches are finished.\n" +
                    "KO system can have winner bracket and loser bracket.",
                
                ["HelpMenusContent"] = "Menu functions:\n\n" +
                    "File:\n• New: Creates a new empty tournament\n• Open/Save: Loads/Saves tournament data\n• Exit: Closes the application\n\n" +
                    "View:\n• Tournament Overview: Shows a fullscreen view of all classes\n\n" +
                    "Settings:\n• Language, theme, and auto-save settings\n\n" +
                    "Help:\n• This help page\n• About dialog with version information",
                
                ["HelpTipsContent"] = "Tips & Tricks:\n\n" +
                    "• Use meaningful group names (e.g. 'Group A', 'Beginners')\n" +
                    "• Enable auto-save in settings\n" +
                    "• The tournament overview is perfect for projector presentations\n" +
                    "• Right-clicking on matches shows additional options\n" +
                    "• With an odd number of players, a bye is automatically assigned\n" +
                    "• Sets and legs are automatically validated\n" +
                    "• Different rules for different tournament rounds are possible\n" +
                    "• Export/Import tournament data via the file menu",
                
                // Turnierübersicht-spezifische Übersetzungen
                ["TournamentOverview"] = "📺 Tournament Overview",
                ["OverviewMode"] = "Overview Mode",
                ["Configure"] = "Configure",
                ["ManualMode"] = "Manual Mode",
                ["AutoCyclingActive"] = "Auto Cycling Active",
                ["CyclingStopped"] = "Auto Cycling Stopped",
                ["ManualControl"] = "Manual Control",
                ["Showing"] = "Showing",

                // Spielstatus
                ["Unknown"] = "Unknown",

                // Weitere Turnierübersicht Begriffe
                ["StartCycling"] = "Start",
                ["StopCycling"] = "Stop",
                ["WinnerBracketMatches"] = "Matches in Winner Bracket",
                ["WinnerBracketTree"] = "Winner Bracket",
                ["LoserBracketMatches"] = "Matches in Loser Bracket",
                ["LoserBracketTree"] = "Loser Bracket",
                ["RoundColumn"] = "Round",
                ["PositionShort"] = "Pos",
                ["PointsShort"] = "Pts",
                ["WinDrawLoss"] = "W-D-L",
                ["NoLoserBracketMatches"] = "No matches available in loser bracket",
                ["NoWinnerBracketMatches"] = "No matches available in winner bracket",
                ["TournamentTreeWillShow"] = "The tournament tree will be displayed once the knockout phase begins",

                // Zusätzliche Gruppenphasenbegriffe
                ["SelectGroup"] = "Select Group",
                ["NoGroupSelected"] = "No group selected",

                // Übersichtskonfigurationsdialog
                ["OverviewConfiguration"] = "Overview Configuration",
                ["TournamentOverviewConfiguration"] = "Tournament Overview Configuration",
                ["TimeBetweenClasses"] = "Time between tournament classes:",
                ["TimeBetweenSubTabs"] = "Time between sub-tabs:",
                ["Seconds"] = "Seconds",
                ["ShowOnlyActiveClassesText"] = "Show only classes with active groups",
                ["OverviewInfoText"] = "Live tournament display for all classes with automatic switching",
                ["InvalidClassInterval"] = "Invalid class interval. Please enter a number ≥ 1.",
                ["InvalidSubTabInterval"] = "Invalid sub-tab interval. Please enter a number ≥ 1.",

                // Turnierübersicht Texte
                ["TournamentName"] = "🏆 Tournament:",
                ["CurrentPhase"] = "🎯 Current Phase:",
                ["GroupsCount"] = "👥 Groups:",
                ["PlayersTotal"] = "🎮 Total Players:",
                ["GameRulesColon"] = "📋 Game Rules:",
                ["CompletedGroups"] = "✅ Completed Groups:",
                ["QualifiedPlayers"] = "🏅 Qualified Players:",
                ["KnockoutMatches"] = "⚔️ KO Matches:",
                ["Completed"] = "completed",

                // Weitere fest codierte Texte
                ["Finalists"] = "Finalists",
                ["KnockoutParticipants"] = "KO Participants",
                ["PlayersText"] = "Players",
                ["OverviewModeTitle"] = "Tournament Overview Mode",
                ["NewTournament"] = "New Tournament",
                ["CreateNewTournament"] = "Create new tournament? Unsaved changes will be lost.",
                ["UnsavedChanges"] = "Unsaved changes",
                ["SaveBeforeExit"] = "There are unsaved changes. Would you like to save before exiting?",
                ["CustomFileNotImplemented"] = "Custom file loading not implemented yet.",
                ["CustomFileSaveNotImplemented"] = "Custom file saving not implemented yet.",
                ["ErrorOpeningHelp"] = "Error opening help:",
                ["ErrorOpeningOverview"] = "Error opening tournament overview:",
                ["AboutText"] = "Dart Tournament Planner\n\nA modern tournament management software.",
                ["ErrorSavingData"] = "Error saving data:",

                // Nachrichten für Turnier-Tab
                ["MinimumTwoPlayers"] = "At least 2 players required.",
                ["ErrorGeneratingMatches"] = "Error creating matches:",
                ["MatchesGeneratedSuccess"] = "Matches have been successfully created!",
                ["MatchesResetSuccess"] = "Matches have been reset!",
                ["ResetTournamentConfirm"] = "Do you really want to reset the entire tournament?\n\n⚠ ALL matches and phases will be deleted!\nOnly groups and players will be kept.",
                ["TournamentResetComplete"] = "The tournament has been successfully reset.",
                ["ResetKnockoutConfirm"] = "Do you really want to reset the knockout phase?\n\n⚠ All KO matches and the tournament tree will be deleted!\nThe tournament will be reset to the group phase.",
                ["ResetKnockoutComplete"] = "The knockout phase has been successfully reset.",
                ["ResetFinalsConfirm"] = "Do you really want to reset the finals?\n\n⚠ All final matches will be deleted!\nThe tournament will be reset to the group phase.",
                ["ResetFinalsComplete"] = "The finals have been successfully reset.",
                ["ErrorResettingTournament"] = "Error resetting tournament:",
                ["CannotAdvancePhase"] = "All matches in the current phase must be completed",
                ["ErrorAdvancingPhase"] = "Error advancing to the next phase:",
                ["UIRefreshed"] = "User interface has been refreshed",
                ["ErrorRefreshing"] = "Error refreshing:",
                ["KOPhaseActiveMSB"] = "Knockout phase is not active",
                ["KOPhaseNotEnoughUserMSB"] = "Not enough participants for the knockout phase (at least 2 required)",

                // Meldungstitel
                ["KOPhaseUsrWarnTitel"] = "Knockout Phase Warning",

                // Tab-Kopfzeilen für Spieleransicht
                ["FinalistsCount"] = "Finalists ({0} Players):",
                ["KnockoutParticipantsCount"] = "KO Participants ({0} Players):",

                // Weitere Phasen-Texte
                ["NextPhaseStart"] = "Start {0}",

                // Match-Ergebnisfenster
                ["EnterMatchResult"] = "Enter Match Result",
                ["SaveResult"] = "Save Result",
                ["Notes"] = "Notes",
                ["InvalidNumbers"] = "Invalid Numbers",
                ["NegativeValues"] = "Negative values are not allowed",
                ["InvalidSetCount"] = "Invalid set count. Maximum: {0}, Total: {1}",
                ["BothPlayersWon"] = "Both players cannot win at the same time",
                ["MatchIncomplete"] = "The match is not yet completed",
                ["InsufficientLegsForSet"] = "{0} does not have enough legs for the won sets. Minimum: {1}",
                ["ExcessiveLegs"] = "Too many legs for the set combination {0}:{1}. Maximum: {2}",
                ["LegsExceedSetRequirement"] = "{0} has more legs than required for the sets",
                ["InvalidLegCount"] = "Invalid leg count. Maximum: {0}, Total: {1}",
                ["SaveBlocked"] = "Save blocked",
                ["ValidationError"] = "Validation error",
                ["NoWinnerFound"] = "No winner found",
                ["GiveBye"] = "Assign Bye",
                ["SelectByeWinner"] = "Select the player to receive the bye:",

                // Eingabedialog
                ["InputDialog"] = "Input",
                ["EnterName"] = "Enter name:",

                // Spenden- und Bug-Report-Funktionen
                ["Donate"] = "💝",
                ["DonateTooltip"] = "Support the development of this project",
                ["BugReport"] = "🐛 Report Bug",
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
                ["Loading"] = "Loading...",
                ["CheckingGroupStatus"] = "Checking group status...",
                ["ProcessingMatches"] = "Processing matches...",
                ["CheckingCompletion"] = "Checking completion...",

                // Startup und Update-Funktionen
                ["StartingApplication"] = "Starting application...",
                ["AppSubtitle"] = "Modern tournament management",
                ["CheckingForUpdates"] = "Checking for updates...",
                ["ConnectingToGitHub"] = "Connecting to GitHub...",
                ["AnalyzingReleases"] = "Analyzing releases...",
                ["UpdateAvailable"] = "Update available",
                ["WhatsNew"] = "What's new:",
                ["RemindLater"] = "Remind me later",
                ["SkipVersion"] = "Skip version",
                ["DownloadUpdate"] = "Download now",
                ["DownloadAndInstall"] = "Download & Install",
                ["DownloadingUpdate"] = "Downloading update",
                ["PreparingDownload"] = "Preparing download...",
                ["DownloadingSetup"] = "Downloading setup...",
                ["DownloadCompleted"] = "Download completed, checking file...",
                ["PreparingInstallation"] = "Preparing installation...",
                ["StartingInstallation"] = "Starting installation...",
                ["InstallationStarted"] = "Installation started",
                ["InstallationCancelled"] = "Installation cancelled",
                ["ErrorStartingSetup"] = "Error starting setup",
                ["AdminRightsRequired"] = "Administrator rights required",
                ["NoUpdateAvailable"] = "No updates available",
            }
        };
    }

    /// <summary>
    /// Gibt den übersetzten Text für den angegebenen Schlüssel und die aktuelle Sprache zurück
    /// Fallback auf den Schlüssel selbst, wenn keine Übersetzung gefunden wird
    /// </summary>
    /// <param name="key">Übersetzungsschlüssel</param>
    /// <returns>Übersetzter Text</returns>
    public string GetTranslation(string key)
    {
        // Überprüfen, ob der Schlüssel in der aktuellen Sprache vorhanden ist
        if (_translations.TryGetValue(_currentLanguage, out var translations)
            && translations.TryGetValue(key, out var translation))
        {
            return translation;
        }

        // Fallback: Schlüssel selbst zurückgeben
        return key;
    }

    /// <summary>
    /// Alias für GetTranslation - für Kompatibilität mit bestehendem Code
    /// </summary>
    /// <param name="key">Übersetzungsschlüssel</param>
    /// <returns>Übersetzter Text</returns>
    public string GetString(string key)
    {
        return GetTranslation(key);
    }

    /// <summary>
    /// Gibt formatierten übersetzten Text zurück mit Platzhaltern
    /// </summary>
    /// <param name="key">Übersetzungsschlüssel</param>
    /// <param name="args">Parameter für string.Format</param>
    /// <returns>Formatierter übersetzter Text</returns>
    public string GetString(string key, params object[] args)
    {
        var template = GetTranslation(key);
        try
        {
            return string.Format(template, args);
        }
        catch
        {
            // Fallback bei Format-Fehlern
            return template;
        }
    }

    /// <summary>
    /// Ändert die aktuelle Sprache und löst die Aktualisierung der UI-Elemente aus
    /// </summary>
    /// <param name="newLanguage">Neuer Sprachcode (z.B. 'de' für Deutsch)</param>
    public void ChangeLanguage(string newLanguage)
    {
        if (_currentLanguage != newLanguage && _translations.ContainsKey(newLanguage))
        {
            _currentLanguage = newLanguage;
            OnPropertyChanged(nameof(CurrentLanguage));
            
            // Weitere UI-Aktualisierungen können hier ausgelöst werden
        }
    }

    /// <summary>
    /// Alias für ChangeLanguage - für Kompatibilität mit bestehendem Code
    /// </summary>
    /// <param name="newLanguage">Neuer Sprachcode (z.B. 'de' für Deutsch)</param>
    public void SetLanguage(string newLanguage)
    {
        ChangeLanguage(newLanguage);
    }

    public string CurrentLanguage => _currentLanguage;

    /// <summary>
    /// Gibt die aktuellen Übersetzungen zurück - für Kompatibilität mit bestehendem Code
    /// </summary>
    public Dictionary<string, string> CurrentTranslations => _translations[_currentLanguage];

    // INotifyPropertyChanged-Implementierung
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}