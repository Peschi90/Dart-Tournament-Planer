using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DartTournamentPlaner.Models.Statistics;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Services.Statistics;

/// <summary>
/// Verwaltet alle Spieler-Statistiken für eine Turnierklasse
/// Extrahiert und speichert Dart-Statistiken aus WebSocket-Nachrichten
/// </summary>
public class PlayerStatisticsManager : INotifyPropertyChanged
{
    private readonly Dictionary<string, PlayerStatistics> _playerStatistics;
    private readonly string _tournamentClassName;

    /// <summary>
    /// ✅ NEUER Konstruktor: Arbeitet mit externer Dictionary-Referenz
    /// </summary>
    /// <param name="tournamentClassName">Name der Turnierklasse</param>
    /// <param name="playerStatisticsData">Direkte Referenz auf TournamentClass.PlayerStatisticsData</param>
    public PlayerStatisticsManager(string tournamentClassName, Dictionary<string, PlayerStatistics> playerStatisticsData)
    {
        _tournamentClassName = tournamentClassName;
        _playerStatistics = playerStatisticsData; // ✅ DIREKTE REFERENZ statt neues Dictionary
        
        System.Diagnostics.Debug.WriteLine($"[STATS-MANAGER] Created for {tournamentClassName} with {_playerStatistics.Count} existing players");
    }

    /// <summary>
    /// ✅ VERALTETER Konstruktor für Rückwärts-Kompatibilität
    /// </summary>
    [Obsolete("Use constructor with playerStatisticsData parameter for direct data reference")]
    public PlayerStatisticsManager(string tournamentClassName)
    {
        _tournamentClassName = tournamentClassName;
        _playerStatistics = new Dictionary<string, PlayerStatistics>(); // Fallback
        
        System.Diagnostics.Debug.WriteLine($"[STATS-MANAGER] Created for {tournamentClassName} with new dictionary (legacy mode)");
    }

    /// <summary>
    /// Alle Spieler-Statistiken dieser Klasse
    /// </summary>
    public Dictionary<string, PlayerStatistics> PlayerStatistics => _playerStatistics;

    /// <summary>
    /// ✅ ERWEITERT: Extrahiert umfangreiche Dart-Statistiken aus WebSocket-Nachrichten
    /// Verarbeitet nun auch direkte statistics-Daten aus der WebSocket-Nachricht UND dartScoringResult-Daten und playerStatistics für detaillierte Statistiken
    /// </summary>
    public void ProcessWebSocketMatchResult(HubMatchUpdateEventArgs matchUpdate)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Processing match update for class {_tournamentClassName}");

            if (matchUpdate.Source != "hub-match-result" && matchUpdate.Source != "websocket-direct")
            {
                System.Diagnostics.Debug.WriteLine($"[STATS] Skipping non-match-result update: {matchUpdate.Source}");
                return;
            }

            // ✅ PRIORITÄT 1: Versuche erweiterte dartScoringResult-Daten zu extrahieren (enthält legData!)
            var enhancedStats = ExtractEnhancedDartStatistics(matchUpdate);
            if (enhancedStats != null)
            {
                System.Diagnostics.Debug.WriteLine($"[STATS] Processing enhanced dart statistics (with legData) for {enhancedStats.Player1Name} vs {enhancedStats.Player2Name}");
                ProcessEnhancedStatistics(matchUpdate, enhancedStats);
                return;
            }

            // ✅ PRIORITÄT 2: Versuche direkte WebSocket statistics-Extraktion
            var directStats = ExtractDirectWebSocketStatistics(matchUpdate);
            if (directStats != null)
            {
                System.Diagnostics.Debug.WriteLine($"[STATS] Processing direct WebSocket statistics for {directStats.Player1Name} vs {directStats.Player2Name}");
                ProcessSimpleStatistics(matchUpdate, directStats);
                return;
            }

            // ✅ PRIORITÄT 3: Versuche top-level statistics-Struktur zu extrahieren
            var topLevelStats = ExtractTopLevelPlayerStatistics(matchUpdate);
            if (topLevelStats != null)
            {
                System.Diagnostics.Debug.WriteLine($"[STATS] Processing top-level player statistics for {topLevelStats.Player1Name} vs {topLevelStats.Player2Name}");
                ProcessSimpleStatistics(matchUpdate, topLevelStats);
                return;
            }

            // ✅ PRIORITÄT 4: Versuche playerStatistics-Struktur zu extrahieren
            var simpleStats = ExtractSimplePlayerStatistics(matchUpdate);
            if (simpleStats != null)
            {
                System.Diagnostics.Debug.WriteLine($"[STATS] Processing simple player statistics for {simpleStats.Player1Name} vs {simpleStats.Player2Name}");
                ProcessSimpleStatistics(matchUpdate, simpleStats);
                return;
            }

            // ✅ FALLBACK: Verwende alte Notes-basierte Extraktion
            System.Diagnostics.Debug.WriteLine("[STATS] Falling back to notes-based statistics extraction");
            ProcessLegacyNotesStatistics(matchUpdate);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error processing match result: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEU: Extrahiert Statistiken direkt aus der WebSocket-Nachricht (statistics property)
    /// Diese Methode verarbeitet die statistics-Sektion direkt aus der WebSocket-Nachricht, nicht aus Notes
    /// </summary>
    private SimplePlayerStatistics? ExtractDirectWebSocketStatistics(HubMatchUpdateEventArgs matchUpdate)
    {
        try
        {
            // ✅ NEU: Parse die komplette WebSocket-Nachricht aus Notes
            if (string.IsNullOrEmpty(matchUpdate.Notes) || !matchUpdate.Notes.TrimStart().StartsWith("{"))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No valid JSON data found in WebSocket message");
                return null;
            }

            var jsonData = JsonDocument.Parse(matchUpdate.Notes);
            var root = jsonData.RootElement;
            
            System.Diagnostics.Debug.WriteLine("[STATS] Processing direct WebSocket statistics");

            // ✅ Suche nach statistics Property auf Root-Level
            if (!root.TryGetProperty("statistics", out var statistics))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No direct statistics found in WebSocket message");
                return null;
            }

            System.Diagnostics.Debug.WriteLine("[STATS] Found direct statistics in WebSocket message");

            var simpleStats = new SimplePlayerStatistics();
            bool hasValidData = false;

            // ✅ Extrahiere Player1 Daten
            if (statistics.TryGetProperty("player1", out var player1Data))
            {
                simpleStats.Player1Stats = ParseSimplePlayerData(player1Data);
                
                if (IsValidPlayerData(simpleStats.Player1Stats))
                {
                    hasValidData = true;
                    System.Diagnostics.Debug.WriteLine($"[STATS] Direct WebSocket Player1: {simpleStats.Player1Stats.Name} - Avg {simpleStats.Player1Stats.Average}, 180s: {simpleStats.Player1Stats.Scores180}, HF: {simpleStats.Player1Stats.HighFinishes}, 26s: {simpleStats.Player1Stats.Scores26}, Checkouts: {simpleStats.Player1Stats.Checkouts}");
                }
            }

            // ✅ Extrahiere Player2 Daten  
            if (statistics.TryGetProperty("player2", out var player2Data))
            {
                simpleStats.Player2Stats = ParseSimplePlayerData(player2Data);
                
                if (IsValidPlayerData(simpleStats.Player2Stats))
                {
                    hasValidData = true;
                    System.Diagnostics.Debug.WriteLine($"[STATS] Direct WebSocket Player2: {simpleStats.Player2Stats.Name} - Avg {simpleStats.Player2Stats.Average}, 180s: {simpleStats.Player2Stats.Scores180}, HF: {simpleStats.Player2Stats.HighFinishes}, 26s: {simpleStats.Player2Stats.Scores26}, Checkouts: {simpleStats.Player2Stats.Checkouts}");
                }
            }

