using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class TournamentOverviewWindow : Window
{
    private readonly List<TournamentClass> _tournamentClasses;
    private readonly LocalizationService _localizationService;
    private readonly DispatcherTimer _cycleTimer;
    private int _currentClassIndex = 0;
    private int _currentSubTabIndex = 0;
    private bool _isRunning = false;
    private List<TabItem> _activeTournamentTabs = new();
    private DateTime _lastCycleTime = DateTime.Now;
    
    // Configuration values - stored internally
    private int _classInterval = 10; // seconds
    private int _subTabInterval = 5; // seconds
    private bool _showOnlyActiveClasses = true;

    public TournamentOverviewWindow(
        List<TournamentClass> tournamentClasses,
        LocalizationService localizationService)
    {
        InitializeComponent();
        
        _tournamentClasses = tournamentClasses;
        _localizationService = localizationService;
        
        _cycleTimer = new DispatcherTimer();
        _cycleTimer.Tick += CycleTimer_Tick;
        
        InitializeOverview();
        UpdateTranslations();
    }

    private void InitializeOverview()
    {
        MainTabControl.Items.Clear();
        _activeTournamentTabs.Clear();

        // Create tabs for tournament classes that have groups
        for (int i = 0; i < _tournamentClasses.Count; i++)
        {
            var tournamentClass = _tournamentClasses[i];
            
            // Skip classes without groups if "Only Active Classes" is enabled
            if (_showOnlyActiveClasses && 
                (tournamentClass.Groups == null || tournamentClass.Groups.Count == 0))
                continue;

            var tabItem = CreateTournamentClassTab(tournamentClass, i);
            MainTabControl.Items.Add(tabItem);
            _activeTournamentTabs.Add(tabItem);
        }

        if (MainTabControl.Items.Count > 0)
        {
            MainTabControl.SelectedIndex = 0;
            _currentClassIndex = 0;
            _currentSubTabIndex = 0;
            
            // Set initial sub-tab
            SetCurrentSubTab();
        }

        UpdateStatus();
    }

    private TabItem CreateTournamentClassTab(TournamentClass tournamentClass, int classIndex)
    {
        var tabItem = new TabItem();
        
        // Create header with color indicator
        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
        
        var colorEllipse = new System.Windows.Shapes.Ellipse
        {
            Width = 12,
            Height = 12,
            Margin = new Thickness(0, 0, 8, 0)
        };

        // Set colors based on class
        switch (classIndex)
        {
            case 0: // Platinum
                colorEllipse.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(156, 39, 176));
                break;
            case 1: // Gold
                colorEllipse.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0));
                break;
            case 2: // Silver
                colorEllipse.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(121, 85, 72));
                break;
            case 3: // Bronze
                colorEllipse.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(141, 110, 99));
                break;
        }

        var headerText = new TextBlock
        {
            Text = tournamentClass.Name,
            FontWeight = FontWeights.Medium
        };

        headerPanel.Children.Add(colorEllipse);
        headerPanel.Children.Add(headerText);
        tabItem.Header = headerPanel;

        // Create content area with sub-tabs based on current phase
        var contentTabControl = CreateContentTabControl(tournamentClass);
        tabItem.Content = contentTabControl;

        return tabItem;
    }

    private TabControl CreateContentTabControl(TournamentClass tournamentClass)
    {
        var tabControl = new TabControl
        {
            Margin = new Thickness(10),
            Background = System.Windows.Media.Brushes.Transparent
        };

        // Check current phase and create appropriate sub-tabs
        var currentPhase = tournamentClass.CurrentPhase;
        
        if (currentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
        {
            // Create tabs for each group
            foreach (var group in tournamentClass.Groups.Where(g => g.Players.Count > 0))
            {
                var groupTab = CreateGroupTab(group, tournamentClass);
                tabControl.Items.Add(groupTab);
            }
        }
        else if (currentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            // Create tabs for knockout brackets
            var winnerBracketTab = CreateKnockoutBracketTab("Winner Bracket", tournamentClass, false);
            tabControl.Items.Add(winnerBracketTab);

            // Add Loser Bracket if double elimination
            if (tournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                var loserBracketTab = CreateKnockoutBracketTab("Loser Bracket", tournamentClass, true);
                tabControl.Items.Add(loserBracketTab);
            }
        }
        else if (currentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
            // Create finals tab
            var finalsTab = CreateFinalsTab(tournamentClass);
            tabControl.Items.Add(finalsTab);
        }

        if (tabControl.Items.Count > 0)
        {
            tabControl.SelectedIndex = 0;
        }

        return tabControl;
    }

    private TabItem CreateGroupTab(Group group, TournamentClass tournamentClass)
    {
        var tabItem = new TabItem
        {
            Header = group.Name
        };

        // Create a grid with matches and standings
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Matches DataGrid
        var matchesGrid = CreateMatchesDataGrid(group);
        Grid.SetColumn(matchesGrid, 0);
        grid.Children.Add(matchesGrid);

        // Standings DataGrid
        var standingsGrid = CreateStandingsDataGrid(group);
        Grid.SetColumn(standingsGrid, 2);
        grid.Children.Add(standingsGrid);

        tabItem.Content = grid;
        return tabItem;
    }

    private TabItem CreateKnockoutBracketTab(string header, TournamentClass tournamentClass, bool isLoserBracket)
    {
        var tabItem = new TabItem { Header = header };

        // Create matches data grid for knockout
        var knockoutMatches = isLoserBracket 
            ? tournamentClass.GetLoserBracketMatches()
            : tournamentClass.GetWinnerBracketMatches();

        var dataGrid = new DataGrid
        {
            ItemsSource = knockoutMatches,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HorizontalGridLinesBrush = System.Windows.Media.Brushes.LightGray,
            Margin = new Thickness(10)
        };

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Round",
            Binding = new System.Windows.Data.Binding("RoundDisplay"),
            Width = new DataGridLength(100)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Match",
            Binding = new System.Windows.Data.Binding("DisplayName"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Result",
            Binding = new System.Windows.Data.Binding("ScoreDisplay"),
            Width = new DataGridLength(100)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Status",
            Binding = new System.Windows.Data.Binding("StatusDisplay"),
            Width = new DataGridLength(100)
        });

        tabItem.Content = dataGrid;
        return tabItem;
    }

    private TabItem CreateFinalsTab(TournamentClass tournamentClass)
    {
        var tabItem = new TabItem { Header = "Finals" };

        // Similar to knockout but for finals matches
        var finalsMatches = tournamentClass.GetFinalsMatches();
        var dataGrid = new DataGrid
        {
            ItemsSource = finalsMatches,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HorizontalGridLinesBrush = System.Windows.Media.Brushes.LightGray,
            Margin = new Thickness(10)
        };

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Match",
            Binding = new System.Windows.Data.Binding("DisplayName"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Result",
            Binding = new System.Windows.Data.Binding("ScoreDisplay"),
            Width = new DataGridLength(100)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Status",
            Binding = new System.Windows.Data.Binding("StatusDisplay"),
            Width = new DataGridLength(100)
        });

        tabItem.Content = dataGrid;
        return tabItem;
    }

    private DataGrid CreateMatchesDataGrid(Group group)
    {
        var dataGrid = new DataGrid
        {
            ItemsSource = group.Matches,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HorizontalGridLinesBrush = System.Windows.Media.Brushes.LightGray,
            Margin = new Thickness(10)
        };

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Match",
            Binding = new System.Windows.Data.Binding("DisplayName"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Result",
            Binding = new System.Windows.Data.Binding("ScoreDisplay"),
            Width = new DataGridLength(100)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Status",
            Binding = new System.Windows.Data.Binding("StatusDisplay"),
            Width = new DataGridLength(100)
        });

        return dataGrid;
    }

    private DataGrid CreateStandingsDataGrid(Group group)
    {
        var standings = group.GetStandings();
        var dataGrid = new DataGrid
        {
            ItemsSource = standings,
            AutoGenerateColumns = false,
            IsReadOnly = true,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            HorizontalGridLinesBrush = System.Windows.Media.Brushes.LightGray,
            Margin = new Thickness(10)
        };

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Pos",
            Binding = new System.Windows.Data.Binding("Position"),
            Width = new DataGridLength(50)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Player",
            Binding = new System.Windows.Data.Binding("Player.Name"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Pts",
            Binding = new System.Windows.Data.Binding("Points"),
            Width = new DataGridLength(50)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "W-D-L",
            Binding = new System.Windows.Data.Binding("RecordDisplay"),
            Width = new DataGridLength(80)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Sets",
            Binding = new System.Windows.Data.Binding("SetRecordDisplay"),
            Width = new DataGridLength(70)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Legs",
            Binding = new System.Windows.Data.Binding("LegRecordDisplay"),
            Width = new DataGridLength(70)
        });

        return dataGrid;
    }

    private void StartStop_Click(object sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            StopCycling();
        }
        else
        {
            StartCycling();
        }
    }

    private void StartCycling()
    {
        _isRunning = true;
        _lastCycleTime = DateTime.Now;
        StartStopButton.Content = "⏸ " + _localizationService.GetString("StopCycling");
        StartStopButton.Style = (Style)FindResource("DangerButton");

        // Set initial sub-tab
        SetCurrentSubTab();

        // Start with sub-tab cycling using the shorter interval
        _cycleTimer.Interval = TimeSpan.FromSeconds(_subTabInterval);
        _cycleTimer.Start();
        
        UpdateStatus();
    }

    private void StopCycling()
    {
        _isRunning = false;
        _cycleTimer.Stop();
        StartStopButton.Content = "▶ " + _localizationService.GetString("StartCycling");
        StartStopButton.Style = (Style)FindResource("SuccessButton");
        
        UpdateStatus();
    }

    private void CycleTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isRunning) return;

        _lastCycleTime = DateTime.Now;

        // Get current class and sub-tab info
        var currentClassTab = MainTabControl.SelectedItem as TabItem;
        if (currentClassTab?.Content is TabControl subTabControl)
        {
            int maxSubTabs = subTabControl.Items.Count;
            
            if (maxSubTabs > 1)
            {
                // Multiple sub-tabs: Cycle through them
                _currentSubTabIndex++;
                
                if (_currentSubTabIndex >= maxSubTabs)
                {
                    // Reached end of sub-tabs in current class
                    _currentSubTabIndex = 0; // Reset to first sub-tab
                    
                    if (_activeTournamentTabs.Count > 1)
                    {
                        // Multiple classes: Move to next class
                        _currentClassIndex++;
                        
                        if (_currentClassIndex >= _activeTournamentTabs.Count)
                        {
                            _currentClassIndex = 0; // Loop back to first class
                        }
                        
                        MainTabControl.SelectedIndex = _currentClassIndex;
                        SetCurrentSubTab(); // Set the sub-tab in the new class
                        
                        // WICHTIG: Reset timer to class interval when switching classes
                        _cycleTimer.Stop();
                        _cycleTimer.Interval = TimeSpan.FromSeconds(_classInterval);
                        _cycleTimer.Start();
                    }
                    else
                    {
                        // Only one class: Keep cycling sub-tabs endlessly
                        SetCurrentSubTab(); // Set sub-tab to first (index 0)
                        
                        // Keep sub-tab interval for continuous cycling
                        _cycleTimer.Stop();
                        _cycleTimer.Interval = TimeSpan.FromSeconds(_subTabInterval);
                        _cycleTimer.Start();
                    }
                }
                else
                {
                    // Still within sub-tabs: Just switch to next sub-tab
                    SetCurrentSubTab();
                }
            }
            else
            {
                // Only one or no sub-tabs: Move to next class
                if (_activeTournamentTabs.Count > 1)
                {
                    _currentClassIndex++;
                    _currentSubTabIndex = 0; // Reset sub-tab index
                    
                    if (_currentClassIndex >= _activeTournamentTabs.Count)
                    {
                        _currentClassIndex = 0; // Loop back to first class
                    }
                    
                    MainTabControl.SelectedIndex = _currentClassIndex;
                    SetCurrentSubTab(); // Set the sub-tab in the new class
                }
                else
                {
                    // Only one class with one sub-tab: Stay put but keep timer running for consistency
                    // This allows manual navigation while keeping the timer active
                }
            }
        }
        else
        {
            // Fallback: No sub-tab control found, just cycle classes
            if (_activeTournamentTabs.Count > 1)
            {
                _currentClassIndex++;
                _currentSubTabIndex = 0;
                
                if (_currentClassIndex >= _activeTournamentTabs.Count)
                {
                    _currentClassIndex = 0;
                }
                
                MainTabControl.SelectedIndex = _currentClassIndex;
                SetCurrentSubTab();
            }
        }

        UpdateStatus();
    }

    private void Configure_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OverviewConfigDialog
        {
            Owner = this,
            ClassInterval = _classInterval,
            SubTabInterval = _subTabInterval,
            ShowOnlyActiveClasses = _showOnlyActiveClasses
        };

        if (dialog.ShowDialog() == true)
        {
            _classInterval = dialog.ClassInterval;
            _subTabInterval = dialog.SubTabInterval;
            _showOnlyActiveClasses = dialog.ShowOnlyActiveClasses;
            
            // Reinitialize if the "show only active" setting changed
            InitializeOverview();
            
            // If currently running, update the timer interval
            if (_isRunning)
            {
                _cycleTimer.Stop();
                _cycleTimer.Interval = TimeSpan.FromSeconds(_subTabInterval);
                _cycleTimer.Start();
            }
        }
    }

    private void SetCurrentSubTab()
    {
        var currentClassTab = MainTabControl.SelectedItem as TabItem;
        if (currentClassTab?.Content is TabControl subTabControl && subTabControl.Items.Count > 0)
        {
            // Ensure sub-tab index is within bounds
            if (_currentSubTabIndex >= subTabControl.Items.Count)
            {
                _currentSubTabIndex = 0;
            }
            
            subTabControl.SelectedIndex = _currentSubTabIndex;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        StopCycling();
    }

    private void UpdateStatus()
    {
        if (_isRunning)
        {
            StatusTextBlock.Text = _localizationService.GetString("AutoCyclingActive");
            
            var currentTab = MainTabControl.SelectedItem as TabItem;
            if (currentTab != null)
            {
                var className = "";
                if (currentTab.Header is StackPanel sp && sp.Children[1] is TextBlock tb)
                {
                    className = tb.Text;
                }

                var subTabInfo = "";
                if (currentTab.Content is TabControl subTabControl && subTabControl.Items.Count > 0)
                {
                    var subTab = subTabControl.SelectedItem as TabItem;
                    if (subTab != null)
                    {
                        subTabInfo = $" → {subTab.Header}";
                        // Show current sub-tab index info
                        if (subTabControl.Items.Count > 1)
                        {
                            subTabInfo += $" ({subTabControl.SelectedIndex + 1}/{subTabControl.Items.Count})";
                        }
                    }
                }

                CurrentDisplayTextBlock.Text = $"{_localizationService.GetString("Showing")}: {className}{subTabInfo}";
                
                // Zeige aktuellen Fortschritt und totale Anzahl für endlose Schleife
                if (_activeTournamentTabs.Count > 1)
                {
                    CycleInfoTextBlock.Text = $"Class: {_currentClassIndex + 1}/{_activeTournamentTabs.Count} (∞ loop)";
                }
                else
                {
                    CycleInfoTextBlock.Text = $"Single class (∞ sub-tabs)";
                }
            }
        }
        else
        {
            StatusTextBlock.Text = _localizationService.GetString("ManualMode");
            CurrentDisplayTextBlock.Text = _localizationService.GetString("CyclingStopped");
            CycleInfoTextBlock.Text = _localizationService.GetString("ManualControl");
        }

        // Update timer display with countdown
        if (_cycleTimer.IsEnabled && _isRunning)
        {
            // Calculate time remaining in current interval
            var elapsed = DateTime.Now - _lastCycleTime;
            var remaining = _cycleTimer.Interval - elapsed;
            
            if (remaining.TotalSeconds > 0)
            {
                TimerTextBlock.Text = remaining.ToString(@"mm\:ss");
            }
            else
            {
                TimerTextBlock.Text = "00:00";
            }
        }
        else
        {
            TimerTextBlock.Text = "--:--";
        }
    }

    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("TournamentOverview");
        
        // Update control text
        var startStopText = StartStopButton.Content?.ToString() ?? "";
        if (startStopText.Contains("Start"))
            StartStopButton.Content = "▶ " + _localizationService.GetString("StartCycling");
        else
            StartStopButton.Content = "⏸ " + _localizationService.GetString("StopCycling");
            
        ConfigureButton.Content = "⚙ " + _localizationService.GetString("Configure");
        CloseButton.Content = "✕ " + _localizationService.GetString("Close");
        
        // Update status
        if (!_isRunning)
        {
            StatusTextBlock.Text = _localizationService.GetString("ManualMode");
            CurrentDisplayTextBlock.Text = _localizationService.GetString("CyclingStopped");
            CycleInfoTextBlock.Text = _localizationService.GetString("ManualControl");
        }
    }
}

