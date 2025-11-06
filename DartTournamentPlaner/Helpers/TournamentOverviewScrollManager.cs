using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Vereinfachter Manager für automatisches Scrollen in DataGrids und ScrollViewern
/// Scrollt parallel zum Tab-Cycling über die gesamte Cycle-Zeit
/// ✅ ERWEITERT: Unterstützt DataGrids UND direkte ScrollViewer (z.B. für TreeViews)
/// </summary>
public class TournamentOverviewScrollManager
{
    private readonly DispatcherTimer _scrollTimer;
    private readonly Func<object?> _getCurrentActiveContent; // ✅ GEÄNDERT: Generic content statt nur DataGrid
    private readonly Action _updateStatus;
    
    private bool _isScrolling = false;
    private DateTime _scrollStartTime;
    private ScrollViewer? _currentScrollViewer;
    private double _targetScrollHeight;
    private double _scrollDuration = 5.0; // Standard-Dauer, wird dynamisch angepasst
    
    // Scroll-Parameter
    private readonly double _pauseAtEnd = 0.5; // Kurze Pause am Ende (0.5 Sekunden)

    public bool IsScrolling => _isScrolling;

    public TournamentOverviewScrollManager(
        Func<DataGrid?> getCurrentActiveDataGrid, // Original parameter für Kompatibilität
        Action onScrollCompleted, // Nicht mehr verwendet
        Action updateStatus)
    {
    // ✅ WRAPPER: Konvertiere DataGrid-Func zu generic content-Func
        _getCurrentActiveContent = () => getCurrentActiveDataGrid() as object;
        _updateStatus = updateStatus;
        
        _scrollTimer = new DispatcherTimer();
        _scrollTimer.Interval = TimeSpan.FromMilliseconds(50); // 50ms für flüssige Animation
        _scrollTimer.Tick += ScrollTimer_Tick;
    }

    /// <summary>
    /// Startet das automatische Scrollen für die aktuelle Ansicht
    /// Läuft parallel zum Tab-Cycling und blockiert es nicht
    /// ✅ ERWEITERT: Unterstützt DataGrids UND ScrollViewer
    /// ✅ KORRIGIERT: Scrollt auch bei minimalem scrollbarem Inhalt (>= 1px)
    /// ✅ NEU: Intelligente Scroll-Strategie basierend auf ScrollableHeight
    /// </summary>
    /// <param name="cycleDuration">Gesamte Cycle-Zeit in Sekunden für diesen Tab</param>
    public void StartScrolling(double cycleDuration = 5.0)
    {
        try
        {
  // Stoppe vorheriges Scrollen falls noch aktiv
   if (_isScrolling)
         {
                StopScrolling();
        }

// ✅ NEU: Finde ScrollViewer direkt oder in DataGrid
      var scrollViewer = FindCurrentScrollViewer();
            
            if (scrollViewer == null)
          {
    System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No ScrollViewer found in current tab");
                return;
      }

      // Umfangreiche Debug-Informationen
   var scrollableHeight = scrollViewer.ScrollableHeight;
       var extentHeight = scrollViewer.ExtentHeight;
var viewportHeight = scrollViewer.ViewportHeight;
            
 System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] ScrollViewer Analysis:");
        System.Diagnostics.Debug.WriteLine($"  - ScrollableHeight: {scrollableHeight:F1}px");
            System.Diagnostics.Debug.WriteLine($"  - ExtentHeight: {extentHeight:F1}px");
          System.Diagnostics.Debug.WriteLine($"  - ViewportHeight: {viewportHeight:F1}px");
     System.Diagnostics.Debug.WriteLine($"  - Cycle Duration: {cycleDuration}s");

  // ✅ KORRIGIERT: Intelligentere Scroll-Entscheidung - scrolle JEDEN Pixel!
        var shouldScroll = false;
            var reason = "";

    if (scrollableHeight >= 1.0) // ✅ GEÄNDERT: Minimum 1px ScrollableHeight (vorher 20px)
       {
     shouldScroll = true;
reason = $"ScrollableHeight ({scrollableHeight:F1}px) >= 1px";
       }
     else
            {
 // Versuche Layout-Update
     scrollViewer.UpdateLayout();
             var newScrollableHeight = scrollViewer.ScrollableHeight;
              
           if (newScrollableHeight >= 1.0) // ✅ GEÄNDERT: Auch hier 1px statt 20px
            {
     scrollableHeight = newScrollableHeight;
           shouldScroll = true;
      reason = $"ScrollableHeight after UpdateLayout ({scrollableHeight:F1}px) >= 1px";
       }
         else
     {
            reason = $"ScrollableHeight ({scrollableHeight:F1}px) < 1px (no content to scroll)";
  System.Diagnostics.Debug.WriteLine($"🚫 [AutoScroll] No scrolling needed - {reason}");
      return;
    }
       }

         // ✅ NEU: Intelligente Scroll-Strategie basierend auf ScrollableHeight
            const double SMALL_SCROLL_THRESHOLD = 10.0; // Pixel-Schwellwert für "kleinen" Scroll
          
    if (scrollableHeight < SMALL_SCROLL_THRESHOLD)
            {
      // ✅ STRATEGIE 1: Kleiner Scroll (< 10px) → Verzögerter Single-Scroll zur Hälfte der Zeit
   _scrollDuration = 0; // Kein animiertes Scrollen
     _targetScrollHeight = scrollableHeight;
    _isScrolling = true;
   _scrollStartTime = DateTime.Now;
     _currentScrollViewer = scrollViewer;
          
         // Starte bei Position 0
   scrollViewer.ScrollToVerticalOffset(0);
  
     // Scroll zur Hälfte der Cycle-Zeit
          var delayUntilScroll = cycleDuration / 2.0;
  
      System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] SMALL SCROLL MODE - {reason}");
  System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] Will scroll {scrollableHeight:F1}px after {delayUntilScroll:F1}s (half of {cycleDuration}s cycle)");
                System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] Strategy: Delayed single-scroll (content too small for smooth animation)");
   
           // Verwende Timer für verzögerten Scroll
       Task.Delay(TimeSpan.FromSeconds(delayUntilScroll)).ContinueWith(_ =>
      {
 if (_isScrolling && _currentScrollViewer != null)
         {
 Application.Current.Dispatcher.BeginInvoke(() =>
                {
         _currentScrollViewer.ScrollToVerticalOffset(_targetScrollHeight);
   System.Diagnostics.Debug.WriteLine($"⬇️ [AutoScroll] Executed delayed scroll to {_targetScrollHeight:F1}px");
            });
     }
           });
             
      // Kein Timer nötig - Scroll ist einmalig
  return;
    }
            else
       {
      // ✅ STRATEGIE 2: Großer Scroll (>= 10px) → Animiertes Scrollen über gesamte Zeit
  _scrollDuration = Math.Max(1.0, cycleDuration - _pauseAtEnd);
         _isScrolling = true;
      _scrollStartTime = DateTime.Now;
      _currentScrollViewer = scrollViewer;
         _targetScrollHeight = scrollableHeight;
  
              // Scroll zurück zum Anfang
   scrollViewer.ScrollToVerticalOffset(0);
 
            _scrollTimer.Start();
         
        System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] SMOOTH SCROLL MODE - {reason}");
   System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] Target Height: {_targetScrollHeight:F1}px, Duration: {_scrollDuration:F1}s (Cycle: {cycleDuration}s)");
   System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] Scroll Speed: {_targetScrollHeight / _scrollDuration:F2} px/s");
