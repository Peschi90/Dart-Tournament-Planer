using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Helpers;

namespace DartTournamentPlaner.Services.Print
{
    /// <summary>
    /// Factory-Klasse für die Erstellung von Print-Tabellen
    /// Verwaltet die Erstellung von Standings-, Match- und Knockout-Tabellen mit QR-Code-Support
/// </summary>
  public class PrintTableFactory
    {
        private readonly LocalizationService? _localizationService;
        private readonly PrintQRCodeHelper? _qrCodeHelper;

 public PrintTableFactory(LocalizationService? localizationService, PrintQRCodeHelper? qrCodeHelper)
   {
          _localizationService = localizationService;
        _qrCodeHelper = qrCodeHelper;
        }

        /// <summary>
        /// Erstellt eine Standings-Tabelle für eine Gruppe
        /// </summary>
        public FrameworkElement CreateStandingsTable(List<PlayerStanding> standings)
 {
        var grid = new Grid
  {
         Background = Brushes.White,
      MaxWidth = 600
            };

    // Spalten definieren
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
       grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
     grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
          grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
       grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
       grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // Header
  grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
      var headers = new[] { 
 _localizationService?.GetString("Position") ?? "Pos",
     _localizationService?.GetString("PlayerHeader") ?? "Spieler",
           _localizationService?.GetString("MatchesPlayedShort") ?? "Sp",
            _localizationService?.GetString("WinsShort") ?? "S",
            _localizationService?.GetString("DrawsShort") ?? "U",
    _localizationService?.GetString("LossesShort") ?? "N",
                _localizationService?.GetString("PointsHeader") ?? "Pkt",
         _localizationService?.GetString("SetsHeader") ?? "Sets"
      };
            
  for (int col = 0; col < headers.Length; col++)
  {
         var headerBorder = new Border
        {
    Background = new SolidColorBrush(Color.FromRgb(52, 58, 64)),
  BorderBrush = Brushes.White,
       BorderThickness = new Thickness(1),
            Padding = new Thickness(5)
    };
           
          var headerText = new TextBlock
         {
      Text = headers[col],
  Foreground = Brushes.White,
       FontWeight = FontWeights.Bold,
           FontSize = 22,
   TextAlignment = col == 1 ? TextAlignment.Left : TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
  };
      
    headerBorder.Child = headerText;
      Grid.SetRow(headerBorder, 0);
            Grid.SetColumn(headerBorder, col);
    grid.Children.Add(headerBorder);
          }

  // Datenzeilen
         for (int row = 0; row < standings.Count; row++)
            {
         grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
              var standing = standings[row];

        var values = new string[]
                {
   standing.Position.ToString(),
       standing.Player?.Name ?? (_localizationService?.GetString("Unknown") ?? "Unknown"),
               standing.MatchesPlayed.ToString(),
        standing.Wins.ToString(),
             standing.Draws.ToString(),
            standing.Losses.ToString(),
  standing.Points.ToString(),
          $"{standing.SetsWon}:{standing.SetsLost}"
   };

     for (int col = 0; col < values.Length; col++)
        {
 var cellBorder = new Border
       {
    Background = row % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
         BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
     BorderThickness = new Thickness(0.5),
             Padding = new Thickness(4)
          };
        
         var cellText = new TextBlock
             {
  Text = values[col],
      FontSize = 20,
      TextAlignment = col == 1 ? TextAlignment.Left : TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
           TextTrimming = TextTrimming.CharacterEllipsis
          };
 
            // Hervorhebung für erste Plätze
   if (col == 0)
            {
         if (standing.Position == 1)
        cellText.Foreground = new SolidColorBrush(Color.FromRgb(212, 175, 55));
    else if (standing.Position == 2)
 cellText.Foreground = new SolidColorBrush(Color.FromRgb(192, 192, 192));
   else if (standing.Position == 3)
    cellText.Foreground = new SolidColorBrush(Color.FromRgb(205, 127, 50));
        }
        
             cellBorder.Child = cellText;
              Grid.SetRow(cellBorder, row + 1);
        Grid.SetColumn(cellBorder, col);
        grid.Children.Add(cellBorder);
    }
            }

     return new Border
       {
          BorderBrush = new SolidColorBrush(Color.FromRgb(52, 58, 64)),
 BorderThickness = new Thickness(2),
     CornerRadius = new CornerRadius(4),
        Child = grid
   };
        }

