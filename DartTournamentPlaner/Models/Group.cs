using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

public class Group : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _id;
    private bool _matchesGenerated = false;

    public Group()
    {
        // Default constructor for serialization
        Players = new ObservableCollection<Player>();
        Matches = new ObservableCollection<Match>();
    }

    public Group(int id, string name) : this()
    {
        _id = id;
        _name = name;
    }

    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public bool MatchesGenerated
    {
        get => _matchesGenerated;
        set
        {
            _matchesGenerated = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
    public ObservableCollection<Match> Matches { get; set; } = new ObservableCollection<Match>();
    public void ResetMatches()
    {
        Matches.Clear();
        MatchesGenerated = false;
        OnPropertyChanged(nameof(Matches));
        OnPropertyChanged(nameof(MatchesGenerated));
    }

    /// <summary>
    /// ? NEU: Setzt nur die Match-Ergebnisse zurück, behält aber die Matches selbst
    /// Verwendet für den "Reset Matches" Button im Setup-Tab
    /// </summary>
    public void ResetMatchResults()
    {
        System.Diagnostics.Debug.WriteLine($"ResetMatchResults: Resetting results for {Matches.Count} matches in group '{Name}'");

        foreach (var match in Matches)
        {
            // Überspringe Bye-Matches
            if (match.IsBye)
            {
                System.Diagnostics.Debug.WriteLine($"  Skipping bye match {match.Id}");
                continue;
            }

            // Setze alle Ergebnisse zurück
            match.Player1Sets = 0;
            match.Player2Sets = 0;
            match.Player1Legs = 0;
            match.Player2Legs = 0;
            match.Winner = null;
            match.Status = MatchStatus.NotStarted;
            match.Notes = null;
            match.FinishedAt = null;
            match.EndTime = null;

            // Force PropertyChanged für UI-Update
            match.ForcePropertyChanged(nameof(match.Status));
            match.ForcePropertyChanged(nameof(match.Winner));
            match.ForcePropertyChanged(nameof(match.ScoreDisplay));
            match.ForcePropertyChanged(nameof(match.StatusDisplay));
            match.ForcePropertyChanged(nameof(match.WinnerDisplay));

            System.Diagnostics.Debug.WriteLine($"  Reset match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}");
        }

        // WICHTIG: MatchesGenerated bleibt true, da Matches noch existieren
        // Benachrichtige UI über Änderungen
        OnPropertyChanged(nameof(Matches));

        System.Diagnostics.Debug.WriteLine($"ResetMatchResults: Completed for group '{Name}' - {Matches.Count} matches reset");
    }

    public void GenerateRoundRobinMatches(GameRules? gameRules = null)
    {
        Matches.Clear();
        var playerList = Players.ToList();
        int matchId = 1;

        // Bestimme ob Sets verwendet werden sollen
        bool usesSets = gameRules?.PlayWithSets ?? false;
        //System.Diagnostics.Debug.WriteLine($"GenerateRoundRobinMatches: usesSets = {usesSets} (from GameRules.PlayWithSets)");

        // Generate all possible combinations (round robin)
        // Bei Round Robin spielt jeder gegen jeden - KEINE Freilose nötig!
        for (int i = 0; i < playerList.Count; i++)
        {
            for (int j = i + 1; j < playerList.Count; j++)
            {
                var match = new Match
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im Match-Konstruktor erstellt
                    Player1 = playerList[i],
                    Player2 = playerList[j],
                    Status = MatchStatus.NotStarted,
                    UsesSets = usesSets, // WICHTIG: Setze UsesSets basierend auf GameRules
                    CreatedAt = DateTime.Now // Setze Erstellungszeit
                };

                // Stelle sicher, dass UUID gültig ist
                match.EnsureUniqueId();

                //System.Diagnostics.Debug.WriteLine($"  Created match {match.Id}: {match.Player1.Name} vs {match.Player2.Name}, UsesSets = {match.UsesSets}, UUID = {match.UniqueId}");
                Matches.Add(match);
            }
        }

        // ENTFERNT: Freilos-Logik ist bei Round Robin nicht nötig!
        // Bei Round Robin spielt jeder gegen jeden, daher sind keine Freilose erforderlich.

        MatchesGenerated = true;
        //System.Diagnostics.Debug.WriteLine($"GenerateRoundRobinMatches: Generated {Matches.Count} matches with UsesSets = {usesSets}, all with UUIDs");

        // WICHTIG: Benachrichtige über Änderungen in der Matches-Collection
        // Dies ist wichtig für die UI-Aktualisierung
        OnPropertyChanged(nameof(Matches));
    }

    public void GenerateMatches()
    {
        if (Players.Count < 2)
        {
            throw new InvalidOperationException("Mindestens 2 Spieler erforderlich für Match-Generierung");
        }

        Matches.Clear();
        int matchId = 1;

        //System.Diagnostics.Debug.WriteLine($"GenerateMatches: Starting match generation for group '{Name}' with {Players.Count} players");

        // Round-Robin: Jeder gegen jeden
        for (int i = 0; i < Players.Count; i++)
        {
            for (int j = i + 1; j < Players.Count; j++)
            {
                var match = new Match
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im Match-Konstruktor erstellt
                    Player1 = Players[i],
                    Player2 = Players[j],
                    Status = MatchStatus.NotStarted,
                    CreatedAt = DateTime.Now
                };

                // Stelle sicher, dass die UUID gültig ist
                match.EnsureUniqueId();

                //System.Diagnostics.Debug.WriteLine($"  Created match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name} (UUID: {match.UniqueId})");
                Matches.Add(match);
            }
        }

        // Bei ungerader Spielerzahl: Bye-Matches erstellen
        if (Players.Count % 2 == 1)
        {
            //System.Diagnostics.Debug.WriteLine($"GenerateMatches: Creating bye matches for odd number of players ({Players.Count})");

            foreach (var player in Players)
            {
                var byeMatch = new Match
                {
                    Id = matchId++,
                    // WICHTIG: UUID wird automatisch im Match-Konstruktor erstellt
                    Player1 = player,
                    Player2 = null, // BYE
                    Status = MatchStatus.Bye,
                    Winner = player,
                    Player1Sets = 1,
                    Player2Sets = 0,
                    Player1Legs = 1,
                    Player2Legs = 0,
                    CreatedAt = DateTime.Now,
                    FinishedAt = DateTime.Now,
                    Notes = "Automatisches Freilos"
                };

                // Stelle sicher, dass die UUID gültig ist
                byeMatch.EnsureUniqueId();

                //System.Diagnostics.Debug.WriteLine($"  Created bye match {byeMatch.Id}: {byeMatch.Player1?.Name} (bye) (UUID: {byeMatch.UniqueId})");
                Matches.Add(byeMatch);
            }
        }

        //System.Diagnostics.Debug.WriteLine($"? Generated {Matches.Count} matches for group '{Name}' - all with valid UUIDs");

        // Log alle Match-UUIDs für Debugging
        //foreach (var match in Matches)
        //{
        //    System.Diagnostics.Debug.WriteLine($"   Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name ?? "BYE"} (UUID: {match.UniqueId})");
        //}
    }

    public void UpdateMatchDisplaySettings(GameRules gameRules)
    {
        bool usesSets = gameRules.PlayWithSets;
        //System.Diagnostics.Debug.WriteLine($"UpdateMatchDisplaySettings: Updating {Matches.Count} matches to UsesSets = {usesSets}");

        foreach (var match in Matches)
        {
            match.UsesSets = usesSets;
        }

        // WICHTIG: Benachrichtige über Änderungen, damit die UI aktualisiert wird
        OnPropertyChanged(nameof(Matches));
    }

    /// <summary>
    /// Stellt sicher, dass alle Match PropertyChanged Events korrekt abonniert sind
    /// Dies ist wichtig nach dem Laden von JSON-Daten, da Events nicht serialisiert werden
    /// </summary>
    public void EnsureMatchEventSubscriptions()
    {
        //System.Diagnostics.Debug.WriteLine($"EnsureMatchEventSubscriptions: Ensuring events for {Matches.Count} matches in group {Name}");

        foreach (var match in Matches)
        {
            // Force PropertyChanged notification for all display properties
            // This ensures the UI gets updated even if events weren't properly subscribed
            match.ForcePropertyChanged(nameof(match.ScoreDisplay));
            match.ForcePropertyChanged(nameof(match.StatusDisplay));
            match.ForcePropertyChanged(nameof(match.WinnerDisplay));
        }

        // Also trigger collection changed to refresh any bound UI elements
        OnPropertyChanged(nameof(Matches));
        //System.Diagnostics.Debug.WriteLine($"EnsureMatchEventSubscriptions: Completed for group {Name}");
    }

    /// <summary>
    /// NEUE METHODE: Überprüft und meldet den Abschlussstatus der Gruppe
    /// </summary>
    public GroupCompletionStatus CheckCompletionStatus()
    {
        //System.Diagnostics.Debug.WriteLine($"=== CheckCompletionStatus for group {Name} START ===");

        if (!MatchesGenerated)
        {
            //System.Diagnostics.Debug.WriteLine($"  Group {Name}: No matches generated");
            return new GroupCompletionStatus(false, "Keine Spiele generiert", 0, 0);
        }

        if (Matches.Count == 0)
        {
            //System.Diagnostics.Debug.WriteLine($"  Group {Name}: No matches found");
            return new GroupCompletionStatus(false, "Keine Spiele vorhanden", 0, 0);
        }

        var totalMatches = Matches.Count;
        var finishedMatches = Matches.Count(m => m.Status == MatchStatus.Finished || m.IsBye);
        var inProgressMatches = Matches.Count(m => m.Status == MatchStatus.InProgress);
        var notStartedMatches = Matches.Count(m => m.Status == MatchStatus.NotStarted);

        //System.Diagnostics.Debug.WriteLine($"  Group {Name}: {finishedMatches}/{totalMatches} finished");
        //System.Diagnostics.Debug.WriteLine($"    - InProgress: {inProgressMatches}");
        //System.Diagnostics.Debug.WriteLine($"    - NotStarted: {notStartedMatches}");

        //// Detaillierte Aufschlüsselung für Debugging
        //foreach (var match in Matches)
        //{
        //    System.Diagnostics.Debug.WriteLine($"    Match {match.Id}: {match.Player1?.Name ?? "null"} vs {match.Player2?.Name ?? "null"} - Status: {match.Status}");
        //}

        bool isComplete = finishedMatches == totalMatches;
        string message = isComplete ? "Alle Spiele beendet" : $"{notStartedMatches + inProgressMatches} Spiele offen";

        //System.Diagnostics.Debug.WriteLine($"  Group {Name}: Complete = {isComplete}");
        //System.Diagnostics.Debug.WriteLine($"=== CheckCompletionStatus for group {Name} END ===");

        return new GroupCompletionStatus(isComplete, message, finishedMatches, totalMatches);
    }

    /// <summary>
    /// NEUE METHODE: Versucht Match-Status-Probleme automatisch zu reparieren
    /// </summary>
    public void RepairMatchStatuses()
    {
        //System.Diagnostics.Debug.WriteLine($"=== RepairMatchStatuses for group {Name} START ===");

        int repairedCount = 0;

        foreach (var match in Matches)
        {
            var originalStatus = match.Status;
            bool wasRepaired = false;

            // Fall 1: Match hat einen Winner aber Status ist nicht Finished
            if (match.Winner != null && match.Status != MatchStatus.Finished && match.Status != MatchStatus.Bye)
            {
                //System.Diagnostics.Debug.WriteLine($"  Repairing match {match.Id}: Has winner but status is {originalStatus}");
                match.Status = MatchStatus.Finished;
                wasRepaired = true;
            }

            // Fall 2: Match ist als Bye markiert aber hat keinen Winner
            if (match.IsBye && match.Status == MatchStatus.Bye && match.Winner == null)
            {
                if (match.Player1 != null && match.Player2 == null)
                {
                    //System.Diagnostics.Debug.WriteLine($"  Repairing bye match {match.Id}: Setting Player1 as winner");
                    match.Winner = match.Player1;
                    wasRepaired = true;
                }
                else if (match.Player1 == null && match.Player2 != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"  Repairing bye match {match.Id}: Setting Player2 as winner");
                    match.Winner = match.Player2;
                    wasRepaired = true;
                }
            }

            // Fall 3: Match hat Sets/Legs Daten aber keinen Winner
            if (match.Winner == null && match.Status != MatchStatus.Bye &&
                ((match.Player1Sets > 0 || match.Player2Sets > 0) || (match.Player1Legs > 0 || match.Player2Legs > 0)))
            {
                //System.Diagnostics.Debug.WriteLine($"  Repairing match {match.Id}: Has score data but no winner");

                // Bestimme Winner basierend auf Sets oder Legs
                if (match.UsesSets && (match.Player1Sets > 0 || match.Player2Sets > 0))
                {
                    if (match.Player1Sets > match.Player2Sets)
                    {
                        match.Winner = match.Player1;
                        match.Status = MatchStatus.Finished;
                        wasRepaired = true;
                    }
                    else if (match.Player2Sets > match.Player1Sets)
                    {
                        match.Winner = match.Player2;
                        match.Status = MatchStatus.Finished;
                        wasRepaired = true;
                    }
                }
                else if (!match.UsesSets && (match.Player1Legs > 0 || match.Player2Legs > 0))
                {
                    if (match.Player1Legs > match.Player2Legs)
                    {
                        match.Winner = match.Player1;
                        match.Status = MatchStatus.Finished;
                        wasRepaired = true;
                    }
                    else if (match.Player2Legs > match.Player1Legs)
                    {
                        match.Winner = match.Player2;
                        match.Status = MatchStatus.Finished;
                        wasRepaired = true;
                    }
                }
            }

            if (wasRepaired)
            {
                repairedCount++;
                //System.Diagnostics.Debug.WriteLine($"  Repaired match {match.Id}: {originalStatus} -> {match.Status}, Winner: {match.Winner?.Name ?? "none"}");

                // Force PropertyChanged notification
                match.ForcePropertyChanged(nameof(match.Status));
                match.ForcePropertyChanged(nameof(match.Winner));
                match.ForcePropertyChanged(nameof(match.StatusDisplay));
                match.ForcePropertyChanged(nameof(match.WinnerDisplay));
            }
        }

        if (repairedCount > 0)
        {
            // Trigger collection changed to refresh UI
            OnPropertyChanged(nameof(Matches));
        }

        //System.Diagnostics.Debug.WriteLine($"  Repaired {repairedCount} matches in group {Name}");
        //System.Diagnostics.Debug.WriteLine($"=== RepairMatchStatuses for group {Name} END ===");
    }

    public List<PlayerStanding> GetStandings()
    {
        var standings = new List<PlayerStanding>();

        // Create standings for each player
        foreach (var player in Players)
        {
            var standing = new PlayerStanding { Player = player };

            // Get all matches involving this player - Use ID comparison instead of object reference
            var playerMatches = Matches.Where(m =>
                (m.Player1?.Id == player.Id) || (m.Player2?.Id == player.Id)).ToList();

            // Calculate statistics
            var finishedMatches = playerMatches.Where(m => m.Status == MatchStatus.Finished).ToList();
            var finishedNonByeMatches = finishedMatches.Where(m => !m.IsBye).ToList();

            // Matches played (excluding byes)
            standing.MatchesPlayed = finishedNonByeMatches.Count;

            // Wins (including byes but only finished matches) - Use ID comparison
            standing.Wins = finishedMatches.Count(m => m.Winner?.Id == player.Id);

            // Losses (only non-bye finished matches where player didn't win)
            standing.Losses = finishedNonByeMatches.Count(m => m.Winner?.Id != player.Id && m.Winner != null);

            // Draws (non-bye finished matches with no winner)
            standing.Draws = finishedNonByeMatches.Count(m => m.Winner == null);

            // Calculate sets and legs for finished matches
            standing.SetsWon = 0;
            standing.SetsLost = 0;
            standing.LegsWon = 0;
            standing.LegsLost = 0;

            foreach (var match in finishedMatches)
            {
                if (match.Player1?.Id == player.Id)
                {
                    // Player is Player1
                    standing.SetsWon += match.Player1Sets;
                    standing.SetsLost += match.Player2Sets;
                    standing.LegsWon += match.Player1Legs;
                    standing.LegsLost += match.Player2Legs;
                }
                else if (match.Player2?.Id == player.Id)
                {
                    // Player is Player2
                    standing.SetsWon += match.Player2Sets;
                    standing.SetsLost += match.Player1Sets;
                    standing.LegsWon += match.Player2Legs;
                    standing.LegsLost += match.Player1Legs;
                }
            }

            // Calculate points (3 for win, 1 for draw, 0 for loss)
            standing.Points = standing.Wins * 3 + standing.Draws * 1;

            standings.Add(standing);
        }

        // Sort by points (descending), then by set difference (descending), then by leg difference (descending)
        var sortedStandings = standings
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.SetDifference)
            .ThenByDescending(s => s.LegDifference)
            .ThenBy(s => s.Player?.Name ?? "")  // Tie-breaker by name
            .ToList();

        // Assign positions
        for (int i = 0; i < sortedStandings.Count; i++)
        {
            sortedStandings[i].Position = i + 1;
        }

        return sortedStandings;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        var localizationService = Match.LocalizationService;

        if (localizationService != null)
        {
            var playersText = localizationService.GetString("PlayersPlural") ?? "Spieler";
            var matchesText = localizationService.GetString("MatchesPlural") ?? "Spiele";
            return $"{Name} ({Players.Count} {playersText}, {Matches.Count} {matchesText})";
        }

        // Fallback zu deutschen Texten wenn LocalizationService nicht verfügbar
        return $"{Name} ({Players.Count} Spieler, {Matches.Count} Spiele)";
    }

    /// <summary>
    /// Status-Klasse für Gruppenabschluss-Überprüfung
    /// </summary>
    public class GroupCompletionStatus
    {
        public bool IsComplete { get; }
        public string StatusMessage { get; }
        public int FinishedMatches { get; }
        public int TotalMatches { get; }
        public double CompletionPercentage => TotalMatches > 0 ? (double)FinishedMatches / TotalMatches * 100 : 0;

        public GroupCompletionStatus(bool isComplete, string statusMessage, int finishedMatches, int totalMatches)
        {
            IsComplete = isComplete;
            StatusMessage = statusMessage;
            FinishedMatches = finishedMatches;
            TotalMatches = totalMatches;
        }

        public override string ToString()
        {
            return $"{StatusMessage} ({FinishedMatches}/{TotalMatches} - {CompletionPercentage:F1}%)";
        }
    }
}