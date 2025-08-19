using System.ComponentModel;
using System.Runtime.CompilerServices;
using DartTournamentPlaner.Services;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Enumeration f�r den Status eines Matches
/// Definiert alle m�glichen Zust�nde eines Dart-Spiels
/// </summary>
public enum MatchStatus
{
    NotStarted, // Match wurde noch nicht begonnen
    InProgress, // Match l�uft gerade
    Finished,   // Match wurde beendet
    Bye         // Freilos (ein Spieler fehlt)
}

/// <summary>
/// Repr�sentiert ein einzelnes Dart-Match in der Gruppenphase
/// Implementiert INotifyPropertyChanged f�r automatische UI-Updates
/// Unterscheidet sich von KnockoutMatch durch einfachere Struktur f�r Gruppenspiele
/// </summary>
public class Match : INotifyPropertyChanged
{
    // Private Backing-Fields f�r alle Eigenschaften
    private int _id;                        // Eindeutige ID des Matches
    private Player? _player1;               // Erster Spieler
    private Player? _player2;               // Zweiter Spieler (kann null bei Freilos sein)
    private int _player1Sets = 0;           // Gewonnene Sets von Spieler 1
    private int _player2Sets = 0;           // Gewonnene Sets von Spieler 2
    private int _player1Legs = 0;           // Gewonnene Legs von Spieler 1
    private int _player2Legs = 0;           // Gewonnene Legs von Spieler 2
    private MatchStatus _status = MatchStatus.NotStarted; // Aktueller Match-Status
    private Player? _winner;                // Gewinner des Matches
    private bool _isBye = false;            // Ist dieses Match ein Freilos?
    private DateTime? _startTime;           // Startzeit des Matches
    private DateTime? _endTime;             // Endzeit des Matches
    private string _notes = string.Empty;   // Zus�tzliche Notizen
    private bool _usesSets = false;         // Verwendet dieses Match Sets oder nur Legs?

    // Statische Referenz zum Lokalisierungsservice (wird von App.xaml.cs gesetzt)
    public static LocalizationService? LocalizationService { get; set; }

