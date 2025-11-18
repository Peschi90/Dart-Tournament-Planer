using System.Collections.ObjectModel;
using DartTournamentPlaner.Models.PowerScore;

namespace DartTournamentPlaner.Services.PowerScore;

/// <summary>
/// Service für PowerScoring-Funktionalität mit Hub-Integration und Persistierung
/// Verwaltet Sessions, Spieler-Scores, WebSocket-Updates und Gruppeneinteilung
/// </summary>
public class PowerScoringService
{
    private readonly PowerScoringPersistenceService _persistenceService;
    
    /// <summary>
    /// Aktuelle PowerScoring-Session
    /// </summary>
    public PowerScoringSession? CurrentSession { get; private set; }

    /// <summary>
    /// Event wird gefeuert wenn ein Player-Update vom Hub empfangen wurde
    /// </summary>
    public event EventHandler<PowerScoringPlayer>? PlayerScoreUpdated;

    public PowerScoringService()
    {
        _persistenceService = new PowerScoringPersistenceService();
    }

    /// <summary>
    /// Erstellt eine neue PowerScoring-Session
    /// </summary>
    /// <param name="rule">Die anzuwendende PowerScoring-Regel</param>
    /// <param name="tournamentId">Optional: Tournament-ID für Hub-Integration</param>
    /// <returns>Die neu erstellte Session</returns>
    public PowerScoringSession CreateNewSession(PowerScoringRule rule, string? tournamentId = null)
    {
        CurrentSession = new PowerScoringSession
        {
            Rule = rule,
            Status = PowerScoringStatus.Setup,
            TournamentId = tournamentId
        };

        System.Diagnostics.Debug.WriteLine($"✨ PowerScoring Session erstellt: {CurrentSession.SessionId}, TournamentId: {tournamentId}");
        
        // Auto-save aktivieren
        _persistenceService.EnableAutoSave(CurrentSession, this);
        
        // Initial speichern
        _persistenceService.SaveSession(CurrentSession);
        
        return CurrentSession;
    }

    /// <summary>
    /// Lädt eine gespeicherte Session
    /// </summary>
    public PowerScoringSession? LoadSession()
    {
        var session = _persistenceService.LoadSession();
        if (session != null)
        {
            CurrentSession = session;
            _persistenceService.EnableAutoSave(CurrentSession, this);
            System.Diagnostics.Debug.WriteLine($"📂 PowerScoring Session geladen: {session.SessionId}");
        }
        return session;
    }

    /// <summary>
    /// Prüft ob eine gespeicherte Session existiert
    /// </summary>
    public bool HasSavedSession()
    {
        return _persistenceService.HasSavedSession();
    }

    /// <summary>
    /// Löscht die gespeicherte Session
    /// </summary>
    public void DeleteSavedSession()
    {
        _persistenceService.DeleteSession();
    }

    /// <summary>
    /// Setzt oder aktualisiert die Tournament-ID der aktuellen Session
    /// </summary>
    public void SetTournamentId(string? tournamentId)
    {
        if (CurrentSession != null)
        {
            CurrentSession.TournamentId = tournamentId;
            _persistenceService.SaveSession(CurrentSession);
            System.Diagnostics.Debug.WriteLine($"🆔 Tournament-ID updated: {tournamentId}");
        }
    }

    /// <summary>
    /// Markiert die Session als mit Hub registriert
    /// </summary>
    public void SetRegisteredWithHub(bool isRegistered)
    {
        if (CurrentSession != null)
        {
            CurrentSession.IsRegisteredWithHub = isRegistered;
            System.Diagnostics.Debug.WriteLine($"🔗 Hub registration status: {isRegistered}");
        }
    }

