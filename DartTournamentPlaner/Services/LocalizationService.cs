using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Services;

public class LocalizationService : INotifyPropertyChanged
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private string _currentLanguage = "de";

    public LocalizationService()
    {
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["de"] = new Dictionary<string, string>
            {
                // Context Menu specific translations
                ["EditResult"] = "Ergebnis bearbeiten",
                ["AutomaticBye"] = "Automatisches Freilos",
                ["UndoByeShort"] = "Freilos rückgängig machen",
                ["NoActionsAvailable"] = "Keine Aktionen verfügbar",
                ["ByeToPlayer"] = "Freilos an {0}",
                
                // Main Window
                ["AppTitle"] = "Dart Turnier Planer",
                ["Platinum"] = "Platin",
                ["Gold"] = "Gold",
                ["Silver"] = "Silber",
                ["Bronze"] = "Bronze",
                
                // Tournament Tab
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
                
                // Phases
                ["GroupPhase"] = "Gruppenphase",
                ["FinalsPhase"] = "Finalrunde",
                ["KnockoutPhase"] = "KO-Phase",
                
                // Game Rules
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

                // Post-Group Phase Settings
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
                
                // Round-specific Rules
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
                ["LoserRound1Rules"] = "LR1 Regeln",
                ["LoserRound2Rules"] = "LR2 Regeln",
                ["LoserRound3Rules"] = "LR3 Regeln",
                ["LoserRound4Rules"] = "LR4 Regeln",
                ["LoserRound5Rules"] = "LR5 Regeln",
                ["LoserRound6Rules"] = "LR6 Regeln",
                ["LoserRound7Rules"] = "LR7 Regeln",
                ["LoserRound8Rules"] = "LR8 Regeln",
                ["LoserRound9Rules"] = "LR9 Regeln",
                ["LoserRound10Rules"] = "LR10 Regeln",
                ["LoserRound11Rules"] = "LR11 Regeln",
                ["LoserRound12Rules"] = "LR12 Regeln",
                ["LoserFinalRules"] = "Loser Final Regeln",

                // Matches
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
                
                // Settings
                ["Settings"] = "Einstellungen",
                ["Language"] = "Sprache",
                ["Theme"] = "Design",
                ["AutoSave"] = "Automatisches Speichern",
                ["AutoSaveInterval"] = "Speicherintervall (Minuten)",
                ["Save"] = "Speichern",
                ["Cancel"] = "Abbrechen",
                
                // Menu
                ["File"] = "Datei",
                ["New"] = "Neu",
                ["Open"] = "Öffnen",
                ["SaveAs"] = "Speichern unter",
                ["Exit"] = "Beenden",
                ["Edit"] = "Bearbeiten",
                ["View"] = "Ansicht",
                ["Help"] = "Hilfe",
                ["About"] = "Über",
                
                // Status
                ["HasUnsavedChanges"] = "Geändert",
                ["NotSaved"] = "Nicht gespeichert",
                ["Saved"] = "Gespeichert",
                ["Ready"] = "Bereit",
                
                // Common
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
                
                // Help System
                ["HelpTitle"] = "Hilfe - Dart Turnier Planer",
                ["HelpGeneral"] = "Allgemeine Bedienung",
                ["HelpTournamentSetup"] = "Turnier-Setup",
                ["HelpGroupManagement"] = "Gruppenverwaltung", 
                ["HelpGameRules"] = "Spielregeln",
                ["HelpMatches"] = "Spiele & Ergebnisse",
                ["HelpTournamentPhases"] = "Turnierphasen",
                ["HelpMenus"] = "Menüs & Funktionen",
                ["HelpTips"] = "Tipps & Tricks",
                
                // Help Content
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
                
                // Turnierübersicht
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

                // Turnierübersicht spezifisch
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
                ["OverviewInfoText"] = "Die Übersicht wechselt automatisch endlos zwischen den Turnierklassen und deren Gruppen/Bäumen. Sie können dieses Fenster auf einen zweiten Monitor verschieben.",
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

                // Donation & Bug Report
                ["Donate"] = "💝 Donate",
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

                // Loading Spinner
                ["Loading"] = "Wird geladen...",
                ["CheckingGroupStatus"] = "Überprüfe Gruppenstatus...",
                ["ProcessingMatches"] = "Verarbeite Spiele...",
                ["CheckingCompletion"] = "Überprüfe Abschluss...",

                // Tournament Tree Messages
                ["NoLoserBracketSingleElimination"] = "Kein Loser Bracket (Single Elimination)",
                ["NoLoserBracketGames"] = "Keine Loser Bracket Spiele vorhanden",
                ["NoWinnerBracketGames"] = "Keine Winner Bracket Spiele vorhanden",
                ["InteractiveTournamentTree"] = "Interaktiver Turnierbaum wird über TournamentClass erstellt",
                ["TBD"] = "TBD",
                ["Versus"] = "vs",
                ["AllGroupsCompleted"] = "🎉 Alle Gruppen sind abgeschlossen!\n\nSie können jetzt zur nächsten Phase wechseln.",
                ["FinalsCompleted"] = "🏆 Die Finalrunde ist abgeschlossen!\n\nAlle Spiele wurden beendet. Das Turnier ist komplett!",
                
                // Loading Progress Messages
                ["CheckingFinalsStatus"] = "Überprüfe Finalrunden-Status...",
                ["ProcessingFinalsMatches"] = "Verarbeite Finals-Spiele...",
            },
            ["en"] = new Dictionary<string, string>
            {
                // Context Menu specific translations
                ["EditResult"] = "Edit Result",
                ["AutomaticBye"] = "Automatic Bye",
                ["UndoByeShort"] = "Undo Bye",
                ["NoActionsAvailable"] = "No actions available",
                ["ByeToPlayer"] = "Bye to {0}",
                
                // Main Window
                ["AppTitle"] = "Dart Tournament Planner",
                ["Platinum"] = "Platinum",
                ["Gold"] = "Gold",
                ["Silver"] = "Silver",
                ["Bronze"] = "Bronze",
                
                // Tournament Tab
                ["SetupTab"] = "Tournament Setup",
                ["GroupPhaseTab"] = "Group Phase",
                ["FinalsTab"] = "Finals",
                ["KnockoutTab"] = "Knockout Round",
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
                ["ResetKnockoutPhase"] = "Reset Knockout Phase",
                ["ResetFinalsPhase"] = "Reset Finals",
                ["RefreshUI"] = "Refresh UI",
                ["RefreshUITooltip"] = "Refreshes the user interface",
                
                // Phases
                ["GroupPhase"] = "Group Phase",
                ["FinalsPhase"] = "Finals",
                ["KnockoutPhase"] = "Knockout Phase",
                
                // Game Rules
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

                // Post-Group Phase Settings
                ["PostGroupPhase"] = "After Group Phase",
                ["PostGroupPhaseMode"] = "Mode after group phase",
                ["PostGroupPhaseNone"] = "Group phase only",
                ["PostGroupPhaseRoundRobin"] = "Finals (Round Robin)",
                ["PostGroupPhaseKnockout"] = "Knockout System",
                ["QualifyingPlayersPerGroup"] = "Qualifiers per group",
                ["KnockoutMode"] = "Knockout Mode", 
                ["SingleElimination"] = "Single Elimination",
                ["DoubleElimination"] = "Double Elimination (Winner + Loser Bracket)",
                ["IncludeGroupPhaseLosersBracket"] = "Include group phase losers in loser bracket",
                
                // Round-specific Rules
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
                ["LoserRound1Rules"] = "LR1 Rules",
                ["LoserRound2Rules"] = "LR2 Rules",
                ["LoserRound3Rules"] = "LR3 Rules",
                ["LoserRound4Rules"] = "LR4 Rules",
                ["LoserRound5Rules"] = "LR5 Rules",
                ["LoserRound6Rules"] = "LR6 Rules",
                ["LoserRound7Rules"] = "LR7 Rules",
                ["LoserRound8Rules"] = "LR8 Rules",
                ["LoserRound9Rules"] = "LR9 Rules",
                ["LoserRound10Rules"] = "LR10 Rules",
                ["LoserRound11Rules"] = "LR11 Rules",
                ["LoserRound12Rules"] = "LR12 Rules",
                ["LoserFinalRules"] = "Loser Final Rules",

                // Matches
                ["Matches"] = "Matches:",
                ["Standings"] = "Standings:",
                ["GenerateMatches"] = "Generate Matches",
                ["MatchesGenerated"] = "Matches were successfully generated!",
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
                
                // Settings
                ["Settings"] = "Settings",
                ["Language"] = "Language",
                ["Theme"] = "Theme",
                ["AutoSave"] = "Auto Save",
                ["AutoSaveInterval"] = "Save Interval (Minutes)",
                ["Save"] = "Save",
                ["Cancel"] = "Cancel",
                
                // Menu
                ["File"] = "File",
                ["New"] = "New",
                ["Open"] = "Open",
                ["SaveAs"] = "Save As",
                ["Exit"] = "Exit",
                ["Edit"] = "Edit",
                ["View"] = "View",
                ["Help"] = "Help",
                ["About"] = "About",
                
                // Status
                ["HasUnsavedChanges"] = "Modified",
                ["NotSaved"] = "Not Saved",
                ["Saved"] = "Saved",
                ["Ready"] = "Ready",
                
                // Common
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
                
                // Help System
                ["HelpTitle"] = "Help - Dart Tournament Planner",
                ["HelpGeneral"] = "General Operation",
                ["HelpTournamentSetup"] = "Tournament Setup",
                ["HelpGroupManagement"] = "Group Management", 
                ["HelpGameRules"] = "Game Rules",
                ["HelpMatches"] = "Matches & Results",
                ["HelpTournamentPhases"] = "Tournament Phases",
                ["HelpMenus"] = "Menus & Functions",
                ["HelpTips"] = "Tips & Tricks",
                
                // Help Content
                ["HelpGeneralContent"] = "The Dart Tournament Planner assists you in managing dart tournaments with up to 4 different classes (Platinum, Gold, Silver, Bronze).\n\n" +
                    "• Use the tabs above to switch between classes\n" +
                    "• All changes are saved automatically (if enabled)\n" +
                    "• The status bar shows the current save status\n" +
                    "• Language can be changed in the settings",
                
                ["HelpTournamentSetupContent"] = "To set up a new tournament:\n\n" +
                    "1. Select a tournament class (Platinum, Gold, Silver, Bronze)\n" +
                    "2. Click 'Add Group' to create groups\n" +
                    "3. Add players to the groups\n" +
                    "4. Configure the game rules using the 'Configure Rules' button\n" +
                    "5. Set the mode after the group phase (Group only, Finals, Knockout system)\n\n" +
                    "Tip: At least 2 players per group are required for match generation.",
                
                ["HelpGroupManagementContent"] = "Group Management:\n\n" +
                    "• 'Add Group': Creates a new group\n" +
                    "• 'Remove Group': Deletes the selected group (warning appears)\n" +
                    "• 'Add Player': Adds a player to the selected group\n" +
                    "• 'Remove Player': Removes the selected player\n\n" +
                    "The player list shows all players in the currently selected group.\n" +
                    "Groups can be named arbitrarily and should have descriptive names.",
                
                ["HelpGameRulesContent"] = "Configure game rules:\n\n" +
                    "• Game Mode: 301, 401, or 501 points\n" +
                    "• Finish Mode: Single Out or Double Out\n" +
                    "• Legs to Win: Number of legs for a win\n" +
                    "• Play with Sets: Enables the set system\n" +
                    "• Sets to Win: Number of sets for a tournament victory\n" +
                    "• Legs per Set: Number of legs per set\n\n" +
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
                    "2. After the Group Phase (optional):\n" +
                    "   • Group phase only: Tournament ends after the groups\n" +
                    "   • Finals: Top players play Round-Robin\n" +
                    "   • Knockout system: Single or double elimination\n\n" +
                    "The 'Advance to Next Phase' button becomes available when all matches are completed.\n" +
                    "Knockout system can have winner bracket and loser bracket.",
                
                ["HelpMenusContent"] = "Menu functions:\n\n" +
                    "File:\n• New: Creates a new empty tournament\n• Open/Save: Loads/Saves tournament data\n• Exit: Closes the application\n\n" +
                    "View:\n• Tournament Overview: Shows a fullscreen view of all classes\n\n" +
                    "Settings:\n• Language, theme, and auto-save settings\n\n" +
                    "Help:\n• This help page\n• About dialog with version information",
                
                ["HelpTipsContent"] = "Tips & Tricks:\n\n" +
                    "• Use descriptive group names (e.g. 'Group A', 'Beginners')\n" +
                    "• Enable auto-save in the settings\n" +
                    "• The tournament overview is perfect for projector presentations\n" +
                    "• Right-clicking on matches shows additional options\n" +
                    "• If the number of players is odd, a bye is automatically assigned\n" +
                    "• Sets and legs are automatically validated\n" +
                    "• Different rules for different tournament rounds are possible\n" +
                    "• Export/Import tournament data via the file menu",
                
                // Tournament Overview
                ["TournamentOverview"] = "📺 Tournament Overview",
                ["OverviewMode"] = "Overview Mode",
                ["Configure"] = "Configure",
                ["ManualMode"] = "Manual Mode",
                ["AutoCyclingActive"] = "Auto cycling active",
                ["CyclingStopped"] = "Auto cycling stopped",
                ["ManualControl"] = "Manual Control",
                ["Showing"] = "Showing",

                // Game Status
                ["Unknown"] = "Unknown",

                // Tournament Overview specific
                ["StartCycling"] = "Start",
                ["StopCycling"] = "Stop",
                ["WinnerBracketMatches"] = "Winner Bracket Matches",
                ["WinnerBracketTree"] = "Winner Bracket",
                ["LoserBracketMatches"] = "Loser Bracket Matches",
                ["LoserBracketTree"] = "Loser Bracket",
                ["RoundColumn"] = "Round",
                ["PositionShort"] = "Pos",
                ["PointsShort"] = "Pts",
                ["WinDrawLoss"] = "W-D-L",
                ["NoLoserBracketMatches"] = "No matches available in loser bracket",
                ["NoWinnerBracketMatches"] = "No matches available in winner bracket",
                ["TournamentTreeWillShow"] = "The tournament tree will be displayed as soon as the knockout phase begins",

                // Additional Group Phase terms
                ["SelectGroup"] = "Select Group",
                ["NoGroupSelected"] = "No Group Selected",

                // Overview Configuration Dialog
                ["OverviewConfiguration"] = "Overview Configuration",
                ["TournamentOverviewConfiguration"] = "Tournament Overview Configuration",
                ["TimeBetweenClasses"] = "Time between tournament classes:",
                ["TimeBetweenSubTabs"] = "Time between sub-tabs:",
                ["Seconds"] = "Seconds",
                ["ShowOnlyActiveClassesText"] = "Show only classes with active groups",
                ["OverviewInfoText"] = "The overview automatically cycles endlessly between tournament classes and their groups/trees. You can move this window to a second monitor.",
                ["InvalidClassInterval"] = "Invalid class interval. Please enter a number ≥ 1.",
                ["InvalidSubTabInterval"] = "Invalid sub-tab interval. Please enter a number ≥ 1.",

                // Tournament Overview Texts
                ["TournamentName"] = "🏆 Tournament:",
                ["CurrentPhase"] = "🎯 Current Phase:",
                ["GroupsCount"] = "👥 Groups:",
                ["PlayersTotal"] = "🎮 Total Players:",
                ["GameRulesColon"] = "📋 Game Rules:",
                ["CompletedGroups"] = "✅ Completed Groups:",
                ["QualifiedPlayers"] = "🏅 Qualified Players:",
                ["KnockoutMatches"] = "⚔️ Knockout Matches:",
                ["Completed"] = "completed",

                // Additional hardcoded texts
                ["Finalists"] = "Finalists",
                ["KnockoutParticipants"] = "Knockout Participants",
                ["PlayersText"] = "Players",
                ["OverviewModeTitle"] = "Tournament Overview Mode",
                ["NewTournament"] = "New Tournament",
                ["CreateNewTournament"] = "Create new tournament? Unsaved changes will be lost.",
                ["UnsavedChanges"] = "Unsaved Changes",
                ["SaveBeforeExit"] = "There are unsaved changes. Do you want to save before exiting?",
                ["CustomFileNotImplemented"] = "Custom file loading not yet implemented.",
                ["CustomFileSaveNotImplemented"] = "Custom file saving not yet implemented.",
                ["ErrorOpeningHelp"] = "Error opening help:",
                ["ErrorOpeningOverview"] = "Error opening tournament overview:",
                ["AboutText"] = "Dart Tournament Planner v1.0\n\nA modern tournament management software.",
                ["ErrorSavingData"] = "Error saving data:",

                // Messages for Tournament Tab
                ["MinimumTwoPlayers"] = "At least 2 players required.",
                ["ErrorGeneratingMatches"] = "Error generating matches:",
                ["MatchesGeneratedSuccess"] = "Matches were successfully generated!",
                ["MatchesResetSuccess"] = "Matches have been reset!",
                ["ResetTournamentConfirm"] = "Do you really want to reset the entire tournament?\n\n⚠ ALL matches and phases will be deleted!\nOnly groups and players will remain.",
                ["TournamentResetComplete"] = "The tournament has been successfully reset.",
                ["ResetKnockoutConfirm"] = "Do you really want to reset the knockout phase?\n\n⚠ All knockout matches and the tournament tree will be deleted!\nThe tournament will be reset to the group phase.",
                ["ResetKnockoutComplete"] = "The knockout phase has been successfully reset.",
                ["ResetFinalsConfirm"] = "Do you really want to reset the finals?\n\n⚠ All final matches will be deleted!\nThe tournament will be reset to the group phase.",
                ["ResetFinalsComplete"] = "The finals have been successfully reset.",
                ["ErrorResettingTournament"] = "Error resetting tournament:",
                ["CannotAdvancePhase"] = "All matches in the current phase must be completed",
                ["ErrorAdvancingPhase"] = "Error advancing to next phase:",
                ["UIRefreshed"] = "User interface has been updated",
                ["ErrorRefreshing"] = "Error refreshing:",
                ["KOPhaseActiveMSB"] = "Knockout phase is not active",
                ["KOPhaseNotEnoughUserMSB"] = "Not enough participants for knockout phase (at least 2 required)",

                // Message Titles
                ["KOPhaseUsrWarnTitel"] = "Knockout Phase Warning",

                // Tab Headers for Player View
                ["FinalistsCount"] = "Finalists ({0} players):",
                ["KnockoutParticipantsCount"] = "Knockout Participants ({0} players):",

                // Additional Phase Texts
                ["NextPhaseStart"] = "Start {0}",

                // Match Result Window
                ["EnterMatchResult"] = "Enter Match Result",
                ["SaveResult"] = "Save Result",
                ["Notes"] = "Notes",
                ["InvalidNumbers"] = "Invalid numbers",
                ["NegativeValues"] = "Negative values are not allowed",
                ["InvalidSetCount"] = "Invalid set count. Maximum: {0}, Total: {1}",
                ["BothPlayersWon"] = "Both players cannot win simultaneously",
                ["MatchIncomplete"] = "The match is not yet completed",
                ["InsufficientLegsForSet"] = "{0} does not have enough legs for the won sets. Minimum: {1}",
                ["ExcessiveLegs"] = "Too many legs for the set combination {0}:{1}. Maximum: {2}",
                ["LegsExceedSetRequirement"] = "{0} has more legs than required for the sets",
                ["InvalidLegCount"] = "Invalid leg count. Maximum: {0}, Total: {1}",
                ["SaveBlocked"] = "Save blocked",
                ["ValidationError"] = "Validation error",
                ["NoWinnerFound"] = "No winner found",
                ["GiveBye"] = "Give Bye",
                ["SelectByeWinner"] = "Select the player who should receive the bye:",

                // Input Dialog
                ["InputDialog"] = "Input",
                ["EnterName"] = "Enter name:",

                // Donation & Bug Report
                ["Donate"] = "💝 Donate",
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

                // Loading Spinner
                ["Loading"] = "Loading...",
                ["CheckingGroupStatus"] = "Checking group status...",
                ["ProcessingMatches"] = "Processing matches...",
                ["CheckingCompletion"] = "Checking completion...",

                // Tournament Tree Messages
                ["NoLoserBracketSingleElimination"] = "No Loser Bracket (Single Elimination)",
                ["NoLoserBracketGames"] = "No loser bracket matches available",
                ["NoWinnerBracketGames"] = "No winner bracket matches available",
                ["InteractiveTournamentTree"] = "Interactive tournament tree is created via TournamentClass",
                ["TBD"] = "TBD",
                ["Versus"] = "vs",
                ["AllGroupsCompleted"] = "🎉 All groups are completed!\n\nYou can now proceed to the next phase.",
                ["FinalsCompleted"] = "🏆 The finals are completed!\n\nAll matches have been finished. The tournament is complete!",
                
                // Loading Progress Messages
                ["CheckingFinalsStatus"] = "Checking finals status...",
                ["ProcessingFinalsMatches"] = "Processing finals matches...",
            },
        };
    }

    // Public method for external access (GetString)
    public string GetString(string key, params object[] args)
    {
        var translation = Translate(key, _currentLanguage);
        if (args.Length > 0)
        {
            return string.Format(translation, args);
        }
        return translation;
    }

    // Public property for CurrentLanguage
    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                OnPropertyChanged();
            }
        }
    }

    // Public property for CurrentTranslations
    public Dictionary<string, string> CurrentTranslations
    {
        get
        {
            if (_translations.TryGetValue(_currentLanguage, out var translations))
            {
                return translations;
            }
            return new Dictionary<string, string>();
        }
    }

    // Keep the original Translate method for internal use
    public string Translate(string key, string? language = null)
    {
        language ??= _currentLanguage;

        if (_translations.TryGetValue(language, out var translations) && translations.TryGetValue(key, out var translation))
        {
            return translation;
        }

        return key; // Fallback to key if no translation is found
    }

    public void SetLanguage(string language)
    {
        _currentLanguage = language;
        OnPropertyChanged(nameof(CurrentLanguage));
    }

    public string GetCurrentLanguage() => _currentLanguage;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}