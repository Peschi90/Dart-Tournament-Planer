using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Models.PowerScore;

namespace DartTournamentPlaner.Services.PowerScore;

/// <summary>
/// Service für die Konvertierung von PowerScoring Gruppeneinteilungen zu Turnier-Daten
/// 
/// VERANTWORTLICHKEIT:
/// - Konvertiert PowerScoring GroupDistributionResult zu TournamentClass
/// - Erstellt Gruppen und weist Spieler zu
/// - Validiert Daten vor der Konvertierung
/// 
/// NICHT VERANTWORTLICH FÜR:
/// - Speichern/Laden von Turnieren (-> DataService)
/// - UI-Logik (-> Windows/Dialogs)
/// - PowerScoring Session Management (-> PowerScoringService)
/// </summary>
public class PowerScoringToTournamentService
{
    /// <summary>
    /// Konvertiert eine PowerScoring Distribution zu Tournament-Klassen
    /// </summary>
    /// <param name="distribution">Die generierte Gruppeneinteilung</param>
    /// <returns>Liste von TournamentClass Objekten</returns>
    public List<TournamentClass> ConvertDistributionToTournamentClasses(
        List<GroupDistributionResult> distribution)
    {
        if (distribution == null || distribution.Count == 0)
        {
            throw new ArgumentException("Distribution cannot be null or empty", nameof(distribution));
        }

        var tournamentClasses = new List<TournamentClass>();
        
        // Gruppiere nach Klassennamen
        var groupedByClass = distribution.GroupBy(g => g.ClassName);
        
        foreach (var classGroup in groupedByClass)
        {
            var tournamentClass = CreateTournamentClass(classGroup.Key, classGroup.ToList());
            tournamentClasses.Add(tournamentClass);
        }
        
        System.Diagnostics.Debug.WriteLine($"✅ Converted {distribution.Count} groups to {tournamentClasses.Count} tournament classes");
        
        return tournamentClasses;
    }
    
    /// <summary>
    /// Erstellt eine TournamentClass aus PowerScoring Gruppen
    /// </summary>
    private TournamentClass CreateTournamentClass(
        string className, 
        List<GroupDistributionResult> groups)
    {
        var tournamentClass = new TournamentClass
        {
            Name = className,
            Groups = new System.Collections.ObjectModel.ObservableCollection<Group>() // ✅ FIX: ObservableCollection
        };
        
        foreach (var groupResult in groups)
        {
            var group = CreateGroup(groupResult);
            tournamentClass.Groups.Add(group);
        }
        
        System.Diagnostics.Debug.WriteLine($"   Created class '{className}' with {groups.Count} groups");
        
        return tournamentClass;
    }
    
    /// <summary>
    /// Erstellt eine Group aus einem GroupDistributionResult
    /// </summary>
    private Group CreateGroup(GroupDistributionResult groupResult)
    {
        var group = new Group
        {
            Name = $"Gruppe {groupResult.GroupNumber}",
            Players = new System.Collections.ObjectModel.ObservableCollection<Player>() // ✅ FIX: ObservableCollection<Player>
        };
        
        // Füge Spieler hinzu (sortiert nach Ranking/Score)
        foreach (var psPlayer in groupResult.Players.OrderByDescending(p => p.AverageScore))
        {
            // ✅ FIX: Erstelle Player-Objekt
            var player = new Player
            {
                Name = psPlayer.Name
            };
            group.Players.Add(player);
        }
        
        System.Diagnostics.Debug.WriteLine($"      Group {groupResult.GroupNumber}: {group.Players.Count} players");
        
        return group;
    }
    
    /// <summary>
    /// Validiert ob die Distribution für Tournament-Erstellung geeignet ist
    /// </summary>
    public ValidationResult ValidateDistribution(List<GroupDistributionResult> distribution)
    {
        var result = new ValidationResult { IsValid = true };
        
        if (distribution == null || distribution.Count == 0)
        {
            result.IsValid = false;
            result.Errors.Add("Keine Gruppeneinteilung vorhanden");
            return result;
        }
        
        // Prüfe ob alle Gruppen Spieler haben
        var emptyGroups = distribution.Where(g => g.Players.Count == 0).ToList();
        if (emptyGroups.Any())
        {
            result.Warnings.Add($"{emptyGroups.Count} Gruppe(n) ohne Spieler werden ignoriert");
        }
        
        // Prüfe Mindestanzahl Spieler pro Klasse
        var groupedByClass = distribution.GroupBy(g => g.ClassName);
        foreach (var classGroup in groupedByClass)
        {
            var totalPlayers = classGroup.Sum(g => g.Players.Count);
            if (totalPlayers < 2)
            {
                result.IsValid = false;
                result.Errors.Add($"Klasse '{classGroup.Key}' hat nur {totalPlayers} Spieler (Minimum: 2)");
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Erstellt eine Vorschau-Zusammenfassung der zu erstellenden Turnier-Struktur
    /// </summary>
    public TournamentPreview CreatePreview(List<GroupDistributionResult> distribution)
    {
        var preview = new TournamentPreview
        {
            Classes = new List<ClassPreview>()
        };
        
        var groupedByClass = distribution.GroupBy(g => g.ClassName);
        
        foreach (var classGroup in groupedByClass)
        {
            var classPreview = new ClassPreview
            {
                ClassName = classGroup.Key,
                GroupCount = classGroup.Count(),
                TotalPlayers = classGroup.Sum(g => g.Players.Count),
                Groups = classGroup.Select(g => new GroupPreview
                {
                    GroupNumber = g.GroupNumber,
                    PlayerCount = g.Players.Count,
                    Players = g.Players.Select(p => p.Name).ToList()
                }).ToList()
            };
            
            preview.Classes.Add(classPreview);
        }
        
        preview.TotalClasses = preview.Classes.Count;
        preview.TotalGroups = preview.Classes.Sum(c => c.GroupCount);
        preview.TotalPlayers = preview.Classes.Sum(c => c.TotalPlayers);
        
        return preview;
    }
}

/// <summary>
/// Validierungsergebnis für Distribution
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public bool HasWarnings => Warnings.Count > 0;
    
    public string GetSummary()
    {
        if (IsValid && !HasWarnings)
            return "✅ Validation successful";
        
        var summary = IsValid ? "⚠️ Validation passed with warnings:\n" : "❌ Validation failed:\n";
        
        foreach (var error in Errors)
        {
            summary += $"  - {error}\n";
        }
        
        foreach (var warning in Warnings)
        {
            summary += $"  ⚠ {warning}\n";
        }
        
        return summary;
    }
}

/// <summary>
/// Vorschau der zu erstellenden Turnier-Struktur
/// </summary>
public class TournamentPreview
{
    public int TotalClasses { get; set; }
    public int TotalGroups { get; set; }
    public int TotalPlayers { get; set; }
    public List<ClassPreview> Classes { get; set; } = new();
    
    public string GetSummary()
    {
        return $"{TotalClasses} Klassen, {TotalGroups} Gruppen, {TotalPlayers} Spieler";
    }
}

public class ClassPreview
{
    public string ClassName { get; set; } = "";
    public int GroupCount { get; set; }
    public int TotalPlayers { get; set; }
    public List<GroupPreview> Groups { get; set; } = new();
}

public class GroupPreview
{
    public int GroupNumber { get; set; }
    public int PlayerCount { get; set; }
    public List<string> Players { get; set; } = new();
}
