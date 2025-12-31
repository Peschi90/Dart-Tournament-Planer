using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für das StartupSplashWindow
/// Enthält alle Texte die beim Anwendungsstart angezeigt werden
/// </summary>
public class GermanStartupLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // StartupSplashWindow Übersetzungen
            ["AppTitle"] = "Dart Tournament Planner",
            ["AppSubtitle"] = "Moderne Turnierverwaltung",
            ["LoadingText"] = "Wird geladen...",
            ["StartingApplication"] = "Starte Anwendung...",
            
            // Status-Texte für verschiedene Startup-Phasen
            ["InitializingServices"] = "Initialisiere Services...",
            ["InitializingAuthentication"] = "Initialisiere Anmeldung...",
            ["ServicesReady"] = "Services bereit",
            ["CheckingForUpdates"] = "Suche nach Updates...",
            ["ConnectingToGitHub"] = "Verbinde mit GitHub...",
            ["AnalyzingReleases"] = "Analysiere Releases...",
            ["UpdateAvailable"] = "Update verfügbar",
            ["NoUpdateAvailable"] = "Keine Updates verfügbar",
            ["UpdateCheckComplete"] = "Update-Überprüfung abgeschlossen",
            ["UpdateCheckFailed"] = "Überprüfung fehlgeschlagen",
            ["PreparingInterface"] = "Bereite Benutzeroberfläche vor...",
            ["Ready"] = "Bereit",
            
            // Update-Dialog Texte
            ["UpdateDownloadStarted"] = "Der Download wurde gestartet. Möchten Sie die Anwendung jetzt beenden um das Update zu installieren?",
            
            // Version Text (Präfix für dynamische Versionsnummer)
            ["VersionPrefix"] = "Version",
            
            // Copyright Text (wird teilweise dynamisch generiert)
            ["CopyrightText"] = "© 2025 by I3uLL3t"
        };
    }
}