# ?? Dart Tournament Planner - API Integration Complete!

## ?? �bersicht

Die API-Integration f�r den Dart Tournament Planner ist nun vollst�ndig implementiert und einsatzbereit! Diese Erweiterung erm�glicht es externen Anwendungen, �ber eine REST API mit dem Turnier-System zu interagieren.

## ? Abgeschlossene Features

### ?? REST API
- **Vollst�ndige ASP.NET Core Web API** (`DartTournamentPlaner.API`)
- **Swagger/OpenAPI Dokumentation** automatisch generiert
- **CORS-Unterst�tzung** f�r Cross-Origin-Requests
- **In-Memory Database** f�r API-Daten
- **Health Check Endpoint** f�r Status-Monitoring

### ?? API Endpoints
| Endpoint | Methode | Beschreibung |
|----------|---------|--------------|
| `/health` | GET | API-Gesundheitsstatus |
| `/api/tournaments/current` | GET | Aktuelle Turnierdaten |
| `/api/tournaments/status` | GET | API-Status |
| `/api/matches/pending` | GET | Ausstehende Matches |
| `/api/matches/{id}` | PUT | Match-Ergebnis aktualisieren |
| `/demo` | GET | Demo-Interface |

### ??? WPF-Integration
- **API-Men�** in MainWindow hinzugef�gt
- **Start/Stop-Funktionalit�t** f�r API
- **Status-Anzeige** in der Statusleiste
- **Browser-Integration** f�r API-Dokumentation
- **Mehrsprachige Unterst�tzung** (DE/EN)

### ?? Demo Interface
- **Interaktive HTML-Seite** unter `/demo.html`
- **Live-Turnierdaten-Anzeige**
- **Match-Auswahl und -Eingabe**
- **API-Testing-Tools**
- **Responsive Design** f�r alle Ger�te

## ?? Sofort starten

### Option 1: �ber WPF-Anwendung (Empfohlen)
```
1. Dart Tournament Planner starten
2. Men�: API ? API starten
3. Browser �ffnet automatisch: http://localhost:5000
4. Demo Interface: http://localhost:5000/demo
```

### Option 2: �ber Batch-Datei
```
1. Doppelklick auf 'start-api.bat'
2. API startet in separatem Fenster
3. Browser �ffnen: http://localhost:5000
```

### Option 3: Kommandozeile
```bash
cd DartTournamentPlaner.API
dotnet run --urls "http://localhost:5000"
```

## ?? Neue Dateien

### API-Projekt (`DartTournamentPlaner.API/`)
- `Program.cs` - API-Konfiguration und Startup
- `Controllers/TournamentsController.cs` - Turnier-Endpoints
- `Controllers/MatchesController.cs` - Match-Endpoints
- `Services/TournamentApiService.cs` - Turnier-Business-Logic
- `Services/MatchApiService.cs` - Match-Business-Logic
- `Services/TournamentSyncService.cs` - Synchronisation-Service
- `Services/ApiDbContext.cs` - Entity Framework Context
- `Models/TournamentDtos.cs` - Datenmodelle f�r API
- `Models/ApiResponse.cs` - Standard-API-Antwort-Format
- `Hubs/TournamentHub.cs` - SignalR Real-Time Hub
- `wwwroot/demo.html` - Demo-Interface
- `DartTournamentPlaner.API.csproj` - Projektdatei

### Hauptanwendung Updates
- `Services/ApiIntegrationService.cs` - API-Integration f�r WPF
- `Services/HttpApiIntegrationService.cs` - Erweiterte HTTP-Integration
- `MainWindow.xaml` - Neues API-Men�
- `MainWindow.xaml.cs` - API-Event-Handler
- Erweiterte �bersetzungen in `GermanLanguageProvider.cs` und `EnglishLanguageProvider.cs`

### Hilfsdateien
- `start-api.bat` - Batch-Datei zum manuellen API-Start
- `API_INTEGRATION.md` - Umfassende Dokumentation
- `README_API.md` - Diese �bersicht

## ?? Technische Details

