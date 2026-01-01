using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DartTournamentPlaner.Models.HubSync;

namespace DartTournamentPlaner.Services.HubSync;

/// <summary>
/// Puffer und Persistenz für eingehende Hub-Turnier-Sync-Nachrichten.
/// </summary>
public class HubTournamentSyncStorage
{
    private readonly string _storagePath;
    private readonly List<HubTournamentSyncPayload> _messages = new();
    private readonly object _lock = new();

    public event Action<HubTournamentSyncPayload>? MessageAdded;

    public HubTournamentSyncStorage(string? storagePath = null)
    {
        _storagePath = storagePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hub_tournament_sync_cache.json");
        LoadFromDisk();
    }

    public IReadOnlyList<HubTournamentSyncPayload> GetMessages()
    {
        lock (_lock)
        {
            return _messages.ToList();
        }
    }

    public void AddMessage(HubTournamentSyncPayload payload)
    {
        lock (_lock)
        {
            _messages.Insert(0, payload);
            SaveToDisk();
        }

        MessageAdded?.Invoke(payload);
    }

    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_storagePath)) return;

            var json = File.ReadAllText(_storagePath);
            var stored = JsonSerializer.Deserialize<List<HubTournamentSyncPayload>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (stored != null)
            {
                _messages.Clear();
                _messages.AddRange(stored);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ [HubTournamentSyncStorage] Load error: {ex.Message}");
        }
    }

    private void SaveToDisk()
    {
        try
        {
            var json = JsonSerializer.Serialize(_messages, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_storagePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ [HubTournamentSyncStorage] Save error: {ex.Message}");
        }
    }
}
