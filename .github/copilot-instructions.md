# Copilot Instructions

## 1. Repository Context
- WPF desktop application for planning and running dart tournaments (classes, group/KO phases, PowerScoring, printing, licensing, hub sync, theming, localization).
- Companion ASP.NET Core API (REST + SignalR) for tournament data sync and demo purposes; references the main project models.
- Assets include light/dark themes, extensive localization dictionaries (DE/EN), and release packaging via Inno Setup.

## 2. Non-Negotiable Rules
- MUST target `net9.0-windows` with C# 13; do not change TFM/LangVersion.
- MUST keep nullable and implicit usings enabled as in the csproj files.
- MUST update both German and English language providers for any new/changed user-facing strings; avoid leaving untranslated text.
- MUST avoid editing generated files under `obj` or `*.g.i.cs`.
- MUST keep changelog entries (`CHANGELOG.md`) and project version metadata (`AssemblyVersion`/`AssemblyFileVersion` in csproj) aligned when shipping versioned changes; release workflow depends on the changelog section for the tag.
- MUST respect existing theme resources (Light/Dark) rather than hard-coding colors; use shared resources where available.

## 3. Architecture & Code Organization
- Main project `DartTournamentPlaner`: `Views` (XAML + code-behind dialogs/windows), `Controls`, `Services` (localization, tournament mgmt, hub/websocket integration, printing, licensing, data), `Models` (tournament, player, licensing, PowerScoring), `Helpers` (UI/event utilities), `Themes` (Light/Dark resource dictionaries), `App.xaml` merges theme resources.
- Localization is implemented via `Services/Languages/*` with `ILanguageProvider` and per-feature language provider classes (German/English). Strings live in these providers and are consumed through `LocalizationService`.
- API project `DartTournamentPlaner.API`: `Controllers`, `Services`, `Hubs`, `Models`; uses EF Core InMemory storage, SignalR hub (`TournamentHub`), Swagger setup, permissive CORS policy. Depends on the main project for shared types.
- Solution file `DartTournamentPlaner.sln` builds both projects; release artifacts are expected under `bin/Release/net9.0-windows`.

## 4. Tech Stack & Tooling
- Languages/Frameworks: C# 13, .NET 9, WPF (desktop), ASP.NET Core 9 (Web + SignalR), EF Core InMemory.
- Key packages: MailKit, MySqlConnector, BCrypt.Net-Next, QRCoder, Newtonsoft.Json; API uses Swashbuckle.AspNetCore, Microsoft.AspNetCore.OpenApi, SignalR, EF Core InMemory, ASP.NET Core NewtonsoftJson.
- Build tooling: NuGet restore, MSBuild; release packaging via Inno Setup (see `installer.iss`).

## 5. Quality Gates (Required Before PR)
- Run dependency restore: `nuget restore DartTournamentPlaner.sln`.
- Build solution in Release: `msbuild DartTournamentPlaner.sln /p:Configuration=Release` (or `dotnet build -c Release` locally) and ensure `net9.0-windows` outputs succeed.
- If modifying API code, ensure the API starts (`dotnet run --project DartTournamentPlaner.API`) and Swagger loads without errors; if modifying desktop UI, perform manual UI smoke-test for affected dialogs/windows.

## 6. Change & Refactoring Guidelines
- Place UI changes in the appropriate `Views`/`Controls` with supporting logic in `Services`/`Helpers`; keep models in `Models` and avoid business logic in code-behind where services exist.
- Mirror localization updates in both DE and EN providers; wire new keys through `LocalizationService` rather than inlining text.
- Reuse shared theme resources from `Themes` instead of duplicating styles; maintain dialog/window styling consistency.
- Keep hub/websocket/API interactions routed through existing services (`HubIntegrationService`, `WebSocketConnectionManager`, API services) rather than new ad-hoc calls.
- When introducing user-visible features or version changes, update `CHANGELOG.md` and bump assembly versions in csproj files consistently with README claims.
- Align new API endpoints with existing controller/service patterns and ensure SignalR hubs remain registered at `/tournamentHub`.

## 7. Expected Output Format
- Releases are tagged `vX.Y.Z` (per workflow); changelog sections use the same `## vX.Y.Z` headers.
- No repo-specific commit message format observed; use clear messages and include a brief testing note in PR descriptions along with the commands run.

## 8. Assumptions & Open Questions
- No automated tests or linting are present; manual validation is assumed for UI/API changes.
- Documentation in `Docs/` is present but not formally enforced; update relevant docs/README when altering major features.
- Top-level `Models/Services/Views` folders at repo root appear unused; assumed legacy/placeholder.

- Bitte antworte immer in deutsch!