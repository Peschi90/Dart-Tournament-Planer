// 🚨 KORRIGIERTE VERSION der OnHubMatchResultReceived Funktion für MainWindow.xaml.cs
// Diese Version sucht auch in Winner Bracket und Loser Bracket nach KO-Matches

/// <summary>
/// Event-Handler für Match-Updates die vom Hub empfangen werden
/// 🚨 KORRIGIERT: Erweitert um KO-Match-Suche in Winner/Loser Brackets
/// </summary>
private void OnHubMatchResultReceived(HubMatchUpdateEventArgs e)
{
    Dispatcher.Invoke(() =>
    {
        try
        {
            // Erweiterte Debug-Ausgabe für Match Results
            var isMatchResult = e.Source?.Contains("match-result") == true;
            
            System.Diagnostics.Debug.WriteLine("📥 [PLANNER] ===== MATCH UPDATE FROM HUB =====");
            System.Diagnostics.Debug.WriteLine($"📥 [PLANNER] Received match update from Hub: Match {e.MatchId} in class {e.ClassId}");
            System.Diagnostics.Debug.WriteLine($"📊 [PLANNER] Result: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs, Status: {e.Status}");
            System.Diagnostics.Debug.WriteLine($"📋 [PLANNER] Group Info: GroupName='{e.GroupName}', GroupId={e.GroupId}, MatchType='{e.MatchType}'");
            
            // DEBUG CONSOLE LOGGING
            if (isMatchResult)
            {
                _hubDebugWindow?.AddDebugMessage("🏆 ===== MATCH-ERGEBNIS EMPFANGEN =====", "MATCH_RESULT");
                _hubDebugWindow?.AddDebugMessage($"📥 Match-Ergebnis: Match {e.MatchId} in Klasse {e.ClassId}", "MATCH_RESULT");
                _hubDebugWindow?.AddDebugMessage($"📊 Endergebnis: {e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs", "MATCH_RESULT");
                _hubDebugWindow?.AddDebugMessage($"📋 Group Info: '{e.GroupName}' (ID: {e.GroupId})", "MATCH_RESULT");
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

            // 🚨 ERWEITERT: Match-Suche in verschiedenen Match-Typen basierend auf GroupName
            Match? targetMatch = null;
            KnockoutMatch? targetKnockoutMatch = null;
            Group? targetGroup = null;
            string matchLocation = "Unknown";

            // 🚨 NEUE: Priorisierte Suche basierend auf GroupName-Pattern
            if (!string.IsNullOrEmpty(e.GroupName))
            {
                System.Diagnostics.Debug.WriteLine($"🎯 [PLANNER] GroupName-basierte Suche: '{e.GroupName}'");
                _hubDebugWindow?.AddDebugMessage($"🎯 GroupName-basierte Suche: '{e.GroupName}'", "SEARCH");
                
                // 1. WINNER BRACKET ERKENNUNG (höchste Priorität)
                if (e.GroupName.StartsWith("Winner Bracket", StringComparison.OrdinalIgnoreCase) && 
                    tournamentClass.CurrentPhase?.WinnerBracket != null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚡ [PLANNER] Suche in Winner Bracket: Match {e.MatchId}");
                    _hubDebugWindow?.AddDebugMessage($"⚡ Erkannt als Winner Bracket - suche Match {e.MatchId}", "SEARCH");
                    
                    targetKnockoutMatch = tournamentClass.CurrentPhase.WinnerBracket.FirstOrDefault(m => m.Id == e.MatchId);
                    if (targetKnockoutMatch != null)
                    {
                        matchLocation = $"Winner Bracket - {targetKnockoutMatch.Round}";
                        System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found Winner Bracket match: {e.MatchId} in {matchLocation}");
                        _hubDebugWindow?.AddDebugMessage($"✅ Winner Bracket Match gefunden: {e.MatchId} in {matchLocation}", "MATCH_RESULT");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Winner Bracket match {e.MatchId} not found");
                        _hubDebugWindow?.AddDebugMessage($"❌ Winner Bracket Match {e.MatchId} nicht gefunden", "WARNING");
                        
                        var availableWBMatches = string.Join(", ", tournamentClass.CurrentPhase.WinnerBracket.Select(m => m.Id));
                        System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Available Winner Bracket matches: {availableWBMatches}");
                        _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Winner Bracket Matches: {availableWBMatches}", "INFO");
                    }
                }
                // 2. LOSER BRACKET ERKENNUNG (zweite Priorität)  
                else if (e.GroupName.StartsWith("Loser Bracket", StringComparison.OrdinalIgnoreCase) && 
                         tournamentClass.CurrentPhase?.LoserBracket != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 [PLANNER] Suche in Loser Bracket: Match {e.MatchId}");
                    _hubDebugWindow?.AddDebugMessage($"🔄 Erkannt als Loser Bracket - suche Match {e.MatchId}", "SEARCH");
                    
                    targetKnockoutMatch = tournamentClass.CurrentPhase.LoserBracket.FirstOrDefault(m => m.Id == e.MatchId);
                    if (targetKnockoutMatch != null)
                    {
                        matchLocation = $"Loser Bracket - {targetKnockoutMatch.Round}";
                        System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found Loser Bracket match: {e.MatchId} in {matchLocation}");
                        _hubDebugWindow?.AddDebugMessage($"✅ Loser Bracket Match gefunden: {e.MatchId} in {matchLocation}", "MATCH_RESULT");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Loser Bracket match {e.MatchId} not found");
                        _hubDebugWindow?.AddDebugMessage($"❌ Loser Bracket Match {e.MatchId} nicht gefunden", "WARNING");
                        
                        var availableLBMatches = string.Join(", ", tournamentClass.CurrentPhase.LoserBracket.Select(m => m.Id));
                        System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Available Loser Bracket matches: {availableLBMatches}");
                        _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Loser Bracket Matches: {availableLBMatches}", "INFO");
                    }
                }
                // 3. FINALS ERKENNUNG (dritte Priorität)
                else if (e.GroupName.Equals("Finals", StringComparison.OrdinalIgnoreCase) && 
                         tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🏆 [PLANNER] Suche in Finals: Match {e.MatchId}");
                    _hubDebugWindow?.AddDebugMessage($"🏆 Erkannt als Finals - suche Match {e.MatchId}", "SEARCH");
                    
                    targetMatch = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                    if (targetMatch != null)
                    {
                        targetGroup = tournamentClass.CurrentPhase.FinalsGroup;
                        matchLocation = "Finals";
                        System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found Finals match: {e.MatchId}");
                        _hubDebugWindow?.AddDebugMessage($"✅ Finals Match gefunden: {e.MatchId}", "MATCH_RESULT");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [PLANNER] Finals match {e.MatchId} not found");
                        _hubDebugWindow?.AddDebugMessage($"❌ Finals Match {e.MatchId} nicht gefunden", "WARNING");
                    }
                }
                // 4. GROUP ERKENNUNG (niedrigste Priorität für bekannte GroupNames)
                else
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Suche in spezifischer Gruppe: '{e.GroupName}'");
                    _hubDebugWindow?.AddDebugMessage($"🔍 Suche in spezifischer Gruppe: '{e.GroupName}'", "SEARCH");
                    
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
                            matchLocation = $"Group - {targetGroup.Name}";
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
            }
            
            // 🚨 FALLBACK: Wenn kein Match gefunden wurde, durchsuche ALLE Bereiche
            if (targetMatch == null && targetKnockoutMatch == null)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Kein Match mit GroupName gefunden - verwende Fallback-Suche");
                _hubDebugWindow?.AddDebugMessage($"🔍 Keine GroupName-Treffer - verwende Fallback-Suche in ALLEN Bereichen", "SEARCH");
                
                // FALLBACK 1: Winner Bracket durchsuchen
                if (tournamentClass.CurrentPhase?.WinnerBracket != null)
                {
                    targetKnockoutMatch = tournamentClass.CurrentPhase.WinnerBracket.FirstOrDefault(m => m.Id == e.MatchId);
                    if (targetKnockoutMatch != null)
                    {
                        matchLocation = $"Winner Bracket - {targetKnockoutMatch.Round} (Fallback)";
                        System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found Winner Bracket match {e.MatchId} via fallback");
                        _hubDebugWindow?.AddDebugMessage($"✅ Winner Bracket Match {e.MatchId} über Fallback gefunden", "SUCCESS");
                    }
                }
                
                // FALLBACK 2: Loser Bracket durchsuchen
                if (targetKnockoutMatch == null && tournamentClass.CurrentPhase?.LoserBracket != null)
                {
                    targetKnockoutMatch = tournamentClass.CurrentPhase.LoserBracket.FirstOrDefault(m => m.Id == e.MatchId);
                    if (targetKnockoutMatch != null)
                    {
                        matchLocation = $"Loser Bracket - {targetKnockoutMatch.Round} (Fallback)";
                        System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found Loser Bracket match {e.MatchId} via fallback");
                        _hubDebugWindow?.AddDebugMessage($"✅ Loser Bracket Match {e.MatchId} über Fallback gefunden", "SUCCESS");
                    }
                }
                
                // FALLBACK 3: Finals durchsuchen
                if (targetMatch == null && tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    targetMatch = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                    if (targetMatch != null)
                    {
                        targetGroup = tournamentClass.CurrentPhase.FinalsGroup;
                        matchLocation = "Finals (Fallback)";
                        System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found match {e.MatchId} in Finals via fallback");
                        _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} in Finals über Fallback gefunden", "SUCCESS");
                    }
                }
                
                // FALLBACK 4: Alle Gruppen durchsuchen
                if (targetMatch == null)
                {
                    foreach (var group in tournamentClass.Groups)
                    {
                        targetMatch = group.Matches.FirstOrDefault(m => m.Id == e.MatchId);
                        if (targetMatch != null)
                        {
                            targetGroup = group;
                            matchLocation = $"Group - {group.Name} (Fallback)";
                            System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Found match {e.MatchId} in group '{group.Name}' via fallback search");
                            _hubDebugWindow?.AddDebugMessage($"⚠️ Match {e.MatchId} in Gruppe '{group.Name}' über Fallback-Suche gefunden", "WARNING");
                            break;
                        }
                    }
                }
            }

            // Prüfe ob ein Match gefunden wurde
            if (targetMatch == null && targetKnockoutMatch == null)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [PLANNER] Match {e.MatchId} nicht gefunden in Klasse {e.ClassId}");
                
                // Erweiterte Debug-Informationen über verfügbare Matches
                var groupMatchCount = tournamentClass.Groups.SelectMany(g => g.Matches).Count();
                var finalsMatchCount = tournamentClass.CurrentPhase?.FinalsGroup?.Matches.Count ?? 0;
                var winnerBracketCount = tournamentClass.CurrentPhase?.WinnerBracket?.Count ?? 0;
                var loserBracketCount = tournamentClass.CurrentPhase?.LoserBracket?.Count ?? 0;
                
                System.Diagnostics.Debug.WriteLine($"🔍 [PLANNER] Verfügbare Matches in {tournamentClass.Name}:");
                System.Diagnostics.Debug.WriteLine($"   - Groups: {groupMatchCount} Matches");
                System.Diagnostics.Debug.WriteLine($"   - Finals: {finalsMatchCount} Matches");
                System.Diagnostics.Debug.WriteLine($"   - Winner Bracket: {winnerBracketCount} Matches");
                System.Diagnostics.Debug.WriteLine($"   - Loser Bracket: {loserBracketCount} Matches");
                
                _hubDebugWindow?.AddDebugMessage($"⚠️ Match {e.MatchId} nicht gefunden in Klasse {e.ClassId}", "WARNING");
                _hubDebugWindow?.AddDebugMessage($"🔍 Verfügbare Matches: Groups={groupMatchCount}, Finals={finalsMatchCount}, WB={winnerBracketCount}, LB={loserBracketCount}", "INFO");
                return;
            }

            // Verarbeite gefundenes Match
            if (targetMatch != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found target match: {targetMatch.Id} ({targetMatch.Player1?.Name} vs {targetMatch.Player2?.Name}) in {matchLocation}");
                _hubDebugWindow?.AddDebugMessage($"✅ Ziel-Match gefunden: {targetMatch.Id} in {matchLocation}", "MATCH_RESULT");

                // Aktualisiere das Group-Match mit den Hub-Daten
                var wasUpdated = UpdateMatchWithHubData(targetMatch, e);
                
                if (wasUpdated)
                {
                    ProcessMatchUpdateSuccess(targetMatch.Player1?.Name, targetMatch.Player2?.Name, e, matchLocation, tournamentClass, isMatchResult);
                }
            }
            else if (targetKnockoutMatch != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Found target knockout match: {targetKnockoutMatch.Id} ({targetKnockoutMatch.Player1?.Name} vs {targetKnockoutMatch.Player2?.Name}) in {matchLocation}");
                _hubDebugWindow?.AddDebugMessage($"✅ Ziel-KO-Match gefunden: {targetKnockoutMatch.Id} in {matchLocation}", "MATCH_RESULT");

                // Aktualisiere das KnockoutMatch mit den Hub-Daten
                var wasUpdated = UpdateKnockoutMatchWithHubData(targetKnockoutMatch, e);
                
                if (wasUpdated)
                {
                    ProcessMatchUpdateSuccess(targetKnockoutMatch.Player1?.Name, targetKnockoutMatch.Player2?.Name, e, matchLocation, tournamentClass, isMatchResult);
                }
            }
            
            if (isMatchResult)
            {
                _hubDebugWindow?.AddDebugMessage("🏆 ===== MATCH-ERGEBNIS VERARBEITUNG ABGESCHLOSSEN =====", "MATCH_RESULT");
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

/// <summary>
/// 🚨 NEUE HILFSMETHODE: Aktualisiert ein KnockoutMatch mit Daten vom Hub
/// </summary>
private bool UpdateKnockoutMatchWithHubData(KnockoutMatch knockoutMatch, HubMatchUpdateEventArgs hubData)
{
    try
    {
        System.Diagnostics.Debug.WriteLine($"🔧 UpdateKnockoutMatchWithHubData called for Match {hubData.MatchId}");
        System.Diagnostics.Debug.WriteLine($"   Current: {knockoutMatch.Player1Sets}-{knockoutMatch.Player2Sets} Sets, {knockoutMatch.Player1Legs}-{knockoutMatch.Player2Legs} Legs, Status: {knockoutMatch.Status}");
        System.Diagnostics.Debug.WriteLine($"   Hub Data: {hubData.Player1Sets}-{hubData.Player2Sets} Sets, {hubData.Player1Legs}-{hubData.Player2Legs} Legs, Status: {hubData.Status}");
        
        // Prüfe ob es tatsächlich Änderungen gibt
        if (knockoutMatch.Player1Sets == hubData.Player1Sets &&
            knockoutMatch.Player2Sets == hubData.Player2Sets &&
            knockoutMatch.Player1Legs == hubData.Player1Legs &&
            knockoutMatch.Player2Legs == hubData.Player2Legs &&
            knockoutMatch.Status.ToString() == hubData.Status)
        {
            System.Diagnostics.Debug.WriteLine($"   No changes detected, skipping update");
            return false; // Keine Änderungen
        }

        // Aktualisiere KnockoutMatch-Daten
        knockoutMatch.SetResult(hubData.Player1Sets, hubData.Player2Sets, 
                               hubData.Player1Legs, hubData.Player2Legs);
        knockoutMatch.Notes = hubData.Notes ?? knockoutMatch.Notes;

        // Aktualisiere Status
        if (Enum.TryParse<MatchStatus>(hubData.Status, out var newStatus))
        {
            knockoutMatch.Status = newStatus;
            System.Diagnostics.Debug.WriteLine($"   Status updated to: {newStatus}");
        }

        // Setze End-Zeit wenn abgeschlossen
        if (knockoutMatch.Status == MatchStatus.Finished && knockoutMatch.EndTime == null)
        {
            knockoutMatch.EndTime = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"   End time set to: {knockoutMatch.EndTime}");
        }
        
        System.Diagnostics.Debug.WriteLine($"   Winner: {knockoutMatch.Winner?.Name ?? "None"}");
        System.Diagnostics.Debug.WriteLine($"   Loser: {knockoutMatch.Loser?.Name ?? "None"}");
        System.Diagnostics.Debug.WriteLine($"   ✅ Knockout match updated successfully");

        return true;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"❌ Error updating knockout match with hub data: {ex.Message}");
        return false;
    }
}

/// <summary>
/// 🚨 HILFSMETHODE: Verarbeitet erfolgreiche Match-Updates (für Code-Wiederverwendung)
/// </summary>
private void ProcessMatchUpdateSuccess(string? player1Name, string? player2Name, HubMatchUpdateEventArgs e, string matchLocation, TournamentClass tournamentClass, bool isMatchResult)
{
    System.Diagnostics.Debug.WriteLine($"✅ [PLANNER] Match {e.MatchId} updated successfully from Hub");
    
    // Triggere UI-Updates
    tournamentClass.TriggerUIRefresh();
    MarkAsChanged();
    
    // Zeige Benachrichtigung
    var playerNames = $"{player1Name ?? "Player 1"} vs {player2Name ?? "Player 2"}";
    var resultInfo = $"{e.Player1Sets}-{e.Player2Sets} Sets, {e.Player1Legs}-{e.Player2Legs} Legs";
    var locationInfo = $" in {matchLocation}";
    
    System.Diagnostics.Debug.WriteLine($"🎯 [PLANNER] {playerNames}: {resultInfo}{locationInfo}");
    System.Diagnostics.Debug.WriteLine($"🔄 [PLANNER] UI refresh triggered for {tournamentClass.Name}");
    
    if (isMatchResult)
    {
        _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} erfolgreich vom Hub aktualisiert{locationInfo}", "MATCH_RESULT");
        _hubDebugWindow?.AddDebugMessage($"🎯 {playerNames}: {resultInfo}", "MATCH_RESULT");
        _hubDebugWindow?.AddDebugMessage($"🔄 UI-Aktualisierung ausgelöst für {tournamentClass.Name}", "MATCH_RESULT");
    }
    else
    {
        _hubDebugWindow?.AddDebugMessage($"✅ Match {e.MatchId} erfolgreich vom Hub aktualisiert{locationInfo}", "SUCCESS");
        _hubDebugWindow?.AddDebugMessage($"🎯 {playerNames}: {resultInfo}", "MATCH");
        _hubDebugWindow?.AddDebugMessage($"🔄 UI-Aktualisierung ausgelöst für {tournamentClass.Name}", "SYNC");
    }
    
    // Optional: Zeige Toast-Benachrichtigung
    ShowToastNotification($"Match Update", $"{playerNames}{locationInfo}\n{resultInfo}", "Hub Update empfangen");
}