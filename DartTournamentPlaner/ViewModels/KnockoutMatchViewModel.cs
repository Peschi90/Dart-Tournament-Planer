using System.ComponentModel;
using System.Runtime.CompilerServices;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.ViewModels;

/// <summary>
/// ViewModel für KnockoutMatch mit UI-spezifischen Properties
/// </summary>
public class KnockoutMatchViewModel : INotifyPropertyChanged
{
    private readonly KnockoutMatch _match;
    private readonly TournamentClass _tournament;
    private bool _showGiveByeButton;
    private bool _showUndoByeButton;
    private string _byeStatusMessage = "";
     
    public KnockoutMatchViewModel(KnockoutMatch match, TournamentClass tournament)
    {
        _match = match;
        _tournament = tournament;
        UpdateByeButtonsVisibility();
        
        // Subscribe to match property changes
        _match.PropertyChanged += OnMatchPropertyChanged;
    }

    private void OnMatchPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Update UI when match properties change
        if (e.PropertyName is nameof(KnockoutMatch.Status) or nameof(KnockoutMatch.Player1) or nameof(KnockoutMatch.Player2) or nameof(KnockoutMatch.Winner))
        {
            UpdateByeButtonsVisibility();
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(ScoreDisplay));
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(RoundDisplay));
            
            // ? NEU: Propagate Status changes for LED indicator updates
            OnPropertyChanged(nameof(Status));
        }
    }

    private void UpdateByeButtonsVisibility()
    {
        var status = _tournament.GetMatchByeUIStatus(_match.Id);
        ShowGiveByeButton = status.ShowGiveByeButton;
        ShowUndoByeButton = status.ShowUndoByeButton;
        ByeStatusMessage = status.StatusMessage;
    }

    // Delegate properties from KnockoutMatch
    public int Id => _match.Id;
    public KnockoutMatch Match => _match;
    public string DisplayName => _match.DisplayName;
    public string ScoreDisplay => _match.ScoreDisplay;
    public string StatusDisplay => _match.StatusDisplay;
    public string RoundDisplay => _match.RoundDisplay;
    
    // ? NEU: Expose Status for LED indicator binding
    public MatchStatus Status => _match.Status;

    // UI-specific properties for bye buttons
    public bool ShowGiveByeButton
    {
        get => _showGiveByeButton;
        private set
        {
            if (_showGiveByeButton != value)
            {
                _showGiveByeButton = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ShowUndoByeButton
    {
        get => _showUndoByeButton;
        private set
        {
            if (_showUndoByeButton != value)
            {
                _showUndoByeButton = value;
                OnPropertyChanged();
            }
        }
    }

    public string ByeStatusMessage
    {
        get => _byeStatusMessage;
        private set
        {
            if (_byeStatusMessage != value)
            {
                _byeStatusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}