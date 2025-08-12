using System.Collections.ObjectModel;

namespace DartTournamentPlaner.Models;

public class TournamentData
{
    public List<TournamentClass> TournamentClasses { get; set; } = new List<TournamentClass>();
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string Version { get; set; } = "1.0";
} 