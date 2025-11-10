using System.Collections.ObjectModel;

namespace DartTournamentPlaner.Models;

public class TournamentData
{
    /// <summary>
    /// Eindeutige Tournament-ID für Hub-Synchronisation
    /// Wird beim Registrieren mit dem Hub gesetzt
    /// </summary>
    public string? TournamentId { get; set; }
    
    /// <summary>
    /// Name des Turniers
    /// </summary>
    public string? TournamentName { get; set; }
    
    public List<TournamentClass> TournamentClasses { get; set; } = new List<TournamentClass>();
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string Version { get; set; } = "1.0";
}