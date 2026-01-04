using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English
{
    public class EnglishPlannerImportLanguageProvider : ILanguageSection
    {
        public Dictionary<string, string> GetSectionTranslations()
        {
            return new Dictionary<string, string>
            {
                ["PlannerImportDialogTitle"] = "Planner Participants",
                ["PlannerImportHeader"] = "Assign tournament participants",
                ["PlannerImportFirstName"] = "First name",
                ["PlannerImportNickname"] = "Nickname",
                ["PlannerImportLastName"] = "Last name",
                ["PlannerImportEmail"] = "Email",
                ["PlannerImportClass"] = "Class",
                ["PlannerImportGroup"] = "Group",
                ["PlannerImportAddRow"] = "Add row",
                ["PlannerImportRemoveRow"] = "Remove row",
                ["PlannerImportCancel"] = "Cancel",
                ["PlannerImportApply"] = "Apply",
                ["PlannerImportSave"] = "Save",
                ["PlannerImportPlayers"] = "Import players",
                ["PlannerImportNext"] = "Next step",
                ["PlannerImportReviewTitle"] = "Review import",
                ["PlannerImportReviewSubtitle"] = "Review metadata, rules and players",
                ["PlannerImportReviewMetadata"] = "Metadata",
                ["PlannerImportReviewClasses"] = "Classes",
                ["PlannerImportReviewGameRules"] = "Game rules",
                ["PlannerImportReviewPlayers"] = "Players",
                ["PlannerImportReviewImport"] = "Import",
                ["PlannerImportReviewBack"] = "Back"
            };
        }
    }
}
