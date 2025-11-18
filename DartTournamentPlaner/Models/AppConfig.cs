using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

public class AppConfig : INotifyPropertyChanged
{
    private string _language = "de";
    private string _theme = "Modern";
    private bool _autoSave = true;
    private int _autoSaveInterval = 5; // minutes
    private bool _showMatchStartNotifications = false; // ? NEU: Match-Start Benachrichtigungen
    private string _hubUrl = "https://dtp.i3ull3t.de"; // ? FIXED: Standard-URL ohne Port

    public string Language
    {
        get => _language;
        set
        {
            _language = value;
            OnPropertyChanged();
        }
    }

    public string Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            OnPropertyChanged();
        }
    }

    public bool AutoSave
    {
        get => _autoSave;
        set
        {
            _autoSave = value;
            OnPropertyChanged();
        }
    }

    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set
        {
            _autoSaveInterval = value;
            OnPropertyChanged();
        }
    }

    // ? NEU: Hub Live-Match-Updates Einstellungen
    public bool ShowMatchStartNotifications
    {
        get => _showMatchStartNotifications;
        set
        {
            _showMatchStartNotifications = value;
            OnPropertyChanged();
        }
    }

    // ? NEU: Konfigurierbare Hub-URL
    /// <summary>
    /// URL des Tournament Hub Servers (z.B. https://dtp.i3ull3t.de)
    /// </summary>
    public string HubUrl
    {
        get => _hubUrl;
        set
        {
            _hubUrl = value?.Trim() ?? "https://dtp.i3ull3t.de";
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}