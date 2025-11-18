using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für die Erstellung und Verwaltung von DataGrids im TournamentOverviewWindow
/// Verantwortlich für alle DataGrid-spezifischen Operationen
/// FIXED: Improved QR code column debugging and dynamic creation
/// UPDATED: Added status LED indicators with colored ellipses
/// </summary>
public class TournamentOverviewDataGridHelper
{
    private readonly LocalizationService _localizationService;
    private readonly HubIntegrationService? _hubService;
    private readonly Func<DataTemplate> _createQRCodeCellTemplate;
    
    public TournamentOverviewDataGridHelper(
        LocalizationService localizationService, 
        HubIntegrationService? hubService,
        Func<DataTemplate> createQRCodeCellTemplate)
    {
        _localizationService = localizationService;
        _hubService = hubService;
        _createQRCodeCellTemplate = createQRCodeCellTemplate;
        
        // ✅ DEBUG: Helper-Initialisierung loggen
        System.Diagnostics.Debug.WriteLine($"🎯 [DataGridHelper] Initialized - HubService: {_hubService != null}, IsRegistered: {_hubService?.IsRegisteredWithHub}");
    }

    /// <summary>
    /// Erstellt ein DataTemplate für die Status-Spalte mit farbiger LED-Anzeige
    /// </summary>
    private DataTemplate CreateStatusCellTemplate()
    {
        var template = new DataTemplate();
        
        // Hauptcontainer: StackPanel horizontal
        var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
        stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
 
        // Farbige Status-Ellipse (LED-Indikator)
        var ellipseFactory = new FrameworkElementFactory(typeof(Ellipse));
        ellipseFactory.SetValue(Ellipse.WidthProperty, 10.0);
        ellipseFactory.SetValue(Ellipse.HeightProperty, 10.0);
        ellipseFactory.SetValue(Ellipse.MarginProperty, new Thickness(0, 0, 8, 0));
        ellipseFactory.SetValue(Ellipse.VerticalAlignmentProperty, VerticalAlignment.Center);
        
        // Binding für die Ellipse-Farbe basierend auf Status
        var ellipseBinding = new Binding("Status");
        ellipseBinding.Converter = new MatchStatusToColorConverter();
        ellipseFactory.SetBinding(Ellipse.FillProperty, ellipseBinding);
      
        stackPanelFactory.AppendChild(ellipseFactory);
        
        // Status-Text
        var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
        textBlockFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
    
        var textBinding = new Binding("StatusDisplay");
        textBlockFactory.SetBinding(TextBlock.TextProperty, textBinding);
 
        stackPanelFactory.AppendChild(textBlockFactory);
      
        template.VisualTree = stackPanelFactory;
     
        return template;
 }

    /// <summary>
    /// Erstellt ein DataGrid für Match-Anzeige
    /// </summary>
    public DataGrid CreateMatchesDataGrid(Group group)
    {
      // ✅ DEBUG: DataGrid-Erstellung loggen
        var hubStatus = _hubService?.IsRegisteredWithHub ?? false;
        System.Diagnostics.Debug.WriteLine($"🎯 [DataGridHelper] Creating matches DataGrid - Hub registered: {hubStatus}");

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
         Binding = new Binding("DisplayName"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
  {
            Header = _localizationService.GetString("Result"),
    Binding = new Binding("ScoreDisplay"),
         Width = new DataGridLength(100)
        });

     // ✅ NEU: Status mit farbiger LED-Anzeige
        dataGrid.Columns.Add(new DataGridTemplateColumn
   {
       Header = _localizationService.GetString("Status"),
        Width = new DataGridLength(120),
CellTemplate = CreateStatusCellTemplate()
});

 // ✅ FIXED: Always add QR-Code column, let converter handle visibility logic
        System.Diagnostics.Debug.WriteLine($"🎯 [DataGridHelper] Adding QR code column - HubService: {_hubService != null}");
        
        var qrColumn = new DataGridTemplateColumn
 {
     Header = "📱",
            Width = new DataGridLength(120),
 CellTemplate = _createQRCodeCellTemplate()
      };
  dataGrid.Columns.Add(qrColumn);
   
        System.Diagnostics.Debug.WriteLine($"✅ [DataGridHelper] Matches DataGrid created with {dataGrid.Columns.Count} columns");

        return dataGrid;
    }

