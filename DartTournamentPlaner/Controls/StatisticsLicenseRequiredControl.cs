using System;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Views.License;

namespace DartTournamentPlaner.Controls;

/// <summary>
/// Control das angezeigt wird, wenn Statistics-Feature ohne entsprechende Lizenz aufgerufen wird
/// </summary>
public partial class StatisticsLicenseRequiredControl : UserControl
{
    private readonly LocalizationService? _localizationService;
    private readonly LicenseManager? _licenseManager;

    public StatisticsLicenseRequiredControl()
    {
        InitializeComponent();
        InitializeControl();
    }

    public StatisticsLicenseRequiredControl(LocalizationService? localizationService, LicenseManager? licenseManager)
    {
        _localizationService = localizationService;
        _licenseManager = licenseManager;
        
        InitializeComponent();
        InitializeControl();
    }

    private void InitializeControl()
    {
        if (_localizationService != null)
        {
            // Übersetzungen anwenden
            TitleText.Text = _localizationService.GetString("StatisticsLicenseRequiredTitle") ?? "Statistik-Lizenz erforderlich";
            MessageText.Text = _localizationService.GetString("StatisticsLicenseRequiredMessage") ?? 
                "Erweiterte Statistiken erfordern eine gültige Lizenz mit dem 'Statistics' Feature und eine aktive Hub-Verbindung.";

            BenefitsTitle.Text = _localizationService.GetString("StatisticsBenefitsTitle") ?? "📈 Statistik-Features beinhalten:";
            
            var benefitsText = _localizationService.GetString("StatisticsBenefits") ??
                "- Detaillierte Spieler-Performance-Analyse\n" +
                "- Spielverlauf und Trends\n" +
                "- Turnier-Fortschritt-Verfolgung\n" +
                "- Echtzeit-Hub-Synchronisation\n" +
                "- Erweiterte statistische Berichte";
            
            BenefitsList.Text = benefitsText;

            HubNoticeTitle.Text = _localizationService.GetString("HubConnectionRequired") ?? "🔗 Hub-Verbindung erforderlich";
            HubNoticeText.Text = _localizationService.GetString("HubConnectionRequiredText") ?? 
                "Statistik-Features erfordern eine aktive Tournament Hub-Verbindung für Echtzeit-Datensynchronisation.";

            ActionText.Text = _localizationService.GetString("StatisticsLicenseActionText") ?? 
                "Möchten Sie eine Lizenz mit Statistik- und Hub-Features anfordern?";
            
            RequestLicenseButton.Content = "🛒 " + (_localizationService.GetString("RequestStatisticsLicense") ?? "Statistik-Lizenz anfordern");
        }
    }

    private void RequestLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("🛒 RequestLicenseButton_Click: Starting license request process...");
        
        try
        {
            if (_licenseManager == null || _localizationService == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ License Manager or Localization Service not available");
                System.Diagnostics.Debug.WriteLine($"   - LicenseManager: {_licenseManager != null}");
                System.Diagnostics.Debug.WriteLine($"   - LocalizationService: {_localizationService != null}");
                
                // Fallback: Versuche Services vom MainWindow zu holen
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    var licenseManagerField = mainWindow.GetType()
                        .GetField("_licenseManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var localizationServiceField = typeof(App).GetProperty("LocalizationService", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                    var fallbackLicenseManager = licenseManagerField?.GetValue(mainWindow) as Services.License.LicenseManager;
                    var fallbackLocalizationService = localizationServiceField?.GetValue(null) as LocalizationService;

                    System.Diagnostics.Debug.WriteLine($"🔄 Fallback services found:");
                    System.Diagnostics.Debug.WriteLine($"   - Fallback LicenseManager: {fallbackLicenseManager != null}");
                    System.Diagnostics.Debug.WriteLine($"   - Fallback LocalizationService: {fallbackLocalizationService != null}");

                    if (fallbackLicenseManager != null && fallbackLocalizationService != null)
                    {
                        OpenPurchaseDialog(fallbackLocalizationService, fallbackLicenseManager);
                        return;
                    }
                }
                
                MessageBox.Show("Fehler: Lizenz-Services nicht verfügbar", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Diagnostics.Debug.WriteLine("✅ Services available, opening Purchase Dialog...");
            OpenPurchaseDialog(_localizationService, _licenseManager);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error in RequestLicenseButton_Click: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            
            var title = _localizationService?.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Lizenzanfrage-Dialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Öffnet den Purchase License Dialog
    /// </summary>
    private void OpenPurchaseDialog(LocalizationService localizationService, Services.License.LicenseManager licenseManager)
    {
        try
        {
            // Finde das Parent Window
            var parentWindow = Window.GetWindow(this);
            System.Diagnostics.Debug.WriteLine($"🪟 Parent window found: {parentWindow != null}");
            
            // Öffne Purchase License Dialog
            var purchaseDialog = new PurchaseLicenseDialog(localizationService, licenseManager);
            if (parentWindow != null)
            {
                purchaseDialog.Owner = parentWindow;
            }

            System.Diagnostics.Debug.WriteLine("🛒 Purchase dialog created, setting up pre-selection...");

            // Pre-select Statistics and Hub Features
            purchaseDialog.Loaded += (s, args) =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("🎯 Purchase dialog loaded, focusing on statistics feature...");
                    
                    // Aktiviere sowohl Statistics als auch Hub Connection Features
                    purchaseDialog.FocusOnStatisticsFeature();
                    
                    System.Diagnostics.Debug.WriteLine("✅ Statistics and Hub features pre-selected in Purchase Dialog");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error pre-selecting statistics features: {ex.Message}");
                }
            };

            System.Diagnostics.Debug.WriteLine("📋 Showing Purchase Dialog...");
            var result = purchaseDialog.ShowDialog();
            System.Diagnostics.Debug.WriteLine($"📋 Purchase Dialog closed with result: {result}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error in OpenPurchaseDialog: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Statische Factory-Methode um das Control zu erstellen
    /// </summary>
    public static StatisticsLicenseRequiredControl Create(LocalizationService? localizationService, LicenseManager? licenseManager)
    {
        return new StatisticsLicenseRequiredControl(localizationService, licenseManager);
    }
}