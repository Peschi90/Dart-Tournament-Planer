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
        
        InitializeComponent();
        DataContext = this;
        
        // Subscribe to language changes to update this window's translations
        _localizationService.PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(_localizationService.CurrentLanguage) || 
                e.PropertyName == nameof(_localizationService.CurrentTranslations))
            {
                UpdateTranslations();
            }
        };
        
        LoadCurrentConfig();
        UpdateTranslations();
    }

    private void LoadCurrentConfig()
    {
        // WICHTIG: Verwende die Referenz auf das echte Config-Objekt, nicht eine Kopie
        _config = _configService.Config;
        
        UpdateUI();
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
        System.Diagnostics.Debug.WriteLine($"SettingsWindow.UpdateTranslations: Updating with language '{_localizationService.CurrentLanguage}'");
        
        Title = _localizationService.GetString("Settings");
        SaveButton.Content = _localizationService.GetString("Save");
        CancelButton.Content = _localizationService.GetString("Cancel");
        AutoSaveCheckBox.Content = _localizationService.GetString("AutoSave");
        
        // Update other UI elements if they exist
        try
        {
            // Language label and other elements would be updated here if they have translation keys
            System.Diagnostics.Debug.WriteLine($"SettingsWindow.UpdateTranslations: Successfully updated UI elements");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SettingsWindow.UpdateTranslations: Error updating UI: {ex.Message}");
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var newLanguage = LanguageComboBox.SelectedValue?.ToString() ?? "de";
            var languageChanged = _configService.Config.Language != newLanguage;
            
            System.Diagnostics.Debug.WriteLine($"SettingsWindow.SaveButton_Click: Current language: {_configService.Config.Language}, New language: {newLanguage}, Changed: {languageChanged}");
            
            // Update other config settings (but NOT language yet)
            _configService.Config.Theme = ThemeComboBox.Text;
            _configService.Config.AutoSave = AutoSaveCheckBox.IsChecked ?? false;
            
            if (int.TryParse(AutoSaveIntervalTextBox.Text, out int interval) && interval > 0)
            {
                _configService.Config.AutoSaveInterval = interval;
            }

            // Save config first
            await _configService.SaveConfigAsync();
            System.Diagnostics.Debug.WriteLine($"SettingsWindow.SaveButton_Click: Config saved");
            
            // WICHTIG: Trigger language change NACH dem Speichern der anderen Einstellungen
            // Das ChangeLanguageAsync wird sowohl die Config setzen als auch das Event feuern
            if (languageChanged)
            {
                System.Diagnostics.Debug.WriteLine($"SettingsWindow.SaveButton_Click: Triggering language change via ChangeLanguageAsync");
                
                // Diese Methode wird die Sprache setzen UND das LanguageChanged Event feuern
                await _configService.ChangeLanguageAsync(newLanguage);
                
                // Warte auf alle UI-Updates
                await Dispatcher.BeginInvoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
                
                System.Diagnostics.Debug.WriteLine($"SettingsWindow.SaveButton_Click: Language change completed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"SettingsWindow.SaveButton_Click: No language change needed");
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SettingsWindow.SaveButton_Click: ERROR: {ex.Message}");
            
            var errorTitle = _localizationService.GetString("Error");
            var errorMessage = $"{_localizationService.GetString("ErrorSavingData")} {ex.Message}";
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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