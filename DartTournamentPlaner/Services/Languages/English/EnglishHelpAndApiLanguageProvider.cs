using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for help system and API functions
/// </summary>
public class EnglishHelpAndApiLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Help System
            ["HelpTitle"] = "Help - Dart Tournament Planner",
            ["HelpGeneral"] = "🏠 General Usage",
            ["HelpTournamentSetup"] = "⚙️ Tournament Setup",
            ["HelpGroupManagement"] = "👥 Group Management",
            ["HelpGameRules"] = "🎯 Game Rules",
            ["HelpMatches"] = "🏆 Matches & Results",
            ["HelpTournamentPhases"] = "📊 Tournament Phases",
            ["HelpMenus"] = "📋 Menus & Features",
            ["HelpLicenseSystem"] = "🔑 License System",
            ["HelpApiIntegration"] = "🌐 API Integration",
            ["HelpTournamentHub"] = "🎯 Tournament Hub",
            ["HelpStatistics"] = "📈 Statistics",
            ["HelpPrinting"] = "🖨️ Printing",
            ["HelpTips"] = "💡 Tips & Tricks",

            // API Functions
            ["API"] = "API",
            ["StartAPI"] = "Start API",
            ["StopAPI"] = "Stop API",
            ["APIDocumentation"] = "API Documentation",
            ["APIStatus"] = "API Status",
            ["APIRunning"] = "API Running",
            ["APIStopped"] = "API Stopped",
            ["APIError"] = "API Error",
            ["APIStarted"] = "API successfully started!\n\nURL: {0}\nDocumentation: {0}",
            ["APIStartError"] = "API could not be started. Check if port 5000 is available.",
            ["APIStopError"] = "Error stopping API.",
            ["APINotRunning"] = "API not running",
            ["APINotRunningMessage"] = "API is not running. Please start the API first via the menu.",
            ["ApiLicenseRequired"] = "API license required",
            ["ApiLicenseRequiredMessage"] = "API functionality requires a valid license with the 'API Connection' feature.",
            ["ApiNotRunningMessage"] = "API is not running. Please start the API first via the menu.",
            ["ApiNotRunning"] = "API not running",
            ["ApiDocumentationInfo"] = "API Documentation:\n\nAPI documentation is available when the API is running.\nDefault URL: http://localhost:5000\n\nTo start the API, use the API menu."
        };
    }
}