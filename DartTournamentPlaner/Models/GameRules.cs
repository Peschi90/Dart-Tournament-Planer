using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

public enum GameMode
{
    Points501,
    Points401,
    Points301
}

public enum FinishMode
{
    SingleOut,
    DoubleOut
}

public enum PostGroupPhaseMode
{
    None,                    // Nur Gruppenphase
    RoundRobinFinals,       // Finalrunde: Beste X Spieler im Round Robin
    KnockoutBracket          // KO-System mit Single/Double Elimination
}

public enum KnockoutMode
{
    SingleElimination,       // Einfaches KO-System
    DoubleElimination       // Doppeltes KO-System (Winner + Loser Bracket)
}

public class GameRules : INotifyPropertyChanged
{
    private GameMode _gameMode = GameMode.Points501;
    private FinishMode _finishMode = FinishMode.DoubleOut;
    private int _legsToWin = 3;
    private bool _playWithSets = false;
    private int _setsToWin = 3;
    private int _legsPerSet = 3;
    
    // Post-Group Phase Settings
    private PostGroupPhaseMode _postGroupPhaseMode = PostGroupPhaseMode.None;
    private int _qualifyingPlayersPerGroup = 2;
    private KnockoutMode _knockoutMode = KnockoutMode.SingleElimination;
    private bool _includeGroupPhaseLosersBracket = false;

    public GameMode GameMode
    {
        get => _gameMode;
        set
        {
            _gameMode = value;
            OnPropertyChanged();
        }
    }

    public FinishMode FinishMode
    {
        get => _finishMode;
        set
        {
            _finishMode = value;
            OnPropertyChanged();
        }
    }

    public int LegsToWin
    {
        get => _legsToWin;
        set
        {
            _legsToWin = value;
            OnPropertyChanged();
        }
    }

    public bool PlayWithSets
    {
        get => _playWithSets;
        set
        {
            _playWithSets = value;
            OnPropertyChanged();
        }
    }

    public int SetsToWin
    {
        get => _setsToWin;
        set
        {
            _setsToWin = value;
            OnPropertyChanged();
        }
    }

    public int LegsPerSet
    {
        get => _legsPerSet;
        set
        {
            _legsPerSet = value;
            OnPropertyChanged();
        }
    }

    // Post-Group Phase Properties
    public PostGroupPhaseMode PostGroupPhaseMode
    {
        get => _postGroupPhaseMode;
        set
        {
            _postGroupPhaseMode = value;
            OnPropertyChanged();
        }
    }

    public int QualifyingPlayersPerGroup
    {
        get => _qualifyingPlayersPerGroup;
        set
        {
            _qualifyingPlayersPerGroup = Math.Max(1, value);
            OnPropertyChanged();
        }
    }

    public KnockoutMode KnockoutMode
    {
        get => _knockoutMode;
        set
        {
            _knockoutMode = value;
            OnPropertyChanged();
        }
    }

    public bool IncludeGroupPhaseLosersBracket
    {
        get => _includeGroupPhaseLosersBracket;
        set
        {
            _includeGroupPhaseLosersBracket = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        var mode = GameMode switch
        {
            GameMode.Points501 => "501",
            GameMode.Points401 => "401",
            GameMode.Points301 => "301",
            _ => "501"
        };

        var finish = FinishMode switch
        {
            FinishMode.SingleOut => "Single Out",
            FinishMode.DoubleOut => "Double Out",
            _ => "Double Out"
        };

        var baseRules = PlayWithSets 
            ? $"{mode} {finish}, First to {SetsToWin} Sets ({LegsPerSet} Legs per Set)"
            : $"{mode} {finish}, First to {LegsToWin} Legs";

        var postGroupInfo = PostGroupPhaseMode switch
        {
            PostGroupPhaseMode.RoundRobinFinals => $" + Finals: Top {QualifyingPlayersPerGroup} per Group (Round Robin)",
            PostGroupPhaseMode.KnockoutBracket => KnockoutMode == KnockoutMode.DoubleElimination 
                ? $" + Finals: Top {QualifyingPlayersPerGroup} per Group (Double Elimination)"
                : $" + Finals: Top {QualifyingPlayersPerGroup} per Group (Single Elimination)",
            _ => ""
        };

        return baseRules + postGroupInfo;
    }
}