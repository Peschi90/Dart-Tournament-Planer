using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Helpers;
using DartTournamentPlaner.Services.Print;

namespace DartTournamentPlaner.Services
{
    /// <summary>
    /// Service für Druckoperationen von Turnierstatistiken
    /// REFACTORED: Verwendet spezialisierte Helper-Klassen für bessere Wartbarkeit
    /// </summary>
    public class PrintService
    {
        private readonly LocalizationService? _localizationService;
        private readonly PrintQRCodeHelper? _qrCodeHelper;
        private readonly PrintLayoutHelper _layoutHelper;
        private readonly PrintTableFactory _tableFactory;
        private readonly PrintPageFactory _pageFactory;

        public PrintService(LocalizationService? localizationService = null, HubIntegrationService? hubService = null)
        {
            _localizationService = localizationService;
    
            if (hubService != null)
     {
         _qrCodeHelper = new PrintQRCodeHelper(hubService);
           System.Diagnostics.Debug.WriteLine($"[PrintService] QR Code Helper initialized - Available: {_qrCodeHelper.AreQRCodesAvailable}");
      }
  
            _layoutHelper = new PrintLayoutHelper(_localizationService, _qrCodeHelper);
            _tableFactory = new PrintTableFactory(_localizationService, _qrCodeHelper);
            _pageFactory = new PrintPageFactory(_localizationService, _layoutHelper, _tableFactory);
        }

