using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using WinColor = System.Windows.Media.Color;
using WinBrushes = System.Windows.Media.Brushes;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für die Erstellung und Verwaltung von Tab-Strukturen im TournamentOverviewWindow
/// Verantwortlich für die Erstellung von Haupt-Tabs und Sub-Tabs basierend auf Tournament-Phasen
/// </summary>
public class TournamentOverviewTabHelper
{
    private readonly LocalizationService _localizationService;
    private readonly TournamentOverviewDataGridHelper _dataGridHelper;
    private readonly Func<TournamentClass, bool, FrameworkElement> _createTournamentTreeView;

    public TournamentOverviewTabHelper(
        LocalizationService localizationService,
        TournamentOverviewDataGridHelper dataGridHelper,
        Func<TournamentClass, bool, FrameworkElement> createTournamentTreeView)
    {
        _localizationService = localizationService;
        _dataGridHelper = dataGridHelper;
        _createTournamentTreeView = createTournamentTreeView;
    }

    /// <summary>
    /// Erstellt einen Tournament-Class Tab mit Header und Content
    /// </summary>
    public TabItem CreateTournamentClassTab(TournamentClass tournamentClass, int classIndex)
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
        SetTabColors(colorEllipse, colorBorder, classIndex);

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

    /// <summary>
    /// Erstellt das Content-TabControl basierend auf der aktuellen Tournament-Phase
    /// </summary>
    public TabControl CreateContentTabControl(TournamentClass tournamentClass)
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
            var winnerBracketMatchesTab = CreateKnockoutBracketTab(
                _localizationService.GetString("WinnerBracketMatches"), 
                tournamentClass, false, false);
            tabControl.Items.Add(winnerBracketMatchesTab);

            var winnerBracketTreeTab = CreateKnockoutBracketTab(
                _localizationService.GetString("WinnerBracketTree"), 
                tournamentClass, false, true);
            tabControl.Items.Add(winnerBracketTreeTab);

            // Add Loser Bracket if double elimination
            if (tournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                var loserBracketMatchesTab = CreateKnockoutBracketTab(
                    _localizationService.GetString("LoserBracketMatches"), 
                    tournamentClass, true, false);
                tabControl.Items.Add(loserBracketMatchesTab);

                var loserBracketTreeTab = CreateKnockoutBracketTab(
                    _localizationService.GetString("LoserBracketTree"), 
                    tournamentClass, true, true);
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

    /// <summary>
    /// Erstellt einen Group-Tab mit Matches und Standings
    /// </summary>
    public TabItem CreateGroupTab(Group group, TournamentClass tournamentClass)
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
        var matchesGrid = _dataGridHelper.CreateMatchesDataGrid(group);
        Grid.SetColumn(matchesGrid, 0);
        grid.Children.Add(matchesGrid);

        // Standings DataGrid
        var standingsGrid = _dataGridHelper.CreateStandingsDataGrid(group);
        Grid.SetColumn(standingsGrid, 2);
        grid.Children.Add(standingsGrid);

        tabItem.Content = grid;
        return tabItem;
    }

    /// <summary>
    /// Erstellt einen Knockout-Bracket Tab
    /// </summary>
    public TabItem CreateKnockoutBracketTab(string header, TournamentClass tournamentClass, bool isLoserBracket, bool isTreeView = false)
    {
        var tabItem = new TabItem { Header = header };

        if (isTreeView)
        {
            // Create tournament tree view for better visual representation
            var treeContent = _createTournamentTreeView(tournamentClass, isLoserBracket);
            tabItem.Content = treeContent;
        }
        else
        {
            // Create matches data grid for knockout
            var knockoutMatches = isLoserBracket 
                ? tournamentClass.GetLoserBracketMatches()
                : tournamentClass.GetWinnerBracketMatches();

            var dataGrid = _dataGridHelper.CreateKnockoutDataGrid(knockoutMatches);
            tabItem.Content = dataGrid;
        }

        return tabItem;
    }

    /// <summary>
    /// Erstellt einen Finals Tab
    /// </summary>
    public TabItem CreateFinalsTab(TournamentClass tournamentClass)
    {
        var tabItem = new TabItem { Header = _localizationService.GetString("FinalsTab") };

        // Similar to knockout but for finals matches
        var finalsMatches = tournamentClass.GetFinalsMatches();
        var dataGrid = _dataGridHelper.CreateFinalsDataGrid(finalsMatches);
        tabItem.Content = dataGrid;

        return tabItem;
    }

    /// <summary>
    /// Setzt die Farben für Tab-Header basierend auf dem Class-Index
    /// </summary>
    private void SetTabColors(System.Windows.Shapes.Ellipse colorEllipse, Border colorBorder, int classIndex)
    {
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
    }
}