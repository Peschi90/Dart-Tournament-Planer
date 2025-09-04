using System;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Views.License;

namespace DartTournamentPlaner.Views
{
    /// <summary>
    /// Statistics License Required Dialog im PrintDialog-Stil
    /// </summary>
    public partial class StatisticsLicenseRequiredDialog : Window
    {
        private readonly LocalizationService? _localizationService;
        private readonly LicenseManager? _licenseManager;

        public bool LicenseRequested { get; private set; } = false;

        public StatisticsLicenseRequiredDialog(
            LocalizationService? localizationService = null,
            LicenseManager? licenseManager = null)
        {
            _localizationService = localizationService;
            _licenseManager = licenseManager;
            
            InitializeComponent();
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            if (_localizationService != null)
            {
                // Übersetzungen anwenden
                Title = _localizationService.GetString("StatisticsLicenseRequiredTitle") ?? "Statistik-Lizenz erforderlich";
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
                
                CancelButton.Content = "❌ " + (_localizationService.GetString("Cancel") ?? "Abbrechen");
                RequestLicenseButton.Content = "🛒 " + (_localizationService.GetString("RequestStatisticsLicense") ?? "Statistik-Lizenz anfordern");
            }
        }

        private void RequestLicenseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🛒 StatisticsLicenseRequiredDialog: RequestLicenseButton_Click");
            
            try
            {
                if (_licenseManager == null || _localizationService == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ License Manager or Localization Service not available");
                    
                    // Fallback: Versuche Services vom MainWindow zu holen
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        var licenseManagerField = mainWindow.GetType()
                            .GetField("_licenseManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var localizationServiceField = typeof(App).GetProperty("LocalizationService", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                        var fallbackLicenseManager = licenseManagerField?.GetValue(mainWindow) as Services.License.LicenseManager;
                        var fallbackLocalizationService = localizationServiceField?.GetValue(null) as LocalizationService;

                        if (fallbackLicenseManager != null && fallbackLocalizationService != null)
                        {
                            LicenseRequested = OpenPurchaseDialog(fallbackLocalizationService, fallbackLicenseManager);
                            DialogResult = LicenseRequested;
                            Close();
                            return;
                        }
                    }
                    
                    // Use ApiErrorDialog instead of MessageBox
                    ApiErrorDialog.ShowSimpleApiError(
                        this,
                        "Fehler: Lizenz-Services nicht verfügbar. Bitte starten Sie die Anwendung neu.",
                        _localizationService);
                    return;
                }

                System.Diagnostics.Debug.WriteLine("✅ Statistics license services available, opening Purchase Dialog...");
                LicenseRequested = OpenPurchaseDialog(_localizationService, _licenseManager);
                DialogResult = LicenseRequested;
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in Statistics RequestLicenseButton_Click: {ex.Message}");
                
                var errorMessage = "Fehler beim Öffnen des Statistik-Lizenzanfrage-Dialogs";
                
                ApiErrorDialog.ShowExceptionApiError(
                    this,
                    errorMessage,
                    ex,
                    _localizationService);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            LicenseRequested = false;
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Öffnet den Purchase License Dialog für Statistics-Features
        /// </summary>
        private bool OpenPurchaseDialog(LocalizationService localizationService, Services.License.LicenseManager licenseManager)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🛒 Statistics License: Creating Purchase dialog...");
                
                var purchaseDialog = new PurchaseLicenseDialog(localizationService, licenseManager);
                purchaseDialog.Owner = this;

                System.Diagnostics.Debug.WriteLine("🛒 Statistics License: Purchase dialog created, setting up pre-selection...");

                // Pre-select Statistics and Hub Connection Features
                purchaseDialog.Loaded += (s, args) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("🎯 Statistics License: Purchase dialog loaded, focusing on statistics feature...");
                        
                        purchaseDialog.FocusOnStatisticsFeature();
                        
                        System.Diagnostics.Debug.WriteLine("✅ Statistics and Hub features pre-selected in Purchase Dialog");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error pre-selecting statistics features: {ex.Message}");
                    }
                };

                System.Diagnostics.Debug.WriteLine("📋 Showing Statistics License Purchase Dialog...");
                var result = purchaseDialog.ShowDialog();
                System.Diagnostics.Debug.WriteLine($"📋 Statistics License Purchase Dialog closed with result: {result}");
                
                return result == true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in Statistics OpenPurchaseDialog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Factory-Methode für den Statistics License Dialog
        /// </summary>
        public static bool ShowLicenseRequiredDialog(
            Window? owner,
            LocalizationService? localizationService = null,
            LicenseManager? licenseManager = null)
        {
            try
            {
                var dialog = new StatisticsLicenseRequiredDialog(localizationService, licenseManager);
                if (owner != null)
                {
                    dialog.Owner = owner;
                }
                
                var result = dialog.ShowDialog();
                var licenseRequested = result == true && dialog.LicenseRequested;
                
                System.Diagnostics.Debug.WriteLine($"✅ Statistics license required dialog shown. License requested: {licenseRequested}");
                
                return licenseRequested;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error showing statistics license required dialog: {ex.Message}");
                return false;
            }
        }
    }
}