using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für Print-Service und Druckfunktionen
/// </summary>
public class GermanPrintLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // PRINT SERVICE ÜBERSETZUNGEN
            ["PrintError"] = "Druckfehler",
            ["ErrorCreatingDocument"] = "Fehler beim Erstellen des Druckdokuments.",
            ["ErrorPrinting"] = "Fehler beim Drucken: {0}",
            ["TournamentOverviewPrint"] = "Turnierübersicht - {0}",
            ["CreatedWith"] = "Erstellt mit Dart Tournament Planner",
            ["GameRulesLabel"] = "Spielregeln: {0}",
            ["CurrentPhaseLabel"] = "Aktuelle Phase: {0}",
            ["NotStarted"] = "Nicht begonnen",
            ["GroupsPlayersLabel"] = "Gruppen: {0}, Spieler gesamt: {1}",
            ["GroupsOverview"] = "Gruppen-Übersicht",
            ["PlayersCount"] = "Spieler: {0}",
            ["MatchesStatus"] = "{0} von {1} Spielen beendet",
            ["Table"] = "Tabelle",
            ["NoStandingsAvailable"] = "Noch keine Tabelle verfügbar.",
            ["MatchResults"] = "Spielergebnisse",
            ["NoMatchesAvailable"] = "Keine Spiele vorhanden.",
            ["WinnerBracketHeader"] = "{0} - Winner Bracket",
            ["LoserBracketHeader"] = "{0} - Loser Bracket",
            ["WinnerBracketMatches"] = "Winner Bracket - Spiele",
            ["LoserBracketMatches"] = "Loser Bracket - Spiele",
            ["NoWinnerBracketGames"] = "Keine Winner Bracket Spiele vorhanden.",
            ["NoLoserBracketGames"] = "Keine Loser Bracket Spiele vorhanden.",
            ["FinalsRound"] = "Finalrunde",

            // Tabellen-Header
            ["PlayerHeader"] = "Spieler",
            ["MatchesPlayedShort"] = "Sp",
            ["WinsShort"] = "S",
            ["DrawsShort"] = "U",
            ["LossesShort"] = "N",
            ["PointsHeader"] = "Pkt",
            ["SetsHeader"] = "Sets",
            ["MatchNumber"] = "Nr",
            ["MatchHeader"] = "Spiel",
            ["StatusHeader"] = "Status",
            ["ResultHeader"] = "Ergebnis",
            ["WinnerHeader"] = "Gewinner",
            ["RoundHeader"] = "Runde",

            // Match Status Texte
            ["ByeStatus"] = "FREILOS",
            ["FinishedStatus"] = "BEENDET",
            ["InProgressStatus"] = "LÄUFT",
            ["PendingStatus"] = "AUSSTEHEND",
            ["ByeGame"] = "{0} (Freilos)",
            ["VersusGame"] = "{0} vs {1}",
            ["Draw"] = "Unentschieden",

            // PRINT DIALOG ÜBERSETZUNGEN
            ["PrintTournamentStatistics"] = "Turnierstatistiken drucken",
            ["TournamentStatisticsIcon"] = "📊 Statistiken",
            ["TournamentClass"] = "🏆 Turnierklasse:",
            ["SelectTournamentClass"] = "Turnierklasse: {0} ({1} Gruppen, {2} Spieler)",
            ["EmptyTournamentClass"] = "❌ {0} (leer)",
            ["ActiveTournamentClass"] = "✅ {0}",
            ["GeneralOptions"] = "⚙️ Allgemeine Optionen",
            ["TournamentOverviewOption"] = "Turnierübersicht",
            ["TitleOptional"] = "📝 Titel (optional):",
            ["SubtitleOptional"] = "📄 Untertitel (optional):",
            ["GroupPhaseSection"] = "👥 Gruppenphase",
            ["IncludeGroupPhase"] = "Gruppenphase einschließen",
            ["SelectGroups"] = "Gruppen auswählen:",
            ["AllGroups"] = "Alle Gruppen",
            ["GroupWithPlayers"] = "{0} ({1} Spieler)",
            ["FinalsSection"] = "🏆 Finalrunde",
            ["IncludeFinals"] = "Finalrunde einschließen",
            ["KnockoutSection"] = "🥊 KO-Phase",
            ["IncludeKnockout"] = "KO-Phase einschließen",
            ["ParticipantsList"] = "Teilnehmer-Liste",
            ["PreviewSection"] = "👁️ Vorschau",
            ["PreviewPlaceholder"] = "📄 Vorschau wird hier angezeigt...",
            ["UpdatePreview"] = "🔄 Vorschau aktualisieren",
            ["PrintButton"] = "🖨️ Drucken",
            ["CancelButton"] = "❌ Abbrechen",
            ["PrintPreviewTitle"] = "Druckvorschau - {0}",
            ["NoContentSelected"] = "Keine Inhalte zum Anzeigen ausgewählt.",
            ["PreviewTitle"] = "Vorschau",
            ["PreviewError"] = "Fehler bei der Vorschau: {0}",
            ["PrintPreparationError"] = "Fehler bei der Druckvorbereitung: {0}",
            ["NoContentToPrint"] = "⚠️ Keine Inhalte zum Drucken ausgewählt",
            ["PreviewError2"] = "❌ Fehler bei der Vorschau: {0}",
            ["PreviewGenerationError"] = "⚠️ Fehler bei der Generierung der Vorschau-Informationen: {0}",
            ["SelectAtLeastOne"] = "Bitte wählen Sie mindestens eine Druckoption aus.",
            ["NoSelection"] = "Keine Auswahl",
            ["NoGroupsAvailable"] = "Die ausgewählte Turnierklasse enthält keine Gruppen zum Drucken.",
            ["NoGroupsAvailableTitle"] = "Keine Gruppen verfügbar",
            ["SelectAtLeastOneGroup"] = "Bitte wählen Sie mindestens eine Gruppe aus.",
            ["InvalidGroupSelection"] = "Die ausgewählten Gruppen sind nicht mehr verfügbar.",
            ["InvalidGroupSelectionTitle"] = "Ungültige Gruppenauswahl",
            ["NoFinalsAvailable"] = "Die ausgewählte Turnierklasse hat keine Finalrunde zum Drucken.",
            ["NoFinalsAvailableTitle"] = "Keine Finalrunde verfügbar",
            ["SelectAtLeastOneKO"] = "Bitte wählen Sie mindestens eine KO-Option aus.",
            ["NoKOOptionSelected"] = "Keine KO-Option ausgewählt",
            ["NoKnockoutAvailable"] = "Die ausgewählte Turnierklasse hat keine KO-Phase zum Drucken.",
            ["NoKnockoutAvailableTitle"] = "Keine KO-Phase verfügbar",

            // Preview-Inhalte
            ["PageOverview"] = "📄 Seite {0}: Turnierübersicht",
            ["OverviewContent1"] = "   • Allgemeine Turnierinformationen",
            ["OverviewContent2"] = "   • Spielregeln und Phasen-Status",
            ["OverviewContent3"] = "   • Gruppen-Übersicht",
            ["PageGroupPhase"] = "📄 Seite {0}: Gruppenphase - {1}",
            ["GroupPlayers"] = "   • {0} Spieler",
            ["GroupMatches"] = "   • {0} Spiele",
            ["GroupContent"] = "   • Tabelle und Ergebnisse",
            ["PageFinals"] = "📄 Seite {0}: Finalrunde",
            ["FinalsContent1"] = "   • Qualifizierte Finalisten",
            ["FinalsContent2"] = "   • Finals-Tabelle",
            ["FinalsContent3"] = "   • Finals-Spiele",
            ["PageWinnerBracket"] = "📄 Seite {0}: Winner Bracket",
            ["WinnerBracketMatches"] = "   • {0} KO-Spiele",
            ["PageLoserBracket"] = "📄 Seite {0}: Loser Bracket",
            ["LoserBracketMatches"] = "   • {0} LB-Spiele",
            ["PageKnockoutParticipants"] = "📄 Seite {0}: KO-Teilnehmer",
            ["KnockoutParticipantsContent"] = "   • {0} qualifizierte Spieler"
        };
    }
}