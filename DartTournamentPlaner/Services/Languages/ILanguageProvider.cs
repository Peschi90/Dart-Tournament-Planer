using System;
using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages;

/// <summary>
/// Interface f�r Sprachdateien-Provider 
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
    /// Gibt alle �bersetzungen f�r diese Sprache zur�ck
    /// </summary>
    /// <returns>Dictionary mit allen �bersetzungen</returns>
    Dictionary<string, string> GetTranslations();
}