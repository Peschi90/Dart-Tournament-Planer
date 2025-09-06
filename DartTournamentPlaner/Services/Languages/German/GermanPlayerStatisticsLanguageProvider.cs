using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche �bersetzungen f�r PlayerStatisticsView und verwandte Statistik-Anzeigen
/// Erg�nzt die GermanStatisticsLanguageProvider um spezifische UI-Texte
/// </summary>
public class GermanPlayerStatisticsLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // PlayerStatisticsView - Spezifische UI-Texte (die nicht in GermanStatisticsLanguageProvider enthalten sind)
            ["PlayersText"] = "Spieler mit Statistiken",

            // DataGrid Column Headers - spezifische f�r PlayerStatisticsView
            ["PlayerHeader"] = "Spieler",
            ["LastMatchDate"] = "Letztes Match",
            ["FastestMatch"] = "Schnellstes Match",
            ["FewestThrowsInMatch"] = "Wenigste W�rfe",

            // Status-Meldungen - spezifisch f�r PlayerStatisticsView
            ["StatisticsLoading"] = "Statistiken werden geladen...",
            ["StatisticsUpdated"] = "Statistiken aktualisiert",
            ["StatisticsNotEnabled"] = "Statistiken nicht aktiviert",
            ["ShowAllPlayers"] = "Angezeigt",

            // Tooltips und Hilfe-Texte - spezifisch f�r PlayerStatisticsView
            ["FastestMatchTooltip"] = "K�rzeste Spieldauer �ber alle Matches (MM:SS)",
            ["FewestThrowsTooltip"] = "Wenigste W�rfe in einem Match (beste Wurf-Effizienz)",
            ["HighFinishScoresTooltip"] = "Alle High Finish Scores durch | getrennt",
            ["PlayerHeaderTooltip"] = "Name des Spielers",
            ["MatchWinRateTooltip"] = "Prozentsatz der gewonnenen Spiele",
            ["BestAverageTooltip"] = "H�chster Average in einem Match",
            ["HighestFinishTooltip"] = "H�chstes High Finish dieses Spielers",
            ["Score26Tooltip"] = "Anzahl der schlechten W�rfe (26 Punkte oder weniger)",
            ["LastMatchTooltip"] = "Datum des letzten gespielten Matches"
        };
    }
}