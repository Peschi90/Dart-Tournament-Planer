using System;
using System.Text.Json.Serialization;

namespace DartTournamentPlaner.Models.HubSync;

/// <summary>
/// Repräsentiert eingehende Hub-Sync-Daten (Turnier, Spieler, Regeln etc.)
/// </summary>
public class HubTournamentSyncPayload
{
    /// <summary>Vom Hub mitgeschickter Lizenzschlüssel (Planner-Lizenz)</summary>
    public string? LicenseKey { get; set; }

    /// <summary>ID des Turniers im Hub</summary>
    public string? TournamentId { get; set; }

    /// <summary>Name/Beschreibung des Turniers</summary>
    public string? TournamentName { get; set; }

    /// <summary>Anzahl der mitgeschickten Spieler/Teilnehmer</summary>
    public int? PlayerCount { get; set; }

    /// <summary>Anzahl der Matches im Payload</summary>
    public int? MatchCount { get; set; }

    /// <summary>Quelle der Nachricht (z. B. "hub-websocket")</summary>
    public string? Source { get; set; }

    /// <summary>Zeitpunkt des Empfangs</summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Rohes JSON des Payloads (zur Anzeige/Speicherung)</summary>
    public string RawJson { get; set; } = string.Empty;

    /// <summary>Kurzer Hinweistext für die Anzeige</summary>
    public string? Summary { get; set; }

    [JsonIgnore]
    public bool HasLicenseKey => !string.IsNullOrWhiteSpace(LicenseKey);
}
