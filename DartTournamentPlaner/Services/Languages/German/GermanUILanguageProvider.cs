using System.Collections.Generic;
using DartTournamentPlaner.Services.Languages; // Assuming the ILanguageSection is in this namespace

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für UI-Elemente und allgemeine Anwendungskomponenten
/// </summary>
public class GermanUILanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Anwendungseinstellungen
            ["Settings"] = "Einstellungen",
            ["Language"] = "Sprache",
            ["Theme"] = "Design",
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
            ["Edit"] = "Bearbeiten",
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
            ["Status"] = "Status",
            ["Position"] = "Platz",
            ["Winner"] = "Sieger",
            ["Information"] = "Information",
            ["Warning"] = "Warnung",
            ["Error"] = "Fehler",

            // Eingabedialog
            ["InputDialog"] = "Eingabe",
            ["EnterName"] = "Name eingeben:",

            // Loading Spinner / Lade-Anzeige
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
            ["InvalidClassInterval"] = "Ungültiges Klassenintervall. Bitte eine Zahl = 1 eingeben.",
            ["InvalidSubTabInterval"] = "Ungültiges Unterreiterintervall. Bitte eine Zahl = 1 eingeben.",

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
            ["Bronze"] = "Bronze"
        };
    }
}