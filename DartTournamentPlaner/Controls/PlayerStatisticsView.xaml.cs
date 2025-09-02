using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Models.Statistics;
using DartTournamentPlaner.Services.Statistics;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Controls;

/// <summary>
/// UserControl für die Anzeige von Spieler-Statistiken einer Turnierklasse
/// </summary>
public partial class PlayerStatisticsView : UserControl, INotifyPropertyChanged
{
    private TournamentClass? _tournamentClass;
    private StatisticsSummary _summary = new();
    private ObservableCollection<PlayerStatisticsDisplayModel> _players = new();
    private string _debugMessage = "";
    private LocalizationService? _localizationService;

    public PlayerStatisticsView()
    {
        InitializeComponent();
        DataContext = this;
        
        // Try to get localization service from App
        _localizationService = App.LocalizationService;
        UpdateTranslations();
    }

    /// <summary>
    /// Die anzuzeigende Turnierklasse
    /// </summary>
    public TournamentClass? TournamentClass
    {
        get => _tournamentClass;
        set
        {
            _tournamentClass = value;
            OnPropertyChanged();
            LoadStatistics();
        }
    }

    /// <summary>
    /// Zusammenfassung aller Statistiken
    /// </summary>
    public StatisticsSummary Summary
    {
        get => _summary;
        set
        {
            _summary = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Spieler-Statistiken für die Anzeige
    /// </summary>
    public ObservableCollection<PlayerStatisticsDisplayModel> Players
    {
        get => _players;
        set
        {
            _players = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// ✅ NEU: Debug-Informationen für Entwickler
    /// </summary>
    public string DebugMessage
    {
        get => _debugMessage;
        set
        {
            _debugMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// ✅ NEU: Aktualisiert alle Übersetzungen der UI-Elemente
    /// </summary>
    public void UpdateTranslations()
    {
        if (_localizationService == null) return;

        try
        {
            // Titel
            if (TitleTextBlock != null)
                TitleTextBlock.Text = $"📊 {_localizationService.GetString("PlayerStatistics")}_";

            // Summary Headers
            if (PlayersHeaderText != null)
                PlayersHeaderText.Text = _localizationService.GetString("Players");
            if (MatchesHeaderText != null)
                MatchesHeaderText.Text = _localizationService.GetString("TotalMatches");
            if (LegsHeaderText != null)
                LegsHeaderText.Text = _localizationService.GetString("Legs");
            if (MaximumsHeaderText != null)
                MaximumsHeaderText.Text = _localizationService.GetString("TotalMaximums");
            if (HighFinishesHeaderText != null)
                HighFinishesHeaderText.Text = _localizationService.GetString("HighFinishes");
            if (AverageHeaderText != null)
                AverageHeaderText.Text = $"⌀ {_localizationService.GetString("OverallAverage")}";
            
            // Buttons und Aktionen
            if (RefreshButton != null)
                RefreshButton.Content = $"🔄 {_localizationService.GetString("RefreshStatistics")}";
            if (PlayersWithStatsText != null)
                PlayersWithStatsText.Text = _localizationService.GetString("PlayersText");
            if (SortByText != null)
                SortByText.Text = $"{_localizationService.GetString("SortBy")}:";

            // Sortierung ComboBox Items
            if (SortByNameItem != null)
                SortByNameItem.Content = _localizationService.GetString("SortByName");
            if (SortByAverageItem != null)
                SortByAverageItem.Content = _localizationService.GetString("SortByAverage");
            if (SortByMatchesItem != null)
                SortByMatchesItem.Content = _localizationService.GetString("SortByMatches");
            if (SortByWinRateItem != null)
                SortByWinRateItem.Content = _localizationService.GetString("SortByWinRate");
            if (SortByMaximumsItem != null)
                SortByMaximumsItem.Content = _localizationService.GetString("SortByMaximums");
            if (SortByHighFinishesItem != null)
                SortByHighFinishesItem.Content = _localizationService.GetString("SortByHighFinishes");

            // DataGrid Column Headers
            if (PlayerColumn != null)
                PlayerColumn.Header = $"🎯 {_localizationService.GetString("PlayerHeader")}";
            if (MatchesColumn != null)
                MatchesColumn.Header = _localizationService.GetString("TotalMatches");
            if (WinRateColumn != null)
                WinRateColumn.Header = _localizationService.GetString("MatchWinRate");
            if (AverageColumn != null)
                AverageColumn.Header = $"⌀ {_localizationService.GetString("OverallAverage")}";
            if (BestAverageColumn != null)
                BestAverageColumn.Header = _localizationService.GetString("BestAverage");
            if (MaximumsColumn != null)
                MaximumsColumn.Header = _localizationService.GetString("TotalMaximums");
            if (HighFinishesColumn != null)
                HighFinishesColumn.Header = _localizationService.GetString("HighFinishes");
            if (HighFinishScoresColumn != null)
                HighFinishScoresColumn.Header = _localizationService.GetString("HighFinishScores");
            if (HighestFinishColumn != null)
                HighestFinishColumn.Header = _localizationService.GetString("HighestFinish");
            if (Score26Column != null)
                Score26Column.Header = _localizationService.GetString("Score26");
            if (LastMatchColumn != null)
                LastMatchColumn.Header = _localizationService.GetString("LastMatchDate");
            // ✅ NEU: Spalten-Header für neue Statistiken
            if (FastestMatchColumn != null)
                FastestMatchColumn.Header = _localizationService.GetString("FastestMatch");
            if (FewestThrowsColumn != null)
                FewestThrowsColumn.Header = _localizationService.GetString("FewestThrowsInMatch");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Error updating translations: {ex.Message}");
        }
    }

    /// <summary>
    /// Lädt die Statistiken von der aktuellen Turnierklasse
    /// </summary>
    private void LoadStatistics()
    {
        try
        {
            if (_tournamentClass == null)
            {
                System.Diagnostics.Debug.WriteLine("[STATS-VIEW] No tournament class set, clearing statistics");
                Summary = new StatisticsSummary();
                Players.Clear();
                DebugMessage = _localizationService?.GetString("NoStatisticsAvailable") ?? "Keine Turnierklasse ausgewählt";
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Loading statistics for class: {_tournamentClass.Name}");

            // ✅ ERWEITERT: Detailliertes Debug-Logging
            var jsonDataCount = _tournamentClass.PlayerStatisticsData.Count;
            var managerCount = _tournamentClass.StatisticsManager?.PlayerStatistics?.Count ?? 0;
            
            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] JSON data: {jsonDataCount} players, Manager: {managerCount} players");
            
            // ✅ KORRIGIERT: Prüfe ob StatisticsManager verfügbar ist
            if (_tournamentClass.StatisticsManager == null)
            {
                System.Diagnostics.Debug.WriteLine("[STATS-VIEW] No statistics manager available");
                Summary = new StatisticsSummary();
                Players.Clear();
                DebugMessage = _localizationService?.GetString("StatisticsNotEnabled") ?? $"StatisticsManager nicht verfügbar (JSON: {jsonDataCount} Spieler)";
                return;
            }

            // ✅ NEU: Force reload from JSON if manager is empty
            if (managerCount == 0 && jsonDataCount > 0)
            {
                System.Diagnostics.Debug.WriteLine("[STATS-VIEW] Manager empty but JSON has data - triggering ValidateAndRepairStatistics");
                _tournamentClass.ValidateAndRepairStatistics();
                managerCount = _tournamentClass.StatisticsManager.PlayerStatistics.Count;
                System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] After repair: Manager has {managerCount} players");
            }

            // Lade Zusammenfassung
            Summary = _tournamentClass.GetStatisticsSummary();
            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Summary loaded: {Summary.TotalPlayers} players, {Summary.TotalMatches} matches");

            // Lade Spieler-Statistiken
            Players.Clear();
            var playerNames = _tournamentClass.GetPlayersWithStatistics();
            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Found {playerNames.Count} players with statistics");
            
            foreach (var playerName in playerNames)
            {
                var playerStats = _tournamentClass.GetPlayerStatistics(playerName);
                if (playerStats != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Adding player: {playerName} " +
                        $"({playerStats.TotalMatches} matches, {playerStats.OverallAverage:F1} avg, " +
                        $"{playerStats.TotalMaximums} max, {playerStats.TotalHighFinishes} HF)");
                    Players.Add(new PlayerStatisticsDisplayModel(playerStats));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Warning: No statistics found for player: {playerName}");
                }
            }

            // Standard-Sortierung nach Average (höchster zuerst)
            SortPlayers(PlayerStatisticsSortType.Average);

            // ✅ NEU: Debug-Information für UI
            var loadingMessage = _localizationService?.GetString("StatisticsLoading") ?? "Wird geladen...";
            var updatedMessage = _localizationService?.GetString("StatisticsUpdated") ?? "Statistiken aktualisiert";
            DebugMessage = $"JSON: {jsonDataCount}, Manager: {managerCount}, {_localizationService?.GetString("ShowAllPlayers") ?? "Angezeigt"}: {Players.Count} - {DateTime.Now:HH:mm:ss}";

            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Statistics loaded successfully: {Players.Count} players displayed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Error loading statistics: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Stack trace: {ex.StackTrace}");
            var errorMessage = _localizationService?.GetString("ErrorLoadingStatistics") ?? "Fehler: {0}";
            DebugMessage = string.Format(errorMessage, ex.Message);
        }
    }

    /// <summary>
    /// Sortiert die Spieler-Liste
    /// </summary>
    private void SortPlayers(PlayerStatisticsSortType sortType)
    {
        try
        {
            var sortedPlayers = sortType switch
            {
                PlayerStatisticsSortType.Name => Players.OrderBy(p => p.PlayerName).ToList(),
                PlayerStatisticsSortType.Average => Players.OrderByDescending(p => p.OverallAverage).ToList(),
                PlayerStatisticsSortType.TotalMatches => Players.OrderByDescending(p => p.TotalMatches).ToList(),
                PlayerStatisticsSortType.WinRate => Players.OrderByDescending(p => p.MatchWinRate).ToList(),
                PlayerStatisticsSortType.Maximums => Players.OrderByDescending(p => p.TotalMaximums).ToList(),
                PlayerStatisticsSortType.HighFinishes => Players.OrderByDescending(p => p.TotalHighFinishes).ToList(),
                _ => Players.OrderBy(p => p.PlayerName).ToList()
            };

            Players.Clear();
            foreach (var player in sortedPlayers)
            {
                Players.Add(player);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[STATS-VIEW] Error sorting players: {ex.Message}");
        }
    }

    /// <summary>
    /// Event Handler für Sortierung-ComboBox
    /// </summary>
    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            var sortType = comboBox.SelectedIndex switch
            {
                0 => PlayerStatisticsSortType.Name,
                1 => PlayerStatisticsSortType.Average,
                2 => PlayerStatisticsSortType.TotalMatches,
                3 => PlayerStatisticsSortType.WinRate,
                4 => PlayerStatisticsSortType.Maximums,
                5 => PlayerStatisticsSortType.HighFinishes,
                _ => PlayerStatisticsSortType.Name
            };

            SortPlayers(sortType);
        }
    }

    /// <summary>
    /// Event Handler für Aktualisieren-Button
    /// </summary>
    private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        LoadStatistics();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Display-Model für Spieler-Statistiken mit formatierten Eigenschaften für die UI
/// </summary>
public class PlayerStatisticsDisplayModel : INotifyPropertyChanged
{
    private readonly PlayerStatistics _playerStatistics;

    public PlayerStatisticsDisplayModel(PlayerStatistics playerStatistics)
    {
        _playerStatistics = playerStatistics;
        
        // NEU: Zuweisung der neuen Eigenschaften im Konstruktor
        TotalCheckouts = playerStatistics.TotalCheckouts;
        FewestDartsToFinish = playerStatistics.FewestDartsToFinish;
        AverageDartsPerCheckout = playerStatistics.AverageDartsPerCheckout;
        FastestMatch = playerStatistics.FastestMatch; // ✅ NEU
        FewestThrowsInMatch = playerStatistics.FewestThrowsInMatch; // ✅ NEU
    }

    // Basis-Eigenschaften
    public string PlayerName => _playerStatistics.PlayerName;
    public int TotalMatches => _playerStatistics.TotalMatches;
    public int MatchesWon => _playerStatistics.MatchesWon;
    public int MatchesLost => _playerStatistics.MatchesLost;
    public double MatchWinRate => _playerStatistics.MatchWinRate;
    public double OverallAverage => _playerStatistics.OverallAverage;
    public double BestAverage => _playerStatistics.BestAverage;
    public double WorstAverage => _playerStatistics.WorstAverage;
    public int TotalMaximums => _playerStatistics.TotalMaximums;
    public int TotalHighFinishes => _playerStatistics.TotalHighFinishes;
    public int TotalCheckouts { get; private set; } // ✅ NEU
    public int FewestDartsToFinish { get; private set; } // ✅ NEU
    public double AverageDartsPerCheckout { get; private set; } // ✅ NEU
    public TimeSpan FastestMatch { get; private set; } // ✅ NEU
    public int FewestThrowsInMatch { get; private set; } // ✅ NEU
    public int TotalScore26 => _playerStatistics.TotalScore26;

    // ✅ NEU: Erweiterte Eigenschaften für die Tabelle
    public string MatchRecord => $"{MatchesWon}W-{MatchesLost}L ({TotalMatches})";
    public string MatchWinRateFormatted => $"{MatchWinRate:F1}%";
    public string OverallAverageFormatted => $"{OverallAverage:F1}";
    public string BestAverageFormatted => $"{BestAverage:F1}";
    public string LastMatchFormatted => _playerStatistics.LastMatchDate?.ToString("dd.MM") ?? "Keine";
    
    /// <summary>
    /// ✅ NEU: Höchstes High Finish aus den Match-Details extrahieren
    /// </summary>
    public string HighestHighFinish
    {
        get
        {
            try
            {
                // ✅ VERBESSERT: Verwende die neuen erweiterten Statistiken
                var highestFinish = _playerStatistics.HighestFinishScore;
                return highestFinish > 0 ? $"{highestFinish}" : "-";
            }
            catch
            {
                return "-";
            }
        }
    }

    /// <summary>
    /// ✅ NEU: Höchster Leg Average
    /// </summary>
    public string HighestLegAverage
    {
        get
        {
            try
            {
                var highestLegAvg = _playerStatistics.HighestLegAverage;
                return highestLegAvg > 0 ? $"{highestLegAvg:F1}" : "-";
            }
            catch
            {
                return "-";
            }
        }
    }

    /// <summary>
    /// ✅ NEU: Turnier Average (Alias für OverallAverage)
    /// </summary>
    public string TournamentAverage => $"{_playerStatistics.TournamentAverage:F1}";

    /// <summary>
    /// ✅ NEU: Wenigste Darts bis zum Finish
    /// </summary>
    public string FewestDartsToFinishFormatted
    {
        get
        {
            try
            {
                var fewestDarts = _playerStatistics.FewestDartsToFinish;
                return fewestDarts > 0 ? $"{fewestDarts}" : "-";
            }
            catch
            {
                return "-";
            }
        }
    }

    /// <summary>
    /// ✅ NEU: Durchschnittliche Darts pro Checkout
    /// </summary>
    public string AverageDartsPerCheckoutFormatted
    {
        get
        {
            try
            {
                var avgDarts = _playerStatistics.AverageDartsPerCheckout;
                return avgDarts > 0 ? $"{avgDarts:F1}" : "-";
            }
            catch
            {
                return "-";
            }
        }
    }
    
    /// <summary>
    /// ✅ ERWEITERT: Alle High Finish Scores als durch | getrennte Liste
    /// </summary>
    public string HighFinishScores
    {
        get
        {
            try
            {
                var allFinishes = _playerStatistics.MatchStatistics
                    .SelectMany(m => m.HighFinishDetails)
                    .Where(hf => hf.Finish > 0)
                    .Select(hf => hf.Finish)
                    .OrderByDescending(f => f)
                    .Distinct()
                    .ToList();
                
                return allFinishes.Any() ? string.Join(" | ", allFinishes) : "-";
            }
            catch
            {
                return "-";
            }
        }
    }

    /// <summary>
    /// ✅ NEU: Schnellstes Match formatiert (MM:SS)
    /// </summary>
    public string FastestMatchFormatted
    {
        get
        {
            if (FastestMatch == TimeSpan.Zero) return "-";
            return FastestMatch.TotalMinutes < 1 ? 
                $"0:{FastestMatch.Seconds:D2}" : 
                $"{(int)FastestMatch.TotalMinutes}:{FastestMatch.Seconds:D2}";
        }
    }

    /// <summary>
    /// Formatierte Anzahl der 26er-Details (wird nicht mehr verwendet)
    /// </summary>
    public string Score26Details => TotalScore26 > 0 ? 
        $"{TotalScore26}x 26" : "0";

    /// <summary>
    /// Detaillierte Match-Informationen für Tooltips oder Details-View
    /// </summary>
    public string MatchDetails => $"Gespielte Matches: {TotalMatches}\n" +
                                 $"Gewonnen: {MatchesWon} ({MatchWinRateFormatted})\n" +
                                 $"Verloren: {MatchesLost}\n" +
                                 $"Turnier Average: {TournamentAverage}\n" +
                                 $"Beste/Schlechteste: {BestAverageFormatted}/{WorstAverage:F1}\n" +
                                 $"Höchster Leg Average: {HighestLegAverage}\n" +
                                 $"180er: {TotalMaximums}\n" +
                                 $"High Finishes: {TotalHighFinishes} (Scores: {HighFinishScores})\n" +
                                 $"Schlechte Scores (≤26): {TotalScore26}\n" +
                                 $"Checkouts: {TotalCheckouts}\n" +
                                 $"Wenigste Darts/Finish: {FewestDartsToFinish}\n" +
                                 $"∅ Darts/Checkout: {AverageDartsPerCheckout}\n" +
                                 $"Schnellstes Match: {FastestMatchFormatted}\n" +
                                 $"Wenigste Würfe: {FewestThrowsInMatch}";

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Sortierungstypen für Spieler-Statistiken
/// </summary>
public enum PlayerStatisticsSortType
{
    Name,
    Average,
    TotalMatches,
    WinRate,
    Maximums,
    HighFinishes
}