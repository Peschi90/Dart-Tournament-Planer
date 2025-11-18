using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DartTournamentPlaner.Models.PowerScore;

/// <summary>
/// Repräsentiert eine PowerScoring-Session für die Spieler-Einteilung
/// Serialisierbar für JSON-Persistierung
/// </summary>
public class PowerScoringSession : INotifyPropertyChanged
{
    // Event wird nicht serialisiert (Events werden automatisch ignoriert)
    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _tournamentId;
    private bool _isRegisteredWithHub;

    /// <summary>
    /// Eindeutige Session-ID
    /// </summary>
    public Guid SessionId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tournament-ID für Hub-Integration
    /// </summary>
    public string? TournamentId
    {
        get => _tournamentId;
        set
        {
            if (_tournamentId != value)
            {
                _tournamentId = value;
                OnPropertyChanged(nameof(TournamentId));
            }
        }
    }

    /// <summary>
    /// Gibt an, ob die Session mit dem Hub registriert ist
    /// </summary>
    public bool IsRegisteredWithHub
    {
        get => _isRegisteredWithHub;
        set
        {
            if (_isRegisteredWithHub != value)
            {
                _isRegisteredWithHub = value;
                OnPropertyChanged(nameof(IsRegisteredWithHub));
            }
        }
    }

    /// <summary>
    /// Spielerliste für die PowerScoring-Session
    /// </summary>
    public ObservableCollection<PowerScoringPlayer> Players { get; set; } = new();

    /// <summary>
    /// Gewählte Regel (Anzahl der Würfe)
    /// </summary>
    public PowerScoringRule Rule { get; set; } = PowerScoringRule.ThrowsOf3x10;

    /// <summary>
    /// Session-Status
    /// </summary>
    public PowerScoringStatus Status { get; set; } = PowerScoringStatus.Setup;

    /// <summary>
    /// Erstellungsdatum der Session
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Gibt an, ob die Session abgeschlossen ist
    /// </summary>
    public bool IsCompleted => Status == PowerScoringStatus.Completed;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Repräsentiert einen Spieler in der PowerScoring-Session
/// </summary>
public class PowerScoringPlayer : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _name = string.Empty;
    private int _totalScore;
    private double _averageScore;
    private bool _isScored;
    private string? _qrCodeUrl;

    /// <summary>
    /// Eindeutige Spieler-ID (wird als ParticipantId für Hub verwendet)
    /// </summary>
    public Guid PlayerId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Name des Spielers
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    /// <summary>
    /// Gesamtscore aller Würfe
    /// </summary>
    public int TotalScore
    {
        get => _totalScore;
        set
        {
            if (_totalScore != value)
            {
                _totalScore = value;
                OnPropertyChanged(nameof(TotalScore));
                CalculateAverage();
            }
        }
    }

    /// <summary>
    /// Durchschnittlicher Score pro Wurf (3 Darts)
    /// </summary>
    public double AverageScore
    {
        get => _averageScore;
        set
        {
            if (Math.Abs(_averageScore - value) > 0.01)
            {
                _averageScore = value;
                OnPropertyChanged(nameof(AverageScore));
            }
        }
    }

    /// <summary>
    /// Gibt an, ob der Spieler bereits gescored wurde
    /// </summary>
    public bool IsScored
    {
        get => _isScored;
        set
        {
            if (_isScored != value)
            {
                _isScored = value;
                OnPropertyChanged(nameof(IsScored));
            }
        }
    }

    /// <summary>
    /// QR-Code URL für Hub PowerScoring
    /// </summary>
    public string? QrCodeUrl
    {
        get => _qrCodeUrl;
        set
        {
            if (_qrCodeUrl != value)
            {
                _qrCodeUrl = value;
                OnPropertyChanged(nameof(QrCodeUrl));
            }
        }
    }

    /// <summary>
    /// Liste der einzelnen Wurf-Scores (je 3 Darts)
    /// </summary>
    public ObservableCollection<int> ThrowScores { get; set; } = new();

    /// <summary>
    /// Detaillierte Wurf-Historie vom Hub
    /// </summary>
    public ObservableCollection<PowerScoringRoundHistory> History { get; set; } = new();

    /// <summary>
    /// Anzahl der Würfe (abhängig von der Regel)
    /// </summary>
    public int NumberOfThrows { get; set; }
    
