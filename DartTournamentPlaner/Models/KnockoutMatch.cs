using System.ComponentModel;
using System.Runtime.CompilerServices;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Enumeration für die verschiedenen Bracket-Typen in einem Double-Elimination-Turnier
/// </summary>
public enum BracketType
{
    Winner,  // Winner Bracket - Hauptturnier für noch nicht eliminierte Spieler
    Loser    // Loser Bracket - Sekundärturnier für einmal eliminierte Spieler
}

/// <summary>
/// Enumeration für die verschiedenen K.O.-Runden in einem Turnier
/// Umfasst sowohl Winner Bracket als auch Loser Bracket Runden
/// </summary>
public enum KnockoutRound
{
    // Winner Bracket Rounds - Hauptturnier Runden
    Best64,        // Beste 64 Spieler
    Best32,        // Beste 32 Spieler
    Best16,        // Beste 16 Spieler
    Quarterfinal,  // Viertelfinale
    Semifinal,     // Halbfinale
    Final,         // Finale
    GrandFinal,    // Grand Final (Entscheidungsspiel zwischen Winner und Loser Bracket Champion)
    
    // Loser Bracket Rounds - Verlierer-Bracket Runden
    LoserRound1,   // Erste Runde im Loser Bracket
    LoserRound2,   // Zweite Runde im Loser Bracket
    LoserRound3,   // Dritte Runde im Loser Bracket
    LoserRound4,   // Vierte Runde im Loser Bracket
    LoserRound5,   // Fünfte Runde im Loser Bracket
    LoserRound6,   // Sechste Runde im Loser Bracket
    LoserRound7,   // Siebte Runde im Loser Bracket
    LoserRound8,   // Achte Runde im Loser Bracket
    LoserRound9,   // Neunte Runde im Loser Bracket
    LoserRound10,  // Zehnte Runde im Loser Bracket
    LoserRound11,  // Elfte Runde im Loser Bracket
    LoserRound12,  // Zwölfte Runde im Loser Bracket
    LoserFinal     // Loser Bracket Finale
}

/// <summary>
/// Repräsentiert ein einzelnes K.O.-Match in einem Dart-Turnier
/// Implementiert INotifyPropertyChanged für UI-Binding und Live-Updates
/// </summary>
public class KnockoutMatch : INotifyPropertyChanged
{
    // Private Felder für alle Eigenschaften mit Backing Fields
    private int _id;                        // Eindeutige numerische ID des Matches (legacy)
    private string _uniqueId;               // Eindeutige UUID für Hub-Integration
    private Player? _player1;               // Erster Spieler (kann null sein wenn TBD)
    private Player? _player2;               // Zweiter Spieler (kann null sein wenn TBD)
    private int _player1Sets = 0;           // Gewonnene Sets von Spieler 1
    private int _player2Sets = 0;           // Gewonnene Sets von Spieler 2
    private int _player1Legs = 0;           // Gewonnene Legs von Spieler 1
    private int _player2Legs = 0;           // Gewonnene Legs von Spieler 2
    private MatchStatus _status = MatchStatus.NotStarted;  // Aktueller Status des Matches
    private Player? _winner;                // Gewinner des Matches
    private Player? _loser;                 // Verlierer des Matches
    private KnockoutRound _round;           // Turnierrunde
    private int _position;                  // Position innerhalb der Runde
    private BracketType _bracketType = BracketType.Winner;  // Bracket-Typ (Winner/Loser)
    private KnockoutMatch? _sourceMatch1;   // Quelle für Player 1 (vorheriges Match)
    private KnockoutMatch? _sourceMatch2;   // Quelle für Player 2 (vorheriges Match)
    private DateTime? _startTime;           // Startzeit des Matches
    private DateTime? _endTime;             // Endzeit des Matches
    private string _notes = string.Empty;   // Notizen zum Match
    private bool _usesSets = false;         // Verwendet dieses Match Sets oder nur Legs
    private bool _player1FromWinner = false;  // Kommt Player 1 vom Winner Bracket
    private bool _player2FromWinner = false;  // Kommt Player 2 vom Winner Bracket

    // Statische Referenz zum Lokalisierungsservice für Übersetzungen
    public static LocalizationService? LocalizationService { get; set; }

