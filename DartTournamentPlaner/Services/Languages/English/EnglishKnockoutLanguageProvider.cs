using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations specifically for the KO tab and KO phase management
/// Extends existing tournament translations with specific KO functions
/// </summary>
public class EnglishKnockoutLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // =====================================
            // KO TAB MAIN AREA
            // =====================================
            
            // Tab headers and areas
            ["KOParticipantsHeader"] = "KO Participants:",
            ["KOParticipantsLabel"] = "KO Participants",
            ["BracketTreeHeader"] = "Tournament Tree",
            ["WinnerBracketHeader"] = "Winner Bracket:",
            ["LoserBracketHeader"] = "Loser Bracket:",
            ["BracketOverviewHeader"] = "Bracket Overview",
            
            // Sub-tab headers within KO tab
            ["TournamentTreeTab"] = "🌳 Tournament Tree",
            ["WinnerBracketDataTab"] = "🏆 Winner Bracket",
            ["LoserBracketDataTab"] = "🥈 Loser Bracket",
            ["LoserBracketTreeTab"] = "🌳 Loser Bracket Tree",
            
            // =====================================
            // BRACKET-SPECIFIC TERMS
            // =====================================
            
            // Bracket types
            ["MainBracket"] = "Main Bracket",
            ["ConsolationBracket"] = "Consolation Bracket",
            ["UpperBracket"] = "Upper Bracket",
            ["LowerBracket"] = "Lower Bracket",
            ["EliminationBracket"] = "Elimination Bracket",
            
            // Bracket navigation
            ["BracketView"] = "Bracket View",
            ["TreeView"] = "Tree View",
            ["DataView"] = "Data View",
            ["SwitchView"] = "Switch View",
            ["ExpandBracket"] = "Expand Bracket",
            ["CollapseBracket"] = "Collapse Bracket",
            
            // =====================================
            // BYE FUNCTIONS
            // =====================================
            
            // Bye buttons and actions
            ["ByeColumn"] = "Bye",
            ["GiveByeButton"] = "✓ Bye",
            ["UndoByeButton"] = "✗ Undo",
            ["GiveByeTooltip"] = "Give bye",
            ["UndoByeTooltip"] = "Undo bye",
            ["ByeManagement"] = "Bye Management",
            
            // Bye status
            ["ByeAwarded"] = "Bye awarded",
            ["ByeUndone"] = "Bye undone",
            ["AutomaticByeAwarded"] = "Automatic bye awarded",
            ["ManualByeAwarded"] = "Manual bye awarded",
            ["ByeNotPossible"] = "Bye not possible",
            ["ByeAlreadyAwarded"] = "Bye already awarded",
            
            // Bye confirmations
            ["ConfirmGiveBye"] = "Do you really want to give a bye?",
            ["ConfirmUndoBye"] = "Do you really want to undo the bye?",
            ["SelectByeWinner"] = "Select the player for the bye:",
            ["ByeGivenTo"] = "Bye given to: {0}",
            
            // =====================================
            // KO MATCH MANAGEMENT
            // =====================================
            
            // Match column headers
            ["RoundColumn"] = "Round",
            ["MatchColumn"] = "Match",
            ["ResultColumn"] = "Result",
            ["StatusColumn"] = "Status",
            ["ActionsColumn"] = "Actions",
            
            // Match status specific to KO
            ["KOPending"] = "Pending",
            ["KOInProgress"] = "In Progress",
            ["KOFinished"] = "Finished",
            ["KOBye"] = "Bye",
            ["KOWaitingForPlayers"] = "Waiting for Players",
            ["KOReadyToStart"] = "Ready to Start",
            
            // Match actions
            ["StartKOMatch"] = "Start KO Match",
            ["EditKOResult"] = "Edit KO Result",
            ["CancelKOMatch"] = "Cancel KO Match",
            ["ResetKOMatch"] = "Reset KO Match",
            
            // =====================================
            // ROUND DESIGNATIONS
            // =====================================
            
            // Extended round names
            ["FirstRound"] = "1st Round",
            ["SecondRound"] = "2nd Round",
            ["ThirdRound"] = "3rd Round",
            ["FourthRound"] = "4th Round",
            ["FifthRound"] = "5th Round",
            ["SixthRound"] = "6th Round",
            
            // Special rounds
            ["WildCardRound"] = "Wild Card Round",
            ["QualifyingRound"] = "Qualifying Round",
            ["PlayoffRound"] = "Playoff Round",
            ["ConsolationRound"] = "Consolation Round",
            
            // =====================================
            // KO PHASE MANAGEMENT
            // =====================================
            
            // Phase status
            ["KOPhaseNotCreated"] = "KO phase not created",
            ["KOPhaseCreated"] = "KO phase created",
            ["KOPhaseStarted"] = "KO phase started",
            ["KOPhaseFinished"] = "KO phase finished",
            
            // Phase actions
            ["CreateKOPhase"] = "Create KO Phase",
            ["StartKOPhase"] = "Start KO Phase",
            ["ResetKOPhase"] = "Reset KO Phase",
            ["FinishKOPhase"] = "Finish KO Phase",
            
            // Phase messages
            ["KOPhaseCreatedSuccess"] = "KO phase has been successfully created!",
            ["KOPhaseStartedSuccess"] = "KO phase has been successfully started!",
            ["KOPhaseResetSuccess"] = "KO phase has been successfully reset!",
            ["KOPhaseFinishedSuccess"] = "KO phase has been successfully finished!",
            
            // =====================================
            // PLAYER QUALIFICATION
            // =====================================
            
            // Qualification status
            ["QualifiedForKO"] = "Qualified for KO",
            ["NotQualifiedForKO"] = "Not qualified for KO",
            ["QualificationPending"] = "Qualification pending",
            
            // Qualification paths
            ["QualifiedAsGroupWinner"] = "Qualified as group winner",
            ["QualifiedAsRunnerUp"] = "Qualified as runner-up",
            ["QualifiedAsBestThird"] = "Qualified as best third place",
            ["QualifiedFromPreviousRound"] = "Qualified from previous round",
            
            // =====================================
            // BRACKET GENERATION
            // =====================================
            
            // Generation actions
            ["GenerateBracket"] = "Generate Bracket",
            ["RegenerateBracket"] = "Regenerate Bracket",
            ["ValidateBracket"] = "Validate Bracket",
            ["OptimizeBracket"] = "Optimize Bracket",
            
            // Generation status
            ["BracketGenerated"] = "Bracket has been generated",
            ["BracketValidated"] = "Bracket has been validated",
            ["BracketOptimized"] = "Bracket has been optimized",
            ["BracketGenerationFailed"] = "Bracket generation failed",
            
            // =====================================
            // SEEDING AND PAIRINGS
            // =====================================
            
            // Seeding methods
            ["SeedingMethod"] = "Seeding Method",
            ["RandomSeeding"] = "Random Seeding",
            ["RankedSeeding"] = "Ranked Seeding",
            ["ManualSeeding"] = "Manual Seeding",
            ["GroupPositionSeeding"] = "Group Position Seeding",
            
            // Seed positions
            ["Seed1"] = "Seed 1",
            ["Seed2"] = "Seed 2",
            ["Seed3"] = "Seed 3",
            ["Seed4"] = "Seed 4",
            ["TopSeed"] = "Top Seed",
            ["BottomSeed"] = "Bottom Seed",
            
            // =====================================
            // DOUBLE ELIMINATION SPECIFIC
            // =====================================
            
            // Double elimination terms
            ["DoubleEliminationMode"] = "Double Elimination",
            ["SingleEliminationMode"] = "Single Elimination",
            ["WinnerAdvances"] = "Winner advances",
            ["LoserEliminated"] = "Loser eliminated",
            ["LoserToLoserBracket"] = "Loser to loser bracket",
            
            // Grand final specific
            ["GrandFinalBracketReset"] = "Grand Final Bracket Reset",
            ["GrandFinalAdvantage"] = "Grand Final Advantage",
            ["WinnerBracketAdvantage"] = "Winner Bracket Advantage",
            ["MustWinTwice"] = "Must win twice",
            
            // =====================================
            // ERROR HANDLING AND VALIDATION
            // =====================================
            
            // Error messages
            ["ErrorInvalidBracket"] = "Invalid bracket",
            ["ErrorInsufficientPlayers"] = "Insufficient players for KO phase",
            ["ErrorBracketNotGenerated"] = "Bracket not generated",
            ["ErrorMatchNotFound"] = "Match not found",
            ["ErrorPlayerNotFound"] = "Player not found",
            
            // Validation messages
            ["ValidationBracketOK"] = "Bracket is valid",
            ["ValidationBracketError"] = "Bracket validation failed",
            ["ValidationPlayerMismatch"] = "Player mismatch",
            ["ValidationRoundMismatch"] = "Round mismatch",
            
            // =====================================
            // EXPORT AND DISPLAY
            // =====================================
            
            // Export functions
            ["ExportKOBracket"] = "Export KO Bracket",
            ["PrintKOBracket"] = "Print KO Bracket",
            ["SaveBracketImage"] = "Save Bracket as Image",
            ["CopyBracketToClipboard"] = "Copy Bracket to Clipboard",
            
            // Display options
            ["ShowPlayerNames"] = "Show Player Names",
            ["ShowMatchTimes"] = "Show Match Times",
            ["ShowRoundNames"] = "Show Round Names",
            ["ShowBracketConnections"] = "Show Bracket Connections",
            ["CompactView"] = "Compact View",
            ["DetailedView"] = "Detailed View",
            
            // =====================================
            // TOOLTIPS AND HELP
            // =====================================
            
            // Help texts
            ["KOTabHelp"] = "KO Tab Help",
            ["BracketGenerationHelp"] = "Bracket Generation Help",
            ["ByeManagementHelp"] = "Bye Management Help",
            ["DoubleEliminationHelp"] = "Double Elimination Help",
            
            // Tooltips
            ["KOTabTooltip"] = "Management of KO phase with bracket generation and match management",
            ["BracketTreeTooltip"] = "Graphical representation of the tournament tree",
            ["WinnerBracketTooltip"] = "Main bracket - losers are eliminated or moved to loser bracket",
            ["LoserBracketTooltip"] = "Consolation bracket - second chance for losers from winner bracket",
            ["ByeButtonTooltip"] = "Give a bye when a player is not available",
            ["UndoByeButtonTooltip"] = "Undo a given bye",
            
            // =====================================
            // STATE AND STATUS
            // =====================================
            
            // State messages
            ["BracketEmpty"] = "Bracket is empty",
            ["BracketComplete"] = "Bracket complete",
            ["BracketInProgress"] = "Bracket in progress",
            ["AllMatchesComplete"] = "All matches complete",
            ["MatchesPending"] = "Matches pending",
            
            // Progress information
            ["MatchesCompleted"] = "Matches completed: {0}/{1}",
            ["RoundsCompleted"] = "Rounds completed: {0}/{1}",
            ["PlayersRemaining"] = "Players remaining: {0}",
            ["NextMatch"] = "Next match: {0}",
            
            // =====================================
            // SPECIAL ACTIONS
            // =====================================
            
            // Special actions
            ["SimulateMatch"] = "Simulate Match",
            ["AutoAdvanceWinner"] = "Auto Advance Winner",
            ["ManualPlayerAdvancement"] = "Manual Player Advancement",
            ["BulkMatchUpdate"] = "Bulk Match Update",
            ["QuickMatchEntry"] = "Quick Match Entry",
            
            // Configuration options
            ["AutoAdvanceEnabled"] = "Auto advance enabled",
            ["AutoAdvanceDisabled"] = "Auto advance disabled",
            ["ManualAdvanceMode"] = "Manual advance mode",
            ["AutomaticByeHandling"] = "Automatic bye handling",
            ["ManualByeHandling"] = "Manual bye handling"
        };
    }
}