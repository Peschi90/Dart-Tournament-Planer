using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Vereinfachter Manager für automatisches Tab-Cycling im TournamentOverviewWindow
/// Klare, einfache Logik: Jeder Tab bekommt seine definierte Zeit, dann wird gewechselt
/// </summary>
public class TournamentOverviewCycleManager
{
    private readonly DispatcherTimer _cycleTimer;
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
    
    // Konfigurierbare Werte
    private int _subTabInterval = 5; // Sekunden pro Sub-Tab
    private bool _isInClassTransition = false; // Flag für Klassen-Übergang

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
        
        _cycleTimer = new DispatcherTimer();
        _cycleTimer.Interval = TimeSpan.FromSeconds(1); // Jede Sekunde prüfen
        _cycleTimer.Tick += CycleTimer_Tick;
    }

    /// <summary>
    /// Startet das automatische Tab-Cycling
    /// </summary>
    public void StartCycling()
    {
        _isRunning = true;
        _tabStartTime = DateTime.Now;
        _isInClassTransition = false;
        
        _setCurrentSubTab();
        _cycleTimer.Start();
        
        // Starte Scrollen für den aktuellen Tab
        _startScrolling();
        
        _updateStatus();
        
        System.Diagnostics.Debug.WriteLine($"🎬 [AutoCycle] Started cycling - {_subTabInterval}s per tab");
    }

    /// <summary>
    /// Stoppt das automatische Tab-Cycling
    /// </summary>
    public void StopCycling()
    {
        _isRunning = false;
        _cycleTimer.Stop();
        _isInClassTransition = false;
        
        _updateStatus();
        
        System.Diagnostics.Debug.WriteLine("⏹️ [AutoCycle] Stopped cycling");
    }

    /// <summary>
    /// Aktualisiert die Konfiguration
    /// </summary>
    public void UpdateConfiguration(int classInterval, int subTabInterval)
    {
        // Vereinfacht: Beide Intervalle werden als Sub-Tab-Interval behandelt
        // Klassen-Wechsel passiert automatisch wenn alle Sub-Tabs durch sind
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
    /// Berechnet die verbleibende Zeit bis zum nächsten Tab-Wechsel
    /// </summary>
    public int GetRemainingTime()
    {
        if (!_isRunning) return 0;
        
        var elapsed = (DateTime.Now - _tabStartTime).TotalSeconds;
        var remaining = Math.Max(0, _subTabInterval - (int)elapsed);
        
        return remaining;
    }

    /// <summary>
    /// Timer-Event für Tab-Cycling - prüft jede Sekunde ob gewechselt werden soll
    /// </summary>
    private void CycleTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isRunning) return;

        var elapsed = (DateTime.Now - _tabStartTime).TotalSeconds;
        
        // Zeit für den aktuellen Tab abgelaufen?
        if (elapsed >= _subTabInterval)
        {
            SwitchToNextTab();
        }
        
        _updateStatus();
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
                    
                    System.Diagnostics.Debug.WriteLine($"📑 [AutoCycle] Switched to sub-tab {nextSubTabIndex + 1}/{subTabControl.Items.Count} in class {currentClassIndex + 1}");
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

            // Timer für neuen Tab zurücksetzen
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

            // Timer für neue Klasse zurücksetzen
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