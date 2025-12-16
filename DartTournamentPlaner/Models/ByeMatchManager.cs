using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Verwaltet alle Freilos-bezogenen Funktionen für K.O.-Turniere
/// Verantwortlich für automatische und manuelle Freilos-Vergabe
/// </summary>
public class ByeMatchManager
{
    private readonly TournamentClass _tournament;

    public ByeMatchManager(TournamentClass tournament)
    {
        _tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
    }

    /// <summary>
    /// HAUPTMETHODE: Propagiert initiale Bye-Matches und aktiviert automatische Freilos-Erkennung
    /// </summary>
    public void PropagateInitialByeMatches(ObservableCollection<KnockoutMatch> bracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        System.Diagnostics.Debug.WriteLine($"=== PropagateInitialByeMatches START ===");
        
        var byeMatches = bracket.Where(m => m.Status == MatchStatus.Bye && m.Winner != null).ToList();
        System.Diagnostics.Debug.WriteLine($"  Found {byeMatches.Count} initial bye matches to propagate");

        foreach (var byeMatch in byeMatches)
        {
            System.Diagnostics.Debug.WriteLine($"  Propagating initial bye match {byeMatch.Id} (winner: {byeMatch.Winner?.Name})");
            PropagateMatchResultWithAutomaticByes(byeMatch, bracket, otherBracket);
        }

        System.Diagnostics.Debug.WriteLine($"=== PropagateInitialByeMatches END ===");
    }

