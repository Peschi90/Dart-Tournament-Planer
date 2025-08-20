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
                ["Score"] = "Punktestand",
                ["SubmitResult"] = "Ergebnis bestätigen",
                ["ResultSubmitted"] = "Ergebnis erfolgreich übermittelt!",
                ["Player1"] = "Spieler 1",
                ["Player2"] = "Spieler 2",
                ["Winner"] = "Sieger",
                ["Loser"] = "Verlierer",
                ["MatchCancelled"] = "Spiel wurde abgebrochen",
                ["CancelMatch"] = "Spiel abbrechen",
                ["MatchCancelledConfirm"] = "Möchten Sie das Spiel wirklich abbrechen?",
                ["MatchCancelledTitle"] = "Spiel abbrechen",
                ["NotImplemented"] = "Nicht implementiert",
                ["FeatureComingSoon"] = "Diese Funktion wird bald verfügbar sein.",
                
                // Weitere Übersetzungen... (gekürzt für Lesbarkeit)
                ["VersusGame"] = "{0} vs {1}",
                ["Draw"] = "Unentschieden",
                ["Unknown"] = "-"
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
                
                // PRINT DIALOG ÜBERSETZUNGEN
                ["PrintTournamentStatistics"] = "Print Tournament Statistics",
                ["TournamentStatisticsIcon"] = "📄",
                ["TournamentClass"] = "🏆 Tournament Class:",
                ["SelectTournamentClass"] = "Tournament Class: {0} ({1} Groups, {2} Players)",
                ["EmptyTournamentClass"] = "⚪ {0} (empty)",
                ["ActiveTournamentClass"] = "🏆 {0}",
                ["GeneralOptions"] = "🔧 General Options",
                ["TournamentOverviewOption"] = "Tournament Overview",
                ["TitleOptional"] = "📋 Title (optional):",
                ["SubtitleOptional"] = "📝 Subtitle (optional):",
                ["GroupPhaseSection"] = "👥 Group Phase",
                ["IncludeGroupPhase"] = "Include Group Phase",
                ["SelectGroups"] = "Select Groups:",
                ["AllGroups"] = "All Groups",
                ["GroupWithPlayers"] = "{0} ({1} Players)",
                ["FinalsSection"] = "🏆 Finals",
                ["IncludeFinals"] = "Include Finals",
                ["KnockoutSection"] = "⚔️ KO Phase",
                ["IncludeKnockout"] = "Include KO Phase",
                ["WinnerBracket"] = "Winner Bracket",
                ["LoserBracket"] = "Loser Bracket",
                ["ParticipantsList"] = "Participants List",
                ["PreviewSection"] = "👁️ Preview",
                ["PreviewPlaceholder"] = "📄 Preview will be displayed here...",
                ["UpdatePreview"] = "👁️ Update Preview",
                ["PrintButton"] = "🖨️ Print",
                ["CancelButton"] = "❌ Cancel",
                ["PrintPreviewTitle"] = "Print Preview - {0}",
                ["NoContentSelected"] = "No content selected for display.",
                ["PreviewTitle"] = "Preview",
                ["PreviewError"] = "Error during preview: {0}",
                ["PrintPreparationError"] = "Error during print preparation: {0}",
                ["NoContentToPrint"] = "📋 No content selected for printing",
                ["PreviewError2"] = "❌ Error during preview: {0}",
                ["PreviewGenerationError"] = "❌ Error generating preview information: {0}",
                ["SelectAtLeastOne"] = "Please select at least one print option.",
                ["NoSelection"] = "No Selection",
                ["NoGroupsAvailable"] = "The selected tournament class contains no groups to print.",
                ["NoGroupsAvailableTitle"] = "No Groups Available",
                ["SelectAtLeastOneGroup"] = "Please select at least one group.",
                ["NoGroupSelected"] = "No Group Selected",
                ["InvalidGroupSelection"] = "The selected groups are no longer available.",
                ["InvalidGroupSelectionTitle"] = "Invalid Group Selection",
                ["NoFinalsAvailable"] = "The selected tournament class has no finals to print.",
                ["NoFinalsAvailableTitle"] = "No Finals Available",
                ["SelectAtLeastOneKO"] = "Please select at least one KO option.",
                ["NoKOOptionSelected"] = "No KO Option Selected",
                ["NoKnockoutAvailable"] = "The selected tournament class has no knockout phase to print.",
                ["NoKnockoutAvailableTitle"] = "No Knockout Phase Available",
                ["ValidationError"] = "Validation error: {0}",
                ["ValidationErrorTitle"] = "Validation Error",
                
                // Preview-Inhalte
                ["PageOverview"] = "📄 Page {0}: Tournament Overview",
                ["OverviewContent1"] = "   • General tournament information",
                ["OverviewContent2"] = "   • Game rules and phase status",
                ["OverviewContent3"] = "   • Groups overview",
                ["PageGroupPhase"] = "📄 Page {0}: Group Phase - {1}",
                ["GroupPlayers"] = "   • {0} Players",
                ["GroupMatches"] = "   • {0} Matches",
                ["GroupContent"] = "   • Standings and results",
                ["PageFinals"] = "📄 Page {0}: Finals",
                ["FinalsContent1"] = "   • Qualified finalists",
                ["FinalsContent2"] = "   • Finals standings",
                ["FinalsContent3"] = "   • Finals matches",
                ["PageWinnerBracket"] = "📄 Page {0}: Winner Bracket",
                ["WinnerBracketMatches"] = "   • {0} KO matches",
                ["PageLoserBracket"] = "📄 Page {0}: Loser Bracket",
                ["LoserBracketMatches"] = "   • {0} LB matches",
                ["PageKnockoutParticipants"] = "📄 Page {0}: KO Participants",
                ["KnockoutParticipantsContent"] = "   • {0} qualified players",
                
                // Weitere Übersetzungen... (gekürzt für Lesbarkeit)
                ["VersusGame"] = "{0} vs {1}",
                ["Draw"] = "Draw",
                ["Unknown"] = "-"
            }
        };
    }

    // Ereignis für Property-Änderungen (INotifyPropertyChanged-Implementierung)
    public event PropertyChangedEventHandler PropertyChanged;

    // Methode zum Auslösen des PropertyChanged-Ereignisses
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    /// <summary>
    /// Aktuelle Sprache
    /// </summary>
    public string CurrentLanguage => _currentLanguage;

    /// <summary>
    /// Gibt die aktuellen Übersetzungen zurück - für Kompatibilität mit bestehendem Code
    /// </summary>
    public Dictionary<string, string> CurrentTranslations => _translations[_currentLanguage];

    /// <summary>
    /// Generiert dynamischen About-Text mit aktueller Assembly-Version
    /// </summary>
    /// <param name="language">Sprache für die Übersetzung (optional, verwendet aktuelle Sprache wenn nicht angegeben)</param>
    /// <returns>About-Text mit aktueller Versionsnummer</returns>
    private string GetDynamicAboutText(string? language = null)
    {
        var currentVersion = GetCurrentAssemblyVersion();
        var lang = language ?? _currentLanguage;

        if (lang == "en")
        {
            return $"Dart Tournament Planner v{currentVersion}\n\nA modern tournament management software.\n\n© 2025 by I3uLL3t";
        }
        else // Deutsch als Standard
        {
            return $"Dart Turnier Planer v{currentVersion}\n\nEine moderne Turnierverwaltungssoftware.\n\n© 2025 by I3uLL3t";
        }
    }

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
    /// Öffentliche Methode um die aktuelle Assembly-Version abzurufen
    /// </summary>
    /// <returns>Aktuelle Versionsnummer als String</returns>
    public string GetApplicationVersion()
    {
        return GetCurrentAssemblyVersion();
    }

    /// <summary>
    /// Gibt den übersetzten Text für den angegebenen Schlüssel und die aktuelle Sprache zurück
    /// Fallback auf den Schlüssel selbst, wenn keine Übersetzung gefunden wird
    /// </summary>
    /// <param name="key">Übersetzungsschlüssel</param>
    /// <returns>Übersetzter Text</returns>
    public string GetTranslation(string key)
    {
        // Spezialbehandlung für AboutText - generiere dynamisch mit aktueller Version
        if (key == "AboutText")
        {
            return GetDynamicAboutText();
        }

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
}