        /// <summary>
        /// Erstellt eine Match-Tabelle mit optionalen QR-Codes
        /// </summary>
        public FrameworkElement CreateMatchesTable(List<Match> matches)
        {
            bool showQRCodes = _qrCodeHelper?.AreQRCodesAvailable ?? false;
      
  var grid = new Grid
            {
       Background = Brushes.White,
  MaxWidth = showQRCodes ? 850 : 700
        };

    // Spalten definieren - mit QR-Code Spalte wenn verfügbar
   // ? KORRIGIERT: Erste Spalte breiter (60 statt 40)
    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(showQRCodes ? 160 : 200) });
  grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
   grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
   
            if (showQRCodes)
            {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
 }

        // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
  
    var headersList = new List<string> { 
     _localizationService?.GetString("MatchNumber") ?? "Nr",
    _localizationService?.GetString("MatchHeader") ?? "Spiel",
  _localizationService?.GetString("StatusHeader") ?? "Status",
        _localizationService?.GetString("ResultHeader") ?? "Ergebnis",
  _localizationService?.GetString("WinnerHeader") ?? "Gewinner"
   };
    
    if (showQRCodes)
    {
                headersList.Add("QR");
       }
       
    for (int col = 0; col < headersList.Count; col++)
            {
         var headerBorder = new Border
          {
   Background = new SolidColorBrush(Color.FromRgb(52, 58, 64)),
         BorderBrush = Brushes.White,
   BorderThickness = new Thickness(1),
 Padding = new Thickness(5)
     };
     
          var headerText = new TextBlock
       {
        Text = headersList[col],
     Foreground = Brushes.White,
           FontWeight = FontWeights.Bold,
      FontSize = 22,
        TextAlignment = col == 1 ? TextAlignment.Left : TextAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center
     };
          
     headerBorder.Child = headerText;
        Grid.SetRow(headerBorder, 0);
      Grid.SetColumn(headerBorder, col);
                grid.Children.Add(headerBorder);
            }

