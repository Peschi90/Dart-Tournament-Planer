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
                
                // Tournament Overview
                ["TournamentOverview"] = "📺 Turnier-Übersicht",
                ["OverviewMode"] = "Übersichtsmodus",
                ["Configure"] = "⚙ Konfigurieren",
                ["ManualMode"] = "Manueller Modus",
                ["AutoCyclingActive"] = "Auto-Cycling aktiv",
                ["CyclingStopped"] = "Cycling gestoppt",
                ["ManualControl"] = "Manuelle Kontrolle",
                ["Showing"] = "Zeigt",
                ["Close"] = "Schließen",
                
                // Additional common terms
                ["Start"] = "Start",
                ["Stop"] = "Stop",
                ["Player"] = "Spieler",
                ["Match"] = "Spiel",
                ["Result"] = "Ergebnis",
                ["Status"] = "Status",
                ["Position"] = "Platz",
                ["Winner"] = "Sieger",
                
                // Match Status Translations
                ["Unknown"] = "Unbekannt",
                
                // Tournament Overview specific
                ["StartCycling"] = "Starten",
                ["StopCycling"] = "Stoppen", 
                ["WinnerBracketMatches"] = "Winner Bracket Spiele",
                ["WinnerBracketTree"] = "Winner Bracket Baum",
                ["LoserBracketMatches"] = "Loser Bracket Spiele", 
                ["LoserBracketTree"] = "Loser Bracket Baum",
                ["RoundColumn"] = "Runde",
                ["PositionShort"] = "Platz",
                ["PointsShort"] = "Pkt",
                ["WinDrawLoss"] = "S-U-N",
                ["NoLoserBracketMatches"] = "Keine Loser Bracket Spiele vorhanden",
                ["NoWinnerBracketMatches"] = "Keine Winner Bracket Spiele vorhanden", 
                ["TournamentTreeWillShow"] = "Der Turnierbaum wird angezeigt sobald die KO-Phase beginnt",
                
                // Additional Group Phase terms
                ["SelectGroup"] = "Gruppe auswählen",
                ["NoGroupSelected"] = "Keine Gruppe ausgewählt",
                
                // OverviewConfigDialog translations
                ["OverviewConfiguration"] = "Übersichts-Konfiguration", 
                ["TournamentOverviewConfiguration"] = "Turnier Übersicht Konfiguration",
                ["TimeBetweenClasses"] = "Zeit zwischen Turnierklassen:",
                ["TimeBetweenSubTabs"] = "Zeit zwischen Unter-Tabs:",
                ["Seconds"] = "Sekunden",
                ["ShowOnlyActiveClassesText"] = "Nur Klassen mit aktiven Gruppen anzeigen",
                ["OverviewInfoText"] = "Die Übersicht durchläuft automatisch die Turnierklassen und ihre Gruppen/Brackets endlos. Sie können dieses Fenster auf einen zweiten Monitor verschieben.",
                ["OK"] = "OK",
                ["InvalidClassInterval"] = "Ungültiges Klassen-Intervall. Bitte geben Sie eine Zahl >= 1 ein.",
                ["InvalidSubTabInterval"] = "Ungültiges Unter-Tab-Intervall. Bitte geben Sie eine Zahl >= 1 ein.",
                ["Error"] = "Fehler",
                
                // Tournament Overview Texts
                ["TournamentName"] = "🏆 Turnier:",
                ["CurrentPhase"] = "🎯 Aktuelle Phase:",
                ["GroupsCount"] = "👥 Gruppen:",
                ["PlayersTotal"] = "🎮 Spieler gesamt:",
                ["GameRulesColon"] = "📋 Spielregeln:",
                ["CompletedGroups"] = "✅ Abgeschlossene Gruppen:",
                ["QualifiedPlayers"] = "🏅 Qualifizierte Spieler:",
                ["KnockoutMatches"] = "⚔️ KO-Spiele:",
                ["Completed"] = "beendet",
                
                // Weitere hardcodierte Texte
                ["Finalists"] = "Finalisten",
                ["KnockoutParticipants"] = "KO-Teilnehmer",
                ["PlayersText"] = "Spieler",
                ["OverviewModeTitle"] = "Tournament Overview Mode",
                ["Information"] = "Information",
                ["Warning"] = "Warnung",
                ["NewTournament"] = "Neues Turnier",
                ["CreateNewTournament"] = "Neues Turnier erstellen? Ungespeicherte Änderungen gehen verloren.",
                ["UnsavedChanges"] = "Ungespeicherte Änderungen",
                ["SaveBeforeExit"] = "Sie haben ungespeicherte Änderungen. Möchten Sie vor dem Beenden speichern?",
                ["CustomFileNotImplemented"] = "Benutzerdefiniertes Laden von Dateien ist noch nicht implementiert.",
                ["CustomFileSaveNotImplemented"] = "Benutzerdefiniertes Speichern von Dateien ist noch nicht implementiert.",
                ["ErrorOpeningHelp"] = "Fehler beim Öffnen der Hilfe:",
                ["ErrorOpeningOverview"] = "Fehler beim Öffnen der Turnier-Übersicht:",
                ["AboutText"] = "Dart Tournament Planner v1.0\n\nEine moderne Turnierverwaltungsanwendung.",
                ["ErrorSavingData"] = "Fehler beim Speichern der Daten:",
                
                // MessageBox Texte für TournamentTab
                ["MinimumTwoPlayers"] = "Mindestens 2 Spieler sind erforderlich.",
                ["ErrorGeneratingMatches"] = "Fehler beim Generieren der Spiele:",
                ["MatchesGeneratedSuccess"] = "Spiele wurden erfolgreich generiert!",
                ["MatchesResetSuccess"] = "Spiele wurden zurückgesetzt!",
                ["ResetTournamentConfirm"] = "Möchten Sie das gesamte Turnier wirklich zurücksetzen?\n\n⚠ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.",
                ["TournamentResetComplete"] = "Turnier wurde erfolgreich zurückgesetzt.",
                ["ResetKnockoutConfirm"] = "Möchten Sie die KO-Phase wirklich zurücksetzen?\n\n⚠ Alle KO-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird zur Gruppenphase zurückgesetzt.",
                ["ResetKnockoutComplete"] = "KO-Phase wurde erfolgreich zurückgesetzt.",
                ["ErrorResettingTournament"] = "Fehler beim Zurücksetzen des Turniers:",
                ["CannotAdvancePhase"] = "Alle Spiele der aktuellen Phase müssen beendet sein",
                ["ErrorAdvancingPhase"] = "Fehler beim Wechsel zur nächsten Phase:",
                ["UIRefreshed"] = "Benutzeroberfläche wurde aktualisiert",
                ["ErrorRefreshing"] = "Fehler beim Aktualisieren:",
                ["KOPhaseActiveMSB"] = "K.O.-Phase ist noch nicht aktiv.",
                ["KOPhaseNotEnoughUserMSB"] = "Nicht genügend Teilnehmer für K.O.-Phase (mindestens 2 erforderlich)",

                // MessageBox Title
                ["KOPhaseUsrWarnTitel"] = "K.O.-Phase Warnung",

                // Tab-Überschriften für UpdatePlayersView
                ["FinalistsCount"] = "Finalisten ({0} Spieler):",
                ["KnockoutParticipantsCount"] = "KO-Teilnehmer ({0} Spieler):",
                
                // Weitere Phase-Texte
                ["NextPhaseStart"] = "{0} starten",
                
                // MatchResultWindow Übersetzungen
                ["EnterMatchResult"] = "Ergebnis eingeben",
                ["SaveResult"] = "Ergebnis speichern",
                ["Notes"] = "Notizen",
                ["InvalidNumbers"] = "Ungültige Zahlen",
                ["NegativeValues"] = "Negative Werte sind nicht erlaubt",
                ["InvalidSetCount"] = "Ungültige Set-Anzahl. Maximum: {0}, Gesamt: {1}",
                ["BothPlayersWon"] = "Beide Spieler können nicht gleichzeitig gewinnen",
                ["MatchIncomplete"] = "Das Spiel ist noch nicht beendet",
                ["InsufficientLegsForSet"] = "{0} hat nicht genügend Legs für die gewonnenen Sets. Minimum: {1}",
                ["ExcessiveLegs"] = "Zu viele Legs für die Set-Kombination {0}:{1}. Maximum: {2}",
                ["LegsExceedSetRequirement"] = "{0} hat mehr Legs als für Sets erforderlich",
                ["InvalidLegCount"] = "Ungültige Leg-Anzahl. Maximum: {0}, Gesamt: {1}",
                ["SaveBlocked"] = "Speichern blockiert",
                ["ValidationError"] = "Validierungsfehler",
                ["NoWinnerFound"] = "Kein Gewinner gefunden",
                ["GiveBye"] = "Freilos vergeben",
                ["SelectByeWinner"] = "Wählen Sie den Spieler aus, der das Freilos erhalten soll:",
                
                // ShowInputDialog Übersetzungen
                ["InputDialog"] = "Eingabe",
                ["EnterName"] = "Name eingeben:",
            },
            ["en"] = new Dictionary<string, string>
            {
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
                ["KnockoutTab"] = "Knockout",
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
                ["NoNameEntered"] = "No Name Entered",
                ["PlayersInGroup"] = "Players in {0}:",
                ["NoGroupSelectedPlayers"] = "Players: (No Group Selected)",
                ["Group"] = "Group {0}",
                ["AdvanceToNextPhase"] = "Start Next Phase",
                ["ResetTournament"] = "Reset Tournament",
                ["ResetKnockoutPhase"] = "Reset Knockout Phase",
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
                
                // Matches
                ["Matches"] = "Matches:",
                ["Standings"] = "Standings:",
                ["GenerateMatches"] = "Generate Matches",
                ["MatchesGenerated"] = "Matches have been generated successfully!",
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
                ["AutoSaveInterval"] = "Save Interval (minutes)",
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
                ["HasUnsavedChanges"] = "Changed",
                ["NotSaved"] = "Not saved",
                ["Saved"] = "Saved",
                ["Ready"] = "Ready",
                
                // Tournament Overview
                ["TournamentOverview"] = "📺 Tournament Overview",
                ["OverviewMode"] = "Overview Mode",
                ["Configure"] = "Configure",
                ["ManualMode"] = "Manual Mode",
                ["AutoCyclingActive"] = "Auto-cycling active",
                ["CyclingStopped"] = "Cycling stopped",
                ["ManualControl"] = "Manual control",
                ["Showing"] = "Showing",
                ["Close"] = "Close",
                
                // Additional common terms
                ["Start"] = "Start",
                ["Stop"] = "Stop",
                ["Player"] = "Player",
                ["Match"] = "Match",
                ["Result"] = "Result",
                ["Status"] = "Status",
                ["Position"] = "Position",
                ["Winner"] = "Winner",
                
                // Match Status Translations
                ["Unknown"] = "Unknown",
                
                // Tournament Overview specific
                ["StartCycling"] = "Start",
                ["StopCycling"] = "Stop",
                ["WinnerBracketMatches"] = "Winner Bracket Matches",
                ["WinnerBracketTree"] = "Winner Bracket Tree",
                ["LoserBracketMatches"] = "Loser Bracket Matches",
                ["LoserBracketTree"] = "Loser Bracket Tree", 
                ["RoundColumn"] = "Round",
                ["PositionShort"] = "Pos",
                ["PointsShort"] = "Pts",
                ["WinDrawLoss"] = "W-D-L",
                ["NoLoserBracketMatches"] = "No Loser Bracket matches available",
                ["NoWinnerBracketMatches"] = "No Winner Bracket matches available",
                ["TournamentTreeWillShow"] = "The tournament tree will be displayed once the knockout phase begins",
                
                // Additional Group Phase terms
                ["SelectGroup"] = "Select Group", 
                ["NoGroupSelected"] = "No Group Selected",
                
                // OverviewConfigDialog translations
                ["OverviewConfiguration"] = "Overview Configuration",
                ["TournamentOverviewConfiguration"] = "Tournament Overview Configuration", 
                ["TimeBetweenClasses"] = "Time between tournament classes:",
                ["TimeBetweenSubTabs"] = "Time between sub-tabs:",
                ["Seconds"] = "seconds",
                ["ShowOnlyActiveClassesText"] = "Show only classes with active groups",
                ["OverviewInfoText"] = "The overview automatically cycles through the tournament classes and their groups/brackets endlessly. You can move this window to a second monitor.",
                ["OK"] = "OK",
                ["InvalidClassInterval"] = "Invalid class interval. Please enter a number >= 1.",
                ["InvalidSubTabInterval"] = "Invalid sub-tab interval. Please enter a number >= 1.",
                ["Error"] = "Error",
                
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
                
                // More hardcoded texts
                ["Finalists"] = "Finalists",
                ["KnockoutParticipants"] = "Knockout Participants",
                ["PlayersText"] = "Players",
                ["OverviewModeTitle"] = "Tournament Overview Mode",
                ["Information"] = "Information",
                ["Warning"] = "Warning",
                ["NewTournament"] = "New Tournament",
                ["CreateNewTournament"] = "Create new tournament? Unsaved changes will be lost.",
                ["UnsavedChanges"] = "Unsaved Changes",
                ["SaveBeforeExit"] = "You have unsaved changes. Do you want to save before exiting?",
                ["CustomFileNotImplemented"] = "Custom file loading not implemented yet.",
                ["CustomFileSaveNotImplemented"] = "Custom file saving not implemented yet.",
                ["ErrorOpeningHelp"] = "Error opening help:",
                ["ErrorOpeningOverview"] = "Error opening tournament overview:",
                ["AboutText"] = "Dart Tournament Planner v1.0\n\nA modern tournament management application.",
                ["ErrorSavingData"] = "Error saving data:",
                
                // MessageBox texts for TournamentTab
                ["MinimumTwoPlayers"] = "At least 2 players are required.",
                ["ErrorGeneratingMatches"] = "Error generating matches:",
                ["MatchesGeneratedSuccess"] = "Matches have been generated successfully!",
                ["MatchesResetSuccess"] = "Matches have been reset!",
                ["ResetTournamentConfirm"] = "Do you really want to reset the entire tournament?\n\n⚠ ALL matches and phases will be deleted!\nOnly groups and players will remain.",
                ["TournamentResetComplete"] = "Tournament has been successfully reset.",
                ["ResetKnockoutConfirm"] = "Do you really want to reset the knockout phase?\n\n⚠ All knockout matches and the tournament tree will be deleted!\nThe tournament will be reset to group phase.",
                ["ResetKnockoutComplete"] = "Knockout phase has been successfully reset.",
                ["ErrorResettingTournament"] = "Error resetting tournament:",
                ["CannotAdvancePhase"] = "All matches in the current phase must be completed",
                ["ErrorAdvancingPhase"] = "Error advancing to next phase:",
                ["UIRefreshed"] = "User interface has been refreshed",
                ["ErrorRefreshing"] = "Error refreshing:",
                ["KOPhaseActiveMSB"] = "K.O.-Phase is not active",
                ["KOPhaseNotEnoughUserMSB"] = "Not enough participants for knockout phase (at least 2 required)",

                // MessageBox Titles
                ["KOPhaseUsrWarnTitel"] = "K.O.-Phase Warning",

                // Tab headers for UpdatePlayersView
                ["FinalistsCount"] = "Finalists ({0} players):",
                ["KnockoutParticipantsCount"] = "Knockout Participants ({0} players):",
                
                // More phase texts
                ["NextPhaseStart"] = "Start {0}",
                
                // MatchResultWindow Translations
                ["EnterMatchResult"] = "Enter Match Result",
                ["SaveResult"] = "Save Result",
                ["Notes"] = "Notes",
                ["InvalidNumbers"] = "Invalid numbers",
                ["NegativeValues"] = "Negative values are not allowed",
                ["InvalidSetCount"] = "Invalid set count. Maximum: {0}, Total: {1}",
                ["BothPlayersWon"] = "Both players cannot win simultaneously",
                ["MatchIncomplete"] = "The match is not yet complete",
                ["InsufficientLegsForSet"] = "{0} does not have enough legs for the won sets. Minimum: {1}",
                ["ExcessiveLegs"] = "Too many legs for the set combination {0}:{1}. Maximum: {2}",
                ["LegsExceedSetRequirement"] = "{0} has more legs than required for sets",
                ["InvalidLegCount"] = "Invalid leg count. Maximum: {0}, Total: {1}",
                ["SaveBlocked"] = "Save blocked",
                ["ValidationError"] = "Validation error",
                ["NoWinnerFound"] = "No winner found",
                ["GiveBye"] = "Give Bye",
                ["SelectByeWinner"] = "Select the player who should receive the bye:",
                
                // ShowInputDialog Translations
                ["InputDialog"] = "Input",
                ["EnterName"] = "Enter name:",
            }
        };
    }

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value && _translations.ContainsKey(value))
            {
                var oldLanguage = _currentLanguage;
                _currentLanguage = value;
                
                System.Diagnostics.Debug.WriteLine($"LocalizationService.CurrentLanguage: Changed from '{oldLanguage}' to '{_currentLanguage}'");
                
                // Fire PropertyChanged events immediately
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentTranslations));
                
                System.Diagnostics.Debug.WriteLine($"LocalizationService.CurrentLanguage: PropertyChanged events fired");
            }
        }
    }

    public Dictionary<string, string> CurrentTranslations => _translations[_currentLanguage];

    public string GetString(string key)
    {
        if (_translations.ContainsKey(_currentLanguage) && _translations[_currentLanguage].ContainsKey(key))
        {
            return _translations[_currentLanguage][key];
        }
        return key; // Return key if translation not found
    }

    public string GetString(string key, params object[] args)
    {
        var format = GetString(key);
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SetLanguage(string language)
    {
        System.Diagnostics.Debug.WriteLine($"LocalizationService.SetLanguage: Changing from '{_currentLanguage}' to '{language}'");
        
        if (_translations.ContainsKey(language))
        {
            CurrentLanguage = language;
            System.Diagnostics.Debug.WriteLine($"LocalizationService.SetLanguage: Successfully changed to '{language}'");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"LocalizationService.SetLanguage: Language '{language}' not found in translations");
        }
    }
}