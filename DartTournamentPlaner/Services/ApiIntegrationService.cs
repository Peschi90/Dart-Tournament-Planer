using DartTournamentPlaner.Models;
using System.Diagnostics;
using System.IO;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Event-Argumente für Match-Ergebnis Updates
/// </summary>
public class MatchResultUpdateEventArgs : EventArgs
{
    public int MatchId { get; set; }
    public int ClassId { get; set; }
    public MatchResultDto Result { get; set; } = new();
}

/// <summary>
/// DTO für Match-Ergebnis Updates
/// </summary>
public class MatchResultDto
{
    public int MatchId { get; set; }
    public int Player1Sets { get; set; }
    public int Player2Sets { get; set; }
    public int Player1Legs { get; set; }
    public int Player2Legs { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Service für die Integration der API in die WPF-Anwendung
/// Ermöglicht das Starten und Stoppen der API aus der Hauptanwendung
/// </summary>
public interface IApiIntegrationService
{
    /// <summary>
    /// Startet die API mit den aktuellen Turnierdaten
    /// </summary>
    Task<bool> StartApiAsync(TournamentData tournamentData, int port = 5000);
    
    /// <summary>
    /// Stoppt die API
    /// </summary>
    Task<bool> StopApiAsync();
    
    /// <summary>
    /// Aktualisiert die Turnierdaten in der laufenden API
    /// </summary>
    Task<bool> UpdateTournamentDataAsync(TournamentData tournamentData);
    
    /// <summary>
    /// Prüft ob die API läuft
    /// </summary>
    bool IsApiRunning { get; }
    
    /// <summary>
    /// URL der laufenden API
    /// </summary>
    string? ApiUrl { get; }
    
    /// <summary>
    /// Event für Match-Ergebnis Updates von der API
    /// </summary>
    event EventHandler<MatchResultUpdateEventArgs>? MatchResultUpdated;
}

/// <summary>
/// Implementierung des API Integration Service
/// </summary>
public class ApiIntegrationService : IApiIntegrationService
{
    private Process? _apiProcess;
    private int _currentPort;
    private bool _isRunning;

    public bool IsApiRunning => _isRunning && (_apiProcess?.HasExited == false);
    public string? ApiUrl => IsApiRunning ? $"http://localhost:{_currentPort}" : null;

    public event EventHandler<MatchResultUpdateEventArgs>? MatchResultUpdated;

    /// <summary>
    /// Startet die API als separaten Prozess
    /// </summary>
    public async Task<bool> StartApiAsync(TournamentData tournamentData, int port = 5000)
    {
        try
        {
            if (_isRunning)
            {
                await StopApiAsync();
            }

            _currentPort = port;

            // Versuche die API als separaten Prozess zu starten
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var apiProjectPath = System.IO.Path.Combine(baseDirectory, "..", "..", "..", "..", "DartTournamentPlaner.API");

            // Starte die API als separaten Prozess
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{apiProjectPath}\" --urls http://localhost:{port}",
                WorkingDirectory = baseDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _apiProcess = Process.Start(processInfo);

            if (_apiProcess == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to start API process");
                return false;
            }

            // Warte kurz bis die API gestartet ist
            await Task.Delay(2000);

            _isRunning = true;
            System.Diagnostics.Debug.WriteLine($"API started on http://localhost:{port}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting API: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Stoppt die API
    /// </summary>
    public async Task<bool> StopApiAsync()
    {
        try
        {
            if (_apiProcess != null && !_apiProcess.HasExited)
            {
                _apiProcess.Kill();
                await _apiProcess.WaitForExitAsync();
            }

            _isRunning = false;
            System.Diagnostics.Debug.WriteLine("API stopped");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping API: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Aktualisiert die Turnierdaten in der laufenden API (Placeholder)
    /// </summary>
    public async Task<bool> UpdateTournamentDataAsync(TournamentData tournamentData)
    {
        try
        {
            // TODO: Implement API call to update tournament data
            await Task.CompletedTask;
            return IsApiRunning;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating tournament data: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Öffnet die API-Dokumentation im Browser
    /// </summary>
    public void OpenApiDocumentation()
    {
        if (IsApiRunning && ApiUrl != null)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ApiUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening API documentation: {ex.Message}");
            }
        }
    }
}