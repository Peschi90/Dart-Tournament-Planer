using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German
{
    public class GermanPlannerFetchLanguageProvider : ILanguageSection
    {
        public Dictionary<string, string> GetSectionTranslations()
        {
            return new Dictionary<string, string>
            {
                ["PlannerFetchDialogTitle"] = "Hub-Turniere abrufen",
                ["PlannerFetchDialogSubtitle"] = "Alle Turniere für deinen Lizenzschlüssel anzeigen",
                ["PlannerFetchDescription"] = "Frage geplante, aktive und kürzlich abgeschlossene Turniere direkt beim Tournament Hub ab.",
                ["PlannerFetchLicenseLabel"] = "Lizenzschlüssel",
                ["PlannerFetchDaysLabel"] = "Tage für abgeschlossene Turniere",
                ["PlannerFetchFetchButton"] = "Turniere abrufen",
                ["PlannerFetchCloseButton"] = "Schließen",
                ["PlannerFetchStatusLoading"] = "Turniere werden geladen...",
                ["PlannerFetchStatusEmpty"] = "Keine Turniere im gewählten Zeitraum gefunden.",
                ["PlannerFetchStatusError"] = "Turniere konnten nicht geladen werden: {0}",
                ["PlannerFetchUpdatedAt"] = "Aktualisiert",
                ["PlannerFetchStartDate"] = "Start",
                ["PlannerFetchLocation"] = "Ort",
                ["PlannerFetchStatus"] = "Status",
                ["PlannerFetchGameMode"] = "Modus",
                ["PlannerFetchLicenseMissing"] = "Kein Lizenzschlüssel gefunden. Bitte zuerst aktivieren.",
                ["PlannerFetchTimeout"] = "Der Tournament Hub hat nicht rechtzeitig geantwortet.",
                ["PlannerFetchSummary"] = "{0} Turniere geladen",
                ["PlannerFetchCopied"] = "Kopiert",
                ["PlannerFetchCopyId"] = "ID kopieren"
            };
        }
    }
}
