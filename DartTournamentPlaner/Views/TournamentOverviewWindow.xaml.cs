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
using DartTournamentPlaner.Services.License;
using WinColor = System.Windows.Media.Color;
using WinBrushes = System.Windows.Media.Brushes;
using DrawingColor = System.Drawing.Color;

namespace DartTournamentPlaner.Views;

public partial class TournamentOverviewWindow : Window
{
    private readonly List<TournamentClass> _tournamentClasses;
    private readonly LocalizationService _localizationService;
    private readonly HubIntegrationService? _hubService;
    private readonly LicenseFeatureService? _licenseFeatureService;
    
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
        HubIntegrationService? hubService = null,
        LicenseFeatureService? licenseFeatureService = null)
    {
        InitializeComponent();
        
        _tournamentClasses = tournamentClasses;
        _localizationService = localizationService;
        _hubService = hubService;
        
        // ✅ KORRIGIERT: Hole LicenseFeatureService vom MainWindow falls nicht übergeben
        _licenseFeatureService = licenseFeatureService;
        if (_licenseFeatureService == null)
        {
            try
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    var licenseServiceField = mainWindow.GetType()
                        .GetField("_licenseFeatureService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    _licenseFeatureService = licenseServiceField?.GetValue(mainWindow) as LicenseFeatureService;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentOverview] Could not get LicenseFeatureService: {ex.Message}");
            }
        }
        
        // Initialisiere Helper-Klassen direkt im Konstruktor
        _qrCodeHelper = new TournamentOverviewQRCodeHelper(_hubService, _localizationService, OnOpenMatchPageClick);
        
        _dataGridHelper = new TournamentOverviewDataGridHelper(
            _localizationService, 
            _hubService, 
            () => _qrCodeHelper.CreateQRCodeCellTemplate());
        
        // ✅ ERWEITERT: TabHelper mit LicenseFeatureService
        _tabHelper = new TournamentOverviewTabHelper(
            _localizationService, 
            _dataGridHelper, 
            CreateTournamentTreeView,
            _licenseFeatureService);
        
        // Scroll Manager
        _scrollManager = new TournamentOverviewScrollManager(
   () => GetCurrentActiveDataGrid() as DataGrid, // ✅ KORRIGIERT: Cast zu DataGrid für Kompatibilität
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
            () => {
     // ✅ AKTUALISIERT: Übergebe Cycle-Dauer an Scroll-Manager
      var cycleDuration = _cycleManager.GetCurrentSubTabInterval();
      _scrollManager.StartScrolling(cycleDuration);
            });
        
        // ✅ NEW: Hub-Status-Event-Handler hinzufügen
        if (_hubService != null)
        {
            _hubService.HubStatusChanged += OnHubStatusChanged;
            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentOverview] Hub status event handler registered");
        }
        
        InitializeOverview();
        UpdateTranslations();

        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentOverview] Window initialized with {_tournamentClasses.Count} classes, " +
            $"Hub: {_hubService != null}, Hub registered: {_hubService?.IsRegisteredWithHub}, License: {_licenseFeatureService != null}");
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

        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentOverview] Overview initialized with {MainTabControl.Items.Count} active tabs");
    }

    /// <summary>
    /// Event Handler für "Match-Page öffnen" Button
    /// UPDATED: Verwendet neue dart-scoring.html URL mit Match-UUID Parameter
    /// </summary>
    private void OnOpenMatchPageClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.DataContext is Match match)
            {
                if (!string.IsNullOrEmpty(match.UniqueId))
                {
                    // ✅ NEW URL FORMAT: dart-scoring.html with match UUID parameter
                    var dartScoringUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?match={match.UniqueId}&uuid=true";
                    
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = dartScoringUrl,
                        UseShellExecute = true
                    };
                    
                    Process.Start(processInfo);
                    System.Diagnostics.Debug.WriteLine($"🌐 [TournamentOverview] Opened dart-scoring page: {dartScoringUrl}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [TournamentOverview] Match UUID is empty for Match {match.Id}");
                }
            }
            else if (sender is Button knockoutButton && knockoutButton.DataContext is KnockoutMatch knockoutMatch)
            {
                if (!string.IsNullOrEmpty(knockoutMatch.UniqueId))
                {
                    // ✅ NEW URL FORMAT: dart-scoring.html with match UUID parameter
                    var dartScoringUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?match={knockoutMatch.UniqueId}&uuid=true";
                    
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = dartScoringUrl,
                        UseShellExecute = true
                    };
                    
                    Process.Start(processInfo);
                    System.Diagnostics.Debug.WriteLine($"🌐 [TournamentOverview] Opened dart-scoring page for knockout match: {dartScoringUrl}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [TournamentOverview] KnockoutMatch UUID is empty for Match {knockoutMatch.Id}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [TournamentOverview] Error opening dart-scoring page: {ex.Message}");
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
        // ✅ VERBESSERT: Aktuelle Werte vom CycleManager holen und besseres Dialog-Handling
        var currentSubTabInterval = _cycleManager?.GetCurrentSubTabInterval() ?? 5;
        
        var dialog = new OverviewConfigDialog
        {
            Owner = this,
            ClassInterval = currentSubTabInterval,
            SubTabInterval = currentSubTabInterval,
            ShowOnlyActiveClasses = _showOnlyActiveClasses
        };

        var dialogResult = dialog.ShowDialog();
        
        if (dialogResult == true)
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
            
            System.Diagnostics.Debug.WriteLine($"⚙️ [TournamentOverview] Configuration saved - " +
                $"SubTab: {dialog.SubTabInterval}s, ShowOnlyActive: {dialog.ShowOnlyActiveClasses}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"⚙️ [TournamentOverview] Configuration dialog cancelled");
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
        
        // ✅ NEW: Hub-Event-Handler entfernen
        if (_hubService != null)
        {
            _hubService.HubStatusChanged -= OnHubStatusChanged;
            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentOverview] Hub status event handler unregistered");
        }
        
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
                    
                    // ✅ ERWEITERT: Zeige auch aktuellen Sub-Tab an
                    if (currentTab.Content is TabControl subTabControl && subTabControl.SelectedItem is TabItem selectedSubTab)
                    {
                        currentDisplay += $" → {selectedSubTab.Header}";
                    }
                    
                    // Zeige Scroll-Status an (aber blockiert nicht den Tab-Wechsel)
                    if (_scrollManager.IsScrolling)
                    {
                        currentDisplay += " (📜 Scrolling...)";
                    }
                }
                
                // ✅ KORRIGIERT: Einfache, exakte Timer-Anzeige
                var remainingTime = _cycleManager.GetRemainingTime();
                
                // ✅ VERBESSERT: Einfache, klare Anzeige
                if (remainingTime <= 0)
                {
                    TimerTextBlock.Text = "00:00";
                }
                else
                {
                    var minutes = remainingTime / 60;
                    var seconds = remainingTime % 60;
                    TimerTextBlock.Text = $"{minutes:D2}:{seconds:D2}";
                }
                
                // ✅ KORRIGIERT: Einfache Status-Informationen
                var totalInterval = _cycleManager.GetCurrentSubTabInterval();
                var elapsedTime = totalInterval - remainingTime;
                var elapsedPercent = totalInterval > 0 ? (elapsedTime * 100.0 / totalInterval) : 0;
                
                cycleInfo = $"Auto-Cycling aktiv ({elapsedPercent:F0}% - {remainingTime}s verbleibend)";
                
                // ✅ KORRIGIERT: Einfache Farbwechsel basierend auf verbleibender Zeit
                if (remainingTime <= 3)
                {
                    TimerTextBlock.Foreground = new SolidColorBrush(WinColor.FromRgb(220, 38, 38)); // Rot
                }
                else if (remainingTime <= 10)
                {
                    TimerTextBlock.Foreground = new SolidColorBrush(WinColor.FromRgb(245, 158, 11)); // Orange
                }
                else
                {
                    TimerTextBlock.Foreground = new SolidColorBrush(WinColor.FromRgb(34, 197, 94)); // Grün
                }
            }
            else
            {
                statusText = _localizationService.GetString("ManualControl");
                buttonText = "▶ " + _localizationService.GetString("StartCycling");
                currentDisplay = _localizationService.GetString("ManualMode");
                TimerTextBlock.Text = "--:--";
                TimerTextBlock.Foreground = new SolidColorBrush(WinColor.FromRgb(107, 114, 128)); // Grau
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
            ConfigureButton.Content = "⚙ " + (_localizationService.GetString("Configure") ?? "Konfigurieren");
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
    /// ✅ ERWEITERT: Findet das aktuell aktive DataGrid oder scrollbaren Content im Sub-Tab
    /// Navigiert durch: MainTab → SubTabControl → aktueller SubTab → Content
    /// </summary>
    private object? GetCurrentActiveDataGrid()
    {
        try
        {
            // 1. Hole das aktuell ausgewählte Haupttab (Tournament-Klasse: Platin, Gold, etc.)
   var currentClassTab = MainTabControl.SelectedItem as TabItem;
   if (currentClassTab == null)
        {
       System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No main tab selected");
         return null;
            }

System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Main tab: {currentClassTab.Header}");

            // 2. Das Content des Haupttabs ist ein TabControl mit Sub-Tabs (Gruppenphase, Winner Bracket, etc.)
            if (currentClassTab.Content is not TabControl subTabControl)
            {
    System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] Main tab content is not a TabControl");
      return null;
            }

            System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] SubTabControl found with {subTabControl.Items.Count} sub-tabs");

    // 3. Hole den aktuell ausgewählten Sub-Tab
    var currentSubTab = subTabControl.SelectedItem as TabItem;
      if (currentSubTab == null)
            {
     System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No sub-tab selected");
        return null;
            }

            System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Current sub-tab: {currentSubTab.Header}, Content type: {currentSubTab.Content?.GetType().Name ?? "null"}");

            // 4. Analysiere den Content des Sub-Tabs
         if (currentSubTab.Content == null)
            {
 System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] Sub-tab content is null");
                return null;
          }

            // Fall 1: Content ist direkt ein ScrollViewer (z.B. TreeView mit Canvas)
            if (currentSubTab.Content is ScrollViewer scrollViewer)
       {
    System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found direct ScrollViewer - ScrollableHeight: {scrollViewer.ScrollableHeight}");
         return scrollViewer;
            }

   // Fall 2: Content ist ein Grid mit DataGrids (z.B. Gruppenphase mit Matches + Standings)
 if (currentSubTab.Content is Grid grid)
 {
  System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found Grid with {grid.Children.Count} children");
  
                // Suche nach DataGrids im Grid
                var dataGrids = grid.Children.OfType<DataGrid>().ToList();
  if (dataGrids.Count > 0)
    {
      // Nimm das erste DataGrid (normalerweise die Matches)
          var dataGrid = dataGrids[0];
           System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found DataGrid in Grid - Items: {dataGrid.Items.Count}");
         return dataGrid;
         }

        // Wenn kein DataGrid, suche nach ScrollViewer im Grid
         var scrollViewerInGrid = grid.Children.OfType<ScrollViewer>().FirstOrDefault();
      if (scrollViewerInGrid != null)
          {
    System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] Found ScrollViewer in Grid");
        return scrollViewerInGrid;
        }
            }

     // Fall 3: Content ist direkt ein DataGrid (z.B. KO-Phase Matches, Finals)
       if (currentSubTab.Content is DataGrid directGrid)
   {
    System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found direct DataGrid - Items: {directGrid.Items.Count}");
    return directGrid;
            }

            // Fall 4: Content ist ein FrameworkElement (z.B. TreeView, PlayerStatisticsView)
      if (currentSubTab.Content is FrameworkElement frameworkElement)
 {
              System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found FrameworkElement: {frameworkElement.GetType().Name}");
      
   // Suche rekursiv nach ScrollViewer oder DataGrid
      var foundScrollViewer = FindVisualChildRecursive<ScrollViewer>(frameworkElement);
     if (foundScrollViewer != null)
   {
     System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found ScrollViewer recursively - ScrollableHeight: {foundScrollViewer.ScrollableHeight}");
      return foundScrollViewer;
           }

 var foundDataGrid = FindVisualChildRecursive<DataGrid>(frameworkElement);
   if (foundDataGrid != null)
       {
        System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found DataGrid recursively - Items: {foundDataGrid.Items.Count}");
return foundDataGrid;
          }
  }

       System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No scrollable content found");
            return null;
        }
        catch (Exception ex)
  {
    System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error finding active content: {ex.Message}\n{ex.StackTrace}");
          return null;
    }
    }

