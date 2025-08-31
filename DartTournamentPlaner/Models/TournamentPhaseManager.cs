using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Verwaltet die Turnierphasen und deren �berg�nge
/// Verantwortlich f�r die Logik der Phasenwechsel und Phasenvalidierung
/// </summary>
public class TournamentPhaseManager
{
    private readonly TournamentClass _tournament;

    public TournamentPhaseManager(TournamentClass tournament)
    {
        _tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
    }

    /// <summary>
    /// Pr�ft ob zur n�chsten Phase gewechselt werden kann
    /// KORRIGIERT: Delegiert die Pr�fung an die aktuelle Phase UND pr�ft ob es �berhaupt eine n�chste Phase gibt
    /// </summary>
    /// <returns>True wenn alle Voraussetzungen f�r den Phasenwechsel erf�llt sind</returns>
    public bool CanProceedToNextPhase()
    {
        try
        {
            //System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase START ===");
            //System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: TournamentClass = {_tournament.Name}");
            //System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: CurrentPhase = {_tournament.CurrentPhase?.PhaseType}");
            //System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: PostGroupPhaseMode = {_tournament.GameRules.PostGroupPhaseMode}");
            
            // KORREKTUR: Pr�fe erst ob �berhaupt eine n�chste Phase existiert
            var nextPhase = GetNextPhase();
            if (nextPhase == null)
            {
                //System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: No next phase available - tournament ends here");
                //System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase END (FALSE - no next phase) ===");
                return false;
            }
            
            // KORREKTUR: Dann pr�fe ob die aktuelle Phase bereit f�r den �bergang ist
            var currentPhaseReady = _tournament.CurrentPhase?.CanProceedToNextPhase() ?? false;
            //System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Current phase ready = {currentPhaseReady}");
            //System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Next phase available = {nextPhase.PhaseType}");
            
            var result = currentPhaseReady;
            //System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Final result = {result}");
            //System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase END ===");
            
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
    /// Ermittelt die n�chste Phase basierend auf der aktuellen Phase und den Spielregeln
    /// Implementiert die Turnierlogik f�r verschiedene Modi (nur Gruppen, Finals, K.O.)
    /// KORRIGIERT: Bessere Debugging-Ausgabe und korrekte PostGroupPhaseMode-Behandlung
    /// </summary>
    /// <returns>Die n�chste Phase oder null wenn das Turnier beendet ist</returns>
    public TournamentPhase? GetNextPhase()
    {
        try
        {
            //System.Diagnostics.Debug.WriteLine($"=== GetNextPhase START ===");
            
            if (_tournament.CurrentPhase == null) 
            {
                //System.Diagnostics.Debug.WriteLine($"GetNextPhase: CurrentPhase is null");
                return null;
            }

            //System.Diagnostics.Debug.WriteLine($"GetNextPhase: Current phase = {_tournament.CurrentPhase.PhaseType}");
            //System.Diagnostics.Debug.WriteLine($"GetNextPhase: PostGroupPhaseMode = {_tournament.GameRules.PostGroupPhaseMode}");

            // KORRIGIERTE Logik: Bestimme n�chste Phase basierend auf aktueller Phase und Spielregeln
            TournamentPhase? nextPhase = _tournament.CurrentPhase.PhaseType switch
            {
                TournamentPhaseType.GroupPhase => _tournament.GameRules.PostGroupPhaseMode switch
                {
                    PostGroupPhaseMode.RoundRobinFinals => CreateRoundRobinFinalsPhase(),
                    PostGroupPhaseMode.KnockoutBracket => CreateKnockoutPhase(),
                    PostGroupPhaseMode.None => null, // Nur Gruppenphase - Turnier endet hier
                    _ => null
                },

                TournamentPhaseType.RoundRobinFinals => _tournament.GameRules.PostGroupPhaseMode switch
                {
                    // KORRIGIERT: Nach Finals kann noch K.O. kommen wenn beide Modi aktiv sind
                    PostGroupPhaseMode.KnockoutBracket => CreateKnockoutPhase(),
                    _ => null // Finals waren letzte Phase
                },

                TournamentPhaseType.KnockoutPhase => null, // K.O.-Phase ist immer die letzte Phase

                _ => null // Unbekannte Phase
            };
            
            //System.Diagnostics.Debug.WriteLine($"GetNextPhase: Next phase = {nextPhase?.PhaseType.ToString() ?? "null"}");
            //System.Diagnostics.Debug.WriteLine($"=== GetNextPhase END ===");
            
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
    /// F�hrt den Wechsel zur n�chsten Phase durch
    /// Markiert die aktuelle Phase als abgeschlossen und aktiviert die n�chste Phase
    /// </summary>
    public void AdvanceToNextPhase()
    {
        // Pr�fung ob Phasenwechsel m�glich ist
        if (!CanProceedToNextPhase()) return;

        // Ermittlung der n�chsten Phase
        var nextPhase = GetNextPhase();
        if (nextPhase == null) return;

        // Markiere aktuelle Phase als abgeschlossen
        if (_tournament.CurrentPhase != null)
        {
            _tournament.CurrentPhase.IsActive = false;
            _tournament.CurrentPhase.IsCompleted = true;
        }

        // F�ge neue Phase hinzu und aktiviere sie
        _tournament.Phases.Add(nextPhase);
        _tournament.CurrentPhase = nextPhase;
        nextPhase.IsActive = true;
    }

    /// <summary>
    /// Erstellt eine Round-Robin-Finalrunde mit den qualifizierten Spielern aus der Gruppenphase
    /// Alle qualifizierten Spieler spielen nochmals jeder gegen jeden
    /// </summary>
    /// <returns>Eine neue TournamentPhase f�r die Finalrunde</returns>
    private TournamentPhase CreateRoundRobinFinalsPhase()
    {
        //System.Diagnostics.Debug.WriteLine($"=== CreateRoundRobinFinalsPhase START ===");
        
        var finalsPhase = new TournamentPhase
        {
            Name = "Finalrunde",
            PhaseType = TournamentPhaseType.RoundRobinFinals
        };

        // Hole qualifizierte Spieler aus der Gruppenphase
        var groupPhase = _tournament.Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        var qualifiedPlayers = groupPhase.GetQualifiedPlayers(_tournament.GameRules.QualifyingPlayersPerGroup);

        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Found {qualifiedPlayers.Count} qualified players");

        // Erstelle Finals-Gruppe
        var finalsGroup = new Group
        {
            Id = 999, // Spezielle ID f�r Finals
            Name = "Finalrunde",
            MatchesGenerated = false
        };

        // F�ge alle qualifizierten Spieler zur Finals-Gruppe hinzu
        foreach (var player in qualifiedPlayers)
        {
            finalsGroup.Players.Add(player);
            //System.Diagnostics.Debug.WriteLine($"  Added player: {player.Name}");
        }

        // KRITISCHER FIX: Generiere die Round Robin Matches f�r die Finals-Gruppe mit korrekten GameRules!
        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Generating Round Robin matches for {finalsGroup.Players.Count} players");
        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Using GameRules - PlayWithSets: {_tournament.GameRules.PlayWithSets}, SetsToWin: {_tournament.GameRules.SetsToWin}, LegsToWin: {_tournament.GameRules.LegsToWin}");
        
        finalsGroup.GenerateRoundRobinMatches(_tournament.GameRules);
        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Generated {finalsGroup.Matches.Count} matches");
        
        // ZUS�TZLICHE VALIDIERUNG: �berpr�fe ob UsesSets korrekt gesetzt wurde UND alle UUIDs vorhanden sind
        foreach (var match in finalsGroup.Matches)
        {
            // Stelle sicher, dass jedes Finals-Match eine g�ltige UUID hat
            match.EnsureUniqueId();
            
            //System.Diagnostics.Debug.WriteLine($"  Finals Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}, UsesSets: {match.UsesSets}, UUID: {match.UniqueId}");
        }

        // WICHTIG: Setze die Finals-Gruppe sowohl in der Phase als auch als QualifiedPlayers
        finalsPhase.FinalsGroup = finalsGroup;
        finalsPhase.QualifiedPlayers = new ObservableCollection<Player>(qualifiedPlayers);

        // ZUS�TZLICH: Trigger UI-Refresh Event f�r sofortige Aktualisierung
        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Triggering UI refresh");
        _tournament.TriggerUIRefresh();

        //System.Diagnostics.Debug.WriteLine($"=== CreateRoundRobinFinalsPhase END - Created phase with {finalsGroup.Matches.Count} matches ===");
        return finalsPhase;
    }

    private TournamentPhase CreateKnockoutPhase()
    {
        try
        {
            //System.Diagnostics.Debug.WriteLine($"=== CreateKnockoutPhase START ===");
            
            var knockoutPhase = new TournamentPhase
            {
                Name = _tournament.GameRules.KnockoutMode == KnockoutMode.DoubleElimination ? "KO-Phase (Double Elimination)" : "KO-Phase (Single Elimination)",
                PhaseType = TournamentPhaseType.KnockoutPhase
            };

            //System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Created phase with name '{knockoutPhase.Name}'");

            // Get qualified players from previous phase
            System.Collections.Generic.List<Player> qualifiedPlayers;
            if (_tournament.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                //System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Getting players from RoundRobinFinals");
                qualifiedPlayers = _tournament.CurrentPhase.GetQualifiedPlayers(int.MaxValue); // All players from finals
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Getting players from GroupPhase");
                var groupPhase = _tournament.Phases.First(p => p.PhaseType == TournamentPhaseType.GroupPhase);
                qualifiedPlayers = groupPhase.GetQualifiedPlayers(_tournament.GameRules.QualifyingPlayersPerGroup);
            }

            //System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Found {qualifiedPlayers.Count} qualified players");
            foreach (var player in qualifiedPlayers)
            {
                //System.Diagnostics.Debug.WriteLine($"  - {player.Name}");
            }

            // WICHTIGE VALIDIERUNG: Pr�fen ob gen�gend Spieler vorhanden
            if (qualifiedPlayers.Count <= 1)
            {
                //System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: ERROR - Not enough players ({qualifiedPlayers.Count})");
                throw new InvalidOperationException($"Nicht gen�gend Spieler f�r K.O.-Phase: {qualifiedPlayers.Count}");
            }

            knockoutPhase.QualifiedPlayers = new ObservableCollection<Player>(qualifiedPlayers);

            // Generate bracket - delegiere an KnockoutBracketGenerator
            var bracketGenerator = new KnockoutBracketGenerator(_tournament);
            
            if (_tournament.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                //System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generating double elimination bracket");
                bracketGenerator.GenerateDoubleEliminationBracket(knockoutPhase, qualifiedPlayers);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generating single elimination bracket");
                bracketGenerator.GenerateSingleEliminationBracket(knockoutPhase, qualifiedPlayers);
            }

            //System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generated {knockoutPhase.WinnerBracket.Count} winner bracket matches");
            //System.Diagnostics.Debug.WriteLine($"=== CreateKnockoutPhase END ===");

            return knockoutPhase;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    /// <summary>
    /// NEUE METHODE: Stellt sicher, dass mindestens eine GroupPhase existiert
    /// Wird nach JSON-Deserialisierung oder bei erstem Zugriff aufgerufen
    /// Implementiert Lazy Initialization um Duplikate zu vermeiden
    /// </summary>
    public void EnsureGroupPhaseExists()
    {
        //System.Diagnostics.Debug.WriteLine($"=== EnsureGroupPhaseExists START for {_tournament.Name} ===");
        //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Current Phases count = {_tournament.Phases.Count}");
        
        // Pr�fe ob bereits eine GroupPhase existiert
        var existingGroupPhase = _tournament.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        
        if (existingGroupPhase == null)
        {
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: No GroupPhase found, creating new one");
            
            // Erstelle eine neue GroupPhase
            var groupPhase = new TournamentPhase
            {
                Name = "Gruppenphase",
                PhaseType = TournamentPhaseType.GroupPhase,
                IsActive = true // Gruppenphase ist standardm��ig aktiv
            };
            
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Created new TournamentPhase with {groupPhase.Groups.Count} groups");
        
            _tournament.Phases.Add(groupPhase);
            _tournament.CurrentPhase = groupPhase;
            
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Created new GroupPhase, total phases = {_tournament.Phases.Count}");
        }
        else
        {
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: GroupPhase already exists, setting as CurrentPhase");
            
            // Setze die existierende GroupPhase als CurrentPhase wenn noch keine gesetzt
            if (_tournament.CurrentPhase == null)
            {
                _tournament.CurrentPhase = existingGroupPhase;
            }
        }

        // WICHTIG: KEINE Groups.Count Aufrufe hier - das w�rde zu Rekursion f�hren!
        //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: CurrentPhase = {_tournament.CurrentPhase?.PhaseType}");
        //System.Diagnostics.Debug.WriteLine($"=== EnsureGroupPhaseExists END ===");
    }

    /// <summary>
    /// NEUE METHODE: Validiert und repariert Finals-Phase nach JSON-Loading
    /// Stellt sicher dass FinalsGroup korrekt initialisiert ist
    /// </summary>
    public void EnsureFinalsPhaseIntegrity()
    {
        //System.Diagnostics.Debug.WriteLine($"=== EnsureFinalsPhaseIntegrity START for {_tournament.Name} ===");
        
        // Suche nach existierender Finals-Phase
        var finalsPhase = _tournament.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals);
        
        if (finalsPhase != null)
        {
            //System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: Finals phase found");
            
            // Pr�fe ob FinalsGroup existiert
            if (finalsPhase.FinalsGroup == null)
            {
                //System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: FinalsGroup is null, attempting to recreate");
                
                // Versuche FinalsGroup aus QualifiedPlayers zu rekonstruieren
                if (finalsPhase.QualifiedPlayers?.Count > 0)
                {
                    //System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: Recreating FinalsGroup from {finalsPhase.QualifiedPlayers.Count} qualified players");
                    
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
                        finalsGroup.GenerateRoundRobinMatches(_tournament.GameRules);
                        //System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: Generated {finalsGroup.Matches.Count} matches");
                    }
                    
                    finalsPhase.FinalsGroup = finalsGroup;
                    
                    //System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: FinalsGroup recreated successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: No qualified players found, cannot recreate FinalsGroup");
                }
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: FinalsGroup exists with {finalsPhase.FinalsGroup.Players.Count} players and {finalsPhase.FinalsGroup.Matches.Count} matches");
                
                // Sicherstelle, dass Matches generiert sind
                if (!finalsPhase.FinalsGroup.MatchesGenerated && finalsPhase.FinalsGroup.Players.Count >= 2)
                {
 //System.Diagnostics.Debug.WriteLine($"EnsureFinalsPhaseIntegrity: Generating missing matches");
                    finalsPhase.FinalsGroup.GenerateRoundRobinMatches(_tournament.GameRules);
                }
            }
        }

        //System.Diagnostics.Debug.WriteLine($"=== EnsureFinalsPhaseIntegrity END ===");
    }

    /// <summary>
    /// NEUE �FFENTLICHE METHODE: F�hrt vollst�ndige Phase-Validierung nach JSON-Loading durch
    /// Sollte aufgerufen werden nachdem Tournament-Daten geladen wurden
    /// </summary>
    public void ValidateAndRepairPhases()
    {
        //System.Diagnostics.Debug.WriteLine($"=== ValidateAndRepairPhases START for {_tournament.Name} ===");
        
        try
        {
            // 1. Stelle sicher dass GroupPhase existiert
            EnsureGroupPhaseExists();
            
            // 2. Repariere Finals-Phase falls vorhanden
            EnsureFinalsPhaseIntegrity();
            
            // 3. WICHTIG: Stelle sicher dass alle Matches g�ltige UUIDs haben
            EnsureAllMatchesHaveUuids();
            
            // 4. Trigger UI-Refresh um sicherzustellen dass alles geladen wird
            _tournament.TriggerUIRefresh();
            
            //System.Diagnostics.Debug.WriteLine($"ValidateAndRepairPhases: All phases validated and repaired");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ValidateAndRepairPhases: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"ValidateAndRepairPhases: Stack trace: {ex.StackTrace}");
        }
        
        //System.Diagnostics.Debug.WriteLine($"=== ValidateAndRepairPhases END ===");
    }

    /// <summary>
    /// NEUE PRIVATE METHODE: Stellt sicher, dass alle Matches in allen Phasen g�ltige UUIDs haben
    /// Diese Methode ruft die entsprechende Methode aus TournamentClass auf
    /// </summary>
    private void EnsureAllMatchesHaveUuids()
    {
        //System.Diagnostics.Debug.WriteLine($"=== EnsureAllMatchesHaveUuids START for {_tournament.Name} ===");
        
        int totalMatches = 0;
        int generatedUuids = 0;
        int validUuids = 0;

        try
        {
            // 1. Pr�fe Gruppen-Matches
            foreach (var group in _tournament.Groups)
            {
                //System.Diagnostics.Debug.WriteLine($"  Checking group '{group.Name}' with {group.Matches.Count} matches");
                
                foreach (var match in group.Matches)
                {
                    totalMatches++;
                    
                    if (!match.HasValidUniqueId())
                    {
                        match.GenerateNewUniqueId();
                        generatedUuids++;
                        //System.Diagnostics.Debug.WriteLine($"    Generated UUID for Group Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name} -> {match.UniqueId}");
                    }
                    else
                    {
                        validUuids++;
                        //System.Diagnostics.Debug.WriteLine($"    Group Match {match.Id} already has valid UUID: {match.UniqueId}");
                    }
                }
            }

            // 2. Pr�fe Finals-Matches
            var finalsPhase = _tournament.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals);
            if (finalsPhase?.FinalsGroup?.Matches != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Checking Finals matches: {finalsPhase.FinalsGroup.Matches.Count} matches");
                
                foreach (var match in finalsPhase.FinalsGroup.Matches)
                {
                    totalMatches++;
                    
                    if (!match.HasValidUniqueId())
                    {
                        match.GenerateNewUniqueId();
                        generatedUuids++;
                        //System.Diagnostics.Debug.WriteLine($"    Generated UUID for Finals Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name} -> {match.UniqueId}");
                    }
                    else
                    {
                        validUuids++;
                        //System.Diagnostics.Debug.WriteLine($"    Finals Match {match.Id} already has valid UUID: {match.UniqueId}");
                    }
                }
            }

            // 3. Pr�fe K.O.-Matches
            var koPhase = _tournament.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.KnockoutPhase);
            if (koPhase != null)
            {
                // Winner Bracket
                if (koPhase.WinnerBracket != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"  Checking Winner Bracket matches: {koPhase.WinnerBracket.Count} matches");
                    
                    foreach (var match in koPhase.WinnerBracket)
                    {
                        totalMatches++;
                        
                        if (!match.HasValidUniqueId())
                        {
                            match.GenerateNewUniqueId();
                            generatedUuids++;
                            //System.Diagnostics.Debug.WriteLine($"    Generated UUID for WB Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name} ({match.Round}) -> {match.UniqueId}");
                        }
                        else
                        {
                            validUuids++;
                            //System.Diagnostics.Debug.WriteLine($"    WB Match {match.Id} already has valid UUID: {match.UniqueId}");
                        }
                    }
                }

                // Loser Bracket
                if (koPhase.LoserBracket != null)
                {
                    //System.Diagnostics.Debug.WriteLine($"  Checking Loser Bracket matches: {koPhase.LoserBracket.Count} matches");
                    
                    foreach (var match in koPhase.LoserBracket)
                    {
                        totalMatches++;
                        
                        if (!match.HasValidUniqueId())
                        {
                            match.GenerateNewUniqueId();
                            generatedUuids++;
                            //System.Diagnostics.Debug.WriteLine($"    Generated UUID for LB Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name} ({match.Round}) -> {match.UniqueId}");
                        }
                        else
                        {
                            validUuids++;
                            //System.Diagnostics.Debug.WriteLine($"    LB Match {match.Id} already has valid UUID: {match.UniqueId}");
                        }
                    }
                }
            }

            //System.Diagnostics.Debug.WriteLine($"? UUID Check completed for {_tournament.Name}:");
            //System.Diagnostics.Debug.WriteLine($"   ?? Total matches processed: {totalMatches}");
            //System.Diagnostics.Debug.WriteLine($"   ? Matches with valid UUIDs: {validUuids}");
            //System.Diagnostics.Debug.WriteLine($"   ?? Generated new UUIDs: {generatedUuids}");

            // Trigger UI refresh wenn UUIDs generiert wurden
            if (generatedUuids > 0)
            {
                _tournament.TriggerUIRefresh();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EnsureAllMatchesHaveUuids: ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"EnsureAllMatchesHaveUuids: Stack trace: {ex.StackTrace}");
        }
        
        //System.Diagnostics.Debug.WriteLine($"=== EnsureAllMatchesHaveUuids END ===");
    }
}