using System.Configuration;
using System.Data;
using System.Windows;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static ConfigService? ConfigService { get; private set; }
    public static LocalizationService? LocalizationService { get; private set; }
    public static DataService? DataService { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Initialize services
        ConfigService = new ConfigService();
        LocalizationService = new LocalizationService();
        DataService = new DataService();

        // Load configuration
        await ConfigService.LoadConfigAsync();
        
        // Set initial language
        LocalizationService.CurrentLanguage = ConfigService.Config.Language;

        base.OnStartup(e);
    }
}

