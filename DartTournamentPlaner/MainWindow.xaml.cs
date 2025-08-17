using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Reflection;

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

        // WICHTIG: Nach der Initialisierung GroupPhase sicherstellen
        System.Diagnostics.Debug.WriteLine("InitializeTournamentClasses: Ensuring GroupPhase exists for all tournament classes...");
        PlatinTab.TournamentClass.EnsureGroupPhaseExists();
        GoldTab.TournamentClass.EnsureGroupPhaseExists();
        SilberTab.TournamentClass.EnsureGroupPhaseExists();
        BronzeTab.TournamentClass.EnsureGroupPhaseExists();

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

    // Track subscribed tournament classes to prevent double subscription
    private readonly HashSet<TournamentClass> _subscribedTournaments = new HashSet<TournamentClass>();

    private void SubscribeToChanges(TournamentClass tournamentClass)
    {
        System.Diagnostics.Debug.WriteLine($"=== SubscribeToChanges START for {tournamentClass.Name} ===");
        
        // WICHTIG: Prüfe ob bereits abonniert, um doppelte Event-Handler zu vermeiden
        if (_subscribedTournaments.Contains(tournamentClass))
        {
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: {tournamentClass.Name} already subscribed, skipping");
            return;
        }
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Subscribing to Groups.CollectionChanged for {tournamentClass.Name}");
            tournamentClass.Groups.CollectionChanged += (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine($"Groups.CollectionChanged triggered for {tournamentClass.Name}: {e.Action}");
                MarkAsChanged();
            };
            
            // NEU: Abonniere das neue DataChangedEvent für Match-Ergebnisse und Freilose
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Subscribing to DataChangedEvent for {tournamentClass.Name}");
            tournamentClass.DataChangedEvent += (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine($"DataChangedEvent triggered for {tournamentClass.Name} - marking as changed");
                MarkAsChanged();
            };
            
            // WICHTIG: Direkte Zugriffe auf Groups vermeiden während der Subscription
            // Verwende stattdessen direkten Zugriff auf die GroupPhase
            var groupPhase = tournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
            if (groupPhase != null)
            {
                foreach (var group in groupPhase.Groups)
                {
                    System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Subscribing to Players.CollectionChanged for group {group.Name}");
                    group.Players.CollectionChanged += (s, e) => MarkAsChanged();
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: No GroupPhase found for {tournamentClass.Name}, skipping group subscriptions");
            }
            
            // Subscribe to GameRules changes for automatic saving
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Subscribing to GameRules.PropertyChanged for {tournamentClass.Name}");
            tournamentClass.GameRules.PropertyChanged += (s, e) => MarkAsChanged();
            
            // Markiere als abonniert
            _subscribedTournaments.Add(tournamentClass);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: ERROR for {tournamentClass.Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Stack trace: {ex.StackTrace}");
        }
        
        System.Diagnostics.Debug.WriteLine($"=== SubscribeToChanges END for {tournamentClass.Name} ===");
    }

    private void InitializeServices()
    {
        _localizationService.PropertyChanged += (s, e) => 
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow: LocalizationService PropertyChanged - {e.PropertyName}");
            UpdateTranslations();
        };
        
        _configService.LanguageChanged += (s, language) => 
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow: LanguageChanged event received - changing from '{_localizationService.CurrentLanguage}' to '{language}'");
            
            // Setze die Sprache im LocalizationService
            _localizationService.SetLanguage(language);
            
            // Force immediate UI update for all components
            Dispatcher.BeginInvoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow: Performing immediate UI updates after language change");
                
                UpdateLanguageStatus();
                UpdateTranslations();
                ForceUIUpdate();
                
                System.Diagnostics.Debug.WriteLine($"MainWindow: Language change UI updates completed");
            }, System.Windows.Threading.DispatcherPriority.Render);
        };
    }

    /// <summary>
    /// Forces an immediate UI update for all components
    /// </summary>
    private void ForceUIUpdate()
    {
        System.Diagnostics.Debug.WriteLine("MainWindow: ForceUIUpdate starting...");
        
        try
        {
            // Update main window components
            UpdateTranslations();
            UpdateLanguageStatus();
            UpdateStatusBar();
            
            // Force child controls to update their translations immediately (synchronously)
            PlatinTab?.UpdateTranslations();
            GoldTab?.UpdateTranslations();
            SilberTab?.UpdateTranslations();
            BronzeTab?.UpdateTranslations();
            
            System.Diagnostics.Debug.WriteLine("MainWindow: ForceUIUpdate completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow: ForceUIUpdate ERROR: {ex.Message}");
        }
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
        System.Diagnostics.Debug.WriteLine("=== MarkAsChanged triggered ===");
        System.Diagnostics.Debug.WriteLine($"MarkAsChanged: Data has been marked as changed at {DateTime.Now:HH:mm:ss.fff}");
        
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
        
        System.Diagnostics.Debug.WriteLine("=== MarkAsChanged end ===");
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
        ViewMenuItem.Header = _localizationService.GetString("View");
        OverviewModeMenuItem.Header = _localizationService.GetString("TournamentOverview");
        SettingsMenuItem.Header = _localizationService.GetString("Settings");
        HelpMenuItem.Header = _localizationService.GetString("Help");
        HelpContentMenuItem.Header = "📖 " + _localizationService.GetString("Help");
        BugReportMenuItem.Header = _localizationService.GetString("BugReport");
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

        // Update donation button
        DonationButton.Content = _localizationService.GetString("Donate");
        DonationButton.ToolTip = _localizationService.GetString("DonateTooltip");

        UpdateLanguageStatus();
        UpdateStatusBar();
        
        // Force child controls to update their translations
        PlatinTab?.Dispatcher.BeginInvoke(() => PlatinTab?.UpdateTranslations());
        GoldTab?.Dispatcher.BeginInvoke(() => GoldTab?.UpdateTranslations());
        SilberTab?.Dispatcher.BeginInvoke(() => SilberTab?.UpdateTranslations());
        BronzeTab?.Dispatcher.BeginInvoke(() => BronzeTab?.UpdateTranslations());
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
                
                // Debug: Log loaded data
                for (int i = 0; i < data.TournamentClasses.Count; i++)
                {
                    var tc = data.TournamentClasses[i];
                    System.Diagnostics.Debug.WriteLine($"Loaded TournamentClass[{i}]: Name={tc.Name}, Groups={tc.Groups.Count}");
                    foreach (var group in tc.Groups)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Group: {group.Name} (ID: {group.Id}) with {group.Players.Count} players");
                    }
                }
                
                // WICHTIG: Direkt die geladenen TournamentClass-Objekte zuweisen
                // Das triggert automatisch UI-Updates durch das TournamentClass Property
                System.Diagnostics.Debug.WriteLine($"LoadData: Assigning loaded tournament classes to tabs...");
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

                // WICHTIG: Nach JSON-Loading GroupPhase-Existenz sicherstellen und Duplikate bereinigen
                System.Diagnostics.Debug.WriteLine($"LoadData: Ensuring GroupPhase exists and cleaning up duplicates...");
                CleanupDuplicatePhasesAndEnsureGroupPhase(PlatinTab.TournamentClass);
                CleanupDuplicatePhasesAndEnsureGroupPhase(GoldTab.TournamentClass);
                CleanupDuplicatePhasesAndEnsureGroupPhase(SilberTab.TournamentClass);
                CleanupDuplicatePhasesAndEnsureGroupPhase(BronzeTab.TournamentClass);

                // Event-Handler für die neuen Objekte abonnieren (mit Duplikat-Schutz)
                System.Diagnostics.Debug.WriteLine($"LoadData: Subscribing to changes for loaded tournament classes...");
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
        if (tournamentClass == null) 
        {
            System.Diagnostics.Debug.WriteLine("UnsubscribeFromChanges: tournamentClass is null, skipping");
            return;
        }
        
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== UnsubscribeFromChanges START for {tournamentClass.Name} ===");
            
            // Entferne aus dem Tracking
            if (_subscribedTournaments.Contains(tournamentClass))
            {
                _subscribedTournaments.Remove(tournamentClass);
                System.Diagnostics.Debug.WriteLine($"UnsubscribeFromChanges: Removed {tournamentClass.Name} from subscription tracking");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"UnsubscribeFromChanges: {tournamentClass.Name} was not in subscription tracking");
            }
            
            // Hinweis: Da wir Lambda-Ausdrücke verwenden, können wir die Event-Handler nicht direkt entfernen
            // Das ist jedoch in Ordnung, da wir durch das Tracking-System doppelte Registrierungen verhindern
            // und die alten TournamentClass-Objekte für Garbage Collection freigegeben werden
            
            System.Diagnostics.Debug.WriteLine($"=== UnsubscribeFromChanges END for {tournamentClass.Name} ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UnsubscribeFromChanges: ERROR for {tournamentClass?.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// NEUE METHODE: Bereinigt Duplikat-Phases und stellt eine korrekte GroupPhase sicher
    /// </summary>
    private void CleanupDuplicatePhasesAndEnsureGroupPhase(TournamentClass tournamentClass)
    {
        System.Diagnostics.Debug.WriteLine($"=== CleanupDuplicatePhasesAndEnsureGroupPhase START for {tournamentClass.Name} ===");
        System.Diagnostics.Debug.WriteLine($"CleanupDuplicatePhasesAndEnsureGroupPhase: Current Phases count = {tournamentClass.Phases.Count}");
        
        try
        {
            // Erstelle eine Liste der zu behaltenden Phasen
            var cleanPhases = new List<TournamentPhase>();
            
            // 1. Stelle sicher, dass genau eine GroupPhase existiert
            var groupPhases = tournamentClass.Phases.Where(p => p.PhaseType == TournamentPhaseType.GroupPhase).ToList();
            TournamentPhase groupPhase;
            
            if (groupPhases.Any())
            {
                System.Diagnostics.Debug.WriteLine($"  Found {groupPhases.Count} GroupPhases, keeping the first one");
                groupPhase = groupPhases.First();
                
                // Merge alle Groups aus anderen GroupPhases in die erste
                for (int i = 1; i < groupPhases.Count; i++)
                {
                    var duplicatePhase = groupPhases[i];
                    foreach (var group in duplicatePhase.Groups)
                    {
                        if (!groupPhase.Groups.Any(g => g.Id == group.Id))
                        {
                            groupPhase.Groups.Add(group);
                            System.Diagnostics.Debug.WriteLine($"    Merged group {group.Name} from duplicate GroupPhase");
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"  No GroupPhases found, creating new one");
                groupPhase = new TournamentPhase
                {
                    Name = "Gruppenphase", 
                    PhaseType = TournamentPhaseType.GroupPhase,
                    IsActive = false,
                    IsCompleted = false
                };
            }
            
            cleanPhases.Add(groupPhase);
            
            // 2. Behalte nur die neueste/aktive Phase von jedem anderen Typ
            var knockoutPhases = tournamentClass.Phases.Where(p => p.PhaseType == TournamentPhaseType.KnockoutPhase).ToList();
            var finalsPhases = tournamentClass.Phases.Where(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals).ToList();
            
            System.Diagnostics.Debug.WriteLine($"  Found {knockoutPhases.Count} KnockoutPhases, {finalsPhases.Count} FinalsPhases");
            
            // Behalte nur die beste KnockoutPhase (falls vorhanden) - Priorisiere die mit den meisten Matches
            if (knockoutPhases.Any())
            {
                var latestKnockout = knockoutPhases
                    .OrderByDescending(p => p.WinnerBracket?.Count ?? 0)
                    .ThenByDescending(p => p.LoserBracket?.Count ?? 0)
                    .ThenByDescending(p => p.IsActive)
                    .First();
                    
                System.Diagnostics.Debug.WriteLine($"  Keeping KnockoutPhase with {latestKnockout.WinnerBracket?.Count ?? 0} WB matches, {latestKnockout.LoserBracket?.Count ?? 0} LB matches");
                latestKnockout.IsActive = true; // Stelle sicher dass sie aktiv ist
                cleanPhases.Add(latestKnockout);
            }
            
            // Behalte nur die letzte/aktivste FinalsPhase (falls vorhanden)
            if (finalsPhases.Any())
            {
                var latestFinals = finalsPhases.OrderByDescending(p => p.IsActive).ThenBy(p => p.IsCompleted).First();
                System.Diagnostics.Debug.WriteLine($"  Keeping FinalsPhase: {latestFinals.Name}, Active: {latestFinals.IsActive}");
                cleanPhases.Add(latestFinals);
            }
            
            // 3. Ersetze die Phases-Collection mit den bereinigten Phasen
            tournamentClass.Phases.Clear();
            foreach (var phase in cleanPhases)
            {
                tournamentClass.Phases.Add(phase);
            }
            
            // 4. Bestimme die CurrentPhase korrekt
            var activeKnockout = cleanPhases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.KnockoutPhase);
            if (activeKnockout != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Setting CurrentPhase to KnockoutPhase");
                activeKnockout.IsActive = true;
                tournamentClass.CurrentPhase = activeKnockout;
                groupPhase.IsActive = false;
                groupPhase.IsCompleted = true;
            }
            else
            {
                var activeFinals = cleanPhases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals);
                if (activeFinals != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Setting CurrentPhase to RoundRobinFinals");
                    activeFinals.IsActive = true;
                    tournamentClass.CurrentPhase = activeFinals;
                    groupPhase.IsActive = false;
                    groupPhase.IsCompleted = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  No advanced phases found, setting to GroupPhase");
                    groupPhase.IsActive = true;
                    groupPhase.IsCompleted = false;
                    tournamentClass.CurrentPhase = groupPhase;
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"CleanupDuplicatePhasesAndEnsureGroupPhase: Final Phases count = {tournamentClass.Phases.Count}");
            System.Diagnostics.Debug.WriteLine($"CleanupDuplicatePhasesAndEnsureGroupPhase: CurrentPhase = {tournamentClass.CurrentPhase?.PhaseType}");
            System.Diagnostics.Debug.WriteLine($"=== CleanupDuplicatePhasesAndEnsureGroupPhase END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CleanupDuplicatePhasesAndEnsureGroupPhase: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"CleanupDuplicatePhasesAndEnsureGroupPhase: Stack trace: {ex.StackTrace}");
            
            // Fallback: Erstelle eine saubere GroupPhase
            tournamentClass.Phases.Clear();
            var emergencyGroupPhase = new TournamentPhase
            {
                Name = "Gruppenphase",
                PhaseType = TournamentPhaseType.GroupPhase,
                IsActive = true
            };
            tournamentClass.Phases.Add(emergencyGroupPhase);
            tournamentClass.CurrentPhase = emergencyGroupPhase;
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
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("ErrorSavingData")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    // Menu Event Handlers
    private void New_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService.GetString("NewTournament");
        var message = _localizationService.GetString("CreateNewTournament");
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        
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
            // Implementation for saving to custom file
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
            // Settings were saved, update any UI that depends on settings
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

    private void OverviewMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get all tournament classes
            var tournamentClasses = new List<TournamentClass>
            {
                PlatinTab.TournamentClass,
                GoldTab.TournamentClass,
                SilberTab.TournamentClass,
                BronzeTab.TournamentClass
            };

            // Create and show the overview window
            var overviewWindow = new TournamentOverviewWindow(tournamentClasses, _localizationService);
            overviewWindow.Show();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("ErrorOpeningOverview")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
}