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
    void ProcessMatchResultUpdate(int matchId, int classId, MatchResultDto result);
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
    public void ProcessMatchResultUpdate(int matchId, int classId, MatchResultDto result)
    {
        Console.WriteLine($"🎯 [SYNC_SERVICE] Processing match result update:");
        Console.WriteLine($"   Match ID: {matchId}");
        Console.WriteLine($"   Class ID: {classId}");
        Console.WriteLine($"   Group Name: {result.GroupName}");
        Console.WriteLine($"   Group ID: {result.GroupId}");
        Console.WriteLine($"   Match Type: {result.MatchType}");
        
        // Finde das entsprechende Match und aktualisiere es
        lock (_dataLock)
        {
            if (_currentTournamentData == null) 
            {
                Console.WriteLine($"❌ [SYNC_SERVICE] No current tournament data available");
                return;
            }

            var tournamentClass = _currentTournamentData.TournamentClasses
                .FirstOrDefault(tc => tc.Id == classId);
                
            if (tournamentClass == null) 
            {
                Console.WriteLine($"❌ [SYNC_SERVICE] Tournament class {classId} not found");
                return;
            }

            Console.WriteLine($"🏆 [SYNC_SERVICE] Found tournament class: {tournamentClass.Name}");

            // 🚨 KORRIGIERT: Verwende GROUP-SPEZIFISCHE Suche für Gruppen-Matches
            if (!string.IsNullOrEmpty(result.GroupName) && result.MatchType == "Group")
            {
                Console.WriteLine($"🔍 [SYNC_SERVICE] Searching for Group match in '{result.GroupName}'...");
                
                // Suche die SPEZIFISCHE Gruppe
                var targetGroup = tournamentClass.Groups
                    .FirstOrDefault(g => g.Name.Equals(result.GroupName, StringComparison.OrdinalIgnoreCase));
                
                if (targetGroup == null)
                {
                    Console.WriteLine($"❌ [SYNC_SERVICE] Group '{result.GroupName}' not found in class {tournamentClass.Name}");
                    Console.WriteLine($"   Available groups: {string.Join(", ", tournamentClass.Groups.Select(g => g.Name))}");
                    return;
                }

                Console.WriteLine($"📋 [SYNC_SERVICE] Found target group: {targetGroup.Name} (ID: {targetGroup.Id})");

                // Suche das Match NUR in der spezifischen Gruppe
                var match = targetGroup.Matches.FirstOrDefault(m => m.Id == matchId);
                if (match != null)
                {
                    Console.WriteLine($"✅ [SYNC_SERVICE] Found match {matchId} in group '{targetGroup.Name}'");
                    Console.WriteLine($"   Match: {match.Player1?.Name} vs {match.Player2?.Name}");
                    
                    // Aktualisiere das Match
                    match.SetResult(result.Player1Sets, result.Player2Sets, 
                                   result.Player1Legs, result.Player2Legs);
                    match.Notes = result.Notes ?? string.Empty;

                    Console.WriteLine($"🎯 [SYNC_SERVICE] Match result updated: {result.Player1Sets}-{result.Player2Sets} Sets, {result.Player1Legs}-{result.Player2Legs} Legs");

                    // Feuere Event für WPF-Anwendung
                    MatchResultUpdated?.Invoke(this, new MatchResultUpdateEventArgs
                    {
                        MatchId = matchId,
                        ClassId = classId,
                        Result = result,
                        GroupName = targetGroup.Name,
                        GroupId = targetGroup.Id
                    });
                    
                    Console.WriteLine($"✅ [SYNC_SERVICE] Successfully processed group match update");
                    return;
                }
                else
                {
                    Console.WriteLine($"❌ [SYNC_SERVICE] Match {matchId} not found in group '{targetGroup.Name}'");
                    Console.WriteLine($"   Available matches in group: {string.Join(", ", targetGroup.Matches.Select(m => m.Id))}");
                }
            }
            // Falls keine Group-Information vorhanden, verwende alte Logik (für Finals/Knockout)
            else
            {
                Console.WriteLine($"🔍 [SYNC_SERVICE] No group info - searching in Finals/Knockout matches...");
                
                // Suche in allen Gruppen (alte Logik für Kompatibilität)
                foreach (var group in tournamentClass.Groups)
                {
                    var match = group.Matches.FirstOrDefault(m => m.Id == matchId);
                    if (match != null)
                    {
                        Console.WriteLine($"⚠️ [SYNC_SERVICE] Found match {matchId} in group '{group.Name}' (fallback search)");
                        
                        // Aktualisiere das Match
                        match.SetResult(result.Player1Sets, result.Player2Sets, 
                                       result.Player1Legs, result.Player2Legs);
                        match.Notes = result.Notes ?? string.Empty;

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

                // Suche in Finals
                if (tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    var match = tournamentClass.CurrentPhase.FinalsGroup.Matches
                        .FirstOrDefault(m => m.Id == matchId);
                    if (match != null)
                    {
                        Console.WriteLine($"🏆 [SYNC_SERVICE] Found Finals match {matchId}");
                        
                        // Aktualisiere das Match
                        match.SetResult(result.Player1Sets, result.Player2Sets, 
                                       result.Player1Legs, result.Player2Legs);
                        match.Notes = result.Notes ?? string.Empty;

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

                // Suche in Winner Bracket
                if (tournamentClass.CurrentPhase?.WinnerBracket != null)
                {
                    var knockoutMatch = tournamentClass.CurrentPhase.WinnerBracket
                        .FirstOrDefault(m => m.Id == matchId);
                    if (knockoutMatch != null)
                    {
                        Console.WriteLine($"⚡ [SYNC_SERVICE] Found Winner Bracket match {matchId}");
                        
                        // Aktualisiere das KnockoutMatch
                        knockoutMatch.SetResult(result.Player1Sets, result.Player2Sets, 
                                               result.Player1Legs, result.Player2Legs);
                        knockoutMatch.Notes = result.Notes ?? string.Empty;

                        // Feuere Event für WPF-Anwendung
                        MatchResultUpdated?.Invoke(this, new MatchResultUpdateEventArgs
                        {
                            MatchId = matchId,
                            ClassId = classId,
                            Result = result,
                            GroupName = $"Winner Bracket - {knockoutMatch.Round}",
                            GroupId = null
                        });
                        return;
                    }
                }

                // Suche in Loser Bracket
                if (tournamentClass.CurrentPhase?.LoserBracket != null)
                {
                    var knockoutMatch = tournamentClass.CurrentPhase.LoserBracket
                        .FirstOrDefault(m => m.Id == matchId);
                    if (knockoutMatch != null)
                    {
                        Console.WriteLine($"🔄 [SYNC_SERVICE] Found Loser Bracket match {matchId}");
                        
                        // Aktualisiere das KnockoutMatch
                        knockoutMatch.SetResult(result.Player1Sets, result.Player2Sets, 
                                               result.Player1Legs, result.Player2Legs);
                        knockoutMatch.Notes = result.Notes ?? string.Empty;

                        // Feuere Event für WPF-Anwendung
                        MatchResultUpdated?.Invoke(this, new MatchResultUpdateEventArgs
                        {
                            MatchId = matchId,
                            ClassId = classId,
                            Result = result,
                            GroupName = $"Loser Bracket - {knockoutMatch.Round}",
                            GroupId = null
                        });
                        return;
                    }
                }
            }
            
            Console.WriteLine($"❌ [SYNC_SERVICE] Match {matchId} not found anywhere in class {classId}");
        }
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