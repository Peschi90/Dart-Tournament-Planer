using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages; // Assuming the interface is in this namespace

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für Hilfe-System und API-Funktionen
/// </summary>
public class GermanHelpAndApiLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Hilfesystem
            ["HelpTitle"] = "Hilfe - Dart Turnier Planer",
            ["HelpGeneral"] = "🏠 Allgemeine Bedienung",
            ["HelpTournamentSetup"] = "⚙️ Turnier-Setup",
            ["HelpGroupManagement"] = "👥 Gruppenverwaltung",
            ["HelpGameRules"] = "🎯 Spielregeln",
            ["HelpMatches"] = "🏆 Spiele & Ergebnisse",
            ["HelpTournamentPhases"] = "📊 Turnierphasen",
            ["HelpMenus"] = "📋 Menüs & Funktionen",
            ["HelpPowerScoring"] = "⚡ PowerScoring",
            ["HelpLicenseSystem"] = "🔑 Lizenz-System",
            ["HelpApiIntegration"] = "🌐 API-Integration",
            ["HelpTournamentHub"] = "🎯 Tournament Hub",
            ["HelpStatistics"] = "📈 Statistiken",
            ["HelpPrinting"] = "🖨️ Drucken",
            ["HelpTips"] = "💡 Tipps & Tricks",

            // API-Funktionen
            ["API"] = "API",
            ["StartAPI"] = "API starten",
            ["StopAPI"] = "API stoppen",
            ["APIDocumentation"] = "API Dokumentation",
            ["APIStatus"] = "API Status",
            ["APIRunning"] = "API läuft",
            ["APIStopped"] = "API gestoppt",
            ["APIError"] = "API Fehler",
            ["APIStarted"] = "API wurde erfolgreich gestartet!\n\nURL: {0}\nDokumentation: {0}",
            ["APIStartError"] = "API konnte nicht gestartet werden. Überprüfen Sie, ob Port 5000 verfügbar ist.",
            ["APIStopError"] = "Fehler beim Stoppen der API.",
            ["APINotRunning"] = "API nicht gestartet",
            ["APINotRunningMessage"] = "Die API ist nicht gestartet. Bitte starten Sie die API zuerst über das Menü.",
            ["ApiLicenseRequired"] = "API-Lizenz erforderlich",
            ["ApiLicenseRequiredMessage"] = "Die API-Funktionalität erfordert eine gültige Lizenz mit dem 'API Connection' Feature.",
            ["ApiNotRunningMessage"] = "Die API ist nicht gestartet. Bitte starten Sie die API zuerst über das Menü.",
            ["ApiNotRunning"] = "API nicht gestartet",
            ["ApiDocumentationInfo"] = "API Dokumentation:\n\nDie API-Dokumentation ist verfügbar wenn die API läuft.\nStandard URL: http://localhost:5000\n\nUm die API zu starten, verwenden Sie das API-Menü."
        };
    }
}