System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] Strategy: Smooth animated scroll with easing");
            }
    }
        catch (Exception ex)
        {
         System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error starting scroll: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ NEU: Findet den aktuellen ScrollViewer (entweder direkt im Content oder in einem DataGrid)
    /// ✅ ERWEITERT: Mit detaillierter Debug-Ausgabe zur Identifikation des richtigen ScrollViewers
    /// </summary>
    private ScrollViewer? FindCurrentScrollViewer()
    {
   try
      {
            // Hole aktuellen Content
            var currentContent = _getCurrentActiveContent();
 
            if (currentContent == null)
     {
        System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No current content");
   return null;
        }

            System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Current content type: {currentContent.GetType().Name}");

       // Fall 1: Content ist bereits ein ScrollViewer
        if (currentContent is ScrollViewer directScrollViewer)
 {
           System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] Found direct ScrollViewer");
     System.Diagnostics.Debug.WriteLine($"    └─ Name: {directScrollViewer.Name ?? "(unnamed)"}");
  System.Diagnostics.Debug.WriteLine($"    └─ Content: {directScrollViewer.Content?.GetType().Name ?? "null"}");
    return directScrollViewer;
  }

      // Fall 2: Content ist ein DataGrid - suche ScrollViewer darin
  if (currentContent is DataGrid dataGrid)
            {
    System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Analyzing DataGrid with {dataGrid.Items.Count} items");
         
    // Optimiere DataGrid für Scrolling
    OptimizeDataGridForScrolling(dataGrid);
            
       // ✅ NEU: Suche alle ScrollViewer im DataGrid und analysiere sie
         var allScrollViewers = FindAllVisualChildren<ScrollViewer>(dataGrid).ToList();
             System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found {allScrollViewers.Count} ScrollViewer(s) in DataGrid:");
      
    foreach (var sv in allScrollViewers)
       {
           System.Diagnostics.Debug.WriteLine($"    ├─ ScrollViewer: Name={sv.Name ?? "(unnamed)"}, " +
     $"ScrollableHeight={sv.ScrollableHeight:F1}px, " +
            $"Orientation={sv.VerticalScrollBarVisibility}");
       }
        
   // Nimm den ersten ScrollViewer (sollte der Haupt-ScrollViewer des DataGrids sein)
        var scrollViewer = allScrollViewers.FirstOrDefault();
 if (scrollViewer != null)
    {
          System.Diagnostics.Debug.WriteLine($"✅ [AutoScroll] Using first ScrollViewer in DataGrid");
     System.Diagnostics.Debug.WriteLine($"    └─ VerticalOffset: {scrollViewer.VerticalOffset:F1}px");
      System.Diagnostics.Debug.WriteLine($"    └─ ScrollableHeight: {scrollViewer.ScrollableHeight:F1}px");
       System.Diagnostics.Debug.WriteLine($"    └─ ExtentHeight: {scrollViewer.ExtentHeight:F1}px");
        System.Diagnostics.Debug.WriteLine($"    └─ ViewportHeight: {scrollViewer.ViewportHeight:F1}px");
     return scrollViewer;
       }
     }

     // Fall 3: Suche recursiv in der Visual Tree
       if (currentContent is DependencyObject dependencyObject)
   {
          System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Searching recursively in {dependencyObject.GetType().Name}");
                
    // ✅ NEU: Finde alle ScrollViewer und analysiere sie
          var allScrollViewers = FindAllVisualChildren<ScrollViewer>(dependencyObject).ToList();
       System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] Found {allScrollViewers.Count} ScrollViewer(s) recursively:");
       
        foreach (var sv in allScrollViewers)
        {
         System.Diagnostics.Debug.WriteLine($"    ├─ ScrollViewer: Name={sv.Name ?? "(unnamed)"}, " +
              $"ScrollableHeight={sv.ScrollableHeight:F1}px, " +
            $"Parent={GetParentTypeName(sv)}");
      }
     
         // ✅ VERBESSERT: Wähle den ScrollViewer mit der größten ScrollableHeight
     // Das ist wahrscheinlich der Haupt-Content-ScrollViewer, nicht ein interner
        var scrollViewer = allScrollViewers
      .OrderByDescending(sv => sv.ScrollableHeight)
            .FirstOrDefault();
        
   if (scrollViewer != null)
          {
 System.Diagnostics.Debug.WriteLine($"✅ [AutoScroll] Using ScrollViewer with largest ScrollableHeight");
  System.Diagnostics.Debug.WriteLine($"    └─ Name: {scrollViewer.Name ?? "(unnamed)"}");
    System.Diagnostics.Debug.WriteLine($"    └─ ScrollableHeight: {scrollViewer.ScrollableHeight:F1}px");
         System.Diagnostics.Debug.WriteLine($"  └─ Parent: {GetParentTypeName(scrollViewer)}");
  return scrollViewer;
       }
    }

      System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No ScrollViewer found");
      return null;
        }
        catch (Exception ex)
        {
System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error finding ScrollViewer: {ex.Message}");
       return null;
        }
    }

    /// <summary>
    /// ✅ NEU: Findet ALLE ScrollViewer in einem Element (nicht nur den ersten)
    /// </summary>
    private IEnumerable<ScrollViewer> FindAllVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
     {
       var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
          
            if (child is T typedChild)
           yield return (ScrollViewer)(object)typedChild;

        foreach (var childOfChild in FindAllVisualChildren<T>(child))
            {
           yield return (ScrollViewer)(object)childOfChild;
            }
        }
    }

    /// <summary>
    /// ✅ NEU: Hilfsmethode um den Parent-Type-Namen zu bekommen
    /// </summary>
    private string GetParentTypeName(DependencyObject element)
    {
        try
 {
 var parent = VisualTreeHelper.GetParent(element);
      return parent?.GetType().Name ?? "null";
        }
        catch
     {
            return "unknown";
    }
    }

    /// <summary>
    /// Optimiert ein DataGrid für besseres Scrolling
