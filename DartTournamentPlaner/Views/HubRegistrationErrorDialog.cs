using System;
using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views
{
    /// <summary>
    /// Tournament Hub Registration Error Dialog im PrintDialog-Stil
    /// </summary>
    public partial class HubRegistrationErrorDialog : Window
    {
        private readonly string _errorMessage;
        private readonly string? _errorCode;
        private readonly string? _technicalDetails;
        private readonly LocalizationService? _localizationService;

        public bool RetryRequested { get; private set; } = false;

        public HubRegistrationErrorDialog(
            string errorMessage,
            string? errorCode = null,
            string? technicalDetails = null,
            LocalizationService? localizationService = null)
        {
            _errorMessage = errorMessage;
            _errorCode = errorCode;
            _technicalDetails = technicalDetails;
            _localizationService = localizationService;
            
            InitializeComponent();
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            // Setze Error-spezifische Daten
            ErrorMessageText.Text = _errorMessage;
            
            if (!string.IsNullOrEmpty(_errorCode))
            {
                ErrorCodeText.Text = _errorCode;
            }
            else
            {
                ErrorCodeSection.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(_technicalDetails))
            {
                TechnicalDetailsText.Text = _technicalDetails;
            }
            else
            {
                TechnicalDetailsExpander.Visibility = Visibility.Collapsed;
            }

            if (_localizationService != null)
            {
                // Übersetzungen anwenden
                Title = _localizationService.GetString("TournamentRegistrationFailedTitle") ?? "Registrierung fehlgeschlagen";
                TitleText.Text = _localizationService.GetString("TournamentRegistrationFailedTitle") ?? "Turnier-Registrierung fehlgeschlagen";
                MessageText.Text = _localizationService.GetString("TournamentRegistrationFailedMessage") ?? 
                    "Die Turnier-Registrierung beim Tournament Hub ist fehlgeschlagen.";

                ErrorDetailsTitle.Text = _localizationService.GetString("ErrorDetails") ?? "🔍 Fehler-Details";
                
                SolutionsTitle.Text = _localizationService.GetString("PossibleSolutions") ?? "💡 Mögliche Lösungen";
                SolutionsText.Text = _localizationService.GetString("HubErrorSolutions") ?? 
                    "- Internetverbindung überprüfen\n- Hub-URL in den Einstellungen verifizieren\n- In wenigen Momenten erneut versuchen\n- Support kontaktieren falls Problem weiterhin besteht";

                ActionText.Text = _localizationService.GetString("HubErrorActionText") ?? 
                    "Sie können das Turnier lokal weiter verwenden oder erneut versuchen zu registrieren.";
                
                TechnicalDetailsExpander.Header = _localizationService.GetString("TechnicalDetails") ?? "Technische Details";
                
                CloseButton.Content = "❌ " + (_localizationService.GetString("Close") ?? "Schließen");
                RetryButton.Content = "🔄 " + (_localizationService.GetString("Retry") ?? "Wiederholen");
            }
        }

        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            RetryRequested = true;
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RetryRequested = false;
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Factory-Methode für den Error Dialog
        /// </summary>
        public static bool ShowErrorDialog(
            Window? owner,
            string errorMessage,
            string? errorCode = null,
            string? technicalDetails = null,
            LocalizationService? localizationService = null)
        {
            try
            {
                var dialog = new HubRegistrationErrorDialog(errorMessage, errorCode, technicalDetails, localizationService);
                if (owner != null)
                {
                    dialog.Owner = owner;
                }
                
                var result = dialog.ShowDialog();
                var retryRequested = result == true && dialog.RetryRequested;
                
                System.Diagnostics.Debug.WriteLine($"✅ Hub registration error dialog shown. Retry requested: {retryRequested}");
                
                return retryRequested;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error showing hub registration error dialog: {ex.Message}");
                return false;
            }
        }
    }
}