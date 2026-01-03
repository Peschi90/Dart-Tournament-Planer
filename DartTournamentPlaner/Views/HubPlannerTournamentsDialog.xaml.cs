using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Services.License;

namespace DartTournamentPlaner.Views
{
    public partial class HubPlannerTournamentsDialog : Window
    {
        private readonly LicensedHubService _hubService;
        private readonly LocalizationService? _localizationService;
        private readonly LicenseManager? _licenseManager;

        public HubPlannerTournamentsDialog()
        {
            InitializeComponent();
        }

        public HubPlannerTournamentsDialog(LicensedHubService hubService, LocalizationService? localizationService, LicenseManager? licenseManager) : this()
        {
            _hubService = hubService;
            _localizationService = localizationService;
            _licenseManager = licenseManager;

            ApplyLocalization();
            LoadLicenseKey();
        }

        public static void ShowDialog(Window owner, LicensedHubService hubService, LocalizationService? localizationService, LicenseManager? licenseManager)
        {
            var dialog = new HubPlannerTournamentsDialog(hubService, localizationService, licenseManager)
            {
                Owner = owner
            };

            dialog.ShowDialog();
        }

        private void ApplyLocalization()
        {
            try
            {
                HeaderTitle.Text = _localizationService?.GetString("PlannerFetchDialogTitle") ?? HeaderTitle.Text;
                HeaderSubtitle.Text = _localizationService?.GetString("PlannerFetchDialogSubtitle") ?? HeaderSubtitle.Text;
                DescriptionText.Text = _localizationService?.GetString("PlannerFetchDescription") ?? DescriptionText.Text;
                LicenseLabel.Text = _localizationService?.GetString("PlannerFetchLicenseLabel") ?? LicenseLabel.Text;
                DaysLabel.Text = _localizationService?.GetString("PlannerFetchDaysLabel") ?? DaysLabel.Text;
                FetchButton.Content = _localizationService?.GetString("PlannerFetchFetchButton") ?? FetchButton.Content;
                CloseButton.Content = _localizationService?.GetString("PlannerFetchCloseButton") ?? CloseButton.Content;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? Error applying localization: {ex.Message}");
            }
        }

        private void LoadLicenseKey()
        {
            try
            {
                var storedKey = _licenseManager?.GetStoredLicenseKey();
                LicenseKeyTextBox.Text = storedKey ?? string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"?? Could not load stored license key: {ex.Message}");
                LicenseKeyTextBox.Text = string.Empty;
            }
        }

        private async void FetchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FetchButton.IsEnabled = false;
                StatusText.Text = _localizationService?.GetString("PlannerFetchStatusLoading") ?? "Loading tournaments...";

                if (string.IsNullOrWhiteSpace(LicenseKeyTextBox.Text))
                {
                    var message = _localizationService?.GetString("PlannerFetchLicenseMissing") ?? "No license key available.";
                    StatusText.Text = message;
                    MessageBox.Show(message, _localizationService?.GetString("PlannerFetchDialogTitle") ?? "Planner Hub", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var days = 14;
                if (!int.TryParse(DaysTextBox.Text, out days) || days <= 0)
                {
                    days = 14;
                }

                var response = await _hubService.FetchPlannerTournamentsAsync(days);
                var tournaments = response?.Tournaments ?? new List<PlannerTournamentSummary>();

                TournamentsItemsControl.ItemsSource = tournaments;

                if (tournaments.Count == 0)
                {
                    StatusText.Text = _localizationService?.GetString("PlannerFetchStatusEmpty") ?? "No tournaments found.";
                }
                else
                {
                    StatusText.Text = string.Format(
                        _localizationService?.GetString("PlannerFetchSummary") ?? "{0} tournaments received",
                        tournaments.Count);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error fetching planner tournaments: {ex.Message}");
                var title = _localizationService?.GetString("PlannerFetchDialogTitle") ?? "Planner Hub";
                var format = _localizationService?.GetString("PlannerFetchStatusError") ?? "Could not fetch tournaments: {0}";
                var message = string.Format(format, ex.Message);
                StatusText.Text = message;
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                FetchButton.IsEnabled = true;
            }
        }

        private void CopyId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string tournamentId && !string.IsNullOrWhiteSpace(tournamentId))
                {
                    Clipboard.SetText(tournamentId);
                    StatusText.Text = _localizationService?.GetString("PlannerFetchCopied") ?? "Tournament ID copied.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error copying tournament ID: {ex.Message}");
            }
        }

        private void TournamentCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement element && element.DataContext is PlannerTournamentSummary summary)
                {
                    HubPlannerTournamentDetailsDialog.ShowDialog(this, summary, _localizationService);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? Error opening tournament details: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
