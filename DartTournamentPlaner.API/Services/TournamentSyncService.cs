using DartTournamentPlaner.Models;
using DartTournamentPlaner.API.Models;

namespace DartTournamentPlaner.API.Services;

/// <summary>
/// Service für die Synchronisierung zwischen WPF-Anwendung und API
/// </summary>
public interface ITournamentSyncService
{
    /// <summary>
    /// Startet die API und macht Turnierdaten verfügbar
    /// </summary>
    Task StartApiAsync(TournamentData tournamentData);
    
    /// <summary>
    /// Stoppt die API
    /// </summary>
    Task StopApiAsync();
    
    /// <summary>
    /// Aktualisiert die Turnierdaten in der API
    /// </summary>
    Task UpdateTournamentDataAsync(TournamentData tournamentData);
    
    /// <summary>
    /// Event für Match-Ergebnis Updates von der API
    /// </summary>
    event EventHandler<MatchResultUpdateEventArgs>? MatchResultUpdated;
    
    /// <summary>
    /// Prüft ob die API läuft
    /// </summary>
    bool IsApiRunning { get; }
    
    /// <summary>
    /// Holt die aktuellen Turnierdaten
    /// </summary>
    TournamentData? GetCurrentTournamentData();
    
    /// <summary>
    /// Verarbeitet Match-Result Updates von der API mit Group-Information
    /// </summary>
    void UpdateMatchResult(int matchId, int classId, MatchResultDto result);
}

/// <summary>
/// Implementierung des Tournament Sync Service
/// </summary>
public class TournamentSyncService : ITournamentSyncService
{
    private TournamentData? _currentTournamentData;
    private readonly object _dataLock = new object();
    
    public event EventHandler<MatchResultUpdateEventArgs>? MatchResultUpdated;
    public bool IsApiRunning { get; private set; }

    public Task StartApiAsync(TournamentData tournamentData)
    {
        lock (_dataLock)
        {
            _currentTournamentData = tournamentData;
            IsApiRunning = true;
        }
        
        return Task.CompletedTask;
    }

    public Task StopApiAsync()
    {
        lock (_dataLock)
        {
            _currentTournamentData = null;
            IsApiRunning = false;
        }
        
        return Task.CompletedTask;
    }

