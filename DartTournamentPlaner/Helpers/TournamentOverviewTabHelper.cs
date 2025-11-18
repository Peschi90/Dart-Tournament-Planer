using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Controls;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Models.License;
using WinColor = System.Windows.Media.Color;
using WinBrushes = System.Windows.Media.Brushes;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für die Erstellung und Verwaltung von Tab-Strukturen im TournamentOverviewWindow
/// Verantwortlich für die Erstellung von Haupt-Tabs und Sub-Tabs basierend auf Tournament-Phasen
/// ✅ ERWEITERT: Mit Statistik-Tab-Integration
/// </summary>
public class TournamentOverviewTabHelper
{
    private readonly LocalizationService _localizationService;
    private readonly TournamentOverviewDataGridHelper _dataGridHelper;
    private readonly Func<TournamentClass, bool, FrameworkElement> _createTournamentTreeView;
    private readonly LicenseFeatureService? _licenseFeatureService;

    public TournamentOverviewTabHelper(
        LocalizationService localizationService,
        TournamentOverviewDataGridHelper dataGridHelper,
        Func<TournamentClass, bool, FrameworkElement> createTournamentTreeView,
        LicenseFeatureService? licenseFeatureService = null)
    {
        _localizationService = localizationService;
        _dataGridHelper = dataGridHelper;
        _createTournamentTreeView = createTournamentTreeView;
        _licenseFeatureService = licenseFeatureService;
    }

    /// <summary>
    /// Erstellt einen Tournament-Class Tab mit Header und Content
    /// ✅ ERWEITERT: Inklusive Statistik-Tabs
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
    /// ✅ ERWEITERT: Mit automatischer Statistik-Tab-Integration
    /// ✅ AKTUALISIERT: Gruppenphase-Tabs bleiben immer sichtbar, auch bei späteren Phasen
    /// ✅ KORRIGIERT: Verbesserte Null-Checks und Debug-Ausgaben für Refresh-Probleme
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
        
        System.Diagnostics.Debug.WriteLine($"🎯 [TabHelper] CreateContentTabControl for {tournamentClass.Name}");
        System.Diagnostics.Debug.WriteLine($"  Current Phase: {currentPhase?.PhaseType.ToString() ?? "null"}");
        System.Diagnostics.Debug.WriteLine($"  Total Phases: {tournamentClass.Phases.Count}");

        // ✅ KORRIGIERT: Suche explizit nach der Gruppenphase in allen Phasen
        var groupPhase = tournamentClass.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        
 if (groupPhase != null)
   {
System.Diagnostics.Debug.WriteLine($"  ✅ GroupPhase found with {groupPhase.Groups?.Count ?? 0} groups");
        }
        else
        {
   System.Diagnostics.Debug.WriteLine($"  ❌ No GroupPhase found in Phases collection!");
            
  // ✅ DEBUG: Liste alle verfügbaren Phasen auf
        foreach (var phase in tournamentClass.Phases)
      {
      System.Diagnostics.Debug.WriteLine($"     - Phase: {phase.PhaseType}, Active: {phase.IsActive}, Groups: {phase.Groups?.Count ?? 0}");
            }
   }
        
  // ✅ NEUE LOGIK: Zeige immer Gruppenphasen-Tabs an, falls Gruppenphase vorhanden ist
      // (auch wenn K.O. oder Finals bereits aktiv sind)
        if (groupPhase != null && groupPhase.Groups != null && groupPhase.Groups.Count > 0)
        {
         System.Diagnostics.Debug.WriteLine($"🎯 [TabHelper] Adding {groupPhase.Groups.Count} group tabs from GroupPhase");
        
     // Gruppenphasen-Tabs hinzufügen
            foreach (var group in groupPhase.Groups.Where(g => g != null && g.Players != null && g.Players.Count > 0))
   {
                System.Diagnostics.Debug.WriteLine($"  ✅ Creating tab for group: {group.Name} with {group.Players.Count} players, {group.Matches?.Count ?? 0} matches");
         
      try
        {
           var groupTab = CreateGroupTab(group, tournamentClass);
  tabControl.Items.Add(groupTab);
           }
          catch (Exception ex)
          {
  System.Diagnostics.Debug.WriteLine($"  ❌ Error creating group tab: {ex.Message}");
            }
         }
 }
        else
  {
            System.Diagnostics.Debug.WriteLine($"⚠️ [TabHelper] No GroupPhase found or GroupPhase has no groups");
        }
  
