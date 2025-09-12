using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

public enum TournamentPhaseType
{
    GroupPhase,
    RoundRobinFinals,
    KnockoutPhase 
}

public class TournamentPhase : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private TournamentPhaseType _phaseType;
    private bool _isActive = false;
    private bool _isCompleted = false;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public TournamentPhaseType PhaseType
    {
        get => _phaseType;
        set
        {
            _phaseType = value;
            OnPropertyChanged();
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged();
        }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            _isCompleted = value;
            OnPropertyChanged();
        }
    }

    // For Group Phase
    public ObservableCollection<Group> Groups { get; set; } = new ObservableCollection<Group>();

    // For Round Robin Finals
    public Group? FinalsGroup { get; set; }

    // For Knockout Phase
    public ObservableCollection<KnockoutMatch> WinnerBracket { get; set; } = new ObservableCollection<KnockoutMatch>();
    public ObservableCollection<KnockoutMatch> LoserBracket { get; set; } = new ObservableCollection<KnockoutMatch>();

    // Qualified players from previous phase
    public ObservableCollection<Player> QualifiedPlayers { get; set; } = new ObservableCollection<Player>();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool CanProceedToNextPhase()
    {
        try
        {
            //System.Diagnostics.Debug.WriteLine($"=== TournamentPhase.CanProceedToNextPhase START ===");
            //System.Diagnostics.Debug.WriteLine($"TournamentPhase.CanProceedToNextPhase: Phase = {PhaseType}");
            
            bool result = PhaseType switch
            {
                TournamentPhaseType.GroupPhase => CheckGroupPhaseComplete(),
                TournamentPhaseType.RoundRobinFinals => FinalsGroup?.Matches.All(m => m.Status == MatchStatus.Finished || m.IsBye) ?? false,
                TournamentPhaseType.KnockoutPhase => WinnerBracket.All(m => m.Status == MatchStatus.Finished) && LoserBracket.All(m => m.Status == MatchStatus.Finished),
                _ => false
            };
            
            //System.Diagnostics.Debug.WriteLine($"TournamentPhase.CanProceedToNextPhase: Result = {result}");
            //System.Diagnostics.Debug.WriteLine($"=== TournamentPhase.CanProceedToNextPhase END ===");
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TournamentPhase.CanProceedToNextPhase: ERROR: {ex.Message}");
            return false;
        }
    }
    
    private bool CheckGroupPhaseComplete()
    {
        try
        {
            //System.Diagnostics.Debug.WriteLine($"=== CheckGroupPhaseComplete START ===");
            //System.Diagnostics.Debug.WriteLine($"CheckGroupPhaseComplete: Groups count = {Groups.Count}");
            
            if (Groups.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"CheckGroupPhaseComplete: No groups found");
                return false;
            }
            
            bool allGroupsComplete = true;
            
            foreach (var group in Groups)
            {
                var status = group.CheckCompletionStatus();
                
                //System.Diagnostics.Debug.WriteLine($"CheckGroupPhaseComplete: Group '{group.Name}':");
                //System.Diagnostics.Debug.WriteLine($"  - Status: {status}");
                //System.Diagnostics.Debug.WriteLine($"  - IsComplete: {status.IsComplete}");
                
                if (!status.IsComplete)
                {
                    //System.Diagnostics.Debug.WriteLine($"  - Group '{group.Name}' is NOT complete: {status.StatusMessage}");
                    allGroupsComplete = false;
                }
            }
            
            //System.Diagnostics.Debug.WriteLine($"CheckGroupPhaseComplete: All groups complete = {allGroupsComplete}");
            return allGroupsComplete;
        }
        catch (Exception ex)
        {
           System.Diagnostics.Debug.WriteLine($"CheckGroupPhaseComplete: ERROR: {ex.Message}");
            return false;
        }
    }

    public List<Player> GetQualifiedPlayers(int playersPerGroup)
    {
        try
        {
            //System.Diagnostics.Debug.WriteLine($"=== GetQualifiedPlayers START ===");
            //System.Diagnostics.Debug.WriteLine($"GetQualifiedPlayers: Phase = {PhaseType}, playersPerGroup = {playersPerGroup}");
            
            var qualifiedPlayers = new List<Player>();

            switch (PhaseType)
            {
                case TournamentPhaseType.GroupPhase:
                    //System.Diagnostics.Debug.WriteLine($"GetQualifiedPlayers: Processing {Groups.Count} groups");
                    foreach (var group in Groups)
                    {
                        var standings = group.GetStandings();
                        var groupQualified = standings.Take(playersPerGroup).Select(s => s.Player).Where(p => p != null).ToList();
                        
                        //System.Diagnostics.Debug.WriteLine($"GetQualifiedPlayers: Group '{group.Name}' - {groupQualified.Count} qualified:");
                        foreach (var player in groupQualified)
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {player.Name}");
                        }
                        
                        qualifiedPlayers.AddRange(groupQualified!);
                    }
                    break;

                case TournamentPhaseType.RoundRobinFinals:
                    if (FinalsGroup != null)
                    {
                        var standings = FinalsGroup.GetStandings();
                        qualifiedPlayers.AddRange(standings.Select(s => s.Player).Where(p => p != null)!);
                        //System.Diagnostics.Debug.WriteLine($"GetQualifiedPlayers: Finals - {qualifiedPlayers.Count} qualified");
                    }
                    break;

                case TournamentPhaseType.KnockoutPhase:
                    // Winner of the tournament
                    var finalMatch = WinnerBracket.FirstOrDefault(m => m.Round == KnockoutRound.Final || m.Round == KnockoutRound.GrandFinal);
                    if (finalMatch?.Winner != null)
                    {
                        qualifiedPlayers.Add(finalMatch.Winner);
                        //System.Diagnostics.Debug.WriteLine($"GetQualifiedPlayers: KO winner - {finalMatch.Winner.Name}");
                    }
                    break;
            }

            //System.Diagnostics.Debug.WriteLine($"GetQualifiedPlayers: Total qualified = {qualifiedPlayers.Count}");
            //System.Diagnostics.Debug.WriteLine($"=== GetQualifiedPlayers END ===");
            
            return qualifiedPlayers;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetQualifiedPlayers: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GetQualifiedPlayers: Stack trace: {ex.StackTrace}");
            return new List<Player>();
        }
    }
}