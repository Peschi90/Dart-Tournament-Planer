using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using DartTournamentPlaner.Services.Languages;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Service zur Verwaltung der Lokalisierung/Übersetzung der Anwendung
/// Unterstützt mehrere Sprachen über separate Language Provider
/// Implementiert INotifyPropertyChanged für UI-Updates bei Sprachwechsel
/// </summary>
public class LocalizationService : INotifyPropertyChanged
{
    // Dictionary mit allen verfügbaren Übersetzungen
    // Erste Ebene: Sprachcodes (de, en)
    // Zweite Ebene: Übersetzungsschlüssel -> Übersetzter Text
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    
    // Liste aller verfügbaren Language Provider
    private readonly List<ILanguageProvider> _languageProviders;
    
    // Aktuelle Sprache (Standard: Deutsch)
    private string _currentLanguage = "de";

    /// <summary>
    /// Initialisiert den LocalizationService mit allen verfügbaren Language Providers
    /// Lädt deutsche und englische Sprachressourcen über separate Provider
    /// </summary>
    public LocalizationService()
    {
        // Initialisierung aller Language Provider
        _languageProviders = new List<ILanguageProvider>
        {
            new GermanLanguageProvider(),
            new EnglishLanguageProvider()
        };
        
        // Laden aller Übersetzungen von den Providern
        _translations = new Dictionary<string, Dictionary<string, string>>();
        
        foreach (var provider in _languageProviders)
        {
            _translations[provider.LanguageCode] = provider.GetTranslations();
        }
    }

    // Ereignis für Property-Änderungen (INotifyPropertyChanged-Implementierung)
    public event PropertyChangedEventHandler PropertyChanged;

    // Methode zum Auslösen des PropertyChanged-Ereignisses
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gibt alle verfügbaren Sprachen zurück
    /// </summary>
    /// <returns>Dictionary mit Sprachcodes als Key und Anzeigenamen als Value</returns>
    public Dictionary<string, string> GetAvailableLanguages()
    {
        return _languageProviders.ToDictionary(p => p.LanguageCode, p => p.DisplayName);
    }

    /// <summary>
    /// Ändert die aktuelle Sprache und löst die Aktualisierung der UI-Elemente aus
    /// </summary>
    /// <param name="newLanguage">Neuer Sprachcode (z.B. 'de' für Deutsch)</param>
    public void ChangeLanguage(string newLanguage)
    {
        if (_currentLanguage != newLanguage && _translations.ContainsKey(newLanguage))
        {
            _currentLanguage = newLanguage;
            OnPropertyChanged(nameof(CurrentLanguage));
            
            // Weitere UI-Aktualisierungen können hier ausgelöst werden
        }
    }

    /// <summary>
    /// Alias für ChangeLanguage - für Kompatibilität mit bestehendem Code
    /// </summary>
    /// <param name="newLanguage">Neuer Sprachcode (z.B. 'de' für Deutsch)</param>
    public void SetLanguage(string newLanguage)
    {
        ChangeLanguage(newLanguage);
    }

    /// <summary>
    /// Aktuelle Sprache
    /// </summary>
    public string CurrentLanguage => _currentLanguage;

    /// <summary>
    /// Gibt die aktuellen Übersetzungen zurück - für Kompatibilität mit bestehendem Code
    /// </summary>
    public Dictionary<string, string> CurrentTranslations => _translations[_currentLanguage];

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
    /// Öffentliche Methode um die aktuelle Assembly-Version abzurufen
    /// </summary>
    /// <returns>Aktuelle Versionsnummer als String</returns>
    public string GetApplicationVersion()
    {
        return GetCurrentAssemblyVersion();
    }

    /// <summary>
    /// Gibt den übersetzten Text für den angegebenen Schlüssel und die aktuelle Sprache zurück
    /// Fallback auf den Schlüssel selbst, wenn keine Übersetzung gefunden wird
    /// </summary>
    /// <param name="key">Übersetzungsschlüssel</param>
    /// <returns>Übersetzter Text</returns>
    public string GetTranslation(string key)
    {
        // Spezialbehandlung für AboutText - generiere dynamisch mit aktueller Version
        if (key == "AboutText")
        {
            var currentProvider = _languageProviders.FirstOrDefault(p => p.LanguageCode == _currentLanguage);
            if (currentProvider != null)
            {
                // Lade Übersetzungen neu, um dynamisch generierte Inhalte zu aktualisieren
                var freshTranslations = currentProvider.GetTranslations();
                if (freshTranslations.TryGetValue("AboutText", out var aboutText))
                {
                    return aboutText;
                }
            }
        }

        // Überprüfen, ob der Schlüssel in der aktuellen Sprache vorhanden ist
        if (_translations.TryGetValue(_currentLanguage, out var translations)
            && translations.TryGetValue(key, out var translation))
        {
            return translation;
        }

        // Fallback: Schlüssel selbst zurückgeben
        return key;
    }

    /// <summary>
    /// Alias für GetTranslation - für Kompatibilität mit bestehendem Code
    /// </summary>
    /// <param name="key">Übersetzungsschlüssel</param>
    /// <returns>Übersetzter Text</returns>
    public string GetString(string key)
    {
        return GetTranslation(key);
    }

    /// <summary>
    /// Gibt formatierten übersetzten Text zurück mit Platzhaltern
    /// </summary>
    /// <param name="key">Übersetzungsschlüssel</param>
    /// <param name="args">Parameter für string.Format</param>
    /// <returns>Formatierter übersetzter Text</returns>
    public string GetString(string key, params object[] args)
    {
        var template = GetTranslation(key);
        try
        {
            return string.Format(template, args);
        }
        catch
        {
            // Fallback bei Format-Fehlern
            return template;
        }
    }

    /// <summary>
    /// Aktualisiert die Übersetzungen von einem bestimmten Language Provider
    /// Nützlich für dynamische Inhalte wie AboutText
    /// </summary>
    /// <param name="languageCode">Sprachcode des zu aktualisierenden Providers</param>
    public void RefreshTranslations(string languageCode)
    {
        var provider = _languageProviders.FirstOrDefault(p => p.LanguageCode == languageCode);
        if (provider != null)
        {
            _translations[languageCode] = provider.GetTranslations();
            
            // UI benachrichtigen wenn es die aktuelle Sprache betrifft
            if (languageCode == _currentLanguage)
            {
                OnPropertyChanged(nameof(CurrentTranslations));
            }
        }
    }

    /// <summary>
    /// Aktualisiert alle Übersetzungen von allen Language Providers
    /// </summary>
    public void RefreshAllTranslations()
    {
        foreach (var provider in _languageProviders)
        {
            _translations[provider.LanguageCode] = provider.GetTranslations();
        }
        
        // UI benachrichtigen
        OnPropertyChanged(nameof(CurrentTranslations));
    }
}