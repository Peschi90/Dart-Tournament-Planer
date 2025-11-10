using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for UI elements and general user interface
/// </summary>
public class EnglishUILanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // =====================================
            // APPLICATION CORE TRANSLATIONS
            // =====================================
            
            // Application settings
            ["Settings"] = "Settings",
            ["Language"] = "Language",
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
            ["View"] = "View",
            ["Help"] = "Help",
            ["About"] = "About",
            
            // Status displays
            ["HasUnsavedChanges"] = "Modified",
            ["NotSaved"] = "Not saved",
            ["Saved"] = "Saved",
            
            // General UI elements
            ["Close"] = "Close",
            ["OK"] = "OK",
            ["Start"] = "Start",
            ["Stop"] = "Stop",
            ["Player"] = "Player",
            ["Match"] = "Match",
            ["Result"] = "Result",
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

            // Donation and Bug Report Functions
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
            ["Bronze"] = "Bronze",
            
            // =====================================
            // ABOUT DIALOG
            // =====================================

            ["About"] = "About",
            ["AboutDescription"] = "A comprehensive and modern application for organizing and managing dart tournaments. " +
                "Supports multiple tournament formats including group stages, finals, and knockout phases. " +
                "Features real-time match tracking, detailed player statistics, tournament hub integration via WebSocket, " +
                "QR code generation for easy mobile access, and professional tournament documentation with print functionality.",
            ["AboutCredits"] = "Special thanks to all contributors, testers, and the dart community for their valuable feedback and support. " +
                "This project is continuously improved through community engagement and feedback.",
            ["Developer"] = "Developer",
            ["DeveloperName"] = "Marcel Peschka",
            ["Framework"] = "Framework",
            ["License"] = "License",
            ["LicenseType"] = "Open Source (MIT License)",
            ["Website"] = "Website",
            ["WebsiteUrl"] = "https://github.com/Peschi90/Dart-Turnament-Planer",
            ["OpenSource"] = "Open Source",
            ["SpecialThanks"] = "Special Thanks",
            ["AppInformation"] = "Application Information",
            ["TechnicalDetails"] = "Technical Details",
            ["Features"] = "Features",
            ["FeatureList"] = "• Multiple tournament formats (Group, Finals, Knockout)\n" +
                "• Real-time match tracking and score management\n" +
                "• Comprehensive player statistics\n" +
                "• WebSocket-based Tournament Hub integration\n" +
                "• QR code generation for mobile access\n" +
                "• Professional print functionality\n" +
                "• Multi-language support (English/German)\n" +
                "• Dark/Light theme support\n" +
                "• Auto-save and data persistence",
            ["ContactSupport"] = "Support & Contact",
            ["GitHubRepository"] = "GitHub Repository",
            ["ReportIssue"] = "Report Issue",
            ["VersionInfo"] = "Version Information",

            // =====================================
            // THEME & DARK MODE
            // =====================================
            
            ["Theme"] = "Theme",
            ["DarkMode"] = "Dark Mode",
            ["LightMode"] = "Light Mode",
            ["SwitchToDarkMode"] = "Switch to Dark Mode",
            ["SwitchToLightMode"] = "Switch to Light Mode",
            ["ThemeSettings"] = "Theme Settings",
            
            // =====================================
            // GENERAL UI ELEMENTS
            // =====================================
            
            // Basic Buttons
            ["OK"] = "OK",
            ["Cancel"] = "Cancel",
            ["Save"] = "Save",
            ["Load"] = "Load",
            ["Delete"] = "Delete",
            ["Edit"] = "Edit",
            ["Add"] = "Add",
            ["Remove"] = "Remove",
            ["Close"] = "Close",
            ["Apply"] = "Apply",
            ["Reset"] = "Reset",
            ["Refresh"] = "Refresh",
            ["Clear"] = "Clear",
            ["Copy"] = "Copy",
            ["Paste"] = "Paste",
            ["Cut"] = "Cut",
            ["Undo"] = "Undo",
            ["Redo"] = "Redo",
            
            // Navigation
            ["Next"] = "Next",
            ["Previous"] = "Previous",
            ["First"] = "First",
            ["Last"] = "Last",
            ["Back"] = "Back",
            ["Forward"] = "Forward",
            ["Up"] = "Up",
            ["Down"] = "Down",
            ["Left"] = "Left",
            ["Right"] = "Right",
            
            // Status Terms
            ["Success"] = "Success",
            ["Error"] = "Error",
            ["Warning"] = "Warning",
            ["Information"] = "Information",
            ["Loading"] = "Loading...",
            ["Saving"] = "Saving...",
            ["Processing"] = "Processing...",
            ["Complete"] = "Complete",
            ["Failed"] = "Failed",
            ["Ready"] = "Ready",
            ["Busy"] = "Busy",
            
            // =====================================
            // DIALOG & WINDOW ELEMENTS
            // =====================================
            
            // Window Title Suffixes
            ["Settings"] = "Settings",
            ["Configuration"] = "Configuration",
            ["Properties"] = "Properties",
            ["Options"] = "Options",
            ["Preferences"] = "Preferences",
            
            // Dialog Type Terms
            ["Dialog"] = "Dialog",
            ["Window"] = "Window",
            ["Form"] = "Form",
            ["Wizard"] = "Wizard",
            ["Setup"] = "Setup",
            
            // =====================================
            // DATA ELEMENTS
            // =====================================
            
            // Tables & Lists
            ["Name"] = "Name",
            ["Value"] = "Value",
            ["Type"] = "Type",
            ["Status"] = "Status",
            ["Date"] = "Date",
            ["Time"] = "Time",
            ["Duration"] = "Duration",
            ["Count"] = "Count",
            ["Total"] = "Total",
            ["Average"] = "Average",
            ["Minimum"] = "Minimum",
            ["Maximum"] = "Maximum",
            
            // Sorting & Filtering
            ["Sort"] = "Sort",
            ["SortBy"] = "Sort by",
            ["Filter"] = "Filter",
            ["FilterBy"] = "Filter by",
            ["Search"] = "Search",
            ["SearchFor"] = "Search for",
            ["Results"] = "Results",
            ["NoResults"] = "No results",
            
            // =====================================
            // FORM ELEMENTS
            // =====================================
            
            // Input Labels
            ["Required"] = "Required",
            ["Optional"] = "Optional",
            ["Default"] = "Default",
            ["Custom"] = "Custom",
            ["Auto"] = "Auto",
            ["Manual"] = "Manual",
            
            // Validation
            ["Valid"] = "Valid",
            ["Invalid"] = "Invalid",
            ["ValidationError"] = "Validation Error",
            ["RequiredField"] = "Required Field",
            ["InvalidFormat"] = "Invalid Format",
            ["ValueTooSmall"] = "Value too small",
            ["ValueTooLarge"] = "Value too large",
            
            // =====================================
            // LANGUAGE-SPECIFIC UI
            // =====================================
            
            ["Language"] = "Language",
            ["LanguageSettings"] = "Language Settings",
            ["SelectLanguage"] = "Select Language",
            ["LanguageChanged"] = "Language Changed",
            ["LanguageChangeRestart"] = "The application needs to be restarted for the language change to take effect.",
            
            // =====================================
            // SYSTEM & DEBUG
            // =====================================
            
            ["System"] = "System",
            ["Debug"] = "Debug",
            ["Log"] = "Log",
            ["Version"] = "Version",
            ["Build"] = "Build",
            ["Platform"] = "Platform",
            ["Memory"] = "Memory",
            ["Performance"] = "Performance",
            
            // =====================================
            // ACCESSIBILITY
            // =====================================
            
            ["AccessibilityMode"] = "Accessibility",
            ["HighContrast"] = "High Contrast",
            ["LargeText"] = "Large Text",
            ["ScreenReader"] = "Screen Reader",
            ["KeyboardNavigation"] = "Keyboard Navigation",
            
            // =====================================
            // FILE & I/O
            // =====================================
            
            ["File"] = "File",
            ["Folder"] = "Folder",
            ["Path"] = "Path",
            ["Size"] = "Size",
            ["Modified"] = "Modified",
            ["Created"] = "Created",
            ["Exists"] = "Exists",
            ["NotFound"] = "Not Found",
            ["ReadOnly"] = "Read Only",
            ["Permission"] = "Permission",
            
            // =====================================
            // NETWORK & CONNECTION
            // =====================================
            
            ["Connected"] = "Connected",
            ["Disconnected"] = "Disconnected",
            ["Connecting"] = "Connecting...",
            ["Connection"] = "Connection",
            ["Network"] = "Network",
            ["Offline"] = "Offline",
            ["Online"] = "Online",
            ["Timeout"] = "Timeout",
            ["Retry"] = "Retry",
            
            // =====================================
            // PRINTING
            // =====================================
            
            ["Print"] = "Print",
            ["PrintPreview"] = "Print Preview",
            ["PrintSettings"] = "Print Settings",
            ["Printer"] = "Printer",
            ["Page"] = "Page",
            ["Pages"] = "Pages",
            ["Copies"] = "Copies",
            ["Quality"] = "Quality",
            ["Orientation"] = "Orientation",
            ["Portrait"] = "Portrait",
            ["Landscape"] = "Landscape",
            
            // =====================================
            // EXPORT & IMPORT
            // =====================================
            
            ["Export"] = "Export",
            ["Import"] = "Import",
            ["Format"] = "Format",
            ["Destination"] = "Destination",
            ["Source"] = "Source",
            ["Progress"] = "Progress",
            ["Completed"] = "Completed",
            ["Cancelled"] = "Cancelled"
        };
    }
}