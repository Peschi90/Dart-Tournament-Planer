using System.Text.Json.Serialization;

namespace DartTournamentPlaner.Services.PowerScore;

/// <summary>
/// Repräsentiert eine einzelne Dart-Wurf
/// </summary>
public class DartThrow
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = "";
    
    [JsonPropertyName("multiplier")]
    public int Multiplier { get; set; }
    
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("displayValue")]
    public string DisplayValue { get; set; } = "";
}

/// <summary>
/// Repräsentiert eine Runde mit 3 Würfen
/// </summary>
public class ThrowRound
{
    [JsonPropertyName("round")]
    public int Round { get; set; }
    
    [JsonPropertyName("darts")]
    public List<DartThrow> Darts { get; set; } = new();
    
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// WebSocket Message für PowerScoring Updates vom Hub
/// Enthält alle Details des Scoring-Prozesses
/// </summary>
public class PowerScoringHubMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    [JsonPropertyName("tournamentId")]
    public string TournamentId { get; set; } = "";
    
    [JsonPropertyName("participantId")]
    public string ParticipantId { get; set; } = "";
    
    [JsonPropertyName("playerName")]
    public string PlayerName { get; set; } = "";
    
    // Basis-Scores
    [JsonPropertyName("totalScore")]
    public int TotalScore { get; set; }
    
    [JsonPropertyName("rounds")]
    public int Rounds { get; set; }
    
    [JsonPropertyName("average")]
    public double Average { get; set; }
    
    // Erweiterte Statistiken
    [JsonPropertyName("highestThrow")]
    public int HighestThrow { get; set; }
    
    [JsonPropertyName("totalDarts")]
    public int TotalDarts { get; set; }
    
    // Detaillierte Wurf-History
    [JsonPropertyName("throwHistory")]
    public List<ThrowRound> ThrowHistory { get; set; } = new();
    
    // Timestamps
    [JsonPropertyName("sessionStartTime")]
    public DateTime? SessionStartTime { get; set; }
    
    [JsonPropertyName("completionTime")]
    public DateTime? CompletionTime { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    // Submission Info
    [JsonPropertyName("submittedVia")]
    public string? SubmittedVia { get; set; }
    
    [JsonPropertyName("submittedAt")]
    public DateTime? SubmittedAt { get; set; }
    
    /// <summary>
    /// Berechnet die Dauer der Session in Sekunden
    /// </summary>
    public double? GetSessionDuration()
    {
        if (SessionStartTime.HasValue && CompletionTime.HasValue)
        {
            return (CompletionTime.Value - SessionStartTime.Value).TotalSeconds;
        }
        return null;
    }
    
    /// <summary>
    /// Gibt die höchste geworfene Punktzahl als String zurück
    /// </summary>
    public string GetHighestThrowDisplay()
    {
        if (HighestThrow == 180)
            return "180 (Maximum!)";
        return HighestThrow.ToString();
    }
}
