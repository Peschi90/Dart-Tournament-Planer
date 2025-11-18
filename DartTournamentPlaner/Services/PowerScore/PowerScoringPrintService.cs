using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DartTournamentPlaner.Models.PowerScore;
using QRCoder;

namespace DartTournamentPlaner.Services.PowerScore;

/// <summary>
/// Service zum Drucken von PowerScoring QR-Codes
/// </summary>
public class PowerScoringPrintService
{
    /// <summary>
    /// Druckt QR-Codes für alle Spieler
    /// </summary>
    public bool PrintQRCodes(List<PowerScoringPlayer> players, string? tournamentId = null)
    {
        try
        {
            var printDialog = new System.Windows.Controls.PrintDialog();
            
            if (printDialog.ShowDialog() == true)
            {
                // Erstelle Dokument
                var document = CreatePrintDocument(players, tournamentId);
                
                // Drucke
                printDialog.PrintDocument(document.DocumentPaginator, "PowerScoring QR Codes");
                
                System.Diagnostics.Debug.WriteLine($"??? Printed QR codes for {players.Count} players");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error printing QR codes: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Erstellt das Print-Dokument mit QR-Codes
    /// </summary>
    private FixedDocument CreatePrintDocument(List<PowerScoringPlayer> players, string? tournamentId)
    {
        var document = new FixedDocument();
        document.DocumentPaginator.PageSize = new Size(96 * 8.5, 96 * 11); // A4 Letter

        // 4 QR-Codes pro Seite (2x2)
        var playersPerPage = 4;
        var pages = (int)Math.Ceiling((double)players.Count / playersPerPage);

        for (int pageNum = 0; pageNum < pages; pageNum++)
        {
            var startIndex = pageNum * playersPerPage;
            var endIndex = Math.Min(startIndex + playersPerPage, players.Count);
            var pagePlayers = players.GetRange(startIndex, endIndex - startIndex);

            var page = CreatePage(pagePlayers, tournamentId, pageNum + 1, pages);
            document.Pages.Add(page);
        }

        return document;
    }

    /// <summary>
    /// Erstellt eine einzelne Seite mit QR-Codes
    /// </summary>
    private PageContent CreatePage(List<PowerScoringPlayer> players, string? tournamentId, int pageNum, int totalPages)
    {
        var page = new FixedPage
        {
            Width = 96 * 8.5,
            Height = 96 * 11,
            Background = Brushes.White
        };

        // Header
        var header = CreateHeader(tournamentId, pageNum, totalPages);
        FixedPage.SetTop(header, 30);
        FixedPage.SetLeft(header, 30);
        page.Children.Add(header);

        // Grid für QR-Codes (2x2)
        var grid = new Grid
        {
            Width = page.Width - 60,
            Height = page.Height - 120,
            Margin = new Thickness(30, 100, 30, 30)
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Füge QR-Codes hinzu
        for (int i = 0; i < players.Count; i++)
        {
            var qrCodePanel = CreateQRCodePanel(players[i]);
            Grid.SetRow(qrCodePanel, i / 2);
            Grid.SetColumn(qrCodePanel, i % 2);
            grid.Children.Add(qrCodePanel);
        }

        page.Children.Add(grid);

        var pageContent = new PageContent();
        ((System.Windows.Markup.IAddChild)pageContent).AddChild(page);
        return pageContent;
    }

    /// <summary>
    /// Erstellt den Header für die Seite
    /// </summary>
    private Border CreateHeader(string? tournamentId, int pageNum, int totalPages)
    {
        var border = new Border
        {
            Width = 96 * 8.5 - 60,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 0, 0, 2),
            Padding = new Thickness(0, 0, 0, 10)
        };

        var stack = new StackPanel();

        var title = new TextBlock
        {
            Text = "PowerScoring QR Codes",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.Black
        };
        stack.Children.Add(title);

        if (!string.IsNullOrEmpty(tournamentId))
        {
            var tournamentInfo = new TextBlock
            {
                Text = $"Tournament: {tournamentId}",
                FontSize = 14,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 5, 0, 0)
            };
            stack.Children.Add(tournamentInfo);
        }

        var pageInfo = new TextBlock
        {
            Text = $"Page {pageNum} of {totalPages}",
            FontSize = 12,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 5, 0, 0)
        };
        stack.Children.Add(pageInfo);

        border.Child = stack;
        return border;
    }

    /// <summary>
    /// Erstellt ein QR-Code Panel für einen Spieler
    /// </summary>
    private Border CreateQRCodePanel(PowerScoringPlayer player)
    {
        var border = new Border
        {
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(10),
            Padding = new Thickness(20),
            Background = Brushes.White
        };

        var stack = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // Spielername
        var nameText = new TextBlock
        {
            Text = player.Name,
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.Black,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15)
        };
        stack.Children.Add(nameText);

        // QR-Code
        if (!string.IsNullOrEmpty(player.QrCodeUrl))
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(player.QrCodeUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                
                var bitmapImage = new BitmapImage();
                using (var stream = new MemoryStream(qrCodeBytes))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }

                var qrImage = new Image
                {
                    Source = bitmapImage,
                    Width = 250,
                    Height = 250,
                    Stretch = Stretch.Uniform
                };
                stack.Children.Add(qrImage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Error generating QR code for print: {ex.Message}");
                
                var errorText = new TextBlock
                {
                    Text = "QR Code Error",
                    FontSize = 14,
                    Foreground = Brushes.Red,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                stack.Children.Add(errorText);
            }
        }

        // URL (klein gedruckt)
        if (!string.IsNullOrEmpty(player.QrCodeUrl))
        {
            var urlText = new TextBlock
            {
                Text = player.QrCodeUrl,
                FontSize = 8,
                Foreground = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 10, 0, 0),
                MaxWidth = 250
            };
            stack.Children.Add(urlText);
        }

        // Anleitung
        var instructionText = new TextBlock
        {
            Text = "Scan this QR code to enter your scores",
            FontSize = 10,
            Foreground = Brushes.DarkGray,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        stack.Children.Add(instructionText);

        border.Child = stack;
        return border;
    }
}
