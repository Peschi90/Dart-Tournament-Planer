using System.Configuration;
using System.Data;
using System.Windows;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static ConfigService? ConfigService { get; private set; }
    public static LocalizationService? LocalizationService { get; private set; }
    public static DataService? DataService { get; private set; }

    /// <summary>
    /// Global event that fires when the application language changes
    /// All windows can subscribe to this to update their translations
    /// </summary>
    public static event EventHandler<string>? GlobalLanguageChanged;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Initialize services
        ConfigService = new ConfigService();
        LocalizationService = new LocalizationService();
        DataService = new DataService();

        // Set static reference in Match and KnockoutMatch for localization
        Match.LocalizationService = LocalizationService;
        KnockoutMatch.LocalizationService = LocalizationService;

        // Load configuration first
        await ConfigService.LoadConfigAsync();
        
        // Set initial language from config
        System.Diagnostics.Debug.WriteLine($"App.OnStartup: Setting initial language to '{ConfigService.Config.Language}'");
        LocalizationService.SetLanguage(ConfigService.Config.Language);
        
        // Connect ConfigService language changes to LocalizationService
        ConfigService.LanguageChanged += (sender, language) =>
        {
            System.Diagnostics.Debug.WriteLine($"App.OnStartup: ConfigService LanguageChanged event - setting to '{language}'");
            LocalizationService.SetLanguage(language);
            
            // Fire global language changed event for all windows
            GlobalLanguageChanged?.Invoke(null, language);
        };

        base.OnStartup(e);
    }
}

