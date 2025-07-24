using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

public enum MatchStatus
{
    NotStarted,
    InProgress,
    Finished,
    Bye
}

public class Match : INotifyPropertyChanged
{
    private int _id;
    private Player? _player1;
    private Player? _player2;
    private int _player1Sets = 0;
    private int _player2Sets = 0;
    private int _player1Legs = 0;
    private int _player2Legs = 0;
    private MatchStatus _status = MatchStatus.NotStarted;
    private Player? _winner;
    private bool _isBye = false;
    private DateTime? _startTime;
    private DateTime? _endTime;
    private string _notes = string.Empty;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged();
        }
    }

    public Player? Player1
    {
        get => _player1;
        set
        {
            _player1 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public Player? Player2
    {
        get => _player2;
        set
        {
            _player2 = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
            UpdateByeStatus();
        }
    }

    public int Player1Sets
    {
        get => _player1Sets;
        set
        {
            _player1Sets = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    public int Player2Sets
    {
        get => _player2Sets;
        set
        {
            _player2Sets = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    public int Player1Legs
    {
        get => _player1Legs;
        set
        {
            _player1Legs = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    public int Player2Legs
    {
        get => _player2Legs;
        set
        {
            _player2Legs = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ScoreDisplay));
        }
    }

    public MatchStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusDisplay));
        }
    }

    public Player? Winner
    {
        get => _winner;
        set
        {
            _winner = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WinnerDisplay));
        }
    }

    public bool IsBye
    {
        get => _isBye;
        set
        {
            _isBye = value;
            OnPropertyChanged();
            if (value)
            {
                Status = MatchStatus.Bye;
            }
        }
    }

    public DateTime? StartTime
    {
        get => _startTime;
        set
        {
            _startTime = value;
            OnPropertyChanged();
        }
    }

    public DateTime? EndTime
    {
        get => _endTime;
        set
        {
            _endTime = value;
            OnPropertyChanged();
        }
    }

    public string Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged();
        }
    }

    // Display Properties
    public string DisplayName => IsBye ? $"{Player1?.Name ?? "TBD"} - Freilos" : $"{Player1?.Name ?? "TBD"} vs {Player2?.Name ?? "TBD"}";

    public string ScoreDisplay
    {
        get
        {
            if (IsBye) return "Freilos";
            if (Status == MatchStatus.NotStarted) return "-:-";
            
            return Player1Sets > 0 || Player2Sets > 0 
                ? $"{Player1Sets}:{Player2Sets} ({Player1Legs}:{Player2Legs})"
                : $"{Player1Legs}:{Player2Legs}";
        }
    }

    public string StatusDisplay => Status switch
    {
        MatchStatus.NotStarted => "Nicht gestartet",
        MatchStatus.InProgress => "Läuft",
        MatchStatus.Finished => "Beendet",
        MatchStatus.Bye => "Freilos",
        _ => "Unbekannt"
    };

    public string WinnerDisplay => Winner?.Name ?? (IsBye ? Player1?.Name ?? "" : "");

    private void UpdateByeStatus()
    {
        IsBye = Player2 == null;
    }

    public void SetResult(int player1Sets, int player2Sets, int player1Legs, int player2Legs)
    {
        Player1Sets = player1Sets;
        Player2Sets = player2Sets;
        Player1Legs = player1Legs;
        Player2Legs = player2Legs;

        // Determine winner
        if (player1Sets > player2Sets || (player1Sets == player2Sets && player1Legs > player2Legs))
        {
            Winner = Player1;
        }
        else if (player2Sets > player1Sets || (player1Sets == player2Sets && player2Legs > player1Legs))
        {
            Winner = Player2;
        }

        Status = MatchStatus.Finished;
        EndTime = DateTime.Now;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}