using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using DartTournamentPlaner.Helpers;
using DartTournamentPlaner.Models.HubSync;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services.PowerScore;
using DartTournamentPlaner.Models.PowerScore;
using DartTournamentPlaner.Services;
using System.Windows.Input;

namespace DartTournamentPlaner.Views;

public partial class HubTournamentSyncDialog : Window
{
    private readonly HubTournamentSyncPayload _payload;

    public HubTournamentSyncDialog(HubTournamentSyncPayload payload)
    {
        _payload = payload;
        InitializeComponent();
        PopulateUi();
    }

    private void PopulateUi()
    {
        SummaryText.Text = $"Daten empfangen am {_payload.ReceivedAt.ToLocalTime():G}";
        LicenseText.Text = string.IsNullOrWhiteSpace(_payload.LicenseKey)
            ? "Keine Lizenz im Payload"
            : _payload.LicenseKey;

        TournamentText.Text = string.IsNullOrWhiteSpace(_payload.TournamentName)
            ? _payload.TournamentId ?? "Unbekanntes Turnier"
            : _payload.TournamentName;

        var players = _payload.PlayerCount.HasValue ? _payload.PlayerCount.Value.ToString() : "?";
        var matches = _payload.MatchCount.HasValue ? _payload.MatchCount.Value.ToString() : "?";
        CountsText.Text = $"Spieler: {players} | Matches: {matches}";

        SourceText.Text = _payload.Source ?? "Hub";

        JsonPreview.Text = PrettyPrintJson(_payload.RawJson);
    }

    private static string PrettyPrintJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return raw;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private record HubSyncFeatures(bool? powerScoring);
    private record HubSyncParticipant(string? name, string? displayName, string? playerName, string? email, string? firstName, string? lastName, string? nickname, double? average);
    private record HubSyncFullPayload(string? tournamentId, string? name, HubSyncFeatures? features, List<HubSyncParticipant>? participants, List<string>? classes, JsonElement? gameRules);