          // Datenzeilen
     for (int row = 0; row < matches.Count; row++)
            {
    // ? KORRIGIERT: Reduzierte Row Height von 105 auf 95px
   grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(showQRCodes ? 95 : 25) });
             var match = matches[row];

        string statusText;
    string resultText;
                string winnerText;
             
      if (match.IsBye || match.Status == MatchStatus.Bye)
  {
        statusText = _localizationService?.GetString("ByeStatus") ?? "FREILOS";
            resultText = _localizationService?.GetString("Unknown") ?? "-";
winnerText = match.Player1?.Name ?? (_localizationService?.GetString("Unknown") ?? "-");
          }
          else if (match.Status == MatchStatus.Finished)
 {
   statusText = _localizationService?.GetString("FinishedStatus") ?? "BEENDET";
  resultText = match.ScoreDisplay ?? (_localizationService?.GetString("Unknown") ?? "-");
     winnerText = match.Winner?.Name ?? (_localizationService?.GetString("Draw") ?? "Unentschieden");
     }
   else if (match.Status == MatchStatus.InProgress)
           {
        statusText = _localizationService?.GetString("InProgressStatus") ?? "LÄUFT";
                resultText = match.ScoreDisplay ?? (_localizationService?.GetString("Unknown") ?? "-");
               winnerText = _localizationService?.GetString("Unknown") ?? "-";
                }
    else
            {
    statusText = _localizationService?.GetString("PendingStatus") ?? "AUSSTEHEND";
 resultText = _localizationService?.GetString("Unknown") ?? "-";
   winnerText = _localizationService?.GetString("Unknown") ?? "-";
    }

      var gameText = match.IsBye 
  ? _localizationService?.GetString("ByeGame", match.Player1?.Name) ?? $"{match.Player1?.Name} (Freilos)"
           : _localizationService?.GetString("VersusGame", match.Player1?.Name, match.Player2?.Name) ?? $"{match.Player1?.Name} vs {match.Player2?.Name}";

           var values = new string[]
        {
   (row + 1).ToString(),
          gameText,
     statusText,
      resultText,
          winnerText
        };

         for (int col = 0; col < values.Length; col++)
                {
     var cellBorder = new Border
        {
     Background = row % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
  BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
         BorderThickness = new Thickness(0.5),
          Padding = new Thickness(4)
     };
              
           var cellText = new TextBlock
        {
      Text = values[col],
           FontSize = 20,
            TextAlignment = col == 1 ? TextAlignment.Left : TextAlignment.Center,
  VerticalAlignment = VerticalAlignment.Center,
     TextTrimming = TextTrimming.CharacterEllipsis
      };

        cellBorder.Child = cellText;
Grid.SetRow(cellBorder, row + 1);
     Grid.SetColumn(cellBorder, col);
     grid.Children.Add(cellBorder);
       }
     
            // QR-Code Zelle
                if (showQRCodes)
     {
        var qrCellBorder = new Border
      {
      Background = row % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
     BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
             BorderThickness = new Thickness(0.5),
       Padding = new Thickness(2)
      };
                    
       var qrImage = _qrCodeHelper?.GenerateMatchQRCode(match, 5);
                 
           if (qrImage != null)
    {
         var image = new System.Windows.Controls.Image
      {
         Source = qrImage,
      Width = 90,
  Height = 90,
        Stretch = System.Windows.Media.Stretch.Uniform
  };
               qrCellBorder.Child = image;
}
         else
       {
     qrCellBorder.Child = new TextBlock
      {
        Text = "-",
       FontSize = 10,
TextAlignment = TextAlignment.Center,
   VerticalAlignment = VerticalAlignment.Center,
           Foreground = Brushes.LightGray
           };
   }
        
               Grid.SetRow(qrCellBorder, row + 1);
  Grid.SetColumn(qrCellBorder, values.Length);
       grid.Children.Add(qrCellBorder);
 }
   }

            return new Border
         {
       BorderBrush = new SolidColorBrush(Color.FromRgb(52, 58, 64)),
         BorderThickness = new Thickness(2),
          CornerRadius = new CornerRadius(4),
       Child = grid
            };
 }

    /// <summary>
        /// Erstellt eine Knockout-Match-Tabelle mit optionalen QR-Codes
        /// </summary>
        public FrameworkElement CreateKnockoutMatchesTable(List<KnockoutMatch> matches)
        {
      bool showQRCodes = _qrCodeHelper?.AreQRCodesAvailable ?? false;
      
      var grid = new Grid
        {
    Background = Brushes.White,
   MaxWidth = showQRCodes ? 900 : 750
            };

            // Spalten definieren - mit QR-Code Spalte wenn verfügbar
      // ? KORRIGIERT: Erste Spalte breiter (60 statt 40)
  grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
 grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
          grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(showQRCodes ? 160 : 200) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
         grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            
      if (showQRCodes)
          {
 grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            }

          // Header
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            
   var headersList = new List<string> { 
      _localizationService?.GetString("MatchNumber") ?? "Nr",
                _localizationService?.GetString("RoundHeader") ?? "Runde",
    _localizationService?.GetString("MatchHeader") ?? "Spiel",
          _localizationService?.GetString("StatusHeader") ?? "Status",
            _localizationService?.GetString("ResultHeader") ?? "Ergebnis",
 _localizationService?.GetString("WinnerHeader") ?? "Gewinner"
      };
            
  if (showQRCodes)
{
           headersList.Add("QR");
            }
            
   for (int col = 0; col < headersList.Count; col++)
       {
        var headerBorder = new Border
              {
 Background = new SolidColorBrush(Color.FromRgb(52, 58, 64)),
        BorderBrush = Brushes.White,
   BorderThickness = new Thickness(1),
     Padding = new Thickness(5)
       };
       
         var headerText = new TextBlock
             {
         Text = headersList[col],
       Foreground = Brushes.White,
        FontWeight = FontWeights.Bold,
           FontSize = 22,
         TextAlignment = col == 2 ? TextAlignment.Left : TextAlignment.Center,
           VerticalAlignment = VerticalAlignment.Center
    };
      
  headerBorder.Child = headerText;
  Grid.SetRow(headerBorder, 0);
                Grid.SetColumn(headerBorder, col);
grid.Children.Add(headerBorder);
            }

// Datenzeilen
            for (int row = 0; row < matches.Count; row++)
 {
          grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(showQRCodes ? 95 : 25) });
   var match = matches[row];

        string statusText;
         string resultText;
