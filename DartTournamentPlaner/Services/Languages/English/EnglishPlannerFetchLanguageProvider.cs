using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English
{
    public class EnglishPlannerFetchLanguageProvider : ILanguageSection
    {
        public Dictionary<string, string> GetSectionTranslations()
        {
            return new Dictionary<string, string>
            {
                ["PlannerFetchDialogTitle"] = "Planner Hub Tournaments",
                ["PlannerFetchDialogSubtitle"] = "Fetch tournaments linked to your license key",
                ["PlannerFetchDescription"] = "Request planned, active and recently finished tournaments from the Tournament Hub.",
                ["PlannerFetchLicenseLabel"] = "License Key",
                ["PlannerFetchDaysLabel"] = "Days for completed tournaments",
                ["PlannerFetchFetchButton"] = "Fetch tournaments",
                ["PlannerFetchCloseButton"] = "Close",
                ["PlannerFetchStatusLoading"] = "Loading tournaments...",
                ["PlannerFetchStatusEmpty"] = "No tournaments found for the selected window.",
                ["PlannerFetchStatusError"] = "Could not fetch tournaments: {0}",
                ["PlannerFetchUpdatedAt"] = "Updated",
                ["PlannerFetchStartDate"] = "Start",
                ["PlannerFetchLocation"] = "Location",
                ["PlannerFetchStatus"] = "Status",
                ["PlannerFetchGameMode"] = "Mode",
                ["PlannerFetchLicenseMissing"] = "No license key available. Please activate your license first.",
                ["PlannerFetchTimeout"] = "The Tournament Hub did not respond in time.",
                ["PlannerFetchSummary"] = "{0} tournaments received",
                ["PlannerFetchCopied"] = "Copied",
                ["PlannerFetchCopyId"] = "Copy ID"
            };
        }
    }
}