    /// <summary>
    /// Konstruktor - erstellt automatisch eine eindeutige UUID
    /// </summary>
    public KnockoutMatch()
    {
        _uniqueId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Eindeutige UUID des Matches für Hub-Integration
    /// Diese ID wird für alle Hub-Operationen verwendet
    /// </summary>
    public string UniqueId
    {
        get => _uniqueId;
        set
        {
            _uniqueId = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Eindeutige numerische ID des Matches (legacy)
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
    /// Erster Spieler im Match
    /// Kann null sein wenn der Spieler noch nicht bestimmt ist (TBD - To Be Determined)
    /// </summary>
    public Player? Player1
    {
        get => _player1;
        set
        {
            _player1 = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(DisplayName)); // Aktualisiert auch Anzeigename
        }
    }

    /// <summary>
    /// Zweiter Spieler im Match
    /// Kann null sein wenn der Spieler noch nicht bestimmt ist (TBD - To Be Determined)
    /// </summary>
    public Player? Player2
    {
        get => _player2;
        set
        {
            _player2 = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(DisplayName)); // Aktualisiert auch Anzeigename
        }
    }

    /// <summary>
    /// Gewinner des Matches - wird nach Beendigung des Spiels gesetzt
    /// </summary>
    public Player? Winner
    {
        get => _winner;
        set
        {
            _winner = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(WinnerDisplay)); // Aktualisiert Gewinner-Anzeige
        }
    }