    private HubSyncFullPayload? ParseFullPayload()
    {
        if (string.IsNullOrWhiteSpace(_payload.RawJson)) return null;

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<HubSyncFullPayload>(_payload.RawJson, options);

            if (parsed != null && parsed.participants == null)
            {
                // fallback: try to extract manually
                using var doc = JsonDocument.Parse(_payload.RawJson);
                if (doc.RootElement.TryGetProperty("participants", out var participantsElement) && participantsElement.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<HubSyncParticipant>();
                    foreach (var p in participantsElement.EnumerateArray())
                    {
                        string? name = p.TryGetProperty("name", out var n) ? n.GetString() : null;
                        string? display = p.TryGetProperty("displayName", out var d) ? d.GetString() : null;
                        string? player = p.TryGetProperty("playerName", out var pn) ? pn.GetString() : null;
                        string? email = p.TryGetProperty("email", out var em) ? em.GetString() : null;
                        string? first = p.TryGetProperty("firstName", out var fn) ? fn.GetString() : null;
                        string? last = p.TryGetProperty("lastName", out var ln) ? ln.GetString() : null;
                        string? nick = p.TryGetProperty("nickname", out var nn) ? nn.GetString() : null;
                        double? avg = null;
                        if (p.TryGetProperty("average", out var avgProp))
                        {
                            if (avgProp.ValueKind == JsonValueKind.Number && avgProp.TryGetDouble(out var avgValue)) avg = avgValue;
                        }
                        list.Add(new HubSyncParticipant(name, display, player, email, first, last, nick, avg));
                    }
                    parsed = parsed with { participants = list };
                }
            }

            return parsed;
        }
        catch
        {
            return null;
        }
    }

    private static List<ParticipantDisplay> BuildParticipantDisplays(HubSyncFullPayload full)
    {
        var participants = new List<(string BaseName, string LastName, double? Average)>();

        if (full.participants != null)
        {
            foreach (var p in full.participants)
            {
                string? baseName = null;

                if (!string.IsNullOrWhiteSpace(p.nickname))
                    baseName = p.nickname!.Trim();
                else if (!string.IsNullOrWhiteSpace(p.firstName))
                    baseName = p.firstName!.Trim();
                else if (!string.IsNullOrWhiteSpace(p.displayName))
                    baseName = p.displayName!.Trim();
                else if (!string.IsNullOrWhiteSpace(p.playerName))
                    baseName = p.playerName!.Trim();
                else if (!string.IsNullOrWhiteSpace(p.name))
                    baseName = p.name!.Trim();

                if (string.IsNullOrWhiteSpace(baseName))
                    baseName = "Spieler";

                var last = p.lastName?.Trim() ?? string.Empty;
                participants.Add((baseName, last, p.average));
            }
        }

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<ParticipantDisplay>();

        // Gruppe nach Basisnamen, dann disambiguiere über Nachnamen-Prefixe
        foreach (var group in participants.GroupBy(p => p.BaseName, StringComparer.OrdinalIgnoreCase))
        {
            var items = group.ToList();

            if (items.Count == 1)
            {
                var candidate = items[0].BaseName;
                candidate = EnsureUnique(candidate, used);
                used.Add(candidate);
                result.Add(new ParticipantDisplay { DisplayName = candidate, Average = items[0].Average });
                continue;
            }

            // Mehrere gleiche Basisnamen: baue eindeutige Namen mit LastName-Präfix
            int prefixLen = 1;
            int maxLastLen = items.Max(i => i.LastName.Length);

            List<string> candidates;
            while (true)
            {
                candidates = new List<string>();
                var localSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < items.Count; i++)
                {
                    var last = items[i].LastName;
                    string candidate;
                    if (!string.IsNullOrWhiteSpace(last))
                    {
                        var len = Math.Min(prefixLen, last.Length);
                        candidate = $"{items[i].BaseName} {last.Substring(0, len)}";
                    }
                    else
                    {
                        candidate = $"{items[i].BaseName} {i + 1}";
                    }

                    candidates.Add(candidate);
                    localSet.Add(candidate);
                }

                bool hasDupes = localSet.Count != candidates.Count || candidates.Any(c => used.Contains(c));

                if (!hasDupes)
                {
                    // Alles eindeutig
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        used.Add(candidates[i]);
                        result.Add(new ParticipantDisplay { DisplayName = candidates[i], Average = items[i].Average });
                    }
                    break;
                }

                // Falls Nachnamen aufgebraucht, hänge Zahlen an
                if (prefixLen >= maxLastLen)
                {
                    candidates.Clear();
                    var counter = 1;
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        var baseCandidate = string.IsNullOrWhiteSpace(item.LastName)
                            ? item.BaseName
                            : $"{item.BaseName} {item.LastName}";

                        var unique = EnsureUnique(baseCandidate, used, counter);
                        candidates.Add(unique);
                        used.Add(unique);
                        result.Add(new ParticipantDisplay { DisplayName = unique, Average = item.Average });
                        counter++;
                    }
                    break;
                }

                prefixLen++;
            }
        }

        return result;
    }

    private static string EnsureUnique(string baseCandidate, HashSet<string> used, int startSuffix = 1)
    {
        var candidate = baseCandidate;
        var suffix = startSuffix;
        while (used.Contains(candidate))
        {
            candidate = $"{baseCandidate} {suffix}";
            suffix++;
        }
        return candidate;
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        LocalizationService? localization = null;
        try
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow?.Services == null)
            {
                TournamentDialogHelper.ShowError("MainWindow oder Services nicht verfügbar.", "Fehler", null, this);
                return;
            }

            var services = mainWindow.Services;
            localization = services.LocalizationService;
            var full = ParseFullPayload();

            if (full == null)
            {
                TournamentDialogHelper.ShowError("Payload konnte nicht interpretiert werden.", "Fehler", services.LocalizationService, this);
                return;
            }

            var participantDisplays = BuildParticipantDisplays(full);
            
            bool hasActive = services.TournamentService.HasActiveTournament();
            if (hasActive)
            {
                var result = TournamentDialogHelper.ShowConfirmation(mainWindow, "Turnier ersetzen?", "Es ist bereits ein Turnier in Planung. Soll es ersetzt werden?", "?", true, services.LocalizationService);
                if (!result)
                {
                    return;
                }
            }

            var tournamentData = services.TournamentService.GetTournamentData();
            tournamentData.TournamentId = full.tournamentId ?? _payload.TournamentId;
            tournamentData.TournamentName = full.name ?? _payload.TournamentName;

            var powerScoringEnabled = full.features?.powerScoring == true;

            if (powerScoringEnabled)
            {
                ImportToPowerScoring(mainWindow, services, tournamentData, participantDisplays);
            }
            else
            {
                ImportWithManualAssignment(mainWindow, services, tournamentData, participantDisplays, full.classes);
            }
        }
        catch (Exception ex)
        {
            TournamentDialogHelper.ShowError($"Fehler beim Import: {ex.Message}", "Fehler", localization, this);
        }
    }

    private static void ImportToPowerScoring(MainWindow mainWindow, Helpers.MainWindowServiceInitializer services, TournamentData tournamentData, List<ParticipantDisplay> participants)
    {
        // Reset PowerScoring Session
        services.PowerScoringService.DeleteSavedSession();

        // Verwende Standard-Regel wie im UI-Default
        var session = services.PowerScoringService.CreateNewSession(PowerScoringRule.ThrowsOf3x3, tournamentData.TournamentId);

        foreach (var name in participants.Select(p => p.DisplayName))
        {
             services.PowerScoringService.AddPlayerToSession(name);
         }

        // Öffne PowerScoring-Fenster zum Weiterarbeiten
        var psWindow = new PowerScoringWindow(
            services.PowerScoringService,
            services.LocalizationService,
            services.HubService,
            services.ConfigService,
            services.TournamentService,
            mainWindow,
            autoAcceptSavedSession: true);

        psWindow.Owner = mainWindow;
        psWindow.Show();

        // Dialog schließen
        mainWindow.Activate();
    }

    private static void ImportWithManualAssignment(MainWindow mainWindow, Helpers.MainWindowServiceInitializer services, TournamentData tournamentData, List<ParticipantDisplay> participants, List<string>? payloadClasses)
     {
        // Öffne Zuordnungsdialog
        var classNames = (payloadClasses != null && payloadClasses.Count > 0)
            ? payloadClasses
            : new List<string> { "Platin", "Gold", "Silber", "Bronze" };
        var assignmentDialog = new HubSyncPlayerAssignmentDialog(participants, classNames)
        {
            Owner = mainWindow
        };

        if (assignmentDialog.ShowDialog() != true)
        {
            return;
        }

        // Reset und neu aufbauen
        services.TournamentService.ResetAllTournaments();

        int playerId = 1;
        var assignments = assignmentDialog.GetAssignments();

        foreach (var assignment in assignments)
        {
            var targetClass = services.TournamentService.AllTournamentClasses
                .FirstOrDefault(c => c.Name.Equals(assignment.SelectedClass, StringComparison.OrdinalIgnoreCase))
                ?? services.TournamentService.AllTournamentClasses.First();

            targetClass.EnsureGroupPhaseExists();
            var group = targetClass.Groups.FirstOrDefault(g => g.Name.Equals(assignment.SelectedGroup, StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                group = new Group(targetClass.Groups.Count + 1, assignment.SelectedGroup);
                targetClass.Groups.Add(group);
            }

            group.Players.Add(new Player(playerId++, assignment.Name));
        }

        services.TournamentService.TriggerUIRefresh();
        mainWindow.RefreshTournamentData();
    }
}
