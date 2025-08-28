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
using DartTournamentPlaner.Helpers;
using WinColor = System.Windows.Media.Color;
using WinBrushes = System.Windows.Media.Brushes;
using DrawingColor = System.Drawing.Color;

namespace DartTournamentPlaner.Views;

public partial class TournamentOverviewWindow : Window
{
    private readonly List<TournamentClass> _tournamentClasses;
    private readonly LocalizationService _localizationService;
    private readonly HubIntegrationService? _hubService;
    
    // Helper-Klassen für spezialisierte Funktionalitäten
    private readonly TournamentOverviewDataGridHelper _dataGridHelper;
    private readonly TournamentOverviewScrollManager _scrollManager;
    private readonly TournamentOverviewCycleManager _cycleManager;
    private readonly TournamentOverviewQRCodeHelper _qrCodeHelper;
    private readonly TournamentOverviewTabHelper _tabHelper;
    
    private int _currentClassIndex = 0;
    private int _currentSubTabIndex = 0;
    private List<TabItem> _activeTournamentTabs = new();
    
    // Configuration values - stored internally
    private bool _showOnlyActiveClasses = true;

    public TournamentOverviewWindow(
        List<TournamentClass> tournamentClasses,
        LocalizationService localizationService,
        HubIntegrationService? hubService = null)
    {
        InitializeComponent();
        
        _tournamentClasses = tournamentClasses;
        _localizationService = localizationService;
        _hubService = hubService;
        
        // Initialisiere Helper-Klassen direkt im Konstruktor
        _qrCodeHelper = new TournamentOverviewQRCodeHelper(_hubService, _localizationService, OnOpenMatchPageClick);
        
        _dataGridHelper = new TournamentOverviewDataGridHelper(
            _localizationService, 
            _hubService, 
            () => _qrCodeHelper.CreateQRCodeCellTemplate());
        
        _tabHelper = new TournamentOverviewTabHelper(
            _localizationService, 
            _dataGridHelper, 
            CreateTournamentTreeView);
        
        // Scroll Manager
        _scrollManager = new TournamentOverviewScrollManager(
            GetCurrentActiveDataGrid,
            () => { }, // Kein Callback mehr nötig - Scrollen blockiert Tab-Wechsel nicht
            UpdateStatus);
        
        // Cycle Manager
        _cycleManager = new TournamentOverviewCycleManager(
            _localizationService,
            () => MainTabControl,
            () => _activeTournamentTabs,
            () => _currentClassIndex,
            () => _currentSubTabIndex,
            (index) => _currentClassIndex = index,
            (index) => _currentSubTabIndex = index,
            SetCurrentSubTab,
            UpdateStatus,
            () => _scrollManager.StartScrolling());
        
        InitializeOverview();
        UpdateTranslations();
    }

    /// <summary>
    /// Diese Methode ist nicht mehr notwendig da die Initialisierung im Konstruktor erfolgt
    /// </summary>
    [Obsolete("Helper initialization moved to constructor")]
    private void InitializeHelpers()
    {
        // Nicht mehr verwendet - Initialisierung erfolgt im Konstruktor
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

            var tabItem = _tabHelper.CreateTournamentClassTab(tournamentClass, i);
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

    /// <summary>
    /// Event Handler für "Match-Page öffnen" Button
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
        if (_cycleManager.IsRunning)
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
        // Start cycle manager - kein komplexes Scroll-Management mehr nötig
        _cycleManager.StartCycling();
        
        StartStopButton.Content = "⏸ " + _localizationService.GetString("StopCycling");
        StartStopButton.Style = (Style)FindResource("DangerButton");

        UpdateStatus();
        
        System.Diagnostics.Debug.WriteLine("🎬 [TournamentOverview] Started automatic cycling");
    }

    private void StopCycling()
    {
        _cycleManager.StopCycling();
        _scrollManager.StopScrolling(); // Stoppe auch das Scrollen
        
        StartStopButton.Content = "▶ " + _localizationService.GetString("StartCycling");
        StartStopButton.Style = (Style)FindResource("SuccessButton");
        
        // Scrolle zurück zum Anfang
        _scrollManager.ResetScrollPosition();
        
        UpdateStatus();
        
        System.Diagnostics.Debug.WriteLine("⏹️ [TournamentOverview] Stopped automatic cycling");
    }

