using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Service für die Verarbeitung von Match-Updates vom Tournament Hub
/// Verwaltet die Suche und Aktualisierung von Matches in verschiedenen Tournament-Phasen
/// ERWEITERT: Verarbeitet auch Dart-Statistiken aus WebSocket-Nachrichten
/// </summary>
public class HubMatchProcessingService
{
    private readonly Func<int, TournamentClass?> _getTournamentClassById;

    public HubMatchProcessingService(Func<int, TournamentClass?> getTournamentClassById)
    {
        _getTournamentClassById = getTournamentClassById;
    }

    /// <summary>
    /// Verarbeitet Match-Updates vom Hub und wendet sie auf die entsprechenden Matches an
    /// Unterstützt alle Match-Typen: Group, Finals, Winner Bracket, Loser Bracket
    /// ERWEITERT: Verarbeitet auch Dart-Statistiken
    /// </summary>
    public bool ProcessHubMatchUpdate(HubMatchUpdateEventArgs e, out string? errorMessage)
    {
        errorMessage = null;
        var debugWindow = HubIntegrationService.GlobalDebugWindow;
        
        try
        {
            debugWindow?.AddDebugMessage("📥 ===== MATCH UPDATE PROCESSING START =====", "MATCH");
            debugWindow?.AddDebugMessage($"🎯 Processing Match {e.MatchId} in Class {e.ClassId}", "MATCH");
            
            var tournamentClass = _getTournamentClassById(e.ClassId);
            if (tournamentClass == null)
            {
                errorMessage = $"Tournament class {e.ClassId} not found";
                debugWindow?.AddDebugMessage($"❌ {errorMessage}", "ERROR");
                return false;
            }

            debugWindow?.AddDebugMessage($"✅ Tournament class found: {tournamentClass.Name}", "SUCCESS");

            // ✅ NEU: Verarbeite Dart-Statistiken
            if (e.Source == "hub-match-result" && !string.IsNullOrEmpty(e.Notes))
            {
                debugWindow?.AddDebugMessage($"📊 Processing dart statistics for class {tournamentClass.Name}", "STATISTICS");
                try
                {
                    tournamentClass.ProcessMatchStatistics(e);
                    debugWindow?.AddDebugMessage($"✅ Dart statistics processed successfully", "SUCCESS");
                }
                catch (Exception statsEx)
                {
                    debugWindow?.AddDebugMessage($"❌ Error processing dart statistics: {statsEx.Message}", "ERROR");
                    // Weiter mit normalem Match-Processing
                }
            }

            // ✅ NEU: Auch für websocket-direct Source verarbeiten (erweiterte Statistiken)
            if ((e.Source == "websocket-direct" || e.Source == "hub-websocket-direct") && !string.IsNullOrEmpty(e.Notes))
            {
                debugWindow?.AddDebugMessage($"📊 Processing enhanced dart statistics for class {tournamentClass.Name}", "STATISTICS");
                try
                {
                    // Prüfe ob es sich um erweiterte JSON-Statistiken handelt
                    if (e.Notes.TrimStart().StartsWith("{"))
                    {
                        debugWindow?.AddDebugMessage($"🔍 Detected enhanced JSON statistics format", "STATISTICS");
                        debugWindow?.AddDebugMessage($"📊 Processing dartScoringResult data", "STATISTICS");
                        tournamentClass.ProcessMatchStatistics(e);
                        debugWindow?.AddDebugMessage($"✅ Enhanced dart statistics processed successfully", "SUCCESS");
                    }
                    else
                    {
                        debugWindow?.AddDebugMessage($"🔍 Using legacy notes-based statistics processing", "STATISTICS");
                        tournamentClass.ProcessMatchStatistics(e);
                        debugWindow?.AddDebugMessage($"✅ Legacy dart statistics processed successfully", "SUCCESS");
                    }
                }
                catch (Exception statsEx)
                {
                    debugWindow?.AddDebugMessage($"❌ Error processing enhanced dart statistics: {statsEx.Message}", "ERROR");
                    // Weiter mit normalem Match-Processing
                }
            }

            // Suche das Match in verschiedenen Bereichen
            var matchResult = FindMatch(tournamentClass, e);
            
            if (matchResult.Match == null && matchResult.KnockoutMatch == null)
            {
                errorMessage = $"Match {e.MatchId} not found in any tournament phase";
                debugWindow?.AddDebugMessage($"❌ {errorMessage}", "WARNING");
                debugWindow?.AddDebugMessage($"🔍 Searched in: {matchResult.SearchedAreas}", "INFO");
                return false;
            }

            debugWindow?.AddDebugMessage($"✅ Match found in: {matchResult.Location}", "SUCCESS");

            // Aktualisiere das gefundene Match
            bool wasUpdated = false;
            if (matchResult.Match != null)
            {
                debugWindow?.AddDebugMessage($"🔧 Updating regular Match: {matchResult.Match.Id}", "MATCH");
                wasUpdated = UpdateMatch(matchResult.Match, e);
            }
            else if (matchResult.KnockoutMatch != null)
            {
                debugWindow?.AddDebugMessage($"🔧 Updating KnockoutMatch: {matchResult.KnockoutMatch.Id}", "MATCH");
                wasUpdated = UpdateKnockoutMatch(matchResult.KnockoutMatch, e);
                
                // 🚨 WICHTIG: Zusätzliches Logging für KnockoutMatch-Updates
                if (wasUpdated && matchResult.KnockoutMatch.Status == MatchStatus.Finished)
                {
                    debugWindow?.AddDebugMessage($"🎯 KnockoutMatch {matchResult.KnockoutMatch.Id} finished via Hub - progression should be triggered", "MATCH_RESULT");
                }
            }

            if (wasUpdated)
            {
                debugWindow?.AddDebugMessage($"✅ Match {e.MatchId} successfully updated", "SUCCESS");
                tournamentClass.TriggerUIRefresh();
                debugWindow?.AddDebugMessage($"🔄 UI refresh triggered for {tournamentClass.Name}", "SYNC");
                debugWindow?.AddDebugMessage("📥 ===== MATCH UPDATE PROCESSING COMPLETE =====", "MATCH");
                return true;
            }
            else
            {
                debugWindow?.AddDebugMessage($"ℹ️ No changes detected for Match {e.MatchId}", "INFO");
                debugWindow?.AddDebugMessage("📥 ===== MATCH UPDATE PROCESSING END (NO CHANGES) =====", "MATCH");
                return false;
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            debugWindow?.AddDebugMessage($"❌ Error processing match update: {ex.Message}", "ERROR");
            debugWindow?.AddDebugMessage("📥 ===== MATCH UPDATE PROCESSING FAILED =====", "ERROR");
            return false;
        }
    }

    private MatchSearchResult FindMatch(TournamentClass tournamentClass, HubMatchUpdateEventArgs e)
    {
        var result = new MatchSearchResult();
        var debugWindow = HubIntegrationService.GlobalDebugWindow;
        var searchedAreas = new List<string>();

        debugWindow?.AddDebugMessage("🔍 Starting match search...", "SEARCH");
        debugWindow?.AddDebugMessage($"🆔 Match identifiers:", "SEARCH");
        debugWindow?.AddDebugMessage($"   Numeric ID: {e.MatchId}", "SEARCH");
        debugWindow?.AddDebugMessage($"   UUID: {e.MatchUuid ?? "none"}", "SEARCH");
        debugWindow?.AddDebugMessage($"   Original String: {e.OriginalMatchIdString ?? "none"}", "SEARCH");

        // Helper function für UUID-aware Match-Suche
        Func<IEnumerable<Match>, Match?> FindMatchInCollection = (matches) =>
        {
            // Priorität 1: Suche über UUID (wenn verfügbar)
            if (!string.IsNullOrEmpty(e.MatchUuid))
            {
                var uuidMatch = matches.FirstOrDefault(m => m.UniqueId == e.MatchUuid);
                if (uuidMatch != null)
                {
                    debugWindow?.AddDebugMessage($"✅ Match found via UUID: {e.MatchUuid}", "SUCCESS");
                    return uuidMatch;
                }
            }
            
            // Priorität 2: Suche über numerische ID (wenn != 0)
            if (e.MatchId != 0)
            {
                var numericMatch = matches.FirstOrDefault(m => m.Id == e.MatchId);
                if (numericMatch != null)
                {
                    debugWindow?.AddDebugMessage($"✅ Match found via numeric ID: {e.MatchId}", "SUCCESS");
                    return numericMatch;
                }
            }
            
            return null;
        };

        // Helper function für UUID-aware KnockoutMatch-Suche
        Func<IEnumerable<KnockoutMatch>, KnockoutMatch?> FindKnockoutMatchInCollection = (knockoutMatches) =>
        {
            // Priorität 1: Suche über UUID (wenn verfügbar)
            if (!string.IsNullOrEmpty(e.MatchUuid))
            {
                var uuidKoMatch = knockoutMatches.FirstOrDefault(km => km.UniqueId == e.MatchUuid);
                if (uuidKoMatch != null)
                {
                    debugWindow?.AddDebugMessage($"✅ KnockoutMatch found via UUID: {e.MatchUuid}", "SUCCESS");
                    return uuidKoMatch;
                }
            }
            
            // Priorität 2: Suche über numerische ID (wenn != 0)
            if (e.MatchId != 0)
            {
                var numericKoMatch = knockoutMatches.FirstOrDefault(km => km.Id == e.MatchId);
                if (numericKoMatch != null)
                {
                    debugWindow?.AddDebugMessage($"✅ KnockoutMatch found via numeric ID: {e.MatchId}", "SUCCESS");
                    return numericKoMatch;
                }
            }
            
            return null;
        };

        // 1. Winner Bracket Suche
        if (!string.IsNullOrEmpty(e.GroupName) && e.GroupName.Contains("Winner Bracket"))
        {
            debugWindow?.AddDebugMessage($"🏆 Searching in Winner Bracket", "SEARCH");
            searchedAreas.Add("Winner Bracket");
            
            if (tournamentClass.CurrentPhase?.WinnerBracket != null)
            {
                result.KnockoutMatch = FindKnockoutMatchInCollection(tournamentClass.CurrentPhase.WinnerBracket);
                if (result.KnockoutMatch != null)
                {
                    result.Location = $"Winner Bracket";
                    debugWindow?.AddDebugMessage($"✅ Match found in Winner Bracket", "SUCCESS");
                    return result;
                }
            }
            debugWindow?.AddDebugMessage($"❌ Match not found in Winner Bracket", "WARNING");
        }

        // 2. Loser Bracket Suche  
        if (!string.IsNullOrEmpty(e.GroupName) && e.GroupName.Contains("Loser Bracket"))
        {
            debugWindow?.AddDebugMessage($"🥈 Searching in Loser Bracket", "SEARCH");
            searchedAreas.Add("Loser Bracket");
            
            if (tournamentClass.CurrentPhase?.LoserBracket != null)
            {
                result.KnockoutMatch = FindKnockoutMatchInCollection(tournamentClass.CurrentPhase.LoserBracket);
                if (result.KnockoutMatch != null)
                {
                    result.Location = $"Loser Bracket";
                    debugWindow?.AddDebugMessage($"✅ Match found in Loser Bracket", "SUCCESS");
                    return result;
                }
            }
            debugWindow?.AddDebugMessage($"❌ Match not found in Loser Bracket", "WARNING");
        }

        // 3. Group Match Suche
        if (!string.IsNullOrEmpty(e.GroupName) && e.MatchType == "Group")
        {
            debugWindow?.AddDebugMessage($"👥 Searching in specific group: '{e.GroupName}'", "SEARCH");
            searchedAreas.Add($"Group '{e.GroupName}'");
            
            var targetGroup = tournamentClass.Groups.FirstOrDefault(g => 
                g.Name.Equals(e.GroupName, StringComparison.OrdinalIgnoreCase));
            
            if (targetGroup != null)
            {
                debugWindow?.AddDebugMessage($"📋 Target group found: {targetGroup.Name}", "SUCCESS");
                result.Match = FindMatchInCollection(targetGroup.Matches);
                if (result.Match != null)
                {
                    result.Group = targetGroup;
                    result.Location = $"Group '{targetGroup.Name}'";
                    debugWindow?.AddDebugMessage($"✅ Match found in specific group", "SUCCESS");
                    return result;
                }
                debugWindow?.AddDebugMessage($"❌ Match not found in group '{targetGroup.Name}'", "WARNING");
            }
            else
            {
                debugWindow?.AddDebugMessage($"❌ Group '{e.GroupName}' not found", "WARNING");
            }
        }

        // 4. Finals Suche
        if (!string.IsNullOrEmpty(e.GroupName) && e.GroupName.Contains("Finals"))
        {
            debugWindow?.AddDebugMessage($"🏆 Searching in Finals", "SEARCH");
            searchedAreas.Add("Finals");
            
            if (tournamentClass.CurrentPhase?.FinalsGroup != null)
            {
                result.Match = FindMatchInCollection(tournamentClass.CurrentPhase.FinalsGroup.Matches);
                if (result.Match != null)
                {
                    result.Group = tournamentClass.CurrentPhase.FinalsGroup;
                    result.Location = "Finals";
                    debugWindow?.AddDebugMessage($"✅ Match found in Finals", "SUCCESS");
                    return result;
                }
            }
            debugWindow?.AddDebugMessage($"❌ Match not found in Finals", "WARNING");
        }

        // 5. Fallback: Suche in allen Bereichen
        debugWindow?.AddDebugMessage($"🔍 Starting fallback search in all areas", "SEARCH");
        return FallbackSearch(tournamentClass, e, searchedAreas, FindMatchInCollection, FindKnockoutMatchInCollection);
    }

    private MatchSearchResult FallbackSearch(
        TournamentClass tournamentClass, 
        HubMatchUpdateEventArgs e, 
        List<string> searchedAreas,
        Func<IEnumerable<Match>, Match?> findMatchFunc,
        Func<IEnumerable<KnockoutMatch>, KnockoutMatch?> findKnockoutMatchFunc)
    {
        var result = new MatchSearchResult();
        var debugWindow = HubIntegrationService.GlobalDebugWindow;

        debugWindow?.AddDebugMessage($"🔄 Fallback search with UUID support", "SEARCH");
        debugWindow?.AddDebugMessage($"🆔 Looking for: UUID={e.MatchUuid ?? "none"}, NumericID={e.MatchId}", "SEARCH");

        // Suche in allen Gruppen
        foreach (var group in tournamentClass.Groups)
        {
            if (!searchedAreas.Contains($"Group '{group.Name}'"))
            {
                result.Match = findMatchFunc(group.Matches);
                if (result.Match != null)
                {
                    result.Group = group;
                    result.Location = $"Group '{group.Name}' (Fallback)";
                    debugWindow?.AddDebugMessage($"✅ Match found in fallback search: {group.Name}", "SUCCESS");
                    debugWindow?.AddDebugMessage($"🆔 Match identification: UUID={result.Match.UniqueId}, NumericID={result.Match.Id}", "SUCCESS");
                    return result;
                }
            }
        }

        // Suche in Finals (falls noch nicht gesucht)
        if (!searchedAreas.Contains("Finals") && tournamentClass.CurrentPhase?.FinalsGroup != null)
        {
            result.Match = findMatchFunc(tournamentClass.CurrentPhase.FinalsGroup.Matches);
            if (result.Match != null)
            {
                result.Group = tournamentClass.CurrentPhase.FinalsGroup;
                result.Location = "Finals (Fallback)";
                debugWindow?.AddDebugMessage($"✅ Match found in Finals fallback search", "SUCCESS");
                debugWindow?.AddDebugMessage($"🆔 Match identification: UUID={result.Match.UniqueId}, NumericID={result.Match.Id}", "SUCCESS");
                return result;
            }
        }

        // Suche in Winner Bracket (falls noch nicht gesucht)
        if (!searchedAreas.Contains("Winner Bracket") && tournamentClass.CurrentPhase?.WinnerBracket != null)
        {
            result.KnockoutMatch = findKnockoutMatchFunc(tournamentClass.CurrentPhase.WinnerBracket);
            if (result.KnockoutMatch != null)
            {
                result.Location = "Winner Bracket (Fallback)";
                debugWindow?.AddDebugMessage($"✅ KnockoutMatch found in Winner Bracket fallback search", "SUCCESS");
                debugWindow?.AddDebugMessage($"🆔 KnockoutMatch identification: UUID={result.KnockoutMatch.UniqueId}, NumericID={result.KnockoutMatch.Id}", "SUCCESS");
                return result;
            }
        }

        // Suche in Loser Bracket (falls noch nicht gesucht)
        if (!searchedAreas.Contains("Loser Bracket") && tournamentClass.CurrentPhase?.LoserBracket != null)
        {
            result.KnockoutMatch = findKnockoutMatchFunc(tournamentClass.CurrentPhase.LoserBracket);
            if (result.KnockoutMatch != null)
            {
                result.Location = "Loser Bracket (Fallback)";
                debugWindow?.AddDebugMessage($"✅ KnockoutMatch found in Loser Bracket fallback search", "SUCCESS");
                debugWindow?.AddDebugMessage($"🆔 KnockoutMatch identification: UUID={result.KnockoutMatch.UniqueId}, NumericID={result.KnockoutMatch.Id}", "SUCCESS");
                return result;
            }
        }

        searchedAreas.AddRange(new[] { "All Groups", "Finals", "Winner Bracket", "Loser Bracket" });
        result.SearchedAreas = string.Join(", ", searchedAreas);
        debugWindow?.AddDebugMessage($"❌ Match not found anywhere with UUID={e.MatchUuid ?? "none"}, NumericID={e.MatchId}. Searched: {result.SearchedAreas}", "ERROR");
        
        return result;
    }

    private bool UpdateMatch(Match match, HubMatchUpdateEventArgs hubData)
    {
        var debugWindow = HubIntegrationService.GlobalDebugWindow;
        
        try
        {
            debugWindow?.AddDebugMessage($"🔧 Updating Match {hubData.MatchId}", "MATCH");
            debugWindow?.AddDebugMessage($"   Current: {match.Player1Sets}-{match.Player2Sets} Sets, {match.Player1Legs}-{match.Player2Legs} Legs, Status: {match.Status}", "INFO");
            debugWindow?.AddDebugMessage($"   Hub Data: {hubData.Player1Sets}-{hubData.Player2Sets} Sets, {hubData.Player1Legs}-{hubData.Player2Legs} Legs, Status: {hubData.Status}", "INFO");
            
            // Prüfe ob es tatsächlich Änderungen gibt
            if (match.Player1Sets == hubData.Player1Sets &&
                match.Player2Sets == hubData.Player2Sets &&
                match.Player1Legs == hubData.Player1Legs &&
                match.Player2Legs == hubData.Player2Legs &&
                match.Status.ToString() == hubData.Status)
            {
                debugWindow?.AddDebugMessage($"   No changes detected, skipping update", "INFO");
                return false;
            }

            // Aktualisiere Match-Daten
            match.Player1Sets = hubData.Player1Sets;
            match.Player2Sets = hubData.Player2Sets;
            match.Player1Legs = hubData.Player1Legs;
            match.Player2Legs = hubData.Player2Legs;
            match.Notes = hubData.Notes ?? match.Notes;

            // Aktualisiere Status
            if (Enum.TryParse<MatchStatus>(hubData.Status, out var newStatus))
            {
                match.Status = newStatus;
                debugWindow?.AddDebugMessage($"   Status updated to: {newStatus}", "INFO");
            }

            // Setze End-Zeit wenn abgeschlossen
            if (match.Status == MatchStatus.Finished && match.EndTime == null)
            {
                match.EndTime = DateTime.Now;
                debugWindow?.AddDebugMessage($"   End time set to: {match.EndTime}", "INFO");
            }

            // Bestimme Gewinner - verwende SetResult für korrekte Gewinner-Bestimmung
            match.SetResult(match.Player1Sets, match.Player2Sets, match.Player1Legs, match.Player2Legs);
            
            debugWindow?.AddDebugMessage($"   Winner determined: {match.Winner?.Name ?? "None"}", "INFO");
            debugWindow?.AddDebugMessage($"   ✅ Match updated successfully", "SUCCESS");

            return true;
        }
        catch (Exception ex)
        {
            debugWindow?.AddDebugMessage($"❌ Error updating match: {ex.Message}", "ERROR");
            return false;
        }
    }

    private bool UpdateKnockoutMatch(KnockoutMatch knockoutMatch, HubMatchUpdateEventArgs hubData)
    {
        var debugWindow = HubIntegrationService.GlobalDebugWindow;
        
        try
        {
            debugWindow?.AddDebugMessage($"🔧 Updating KnockoutMatch {hubData.MatchId}", "MATCH");
            debugWindow?.AddDebugMessage($"   Current: {knockoutMatch.Player1Sets}-{knockoutMatch.Player2Sets} Sets, {knockoutMatch.Player1Legs}-{knockoutMatch.Player2Legs} Legs, Status: {knockoutMatch.Status}", "INFO");
            debugWindow?.AddDebugMessage($"   Hub Data: {hubData.Player1Sets}-{hubData.Player2Sets} Sets, {hubData.Player1Legs}-{hubData.Player2Legs} Legs, Status: {hubData.Status}", "INFO");
            
            // Prüfe ob es tatsächlich Änderungen gibt
            if (knockoutMatch.Player1Sets == hubData.Player1Sets &&
                knockoutMatch.Player2Sets == hubData.Player2Sets &&
                knockoutMatch.Player1Legs == hubData.Player1Legs &&
                knockoutMatch.Player2Legs == hubData.Player2Legs &&
                knockoutMatch.Status.ToString() == hubData.Status)
            {
                debugWindow?.AddDebugMessage($"   No changes detected, skipping update", "INFO");
                return false;
            }

            // Speichere alten Status für Vergleich
            var oldStatus = knockoutMatch.Status;

            // Aktualisiere KnockoutMatch-Daten
            knockoutMatch.Player1Sets = hubData.Player1Sets;
            knockoutMatch.Player2Sets = hubData.Player2Sets;
            knockoutMatch.Player1Legs = hubData.Player1Legs;
            knockoutMatch.Player2Legs = hubData.Player2Legs;
            knockoutMatch.Notes = hubData.Notes ?? knockoutMatch.Notes;

            // Aktualisiere Status
            if (Enum.TryParse<MatchStatus>(hubData.Status, out var newStatus))
            {
                knockoutMatch.Status = newStatus;
                debugWindow?.AddDebugMessage($"   Status updated to: {newStatus}", "INFO");
            }

            // Setze End-Zeit wenn abgeschlossen
            if (knockoutMatch.Status == MatchStatus.Finished && knockoutMatch.EndTime == null)
            {
                knockoutMatch.EndTime = DateTime.Now;
                debugWindow?.AddDebugMessage($"   End time set to: {knockoutMatch.EndTime}", "INFO");
            }

            // Bestimme Gewinner für KnockoutMatch - verwende SetResult für korrekte Gewinner-Bestimmung
            knockoutMatch.SetResult(knockoutMatch.Player1Sets, knockoutMatch.Player2Sets, knockoutMatch.Player1Legs, knockoutMatch.Player2Legs);
            
            debugWindow?.AddDebugMessage($"   Winner determined: {knockoutMatch.Winner?.Name ?? "None"}", "INFO");

            // 🚨 KRITISCH: Wenn das Match jetzt finished ist und vorher nicht, triggere die Progression!
            if (knockoutMatch.Status == MatchStatus.Finished && oldStatus != MatchStatus.Finished && knockoutMatch.Winner != null)
            {
                debugWindow?.AddDebugMessage($"   🎯 Match finished via Hub update - triggering progression!", "MATCH_RESULT");
                
                // Hole die TournamentClass über die getTournamentClassById Funktion
                var tournamentClass = _getTournamentClassById(hubData.ClassId);
                if (tournamentClass != null)
                {
                    debugWindow?.AddDebugMessage($"   📋 Found TournamentClass: {tournamentClass.Name}", "SUCCESS");
                    
                    // WICHTIG: Verwende ProcessMatchResult aus TournamentClass für korrekte Progression
                    bool progressionSuccess = tournamentClass.ProcessMatchResult(knockoutMatch);
                    
                    if (progressionSuccess)
                    {
                        debugWindow?.AddDebugMessage($"   ✅ KO Match progression completed successfully!", "SUCCESS");
                        debugWindow?.AddDebugMessage($"   🔄 Winner {knockoutMatch.Winner.Name} advanced to next round", "MATCH_RESULT");
                    }
                    else
                    {
                        debugWindow?.AddDebugMessage($"   ⚠️ KO Match progression failed or not needed", "WARNING");
                    }
                }
                else
                {
                    debugWindow?.AddDebugMessage($"   ❌ Could not find TournamentClass for progression", "ERROR");
                }
            }
            else if (oldStatus == MatchStatus.Finished && knockoutMatch.Status != MatchStatus.Finished)
            {
                debugWindow?.AddDebugMessage($"   ⚠️ Match status reverted from finished - manual intervention may be needed", "WARNING");
            }
            
            debugWindow?.AddDebugMessage($"   ✅ KnockoutMatch updated successfully", "SUCCESS");

            return true;
        }
        catch (Exception ex)
        {
            debugWindow?.AddDebugMessage($"❌ Error updating knockout match: {ex.Message}", "ERROR");
            return false;
        }
    }

    private class MatchSearchResult
    {
        public Match? Match { get; set; }
        public KnockoutMatch? KnockoutMatch { get; set; }
        public Group? Group { get; set; }
        public string Location { get; set; } = "";
        public string SearchedAreas { get; set; } = "";
    }
}