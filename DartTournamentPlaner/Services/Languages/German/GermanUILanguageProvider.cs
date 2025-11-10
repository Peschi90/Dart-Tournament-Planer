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
            // ANWENDUNGSKERN ÜBERSETZUNGEN
            // =====================================
            
            // Anwendungseinstellungen
            ["Settings"] = "Einstellungen",
            ["Language"] = "Sprache",
            ["AutoSave"] = "Automatisches Speichern",
            ["AutoSaveInterval"] = "Speicherintervall (Minuten)",
            ["Save"] = "Speichern",
            ["Cancel"] = "Abbrechen",
            
            // Menü-Einträge
            ["File"] = "Datei",
            ["New"] = "Neu",
            ["Open"] = "Öffnen",
            ["SaveAs"] = "Speichern unter",
            ["Print"] = "Drucken",
            ["Exit"] = "Beenden",
            ["View"] = "Ansicht",
            ["Help"] = "Hilfe",
            ["About"] = "Über",
            
            // Status-Anzeigen
            ["HasUnsavedChanges"] = "Geändert",
            ["NotSaved"] = "Nicht gespeichert",
            ["Saved"] = "Gespeichert",
            ["Ready"] = "Bereit",
            
            // Allgemeine UI-Elemente
            ["Close"] = "Schließen",
            ["OK"] = "OK",
            ["Start"] = "Start",
            ["Stop"] = "Stop",
            ["Player"] = "Spieler",
            ["Match"] = "Spiel",
            ["Result"] = "Ergebnis",
            ["Position"] = "Platz",
            ["Winner"] = "Sieger",
            ["Information"] = "Information",
            ["Warning"] = "Warnung",
            ["Error"] = "Fehler",
  
            // Eingabedialog
            ["InputDialog"] = "Eingabe",
            ["EnterName"] = "Name eingeben:",
    
            // Loading Spinner
            ["Loading"] = "Wird geladen...",
            ["CheckingGroupStatus"] = "Überprüfe Gruppenstatus...",
            ["ProcessingMatches"] = "Verarbeite Spiele...",
            ["CheckingCompletion"] = "Überprüfe Abschluss...",
   
            // Startup und Update-Funktionen
            ["StartingApplication"] = "Starte Anwendung...",
            ["AppSubtitle"] = "Moderne Turnierverwaltung",
            ["CheckingForUpdates"] = "Suche nach Updates...",
            ["ConnectingToGitHub"] = "Verbinde mit GitHub...",
            ["AnalyzingReleases"] = "Analysiere Releases...",
            ["UpdateAvailable"] = "Update verfügbar",
            ["WhatsNew"] = "Was ist neu:",
            ["RemindLater"] = "Später erinnern",
            ["SkipVersion"] = "Version überspringen",
            ["DownloadUpdate"] = "Jetzt herunterladen",
            ["DownloadAndInstall"] = "Herunterladen & Installieren",
            ["DownloadingUpdate"] = "Update wird heruntergeladen",
            ["PreparingDownload"] = "Bereite Download vor...",
            ["DownloadingSetup"] = "Lade Setup herunter...",
            ["DownloadCompleted"] = "Download abgeschlossen, prüfe Datei...",
            ["PreparingInstallation"] = "Bereite Installation vor...",
            ["StartingInstallation"] = "Starte Installation...",
            ["InstallationStarted"] = "Installation gestartet",
            ["InstallationCancelled"] = "Installation abgebrochen",
            ["ErrorStartingSetup"] = "Fehler beim Starten",
            ["AdminRightsRequired"] = "Administratorrechte erforderlich",
            ["NoUpdateAvailable"] = "Keine Updates verfügbar",

            // Übersichtskonfigurationsdialog
            ["OverviewConfiguration"] = "Übersichtskonfiguration",
            ["TournamentOverviewConfiguration"] = "Turnierübersichtskonfiguration",
            ["TimeBetweenClasses"] = "Zeit zwischen Turnierklassen:",
            ["TimeBetweenSubTabs"] = "Zeit zwischen Unterreitern:",
            ["Seconds"] = "Sekunden",
            ["ShowOnlyActiveClassesText"] = "Nur Klassen mit aktiven Gruppen anzeigen",
            ["OverviewInfoText"] = "Live-Turnierdarstellung für alle Klassen mit automatischem Wechsel",
            ["InvalidClassInterval"] = "Ungültiges Klassenintervall. Bitte eine Zahl ≥ 1 eingeben.",
            ["InvalidSubTabInterval"] = "Ungültiges Unterreiterintervall. Bitte eine Zahl ≥ 1 eingeben.",
            
            // Turnierübersicht Texte
            ["TournamentName"] = "⚽ Turnier:",
            ["CurrentPhase"] = "🏁 Aktuelle Phase:",
            ["GroupsCount"] = "👥 Gruppen:",
            ["PlayersTotal"] = "👤 Gesamtspieler:",
            ["GameRulesColon"] = "📋 Spielregeln:",
            ["CompletedGroups"] = "✅ Abgeschlossene Gruppen:",
            ["QualifiedPlayers"] = "🎯 Qualifizierte Spieler:",
            ["KnockoutMatches"] = "🏆 K.-o.-Spiele:",
            ["Completed"] = "abgeschlossen",
            
            // Weitere fest codierte Texte
            ["Finalists"] = "Finalisten",
            ["KnockoutParticipants"] = "K.-o.-Teilnehmer",
            ["PlayersText"] = "Spieler",
            ["OverviewModeTitle"] = "Turnierübersichtsmodus",
            ["NewTournament"] = "Neues Turnier",
            ["CreateNewTournament"] = "Neues Turnier erstellen? Ungespeicherte Änderungen gehen verloren.",
            ["UnsavedChanges"] = "Ungespeicherte Änderungen",
            ["SaveBeforeExit"] = "Es gibt ungespeicherte Änderungen. Möchten Sie vor dem Beenden speichern?",
            ["CustomFileNotImplemented"] = "Benutzerdefiniertes Laden von Dateien noch nicht implementiert.",
            ["CustomFileSaveNotImplemented"] = "Benutzerdefiniertes Speichern von Dateien noch nicht implementiert.",
            ["ErrorOpeningHelp"] = "Fehler beim Öffnen der Hilfe:",
            ["ErrorOpeningOverview"] = "Fehler beim Öffnen der Turnierübersicht:",
            ["ErrorSavingData"] = "Fehler beim Speichern der Daten:",

            // Spenden- und Bug-Report-Funktionen
            ["Donate"] = "💝",
            ["DonateTooltip"] = "Support the development of this project",
            ["BugReport"] = "🐛 Fehler melden",
            ["BugReportTooltip"] = "Report bugs or suggest improvements",
            ["BugReportTitle"] = "Bug Report",
            ["BugReportDescription"] = "Describe the problem or your improvement idea:",
            ["BugReportEmailSubject"] = "Dart Tournament Planner - Bug Report",
            ["BugReportSteps"] = "Steps to reproduce:",
            ["BugReportExpected"] = "Expected behavior:",
            ["BugReportActual"] = "Actual behavior:",
            ["BugReportSystemInfo"] = "System Information:",
            ["BugReportVersion"] = "Version:",
            ["BugReportOS"] = "Operating System:",
            ["BugReportSubmitEmail"] = "Send via Email",
            ["BugReportSubmitGitHub"] = "Open on GitHub",
            ["ThankYouSupport"] = "Thank you for your support!",
            ["BugReportSent"] = "Bug report has been successfully sent. Thank you!",
            ["ErrorSendingBugReport"] = "Error sending bug report:",
            ["SupportDevelopment"] = "Support Development",
            ["DonationMessage"] = "Do you like this Dart Tournament Planner?\n\nSupport further development with a small donation.\nEvery contribution helps with improving and maintaining the software.",
            ["OpenDonationPage"] = "Open Donation Page",

            // Hauptfenster
            ["AppTitle"] = "Dart Turnier Planer",
            ["Platinum"] = "Platin",
            ["Gold"] = "Gold",
            ["Silver"] = "Silber",
            ["Bronze"] = "Bronze",
       
            // =====================================
            // ÜBER-DIALOG
            // =====================================

            ["About"] = "Über",
            ["AboutDescription"] = "Eine umfassende und moderne Anwendung zur Organisation und Verwaltung von Dart-Turnieren. " +
            "Unterstützt mehrere Turnierformate, einschließlich Gruppenphase, Finalrunde und K.-o.-Phase. " +
            "Bietet Echtzeit-Spielverfolgung, detaillierte Spielerstatistiken, Tournament-Hub-Integration via WebSocket, " +
            "QR-Code-Generierung für einfachen mobilen Zugriff und professionelle Turnierdokumentation mit Druckfunktion.",
            ["AboutCredits"] = "Besonderer Dank gilt allen Mitwirkenden, Testern und der Dart-Community für ihr wertvolles Feedback und ihre Unterstützung. " +
            "Dieses Projekt wird kontinuierlich durch Community-Engagement und Feedback verbessert.",
            ["Developer"] = "Entwickler",
            ["DeveloperName"] = "Marcel Peschka",
            ["Framework"] = "Framework",
            ["License"] = "Lizenz",
            ["LicenseType"] = "Open Source (MIT-Lizenz)",
            ["Website"] = "Webseite",
            ["WebsiteUrl"] = "https://github.com/Peschi90/Dart-Turnament-Planer",
            ["OpenSource"] = "Open Source",
            ["SpecialThanks"] = "Besonderer Dank",
            ["AppInformation"] = "Anwendungsinformationen",
            ["TechnicalDetails"] = "Technische Details",
            ["Features"] = "Funktionen",
            ["FeatureList"] = "• Mehrere Turnierformate (Gruppe, Finale, K.-o.)\n" +
            "• Echtzeit-Spielverfolgung und Punkteverwaltung\n" +
            "• Umfassende Spielerstatistiken\n" +
            "• WebSocket-basierte Tournament-Hub-Integration\n" +
            "• QR-Code-Generierung für mobilen Zugriff\n" +
            "• Professionelle Druckfunktion\n" +
            "• Mehrsprachige Unterstützung (Englisch/Deutsch)\n" +
            "• Hell-/Dunkelmodus-Unterstützung\n" +
            "• Automatisches Speichern und Datenpersistenz",
            ["ContactSupport"] = "Support & Kontakt",
            ["GitHubRepository"] = "GitHub Repository",
            ["ReportIssue"] = "Problem melden",
            ["VersionInfo"] = "Versionsinformationen",

            // =====================================
            // THEME & DARK MODE
            // =====================================
    
            ["Theme"] = "Design",
            ["DarkMode"] = "Dunkler Modus",
            ["LightMode"] = "Heller Modus",
            ["SwitchToDarkMode"] = "Zu dunklem Modus wechseln",
            ["SwitchToLightMode"] = "Zu hellem Modus wechseln",
            ["ThemeSettings"] = "Design-Einstellungen",
            
            // =====================================
            // ALLGEMEINE UI-ELEMENTE
            // =====================================
      
            // Basis-Buttons
            ["Load"] = "Laden",
            ["Delete"] = "Löschen",
            ["Edit"] = "Bearbeiten",
            ["Add"] = "Hinzufügen",
            ["Remove"] = "Entfernen",
            ["Apply"] = "Anwenden",
            ["Reset"] = "Zurücksetzen",
            ["Refresh"] = "Aktualisieren",
            ["Clear"] = "Löschen",
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
            ["Up"] = "Nach oben",
            ["Down"] = "Nach unten",
            ["Left"] = "Links",
            ["Right"] = "Rechts",
            
            // Status-Begriffe
            ["Success"] = "Erfolg",
            ["Saving"] = "Wird gespeichert...",
            ["Processing"] = "Wird verarbeitet...",
            ["Complete"] = "Abgeschlossen",
            ["Failed"] = "Fehlgeschlagen",
            ["Ready"] = "Bereit",
            ["Busy"] = "Beschäftigt",
            
            // =====================================
            // DIALOG & FENSTER-ELEMENTE
            // =====================================
            
            // Fenster-Titel-Suffixe
            ["Configuration"] = "Konfiguration",
            ["Properties"] = "Eigenschaften",
            ["Options"] = "Optionen",
            ["Preferences"] = "Einstellungen",
            
            // Dialog-Typ-Begriffe
            ["Dialog"] = "Dialog",
            ["Window"] = "Fenster",
            ["Form"] = "Formular",
            ["Wizard"] = "Assistent",
            ["Setup"] = "Einrichtung",
       
            // =====================================
            // DATEN-ELEMENTE
            // =====================================
          
            // Tabellen & Listen
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
      
            // Sortieren & Filtern
            ["Sort"] = "Sortieren",
            ["SortBy"] = "Sortieren nach",
            ["Filter"] = "Filter",
            ["FilterBy"] = "Filtern nach",
            ["Search"] = "Suchen",
            ["SearchFor"] = "Suchen nach",
            ["Results"] = "Ergebnisse",
            ["NoResults"] = "Keine Ergebnisse",
     
            // =====================================
            // FORMULAR-ELEMENTE
            // =====================================
     
            // Eingabe-Labels
            ["Required"] = "Erforderlich",
            ["Optional"] = "Optional",
            ["Default"] = "Standard",
            ["Custom"] = "Benutzerdefiniert",
            ["Auto"] = "Automatisch",
            ["Manual"] = "Manuell",
            
            // Validierung
            ["Valid"] = "Gültig",
            ["Invalid"] = "Ungültig",
            ["ValidationError"] = "Validierungsfehler",
            ["RequiredField"] = "Pflichtfeld",
            ["InvalidFormat"] = "Ungültiges Format",
            ["ValueTooSmall"] = "Wert zu klein",
            ["ValueTooLarge"] = "Wert zu groß",
         
            // =====================================
            // SPRACHSPEZIFISCHE UI
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
            ["Log"] = "Protokoll",
            ["Version"] = "Version",
            ["Build"] = "Build",
            ["Platform"] = "Plattform",
            ["Memory"] = "Speicher",
            ["Performance"] = "Leistung",
        
            // =====================================
            // BARRIEREFREIHEIT
            // =====================================

            ["AccessibilityMode"] = "Barrierefreiheit",
            ["HighContrast"] = "Hoher Kontrast",
            ["LargeText"] = "Großer Text",
            ["ScreenReader"] = "Bildschirmleser",
            ["KeyboardNavigation"] = "Tastaturnavigation",
            
            // =====================================
            // DATEI & E/A
            // =====================================
            
            ["Folder"] = "Ordner",
            ["Path"] = "Pfad",
            ["Size"] = "Größe",
            ["Modified"] = "Geändert",
            ["Created"] = "Erstellt",
            ["Exists"] = "Vorhanden",
            ["NotFound"] = "Nicht gefunden",
            ["ReadOnly"] = "Schreibgeschützt",
            ["Permission"] = "Berechtigung",
    
            // =====================================
            // NETZWERK & VERBINDUNG
            // =====================================
     
            ["Connected"] = "Verbunden",
            ["Disconnected"] = "Getrennt",
            ["Connecting"] = "Verbinde...",
            ["Connection"] = "Verbindung",
            ["Network"] = "Netzwerk",
            ["Offline"] = "Offline",
            ["Online"] = "Online",
            ["Timeout"] = "Zeitüberschreitung",
            ["Retry"] = "Wiederholen",
        
            // =====================================
            // DRUCKEN
            // =====================================
        
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
            ["Cancelled"] = "Abgebrochen"
     };
    }
}