    private void Configure_Click(object sender, RoutedEventArgs e)
    {
        // Aktuelle Werte vom CycleManager holen
        var currentSubTabInterval = _cycleManager?.GetCurrentSubTabInterval() ?? 5;
        
        var dialog = new OverviewConfigDialog
        {
            Owner = this,
            ClassInterval = currentSubTabInterval, // Beide Werte gleich setzen für Benutzerfreundlichkeit
            SubTabInterval = currentSubTabInterval,
            ShowOnlyActiveClasses = _showOnlyActiveClasses
        };

        if (dialog.ShowDialog() == true)
        {
            _showOnlyActiveClasses = dialog.ShowOnlyActiveClasses;
            
            // Update cycle manager configuration mit den neuen Werten
            _cycleManager.UpdateConfiguration(dialog.ClassInterval, dialog.SubTabInterval);
            
            // Reinitialize if the "show only active" setting changed
            var wasRunning = _cycleManager.IsRunning;
            if (wasRunning)
            {
                StopCycling();
            }
            
            InitializeOverview();
            
            if (wasRunning)
            {
                StartCycling();
            }
            
            System.Diagnostics.Debug.WriteLine($"⚙️ [TournamentOverview] Configuration updated - SubTab: {dialog.SubTabInterval}s, ShowOnlyActive: {dialog.ShowOnlyActiveClasses}");
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
        
        // Cleanup für Helper-Klassen
        _scrollManager?.Dispose();
        _cycleManager?.Dispose();
        
        System.Diagnostics.Debug.WriteLine("🔄 [TournamentOverview] Window closing - all timers stopped");
    }

    private void UpdateStatus()
    {
        try
        {
            var statusText = _localizationService.GetString("Ready");
            var currentDisplay = "";
            var cycleInfo = "";
            var buttonText = "";

            if (_cycleManager.IsRunning)
            {
                statusText = _localizationService.GetString("AutoCyclingActive");
                buttonText = "⏸ " + _localizationService.GetString("StopCycling");
                
                if (_currentClassIndex < _activeTournamentTabs.Count)
                {
                    var currentTab = _activeTournamentTabs[_currentClassIndex];
                    currentDisplay = _localizationService.GetString("Showing") + ": " + currentTab.Header?.ToString();
                    
                    // Zeige Scroll-Status an (aber blockiert nicht den Tab-Wechsel)
                    if (_scrollManager.IsScrolling)
                    {
                        currentDisplay += " (📜 Scrolling...)";
                    }
                }
                
                // Vereinfachte Timer-Anzeige
                var remainingTime = _cycleManager.GetRemainingTime();
                TimerTextBlock.Text = $"{remainingTime / 60:D2}:{remainingTime % 60:D2}";
                cycleInfo = $"Auto-Cycling aktiv - {remainingTime}s verbleibend";
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

                //// Draw connection lines to next round
                //if (roundIndex < matchesByRound.Count - 1)
                //{
                //    DrawConnectionLine(canvas, xPos + roundWidth - 20, yPos + matchHeight / 2, 
                //                     xPos + roundWidth + roundSpacing - 20, yPos + matchHeight / 2);
                //}
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
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 2)
        };

        // Player names with winner highlighting
        var player1Text = new TextBlock
        {
            Text = match.Player1?.Name ?? "TBD",
            FontSize = 13,
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
            FontSize = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 2),
            FontStyle = FontStyles.Italic,
            Foreground = System.Windows.Media.Brushes.Gray
        };

        var player2Text = new TextBlock
        {
            Text = match.Player2?.Name ?? "TBD",
            FontSize = 13,
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
            FontSize = 14,
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

        // QR-Code und Web-Button in der Ecke wenn Hub verfügbar
        var qrPanel = _qrCodeHelper.CreateTreeViewQRCodePanel(match);
        if (qrPanel != null)
        {
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

    /// <summary>
    /// Findet das aktuell aktive DataGrid
    /// </summary>
    private DataGrid? GetCurrentActiveDataGrid()
    {
        try
        {
            var currentClassTab = MainTabControl.SelectedItem as TabItem;
            if (currentClassTab?.Content is not TabControl subTabControl) 
            {
                System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No sub-tab control found");
                return null;
            }

            var currentSubTab = subTabControl.SelectedItem as TabItem;
            if (currentSubTab?.Content == null) 
            {
                System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No current sub-tab content");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Current sub-tab header: {currentSubTab.Header}, Content type: {currentSubTab.Content.GetType().Name}");

            // Für Group Tabs: Suche in der Grid-Struktur
            if (currentSubTab.Content is Grid grid)
            {
                var dataGrid = grid.Children.OfType<DataGrid>().FirstOrDefault();
                if (dataGrid != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found DataGrid in Grid - Items count: {dataGrid.Items.Count}");
                    return dataGrid;
                }
                System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] No DataGrid found in Grid - Children: {grid.Children.Count}");
                foreach (var child in grid.Children)
                {
                    System.Diagnostics.Debug.WriteLine($"  - Child type: {child.GetType().Name}");
                }
            }

            // Für andere Tab-Typen: Direkte DataGrid-Suche
            if (currentSubTab.Content is DataGrid directGrid)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found direct DataGrid - Items count: {directGrid.Items.Count}");
                return directGrid;
            }

            // Recursive search in complex layouts
            var recursiveResult = FindDataGridRecursive(currentSubTab.Content);
            if (recursiveResult != null)
            {
                System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found DataGrid recursively - Items count: {recursiveResult.Items.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No DataGrid found recursively");
            }
            
            return recursiveResult;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error finding DataGrid: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Rekursive Suche nach DataGrid in komplexen Layouts
    /// </summary>
    private DataGrid? FindDataGridRecursive(object content)
    {
        if (content is DataGrid dataGrid)
            return dataGrid;

        if (content is Panel panel)
        {
            // Convert UIElementCollection to IEnumerable for LINQ
            var dataGridFromChildren = panel.Children.OfType<DataGrid>().FirstOrDefault();
            if (dataGridFromChildren != null) return dataGridFromChildren;
            
            // Search recursively in child elements
            foreach (UIElement child in panel.Children)
            {
                var result = FindDataGridRecursive(child);
                if (result != null) return result;
            }
        }

        if (content is ContentControl contentControl && contentControl.Content != null)
        {
            return FindDataGridRecursive(contentControl.Content);
        }

        return null;
    }
}