// 🚨 KORRIGIERTE VERSION der OnHubMatchResultReceived Funktion für MainWindow.xaml.cs
// Diese Version berücksichtigt die Group-Information bei der Match-Suche

private void OnHubMatchResultReceived(HubMatchUpdateEventArgs e)
{
    Dispatcher.Invoke(() =>
    {
        try
        {
            // Erweiterte Debug-Ausgabe für Match Results
            var isMatchResult = e.Source?.Contains("match-result") == true;
            var logCategory = isMatchResult ? "MATCH_RESULT" : "MATCH";
            
            System.Diagnostics.Debug.WriteLine("📥 [PLANNER] ===== MATCH UPDATE FROM HUB =====");
            System.Diagnostics.Debug.WriteLine($"📥 [PLANNER] Received match update from Hub: Match {e.MatchId} in class {e.ClassId}");
            System.Diagnostics.Debug.WriteLine($"📊 [PLANNER] Result: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs, Status: {e.Status}");
            System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Source: {e.Source}, UpdatedAt: {e.UpdatedAt}");
            System.Diagnostics.Debug.WriteLine($"📋 [PLANNER] Group Info: GroupName='{e.GroupName}', GroupId={e.GroupId}, MatchType='{e.MatchType}'");
            
            // DEBUG CONSOLE LOGGING - mit farblicher Hervorhebung
            if (isMatchResult)
            {
                _hubDebugWindow?.AddDebugMessage("🏆 ===== MATCH-ERGEBNIS EMPFANGEN =====", "MATCH_RESULT");
                _hubDebugWindow?.AddDebugMessage($"📥 Match-Ergebnis: Match {e.MatchId} in Klasse {e.ClassId}", "MATCH_RESULT");
                _hubDebugWindow?.AddDebugMessage($"📊 Endergebnis: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs", "MATCH_RESULT");
                _hubDebugWindow?.AddDebugMessage($"🔍 Status: {e.Status}, Quelle: {e.Source}", "MATCH_RESULT");
                _hubDebugWindow?.AddDebugMessage($"📋 Group Info: '{e.GroupName}' (ID: {e.GroupId})", "MATCH_RESULT");
            }
            else
            {
                _hubDebugWindow?.AddDebugMessage("===== MATCH UPDATE FROM HUB =====", "MATCH");
                _hubDebugWindow?.AddDebugMessage($"📥 Match Update empfangen: Match {e.MatchId} in Klasse {e.ClassId}", "MATCH");
                _hubDebugWindow?.AddDebugMessage($"📊 Ergebnis: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs", "MATCH");
                _hubDebugWindow?.AddDebugMessage($"🔍 Status: {e.Status}, Quelle: {e.Source}", "MATCH");
                _hubDebugWindow?.AddDebugMessage($"📋 Group Info: '{e.GroupName}' (ID: {e.GroupId})", "MATCH");
            }
            
            // Finde die entsprechende Tournament-Klasse
            var tournamentClass = GetTournamentClassById(e.ClassId);
            if (tournamentClass == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Tournament class {e.ClassId} not found");
                _hubDebugWindow?.AddDebugMessage($"⚠️ Tournament-Klasse {e.ClassId} nicht gefunden", "WARNING");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found tournament class: {tournamentClass.Name}");
            _hubDebugWindow?.AddDebugMessage($"✅ Tournament-Klasse gefunden: {tournamentClass.Name}", "SUCCESS");

            // Finde das entsprechende Match
            Match? targetMatch = null;
            Group? targetGroup = null;

            // 🚨 KORRIGIERT: Verwende Group-spezifische Suche falls Group-Information verfügbar ist
            if (!string.IsNullOrEmpty(e.GroupName) && e.MatchType == "Group")
            {
                System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Group-spezifische Suche: Match {e.MatchId} in '{e.GroupName}'");
                _hubDebugWindow?.AddDebugMessage($"🔍 Group-spezifische Suche: Match {e.MatchId} in '{e.GroupName}'", "SEARCH");
                
                // Suche die SPEZIFISCHE Gruppe
                targetGroup = tournamentClass.Groups
                    .FirstOrDefault(g => g.Name.Equals(e.GroupName, StringComparison.OrdinalIgnoreCase));
                
                if (targetGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"📋 [PLANNER] Found target group: {targetGroup.Name}");
                    _hubDebugWindow?.AddDebugMessage($"📋 Zielgruppe gefunden: {targetGroup.Name}", "SUCCESS");
                    
                    // Suche das Match NUR in der spezifischen Gruppe
                    targetMatch = targetGroup.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                    
                    if (targetMatch != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found match {e.MatchId} in specific group '{targetGroup.Name}'");
                        _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} in spezifischer Gruppe '{targetGroup.Name}' gefunden", "MATCH_RESULT");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Match {e.MatchId} not found in group '{targetGroup.Name}'");
                        _hubDebugWindow?.AddDebugMessage($"❌ Match {e.MatchId} nicht in Gruppe '{targetGroup.Name}' gefunden", "WARNING");
                        
                        var availableMatches = string.Join(", ", targetGroup.Matches.Select(m => m.Id));
                        System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Available matches in group: {availableMatches}");
                        _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Matches in Gruppe: {availableMatches}", "INFO");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Group '{e.GroupName}' not found in class {tournamentClass.Name}");
                    _hubDebugWindow?.AddDebugMessage($"❌ Gruppe '{e.GroupName}' nicht in Klasse {tournamentClass.Name} gefunden", "WARNING");
                    
                    var availableGroups = string.Join(", ", tournamentClass.Groups.Select(g => g.Name));
                    System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Available groups: {availableGroups}");
                    _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Gruppen: {availableGroups}", "INFO");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] No group info available - using fallback search in all groups");
                _hubDebugWindow?.AddDebugMessage($"🔍 Keine Group-Info - verwende Fallback-Suche in allen Gruppen", "SEARCH");
                
                // FALLBACK: Suche in allen Gruppen (alte Logik)
                foreach (var group in tournamentClass.Groups)
                {
                    targetMatch = group.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                    if (targetMatch != null)
                    {
                        targetGroup = group;
                        System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Found match {e.MatchId} in group '{group.Name}' via fallback search");
                        _hubDebugWindow?.AddDebugMessage($"⚠️ Match {e.MatchId} in Gruppe '{group.Name}' über Fallback-Suche gefunden", "WARNING");
                        break;
                    }
                }
            }

            // Suche in Finals falls nicht in Gruppen gefunden
            if (targetMatch == null && tournamentClass.CurrentPhase?.FinalsGroup != null)
            {
                System.Diagnostics.Debug.WriteLine($"🏆 [PLANNER] Searching in Finals for match {e.MatchId}");
                _hubDebugWindow?.AddDebugMessage($"🏆 Suche in Finals nach Match {e.MatchId}", "SEARCH");
                
                targetMatch = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                if (targetMatch != null)
                {
                    targetGroup = tournamentClass.CurrentPhase.FinalsGroup;
                    System.Diagnostics.Debug.WriteLine($"🏆 [PLANNER] Found match {e.MatchId} in Finals");
                    _hubDebugWindow?.AddDebugMessage($"🏆 Match {e.MatchId} in Finals gefunden", "SUCCESS");
                }
            }

            if (targetMatch == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Match {e.MatchId} not found in class {e.ClassId}");
                System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Available matches in class: {string.Join(", ", tournamentClass.Groups.SelectMany(g => g.Matches).Select(m => m.Id))}");
                
                _hubDebugWindow?.AddDebugMessage($"⚠️ Match {e.MatchId} nicht gefunden in Klasse {e.ClassId}", "WARNING");
                _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Matches: {string.Join(", ", tournamentClass.Groups.SelectMany(g => g.Matches).Select(m => m.Id))}", "INFO");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found target match: {targetMatch.Id} ({targetMatch.Player1?.Name} vs {targetMatch.Player2?.Name}) in group '{targetGroup?.Name}'");
            
            if (isMatchResult)
            {
                _hubDebugWindow?.AddDebugMessage($"✅ Ziel-Match gefunden: {targetMatch.Id} ({targetMatch.Player1?.Name} vs {targetMatch.Player2?.Name}) in Gruppe '{targetGroup?.Name}'", "MATCH_RESULT");
            }
            else
            {
                _hubDebugWindow?.AddDebugMessage($"✅ Ziel-Match gefunden: {targetMatch.Id} ({targetMatch.Player1?.Name} vs {targetMatch.Player2?.Name}) in Gruppe '{targetGroup?.Name}'", "SUCCESS");
            }

            // Aktualisiere das Match mit den Hub-Daten
            var wasUpdated = UpdateMatchWithHubData(targetMatch, e);
            
            if (wasUpdated)
            {
                System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Match {e.MatchId} updated successfully from Hub");
                
                // Triggere UI-Updates
                tournamentClass.TriggerUIRefresh();
                MarkAsChanged();
                
                // Zeige Benachrichtigung
                var playerNames = $"{targetMatch.Player1?.Name ?? "Player 1"} vs {targetMatch.Player2?.Name ?? "Player 2"}";
                var resultInfo = $"{e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs";
                var groupInfo = targetGroup?.Name != null ? $" in Gruppe '{targetGroup.Name}'" : "";
                
                System.Diagnostics.Debug.WriteLine($"🎯 [PLANNER] {playerNames}: {resultInfo}{groupInfo}");
                System.Diagnostics.Debug.WriteLine($"🔄 [PLANNER] UI refresh triggered for {tournamentClass.Name}");
                
                if (isMatchResult)
                {
                    _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} erfolgreich vom Hub aktualisiert{groupInfo}", "MATCH_RESULT");
                    _hubDebugWindow?.AddDebugMessage($"🎯 {playerNames}: {resultInfo}", "MATCH_RESULT");
                    _hubDebugWindow?.AddDebugMessage($"🔄 UI-Aktualisierung ausgelöst für {tournamentClass.Name}", "MATCH_RESULT");
                }
                else
                {
                    _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} erfolgreich vom Hub aktualisiert{groupInfo}", "SUCCESS");
                    _hubDebugWindow?.AddDebugMessage($"🎯 {playerNames}: {resultInfo}", "MATCH");
                    _hubDebugWindow?.AddDebugMessage($"🔄 UI-Aktualisierung ausgelöst für {tournamentClass.Name}", "SYNC");
                }
                
                // Optional: Zeige Toast-Benachrichtigung
                ShowToastNotification($"Match Update", $"{playerNames}{groupInfo}\n{resultInfo}", "Hub Update empfangen");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ℹ️ [PLANNER] No changes detected for match {e.MatchId}");
                _hubDebugWindow?.AddDebugMessage($"ℹ️ Keine Änderungen für Match {e.MatchId} erkannt", "INFO");
            }
            
            if (isMatchResult)
            {
                _hubDebugWindow?.AddDebugMessage("🏆 ===== MATCH-ERGEBNIS VERARBEITUNG ABGESCHLOSSEN =====", "MATCH_RESULT");
            }
            else
            {
                _hubDebugWindow?.AddDebugMessage("===== MATCH UPDATE ENDE =====", "MATCH");
            }
            
            System.Diagnostics.Debug.WriteLine("📥 [PLANNER] ===== END MATCH UPDATE =====");
            
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Error processing Hub match update: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Stack trace: {ex.StackTrace}");
            
            _hubDebugWindow?.AddDebugMessage($"❌ Fehler beim Verarbeiten des Hub Match Updates: {ex.Message}", "ERROR");
        }
    });
}