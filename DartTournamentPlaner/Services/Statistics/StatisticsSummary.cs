using System;

namespace DartTournamentPlaner.Services.Statistics;

/// <summary>
/// ? ERWEITERT: Zusammenfassung aller Statistiken einer Klasse
/// </summary>
public class StatisticsSummary
{
    public int TotalPlayers { get; set; }
    public int TotalMatches { get; set; }
    public int TotalLegs { get; set; }
    public int TotalMaximums { get; set; }
    public int TotalHighFinishes { get; set; }
    public double AverageOverallAverage { get; set; }
    
    // ? NEU: Erweiterte Zusammenfassungs-Statistiken
    public double HighestLegAverage { get; set; } = 0.0;
    public int HighestFinishScore { get; set; } = 0;
    public int FewestDartsToFinish { get; set; } = 0;

    public override string ToString()
    {
        return $"Players: {TotalPlayers}, Matches: {TotalMatches}, Avg: {AverageOverallAverage:F1}, Max: {TotalMaximums}";
    }
}