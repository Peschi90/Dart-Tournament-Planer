using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services
{
    /// <summary>
    /// Service für Druckoperationen von Turnierstatistiken
    /// </summary>
    public class PrintService
    {
        private readonly LocalizationService? _localizationService;

        public PrintService(LocalizationService? localizationService = null)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Druckt die Statistiken einer Turnierklasse
        /// </summary>
        public bool PrintTournamentStatistics(TournamentClass tournamentClass, TournamentPrintOptions printOptions)
        {
            try
            {
                var printDialog = new PrintDialog();
                
                if (printOptions.ShowPrintDialog && printDialog.ShowDialog() != true)
                {
                    return false;
                }

                var document = CreateTournamentDocument(tournamentClass, printOptions);
                
                if (document == null)
                {
                    MessageBox.Show("Fehler beim Erstellen des Druckdokuments.", "Druckfehler", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                ConfigurePrintSettings(printDialog);
                printDialog.PrintDocument(document.DocumentPaginator, 
                    $"Turnierstatistiken - {tournamentClass.Name}");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Drucken: {ex.Message}", "Druckfehler", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Erstellt eine Druckvorschau
        /// </summary>
        public FrameworkElement? CreatePrintPreview(TournamentClass tournamentClass, TournamentPrintOptions printOptions)
        {
            try
            {
                var document = CreateTournamentDocument(tournamentClass, printOptions);
                if (document == null) return null;

                var viewer = new DocumentViewer
                {
                    Document = document,
                    Background = Brushes.White
                };

                return viewer;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PrintService.CreatePrintPreview: ERROR: {ex.Message}");
                return null;
            }
        }

        private void ConfigurePrintSettings(PrintDialog printDialog)
        {
            var printTicket = printDialog.PrintTicket;
            printTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
            printTicket.PageOrientation = PageOrientation.Portrait;
        }

        private FixedDocument? CreateTournamentDocument(TournamentClass tournamentClass, TournamentPrintOptions printOptions)
        {
            var document = new FixedDocument();

            if (printOptions.IncludeOverview)
            {
                var overviewPage = CreateOverviewPage(tournamentClass, printOptions);
                if (overviewPage != null)
                    document.Pages.Add(overviewPage);
            }

            if (printOptions.IncludeGroupPhase && tournamentClass.Groups.Any())
            {
                foreach (var group in tournamentClass.Groups)
                {
                    if (!printOptions.SelectedGroups.Any() || printOptions.SelectedGroups.Contains(group.Id))
                    {
                        var groupPage = CreateGroupPhasePage(group, tournamentClass, printOptions);
                        if (groupPage != null)
                            document.Pages.Add(groupPage);
                    }
                }
            }

            if (printOptions.IncludeFinalsPhase && 
                tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                var finalsPage = CreateFinalsPage(tournamentClass, printOptions);
                if (finalsPage != null)
                    document.Pages.Add(finalsPage);
            }

            if (printOptions.IncludeKnockoutPhase && 
                tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                var knockoutPages = CreateKnockoutPages(tournamentClass, printOptions);
                foreach (var page in knockoutPages)
                {
                    document.Pages.Add(page);
                }
            }

            return document.Pages.Count > 0 ? document : null;
        }

        private PageContent? CreateOverviewPage(TournamentClass tournamentClass, TournamentPrintOptions printOptions)
        {
            try
            {
                var pageContent = new PageContent();
                var fixedPage = new FixedPage
                {
                    Width = 793.7,
                    Height = 1122.5,
                    Background = Brushes.White
                };

                AddPageHeader(fixedPage, $"Turnierübersicht - {tournamentClass.Name}", printOptions);
                double yPosition = 90;
                yPosition = AddTournamentInfo(fixedPage, tournamentClass, yPosition);
                
                if (tournamentClass.Groups.Any())
                {
                    yPosition = AddGroupsOverview(fixedPage, tournamentClass, yPosition);
                }

                AddPageFooter(fixedPage, printOptions);
                pageContent.Child = fixedPage;
                return pageContent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateOverviewPage: ERROR: {ex.Message}");
                return null;
            }
        }

        private PageContent? CreateGroupPhasePage(Group group, TournamentClass tournamentClass, TournamentPrintOptions printOptions)
        {
            try
            {
                var pageContent = new PageContent();
                var fixedPage = new FixedPage
                {
                    Width = 793.7,
                    Height = 1122.5,
                    Background = Brushes.White
                };

                AddPageHeader(fixedPage, $"{tournamentClass.Name} - {group.Name}", printOptions);
                double yPosition = 90;
                yPosition = AddGroupDetails(fixedPage, group, yPosition);

                if (group.Players.Any())
                {
                    yPosition = AddStandingsTable(fixedPage, group, yPosition);
                }

                if (group.Matches.Any() && yPosition < 900)
                {
                    yPosition = AddMatchResults(fixedPage, group, yPosition);
                }

                AddPageFooter(fixedPage, printOptions);
                pageContent.Child = fixedPage;
                return pageContent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateGroupPhasePage: ERROR: {ex.Message}");
                return null;
            }
        }

        private PageContent? CreateFinalsPage(TournamentClass tournamentClass, TournamentPrintOptions printOptions)
        {
            try
            {
                var pageContent = new PageContent();
                var fixedPage = new FixedPage
                {
                    Width = 793.7,
                    Height = 1122.5,
                    Background = Brushes.White
                };

                AddPageHeader(fixedPage, $"{tournamentClass.Name} - Finalrunde", printOptions);
                double yPosition = 90;

                if (tournamentClass.CurrentPhase?.FinalsGroup != null)
                {
                    yPosition = AddStandingsTable(fixedPage, tournamentClass.CurrentPhase.FinalsGroup, yPosition);
                }

                var finalsMatches = tournamentClass.GetFinalsMatches();
                if (finalsMatches.Any() && yPosition < 900)
                {
                    yPosition = AddMatchResults(fixedPage, tournamentClass.CurrentPhase.FinalsGroup, yPosition);
                }

                AddPageFooter(fixedPage, printOptions);
                pageContent.Child = fixedPage;
                return pageContent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateFinalsPage: ERROR: {ex.Message}");
                return null;
            }
        }

        private List<PageContent> CreateKnockoutPages(TournamentClass tournamentClass, TournamentPrintOptions printOptions)
        {
            var pages = new List<PageContent>();

            try
            {
                if (printOptions.IncludeWinnerBracket)
                {
                    var winnerPage = CreateKnockoutBracketPage(tournamentClass, false, printOptions);
                    if (winnerPage != null)
                        pages.Add(winnerPage);
                }

                if (printOptions.IncludeLoserBracket && 
                    tournamentClass.GetLoserBracketMatches().Any())
                {
                    var loserPage = CreateKnockoutBracketPage(tournamentClass, true, printOptions);
                    if (loserPage != null)
                        pages.Add(loserPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPages: ERROR: {ex.Message}");
            }

            return pages;
        }

        private PageContent? CreateKnockoutBracketPage(TournamentClass tournamentClass, bool isLoserBracket, TournamentPrintOptions printOptions)
        {
            try
            {
                var pageContent = new PageContent();
                var fixedPage = new FixedPage
                {
                    Width = 793.7,
                    Height = 1122.5,
                    Background = Brushes.White
                };

                var bracketName = isLoserBracket ? "Loser Bracket" : "Winner Bracket";
                AddPageHeader(fixedPage, $"{tournamentClass.Name} - {bracketName}", printOptions);
                double yPosition = 90;

                var matches = isLoserBracket ? 
                    tournamentClass.GetLoserBracketMatches() : 
                    tournamentClass.GetWinnerBracketMatches();

                yPosition = AddKnockoutMatches(fixedPage, matches.ToList(), yPosition, isLoserBracket);
                AddPageFooter(fixedPage, printOptions);

                pageContent.Child = fixedPage;
                return pageContent;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutBracketPage: ERROR: {ex.Message}");
                return null;
            }
        }

        #region Helper Methods

        private void AddPageHeader(FixedPage page, string title, TournamentPrintOptions printOptions)
        {
            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            
            FixedPage.SetLeft(titleBlock, 50);
            FixedPage.SetTop(titleBlock, 30);
            page.Children.Add(titleBlock);

            var dateBlock = new TextBlock
            {
                Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                FontSize = 12,
                Foreground = Brushes.Gray
            };
            
            FixedPage.SetRight(dateBlock, 50);
            FixedPage.SetTop(dateBlock, 35);
            page.Children.Add(dateBlock);

            var line = new Line
            {
                X1 = 50, X2 = 743, Y1 = 75, Y2 = 75,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1.5
            };
            page.Children.Add(line);
        }

        private void AddPageFooter(FixedPage page, TournamentPrintOptions printOptions)
        {
            var line = new Line
            {
                X1 = 50, X2 = 743, Y1 = 1070, Y2 = 1070,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            page.Children.Add(line);

            var footerBlock = new TextBlock
            {
                Text = "Erstellt mit Dart Tournament Planner",
                FontSize = 8,
                Foreground = Brushes.Gray
            };
            
            FixedPage.SetLeft(footerBlock, 50);
            FixedPage.SetTop(footerBlock, 1080);
            page.Children.Add(footerBlock);
        }

        private double AddTournamentInfo(FixedPage page, TournamentClass tournamentClass, double yPosition)
        {
            var infoPanel = new StackPanel { Orientation = Orientation.Vertical };

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"Spielregeln: {tournamentClass.GameRules}",
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"Aktuelle Phase: {tournamentClass.CurrentPhase?.Name ?? "Nicht begonnen"}",
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            });

            var totalPlayers = tournamentClass.Groups.SelectMany(g => g.Players).Count();
            infoPanel.Children.Add(new TextBlock
            {
                Text = $"Gruppen: {tournamentClass.Groups.Count}, Spieler gesamt: {totalPlayers}",
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            });

            FixedPage.SetLeft(infoPanel, 50);
            FixedPage.SetTop(infoPanel, yPosition);
            page.Children.Add(infoPanel);

            return yPosition + 100;
        }

        private double AddGroupsOverview(FixedPage page, TournamentClass tournamentClass, double yPosition)
        {
            var titleBlock = new TextBlock
            {
                Text = "Gruppen-Übersicht",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 15, 0, 10)
            };

            FixedPage.SetLeft(titleBlock, 50);
            FixedPage.SetTop(titleBlock, yPosition);
            page.Children.Add(titleBlock);
            yPosition += 40;

            foreach (var group in tournamentClass.Groups)
            {
                var groupInfo = new TextBlock
                {
                    Text = $"• {group.Name}: {group.Players.Count} Spieler, {group.Matches.Count(m => m.Status == MatchStatus.Finished)} von {group.Matches.Count} Spielen beendet",
                    FontSize = 13,
                    Margin = new Thickness(20, 4, 0, 0)
                };

                FixedPage.SetLeft(groupInfo, 50);
                FixedPage.SetTop(groupInfo, yPosition);
                page.Children.Add(groupInfo);
                yPosition += 25;
            }

            return yPosition + 25;
        }

        private double AddGroupDetails(FixedPage page, Group group, double yPosition)
        {
            var detailsPanel = new StackPanel { Orientation = Orientation.Vertical };

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Spieler: {group.Players.Count}",
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Spiele: {group.Matches.Count(m => m.Status == MatchStatus.Finished)} von {group.Matches.Count} beendet",
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            });

            FixedPage.SetLeft(detailsPanel, 50);
            FixedPage.SetTop(detailsPanel, yPosition);
            page.Children.Add(detailsPanel);

            return yPosition + 70;
        }

        private double AddStandingsTable(FixedPage page, Group group, double yPosition)
        {
            var titleBlock = new TextBlock
            {
                Text = "Tabelle",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };

            FixedPage.SetLeft(titleBlock, 50);
            FixedPage.SetTop(titleBlock, yPosition);
            page.Children.Add(titleBlock);
            yPosition += 45;

            try
            {
                var standings = group.GetStandings();
                if (standings != null && standings.Any())
                {
                    var table = CreateStandingsTable(standings);
                    FixedPage.SetLeft(table, 50);
                    FixedPage.SetTop(table, yPosition);
                    page.Children.Add(table);
                    return yPosition + 200;
                }
                else
                {
                    var noStandingsBlock = new TextBlock
                    {
                        Text = "Noch keine Tabelle verfügbar.",
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        FontSize = 14
                    };
                    
                    FixedPage.SetLeft(noStandingsBlock, 50);
                    FixedPage.SetTop(noStandingsBlock, yPosition);
                    page.Children.Add(noStandingsBlock);
                    
                    return yPosition + 40;
                }
            }
            catch (Exception ex)
            {
                var errorBlock = new TextBlock
                {
                    Text = $"FEHLER: {ex.Message}",
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Red,
                    FontSize = 11
                };
                
                FixedPage.SetLeft(errorBlock, 50);
                FixedPage.SetTop(errorBlock, yPosition);
                page.Children.Add(errorBlock);
                
                return yPosition + 60;
            }
        }

        private double AddMatchResults(FixedPage page, Group group, double yPosition)
        {
            var titleBlock = new TextBlock
            {
                Text = "Spielergebnisse",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 25, 0, 15)
            };

            FixedPage.SetLeft(titleBlock, 50);
            FixedPage.SetTop(titleBlock, yPosition);
            page.Children.Add(titleBlock);
            yPosition += 50;

            try
            {
                var allMatches = group.Matches.ToList();
                if (allMatches.Any())
                {
                    var sortedMatches = allMatches
                        .OrderBy(m => m.Status == MatchStatus.Finished || m.Status == MatchStatus.Bye ? 0 : 1)
                        .ThenBy(m => m.Id)
                        .ToList();

                    var matchesTable = CreateMatchesTable(sortedMatches.Take(20).ToList());
                    FixedPage.SetLeft(matchesTable, 50);
                    FixedPage.SetTop(matchesTable, yPosition);
                    page.Children.Add(matchesTable);

                    return yPosition + Math.Max(250, sortedMatches.Count * 25 + 100);
                }
                else
                {
                    var noMatchesBlock = new TextBlock
                    {
                        Text = "Keine Spiele vorhanden.",
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        FontSize = 12
                    };

                    FixedPage.SetLeft(noMatchesBlock, 50);
                    FixedPage.SetTop(noMatchesBlock, yPosition);
                    page.Children.Add(noMatchesBlock);

                    return yPosition + 25;
                }
            }
            catch (Exception ex)
            {
                var errorBlock = new TextBlock
                {
                    Text = $"FEHLER bei Spielergebnissen: {ex.Message}",
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Red,
                    FontSize = 11
                };

                FixedPage.SetLeft(errorBlock, 50);
                FixedPage.SetTop(errorBlock, yPosition);
                page.Children.Add(errorBlock);

                return yPosition + 60;
            }
        }

        private double AddKnockoutMatches(FixedPage page, List<KnockoutMatch> matches, double yPosition, bool isLoserBracket)
        {
            var titleBlock = new TextBlock
            {
                Text = $"{(isLoserBracket ? "Loser Bracket" : "Winner Bracket")} - Spiele",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 25, 0, 15)
            };

            FixedPage.SetLeft(titleBlock, 50);
            FixedPage.SetTop(titleBlock, yPosition);
            page.Children.Add(titleBlock);
            yPosition += 50;

            try
            {
                if (matches.Any())
                {
                    var table = CreateKnockoutMatchesTable(matches);
                    FixedPage.SetLeft(table, 50);
                    FixedPage.SetTop(table, yPosition);
                    page.Children.Add(table);

                    return yPosition + Math.Max(300, matches.Count * 25 + 120);
                }
                else
                {
                    var noMatchesBlock = new TextBlock
                    {
                        Text = $"Keine {(isLoserBracket ? "Loser Bracket" : "Winner Bracket")} Spiele vorhanden.",
                        FontStyle = FontStyles.Italic,
                        Foreground = Brushes.Gray,
                        FontSize = 12
                    };

                    FixedPage.SetLeft(noMatchesBlock, 50);
                    FixedPage.SetTop(noMatchesBlock, yPosition);
                    page.Children.Add(noMatchesBlock);

                    return yPosition + 25;
                }
            }
            catch (Exception ex)
            {
                var errorBlock = new TextBlock
                {
                    Text = $"FEHLER bei {(isLoserBracket ? "Loser" : "Winner")} Bracket: {ex.Message}",
                    FontStyle = FontStyles.Italic,
                    Foreground = Brushes.Red,
                    FontSize = 11
                };

                FixedPage.SetLeft(errorBlock, 50);
                FixedPage.SetTop(errorBlock, yPosition);
                page.Children.Add(errorBlock);

                return yPosition + 60;
            }
        }

        private FrameworkElement CreateStandingsTable(List<PlayerStanding> standings)
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

            // Header-Zeile
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            var headers = new[] { "Pos", "Spieler", "Sp", "S", "U", "N", "Pkt", "Sets" };
            
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
                    FontSize = 11,
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
                    standing.Player?.Name ?? "Unknown",
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
                        FontSize = 10,
                        TextAlignment = col == 1 ? TextAlignment.Left : TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };
                    
                    // Hervorhebung für erste Plätze
                    if (col == 0)
                    {
                        if (standing.Position == 1)
                            cellText.Foreground = new SolidColorBrush(Color.FromRgb(212, 175, 55)); // Gold
                        else if (standing.Position == 2)
                            cellText.Foreground = new SolidColorBrush(Color.FromRgb(192, 192, 192)); // Silber
                        else if (standing.Position == 3)
                            cellText.Foreground = new SolidColorBrush(Color.FromRgb(205, 127, 50)); // Bronze
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
                Child = grid,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 315,
                    ShadowDepth = 2,
                    BlurRadius = 4,
                    Opacity = 0.3
                }
            };
        }

        private FrameworkElement CreateMatchesTable(List<Match> matches)
        {
            var grid = new Grid
            {
                Background = Brushes.White,
                MaxWidth = 700
            };

            // Spalten definieren
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            // Header-Zeile
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            var headers = new[] { "Nr", "Spiel", "Status", "Ergebnis", "Gewinner" };
            
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
                    FontSize = 11,
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
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
                var match = matches[row];

                string statusText;
                string resultText;
                string winnerText;
                Brush textColor = Brushes.Black;
                
                if (match.IsBye || match.Status == MatchStatus.Bye)
                {
                    statusText = "FREILOS";
                    resultText = "-";
                    winnerText = match.Player1?.Name ?? "-";
                    textColor = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                }
                else if (match.Status == MatchStatus.Finished)
                {
                    statusText = "BEENDET";
                    resultText = match.ScoreDisplay ?? "-";
                    winnerText = match.Winner?.Name ?? "Unentschieden";
                    textColor = new SolidColorBrush(Color.FromRgb(25, 135, 84));
                }
                else if (match.Status == MatchStatus.InProgress)
                {
                    statusText = "LÄUFT";
                    resultText = match.ScoreDisplay ?? "-";
                    winnerText = "-";
                    textColor = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                }
                else
                {
                    statusText = "AUSSTEHEND";
                    resultText = "-";
                    winnerText = "-";
                    textColor = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                }

                var values = new string[]
                {
                    (row + 1).ToString(),
                    match.IsBye ? $"{match.Player1?.Name} (Freilos)" : $"{match.Player1?.Name} vs {match.Player2?.Name}",
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
                        FontSize = 10,
                        TextAlignment = col == 1 ? TextAlignment.Left : TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        Foreground = col == 2 ? textColor : Brushes.Black
                    };
                    
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
                Child = grid,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 315,
                    ShadowDepth = 2,
                    BlurRadius = 4,
                    Opacity = 0.3
                }
            };
        }

        private FrameworkElement CreateKnockoutMatchesTable(List<KnockoutMatch> matches)
        {
            var grid = new Grid
            {
                Background = Brushes.White,
                MaxWidth = 750
            };

            // Spalten definieren
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            // Header-Zeile
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            var headers = new[] { "Nr", "Runde", "Spiel", "Status", "Ergebnis", "Gewinner" };
            
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
                    FontSize = 11,
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
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
                var match = matches[row];

                string statusText;
                string resultText;
                string winnerText;
                Brush textColor = Brushes.Black;
                
                bool isBye = match.Status == MatchStatus.Bye || 
                            (match.Player1 != null && match.Player2 == null) || 
                            (match.Player1 == null && match.Player2 != null);
                
                if (isBye)
                {
                    statusText = "FREILOS";
                    resultText = "-";
                    winnerText = match.Player1?.Name ?? match.Player2?.Name ?? "-";
                    textColor = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                }
                else if (match.Status == MatchStatus.Finished)
                {
                    statusText = "BEENDET";
                    resultText = match.ScoreDisplay ?? "-";
                    winnerText = match.Winner?.Name ?? "Unentschieden";
                    textColor = new SolidColorBrush(Color.FromRgb(25, 135, 84));
                }
                else if (match.Status == MatchStatus.InProgress)
                {
                    statusText = "LÄUFT";
                    resultText = match.ScoreDisplay ?? "-";
                    winnerText = "-";
                    textColor = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                }
                else
                {
                    statusText = "AUSSTEHEND";
                    resultText = "-";
                    winnerText = "-";
                    textColor = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                }

                var player1Name = match.Player1?.Name ?? "TBD";
                var player2Name = match.Player2?.Name ?? "TBD";
                var matchText = isBye ? $"{player1Name} (Freilos)" : $"{player1Name} vs {player2Name}";

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
                        FontSize = 10,
                        TextAlignment = col == 2 ? TextAlignment.Left : TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        Foreground = col == 3 ? textColor : Brushes.Black
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
            }

            return new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(52, 58, 64)),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(4),
                Child = grid,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Gray,
                    Direction = 315,
                    ShadowDepth = 2,
                    BlurRadius = 4,
                    Opacity = 0.3
                }
            };
        }

        private string GetRoundDisplayName(KnockoutRound round)
        {
            return round switch
            {
                KnockoutRound.Best64 => "Beste 64",
                KnockoutRound.Best32 => "Beste 32", 
                KnockoutRound.Best16 => "Beste 16",
                KnockoutRound.Quarterfinal => "Viertelfinale",
                KnockoutRound.Semifinal => "Halbfinale", 
                KnockoutRound.Final => "Finale",
                KnockoutRound.GrandFinal => "Grand Final",
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

        #endregion
    }

    /// <summary>
    /// Konfigurationsklasse für Druckoptionen
    /// </summary>
    public class TournamentPrintOptions
    {
        public bool ShowPrintDialog { get; set; } = true;
        public bool IncludeOverview { get; set; } = true;
        public bool IncludeGroupPhase { get; set; } = true;
        public List<int> SelectedGroups { get; set; } = new List<int>();
        public bool IncludeFinalsPhase { get; set; } = true;
        public bool IncludeKnockoutPhase { get; set; } = true;
        public bool IncludeWinnerBracket { get; set; } = true;
        public bool IncludeLoserBracket { get; set; } = true;
        public bool IncludeKnockoutParticipants { get; set; } = true;
        public string Title { get; set; } = "";
        public string Subtitle { get; set; } = "";
    }
}