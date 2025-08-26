using DartTournamentPlaner.API.Models;

namespace DartTournamentPlaner.API.Services;

/// <summary>
/// Service Interface für Match API Operationen
/// </summary>
public interface IMatchApiService
{
    Task<ApiResponse<MatchDto>> GetMatchAsync(int tournamentId, int classId, int matchId);
    Task<ApiResponse<MatchDto>> UpdateMatchResultAsync(int tournamentId, int classId, int matchId, MatchResultDto result);
    Task<ApiResponse<List<MatchDto>>> GetPendingMatchesAsync(int tournamentId, int classId);
    Task<ApiResponse<bool>> ResetMatchAsync(int tournamentId, int classId, int matchId);
}

/// <summary>
/// Service Implementierung für Match API Operationen
/// </summary>
public class MatchApiService : IMatchApiService
{
    private readonly ITournamentSyncService _syncService;

    public MatchApiService(ITournamentSyncService syncService)
    {
        _syncService = syncService; 
    }

    public Task<ApiResponse<MatchDto>> GetMatchAsync(int tournamentId, int classId, int matchId)
    {
        try
        {
            var tournamentData = _syncService.GetCurrentTournamentData();
            if (tournamentData == null)
            {
                return Task.FromResult(ApiResponse<MatchDto>.ErrorResult("Keine aktiven Turnierdaten gefunden"));
            }

            var tournamentClass = tournamentData.TournamentClasses.FirstOrDefault(tc => tc.Id == classId);
            if (tournamentClass == null)
            {
                return Task.FromResult(ApiResponse<MatchDto>.ErrorResult("Turnierklasse nicht gefunden"));
            }

            // Suche in Gruppenmmatches
            DartTournamentPlaner.Models.Match? match = null;
            string? foundGroupName = null;
            foreach (var group in tournamentClass.Groups)
            {
                match = group.Matches.FirstOrDefault(m => m.Id == matchId);
                if (match != null) 
                {
                    foundGroupName = group.Name;
                    var dto = ConvertToDto(match, foundGroupName);
                    dto.ClassId = classId;
                    dto.ClassName = tournamentClass.Name;
                    return Task.FromResult(ApiResponse<MatchDto>.SuccessResult(dto));
                }
            }

            // Suche in Finals
            if (tournamentClass.CurrentPhase?.FinalsGroup != null)
            {
                match = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == matchId);
                if (match != null)
                {
                    var dto = ConvertToDto(match, "Finals");
                    dto.ClassId = classId;
                    dto.ClassName = tournamentClass.Name;
                    dto.MatchType = "Finals";
                    return Task.FromResult(ApiResponse<MatchDto>.SuccessResult(dto));
                }
            }

            // Suche in Winner Bracket
            if (tournamentClass.CurrentPhase?.WinnerBracket != null)
            {
                var knockoutMatch = tournamentClass.CurrentPhase.WinnerBracket.FirstOrDefault(m => m.Id == matchId);
                if (knockoutMatch != null)
                {
                    var dto = ConvertKnockoutMatchToDto(knockoutMatch);
                    return Task.FromResult(ApiResponse<MatchDto>.SuccessResult(dto));
                }
            }

            // Suche in Loser Bracket
            if (tournamentClass.CurrentPhase?.LoserBracket != null)
            {
                var knockoutMatch = tournamentClass.CurrentPhase.LoserBracket.FirstOrDefault(m => m.Id == matchId);
                if (knockoutMatch != null)
                {
                    var dto = ConvertKnockoutMatchToDto(knockoutMatch);
                    return Task.FromResult(ApiResponse<MatchDto>.SuccessResult(dto));
                }
            }

            return Task.FromResult(ApiResponse<MatchDto>.ErrorResult("Match nicht gefunden"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ApiResponse<MatchDto>.ErrorResult($"Fehler beim Laden des Matches: {ex.Message}"));
        }
    }

