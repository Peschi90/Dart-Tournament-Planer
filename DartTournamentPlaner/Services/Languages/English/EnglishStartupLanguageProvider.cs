using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for the StartupSplashWindow
/// Contains all texts displayed during application startup
/// </summary>
public class EnglishStartupLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // StartupSplashWindow translations
            ["AppTitle"] = "Dart Tournament Planner",
            ["AppSubtitle"] = "Modern Tournament Management",
            ["LoadingText"] = "Loading...",
            ["StartingApplication"] = "Starting application...",
            
            // Status texts for different startup phases
            ["InitializingServices"] = "Initializing services...",
            ["ServicesReady"] = "Services ready",
            ["CheckingForUpdates"] = "Checking for updates...",
            ["ConnectingToGitHub"] = "Connecting to GitHub...",
            ["AnalyzingReleases"] = "Analyzing releases...",
            ["UpdateAvailable"] = "Update available",
            ["NoUpdateAvailable"] = "No updates available",
            ["UpdateCheckComplete"] = "Update check completed",
            ["UpdateCheckFailed"] = "Check failed",
            ["PreparingInterface"] = "Preparing user interface...",
            ["Ready"] = "Ready",
            
            // Update dialog texts
            ["UpdateDownloadStarted"] = "The download has been started. Would you like to exit the application now to install the update?",
            
            // Version text (prefix for dynamic version number)
            ["VersionPrefix"] = "Version",
            
            // Copyright text (partially generated dynamically)
            ["CopyrightText"] = "© 2025 by I3uLL3t"
        };
    }
}