// Simple configuration dialog
public partial class OverviewConfigDialog : Window
{
    public int ClassInterval { get; set; }
    public int SubTabInterval { get; set; }
    public bool ShowOnlyActiveClasses { get; set; }

    public OverviewConfigDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = "Overview Configuration";
        Width = 450;
        Height = 300;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var margin = new Thickness(15);

        // Title
        var titleBlock = new TextBlock { 
            Text = "Tournament Overview Configuration", 
            FontSize = 16, 
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(15, 15, 15, 20) 
        };
        Grid.SetRow(titleBlock, 0);
        Grid.SetColumnSpan(titleBlock, 2);
        grid.Children.Add(titleBlock);

        // Class Interval
        var classLabel = new TextBlock { 
            Text = "Time between tournament classes:", 
            Margin = margin, 
            VerticalAlignment = VerticalAlignment.Center 
        };
        Grid.SetRow(classLabel, 1);
        Grid.SetColumn(classLabel, 0);
        grid.Children.Add(classLabel);

        var classPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = margin };
        var classTextBox = new TextBox { Name = "ClassIntervalTextBox", Width = 60, TextAlignment = TextAlignment.Center };
        var classSecondsLabel = new TextBlock { Text = " seconds", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
        classPanel.Children.Add(classTextBox);
        classPanel.Children.Add(classSecondsLabel);
        Grid.SetRow(classPanel, 1);
        Grid.SetColumn(classPanel, 1);
        grid.Children.Add(classPanel);

        // Sub-Tab Interval
        var subLabel = new TextBlock { 
            Text = "Time between sub-tabs:", 
            Margin = margin, 
            VerticalAlignment = VerticalAlignment.Center 
        };
        Grid.SetRow(subLabel, 2);
        Grid.SetColumn(subLabel, 0);
        grid.Children.Add(subLabel);

        var subPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = margin };
        var subTextBox = new TextBox { Name = "SubTabIntervalTextBox", Width = 60, TextAlignment = TextAlignment.Center };
        var subSecondsLabel = new TextBlock { Text = " seconds", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
        subPanel.Children.Add(subTextBox);
        subPanel.Children.Add(subSecondsLabel);
        Grid.SetRow(subPanel, 2);
        Grid.SetColumn(subPanel, 1);
        grid.Children.Add(subPanel);

        // Show Only Active Classes
        var activeCheckBox = new CheckBox { 
            Name = "ShowOnlyActiveCheckBox", 
            Content = "Show only classes with active groups", 
            Margin = margin 
        };
        Grid.SetRow(activeCheckBox, 3);
        Grid.SetColumnSpan(activeCheckBox, 2);
        grid.Children.Add(activeCheckBox);

        // Info text
        var infoText = new TextBlock { 
            Text = "The overview will automatically cycle through tournament classes and their groups/brackets endlessly. You can move this window to a second monitor for display purposes.",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(15, 10, 15, 15),
            Foreground = System.Windows.Media.Brushes.Gray,
            FontStyle = FontStyles.Italic
        };
        Grid.SetRow(infoText, 4);
        Grid.SetColumnSpan(infoText, 2);
        grid.Children.Add(infoText);

        // Buttons
        var buttonPanel = new StackPanel { 
            Orientation = Orientation.Horizontal, 
            HorizontalAlignment = HorizontalAlignment.Right, 
            Margin = margin 
        };
        
        var okButton = new Button { 
            Content = "OK", 
            Padding = new Thickness(20, 5, 20, 5), 
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true
        };
        okButton.Click += (s, e) => { SaveAndClose(); };
        
        var cancelButton = new Button { 
            Content = "Cancel", 
            Padding = new Thickness(20, 5, 20, 5),
            IsCancel = true
        };
        cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
        
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        
        Grid.SetRow(buttonPanel, 5);
        Grid.SetColumnSpan(buttonPanel, 2);
        grid.Children.Add(buttonPanel);

        Content = grid;
        
        Loaded += (s, e) =>
        {
            var classTextBoxControl = (TextBox)grid.Children.OfType<StackPanel>().First(sp => sp.Children.OfType<TextBox>().Any(tb => tb.Name == "ClassIntervalTextBox")).Children.OfType<TextBox>().First();
            var subTextBoxControl = (TextBox)grid.Children.OfType<StackPanel>().First(sp => sp.Children.OfType<TextBox>().Any(tb => tb.Name == "SubTabIntervalTextBox")).Children.OfType<TextBox>().First();
            var checkBoxControl = (CheckBox)grid.Children.OfType<CheckBox>().First(cb => cb.Name == "ShowOnlyActiveCheckBox");
            
            classTextBoxControl.Text = ClassInterval.ToString();
            subTextBoxControl.Text = SubTabInterval.ToString();
            checkBoxControl.IsChecked = ShowOnlyActiveClasses;
            
            classTextBoxControl.Focus();
            classTextBoxControl.SelectAll();
        };
    }

    private void SaveAndClose()
    {
        var grid = (Grid)Content;
        var classTextBox = (TextBox)grid.Children.OfType<StackPanel>().First(sp => sp.Children.OfType<TextBox>().Any(tb => tb.Name == "ClassIntervalTextBox")).Children.OfType<TextBox>().First();
        var subTextBox = (TextBox)grid.Children.OfType<StackPanel>().First(sp => sp.Children.OfType<TextBox>().Any(tb => tb.Name == "SubTabIntervalTextBox")).Children.OfType<TextBox>().First();
        var checkBox = (CheckBox)grid.Children.OfType<CheckBox>().First(cb => cb.Name == "ShowOnlyActiveCheckBox");

        if (!int.TryParse(classTextBox.Text, out int classInterval) || classInterval < 1)
        {
            MessageBox.Show("Invalid class interval. Please enter a number >= 1.", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            classTextBox.Focus();
            classTextBox.SelectAll();
            return;
        }

        if (!int.TryParse(subTextBox.Text, out int subTabInterval) || subTabInterval < 1)
        {
            MessageBox.Show("Invalid sub-tab interval. Please enter a number >= 1.", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            subTextBox.Focus();
            subTextBox.SelectAll();
            return;
        }

        ClassInterval = classInterval;
        SubTabInterval = subTabInterval;
        ShowOnlyActiveClasses = checkBox.IsChecked == true;

        DialogResult = true;
        Close();
    }
}