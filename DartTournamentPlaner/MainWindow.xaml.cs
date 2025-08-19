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

namespace DartTournamentPlaner;

/// <summary>
/// Code-Behind-Klasse für das Hauptfenster der Dart Tournament Planer Anwendung
/// Verwaltet die vier Turnierklassen (Platin, Gold, Silber, Bronze) und koordiniert
/// alle Services wie Konfiguration, Lokalisierung und Datenverwaltung
/// </summary>
public partial class MainWindow : Window
{
    // Service-Instanzen für die gesamte Anwendung
    private readonly ConfigService _configService;           // Verwaltet App-Einstellungen
    private readonly LocalizationService _localizationService; // Verwaltet Übersetzungen
    private readonly DataService _dataService;              // Verwaltet Datenspeicherung/laden
    
    // Auto-Save System
    private readonly DispatcherTimer _autoSaveTimer = new DispatcherTimer(); // Timer für automatisches Speichern
    private bool _hasUnsavedChanges = false;                 // Flag für ungespeicherte Änderungen

    /// <summary>
    /// Konstruktor des Hauptfensters
    /// Initialisiert alle Services und lädt gespeicherte Daten
    /// </summary>
    public MainWindow()
    {
        InitializeComponent(); // WPF-Standard-Initialisierung
        
        // Services aus App.xaml.cs holen und Null-Checks durchführen
        _configService = App.ConfigService ?? throw new InvalidOperationException("ConfigService not initialized");
        _localizationService = App.LocalizationService ?? throw new InvalidOperationException("LocalizationService not initialized");
        _dataService = App.DataService ?? throw new InvalidOperationException("DataService not initialized");

        // Initialisierung in logischer Reihenfolge
        InitializeTournamentClasses(); // Erstelle die 4 Turnierklassen
        InitializeServices();          // Konfiguriere Service-Events
        InitializeAutoSave();         // Konfiguriere Auto-Save System
        UpdateTranslations();         // Setze initiale Übersetzungen
        LoadData();                   // Lade gespeicherte Turnierdaten
    }

    /// <summary>
    /// Initialisiert die vier Turnierklassen (Platin, Gold, Silber, Bronze)
    /// Jede Klasse bekommt ihre eigene ID und ihren Namen zugewiesen
    /// </summary>
    private void InitializeTournamentClasses()
    {
        // Erstelle neue TournamentClass-Instanzen für jede Turnierstufe
        PlatinTab.TournamentClass = new TournamentClass { Id = 1, Name = "Platin" };
        GoldTab.TournamentClass = new TournamentClass { Id = 2, Name = "Gold" };
        SilberTab.TournamentClass = new TournamentClass { Id = 3, Name = "Silber" };
        BronzeTab.TournamentClass = new TournamentClass { Id = 4, Name = "Bronze" };

        // WICHTIG: Nach der Initialisierung GroupPhase für alle Klassen sicherstellen
        // Das verhindert Null-Reference-Exceptions beim ersten Zugriff auf Groups
        System.Diagnostics.Debug.WriteLine("InitializeTournamentClasses: Ensuring GroupPhase exists for all tournament classes...");
        PlatinTab.TournamentClass.EnsureGroupPhaseExists();
        GoldTab.TournamentClass.EnsureGroupPhaseExists();
        SilberTab.TournamentClass.EnsureGroupPhaseExists();
        BronzeTab.TournamentClass.EnsureGroupPhaseExists();

        // Event-Handler für Datenänderungen abonnieren
        SubscribeToChanges(PlatinTab.TournamentClass);
        SubscribeToChanges(GoldTab.TournamentClass);
        SubscribeToChanges(SilberTab.TournamentClass);
        SubscribeToChanges(BronzeTab.TournamentClass);

        // Zusätzlich: Events von den Tab-Controls selbst abonnieren
        PlatinTab.DataChanged += (s, e) => MarkAsChanged();
        GoldTab.DataChanged += (s, e) => MarkAsChanged();
        SilberTab.DataChanged += (s, e) => MarkAsChanged();
        BronzeTab.DataChanged += (s, e) => MarkAsChanged();
    } 

