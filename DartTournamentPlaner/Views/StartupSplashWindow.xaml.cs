using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Splash Screen für den Start der Anwendung
/// Zeigt Loading-Animation während der Initialisierung und Update-Überprüfung
/// </summary>
public partial class StartupSplashWindow : Window
{
    private readonly LocalizationService? _localizationService;
    private readonly DispatcherTimer _minimumShowTimer;
    private bool _canClose = false;
    private DateTime _showStartTime;
    private const int MINIMUM_SHOW_TIME_MS = 2000; // Mindestens 2 Sekunden anzeigen

    public StartupSplashWindow(LocalizationService? localizationService = null)
    {
        _localizationService = localizationService;
        
        InitializeComponent();
        InitializeSplashScreen();

        // Timer für Mindestanzeige-Zeit
        _minimumShowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(MINIMUM_SHOW_TIME_MS)
        };
        _minimumShowTimer.Tick += (s, e) =>
        {
            _minimumShowTimer.Stop();
            _canClose = true;
        };

        _showStartTime = DateTime.Now;
        _minimumShowTimer.Start();
    }

    private void InitializeSplashScreen()
    {
        try
        {
            // Versionsinformation setzen
            var version = GetApplicationVersion();
            VersionTextBlock.Text = $"Version {version}";

            // Lokalisierte Texte setzen
            UpdateTranslatedTexts();

            System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Initialized successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.InitializeSplashScreen: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert die übersetzten Texte
    /// </summary>
    private void UpdateTranslatedTexts()
    {
        try
        {
            if (_localizationService != null)
            {
                AppTitleBlock.Text = _localizationService.GetTranslation("AppTitle");
                AppSubtitleBlock.Text = _localizationService.GetTranslation("AppSubtitle");
                StatusTextBlock.Text = _localizationService.GetTranslation("StartingApplication");
                CopyrightTextBlock.Text = $"© 2025 by I3uLL3t";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.UpdateTranslatedTexts: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Ermittelt die Anwendungsversion
    /// </summary>
    private string GetApplicationVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            
            if (version != null)
            {
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            
            return "1.0.0"; // Fallback
        }
        catch
        {
            return "1.0.0"; // Fallback bei Fehlern
        }
    }

    /// <summary>
    /// Aktualisiert den Status-Text und Progress
    /// </summary>
    /// <param name="statusText">Neuer Status-Text</param>
    /// <param name="progressText">Optionaler Progress-Text für den Spinner</param>
    public void UpdateStatus(string statusText, string? progressText = null)
    {
        try
        {
            Dispatcher.BeginInvoke(() =>
            {
                StatusTextBlock.Text = statusText;

                System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Status updated to: {statusText}");
            }, DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.UpdateStatus: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Führt eine asynchrone Operation mit Statusaktualisierung aus
    /// </summary>
    /// <param name="operation">Die auszuführende Operation</param>
    /// <param name="statusText">Status-Text während der Operation</param>
    /// <param name="progressCallback">Callback für Progress-Updates</param>
    public async Task ExecuteWithStatusAsync(Func<IProgress<string>, Task> operation, string statusText, Action<string>? progressCallback = null)
    {
        try
        {
            UpdateStatus(statusText);

            var progress = new Progress<string>(message =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    StatusTextBlock.Text = message;
                    progressCallback?.Invoke(message);
                }, DispatcherPriority.DataBind);
            });

            await operation(progress);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.ExecuteWithStatusAsync: ERROR: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Schließt den Splash Screen mit sanfter Animation
    /// Wartet die Mindestanzeige-Zeit ab
    /// </summary>
    public async Task CloseGracefullyAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Closing gracefully...");

            // Warten auf Mindestanzeige-Zeit
            var elapsedTime = (DateTime.Now - _showStartTime).TotalMilliseconds;
            if (elapsedTime < MINIMUM_SHOW_TIME_MS)
            {
                var remainingTime = MINIMUM_SHOW_TIME_MS - (int)elapsedTime;
                System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Waiting additional {remainingTime}ms for minimum show time");
                
                UpdateStatus(_localizationService?.GetTranslation("Ready") ?? "Bereit", "");
                await Task.Delay(remainingTime);
            }

            // Warten bis Timer erlaubt zu schließen
            while (!_canClose)
            {
                await Task.Delay(100);
            }

            // Sanfte Fade-out Animation (optional)
            await FadeOutAsync();

            // Schließen
            Dispatcher.BeginInvoke(() => 
            {
                try
                {
                    Close();
                    System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Closed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.CloseGracefullyAsync: Error closing: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.CloseGracefullyAsync: ERROR: {ex.Message}");
            // Fallback: Direkt schließen
            try
            {
                Close();
            }
            catch
            {
                // Ignore secondary errors
            }
        }
    }

    /// <summary>
    /// Fade-out Animation für sanftes Schließen
    /// </summary>
    private async Task FadeOutAsync()
    {
        try
        {
            const int fadeSteps = 20;
            const int fadeDelay = 15;

            for (int i = fadeSteps; i >= 0; i--)
            {
                var opacity = (double)i / fadeSteps;
                Dispatcher.BeginInvoke(() => 
                { 
                    Opacity = opacity; 
                });
                
                await Task.Delay(fadeDelay);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.FadeOutAsync: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Verhindert manuelles Schließen während der Mindestanzeige-Zeit
    /// </summary>
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (!_canClose)
        {
            e.Cancel = true;
            return;
        }
        
        base.OnClosing(e);
    }

    /// <summary>
    /// Cleanup bei Window-Schließung
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        try
        {
            _minimumShowTimer?.Stop();
            
            System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Cleanup completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.OnClosed: ERROR during cleanup: {ex.Message}");
        }
        
        base.OnClosed(e);
    }
}