/// </summary>
    private void OptimizeDataGridForScrolling(DataGrid dataGrid)
    {
  try
        {
    // Stelle sicher, dass Virtualization aktiviert ist für Performance
            if (!dataGrid.EnableRowVirtualization)
        {
     dataGrid.EnableRowVirtualization = true;
                System.Diagnostics.Debug.WriteLine("🔧 [AutoScroll] Enabled row virtualization");
        }

      // Stelle sicher, dass das DataGrid eine vernünftige Mindesthöhe hat
            if (double.IsNaN(dataGrid.Height) && double.IsNaN(dataGrid.MinHeight))
       {
            var itemCount = dataGrid.Items?.Count ?? 0;
          if (itemCount > 10)
                {
         // Setze eine MinHeight basierend auf Item-Anzahl
        var estimatedHeight = Math.Min(400, Math.Max(200, itemCount * 25));
 dataGrid.MinHeight = estimatedHeight;
 System.Diagnostics.Debug.WriteLine($"🔧 [AutoScroll] Set MinHeight to {estimatedHeight}px for {itemCount} items");
      }
      }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error optimizing DataGrid: {ex.Message}");
        }
    }

    /// <summary>
    /// Beendet das automatische Scrollen
    /// </summary>
    public void StopScrolling()
    {
        if (!_isScrolling) return;

  _scrollTimer.Stop();
  _isScrolling = false;
        _currentScrollViewer = null;
        
      System.Diagnostics.Debug.WriteLine("✅ [AutoScroll] Scrolling stopped");
  }

