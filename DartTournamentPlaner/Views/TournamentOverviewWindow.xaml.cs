using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using System.Drawing.Imaging;
using QRCoder;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using WinColor = System.Windows.Media.Color;
using WinBrushes = System.Windows.Media.Brushes;
using DrawingColor = System.Drawing.Color;

namespace DartTournamentPlaner.Views;

public partial class TournamentOverviewWindow : Window
{
    private readonly List<TournamentClass> _tournamentClasses;
    private readonly LocalizationService _localizationService;
    private readonly DispatcherTimer _cycleTimer;
    private readonly HubIntegrationService? _hubService; // ✅ NEU: Hub Service für QR-Codes
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
        LocalizationService localizationService,
        HubIntegrationService? hubService = null) // ✅ NEU: Optional Hub Service Parameter
    {
        InitializeComponent();
        
        _tournamentClasses = tournamentClasses;
        _localizationService = localizationService;
        _hubService = hubService;
        
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
        
        // Create header with color indicator and modern design like MainWindow
        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
        
        // Modern border design like MainWindow
        var colorBorder = new Border
        {
            Width = 16,
            Height = 16,
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(0, 0, 10, 0),
            Background = WinBrushes.White,
            BorderBrush = new SolidColorBrush(WinColor.FromRgb(229, 231, 235)),
            BorderThickness = new Thickness(2)
        };

        var colorEllipse = new System.Windows.Shapes.Ellipse
        {
            Margin = new Thickness(2)
        };

        // Set colors and effects exactly like MainWindow
        switch (classIndex)
        {
            case 0: // Platinum - SeaShell
                colorEllipse.Fill = new SolidColorBrush(Colors.SeaShell);
                colorBorder.Effect = new DropShadowEffect
                {
                    Color = Colors.SeaShell,
                    Direction = 0,
                    ShadowDepth = 0,
                    BlurRadius = 6,
                    Opacity = 0.8
                };
                break;
            case 1: // Gold - #FFC107
                colorEllipse.Fill = new SolidColorBrush(WinColor.FromRgb(255, 193, 7));
                colorBorder.Effect = new DropShadowEffect
                {
                    Color = WinColor.FromRgb(255, 152, 0),
                    Direction = 0,
                    ShadowDepth = 0,
                    BlurRadius = 6,
                    Opacity = 0.6
                };
                break;
            case 2: // Silver - Silver
                colorEllipse.Fill = new SolidColorBrush(Colors.Silver);
                colorBorder.Effect = new DropShadowEffect
                {
                    Color = Colors.Silver,
                    Direction = 0,
                    ShadowDepth = 0,
                    BlurRadius = 6,
                    Opacity = 0.6
                };
                break;
            case 3: // Bronze - #A1887F
                colorEllipse.Fill = new SolidColorBrush(WinColor.FromRgb(161, 136, 127));
                colorBorder.Effect = new DropShadowEffect
                {
                    Color = WinColor.FromRgb(141, 110, 99),
                    Direction = 0,
                    ShadowDepth = 0,
                    BlurRadius = 6,
                    Opacity = 0.6
                };
                break;
        }

        colorBorder.Child = colorEllipse;

        var headerText = new TextBlock
        {
            Text = tournamentClass.Name,
            FontWeight = FontWeights.SemiBold,
            FontSize = 14
        };

        headerPanel.Children.Add(colorBorder);
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

            // ✅ NEU: QR-Code Spalte für Knockout Matches
            if (_hubService != null && _hubService.IsRegisteredWithHub)
            {
                var qrColumn = new DataGridTemplateColumn
                {
                    Header = "📱",
                    Width = new DataGridLength(120), // 60 -> 120 für größere QR-Codes
                    CellTemplate = CreateQRCodeCellTemplate()
                };
                dataGrid.Columns.Add(qrColumn);
            }

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

        // ✅ NEU: QR-Code Spalte für Finals Matches
        if (_hubService != null && _hubService.IsRegisteredWithHub)
        {
            var qrColumn = new DataGridTemplateColumn
            {
                Header = "📱",
                Width = new DataGridLength(120), // 60 -> 120 für größere QR-Codes
                CellTemplate = CreateQRCodeCellTemplate()
            };
            dataGrid.Columns.Add(qrColumn);
        }

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

        // ✅ NEU: QR-Code Spalte hinzufügen wenn Hub Service verfügbar
        if (_hubService != null && _hubService.IsRegisteredWithHub)
        {
            var qrColumn = new DataGridTemplateColumn
            {
                Header = "📱",
                Width = new DataGridLength(120), // 60 -> 120 für größere QR-Codes
                CellTemplate = CreateQRCodeCellTemplate()
            };
            dataGrid.Columns.Add(qrColumn);
        }

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

    /// <summary>
    /// ✅ NEU: Erstellt DataTemplate für QR-Code Zellen in DataGrid
    /// </summary>
    private DataTemplate CreateQRCodeCellTemplate()
    {
        var template = new DataTemplate();
        
        var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        stackPanelFactory.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        
        // ✅ VERGRÖSSERT: QR-Code Image - doppelt so groß
        var imageFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Image));
        imageFactory.SetValue(System.Windows.Controls.Image.WidthProperty, 64.0); // 32 -> 64
        imageFactory.SetValue(System.Windows.Controls.Image.HeightProperty, 64.0); // 32 -> 64
        imageFactory.SetValue(System.Windows.Controls.Image.StretchProperty, Stretch.Uniform);
        imageFactory.SetValue(System.Windows.Controls.Image.MarginProperty, new Thickness(4)); // 2 -> 4
        imageFactory.SetValue(System.Windows.Controls.Image.ToolTipProperty, "QR-Code zum Match scannen");
        
        // QR-Code generieren und als Source setzen
        var binding = new System.Windows.Data.Binding(".");
        var converter = new MatchToQRCodeConverter(_hubService, _localizationService);
        binding.Converter = converter;
        imageFactory.SetBinding(System.Windows.Controls.Image.SourceProperty, binding);
        
        // ✅ KONDITIONELL: Button nur wenn Hub registriert ist
        if (_hubService != null && _hubService.IsRegisteredWithHub)
        {
            // ✅ VERGRÖSSERT: Button für direktes Öffnen der Match-Page
            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(Button.ContentProperty, "🌐");
            buttonFactory.SetValue(Button.WidthProperty, 40.0); // 24 -> 40
            buttonFactory.SetValue(Button.HeightProperty, 40.0); // 24 -> 40
            buttonFactory.SetValue(Button.FontSizeProperty, 16.0); // 10 -> 16
            buttonFactory.SetValue(Button.MarginProperty, new Thickness(8, 0, 0, 0)); // 2 -> 8
            buttonFactory.SetValue(Button.ToolTipProperty, "Match-Page im Browser öffnen");
            buttonFactory.SetValue(Button.CursorProperty, System.Windows.Input.Cursors.Hand);
            
            // Event für Button-Click
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnOpenMatchPageClick));
            
            //stackPanelFactory.AppendChild(buttonFactory);
        }
        
        stackPanelFactory.AppendChild(imageFactory);
        
        template.VisualTree = stackPanelFactory;
        return template;
    }

    /// <summary>
    /// ✅ NEU: Event Handler für "Match-Page öffnen" Button
    /// </summary>
    private void OnOpenMatchPageClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.DataContext is Match match)
            {
                var tournamentId = _hubService?.GetCurrentTournamentId();
                if (!string.IsNullOrEmpty(tournamentId) && !string.IsNullOrEmpty(match.UniqueId))
                {
                    var matchPageUrl = $"https://dtp.i3ull3t.de:9443/match/{tournamentId}/{match.UniqueId}?uuid=true";
                    
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = matchPageUrl,
                        UseShellExecute = true
                    };
                    
                    Process.Start(processInfo);
                    System.Diagnostics.Debug.WriteLine($"🌐 [TournamentOverview] Opened Match-Page: {matchPageUrl}");
                }
            }
            else if (sender is Button knockoutButton && knockoutButton.DataContext is KnockoutMatch knockoutMatch)
            {
                var tournamentId = _hubService?.GetCurrentTournamentId();
                if (!string.IsNullOrEmpty(tournamentId) && !string.IsNullOrEmpty(knockoutMatch.UniqueId))
                {
                    var matchPageUrl = $"https://dtp.i3ull3t.de:9443/match/{tournamentId}/{knockoutMatch.UniqueId}?uuid=true";
                    
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = matchPageUrl,
                        UseShellExecute = true
                    };
                    
                    Process.Start(processInfo);
                    System.Diagnostics.Debug.WriteLine($"🌐 [TournamentOverview] Opened Knockout Match-Page: {matchPageUrl}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [TournamentOverview] Error opening match page: {ex.Message}");
        }
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
        try
        {
            var statusText = _localizationService.GetString("Ready");
            var currentDisplay = "";
            var cycleInfo = "";
            var buttonText = "";

            if (_isRunning)
            {
                statusText = _localizationService.GetString("AutoCyclingActive");
                buttonText = "⏸ " + _localizationService.GetString("StopCycling");
                
                if (_currentClassIndex < _activeTournamentTabs.Count)
                {
                    var currentTab = _activeTournamentTabs[_currentClassIndex];
                    currentDisplay = _localizationService.GetString("Showing") + ": " + currentTab.Header?.ToString();
                }
                
                var remainingTime = GetRemainingTime();
                TimerTextBlock.Text = $"{remainingTime / 60:D2}:{remainingTime % 60:D2}";
                cycleInfo = _localizationService.GetString("AutoCyclingActive");
            }
            else
            {
                statusText = _localizationService.GetString("ManualControl");
                buttonText = "▶ " + _localizationService.GetString("StartCycling");
                currentDisplay = _localizationService.GetString("ManualMode");
                TimerTextBlock.Text = "--:--";
                cycleInfo = _localizationService.GetString("CyclingStopped");
            }

            StatusTextBlock.Text = statusText;
            CurrentDisplayTextBlock.Text = currentDisplay;
            StartStopButton.Content = buttonText;
            CycleInfoTextBlock.Text = cycleInfo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateStatus error: {ex.Message}");
        }
    }

    private void UpdateTranslations()
    {
        try
        {
            Title = _localizationService.GetString("TournamentOverview");
            OverviewModeText.Text = _localizationService.GetString("OverviewModeTitle");
            ConfigureButton.Content = "Konfigurieren";
            CloseButton.ToolTip = _localizationService.GetString("Close");
            
            UpdateStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateTranslations error: {ex.Message}");
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
            //Background = new System.Windows.Media.LinearGradientBrush(
                //System.Windows.Media.Colors.WhiteSmoke,
                //System.Windows.Media.Colors.LightGray, 90),
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
                Text = "Turnier",
                FontSize = 48,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20),
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.DarkBlue
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
            Text = isLoserBracket ? "Loser Bracket" : "Winner Bracket",
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

        double roundWidth = 240;
        double matchHeight = 100;
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

        var mainGrid = new Grid();
        
        // Hauptinhalt (Spielerdaten)
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

        mainGrid.Children.Add(stackPanel);

        // ✅ NEU: QR-Code und Web-Button in der Ecke wenn Hub verfügbar
        if (_hubService != null && _hubService.IsRegisteredWithHub && !string.IsNullOrEmpty(match.UniqueId))
        {
            var qrPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 4, 4, 0) // 0,2,2,0 -> 0,4,4,0
            };

            // ✅ VERGRÖSSERT: Mini QR-Code im Tree View
            var qrImage = new System.Windows.Controls.Image
            {
                Width = 60, // 20 -> 40
                Height = 60, // 20 -> 40
                Margin = new Thickness(0, 0, 0, 2) // 0,0,0,1 -> 0,0,0,2
            };

            var qrBinding = new System.Windows.Data.Binding(".")
            {
                Source = match,
                Converter = new MatchToQRCodeConverter(_hubService, _localizationService)
            };
            qrImage.SetBinding(System.Windows.Controls.Image.SourceProperty, qrBinding);

            // ✅ VERGRÖSSERT: Mini Web-Button im Tree View
            var webButton = new Button
            {
                Content = "🌐",
                Width = 40, // 20 -> 40
                Height = 24, // 15 -> 24
                FontSize = 12, // 8 -> 12
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = "Match-Page öffnen"
            };

            webButton.Click += (s, e) =>
            {
                try
                {
                    var tournamentId = _hubService.GetCurrentTournamentId();
                    if (!string.IsNullOrEmpty(tournamentId))
                    {
                        var matchPageUrl = $"https://dtp.i3ull3t.de:9443/match/{tournamentId}/{match.UniqueId}?uuid=true";
                        var processInfo = new ProcessStartInfo
                        {
                            FileName = matchPageUrl,
                            UseShellExecute = true
                        };
                        Process.Start(processInfo);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error opening match page: {ex.Message}");
                }
            };

            qrPanel.Children.Add(qrImage);
            //qrPanel.Children.Add(webButton);
            mainGrid.Children.Add(qrPanel);
        }

        border.Child = mainGrid;

        // Enhanced tooltip with more match details and QR info
        var tooltipText = $"Match {match.Id} - {match.RoundDisplay}\n" +
                         $"Status: {match.StatusDisplay}\n" +
                         $"Spieler 1: {match.Player1?.Name ?? "TBD"}\n" +
                         $"Spieler 2: {match.Player2?.Name ?? "TBD"}";
        
        if (match.Status == MatchStatus.Finished && match.Winner != null)
        {
            tooltipText += $"\nSieger: {match.Winner.Name}";
        }

        if (_hubService != null && _hubService.IsRegisteredWithHub && !string.IsNullOrEmpty(match.UniqueId))
        {
            tooltipText += $"\n📱 QR-Code verfügbar für Mobile-Zugriff";
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

    private int GetRemainingTime()
    {
        var elapsed = (DateTime.Now - _lastCycleTime).TotalSeconds;
        var interval = _subTabInterval; // Default to sub-tab interval
        
        // Use class interval if we're about to switch classes
        var currentClassTab = MainTabControl.SelectedItem as TabItem;
        if (currentClassTab?.Content is TabControl subTabControl)
        {
            if (_currentSubTabIndex >= subTabControl.Items.Count - 1 && _activeTournamentTabs.Count > 1)
            {
                interval = _classInterval;
            }
        }
        
        var remaining = Math.Max(0, interval - (int)elapsed);
        return remaining;
    }
}

