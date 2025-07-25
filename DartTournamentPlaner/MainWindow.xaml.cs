using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using Microsoft.Win32;

namespace DartTournamentPlaner;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ConfigService _configService;
    private readonly LocalizationService _localizationService;
    private readonly DataService _dataService;
    private readonly DispatcherTimer _autoSaveTimer = new DispatcherTimer();
    private bool _hasUnsavedChanges = false;

    public MainWindow()
    {
        InitializeComponent();
        
        _configService = App.ConfigService ?? throw new InvalidOperationException("ConfigService not initialized");
        _localizationService = App.LocalizationService ?? throw new InvalidOperationException("LocalizationService not initialized");
        _dataService = App.DataService ?? throw new InvalidOperationException("DataService not initialized");

        InitializeTournamentClasses();
        InitializeServices();
        InitializeAutoSave();
        UpdateTranslations();
        LoadData();
    }

    private void InitializeTournamentClasses()
    {
        // Initialize each tournament class with its name
        PlatinTab.TournamentClass = new TournamentClass { Id = 1, Name = "Platin" };
        GoldTab.TournamentClass = new TournamentClass { Id = 2, Name = "Gold" };
        SilberTab.TournamentClass = new TournamentClass { Id = 3, Name = "Silber" };
        BronzeTab.TournamentClass = new TournamentClass { Id = 4, Name = "Bronze" };

        // Subscribe to changes
        SubscribeToChanges(PlatinTab.TournamentClass);
        SubscribeToChanges(GoldTab.TournamentClass);
        SubscribeToChanges(SilberTab.TournamentClass);
        SubscribeToChanges(BronzeTab.TournamentClass);

        // Subscribe to data changed events from tabs
        PlatinTab.DataChanged += (s, e) => MarkAsChanged();
        GoldTab.DataChanged += (s, e) => MarkAsChanged();
        SilberTab.DataChanged += (s, e) => MarkAsChanged();
        BronzeTab.DataChanged += (s, e) => MarkAsChanged();
    }

    private void SubscribeToChanges(TournamentClass tournamentClass)
    {
        tournamentClass.Groups.CollectionChanged += (s, e) => MarkAsChanged();
        foreach (var group in tournamentClass.Groups)
        {
            group.Players.CollectionChanged += (s, e) => MarkAsChanged();
        }
        
        // Subscribe to GameRules changes for automatic saving
        tournamentClass.GameRules.PropertyChanged += (s, e) => MarkAsChanged();
    }

    private void InitializeServices()
    {
        _localizationService.PropertyChanged += (s, e) => UpdateTranslations();
        _configService.LanguageChanged += (s, language) => UpdateLanguageStatus();
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
            System.Diagnostics.Debug.WriteLine("AutoSave_Tick: Auto-saving due to changes...");
            await SaveDataInternal();
            
            // Reset timer to normal interval
            UpdateAutoSaveTimer();
        }
    }

    private void MarkAsChanged()
    {
        System.Diagnostics.Debug.WriteLine("MarkAsChanged: Data has been marked as changed");
        _hasUnsavedChanges = true;
        UpdateStatusBar();
        
        // Triggere sofortiges Auto-Save wenn aktiviert (mit kleiner Verzögerung)
        if (_configService.Config.AutoSave)
        {
            System.Diagnostics.Debug.WriteLine("MarkAsChanged: Auto-save enabled, scheduling immediate save...");
            
            // Auto-Save nach 2 Sekunden Verzögerung um mehrfache Aufrufe zu vermeiden
            _autoSaveTimer.Stop();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(2);
            _autoSaveTimer.Start();
        }
    }

    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("AppTitle");
        
        // Update menu items
        FileMenuItem.Header = _localizationService.GetString("File");
        NewMenuItem.Header = _localizationService.GetString("New");
        OpenMenuItem.Header = _localizationService.GetString("Open");
        SaveMenuItem.Header = _localizationService.GetString("Save");
        SaveAsMenuItem.Header = _localizationService.GetString("SaveAs");
        ExitMenuItem.Header = _localizationService.GetString("Exit");
        SettingsMenuItem.Header = _localizationService.GetString("Settings");
        HelpMenuItem.Header = _localizationService.GetString("Help");
        AboutMenuItem.Header = _localizationService.GetString("About");

        // Update tab headers
        var platinTextBlock = FindTextBlockInHeader(PlatinTabItem);
        if (platinTextBlock != null) platinTextBlock.Text = _localizationService.GetString("Platinum");
        
        var goldTextBlock = FindTextBlockInHeader(GoldTabItem);
        if (goldTextBlock != null) goldTextBlock.Text = _localizationService.GetString("Gold");
        
        var silverTextBlock = FindTextBlockInHeader(SilverTabItem);
        if (silverTextBlock != null) silverTextBlock.Text = _localizationService.GetString("Silver");
        
        var bronzeTextBlock = FindTextBlockInHeader(BronzeTabItem);
        if (bronzeTextBlock != null) bronzeTextBlock.Text = _localizationService.GetString("Bronze");

        UpdateLanguageStatus();
        UpdateStatusBar();
    }

    private TextBlock? FindTextBlockInHeader(TabItem tabItem)
    {
        if (tabItem.Header is StackPanel stackPanel)
        {
            return stackPanel.Children.OfType<TextBlock>().FirstOrDefault();
        }
        return null;
    }

    private void UpdateLanguageStatus()
    {
        LanguageStatusBlock.Text = _localizationService.CurrentLanguage.ToUpper();
    }

    private void UpdateStatusBar()
    {
        StatusTextBlock.Text = _hasUnsavedChanges ? 
            _localizationService.GetString("HasUnsavedChanges") : 
            _localizationService.GetString("Ready");
        
        LastSavedBlock.Text = _hasUnsavedChanges ? 
            _localizationService.GetString("NotSaved") : 
            _localizationService.GetString("Saved");
    }

    private async void LoadData()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== LoadData START ===");
            
            var data = await _dataService.LoadTournamentDataAsync();
            if (data.TournamentClasses.Count >= 4)
            {
                System.Diagnostics.Debug.WriteLine($"LoadData: Found {data.TournamentClasses.Count} tournament classes");
                
                // WICHTIG: Erst die Event-Handler der alten Objekte entfernen
                UnsubscribeFromChanges(PlatinTab.TournamentClass);
                UnsubscribeFromChanges(GoldTab.TournamentClass);
                UnsubscribeFromChanges(SilberTab.TournamentClass);
                UnsubscribeFromChanges(BronzeTab.TournamentClass);
                
                // Debug: Log groups before loading
                System.Diagnostics.Debug.WriteLine($"Before loading - Platin groups: {PlatinTab.TournamentClass?.Groups?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Before loading - Gold groups: {GoldTab.TournamentClass?.Groups?.Count ?? 0}");
                
                // Direkt die geladenen TournamentClass-Objekte zuweisen
                PlatinTab.TournamentClass = data.TournamentClasses[0];
                GoldTab.TournamentClass = data.TournamentClasses[1];
                SilberTab.TournamentClass = data.TournamentClasses[2];
                BronzeTab.TournamentClass = data.TournamentClasses[3];

                // Debug: Log groups after loading
                System.Diagnostics.Debug.WriteLine($"After loading - Platin groups: {PlatinTab.TournamentClass?.Groups?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"After loading - Gold groups: {GoldTab.TournamentClass?.Groups?.Count ?? 0}");
                
                // Namen und IDs sicherstellen
                PlatinTab.TournamentClass.Name = "Platin";
                GoldTab.TournamentClass.Name = "Gold";
                SilberTab.TournamentClass.Name = "Silber";
                BronzeTab.TournamentClass.Name = "Bronze";

                PlatinTab.TournamentClass.Id = 1;
                GoldTab.TournamentClass.Id = 2;
                SilberTab.TournamentClass.Id = 3;
                BronzeTab.TournamentClass.Id = 4;

                // Event-Handler für die neuen Objekte abonnieren
                SubscribeToChanges(PlatinTab.TournamentClass);
                SubscribeToChanges(GoldTab.TournamentClass);
                SubscribeToChanges(SilberTab.TournamentClass);
                SubscribeToChanges(BronzeTab.TournamentClass);
                
                System.Diagnostics.Debug.WriteLine("LoadData: Successfully loaded and assigned tournament classes");
                
                // Daten wurden erfolgreich geladen - KEINE weitere Initialisierung nötig
                _hasUnsavedChanges = false;
                UpdateStatusBar();
                System.Diagnostics.Debug.WriteLine("=== LoadData END ===");
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("LoadData: Not enough tournament classes loaded, keeping initialized ones");
            }
            
            _hasUnsavedChanges = false;
            UpdateStatusBar();
            System.Diagnostics.Debug.WriteLine("=== LoadData END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadData: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"LoadData: Stack trace: {ex.StackTrace}");
            // Bei Fehler: Behalte die initialisierten TournamentClass-Objekte
        }
    }
    
    private void UnsubscribeFromChanges(TournamentClass? tournamentClass)
    {
        if (tournamentClass?.Groups == null) return;
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"UnsubscribeFromChanges: Unsubscribing from {tournamentClass.Name}");
            
            // Entferne Collection-Event-Handler
            tournamentClass.Groups.CollectionChanged -= (s, e) => MarkAsChanged();
            
            // Entferne Player-Event-Handler für alle Gruppen
            foreach (var group in tournamentClass.Groups)
            {
                if (group?.Players != null)
                {
                    group.Players.CollectionChanged -= (s, e) => MarkAsChanged();
                }
            }
            
            // Entferne GameRules Event-Handler
            if (tournamentClass.GameRules != null)
            {
                tournamentClass.GameRules.PropertyChanged -= (s, e) => MarkAsChanged();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UnsubscribeFromChanges: ERROR: {ex.Message}");
        }
    }

    private async Task SaveDataInternal()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== SaveDataInternal START ===");
            
            var data = new TournamentData
            {
                TournamentClasses = new List<TournamentClass>
                {
                    PlatinTab.TournamentClass,
                    GoldTab.TournamentClass,
                    SilberTab.TournamentClass,
                    BronzeTab.TournamentClass
                }
            };

            System.Diagnostics.Debug.WriteLine($"SaveDataInternal: Saving {data.TournamentClasses.Count} tournament classes");
            
            await _dataService.SaveTournamentDataAsync(data);
            _hasUnsavedChanges = false;
            UpdateStatusBar();
            
            System.Diagnostics.Debug.WriteLine("=== SaveDataInternal END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveDataInternal: ERROR: {ex.Message}");
            MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    // Menu Event Handlers
    private void New_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Create new tournament? Unsaved changes will be lost.", "New Tournament", 
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            InitializeTournamentClasses();
            _hasUnsavedChanges = false;
            UpdateStatusBar();
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
            // Implementation for loading from custom file
            MessageBox.Show("Custom file loading not implemented yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
            // Implementation for saving to custom file
            MessageBox.Show("Custom file saving not implemented yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_configService, _localizationService);
        settingsWindow.Owner = this;
        
        if (settingsWindow.ShowDialog() == true)
        {
            // Settings were saved, update any UI that depends on settings
            UpdateAutoSaveTimer();
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Dart Tournament Planner v1.0\n\nA modern tournament management application.", 
            "About", MessageBoxButton.OK, MessageBoxImage.Information);
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
            var result = MessageBox.Show("You have unsaved changes. Do you want to save before exiting?", 
                "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

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
                        // If save fails, don't exit
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
}