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
            
            // ✅ NEU: PowerScoring Übersetzungen
            ["PowerScoring"] = "PowerScoring",
            ["PowerScoringRequiresLicense"] = "🎯 PowerScoring ist ein Premium-Feature\n\n" +
                "Mit PowerScoring können Sie:\n" +
                "• Spieler-Scores systematisch erfassen\n" +
                "• Automatische Ranking-Erstellung\n" +
                "• Optimale Gruppeneinteilung basierend auf Skill-Level\n\n" +
                "Aktivieren Sie eine Lizenz mit 'powerscore' Feature um diese Funktion zu nutzen.",
            ["FeatureNotAvailable"] = "Feature nicht verfügbar",
            
            // PowerScoring License Dialog
            ["PowerScoringLicenseRequired_Title"] = "PowerScoring Lizenz erforderlich",
            ["PowerScoringLicenseRequired_Message"] = "PowerScoring ist ein Premium-Feature, das Ihnen hilft, Spieler basierend auf ihrem Skill-Level zu organisieren.",
            ["PowerScoringLicenseRequired_BenefitsTitle"] = "PowerScoring beinhaltet:",
            ["PowerScoringLicenseRequired_Benefit1"] = "- Systematische Spieler-Score-Erfassung",
            ["PowerScoringLicenseRequired_Benefit2"] = "- Automatische Ranking-Erstellung",
            ["PowerScoringLicenseRequired_Benefit3"] = "- Optimale Gruppeneinteilung basierend auf Skill-Level",
            ["PowerScoringLicenseRequired_Benefit4"] = "- Flexible Scoring-Regeln (1x3, 8x3, 10x3, 15x3 Würfe)",
            ["PowerScoringLicenseRequired_Benefit5"] = "- Snake-Draft Gruppenzuteilung",
            ["PowerScoringLicenseRequired_ActionText"] = "Möchten Sie eine Lizenz mit dem PowerScoring-Feature anfordern?",
            ["RequestLicense"] = "Lizenz anfordern",
            
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
            ["InvalidSubTabInterval"] = "Ungültiges Unterreiter-Intervall. Bitte eine Zahl ≥ 1 eingeben.",
            
            // =====================================
            // ROUND RULES WINDOW
            // =====================================
            ["RoundRulesConfiguration"] = "Rundenregeln-Konfiguration",
            ["WinnerBracketRules"] = "Winner Bracket Regeln",
            ["LoserBracketRules"] = "Loser Bracket Regeln",
            ["RoundRobinFinalsRules"] = "Round Robin Finals Regeln",
            ["RoundRobinFinals"] = "Finalrunde",
            ["ResetToDefault"] = "Auf Standard zurücksetzen"
      };
    }
}