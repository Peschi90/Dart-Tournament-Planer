using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für PowerScoring Feature
/// </summary>
public class GermanPowerScoringLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // =====================================
            // POWERSCORING WINDOW
            // =====================================
            ["PowerScoring_Title"] = "PowerScoring - Spieler-Einteilung",
            ["PowerScoring_Setup"] = "Einrichtung",
            ["PowerScoring_Scoring"] = "Scoring",
            ["PowerScoring_Results"] = "Ergebnisse",
            
            // Setup Panel
            ["PowerScoring_Rule"] = "Regel:",
            ["PowerScoring_ThrowsOf3x1"] = "1 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x2"] = "2 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x3"] = "3 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x4"] = "4 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x5"] = "5 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x6"] = "6 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x7"] = "7 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x8"] = "8 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x9"] = "9 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x10"] = "10 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x11"] = "11 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x12"] = "12 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x13"] = "13 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x14"] = "14 x 3 Würfe",
            ["PowerScoring_ThrowsOf3x15"] = "15 x 3 Würfe",
            
            ["PowerScoring_TournamentId"] = "Turnier-ID:",
            ["PowerScoring_GenerateId"] = "🔄 Neue ID generieren",
            
            ["PowerScoring_AddPlayer"] = "Spieler hinzufügen",
            ["PowerScoring_PlayerName"] = "Spielername",
            ["PowerScoring_PlayerList"] = "Spielerliste:",
            ["PowerScoring_Remove"] = "Entfernen",
            
            // Buttons
            ["PowerScoring_NewSession"] = "Neue Session",
            ["PowerScoring_StartScoring"] = "Scoring starten",
            ["PowerScoring_CompletScoring"] = "Scoring abschließen",
            ["PowerScoring_PrintQRCodes"] = "QR-Codes drucken",
            ["PowerScoring_ExportGroups"] = "Gruppen erstellen",
            
            // Results
            ["PowerScoring_ResultsTitle"] = "📊 Ergebnisse (sortiert nach Score):",
            ["PowerScoring_Rank"] = "Rang",
            ["PowerScoring_Player"] = "Spieler",
            ["PowerScoring_Total"] = "Gesamt",
            ["PowerScoring_Average"] = "Durchschnitt",
            
            // Context Menu
            ["PowerScoring_ShowDetails"] = "📊 Details anzeigen",
            ["PowerScoring_CopyPlayerName"] = "📋 Spielername kopieren",
            
            // QR Code Panel
            ["PowerScoring_QRCodeFor"] = "QR-Code für",
            ["PowerScoring_URL"] = "URL:",
            ["PowerScoring_CopyURL"] = "URL kopieren",
            ["PowerScoring_LiveScore"] = "Live-Score:",
            ["PowerScoring_WaitingForScores"] = "Warte auf Scores...",
            ["PowerScoring_Statistics"] = "Statistiken:",
            ["PowerScoring_Highest"] = "Höchster",
            ["PowerScoring_TotalDarts"] = "Darts gesamt",
            ["PowerScoring_Completed"] = "Abgeschlossen",
            ["PowerScoring_Waiting"] = "Wartend...",
            ["PowerScoring_ShowDetails"] = "Details anzeigen",
            
            // =====================================
            // PLAYER DETAILS DIALOG
            // =====================================
            ["PowerScoring_PlayerDetails_Title"] = "Spieler-Details",
            ["PowerScoring_PlayerDetails_Statistics"] = "📈 Statistiken",
            ["PowerScoring_PlayerDetails_Performance"] = "🎯 Leistung",
            ["PowerScoring_PlayerDetails_SessionInfo"] = "ℹ️ Session-Info",
            ["PowerScoring_PlayerDetails_ThrowHistory"] = "🎲 Wurf-Historie",
            
            ["PowerScoring_PlayerDetails_TotalScore"] = "Gesamtscore",
            ["PowerScoring_PlayerDetails_Average"] = "Durchschnitt",
            ["PowerScoring_PlayerDetails_HighestThrow"] = "Höchster Wurf",
            ["PowerScoring_PlayerDetails_TotalDarts"] = "Darts gesamt",
            ["PowerScoring_PlayerDetails_Rounds"] = "Runden:",
            ["PowerScoring_PlayerDetails_Duration"] = "Dauer:",
            ["PowerScoring_PlayerDetails_SubmittedVia"] = "Eingereicht via:",
            ["PowerScoring_PlayerDetails_Close"] = "✅ Schließen",
            
            // =====================================
            // ADVANCED GROUP DISTRIBUTION
            // =====================================
            ["PowerScoring_GroupDistribution_Title"] = "Erweiterte Gruppeneinteilung",
            ["PowerScoring_GroupDistribution_Description"] = "Konfigurieren Sie Klassen, Gruppen und Spieler pro Gruppe",
            ["PowerScoring_GroupDistribution_DistributionPreview"] = "Verteilungs-Vorschau:",
            
            ["PowerScoring_GroupDistribution_SelectClasses"] = "Klassen auswählen:",
            ["PowerScoring_GroupDistribution_GroupsPerClass"] = "Gruppen pro Klasse:",
            ["PowerScoring_GroupDistribution_PlayersPerGroup"] = "Spieler pro Gruppe:",
            ["PowerScoring_GroupDistribution_Advanced"] = "⚙️ Erweitert",
            
            ["PowerScoring_GroupDistribution_1Group"] = "1 Gruppe",
            ["PowerScoring_GroupDistribution_2Groups"] = "2 Gruppen",
            ["PowerScoring_GroupDistribution_3Groups"] = "3 Gruppen",
            ["PowerScoring_GroupDistribution_4Groups"] = "4 Gruppen",
            
            ["PowerScoring_GroupDistribution_2Players"] = "2 Spieler",
            ["PowerScoring_GroupDistribution_3Players"] = "3 Spieler",
            ["PowerScoring_GroupDistribution_4Players"] = "4 Spieler",
            ["PowerScoring_GroupDistribution_5Players"] = "5 Spieler",
            ["PowerScoring_GroupDistribution_6Players"] = "6 Spieler",
            
            ["PowerScoring_GroupDistribution_Generate"] = "🎲 Generieren",
            ["PowerScoring_GroupDistribution_Export"] = "💾 Exportieren",
            ["PowerScoring_GroupDistribution_Cancel"] = "❌ Abbrechen",
            
            // =====================================
            // ADVANCED SETTINGS DIALOG
            // =====================================
            ["PowerScoring_AdvancedSettings_Title"] = "Erweiterte Verteilungs-Einstellungen",
            
            // Distribution Modes
            ["PowerScoring_AdvancedSettings_DistributionMode"] = "🎯 Verteilungsmodus",
            ["PowerScoring_AdvancedSettings_Mode_Balanced"] = "⚖️ Ausgewogen (Gleichmäßige Verteilung)",
            ["PowerScoring_AdvancedSettings_Mode_SnakeDraft"] = "🐍 Snake Draft (1-2-3-4-4-3-2-1)",
            ["PowerScoring_AdvancedSettings_Mode_TopHeavy"] = "🔝 Top-Heavy (Stärkste zuerst)",
            ["PowerScoring_AdvancedSettings_Mode_Random"] = "🎲 Zufällig",
            
            ["PowerScoring_AdvancedSettings_Mode_Balanced_Desc"] = "Spieler werden gleichmäßig auf Gruppen verteilt basierend auf Ranking.",
            ["PowerScoring_AdvancedSettings_Mode_SnakeDraft_Desc"] = "Spieler werden im Snake-Muster verteilt: Gruppe 1-2-3-4-4-3-2-1, für faire Verteilung.",
            ["PowerScoring_AdvancedSettings_Mode_TopHeavy_Desc"] = "Stärkste Spieler werden in die ersten Gruppen platziert, wodurch stärkere obere Gruppen entstehen.",
            ["PowerScoring_AdvancedSettings_Mode_Random_Desc"] = "Spieler werden zufällig auf Gruppen verteilt (nützlich für Tests oder Varietät).",
            
            // Player Limits
            ["PowerScoring_AdvancedSettings_PlayerLimits"] = "👥 Spieler-Limits pro Gruppe",
            ["PowerScoring_AdvancedSettings_Minimum"] = "Minimum:",
            ["PowerScoring_AdvancedSettings_Maximum"] = "Maximum:",
            
            // Class Rules
            ["PowerScoring_AdvancedSettings_ClassRules"] = "🏆 Klassen-spezifische Regeln",
            ["PowerScoring_AdvancedSettings_Groups"] = "Gruppen:",
            ["PowerScoring_AdvancedSettings_Players"] = "Spieler:",
            ["PowerScoring_AdvancedSettings_Skip"] = "Überspringen",
            
            // Info
            ["PowerScoring_AdvancedSettings_Tip"] = "💡",
            ["PowerScoring_AdvancedSettings_TipText"] = "Tipp: Verwenden Sie klassen-spezifische Regeln um individuelle Gruppenanzahlen für einzelne Klassen zu erstellen. Leere Felder verwenden Standard-Einstellungen.",
            
            // ✅ NEU: Zusätzliche UI-Texte
            ["PowerScoring_AdvancedSettings_RegenerateGroups"] = "🔄 Gruppen neu generieren",
            ["PowerScoring_AdvancedSettings_AutoRegenerate"] = "Die Gruppen werden automatisch mit den neuen Einstellungen neu generiert.",
            
            // Buttons
            ["PowerScoring_AdvancedSettings_Apply"] = "✅ Übernehmen",
            ["PowerScoring_AdvancedSettings_Cancel"] = "❌ Abbrechen",
            
            // Validation Messages
            ["PowerScoring_AdvancedSettings_InvalidMinPlayers"] = "Minimale Spieleranzahl muss mindestens 1 sein.",
            ["PowerScoring_AdvancedSettings_InvalidMaxPlayers"] = "Maximale Spieleranzahl muss größer oder gleich der minimalen sein.",
            ["PowerScoring_AdvancedSettings_InvalidInput"] = "Ungültige Eingabe",
            
            // Success Messages
            ["PowerScoring_AdvancedSettings_SettingsApplied"] = "Einstellungen übernommen",
            ["PowerScoring_AdvancedSettings_SettingsAppliedMessage"] = "Erweiterte Verteilungs-Einstellungen wurden übernommen.",
            
            // =====================================
            // CONFIRM DIALOGS
            // =====================================
            ["PowerScoring_Confirm_NewSession_Title"] = "Neue Session erstellen",
            ["PowerScoring_Confirm_NewSession_Message"] = "Möchten Sie wirklich eine neue Session erstellen?\n\nDie aktuelle Session wird gelöscht.",
            
            ["PowerScoring_Confirm_SavedSession_Title"] = "Gespeicherte Session gefunden",
            ["PowerScoring_Confirm_SavedSession_Message"] = "Es wurde eine gespeicherte PowerScoring Session gefunden.\n\nMöchten Sie diese Session fortsetzen?",
            
            ["PowerScoring_Success_SessionLoaded"] = "Session geladen",
            ["PowerScoring_Success_NewSession"] = "Neue Session",
            ["PowerScoring_Success_NewSessionCreated"] = "Neue PowerScoring Session erstellt.",
            
            ["PowerScoring_Error_NoPlayers"] = "Keine Spieler",
            ["PowerScoring_Error_AddPlayers"] = "Bitte fügen Sie mindestens einen Spieler hinzu.",
            ["PowerScoring_Error_PlayerExists"] = "Duplikat",
            ["PowerScoring_Error_PlayerExistsMessage"] = "Spieler existiert bereits.",
            
            ["PowerScoring_Warning_NoClasses"] = "Keine Klassen ausgewählt",
            ["PowerScoring_Warning_SelectClasses"] = "Bitte wählen Sie mindestens eine Klasse aus.",
            
            ["PowerScoring_Warning_NoPlayersToPrint"] = "Es gibt keine Spieler für die QR-Codes gedruckt werden können.",
            ["PowerScoring_Info_PrintQRCodes"] = "QR-Codes drucken",
            ["PowerScoring_Info_PrintFeatureComingSoon"] = "QR-Code-Druck-Funktion wird bald implementiert.",
            
            // =====================================
            // GENERAL TRANSLATIONS
            // =====================================
            ["Success"] = "Erfolg",
            ["Error"] = "Fehler",
            ["Warning"] = "Warnung",
            ["Information"] = "Information",
            ["Question"] = "Frage",
            ["Yes"] = "Ja",
            ["No"] = "Nein",
            ["OK"] = "OK",
            ["Cancel"] = "Abbrechen",
            ["Close"] = "Schließen",
            ["Apply"] = "Übernehmen",
        };
    }
}
