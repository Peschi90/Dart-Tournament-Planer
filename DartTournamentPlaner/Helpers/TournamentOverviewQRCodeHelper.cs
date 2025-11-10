using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using QRCoder;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using WinColor = System.Windows.Media.Color;
using DrawingColor = System.Drawing.Color;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für QR-Code Funktionalität im TournamentOverviewWindow
/// Verwaltet QR-Code Generierung, Templates und Event-Handling
/// UPDATED: Verwendet neue dart-scoring.html URL mit Match-UUID UND Tournament-ID Parameter
/// FIXED: Verbesserte Debug-Ausgaben und Fehlerbehandlung
/// </summary>
public class TournamentOverviewQRCodeHelper
{
    private readonly HubIntegrationService? _hubService;
    private readonly LocalizationService _localizationService;
    private readonly Action<object, RoutedEventArgs> _onOpenMatchPageClick;
    private readonly string? _tournamentId;  // ⭐ NEU

    public TournamentOverviewQRCodeHelper(
        HubIntegrationService? hubService, 
        LocalizationService localizationService,
        Action<object, RoutedEventArgs> onOpenMatchPageClick,
        string? tournamentId = null)  // ⭐ NEU
    {
        _hubService = hubService;
        _localizationService = localizationService;
        _onOpenMatchPageClick = onOpenMatchPageClick;
        _tournamentId = tournamentId;  // ⭐ NEU
        
        // ✅ DEBUG: Initialisierungs-Status loggen
        System.Diagnostics.Debug.WriteLine($"🎯 [QRCodeHelper] Initialized - HubService: {_hubService != null}, IsRegistered: {_hubService?.IsRegisteredWithHub}, TournamentId: {_tournamentId ?? "null"}");
    }

    /// <summary>
    /// Erstellt DataTemplate für QR-Code Zellen in DataGrid
    /// </summary>
    public DataTemplate CreateQRCodeCellTemplate()
    {
        // ✅ DEBUG: Template-Erstellung loggen
        var hubStatus = _hubService?.IsRegisteredWithHub ?? false;
     System.Diagnostics.Debug.WriteLine($"🎯 [QRCodeHelper] Creating QR template - Hub registered: {hubStatus}, TournamentId: {_tournamentId ?? "null"}");
        
        var template = new DataTemplate();
        
        var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
      stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
        stackPanelFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
      stackPanelFactory.SetValue(StackPanel.VerticalAlignmentProperty, VerticalAlignment.Center);
        
        // QR-Code Image
        var imageFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Image));
        imageFactory.SetValue(System.Windows.Controls.Image.WidthProperty, 80.0);
        imageFactory.SetValue(System.Windows.Controls.Image.HeightProperty, 80.0);
        imageFactory.SetValue(System.Windows.Controls.Image.StretchProperty, Stretch.Uniform);
        imageFactory.SetValue(System.Windows.Controls.Image.MarginProperty, new Thickness(4));
   imageFactory.SetValue(System.Windows.Controls.Image.ToolTipProperty, "QR-Code zum Match scannen - öffnet dart-scoring.html");
        
        // QR-Code generieren und als Source setzen
  var binding = new Binding(".");
  var converter = new MatchToQRCodeConverter(_hubService, _localizationService, _tournamentId);// ⭐ Tournament-ID übergeben
        binding.Converter = converter;
  imageFactory.SetBinding(System.Windows.Controls.Image.SourceProperty, binding);
        
        stackPanelFactory.AppendChild(imageFactory);
        
   template.VisualTree = stackPanelFactory;
        
        // ✅ DEBUG: Template erstellt
        System.Diagnostics.Debug.WriteLine($"✅ [QRCodeHelper] QR template created successfully");
        
 return template;
    }

    /// <summary>
    /// Erstellt QR-Code und Web-Button für Tournament Tree View Match Controls
    /// </summary>
    public UIElement? CreateTreeViewQRCodePanel(KnockoutMatch match)
    {
        // ✅ IMPROVED DEBUG: Detaillierte Prüfung aller Bedingungen
        System.Diagnostics.Debug.WriteLine($"🎯 [QRCodeHelper] CreateTreeViewQRCodePanel called");
    System.Diagnostics.Debug.WriteLine($"   HubService available: {_hubService != null}");
        System.Diagnostics.Debug.WriteLine($"   Hub registered: {_hubService?.IsRegisteredWithHub}");
        System.Diagnostics.Debug.WriteLine($"   Tournament ID: {_tournamentId ?? "null"}");  // ⭐ NEU
    System.Diagnostics.Debug.WriteLine($"   Match UUID: {match.UniqueId ?? "null"}");
  
        if (_hubService == null)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [QRCodeHelper] No HubService available");
        return null;
}
        
        if (!_hubService.IsRegisteredWithHub)
   {
    System.Diagnostics.Debug.WriteLine($"❌ [QRCodeHelper] Tournament not registered with hub");
 return null;
        }
        
  if (string.IsNullOrEmpty(match.UniqueId))
      {
  System.Diagnostics.Debug.WriteLine($"❌ [QRCodeHelper] Match UUID is empty for match {match.Id}");
       return null;
        }

        var qrPanel = new StackPanel
        {
       Orientation = Orientation.Vertical,
       HorizontalAlignment = HorizontalAlignment.Right,
   VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 4, 4, 0)
        };

        // QR-Code Image
        var qrImage = new System.Windows.Controls.Image
      {
            Width = 60,
Height = 60,
      Margin = new Thickness(0, 0, 0, 2),
     ToolTip = "QR-Code zum Match scannen - öffnet dart-scoring.html"
        };

      var qrBinding = new Binding(".")
        {
            Source = match,
            Converter = new MatchToQRCodeConverter(_hubService, _localizationService, _tournamentId)  // ⭐ Tournament-ID übergeben
   };
        qrImage.SetBinding(System.Windows.Controls.Image.SourceProperty, qrBinding);

        qrPanel.Children.Add(qrImage);
      
        System.Diagnostics.Debug.WriteLine($"✅ [QRCodeHelper] TreeView QR panel created for match {match.Id}");
      
        return qrPanel;
    }
}

