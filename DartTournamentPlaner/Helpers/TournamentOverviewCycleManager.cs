using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Vereinfachter Manager für automatisches Tab-Cycling im TournamentOverviewWindow
/// Klare, einfache Logik: Jeder Tab bekommt seine definierte Zeit, dann wird gewechselt
/// ✅ KORRIGIERT: Hochpräziser Timer mit DateTime-basierter Kontrolle
/// </summary>
public class TournamentOverviewCycleManager
{
    private readonly DispatcherTimer _checkTimer; // ✅ KORRIGIERT: Hochfrequenter Check-Timer
    private readonly LocalizationService _localizationService;
    private readonly Func<TabControl> _getMainTabControl;
    private readonly Func<List<TabItem>> _getActiveTournamentTabs;
    private readonly Func<int> _getCurrentClassIndex;
    private readonly Func<int> _getCurrentSubTabIndex;
    private readonly Action<int> _setCurrentClassIndex;
    private readonly Action<int> _setCurrentSubTabIndex;
    private readonly Action _setCurrentSubTab;
    private readonly Action _updateStatus;
    private readonly Action _startScrolling;
    
    private bool _isRunning = false;
    private DateTime _tabStartTime = DateTime.Now;
    private DateTime _lastUIUpdate = DateTime.Now; // ✅ NEU: Für UI-Update-Kontrolle
    
    // Konfigurierbare Werte
    private int _subTabInterval = 5; // Sekunden pro Sub-Tab

    public bool IsRunning => _isRunning;
    public DateTime TabStartTime => _tabStartTime;

    public TournamentOverviewCycleManager(
        LocalizationService localizationService,
        Func<TabControl> getMainTabControl,
        Func<List<TabItem>> getActiveTournamentTabs,
        Func<int> getCurrentClassIndex,
        Func<int> getCurrentSubTabIndex,
        Action<int> setCurrentClassIndex,
        Action<int> setCurrentSubTabIndex,
        Action setCurrentSubTab,
        Action updateStatus,
        Action startScrolling)
    {
        _localizationService = localizationService;
        _getMainTabControl = getMainTabControl;
        _getActiveTournamentTabs = getActiveTournamentTabs;
        _getCurrentClassIndex = getCurrentClassIndex;
        _getCurrentSubTabIndex = getCurrentSubTabIndex;
        _setCurrentClassIndex = setCurrentClassIndex;
        _setCurrentSubTabIndex = setCurrentSubTabIndex;
        _setCurrentSubTab = setCurrentSubTab;
        _updateStatus = updateStatus;
        _startScrolling = startScrolling;
        
        // ✅ KORRIGIERT: Hochfrequenter Timer für präzise Kontrolle
        _checkTimer = new DispatcherTimer(DispatcherPriority.Normal);
        _checkTimer.Interval = TimeSpan.FromMilliseconds(100); // Alle 100ms prüfen
        _checkTimer.Tick += CheckTimer_Tick;
    }

    /// <summary>
    /// Startet das automatische Tab-Cycling
    /// </summary>
    public void StartCycling()
    {
        _isRunning = true;
        _tabStartTime = DateTime.Now;
        _lastUIUpdate = DateTime.Now;
        
        _setCurrentSubTab();
        _checkTimer.Start();
        
        // Starte Scrollen für den aktuellen Tab
        _startScrolling();
        
        _updateStatus();
        
        System.Diagnostics.Debug.WriteLine($"🎬 [AutoCycle] Started cycling - {_subTabInterval}s per tab (high-precision DateTime-based)");
    }

    /// <summary>
    /// Stoppt das automatische Tab-Cycling
    /// </summary>
    public void StopCycling()
    {
        _isRunning = false;
        _checkTimer.Stop();
        
        _updateStatus();
        
        System.Diagnostics.Debug.WriteLine("⏹️ [AutoCycle] Stopped cycling");
    }

    /// <summary>
    /// Aktualisiert die Konfiguration
    /// </summary>
    public void UpdateConfiguration(int classInterval, int subTabInterval)
    {
        _subTabInterval = subTabInterval;
        
        System.Diagnostics.Debug.WriteLine($"⚙️ [AutoCycle] Updated config - {_subTabInterval}s per tab");
    }

    /// <summary>
    /// Gibt das aktuelle Sub-Tab-Intervall zurück
    /// </summary>
    public int GetCurrentSubTabInterval()
    {
        return _subTabInterval;
    }

    /// <summary>
    /// ✅ KORRIGIERT: Berechnet die verbleibende Zeit exakt basierend auf DateTime
    /// </summary>
    public int GetRemainingTime()
    {
        if (!_isRunning) return 0;
        
        var elapsed = (DateTime.Now - _tabStartTime).TotalSeconds;
        var remaining = Math.Max(0, _subTabInterval - elapsed);
        return (int)Math.Ceiling(remaining);
    }

