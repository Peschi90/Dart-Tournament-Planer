using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DartTournamentPlaner.Models;

/// <summary>
/// Repräsentiert einen Dart-Spieler im Turniersystem
/// Implementiert INotifyPropertyChanged für automatische UI-Updates bei Eigenschaftsänderungen
/// </summary>
public class Player : INotifyPropertyChanged
{
    // Private Backing-Fields für die Eigenschaften
    private string _name = string.Empty;  // Name des Spielers
    private int _id;                      // Eindeutige ID des Spielers
    private string? _email;               // E-Mail-Adresse des Spielers (optional)

    /// <summary>
    /// Standard-Konstruktor ohne Parameter
    /// Wird hauptsächlich für Serialisierung/Deserialisierung benötigt
    /// </summary>
    public Player()
    {
        // Parameterloser Konstruktor für JSON/XML-Serialisierung
    }

    /// <summary>
    /// Konstruktor mit Parametern zur direkten Initialisierung
    /// </summary>
    /// <param name="id">Eindeutige Identifikations-ID des Spielers</param>
    /// <param name="name">Name des Spielers</param>
    public Player(int id, string name)
    {
        _id = id;
        _name = name;
    }

    /// <summary>
    /// Konstruktor mit allen Parametern zur direkten Initialisierung
    /// </summary>
    /// <param name="id">Eindeutige Identifikations-ID des Spielers</param>
    /// <param name="name">Name des Spielers</param>
    /// <param name="email">E-Mail-Adresse des Spielers (optional)</param>
    public Player(int id, string name, string? email)
    {
        _id = id;
        _name = name;
        _email = email;
    }

    /// <summary>
    /// Eindeutige Identifikations-ID des Spielers
    /// Wird zur internen Referenzierung und Datenbankverknüpfung verwendet
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
    /// Name des Spielers
    /// Wird in der UI und für die Anzeige in Matches verwendet
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
    /// E-Mail-Adresse des Spielers (optional)
    /// Kann für Benachrichtigungen oder Kontaktinformationen verwendet werden
    /// </summary>
    public string? Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged(); // Benachrichtigt UI über Änderung
        }
    }

    // INotifyPropertyChanged Implementation für WPF Data Binding
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Löst das PropertyChanged Event aus um die UI über Eigenschaftsänderungen zu informieren
    /// CallerMemberName Attribut erkennt automatisch den Namen der aufrufenden Eigenschaft
    /// </summary>
    /// <param name="propertyName">Name der geänderten Eigenschaft (wird automatisch erkannt)</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Überschreibt ToString() um eine sinnvolle Textdarstellung des Spielers zu liefern
    /// Wird in Dropdown-Listen und Debug-Ausgaben verwendet
    /// </summary>
    /// <returns>Name des Spielers als String-Repräsentation</returns>
    public override string ToString()
    {
        return Name;
    }
}