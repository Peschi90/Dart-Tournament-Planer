using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für das Lizenz-System
/// </summary>
public class GermanLicenseLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Lizenz-Menü
            ["License"] = "Lizenz",
            ["LicenseStatus"] = "Lizenz-Status",
            ["LicenseStatusSubtitle"] = "Übersicht über Ihren aktuellen Lizenz-Status und verfügbare Features",
            ["ActivateLicense"] = "Lizenz aktivieren",
            ["LicenseInfo"] = "Lizenz-Informationen",
            ["LicenseInfoSubtitle"] = "Aktuelle Lizenz- und Feature-Informationen",
            ["PurchaseLicense"] = "Hol dir deine Lizenz!",
            ["ShowDetails"] = "Details anzeigen",
            ["ActivatingLicense"] = "Lizenz wird aktiviert...",
            ["ValidatingLicense"] = "Lizenz wird validiert...",
            ["LicenseActivationMessage"] = "Geben Sie Ihren Lizenzschlüssel ein:",
            ["LicenseActivatedSuccessfully"] = "Lizenz erfolgreich aktiviert!",
            ["PurchaseLicenseMessage"] = "Besuchen Sie unsere Website, um eine Lizenz zu erwerben:",
            ["RemoveLicense"] = "Lizenz entfernen",
            ["RemoveLicenseConfirmation"] = "Möchten Sie die aktivierte Lizenz wirklich entfernen?",
            ["LicenseRemovedSuccess"] = "Lizenz wurde erfolgreich entfernt!",
            ["LicenseRemoveError"] = "Fehler beim Entfernen der Lizenz.",
            ["Success"] = "Erfolg",
            ["Refresh"] = "Aktualisieren",

            // Erweiterte Lizenz-Status Übersetzungen
            ["LicenseValid"] = "Lizenz gültig",
            ["LicenseInvalid"] = "Lizenz ungültig",
            ["LicenseExpired"] = "Lizenz abgelaufen",
            ["LicenseActivationRequired"] = "Lizenz-Aktivierung erforderlich",
            ["LicensedMode"] = "Lizenzierter Modus",
            ["UnlicensedMode"] = "Unlizenzierter Modus",
            ["OfflineMode"] = "Offline-Modus",
            ["OnlineMode"] = "Online-Modus",

            // Übersetzungen für Purchase License Dialog
            ["PurchaseLicenseTitle"] = "Hol dir deine Lizenz!",
            ["PurchaseLicenseSubtitle"] = "Füllen Sie das Formular aus, um eine Lizenz anzufordern. \nZu beginn des Projekts sind die Lizenzen kostenfrei zu bekommen!",
            ["PurchasePersonalInformation"] = "Persönliche Informationen",
            ["PurchaseLicenseRequirements"] = "Lizenz Vorraussetungen",
            ["PurchaseAdditionalInformation"] = "Zusätzliche Informationen",
            ["PurchaseSystemInformation"] = "System Informationen",
            ["PurchaseHardwareInfoII"] = "Diese Hardware - ID wird für die Lizenzaktivierung verwendet und ist für die Bearbeitung Ihrer Anfrage erforderlich.",
            ["PurchaseHardwareInfoI"] = "Ihre Hardware - ID(automatisch in der Anfrage enthalten):",
            ["FirstName"] = "Vorname",
            ["LastName"] = "Nachname",
            ["Email"] = "E-Mail-Adresse",
            ["Company"] = "Unternehmen / Organisation",
            ["LicenseType"] = "Lizenztyp",
            ["RequiredActivations"] = "Benötigte Aktivierungen",
            ["RequiredFeatures"] = "Benötigte Features",
            ["AdditionalMessage"] = "Nachricht / Besondere Anforderungen",
            ["HowDidYouHear"] = "Wie haben Sie von uns erfahren?",
            ["SendLicenseRequest"] = "Lizenzanfrage senden",
            ["ValidationError"] = "Validierungsfehler",
            ["EmailClientOpened"] = "E-Mail-Client wurde geöffnet",

            // Spenden-Dialog
            ["SupportProject"] = "Projekt unterstützen",
            ["SupportProjectTitle"] = "Unser Projekt unterstützen",
            ["SupportProjectSubtitle"] = "Ihre Spende hilft uns, Dart Tournament Planner weiterzuentwickeln!",
            ["SelectDonationAmount"] = "Wählen Sie Ihren Spendenbetrag:",
            ["Skip"] = "Überspringen",
            ["DonateViaPayPal"] = "Via PayPal spenden",
            ["ThankYou"] = "Vielen Dank!",
            ["PayPalOpened"] = "PayPal geöffnet für Spende",

            // DONATION DIALOG - NEU
            ["SupportDevelopmentTitle"] = "Entwicklung unterstützen",
            ["DonationDialogMessage"] = "Gefällt Ihnen dieser Dart Turnier Planer?\n\nUnterstützen Sie die Weiterentwicklung mit einer kleinen Spende.\nJeder Beitrag hilft dabei, die Software zu verbessern und zu pflegen.",
            ["WithYourSupportYouEnable"] = "Mit Ihrer Unterstützung ermöglichen Sie:",
            ["RegularUpdatesAndNewFeatures"] = "• Regelmäßige Updates und neue Funktionen",
            ["FastBugFixes"] = "• Schnelle Fehlerbehebung",
            ["ContinuousSupport"] = "• Kontinuierliche Unterstützung",
            ["FreeUseForEveryone"] = "• Kostenlose Nutzung für alle",
            ["OpenDonationPageButton"] = "Spendenseite öffnen",
            ["LaterButton"] = "Später",

            // TOURNAMENT OVERVIEW LICENSE DIALOG - NEU
            ["TournamentOverviewLicenseRequiredTitle"] = "Turnierübersicht-Lizenz erforderlich",
            ["TournamentOverviewLicenseRequiredSubtitle"] = "Premium-Feature für professionelle Turnierverwaltung",
            ["TournamentOverviewLicenseMessage"] = "Die erweiterte Turnierübersicht ist ein Premium-Feature, das eine gültige Lizenz erfordert.",
            ["TournamentOverviewBenefitsTitle"] = "Die Turnierübersicht bietet:",
            ["TournamentOverviewBenefits"] = "• Live-Anzeige aller Turnierklassen\n• Automatischer Wechsel zwischen Phasen\n• Professionelle Präsentation für Zuschauer\n• Echtzeit-Updates und Synchronisation\n• Anpassbare Anzeigeoptionen",
            ["TournamentOverviewActionText"] = "Möchten Sie eine Lizenz mit Turnierübersicht-Features anfordern?",
            ["RequestTournamentOverviewLicense"] = "Hol dir deine Lizenz!",

            // DRUCK LIZENZ
            ["PrintLicenseRequired"] = "Drucklizenz erforderlich",
            ["PrintLicenseRequiredTitle"] = "Drucklizenz erforderlich",
            ["PrintLicenseRequiredMessage"] = "Die Druckfunktionalität erfordert eine gültige Lizenz mit dem 'Enhanced Printing' Feature.",
            ["EnhancedPrintingBenefitsTitle"] = "Enhanced Printing beinhaltet:",
            ["EnhancedPrintingBenefits"] = "- Professionelle Turnierberichte\n- Spielergebnisse und Turnierbäume\n- Export zu PDF Funktionalität",
            ["PrintLicenseActionText"] = "Möchten Sie eine Lizenz mit dem Enhanced Printing Feature anfordern?",
            ["RequestLicense"] = "Lizenz anfordern",
            ["RequestPrintLicense"] = "Hol dir deine Lizenz!",

            // Statistik Lizenz Meldungen
            ["StatisticsLicenseRequiredTitle"] = "Statistik-Lizenz erforderlich",
            ["StatisticsLicenseRequiredMessage"] = "Erweiterte Statistiken erfordern eine gültige Lizenz mit dem 'Statistics' Feature und eine aktive Hub-Verbindung.",
            ["StatisticsBenefitsTitle"] = "Statistik-Features beinhalten:",
            ["StatisticsBenefits"] = "- Detaillierte Spieler-Performance-Analyse\n- Spielverlauf und Trends\n- Turnier-Fortschritt-Verfolgung\n- Echtzeit-Hub-Synchronisation\n- Erweiterte statistische Berichte",
            ["HubConnectionRequired"] = "Hub-Verbindung erforderlich",
            ["HubConnectionRequiredText"] = "Statistik-Features erfordern eine aktive Tournament Hub-Verbindung für Echtzeit-Datensynchronisation.",
            ["StatisticsLicenseActionText"] = "Möchten Sie eine Lizenz mit Statistik- und Hub-Features anfordern?",
            ["RequestStatisticsLicense"] = "Hol dir deine Lizenz!",

            // API Lizenz Texte
            ["ApiLicenseRequired"] = "API-Lizenz erforderlich",
            ["ApiLicenseRequiredTitle"] = "API-Lizenz erforderlich",
            ["ApiLicenseRequiredMessage"] = "Die API-Funktionalität erfordert eine gültige Lizenz mit dem 'API Connection' Feature.",
            ["ApiConnectionBenefitsTitle"] = "API Connection Features beinhalten:",
            ["ApiConnectionBenefits"] = "- REST API für Turnierdaten\n- Webhooks für Echtzeit-Updates\n- Externe Integrationen\n- Automatisierte Datenverarbeitung",
            ["ApiLicenseActionText"] = "Möchten Sie eine Lizenz mit API Connection Features anfordern?",
            ["RequestApiLicense"] = "Hol dir deine Lizenz!",

            // Lizenz-Aktivierung Dialog
            ["LicenseKeyFormatInfo"] = "Lizenzschlüssel bestehen aus 8 Blöcken mit je 4 Zeichen (A-F, 0-9), getrennt durch Bindestriche.",
            ["InvalidLicenseKeyFormat"] = "Ungültiges Lizenzschlüssel-Format. Bitte überprüfen Sie die Eingabe.",
            ["Continue"] = "Weiter",
            ["Unlimited"] = "Unbegrenzt",

            // Lizenz-Erfolg Dialog
            ["LicenseActivationSuccessSubtitle"] = "Alle Premium-Features sind jetzt verfügbar",
            ["NoSpecificFeatures"] = "Alle Standard-Features verfügbar",
            ["GenericLicenseActivated"] = "Ihre Lizenz wurde erfolgreich aktiviert.",
            ["LastActivationWarning"] = "Dies war Ihre letzte verfügbare Aktivierung für diese Lizenz. Kontaktieren Sie den Support, falls Sie die Software auf zusätzlichen Computern installieren müssen.",
            ["FewActivationsWarning"] = "Sie haben noch {0} Aktivierung(en) für diese Lizenz übrig. Planen Sie weitere Installationen sorgfältig."
        };
    }
}