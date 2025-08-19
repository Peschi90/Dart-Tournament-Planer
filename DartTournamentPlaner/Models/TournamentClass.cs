using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using DartTournamentPlaner.Services;
using DartTournamentPlaner.Views;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Repräsentiert eine komplette Turnierklasse (z.B. Platin, Gold, Silber, Bronze)
/// Diese Klasse ist das Herzstück des Turniersystems und verwaltet alle Phasen eines Turniers
/// Implementiert INotifyPropertyChanged für UI-Updates und unterstützt alle Turniermodi
/// </summary>
public class TournamentClass : INotifyPropertyChanged
{
    // Private Backing-Fields für die Eigenschaften
    private int _id;                        // Eindeutige ID der Turnierklasse
    private string _name = "Platin";        // Name der Klasse (z.B. Platin, Gold, etc.)
    private GameRules _gameRules = new GameRules(); // Spielregeln für diese Klasse
    private TournamentPhase? _currentPhase; // Aktuelle Phase des Turniers

    /// <summary>
    /// Eindeutige Identifikations-ID der Turnierklasse
    /// </summary>
    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Name der Turnierklasse (z.B. "Platin", "Gold", "Silber", "Bronze")
    /// Wird in der UI zur Anzeige der Tabs verwendet
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Spielregeln für diese Turnierklasse
    /// Definiert Punkte (301/401/501), Sets/Legs, K.O.-Modi, etc.
    /// </summary>
    public GameRules GameRules
    {
        get => _gameRules;
        set
        {
            _gameRules = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Aktuelle Phase des Turniers (Gruppenphase, Finalrunde, K.O.-Phase)
    /// Bestimmt welche Ansicht in der UI angezeigt wird
    /// </summary>
    public TournamentPhase? CurrentPhase
    {
        get => _currentPhase;
        set
        {
            _currentPhase = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Legacy-Unterstützung: Gruppen aus der aktuellen oder Gruppenphase
    /// Diese Eigenschaft stellt eine einheitliche Schnittstelle für den Zugriff auf Gruppen bereit
    /// und behandelt sowohl direkte Groups (für JSON-Deserialisierung) als auch Phase-basierte Groups
    /// </summary>
    public ObservableCollection<Group> Groups 
    { 
        get 
        {
            System.Diagnostics.Debug.WriteLine($"TournamentClass.Groups getter called for {Name}");
            
            // NEUE STRATEGIE: Stelle sicher dass GroupPhase existiert (nach JSON-Loading)
            EnsureGroupPhaseExists();
            
            // WICHTIG: Erst schauen ob direkt Groups auf TournamentClass-Ebene vorhanden sind (für Legacy/Loading)
            if (_directGroups != null && _directGroups.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"  Using direct groups collection with {_directGroups.Count} groups");
                
                // Einmalige Migration: Kopiere direkte Groups in die aktuelle Phase
                if (CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase && CurrentPhase.Groups.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"  Migrating {_directGroups.Count} groups to CurrentPhase");
                    foreach (var group in _directGroups)
                    {
                        CurrentPhase.Groups.Add(group);
                    }
                    
                    // Nach der Migration directe Groups leeren
                    _directGroups.Clear();
                    System.Diagnostics.Debug.WriteLine($"  Migration completed, cleared direct groups");
                }
                
                return CurrentPhase?.Groups ?? new ObservableCollection<Group>();
            }
            
            // Wenn aktuelle Phase die Gruppenphase ist, gib deren Groups zurück
            if (CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Current phase is GroupPhase, returning {CurrentPhase.Groups.Count} groups");
                return CurrentPhase.Groups;
            }
            
            // Wenn wir in späteren Phasen sind, gib die Groups aus der Gruppenphase zurück
            var groupPhase = Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
            
            if (groupPhase?.Groups != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in GroupPhase, found GroupPhase with {groupPhase.Groups.Count} groups");
                return groupPhase.Groups;
            }
            
            System.Diagnostics.Debug.WriteLine($"  ERROR: No GroupPhase found after EnsureGroupPhaseExists - this should not happen!");
            // Fallback: Notfall-GroupPhase erstellen wenn alle anderen Strategien fehlschlagen
            var emergencyGroupPhase = new TournamentPhase
            {
                Name = "Gruppenphase",
                PhaseType = TournamentPhaseType.GroupPhase,
                IsActive = true
            };
            Phases.Add(emergencyGroupPhase);
            CurrentPhase = emergencyGroupPhase;
            
            return emergencyGroupPhase.Groups;
        }
        set 
        {
            System.Diagnostics.Debug.WriteLine($"TournamentClass.Groups setter called for {Name} with {value?.Count ?? 0} groups");
            
            // Für JSON-Deserialisierung: Speichere Groups temporär in direkter Collection
            _directGroups = value ?? new ObservableCollection<Group>();
            
            // Wenn bereits eine CurrentPhase existiert, kopiere sofort
            if (CurrentPhase?.PhaseType == TournamentPhaseType.GroupPhase && CurrentPhase.Groups.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"  Immediately copying {_directGroups.Count} groups to CurrentPhase");
                CurrentPhase.Groups.Clear();
                foreach (var group in _directGroups)
                {
                    CurrentPhase.Groups.Add(group);
                }
                _directGroups.Clear();
            }
            
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }
    
    // Temporärer Storage für Groups beim JSON-Loading
    // Wird während der Deserialisierung verwendet um Groups zu speichern bis die Phasen geladen sind
    private ObservableCollection<Group>? _directGroups;

    /// <summary>
    /// Alle Turnierphasen dieser Klasse
    /// Enthält normalerweise: Gruppenphase, optional Finalrunde, optional K.O.-Phase
    /// </summary>
    public ObservableCollection<TournamentPhase> Phases { get; set; } = new ObservableCollection<TournamentPhase>();

    /// <summary>
    /// Standard-Konstruktor für TournamentClass
    /// Wichtig: Erstellt KEINE automatische GroupPhase um JSON-Deserialisierung nicht zu beeinträchtigen
    /// </summary>
    public TournamentClass()
    {
        System.Diagnostics.Debug.WriteLine($"=== TournamentClass Constructor START ===");
        
        // WICHTIG: KEINE automatische GroupPhase-Erstellung im Constructor!
        // Das würde bei JSON-Deserialisierung zu Duplikaten führen, da:
        // 1. Constructor erstellt GroupPhase (Phases ist noch leer)
        // 2. JSON-Deserialisierung fügt weitere Phases hinzu
        // 3. Resultat: Duplikat-GroupPhases!
        
        // Stattdessen: Verwende eine Lazy Initialization-Strategie über EnsureGroupPhaseExists()
        System.Diagnostics.Debug.WriteLine($"TournamentClass Constructor: Phases collection initialized, count = {Phases.Count}");
        
        System.Diagnostics.Debug.WriteLine($"=== TournamentClass Constructor END ===");
    }

    /// <summary>
    /// NEUE METHODE: Stellt sicher, dass mindestens eine GroupPhase existiert
    /// Wird nach JSON-Deserialisierung oder bei erstem Zugriff aufgerufen
    /// Implementiert Lazy Initialization um Duplikate zu vermeiden
    /// </summary>
    public void EnsureGroupPhaseExists()
    {
        System.Diagnostics.Debug.WriteLine($"=== EnsureGroupPhaseExists START for {Name} ===");
        System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Current Phases count = {Phases.Count}");
        
        // Prüfe ob bereits eine GroupPhase existiert
        var existingGroupPhase = Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        
        if (existingGroupPhase == null)
        {
            System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: No GroupPhase found, creating new one");
            
            // Erstelle eine neue GroupPhase
            var groupPhase = new TournamentPhase
            {
                Name = "Gruppenphase",
                PhaseType = TournamentPhaseType.GroupPhase,
                IsActive = true // Gruppenphase ist standardmäßig aktiv
            };
            
            System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Created new TournamentPhase with {groupPhase.Groups.Count} groups");
        
            Phases.Add(groupPhase);
            CurrentPhase = groupPhase;
            
            System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Created new GroupPhase, total phases = {Phases.Count}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: GroupPhase already exists, setting as CurrentPhase");
            
            // Setze die existierende GroupPhase als CurrentPhase wenn noch keine gesetzt
            if (CurrentPhase == null)
            {
                CurrentPhase = existingGroupPhase;
            }
        }
        
        // WICHTIG: KEINE Groups.Count Aufrufe hier - das würde zu Rekursion führen!
        System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: CurrentPhase = {CurrentPhase?.PhaseType}");
        System.Diagnostics.Debug.WriteLine($"=== EnsureGroupPhaseExists END ===");
    }

    /// <summary>
    /// NEUE METHODE: Validiert und repariert Finals-Phase nach JSON-Loading
    /// Stellt sicher dass FinalsGroup korrekt initialisiert ist
    /// </summary>
    public void EnsureFinalsPhaseIntegrity()
    {
        System.Diagnostics.Debug.WriteLine($"=== EnsureFinalsPhaseIntegrity START for {Name} ===");
        
        // Suche nach existierender Finals-Phase
        var finalsPhase = Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals);
        
        if (finalsPhase != null)
        {
            System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: Finals phase found");
            
            // Prüfe ob FinalsGroup existiert
            if (finalsPhase.FinalsGroup == null)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: FinalsGroup is null, attempting to recreate");
                
                // Versuche FinalsGroup aus QualifiedPlayers zu rekonstruieren
                if (finalsPhase.QualifiedPlayers?.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: Recreating FinalsGroup from {finalsPhase.QualifiedPlayers.Count} qualified players");
                    
                    var finalsGroup = new Group
                    {
                        Id = 999,
                        Name = "Finalrunde",
                        MatchesGenerated = false
                    };
                    
                    foreach (var player in finalsPhase.QualifiedPlayers)
                    {
                        finalsGroup.Players.Add(player);
                    }
                    
                    // Generiere Matches falls noch nicht vorhanden
                    if (finalsGroup.Players.Count >= 2)
                    {
                        finalsGroup.GenerateRoundRobinMatches(GameRules);
                        System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: Generated {finalsGroup.Matches.Count} matches");
                    }
                    
                    finalsPhase.FinalsGroup = finalsGroup;
                    
                    System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: FinalsGroup recreated successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: No qualified players found, cannot recreate FinalsGroup");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: FinalsGroup exists with {finalsPhase.FinalsGroup.Players.Count} players and {finalsPhase.FinalsGroup.Matches.Count} matches");
                
                // Sicherstelle, dass Matches generiert sind
                if (!finalsPhase.FinalsGroup.MatchesGenerated && finalsPhase.FinalsGroup.Players.Count >= 2)
                {
                    System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: Generating missing matches");
                    finalsPhase.FinalsGroup.GenerateRoundRobinMatches(GameRules);
                }
            }
        }
        
        System.Diagnostics.Debug.WriteLine($"=== EnsureFinalsPhaseIntegrity END ===");
    }

    /// <summary>
    /// NEUE ÖFFENTLICHE METHODE: Führt vollständige Phase-Validierung nach JSON-Loading durch
    /// Sollte aufgerufen werden nachdem Tournament-Daten geladen wurden
    /// </summary>
    public void ValidateAndRepairPhases()
    {
        System.Diagnostics.Debug.WriteLine($"=== ValidateAndRepairPhases START for {Name} ===");
        
        try
        {
            // 1. Stelle sicher dass GroupPhase existiert
            EnsureGroupPhaseExists();
            
            // 2. Repariere Finals-Phase falls vorhanden
            EnsureFinalsPhaseIntegrity();
            
            // 3. Trigger UI-Refresh um sicherzustellen dass alles geladen wird
            TriggerUIRefresh();
            
            System.Diagnostics.Debug.WriteLine($"ValidateAndRepairPhases: All phases validated and repaired");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ValidateAndRepairPhases: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"ValidateAndRepairPhases: Stack trace: {ex.StackTrace}");
        }
        