    public Task<ApiResponse<MatchDto>> UpdateMatchResultAsync(int tournamentId, int classId, int matchId, MatchResultDto result)
    {
        try
        {
            var tournamentData = _syncService.GetCurrentTournamentData();
            if (tournamentData == null)
            {
                return Task.FromResult(ApiResponse<MatchDto>.ErrorResult("Keine aktiven Turnierdaten gefunden"));
            }

            // Prozessiere das Update über den Sync Service
            _syncService.ProcessMatchResultUpdate(matchId, classId, result);

            // Hole das aktualisierte Match
            var tournamentClass = tournamentData.TournamentClasses.FirstOrDefault(tc => tc.Id == classId);
            if (tournamentClass == null)
            {
                return Task.FromResult(ApiResponse<MatchDto>.ErrorResult("Turnierklasse nicht gefunden"));
            }

            // Suche in Gruppenmmatches
            foreach (var group in tournamentClass.Groups)
            {
                var match = group.Matches.FirstOrDefault(m => m.Id == matchId);
                if (match != null) 
                {
                    var dto = ConvertToDto(match, group.Name);
                    dto.ClassId = classId;
                    dto.ClassName = tournamentClass.Name;
                    return Task.FromResult(ApiResponse<MatchDto>.SuccessResult(dto, "Match-Ergebnis erfolgreich aktualisiert"));
                }
            }

            // Suche in Finals
            if (tournamentClass.CurrentPhase?.FinalsGroup != null)
            {
                var match = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == matchId);
                if (match != null)
                {
                    var dto = ConvertToDto(match, "Finals");
                    dto.ClassId = classId;
                    dto.ClassName = tournamentClass.Name;
                    dto.MatchType = "Finals";
                    return Task.FromResult(ApiResponse<MatchDto>.SuccessResult(dto, "Match-Ergebnis erfolgreich aktualisiert"));
                }
            }

            // Suche in Winner Bracket
            if (tournamentClass.CurrentPhase?.WinnerBracket != null)
            {
                var knockoutMatch = tournamentClass.CurrentPhase.WinnerBracket.FirstOrDefault(m => m.Id == matchId);
                if (knockoutMatch != null)
                {
                    var dto = ConvertKnockoutMatchToDto(knockoutMatch);
                    return Task.FromResult(ApiResponse<MatchDto>.SuccessResult(dto, "Match-Ergebnis erfolgreich aktualisiert"));
                }
            }

            // Suche in Loser Bracket
            if (tournamentClass.CurrentPhase?.LoserBracket != null)
            {
                var knockoutMatch = tournamentClass.CurrentPhase.LoserBracket.FirstOrDefault(m => m.Id == matchId);
                if (knockoutMatch != null)
                {
                    var dto = ConvertKnockoutMatchToDto(knockoutMatch);
                    return Task.FromResult(ApiResponse<MatchDto>.SuccessResult(dto, "Match-Ergebnis erfolgreich aktualisiert"));
                }
            }

            return Task.FromResult(ApiResponse<MatchDto>.ErrorResult("Match nicht gefunden"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ApiResponse<MatchDto>.ErrorResult($"Fehler beim Aktualisieren des Match-Ergebnisses: {ex.Message}"));
        }
    }

    public Task<ApiResponse<List<MatchDto>>> GetPendingMatchesAsync(int tournamentId, int classId)
    {
        try
        {
            var tournamentData = _syncService.GetCurrentTournamentData();
            if (tournamentData == null)
            {
                return Task.FromResult(ApiResponse<List<MatchDto>>.ErrorResult("Keine aktiven Turnierdaten gefunden"));
            }

            var tournamentClass = tournamentData.TournamentClasses.FirstOrDefault(tc => tc.Id == classId);
            if (tournamentClass == null)
            {
                return Task.FromResult(ApiResponse<List<MatchDto>>.ErrorResult("Turnierklasse nicht gefunden"));
            }

            var allPendingMatches = new List<MatchDto>();

            // Sammle ausstehende Matches aus allen Gruppen
            foreach (var group in tournamentClass.Groups)
            {
                var groupMatches = group.Matches
                    .Where(m => m.Status == DartTournamentPlaner.Models.MatchStatus.NotStarted && !m.IsBye)
                    .Select(m => ConvertToDtoWithClass(m, tournamentClass.Id, tournamentClass.Name, group.Name));
                allPendingMatches.AddRange(groupMatches);
            }

            // Sammle ausstehende Finals-Matches
            if (tournamentClass.CurrentPhase?.FinalsGroup != null)
            {
                var finalsMatches = tournamentClass.CurrentPhase.FinalsGroup.Matches
                    .Where(m => m.Status == DartTournamentPlaner.Models.MatchStatus.NotStarted && !m.IsBye)
                    .Select(m => ConvertToDtoWithClass(m, tournamentClass.Id, tournamentClass.Name, "Finals"));
                allPendingMatches.AddRange(finalsMatches);
            }

            // Sammle ausstehende Winner Bracket Matches
            if (tournamentClass.CurrentPhase?.WinnerBracket != null)
            {
                var winnerBracketMatches = tournamentClass.CurrentPhase.WinnerBracket
                    .Where(m => m.Status == DartTournamentPlaner.Models.MatchStatus.NotStarted)
                    .Select(m => ConvertKnockoutMatchToDtoWithClass(m, tournamentClass.Id, tournamentClass.Name));
                allPendingMatches.AddRange(winnerBracketMatches);
            }

            // Sammle ausstehende Loser Bracket Matches
            if (tournamentClass.CurrentPhase?.LoserBracket != null)
            {
                var loserBracketMatches = tournamentClass.CurrentPhase.LoserBracket
                    .Where(m => m.Status == DartTournamentPlaner.Models.MatchStatus.NotStarted)
                    .Select(m => ConvertKnockoutMatchToDtoWithClass(m, tournamentClass.Id, tournamentClass.Name));
                allPendingMatches.AddRange(loserBracketMatches);
            }

            return Task.FromResult(ApiResponse<List<MatchDto>>.SuccessResult(allPendingMatches, $"{allPendingMatches.Count} ausstehende Matches gefunden (alle Phasen)"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ApiResponse<List<MatchDto>>.ErrorResult($"Fehler beim Laden der ausstehenden Matches: {ex.Message}"));
        }
    }

    public Task<ApiResponse<bool>> ResetMatchAsync(int tournamentId, int classId, int matchId)
    {
        try
        {
            var tournamentData = _syncService.GetCurrentTournamentData();
            if (tournamentData == null)
            {
                return Task.FromResult(ApiResponse<bool>.ErrorResult("Keine aktiven Turnierdaten gefunden"));
            }

            var tournamentClass = tournamentData.TournamentClasses.FirstOrDefault(tc => tc.Id == classId);
            if (tournamentClass == null)
            {
                return Task.FromResult(ApiResponse<bool>.ErrorResult("Turnierklasse nicht gefunden"));
            }

            // Suche das Match und setze es zurück
            // Suche in Gruppenmmatches
            foreach (var group in tournamentClass.Groups)
            {
                var match = group.Matches.FirstOrDefault(m => m.Id == matchId);
                if (match != null)
                {
                    ResetMatch(match);
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(true, "Match erfolgreich zurückgesetzt"));
                }
            }

            // Suche in Finals
            if (tournamentClass.CurrentPhase?.FinalsGroup != null)
            {
                var match = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m => m.Id == matchId);
                if (match != null)
                {
                    ResetMatch(match);
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(true, "Match erfolgreich zurückgesetzt"));
                }
            }

