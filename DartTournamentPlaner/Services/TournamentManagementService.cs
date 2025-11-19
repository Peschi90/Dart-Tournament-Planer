using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using DartTournamentPlaner.Models;
using DartTournamentPlaner.Views;
using DartTournamentPlaner.Services.License;

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
    private readonly TournamentData _tournamentData;  // ⭐ NEU: Persistente TournamentData Instanz

    public event Action? DataChanged;

    public TournamentManagementService(
        LocalizationService localizationService,
        DataService dataService)
    {
        _localizationService = localizationService;
        _dataService = dataService;
        _tournamentClasses = new List<TournamentClass>();
  _tournamentData = new TournamentData();  // ⭐ NEU: Initialisiere einmal
    
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
                
         // ⭐ NEU: Restore Tournament-ID from loaded data
       _tournamentData.TournamentId = data.TournamentId;
      _tournamentData.TournamentName = data.TournamentName;
           
       System.Diagnostics.Debug.WriteLine($"✅ [LoadData] Tournament ID restored: {_tournamentData.TournamentId ?? "null"}");
            
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
     // ⭐ FIXED: Verwende persistente TournamentData für Speichern
      _tournamentData.TournamentClasses = _tournamentClasses;
   
            System.Diagnostics.Debug.WriteLine($"✅ [SaveData] Saving with Tournament ID: {_tournamentData.TournamentId ?? "null"}");

            await _dataService.SaveTournamentDataAsync(_tournamentData);
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
        // ⭐ FIXED: Gebe persistente Instanz zurück statt neue zu erstellen
      _tournamentData.TournamentClasses = _tournamentClasses;
     return _tournamentData;
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
    
    /// <summary>
    /// ✅ NEU: Erstellt ein neues Turnier aus PowerScoring TournamentClasses
    /// PHASE 3 - PowerScoring to Tournament Integration
    /// 
    /// VERANTWORTLICHKEIT:
    /// - Speichert aktuelles Turnier (falls vorhanden)
    /// - Ersetzt TournamentClasses mit PowerScoring-Daten
    /// - Triggert UI-Refresh
    /// 
    /// NICHT VERANTWORTLICH FÜR:
    /// - Konvertierung von PowerScoring-Daten (-> PowerScoringToTournamentService)
    /// - UI-Navigation (-> Caller)
    /// </summary>
    /// <param name="powerScoringClasses">TournamentClasses aus PowerScoring Distribution</param>
    /// <returns>true wenn erfolgreich, false bei Fehler</returns>
    public async Task<bool> CreateTournamentFromPowerScoringAsync(List<TournamentClass> powerScoringClasses)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("🏗️ Creating tournament from PowerScoring classes...");
            System.Diagnostics.Debug.WriteLine($"   Received {powerScoringClasses.Count} classes");
            
            // 1. Speichere aktuelles Turnier falls vorhanden
            if (HasActiveTournament())
            {
                System.Diagnostics.Debug.WriteLine("   💾 Saving existing tournament...");
                try
                {
                    await _dataService.SaveTournamentDataAsync(_tournamentData);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"   ❌ Failed to save existing tournament: {ex.Message}");
                    return false;
                }
            }
            
            // 2. Unsubscribe von alten Tournaments
            foreach (var tc in _tournamentClasses)
            {
                UnsubscribeFromChanges(tc);
            }
            
            // 3. Lösche alte Klassen
            _tournamentClasses.Clear();
            
            // 4. Füge PowerScoring-Klassen hinzu
            // Mappe PowerScoring-Klassen auf bekannte IDs (Platin=1, Gold=2, etc.)
            var classNameToId = new Dictionary<string, int>
            {
                { "Platin", 1 },
                { "Gold", 2 },
                { "Silber", 3 },
                { "Bronze", 4 },
                { "Eisen", 5 }
            };
            
            foreach (var psClass in powerScoringClasses)
            {
                // Setze ID basierend auf Namen
                if (classNameToId.TryGetValue(psClass.Name, out var classId))
                {
                    psClass.Id = classId;
                }
                
                _tournamentClasses.Add(psClass);
                SubscribeToChanges(psClass);
                
                System.Diagnostics.Debug.WriteLine($"   ✅ Added class '{psClass.Name}' with {psClass.Groups.Count} groups");
            }
            
            // 5. Sortiere Klassen nach ID
            _tournamentClasses.Sort((a, b) => a.Id.CompareTo(b.Id));
            
            // 6. Trigger UI Refresh
            TriggerUIRefresh();
            DataChanged?.Invoke();
            
            System.Diagnostics.Debug.WriteLine($"✅ Tournament created successfully with {_tournamentClasses.Count} classes");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error creating tournament from PowerScoring: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// ✅ Prüft ob ein aktives Turnier vorhanden ist
    /// </summary>
    public bool HasActiveTournament()
    {
        return _tournamentClasses.Any(tc => 
            tc.Groups?.Any(g => g.Players?.Any() == true) == true);
    }
}