    // Tracking-Set um doppelte Event-Handler-Registrierungen zu vermeiden
    private readonly HashSet<TournamentClass> _subscribedTournaments = new HashSet<TournamentClass>();

    /// <summary>
    /// Abonniert alle relevanten Events einer TournamentClass für automatisches Speichern
    /// Verhindert doppelte Registrierungen durch Tracking-System
    /// </summary>
    /// <param name="tournamentClass">Die zu abonnierende TournamentClass</param>
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
            // Abonniere Groups-Collection-Änderungen (Gruppen hinzufügen/entfernen)
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Subscribing to Groups.CollectionChanged for {tournamentClass.Name}");
            tournamentClass.Groups.CollectionChanged += (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine($"Groups.CollectionChanged triggered for {tournamentClass.Name}: {e.Action}");
                MarkAsChanged();
            };
            
            // NEU: Abonniere das DataChangedEvent für Match-Ergebnisse und Freilose
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Subscribing to DataChangedEvent for {tournamentClass.Name}");
            tournamentClass.DataChangedEvent += (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine($"DataChangedEvent triggered for {tournamentClass.Name} - marking as changed");
                MarkAsChanged();
            };
            
            // WICHTIG: Direkte Zugriffe auf Groups vermeiden während der Subscription
            // Verwende stattdessen direkten Zugriff auf die GroupPhase um Rekursion zu vermeiden
            var groupPhase = tournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
            if (groupPhase != null)
            {
                // Abonniere Player-Collection-Änderungen für jede Gruppe
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
            
            // Abonniere GameRules-Änderungen für automatisches Speichern
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Subscribing to GameRules.PropertyChanged for {tournamentClass.Name}");
            tournamentClass.GameRules.PropertyChanged += (s, e) => MarkAsChanged();
            
            // Markiere als abonniert im Tracking-System
            _subscribedTournaments.Add(tournamentClass);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: ERROR for {tournamentClass.Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges: Stack trace: {ex.StackTrace}");
        }
        
        System.Diagnostics.Debug.WriteLine($"=== SubscribeToChanges END for {tournamentClass.Name} ===");
    }

    /// <summary>
    /// Initialisiert Service-Events und Callbacks
    /// Konfiguriert Reaktionen auf Sprach- und Konfigurationsänderungen
    /// </summary>
    private void InitializeServices()
    {
        // Reagiere auf Lokalisierungs-Änderungen (Sprachwechsel)
        _localizationService.PropertyChanged += (s, e) => 
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow: LocalizationService PropertyChanged - {e.PropertyName}");
            UpdateTranslations(); // Aktualisiere alle UI-Texte
        };
        
        // Reagiere auf Sprachwechsel über ConfigService
        _configService.LanguageChanged += (s, language) => 
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow: LanguageChanged event received - changing from '{_localizationService.CurrentLanguage}' to '{language}'");
            
            // Setze die neue Sprache im LocalizationService
            _localizationService.SetLanguage(language);
            
            // Erzwinge sofortiges UI-Update für alle Komponenten
            Dispatcher.BeginInvoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow: Performing immediate UI updates after language change");
                
                UpdateLanguageStatus();    // Statusbar-Sprache
                UpdateTranslations();      // Alle UI-Texte
                ForceUIUpdate();          // Alle Child-Controls
                
