using System.IO;
using Newtonsoft.Json;
using DartTournamentPlaner.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                Console.WriteLine("?? [DATA-SERVICE] Loading tournament data from JSON...");
                
                var json = await File.ReadAllTextAsync(_dataPath);
                var data = JsonConvert.DeserializeObject<TournamentData>(json);
                
                if (data != null)
                {
                    Console.WriteLine($"? [DATA-SERVICE] Tournament data loaded with {data.TournamentClasses.Count} classes");
                    
                    // WICHTIG: Nach dem Laden die Daten bereinigen und validieren
                    foreach (var tournamentClass in data.TournamentClasses)
                    {
                        CleanupDuplicatePhases(tournamentClass);
                        
                        // ? NEU: Validiere und repariere Statistiken nach dem Laden
                        tournamentClass.ValidateAndRepairStatistics();
                        
                        Console.WriteLine($"? [DATA-SERVICE] Statistics loaded for class {tournamentClass.Name}: " +
                            $"{tournamentClass.PlayerStatisticsData.Count} players");
                    }
                    
                    // ?? UUID-SYSTEM: Validiere und repariere UUIDs nach dem Laden
                    await ValidateAndRepairUuidsAfterLoading(data);
                }
                
                return data ?? new TournamentData();
            }
            else
            {
                Console.WriteLine("?? [DATA-SERVICE] No existing data file found, creating new tournament data");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [DATA-SERVICE] Error loading tournament data: {ex.Message}");
            Console.WriteLine($"?? [DATA-SERVICE] Stack trace: {ex.StackTrace}");
        }
        
        return new TournamentData();
    }

    public async Task SaveTournamentDataAsync(TournamentData data)
    {
        try
        {
            data.LastModified = DateTime.Now;
            
            // ?? UUID-SYSTEM: Stelle sicher, dass alle Matches gültige UUIDs haben vor dem Speichern
            Console.WriteLine("?? [DATA-SERVICE] Ensuring all matches have valid UUIDs before saving...");
            
            int totalMatches = 0;
            int generatedUuids = 0;
            
            foreach (var tournamentClass in data.TournamentClasses)
            {
                Console.WriteLine($"?? [DATA-SERVICE] Processing {tournamentClass.Name}...");
                
                // 1. Gruppen-Matches
                foreach (var group in tournamentClass.Groups)
                {
                    foreach (var match in group.Matches)
                    {
                        totalMatches++;
                        if (!match.HasValidUniqueId())
                        {
                            match.EnsureUniqueId();
                            generatedUuids++;
                            Console.WriteLine($"   ?? Generated UUID for Group Match {match.Id}: {match.UniqueId}");
                        }
                    }
                }
                
                // 2. Finals-Matches
                var finalsPhase = tournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals);
                if (finalsPhase?.FinalsGroup?.Matches != null)
                {
                    foreach (var match in finalsPhase.FinalsGroup.Matches)
                    {
                        totalMatches++;
                        if (!match.HasValidUniqueId())
                        {
                            match.EnsureUniqueId();
                            generatedUuids++;
                            Console.WriteLine($"   ?? Generated UUID for Finals Match {match.Id}: {match.UniqueId}");
                        }
                    }
                }
                
                // 3. K.O.-Matches (Winner & Loser Bracket)
                var koPhase = tournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.KnockoutPhase);
                if (koPhase != null)
                {
                    // Winner Bracket
                    if (koPhase.WinnerBracket != null)
                    {
                        foreach (var match in koPhase.WinnerBracket)
                        {
                            totalMatches++;
                            if (!match.HasValidUniqueId())
                            {
                                match.EnsureUniqueId();
                                generatedUuids++;
                                Console.WriteLine($"   ? Generated UUID for Winner Bracket Match {match.Id}: {match.UniqueId}");
                            }
                        }
                    }
                    
                    // Loser Bracket
                    if (koPhase.LoserBracket != null)
                    {
                        foreach (var match in koPhase.LoserBracket)
                        {
                            totalMatches++;
                            if (!match.HasValidUniqueId())
                            {
                                match.EnsureUniqueId();
                                generatedUuids++;
                                Console.WriteLine($"   ?? Generated UUID for Loser Bracket Match {match.Id}: {match.UniqueId}");
                            }
                        }
                    }
                }
            }
            
            Console.WriteLine($"? [DATA-SERVICE] UUID validation complete:");
            Console.WriteLine($"   ?? Total matches: {totalMatches}");
            Console.WriteLine($"   ?? Generated UUIDs: {generatedUuids}");
            Console.WriteLine($"   ? Matches already with UUID: {totalMatches - generatedUuids}");
            
            // ?? ERWEITERTE JSON-SERIALISIERUNG: Konfiguriere für UUID-System
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include, // UUIDs könnten null sein
                DefaultValueHandling = DefaultValueHandling.Include, // Alle Werte speichern
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                // Metadaten für UUID-System
                MetadataPropertyHandling = MetadataPropertyHandling.Default
            };
            
            var json = JsonConvert.SerializeObject(data, jsonSettings);
            
            // Validiere JSON vor dem Speichern
            if (string.IsNullOrWhiteSpace(json) || json.Length < 100)
            {
                throw new InvalidOperationException("Generated JSON appears to be invalid or too small");
            }
            
            await File.WriteAllTextAsync(_dataPath, json);
            
            Console.WriteLine($"?? [DATA-SERVICE] Tournament data saved successfully with {totalMatches} matches and UUIDs");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [DATA-SERVICE] Error saving tournament data: {ex.Message}");
            Console.WriteLine($"?? [DATA-SERVICE] Stack trace: {ex.StackTrace}");
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

    /// <summary>
    /// ?? NEUE METHODE: Validiert und repariert UUIDs nach dem Laden aus JSON
    /// </summary>
    private async Task ValidateAndRepairUuidsAfterLoading(TournamentData data)
    {
        try
        {
            Console.WriteLine("?? [DATA-SERVICE] Validating UUIDs after loading from JSON...");
            
            int totalMatches = 0;
            int validUuids = 0;
            int repairedUuids = 0;
            int duplicateUuids = 0;
            
            var seenUuids = new HashSet<string>();
            
            foreach (var tournamentClass in data.TournamentClasses)
            {
                Console.WriteLine($"?? [DATA-SERVICE] Checking {tournamentClass.Name}...");
                
                // 1. Gruppen-Matches validieren
                foreach (var group in tournamentClass.Groups)
                {
                    foreach (var match in group.Matches)
                    {
                        totalMatches++;
                        
                        if (match.HasValidUniqueId())
                        {
                            if (seenUuids.Contains(match.UniqueId))
                            {
                                // Duplikat gefunden - neue UUID generieren
                                var oldUuid = match.UniqueId;
                                match.GenerateNewUniqueId();
                                duplicateUuids++;
                                Console.WriteLine($"   ?? Repaired duplicate UUID for Group Match {match.Id}: {oldUuid} -> {match.UniqueId}");
                            }
                            else
                            {
                                seenUuids.Add(match.UniqueId);
                                validUuids++;
                            }
                        }
                        else
                        {
                            match.EnsureUniqueId();
                            if (!string.IsNullOrEmpty(match.UniqueId))
                            {
                                seenUuids.Add(match.UniqueId);
                            }
                            repairedUuids++;
                            Console.WriteLine($"   ?? Repaired missing UUID for Group Match {match.Id}: {match.UniqueId}");
                        }
                    }
                }
                
                // 2. Finals-Matches validieren
                var finalsPhase = tournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals);
                if (finalsPhase?.FinalsGroup?.Matches != null)
                {
                    foreach (var match in finalsPhase.FinalsGroup.Matches)
                    {
                        totalMatches++;
                        
                        if (match.HasValidUniqueId())
                        {
                            if (seenUuids.Contains(match.UniqueId))
                            {
                                var oldUuid = match.UniqueId;
                                match.GenerateNewUniqueId();
                                duplicateUuids++;
                                Console.WriteLine($"   ?? Repaired duplicate UUID for Finals Match {match.Id}: {oldUuid} -> {match.UniqueId}");
                            }
                            else
                            {
                                seenUuids.Add(match.UniqueId);
                                validUuids++;
                            }
                        }
                        else
                        {
                            match.EnsureUniqueId();
                            if (!string.IsNullOrEmpty(match.UniqueId))
                            {
                                seenUuids.Add(match.UniqueId);
                            }
                            repairedUuids++;
                            Console.WriteLine($"   ?? Repaired missing UUID for Finals Match {match.Id}: {match.UniqueId}");
                        }
                    }
                }
                
                // 3. K.O.-Matches validieren
                var koPhase = tournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.KnockoutPhase);
                if (koPhase != null)
                {
                    // Winner Bracket
                    if (koPhase.WinnerBracket != null)
                    {
                        foreach (var match in koPhase.WinnerBracket)
                        {
                            totalMatches++;
                            
                            if (match.HasValidUniqueId())
                            {
                                if (seenUuids.Contains(match.UniqueId))
                                {
                                    var oldUuid = match.UniqueId;
                                    match.GenerateNewUniqueId();
                                    duplicateUuids++;
                                    Console.WriteLine($"   ?? Repaired duplicate UUID for WB Match {match.Id}: {oldUuid} -> {match.UniqueId}");
                                }
                                else
                                {
                                    seenUuids.Add(match.UniqueId);
                                    validUuids++;
                                }
                            }
                            else
                            {
                                match.EnsureUniqueId();
                                if (!string.IsNullOrEmpty(match.UniqueId))
                                {
                                    seenUuids.Add(match.UniqueId);
                                }
                                repairedUuids++;
                                Console.WriteLine($"   ? Repaired missing UUID for WB Match {match.Id}: {match.UniqueId}");
                            }
                        }
                    }
                    
                    // Loser Bracket
                    if (koPhase.LoserBracket != null)
                    {
                        foreach (var match in koPhase.LoserBracket)
                        {
                            totalMatches++;
                            
                            if (match.HasValidUniqueId())
                            {
                                if (seenUuids.Contains(match.UniqueId))
                                {
                                    var oldUuid = match.UniqueId;
                                    match.GenerateNewUniqueId();
                                    duplicateUuids++;
                                    Console.WriteLine($"   ?? Repaired duplicate UUID for LB Match {match.Id}: {oldUuid} -> {match.UniqueId}");
                                }
                                else
                                {
                                    seenUuids.Add(match.UniqueId);
                                    validUuids++;
                                }
                            }
                            else
                            {
                                match.EnsureUniqueId();
                                if (!string.IsNullOrEmpty(match.UniqueId))
                                {
                                    seenUuids.Add(match.UniqueId);
                                }
                                repairedUuids++;
                                Console.WriteLine($"   ?? Repaired missing UUID for LB Match {match.Id}: {match.UniqueId}");
                            }
                        }
                    }
                }
            }
            
            Console.WriteLine($"? [DATA-SERVICE] UUID validation complete:");
            Console.WriteLine($"   ?? Total matches: {totalMatches}");
            Console.WriteLine($"   ? Valid UUIDs: {validUuids}");
            Console.WriteLine($"   ?? Repaired UUIDs: {repairedUuids}");
            Console.WriteLine($"   ?? Fixed duplicates: {duplicateUuids}");
            
            // Speichere automatisch wenn Reparaturen durchgeführt wurden
            if (repairedUuids > 0 || duplicateUuids > 0)
            {
                Console.WriteLine($"?? [DATA-SERVICE] Auto-saving data after UUID repairs...");
                await SaveTournamentDataAsync(data);
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? [DATA-SERVICE] Error validating UUIDs after loading: {ex.Message}");
            Console.WriteLine($"?? [DATA-SERVICE] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Lädt ein Turnier aus einer JSON-Datei
    /// ERWEITERT: Lädt auch Spieler-Statistiken
    /// </summary>
    /// <param name="filePath">Pfad zur JSON-Datei</param>
    /// <returns>Das geladene TournamentData-Objekt oder null bei Fehler</returns>
    public TournamentData? LoadTournamentFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"Tournament file not found: {filePath}");
                return null;
            }

            var json = File.ReadAllText(filePath);
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                WriteIndented = true
            };

            var tournamentData = System.Text.Json.JsonSerializer.Deserialize<TournamentData>(json, options);
            
            if (tournamentData != null)
            {
                System.Diagnostics.Debug.WriteLine($"? Tournament loaded successfully from: {filePath}");
                
                // Post-Processing für alle TournamentClasses
                foreach (var tournamentClass in tournamentData.TournamentClasses)
                {
                    // ? NEU: Validiere und repariere Statistiken
                    tournamentClass.ValidateAndRepairStatistics();
                    
                    System.Diagnostics.Debug.WriteLine($"Tournament class processed: {tournamentClass.Name} " +
                        $"(Groups: {tournamentClass.Groups.Count}, " +
                        $"Phases: {tournamentClass.Phases.Count}, " +
                        $"Statistics: {tournamentClass.PlayerStatisticsData.Count})");
                }
            }

            return tournamentData;
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            System.Diagnostics.Debug.WriteLine($"JSON parsing error: {jsonEx.Message}");
            System.Windows.MessageBox.Show($"Die Turnier-Datei konnte nicht gelesen werden.\n\nFehler: {jsonEx.Message}", "JSON-Fehler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading tournament: {ex.Message}");
            System.Windows.MessageBox.Show($"Das Turnier konnte nicht geladen werden.\n\nFehler: {ex.Message}", "Ladefehler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return null;
        }
    }

    /// <summary>
    /// Speichert ein Turnier in eine JSON-Datei
    /// ERWEITERT: Speichert auch Spieler-Statistiken
    /// </summary>
    /// <param name="tournamentData">Das zu speichernde TournamentData-Objekt</param>
    /// <param name="filePath">Pfad zur JSON-Datei</param>
    /// <returns>True bei Erfolg, False bei Fehler</returns>
    public bool SaveTournamentToFile(TournamentData tournamentData, string filePath)
    {
        try
        {
            // ? NEU: Vor dem Speichern alle Statistiken synchronisieren
            foreach (var tournamentClass in tournamentData.TournamentClasses)
            {
                // Synchronisiere aktuelle Statistiken vom Manager zu den JSON-Daten
                try
                {
                    var statsManager = tournamentClass.StatisticsManager;
                    tournamentClass.PlayerStatisticsData.Clear();
                    
                    foreach (var playerStats in statsManager.PlayerStatistics)
                    {
                        tournamentClass.PlayerStatisticsData[playerStats.Key] = playerStats.Value;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[DATA-SERVICE] Synced {tournamentClass.PlayerStatisticsData.Count} player statistics for class {tournamentClass.Name}");
                }
                catch (Exception statsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[DATA-SERVICE] Error syncing statistics for {tournamentClass.Name}: {statsEx.Message}");
                }
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // Erstelle Backup falls Datei existiert
            if (File.Exists(filePath))
            {
                var backupPath = filePath.Replace(".json", $"_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                File.Copy(filePath, backupPath, true);
                System.Diagnostics.Debug.WriteLine($"Backup created: {backupPath}");
            }

            var json = System.Text.Json.JsonSerializer.Serialize(tournamentData, options);
            File.WriteAllText(filePath, json);

            System.Diagnostics.Debug.WriteLine($"? Tournament saved successfully to: {filePath}");
            System.Diagnostics.Debug.WriteLine($"   Classes: {tournamentData.TournamentClasses.Count}");
            
            // Debug: Statistik-Info
            var totalStats = tournamentData.TournamentClasses.Sum(tc => tc.PlayerStatisticsData.Count);
            if (totalStats > 0)
            {
                System.Diagnostics.Debug.WriteLine($"   Player Statistics: {totalStats} total across all classes");
            }
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving tournament: {ex.Message}");
            System.Windows.MessageBox.Show($"Das Turnier konnte nicht gespeichert werden.\n\nFehler: {ex.Message}", "Speicherfehler", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return false;
        }
    }
}