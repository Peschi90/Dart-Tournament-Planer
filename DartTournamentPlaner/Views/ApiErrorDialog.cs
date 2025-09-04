using System;
using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views
{
    /// <summary>
    /// API Error Dialog im PrintDialog-Stil
    /// </summary>
    public partial class ApiErrorDialog : Window
    {
        private readonly string _errorMessage;
        private readonly string? _errorCode;
        private readonly string? _errorDetails;
        private readonly string? _technicalDetails;
        private readonly LocalizationService? _localizationService;

        public bool RetryRequested { get; private set; } = false;

        public ApiErrorDialog(
            string errorMessage,
            string? errorCode = null,
            string? errorDetails = null,
            string? technicalDetails = null,
            LocalizationService? localizationService = null)
        {
            _errorMessage = errorMessage;
            _errorCode = errorCode;
            _errorDetails = errorDetails;
            _technicalDetails = technicalDetails;
            _localizationService = localizationService;
            
            InitializeComponent();
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            // Setze Error-spezifische Daten
            MessageText.Text = _errorMessage;
            
            if (!string.IsNullOrEmpty(_errorCode))
            {
                ErrorCodeText.Text = _errorCode;
            }
            else
            {
                ErrorCodeSection.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrEmpty(_errorDetails))
            {
                ErrorDetailsText.Text = _errorDetails;
            }
            else
            {
                ErrorDetailsText.Text = _errorMessage;
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
                Title = _localizationService.GetString("ApiConnectionErrorTitle") ?? "API-Verbindungsfehler";
                TitleText.Text = _localizationService.GetString("ApiConnectionErrorTitle") ?? "API-Verbindungsfehler";

                ErrorDetailsTitle.Text = _localizationService.GetString("ErrorDetails") ?? "🔍 Fehler-Details";
                
                SolutionsTitle.Text = _localizationService.GetString("PossibleSolutions") ?? "💡 Mögliche Lösungen";
                SolutionsText.Text = _localizationService.GetString("ApiErrorSolutions") ?? 
                    "- Internetverbindung überprüfen\n- API-Server-Verfügbarkeit prüfen\n- Firewall- und Sicherheitseinstellungen überprüfen\n- In wenigen Momenten erneut versuchen\n- Support kontaktieren falls Problem weiterhin besteht";

                ActionText.Text = _localizationService.GetString("ApiErrorActionText") ?? 
                    "Sie können die Anwendung im Offline-Modus verwenden oder die Verbindung erneut versuchen.";
                
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
        /// Factory-Methode für den API Error Dialog
        /// </summary>
        public static bool ShowApiErrorDialog(
            Window? owner,
            string errorMessage,
            string? errorCode = null,
            string? errorDetails = null,
            string? technicalDetails = null,
            LocalizationService? localizationService = null)
        {
            try
            {
                var dialog = new ApiErrorDialog(errorMessage, errorCode, errorDetails, technicalDetails, localizationService);
                if (owner != null)
                {
                    dialog.Owner = owner;
                }
                
                var result = dialog.ShowDialog();
                var retryRequested = result == true && dialog.RetryRequested;
                
                System.Diagnostics.Debug.WriteLine($"✅ API error dialog shown. Retry requested: {retryRequested}");
                
                return retryRequested;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error showing API error dialog: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Zeigt einen einfachen API Error Dialog mit nur einer Fehlermeldung
        /// </summary>
        public static bool ShowSimpleApiError(
            Window? owner,
            string errorMessage,
            LocalizationService? localizationService = null)
        {
            return ShowApiErrorDialog(owner, errorMessage, null, null, null, localizationService);
        }

        /// <summary>
        /// Zeigt einen API Error Dialog mit HTTP Status Code
        /// </summary>
        public static bool ShowHttpApiError(
            Window? owner,
            string errorMessage,
            int statusCode,
            string? responseContent = null,
            LocalizationService? localizationService = null)
        {
            var errorCode = $"HTTP {statusCode}";
            return ShowApiErrorDialog(owner, errorMessage, errorCode, responseContent, null, localizationService);
        }

        /// <summary>
        /// Zeigt einen API Error Dialog mit Exception Details
        /// </summary>
        public static bool ShowExceptionApiError(
            Window? owner,
            string errorMessage,
            Exception exception,
            LocalizationService? localizationService = null)
        {
            var errorCode = exception.GetType().Name;
            var technicalDetails = $"{exception.Message}\n\n{exception.StackTrace}";
            return ShowApiErrorDialog(owner, errorMessage, errorCode, exception.Message, technicalDetails, localizationService);
        }
    }
}