/// <summary>
/// ✅ NEU: Converter für Match zu QR-Code Konvertierung
/// </summary>
public class MatchToQRCodeConverter : System.Windows.Data.IValueConverter
{
    private readonly HubIntegrationService? _hubService;
    private readonly LocalizationService? _localizationService;

    public MatchToQRCodeConverter(HubIntegrationService? hubService, LocalizationService? localizationService)
    {
        _hubService = hubService;
        _localizationService = localizationService;
    }

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            string? matchUuid = null;
            
            if (value is Match match)
            {
                matchUuid = match.UniqueId;
            }
            else if (value is KnockoutMatch knockoutMatch)
            {
                matchUuid = knockoutMatch.UniqueId;
            }

            if (string.IsNullOrEmpty(matchUuid) || 
                _hubService == null || 
                !_hubService.IsRegisteredWithHub ||
                string.IsNullOrEmpty(_hubService.GetCurrentTournamentId()))
            {
                return null; // Kein QR-Code wenn Voraussetzungen nicht erfüllt
            }

            var tournamentId = _hubService.GetCurrentTournamentId();
            var matchPageUrl = $"https://dtp.i3ull3t.de:9443/match/{tournamentId}/{matchUuid}?uuid=true";

            return GenerateQRCodeImage(matchPageUrl);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [QR-Converter] Error: {ex.Message}");
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private BitmapImage? GenerateQRCodeImage(string url)
    {
        try
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
                
                using (var qrCode = new QRCoder.QRCode(qrCodeData))
                {
                    // ✅ VERGRÖSSERT: Höher aufgelöste QR Codes für DataGrid (6 -> 12)
                    using (var qrCodeBitmap = qrCode.GetGraphic(12, DrawingColor.Black, DrawingColor.White, true))
                    {
                        // Konvertiere zu WPF BitmapImage
                        var bitmapImage = new BitmapImage();
                        using (var stream = new MemoryStream())
                        {
                            qrCodeBitmap.Save(stream, ImageFormat.Png);
                            stream.Position = 0;
                            
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = stream;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze(); // Thread-Safety
                        }
                        
                        return bitmapImage;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [QR-Generator] Error generating QR code: {ex.Message}");
            return null;
        }
    }
}