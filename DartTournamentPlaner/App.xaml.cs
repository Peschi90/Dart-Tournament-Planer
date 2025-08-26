using System.Configuration;
using System.Data;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static ConfigService? ConfigService { get; private set; }
    public static LocalizationService? LocalizationService { get; private set; }
    public static DataService? DataService { get; private set; }
    public static UpdateService? UpdateService { get; private set; }
    public static IApiIntegrationService? ApiIntegrationService { get; private set; }

    /// <summary>
    /// Global event that fires when the application language changes
    /// All windows can subscribe to this to update their translations
    /// </summary>
    public static event EventHandler<string>? GlobalLanguageChanged;

    protected override async void OnStartup(StartupEventArgs e)
    {
        StartupSplashWindow? splashWindow = null;
        
        try
        {
            System.Diagnostics.Debug.WriteLine("=== App.OnStartup START ===");

            // Initialize services
            ConfigService = new ConfigService();
            LocalizationService = new LocalizationService();
            DataService = new DataService();
            UpdateService = new UpdateService(LocalizationService);
            ApiIntegrationService = new ApiIntegrationService();

            // Set static reference in Match and KnockoutMatch for localization
            Match.LocalizationService = LocalizationService;
            KnockoutMatch.LocalizationService = LocalizationService;

            // Load configuration first
            await ConfigService.LoadConfigAsync();
            
            // Set initial language from config
            System.Diagnostics.Debug.WriteLine($"App.OnStartup: Setting initial language to '{ConfigService.Config.Language}'");
            LocalizationService.ChangeLanguage(ConfigService.Config.Language);
            
            // Connect ConfigService language changes to LocalizationService
            ConfigService.LanguageChanged += (sender, language) =>
            {
                System.Diagnostics.Debug.WriteLine($"App.OnStartup: ConfigService LanguageChanged event - setting to '{language}'");
                LocalizationService.ChangeLanguage(language);
                
                // Fire global language changed event for all windows
                GlobalLanguageChanged?.Invoke(null, language);
            };

            // Zeige Splash Screen mit Loading Animation
            splashWindow = new StartupSplashWindow(LocalizationService);
            splashWindow.Show();

            System.Diagnostics.Debug.WriteLine("App.OnStartup: Splash screen displayed");

            // Lade Services und überprüfe Updates mit Progress-Anzeige
            await InitializeApplicationAsync(splashWindow);

            // Erstelle und zeige Main Window
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;

            // Schließe Splash Screen und zeige Main Window
            await splashWindow.CloseGracefullyAsync();
            mainWindow.Show();

            System.Diagnostics.Debug.WriteLine("=== App.OnStartup END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App.OnStartup: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"App.OnStartup: Stack trace: {ex.StackTrace}");

            // Schließe Splash Screen bei Fehlern
            try
            {
                if (splashWindow != null)
                {
                    await splashWindow.CloseGracefullyAsync();
                }
            }
            catch
            {
                // Ignore splash window cleanup errors
            }

            // Zeige Error und erstelle trotzdem Main Window als Fallback
            MessageBox.Show(
                $"Fehler beim Starten der Anwendung:\n\n{ex.Message}",
                "Startup-Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            try
            {
                var fallbackMainWindow = new MainWindow();
                MainWindow = fallbackMainWindow;
                fallbackMainWindow.Show();
            }
            catch (Exception fallbackEx)
            {
                System.Diagnostics.Debug.WriteLine($"App.OnStartup: FALLBACK ERROR: {fallbackEx.Message}");
                MessageBox.Show(
                    $"Kritischer Fehler - Anwendung kann nicht gestartet werden:\n\n{fallbackEx.Message}",
                    "Kritischer Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                Current?.Shutdown(-1);
            }
        }

        // WICHTIG: base.OnStartup() NICHT aufrufen, da wir MainWindow manuell erstellen
        // base.OnStartup(e);
    }

    /// <summary>
    /// Initialisiert die Anwendung asynchron mit Progress-Anzeige
    /// </summary>
    private async Task InitializeApplicationAsync(StartupSplashWindow splashWindow)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== InitializeApplicationAsync START ===");

            // Phase 1: Services initialisieren
            await splashWindow.ExecuteWithStatusAsync(async (progress) =>
            {
                progress.Report("Initialisiere Services...");
                await Task.Delay(300); // Kurze Pause für visuelle Wahrnehmung
                
                progress.Report("Services bereit");
                await Task.Delay(200);
                
            }, LocalizationService?.GetTranslation("StartingApplication") ?? "Starte Anwendung...");

            // Phase 2: Update-Überprüfung
            await CheckForUpdatesAsync(splashWindow);

            // Phase 3: Finale Vorbereitung
            await splashWindow.ExecuteWithStatusAsync(async (progress) =>
            {
                progress.Report("Bereite Benutzeroberfläche vor...");
                await Task.Delay(400);
                
                progress.Report("Fertig");
                await Task.Delay(300);
                
            }, LocalizationService?.GetTranslation("Ready") ?? "Bereit");

            System.Diagnostics.Debug.WriteLine("=== InitializeApplicationAsync END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"InitializeApplicationAsync: ERROR: {ex.Message}");
            // Fehler nicht weiterwerfen - Anwendung soll trotzdem starten
        }
    }

    /// <summary>
    /// Überprüft asynchron nach verfügbaren Updates
    /// </summary>
    private async Task CheckForUpdatesAsync(StartupSplashWindow splashWindow)
    {
        UpdateInfo? updateInfo = null;
        
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CheckForUpdatesAsync START ===");

            await splashWindow.ExecuteWithStatusAsync(async (progress) =>
            {
                progress.Report(LocalizationService?.GetTranslation("ConnectingToGitHub") ?? "Verbinde mit GitHub...");
                await Task.Delay(200);
                
                progress.Report(LocalizationService?.GetTranslation("AnalyzingReleases") ?? "Analysiere Releases...");
                
                // Eigentliche Update-Überprüfung
                updateInfo = await UpdateService!.CheckForUpdatesAsync();
                
                await Task.Delay(300); // Kurze Pause damit User den Progress sieht
                
                var resultText = updateInfo != null 
                    ? LocalizationService?.GetTranslation("UpdateAvailable") ?? "Update verfügbar"
                    : LocalizationService?.GetTranslation("NoUpdateAvailable") ?? "Keine Updates verfügbar";
                    
                progress.Report(resultText);
                await Task.Delay(500);
                
            }, LocalizationService?.GetTranslation("CheckingForUpdates") ?? "Suche nach Updates...");

            // Zeige Update-Dialog falls verfügbar
            if (updateInfo != null)
            {
                System.Diagnostics.Debug.WriteLine($"CheckForUpdatesAsync: Update available - {updateInfo.LatestVersion}");
                await ShowUpdateDialogAsync(updateInfo);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CheckForUpdatesAsync: No updates available");
            }

            System.Diagnostics.Debug.WriteLine("=== CheckForUpdatesAsync END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckForUpdatesAsync: ERROR: {ex.Message}");
            
            // Update Splash mit Fehlerstatus
            try
            {
                splashWindow.UpdateStatus(
                    LocalizationService?.GetTranslation("UpdateCheckComplete") ?? "Update-Überprüfung abgeschlossen", 
                    LocalizationService?.GetTranslation("UpdateCheckFailed") ?? "Überprüfung fehlgeschlagen");
                await Task.Delay(800);
            }
            catch
            {
                // Ignore splash window update errors
            }
        }
    }

    /// <summary>
    /// Zeigt den Update-Dialog an und behandelt die Benutzerantwort
    /// </summary>
    private async Task ShowUpdateDialogAsync(UpdateInfo updateInfo)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("ShowUpdateDialogAsync: Displaying update dialog");

            await Dispatcher.InvokeAsync(async () =>
            {
                var updateDialog = new UpdateDialog(updateInfo, LocalizationService, UpdateService);
                updateDialog.ShowDialog();

                if (updateDialog.DownloadRequested)
                {
                    System.Diagnostics.Debug.WriteLine("ShowUpdateDialogAsync: User requested download");
                    
                    if (updateDialog.InstallationStarted)
                    {
                        // Installation wurde automatisch gestartet - App beenden
                        System.Diagnostics.Debug.WriteLine("ShowUpdateDialogAsync: Installation started, shutting down application");
                        Current?.Shutdown(0);
                        return;
                    }
                    else if (!updateInfo.CanAutoDownload)
                    {
                        // Fallback: Öffne Download-Seite im Browser
                        updateDialog.OpenDownloadPage();
                        
                        // Optional: Zeige Hinweis dass Anwendung nach Download beendet werden sollte
                        var result = MessageBox.Show(
                            LocalizationService?.GetTranslation("UpdateDownloadStarted") ?? 
                            "Der Download wurde gestartet. Möchten Sie die Anwendung jetzt beenden um das Update zu installieren?",
                            LocalizationService?.GetTranslation("UpdateAvailable") ?? "Update verfügbar",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Debug.WriteLine("ShowUpdateDialogAsync: User chose to exit for manual update installation");
                            Current?.Shutdown(0);
                            return;
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ShowUpdateDialogAsync: User declined download");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ShowUpdateDialogAsync: ERROR: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleanup beim Beenden der Anwendung
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("App.OnExit: Cleaning up services");
            
            // Stop API if running
            if (ApiIntegrationService?.IsApiRunning == true)
            {
                System.Diagnostics.Debug.WriteLine("App.OnExit: Stopping API service");
                _ = Task.Run(async () => await ApiIntegrationService.StopApiAsync());
            }

            UpdateService?.Dispose();
            
            System.Diagnostics.Debug.WriteLine("App.OnExit: Cleanup completed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"App.OnExit: ERROR during cleanup: {ex.Message}");
        }
        
        base.OnExit(e);
    }
}