            if (!hasValidData)
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No valid direct WebSocket statistics data found");
                return null;
            }

            // ✅ Extrahiere Match-Metadaten
            if (statistics.TryGetProperty("match", out var matchData))
            {
                // Duration als Millisekunden
                if (matchData.TryGetProperty("duration", out var duration))
                {
                    if (duration.ValueKind == JsonValueKind.Number)
                    {
                        var milliseconds = duration.GetInt64();
                        simpleStats.MatchDuration = TimeSpan.FromMilliseconds(milliseconds);
                        simpleStats.MatchDurationString = FormatDuration(simpleStats.MatchDuration);
                        System.Diagnostics.Debug.WriteLine($"[STATS] Direct WebSocket match duration: {milliseconds}ms = {simpleStats.MatchDurationString}");
                    }
                }
                
                if (matchData.TryGetProperty("format", out var format))
                {
                    simpleStats.MatchFormat = format.GetString() ?? "Unknown";
                }

                if (matchData.TryGetProperty("startTime", out var startTime))
                {
                    if (DateTime.TryParse(startTime.GetString(), out var parsedStartTime))
                    {
                        simpleStats.StartTime = parsedStartTime;
                    }
                }

                if (matchData.TryGetProperty("endTime", out var endTime))
                {
                    if (DateTime.TryParse(endTime.GetString(), out var parsedEndTime))
                    {
                        simpleStats.EndTime = parsedEndTime;
                    }
                }

                if (matchData.TryGetProperty("totalThrows", out var totalThrows))
                {
                    simpleStats.TotalThrows = totalThrows.GetInt32();
                }
            }

            // ✅ Spielernamen aus matchUpdate.result extrahieren
            if (root.TryGetProperty("matchUpdate", out var matchUpdate_El) && 
                matchUpdate_El.TryGetProperty("result", out var resultEl))
            {
                if (resultEl.TryGetProperty("player1Name", out var player1Name))
                {
                    simpleStats.Player1Name = player1Name.GetString() ?? simpleStats.Player1Stats.Name;
                }

                if (resultEl.TryGetProperty("player2Name", out var player2Name))
                {
                    simpleStats.Player2Name = player2Name.GetString() ?? simpleStats.Player2Stats.Name;
                }
            }

            // Fallback auf Namen aus statistics falls nicht in result gefunden
            if (string.IsNullOrEmpty(simpleStats.Player1Name))
            {
                simpleStats.Player1Name = simpleStats.Player1Stats.Name;
            }
            if (string.IsNullOrEmpty(simpleStats.Player2Name))
            {
                simpleStats.Player2Name = simpleStats.Player2Stats.Name;
            }

            // Bestimme Gewinner
            DetermineWinnerForSimpleStats(matchUpdate, simpleStats);

            System.Diagnostics.Debug.WriteLine($"[STATS] Direct WebSocket extraction successful: {simpleStats.Player1Name} vs {simpleStats.Player2Name}");
            return simpleStats;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error parsing direct WebSocket statistics: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// ✅ NEU: Extrahiert Statistiken aus der neuen top-level "statistics" Struktur
    /// Verarbeitet die Struktur mit statistics.player1/player2 und matchUpdate.result.player1Name/player2Name
    /// </summary>
    private SimplePlayerStatistics? ExtractTopLevelPlayerStatistics(HubMatchUpdateEventArgs matchUpdate)
    {
        try
        {
            JsonElement result;
            
            if (!string.IsNullOrEmpty(matchUpdate.Notes) && matchUpdate.Notes.TrimStart().StartsWith("{"))
            {
                var jsonData = JsonDocument.Parse(matchUpdate.Notes);
                result = jsonData.RootElement;
                System.Diagnostics.Debug.WriteLine("[STATS] Processing JSON from Notes field for top-level statistics");
                
                // ✅ NEU: Debug-Ausgabe der verfügbaren Properties
                if (result.ValueKind == JsonValueKind.Object)
                {
                    var properties = new List<string>();
                    foreach (var property in result.EnumerateObject())
                    {
                        properties.Add(property.Name);
                    }
                    System.Diagnostics.Debug.WriteLine($"[STATS] Available top-level properties: {string.Join(", ", properties)}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No JSON data found in Notes for top-level statistics");
                return null;
            }

            // Suche nach top-level statistics Property
            if (!result.TryGetProperty("statistics", out var statistics))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No top-level statistics found in JSON structure");
                return null;
            }

            System.Diagnostics.Debug.WriteLine("[STATS] Found top-level statistics in JSON");

            var simpleStats = new SimplePlayerStatistics();
            bool hasValidData = false;

            // Extrahiere Player1 Daten
            if (statistics.TryGetProperty("player1", out var player1Data))
            {
                simpleStats.Player1Stats = ParseSimplePlayerData(player1Data);
                
                // ✅ NEU: Prüfe ob die Daten gültig sind (nicht alle null/0)
                if (IsValidPlayerData(simpleStats.Player1Stats))
                {
                    hasValidData = true;
                    System.Diagnostics.Debug.WriteLine($"[STATS] Extracted valid Player1: Avg {simpleStats.Player1Stats.Average}, 180s: {simpleStats.Player1Stats.Scores180}, HF: {simpleStats.Player1Stats.HighFinishes}, 26s: {simpleStats.Player1Stats.Scores26}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[STATS] Player1 data is empty/null: Avg {simpleStats.Player1Stats.Average}, 180s: {simpleStats.Player1Stats.Scores180}, HF: {simpleStats.Player1Stats.HighFinishes}, 26s: {simpleStats.Player1Stats.Scores26}");
                }
            }

            // Extrahiere Player2 Daten  
            if (statistics.TryGetProperty("player2", out var player2Data))
            {
                simpleStats.Player2Stats = ParseSimplePlayerData(player2Data);
                
                // ✅ NEU: Prüfe ob die Daten gültig sind (nicht alle null/0)
                if (IsValidPlayerData(simpleStats.Player2Stats))
                {
                    hasValidData = true;
                    System.Diagnostics.Debug.WriteLine($"[STATS] Extracted valid Player2: Avg {simpleStats.Player2Stats.Average}, 180s: {simpleStats.Player2Stats.Scores180}, HF: {simpleStats.Player2Stats.HighFinishes}, 26s: {simpleStats.Player2Stats.Scores26}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[STATS] Player2 data is empty/null: Avg {simpleStats.Player2Stats.Average}, 180s: {simpleStats.Player2Stats.Scores180}, HF: {simpleStats.Player2Stats.HighFinishes}, 26s: {simpleStats.Player2Stats.Scores26}");
                }
            }

            // ✅ NEU: Wenn keine gültigen Statistik-Daten vorhanden sind, gib null zurück für Fallback
            if (!hasValidData)
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No valid statistics data found in top-level structure, will fallback to notes parsing");
                return null;
            }

            // Extrahiere Match-Metadaten
            if (statistics.TryGetProperty("match", out var matchData))
            {
                // ✅ NEU: Duration als Millisekunden
                if (matchData.TryGetProperty("duration", out var duration))
                {
                    if (duration.ValueKind == JsonValueKind.Number)
                    {
                        var milliseconds = duration.GetInt64();
                        simpleStats.MatchDuration = TimeSpan.FromMilliseconds(milliseconds);
                        simpleStats.MatchDurationString = FormatDuration(simpleStats.MatchDuration);
                        System.Diagnostics.Debug.WriteLine($"[STATS] Match duration: {milliseconds}ms = {simpleStats.MatchDurationString}");
                    }
                    else if (duration.ValueKind == JsonValueKind.String)
                    {
                        var durationStr = duration.GetString() ?? "0 min";
                        simpleStats.MatchDurationString = durationStr;
                        System.Diagnostics.Debug.WriteLine($"[STATS] Match duration string: {durationStr}");
                        
                        // Versuche Duration zu parsen (z.B. "0 minutes", "15 min")
                        var match = System.Text.RegularExpressions.Regex.Match(durationStr, @"(\d+)\s*(min|minutes?)");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int minutes))
                        {
                            simpleStats.MatchDuration = TimeSpan.FromMinutes(minutes);
                            System.Diagnostics.Debug.WriteLine($"[STATS] Parsed duration: {minutes} minutes");
                        }
                    }
                }
                
                if (matchData.TryGetProperty("format", out var format))
                {
                    simpleStats.MatchFormat = format.GetString() ?? "Unknown";
                    System.Diagnostics.Debug.WriteLine($"[STATS] Match format: {simpleStats.MatchFormat}");
                }

                // ✅ NEU: Start- und End-Zeit
                if (matchData.TryGetProperty("startTime", out var startTime))
                {
                    if (DateTime.TryParse(startTime.GetString(), out var parsedStartTime))
                    {
                        simpleStats.StartTime = parsedStartTime;
                        System.Diagnostics.Debug.WriteLine($"[STATS] Start time: {simpleStats.StartTime}");
                    }
                }

                if (matchData.TryGetProperty("endTime", out var endTime))
                {
                    if (DateTime.TryParse(endTime.GetString(), out var parsedEndTime))
                    {
                        simpleStats.EndTime = parsedEndTime;
                        System.Diagnostics.Debug.WriteLine($"[STATS] End time: {simpleStats.EndTime}");
                    }
                }

                // ✅ NEU: Total Throws
                if (matchData.TryGetProperty("totalThrows", out var totalThrows))
                {
                    simpleStats.TotalThrows = totalThrows.GetInt32();
                    System.Diagnostics.Debug.WriteLine($"[STATS] Total throws: {simpleStats.TotalThrows}");
                }
            }

            // ✅ NEU: Extrahiere Spielernamen aus matchUpdate.result
            if (result.TryGetProperty("matchUpdate", out var matchUpdate_El) && 
                matchUpdate_El.TryGetProperty("result", out var resultEl))
            {
                if (resultEl.TryGetProperty("player1Name", out var player1Name))
                {
                    simpleStats.Player1Name = player1Name.GetString() ?? "Player 1";
                    System.Diagnostics.Debug.WriteLine($"[STATS] Found player1Name from matchUpdate.result: {simpleStats.Player1Name}");
                }

                if (resultEl.TryGetProperty("player2Name", out var player2Name))
                {
                    simpleStats.Player2Name = player2Name.GetString() ?? "Player 2";
                    System.Diagnostics.Debug.WriteLine($"[STATS] Found player2Name from matchUpdate.result: {simpleStats.Player2Name}");
                }
            }

            // Fallback: Versuche Spielernamen aus verschiedenen Pfaden zu extrahieren
            if (string.IsNullOrEmpty(simpleStats.Player1Name) || string.IsNullOrEmpty(simpleStats.Player2Name))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] Using fallback player name extraction");
                simpleStats.Player1Name = GetPlayerNameFromMatch(matchUpdate, 1);
                simpleStats.Player2Name = GetPlayerNameFromMatch(matchUpdate, 2);
                System.Diagnostics.Debug.WriteLine($"[STATS] Fallback player names: {simpleStats.Player1Name}, {simpleStats.Player2Name}");
            }

            // Bestimme Gewinner basierend auf Match-Update Ergebnis
            DetermineWinnerForSimpleStats(matchUpdate, simpleStats);

            System.Diagnostics.Debug.WriteLine($"[STATS] Final extracted statistics with valid data: {simpleStats.Player1Name} vs {simpleStats.Player2Name}");
            return simpleStats;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error parsing top-level player statistics: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// ✅ NEU: Prüft ob PlayerData gültige (nicht leere/null) Statistiken enthält mit neuen Feldern
    /// </summary>
    private bool IsValidPlayerData(SimplePlayerData playerData)
    {
        // Prüfe ob mindestens ein Wert gesetzt ist (nicht 0 oder null)
        return playerData.Average > 0 ||
               playerData.Scores180 > 0 ||
               playerData.HighFinishes > 0 ||
               playerData.Scores26 > 0 ||
               playerData.Checkouts > 0 ||  // ✅ NEU
               playerData.TotalThrows > 0 || // ✅ NEU
               playerData.TotalScore > 0 ||  // ✅ NEU
               (playerData.HighFinishScores?.Count > 0) ||
               (playerData.HighFinishDetails?.Count > 0); // ✅ NEU
    }

    /// <summary>
    /// ✅ NEU: Parst einfache Spieler-Statistiken aus JSON mit erweiterten Daten
    /// Unterstützt nun auch detaillierte highFinishScores, totalThrows, totalScore und checkouts
    /// </summary>
    private SimplePlayerData ParseSimplePlayerData(JsonElement playerData)
    {
        var data = new SimplePlayerData();

        // Name (falls verfügbar)
        if (playerData.TryGetProperty("name", out var name))
        {
            data.Name = name.GetString() ?? "";
        }

        // Average
        if (playerData.TryGetProperty("average", out var avg))
        {
            if (avg.ValueKind == JsonValueKind.Number)
            {
                data.Average = avg.GetDouble();
            }
            else if (avg.ValueKind == JsonValueKind.String)
            {
                if (double.TryParse(avg.GetString(), System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out double avgValue))
                {
                    data.Average = avgValue;
                }
            }
            else if (avg.ValueKind == JsonValueKind.Null)
            {
                data.Average = 0.0; // Handle null values
            }
        }

        // 180er Scores
        if (playerData.TryGetProperty("scores180", out var scores180))
        {
            data.Scores180 = scores180.GetInt32();
        }

        // High Finishes Anzahl
        if (playerData.TryGetProperty("highFinishes", out var highFinishes))
        {
            data.HighFinishes = highFinishes.GetInt32();
        }

        // ✅ NEU: High Finish Scores Array mit Details
        if (playerData.TryGetProperty("highFinishScores", out var highFinishScores) && 
            highFinishScores.ValueKind == JsonValueKind.Array)
        {
            foreach (var highFinishElement in highFinishScores.EnumerateArray())
            {
                var highFinishDetail = new HighFinishDetail();
                
                if (highFinishElement.TryGetProperty("finish", out var finish))
                    highFinishDetail.Finish = finish.GetInt32();
                
                if (highFinishElement.TryGetProperty("remainingScore", out var remaining))
                    highFinishDetail.RemainingScore = remaining.GetInt32();
                
                if (highFinishElement.TryGetProperty("timestamp", out var timestamp))
                {
                    if (DateTime.TryParse(timestamp.GetString(), out var parsedTime))
                        highFinishDetail.Timestamp = parsedTime;
                    else
                        highFinishDetail.Timestamp = DateTime.Now;
                }

                if (highFinishElement.TryGetProperty("darts", out var darts) && darts.ValueKind == JsonValueKind.Array)
                {
                    highFinishDetail.Darts = darts.EnumerateArray().Select(d => d.GetInt32()).ToList();
                }

                data.HighFinishDetails.Add(highFinishDetail);
                
                // Auch in die einfache Liste für Rückwärtskompatibilität
                data.HighFinishScores.Add(highFinishDetail.Finish);
            }
            
            System.Diagnostics.Debug.WriteLine($"[STATS] Extracted {data.HighFinishDetails.Count} high finish details: [{string.Join(", ", data.HighFinishScores)}]");
        }

        // 26er Scores Anzahl
        if (playerData.TryGetProperty("scores26", out var scores26))
        {
            data.Scores26 = scores26.GetInt32();
        }

        // ✅ NEU: Checkouts Anzahl
        if (playerData.TryGetProperty("checkouts", out var checkouts))
        {
            data.Checkouts = checkouts.GetInt32();
        }

        // ✅ NEU: Total Throws
        if (playerData.TryGetProperty("totalThrows", out var totalThrows))
        {
            data.TotalThrows = totalThrows.GetInt32();
        }

        // ✅ NEU: Total Score
        if (playerData.TryGetProperty("totalScore", out var totalScore))
        {
            data.TotalScore = totalScore.GetInt32();
        }

        return data;
    }

    /// <summary>
    /// ✅ NEU: Extrahiert einfache playerStatistics aus WebSocket-Nachrichten (legacy Notes-basiert mit neuen Spielernamen)
    /// Verarbeitet sowohl die alte Struktur mit player1/player2 als auch neue Spielernamen-Extraktion
    /// </summary>
    private SimplePlayerStatistics? ExtractSimplePlayerStatistics(HubMatchUpdateEventArgs matchUpdate)
    {
        try
        {
            JsonElement result;
            
            if (!string.IsNullOrEmpty(matchUpdate.Notes) && matchUpdate.Notes.TrimStart().StartsWith("{"))
            {
                var jsonData = JsonDocument.Parse(matchUpdate.Notes);
                result = jsonData.RootElement;
                System.Diagnostics.Debug.WriteLine("[STATS] Processing JSON from Notes field for simple player statistics");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No JSON data found in Notes for simple player statistics");
                return null;
            }

            // Suche nach playerStatistics Property
            if (!result.TryGetProperty("playerStatistics", out var playerStats))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No playerStatistics found in JSON structure");
                return null;
            }

            System.Diagnostics.Debug.WriteLine("[STATS] Found playerStatistics in JSON");

            var simpleStats = new SimplePlayerStatistics();

            // Extrahiere Player1 Daten
            if (playerStats.TryGetProperty("player1", out var player1Data))
            {
                simpleStats.Player1Stats = ParseSimplePlayerData(player1Data);
                System.Diagnostics.Debug.WriteLine($"[STATS] Extracted Player1: Avg {simpleStats.Player1Stats.Average}, 180s: {simpleStats.Player1Stats.Scores180}, HF: {simpleStats.Player1Stats.HighFinishes}");
            }

            // Extrahiere Player2 Daten  
            if (playerStats.TryGetProperty("player2", out var player2Data))
            {
                simpleStats.Player2Stats = ParseSimplePlayerData(player2Data);
                System.Diagnostics.Debug.WriteLine($"[STATS] Extracted Player2: Avg {simpleStats.Player2Stats.Average}, 180s: {simpleStats.Player2Stats.Scores180}, HF: {simpleStats.Player2Stats.HighFinishes}");
            }

            // Extrahiere Match-Metadaten
            if (playerStats.TryGetProperty("match", out var matchData))
            {
                if (matchData.TryGetProperty("duration", out var duration))
                {
                    var durationStr = duration.GetString() ?? "0 min";
                    simpleStats.MatchDurationString = durationStr;
                    
                    // Versuche Duration zu parsen (z.B. "15 min")
                    var match = System.Text.RegularExpressions.Regex.Match(durationStr, @"(\d+)\s*(min|minutes?)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int minutes))
                    {
                        simpleStats.MatchDuration = TimeSpan.FromMinutes(minutes);
                        System.Diagnostics.Debug.WriteLine($"[STATS] Parsed duration: {minutes} minutes");
                    }
                }
                
                if (matchData.TryGetProperty("format", out var format))
                {
                    simpleStats.MatchFormat = format.GetString() ?? "Unknown";
                }
            }

            // ✅ ERWEITERT: Versuche zuerst Spielernamen aus der neuen matchUpdate.result Struktur zu extrahieren
            if (result.TryGetProperty("matchUpdate", out var matchUpdate_El) && 
                matchUpdate_El.TryGetProperty("result", out var resultEl))
            {
                if (resultEl.TryGetProperty("player1Name", out var player1Name))
                {
                    simpleStats.Player1Name = player1Name.GetString() ?? "Player 1";
                    System.Diagnostics.Debug.WriteLine($"[STATS] Found player1Name from matchUpdate.result: {simpleStats.Player1Name}");
                }

                if (resultEl.TryGetProperty("player2Name", out var player2Name))
                {
                    simpleStats.Player2Name = player2Name.GetString() ?? "Player 2";
                    System.Diagnostics.Debug.WriteLine($"[STATS] Found player2Name from matchUpdate.result: {simpleStats.Player2Name}");
                }
            }

            // Bestimme Spielernamen aus Match-Update (Fallback)
            if (string.IsNullOrEmpty(simpleStats.Player1Name) || string.IsNullOrEmpty(simpleStats.Player2Name))
            {
                simpleStats.Player1Name = GetPlayerNameFromMatch(matchUpdate, 1);
                simpleStats.Player2Name = GetPlayerNameFromMatch(matchUpdate, 2);
                System.Diagnostics.Debug.WriteLine($"[STATS] Using fallback player name extraction: {simpleStats.Player1Name}, {simpleStats.Player2Name}");
            }

            // Bestimme Gewinner basierend auf Match-Update Ergebnis
            DetermineWinnerForSimpleStats(matchUpdate, simpleStats);

            return simpleStats;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error parsing simple player statistics: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// ✅ NEU: Extrahiert erweiterte Dart-Statistiken direkt aus dartScoringResult mit neuen Daten
    /// </summary>
    private EnhancedDartStatistics? ExtractEnhancedDartStatistics(HubMatchUpdateEventArgs matchUpdate)
    {
        try
        {
            // ✅ VERBESSERT: Unterstütze sowohl JSON-String in Notes als auch direkte JSON-Struktur
            JsonElement result;
            
            if (!string.IsNullOrEmpty(matchUpdate.Notes) && matchUpdate.Notes.TrimStart().StartsWith("{"))
            {
                // JSON in Notes-String
                var jsonData = JsonDocument.Parse(matchUpdate.Notes);
                result = jsonData.RootElement;
                System.Diagnostics.Debug.WriteLine("[STATS] Processing JSON from Notes field");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No JSON data found in Notes");
                return null;
            }

            // ✅ FLEXIBEL: Unterstütze verschiedene JSON-Strukturen
            JsonElement dartScoring;
            
            // Variante 1: Direkte dartScoringResult-Property
            if (result.TryGetProperty("dartScoringResult", out dartScoring))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] Found dartScoringResult in JSON");
            }
            // Variante 2: In matchUpdate > result > dartScoringResult
            else if (result.TryGetProperty("matchUpdate", out var matchUpdateEl) &&
                     matchUpdateEl.TryGetProperty("result", out var resultEl) &&
                     resultEl.TryGetProperty("dartScoringResult", out dartScoring))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] Found dartScoringResult in matchUpdate.result");
            }
            // Variante 3: Direkt im result-Element
            else if (result.TryGetProperty("result", out var directResult) &&
                     directResult.TryGetProperty("dartScoringResult", out dartScoring))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] Found dartScoringResult in result");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No dartScoringResult found in JSON structure");
                return null;
            }

            var enhancedStats = new EnhancedDartStatistics();

            // Extrahiere Player1Stats
            if (dartScoring.TryGetProperty("player1Stats", out var player1Data))
            {
                enhancedStats.Player1Stats = ParsePlayerStats(player1Data);
                enhancedStats.Player1Name = enhancedStats.Player1Stats.Name;
                System.Diagnostics.Debug.WriteLine($"[STATS] Extracted Player1: {enhancedStats.Player1Name}, Avg: {enhancedStats.Player1Stats.Average}, LegData Count: {enhancedStats.Player1Stats.LegData.Count}");
            }

            // Extrahiere Player2Stats  
            if (dartScoring.TryGetProperty("player2Stats", out var player2Data))
            {
                enhancedStats.Player2Stats = ParsePlayerStats(player2Data);
                enhancedStats.Player2Name = enhancedStats.Player2Stats.Name;
                System.Diagnostics.Debug.WriteLine($"[STATS] Extracted Player2: {enhancedStats.Player2Name}, Avg: {enhancedStats.Player2Stats.Average}, LegData Count: {enhancedStats.Player2Stats.LegData.Count}");
            }

            // ✅ ERWEITERT: Match-Metadaten mit neuen Feldern
            if (dartScoring.TryGetProperty("matchDuration", out var duration))
            {
                var durationMs = duration.GetInt64();
                enhancedStats.MatchDuration = TimeSpan.FromMilliseconds(durationMs);
                System.Diagnostics.Debug.WriteLine($"[STATS] Match duration: {durationMs}ms = {FormatDuration(enhancedStats.MatchDuration)}");
            }

            if (dartScoring.TryGetProperty("startTime", out var startTime))
            {
                if (DateTime.TryParse(startTime.GetString(), out var parsedStartTime))
                {
                    enhancedStats.StartTime = parsedStartTime;
                    System.Diagnostics.Debug.WriteLine($"[STATS] Start time: {enhancedStats.StartTime}");
                }
            }

            if (dartScoring.TryGetProperty("endTime", out var endTime))
            {
                if (DateTime.TryParse(endTime.GetString(), out var parsedEndTime))
                {
                    enhancedStats.EndTime = parsedEndTime;
                    System.Diagnostics.Debug.WriteLine($"[STATS] End time: {enhancedStats.EndTime}");
                }
            }

            // ✅ NEU: Game Rules
            if (dartScoring.TryGetProperty("gameRules", out var gameRules))
            {
                if (gameRules.TryGetProperty("gameMode", out var gameMode))
                    enhancedStats.GameMode = gameMode.GetString() ?? "";
                if (gameRules.TryGetProperty("doubleOut", out var doubleOut))
                    enhancedStats.DoubleOut = doubleOut.GetBoolean();
                if (gameRules.TryGetProperty("startingScore", out var startingScore))
                    enhancedStats.StartingScore = startingScore.GetInt32();
            }

            // ✅ NEU: Version und Submission Info
            if (dartScoring.TryGetProperty("version", out var version))
                enhancedStats.Version = version.GetString() ?? "";
            if (dartScoring.TryGetProperty("submittedVia", out var submittedVia))
                enhancedStats.SubmittedVia = submittedVia.GetString() ?? "";

            // Bestimme Gewinner basierend auf Legs/Sets
            if (enhancedStats.Player1Stats.Legs > enhancedStats.Player2Stats.Legs)
            {
                enhancedStats.Player1Stats.IsWinner = true;
                enhancedStats.Player2Stats.IsWinner = false;
                System.Diagnostics.Debug.WriteLine($"[STATS] Winner: {enhancedStats.Player1Name} ({enhancedStats.Player1Stats.Legs}-{enhancedStats.Player2Stats.Legs})");
            }
            else if (enhancedStats.Player2Stats.Legs > enhancedStats.Player1Stats.Legs)
            {
                enhancedStats.Player1Stats.IsWinner = false;
                enhancedStats.Player2Stats.IsWinner = true;
                System.Diagnostics.Debug.WriteLine($"[STATS] Winner: {enhancedStats.Player2Name} ({enhancedStats.Player2Stats.Legs}-{enhancedStats.Player1Stats.Legs})");
            }

            return enhancedStats;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error parsing enhanced dart statistics: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// ✅ NEU: Parst erweiterte Spieler-Statistiken aus JSON
    /// </summary>
    private EnhancedPlayerStats ParsePlayerStats(JsonElement playerData)
    {
        var stats = new EnhancedPlayerStats();

        // Basis-Daten
        if (playerData.TryGetProperty("name", out var name))
            stats.Name = name.GetString() ?? "";
        
        // ✅ KORRIGIERT: Average-Parsing mit korrekter Dezimalzahl-Behandlung
        if (playerData.TryGetProperty("average", out var avg))
        {
            if (avg.ValueKind == JsonValueKind.Number)
            {
                stats.Average = avg.GetDouble();
                System.Diagnostics.Debug.WriteLine($"[STATS] Parsed average for {stats.Name}: {stats.Average}");
            }
            else if (avg.ValueKind == JsonValueKind.String)
            {
                // Fallback für String-Werte
                if (double.TryParse(avg.GetString(), System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out double avgValue))
                {
                    stats.Average = avgValue;
                    System.Diagnostics.Debug.WriteLine($"[STATS] Parsed average (from string) for {stats.Name}: {stats.Average}");
                }
            }
        }
        
        if (playerData.TryGetProperty("legs", out var legs))
            stats.Legs = legs.GetInt32();
        if (playerData.TryGetProperty("sets", out var sets))
            stats.Sets = sets.GetInt32();
        if (playerData.TryGetProperty("totalThrows", out var throws))
            stats.TotalThrows = throws.GetInt32();
        if (playerData.TryGetProperty("totalScore", out var score))
            stats.TotalScore = score.GetInt32();
        if (playerData.TryGetProperty("maximums", out var maximums))
            stats.Maximums = maximums.GetInt32();
        if (playerData.TryGetProperty("highFinishes", out var highFinishes))
            stats.HighFinishes = highFinishes.GetInt32();
        if (playerData.TryGetProperty("score26Count", out var score26))
            stats.Score26Count = score26.GetInt32();
        if (playerData.TryGetProperty("checkouts", out var checkouts))
            stats.Checkouts = checkouts.GetInt32();
            
        // ✅ KORRIGIERT: AverageLegAverage-Parsing mit korrekter Dezimalzahl-Behandlung
        if (playerData.TryGetProperty("averageLegAverage", out var legAvg))
        {
            if (legAvg.ValueKind == JsonValueKind.Number)
            {
                stats.AverageLegAverage = legAvg.GetDouble();
            }
            else if (legAvg.ValueKind == JsonValueKind.String)
            {
                if (double.TryParse(legAvg.GetString(), System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out double legAvgValue))
                {
                    stats.AverageLegAverage = legAvgValue;
                }
            }
        }

        // ✅ NEU: Detaillierte Listen extrahieren

        // Maximum Details
        if (playerData.TryGetProperty("maximumDetails", out var maxDetails) && maxDetails.ValueKind == JsonValueKind.Array)
        {
            foreach (var detail in maxDetails.EnumerateArray())
            {
                var maxDetail = new MaximumDetail
                {
                    Total = detail.TryGetProperty("total", out var total) ? total.GetInt32() : 180
                };
                
                if (detail.TryGetProperty("timestamp", out var timestamp))
                    maxDetail.Timestamp = DateTime.Parse(timestamp.GetString() ?? DateTime.Now.ToString());

                if (detail.TryGetProperty("darts", out var darts) && darts.ValueKind == JsonValueKind.Array)
                    maxDetail.Darts = darts.EnumerateArray().Select(d => d.GetInt32()).ToList();

                stats.MaximumDetails.Add(maxDetail);
            }
        }

        // High Finish Details
        if (playerData.TryGetProperty("highFinishDetails", out var finishDetails) && finishDetails.ValueKind == JsonValueKind.Array)
        {
            foreach (var detail in finishDetails.EnumerateArray())
            {
                var finishDetail = new HighFinishDetail();
                
                if (detail.TryGetProperty("finish", out var finish))
                    finishDetail.Finish = finish.GetInt32();
                if (detail.TryGetProperty("remainingScore", out var remaining))
                    finishDetail.RemainingScore = remaining.GetInt32();
                if (detail.TryGetProperty("timestamp", out var timestamp))
                    finishDetail.Timestamp = DateTime.Parse(timestamp.GetString() ?? DateTime.Now.ToString());

                if (detail.TryGetProperty("darts", out var darts) && darts.ValueKind == JsonValueKind.Array)
                    finishDetail.Darts = darts.EnumerateArray().Select(d => d.GetInt32()).ToList();

                stats.HighFinishDetails.Add(finishDetail);
            }
        }

        // Score26 Details
        if (playerData.TryGetProperty("score26Details", out var score26Details) && score26Details.ValueKind == JsonValueKind.Array)
        {
            foreach (var detail in score26Details.EnumerateArray())
            {
                var score26Detail = new Score26Detail();
                
                if (detail.TryGetProperty("total", out var total))
                    score26Detail.Total = total.GetInt32();
                if (detail.TryGetProperty("timestamp", out var timestamp))
                    score26Detail.Timestamp = DateTime.Parse(timestamp.GetString() ?? DateTime.Now.ToString());

                if (detail.TryGetProperty("darts", out var darts) && darts.ValueKind == JsonValueKind.Array)
                    score26Detail.Darts = darts.EnumerateArray().Select(d => d.GetInt32()).ToList();

                stats.Score26Details.Add(score26Detail);
            }
        }

        // Checkout Details
        if (playerData.TryGetProperty("checkoutDetails", out var checkoutDetails) && checkoutDetails.ValueKind == JsonValueKind.Array)
        {
            foreach (var detail in checkoutDetails.EnumerateArray())
            {
                var checkoutDetail = new CheckoutDetail();
                
                if (detail.TryGetProperty("finish", out var finish))
                    checkoutDetail.Finish = finish.GetInt32();
                if (detail.TryGetProperty("doubleOut", out var doubleOut))
                    checkoutDetail.DoubleOut = doubleOut.GetBoolean();
                if (detail.TryGetProperty("timestamp", out var timestamp))
                    checkoutDetail.Timestamp = DateTime.Parse(timestamp.GetString() ?? DateTime.Now.ToString());

                if (detail.TryGetProperty("darts", out var darts) && darts.ValueKind == JsonValueKind.Array)
                    checkoutDetail.Darts = darts.EnumerateArray().Select(d => d.GetInt32()).ToList();

                stats.CheckoutDetails.Add(checkoutDetail);
            }
        }

        // ✅ NEU: Leg Averages mit korrektem Double-Parsing
        if (playerData.TryGetProperty("legAverages", out var legAverages) && legAverages.ValueKind == JsonValueKind.Array)
        {
            foreach (var legAverageElement in legAverages.EnumerateArray())
            {
                var legAverage = new LegAverage();
                
                if (legAverageElement.TryGetProperty("legNumber", out var legNumber))
                    legAverage.LegNumber = legNumber.GetInt32();
                    
                // ✅ KORRIGIERT: Leg Average mit korrektem Double-Parsing
                if (legAverageElement.TryGetProperty("average", out var average))
                {
                    if (average.ValueKind == JsonValueKind.Number)
                    {
                        legAverage.Average = average.GetDouble();
                    }
                    else if (average.ValueKind == JsonValueKind.String)
                    {
                        if (double.TryParse(average.GetString(), System.Globalization.NumberStyles.Float, 
                            System.Globalization.CultureInfo.InvariantCulture, out double avgVal))
                        {
                            legAverage.Average = avgVal;
                        }
                    }
                }
                
                if (legAverageElement.TryGetProperty("score", out var legScore))
                    legAverage.Score = legScore.GetInt32();
                if (legAverageElement.TryGetProperty("throws", out var legThrows))
                    legAverage.Throws = legThrows.GetInt32();
                if (legAverageElement.TryGetProperty("timestamp", out var timestamp))
                    legAverage.Timestamp = DateTime.Parse(timestamp.GetString() ?? DateTime.Now.ToString());

                stats.LegAverages.Add(legAverage);
            }
        }

        // ✅ NEU: Leg Data mit Darts pro Leg
        if (playerData.TryGetProperty("legData", out var legData) && legData.ValueKind == JsonValueKind.Array)
        {
            foreach (var legDataElement in legData.EnumerateArray())
            {
                var legDataItem = new LegData();
                
                if (legDataElement.TryGetProperty("legNumber", out var legNumber))
                    legDataItem.LegNumber = legNumber.GetInt32();
                    
                // ✅ NEU: Darts pro Leg
                if (legDataElement.TryGetProperty("darts", out var darts))
                    legDataItem.Darts = darts.GetInt32();
                    
                // ✅ NEU: Won-Status
                if (legDataElement.TryGetProperty("won", out var won))
                    legDataItem.Won = won.GetBoolean();
                    
                // Average mit korrektem Double-Parsing
                if (legDataElement.TryGetProperty("average", out var average))
                {
                    if (average.ValueKind == JsonValueKind.Number)
                    {
                        legDataItem.Average = average.GetDouble();
                    }
                    else if (average.ValueKind == JsonValueKind.String)
                    {
                        if (double.TryParse(average.GetString(), System.Globalization.NumberStyles.Float, 
                            System.Globalization.CultureInfo.InvariantCulture, out double avgVal))
                        {
                            legDataItem.Average = avgVal;
                        }
                    }
                }
                
                if (legDataElement.TryGetProperty("score", out var legScore))
                    legDataItem.Score = legScore.GetInt32();
                if (legDataElement.TryGetProperty("timestamp", out var timestamp))
                    legDataItem.Timestamp = DateTime.Parse(timestamp.GetString() ?? DateTime.Now.ToString());

                stats.LegData.Add(legDataItem);
                
                System.Diagnostics.Debug.WriteLine($"[STATS] Leg {legDataItem.LegNumber}: {legDataItem.Darts} darts, Ø {legDataItem.Average:F1}, Won: {legDataItem.Won}");
            }
        }

        return stats;
    }

    /// <summary>
    /// ✅ NEU: Verarbeitet einfache Statistiken und erstellt PlayerMatchStatistics
    /// </summary>
    private void ProcessSimpleStatistics(HubMatchUpdateEventArgs matchUpdate, SimplePlayerStatistics simpleStats)
    {
        try
        {
            // Erstelle Match-Statistiken für beide Spieler
            var player1Stats = CreateSimpleMatchStatistics(
                matchUpdate,
                simpleStats.Player1Name,
                simpleStats.Player2Name,
                simpleStats.Player1Stats,
                simpleStats.MatchDuration
            );

            var player2Stats = CreateSimpleMatchStatistics(
                matchUpdate,
                simpleStats.Player2Name,
                simpleStats.Player1Name,
                simpleStats.Player2Stats,
                simpleStats.MatchDuration
            );

            // Füge Statistiken zu den Spieler-Daten hinzu
            AddOrUpdatePlayerStatistics(simpleStats.Player1Name, player1Stats);
            AddOrUpdatePlayerStatistics(simpleStats.Player2Name, player2Stats);

            System.Diagnostics.Debug.WriteLine($"[STATS] Successfully processed simple statistics for {simpleStats.Player1Name} and {simpleStats.Player2Name}");
            
            OnPropertyChanged(nameof(PlayerStatistics));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error processing simple statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEU: Erstellt PlayerMatchStatistics aus einfachen Daten mit allen neuen Feldern
    /// </summary>
    private PlayerMatchStatistics CreateSimpleMatchStatistics(
        HubMatchUpdateEventArgs matchUpdate,
        string playerName,
        string opponentName,
        SimplePlayerData playerData,
        TimeSpan matchDuration)
    {
        var stats = new PlayerMatchStatistics
        {
            MatchId = matchUpdate.MatchUuid ?? matchUpdate.MatchId.ToString(),
            PlayerName = playerName,
            Opponent = opponentName,
            Average = playerData.Average,
            Legs = GetPlayerLegs(matchUpdate, playerName),
            Sets = GetPlayerSets(matchUpdate, playerName),
            TotalThrows = playerData.TotalThrows, // ✅ NEU: Verwende echte Daten
            TotalScore = playerData.TotalScore,   // ✅ NEU: Verwende echte Daten
            Maximums = playerData.Scores180,
            HighFinishes = playerData.HighFinishes,
            Score26Count = playerData.Scores26,
            Checkouts = playerData.Checkouts, // ✅ NEU: Verwende echte Checkout-Daten
            AverageLegAverage = playerData.Average, // Annahme für einfache Struktur
            MatchDate = matchUpdate.UpdatedAt,
            IsWinner = IsPlayerWinner(matchUpdate, playerName),
            MatchType = matchUpdate.MatchType ?? "Unknown",
            MatchDuration = matchDuration
        };

        // ✅ ERWEITERT: Verwende echte High Finish Details wenn verfügbar
        if (playerData.HighFinishDetails.Count > 0)
        {
            stats.HighFinishDetails = playerData.HighFinishDetails.ToList();
            System.Diagnostics.Debug.WriteLine($"[STATS] {playerName}: {playerData.HighFinishDetails.Count} high finishes");
        }
        else
        {
            // Fallback: Erstelle Detail-Listen aus einfachen Daten für Rückwärtskompatibilität
            foreach (var finishScore in playerData.HighFinishScores)
            {
                stats.HighFinishDetails.Add(new HighFinishDetail
                {
                    Finish = finishScore,
                    RemainingScore = finishScore,
                    Timestamp = matchUpdate.UpdatedAt,
                    Darts = new List<int>() // Keine Detail-Darts verfügbar
                });
            }
        }

        // Maximum Details für 180er (vereinfacht)
        for (int i = 0; i < playerData.Scores180; i++)
        {
            stats.MaximumDetails.Add(new MaximumDetail
            {
                Total = 180,
                Timestamp = matchUpdate.UpdatedAt,
                Darts = new List<int> { 60, 60, 60 } // Standard 180er
            });
        }

        // Score26 Details (vereinfacht)
        for (int i = 0; i < playerData.Scores26; i++)
        {
            stats.Score26Details.Add(new Score26Detail
            {
                Total = 26,
                Timestamp = matchUpdate.UpdatedAt,
                Darts = new List<int> { 20, 6, 0 } // Standard schlechter Wurf
            });
        }

        // ✅ NEU: Checkout Details (vereinfacht aus High Finishes)
        foreach (var highFinish in stats.HighFinishDetails)
        {
            stats.CheckoutDetails.Add(new CheckoutDetail
            {
                Finish = highFinish.Finish,
                Darts = highFinish.Darts.ToList(),
                DoubleOut = false, // Nicht verfügbar in einfacher Struktur
                Timestamp = highFinish.Timestamp
            });
        }

        return stats;
    }

    /// <summary>
    /// ✅ NEU: Verarbeitet erweiterte Statistiken und erstellt PlayerMatchStatistics
    /// </summary>
    private void ProcessEnhancedStatistics(HubMatchUpdateEventArgs matchUpdate, EnhancedDartStatistics enhancedStats)
    {
        try
        {
            // Erstelle Match-Statistiken für beide Spieler mit vollen Details
            var player1Stats = CreateEnhancedMatchStatistics(
                matchUpdate,
                enhancedStats.Player1Name,
                enhancedStats.Player2Name,
                enhancedStats.Player1Stats,
                enhancedStats.MatchDuration
            );

            var player2Stats = CreateEnhancedMatchStatistics(
                matchUpdate,
                enhancedStats.Player2Name,
                enhancedStats.Player1Name,
                enhancedStats.Player2Stats,
                enhancedStats.MatchDuration
            );

            // Füge Statistiken zu den Spieler-Daten hinzu
            AddOrUpdatePlayerStatistics(enhancedStats.Player1Name, player1Stats);
            AddOrUpdatePlayerStatistics(enhancedStats.Player2Name, player2Stats);

            System.Diagnostics.Debug.WriteLine($"[STATS] Successfully processed enhanced statistics for {enhancedStats.Player1Name} and {enhancedStats.Player2Name}");
            
            OnPropertyChanged(nameof(PlayerStatistics));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error processing enhanced statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEU: Erstellt PlayerMatchStatistics aus erweiterten Daten
    /// </summary>
    private PlayerMatchStatistics CreateEnhancedMatchStatistics(
        HubMatchUpdateEventArgs matchUpdate,
        string playerName,
        string opponentName,
        EnhancedPlayerStats playerStats,
        TimeSpan matchDuration)
    {
        return new PlayerMatchStatistics
        {
            MatchId = matchUpdate.MatchUuid ?? matchUpdate.MatchId.ToString(),
            PlayerName = playerName,
            Opponent = opponentName,
            Average = playerStats.Average,
            Legs = playerStats.Legs,
            Sets = playerStats.Sets,
            TotalThrows = playerStats.TotalThrows,
            TotalScore = playerStats.TotalScore,
            Maximums = playerStats.Maximums,
            HighFinishes = playerStats.HighFinishes,
            Score26Count = playerStats.Score26Count,
            Checkouts = playerStats.Checkouts,
            AverageLegAverage = playerStats.AverageLegAverage,
            MatchDate = matchUpdate.UpdatedAt,
            IsWinner = playerStats.IsWinner,
            MatchType = matchUpdate.MatchType ?? "Unknown",
            MatchDuration = matchDuration,
            
            // ✅ NEU: Detaillierte Listen übertragen
            MaximumDetails = playerStats.MaximumDetails.ToList(),
            HighFinishDetails = playerStats.HighFinishDetails.ToList(),
            Score26Details = playerStats.Score26Details.ToList(),
            CheckoutDetails = playerStats.CheckoutDetails.ToList(),
            LegAverages = playerStats.LegAverages.ToList(),
            LegData = playerStats.LegData.ToList() // ✅ NEU: Leg Data mit Darts pro Leg
        };
    }

    /// <summary>
    /// ✅ FALLBACK: Verarbeitet legacy Notes-basierte Statistiken
    /// </summary>
    private void ProcessLegacyNotesStatistics(HubMatchUpdateEventArgs matchUpdate)
    {
        // ✅ NEU: Versuche zuerst Spielernamen aus der neuen JSON-Struktur zu extrahieren
        string player1Name = "Player 1";
        string player2Name = "Player 2";
        
        // ✅ NEU: Extrahiere auch statistics-Daten parallel
        SimplePlayerData player1StatsData = new SimplePlayerData();
        SimplePlayerData player2StatsData = new SimplePlayerData();
        
        try
        {
            if (!string.IsNullOrEmpty(matchUpdate.Notes) && matchUpdate.Notes.TrimStart().StartsWith("{"))
            {
                var jsonData = JsonDocument.Parse(matchUpdate.Notes);
                var root = jsonData.RootElement;

                // Extrahiere Spielernamen aus matchUpdate.result
                if (root.TryGetProperty("matchUpdate", out var matchUpdate_El) && 
                    matchUpdate_El.TryGetProperty("result", out var resultEl))
                {
                    if (resultEl.TryGetProperty("player1Name", out var p1Name))
                    {
                        player1Name = p1Name.GetString() ?? "Player 1";
                        System.Diagnostics.Debug.WriteLine($"[STATS] Legacy extraction found player1Name: {player1Name}");
                    }

                    if (resultEl.TryGetProperty("player2Name", out var p2Name))
                    {
                        player2Name = p2Name.GetString() ?? "Player 2";
                        System.Diagnostics.Debug.WriteLine($"[STATS] Legacy extraction found player2Name: {player2Name}");
                    }
                }

                // ✅ NEU: Extrahiere statistics-Daten parallel
                if (root.TryGetProperty("statistics", out var statistics))
                {
                    System.Diagnostics.Debug.WriteLine("[STATS] Found statistics section in legacy processing, extracting data...");
                    
                    if (statistics.TryGetProperty("player1", out var p1Stats))
                    {
                        player1StatsData = ParseSimplePlayerData(p1Stats);
                        System.Diagnostics.Debug.WriteLine($"[STATS] Extracted statistics player1: Avg {player1StatsData.Average}, 180s: {player1StatsData.Scores180}, HF: {player1StatsData.HighFinishes}, 26s: {player1StatsData.Scores26}");
                    }
                    
                    if (statistics.TryGetProperty("player2", out var p2Stats))
                    {
                        player2StatsData = ParseSimplePlayerData(p2Stats);
                        System.Diagnostics.Debug.WriteLine($"[STATS] Extracted statistics player2: Avg {player2StatsData.Average}, 180s: {player2StatsData.Scores180}, HF: {player2StatsData.HighFinishes}, 26s: {player2StatsData.Scores26}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error extracting data from JSON: {ex.Message}");
        }

        // Parse Notes für erweiterte Statistiken aus Text
        var dartStats = ExtractDartStatisticsFromNotes(matchUpdate.Notes);
        if (dartStats == null)
        {
            System.Diagnostics.Debug.WriteLine("[STATS] No dart statistics found in match update notes");
            
            // ✅ NEU: Fallback - erstelle Statistiken nur aus statistics-Daten wenn keine Notes-Daten vorhanden
            if (IsValidPlayerData(player1StatsData) || IsValidPlayerData(player2StatsData))
            {
                System.Diagnostics.Debug.WriteLine("[STATS] Creating statistics from statistics-section only");
                CreateStatisticsFromStatisticsData(matchUpdate, player1Name, player2Name, player1StatsData, player2StatsData);
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[STATS] No valid data in either notes or statistics section");
                return;
            }
        }

        // ✅ NEU: Verwende die extrahierten Spielernamen statt der aus Notes extrahierten
        if (player1Name != "Player 1")
        {
            dartStats.Player1Name = player1Name;
        }
        if (player2Name != "Player 2")
        {
            dartStats.Player2Name = player2Name;
        }

        // ✅ NEU: Kombiniere Notes-Daten mit statistics-Daten (statistics haben Priorität bei gültigen Werten)
        dartStats.Player1Stats = MergePlayerStatistics(dartStats.Player1Stats, player1StatsData);
        dartStats.Player2Stats = MergePlayerStatistics(dartStats.Player2Stats, player2StatsData);

        System.Diagnostics.Debug.WriteLine($"[STATS] Final merged statistics: {dartStats.Player1Name} vs {dartStats.Player2Name}");
        System.Diagnostics.Debug.WriteLine($"[STATS] Player1 merged: Avg {dartStats.Player1Stats.Average}, 180s: {dartStats.Player1Stats.Maximums}, HF: {dartStats.Player1Stats.HighFinishes}, 26s: {dartStats.Player1Stats.Score26Count}");
        System.Diagnostics.Debug.WriteLine($"[STATS] Player2 merged: Avg {dartStats.Player2Stats.Average}, 180s: {dartStats.Player2Stats.Maximums}, HF: {dartStats.Player2Stats.HighFinishes}, 26s: {dartStats.Player2Stats.Score26Count}");

        // Erstelle Match-Statistiken für beide Spieler
        var player1Stats = CreateMatchStatistics(
            matchUpdate,
            dartStats.Player1Name,
            dartStats.Player2Name,
            dartStats.Player1Stats,
            dartStats.Player1Stats.Winner
        );

        var player2Stats = CreateMatchStatistics(
            matchUpdate,
            dartStats.Player2Name,
            dartStats.Player1Name,
            dartStats.Player2Stats,
            dartStats.Player2Stats.Winner
        );

        // Füge Statistiken zu den Spieler-Daten hinzu
        AddOrUpdatePlayerStatistics(dartStats.Player1Name, player1Stats);
        AddOrUpdatePlayerStatistics(dartStats.Player2Name, player2Stats);

        System.Diagnostics.Debug.WriteLine($"[STATS] Successfully processed merged match statistics for {dartStats.Player1Name} and {dartStats.Player2Name}");
        
        OnPropertyChanged(nameof(PlayerStatistics));
    }

    /// <summary>
    /// ✅ NEU: Erstellt Statistiken nur aus statistics-Daten (wenn keine Notes verfügbar)
    /// </summary>
    private void CreateStatisticsFromStatisticsData(HubMatchUpdateEventArgs matchUpdate, 
        string player1Name, string player2Name, 
        SimplePlayerData player1StatsData, SimplePlayerData player2StatsData)
    {
        try
        {
            // Bestimme Gewinner
            player1StatsData.IsWinner = IsPlayerWinner(matchUpdate, player1Name);
            player2StatsData.IsWinner = IsPlayerWinner(matchUpdate, player2Name);

            // Erstelle Match-Statistiken für beide Spieler
            var player1Stats = CreateSimpleMatchStatistics(
                matchUpdate,
                player1Name,
                player2Name,
                player1StatsData,
                TimeSpan.Zero
            );

            var player2Stats = CreateSimpleMatchStatistics(
                matchUpdate,
                player2Name,
                player1Name,
                player2StatsData,
                TimeSpan.Zero
            );

            // Füge Statistiken zu den Spieler-Daten hinzu
            AddOrUpdatePlayerStatistics(player1Name, player1Stats);
            AddOrUpdatePlayerStatistics(player2Name, player2Stats);

            System.Diagnostics.Debug.WriteLine($"[STATS] Successfully processed statistics-only data for {player1Name} and {player2Name}");
            
            OnPropertyChanged(nameof(PlayerStatistics));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error creating statistics from statistics-data: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEU: Kombiniert Notes-basierte Statistiken mit statistics-Daten
    /// statistics-Daten haben Priorität bei gültigen Werten
    /// </summary>
    private DartStatisticsExtract.PlayerStatsExtract MergePlayerStatistics(
        DartStatisticsExtract.PlayerStatsExtract notesStats, 
        SimplePlayerData statisticsData)
    {
        var merged = new DartStatisticsExtract.PlayerStatsExtract
        {
            // Verwende statistics-Werte wenn gültig, sonst Notes-Werte
            Average = statisticsData.Average > 0 ? statisticsData.Average : notesStats.Average,
            Maximums = statisticsData.Scores180 > 0 ? statisticsData.Scores180 : notesStats.Maximums,
            HighFinishes = statisticsData.HighFinishes > 0 ? statisticsData.HighFinishes : notesStats.HighFinishes,
            Score26Count = statisticsData.Scores26 > 0 ? statisticsData.Scores26 : notesStats.Score26Count,
            
            // ✅ NEU: Verwende neue Felder aus statistics wenn verfügbar
            Checkouts = statisticsData.Checkouts > 0 ? statisticsData.Checkouts : notesStats.Checkouts,
            TotalThrows = statisticsData.TotalThrows > 0 ? statisticsData.TotalThrows : 0,
            TotalScore = statisticsData.TotalScore > 0 ? statisticsData.TotalScore : 0,
            
            // Behalte Notes-Werte für nicht-statistics-Felder
            Legs = notesStats.Legs,
            Sets = notesStats.Sets,
            Winner = notesStats.Winner
        };

        System.Diagnostics.Debug.WriteLine($"[STATS] Merged stats - Avg: {merged.Average} (notes: {notesStats.Average}, stats: {statisticsData.Average})");
        System.Diagnostics.Debug.WriteLine($"[STATS] Merged stats - 180s: {merged.Maximums} (notes: {notesStats.Maximums}, stats: {statisticsData.Scores180})");
        System.Diagnostics.Debug.WriteLine($"[STATS] Merged stats - HF: {merged.HighFinishes} (notes: {notesStats.HighFinishes}, stats: {statisticsData.HighFinishes})");
        System.Diagnostics.Debug.WriteLine($"[STATS] Merged stats - 26s: {merged.Score26Count} (notes: {notesStats.Score26Count}, stats: {statisticsData.Scores26})");
        System.Diagnostics.Debug.WriteLine($"[STATS] Merged stats - Checkouts: {merged.Checkouts} (notes: {notesStats.Checkouts}, stats: {statisticsData.Checkouts})");
        System.Diagnostics.Debug.WriteLine($"[STATS] Merged stats - TotalThrows: {merged.TotalThrows}, TotalScore: {merged.TotalScore}");

        return merged;
    }

    /// <summary>
    /// Hilfsklasse für die Extraktion von Dart-Statistiken aus WebSocket-Nachrichten
    /// </summary>
    internal class DartStatisticsExtract
    {
        public string Player1Name { get; set; } = "";
        public string Player2Name { get; set; } = "";
        public PlayerStatsExtract Player1Stats { get; set; } = new();
        public PlayerStatsExtract Player2Stats { get; set; } = new();

        public class PlayerStatsExtract
        {
            public double Average { get; set; } = 0.0;
            public int Legs { get; set; } = 0;
            public int Sets { get; set; } = 0;
            public int Maximums { get; set; } = 0;
            public int HighFinishes { get; set; } = 0;
            public int Score26Count { get; set; } = 0;
            public int Checkouts { get; set; } = 0;
            public int TotalThrows { get; set; } = 0; // ✅ NEU
            public int TotalScore { get; set; } = 0; // ✅ NEU
            public bool Winner { get; set; } = false;
        }
    }

    /// <summary>
    /// ✅ ERWEITERT: Erweiterte Dart-Statistiken aus dartScoringResult mit neuen Feldern
    /// </summary>
    internal class EnhancedDartStatistics
    {
        public string Player1Name { get; set; } = "";
        public string Player2Name { get; set; } = "";
        public EnhancedPlayerStats Player1Stats { get; set; } = new();
        public EnhancedPlayerStats Player2Stats { get; set; } = new();
        public TimeSpan MatchDuration { get; set; } = TimeSpan.Zero;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; } = DateTime.Now;
        
        // ✅ NEU: Game Rules
        public string GameMode { get; set; } = "";
        public bool DoubleOut { get; set; } = false;
        public int StartingScore { get; set; } = 501;
        
        // ✅ NEU: Submission Info
        public string Version { get; set; } = "";
        public string SubmittedVia { get; set; } = "";
    }

    /// <summary>
    /// ✅ NEU: Erweiterte Spieler-Statistiken mit Detail-Listen
    /// </summary>
    internal class EnhancedPlayerStats
    {
        public string Name { get; set; } = "";
        public double Average { get; set; } = 0.0;
        public int Legs { get; set; } = 0;
        public int Sets { get; set; } = 0;
        public int TotalThrows { get; set; } = 0;
        public int TotalScore { get; set; } = 0;
        public int Maximums { get; set; } = 0;
        public int HighFinishes { get; set; } = 0;
        public int Score26Count { get; set; } = 0;
        public int Checkouts { get; set; } = 0;
        public double AverageLegAverage { get; set; } = 0.0;
        public bool IsWinner { get; set; } = false;

        // Detail-Listen
        public List<MaximumDetail> MaximumDetails { get; set; } = new();
        public List<HighFinishDetail> HighFinishDetails { get; set; } = new();
        public List<Score26Detail> Score26Details { get; set; } = new();
        public List<CheckoutDetail> CheckoutDetails { get; set; } = new();
        public List<LegAverage> LegAverages { get; set; } = new();
        public List<LegData> LegData { get; set; } = new();  // ✅ NEU
    }

    /// <summary>
    /// ✅ NEU: Einfache Dart-Statistiken aus playerStatistics (erweitert mit Match-Dauer)
    /// </summary>
    internal class SimplePlayerStatistics
    {
        public string Player1Name { get; set; } = "";
        public string Player2Name { get; set; } = "";
        public SimplePlayerData Player1Stats { get; set; } = new();
        public SimplePlayerData Player2Stats { get; set; } = new();
        public TimeSpan MatchDuration { get; set; } = TimeSpan.Zero;
        public string MatchDurationString { get; set; } = "";
        public string MatchFormat { get; set; } = "";
        public DateTime StartTime { get; set; } = DateTime.Now; // ✅ NEU
        public DateTime EndTime { get; set; } = DateTime.Now; // ✅ NEU
        public int TotalThrows { get; set; } = 0; // ✅ NEU
    }

    /// <summary>
    /// ✅ NEU: Einfache Spieler-Daten aus playerStatistics (erweitert mit neuen Feldern)
    /// </summary>
    internal class SimplePlayerData
    {
        public string Name { get; set; } = "";
        public double Average { get; set; } = 0.0;
        public int Scores180 { get; set; } = 0;
        public int HighFinishes { get; set; } = 0;
        public List<int> HighFinishScores { get; set; } = new();
        public List<HighFinishDetail> HighFinishDetails { get; set; } = new(); // ✅ NEU
        public int Scores26 { get; set; } = 0;
        public int Checkouts { get; set; } = 0; // ✅ NEU
        public int TotalThrows { get; set; } = 0; // ✅ NEU
        public int TotalScore { get; set; } = 0; // ✅ NEU
        public bool IsWinner { get; set; } = false;
    }

    /// <summary>
    /// ✅ NEU: Hilfsmethoden для einfache Statistik-Extraktion
    /// </summary>
    private string GetPlayerNameFromMatch(HubMatchUpdateEventArgs matchUpdate, int playerNumber)
    {
        try
        {
            // ✅ ERWEITERT: Extrahiere Spielernamen aus Notes JSON wenn verfügbar
            if (!string.IsNullOrEmpty(matchUpdate.Notes) && matchUpdate.Notes.TrimStart().StartsWith("{"))
            {
                var jsonData = JsonDocument.Parse(matchUpdate.Notes);
                var root = jsonData.RootElement;

                // ✅ NEU: Versuche matchUpdate.result.player1Name/player2Name
                if (root.TryGetProperty("matchUpdate", out var matchUpdateEl) && 
                    matchUpdateEl.TryGetProperty("result", out var resultEl))
                {
                    if (playerNumber == 1 && resultEl.TryGetProperty("player1Name", out var player1Name))
                    {
                        var name = player1Name.GetString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            System.Diagnostics.Debug.WriteLine($"[STATS] Found player1Name in matchUpdate.result: {name}");
                            return name;
                        }
                    }
                    if (playerNumber == 2 && resultEl.TryGetProperty("player2Name", out var player2Name))
                    {
                        var name = player2Name.GetString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            System.Diagnostics.Debug.WriteLine($"[STATS] Found player2Name in matchUpdate.result: {name}");
                            return name;
                        }
                    }
                }

                // Versuche verschiedene andere JSON-Strukturen
                if (root.TryGetProperty("result", out var result))
                {
                    if (playerNumber == 1 && result.TryGetProperty("player1Name", out var player1))
                    {
                        var name = player1.GetString();
                        if (!string.IsNullOrEmpty(name)) return name;
                    }
                    if (playerNumber == 2 && result.TryGetProperty("player2Name", out var player2))
                    {
                        var name = player2.GetString();
                        if (!string.IsNullOrEmpty(name)) return name;
                    }

                    // Fallback auf player1/player2 ohne "Name" Suffix
                    if (playerNumber == 1 && result.TryGetProperty("player1", out var p1))
                    {
                        var name = p1.GetString();
                        if (!string.IsNullOrEmpty(name)) return name;
                    }
                    if (playerNumber == 2 && result.TryGetProperty("player2", out var p2))
                    {
                        var name = p2.GetString();
                        if (!string.IsNullOrEmpty(name)) return name;
                    }
                }

                // Versuche auch direkt im Root
                if (playerNumber == 1 && root.TryGetProperty("player1Name", out var rootP1))
                {
                    var name = rootP1.GetString();
                    if (!string.IsNullOrEmpty(name)) return name;
                }
                if (playerNumber == 2 && root.TryGetProperty("player2Name", out var rootP2))
                {
                    var name = rootP2.GetString();
                    if (!string.IsNullOrEmpty(name)) return name;
                }

                // Fallback auf player1/player2 im Root
                if (playerNumber == 1 && root.TryGetProperty("player1", out var directP1))
                {
                    var name = directP1.GetString();
                    if (!string.IsNullOrEmpty(name)) return name;
                }
                if (playerNumber == 2 && root.TryGetProperty("player2", out var directP2))
                {
                    var name = directP2.GetString();
                    if (!string.IsNullOrEmpty(name)) return name;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[STATS] No player name found for player {playerNumber}, using fallback");
            return $"Player {playerNumber}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error extracting player name: {ex.Message}");
            return $"Player {playerNumber}";
        }
    }

    private void DetermineWinnerForSimpleStats(HubMatchUpdateEventArgs matchUpdate, SimplePlayerStatistics simpleStats)
    {
        try
        {
            // Bestimme Gewinner basierend auf Legs oder Sets
            int player1Legs = matchUpdate.Player1Legs;
            int player2Legs = matchUpdate.Player2Legs;
            int player1Sets = matchUpdate.Player1Sets;
            int player2Sets = matchUpdate.Player2Sets;

            if (player1Sets > player2Sets || (player1Sets == player2Sets && player1Legs > player2Legs))
            {
                simpleStats.Player1Stats.IsWinner = true;
                simpleStats.Player2Stats.IsWinner = false;
                System.Diagnostics.Debug.WriteLine($"[STATS] Winner: {simpleStats.Player1Name} (Sets: {player1Sets}-{player2Sets}, Legs: {player1Legs}-{player2Legs})");
            }
            else if (player2Sets > player1Sets || (player1Sets == player2Sets && player2Legs > player1Legs))
            {
                simpleStats.Player1Stats.IsWinner = false;
                simpleStats.Player2Stats.IsWinner = true;
                System.Diagnostics.Debug.WriteLine($"[STATS] Winner: {simpleStats.Player2Name} (Sets: {player1Sets}-{player2Sets}, Legs: {player1Legs}-{player2Legs})");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error determining winner: {ex.Message}");
        }
    }

    private int GetPlayerLegs(HubMatchUpdateEventArgs matchUpdate, string playerName)
    {
        try
        {
            // ✅ KORRIGIERT: Da wir nicht direkt vergleichen können, verwende Positionslogik
            if (playerName == GetPlayerNameFromMatch(matchUpdate, 1))
                return matchUpdate.Player1Legs;
            else if (playerName == GetPlayerNameFromMatch(matchUpdate, 2))
                return matchUpdate.Player2Legs;
        }
        catch
        {
            // Ignoriere Fehler
        }
        return 0;
    }

    private int GetPlayerSets(HubMatchUpdateEventArgs matchUpdate, string playerName)
    {
        try
        {
            // ✅ KORRIGIERT: Da wir nicht direkt vergleichen können, verwende Positionslogik
            if (playerName == GetPlayerNameFromMatch(matchUpdate, 1))
                return matchUpdate.Player1Sets;
            else if (playerName == GetPlayerNameFromMatch(matchUpdate, 2))
                return matchUpdate.Player2Sets;
        }
        catch
        {
            // Ignoriere Fehler
        }
        return 0;
    }

    private bool IsPlayerWinner(HubMatchUpdateEventArgs matchUpdate, string playerName)
    {
        try
        {
            // ✅ KORRIGIERT: Bestimme Gewinner basierend auf Legs/Sets
            string player1Name = GetPlayerNameFromMatch(matchUpdate, 1);
            string player2Name = GetPlayerNameFromMatch(matchUpdate, 2);
            
            int player1Legs = matchUpdate.Player1Legs;
            int player2Legs = matchUpdate.Player2Legs;
            int player1Sets = matchUpdate.Player1Sets;
            int player2Sets = matchUpdate.Player2Sets;

            // Bestimme Gewinner
            bool player1Wins = player1Sets > player2Sets || (player1Sets == player2Sets && player1Legs > player2Legs);
            bool player2Wins = player2Sets > player1Sets || (player1Sets == player2Sets && player2Legs > player1Legs);

            if (playerName == player1Name && player1Wins)
                return true;
            if (playerName == player2Name && player2Wins)
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gibt Statistiken-Zusammenfassung zurück
    /// </summary>
    public StatisticsSummary GetStatisticsSummary()
    {
        return new StatisticsSummary
        {
            TotalPlayers = _playerStatistics.Count,
            TotalMatches = _playerStatistics.Values.Sum(p => p.TotalMatches),
            TotalLegs = _playerStatistics.Values.Sum(p => p.LegsWon + p.LegsLost),
            TotalMaximums = _playerStatistics.Values.Sum(p => p.TotalMaximums),
            TotalHighFinishes = _playerStatistics.Values.Sum(p => p.TotalHighFinishes),
            AverageOverallAverage = _playerStatistics.Values.Count > 0 ? 
                _playerStatistics.Values.Average(p => p.TournamentAverage) : 0.0,
            // ✅ NEU: Erweiterte Zusammenfassungs-Statistiken
            HighestLegAverage = _playerStatistics.Values.Count > 0 ?
                _playerStatistics.Values.Max(p => p.HighestLegAverage) : 0.0,
            HighestFinishScore = _playerStatistics.Values.Count > 0 ?
                _playerStatistics.Values.Max(p => p.HighestFinishScore) : 0,
            FewestDartsToFinish = _playerStatistics.Values.Where(p => p.FewestDartsToFinish > 0).Any() ?
                _playerStatistics.Values.Where(p => p.FewestDartsToFinish > 0).Min(p => p.FewestDartsToFinish) : 0
        };
    }

    public PlayerStatistics? GetPlayerStatistics(string playerName)
    {
        return _playerStatistics.TryGetValue(playerName, out var stats) ? stats : null;
    }

    public List<string> GetAllPlayerNames()
    {
        return _playerStatistics.Keys.ToList();
    }

    public void ClearAllStatistics()
    {
        _playerStatistics.Clear();
        OnPropertyChanged(nameof(PlayerStatistics));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    /// <summary>
    /// ✅ NEU: Öffentliche Methode für externe PropertyChanged-Trigger
    /// </summary>
    public void TriggerPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void AddOrUpdatePlayerStatistics(string playerName, PlayerMatchStatistics matchStats)
    {
        if (!_playerStatistics.TryGetValue(playerName, out var playerStats))
        {
            playerStats = new PlayerStatistics { PlayerName = playerName };
            _playerStatistics[playerName] = playerStats;
        }

        playerStats.AddMatchStatistics(matchStats);
    }

    // ✅ LEGACY: Methoden für Rückwärtskompatibilität

    private DartStatisticsExtract? ExtractDartStatisticsFromNotes(string? notes)
    {
        if (string.IsNullOrEmpty(notes))
            return null;

        try
        {
            var extract = new DartStatisticsExtract();
            
            // Extrahiere Spielernamen und Averages
            if (ExtractAverages(notes, out string player1, out double avg1, out string player2, out double avg2))
            {
                extract.Player1Name = player1;
                extract.Player2Name = player2;
                extract.Player1Stats.Average = avg1;
                extract.Player2Stats.Average = avg2;
            }
            else
            {
                return null;
            }

            // Extrahiere weitere Daten
            ExtractMaximums(notes, extract.Player1Name, extract.Player2Name, extract.Player1Stats, extract.Player2Stats);
            ExtractHighFinishes(notes, extract.Player1Name, extract.Player2Name, extract.Player1Stats, extract.Player2Stats);
            ExtractMatchResult(notes, extract.Player1Name, extract.Player2Name, extract.Player1Stats, extract.Player2Stats);

            return extract;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error extracting dart statistics: {ex.Message}");
            return null;
        }
    }

    private bool ExtractAverages(string notes, out string player1, out double avg1, out string player2, out double avg2)
    {
        player1 = "";
        player2 = "";
        avg1 = 0.0;
        avg2 = 0.0;

        try
        {
            var averageMatch = System.Text.RegularExpressions.Regex.Match(
                notes, 
                @"Averages:\s*([^0-9]+)\s+(\d+\.?\d*),\s*([^0-9•]+)\s+(\d+\.?\d*)"
            );

            if (averageMatch.Success)
            {
                player1 = averageMatch.Groups[1].Value.Trim();
                player2 = averageMatch.Groups[3].Value.Trim();
                
                // ✅ KORRIGIERT: Verwende InvariantCulture für korrekte Dezimalzahl-Parsing
                if (double.TryParse(averageMatch.Groups[2].Value, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out avg1) &&
                    double.TryParse(averageMatch.Groups[4].Value, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out avg2))
                {
                    System.Diagnostics.Debug.WriteLine($"[STATS] Extracted averages: {player1} {avg1}, {player2} {avg2}");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[STATS] Failed to parse averages: '{averageMatch.Groups[2].Value}', '{averageMatch.Groups[4].Value}'");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error extracting averages: {ex.Message}");
        }

        return false;
    }

    private void ExtractMaximums(string notes, string player1Name, string player2Name, 
        DartStatisticsExtract.PlayerStatsExtract player1Stats, DartStatisticsExtract.PlayerStatsExtract player2Stats)
    {
        try
        {
            var maximumMatch = System.Text.RegularExpressions.Regex.Match(
                notes,
                @"180s:\s*" + System.Text.RegularExpressions.Regex.Escape(player1Name) + @"\s+(\d+),\s*" + 
                System.Text.RegularExpressions.Regex.Escape(player2Name) + @"\s+(\d+)"
            );

            if (maximumMatch.Success)
            {
                player1Stats.Maximums = int.Parse(maximumMatch.Groups[1].Value);
                player2Stats.Maximums = int.Parse(maximumMatch.Groups[2].Value);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error extracting maximums: {ex.Message}");
        }
    }

    private void ExtractHighFinishes(string notes, string player1Name, string player2Name,
        DartStatisticsExtract.PlayerStatsExtract player1Stats, DartStatisticsExtract.PlayerStatsExtract player2Stats)
    {
        try
        {
            var highFinishMatch = System.Text.RegularExpressions.Regex.Match(
                notes,
                @"High Finishes \([^)]+\):\s*" + System.Text.RegularExpressions.Regex.Escape(player1Name) + @"\s+(\d+),\s*" +
                System.Text.RegularExpressions.Regex.Escape(player2Name) + @"\s+(\d+)"
            );

            if (highFinishMatch.Success)
            {
                player1Stats.HighFinishes = int.Parse(highFinishMatch.Groups[1].Value);
                player2Stats.HighFinishes = int.Parse(highFinishMatch.Groups[2].Value);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error extracting high finishes: {ex.Message}");
        }
    }

    private void ExtractMatchResult(string notes, string player1Name, string player2Name,
        DartStatisticsExtract.PlayerStatsExtract player1Stats, DartStatisticsExtract.PlayerStatsExtract player2Stats)
    {
        try
        {
            var resultMatch = System.Text.RegularExpressions.Regex.Match(
                notes,
                @"Result:\s*(\d+)-(\d+)\s+(Legs|Sets)"
            );

            if (resultMatch.Success)
            {
                int score1 = int.Parse(resultMatch.Groups[1].Value);
                int score2 = int.Parse(resultMatch.Groups[2].Value);
                string scoreType = resultMatch.Groups[3].Value;

                if (scoreType == "Legs")
                {
                    player1Stats.Legs = score1;
                    player2Stats.Legs = score2;
                }
                else if (scoreType == "Sets")
                {
                    player1Stats.Sets = score1;
                    player2Stats.Sets = score2;
                }

                if (score1 > score2)
                {
                    player1Stats.Winner = true;
                    player2Stats.Winner = false;
                }
                else if (score2 > score1)
                {
                    player1Stats.Winner = false;
                    player2Stats.Winner = true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS] Error extracting match result: {ex.Message}");
        }
    }

    private PlayerMatchStatistics CreateMatchStatistics(HubMatchUpdateEventArgs matchUpdate, string playerName, 
        string opponentName, DartStatisticsExtract.PlayerStatsExtract playerStats, bool isWinner)
    {
        return new PlayerMatchStatistics
        {
            MatchId = matchUpdate.MatchUuid ?? matchUpdate.MatchId.ToString(),
            PlayerName = playerName,
            Opponent = opponentName,
            Average = playerStats.Average,
            Legs = playerStats.Legs,
            Sets = playerStats.Sets,
            Maximums = playerStats.Maximums,
            HighFinishes = playerStats.HighFinishes,
            Score26Count = playerStats.Score26Count,
            Checkouts = playerStats.Checkouts,
            MatchDate = matchUpdate.UpdatedAt,
            IsWinner = isWinner,
            MatchType = matchUpdate.MatchType ?? "Unknown",
            MatchDuration = TimeSpan.Zero,
        };
    }

    /// <summary>
    /// ✅ NEU: Formatiert eine TimeSpan-Dauer in ein lesbares Format
    /// </summary>
    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
        {
            return $"{duration.Seconds} Sekunden";
        }
        else if (duration.TotalHours < 1)
        {
            return $"{duration.Minutes:D2}:{duration.Seconds:D2} Minuten";
        }
        else
        {
            return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2} Stunden";
        }
    }
}