using System.IO;
using Newtonsoft.Json;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services;

public class DataService
{
    private readonly string _dataPath = "tournament_data.json";

    public async Task<TournamentData> LoadTournamentDataAsync()
    {
        try
        {
            if (File.Exists(_dataPath))
            {
                var json = await File.ReadAllTextAsync(_dataPath);
                var data = JsonConvert.DeserializeObject<TournamentData>(json);
                
                if (data != null)
                {
                    // WICHTIG: Nach dem Laden die Daten bereinigen und validieren
                    foreach (var tournamentClass in data.TournamentClasses)
                    {
                        CleanupDuplicatePhases(tournamentClass);
                    }
                }
                
                return data ?? new TournamentData();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading tournament data: {ex.Message}");
        }
        
        return new TournamentData();
    }

    public async Task SaveTournamentDataAsync(TournamentData data)
    {
        try
        {
            data.LastModified = DateTime.Now;
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(_dataPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving tournament data: {ex.Message}");
            throw;
        }
    }

    public bool BackupData(string backupPath)
    {
        try
        {
            if (File.Exists(_dataPath))
            {
                File.Copy(_dataPath, backupPath, true);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating backup: {ex.Message}");
        }
        return false;
    }

    /// <summary>
    /// Bereinigt Duplikat-Phasen direkt beim Laden der JSON-Daten
    /// </summary>
    private void CleanupDuplicatePhases(TournamentClass tournamentClass)
    {
        try
        {
            Console.WriteLine($"DataService: Cleaning up phases for {tournamentClass.Name} - Current count: {tournamentClass.Phases.Count}");
            
            var cleanPhases = new List<TournamentPhase>();
            
            // 1. Sichere genau eine GroupPhase
            var groupPhases = tournamentClass.Phases.Where(p => p.PhaseType == TournamentPhaseType.GroupPhase).ToList();
            TournamentPhase groupPhase;
            
            if (groupPhases.Any())
            {
                groupPhase = groupPhases.First();
                // Merge Groups aus Duplikaten
                for (int i = 1; i < groupPhases.Count; i++)
                {
                    var duplicatePhase = groupPhases[i];
                    foreach (var group in duplicatePhase.Groups)
                    {
                        if (!groupPhase.Groups.Any(g => g.Id == group.Id))
                        {
                            groupPhase.Groups.Add(group);
                        }
                    }
                }
            }
            else
            {
                groupPhase = new TournamentPhase
                {
                    Name = "Gruppenphase",
                    PhaseType = TournamentPhaseType.GroupPhase,
                    IsActive = false,
                    IsCompleted = false
                };
            }
            cleanPhases.Add(groupPhase);
            
            // 2. Behalte nur die beste K.O.-Phase
            var knockoutPhases = tournamentClass.Phases.Where(p => p.PhaseType == TournamentPhaseType.KnockoutPhase).ToList();
            if (knockoutPhases.Any())
            {
                var bestKnockout = knockoutPhases
                    .OrderByDescending(p => p.WinnerBracket?.Count ?? 0)
                    .ThenByDescending(p => p.LoserBracket?.Count ?? 0)
                    .First();
                    
                bestKnockout.IsActive = true;
                cleanPhases.Add(bestKnockout);
                Console.WriteLine($"DataService: Keeping knockout phase with {bestKnockout.WinnerBracket?.Count ?? 0} WB matches");
            }
            
            // 3. Behalte nur die beste Finals-Phase
            var finalsPhases = tournamentClass.Phases.Where(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals).ToList();
            if (finalsPhases.Any())
            {
                var bestFinals = finalsPhases.OrderByDescending(p => p.IsActive).First();
                cleanPhases.Add(bestFinals);
            }
            
            // 4. Ersetze Phases-Collection
            tournamentClass.Phases.Clear();
            foreach (var phase in cleanPhases)
            {
                tournamentClass.Phases.Add(phase);
            }
            
            // 5. Setze CurrentPhase korrekt
            var knockoutPhase = cleanPhases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.KnockoutPhase);
            if (knockoutPhase != null)
            {
                tournamentClass.CurrentPhase = knockoutPhase;
                groupPhase.IsActive = false;
                groupPhase.IsCompleted = true;
                Console.WriteLine($"DataService: Set CurrentPhase to KnockoutPhase");
            }
            else
            {
                var finalsPhase = cleanPhases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals);
                if (finalsPhase != null)
                {
                    tournamentClass.CurrentPhase = finalsPhase;
                    groupPhase.IsActive = false;
                    groupPhase.IsCompleted = true;
                }
                else
                {
                    tournamentClass.CurrentPhase = groupPhase;
                    groupPhase.IsActive = true;
                    groupPhase.IsCompleted = false;
                }
            }
            
            Console.WriteLine($"DataService: Cleanup complete - Final phases: {tournamentClass.Phases.Count}, CurrentPhase: {tournamentClass.CurrentPhase?.PhaseType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DataService: Error during cleanup for {tournamentClass.Name}: {ex.Message}");
        }
    }
}