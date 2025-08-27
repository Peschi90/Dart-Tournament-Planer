using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Service für die Verarbeitung von Match-Updates vom Tournament Hub
/// Verwaltet die Suche und Aktualisierung von Matches in verschiedenen Tournament-Phasen
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

        // 1. Winner Bracket Suche
        if (!string.IsNullOrEmpty(e.GroupName) && e.GroupName.Contains("Winner Bracket"))
        {
            debugWindow?.AddDebugMessage($"🏆 Searching in Winner Bracket for Match {e.MatchId}", "SEARCH");
            searchedAreas.Add("Winner Bracket");
            
            result.KnockoutMatch = tournamentClass.CurrentPhase?.WinnerBracket?.FirstOrDefault(km => km.Id == e.MatchId);
            if (result.KnockoutMatch != null)
            {
                result.Location = $"Winner Bracket";
                debugWindow?.AddDebugMessage($"✅ Match found in Winner Bracket", "SUCCESS");
                return result;
            }
            debugWindow?.AddDebugMessage($"❌ Match not found in Winner Bracket", "WARNING");
        }

        // 2. Loser Bracket Suche  
        if (!string.IsNullOrEmpty(e.GroupName) && e.GroupName.Contains("Loser Bracket"))
        {
            debugWindow?.AddDebugMessage($"🥈 Searching in Loser Bracket for Match {e.MatchId}", "SEARCH");
            searchedAreas.Add("Loser Bracket");
            
            result.KnockoutMatch = tournamentClass.CurrentPhase?.LoserBracket?.FirstOrDefault(km => km.Id == e.MatchId);
            if (result.KnockoutMatch != null)
            {
                result.Location = $"Loser Bracket";
                debugWindow?.AddDebugMessage($"✅ Match found in Loser Bracket", "SUCCESS");
                return result;
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
                result.Match = targetGroup.Matches.FirstOrDefault(m => m.Id == e.MatchId);
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
            debugWindow?.AddDebugMessage($"🏆 Searching in Finals for Match {e.MatchId}", "SEARCH");
            searchedAreas.Add("Finals");
            
            result.Match = tournamentClass.CurrentPhase?.FinalsGroup?.Matches.FirstOrDefault(m => m.Id == e.MatchId);
            if (result.Match != null)
            {
                result.Group = tournamentClass.CurrentPhase.FinalsGroup;
                result.Location = "Finals";
                debugWindow?.AddDebugMessage($"✅ Match found in Finals", "SUCCESS");
                return result;
            }
            debugWindow?.AddDebugMessage($"❌ Match not found in Finals", "WARNING");
        }

        // 5. Fallback: Suche in allen Bereichen
        debugWindow?.AddDebugMessage($"🔍 Starting fallback search in all areas", "SEARCH");
        return FallbackSearch(tournamentClass, e.MatchId, searchedAreas);
    }

    private MatchSearchResult FallbackSearch(TournamentClass tournamentClass, int matchId, List<string> searchedAreas)
    {
        var result = new MatchSearchResult();
        var debugWindow = HubIntegrationService.GlobalDebugWindow;

        debugWindow?.AddDebugMessage($"🔄 Fallback search for Match {matchId}", "SEARCH");

        // Suche in allen Gruppen
        foreach (var group in tournamentClass.Groups)
        {
            if (!searchedAreas.Contains($"Group '{group.Name}'"))
            {
                result.Match = group.Matches.FirstOrDefault(m => m.Id == matchId);
                if (result.Match != null)
                {
                    result.Group = group;
                    result.Location = $"Group '{group.Name}' (Fallback)";
                    debugWindow?.AddDebugMessage($"✅ Match found in fallback search: {group.Name}", "SUCCESS");
                    return result;
                }
            }
        }

        // Suche in Finals (falls noch nicht gesucht)
        if (!searchedAreas.Contains("Finals") && tournamentClass.CurrentPhase?.FinalsGroup != null)
        {
            result.Match = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == matchId);
            if (result.Match != null)
            {
                result.Group = tournamentClass.CurrentPhase.FinalsGroup;
                result.Location = "Finals (Fallback)";
                debugWindow?.AddDebugMessage($"✅ Match found in Finals fallback search", "SUCCESS");
                return result;
            }
        }

        // Suche in Winner Bracket (falls noch nicht gesucht)
        if (!searchedAreas.Contains("Winner Bracket") && tournamentClass.CurrentPhase?.WinnerBracket != null)
        {
            result.KnockoutMatch = tournamentClass.CurrentPhase.WinnerBracket.FirstOrDefault(km => km.Id == matchId);
            if (result.KnockoutMatch != null)
            {
                result.Location = "Winner Bracket (Fallback)";
                debugWindow?.AddDebugMessage($"✅ Match found in Winner Bracket fallback search", "SUCCESS");
                return result;
            }
        }

        // Suche in Loser Bracket (falls noch nicht gesucht)
        if (!searchedAreas.Contains("Loser Bracket") && tournamentClass.CurrentPhase?.LoserBracket != null)
        {
            result.KnockoutMatch = tournamentClass.CurrentPhase.LoserBracket.FirstOrDefault(km => km.Id == matchId);
            if (result.KnockoutMatch != null)
            {
                result.Location = "Loser Bracket (Fallback)";
                debugWindow?.AddDebugMessage($"✅ Match found in Loser Bracket fallback search", "SUCCESS");
                return result;
            }
        }

        searchedAreas.AddRange(new[] { "All Groups", "Finals", "Winner Bracket", "Loser Bracket" });
        result.SearchedAreas = string.Join(", ", searchedAreas);
        debugWindow?.AddDebugMessage($"❌ Match {matchId} not found anywhere. Searched: {result.SearchedAreas}", "ERROR");
        
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