                System.Diagnostics.Debug.WriteLine($"MainWindow: Language change UI updates completed");
            }, System.Windows.Threading.DispatcherPriority.Render);
        };
    }

    /// <summary>
    /// Erzwingt ein sofortiges UI-Update für alle Komponenten
    /// Wird nach Sprachwechseln aufgerufen um sicherzustellen dass alles aktualisiert wird
    /// </summary>
    private void ForceUIUpdate()
    {
        System.Diagnostics.Debug.WriteLine("MainWindow: ForceUIUpdate starting...");
        
        try
        {
            // Aktualisiere Hauptfenster-Komponenten
            UpdateTranslations();
            UpdateLanguageStatus();
            UpdateStatusBar();
            
            // Erzwinge sofortige Übersetzungsaktualisierung für alle Tab-Controls
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

    /// <summary>
    /// Initialisiert das automatische Speichersystem
    /// Konfiguriert Timer und reagiert auf Konfigurationsänderungen
    /// </summary>
    private void InitializeAutoSave()
    {
        // Konfiguriere Auto-Save Timer
        _autoSaveTimer.Tick += AutoSave_Tick;
        UpdateAutoSaveTimer();
        
        // Reagiere auf Änderungen der Auto-Save-Einstellungen
        _configService.Config.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AppConfig.AutoSave) || e.PropertyName == nameof(AppConfig.AutoSaveInterval))
            {
                UpdateAutoSaveTimer(); // Timer neu konfigurieren
            }
        };
    }

    /// <summary>
    /// Aktualisiert die Auto-Save Timer-Konfiguration basierend auf den Einstellungen
    /// Startet oder stoppt den Timer je nach Konfiguration
    /// </summary>
    private void UpdateAutoSaveTimer()
    {
        _autoSaveTimer.Stop();
        if (_configService.Config.AutoSave)
        {
            // Setze Intervall aus Konfiguration und starte Timer
            _autoSaveTimer.Interval = TimeSpan.FromMinutes(_configService.Config.AutoSaveInterval);
            _autoSaveTimer.Start();
        }
    }

    /// <summary>
    /// Event-Handler für Auto-Save Timer
    /// Speichert automatisch wenn ungespeicherte Änderungen vorhanden sind
    /// </summary>
    private async void AutoSave_Tick(object? sender, EventArgs e)
    {
        if (_hasUnsavedChanges)
        {
            System.Diagnostics.Debug.WriteLine("AutoSave_Tick: Auto-saving due to changes...");
            await SaveDataInternal();
            
            // Timer auf normales Intervall zurücksetzen
            UpdateAutoSaveTimer();
        }
    }

    /// <summary>
    /// Markiert die Anwendung als "geändert" und löst Auto-Save-Logik aus
    /// Zentrale Methode für alle Datenänderungen in der Anwendung
    /// </summary>
    private void MarkAsChanged()
    {
        System.Diagnostics.Debug.WriteLine("=== MarkAsChanged triggered ===");
        System.Diagnostics.Debug.WriteLine($"MarkAsChanged: Data has been marked as changed at {DateTime.Now:HH:mm:ss.fff}");
        
        _hasUnsavedChanges = true;
        UpdateStatusBar(); // Statusleiste aktualisieren
        
        // Triggere sofortiges Auto-Save wenn aktiviert (mit kleiner Verzögerung für Batch-Updates)
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

    /// <summary>
    /// Aktualisiert alle übersetzten Texte in der Benutzeroberfläche
    /// Wird bei Sprachwechseln und bei der Initialisierung aufgerufen
    /// </summary>
    private void UpdateTranslations()
    {
        // Hauptfenster-Titel
        Title = _localizationService.GetString("AppTitle");
        
        // Menü-Einträge aktualisieren
        FileMenuItem.Header = _localizationService.GetString("File");
        NewMenuItem.Header = _localizationService.GetString("New");
        OpenMenuItem.Header = _localizationService.GetString("Open");
        SaveMenuItem.Header = _localizationService.GetString("Save");
        SaveAsMenuItem.Header = _localizationService.GetString("SaveAs");
        PrintMenuItem.Header = "🖨️ " + (_localizationService.GetString("Print") ?? "Drucken");
        ExitMenuItem.Header = _localizationService.GetString("Exit");
        ViewMenuItem.Header = _localizationService.GetString("View");
        OverviewModeMenuItem.Header = _localizationService.GetString("TournamentOverview");
        SettingsMenuItem.Header = _localizationService.GetString("Settings");
        HelpMenuItem.Header = _localizationService.GetString("Help");
        HelpContentMenuItem.Header = "📖 " + _localizationService.GetString("Help");
        BugReportMenuItem.Header = _localizationService.GetString("BugReport");
        AboutMenuItem.Header = _localizationService.GetString("About");

        // Tab-Header aktualisieren (suche TextBlocks in StackPanel-Headern)
        var platinTextBlock = FindTextBlockInHeader(PlatinTabItem);
        if (platinTextBlock != null) platinTextBlock.Text = _localizationService.GetString("Platinum");
        
        var goldTextBlock = FindTextBlockInHeader(GoldTabItem);
        if (goldTextBlock != null) goldTextBlock.Text = _localizationService.GetString("Gold");
        
        var silverTextBlock = FindTextBlockInHeader(SilverTabItem);
        if (silverTextBlock != null) silverTextBlock.Text = _localizationService.GetString("Silver");
        
        var bronzeTextBlock = FindTextBlockInHeader(BronzeTabItem);
        if (bronzeTextBlock != null) bronzeTextBlock.Text = _localizationService.GetString("Bronze");

        // Spenden-Button aktualisieren
        DonationButton.Content = _localizationService.GetString("Donate");
        DonationButton.ToolTip = _localizationService.GetString("DonateTooltip");

        // Statusleiste und Sprachindikator aktualisieren
        UpdateLanguageStatus();
        UpdateStatusBar();
        
        // Child-Controls zur Übersetzungsaktualisierung auffordern (asynchron)
        PlatinTab?.Dispatcher.BeginInvoke(() => PlatinTab?.UpdateTranslations());
        GoldTab?.Dispatcher.BeginInvoke(() => GoldTab?.UpdateTranslations());
        SilberTab?.Dispatcher.BeginInvoke(() => SilberTab?.UpdateTranslations());
        BronzeTab?.Dispatcher.BeginInvoke(() => BronzeTab?.UpdateTranslations());
    }

    /// <summary>
    /// Hilfsmethode: Findet das TextBlock-Element im Header eines TabItems
    /// Tab-Header bestehen aus StackPanels mit Icons und TextBlocks
    /// </summary>
    /// <param name="tabItem">Das TabItem dessen Header durchsucht werden soll</param>
    /// <returns>Das TextBlock-Element oder null wenn nicht gefunden</returns>
    private TextBlock? FindTextBlockInHeader(TabItem tabItem)
    {
        if (tabItem.Header is StackPanel stackPanel)
        {
            return stackPanel.Children.OfType<TextBlock>().FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Aktualisiert den Sprachindikator in der Statusleiste
    /// Zeigt die aktuelle Sprache als Kürzel (DE/EN) an
    /// </summary>
    private void UpdateLanguageStatus()
    {
        LanguageStatusBlock.Text = _localizationService.CurrentLanguage.ToUpper();
    }

    /// <summary>
    /// Aktualisiert die Statusleiste mit Save-Status und allgemeinen Informationen
    /// Zeigt an ob ungespeicherte Änderungen vorhanden sind
    /// </summary>
    private void UpdateStatusBar()
    {
        // Hauptstatus (Ready/Geändert)
        StatusTextBlock.Text = _hasUnsavedChanges ? 
            _localizationService.GetString("HasUnsavedChanges") : 
            _localizationService.GetString("Ready");
        
        // Speicherstatus (Gespeichert/Nicht gespeichert)
        LastSavedBlock.Text = _hasUnsavedChanges ? 
            _localizationService.GetString("NotSaved") : 
            _localizationService.GetString("Saved");
    }

    /// <summary>
    /// Lädt gespeicherte Turnierdaten und weist sie den TournamentClass-Instanzen zu
    /// Behandelt sowohl erfolgreiche Ladevorgänge als auch Fehler graceful
    /// </summary>
    private async void LoadData()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== LoadData START ===");
            
            // Lade Turnierdaten über DataService
            var data = await _dataService.LoadTournamentDataAsync();
            if (data.TournamentClasses.Count >= 4)
            {
                System.Diagnostics.Debug.WriteLine($"LoadData: Found {data.TournamentClasses.Count} tournament classes");
                
                // WICHTIG: Erst die Event-Handler der alten Objekte entfernen
                UnsubscribeFromChanges(PlatinTab.TournamentClass);
                UnsubscribeFromChanges(GoldTab.TournamentClass);
                UnsubscribeFromChanges(SilberTab.TournamentClass);
                UnsubscribeFromChanges(BronzeTab.TournamentClass);
                
                // Debug: Logge Groups vor dem Laden
                System.Diagnostics.Debug.WriteLine($"Before loading - Platin groups: {PlatinTab.TournamentClass?.Groups?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"Before loading - Gold groups: {GoldTab.TournamentClass?.Groups?.Count ?? 0}");
                
                // Debug: Logge geladene Daten
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

                // Debug: Logge Groups nach dem Laden
                System.Diagnostics.Debug.WriteLine($"After loading - Platin groups: {PlatinTab.TournamentClass?.Groups?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"After loading - Gold groups: {GoldTab.TournamentClass?.Groups?.Count ?? 0}");
                
                // Namen und IDs sicherstellen (überschreibt JSON-Daten mit korrekten Werten)
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
                
                // Daten wurden erfolgreich geladen - keine ungespeicherten Änderungen
                _hasUnsavedChanges = false;
                UpdateStatusBar();
                System.Diagnostics.Debug.WriteLine("=== LoadData END ===");
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("LoadData: Not enough tournament classes loaded, keeping initialized ones");
            }
            
            // Standard-Situation: Keine ungespeicherten Änderungen bei frischer Initialisierung
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
    
    /// <summary>
    /// Entfernt Event-Handler-Registrierungen für eine TournamentClass
    /// Wichtig beim Laden neuer Daten um Memory Leaks zu vermeiden
    /// </summary>
    /// <param name="tournamentClass">Die TournamentClass deren Events entfernt werden sollen</param>
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
            
            // Entferne aus dem Tracking-System
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
    /// Wird nach JSON-Deserialisierung aufgerufen um inkonsistente Datenstrukturen zu bereinigen
    /// </summary>
    /// <param name="tournamentClass">Die zu bereinigende TournamentClass</param>
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
            
            // 2. Behalte nur die beste/neueste Phase von jedem anderen Typ
            var knockoutPhases = tournamentClass.Phases.Where(p => p.PhaseType == TournamentPhaseType.KnockoutPhase).ToList();
            var finalsPhases = tournamentClass.Phases.Where(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals).ToList();
            
            System.Diagnostics.Debug.WriteLine($"  Found {knockoutPhases.Count} KnockoutPhases, {finalsPhases.Count} FinalsPhases");
            
            // Behalte nur die beste KnockoutPhase (falls vorhanden) - Priorisiere die mit den meisten Matches
            if (knockoutPhases.Any())
            {
                var latestKnockout = knockoutPhases
                    .OrderByDescending(p => p.WinnerBracket?.Count ?? 0)  // Meiste Winner Bracket Matches
                    .ThenByDescending(p => p.LoserBracket?.Count ?? 0)    // Meiste Loser Bracket Matches
                    .ThenByDescending(p => p.IsActive)                    // Aktive Phases bevorzugen
                    .First();
                    
                System.Diagnostics.Debug.WriteLine($"  Keeping KnockoutPhase with {latestKnockout.WinnerBracket?.Count ?? 0} WB matches, {latestKnockout.LoserBracket?.Count ?? 0} LB matches");
                latestKnockout.IsActive = true; // Stelle sicher dass sie aktiv ist
                cleanPhases.Add(latestKnockout);
            }
            
            // Behalte nur die beste/aktivste FinalsPhase (falls vorhanden)
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
            
            // 4. Bestimme die CurrentPhase korrekt basierend on verfügbaren Phasen
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
            
            // Fallback: Erstelle eine saubere GroupPhase wenn alles fehlschlägt
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

    /// <summary>
    /// Interne Methode zum Speichern der Turnierdaten
    /// Sammelt alle vier TournamentClass-Instanzen und speichert sie über den DataService
    /// </summary>
    private async Task SaveDataInternal()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== SaveDataInternal START ===");
            
            // Erstelle TournamentData-Objekt mit allen vier Klassen
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
            
            // Speichere über DataService
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
            throw; // Re-throw um Auto-Save-Logik zu benachrichtigen
        }
    }

    // MENÜ-EVENT-HANDLER - Behandeln alle Menü-Aktionen des Hauptfensters

    /// <summary>
    /// Event-Handler: Neues Turnier erstellen
    /// Fragt Benutzer um Bestätigung und initialisiert alle TournamentClass-Instanzen neu
    /// </summary>
    private void New_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService.GetString("NewTournament");
        var message = _localizationService.GetString("CreateNewTournament");
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            InitializeTournamentClasses(); // Erstelle neue, leere TournamentClass-Instanzen
            _hasUnsavedChanges = false;
            UpdateStatusBar();
        }
    }

    /// <summary>
    /// Event-Handler: Turnier aus Datei laden
    /// Zeigt Dateiauswahl-Dialog (Feature noch nicht implementiert)
    /// </summary>
    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Tournament files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            // TODO: Implementierung für Laden aus benutzerdefinierter Datei
            var title = _localizationService.GetString("Information");
            var message = _localizationService.GetString("CustomFileNotImplemented");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Event-Handler: Turnier speichern
    /// Speichert in die Standard-App-Daten
    /// </summary>
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await SaveDataInternal();
    }

    /// <summary>
    /// Event-Handler: Turnier unter neuem Namen speichern
    /// Zeigt Dateiauswahl-Dialog (Feature noch nicht implementiert)
    /// </summary>
    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Tournament files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            // TODO: Implementierung für Speichern in benutzerdefinierte Datei
            var title = _localizationService.GetString("Information");
            var message = _localizationService.GetString("CustomFileSaveNotImplemented");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Event-Handler: Einstellungen öffnen
    /// Zeigt SettingsWindow als modalen Dialog
    /// </summary>
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_configService, _localizationService);
        settingsWindow.Owner = this;
        
        if (settingsWindow.ShowDialog() == true)
        {
            // Einstellungen wurden gespeichert, aktualisiere abhängige UI-Elemente
            UpdateAutoSaveTimer();
        }
    }

    /// <summary>
    /// Event-Handler: Über-Dialog anzeigen
    /// Zeigt grundlegende Informationen über die Anwendung
    /// </summary>
    private void About_Click(object sender, RoutedEventArgs e)
    {
        var title = _localizationService.GetString("About");
        var message = _localizationService.GetString("AboutText");
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Event-Handler: Hilfe anzeigen
    /// Öffnet das HelpWindow mit der Anwendungsdokumentation
    /// </summary>
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

    /// <summary>
    /// Event-Handler: Anwendung beenden
    /// Delegiert an HandleAppExit für einheitliche Exit-Behandlung
    /// </summary>
    private async void Exit_Click(object sender, RoutedEventArgs e)
    {
        await HandleAppExit();
    }

    /// <summary>
    /// Override: Behandelt Fenster-Schließen-Event
    /// Abbrechen des Schließvorgangs und Delegation an HandleAppExit
    /// </summary>
    protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true; // Verhindere Standard-Schließverhalten
        await HandleAppExit(); // Behandle Schließen mit Save-Prompt
    }

    /// <summary>
    /// Behandelt das Beenden der Anwendung mit Save-Prompt
    /// Fragt bei ungespeicherten Änderungen ob gespeichert werden soll
    /// </summary>
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
                        await SaveDataInternal(); // Speichere vor dem Beenden
                        Application.Current.Shutdown();
                    }
                    catch
                    {
                        // Wenn Speichern fehlschlägt, nicht beenden
                        return;
                    }
                    break;
                case MessageBoxResult.No:
                    Application.Current.Shutdown(); // Beenden ohne Speichern
                    break;
                case MessageBoxResult.Cancel:
                    return; // Beenden abbrechen
            }
        }
        else
        {
            Application.Current.Shutdown(); // Kein Save nötig, direkt beenden
        }
    }

    /// <summary>
    /// Event-Handler: Turnier-Übersichtsmodus öffnen
    /// Erstellt und zeigt das TournamentOverviewWindow für Vollbild-Präsentationen
    /// </summary>
    private void OverviewMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Sammle alle TournamentClass-Instanzen
            var tournamentClasses = new List<TournamentClass>
            {
                PlatinTab.TournamentClass,
                GoldTab.TournamentClass,
                SilberTab.TournamentClass,
                BronzeTab.TournamentClass
            };

            // Erstelle und zeige das Übersichtsfenster (nicht modal)
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

    /// <summary>
    /// Event-Handler: Bug-Report-Dialog öffnen
    /// Ermöglicht Benutzern das Melden von Fehlern oder Verbesserungsvorschlägen
    /// </summary>
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

    /// <summary>
    /// Event-Handler: Spenden-Dialog öffnen
    /// Zeigt Informationen zur Unterstützung der Entwicklung
    /// </summary>
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

    /// <summary>
    /// Event-Handler: Druckdialog öffnen
    /// Ermöglicht das Drucken von Turnierstatistiken mit Klassenauswahl
    /// </summary>
    private void Print_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Sammle alle verfügbaren Turnierklassen
            var allTournamentClasses = new List<TournamentClass>
            {
                PlatinTab.TournamentClass,
                GoldTab.TournamentClass,
                SilberTab.TournamentClass,
                BronzeTab.TournamentClass
            };

            // Bestimme die aktuell ausgewählte Turnierklasse basierend auf dem aktiven Tab
            TournamentClass? selectedTournamentClass = null;

            // Ermittle das TabControl direkt
            var mainTabControl = this.Content as Border;
            if (mainTabControl?.Child is Grid mainGrid)
            {
                // Suche das TabControl in der Grid-Struktur
                var contentBorder = mainGrid.Children.OfType<Border>().Skip(1).FirstOrDefault(); // Der zweite Border (Grid.Row="1")
                if (contentBorder?.Child is TabControl tabControl)
                {
                    var selectedTab = tabControl.SelectedItem as TabItem;
                    
                    // Bestimme die TournamentClass basierend auf dem ausgewählten Tab
                    selectedTournamentClass = selectedTab?.Name switch
                    {
                        nameof(PlatinTabItem) => PlatinTab.TournamentClass,
                        nameof(GoldTabItem) => GoldTab.TournamentClass,
                        nameof(SilverTabItem) => SilberTab.TournamentClass,
                        nameof(BronzeTabItem) => BronzeTab.TournamentClass,
                        _ => PlatinTab.TournamentClass // Fallback
                    };
                }
            }

            // Fallback: Verwende Platin wenn nichts gefunden wurde
            selectedTournamentClass ??= PlatinTab.TournamentClass;

            // Verwende den PrintHelper für die Druckfunktionalität mit allen Klassen
            PrintHelper.ShowPrintDialog(allTournamentClasses, selectedTournamentClass, this, _localizationService);
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler"; 
            var message = $"Fehler beim Öffnen des Druckdialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            System.Diagnostics.Debug.WriteLine($"Print_Click: ERROR: {ex.Message}");
        }
    }
}