    /// <summary>
    /// Generiert QR-Code URLs für alle Spieler
    /// </summary>
    /// <param name="hubUrl">Base URL des Tournament Hubs</param>
    public void GenerateQrCodeUrls(string hubUrl)
    {
        if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.TournamentId))
        {
            System.Diagnostics.Debug.WriteLine("⚠️ Cannot generate QR codes: No session or tournament ID");
            return;
        }

        // Entferne trailing slash von hubUrl
        hubUrl = hubUrl.TrimEnd('/');

        var rounds = (int)CurrentSession.Rule;

        foreach (var player in CurrentSession.Players)
        {
            // Format: hubURL/power-scoring.html?tournamentId=xxx&participantId=xxx&playerName=xxx&rounds=xxx
            var qrCodeUrl = $"{hubUrl}/power-scoring.html" +
                $"?tournamentId={Uri.EscapeDataString(CurrentSession.TournamentId)}" +
                $"&participantId={Uri.EscapeDataString(player.PlayerId.ToString())}" +
                $"&playerName={Uri.EscapeDataString(player.Name)}" +
                $"&rounds={rounds}";

            player.QrCodeUrl = qrCodeUrl;
            System.Diagnostics.Debug.WriteLine($"📱 QR Code generated for {player.Name}: {qrCodeUrl}");
        }
    }

    /// <summary>
    /// Fügt einen Spieler zur aktuellen Session hinzu
    /// </summary>
    /// <param name="playerName">Name des Spielers</param>
    /// <returns>Der hinzugefügte Spieler</returns>
    public PowerScoringPlayer? AddPlayerToSession(string playerName)
    {
        if (CurrentSession == null || string.IsNullOrWhiteSpace(playerName))
            return null;

        // Prüfe ob Spieler bereits existiert
        if (CurrentSession.Players.Any(p => p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase)))
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Spieler '{playerName}' existiert bereits");
            return null;
        }

        var player = new PowerScoringPlayer
        {
            Name = playerName.Trim(),
            NumberOfThrows = (int)CurrentSession.Rule
        };

        CurrentSession.Players.Add(player);
        _persistenceService.SaveSession(CurrentSession);
        System.Diagnostics.Debug.WriteLine($"✅ Spieler hinzugefügt: {playerName} (ID: {player.PlayerId})");
        return player;
    }

    /// <summary>
    /// Entfernt einen Spieler aus der aktuellen Session
    /// </summary>
    public bool RemovePlayerFromSession(PowerScoringPlayer player)
    {
        if (CurrentSession == null || player == null)
            return false;

        var removed = CurrentSession.Players.Remove(player);
        if (removed)
        {
            _persistenceService.SaveSession(CurrentSession);
            System.Diagnostics.Debug.WriteLine($"🗑️ Spieler entfernt: {player.Name}");
        }
        return removed;
    }

    /// <summary>
    /// Startet die Scoring-Phase
    /// </summary>
    public bool StartScoring()
    {
        if (CurrentSession == null || CurrentSession.Players.Count == 0)
            return false;

        CurrentSession.Status = PowerScoringStatus.Scoring;
        _persistenceService.SaveSession(CurrentSession);
        System.Diagnostics.Debug.WriteLine($"🎯 Scoring gestartet für {CurrentSession.Players.Count} Spieler");
        return true;
    }

    /// <summary>
    /// Verarbeitet ein PowerScoring-Result Update vom Hub mit erweiterten Daten
    /// </summary>
    public bool ProcessPowerScoringResult(PowerScoringHubMessage message)
    {
        if (CurrentSession == null || message == null)
            return false;

        // Finde Spieler anhand ParticipantId
        var player = CurrentSession.Players.FirstOrDefault(p => 
            p.PlayerId.ToString().Equals(message.ParticipantId, StringComparison.OrdinalIgnoreCase));

        if (player == null)
        {
            System.Diagnostics.Debug.WriteLine($"⚠️ Player not found for ParticipantId: {message.ParticipantId}");
            return false;
        }

        // Aktualisiere Basis-Daten
        player.TotalScore = message.TotalScore;
        player.AverageScore = message.Average;
        player.IsScored = message.Type == "power-scoring-result"; // Final result
        
        // Aktualisiere erweiterte Statistiken
        player.HighestThrow = message.HighestThrow;
        player.TotalDarts = message.TotalDarts;
        player.SessionStartTime = message.SessionStartTime;
        player.CompletionTime = message.CompletionTime;
        player.SubmittedVia = message.SubmittedVia;

        // Aktualisiere detaillierte History mit Dart-Details
        player.History.Clear();
        if (message.ThrowHistory != null && message.ThrowHistory.Count > 0)
        {
            foreach (var round in message.ThrowHistory)
            {
                var historyItem = new Models.PowerScore.PowerScoringRoundHistory
                {
                    Round = round.Round,
                    Total = round.Total,
                    Timestamp = round.Timestamp
                };
                
                // Konvertiere Dart-Details
                if (round.Darts != null && round.Darts.Count >= 3)
                {
                    historyItem.Throw1 = round.Darts[0].Score;
                    historyItem.Throw2 = round.Darts[1].Score;
                    historyItem.Throw3 = round.Darts[2].Score;
                    
                    // Füge detaillierte Dart-Informationen hinzu
                    historyItem.Darts = round.Darts.Select(d => new Models.PowerScore.DartThrowDetail
                    {
                        Number = d.Number,
                        Multiplier = d.Multiplier,
                        Score = d.Score,
                        DisplayValue = d.DisplayValue
                    }).ToList();
                }
                
                player.History.Add(historyItem);
            }
        }

        var duration = message.GetSessionDuration();
        var durationText = duration.HasValue ? $", Duration={duration.Value:F1}s" : "";
        
        System.Diagnostics.Debug.WriteLine($"📊 PowerScoring {message.Type} processed for {player.Name}:");
        System.Diagnostics.Debug.WriteLine($"   Total={player.TotalScore}, Avg={player.AverageScore:F2}, Highest={player.HighestThrow}{durationText}");
        System.Diagnostics.Debug.WriteLine($"   Rounds={player.History.Count}, Darts={player.TotalDarts}");

        // Speichere Session nach Update
        _persistenceService.SaveSession(CurrentSession);

        // Feuere Event für UI-Update
        PlayerScoreUpdated?.Invoke(this, player);

        return true;
    }

    /// <summary>
    /// Schließt die Session ab und berechnet Rankings
    /// </summary>
    public bool CompleteSession()
    {
        if (CurrentSession == null)
            return false;

        CurrentSession.Status = PowerScoringStatus.Completed;
        _persistenceService.SaveSession(CurrentSession);
        System.Diagnostics.Debug.WriteLine($"✅ PowerScoring Session abgeschlossen");
        return true;
    }

    /// <summary>
    /// Gibt die Spieler sortiert nach Score zurück (absteigend)
    /// </summary>
    public List<PowerScoringPlayer> GetRankedPlayers()
    {
        if (CurrentSession == null)
            return new List<PowerScoringPlayer>();

        return CurrentSession.Players
            .Where(p => p.IsScored)
            .OrderByDescending(p => p.AverageScore)
            .ThenByDescending(p => p.TotalScore)
            .ToList();
    }

    /// <summary>
    /// Teilt Spieler basierend auf Score in Gruppen ein
    /// </summary>
    /// <param name="numberOfGroups">Anzahl der Gruppen</param>
    /// <returns>Dictionary mit Gruppennummer und zugewiesenen Spielern</returns>
    public Dictionary<int, List<PowerScoringPlayer>> DistributePlayersToGroups(int numberOfGroups)
    {
        if (CurrentSession == null || numberOfGroups <= 0)
            return new Dictionary<int, List<PowerScoringPlayer>>();

        var rankedPlayers = GetRankedPlayers();
        var groups = new Dictionary<int, List<PowerScoringPlayer>>();

        // Initialisiere Gruppen
        for (int i = 1; i <= numberOfGroups; i++)
        {
            groups[i] = new List<PowerScoringPlayer>();
        }

        // Snake-Draft-Verteilung: 1,2,3,4,4,3,2,1,1,2,3,4...
        int currentGroup = 1;
        bool ascending = true;

        foreach (var player in rankedPlayers)
        {
            groups[currentGroup].Add(player);

            if (ascending)
            {
                currentGroup++;
                if (currentGroup > numberOfGroups)
                {
                    currentGroup = numberOfGroups;
                    ascending = false;
                }
            }
            else
            {
                currentGroup--;
                if (currentGroup < 1)
                {
                    currentGroup = 1;
                    ascending = true;
                }
            }
        }

        System.Diagnostics.Debug.WriteLine($"📋 Spieler auf {numberOfGroups} Gruppen verteilt");
        return groups;
    }

    /// <summary>
    /// Setzt den Service und die aktuelle Session zurück
    /// </summary>
    public void Reset()
    {
        CurrentSession = null;
        System.Diagnostics.Debug.WriteLine($"🔄 PowerScoring Service zurückgesetzt");
    }

    public void ResetSession()
    {
        CurrentSession = null;
        System.Diagnostics.Debug.WriteLine($"🔄 PowerScoring Service zurückgesetzt");
    }
}
