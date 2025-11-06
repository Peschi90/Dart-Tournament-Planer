using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services.Print
{
    /// <summary>
    /// Factory-Klasse für die Erstellung von Print-Seiten mit automatischem Multi-Page Support
    /// Verwaltet Overflow-Seiten wenn Matches nicht auf eine Seite passen
    /// </summary>
    public class PrintPageFactory
    {
        private readonly LocalizationService? _localizationService;
        private readonly PrintLayoutHelper _layoutHelper;
        private readonly PrintTableFactory _tableFactory;

        public PrintPageFactory(LocalizationService? localizationService, PrintLayoutHelper layoutHelper, PrintTableFactory tableFactory)
 {
         _localizationService = localizationService;
            _layoutHelper = layoutHelper;
        _tableFactory = tableFactory;
        }

        /// <summary>
        /// Erstellt zusätzliche Seiten für Group-Matches die nicht auf die Hauptseite passen
      /// </summary>
        public List<PageContent> CreateAdditionalGroupMatchPages(Group group, TournamentClass tournamentClass, double firstPageYPosition)
        {
            var pages = new List<PageContent>();

            try
         {
       double pageHeight = _layoutHelper.GetPageHeight();
                double estimatedRowHeight = _layoutHelper.GetEstimatedRowHeight();

         // Berechne Matches auf erster Seite (mit Tabelle und Details)
         double firstPageAvailableSpace = pageHeight - firstPageYPosition - PrintLayoutHelper.MARGIN_BOTTOM;
                int maxMatchesFirstPage = _layoutHelper.CalculateMaxMatchesPerPage(firstPageAvailableSpace);

            // Berechne Matches auf Folgeseiten (nur Matches, kein Table/Details)
            // Header (90) + Match-Titel (50) = 140px oben, dann MARGIN_BOTTOM (100) unten
        double followUpPageAvailableSpace = pageHeight - 140 - PrintLayoutHelper.MARGIN_BOTTOM;
       int maxMatchesPerFollowUpPage = _layoutHelper.CalculateMaxMatchesPerPage(followUpPageAvailableSpace);
 
            System.Diagnostics.Debug.WriteLine($"[CreateAdditionalGroupMatchPages] Page height: {pageHeight}px");
       System.Diagnostics.Debug.WriteLine($"[CreateAdditionalGroupMatchPages] First page: yPos={firstPageYPosition}px, available={firstPageAvailableSpace}px, max={maxMatchesFirstPage}");
       System.Diagnostics.Debug.WriteLine($"[CreateAdditionalGroupMatchPages] Follow-up pages: available={followUpPageAvailableSpace}px, max={maxMatchesPerFollowUpPage}");
           

        var allMatches = group.Matches
           .OrderBy(m => m.Status == MatchStatus.Finished || m.Status == MatchStatus.Bye ? 0 : 1)
        .ThenBy(m => m.Id)
    .ToList();

                // Überspringe die Matches die bereits auf der ersten Seite sind
      var remainingMatches = allMatches.Skip(maxMatchesFirstPage).ToList();

    if (!remainingMatches.Any())
      return pages;

         System.Diagnostics.Debug.WriteLine($"[CreateAdditionalGroupMatchPages] Creating overflow pages for {remainingMatches.Count} matches in {group.Name}");

    // Erstelle Folgeseiten
 int pageNumber = 2;
     while (remainingMatches.Any())
         {
          var matchesForThisPage = remainingMatches.Take(maxMatchesPerFollowUpPage).ToList();
              remainingMatches = remainingMatches.Skip(maxMatchesPerFollowUpPage).ToList();

   var pageContent = new PageContent();
        var fixedPage = _layoutHelper.CreateFixedPage();

                // Header mit Seitennummer
        _layoutHelper.AddPageHeader(fixedPage, $"{tournamentClass.Name} - {group.Name} (Seite {pageNumber})");
          double yPosition = PrintLayoutHelper.MARGIN_TOP;

                 // Matches Tabelle
      var titleText = _localizationService?.GetString("MatchResults") ?? "Spielergebnisse";
             var titleBlock = new TextBlock
            {
    Text = $"{titleText} (Fortsetzung)",
 FontSize = 18,
       FontWeight = FontWeights.Bold,
       Margin = new Thickness(0, 0, 0, 15)
        };

  FixedPage.SetLeft(titleBlock, PrintLayoutHelper.MARGIN_LEFT);
      FixedPage.SetTop(titleBlock, yPosition);
     fixedPage.Children.Add(titleBlock);
         yPosition += 40;

      var matchesTable = _tableFactory.CreateMatchesTable(matchesForThisPage);
      FixedPage.SetLeft(matchesTable, PrintLayoutHelper.MARGIN_LEFT);
  FixedPage.SetTop(matchesTable, yPosition);
          fixedPage.Children.Add(matchesTable);

          _layoutHelper.AddPageFooter(fixedPage);
  pageContent.Child = fixedPage;
          pages.Add(pageContent);

   pageNumber++;
          System.Diagnostics.Debug.WriteLine($"[CreateAdditionalGroupMatchPages] Created page {pageNumber - 1} with {matchesForThisPage.Count} matches");
      }
            }
          catch (Exception ex)
  {
                System.Diagnostics.Debug.WriteLine($"[CreateAdditionalGroupMatchPages] ERROR: {ex.Message}");
 }

  return pages;
     }

        /// <summary>
        /// Erstellt zusätzliche Seiten für Finals-Matches
   /// </summary>
    public List<PageContent> CreateAdditionalFinalsMatchPages(TournamentClass tournamentClass, double firstPageYPosition)
        {
      var pages = new List<PageContent>();

 try
    {
             if (tournamentClass.CurrentPhase?.FinalsGroup == null)
    return pages;

        double pageHeight = _layoutHelper.GetPageHeight();
      double estimatedRowHeight = _layoutHelper.GetEstimatedRowHeight();

          // Berechne Matches auf erster Seite (mit Tabelle)
        double firstPageAvailableSpace = pageHeight - firstPageYPosition - PrintLayoutHelper.MARGIN_BOTTOM;
    int maxMatchesFirstPage = _layoutHelper.CalculateMaxMatchesPerPage(firstPageAvailableSpace);

    // Berechne Matches auf Folgeseiten (nur Matches)
   // Header (90) + Match-Titel (50) = 140px oben, dann MARGIN_BOTTOM (100) unten
      double followUpPageAvailableSpace = pageHeight - 140 - PrintLayoutHelper.MARGIN_BOTTOM;
   int maxMatchesPerFollowUpPage = _layoutHelper.CalculateMaxMatchesPerPage(followUpPageAvailableSpace);
      
       System.Diagnostics.Debug.WriteLine($"[CreateAdditionalFinalsMatchPages] Page height: {pageHeight}px");
   System.Diagnostics.Debug.WriteLine($"[CreateAdditionalFinalsMatchPages] First page: yPos={firstPageYPosition}px, available={firstPageAvailableSpace}px, max={maxMatchesFirstPage}");
       System.Diagnostics.Debug.WriteLine($"[CreateAdditionalFinalsMatchPages] Follow-up pages: available={followUpPageAvailableSpace}px, max={maxMatchesPerFollowUpPage}");

                var allMatches = tournamentClass.GetFinalsMatches()
              .OrderBy(m => m.Status == MatchStatus.Finished || m.Status == MatchStatus.Bye ? 0 : 1)
    .ThenBy(m => m.Id)
      .ToList();

      var remainingMatches = allMatches.Skip(maxMatchesFirstPage).ToList();

       if (!remainingMatches.Any())
          return pages;

                System.Diagnostics.Debug.WriteLine($"[CreateAdditionalFinalsMatchPages] Creating overflow pages for {remainingMatches.Count} finals matches");

    int pageNumber = 2;
            while (remainingMatches.Any())
                {
      var matchesForThisPage = remainingMatches.Take(maxMatchesPerFollowUpPage).ToList();
            remainingMatches = remainingMatches.Skip(maxMatchesPerFollowUpPage).ToList();

         var pageContent = new PageContent();
       var fixedPage = _layoutHelper.CreateFixedPage();

  var pageTitle = $"{tournamentClass.Name} - {_localizationService?.GetString("FinalsRound") ?? "Finalrunde"} (Seite {pageNumber})";
         _layoutHelper.AddPageHeader(fixedPage, pageTitle);
              double yPosition = PrintLayoutHelper.MARGIN_TOP;

         var titleText = _localizationService?.GetString("MatchResults") ?? "Spielergebnisse";
            var titleBlock = new TextBlock
      {
     Text = $"{titleText} (Fortsetzung)",
FontSize = 18,
           FontWeight = FontWeights.Bold,
    Margin = new Thickness(0, 0, 0, 15)
    };

        FixedPage.SetLeft(titleBlock, PrintLayoutHelper.MARGIN_LEFT);
         FixedPage.SetTop(titleBlock, yPosition);
         fixedPage.Children.Add(titleBlock);
 yPosition += 40;

      var matchesTable = _tableFactory.CreateMatchesTable(matchesForThisPage);
        FixedPage.SetLeft(matchesTable, PrintLayoutHelper.MARGIN_LEFT);
      FixedPage.SetTop(matchesTable, yPosition);
       fixedPage.Children.Add(matchesTable);

          _layoutHelper.AddPageFooter(fixedPage);
           pageContent.Child = fixedPage;
    pages.Add(pageContent);

             pageNumber++;
        System.Diagnostics.Debug.WriteLine($"[CreateAdditionalFinalsMatchPages] Created page {pageNumber - 1} with {matchesForThisPage.Count} matches");
     }
  }
  catch (Exception ex)
            {
     System.Diagnostics.Debug.WriteLine($"[CreateAdditionalFinalsMatchPages] ERROR: {ex.Message}");
         }

     return pages;
      }

        /// <summary>
    /// Erstellt zusätzliche Seiten für Knockout-Matches (Winner oder Loser Bracket)
        /// </summary>
        public List<PageContent> CreateAdditionalKnockoutMatchPages(TournamentClass tournamentClass, bool isLoserBracket, double firstPageYPosition)
        {
       var pages = new List<PageContent>();

     try
     {
     double pageHeight = _layoutHelper.GetPageHeight();

      // Berechne Matches auf erster Seite
             double firstPageAvailableSpace = pageHeight - firstPageYPosition - PrintLayoutHelper.MARGIN_BOTTOM;
     int maxMatchesFirstPage = _layoutHelper.CalculateMaxMatchesPerPage(firstPageAvailableSpace);

            // Berechne Matches auf Folgeseiten
        // Header (90) + Match-Titel (50) = 140px oben, dann MARGIN_BOTTOM (100) unten
   double followUpPageAvailableSpace = pageHeight - 140 - PrintLayoutHelper.MARGIN_BOTTOM;
  int maxMatchesPerFollowUpPage = _layoutHelper.CalculateMaxMatchesPerPage(followUpPageAvailableSpace);

      var allMatches = (isLoserBracket
      ? tournamentClass.GetLoserBracketMatches()
  : tournamentClass.GetWinnerBracketMatches()).ToList();

  var remainingMatches = allMatches.Skip(maxMatchesFirstPage).ToList();

    if (!remainingMatches.Any())
   return pages;

    var bracketName = isLoserBracket ? "Loser Bracket" : "Winner Bracket";
       System.Diagnostics.Debug.WriteLine($"[CreateAdditionalKnockoutMatchPages] {bracketName} - Page height: {pageHeight}px");
 System.Diagnostics.Debug.WriteLine($"[CreateAdditionalKnockoutMatchPages] {bracketName} - First page: yPos={firstPageYPosition}px, available={firstPageAvailableSpace}px, max={maxMatchesFirstPage}");
     System.Diagnostics.Debug.WriteLine($"[CreateAdditionalKnockoutMatchPages] {bracketName} - Follow-up pages: available={followUpPageAvailableSpace}px, max={maxMatchesPerFollowUpPage}");
       System.Diagnostics.Debug.WriteLine($"[CreateAdditionalKnockoutMatchPages] Creating overflow pages for {remainingMatches.Count} {bracketName} matches");

   int pageNumber = 2;
      while (remainingMatches.Any())
             {
var matchesForThisPage = remainingMatches.Take(maxMatchesPerFollowUpPage).ToList();
      remainingMatches = remainingMatches.Skip(maxMatchesPerFollowUpPage).ToList();

    var pageContent = new PageContent();
 var fixedPage = _layoutHelper.CreateFixedPage();

  var pageTitle = isLoserBracket
         ? $"{tournamentClass.Name} - {_localizationService?.GetString("LoserBracket") ?? "Loser Bracket"} (Seite {pageNumber})"
               : $"{tournamentClass.Name} - {_localizationService?.GetString("WinnerBracket") ?? "Winner Bracket"} (Seite {pageNumber})";

     _layoutHelper.AddPageHeader(fixedPage, pageTitle);
            double yPosition = PrintLayoutHelper.MARGIN_TOP;

      var titleText = isLoserBracket
       ? (_localizationService?.GetString("LoserBracketMatches") ?? "Loser Bracket - Spiele")
     : (_localizationService?.GetString("WinnerBracketMatches") ?? "Winner Bracket - Spiele");
    
    var titleBlock = new TextBlock
      {
 Text = $"{titleText} (Fortsetzung)",
     FontSize = 18,
FontWeight = FontWeights.Bold,
       Margin = new Thickness(0, 0, 0, 15)
};

    FixedPage.SetLeft(titleBlock, PrintLayoutHelper.MARGIN_LEFT);
  FixedPage.SetTop(titleBlock, yPosition);
    fixedPage.Children.Add(titleBlock);
     yPosition += 40;

      var table = _tableFactory.CreateKnockoutMatchesTable(matchesForThisPage);
     FixedPage.SetLeft(table, PrintLayoutHelper.MARGIN_LEFT);
       FixedPage.SetTop(table, yPosition);
       fixedPage.Children.Add(table);

     _layoutHelper.AddPageFooter(fixedPage);
     pageContent.Child = fixedPage;
  pages.Add(pageContent);

             pageNumber++;
        System.Diagnostics.Debug.WriteLine($"[CreateAdditionalKnockoutMatchPages] Created {bracketName} page {pageNumber - 1} with {matchesForThisPage.Count} matches");
        }
            }
   catch (Exception ex)
 {
           System.Diagnostics.Debug.WriteLine($"[CreateAdditionalKnockoutMatchPages] ERROR: {ex.Message}");
    }

 return pages;
   }
    }
}