    /// <summary>
    /// Höchster geworfener Score in einer Runde
    /// </summary>
    public int HighestThrow { get; set; }
    
    /// <summary>
    /// Gesamtanzahl der geworfenen Darts
    /// </summary>
    public int TotalDarts { get; set; }
    
    /// <summary>
    /// Zeitpunkt des Session-Starts
    /// </summary>
    public DateTime? SessionStartTime { get; set; }
    
    /// <summary>
    /// Zeitpunkt der Completion
    /// </summary>
    public DateTime? CompletionTime { get; set; }
    
    /// <summary>
    /// Über welchen Weg wurde submitted (Socket.IO, HTTP, etc.)
    /// </summary>
    public string? SubmittedVia { get; set; }

    private void CalculateAverage()
    {
        if (NumberOfThrows > 0 && ThrowScores.Count == NumberOfThrows)
        {
            AverageScore = TotalScore / (double)NumberOfThrows;
        }
        else if (History.Count > 0)
        {
            // Berechne Average aus History wenn vorhanden
            var totalRounds = History.Count;
            if (totalRounds > 0)
            {
                AverageScore = TotalScore / (double)totalRounds;
            }
        }
        else
        {
            AverageScore = 0;
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Repräsentiert die Historie eines PowerScoring-Rounds
/// </summary>
public class PowerScoringRoundHistory : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _round;
    private int _throw1;
    private int _throw2;
    private int _throw3;
    private int _total;

    public int Round
    {
        get => _round;
        set
        {
            if (_round != value)
            {
                _round = value;
                OnPropertyChanged(nameof(Round));
            }
        }
    }

    public int Throw1
    {
        get => _throw1;
        set
        {
            if (_throw1 != value)
            {
                _throw1 = value;
                OnPropertyChanged(nameof(Throw1));
            }
        }
    }

    public int Throw2
    {
        get => _throw2;
        set
        {
            if (_throw2 != value)
            {
                _throw2 = value;
                OnPropertyChanged(nameof(Throw2));
            }
        }
    }

    public int Throw3
    {
        get => _throw3;
        set
        {
            if (_throw3 != value)
            {
                _throw3 = value;
                OnPropertyChanged(nameof(Throw3));
            }
        }
    }

    public int Total
    {
        get => _total;
        set
        {
            if (_total != value)
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }
    }
    
    /// <summary>
    /// Detaillierte Dart-Würfe (mit Number, Multiplier, DisplayValue)
    /// </summary>
    public List<DartThrowDetail> Darts { get; set; } = new();
    
    /// <summary>
    /// Timestamp der Runde
    /// </summary>
    public DateTime? Timestamp { get; set; }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Detaillierte Informationen zu einem einzelnen Dart-Wurf
/// </summary>
public class DartThrowDetail
{
    public string Number { get; set; } = "";
    public int Multiplier { get; set; }
    public int Score { get; set; }
    public string DisplayValue { get; set; } = "";
    
    public override string ToString()
    {
        return DisplayValue;
    }
}

/// <summary>
/// PowerScoring-Regeln (Anzahl der 3-Dart-Würfe)
/// </summary>
public enum PowerScoringRule
{
    ThrowsOf3x1 = 1,
    ThrowsOf3x2 = 2,
    ThrowsOf3x3 = 3,
    ThrowsOf3x4 = 4,
    ThrowsOf3x5 = 5,
    ThrowsOf3x6 = 6,
    ThrowsOf3x7 = 7,
    ThrowsOf3x8 = 8,
    ThrowsOf3x9 = 9,
    ThrowsOf3x10 = 10,
    ThrowsOf3x11 = 11,
    ThrowsOf3x12 = 12,
    ThrowsOf3x13 = 13,
    ThrowsOf3x14 = 14,
    ThrowsOf3x15 = 15
}

/// <summary>
/// Status der PowerScoring-Session
/// </summary>
public enum PowerScoringStatus
{
    /// <summary>
    /// Setup-Phase (Spieler hinzufügen, Regeln festlegen)
    /// </summary>
    Setup,

    /// <summary>
    /// Scoring-Phase (Scores eingeben - Hub-basiert mit QR-Codes)
    /// </summary>
    Scoring,

    /// <summary>
    /// Abgeschlossen (Scores berechnet, Sortierung verfügbar)
    /// </summary>
    Completed
}
