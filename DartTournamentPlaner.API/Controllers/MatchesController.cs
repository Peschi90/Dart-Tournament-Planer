using Microsoft.AspNetCore.Mvc;
using DartTournamentPlaner.API.Services;
using DartTournamentPlaner.API.Models;
using Microsoft.AspNetCore.SignalR;
using DartTournamentPlaner.API.Hubs;

namespace DartTournamentPlaner.API.Controllers;

/// <summary>
/// API Controller für Match-Operationen
/// </summary>
[ApiController]
[Route("api/tournaments/{tournamentId}/classes/{classId}/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchApiService _matchService;
    private readonly IHubContext<TournamentHub> _hubContext;

    public MatchesController(IMatchApiService matchService, IHubContext<TournamentHub> hubContext)
    {
        _matchService = matchService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Holt ein spezifisches Match
    /// </summary>
    [HttpGet("{matchId}")]
    public async Task<ActionResult<ApiResponse<MatchDto>>> GetMatch(int tournamentId, int classId, int matchId)
    {
        var result = await _matchService.GetMatchAsync(tournamentId, classId, matchId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Holt alle ausstehenden Matches einer Klasse
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<List<MatchDto>>>> GetPendingMatches(int tournamentId, int classId)
    {
        var result = await _matchService.GetPendingMatchesAsync(tournamentId, classId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Aktualisiert das Ergebnis eines Matches
    /// </summary>
    [HttpPut("{matchId}/result")]
    public async Task<ActionResult<ApiResponse<MatchDto>>> UpdateMatchResult(
        int tournamentId, 
        int classId, 
        int matchId, 
        [FromBody] MatchResultDto result)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<MatchDto>.ErrorResult("Ungültige Eingabedaten", errors));
        }

        result.MatchId = matchId; // Ensure correct match ID
        var updateResult = await _matchService.UpdateMatchResultAsync(tournamentId, classId, matchId, result);
        
        if (updateResult.Success)
        {
            // Benachrichtige alle verbundenen Clients über das Update
            await _hubContext.Clients.Group($"tournament_{tournamentId}")
                .SendAsync("MatchResultUpdated", new { tournamentId, classId, matchId, result = updateResult.Data });
        }

        return updateResult.Success ? Ok(updateResult) : BadRequest(updateResult);
    }

    /// <summary>
    /// Setzt ein Match zurück
    /// </summary>
    [HttpPost("{matchId}/reset")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetMatch(int tournamentId, int classId, int matchId)
    {
        var result = await _matchService.ResetMatchAsync(tournamentId, classId, matchId);
        
        if (result.Success)
        {
            // Benachrichtige alle verbundenen Clients über das Reset
            await _hubContext.Clients.Group($"tournament_{tournamentId}")
                .SendAsync("MatchReset", new { tournamentId, classId, matchId });
        }

        return result.Success ? Ok(result) : BadRequest(result);
    }
}

/// <summary>
/// Separater Controller für Live-Matches (ohne Tournament/Class in der Route)
/// </summary>
[ApiController]
[Route("api/matches")]
public class LiveMatchesController : ControllerBase
{
    private readonly ITournamentSyncService _syncService;
    private readonly IHubContext<TournamentHub> _hubContext;

    public LiveMatchesController(ITournamentSyncService syncService, IHubContext<TournamentHub> hubContext)
    {
        _syncService = syncService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Holt alle ausstehenden Matches aus dem aktuellen Live-Turnier
    /// </summary>
    [HttpGet("pending")]
    public ActionResult<ApiResponse<List<MatchDto>>> GetAllPendingMatches()
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

            var allPendingMatches = new List<MatchDto>();

            foreach (var tournamentClass in currentData.TournamentClasses)
            {
                // Sammle ausstehende Matches aus allen Gruppen
                foreach (var group in tournamentClass.Groups)
                {
                    var pendingMatches = group.Matches
                        .Where(m => m.Status == DartTournamentPlaner.Models.MatchStatus.NotStarted && !m.IsBye)
                        .Select(m => new MatchDto
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
                            Notes = m.Notes,
                            ClassId = tournamentClass.Id,
                            ClassName = tournamentClass.Name,
                            MatchType = "Group",
                            // ERWEITERT: Group-Information hinzufügen
                            GroupName = group.Name
                        });
                    
                    allPendingMatches.AddRange(pendingMatches);
                }

                // Sammle ausstehende Finals-Matches
                if (tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    var finalsMatches = tournamentClass.CurrentPhase.FinalsGroup.Matches
                        .Where(m => m.Status == DartTournamentPlaner.Models.MatchStatus.NotStarted && !m.IsBye)
                        .Select(m => new MatchDto
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
                            Notes = m.Notes,
                            ClassId = tournamentClass.Id,
                            ClassName = tournamentClass.Name,
                            MatchType = "Finals",
                            // Finals haben keine separate Gruppen-Info  
                            GroupName = "Finals"
                        });
                    
                    allPendingMatches.AddRange(finalsMatches);
                }

                // Sammle ausstehende Knockout-Matches (Winner Bracket)
                if (tournamentClass.CurrentPhase?.WinnerBracket != null)
                {
                    var winnerBracketMatches = tournamentClass.CurrentPhase.WinnerBracket
                        .Where(m => m.Status == DartTournamentPlaner.Models.MatchStatus.NotStarted)
                        .Select(m => new MatchDto
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
                            IsBye = false, // KnockoutMatch doesn't have IsBye property
                            StartTime = m.StartTime,
                            EndTime = m.EndTime,
                            Notes = m.Notes,
                            ClassId = tournamentClass.Id,
                            ClassName = tournamentClass.Name,
                            MatchType = $"Knockout-WB-{m.Round}"
                        });
                    
                    allPendingMatches.AddRange(winnerBracketMatches);
                }

                // Sammle ausstehende Knockout-Matches (Loser Bracket)
                if (tournamentClass.CurrentPhase?.LoserBracket != null)
                {
                    var loserBracketMatches = tournamentClass.CurrentPhase.LoserBracket
                        .Where(m => m.Status == DartTournamentPlaner.Models.MatchStatus.NotStarted)
                        .Select(m => new MatchDto
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
                            IsBye = false, // KnockoutMatch doesn't have IsBye property
                            StartTime = m.StartTime,
                            EndTime = m.EndTime,
                            Notes = m.Notes,
                            ClassId = tournamentClass.Id,
                            ClassName = tournamentClass.Name,
                            MatchType = $"Knockout-LB-{m.Round}"
                        });
                    
                    allPendingMatches.AddRange(loserBracketMatches);
                }
            }

            return Ok(ApiResponse<List<MatchDto>>.SuccessResult(allPendingMatches, $"{allPendingMatches.Count} ausstehende Matches gefunden (Gruppen + Finals + Knockout)"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<MatchDto>>.ErrorResult($"Fehler beim Laden der ausstehenden Matches: {ex.Message}"));
        }
    }

    /// <summary>
    /// Aktualisiert ein Match-Ergebnis im Live-Turnier
    /// </summary>
    [HttpPut("{matchId}")]
    public async Task<ActionResult<ApiResponse<MatchDto>>> UpdateLiveMatch(int matchId, [FromBody] MatchResultDto result)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<MatchDto>.ErrorResult("Ungültige Eingabedaten", errors));
            }

            if (!_syncService.IsApiRunning)
            {
                return BadRequest(ApiResponse<MatchDto>.ErrorResult("Keine aktive API-Verbindung zur Hauptanwendung"));
            }

            var currentData = _syncService.GetCurrentTournamentData();
            if (currentData == null)
            {
                return NotFound(ApiResponse<MatchDto>.ErrorResult("Keine aktiven Turnierdaten gefunden"));
            }

            // Finde das Match in allen Klassen und allen Match-Typen
            DartTournamentPlaner.Models.Match? targetMatch = null;
            DartTournamentPlaner.Models.KnockoutMatch? targetKnockoutMatch = null;
            int targetClassId = 0;
            string matchType = "Unknown";

            foreach (var tournamentClass in currentData.TournamentClasses)
            {
                // Suche in Gruppenmmatches
                foreach (var group in tournamentClass.Groups)
                {
                    targetMatch = group.Matches.FirstOrDefault(m => m.Id == matchId);
                    if (targetMatch != null)
                    {
                        targetClassId = tournamentClass.Id;
                        matchType = "Group";
                        break;
                    }
                }

                // Suche in Finals falls noch nicht gefunden
                if (targetMatch == null && tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    targetMatch = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == matchId);
                    if (targetMatch != null)
                    {
                        targetClassId = tournamentClass.Id;
                        matchType = "Finals";
                        break;
                    }
                }

                // Suche in Winner Bracket falls noch nicht gefunden
                if (targetMatch == null && tournamentClass.CurrentPhase?.WinnerBracket != null)
                {
                    targetKnockoutMatch = tournamentClass.CurrentPhase.WinnerBracket.FirstOrDefault(m => m.Id == matchId);
                    if (targetKnockoutMatch != null)
                    {
                        targetClassId = tournamentClass.Id;
                        matchType = $"Knockout-WB-{targetKnockoutMatch.Round}";
                        break;
                    }
                }

                // Suche in Loser Bracket falls noch nicht gefunden
                if (targetKnockoutMatch == null && tournamentClass.CurrentPhase?.LoserBracket != null)
                {
                    targetKnockoutMatch = tournamentClass.CurrentPhase.LoserBracket.FirstOrDefault(m => m.Id == matchId);
                    if (targetKnockoutMatch != null)
                    {
                        targetClassId = tournamentClass.Id;
                        matchType = $"Knockout-LB-{targetKnockoutMatch.Round}";
                        break;
                    }
                }
            }

            if (targetMatch == null && targetKnockoutMatch == null)
            {
                return NotFound(ApiResponse<MatchDto>.ErrorResult("Match nicht gefunden"));
            }

            // Prozessiere das Update über den Sync Service
            _syncService.ProcessMatchResultUpdate(matchId, targetClassId, result);

            // Erstelle Response DTO
            MatchDto dto;
            if (targetMatch != null)
            {
                dto = new MatchDto
                {
                    Id = targetMatch.Id,
                    Player1 = targetMatch.Player1 != null ? new PlayerDto { Id = targetMatch.Player1.Id, Name = targetMatch.Player1.Name, Email = targetMatch.Player1.Email } : null,
                    Player2 = targetMatch.Player2 != null ? new PlayerDto { Id = targetMatch.Player2.Id, Name = targetMatch.Player2.Name, Email = targetMatch.Player2.Email } : null,
                    Player1Sets = targetMatch.Player1Sets,
                    Player2Sets = targetMatch.Player2Sets,
                    Player1Legs = targetMatch.Player1Legs,
                    Player2Legs = targetMatch.Player2Legs,
                    Status = targetMatch.Status.ToString(),
                    Winner = targetMatch.Winner != null ? new PlayerDto { Id = targetMatch.Winner.Id, Name = targetMatch.Winner.Name, Email = targetMatch.Winner.Email } : null,
                    IsBye = targetMatch.IsBye,
                    StartTime = targetMatch.StartTime,
                    EndTime = targetMatch.EndTime,
                    ClassId = targetClassId,
                    MatchType = matchType
                };
            }
            else
            {
                dto = new MatchDto
                {
                    Id = targetKnockoutMatch!.Id,
                    Player1 = targetKnockoutMatch.Player1 != null ? new PlayerDto { Id = targetKnockoutMatch.Player1.Id, Name = targetKnockoutMatch.Player1.Name, Email = targetKnockoutMatch.Player1.Email } : null,
                    Player2 = targetKnockoutMatch.Player2 != null ? new PlayerDto { Id = targetKnockoutMatch.Player2.Id, Name = targetKnockoutMatch.Player2.Name, Email = targetKnockoutMatch.Player2.Email } : null,
                    Player1Sets = targetKnockoutMatch.Player1Sets,
                    Player2Sets = targetKnockoutMatch.Player2Sets,
                    Player1Legs = targetKnockoutMatch.Player1Legs,
                    Player2Legs = targetKnockoutMatch.Player2Legs,
                    Status = targetKnockoutMatch.Status.ToString(),
                    Winner = targetKnockoutMatch.Winner != null ? new PlayerDto { Id = targetKnockoutMatch.Winner.Id, Name = targetKnockoutMatch.Winner.Name, Email = targetKnockoutMatch.Winner.Email } : null,
                    IsBye = false, // KnockoutMatch doesn't have IsBye property
                    StartTime = targetKnockoutMatch.StartTime,
                    EndTime = targetKnockoutMatch.EndTime,
                    Notes = targetKnockoutMatch.Notes,
                    ClassId = targetClassId,
                    MatchType = matchType
                };
            }

            // Benachrichtige alle verbundenen Clients über das Update
            await _hubContext.Clients.All.SendAsync("MatchResultUpdated", new { matchId, classId = targetClassId, result = dto, matchType });

            return Ok(ApiResponse<MatchDto>.SuccessResult(dto, $"Match-Ergebnis ??????? aktualisiert ({matchType})"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<MatchDto>.ErrorResult($"Fehler beim Aktualisieren des Match-Ergebnisses: {ex.Message}"));
        }
    }
}