string winnerText;
          
      bool isBye = match.Status == MatchStatus.Bye || 
   (match.Player1 != null && match.Player2 == null) || 
          (match.Player1 == null && match.Player2 != null);
          
        if (isBye)
            {
        statusText = _localizationService?.GetString("ByeStatus") ?? "FREILOS";
  resultText = _localizationService?.GetString("Unknown") ?? "-";
    winnerText = match.Player1?.Name ?? match.Player2?.Name ?? (_localizationService?.GetString("Unknown") ?? "-");
     }
  else if (match.Status == MatchStatus.Finished)
          {
   statusText = _localizationService?.GetString("FinishedStatus") ?? "BEENDET";
      resultText = match.ScoreDisplay ?? (_localizationService?.GetString("Unknown") ?? "-");
          winnerText = match.Winner?.Name ?? (_localizationService?.GetString("Draw") ?? "Unentschieden");
         }
      else if (match.Status == MatchStatus.InProgress)
         {
          statusText = _localizationService?.GetString("InProgressStatus") ?? "LÄUFT";
        resultText = match.ScoreDisplay ?? (_localizationService?.GetString("Unknown") ?? "-");
         winnerText = _localizationService?.GetString("Unknown") ?? "-";
      }
         else
        {
           statusText = _localizationService?.GetString("PendingStatus") ?? "AUSSTEHEND";
           resultText = _localizationService?.GetString("Unknown") ?? "-";
      winnerText = _localizationService?.GetString("Unknown") ?? "-";
        }

                var player1Name = match.Player1?.Name ?? "TBD";
      var player2Name = match.Player2?.Name ?? "TBD";
      var matchText = isBye 
        ? _localizationService?.GetString("ByeGame", player1Name) ?? $"{player1Name} (Freilos)"
     : _localizationService?.GetString("VersusGame", player1Name, player2Name) ?? $"{player1Name} vs {player2Name}";

        var values = new string[]
       {
 $"#{match.Id}",
      GetRoundDisplayName(match.Round),
            matchText,
 statusText,
           resultText,
       winnerText
   };

             for (int col = 0; col < values.Length; col++)
        {
        var cellBorder = new Border
            {
Background = row % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
   BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
       BorderThickness = new Thickness(0.5),
   Padding = new Thickness(4)
      };
     
  var cellText = new TextBlock
        {
  Text = values[col],
   FontSize = 20,
 TextAlignment = col == 2 ? TextAlignment.Left : TextAlignment.Center,
    VerticalAlignment = VerticalAlignment.Center,
     TextTrimming = TextTrimming.CharacterEllipsis
            };

 if (values[col].Contains("TBD"))
{
             cellText.FontStyle = FontStyles.Italic;
      cellText.Foreground = new SolidColorBrush(Color.FromRgb(108, 117, 125));
          }
   
              cellBorder.Child = cellText;
 Grid.SetRow(cellBorder, row + 1);
    Grid.SetColumn(cellBorder, col);
        grid.Children.Add(cellBorder);
                }

          // QR-Code Zelle
           if (showQRCodes)
   {
        var qrCellBorder = new Border
    {
        Background = row % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
              BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                BorderThickness = new Thickness(0.5),
   Padding = new Thickness(2)
  };
          
     var qrImage = _qrCodeHelper?.GenerateMatchQRCode(match, 5);
        
  if (qrImage != null)
 {
    // ? KORRIGIERT: QR-Code Größe von 100x100 auf 90x90
       var image = new System.Windows.Controls.Image
           {
      Source = qrImage,
          Width = 90,
     Height = 90,
           Stretch = System.Windows.Media.Stretch.Uniform
         };
           qrCellBorder.Child = image;
     }
       else
     {
              qrCellBorder.Child = new TextBlock
            {
    Text = "-",
      FontSize = 10,
     TextAlignment = TextAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
     Foreground = Brushes.LightGray
              };
       }
             
          Grid.SetRow(qrCellBorder, row + 1);
          Grid.SetColumn(qrCellBorder, values.Length);
   grid.Children.Add(qrCellBorder);
 }
   }

            return new Border
    {
             BorderBrush = new SolidColorBrush(Color.FromRgb(52, 58, 64)),
     BorderThickness = new Thickness(2),
        CornerRadius = new CornerRadius(4),
    Child = grid
            };
        }

        private string GetRoundDisplayName(KnockoutRound round)
        {
       return round switch
        {
      KnockoutRound.Best64 => _localizationService?.GetString("Best64") ?? "Beste 64",
       KnockoutRound.Best32 => _localizationService?.GetString("Best32") ?? "Beste 32", 
  KnockoutRound.Best16 => _localizationService?.GetString("Best16") ?? "Beste 16",
          KnockoutRound.Quarterfinal => _localizationService?.GetString("Quarterfinal") ?? "Viertelfinale",
       KnockoutRound.Semifinal => _localizationService?.GetString("Semifinal") ?? "Halbfinale", 
         KnockoutRound.Final => _localizationService?.GetString("Final") ?? "Finale",
         KnockoutRound.GrandFinal => _localizationService?.GetString("GrandFinal") ?? "Grand Final",
 KnockoutRound.LoserRound1 => "LR1",
  KnockoutRound.LoserRound2 => "LR2",
         KnockoutRound.LoserRound3 => "LR3",
        KnockoutRound.LoserRound4 => "LR4",
         KnockoutRound.LoserRound5 => "LR5",
                KnockoutRound.LoserRound6 => "LR6",
      KnockoutRound.LoserRound7 => "LR7",
    KnockoutRound.LoserRound8 => "LR8",
      KnockoutRound.LoserRound9 => "LR9",
      KnockoutRound.LoserRound10 => "LR10",
       KnockoutRound.LoserRound11 => "LR11",
   KnockoutRound.LoserRound12 => "LR12",
       KnockoutRound.LoserFinal => "LF",
                _ => ""
      };
      }
    }
}
