using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German
{
    public class GermanPlannerImportLanguageProvider : ILanguageSection
    {
        public Dictionary<string, string> GetSectionTranslations()
        {
            return new Dictionary<string, string>
            {
                ["PlannerImportDialogTitle"] = "Planner Teilnehmer",
                ["PlannerImportHeader"] = "Turnierteilnehmer zuordnen",
                ["PlannerImportFirstName"] = "Vorname",
                ["PlannerImportNickname"] = "Spitzname",
                ["PlannerImportLastName"] = "Nachname",
                ["PlannerImportEmail"] = "E-Mail",
                ["PlannerImportClass"] = "Klasse",
                ["PlannerImportGroup"] = "Gruppe",
                ["PlannerImportAddRow"] = "Zeile hinzufügen",
                ["PlannerImportRemoveRow"] = "Zeile entfernen",
                ["PlannerImportCancel"] = "Abbrechen",
                ["PlannerImportApply"] = "Übernehmen",
                ["PlannerImportSave"] = "Speichern",
                ["PlannerImportPlayers"] = "Spieler übernehmen",
                ["PlannerImportNext"] = "Nächster Schritt",
                ["PlannerImportReviewTitle"] = "Import prüfen",
                ["PlannerImportReviewSubtitle"] = "Metadaten, Spielregeln und Spieler prüfen",
                ["PlannerImportReviewMetadata"] = "Metadaten",
                ["PlannerImportReviewClasses"] = "Klassen",
                ["PlannerImportReviewGameRules"] = "Spielregeln",
                ["PlannerImportReviewPlayers"] = "Spieler",
                ["PlannerImportReviewImport"] = "Importieren",
                ["PlannerImportReviewBack"] = "Zurück"
            };
        }
    }
}
