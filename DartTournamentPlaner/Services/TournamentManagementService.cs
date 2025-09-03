using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Services;

/// <summary>
/// Service für Tournament-spezifische Operationen und Datenmanagement
/// Verwaltet die vier Tournament-Klassen und deren Operationen
/// </summary>
public class TournamentManagementService
{
    private readonly LocalizationService _localizationService;
    private readonly DataService _dataService;
    private readonly List<TournamentClass> _tournamentClasses;
    private readonly HashSet<TournamentClass> _subscribedTournaments = new();

    public event Action? DataChanged;

    public TournamentManagementService(
        LocalizationService localizationService,
        DataService dataService)
    {
        _localizationService = localizationService;
        _dataService = dataService;
        _tournamentClasses = new List<TournamentClass>();
        
        InitializeTournamentClasses();
    }

    public TournamentClass? PlatinClass => _tournamentClasses.FirstOrDefault(tc => tc.Id == 1);
    public TournamentClass? GoldClass => _tournamentClasses.FirstOrDefault(tc => tc.Id == 2);
    public TournamentClass? SilberClass => _tournamentClasses.FirstOrDefault(tc => tc.Id == 3);
    public TournamentClass? BronzeClass => _tournamentClasses.FirstOrDefault(tc => tc.Id == 4);

    public List<TournamentClass> AllTournamentClasses => _tournamentClasses;

    private void InitializeTournamentClasses()
    {
        var classes = new[]
        {
            new TournamentClass { Id = 1, Name = "Platin" },
            new TournamentClass { Id = 2, Name = "Gold" },
            new TournamentClass { Id = 3, Name = "Silber" },
            new TournamentClass { Id = 4, Name = "Bronze" }
        };

        foreach (var tournamentClass in classes)
        {
            tournamentClass.EnsureGroupPhaseExists();
            SubscribeToChanges(tournamentClass);
            _tournamentClasses.Add(tournamentClass);
        }
    }

    private void SubscribeToChanges(TournamentClass tournamentClass)
    {
        if (_subscribedTournaments.Contains(tournamentClass))
        {
            return;
        }
        
        try
        {
            tournamentClass.Groups.CollectionChanged += (s, e) => DataChanged?.Invoke();
            tournamentClass.DataChangedEvent += (s, e) => DataChanged?.Invoke();
            tournamentClass.GameRules.PropertyChanged += (s, e) => DataChanged?.Invoke();
            
            _subscribedTournaments.Add(tournamentClass);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SubscribeToChanges ERROR for {tournamentClass.Name}: {ex.Message}");
        }
    }

    private void UnsubscribeFromChanges(TournamentClass? tournamentClass)
    {
        if (tournamentClass != null && _subscribedTournaments.Contains(tournamentClass))
        {
            _subscribedTournaments.Remove(tournamentClass);
        }
    }

    public TournamentClass? GetTournamentClassById(int classId)
    {
        return _tournamentClasses.FirstOrDefault(tc => tc.Id == classId);
    }

    public async Task<bool> LoadDataAsync()
    {
        try
        {
            var data = await _dataService.LoadTournamentDataAsync();
            if (data.TournamentClasses.Count >= 4)
            {
                // Unsubscribe von alten Klassen
                foreach (var tc in _tournamentClasses)
                {
                    UnsubscribeFromChanges(tc);
                }
                
                // Aktualisiere Tournament Classes
                for (int i = 0; i < 4 && i < data.TournamentClasses.Count; i++)
                {
                    var loadedClass = data.TournamentClasses[i];
                    loadedClass.Id = i + 1;
                    loadedClass.Name = new[] { "Platin", "Gold", "Silber", "Bronze" }[i];
                    
                    _tournamentClasses[i] = loadedClass;
                    SubscribeToChanges(_tournamentClasses[i]);
                }
                
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadData ERROR: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SaveDataAsync()
    {
        try
        {
            var data = new TournamentData
            {
                TournamentClasses = _tournamentClasses
            };

            await _dataService.SaveTournamentDataAsync(data);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SaveData ERROR: {ex.Message}");
            
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("ErrorSavingData")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            
            return false;
        }
    }

    public TournamentData GetTournamentData()
    {
        return new TournamentData
        {
            TournamentClasses = _tournamentClasses
        };
    }

    public void ResetAllTournaments()
    {
        foreach (var tc in _tournamentClasses)
        {
            UnsubscribeFromChanges(tc);
        }
        
        _tournamentClasses.Clear();
        InitializeTournamentClasses();
    }

    public int GetActiveMatchesCount()
    {
        return _tournamentClasses.Sum(tc => 
            tc.Groups?.Sum(g => g.Matches?.Count(m => m.Status == MatchStatus.InProgress) ?? 0) ?? 0);
    }

    public int GetTotalPlayersCount()
    {
        return _tournamentClasses.Sum(tc => 
            tc.Groups?.Sum(g => g.Players?.Count ?? 0) ?? 0);
    }

    public void TriggerUIRefresh()
    {
        foreach (var tc in _tournamentClasses)
        {
            tc.TriggerUIRefresh();
        }
    }

    public void ShowTournamentOverview()
    {
        try
        {
            // ? ERWEITERT: Hole HubIntegrationService für QR-Code Integration
            HubIntegrationService? hubService = null;
            try
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    var hubServiceField = mainWindow.GetType()
                        .GetField("_hubService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    hubService = hubServiceField?.GetValue(mainWindow) as HubIntegrationService;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not get HubService for TournamentOverview: {ex.Message}");
            }
            
            var overviewWindow = new TournamentOverviewWindow(_tournamentClasses, _localizationService, hubService);
            overviewWindow.Show();
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error");
            var message = $"{_localizationService.GetString("ErrorOpeningOverview")} {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void ShowPrintDialog(Window? owner = null, 
        Services.License.LicenseFeatureService? licenseFeatureService = null, 
        Services.License.LicenseManager? licenseManager = null)
    {
        try
        {
            Helpers.PrintHelper.ShowPrintDialog(
                _tournamentClasses, 
                PlatinClass, 
                owner, 
                _localizationService,
                licenseFeatureService,  // Lizenzprüfung
                licenseManager         // Für Lizenz-Dialog
            );
        }
        catch (Exception ex)
        {
            var title = _localizationService.GetString("Error") ?? "Fehler";
            var message = $"Fehler beim Öffnen des Druckdialogs: {ex.Message}";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}