    /// <summary>
    /// KERNMETHODE: Propagiert Match-Ergebnisse und überprüft automatisch auf neue Freilose
    /// </summary>
    public void PropagateMatchResultWithAutomaticByes(KnockoutMatch completedMatch, ObservableCollection<KnockoutMatch> sameBracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        if (completedMatch.Winner == null) return;

        System.Diagnostics.Debug.WriteLine($"    Propagating match {completedMatch.Id} (winner: {completedMatch.Winner.Name})");

        // 1. Propagiere Gewinner in das gleiche Bracket
        var sameBracketDependents = sameBracket.Where(m => 
            (m.SourceMatch1?.Id == completedMatch.Id && m.Player1FromWinner) ||
            (m.SourceMatch2?.Id == completedMatch.Id && m.Player2FromWinner)).ToList();

        foreach (var dependent in sameBracketDependents)
        {
            if (dependent.SourceMatch1?.Id == completedMatch.Id && dependent.Player1FromWinner)
            {
                dependent.Player1 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player1 in match {dependent.Id}");
            }
            
            if (dependent.SourceMatch2?.Id == completedMatch.Id && dependent.Player2FromWinner)
            {
                dependent.Player2 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player2 in match {dependent.Id}");
            }
        }

        // 2. Propagiere Verlierer ins andere Bracket (Winner → Loser)
        System.Diagnostics.Debug.WriteLine($"      DEBUG-PWAB: otherBracket != null? {otherBracket != null}");
        System.Diagnostics.Debug.WriteLine($"      DEBUG-PWAB: completedMatch.BracketType = {completedMatch.BracketType}");
        System.Diagnostics.Debug.WriteLine($"      DEBUG-PWAB: completedMatch.Loser = {completedMatch.Loser?.Name ?? "null"}");
        
        if (otherBracket != null && completedMatch.BracketType == BracketType.Winner)
        {
            // ✅ WICHTIG: Verwende match.Loser wenn es gesetzt ist (z.B. bei Byes), sonst GetMatchLoser
            var loser = completedMatch.Loser ?? GetMatchLoser(completedMatch);
            
            System.Diagnostics.Debug.WriteLine($"      DEBUG-PWAB: loser (after fallback) = {loser?.Name ?? "null"}");
            
            if (loser != null)
            {
                System.Diagnostics.Debug.WriteLine($"      WB Match {completedMatch.Id}: Loser {loser.Name} goes to LB");
                
                var crossBracketDependents = otherBracket.Where(m => 
                    (m.SourceMatch1?.Id == completedMatch.Id && !m.Player1FromWinner) ||
                    (m.SourceMatch2?.Id == completedMatch.Id && !m.Player2FromWinner)).ToList();

                System.Diagnostics.Debug.WriteLine($"      DEBUG-PWAB: Found {crossBracketDependents.Count} cross-bracket dependents");

                foreach (var dependent in crossBracketDependents)
                {
                    if (dependent.SourceMatch1?.Id == completedMatch.Id && !dependent.Player1FromWinner)
                    {
                        dependent.Player1 = loser;
                        System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player1 in LB match {dependent.Id}");
                    }
                    
                    if (dependent.SourceMatch2?.Id == completedMatch.Id && !dependent.Player2FromWinner)
                    {
                        dependent.Player2 = loser;
                        System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player2 in LB match {dependent.Id}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"      ⚠️ PWAB: No loser found for match {completedMatch.Id}!");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"      ⚠️ PWAB: Skipping loser propagation - condition failed!");
        }

        // 3. KRITISCH: Prüfe nach der Propagation auf neue automatische Freilose
        CheckAndHandleAutomaticByes(sameBracket, otherBracket);
        if (otherBracket != null)
        {
            CheckAndHandleAutomaticByes(otherBracket, sameBracket);
        }
    }

    /// <summary>
    /// Direktes Propagieren von Spielergewinnen-Nachrichten ohne Nachverfolgung von Freilosen
    /// ✅ ERWEITERT: Propagiert auch Verlierer ins Loser Bracket (wichtig für Bye-Matches mit echtem Gegner)
    /// </summary>
    public void PropagateMatchResultDirectly(KnockoutMatch completedMatch, ObservableCollection<KnockoutMatch> sameBracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        if (completedMatch.Winner == null) return;

        System.Diagnostics.Debug.WriteLine($"    Directly propagating match {completedMatch.Id} (winner: {completedMatch.Winner.Name})");
        
        // ✅ WICHTIG: Bei Bye-Matches mit einem echten Gegner, bestimme den Verlierer!
        Player? loser = null;
        if (completedMatch.Status == MatchStatus.Bye)
        {
            // Bei Bye: Der Gegner des Gewinners ist der Verlierer (wenn vorhanden)
            if (completedMatch.Player1 == completedMatch.Winner && completedMatch.Player2 != null)
            {
                loser = completedMatch.Player2;
                completedMatch.Loser = loser;
                System.Diagnostics.Debug.WriteLine($"      Bye match loser determined: {loser.Name} (was Player2)");
            }
            else if (completedMatch.Player2 == completedMatch.Winner && completedMatch.Player1 != null)
            {
                loser = completedMatch.Player1;
                completedMatch.Loser = loser;
                System.Diagnostics.Debug.WriteLine($"      Bye match loser determined: {loser.Name} (was Player1)");
            }
        }
        else
        {
            // Bei normalen Matches, hole den Verlierer
            loser = GetMatchLoser(completedMatch);
        }

        // 1. Propagiere Gewinner in das gleiche Bracket
        var sameBracketDependents = sameBracket.Where(m => 
            (m.SourceMatch1?.Id == completedMatch.Id && m.Player1FromWinner) ||
            (m.SourceMatch2?.Id == completedMatch.Id && m.Player2FromWinner)).ToList();

        foreach (var dependent in sameBracketDependents)
        {
            if (dependent.SourceMatch1?.Id == completedMatch.Id && dependent.Player1FromWinner)
            {
                dependent.Player1 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player1 in match {dependent.Id}");
            }
            
            if (dependent.SourceMatch2?.Id == completedMatch.Id && dependent.Player2FromWinner)
            {
                dependent.Player2 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player2 in match {dependent.Id}");
            }
        }

        // 2. ✅ NEU: Propagiere Verlierer ins andere Bracket (Winner → Loser) AUCH BEI BYE!
        System.Diagnostics.Debug.WriteLine($"      DEBUG: otherBracket != null? {otherBracket != null}");
        System.Diagnostics.Debug.WriteLine($"      DEBUG: completedMatch.BracketType = {completedMatch.BracketType}");
        System.Diagnostics.Debug.WriteLine($"      DEBUG: loser != null? {loser != null}");
        
        if (otherBracket != null && completedMatch.BracketType == BracketType.Winner && loser != null)
        {
            System.Diagnostics.Debug.WriteLine($"      WB Match {completedMatch.Id}: Loser {loser.Name} goes to LB (from Bye or normal match)");
            
            var crossBracketDependents = otherBracket.Where(m => 
                (m.SourceMatch1?.Id == completedMatch.Id && !m.Player1FromWinner) ||
                (m.SourceMatch2?.Id == completedMatch.Id && !m.Player2FromWinner)).ToList();

            System.Diagnostics.Debug.WriteLine($"      DEBUG: Found {crossBracketDependents.Count} cross-bracket dependents");

            foreach (var dependent in crossBracketDependents)
            {
                if (dependent.SourceMatch1?.Id == completedMatch.Id && !dependent.Player1FromWinner)
                {
                    dependent.Player1 = loser;
                    System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player1 in LB match {dependent.Id}");
                }
                
                if (dependent.SourceMatch2?.Id == completedMatch.Id && !dependent.Player2FromWinner)
                {
                    dependent.Player2 = loser;
                    System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player2 in LB match {dependent.Id}");
                }
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"      ⚠️ SKIPPING loser propagation - condition failed!");
            System.Diagnostics.Debug.WriteLine($"         otherBracket: {otherBracket != null}, BracketType: {completedMatch.BracketType}, loser: {loser?.Name ?? "null"}");
        }
    }

    /// <summary>
    /// VERBESSERTE LÖSUNG: Überprüft ALLE Arten von automatischen Freilosen
    /// WICHTIG: Nur für Freilos-Szenarien, nicht für normale Match-Ergebnisse
    /// </summary>
    public void CheckAndHandleAutomaticByes(ObservableCollection<KnockoutMatch> bracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        System.Diagnostics.Debug.WriteLine($"      === ENHANCED: CheckAndHandleAutomaticByes START ===");
        
        bool foundNewByes;
        int iterations = 0;
        const int maxIterations = 15; // Erhöht für komplexe Szenarien

        do
        {
            foundNewByes = false;
            iterations++;
            
            System.Diagnostics.Debug.WriteLine($"        Iteration {iterations}: Checking for automatic byes...");

            // FALL 1: Neue Freilose durch Spieler-Hinzufügung (aber nicht bei bereits beendeten Matches)
            var newByeMatches = bracket.Where(m => 
                m.Status == MatchStatus.NotStarted && 
                ShouldBeAutomaticBye(m))
                .ToList();

            // FALL 2: Ursprünglich Bye-markierte Matches mit neuen Spielern (aber keine finished matches)
            var convertedMatches = bracket.Where(m => 
                m.Status == MatchStatus.Bye && 
                m.Winner == null && 
                ShouldConvertByeToAutomatic(m))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"        Found {newByeMatches.Count} new byes, {convertedMatches.Count} converted byes");

            // Behandle neue automatische Freilose
            foreach (var match in newByeMatches)
            {
                var automaticWinner = DetermineAutomaticWinner(match);
                
                if (automaticWinner != null)
                {
                    System.Diagnostics.Debug.WriteLine($"        NEW AUTO BYE: Match {match.Id} - {automaticWinner.Name} advances automatically");
    
                    match.Status = MatchStatus.Bye;
                    match.Winner = automaticWinner;
                    foundNewByes = true;
                    
                    // Verwende direkte Propagation ohne weitere Bye-Checks
                    PropagateMatchResultDirectly(match, bracket, otherBracket);
                }
            }

            // Behandle konvertierte Bye-Matches
            foreach (var match in convertedMatches)
            {
                var automaticWinner = DetermineAutomaticWinner(match);
                
                if (automaticWinner != null)
                {
                    System.Diagnostics.Debug.WriteLine($"        CONVERTED BYE: Match {match.Id} - {automaticWinner.Name} now gets automatic bye");
                    
                    match.Winner = automaticWinner; // Status bleibt Bye
                    foundNewByes = true;
                    
                    // Verwende direkte Propagation ohne weitere Bye-Checks
                    PropagateMatchResultDirectly(match, bracket, otherBracket);
                }
            }
            
        } while (foundNewByes && iterations < maxIterations);

        if (iterations >= maxIterations)
        {
            System.Diagnostics.Debug.WriteLine($"        WARNING: Reached maximum iterations ({maxIterations}) in enhanced bye check");
        }
        
        System.Diagnostics.Debug.WriteLine($"      === ENHANCED: CheckAndHandleAutomaticByes END ===");
    }