     // ✅ ZUSÄTZLICH: Zeige K.O.-Phase Tabs, falls K.O.-Phase aktiv ist
      if (currentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
        {
            System.Diagnostics.Debug.WriteLine($"🎯 [TabHelper] Current phase is KnockoutPhase - adding knockout tabs");
      
         // Create tabs for knockout brackets - both matches and tree views
   var winnerBracketMatchesTab = CreateKnockoutBracketTab(
 _localizationService.GetString("WinnerBracketMatches") ?? "Winner Bracket Spiele", 
           tournamentClass, false, false);
tabControl.Items.Add(winnerBracketMatchesTab);

      var winnerBracketTreeTab = CreateKnockoutBracketTab(
         _localizationService.GetString("WinnerBracketTree") ?? "Winner Bracket Baum", 
                tournamentClass, false, true);
    tabControl.Items.Add(winnerBracketTreeTab);

            // Add Loser Bracket if double elimination
      if (tournamentClass.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
  {
       var loserBracketMatchesTab = CreateKnockoutBracketTab(
     _localizationService.GetString("LoserBracketMatches") ?? "Loser Bracket Spiele", 
      tournamentClass, true, false);
                tabControl.Items.Add(loserBracketMatchesTab);

          var loserBracketTreeTab = CreateKnockoutBracketTab(
  _localizationService.GetString("LoserBracketTree") ?? "Loser Bracket Baum", 
 tournamentClass, true, true);
         tabControl.Items.Add(loserBracketTreeTab);
    }
        }
        // ✅ ZUSÄTZLICH: Zeige Finals-Tab, falls Finals-Phase aktiv ist
else if (currentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
        {
     System.Diagnostics.Debug.WriteLine($"🎯 [TabHelper] Current phase is RoundRobinFinals - adding finals tab");
  
          // Create finals tab
        var finalsTab = CreateFinalsTab(tournamentClass);
            tabControl.Items.Add(finalsTab);
        }

        // ✅ NEU: Statistik-Tab automatisch hinzufügen
  var statisticsTab = CreateStatisticsTab(tournamentClass);
        if (statisticsTab != null)
        {
  tabControl.Items.Add(statisticsTab);
        }

        if (tabControl.Items.Count > 0)
        {
            tabControl.SelectedIndex = 0;
     }
        else
     {
          System.Diagnostics.Debug.WriteLine($"⚠️ [TabHelper] No tabs created for tournament class: {tournamentClass.Name}");
  }

System.Diagnostics.Debug.WriteLine($"✅ [TabHelper] Created {tabControl.Items.Count} tabs total");
    
        return tabControl;
    }

    /// <summary>
    /// Erstellt einen Group-Tab mit Matches und Standings
    /// ✅ ERWEITERT: Mit Debug-Ausgaben und Null-Checks
    /// </summary>
    public TabItem CreateGroupTab(Group group, TournamentClass tournamentClass)
    {
        System.Diagnostics.Debug.WriteLine($"🔹 [TabHelper] CreateGroupTab START for: {group.Name}");
        System.Diagnostics.Debug.WriteLine($"  Group Players: {group.Players?.Count ?? 0}");
        System.Diagnostics.Debug.WriteLine($"  Group Matches: {group.Matches?.Count ?? 0}");
        
        var tabItem = new TabItem
        {
   Header = group.Name,
            Tag = group // ✅ NEU: Speichere Gruppe als Tag für spätere Referenz
        };

      // Create a grid with matches and standings
    var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

 try
        {
     // Matches DataGrid
   System.Diagnostics.Debug.WriteLine($"  Creating Matches DataGrid...");
            var matchesGrid = _dataGridHelper.CreateMatchesDataGrid(group);
            
            if (matchesGrid != null && matchesGrid.ItemsSource != null)
     {
         System.Diagnostics.Debug.WriteLine($"  ✅ Matches DataGrid created with {matchesGrid.Items.Count} items");
 }
            else
   {
     System.Diagnostics.Debug.WriteLine($"  ⚠️ Matches DataGrid is null or has no ItemsSource!");
  }
            
    Grid.SetColumn(matchesGrid, 0);
        grid.Children.Add(matchesGrid);

   // Standings DataGrid
            System.Diagnostics.Debug.WriteLine($"  Creating Standings DataGrid...");
            var standingsGrid = _dataGridHelper.CreateStandingsDataGrid(group);
            
     if (standingsGrid != null && standingsGrid.ItemsSource != null)
  {
       System.Diagnostics.Debug.WriteLine($"  ✅ Standings DataGrid created with {standingsGrid.Items.Count} items");
}
         else
     {
         System.Diagnostics.Debug.WriteLine($"  ⚠️ Standings DataGrid is null or has no ItemsSource!");
            }
          
      Grid.SetColumn(standingsGrid, 2);
            grid.Children.Add(standingsGrid);
   }
        catch (Exception ex)
        {
       System.Diagnostics.Debug.WriteLine($"  ❌ Error creating DataGrids: {ex.Message}");
        }

        tabItem.Content = grid;
     
        System.Diagnostics.Debug.WriteLine($"🔹 [TabHelper] CreateGroupTab END for: {group.Name}");
        
 return tabItem;
    }

    /// <summary>
    /// Erstellt einen Knockout-Bracket Tab
    /// ✅ KORRIGIERT: Verhindert mehrfaches Generieren des Brackets durch gecachte Matches
    /// </summary>
    public TabItem CreateKnockoutBracketTab(string header, TournamentClass tournamentClass, bool isLoserBracket, bool isTreeView = false)
    {
  System.Diagnostics.Debug.WriteLine($"🔹 [TabHelper] CreateKnockoutBracketTab START: {header}, isTreeView: {isTreeView}, isLoserBracket: {isLoserBracket}");
        
        var tabItem = new TabItem { Header = header };

     if (isTreeView)
        {
            System.Diagnostics.Debug.WriteLine($"  Creating TreeView for {(isLoserBracket ? "Loser" : "Winner")} Bracket");
       
            // Create tournament tree view for better visual representation
            var treeContent = _createTournamentTreeView(tournamentClass, isLoserBracket);
            tabItem.Content = treeContent;
        }
        else
        {
        System.Diagnostics.Debug.WriteLine($"  Creating DataGrid for {(isLoserBracket ? "Loser" : "Winner")} Bracket");
            
            // ✅ KORRIGIERT: Hole Matches OHNE sie neu zu generieren
            // GetWinnerBracketMatches und GetLoserBracketMatches sollten nur lesen, nicht neu generieren
        var knockoutMatches = isLoserBracket 
        ? tournamentClass.GetLoserBracketMatches()
       : tournamentClass.GetWinnerBracketMatches();

            System.Diagnostics.Debug.WriteLine($"  Retrieved {knockoutMatches.Count} matches from bracket");
    
         var dataGrid = _dataGridHelper.CreateKnockoutDataGrid(knockoutMatches);
            tabItem.Content = dataGrid;
        }

        System.Diagnostics.Debug.WriteLine($"🔹 [TabHelper] CreateKnockoutBracketTab END: {header}");
        
        return tabItem;
    }

    /// <summary>
    /// Erstellt einen Finals Tab
    /// </summary>
    public TabItem CreateFinalsTab(TournamentClass tournamentClass)
    {
        var tabItem = new TabItem { Header = _localizationService.GetString("FinalsTab") ?? "Finale" };

        // Similar to knockout but for finals matches
        var finalsMatches = tournamentClass.GetFinalsMatches();
        var dataGrid = _dataGridHelper.CreateFinalsDataGrid(finalsMatches);
        tabItem.Content = dataGrid;

        return tabItem;
    }

    /// <summary>
    /// ✅ KORRIGIERT: Erstellt einen Statistik-Tab nur wenn Lizenz vorhanden ist
    /// </summary>
    public TabItem? CreateStatisticsTab(TournamentClass tournamentClass)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"📊 [TabHelper] Checking statistics license for class: {tournamentClass.Name}");

            // ✅ KORRIGIERT: Prüfe Lizenz ZUERST - kein Tab wenn keine Lizenz
            var hasStatisticsLicense = CheckStatisticsLicense();
            
            if (!hasStatisticsLicense)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [TabHelper] No statistics license - not creating statistics tab");
                return null; // ✅ Kein Tab erstellen wenn keine Lizenz
            }

