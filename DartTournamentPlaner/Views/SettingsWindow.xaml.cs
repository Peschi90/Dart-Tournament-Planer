using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Views;

public partial class SettingsWindow : Window, INotifyPropertyChanged
{
    private readonly ConfigService _configService;
    private readonly LocalizationService _localizationService;
    private AppConfig _config;

    public AppConfig Config
    {
        get => _config;
        set
        {
            _config = value;
            OnPropertyChanged();
            UpdateUI();
        }
    }

    public SettingsWindow(ConfigService configService, LocalizationService localizationService)
    {
        _configService = configService;
        _localizationService = localizationService;
        _config = new AppConfig();
        
        InitializeComponent();
        DataContext = this;
        
        LoadCurrentConfig();
        UpdateTranslations();
    }

    private void LoadCurrentConfig()
    {
        Config = _configService.Config;
    }

    private void UpdateUI()
    {
        LanguageComboBox.SelectedValue = Config.Language;
        ThemeComboBox.Text = Config.Theme;
        AutoSaveCheckBox.IsChecked = Config.AutoSave;
        AutoSaveIntervalTextBox.Text = Config.AutoSaveInterval.ToString();
    }

    private void UpdateTranslations()
    {
        Title = _localizationService.GetString("Settings");
        SaveButton.Content = _localizationService.GetString("Save");
        CancelButton.Content = _localizationService.GetString("Cancel");
        AutoSaveCheckBox.Content = _localizationService.GetString("AutoSave");
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Update config with UI values
            Config.Language = LanguageComboBox.SelectedValue?.ToString() ?? "de";
            Config.Theme = ThemeComboBox.Text;
            Config.AutoSave = AutoSaveCheckBox.IsChecked ?? false;
            
            if (int.TryParse(AutoSaveIntervalTextBox.Text, out int interval) && interval > 0)
            {
                Config.AutoSaveInterval = interval;
            }

            // Save config
            await _configService.SaveConfigAsync();
            
            // Update localization if language changed
            if (_localizationService.CurrentLanguage != Config.Language)
            {
                _localizationService.CurrentLanguage = Config.Language;
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}