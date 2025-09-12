using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für UI-spezifische Funktionen im TournamentTab
/// Enthält Methoden zur Erstellung und Verwaltung von UI-Elementen
/// </summary>
public static class TournamentUIHelper
{
    /// <summary>
    /// Erstellt ein Knockout-Match-Control für die Turnierbaum-Darstellung
    /// </summary>
    /// <param name="match">Das Knockout-Match</param>
    /// <param name="width">Breite des Controls</param>
    /// <param name="height">Höhe des Controls</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>Border-Element mit dem Match-Control</returns>
    public static Border CreateKnockoutMatchControl(KnockoutMatch match, double width, double height, LocalizationService? localizationService = null)
    {
        var border = new Border
        {
            Width = width,
            Height = height,
            BorderBrush = Brushes.DarkSlateGray,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(3),
            Effect = new DropShadowEffect
            {
                Color = Colors.Gray,
                Direction = 315,
                ShadowDepth = 3,
                Opacity = 0.5
            }
        };

        // Set background color and border based on match status
        switch (match.Status)
        {
            case MatchStatus.NotStarted:
                border.Background = Brushes.WhiteSmoke;
                border.BorderBrush = Brushes.Silver;
                break;
            case MatchStatus.InProgress:
                border.Background = new SolidColorBrush(Color.FromRgb(255, 255, 200));
                border.BorderBrush = Brushes.Orange;
                break;
            case MatchStatus.Finished:
                border.Background = new SolidColorBrush(Color.FromRgb(200, 255, 200));
                border.BorderBrush = Brushes.Green;
                break;
            case MatchStatus.Bye:
                border.Background = new SolidColorBrush(Color.FromRgb(200, 220, 255));
                border.BorderBrush = Brushes.RoyalBlue;
                break;
            default:
                border.Background = Brushes.White;
                break;
        }

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
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 2)
        };

        // Player names with winner highlighting
        var tbdText = localizationService?.GetString("TBD") ?? "TBD";
        var vsText = localizationService?.GetString("Versus") ?? "vs";
        
        var player1Text = new TextBlock
        {
            Text = match.Player1?.Name ?? tbdText,
            FontSize = 11,
            FontWeight = match.Winner?.Id == match.Player1?.Id ? FontWeights.Bold : FontWeights.Normal,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player1?.Id 
                ? Brushes.DarkGreen 
                : Brushes.Black
        };

        var vsTextBlock = new TextBlock
        {
            Text = vsText,
            FontSize = 9,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 2, 0, 2),
            FontStyle = FontStyles.Italic,
            Foreground = Brushes.Gray
        };

        var player2Text = new TextBlock
        {
            Text = match.Player2?.Name ?? tbdText,
            FontSize = 11,
            FontWeight = match.Winner?.Id == match.Player2?.Id ? FontWeights.Bold : FontWeights.Normal,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player2?.Id 
                ? Brushes.DarkGreen 
                : Brushes.Black
        };

        // Score display with better styling
        var scoreText = new TextBlock
        {
            Text = match.Status == MatchStatus.NotStarted ? "--" : match.ScoreDisplay,
            FontSize = 10,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = Brushes.DarkBlue,
            Margin = new Thickness(0, 3, 0, 0)
        };

        stackPanel.Children.Add(matchIdText);
        stackPanel.Children.Add(player1Text);
        stackPanel.Children.Add(vsTextBlock);
        stackPanel.Children.Add(player2Text);
        stackPanel.Children.Add(scoreText);

        border.Child = stackPanel;

        // Enhanced tooltip with more match details
        var tooltipText = $"{localizationService?.GetString("Match") ?? "Match"} {match.Id} - {match.RoundDisplay}\n" +
                         $"{localizationService?.GetString("Status")}: {match.StatusDisplay}\n" +
                         $"{localizationService?.GetString("Player")} 1: {match.Player1?.Name ?? tbdText}\n" +
                         $"{localizationService?.GetString("Player")} 2: {match.Player2?.Name ?? tbdText}";
        
        if (match.Status == MatchStatus.Finished && match.Winner != null)
        {
            tooltipText += $"\n🏆 {localizationService?.GetString("Winner")}: {match.Winner.Name}";
        }

        border.ToolTip = tooltipText;

        return border;
    }

    /// <summary>
    /// Zeichnet eine Verbindungslinie zwischen Match-Controls im Turnierbaum
    /// </summary>
    /// <param name="canvas">Canvas auf dem gezeichnet werden soll</param>
    /// <param name="x1">X-Startposition</param>
    /// <param name="y1">Y-Startposition</param>
    /// <param name="x2">X-Endposition</param>
    /// <param name="y2">Y-Endposition</param>
    public static void DrawBracketConnectionLine(Canvas canvas, double x1, double y1, double x2, double y2)
    {
        var line = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = new SolidColorBrush(Color.FromRgb(70, 130, 180)),
            StrokeThickness = 2,
            Opacity = 0.7
        };

        // Create subtle dashed line effect
        line.StrokeDashArray = new System.Windows.Media.DoubleCollection(new double[] { 5, 3 });

        // Add subtle glow effect
        line.Effect = new DropShadowEffect
        {
            Color = Colors.SteelBlue,
            Direction = 0,
            ShadowDepth = 0,
            BlurRadius = 2,
            Opacity = 0.3
        };

        canvas.Children.Add(line);
    }

    /// <summary>
    /// Erstellt eine leere Nachricht für den Turnierbaum wenn keine Matches vorhanden sind
    /// </summary>
    /// <param name="canvas">Canvas auf dem die Nachricht angezeigt werden soll</param>
    /// <param name="message">Die anzuzeigende Nachricht</param>
    /// <param name="isLoserBracket">Ob es sich um das Loser Bracket handelt</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    public static void DrawEmptyBracketMessage(Canvas canvas, string message, bool isLoserBracket, LocalizationService? localizationService = null)
    {
        canvas.MinWidth = 800;
        canvas.MinHeight = 600;
        canvas.Background = GetThemeBrush("BackgroundBrush", Brushes.White);

        var messagePanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var icon = new TextBlock
        {
            Text = isLoserBracket ? "🥈" : "🏆",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var messageText = new TextBlock
        {
            Text = message,
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = GetThemeBrush("SecondaryTextBrush", Brushes.DarkGray)
        };

        var subText = new TextBlock
        {
            Text = localizationService?.GetString("TournamentTreeWillShow") ?? "Der Turnierbaum wird angezeigt sobald die KO-Phase beginnt",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = GetThemeBrush("SecondaryTextBrush", Brushes.Gray),
            Margin = new Thickness(0, 10, 0, 0)
        };

        messagePanel.Children.Add(icon);
        messagePanel.Children.Add(messageText);
        messagePanel.Children.Add(subText);

        Canvas.SetLeft(messagePanel, 250);
        Canvas.SetTop(messagePanel, 200);
        canvas.Children.Add(messagePanel);
    }

    /// <summary>
    /// Hilfsmethode um Theme-Ressourcen zu holen
    /// </summary>
    private static object? GetThemeResource(string resourceKey)
    {
        try
        {
            return Application.Current?.Resources[resourceKey];
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Hilfsmethode um Brush aus Theme-Ressourcen zu holen
    /// </summary>
    private static Brush GetThemeBrush(string resourceKey, Brush fallback)
    {
        return GetThemeResource(resourceKey) as Brush ?? fallback;
    }

    /// <summary>
    /// Hilfsmethode: Findet das TextBlock-Element im Header eines TabItems
    /// </summary>
    /// <param name="tabItem">Das TabItem dessen Header durchsucht werden soll</param>
    /// <returns>Das TextBlock-Element oder null wenn nicht gefunden</returns>
    public static TextBlock? FindTextBlockInHeader(TabItem tabItem)
    {
        if (tabItem.Header is StackPanel stackPanel)
        {
            return stackPanel.Children.OfType<TextBlock>().FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// Zeigt eine dezente Toast-Benachrichtigung in einem übergeordneten Grid
    /// </summary>
    /// <param name="parentGrid">Das übergeordnete Grid</param>
    /// <param name="title">Titel der Benachrichtigung</param>
    /// <param name="message">Nachricht der Benachrichtigung</param>
    public static void ShowToastNotification(Grid parentGrid, string title, string message)
    {
        try
        {
            // Erstelle ein einfaches Toast-Panel
            var toastPanel = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(230, 76, 175, 80)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                MaxWidth = 400,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    Direction = 270,
                    ShadowDepth = 4,
                    BlurRadius = 8,
                    Opacity = 0.3
                }
            };
            
            var stackPanel = new StackPanel();
            
            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 4)
            };
            
            var messageBlock = new TextBlock
            {
                Text = message,
                FontSize = 12,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap
            };
            
            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(messageBlock);
            toastPanel.Child = stackPanel;
            
            // Füge Toast zum Grid hinzu
            Grid.SetRowSpan(toastPanel, parentGrid.RowDefinitions.Count > 0 ? parentGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(toastPanel, parentGrid.ColumnDefinitions.Count > 0 ? parentGrid.ColumnDefinitions.Count : 1);
            parentGrid.Children.Add(toastPanel);
            
            // Auto-remove nach 4 Sekunden mit Timer
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                parentGrid.Children.Remove(toastPanel);
            };
            timer.Start();
            
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowToastNotification: ERROR: {ex.Message}");
            // Fallback zu normalem MessageBox
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}