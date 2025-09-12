using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Helper-Klasse für UI-Updates und Status-Management im MainWindow
/// Verwaltet Statusleiste, Menü-Updates und visuelle Anzeigen
/// </summary>
public class MainWindowUIHelper
{
    private readonly LocalizationService _localizationService;
    private readonly Dispatcher _dispatcher;

    // UI Elements (werden vom MainWindow gesetzt)
    public Ellipse? HubStatusIndicator { get; set; }
    public TextBlock? HubStatusText { get; set; }
    public TextBlock? HubSyncStatus { get; set; }
    public Ellipse? ApiStatusIndicator { get; set; }
    public TextBlock? ApiStatusText { get; set; }
    public TextBlock? StatusTextBlock { get; set; }
    public TextBlock? LastSavedBlock { get; set; }
    public TextBlock? LanguageStatusBlock { get; set; }
    public MenuItem? ApiStatusMenuItem { get; set; }
    public MenuItem? StartApiMenuItem { get; set; }
    public MenuItem? StopApiMenuItem { get; set; }
    public MenuItem? OpenApiDocsMenuItem { get; set; }

    public MainWindowUIHelper(LocalizationService localizationService, Dispatcher dispatcher)
    {
        _localizationService = localizationService;
        _dispatcher = dispatcher;
    }

    public void UpdateHubStatus(bool isConnected, string tournamentId, bool isSyncing, DateTime lastSyncTime)
    {
        try
        {
            _dispatcher.Invoke(() =>
            {
                if (HubStatusIndicator is null || HubStatusText is null || HubSyncStatus is null) return;

                if (isConnected)
                {
                    HubStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                    HubStatusText.Text = $"Hub: Verbunden ({tournamentId})";
                    
                    if (isSyncing)
                    {
                        HubSyncStatus.Text = "🔄 Synchronisiert...";
                        HubSyncStatus.Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                    }
                    else if (lastSyncTime != DateTime.MinValue)
                    {
                        var timeSinceSync = DateTime.Now - lastSyncTime;
                        if (timeSinceSync.TotalMinutes < 2)
                        {
                            HubSyncStatus.Text = "✅ WebSocket Live";
                            HubSyncStatus.Foreground = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                        }
                        else
                        {
                            HubSyncStatus.Text = $"⏱️ Sync vor {timeSinceSync.Minutes}min";
                            HubSyncStatus.Foreground = new SolidColorBrush(Color.FromRgb(241, 196, 15));
                        }
                    }
                    else
                    {
                        HubSyncStatus.Text = "🔌 WebSocket aktiv";
                        HubSyncStatus.Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219));
                    }
                }
                else
                {
                    HubStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    HubStatusText.Text = "Hub: Getrennt";
                    HubSyncStatus.Text = "(WebSocket inaktiv)";
                    HubSyncStatus.Foreground = new SolidColorBrush(Color.FromRgb(149, 165, 166));
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating hub status: {ex.Message}");
        }
    }

    public void UpdateApiStatus(bool isRunning, string? apiUrl)
    {
        try
        {
            var statusText = isRunning ? 
                (_localizationService.GetString("APIRunning") ?? "API läuft") : 
                (_localizationService.GetString("APIStopped") ?? "API gestoppt");
            
            if (ApiStatusMenuItem is not null)
            {
                ApiStatusMenuItem.Header = $"📊 {statusText}";
                
                if (isRunning && !string.IsNullOrEmpty(apiUrl))
                {
                    ApiStatusMenuItem.Header += $" ({apiUrl})";
                }
            }

            _dispatcher.Invoke(() =>
            {
                if (ApiStatusIndicator is not null && ApiStatusText is not null)
                {
                    if (isRunning)
                    {
                        ApiStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(46, 204, 113));
                        ApiStatusText.Text = "API: Läuft";
                    }
                    else
                    {
                        ApiStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                        ApiStatusText.Text = "API: Gestoppt";
                    }
                }
            });
            
            if (StartApiMenuItem is not null) StartApiMenuItem.IsEnabled = !isRunning;
            if (StopApiMenuItem is not null) StopApiMenuItem.IsEnabled = isRunning;
            if (OpenApiDocsMenuItem is not null) OpenApiDocsMenuItem.IsEnabled = isRunning;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UpdateApiStatus: Error: {ex.Message}");
            
            if (ApiStatusMenuItem is not null)
            {
                ApiStatusMenuItem.Header = "📊 " + (_localizationService.GetString("APIError") ?? "API Fehler");
            }
            
            _dispatcher.Invoke(() =>
            {
                if (ApiStatusIndicator is not null && ApiStatusText is not null)
                {
                    ApiStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    ApiStatusText.Text = "API: Fehler";
                }
            });
        }
    }

