using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

public class Group : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _id;
    private bool _matchesGenerated = false;

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public bool MatchesGenerated
    {
        get => _matchesGenerated;
        set
        {
            _matchesGenerated = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
    public ObservableCollection<Match> Matches { get; set; } = new ObservableCollection<Match>();

    public void GenerateRoundRobinMatches()
    {
        Matches.Clear();
        var playerList = Players.ToList();
        int matchId = 1;

        // Generate all possible combinations (round robin)
        for (int i = 0; i < playerList.Count; i++)
        {
            for (int j = i + 1; j < playerList.Count; j++)
            {
                var match = new Match
                {
                    Id = matchId++,
                    Player1 = playerList[i],
                    Player2 = playerList[j],
                    Status = MatchStatus.NotStarted
                };
                Matches.Add(match);
            }
        }

        // Handle odd number of players (bye)
        if (playerList.Count % 2 == 1)
        {
            foreach (var player in playerList)
            {
                var byeMatch = new Match
                {
                    Id = matchId++,
                    Player1 = player,
                    Player2 = null,
                    IsBye = true,
                    Status = MatchStatus.Bye,
                    Winner = player
                };
                Matches.Add(byeMatch);
            }
        }

        MatchesGenerated = true;
    }

    public List<PlayerStanding> GetStandings()
    {
        var standings = new List<PlayerStanding>();

        // Create standings for each player
        foreach (var player in Players)
        {
            var standing = new PlayerStanding { Player = player };
            
            // Get all matches involving this player - Use ID comparison instead of object reference
            var playerMatches = Matches.Where(m => 
                (m.Player1?.Id == player.Id) || (m.Player2?.Id == player.Id)).ToList();
            
            // Calculate statistics
            var finishedMatches = playerMatches.Where(m => m.Status == MatchStatus.Finished).ToList();
            var finishedNonByeMatches = finishedMatches.Where(m => !m.IsBye).ToList();
            
            // Matches played (excluding byes)
            standing.MatchesPlayed = finishedNonByeMatches.Count;
            
            // Wins (including byes but only finished matches) - Use ID comparison
            standing.Wins = finishedMatches.Count(m => m.Winner?.Id == player.Id);
            
            // Losses (only non-bye finished matches where player didn't win)
            standing.Losses = finishedNonByeMatches.Count(m => m.Winner?.Id != player.Id && m.Winner != null);
            
            // Draws (non-bye finished matches with no winner)
            standing.Draws = finishedNonByeMatches.Count(m => m.Winner == null);
            
            // Calculate sets and legs for finished matches
            standing.SetsWon = 0;
            standing.SetsLost = 0;
            standing.LegsWon = 0;
            standing.LegsLost = 0;
            
            foreach (var match in finishedMatches)
            {
                if (match.Player1?.Id == player.Id)
                {
                    // Player is Player1
                    standing.SetsWon += match.Player1Sets;
                    standing.SetsLost += match.Player2Sets;
                    standing.LegsWon += match.Player1Legs;
                    standing.LegsLost += match.Player2Legs;
                }
                else if (match.Player2?.Id == player.Id)
                {
                    // Player is Player2
                    standing.SetsWon += match.Player2Sets;
                    standing.SetsLost += match.Player1Sets;
                    standing.LegsWon += match.Player2Legs;
                    standing.LegsLost += match.Player1Legs;
                }
            }
            
            // Calculate points (3 for win, 1 for draw, 0 for loss)
            standing.Points = standing.Wins * 3 + standing.Draws * 1;
            
            standings.Add(standing);
        }

        // Sort by points (descending), then by set difference (descending), then by leg difference (descending)
        var sortedStandings = standings
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.SetDifference)
            .ThenByDescending(s => s.LegDifference)
            .ThenBy(s => s.Player?.Name ?? "")  // Tie-breaker by name
            .ToList();

        // Assign positions
        for (int i = 0; i < sortedStandings.Count; i++)
        {
            sortedStandings[i].Position = i + 1;
        }

        return sortedStandings;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{Name} ({Players.Count} Spieler, {Matches.Count} Spiele)";
    }
}