using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DartTournamentPlaner.Models.Statistics;

/// <summary>
/// Repr�sentiert die gesammelten Statistiken eines Spielers �ber alle Matches hinweg
/// </summary>
public class PlayerStatistics : INotifyPropertyChanged
{
    private string _playerName = string.Empty;
    private int _totalMatches = 0;
    private int _matchesWon = 0;
    private int _matchesLost = 0;
    private int _totalLegs = 0;
    private int _legsWon = 0;
    private int _legsLost = 0;
    private int _totalSets = 0;
    private int _setsWon = 0;
    private int _setsLost = 0;

    /// <summary>
    /// Name des Spielers
    /// </summary>
    public string PlayerName
    {
        get => _playerName;
        set
        {
            _playerName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Alle Match-Statistiken dieses Spielers
    /// </summary>
    public List<PlayerMatchStatistics> MatchStatistics { get; set; } = new List<PlayerMatchStatistics>();

    /// <summary>
    /// Gesamtanzahl gespielter Matches
    /// </summary>
    public int TotalMatches
    {
        get => _totalMatches;
        set
        {
            _totalMatches = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl gewonnener Matches
    /// </summary>
    public int MatchesWon
    {
        get => _matchesWon;
        set
        {
            _matchesWon = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl verlorener Matches
    /// </summary>
    public int MatchesLost
    {
        get => _matchesLost;
        set
        {
            _matchesLost = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gesamtanzahl gespielter Legs
    /// </summary>
    public int TotalLegs
    {
        get => _totalLegs;
        set
        {
            _totalLegs = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl gewonnener Legs
    /// </summary>
    public int LegsWon
    {
        get => _legsWon;
        set
        {
            _legsWon = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl verlorener Legs
    /// </summary>
    public int LegsLost
    {
        get => _legsLost;
        set
        {
            _legsLost = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gesamtanzahl gespielter Sets
    /// </summary>
    public int TotalSets
    {
        get => _totalSets;
        set
        {
            _totalSets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl gewonnener Sets
    /// </summary>
    public int SetsWon
    {
        get => _setsWon;
        set
        {
            _setsWon = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl verlorener Sets
    /// </summary>
    public int SetsLost
    {
        get => _setsLost;
        set
        {
            _setsLost = value;
            OnPropertyChanged();
        }
    }

    // Berechnete Eigenschaften

    /// <summary>
    /// Match-Gewinnrate in Prozent
    /// </summary>
    [JsonIgnore]
    public double MatchWinRate => TotalMatches > 0 ? (double)MatchesWon / TotalMatches * 100 : 0.0;

    /// <summary>
    /// Leg-Gewinnrate in Prozent
    /// </summary>
    [JsonIgnore]
    public double LegWinRate => TotalLegs > 0 ? (double)LegsWon / TotalLegs * 100 : 0.0;

    /// <summary>
    /// Set-Gewinnrate in Prozent
    /// </summary>
    [JsonIgnore]
    public double SetWinRate => TotalSets > 0 ? (double)SetsWon / TotalSets * 100 : 0.0;

    /// <summary>
    /// Durchschnittlicher Average �ber alle Matches (Turnier Average)
    /// </summary>
    [JsonIgnore]
    public double TournamentAverage => MatchStatistics.Count > 0 ? MatchStatistics.Average(m => m.Average) : 0.0;

    /// <summary>
    /// Alias f�r TournamentAverage (f�r R�ckw�rtskompatibilit�t)
    /// </summary>
    [JsonIgnore]
    public double OverallAverage => TournamentAverage;

    /// <summary>
    /// Bester Average in einem Match
    /// </summary>
    [JsonIgnore]
    public double BestAverage => MatchStatistics.Count > 0 ? MatchStatistics.Max(m => m.Average) : 0.0;

    /// <summary>
    /// Schlechtester Average in einem Match
    /// </summary>
    [JsonIgnore]
    public double WorstAverage => MatchStatistics.Count > 0 ? MatchStatistics.Min(m => m.Average) : 0.0;

    // ? NEU: Erweiterte Leg-Average Statistiken

    /// <summary>
    /// H�chster Leg-Average �ber alle Matches
    /// </summary>
    [JsonIgnore]
    public double HighestLegAverage
    {
        get
        {
            var allLegAverages = MatchStatistics
                .SelectMany(m => m.LegAverages)
                .Where(la => la.Average > 0);
            return allLegAverages.Any() ? allLegAverages.Max(la => la.Average) : 0.0;
        }
    }

    /// <summary>
    /// Durchschnittlicher Leg-Average �ber alle Legs in allen Matches
    /// </summary>
    [JsonIgnore]
    public double AverageLegAverage
    {
        get
        {
            var allLegAverages = MatchStatistics
                .SelectMany(m => m.LegAverages)
                .Where(la => la.Average > 0);
            return allLegAverages.Any() ? allLegAverages.Average(la => la.Average) : 0.0;
        }
    }

    /// <summary>
    /// Gesamtanzahl der 180er-W�rfe
    /// </summary>
    [JsonIgnore]
    public int TotalMaximums => MatchStatistics.Sum(m => m.Maximums);

    /// <summary>
    /// Durchschnittliche Anzahl 180er pro Match
    /// </summary>
    [JsonIgnore]
    public double AverageMaximumsPerMatch => TotalMatches > 0 ? (double)TotalMaximums / TotalMatches : 0.0;

    /// <summary>
    /// Gesamtanzahl der High Finishes
    /// </summary>
    [JsonIgnore]
    public int TotalHighFinishes => MatchStatistics.Sum(m => m.HighFinishes);

    /// <summary>
    /// Durchschnittliche Anzahl High Finishes pro Match
    /// </summary>
    [JsonIgnore]
    public double AverageHighFinishesPerMatch => TotalMatches > 0 ? (double)TotalHighFinishes / TotalMatches : 0.0;

    // ? NEU: High Finish Statistiken

    /// <summary>
    /// H�chster High Finish Score �ber alle Matches
    /// </summary>
    [JsonIgnore]
    public int HighestFinishScore
    {
        get
        {
            var allFinishes = MatchStatistics
                .SelectMany(m => m.HighFinishDetails)
                .Where(hf => hf.Finish > 0);
            return allFinishes.Any() ? allFinishes.Max(hf => hf.Finish) : 0;
        }
    }

    /// <summary>
    /// Durchschnittlicher High Finish Score
    /// </summary>
    [JsonIgnore]
    public double AverageFinishScore
    {
        get
        {
            var allFinishes = MatchStatistics
                .SelectMany(m => m.HighFinishDetails)
                .Where(hf => hf.Finish > 0);
            return allFinishes.Any() ? allFinishes.Average(hf => hf.Finish) : 0.0;
        }
    }

    // ? NEU: Checkout-Effizienz Statistiken

    /// <summary>
    /// Wenigste Darts bis zum Finish �ber alle Matches
    /// </summary>
    [JsonIgnore]
    public int FewestDartsToFinish
    {
        get
        {
            var allCheckouts = MatchStatistics
                .SelectMany(m => m.CheckoutDetails)
                .Where(cd => cd.Darts.Any(d => d > 0));
            
            if (!allCheckouts.Any()) return 0;
            
            return allCheckouts.Min(cd => cd.Darts.Count(d => d > 0));
        }
    }

    /// <summary>
    /// Durchschnittliche Darts pro Checkout �ber alle Matches
    /// </summary>
    [JsonIgnore]
    public double AverageDartsPerCheckout
    {
        get
        {
            var allCheckouts = MatchStatistics
                .SelectMany(m => m.CheckoutDetails)
                .Where(cd => cd.Darts.Any(d => d > 0));
            
            if (!allCheckouts.Any()) return 0.0;
            
            return allCheckouts.Average(cd => cd.Darts.Count(d => d > 0));
        }
    }

    /// <summary>
    /// Checkout-Quote (Checkouts pro gewonnenes Leg)
    /// </summary>
    [JsonIgnore]
    public double CheckoutRate => LegsWon > 0 ? (double)TotalCheckouts / LegsWon : 0.0;

    /// <summary>
    /// Gesamtanzahl der 26er-W�rfe
    /// </summary>
    [JsonIgnore]
    public int TotalScore26 => MatchStatistics.Sum(m => m.Score26Count);

    /// <summary>
    /// Gesamtanzahl der Checkouts
    /// </summary>
    [JsonIgnore]
    public int TotalCheckouts => MatchStatistics.Sum(m => m.Checkouts);

    /// <summary>
    /// Gesamte Spielzeit
    /// </summary>
    [JsonIgnore]
    public TimeSpan TotalPlayTime => TimeSpan.FromTicks(MatchStatistics.Sum(m => m.MatchDuration.Ticks));

    /// <summary>
    /// Durchschnittliche Match-Dauer
    /// </summary>
    [JsonIgnore]
    public TimeSpan AverageMatchDuration => TotalMatches > 0 ? 
        TimeSpan.FromTicks(TotalPlayTime.Ticks / TotalMatches) : TimeSpan.Zero;

    // ? NEU: Wurf-Effizienz Statistiken

    /// <summary>
    /// Gesamtanzahl aller W�rfe �ber alle Matches
    /// </summary>
    [JsonIgnore]
    public int TotalThrows => MatchStatistics.Sum(m => m.TotalThrows);

    /// <summary>
    /// Durchschnittliche W�rfe pro Leg
    /// </summary>
    [JsonIgnore]
    public double AverageThrowsPerLeg => LegsWon > 0 ? (double)TotalThrows / LegsWon : 0.0;

    /// <summary>
    /// Wenigste W�rfe in einem gewonnenen Leg (beste Effizienz)
    /// </summary>
    [JsonIgnore]
    public int BestLegEfficiency
    {
        get
        {
            var wonLegAverages = MatchStatistics
                .Where(m => m.IsWinner)
                .SelectMany(m => m.LegAverages)
                .Where(la => la.Throws > 0);
            
            return wonLegAverages.Any() ? wonLegAverages.Min(la => la.Throws) : 0;
        }
    }

    /// <summary>
    /// Letztes gespieltes Match
    /// </summary>
    [JsonIgnore]
    public DateTime? LastMatchDate => MatchStatistics.Count > 0 ? 
        MatchStatistics.Max(m => m.MatchDate) : null;

    /// <summary>
    /// Erstes gespieltes Match
    /// </summary>
    [JsonIgnore]
    public DateTime? FirstMatchDate => MatchStatistics.Count > 0 ? 
        MatchStatistics.Min(m => m.MatchDate) : null;

    /// <summary>
    /// Aktualisiert alle berechneten Statistiken basierend auf den Match-Daten
    /// </summary>
    public void RecalculateStatistics()
    {
        TotalMatches = MatchStatistics.Count;
        MatchesWon = MatchStatistics.Count(m => m.IsWinner);
        MatchesLost = TotalMatches - MatchesWon;

        LegsWon = MatchStatistics.Sum(m => m.Legs);
        LegsLost = CalculateLegsLost();
        TotalLegs = LegsWon + LegsLost;

        SetsWon = MatchStatistics.Sum(m => m.Sets);
        SetsLost = CalculateSetsLost();
        TotalSets = SetsWon + SetsLost;

        // Trigge PropertyChanged f�r alle berechneten Properties
        OnPropertyChanged(nameof(MatchWinRate));
        OnPropertyChanged(nameof(LegWinRate));
        OnPropertyChanged(nameof(SetWinRate));
        OnPropertyChanged(nameof(TournamentAverage));
        OnPropertyChanged(nameof(OverallAverage));
        OnPropertyChanged(nameof(BestAverage));
        OnPropertyChanged(nameof(WorstAverage));
        OnPropertyChanged(nameof(HighestLegAverage));
        OnPropertyChanged(nameof(AverageLegAverage));
        OnPropertyChanged(nameof(TotalMaximums));
        OnPropertyChanged(nameof(AverageMaximumsPerMatch));
        OnPropertyChanged(nameof(TotalHighFinishes));
        OnPropertyChanged(nameof(AverageHighFinishesPerMatch));
        OnPropertyChanged(nameof(HighestFinishScore));
        OnPropertyChanged(nameof(AverageFinishScore));
        OnPropertyChanged(nameof(FewestDartsToFinish));
        OnPropertyChanged(nameof(AverageDartsPerCheckout));
        OnPropertyChanged(nameof(CheckoutRate));
        OnPropertyChanged(nameof(TotalScore26));
        OnPropertyChanged(nameof(TotalCheckouts));
        OnPropertyChanged(nameof(TotalPlayTime));
        OnPropertyChanged(nameof(AverageMatchDuration));
        OnPropertyChanged(nameof(TotalThrows));
        OnPropertyChanged(nameof(AverageThrowsPerLeg));
        OnPropertyChanged(nameof(BestLegEfficiency));
        OnPropertyChanged(nameof(LastMatchDate));
        OnPropertyChanged(nameof(FirstMatchDate));
    }

    /// <summary>
    /// F�gt eine neue Match-Statistik hinzu und aktualisiert die Gesamtstatistiken
    /// </summary>
    public void AddMatchStatistics(PlayerMatchStatistics matchStats)
    {
        if (matchStats == null) return;

        // Pr�fe ob Match bereits existiert
        var existingMatch = MatchStatistics.FirstOrDefault(m => m.MatchId == matchStats.MatchId);
        if (existingMatch != null)
        {
            // Update existing match
            var index = MatchStatistics.IndexOf(existingMatch);
            MatchStatistics[index] = matchStats;
        }
        else
        {
            // Add new match
            MatchStatistics.Add(matchStats);
        }

        RecalculateStatistics();
    }

    /// <summary>
    /// Berechnet die Anzahl verlorener Legs basierend auf Match-Ergebnissen
    /// Da wir nicht alle Leg-Informationen haben, verwenden wir Heuristiken
    /// </summary>
    private int CalculateLegsLost()
    {
        int totalLegsLost = 0;
        
        foreach (var match in MatchStatistics)
        {
            if (!match.IsWinner)
            {
                // Wenn verloren, sch�tze die Legs des Gegners
                // Bei einem Standard "First to X Legs" Match hat der Gewinner X Legs
                // Wir k�nnen nicht genau wissen wie viele, aber verwenden eine Sch�tzung
                totalLegsLost += EstimateOpponentLegs(match);
            }
        }
        
        return totalLegsLost;
    }

    /// <summary>
    /// Sch�tzt die Anzahl der Legs des Gegners basierend auf Match-Typ und Ergebnis
    /// </summary>
    private int EstimateOpponentLegs(PlayerMatchStatistics match)
    {
        // Heuristik: Bei verlorenen Matches hat der Gegner mindestens so viele Legs wie n�tig zum Gewinnen
        // Standard ist meist "First to 2 Legs" oder �hnlich
        if (match.MatchType.Contains("First to"))
        {
            // Versuche Zielanzahl aus MatchType zu extrahieren
            // Fallback: Sch�tze 2 Legs als Standard
            return 2;
        }
        
        // Weitere Heuristiken k�nnen hier hinzugef�gt werden
        return Math.Max(match.Legs + 1, 2); // Minimum 2, oder eigene Legs + 1
    }

    /// <summary>
    /// Berechnet die Anzahl verlorener Sets analog zu verlorenen Legs
    /// </summary>
    private int CalculateSetsLost()
    {
        int totalSetsLost = 0;
        
        foreach (var match in MatchStatistics)
        {
            if (!match.IsWinner && match.Sets > 0)
            {
                // �hnliche Logik wie bei Legs
                totalSetsLost += EstimateOpponentSets(match);
            }
        }
        
        return totalSetsLost;
    }

    /// <summary>
    /// Sch�tzt die Anzahl der Sets des Gegners
    /// </summary>
    private int EstimateOpponentSets(PlayerMatchStatistics match)
    {
        return Math.Max(match.Sets + 1, 1);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{PlayerName}: {TotalMatches} Matches, {MatchWinRate:F1}% WR, Avg {TournamentAverage:F1}";
    }
}