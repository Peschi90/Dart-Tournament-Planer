using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.English;

/// <summary>
/// English translations for Tournament Hub functionality
/// </summary>
public class EnglishHubLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Tournament Hub Menu
            ["TournamentHub"] = "Tournament Hub",
            ["RegisterWithHub"] = "Register with Hub",
            ["UnregisterFromHub"] = "Unregister from Hub",
            ["ShowJoinUrl"] = "Show Join URL",
            ["ManualSync"] = "Manual Sync",
            ["HubSettings"] = "Hub Settings",

            // Hub Status Translations
            ["HubStatus"] = "Hub Status",
            ["HubConnected"] = "Connected",
            ["HubDisconnected"] = "Disconnected",
            ["HubConnecting"] = "Connecting...",
            ["HubReconnecting"] = "Reconnecting...",
            ["HubError"] = "Error",
            ["HubSyncing"] = "Syncing...",
            ["HubSyncComplete"] = "Sync Complete",
            ["HubWebSocket"] = "WebSocket",
            ["HubHTTP"] = "HTTP",

            // Hub Registration and Management
            ["RegisterTournamentTitle"] = "Register Tournament with Hub",
            ["RegisterTournamentSuccess"] = "🎯 Tournament successfully registered with Hub!\n\nTournament ID: {0}\nJoin URL: {1}\n\nYou can send this URL to players.",
            ["RegisterTournamentError"] = "❌ Tournament could not be registered with Hub.",
            ["UnregisterTournamentTitle"] = "Unregister Tournament",
            ["UnregisterTournamentConfirm"] = "Really unregister tournament '{0}' from Hub?",
            ["UnregisterTournamentSuccess"] = "Tournament successfully unregistered from Hub.",
            ["UnregisterTournamentError"] = "Error unregistering: {0}",
            ["NoTournamentRegistered"] = "No tournament registered with Hub.",

            // Hub Registration Success Dialog
            ["TournamentRegisteredTitle"] = "Tournament Successfully Registered",
            ["TournamentRegisteredMessage"] = "Your tournament has been successfully registered with the Tournament Hub!",
            ["TournamentInformation"] = "🏆 Tournament Information",
            ["ActiveFeatures"] = "🌟 Active Features",
            ["TournamentFeaturesText"] = "- Real-time tournament synchronization\n- Multi-device tournament access\n- Live match result updates",
            ["JoinUrlCopiedText"] = "The join URL has been copied to your clipboard automatically.",
            ["CopyUrl"] = "Copy URL",
            ["OK"] = "OK",
            ["Copied"] = "Copied!",
            ["Error"] = "Error",
            ["CopyError"] = "Error copying URL.",

            // Hub Synchronization
            ["SyncWithHub"] = "Sync with Hub",
            ["SyncSuccess"] = "Tournament successfully synced with Hub!",
            ["SyncError"] = "Error syncing with Hub.",
            ["ManualSyncError"] = "Error in manual sync: {0}",
            ["AutoSyncEnabled"] = "Automatic synchronization enabled",
            ["AutoSyncDisabled"] = "Automatic synchronization disabled",

            // Join URL Functions
            ["JoinUrlTitle"] = "Tournament Join URL",
            ["JoinUrlMessage"] = "Tournament ID: {0}\n\nJoin URL:\n{1}\n\nYou can send this URL to players.",
            ["JoinUrlError"] = "Error showing Join URL: {0}",
            ["JoinUrlCopied"] = "Join URL copied to clipboard",

            // Hub Settings
            ["HubSettingsTitle"] = "Hub Settings",
            ["HubSettingsPrompt"] = "Enter the Tournament Hub URL:",
            ["HubUrlUpdated"] = "Hub URL updated:\n{0}",
            ["HubSettingsError"] = "Error in Hub settings: {0}",
            ["InvalidHubUrl"] = "Invalid Hub URL. Please enter a complete URL.",

            // WebSocket Connection
            ["WebSocketConnecting"] = "Establishing WebSocket connection...",
            ["WebSocketConnected"] = "WebSocket connection established",
            ["WebSocketDisconnected"] = "WebSocket connection disconnected",
            ["WebSocketError"] = "WebSocket error: {0}",
            ["WebSocketReconnecting"] = "WebSocket reconnecting...",
            ["WebSocketReconnected"] = "WebSocket reconnection successful",
            ["WebSocketMaxRetriesReached"] = "Maximum WebSocket connection attempts reached",

            // Match Updates from Hub
            ["MatchUpdateReceived"] = "Match update received",
            ["MatchUpdateProcessed"] = "Match {0} successfully updated",
            ["MatchUpdateError"] = "Error processing match update: {0}",
            ["MatchResultFromHub"] = "Match result received from Hub",
            ["InvalidMatchUpdate"] = "Invalid match update received",

            // Tournament Data Sync
            ["TournamentDataSyncing"] = "Syncing tournament data...",
            ["TournamentDataSynced"] = "Tournament data successfully synced",
            ["TournamentDataSyncError"] = "Error in tournament data synchronization: {0}",
            ["SendingTournamentData"] = "Sending tournament data...",
            ["TournamentDataSent"] = "Tournament data successfully sent",

            // Hub Debug Console
            ["HubDebugConsole"] = "Tournament Hub Debug Console",
            ["DebugConsoleTitle"] = "Hub Debug Console",
            ["DebugConsoleReady"] = "Ready for debugging...",
            ["DebugConsoleStarted"] = "Tournament Hub Debug Console started",
            ["DebugConsoleClear"] = "Clear Debug Console",
            ["DebugConsoleClearConfirm"] = "Do you want to clear all debug messages?",
            ["DebugConsoleCleared"] = "Debug Console cleared",
            ["DebugConsoleSave"] = "Save Debug Log",
            ["DebugConsoleSaved"] = "Debug Log saved to: {0}",
            ["DebugConsoleSaveError"] = "Error saving: {0}",
            ["DebugConsoleClose"] = "Close",
            ["AutoScrollEnabled"] = "Auto-scroll enabled",
            ["AutoScrollDisabled"] = "Auto-scroll disabled",
            ["MessagesCount"] = "Messages: {0}",

            // Hub Connection Status Details
            ["ConnectionStatusUpdated"] = "Connection status updated",
            ["HubServiceStatus"] = "Hub Service Status",
            ["LastSyncTime"] = "Last sync: {0}",
            ["NextSyncIn"] = "Next sync in: {0}",
            ["ConnectionQuality"] = "Connection Quality",
            ["ConnectionStable"] = "Stable",
            ["ConnectionUnstable"] = "Unstable",
            ["ConnectionPoor"] = "Poor",

            // Hub Heartbeat
            ["HeartbeatSent"] = "Heartbeat sent",
            ["HeartbeatReceived"] = "Heartbeat received",
            ["HeartbeatError"] = "Heartbeat error: {0}",
            ["HeartbeatTimeout"] = "Heartbeat timeout",

            // Tournament ID and Client Info
            ["TournamentId"] = "Tournament ID",
            ["ClientType"] = "Client Type",
            ["TournamentPlanner"] = "Tournament Planner",
            ["ClientVersion"] = "Client Version",
            ["ConnectedAt"] = "Connected since",
            ["ClientId"] = "Client ID",

            // Subscription Management
            ["SubscribingToTournament"] = "Subscribing to tournament updates...",
            ["SubscribedToTournament"] = "Tournament updates successfully subscribed",
            ["UnsubscribingFromTournament"] = "Unsubscribing from tournament updates...",
            ["UnsubscribedFromTournament"] = "Tournament updates subscription successfully cancelled",
            ["SubscriptionError"] = "Subscription error: {0}",

            // Hub Service Messages
            ["HubServiceStarted"] = "Hub Service started",
            ["HubServiceStopped"] = "Hub Service stopped",
            ["HubServiceInitialized"] = "Hub Service initialized",
            ["HubServiceError"] = "Hub Service error: {0}",
            ["HubServiceRestarting"] = "Hub Service restarting...",

            // Network and Connection
            ["NetworkError"] = "Network error: {0}",
            ["ConnectionTimeout"] = "Connection timeout",
            ["ConnectionRefused"] = "Connection refused",
            ["ServerNotReachable"] = "Server not reachable",
            ["InternetConnectionRequired"] = "Internet connection required",

            // Tournament Hub URL Validation
            ["ValidatingHubUrl"] = "Validating Hub URL...",
            ["HubUrlValid"] = "Hub URL is valid",
            ["HubUrlInvalid"] = "Hub URL is invalid",
            ["HubUrlNotReachable"] = "Hub URL not reachable",
            ["DefaultHubUrl"] = "Use default Hub URL",

            // Status Bar Messages for Hub
            ["HubStatusConnected"] = "Hub: Connected",
            ["HubStatusDisconnected"] = "Hub: Disconnected",
            ["HubStatusConnecting"] = "Hub: Connecting...",
            ["HubStatusError"] = "Hub: Error",
            ["HubStatusSyncing"] = "Hub: Syncing...",
            ["HubStatusReady"] = "Hub: Ready",

            // Tournament Registration Details
            ["GeneratingTournamentId"] = "Generating tournament ID...",
            ["TournamentIdGenerated"] = "Tournament ID generated: {0}",
            ["RegisteringWithServer"] = "Registering with server...",
            ["ServerRegistrationComplete"] = "Server registration complete",
            ["ObtainingJoinUrl"] = "Obtaining join URL...",
            ["JoinUrlObtained"] = "Join URL obtained: {0}",

            // Error Categories for Debug Console
            ["InfoMessage"] = "Info",
            ["WarningMessage"] = "Warning",
            ["ErrorMessage"] = "Error",
            ["SuccessMessage"] = "Success",
            ["WebSocketMessage"] = "WebSocket",
            ["SyncMessage"] = "Sync",
            ["TournamentMessage"] = "Tournament",
            ["MatchMessage"] = "Match",
            ["MatchResultMessage"] = "Match Result",

            // Advanced Hub Features
            ["HubStatistics"] = "Hub Statistics",
            ["ConnectedClients"] = "Connected Clients: {0}",
            ["ActiveTournaments"] = "Active Tournaments: {0}",
            ["TotalMatches"] = "Total Matches: {0}",
            ["DataTransferred"] = "Data Transferred: {0}",
            ["UptimeInfo"] = "Uptime: {0}",

            // Hub Configuration
            ["HubConfiguration"] = "Hub Configuration",
            ["AutoReconnect"] = "Auto Reconnect",
            ["ReconnectInterval"] = "Reconnect Interval",
            ["MaxReconnectAttempts"] = "Max Reconnect Attempts",
            ["SyncInterval"] = "Sync Interval",
            ["HeartbeatInterval"] = "Heartbeat Interval",

            // Feature Flags and Capabilities
            ["HubFeatures"] = "Hub Features",
            ["RealTimeUpdates"] = "Real-time Updates",
            ["MatchStreaming"] = "Match Streaming",
            ["StatisticsSync"] = "Statistics Sync",
            ["MultiDeviceSupport"] = "Multi-Device Support",
            ["OfflineMode"] = "Offline Mode",

            // User Experience Messages
            ["PleaseWait"] = "Please wait...",
            ["ProcessingRequest"] = "Processing request...",
            ["AlmostDone"] = "Almost done...",
            ["OperationCompleted"] = "Operation completed",
            ["OperationCancelled"] = "Operation cancelled",
            ["TryAgain"] = "Try again",
            ["CheckConnection"] = "Check connection",

            // Tournament Hub Service Lifecycle
            ["ServiceInitializing"] = "Service initializing...",
            ["ServiceReady"] = "Service ready",
            ["ServiceShuttingDown"] = "Service shutting down...",
            ["ServiceShutdown"] = "Service shutdown",
            ["ServiceRestarted"] = "Service restarted",
            ["ServiceHealthy"] = "Service is healthy",
            ["ServiceUnhealthy"] = "Service is unhealthy",

            // Hub License Control
            ["HubLicenseRequiredTitle"] = "Hub Connection License Required",
            ["HubLicenseRequiredMessage"] = "Tournament Hub connection requires a valid license with the 'Hub Connection' feature.",
            ["HubBenefitsTitle"] = "Hub Connection features include:",
            ["HubBenefits"] = "- Real-time tournament synchronization\n- Multi-Device tournament management\n- Live match result updates\n- QR-Code sharing for easy access\n- Automatic data backup and sync",
            ["MultiDeviceManagement"] = "Multi-Device Tournament Management",
            ["MultiDeviceManagementText"] = "Hub connections enable seamless tournament management across multiple devices with real-time synchronization.",
            ["HubLicenseActionText"] = "Would you like to request a license with Hub Connection features?",
            ["RequestHubLicense"] = "Request Hub License"
        };
    }
}