    /// <summary>
    /// Verlierer des Matches - wird nach Beendigung des Spiels gesetzt
    /// Wichtig für Loser Bracket Progression
    /// </summary>
    public Player? Loser
    {
        get => _loser;
        set
        {
            _loser = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Anzahl der von Spieler 1 gewonnenen Sets
    /// Sets sind übergeordnete Spieleinheiten, die mehrere Legs enthalten
    /// </summary>
    public int Player1Sets
    {
        get => _player1Sets;
        set
        {
            _player1Sets = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    /// <summary>
    /// Anzahl der von Spieler 2 gewonnenen Sets
    /// Sets sind übergeordnete Spieleinheiten, die mehrere Legs enthalten
    /// </summary>
    public int Player2Sets
    {
        get => _player2Sets;
        set
        {
            _player2Sets = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    /// <summary>
    /// Anzahl der von Spieler 1 gewonnenen Legs
    /// Legs sind die grundlegenden Spieleinheiten in Dart (501, 301, etc.)
    /// </summary>
    public int Player1Legs
    {
        get => _player1Legs;
        set
        {
            _player1Legs = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    /// <summary>
    /// Anzahl der von Spieler 2 gewonnenen Legs
    /// Legs sind die grundlegenden Spieleinheiten in Dart (501, 301, etc.)
    /// </summary>
    public int Player2Legs
    {
        get => _player2Legs;
        set
        {
            _player2Legs = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    /// <summary>
    /// Aktueller Status des Matches (Nicht gestartet, Läuft, Beendet)
    /// </summary>
    public MatchStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(StatusDisplay)); // Aktualisiert Status-Anzeige
        }
    }

    /// <summary>
    /// Bracket-Typ: Winner Bracket (Hauptturnier) oder Loser Bracket (Verliererturnier)
    /// Wichtig für Double-Elimination Turniere
    /// </summary>
    public BracketType BracketType
    {
        get => _bracketType;
        set
        {
            _bracketType = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(BracketDisplay)); // Aktualisiert Bracket-Anzeige
        }
    }

    /// <summary>
    /// Turnierrunde (z.B. Viertelfinale, Halbfinale, etc.)
    /// Bestimmt die Position im Turnierablauf
    /// </summary>
    public KnockoutRound Round
    {
        get => _round;
        set
        {
            _round = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(RoundDisplay)); // Aktualisiert Runden-Anzeige
        }
    }

    /// <summary>
    /// Position des Matches innerhalb der Runde
    /// Zur Sortierung und eindeutigen Identifikation
    /// </summary>
    public int Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Zusätzliche Notizen zum Match
    /// Für Kommentare, besondere Umstände, etc.
    /// </summary>
    public string Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Startzeit des Matches
    /// Wird gesetzt wenn das Match beginnt
    /// </summary>
    public DateTime? StartTime
    {
        get => _startTime;
        set
        {
            _startTime = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Endzeit des Matches
    /// Wird automatisch gesetzt wenn das Match beendet wird
    /// </summary>
    public DateTime? EndTime
    {
        get => _endTime;
        set
        {
            _endTime = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Bestimmt ob dieses Match Sets verwendet oder nur Legs zählt
    /// Sets sind übergeordnete Einheiten, die mehrere Legs enthalten
    /// </summary>
    public bool UsesSets
    {
        get => _usesSets;
        set
        {
            _usesSets = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    // Quell-Matches für Bracket-Progression - diese Matches liefern die Spieler

    /// <summary>
    /// Erstes Quell-Match das einen Spieler für dieses Match liefert
    /// Wird für die automatische Bracket-Progression verwendet
    /// </summary>
    public KnockoutMatch? SourceMatch1
    {
        get => _sourceMatch1;
        set
        {
            _sourceMatch1 = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Zweites Quell-Match das einen Spieler für dieses Match liefert
    /// Wird für die automatische Bracket-Progression verwendet
    /// </summary>
    public KnockoutMatch? SourceMatch2
    {
        get => _sourceMatch2;
        set
        {
            _sourceMatch2 = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Gibt an ob Spieler 1 vom Winner Bracket kommt (true) oder Loser Bracket (false)
    /// Wichtig für korrekte Bracket-Progression in Double-Elimination
    /// </summary>
    public bool Player1FromWinner
    {
        get => _player1FromWinner;
        set
        {
            _player1FromWinner = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    /// <summary>
    /// Gibt an ob Spieler 2 vom Winner Bracket kommt (true) oder Loser Bracket (false)
    /// Wichtig für korrekte Bracket-Progression in Double-Elimination
    /// </summary>
    public bool Player2FromWinner
    {
        get => _player2FromWinner;
        set
        {
            _player2FromWinner = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    // Display Properties - Berechnete Eigenschaften für die UI-Anzeige

    /// <summary>
    /// Anzeigename des Matches für die UI
    /// Format: "Spieler1 vs Spieler2" oder "TBD vs TBD" wenn Spieler noch nicht bestimmt
    /// </summary>
    public string DisplayName => $"{Player1?.Name ?? "TBD"} vs {Player2?.Name ?? "TBD"}";

    /// <summary>
    /// Ergebnis-Anzeige für die UI
    /// Zeigt Sets und Legs an wenn Sets verwendet werden, sonst nur Legs
    /// </summary>
    public string ScoreDisplay
    {
        get
        {
            // Zeigt "-:-" wenn Match noch nicht gestartet
            if (Status == MatchStatus.NotStarted) return "-:-";
            
            // Verwendet UsesSets Flag um Format zu bestimmen
            return UsesSets 
                ? $"{Player1Sets}:{Player2Sets} ({Player1Legs}:{Player2Legs})"  // Sets mit Legs in Klammern
                : $"{Player1Legs}:{Player2Legs}";                                // Nur Legs
        }
    }

    /// <summary>
    /// Lokalisierte Status-Anzeige für die UI
    /// Übersetzt den Match-Status in die aktuelle Sprache
    /// </summary>
    public string StatusDisplay => Status switch
    {
        MatchStatus.NotStarted => LocalizationService?.GetString("MatchNotStarted") ?? "Nicht gestartet",
        MatchStatus.InProgress => LocalizationService?.GetString("MatchInProgress") ?? "Läuft",
        MatchStatus.Finished => LocalizationService?.GetString("MatchFinished") ?? "Beendet",
        _ => LocalizationService?.GetString("Unknown") ?? "Unbekannt"
    };

    /// <summary>
    /// Anzeige des Gewinners für die UI
    /// Zeigt den Namen des Gewinners oder leeren String wenn noch kein Gewinner
    /// </summary>
    public string WinnerDisplay => Winner?.Name ?? "";

    /// <summary>
    /// Anzeige des Bracket-Typs für die UI
    /// Übersetzt Winner/Loser Bracket in lesbare Form
    /// </summary>
    public string BracketDisplay => BracketType switch
    {
        BracketType.Winner => "Winner Bracket",
        BracketType.Loser => "Loser Bracket",
        _ => ""
    };

    /// <summary>
    /// Statische Anzeige der Runde für die UI
    /// Übersetzt die Runden-Enumeration in deutsche Bezeichnungen
    /// </summary>
    public string RoundDisplay => Round switch
    {
        KnockoutRound.Best64 => "Beste 64",
        KnockoutRound.Best32 => "Beste 32",
        KnockoutRound.Best16 => "Beste 16",
        KnockoutRound.Quarterfinal => "Viertelfinale",
        KnockoutRound.Semifinal => "Halbfinale",
        KnockoutRound.Final => "Finale",
        KnockoutRound.GrandFinal => "Grand Final",
        KnockoutRound.LoserRound1 => "LR1",
        KnockoutRound.LoserRound2 => "LR2",
        KnockoutRound.LoserRound3 => "LR3",
        KnockoutRound.LoserRound4 => "LR4",
        KnockoutRound.LoserRound5 => "LR5",
        KnockoutRound.LoserRound6 => "LR6",
        KnockoutRound.LoserRound7 => "LR7",
        KnockoutRound.LoserRound8 => "LR8",
        KnockoutRound.LoserRound9 => "LR9",
        KnockoutRound.LoserRound10 => "LR10",
        KnockoutRound.LoserRound11 => "LR11",
        KnockoutRound.LoserRound12 => "LR12",
        KnockoutRound.LoserFinal => "LF",
        _ => ""
    };

    /// <summary>
    /// Dynamische Runden-Anzeige basierend auf Turnier-Kontext
    /// Berechnet die korrekte Rundenbezeichnung abhängig von der Teilnehmerzahl
    /// </summary>
    /// <param name="totalParticipants">Gesamtzahl der Turnierteilnehmer</param>
    /// <param name="localizationService">Lokalisierungsservice für Übersetzungen</param>
    /// <returns>Dynamischer Rundenname basierend auf Kontext</returns>
    public string GetDynamicRoundDisplay(int totalParticipants, LocalizationService? localizationService = null)
    {
        // Umfassende Eingabevalidierung mit Debug-Ausgaben
        if (totalParticipants <= 0)
        {
            System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Invalid totalParticipants = {totalParticipants}");
            return "Keine Teilnehmer definiert";
        }

        if (totalParticipants == 1)
        {
            System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Only one participant = {totalParticipants}");
            return "Nur ein Teilnehmer - Keine K.O.-Phase möglich";
        }

        // Sicherheitscheck gegen extrem große Teilnehmerzahlen
        if (totalParticipants > 1024)
        {
            System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Too many participants = {totalParticipants}");
            return "Zu viele Teilnehmer";
        }

        try
        {
            // Loser Bracket Runden werden separat behandelt - haben feste Namen
            if (BracketType == BracketType.Loser)
            {
                return Round switch
                {
                    KnockoutRound.LoserRound1 => "Loser Runde 1",
                    KnockoutRound.LoserRound2 => "Loser Runde 2",
                    KnockoutRound.LoserRound3 => "Loser Runde 3",
                    KnockoutRound.LoserRound4 => "Loser Runde 4",
                    KnockoutRound.LoserRound5 => "Loser Runde 5",
                    KnockoutRound.LoserRound6 => "Loser Runde 6",
                    KnockoutRound.LoserRound7 => "Loser Runde 7",
                    KnockoutRound.LoserRound8 => "Loser Runde 8",
                    KnockoutRound.LoserRound9 => "Loser Runde 9",
                    KnockoutRound.LoserRound10 => "Loser Runde 10",
                    KnockoutRound.LoserRound11 => "Loser Runde 11",
                    KnockoutRound.LoserRound12 => "Loser Runde 12",
                    KnockoutRound.LoserFinal => "Loser Finale",
                    _ => RoundDisplay // Fallback auf statische Anzeige
                };
            }

            // Winner Bracket: Berechne erforderliche Bracket-Größe (nächste Zweierpotenz)
            int bracketSize = 1;
            int iterations = 0;
            
            // Finde die nächste Zweierpotenz >= totalParticipants
            // Sicherheitscheck gegen Endlosschleife
            while (bracketSize < totalParticipants && iterations < 20)
            {
                bracketSize *= 2;
                iterations++;
            }
            
            // Fallback wenn die Schleife zu lange läuft
            if (iterations >= 20)
            {
                System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Infinite loop prevented for totalParticipants = {totalParticipants}");
                return "Fehler: Berechnung fehlgeschlagen";
            }

            // Spezialbehandlung für Grand Final
            if (Round == KnockoutRound.GrandFinal)
            {
                return localizationService?.GetString("GrandFinal") ?? "Grand Final";
            }

            // Berechne welche "logische Runde" diese Enum-Runde im Kontext des Turniers repräsentiert
            int logicalRound = Round switch
            {
                KnockoutRound.Best64 => GetBest64LogicalRound(bracketSize),
                KnockoutRound.Best32 => GetBest32LogicalRound(bracketSize),
                KnockoutRound.Best16 => GetBest16LogicalRound(bracketSize),
                KnockoutRound.Quarterfinal => GetQuarterfinalLogicalRound(bracketSize),
                KnockoutRound.Semifinal => GetSemifinalLogicalRound(bracketSize),
                KnockoutRound.Final => GetFinalLogicalRound(bracketSize),
                _ => 1 // Default Wert
            };

            // Verwende die statische Methode für die finale Namensauflösung
            return GetKnockoutRoundName(bracketSize, logicalRound, localizationService);
        }
        catch (Exception ex)
        {
            // Fehlerbehandlung mit Debug-Ausgabe
            System.Diagnostics.Debug.WriteLine($"GetDynamicRoundDisplay: Exception = {ex.Message}");
            return "Fehler bei Rundenberechnung";
        }
    }
    
    // Hilfsmethoden zur Berechnung der logischen Runden basierend auf Bracket-Größe
    
    /// <summary>
    /// Berechnet die logische Runde für "Beste 64" basierend auf der Bracket-Größe
    /// </summary>
    private static int GetBest64LogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 5); // Best64 ist 6 Runden vor dem Ende
    }
    
    /// <summary>
    /// Berechnet die logische Runde für "Beste 32" basierend auf der Bracket-Größe
    /// </summary>
    private static int GetBest32LogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 4); // Best32 ist 5 Runden vor dem Ende
    }
    
    /// <summary>
    /// Berechnet die logische Runde für "Beste 16" basierend auf der Bracket-Größe
    /// </summary>
    private static int GetBest16LogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 3); // Best16 ist 4 Runden vor dem Ende
    }
    
    /// <summary>
    /// Berechnet die logische Runde für das Viertelfinale basierend auf der Bracket-Größe
    /// </summary>
    private static int GetQuarterfinalLogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 2); // Viertelfinale ist 3 Runden vor dem Ende
    }
    
    /// <summary>
    /// Berechnet die logische Runde für das Halbfinale basierend auf der Bracket-Größe
    /// </summary>
    private static int GetSemifinalLogicalRound(int bracketSize)
    {
        var totalRounds = (int)Math.Log2(bracketSize);
        return Math.Max(1, totalRounds - 1); // Halbfinale ist 2 Runden vor dem Ende
    }
    
    /// <summary>
    /// Berechnet die logische Runde für das Finale basierend auf der Bracket-Größe
    /// </summary>
    private static int GetFinalLogicalRound(int bracketSize)
    {
        return (int)Math.Log2(bracketSize); // Finale ist die letzte Runde
    }

    /// <summary>
    /// Setzt das Ergebnis des Matches und bestimmt automatisch Gewinner und Verlierer
    /// Aktualisiert auch Status und Endzeit
    /// </summary>
    /// <param name="player1Sets">Gewonnene Sets von Spieler 1</param>
    /// <param name="player2Sets">Gewonnene Sets von Spieler 2</param>
    /// <param name="player1Legs">Gewonnene Legs von Spieler 1</param>
    /// <param name="player2Legs">Gewonnene Legs von Spieler 2</param>
    public void SetResult(int player1Sets, int player2Sets, int player1Legs, int player2Legs)
    {
        // Bestimme automatisch ob Sets verwendet werden basierend auf Eingabe
        UsesSets = player1Sets > 0 || player2Sets > 0;
        
        // Setze die Ergebniswerte
        Player1Sets = player1Sets;
        Player2Sets = player2Sets;
        Player1Legs = player1Legs;
        Player2Legs = player2Legs;

        // Bestimme Gewinner und Verlierer basierend auf UsesSets Flag
        if (UsesSets)
        {
            // Bei Sets: Erst Sets vergleichen, bei Gleichstand Legs
            if (player1Sets > player2Sets)
            {
                Winner = Player1;
                Loser = Player2;
            }
            else if (player2Sets > player1Sets)
            {
                Winner = Player2;
                Loser = Player1;
            }
            else if (player1Legs > player2Legs) // Sets gleich, Legs entscheiden
            {
                Winner = Player1;
                Loser = Player2;
            }
            else if (player2Legs > player1Legs)
            {
                Winner = Player2;
                Loser = Player1;
            }
        }
        else
        {
            // Ohne Sets: Nur Legs zählen
            if (player1Legs > player2Legs)
            {
                Winner = Player1;
                Loser = Player2;
            }
            else if (player2Legs > player1Legs)
            {
                Winner = Player2;
                Loser = Player1;
            }
        }

        // Setze Match als beendet und erfasse Endzeit
        Status = MatchStatus.Finished;
        EndTime = DateTime.Now;
        
        // Wichtige PropertyChanged Events für komplette UI-Aktualisierung
        // Diese stellen sicher, dass alle abhängigen UI-Eigenschaften aktualisiert werden
        OnPropertyChanged(nameof(ScoreDisplay));
        OnPropertyChanged(nameof(StatusDisplay));
        OnPropertyChanged(nameof(WinnerDisplay));
        OnPropertyChanged(nameof(BracketDisplay));
        OnPropertyChanged(nameof(RoundDisplay));
    }

    // INotifyPropertyChanged Implementation für WPF Data Binding
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Löst PropertyChanged Event aus für UI-Aktualisierung
    /// CallerMemberName Attribut sorgt für automatische Eigenschaftserkennung
    /// </summary>
    /// <param name="propertyName">Name der geänderten Eigenschaft (automatisch erkannt)</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Statische Methode: Gibt den korrekten deutschen K.O.-Rundennamen basierend auf Teilnehmerzahl zurück
    /// Berechnet dynamisch die korrekte Bezeichnung abhängig von der Turniergröße
    /// </summary>
    /// <param name="totalParticipants">Gesamtzahl der Teilnehmer in der K.O.-Phase</param>
    /// <param name="currentRound">Aktuelle Rundennummer (1-basiert)</param>
    /// <param name="localizationService">Lokalisierungsservice für Übersetzungen</param>
    /// <returns>Korrekte Rundenbezeichnung in aktueller Sprache</returns>
    public static string GetKnockoutRoundName(int totalParticipants, int currentRound, LocalizationService? localizationService = null)
    {
        // Eingabevalidierung
        if (totalParticipants <= 0)
        {
            return "Fehler: Keine Teilnehmer";
        }

        if (currentRound <= 0)
        {
            return "Fehler: Ungültige Runde";
        }

        // Berechne Gesamtzahl der benötigten Runden
        var totalRounds = (int)Math.Ceiling(Math.Log2(totalParticipants));
        
        // Validiere Rundenzahl
        if (currentRound > totalRounds)
        {
            return "Fehler: Runde außerhalb des Turniers";
        }

        // Berechne verbleibende Spieler zu Beginn der aktuellen Runde
        var playersInRound = totalParticipants / (int)Math.Pow(2, currentRound - 1);

        // Bestimme Rundenname basierend auf Spielerzahl in der Runde
        string roundKey = playersInRound switch
        {
            64 => "Best64",       // Beste 64
            32 => "Best32",       // Beste 32
            16 => "Best16",       // Beste 16
            8 => "Best8",         // Viertelfinale  
            4 => "Best4",         // Halbfinale
            2 => "Final",         // Finale
            _ => "LastOfRound"    // Allgemeine Runde X
        };

        // Spezialfall: Finale ist immer die letzte Runde
        if (currentRound == totalRounds)
        {
            roundKey = "Final";
        }

        // Hole übersetzten Namen vom LocalizationService
        if (localizationService != null)
        {
            if (roundKey == "LastOfRound")
            {
                // Für allgemeine Runden mit Rundennummer
                return localizationService.GetString(roundKey, currentRound);
            }
            return localizationService.GetString(roundKey);
        }

        // Fallback auf deutsche Standardnamen
        return roundKey switch
        {
            "Best64" => "Beste 64",
            "Best32" => "Beste 32",
            "Best16" => "Beste 16",
            "Best8" => "Viertelfinale",
            "Best4" => "Halbfinale",
            "Final" => "Finale",
            _ => $"Runde {currentRound}"
        };
    }

    /// <summary>
    /// Validiert ob eine K.O.-Phase mit der gegebenen Teilnehmeranzahl gestartet werden kann
    /// </summary>
    /// <param name="totalParticipants">Anzahl der Teilnehmer</param>
    /// <returns>True wenn K.O.-Phase möglich ist, sonst false</returns>
    public static bool CanStartKnockoutPhase(int totalParticipants)
    {
        return totalParticipants > 1; // Mindestens 2 Teilnehmer erforderlich
    }

    /// <summary>
    /// Gibt eine Fehlermeldung zurück, falls die K.O.-Phase nicht gestartet werden kann
    /// Zur Validierung vor Turnierbeginn
    /// </summary>
    /// <param name="totalParticipants">Anzahl der Teilnehmer</param>
    /// <returns>Fehlermeldung oder null wenn alles in Ordnung ist</returns>
    public static string? ValidateKnockoutPhaseStart(int totalParticipants)
    {
        if (totalParticipants <= 0)
            return "Keine Teilnehmer für K.O.-Phase qualifiziert";
        
        if (totalParticipants == 1)
            return "Nur ein Teilnehmer qualifiziert - K.O.-Phase nicht möglich";
        
        return null; // Alles in Ordnung - K.O.-Phase kann starten
    }

    /// <summary>
    /// Statische Methode: Aktualisiert Loser Bracket Matches mit eliminierten Spielern aus dem Winner Bracket
    /// Wird aufgerufen wenn ein Winner Bracket Match beendet wird, um den Verlierer ins Loser Bracket zu verschieben
    /// </summary>
    /// <param name="completedWinnerMatch">Das beendete Winner Bracket Match</param>
    /// <param name="loserBracket">Alle Loser Bracket Matches</param>
    public static void UpdateLoserBracketFromWinnerMatch(KnockoutMatch completedWinnerMatch, IEnumerable<KnockoutMatch> loserBracket)
    {
        // Validierung: Nur Winner Bracket Matches mit Verlierern verarbeiten
        if (completedWinnerMatch.BracketType != BracketType.Winner || completedWinnerMatch.Loser == null)
            return;

        // Finde Loser Bracket Matches, die diesen eliminierten Spieler erhalten sollen
        var targetLoserMatches = loserBracket
            .Where(lm => lm.SourceMatch1 == completedWinnerMatch || lm.SourceMatch2 == completedWinnerMatch)
            .ToList();

        // Setze den eliminierten Spieler in die entsprechenden Loser Bracket Matches
        foreach (var loserMatch in targetLoserMatches)
        {
            if (loserMatch.SourceMatch1 == completedWinnerMatch && !loserMatch.Player1FromWinner)
            {
                // Verlierer wird Spieler 1 im Loser Match
                loserMatch.Player1 = completedWinnerMatch.Loser;
            }
            else if (loserMatch.SourceMatch2 == completedWinnerMatch && !loserMatch.Player2FromWinner)
            {
                // Verlierer wird Spieler 2 im Loser Match
                loserMatch.Player2 = completedWinnerMatch.Loser;
            }
        }
    }

    /// <summary>
    /// Statische Methode: Aktualisiert nachfolgende Runden-Matches mit Gewinnern aus beendeten Matches
    /// Funktioniert für beide Brackets (Winner und Loser)
    /// </summary>
    /// <param name="completedMatch">Das beendete Match</param>
    /// <param name="allMatches">Alle Matches im Turnier</param>
    public static void UpdateNextRoundFromCompletedMatch(KnockoutMatch completedMatch, IEnumerable<KnockoutMatch> allMatches)
    {
        // Überspringe wenn kein Gewinner vorhanden
        if (completedMatch.Winner == null) return;

        // Finde nachfolgende Matches, die den Gewinner erhalten sollen
        var nextRoundMatches = allMatches
            .Where(m => m.SourceMatch1 == completedMatch || m.SourceMatch2 == completedMatch)
            .ToList();

        // Setze den Gewinner in die entsprechenden nachfolgenden Matches
        foreach (var nextMatch in nextRoundMatches)
        {
            if (nextMatch.SourceMatch1 == completedMatch && nextMatch.Player1FromWinner)
            {
                // Gewinner wird Spieler 1 im nächsten Match
                nextMatch.Player1 = completedMatch.Winner;
            }
            else if (nextMatch.SourceMatch2 == completedMatch && nextMatch.Player2FromWinner)
            {
                // Gewinner wird Spieler 2 im nächsten Match
                nextMatch.Player2 = completedMatch.Winner;
            }
        }
    }
}

