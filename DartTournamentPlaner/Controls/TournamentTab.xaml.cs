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
            getHubService
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
                _uiManager?.RefreshKnockoutView();
                _uiManager?.UpdateMatchesView(SelectedGroup);
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
            _uiManager?.UpdateUI();
            _uiManager?.UpdatePlayersView(SelectedGroup);
            _uiManager?.UpdateMatchesView(SelectedGroup);
            _uiManager?.UpdatePhaseDisplay();
            
            if (TournamentClass?.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                _uiManager?.RefreshKnockoutView();
            }
            
            var parentGrid = Window.GetWindow(this)?.Content as Grid;
            if (parentGrid != null)
            {
                var successMessage = _localizationService?.GetString("UIRefreshed") ?? "Benutzeroberfläche wurde aktualisiert";
                var title = _localizationService?.GetString("Information") ?? "Information";
                TournamentUIHelper.ShowToastNotification(parentGrid, title, successMessage);
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{_localizationService?.GetString("ErrorRefreshing") ?? "Fehler beim Aktualisieren:"} {ex.Message}";
            TournamentDialogHelper.ShowError(errorMessage, null, _localizationService);
        }
    }

    private void ResetFinalsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (TournamentDialogHelper.ShowResetFinalsConfirmation(Window.GetWindow(this), _localizationService))
            {
                TournamentKnockoutHelper.ResetToGroupPhase(TournamentClass);
                _uiManager?.ClearKnockoutCanvases();
                
                _uiManager?.UpdateUI();
                _uiManager?.UpdatePlayersView(SelectedGroup);
                _uiManager?.UpdateMatchesView(SelectedGroup);
                _uiManager?.UpdatePhaseDisplay();
                
                if (MainTabControl != null && SetupTabItem != null)
                {
                    MainTabControl.SelectedItem = SetupTabItem;
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
                TournamentKnockoutHelper.ResetToGroupPhase(TournamentClass);
                _uiManager?.ClearKnockoutCanvases();
                
                _uiManager?.UpdateUI();
                _uiManager?.UpdatePlayersView(SelectedGroup);
                _uiManager?.UpdateMatchesView(SelectedGroup);
                _uiManager?.UpdatePhaseDisplay();
                
                if (MainTabControl != null && SetupTabItem != null)
                {
                    MainTabControl.SelectedItem = SetupTabItem;
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

    private void EditMatchResult_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Match match }) return;

        try
        {
            // ✅ FIX: HubIntegrationService vom MainWindow holen und an MatchResultWindow übergeben
            HubIntegrationService? hubService = null;
            try
            {
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] Starting HubService retrieval...");
                
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
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [TournamentTab-EditMatchResult] MainWindow not found or wrong type");
                }
            }
            catch (Exception hubEx)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentTab-EditMatchResult] Could not get HubService: {hubEx.Message}");
                System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentTab-EditMatchResult] StackTrace: {hubEx.StackTrace}");
            }

            var gameRules = TournamentClass?.GameRules ?? new GameRules();
            
            // ✅ FIX: HubService als Parameter übergeben
            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTab-EditMatchResult] Creating MatchResultWindow with HubService: {hubService != null}");
            var dialog = new MatchResultWindow(match, gameRules, _localizationService, hubService);
            
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

    // Knockout-specific event handlers
    private void KnockoutDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Placeholder for context menu functionality
    }

    private void GiveByeButton_Click(object sender, RoutedEventArgs e)
    {
        KnockoutMatch? match = null;

        if (sender is Button button)
        {
            if (button.Tag is KnockoutMatch knockoutMatch)
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
            if (button.Tag is KnockoutMatch knockoutMatch)
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
            if (TournamentClass == null)
            {
                System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] No TournamentClass available for statistics");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Updating statistics tab for class: {TournamentClass.Name}");

            // NEU: Lizenzprüfung für Statistics Feature
            var hasStatisticsLicense = CheckStatisticsLicense();
            
            if (!hasStatisticsLicense)
            {
                System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Statistics license not available - showing license required control");
                ShowStatisticsLicenseRequired();
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Statistics license verified - showing statistics view");
                ShowStatisticsView();
            }

            // ✅ KORRIGIERT: Finde StatisticsView im Control-Tree
            PlayerStatisticsView? statisticsView = null;

            // Suche nach PlayerStatisticsView in den Tabs
            if (MainTabControl?.Items != null)
            {
                foreach (TabItem tab in MainTabControl.Items)
                {
                    // Prüfe direkt auf Content
                    if (tab.Content is PlayerStatisticsView psv)
                    {
                        statisticsView = psv;
                        break;
                    }
                    
                    // Suche in Container
                    statisticsView = FindStatisticsViewInContainer(tab.Content);
                    if (statisticsView != null) break;
                }
            }

            if (statisticsView != null)
            {
                // Validiere und repariere Statistiken falls nötig
                TournamentClass.ValidateAndRepairStatistics();

                // ✅ NEU: Update translations first
                statisticsView.UpdateTranslations();

                // Aktualisiere die statistik-View
                statisticsView.TournamentClass = TournamentClass;

                System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Statistics tab updated successfully with class: {TournamentClass.Name}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] PlayerStatisticsView not found in control tree");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TOURNAMENT-TAB] Error updating statistics tab: {ex.Message}");
            var title = _localizationService.GetString("Error");
            var message = _localizationService.GetString("ErrorLoadingStatistics", ex.Message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// NEU: Prüft ob Statistics-Lizenz vorhanden ist
    /// </summary>
    private bool CheckStatisticsLicense()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔍 CheckStatisticsLicense: Starting license check...");
            
            // Hole License Services vom MainWindow
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                // Verwende Reflection um auf private Felder zuzugreifen
                var licenseFeatureServiceField = mainWindow.GetType()
                    .GetField("_licenseFeatureService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var licenseManagerField = mainWindow.GetType()
                    .GetField("_licenseManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                _licenseFeatureService = licenseFeatureServiceField?.GetValue(mainWindow) as LicenseFeatureService;
                _licenseManager = licenseManagerField?.GetValue(mainWindow) as Services.License.LicenseManager;

                System.Diagnostics.Debug.WriteLine($"🔍 CheckStatisticsLicense: LicenseFeatureService found: {_licenseFeatureService != null}");
                System.Diagnostics.Debug.WriteLine($"🔍 CheckStatisticsLicense: LicenseManager found: {_licenseManager != null}");

                if (_licenseFeatureService != null)
                {
                    var status = _licenseFeatureService.CurrentStatus;
                    var hasStatistics = _licenseFeatureService.HasFeature(DartTournamentPlaner.Models.License.LicenseFeatures.STATISTICS);
                    
                    System.Diagnostics.Debug.WriteLine($"🔍 Statistics License Check:");
                    System.Diagnostics.Debug.WriteLine($"   - License Service available: TRUE");
                    System.Diagnostics.Debug.WriteLine($"   - Status.IsLicensed: {status?.IsLicensed ?? false}");
                    System.Diagnostics.Debug.WriteLine($"   - Status.IsValid: {status?.IsValid ?? false}");
                    System.Diagnostics.Debug.WriteLine($"   - HasFeature(STATISTICS): {hasStatistics}");
                    System.Diagnostics.Debug.WriteLine($"   - ActiveFeatures Count: {status?.ActiveFeatures?.Count ?? 0}");
                    
                    if (status?.ActiveFeatures != null && status.ActiveFeatures.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"   - Active Features: {string.Join(", ", status.ActiveFeatures)}");
                    }
                    
                    var result = status?.IsLicensed == true && hasStatistics;
                    System.Diagnostics.Debug.WriteLine($"🔍 CheckStatisticsLicense: Final result: {result}");
                    return result;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ CheckStatisticsLicense: MainWindow not found or wrong type");
            }

            System.Diagnostics.Debug.WriteLine("⚠️ License services not available - allowing statistics access");
            return true; // Fallback: erlaubt Zugriff wenn Service nicht verfügbar
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error checking statistics license: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            return true; // Fallback: erlaubt Zugriff bei Fehlern
        }
    }

    /// <summary>
    /// NEU: Zeigt das Statistics License Required Dialog
    /// </summary>
    private void ShowStatisticsLicenseRequired()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🔒 ShowStatisticsLicenseRequired: Starting...");
            
            // Verstecke Statistics View
            if (StatisticsView != null)
            {
                StatisticsView.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine("✅ StatisticsView hidden");
            }

            // NEU: Zeige das moderne StatisticsLicenseRequiredDialog
            StatisticsLicenseRequiredDialog.ShowLicenseRequiredDialog(
                Window.GetWindow(this),
                _localizationService,
                _licenseManager
            );
            
            System.Diagnostics.Debug.WriteLine("✅ StatisticsLicenseRequiredDialog shown");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error showing statistics license required: {ex.Message}");
        }
    }

    /// <summary>
    /// NEU: Zeigt die normale Statistics View
    /// </summary>
    private void ShowStatisticsView()
    {
        try
        {
            // Zeige Statistics View
            if (StatisticsView != null)
            {
                StatisticsView.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine("✅ StatisticsView set to visible");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error showing statistics view: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEU: Rekursive Suche nach PlayerStatisticsView in Container-Elementen
    /// </summary>
    private PlayerStatisticsView? FindStatisticsViewInContainer(object? container)
    {
        if (container == null) return null;

        if (container is PlayerStatisticsView statisticsView)
        {
            return statisticsView;
        }

        if (container is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                var result = FindStatisticsViewInContainer(child);
                if (result != null) return result;
            }
        }

        if (container is ContentControl contentControl)
        {
            return FindStatisticsViewInContainer(contentControl.Content);
        }

        if (container is Decorator decorator)
        {
            return FindStatisticsViewInContainer(decorator.Child);
        }

        return null;
    }

    /// <summary>
    /// Dispose-Pattern für ordnungsgemäße Ressourcenverwaltung
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Geschützte Dispose-Methode
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                // Tournament Class Events abmelden
                if (_tournamentClass != null)
                {
                    _tournamentClass.UIRefreshRequested -= OnTournamentUIRefreshRequested;
                }

                // Unsubscribe from group events
                UnsubscribeFromGroupEvents(_selectedGroup);

                // Localization Service Events abmelden
                if (_localizationService != null)
                {
                    // Das Event wird automatisch aufgeräumt wenn das Control disposed wird
                }

                // Manager Classes ordnungsgemäß disposed
                _uiManager?.Dispose();
                _eventHandlers?.Dispose();
                _translationManager?.Dispose();

                System.Diagnostics.Debug.WriteLine($"[TournamentTab] Disposed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TournamentTab] Error during dispose: {ex.Message}");
            }
        }
    }
}