using System.ComponentModel.DataAnnotations;

namespace DartTournamentPlaner.API.Models;

/// <summary>
/// DTO für Tournament-Daten in der API
/// </summary>
public class TournamentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TournamentClassDto> Classes { get; set; } = new();
}

/// <summary>
/// DTO für TournamentClass-Daten in der API
/// </summary>
public class TournamentClassDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrentPhase { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
    public int GroupCount { get; set; }
    public int MatchCount { get; set; }
    public GameRulesDto GameRules { get; set; } = new(); 
    public List<GroupDto> Groups { get; set; } = new();
    public List<MatchDto> FinalsMatches { get; set; } = new();
    public List<KnockoutMatchDto> KnockoutMatches { get; set; } = new();
}

/// <summary>
/// DTO für GameRules-Daten in der API
/// </summary>
public class GameRulesDto
{
    public int GamePoints { get; set; }
    public int SetsToWin { get; set; }
    public int LegsToWin { get; set; }
    public int LegsPerSet { get; set; }
    public string PostGroupPhaseMode { get; set; } = string.Empty;
    public int QualifyingPlayersPerGroup { get; set; }
}

/// <summary>
/// DTO für Group-Daten in der API
/// </summary>
public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PlayerDto> Players { get; set; } = new();
    public List<MatchDto> Matches { get; set; } = new();
    public List<PlayerStandingDto> Standings { get; set; } = new();
}

/// <summary>
/// DTO für Player-Daten in der API
/// </summary>
public class PlayerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// DTO für Match-Daten in der API
/// </summary>
public class MatchDto
{
    public int Id { get; set; }
    public PlayerDto? Player1 { get; set; }
    public PlayerDto? Player2 { get; set; }
    public int Player1Sets { get; set; }
    public int Player2Sets { get; set; }
    public int Player1Legs { get; set; }
    public int Player2Legs { get; set; }
    public string Status { get; set; } = string.Empty;
    public PlayerDto? Winner { get; set; }
    public bool IsBye { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int? ClassId { get; set; }
    public string? ClassName { get; set; }
    public string? MatchType { get; set; }
    // ERWEITERT: Group Information für eindeutige Match-Identifikation
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
}

/// <summary>
/// DTO für KnockoutMatch-Daten in der API
/// </summary>
public class KnockoutMatchDto : MatchDto
{
    public string BracketType { get; set; } = string.Empty;
    public string Round { get; set; } = string.Empty;
    public int Position { get; set; }
    public int? SourceMatch1Id { get; set; }
    public int? SourceMatch2Id { get; set; }
    public bool Player1FromWinner { get; set; }
    public bool Player2FromWinner { get; set; }
}

/// <summary>
/// DTO für PlayerStanding-Daten in der API
/// </summary>
public class PlayerStandingDto
{
    public PlayerDto Player { get; set; } = new();
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int SetsWon { get; set; }
    public int SetsLost { get; set; }
    public int LegsWon { get; set; }
    public int LegsLost { get; set; }
    public int Points { get; set; }
    public int Position { get; set; }
}

/// <summary>
/// DTO für Match-Ergebnis Updates
/// </summary>
public class MatchResultDto
{
    [Required]
    public int MatchId { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Player1Sets { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Player2Sets { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Player1Legs { get; set; }
    
    [Range(0, int.MaxValue)]
    public int Player2Legs { get; set; }
    
    public string? Notes { get; set; }
    
    // ?? HINZUGEFÜGT: Group-Information für eindeutige Match-Identifikation
    public int? ClassId { get; set; }
    public string? ClassName { get; set; }
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    public string? MatchType { get; set; }
}