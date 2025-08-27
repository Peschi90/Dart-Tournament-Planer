using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using DartTournamentPlaner.Helpers;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.Json;

namespace DartTournamentPlaner;

/// <summary>
/// Vereinfachtes Code-Behind für das Hauptfenster
/// Delegiert die meiste Logik an spezialisierte Services
/// </summary>
public partial class MainWindow : Window
{
    // Services
    private readonly ConfigService _configService;
    private readonly LocalizationService _localizationService;
    private readonly IApiIntegrationService _apiService;
    
    // Neue Services
    private readonly TournamentManagementService _tournamentService;
    private readonly HubIntegrationService _hubService;
    private readonly HubMatchProcessingService _hubMatchProcessor;
    private readonly MainWindowUIHelper _uiHelper;

    // Auto-Save System
    private readonly DispatcherTimer _autoSaveTimer = new DispatcherTimer();
    private bool _hasUnsavedChanges = false;

    // Properties für Legacy-Kompatibilität
    public ITournamentHubService? TournamentHubService => _hubService.TournamentHubService;
    public string GetCurrentTournamentId() => _hubService.GetCurrentTournamentId();
    public bool IsRegisteredWithHub => _hubService.IsRegisteredWithHub;

    /// <summary>
    /// Konstruktor des Hauptfensters - vereinfacht durch Service-Delegation
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        
        // Services aus App.xaml.cs holen
        _configService = App.ConfigService ?? throw new InvalidOperationException("ConfigService not initialized");
        _localizationService = App.LocalizationService ?? throw new InvalidOperationException("LocalizationService not initialized");
        _apiService = App.ApiIntegrationService ?? throw new InvalidOperationException("ApiIntegrationService not initialized");

        // Neue Services initialisieren
        _tournamentService = new TournamentManagementService(_localizationService, App.DataService!);
        _hubService = new HubIntegrationService(_configService, _localizationService, _apiService, Dispatcher);
        _hubMatchProcessor = new HubMatchProcessingService(_tournamentService.GetTournamentClassById);
        _uiHelper = new MainWindowUIHelper(_localizationService, Dispatcher);
        
        // Services konfigurieren und starten
        InitializeServices();
        InitializeAutoSave();
        InitializeApiService();
        
        UpdateTranslations();
        LoadData();
        
