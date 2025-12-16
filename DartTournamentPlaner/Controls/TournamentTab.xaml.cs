using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DartTournamentPlaner.Helpers;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using DartTournamentPlaner.ViewModels; // ✅ FIX: Import ViewModels namespace for KnockoutMatchViewModel
using DartTournamentPlaner.Services.License; // NEU: License Services

namespace DartTournamentPlaner.Controls;

/// <summary>
/// TournamentTab Control - Refactored für bessere Wartbarkeit
/// Delegiert Verantwortlichkeiten an spezialisierte Manager-Klassen
/// </summary>
public partial class TournamentTab : UserControl, INotifyPropertyChanged, IDisposable
{
    // Core data and services
    private TournamentClass _tournamentClass;
    private LocalizationService? _localizationService;
    private Group? _selectedGroup;
    private Player? _selectedPlayer;
    private string _newPlayerName = string.Empty;

    // Manager classes for different responsibilities
    private TournamentTabUIManager? _uiManager;
    private TournamentTabEventHandlers? _eventHandlers;
    private TournamentTabTranslationManager? _translationManager;

    // NEU: License Services für Statistics-Prüfung
    private LicenseFeatureService? _licenseFeatureService;
    private Services.License.LicenseManager? _licenseManager;

    // Properties
    public TournamentClass TournamentClass
    {
        get => _tournamentClass;
        set
        {
            if (_tournamentClass == value) return;

            if (_tournamentClass != null)
            {
                _tournamentClass.UIRefreshRequested -= OnTournamentUIRefreshRequested;
            }

            _tournamentClass = value;
            
            if (_tournamentClass != null)
            {
                _tournamentClass.UIRefreshRequested += OnTournamentUIRefreshRequested;
                _tournamentClass.ValidateAndRepairPhases();
            }

            OnPropertyChanged();
            
            // Initialize managers with new tournament class
            InitializeManagers();
            
            _uiManager?.UpdateUI();
            
            // ✅ NEU: Aktualisiere Statistik-View mit der neuen Tournament Class
            UpdateStatisticsTab();
            
            // Handle specific phase updates
            if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                Dispatcher.BeginInvoke(() => _uiManager?.RefreshKnockoutView(), DispatcherPriority.Loaded);
            }
            else if (_tournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                Dispatcher.BeginInvoke(() => _uiManager?.RefreshFinalsView(), DispatcherPriority.Loaded);
            }
        }
    }

    public Group? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (_selectedGroup == value) return;
            
            UnsubscribeFromGroupEvents(_selectedGroup);
            _selectedGroup = value;
            SubscribeToGroupEvents(_selectedGroup);
            
            OnPropertyChanged();
            RemovePlayerButton.IsEnabled = false;
            
            _uiManager?.UpdatePlayersView(_selectedGroup);
            _uiManager?.UpdateMatchesView(_selectedGroup);
        }
    }

    public Player? SelectedPlayer
    {
        get => _selectedPlayer;
        set
        {
            _selectedPlayer = value;
            OnPropertyChanged();
            RemovePlayerButton.IsEnabled = value != null;
        }
    }

    public string NewPlayerName
    {
        get => _newPlayerName;
        set
        {
            _newPlayerName = value;
            OnPropertyChanged();
        }
    }

    // Events
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? DataChanged;

    public TournamentTab()
    {
        InitializeComponent();
        DataContext = this;
        _tournamentClass = new TournamentClass();
        
        Loaded += TournamentTab_Loaded;
    }

    private void TournamentTab_Loaded(object sender, RoutedEventArgs e)
    {
        _localizationService = App.LocalizationService;
        if (_localizationService != null)
        {
            _localizationService.PropertyChanged += (s, e) => 
            {
                if (Dispatcher.CheckAccess())
                {
                    UpdateTranslations();
                }
                else
                {
                    Dispatcher.BeginInvoke(() => UpdateTranslations(), DispatcherPriority.Render);
                }
            };
        }
        
        InitializeManagers();
        UpdateTranslations();
    }

    private void InitializeManagers()
    {
        if (_localizationService == null) return;

        // Initialize UI Manager
        _uiManager = new TournamentTabUIManager(_tournamentClass, _localizationService, Dispatcher);
        SetupUIManagerElements();

        // ✅ KORRIGIERT: Übergebe HubService-Getter für QR-Code Integration
        var getHubService = new Func<HubIntegrationService?>(() =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-InitializeManagers] getHubService() callback called");
                
                // Versuche HubService über MainWindow zu finden
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-InitializeManagers] MainWindow found: {mainWindow.GetType().Name}");
                    
                    // ✅ FIX: Hole den LicensedHubService und extrahiere den inneren HubIntegrationService
                    var hubServiceField = mainWindow.GetType()
                        .GetField("_hubService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-InitializeManagers] HubServiceField found: {hubServiceField != null}");
                    
                    var hubServiceValue = hubServiceField?.GetValue(mainWindow);
                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-InitializeManagers] HubServiceValue type: {hubServiceValue?.GetType().Name ?? "null"}");
                    
                    if (hubServiceValue is LicensedHubService licensedHubService)
                    {
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-InitializeManagers] LicensedHubService found, getting inner service...");
                        
                        // Zugriff auf den inneren HubIntegrationService über Reflection
                        var innerServiceField = licensedHubService.GetType()
                            .GetField("_innerHubService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-InitializeManagers] InnerServiceField found: {innerServiceField != null}");
                        
                        var hubService = innerServiceField?.GetValue(licensedHubService) as HubIntegrationService;
                        
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-InitializeManagers] HubIntegrationService retrieved: {hubService != null}");
                        
                        if (hubService != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-InitializeManagers] HubService registered: {hubService.IsRegisteredWithHub}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab-InitializeManagers] HubIntegrationService is null");
                        }
                        
                        return hubService;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab-InitializeManagers] Not a LicensedHubService or null");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab-InitializeManagers] MainWindow not found or wrong type");
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab-InitializeManagers] Error getting HubService: {ex.Message}");
                return null;
            }
        });

        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] Testing getHubService callback during initialization...");
        var testHubService = getHubService();
        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] getHubService test result: {testHubService != null}");

        // Initialize Event Handlers
        _eventHandlers = new TournamentTabEventHandlers(
            _tournamentClass,
            _localizationService,
            () => SelectedGroup,
            () => SelectedPlayer,
            name => NewPlayerName = name,
            () => NewPlayerName,
            () => DataChanged?.Invoke(this, EventArgs.Empty),
            UpdateNextIds,
            () => _uiManager?.UpdatePlayersView(SelectedGroup),
            () => _uiManager?.UpdateMatchesView(SelectedGroup),
            () => _uiManager?.RefreshFinalsView(),
            () => _uiManager?.RefreshKnockoutView(),
            () => Window.GetWindow(this),
            getHubService,
            // ✅ NEU: Callback für Tab-Wechsel
            (phaseType) => SwitchToPhaseTab(phaseType),
            // ✅ NEU: Callback für vollständiges UI-Update
            () => _uiManager?.UpdateUI()
        );

        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] TournamentTabEventHandlers initialized with HubService callback");

        // Initialize Translation Manager
        _translationManager = new TournamentTabTranslationManager(_localizationService);
        SetupTranslationManagerElements();
    }

    private void SetupUIManagerElements()
    {
        if (_uiManager == null) return;
        
        _uiManager.GroupsListBox = GroupsListBox;
        _uiManager.GroupPhaseGroupsList = GroupPhaseGroupsList;
        _uiManager.PlayersListBox = PlayersListBox;
        _uiManager.KnockoutParticipantsListBox = KnockoutParticipantsListBox;
        _uiManager.FinalistsListBox = FinalistsListBox;
        _uiManager.MatchesDataGrid = MatchesDataGrid;
        _uiManager.StandingsDataGrid = StandingsDataGrid;
        _uiManager.KnockoutMatchesDataGrid = KnockoutMatchesDataGrid;
        _uiManager.LoserBracketDataGrid = LoserBracketDataGrid;
        _uiManager.FinalsMatchesDataGrid = FinalsMatchesDataGrid;
        _uiManager.FinalsStandingsDataGrid = FinalsStandingsDataGrid;
        _uiManager.PlayersHeaderText = PlayersHeaderText;
        _uiManager.CurrentPhaseText = CurrentPhaseText;
        _uiManager.TournamentOverviewText = TournamentOverviewText;
        _uiManager.GenerateMatchesButton = GenerateMatchesButton;
        _uiManager.ResetMatchesButton = ResetMatchesButton;
        _uiManager.AdvanceToNextPhaseButton = AdvanceToNextPhaseButton;
        _uiManager.ResetTournamentButton = ResetTournamentButton;
        _uiManager.AddPlayerButton = AddPlayerButton;
        _uiManager.RemoveGroupButton = RemoveGroupButton;
        _uiManager.PlayerNameTextBox = PlayerNameTextBox;
        _uiManager.FinalsTabItem = FinalsTabItem;
        _uiManager.KnockoutTabItem = KnockoutTabItem;
        _uiManager.LoserBracketTab = LoserBracketTab;
        _uiManager.LoserBracketTreeTab = LoserBracketTreeTab;
        _uiManager.BracketCanvas = BracketCanvas;
        _uiManager.LoserBracketCanvas = LoserBracketCanvas;
    }

    private void SetupTranslationManagerElements()
    {
        if (_translationManager == null) return;
        
        _translationManager.SetupTabItem = SetupTabItem;
        _translationManager.GroupPhaseTabItem = GroupPhaseTabItem;
        _translationManager.FinalsTabItem = FinalsTabItem;
        _translationManager.KnockoutTabItem = KnockoutTabItem;
        _translationManager.ConfigureRulesButton = ConfigureRulesButton;
        _translationManager.AddGroupButton = AddGroupButton;
        _translationManager.RemoveGroupButton = RemoveGroupButton;
        _translationManager.AddPlayerButton = AddPlayerButton;
        _translationManager.RemovePlayerButton = RemovePlayerButton;
        _translationManager.GenerateMatchesButton = GenerateMatchesButton;
        _translationManager.ResetMatchesButton = ResetMatchesButton;
        _translationManager.AdvanceToNextPhaseButton = AdvanceToNextPhaseButton;
        _translationManager.ResetTournamentButton = ResetTournamentButton;
        _translationManager.ResetKnockoutButton = ResetKnockoutButton;
        _translationManager.ResetFinalsButton = ResetFinalsButton;
        _translationManager.RefreshUIButton = RefreshUIButton;
        _translationManager.GroupsHeaderText = GroupsHeaderText;
        _translationManager.MatchesHeaderText = MatchesHeaderText;
        _translationManager.StandingsHeaderText = StandingsHeaderText;
        _translationManager.SelectGroupText = SelectGroupText;
        _translationManager.TournamentOverviewHeader = TournamentOverviewHeader;
        _translationManager.GamesTabItem = GamesTabItem;
        _translationManager.TableTabItem = TableTabItem;
        _translationManager.MatchesDataGrid = MatchesDataGrid;
        _translationManager.FinalsMatchesDataGrid = FinalsMatchesDataGrid;
        _translationManager.KnockoutMatchesDataGrid = KnockoutMatchesDataGrid;
        _translationManager.LoserBracketDataGrid = LoserBracketDataGrid; // ✅ NEU
        _translationManager.StandingsDataGrid = StandingsDataGrid;
        _translationManager.PlayersHeaderText = PlayersHeaderText;

        // ✅ NEU: StatisticsTab
        _translationManager.StatisticsTabItem = StatisticsTabItem;
        
        // ✅ NEU: KO-Tab spezifische Elemente (jetzt mit direkten XAML-Referenzen)
        _translationManager.KOParticipantsHeaderText = KOParticipantsHeaderText;
        _translationManager.WinnerBracketHeaderText = WinnerBracketHeaderText;
        _translationManager.LoserBracketHeaderText = LoserBracketHeaderText;
        _translationManager.LoserBracketTabItem = LoserBracketTab;
        _translationManager.LoserBracketTreeTabItem = LoserBracketTreeTab;
        
        // Tab-Items für KO-Bereich (diese müssen zur Laufzeit gefunden werden)
        SetupKnockoutTabReferences();
    }

    /// <summary>
    /// ✅ NEU: Setzt Referenzen für KO-Tab Sub-Tabs zur Laufzeit
    /// </summary>
    private void SetupKnockoutTabReferences()
    {
        try
        {
            var knockoutContent = KnockoutTabItem?.Content as Grid;
            if (knockoutContent != null)
            {
                var tabControl = FindChildOfType<TabControl>(knockoutContent);
                if (tabControl != null)
                {
                    // Finde Tab-Items basierend auf Header-Text (da diese nicht direkt benannt sind)
                    foreach (TabItem tabItem in tabControl.Items.OfType<TabItem>())
                    {
                        var headerText = tabItem.Header?.ToString() ?? "";
                        
                        if (headerText.Contains("Turnierbaum") || headerText.Contains("Tournament Tree"))
                        {
                            _translationManager.TournamentTreeTabItem = tabItem;
                        }
                        else if (headerText.Contains("Winner Bracket") && !headerText.Contains("Loser"))
                        {
                            _translationManager.WinnerBracketTabItem = tabItem;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Error setting up KO tab references: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEU: Hilfsmethode zum Finden von Child-Controls eines bestimmten Typs
    /// </summary>
    private T? FindChildOfType<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is T result)
                return result;
                
            var childResult = FindChildOfType<T>(child);
            if (childResult != null)
                return childResult;
        }
        
        return null;
    }

    /// <summary>
    /// ✅ NEU: Hilfsmethode zum Finden von TextBlocks mit bestimmtem Text-Inhalt
    /// </summary>
    private TextBlock? FindChildTextBlock(DependencyObject parent, string containsText)
    {
        if (parent == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is TextBlock textBlock && textBlock.Text.Contains(containsText))
                return textBlock;
                
            var childResult = FindChildTextBlock(child, containsText);
            if (childResult != null)
                return childResult;
        }
        
        return null;
    }

    public void UpdateTranslations()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] UpdateTranslations() called for {TournamentClass?.Name}");
            
            _translationManager?.UpdateTranslations();
            _uiManager?.UpdateUI(); // ✅ Wichtig: UI aktualisieren inklusive Button-States
            
            // ✅ NEU: Statistiken-Tab aktualisieren
            if (StatisticsView != null && TournamentClass != null)
            {
                try
                {
                    StatisticsView.TournamentClass = TournamentClass;
                    System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Statistics view updated with class: {TournamentClass.Name}");
                }
                catch (Exception statsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Statistics view update error: {statsEx.Message}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] UpdateTranslations() completed for {TournamentClass?.Name}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] UpdateTranslations ERROR: {ex.Message}");
        }
    }

    private void OnTournamentUIRefreshRequested(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                // ✅ FIX: Check current phase and refresh accordingly
                if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnTournamentUIRefreshRequested] Refreshing Finals view");
                    _uiManager?.RefreshFinalsView();
                }
                else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnTournamentUIRefreshRequested] Refreshing Knockout view");
                    _uiManager?.RefreshKnockoutView();
                }
                else
                {
                    // Fallback to UpdateMatchesView for group phase
                    System.Diagnostics.Debug.WriteLine($"[OnTournamentUIRefreshRequested] Refreshing Matches view for phase: {TournamentClass?.CurrentPhase?.PhaseType}");
                    _uiManager?.UpdateMatchesView(SelectedGroup);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnTournamentUIRefreshRequested: {ex.Message}");
            }
        }, DispatcherPriority.DataBind);
    }

    // Event subscription/unsubscription
    private void SubscribeToGroupEvents(Group? group)
    {
        if (group == null) return;
        
        foreach (var match in group.Matches)
        {
            match.PropertyChanged += Match_PropertyChanged;
        }
        group.Matches.CollectionChanged += Matches_CollectionChanged;
    }

    private void UnsubscribeFromGroupEvents(Group? group)
    {
        if (group == null) return;
        
        foreach (var match in group.Matches)
        {
            match.PropertyChanged -= Match_PropertyChanged;
        }
        group.Matches.CollectionChanged -= Matches_CollectionChanged;
    }

    private void Match_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Match.Status) or nameof(Match.Winner))
        {
            _uiManager?.UpdateMatchesView(SelectedGroup);
            
            // ✅ NEU: Aktualisiere Statistiken bei Match-Änderungen UND bei Resets
            Dispatcher.BeginInvoke(() => UpdateStatisticsTab(), DispatcherPriority.Background);
            
            if (sender is Match match && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                var parentWindow = Window.GetWindow(this);
                Task.Run(() => TournamentValidationHelper.CheckAllGroupsCompletion(TournamentClass, parentWindow, _localizationService));
            }
        }
    }

    private void Matches_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (Match match in e.NewItems)
            {
                match.PropertyChanged += Match_PropertyChanged;
            }
        }
        
        if (e.OldItems != null)
        {
            foreach (Match match in e.OldItems)
            {
                match.PropertyChanged -= Match_PropertyChanged;
            }
        }
        
        _uiManager?.UpdateMatchesView(SelectedGroup);
        
        // ✅ NEU: Aktualisiere auch Statistiken wenn Matches hinzugefügt oder entfernt werden (z.B. bei Reset)
        Dispatcher.BeginInvoke(() => UpdateStatisticsTab(), DispatcherPriority.Background);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Event Handlers - delegated to EventHandlers class
    private void ConfigureRulesButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.ConfigureRulesButton_Click(sender, e);

    private void RefreshUIButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔄 RefreshUIButton_Click: Starting UI refresh...");
            System.Diagnostics.Debug.WriteLine($"🔄 Current Phase: {TournamentClass?.CurrentPhase?.PhaseType}");
            System.Diagnostics.Debug.WriteLine($"🔄 Phases count: {TournamentClass?.Phases.Count}");
            
            // ✅ WICHTIG: ValidateAndRepairPhases DARF KEINE Daten löschen!
            // Es soll nur sicherstellen dass die Phase-Struktur konsistent ist
            if (TournamentClass?.CurrentPhase != null)
            {
                System.Diagnostics.Debug.WriteLine("🔄 Skipping ValidateAndRepairPhases to preserve data");
                // NICHT aufrufen: TournamentClass?.ValidateAndRepairPhases();
            }
            
            // Aktualisiere UI Manager (ohne Daten zu ändern)
            _uiManager?.UpdateUI();
            
            // Aktualisiere aktuelle View basierend auf Phase
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine("🔄 Refreshing KO view...");
                System.Diagnostics.Debug.WriteLine($"🔄 Winner Bracket matches: {TournamentClass.CurrentPhase.WinnerBracket?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"🔄 Loser Bracket matches: {TournamentClass.CurrentPhase.LoserBracket?.Count ?? 0}");
                
                // Refreshe KO View
                _uiManager?.RefreshKnockoutView();
                
                // Stelle sicher dass KO-Tab ausgewählt ist
                if (MainTabControl != null && KnockoutTabItem != null)
                {
                    System.Diagnostics.Debug.WriteLine("🔄 Switching to KO tab...");
                    MainTabControl.SelectedItem = KnockoutTabItem;
                }
                
                // Force UI update über Dispatcher
                Dispatcher.BeginInvoke(() =>
                {
                    System.Diagnostics.Debug.WriteLine("🔄 Dispatcher: Forcing KO view refresh...");
                    _uiManager?.RefreshKnockoutView();
                }, DispatcherPriority.Loaded);
            }
            else if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine("🔄 Refreshing Finals view...");
                _uiManager?.RefreshFinalsView();
                
                // Stelle sicher dass Finals-Tab ausgewählt ist
                if (MainTabControl != null && FinalsTabItem != null)
                {
                    MainTabControl.SelectedItem = FinalsTabItem;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔄 Refreshing Group phase view...");
                _uiManager?.UpdatePlayersView(SelectedGroup);
                _uiManager?.UpdateMatchesView(SelectedGroup);
            }
            
            // Aktualisiere Phase Display
            _uiManager?.UpdatePhaseDisplay();
            
            System.Diagnostics.Debug.WriteLine("✅ RefreshUIButton_Click: UI refresh complete");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ RefreshUIButton_Click ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            var title = _localizationService?.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Aktualisieren der UI: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetFinalsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (TournamentDialogHelper.ShowResetFinalsConfirmation(Window.GetWindow(this), _localizationService))
            {
                // ✅ GEÄNDERT: Verwende neue Methode die nur Finals-Phase zurücksetzt
                TournamentKnockoutHelper.ResetFinalsPhaseOnly(TournamentClass);
                _uiManager?.ClearKnockoutCanvases();
                
                _uiManager?.UpdateUI();
                _uiManager?.UpdatePlayersView(SelectedGroup);
                _uiManager?.UpdateMatchesView(SelectedGroup);
                _uiManager?.UpdatePhaseDisplay();
   
                if (MainTabControl != null && GroupPhaseTabItem != null)
                {
                    MainTabControl.SelectedItem = GroupPhaseTabItem;
                }
                
                var successMessage = _localizationService?.GetString("ResetFinalsComplete") ?? "Die Finalrunde wurde erfolgreich zurückgesetzt.";
                TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService, Window.GetWindow(this));
  
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void ResetKnockoutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (TournamentDialogHelper.ShowResetKnockoutConfirmation(Window.GetWindow(this), _localizationService))
            {
                // ✅ GEÄNDERT: Verwende neue Methode die nur KO-Phase zurücksetzt
                TournamentKnockoutHelper.ResetKnockoutPhaseOnly(TournamentClass);
                _uiManager?.ClearKnockoutCanvases();
                
                _uiManager?.UpdateUI();
                _uiManager?.UpdatePlayersView(SelectedGroup);
                _uiManager?.UpdateMatchesView(SelectedGroup);
                _uiManager?.UpdatePhaseDisplay();
   
                if (MainTabControl != null && GroupPhaseTabItem != null)
                {
                    MainTabControl.SelectedItem = GroupPhaseTabItem;
                }
                
                var successMessage = _localizationService?.GetString("ResetKnockoutComplete") ?? "Die K.-o.-Phase wurde erfolgreich zurückgesetzt.";
                TournamentDialogHelper.ShowInformation(successMessage, null, _localizationService, Window.GetWindow(this));
  
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService?.GetString("ErrorResettingTournament") ?? "Fehler beim Zurücksetzen:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source == MainTabControl && MainTabControl.SelectedItem is TabItem selectedTab)
        {
            try
            {
                if (selectedTab.Name == "FinalsTabItem" && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
                {
                    Dispatcher.BeginInvoke(() => _uiManager?.RefreshFinalsView(), DispatcherPriority.DataBind);
                }
                else if (selectedTab.Name == "KnockoutTabItem" && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
                {
                    Dispatcher.BeginInvoke(() => _uiManager?.RefreshKnockoutView(), DispatcherPriority.DataBind);
                }
                else if (selectedTab.Name == "GroupPhaseTabItem" && TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
                {
                    _uiManager?.UpdateMatchesView(SelectedGroup);
                }
                // ✅ NEU: Aktualisiere Statistiken wenn Statistik-Tab ausgewählt wird
                else if (selectedTab.Name == "StatisticsTabItem" || selectedTab.Header?.ToString()?.Contains("Statistik") == true)
                {
                    System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Statistics tab selected, updating with class: {TournamentClass?.Name}");
                    Dispatcher.BeginInvoke(() => UpdateStatisticsTab(), DispatcherPriority.DataBind);
                }
                
                _uiManager?.UpdatePhaseDisplay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainTabControl_SelectionChanged: ERROR: {ex.Message}");
            }
        }
    }

    private void AdvanceToNextPhaseButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.AdvanceToNextPhaseButton_Click(sender, e, SwitchToTab);

    private void SwitchToTab(TournamentPhaseType phaseType)
    {
        if (MainTabControl == null) return;
        
        switch (phaseType)
        {
            case TournamentPhaseType.RoundRobinFinals:
                MainTabControl.SelectedItem = FinalsTabItem;
                break;
            case TournamentPhaseType.KnockoutPhase:
                MainTabControl.SelectedItem = KnockoutTabItem;
                break;
        }
    }
    
    /// <summary>
    /// ✅ NEU: Öffentliche Methode für Phase-Tab-Wechsel (für Event Handlers)
    /// </summary>
    private void SwitchToPhaseTab(TournamentPhaseType phaseType)
    {
        System.Diagnostics.Debug.WriteLine($"🔄 SwitchToPhaseTab called for phase: {phaseType}");
        
        if (MainTabControl == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ MainTabControl is null!");
            return;
        }
        
        Dispatcher.BeginInvoke(() =>
        {
            try
            {
                switch (phaseType)
                {
                    case TournamentPhaseType.KnockoutPhase:
                        System.Diagnostics.Debug.WriteLine($"✅ Switching to KnockoutTabItem");
                        if (KnockoutTabItem != null)
                        {
                            MainTabControl.SelectedItem = KnockoutTabItem;
                            System.Diagnostics.Debug.WriteLine($"✅ KnockoutTabItem selected: {MainTabControl.SelectedItem == KnockoutTabItem}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("❌ KnockoutTabItem is null!");
                        }
                        break;
                        
                    case TournamentPhaseType.RoundRobinFinals:
                        System.Diagnostics.Debug.WriteLine($"✅ Switching to FinalsTabItem");
                        if (FinalsTabItem != null)
                        {
                            MainTabControl.SelectedItem = FinalsTabItem;
                        }
                        break;
                        
                    case TournamentPhaseType.GroupPhase:
                        System.Diagnostics.Debug.WriteLine($"✅ Switching to GroupPhaseTabItem");
                        if (GroupPhaseTabItem != null)
                        {
                            MainTabControl.SelectedItem = GroupPhaseTabItem;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error switching tab: {ex.Message}");
            }
        }, DispatcherPriority.Normal);
    }

    private void ResetMatchesButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.ResetMatchesButton_Click(sender, e);

    private void ResetTournamentButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.ResetTournamentButton_Click(sender, e);

    private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.AddGroupButton_Click(sender, e);

    private void RemoveGroupButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.RemoveGroupButton_Click(sender, e, GroupsListBox);

    private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedGroup = GroupsListBox.SelectedItem as Group;
        RemoveGroupButton.IsEnabled = SelectedGroup != null;
    }

    private void PlayerNameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && AddPlayerButton.IsEnabled)
        {
            _eventHandlers?.AddPlayerButton_Click(sender, new RoutedEventArgs());
        }
    }

    private void AddPlayerButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.AddPlayerButton_Click(sender, e);

    private void RemovePlayerButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.RemovePlayerButton_Click(sender, e);

    private void GenerateMatchesButton_Click(object sender, RoutedEventArgs e)
        => _eventHandlers?.GenerateMatchesButton_Click(sender, e);

    private void PlayersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => SelectedPlayer = PlayersListBox.SelectedItem as Player;

    private void GroupPhaseGroupsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedGroup = GroupPhaseGroupsList.SelectedItem as Group;
        _uiManager?.UpdateMatchesView(SelectedGroup);
    }

    private async void FinalsMatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (FinalsMatchesDataGrid.SelectedItem is Match selectedMatch && _eventHandlers != null)
        {
            await _eventHandlers.HandleMatchDoubleClick(selectedMatch, "Finals");
        }
    }

    private async void KnockoutMatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // ✅ FIX: Cast to KnockoutMatchViewModel and extract the match
            if (KnockoutMatchesDataGrid.SelectedItem is KnockoutMatchViewModel viewModel && 
                viewModel.Match is KnockoutMatch selectedMatch && 
                _eventHandlers != null)
            {
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] KnockoutMatchesDataGrid_MouseDoubleClick called for match {selectedMatch.Id}");
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] About to call HandleKnockoutMatchDoubleClick...");
                
                await _eventHandlers.HandleKnockoutMatchDoubleClick(selectedMatch, "Winner Bracket");
                
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] HandleKnockoutMatchDoubleClick completed successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab] KnockoutMatchesDataGrid_MouseDoubleClick - selectedItem available: {KnockoutMatchesDataGrid.SelectedItem != null}, _eventHandlers: {_eventHandlers != null}");
                if (KnockoutMatchesDataGrid.SelectedItem != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab] SelectedItem type: {KnockoutMatchesDataGrid.SelectedItem.GetType().Name}");
                    
                    // ✅ DEBUG: Try to access the Match property if it's a ViewModel
                    if (KnockoutMatchesDataGrid.SelectedItem is KnockoutMatchViewModel vm)
                    {
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] ViewModel found, Match: {vm.Match?.Id}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab] Exception in KnockoutMatchesDataGrid_MouseDoubleClick: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab] StackTrace: {ex.StackTrace}");
        }
    }

    private async void LoserBracketDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // ✅ FIX: Cast to KnockoutMatchViewModel and extract the match
            if (LoserBracketDataGrid.SelectedItem is KnockoutMatchViewModel viewModel && 
                viewModel.Match is KnockoutMatch selectedMatch && 
                _eventHandlers != null)
            {
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] LoserBracketDataGrid_MouseDoubleClick called for match {selectedMatch.Id}");
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] About to call HandleKnockoutMatchDoubleClick...");
                
                await _eventHandlers.HandleKnockoutMatchDoubleClick(selectedMatch, "Loser Bracket");
                
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] HandleKnockoutMatchDoubleClick completed successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab] LoserBracketDataGrid_MouseDoubleClick - selectedItem available: {LoserBracketDataGrid.SelectedItem != null}, _eventHandlers: {_eventHandlers != null}");
                if (LoserBracketDataGrid.SelectedItem != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab] SelectedItem type: {LoserBracketDataGrid.SelectedItem.GetType().Name}");
                    
                    // ✅ DEBUG: Try to access the Match property if it's a ViewModel
                    if (LoserBracketDataGrid.SelectedItem is KnockoutMatchViewModel vm)
                    {
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab] ViewModel found, Match: {vm.Match?.Id}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab] Exception in LoserBracketDataGrid_MouseDoubleClick: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab] StackTrace: {ex.StackTrace}");
        }
    }

    private async void MatchesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (MatchesDataGrid.SelectedItem is Match selectedMatch && _eventHandlers != null)
        {
            await _eventHandlers.HandleMatchDoubleClick(selectedMatch, "Group");
        }
    }
    
    // Context Menu Handler für Knockout DataGrids
    private void KnockoutDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (sender is not DataGrid dataGrid)
                return;
                
            if (dataGrid.SelectedItem is not KnockoutMatchViewModel viewModel)
                return;
                
            var match = viewModel.Match;
            if (match == null)
                return;
                
            System.Diagnostics.Debug.WriteLine($"🖱️ [Context Menu] Right-click on match {match.Id}");
            
            // Erstelle Context Menu
            var contextMenu = new ContextMenu();
            
            // ✅ 1. Match Ergebnis eingeben / bearbeiten
            if (match.Player1 != null && match.Player2 != null)
            {
                var editMenuItem = new MenuItem
                {
                    Header = match.Status == MatchStatus.NotStarted ? "📝 Ergebnis eingeben" : "✏️ Ergebnis bearbeiten",
                    Icon = new TextBlock { Text = match.Status == MatchStatus.NotStarted ? "📝" : "✏️", FontSize = 12 }
                };
                editMenuItem.Click += async (s, args) => 
                {
                    await _eventHandlers.HandleKnockoutMatchDoubleClick(match, match.BracketType == BracketType.Winner ? "Winner Bracket" : "Loser Bracket");
                };
                contextMenu.Items.Add(editMenuItem);
                
                contextMenu.Items.Add(new Separator());
            }
            
            // ✅ 2. Bye Management
            if (match.Player1 != null && match.Player2 != null && match.Status == MatchStatus.NotStarted)
            {
                // Freilos an Player 1
                var byePlayer1 = new MenuItem
                {
                    Header = $"🎯 Freilos an {match.Player1.Name}",
                    Icon = new TextBlock { Text = "🎯", FontSize = 12 }
                };
                byePlayer1.Click += (s, args) => 
                {
                    if (TournamentKnockoutHelper.ProcessByeSelection(TournamentClass, match, Window.GetWindow(this), _localizationService))
                    {
                        _uiManager?.RefreshKnockoutView();
                        DataChanged?.Invoke(this, EventArgs.Empty);
                    }
                };
                contextMenu.Items.Add(byePlayer1);
                
                // Freilos an Player 2
                var byePlayer2 = new MenuItem
                {
                    Header = $"🎯 Freilos an {match.Player2.Name}",
                    Icon = new TextBlock { Text = "🎯", FontSize = 12 }
                };
                byePlayer2.Click += (s, args) => 
                {
                    if (TournamentKnockoutHelper.ProcessByeSelection(TournamentClass, match, Window.GetWindow(this), _localizationService))
                    {
                        _uiManager?.RefreshKnockoutView();
                        DataChanged?.Invoke(this, EventArgs.Empty);
                    }
                };
                contextMenu.Items.Add(byePlayer2);
            }

            if (match.Status == MatchStatus.Bye)
            {
                var undoBye = new MenuItem
                {
                    Header = "↩️ Freilos rückgängig",
                    Icon = new TextBlock { Text = "↩️", FontSize = 12 }
                };
                undoBye.Click += (s, args) => 
                {
                    if (TournamentKnockoutHelper.HandleUndoKnockoutBye(TournamentClass, match, _localizationService))
                    {
                        _uiManager?.RefreshKnockoutView();
                        DataChanged?.Invoke(this, EventArgs.Empty);
                    }
                };
                contextMenu.Items.Add(undoBye);
            }
            
            // Zeige Menu nur wenn Items vorhanden
            if (contextMenu.Items.Count > 0)
            {
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [Context Menu] Error: {ex.Message}");
        }
    }

    private void GiveByeButton_Click(object sender, RoutedEventArgs e)
    {
        KnockoutMatch? match = null;

        if (sender is Button button)
        {
            // ✅ FIX: Tag ist jetzt KnockoutMatchViewModel!
            if (button.Tag is KnockoutMatchViewModel viewModel)
            {
                match = viewModel.Match;
            }
            else if (button.Tag is KnockoutMatch knockoutMatch)
            {
                match = knockoutMatch;
            }
        }

        if (match != null)
        {
            if (TournamentKnockoutHelper.ProcessByeSelection(TournamentClass, match, Window.GetWindow(this), _localizationService))
            {
                _uiManager?.RefreshKnockoutView();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void UndoByeButton_Click(object sender, RoutedEventArgs e)
    {
        KnockoutMatch? match = null;

        if (sender is Button button)
        {
            // ✅ FIX: Tag ist jetzt KnockoutMatchViewModel!
            if (button.Tag is KnockoutMatchViewModel viewModel)
            {
                match = viewModel.Match;
            }
            else if (button.Tag is KnockoutMatch knockoutMatch)
            {
                match = knockoutMatch;
            }
        }

        if (match != null)
        {
            if (TournamentKnockoutHelper.HandleUndoKnockoutBye(TournamentClass, match, _localizationService))
            {
                _uiManager?.RefreshKnockoutView();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void EditMatchResult_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Match match }) return;

        try
        {
            // ✅ FIXED: HubIntegrationService UND Tournament-ID über TournamentManagementService holen
            HubIntegrationService? hubService = null;
            string? tournamentId = null;
 
            try
            {
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] Starting HubService and TournamentId retrieval...");
        
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] MainWindow found: {mainWindow.GetType().Name}");
       
                    // Zugriff auf den LicensedHubService über Reflection
                    var hubServiceField = mainWindow.GetType()
                        .GetField("_hubService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
  
                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] HubServiceField found: {hubServiceField != null}");
    
                    var hubServiceValue = hubServiceField?.GetValue(mainWindow);
                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] HubServiceValue type: {hubServiceValue?.GetType().Name ?? "null"}");
 
                    if (hubServiceValue is LicensedHubService licensedHubService)
                    {
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] LicensedHubService found, getting inner service...");

                        // Zugriff auf den inneren HubIntegrationService über Reflection
                        var innerServiceField = licensedHubService.GetType()
                            .GetField("_innerHubService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
     
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] InnerServiceField found: {innerServiceField != null}");
  
                        hubService = innerServiceField?.GetValue(licensedHubService) as HubIntegrationService;
        
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] HubIntegrationService retrieved: {hubService != null}");
    
                        if (hubService != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] HubService registered: {hubService.IsRegisteredWithHub}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab-EditMatchResult] HubIntegrationService is null");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab-EditMatchResult] Not a LicensedHubService or null");
                    }
   
                    // ⭐ KORRIGIERT: Hole Tournament-ID über TournamentManagementService
                    var tournamentServiceField = mainWindow.GetType()
                        .GetField("_tournamentService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
   
                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] TournamentServiceField found: {tournamentServiceField != null}");
   
                    if (tournamentServiceField?.GetValue(mainWindow) is TournamentManagementService tournamentService)
                    {
                        var tournamentData = tournamentService.GetTournamentData();
                        tournamentId = tournamentData?.TournamentId;
 
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] Tournament ID from TournamentService: {tournamentId ?? "null"}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentTab-EditMatchResult] Could not get TournamentManagementService");
                    }
                }
            }
            catch (Exception hubEx)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentTab-EditMatchResult] Could not get HubService or TournamentId: {hubEx.Message}");
                System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentTab-EditMatchResult] StackTrace: {hubEx.StackTrace}");
            }

            var gameRules = TournamentClass?.GameRules ?? new GameRules();
  
            // ✅ FIXED: HubService UND Tournament-ID als Parameter übergeben
            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] Creating MatchResultWindow with HubService: {hubService != null}, TournamentId: {tournamentId ?? "null"}");
            var dialog = new MatchResultWindow(match, gameRules, _localizationService, hubService, tournamentId);
  
            if (dialog.ShowDialog() == true)
            {
                TournamentClass?.TriggerUIRefresh();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            var title = _localizationService?.GetString("Error");
            var message = $"{_localizationService?.GetString("ErrorEditingMatchResult")}\n\n{ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void UpdateNextIds()
    {
        // This method is kept for backward compatibility
        // The actual logic is now handled by the EventHandlers class
        // which calculates the next IDs dynamically
    }
    
    /// <summary>
    /// ✅ NEU: Aktualisiert die Statistik-Ansicht
    /// </summary>
    private void UpdateStatisticsTab()
    {
        try
        {
            if (StatisticsView != null && TournamentClass != null)
            {
                try
                {
                    // Lade License Services wenn noch nicht vorhanden
                    if (_licenseFeatureService == null && Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        try
                        {
                            var licenseServiceField = mainWindow.GetType()
                                .GetField("_licenseFeatureService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            _licenseFeatureService = licenseServiceField?.GetValue(mainWindow) as LicenseFeatureService;
                            
                            var licenseManagerField = mainWindow.GetType()
                                .GetField("_licenseManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            _licenseManager = licenseManagerField?.GetValue(mainWindow) as Services.License.LicenseManager;
                        }
                        catch (Exception licEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Could not get License services: {licEx.Message}");
                        }
                    }
                    
                    // Setze License Services in StatisticsView
                    if (_licenseFeatureService != null)
                    {
                        var licenseFeatureServiceProperty = StatisticsView.GetType()
                            .GetProperty("LicenseFeatureService", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        licenseFeatureServiceProperty?.SetValue(StatisticsView, _licenseFeatureService);
                    }
                    
                    if (_licenseManager != null)
                    {
                        var licenseManagerProperty = StatisticsView.GetType()
                            .GetProperty("LicenseManager", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        licenseManagerProperty?.SetValue(StatisticsView, _licenseManager);
                    }
                    
                    // Setze TournamentClass
                    StatisticsView.TournamentClass = TournamentClass;
                    System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Statistics view updated with class: {TournamentClass.Name}");
                }
                catch (Exception statsEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Statistics view update error: {statsEx.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] UpdateStatisticsTab ERROR: {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        _uiManager?.Dispose();
        _eventHandlers?.Dispose();
        _translationManager?.Dispose();
    }
}