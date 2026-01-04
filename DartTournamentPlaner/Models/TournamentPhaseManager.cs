using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Verwaltet die Turnierphasen und deren Übergänge
/// Verantwortlich für die Logik der Phasenwechsel und Phasenvalidierung
/// </summary>
public class TournamentPhaseManager
{
    private readonly TournamentClass _tournament;

    public TournamentPhaseManager(TournamentClass tournament)
    {
        _tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
    }

    /// <summary>
    /// Prüft ob zur nächsten Phase gewechselt werden kann
    /// KORRIGIERT: Delegiert die Prüfung an die aktuelle Phase UND prüft ob es überhaupt eine nächste Phase gibt
    /// ? ERWEITERT: Berücksichtigt SkipGroupPhase Modus
    /// </summary>
    /// <returns>True wenn alle Voraussetzungen für den Phasenwechsel erfüllt sind</returns>
    public bool CanProceedToNextPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase START ===");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: TournamentClass = {_tournament.Name}");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: CurrentPhase = {_tournament.CurrentPhase?.PhaseType}");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: PostGroupPhaseMode = {_tournament.GameRules.PostGroupPhaseMode}");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: SkipGroupPhase = {_tournament.GameRules.SkipGroupPhase}");
            
            // ? WICHTIG: Bei SkipGroupPhase + KO-Phase gibt es keine nächste Phase!
            if (_tournament.GameRules.SkipGroupPhase && _tournament.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine("CanProceedToNextPhase: SkipGroupPhase + KO-Phase - no next phase possible");
                System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase END (FALSE - SkipGroupPhase KO) ===");
                return false;
            }
            
            // KORREKTUR: Prüfe erst ob überhaupt eine nächste Phase existiert
            var nextPhase = GetNextPhase();
            if (nextPhase == null)
            {
                System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: No next phase available - tournament ends here");
                System.Diagnostics.Debug.WriteLine($"=== CanProceedToNextPhase END (FALSE - no next phase) ===");
                return false;
            }
            
            // KORREKTUR: Dann prüfe ob die aktuelle Phase bereit für den Übergang ist
            var currentPhaseReady = _tournament.CurrentPhase?.CanProceedToNextPhase() ?? false;
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Current phase ready = {currentPhaseReady}");
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Next phase available = {nextPhase.PhaseType}");
            
            var result = currentPhaseReady;
            System.Diagnostics.Debug.WriteLine($"CanProceedToNextPhase: Final result = {result}");
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
    /// KORRIGIERT: Bessere Debugging-Ausgabe und korrekte PostGroupPhaseMode-Behandlung
    /// ? ERWEITERT: Berücksichtigt SkipGroupPhase Modus
    /// </summary>
    /// <returns>Die nächste Phase oder null wenn das Turnier beendet ist</returns>
    public TournamentPhase? GetNextPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GetNextPhase START ===");
            System.Diagnostics.Debug.WriteLine($"SkipGroupPhase: {_tournament.GameRules.SkipGroupPhase}");
            
            if (_tournament.CurrentPhase == null) 
            {
                System.Diagnostics.Debug.WriteLine($"GetNextPhase: CurrentPhase is null");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"GetNextPhase: Current phase = {_tournament.CurrentPhase.PhaseType}");
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: PostGroupPhaseMode = {_tournament.GameRules.PostGroupPhaseMode}");
            
            // ? WICHTIG: Bei SkipGroupPhase gibt es keine "nächste Phase" nach KO-Phase!
            if (_tournament.GameRules.SkipGroupPhase && _tournament.CurrentPhase.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                System.Diagnostics.Debug.WriteLine("GetNextPhase: SkipGroupPhase + KO-Phase active - no next phase");
                return null;
            }

            // KORRIGIERTE Logik: Bestimme nächste Phase basierend auf aktueller Phase und Spielregeln
            TournamentPhase? nextPhase = _tournament.CurrentPhase.PhaseType switch
            {
                TournamentPhaseType.GroupPhase => _tournament.GameRules.PostGroupPhaseMode switch
                {
                    PostGroupPhaseMode.RoundRobinFinals => CreateRoundRobinFinalsPhase(),
                    PostGroupPhaseMode.KnockoutBracket => CreateKnockoutPhase(), // ? Nur aufrufen wenn NICHT SkipGroupPhase
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
            
            System.Diagnostics.Debug.WriteLine($"GetNextPhase: Next phase = {nextPhase?.PhaseType.ToString() ?? "null"}");
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
    /// ? KORRIGIERT: Verhindert mehrfaches Erstellen der gleichen Phase
    /// </summary>
    public void AdvanceToNextPhase()
    {
        System.Diagnostics.Debug.WriteLine($"=== AdvanceToNextPhase START for {_tournament.Name} ===");
    System.Diagnostics.Debug.WriteLine($"  Current Phase: {_tournament.CurrentPhase?.PhaseType.ToString() ?? "null"}");
        
   // Prüfung ob Phasenwechsel möglich ist
   if (!CanProceedToNextPhase())
        {
          System.Diagnostics.Debug.WriteLine($"  ? Cannot proceed to next phase");
 return;
    }

 // Ermittlung der nächsten Phase
      var nextPhase = GetNextPhase();
if (nextPhase == null)
      {
      System.Diagnostics.Debug.WriteLine($"  ? No next phase available");
       return;
   }

      System.Diagnostics.Debug.WriteLine($"  Next Phase Type: {nextPhase.PhaseType}");
        
     // ? NEU: Prüfe ob diese Phase bereits existiert (verhindert Duplikate!)
 var existingPhase = _tournament.Phases.FirstOrDefault(p => p.PhaseType == nextPhase.PhaseType && p != _tournament.CurrentPhase);
  if (existingPhase != null)
        {
       System.Diagnostics.Debug.WriteLine($"  ?? Phase {nextPhase.PhaseType} already exists! Using existing phase instead of creating new one.");
     
     // Verwende die existierende Phase statt eine neue zu erstellen
nextPhase = existingPhase;
        }
    else
      {
  System.Diagnostics.Debug.WriteLine($"  ? Creating new phase: {nextPhase.PhaseType}");
 }

      // Markiere aktuelle Phase als abgeschlossen
        if (_tournament.CurrentPhase != null)
        {
            _tournament.CurrentPhase.IsActive = false;
        _tournament.CurrentPhase.IsCompleted = true;
          System.Diagnostics.Debug.WriteLine($"  Marked {_tournament.CurrentPhase.PhaseType} as completed");
        }

  // ? KORRIGIERT: Füge neue Phase nur hinzu, wenn sie noch nicht existiert
        if (!_tournament.Phases.Contains(nextPhase))
 {
            _tournament.Phases.Add(nextPhase);
        System.Diagnostics.Debug.WriteLine($"  Added new phase to Phases collection");
     }

      // Aktiviere die nächste Phase
     _tournament.CurrentPhase = nextPhase;
        nextPhase.IsActive = true;

        System.Diagnostics.Debug.WriteLine($"=== AdvanceToNextPhase END - Now in {nextPhase.PhaseType} ===");
    }

    /// <summary>
    /// Erstellt eine Round-Robin-Finalrunde mit den qualifizierten Spielern aus der Gruppenphase
    /// Alle qualifizierten Spieler spielen nochmals jeder gegen jeden
    /// </summary>
    /// <returns>Eine neue TournamentPhase für die Finalrunde</returns>
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
            Id = 999, // Spezielle ID für Finals
            Name = "Finalrunde",
            MatchesGenerated = false
        };

        // Füge alle qualifizierten Spieler zur Finals-Gruppe hinzu
        foreach (var player in qualifiedPlayers)
        {
            finalsGroup.Players.Add(player);
            //System.Diagnostics.Debug.WriteLine($"  Added player: {player.Name}");
        }

        // KRITISCHER FIX: Generiere die Round Robin Matches für die Finals-Gruppe mit korrekten GameRules!
        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Generating Round Robin matches for {finalsGroup.Players.Count} players");
        
        // WICHTIG: Verwende die rundenspezifischen Regeln für Round Robin Finals
        var finalsRules = _tournament.GameRules.GetRulesForRoundRobinFinals(RoundRobinFinalsRound.Finals);
        var finalsGameRules = new GameRules
        {
            GameMode = _tournament.GameRules.GameMode,
            FinishMode = _tournament.GameRules.FinishMode,
            PlayWithSets = finalsRules.SetsToWin > 0,
            SetsToWin = finalsRules.SetsToWin,
            LegsToWin = finalsRules.LegsToWin,
            LegsPerSet = finalsRules.LegsPerSet
        };
        
        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Using finalsGameRules - PlayWithSets: {finalsGameRules.PlayWithSets}, SetsToWin: {finalsGameRules.SetsToWin}, LegsToWin: {finalsGameRules.LegsToWin}");
        
        finalsGroup.GenerateRoundRobinMatches(finalsGameRules);
        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Generated {finalsGroup.Matches.Count} matches");
        
        // ZUSÄTZLICHE VALIDIERUNG: Überprüfe ob UsesSets korrekt gesetzt wurde UND alle UUIDs vorhanden sind
        foreach (var match in finalsGroup.Matches)
        {
            // Stelle sicher, dass jedes Finals-Match eine gültige UUID hat
            match.EnsureUniqueId();
            
            //System.Diagnostics.Debug.WriteLine($"  Finals Match {match.Id}: {match.Player1?.Name} vs {match.Player2?.Name}, UsesSets: {match.UsesSets}, UUID: {match.UniqueId}");
        }

        // WICHTIG: Setze die Finals-Gruppe sowohl in der Phase als auch als QualifiedPlayers
        finalsPhase.FinalsGroup = finalsGroup;
        finalsPhase.QualifiedPlayers = new ObservableCollection<Player>(qualifiedPlayers);

        // ZUSÄTZLICH: Trigger UI-Refresh Event für sofortige Aktualisierung
        //System.Diagnostics.Debug.WriteLine($"CreateRoundRobinFinalsPhase: Triggering UI refresh");
        _tournament.TriggerUIRefresh();

        //System.Diagnostics.Debug.WriteLine($"=== CreateRoundRobinFinalsPhase END - Created phase with {finalsGroup.Matches.Count} matches ===");
        return finalsPhase;
    }

    private TournamentPhase CreateKnockoutPhase()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== CreateKnockoutPhase START ===");
            System.Diagnostics.Debug.WriteLine($"SkipGroupPhase: {_tournament.GameRules.SkipGroupPhase}");
            System.Diagnostics.Debug.WriteLine($"CurrentPhase: {_tournament.CurrentPhase?.PhaseType}");
            
            var knockoutPhase = new TournamentPhase
            {
                Name = _tournament.GameRules.KnockoutMode == KnockoutMode.DoubleElimination ? "KO-Phase (Double Elimination)" : "KO-Phase (Single Elimination)",
                PhaseType = TournamentPhaseType.KnockoutPhase
            };

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Created phase with name '{knockoutPhase.Name}'");

            // ? NEU: Bei SkipGroupPhase darf diese Methode NICHT aufgerufen werden!
            // CreateDirectKnockoutPhase() sollte stattdessen verwendet werden
            if (_tournament.GameRules.SkipGroupPhase)
            {
                System.Diagnostics.Debug.WriteLine("CreateKnockoutPhase: ERROR - SkipGroupPhase is active! Use CreateDirectKnockoutPhase() instead!");
                throw new InvalidOperationException("CreateKnockoutPhase darf nicht im SkipGroupPhase-Modus aufgerufen werden. Verwenden Sie CreateDirectKnockoutPhase().");
            }

            // Get qualified players from previous phase
            System.Collections.Generic.List<Player> qualifiedPlayers;
            if (_tournament.CurrentPhase?.PhaseType == TournamentPhaseType.RoundRobinFinals)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Getting players from RoundRobinFinals");
                qualifiedPlayers = _tournament.CurrentPhase.GetQualifiedPlayers(int.MaxValue); // All players from finals
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Getting players from GroupPhase");
                var groupPhase = _tournament.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
                
                if (groupPhase == null)
                {
                    System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: ERROR - No GroupPhase found!");
                    throw new InvalidOperationException("Keine Gruppenphase gefunden. CreateKnockoutPhase benötigt eine vorherige Gruppenphase.");
                }
                
                qualifiedPlayers = groupPhase.GetQualifiedPlayers(_tournament.GameRules.QualifyingPlayersPerGroup);
            }

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Found {qualifiedPlayers.Count} qualified players");
            foreach (var player in qualifiedPlayers)
            {
                System.Diagnostics.Debug.WriteLine($"  - {player.Name}");
            }

            // WICHTIGE VALIDIERUNG: Prüfen ob genügend Spieler vorhanden
            if (qualifiedPlayers.Count <= 1)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: CRITICAL ERROR - Not enough players ({qualifiedPlayers.Count})");
                throw new InvalidOperationException($"Nicht genügend Spieler für K.O.-Phase: {qualifiedPlayers.Count}");
            }

            knockoutPhase.QualifiedPlayers = new ObservableCollection<Player>(qualifiedPlayers);

            // Generate bracket - delegiere an KnockoutBracketGenerator
            var bracketGenerator = new KnockoutBracketGenerator(_tournament);
            
            if (_tournament.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generating double elimination bracket");
                bracketGenerator.GenerateDoubleEliminationBracket(knockoutPhase, qualifiedPlayers);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generating single elimination bracket");
                bracketGenerator.GenerateSingleEliminationBracket(knockoutPhase, qualifiedPlayers);
            }

            System.Diagnostics.Debug.WriteLine($"CreateKnockoutPhase: Generated {knockoutPhase.WinnerBracket.Count} winner bracket matches");
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
    /// ? ERWEITERT: Überschreibt CurrentPhase NICHT wenn bereits gesetzt
    /// </summary>
    public void EnsureGroupPhaseExists()
    {
        //System.Diagnostics.Debug.WriteLine($"=== EnsureGroupPhaseExists START for {_tournament.Name} ===");
        //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Current Phases count = {_tournament.Phases.Count}");
        //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: CurrentPhase = {_tournament.CurrentPhase?.PhaseType}");
        //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: SkipGroupPhase = {_tournament.GameRules.SkipGroupPhase}");
        
        // ? WICHTIG: Wenn SkipGroupPhase aktiv ist, NICHTS tun!
        if (_tournament.GameRules.SkipGroupPhase)
        {
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: SkipGroupPhase is active, skipping GroupPhase management");
            //System.Diagnostics.Debug.WriteLine($"=== EnsureGroupPhaseExists END (early exit - SkipGroupPhase) ===");
            return;
        }
        
        // ? WICHTIG: Wenn bereits eine CurrentPhase gesetzt ist (z.B. KO-Phase), NICHT überschreiben!
        if (_tournament.CurrentPhase != null && _tournament.CurrentPhase.PhaseType != TournamentPhaseType.GroupPhase)
        {
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: CurrentPhase is already set to {_tournament.CurrentPhase.PhaseType}, not changing");
            //System.Diagnostics.Debug.WriteLine($"=== EnsureGroupPhaseExists END (early exit - other phase active) ===");
            return;
        }
        
        // Prüfe ob bereits eine GroupPhase existiert
        var existingGroupPhase = _tournament.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.GroupPhase);
        
        if (existingGroupPhase == null)
        {
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: No GroupPhase found, creating new one");
            
            // Erstelle eine neue GroupPhase
            var groupPhase = new TournamentPhase
            {
                Name = "Gruppenphase",
                PhaseType = TournamentPhaseType.GroupPhase,
                IsActive = true // Gruppenphase ist standardmäßig aktiv
            };
            
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Created new TournamentPhase");
        
            _tournament.Phases.Add(groupPhase);
            
            // ? NUR setzen wenn CurrentPhase null ist
            if (_tournament.CurrentPhase == null)
            {
                _tournament.CurrentPhase = groupPhase;
                //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Set new GroupPhase as CurrentPhase");
            }
            
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Created new GroupPhase, total phases = {_tournament.Phases.Count}");
        }
        else
        {
            //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: GroupPhase already exists");
            
            // ? Setze die existierende GroupPhase als CurrentPhase NUR wenn noch keine gesetzt
            if (_tournament.CurrentPhase == null)
            {
                _tournament.CurrentPhase = existingGroupPhase;
                //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: Set existing GroupPhase as CurrentPhase");
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"EnsureGroupPhaseExists: CurrentPhase already set to {_tournament.CurrentPhase.PhaseType}, not overwriting");
            }
        }

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
            
            // Prüfe ob FinalsGroup existiert
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
                        // WICHTIG: Verwende die rundenspezifischen Regeln für Round Robin Finals
                        var finalsRules = _tournament.GameRules.GetRulesForRoundRobinFinals(RoundRobinFinalsRound.Finals);
                        var finalsGameRules = new GameRules
                        {
                            GameMode = _tournament.GameRules.GameMode,
                            FinishMode = _tournament.GameRules.FinishMode,
                            PlayWithSets = finalsRules.SetsToWin > 0,
                            SetsToWin = finalsRules.SetsToWin,
                            LegsToWin = finalsRules.LegsToWin,
                            LegsPerSet = finalsRules.LegsPerSet
                        };
                        
                        finalsGroup.GenerateRoundRobinMatches(finalsGameRules);
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
                    
                    // WICHTIG: Verwende die rundenspezifischen Regeln für Round Robin Finals
                    var finalsRules = _tournament.GameRules.GetRulesForRoundRobinFinals(RoundRobinFinalsRound.Finals);
                    var finalsGameRules = new GameRules
                    {
                        GameMode = _tournament.GameRules.GameMode,
                        FinishMode = _tournament.GameRules.FinishMode,
                        PlayWithSets = finalsRules.SetsToWin > 0,
                        SetsToWin = finalsRules.SetsToWin,
                        LegsToWin = finalsRules.LegsToWin,
                        LegsPerSet = finalsRules.LegsPerSet
                    };
                    
                    finalsPhase.FinalsGroup.GenerateRoundRobinMatches(finalsGameRules);
                }
            }
        }

        //System.Diagnostics.Debug.WriteLine($"=== EnsureFinalsPhaseIntegrity END ===");
    }

    /// <summary>
    /// NEUE ÖFFENTLICHE METHODE: Führt vollständige Phase-Validierung nach JSON-Loading durch
    /// Sollte aufgerufen werden nachdem Tournament-Daten geladen wurden
    /// ? ERWEITERT: Berücksichtigt SkipGroupPhase Modus
    /// </summary>
    public void ValidateAndRepairPhases()
    {
        //System.Diagnostics.Debug.WriteLine($"=== ValidateAndRepairPhases START for {_tournament.Name} ===");
        //System.Diagnostics.Debug.WriteLine($"SkipGroupPhase: {_tournament.GameRules.SkipGroupPhase}");
        //System.Diagnostics.Debug.WriteLine($"Current Phase: {_tournament.CurrentPhase?.PhaseType}");
        //System.Diagnostics.Debug.WriteLine($"Phases count: {_tournament.Phases.Count}");
        
        try
        {
            // ? WICHTIG: Wenn SkipGroupPhase aktiv ist und wir bereits in KO-Phase sind,
            // NICHT die GroupPhase erstellen!
            if (_tournament.GameRules.SkipGroupPhase && 
                _tournament.CurrentPhase?.PhaseType == TournamentPhaseType.KnockoutPhase)
            {
                //System.Diagnostics.Debug.WriteLine("ValidateAndRepairPhases: SkipGroupPhase mode - skipping GroupPhase creation");
                
                // Nur UUID-Validierung und UI-Refresh
                EnsureAllMatchesHaveUuids();
                _tournament.TriggerUIRefresh();
                
                //System.Diagnostics.Debug.WriteLine("ValidateAndRepairPhases: Done (SkipGroupPhase mode)");
                return;
            }
            
            // Normale Validierung mit Gruppenphase
            // 1. Stelle sicher dass GroupPhase existiert (nur wenn nicht SkipGroupPhase)
            if (!_tournament.GameRules.SkipGroupPhase)
            {
                EnsureGroupPhaseExists();
            }
            
            // 2. Repariere Finals-Phase falls vorhanden
            EnsureFinalsPhaseIntegrity();
            
            // 3. WICHTIG: Stelle sicher dass alle Matches gültige UUIDs haben
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
    /// NEUE PRIVATE METHODE: Stellt sicher, dass alle Matches in allen Phasen gültige UUIDs haben
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
            // 1. Prüfe Gruppen-Matches
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

            // 2. Prüfe Finals-Matches
            var finalsPhase = _tournament.Phases.FirstOrDefault(p => p.PhaseType == TournamentPhaseType.RoundRobinFinals);
            if (finalsPhase?.FinalsGroup?.Matches != null)
            {
                //System.Diagnostics.Debug.WriteLine($"  Checking Finals matches: {finalsPhase.FinalsGroup.Matches.Count} matches");
                
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

            // 3. Prüfe K.O.-Matches
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
    
    /// <summary>
    /// ? NEU: Erstellt eine KO-Phase direkt aus allen Spielern (ohne Gruppenphase)
    /// Wird verwendet wenn GameRules.SkipGroupPhase = true
    /// </summary>
    /// <param name="allPlayers">Liste aller Turnierteilnehmer</param>
    /// <returns>Eine neue KO-Phase mit allen Spielern</returns>
    public TournamentPhase CreateDirectKnockoutPhase(System.Collections.Generic.List<Player> allPlayers)
    {
        try
        {
            //System.Diagnostics.Debug.WriteLine($"=== CreateDirectKnockoutPhase START ===");
            //System.Diagnostics.Debug.WriteLine($"CreateDirectKnockoutPhase: Players count = {allPlayers?.Count ?? 0}");
            
            // Validierung
            if (allPlayers == null || allPlayers.Count <= 1)
            {
                throw new InvalidOperationException($"Nicht genügend Spieler für direkte KO-Phase: {allPlayers?.Count ?? 0}");
            }
            
            // Erstelle KO-Phase
            var knockoutPhase = new TournamentPhase
            {
                Name = _tournament.GameRules.KnockoutMode == KnockoutMode.DoubleElimination 
                    ? "KO-Phase (Double Elimination)" 
                    : "KO-Phase (Single Elimination)",
                PhaseType = TournamentPhaseType.KnockoutPhase,
                IsActive = true // Direkt aktiv, da keine Gruppenphase
            };
            
            //System.Diagnostics.Debug.WriteLine($"CreateDirectKnockoutPhase: Created phase '{knockoutPhase.Name}'");
            
            // Alle Spieler sind qualifiziert (keine Gruppenphase)
            knockoutPhase.QualifiedPlayers = new ObservableCollection<Player>(allPlayers);
            
            // Generiere Bracket
            var bracketGenerator = new KnockoutBracketGenerator(_tournament);
            
            if (_tournament.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                //System.Diagnostics.Debug.WriteLine($"CreateDirectKnockoutPhase: Generating double elimination bracket");
                bracketGenerator.GenerateDoubleEliminationBracket(knockoutPhase, allPlayers);
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine($"CreateDirectKnockoutPhase: Generating single elimination bracket");
                bracketGenerator.GenerateSingleEliminationBracket(knockoutPhase, allPlayers);
            }
            
            //System.Diagnostics.Debug.WriteLine($"CreateDirectKnockoutPhase: Generated {knockoutPhase.WinnerBracket.Count} winner bracket matches");
            if (_tournament.GameRules.KnockoutMode == KnockoutMode.DoubleElimination)
            {
                System.Diagnostics.Debug.WriteLine($"CreateDirectKnockoutPhase: Generated {knockoutPhase.LoserBracket.Count} loser bracket matches");
            }
            
            //System.Diagnostics.Debug.WriteLine($"=== CreateDirectKnockoutPhase END ===");
            
            return knockoutPhase;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CreateDirectKnockoutPhase: CRITICAL ERROR: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"CreateDirectKnockoutPhase: Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}