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
    
    /// <summary>
    /// Beschreibung des Turniers
    /// </summary>
    public string? TournamentDescription { get; set; }

    /// <summary>
    /// Veranstaltungsort des Turniers
    /// </summary>
    public string? TournamentLocation { get; set; }

    /// <summary>
    /// Startzeit des Turniers (ISO 8601)
    /// </summary>
    public string? TournamentStartTimeIso { get; set; }

    /// <summary>
    /// Gesamtspielerzahl (optional, für Hub-Registrierung)
    /// </summary>
    public int TournamentTotalPlayers { get; set; }

    /// <summary>
    /// Feature-Flags
    /// </summary>
    public bool FeaturePowerScoring { get; set; }
    public bool FeatureQrRegistration { get; set; }
    public bool FeaturePublicView { get; set; } = true;

    public List<TournamentClass> TournamentClasses { get; set; } = new List<TournamentClass>();
    public DateTime LastModified { get; set; } = DateTime.Now;
    public string Version { get; set; } = "1.0";
}