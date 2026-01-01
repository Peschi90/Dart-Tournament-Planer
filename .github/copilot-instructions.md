# Copilot Instructions for DartTournamentPlaner

## Big picture
- WPF desktop app targeting **.NET 9 / C# 13** (`DartTournamentPlaner.csproj`). UI is XAML-first with lots of code-behind helpers; no tests.
- Core domains: tournament management (groups/knockout/finals), **PowerScoring** (player seeding with QR/Hub), **Tournament Hub** WebSocket sync, **licensing** (free vs premium), theming (Light/Dark), localization (DE/EN).
- Data is persisted as JSON (see `Services` folder) with auto-save/backups; print/export uses dedicated dialogs.

## Patterns to follow
- **Dialogs**: Prefer modern helpers over raw `MessageBox`. Use `TournamentDialogHelper` (info/warn/error/confirm) or specialized dialog classes (e.g., `PowerScoringConfirmDialog`).
- **Localization**: Fetch text via `LocalizationService.GetString/GetTranslation`. Update UI text on language change events where applicable. Use existing keys before adding new ones.
- **Theming/resources**: Use dynamic resources (`BackgroundBrush`, `SurfaceBrush`, `Dialog*Gradient`, etc.) from theme dictionaries; keep controls theme-aware and dark-mode friendly.
- **Layouts**: Windows are borderless, draggable via header `MouseLeftButtonDown` and typically resizable with minimum size set to current defaults. Maintain rounding/shadows (`CardShadow`) and gradients consistent with modern dialogs.
- **PowerScoring**: Uses `PowerScoringService` for sessions; respects Tournament ID syncing and auto-save. QR codes via `QRCoder`. Live updates handled through event subscriptions (`PlayerScoreUpdated`, Hub message handlers).
- **Licensing**: Managed by `LicenseManager` + `LicenseFeatureService`. UI windows (`LicenseInfoWindow`, `LicenseStatusWindow`, activation dialogs) show status badges and handle online/offline validation. Avoid bypassing these services.
- **Tournament Hub**: WebSocket integration (`HubIntegrationService`, `LicensedHubService`) with reconnect/register logic and status indicators; don’t hand-roll networking.

## Key files/directories
- UI: `Views/` (dialogs/windows), styling resources in `Themes/LightTheme.xaml` & `DarkTheme.xaml`.
- Helpers/Services: `Helpers/` (UI helpers, dialog helpers), `Services/` (Hub, License, PowerScoring, Config, Localization).
- Entry: `MainWindow.xaml` (+ code-behind), splash: `StartupSplashWindow`.
- Printing: `Views/TournamentPrintDialog.xaml` and related helpers.

## Build & run
- Restore/build normally: `dotnet build` at solution root. No custom scripts/tests.
- App is WPF; launch via Visual Studio/`dotnet run` in `DartTournamentPlaner` project.

## Implementation tips
- Preserve **auto-save/session** behavior when touching PowerScoring or tournament flows.
- Respect **min-size** constraints when adjusting windows; keep drag handlers on headers.
- When adding strings/UI, ensure **dark/light** contrast and dynamic resources are used.
- Prefer **async** patterns already present; UI updates on Dispatcher as seen in PowerScoring/Hubs.

If any section feels incomplete or unclear, ask which areas need more detail (e.g., Hub sync, licensing flows, printing, or localization keys).