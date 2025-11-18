using System.IO;
using System.Text.Json;
using DartTournamentPlaner.Models.PowerScore;

namespace DartTournamentPlaner.Services.PowerScore;

/// <summary>
/// Service für das Speichern und Laden von PowerScoring Sessions
/// </summary>
public class PowerScoringPersistenceService
{
    private const string SessionFileName = "powerscoring_session.json";
    private readonly string _sessionFilePath;
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true  // ✅ FIX: Case-insensitive deserialization
    };

    public PowerScoringPersistenceService()
    {
        // Speichere im AppData-Ordner
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DartTournamentPlaner",
            "PowerScoring"
        );
        
        Directory.CreateDirectory(appDataPath);
        _sessionFilePath = Path.Combine(appDataPath, SessionFileName);
        
        System.Diagnostics.Debug.WriteLine($"?? PowerScoring persistence path: {_sessionFilePath}");
    }

    /// <summary>
    /// Speichert die aktuelle Session als JSON
    /// </summary>
    public bool SaveSession(PowerScoringSession session)
    {
        try
        {
            var json = JsonSerializer.Serialize(session, JsonOptions);
            File.WriteAllText(_sessionFilePath, json);
            
            System.Diagnostics.Debug.WriteLine($"? PowerScoring session saved: {session.SessionId}");
            System.Diagnostics.Debug.WriteLine($"   Players: {session.Players.Count}, Status: {session.Status}");
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error saving PowerScoring session: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Lädt die gespeicherte Session
    /// </summary>
    public PowerScoringSession? LoadSession()
    {
        try
        {
            if (!File.Exists(_sessionFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"?? No saved PowerScoring session found");
                return null;
            }

            var json = File.ReadAllText(_sessionFilePath);
            var session = JsonSerializer.Deserialize<PowerScoringSession>(json, JsonOptions);
            
            if (session != null)
            {
                System.Diagnostics.Debug.WriteLine($"? PowerScoring session loaded: {session.SessionId}");
                System.Diagnostics.Debug.WriteLine($"   Players: {session.Players.Count}, Status: {session.Status}");
            }
            
            return session;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error loading PowerScoring session: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Prüft ob eine gespeicherte Session existiert
    /// </summary>
    public bool HasSavedSession()
    {
        return File.Exists(_sessionFilePath);
    }

    /// <summary>
    /// Löscht die gespeicherte Session
    /// </summary>
    public bool DeleteSession()
    {
        try
        {
            if (File.Exists(_sessionFilePath))
            {
                File.Delete(_sessionFilePath);
                System.Diagnostics.Debug.WriteLine($"??? PowerScoring session deleted");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Error deleting PowerScoring session: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gibt den Pfad zur Session-Datei zurück
    /// </summary>
    public string GetSessionFilePath() => _sessionFilePath;

    /// <summary>
    /// Automatisches Speichern bei Änderungen
    /// </summary>
    public void EnableAutoSave(PowerScoringSession session, PowerScoringService service)
    {
        // Bei jedem Player-Update speichern
        service.PlayerScoreUpdated += (sender, player) =>
        {
            SaveSession(session);
        };
        
        System.Diagnostics.Debug.WriteLine($"?? Auto-save enabled for PowerScoring session");
    }
}