    public void UpdateStatusBar(bool hasUnsavedChanges)
    {
        try
        {
            _dispatcher.Invoke(() =>
            {
                if (StatusTextBlock is not null)
                {
                    StatusTextBlock.Text = hasUnsavedChanges ? 
                        _localizationService.GetString("HasUnsavedChanges") : 
                        "WebSocket-Hub Integration aktiviert";
                }
                
                if (LastSavedBlock is not null)
                {
                    LastSavedBlock.Text = hasUnsavedChanges ? 
                        _localizationService.GetString("NotSaved") : 
                        _localizationService.GetString("Saved");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating status bar: {ex.Message}");
        }
    }

    public void UpdateLanguageStatus()
    {
        try 
        {
            _dispatcher.Invoke(() =>
            {
                if (LanguageStatusBlock is not null)
                {
                    LanguageStatusBlock.Text = _localizationService.CurrentLanguage.ToUpper();
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating language status: {ex.Message}");
        }
    }

    public TextBlock? FindTextBlockInHeader(TabItem tabItem)
    {
        if (tabItem.Header is StackPanel stackPanel)
        {
            return stackPanel.Children.OfType<TextBlock>().FirstOrDefault();
        }
        return null;
    }

    public void UpdateTabHeaders(TabItem platinTab, TabItem goldTab, TabItem silverTab, TabItem bronzeTab)
    {
        var platinTextBlock = FindTextBlockInHeader(platinTab);
        if (platinTextBlock is not null) platinTextBlock.Text = _localizationService.GetString("Platinum");
        
        var goldTextBlock = FindTextBlockInHeader(goldTab);
        if (goldTextBlock is not null) goldTextBlock.Text = _localizationService.GetString("Gold");
        
        var silverTextBlock = FindTextBlockInHeader(silverTab);
        if (silverTextBlock is not null) silverTextBlock.Text = _localizationService.GetString("Silver");
        
        var bronzeTextBlock = FindTextBlockInHeader(bronzeTab);
        if (bronzeTextBlock is not null) bronzeTextBlock.Text = _localizationService.GetString("Bronze");
    }

    public void UpdateMenuTranslations(
        MenuItem fileMenuItem, MenuItem newMenuItem, MenuItem openMenuItem, 
        MenuItem saveMenuItem, MenuItem saveAsMenuItem, MenuItem printMenuItem, MenuItem exitMenuItem,
        MenuItem viewMenuItem, MenuItem overviewModeMenuItem,
        //MenuItem apiMenuItem, MenuItem startApiMenuItem, MenuItem stopApiMenuItem, MenuItem openApiDocsMenuItem,
        MenuItem tournamentHubMenuItem, MenuItem registerWithHubMenuItem, MenuItem unregisterFromHubMenuItem,
        MenuItem showJoinUrlMenuItem, MenuItem manualSyncMenuItem, MenuItem hubSettingsMenuItem,
        MenuItem licenseMenuItem, MenuItem licenseStatusMenuItem, MenuItem activateLicenseMenuItem, 
        MenuItem licenseInfoMenuItem, MenuItem removeLicenseMenuItem, MenuItem purchaseLicenseMenuItem,
        MenuItem settingsMenuItem, MenuItem helpMenuItem, MenuItem helpContentMenuItem, 
        MenuItem bugReportMenuItem, MenuItem aboutMenuItem)
    {
        // Datei-Menü
        fileMenuItem.Header = _localizationService.GetString("File");
        newMenuItem.Header = _localizationService.GetString("New");
        openMenuItem.Header = _localizationService.GetString("Open");
        saveMenuItem.Header = _localizationService.GetString("Save");
        saveAsMenuItem.Header = _localizationService.GetString("SaveAs");
        printMenuItem.Header = "🖨️ " + (_localizationService.GetString("Print") ?? "Drucken");
        exitMenuItem.Header = _localizationService.GetString("Exit");
        
        // Ansicht-Menü
        viewMenuItem.Header = _localizationService.GetString("View");
        overviewModeMenuItem.Header = _localizationService.GetString("TournamentOverview");
        
        // API-Menü
        //apiMenuItem.Header = "🌐 " + (_localizationService.GetString("API") ?? "API");
        //startApiMenuItem.Header = "▶️ " + (_localizationService.GetString("StartAPI") ?? "API starten");
        //stopApiMenuItem.Header = "⏹️ " + (_localizationService.GetString("StopAPI") ?? "API stoppen");
        //openApiDocsMenuItem.Header = "📖 " + (_localizationService.GetString("APIDocumentation") ?? "API Dokumentation");
        
        // Tournament Hub-Menü
        tournamentHubMenuItem.Header = "🎯 " + (_localizationService.GetString("TournamentHub") ?? "Tournament Hub");
        registerWithHubMenuItem.Header = "🏁 " + (_localizationService.GetString("RegisterWithHub") ?? "Bei Hub registrieren");
        unregisterFromHubMenuItem.Header = "📴 " + (_localizationService.GetString("UnregisterFromHub") ?? "Vom Hub entregistrieren");
        showJoinUrlMenuItem.Header = "📱 " + (_localizationService.GetString("ShowJoinURL") ?? "Join-URL anzeigen");
        manualSyncMenuItem.Header = "🔄 " + (_localizationService.GetString("ManualSync") ?? "Manuell synchronisieren");
        hubSettingsMenuItem.Header = "⚙️ " + (_localizationService.GetString("HubSettings") ?? "Hub-Einstellungen");
        
        // NEU: Lizenz-Menü
        licenseMenuItem.Header = "🔑 " + (_localizationService.GetString("License") ?? "Lizenz");
        licenseStatusMenuItem.Header = "📊 " + (_localizationService.GetString("LicenseStatus") ?? "Lizenz-Status");
        activateLicenseMenuItem.Header = "✨ " + (_localizationService.GetString("ActivateLicense") ?? "Lizenz aktivieren");
        licenseInfoMenuItem.Header = "📋 " + (_localizationService.GetString("LicenseInfo") ?? "Lizenz-Informationen");
        removeLicenseMenuItem.Header = "🗑️ " + (_localizationService.GetString("RemoveLicense") ?? "Lizenz entfernen");
        purchaseLicenseMenuItem.Header = "🛒 " + (_localizationService.GetString("PurchaseLicense") ?? "Lizenz kaufen");
        
        // Hilfe-Menü
        settingsMenuItem.Header = _localizationService.GetString("Settings");
        helpMenuItem.Header = _localizationService.GetString("Help");
        helpContentMenuItem.Header = "📖 " + _localizationService.GetString("Help");
        bugReportMenuItem.Header = _localizationService.GetString("BugReport");
        aboutMenuItem.Header = _localizationService.GetString("About");
    }
}