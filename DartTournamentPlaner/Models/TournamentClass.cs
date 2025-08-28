using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Repräsentiert eine komplette Turnierklasse (z.B. Platin, Gold, Silber, Bronze)
/// Diese Klasse ist das Herzstück des Turniersystems und verwaltet alle Phasen eines Turniers
/// Implementiert INotifyPropertyChanged für UI-Updates und unterstützt alle Turniermodi
/// 
/// REFACTORING: Die ursprünglich sehr große Klasse wurde in spezialisierte Manager-Klassen aufgeteilt:
/// - TournamentPhaseManager: Phasenverwaltung und -übergänge
/// - KnockoutBracketGenerator: K.O.-Turnierbaum-Generierung
/// - TournamentTreeRenderer: UI-Rendering für Turnierbäume
/// - ByeMatchManager: Freilos-Verwaltung
/// </summary>
public class TournamentClass : INotifyPropertyChanged
{
    // Manager-Instanzen für spezialisierte Funktionen
    private readonly TournamentPhaseManager _phaseManager;
    private readonly TournamentTreeRenderer _treeRenderer;
    private readonly ByeMatchManager _byeMatchManager;

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
    /// KORRIGIERT: Verwendet nur ein Backing-Field
    /// </summary>
    public GameRules GameRules
    {
        get => _gameRules;
        set
        {
            // WICHTIG: Event-Handler vom alten GameRules entfernen
            if (_gameRules != null)
            {
                _gameRules.PropertyChanged -= OnGameRulesPropertyChanged;
            }
            
            _gameRules = value ?? new GameRules();
            
            // WICHTIG: Event-Handler zum neuen GameRules hinzufügen
            _gameRules.PropertyChanged += OnGameRulesPropertyChanged;
            
            System.Diagnostics.Debug.WriteLine($"GameRules set for {Name}: {_gameRules}");
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Event-Handler für GameRules-Änderungen
    /// Propagiert Änderungen in den GameRules an die UI
    /// </summary>
    private void OnGameRulesPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"GameRules property changed: {e.PropertyName} for {Name}");
        
        // WICHTIG: Spezifische Property-Updates loggen
        if (e.PropertyName == nameof(GameRules.PostGroupPhaseMode))
        {
            System.Diagnostics.Debug.WriteLine($"  PostGroupPhaseMode changed to: {GameRules.PostGroupPhaseMode}");
        }
        else if (e.PropertyName == nameof(GameRules.QualifyingPlayersPerGroup))
        {
            System.Diagnostics.Debug.WriteLine($"  QualifyingPlayersPerGroup changed to: {GameRules.QualifyingPlayersPerGroup}");
        }
        else if (e.PropertyName == nameof(GameRules.KnockoutMode))
        {
            System.Diagnostics.Debug.WriteLine($"  KnockoutMode changed to: {GameRules.KnockoutMode}");
        }
        else if (e.PropertyName == nameof(GameRules.IncludeGroupPhaseLosersBracket))
        {
            System.Diagnostics.Debug.WriteLine($"  IncludeGroupPhaseLosersBracket changed to: {GameRules.IncludeGroupPhaseLosersBracket}");
        }
        
        // Propagiere GameRules-Änderungen an UI
        OnPropertyChanged(nameof(GameRules));
        
        // Spezifische Propagation für wichtige Eigenschaften
        if (e.PropertyName == nameof(GameRules.PostGroupPhaseMode) ||
            e.PropertyName == nameof(GameRules.QualifyingPlayersPerGroup) ||
            e.PropertyName == nameof(GameRules.KnockoutMode) ||
            e.PropertyName == nameof(GameRules.IncludeGroupPhaseLosersBracket))
        {
            System.Diagnostics.Debug.WriteLine($"  Triggering UI refresh for important property change: {e.PropertyName}");
            
            // Trigger UI refresh wenn sich die Turnierstruktur ändert
            TriggerUIRefresh();
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
            System.Diagnostics.Debug.WriteLine($"CurrentPhase set for {Name}: {_currentPhase?.PhaseType}");
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
            _phaseManager.EnsureGroupPhaseExists();
            
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
    /// Initialisiert die Manager-Instanzen
    /// </summary>
    public TournamentClass()
    {
        System.Diagnostics.Debug.WriteLine($"=== TournamentClass Constructor START ===");
        
        // WICHTIG: Initialisiere Manager-Instanzen ZUERST
        _phaseManager = new TournamentPhaseManager(this);
        _treeRenderer = new TournamentTreeRenderer(this);
        _byeMatchManager = new ByeMatchManager(this);
        
        // WICHTIG: KEINE automatische GroupPhase-Erstellung im Constructor!
        // Das würde bei JSON-Deserialisierung zu Duplikaten führen, da:
        // 1. Constructor erstellt GroupPhase (Phases ist noch leer)
        // 2. JSON-Deserialisierung fügt weitere Phases hinzu
        // 3. Resultat: Duplikat-GroupPhases!
        
        // KORRIGIERT: Event-Handler für GameRules hinzufügen
        if (_gameRules != null)
        {
            _gameRules.PropertyChanged += OnGameRulesPropertyChanged;
        }
        
        // Stattdessen: Verwende eine Lazy Initialization-Strategie über EnsureGroupPhaseExists()
        System.Diagnostics.Debug.WriteLine($"TournamentClass Constructor: Phases collection initialized, count = {Phases.Count}");
        
        System.Diagnostics.Debug.WriteLine($"=== TournamentClass Constructor END ===");
    }

    #region Phase Management - Delegiert an TournamentPhaseManager

    /// <summary>
    /// Prüft ob zur nächsten Phase gewechselt werden kann
    /// DELEGIERT an TournamentPhaseManager
    /// </summary>
    public bool CanProceedToNextPhase() => _phaseManager.CanProceedToNextPhase();

    /// <summary>
    /// Ermittelt die nächste Phase basierend auf der aktuellen Phase und den Spielregeln
    /// DELEGIERT an TournamentPhaseManager
    /// </summary>
    public TournamentPhase? GetNextPhase() => _phaseManager.GetNextPhase();

    /// <summary>
    /// Führt den Wechsel zur nächsten Phase durch
    /// DELEGIERT an TournamentPhaseManager
    /// </summary>
    public void AdvanceToNextPhase() => _phaseManager.AdvanceToNextPhase();

    /// <summary>
    /// Stellt sicher, dass mindestens eine GroupPhase existiert
    /// DELEGIERT an TournamentPhaseManager
    /// </summary>
    public void EnsureGroupPhaseExists() => _phaseManager.EnsureGroupPhaseExists();

    /// <summary>
    /// Validiert und repariert Finals-Phase nach JSON-Loading
    /// DELEGIERT an TournamentPhaseManager
    /// </summary>
    public void EnsureFinalsPhaseIntegrity() => _phaseManager.EnsureFinalsPhaseIntegrity();

    /// <summary>
    /// Führt vollständige Phase-Validierung nach JSON-Loading durch
    /// DELEGIERT an TournamentPhaseManager
    /// </summary>
    public void ValidateAndRepairPhases() => _phaseManager.ValidateAndRepairPhases();

    #endregion

    #region Bye Match Management - Delegiert an ByeMatchManager

    /// <summary>
    /// Manueller Refresh aller Freilose
    /// DELEGIERT an ByeMatchManager
    /// </summary>
    public void RefreshAllByeMatches() => _byeMatchManager.RefreshAllByeMatches();

    /// <summary>
    /// Manuelle Freilos-Vergabe
    /// DELEGIERT an ByeMatchManager
    /// </summary>
    public bool GiveManualBye(KnockoutMatch match, Player? byeWinner = null) 
        => _byeMatchManager.GiveManualBye(match, byeWinner);

    /// <summary>
    /// Rückgängigmachen eines Freiloses
    /// DELEGIERT an ByeMatchManager
    /// </summary>
    public bool UndoBye(KnockoutMatch match) => _byeMatchManager.UndoBye(match);

    /// <summary>
    /// Validierung von Freilos-Operationen
    /// DELEGIERT an ByeMatchManager
    /// </summary>
    public ByeValidationResult ValidateByeOperation(KnockoutMatch match) 
        => _byeMatchManager.ValidateByeOperation(match);

    /// <summary>
    /// Status-Überprüfung für UI-Buttons
    /// DELEGIERT an ByeMatchManager
    /// </summary>
    public MatchByeUIStatus GetMatchByeUIStatus(int matchId) 
        => _byeMatchManager.GetMatchByeUIStatus(matchId);

    #endregion

    #region Tournament Tree Rendering - Delegiert an TournamentTreeRenderer

    /// <summary>
    /// Erstellt eine interaktive Turnierbaum-Ansicht
    /// DELEGIERT an TournamentTreeRenderer
    /// </summary>
    public FrameworkElement? CreateTournamentTreeView(Canvas targetCanvas, bool isLoserBracket, LocalizationService? localizationService = null) 
        => _treeRenderer.CreateTournamentTreeView(targetCanvas, isLoserBracket, localizationService);

    #endregion

    #region Match Processing and Utility Methods

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

            // WICHTIG: DIREKTE Spieler-Propagation verwenden - delegiert an ByeMatchManager!
            var winnerBracket = CurrentPhase.WinnerBracket;
            var loserBracket = CurrentPhase.LoserBracket;
            
            System.Diagnostics.Debug.WriteLine($"  DIRECT propagation: Using PropagateMatchResultWithAutomaticByes");
            
            // Verwende die Propagations-Logik aus ByeMatchManager
            if (completedMatch.BracketType == BracketType.Winner)
            {
                _byeMatchManager.PropagateMatchResultWithAutomaticByes(completedMatch, winnerBracket, loserBracket);
            }
            else
            {
                _byeMatchManager.PropagateMatchResultWithAutomaticByes(completedMatch, loserBracket, null);
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

    #endregion

    #region UI and Event Management

    /// <summary>
    /// Löst ein UI-Refresh-Event aus, um ViewModels zu aktualisieren
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
    /// Spezifischer Refresh für Match-Status-Änderungen
    /// Dieser sollte verwendet werden wenn sich der Status einzelner Matches ändert
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
    /// Event für Datenänderungen - wird gefeuert wenn Match-Ergebnisse eingegeben oder Freilose vergeben werden
    /// </summary>
    public event EventHandler? DataChangedEvent;

    /// <summary>
    /// Event für UI-Refreshs nach automatischen Freilos-Änderungen
    /// </summary>
    public event EventHandler? UIRefreshRequested;

    /// <summary>
    /// Event für spezifische Match-Status-Änderungen
    /// </summary>
    public event EventHandler<MatchStatusChangedEventArgs>? MatchStatusChanged;

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    public override string ToString()
    {
        return $"{Name} - {GameRules}";
    }
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

/// <summary>
/// Report für UUID-Validierung in einem Tournament
/// </summary>
public class UuidValidationReport
{
    public int TotalMatches { get; set; }
    public int ValidUuids { get; set; }
    public int InvalidUuids { get; set; }
    public int DuplicateUuids { get; set; }
    public List<string> Issues { get; set; } = new List<string>();
    public bool IsValid { get; set; }
    public double ValidPercentage { get; set; }

    public override string ToString()
    {
        var status = IsValid ? "✅ VALID" : "❌ INVALID";
        return $"{status} - {ValidUuids}/{TotalMatches} valid UUIDs ({ValidPercentage:F1}%), {InvalidUuids} invalid, {DuplicateUuids} duplicates";
    }

    /// <summary>
    /// Gibt einen detaillierten Report als String zurück
    /// </summary>
    public string GetDetailedReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"📊 UUID Validation Report");
        sb.AppendLine($"========================");
        sb.AppendLine($"Total Matches: {TotalMatches}");
        sb.AppendLine($"Valid UUIDs: {ValidUuids} ({ValidPercentage:F1}%)");
        sb.AppendLine($"Invalid UUIDs: {InvalidUuids}");
        sb.AppendLine($"Duplicate UUIDs: {DuplicateUuids}");
        sb.AppendLine($"Overall Status: {(IsValid ? "✅ VALID" : "❌ ISSUES FOUND")}");
        
        if (Issues.Any())
        {
            sb.AppendLine();
            sb.AppendLine("🔍 Issues Found:");
            foreach (var issue in Issues)
            {
                sb.AppendLine($"  • {issue}");
            }
        }
        
        return sb.ToString();
    }
}
