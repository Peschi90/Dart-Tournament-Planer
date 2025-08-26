# Dart Tournament Planner API - Vollst�ndige Integration

Diese neue Funktionalit�t erweitert den Dart Tournament Planner um eine REST API, die es erm�glicht, Turnierdaten extern zu verwalten und Match-Ergebnisse von Webseiten oder Apps einzutragen.

## ?? Neue Features

### ?? REST API Integration
- **Separates API-Projekt**: `DartTournamentPlaner.API` als eigenst�ndige ASP.NET Core Web API
- **Strukturierte Architektur**: Saubere Trennung zwischen WPF-Anwendung und API
- **Live-Datenintegration**: API arbeitet direkt mit den aktuellen Turnierdaten der Hauptanwendung

### ?? API-Funktionen
- **Turnierdaten abrufen**: GET `/api/tournaments/current` - Holt aktuelle Live-Turnierdaten
- **Match-Ergebnisse eintragen**: PUT `/api/matches/{matchId}` - Aktualisiert Match-Ergebnisse
- **Ausstehende Matches**: GET `/api/matches/pending` - Zeigt alle offenen Matches
- **Real-Time Updates**: SignalR Hub f�r Live-Updates zwischen API und Anwendung
- **Demo Interface**: Benutzerfreundliche Web-Oberfl�che unter `/demo`

### ??? WPF-Integration
- **API-Men�**: Neuer Men�punkt "?? API" in der Hauptanwendung
- **Start/Stop Funktionen**: API direkt aus der Anwendung heraus starten und stoppen
- **Automatische Dokumentation**: Swagger/OpenAPI Dokumentation unter http://localhost:5000
- **Status-Anzeige**: Live-Status der API in der Statusleiste

## ?? Schnellstart

### Option 1: �ber die WPF-Anwendung (Empfohlen)
1. �ffnen Sie den Dart Tournament Planer
2. Gehen Sie zu **API ? API starten**
3. Die API startet automatisch auf Port 5000
4. �ffnen Sie **API ? API Dokumentation** f�r die Swagger-UI

### Option 2: Manuell �ber Batch-Datei
1. Doppelklicken Sie auf `start-api.bat` im Projektordner
2. Die API startet in einem separaten Konsolenfenster
3. �ffnen Sie http://localhost:5000 f�r die API-Dokumentation
4. �ffnen Sie http://localhost:5000/demo f�r das Demo-Interface

### Option 3: �ber Kommandozeile
```bash
cd DartTournamentPlaner.API
dotnet run --urls "http://localhost:5000"
```

## ?? Verwendung

### API-Dokumentation aufrufen
- **Swagger UI**: http://localhost:5000
- **Demo Interface**: http://localhost:5000/demo
- **Health Check**: http://localhost:5000/health

### Demo Interface verwenden
1. �ffnen Sie http://localhost:5000/demo
2. Das Interface zeigt automatisch:
   - Aktuelle Turnierdaten
   - Liste aller ausstehenden Matches
   - Formular zur Match-Ergebnis-Eingabe
3. W�hlen Sie ein Match aus der Liste
4. Geben Sie das Ergebnis ein und speichern Sie es

### Match-Ergebnisse von extern eintragen

#### �ber das Demo Interface (Benutzerfreundlich)
1. �ffnen Sie http://localhost:5000/demo
2. W�hlen Sie ein Match aus der "Ausstehende Matches" Liste
3. Geben Sie Sets und Legs f�r beide Spieler ein
4. Klicken Sie "Ergebnis speichern"

#### �ber API-Aufrufe (Programmtisch)
```bash
# Beispiel: Match-Ergebnis �ber API eintragen
curl -X PUT "http://localhost:5000/api/matches/123" \
  -H "Content-Type: application/json" \
  -d '{
    "player1Sets": 3,
    "player2Sets": 1,
    "player1Legs": 9,
    "player2Legs": 6,
    "notes": "Eingegeben �ber API"
  }'
```

