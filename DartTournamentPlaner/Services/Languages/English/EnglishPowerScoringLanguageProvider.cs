using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for PowerScoring Feature
/// </summary>
public class EnglishPowerScoringLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // =====================================
            // POWERSCORING WINDOW
            // =====================================
            ["PowerScoring_Title"] = "PowerScoring - Player Distribution",
            ["PowerScoring_Setup"] = "Setup",
            ["PowerScoring_Scoring"] = "Scoring",
            ["PowerScoring_Results"] = "Results",
            
            // Setup Panel
            ["PowerScoring_Rule"] = "Rule:",
            ["PowerScoring_ThrowsOf3x1"] = "1 x 3 Throws",
            ["PowerScoring_ThrowsOf3x2"] = "2 x 3 Throws",
            ["PowerScoring_ThrowsOf3x3"] = "3 x 3 Throws",
            ["PowerScoring_ThrowsOf3x4"] = "4 x 3 Throws",
            ["PowerScoring_ThrowsOf3x5"] = "5 x 3 Throws",
            ["PowerScoring_ThrowsOf3x6"] = "6 x 3 Throws",
            ["PowerScoring_ThrowsOf3x7"] = "7 x 3 Throws",
            ["PowerScoring_ThrowsOf3x8"] = "8 x 3 Throws",
            ["PowerScoring_ThrowsOf3x9"] = "9 x 3 Throws",
            ["PowerScoring_ThrowsOf3x10"] = "10 x 3 Throws",
            ["PowerScoring_ThrowsOf3x11"] = "11 x 3 Throws",
            ["PowerScoring_ThrowsOf3x12"] = "12 x 3 Throws",
            ["PowerScoring_ThrowsOf3x13"] = "13 x 3 Throws",
            ["PowerScoring_ThrowsOf3x14"] = "14 x 3 Throws",
            ["PowerScoring_ThrowsOf3x15"] = "15 x 3 Throws",

            ["PowerScoring_TournamentId"] = "Tournament ID:",
            ["PowerScoring_GenerateId"] = "🔄 Generate New ID",
            
            ["PowerScoring_AddPlayer"] = "Add Player",
            ["PowerScoring_PlayerName"] = "Player Name",
            ["PowerScoring_PlayerList"] = "Player List:",
            ["PowerScoring_Remove"] = "Remove",
            
            // Buttons
            ["PowerScoring_NewSession"] = "New Session",
            ["PowerScoring_StartScoring"] = "Start Scoring",
            ["PowerScoring_CompleteScoring"] = "Complete Scoring",
            ["PowerScoring_PrintQRCodes"] = "Print QR Codes",
            ["PowerScoring_ExportGroups"] = "Export Groups",
            
            // Results
            ["PowerScoring_ResultsTitle"] = "📊 Results (sorted by score):",
            ["PowerScoring_Rank"] = "Rank",
            ["PowerScoring_Player"] = "Player",
            ["PowerScoring_Total"] = "Total",
            ["PowerScoring_Average"] = "Average",
            
            // Context Menu
            ["PowerScoring_ShowDetails"] = "📊 Show Details",
            ["PowerScoring_CopyPlayerName"] = "📋 Copy Player Name",
            
            // QR Code Panel
            ["PowerScoring_QRCodeFor"] = "QR Code for",
            ["PowerScoring_URL"] = "URL:",
            ["PowerScoring_CopyURL"] = "Copy URL",
            ["PowerScoring_LiveScore"] = "Live Score:",
            ["PowerScoring_WaitingForScores"] = "Waiting for scores...",
            ["PowerScoring_Statistics"] = "Statistics:",
            ["PowerScoring_Highest"] = "Highest",
            ["PowerScoring_TotalDarts"] = "Total Darts",
            ["PowerScoring_Completed"] = "Completed",
            ["PowerScoring_Waiting"] = "Waiting...",
            ["PowerScoring_ShowDetails"] = "Show Details",
            
            // =====================================
            // PLAYER DETAILS DIALOG
            // =====================================
            ["PowerScoring_PlayerDetails_Title"] = "Player Details",
            ["PowerScoring_PlayerDetails_Statistics"] = "📈 Statistics",
            ["PowerScoring_PlayerDetails_Performance"] = "🎯 Performance",
            ["PowerScoring_PlayerDetails_SessionInfo"] = "ℹ️ Session Info",
            ["PowerScoring_PlayerDetails_ThrowHistory"] = "🎲 Throw History",
            
            ["PowerScoring_PlayerDetails_TotalScore"] = "Total Score",
            ["PowerScoring_PlayerDetails_Average"] = "Average",
            ["PowerScoring_PlayerDetails_HighestThrow"] = "Highest Throw",
            ["PowerScoring_PlayerDetails_TotalDarts"] = "Total Darts",
            ["PowerScoring_PlayerDetails_Rounds"] = "Rounds:",
            ["PowerScoring_PlayerDetails_Duration"] = "Duration:",
            ["PowerScoring_PlayerDetails_SubmittedVia"] = "Submitted via:",
            ["PowerScoring_PlayerDetails_Close"] = "✅ Close",
            
            // =====================================
            // ADVANCED GROUP DISTRIBUTION
            // =====================================
            ["PowerScoring_GroupDistribution_Title"] = "Advanced Group Distribution",
            ["PowerScoring_GroupDistribution_Description"] = "Configure classes, groups, and players per group",
            ["PowerScoring_GroupDistribution_DistributionPreview"] = "Distribution Preview:",
            
            ["PowerScoring_GroupDistribution_SelectClasses"] = "Select Classes:",
            ["PowerScoring_GroupDistribution_GroupsPerClass"] = "Groups per Class:",
            ["PowerScoring_GroupDistribution_PlayersPerGroup"] = "Players per Group:",
            ["PowerScoring_GroupDistribution_Advanced"] = "⚙️ Advanced",
            
            ["PowerScoring_GroupDistribution_1Group"] = "1 Group",
            ["PowerScoring_GroupDistribution_2Groups"] = "2 Groups",
            ["PowerScoring_GroupDistribution_3Groups"] = "3 Groups",
            ["PowerScoring_GroupDistribution_4Groups"] = "4 Groups",
            
            ["PowerScoring_GroupDistribution_2Players"] = "2 Players",
            ["PowerScoring_GroupDistribution_3Players"] = "3 Players",
            ["PowerScoring_GroupDistribution_4Players"] = "4 Players",
            ["PowerScoring_GroupDistribution_5Players"] = "5 Players",
            ["PowerScoring_GroupDistribution_6Players"] = "6 Players",
            
            ["PowerScoring_GroupDistribution_Generate"] = "🎲 Generate",
            ["PowerScoring_GroupDistribution_Export"] = "💾 Export",
            ["PowerScoring_GroupDistribution_Cancel"] = "❌ Cancel",
            
            // ✅ NEW: Create Tournament
            ["PowerScoring_CreateTournament_Title"] = "Create Tournament",
            ["PowerScoring_CreateTournament_Create"] = "🏆 Create Tournament",
            ["PowerScoring_CreateTournament_Summary"] = "Tournament Overview",
            ["PowerScoring_CreateTournament_Warning"] = "Existing Tournament Found",
            ["PowerScoring_CreateTournament_WarningMessage"] = "The current tournament will be saved and a new tournament will be created based on this distribution.",
            ["PowerScoring_CreateTournament_Details"] = "Details",
            ["PowerScoring_CreateTournament_Confirmation"] = "Do you want to create the tournament with this configuration?",

            // ✅ PHASE 3: Success Messages
            ["PowerScoring_CreateTournament_Success_Title"] = "Tournament Created",
            ["PowerScoring_CreateTournament_Success_Message"] = "The tournament has been created successfully!\n\nYou can now continue with tournament management.",

            // =====================================
            // ADVANCED SETTINGS DIALOG
            // =====================================
            ["PowerScoring_AdvancedSettings_Title"] = "Advanced Distribution Settings",
            
            // Distribution Modes
            ["PowerScoring_AdvancedSettings_DistributionMode"] = "🎯 Distribution Mode",
            ["PowerScoring_AdvancedSettings_Mode_Balanced"] = "⚖️ Balanced (Even distribution)",
            ["PowerScoring_AdvancedSettings_Mode_SnakeDraft"] = "🐍 Snake Draft (1-2-3-4-4-3-2-1)",
            ["PowerScoring_AdvancedSettings_Mode_TopHeavy"] = "🔝 Top-Heavy (Strongest first)",
            ["PowerScoring_AdvancedSettings_Mode_Random"] = "🎲 Random",
            
            ["PowerScoring_AdvancedSettings_Mode_Balanced_Desc"] = "Players are evenly distributed across groups based on ranking.",
            ["PowerScoring_AdvancedSettings_Mode_SnakeDraft_Desc"] = "Players are distributed in a snake pattern: Group 1-2-3-4-4-3-2-1, ensuring fair distribution.",
            ["PowerScoring_AdvancedSettings_Mode_TopHeavy_Desc"] = "Strongest players are placed in the first groups, creating stronger upper groups.",
            ["PowerScoring_AdvancedSettings_Mode_Random_Desc"] = "Players are randomly assigned to groups (useful for testing or variety).",
            
            // Player Limits
            ["PowerScoring_AdvancedSettings_PlayerLimits"] = "👥 Player Limits Per Group",
            ["PowerScoring_AdvancedSettings_Minimum"] = "Minimum:",
            ["PowerScoring_AdvancedSettings_Maximum"] = "Maximum:",
            
            // Class Rules
            ["PowerScoring_AdvancedSettings_ClassRules"] = "🏆 Class-Specific Rules",
            ["PowerScoring_AdvancedSettings_Groups"] = "Groups:",
            ["PowerScoring_AdvancedSettings_Players"] = "Players:",
            ["PowerScoring_AdvancedSettings_Skip"] = "Skip",
            
            // Info
            ["PowerScoring_AdvancedSettings_Tip"] = "💡",
            ["PowerScoring_AdvancedSettings_TipText"] = "Tip: Use class-specific rules to create custom group counts for individual classes. Empty fields will use default settings.",
            
            // ✅ NEW: Additional UI texts
            ["PowerScoring_AdvancedSettings_RegenerateGroups"] = "🔄 Regenerate Groups",
            ["PowerScoring_AdvancedSettings_AutoRegenerate"] = "Groups will be automatically regenerated with the new settings.",
            
            // Buttons
            ["PowerScoring_AdvancedSettings_Apply"] = "✅ Apply",
            ["PowerScoring_AdvancedSettings_Cancel"] = "❌ Cancel",
            
            // Validation Messages
            ["PowerScoring_AdvancedSettings_InvalidMinPlayers"] = "Minimum players must be at least 1.",
            ["PowerScoring_AdvancedSettings_InvalidMaxPlayers"] = "Maximum players must be greater than or equal to minimum.",
            ["PowerScoring_AdvancedSettings_InvalidInput"] = "Invalid Input",
            
            // Success Messages
            ["PowerScoring_AdvancedSettings_SettingsApplied"] = "Settings Applied",
            ["PowerScoring_AdvancedSettings_SettingsAppliedMessage"] = "Advanced distribution settings have been applied.",
            
            // =====================================
            // CONFIRM DIALOGS
            // =====================================
            ["PowerScoring_Confirm_NewSession_Title"] = "Create New Session",
            ["PowerScoring_Confirm_NewSession_Message"] = "Do you really want to create a new session?\n\nThe current session will be deleted.",
            
            ["PowerScoring_Confirm_SavedSession_Title"] = "Saved Session Found",
            ["PowerScoring_Confirm_SavedSession_Message"] = "A saved PowerScoring session was found.\n\nDo you want to continue this session?",
            
            ["PowerScoring_Success_SessionLoaded"] = "Session Loaded",
            ["PowerScoring_Success_NewSession"] = "New Session",
            ["PowerScoring_Success_NewSessionCreated"] = "New PowerScoring session created.",
            
            ["PowerScoring_Error_NoPlayers"] = "No Players",
            ["PowerScoring_Error_AddPlayers"] = "Please add at least one player.",
            ["PowerScoring_Error_PlayerExists"] = "Duplicate",
            ["PowerScoring_Error_PlayerExistsMessage"] = "Player already exists.",
            
            ["PowerScoring_Warning_NoClasses"] = "No Classes Selected",
            ["PowerScoring_Warning_SelectClasses"] = "Please select at least one class.",
            
            ["PowerScoring_Warning_NoPlayersToPrint"] = "There are no players to print QR codes for.",
            ["PowerScoring_Info_PrintQRCodes"] = "Print QR Codes",
            ["PowerScoring_Info_PrintFeatureComingSoon"] = "QR Code printing feature will be implemented soon.",
            
            // =====================================
            // GENERAL TRANSLATIONS
            // =====================================
            ["Success"] = "Success",
            ["Error"] = "Error",
            ["Warning"] = "Warning",
            ["Information"] = "Information",
            ["Question"] = "Question",
            ["Yes"] = "Yes",
            ["No"] = "No",
            ["OK"] = "OK",
            ["Cancel"] = "Cancel",
            ["Close"] = "Close",
            ["Apply"] = "Apply",
        };
    }
}
