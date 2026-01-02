# Copilot-Nutzungsregeln für Dart Tournament Planner

## Antwortformat & Nachvollziehbarkeit
- **Alle Chat-Antworten ausschließlich auf Deutsch.**
- **Jeden Agenten-Schritt klar beschreiben** (Datei, Zweck, wichtigste Methoden/Pfade), damit Änderungen nachvollziehbar sind.
- Zusammenfassungen immer im Markdown-Format mit Überschriften; falls passend „Nächste mögliche Anpassungen“ als Stichpunkte ergänzen.

## Projektarchitektur (WPF, .NET 9, C# 13)
- Hauptprojekt `DartTournamentPlaner` (WPF, MVVM-orientiert) + Nebenprojekt `DartTournamentPlaner.API` (In-Memory REST/SignalR).
- Schichten: `Services` (Logik/IO), `Models` (Domäne), `Views/*.xaml(.cs)` + `Controls` (UI), `Helpers` (Event-/Dialog-/UI-Helfer), `Themes` (Hell/Dunkel Ressourcen).

## Start- und Service-Flow
- `App.xaml.cs` initialisiert `ConfigService` (config.json), `LocalizationService`, `DataService`, `UpdateService`, `ThemeService`, `UserAuthService`, temporär `HttpApiIntegrationService`; zeigt `StartupSplashWindow` mit Fortschritt ? checkt Updates ? öffnet `MainWindow`.
- `ConfigService` feuert `LanguageChanged` ? `LocalizationService` + `App.GlobalLanguageChanged` verteilen Übersetzungen an Fenster.

## Persistenz & Config
- Turnierdaten in `tournament_data.json` über `DataService`: lädt, bereinigt Phasen, repariert UUIDs (`Match.HasValidUniqueId/EnsureUniqueId`), validiert Statistiken (`TournamentClass.ValidateAndRepairStatistics`), erstellt Backups beim Speichern.
- `ConfigService` speichert Sprache/Hub-URL/Theme in `config.json`; `ChangeLanguageAsync` triggert Event und persistiert sofort.

## Lizenzierung
- `Services/License/LicenseManager` validiert gegen `https://license-dtp.i3ull3t.de`, speichert Key + letzte Validierung in Registry (`HKCU\SOFTWARE\DartTournamentPlanner`).
- Feature-Gates über `LicenseFeatureService`/Dialogs `Views/License/*` (PowerScoring, erweiterter Druck, Hub-Premium). Bei Premium-Funktionen Lizenzstatus prüfen.

## Tournament Hub & Live-Sync
- `HubIntegrationService` + `TournamentHubService` + `HubWebSocket/*`: WebSocket init, Registrierung (`RegisterTournamentAsync`), Heartbeat/Sync-Timer, Reconnect-Logik, Debug-Konsole (`HubDebugWindow` global).
- Events: `MatchResultReceived`, Live-Events (MatchStarted/LegCompleted/Progress), `PowerScoringMessageReceived`, `TournamentNeedsResync` für Re-Sync nach Reconnect. Join-URL über `GetJoinUrl`.

## PowerScoring & Premium
- PowerScoring-UI/Logik in `Views/PowerScoring*` und `Services/PowerScore/*` (Session, Distribution, Persistenz, Hub-Nachrichten). Verteilung kann direkt Turnier erzeugen (`PowerScoringToTournamentService`).

## Drucken & QR
- Drucken via `Helpers/PrintHelper.cs`, `Services/Print/*`, Dialog `Views/TournamentPrintDialog.xaml(.cs)`; Premium-Gate möglich (`PrintLicenseRequiredDialog`).
- QR-Codes: `Helpers/PrintQRCodeHelper.cs`, `TournamentOverviewQRCodeHelper.cs`, Bibliothek `QRCoder`.

## Lokalisierung & Themes
- Sprachprovider pro Bereich unter `Services/Languages/{German,English}/*` (UI/Startup/Tournament/Knockout/License/Statistics/PowerScoring/Help/Print/Hub). Umschaltbar zur Laufzeit ohne Neustart.
- `ThemeService` liest Config und wendet Theme-Ressourcen (`Themes/LightTheme.xaml`, `Themes/DarkTheme.xaml`) früh im Startup an.

## API-Projekt (optional)
- `DartTournamentPlaner.API`: ASP.NET Core mit Swagger, In-Memory EF (`ApiDbContext`), SignalR Hub `TournamentHub`, Controller für Tournaments/Matches, Sync-Service.
- Startbar mit `dotnet run --project DartTournamentPlaner.API --urls http://localhost:5000`; Swagger unter `/` verfügbar. WPF-seitig Integration über `ApiIntegrationService`/`HttpApiIntegrationService`/`LicensedApiIntegrationService` (Start/Stop API-Prozess, URLs merken).

## Build- & Laufhinweise
- Standard: `dotnet build DartTournamentPlaner.sln` (WPF, net9.0-windows). Tests nicht vorhanden.
- Lauf/Debug primär in Visual Studio; auf Windows-spezifische APIs achten (Registry, `ManagementObjectSearcher`). Assets/Icons unter `Assets/Images/*` im csproj als Resource.

## Arbeitsmuster & Konventionen
- UI-Logik meist in Code-Behind (`Views/*.xaml.cs`) plus Helper (`MainWindow*`, `TournamentTab*`, `TournamentDialogHelper`).
- Sprache/Übersetzungen stets über `LocalizationService` abrufen; keine Hardcoded-Strings für UI.
- Bei Premium-Funktionen zuerst Lizenzstatus prüfen; Hub-Operationen nur bei aktiver WebSocket-Verbindung/registrierter Tournament-ID.
- Vor Änderungen relevante Dateien lesen (keine Pfade raten); reale Strukturen aus obigen Pfaden nutzen.
