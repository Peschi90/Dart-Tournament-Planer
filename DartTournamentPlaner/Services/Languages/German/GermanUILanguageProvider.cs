using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages; // Assuming the ILanguageSection is in this namespace

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für UI-Elemente und allgemeine Benutzeroberfläche
/// </summary>
public class GermanUILanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // =====================================
            // APPLICATION CORE TRANSLATIONS
            // =====================================

            // Application settings
            ["Settings"] = "Einstellungen",
            ["Language"] = "Sprache",
            ["AutoSave"] = "Automatisches Speichern",
            ["AutoSaveInterval"] = "Speicherintervall (Minuten)",
            ["Save"] = "Speichern",
            ["Cancel"] = "Abbrechen",

            // Menu entries
            ["File"] = "Datei",
            ["New"] = "Neu",
            ["Open"] = "Öffnen",
            ["SaveAs"] = "Speichern unter",
            ["Print"] = "Drucken",
            ["Exit"] = "Beenden",
            ["View"] = "Ansicht",
            ["Help"] = "Hilfe",
            ["About"] = "Über",

            // PowerScoring
            ["PowerScoring"] = "PowerScoring",
            ["PowerScoringRequiresLicense"] = "🎯 PowerScoring ist eine Premium-Funktion\n\n" +
                "Mit PowerScoring kannst du:\n" +
                "• Spielerwertungen systematisch erfassen\n" +
                "• Automatische Ranglistenerstellung\n" +
                "• Optimale Gruppeneinteilung basierend auf Spielstärke\n\n" +
                "Aktiviere eine Lizenz mit dem Feature 'powerscore', um diese Funktion zu nutzen.",
            ["FeatureNotAvailable"] = "Funktion nicht verfügbar",

            // PowerScoring License Dialog
            ["PowerScoringLicenseRequired_Title"] = "PowerScoring-Lizenz erforderlich",
            ["PowerScoringLicenseRequired_Message"] = "PowerScoring ist eine Premium-Funktion, die dir hilft, Spieler nach Spielstärke zu organisieren.",
            ["PowerScoringLicenseRequired_BenefitsTitle"] = "PowerScoring beinhaltet:",
            ["PowerScoringLicenseRequired_Benefit1"] = "- Systematische Spielerwertungserfassung",
            ["PowerScoringLicenseRequired_Benefit2"] = "- Automatische Ranglistenerstellung",
            ["PowerScoringLicenseRequired_Benefit3"] = "- Optimale Gruppeneinteilung basierend auf Spielstärke",
            ["PowerScoringLicenseRequired_Benefit4"] = "- Flexible Wertungsregeln (1x3, 8x3, 10x3, 15x3 Würfe)",
            ["PowerScoringLicenseRequired_Benefit5"] = "- Snake-Draft Gruppenzuweisung",
            ["PowerScoringLicenseRequired_ActionText"] = "Möchtest du eine Lizenz mit dem PowerScoring-Feature anfordern?",
            ["RequestLicense"] = "Lizenz anfordern",

            // Status displays
            ["HasUnsavedChanges"] = "Geändert",
            ["NotSaved"] = "Nicht gespeichert",
            ["Saved"] = "Gespeichert",

            // General UI elements
            ["Close"] = "Schließen",
            ["OK"] = "OK",
            ["Start"] = "Start",
            ["Stop"] = "Stopp",
            ["Player"] = "Spieler",
            ["Match"] = "Spiel",
            ["Result"] = "Ergebnis",
            ["Position"] = "Position",
            ["Winner"] = "Sieger",
            ["Information"] = "Information",
            ["Warning"] = "Warnung",
            ["Error"] = "Fehler",

            // Input dialog
            ["InputDialog"] = "Eingabe",
            ["EnterName"] = "Name eingeben:",

            // Loading Spinner
            ["Loading"] = "Lade...",
            ["CheckingGroupStatus"] = "Prüfe Gruppenstatus...",
            ["ProcessingMatches"] = "Verarbeite Spiele...",
            ["CheckingCompletion"] = "Prüfe Abschluss...",

            // Startup and update functions
            ["StartingApplication"] = "Starte Anwendung...",
            ["AppSubtitle"] = "Moderne Turnierverwaltung",
            ["CheckingForUpdates"] = "Suche nach Updates...",
            ["ConnectingToGitHub"] = "Verbinde mit GitHub...",
            ["AnalyzingReleases"] = "Analysiere Releases...",
            ["UpdateAvailable"] = "Update verfügbar",
            ["WhatsNew"] = "Neues in dieser Version:",
            ["RemindLater"] = "Später erinnern",
            ["SkipVersion"] = "Version überspringen",
            ["DownloadUpdate"] = "Jetzt herunterladen",
            ["DownloadAndInstall"] = "Herunterladen & Installieren",
            ["DownloadingUpdate"] = "Update wird heruntergeladen",
            ["PreparingDownload"] = "Bereite Download vor...",
            ["DownloadingSetup"] = "Setupdatei wird heruntergeladen...",
            ["DownloadCompleted"] = "Download abgeschlossen, prüfe Datei...",
            ["PreparingInstallation"] = "Bereite Installation vor...",
            ["StartingInstallation"] = "Starte Installation...",
            ["InstallationStarted"] = "Installation gestartet",
            ["InstallationCancelled"] = "Installation abgebrochen",
            ["ErrorStartingSetup"] = "Fehler beim Starten des Setups",
            ["AdminRightsRequired"] = "Administratorrechte erforderlich",
            ["NoUpdateAvailable"] = "Keine Updates verfügbar",

            // Overview configuration dialog
            ["OverviewConfiguration"] = "Übersichts-Konfiguration",
            ["TournamentOverviewConfiguration"] = "Turnierübersicht-Konfiguration",
            ["TimeBetweenClasses"] = "Zeit zwischen Turnierklassen:",
            ["TimeBetweenSubTabs"] = "Zeit zwischen Untertabs:",
            ["Seconds"] = "Sekunden",
            ["ShowOnlyActiveClassesText"] = "Nur Klassen mit aktiven Gruppen anzeigen",
            ["OverviewInfoText"] = "Live-Turnieranzeige für alle Klassen mit automatischem Wechsel",
            ["InvalidClassInterval"] = "Ungültiges Klassenintervall. Bitte Zahl ≥ 1 eingeben.",
            ["InvalidSubTabInterval"] = "Ungültiges Untertabintervall. Bitte Zahl ≥ 1 eingeben.",

            // Tournament overview texts
            ["TournamentName"] = "⚽ Turnier:",
            ["CurrentPhase"] = "🏁 Aktuelle Phase:",
            ["GroupsCount"] = "👥 Gruppen:",
            ["PlayersTotal"] = "👤 Gesamtspieler:",
            ["GameRulesColon"] = "📋 Spielregeln:",
            ["CompletedGroups"] = "✅ Abgeschlossene Gruppen:",
            ["QualifiedPlayers"] = "🎯 Qualifizierte Spieler:",
            ["KnockoutMatches"] = "🏆 K.O.-Spiele:",
            ["Completed"] = "abgeschlossen",

            // Other hardcoded texts
            ["Finalists"] = "Finalisten",
            ["KnockoutParticipants"] = "K.O.-Teilnehmer",
            ["PlayersText"] = "Spieler",
            ["OverviewModeTitle"] = "Turnierübersichtsmodus",
            ["NewTournament"] = "Neues Turnier",
            ["CreateNewTournament"] = "Neues Turnier erstellen? Ungespeicherte Änderungen gehen verloren.",
            ["UnsavedChanges"] = "Ungespeicherte Änderungen",
            ["SaveBeforeExit"] = "Es gibt ungespeicherte Änderungen. Vor dem Beenden speichern?",
            ["CustomFileNotImplemented"] = "Benutzerdefinierte Datei laden noch nicht implementiert.",
            ["CustomFileSaveNotImplemented"] = "Benutzerdefinierte Datei speichern noch nicht implementiert.",
            ["ErrorOpeningHelp"] = "Fehler beim Öffnen der Hilfe:",
            ["ErrorOpeningOverview"] = "Fehler beim Öffnen der Turnierübersicht:",
            ["ErrorSavingData"] = "Fehler beim Speichern der Daten:",

            // Donation and Bug Report Functions
            ["Donate"] = "💝",
            ["DonateTooltip"] = "Unterstütze die Entwicklung dieses Projekts",
            ["BugReport"] = "🐛 Fehler melden",
            ["BugReportTooltip"] = "Melde Fehler oder schlage Verbesserungen vor",
            ["BugReportTitle"] = "Fehlermeldung",
            ["BugReportDescription"] = "Beschreibe das Problem oder deine Idee zur Verbesserung:",
            ["BugReportEmailSubject"] = "Dart Turnierplaner - Fehlermeldung",
            ["BugReportSteps"] = "Schritte zur Reproduktion:",
            ["BugReportExpected"] = "Erwartetes Verhalten:",
            ["BugReportActual"] = "Tatsächliches Verhalten:",
            ["BugReportSystemInfo"] = "Systeminformationen:",
            ["BugReportVersion"] = "Version:",
            ["BugReportOS"] = "Betriebssystem:",
            ["BugReportSubmitEmail"] = "Per E-Mail senden",
            ["BugReportSubmitGitHub"] = "Auf GitHub öffnen",
            ["ThankYouSupport"] = "Vielen Dank für deine Unterstützung!",
            ["BugReportSent"] = "Fehlermeldung erfolgreich gesendet. Danke!",
            ["ErrorSendingBugReport"] = "Fehler beim Senden der Fehlermeldung:",
            ["SupportDevelopment"] = "Entwicklung unterstützen",
            ["DonationMessage"] = "Gefällt dir der Dart Turnierplaner?\n\nUnterstütze die weitere Entwicklung mit einer kleinen Spende.\nJeder Beitrag hilft, die Software zu verbessern und zu pflegen.",
            ["OpenDonationPage"] = "Spendenseite öffnen",

            // Main window
            ["AppTitle"] = "Dart Turnierplaner",
            ["Platinum"] = "Platin",
            ["Gold"] = "Gold",
            ["Silver"] = "Silber",
            ["Bronze"] = "Bronze",

            // =====================================
            // ABOUT DIALOG
            // =====================================

            ["About"] = "Über",
            ["AboutDescription"] = "Eine umfassende und moderne Anwendung zur Organisation und Verwaltung von Dartturnieren. " +
                "Unterstützt mehrere Turnierformate einschließlich Gruppenphase, Finals und K.O.-Phase. " +
                "Bietet Echtzeit-Spielverfolgung, detaillierte Spielerstatistiken, Turnierhub-Integration via WebSocket, " +
                "QR-Code-Generierung für mobilen Zugriff und professionelle Turnierdokumentation mit Druckfunktion.",
            ["AboutCredits"] = "Besonderen Dank an alle Mitwirkenden, Tester und die Dart-Community für wertvolles Feedback und Unterstützung. " +
                "Dieses Projekt wird kontinuierlich durch Community-Engagement weiterentwickelt.",
            ["Developer"] = "Entwickler",
            ["DeveloperName"] = "Marcel Peschka",
            ["Framework"] = "Framework",
            ["License"] = "Lizenz",
            ["LicenseType"] = "Open Source (MIT-Lizenz)",
            ["Website"] = "Webseite",
            ["WebsiteUrl"] = "https://github.com/Peschi90/Dart-Turnament-Planer",
            ["OpenSource"] = "Open Source",
            ["SpecialThanks"] = "Besonderen Dank",
            ["AppInformation"] = "Anwendungsinformationen",
            ["TechnicalDetails"] = "Technische Details",
            ["Features"] = "Funktionen",
            ["FeatureList"] = "• Mehrere Turnierformate (Gruppenphase, Finals, K.O.)\n" +
                "• Echtzeit-Spielverfolgung und Score-Management\n" +
                "• Umfassende Spielerstatistiken\n" +
                "• Turnierhub-Integration via WebSocket\n" +
                "• QR-Code-Generierung für mobilen Zugriff\n" +
                "• Professionelle Druckfunktion\n" +
                "• Mehrsprachige Unterstützung (Deutsch/Englisch)\n" +
                "• Dark-/Light-Mode Unterstützung\n" +
                "• Automatisches Speichern und Datenpersistenz",
            ["ContactSupport"] = "Support & Kontakt",
            ["GitHubRepository"] = "GitHub-Repository",
            ["ReportIssue"] = "Problem melden",
            ["VersionInfo"] = "Versionsinformationen",

            // =====================================
            // THEME & DARK MODE
            // =====================================

            ["Theme"] = "Design",
            ["DarkMode"] = "Dunkelmodus",
            ["LightMode"] = "Hellmodus",
            ["SwitchToDarkMode"] = "In den Dunkelmodus wechseln",
            ["SwitchToLightMode"] = "In den Hellmodus wechseln",
            ["ThemeSettings"] = "Design-Einstellungen",

            // =====================================
            // GENERAL UI ELEMENTS
            // =====================================

            // Basic Buttons
            ["OK"] = "OK",
            ["Cancel"] = "Abbrechen",
            ["Save"] = "Speichern",
            ["Load"] = "Laden",
            ["Delete"] = "Löschen",
            ["Edit"] = "Bearbeiten",
            ["Add"] = "Hinzufügen",
            ["Remove"] = "Entfernen",
            ["Close"] = "Schließen",
            ["Apply"] = "Anwenden",
            ["Reset"] = "Zurücksetzen",
            ["Refresh"] = "Aktualisieren",
            ["Clear"] = "Leeren",
            ["Copy"] = "Kopieren",
            ["Paste"] = "Einfügen",
            ["Cut"] = "Ausschneiden",
            ["Undo"] = "Rückgängig",
            ["Redo"] = "Wiederholen",

            // Navigation
            ["Next"] = "Weiter",
            ["Previous"] = "Zurück",
            ["First"] = "Erste",
            ["Last"] = "Letzte",
            ["Back"] = "Zurück",
            ["Forward"] = "Vorwärts",
            ["Up"] = "Hoch",
            ["Down"] = "Runter",
            ["Left"] = "Links",
            ["Right"] = "Rechts",

            // Status Terms
            ["Success"] = "Erfolg",
            ["Error"] = "Fehler",
            ["Warning"] = "Warnung",
            ["Information"] = "Information",
            ["Loading"] = "Lädt...",
            ["Saving"] = "Speichere...",
            ["Processing"] = "Verarbeite...",
            ["Complete"] = "Abgeschlossen",
            ["Failed"] = "Fehlgeschlagen",
            ["Ready"] = "Bereit",
            ["Busy"] = "Beschäftigt",

            // =====================================
            // DIALOG & WINDOW ELEMENTS
            // =====================================

            // Window Title Suffixes
            ["Settings"] = "Einstellungen",
            ["Configuration"] = "Konfiguration",
            ["Properties"] = "Eigenschaften",
            ["Options"] = "Optionen",
            ["Preferences"] = "Voreinstellungen",

            // Dialog & Window
            ["Dialog"] = "Dialog",
            ["Window"] = "Fenster",
            ["Form"] = "Formular",
            ["Wizard"] = "Assistent",
            ["Setup"] = "Setup",

            // =====================================
            // DATA ELEMENTS
            // =====================================

            // Tables & Lists
            ["Name"] = "Name",
            ["Value"] = "Wert",
            ["Type"] = "Typ",
            ["Status"] = "Status",
            ["Date"] = "Datum",
            ["Time"] = "Zeit",
            ["Duration"] = "Dauer",
            ["Count"] = "Anzahl",
            ["Total"] = "Gesamt",
            ["Average"] = "Durchschnitt",
            ["Minimum"] = "Minimum",
            ["Maximum"] = "Maximum",

            // Sorting & Filtering
            ["Sort"] = "Sortieren",
            ["SortBy"] = "Sortieren nach",
            ["Filter"] = "Filtern",
            ["FilterBy"] = "Filtern nach",
            ["Search"] = "Suchen",
            ["SearchFor"] = "Suchen nach",
            ["Results"] = "Ergebnisse",
            ["NoResults"] = "Keine Ergebnisse",

            // =====================================
            // FORM ELEMENTS
            // =====================================

            // Input Labels
            ["Required"] = "Erforderlich",
            ["Optional"] = "Optional",
            ["Default"] = "Standard",
            ["Custom"] = "Benutzerdefiniert",
            ["Auto"] = "Automatisch",
            ["Manual"] = "Manuell",

            // Validation
            ["Valid"] = "Gültig",
            ["Invalid"] = "Ungültig",
            ["ValidationError"] = "Validierungsfehler",
            ["RequiredField"] = "Pflichtfeld",
            ["InvalidFormat"] = "Ungültiges Format",
            ["ValueTooSmall"] = "Wert zu klein",
            ["ValueTooLarge"] = "Wert zu groß",


            // =====================================
            // LANGUAGE-SPECIFIC UI
            // =====================================

            ["LanguageSettings"] = "Spracheinstellungen",
            ["SelectLanguage"] = "Sprache auswählen",
            ["LanguageChanged"] = "Sprache geändert",
            ["LanguageChangeRestart"] = "Die Anwendung muss neu gestartet werden, damit die Sprachänderung wirksam wird.",

            // =====================================
            // SYSTEM & DEBUG
            // =====================================

            ["System"] = "System",
            ["Debug"] = "Debug",
            ["Log"] = "Log",
            ["Version"] = "Version",
            ["Build"] = "Build",
            ["Platform"] = "Plattform",
            ["Memory"] = "Speicher",
            ["Performance"] = "Leistung",

            // =====================================
            // ACCESSIBILITY
            // =====================================

            ["AccessibilityMode"] = "Barrierefreiheit",
            ["HighContrast"] = "Hoher Kontrast",
            ["LargeText"] = "Großer Text",
            ["ScreenReader"] = "Bildschirmleser",
            ["KeyboardNavigation"] = "Tastaturnavigation",

            // =====================================
            // FILE & I/O
            // =====================================
            
            ["File"] = "Datei",
            ["Folder"] = "Ordner",
            ["Path"] = "Pfad",
            ["Size"] = "Größe",
            ["Modified"] = "Geändert",
            ["Created"] = "Erstellt",
            ["Exists"] = "Existiert",
            ["NotFound"] = "Nicht gefunden",
            ["ReadOnly"] = "Nur lesen",
            ["Permission"] = "Berechtigung",

            // =====================================
            // NETWORK & CONNECTION
            // =====================================

            ["Connected"] = "Verbunden",
            ["Disconnected"] = "Getrennt",
            ["Connecting"] = "Verbinden...",
            ["Connection"] = "Verbindung",
            ["Network"] = "Netzwerk",
            ["Offline"] = "Offline",
            ["Online"] = "Online",
            ["Timeout"] = "Zeitüberschreitung",
            ["Retry"] = "Erneut versuchen",

            // =====================================
            // PRINTING
            // =====================================

            ["Print"] = "Drucken",
            ["PrintPreview"] = "Druckvorschau",
            ["PrintSettings"] = "Druckeinstellungen",
            ["Printer"] = "Drucker",
            ["Page"] = "Seite",
            ["Pages"] = "Seiten",
            ["Copies"] = "Kopien",
            ["Quality"] = "Qualität",
            ["Orientation"] = "Ausrichtung",
            ["Portrait"] = "Hochformat",
            ["Landscape"] = "Querformat",

            // =====================================
            // EXPORT & IMPORT
            // =====================================

            ["Export"] = "Exportieren",
            ["Import"] = "Importieren",
            ["Format"] = "Format",
            ["Destination"] = "Ziel",
            ["Source"] = "Quelle",
            ["Progress"] = "Fortschritt",
            ["Cancelled"] = "Abgebrochen",

            // =====================================
            // ROUND RULES WINDOW
            // =====================================

            ["RoundRulesConfiguration"] = "Rundenregel-Konfiguration",
            ["WinnerBracketRules"] = "Gewinnerbaum-Regeln",
            ["LoserBracketRules"] = "Verliererbaum-Regeln",
            ["RoundRobinFinalsRules"] = "Round-Robin-Finals-Regeln",
            ["RoundRobinFinals"] = "Finalrunde",
            ["ResetToDefault"] = "Auf Standard zurücksetzen",

            // =====================================
            // AUTHENTICATION
            // =====================================
            ["Account"] = "Konto",
            ["Login"] = "Anmelden",
            ["Logout"] = "Abmelden",
            ["Register"] = "Registrieren",
            ["Username"] = "Benutzername",
            ["Password"] = "Passwort",
            ["ConfirmPassword"] = "Passwort wiederholen",
            ["Email"] = "E-Mail",
            ["FirstName"] = "Vorname",
            ["LastName"] = "Nachname",
            ["RememberSession"] = "Angemeldet bleiben",
            ["LoginSubtitle"] = "Melde dich mit deinem Konto an.",
            ["RegisterSubtitle"] = "Erstelle ein neues Konto für den Hub.",
            ["LogoutSuccess"] = "Erfolgreich abgemeldet.",
            ["AuthRegistrationFailed"] = "Registrierung fehlgeschlagen.",
            ["Profile"] = "Profil",
            ["ProfileInfoText"] = "Deine Profildaten werden zentral im Hub verwaltet. Bearbeitung folgt in Kürze.",

            // Neu hinzugefügte Übersetzungen
            ["AuthNotLoggedIn"] = "Nicht angemeldet.",
            ["ProfileUpdated"] = "Profil aktualisiert.",
            ["LicenseAutoFill"] = "Lizenz aus Anwendung übernehmen",
            ["LicenseAutoFillSuccess"] = "Lizenzschlüssel übernommen.",
            ["LicenseAutoFillMissing"] = "Kein Lizenzschlüssel gefunden.",
            ["LoginSuccess"] = "Erfolgreich angemeldet. Willkommen zurück und viel Spaß mit dem Dart Turnierplaner!",
            ["RegistrationSuccess"] = "Registrierung erfolgreich abgeschlossen. Du kannst dein Konto jetzt im Dart Turnierplaner nutzen.",

            // Turniereinstellungen Dialog
            ["TournamentSettings"] = "Turniereinstellungen",
            ["TournamentMetadata"] = "Turnier-Metadaten",
            ["TournamentName"] = "Turniername",
            ["TournamentDescription"] = "Beschreibung",
            ["TournamentLocation"] = "Veranstaltungsort",
            ["TournamentSchedule"] = "Zeitplan",
            ["StartDate"] = "Startdatum",
            ["StartTime"] = "Startzeit",
            ["PowerScoring"] = "PowerScoring",
            ["QRRegistration"] = "QR-Registrierung",
            ["PublicView"] = "Öffentliche Ansicht",
            ["TotalPlayers"] = "Gesamtspieler",
        };
    }
}