    /// <summary>
    /// Erstellt ein DataGrid für Standings-Anzeige
    /// </summary>
    public DataGrid CreateStandingsDataGrid(Group group)
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
            Binding = new Binding("Position"),
            Width = new DataGridLength(50)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Player"),
            Binding = new Binding("Player.Name"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("PointsShort"),
            Binding = new Binding("Points"),
            Width = new DataGridLength(50)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("WinDrawLoss"),
            Binding = new Binding("RecordDisplay"),
            Width = new DataGridLength(80)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Sets"),
            Binding = new Binding("SetRecordDisplay"),
            Width = new DataGridLength(70)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Legs"),
            Binding = new Binding("LegRecordDisplay"),
            Width = new DataGridLength(70)
        });

        return dataGrid;
    }

    /// <summary>
    /// Erstellt ein DataGrid für Knockout-Matches
    /// </summary>
 public DataGrid CreateKnockoutDataGrid(IEnumerable<KnockoutMatch> knockoutMatches)
  {
        // ✅ DEBUG: DataGrid-Erstellung loggen
     var hubStatus = _hubService?.IsRegisteredWithHub ?? false;
        System.Diagnostics.Debug.WriteLine($"🎯 [DataGridHelper] Creating knockout DataGrid - Hub registered: {hubStatus}");
        
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
         Binding = new Binding("RoundDisplay"),
       Width = new DataGridLength(100)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Match"),
      Binding = new Binding("DisplayName"),
    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
      });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = _localizationService.GetString("Result"),
            Binding = new Binding("ScoreDisplay"),
  Width = new DataGridLength(100)
        });

        // ✅ NEU: Status mit farbiger LED-Anzeige
        dataGrid.Columns.Add(new DataGridTemplateColumn
        {
       Header = _localizationService.GetString("Status"),
            Width = new DataGridLength(120),
          CellTemplate = CreateStatusCellTemplate()
      });

        // ✅ FIXED: Always add QR-Code column, let converter handle visibility logic
        System.Diagnostics.Debug.WriteLine($"🎯 [DataGridHelper] Adding QR code column to knockout DataGrid");
        
     var qrColumn = new DataGridTemplateColumn
        {
       Header = "📱",
   Width = new DataGridLength(120),
            CellTemplate = _createQRCodeCellTemplate()
        };
        dataGrid.Columns.Add(qrColumn);
    
        System.Diagnostics.Debug.WriteLine($"✅ [DataGridHelper] Knockout DataGrid created with {dataGrid.Columns.Count} columns");

      return dataGrid;
    }

    /// <summary>
    /// Erstellt ein DataGrid für Finals-Matches
    /// </summary>
    public DataGrid CreateFinalsDataGrid(IEnumerable<Match> finalsMatches)
    {
        // ✅ DEBUG: DataGrid-Erstellung loggen
    var hubStatus = _hubService?.IsRegisteredWithHub ?? false;
        System.Diagnostics.Debug.WriteLine($"🎯 [DataGridHelper] Creating finals DataGrid - Hub registered: {hubStatus}");
        
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
     Binding = new Binding("DisplayName"),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        dataGrid.Columns.Add(new DataGridTextColumn
        {
  Header = _localizationService.GetString("Result"),
Binding = new Binding("ScoreDisplay"),
    Width = new DataGridLength(100)
        });

        // ✅ NEU: Status mit farbiger LED-Anzeige
      dataGrid.Columns.Add(new DataGridTemplateColumn
        {
        Header = _localizationService.GetString("Status"),
      Width = new DataGridLength(120),
     CellTemplate = CreateStatusCellTemplate()
     });

  // ✅ FIXED: Always add QR-Code column, let converter handle visibility logic
    System.Diagnostics.Debug.WriteLine($"🎯 [DataGridHelper] Adding QR code column to finals DataGrid");
        
     var qrColumn = new DataGridTemplateColumn
 {
        Header = "📱",
            Width = new DataGridLength(120),
            CellTemplate = _createQRCodeCellTemplate()
        };
     dataGrid.Columns.Add(qrColumn);
        
        System.Diagnostics.Debug.WriteLine($"✅ [DataGridHelper] Finals DataGrid created with {dataGrid.Columns.Count} columns");

        return dataGrid;
    }
}

/// <summary>
/// Converter für Match-Status zu Farbe (für LED-Indikator)
/// </summary>
public class MatchStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is MatchStatus status)
        {
            return status switch
  {
   MatchStatus.NotStarted => new SolidColorBrush(Color.FromRgb(148, 163, 184)), // Grau - SecondaryTextBrush
                MatchStatus.InProgress => new SolidColorBrush(Color.FromRgb(245, 158, 11)),  // Orange - WarningBrush
    MatchStatus.Finished => new SolidColorBrush(Color.FromRgb(16, 185, 129)),    // Grün - SuccessBrush
        MatchStatus.Bye => new SolidColorBrush(Color.FromRgb(59, 130, 246)), // Blau - AccentBrush
           _ => new SolidColorBrush(Color.FromRgb(148, 163, 184))        // Default: Grau
            };
        }
        
    return new SolidColorBrush(Color.FromRgb(148, 163, 184)); // Default: Grau
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
 throw new NotImplementedException();
    }
}