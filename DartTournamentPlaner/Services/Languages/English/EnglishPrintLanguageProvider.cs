using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for print service and printing functions
/// </summary>
public class EnglishPrintLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // PRINT SERVICE TRANSLATIONS
            ["PrintError"] = "Print Error",
            ["ErrorCreatingDocument"] = "Error creating print document.",
            ["ErrorPrinting"] = "Error printing: {0}",
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

            // Table Headers
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

            // Match Status Texts
            ["ByeStatus"] = "BYE",
            ["FinishedStatus"] = "FINISHED",
            ["InProgressStatus"] = "IN PROGRESS",
            ["PendingStatus"] = "PENDING",
            ["ByeGame"] = "{0} (Bye)",
            ["VersusGame"] = "{0} vs {1}",
            ["Draw"] = "Draw",

            // PRINT DIALOG TRANSLATIONS
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

            // Preview Contents
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
            ["KnockoutParticipantsContent"] = "   • {0} qualified players"
        };
    }
}