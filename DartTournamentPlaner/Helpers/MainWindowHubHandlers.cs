using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Verwaltet alle Hub-bezogenen Event-Handlers und Match-Update-Verarbeitung
/// Trennt Hub-Logik vom MainWindow für bessere Wartbarkeit
/// </summary>
public class MainWindowHubHandlers
{
    private readonly MainWindow _mainWindow;
    private readonly MainWindowServiceInitializer _services;
    private readonly Action _markAsChanged;
    private readonly Action _refreshMatchDisplays;

    public MainWindowHubHandlers(
        MainWindow mainWindow,
        MainWindowServiceInitializer services,
        Action markAsChanged,
        Action refreshMatchDisplays)
    {
        _mainWindow = mainWindow;
        _services = services;
        _markAsChanged = markAsChanged;
        _refreshMatchDisplays = refreshMatchDisplays;
    }

    #region Hub Menu Event Handlers

    public async void OnRegisterWithHub(object sender, RoutedEventArgs e)
    {
        var success = HubRegistrationDialog.ShowDialog(
            _mainWindow,
            _services.HubService,
            _services.TournamentService,
            _markAsChanged,
            _services.LocalizationService);

        if (success)
        {
            Debug.WriteLine("✅ [RegisterWithHub] Hub registration successful");
        }
        else
        {
            Debug.WriteLine("⚠️ [RegisterWithHub] Hub registration cancelled or failed");
        }
    }