## ??? Technische Details

### Projekt-Struktur
```
??? DartTournamentPlaner/              # Hauptanwendung (WPF)
?   ??? Services/
?   ?   ??? ApiIntegrationService.cs   # Basis API-Integration
?   ?   ??? HttpApiIntegrationService.cs # Erweiterte HTTP-Integration
?   ??? MainWindow.xaml.cs             # API-Men� Integration
??? DartTournamentPlaner.API/          # API-Projekt
?   ??? Controllers/                   # REST API Endpoints
?   ?   ??? TournamentsController.cs   # Turnier-Endpoints
?   ?   ??? MatchesController.cs       # Match-Endpoints
?   ??? Models/                        # DTOs und Response-Modelle
?   ?   ??? TournamentDtos.cs          # Datenmodelle
?   ?   ??? ApiResponse.cs             # Antwort-Wrapper
?   ??? Services/                      # Business Logic
?   ?   ??? TournamentApiService.cs    # Turnier-Service
?   ?   ??? MatchApiService.cs         # Match-Service
?   ?   ??? TournamentSyncService.cs   # Synchronisation
?   ?   ??? ApiDbContext.cs            # Datenbank-Context
?   ??? Hubs/                          # SignalR Real-Time Communication
?   ?   ??? TournamentHub.cs           # Real-Time Hub
?   ??? wwwroot/                       # Statische Dateien
?   ?   ??? demo.html                  # Demo Interface
?   ??? Program.cs                     # API-Konfiguration
??? start-api.bat                      # Batch-Datei zum manuellen Start
??? API_INTEGRATION.md                 # Diese Dokumentation
```

### API Endpoints

#### Turniere
- `GET /api/tournaments/current` - Aktuelle Turnierdaten
- `GET /api/tournaments/status` - API-Status
- `GET /api/tournaments/{id}` - Spezifisches Turnier

#### Matches
- `GET /api/matches/pending` - Alle ausstehenden Matches
- `PUT /api/matches/{matchId}` - Match-Ergebnis aktualisieren
- `GET /api/tournaments/{tournamentId}/classes/{classId}/matches/{matchId}` - Spezifisches Match
- `POST /api/tournaments/{tournamentId}/classes/{classId}/matches/{matchId}/reset` - Match zur�cksetzen

#### Verwaltung
- `GET /health` - API-Gesundheitsstatus
- `GET /demo` - Demo-Interface (Redirect zu /demo.html)

#### Real-Time
- `/tournamentHub` - SignalR Hub f�r Live-Updates

### Datenmodelle

#### MatchResultDto
```json
{
  "matchId": 123,
  "player1Sets": 3,
  "player2Sets": 1,
  "player1Legs": 9,
  "player2Legs": 6,
  "notes": "Optional notes"
}
```

