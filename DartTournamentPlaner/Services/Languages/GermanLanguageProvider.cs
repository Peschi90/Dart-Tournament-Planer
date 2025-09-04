using System;
using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages.German;

namespace DartTournamentPlaner.Services.Languages;

/// <summary>
/// Deutsche Übersetzungen für den Dart Turnier Planer
/// Verwendet modulare Struktur mit verschiedenen Sektionen
/// </summary>
public class GermanLanguageProvider : ILanguageProvider
{
    public string LanguageCode => "de";
    public string DisplayName => "Deutsch";

    private readonly List<ILanguageSection> _sections;

    public GermanLanguageProvider()
    {
        _sections = new List<ILanguageSection>
        {
            new GermanUILanguageProvider(),
            new GermanHubLanguageProvider(),
            new GermanLicenseLanguageProvider(),
            new GermanTournamentLanguageProvider(),
            new GermanHelpAndApiLanguageProvider(),
            new GermanStatisticsLanguageProvider(),
            new GermanPrintLanguageProvider(),
            new GermanHelpContentLanguageProvider()
        };
    }

    /// <summary>
    /// Ermittelt die aktuelle Assembly-Version der Anwendung
    /// </summary>
    /// <returns>Versionsnummer als String (z.B. "1.2.3")</returns>
    private string GetCurrentAssemblyVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            
            if (version != null)
            {
                // Format: Major.Minor.Build (ohne Revision für bessere Lesbarkeit)
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            
            return "1.0.0"; // Fallback
        }
        catch
        {
            return "1.0.0"; // Fallback bei Fehlern
        }
    }

    /// <summary>
    /// Generiert dynamischen About-Text mit aktueller Assembly-Version
    /// </summary>
    /// <returns>About-Text mit aktueller Versionsnummer</returns>
    private string GetDynamicAboutText()
    {
        var currentVersion = GetCurrentAssemblyVersion();
        return $"Dart Turnier Planer v{currentVersion}\n\nEine moderne Turnierverwaltungssoftware.\n\n© 2025 by I3uLL3t";
    }

    public Dictionary<string, string> GetTranslations()
    {
        var allTranslations = new Dictionary<string, string>();

        // Dynamische About-Text-Methode beibehalten
        allTranslations["AboutText"] = GetDynamicAboutText();

        // Alle Teilbereiche zusammenführen
        foreach (var section in _sections)
        {
            var sectionTranslations = section.GetSectionTranslations();
            foreach (var kvp in sectionTranslations)
            {
                // Überschreibungen verhindern und bei Konflikten warnen
                if (allTranslations.ContainsKey(kvp.Key))
                {
                    Console.WriteLine($"Warning: Duplicate translation key '{kvp.Key}' found in German translations");
                }
                else
                {
                    allTranslations[kvp.Key] = kvp.Value;
                }
            }
        }

        return allTranslations;
    }
}