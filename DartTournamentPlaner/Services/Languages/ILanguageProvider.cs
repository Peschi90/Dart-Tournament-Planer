using System;
using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages;

/// <summary>
/// Interface für Sprachdateien-Provider 
/// Definiert die Methoden, die jede Sprachdatei implementieren muss
/// </summary>
public interface ILanguageProvider
{
    /// <summary>
    /// Sprachcode (z.B. "de", "en")
    /// </summary>
    string LanguageCode { get; }
    
    /// <summary>
    /// Anzeigename der Sprache (z.B. "Deutsch", "English")
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Gibt alle Übersetzungen für diese Sprache zurück
    /// </summary>
    /// <returns>Dictionary mit allen Übersetzungen</returns>
    Dictionary<string, string> GetTranslations();
}