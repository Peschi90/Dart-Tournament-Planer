using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for tournament management and games
/// </summary>
public class EnglishTournamentLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Tournament Tab Translations
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
            ["RemoveGroupConfirm"] = "Do you really want to remove group '{0}'?\nAll players in this group will also be removed.",
            ["RemoveGroupTitle"] = "Remove Group",
            ["RemovePlayerConfirm"] = "Do you really want to remove player '{0}'?",
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
            ["AdvanceToNextPhase"] = "Start Next Phase",
            ["ResetTournament"] = "Reset Tournament",
            ["ResetKnockoutPhase"] = "Reset KO Phase",
            ["ResetFinalsPhase"] = "Reset Finals Phase",
            ["RefreshUI"] = "Refresh UI",
            ["RefreshUITooltip"] = "Refreshes the user interface",
            
            // ✅ NEU: For Group.ToString() - pure plural forms without colon
            ["PlayersPlural"] = "Players",
            ["MatchesPlural"] = "Matches",

            // ✅ NEU: Group phase sub-tabs
            ["GamesTab"] = "🎯 Games",
            ["TableTab"] = "📊 Table",

            // Tournament Process Phases
            ["GroupPhase"] = "Group Phase",
            ["FinalsPhase"] = "Final Round",
            ["KnockoutPhase"] = "KO Phase",

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

            // Post Group Phase Settings
            ["PostGroupPhase"] = "After Group Phase",
            ["PostGroupPhaseMode"] = "Post Group Phase Mode",
            ["PostGroupPhaseNone"] = "Group Phase Only",
            ["PostGroupPhaseRoundRobin"] = "Final Round (Round Robin)",
            ["PostGroupPhaseKnockout"] = "KO System",
            ["QualifyingPlayersPerGroup"] = "Qualified per Group",
            ["KnockoutMode"] = "KO Mode",
            ["SingleElimination"] = "Single Elimination",
            ["DoubleElimination"] = "Double Elimination (Winner + Loser Bracket)",
            ["IncludeGroupPhaseLosersBracket"] = "Include Group Phase Losers in Loser Bracket",
            
            // ✅ NEU: Skip Group Phase Option
            ["SkipGroupPhase"] = "⚡ Skip Group Phase (direct KO Phase)",
            ["SkipGroupPhaseTooltip"] = "Starts the tournament directly with the KO phase without group phase",

            // Round Specific Rules
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

            // Games and Match Management
            ["Matches"] = "Matches:",
            ["Standings"] = "Standings:",
            ["Match"] = "Match", // ✅ NEU: For DataGrid headers
            ["Result"] = "Result", // ✅ NEU: For DataGrid headers
            ["Status"] = "Status", // ✅ NEU: For DataGrid headers
            ["Position"] = "Pos", // ✅ NEU: For DataGrid headers
            ["Player"] = "Player", // ✅ NEU: For DataGrid headers
            ["Score"] = "Pts", // ✅ NEU: For DataGrid headers
            ["Sets"] = "Sets", // ✅ NEU: For DataGrid headers
            ["Legs"] = "Legs", // ✅ NEU: For DataGrid headers
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
            ["SubmitResult"] = "Submit Result",
            ["ResultSubmitted"] = "Result successfully submitted!",
            ["Player1"] = "Player 1",
            ["Player2"] = "Player 2",
            ["Loser"] = "Loser",
            ["MatchCancelled"] = "Match was cancelled",
            ["CancelMatch"] = "Cancel Match",
            ["MatchCancelledConfirm"] = "Do you really want to cancel the match?",
            ["MatchCancelledTitle"] = "Cancel Match",
            ["NotImplemented"] = "Not Implemented",
            ["FeatureComingSoon"] = "This feature will be available soon.",

            // Tournament Tab Messages
            ["MinimumTwoPlayers"] = "Minimum 2 players required.",
            ["ErrorGeneratingMatches"] = "Error generating matches:",
            ["MatchesGeneratedSuccess"] = "Matches have been successfully created!",
            ["MatchesResetSuccess"] = "Matches have been reset!",
            ["ResetTournamentConfirm"] = "Do you really want to reset the entire tournament?\n\n⚠️ ALL matches and phases will be deleted!\nOnly groups and players will remain.",
            ["TournamentResetComplete"] = "Tournament has been successfully reset.",
            ["ResetKnockoutConfirm"] = "Do you really want to reset the KO phase?\n\n⚠️ All KO matches and the tournament tree will be deleted!\nThe tournament will be reset to the group phase.",
            ["ResetKnockoutComplete"] = "KO phase has been successfully reset.",
            ["ResetFinalsConfirm"] = "Do you really want to reset the final round?\n\n⚠️ All final matches will be deleted!\nThe tournament will be reset to the group phase.",
            ["ResetFinalsComplete"] = "Final round has been successfully reset.",
            ["ErrorResettingTournament"] = "Error resetting tournament:",
            ["CannotAdvancePhase"] = "All matches of the current phase must be completed",
            ["ErrorAdvancingPhase"] = "Error advancing to next phase:",
            ["UIRefreshed"] = "User interface has been refreshed",
            ["ErrorRefreshing"] = "Error refreshing:",
            ["KOPhaseActiveMSB"] = "KO phase is not active",
            ["KOPhaseNotEnoughUserMSB"] = "Not enough participants for KO phase (minimum 2 required)",

            // Message Titles
            ["KOPhaseUsrWarnTitel"] = "KO Phase Warning",

            // Tab Headers for Player View
            ["FinalistsCount"] = "Finalists ({0} players):",
            ["KnockoutParticipantsCount"] = "KO Participants ({0} players):",

            // Additional Phase Texts
            ["NextPhaseStart"] = "Start {0}",

            // Match Result Window
            ["EnterMatchResult"] = "Enter Match Result",
            ["SaveResult"] = "Save Result",
            ["Notes"] = "Notes",
            ["InvalidNumbers"] = "Invalid Numbers",
            ["NegativeValues"] = "Negative values are not allowed",
            ["InvalidSetCount"] = "Invalid set count. Maximum: {0}, Total: {1}",
            ["BothPlayersWon"] = "Both players cannot win simultaneously",
            ["MatchIncomplete"] = "Match is not yet complete",
            ["InsufficientLegsForSet"] = "{0} does not have enough legs for the won sets. Minimum: {1}",
            ["ExcessiveLegs"] = "Too many legs for set combination {0}:{1}. Maximum: {2}",
            ["LegsExceedSetRequirement"] = "{0} has more legs than required for the sets",
            ["InvalidLegCount"] = "Invalid leg count. Maximum: {0}, Total: {1}",
            ["SaveBlocked"] = "Save Blocked",
            ["GiveBye"] = "Give Bye",
            ["SelectByeWinner"] = "Select the player who should receive the bye:",
            ["NoWinnerFound"] = "No winner found",

            // Context Menu Specific Translations
            ["EditResult"] = "Edit Result",
            ["AutomaticBye"] = "Automatic Bye",
            ["UndoByeShort"] = "Undo Bye",
            ["NoActionsAvailable"] = "No actions available",
            ["ByeToPlayer"] = "Bye to {0}",

            // KO Tab Header and Navigation
            ["KOTab"] = "KO Round",
            ["StatisticsTab"] = "Statistics",
            ["KOPhaseTab"] = "KO Phase",
            ["WinnerBracketTab"] = "Winner Bracket",
            ["LoserBracketTab"] = "Loser Bracket",
            ["KOParticipantsTab"] = "KO Participants",
            ["BracketOverviewTab"] = "Bracket Overview",
            ["TreeViewTab"] = "Tournament Tree",

            // KO Phase Status and Messages
            ["KOPhaseNotActive"] = "KO phase is not active",
            ["KOPhaseWaitingForGroupCompletion"] = "Waiting for group phase completion",
            ["KOPhaseReady"] = "KO phase ready",
            ["KOPhaseInProgress"] = "KO phase in progress",
            ["KOPhaseComplete"] = "KO phase complete",
            ["GenerateKOBracket"] = "Generate KO Bracket",
            ["KOBracketGenerated"] = "KO bracket has been successfully generated!",
            ["ErrorGeneratingKOBracket"] = "Error generating KO bracket:",
            ["NoQualifiedPlayersForKO"] = "No qualified players found for KO phase.",
            ["InsufficientPlayersForKO"] = "Not enough players for KO phase (minimum 2 required).",

            // KO Match Status and Actions
            ["KOMatchPending"] = "Pending",
            ["KOMatchInProgress"] = "In Progress",
            ["KOMatchFinished"] = "Finished",
            ["KOMatchBye"] = "Bye",
            ["NextRound"] = "Next Round",
            ["AdvanceWinner"] = "Advance Winner",
            ["EliminateLoser"] = "Eliminate Loser",
            ["WinnerAdvancesTo"] = "Winner advances to: {0}",
            ["LoserMovesToLB"] = "Loser moves to Loser Bracket: {0}",
            ["WaitingForPreviousMatch"] = "Waiting for previous match",

            // KO Bracket Structure
            ["Round1"] = "Round 1",
            ["Round2"] = "Round 2",
            ["Round3"] = "Round 3",
            ["Quarterfinals"] = "Quarterfinals",
            ["Semifinals"] = "Semifinals",
            ["Finals"] = "Finals",
            ["GrandFinals"] = "Grand Final",
            ["ThirdPlacePlayoff"] = "Third Place Playoff",
            ["WinnerBracketFinal"] = "Winner Bracket Final",
            ["LoserBracketFinal"] = "Loser Bracket Final",

            // KO Participants and Seeding
            ["QualifiedFromGroup"] = "Qualified from Group {0}",
            ["SeedPosition"] = "Seed Position {0}",
            ["HighestSeed"] = "Highest Seed",
            ["LowestSeed"] = "Lowest Seed",
            ["RandomSeeding"] = "Random Seeding",
            ["GroupWinners"] = "Group Winners",
            ["GroupRunners"] = "Group Runners-up",
            ["BestThirds"] = "Best Third Places",

            // KO Bracket Actions and Buttons
            ["ResetKOBracket"] = "Reset KO Bracket",
            ["ResetKOBracketConfirm"] = "Do you really want to reset the KO bracket?\n\n⚠️ All KO matches will be deleted!\nQualified players will remain.",
            ["KOBracketReset"] = "KO bracket has been reset!",
            ["ExpandAllMatches"] = "Expand All Matches",
            ["CollapseAllMatches"] = "Collapse All Matches",
            ["ShowBracketTree"] = "Show Bracket Tree",
            ["ShowMatchList"] = "Show Match List",
            ["ExportBracket"] = "Export Bracket",
            ["PrintBracket"] = "Print Bracket",

            // Double Elimination Specific Terms
            ["WinnerBracket"] = "Winner Bracket",
            ["LoserBracket"] = "Loser Bracket",
            ["WinnerBracketMatches"] = "Winner Bracket Matches",
            ["LoserBracketMatches"] = "Loser Bracket Matches",
            ["UpperBracket"] = "Upper Bracket",
            ["LowerBracket"] = "Lower Bracket",
            ["ConsolationBracket"] = "Consolation Bracket",
            ["EliminationMatch"] = "Elimination Match",
            ["ConsolationMatch"] = "Consolation Match",

            // KO Match Information
            ["MatchDuration"] = "Match Duration: {0}",
            ["MatchStarted"] = "Match Started: {0}",
            ["MatchFinishedAt"] = "Match Finished: {0}",  // ✅ RENAMED from MatchFinished to MatchFinishedAt
            ["ElapsedTime"] = "Elapsed Time: {0}",
            ["EstimatedDuration"] = "Estimated Duration: {0}",
            ["QualificationPath"] = "Qualification Path: {0}"
        };
    }
}