            System.Diagnostics.Debug.WriteLine($"✅ [TabHelper] Statistics license available - creating tab");
            
            var tabItem = new TabItem();
            
            // Header mit Icon
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            
            var iconText = new TextBlock
            {
                Text = "📊",
                FontSize = 12,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            var headerText = new TextBlock
            {
                Text = _localizationService.GetString("Statistics") ?? "Statistiken",
                FontWeight = FontWeights.Medium,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };

            headerPanel.Children.Add(iconText);
            headerPanel.Children.Add(headerText);
            tabItem.Header = headerPanel;

            // ✅ Da wir hier sind, haben wir eine gültige Lizenz - erstelle Statistik-View
            var statisticsView = new PlayerStatisticsView
            {
                TournamentClass = tournamentClass,
                Margin = new Thickness(10)
            };
            
            // Ensure translations are updated
            statisticsView.UpdateTranslations();
            
            tabItem.Content = statisticsView;
            
            System.Diagnostics.Debug.WriteLine($"✅ [TabHelper] Statistics tab created successfully with licensed view");
            return tabItem;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [TabHelper] Error creating statistics tab: {ex.Message}");
            return null; // No statistics tab if error
        }
    }

    /// <summary>
    /// ✅ KORRIGIERT: Prüft ob Statistics-Lizenz vorhanden ist
    /// </summary>
    private bool CheckStatisticsLicense()
    {
        try
        {
            if (_licenseFeatureService == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ [TabHelper] No LicenseFeatureService available");
                return false;
            }

            // ✅ KORRIGIERT: Verwende den korrekten Feature-Identifier und Methode
            var isEnabled = _licenseFeatureService.HasFeature(LicenseFeatures.STATISTICS);
            System.Diagnostics.Debug.WriteLine($"🔍 [TabHelper] Statistics license check result: {isEnabled}");
            
            return isEnabled;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [TabHelper] Error checking statistics license: {ex.Message}");
            return false;
        }
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