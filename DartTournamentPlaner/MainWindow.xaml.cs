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
    private readonly LicensedHubService _hubService;  // NEU: Lizenzierter Hub Service
    private readonly HubMatchProcessingService _hubMatchProcessor;
    private readonly MainWindowUIHelper _uiHelper;

    // NEU: License Services
    private readonly Services.License.LicenseManager _licenseManager;
    private readonly LicenseFeatureService _licenseFeatureService;

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
        
        // NEU: License Services initialisieren
        _licenseManager = new Services.License.LicenseManager();
        _licenseFeatureService = new LicenseFeatureService(_licenseManager);
        
        // NEU: Ersetze API Service durch lizenzierten API Service
        var originalApiService = App.ApiIntegrationService ?? throw new InvalidOperationException("ApiIntegrationService not initialized");
        _apiService = new LicensedApiIntegrationService(_licenseFeatureService, _localizationService);
        
        // Neue Services initialisieren
        _tournamentService = new TournamentManagementService(_localizationService, App.DataService!);
        
        // NEU: Erstelle lizenzierten Hub Service
        var innerHubService = new HubIntegrationService(_configService, _localizationService, _apiService, Dispatcher);
        _hubService = new LicensedHubService(innerHubService, _licenseFeatureService, _localizationService, _licenseManager);
        
        _hubMatchProcessor = new HubMatchProcessingService(_tournamentService.GetTournamentClassById);
        _uiHelper = new MainWindowUIHelper(_localizationService, Dispatcher);
        
        // Services konfigurieren und starten
        InitializeServices();
        InitializeAutoSave();
        InitializeApiService();
        
        UpdateTranslations();
        LoadData();
        
        _ = Task.Run(async () => await _hubService.InitializeAsync());
        
        // NEU: Lizenz-System initialisieren
        _ = InitializeLicenseSystemAsync();
    }

    private void InitializeServices()
    {
        // UI Helper konfigurieren
        _uiHelper.HubStatusIndicator = HubStatusIndicator;
        _uiHelper.HubStatusText = HubStatusText;
        _uiHelper.HubSyncStatus = HubSyncStatus;
        //_uiHelper.ApiStatusIndicator = ApiStatusIndicator;
        //_uiHelper.ApiStatusText = ApiStatusText;
        _uiHelper.StatusTextBlock = StatusTextBlock;
        _uiHelper.LastSavedBlock = LastSavedBlock;
        _uiHelper.LanguageStatusBlock = LanguageStatusBlock;
        //_uiHelper.ApiStatusMenuItem = ApiStatusMenuItem;
        //_uiHelper.StartApiMenuItem = StartApiMenuItem;
        //_uiHelper.StopApiMenuItem = StopApiMenuItem;
        //_uiHelper.OpenApiDocsMenuItem = OpenApiDocsMenuItem;

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

        // NEU: License Service Events
        _licenseFeatureService.LicenseStatusChanged += OnLicenseStatusChanged;

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
            TournamentHubMenuItem, RegisterWithHubMenuItem, UnregisterFromHubMenuItem,
            ShowJoinUrlMenuItem, ManualSyncMenuItem, HubSettingsMenuItem,
            LicenseMenuItem, LicenseStatusMenuItem, ActivateLicenseMenuItem, LicenseInfoMenuItem, RemoveLicenseMenuItem, PurchaseLicenseMenuItem,
            SettingsMenuItem, HelpMenuItem, HelpContentMenuItem, BugReportMenuItem, AboutMenuItem
        );
        //ApiMenuItem, StartApiMenuItem, StopApiMenuItem, OpenApiDocsMenuItem,

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
            _hubService.LastSyncTime ?? DateTime.MinValue
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
        var settingsWindow = new SettingsWindow(_configService, _localizationService, App.ThemeService);
        settingsWindow.Owner = this;
        
        if (settingsWindow.ShowDialog() == true)
        {
            UpdateAutoSaveTimer();
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AboutDialog.ShowDialog(this, _localizationService);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("About");
            var message = _localizationService.GetString("AboutText");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
                        // NEU: License Manager disposing
                        _licenseManager?.Dispose();
                        Application.Current.Shutdown();
                    }
                    catch
                    {
                        return;
                    }
                    break;
                case MessageBoxResult.No:
                    // NEU: License Manager disposing
                    _licenseManager?.Dispose();
                    Application.Current.Shutdown();
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }
        else
        {
            // NEU: License Manager disposing
            _licenseManager?.Dispose();
            Application.Current.Shutdown();
        }
    }

    // NEU: Lizenz-System Initialisierung
    private async Task InitializeLicenseSystemAsync()
    {
        try
        {
            await _licenseFeatureService.InitializeAsync();
            
            // UI für Lizenz-Status aktualisieren
            Dispatcher.Invoke(() => UpdateLicenseMenuVisibility());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"License system initialization error: {ex.Message}");
        }
    }
    
    // NEU: Lizenz-Status Event Handler
    private void OnLicenseStatusChanged(object? sender, Models.License.LicenseStatus status)
    {
        Dispatcher.Invoke(() =>
        {
            UpdateLicenseMenuVisibility();
            
            // Optional: Status in der Statusleiste anzeigen
            if (status.IsLicensed)
            {
                System.Diagnostics.Debug.WriteLine($"License active: {status.LicenseType} - {status.CustomerName}");
            }
        });
    }
    
    // NEU: Lizenz-Menü Sichtbarkeit aktualisieren
    private void UpdateLicenseMenuVisibility()
    {
        var hasLicense = _licenseManager.HasLicense();
        var currentStatus = _licenseFeatureService.CurrentStatus;
        
        // Lizenz-Info nur anzeigen wenn Lizenz vorhanden
        LicenseInfoMenuItem.Visibility = hasLicense ? Visibility.Visible : Visibility.Collapsed;
        LicenseSeparator.Visibility = hasLicense ? Visibility.Visible : Visibility.Collapsed;
        
        // Aktivierung-Text ändern basierend auf Status
        if (hasLicense && currentStatus.IsValid)
        {
            ActivateLicenseMenuItem.Header = "🔄 Lizenz erneuern";
        }
        else
        {
            ActivateLicenseMenuItem.Header = "✨ Lizenz aktivieren";
        }
    }
    
    // NEU: Lizenz Event Handlers
    
    private void LicenseStatus_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var statusWindow = new Views.License.LicenseStatusWindow(_licenseManager, _licenseFeatureService, _localizationService);
            statusWindow.Owner = this;
            statusWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Lizenz-Status: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private async void ActivateLicense_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new SimpleLicenseActivationDialog(_localizationService, _licenseManager);
            
            if (await dialog.ShowDialogAsync())
            {
                // Lizenz wurde erfolgreich aktiviert
                UpdateLicenseMenuVisibility();
                
                // Feature Service Status aktualisieren
                await _licenseFeatureService.InitializeAsync();
                
                System.Diagnostics.Debug.WriteLine($"✅ License activated and UI updated");
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Aktivieren der Lizenz: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void LicenseInfo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new SimpleLicenseInfoDialog(_licenseManager, _licenseFeatureService, _localizationService);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Anzeigen der Lizenz-Informationen: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void PurchaseLicense_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var purchaseDialog = new Views.License.PurchaseLicenseDialog(_localizationService, _licenseManager);
            purchaseDialog.Owner = this;
            purchaseDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Lizenzkauf-Dialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OverviewMode_Click(object sender, RoutedEventArgs e)
    {
        _tournamentService.ShowTournamentOverview(this, _licenseFeatureService, _licenseManager);
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
        try
        {
            // TEMPORARY DEBUG: Zeige Debug-Dialog vor dem Print
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift))
            {
                Views.License.LicenseDebugDialog.ShowDebugDialog(this, _licenseFeatureService, _licenseManager, _localizationService);
                return;
            }
            
            // ✅ Hole den inneren HubIntegrationService direkt vom LicensedHubService
            HubIntegrationService? hubIntegrationService = _hubService?.InnerHubService;
        
            // ⭐ NEU: Hole Tournament-ID aus TournamentData
            string? tournamentId = _tournamentService.GetTournamentData()?.TournamentId;
        
            if (hubIntegrationService != null)
            {
                System.Diagnostics.Debug.WriteLine("[Print_Click] HubIntegrationService erfolgreich extrahiert für QR-Codes");
                System.Diagnostics.Debug.WriteLine($"[Print_Click] Hub registered: {hubIntegrationService.IsRegisteredWithHub}");
                System.Diagnostics.Debug.WriteLine($"[Print_Click] Tournament ID: {tournamentId ?? "null"}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[Print_Click] Kein HubIntegrationService verfügbar - Drucke ohne QR-Codes");
            }
            
            // ✅ FIXED: Verwende PrintHelper mit Tournament-ID
            Helpers.PrintHelper.ShowPrintDialog(
                _tournamentService.AllTournamentClasses, 
                _tournamentService.PlatinClass, 
                this, 
                _localizationService,
                _licenseFeatureService,  // LicenseFeatureService für Lizenzprüfung
                _licenseManager,         // LicenseManager für Dialog
                hubIntegrationService,   // HubService für QR-Codes
                tournamentId);         // ⭐ NEU: Tournament-ID für QR-Code URLs
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Druckdialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            System.Diagnostics.Debug.WriteLine($"[Print_Click] Fehler: {ex.Message}\n{ex.StackTrace}");
        }
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
        // ⭐ NEU: Verwende neuen HubRegistrationDialog mit custom ID Support
      var success = HubRegistrationDialog.ShowDialog(
      this,
        _hubService,
 _tournamentService,
         MarkAsChanged,
    _localizationService);

        if (success)
 {
   System.Diagnostics.Debug.WriteLine("✅ [RegisterWithHub_Click] Hub registration successful");
 }
        else
        {
    System.Diagnostics.Debug.WriteLine("⚠️ [RegisterWithHub_Click] Hub registration cancelled or failed");
        }
    }

    private async void UnregisterFromHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_hubService.IsRegisteredWithHub)
            {
                var infoTitle = _localizationService.GetString("Information");
                var infoMessage = _localizationService.GetString("NoTournamentRegistered");
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmTitle = _localizationService.GetString("UnregisterTournamentTitle");
            var confirmMessage = _localizationService.GetString("UnregisterTournamentConfirm", 
                _hubService.GetCurrentTournamentId());
            
            var result = MessageBox.Show(confirmMessage, confirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                await _hubService.UnregisterTournamentAsync();
                
                var title = _localizationService.GetString("Success");
                var message = _localizationService.GetString("UnregisterTournamentSuccess");
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = _localizationService.GetString("UnregisterTournamentError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ManualSyncWithHub_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!_hubService.IsRegisteredWithHub)
            {
                var infoTitle = _localizationService.GetString("Information");
                var infoMessage = _localizationService.GetString("NoTournamentRegistered");
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var success = await _hubService.SyncTournamentAsync(_tournamentService.GetTournamentData());
            
            var title = success ?
                _localizationService.GetString("Success") :
                _localizationService.GetString("Error");
            
            var message = success ?
                _localizationService.GetString("SyncSuccess") :
                _localizationService.GetString("SyncError");
            
            MessageBox.Show(message, title, MessageBoxButton.OK, 
                success ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = _localizationService.GetString("ManualSyncError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowJoinUrl_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_hubService.GetCurrentTournamentId()))
            {
                var infoTitle = _localizationService.GetString("Information");
                var infoMessage = _localizationService.GetString("NoTournamentRegistered");
                MessageBox.Show(infoMessage, infoTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var joinUrl = _hubService.GetJoinUrl();
            
            var dialogTitle = _localizationService.GetString("JoinUrlTitle");
            var dialogMessage = _localizationService.GetString("JoinUrlMessage", 
                _hubService.GetCurrentTournamentId(), joinUrl);
            
            MessageBox.Show(dialogMessage, dialogTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            System.Windows.Clipboard.SetText(joinUrl);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = _localizationService.GetString("JoinUrlError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void HubSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var currentHubUrl = _hubService.TournamentHubService.HubUrl;
            
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                _localizationService.GetString("HubSettingsPrompt"),
                _localizationService.GetString("HubSettingsTitle"),
                currentHubUrl
            );
            
            if (!string.IsNullOrWhiteSpace(input) && input != currentHubUrl)
            {
                _hubService.UpdateHubUrl(input);
                
                var title = _localizationService.GetString("Success");
                var message = _localizationService.GetString("HubUrlUpdated", input);
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = _localizationService.GetString("HubSettingsError", ex.Message);
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
                globalDebugWindow?.AddDebugMessage(_localizationService.GetString("DebugConsoleStarted"), "INFO");
                System.Diagnostics.Debug.WriteLine("🔍 Global Debug Console opened via MainWindow");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error toggling Hub Debug Console: {ex.Message}");
            var title = _localizationService.GetString("Error");
            var message = _localizationService.GetString("HubSettingsError", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    // ✅ NEU: Dark Mode Toggle Event Handler
    private void ToggleDarkMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            App.ThemeService?.ToggleTheme();
            
            // Menü-Text aktualisieren basierend auf aktuellem Theme
            var currentTheme = App.ThemeService?.GetCurrentTheme() ?? "Light";
            var isDark = currentTheme.ToLower() == "dark";
            var newText = isDark ? "☀️ Switch to Light Mode" : "🌙 Switch to Dark Mode";
            
            // Versuche Übersetzung zu finden
            var translationKey = isDark ? "SwitchToLightMode" : "SwitchToDarkMode";
            var translatedText = _localizationService.GetString(translationKey);
            
            if (!string.IsNullOrEmpty(translatedText) && translatedText != translationKey)
            {
                newText = isDark ? "☀️ " + translatedText : "🌙 " + translatedText;
            }
            
            if (sender is MenuItem menuItem)
            {
                menuItem.Header = newText;
            }
            
            System.Diagnostics.Debug.WriteLine($"🎨 Theme toggled to: {currentTheme}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error toggling dark mode: {ex.Message}");
            
            var title = _localizationService.GetString("Error") ?? "Error";
            var message = $"Error switching theme: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    
    private async void RemoveLicense_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _localizationService.GetString("RemoveLicense") ?? "Lizenz entfernen";
            var message = _localizationService.GetString("RemoveLicenseConfirmation") ?? 
                "Möchten Sie die aktivierte Lizenz wirklich entfernen?\n\n" +
                "• Die Anwendung wird danach als unlizenziert ausgeführt\n" +
                "• Alle Core-Features bleiben verfügbar\n" +
                "• Sie können jederzeit eine neue Lizenz aktivieren\n\n" +
                "Fortfahren?";
            
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            
            if (result == MessageBoxResult.Yes)
            {
                // Progress anzeigen
                var progressDialog = new SimpleLicenseActivationDialog(_localizationService, _licenseManager);
                
                // Lizenz entfernen
                var success = await _licenseManager.RemoveLicenseAndResetAsync();
                
                if (success)
                {
                    // LicenseFeatureService aktualisieren
                    await _licenseFeatureService.InitializeAsync();
                    
                    // UI-Status aktualisieren
                    UpdateLicenseMenuVisibility(false);
                    
                    var successTitle = _localizationService.GetString("Success") ?? "Erfolg";
                    var successMessage = _localizationService.GetString("LicenseRemovedSuccess") ?? 
                        "✅ Lizenz wurde erfolgreich entfernt!\n\n" +
                        "Die Anwendung läuft jetzt im unlizenzierte Modus mit allen Core-Features.\n" +
                        "Sie können jederzeit über das Lizenz-Menü eine neue Lizenz aktivieren.";
                    
                    MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errorTitle = _localizationService.GetString("Error") ?? "Fehler";
                    var errorMessage = _localizationService.GetString("LicenseRemoveError") ?? 
                        "❌ Fehler beim Entfernen der Lizenz.\n\nBitte versuchen Sie es erneut oder kontaktieren Sie den Support.";
                    
                    MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _localizationService.GetString("Error") ?? "Fehler";
            var errorMessage = $"Unerwarteter Fehler beim Entfernen der Lizenz:\n\n{ex.Message}";
            
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Aktualisiert die Sichtbarkeit der Lizenz-Menüeinträge basierend auf dem Lizenz-Status
    /// </summary>
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
}