using Microsoft.AspNetCore.Mvc;
using DartTournamentPlaner.API.Services;
using DartTournamentPlaner.API.Models;

namespace DartTournamentPlaner.API.Controllers;

/// <summary>
/// API Controller für Tournament-Operationen
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentApiService _tournamentService;
    private readonly ITournamentSyncService _syncService;

    public TournamentsController(ITournamentApiService tournamentService, ITournamentSyncService syncService)
    {
        _tournamentService = tournamentService;
        _syncService = syncService;
    }

    /// <summary>
    /// Holt alle verfügbaren Turniere
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TournamentDto>>>> GetTournaments()
    {
        var result = await _tournamentService.GetTournamentsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Holt ein spezifisches Turnier
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TournamentDto>>> GetTournament(int id)
    {
        var result = await _tournamentService.GetTournamentAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Holt das aktuell laufende Turnier (Live-Daten)
    /// </summary>
    [HttpGet("current")]
    public ActionResult<ApiResponse<TournamentDto>> GetCurrentTournament()
    {
        try
        {
            if (!_syncService.IsApiRunning)
            {
                return BadRequest(ApiResponse<TournamentDto>.ErrorResult("Keine aktive API-Verbindung zur Hauptanwendung"));
            }

            var currentData = _syncService.GetCurrentTournamentData();
            if (currentData == null)
            {
                return NotFound(ApiResponse<TournamentDto>.ErrorResult("Keine aktiven Turnierdaten gefunden"));
            }

            // Verwende eine temp ID für Live-Daten
            var dto = new TournamentDto
            {
                Id = 0, // Live tournament
                Name = "Live Tournament",
                CreatedAt = DateTime.UtcNow,
                LastModified = currentData.LastModified,
                Status = "Live",
                Classes = currentData.TournamentClasses.Select(tc => new TournamentClassDto
                {
                    Id = tc.Id,
                    Name = tc.Name,
                    CurrentPhase = tc.CurrentPhase?.PhaseType.ToString() ?? "GroupPhase",
                    GameRules = new GameRulesDto
                    {
                        GamePoints = tc.GameRules.GamePoints,
                        SetsToWin = tc.GameRules.SetsToWin,
                        LegsToWin = tc.GameRules.LegsToWin,
                        LegsPerSet = tc.GameRules.LegsPerSet,
                        PostGroupPhaseMode = tc.GameRules.PostGroupPhaseMode.ToString(),
                        QualifyingPlayersPerGroup = tc.GameRules.QualifyingPlayersPerGroup
                    },
                    Groups = tc.Groups.Select(g => new GroupDto
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Players = g.Players.Select(p => new PlayerDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            Email = p.Email
                        }).ToList(),
                        Matches = g.Matches.Select(m => new MatchDto
                        {
                            Id = m.Id,
                            Player1 = m.Player1 != null ? new PlayerDto { Id = m.Player1.Id, Name = m.Player1.Name, Email = m.Player1.Email } : null,
                            Player2 = m.Player2 != null ? new PlayerDto { Id = m.Player2.Id, Name = m.Player2.Name, Email = m.Player2.Email } : null,
                            Player1Sets = m.Player1Sets,
                            Player2Sets = m.Player2Sets,
                            Player1Legs = m.Player1Legs,
                            Player2Legs = m.Player2Legs,
                            Status = m.Status.ToString(),
                            Winner = m.Winner != null ? new PlayerDto { Id = m.Winner.Id, Name = m.Winner.Name, Email = m.Winner.Email } : null,
                            IsBye = m.IsBye,
                            StartTime = m.StartTime,
                            EndTime = m.EndTime,
                            Notes = m.Notes
                        }).ToList(),
                        Standings = g.GetStandings().Select((ps, index) => new PlayerStandingDto
                        {
                            Player = new PlayerDto { Id = ps.Player.Id, Name = ps.Player.Name, Email = ps.Player.Email },
                            Wins = ps.Wins,
                            Losses = ps.Losses,
                            SetsWon = ps.SetsWon,
                            SetsLost = ps.SetsLost,
                            LegsWon = ps.LegsWon,
                            LegsLost = ps.LegsLost,
                            Points = ps.Points,
                            Position = index + 1
                        }).ToList()
                    }).ToList(),
                    FinalsMatches = tc.GetFinalsMatches().Select(m => new MatchDto
                    {
                        Id = m.Id,
                        Player1 = m.Player1 != null ? new PlayerDto { Id = m.Player1.Id, Name = m.Player1.Name, Email = m.Player1.Email } : null,
                        Player2 = m.Player2 != null ? new PlayerDto { Id = m.Player2.Id, Name = m.Player2.Name, Email = m.Player2.Email } : null,
                        Player1Sets = m.Player1Sets,
                        Player2Sets = m.Player2Sets,
                        Player1Legs = m.Player1Legs,
                        Player2Legs = m.Player2Legs,
                        Status = m.Status.ToString(),
                        Winner = m.Winner != null ? new PlayerDto { Id = m.Winner.Id, Name = m.Winner.Name, Email = m.Winner.Email } : null,
                        IsBye = m.IsBye,
                        StartTime = m.StartTime,
                        EndTime = m.EndTime,
                        Notes = m.Notes
                    }).ToList(),
                    KnockoutMatches = tc.GetWinnerBracketMatches().Concat(tc.GetLoserBracketMatches()).Select(km => new KnockoutMatchDto
                    {
                        Id = km.Id,
                        Player1 = km.Player1 != null ? new PlayerDto { Id = km.Player1.Id, Name = km.Player1.Name, Email = km.Player1.Email } : null,
                        Player2 = km.Player2 != null ? new PlayerDto { Id = km.Player2.Id, Name = km.Player2.Name, Email = km.Player2.Email } : null,
                        Player1Sets = km.Player1Sets,
                        Player2Sets = km.Player2Sets,
                        Player1Legs = km.Player1Legs,
                        Player2Legs = km.Player2Legs,
                        Status = km.Status.ToString(),
                        Winner = km.Winner != null ? new PlayerDto { Id = km.Winner.Id, Name = km.Winner.Name, Email = km.Winner.Email } : null,
                        IsBye = km.Status == DartTournamentPlaner.Models.MatchStatus.Bye,
                        StartTime = km.StartTime,
                        EndTime = km.EndTime,
                        Notes = km.Notes,
                        BracketType = km.BracketType.ToString(),
                        Round = km.Round.ToString(),
                        Position = km.Position,
                        SourceMatch1Id = km.SourceMatch1?.Id,
                        SourceMatch2Id = km.SourceMatch2?.Id,
                        Player1FromWinner = km.Player1FromWinner,
                        Player2FromWinner = km.Player2FromWinner
                    }).ToList()
                }).ToList()
            };

            return Ok(ApiResponse<TournamentDto>.SuccessResult(dto, "Live tournament data"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TournamentDto>.ErrorResult($"Fehler beim Laden der Live-Turnierdaten: {ex.Message}"));
        }
    }

    /// <summary>
    /// Status der API-Verbindung prüfen
    /// </summary>
    [HttpGet("status")]
    public ActionResult<ApiResponse<object>> GetApiStatus()
    {
        var status = new
        {
            IsConnected = _syncService.IsApiRunning,
            HasActiveTournament = _syncService.GetCurrentTournamentData() != null,
            Timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.SuccessResult(status, "API Status"));
    }

    /// <summary>
    /// Holt alle Klassen eines Turniers
    /// </summary>
    [HttpGet("{id}/classes")]
    public ActionResult<ApiResponse<List<TournamentClassDto>>> GetTournamentClasses(int id)
    {
        try
        {
            if (!_syncService.IsApiRunning)
            {
                return BadRequest(ApiResponse<List<TournamentClassDto>>.ErrorResult("Keine aktive API-Verbindung zur Hauptanwendung"));
            }

            var currentData = _syncService.GetCurrentTournamentData();
            if (currentData == null)
            {
                return NotFound(ApiResponse<List<TournamentClassDto>>.ErrorResult("Keine aktiven Turnierdaten gefunden"));
            }

            var classesDto = currentData.TournamentClasses.Select(tc => new TournamentClassDto
            {
                Id = tc.Id,
                Name = tc.Name,
                CurrentPhase = tc.CurrentPhase?.PhaseType.ToString() ?? "GroupPhase",
                PlayerCount = tc.Groups.Sum(g => g.Players.Count),
                GroupCount = tc.Groups.Count,
                MatchCount = tc.Groups.Sum(g => g.Matches.Count) +
                           (tc.CurrentPhase?.FinalsGroup?.Matches.Count ?? 0) +
                           (tc.CurrentPhase?.WinnerBracket?.Count ?? 0) +
                           (tc.CurrentPhase?.LoserBracket?.Count ?? 0),
                GameRules = new GameRulesDto
                {
                    GamePoints = tc.GameRules.GamePoints,
                    SetsToWin = tc.GameRules.SetsToWin,
                    LegsToWin = tc.GameRules.LegsToWin,
                    LegsPerSet = tc.GameRules.LegsPerSet,
                    PostGroupPhaseMode = tc.GameRules.PostGroupPhaseMode.ToString(),
                    QualifyingPlayersPerGroup = tc.GameRules.QualifyingPlayersPerGroup
                }
            }).ToList();

            return Ok(ApiResponse<List<TournamentClassDto>>.SuccessResult(classesDto, $"{classesDto.Count} Turnierklassen gefunden"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<TournamentClassDto>>.ErrorResult($"Fehler beim Laden der Turnierklassen: {ex.Message}"));
        }
    }

    /// <summary>
    /// Holt alle Matches eines Turniers, optional gefiltert nach Klasse
    /// </summary>
    [HttpGet("{id}/matches")]
    public ActionResult<ApiResponse<List<MatchDto>>> GetTournamentMatches(int id, int? classId = null)
    {
        try
        {
            if (!_syncService.IsApiRunning)
            {
                return BadRequest(ApiResponse<List<MatchDto>>.ErrorResult("Keine aktive API-Verbindung zur Hauptanwendung"));
            }

            var currentData = _syncService.GetCurrentTournamentData();
            if (currentData == null)
            {
                return NotFound(ApiResponse<List<MatchDto>>.ErrorResult("Keine aktiven Turnierdaten gefunden"));
            }

            var allMatches = new List<MatchDto>();
            var classesToProcess = classId.HasValue
                ? currentData.TournamentClasses.Where(tc => tc.Id == classId.Value).ToList()
                : currentData.TournamentClasses.ToList();

            foreach (var tournamentClass in classesToProcess)
            {
                // Sammle alle Gruppenmmatches
                foreach (var group in tournamentClass.Groups)
                {
                    var groupMatches = group.Matches.Select(m => ConvertMatchToDto(m, tournamentClass.Id, tournamentClass.Name, "Group", group.Name));
                    allMatches.AddRange(groupMatches);
                }

                // Sammle Finals-Matches
                if (tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    var finalsMatches = tournamentClass.CurrentPhase.FinalsGroup.Matches
                        .Select(m => ConvertMatchToDto(m, tournamentClass.Id, tournamentClass.Name, "Finals", "Finals"));
                    allMatches.AddRange(finalsMatches);
                }

                // Sammle Winner Bracket Matches
                if (tournamentClass.CurrentPhase?.WinnerBracket != null)
                {
                    var winnerBracketMatches = tournamentClass.CurrentPhase.WinnerBracket
                        .Select(m => ConvertKnockoutMatchToDto(m, tournamentClass.Id, tournamentClass.Name));
                    allMatches.AddRange(winnerBracketMatches);
                }

                // Sammle Loser Bracket Matches
                if (tournamentClass.CurrentPhase?.LoserBracket != null)
                {
                    var loserBracketMatches = tournamentClass.CurrentPhase.LoserBracket
                        .Select(m => ConvertKnockoutMatchToDto(m, tournamentClass.Id, tournamentClass.Name));
                    allMatches.AddRange(loserBracketMatches);
                }
            }

            var message = classId.HasValue
                ? $"{allMatches.Count} Matches für Klasse {classId} gefunden"
                : $"{allMatches.Count} Matches insgesamt gefunden";

            return Ok(ApiResponse<List<MatchDto>>.SuccessResult(allMatches, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<MatchDto>>.ErrorResult($"Fehler beim Laden der Matches: {ex.Message}"));
        }
    }

    // Helper methods for match conversion
    private static MatchDto ConvertMatchToDto(DartTournamentPlaner.Models.Match match, int classId, string className, string matchType, string? groupName = null)
    {
        return new MatchDto
        {
            Id = match.Id,
            Player1 = match.Player1 != null ? new PlayerDto { Id = match.Player1.Id, Name = match.Player1.Name, Email = match.Player1.Email } : null,
            Player2 = match.Player2 != null ? new PlayerDto { Id = match.Player2.Id, Name = match.Player2.Name, Email = match.Player2.Email } : null,
            Player1Sets = match.Player1Sets,
            Player2Sets = match.Player2Sets,
            Player1Legs = match.Player1Legs,
            Player2Legs = match.Player2Legs,
            Status = match.Status.ToString(),
            Winner = match.Winner != null ? new PlayerDto { Id = match.Winner.Id, Name = match.Winner.Name, Email = match.Winner.Email } : null,
            IsBye = match.IsBye,
            StartTime = match.StartTime,
            EndTime = match.EndTime,
            Notes = match.Notes,
            ClassId = classId,
            ClassName = className,
            MatchType = matchType,
            // ERWEITERT: Group Information hinzufügen
            GroupName = groupName
        };
    }

    private static MatchDto ConvertKnockoutMatchToDto(DartTournamentPlaner.Models.KnockoutMatch knockoutMatch, int classId, string className)
    {
        return new MatchDto
        {
            Id = knockoutMatch.Id,
            Player1 = knockoutMatch.Player1 != null ? new PlayerDto { Id = knockoutMatch.Player1.Id, Name = knockoutMatch.Player1.Name, Email = knockoutMatch.Player1.Email } : null,
            Player2 = knockoutMatch.Player2 != null ? new PlayerDto { Id = knockoutMatch.Player2.Id, Name = knockoutMatch.Player2.Name, Email = knockoutMatch.Player2.Email } : null,
            Player1Sets = knockoutMatch.Player1Sets,
            Player2Sets = knockoutMatch.Player2Sets,
            Player1Legs = knockoutMatch.Player1Legs,
            Player2Legs = knockoutMatch.Player2Legs,
            Status = knockoutMatch.Status.ToString(),
            Winner = knockoutMatch.Winner != null ? new PlayerDto { Id = knockoutMatch.Winner.Id, Name = knockoutMatch.Winner.Name, Email = knockoutMatch.Winner.Email } : null,
            IsBye = false,
            StartTime = knockoutMatch.StartTime,
            EndTime = knockoutMatch.EndTime,
            Notes = knockoutMatch.Notes,
            ClassId = classId,
            ClassName = className,
            MatchType = $"Knockout-{knockoutMatch.BracketType}-{knockoutMatch.Round}",
            // Knockout matches haben keine Gruppen
            GroupName = null
        };
    }
}