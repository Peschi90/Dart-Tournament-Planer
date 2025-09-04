using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for UI elements and general application components
/// </summary>
public class EnglishUILanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Application settings
            ["Settings"] = "Settings",
            ["Language"] = "Language",
            ["Theme"] = "Theme",
            ["AutoSave"] = "Auto Save",
            ["AutoSaveInterval"] = "Save Interval (minutes)",
            ["Save"] = "Save",
            ["Cancel"] = "Cancel",

            // Menu entries
            ["File"] = "File",
            ["New"] = "New",
            ["Open"] = "Open",
            ["SaveAs"] = "Save As",
            ["Print"] = "Print",
            ["Exit"] = "Exit",
            ["Edit"] = "Edit",
            ["View"] = "View",
            ["Help"] = "Help",
            ["About"] = "About",

            // Status displays
            ["HasUnsavedChanges"] = "Modified",
            ["NotSaved"] = "Not saved",
            ["Saved"] = "Saved",
            ["Ready"] = "Ready",

            // General UI elements
            ["Close"] = "Close",
            ["OK"] = "OK",
            ["Start"] = "Start",
            ["Stop"] = "Stop",
            ["Player"] = "Player",
            ["Match"] = "Match",
            ["Result"] = "Result",
            ["Status"] = "Status",
            ["Position"] = "Position",
            ["Winner"] = "Winner",
            ["Information"] = "Information",
            ["Warning"] = "Warning",
            ["Error"] = "Error",

            // Input dialog
            ["InputDialog"] = "Input",
            ["EnterName"] = "Enter name:",

            // Loading Spinner
            ["Loading"] = "Loading...",
            ["CheckingGroupStatus"] = "Checking group status...",
            ["ProcessingMatches"] = "Processing matches...",
            ["CheckingCompletion"] = "Checking completion...",

            // Startup and update functions
            ["StartingApplication"] = "Starting application...",
            ["AppSubtitle"] = "Modern tournament management",
            ["CheckingForUpdates"] = "Checking for updates...",
            ["ConnectingToGitHub"] = "Connecting to GitHub...",
            ["AnalyzingReleases"] = "Analyzing releases...",
            ["UpdateAvailable"] = "Update available",
            ["WhatsNew"] = "What's new:",
            ["RemindLater"] = "Remind later",
            ["SkipVersion"] = "Skip version",
            ["DownloadUpdate"] = "Download now",
            ["DownloadAndInstall"] = "Download & Install",
            ["DownloadingUpdate"] = "Downloading update",
            ["PreparingDownload"] = "Preparing download...",
            ["DownloadingSetup"] = "Downloading setup...",
            ["DownloadCompleted"] = "Download completed, checking file...",
            ["PreparingInstallation"] = "Preparing installation...",
            ["StartingInstallation"] = "Starting installation...",
            ["InstallationStarted"] = "Installation started",
            ["InstallationCancelled"] = "Installation cancelled",
            ["ErrorStartingSetup"] = "Error starting setup",
            ["AdminRightsRequired"] = "Administrator rights required",
            ["NoUpdateAvailable"] = "No updates available",

            // Overview configuration dialog
            ["OverviewConfiguration"] = "Overview Configuration",
            ["TournamentOverviewConfiguration"] = "Tournament Overview Configuration",
            ["TimeBetweenClasses"] = "Time between tournament classes:",
            ["TimeBetweenSubTabs"] = "Time between sub-tabs:",
            ["Seconds"] = "Seconds",
            ["ShowOnlyActiveClassesText"] = "Show only classes with active groups",
            ["OverviewInfoText"] = "Live tournament display for all classes with automatic switching",
            ["InvalidClassInterval"] = "Invalid class interval. Please enter a number ≥ 1.",
            ["InvalidSubTabInterval"] = "Invalid sub-tab interval. Please enter a number ≥ 1.",

            // Tournament overview texts
            ["TournamentName"] = "⚽ Tournament:",
            ["CurrentPhase"] = "🏁 Current Phase:",
            ["GroupsCount"] = "👥 Groups:",
            ["PlayersTotal"] = "👤 Total Players:",
            ["GameRulesColon"] = "📋 Game Rules:",
            ["CompletedGroups"] = "✅ Completed Groups:",
            ["QualifiedPlayers"] = "🎯 Qualified Players:",
            ["KnockoutMatches"] = "🏆 K.O. Matches:",
            ["Completed"] = "completed",

            // Other hardcoded texts
            ["Finalists"] = "Finalists",
            ["KnockoutParticipants"] = "K.O. Participants",
            ["PlayersText"] = "Players",
            ["OverviewModeTitle"] = "Tournament Overview Mode",
            ["NewTournament"] = "New Tournament",
            ["CreateNewTournament"] = "Create new tournament? Unsaved changes will be lost.",
            ["UnsavedChanges"] = "Unsaved Changes",
            ["SaveBeforeExit"] = "There are unsaved changes. Do you want to save before exiting?",
            ["CustomFileNotImplemented"] = "Custom file loading not yet implemented.",
            ["CustomFileSaveNotImplemented"] = "Custom file saving not yet implemented.",
            ["ErrorOpeningHelp"] = "Error opening help:",
            ["ErrorOpeningOverview"] = "Error opening tournament overview:",
            ["ErrorSavingData"] = "Error saving data:",

            // Donation and Bug Report functions
            ["Donate"] = "💝",
            ["DonateTooltip"] = "Support the development of this project",
            ["BugReport"] = "🐛 Report Bug",
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

            // Main window
            ["AppTitle"] = "Dart Tournament Planner",
            ["Platinum"] = "Platinum",
            ["Gold"] = "Gold",
            ["Silver"] = "Silver",
            ["Bronze"] = "Bronze"
        };
    }
}