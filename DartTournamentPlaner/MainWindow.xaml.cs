using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using DartTournamentPlaner.Helpers;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Views.License;
using DartTournamentPlaner.Services.PowerScore;

namespace DartTournamentPlaner;

/// <summary>
/// Vereinfachtes Code-Behind für das Hauptfenster
/// REFACTORED: Delegiert Logik an spezialisierte Helper-Klassen:
/// - MainWindowServiceInitializer: Service-Initialisierung
/// - MainWindowEventHandlers: Menü- und UI-Event-Handlers
/// - MainWindowHubHandlers: Hub-Integration und Match-Updates
/// - MainWindowUIHelper: UI-Updates und Status-Management
/// </summary>
public partial class MainWindow : Window
{
    // Helper-Klassen
    private readonly MainWindowServiceInitializer _serviceInitializer;
    private readonly MainWindowEventHandlers _eventHandlers;
    private readonly MainWindowHubHandlers _hubHandlers;

    // Auto-Save System
    private readonly DispatcherTimer _autoSaveTimer = new DispatcherTimer();
    private bool _hasUnsavedChanges = false;

    // Properties für Legacy-Kompatibilität
    public ITournamentHubService? TournamentHubService => _serviceInitializer.HubService.TournamentHubService;
    public string GetCurrentTournamentId() => _serviceInitializer.HubService.GetCurrentTournamentId();
    public bool IsRegisteredWithHub => _serviceInitializer.HubService.IsRegisteredWithHub;

    /// <summary>
    /// Konstruktor des Hauptfensters - stark vereinfacht durch Service-Delegation
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // Service-Initialisierung
        _serviceInitializer = new MainWindowServiceInitializer(this);

        // Event-Handlers initialisieren
        _eventHandlers = new MainWindowEventHandlers(
            this,
            _serviceInitializer,
            SaveDataInternal,
            MarkAsChanged,
            UpdateTranslations,
            ConfigureTournamentTabs);

        // Hub-Handlers initialisieren
        _hubHandlers = new MainWindowHubHandlers(
            this,
            _serviceInitializer,
            MarkAsChanged,
            RefreshMatchDisplays);

        // Services konfigurieren
        InitializeServices();
        InitializeAutoSave();
        InitializeApiService();

        UpdateTranslations();
        LoadData();

