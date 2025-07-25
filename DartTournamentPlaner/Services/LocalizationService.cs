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
                ["Player1"] = "Spieler 1",
                ["Player2"] = "Spieler 2",
                ["Sets"] = "Sets",
                ["Legs"] = "Legs",
                ["Winner"] = "Sieger",
                ["Result"] = "Ergebnis",
                ["Score"] = "Punkte",
                ["Position"] = "Platz",
                ["MatchesPlayed"] = "Spiele",
                ["Wins"] = "Siege",
                ["Losses"] = "Niederlagen",
                ["Draws"] = "Unentschieden",
                ["SetsWon"] = "Sets gewonnen",
                ["SetsLost"] = "Sets verloren",
                ["LegsWon"] = "Legs gewonnen",
                ["LegsLost"] = "Legs verloren",
                ["SetDifference"] = "Set-Differenz",
                ["LegDifference"] = "Leg-Differenz",
                ["Match"] = "Spiel",
                ["Status"] = "Status",
                ["Player"] = "Spieler",
                ["Notes"] = "Notizen",
                
                // Match Entry Dialog
                ["EnterMatchResult"] = "Spielergebnis eingeben",
                ["Player1Sets"] = "Sets Spieler 1",
                ["Player2Sets"] = "Sets Spieler 2", 
                ["Player1Legs"] = "Legs Spieler 1",
                ["Player2Legs"] = "Legs Spieler 2",
                ["SaveResult"] = "Ergebnis speichern",
                ["MatchResultSaved"] = "Ergebnis wurde gespeichert",
                
                // Enhanced Validation Messages
                ["ValidationError"] = "Validierungsfehler",
                ["InvalidWinCondition"] = "Ungültige Siegbedingung: {0} benötigt {1} {2} zum Sieg, aber hat nur {3}.",
                ["NoWinnerFound"] = "Kein Sieger ermittelt. Mindestens ein Spieler muss die Siegbedingung erfüllen.",
                ["BothPlayersWon"] = "Beide Spieler erfüllen die Siegbedingung. Das ist nicht möglich.",
                ["IncompleteScore"] = "Unvollständige Punkte: Bitte geben Sie Sets und Legs für beide Spieler ein.",
                ["NegativeValues"] = "Negative Werte sind nicht erlaubt.",
                ["InvalidNumbers"] = "Bitte geben Sie gültige Zahlen ein.",
                ["MatchIncomplete"] = "Das Spiel ist noch nicht beendet. Mindestens ein Spieler muss gewinnen.",
                ["InvalidSetCount"] = "Ungültige Set-Anzahl: Maximal {0} Sets sind bei Best-of-{1} möglich.",
                ["InvalidLegCount"] = "Ungültige Leg-Anzahl: Maximal {0} Legs sind bei Best-of-{1} möglich.",
                ["InvalidSetRatio"] = "Ungültiges Set-Verhältnis: Bei {0}:{1} Sets sollten die Legs ungefähr {2}:{3} betragen.",
                ["InconsistentResult"] = "Inkonsistentes Ergebnis: Das Legs-Verhältnis passt nicht zum Set-Ergebnis.",
                ["ExcessiveLegs"] = "Zu viele Legs: Bei {0}:{1} Sets sind maximal {2} Legs pro Spieler möglich.",
                ["InsufficientLegsForSet"] = "Nicht genügend Legs für Set-Sieg: {0} benötigt {1} Legs um ein Set zu gewinnen.",
                ["TooManyLegsInDecidingSet"] = "Zu viele Legs im entscheidenden Set: Maximal {0} Legs sind möglich.",
                ["LegsExceedSetRequirement"] = "{0} hat genug Legs für ein Set gewonnen, aber die Set-Anzahl stimmt nicht überein.",
                ["SaveBlocked"] = "Speicherung verhindert: Das Ergebnis entspricht nicht den konfigurierten Spielregeln.",
                
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
                ["Save"] = "Speichern",
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
                
                // Messages
                ["SaveSuccess"] = "Daten erfolgreich gespeichert.",
                ["LoadSuccess"] = "Daten erfolgreich geladen.",
                ["SaveError"] = "Fehler beim Speichern der Daten.",
                ["LoadError"] = "Fehler beim Laden der Daten.",

                // Post-Group Phase
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
                ["AdvanceToNextPhase"] = "Nächste Phase starten",
                ["CannotAdvancePhase"] = "Alle Spiele der aktuellen Phase müssen beendet sein",
                ["PhaseCompleted"] = "Phase abgeschlossen",
                ["GroupPhase"] = "Gruppenphase",
                ["FinalsPhase"] = "Finalrunde", 
                ["KnockoutPhase"] = "KO-Phase",
                ["WinnerBracket"] = "Winner Bracket",
                ["LoserBracket"] = "Loser Bracket",
                ["Round"] = "Runde",
                ["Quarterfinal"] = "Viertelfinale",
                ["Semifinal"] = "Halbfinale",
                ["Final"] = "Finale",
                ["GrandFinal"] = "Grand Final",
                
                // German KO Round Names based on player count
                ["Best64"] = "Beste 64",
                ["Best32"] = "Beste 32", 
                ["Best8"] = "Achtelfinale",
                ["Best4"] = "Viertelfinale",
                ["Best2"] = "Halbfinale",
                ["LastOfRound"] = "Runde {0}",
                
                // Tabs
                ["SetupTab"] = "Turnier-Setup",
                ["GroupPhaseTab"] = "Gruppenphase", 
                ["FinalsTab"] = "Finalrunde",
                ["KnockoutTab"] = "KO-Runde",
                ["LoserBracketTab"] = "Loser Bracket",
                ["LoserBracketTreeTab"] = "Loser Bracket Baum",
                ["GroupSetup"] = "Gruppenerstellung",
                ["MatchArea"] = "Spielbereich",

                // Tournament Reset
                ["TournamentReset"] = "Turnier zurückgesetzt",
                ["TournamentResetWarning"] = "?? WARNUNG: Das Turnier wird auf die Gruppenphase zurückgesetzt!",
                ["ResetTournament"] = "Turnier zurücksetzen",
                ["ResetTournamentTitle"] = "Turnier komplett zurücksetzen", 
                ["ResetTournamentConfirm"] = "Möchten Sie das gesamte Turnier wirklich zurücksetzen?\n\n?? ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.",
                ["TournamentResetComplete"] = "Turnier wurde erfolgreich zurückgesetzt.",
                
                // KO Reset
                ["ResetKnockoutPhase"] = "KO-Phase zurücksetzen",
                ["ResetKnockoutTitle"] = "KO-Phase zurücksetzen",
                ["ResetKnockoutConfirm"] = "Möchten Sie die KO-Phase wirklich zurücksetzen?\n\n?? Alle KO-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird zur Gruppenphase zurückgesetzt.",
                ["ResetKnockoutComplete"] = "KO-Phase wurde erfolgreich zurückgesetzt.",
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
                ["NoGroupSelectedPlayers"] = "Players: (No group selected)",
                ["Group"] = "Group {0}",
                
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
                ["MatchesGenerated"] = "Matches generated successfully!",
                ["ResetMatches"] = "Reset Matches",
                ["ResetMatchesConfirm"] = "Do you really want to reset all matches for group '{0}'?\nAll results will be lost!",
                ["ResetMatchesTitle"] = "Reset Matches",
                ["MatchesReset"] = "Matches have been reset!",
                ["EnterResult"] = "Enter Result",
                ["MatchNotStarted"] = "Not Started",
                ["MatchInProgress"] = "In Progress",
                ["MatchFinished"] = "Finished",
                ["MatchBye"] = "Bye",
                ["Player1"] = "Player 1",
                ["Player2"] = "Player 2",
                ["Sets"] = "Sets",
                ["Legs"] = "Legs",
                ["Winner"] = "Winner",
                ["Result"] = "Result",
                ["Score"] = "Points",
                ["Position"] = "Position",
                ["MatchesPlayed"] = "Played",
                ["Wins"] = "Wins",
                ["Losses"] = "Losses",
                ["Draws"] = "Draws",
                ["SetsWon"] = "Sets Won",
                ["SetsLost"] = "Sets Lost",
                ["LegsWon"] = "Legs Won",
                ["LegsLost"] = "Legs Lost",
                ["SetDifference"] = "Set Difference",
                ["LegDifference"] = "Leg Difference",
                ["Match"] = "Match",
                ["Status"] = "Status",
                ["Player"] = "Player",
                ["Notes"] = "Notes",
                
                // Match Entry Dialog
                ["EnterMatchResult"] = "Enter Match Result",
                ["Player1Sets"] = "Player 1 Sets",
                ["Player2Sets"] = "Player 2 Sets",
                ["Player1Legs"] = "Player 1 Legs",
                ["Player2Legs"] = "Player 2 Legs",
                ["SaveResult"] = "Save Result",
                ["MatchResultSaved"] = "Match result has been saved",
                
                // Enhanced Validation Messages
                ["ValidationError"] = "Validation Error",
                ["InvalidWinCondition"] = "Invalid win condition: {0} needs {1} {2} to win, but only has {3}.",
                ["NoWinnerFound"] = "No winner determined. At least one player must meet the win condition.",
                ["BothPlayersWon"] = "Both players meet the win condition. This is not possible.",
                ["IncompleteScore"] = "Incomplete score: Please enter sets and legs for both players.",
                ["NegativeValues"] = "Negative values are not allowed.",
                ["InvalidNumbers"] = "Please enter valid numbers.",
                ["MatchIncomplete"] = "The match is not finished. At least one player must win.",
                ["InvalidSetCount"] = "Invalid set count: Maximum {0} sets possible in best-of-{1}.",
                ["InvalidLegCount"] = "Invalid leg count: Maximum {0} legs possible in best-of-{1}.",
                ["InvalidSetRatio"] = "Invalid set ratio: With {0}:{1} sets, legs should be approximately {2}:{3}.",
                ["InconsistentResult"] = "Inconsistent result: The leg ratio doesn't match the set result.",
                ["ExcessiveLegs"] = "Too many legs: With {0}:{1} sets, maximum {2} legs per player possible.",
                ["InsufficientLegsForSet"] = "Insufficient legs for set win: {0} needs {1} legs to win a set.",
                ["TooManyLegsInDecidingSet"] = "Too many legs in deciding set: Maximum {0} legs possible.",
                ["LegsExceedSetRequirement"] = "{0} has enough legs to win a set, but set count doesn't match.",
                ["SaveBlocked"] = "Save blocked: The result doesn't comply with the configured game rules.",
                
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
                ["Save"] = "Save",
                ["SaveAs"] = "Save As",
                ["Exit"] = "Exit",
                ["Edit"] = "Edit",
                ["View"] = "View",
                ["Help"] = "Help",
                ["About"] = "About",
                
                // Status
                ["HasUnsavedChanges"] = "Modified",
                ["NotSaved"] = "Not saved",
                ["Saved"] = "Saved",
                ["Ready"] = "Ready",
                
                // Messages
                ["SaveSuccess"] = "Data saved successfully.",
                ["LoadSuccess"] = "Data loaded successfully.",
                ["SaveError"] = "Error saving data.",
                ["LoadError"] = "Error loading data.",

                // Post-Group Phase
                ["PostGroupPhase"] = "Post-Group Phase",
                ["PostGroupPhaseMode"] = "Post-Group Phase Mode",
                ["PostGroupPhaseNone"] = "Group Phase Only",
                ["PostGroupPhaseRoundRobin"] = "Finals Round (Round Robin)",
                ["PostGroupPhaseKnockout"] = "Knockout System",
                ["QualifyingPlayersPerGroup"] = "Qualifying Players per Group",
                ["KnockoutMode"] = "Knockout Mode",
                ["SingleElimination"] = "Single Elimination",
                ["DoubleElimination"] = "Double Elimination (Winner + Loser Bracket)",
                ["IncludeGroupPhaseLosersBracket"] = "Include Group Phase Losers in Loser Bracket",
                ["AdvanceToNextPhase"] = "Advance to Next Phase",
                ["CannotAdvancePhase"] = "All matches in current phase must be completed",
                ["PhaseCompleted"] = "Phase completed",
                ["GroupPhase"] = "Group Phase",
                ["FinalsPhase"] = "Finals Phase",
                ["KnockoutPhase"] = "Knockout Phase",
                ["WinnerBracket"] = "Winner Bracket",
                ["LoserBracket"] = "Loser Bracket",
                ["Round"] = "Round",
                ["Quarterfinal"] = "Quarterfinal",
                ["Semifinal"] = "Semifinal",
                ["Final"] = "Final",
                ["GrandFinal"] = "Grand Final",

                // English KO Round Names based on player count  
                ["Best64"] = "Round of 64",
                ["Best32"] = "Round of 32",
                ["Best8"] = "Round of 8",
                ["Best4"] = "Quarterfinal",
                ["Best2"] = "Semifinal",
                ["LastOfRound"] = "Round {0}",
                
                // Tabs
                ["SetupTab"] = "Tournament Setup",
                ["GroupPhaseTab"] = "Group Phase",
                ["FinalsTab"] = "Finals Round", 
                ["KnockoutTab"] = "Knockout Round",
                ["LoserBracketTab"] = "Loser Bracket",
                ["LoserBracketTreeTab"] = "Loser Bracket Tree",
                ["GroupSetup"] = "Group Creation",
                ["MatchArea"] = "Match Area",

                // Tournament Reset
                ["TournamentReset"] = "Tournament Reset",
                ["TournamentResetWarning"] = "?? WARNING: Tournament will be reset to group phase!",
                ["ResetTournament"] = "Reset Tournament",
                ["ResetTournamentTitle"] = "Reset Tournament Completely",
                ["ResetTournamentConfirm"] = "Do you really want to reset the entire tournament?\n\n?? ALL matches and phases will be deleted!\nOnly groups and players will remain.",
                ["TournamentResetComplete"] = "Tournament has been successfully reset.",
                
                // KO Reset
                ["ResetKnockoutPhase"] = "Reset Knockout Phase",
                ["ResetKnockoutTitle"] = "Reset Knockout Phase",
                ["ResetKnockoutConfirm"] = "Do you really want to reset the knockout phase?\n\n?? All knockout matches and bracket tree will be deleted!\nTournament will be reset to group phase.",
                ["ResetKnockoutComplete"] = "Knockout phase has been successfully reset.",
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
                _currentLanguage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentTranslations));
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
}