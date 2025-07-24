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
        if (_config.Language != language)
        {
            _config.Language = language;
            await SaveConfigAsync();
            LanguageChanged?.Invoke(this, language);
        }
    }
}