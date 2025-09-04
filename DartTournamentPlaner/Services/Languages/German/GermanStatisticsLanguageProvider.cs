using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für Statistiken und WebSocket-Funktionen
/// </summary>
public class GermanStatisticsLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Statistiken Tab Header
            ["PlayerStatistics"] = "Spieler-Statistiken",
            ["TournamentStatistics"] = "Turnier-Statistiken",
            ["StatisticsOverview"] = "Statistik-Übersicht",
            ["PlayerRankings"] = "Spieler-Rangliste",
            ["PerformanceAnalysis"] = "Leistungsanalyse",
            ["StatisticsSummary"] = "Statistik-Zusammenfassung",
            ["DetailedStats"] = "Detaillierte Statistiken",

            // Statistik-Kategorien
            ["MatchStatistics"] = "Match-Statistiken",
            ["ScoreStatistics"] = "Punkte-Statistiken",
            ["AccuracyStatistics"] = "Genauigkeits-Statistiken",
            ["FinishStatistics"] = "Finish-Statistiken",
            ["ConsistencyStats"] = "Konsistenz-Statistiken",
            ["ProgressionStats"] = "Fortschritts-Statistiken",

            // Spieler-Statistik Werte
            ["TotalMatches"] = "Gesamt Spiele",
            ["MatchesWon"] = "Siege",
            ["MatchesLost"] = "Niederlagen",
            ["MatchWinRate"] = "Sieg-Quote",
            ["OverallAverage"] = "Gesamt-Average",
            ["TournamentAverage"] = "Turnier-Average",
            ["BestAverage"] = "Bester Average",
            ["WorstAverage"] = "Schlechtester Average",
            ["HighestLegAverage"] = "Höchster Leg-Average",
            ["AverageScorePerDart"] = "Durchschnitt pro Pfeil",

            // Finish-Statistiken
            ["TotalCheckouts"] = "Gesamt Checkouts",
            ["CheckoutRate"] = "Checkout-Quote",
            ["HighFinishes"] = "High Finishes",
            ["TotalHighFinishes"] = "Gesamt High Finishes",
            ["HighFinishScores"] = "HF Scores",
            ["HighestFinish"] = "Höchstes Finish",
            ["HighestFinishScore"] = "Höchstes Finish",
            ["AverageCheckout"] = "Durchschnittliches Checkout",
            ["CheckoutAccuracy"] = "Checkout-Genauigkeit",
            ["FewestDartsToFinish"] = "Wenigste Pfeile zum Finish",
            ["AverageDartsPerCheckout"] = "Ø Pfeile pro Checkout",
            ["FastestCheckout"] = "Schnellstes Checkout",

            // Score-Statistiken 
            ["TotalMaximums"] = "180er",
            ["MaximumsPerGame"] = "180er pro Spiel",
            ["Score26"] = "26er",
            ["TotalScore26"] = "26er",
            ["Score26PerGame"] = "26er pro Spiel",
            ["HighScores"] = "Hohe Scores",
            ["ScoreDistribution"] = "Score-Verteilung",
            ["Above100Average"] = "Über 100 Average",
            ["Above80Average"] = "Über 80 Average",
            ["Above60Average"] = "Über 60 Average",

            // Erweiterte Effizienz-Statistiken
            ["FastestMatch"] = "Schnellstes Match",
            ["FewestThrowsInMatch"] = "Wenigste Würfe",
            ["FastestMatchTooltip"] = "Kürzeste Spieldauer über alle Matches (MM:SS)",
            ["FewestThrowsTooltip"] = "Wenigste Würfe in einem Match (beste Wurf-Effizienz)",

            // Zeit-basierte Statistiken
            ["LastMatchDate"] = "Letztes Spiel",
            ["FirstMatchDate"] = "Erstes Spiel",
            ["TotalPlayingTime"] = "Gesamt Spielzeit",
            ["AverageMatchDuration"] = "Durchschnittliche Spieldauer",
            ["LongestMatch"] = "Längstes Spiel",
            ["ShortestMatch"] = "Kürzestes Spiel",
            ["PlayingDays"] = "Spieltage",
            ["MatchesPerDay"] = "Spiele pro Tag",

            // Turnier-Kontext Statistiken
            ["GroupPhaseStats"] = "Gruppenphase-Statistiken",
            ["FinalsStats"] = "Final-Statistiken",
            ["KOPhaseStats"] = "KO-Phasen-Statistiken",
            ["OverallTournamentStats"] = "Gesamt-Turnier-Statistiken",
            ["PhaseComparison"] = "Phasen-Vergleich",
            ["PerformanceByPhase"] = "Leistung nach Phase",

            // Statistik-Sortierung und Filter
            ["SortBy"] = "Sortieren nach",
            ["SortByName"] = "Name",
            ["SortByAverage"] = "Average",
            ["SortByMatches"] = "Spiele",
            ["SortByWinRate"] = "Sieg-Quote",
            ["SortByMaximums"] = "180er",
            ["SortByHighFinishes"] = "High Finishes",
            ["SortByCheckouts"] = "Checkouts",
            ["FilterPlayers"] = "Spieler filtern",
            ["ShowAllPlayers"] = "Alle Spieler anzeigen",
            ["ShowTopPlayers"] = "Top-Spieler anzeigen",
            ["MinimumMatches"] = "Mindest-Spielanzahl",

            // Statistik-Aktionen
            ["RefreshStatistics"] = "Statistiken aktualisieren",
            ["ExportStatistics"] = "Statistiken exportieren",
            ["PrintStatistics"] = "Statistiken drucken",
            ["ResetStatistics"] = "Statistiken zurücksetzen",
            ["SaveStatistics"] = "Statistiken speichern",
            ["LoadStatistics"] = "Statistiken laden",
            ["CompareToAverage"] = "Mit Durchschnitt vergleichen",

            // Statistik-Meldungen
            ["NoStatisticsAvailable"] = "Keine Statistiken verfügbar",
            ["StatisticsLoading"] = "Statistiken werden geladen...",
            ["StatisticsUpdated"] = "Statistiken aktualisiert",
            ["ErrorLoadingStatistics"] = "Fehler beim Laden der Statistiken: {0}",
            ["StatisticsNotEnabled"] = "Statistiken sind nicht aktiviert",
            ["InsufficientDataForStats"] = "Ungenügend Daten für Statistiken",

            // Detail-Ansichten
            ["PlayerDetails"] = "Spieler-Details für {0}",
            ["MatchHistory"] = "Spiel-Historie",
            ["ScoreHistory"] = "Score-Historie",
            ["PerformanceTrend"] = "Leistungstrend",
            ["StrengthsWeaknesses"] = "Stärken & Schwächen",
            ["RecentPerformance"] = "Aktuelle Leistung",
            ["CareerHighlights"] = "Karriere-Höhepunkte",

            // Vergleichs-Features
            ["ComparePlayer"] = "Spieler vergleichen",
            ["PlayerComparison"] = "Spieler-Vergleich",
            ["CompareWith"] = "Vergleichen mit",
            ["ComparisonResult"] = "Vergleichsergebnis",
            ["BetterThan"] = "Besser als {0}",
            ["WorseThan"] = "Schlechter als {0}",
            ["SimilarTo"] = "Ähnlich zu {0}",

            // Ranking und Positionen
            ["CurrentRank"] = "Aktuelle Position",
            ["RankByAverage"] = "Position nach Average",
            ["RankByWinRate"] = "Position nach Sieg-Quote",
            ["RankByMatches"] = "Position nach Spielen",
            ["TopPerformer"] = "Top-Performer",
            ["RankingChange"] = "Positionsänderung",
            ["MovedUp"] = "Aufgestiegen um {0}",
            ["MovedDown"] = "Abgestiegen um {0}",
            ["NoChange"] = "Keine Änderung",

            // Statistik-Tooltips und Hilfe
            ["AverageTooltip"] = "Durchschnittliche Punkte pro 3 Pfeile",
            ["WinRateTooltip"] = "Prozentsatz der gewonnenen Spiele",
            ["MaximumsTooltip"] = "Anzahl der 180-Punkte-Würfe",
            ["HighFinishesTooltip"] = "Checkouts über 100 Punkte",
            ["CheckoutRateTooltip"] = "Prozentsatz erfolgreicher Checkout-Versuche",
            ["ConsistencyTooltip"] = "Gleichmäßigkeit der Leistung",

            // Erweiterte Statistik-Features
            ["TrendAnalysis"] = "Trend-Analyse",
            ["PerformanceGraph"] = "Leistungsdiagramm",
            ["StatisticalSignificance"] = "Statistische Signifikanz",
            ["ConfidenceInterval"] = "Konfidenzintervall",
            ["StandardDeviation"] = "Standardabweichung",
            ["Correlation"] = "Korrelation",
            ["Regression"] = "Regression",
            ["PredictiveAnalysis"] = "Vorhersage-Analyse",

            // Debug und Entwickler-Info
            ["DebugStatistics"] = "Debug-Statistiken",
            ["StatisticsDebugInfo"] = "Statistik Debug-Info",
            ["DataIntegrity"] = "Daten-Integrität",
            ["ValidationStatus"] = "Validierungs-Status",
            ["LastUpdate"] = "Letzte Aktualisierung",
            ["DataSource"] = "Datenquelle",
            ["RecordCount"] = "Anzahl Datensätze",

            // WEBSOCKET STATISTIK-EXTRAKTION
            // Statistik-Extraktion Meldungen
            ["ProcessingMatchUpdate"] = "Verarbeite Match-Update für Klasse {0}",
            ["SkippingNonMatchResult"] = "Überspringe Nicht-Match-Ergebnis Update: {0}",
            ["ProcessingTopLevelStats"] = "Verarbeite Top-Level-Spieler-Statistiken für {0} vs {1}",
            ["ProcessingSimpleStats"] = "Verarbeite einfache Spieler-Statistiken für {0} vs {1}",
            ["ProcessingEnhancedStats"] = "Verarbeite erweiterte Dart-Statistiken für {0} vs {1}",
            ["FallbackNotesExtraction"] = "Fallback auf Notes-basierte Statistik-Extraktion",
            ["ErrorProcessingMatchResult"] = "Fehler beim Verarbeiten des Match-Ergebnisses: {0}",

            // JSON-Parsing Meldungen
            ["ProcessingJSONFromNotes"] = "Verarbeite JSON aus Notes-Feld für Top-Level-Statistiken",
            ["NoJSONDataFound"] = "Keine JSON-Daten in Notes für Top-Level-Statistiken gefunden",
            ["AvailableTopLevelProperties"] = "Verfügbare Top-Level-Properties: {0}",
            ["NoTopLevelStatsFound"] = "Keine Top-Level-Statistiken in JSON-Struktur gefunden",
            ["FoundTopLevelStats"] = "Top-Level-Statistiken in JSON gefunden",

            // Spieler-Daten Extraktion
            ["ExtractedPlayer1"] = "Player1 extrahiert: Avg {0}, 180er: {1}, HF: {2}, 26er: {3}",
            ["ExtractedPlayer2"] = "Player2 extrahiert: Avg {0}, 180er: {1}, HF: {2}, 26er: {3}",
            ["ParsedDuration"] = "Dauer geparst: {0} Minuten",
            ["MatchFormat"] = "Match-Format: {0}",

            // Spielernamen Extraktion
            ["FoundPlayer1NameFromResult"] = "player1Name aus matchUpdate.result gefunden: {0}",
            ["FoundPlayer2NameFromResult"] = "player2Name aus matchUpdate.result gefunden: {0}",
            ["UsingFallbackPlayerNames"] = "Verwende Fallback-Spielernamen-Extraktion",
            ["FallbackPlayerNames"] = "Fallback-Spielernamen: {0}, {1}",
            ["FinalExtractedStats"] = "Final extrahierte Statistiken: {0} vs {1}",
            ["ErrorParsingTopLevelStats"] = "Fehler beim Parsen der Top-Level-Spieler-Statistiken: {0}",

            // High Finish Scores Extraktion
            ["ExtractedHighFinishScores"] = "High Finish Scores extrahiert: [{0}]",
            ["NoPlayerNameFound"] = "Kein Spielername für Player {0} gefunden, verwende Fallback",
            ["ErrorExtractingPlayerName"] = "Fehler beim Extrahieren des Spielernamens: {0}",
            ["FoundPlayerNameInResult"] = "player{0}Name in matchUpdate.result gefunden: {1}",

            // Statistik-Verarbeitung Erfolg
            ["SuccessfullyProcessedSimpleStats"] = "Einfache Statistiken erfolgreich verarbeitet für {0} und {1}",
            ["SuccessfullyProcessedEnhancedStats"] = "Erweiterte Statistiken erfolgreich verarbeitet für {0} und {1}",
            ["ErrorProcessingSimpleStats"] = "Fehler beim Verarbeiten einfacher Statistiken: {0}",
            ["ErrorProcessingEnhancedStats"] = "Fehler beim Verarbeiten erweiterter Statistiken: {0}",

            // Gewinner-Bestimmung
            ["WinnerDetermined"] = "Gewinner bestimmt: {0} (Sets: {1}-{2}, Legs: {3}-{4})",
            ["ErrorDeterminingWinner"] = "Fehler beim Bestimmen des Gewinners:",

            // Erweiterte Statistik-Extraktion
            ["ExtractedEnhancedDetails"] = "{0} high finish details extrahiert",
            ["MatchDurationMs"] = "Match-Dauer: {0}ms = {1}",
            ["MatchDurationString"] = "Match-Dauer String: {0}",
            ["ExtractedStartTime"] = "Startzeit: {0}",
            ["ExtractedEndTime"] = "Endzeit: {0}",
            ["ExtractedTotalThrows"] = "Gesamtwürfe: {0}",
            ["ExtractedCheckouts"] = "Checkouts extrahiert: {0}",
            ["ExtractedTotalScore"] = "Gesamtpunktzahl extrahiert: {0}",
            ["ExtractedHighFinishDetails"] = "{0} High Finish Details extrahiert",
            ["GameRulesExtracted"] = "Spielregeln extrahiert: {0}, Double Out: {1}, Startpunktzahl: {2}",
            ["VersionInfoExtracted"] = "Version: {0}, Eingereicht via: {1}",
            ["DurationFormatted"] = "Dauer formatiert: {0}ms = {1}",

            // Match-Dauer Formatierung
            ["DurationSeconds"] = "{0} Sekunden",
            ["DurationMinutes"] = "{0:D2}:{1:D2} Minuten",
            ["DurationHours"] = "{0}:{1:D2}:{2:D2} Stunden",

            // Erweiterte Player-Statistiken
            ["PlayerStatsValidation"] = "Spieler-Daten Validierung: {0} (Avg: {1}, Throws: {2}, Score: {3}, Checkouts: {4})",
            ["DetailedStatsMerge"] = "Detaillierte Statistiken zusammengeführt: Checkouts: {0}, TotalThrows: {1}, TotalScore: {2}",
            ["RealDataUsage"] = "Verwende echte Daten: Throws: {0}, Score: {1}, Checkouts: {2}",

            // High Finish Details Verarbeitung
            ["HighFinishDetailsParsed"] = "High Finish Details geparst: Finish {0}, Darts: [{1}], Zeitstempel: {2}",
            ["HighFinishScoresExtracted"] = "High Finish Scores extrahiert: [{0}]",
            ["CheckoutDetailsCreated"] = "Checkout Details erstellt: {0} Checkouts",

            // Match-Metadaten
            ["MatchMetadataExtracted"] = "Match-Metadaten extrahiert: Format {0}, Start {1}, Ende {2}, Dauer {3}ms",
            ["GameModeDetected"] = "Spielmodus erkannt: {0}",
            ["SubmissionInfoExtracted"] = "Übertragungsinfo extrahiert: {0} v{1}",

            // Statistik-Berechnung
            ["StatisticsCalculated"] = "Statistiken berechnet für {0}: Avg {1}, {2} Würfe, {3} Punkte",
            ["PerformanceMetrics"] = "Leistungsmetriken: Durchschnitt pro Wurf: {0:F1}, HF-Rate: {1:F2}, Maximum-Rate: {2:F2}",
            ["DetailListsSizes"] = "Detail-Listen Größen: {0} HF, {1} Max, {2} Score26, {3} Checkouts",

            // Direkte WebSocket-Extraktion
            ["DirectWebSocketExtraction"] = "Direkte WebSocket-Statistik-Extraktion",
            ["NoValidJSONInWebSocket"] = "Keine gültigen JSON-Daten in WebSocket-Nachricht gefunden",
            ["ProcessingDirectWebSocketStats"] = "Verarbeite direkte WebSocket-Statistiken",
            ["NoDirectStatsFound"] = "Keine direkten Statistiken in WebSocket-Nachricht gefunden",
            ["FoundDirectStatsInWebSocket"] = "Direkte Statistiken in WebSocket-Nachricht gefunden",
            ["NoValidDirectWebSocketData"] = "Keine gültigen direkten WebSocket-Statistikdaten gefunden",
            ["DirectWebSocketExtractionSuccess"] = "Direkte WebSocket-Extraktion erfolgreich: {0} vs {1}",
            ["ErrorParsingDirectWebSocketStats"] = "Fehler beim Parsen direkter WebSocket-Statistiken: {0}",
            ["DirectWebSocketPlayer1"] = "Direkter WebSocket Player1: {0} - Avg {1}, 180s: {2}, HF: {3}, 26s: {4}, Checkouts: {5}",
            ["DirectWebSocketPlayer2"] = "Direkter WebSocket Player2: {0} - Avg {1}, 180s: {2}, HF: {3}, 26s: {4}, Checkouts: {5}",
            ["DirectWebSocketMatchDuration"] = "Direkte WebSocket Match-Dauer: {0}ms = {1}",
            ["ProcessingDirectWebSocketStatsFor"] = "Verarbeite direkte WebSocket-Statistiken für {0} vs {1}"
        };
    }
}