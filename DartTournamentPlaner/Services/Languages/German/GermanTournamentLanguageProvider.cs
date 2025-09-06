using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für Turnier-Management und Spiele
/// </summary>
public class GermanTournamentLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Turnier-Tab Übersetzungen
            ["SetupTab"] = "Turnier-Setup",
            ["GroupPhaseTab"] = "Gruppenphase",
            ["FinalsTab"] = "Finalrunde",
            ["KnockoutTab"] = "KO-Runde",
            ["Groups"] = "Gruppen:",
            ["Players"] = "Spieler:",
            ["AddGroup"] = "Gruppe hinzufügen",
            ["RemoveGroup"] = "Gruppe entfernen",
            ["AddPlayer"] = "Spieler hinzufügen",
            ["RemovePlayer"] = "Spieler entfernen",
            ["NewGroup"] = "Neue Gruppe",
            ["GroupName"] = "Geben Sie den Namen der neuen Gruppe ein:",
            ["RemoveGroupConfirm"] = "Möchten Sie die Gruppe '{0}' wirklich entfernen?\nAlle Spieler in dieser Gruppe werden ebenfalls entfernt.",
            ["RemoveGroupTitle"] = "Gruppe entfernen",
            ["RemovePlayerConfirm"] = "Möchten Sie den Spieler '{0}' wirklich entfernen?",
            ["RemovePlayerTitle"] = "Spieler entfernen",
            ["NoGroupSelected"] = "Bitte wählen Sie eine Gruppe aus, die entfernt werden soll.",
            ["NoGroupSelectedTitle"] = "Keine Gruppe ausgewählt",
            ["NoPlayerSelected"] = "Bitte wählen Sie einen Spieler aus, der entfernt werden soll.",
            ["NoPlayerSelectedTitle"] = "Kein Spieler ausgewählt",
            ["SelectGroupFirst"] = "Bitte wählen Sie zuerst eine Gruppe aus.",
            ["EnterPlayerName"] = "Bitte geben Sie einen Spielernamen ein.",
            ["NoNameEntered"] = "Kein Name eingegeben",
            ["PlayersInGroup"] = "Spieler in {0}:",
            ["NoGroupSelectedPlayers"] = "Spieler: (Keine Gruppe ausgewählt)",
            ["Group"] = "Gruppe {0}",
            ["AdvanceToNextPhase"] = "Nächste Phase starten",
            ["ResetTournament"] = "Turnier zurücksetzen",
            ["ResetKnockoutPhase"] = "KO-Phase zurücksetzen",
            ["ResetFinalsPhase"] = "Finalrunde zurücksetzen",
            ["RefreshUI"] = "UI aktualisieren",
            ["RefreshUITooltip"] = "Aktualisiert die Benutzeroberfläche",

            // Turnierprozessphasen
            ["GroupPhase"] = "Gruppenphase",
            ["FinalsPhase"] = "Finalrunde",
            ["KnockoutPhase"] = "KO-Phase",

            // Spielregeln
            ["GameRules"] = "Spielregeln",
            ["GameMode"] = "Spielmodus",
            ["Points501"] = "501 Punkte",
            ["Points401"] = "401 Punkte",
            ["Points301"] = "301 Punkte",
            ["FinishMode"] = "Finish-Modus",
            ["SingleOut"] = "Single Out",
            ["DoubleOut"] = "Double Out",
            ["LegsToWin"] = "Legs zum Sieg",
            ["PlayWithSets"] = "Mit Sets spielen",
            ["SetsToWin"] = "Sets zum Sieg",
            ["LegsPerSet"] = "Legs pro Set",
            ["ConfigureRules"] = "Regeln konfigurieren",
            ["RulesPreview"] = "Regelvorschau",
            ["AfterGroupPhaseHeader"] = "Nach der Gruppenphase",

            // Einstellungen nach der Gruppenphase
            ["PostGroupPhase"] = "Nach der Gruppenphase",
            ["PostGroupPhaseMode"] = "Modus nach Gruppenphase",
            ["PostGroupPhaseNone"] = "Nur Gruppenphase",
            ["PostGroupPhaseRoundRobin"] = "Finalrunde (Round Robin)",
            ["PostGroupPhaseKnockout"] = "KO-System",
            ["QualifyingPlayersPerGroup"] = "Qualifizierte pro Gruppe",
            ["KnockoutMode"] = "KO-Modus",
            ["SingleElimination"] = "Einfaches KO",
            ["DoubleElimination"] = "Doppeltes KO (Winner + Loser Bracket)",
            ["IncludeGroupPhaseLosersBracket"] = "Gruppenphase-Verlierer ins Loser Bracket",

            // Rundenspezifische Regeln
            ["RoundSpecificRules"] = "Rundenspezifische Regeln",
            ["ConfigureRoundRules"] = "Rundenregeln konfigurieren",
            ["WinnerBracketRules"] = "Winner Bracket Regeln",
            ["LoserBracketRules"] = "Loser Bracket Regeln",
            ["RoundRulesFor"] = "Regeln für {0}",
            ["DefaultRules"] = "Standard-Regeln",
            ["ResetToDefault"] = "Auf Standard zurücksetzen",
            ["RoundRulesConfiguration"] = "Rundenregeln-Konfiguration",
            ["Best64Rules"] = "Beste 64 Regeln",
            ["Best32Rules"] = "Beste 32 Regeln",
            ["Best16Rules"] = "Beste 16 Regeln",
            ["QuarterfinalRules"] = "Viertelfinale Regeln",
            ["SemifinalRules"] = "Halbfinale Regeln",
            ["FinalRules"] = "Finale Regeln",
            ["GrandFinalRules"] = "Grand Final Regeln",

            // Individual round names for GetRoundDisplayName
            ["Best64"] = "Beste 64",
            ["Best32"] = "Beste 32",
            ["Best16"] = "Beste 16",
            ["Quarterfinal"] = "Viertelfinale",
            ["Semifinal"] = "Halbfinale",
            ["Final"] = "Finale",
            ["GrandFinal"] = "Grand Final",
            ["LoserBracket"] = "Loser Bracket",

            // Spiele und Match-Management
            ["Matches"] = "Spiele:",
            ["Standings"] = "Tabelle:",
            ["Match"] = "Match", // ✅ NEU: Für DataGrid-Header
            ["Result"] = "Ergebnis", // ✅ NEU: Für DataGrid-Header
            ["Status"] = "Status", // ✅ NEU: Für DataGrid-Header
            ["Position"] = "Pos", // ✅ NEU: Für DataGrid-Header
            ["Player"] = "Spieler", // ✅ NEU: Für DataGrid-Header
            ["Score"] = "Pkt", // ✅ NEU: Für DataGrid-Header
            ["Sets"] = "Sets", // ✅ NEU: Für DataGrid-Header
            ["Legs"] = "Legs", // ✅ NEU: Für DataGrid-Header
            ["GenerateMatches"] = "Spiele generieren",
            ["MatchesGenerated"] = "Spiele wurden erfolgreich generiert!",
            ["ResetMatches"] = "Spiele zurücksetzen",
            ["ResetMatchesConfirm"] = "Möchten Sie alle Spiele für Gruppe '{0}' wirklich zurücksetzen?\nAlle Ergebnisse gehen verloren!",
            ["ResetMatchesTitle"] = "Spiele zurücksetzen",
            ["MatchesReset"] = "Spiele wurden zurückgesetzt!",
            ["EnterResult"] = "Ergebnis eingeben",
            ["MatchNotStarted"] = "Nicht gestartet",
            ["MatchInProgress"] = "Läuft",
            ["MatchFinished"] = "Beendet",
            ["MatchBye"] = "Freilos",
            ["Round"] = "Runde",
            ["Sets"] = "Sets",
            ["Legs"] = "Legs",
            ["Score"] = "Punktestand",
            ["SubmitResult"] = "Ergebnis bestätigen",
            ["ResultSubmitted"] = "Ergebnis erfolgreich übermittelt!",
            ["Player1"] = "Spieler 1",
            ["Player2"] = "Spieler 2",
            ["Loser"] = "Verlierer",
            ["MatchCancelled"] = "Spiel wurde abgebrochen",
            ["CancelMatch"] = "Spiel abbrechen",
            ["MatchCancelledConfirm"] = "Möchten Sie das Spiel wirklich abbrechen?",
            ["MatchCancelledTitle"] = "Spiel abbrechen",
            ["NotImplemented"] = "Nicht implementiert",
            ["FeatureComingSoon"] = "Diese Funktion wird bald verfügbar sein.",

            // Nachrichten für Turnier-Tab
            ["MinimumTwoPlayers"] = "Mindestens 2 Spieler erforderlich.",
            ["ErrorGeneratingMatches"] = "Fehler beim Erstellen der Spiele:",
            ["MatchesGeneratedSuccess"] = "Spiele wurden erfolgreich erstellt!",
            ["MatchesResetSuccess"] = "Spiele wurden zurückgesetzt!",
            ["ResetTournamentConfirm"] = "Möchten Sie wirklich das gesamte Turnier zurücksetzen?\n\n⚠️ ALLE Spiele und Phasen werden gelöscht!\nNur Gruppen und Spieler bleiben erhalten.",
            ["TournamentResetComplete"] = "Das Turnier wurde erfolgreich zurückgesetzt.",
            ["ResetKnockoutConfirm"] = "Möchten Sie wirklich die K.-o.-Phase zurücksetzen?\n\n⚠️ Alle K.-o.-Spiele und der Turnierbaum werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.",
            ["ResetKnockoutComplete"] = "Die K.-o.-Phase wurde erfolgreich zurückgesetzt.",
            ["ResetFinalsConfirm"] = "Möchten Sie wirklich die Finalrunde zurücksetzen?\n\n⚠️ Alle Finalspiele werden gelöscht!\nDas Turnier wird auf die Gruppenphase zurückgesetzt.",
            ["ResetFinalsComplete"] = "Die Finalrunde wurde erfolgreich zurückgesetzt.",
            ["ErrorResettingTournament"] = "Fehler beim Zurücksetzen des Turniers:",
            ["CannotAdvancePhase"] = "Alle Spiele der aktuellen Phase müssen abgeschlossen sein",
            ["ErrorAdvancingPhase"] = "Fehler beim Wechsel in die nächste Phase:",
            ["UIRefreshed"] = "Benutzeroberfläche wurde aktualisiert",
            ["ErrorRefreshing"] = "Fehler beim Aktualisieren:",
            ["KOPhaseActiveMSB"] = "K.-o.-Phase ist nicht aktiv",
            ["KOPhaseNotEnoughUserMSB"] = "Nicht genügend Teilnehmer für die K.-o.-Phase (mindestens 2 erforderlich)",

            // Meldungstitel
            ["KOPhaseUsrWarnTitel"] = "K.-o.-Phasen-Warnung",

            // Tab-Kopfzeilen für Spieleransicht
            ["FinalistsCount"] = "Finalisten ({0} Spieler):",
            ["KnockoutParticipantsCount"] = "K.-o.-Teilnehmer ({0} Spieler):",

            // Weitere Phasen-Texte
            ["NextPhaseStart"] = "Start {0}",

            // Match-Ergebnisfenster
            ["EnterMatchResult"] = "Match-Ergebnis eingeben",
            ["SaveResult"] = "Ergebnis speichern",
            ["Notes"] = "Notizen",
            ["InvalidNumbers"] = "Ungültige Zahlen",
            ["NegativeValues"] = "Negative Werte sind nicht erlaubt",
            ["InvalidSetCount"] = "Ungültige Satzanzahl. Maximum: {0}, Gesamt: {1}",
            ["BothPlayersWon"] = "Beide Spieler können nicht gleichzeitig gewinnen",
            ["MatchIncomplete"] = "Das Spiel ist noch nicht abgeschlossen",
            ["InsufficientLegsForSet"] = "{0} hat nicht genügend Legs für die gewonnenen Sätze. Minimum: {1}",
            ["ExcessiveLegs"] = "Zu viele Legs für die Satzkombination {0}:{1}. Maximum: {2}",
            ["LegsExceedSetRequirement"] = "{0} hat mehr Legs als für die Sätze erforderlich",
            ["InvalidLegCount"] = "Ungültige Leg-Anzahl. Maximum: {0}, Gesamt: {1}",
            ["SaveBlocked"] = "Speichern blockiert",
            ["GiveBye"] = "Freilos vergeben",
            ["SelectByeWinner"] = "Wählen Sie den Spieler, der das Freilos erhalten soll:",
            ["NoWinnerFound"] = "Kein Gewinner gefunden",

            // Kontext-Menü spezifische Übersetzungen
            ["EditResult"] = "Ergebnis bearbeiten",
            ["AutomaticBye"] = "Automatisches Freilos",
            ["UndoByeShort"] = "Freilos rückgängig machen",
            ["NoActionsAvailable"] = "Keine Aktionen verfügbar",
            ["ByeToPlayer"] = "Freilos an {0}",

            // KO-Tab Header und Navigation
            ["KOTab"] = "KO-Runde",
            ["StatisticsTab"] = "Statistiken",
            ["KOPhaseTab"] = "KO-Phase",
            ["WinnerBracketTab"] = "Winner Bracket",
            ["LoserBracketTab"] = "Loser Bracket",
            ["KOParticipantsTab"] = "KO-Teilnehmer",
            ["BracketOverviewTab"] = "Bracket-Übersicht",
            ["TreeViewTab"] = "Turnierturm",

            // KO-Phase Status und Meldungen
            ["KOPhaseNotActive"] = "KO-Phase ist nicht aktiv",
            ["KOPhaseWaitingForGroupCompletion"] = "Warten auf Abschluss der Gruppenphase",
            ["KOPhaseReady"] = "KO-Phase bereit",
            ["KOPhaseInProgress"] = "KO-Phase läuft",
            ["KOPhaseComplete"] = "KO-Phase abgeschlossen",
            ["GenerateKOBracket"] = "KO-Bracket generieren",
            ["KOBracketGenerated"] = "KO-Bracket wurde erfolgreich generiert!",
            ["ErrorGeneratingKOBracket"] = "Fehler beim Generieren des KO-Brackets:",
            ["NoQualifiedPlayersForKO"] = "Keine qualifizierten Spieler für die KO-Phase gefunden.",
            ["InsufficientPlayersForKO"] = "Nicht genügend Spieler für die KO-Phase (mindestens 2 erforderlich).",

            // KO-Match Status und Aktionen
            ["KOMatchPending"] = "Ausstehend",
            ["KOMatchInProgress"] = "Läuft",
            ["KOMatchFinished"] = "Beendet",
            ["KOMatchBye"] = "Freilos",
            ["NextRound"] = "Nächste Runde",
            ["AdvanceWinner"] = "Sieger weiterleiten",
            ["EliminateLoser"] = "Verlierer eliminieren",
            ["WinnerAdvancesTo"] = "Sieger qualifiziert sich für: {0}",
            ["LoserMovesToLB"] = "Verlierer ins Loser Bracket: {0}",
            ["WaitingForPreviousMatch"] = "Warten auf vorheriges Spiel",

            // KO-Bracket Struktur
            ["Round1"] = "1. Runde",
            ["Round2"] = "2. Runde",
            ["Round3"] = "3. Runde",
            ["Quarterfinals"] = "Viertelfinale",
            ["Semifinals"] = "Halbfinale",
            ["Finals"] = "Finale",
            ["GrandFinals"] = "Grand Final",
            ["ThirdPlacePlayoff"] = "Spiel um Platz 3",
            ["WinnerBracketFinal"] = "Winner Bracket Finale",
            ["LoserBracketFinal"] = "Loser Bracket Finale",

            // KO-Teilnehmer und Seeding
            ["QualifiedFromGroup"] = "Qualifiziert aus Gruppe {0}",
            ["SeedPosition"] = "Setzlistenplatz {0}",
            ["HighestSeed"] = "Höchste Setzung",
            ["LowestSeed"] = "Niedrigste Setzung",
            ["RandomSeeding"] = "Zufällige Setzung",
            ["GroupWinners"] = "Gruppensieger",
            ["GroupRunners"] = "Gruppenzweite",
            ["BestThirds"] = "Beste Drittplatzierte",

            // KO-Bracket Actions und Buttons
            ["ResetKOBracket"] = "KO-Bracket zurücksetzen",
            ["ResetKOBracketConfirm"] = "Möchten Sie das KO-Bracket wirklich zurücksetzen?\n\n⚠️ Alle KO-Spiele werden gelöscht!\nDie qualifizierten Spieler bleiben erhalten.",
            ["KOBracketReset"] = "KO-Bracket wurde zurückgesetzt!",
            ["ExpandAllMatches"] = "Alle Spiele aufklappen",
            ["CollapseAllMatches"] = "Alle Spiele einklappen",
            ["ShowBracketTree"] = "Bracket-Baum anzeigen",
            ["ShowMatchList"] = "Spiele-Liste anzeigen",
            ["ExportBracket"] = "Bracket exportieren",
            ["PrintBracket"] = "Bracket drucken",

            // Double Elimination spezifische Begriffe
            ["WinnerBracket"] = "Winner Bracket",
            ["LoserBracket"] = "Loser Bracket",
            ["WinnerBracketMatches"] = "Winner Bracket Spiele",
            ["LoserBracketMatches"] = "Loser Bracket Spiele",
            ["UpperBracket"] = "Oberes Bracket",
            ["LowerBracket"] = "Unteres Bracket",
            ["ConsolationBracket"] = "Trostbracket",
            ["EliminationMatch"] = "Eliminierungsspiel",
            ["ConsolationMatch"] = "Trostspiel",

            // KO-Match Informationen
            ["MatchDuration"] = "Spieldauer: {0}",
            ["MatchStarted"] = "Spiel gestartet: {0}",
            ["MatchFinished"] = "Spiel beendet: {0}",
            ["ElapsedTime"] = "Vergangene Zeit: {0}",
            ["EstimatedDuration"] = "Geschätzte Dauer: {0}",
            ["QualificationPath"] = "Qualifikationsweg: {0}"
        };
    }
}