/// <summary>
    /// Scrollt den aktuellen Content zurück zum Anfang
    /// </summary>
    public void ResetScrollPosition()
    {
        try
        {
            var scrollViewer = FindCurrentScrollViewer();
       scrollViewer?.ScrollToVerticalOffset(0);
        }
        catch (Exception ex)
        {
 System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error resetting scroll: {ex.Message}");
        }
    }

    /// <summary>
    /// Timer für automatisches Scrollen - läuft unabhängig vom Tab-Cycling
    /// </summary>
    private void ScrollTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isScrolling || _currentScrollViewer == null) 
        {
          StopScrolling();
       return;
        }

 try
        {
  var elapsedTime = (DateTime.Now - _scrollStartTime).TotalSeconds;
    var totalDuration = _scrollDuration + _pauseAtEnd;

     if (elapsedTime < _scrollDuration)
   {
         // Scroll-Phase: Gleichmäßig nach unten scrollen über die gesamte Cycle-Zeit
                var progress = elapsedTime / _scrollDuration;

                // ✅ VERBESSERT: Easing-Funktion für sanfteres Scrollen
   // Verwendet eine ease-in-out Kurve für natürlichere Bewegung
      var easedProgress = EaseInOutCubic(progress);
       
             var targetPosition = _targetScrollHeight * easedProgress;
      _currentScrollViewer.ScrollToVerticalOffset(targetPosition);
    
           // Prüfe ob sich die ScrollableHeight geändert hat (Layout-Updates)
 var currentScrollableHeight = _currentScrollViewer.ScrollableHeight;
         if (Math.Abs(currentScrollableHeight - _targetScrollHeight) > 1)
                {
    System.Diagnostics.Debug.WriteLine($"📐 [AutoScroll] ScrollableHeight updated during scroll: {_targetScrollHeight:F1} → {currentScrollableHeight:F1}px");
          _targetScrollHeight = currentScrollableHeight;
              }
       
     // Reduzierte Debug-Ausgabe - nur alle 2 Sekunden
                if ((int)elapsedTime % 2 == 0 && elapsedTime - Math.Floor(elapsedTime) < 0.1)
   {
     System.Diagnostics.Debug.WriteLine($"🔽 [AutoScroll] Scrolling: {progress:P0} - Position: {targetPosition:F0}/{_targetScrollHeight:F0}px - {_scrollDuration - elapsedTime:F0}s remaining");
           }
            }
   else if (elapsedTime < totalDuration)
       {
    // Pause-Phase: Am Ende verweilen
        _currentScrollViewer.ScrollToVerticalOffset(_targetScrollHeight);
       var remainingPause = totalDuration - elapsedTime;
     
    // Debug nur einmal ausgeben
         if (Math.Abs(remainingPause - _pauseAtEnd) < 0.1)
           {
          System.Diagnostics.Debug.WriteLine($"⏸️ [AutoScroll] Pausing at bottom: {remainingPause:F1}s remaining");
    }
          }
  else
            {
         // Scrollen abgeschlossen - automatisch stoppen
    System.Diagnostics.Debug.WriteLine($"✅ [AutoScroll] Scroll cycle completed after {elapsedTime:F1}s");
     StopScrolling();
            }
     
   _updateStatus?.Invoke();
  }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error during scroll: {ex.Message}");
     StopScrolling();
        }
    }

    /// <summary>
    /// Easing-Funktion für sanftere Scroll-Animation
    /// Implementiert eine cubic ease-in-out Kurve
    /// </summary>
    private double EaseInOutCubic(double t)
    {
return t < 0.5 
      ? 4 * t * t * t 
          : 1 - Math.Pow(-2 * t + 2, 3) / 2;
  }

    /// <summary>
    /// Hilfsmethode zur Suche von visuellen Child-Elementen
    /// </summary>
    private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
   {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
    
     if (child is T typedChild)
     return typedChild;

   var result = FindVisualChild<T>(child);
   if (result != null)
                return result;
   }

        return null;
    }

    public void Dispose()
    {
    StopScrolling();
    }
}