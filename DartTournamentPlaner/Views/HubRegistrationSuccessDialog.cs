using System;
using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views
{
    /// <summary>
    /// Tournament Hub Registration Success Dialog im PrintDialog-Stil
    /// </summary>
    public partial class HubRegistrationSuccessDialog : Window
    {
        private readonly string _tournamentId;
        private readonly string _joinUrl;
        private readonly LocalizationService? _localizationService;

        public HubRegistrationSuccessDialog(
            string tournamentId, 
            string joinUrl, 
            LocalizationService? localizationService = null)
        {
            _tournamentId = tournamentId;
            _joinUrl = joinUrl;
            _localizationService = localizationService;
            
            InitializeComponent();
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            // Setze Tournament-spezifische Daten
            TournamentIdText.Text = _tournamentId;
            JoinUrlText.Text = _joinUrl;

            if (_localizationService != null)
            {
                // Übersetzungen anwenden
                Title = _localizationService.GetString("TournamentRegisteredTitle") ?? "Turnier registriert";
                TitleText.Text = _localizationService.GetString("TournamentRegisteredTitle") ?? "Turnier erfolgreich registriert";
                MessageText.Text = _localizationService.GetString("TournamentRegisteredMessage") ?? 
                    "Ihr Turnier wurde erfolgreich beim Tournament Hub registriert!";

                TournamentInfoTitle.Text = _localizationService.GetString("TournamentInformation") ?? "🏆 Turnier-Informationen";
                
                FeaturesTitle.Text = _localizationService.GetString("ActiveFeatures") ?? "🌟 Aktive Features";
                FeaturesText.Text = _localizationService.GetString("TournamentFeaturesText") ?? 
                    "- Echtzeit-Turnier-Synchronisation\n- Multi-Device Turnier-Zugang\n- Live Match-Ergebnis Updates";

                ActionText.Text = _localizationService.GetString("JoinUrlCopiedText") ?? 
                    "Die Join-URL wurde automatisch in die Zwischenablage kopiert.";
                
                CopyUrlButton.Content = "📋 " + (_localizationService.GetString("CopyUrl") ?? "URL kopieren");
                OkButton.Content = "✅ " + (_localizationService.GetString("OK") ?? "OK");
            }

            // Kopiere Join URL automatisch in Zwischenablage
            try
            {
                Clipboard.SetText(_joinUrl);
                System.Diagnostics.Debug.WriteLine($"✅ Join URL copied to clipboard: {_joinUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error copying to clipboard: {ex.Message}");
            }
        }

        private void CopyUrlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_joinUrl);
                
                // Visual feedback
                var originalContent = CopyUrlButton.Content;
                CopyUrlButton.Content = "✅ " + (_localizationService?.GetString("Copied") ?? "Kopiert!");
                
                // Reset nach 2 Sekunden
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += (s, args) =>
                {
                    CopyUrlButton.Content = originalContent;
                    timer.Stop();
                };
                timer.Start();
                
                System.Diagnostics.Debug.WriteLine($"✅ Join URL manually copied: {_joinUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error copying join URL: {ex.Message}");
                
                var title = _localizationService?.GetString("Error") ?? "Fehler";
                var message = _localizationService?.GetString("CopyError") ?? "Fehler beim Kopieren der URL.";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Factory-Methode für den Success Dialog
        /// </summary>
        public static void ShowSuccessDialog(
            Window? owner,
            string tournamentId, 
            string joinUrl, 
            LocalizationService? localizationService)
        {
            try
            {
                var dialog = new HubRegistrationSuccessDialog(tournamentId, joinUrl, localizationService);
                if (owner != null)
                {
                    dialog.Owner = owner;
                }
                
                dialog.ShowDialog();
                
                System.Diagnostics.Debug.WriteLine($"✅ Hub registration success dialog shown for tournament: {tournamentId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error showing hub registration success dialog: {ex.Message}");
            }
        }
    }
}