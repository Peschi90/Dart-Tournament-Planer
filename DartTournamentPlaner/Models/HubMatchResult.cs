using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Model for Hub Match Result data stored in Notes field
/// </summary>
public class HubMatchResultData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("tournamentId")]
    public string TournamentId { get; set; } = string.Empty;

    [JsonPropertyName("matchUpdate")]
    public MatchUpdateData? MatchUpdate { get; set; }

    [JsonPropertyName("statistics")]
    public MatchStatistics? Statistics { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("matchResultHighlight")]
    public bool MatchResultHighlight { get; set; }

    [JsonPropertyName("classId")]
    public int ClassId { get; set; }

    [JsonPropertyName("className")]
    public string ClassName { get; set; } = string.Empty;
}

public class MatchUpdateData
{
    [JsonPropertyName("tournamentId")]
    public string TournamentId { get; set; } = string.Empty;

    [JsonPropertyName("matchId")]
    public string MatchId { get; set; } = string.Empty;

    [JsonPropertyName("result")]
    public MatchResultData? Result { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("classId")]
    public int ClassId { get; set; }

    [JsonPropertyName("className")]
    public string ClassName { get; set; } = string.Empty;
}

public class MatchResultData
{
    [JsonPropertyName("player1Sets")]
    public int Player1Sets { get; set; }

    [JsonPropertyName("player2Sets")]
    public int Player2Sets { get; set; }

    [JsonPropertyName("player1Legs")]
    public int Player1Legs { get; set; }

    [JsonPropertyName("player2Legs")]
    public int Player2Legs { get; set; }

    [JsonPropertyName("winner")]
    public string Winner { get; set; } = string.Empty;

    [JsonPropertyName("winnerPlayerNumber")]
    public int WinnerPlayerNumber { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("submittedVia")]
    public string SubmittedVia { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("submittedAt")]
    public DateTime? SubmittedAt { get; set; }

    [JsonPropertyName("dartScoringResult")]
    public DartScoringResultData? DartScoringResult { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("player1Name")]
    public string Player1Name { get; set; } = string.Empty;

    [JsonPropertyName("player2Name")]
    public string Player2Name { get; set; } = string.Empty;

    [JsonPropertyName("matchType")]
    public string MatchType { get; set; } = string.Empty;
}

public class DartScoringResultData
{
    [JsonPropertyName("player1Stats")]
    public PlayerStatsData? Player1Stats { get; set; }

    [JsonPropertyName("player2Stats")]
    public PlayerStatsData? Player2Stats { get; set; }

    [JsonPropertyName("gameRules")]
    public GameRulesData? GameRules { get; set; }

    [JsonPropertyName("matchDuration")]
    public long MatchDuration { get; set; }

    [JsonPropertyName("totalLegs")]
    public int TotalLegs { get; set; }

    [JsonPropertyName("totalSets")]
    public int TotalSets { get; set; }

    [JsonPropertyName("startTime")]
    public DateTime? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }

    [JsonPropertyName("submittedVia")]
    public string SubmittedVia { get; set; } = string.Empty;

    [JsonPropertyName("submissionTimestamp")]
    public DateTime? SubmissionTimestamp { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

public class PlayerStatsData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("average")]
    public double Average { get; set; }

    [JsonPropertyName("legs")]
    public int Legs { get; set; }

    [JsonPropertyName("sets")]
    public int Sets { get; set; }

    [JsonPropertyName("totalThrows")]
    public int TotalThrows { get; set; }

    [JsonPropertyName("totalScore")]
    public int TotalScore { get; set; }

    [JsonPropertyName("maximums")]
    public int Maximums { get; set; }

    [JsonPropertyName("maximumDetails")]
    public List<MaximumDetail>? MaximumDetails { get; set; }

    [JsonPropertyName("highFinishes")]
    public int HighFinishes { get; set; }

    [JsonPropertyName("highFinishDetails")]
    public List<HighFinishDetail>? HighFinishDetails { get; set; }

    [JsonPropertyName("score26Count")]
    public int Score26Count { get; set; }

    [JsonPropertyName("score26Details")]
    public List<object>? Score26Details { get; set; }

    [JsonPropertyName("checkouts")]
    public int Checkouts { get; set; }

    [JsonPropertyName("checkoutDetails")]
    public List<CheckoutDetail>? CheckoutDetails { get; set; }

    [JsonPropertyName("legAverages")]
    public List<LegAverage>? LegAverages { get; set; }

    [JsonPropertyName("legAveragesCount")]
    public int LegAveragesCount { get; set; }

    [JsonPropertyName("averageLegAverage")]
    public double AverageLegAverage { get; set; }
}

public class MaximumDetail
{
    [JsonPropertyName("darts")]
    public List<int>? Darts { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
}

public class HighFinishDetail
{
    [JsonPropertyName("finish")]
    public int Finish { get; set; }

    [JsonPropertyName("darts")]
    public List<int>? Darts { get; set; }

    [JsonPropertyName("remainingScore")]
    public int RemainingScore { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
}

public class CheckoutDetail
{
    [JsonPropertyName("finish")]
    public int Finish { get; set; }

    [JsonPropertyName("darts")]
    public List<int>? Darts { get; set; }

    [JsonPropertyName("doubleOut")]
    public bool DoubleOut { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
}

public class LegAverage
{
    [JsonPropertyName("legNumber")]
    public int LegNumber { get; set; }

    [JsonPropertyName("average")]
    public double Average { get; set; }

    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("throws")]
    public int Throws { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
}

public class GameRulesData
{
    [JsonPropertyName("gameMode")]
    public string GameMode { get; set; } = string.Empty;

    [JsonPropertyName("startingScore")]
    public int StartingScore { get; set; }

    [JsonPropertyName("legsToWin")]
    public int LegsToWin { get; set; }

    [JsonPropertyName("setsToWin")]
    public int SetsToWin { get; set; }

    [JsonPropertyName("playWithSets")]
    public bool PlayWithSets { get; set; }

    [JsonPropertyName("usesSets")]
    public bool UsesSets { get; set; }

    [JsonPropertyName("doubleOut")]
    public bool DoubleOut { get; set; }
}

public class MatchStatistics
{
    [JsonPropertyName("player1")]
    public PlayerStatisticsData? Player1 { get; set; }

    [JsonPropertyName("player2")]
    public PlayerStatisticsData? Player2 { get; set; }

    [JsonPropertyName("match")]
    public MatchInfoData? Match { get; set; }
}

public class PlayerStatisticsData
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("average")]
    public double Average { get; set; }

    [JsonPropertyName("scores180")]
    public int Scores180 { get; set; }

    [JsonPropertyName("highFinishes")]
    public int HighFinishes { get; set; }

    [JsonPropertyName("highFinishScores")]
    public List<HighFinishDetail>? HighFinishScores { get; set; }

    [JsonPropertyName("scores26")]
    public int Scores26 { get; set; }

    [JsonPropertyName("checkouts")]
    public int Checkouts { get; set; }

    [JsonPropertyName("totalThrows")]
    public int TotalThrows { get; set; }

    [JsonPropertyName("totalScore")]
    public int TotalScore { get; set; }
}

public class MatchInfoData
{
    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("totalThrows")]
    public int TotalThrows { get; set; }

    [JsonPropertyName("startTime")]
    public DateTime? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }
}