    /// <summary>
    /// NEU: Bestimmt ob ein Match automatisch als Bye behandelt werden sollte
    /// WICHTIG: Nur für NotStarted Matches, nicht für bereits fertige Matches
    /// </summary>
    private bool ShouldBeAutomaticBye(KnockoutMatch match)
    {
        // SICHERHEITSCHECK: Keine Automatic-Byes für bereits beendete oder laufende Matches
        if (match.Status != MatchStatus.NotStarted)
        {
            System.Diagnostics.Debug.WriteLine($"        ShouldBeAutomaticBye: Match {match.Id} not NotStarted, skipping");
            return false;
        }

        // Hilfsfunktion: Prüft ob ein Player gültig ist (nicht null und nicht "TBD")
        bool IsValidPlayer(Player? player)
        {
            return player != null && 
                   !string.IsNullOrEmpty(player.Name) && 
                   !player.Name.Equals("TBD", StringComparison.OrdinalIgnoreCase) &&
                   !player.Name.Equals("To Be Determined", StringComparison.OrdinalIgnoreCase);
        }

        System.Diagnostics.Debug.WriteLine($"        ShouldBeAutomaticBye: Match {match.Id} - Player1: {match.Player1?.Name ?? "null"}, Player2: {match.Player2?.Name ?? "null"}");

        // Fall 1: Ein gültiger Spieler vorhanden, kein zweiter erwartet
        if (IsValidPlayer(match.Player1) && match.Player2 == null && match.SourceMatch2 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player1, no Player2 expected");
            return true;
        }
            
        // Fall 2: Ein gültiger Spieler vorhanden, kein erster erwartet  
        if (match.Player1 == null && IsValidPlayer(match.Player2) && match.SourceMatch1 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player2, no Player1 expected");
            return true;
        }

        // Fall 3: Ein gültiger Spieler + ein TBD Spieler, aber keine wartenden Matches
        if (IsValidPlayer(match.Player1) && match.Player2 != null && !IsValidPlayer(match.Player2) && match.SourceMatch2 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player1, Player2 is TBD, no SourceMatch2");
            return true;
        }

        if (match.Player1 != null && !IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2) && match.SourceMatch1 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player2, Player1 is TBD, no SourceMatch1");
            return true;
        }
            
