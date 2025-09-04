using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages;

/// <summary>
/// Interface für Sprachsektionen
/// Ermöglicht die modulare Aufteilung von Übersetzungen in thematische Bereiche
/// </summary>
public interface ILanguageSection
{
    /// <summary>
    /// Gibt die Übersetzungen für diese Sektion zurück
    /// </summary>
    /// <returns>Dictionary mit allen Übersetzungen dieser Sektion</returns>
    Dictionary<string, string> GetSectionTranslations();
}