        System.Diagnostics.Debug.WriteLine($"=== ValidateAndRepairPhases END ===");
    }

    /// <summary>
    /// Prüft ob zur nächsten Phase gewechselt werden kann
    /// Delegiert die Prüfung an die aktuelle Phase
    /// </summary>
    /// <returns>True wenn alle Voraussetzungen für den Phasenwechsel erfüllt sind</returns>
    public bool CanProceedToNextPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase START ===");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: TournamentClass = {Name}");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: CurrentPhase = {CurrentPhase?.PhaseType}");            
            var result = CurrentPhase?.CanProceedToNextPhase() ?? false;
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Result = {result}");
            System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase END ===");
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Ermittelt die nächste Phase basierend auf der aktuellen Phase und den Spielregeln
    /// Implementiert die Turnierlogik für verschiedene Modi (nur Gruppen, Finals, K.O.)
    /// </summary>
    /// <returns>Die nächste Phase oder null wenn das Turnier beendet ist</returns>
    public TournamentPhase? GetNextPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GetNextPhase START ===");
            
            if (CurrentPhase == null) 
            {
                System.Diagnostics.Debug.WriteLine($"GetNextPhase: CurrentPhase is null");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"GetNextPhase: Current phase = {CurrentPhase.PhaseType}");
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: PostGroupPhaseMode = {GameRules.PostGroupPhaseMode}");

            // Bestimme nächste Phase basierend auf aktueller Phase und Spielregeln
            TournamentPhase? nextPhase = CurrentPhase.PhaseType switch
            {
                TournamentPhaseType.GroupPhase => GameRules.PostGroupPhaseMode switch
                {
                    PostGroupPhaseMode.RoundRobinFinals => CreateRoundRobinFinalsPhase(),
                    PostGroupPhaseMode.KnockoutBracket => CreateKnockoutPhase(),
                    _ => null // Nur Gruppenphase - Turnier endet hier
                },

                TournamentPhaseType.RoundRobinFinals => GameRules.PostGroupPhaseMode == PostGroupPhaseMode.KnockoutBracket 
                    ? CreateKnockoutPhase() 
                    : null, // Finals waren letzte Phase

                TournamentPhaseType.KnockoutPhase => null, // K.O.-Phase ist immer die letzte Phase

                _ => null // Unbekannte Phase
            };
            
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: Next phase = {nextPhase?.PhaseType}");
            System.Diagnostics.Debug.WriteLine($"=== GetNextPhase END ===");
            
            return nextPhase;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Führt den Wechsel zur nächsten Phase durch
    /// Markiert die aktuelle Phase als abgeschlossen und aktiviert die nächste Phase
    /// </summary>
    public void AdvanceToNextPhase()
    {
        // Prüfung ob Phasenwechsel möglich ist
        if (!CanProceedToNextPhase()) return;

        // Ermittlung der nächsten Phase
        var nextPhase = GetNextPhase();
        if (nextPhase == null) return;

        // Markiere aktuelle Phase als abgeschlossen
        if (CurrentPhase != null)
        {
            CurrentPhase.IsActive = false;
            CurrentPhase.IsCompleted = true;
        }

        // Füge neue Phase hinzu und aktiviere sie
        Phases.Add(nextPhase);
        CurrentPhase = nextPhase;
        nextPhase.IsActive = true;
    }

    /// <summary>
    /// Erstellt eine Round-Robin-Finalrunde mit den qualifizierten Spielern aus der Gruppenphase
    /// Alle qualifizierten Spieler spielen nochmals jeder gegen jeden
    /// </summary>
    /// <returns>Eine neue TournamentPhase für die Finalrunde</returns>
    private TournamentPhase CreateRoundRobinFinalsPhase()
    {
        System.Diagnostics.Debug.WriteLine($"=== CreateRoundRobinFinalsPhase START ===");
        
        var finalsPhase = new TournamentPhase
        {
            Name = "Finalrunde",
            PhaseType = TournamentPhaseType.RoundRobinFinals
        };

        // Hole qualifizierte Spieler aus der Gruppenphase
        var groupPhase = Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        var qualifiedPlayers = groupPhase.GetQualifiedPlayers(GameRules.QualifyingPlayersPerGroup);

        System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Found {qualifiedPlayers.Count} qualified players");

        // Erstelle Finals-Gruppe
        var finalsGroup = new Group
        {
            Id = 999, // Spezielle ID für Finals
            Name = "Finalrunde",
            MatchesGenerated = false
        };

        // Füge alle qualifizierten Spieler zur Finals-Gruppe hinzu
        foreach (var player in qualifiedPlayers)
        {
            finalsGroup.Players.Add(player);
            System.Diagnostics.Debug.WriteLine($"  Added player: {player.Name}");
        }

        // KRITISCHER FIX: Generiere die Round Robin Matches für die Finals-Gruppe!
        System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Generating Round Robin matches for {finalsGroup.Players.Count} players");
        finalsGroup.GenerateRoundRobinMatches(GameRules);
        System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Generated {finalsGroup.Matches.Count} matches");

        // WICHTIG: Setze die Finals-Gruppe sowohl in der Phase als auch als QualifiedPlayers
        finalsPhase.FinalsGroup = finalsGroup;
        finalsPhase.QualifiedPlayers = new ObservableCollection<Player>(qualifiedPlayers);

        // ZUSÄTZLICH: Trigger UI-Refresh Event für sofortige Aktualisierung
        System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Triggering UI refresh");
        TriggerUIRefresh();

        System.Diagnostics.Debug.WriteLine($"=== CreateRoundRobinFinalsPhase END - Created phase with {finalsGroup.Matches.Count} matches ===");
        return finalsPhase;
    }

    private TournamentPhase CreateKnockoutPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateKnockoutPhase START ===");
            
            var knockoutPhase = new TournamentPhase
            {
                Name = GameRules.KnockoutMode == KnockoutMode.DoubleElimination ? "KO-Phase (Double Elimination)" : "KO-Phase (Single Elimination)",
                PhaseType = TournamentPhaseType.KnockoutPhase
            };

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Created phase with name '{knockoutPhase.Name}'");

            // Get qualified players from previous phase
            List<Player> qualifiedPlayers;
            if (CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Getting players from RoundRobinFinals");
                qualifiedPlayers = CurrentPhase.GetQualifiedPlayers(int.MaxValue); // All players from finals
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Getting players from GroupPhase");
                var groupPhase = Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
                qualifiedPlayers = groupPhase.GetQualifiedPlayers(GameRules.QualifyingPlayersPerGroup);
            }

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Found {qualifiedPlayers.Count} qualified players");
            foreach (var player in qualifiedPlayers)
            {
                System.Diagnostics.Debug.WriteLine($"  - {player.Name}");
            }

            // WICHTIGE VALIDIERUNG: Prüfen ob genügend Spieler vorhanden
            if (qualifiedPlayers.Count <= 1)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: ERROR - Not enough players ({qualifiedPlayers.Count})");
                throw new InvalidOperationException($"Nicht genügend Spieler für K.O.-Phase: {qualifiedPlayers.Count}");
            }

            knockoutPhase.QualifiedPlayers = new ObservableCollection<Player>(qualifiedPlayers);

            // Generate bracket
            if (GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generating double elimination bracket");
                GenerateDoubleEliminationBracket(knockoutPhase, qualifiedPlayers);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generating single elimination bracket");
                GenerateSingleEliminationBracket(knockoutPhase, qualifiedPlayers);
            }

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generated {knockoutPhase.WinnerBracket.Count} winner bracket matches");
            System.Diagnostics.Debug.WriteLine($"=== CreateKnockoutPhase END ===");

            return knockoutPhase;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void GenerateSingleEliminationBracket(TournamentPhase phase, List<Player> players)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GenerateSingleEliminationBracket START ===");
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: {players.Count} players");
            
            var shuffledPlayers = players.OrderBy(x => Guid.NewGuid()).ToList(); // Random seeding
            var matches = new List<KnockoutMatch>();
            int matchId = 1;

            // Calculate bracket structure
            int playersCount = shuffledPlayers.Count;
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: playersCount = {playersCount}");
            
            // Find next power of 2
            int nextPowerOf2 = 1;
            while (nextPowerOf2 < playersCount)
            {
                nextPowerOf2 *= 2;
            }
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: nextPowerOf2 = {nextPowerOf2}");

            // Calculate byes and actual matches in first round
            int byesNeeded = nextPowerOf2 - playersCount;
            int playersWithoutBye = playersCount - byesNeeded;
            int firstRoundMatches = playersWithoutBye / 2;
            
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: byesNeeded = {byesNeeded}");
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: playersWithoutBye = {playersWithoutBye}");
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: firstRoundMatches = {firstRoundMatches}");

            // Determine starting round based on final bracket size
            KnockoutRound startingRound = GetStartingRound(nextPowerOf2);
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: startingRound = {startingRound}");

            // Create first round matches
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Generating first round with {firstRoundMatches} actual matches");
            var currentRoundMatches = new List<KnockoutMatch>();
            
            // First, create matches with actual players (no byes)
            int playerIndex = 0;
            for (int i = 0; i < firstRoundMatches; i++)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln
                var roundRules = GameRules.GetRulesForRound(startingRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = shuffledPlayers[playerIndex++],
                    Player2 = shuffledPlayers[playerIndex++],
                    BracketType = BracketType.Winner,
                    Round = startingRound,
                    Position = i,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                };

                System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}, Round: {startingRound}, UsesSets: {match.UsesSets} (SetsToWin: {roundRules.SetsToWin})");
                currentRoundMatches.Add(match);
                matches.Add(match);
            }

            // Then, create "bye matches" for remaining players
            for (int i = 0; i < byesNeeded; i++)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln auch für Bye-Matches
                var roundRules = GameRules.GetRulesForRound(startingRound);
                
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = shuffledPlayers[playerIndex++],
                    Player2 = null, // Bye
                    Winner = shuffledPlayers[playerIndex - 1], // Player with bye automatically wins
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Winner,
                    Round = startingRound,
                    Position = firstRoundMatches + i,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets auch für Bye-Matches
                };

                System.Diagnostics.Debug.WriteLine($"  Match {byeMatch.Id}: {byeMatch.Player1?.Name} gets bye, Round: {startingRound}, UsesSets: {byeMatch.UsesSets} (SetsToWin: {roundRules.SetsToWin})");
                currentRoundMatches.Add(byeMatch);
                matches.Add(byeMatch);
            }
            
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Generated {currentRoundMatches.Count} first round matches ({firstRoundMatches} actual + {byesNeeded} byes)");

            // Generate subsequent rounds
            var currentRound = startingRound;
            int roundCounter = 2;
            while (currentRoundMatches.Count > 1)
            {
                System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Generating round {roundCounter} with {currentRoundMatches.Count} current matches");
                
                var nextRoundMatches = new List<KnockoutMatch>();
                
                // Calculate next round BEFORE creating matches
                var nextRound = GetNextRound(currentRound, currentRoundMatches.Count);
                System.Diagnostics.Debug.WriteLine($"  Next round will be: {nextRound}");
                
                for (int i = 0; i < currentRoundMatches.Count; i += 2)
                {
                    // WICHTIG: Bestimme rundenspezifische Regeln für jede Runde
                    var roundRules = GameRules.GetRulesForRound(nextRound);
                    
                    var match = new KnockoutMatch
                    {
                        Id = matchId++,
                        SourceMatch1 = currentRoundMatches[i],
                        SourceMatch2 = i + 1 < currentRoundMatches.Count ? currentRoundMatches[i + 1] : null,
                        BracketType = BracketType.Winner,
                        Round = nextRound,
                        Position = i / 2,
                        Player1FromWinner = true,
                        Player2FromWinner = true,
                        UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                    };

                    System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: Winner of {match.SourceMatch1?.Id} vs Winner of {match.SourceMatch2?.Id ?? 0}, Round: {nextRound}, UsesSets: {match.UsesSets} (SetsToWin: {roundRules.SetsToWin})");
                    nextRoundMatches.Add(match);
                    matches.Add(match);
                }

                currentRoundMatches = nextRoundMatches;
                currentRound = nextRound;
                roundCounter++;
                
                System.Diagnostics.Debug.WriteLine($"  Generated {nextRoundMatches.Count} matches for round {roundCounter - 1}");
            }

            phase.WinnerBracket = new ObservableCollection<KnockoutMatch>(matches);
            
            // Propagiere nur initiale Bye-Matches
            PropagateInitialByeMatches(phase.WinnerBracket, null);
            
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Total matches generated: {matches.Count}");
            System.Diagnostics.Debug.WriteLine($"=== GenerateSingleEliminationBracket END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GenerateSingleEliminationBracket: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private void GenerateDoubleEliminationBracket(TournamentPhase phase, List<Player> players)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GenerateDoubleEliminationBracket START ===");
            System.Diagnostics.Debug.WriteLine($"GenerateDoubleEliminationBracket: {players.Count} players");

            // Generate winner bracket
            var winnerBracketMatches = GenerateWinnerBracket(players, 1);
            phase.WinnerBracket = new ObservableCollection<KnockoutMatch>(winnerBracketMatches);

            // Generate loser bracket
            var loserBracketMatches = GenerateLoserBracket(winnerBracketMatches, players);
            phase.LoserBracket = new ObservableCollection<KnockoutMatch>(loserBracketMatches);

            // Generate grand final
            if (winnerBracketMatches.Any() && loserBracketMatches.Any())
            {
                GenerateGrandFinal(phase);
            }

            // Propagate initial bye matches
            PropagateInitialByeMatches(phase.WinnerBracket, phase.LoserBracket);
            PropagateInitialByeMatches(phase.LoserBracket, null);

            System.Diagnostics.Debug.WriteLine($"GenerateDoubleEliminationBracket: Generated {phase.WinnerBracket.Count} winner matches and {phase.LoserBracket.Count} loser matches");
            System.Diagnostics.Debug.WriteLine($"=== GenerateDoubleEliminationBracket END ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GenerateDoubleEliminationBracket: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"GenerateDoubleEliminationBracket: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private List<KnockoutMatch> GenerateWinnerBracket(List<Player> players, int startId)
    {
        var matches = new List<KnockoutMatch>();
        var shuffledPlayers = players.OrderBy(x => Guid.NewGuid()).ToList();
        int matchId = startId;

        // Calculate bracket size
        int playersCount = shuffledPlayers.Count;
        int nextPowerOf2 = 1;
        while (nextPowerOf2 < playersCount) nextPowerOf2 *= 2;

        int byesNeeded = nextPowerOf2 - playersCount;
        int firstRoundMatches = (playersCount - byesNeeded) / 2;

        KnockoutRound startingRound = GetStartingRound(nextPowerOf2);
        var currentRoundMatches = new List<KnockoutMatch>();

        int playerIndex = 0;

        // Create first round actual matches
        for (int i = 0; i < firstRoundMatches; i++)
        {
            // WICHTIG: Bestimme rundenspezifische Regeln
            var roundRules = GameRules.GetRulesForRound(startingRound);
            
            var match = new KnockoutMatch
            {
                Id = matchId++,
                Player1 = shuffledPlayers[playerIndex++],
                Player2 = shuffledPlayers[playerIndex++],
                BracketType = BracketType.Winner,
                Round = startingRound,
                Position = i,
                UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets basierend auf Rundenregeln
            };

            System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}, Round: {startingRound}, UsesSets: {match.UsesSets} (SetsToWin: {roundRules.SetsToWin})");
            currentRoundMatches.Add(match);
            matches.Add(match);
        }

        // Create bye matches
        for (int i = 0; i < byesNeeded; i++)
        {
            // WICHTIG: Bestimme rundenspezifische Regeln auch für Bye-Matches
            var roundRules = GameRules.GetRulesForRound(startingRound);
            
            var byeMatch = new KnockoutMatch
            {
                Id = matchId++,
                Player1 = shuffledPlayers[playerIndex++],
                Player2 = null,
                Winner = shuffledPlayers[playerIndex - 1],
                Status = MatchStatus.Bye,
                BracketType = BracketType.Winner,
                Round = startingRound,
                Position = firstRoundMatches + i,
                UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets auch für Bye-Matches
            };
            currentRoundMatches.Add(byeMatch);
            matches.Add(byeMatch);
        }

        // Generate subsequent rounds
        var currentRound = startingRound;
        while (currentRoundMatches.Count > 1)
        {
            var nextRoundMatches = new List<KnockoutMatch>();
            var nextRound = GetNextRound(currentRound, currentRoundMatches.Count);

            for (int i = 0; i < currentRoundMatches.Count; i += 2)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln für jede Runde
                var roundRules = GameRules.GetRulesForRound(nextRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    SourceMatch1 = currentRoundMatches[i],
                    SourceMatch2 = i + 1 < currentRoundMatches.Count ? currentRoundMatches[i + 1] : null,
                    BracketType = BracketType.Winner,
                    Round = nextRound,
                    Position = i / 2,
                    Player1FromWinner = true,
                    Player2FromWinner = true,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                };

                System.Diagnostics.Debug.WriteLine($"  Match {match.Id}: Winner of {match.SourceMatch1?.Id} vs Winner of {match.SourceMatch2?.Id ?? 0}, Round: {nextRound}, UsesSets: {match.UsesSets} (SetsToWin: {roundRules.SetsToWin})");
                nextRoundMatches.Add(match);
                matches.Add(match);
            }

            currentRoundMatches = nextRoundMatches;
            currentRound = nextRound;
        }

        return matches;
    }

    private List<KnockoutMatch> GenerateLoserBracket(List<KnockoutMatch> winnerMatches, List<Player> allPlayers)
    {
        var loserMatches = new List<KnockoutMatch>();
        int matchId = winnerMatches.Max(m => m.Id) + 1;

        // Get group phase losers if enabled
        var groupPhaseLosers = new List<Player>();
        if (GameRules.IncludeGroupPhaseLosersBracket)
        {
            var groupPhase = Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
            var qualifiedPlayers = groupPhase.GetQualifiedPlayers(GameRules.QualifyingPlayersPerGroup);
            var allGroupPlayers = groupPhase.Groups.SelectMany(g => g.Players).ToList();
            groupPhaseLosers = allGroupPlayers.Where(p => !qualifiedPlayers.Contains(p)).ToList();
        }

        // Start with group phase losers
        List<KnockoutMatch> currentRound = new List<KnockoutMatch>();
        KnockoutRound currentLoserRound = KnockoutRound.LoserRound1;

        if (groupPhaseLosers.Any())
        {
            int position = 0;
            for (int i = 0; i < groupPhaseLosers.Count - 1; i += 2)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln für Loser Bracket
                var roundRules = GameRules.GetRulesForRound(currentLoserRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = groupPhaseLosers[i],
                    Player2 = groupPhaseLosers[i + 1],
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                };
                currentRound.Add(match);
                loserMatches.Add(match);
            }

            // Handle odd number with bye
            if (groupPhaseLosers.Count % 2 == 1)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln auch für Bye-Matches
                var roundRules = GameRules.GetRulesForRound(currentLoserRound);
                
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    Player1 = groupPhaseLosers.Last(),
                    Player2 = null,
                    Winner = groupPhaseLosers.Last(),
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets auch für Bye-Matches
                };
                currentRound.Add(byeMatch);
                loserMatches.Add(byeMatch);
            }
        }

        // Integrate winner bracket losers
        var winnerRounds = winnerMatches.GroupBy(m => m.Round).OrderBy(g => g.Key).ToList();
        
        foreach (var winnerRound in winnerRounds.Take(winnerRounds.Count - 1)) // Skip final
        {
            // *** KORREKTUR: Nur advance wenn bereits Matches im currentRound sind ***

            if (currentRound.Any())
            {
                currentLoserRound = GetNextLoserRound(currentLoserRound);
            }
            
            var nextRound = new List<KnockoutMatch>();
            int position = 0;

            var loserProducingMatches = winnerRound.Where(m => m.Status != MatchStatus.Bye).ToList();
            var allParticipants = new List<object>();
            allParticipants.AddRange(currentRound.Cast<object>());
            allParticipants.AddRange(loserProducingMatches.Cast<object>());

            for (int i = 0; i < allParticipants.Count - 1; i += 2)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln für jede Loser Bracket Runde
                var roundRules = GameRules.GetRulesForRound(currentLoserRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                };

                SetLoserBracketParticipant(match, allParticipants[i], true);
                SetLoserBracketParticipant(match, allParticipants[i + 1], false);

                nextRound.Add(match);
                loserMatches.Add(match);
            }

            // Handle odd number
            if (allParticipants.Count % 2 == 1)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln auch für Bye-Matches
                var roundRules = GameRules.GetRulesForRound(currentLoserRound);
                
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    Player2 = null,
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets auch für Bye-Matches
                };

                SetLoserBracketParticipant(byeMatch, allParticipants.Last(), true);
                if (allParticipants.Last() is Player player)
                {
                    byeMatch.Winner = player;
                }

                nextRound.Add(byeMatch);
                loserMatches.Add(byeMatch);
            }

            currentRound = nextRound;
        }

        // Continue loser bracket until final
        while (currentRound.Count > 1)
        {
            currentLoserRound = GetNextLoserRound(currentLoserRound);
            var nextRound = new List<KnockoutMatch>();
            int position = 0;

            for (int i = 0; i < currentRound.Count - 1; i += 2)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln für jede weitere Runde
                var roundRules = GameRules.GetRulesForRound(currentLoserRound);
                
                var match = new KnockoutMatch
                {
                    Id = matchId++,
                    SourceMatch1 = currentRound[i],
                    SourceMatch2 = currentRound[i + 1],
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    Player1FromWinner = true,
                    Player2FromWinner = true,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets basierend auf Rundenregeln
                };
                nextRound.Add(match);
                loserMatches.Add(match);
            }

            if (currentRound.Count % 2 == 1)
            {
                // WICHTIG: Bestimme rundenspezifische Regeln auch für finale Bye-Matches
                var roundRules = GameRules.GetRulesForRound(currentLoserRound);
                
                var byeMatch = new KnockoutMatch
                {
                    Id = matchId++,
                    SourceMatch1 = currentRound.Last(),
                    SourceMatch2 = null,
                    Status = MatchStatus.Bye,
                    BracketType = BracketType.Loser,
                    Round = currentLoserRound,
                    Position = position++,
                    Player1FromWinner = true,
                    UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets auch für finale Bye-Matches
                };
                nextRound.Add(byeMatch);
                loserMatches.Add(byeMatch);
            }

            currentRound = nextRound;
        }

        // *** KORREKTUR: Stelle sicher dass das letzte Match als LoserFinal markiert wird ***
        if (currentRound.Count == 1)
        {
            var finalMatch = currentRound[0];
            System.Diagnostics.Debug.WriteLine($"GenerateLoserBracket: Correcting final match {finalMatch.Id} from {finalMatch.Round} to LoserFinal");
            finalMatch.Round = KnockoutRound.LoserFinal;
        }

        return loserMatches;
    }

    private void SetLoserBracketParticipant(KnockoutMatch loserMatch, object participant, bool isPlayer1)
    {
        if (participant is Player player)
        {
            if (isPlayer1)
                loserMatch.Player1 = player;
            else
                loserMatch.Player2 = player;
        }
        else if (participant is KnockoutMatch sourceMatch)
        {
            if (isPlayer1)
            {
                loserMatch.SourceMatch1 = sourceMatch;
                loserMatch.Player1FromWinner = sourceMatch.BracketType != BracketType.Winner;
            }
            else
            {
                loserMatch.SourceMatch2 = sourceMatch;
                loserMatch.Player2FromWinner = sourceMatch.BracketType != BracketType.Winner;
            }
        }
    }

    private void GenerateGrandFinal(TournamentPhase phase)
    {
        var winnerFinal = phase.WinnerBracket.LastOrDefault(m => m.Round != KnockoutRound.GrandFinal);
        var loserFinal = phase.LoserBracket.LastOrDefault(m => m.Round == KnockoutRound.LoserFinal);

        System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: winnerFinal = {winnerFinal?.Id} (Round: {winnerFinal?.Round})");
        System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: loserFinal = {loserFinal?.Id} (Round: {loserFinal?.Round})");

        if (winnerFinal == null || loserFinal == null) 
        {
            System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Cannot create Grand Final - missing source matches");
            return;
        }

        int matchId = Math.Max(
            phase.WinnerBracket.Max(m => m.Id),
            phase.LoserBracket.Max(m => m.Id)
        ) + 1;

        // WICHTIG: Bestimme rundenspezifische Regeln auch für Grand Final
        var roundRules = GameRules.GetRulesForRound(KnockoutRound.GrandFinal);

        var grandFinal = new KnockoutMatch
        {
            Id = matchId,
            BracketType = BracketType.Winner,
            Round = KnockoutRound.GrandFinal,
            Position = 0,
            SourceMatch1 = winnerFinal,
            SourceMatch2 = loserFinal,
            Player1FromWinner = true,
            Player2FromWinner = true,
            UsesSets = roundRules.SetsToWin > 0 // WICHTIG: Setze UsesSets auch für Grand Final
        };

        System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Created Grand Final match {grandFinal.Id} with sources WB:{winnerFinal.Id} and LB:{loserFinal.Id}");

        var winnerMatches = phase.WinnerBracket.ToList();
        winnerMatches.Add(grandFinal);
        phase.WinnerBracket = new ObservableCollection<KnockoutMatch>(winnerMatches);
        
        System.Diagnostics.Debug.WriteLine($"GenerateGrandFinal: Grand Final added to Winner Bracket, total matches = {phase.WinnerBracket.Count}");
    }

    private KnockoutRound GetNextLoserRound(KnockoutRound currentRound)
    {
        return currentRound switch
        {
            KnockoutRound.LoserRound1 => KnockoutRound.LoserRound2,
            KnockoutRound.LoserRound2 => KnockoutRound.LoserRound3,
            KnockoutRound.LoserRound3 => KnockoutRound.LoserRound4,
            KnockoutRound.LoserRound4 => KnockoutRound.LoserRound5,
            KnockoutRound.LoserRound5 => KnockoutRound.LoserRound6,
            KnockoutRound.LoserRound6 => KnockoutRound.LoserRound7,
            KnockoutRound.LoserRound7 => KnockoutRound.LoserRound8,
            KnockoutRound.LoserRound8 => KnockoutRound.LoserRound9,
            KnockoutRound.LoserRound9 => KnockoutRound.LoserRound10,
            KnockoutRound.LoserRound10 => KnockoutRound.LoserRound11,
            KnockoutRound.LoserRound11 => KnockoutRound.LoserRound12,
            KnockoutRound.LoserRound12 => KnockoutRound.LoserFinal,
            _ => KnockoutRound.LoserFinal
        };
    }

    #region Helper Methods

    private static int GetNextPowerOfTwo(int n)
    {
        if (n <= 0) return 1;
        int power = 1;
        while (power < n) power *= 2;
        return power;
    }

    private static KnockoutRound GetStartingRound(int bracketSize)
    {
        return bracketSize switch
        {
            <= 2 => KnockoutRound.Final,
            <= 4 => KnockoutRound.Semifinal,
            <= 8 => KnockoutRound.Quarterfinal,
            <= 16 => KnockoutRound.Best16,
            <= 32 => KnockoutRound.Best32,
            <= 64 => KnockoutRound.Best64,
            _ => KnockoutRound.Best64
        };
    }

    private static KnockoutRound GetNextRound(KnockoutRound currentRound, int currentMatchCount)
    {
        return currentMatchCount switch
        {
            <= 2 => KnockoutRound.Final,
            <= 4 => KnockoutRound.Semifinal,
            <= 8 => KnockoutRound.Quarterfinal,
            <= 16 => KnockoutRound.Best16,
            <= 32 => KnockoutRound.Best32,
            <= 64 => KnockoutRound.Best64,
            _ => currentRound switch
            {
                KnockoutRound.Best64 => KnockoutRound.Best32,
                KnockoutRound.Best32 => KnockoutRound.Best16,
                KnockoutRound.Best16 => KnockoutRound.Quarterfinal,
                KnockoutRound.Quarterfinal => KnockoutRound.Semifinal,
                KnockoutRound.Semifinal => KnockoutRound.Final,
                _ => KnockoutRound.Final
            }
        };
    }

    #endregion

    #region Propagation Methods - VOLLSTÄNDIGE FREILOS-LÖSUNG

    /// <summary>
    /// HAUPTMETHODE: Propagiert initiale Bye-Matches und aktiviert automatische Freilos-Erkennung
    /// </summary>
    private void PropagateInitialByeMatches(ObservableCollection<KnockoutMatch> bracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        System.Diagnostics.Debug.WriteLine($"=== PropagateInitialByeMatches START ===");
        
        var byeMatches = bracket.Where(m => m.Status == MatchStatus.Bye && m.Winner != null).ToList();
        System.Diagnostics.Debug.WriteLine($"  Found {byeMatches.Count} initial bye matches to propagate");

        foreach (var byeMatch in byeMatches)
        {
            System.Diagnostics.Debug.WriteLine($"  Propagating initial bye match {byeMatch.Id} (winner: {byeMatch.Winner?.Name})");
            PropagateMatchResultWithAutomaticByes(byeMatch, bracket, otherBracket);
        }

        System.Diagnostics.Debug.WriteLine($"=== PropagateInitialByeMatches END ===");
    }

    /// <summary>
    /// KERNMETHODE: Propagiert Match-Ergebnisse und überprüft automatisch auf neue Freilose
    /// </summary>
    private void PropagateMatchResultWithAutomaticByes(KnockoutMatch completedMatch, ObservableCollection<KnockoutMatch> sameBracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        if (completedMatch.Winner == null) return;

        System.Diagnostics.Debug.WriteLine($"    Propagating match {completedMatch.Id} (winner: {completedMatch.Winner.Name})");

        // 1. Propagiere Gewinner in das gleiche Bracket
        var sameBracketDependents = sameBracket.Where(m => 
            (m.SourceMatch1?.Id == completedMatch.Id && m.Player1FromWinner) ||
            (m.SourceMatch2?.Id == completedMatch.Id && m.Player2FromWinner)).ToList();

        foreach (var dependent in sameBracketDependents)
        {
            if (dependent.SourceMatch1?.Id == completedMatch.Id && dependent.Player1FromWinner)
            {
                dependent.Player1 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player1 in match {dependent.Id}");
            }
            
            if (dependent.SourceMatch2?.Id == completedMatch.Id && dependent.Player2FromWinner)
            {
                dependent.Player2 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player2 in match {dependent.Id}");
            }
        }

        // 2. Propagiere Verlierer ins andere Bracket (Winner ? Loser)
        if (otherBracket != null && completedMatch.BracketType == BracketType.Winner)
        {
            var loser = GetMatchLoser(completedMatch);
            if (loser != null)
            {
                System.Diagnostics.Debug.WriteLine($"      WB Match {completedMatch.Id}: Loser {loser.Name} goes to LB");
                
                var crossBracketDependents = otherBracket.Where(m => 
                    (m.SourceMatch1?.Id == completedMatch.Id && !m.Player1FromWinner) ||
                    (m.SourceMatch2?.Id == completedMatch.Id && !m.Player2FromWinner)).ToList();

                foreach (var dependent in crossBracketDependents)
                {
                    if (dependent.SourceMatch1?.Id == completedMatch.Id && !dependent.Player1FromWinner)
                    {
                        dependent.Player1 = loser;
                        System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player1 in LB match {dependent.Id}");
                    }
                    
                    if (dependent.SourceMatch2?.Id == completedMatch.Id && !dependent.Player2FromWinner)
                    {
                        dependent.Player2 = loser;
                        System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player2 in LB match {dependent.Id}");
                    }
                }
            }
        }

        // 3. KRITISCH: Prüfe nach der Propagation auf neue automatische Freilose
        CheckAndHandleAutomaticByes(sameBracket, otherBracket);
        if (otherBracket != null)
        {
            CheckAndHandleAutomaticByes(otherBracket, sameBracket);
        }
    }

    /// <summary>
    /// Direktes Propagieren von Spielergewinnen-Nachrichten ohne Nachverfolgung von Freilosen
    /// </summary>
    private void PropagateMatchResultDirectly(KnockoutMatch completedMatch, ObservableCollection<KnockoutMatch> sameBracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        if (completedMatch.Winner == null) return;

        System.Diagnostics.Debug.WriteLine($"    Directly propagating match {completedMatch.Id} (winner: {completedMatch.Winner.Name})");

        // 1. Propagiere Gewinner in das gleiche Bracket
        var sameBracketDependents = sameBracket.Where(m => 
            (m.SourceMatch1?.Id == completedMatch.Id && m.Player1FromWinner) ||
            (m.SourceMatch2?.Id == completedMatch.Id && m.Player2FromWinner)).ToList();

        foreach (var dependent in sameBracketDependents)
        {
            if (dependent.SourceMatch1?.Id == completedMatch.Id && dependent.Player1FromWinner)
            {
                dependent.Player1 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player1 in match {dependent.Id}");
            }
            
            if (dependent.SourceMatch2?.Id == completedMatch.Id && dependent.Player2FromWinner)
            {
                dependent.Player2 = completedMatch.Winner;
                System.Diagnostics.Debug.WriteLine($"      Set winner {completedMatch.Winner.Name} as Player2 in match {dependent.Id}");
            }
        }

        // 2. Propagiere Verlierer ins andere Bracket (Winner ? Loser)
        if (otherBracket != null && completedMatch.BracketType == BracketType.Winner)
        {
            var loser = GetMatchLoser(completedMatch);
            if (loser != null)
            {
                System.Diagnostics.Debug.WriteLine($"      WB Match {completedMatch.Id}: Loser {loser.Name} goes to LB");
                
                var crossBracketDependents = otherBracket.Where(m => 
                    (m.SourceMatch1?.Id == completedMatch.Id && !m.Player1FromWinner) ||
                    (m.SourceMatch2?.Id == completedMatch.Id && !m.Player2FromWinner)).ToList();

                foreach (var dependent in crossBracketDependents)
                {
                    if (dependent.SourceMatch1?.Id == completedMatch.Id && !dependent.Player1FromWinner)
                    {
                        dependent.Player1 = loser;
                        System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player1 in LB match {dependent.Id}");
                    }
                    
                    if (dependent.SourceMatch2?.Id == completedMatch.Id && !dependent.Player2FromWinner)
                    {
                        dependent.Player2 = loser;
                        System.Diagnostics.Debug.WriteLine($"      Set loser {loser.Name} as Player2 in LB match {dependent.Id}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// VERBESSERTE LÖSUNG: Überprüft ALLE Arten von automatischen Freilosen
    /// WICHTIG: Nur für Freilos-Szenarien, nicht für normale Match-Ergebnisse
    /// </summary>
    private void CheckAndHandleAutomaticByes(ObservableCollection<KnockoutMatch> bracket, ObservableCollection<KnockoutMatch>? otherBracket)
    {
        System.Diagnostics.Debug.WriteLine($"      === ENHANCED: CheckAndHandleAutomaticByes START ===");
        
        bool foundNewByes;
        int iterations = 0;
        const int maxIterations = 15; // Erhöht für komplexe Szenarien

        do
        {
            foundNewByes = false;
            iterations++;
            
            System.Diagnostics.Debug.WriteLine($"        Iteration {iterations}: Checking for automatic byes...");

            // FALL 1: Neue Freilose durch Spieler-Hinzufügung (aber nicht bei bereits beendeten Matches)
            var newByeMatches = bracket.Where(m => 
                m.Status == MatchStatus.NotStarted && 
                ShouldBeAutomaticBye(m))
                .ToList();

            // FALL 2: Ursprünglich Bye-markierte Matches mit neuen Spielern (aber keine finished matches)
            var convertedMatches = bracket.Where(m => 
                m.Status == MatchStatus.Bye && 
                m.Winner == null && 
                ShouldConvertByeToAutomatic(m))
                .ToList();

            System.Diagnostics.Debug.WriteLine($"        Found {newByeMatches.Count} new byes, {convertedMatches.Count} converted byes");

            // Behandle neue automatische Freilose
            foreach (var match in newByeMatches)
            {
                var automaticWinner = DetermineAutomaticWinner(match);
                
                if (automaticWinner != null)
                {
                    System.Diagnostics.Debug.WriteLine($"        NEW AUTO BYE: Match {match.Id} - {automaticWinner.Name} advances automatically");
    
                    match.Status = MatchStatus.Bye;
                    match.Winner = automaticWinner;
                    foundNewByes = true;
                    
                    // Verwende direkte Propagation ohne weitere Bye-Checks
                    PropagateMatchResultDirectly(match, bracket, otherBracket);
                }
            }

            // Behandle konvertierte Bye-Matches
            foreach (var match in convertedMatches)
            {
                var automaticWinner = DetermineAutomaticWinner(match);
                
                if (automaticWinner != null)
                {
                    System.Diagnostics.Debug.WriteLine($"        CONVERTED BYE: Match {match.Id} - {automaticWinner.Name} now gets automatic bye");
                    
                    match.Winner = automaticWinner; // Status bleibt Bye
                    foundNewByes = true;
                    
                    // Verwende direkte Propagation ohne weitere Bye-Checks
                    PropagateMatchResultDirectly(match, bracket, otherBracket);
                }
            }
            
        } while (foundNewByes && iterations < maxIterations);

        if (iterations >= maxIterations)
        {
            System.Diagnostics.Debug.WriteLine($"        WARNING: Reached maximum iterations ({maxIterations}) in enhanced bye check");
        }
        
        System.Diagnostics.Debug.WriteLine($"      === ENHANCED: CheckAndHandleAutomaticByes END ===");
    }

    /// <summary>
    /// NEU: Bestimmt ob ein Match automatisch als Bye behandelt werden sollte
    /// WICHTIG: Nur für NotStarted Matches, nicht für bereits fertige Matches
    /// </summary>
    private bool ShouldBeAutomaticBye(KnockoutMatch match)
    {
        // SICHERHEITSCHECK: Keine Automatic-Byes für bereits beendete oder laufende Matches
        if (match.Status != MatchStatus.NotStarted)
        {
            System.Diagnostics.Debug.WriteLine($"        ShouldBeAutomaticBye: Match {match.Id} not NotStarted, skipping");
            return false;
        }

        // Hilfsfunktion: Prüft ob ein Player gültig ist (nicht null und nicht "TBD")
        bool IsValidPlayer(Player? player)
        {
            return player != null && 
                   !string.IsNullOrEmpty(player.Name) && 
                   !player.Name.Equals("TBD", StringComparison.OrdinalIgnoreCase) &&
                   !player.Name.Equals("To Be Determined", StringComparison.OrdinalIgnoreCase);
        }

        System.Diagnostics.Debug.WriteLine($"        ShouldBeAutomaticBye: Match {match.Id} - Player1: {match.Player1?.Name ?? "null"}, Player2: {match.Player2?.Name ?? "null"}");

        // Fall 1: Ein gültiger Spieler vorhanden, kein zweiter erwartet
        if (IsValidPlayer(match.Player1) && match.Player2 == null && match.SourceMatch2 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player1, no Player2 expected");
            return true;
        }
            
        // Fall 2: Ein gültiger Spieler vorhanden, kein erster erwartet  
        if (match.Player1 == null && IsValidPlayer(match.Player2) && match.SourceMatch1 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player2, no Player1 expected");
            return true;
        }

        // Fall 3: Ein gültiger Spieler + ein TBD Spieler, aber keine wartenden Matches
        if (IsValidPlayer(match.Player1) && match.Player2 != null && !IsValidPlayer(match.Player2) && match.SourceMatch2 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player1, Player2 is TBD, no SourceMatch2");
            return true;
        }

        if (match.Player1 != null && !IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2) && match.SourceMatch1 == null)
        {
            System.Diagnostics.Debug.WriteLine($"          -> Should be bye: Valid Player2, Player1 is TBD, no SourceMatch1");
            return true;
        }
            
        System.Diagnostics.Debug.WriteLine($"          -> Should NOT be bye");
        return false;
    }

    /// <summary>
    /// NEU: Bestimmt ob ein ursprüngliches Bye-Match zu einem automatischen Bye konvertiert werden sollte
    /// </summary>
    private bool ShouldConvertByeToAutomatic(KnockoutMatch match)
    {
        // Bereits als Bye markiert, aber ohne Gewinner und jetzt mit Spieler(n)
        return (match.Player1 != null || match.Player2 != null);
    }

    /// <summary>
    /// VERBESSERT: Bestimmt den automatischen Gewinner eines Matches (filtert TBD aus)
    /// </summary>
    private Player? DetermineAutomaticWinner(KnockoutMatch match)
    {
        // Hilfsfunktion: Prüft ob ein Player gültig ist (nicht null und nicht "TBD")
        bool IsValidPlayer(Player? player)
        {
            return player != null && 
                   !string.IsNullOrEmpty(player.Name) && 
                   !player.Name.Equals("TBD", StringComparison.OrdinalIgnoreCase) &&
                   !player.Name.Equals("To Be Determined", StringComparison.OrdinalIgnoreCase);
        }

        System.Diagnostics.Debug.WriteLine($"        DetermineAutomaticWinner: Match {match.Id}");
        System.Diagnostics.Debug.WriteLine($"          Player1: {match.Player1?.Name ?? "null"}");
        System.Diagnostics.Debug.WriteLine($"          Player2: {match.Player2?.Name ?? "null"}");

        // Fall 1: Nur Player1 ist ein gültiger Spieler
        if (IsValidPlayer(match.Player1) && !IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Player1 {match.Player1.Name} wins automatically (Player2 is TBD/null)");
            return match.Player1;
        }
            
        // Fall 2: Nur Player2 ist ein gültiger Spieler
        if (!IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Player2 {match.Player2.Name} wins automatically (Player1 is TBD/null)");
            return match.Player2;
        }
            
        // Fall 3: Beide Spieler sind gültig - kein automatisches Bye
        if (IsValidPlayer(match.Player1) && IsValidPlayer(match.Player2))
        {
            System.Diagnostics.Debug.WriteLine($"          -> Both players valid, no automatic bye");
            return null;
        }

        // Fall 4: Beide Spieler sind TBD/null - kein automatisches Bye
        System.Diagnostics.Debug.WriteLine($"          -> Both players TBD/null, no automatic bye");
        return null;
    }

    /// <summary>
    /// ÖFFENTLICHE METHODE: Manueller Refresh aller Freilose
    /// </summary>
    public void RefreshAllByeMatches()
    {
        System.Diagnostics.Debug.WriteLine($"=== PUBLIC RefreshAllByeMatches START ===");
        
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return;
        }

        var winnerBracket = CurrentPhase.WinnerBracket;
        var loserBracket = CurrentPhase.LoserBracket;
        
        CheckAndHandleAutomaticByes(winnerBracket, loserBracket);
        if (loserBracket.Any())
        {
            CheckAndHandleAutomaticByes(loserBracket, winnerBracket);
        }
        
        // Trigger UI refresh nach Bye-Check
        TriggerUIRefresh();
        
        System.Diagnostics.Debug.WriteLine($"=== PUBLIC RefreshAllByeMatches END ===");
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE für manuelle Freilos-Vergabe
    /// </summary>
    /// <param name="match">Das Match, in dem ein Freilos vergeben werden soll</param>
    /// <param name="byeWinner">Der Spieler, der das Freilos bekommen soll (null = automatisch bestimmen)</param>
    /// <returns>True wenn erfolgreich, False wenn nicht möglich</returns>
    public bool GiveManualBye(KnockoutMatch match, Player? byeWinner = null)
    {
        System.Diagnostics.Debug.WriteLine($"=== GiveManualBye START for match {match.Id} ===");
        
        try
        {
            if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in knockout phase - cannot give bye");
                return false;
            }

            // Validation: Match must not be finished
            if (match.Status == MatchStatus.Finished)
            {
                System.Diagnostics.Debug.WriteLine($"  Match {match.Id} already finished - cannot give bye");
                return false;
            }

            // Determine bye winner
            Player? winner = byeWinner;
            
            if (winner == null)
            {
                // Automatic determination
                if (match.Player1 != null && match.Player2 == null)
                    winner = match.Player1;
                else if (match.Player1 == null && match.Player2 != null)
                    winner = match.Player2;
                else if (match.Player1 != null && match.Player2 != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Both players present in match {match.Id} - manual bye winner required");
                    return false; // Beide Spieler vorhanden - explizite Auswahl nötig
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  No players in match {match.Id} - cannot give bye");
                    return false; // Keine Spieler vorhanden
                }
            }
            else
            {
                // Validate that the specified winner is actually in the match
                if (match.Player1?.Id != winner.Id && match.Player2?.Id != winner.Id)
                {
                    System.Diagnostics.Debug.WriteLine($"  Specified winner {winner.Name} not in match {match.Id}");
                    return false;
                }
            }

            if (winner == null)
            {
                System.Diagnostics.Debug.WriteLine($"  Could not determine winner for bye in match {match.Id}");
                return false;
            }

            // Apply the bye - WICHTIG: Status wird auf Bye gesetzt für korrektes Design
            match.Status = MatchStatus.Bye;
            match.Winner = winner;
            match.Loser = null; // Freilos hat keinen Verlierer
            match.Player1Sets = 0;
            match.Player2Sets = 0;
            match.Player1Legs = 0;
            match.Player2Legs = 0;
            match.EndTime = DateTime.Now;

            System.Diagnostics.Debug.WriteLine($"  Manual bye given to {winner.Name} in match {match.Id} - Status set to Bye");

            // Propagate the bye result
            var winnerBracket = CurrentPhase.WinnerBracket;
            var loserBracket = CurrentPhase.LoserBracket;

            if (match.BracketType == BracketType.Winner)
            {
                PropagateMatchResultWithAutomaticByes(match, winnerBracket, loserBracket);
            }
            else
            {
                PropagateMatchResultWithAutomaticByes(match, loserBracket, null);
            }

            // WICHTIG: UI-Refresh triggern um visuelles Update zu erzwingen
            TriggerUIRefresh();
            System.Diagnostics.Debug.WriteLine($"  UI refresh triggered for manual bye in match {match.Id}");
            
            // ZUSÄTZLICH: Feuere ein spezifisches Event für Datenänderungen
            DataChangedEvent?.Invoke(this, EventArgs.Empty);

            System.Diagnostics.Debug.WriteLine($"=== GiveManualBye SUCCESS for match {match.Id} ===");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GiveManualBye ERROR: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE zum Rückgängigmachen eines Freiloses
    /// </summary>
    /// <param name="match">Das Match, dessen Freilos rückgängig gemacht werden soll</param>
    /// <returns>True wenn erfolgreich, False wenn nicht möglich</returns>
    public bool UndoBye(KnockoutMatch match)
    {
        System.Diagnostics.Debug.WriteLine($"=== UndoBye START for match {match.Id} ===");
        
        try
        {
            if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in knockout phase - cannot undo bye");
                return false;
            }

            // Validation: Match must be a bye
            if (match.Status != MatchStatus.Bye)
            {
                System.Diagnostics.Debug.WriteLine($"  Match {match.Id} is not a bye - cannot undo");
                return false;
            }

            // Check if there are dependent matches that would be affected
            var allMatches = CurrentPhase.WinnerBracket.Concat(CurrentPhase.LoserBracket).ToList();
            var dependentMatches = allMatches.Where(m => 
                (m.SourceMatch1?.Id == match.Id) || (m.SourceMatch2?.Id == match.Id)).ToList();

            bool hasProgressedDependents = dependentMatches.Any(m => 
                m.Status == MatchStatus.Finished || 
                (m.Status == MatchStatus.Bye && m.Winner != null));

            if (hasProgressedDependents)
            {
                System.Diagnostics.Debug.WriteLine($"  Cannot undo bye for match {match.Id} - dependent matches have progressed");
                return false;
            }

            // Reset the match - WICHTIG: Status wird zurück auf NotStarted gesetzt
            match.Status = MatchStatus.NotStarted;
            match.Winner = null;
            match.Loser = null;
            match.Player1Sets = 0;
            match.Player2Sets = 0;
            match.Player1Legs = 0;
            match.Player2Legs = 0;
            match.EndTime = null;

            System.Diagnostics.Debug.WriteLine($"  Bye undone for match {match.Id} - Status reset to NotStarted");

            // Clear dependent matches
            foreach (var dependent in dependentMatches)
            {
                if (dependent.SourceMatch1?.Id == match.Id)
                {
                    dependent.Player1 = null;
                }
                if (dependent.SourceMatch2?.Id == match.Id)
                {
                    dependent.Player2 = null;
                }
            }

            // Re-check for automatic byes after the change
            RefreshAllByeMatches();

            // WICHTIG: Zusätzlicher UI-Refresh um visuelles Update zu erzwingen  
            TriggerUIRefresh();
            System.Diagnostics.Debug.WriteLine($"  Additional UI refresh triggered after undo bye for match {match.Id}");
            
            // ZUSÄTZLICH: Feuere ein spezifisches Event für Datenänderungen
            DataChangedEvent?.Invoke(this, EventArgs.Empty);

            System.Diagnostics.Debug.WriteLine($"=== UndoBye SUCCESS for match {match.Id} ===");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"UndoBye ERROR: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// NEU: ÖFFENTLICHE METHODE zur Validierung von Freilos-Operationen
    /// </summary>
    /// <param name="match">Das zu validierende Match</param>
    /// <returns>Validierungsresultat</returns>
    public ByeValidationResult ValidateByeOperation(KnockoutMatch match)
    {
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return new ByeValidationResult(false, "Nicht in K.O.-Phase", false, false);
        }

        bool canGiveBye = false;
        bool canUndoBye = false;
        string message = "";

        if (match.Status == MatchStatus.Bye)
        {
            // Check if bye can be undone
            var allMatches = CurrentPhase.WinnerBracket.Concat(CurrentPhase.LoserBracket).ToList();
            var dependentMatches = allMatches.Where(m => 
                (m.SourceMatch1?.Id == match.Id) || (m.SourceMatch2?.Id == match.Id)).ToList();

            bool hasProgressedDependents = dependentMatches.Any(m => 
                m.Status == MatchStatus.Finished || 
                (m.Status == MatchStatus.Bye && m.Winner != null));

            canUndoBye = !hasProgressedDependents;
            message = canUndoBye ? "Freilos kann rückgängig gemacht werden" : "Freilos kann nicht rückgängig gemacht werden - nachfolgende Matches bereits gespielt";
        }
        else if (match.Status == MatchStatus.NotStarted)
        {
            // Check if bye can be given
            bool hasPlayers = match.Player1 != null || match.Player2 != null;
            canGiveBye = hasPlayers;
            message = hasPlayers ? "Freilos kann vergeben werden" : "Keine Spieler im Match - Freilos nicht möglich";
        }
        else if (match.Status == MatchStatus.Finished)
        {
            message = "Match bereits beendet - keine Freilos-Operationen möglich";
        }
        else
        {
            message = "Match läuft - keine Freilos-Operationen möglich";
        }

        return new ByeValidationResult(true, message, canGiveBye, canUndoBye);
    }

    /// <summary>
    /// BEISPIEL: Status-Überprüfung für UI-Buttons
    /// </summary>
    /// <param name="matchId">ID des Matches</param>
    /// <returns>UI-Status für Freilos-Buttons</returns>
    public MatchByeUIStatus GetMatchByeUIStatus(int matchId)
    {
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return new MatchByeUIStatus(false, false, "Nicht in K.O.-Phase");
        }

        var allMatches = CurrentPhase.WinnerBracket.Concat(CurrentPhase.LoserBracket).ToList();
        var match = allMatches.FirstOrDefault(m => m.Id == matchId);
        
        if (match == null)
        {
            return new MatchByeUIStatus(false, false, "Match nicht gefunden");
        }

        var validation = ValidateByeOperation(match);
        
        return new MatchByeUIStatus(
            validation.CanGiveBye,
            validation.CanUndoBye,
            validation.Message
        );
    }

    private Player? GetMatchLoser(KnockoutMatch match)
    {
        if (match.Winner == null || match.Status == MatchStatus.Bye) 
            return null;

        if (match.Player1 != null && match.Player2 != null)
        {
            return match.Winner.Id == match.Player1.Id ? match.Player2 : match.Player1;
        }

        return null;
    }

    /// <summary>
    /// Gets Finals matches for the overview display
    /// </summary>
    public ObservableCollection<Match> GetFinalsMatches()
    {
        if (CurrentPhase?.PhaseType != TournamentPhaseType.RoundRobinFinals)
            return new ObservableCollection<Match>();
            
        return CurrentPhase.FinalsGroup?.Matches ?? new ObservableCollection<Match>();
    }

    /// <summary>
    /// NEUE INTERAKTIVE METHODE: Erstellt eine interaktive Turnierbaum-Ansicht
    /// Direkt in das gegebene Canvas-Element eingebettet mit klickbaren Controls
    /// </summary>
    /// <param name="targetCanvas">Das Canvas, in das der Turnierbaum gerendert werden soll</param>
    /// <param name="isLoserBracket">True für Loser Bracket, False für Winner Bracket</param>
    /// <param name="localizationService">Service für Übersetzungen</param>
    /// <returns>Das gerenderte FrameworkElement</returns>
    public FrameworkElement? CreateTournamentTreeView(Canvas targetCanvas, bool isLoserBracket, LocalizationService? localizationService = null)
    {
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            System.Diagnostics.Debug.WriteLine("CreateTournamentTreeView: Not in knockout phase");
            return null;
        }

        System.Diagnostics.Debug.WriteLine($"CreateTournamentTreeView: Creating interactive tree for {(isLoserBracket ? "Loser" : "Winner")} Bracket");

        try
        {
            targetCanvas.Children.Clear();

            var matches = isLoserBracket ? CurrentPhase.LoserBracket.ToList() : CurrentPhase.WinnerBracket.ToList();

            if (matches.Count == 0)
            {
                CreateEmptyBracketMessage(targetCanvas, isLoserBracket, localizationService);
                return targetCanvas;
            }

            // Set canvas properties
            targetCanvas.Background = System.Windows.Media.Brushes.White; // Weißer Hintergrund anstatt Gradient
            targetCanvas.MinWidth = 1200;
            targetCanvas.MinHeight = 800;

            // Add title
            CreateBracketTitle(targetCanvas, isLoserBracket, localizationService);

            // Group matches by round for layout
            var matchesByRound = matches
                .GroupBy(m => m.Round)
                .OrderBy(g => GetRoundOrderValue(g.Key, isLoserBracket))
                .ToList();

            double roundWidth = 250;
            double matchHeight = 80;
            double matchSpacing = 30;
            double roundSpacing = 60;

            // Create interactive match controls
            for (int roundIndex = 0; roundIndex < matchesByRound.Count; roundIndex++)
            {
                var roundGroup = matchesByRound[roundIndex];
                var roundMatches = roundGroup.OrderBy(m => m.Position).ToList();

                double xPos = roundIndex * (roundWidth + roundSpacing) + 50;
                double startY = 100;

                // Calculate vertical spacing to center matches
                double totalRoundHeight = roundMatches.Count * matchHeight + (roundMatches.Count - 1) * matchSpacing;
                double roundStartY = Math.Max(startY, (targetCanvas.MinHeight - totalRoundHeight) / 2);

                // Create round label
                CreateRoundLabel(targetCanvas, xPos, roundMatches.First(), roundWidth, isLoserBracket, localizationService);

                // Create match controls
                for (int matchIndex = 0; matchIndex < roundMatches.Count; matchIndex++)
                {
                    var match = roundMatches[matchIndex];
                    double yPos = roundStartY + matchIndex * (matchHeight + matchSpacing);

                    var matchControl = CreateInteractiveMatchControl(match, roundWidth - 30, matchHeight - 20, localizationService);
                    Canvas.SetLeft(matchControl, xPos);
                    Canvas.SetTop(matchControl, yPos);
                    targetCanvas.Children.Add(matchControl);

                    // Verbindungslinien entfernt - keine CreateConnectionLine mehr
                }
            }

            // Adjust canvas size based on content
            targetCanvas.Width = Math.Max(1200, matchesByRound.Count * (roundWidth + roundSpacing) + 200);
            targetCanvas.Height = Math.Max(800, matchesByRound.Max(r => r.Count()) * (matchHeight + matchSpacing) + 400);
            targetCanvas.MinWidth = targetCanvas.Width;
            targetCanvas.MinHeight = targetCanvas.Height;

            System.Diagnostics.Debug.WriteLine($"CreateTournamentTreeView: Successfully created interactive tree with {matches.Count} matches");
            return targetCanvas;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateTournamentTreeView: ERROR: {ex.Message}");
            return null;
        }
    }

    private void CreateEmptyBracketMessage(Canvas canvas, bool isLoserBracket, LocalizationService? localizationService)
    {
        var messagePanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var icon = new TextBlock
        {
            Text = isLoserBracket ? "🥈" : "🏆",
            FontSize = 60,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 30)
        };

        var messageText = new TextBlock
        {
            Text = isLoserBracket 
                ? (localizationService?.GetString("NoLoserBracketMatches") ?? "Keine Loser Bracket Spiele vorhanden")
                : (localizationService?.GetString("NoWinnerBracketMatches") ?? "Keine Winner Bracket Spiele vorhanden"),
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.DarkGray
        };

        var subText = new TextBlock
        {
            Text = localizationService?.GetString("TournamentTreeWillShow") ?? "Der Turnierbaum wird angezeigt sobald die KO-Phase beginnt",
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.Gray,
            Margin = new Thickness(0, 20, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400
        };

        messagePanel.Children.Add(icon);
        messagePanel.Children.Add(messageText);
        messagePanel.Children.Add(subText);

        Canvas.SetLeft(messagePanel, 350);
        Canvas.SetTop(messagePanel, 300);
        canvas.Children.Add(messagePanel);

        canvas.Background = System.Windows.Media.Brushes.White; // Weißer Hintergrund anstatt Gradient
    }

    private void CreateBracketTitle(Canvas canvas, bool isLoserBracket, LocalizationService? localizationService)
    {
        var titleText = new TextBlock
        {
            Text = isLoserBracket 
                ? (localizationService?.GetString("LoserBracket") ?? "🥈 Loser Bracket")
                : (localizationService?.GetString("WinnerBracket") ?? "🏆 Winner Bracket"),
            FontSize = 32,
            FontWeight = FontWeights.Bold,
            Foreground = isLoserBracket 
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 92, 92))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 139, 34)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Gray,
                Direction = 315,
                ShadowDepth = 2,
                BlurRadius = 4,
                Opacity = 0.5
            }
        };

        Canvas.SetLeft(titleText, 50);
        Canvas.SetTop(titleText, 20);
        canvas.Children.Add(titleText);
    }

    private void CreateRoundLabel(Canvas canvas, double xPos, KnockoutMatch sampleMatch, double roundWidth, bool isLoserBracket, LocalizationService? localizationService)
    {
        var roundLabelBorder = new Border
        {
            Background = isLoserBracket 
                ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 255, 182, 193))
                : new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 144, 238, 144)),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(20, 8, 20, 8),
            BorderBrush = isLoserBracket 
                ? System.Windows.Media.Brushes.IndianRed
                : System.Windows.Media.Brushes.ForestGreen,
            BorderThickness = new Thickness(3),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Gray,
                Direction = 315,
                ShadowDepth = 3,
                BlurRadius = 5,
                Opacity = 0.4
            }
        };

        var roundLabel = new TextBlock
        {
            Text = sampleMatch.RoundDisplay,
            FontWeight = FontWeights.Bold,
            FontSize = 18,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        roundLabelBorder.Child = roundLabel;
        Canvas.SetLeft(roundLabelBorder, xPos + (roundWidth - 180) / 2);
        Canvas.SetTop(roundLabelBorder, 70);
        canvas.Children.Add(roundLabelBorder);
    }

    private Border CreateInteractiveMatchControl(KnockoutMatch match, double width, double height, LocalizationService? localizationService)
    {
        var border = new Border
        {
            Width = width,
            Height = height,
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(5),
            Cursor = System.Windows.Input.Cursors.Hand,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Colors.Gray,
                Direction = 315,
                ShadowDepth = 4,
                BlurRadius = 6,
                Opacity = 0.6
            }
        };

        // Set background and border based on match status
        SetMatchControlAppearance(border, match);

        // Create content grid mit WENIGER Zeilen (Match ID entfernt)
        var contentGrid = new Grid
        {
            Background = System.Windows.Media.Brushes.Transparent 
        };
        // NUR 2 Zeilen: Spieler-Bereich und Score/Status DC
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Spieler bekommen mehr Platz
        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Score bleibt unten

        // NEUER ANSATZ: Grid statt StackPanel für bessere Kontrolle über das Layout
        var playersGrid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8) // Mehr Margins da mehr Platz vorhanden
        };

        // Grid-Definitionen: 3 Spalten für Player1, vs, Player2 (statt Zeilen)
        playersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Player1 - nimmt verfügbaren Platz
        playersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // "vs" - nur so breit wie nötig
        playersGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Player2 - nimmt verfügbaren Platz
                                                                                                                  // Player 1 LINKS positioniert - in Spalte 0
        var player1Text = new TextBlock
        {
            Text = !string.IsNullOrEmpty(match.Player1?.Name) ? match.Player1.Name : "TBD",
            FontSize = 14,
            FontWeight = match.Winner?.Id == match.Player1?.Id ? FontWeights.Bold : FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Left,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player1?.Id
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 100, 0))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)),
            Margin = new Thickness(10, 2, 2, 0),
            TextAlignment = TextAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(player1Text, 0); // Spalte 0 statt Zeile 0

        var vsText = new TextBlock
        {
            Text = "vs",
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontStyle = FontStyles.Italic,
            FontWeight = FontWeights.Normal,
            Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 128, 128)),
            Margin = new Thickness(8, 0, 8, 0), // Horizontaler Abstand statt vertikaler
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(vsText, 1); // Spalte 1 statt Zeile 1

        // Player 2 RECHTS positioniert - in Spalte 2
        var player2Text = new TextBlock
        {
            Text = !string.IsNullOrEmpty(match.Player2?.Name) ? match.Player2.Name : "TBD",
            FontSize = 14,
            FontWeight = match.Winner?.Id == match.Player2?.Id ? FontWeights.Bold : FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Right,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Foreground = match.Winner?.Id == match.Player2?.Id
                ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 100, 0))
                : new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)),
            Margin = new Thickness(2, 2, 10, 0),
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(player2Text, 2); // Spalte 2 statt Zeile 2


        //// Player 1 LINKS positioniert - in Zeile 0 - GRÖSSERE SCHRIFT da mehr Platz
        //var player1Text = new TextBlock
        //{
        //    Text = !string.IsNullOrEmpty(match.Player1?.Name) ? match.Player1.Name : "TBD",
        //   // Text = "TBD", // Sicherstellen, dass es nie null ist
        //    FontSize = 14, // Größere Schrift da Match ID weg ist
        //    FontWeight = match.Winner?.Id == match.Player1?.Id ? FontWeights.Bold : FontWeights.Medium,
        //   // FontWeight = FontWeights.Medium,
        //    HorizontalAlignment = HorizontalAlignment.Left, 
        //    TextTrimming = TextTrimming.CharacterEllipsis,
        //    MaxWidth = width - 20, 
        //    Foreground = match.Winner?.Id == match.Player1?.Id 
        //        ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 100, 0)) 
        //        : new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)), 
        //    Margin = new Thickness(2, 2, 2, 1), // Mehr Margins für bessere Verteilung
        //    Background = System.Windows.Media.Brushes.Transparent,
        //   // Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)), // Standardfarbe
        //    TextAlignment = TextAlignment.Left
        //};
        //Grid.SetRow(player1Text, 0);

        //var vsText = new TextBlock
        //{
        //    Text = "vs", 
        //    FontSize = 11, // Auch größer da mehr Platz vorhanden
        //    HorizontalAlignment = HorizontalAlignment.Center, 
        //    FontStyle = FontStyles.Italic,
        //    FontWeight = FontWeights.Normal, 
        //    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(128, 128, 128)), 
        //    Margin = new Thickness(0, 1, 0, 1), // Etwas mehr Margin für bessere Sichtbarkeit
        //    TextAlignment = TextAlignment.Center
        //};
        //Grid.SetRow(vsText, 1);

        //// Player 2 RECHTS positioniert - in Zeile 2 - GRÖSSERE SCHRIFT da mehr Platz
        //var player2Text = new TextBlock
        //{
        //    Text = !string.IsNullOrEmpty(match.Player2?.Name) ? match.Player2.Name : "TBD",
        //    FontSize = 14, // Größere Schrift da Match ID weg ist
        //    FontWeight = match.Winner?.Id == match.Player2?.Id ? FontWeights.Bold : FontWeights.Medium,
        //    HorizontalAlignment = HorizontalAlignment.Right, 
        //    TextTrimming = TextTrimming.CharacterEllipsis,
        //    MaxWidth = width - 20, 
        //    Foreground = match.Winner?.Id == match.Player2?.Id 
        //        ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 100, 0)) 
        //        : new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)), 
        //    Margin = new Thickness(2, 1, 2, 2), // Mehr vertikale Margins für bessere Verteilung
        //    Background = System.Windows.Media.Brushes.Transparent,
        //    TextAlignment = TextAlignment.Right
        //};
        //Grid.SetRow(player2Text, 2);

        playersGrid.Children.Add(player1Text);
        playersGrid.Children.Add(vsText);
        playersGrid.Children.Add(player2Text);

        // DEBUGGING: Vergewissere dich, dass die Children hinzugefügt wurden
        //System.Diagnostics.Debug.WriteLine($"CreateInteractiveMatchControl: Match {match.Id} - OHNE Match ID Header");
        //System.Diagnostics.Debug.WriteLine($"  - PlayersGrid mit {playersGrid.Children.Count} children, größere Schrift (14px/11px)");
        //System.Diagnostics.Debug.WriteLine($"  - Player1: '{player1Text.Text}', Player2: '{player2Text.Text}', vs: '{vsText.Text}'");

        Grid.SetRow(playersGrid, 0); // Spieler bekommen die erste (größere) Zeile
        contentGrid.Children.Add(playersGrid);

        // Score/Status area - jetzt in Zeile 1 statt 2
        var scorePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6) // Mehr Margins für bessere Optik
        };

        // Score - auch etwas größer da mehr Platz
        var scoreText = new TextBlock
        {
            Text = match.Status == MatchStatus.NotStarted ? "--:--" : match.ScoreDisplay,
            FontSize = 13, // Etwas größer
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkBlue,
            Margin = new Thickness(0, 0, 10, 0) // Mehr Abstand zum Status-Indikator
        };

        // Status indicator - auch etwas größer
        var statusIndicator = new Ellipse
        {
            Width = 10, // Größer
            Height = 10, // Größer
            Margin = new Thickness(4, 0, 0, 0)
        };

        statusIndicator.Fill = match.Status switch
        {
            MatchStatus.NotStarted => System.Windows.Media.Brushes.Gray,
            MatchStatus.InProgress => System.Windows.Media.Brushes.Orange,
            MatchStatus.Finished => System.Windows.Media.Brushes.Green,
            MatchStatus.Bye => System.Windows.Media.Brushes.RoyalBlue,
            _ => System.Windows.Media.Brushes.Gray
        };

        scorePanel.Children.Add(scoreText);
        scorePanel.Children.Add(statusIndicator);

        Grid.SetRow(scorePanel, 1); // Jetzt Zeile 1 statt 2
        contentGrid.Children.Add(scorePanel);

        // WICHTIG: Das contentGrid muss dem border hinzugefügt werden!
        border.Child = contentGrid;

        //System.Diagnostics.Debug.WriteLine($"CreateInteractiveMatchControl: Match {match.Id} layout complete - MEHR PLATZ für Namen!");
        //System.Diagnostics.Debug.WriteLine($"  - ContentGrid: {contentGrid.Children.Count} children (ohne Match ID header)");
        //System.Diagnostics.Debug.WriteLine($"  - PlayersGrid: {playersGrid.Children.Count} text children mit 14px/11px Schrift");
        //System.Diagnostics.Debug.WriteLine($"  - ScorePanel in Zeile 1 mit größeren Elementen");

        // Add interactivity
        AddMatchInteractivity(border, match, localizationService);

        // Enhanced tooltip - Match ID bleibt nur im Tooltip
        CreateMatchTooltip(border, match, localizationService);

        return border;
    }

    private void SetMatchControlAppearance(Border border, KnockoutMatch match)
    {
        switch (match.Status)
        {
            case MatchStatus.NotStarted:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250));
                border.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 200, 200));
                border.BorderThickness = new Thickness(2);
                break;
            case MatchStatus.InProgress:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 248, 220));
                border.BorderBrush = System.Windows.Media.Brushes.Orange;
                border.BorderThickness = new Thickness(3);
                break;
            case MatchStatus.Finished:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 255, 240));
                border.BorderBrush = System.Windows.Media.Brushes.Green;
                border.BorderThickness = new Thickness(3);
                break;
            case MatchStatus.Bye:
                border.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 245, 255));
                border.BorderBrush = System.Windows.Media.Brushes.RoyalBlue;
                border.BorderThickness = new Thickness(3);
                break;
            default:
                border.Background = System.Windows.Media.Brushes.White;
                border.BorderBrush = System.Windows.Media.Brushes.Gray;
                border.BorderThickness = new Thickness(2);
                break;
        }
    }

    private void AddMatchInteractivity(Border border, KnockoutMatch match, LocalizationService? localizationService)
    {
        // Mouse enter/leave effects
        border.MouseEnter += (s, e) =>
        {
            var transform = new System.Windows.Media.ScaleTransform(1.05, 1.05);
            border.RenderTransformOrigin = new Point(0.5, 0.5);
            border.RenderTransform = transform;
            
            var dropShadow = (System.Windows.Media.Effects.DropShadowEffect?)border.Effect;
            if (dropShadow != null)
            {
                dropShadow.ShadowDepth = 6;
                dropShadow.BlurRadius = 8;
            }
        };

        border.MouseLeave += (s, e) =>
        {
            border.RenderTransform = null;
            
            var dropShadow = (System.Windows.Media.Effects.DropShadowEffect?)border.Effect;
            if (dropShadow != null)
            {
                dropShadow.ShadowDepth = 4;
                dropShadow.BlurRadius = 6;
            }
        };

        // Double-click to open match result dialog
        border.MouseLeftButtonDown += (s, e) =>
        {
            if (e.ClickCount == 2)
            {
                // WICHTIG: Verwende OpenMatchResultDialog mit rundenspezifischen Regeln
                OpenMatchResultDialog(match, localizationService);
            }
        };

        // Context menu for advanced options
        var contextMenu = CreateMatchContextMenu(match, localizationService);
        border.ContextMenu = contextMenu;
    }

    private void CreateMatchTooltip(Border border, KnockoutMatch match, LocalizationService? localizationService)
    {
        var tooltipText = $"Match {match.Id} - {match.RoundDisplay}\n" +
                         $"Status: {match.StatusDisplay}\n" +
                         $"Spieler 1: {match.Player1?.Name ?? "TBD"}\n" +
                         $"Spieler 2: {match.Player2?.Name ?? "TBD"}";

        if (match.Status == MatchStatus.Finished && match.Winner != null)
        {
            tooltipText += $"\n🏆 Sieger: {match.Winner.Name}";
        }
        else if (match.Status == MatchStatus.Bye && match.Winner != null)
        {
            tooltipText += $"\n🎯 Freilos: {match.Winner.Name}";
        }

        tooltipText += "\n\n🖱️ Doppelklick: Ergebnis eingeben";
        tooltipText += "\n🖱️ Rechtsklick: Weitere Optionen";

        border.ToolTip = tooltipText;
    }

    private ContextMenu CreateMatchContextMenu(KnockoutMatch match, LocalizationService? localizationService)
    {
        var contextMenu = new ContextMenu();

        // Enter/Edit result
        if (match.Status != MatchStatus.Bye && match.Player1 != null && match.Player2 != null)
        {
            var resultMenuItem = new MenuItem
            {
                Header = match.Status == MatchStatus.Finished 
                    ? (localizationService?.GetString("EditResult") ?? "Ergebnis bearbeiten")
                    : (localizationService?.GetString("EnterResult") ?? "Ergebnis eingeben"),
                Icon = new TextBlock { Text = "📝", FontSize = 12 }
            };
            // WICHTIG: Verwende OpenMatchResultDialog mit rundenspezifischen Regeln
            resultMenuItem.Click += (s, e) => OpenMatchResultDialog(match, localizationService);
            contextMenu.Items.Add(resultMenuItem);
        }

        // Bye options
        if (match.Status == MatchStatus.NotStarted || match.Status == MatchStatus.Bye)
        {
            var validation = ValidateByeOperation(match);
            
            if (validation.CanGiveBye)
            {
                // Give bye to specific player
                if (match.Player1 != null)
                {
                    var byePlayer1 = new MenuItem
                    {
                        Header = $"Freilos an {match.Player1.Name}",
                        Icon = new TextBlock { Text = "🎯", FontSize = 12 }
                    };
                    byePlayer1.Click += (s, e) => GiveManualBye(match, match.Player1);
                    contextMenu.Items.Add(byePlayer1);
                }

                if (match.Player2 != null)
                {
                    var byePlayer2 = new MenuItem
                    {
                        Header = $"Freilos an {match.Player2.Name}",
                        Icon = new TextBlock { Text = "🎯", FontSize = 12 }
                    };
                    byePlayer2.Click += (s, e) => GiveManualBye(match, match.Player2);
                    contextMenu.Items.Add(byePlayer2);
                }

                // Auto bye
                var autoBye = new MenuItem
                {
                    Header = "Automatisches Freilos",
                    Icon = new TextBlock { Text = "🤖", FontSize = 12 }
                };
                autoBye.Click += (s, e) => GiveManualBye(match, null);
                contextMenu.Items.Add(autoBye);
            }

            if (validation.CanUndoBye)
            {
                var undoBye = new MenuItem
                {
                    Header = "Freilos rückgängigmachen",
                    Icon = new TextBlock { Text = "↩️", FontSize = 12 }
                };
                undoBye.Click += (s, e) => UndoBye(match);
                contextMenu.Items.Add(undoBye);
            }
        }

        // Show info if no actions possible
        if (contextMenu.Items.Count == 0)
        {
            var noActionItem = new MenuItem
            {
                Header = "Keine Aktionen verfügbar",
                IsEnabled = false,
                Icon = new TextBlock { Text = "ℹ️", FontSize = 12 }
            };
            contextMenu.Items.Add(noActionItem);
        }

        return contextMenu;
    }

    private void OpenMatchResultDialog(KnockoutMatch match, LocalizationService? localizationService)
    {
        if (match.Player1 == null || match.Player2 == null || match.Status == MatchStatus.Bye)
            return;

        try
        {
            // WICHTIG: Verwende rundenspezifische Regeln für KO-Matches
            var roundRules = GameRules.GetRulesForRound(match.Round);
            
            System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog: Match {match.Id} in {match.Round}");
            System.Diagnostics.Debug.WriteLine($"  Round Rules: SetsToWin={roundRules.SetsToWin}, LegsToWin={roundRules.LegsToWin}, LegsPerSet={roundRules.LegsPerSet}");
            System.Diagnostics.Debug.WriteLine($"  Using SPECIALIZED constructor for KnockoutMatch");

            // KORREKTUR: Verwende den spezialisierten Constructor für KnockoutMatches
            var resultWindow = new MatchResultWindow(match, roundRules, GameRules, localizationService);
            
            // Try to find parent window
            var parentWindow = Application.Current.MainWindow;
            if (parentWindow != null)
            {
                resultWindow.Owner = parentWindow;
            }

            if (resultWindow.ShowDialog() == true)
            {
                var internalMatch = resultWindow.InternalMatch;
                match.Player1Sets = internalMatch.Player1Sets;
                match.Player2Sets = internalMatch.Player2Sets;
                match.Player1Legs = internalMatch.Player1Legs;
                match.Player2Legs = internalMatch.Player2Legs;
                match.Winner = internalMatch.Winner;
                match.Status = internalMatch.Status;
                match.Notes = internalMatch.Notes;
                match.StartTime = internalMatch.StartTime;
                match.EndTime = DateTime.Now;

                System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog: Match {match.Id} result saved");
                System.Diagnostics.Debug.WriteLine($"  Winner: {match.Winner?.Name}, Sets: {match.Player1Sets}:{match.Player2Sets}, Legs: {match.Player1Legs}:{match.Player2Legs}");

                // Process the result through the tournament system
                ProcessMatchResult(match);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OpenMatchResultDialog: ERROR: {ex.Message}");
            MessageBox.Show($"Fehler beim Öffnen des Ergebnis-Fensters: {ex.Message}", 
                           "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private int GetRoundOrderValue(KnockoutRound round, bool isLoserBracket)
    {
        if (isLoserBracket)
        {
            return round switch
            {
                KnockoutRound.LoserRound1 => 1,
                KnockoutRound.LoserRound2 => 2,
                KnockoutRound.LoserRound3 => 3,
                KnockoutRound.LoserRound4 => 4,
                KnockoutRound.LoserRound5 => 5,
                KnockoutRound.LoserRound6 => 6,
                KnockoutRound.LoserRound7 => 7,
                KnockoutRound.LoserRound8 => 8,
                KnockoutRound.LoserRound9 => 9,
                KnockoutRound.LoserRound10 => 10,
                KnockoutRound.LoserRound11 => 11,
                KnockoutRound.LoserRound12 => 12,
                KnockoutRound.LoserFinal => 13,
                _ => 99
            };
        }
        else
        {
            return round switch
            {
                KnockoutRound.Best64 => 1,
                KnockoutRound.Best32 => 2,
                KnockoutRound.Best16 => 3,
                KnockoutRound.Quarterfinal => 4,
                KnockoutRound.Semifinal => 5,
                KnockoutRound.Final => 6,
                KnockoutRound.GrandFinal => 7,
                _ => 99
            };
        }
    }

    /// <summary>
    /// NEU: Löst ein UI-Refresh-Event aus, um ViewModels zu aktualisieren
    /// VERBESSERT: Zusätzliche Infos für UI-Updates bei Freilos-Änderungen
    /// </summary>
    public void TriggerUIRefresh()
    {
        System.Diagnostics.Debug.WriteLine($"TriggerUIRefresh: Firing UIRefreshRequested event");
        UIRefreshRequested?.Invoke(this, EventArgs.Empty);
        
        // ZUSÄTZLICH: Feuere ein spezifisches Event für Datenänderungen
        DataChangedEvent?.Invoke(this, EventArgs.Empty);

        // WICHTIG: Zusätzlich die PropertyChanged für Bindings feuern
        OnPropertyChanged(nameof(CurrentPhase));
    }

    /// <summary>
    /// NEU: Spezifischer Refresh für Match-Status-Änderungen
    /// Dieser sollte aufgerufen werden wenn sich der Status einzelner Matches ändert
    /// </summary>
    /// <param name="matchId">ID des geänderten Matches</param>
    /// <param name="newStatus">Der neue Status des Matches</param>
    public void TriggerMatchStatusRefresh(int matchId, MatchStatus newStatus)
    {
        System.Diagnostics.Debug.WriteLine($"TriggerMatchStatusRefresh: Match {matchId} changed to {newStatus}");
        
        // Standard UI-Refresh
        TriggerUIRefresh();
        
        // Zusätzliches Event für spezifische Match-Updates
        MatchStatusChanged?.Invoke(this, new MatchStatusChangedEventArgs(matchId, newStatus));
    }

    /// <summary>
    /// NEU: Event für Datenänderungen - wird gefeuert wenn Match-Ergebnisse eingegeben oder Freilose vergeben werden
    /// </summary>
    public event EventHandler? DataChangedEvent;

    /// <summary>
    /// NEU: Event für UI-Refreshs nach automatischen Freilos-Änderungen
    /// </summary>
    public event EventHandler? UIRefreshRequested;

    /// <summary>
    /// NEU: Event für spezifische Match-Status-Änderungen
    /// </summary>
    public event EventHandler<MatchStatusChangedEventArgs>? MatchStatusChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{Name} - {GameRules}";
    }

    /// <summary>
    /// ÖFFENTLICHE METHODE: Für normale Match-Ergebnis-Progression (nicht für Freilose)
    /// Diese Methode sollte verwendet werden, wenn ein Match über die UI beendet wird
    /// </summary>
    /// <param name="completedMatch">Das beendete Match</param>
    /// <returns>True wenn erfolgreich</returns>
    public bool ProcessMatchResult(KnockoutMatch completedMatch)
    {
        System.Diagnostics.Debug.WriteLine($"=== ProcessMatchResult START for match {completedMatch.Id} ===");
        
        try
        {
            if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine($"  Not in knockout phase - cannot process match result");
                return false;
            }

            if (completedMatch.Winner == null || completedMatch.Status != MatchStatus.Finished)
            {
                System.Diagnostics.Debug.WriteLine($"  Match {completedMatch.Id} not finished or no winner - cannot process");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"  Processing match {completedMatch.Id} - Winner: {completedMatch.Winner.Name}");

            // Setze den Verlierer
            if (completedMatch.Player1 != null && completedMatch.Player2 != null)
            {
                completedMatch.Loser = completedMatch.Winner.Id == completedMatch.Player1.Id ? completedMatch.Player2 : completedMatch.Player1;
            }

            // WICHTIG: DIREKTE Spieler-Propagation verwenden statt externe Methoden!
            var winnerBracket = CurrentPhase.WinnerBracket;
            var loserBracket = CurrentPhase.LoserBracket;
            
            System.Diagnostics.Debug.WriteLine($"  DIRECT propagation: Using PropagateMatchResultWithAutomaticByes");
            
            // Verwende UNSERE eigene Propagations-Logik die definitiv funktioniert
            if (completedMatch.BracketType == BracketType.Winner)
            {
                PropagateMatchResultWithAutomaticByes(completedMatch, winnerBracket, loserBracket);
            }
            else
            {
                PropagateMatchResultWithAutomaticByes(completedMatch, loserBracket, null);
            }

            // WICHTIG: Trigger UI refresh UND Data Changed Event
            TriggerUIRefresh();
            
            // ZUSÄTZLICH: Feuere ein spezifisches Event für Datenänderungen
            DataChangedEvent?.Invoke(this, EventArgs.Empty);

            System.Diagnostics.Debug.WriteLine($"=== ProcessMatchResult SUCCESS for match {completedMatch.Id} ===");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ProcessMatchResult ERROR: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets Winner Bracket matches for the overview display
    /// </summary>
    public ObservableCollection<KnockoutMatch> GetWinnerBracketMatches()
    {
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return new ObservableCollection<KnockoutMatch>();
        }
            
        return CurrentPhase.WinnerBracket ?? new ObservableCollection<KnockoutMatch>();
    }
    
    /// <summary>
    /// Gets Loser Bracket matches for the overview display
    /// </summary>
    public ObservableCollection<KnockoutMatch> GetLoserBracketMatches()
    {
        if (CurrentPhase?.PhaseType != TournamentPhaseType.KnockoutPhase)
        {
            return new ObservableCollection<KnockoutMatch>();
        }
            
        return CurrentPhase.LoserBracket ?? new ObservableCollection<KnockoutMatch>();
    }

    #endregion
}
/// <summary>
/// Ergebnis der Validierung von Freilos-Operationen
/// </summary>
public class ByeValidationResult
{
    public bool IsValid { get; }
    public string Message { get; }
    public bool CanGiveBye { get; }
    public bool CanUndoBye { get; }

    public ByeValidationResult(bool isValid, string message, bool canGiveBye, bool canUndoBye)
    {
        IsValid = isValid;
        Message = message;
        CanGiveBye = canGiveBye;
        CanUndoBye = canUndoBye;
    }
}

/// <summary>
/// UI-Status für Freilos-Buttons
/// </summary>
public class MatchByeUIStatus
{
    public bool ShowGiveByeButton { get; }
    public bool ShowUndoByeButton { get; }
    public string StatusMessage { get; }

    public MatchByeUIStatus(bool showGiveByeButton, bool showUndoByeButton, string statusMessage)
    {
        ShowGiveByeButton = showGiveByeButton;
        ShowUndoByeButton = showUndoByeButton;
        StatusMessage = statusMessage;
    }
}

/// <summary>
/// Event-Argumente für Match-Status-Änderungen
/// </summary>
public class MatchStatusChangedEventArgs : EventArgs
{
    public int MatchId { get; }
    public MatchStatus NewStatus { get; }

    public MatchStatusChangedEventArgs(int matchId, MatchStatus newStatus)
    {
        MatchId = matchId;
        NewStatus = newStatus;
    }
}