/// <summary>
    /// ✅ NEU: Rekursive Suche nach einem bestimmten Visual Child-Typ in der Visual Tree
    /// </summary>
    private T? FindVisualChildRecursive<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

 // Prüfe zuerst ob der Parent selbst der gesuchte Typ ist
        if (parent is T typedParent)
        return typedParent;

        // Durchsuche alle Children
        var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
   {
            var child = VisualTreeHelper.GetChild(parent, i);
         
            // Prüfe das Child
            if (child is T typedChild)
        return typedChild;

    // Rekursive Suche in den Children des Childs
            var result = FindVisualChildRecursive<T>(child);
 if (result != null)
return result;
     }

        return null;
    }

    /// <summary>
    /// OBSOLETE: Alte rekursive Suche - ersetzt durch GetCurrentActiveDataGrid
    /// </summary>
    [Obsolete("Use GetCurrentActiveDataGrid instead")]
    private DataGrid? FindDataGridRecursive(object content)
    {
   if (content is DataGrid dataGrid)
        return dataGrid;

        if (content is Panel panel)
        {
var dataGridFromChildren = panel.Children.OfType<DataGrid>().FirstOrDefault();
      if (dataGridFromChildren != null) return dataGridFromChildren;
    
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

    // ✅ NEW: Event handler for hub status changes
    private void OnHubStatusChanged(object? sender, EventArgs e)
    {
        try
        {
            if (_hubService == null) return;

            // Aktiviere oder deaktiviere die Schaltflächen basierend auf dem Hub-Status
            var isRegistered = _hubService.IsRegisteredWithHub;
            
            Dispatcher.Invoke(() =>
            {
                // Beispiel: Aktiviere oder deaktiviere eine Schaltfläche
                // MyButton.IsEnabled = isRegistered;
                
                UpdateStatus();
            });
            
            System.Diagnostics.Debug.WriteLine($"🔄 [TournamentOverview] Hub status changed: Registered = {isRegistered}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [TournamentOverview] Error handling hub status change: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEW: Event-Handler für Hub-Status-Änderungen
    /// </summary>
    private void OnHubStatusChanged(bool isRegistered)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentOverview] Hub status changed: {isRegistered}");
            
            // UI-Update auf UI-Thread durchführen
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"🔄 [TournamentOverview] Reinitializing overview due to hub status change");
                
                // ✅ FIXED: Bessere Reinitialisierung - behalte aktuellen Tab
                var currentTabIndex = MainTabControl.SelectedIndex;
                var currentSubTabIndex = _currentSubTabIndex;
                var wasRunning = _cycleManager?.IsRunning ?? false;
                
                if (wasRunning)
                {
                    StopCycling();
                }
                
                // Reinitialize overview to rebuild DataGrids with updated QR code columns
                InitializeOverview();
                
                // Restore tab selection
                if (currentTabIndex >= 0 && currentTabIndex < MainTabControl.Items.Count)
                {
                    MainTabControl.SelectedIndex = currentTabIndex;
                    _currentClassIndex = currentTabIndex;
                    _currentSubTabIndex = currentSubTabIndex;
                    SetCurrentSubTab();
                }
                
                if (wasRunning)
                {
                    StartCycling();
                }
                
                System.Diagnostics.Debug.WriteLine($"✅ [TournamentOverview] Overview reinitialized with new hub status: {isRegistered}");
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [TournamentOverview] Error handling hub status change: {ex.Message}");
        }
    }
}