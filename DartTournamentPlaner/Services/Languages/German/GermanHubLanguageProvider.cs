﻿using System.Collections.Generic;

namespace DartTournamentPlaner.Services.Languages.German;

/// <summary>
/// Deutsche Übersetzungen für Tournament Hub Funktionalität
/// </summary>
public class GermanHubLanguageProvider : ILanguageSection
{
    public Dictionary<string, string> GetSectionTranslations()
    {
        return new Dictionary<string, string>
        {
            // Tournament Hub Menü
            ["TournamentHub"] = "Tournament Hub",
            ["RegisterWithHub"] = "Bei Hub registrieren",
            ["UnregisterFromHub"] = "Vom Hub entregistrieren",
            ["ShowJoinUrl"] = "Join-URL anzeigen",
            ["ManualSync"] = "Manuell synchronisieren",
            ["HubSettings"] = "Hub-Einstellungen",

            // Hub Status Übersetzungen
            ["HubStatus"] = "Hub Status",
            ["HubConnected"] = "Verbunden",
            ["HubDisconnected"] = "Getrennt",
            ["HubConnecting"] = "Verbinde...",
            ["HubReconnecting"] = "Wiederverbindung...",
            ["HubError"] = "Fehler",
            ["HubSyncing"] = "Synchronisiere...",
            ["HubSyncComplete"] = "Sync abgeschlossen",
            ["HubWebSocket"] = "WebSocket",
            ["HubHTTP"] = "HTTP",

            // Hub Registrierung und Verwaltung
            ["RegisterTournamentTitle"] = "Tournament beim Hub registrieren",
            ["RegisterTournamentSuccess"] = "🎯 Tournament erfolgreich beim Hub registriert!\n\nTournament ID: {0}\nJoin URL: {1}\n\nDiese URL können Sie an Spieler senden.",
            ["RegisterTournamentError"] = "❌ Tournament konnte nicht beim Hub registriert werden.",
            ["UnregisterTournamentTitle"] = "Tournament entregistrieren",
            ["UnregisterTournamentConfirm"] = "Tournament '{0}' wirklich vom Hub entregistrieren?",
            ["UnregisterTournamentSuccess"] = "Tournament erfolgreich vom Hub entregistriert.",
            ["UnregisterTournamentError"] = "Fehler beim Entregistrieren: {0}",
            ["NoTournamentRegistered"] = "Kein Tournament beim Hub registriert.",

            // Hub Registration Success Dialog
            ["TournamentRegisteredTitle"] = "Turnier erfolgreich registriert",
            ["TournamentRegisteredMessage"] = "Ihr Turnier wurde erfolgreich beim Tournament Hub registriert!",
            ["TournamentInformation"] = "🏆 Turnier-Informationen",
            ["ActiveFeatures"] = "🌟 Aktive Features",
            ["TournamentFeaturesText"] = "- Echtzeit-Turnier-Synchronisation\n- Multi-Device Turnier-Zugang\n- Live Match-Ergebnis Updates",
            ["JoinUrlCopiedText"] = "Die Join-URL wurde automatisch in die Zwischenablage kopiert.",
            ["CopyUrl"] = "URL kopieren",
            ["OK"] = "OK",
            ["Copied"] = "Kopiert!",
            ["Error"] = "Fehler",
            ["CopyError"] = "Fehler beim Kopieren der URL.",

            // Hub Synchronisation
            ["SyncWithHub"] = "Mit Hub synchronisieren",
            ["SyncSuccess"] = "Tournament erfolgreich mit Hub synchronisiert!",
            ["SyncError"] = "Fehler beim Synchronisieren mit Hub.",
            ["ManualSyncError"] = "Fehler beim manuellen Sync: {0}",
            ["AutoSyncEnabled"] = "Automatische Synchronisation aktiviert",
            ["AutoSyncDisabled"] = "Automatische Synchronisation deaktiviert",

            // Join URL Funktionen
            ["JoinUrlTitle"] = "Tournament Join URL",
            ["JoinUrlMessage"] = "Tournament ID: {0}\n\nJoin URL:\n{1}\n\nDiese URL können Sie an Spieler senden.",
            ["JoinUrlError"] = "Fehler beim Anzeigen der Join-URL: {0}",
            ["JoinUrlCopied"] = "Join-URL wurde in die Zwischenablage kopiert",

            // Hub Einstellungen
            ["HubSettingsTitle"] = "Hub-Einstellungen",
            ["HubSettingsPrompt"] = "Geben Sie die Tournament Hub URL ein:",
            ["HubUrlUpdated"] = "Hub-URL aktualisiert:\n{0}",
            ["HubSettingsError"] = "Fehler bei den Hub-Einstellungen: {0}",
            ["InvalidHubUrl"] = "Ungültige Hub-URL. Bitte geben Sie eine vollständige URL ein.",

            // WebSocket Verbindung
            ["WebSocketConnecting"] = "WebSocket-Verbindung wird hergestellt...",
            ["WebSocketConnected"] = "WebSocket-Verbindung hergestellt",
            ["WebSocketDisconnected"] = "WebSocket-Verbindung getrennt",
            ["WebSocketError"] = "WebSocket-Fehler: {0}",
            ["WebSocketReconnecting"] = "WebSocket-Wiederverbindung...",
            ["WebSocketReconnected"] = "WebSocket-Wiederverbindung erfolgreich",
            ["WebSocketMaxRetriesReached"] = "Maximale WebSocket-Verbindungsversuche erreicht",

            // Match Updates vom Hub
            ["MatchUpdateReceived"] = "Match-Update erhalten",
            ["MatchUpdateProcessed"] = "Match {0} erfolgreich aktualisiert",
            ["MatchUpdateError"] = "Fehler beim Verarbeiten des Match-Updates: {0}",
            ["MatchResultFromHub"] = "Match-Ergebnis vom Hub empfangen",
            ["InvalidMatchUpdate"] = "Ungültiges Match-Update erhalten",

            // Tournament Data Sync
            ["TournamentDataSyncing"] = "Tournament-Daten werden synchronisiert...",
            ["TournamentDataSynced"] = "Tournament-Daten erfolgreich synchronisiert",
            ["TournamentDataSyncError"] = "Fehler bei der Tournament-Daten-Synchronisation: {0}",
            ["SendingTournamentData"] = "Tournament-Daten werden gesendet...",
            ["TournamentDataSent"] = "Tournament-Daten erfolgreich gesendet",

            // Hub Debug Console
            ["HubDebugConsole"] = "Tournament Hub Debug Console",
            ["DebugConsoleTitle"] = "Hub Debug Console",
            ["DebugConsoleReady"] = "Ready for debugging...",
            ["DebugConsoleStarted"] = "Tournament Hub Debug Console gestartet",
            ["DebugConsoleClear"] = "Debug Console löschen",
            ["DebugConsoleClearConfirm"] = "Möchten Sie alle Debug-Nachrichten löschen?",
            ["DebugConsoleCleared"] = "Debug Console geleert",
            ["DebugConsoleSave"] = "Debug Log speichern",
            ["DebugConsoleSaved"] = "Debug Log gespeichert unter: {0}",
            ["DebugConsoleSaveError"] = "Fehler beim Speichern: {0}",
            ["DebugConsoleClose"] = "Schließen",
            ["AutoScrollEnabled"] = "Auto-Scroll aktiviert",
            ["AutoScrollDisabled"] = "Auto-Scroll deaktiviert",
            ["MessagesCount"] = "Nachrichten: {0}",

            // Hub Connection Status Details
            ["ConnectionStatusUpdated"] = "Verbindungsstatus aktualisiert",
            ["HubServiceStatus"] = "Hub Service Status",
            ["LastSyncTime"] = "Letzte Synchronisation: {0}",
            ["NextSyncIn"] = "Nächste Synchronisation in: {0}",
            ["ConnectionQuality"] = "Verbindungsqualität",
            ["ConnectionStable"] = "Stabil",
            ["ConnectionUnstable"] = "Instabil",
            ["ConnectionPoor"] = "Schlecht",

            // Hub Heartbeat
            ["HeartbeatSent"] = "Heartbeat gesendet",
            ["HeartbeatReceived"] = "Heartbeat empfangen",
            ["HeartbeatError"] = "Heartbeat-Fehler: {0}",
            ["HeartbeatTimeout"] = "Heartbeat-Timeout",

            // Tournament ID und Client Info
            ["TournamentId"] = "Tournament ID",
            ["ClientType"] = "Client-Typ",
            ["TournamentPlanner"] = "Tournament Planner",
            ["ClientVersion"] = "Client-Version",
            ["ConnectedAt"] = "Verbunden seit",
            ["ClientId"] = "Client-ID",

            // Subscription Management
            ["SubscribingToTournament"] = "Abonniere Tournament-Updates...",
            ["SubscribedToTournament"] = "Tournament-Updates erfolgreich abonniert",
            ["UnsubscribingFromTournament"] = "Tournament-Updates-Abonnement kündigen...",
            ["UnsubscribedFromTournament"] = "Tournament-Updates-Abonnement erfolgreich gekündigt",
            ["SubscriptionError"] = "Abonnement-Fehler: {0}",

            // Hub Service Messages
            ["HubServiceStarted"] = "Hub Service gestartet",
            ["HubServiceStopped"] = "Hub Service gestoppt",
            ["HubServiceInitialized"] = "Hub Service initialisiert",
            ["HubServiceError"] = "Hub Service Fehler: {0}",
            ["HubServiceRestarting"] = "Hub Service wird neugestartet...",

            // Network und Connection
            ["NetworkError"] = "Netzwerkfehler: {0}",
            ["ConnectionTimeout"] = "Verbindungs-Timeout",
            ["ConnectionRefused"] = "Verbindung verweigert",
            ["ServerNotReachable"] = "Server nicht erreichbar",
            ["InternetConnectionRequired"] = "Internetverbindung erforderlich",

            // Tournament Hub URL Validation
            ["ValidatingHubUrl"] = "Hub-URL wird validiert...",
            ["HubUrlValid"] = "Hub-URL ist gültig",
            ["HubUrlInvalid"] = "Hub-URL ist ungültig",
            ["HubUrlNotReachable"] = "Hub-URL nicht erreichbar",
            ["DefaultHubUrl"] = "Standard-Hub-URL verwenden",

            // Status Bar Messages für Hub
            ["HubStatusConnected"] = "Hub: Verbunden",
            ["HubStatusDisconnected"] = "Hub: Getrennt",
            ["HubStatusConnecting"] = "Hub: Verbinde...",
            ["HubStatusError"] = "Hub: Fehler",
            ["HubStatusSyncing"] = "Hub: Sync...",
            ["HubStatusReady"] = "Hub: Bereit",

            // Tournament Registration Details
            ["GeneratingTournamentId"] = "Tournament-ID wird generiert...",
            ["TournamentIdGenerated"] = "Tournament-ID generiert: {0}",
            ["RegisteringWithServer"] = "Registrierung beim Server...",
            ["ServerRegistrationComplete"] = "Server-Registrierung abgeschlossen",
            ["ObtainingJoinUrl"] = "Join-URL wird abgerufen...",
            ["JoinUrlObtained"] = "Join-URL erhalten: {0}",

            // Error Categories für Debug Console
            ["InfoMessage"] = "Info",
            ["WarningMessage"] = "Warnung",
            ["ErrorMessage"] = "Fehler",
            ["SuccessMessage"] = "Erfolg",
            ["WebSocketMessage"] = "WebSocket",
            ["SyncMessage"] = "Sync",
            ["TournamentMessage"] = "Tournament",
            ["MatchMessage"] = "Match",
            ["MatchResultMessage"] = "Match-Ergebnis",

            // Advanced Hub Features
            ["HubStatistics"] = "Hub-Statistiken",
            ["ConnectedClients"] = "Verbundene Clients: {0}",
            ["ActiveTournaments"] = "Aktive Tournaments: {0}",
            ["TotalMatches"] = "Gesamte Matches: {0}",
            ["DataTransferred"] = "Übertragene Daten: {0}",
            ["UptimeInfo"] = "Betriebszeit: {0}",

            // Hub Configuration
            ["HubConfiguration"] = "Hub-Konfiguration",
            ["AutoReconnect"] = "Automatische Wiederverbindung",
            ["ReconnectInterval"] = "Wiederverbindungsintervall",
            ["MaxReconnectAttempts"] = "Maximale Wiederverbindungsversuche",
            ["SyncInterval"] = "Synchronisationsintervall",
            ["HeartbeatInterval"] = "Heartbeat-Intervall",

            // Feature Flags und Capabilities
            ["HubFeatures"] = "Hub-Features",
            ["RealTimeUpdates"] = "Echtzeit-Updates",
            ["MatchStreaming"] = "Match-Streaming",
            ["StatisticsSync"] = "Statistik-Synchronisation",
            ["MultiDeviceSupport"] = "Multi-Device-Unterstützung",
            ["OfflineMode"] = "Offline-Modus",

            // User Experience Messages
            ["PleaseWait"] = "Bitte warten...",
            ["ProcessingRequest"] = "Anfrage wird verarbeitet...",
            ["AlmostDone"] = "Fast fertig...",
            ["OperationCompleted"] = "Vorgang abgeschlossen",
            ["OperationCancelled"] = "Vorgang abgebrochen",
            ["TryAgain"] = "Erneut versuchen",
            ["CheckConnection"] = "Verbindung prüfen",

            // Tournament Hub Service Lifecycle
            ["ServiceInitializing"] = "Service wird initialisiert...",
            ["ServiceReady"] = "Service bereit",
            ["ServiceShuttingDown"] = "Service wird heruntergefahren...",
            ["ServiceShutdown"] = "Service heruntergefahren",
            ["ServiceRestarted"] = "Service neugestartet",
            ["ServiceHealthy"] = "Service ist gesund",
            ["ServiceUnhealthy"] = "Service ist nicht gesund",

            // Hub License Control
            ["HubLicenseRequiredTitle"] = "Hub-Verbindung-Lizenz erforderlich",
            ["HubLicenseRequiredMessage"] = "Tournament Hub-Verbindung erfordert eine gültige Lizenz mit dem 'Hub Connection' Feature.",
            ["HubBenefitsTitle"] = "Hub-Verbindung-Features beinhalten:",
            ["HubBenefits"] = "- Echtzeit-Turnier-Synchronisation\n- Multi-Device Turnier-Management\n- Live Match-Ergebnis Updates\n- QR-Code-Freigabe für einfachen Zugriff\n- Automatische Daten-Backup und Sync",
            ["MultiDeviceManagement"] = "Multi-Device Tournament Management",
            ["MultiDeviceManagementText"] = "Hub-Verbindungen ermöglichen nahtloses Turnier-Management über mehrere Geräte mit Echtzeit-Synchronisation.",
            ["HubLicenseActionText"] = "Möchten Sie eine Lizenz mit Hub-Verbindung-Features anfordern?",
            ["RequestHubLicense"] = "Hub-Lizenz anfordern"
        };
    }
}