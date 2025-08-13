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
            // Create tabs for knockout brackets - both matches and tree views
            var winnerBracketMatchesTab = CreateKnockoutBracketTab(_localizationService.GetString("WinnerBracketMatches"), tournamentClass, false, false);
            tabControl.Items.Add(winnerBracketMatchesTab);

            var winnerBracketTreeTab = CreateKnockoutBracketTab(_localizationService.GetString("WinnerBracketTree"), tournamentClass, false, true);
            tabControl.Items.Add(winnerBracketTreeTab);

            // Add Loser Bracket if double elimination
            if (tournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                var loserBracketMatchesTab = CreateKnockoutBracketTab(_localizationService.GetString("LoserBracketMatches"), tournamentClass, true, false);
                tabControl.Items.Add(loserBracketMatchesTab);

                var loserBracketTreeTab = CreateKnockoutBracketTab(_localizationService.GetString("LoserBracketTree"), tournamentClass, true, true);
                tabControl.Items.Add(loserBracketTreeTab);
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

    private TabItem CreateKnockoutBracketTab(string header, TournamentClass tournamentClass, bool isLoserBracket, bool isTreeView = false)
    {
        var tabItem = new TabItem { Header = header };

        if (isTreeView)
        {
            // Create tournament tree view for better visual representation
            var treeContent = CreateTournamentTreeView(tournamentClass, isLoserBracket);
            tabItem.Content = treeContent;
        }
        else
        {
            // Create matches data grid for knockout (existing functionality)
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
                Header = _localizationService.GetString("RoundColumn"),
                Binding = new System.Windows.Data.Binding("RoundDisplay"),
                Width = new DataGridLength(100)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = _localizationService.GetString("Match"),
                Binding = new System.Windows.Data.Binding("DisplayName"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = _localizationService.GetString("Result"),
                Binding = new System.Windows.Data.Binding("ScoreDisplay"),
                Width = new DataGridLength(100)
            });

            dataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = _localizationService.GetString("Status"),
                Binding = new System.Windows.Data.Binding("StatusDisplay"),
                Width = new DataGridLength(100)
            });

            tabItem.Content = dataGrid;
        }

        return tabItem;
    }

    private TabItem CreateFinalsTab(TournamentClass tournamentClass)
    {
        var tabItem = new TabItem { Header = _localizationService.GetString("FinalsTab") };

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
            Header = _localizationService.GetString("Match"),
            Binding = new System.Windows.Data.Binding("DisplayName"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Result"),
            Binding = new System.Windows.Data.Binding("ScoreDisplay"),
            Width = new DataGridLength(100)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Status"),
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
            Header = _localizationService.GetString("Match"),
            Binding = new System.Windows.Data.Binding("DisplayName"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Result"),
            Binding = new System.Windows.Data.Binding("ScoreDisplay"),
            Width = new DataGridLength(100)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Status"),
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
            Header = _localizationService.GetString("PositionShort"),
            Binding = new System.Windows.Data.Binding("Position"),
            Width = new DataGridLength(50)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Player"),
            Binding = new System.Windows.Data.Binding("Player.Name"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("PointsShort"),
            Binding = new System.Windows.Data.Binding("Points"),
            Width = new DataGridLength(50)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("WinDrawLoss"),
            Binding = new System.Windows.Data.Binding("RecordDisplay"),
            Width = new DataGridLength(80)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Sets"),
            Binding = new System.Windows.Data.Binding("SetRecordDisplay"),
            Width = new DataGridLength(70)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Legs"),
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
        
        // Update main header text
        OverviewModeText.Text = _localizationService.GetString("TournamentOverview");
        
        // Update control text
        if (_isRunning)
        {
            StartStopButton.Content = "⏸ " + _localizationService.GetString("StopCycling");
        }
        else
        {
            StartStopButton.Content = "▶ " + _localizationService.GetString("StartCycling");
        }
            
        ConfigureButton.Content = _localizationService.GetString("Configure");
        CloseButton.Content = _localizationService.GetString("Close");
        
        // Update status if not running
        if (!_isRunning)
        {
            StatusTextBlock.Text = _localizationService.GetString("ManualMode");
            CurrentDisplayTextBlock.Text = _localizationService.GetString("CyclingStopped");
            CycleInfoTextBlock.Text = _localizationService.GetString("ManualControl");
        }
    }

    private FrameworkElement CreateTournamentTreeView(TournamentClass tournamentClass, bool isLoserBracket)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(10)
        };

        var canvas = new Canvas
        {
            Background = new System.Windows.Media.LinearGradientBrush(
                System.Windows.Media.Colors.WhiteSmoke,
                System.Windows.Media.Colors.LightGray, 90),
            MinWidth = 1000,
            MinHeight = 700
        };

        var matches = isLoserBracket 
            ? tournamentClass.GetLoserBracketMatches().ToList()
            : tournamentClass.GetWinnerBracketMatches().ToList();

        if (matches.Count == 0)
        {
            var noMatchesPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var icon = new TextBlock
            {
                Text = "🏆",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var noMatchesText = new TextBlock
            {
                Text = isLoserBracket 
                    ? _localizationService.GetString("NoLoserBracketMatches") ?? "Keine Loser Bracket Spiele vorhanden" 
                    : _localizationService.GetString("NoWinnerBracketMatches") ?? "Keine Winner Bracket Spiele vorhanden",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.DarkGray
            };

            var subText = new TextBlock
            {
                Text = _localizationService.GetString("TournamentTreeWillShow") ?? "Der Turnierbaum wird angezeigt sobald die KO-Phase beginnt",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 10, 0, 0)
            };

            noMatchesPanel.Children.Add(icon);
            noMatchesPanel.Children.Add(noMatchesText);
            noMatchesPanel.Children.Add(subText);
            
            Canvas.SetLeft(noMatchesPanel, 300);
            Canvas.SetTop(noMatchesPanel, 250);
            canvas.Children.Add(noMatchesPanel);
            
            scrollViewer.Content = canvas;
            return scrollViewer;
        }

        // Add title
        var titleText = new TextBlock
        {
            Text = isLoserBracket ? "🥈 Loser Bracket" : "🏆 Winner Bracket",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = isLoserBracket 
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 92, 92))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 139, 34)),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        Canvas.SetLeft(titleText, 20);
        Canvas.SetTop(titleText, 10);
        canvas.Children.Add(titleText);

        // Group matches by round for layout
        var matchesByRound = matches
            .GroupBy(m => m.Round)
            .OrderBy(g => GetRoundOrderValue(g.Key, isLoserBracket))
            .ToList();

        double roundWidth = 220;
        double matchHeight = 70;
        double matchSpacing = 25;
        double roundSpacing = 50;

        for (int roundIndex = 0; roundIndex < matchesByRound.Count; roundIndex++)
        {
            var roundGroup = matchesByRound[roundIndex];
            var roundMatches = roundGroup.OrderBy(m => m.Position).ToList();

            double xPos = roundIndex * (roundWidth + roundSpacing) + 30;
            double startY = 80;

            // Calculate vertical spacing to center matches in their round
            double totalRoundHeight = roundMatches.Count * matchHeight + (roundMatches.Count - 1) * matchSpacing;
            double roundStartY = Math.Max(startY, (canvas.MinHeight - totalRoundHeight) / 2);

            for (int matchIndex = 0; matchIndex < roundMatches.Count; matchIndex++)
            {
                var match = roundMatches[matchIndex];
                double yPos = roundStartY + matchIndex * (matchHeight + matchSpacing);

                var matchControl = CreateMatchControl(match, roundWidth - 20, matchHeight - 10);
                Canvas.SetLeft(matchControl, xPos);
                Canvas.SetTop(matchControl, yPos);
                canvas.Children.Add(matchControl);

                // Draw connection lines to next round
                if (roundIndex < matchesByRound.Count - 1)
                {
                    DrawConnectionLine(canvas, xPos + roundWidth - 20, yPos + matchHeight / 2, 
                                     xPos + roundWidth + roundSpacing - 20, yPos + matchHeight / 2);
                }
            }

            // Add round label with background
            var roundLabelBorder = new Border
            {
                Background = isLoserBracket 
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 182, 193))
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 144, 238, 144)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(15, 5, 15, 5),
                BorderBrush = isLoserBracket 
                    ? System.Windows.Media.Brushes.IndianRed
                    : System.Windows.Media.Brushes.ForestGreen,
                BorderThickness = new Thickness(2)
            };

            var roundLabel = new TextBlock
            {
                Text = roundMatches.First().RoundDisplay,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.DarkSlateGray,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            roundLabelBorder.Child = roundLabel;
            Canvas.SetLeft(roundLabelBorder, xPos + (roundWidth - 140) / 2);
            Canvas.SetTop(roundLabelBorder, 50);
            canvas.Children.Add(roundLabelBorder);
        }

        // Adjust canvas size based on content
        canvas.Width = Math.Max(1000, matchesByRound.Count * (roundWidth + roundSpacing) + 150);
        canvas.Height = Math.Max(700, matchesByRound.Max(r => r.Count()) * (matchHeight + matchSpacing) + 300);

        scrollViewer.Content = canvas;
        return scrollViewer;
    }

    private int GetRoundOrderValue(KnockoutRound round, bool isLoserBracket)
    {
        if (isLoserBracket)
        {
            return round switch
            {
                KnockoutRound.LoserRound1 => 1,
                KnockoutRound.LoserRound2 => 2,
                KnockoutRound.LoserRound3 => 3,
                KnockoutRound.LoserRound4 => 4,
                KnockoutRound.LoserRound5 => 5,
                KnockoutRound.LoserRound6 => 6,
                KnockoutRound.LoserRound7 => 7,
                KnockoutRound.LoserRound8 => 8,
                KnockoutRound.LoserRound9 => 9,
                KnockoutRound.LoserRound10 => 10,
                KnockoutRound.LoserRound11 => 11,
                KnockoutRound.LoserRound12 => 12,
                KnockoutRound.LoserFinal => 13,
                _ => 99
            };
        }
        else
        {
            return round switch
            {
                KnockoutRound.Best64 => 1,
                KnockoutRound.Best32 => 2,
                KnockoutRound.Best16 => 3,
                KnockoutRound.Quarterfinal => 4,
                KnockoutRound.Semifinal => 5,
                KnockoutRound.Final => 6,
                KnockoutRound.GrandFinal => 7,
                _ => 99
            };
        }
    }

    private Border CreateMatchControl(KnockoutMatch match, double width, double height)
    {
        var border = new Border
        {
            Width = width,
            Height = height,
            BorderBrush = System.Windows.Media.Brushes.DarkSlateGray,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(3),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Gray,
                Direction = 315,
                ShadowDepth = 3,
                Opacity = 0.5
            }
        };

        // Set background color and border based on match status
        switch (match.Status)
        {
            case MatchStatus.NotStarted:
                border.Background = System.Windows.Media.Brushes.WhiteSmoke;
                border.BorderBrush = System.Windows.Media.Brushes.Silver;
                break;
            case MatchStatus.InProgress:
                border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 200));
                border.BorderBrush = System.Windows.Media.Brushes.Orange;
                break;
            case MatchStatus.Finished:
                border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 255, 200));
                border.BorderBrush = System.Windows.Media.Brushes.Green;
                break;
            case MatchStatus.Bye:
                border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 220, 255));
                border.BorderBrush = System.Windows.Media.Brushes.RoyalBlue;
                break;
            default:
                border.Background = System.Windows.Media.Brushes.White;
                break;
        }

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Match ID/Position indicator
        var matchIdText = new TextBlock
        {
            Text = $"#{match.Id}",
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 2)
        };

        // Player names with winner highlighting
        var player1Text = new TextBlock
        {
            Text = match.Player1?.Name ?? "TBD",
            FontSize = 11,
            FontWeight = match.Winner?.Id == match.Player1?.Id ? FontWeights.Bold : FontWeights.Normal,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player1?.Id 
                ? System.Windows.Media.Brushes.DarkGreen 
                : System.Windows.Media.Brushes.Black
        };

        var vsText = new TextBlock
        {
            Text = "vs",
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 2),
            FontStyle = FontStyles.Italic,
            Foreground = System.Windows.Media.Brushes.Gray
        };

        var player2Text = new TextBlock
        {
            Text = match.Player2?.Name ?? "TBD",
            FontSize = 11,
            FontWeight = match.Winner?.Id == match.Player2?.Id ? FontWeights.Bold : FontWeights.Normal,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player2?.Id 
                ? System.Windows.Media.Brushes.DarkGreen 
                : System.Windows.Media.Brushes.Black
        };

        // Score display with better styling
        var scoreText = new TextBlock
        {
            Text = match.Status == MatchStatus.NotStarted ? "--" : match.ScoreDisplay,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.DarkBlue,
            Margin = new Thickness(0, 3, 0, 0)
        };

        stackPanel.Children.Add(matchIdText);
        stackPanel.Children.Add(player1Text);
        stackPanel.Children.Add(vsText);
        stackPanel.Children.Add(player2Text);
        stackPanel.Children.Add(scoreText);

        border.Child = stackPanel;

        // Enhanced tooltip with more match details
        var tooltipText = $"Match {match.Id} - {match.RoundDisplay}\n" +
                         $"Status: {match.StatusDisplay}\n" +
                         $"Spieler 1: {match.Player1?.Name ?? "TBD"}\n" +
                         $"Spieler 2: {match.Player2?.Name ?? "TBD"}";
        
        if (match.Status == MatchStatus.Finished && match.Winner != null)
        {
            tooltipText += $"\n🏆 Sieger: {match.Winner.Name}";
        }

        border.ToolTip = tooltipText;

        return border;
    }

    private void DrawConnectionLine(Canvas canvas, double x1, double y1, double x2, double y2)
    {
        var line = new System.Windows.Shapes.Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(70, 130, 180)),
            StrokeThickness = 2,
            Opacity = 0.7
        };

        // Create subtle dashed line effect
        line.StrokeDashArray = new System.Windows.Media.DoubleCollection(new double[] { 5, 3 });

        // Add subtle glow effect
        line.Effect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = System.Windows.Media.Colors.SteelBlue,
            Direction = 0,
            ShadowDepth = 0,
            BlurRadius = 2,
            Opacity = 0.3
        };

        canvas.Children.Add(line);
    }
}