    /// <summary>
    /// Eindeutige Identifikations-ID des Matches
    /// Wird zur internen Referenzierung verwendet
    /// </summary>
    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
        }
    }

    /// <summary>
    /// Erster Spieler im Match
    /// Ist immer gesetzt, auch bei Freilos
    /// </summary>
    public Player? Player1
    {
        get => _player1;
        set
        {
            _player1 = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(DisplayName)); // Aktualisiert auch Display-Name
        }
    }

    /// <summary>
    /// Zweiter Spieler im Match
    /// Ist null wenn es sich um ein Freilos handelt
    /// </summary>
    public Player? Player2
    {
        get => _player2;
        set
        {
            _player2 = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(DisplayName)); // Aktualisiert auch Display-Name
            UpdateByeStatus(); // Pr�ft ob es sich um ein Freilos handelt
        }
    }

    /// <summary>
    /// Anzahl der von Spieler 1 gewonnenen Sets
    /// Sets sind �bergeordnete Spieleinheiten
    /// </summary>
    public int Player1Sets
    {
        get => _player1Sets;
        set
        {
            _player1Sets = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    /// <summary>
    /// Anzahl der von Spieler 2 gewonnenen Sets
    /// Sets sind �bergeordnete Spieleinheiten
    /// </summary>
    public int Player2Sets
    {
        get => _player2Sets;
        set
        {
            _player2Sets = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    /// <summary>
    /// Anzahl der von Spieler 1 gewonnenen Legs
    /// Legs sind die grundlegenden Spieleinheiten (501, 301, etc.)
    /// </summary>
    public int Player1Legs
    {
        get => _player1Legs;
        set
        {
            _player1Legs = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    /// <summary>
    /// Anzahl der von Spieler 2 gewonnenen Legs
    /// Legs sind die grundlegenden Spieleinheiten (501, 301, etc.)
    /// </summary>
    public int Player2Legs
    {
        get => _player2Legs;
        set
        {
            _player2Legs = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    /// <summary>
    /// Aktueller Status des Matches
    /// Bestimmt wie das Match in der UI dargestellt wird
    /// </summary>
    public MatchStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(StatusDisplay)); // Aktualisiert Status-Anzeige
        }
    }

    /// <summary>
    /// Gewinner des Matches
    /// Wird automatisch bei Ergebniseingabe gesetzt
    /// </summary>
    public Player? Winner
    {
        get => _winner;
        set
        {
            _winner = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(WinnerDisplay)); // Aktualisiert Gewinner-Anzeige
        }
    }

    /// <summary>
    /// Gibt an ob dieses Match ein Freilos ist
    /// Freilos bedeutet, dass nur ein Spieler verf�gbar ist
    /// </summary>
    public bool IsBye
    {
        get => _isBye;
        set
        {
            _isBye = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            if (value)
            {
                Status = MatchStatus.Bye; // Setzt Status automatisch auf Freilos
            }
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
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
        }
    }

    /// <summary>
    /// Endzeit des Matches
    /// Wird automatisch bei Ergebniseingabe gesetzt
    /// </summary>
    public DateTime? EndTime
    {
        get => _endTime;
        set
        {
            _endTime = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
        }
    }

    /// <summary>
    /// Zus�tzliche Notizen zum Match
    /// F�r besondere Umst�nde oder Kommentare
    /// </summary>
    public string Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
        }
    }

    /// <summary>
    /// Bestimmt ob dieses Match Sets verwendet oder nur Legs z�hlt
    /// Beeinflusst wie das Ergebnis angezeigt und ausgewertet wird
    /// </summary>
    public bool UsesSets
    {
        get => _usesSets;
        set
        {
            _usesSets = value;
            OnPropertyChanged(); // Benachrichtigt UI �ber �nderung
            OnPropertyChanged(nameof(ScoreDisplay)); // Aktualisiert Ergebnis-Anzeige
        }
    }

    // Display Properties - Berechnete Eigenschaften f�r die UI-Anzeige

    /// <summary>
    /// Anzeigename des Matches f�r die UI
    /// Zeigt "Spieler vs Spieler" oder "Spieler - Freilos" bei Freilos
    /// </summary>
    public string DisplayName => IsBye 
        ? $"{Player1?.Name ?? "TBD"} - {LocalizationService?.GetString("MatchBye") ?? "Freilos"}" 
        : $"{Player1?.Name ?? "TBD"} vs {Player2?.Name ?? "TBD"}";

    /// <summary>
    /// Ergebnis-Anzeige f�r die UI
    /// Format h�ngt davon ab ob Sets verwendet werden
    /// </summary>
    public string ScoreDisplay
    {
        get
        {
            if (IsBye) return LocalizationService?.GetString("MatchBye") ?? "Freilos";
            if (Status == MatchStatus.NotStarted) return "-:-";
            
            // Verwendet UsesSets Flag um Anzeige-Format zu bestimmen
            return UsesSets 
                ? $"{Player1Sets}:{Player2Sets} ({Player1Legs}:{Player2Legs})" // Sets mit Legs in Klammern
                : $"{Player1Legs}:{Player2Legs}";                              // Nur Legs
        }
    }

    /// <summary>
    /// Lokalisierte Status-Anzeige f�r die UI
    /// �bersetzt den Match-Status in die aktuelle Sprache
    /// </summary>
    public string StatusDisplay => Status switch
    {
        MatchStatus.NotStarted => LocalizationService?.GetString("MatchNotStarted") ?? "Nicht gestartet",
        MatchStatus.InProgress => LocalizationService?.GetString("MatchInProgress") ?? "L�uft",
        MatchStatus.Finished => LocalizationService?.GetString("MatchFinished") ?? "Beendet",
        MatchStatus.Bye => LocalizationService?.GetString("MatchBye") ?? "Freilos",
        _ => LocalizationService?.GetString("Unknown") ?? "Unbekannt"
    };

    /// <summary>
    /// Anzeige des Gewinners f�r die UI
    /// Bei Freilos wird automatisch Player1 als Gewinner angezeigt
    /// </summary>
    public string WinnerDisplay => Winner?.Name ?? (IsBye ? Player1?.Name ?? "" : "");

    /// <summary>
    /// Private Methode: Aktualisiert den Freilos-Status basierend auf verf�gbaren Spielern
    /// Wird automatisch aufgerufen wenn Player2 gesetzt wird
    /// </summary>
    private void UpdateByeStatus()
    {
        IsBye = Player2 == null; // Freilos wenn kein zweiter Spieler vorhanden
    }

    /// <summary>
    /// Erweiterte SetResult-Methode mit Sets-Parameter
    /// Setzt das Ergebnis des Matches und bestimmt automatisch den Gewinner
    /// </summary>
    /// <param name="player1Sets">Gewonnene Sets von Spieler 1</param>
    /// <param name="player2Sets">Gewonnene Sets von Spieler 2</param>
    /// <param name="player1Legs">Gewonnene Legs von Spieler 1</param>
    /// <param name="player2Legs">Gewonnene Legs von Spieler 2</param>
    /// <param name="usesSets">Ob Sets verwendet werden sollen</param>
    public void SetResult(int player1Sets, int player2Sets, int player1Legs, int player2Legs, bool usesSets = false)
    {
        UsesSets = usesSets; // Setzt den Anzeigemodus
        
        // Setze die Ergebniswerte
        Player1Sets = player1Sets;
        Player2Sets = player2Sets;
        Player1Legs = player1Legs;
        Player2Legs = player2Legs;

        // Bestimme Gewinner basierend auf Sets oder Legs
        if (UsesSets)
        {
            // Bei Sets: Zuerst Sets vergleichen, dann Legs bei Gleichstand
            if (player1Sets > player2Sets)
            {
                Winner = Player1;
            }
            else if (player2Sets > player1Sets)
            {
                Winner = Player2;
            }
            else if (player1Legs > player2Legs) // Sets gleich, Legs entscheiden
            {
                Winner = Player1;
            }
            else if (player2Legs > player1Legs)
            {
                Winner = Player2;
            }
        }
        else
        {
            // Ohne Sets: Nur Legs z�hlen
            if (player1Legs > player2Legs)
            {
                Winner = Player1;
            }
            else if (player2Legs > player1Legs)
            {
                Winner = Player2;
            }
        }

        // Setze Match als beendet und erfasse Endzeit
        Status = MatchStatus.Finished;
        EndTime = DateTime.Now;
        
        // Wichtige PropertyChanged Events f�r komplette UI-Aktualisierung
        // Diese stellen sicher, dass alle abh�ngigen UI-Eigenschaften aktualisiert werden
        OnPropertyChanged(nameof(ScoreDisplay));
        OnPropertyChanged(nameof(StatusDisplay));
        OnPropertyChanged(nameof(WinnerDisplay));
    }

    /// <summary>
    /// Legacy-�berladung f�r R�ckw�rtskompatibilit�t
    /// Bestimmt automatisch ob Sets verwendet werden basierend auf den Werten
    /// </summary>
    /// <param name="player1Sets">Gewonnene Sets von Spieler 1</param>
    /// <param name="player2Sets">Gewonnene Sets von Spieler 2</param>
    /// <param name="player1Legs">Gewonnene Legs von Spieler 1</param>
    /// <param name="player2Legs">Gewonnene Legs von Spieler 2</param>
    public void SetResult(int player1Sets, int player2Sets, int player1Legs, int player2Legs)
    {
        // Bestimme Sets-Verwendung basierend auf ob die Werte gr��er als 0 sind
        bool usesSets = player1Sets > 0 || player2Sets > 0;
        SetResult(player1Sets, player2Sets, player1Legs, player2Legs, usesSets);
    }

    // INotifyPropertyChanged Implementation f�r WPF Data Binding
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// L�st PropertyChanged Event aus f�r UI-Aktualisierung
    /// CallerMemberName Attribut sorgt f�r automatische Eigenschaftserkennung
    /// </summary>
    /// <param name="propertyName">Name der ge�nderten Eigenschaft (automatisch erkannt)</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    /// <summary>
    /// �ffentliche Version von OnPropertyChanged f�r externe Aufrufe
    /// Erm�glicht anderen Klassen das Ausl�sen von PropertyChanged Events
    /// </summary>
    /// <param name="propertyName">Name der Eigenschaft die sich ge�ndert hat</param>
    public void ForcePropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
    }
}