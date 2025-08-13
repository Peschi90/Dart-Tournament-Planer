using System.IO;
using Newtonsoft.Json;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services;

public class ConfigService
{
    private readonly string _configPath = "config.json";
    private AppConfig _config = new AppConfig();

    public AppConfig Config => _config;

    public event EventHandler<string>? LanguageChanged;

    public async Task LoadConfigAsync()
    {
        try 
        {
            if (File.Exists(_configPath))
            {
                var json = await File.ReadAllTextAsync(_configPath);
                var loadedConfig = JsonConvert.DeserializeObject<AppConfig>(json);
                if (loadedConfig != null)
                {
                    _config = loadedConfig;
                }
            }
        }
        catch (Exception ex)
        {
            // If config loading fails, use default config
            Console.WriteLine($"Error loading config: {ex.Message}");
        }
    }

    public async Task SaveConfigAsync()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
            await File.WriteAllTextAsync(_configPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
        }
    }

    public async Task ChangeLanguageAsync(string language)
    {
        System.Diagnostics.Debug.WriteLine($"ConfigService.ChangeLanguageAsync: Called with language '{language}'");
        System.Diagnostics.Debug.WriteLine($"ConfigService.ChangeLanguageAsync: Current language is '{_config.Language}'");
        
        if (_config.Language != language)
        {
            System.Diagnostics.Debug.WriteLine($"ConfigService.ChangeLanguageAsync: Language changed - updating config and firing event");
            
            _config.Language = language;
            await SaveConfigAsync();
            
            System.Diagnostics.Debug.WriteLine($"ConfigService.ChangeLanguageAsync: Firing LanguageChanged event");
            LanguageChanged?.Invoke(this, language);
            
            System.Diagnostics.Debug.WriteLine($"ConfigService.ChangeLanguageAsync: LanguageChanged event fired");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"ConfigService.ChangeLanguageAsync: No change needed - languages are the same");
        }
    }
}