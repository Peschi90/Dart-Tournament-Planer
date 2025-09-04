using System;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;
using DartTournamentPlaner.Views.License;

namespace DartTournamentPlaner.Views
{
    /// <summary>
    /// Hub License Required Dialog im PrintDialog-Stil
    /// </summary>
    public partial class HubLicenseRequiredDialog : Window
    {
        private readonly LocalizationService? _localizationService;
        private readonly LicenseManager? _licenseManager;

        public bool LicenseRequested { get; private set; } = false;

        public HubLicenseRequiredDialog(
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
                Title = _localizationService.GetString("HubLicenseRequiredTitle") ?? "Hub-Lizenz erforderlich";
                TitleText.Text = _localizationService.GetString("HubLicenseRequiredTitle") ?? "Hub-Verbindung-Lizenz erforderlich";
                MessageText.Text = _localizationService.GetString("HubLicenseRequiredMessage") ?? 
                    "Tournament Hub-Verbindung erfordert eine gültige Lizenz mit dem 'Hub Connection' Feature.";

                BenefitsTitle.Text = _localizationService.GetString("HubBenefitsTitle") ?? "🔗 Hub-Verbindung-Features beinhalten:";
                
                var benefitsText = _localizationService.GetString("HubBenefits") ??
                    "- Echtzeit-Turnier-Synchronisation\n" +
                    "- Multi-Device Turnier-Management\n" +
                    "- Live Match-Ergebnis Updates\n" +
                    "- QR-Code-Freigabe für einfachen Zugriff\n" +
                    "- Automatische Daten-Backup und Sync";
                
                BenefitsList.Text = benefitsText;

                ConnectionNoticeTitle.Text = _localizationService.GetString("MultiDeviceManagement") ?? "🌐 Multi-Device Tournament Management";
                ConnectionNoticeText.Text = _localizationService.GetString("MultiDeviceManagementText") ?? 
                    "Hub-Verbindungen ermöglichen nahtloses Turnier-Management über mehrere Geräte mit Echtzeit-Synchronisation.";

                ActionText.Text = _localizationService.GetString("HubLicenseActionText") ?? 
                    "Möchten Sie eine Lizenz mit Hub-Verbindung-Features anfordern?";
                
                CancelButton.Content = "❌ " + (_localizationService.GetString("Cancel") ?? "Abbrechen");
                RequestLicenseButton.Content = "🛒 " + (_localizationService.GetString("RequestHubLicense") ?? "Hub-Lizenz anfordern");
            }
        }

        private void RequestLicenseButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("🛒 HubLicenseRequiredDialog: RequestLicenseButton_Click");
            
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

                System.Diagnostics.Debug.WriteLine("✅ Hub license services available, opening Purchase Dialog...");
                LicenseRequested = OpenPurchaseDialog(_localizationService, _licenseManager);
                DialogResult = LicenseRequested;
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in Hub RequestLicenseButton_Click: {ex.Message}");
                
                var errorMessage = "Fehler beim Öffnen des Hub-Lizenzanfrage-Dialogs";
                
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
        /// Öffnet den Purchase License Dialog für Hub-Features
        /// </summary>
        private bool OpenPurchaseDialog(LocalizationService localizationService, Services.License.LicenseManager licenseManager)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🛒 Hub License: Creating Purchase dialog...");
                
                var purchaseDialog = new PurchaseLicenseDialog(localizationService, licenseManager);
                purchaseDialog.Owner = this;

                System.Diagnostics.Debug.WriteLine("🛒 Hub License: Purchase dialog created, setting up pre-selection...");

                // Pre-select Hub Connection Feature
                purchaseDialog.Loaded += (s, args) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("🎯 Hub License: Purchase dialog loaded, focusing on hub feature...");
                        
                        purchaseDialog.FocusOnHubFeature();
                        
                        System.Diagnostics.Debug.WriteLine("✅ Hub Connection feature pre-selected in Purchase Dialog");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Error pre-selecting hub feature: {ex.Message}");
                    }
                };

                System.Diagnostics.Debug.WriteLine("📋 Showing Hub License Purchase Dialog...");
                var result = purchaseDialog.ShowDialog();
                System.Diagnostics.Debug.WriteLine($"📋 Hub License Purchase Dialog closed with result: {result}");
                
                return result == true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error in Hub OpenPurchaseDialog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Factory-Methode für den Hub License Dialog
        /// </summary>
        public static bool ShowLicenseRequiredDialog(
            Window? owner,
            LocalizationService? localizationService = null,
            LicenseManager? licenseManager = null)
        {
            try
            {
                var dialog = new HubLicenseRequiredDialog(localizationService, licenseManager);
                if (owner != null)
                {
                    dialog.Owner = owner;
                }
                
                var result = dialog.ShowDialog();
                var licenseRequested = result == true && dialog.LicenseRequested;
                
                System.Diagnostics.Debug.WriteLine($"✅ Hub license required dialog shown. License requested: {licenseRequested}");
                
                return licenseRequested;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error showing hub license required dialog: {ex.Message}");
                return false;
            }
        }
    }
}