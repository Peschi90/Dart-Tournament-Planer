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
using DartTournamentPlaner.Helpers;

namespace DartTournamentPlaner.Services
{
    /// <summary>
    /// Service für Druckoperationen von Turnierstatistiken
    /// Unterstützt QR-Codes für Matches wenn Tournament beim Hub registriert ist
    /// </summary>
    public class PrintService
    {
        private readonly LocalizationService? _localizationService;
        private readonly PrintQRCodeHelper? _qrCodeHelper;

        public PrintService(LocalizationService? localizationService = null, HubIntegrationService? hubService = null)
        {
            _localizationService = localizationService;
     
            // QR-Code Helper initialisieren wenn HubService verfügbar
      if (hubService != null)
            {
                _qrCodeHelper = new PrintQRCodeHelper(hubService);
     System.Diagnostics.Debug.WriteLine($"[PrintService] QR Code Helper initialized - Available: {_qrCodeHelper.AreQRCodesAvailable}");
      System.Diagnostics.Debug.WriteLine($"[PrintService] Hub registered: {hubService.IsRegisteredWithHub}");
 }
            else
         {
      System.Diagnostics.Debug.WriteLine($"[PrintService] No HubService provided - QR Codes will not be available");
            }
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
                    var errorMessage = _localizationService?.GetString("ErrorCreatingDocument") ?? "Fehler beim Erstellen des Druckdokuments.";
                    MessageBox.Show(errorMessage, _localizationService?.GetString("PrintError") ?? "Druckfehler", 
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                ConfigurePrintSettings(printDialog);
                var documentTitle = _localizationService?.GetString("TournamentStatistics", tournamentClass.Name) ?? $"Turnierstatistiken - {tournamentClass.Name}";
                printDialog.PrintDocument(document.DocumentPaginator, documentTitle);

                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = _localizationService?.GetString("ErrorPrinting", ex.Message) ?? $"Fehler beim Drucken: {ex.Message}";
                MessageBox.Show(errorMessage, _localizationService?.GetString("PrintError") ?? "Druckfehler", 
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

                var pageTitle = _localizationService?.GetString("TournamentOverviewPrint", tournamentClass.Name) ?? $"Turnierübersicht - {tournamentClass.Name}";
                AddPageHeader(fixedPage, pageTitle, printOptions);
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

                var pageTitle = $"{tournamentClass.Name} - {_localizationService?.GetString("FinalsRound") ?? "Finalrunde"}";
                AddPageHeader(fixedPage, pageTitle, printOptions);
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

                var bracketName = isLoserBracket 
                    ? (_localizationService?.GetString("LoserBracket") ?? "Loser Bracket")
                    : (_localizationService?.GetString("WinnerBracket") ?? "Winner Bracket");
                var pageTitle = isLoserBracket
                    ? _localizationService?.GetString("LoserBracketHeader", tournamentClass.Name) ?? $"{tournamentClass.Name} - Loser Bracket"
                    : _localizationService?.GetString("WinnerBracketHeader", tournamentClass.Name) ?? $"{tournamentClass.Name} - Winner Bracket";
                    
                AddPageHeader(fixedPage, pageTitle, printOptions);
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

            var footerText = _localizationService?.GetString("CreatedWith") ?? "Erstellt mit Dart Tournament Planner";
            var footerBlock = new TextBlock
            {
                Text = footerText,
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

            var gameRulesText = _localizationService?.GetString("GameRulesLabel", tournamentClass.GameRules) ?? $"Spielregeln: {tournamentClass.GameRules}";
            infoPanel.Children.Add(new TextBlock
            {
                Text = gameRulesText,
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            });

            var currentPhaseName = tournamentClass.CurrentPhase?.Name ?? (_localizationService?.GetString("NotStarted") ?? "Nicht begonnen");
            var currentPhaseText = _localizationService?.GetString("CurrentPhaseLabel", currentPhaseName) ?? $"Aktuelle Phase: {currentPhaseName}";
            infoPanel.Children.Add(new TextBlock
            {
                Text = currentPhaseText,
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            });

            var totalPlayers = tournamentClass.Groups.SelectMany(g => g.Players).Count();
            var groupsPlayersText = _localizationService?.GetString("GroupsPlayersLabel", tournamentClass.Groups.Count, totalPlayers) ?? $"Gruppen: {tournamentClass.Groups.Count}, Spieler gesamt: {totalPlayers}";
            infoPanel.Children.Add(new TextBlock
            {
                Text = groupsPlayersText,
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
            var titleText = _localizationService?.GetString("GroupsOverview") ?? "Gruppen-Übersicht";
            var titleBlock = new TextBlock
            {
                Text = titleText,
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
                var playersText = _localizationService?.GetString("PlayersCount", group.Players.Count) ?? $"Spieler: {group.Players.Count}";
                var finishedMatches = group.Matches.Count(m => m.Status == MatchStatus.Finished);
                var totalMatches = group.Matches.Count;
                var matchesText = _localizationService?.GetString("MatchesStatus", finishedMatches, totalMatches) ?? $"{finishedMatches} von {totalMatches} Spielen beendet";
                
                var groupInfo = new TextBlock
                {
                    Text = $"• {group.Name}: {playersText}, {matchesText}",
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

            var playersText = _localizationService?.GetString("PlayersCount", group.Players.Count) ?? $"Spieler: {group.Players.Count}";
            detailsPanel.Children.Add(new TextBlock
            {
                Text = playersText,
                FontSize = 14,
                Margin = new Thickness(0, 8, 0, 0)
            });

            var finishedMatches = group.Matches.Count(m => m.Status == MatchStatus.Finished);
            var totalMatches = group.Matches.Count;
            var matchesText = _localizationService?.GetString("MatchesStatus", finishedMatches, totalMatches) ?? $"Spiele: {finishedMatches} von {totalMatches} beendet";
            detailsPanel.Children.Add(new TextBlock
            {
                Text = matchesText,
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
            var titleText = _localizationService?.GetString("Table") ?? "Tabelle";
            var titleBlock = new TextBlock
            {
                Text = titleText,
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
                    var noStandingsText = _localizationService?.GetString("NoStandingsAvailable") ?? "Noch keine Tabelle verfügbar.";
                    var noStandingsBlock = new TextBlock
                    {
                        Text = noStandingsText,
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
            var titleText = _localizationService?.GetString("MatchResults") ?? "Spielergebnisse";
            var titleBlock = new TextBlock
            {
                Text = titleText,
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
                    var noMatchesText = _localizationService?.GetString("NoMatchesAvailable") ?? "Keine Spiele vorhanden.";
                    var noMatchesBlock = new TextBlock
                    {
                        Text = noMatchesText,
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
                var errorText = $"FEHLER bei Spielergebnissen: {ex.Message}";
                var errorBlock = new TextBlock
                {
                    Text = errorText,
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
            var titleText = isLoserBracket 
                ? (_localizationService?.GetString("LoserBracketMatches") ?? "Loser Bracket - Spiele")
                : (_localizationService?.GetString("WinnerBracketMatches") ?? "Winner Bracket - Spiele");
            var titleBlock = new TextBlock
            {
                Text = titleText,
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
                    var noMatchesText = isLoserBracket 
                        ? (_localizationService?.GetString("NoLoserBracketGames") ?? "Keine Loser Bracket Spiele vorhanden.")
                        : (_localizationService?.GetString("NoWinnerBracketGames") ?? "Keine Winner Bracket Spiele vorhanden.");
                    var noMatchesBlock = new TextBlock
                    {
                        Text = noMatchesText,
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
                var bracketType = isLoserBracket ? "Loser" : "Winner";
                var errorText = $"FEHLER bei {bracketType} Bracket: {ex.Message}";
                var errorBlock = new TextBlock
                {
                    Text = errorText,
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

            // Header-Zeile mit lokalisierten Texten
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
                Child = grid
            };
        }

        private FrameworkElement CreateMatchesTable(List<Match> matches)
        {
            // ? QR-Code Verfügbarkeit prüfen
    bool showQRCodes = _qrCodeHelper?.AreQRCodesAvailable ?? false;
 
         System.Diagnostics.Debug.WriteLine($"[CreateMatchesTable] Creating table for {matches.Count} matches");
            System.Diagnostics.Debug.WriteLine($"[CreateMatchesTable] QR Codes available: {showQRCodes}");

            var grid = new Grid
            {
       Background = Brushes.White,
       MaxWidth = showQRCodes ? 850 : 700
     };

            // Spalten definieren - mit QR-Code Spalte wenn verfügbar
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(showQRCodes ? 180 : 200) });
   grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
      grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
       grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
    
if (showQRCodes)
    {
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // QR-Code Spalte
            }

            // Header-Zeile
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
    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(showQRCodes ? 105 : 25) });
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
       FontSize = 10,
         TextAlignment = col == 1 ? TextAlignment.Left : TextAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
        TextTrimming = TextTrimming.CharacterEllipsis
            };

          cellBorder.Child = cellText;
        Grid.SetRow(cellBorder, row + 1);
         Grid.SetColumn(cellBorder, col);
          grid.Children.Add(cellBorder);
      }
   
          // ? QR-Code Zelle hinzufügen wenn verfügbar
                if (showQRCodes)
       {
            var qrCellBorder = new Border
 {
 Background = row % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
               BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                 BorderThickness = new Thickness(0.5),
       Padding = new Thickness(2)
         };
         
     System.Diagnostics.Debug.WriteLine($"[CreateMatchesTable] Generating QR for match {match.Id}, UniqueId: {match.UniqueId}");
      var qrImage = _qrCodeHelper?.GenerateMatchQRCode(match, 5);
     System.Diagnostics.Debug.WriteLine($"[CreateMatchesTable] QR Image generated: {qrImage != null}");
             
           if (qrImage != null)
        {
  var image = new System.Windows.Controls.Image
     {
              Source = qrImage,
    Width = 100,
     Height = 100,
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

        private FrameworkElement CreateKnockoutMatchesTable(List<KnockoutMatch> matches)
        {
    // ? QR-Code Verfügbarkeit prüfen
            bool showQRCodes = _qrCodeHelper?.AreQRCodesAvailable ?? false;
  
   System.Diagnostics.Debug.WriteLine($"[CreateKnockoutMatchesTable] Creating table for {matches.Count} matches");
   System.Diagnostics.Debug.WriteLine($"[CreateKnockoutMatchesTable] QR Codes available: {showQRCodes}");

            var grid = new Grid
            {
    Background = Brushes.White,
   MaxWidth = showQRCodes ? 900 : 750
  };

// Spalten definieren - mit QR-Code Spalte wenn verfügbar
   grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
  grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
         grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(showQRCodes ? 180 : 200) });
   grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
       grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
   grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            
            if (showQRCodes)
            {
     grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) }); // QR-Code Spalte
       }

            // Header-Zeile
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
           grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(showQRCodes ? 105 : 25) });
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
        FontSize = 10,
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
                
          // ? QR-Code Zelle hinzufügen wenn verfügbar
             if (showQRCodes)
{
      var qrCellBorder = new Border
             {
  Background = row % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(248, 249, 250)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
        BorderThickness = new Thickness(0.5),
            Padding = new Thickness(2)
          };
      
     System.Diagnostics.Debug.WriteLine($"[CreateKnockoutMatchesTable] Generating QR for match {match.Id}, UniqueId: {match.UniqueId}");
         var qrImage = _qrCodeHelper?.GenerateMatchQRCode(match, 5);
      System.Diagnostics.Debug.WriteLine($"[CreateKnockoutMatchesTable] QR Image generated: {qrImage != null}");
           
            if (qrImage != null)
    {
      var image = new System.Windows.Controls.Image
   {
       Source = qrImage,
     Width = 100,
          Height = 100,
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