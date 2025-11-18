using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für PlayerStatisticsView und verwandte Statistik-Anzeigen
/// Ergänzt die GermanStatisticsLanguageProvider um spezifische UI-Texte
/// </summary>
public class GermanPlayerStatisticsLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // PlayerStatisticsView - Spezifische UI-Texte (die nicht in GermanStatisticsLanguageProvider enthalten sind)
            ["PlayersText"] = "Spieler mit Statistiken",

            // DataGrid Column Headers - spezifische für PlayerStatisticsView
            ["PlayerHeader"] = "Spieler",
            ["LastMatchDate"] = "Letztes Match",
            ["FastestMatch"] = "Schnellstes Match",
            ["FewestThrowsInMatch"] = "Wenigste Würfe",
            ["FewestDartsPerLeg"] = "Min Darts/Leg", // ? NEU
            ["AverageDartsPerLeg"] = "? Darts/Leg", // ? NEU
            ["BestLegEfficiency"] = "Beste Leg-Effizienz", // ? NEU

            // Status-Meldungen - spezifisch für PlayerStatisticsView
            ["StatisticsLoading"] = "Statistiken werden geladen...",
            ["StatisticsUpdated"] = "Statistiken aktualisiert",
            ["StatisticsNotEnabled"] = "Statistiken nicht aktiviert",
            ["ShowAllPlayers"] = "Angezeigt",

            // Tooltips und Hilfe-Texte - spezifisch für PlayerStatisticsView
            ["FastestMatchTooltip"] = "Kürzeste Spieldauer über alle Matches (MM:SS)",
            ["FewestThrowsTooltip"] = "Wenigste Würfe in einem Match (beste Wurf-Effizienz)",
            ["FewestDartsPerLegTooltip"] = "Wenigste benötigte Darts für ein gewonnenes Leg", // ? NEU
            ["AverageDartsPerLegTooltip"] = "Durchschnittliche Darts pro gewonnenem Leg", // ? NEU
            ["BestLegEfficiencyTooltip"] = "Beste Leg-Effizienz (wenigste Darts + Average)", // ? NEU
            ["HighFinishScoresTooltip"] = "Alle High Finish Scores durch | getrennt",
            ["PlayerHeaderTooltip"] = "Name des Spielers",
            ["MatchWinRateTooltip"] = "Prozentsatz der gewonnenen Spiele",
            ["BestAverageTooltip"] = "Höchster Average in einem Match",
            ["HighestFinishTooltip"] = "Höchstes High Finish dieses Spielers",
            ["Score26Tooltip"] = "Anzahl der schlechten Würfe (26 Punkte oder weniger)",
            ["LastMatchTooltip"] = "Datum des letzten gespielten Matches"
        };
    }
}