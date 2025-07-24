using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

public class PlayerStanding : INotifyPropertyChanged
{
    private Player? _player;
    private int _position = 0;
    private int _matchesPlayed = 0;
    private int _wins = 0;
    private int _losses = 0;
    private int _draws = 0;
    private int _points = 0;
    private int _setsWon = 0;
    private int _setsLost = 0;
    private int _legsWon = 0;
    private int _legsLost = 0;

    public Player? Player
    {
        get => _player;
        set
        {
            _player = value;
            OnPropertyChanged();
        }
    }

    public int Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPropertyChanged();
        }
    }

    public int MatchesPlayed
    {
        get => _matchesPlayed;
        set
        {
            _matchesPlayed = value;
            OnPropertyChanged();
        }
    }

    public int Wins
    {
        get => _wins;
        set
        {
            _wins = value;
            OnPropertyChanged();
        }
    }

    public int Losses
    {
        get => _losses;
        set
        {
            _losses = value;
            OnPropertyChanged();
        }
    }

    public int Draws
    {
        get => _draws;
        set
        {
            _draws = value;
            OnPropertyChanged();
        }
    }

    public int Points
    {
        get => _points;
        set
        {
            _points = value;
            OnPropertyChanged();
        }
    }

    public int SetsWon
    {
        get => _setsWon;
        set
        {
            _setsWon = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SetDifference));
        }
    }

    public int SetsLost
    {
        get => _setsLost;
        set
        {
            _setsLost = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SetDifference));
        }
    }

    public int LegsWon
    {
        get => _legsWon;
        set
        {
            _legsWon = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LegDifference));
        }
    }

    public int LegsLost
    {
        get => _legsLost;
        set
        {
            _legsLost = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LegDifference));
        }
    }

    public int SetDifference => SetsWon - SetsLost;
    public int LegDifference => LegsWon - LegsLost;

    public string RecordDisplay => $"{Wins}-{Draws}-{Losses}";
    public string SetRecordDisplay => $"{SetsWon}:{SetsLost}";
    public string LegRecordDisplay => $"{LegsWon}:{LegsLost}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}