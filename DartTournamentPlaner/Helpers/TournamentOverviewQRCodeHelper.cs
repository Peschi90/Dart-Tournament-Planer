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
/// </summary>
public class TournamentOverviewQRCodeHelper
{
    private readonly HubIntegrationService? _hubService;
    private readonly LocalizationService _localizationService;
    private readonly Action<object, RoutedEventArgs> _onOpenMatchPageClick;

    public TournamentOverviewQRCodeHelper(
        HubIntegrationService? hubService, 
        LocalizationService localizationService,
        Action<object, RoutedEventArgs> onOpenMatchPageClick)
    {
        _hubService = hubService;
        _localizationService = localizationService;
        _onOpenMatchPageClick = onOpenMatchPageClick;
    }

    /// <summary>
    /// Erstellt DataTemplate für QR-Code Zellen in DataGrid
    /// </summary>
    public DataTemplate CreateQRCodeCellTemplate()
    {
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
        imageFactory.SetValue(System.Windows.Controls.Image.ToolTipProperty, "QR-Code zum Match scannen");
        
        // QR-Code generieren und als Source setzen
        var binding = new Binding(".");
        var converter = new MatchToQRCodeConverter(_hubService, _localizationService);
        binding.Converter = converter;
        imageFactory.SetBinding(System.Windows.Controls.Image.SourceProperty, binding);
        
        // Button nur wenn Hub registriert ist
        if (_hubService != null && _hubService.IsRegisteredWithHub)
        {
            // Button für direktes Öffnen der Match-Page
            var buttonFactory = new FrameworkElementFactory(typeof(Button));
            buttonFactory.SetValue(Button.ContentProperty, "🌐");
            buttonFactory.SetValue(Button.WidthProperty, 40.0);
            buttonFactory.SetValue(Button.HeightProperty, 40.0);
            buttonFactory.SetValue(Button.FontSizeProperty, 16.0);
            buttonFactory.SetValue(Button.MarginProperty, new Thickness(8, 0, 0, 0));
            buttonFactory.SetValue(Button.ToolTipProperty, "Match-Page im Browser öffnen");
            buttonFactory.SetValue(Button.CursorProperty, System.Windows.Input.Cursors.Hand);
            
            // Event für Button-Click
            buttonFactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(_onOpenMatchPageClick));
            
            //stackPanelFactory.AppendChild(buttonFactory);
        }
        
        stackPanelFactory.AppendChild(imageFactory);
        
        template.VisualTree = stackPanelFactory;
        return template;
    }

    /// <summary>
    /// Erstellt QR-Code und Web-Button für Tournament Tree View Match Controls
    /// </summary>
    public UIElement? CreateTreeViewQRCodePanel(KnockoutMatch match)
    {
        if (_hubService == null || !_hubService.IsRegisteredWithHub || string.IsNullOrEmpty(match.UniqueId))
            return null;

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
            Margin = new Thickness(0, 0, 0, 2)
        };

        var qrBinding = new Binding(".")
        {
            Source = match,
            Converter = new MatchToQRCodeConverter(_hubService, _localizationService)
        };
        qrImage.SetBinding(System.Windows.Controls.Image.SourceProperty, qrBinding);

        qrPanel.Children.Add(qrImage);
        
        return qrPanel;
    }
}

/// <summary>
/// Converter für Match zu QR-Code Konvertierung
/// </summary>
public class MatchToQRCodeConverter : IValueConverter
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
                return null;
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