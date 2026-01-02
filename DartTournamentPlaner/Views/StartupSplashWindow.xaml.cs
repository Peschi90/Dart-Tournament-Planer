using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

/// <summary>
/// Splash Screen für den Start der Anwendung
/// Zeigt Loading-Animation während der Initialisierung und Update-Überprüfung
/// Erweitert mit modernen Animationen und Fortschrittsanzeige
/// </summary>
public partial class StartupSplashWindow : Window
{
    private readonly LocalizationService? _localizationService;
    private readonly DispatcherTimer _minimumShowTimer;
    private bool _canClose = false;
    private DateTime _showStartTime;
    private const int MINIMUM_SHOW_TIME_MS = 3000; // Mindestens 3 Sekunden anzeigen (verlängert für Animationen)

    // Animation Storyboards
    private Storyboard? _progressAnimation;
    private double _currentProgress = 0;

    public StartupSplashWindow(LocalizationService? localizationService = null)
    {
        _localizationService = localizationService;
        
        InitializeComponent();
        InitializeSplashScreen();
        InitializeAnimations();

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
            // Lokalisierte Texte setzen (macht auch Version-Update)
            UpdateTranslatedTexts();
            
            // Versionsinformation mit lokalisiertem Präfix setzen
            UpdateVersionText();

            // Icon-Anzeige initialisieren
            InitializeAppIcon();

            System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Initialized successfully with animations");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.InitializeSplashScreen: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialisiert die App-Icon-Anzeige mit Fallback-Handling
    /// </summary>
    private void InitializeAppIcon()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Initializing app icon...");
            
            // Standardmäßig Fallback anzeigen bis Icon geladen ist
            if (FallbackIconText != null)
            {
                FallbackIconText.Visibility = Visibility.Visible;
            }
            if (AppIconImage != null)
            {
                AppIconImage.Visibility = Visibility.Collapsed;
            }

            // Event Handler für Icon-Loading
            if (AppIconImage != null)
            {
                AppIconImage.ImageFailed += (s, e) =>
                {
                    // Fallback auf Emoji wenn Icon nicht geladen werden kann
                    System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: App icon failed to load from primary source. Exception: {e.ErrorException?.Message}");
                    TryAlternativeIconPaths();
                };

                AppIconImage.Loaded += (s, e) =>
                {
                    // Icon erfolgreich geladen
                    System.Diagnostics.Debug.WriteLine("StartupSplashWindow: App icon loaded successfully from primary source");
                    AppIconImage.Visibility = Visibility.Visible;
                    FallbackIconText.Visibility = Visibility.Collapsed;
                };

                // Versuche sofort das Icon zu laden
                try
                {
                    var uri = new Uri("pack://application:,,,/Assets/Images/logo.png");
                    System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Attempting to load icon from: {uri}");
                    AppIconImage.Source = new BitmapImage(uri);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Failed to set initial icon source: {ex.Message}");
                    TryAlternativeIconPaths();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.InitializeAppIcon: ERROR: {ex.Message}");
            // Bei Fehler verwende Emoji-Fallback
            ShowFallbackIcon();
        }
    }

    /// <summary>
    /// Versucht alternative Icon-Pfade zu laden
    /// </summary>
    private void TryAlternativeIconPaths()
    {
        var alternativePaths = new[]
        {
            // Zuerst logo.png versuchen
            "pack://application:,,,/Assets/Images/logo.png",
            "pack://application:,,,/logo.png",
            "pack://application:,,,/Resources/logo.png", 
            "pack://application:,,,/Images/logo.png",
            "pack://application:,,,/Assets/logo.png",
            
            // Dann favicon.ico als Fallback
            "pack://application:,,,/Assets/Images/favicon.ico",
            "pack://application:,,,/favicon.ico",
            "pack://application:,,,/Resources/favicon.ico",
            
            // Weitere alternative Pfade
            "pack://application:,,,/Resources/Images/logo.png",
            "/logo.png",
            "/Assets/Images/logo.png",
            "/Resources/logo.png"
        };

        foreach (var path in alternativePaths)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Trying icon path: {path}");
                var uri = new Uri(path, UriKind.RelativeOrAbsolute);
                var bitmap = new BitmapImage(uri);
                
                AppIconImage.Source = bitmap;
                AppIconImage.Visibility = Visibility.Visible;
                FallbackIconText.Visibility = Visibility.Collapsed;
                
                System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Successfully loaded icon from: {path}");
                return; // Erfolgreich geladen
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Failed to load from {path}: {ex.Message}");
                // Nächsten Pfad versuchen
                continue;
            }
        }

