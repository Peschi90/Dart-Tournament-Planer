using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for detailed help content
/// </summary>
public class EnglishHelpContentLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Detailed Help Content
            ["HelpGeneralContent"] = "The Dart Tournament Planner helps you manage dart tournaments with up to 4 different classes (Platinum, Gold, Silver, Bronze).\n\n" +
                "• Use the tabs above to switch between classes\n" +
                "• All changes are automatically saved (if enabled)\n" +
                "• The status bar shows the current save status\n" +
                "• Language can be changed in the settings\n\n" +
                "🆕 NEW FEATURES:\n" +
                "• License system for premium features\n" +
                "• API integration for external applications\n" +
                "• Tournament Hub for real-time sharing\n" +
                "• Advanced statistics and reporting\n" +
                "• Enhanced printing capabilities",

            ["HelpTournamentSetupContent"] = "To set up a new tournament:\n\n" +
                "1. Select a tournament class (Platinum, Gold, Silver, Bronze)\n" +
                "2. Click 'Add Group' to create groups\n" +
                "3. Add players to the groups\n" +
                "4. Configure the game rules using the 'Configure Rules' button\n" +
                "5. Set the mode after the group phase (Group phase only, Finals, KO system)\n\n" +
                "Tip: At least 2 players per group are required for match generation.",

            ["HelpGroupManagementContent"] = "Group Management:\n\n" +
                "• 'Add Group': Creates a new group\n" +
                "• 'Remove Group': Deletes the selected group (warning appears)\n" +
                "• 'Add Player': Adds a player to the selected group\n" +
                "• 'Remove Player': Removes the selected player\n\n" +
                "The player list shows all players in the currently selected group.\n" +
                "Groups can be named arbitrarily and should have meaningful names.",

            ["HelpGameRulesContent"] = "Configure game rules:\n\n" +
                "• Game mode: 301, 401, or 501 points\n" +
                "• Finish mode: Single Out or Double Out\n" +
                "• Legs to win: Number of legs for a win\n" +
                "• Play with sets: Activates the set system\n" +
                "• Sets to win: Number of sets for a tournament win\n" +
                "• Match validation: Ensures correct score entry\n\n" +
                "All rules apply to all matches within a tournament class.\n" +
                "Different classes can have different rule configurations.",

            ["HelpMatchesContent"] = "Match and Result Management:\n\n" +
                "• Enter scores for both players in each leg\n" +
                "• Matches are automatically validated\n" +
                "• Winners are determined based on configured rules\n" +
                "• Results are immediately saved and synchronized\n\n" +
                "FEATURES:\n" +
                "• Real-time validation of match results\n" +
                "• Automatic advancement to next tournament phases\n" +
                "• Integration with Tournament Hub for live updates\n" +
                "• API endpoints for external score entry\n\n" +
                "Match results can be entered manually or received from external applications via API.",

            ["HelpTournamentPhasesContent"] = "Tournament Phases:\n\n" +
                "1. GROUP PHASE:\n" +
                "   • All players play against each other within their group\n" +
                "   • Rankings are calculated based on wins/losses\n" +
                "   • Points difference is used for tie-breaking\n\n" +
                "2. FINALS:\n" +
                "   • Best players from each group advance\n" +
                "   • Single elimination format\n" +
                "   • Determines overall tournament winner\n\n" +
                "3. KNOCKOUT SYSTEM:\n" +
                "   • Direct elimination tournament\n" +
                "   • Players are eliminated after losing a match\n" +
                "   • Bracket-style progression\n\n" +
                "Phase progression is automatic when all matches in the current phase are completed.",

            ["HelpMenusContent"] = "Menu Functions:\n\n" +
                "FILE MENU:\n" +
                "• New: Create a new tournament\n" +
                "• Open/Save: Load and save tournament files\n" +
                "• Print: Advanced printing with license features\n\n" +
                "VIEW MENU:\n" +
                "• Tournament Overview: Complete tournament status\n\n" +
                "API MENU:\n" +
                "• Start/Stop API: Control REST API server\n" +
                "• API Documentation: Open API documentation\n\n" +
                "TOURNAMENT HUB MENU:\n" +
                "• Register/Unregister: Connect to Tournament Hub\n" +
                "• Show Join URL: Share tournament access\n" +
                "• Manual Sync: Force synchronization\n\n" +
                "LICENSE MENU:\n" +
                "• License Status: View current license information\n" +
                "• Activate License: Enter license key\n" +
                "• Purchase License: Request new license",

            ["HelpLicenseSystemContent"] = "🔑 LICENSE SYSTEM\n\n" +
                "The application includes a comprehensive license system for premium features:\n\n" +
                "CORE FEATURES (Always Free):\n" +
                "• Basic tournament management\n" +
                "• Player and group management\n" +
                "• Match result entry\n" +
                "• Basic printing\n" +
                "• Standard statistics\n\n" +
                "PREMIUM FEATURES (License Required):\n" +
                "• 🌐 API Integration - REST API for external apps\n" +
                "• 🎯 Tournament Hub - Real-time tournament sharing\n" +
                "• 📈 Advanced Statistics - Detailed player analytics\n" +
                "• 🖨️ Enhanced Printing - Professional tournament reports\n" +
                "• 📊 Advanced Reporting - Export and analysis tools\n\n" +
                "LICENSE MANAGEMENT:\n" +
                "• License → License Status: View current license info\n" +
                "• License → Activate License: Enter license key\n" +
                "• License → Purchase License: Request new license\n" +
                "• Hardware-based licensing ensures security\n\n" +
                "LICENSE TYPES:\n" +
                "• Personal: Single computer activation\n" +
                "• Professional: Up to 5 computer activations\n" +
                "• Enterprise: Up to 10 computer activations\n" +
                "• Custom: Contact for specific requirements\n\n" +
                "Without a license, the application gracefully falls back to core functionality.",

            ["HelpApiIntegrationContent"] = "🌐 API INTEGRATION\n\n" +
                "The REST API provides programmatic access to tournament data:\n\n" +
                "GETTING STARTED:\n" +
                "1. Ensure you have an active license\n" +
                "2. Use API → Start API to launch the server\n" +
                "3. Access API documentation at the provided URL\n" +
                "4. Use API endpoints to interact with tournament data\n\n" +
                "AVAILABLE ENDPOINTS:\n" +
                "• GET /api/tournaments - List all tournaments\n" +
                "• GET /api/tournaments/{id} - Get specific tournament\n" +
                "• GET /api/tournaments/{id}/matches - Get tournament matches\n" +
                "• POST /api/tournaments/{id}/matches/{matchId}/result - Submit match result\n" +
                "• GET /api/tournaments/{id}/statistics - Get tournament statistics\n\n" +
                "FEATURES:\n" +
                "• Real-time match result submission\n" +
                "• Live tournament data access\n" +
                "• JSON-based RESTful interface\n" +
                "• Automatic validation and error handling\n" +
                "• CORS support for web applications\n\n" +
                "USE CASES:\n" +
                "• External scoring applications\n" +
                "• Tournament websites and displays\n" +
                "• Mobile companion apps\n" +
                "• Integration with dart board systems\n" +
                "• Custom reporting tools\n\n" +
                "The API server runs locally and can be accessed by applications on the same network.",

            ["HelpTournamentHubContent"] = "🎯 TOURNAMENT HUB\n\n" +
                "The Tournament Hub enables real-time tournament sharing and collaboration:\n\n" +
                "SETUP:\n" +
                "1. Ensure you have an active license with Hub features\n" +
                "2. Tournament Hub → Register with Hub\n" +
                "3. Share the provided Join URL with participants\n" +
                "4. Tournament data is synchronized in real-time\n\n" +
                "FEATURES:\n" +
                "• 📡 Real-time WebSocket synchronization\n" +
                "• 📱 Shareable tournament URLs\n" +
                "• 🔄 Automatic data synchronization\n" +
                "• 👥 Multi-device access\n" +
                "• 📊 Live tournament viewing\n" +
                "• 🎮 Remote match result submission\n\n" +
                "HUB FUNCTIONS:\n" +
                "• Register with Hub: Connect tournament to hub server\n" +
                "• Show Join URL: Get shareable tournament link\n" +
                "• Manual Sync: Force synchronization\n" +
                "• Unregister from Hub: Disconnect tournament\n" +
                "• Hub Settings: Configure hub server URL\n\n" +
                "STATUS INDICATORS:\n" +
                "• 🟢 Green: Connected and synchronized\n" +
                "• 🔴 Red: Disconnected or error\n" +
                "• 🟡 Yellow: Connecting or sync in progress\n\n" +
                "BENEFITS:\n" +
                "• Spectators can follow tournaments live\n" +
                "• Multiple officials can manage the same tournament\n" +
                "• Remote score entry from mobile devices\n" +
                "• Backup and redundancy through cloud sync\n" +
                "• Professional tournament presentation\n\n" +
                "The Hub requires an internet connection and compatible hub server.",

            ["HelpStatisticsContent"] = "📈 STATISTICS\n\n" +
                "Advanced statistics provide detailed insights into player and tournament performance:\n\n" +
                "PLAYER STATISTICS:\n" +
                "• Total matches played and won\n" +
                "• Win percentage and performance trends\n" +
                "• Average scores and checkout statistics\n" +
                "• Head-to-head records\n" +
                "• Performance by tournament class\n" +
                "• Improvement tracking over time\n\n" +
                "TOURNAMENT STATISTICS:\n" +
                "• Overall tournament progress\n" +
                "• Match completion rates\n" +
                "• Class-specific performance data\n" +
                "• Group standings and rankings\n" +
                "• Phase progression statistics\n\n" +
                "ADVANCED FEATURES (License Required):\n" +
                "• 📊 Detailed performance analytics\n" +
                "• 📈 Graphical trend analysis\n" +
                "• 🎯 Accuracy and consistency metrics\n" +
                "• 🏆 Achievement tracking\n" +
                "• 📋 Exportable reports\n" +
                "• 📱 Mobile-friendly statistics views\n\n" +
                "ACCESSING STATISTICS:\n" +
                "• Click on any player in the player list\n" +
                "• Use the Statistics tab (license required)\n" +
                "• View through Tournament Overview\n" +
                "• Export via API endpoints\n\n" +
                "Statistics are updated in real-time as matches are completed and provide valuable insights for players and tournament organizers.",

            ["HelpPrintingContent"] = "🖨️ PRINTING\n\n" +
                "The application offers comprehensive printing capabilities for tournament documentation:\n\n" +
                "BASIC PRINTING (Always Available):\n" +
                "• Tournament bracket overview\n" +
                "• Group standings\n" +
                "• Match schedules\n" +
                "• Basic player lists\n\n" +
                "ENHANCED PRINTING (License Required):\n" +
                "• 📄 Professional tournament reports\n" +
                "• 📊 Statistical summaries\n" +
                "• 🏆 Championship certificates\n" +
                "• 📋 Detailed match histories\n" +
                "• 🎨 Custom formatting and branding\n" +
                "• 📱 Mobile-optimized print layouts\n\n" +
                "PRINTING OPTIONS:\n" +
                "• File → Print to access print dialog\n" +
                "• Select specific tournament classes\n" +
                "• Choose print content and format\n" +
                "• Preview before printing\n" +
                "• Export to PDF (license required)\n\n" +
                "PRINT CONTENT:\n" +
                "• Tournament overview and structure\n" +
                "• Complete group standings\n" +
                "• Match results and schedules\n" +
                "• Player statistics and rankings\n" +
                "• Tournament rules and information\n\n" +
                "PROFESSIONAL FEATURES:\n" +
                "• Custom headers and logos\n" +
                "• Multiple format options\n" +
                "• Batch printing capabilities\n" +
                "• High-quality PDF generation\n" +
                "• Print job optimization\n\n" +
                "Enhanced printing features require an active license and provide professional-quality tournament documentation suitable for official events.",

            ["HelpTipsContent"] = "💡 TIPS & TRICKS\n\n" +
                "GENERAL TIPS:\n" +
                "• Enable Auto-Save in settings to prevent data loss\n" +
                "• Use meaningful group and player names\n" +
                "• Check tournament rules before starting matches\n" +
                "• Regularly sync with Tournament Hub if connected\n\n" +
                "EFFICIENCY TIPS:\n" +
                "• Use keyboard shortcuts where available\n" +
                "• Batch-add players using copy-paste\n" +
                "• Set up templates for recurring tournaments\n" +
                "• Use the Tournament Overview for quick status checks\n\n" +
                "ADVANCED FEATURES:\n" +
                "• Press Shift+Click on Print for debug information\n" +
                "• Click on Hub status indicator for debug console\n" +
                "• Use API integration for automated scoring\n" +
                "• Leverage Tournament Hub for remote management\n\n" +
                "TROUBLESHOOTING:\n" +
                "• Check internet connection for Hub/API features\n" +
                "• Verify license status for premium features\n" +
                "• Use Help → Report Bug for issues\n" +
                "• Check the debug console for technical information\n\n" +
                "LICENSE OPTIMIZATION:\n" +
                "• Consider Professional license for multiple computers\n" +
                "• Use Enterprise license for tournament organizations\n" +
                "• Contact support for custom licensing needs\n" +
                "• Premium features significantly enhance tournament management\n\n" +
                "SUPPORT:\n" +
                "• Use Help → Report Bug to report issues\n" +
                "• Include system information in bug reports\n" +
                "• Check the GitHub repository for updates\n" +
                "• Consider supporting development through donations"
        };
    }
}