using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Linq;

namespace DartTournamentPlaner.Models.Statistics;

/// <summary>
/// Repräsentiert die Dart-Statistiken eines einzelnen Matches für einen Spieler
/// </summary>
public class PlayerMatchStatistics : INotifyPropertyChanged
{
    private string _matchId = string.Empty;
    private string _playerName = string.Empty;
    private double _average = 0.0;
    private int _legs = 0;
    private int _sets = 0;
    private int _totalThrows = 0;
    private int _totalScore = 0;
    private int _maximums = 0;
    private int _highFinishes = 0;
    private int _score26Count = 0;
    private int _checkouts = 0;
    private DateTime _matchDate = DateTime.Now;
    private string _opponent = string.Empty;
    private bool _isWinner = false;
    private string _matchType = string.Empty;
    private TimeSpan _matchDuration = TimeSpan.Zero;

    /// <summary>
    /// Eindeutige Match-ID (UUID oder numerische ID)
    /// </summary>
    public string MatchId
    {
        get => _matchId;
        set
        {
            _matchId = value;
            OnPropertyChanged();
        }
    }

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
    /// Durchschnittliche Punktzahl pro Dart-Wurf
    /// </summary>
    public double Average
    {
        get => _average;
        set
        {
            _average = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl gewonnener Legs in diesem Match
    /// </summary>
    public int Legs
    {
        get => _legs;
        set
        {
            _legs = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl gewonnener Sets in diesem Match
    /// </summary>
    public int Sets
    {
        get => _sets;
        set
        {
            _sets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gesamtanzahl der Dart-Würfe in diesem Match
    /// </summary>
    public int TotalThrows
    {
        get => _totalThrows;
        set
        {
            _totalThrows = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gesamtpunktzahl in diesem Match
    /// </summary>
    public int TotalScore
    {
        get => _totalScore;
        set
        {
            _totalScore = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Anzahl der 180er-Würfe (Maximum Score)
    /// </summary>
    public int Maximums
    {
        get => _maximums;
        set
        {
            _maximums = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Detaillierte Informationen zu 180er-Würfen
    /// </summary>
    public List<MaximumDetail> MaximumDetails { get; set; } = new List<MaximumDetail>();

    /// <summary>
    /// Anzahl der High Finishes (≥100 Punkte zum Beenden)
    /// </summary>
    public int HighFinishes
    {
        get => _highFinishes;
        set
        {
            _highFinishes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Detaillierte Informationen zu High Finishes
    /// </summary>
    public List<HighFinishDetail> HighFinishDetails { get; set; } = new List<HighFinishDetail>();

    /// <summary>
    /// Anzahl der 26-Punkte-Würfe (schlechtester Score mit 3 Darts)
    /// </summary>
    public int Score26Count
    {
        get => _score26Count;
        set
        {
            _score26Count = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Detaillierte Informationen zu 26er-Würfen
    /// </summary>
    public List<Score26Detail> Score26Details { get; set; } = new List<Score26Detail>();

    /// <summary>
    /// Anzahl der erfolgreichen Checkouts (Leg-Beendigungen)
    /// </summary>
    public int Checkouts
    {
        get => _checkouts;
        set
        {
            _checkouts = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Detaillierte Informationen zu Checkouts
    /// </summary>
    public List<CheckoutDetail> CheckoutDetails { get; set; } = new List<CheckoutDetail>();

    // ✅ NEU: Erweiterte Dart-Statistiken
    
    /// <summary>
    /// Leg-Averages für jedes gespielte Leg
    /// </summary>
    public List<LegAverage> LegAverages { get; set; } = new List<LegAverage>();

    /// <summary>
    /// ✅ NEU: Detaillierte Leg-Daten für gewonnene Legs (inkl. benötigte Darts)
    /// </summary>
    public List<LegData> LegData { get; set; } = new List<LegData>();

    /// <summary>
    /// Durchschnittlicher Leg-Average über alle Legs
    /// </summary>
    public double AverageLegAverage { get; set; } = 0.0;

    /// <summary>
    /// Höchster Leg-Average in diesem Match
    /// </summary>
    [JsonIgnore]
    public double HighestLegAverage => LegAverages.Count > 0 ? LegAverages.Max(la => la.Average) : 0.0;

    /// <summary>
    /// Höchster High Finish Score in diesem Match
    /// </summary>
    [JsonIgnore]
    public int HighestFinishScore => HighFinishDetails.Count > 0 ? HighFinishDetails.Max(hf => hf.Finish) : 0;

    /// <summary>
    /// Wenigste Darts bis zum Finish (kürzestes Checkout)
    /// </summary>
    [JsonIgnore]
    public int FewestDartsToFinish => CheckoutDetails.Count > 0 ? CheckoutDetails.Min(cd => cd.Darts.Count(d => d > 0)) : 0;

    /// <summary>
    /// Durchschnittliche Darts pro Checkout
    /// </summary>
    [JsonIgnore]
    public double AverageDartsPerCheckout => CheckoutDetails.Count > 0 ? 
        CheckoutDetails.Average(cd => cd.Darts.Count(d => d > 0)) : 0.0;

    /// <summary>
    /// ✅ NEU: Wenigste Darts für ein gewonnenes Leg
    /// </summary>
    [JsonIgnore]
    public int FewestDartsPerLeg => LegData.Count > 0 && LegData.Any(ld => ld.Won) ? 
        LegData.Where(ld => ld.Won).Min(ld => ld.Darts) : 0;

    /// <summary>
    /// ✅ NEU: Durchschnittliche Darts pro gewonnenem Leg
    /// </summary>
    [JsonIgnore]
    public double AverageDartsPerLeg => LegData.Count > 0 && LegData.Any(ld => ld.Won) ? 
        LegData.Where(ld => ld.Won).Average(ld => ld.Darts) : 0.0;

    /// <summary>
    /// ✅ NEU: Alle Leg-Darts als durch | getrennte Liste (z.B. "6 | 9 | 12") - nur gewonnene Legs
    /// </summary>
    [JsonIgnore]
    public string LegDartsFormatted => LegData.Count > 0 && LegData.Any(ld => ld.Won) ? 
        string.Join(" | ", LegData.Where(ld => ld.Won).Select(ld => ld.Darts)) : "-";

    /// <summary>
    /// Datum und Uhrzeit des Matches
    /// </summary>
    public DateTime MatchDate
    {
        get => _matchDate;
        set
        {
            _matchDate = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Name des Gegners
    /// </summary>
    public string Opponent
    {
        get => _opponent;
        set
        {
            _opponent = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gibt an, ob der Spieler dieses Match gewonnen hat
    /// </summary>
    public bool IsWinner
    {
        get => _isWinner;
        set
        {
            _isWinner = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Art des Matches (z.B. "Group", "Knockout-Winner-Quarterfinal")
    /// </summary>
    public string MatchType
    {
        get => _matchType;
        set
        {
            _matchType = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Dauer des Matches
    /// </summary>
    public TimeSpan MatchDuration
    {
        get => _matchDuration;
        set
        {
            _matchDuration = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Berechnet die durchschnittliche Punktzahl pro 3-Dart-Aufnahme
    /// </summary>
    [JsonIgnore]
    public double AveragePerTurn => TotalThrows > 0 ? (TotalScore / (double)TotalThrows) * 3 : 0.0;

    /// <summary>
    /// Berechnet die Quote der High Finishes pro Leg
    /// </summary>
    [JsonIgnore]
    public double HighFinishRate => Legs > 0 ? (double)HighFinishes / Legs : 0.0;

    /// <summary>
    /// Berechnet die Quote der 180er pro Leg
    /// </summary>
    [JsonIgnore]
    public double MaximumRate => Legs > 0 ? (double)Maximums / Legs : 0.0;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{PlayerName} vs {Opponent}: Avg {Average:F1}, {Maximums}x180, {HighFinishes} HF ({MatchDate:dd.MM.yyyy})";
    }
}

/// <summary>
/// ✅ NEU: Repräsentiert den Average eines einzelnen Legs
/// </summary>
public class LegAverage
{
    /// <summary>
    /// Leg-Nummer (1-basiert)
    /// </summary>
    public int LegNumber { get; set; }

    /// <summary>
    /// Average für dieses Leg
    /// </summary>
    public double Average { get; set; }

    /// <summary>
    /// Erzielte Punkte in diesem Leg
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Anzahl Würfe in diesem Leg
    /// </summary>
    public int Throws { get; set; }

    /// <summary>
    /// Zeitstempel des Leg-Abschlusses
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// ✅ NEU: Repräsentiert detaillierte Informationen zu einem gewonnenen Leg
/// </summary>
public class LegData
{
    /// <summary>
    /// Leg-Nummer (1-basiert)
    /// </summary>
    public int LegNumber { get; set; }

    /// <summary>
    /// Average für dieses Leg
    /// </summary>
    public double Average { get; set; }

    /// <summary>
    /// Anzahl benötigter Darts um das Leg zu gewinnen
    /// </summary>
    public int Darts { get; set; }

    /// <summary>
    /// Erzielte Punkte in diesem Leg
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// ✅ NEU: Gibt an, ob dieses Leg gewonnen wurde
    /// </summary>
    public bool Won { get; set; }

    /// <summary>
    /// Zeitstempel des Leg-Abschlusses
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Formatierte Anzeige: "Leg X: Y Darts (Ø Z)"
    /// </summary>
    [JsonIgnore]
    public string FormattedDisplay => $"Leg {LegNumber}: {Darts} Darts (Ø {Average:F1}){(Won ? " ✓" : "")}";
}

/// <summary>
/// Detaillierte Informationen zu einem 180er-Wurf
/// </summary>
public class MaximumDetail
{
    /// <summary>
    /// Die drei geworfenen Dart-Werte
    /// </summary>
    public List<int> Darts { get; set; } = new List<int>();

    /// <summary>
    /// Gesamtpunktzahl (sollte 180 sein)
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Zeitstempel des Wurfs
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Detaillierte Informationen zu einem High Finish
/// </summary>
public class HighFinishDetail
{
    /// <summary>
    /// Punktzahl des Finishs
    /// </summary>
    public int Finish { get; set; }

    /// <summary>
    /// Die drei geworfenen Dart-Werte
    /// </summary>
    public List<int> Darts { get; set; } = new List<int>();

    /// <summary>
    /// Verbleibende Punktzahl vor dem Finish
    /// </summary>
    public int RemainingScore { get; set; }

    /// <summary>
    /// Zeitstempel des Finishs
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Detaillierte Informationen zu einem 26er-Wurf (schlechtester Score)
/// </summary>
public class Score26Detail
{
    /// <summary>
    /// Die drei geworfenen Dart-Werte
    /// </summary>
    public List<int> Darts { get; set; } = new List<int>();

    /// <summary>
    /// Gesamtpunktzahl (sollte 26 oder weniger sein)
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Zeitstempel des Wurfs
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Detaillierte Informationen zu einem Checkout
/// </summary>
public class CheckoutDetail
{
    /// <summary>
    /// Punktzahl des Checkouts
    /// </summary>
    public int Finish { get; set; }

    /// <summary>
    /// Die drei geworfenen Dart-Werte
    /// </summary>
    public List<int> Darts { get; set; } = new List<int>();

    /// <summary>
    /// Ob es ein Double-Out war
    /// </summary>
    public bool DoubleOut { get; set; }

    /// <summary>
    /// Zeitstempel des Checkouts
    /// </summary>
    public DateTime Timestamp { get; set; }
}