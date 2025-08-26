using DartTournamentPlaner.Models;
using System.Diagnostics;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Erweiterte API-Integration mit HTTP-Client-Unterstützung
/// Diese Version kommuniziert über HTTP mit der separaten API
/// ERWEITERT: Bidirektionale Updates zwischen Tournament Planner und Tournament Hub
/// </summary>
public class HttpApiIntegrationService : IApiIntegrationService
{
    private Process? _apiProcess;
    private readonly HttpClient _httpClient;
    private int _currentPort;
    private bool _isRunning;
    private Timer? _healthCheckTimer;
    private Timer? _hubPollingTimer;

    // NEU: Hub-Integration für eingehende Updates
    private string? _currentTournamentId;
    private readonly Dictionary<int, DateTime> _lastMatchUpdates = new();

    public bool IsApiRunning => _isRunning && (_apiProcess?.HasExited == false);
    public string? ApiUrl => IsApiRunning ? $"http://localhost:{_currentPort}" : null;

    public event EventHandler<MatchResultUpdateEventArgs>? MatchResultUpdated;

    public HttpApiIntegrationService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

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
            var apiProjectPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "DartTournamentPlaner.API");
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --urls http://localhost:{port}",
                WorkingDirectory = apiProjectPath,
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

            // Warte und teste die API-Verfügbarkeit
            var success = await WaitForApiStartup();
            
            if (success)
            {
                _isRunning = true;
                StartHealthCheck();
                
                // Sende initiale Turnierdaten an die API
                await SendTournamentDataToApi(tournamentData);
                
                // NEU: Starte Hub-Update-Polling für bidirektionale Updates
                StartHubUpdatePolling();
                
                System.Diagnostics.Debug.WriteLine($"API successfully started on http://localhost:{port}");
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("API failed to start properly");
                await StopApiAsync();
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting API: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// NEU: Startet Polling für Updates vom Tournament Hub
    /// </summary>
    private void StartHubUpdatePolling()
    {
        _hubPollingTimer = new Timer(async _ => await CheckForHubUpdates(), null, 
                                   TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15));
        System.Diagnostics.Debug.WriteLine("🔄 Started Hub update polling");
    }

    /// <summary>
    /// NEU: Überprüft auf Updates vom Tournament Hub
    /// </summary>
    private async Task CheckForHubUpdates()
    {
        try
        {
            if (string.IsNullOrEmpty(_currentTournamentId) || !_isRunning) return;

            // Frage die API nach aktuellen Match-Daten
            var response = await _httpClient.GetAsync($"http://localhost:{_currentPort}/api/tournaments/{_currentTournamentId}/matches");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                // Hier würde normalerweise eine Analyse der Match-Updates stattfinden
                // und entsprechende Events ausgelöst werden
                
                System.Diagnostics.Debug.WriteLine($"🔄 Hub update check completed for {_currentTournamentId}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Hub update check error: {ex.Message}");
        }
    }

    /// <summary>
    /// NEU: Setzt die aktuelle Tournament-ID für Hub-Updates
    /// </summary>
    public void SetCurrentTournamentId(string tournamentId)
    {
        _currentTournamentId = tournamentId;
        System.Diagnostics.Debug.WriteLine($"🎯 API Service: Set current tournament ID to {tournamentId}");
    }

    public async Task<bool> StopApiAsync()
    {
        try
        {
            _isRunning = false;
            
            // Timer stoppen
            _healthCheckTimer?.Dispose();
            _healthCheckTimer = null;
            
            _hubPollingTimer?.Dispose();
            _hubPollingTimer = null;

            // API-Prozess beenden
            if (_apiProcess != null && !_apiProcess.HasExited)
            {
                _apiProcess.Kill();
                _apiProcess.Dispose();
                _apiProcess = null;
            }

            System.Diagnostics.Debug.WriteLine("API stopped successfully");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping API: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateTournamentDataAsync(TournamentData tournamentData)
    {
        try
        {
            if (!IsApiRunning) return false;

            await SendTournamentDataToApi(tournamentData);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating tournament data: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> WaitForApiStartup()
    {
        const int maxRetries = 30; // 30 seconds
        const int retryDelayMs = 1000; // 1 second

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:{_currentPort}/health");
                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"API is responding after {i + 1} attempts");
                    return true;
                }
            }
            catch
            {
                // API not ready yet
            }

            await Task.Delay(retryDelayMs);
        }

        System.Diagnostics.Debug.WriteLine("API failed to respond within timeout period");
        return false;
    }

    private void StartHealthCheck()
    {
        _healthCheckTimer = new Timer(async _ => await PerformHealthCheck(), null, 
            TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private async Task PerformHealthCheck()
    {
        if (!_isRunning) return;

        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:{_currentPort}/health");
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("API health check failed - marking as stopped");
                _isRunning = false;
            }
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine("API health check failed - marking as stopped");
            _isRunning = false;
        }
    }

    private async Task SendTournamentDataToApi(TournamentData tournamentData)
    {
        try
        {
            // TODO: Implement API call to send tournament data
            // For now, this is a placeholder since the API works with live data
            await Task.CompletedTask;
            System.Diagnostics.Debug.WriteLine("Tournament data sent to API (placeholder)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending tournament data to API: {ex.Message}");
        }
    }

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

    public void Dispose()
    {
        _healthCheckTimer?.Dispose();
        _httpClient?.Dispose();
        _apiProcess?.Dispose();
    }
}