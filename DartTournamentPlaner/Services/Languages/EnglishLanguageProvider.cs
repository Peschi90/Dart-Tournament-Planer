using System;
using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages;

/// <summary> 
/// English translations for the Dart Tournament Planner
/// </summary>
public class EnglishLanguageProvider : ILanguageProvider
{
    public string LanguageCode => "en";
    public string DisplayName => "English";

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
    /// Generates dynamic About text with current assembly version
    /// </summary>
    /// <returns>About text with current version number</returns>
    private string GetDynamicAboutText()
    {
        var currentVersion = GetCurrentAssemblyVersion();
        return $"Dart Tournament Planner v{currentVersion}\n\nA modern tournament management software.\n\n© 2025 by I3uLL3t";
    }

    public Dictionary<string, string> GetTranslations()
    {
        return new Dictionary<string, string>
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
            ["Print"] = "Print",
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
            ["TournamentOverview"] = "🏆 Tournament Overview",
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
            ["InvalidClassInterval"] = "Invalid class interval. Please enter a number = 1.",
            ["InvalidSubTabInterval"] = "Invalid sub-tab interval. Please enter a number = 1.",

            // Turnierübersicht Texte
            ["TournamentName"] = "🎯 Tournament:",
            ["CurrentPhase"] = "📋 Current Phase:",
            ["GroupsCount"] = "👥 Groups:",
            ["PlayersTotal"] = "👤 Total Players:",
            ["GameRulesColon"] = "📜 Game Rules:",
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
            ["AboutText"] = GetDynamicAboutText(),
            ["ErrorSavingData"] = "Error saving data:",

            // Nachrichten für Turnier-Tab
            ["MinimumTwoPlayers"] = "At least 2 players required.",
            ["ErrorGeneratingMatches"] = "Error creating matches:",
            ["MatchesGeneratedSuccess"] = "Matches have been successfully created!",
            ["MatchesResetSuccess"] = "Matches have been reset!",
            ["ResetTournamentConfirm"] = "Do you really want to reset the entire tournament?\n\n⚠️ All matches and phases will be deleted!\nOnly groups and players will be kept.",
            ["TournamentResetComplete"] = "The tournament has been successfully reset.",
            ["ResetKnockoutConfirm"] = "Do you really want to reset the knockout phase?\n\n⚠️ All KO matches and the tournament tree will be deleted!\nThe tournament will be reset to the group phase.",
            ["ResetKnockoutComplete"] = "The knockout phase has been successfully reset.",
            ["ResetFinalsConfirm"] = "Do you really want to reset the finals?\n\n⚠️ All final matches will be deleted!\nThe tournament will be reset to the group phase.",
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
            ["PrintError"] = "Print Error",
            ["ErrorCreatingDocument"] = "Error creating print document.",
            ["ErrorPrinting"] = "Error printing: {0}",
            ["TournamentStatistics"] = "Tournament Statistics - {0}",
            ["TournamentOverviewPrint"] = "Tournament Overview - {0}",
            ["CreatedWith"] = "Created with Dart Tournament Planner",
            ["GameRulesLabel"] = "Game Rules: {0}",
            ["CurrentPhaseLabel"] = "Current Phase: {0}",
            ["NotStarted"] = "Not Started",
            ["GroupsPlayersLabel"] = "Groups: {0}, Total Players: {1}",
            ["GroupsOverview"] = "Groups Overview",
            ["PlayersCount"] = "Players: {0}",
            ["MatchesStatus"] = "{0} of {1} matches completed",
            ["Table"] = "Standings",
            ["NoStandingsAvailable"] = "No standings available yet.",
            ["MatchResults"] = "Match Results",
            ["NoMatchesAvailable"] = "No matches available.",
            ["WinnerBracketHeader"] = "{0} - Winner Bracket",
            ["LoserBracketHeader"] = "{0} - Loser Bracket",
            ["WinnerBracketMatches"] = "Winner Bracket - Matches",
            ["LoserBracketMatches"] = "Loser Bracket - Matches",
            ["NoWinnerBracketGames"] = "No Winner Bracket matches available.",
            ["NoLoserBracketGames"] = "No Loser Bracket matches available.",
            ["FinalsRound"] = "Finals",

            // Tabellen-Header
            ["Position"] = "Pos",
            ["PlayerHeader"] = "Player",
            ["MatchesPlayedShort"] = "MP",
            ["WinsShort"] = "W",
            ["DrawsShort"] = "D",
            ["LossesShort"] = "L",
            ["PointsHeader"] = "Pts",
            ["SetsHeader"] = "Sets",
            ["MatchNumber"] = "No",
            ["MatchHeader"] = "Match",
            ["StatusHeader"] = "Status",
            ["ResultHeader"] = "Result",
            ["WinnerHeader"] = "Winner",
            ["RoundHeader"] = "Round",

            // Match Status Texte
            ["ByeStatus"] = "BYE",
            ["FinishedStatus"] = "FINISHED",
            ["InProgressStatus"] = "IN PROGRESS",
            ["PendingStatus"] = "PENDING",
            ["ByeGame"] = "{0} (Bye)",
            ["VersusGame"] = "{0} vs {1}",
            ["Draw"] = "Draw",

            // PRINT DIALOG ÜBERSETZUNGEN
            ["PrintTournamentStatistics"] = "Print Tournament Statistics",
            ["TournamentStatisticsIcon"] = "📊 Statistics",
            ["TournamentClass"] = "🏆 Tournament Class:",
            ["SelectTournamentClass"] = "Tournament Class: {0} ({1} Groups, {2} Players)",
            ["EmptyTournamentClass"] = "❌ {0} (empty)",
            ["ActiveTournamentClass"] = "✅ {0}",
            ["GeneralOptions"] = "⚙️ General Options",
            ["TournamentOverviewOption"] = "Tournament Overview",
            ["TitleOptional"] = "📝 Title (optional):",
            ["SubtitleOptional"] = "📄 Subtitle (optional):",
            ["GroupPhaseSection"] = "👥 Group Phase",
            ["IncludeGroupPhase"] = "Include Group Phase",
            ["SelectGroups"] = "Select Groups:",
            ["AllGroups"] = "All Groups",
            ["GroupWithPlayers"] = "{0} ({1} Players)",
            ["FinalsSection"] = "🏅 Finals",
            ["IncludeFinals"] = "Include Finals",
            ["KnockoutSection"] = "⚔️ KO Phase",
            ["IncludeKnockout"] = "Include KO Phase",
            ["WinnerBracket"] = "Winner Bracket",
            ["LoserBracket"] = "Loser Bracket",
            ["ParticipantsList"] = "Participants List",
            ["PreviewSection"] = "👁️ Preview",
            ["PreviewPlaceholder"] = "📋 Preview will be displayed here...",
            ["UpdatePreview"] = "🔄 Update Preview",
            ["PrintButton"] = "🖨️ Print",
            ["CancelButton"] = "❌ Cancel",
            ["PrintPreviewTitle"] = "Print Preview - {0}",
            ["NoContentSelected"] = "No content selected for display.",
            ["PreviewTitle"] = "Preview",
            ["PreviewError"] = "Error during preview: {0}",
            ["PrintPreparationError"] = "Error during print preparation: {0}",
            ["NoContentToPrint"] = "⚠️ No content selected for printing",
            ["PreviewError2"] = "❌ Error during preview: {0}",
            ["PreviewGenerationError"] = "⚠️ Error generating preview information: {0}",
            ["SelectAtLeastOne"] = "Please select at least one print option.",
            ["NoSelection"] = "No Selection",
            ["NoGroupsAvailable"] = "The selected tournament class contains no groups to print.",
            ["NoGroupsAvailableTitle"] = "No Groups Available",
            ["SelectAtLeastOneGroup"] = "Please select at least one group.",
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

            // Additional fields
            ["SubmitResult"] = "Submit Result",
            ["ResultSubmitted"] = "Result successfully submitted!",
            ["Player1"] = "Player 1",
            ["Player2"] = "Player 2",
            ["Loser"] = "Loser",
            ["MatchCancelled"] = "Match cancelled",
            ["CancelMatch"] = "Cancel Match",
            ["MatchCancelledConfirm"] = "Do you really want to cancel the match?",
            ["MatchCancelledTitle"] = "Cancel Match",
            ["NotImplemented"] = "Not implemented",
            ["FeatureComingSoon"] = "This feature will be available soon."
        };
    }
}