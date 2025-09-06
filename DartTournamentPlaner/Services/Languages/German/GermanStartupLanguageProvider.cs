using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche �bersetzungen f�r das StartupSplashWindow
/// Enth�lt alle Texte die beim Anwendungsstart angezeigt werden
/// </summary>
public class GermanStartupLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // StartupSplashWindow �bersetzungen
            ["AppTitle"] = "Dart Tournament Planner",
            ["AppSubtitle"] = "Moderne Turnierverwaltung",
            ["LoadingText"] = "Wird geladen...",
            ["StartingApplication"] = "Starte Anwendung...",
            
            // Status-Texte f�r verschiedene Startup-Phasen
            ["InitializingServices"] = "Initialisiere Services...",
            ["ServicesReady"] = "Services bereit",
            ["CheckingForUpdates"] = "Suche nach Updates...",
            ["ConnectingToGitHub"] = "Verbinde mit GitHub...",
            ["AnalyzingReleases"] = "Analysiere Releases...",
            ["UpdateAvailable"] = "Update verf�gbar",
            ["NoUpdateAvailable"] = "Keine Updates verf�gbar",
            ["UpdateCheckComplete"] = "Update-�berpr�fung abgeschlossen",
            ["UpdateCheckFailed"] = "�berpr�fung fehlgeschlagen",
            ["PreparingInterface"] = "Bereite Benutzeroberfl�che vor...",
            ["Ready"] = "Bereit",
            
            // Update-Dialog Texte
            ["UpdateDownloadStarted"] = "Der Download wurde gestartet. M�chten Sie die Anwendung jetzt beenden um das Update zu installieren?",
            
            // Version Text (Pr�fix f�r dynamische Versionsnummer)
            ["VersionPrefix"] = "Version",
            
            // Copyright Text (wird teilweise dynamisch generiert)
            ["CopyrightText"] = "� 2025 by I3uLL3t"
        };
    }
}