    public async void OnUnregisterFromHub(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_services.HubService.IsRegisteredWithHub)
            {
                var infoTitle = _services.LocalizationService.GetString("Information");
                var infoMessage = _services.LocalizationService.GetString("NoTournamentRegistered");
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmTitle = _services.LocalizationService.GetString("UnregisterTournamentTitle");
            var confirmMessage = _services.LocalizationService.GetString("UnregisterTournamentConfirm",
                _services.HubService.GetCurrentTournamentId());

            var result = MessageBox.Show(confirmMessage, confirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _services.HubService.UnregisterTournamentAsync();

                var title = _services.LocalizationService.GetString("Success");
                var message = _services.LocalizationService.GetString("UnregisterTournamentSuccess");
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error");
            var message = _services.LocalizationService.GetString("UnregisterTournamentError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async void OnManualSyncWithHub(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_services.HubService.IsRegisteredWithHub)
            {
                var infoTitle = _services.LocalizationService.GetString("Information");
                var infoMessage = _services.LocalizationService.GetString("NoTournamentRegistered");
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var success = await _services.HubService.SyncTournamentAsync(_services.TournamentService.GetTournamentData());

            var title = success ?
                _services.LocalizationService.GetString("Success") :
                _services.LocalizationService.GetString("Error");

            var message = success ?
                _services.LocalizationService.GetString("SyncSuccess") :
                _services.LocalizationService.GetString("SyncError");

            MessageBox.Show(message, title, MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error");
            var message = _services.LocalizationService.GetString("ManualSyncError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnShowJoinUrl(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_services.HubService.GetCurrentTournamentId()))
            {
                var infoTitle = _services.LocalizationService.GetString("Information");
                var infoMessage = _services.LocalizationService.GetString("NoTournamentRegistered");
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var joinUrl = _services.HubService.GetJoinUrl();

            var dialogTitle = _services.LocalizationService.GetString("JoinUrlTitle");
            var dialogMessage = _services.LocalizationService.GetString("JoinUrlMessage",
                _services.HubService.GetCurrentTournamentId(), joinUrl);

            MessageBox.Show(dialogMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            System.Windows.Clipboard.SetText(joinUrl);
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error");
            var message = _services.LocalizationService.GetString("JoinUrlError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnPlannerTournaments(object sender, RoutedEventArgs e)
    {
        try
        {
            HubPlannerTournamentsDialog.ShowDialog(
                _mainWindow,
                _services.HubService,
                _services.LocalizationService,
                _services.LicenseManager);
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error");
            var message = _services.LocalizationService.GetString("PlannerFetchStatusError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnHubSettings(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentHubUrl = _services.HubService.TournamentHubService.HubUrl;

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                _services.LocalizationService.GetString("HubSettingsPrompt"),
                _services.LocalizationService.GetString("HubSettingsTitle"),
                currentHubUrl
            );

            if (!string.IsNullOrWhiteSpace(input) && input != currentHubUrl)
            {
                _services.HubService.UpdateHubUrl(input);

                var title = _services.LocalizationService.GetString("Success");
                var message = _services.LocalizationService.GetString("HubUrlUpdated", input);
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error");
            var message = _services.LocalizationService.GetString("HubSettingsError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnHubStatusClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            var globalDebugWindow = HubIntegrationService.GlobalDebugWindow;

            if (globalDebugWindow?.IsVisible == true)
            {
                globalDebugWindow.Hide();
                Debug.WriteLine("🔍 Global Debug Console hidden via MainWindow");
            }
            else
            {
                globalDebugWindow?.Show();
                globalDebugWindow?.AddDebugMessage(_services.LocalizationService.GetString("DebugConsoleStarted"), "INFO");
                Debug.WriteLine("🔍 Global Debug Console opened via MainWindow");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error toggling Hub Debug Console: {ex.Message}");
            var title = _services.LocalizationService.GetString("Error");
            var message = _services.LocalizationService.GetString("HubSettingsError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Hub Event Handlers

    public async Task OnHubMatchResultReceived(HubMatchUpdateEventArgs e)
    {
        var success = _services.HubMatchProcessor.ProcessHubMatchUpdate(e, out var errorMessage);

        if (success)
        {
            _markAsChanged();
            ShowToastNotification("Match Update", $"Match {e.MatchId} aktualisiert", "Hub");
        }
        else if (!string.IsNullOrEmpty(errorMessage))
        {
            Debug.WriteLine($"❌ Hub Match Update Error: {errorMessage}");
        }
    }

    public async Task OnHubConnectionStateChanged(HubConnectionState state)
    {
        Debug.WriteLine($"🔔 [HubHandlers] Hub connection state changed: {state}");

        var tournamentId = _services.HubService.GetCurrentTournamentId();
        var isSyncing = _services.HubService.IsSyncing;
        var lastSyncTime = _services.HubService.LastSyncTime ?? DateTime.MinValue;

        _services.UiHelper.UpdateHubStatusDetailed(state, tournamentId, isSyncing, lastSyncTime);
    }

    public async Task OnTournamentNeedsResync()
    {
        Debug.WriteLine($"🔄 [HubHandlers] Tournament needs resync after reconnect");

        try
        {
            var tournamentData = _services.TournamentService.GetTournamentData();

            if (tournamentData == null)
            {
                Debug.WriteLine($"⚠️ [HubHandlers] No tournament data available for resync");
                return;
            }

            Debug.WriteLine($"🔄 [HubHandlers] Syncing tournament data after reconnect...");

            var success = await _services.HubService.SyncTournamentAsync(tournamentData);

            if (success)
            {
                Debug.WriteLine($"✅ [HubHandlers] Tournament data resynced successfully");
            }
            else
            {
                Debug.WriteLine($"⚠️ [HubHandlers] Tournament data resync failed");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ [HubHandlers] Error resyncing tournament data: {ex.Message}");
        }
    }

    #endregion

    #region Live Match Update Handlers

    public async Task OnHubMatchStarted(HubMatchUpdateEventArgs args)
    {
        await _mainWindow.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                Debug.WriteLine($"🎬 [MATCH-STARTED] {args.GetMatchIdentificationSummary()}");
                Debug.WriteLine($"   📊 Status from Hub: '{args.Status}' (IsMatchStarted: {args.IsMatchStarted})");
                Debug.WriteLine($"   📊 Status Description: {args.GetStatusDescription()}");

                if (args.Status != "InProgress")
                {
                    Debug.WriteLine($"   ⚠️ WARNING: Status should be 'InProgress' but is '{args.Status}'!");
                }

                Debug.WriteLine($"🔍 [MATCH-STARTED] Calling ProcessHubMatchUpdate...");

                if (_services.HubMatchProcessor == null)
                {
                    Debug.WriteLine($"   ❌ ERROR: HubMatchProcessor is NULL!");
                    return;
                }

                var success = _services.HubMatchProcessor.ProcessHubMatchUpdate(args, out var errorMessage);

                if (!success && !string.IsNullOrEmpty(errorMessage))
                {
                    Debug.WriteLine($"   ❌ Match processing failed: {errorMessage}");
                }
                else if (success)
                {
                    Debug.WriteLine($"   ✅ Match processing succeeded!");
                }

                UpdateMatchStatusInUI(args, "InProgress");
                UpdateMatchScoreInUI(args);

                if (_services.ConfigService.Config.ShowMatchStartNotifications)
                {
                    ShowMatchStartNotification(args);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [MATCH-STARTED] Error handling match started event: {ex.Message}");
            }
        });
    }

    public async Task OnHubLegCompleted(HubMatchUpdateEventArgs args)
    {
        await _mainWindow.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                Debug.WriteLine($"🎯 [LEG-COMPLETED] {args.GetMatchIdentificationSummary()}");
                Debug.WriteLine($"   📊 Leg {args.CurrentLeg}/{args.TotalLegs} - Score: {args.Player1Legs}-{args.Player2Legs}");

                var success = _services.HubMatchProcessor.ProcessHubMatchUpdate(args, out var errorMessage);

                if (!success && !string.IsNullOrEmpty(errorMessage))
                {
                    Debug.WriteLine($"   ❌ Match processing failed: {errorMessage}");
                }

                UpdateMatchScoreInUI(args);

                if (args.LegResults != null && args.LegResults.Count > 0)
                {
                    var lastLeg = args.LegResults.LastOrDefault();
                    if (lastLeg != null)
                    {
                        Debug.WriteLine($"   🏆 Winner: {lastLeg.Winner}");
                        Debug.WriteLine($"   ⏱️ Duration: {lastLeg.Duration:mm\\:ss}");
                        Debug.WriteLine($"   🎯 Darts: P1={lastLeg.Player1Darts}, P2={lastLeg.Player2Darts}");
                    }
                }

                _refreshMatchDisplays();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [LEG-COMPLETED] Error handling leg completed event: {ex.Message}");
            }
        });
    }

    public async Task OnHubMatchProgressUpdated(HubMatchUpdateEventArgs args)
    {
        await _mainWindow.Dispatcher.InvokeAsync(() =>
        {
            try
            {
                Debug.WriteLine($"📈 [MATCH-PROGRESS] {args.GetMatchIdentificationSummary()}");

                var success = _services.HubMatchProcessor.ProcessHubMatchUpdate(args, out var errorMessage);

                if (!success && !string.IsNullOrEmpty(errorMessage))
                {
                    Debug.WriteLine($"   ❌ Match processing failed: {errorMessage}");
                }

                if (args.MatchDuration.HasValue)
                {
                    Debug.WriteLine($"   ⏱️ Duration: {args.MatchDuration.Value:mm\\:ss}");
                }

                if (args.CurrentPlayer1LegScore.HasValue && args.CurrentPlayer2LegScore.HasValue)
                {
                    Debug.WriteLine($"   📊 Current Leg: {args.CurrentPlayer1LegScore}-{args.CurrentPlayer2LegScore}");
                }

                UpdateMatchProgressInUI(args);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [MATCH-PROGRESS] Error handling match progress event: {ex.Message}");
            }
        });
    }

    #endregion

    #region Match Update Helper Methods

    private void UpdateMatchStatusInUI(HubMatchUpdateEventArgs args, string status)
    {
        try
        {
            var tournamentData = _services.TournamentService.GetTournamentData();
            if (tournamentData == null) return;

            var matchObj = FindMatchInTournament(tournamentData, args);
            if (matchObj == null)
            {
                Debug.WriteLine($"⚠️ [UPDATE-STATUS] Match not found: {args.GetMatchIdentificationSummary()}");
                return;
            }

            MatchStatus matchStatus = status switch
            {
                "InProgress" => MatchStatus.InProgress,
                "Finished" => MatchStatus.Finished,
                "NotStarted" => MatchStatus.NotStarted,
                "Bye" => MatchStatus.Bye,
                _ => MatchStatus.InProgress
            };

            if (matchObj is Match match)
            {
                match.Status = matchStatus;

                var liveIndicator = _services.LocalizationService.GetString("Hub_LiveIndicator");
                var statusIndicator = status == "InProgress" ? liveIndicator : "";
                match.Notes = $"{statusIndicator} {match.Notes?.Replace(liveIndicator, "").Trim()}".Trim();

                Debug.WriteLine($"✅ [UPDATE-STATUS] Match status updated to: {matchStatus} ({status})");
                Debug.WriteLine($"   📊 Match.StatusDisplay will now show: {match.StatusDisplay}");
            }
            else if (matchObj is KnockoutMatch koMatch)
            {
                koMatch.Status = matchStatus;

                var liveIndicator = _services.LocalizationService.GetString("Hub_LiveIndicator");
                var statusIndicator = status == "InProgress" ? liveIndicator : "";
                koMatch.Notes = $"{statusIndicator} {koMatch.Notes?.Replace(liveIndicator, "").Trim()}".Trim();

                Debug.WriteLine($"✅ [UPDATE-STATUS] KO Match status updated to: {matchStatus} ({status})");
                Debug.WriteLine($"   📊 KnockoutMatch.StatusDisplay will now show: {koMatch.StatusDisplay}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ [UPDATE-STATUS] Error: {ex.Message}");
        }
    }

    private void UpdateMatchScoreInUI(HubMatchUpdateEventArgs args)
    {
        try
        {
            var tournamentData = _services.TournamentService.GetTournamentData();
            if (tournamentData == null) return;

            var matchObj = FindMatchInTournament(tournamentData, args);
            if (matchObj == null) return;

            if (matchObj is Match match)
            {
                UpdateRegularMatch(match, args);
            }
            else if (matchObj is KnockoutMatch koMatch)
            {
                UpdateKnockoutMatch(koMatch, args);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ [UPDATE-SCORE] Error: {ex.Message}");
        }
    }

    private void UpdateRegularMatch(Match match, HubMatchUpdateEventArgs args)
    {
        match.Player1Legs = args.Player1Legs;
        match.Player2Legs = args.Player2Legs;
        match.Player1Sets = args.Player1Sets;
        match.Player2Sets = args.Player2Sets;

        Debug.WriteLine($"🔍 [UPDATE-REGULAR-MATCH] Status Check:");
        Debug.WriteLine($"   args.IsMatchCompleted: {args.IsMatchCompleted}");
        Debug.WriteLine($"   args.IsMatchStarted: {args.IsMatchStarted}");
        Debug.WriteLine($"   args.Status: {args.Status}");
        Debug.WriteLine($"   Current match.Status: {match.Status}");

        if (args.IsMatchCompleted)
        {
            match.Status = MatchStatus.Finished;
            Debug.WriteLine($"   ✅ Match marked as Finished");
        }
        else if (args.IsMatchStarted)
        {
            match.Status = MatchStatus.InProgress;
            Debug.WriteLine($"   ✅ Match marked as InProgress");
        }
        else
        {
            Debug.WriteLine($"   ⚠️ WARNING: Neither IsMatchCompleted nor IsMatchStarted is true!");
        }

        int calculatedTotalLegs = CalculateMaxLegs(match, args);

        var liveIndicator = _services.LocalizationService.GetString("Hub_LiveIndicator");
        var legProgress = _services.LocalizationService.GetString("Hub_LegProgress", args.CurrentLeg, calculatedTotalLegs);
        match.Notes = $"{liveIndicator} - {legProgress}";

        Debug.WriteLine($"✅ [UPDATE-SCORE] Match score updated: {args.Player1Legs}-{args.Player2Legs}");
        Debug.WriteLine($"   📊 Calculated total legs: {calculatedTotalLegs} (Hub sent: {args.TotalLegs})");
        Debug.WriteLine($"   📊 Status: {match.Status}, Display: {match.StatusDisplay}");
        Debug.WriteLine($"   📝 Notes: {match.Notes}");

        try
        {
            var tournamentData = _services.TournamentService.GetTournamentData();
            if (tournamentData != null)
            {
                var tournamentClass = tournamentData.TournamentClasses.FirstOrDefault(c => c.Id == args.ClassId);
                if (tournamentClass != null)
                {
                    Debug.WriteLine($"🔄 [UI-REFRESH] Triggering UI refresh for tournament class {tournamentClass.Name}");
                    tournamentClass.TriggerUIRefresh();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ [UI-REFRESH] Error triggering UI refresh: {ex.Message}");
        }

        if (args.IsMatchCompleted)
        {
            _markAsChanged();
        }
    }

    private void UpdateKnockoutMatch(KnockoutMatch koMatch, HubMatchUpdateEventArgs args)
    {
        koMatch.Player1Legs = args.Player1Legs;
        koMatch.Player2Legs = args.Player2Legs;
        koMatch.Player1Sets = args.Player1Sets;
        koMatch.Player2Sets = args.Player2Sets;

        if (args.IsMatchCompleted)
        {
            koMatch.Status = MatchStatus.Finished;
            Debug.WriteLine($"   ✅ KO Match marked as Finished");
        }
        else if (args.IsMatchStarted)
        {
            koMatch.Status = MatchStatus.InProgress;
            Debug.WriteLine($"   ✅ KO Match marked as InProgress");
        }

        var tempMatch = new Match { Id = koMatch.Id, UniqueId = koMatch.UniqueId };
        int calculatedTotalLegs = CalculateMaxLegs(tempMatch, args);

        var liveIndicator = _services.LocalizationService.GetString("Hub_LiveIndicator");
        var legProgress = _services.LocalizationService.GetString("Hub_LegProgress", args.CurrentLeg, calculatedTotalLegs);
        koMatch.Notes = $"{liveIndicator} - {legProgress}";

        Debug.WriteLine($"✅ [UPDATE-SCORE] KO Match score updated: {args.Player1Legs}-{args.Player2Legs}");
        Debug.WriteLine($"   📊 Calculated total legs: {calculatedTotalLegs} (Hub sent: {args.TotalLegs})");
        Debug.WriteLine($"   📊 Status: {koMatch.Status}, Display: {koMatch.StatusDisplay}");
        Debug.WriteLine($"   📝 Notes: {koMatch.Notes}");

        Debug.WriteLine($"🌳 [TREE-UPDATE] Checking TournamentTreeRenderer.CurrentInstance...");
        Debug.WriteLine($"   📦 Current Instance: {TournamentTreeRenderer.CurrentInstance != null}");

        if (TournamentTreeRenderer.CurrentInstance != null)
        {
            try
            {
                bool isLoserBracket = args.MatchType?.Contains("Loser") == true;

                Debug.WriteLine($"🌳 [TREE-UPDATE] Refreshing match {koMatch.Id} in tree (Loser: {isLoserBracket})");
                TournamentTreeRenderer.CurrentInstance.RefreshMatchInTree(koMatch.Id, isLoserBracket);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ [TREE-UPDATE] Error: {ex.Message}");
            }
        }
        else
        {
            Debug.WriteLine($"⚠️ [TREE-UPDATE] TournamentTreeRenderer.CurrentInstance is NULL - tree won't update!");
            Debug.WriteLine($"   💡 Hint: Switch to KO Tab to initialize the tree renderer");
        }

        if (args.IsMatchCompleted)
        {
            _markAsChanged();
        }
    }

    private int CalculateMaxLegs(Match match, HubMatchUpdateEventArgs args)
    {
        try
        {
            var tournamentData = _services.TournamentService.GetTournamentData();
            if (tournamentData == null) return args.TotalLegs;

            var tournamentClass = tournamentData.TournamentClasses.FirstOrDefault(c => c.Id == args.ClassId);
            if (tournamentClass == null) return args.TotalLegs;

            var gameRules = tournamentClass.GameRules;
            if (gameRules == null) return args.TotalLegs;

            int legsToWin = gameRules.LegsToWin;

            if (args.MatchType?.Contains("Knockout") == true)
            {
                Debug.WriteLine($"   🏆 KO match detected: {args.MatchType}");

                if (args.TotalLegs > 0)
                {
                    legsToWin = args.TotalLegs;
                    Debug.WriteLine($"   🔧 Using Hub's totalLegs ({args.TotalLegs}) as LegsToWin for KO match");
                }
            }
            else
            {
                Debug.WriteLine($"   📊 Regular match (Group/Finals): {args.MatchType ?? "Unknown"}");
                Debug.WriteLine($"   📋 Using GameRules.LegsToWin = {legsToWin}");
            }

            int maxLegs = (2 * legsToWin) - 1;

            Debug.WriteLine($"🎯 Game Rules: LegsToWin = {legsToWin}, Calculated MaxLegs = {maxLegs}");

            return maxLegs;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"   ⚠️ Error calculating max legs: {ex.Message}, using Hub value");
            return (2 * args.TotalLegs) - 1;
        }
    }

    private void UpdateMatchProgressInUI(HubMatchUpdateEventArgs args)
    {
        try
        {
            _refreshMatchDisplays();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ [UPDATE-PROGRESS] Error: {ex.Message}");
        }
    }

    private void ShowMatchStartNotification(HubMatchUpdateEventArgs args)
    {
        try
        {
            var tournamentData = _services.TournamentService.GetTournamentData();
            if (tournamentData == null) return;

            var matchObj = FindMatchInTournament(tournamentData, args);
            if (matchObj == null) return;

            var className = tournamentData.TournamentClasses
                .FirstOrDefault(c => c.Id == args.ClassId)?.Name ?? "Unknown Class";

            string? player1Name = null;
            string? player2Name = null;

            if (matchObj is Match match)
            {
                player1Name = match.Player1?.Name;
                player2Name = match.Player2?.Name;
            }
            else if (matchObj is KnockoutMatch koMatch)
            {
                player1Name = koMatch.Player1?.Name;
                player2Name = koMatch.Player2?.Name;
            }

            var message = _services.LocalizationService.GetString(
                "Hub_MatchStartNotification",
                player1Name ?? "Player 1",
                player2Name ?? "Player 2",
                className);

            Debug.WriteLine($"📢 {message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ [NOTIFICATION] Error: {ex.Message}");
        }
    }

    private object? FindMatchInTournament(TournamentData tournamentData, HubMatchUpdateEventArgs args)
    {
        var tournamentClass = tournamentData.TournamentClasses.FirstOrDefault(c => c.Id == args.ClassId);
        if (tournamentClass == null) return null;

        if (args.GroupId.HasValue)
        {
            var group = tournamentClass.Groups.FirstOrDefault(g => g.Id == args.GroupId.Value);
            if (group != null)
            {
                return group.Matches.FirstOrDefault(m =>
                    (args.HasUuid && m.UniqueId == args.MatchUuid) ||
                    (!args.HasUuid && m.Id == args.MatchId)
                );
            }
        }

        if (!args.GroupId.HasValue && args.GroupName?.Contains("Finals") == true)
        {
            Debug.WriteLine($"   🔍 Searching for Finals match: {args.GroupName}");

            if (tournamentClass.CurrentPhase?.FinalsGroup != null)
            {
                var finalsMatch = tournamentClass.CurrentPhase.FinalsGroup.Matches.FirstOrDefault(m =>
                    (args.HasUuid && m.UniqueId == args.MatchUuid) ||
                    (!args.HasUuid && m.Id == args.MatchId)
                );

                if (finalsMatch != null)
                {
                    Debug.WriteLine($"   ✅ Found match in Finals");
                    return finalsMatch;
                }
            }

            Debug.WriteLine($"   ⚠️ Finals match not found");
        }

        if (!args.GroupId.HasValue && args.MatchType?.Contains("Knockout") == true)
        {
            Debug.WriteLine($"   🔍 Searching for KO match: {args.MatchType}");

            foreach (var phase in tournamentClass.Phases)
            {
                if (phase.PhaseType == TournamentPhaseType.KnockoutPhase)
                {
                    foreach (var koMatch in phase.WinnerBracket)
                    {
                        if ((args.HasUuid && koMatch.UniqueId == args.MatchUuid) ||
                            (!args.HasUuid && koMatch.Id == args.MatchId))
                        {
                            Debug.WriteLine($"   ✅ Found KO match in Winner Bracket");
                            return koMatch;
                        }
                    }

                    foreach (var koMatch in phase.LoserBracket)
                    {
                        if ((args.HasUuid && koMatch.UniqueId == args.MatchUuid) ||
                            (!args.HasUuid && koMatch.Id == args.MatchId))
                        {
                            Debug.WriteLine($"   ✅ Found KO match in Loser Bracket");
                            return koMatch;
                        }
                    }
                }
            }

            Debug.WriteLine($"   ⚠️ KO match not found in any bracket");
        }

        if (!args.GroupId.HasValue)
        {
            foreach (var group in tournamentClass.Groups)
            {
                var match = group.Matches.FirstOrDefault(m =>
                    (args.HasUuid && m.UniqueId == args.MatchUuid) ||
                    (!args.HasUuid && m.Id == args.MatchId)
                );
                if (match != null) return match;
            }
        }

        return null;
    }

    #endregion

    #region Helper Methods

    private void ShowToastNotification(string title, string message, string source)
    {
        Debug.WriteLine($"🔔 {title}: {message} (Source: {source})");
    }

    #endregion
}
