using System;
using System.Collections.Generic;
using System.Linq;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services
{
    /// <summary>
    /// Service für UUID-Migration bestehender Matches
    /// </summary>
    public static class UuidMigrationService
    {
        /// <summary>
        /// Migriert alle Matches in Gruppen zu UUID-System
        /// </summary>
        public static void MigrateGroupMatches(List<Group> groups)
        {
            Console.WriteLine("?? [UUID-MIGRATION] Starting UUID migration for group matches...");
            
            int migratedCount = 0;
            int totalMatches = 0;
            
            foreach (var group in groups)
            {
                foreach (var match in group.Matches)
                {
                    totalMatches++;
                    
                    if (!match.HasValidUniqueId())
                    {
                        match.GenerateNewUniqueId();
                        migratedCount++;
                        Console.WriteLine($"   ? Migrated Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name} -> {match.UniqueId}");
                    }
                    else
                    {
                        Console.WriteLine($"   ? Match {match.Id} already has UUID: {match.UniqueId}");
                    }
                }
            }
            
            Console.WriteLine($"? [UUID-MIGRATION] Group matches migration complete:");
            Console.WriteLine($"   ?? Total matches: {totalMatches}");
            Console.WriteLine($"   ?? Migrated: {migratedCount}");
            Console.WriteLine($"   ? Already had UUIDs: {totalMatches - migratedCount}");
        }
        
        /// <summary>
        /// Migriert alle KO-Matches zu UUID-System
        /// </summary>
        public static void MigrateKnockoutMatches(List<KnockoutMatch> knockoutMatches)
        {
            Console.WriteLine("?? [UUID-MIGRATION] Starting UUID migration for knockout matches...");
            
            int migratedCount = 0;
            int totalMatches = knockoutMatches?.Count ?? 0;
            
            if (knockoutMatches != null)
            {
                foreach (var match in knockoutMatches)
                {
                    if (!match.HasValidUniqueId())
                    {
                        match.GenerateNewUniqueId();
                        migratedCount++;
                        Console.WriteLine($"   ? Migrated KO Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name} ({match.Round}) -> {match.UniqueId}");
                    }
                    else
                    {
                        Console.WriteLine($"   ? KO Match {match.Id} already has UUID: {match.UniqueId}");
                    }
                }
            }
            
            Console.WriteLine($"? [UUID-MIGRATION] Knockout matches migration complete:");
            Console.WriteLine($"   ?? Total KO matches: {totalMatches}");
            Console.WriteLine($"   ?? Migrated: {migratedCount}");
            Console.WriteLine($"   ? Already had UUIDs: {totalMatches - migratedCount}");
        }
        
        /// <summary>
        /// Validiert alle UUIDs in einem Tournament
        /// </summary>
        public static UuidValidationResult ValidateAllUuids(List<Group> groups, List<KnockoutMatch> knockoutMatches = null)
        {
            Console.WriteLine("?? [UUID-VALIDATION] Validating all UUIDs...");
            
            var result = new UuidValidationResult();
            var allUuids = new HashSet<string>();
            
            // Validiere Group Matches
            foreach (var group in groups ?? new List<Group>())
            {
                foreach (var match in group.Matches)
                {
                    result.TotalMatches++;
                    
                    if (match.HasValidUniqueId())
                    {
                        result.ValidUuids++;
                        
                        if (!allUuids.Add(match.UniqueId))
                        {
                            result.DuplicateUuids++;
                            result.DuplicateDetails.Add($"Group Match {match.Id}: {match.UniqueId}");
                        }
                    }
                    else
                    {
                        result.InvalidUuids++;
                        result.InvalidDetails.Add($"Group Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}");
                    }
                }
            }
            
            // Validiere KO Matches
            foreach (var match in knockoutMatches ?? new List<KnockoutMatch>())
            {
                result.TotalMatches++;
                
                if (match.HasValidUniqueId())
                {
                    result.ValidUuids++;
                    
                    if (!allUuids.Add(match.UniqueId))
                    {
                        result.DuplicateUuids++;
                        result.DuplicateDetails.Add($"KO Match {match.Id}: {match.UniqueId}");
                    }
                }
                else
                {
                    result.InvalidUuids++;
                    result.InvalidDetails.Add($"KO Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name} ({match.Round})");
                }
            }
            
            Console.WriteLine($"?? [UUID-VALIDATION] Validation results:");
            Console.WriteLine($"   ?? Total matches: {result.TotalMatches}");
            Console.WriteLine($"   ? Valid UUIDs: {result.ValidUuids}");
            Console.WriteLine($"   ? Invalid UUIDs: {result.InvalidUuids}");
            Console.WriteLine($"   ?? Duplicate UUIDs: {result.DuplicateUuids}");
            
            if (result.InvalidDetails.Any())
            {
                Console.WriteLine("?? [UUID-VALIDATION] Matches without valid UUID:");
                foreach (var detail in result.InvalidDetails)
                {
                    Console.WriteLine($"     {detail}");
                }
            }
            
            if (result.DuplicateDetails.Any())
            {
                Console.WriteLine("?? [UUID-VALIDATION] Duplicate UUIDs found:");
                foreach (var detail in result.DuplicateDetails)
                {
                    Console.WriteLine($"     {detail}");
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Ergebnis der UUID-Validierung
    /// </summary>
    public class UuidValidationResult
    {
        public int TotalMatches { get; set; }
        public int ValidUuids { get; set; }
        public int InvalidUuids { get; set; }
        public int DuplicateUuids { get; set; }
        public List<string> InvalidDetails { get; set; } = new List<string>();
        public List<string> DuplicateDetails { get; set; } = new List<string>();
        
        public bool IsValid => InvalidUuids == 0 && DuplicateUuids == 0;
        public double ValidPercentage => TotalMatches > 0 ? (double)ValidUuids / TotalMatches * 100 : 0;
    }
}