        _ = Task.Run(async () => await _hubService.InitializeAsync());
    }

    private void InitializeServices()
    {
        // UI Helper konfigurieren
        _uiHelper.HubStatusIndicator = HubStatusIndicator;
        _uiHelper.HubStatusText = HubStatusText;
        _uiHelper.HubSyncStatus = HubSyncStatus;
        _uiHelper.ApiStatusIndicator = ApiStatusIndicator;
        _uiHelper.ApiStatusText = ApiStatusText;
        _uiHelper.StatusTextBlock = StatusTextBlock;
        _uiHelper.LastSavedBlock = LastSavedBlock;
        _uiHelper.LanguageStatusBlock = LanguageStatusBlock;
        _uiHelper.ApiStatusMenuItem = ApiStatusMenuItem;
        _uiHelper.StartApiMenuItem = StartApiMenuItem;
        _uiHelper.StopApiMenuItem = StopApiMenuItem;
        _uiHelper.OpenApiDocsMenuItem = OpenApiDocsMenuItem;

        // Event-Handler
        _localizationService.PropertyChanged += (s, e) => UpdateTranslations();
        _configService.LanguageChanged += (s, language) => 
        {
            _localizationService.SetLanguage(language);
            Dispatcher.BeginInvoke(() =>
            {
                _uiHelper.UpdateLanguageStatus();
                UpdateTranslations();
                ForceUIUpdate();
            }, DispatcherPriority.Render);
        };

        // Tournament Service Events
        _tournamentService.DataChanged += () => MarkAsChanged();

        // Hub Service Events
        _hubService.MatchResultReceived += OnHubMatchResultReceived;
        _hubService.HubStatusChanged += OnHubStatusChanged;
        _hubService.DataChanged += () => MarkAsChanged();

        // Tournament Tabs konfigurieren
        ConfigureTournamentTabs();
    }

    private void ConfigureTournamentTabs()
    {
        PlatinTab.TournamentClass = _tournamentService.PlatinClass;
        GoldTab.TournamentClass = _tournamentService.GoldClass;
        SilberTab.TournamentClass = _tournamentService.SilberClass;
        BronzeTab.TournamentClass = _tournamentService.BronzeClass;

        PlatinTab.DataChanged += (s, e) => MarkAsChanged();
        GoldTab.DataChanged += (s, e) => MarkAsChanged();
        SilberTab.DataChanged += (s, e) => MarkAsChanged();
        BronzeTab.DataChanged += (s, e) => MarkAsChanged();
    }

    private void InitializeAutoSave()
    {
        _autoSaveTimer.Tick += AutoSave_Tick;
        UpdateAutoSaveTimer();
        
        _configService.Config.PropertyChanged += (s, e) =>
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
            _apiService.MatchResultUpdated += OnApiMatchResultUpdated;
            _uiHelper.UpdateApiStatus(_apiService.IsApiRunning, _apiService.ApiUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing API service: {ex.Message}");
        }
    }

    private void ForceUIUpdate()
    {
        try
        {
            UpdateTranslations();
            _uiHelper.UpdateLanguageStatus();
            _uiHelper.UpdateStatusBar(_hasUnsavedChanges);
            
            PlatinTab?.UpdateTranslations();
            GoldTab?.UpdateTranslations();
            SilberTab?.UpdateTranslations();
            BronzeTab?.UpdateTranslations();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow: ForceUIUpdate ERROR: {ex.Message}");
        }
    }

    private void UpdateAutoSaveTimer()
    {
        _autoSaveTimer.Stop();
        if (_configService.Config.AutoSave)
        {
            _autoSaveTimer.Interval = TimeSpan.FromMinutes(_configService.Config.AutoSaveInterval);
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

    private async void LoadData()
    {
        try
        {
            var success = await _tournamentService.LoadDataAsync();
            if (success)
            {
                ConfigureTournamentTabs(); // Tabs mit geladenen Daten neu konfigurieren
                _hasUnsavedChanges = false;
                _uiHelper.UpdateStatusBar(_hasUnsavedChanges);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadData: ERROR: {ex.Message}");
        }
    }

    private async Task SaveDataInternal()
    {
        try
        {
            var success = await _tournamentService.SaveDataAsync();
            if (success)
            {
                _hasUnsavedChanges = false;
                _uiHelper.UpdateStatusBar(_hasUnsavedChanges);
            }
        }
        catch
        {
            throw; // Tournament Service zeigt bereits Fehler-Dialog
        }
    }

    private void MarkAsChanged()
    {
        _hasUnsavedChanges = true;
        _uiHelper.UpdateStatusBar(_hasUnsavedChanges);
        
        if (_configService.Config.AutoSave)
        {
            _autoSaveTimer.Stop();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(2);
            _autoSaveTimer.Start();
        }
        
        // Hub-Synchronisation
        if (_hubService.IsRegisteredWithHub && !_hubService.IsSyncing)
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                if (!_hubService.IsSyncing)
                {
                    await _hubService.SyncTournamentAsync(_tournamentService.GetTournamentData());
                }
            });
        }
    }

    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("AppTitle");
        
        // Menü-Übersetzungen über UI Helper
        _uiHelper.UpdateMenuTranslations(
            FileMenuItem, NewMenuItem, OpenMenuItem, SaveMenuItem, SaveAsMenuItem, 
            PrintMenuItem, ExitMenuItem, ViewMenuItem, OverviewModeMenuItem,
            ApiMenuItem, StartApiMenuItem, StopApiMenuItem, OpenApiDocsMenuItem,
            TournamentHubMenuItem, RegisterWithHubMenuItem, UnregisterFromHubMenuItem,
            ShowJoinUrlMenuItem, ManualSyncMenuItem, HubSettingsMenuItem,
            SettingsMenuItem, HelpMenuItem, HelpContentMenuItem, BugReportMenuItem, AboutMenuItem
        );

        // Tab-Header aktualisieren
        _uiHelper.UpdateTabHeaders(PlatinTabItem, GoldTabItem, SilverTabItem, BronzeTabItem);

        // Spenden-Button aktualisieren
        DonationButton.Content = _localizationService.GetString("Donate");
        DonationButton.ToolTip = _localizationService.GetString("DonateTooltip");

        // Status aktualisieren
        _uiHelper.UpdateLanguageStatus();
        _uiHelper.UpdateStatusBar(_hasUnsavedChanges);
        _uiHelper.UpdateApiStatus(_apiService.IsApiRunning, _apiService.ApiUrl);
        
        // Child-Controls aktualisieren
        PlatinTab?.Dispatcher.BeginInvoke(() => PlatinTab?.UpdateTranslations());
        GoldTab?.Dispatcher.BeginInvoke(() => GoldTab?.UpdateTranslations());
        SilberTab?.Dispatcher.BeginInvoke(() => SilberTab?.UpdateTranslations());
        BronzeTab?.Dispatcher.BeginInvoke(() => BronzeTab?.UpdateTranslations());
    }

    // Event Handlers - vereinfacht

    private void OnHubMatchResultReceived(HubMatchUpdateEventArgs e)
    {
        var success = _hubMatchProcessor.ProcessHubMatchUpdate(e, out var errorMessage);
        
        if (success)
        {
            MarkAsChanged();
            ShowToastNotification("Match Update", $"Match {e.MatchId} aktualisiert", "Hub");
        }
        else if (!string.IsNullOrEmpty(errorMessage))
        {
            System.Diagnostics.Debug.WriteLine($"❌ Hub Match Update Error: {errorMessage}");
        }
    }

    private void OnHubStatusChanged(bool isConnected)
    {
        _uiHelper.UpdateHubStatus(
            isConnected, 
            _hubService.GetCurrentTournamentId(),
            _hubService.IsSyncing,
            _hubService.LastSyncTime
        );
    }

    private void OnApiMatchResultUpdated(object? sender, MatchResultUpdateEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                var tournamentClass = _tournamentService.GetTournamentClassById(e.ClassId);
                tournamentClass?.TriggerUIRefresh();
                MarkAsChanged();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing API match result: {ex.Message}");
            }
        });
    }

    private void ShowToastNotification(string title, string message, string source)
    {
        System.Diagnostics.Debug.WriteLine($"🔔 {title}: {message} (Source: {source})");
    }

    // Menü Event Handlers - vereinfacht

    private void New_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService.GetString("NewTournament");
        var message = _localizationService.GetString("CreateNewTournament");
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            _tournamentService.ResetAllTournaments();
            ConfigureTournamentTabs();
            _hasUnsavedChanges = false;
            _uiHelper.UpdateStatusBar(_hasUnsavedChanges);
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Tournament files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            var title = _localizationService.GetString("Information");
            var message = _localizationService.GetString("CustomFileNotImplemented");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await SaveDataInternal();
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Tournament files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            var title = _localizationService.GetString("Information");
            var message = _localizationService.GetString("CustomFileSaveNotImplemented");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_configService, _localizationService);
        settingsWindow.Owner = this;
        
        if (settingsWindow.ShowDialog() == true)
        {
            UpdateAutoSaveTimer();
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService.GetString("About");
        var message = _localizationService.GetString("AboutText");
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var helpWindow = new HelpWindow(_localizationService);
            helpWindow.Owner = this;
            helpWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("ErrorOpeningHelp")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Exit_Click(object sender, RoutedEventArgs e)
    {
        await HandleAppExit();
    }

    protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        await HandleAppExit();
    }

    private async Task HandleAppExit()
    {
        if (_hasUnsavedChanges)
        {
            var title = _localizationService.GetString("UnsavedChanges");
            var message = _localizationService.GetString("SaveBeforeExit");
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        await SaveDataInternal();
                        Application.Current.Shutdown();
                    }
                    catch
                    {
                        return;
                    }
                    break;
                case MessageBoxResult.No:
                    Application.Current.Shutdown();
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }
        else
        {
            Application.Current.Shutdown();
        }
    }

    private void OverviewMode_Click(object sender, RoutedEventArgs e)
    {
        _tournamentService.ShowTournamentOverview();
    }

    private void BugReport_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var bugReportDialog = new BugReportDialog(_localizationService);
            bugReportDialog.Owner = this;
            bugReportDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("Error")}: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Donation_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var donationDialog = new DonationDialog(_localizationService);
            donationDialog.Owner = this;
            donationDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("Error")}: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        _tournamentService.ShowPrintDialog();
    }

    // API Event Handlers

    private async void StartApi_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var success = await _apiService.StartApiAsync(_tournamentService.GetTournamentData(), 5000);
            
            var title = success ? 
                (_localizationService.GetString("Success") ?? "Erfolgreich") :
                (_localizationService.GetString("Error") ?? "Fehler");
            
            var message = success ?
                (_localizationService.GetString("APIStarted") ?? $"API erfolgreich gestartet!\n\nURL: {_apiService.ApiUrl}") :
                (_localizationService.GetString("APIStartError") ?? "API konnte nicht gestartet werden.");
            
            MessageBox.Show(message, title, MessageBoxButton.OK, 
                success ? MessageBoxImage.Information : MessageBoxImage.Error);
            
            _uiHelper.UpdateApiStatus(_apiService.IsApiRunning, _apiService.ApiUrl);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Starten der API: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void StopApi_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var success = await _apiService.StopApiAsync();
            
            if (success)
            {
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = _localizationService.GetString("APIStopped") ?? "API gestoppt.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            
            _uiHelper.UpdateApiStatus(_apiService.IsApiRunning, _apiService.ApiUrl);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Stoppen der API: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenApiDocs_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_apiService.IsApiRunning)
            {
                var title = _localizationService.GetString("Information") ?? "Information";
                var message = "API ist nicht gestartet. Starten Sie die API zuerst.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_apiService.ApiUrl != null)
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _apiService.ApiUrl,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Browsers: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Tournament Hub Event Handlers

    private async void RegisterWithHub_Click(object sender, RoutedEventArgs e)
    {
        var success = await _hubService.RegisterTournamentAsync();
        
        if (success)
        {
            await _hubService.SyncTournamentAsync(_tournamentService.GetTournamentData());
            
            var joinUrl = _hubService.GetJoinUrl();
            var title = _localizationService.GetString("Success") ?? "Erfolgreich";
            var message = $"🎯 Tournament erfolgreich beim Hub registriert!\n\n" +
                         $"Tournament ID: {_hubService.GetCurrentTournamentId()}\n" +
                         $"Join URL: {joinUrl}\n\n" +
                         $"Diese URL können Sie an Spieler senden.";
            
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            System.Windows.Clipboard.SetText(joinUrl);
        }
        else
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = "❌ Tournament konnte nicht beim Hub registriert werden.";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void UnregisterFromHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_hubService.IsRegisteredWithHub)
            {
                var infoTitle = _localizationService.GetString("Information") ?? "Information";
                var infoMessage = "Kein Tournament beim Hub registriert.";
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmTitle = "Tournament entregistrieren";
            var confirmMessage = $"Tournament '{_hubService.GetCurrentTournamentId()}' wirklich vom Hub entregistrieren?";
            
            var result = MessageBox.Show(confirmMessage, confirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                await _hubService.UnregisterTournamentAsync();
                
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = "Tournament erfolgreich vom Hub entregistriert.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Entregistrieren: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ManualSyncWithHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_hubService.IsRegisteredWithHub)
            {
                var infoTitle = _localizationService.GetString("Information") ?? "Information";
                var infoMessage = "Kein Tournament beim Hub registriert.";
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var success = await _hubService.SyncTournamentAsync(_tournamentService.GetTournamentData());
            
            var title = success ?
                (_localizationService.GetString("Success") ?? "Erfolgreich") :
                (_localizationService.GetString("Error") ?? "Fehler");
            
            var message = success ?
                "Tournament erfolgreich mit Hub synchronisiert!" :
                "Fehler beim Synchronisieren mit Hub.";
            
            MessageBox.Show(message, title, MessageBoxButton.OK, 
                success ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim manuellen Sync: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowJoinUrl_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_hubService.GetCurrentTournamentId()))
            {
                var infoTitle = _localizationService.GetString("Information") ?? "Information";
                var infoMessage = "Kein Tournament beim Hub registriert.";
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var joinUrl = _hubService.GetJoinUrl();
            
            var dialogTitle = "Tournament Join URL";
            var dialogMessage = $"Tournament ID: {_hubService.GetCurrentTournamentId()}\n\n" +
                               $"Join URL:\n{joinUrl}\n\n" +
                               $"Diese URL können Sie an Spieler senden.";
            
            MessageBox.Show(dialogMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            System.Windows.Clipboard.SetText(joinUrl);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Anzeigen der Join-URL: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void HubSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentHubUrl = _hubService.TournamentHubService.HubUrl;
            
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Geben Sie die Tournament Hub URL ein:",
                "Hub-Einstellungen",
                currentHubUrl
            );
            
            if (!string.IsNullOrWhiteSpace(input) && input != currentHubUrl)
            {
                _hubService.UpdateHubUrl(input);
                
                var title = _localizationService.GetString("Success") ?? "Erfolgreich";
                var message = $"Hub-URL aktualisiert:\n{input}";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler bei den Hub-Einstellungen: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void HubStatus_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            // Verwende die globale Debug Console vom HubIntegrationService
            var globalDebugWindow = HubIntegrationService.GlobalDebugWindow;
            
            if (globalDebugWindow?.IsVisible == true)
            {
                globalDebugWindow.Hide();
                System.Diagnostics.Debug.WriteLine("🔍 Global Debug Console hidden via MainWindow");
            }
            else
            {
                globalDebugWindow?.Show();
                globalDebugWindow?.AddDebugMessage("🔍 Debug Console geöffnet via MainWindow", "INFO");
                System.Diagnostics.Debug.WriteLine("🔍 Global Debug Console opened via MainWindow");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error toggling Hub Debug Console: {ex.Message}");
            MessageBox.Show($"Fehler beim Öffnen der Debug Console: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}