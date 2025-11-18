namespace DartTournamentPlaner.Models.PowerScore;

/// <summary>
/// Konfiguration für erweiterte Gruppeneinteilung
/// </summary>
public class GroupDistributionConfig
{
    /// <summary>
    /// Gewählte Klassen für die Einteilung
    /// </summary>
    public List<string> SelectedClasses { get; set; } = new();
    
    /// <summary>
    /// Anzahl Gruppen pro Klasse (Standard)
    /// </summary>
    public int GroupsPerClass { get; set; } = 1;
    
    /// <summary>
    /// Maximale Spieler pro Gruppe (Standard)
    /// </summary>
    public int PlayersPerGroup { get; set; } = 4;
    
    /// <summary>
    /// ✅ NEU: Spezielle Regeln pro Klasse
    /// </summary>
    public Dictionary<string, ClassSpecificRules> ClassRules { get; set; } = new();
    
    /// <summary>
    /// ✅ NEU: Spezielle Regeln pro Gruppe
    /// </summary>
    public Dictionary<string, GroupSpecificRules> GroupRules { get; set; } = new();
    
    /// <summary>
    /// ✅ NEU: Verteilungsmodus
    /// </summary>
    public DistributionMode Mode { get; set; } = DistributionMode.Balanced;
    
    /// <summary>
    /// ✅ NEU: Minimale Spieler pro Gruppe
    /// </summary>
    public int MinPlayersPerGroup { get; set; } = 2;
    
    /// <summary>
    /// ✅ NEU: Maximale Spieler pro Gruppe
    /// </summary>
    public int MaxPlayersPerGroup { get; set; } = 6;
    
    /// <summary>
    /// Verwendete Klassen-Namen
    /// </summary>
    public static readonly List<string> AvailableClasses = new()
    {
        "Platin",
        "Gold",
        "Silber",
        "Bronze",
        "Eisen"
    };
    
    /// <summary>
    /// ✅ NEU: Holt Gruppenanzahl für eine Klasse (mit Override)
    /// </summary>
    public int GetGroupsForClass(string className)
    {
        if (ClassRules.TryGetValue(className, out var rules) && rules.CustomGroupCount.HasValue)
        {
            return rules.CustomGroupCount.Value;
        }
        return GroupsPerClass;
    }
    
    /// <summary>
    /// ✅ NEU: Holt Spieler pro Gruppe für eine Klasse (mit Override)
    /// </summary>
    public int GetPlayersPerGroupForClass(string className)
    {
        if (ClassRules.TryGetValue(className, out var rules) && rules.PlayersPerGroup.HasValue)
        {
            return rules.PlayersPerGroup.Value;
        }
        return PlayersPerGroup; // Fallback auf globalen Wert
    }
    
    /// <summary>
    /// ✅ Holt maximale Spieler für eine Gruppe (mit Override)
    /// </summary>
    public int GetMaxPlayersForGroup(string className, int groupNumber)
    {
        var groupKey = $"{className}_Group{groupNumber}";
        if (GroupRules.TryGetValue(groupKey, out var rules) && rules.CustomMaxPlayers.HasValue)
        {
            return rules.CustomMaxPlayers.Value;
        }
        
        // ✅ FIX: Verwende klassen-spezifische Spieleranzahl falls vorhanden
        return GetPlayersPerGroupForClass(className);
    }
}

/// <summary>
/// ✅ NEU: Spezielle Regeln für eine Klasse
/// </summary>
public class ClassSpecificRules
{
    /// <summary>
    /// Überschreibt die Anzahl der Gruppen für diese Klasse
    /// </summary>
    public int? CustomGroupCount { get; set; }
    
    /// <summary>
    /// ✅ NEU: Spieler pro Gruppe für diese Klasse
    /// </summary>
    public int? PlayersPerGroup { get; set; }
    
    /// <summary>
    /// Priorität dieser Klasse bei der Verteilung (höher = früher)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Soll diese Klasse übersprungen werden?
    /// </summary>
    public bool Skip { get; set; } = false;
}

/// <summary>
/// ✅ NEU: Spezielle Regeln für eine einzelne Gruppe
/// </summary>
public class GroupSpecificRules
{
    /// <summary>
    /// Überschreibt die maximale Spieleranzahl für diese Gruppe
    /// </summary>
    public int? CustomMaxPlayers { get; set; }
    
    /// <summary>
    /// Überschreibt die minimale Spieleranzahl für diese Gruppe
    /// </summary>
    public int? CustomMinPlayers { get; set; }
    
    /// <summary>
    /// Soll diese Gruppe übersprungen werden?
    /// </summary>
    public bool Skip { get; set; } = false;
}

/// <summary>
/// ✅ NEU: Verteilungsmodus für Spieler
/// </summary>
public enum DistributionMode
{
    /// <summary>
    /// Gleichmäßige Verteilung (Standard)
    /// </summary>
    Balanced,
    
    /// <summary>
    /// Snake-Draft (1-2-3-4-4-3-2-1)
    /// </summary>
    SnakeDraft,
    
    /// <summary>
    /// Top-Heavy (Stärkste Spieler in erste Gruppen)
    /// </summary>
    TopHeavy,
    
    /// <summary>
    /// Zufällig (für Testing)
    /// </summary>
    Random
}

/// <summary>
/// Ergebnis der Gruppeneinteilung mit Klassen
/// </summary>
public class GroupDistributionResult
{
    public string ClassName { get; set; } = "";
    public int GroupNumber { get; set; }
    public List<PowerScoringPlayer> Players { get; set; } = new();
    
    public string GetGroupDisplayName()
    {
        return $"{GetClassEmoji()} {ClassName} - Gruppe {GroupNumber}";
    }
    
    private string GetClassEmoji()
    {
        return ClassName switch
        {
            "Platin" => "🏆",
            "Gold" => "🥇",
            "Silber" => "🥈",
            "Bronze" => "🥉",
            "Eisen" => "⚙️",
            _ => "📋"
        };
    }
}