        _ = Task.Run(async () => await _serviceInitializer.InitializeHubAsync());
        _ = InitializeLicenseSystemAsync();
    }

    #region Service Initialization

    private void InitializeServices()
    {
        // UI Helper konfigurieren
        _serviceInitializer.UiHelper.HubStatusIndicator = HubStatusIndicator;
        _serviceInitializer.UiHelper.HubStatusText = HubStatusText;
        _serviceInitializer.UiHelper.HubSyncStatus = HubSyncStatus;
        _serviceInitializer.UiHelper.StatusTextBlock = StatusTextBlock;
        _serviceInitializer.UiHelper.LastSavedBlock = LastSavedBlock;
        _serviceInitializer.UiHelper.LanguageStatusBlock = LanguageStatusBlock;

        // Services mit Event-Handlers konfigurieren
        _serviceInitializer.ConfigureServices(
            markAsChanged: MarkAsChanged,
            updateTranslations: UpdateTranslations,
            forceUIUpdate: ForceUIUpdate,
            configureTournamentTabs: ConfigureTournamentTabs,
            onHubMatchResultReceived: _hubHandlers.OnHubMatchResultReceived,
            onHubConnectionStateChanged: _hubHandlers.OnHubConnectionStateChanged,
            onTournamentNeedsResync: _hubHandlers.OnTournamentNeedsResync,
            onApiMatchResultUpdated: OnApiMatchResultUpdated,
            onLicenseStatusChanged: OnLicenseStatusChanged,
            onHubMatchStarted: _hubHandlers.OnHubMatchStarted,
            onHubLegCompleted: _hubHandlers.OnHubLegCompleted,
            onHubMatchProgressUpdated: _hubHandlers.OnHubMatchProgressUpdated);

        // Tournament Tabs konfigurieren
        ConfigureTournamentTabs();
    }

    private void ConfigureTournamentTabs()
    {
        PlatinTab.TournamentClass = _serviceInitializer.TournamentService.PlatinClass;
        GoldTab.TournamentClass = _serviceInitializer.TournamentService.GoldClass;
        SilberTab.TournamentClass = _serviceInitializer.TournamentService.SilberClass;
        BronzeTab.TournamentClass = _serviceInitializer.TournamentService.BronzeClass;

        PlatinTab.DataChanged += (s, e) => MarkAsChanged();
        GoldTab.DataChanged += (s, e) => MarkAsChanged();
        SilberTab.DataChanged += (s, e) => MarkAsChanged();
        BronzeTab.DataChanged += (s, e) => MarkAsChanged();
    }

    private void InitializeAutoSave()
    {
        _autoSaveTimer.Tick += AutoSave_Tick;
        UpdateAutoSaveTimer();

        _serviceInitializer.ConfigService.Config.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AppConfig.AutoSave) || e.PropertyName == nameof(AppConfig.AutoSaveInterval))
            {
                UpdateAutoSaveTimer();
            }
        };
    }

    private void InitializeApiService()
    {
        try
        {
            _serviceInitializer.UiHelper.UpdateApiStatus(_serviceInitializer.ApiService.IsApiRunning, _serviceInitializer.ApiService.ApiUrl);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing API service: {ex.Message}");
        }
    }

    private async Task InitializeLicenseSystemAsync()
    {
        await _serviceInitializer.InitializeLicenseSystemAsync();
        Dispatcher.Invoke(() => UpdateLicenseMenuVisibility());
    }

    #endregion

    #region Data Management

    private async void LoadData()
    {
        try
        {
            var success = await _serviceInitializer.TournamentService.LoadDataAsync();
            if (success)
            {
                ConfigureTournamentTabs();
                _hasUnsavedChanges = false;
                _serviceInitializer.UiHelper.UpdateStatusBar(_hasUnsavedChanges);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadData: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Öffentliche Methode um Tournament-Daten neu zu laden und UI zu aktualisieren
    /// Wird von PowerScoring aufgerufen nach Turnier-Erstellung
    /// </summary>
    public void RefreshTournamentData()
    {
        try
        {
            Debug.WriteLine("🔄 RefreshTournamentData called - reloading UI...");

            ConfigureTournamentTabs();

            _hasUnsavedChanges = false;
            _serviceInitializer.UiHelper.UpdateStatusBar(_hasUnsavedChanges);

            _serviceInitializer.TournamentService.TriggerUIRefresh();

            Debug.WriteLine("✅ RefreshTournamentData complete - UI updated");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ RefreshTournamentData ERROR: {ex.Message}");
        }
    }

    private async Task SaveDataInternal()
    {
        try
        {
            var success = await _serviceInitializer.TournamentService.SaveDataAsync();
            if (success)
            {
                _hasUnsavedChanges = false;
                _serviceInitializer.UiHelper.UpdateStatusBar(_hasUnsavedChanges);
            }
        }
        catch
        {
            throw;
        }
    }

    private void MarkAsChanged()
    {
        _hasUnsavedChanges = true;
        _serviceInitializer.UiHelper.UpdateStatusBar(_hasUnsavedChanges);

        if (_serviceInitializer.ConfigService.Config.AutoSave)
        {
            _autoSaveTimer.Stop();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(2);
            _autoSaveTimer.Start();
        }

        // Hub-Synchronisation
        if (_serviceInitializer.HubService.IsRegisteredWithHub && !_serviceInitializer.HubService.IsSyncing)
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                if (!_serviceInitializer.HubService.IsSyncing)
                {
                    await _serviceInitializer.HubService.SyncTournamentAsync(_serviceInitializer.TournamentService.GetTournamentData());
                }
            });
        }
    }

    #endregion

    #region Auto-Save

    private void UpdateAutoSaveTimer()
    {
        _autoSaveTimer.Stop();
        if (_serviceInitializer.ConfigService.Config.AutoSave)
        {
            _autoSaveTimer.Interval = TimeSpan.FromMinutes(_serviceInitializer.ConfigService.Config.AutoSaveInterval);
            _autoSaveTimer.Start();
        }
    }

    private async void AutoSave_Tick(object? sender, EventArgs e)
    {
        if (_hasUnsavedChanges)
        {
            await SaveDataInternal();
            UpdateAutoSaveTimer();
        }
    }

    #endregion

    #region UI Updates

    private void ForceUIUpdate()
    {
        try
        {
            UpdateTranslations();
            _serviceInitializer.UiHelper.UpdateLanguageStatus();
            _serviceInitializer.UiHelper.UpdateStatusBar(_hasUnsavedChanges);

            PlatinTab?.UpdateTranslations();
            GoldTab?.UpdateTranslations();
            SilberTab?.UpdateTranslations();
            BronzeTab?.UpdateTranslations();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainWindow: ForceUIUpdate ERROR: {ex.Message}");
        }
    }

    private void UpdateTranslations()
    {
        Title = _serviceInitializer.LocalizationService.GetString("AppTitle");

        // Menü-Übersetzungen über UI Helper
        _serviceInitializer.UiHelper.UpdateMenuTranslations(
            FileMenuItem, NewMenuItem, OpenMenuItem, SaveMenuItem, SaveAsMenuItem,
            PrintMenuItem, ExitMenuItem, ViewMenuItem, OverviewModeMenuItem,
            TournamentHubMenuItem, RegisterWithHubMenuItem, UnregisterFromHubMenuItem,
            ShowJoinUrlMenuItem, ManualSyncMenuItem, HubSettingsMenuItem,
            LicenseMenuItem, LicenseStatusMenuItem, ActivateLicenseMenuItem, LicenseInfoMenuItem, RemoveLicenseMenuItem, PurchaseLicenseMenuItem,
            AccountMenuItem, LoginMenuItem, ProfileMenuItem, LogoutMenuItem,
            SettingsMenuItem, HelpMenuItem, HelpContentMenuItem, BugReportMenuItem, AboutMenuItem
        );

        _serviceInitializer.UiHelper.UpdateAuthMenu(_serviceInitializer.UserAuthService, AccountMenuItem, LoginMenuItem, ProfileMenuItem, LogoutMenuItem);

        // Tab-Header aktualisieren
        _serviceInitializer.UiHelper.UpdateTabHeaders(PlatinTabItem, GoldTabItem, SilverTabItem, BronzeTabItem);

        // Spenden-Button aktualisieren
        DonationButton.Content = _serviceInitializer.LocalizationService.GetString("Donate");
        DonationButton.ToolTip = _serviceInitializer.LocalizationService.GetString("DonateTooltip");

        // Status aktualisieren
        _serviceInitializer.UiHelper.UpdateLanguageStatus();
        _serviceInitializer.UiHelper.UpdateStatusBar(_hasUnsavedChanges);
        _serviceInitializer.UiHelper.UpdateApiStatus(_serviceInitializer.ApiService.IsApiRunning, _serviceInitializer.ApiService.ApiUrl);

        // Child-Controls aktualisieren
        PlatinTab?.Dispatcher.BeginInvoke(() => PlatinTab?.UpdateTranslations());
        GoldTab?.Dispatcher.BeginInvoke(() => GoldTab?.UpdateTranslations());
        SilberTab?.Dispatcher.BeginInvoke(() => SilberTab?.UpdateTranslations());
        BronzeTab?.Dispatcher.BeginInvoke(() => BronzeTab?.UpdateTranslations());
    }

    private void RefreshMatchDisplays()
    {
        try
        {
            Debug.WriteLine("📊 [REFRESH-DISPLAYS] Match displays refreshed");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ [REFRESH-DISPLAYS] Error: {ex.Message}");
        }
    }

    #endregion

    #region License Management

    private async Task OnLicenseStatusChanged(object? sender, Models.License.LicenseStatus status)
    {
        await Dispatcher.InvokeAsync(() =>
        {
            UpdateLicenseMenuVisibility();

            if (status.IsLicensed)
            {
                Debug.WriteLine($"License active: {status.LicenseType} - {status.CustomerName}");
            }
        });
    }

    private void UpdateLicenseMenuVisibility()
    {
        var hasLicense = _serviceInitializer.LicenseManager.HasLicense();
        var currentStatus = _serviceInitializer.LicenseFeatureService.CurrentStatus;

        LicenseInfoMenuItem.Visibility = hasLicense ? Visibility.Visible : Visibility.Collapsed;
        LicenseSeparator.Visibility = hasLicense ? Visibility.Visible : Visibility.Collapsed;

        if (hasLicense && currentStatus.IsValid)
        {
            ActivateLicenseMenuItem.Header = "🔄 Lizenz erneuern";
        }
        else
        {
            ActivateLicenseMenuItem.Header = "✨ Lizenz aktivieren";
        }
    }

    private void UpdateLicenseMenuVisibility(bool isLicensed)
    {
        try
        {
            LicenseInfoMenuItem.Visibility = isLicensed ? Visibility.Visible : Visibility.Collapsed;
            RemoveLicenseMenuItem.Visibility = isLicensed ? Visibility.Visible : Visibility.Collapsed;
            LicenseSeparator.Visibility = isLicensed ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating license menu visibility: {ex.Message}");
        }
    }

    #endregion

    #region API Event Handlers

    private async Task OnApiMatchResultUpdated(object? sender, MatchResultUpdateEventArgs e)
    {
        await Dispatcher.InvokeAsync(() =>
        {
            try
            {
                var tournamentClass = _serviceInitializer.TournamentService.GetTournamentClassById(e.ClassId);
                tournamentClass?.TriggerUIRefresh();
                MarkAsChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing API match result: {ex.Message}");
            }
        });
    }

    #endregion

    #region Window Lifecycle

    protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        await HandleAppExit();
    }

    private async Task HandleAppExit()
    {
        if (_hasUnsavedChanges)
        {
            var title = _serviceInitializer.LocalizationService.GetString("UnsavedChanges");
            var message = _serviceInitializer.LocalizationService.GetString("SaveBeforeExit");
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        await SaveDataInternal();
                        _serviceInitializer.LicenseManager?.Dispose();
                        Application.Current.Shutdown();
                    }
                    catch
                    {
                        return;
                    }
                    break;
                case MessageBoxResult.No:
                    _serviceInitializer.LicenseManager?.Dispose();
                    Application.Current.Shutdown();
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }
        else
        {
            _serviceInitializer.LicenseManager?.Dispose();
            Application.Current.Shutdown();
        }
    }

    #endregion

    #region Menu Event Handlers - Delegiert an Helper-Klassen

    // File Menu
    private void New_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnNew(sender, e);
    private void Open_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnOpen(sender, e);
    private async void Save_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnSave(sender, e);
    private void SaveAs_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnSaveAs(sender, e);
    private void Print_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnPrint(sender, e);
    private async void Exit_Click(object sender, RoutedEventArgs e) => await HandleAppExit();

    // View Menu
    private void OverviewMode_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnOverviewMode(sender, e);
    private void PowerScoring_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnPowerScoring(sender, e);
    private void ToggleDarkMode_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnToggleDarkMode(sender, e);

    // Settings Menu
    private void Settings_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnSettings(sender, e);

    // Account Menu
    private void Login_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnLogin(sender, e);
    private void Profile_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnProfile(sender, e);
    private async void Logout_Click(object sender, RoutedEventArgs e) => await _eventHandlers.OnLogout(sender, e);

    // Help Menu
    private void Help_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnHelp(sender, e);
    private void BugReport_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnBugReport(sender, e);
    private void About_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnAbout(sender, e);

    // Donation
    private void Donation_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnDonation(sender, e);

    // License Menu
    private void LicenseStatus_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnLicenseStatus(sender, e);
    private async void ActivateLicense_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnActivateLicense(sender, e);
    private void LicenseInfo_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnLicenseInfo(sender, e);
    private void PurchaseLicense_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnPurchaseLicense(sender, e);
    private async void RemoveLicense_Click(object sender, RoutedEventArgs e) => _eventHandlers.OnRemoveLicense(sender, e);

    // Hub Menu - Delegiert an Hub Handlers
    private async void RegisterWithHub_Click(object sender, RoutedEventArgs e) => _hubHandlers.OnRegisterWithHub(sender, e);
    private async void UnregisterFromHub_Click(object sender, RoutedEventArgs e) => _hubHandlers.OnUnregisterFromHub(sender, e);
    private async void ManualSyncWithHub_Click(object sender, RoutedEventArgs e) => _hubHandlers.OnManualSyncWithHub(sender, e);
    private void ShowJoinUrl_Click(object sender, RoutedEventArgs e) => _hubHandlers.OnShowJoinUrl(sender, e);
    private void HubSettings_Click(object sender, RoutedEventArgs e) => _hubHandlers.OnHubSettings(sender, e);
    private void HubStatus_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) => _hubHandlers.OnHubStatusClick(sender, e);

    // API Menu - Delegiert an Event Handlers (diese könnten auch in separate Klasse)
    private async void StartApi_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var success = await _serviceInitializer.ApiService.StartApiAsync(_serviceInitializer.TournamentService.GetTournamentData(), 5000);

            var title = success ?
                (_serviceInitializer.LocalizationService.GetString("Success") ?? "Erfolgreich") :
                (_serviceInitializer.LocalizationService.GetString("Error") ?? "Fehler");

            var message = success ?
                (_serviceInitializer.LocalizationService.GetString("APIStarted") ?? $"API erfolgreich gestartet!\n\nURL: {_serviceInitializer.ApiService.ApiUrl}") :
                (_serviceInitializer.LocalizationService.GetString("APIStartError") ?? "API konnte nicht gestartet werden.");

            MessageBox.Show(message, title, MessageBoxButton.OK,
                success ? MessageBoxImage.Information : MessageBoxImage.Error);

            _serviceInitializer.UiHelper.UpdateApiStatus(_serviceInitializer.ApiService.IsApiRunning, _serviceInitializer.ApiService.ApiUrl);
        }
        catch (Exception ex)
        {
            var title = _serviceInitializer.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Starten der API: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void StopApi_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var success = await _serviceInitializer.ApiService.StopApiAsync();

            if (success)
            {
                var title = _serviceInitializer.LocalizationService.GetString("Success") ?? "Erfolgreich";
                var message = _serviceInitializer.LocalizationService.GetString("APIStopped") ?? "API gestoppt.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _serviceInitializer.UiHelper.UpdateApiStatus(_serviceInitializer.ApiService.IsApiRunning, _serviceInitializer.ApiService.ApiUrl);
        }
        catch (Exception ex)
        {
            var title = _serviceInitializer.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Stoppen der API: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenApiDocs_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_serviceInitializer.ApiService.IsApiRunning)
            {
                var title = _serviceInitializer.LocalizationService.GetString("Information") ?? "Information";
                var message = "API ist nicht gestartet. Starten Sie die API zuerst.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_serviceInitializer.ApiService.ApiUrl != null)
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _serviceInitializer.ApiService.ApiUrl,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
            }
        }
        catch (Exception ex)
        {
            var title = _serviceInitializer.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Browsers: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    private void ViewMenuItem_Click()
    {

    }
}