using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;
using DartTournamentPlaner.Views.Auth;
using DartTournamentPlaner.Views.Common;
using DartTournamentPlaner.Views.License;
using Microsoft.Win32;

namespace DartTournamentPlaner.Helpers;

/// <summary>
/// Verwaltet alle Event-Handlers für Menü-Events und UI-Interaktionen
/// Trennt die Event-Handler-Logik vom MainWindow
/// </summary>
public class MainWindowEventHandlers
{
    private readonly MainWindow _mainWindow;
    private readonly MainWindowServiceInitializer _services;
    private readonly Func<Task> _saveDataInternal;
    private readonly Action _markAsChanged;
    private readonly Action _updateTranslations;
    private readonly Action _configureTournamentTabs;

    public MainWindowEventHandlers(
        MainWindow mainWindow,
        MainWindowServiceInitializer services,
        Func<Task> saveDataInternal,
        Action markAsChanged,
        Action updateTranslations,
        Action configureTournamentTabs)
    {
        _mainWindow = mainWindow;
        _services = services;
        _saveDataInternal = saveDataInternal;
        _markAsChanged = markAsChanged;
        _updateTranslations = updateTranslations;
        _configureTournamentTabs = configureTournamentTabs;
    }

    #region File Menu Handlers

    public void OnNew(object sender, RoutedEventArgs e)
    {
        var result = TournamentDialogHelper.ShowCreateNewTournamentConfirmation(_mainWindow, _services.LocalizationService);

        if (result)
        {
            _services.TournamentService.ResetAllTournaments();
            _configureTournamentTabs();
        }
    }

