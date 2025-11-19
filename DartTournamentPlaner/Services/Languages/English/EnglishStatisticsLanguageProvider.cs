using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for statistics and WebSocket functions
/// </summary>
public class EnglishStatisticsLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Statistics Tab Header
            ["PlayerStatistics"] = "Player Statistics",
            ["TournamentStatistics"] = "Tournament Statistics",
            ["StatisticsOverview"] = "Statistics Overview",
            ["PlayerRankings"] = "Player Rankings",
            ["PerformanceAnalysis"] = "Performance Analysis",
            ["StatisticsSummary"] = "Statistics Summary",
            ["DetailedStats"] = "Detailed Statistics",

            // Statistics Categories
            ["MatchStatistics"] = "Match Statistics",
            ["ScoreStatistics"] = "Score Statistics",
            ["AccuracyStatistics"] = "Accuracy Statistics",
            ["FinishStatistics"] = "Finish Statistics",
            ["ConsistencyStats"] = "Consistency Statistics",
            ["ProgressionStats"] = "Progression Statistics",

            // Player Statistics Values
            ["TotalMatches"] = "Total Matches",
            ["MatchesWon"] = "Matches Won",
            ["MatchesLost"] = "Matches Lost",
            ["MatchWinRate"] = "Win Rate",
            ["OverallAverage"] = "Overall Average",
            ["TournamentAverage"] = "Tournament Average",
            ["BestAverage"] = "Best Average",
            ["WorstAverage"] = "Worst Average",
            ["HighestLegAverage"] = "Highest Leg Average",
            ["AverageScorePerDart"] = "Average per Dart",

            // Finish Statistics
            ["TotalCheckouts"] = "Total Checkouts",
            ["CheckoutRate"] = "Checkout Rate",
            ["HighFinishes"] = "High Finishes",
            ["TotalHighFinishes"] = "Total High Finishes",
            ["HighFinishScores"] = "HF Scores",
            ["HighestFinish"] = "Highest Finish",
            ["HighestFinishScore"] = "Highest Finish",
            ["AverageCheckout"] = "Average Checkout",
            ["CheckoutAccuracy"] = "Checkout Accuracy",
            ["FewestDartsToFinish"] = "Fewest Darts to Finish",
            ["AverageDartsPerCheckout"] = "⌀ Avg Darts per Checkout",
            ["FastestCheckout"] = "Fastest Checkout",

            // Score Statistics
            ["TotalMaximums"] = "180s",
            ["MaximumsPerGame"] = "180s per Game",
            ["Score26"] = "26s",
            ["TotalScore26"] = "26s",
            ["Score26PerGame"] = "26s per Game",
            ["HighScores"] = "High Scores",
            ["ScoreDistribution"] = "Score Distribution",
            ["Above100Average"] = "Above 100 Average",
            ["Above80Average"] = "Above 80 Average",
            ["Above60Average"] = "Above 60 Average",

            // Advanced Efficiency Statistics
            ["FastestMatch"] = "Fastest Match",
            ["FewestThrowsInMatch"] = "Fewest Throws",
            ["FastestMatchTooltip"] = "Shortest match duration across all matches (MM:SS)",
            ["FewestThrowsTooltip"] = "Fewest throws in a match (best throw efficiency)",

            // Time-based Statistics
            ["LastMatchDate"] = "Last Match",
            ["FirstMatchDate"] = "First Match",
            ["TotalPlayingTime"] = "Total Playing Time",
            ["AverageMatchDuration"] = "Average Match Duration",
            ["LongestMatch"] = "Longest Match",
            ["ShortestMatch"] = "Shortest Match",
            ["PlayingDays"] = "Playing Days",
            ["MatchesPerDay"] = "Matches per Day",

            // Tournament Context Statistics
            ["GroupPhaseStats"] = "Group Phase Statistics",
            ["FinalsStats"] = "Finals Statistics",
            ["KOPhaseStats"] = "KO Phase Statistics",
            ["OverallTournamentStats"] = "Overall Tournament Statistics",
            ["PhaseComparison"] = "Phase Comparison",
            ["PerformanceByPhase"] = "Performance by Phase",

            // Statistics Sorting and Filtering
            ["SortBy"] = "Sort by",
            ["SortByName"] = "Name",
            ["SortByAverage"] = "Average",
            ["SortByMatches"] = "Matches",
            ["SortByWinRate"] = "Win Rate",
            ["SortByMaximums"] = "180s",
            ["SortByHighFinishes"] = "High Finishes",
            ["SortByCheckouts"] = "Checkouts",
            ["FilterPlayers"] = "Filter Players",
            ["ShowAllPlayers"] = "Show All Players",
            ["ShowTopPlayers"] = "Show Top Players",
            ["MinimumMatches"] = "Minimum Matches",

            // Statistics Actions
            ["RefreshStatistics"] = "Refresh Statistics",
            ["ExportStatistics"] = "Export Statistics",
            ["PrintStatistics"] = "Print Statistics",
            ["ResetStatistics"] = "Reset Statistics",
            ["SaveStatistics"] = "Save Statistics",
            ["LoadStatistics"] = "Load Statistics",
            ["CompareToAverage"] = "Compare to Average",

            // Statistics Messages
            ["NoStatisticsAvailable"] = "No statistics available",
            ["StatisticsLoading"] = "Loading statistics...",
            ["StatisticsUpdated"] = "Statistics updated",
            ["ErrorLoadingStatistics"] = "Error loading statistics: {0}",
            ["StatisticsNotEnabled"] = "Statistics not enabled",
            ["InsufficientDataForStats"] = "Insufficient data for statistics",

            // Detail Views
            ["PlayerDetails"] = "Player Details for {0}",
            ["MatchHistory"] = "Match History",
            ["ScoreHistory"] = "Score History",
            ["PerformanceTrend"] = "Performance Trend",
            ["StrengthsWeaknesses"] = "Strengths & Weaknesses",
            ["RecentPerformance"] = "Recent Performance",
            ["CareerHighlights"] = "Career Highlights",

            // Comparison Features
            ["ComparePlayer"] = "Compare Player",
            ["PlayerComparison"] = "Player Comparison",
            ["CompareWith"] = "Compare with",
            ["ComparisonResult"] = "Comparison Result",
            ["BetterThan"] = "Better than {0}",
            ["WorseThan"] = "Worse than {0}",
            ["SimilarTo"] = "Similar to {0}",

            // Ranking and Positions
            ["CurrentRank"] = "Current Rank",
            ["RankByAverage"] = "Rank by Average",
            ["RankByWinRate"] = "Rank by Win Rate",
            ["RankByMatches"] = "Rank by Matches",
            ["TopPerformer"] = "Top Performer",
            ["RankingChange"] = "Ranking Change",
            ["MovedUp"] = "Moved up {0}",
            ["MovedDown"] = "Moved down {0}",
            ["NoChange"] = "No Change",

            // Statistics Tooltips and Help
            ["AverageTooltip"] = "Average points per 3 darts",
            ["WinRateTooltip"] = "Percentage of matches won",
            ["MaximumsTooltip"] = "Number of 180-point throws",
            ["HighFinishesTooltip"] = "Checkouts over 100 points",
            ["CheckoutRateTooltip"] = "Percentage of successful checkout attempts",
            ["ConsistencyTooltip"] = "Consistency of performance",

            // Advanced Statistics Features
            ["TrendAnalysis"] = "Trend Analysis",
            ["PerformanceGraph"] = "Performance Graph",
            ["StatisticalSignificance"] = "Statistical Significance",
            ["ConfidenceInterval"] = "Confidence Interval",
            ["StandardDeviation"] = "Standard Deviation",
            ["Correlation"] = "Correlation",
            ["Regression"] = "Regression",
            ["PredictiveAnalysis"] = "Predictive Analysis",

            // Debug and Developer Info
            ["DebugStatistics"] = "Debug Statistics",
            ["StatisticsDebugInfo"] = "Statistics Debug Info",
            ["DataIntegrity"] = "Data Integrity",
            ["ValidationStatus"] = "Validation Status",
            ["LastUpdate"] = "Last Update",
            ["DataSource"] = "Data Source",
            ["RecordCount"] = "Record Count",

            // WEBSOCKET STATISTICS EXTRACTION
            // Statistics Extraction Messages
            ["ProcessingMatchUpdate"] = "Processing match update for class {0}",
            ["SkippingNonMatchResult"] = "Skipping non-match result update: {0}",
            ["ProcessingTopLevelStats"] = "Processing top-level player statistics for {0} vs {1}",
            ["ProcessingSimpleStats"] = "Processing simple player statistics for {0} vs {1}",
            ["ProcessingEnhancedStats"] = "Processing enhanced dart statistics for {0} vs {1}",
            ["FallbackNotesExtraction"] = "Fallback to notes-based statistics extraction",
            ["ErrorProcessingMatchResult"] = "Error processing match result: {0}",

            // JSON Parsing Messages
            ["ProcessingJSONFromNotes"] = "Processing JSON from notes field for top-level statistics",
            ["NoJSONDataFound"] = "No JSON data found in notes for top-level statistics",
            ["AvailableTopLevelProperties"] = "Available top-level properties: {0}",
            ["NoTopLevelStatsFound"] = "No top-level statistics found in JSON structure",
            ["FoundTopLevelStats"] = "Top-level statistics found in JSON",

            // Player Data Extraction
            ["ExtractedPlayer1"] = "Player1 extracted: Avg {0}, 180s: {1}, HF: {2}, 26s: {3}",
            ["ExtractedPlayer2"] = "Player2 extracted: Avg {0}, 180s: {1}, HF: {2}, 26s: {3}",
            ["ParsedDuration"] = "Duration parsed: {0} minutes",
            ["MatchFormat"] = "Match format: {0}",

            // Player Name Extraction
            ["FoundPlayer1NameFromResult"] = "player1Name from matchUpdate.result found: {0}",
            ["FoundPlayer2NameFromResult"] = "player2Name from matchUpdate.result found: {0}",
            ["UsingFallbackPlayerNames"] = "Using fallback player name extraction",
            ["FallbackPlayerNames"] = "Fallback player names: {0}, {1}",
            ["FinalExtractedStats"] = "Final extracted statistics: {0} vs {1}",
            ["ErrorParsingTopLevelStats"] = "Error parsing top-level player statistics: {0}",

            // High Finish Scores Extraction
            ["ExtractedHighFinishScores"] = "High finish scores extracted: [{0}]",
            ["NoPlayerNameFound"] = "No player name found for Player {0}, using fallback",
            ["ErrorExtractingPlayerName"] = "Error extracting player name: {0}",
            ["FoundPlayerNameInResult"] = "player{0}Name in matchUpdate.result found: {1}",

            // Statistics Processing Success
            ["SuccessfullyProcessedSimpleStats"] = "Simple statistics successfully processed for {0} and {1}",
            ["SuccessfullyProcessedEnhancedStats"] = "Enhanced statistics successfully processed for {0} and {1}",
            ["ErrorProcessingSimpleStats"] = "Error processing simple statistics: {0}",
            ["ErrorProcessingEnhancedStats"] = "Error processing enhanced statistics: {0}",

            // Winner Determination
            ["WinnerDetermined"] = "Winner determined: {0} (Sets: {1}-{2}, Legs: {3}-{4})",
            ["ErrorDeterminingWinner"] = "Error determining winner:",

            // Enhanced Statistics Extraction
            ["ExtractedEnhancedDetails"] = "{0} high finish details extracted",
            ["MatchDurationMs"] = "Match duration: {0}ms = {1}",
            ["MatchDurationString"] = "Match duration string: {0}",
            ["ExtractedStartTime"] = "Start time: {0}",
            ["ExtractedEndTime"] = "End time: {0}",
            ["ExtractedTotalThrows"] = "Total throws: {0}",
            ["ExtractedCheckouts"] = "Checkouts extracted: {0}",
            ["ExtractedTotalScore"] = "Total score extracted: {0}",
            ["ExtractedHighFinishDetails"] = "{0} High finish details extracted",
            ["GameRulesExtracted"] = "Game rules extracted: {0}, Double Out: {1}, Starting score: {2}",
            ["VersionInfoExtracted"] = "Version: {0}, Submitted via: {1}",
            ["DurationFormatted"] = "Duration formatted: {0}ms = {1}",

            // Match Duration Formatting
            ["DurationSeconds"] = "{0} seconds",
            ["DurationMinutes"] = "{0:D2}:{1:D2} minutes",
            ["DurationHours"] = "{0}:{1:D2}:{2:D2} hours",

            // Enhanced Player Statistics
            ["PlayerStatsValidation"] = "Player data validation: {0} (Avg: {1}, Throws: {2}, Score: {3}, Checkouts: {4})",
            ["DetailedStatsMerge"] = "Detailed statistics merged: Checkouts: {0}, TotalThrows: {1}, TotalScore: {2}",
            ["RealDataUsage"] = "Using real data: Throws: {0}, Score: {1}, Checkouts: {2}",

            // High Finish Details Processing
            ["HighFinishDetailsParsed"] = "High finish details parsed: Finish {0}, Darts: [{1}], Timestamp: {2}",
            ["HighFinishScoresExtracted"] = "High finish scores extracted: [{0}]",
            ["CheckoutDetailsCreated"] = "Checkout details created: {0} checkouts",

            // Match Metadata
            ["MatchMetadataExtracted"] = "Match metadata extracted: Format {0}, Start {1}, End {2}, Duration {3}ms",
            ["GameModeDetected"] = "Game mode detected: {0}",
            ["SubmissionInfoExtracted"] = "Submission info extracted: {0} v{1}",

            // Statistics Calculation
            ["StatisticsCalculated"] = "Statistics calculated for {0}: Avg {1}, {2} throws, {3} score",
            ["PerformanceMetrics"] = "Performance metrics: Average per throw: {0:F1}, HF rate: {1:F2}, Maximum rate: {2:F2}",
            ["DetailListsSizes"] = "Detail lists sizes: {0} HF, {1} Max, {2} Score26, {3} Checkouts",

            // Direct WebSocket Extraction
            ["DirectWebSocketExtraction"] = "Direct WebSocket statistics extraction",
            ["NoValidJSONInWebSocket"] = "No valid JSON data found in WebSocket message",
            ["ProcessingDirectWebSocketStats"] = "Processing direct WebSocket statistics",
            ["NoDirectStatsFound"] = "No direct statistics found in WebSocket message",
            ["FoundDirectStatsInWebSocket"] = "Direct statistics found in WebSocket message",
            ["NoValidDirectWebSocketData"] = "No valid direct WebSocket statistics data found",
            ["DirectWebSocketExtractionSuccess"] = "Direct WebSocket extraction successful: {0} vs {1}",
            ["ErrorParsingDirectWebSocketStats"] = "Error parsing direct WebSocket statistics: {0}",
            ["DirectWebSocketPlayer1"] = "Direct WebSocket Player1: {0} - Avg {1}, 180s: {2}, HF: {3}, 26s: {4}, Checkouts: {5}",
            ["DirectWebSocketPlayer2"] = "Direct WebSocket Player2: {0} - Avg {1}, 180s: {2}, HF: {3}, 26s: {4}, Checkouts: {5}",
            ["DirectWebSocketMatchDuration"] = "Direct WebSocket match duration: {0}ms = {1}",
            ["ProcessingDirectWebSocketStatsFor"] = "Processing direct WebSocket statistics for {0} vs {1}"
        };
    }
}