// Simple configuration dialog
public partial class OverviewConfigDialog : Window
{
    private readonly LocalizationService _localizationService;
    
    public int ClassInterval { get; set; }
    public int SubTabInterval { get; set; }
    public bool ShowOnlyActiveClasses { get; set; }

    public OverviewConfigDialog()
    {
        _localizationService = App.LocalizationService ?? throw new InvalidOperationException("LocalizationService not available");
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Title = _localizationService.GetString("OverviewConfiguration");
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
            Text = _localizationService.GetString("TournamentOverviewConfiguration"), 
            FontSize = 16, 
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(15, 15, 15, 20) 
        };
        Grid.SetRow(titleBlock, 0);
        Grid.SetColumnSpan(titleBlock, 2);
        grid.Children.Add(titleBlock);

        // Class Interval
        var classLabel = new TextBlock { 
            Text = _localizationService.GetString("TimeBetweenClasses"), 
            Margin = margin, 
            VerticalAlignment = VerticalAlignment.Center 
        };
        Grid.SetRow(classLabel, 1);
        Grid.SetColumn(classLabel, 0);
        grid.Children.Add(classLabel);

        var classPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = margin };
        var classTextBox = new TextBox { Name = "ClassIntervalTextBox", Width = 60, TextAlignment = TextAlignment.Center };
        var classSecondsLabel = new TextBlock { Text = " " + _localizationService.GetString("Seconds"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
        classPanel.Children.Add(classTextBox);
        classPanel.Children.Add(classSecondsLabel);
        Grid.SetRow(classPanel, 1);
        Grid.SetColumn(classPanel, 1);
        grid.Children.Add(classPanel);

        // Sub-Tab Interval
        var subLabel = new TextBlock { 
            Text = _localizationService.GetString("TimeBetweenSubTabs"), 
            Margin = margin, 
            VerticalAlignment = VerticalAlignment.Center 
        };
        Grid.SetRow(subLabel, 2);
        Grid.SetColumn(subLabel, 0);
        grid.Children.Add(subLabel);

        var subPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = margin };
        var subTextBox = new TextBox { Name = "SubTabIntervalTextBox", Width = 60, TextAlignment = TextAlignment.Center };
        var subSecondsLabel = new TextBlock { Text = " " + _localizationService.GetString("Seconds"), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
        subPanel.Children.Add(subTextBox);
        subPanel.Children.Add(subSecondsLabel);
        Grid.SetRow(subPanel, 2);
        Grid.SetColumn(subPanel, 1);
        grid.Children.Add(subPanel);

        // Show Only Active Classes
        var activeCheckBox = new CheckBox { 
            Name = "ShowOnlyActiveCheckBox", 
            Content = _localizationService.GetString("ShowOnlyActiveClassesText"), 
            Margin = margin 
        };
        Grid.SetRow(activeCheckBox, 3);
        Grid.SetColumnSpan(activeCheckBox, 2);
        grid.Children.Add(activeCheckBox);

        // Info text
        var infoText = new TextBlock { 
            Text = _localizationService.GetString("OverviewInfoText"),
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
            Content = _localizationService.GetString("OK"), 
            Padding = new Thickness(20, 5, 20, 5), 
            Margin = new Thickness(0, 0, 10, 0),
            IsDefault = true
        };
        okButton.Click += (s, e) => { SaveAndClose(); };
        
        var cancelButton = new Button { 
            Content = _localizationService.GetString("Cancel"), 
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
            MessageBox.Show(_localizationService.GetString("InvalidClassInterval"), _localizationService.GetString("Error"), 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            classTextBox.Focus();
            classTextBox.SelectAll();
            return;
        }

        if (!int.TryParse(subTextBox.Text, out int subTabInterval) || subTabInterval < 1)
        {
            MessageBox.Show(_localizationService.GetString("InvalidSubTabInterval"), _localizationService.GetString("Error"), 
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