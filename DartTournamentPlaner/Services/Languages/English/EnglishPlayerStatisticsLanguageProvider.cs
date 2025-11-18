using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for PlayerStatisticsView and related statistics displays
/// Complements the EnglishStatisticsLanguageProvider with specific UI texts
/// </summary>
public class EnglishPlayerStatisticsLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // PlayerStatisticsView - Specific UI texts (not included in EnglishStatisticsLanguageProvider)
            ["PlayersText"] = "players with statistics",

            // DataGrid Column Headers - specific for PlayerStatisticsView
            ["PlayerHeader"] = "Player",
            ["LastMatchDate"] = "Last Match",
            ["FastestMatch"] = "Fastest Match",
            ["FewestThrowsInMatch"] = "Fewest Throws",
            ["FewestDartsPerLeg"] = "Min Darts/Leg", // ? NEW
            ["AverageDartsPerLeg"] = "? Darts/Leg", // ? NEW
            ["BestLegEfficiency"] = "Best Leg Efficiency", // ? NEW

            // Status messages - specific for PlayerStatisticsView
            ["StatisticsLoading"] = "Loading statistics...",
            ["StatisticsUpdated"] = "Statistics updated",
            ["StatisticsNotEnabled"] = "Statistics not enabled",
            ["ShowAllPlayers"] = "Showing",

            // Tooltips and help texts - specific for PlayerStatisticsView
            ["FastestMatchTooltip"] = "Shortest match duration across all matches (MM:SS)",
            ["FewestThrowsTooltip"] = "Fewest throws in a single match (best throw efficiency)",
            ["FewestDartsPerLegTooltip"] = "Fewest darts needed to win a leg", // ? NEW
            ["AverageDartsPerLegTooltip"] = "Average darts per won leg", // ? NEW
            ["BestLegEfficiencyTooltip"] = "Best leg efficiency (fewest darts + average)", // ? NEW
            ["HighFinishScoresTooltip"] = "All High Finish scores separated by |",
            ["PlayerHeaderTooltip"] = "Player name",
            ["MatchWinRateTooltip"] = "Percentage of won games",
            ["BestAverageTooltip"] = "Highest average in a single match",
            ["HighestFinishTooltip"] = "Highest High Finish by this player",
            ["Score26Tooltip"] = "Number of bad throws (26 points or less)",
            ["LastMatchTooltip"] = "Date of the last played match"
        };
    }
}