    /// <summary>
    /// ✅ KORRIGIERT: Hochfrequenter Check-Timer (100ms) mit DateTime-basierter Logik
    /// </summary>
    private void CheckTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isRunning) return;

        var now = DateTime.Now;
        var elapsedSeconds = (now - _tabStartTime).TotalSeconds;
        
        // ✅ NEU: UI-Update alle 500ms für flüssige Anzeige ohne Performance-Impact
        if ((now - _lastUIUpdate).TotalMilliseconds >= 500)
        {
            _updateStatus();
            _lastUIUpdate = now;
            
            // ✅ DEBUG: Zeige präzise Timer-Info alle 500ms
            var remainingTime = Math.Max(0, _subTabInterval - elapsedSeconds);
            System.Diagnostics.Debug.WriteLine($"⏱️ [AutoCycle] Precise check: elapsed={elapsedSeconds:F1}s, remaining={remainingTime:F1}s");
        }
        
        // ✅ KORRIGIERT: Exakte Tab-Wechsel basierend auf DateTime
        if (elapsedSeconds >= _subTabInterval)
        {
            System.Diagnostics.Debug.WriteLine($"⏰ [AutoCycle] Tab switch triggered after {elapsedSeconds:F3}s (target: {_subTabInterval}s)");
            SwitchToNextTab();
        }
    }

    /// <summary>
    /// Wechselt zum nächsten Tab - einfache, klare Logik
    /// </summary>
    private void SwitchToNextTab()
    {
        try
        {
            var mainTabControl = _getMainTabControl();
            var activeTournamentTabs = _getActiveTournamentTabs();
            var currentClassIndex = _getCurrentClassIndex();
            var currentSubTabIndex = _getCurrentSubTabIndex();

            if (activeTournamentTabs.Count == 0) return;

            // ✅ KORRIGIERT: Zeige exakte Timer-Performance
            var actualElapsed = (DateTime.Now - _tabStartTime).TotalSeconds;
            var expectedTime = _subTabInterval;
     var timingDifference = actualElapsed - expectedTime;

        // Hole aktuelle Sub-Tab-Informationen
     var currentClassTab = mainTabControl.SelectedItem as TabItem;
if (currentClassTab?.Content is TabControl subTabControl && subTabControl.Items.Count > 0)
          {
  // Nächster Sub-Tab in der gleichen Klasse
     var nextSubTabIndex = currentSubTabIndex + 1;
      
       if (nextSubTabIndex < subTabControl.Items.Count)
        {
   // Bleibe in der gleichen Klasse, wechsle Sub-Tab
          _setCurrentSubTabIndex(nextSubTabIndex);
_setCurrentSubTab();
       
   System.Diagnostics.Debug.WriteLine($"📑 [AutoCycle] Switched to sub-tab {nextSubTabIndex + 1}/{subTabControl.Items.Count} in class {currentClassIndex + 1} " +
         $"(precise: {actualElapsed:F3}s, expected: {expectedTime}s, diff: {timingDifference:+0.000;-0.000}s)");
         }
      else
     {
        // Alle Sub-Tabs durch, wechsle zur nächsten Klasse
      SwitchToNextClass();
               return;
                }
            }
            else
{
                // Keine Sub-Tabs, direkt zur nächsten Klasse
         SwitchToNextClass();
     return;
          }

            // ✅ KORRIGIERT: Exakte Timer-Zurücksetzung
         _tabStartTime = DateTime.Now;
 
            // Starte Scrollen für den neuen Tab nach kurzer Verzögerung
            Task.Delay(200).ContinueWith(_ =>
       {
 if (_isRunning)
             {
         Application.Current.Dispatcher.BeginInvoke(() =>
        {
      _startScrolling();
   });
            }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ [AutoCycle] Error switching tab: {ex.Message}");
        }
    }

    /// <summary>
    /// Wechselt zur nächsten Tournament-Klasse
    /// </summary>
    private void SwitchToNextClass()
    {
   try
    {
            var mainTabControl = _getMainTabControl();
  var activeTournamentTabs = _getActiveTournamentTabs();
  var currentClassIndex = _getCurrentClassIndex();

if (activeTournamentTabs.Count <= 1) 
        {
       // Nur eine Klasse - starte Sub-Tabs von vorne
                _setCurrentSubTabIndex(0);
   _setCurrentSubTab();
  
             System.Diagnostics.Debug.WriteLine("🔄 [AutoCycle] Only one class - restarting sub-tabs");
            }
     else
         {
          // Nächste Klasse
         var nextClassIndex = (currentClassIndex + 1) % activeTournamentTabs.Count;
                
  mainTabControl.SelectedIndex = nextClassIndex;
    _setCurrentClassIndex(nextClassIndex);
                _setCurrentSubTabIndex(0); // Beginne mit erstem Sub-Tab
                _setCurrentSubTab();
          
 System.Diagnostics.Debug.WriteLine($"🏆 [AutoCycle] Switched to class {nextClassIndex + 1}/{activeTournamentTabs.Count}");
            }

            // ✅ KORRIGIERT: Exakte Timer-Zurücksetzung
    _tabStartTime = DateTime.Now;
     
       // Starte Scrollen für den neuen Tab nach kurzer Verzögerung
     Task.Delay(200).ContinueWith(_ =>
       {
  if (_isRunning)
           {
            Application.Current.Dispatcher.BeginInvoke(() =>
      {
            _startScrolling();
     });
    }
       });
        }
        catch (Exception ex)
        {
         System.Diagnostics.Debug.WriteLine($"❌ [AutoCycle] Error switching class: {ex.Message}");
        }
    }

    public void Dispose()
    {
        StopCycling();
    }
}