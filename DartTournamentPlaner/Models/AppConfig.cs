using System;
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
    private string? _authSessionToken;
    private string? _authUsername;
    private bool _rememberAuthSession;
    private bool _useNewPlannerRegistration = false;
    private string _tournamentLocation = string.Empty;
    private string _tournamentName = string.Empty;
    private string _tournamentDescription = string.Empty;
    private string? _tournamentStartTimeIso;
    private bool _featurePowerScoring = false;
    private bool _featureQrRegistration = false;
    private bool _featurePublicView = true;
    private int _tournamentTotalPlayers = 0;

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

    /// <summary>
    /// Gespeicherter Session-Token für die Benutzeranmeldung (optional)
    /// </summary>
    public string? AuthSessionToken
    {
        get => _authSessionToken;
        set
        {
            _authSessionToken = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Zuletzt verwendeter Benutzername für den Login-Dialog
    /// </summary>
    public string? AuthUsername
    {
        get => _authUsername;
        set
        {
            _authUsername = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Steuert, ob die Session lokal gemerkt werden soll
    /// </summary>
    public bool RememberAuthSession
    {
        get => _rememberAuthSession;
        set
        {
            _rememberAuthSession = value;
            OnPropertyChanged();
        }
    }

    public string TournamentLocation
    {
        get => _tournamentLocation;
        set
        {
            _tournamentLocation = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public string TournamentName
    {
        get => _tournamentName;
        set
        {
            _tournamentName = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    public string TournamentDescription
    {
        get => _tournamentDescription;
        set
        {
            _tournamentDescription = value ?? string.Empty;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// ISO-8601 Startzeit des Turniers (UTC oder lokale Zeit, je nach Eingabe)
    /// </summary>
    public string? TournamentStartTimeIso
    {
        get => _tournamentStartTimeIso;
        set
        {
            _tournamentStartTimeIso = value;
            OnPropertyChanged();
        }
    }

    public bool FeaturePowerScoring
    {
        get => _featurePowerScoring;
        set
        {
            _featurePowerScoring = value;
            OnPropertyChanged();
        }
    }

    public bool FeatureQrRegistration
    {
        get => _featureQrRegistration;
        set
        {
            _featureQrRegistration = value;
            OnPropertyChanged();
        }
    }

    public bool FeaturePublicView
    {
        get => _featurePublicView;
        set
        {
            _featurePublicView = value;
            OnPropertyChanged();
        }
    }

    public int TournamentTotalPlayers
    {
        get => _tournamentTotalPlayers;
        set
        {
            _tournamentTotalPlayers = Math.Max(0, value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Steuert, ob die neue Planner-Registrierungsroutine verwendet wird
    /// </summary>
    public bool UseNewPlannerRegistration
    {
        get => _useNewPlannerRegistration;
        set
        {
            _useNewPlannerRegistration = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}