/// <summary>
/// Converter für Match zu QR-Code Konvertierung
/// UPDATED: Verwendet neue dart-scoring.html URL mit Match-UUID UND Tournament-ID Parameter
/// FIXED: Verbesserte Debug-Ausgaben und Fehlerbehandlung
/// </summary>
public class MatchToQRCodeConverter : IValueConverter
{
    private readonly HubIntegrationService? _hubService;
    private readonly LocalizationService? _localizationService;
    private readonly string? _tournamentId;  // ⭐ NEU

    public MatchToQRCodeConverter(HubIntegrationService? hubService, LocalizationService? localizationService, string? tournamentId = null)
    {
        _hubService = hubService;
        _localizationService = localizationService;
        _tournamentId = tournamentId;  // ⭐ NEU
    }

    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
 {
        try
      {
            string? matchUuid = null;
            int matchId = 0;
      
   if (value is Match match)
            {
       matchUuid = match.UniqueId;
        matchId = match.Id;
            }
       else if (value is KnockoutMatch knockoutMatch)
 {
                matchUuid = knockoutMatch.UniqueId;
         matchId = knockoutMatch.Id;
            }

   // ✅ IMPROVED DEBUG: Detaillierte Prüfung aller Bedingungen
            System.Diagnostics.Debug.WriteLine($"🎯 [QR-Converter] Convert called for match {matchId} (Type: {value?.GetType().Name})");
            System.Diagnostics.Debug.WriteLine($"   Match UUID: {matchUuid ?? "null"}");
   System.Diagnostics.Debug.WriteLine($"   Tournament ID: {_tournamentId ?? "null"}");
   System.Diagnostics.Debug.WriteLine($"   HubService available: {_hubService != null}");
      
      if (_hubService != null)
 {
                System.Diagnostics.Debug.WriteLine($"   Hub registered: {_hubService.IsRegisteredWithHub}");
            }

            if (string.IsNullOrEmpty(matchUuid))
          {
          System.Diagnostics.Debug.WriteLine($"❌ [QR-Converter] Match UUID is empty for match {matchId}");
  return null;
            }

  if (_hubService == null)
        {
       System.Diagnostics.Debug.WriteLine($"❌ [QR-Converter] HubService is null");
      return null;
    }

       if (!_hubService.IsRegisteredWithHub)
            {
         System.Diagnostics.Debug.WriteLine($"❌ [QR-Converter] Tournament not registered with hub");
return null;
 }

       // ✅ FIXED: NEW URL FORMAT mit Tournament-ID UND Match-UUID
    string dartScoringUrl;
      if (!string.IsNullOrEmpty(_tournamentId))
            {
      // Mit Tournament-ID (empfohlen)
           dartScoringUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?tournament={_tournamentId}&match={matchUuid}&uuid=true";
        System.Diagnostics.Debug.WriteLine($"🎯 [QR-Converter] ✅ Generated dart-scoring URL with Tournament ID: {dartScoringUrl}");
       }
            else
        {
    // Fallback: Ohne Tournament-ID (legacy)
           dartScoringUrl = $"https://dtp.i3ull3t.de:9443/dart-scoring.html?match={matchUuid}&uuid=true";
System.Diagnostics.Debug.WriteLine($"🎯 [QR-Converter] ⚠️ Generated dart-scoring URL WITHOUT Tournament ID (legacy): {dartScoringUrl}");
            }

   var qrImage = GenerateQRCodeImage(dartScoringUrl);
            
     if (qrImage != null)
      {
        System.Diagnostics.Debug.WriteLine($"✅ [QR-Converter] QR code generated successfully for match {matchId}");
         }
  else
            {
   System.Diagnostics.Debug.WriteLine($"❌ [QR-Converter] Failed to generate QR code for match {matchId}");
     }
            
         return qrImage;
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
        // Höher aufgelöste QR Codes für DataGrid
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