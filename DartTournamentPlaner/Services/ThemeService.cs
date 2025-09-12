using System;
using System.Linq;
using System.Windows;

namespace DartTournamentPlaner.Services
{
    /// <summary>
    /// Service für die Verwaltung von Anwendungsthemes (Light/Dark Mode)
    /// </summary>
    public class ThemeService
    {
        private readonly ConfigService _configService;
        
        /// <summary>
        /// Event das gefeuert wird wenn das Theme geändert wird
        /// </summary>
        public event EventHandler<string>? ThemeChanged;
        
        public ThemeService(ConfigService configService)
        {
            _configService = configService;
        }
        
        /// <summary>
        /// Wendet das angegebene Theme auf die Anwendung an
        /// </summary>
        /// <param name="themeName">Name des Themes (Light, Dark)</param>
        public void ApplyTheme(string themeName)
        {
            try
            {
                var themeUri = themeName.ToLower() switch
                {
                    "dark" => new Uri("pack://application:,,,/Themes/DarkTheme.xaml"),
                    "light" => new Uri("pack://application:,,,/Themes/LightTheme.xaml"),
                    _ => new Uri("pack://application:,,,/Themes/LightTheme.xaml")
                };
                
                // Entferne existierendes Theme
                var existingTheme = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme.xaml") == true);
                
                if (existingTheme != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
                }
                
                // Füge neues Theme hinzu
                var themeDict = new ResourceDictionary { Source = themeUri };
                Application.Current.Resources.MergedDictionaries.Insert(0, themeDict);
                
                // Aktualisiere Config
                _configService.Config.Theme = themeName;
                
                // Feuere Theme-Changed Event
                ThemeChanged?.Invoke(this, themeName);
                
                System.Diagnostics.Debug.WriteLine($"ThemeService: Applied theme '{themeName}' and fired ThemeChanged event");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ThemeService.ApplyTheme: Error applying theme '{themeName}': {ex.Message}");
                
                // Fallback auf Light Theme
                if (themeName.ToLower() != "light")
                {
                    ApplyTheme("Light");
                }
            }
        }
        
        /// <summary>
        /// Wechselt zwischen Light und Dark Theme
        /// </summary>
        public void ToggleTheme()
        {
            var currentTheme = _configService.Config.Theme;
            var newTheme = currentTheme?.ToLower() == "dark" ? "Light" : "Dark";
            ApplyTheme(newTheme);
        }
        
        /// <summary>
        /// Gibt das aktuell verwendete Theme zurück
        /// </summary>
        public string GetCurrentTheme()
        {
            return _configService.Config.Theme ?? "Light";
        }
        
        /// <summary>
        /// Prüft, ob gerade das Dark Theme aktiv ist
        /// </summary>
        public bool IsDarkTheme()
        {
            return GetCurrentTheme().ToLower() == "dark";
        }
        
        /// <summary>
        /// Initialisiert das Theme-System beim Anwendungsstart
        /// </summary>
        public void InitializeTheme()
        {
            var savedTheme = _configService.Config.Theme ?? "Light";
            ApplyTheme(savedTheme);
        }
    }
}