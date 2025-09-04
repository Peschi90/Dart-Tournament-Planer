using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages;

/// <summary>
/// Interface f�r Sprachsektionen
/// Erm�glicht die modulare Aufteilung von �bersetzungen in thematische Bereiche
/// </summary>
public interface ILanguageSection
{
    /// <summary>
    /// Gibt die �bersetzungen f�r diese Sektion zur�ck
    /// </summary>
    /// <returns>Dictionary mit allen �bersetzungen dieser Sektion</returns>
    Dictionary<string, string> GetSectionTranslations();
}