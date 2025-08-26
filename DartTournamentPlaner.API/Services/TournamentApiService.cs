using DartTournamentPlaner.Models;
using DartTournamentPlaner.API.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DartTournamentPlaner.API.Services;

/// <summary>
/// Service Interface für Tournament API Operationen
/// </summary>
public interface ITournamentApiService
{
    Task<ApiResponse<List<TournamentDto>>> GetTournamentsAsync();
    Task<ApiResponse<TournamentDto>> GetTournamentAsync(int id);
    Task<ApiResponse<TournamentDto>> CreateTournamentAsync(TournamentData tournamentData);
    Task<ApiResponse<TournamentDto>> UpdateTournamentAsync(int id, TournamentData tournamentData);
    Task<ApiResponse<bool>> DeleteTournamentAsync(int id);
}

/// <summary>
/// Service Implementierung für Tournament API Operationen
/// </summary> 
public class TournamentApiService : ITournamentApiService
{
    private readonly ApiDbContext _context;
    private readonly ITournamentSyncService _syncService;

    public TournamentApiService(ApiDbContext context, ITournamentSyncService syncService)
    {
        _context = context;
        _syncService = syncService;
    }

    public async Task<ApiResponse<List<TournamentDto>>> GetTournamentsAsync()
    {
        try
        {
            var tournaments = await _context.Tournaments
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.LastModified)
                .ToListAsync();

            var dtos = tournaments.Select(t => new TournamentDto
            {
                Id = t.Id,
                Name = t.Name,
                CreatedAt = t.CreatedAt,
                LastModified = t.LastModified,
                Status = "Active" // TODO: Determine actual status
            }).ToList();

            return ApiResponse<List<TournamentDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<TournamentDto>>.ErrorResult($"Fehler beim Laden der Turniere: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TournamentDto>> GetTournamentAsync(int id)
    {
        try
        {
            // Prüfe erst den laufenden Sync Service
            var currentData = _syncService.GetCurrentTournamentData();
            if (currentData != null)
            {
                var liveDto = ConvertToDto(currentData, id);
                return ApiResponse<TournamentDto>.SuccessResult(liveDto, "Live tournament data");
            }

            // Fallback zur Datenbank
            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (tournament == null)
            {
                return ApiResponse<TournamentDto>.ErrorResult("Turnier nicht gefunden");
            }

            var tournamentData = JsonConvert.DeserializeObject<TournamentData>(tournament.JsonData);
            if (tournamentData == null)
            {
                return ApiResponse<TournamentDto>.ErrorResult("Turnierdaten konnten nicht geladen werden");
            }

            var dto = ConvertToDto(tournamentData, tournament.Id);
            return ApiResponse<TournamentDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<TournamentDto>.ErrorResult($"Fehler beim Laden des Turniers: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TournamentDto>> CreateTournamentAsync(TournamentData tournamentData)
    {
        try
        {
            var jsonData = JsonConvert.SerializeObject(tournamentData, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            var apiTournament = new ApiTournamentData
            {
                Name = $"Tournament {DateTime.Now:yyyy-MM-dd HH:mm}",
                JsonData = jsonData,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.Tournaments.Add(apiTournament);
            await _context.SaveChangesAsync();

            var dto = ConvertToDto(tournamentData, apiTournament.Id);
            return ApiResponse<TournamentDto>.SuccessResult(dto, "Turnier erfolgreich erstellt");
        }
        catch (Exception ex)
        {
            return ApiResponse<TournamentDto>.ErrorResult($"Fehler beim Erstellen des Turniers: {ex.Message}");
        }
    }

    public async Task<ApiResponse<TournamentDto>> UpdateTournamentAsync(int id, TournamentData tournamentData)
    {
        try
        {
            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (tournament == null)
            {
                return ApiResponse<TournamentDto>.ErrorResult("Turnier nicht gefunden");
            }

            var jsonData = JsonConvert.SerializeObject(tournamentData, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            tournament.JsonData = jsonData;
            tournament.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var dto = ConvertToDto(tournamentData, tournament.Id);
            return ApiResponse<TournamentDto>.SuccessResult(dto, "Turnier erfolgreich aktualisiert");
        }
        catch (Exception ex)
        {
            return ApiResponse<TournamentDto>.ErrorResult($"Fehler beim Aktualisieren des Turniers: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteTournamentAsync(int id)
    {
        try
        {
            var tournament = await _context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (tournament == null)
            {
                return ApiResponse<bool>.ErrorResult("Turnier nicht gefunden");
            }

            tournament.IsActive = false;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Turnier erfolgreich gelöscht");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult($"Fehler beim Löschen des Turniers: {ex.Message}");
        }
    }

    private TournamentDto ConvertToDto(TournamentData tournamentData, int apiId)
    {
        return new TournamentDto
        {
            Id = apiId,
            Name = $"Tournament {DateTime.Now:yyyy-MM-dd}",
            CreatedAt = DateTime.UtcNow,
            LastModified = tournamentData.LastModified,
            Status = "Active",
            Classes = tournamentData.TournamentClasses.Select(tc => new TournamentClassDto
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
                        Notes = m.Notes,
                        // ?? HINZUGEFÜGT: Group-Information für eindeutige Match-Identifikation
                        ClassId = tc.Id,
                        ClassName = tc.Name,
                        GroupId = g.Id,
                        GroupName = g.Name,
                        MatchType = "Group"
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
                    Notes = m.Notes,
                    // ?? HINZUGEFÜGT: Class-Information für Finals
                    ClassId = tc.Id,
                    ClassName = tc.Name,
                    GroupId = null,
                    GroupName = "Finals",
                    MatchType = "Finals"
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
                    Player2FromWinner = km.Player2FromWinner,
                    // ?? HINZUGEFÜGT: Class-Information für Knockout-Matches
                    ClassId = tc.Id,
                    ClassName = tc.Name,
                    GroupId = null,
                    GroupName = $"{km.BracketType} Bracket - {km.Round}",
                    MatchType = $"Knockout-{km.BracketType}-{km.Round}"
                }).ToList()
            }).ToList()
        };
    }
}