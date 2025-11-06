using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using DartTournamentPlaner.Helpers;

namespace DartTournamentPlaner.Services.Print
{
    /// <summary>
    /// Hilfsklasse für Print-Layout-Berechnungen und gemeinsame Layout-Funktionen
    /// Verwaltet Seitendimensionen, Header, Footer und Layout-Konstanten
    /// </summary>
    public class PrintLayoutHelper
    {
        private readonly LocalizationService? _localizationService;
        private readonly PrintQRCodeHelper? _qrCodeHelper;

        // Layout-Konstanten
        public const double PORTRAIT_WIDTH = 793.7;
        public const double PORTRAIT_HEIGHT = 1122.5;
        public const double LANDSCAPE_WIDTH = 1122.5;
public const double LANDSCAPE_HEIGHT = 793.7;
      
        public const double MARGIN_LEFT = 50;
        public const double MARGIN_RIGHT = 50;
     public const double MARGIN_TOP = 90;
        public const double MARGIN_BOTTOM = 100;

   public PrintLayoutHelper(LocalizationService? localizationService, PrintQRCodeHelper? qrCodeHelper)
        {
     _localizationService = localizationService;
     _qrCodeHelper = qrCodeHelper;
        }

    /// <summary>
        /// Gibt die Seitenbreite basierend auf QR-Code-Verfügbarkeit zurück
        /// </summary>
        public double GetPageWidth()
{
            bool showQRCodes = _qrCodeHelper?.AreQRCodesAvailable ?? false;
     return showQRCodes ? LANDSCAPE_WIDTH : PORTRAIT_WIDTH;
        }

        /// <summary>
 /// Gibt die Seitenhöhe basierend auf QR-Code-Verfügbarkeit zurück
        /// </summary>
    public double GetPageHeight()
        {
            bool showQRCodes = _qrCodeHelper?.AreQRCodesAvailable ?? false;
            return showQRCodes ? LANDSCAPE_HEIGHT : PORTRAIT_HEIGHT;
   }

      /// <summary>
        /// Berechnet verfügbare Höhe für Content
  /// </summary>
  public double GetAvailableContentHeight(double currentYPosition)
        {
      return GetPageHeight() - currentYPosition - MARGIN_BOTTOM;
        }

        /// <summary>
  /// Berechnet die geschätzte Zeilenhöhe basierend auf QR-Code-Verfügbarkeit
        /// </summary>
      public double GetEstimatedRowHeight()
        {
            bool showQRCodes = _qrCodeHelper?.AreQRCodesAvailable ?? false;
         return showQRCodes ? 95 : 25;
        }

        /// <summary>
        /// Erstellt eine neue FixedPage mit korrekten Dimensionen
     /// </summary>
        public FixedPage CreateFixedPage()
        {
   return new FixedPage
       {
        Width = GetPageWidth(),
   Height = GetPageHeight(),
      Background = Brushes.White
        };
   }

 /// <summary>
        /// Fügt Header zu einer Seite hinzu
        /// </summary>
      public void AddPageHeader(FixedPage page, string title)
        {
   var titleBlock = new TextBlock
       {
          Text = title,
      FontSize = 24,
    FontWeight = FontWeights.Bold,
      Foreground = Brushes.Black
            };
     
            FixedPage.SetLeft(titleBlock, MARGIN_LEFT);
            FixedPage.SetTop(titleBlock, 30);
            page.Children.Add(titleBlock);

       var dateBlock = new TextBlock
            {
                Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
      FontSize = 12,
 Foreground = Brushes.Gray
            };
            
            FixedPage.SetRight(dateBlock, MARGIN_RIGHT);
  FixedPage.SetTop(dateBlock, 35);
       page.Children.Add(dateBlock);

      var line = new Line
            {
          X1 = MARGIN_LEFT, 
      X2 = GetPageWidth() - MARGIN_RIGHT, 
             Y1 = 75, 
         Y2 = 75,
     Stroke = Brushes.LightGray,
                StrokeThickness = 1.5
    };
        page.Children.Add(line);
   }

        /// <summary>
        /// Fügt Footer zu einer Seite hinzu
        /// </summary>
public void AddPageFooter(FixedPage page)
        {
     double footerY = GetPageHeight() - 52;
     
   var line = new Line
            {
    X1 = MARGIN_LEFT, 
                X2 = GetPageWidth() - MARGIN_RIGHT, 
        Y1 = footerY, 
                Y2 = footerY,
             Stroke = Brushes.LightGray,
    StrokeThickness = 1
   };
     page.Children.Add(line);

       var footerText = _localizationService?.GetString("CreatedWith") ?? "Erstellt mit Dart Tournament Planner";
            var footerBlock = new TextBlock
{
          Text = footerText,
          FontSize = 8,
       Foreground = Brushes.Gray
            };
            
     FixedPage.SetLeft(footerBlock, MARGIN_LEFT);
       FixedPage.SetTop(footerBlock, footerY + 10);
          page.Children.Add(footerBlock);
}

  /// <summary>
 /// Berechnet maximale Anzahl von Matches pro Seite
        /// </summary>
        public int CalculateMaxMatchesPerPage(double availableHeight)
     {
   double rowHeight = GetEstimatedRowHeight();
            
            // ? KORRIGIERT: Reserve für Header der Match-Tabelle (~80px)
        double tableHeaderReserve = 80;
       double effectiveHeight = availableHeight - tableHeaderReserve;
      
            int maxMatches = Math.Max(1, (int)(effectiveHeight / rowHeight));
         
            System.Diagnostics.Debug.WriteLine($"[CalculateMaxMatchesPerPage] Available: {availableHeight}px, Row height: {rowHeight}px, Table header: {tableHeaderReserve}px, Max matches: {maxMatches}");
            
      return maxMatches;
        }
    }
}