        public bool PrintTournamentStatistics(TournamentClass tournamentClass, TournamentPrintOptions printOptions)
        {
  try
 {
         var printDialog = new PrintDialog();
   
       if (printOptions.ShowPrintDialog && printDialog.ShowDialog() != true)
  return false;

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

        public FrameworkElement? CreatePrintPreview(TournamentClass tournamentClass, TournamentPrintOptions printOptions)
      {
  try
   {
      var document = CreateTournamentDocument(tournamentClass, printOptions);
      if (document == null) return null;

           return new DocumentViewer
        {
             Document = document,
  Background = Brushes.White
        };
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
            
            bool showQRCodes = _qrCodeHelper?.AreQRCodesAvailable ?? false;
        printTicket.PageOrientation = showQRCodes ? PageOrientation.Landscape : PageOrientation.Portrait;
            
     System.Diagnostics.Debug.WriteLine($"[ConfigurePrintSettings] QR Codes: {showQRCodes}, Orientation: {printTicket.PageOrientation}");
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
    
       // ? KORRIGIERT: Berechne die tatsächliche Y-Position nach Header, Details, Tabelle und Match-Title
        double yPositionAfterFirstPage = PrintLayoutHelper.MARGIN_TOP  // 90
   + 70   // Group Details
        + (group.Players.Any() ? 245 : 0)  // Standings Table (45 title + 200 table) oder 0
    + 50;  // Match Results Title
        
     var additionalPages = _pageFactory.CreateAdditionalGroupMatchPages(group, tournamentClass, yPositionAfterFirstPage);
  foreach (var additionalPage in additionalPages)
      {
 document.Pages.Add(additionalPage);
          }
     }
            }
        }

            if (printOptions.IncludeFinalsPhase && 
           tournamentClass.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
    {
       var finalsPage = CreateFinalsPage(tournamentClass, printOptions);
         if (finalsPage != null)
document.Pages.Add(finalsPage);
       
       // ? KORRIGIERT: Berechne die tatsächliche Y-Position nach Header, Tabelle und Match-Title
  double yPositionAfterFirstPage = PrintLayoutHelper.MARGIN_TOP  // 90
         + (tournamentClass.CurrentPhase?.FinalsGroup != null ? 245 : 0)  // Standings Table oder 0
+ 50;  // Match Results Title
    
   var additionalFinalsPages = _pageFactory.CreateAdditionalFinalsMatchPages(tournamentClass, yPositionAfterFirstPage);
         foreach (var additionalPage in additionalFinalsPages)
     {
  document.Pages.Add(additionalPage);
   }
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
     var fixedPage = _layoutHelper.CreateFixedPage();

       var pageTitle = _localizationService?.GetString("TournamentOverviewPrint", tournamentClass.Name) ?? $"Turnierübersicht - {tournamentClass.Name}";
    _layoutHelper.AddPageHeader(fixedPage, pageTitle);
         
          double yPosition = PrintLayoutHelper.MARGIN_TOP;
           yPosition = AddTournamentInfo(fixedPage, tournamentClass, yPosition);
    
   if (tournamentClass.Groups.Any())
           {
  yPosition = AddGroupsOverview(fixedPage, tournamentClass, yPosition);
    }

           _layoutHelper.AddPageFooter(fixedPage);
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
        var fixedPage = _layoutHelper.CreateFixedPage();

      _layoutHelper.AddPageHeader(fixedPage, $"{tournamentClass.Name} - {group.Name}");
                double yPosition = PrintLayoutHelper.MARGIN_TOP;
        
       yPosition = AddGroupDetails(fixedPage, group, yPosition);

     if (group.Players.Any())
         {
 yPosition = AddStandingsTable(fixedPage, group, yPosition);
     }

              if (group.Matches.Any() && yPosition < _layoutHelper.GetPageHeight() - 200)
        {
         yPosition = AddMatchResults(fixedPage, group, yPosition);
       }

      _layoutHelper.AddPageFooter(fixedPage);
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
      var fixedPage = _layoutHelper.CreateFixedPage();

       var pageTitle = $"{tournamentClass.Name} - {_localizationService?.GetString("FinalsRound") ?? "Finalrunde"}";
           _layoutHelper.AddPageHeader(fixedPage, pageTitle);
       double yPosition = PrintLayoutHelper.MARGIN_TOP;

       if (tournamentClass.CurrentPhase?.FinalsGroup != null)
          {
     yPosition = AddStandingsTable(fixedPage, tournamentClass.CurrentPhase.FinalsGroup, yPosition);
   }

   var finalsMatches = tournamentClass.GetFinalsMatches();
 if (finalsMatches.Any() && yPosition < _layoutHelper.GetPageHeight() - 200)
         {
         yPosition = AddMatchResults(fixedPage, tournamentClass.CurrentPhase.FinalsGroup, yPosition);
      }

           _layoutHelper.AddPageFooter(fixedPage);
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
      
 // ? KORRIGIERT: Berechne die tatsächliche Y-Position nach Header und Match-Title
     double yPositionAfterFirstPage = PrintLayoutHelper.MARGIN_TOP  // 90
    + 50;  // Match Title
        
     var additionalWinnerPages = _pageFactory.CreateAdditionalKnockoutMatchPages(tournamentClass, false, yPositionAfterFirstPage);
  foreach (var additionalPage in additionalWinnerPages)
   {
    pages.Add(additionalPage);
 }
   }

          if (printOptions.IncludeLoserBracket && tournamentClass.GetLoserBracketMatches().Any())
    {
     var loserPage = CreateKnockoutBracketPage(tournamentClass, true, printOptions);
        if (loserPage != null)
       pages.Add(loserPage);
 
  // ? KORRIGIERT: Berechne die tatsächliche Y-Position nach Header und Match-Title
            double yPositionAfterFirstPage = PrintLayoutHelper.MARGIN_TOP  // 90
   + 50;  // Match Title
   
   var additionalLoserPages = _pageFactory.CreateAdditionalKnockoutMatchPages(tournamentClass, true, yPositionAfterFirstPage);
   foreach (var additionalPage in additionalLoserPages)
    {
          pages.Add(additionalPage);
 }
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
                var fixedPage = _layoutHelper.CreateFixedPage();

     var pageTitle = isLoserBracket
              ? _localizationService?.GetString("LoserBracketHeader", tournamentClass.Name) ?? $"{tournamentClass.Name} - Loser Bracket"
: _localizationService?.GetString("WinnerBracketHeader", tournamentClass.Name) ?? $"{tournamentClass.Name} - Winner Bracket";
      
                _layoutHelper.AddPageHeader(fixedPage, pageTitle);
            double yPosition = PrintLayoutHelper.MARGIN_TOP;

                var matches = isLoserBracket ? 
        tournamentClass.GetLoserBracketMatches() : 
     tournamentClass.GetWinnerBracketMatches();

         yPosition = AddKnockoutMatches(fixedPage, matches.ToList(), yPosition, isLoserBracket);
          _layoutHelper.AddPageFooter(fixedPage);

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

        private double AddTournamentInfo(FixedPage page, TournamentClass tournamentClass, double yPosition)
        {
            var infoPanel = new StackPanel { Orientation = Orientation.Vertical };

         var gameRulesText = _localizationService?.GetString("GameRulesLabel", tournamentClass.GameRules) ?? $"Spielregeln: {tournamentClass.GameRules}";
    infoPanel.Children.Add(new TextBlock { Text = gameRulesText, FontSize = 14, Margin = new Thickness(0, 8, 0, 0) });

       var currentPhaseName = tournamentClass.CurrentPhase?.Name ?? (_localizationService?.GetString("NotStarted") ?? "Nicht begonnen");
       var currentPhaseText = _localizationService?.GetString("CurrentPhaseLabel", currentPhaseName) ?? $"Aktuelle Phase: {currentPhaseName}";
    infoPanel.Children.Add(new TextBlock { Text = currentPhaseText, FontSize = 14, Margin = new Thickness(0, 8, 0, 0) });

            var totalPlayers = tournamentClass.Groups.SelectMany(g => g.Players).Count();
         var groupsPlayersText = _localizationService?.GetString("GroupsPlayersLabel", tournamentClass.Groups.Count, totalPlayers) ?? $"Gruppen: {tournamentClass.Groups.Count}, Spieler gesamt: {totalPlayers}";
            infoPanel.Children.Add(new TextBlock { Text = groupsPlayersText, FontSize = 14, Margin = new Thickness(0, 8, 0, 0) });

         FixedPage.SetLeft(infoPanel, PrintLayoutHelper.MARGIN_LEFT);
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

       FixedPage.SetLeft(titleBlock, PrintLayoutHelper.MARGIN_LEFT);
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

           FixedPage.SetLeft(groupInfo, PrintLayoutHelper.MARGIN_LEFT);
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
     detailsPanel.Children.Add(new TextBlock { Text = playersText, FontSize = 14, Margin = new Thickness(0, 8, 0, 0) });

            var finishedMatches = group.Matches.Count(m => m.Status == MatchStatus.Finished);
      var totalMatches = group.Matches.Count;
    var matchesText = _localizationService?.GetString("MatchesStatus", finishedMatches, totalMatches) ?? $"Spiele: {finishedMatches} von {totalMatches} beendet";
   detailsPanel.Children.Add(new TextBlock { Text = matchesText, FontSize = 14, Margin = new Thickness(0, 8, 0, 0) });

            FixedPage.SetLeft(detailsPanel, PrintLayoutHelper.MARGIN_LEFT);
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

        FixedPage.SetLeft(titleBlock, PrintLayoutHelper.MARGIN_LEFT);
     FixedPage.SetTop(titleBlock, yPosition);
            page.Children.Add(titleBlock);
       yPosition += 45;

            try
         {
  var standings = group.GetStandings();
 if (standings != null && standings.Any())
      {
        var table = _tableFactory.CreateStandingsTable(standings);
          FixedPage.SetLeft(table, PrintLayoutHelper.MARGIN_LEFT);
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
      
    FixedPage.SetLeft(noStandingsBlock, PrintLayoutHelper.MARGIN_LEFT);
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
                
    FixedPage.SetLeft(errorBlock, PrintLayoutHelper.MARGIN_LEFT);
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

            FixedPage.SetLeft(titleBlock, PrintLayoutHelper.MARGIN_LEFT);
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

 // ? KORRIGIERT: Berechne wie viele Matches auf diese Seite passen
          double availableSpace = _layoutHelper.GetPageHeight() - yPosition - PrintLayoutHelper.MARGIN_BOTTOM;
   int maxMatches = _layoutHelper.CalculateMaxMatchesPerPage(availableSpace);
         
    // Nur so viele Matches anzeigen wie Platz ist
 var matchesToShow = sortedMatches.Take(Math.Max(1, maxMatches)).ToList();
   
           System.Diagnostics.Debug.WriteLine($"[AddMatchResults] Available space: {availableSpace}px, Max matches: {maxMatches}, Showing: {matchesToShow.Count}/{sortedMatches.Count}");
        
         var matchesTable = _tableFactory.CreateMatchesTable(matchesToShow);
        FixedPage.SetLeft(matchesTable, PrintLayoutHelper.MARGIN_LEFT);
               FixedPage.SetTop(matchesTable, yPosition);
  page.Children.Add(matchesTable);

                    return yPosition + Math.Max(250, matchesToShow.Count * (_layoutHelper.GetEstimatedRowHeight()) + 100);
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

  FixedPage.SetLeft(noMatchesBlock, PrintLayoutHelper.MARGIN_LEFT);
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

        FixedPage.SetLeft(errorBlock, PrintLayoutHelper.MARGIN_LEFT);
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

  FixedPage.SetLeft(titleBlock, PrintLayoutHelper.MARGIN_LEFT);
      FixedPage.SetTop(titleBlock, yPosition);
 page.Children.Add(titleBlock);
 yPosition += 50;

            try
 {
    if (matches.Any())
          {
     // ? KORRIGIERT: Berechne wie viele Matches auf diese Seite passen
        double availableSpace = _layoutHelper.GetPageHeight() - yPosition - PrintLayoutHelper.MARGIN_BOTTOM;
        int maxMatches = _layoutHelper.CalculateMaxMatchesPerPage(availableSpace);
    
     // Nur so viele Matches anzeigen wie Platz ist
 var matchesToShow = matches.Take(Math.Max(1, maxMatches)).ToList();
    
     System.Diagnostics.Debug.WriteLine($"[AddKnockoutMatches] {(isLoserBracket ? "Loser" : "Winner")} Bracket - Available space: {availableSpace}px, Max matches: {maxMatches}, Showing: {matchesToShow.Count}/{matches.Count}");

       var table = _tableFactory.CreateKnockoutMatchesTable(matchesToShow);
  FixedPage.SetLeft(table, PrintLayoutHelper.MARGIN_LEFT);
        FixedPage.SetTop(table, yPosition);
  page.Children.Add(table);

  return yPosition + Math.Max(300, matchesToShow.Count * (_layoutHelper.GetEstimatedRowHeight()) + 120);
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

   FixedPage.SetLeft(noMatchesBlock, PrintLayoutHelper.MARGIN_LEFT);
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

    FixedPage.SetLeft(errorBlock, PrintLayoutHelper.MARGIN_LEFT);
      FixedPage.SetTop(errorBlock, yPosition);
        page.Children.Add(errorBlock);

         return yPosition + 60;
     }
        }

        #endregion
    }

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