    public Task UpdateTournamentDataAsync(TournamentData tournamentData)
    {
        lock (_dataLock)
        {
            _currentTournamentData = tournamentData;
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Holt die aktuellen Turnierdaten
    /// </summary>
    public TournamentData? GetCurrentTournamentData()
    {
        lock (_dataLock)
        {
            return _currentTournamentData;
        }
    }

    /// <summary>
    /// Verarbeitet Match-Result Updates von der API mit Group-Information
    /// </summary>
    public void UpdateMatchResult(int matchId, int classId, MatchResultDto result)
    {
        Console.WriteLine($"🎯 [SYNC_SERVICE] ===== UPDATING MATCH RESULT =====");
        Console.WriteLine($"🎯 [SYNC_SERVICE] Match ID: {matchId}");
        Console.WriteLine($"🎯 [SYNC_SERVICE] Class ID: {classId}");
        Console.WriteLine($"📊 [SYNC_SERVICE] Result: {result.Player1Sets}-{result.Player2Sets} Sets, {result.Player1Legs}-{result.Player2Legs} Legs");
        Console.WriteLine($"📝 [SYNC_SERVICE] Notes: \"{result.Notes ?? "None"}\"");
        Console.WriteLine($"🔍 [SYNC_SERVICE] Status: Finished");

        if (_currentTournamentData?.TournamentClasses == null)
        {
            Console.WriteLine($"❌ [SYNC_SERVICE] No tournament data available");
            return;
        }

        var tournamentClass = _currentTournamentData.TournamentClasses.FirstOrDefault(tc => tc.Id == classId);
        if (tournamentClass == null)
        {
            Console.WriteLine($"❌ [SYNC_SERVICE] Tournament class {classId} not found");
            return;
        }

        Console.WriteLine($"✅ [SYNC_SERVICE] Found tournament class: {tournamentClass.Name}");
        Console.WriteLine($"🔍 [SYNC_SERVICE] Searching for match {matchId} in all match types...");

        // ERWEITERTE SUCHE: Zuerst in aktueller Phase suchen, dann in Gruppen

        // 1. NEUE: Suche in Winner Bracket (höchste Priorität)
        if (tournamentClass.CurrentPhase?.WinnerBracket != null)
        {
            var winnerMatch = tournamentClass.CurrentPhase.WinnerBracket
                .FirstOrDefault(m => m.Id == matchId);
            if (winnerMatch != null)
            {
                Console.WriteLine($"⚡ [SYNC_SERVICE] Found Winner Bracket match {matchId} in round {winnerMatch.Round}");
                
                // Aktualisiere das KnockoutMatch
                winnerMatch.SetResult(result.Player1Sets, result.Player2Sets, 
                                     result.Player1Legs, result.Player2Legs);
                winnerMatch.Notes = result.Notes ?? string.Empty;

                // Feuere Event für WPF-Anwendung
                MatchResultUpdated?.Invoke(this, new MatchResultUpdateEventArgs
                {
                    MatchId = matchId,
                    ClassId = classId,
                    Result = result,
                    GroupName = $"Winner Bracket - Round {winnerMatch.Round}",
                    GroupId = null
                });
                return;
            }
        }

        // 2. NEUE: Suche in Loser Bracket (zweite Priorität)
        if (tournamentClass.CurrentPhase?.LoserBracket != null)
        {
            var loserMatch = tournamentClass.CurrentPhase.LoserBracket
                .FirstOrDefault(m => m.Id == matchId);
            if (loserMatch != null)
            {
                Console.WriteLine($"🔄 [SYNC_SERVICE] Found Loser Bracket match {matchId} in round {loserMatch.Round}");
                
                // Aktualisiere das KnockoutMatch
                loserMatch.SetResult(result.Player1Sets, result.Player2Sets, 
                                    result.Player1Legs, result.Player2Legs);
                loserMatch.Notes = result.Notes ?? string.Empty;

                // Feuere Event für WPF-Anwendung
                MatchResultUpdated?.Invoke(this, new MatchResultUpdateEventArgs
                {
                    MatchId = matchId,
                    ClassId = classId,
                    Result = result,
                    GroupName = $"Loser Bracket - Round {loserMatch.Round}",
                    GroupId = null
                });
                return;
            }
        }

        // 3. NEUE: Suche in Finals (dritte Priorität)
        if (tournamentClass.CurrentPhase?.FinalsGroup != null)
        {
            var finalsMatch = tournamentClass.CurrentPhase.FinalsGroup.Matches
                .FirstOrDefault(m => m.Id == matchId);
            if (finalsMatch != null)
            {
                Console.WriteLine($"🏆 [SYNC_SERVICE] Found Finals match {matchId}");
                
                // Aktualisiere das Match
                finalsMatch.SetResult(result.Player1Sets, result.Player2Sets, 
                                     result.Player1Legs, result.Player2Legs);
                finalsMatch.Notes = result.Notes ?? string.Empty;

                // Feuere Event für WPF-Anwendung
                MatchResultUpdated?.Invoke(this, new MatchResultUpdateEventArgs
                {
                    MatchId = matchId,
                    ClassId = classId,
                    Result = result,
                    GroupName = "Finals",
                    GroupId = null
                });
                return;
            }
        }

        // 4. Fallback: Suche in allen Gruppen (niedrigste Priorität, für Kompatibilität)
        foreach (var group in tournamentClass.Groups)
        {
            var groupMatch = group.Matches.FirstOrDefault(m => m.Id == matchId);
            if (groupMatch != null)
            {
                Console.WriteLine($"🔸 [SYNC_SERVICE] Found Group match {matchId} in group '{group.Name}' (fallback search)");
                
                // Aktualisiere das Match
                groupMatch.SetResult(result.Player1Sets, result.Player2Sets, 
                                    result.Player1Legs, result.Player2Legs);
                groupMatch.Notes = result.Notes ?? string.Empty;

                // Feuere Event für WPF-Anwendung
                MatchResultUpdated?.Invoke(this, new MatchResultUpdateEventArgs
                {
                    MatchId = matchId,
                    ClassId = classId,
                    Result = result,
                    GroupName = group.Name,
                    GroupId = group.Id
                });
                return;
            }
        }

        Console.WriteLine($"❌ [SYNC_SERVICE] Match {matchId} not found in any match type for class {classId}");
        Console.WriteLine($"🔍 [SYNC_SERVICE] Searched in:");
        Console.WriteLine($"   - Winner Bracket: {tournamentClass.CurrentPhase?.WinnerBracket?.Count ?? 0} matches");
        Console.WriteLine($"   - Loser Bracket: {tournamentClass.CurrentPhase?.LoserBracket?.Count ?? 0} matches");
        Console.WriteLine($"   - Finals: {tournamentClass.CurrentPhase?.FinalsGroup?.Matches?.Count ?? 0} matches");
        Console.WriteLine($"   - Groups: {tournamentClass.Groups.Sum(g => g.Matches.Count)} matches");
    }
}

/// <summary>
/// Event-Argumente für Match-Ergebnis Updates
/// </summary>
public class MatchResultUpdateEventArgs : EventArgs
{
    public int MatchId { get; set; }
    public int ClassId { get; set; }
    public MatchResultDto Result { get; set; } = new();
    
    // 🚨 HINZUGEFÜGT: Group-Information für eindeutige Match-Identifikation
    public string? GroupName { get; set; }
    public int? GroupId { get; set; }
}