    public void OnOpen(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Tournament files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            var title = _services.LocalizationService.GetString("Information");
            var message = _services.LocalizationService.GetString("CustomFileNotImplemented");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public async void OnSave(object sender, RoutedEventArgs e)
    {
        await _saveDataInternal();
    }

    public void OnSaveAs(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Tournament files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dialog.ShowDialog() == true)
        {
            var title = _services.LocalizationService.GetString("Information");
            var message = _services.LocalizationService.GetString("CustomFileSaveNotImplemented");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public void OnPrint(object sender, RoutedEventArgs e)
    {
        try
        {
            // TEMPORARY DEBUG: Zeige Debug-Dialog vor dem Print
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift))
            {
                LicenseDebugDialog.ShowDebugDialog(_mainWindow, _services.LicenseFeatureService, _services.LicenseManager, _services.LocalizationService);
                return;
            }

            // Hole inneren HubIntegrationService
            HubIntegrationService? hubIntegrationService = _services.HubService?.InnerHubService;

            // Hole Tournament-ID
            string? tournamentId = _services.TournamentService.GetTournamentData()?.TournamentId;

            if (hubIntegrationService != null)
            {
                Debug.WriteLine("[Print] HubIntegrationService erfolgreich extrahiert für QR-Codes");
                Debug.WriteLine($"[Print] Hub registered: {hubIntegrationService.IsRegisteredWithHub}");
                Debug.WriteLine($"[Print] Tournament ID: {tournamentId ?? "null"}");
            }
            else
            {
                Debug.WriteLine("[Print] Kein HubIntegrationService verfügbar - Drucke ohne QR-Codes");
            }

            // Verwende PrintHelper mit Tournament-ID
            PrintHelper.ShowPrintDialog(
                _services.TournamentService.AllTournamentClasses,
                _services.TournamentService.PlatinClass,
                _mainWindow,
                _services.LocalizationService,
                _services.LicenseFeatureService,
                _services.LicenseManager,
                hubIntegrationService,
                tournamentId);
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Druckdialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            Debug.WriteLine($"[Print] Fehler: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region View Menu Handlers

    public void OnOverviewMode(object sender, RoutedEventArgs e)
    {
        try
        {
            // Hole Tournament-ID
            var tournamentData = _services.TournamentService.GetTournamentData();
            var tournamentId = tournamentData?.TournamentId;

            // Hole inneren HubService
            HubIntegrationService? hubIntegrationService = _services.HubService?.InnerHubService;

            // Erstelle Tournament Overview Window
            var overviewWindow = new TournamentOverviewWindow(
                _services.TournamentService.AllTournamentClasses,
                _services.LocalizationService,
                hubIntegrationService,
                _services.LicenseFeatureService,
                tournamentId
            );

            overviewWindow.Owner = _mainWindow;
            overviewWindow.ShowDialog();

            Debug.WriteLine("✅ Tournament Overview window opened");
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error") ?? "Error";
            var message = $"Error opening Tournament Overview: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            Debug.WriteLine($"❌ Tournament Overview Error: {ex.Message}");
        }
    }

    public void OnPowerScoring(object sender, RoutedEventArgs e)
    {
        try
        {
            // Prüfe PowerScoring-Feature
            if (!_services.LicenseFeatureService.IsFeatureEnabled(Models.License.LicenseFeatures.POWERSCORING))
            {
                var requestLicense = PowerScoringLicenseRequiredDialog.ShowDialog(
                    _mainWindow,
                    _services.LocalizationService,
                    _services.LicenseManager);

                if (requestLicense)
                {
                    // Öffne Purchase Dialog
                    OnPurchaseLicense(sender, e);
                }

                return;
            }

            // Öffne PowerScoring-Fenster
            var powerScoringWindow = new PowerScoringWindow(
                _services.PowerScoringService,
                _services.LocalizationService,
                _services.HubService,
                _services.ConfigService,
                _services.TournamentService,
                _mainWindow);

            powerScoringWindow.Owner = _mainWindow;
            powerScoringWindow.ShowDialog();

            Debug.WriteLine("✅ PowerScoring-Fenster geöffnet");
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen von PowerScoring: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            Debug.WriteLine($"❌ PowerScoring Error: {ex.Message}");
        }
    }

    public void OnToggleDarkMode(object sender, RoutedEventArgs e)
    {
        try
        {
            App.ThemeService?.ToggleTheme();

            // Menü-Text aktualisieren
            var currentTheme = App.ThemeService?.GetCurrentTheme() ?? "Light";
            var isDark = currentTheme.ToLower() == "dark";
            var translationKey = isDark ? "SwitchToLightMode" : "SwitchToDarkMode";
            var translatedText = _services.LocalizationService.GetString(translationKey);

            var newText = isDark ? "☀️ " : "🌙 ";
            if (!string.IsNullOrEmpty(translatedText) && translatedText != translationKey)
            {
                newText += translatedText;
            }
            else
            {
                newText += isDark ? "Switch to Light Mode" : "Switch to Dark Mode";
            }

            if (sender is System.Windows.Controls.MenuItem menuItem)
            {
                menuItem.Header = newText;
            }

            Debug.WriteLine($"🎨 Theme toggled to: {currentTheme}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error toggling dark mode: {ex.Message}");

            var title = _services.LocalizationService.GetString("Error") ?? "Error";
            var message = $"Error switching theme: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    #endregion

    #region Settings Menu Handlers

    public void OnSettings(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_services.ConfigService, _services.LocalizationService, App.ThemeService);
        settingsWindow.Owner = _mainWindow;

        if (settingsWindow.ShowDialog() == true)
        {
            // Auto-save timer wird über Config-Event automatisch aktualisiert
        }
    }

    #endregion

    #region Help Menu Handlers

    public void OnHelp(object sender, RoutedEventArgs e)
    {
        try
        {
            var helpWindow = new HelpWindow(_services.LocalizationService);
            helpWindow.Owner = _mainWindow;
            helpWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error");
            var message = $"{_services.LocalizationService.GetString("ErrorOpeningHelp")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnBugReport(object sender, RoutedEventArgs e)
    {
        try
        {
            var bugReportDialog = new BugReportDialog(_services.LocalizationService);
            bugReportDialog.Owner = _mainWindow;
            bugReportDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error");
            var message = $"{_services.LocalizationService.GetString("Error")}: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnAbout(object sender, RoutedEventArgs e)
    {
        try
        {
            AboutDialog.ShowDialog(_mainWindow, _services.LocalizationService);
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("About");
            var message = _services.LocalizationService.GetString("AboutText");
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    #endregion

    #region Donation Handlers

    public void OnDonation(object sender, RoutedEventArgs e)
    {
        try
        {
            var donationDialog = new DonationDialog(_services.LocalizationService);
            donationDialog.Owner = _mainWindow;
            donationDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error");
            var message = $"{_services.LocalizationService.GetString("Error")}: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region License Menu Handlers - Delegiert an MainWindowLicenseHandlers

    public void OnLicenseStatus(object sender, RoutedEventArgs e)
    {
        try
        {
            var statusWindow = new LicenseStatusWindow(_services.LicenseManager, _services.LicenseFeatureService, _services.LocalizationService);
            statusWindow.Owner = _mainWindow;
            statusWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Lizenz-Status: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async void OnActivateLicense(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new SimpleLicenseActivationDialog(_services.LocalizationService, _services.LicenseManager);

            if (await dialog.ShowDialogAsync())
            {
                // Lizenz wurde erfolgreich aktiviert
                await _services.LicenseFeatureService.InitializeAsync();
                Debug.WriteLine($"✅ License activated and UI updated");
            }
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Aktivieren der Lizenz: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnLicenseInfo(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new SimpleLicenseInfoDialog(_services.LicenseManager, _services.LicenseFeatureService, _services.LocalizationService);
            dialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Anzeigen der Lizenz-Informationen: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void OnPurchaseLicense(object sender, RoutedEventArgs e)
    {
        try
        {
            var purchaseDialog = new PurchaseLicenseDialog(_services.LocalizationService, _services.LicenseManager);
            purchaseDialog.Owner = _mainWindow;
            purchaseDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            var title = _services.LocalizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Lizenzkauf-Dialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async void OnRemoveLicense(object sender, RoutedEventArgs e)
    {
        try
        {
            var title = _services.LocalizationService.GetString("RemoveLicense") ?? "Lizenz entfernen";
            var message = _services.LocalizationService.GetString("RemoveLicenseConfirmation") ??
                "Möchten Sie die aktivierte Lizenz wirklich entfernen?\n\n" +
                "• Die Anwendung wird danach als unlizenziert ausgeführt\n" +
                "• Alle Core-Features bleiben verfügbar\n" +
                "• Sie können jederzeit eine neue Lizenz aktivieren\n\n" +
                "Fortfahren?";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _services.LicenseManager.RemoveLicenseAndResetAsync();

                if (success)
                {
                    await _services.LicenseFeatureService.InitializeAsync();

                    var successTitle = _services.LocalizationService.GetString("Success") ?? "Erfolg";
                    var successMessage = _services.LocalizationService.GetString("LicenseRemovedSuccess") ??
                        "✅ Lizenz wurde erfolgreich entfernt!\n\n" +
                        "Die Anwendung läuft jetzt im unlizenzierte Modus mit allen Core-Features.\n" +
                        "Sie können jederzeit über das Lizenz-Menü eine neue Lizenz aktivieren.";

                    MessageBox.Show(successMessage, successTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errorTitle = _services.LocalizationService.GetString("Error") ?? "Fehler";
                    var errorMessage = _services.LocalizationService.GetString("LicenseRemoveError") ??
                        "❌ Fehler beim Entfernen der Lizenz.\n\nBitte versuchen Sie es erneut oder kontaktieren Sie den Support.";

                    MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            var errorTitle = _services.LocalizationService.GetString("Error") ?? "Fehler";
            var errorMessage = $"Unerwarteter Fehler beim Entfernen der Lizenz:\n\n{ex.Message}";

            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Account Menu Handlers

    public void OnLogin(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new AuthDialog(_services.UserAuthService, _services.LocalizationService);
            dialog.Owner = _mainWindow;
            dialog.ShowDialog();

            _services.UiHelper.UpdateAuthMenu(_services.UserAuthService, _mainWindow.AccountMenuItem, _mainWindow.LoginMenuItem, _mainWindow.ProfileMenuItem, _mainWindow.LogoutMenuItem);
        }
        catch (Exception ex)
        {
            StyledInfoDialog.Show(
                _services.LocalizationService.GetString("Error") ?? "Fehler",
                $"Fehler beim Öffnen des Login-Dialogs: {ex.Message}",
                _services.LocalizationService);
        }
    }

    public async Task OnLogout(object sender, RoutedEventArgs e)
    {
        try
        {
            await _services.UserAuthService.LogoutAsync();
            _services.UiHelper.UpdateAuthMenu(_services.UserAuthService, _mainWindow.AccountMenuItem, _mainWindow.LoginMenuItem, _mainWindow.ProfileMenuItem, _mainWindow.LogoutMenuItem);

            StyledInfoDialog.Show(
                _services.LocalizationService.GetString("Information") ?? "Info",
                _services.LocalizationService.GetString("LogoutSuccess") ?? "Erfolgreich abgemeldet.",
                _services.LocalizationService,
                isSuccess: true,
                owner: _mainWindow);
        }
        catch (Exception ex)
        {
            StyledInfoDialog.Show(
                _services.LocalizationService.GetString("Error") ?? "Fehler",
                $"Fehler beim Abmelden: {ex.Message}",
                _services.LocalizationService,
                isError: true,
                owner: _mainWindow);
        }
    }

    public void OnProfile(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_services.UserAuthService.CurrentUser == null)
            {
                OnLogin(sender, e);
                return;
            }

            var profileDialog = new ProfileDialog(_services.UserAuthService, _services.LocalizationService, _services.LicenseManager);
            profileDialog.Owner = _mainWindow;
            profileDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            StyledInfoDialog.Show(
                _services.LocalizationService.GetString("Error") ?? "Fehler",
                $"Fehler beim Öffnen des Profils: {ex.Message}",
                _services.LocalizationService,
                isError: true,
                owner: _mainWindow);
        }
    }

    #endregion
}
