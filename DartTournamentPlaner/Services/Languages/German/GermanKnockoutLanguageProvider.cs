using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen speziell für das KO-Tab und KO-Phasen-Management
/// Erweitert die bestehenden Turnier-Übersetzungen um spezifische KO-Funktionen
/// </summary>
public class GermanKnockoutLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // =====================================
            // KO-TAB HAUPTBEREICH
            // =====================================
            
            // Tab-Header und Bereiche
            ["KOParticipantsHeader"] = "KO-Teilnehmer:",
            ["KOParticipantsLabel"] = "KO-Teilnehmer",
            ["BracketTreeHeader"] = "Turnierbaum",
            ["WinnerBracketHeader"] = "Winner Bracket:",
            ["LoserBracketHeader"] = "Loser Bracket:",
            ["BracketOverviewHeader"] = "Bracket-Übersicht",
            
            // Sub-Tab Headers innerhalb des KO-Tabs
            ["TournamentTreeTab"] = "🌳 Turnierbaum",
            ["WinnerBracketDataTab"] = "🏆 Winner Bracket",
            ["LoserBracketDataTab"] = "🥈 Loser Bracket",
            ["LoserBracketTreeTab"] = "🌳 Loser Bracket Baum",
            
            // =====================================
            // BRACKET-SPEZIFISCHE BEGRIFFE
            // =====================================
            
            // Bracket-Typen
            ["MainBracket"] = "Hauptbracket",
            ["ConsolationBracket"] = "Trostbracket",
            ["UpperBracket"] = "Oberes Bracket",
            ["LowerBracket"] = "Unteres Bracket",
            ["EliminationBracket"] = "Eliminierungsbracket",
            
            // Bracket-Navigation
            ["BracketView"] = "Bracket-Ansicht",
            ["TreeView"] = "Baum-Ansicht",
            ["DataView"] = "Daten-Ansicht",
            ["SwitchView"] = "Ansicht wechseln",
            ["ExpandBracket"] = "Bracket erweitern",
            ["CollapseBracket"] = "Bracket einklappen",
            
            // =====================================
            // FREILOS-FUNKTIONEN
            // =====================================
            
            // Freilos-Buttons und Aktionen
            ["ByeColumn"] = "Freilos",
            ["GiveByeButton"] = "✓ Bye",
            ["UndoByeButton"] = "✗ Undo", 
            ["GiveByeTooltip"] = "Freilos vergeben",
            ["UndoByeTooltip"] = "Freilos rückgängig machen",
            ["ByeManagement"] = "Freilos-Verwaltung",
            
            // Freilos-Status
            ["ByeAwarded"] = "Freilos vergeben",
            ["ByeUndone"] = "Freilos rückgängig",
            ["AutomaticByeAwarded"] = "Automatisches Freilos vergeben",
            ["ManualByeAwarded"] = "Manuelles Freilos vergeben",
            ["ByeNotPossible"] = "Freilos nicht möglich",
            ["ByeAlreadyAwarded"] = "Freilos bereits vergeben",
            
            // Freilos-Bestätigungen
            ["ConfirmGiveBye"] = "Möchten Sie wirklich ein Freilos vergeben?",
            ["ConfirmUndoBye"] = "Möchten Sie das Freilos wirklich rückgängig machen?",
            ["SelectByeWinner"] = "Wählen Sie den Spieler für das Freilos:",
            ["ByeGivenTo"] = "Freilos vergeben an: {0}",
            
            // =====================================
            // KO-MATCH VERWALTUNG
            // =====================================
            
            // Match-Spalten-Header
            ["RoundColumn"] = "Runde",
            ["MatchColumn"] = "Match", 
            ["ResultColumn"] = "Ergebnis",
            ["StatusColumn"] = "Status",
            ["ActionsColumn"] = "Aktionen",
            
            // Match-Status spezifisch für KO
            ["KOPending"] = "Ausstehend",
            ["KOInProgress"] = "Läuft",
            ["KOFinished"] = "Beendet",
            ["KOBye"] = "Freilos",
            ["KOWaitingForPlayers"] = "Wartet auf Spieler",
            ["KOReadyToStart"] = "Bereit zum Start",
            
            // Match-Aktionen
            ["StartKOMatch"] = "KO-Match starten",
            ["EditKOResult"] = "KO-Ergebnis bearbeiten",
            ["CancelKOMatch"] = "KO-Match abbrechen",
            ["ResetKOMatch"] = "KO-Match zurücksetzen",
            
            // =====================================
            // RUNDENBEZEICHNUNGEN
            // =====================================
            
            // Erweiterte Runden-Namen
            ["FirstRound"] = "1. Runde",
            ["SecondRound"] = "2. Runde", 
            ["ThirdRound"] = "3. Runde",
            ["FourthRound"] = "4. Runde",
            ["FifthRound"] = "5. Runde",
            ["SixthRound"] = "6. Runde",
            
            // Spezielle Runden
            ["WildCardRound"] = "Wild Card Runde",
            ["QualifyingRound"] = "Qualifikationsrunde",
            ["PlayoffRound"] = "Playoff-Runde",
            ["ConsolationRound"] = "Trostrunde",
            
            // =====================================
            // KO-PHASE MANAGEMENT
            // =====================================
            
            // Phase-Status
            ["KOPhaseNotCreated"] = "KO-Phase nicht erstellt",
            ["KOPhaseCreated"] = "KO-Phase erstellt",
            ["KOPhaseStarted"] = "KO-Phase gestartet",
            ["KOPhaseFinished"] = "KO-Phase beendet",
            
            // Phase-Aktionen
            ["CreateKOPhase"] = "KO-Phase erstellen",
            ["StartKOPhase"] = "KO-Phase starten",
            ["ResetKOPhase"] = "KO-Phase zurücksetzen",
            ["FinishKOPhase"] = "KO-Phase beenden",
            
            // Phase-Meldungen
            ["KOPhaseCreatedSuccess"] = "KO-Phase wurde erfolgreich erstellt!",
            ["KOPhaseStartedSuccess"] = "KO-Phase wurde erfolgreich gestartet!",
            ["KOPhaseResetSuccess"] = "KO-Phase wurde erfolgreich zurückgesetzt!",
            ["KOPhaseFinishedSuccess"] = "KO-Phase wurde erfolgreich beendet!",
            
            // =====================================
            // SPIELER-QUALIFIKATION
            // =====================================
            
            // Qualifikations-Status
            ["QualifiedForKO"] = "Qualifiziert für KO",
            ["NotQualifiedForKO"] = "Nicht qualifiziert für KO",
            ["QualificationPending"] = "Qualifikation ausstehend",
            
            // Qualifikations-Wege
            ["QualifiedAsGroupWinner"] = "Qualifiziert als Gruppensieger",
            ["QualifiedAsRunnerUp"] = "Qualifiziert als Zweitplatzierter",
            ["QualifiedAsBestThird"] = "Qualifiziert als bester Drittplatzierter",
            ["QualifiedFromPreviousRound"] = "Qualifiziert aus vorheriger Runde",
            
            // =====================================
            // BRACKET-GENERATION
            // =====================================
            
            // Generation-Aktionen
            ["GenerateBracket"] = "Bracket generieren",
            ["RegenerateBracket"] = "Bracket neu generieren",
            ["ValidateBracket"] = "Bracket validieren",
            ["OptimizeBracket"] = "Bracket optimieren",
            
            // Generation-Status
            ["BracketGenerated"] = "Bracket wurde generiert",
            ["BracketValidated"] = "Bracket wurde validiert",
            ["BracketOptimized"] = "Bracket wurde optimiert",
            ["BracketGenerationFailed"] = "Bracket-Generierung fehlgeschlagen",
            
            // =====================================
            // SEEDING UND PAARUNGEN
            // =====================================
            
            // Seeding-Methoden
            ["SeedingMethod"] = "Setzungsverfahren",
            ["RandomSeeding"] = "Zufällige Setzung",
            ["RankedSeeding"] = "Gerankte Setzung",
            ["ManualSeeding"] = "Manuelle Setzung",
            ["GroupPositionSeeding"] = "Setzung nach Gruppenplatz",
            
            // Setzungsplätze
            ["Seed1"] = "Setzplatz 1",
            ["Seed2"] = "Setzplatz 2", 
            ["Seed3"] = "Setzplatz 3",
            ["Seed4"] = "Setzplatz 4",
            ["TopSeed"] = "Top-Gesetzter",
            ["BottomSeed"] = "Niedriger Gesetzter",
            
            // =====================================
            // DOPPEL-ELIMINIERUNG SPEZIFISCH
            // =====================================
            
            // Double Elimination Begriffe
            ["DoubleEliminationMode"] = "Doppel-Eliminierung",
            ["SingleEliminationMode"] = "Einfach-Eliminierung",
            ["WinnerAdvances"] = "Sieger steigt auf",
            ["LoserEliminated"] = "Verlierer ausgeschieden",
            ["LoserToLoserBracket"] = "Verlierer ins Loser Bracket",
            
            // Grand Final spezifisch
            ["GrandFinalBracketReset"] = "Grand Final Bracket Reset",
            ["GrandFinalAdvantage"] = "Grand Final Vorteil",
            ["WinnerBracketAdvantage"] = "Winner Bracket Vorteil",
            ["MustWinTwice"] = "Muss zweimal gewinnen",
            
            // =====================================
            // ERROR HANDLING UND VALIDIERUNG
            // =====================================
            
            // Fehler-Meldungen
            ["ErrorInvalidBracket"] = "Ungültiges Bracket",
            ["ErrorInsufficientPlayers"] = "Ungenügend Spieler für KO-Phase",
            ["ErrorBracketNotGenerated"] = "Bracket nicht generiert",
            ["ErrorMatchNotFound"] = "Match nicht gefunden",
            ["ErrorPlayerNotFound"] = "Spieler nicht gefunden",
            
            // Validierungs-Meldungen
            ["ValidationBracketOK"] = "Bracket ist gültig",
            ["ValidationBracketError"] = "Bracket-Validierung fehlgeschlagen",
            ["ValidationPlayerMismatch"] = "Spieler-Unstimmigkeit",
            ["ValidationRoundMismatch"] = "Runden-Unstimmigkeit",
            
            // =====================================
            // EXPORT UND ANZEIGE
            // =====================================
            
            // Export-Funktionen
            ["ExportKOBracket"] = "KO-Bracket exportieren",
            ["PrintKOBracket"] = "KO-Bracket drucken",
            ["SaveBracketImage"] = "Bracket als Bild speichern",
            ["CopyBracketToClipboard"] = "Bracket in Zwischenablage kopieren",
            
            // Anzeige-Optionen
            ["ShowPlayerNames"] = "Spielernamen anzeigen",
            ["ShowMatchTimes"] = "Match-Zeiten anzeigen",
            ["ShowRoundNames"] = "Rundennamen anzeigen",
            ["ShowBracketConnections"] = "Bracket-Verbindungen anzeigen",
            ["CompactView"] = "Kompakte Ansicht",
            ["DetailedView"] = "Detaillierte Ansicht",
            
            // =====================================
            // TOOLTIPS UND HILFE
            // =====================================
            
            // Hilfe-Texte
            ["KOTabHelp"] = "KO-Tab Hilfe",
            ["BracketGenerationHelp"] = "Bracket-Generierung Hilfe",
            ["ByeManagementHelp"] = "Freilos-Verwaltung Hilfe",
            ["DoubleEliminationHelp"] = "Doppel-Eliminierung Hilfe",
            
            // Tooltips
            ["KOTabTooltip"] = "Verwaltung der K.-o.-Phase mit Bracket-Generation und Match-Verwaltung",
            ["BracketTreeTooltip"] = "Grafische Darstellung des Turnierbaums",
            ["WinnerBracketTooltip"] = "Hauptbracket - Verlierer werden eliminiert oder ins Loser Bracket verschoben",
            ["LoserBracketTooltip"] = "Trostbracket - Zweite Chance für Verlierer aus dem Winner Bracket",
            ["ByeButtonTooltip"] = "Vergeben Sie ein Freilos, wenn ein Spieler nicht verfügbar ist",
            ["UndoByeButtonTooltip"] = "Machen Sie ein vergebenes Freilos rückgängig",
            
            // =====================================
            // ZUSTAND UND STATUS
            // =====================================
            
            // Zustandsmeldungen
            ["BracketEmpty"] = "Bracket ist leer",
            ["BracketComplete"] = "Bracket vollständig",
            ["BracketInProgress"] = "Bracket läuft",
            ["AllMatchesComplete"] = "Alle Matches abgeschlossen",
            ["MatchesPending"] = "Matches ausstehend",
            
            // Fortschritts-Informationen
            ["MatchesCompleted"] = "Abgeschlossene Matches: {0}/{1}",
            ["RoundsCompleted"] = "Abgeschlossene Runden: {0}/{1}",
            ["PlayersRemaining"] = "Verbleibende Spieler: {0}",
            ["NextMatch"] = "Nächstes Match: {0}",
            
            // =====================================
            // SPEZIELLE AKTIONEN
            // =====================================
            
            // Spezial-Aktionen
            ["SimulateMatch"] = "Match simulieren",
            ["AutoAdvanceWinner"] = "Sieger automatisch weiterleiten",
            ["ManualPlayerAdvancement"] = "Manuelle Spieler-Weiterleitung",
            ["BulkMatchUpdate"] = "Massen-Match-Update",
            ["QuickMatchEntry"] = "Schnelle Match-Eingabe",
            
            // Konfigurations-Optionen
            ["AutoAdvanceEnabled"] = "Auto-Weiterleitung aktiviert", 
            ["AutoAdvanceDisabled"] = "Auto-Weiterleitung deaktiviert",
            ["ManualAdvanceMode"] = "Manueller Weiterleitungs-Modus",
            ["AutomaticByeHandling"] = "Automatische Freilos-Behandlung",
            ["ManualByeHandling"] = "Manuelle Freilos-Behandlung"
        };
    }
}