using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class TournamentMetadataDialog : Window, INotifyPropertyChanged
{
    private readonly TournamentManagementService _tournamentService;
    private readonly ConfigService _configService;
    private readonly LocalizationService _localizationService;

    public TournamentMetadataDialog(ConfigService configService, LocalizationService localizationService, TournamentManagementService tournamentService)
    {
        _configService = configService;
        _localizationService = localizationService;
        _tournamentService = tournamentService;
        InitializeComponent();
        DataContext = this;
        LoadFromConfig();
        UpdateTranslations();

        _localizationService.PropertyChanged += LocalizationServiceOnPropertyChanged;
    }

    private void LocalizationServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocalizationService.CurrentLanguage) ||
            e.PropertyName == nameof(LocalizationService.CurrentTranslations))
        {
            UpdateTranslations();
        }
    }

    private void LoadFromConfig()
    {
        var data = _tournamentService.GetTournamentData();
        TournamentNameTextBox.Text = data.TournamentName;
        TournamentDescriptionTextBox.Text = data.TournamentDescription;
        TournamentLocationTextBox.Text = data.TournamentLocation;

        if (!string.IsNullOrWhiteSpace(data.TournamentStartTimeIso) && DateTime.TryParse(data.TournamentStartTimeIso, out var start))
        {
            StartDatePicker.SelectedDate = start.Date;
            StartTimeTextBox.Text = start.ToString("HH:mm");
        }
        else
        {
            StartDatePicker.SelectedDate = null;
            StartTimeTextBox.Text = string.Empty;
        }

        PowerScoringCheckBox.IsChecked = data.FeaturePowerScoring;
        QrRegistrationCheckBox.IsChecked = data.FeatureQrRegistration;
        PublicViewCheckBox.IsChecked = data.FeaturePublicView;
        TotalPlayersTextBox.Text = data.TournamentTotalPlayers > 0 ? data.TournamentTotalPlayers.ToString() : string.Empty;
    }

    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("TournamentSettings") ?? "Tournament Settings";
        HeaderTitle.Text = _localizationService.GetString("TournamentSettings") ?? "Tournament Settings";
        MetadataLabel.Text = _localizationService.GetString("TournamentMetadata") ?? "Tournament Metadata";
        NameLabel.Text = _localizationService.GetString("TournamentName") ?? "Tournament Name";
        DescriptionLabel.Text = _localizationService.GetString("TournamentDescription") ?? "Description";
        LocationLabel.Text = _localizationService.GetString("TournamentLocation") ?? "Location";
        ScheduleLabel.Text = _localizationService.GetString("TournamentSchedule") ?? "Schedule";
        StartDateLabel.Text = _localizationService.GetString("StartDate") ?? "Start Date";
        StartTimeLabel.Text = _localizationService.GetString("StartTime") ?? "Start Time";
        FeaturesLabel.Text = _localizationService.GetString("Features") ?? "Features";
        TotalPlayersLabel.Text = _localizationService.GetString("TotalPlayers") ?? "Total Players";

        PowerScoringCheckBox.Content = _localizationService.GetString("PowerScoring") ?? "PowerScoring";
        QrRegistrationCheckBox.Content = _localizationService.GetString("QRRegistration") ?? "QR Registration";
        PublicViewCheckBox.Content = _localizationService.GetString("PublicView") ?? "Public View";

        SaveButton.Content = _localizationService.GetString("Save") ?? "Save";
        CancelButton.Content = _localizationService.GetString("Cancel") ?? "Cancel";
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var data = _tournamentService.GetTournamentData();
            data.TournamentName = TournamentNameTextBox.Text?.Trim() ?? string.Empty;
            data.TournamentDescription = TournamentDescriptionTextBox.Text?.Trim() ?? string.Empty;
            data.TournamentLocation = TournamentLocationTextBox.Text?.Trim() ?? string.Empty;

            DateTime? start = null;
            if (StartDatePicker.SelectedDate.HasValue)
            {
                var date = StartDatePicker.SelectedDate.Value;
                if (TimeSpan.TryParse(StartTimeTextBox.Text, out var time))
                {
                    start = date.Date + time;
                }
                else
                {
                    start = date.Date;
                }
            }

            data.TournamentStartTimeIso = start?.ToString("o");
            data.FeaturePowerScoring = PowerScoringCheckBox.IsChecked == true;
            data.FeatureQrRegistration = QrRegistrationCheckBox.IsChecked == true;
            data.FeaturePublicView = PublicViewCheckBox.IsChecked != false;

            if (int.TryParse(TotalPlayersTextBox.Text, out var totalPlayers) && totalPlayers >= 0)
            {
                data.TournamentTotalPlayers = totalPlayers;
            }
            else
            {
                data.TournamentTotalPlayers = 0;
            }

            await _tournamentService.SaveDataAsync();

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, _localizationService.GetString("Error") ?? "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