            // Suche in Winner Bracket
            if (tournamentClass.CurrentPhase?.WinnerBracket != null)
            {
                var knockoutMatch = tournamentClass.CurrentPhase.WinnerBracket.FirstOrDefault(m => m.Id == matchId);
                if (knockoutMatch != null)
                {
                    ResetKnockoutMatch(knockoutMatch);
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(true, "Match erfolgreich zurückgesetzt"));
                }
            }

            // Suche in Loser Bracket
            if (tournamentClass.CurrentPhase?.LoserBracket != null)
            {
                var knockoutMatch = tournamentClass.CurrentPhase.LoserBracket.FirstOrDefault(m => m.Id == matchId);
                if (knockoutMatch != null)
                {
                    ResetKnockoutMatch(knockoutMatch);
                    return Task.FromResult(ApiResponse<bool>.SuccessResult(true, "Match erfolgreich zurückgesetzt"));
                }
            }

            return Task.FromResult(ApiResponse<bool>.ErrorResult("Match nicht gefunden"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ApiResponse<bool>.ErrorResult($"Fehler beim Zurücksetzen des Matches: {ex.Message}"));
        }
    }

    private static void ResetMatch(DartTournamentPlaner.Models.Match match)
    {
        match.Status = DartTournamentPlaner.Models.MatchStatus.NotStarted;
        match.Player1Sets = 0;
        match.Player2Sets = 0;
        match.Player1Legs = 0;
        match.Player2Legs = 0;
        match.Winner = null;
        match.StartTime = null;
        match.EndTime = null;
        match.Notes = string.Empty;
    }

    private static void ResetKnockoutMatch(DartTournamentPlaner.Models.KnockoutMatch knockoutMatch)
    {
        knockoutMatch.Status = DartTournamentPlaner.Models.MatchStatus.NotStarted;
        knockoutMatch.Player1Sets = 0;
        knockoutMatch.Player2Sets = 0;
        knockoutMatch.Player1Legs = 0;
        knockoutMatch.Player2Legs = 0;
        knockoutMatch.Winner = null;
        knockoutMatch.Loser = null;
        knockoutMatch.StartTime = null;
        knockoutMatch.EndTime = null;
        knockoutMatch.Notes = string.Empty;
    }

    private static MatchDto ConvertToDto(DartTournamentPlaner.Models.Match match, string? groupName = null)
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
            MatchType = "Group",
            // ERWEITERT: Group Information
            GroupName = groupName
        };
    }

    private static MatchDto ConvertKnockoutMatchToDto(DartTournamentPlaner.Models.KnockoutMatch knockoutMatch)
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
            IsBye = false, // KnockoutMatch doesn't have IsBye property
            StartTime = knockoutMatch.StartTime,
            EndTime = knockoutMatch.EndTime,
            Notes = knockoutMatch.Notes,
            MatchType = $"Knockout-{knockoutMatch.BracketType}-{knockoutMatch.Round}",
            // Knockout matches haben keine Gruppen
            GroupName = null
        };
    }

    // Helper method to set ClassId for matches from a specific tournament class
    private static MatchDto ConvertToDtoWithClass(DartTournamentPlaner.Models.Match match, int classId, string className = null, string? groupName = null)
    {
        var dto = ConvertToDto(match, groupName);
        dto.ClassId = classId;
        dto.ClassName = className;
        return dto;
    }

    private static MatchDto ConvertKnockoutMatchToDtoWithClass(DartTournamentPlaner.Models.KnockoutMatch knockoutMatch, int classId, string className = null)
    {
        var dto = ConvertKnockoutMatchToDto(knockoutMatch);
        dto.ClassId = classId;
        dto.ClassName = className;
        return dto;
    }
}