#### ApiResponse<T>
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* actual data */ },
  "errors": []
}
```

## ?? Erweiterte Anwendungsf�lle

### 1. Webseite f�r Spieler
Erstellen Sie eine einfache HTML-Seite, die Spielern erm�glicht, ihre Match-Ergebnisse selbst einzutragen:

```html
<!DOCTYPE html>
<html>
<head>
    <title>Dart Tournament - Match Results</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        input { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px; }
        button { background: #007bff; color: white; padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer; }
        button:hover { background: #0056b3; }
        .status { padding: 10px; margin: 10px 0; border-radius: 4px; }
        .success { background: #d4edda; color: #155724; }
        .error { background: #f8d7da; color: #721c24; }
    </style>
</head>
<body>
    <h1>?? Dart Tournament - Match Result Entry</h1>
    
    <div id="status"></div>
    
    <form id="resultForm">
        <div class="form-group">
            <label for="matchId">Match ID:</label>
            <input type="number" id="matchId" required>
        </div>
        
        <div class="form-group">
            <label for="player1Sets">Player 1 - Sets:</label>
            <input type="number" id="player1Sets" min="0" value="0">
        </div>
        
        <div class="form-group">
            <label for="player2Sets">Player 2 - Sets:</label>
            <input type="number" id="player2Sets" min="0" value="0">
        </div>
        
        <div class="form-group">
            <label for="player1Legs">Player 1 - Legs:</label>
            <input type="number" id="player1Legs" min="0" value="0">
        </div>
        
        <div class="form-group">
            <label for="player2Legs">Player 2 - Legs:</label>
            <input type="number" id="player2Legs" min="0" value="0">
        </div>
        
        <button type="submit">Submit Result</button>
    </form>
    
    <script>
        document.getElementById('resultForm').onsubmit = async (e) => {
            e.preventDefault();
            const statusDiv = document.getElementById('status');
            
            const formData = new FormData(e.target);
            const data = {
                matchId: parseInt(formData.get('matchId')),
                player1Sets: parseInt(formData.get('player1Sets')),
                player2Sets: parseInt(formData.get('player2Sets')),
                player1Legs: parseInt(formData.get('player1Legs')),
                player2Legs: parseInt(formData.get('player2Legs'))
            };
            
            try {
                const response = await fetch(`http://localhost:5000/api/matches/${data.matchId}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                });
                
                const result = await response.json();
                
                if (response.ok && result.success) {
                    statusDiv.innerHTML = '<div class="status success">? Result submitted successfully!</div>';
                    e.target.reset();
                } else {
                    throw new Error(result.message || 'Unknown error');
                }
            } catch (error) {
                statusDiv.innerHTML = `<div class="status error">? Error: ${error.message}</div>`;
            }
        };
    </script>
</body>
</html>
```

### 2. Mobile App Integration
Die API kann auch von mobilen Apps angesprochen werden:

#### React Native Beispiel
```javascript
const submitMatchResult = async (matchId, result) => {
  try {
    const response = await fetch(`http://localhost:5000/api/matches/${matchId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(result)
    });
    
    const data = await response.json();
    
    if (data.success) {
      Alert.alert('Success', 'Match result submitted successfully!');
    } else {
      Alert.alert('Error', data.message);
    }
  } catch (error) {
    Alert.alert('Error', error.message);
  }
};
```

#### Flutter Beispiel
```dart
Future<void> submitMatchResult(int matchId, Map<String, dynamic> result) async {
  try {
    final response = await http.put(
      Uri.parse('http://localhost:5000/api/matches/$matchId'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(result),
    );
    
    final data = jsonDecode(response.body);
    
    if (response.statusCode == 200 && data['success']) {
      // Show success message
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Match result submitted successfully!')),
      );
    } else {
      throw Exception(data['message']);
    }
  } catch (error) {
    // Show error message
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text('Error: $error')),
    );
  }
}
```

### 3. Real-Time Updates mit SignalR
```javascript
// JavaScript SignalR Client
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/tournamentHub")
    .build();

// Event-Handler f�r Match-Updates
connection.on("MatchResultUpdated", (data) => {
    console.log("Match updated:", data);
    // Update UI accordingly
});

// Verbindung starten
connection.start().then(() => {
    console.log("Connected to tournament hub");
}).catch(err => console.error(err));
```

## ?? Sicherheit und Zug�nglichkeit

### Aktuelle Konfiguration
- **CORS aktiviert**: Zugriff von Webseiten m�glich
- **Lokaler Zugriff**: API l�uft nur lokal (localhost:5000)
- **Keine Authentifizierung**: F�r lokale Nutzung optimiert
- **Automatischer Start/Stop**: Vollst�ndig in WPF-Anwendung integriert

### Sicherheits�berlegungen
- Die API ist f�r **lokale Nutzung** designed
- **Kein Internet-Zugang** standardm��ig
- **Keine sensiblen Daten** werden �ber die API �bertragen
- **Vollst�ndige Kontrolle** �ber API-Verf�gbarkeit durch WPF-App

## ?? Performance und Optimierung

### Leistung
- **Minimaler Overhead**: API l�uft nur bei Bedarf
- **Direkter Datenzugriff**: Keine Datenbank-Zwischenschicht
- **Live-Synchronisation**: �nderungen in Echtzeit zwischen API und WPF
- **In-Memory Database**: F�r tempor�re API-Daten

### Ressourcenverbrauch
- **Separate Prozesse**: API l�uft unabh�ngig von WPF
- **Automatische Cleanup**: Prozess wird beim Schlie�en der WPF-App beendet
- **Health Monitoring**: Automatische �berwachung der API-Verf�gbarkeit

## ?? Troubleshooting

### H�ufige Probleme

#### 1. API startet nicht
**Problem**: "API konnte nicht gestartet werden"
**L�sung**: 
- Pr�fen Sie ob .NET 9 SDK installiert ist: `dotnet --version`
- Pr�fen Sie ob Port 5000 verf�gbar ist
- Starten Sie als Administrator (falls n�tig)

#### 2. CORS-Fehler in Webseiten
**Problem**: "CORS policy error"
**L�sung**: 
- API sollte bereits CORS f�r alle Origins aktiviert haben
- Pr�fen Sie die Browser-Konsole f�r Details
- Verwenden Sie http://localhost:5000/demo f�r Tests

#### 3. Verbindung zur WPF-App verloren
**Problem**: "Keine aktiven Turnierdaten gefunden"
**L�sung**:
- Starten Sie die API �ber die WPF-Anwendung neu
- Pr�fen Sie ob ein Turnier in der WPF-App ge�ffnet ist
- Verwenden Sie "API ? API Status" zur Diagnose

#### 4. Match nicht gefunden
**Problem**: "Match nicht gefunden" bei PUT-Request
**L�sung**:
- Verwenden Sie GET /api/matches/pending f�r verf�gbare Matches
- Pr�fen Sie die Match-ID in der WPF-Anwendung
- Stellen Sie sicher, dass das Match noch nicht beendet ist

### Debug-Modus
```bash
# API mit Debug-Ausgabe starten
cd DartTournamentPlaner.API
dotnet run --configuration Debug --verbosity detailed
```

## ?? Zukunftige Erweiterungen

### Geplante Features
- **Authentifizierung**: Login-System f�r sichere Nutzung
- **Cloud-Integration**: Remote-Zugriff �ber Internet
- **Mobile App**: Dedizierte Smartphone-App f�r iOS/Android
- **Erweiterte Statistiken**: API f�r erweiterte Analyse-Tools
- **Webhook-Support**: Automatische Benachrichtigungen bei Events
- **Database Persistence**: Dauerhafte Speicherung von API-Daten

### Erweiterungsm�glichkeiten
- **Multi-Tournament Support**: Mehrere Turniere gleichzeitig
- **User Management**: Benutzer- und Rechteverwaltung
- **Export/Import**: API f�r Datenexport in verschiedene Formate
- **Streaming Integration**: Live-Streaming-Support f�r Turniere
- **Analytics Dashboard**: Web-basiertes Analytics-Interface

## ?? Support und Beitrag

### Dokumentation
- **API-Dokumentation**: http://localhost:5000 (wenn gestartet)
- **Demo-Interface**: http://localhost:5000/demo
- **Source Code**: Alle Dateien sind vollst�ndig dokumentiert

### Beitrag zum Projekt
- Die API ist vollst�ndig Open Source
- Modular aufgebaut f�r einfache Erweiterungen
- Gut dokumentiert f�r Entwickler
- Tests k�nnen �ber das Demo-Interface durchgef�hrt werden

Diese API-Integration macht den Dart Tournament Planer zu einer modernen, erweiterbaren Plattform f�r digitale Turniere! ??