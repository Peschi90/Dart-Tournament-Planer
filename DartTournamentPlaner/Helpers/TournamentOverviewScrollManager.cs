using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Vereinfachter Manager für automatisches Scrollen in DataGrids
/// Scrollt parallel zum Tab-Cycling, ohne dieses zu blockieren
/// </summary>
public class TournamentOverviewScrollManager
{
    private readonly DispatcherTimer _scrollTimer;
    private readonly Func<DataGrid?> _getCurrentActiveDataGrid;
    private readonly Action _updateStatus;
    
    private bool _isScrolling = false;
    private DateTime _scrollStartTime;
    private ScrollViewer? _currentScrollViewer;
    private double _targetScrollHeight;
    
    // Scroll-Parameter (einfacher und robuster)
    private readonly double _scrollDuration = 3.0; // Sekunden für kompletten Scroll
    private readonly double _pauseDuration = 1.0; // Pause am Ende

    public bool IsScrolling => _isScrolling;

    public TournamentOverviewScrollManager(
        Func<DataGrid?> getCurrentActiveDataGrid,
        Action onScrollCompleted, // Nicht mehr verwendet - Scrollen blockiert Tab-Wechsel nicht
        Action updateStatus)
    {
        _getCurrentActiveDataGrid = getCurrentActiveDataGrid;
        _updateStatus = updateStatus;
        
        _scrollTimer = new DispatcherTimer();
        _scrollTimer.Interval = TimeSpan.FromMilliseconds(50); // 50ms für flüssige Animation
        _scrollTimer.Tick += ScrollTimer_Tick;
    }

