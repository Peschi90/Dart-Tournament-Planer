using System;
using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages.English;

namespace DartTournamentPlaner.Services.Languages;

/// <summary> 
/// English translations for the Dart Tournament Planner
/// Uses modular structure with different sections
/// </summary>
public class EnglishLanguageProvider : ILanguageProvider
{
    public string LanguageCode => "en";
    public string DisplayName => "English";

    private readonly List<ILanguageSection> _sections;

    public EnglishLanguageProvider()
    {
        _sections = new List<ILanguageSection>
        {
            new EnglishUILanguageProvider(),
            new EnglishStartupLanguageProvider(), // ✅ NEU: Startup/Splash Screen Übersetzungen
            new EnglishHubLanguageProvider(),
            new EnglishLicenseLanguageProvider(),
            new EnglishTournamentLanguageProvider(),
            new EnglishKnockoutLanguageProvider(), // ✅ NEU: KO-Tab spezifische Übersetzungen
            new EnglishHelpAndApiLanguageProvider(),
            new EnglishStatisticsLanguageProvider(),
            new EnglishPlayerStatisticsLanguageProvider(), // ✅ NEU: PlayerStatistics übersetzungen
            new EnglishPrintLanguageProvider(),
            new EnglishHelpContentLanguageProvider()
        };
    }

    /// <summary>
    /// Gets the current assembly version of the application
    /// </summary>
    /// <returns>Version number as string (e.g. "1.2.3")</returns>
    private string GetCurrentAssemblyVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            
            if (version != null)
            {
                // Format: Major.Minor.Build (without revision for better readability)
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            
            return "1.0.0"; // Fallback
        }
        catch
        {
            return "1.0.0"; // Fallback on errors
        }
    }

    /// <summary>
    /// Generates dynamic About text with current assembly version
    /// </summary>
    /// <returns>About text with current version number</returns>
    private string GetDynamicAboutText()
    {
        var currentVersion = GetCurrentAssemblyVersion();
        return $"Dart Tournament Planner v{currentVersion}\n\nA modern tournament management software.\n\n© 2025 by I3uLL3t";
    }

    public Dictionary<string, string> GetTranslations()
    {
        var allTranslations = new Dictionary<string, string>();

        // Keep dynamic About text method
        allTranslations["AboutText"] = GetDynamicAboutText();

        // Merge all sections
        foreach (var section in _sections)
        {
            var sectionTranslations = section.GetSectionTranslations();
            foreach (var kvp in sectionTranslations)
            {
                // Prevent overwrites and warn on conflicts
                if (allTranslations.ContainsKey(kvp.Key))
                {
                    Console.WriteLine($"Warning: Duplicate translation key '{kvp.Key}' found in English translations");
                }
                else
                {
                    allTranslations[kvp.Key] = kvp.Value;
                }
            }
        }

        // Add any remaining translations that might not be covered yet
        AddRemainingTranslations(allTranslations);

        return allTranslations;
    }

    /// <summary>
    /// Add any remaining translations that haven't been moved to section providers yet
    /// </summary>
    private void AddRemainingTranslations(Dictionary<string, string> translations)
    {
        // Add any missing translations that are still needed
        var remainingTranslations = new Dictionary<string, string>
        {
            // Tournament Overview specific translations
            ["TournamentOverview"] = "🏆 Tournament Overview",
            ["OverviewMode"] = "Overview Mode",
            ["Configure"] = "Configure",
            ["ManualMode"] = "Manual Mode",
            ["AutoCyclingActive"] = "Auto Cycling Active",
            ["CyclingStopped"] = "Auto Cycling Stopped",
            ["ManualControl"] = "Manual Control",
            ["Showing"] = "Showing",
            ["Unknown"] = "Unknown",
            ["StartCycling"] = "Start",
            ["StopCycling"] = "Stop",
            ["WinnerBracketMatches"] = "Matches in Winner Bracket",
            ["WinnerBracketTree"] = "Winner Bracket",
            ["LoserBracketMatches"] = "Matches in Loser Bracket",
            ["LoserBracketTree"] = "Loser Bracket",
            ["RoundColumn"] = "Round",
            ["PositionShort"] = "Pos",
            ["PointsShort"] = "Pts",
            ["WinDrawLoss"] = "W-D-L",
            ["NoLoserBracketMatches"] = "No matches available in loser bracket",
            ["NoWinnerBracketMatches"] = "No matches available in winner bracket",
            ["TournamentTreeWillShow"] = "The tournament tree will be displayed once the knockout phase begins",
            ["SelectGroup"] = "Select Group",
            ["NoGroupSelected"] = "No group selected",

            // Additional translations that might be needed
            ["ValidationError"] = "Validation error",
            ["ValidationErrorTitle"] = "Validation Error"
        };

        foreach (var kvp in remainingTranslations)
        {
            if (!translations.ContainsKey(kvp.Key))
            {
                translations[kvp.Key] = kvp.Value;
            }
        }
    }
}