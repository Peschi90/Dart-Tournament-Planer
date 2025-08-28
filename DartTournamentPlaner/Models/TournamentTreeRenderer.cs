using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Verantwortlich für das Rendering von interaktiven Turnierbäumen
/// Erstellt Canvas-basierte UI-Elemente für K.O.-Turniere
/// </summary>
public class TournamentTreeRenderer
{
    private readonly TournamentClass _tournament;

    public TournamentTreeRenderer(TournamentClass tournament)
    {
        _tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
    }

    /// <summary>
    /// NEUE INTERAKTIVE METHODE: Erstellt eine interaktive Turnierbaum-Ansicht
    /// Direkt in das gegebene Canvas-Element eingebettet mit klickbaren Controls
    /// </summary>
    /// <param name="targetCanvas">Das Canvas, in das der Turnierbaum gerendert werden soll</param>
    /// <param name="isLoserBracket">True für Loser Bracket, False für Winner Bracket</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>Das gerenderte FrameworkElement</returns>
    public FrameworkElement? CreateTournamentTreeView(Canvas targetCanvas, bool isLoserBracket, LocalizationService? localizationService = null)
    {
        if (_tournament.CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            System.Diagnostics.Debug.WriteLine("CreateTournamentTreeView: Not in knockout phase");
            return null;
        }

        System.Diagnostics.Debug.WriteLine($"CreateTournamentTreeView: Creating interactive tree for {(isLoserBracket ? "Loser" : "Winner")} Bracket");

        try
        {
            targetCanvas.Children.Clear();

            var matches = isLoserBracket ? _tournament.CurrentPhase.LoserBracket.ToList() : _tournament.CurrentPhase.WinnerBracket.ToList();

            if (matches.Count == 0)
            {
                CreateEmptyBracketMessage(targetCanvas, isLoserBracket, localizationService);
                return targetCanvas;
            }

            // Set canvas properties
            targetCanvas.Background = System.Windows.Media.Brushes.White; // Weißer Hintergrund anstatt Gradient
            targetCanvas.MinWidth = 1200;
            targetCanvas.MinHeight = 800;

            // Add title
            CreateBracketTitle(targetCanvas, isLoserBracket, localizationService);

            // Group matches by round for layout
            var matchesByRound = matches
                .GroupBy(m => m.Round)
                .OrderBy(g => GetRoundOrderValue(g.Key, isLoserBracket))
                .ToList();

            double roundWidth = 250;
            double matchHeight = 80;
            double matchSpacing = 30;
            double roundSpacing = 60;

            // Create interactive match controls
            for (int roundIndex = 0; roundIndex < matchesByRound.Count; roundIndex++)
            {
                var roundGroup = matchesByRound[roundIndex];
                var roundMatches = roundGroup.OrderBy(m => m.Position).ToList();

                double xPos = roundIndex * (roundWidth + roundSpacing) + 50;
                double startY = 100;

                // Calculate vertical spacing to center matches
                double totalRoundHeight = roundMatches.Count * matchHeight + (roundMatches.Count - 1) * matchSpacing;
                double roundStartY = Math.Max(startY, (targetCanvas.MinHeight - totalRoundHeight) / 2);

                // Create round label
                CreateRoundLabel(targetCanvas, xPos, roundMatches.First(), roundWidth, isLoserBracket, localizationService);

                // Create match controls
                for (int matchIndex = 0; matchIndex < roundMatches.Count; matchIndex++)
                {
                    var match = roundMatches[matchIndex];
                    double yPos = roundStartY + matchIndex * (matchHeight + matchSpacing);

                    var matchControl = CreateInteractiveMatchControl(match, roundWidth - 30, matchHeight - 20, localizationService);
                    Canvas.SetLeft(matchControl, xPos);
                    Canvas.SetTop(matchControl, yPos);
                    targetCanvas.Children.Add(matchControl);

                    // Verbindungslinien entfernt - keine CreateConnectionLine mehr
                }
            }

            // Adjust canvas size based on content
            targetCanvas.Width = Math.Max(1200, matchesByRound.Count * (roundWidth + roundSpacing) + 200);
            targetCanvas.Height = Math.Max(800, matchesByRound.Max(r => r.Count()) * (matchHeight + matchSpacing) + 400);
            targetCanvas.MinWidth = targetCanvas.Width;
            targetCanvas.MinHeight = targetCanvas.Height;

            System.Diagnostics.Debug.WriteLine($"CreateTournamentTreeView: Successfully created interactive tree with {matches.Count} matches");
            return targetCanvas;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateTournamentTreeView: ERROR: {ex.Message}");
            return null;
        }
    }

