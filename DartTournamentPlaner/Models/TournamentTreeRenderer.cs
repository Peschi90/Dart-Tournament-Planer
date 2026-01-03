using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License; // ✅ FIX: Add License services import
using DartTournamentPlaner.Views;
using DartTournamentPlaner.Helpers; // ✅ NEW: Zugriff auf MainWindowServiceInitializer

namespace DartTournamentPlaner.Models;

/// <summary>
/// Verantwortlich für das Rendering von interaktiven Turnierbäumen
/// Erstellt Canvas-basierte UI-Elemente für K.O.-Turniere mit Theme-Unterstützung
/// </summary>
public class TournamentTreeRenderer
{
    private readonly TournamentClass _tournament;
    // ✅ NEU: Speichere Referenzen zu aktiven Canvas-Elementen für Live-Updates
    private Canvas? _currentWinnerBracketCanvas;
    private Canvas? _currentLoserBracketCanvas;
 // ✅ NEU: Statische Instanz für globalen Zugriff (z.B. von MainWindow)
    public static TournamentTreeRenderer? CurrentInstance { get; private set; }

    public TournamentTreeRenderer(TournamentClass tournament)
    {
    _tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
    CurrentInstance = this; // Setze statische Referenz
    }
    
    /// <summary>
    /// ✅ NEU: Propagiert Match-Ergebnisse aus dem Tournament Tree mit spezieller Grand Final Logik
    /// </summary>
    private void PropagateMatchResultFromTree(KnockoutMatch match)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"🔄 [PropagateFromTree] Starting for match {match.Id}, Winner: {match.Winner?.Name}, Loser: {match.Loser?.Name}, Round: {match.Round}");
            
            var currentPhase = _tournament.CurrentPhase;
            if (currentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine("❌ Not in KnockoutPhase!");
                return;
            }
            
            if (match.Winner == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No winner set - skipping propagation");
                return;
            }
            
            // ✅ SPEZIALFALL: Loser Final Winner geht ins Grand Final!
            if (match.Round == KnockoutRound.LoserFinal)
            {
                System.Diagnostics.Debug.WriteLine($"🏆 [SPECIAL] Loser Final Winner {match.Winner.Name} should go to Grand Final!");
                
                // Finde das Grand Final (immer im Winner Bracket)
                var grandFinal = currentPhase.WinnerBracket.FirstOrDefault(m => m.Round == KnockoutRound.GrandFinal);
                if (grandFinal != null)
                {
                    // Loser Final Winner ist immer Player2 im Grand Final
                    grandFinal.Player2 = match.Winner;
                    System.Diagnostics.Debug.WriteLine($"✅✅✅ Set {match.Winner.Name} as Player2 in Grand Final (from Loser Final)");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Grand Final not found!");
                }
            }
            
            // Normale Propagierung für alle anderen Matches
            // Winner Bracket
            foreach (var nextMatch in currentPhase.WinnerBracket)
            {
                bool updated = false;
                
                if (nextMatch.SourceMatch1 == match)
                {
                    if (nextMatch.Player1FromWinner)
                    {
                        nextMatch.Player1 = match.Winner;
                        updated = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Set Player1 of match {nextMatch.Id} to {match.Winner.Name} (from Winner)");
                    }
                    else if (match.Loser != null)
                    {
                        nextMatch.Player1 = match.Loser;
                        updated = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Set Player1 of match {nextMatch.Id} to {match.Loser.Name} (from Loser)");
                    }
                }
                
                if (nextMatch.SourceMatch2 == match)
                {
                    if (nextMatch.Player2FromWinner)
                    {
                        nextMatch.Player2 = match.Winner;
                        updated = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Set Player2 of match {nextMatch.Id} to {match.Winner.Name} (from Winner)");
                    }
                    else if (match.Loser != null)
                    {
                        nextMatch.Player2 = match.Loser;
                        updated = true;
                        System.Diagnostics.Debug.WriteLine($"✅ Set Player2 of match {nextMatch.Id} to {match.Loser.Name} (from Loser)");
                    }
                }
                
                if (updated)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 Updated match {nextMatch.Id}: {nextMatch.Player1?.Name ?? "TBD"} vs {nextMatch.Player2?.Name ?? "TBD"}");
                }
            }
            
            // Loser Bracket
            if (currentPhase.LoserBracket != null)
            {
                foreach (var nextMatch in currentPhase.LoserBracket)
                {
                    bool updated = false;
                    
                    if (nextMatch.SourceMatch1 == match)
                    {
                        if (nextMatch.Player1FromWinner)
                        {
                            nextMatch.Player1 = match.Winner;
                            updated = true;
                            System.Diagnostics.Debug.WriteLine($"✅ Set Player1 of LB match {nextMatch.Id} to {match.Winner.Name} (from Winner)");
                        }
                        else if (match.Loser != null)
                        {
                            nextMatch.Player1 = match.Loser;
                            updated = true;
                            System.Diagnostics.Debug.WriteLine($"✅ Set Player1 of LB match {nextMatch.Id} to {match.Loser.Name} (from Loser)");
                        }
                    }
                    
                    if (nextMatch.SourceMatch2 == match)
                    {
                        if (nextMatch.Player2FromWinner)
                        {
                            nextMatch.Player2 = match.Winner;
                            updated = true;
                            System.Diagnostics.Debug.WriteLine($"✅ Set Player2 of LB match {nextMatch.Id} to {match.Winner.Name} (from Winner)");
                        }
                        else if (match.Loser != null)
                        {
                            nextMatch.Player2 = match.Loser;
                            updated = true;
                            System.Diagnostics.Debug.WriteLine($"✅ Set Player2 of LB match {nextMatch.Id} to {match.Loser.Name} (from Loser)");
                        }
                    }
                    
                    if (updated)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔄 Updated LB match {nextMatch.Id}: {nextMatch.Player1?.Name ?? "TBD"} vs {nextMatch.Player2?.Name ?? "TBD"}");
                    }
                }
            }
            
            // ✅ NEU: Prüfe automatische Byes nach der Propagierung
            var byeManager = new ByeMatchManager(_tournament);
            byeManager.CheckAndHandleAutomaticByes(
                currentPhase.WinnerBracket, 
                currentPhase.LoserBracket);
            
            if (currentPhase.LoserBracket != null)
            {
                byeManager.CheckAndHandleAutomaticByes(
                    currentPhase.LoserBracket, 
                    currentPhase.WinnerBracket);
            }
            
            System.Diagnostics.Debug.WriteLine($"✅ [PropagateFromTree] Completed for match {match.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [PropagateFromTree] ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// ✅ NEU: Aktualisiert ein spezifisches Match im Turnierbaum (für Live-Updates)
    /// </summary>
    public void RefreshMatchInTree(int matchId, bool isLoserBracket)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
       try
   {
  var canvas = isLoserBracket ? _currentLoserBracketCanvas : _currentWinnerBracketCanvas;
  if (canvas == null)
       {
          System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentTree] No active canvas for {(isLoserBracket ? "Loser" : "Winner")} Bracket");
               return;
  }

           System.Diagnostics.Debug.WriteLine($"🔄 [TournamentTree] Match {matchId} refresh requested - canvas has {canvas.Children.Count} children");
    
           // Triggere ein komplettes UI-Refresh - einfachste Lösung
             _tournament.TriggerUIRefresh();
            }
            catch (Exception ex)
    {
System.Diagnostics.Debug.WriteLine($"❌ [TournamentTree] Error: {ex.Message}");
      }
   });
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

  // ✅ NEU: Speichere Canvas-Referenz für Live-Updates
        if (isLoserBracket)
  _currentLoserBracketCanvas = targetCanvas;
       else
   _currentWinnerBracketCanvas = targetCanvas;

   try
      {
 targetCanvas.Children.Clear();

  var matches = isLoserBracket ? _tournament.CurrentPhase.LoserBracket.ToList() : _tournament.CurrentPhase.WinnerBracket.ToList();

    if (matches.Count == 0)
      {
   CreateEmptyBracketMessage(targetCanvas, isLoserBracket, localizationService);
       return targetCanvas;
      }

            // Set canvas properties mit Theme-Unterstützung
            targetCanvas.Background = GetThemeResource("BackgroundBrush") as Brush ?? Brushes.White;
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

    /// <summary>
    /// Hilfsmethode um Theme-Ressourcen zu holen
    /// </summary>
    private object? GetThemeResource(string resourceKey)
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
    private Brush GetThemeBrush(string resourceKey, Brush fallback)
    {
        return GetThemeResource(resourceKey) as Brush ?? fallback;
    }

    /// <summary>
    /// Hilfsmethode um Color aus Theme-Ressourcen zu holen
    /// </summary>
    private Color GetThemeColor(string resourceKey, Color fallback)
    {
        var brush = GetThemeResource(resourceKey) as SolidColorBrush;
        return brush?.Color ?? fallback;
    }

    /// <summary>
    /// Hilfsmethode um eine kontraststarke Farbe für Gewinner-Text zu bekommen
    /// </summary>
    private Brush GetWinnerTextBrush()
    {
        // Für Gewinner verwenden wir eine besonders kontraststarke Farbe
        // Im Dark Mode: helles Grün, im Light Mode: dunkles Grün
        var isDarkMode = IsCurrentThemeDark();
        
        if (isDarkMode)
        {
            // Dark Mode: Verwende ein helles, kontraststarkes Grün
            return new SolidColorBrush(Color.FromRgb(74, 222, 128)); // Helles Grün
        }
        else
        {
            // Light Mode: Verwende ein dunkles, kontraststarkes Grün
            return new SolidColorBrush(Color.FromRgb(21, 128, 61)); // Dunkles Grün
        }
    }

    /// <summary>
    /// Hilfsmethode um zu erkennen ob das aktuelle Theme dunkel ist
    /// </summary>
    private bool IsCurrentThemeDark()
    {
        try
        {
            var backgroundBrush = GetThemeResource("BackgroundBrush") as SolidColorBrush;
            if (backgroundBrush != null)
            {
                var color = backgroundBrush.Color;
                // Berechne die Helligkeit (Luminanz) der Hintergrundfarbe
                var luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
                return luminance < 0.5; // Wenn Luminanz < 0.5, dann ist es ein dunkles Theme
            }
        }
        catch
        {
            // Fallback
        }
        
        return false; // Standardannahme: helles Theme
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
            Foreground = GetThemeBrush("SecondaryTextBrush", Brushes.DarkGray)
        };

        var subText = new TextBlock
        {
            Text = localizationService?.GetString("TournamentTreeWillShow") ?? "Der Turnierbaum wird angezeigt sobald die KO-Phase beginnt",
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = GetThemeBrush("SecondaryTextBrush", Brushes.Gray),
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

        canvas.Background = GetThemeBrush("BackgroundBrush", Brushes.White);
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
                ? GetThemeBrush("ErrorBrush", new SolidColorBrush(Color.FromRgb(205, 92, 92)))
                : GetThemeBrush("SuccessBrush", new SolidColorBrush(Color.FromRgb(34, 139, 34))),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = GetThemeColor("SecondaryTextBrush", Colors.Gray),
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
                ? GetThemeBrush("ErrorBrush", new SolidColorBrush(Color.FromArgb(220, 255, 182, 193)))
                : GetThemeBrush("SuccessBrush", new SolidColorBrush(Color.FromArgb(220, 144, 238, 144))),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(20, 8, 20, 8),
            BorderBrush = isLoserBracket 
                ? GetThemeBrush("ErrorBrush", Brushes.IndianRed)
                : GetThemeBrush("SuccessBrush", Brushes.ForestGreen),
            BorderThickness = new Thickness(3),
            Effect = new DropShadowEffect
            {
                Color = GetThemeColor("SecondaryTextBrush", Colors.Gray),
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
            Foreground = GetThemeBrush("TextBrush", Brushes.DarkSlateGray),
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
            Effect = new DropShadowEffect
            {
                Color = GetThemeColor("SecondaryTextBrush", Colors.Gray),
                Direction = 315,
                ShadowDepth = 4,
                BlurRadius = 6,
                Opacity = 0.6
            }
        };

        // Set background and border based on match status with theme support
        SetMatchControlAppearance(border, match);

        // Create content grid mit WENIGER Zeilen (Match ID entfernt)
        var contentGrid = new Grid
        {
            Background = Brushes.Transparent 
        };
        // NUR 2 Zeilen: Spieler-Bereich und Score/Status
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
            // Verbesserte Farb-Logik für bessere Lesbarkeit
            Foreground = match.Winner?.Id == match.Player1?.Id
                ? GetWinnerTextBrush()
                : GetThemeBrush("TextBrush", new SolidColorBrush(Color.FromRgb(30, 30, 30))),
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
            Foreground = GetThemeBrush("SecondaryTextBrush", new SolidColorBrush(Color.FromRgb(128, 128, 128))),
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
            // Verbesserte Farb-Logik für bessere Lesbarkeit
            Foreground = match.Winner?.Id == match.Player2?.Id
                ? GetWinnerTextBrush()
                : GetThemeBrush("TextBrush", new SolidColorBrush(Color.FromRgb(30, 30, 30))),
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
            Foreground = GetThemeBrush("AccentBrush", Brushes.DarkBlue),
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
            MatchStatus.NotStarted => GetThemeBrush("SecondaryTextBrush", Brushes.Gray),
            MatchStatus.InProgress => GetThemeBrush("WarningBrush", Brushes.Orange),
            MatchStatus.Finished => GetThemeBrush("SuccessBrush", Brushes.Green),
            MatchStatus.Bye => GetThemeBrush("AccentBrush", Brushes.RoyalBlue),
            _ => GetThemeBrush("SecondaryTextBrush", Brushes.Gray)
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
                border.Background = GetThemeBrush("SurfaceBrush", new SolidColorBrush(Color.FromRgb(248, 249, 250)));
                border.BorderBrush = GetThemeBrush("BorderBrush", new SolidColorBrush(Color.FromRgb(200, 200, 200)));
                border.BorderThickness = new Thickness(2);
                break;
            case MatchStatus.InProgress:
                // Verwende eine hellere Version der WarningBrush für den Hintergrund
                var warningColor = GetThemeColor("WarningBrush", Color.FromRgb(251, 191, 36));
                border.Background = new SolidColorBrush(Color.FromArgb(50, warningColor.R, warningColor.G, warningColor.B)); // 50 = ~20% Opazität
                border.BorderBrush = GetThemeBrush("WarningBrush", Brushes.Orange);
                border.BorderThickness = new Thickness(3);
                break;
            case MatchStatus.Finished:
                // Verwende eine hellere Version der SuccessBrush für den Hintergrund
                var successColor = GetThemeColor("SuccessBrush", Color.FromRgb(34, 197, 94));
                border.Background = new SolidColorBrush(Color.FromArgb(50, successColor.R, successColor.G, successColor.B)); // 50 = ~20% Opazität
                border.BorderBrush = GetThemeBrush("SuccessBrush", Brushes.Green);
                border.BorderThickness = new Thickness(3);
                break;
            case MatchStatus.Bye:
                // Verwende eine hellere Version der AccentBrush für den Hintergrund
                var accentColor = GetThemeColor("AccentBrush", Color.FromRgb(59, 130, 246));
                border.Background = new SolidColorBrush(Color.FromArgb(50, accentColor.R, accentColor.G, accentColor.B)); // 50 = ~20% Opazität
                border.BorderBrush = GetThemeBrush("AccentBrush", Brushes.RoyalBlue);
                border.BorderThickness = new Thickness(3);
                break;
            default:
                border.Background = GetThemeBrush("SurfaceBrush", Brushes.White);
                border.BorderBrush = GetThemeBrush("BorderBrush", Brushes.Gray);
                border.BorderThickness = new Thickness(2);
                break;
        }
    }

    private void AddMatchInteractivity(Border border, KnockoutMatch match, LocalizationService? localizationService)
    {
        // Mouse enter/leave effects
        border.MouseEnter += (s, e) =>
        {
            var transform = new ScaleTransform(1.05, 1.05);
            border.RenderTransformOrigin = new Point(0.5, 0.5);
            border.RenderTransform = transform;
            
            var dropShadow = (DropShadowEffect?)border.Effect;
            if (dropShadow != null)
            {
                dropShadow.ShadowDepth = 6;
                dropShadow.BlurRadius = 8;
            }
        };

        border.MouseLeave += (s, e) =>
        {
            border.RenderTransform = null;
            
            var dropShadow = (DropShadowEffect?)border.Effect;
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
            // ✅ FIXED: HubIntegrationService UND Tournament-ID vom MainWindow holen
            HubIntegrationService? hubService = null;
            string? tournamentId = null;  // ⭐ NEU
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTreeRenderer] Getting HubService and TournamentId for match {match.Id}...");
                
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTreeRenderer] MainWindow found: {mainWindow.GetType().Name}");
                    
                    // Verwende den ServiceInitializer des MainWindow, um Services abzurufen
                    var initializerField = mainWindow.GetType()
                        .GetField("_serviceInitializer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var initializer = initializerField?.GetValue(mainWindow) as MainWindowServiceInitializer;

                    System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTreeRenderer] ServiceInitializer found: {initializer != null}");

                    if (initializer != null)
                    {
                        var licensedHubService = initializer.HubService;
                        hubService = licensedHubService?.InnerHubService;
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTreeRenderer] HubIntegrationService retrieved: {hubService != null}");
                        if (hubService != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTreeRenderer] HubService registered: {hubService.IsRegisteredWithHub}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"❌ [TournamentTreeRenderer] HubIntegrationService is null");
                        }

                        var tournamentData = initializer.TournamentService.GetTournamentData();
                        tournamentId = tournamentData?.TournamentId;
                        System.Diagnostics.Debug.WriteLine($"🎯 [TournamentTreeRenderer] Tournament ID from TournamentService: {tournamentId ?? "null"}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentTreeRenderer] ServiceInitializer not found");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ [TournamentTreeRenderer] MainWindow not found or wrong type");
                }
            }
            catch (Exception hubEx)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [TournamentTreeRenderer] Could not get HubService or TournamentId: {hubEx.Message}");
            }

            // WICHTIG: Verwende rundenspezifische Regeln für KO-Matches
            var roundRules = _tournament.GameRules.GetRulesForRound(match.Round);
            
            System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog: Match {match.Id} in {match.Round}");
            System.Diagnostics.Debug.WriteLine($"  Round Rules: SetsToWin={roundRules.SetsToWin}, LegsToWin={roundRules.LegsToWin}, LegsPerSet={roundRules.LegsPerSet}");
            System.Diagnostics.Debug.WriteLine($"  Using SPECIALIZED constructor for KnockoutMatch with HubService: {hubService != null}, TournamentId: {tournamentId ?? "null"}");

            // ✅ FIXED: Übergebe HubService UND Tournament-ID an das MatchResultWindow
            var resultWindow = new MatchResultWindow(match, roundRules, _tournament.GameRules, localizationService, hubService, tournamentId);  // ⭐ Tournament-ID hinzugefügt
            
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
                
                // ✅ WICHTIG: Setze auch den Verlierer!
                if (match.Winner != null)
                {
                    if (match.Player1 == match.Winner)
                    {
                        match.Loser = match.Player2;
                    }
                    else if (match.Player2 == match.Winner)
                    {
                        match.Loser = match.Player1;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog: Match {match.Id} result saved");
                    System.Diagnostics.Debug.WriteLine($"  Winner: {match.Winner.Name}, Loser: {match.Loser?.Name}, Sets: {match.Player1Sets}:{match.Player2Sets}, Legs: {match.Player1Legs}:{match.Player2Legs}");
                }

                // ✅ NEU: Verwende die verbesserte Propagierungslogik!
                PropagateMatchResultFromTree(match);
                
                // Process the result through the tournament system for statistics etc.
                _tournament.ProcessMatchResult(match);
                
                // Trigger UI refresh
                _tournament.TriggerUIRefresh();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog ERROR: {ex.Message}");
            MessageBox.Show($"Fehler beim Öffnen des Ergebnis-Dialogs: {ex.Message}", "Fehler", 
    MessageBoxButton.OK, MessageBoxImage.Error);
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