    /// <summary>
    /// Startet das automatische Scrollen für die aktuelle Ansicht
    /// Läuft parallel zum Tab-Cycling und blockiert es nicht
    /// </summary>
    public void StartScrolling()
    {
        try
        {
            // Stoppe vorheriges Scrollen falls noch aktiv
            if (_isScrolling)
            {
                StopScrolling();
            }

            var currentDataGrid = _getCurrentActiveDataGrid();
            if (currentDataGrid == null) 
            {
                System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No DataGrid found");
                return;
            }

            // Optimiere DataGrid für Scrolling
            OptimizeDataGridForScrolling(currentDataGrid);

            var scrollViewer = FindScrollViewer(currentDataGrid);
            if (scrollViewer == null) 
            {
                System.Diagnostics.Debug.WriteLine("🔍 [AutoScroll] No ScrollViewer found in DataGrid");
                return;
            }

            // Umfangreiche Debug-Informationen
            var itemCount = currentDataGrid.Items?.Count ?? 0;
            var actualHeight = currentDataGrid.ActualHeight;
            var scrollableHeight = scrollViewer.ScrollableHeight;
            
            System.Diagnostics.Debug.WriteLine($"🔍 [AutoScroll] DataGrid Analysis:");
            System.Diagnostics.Debug.WriteLine($"  - Items: {itemCount}");
            System.Diagnostics.Debug.WriteLine($"  - ActualHeight: {actualHeight:F1}px");
            System.Diagnostics.Debug.WriteLine($"  - ScrollableHeight: {scrollableHeight:F1}px");
            System.Diagnostics.Debug.WriteLine($"  - ExtentHeight: {scrollViewer.ExtentHeight:F1}px");
            System.Diagnostics.Debug.WriteLine($"  - ViewportHeight: {scrollViewer.ViewportHeight:F1}px");

            // Intelligente Scroll-Entscheidung basierend auf Items UND ScrollableHeight
            var shouldScroll = false;
            var reason = "";

            if (scrollableHeight > 20) // Minimum 20px ScrollableHeight
            {
                shouldScroll = true;
                reason = $"ScrollableHeight ({scrollableHeight:F1}px) > 20px";
            }
            else if (itemCount >= 10) // Oder mindestens 10 Items (auch wenn ScrollableHeight klein ist)
            {
                shouldScroll = true;
                reason = $"ItemCount ({itemCount}) >= 10 items";
                
                // Forciere eine Aktualisierung der Layout-Eigenschaften
                currentDataGrid.UpdateLayout();
                scrollViewer.UpdateLayout();
                
                // Prüfe nochmal nach Layout-Update
                var newScrollableHeight = scrollViewer.ScrollableHeight;
                System.Diagnostics.Debug.WriteLine($"🔄 [AutoScroll] After UpdateLayout - ScrollableHeight: {newScrollableHeight:F1}px");
                
                if (newScrollableHeight > scrollableHeight)
                {
                    scrollableHeight = newScrollableHeight;
                    reason += $" (updated to {scrollableHeight:F1}px)";
                }
            }
            else if (itemCount >= 5) // Oder mindestens 5 Items als letzter Versuch
            {
                // Versuche das DataGrid zu "erweitern" für besseres Scrolling
                var originalHeight = currentDataGrid.Height;
                if (double.IsNaN(originalHeight) || originalHeight < 200)
                {
                    currentDataGrid.MinHeight = 300; // Minimale Höhe für Scrolling
                    currentDataGrid.UpdateLayout();
                    scrollViewer.UpdateLayout();
                    
                    var newScrollableHeight = scrollViewer.ScrollableHeight;
                    System.Diagnostics.Debug.WriteLine($"🔄 [AutoScroll] After MinHeight adjustment - ScrollableHeight: {newScrollableHeight:F1}px");
                    
                    if (newScrollableHeight > 5)
                    {
                        shouldScroll = true;
                        scrollableHeight = newScrollableHeight;
                        reason = $"ItemCount ({itemCount}) >= 5 items (forced MinHeight, ScrollableHeight: {scrollableHeight:F1}px)";
                    }
                }
            }

            if (!shouldScroll)
            {
                reason = $"ScrollableHeight ({scrollableHeight:F1}px) <= 20px AND ItemCount ({itemCount}) < 10";
                System.Diagnostics.Debug.WriteLine($"🚫 [AutoScroll] No scrolling needed - {reason}");
                return;
            }

            _isScrolling = true;
            _scrollStartTime = DateTime.Now;
            _currentScrollViewer = scrollViewer;
            _targetScrollHeight = scrollableHeight;
            
            // Scroll zurück zum Anfang
            scrollViewer.ScrollToVerticalOffset(0);
            
            _scrollTimer.Start();
            
            System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] Started scrolling - {reason}");
            System.Diagnostics.Debug.WriteLine($"🎬 [AutoScroll] Target Height: {_targetScrollHeight:F1}px, Duration: {_scrollDuration}s");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error starting scroll: {ex.Message}");
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
    /// Scrollt das aktuelle DataGrid zurück zum Anfang
    /// </summary>
    public void ResetScrollPosition()
    {
        try
        {
            var currentDataGrid = _getCurrentActiveDataGrid();
            if (currentDataGrid != null)
            {
                var scrollViewer = FindScrollViewer(currentDataGrid);
                scrollViewer?.ScrollToVerticalOffset(0);
            }
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
            var totalDuration = _scrollDuration + _pauseDuration;

            if (elapsedTime < _scrollDuration)
            {
                // Scroll-Phase: Gleichmäßig nach unten scrollen
                var progress = elapsedTime / _scrollDuration;
                var targetPosition = _targetScrollHeight * progress;
                _currentScrollViewer.ScrollToVerticalOffset(targetPosition);
                
                // Prüfe ob sich die ScrollableHeight geändert hat (Layout-Updates)
                var currentScrollableHeight = _currentScrollViewer.ScrollableHeight;
                if (Math.Abs(currentScrollableHeight - _targetScrollHeight) > 1)
                {
                    System.Diagnostics.Debug.WriteLine($"📐 [AutoScroll] ScrollableHeight updated during scroll: {_targetScrollHeight:F1} → {currentScrollableHeight:F1}px");
                    _targetScrollHeight = currentScrollableHeight;
                }
                
                System.Diagnostics.Debug.WriteLine($"🔽 [AutoScroll] Scrolling: {progress:P0} - Position: {targetPosition:F0}/{_targetScrollHeight:F0}px");
            }
            else if (elapsedTime < totalDuration)
            {
                // Pause-Phase: Am Ende verweilen
                _currentScrollViewer.ScrollToVerticalOffset(_targetScrollHeight);
                var remainingPause = totalDuration - elapsedTime;
                System.Diagnostics.Debug.WriteLine($"⏸️ [AutoScroll] Pausing at bottom: {remainingPause:F1}s remaining");
            }
            else
            {
                // Scrollen abgeschlossen - automatisch stoppen
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
    /// Findet den ScrollViewer eines DataGrids
    /// </summary>
    private ScrollViewer? FindScrollViewer(DataGrid dataGrid)
    {
        try
        {
            return FindVisualChild<ScrollViewer>(dataGrid);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [AutoScroll] Error finding ScrollViewer: {ex.Message}");
            return null;
        }
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