    private void CreateEmptyBracketMessage(Canvas canvas, bool isLoserBracket, LocalizationService? localizationService)
    {
        var messagePanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var icon = new TextBlock
        {
            Text = isLoserBracket ? "🥈" : "🏆",
            FontSize = 60,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };

        var messageText = new TextBlock
        {
            Text = isLoserBracket 
                ? (localizationService?.GetString("NoLoserBracketMatches") ?? "Keine Loser Bracket Spiele vorhanden")
                : (localizationService?.GetString("NoWinnerBracketMatches") ?? "Keine Winner Bracket Spiele vorhanden"),
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.DarkGray
        };

        var subText = new TextBlock
        {
            Text = localizationService?.GetString("TournamentTreeWillShow") ?? "Der Turnierbaum wird angezeigt sobald die KO-Phase beginnt",
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Gray,
            Margin = new Thickness(0, 20, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400
        };

        messagePanel.Children.Add(icon);
        messagePanel.Children.Add(messageText);
        messagePanel.Children.Add(subText);

        Canvas.SetLeft(messagePanel, 350);
        Canvas.SetTop(messagePanel, 300);
        canvas.Children.Add(messagePanel);

        canvas.Background = System.Windows.Media.Brushes.White; // Weißer Hintergrund anstatt Gradient
    }

    private void CreateBracketTitle(Canvas canvas, bool isLoserBracket, LocalizationService? localizationService)
    {
        var titleText = new TextBlock
        {
            Text = isLoserBracket 
                ? (localizationService?.GetString("LoserBracket") ?? "🥈 Loser Bracket")
                : (localizationService?.GetString("WinnerBracket") ?? "🏆 Winner Bracket"),
            FontSize = 32,
            FontWeight = FontWeights.Bold,
            Foreground = isLoserBracket 
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 92, 92))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 139, 34)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Gray,
                Direction = 315,
                ShadowDepth = 2,
                BlurRadius = 4,
                Opacity = 0.5
            }
        };

        Canvas.SetLeft(titleText, 50);
        Canvas.SetTop(titleText, 20);
        canvas.Children.Add(titleText);
    }

    private void CreateRoundLabel(Canvas canvas, double xPos, KnockoutMatch sampleMatch, double roundWidth, bool isLoserBracket, LocalizationService? localizationService)
    {
        var roundLabelBorder = new Border
        {
            Background = isLoserBracket 
                ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 255, 182, 193))
                : new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 144, 238, 144)),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(20, 8, 20, 8),
            BorderBrush = isLoserBracket 
                ? System.Windows.Media.Brushes.IndianRed
                : System.Windows.Media.Brushes.ForestGreen,
            BorderThickness = new Thickness(3),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Gray,
                Direction = 315,
                ShadowDepth = 3,
                BlurRadius = 5,
                Opacity = 0.4
            }
        };

        var roundLabel = new TextBlock
        {
            Text = sampleMatch.RoundDisplay,
            FontWeight = FontWeights.Bold,
            FontSize = 18,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        roundLabelBorder.Child = roundLabel;
        Canvas.SetLeft(roundLabelBorder, xPos + (roundWidth - 180) / 2);
        Canvas.SetTop(roundLabelBorder, 70);
        canvas.Children.Add(roundLabelBorder);
    }

    private Border CreateInteractiveMatchControl(KnockoutMatch match, double width, double height, LocalizationService? localizationService)
    {
        var border = new Border
        {
            Width = width,
            Height = height,
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(5),
            Cursor = System.Windows.Input.Cursors.Hand,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Gray,
                Direction = 315,
                ShadowDepth = 4,
                BlurRadius = 6,
                Opacity = 0.6
            }
        };

        // Set background and border based on match status
        SetMatchControlAppearance(border, match);

        // Create content grid mit WENIGER Zeilen (Match ID entfernt)
        var contentGrid = new Grid
        {
            Background = System.Windows.Media.Brushes.Transparent 
        };
        // NUR 2 Zeilen: Spieler-Bereich und Score/Status DC
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Spieler bekommen mehr Platz
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Score bleibt unten

        // NEUER ANSATZ: Grid statt StackPanel für bessere Kontrolle über das Layout
        var playersGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8) // Mehr Margins da mehr Platz vorhanden
        };

        // Grid-Definitionen: 3 Spalten für Player1, vs, Player2 (statt Zeilen)
        playersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Player1 - nimmt verfügbaren Platz
        playersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // "vs" - nur so breit wie nötig
        playersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Player2 - nimmt verfügbaren Platz

        // Player 1 LINKS positioniert - in Spalte 0
        var player1Text = new TextBlock
        {
            Text = !string.IsNullOrEmpty(match.Player1?.Name) ? match.Player1.Name : "TBD",
            FontSize = 14,
            FontWeight = match.Winner?.Id == match.Player1?.Id ? FontWeights.Bold : FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Left,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player1?.Id
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 100, 0))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)),
            Margin = new Thickness(10, 2, 2, 0),
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(player1Text, 0); // Spalte 0 statt Zeile 0

        var vsText = new TextBlock
        {
            Text = "vs",
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontStyle = FontStyles.Italic,
            FontWeight = FontWeights.Normal,
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 128, 128)),
            Margin = new Thickness(8, 0, 8, 0), // Horizontaler Abstand statt vertikaler
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(vsText, 1); // Spalte 1 statt Zeile 1

        // Player 2 RECHTS positioniert - in Spalte 2
        var player2Text = new TextBlock
        {
            Text = !string.IsNullOrEmpty(match.Player2?.Name) ? match.Player2.Name : "TBD",
            FontSize = 14,
            FontWeight = match.Winner?.Id == match.Player2?.Id ? FontWeights.Bold : FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Right,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player2?.Id
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 100, 0))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)),
            Margin = new Thickness(2, 2, 10, 0),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(player2Text, 2); // Spalte 2 statt Zeile 2

        playersGrid.Children.Add(player1Text);
        playersGrid.Children.Add(vsText);
        playersGrid.Children.Add(player2Text);

        Grid.SetRow(playersGrid, 0); // Spieler bekommen die erste (größere) Zeile
        contentGrid.Children.Add(playersGrid);

        // Score/Status area - jetzt in Zeile 1 statt 2
        var scorePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6) // Mehr Margins für bessere Optik
        };

        // Score - auch etwas größer da mehr Platz
        var scoreText = new TextBlock
        {
            Text = match.Status == MatchStatus.NotStarted ? "--:--" : match.ScoreDisplay,
            FontSize = 13, // Etwas größer
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkBlue,
            Margin = new Thickness(0, 0, 10, 0) // Mehr Abstand zum Status-Indikator
        };

        // Status indicator - auch etwas größer
        var statusIndicator = new Ellipse
        {
            Width = 10, // Größer
            Height = 10, // Größer
            Margin = new Thickness(4, 0, 0, 0)
        };

        statusIndicator.Fill = match.Status switch
        {
            MatchStatus.NotStarted => System.Windows.Media.Brushes.Gray,
            MatchStatus.InProgress => System.Windows.Media.Brushes.Orange,
            MatchStatus.Finished => System.Windows.Media.Brushes.Green,
            MatchStatus.Bye => System.Windows.Media.Brushes.RoyalBlue,
            _ => System.Windows.Media.Brushes.Gray
        };

        scorePanel.Children.Add(scoreText);
        scorePanel.Children.Add(statusIndicator);

        Grid.SetRow(scorePanel, 1); // Jetzt Zeile 1 statt 2
        contentGrid.Children.Add(scorePanel);

        // WICHTIG: Das contentGrid muss dem border hinzugefügt werden!
        border.Child = contentGrid;

        // Add interactivity
        AddMatchInteractivity(border, match, localizationService);

        // Enhanced tooltip - Match ID bleibt nur im Tooltip
        CreateMatchTooltip(border, match, localizationService);

        return border;
    }

    private void SetMatchControlAppearance(Border border, KnockoutMatch match)
    {
        switch (match.Status)
        {
            case MatchStatus.NotStarted:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250));
                border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                border.BorderThickness = new Thickness(2);
                break;
            case MatchStatus.InProgress:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 248, 220));
                border.BorderBrush = System.Windows.Media.Brushes.Orange;
                border.BorderThickness = new Thickness(3);
                break;
            case MatchStatus.Finished:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 255, 240));
                border.BorderBrush = System.Windows.Media.Brushes.Green;
                border.BorderThickness = new Thickness(3);
                break;
            case MatchStatus.Bye:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 245, 255));
                border.BorderBrush = System.Windows.Media.Brushes.RoyalBlue;
                border.BorderThickness = new Thickness(3);
                break;
            default:
                border.Background = System.Windows.Media.Brushes.White;
                border.BorderBrush = System.Windows.Media.Brushes.Gray;
                border.BorderThickness = new Thickness(2);
                break;
        }
    }

    private void AddMatchInteractivity(Border border, KnockoutMatch match, LocalizationService? localizationService)
    {
        // Mouse enter/leave effects
        border.MouseEnter += (s, e) =>
        {
            var transform = new System.Windows.Media.ScaleTransform(1.05, 1.05);
            border.RenderTransformOrigin = new Point(0.5, 0.5);
            border.RenderTransform = transform;
            
            var dropShadow = (System.Windows.Media.Effects.DropShadowEffect?)border.Effect;
            if (dropShadow != null)
            {
                dropShadow.ShadowDepth = 6;
                dropShadow.BlurRadius = 8;
            }
        };

        border.MouseLeave += (s, e) =>
        {
            border.RenderTransform = null;
            
            var dropShadow = (System.Windows.Media.Effects.DropShadowEffect?)border.Effect;
            if (dropShadow != null)
            {
                dropShadow.ShadowDepth = 4;
                dropShadow.BlurRadius = 6;
            }
        };

        // Double-click to open match result dialog
        border.MouseLeftButtonDown += (s, e) =>
        {
            if (e.ClickCount == 2)
            {
                // WICHTIG: Verwende OpenMatchResultDialog mit rundenspezifischen Regeln
                OpenMatchResultDialog(match, localizationService);
            }
        };

        // Context menu for advanced options
        var contextMenu = CreateMatchContextMenu(match, localizationService);
        border.ContextMenu = contextMenu;
    }

    private void CreateMatchTooltip(Border border, KnockoutMatch match, LocalizationService? localizationService)
    {
        var tooltipText = $"Match {match.Id} - {match.RoundDisplay}\n" +
                         $"Status: {match.StatusDisplay}\n" +
                         $"Spieler 1: {match.Player1?.Name ?? "TBD"}\n" +
                         $"Spieler 2: {match.Player2?.Name ?? "TBD"}";

        if (match.Status == MatchStatus.Finished && match.Winner != null)
        {
            tooltipText += $"\n🏆 Sieger: {match.Winner.Name}";
        }
        else if (match.Status == MatchStatus.Bye && match.Winner != null)
        {
            tooltipText += $"\n🎯 Freilos: {match.Winner.Name}";
        }

        tooltipText += "\n\n🖱️ Doppelklick: Ergebnis eingeben";
        tooltipText += "\n🖱️ Rechtsklick: Weitere Optionen";

        border.ToolTip = tooltipText;
    }

    private ContextMenu CreateMatchContextMenu(KnockoutMatch match, LocalizationService? localizationService)
    {
        var contextMenu = new ContextMenu();
        var byeMatchManager = new ByeMatchManager(_tournament);

        // Enter/Edit result
        if (match.Status != MatchStatus.Bye && match.Player1 != null && match.Player2 != null)
        {
            var resultMenuItem = new MenuItem
            {
                Header = match.Status == MatchStatus.Finished 
                    ? (localizationService?.GetString("EditResult") ?? "Ergebnis bearbeiten")
                    : (localizationService?.GetString("EnterResult") ?? "Ergebnis eingeben"),
                Icon = new TextBlock { Text = "📝", FontSize = 12 }
            };
            // WICHTIG: Verwende OpenMatchResultDialog mit rundenspezifischen Regeln
            resultMenuItem.Click += (s, e) => OpenMatchResultDialog(match, localizationService);
            contextMenu.Items.Add(resultMenuItem);
        }

        // Bye options
        if (match.Status == MatchStatus.NotStarted || match.Status == MatchStatus.Bye)
        {
            var validation = byeMatchManager.ValidateByeOperation(match);
            
            if (validation.CanGiveBye)
            {
                // Give bye to specific player
                if (match.Player1 != null)
                {
                    var byePlayer1 = new MenuItem
                    {
                        Header = $"Freilos an {match.Player1.Name}",
                        Icon = new TextBlock { Text = "🎯", FontSize = 12 }
                    };
                    byePlayer1.Click += (s, e) => byeMatchManager.GiveManualBye(match, match.Player1);
                    contextMenu.Items.Add(byePlayer1);
                }

                if (match.Player2 != null)
                {
                    var byePlayer2 = new MenuItem
                    {
                        Header = $"Freilos an {match.Player2.Name}",
                        Icon = new TextBlock { Text = "🎯", FontSize = 12 }
                    };
                    byePlayer2.Click += (s, e) => byeMatchManager.GiveManualBye(match, match.Player2);
                    contextMenu.Items.Add(byePlayer2);
                }

                // Auto bye
                var autoBye = new MenuItem
                {
                    Header = "Automatisches Freilos",
                    Icon = new TextBlock { Text = "🤖", FontSize = 12 }
                };
                autoBye.Click += (s, e) => byeMatchManager.GiveManualBye(match, null);
                contextMenu.Items.Add(autoBye);
            }

            if (validation.CanUndoBye)
            {
                var undoBye = new MenuItem
                {
                    Header = "Freilos rückgängigmachen",
                    Icon = new TextBlock { Text = "↩️", FontSize = 12 }
                };
                undoBye.Click += (s, e) => byeMatchManager.UndoBye(match);
                contextMenu.Items.Add(undoBye);
            }
        }

        // Show info if no actions possible
        if (contextMenu.Items.Count == 0)
        {
            var noActionItem = new MenuItem
            {
                Header = "Keine Aktionen verfügbar",
                IsEnabled = false,
                Icon = new TextBlock { Text = "ℹ️", FontSize = 12 }
            };
            contextMenu.Items.Add(noActionItem);
        }

        return contextMenu;
    }

    private void OpenMatchResultDialog(KnockoutMatch match, LocalizationService? localizationService)
    {
        if (match.Player1 == null || match.Player2 == null || match.Status == MatchStatus.Bye)
            return;

        try
        {
            // WICHTIG: Verwende rundenspezifische Regeln für KO-Matches
            var roundRules = _tournament.GameRules.GetRulesForRound(match.Round);
            
            System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog: Match {match.Id} in {match.Round}");
            System.Diagnostics.Debug.WriteLine($"  Round Rules: SetsToWin={roundRules.SetsToWin}, LegsToWin={roundRules.LegsToWin}, LegsPerSet={roundRules.LegsPerSet}");
            System.Diagnostics.Debug.WriteLine($"  Using SPECIALIZED constructor for KnockoutMatch");

            // KORREKTUR: Verwende den spezialisierten Constructor für KnockoutMatches
            var resultWindow = new MatchResultWindow(match, roundRules, _tournament.GameRules, localizationService);
            
            // Try to find parent window
            var parentWindow = Application.Current.MainWindow;
            if (parentWindow != null)
            {
                resultWindow.Owner = parentWindow;
            }

            if (resultWindow.ShowDialog() == true)
            {
                var internalMatch = resultWindow.InternalMatch;
                match.Player1Sets = internalMatch.Player1Sets;
                match.Player2Sets = internalMatch.Player2Sets;
                match.Player1Legs = internalMatch.Player1Legs;
                match.Player2Legs = internalMatch.Player2Legs;
                match.Winner = internalMatch.Winner;
                match.Status = internalMatch.Status;
                match.Notes = internalMatch.Notes;
                match.StartTime = internalMatch.StartTime;
                match.EndTime = DateTime.Now;

                System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog: Match {match.Id} result saved");
                System.Diagnostics.Debug.WriteLine($"  Winner: {match.Winner?.Name}, Sets: {match.Player1Sets}:{match.Player2Sets}, Legs: {match.Player1Legs}:{match.Player2Legs}");

                // Process the result through the tournament system
                _tournament.ProcessMatchResult(match);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog: ERROR: {ex.Message}");
            MessageBox.Show($"Fehler beim Öffnen des Ergebnis-Fensters: {ex.Message}", 
                           "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
}