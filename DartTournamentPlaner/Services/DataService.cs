using System.IO;
using Newtonsoft.Json;
using DartTournamentPlaner.Models;

namespace DartTournamentPlaner.Services;

public class DataService
{
    private readonly string _dataPath = "tournament_data.json";

    public async Task<TournamentData> LoadTournamentDataAsync()
    {
        try
        {
            if (File.Exists(_dataPath))
            {
                var json = await File.ReadAllTextAsync(_dataPath);
                var data = JsonConvert.DeserializeObject<TournamentData>(json);
                return data ?? new TournamentData();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading tournament data: {ex.Message}");
        }
        
        return new TournamentData();
    }

    public async Task SaveTournamentDataAsync(TournamentData data)
    {
        try
        {
            data.LastModified = DateTime.Now;
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            await File.WriteAllTextAsync(_dataPath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving tournament data: {ex.Message}");
            throw;
        }
    }

    public bool BackupData(string backupPath)
    {
        try
        {
            if (File.Exists(_dataPath))
            {
                File.Copy(_dataPath, backupPath, true);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating backup: {ex.Message}");
        }
        return false;
    }
} 