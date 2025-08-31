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
    /// Verarbeitet nun auch dartScoringResult-Daten für detaillierte Statistiken
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

            // ✅ NEU: Versuche zuerst erweiterte dartScoringResult-Daten zu extrahieren
            var enhancedStats = ExtractEnhancedDartStatistics(matchUpdate);
            if (enhancedStats != null)
            {
                System.Diagnostics.Debug.WriteLine($"[STATS] Processing enhanced dart statistics for {enhancedStats.Player1Name} vs {enhancedStats.Player2Name}");
                ProcessEnhancedStatistics(matchUpdate, enhancedStats);
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
    /// ✅ NEU: Extrahiert erweiterte Dart-Statistiken direkt aus dartScoringResult
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
                System.Diagnostics.Debug.WriteLine($"[STATS] Extracted Player1: {enhancedStats.Player1Name}, Avg: {enhancedStats.Player1Stats.Average}");
            }

            // Extrahiere Player2Stats  
            if (dartScoring.TryGetProperty("player2Stats", out var player2Data))
            {
                enhancedStats.Player2Stats = ParsePlayerStats(player2Data);
                enhancedStats.Player2Name = enhancedStats.Player2Stats.Name;
                System.Diagnostics.Debug.WriteLine($"[STATS] Extracted Player2: {enhancedStats.Player2Name}, Avg: {enhancedStats.Player2Stats.Average}");
            }

            // Extrahiere Match-Metadaten
            if (dartScoring.TryGetProperty("matchDuration", out var duration))
            {
                enhancedStats.MatchDuration = TimeSpan.FromMilliseconds(duration.GetDouble());
                System.Diagnostics.Debug.WriteLine($"[STATS] Match duration: {enhancedStats.MatchDuration}");
            }

            if (dartScoring.TryGetProperty("startTime", out var startTime))
                enhancedStats.StartTime = DateTime.Parse(startTime.GetString() ?? DateTime.Now.ToString());

            if (dartScoring.TryGetProperty("endTime", out var endTime))
                enhancedStats.EndTime = DateTime.Parse(endTime.GetString() ?? DateTime.Now.ToString());

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
            LegAverages = playerStats.LegAverages.ToList()
        };
    }

    /// <summary>
    /// ✅ FALLBACK: Verarbeitet legacy Notes-basierte Statistiken
    /// </summary>
    private void ProcessLegacyNotesStatistics(HubMatchUpdateEventArgs matchUpdate)
    {
        // Parse Notes für erweiterte Statistiken
        var dartStats = ExtractDartStatisticsFromNotes(matchUpdate.Notes);
        if (dartStats == null)
        {
            System.Diagnostics.Debug.WriteLine("[STATS] No dart statistics found in match update");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[STATS] Extracted legacy dart statistics: {dartStats.Player1Name} vs {dartStats.Player2Name}");

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

        System.Diagnostics.Debug.WriteLine($"[STATS] Successfully processed legacy match statistics for {dartStats.Player1Name} and {dartStats.Player2Name}");
        
        OnPropertyChanged(nameof(PlayerStatistics));
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
            public bool Winner { get; set; } = false;
        }
    }

    /// <summary>
    /// ✅ NEU: Erweiterte Dart-Statistiken aus dartScoringResult
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
}

// ✅ NEU: Erweiterte Hilfsklassen für umfangreiche Dart-Statistiken