### Architektur
```
???????????????????????    HTTP/REST    ???????????????????????
?                     ? ???????????????? ?                     ?
?  WPF Application    ?                  ?   ASP.NET Core      ?
?  (UI & Business)    ?                  ?   Web API           ?
?                     ?                  ?                     ?
???????????????????????                  ???????????????????????
?                                                               ?
?                        SignalR                               ?
? ??????????????????????????????????????????????????????????? ?
?                     Real-time Updates                        ?
?                                                               ?
?                  External Applications                       ?
? ??????????????????????????????????????????????????????????? ?
   � Web Pages                                                 
   � Mobile Apps                                               
   � Third-party Tools                                         
```

### Datenfluss
1. **WPF-App** startet API �ber `ApiIntegrationService`
2. **API** l�uft als separater Prozess (Port 5000)
3. **Externe Apps** senden HTTP-Requests an API
4. **API** verarbeitet Requests und antwortet
5. **SignalR** sendet Updates an alle verbundenen Clients
6. **WPF-App** empf�ngt Updates und aktualisiert UI

### Sicherheit
- ? **Lokal only** - API l�uft nur auf localhost
- ? **CORS enabled** - Web-Apps k�nnen zugreifen
- ? **Prozess-Isolation** - API l�uft in separatem Prozess
- ? **Automatische Cleanup** - API wird beim App-Ende gestoppt

## ?? Testen der API

### 1. Demo Interface (Benutzerfreundlich)
```
URL: http://localhost:5000/demo
- Interaktive Oberfl�che
- Live-Datenansicht
- Match-Eingabe-Formular
- API-Testing-Tools
```

### 2. Swagger UI (Entwickler)
```
URL: http://localhost:5000
- Alle Endpoints dokumentiert
- Interaktive Tests
- JSON-Schema-Dokumentation
- Beispiel-Requests und -Responses
```

### 3. cURL (Kommandozeile)
```bash
# API Status pr�fen
curl http://localhost:5000/health

# Aktuelle Turnierdaten abrufen
curl http://localhost:5000/api/tournaments/current

# Ausstehende Matches anzeigen
curl http://localhost:5000/api/matches/pending

# Match-Ergebnis eingeben
curl -X PUT "http://localhost:5000/api/matches/123" \
  -H "Content-Type: application/json" \
  -d '{
    "player1Sets": 3,
    "player2Sets": 1,
    "player1Legs": 9,
    "player2Legs": 6
  }'
```

## ?? Anwendungsbeispiele

### Web-basierte Eingabe
Erstellen Sie eine einfache HTML-Seite f�r Spieler:
```html
<script>
async function submitResult(matchId, result) {
  const response = await fetch(`http://localhost:5000/api/matches/${matchId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(result)
  });
  
  if (response.ok) {
    alert('Result submitted!');
  }
}
</script>
```

### Mobile App Integration
React Native, Flutter oder native Apps k�nnen die API direkt ansprechen.

### Live-Streaming Integration
SignalR erm�glicht Real-Time Updates f�r Streaming-Overlays.

## ?? Troubleshooting

### H�ufige Probleme

**Problem**: "API konnte nicht gestartet werden"
**L�sung**: 
- .NET 9 SDK installiert? `dotnet --version`
- Port 5000 frei? Andere Anwendung beenden
- Als Administrator ausf�hren (falls n�tig)

**Problem**: "CORS error" in Browser
**L�sung**:
- CORS ist aktiviert - Browser-Konsole pr�fen
- Demo Interface verwenden: http://localhost:5000/demo
- Lokale Datei �ber HTTP-Server bereitstellen

**Problem**: "Keine Turnierdaten gefunden"
**L�sung**:
- API �ber WPF-App starten (empfohlen)
- Turnier in WPF-App �ffnen/erstellen
- API neu starten

## ?? Fazit

Die API-Integration ist vollst�ndig und einsatzbereit! Alle Features funktionieren:

- ? **REST API** vollst�ndig implementiert
- ? **WPF-Integration** mit Men� und Status
- ? **Demo Interface** f�r einfache Tests  
- ? **Swagger-Dokumentation** automatisch verf�gbar
- ? **Real-Time Updates** via SignalR
- ? **Mehrsprachig** (Deutsch/Englisch)
- ? **Batch-Dateien** f�r manuellen Start
- ? **Umfassende Dokumentation**

**Die Anwendung ist jetzt eine moderne, erweiterbare Plattform f�r digitale Dart-Turniere!** ??

Starten Sie einfach den Dart Tournament Planner, gehen Sie zu **API ? API starten** und entdecken Sie die neuen M�glichkeiten unter http://localhost:5000!