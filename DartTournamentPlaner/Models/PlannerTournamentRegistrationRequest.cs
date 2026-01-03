using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Message-Payload für die neue Planner-WebSocket-Registrierung
/// Entspricht dem erwarteten Format laut Tournament Hub Dokumentation
/// </summary>
public class PlannerTournamentRegistrationRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "planner-register-tournament";

    [JsonPropertyName("tournamentId")]
    public string TournamentId { get; set; } = string.Empty;

    [JsonPropertyName("licenseKey")]
    public string LicenseKey { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "active";

    [JsonPropertyName("totalPlayers")]
    public int TotalPlayers { get; set; } = 0;

    [JsonPropertyName("classes")]
    public object? Classes { get; set; } = new List<object>();

    [JsonPropertyName("gameRules")]
    public object? GameRules { get; set; } = new List<object>();

    [JsonPropertyName("winnerBracketRules")]
    public object? WinnerBracketRules { get; set; } = new { };

    [JsonPropertyName("loserBracketRules")]
    public object? LoserBracketRules { get; set; } = new { };

    [JsonPropertyName("participants")]
    public object? Participants { get; set; } = new List<object>();

    [JsonPropertyName("matches")]
    public object? Matches { get; set; } = new List<object>();

    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

    [JsonPropertyName("features")]
    public PlannerRegistrationFeatures Features { get; set; } = new();
}

public class PlannerRegistrationFeatures
{
    [JsonPropertyName("powerScoring")]
    public bool PowerScoring { get; set; }

    [JsonPropertyName("qrRegistration")]
    public bool QrRegistration { get; set; }

    [JsonPropertyName("liveScoring")]
    public bool LiveScoring { get; set; }

    [JsonPropertyName("statistics")]
    public bool Statistics { get; set; }

    [JsonPropertyName("publicView")]
    public bool PublicView { get; set; } = true;

    [JsonPropertyName("koRound")]
    public bool KoRound { get; set; }

    [JsonPropertyName("loserBracket")]
    public bool LoserBracket { get; set; }
}