        System.Diagnostics.Debug.WriteLine($"          -> Should NOT be bye");
        return false;
    }

    /// <summary>
    /// NEU: Bestimmt ob ein ursprüngliches Bye-Match zu einem automatischen Bye konvertiert werden sollte
    /// </summary>
    private bool ShouldConvertByeToAutomatic(KnockoutMatch match)
    {
        // Bereits als Bye markiert, aber ohne Gewinner und jetzt mit Spieler(n)
        return (match.Player1 != null || match.Player2 != null);
    }

    /// <summary>
    /// VERBESSERT: Bestimmt den automatischen Gewinner eines Matches (filtert TBD aus)
    /// </summary>
    private Player? DetermineAutomaticWinner(KnockoutMatch match)
    {
        // Hilfsfunktion: Prüft ob ein Player gültig ist (nicht null und nicht "TBD")
        bool IsValidPlayer(Player? player)
        {
            return player != null && 
                   !string.IsNullOrEmpty(player.Name) && 
                   !player.Name.Equals("TBD", StringComparison.OrdinalIgnoreCase) &&
                   !player.Name.Equals("To Be Determined", StringComparison.OrdinalIgnoreCase);
        }

        System.Diagnostics.Debug.WriteLine($"        DetermineAutomaticWinner: Match {match.Id}");
        System.Diagnostics.Debug.WriteLine($"          Player1: {match.Player1?.Name ?? "null"}");
        System.Diagnostics.Debug.WriteLine($"          Player2: {match.Player2?.Name ?? "null"}");

        // Fall 1: Nur Player1 ist ein gültiger Spieler
        if (IsValidPlayer(match.Player1) && !IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Player1 {match.Player1.Name} wins automatically (Player2 is TBD/null)");
            return match.Player1;
        }
            
        // Fall 2: Nur Player2 ist ein gültiger Spieler
        if (!IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Player2 {match.Player2.Name} wins automatically (Player1 is TBD/null)");
            return match.Player2;
        }
            
        // Fall 3: Beide Spieler sind gültig - kein automatisches Bye
        if (IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Both players valid, no automatic bye");
            return null;
        }

        // Fall 4: Beide Spieler sind TBD/null - kein automatisches Bye
        System.Diagnostics.Debug.WriteLine($"          -> Both players TBD/null, no automatic bye");
        return null;
    }

    /// <summary>
    /// ÖFFENTLICHE METHODE: Manueller Refresh aller Freilose
    /// </summary>
    public void RefreshAllByeMatches()
    {
        System.Diagnostics.Debug.WriteLine($"=== PUBLIC RefreshAllByeMatches START ===");
        
        if (_tournament.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return;
        }

        var winnerBracket = _tournament.CurrentPhase.WinnerBracket;
        var loserBracket = _tournament.CurrentPhase.LoserBracket;
        
        CheckAndHandleAutomaticByes(winnerBracket, loserBracket);
        if (loserBracket.Any())
        {
            CheckAndHandleAutomaticByes(loserBracket, winnerBracket);
        }
        
        // Trigger UI refresh nach Bye-Check
        _tournament.TriggerUIRefresh();
        
        System.Diagnostics.Debug.WriteLine($"=== PUBLIC RefreshAllByeMatches END ===");
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE für manuelle Freilos-Vergabe
    /// </summary>
    /// <param name="match">Das Match, in dem ein Freilos vergeben werden soll</param>
    /// <param name="byeWinner">Der Spieler, der das Freilos bekommen soll (null = automatisch bestimmen)</param>
    /// <returns>True wenn erfolgreich, False wenn nicht möglich</returns>
    public bool GiveManualBye(KnockoutMatch match, Player? byeWinner = null)
    {
        System.Diagnostics.Debug.WriteLine($"=== GiveManualBye START for match {match.Id} ===");
        
        try
        {
            if (_tournament.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in knockout phase - cannot give bye");
                return false;
            }

            // Validation: Match must not be finished
            if (match.Status == MatchStatus.Finished)
            {
                System.Diagnostics.Debug.WriteLine($"  Match {match.Id} already finished - cannot give bye");
                return false;
            }

            // Determine bye winner
            Player? winner = byeWinner;
            
            if (winner == null)
            {
                // Automatic determination
                if (match.Player1 != null && match.Player2 == null)
                    winner = match.Player1;
                else if (match.Player1 == null && match.Player2 != null)
                    winner = match.Player2;
                else if (match.Player1 != null && match.Player2 != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Both players present in match {match.Id} - manual bye winner required");
                    return false; // Beide Spieler vorhanden - explizite Auswahl nötig
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  No players in match {match.Id} - cannot give bye");
                    return false; // Keine Spieler vorhanden
                }
            }
            else
            {
                // Validate that the specified winner is actually in the match
                if (match.Player1?.Id != winner.Id && match.Player2?.Id != winner.Id)
                {
                    System.Diagnostics.Debug.WriteLine($"  Specified winner {winner.Name} not in match {match.Id}");
                    return false;
                }
            }

            if (winner == null)
            {
                System.Diagnostics.Debug.WriteLine($"  Could not determine winner for bye in match {match.Id}");
                return false;
            }

            // Apply the bye - WICHTIG: Status wird auf Bye gesetzt für korrektes Design
            match.Status = MatchStatus.Bye;
            match.Winner = winner;
            
            // ✅ WICHTIG: Setze Loser wenn beide Spieler vorhanden waren!
            if (match.Player1 != null && match.Player2 != null)
            {
                // Beide Spieler waren vorhanden - der andere ist der Verlierer
                match.Loser = (match.Player1.Id == winner.Id) ? match.Player2 : match.Player1;
                System.Diagnostics.Debug.WriteLine($"  Bye match loser set to {match.Loser.Name} (other player)");
            }
            else
            {
                // Nur ein Spieler vorhanden - kein Verlierer
                match.Loser = null;
                System.Diagnostics.Debug.WriteLine($"  No loser (only one player was present)");
            }
            
            match.Player1Sets = 0;
            match.Player2Sets = 0;
            match.Player1Legs = 0;
            match.Player2Legs = 0;
            match.EndTime = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"  Manual bye given to {winner.Name} in match {match.Id} - Status set to Bye");

            // Propagate the bye result
            var winnerBracket = _tournament.CurrentPhase.WinnerBracket;
            var loserBracket = _tournament.CurrentPhase.LoserBracket;

            if (match.BracketType == BracketType.Winner)
            {
                PropagateMatchResultWithAutomaticByes(match, winnerBracket, loserBracket);
            }
            else
            {
                PropagateMatchResultWithAutomaticByes(match, loserBracket, null);
            }

            // WICHTIG: UI-Refresh triggern um visuelles Update zu erzwingen
            _tournament.TriggerUIRefresh();
            
            // ZUSÄTZLICH: Feuere ein spezifisches Event für Datenänderungen
            // KORREKTUR: Verwende eine öffentliche Methode anstatt direkten Event-Zugriff
            _tournament.TriggerMatchStatusRefresh(match.Id, MatchStatus.Bye);
            
            System.Diagnostics.Debug.WriteLine($"=== GiveManualBye SUCCESS for match {match.Id} ===");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GiveManualBye ERROR: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE zum Rückgängigmachen eines Freiloses
    /// </summary>
    /// <param name="match">Das Match, dessen Freilos rückgängig gemacht werden soll</param>
    /// <returns>True wenn erfolgreich, False wenn nicht möglich</returns>
    public bool UndoBye(KnockoutMatch match)
    {
        System.Diagnostics.Debug.WriteLine($"=== UndoBye START for match {match.Id} ===");
        
        try
        {
            if (_tournament.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in knockout phase - cannot undo bye");
                return false;
            }

            // Validation: Match must be a bye
            if (match.Status != MatchStatus.Bye)
            {
                System.Diagnostics.Debug.WriteLine($"  Match {match.Id} is not a bye - cannot undo");
                return false;
            }

            // Check if there are dependent matches that would be affected
            var allMatches = _tournament.CurrentPhase.WinnerBracket.Concat(_tournament.CurrentPhase.LoserBracket).ToList();
            var dependentMatches = allMatches.Where(m => 
                (m.SourceMatch1?.Id == match.Id) || (m.SourceMatch2?.Id == match.Id)).ToList();

            bool hasProgressedDependents = dependentMatches.Any(m => 
                m.Status == MatchStatus.Finished || 
                (m.Status == MatchStatus.Bye && m.Winner != null));

            if (hasProgressedDependents)
            {
                System.Diagnostics.Debug.WriteLine($"  Cannot undo bye for match {match.Id} - dependent matches have progressed");
                return false;
            }

            // Reset the match - WICHTIG: Status wird zurück auf NotStarted gesetzt
            match.Status = MatchStatus.NotStarted;
            match.Winner = null;
            match.Loser = null;
            match.Player1Sets = 0;
            match.Player2Sets = 0;
            match.Player1Legs = 0;
            match.Player2Legs = 0;
            match.EndTime = null;

            System.Diagnostics.Debug.WriteLine($"  Bye undone for match {match.Id} - Status reset to NotStarted");

            // Clear dependent matches
            foreach (var dependent in dependentMatches)
            {
                if (dependent.SourceMatch1?.Id == match.Id)
                {
                    dependent.Player1 = null;
                }
                if (dependent.SourceMatch2?.Id == match.Id)
                {
                    dependent.Player2 = null;
                }
            }

            // Re-check for automatic byes after the change
            RefreshAllByeMatches();

            // WICHTIG: Zusätzlicher UI-Refresh um visuelles Update zu erzwingen  
            _tournament.TriggerUIRefresh();
            
            // ZUSÄTZLICH: Feuere ein spezifisches Event für Datenänderungen
            // KORREKTUR: Verwende eine öffentliche Methode anstatt direkten Event-Zugriff
            _tournament.TriggerMatchStatusRefresh(match.Id, MatchStatus.NotStarted);
            
            System.Diagnostics.Debug.WriteLine($"=== UndoBye SUCCESS for match {match.Id} ===");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UndoBye ERROR: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE zur Validierung von Freilos-Operationen
    /// </summary>
    /// <param name="match">Das zuvalidierende Match</param>
    /// <returns>Validierungsresultat</returns>
    public ByeValidationResult ValidateByeOperation(KnockoutMatch match)
    {
        if (_tournament.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return new ByeValidationResult(false, "Nicht in K.O.-Phase", false, false);
        }

        bool canGiveBye = false;
        bool canUndoBye = false;
        string message = "";

        if (match.Status == MatchStatus.Bye)
        {
            // Check if bye can be undone
            var allMatches = _tournament.CurrentPhase.WinnerBracket.Concat(_tournament.CurrentPhase.LoserBracket).ToList();
            var dependentMatches = allMatches.Where(m => 
                (m.SourceMatch1?.Id == match.Id) || (m.SourceMatch2?.Id == match.Id)).ToList();

            bool hasProgressedDependents = dependentMatches.Any(m => 
                m.Status == MatchStatus.Finished || 
                (m.Status == MatchStatus.Bye && m.Winner != null));

            canUndoBye = !hasProgressedDependents;
            message = canUndoBye ? "Freilos kann rückgängig gemacht werden" : "Freilos kann nicht rückgängig gemacht werden - nachfolgende Matches bereits gespielt";
        }
        else if (match.Status == MatchStatus.NotStarted)
        {
            // Check if bye can be given
            bool hasPlayers = match.Player1 != null || match.Player2 != null;
            canGiveBye = hasPlayers;
            message = hasPlayers ? "Freilos kann vergeben werden" : "Keine Spieler im Match - Freilos nicht möglich";
        }
        else if (match.Status == MatchStatus.Finished)
        {
            message = "Match bereits beendet - keine Freilos-Operationen möglich";
        }
        else
        {
            message = "Match läuft - keine Freilos-Operationen möglich";
        }

        return new ByeValidationResult(true, message, canGiveBye, canUndoBye);
    }

    /// <summary>
    /// BEISPIEL: Status-Überprüfung für UI-Buttons
    /// </summary>
    /// <param name="matchId">ID des Matches</param>
    /// <returns>UI-Status für Freilos-Buttons</returns>
    public MatchByeUIStatus GetMatchByeUIStatus(int matchId)
    {
        if (_tournament.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return new MatchByeUIStatus(false, false, "Nicht in K.O.-Phase");
        }

        var allMatches = _tournament.CurrentPhase.WinnerBracket.Concat(_tournament.CurrentPhase.LoserBracket).ToList();
        var match = allMatches.FirstOrDefault(m => m.Id == matchId);
        
        if (match == null)
        {
            return new MatchByeUIStatus(false, false, "Match nicht gefunden");
        }

        var validation = ValidateByeOperation(match);
        
        return new MatchByeUIStatus(
            validation.CanGiveBye,
            validation.CanUndoBye,
            validation.Message
        );
    }

    private Player? GetMatchLoser(KnockoutMatch match)
    {
        if (match.Winner == null || match.Status == MatchStatus.Bye) 
            return null;

        if (match.Player1 != null && match.Player2 != null)
        {
            return match.Winner.Id == match.Player1.Id ? match.Player2 : match.Player1;
        }

        return null;
    }
}