        // Alle Pfade fehlgeschlagen - verwende Emoji-Fallback
        System.Diagnostics.Debug.WriteLine("StartupSplashWindow: All icon paths failed, using emoji fallback");
        ShowFallbackIcon();
    }

    /// <summary>
    /// Zeigt das Emoji-Fallback-Icon an
    /// </summary>
    private void ShowFallbackIcon()
    {
        try
        {
            if (AppIconImage != null)
            {
                AppIconImage.Visibility = Visibility.Collapsed;
            }
            if (FallbackIconText != null)
            {
                FallbackIconText.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.ShowFallbackIcon: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Initialisiert die Animationen
    /// </summary>
    private void InitializeAnimations()
    {
        try
        {
            // Starte die Fortschrittsanimation automatisch
            _progressAnimation = FindResource("ProgressAnimation") as Storyboard;
            
            // Verzögerte den Start der Fortschrittsanimation etwas
            var delayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            delayTimer.Tick += (s, e) =>
            {
                delayTimer.Stop();
                StartProgressAnimation();
            };
            delayTimer.Start();

            System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Animations initialized");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.InitializeAnimations: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Startet die Fortschrittsbalken-Animation
    /// </summary>
    private void StartProgressAnimation()
    {
        try
        {
            _progressAnimation?.Begin();
            System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Progress animation started");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.StartProgressAnimation: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert den Fortschritt manuell (0.0 bis 1.0)
    /// </summary>
    /// <param name="progress">Fortschritt zwischen 0.0 und 1.0</param>
    public void UpdateProgress(double progress)
    {
        try
        {
            progress = Math.Max(0.0, Math.Min(1.0, progress)); // Clamp zwischen 0 und 1
            _currentProgress = progress;

            Dispatcher.BeginInvoke(() =>
            {
                // Animiere den Fortschrittsbalken zur neuen Position
                var animation = new DoubleAnimation
                {
                    To = 300 * progress, // 300 ist die neue maximale Breite
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                ProgressFill.BeginAnimation(FrameworkElement.WidthProperty, animation);

                System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Progress updated to {progress:P1}");
            }, DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.UpdateProgress: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Animiert eine Status-Änderung mit sanftem Übergang
    /// </summary>
    /// <param name="newStatus">Neuer Status-Text</param>
    public void UpdateStatusAnimated(string newStatus)
    {
        try
        {
            Dispatcher.BeginInvoke(async () =>
            {
                // Fade Out
                var fadeOut = new DoubleAnimation
                {
                    To = 0.3,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };

                StatusTextBlock.BeginAnimation(OpacityProperty, fadeOut);
                await Task.Delay(200);

                // Text ändern
                StatusTextBlock.Text = newStatus;

                // Fade In
                var fadeIn = new DoubleAnimation
                {
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                StatusTextBlock.BeginAnimation(OpacityProperty, fadeIn);

                System.Diagnostics.Debug.WriteLine($"StartupSplashWindow: Status animated to: {newStatus}");
            }, DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.UpdateStatusAnimated: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert den Versions-Text mit Lokalisierung
    /// </summary>
    private void UpdateVersionText()
    {
        try
        {
            var version = GetApplicationVersion();
            var versionPrefix = _localizationService?.GetTranslation("VersionPrefix") ?? "Version";
            VersionTextBlock.Text = $"{versionPrefix} {version}";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.UpdateVersionText: ERROR: {ex.Message}");
            // Fallback
            VersionTextBlock.Text = $"Version {GetApplicationVersion()}";
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
                CopyrightTextBlock.Text = _localizationService.GetTranslation("CopyrightText");
                
                // Update LoadingSpinner text if available
                if (MainLoadingSpinner != null)
                {
                    MainLoadingSpinner.LoadingText = _localizationService.GetTranslation("LoadingText");
                }
            }
            else
            {
                // Fallback auf deutsche Texte wenn kein LocalizationService verfügbar
                AppTitleBlock.Text = "Dart Tournament Planner";
                AppSubtitleBlock.Text = "Moderne Turnierverwaltung";
                StatusTextBlock.Text = "Starte Anwendung...";
                CopyrightTextBlock.Text = "© 2025 by I3uLL3t";
                
                if (MainLoadingSpinner != null)
                {
                    MainLoadingSpinner.LoadingText = "Wird geladen...";
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.UpdateTranslatedTexts: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Aktualisiert die Sprache der Splash Screen Texte
    /// Kann aufgerufen werden wenn sich die Sprache ändert
    /// </summary>
    public void UpdateLanguage()
    {
        try
        {
            UpdateTranslatedTexts();
            UpdateVersionText();
            System.Diagnostics.Debug.WriteLine("StartupSplashWindow: Language updated successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.UpdateLanguage: ERROR: {ex.Message}");
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
            UpdateStatusAnimated(statusText);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"StartupSplashWindow.UpdateStatus: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Führt eine asynchrone Operation mit Statusaktualisierung und Fortschrittsanzeige aus
    /// </summary>
    /// <param name="operation">Die auszuführende Operation</param>
    /// <param name="statusText">Status-Text während der Operation</param>
    /// <param name="progressCallback">Callback für Progress-Updates</param>
    public async Task ExecuteWithStatusAsync(Func<IProgress<string>, Task> operation, string statusText, Action<string>? progressCallback = null)
    {
        try
        {
            UpdateStatusAnimated(statusText);

            var progress = new Progress<string>(message =>
            {
                Dispatcher.BeginInvoke(() =>
                {
                    UpdateStatusAnimated(message);
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
                
                UpdateStatusAnimated(_localizationService?.GetTranslation("Ready") ?? "Bereit");
                UpdateProgress(1.0); // Vollständiger Fortschritt
                await Task.Delay(remainingTime);
            }

            // Warten bis Timer erlaubt zu schließen
            while (!_canClose)
            {
                await Task.Delay(100);
            }

            // Sanfte Fade-out Animation
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
            var fadeOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var tcs = new TaskCompletionSource<bool>();
            fadeOut.Completed += (s, e) => tcs.SetResult(true);

            BeginAnimation(OpacityProperty, fadeOut);
            await tcs.Task;
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