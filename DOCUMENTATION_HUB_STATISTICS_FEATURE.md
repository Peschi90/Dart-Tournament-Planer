# Hub Match Statistics Display Feature

## Übersicht

Die Dart Tournament Planer Desktop-Anwendung kann nun **Hub Match Statistics** automatisch aus dem Notizen-Feld des Match Result Dialog parsen und strukturiert anzeigen.

## Implementierte Änderungen

### 1. Neue Model-Klassen (`HubMatchResult.cs`)

Erstellt vollständige Datenmodelle für die JSON-Struktur, die vom Hub übermittelt wird:

- `HubMatchResultData` - Hauptcontainer für Hub-Daten
- `MatchUpdateData` - Match-Update Informationen
- `MatchResultData` - Match-Ergebnis Details
- `DartScoringResultData` - Detaillierte Dart-Scoring Ergebnisse
- `PlayerStatsData` - Spieler-Statistiken
- `MatchStatistics` - Zusammengefasste Match-Statistiken
- Plus Hilfsklassen für Details (Maximums, High Finishes, Checkouts, etc.)

### 2. UI-Erweiterungen (`MatchResultWindow.xaml`)

**Neue Statistics Section** wurde zwischen Notes und QR Code Section eingefügt:

```xaml
<Border Name="HubStatisticsSection" Background="#F0F9FF" BorderBrush="#BAE6FD" 
        BorderThickness="1" CornerRadius="12" 
        Padding="16" Margin="0,0,0,20" Visibility="Collapsed">
    <!-- Statistiken werden hier angezeigt -->
</Border>
```

**Angezeigte Statistiken:**

#### Pro Spieler:
- ⌀ Average (Durchschnitt)
- 🎯 180s (Maximums)
- 🏆 High Finishes (≥100)
- ✓ Checkouts

#### Match Information:
- ⏱️ Dauer
- 🎮 Format
- 📡 Eingabe via (z.B. "DartScoringAdvanced")

### 3. Code-Behind Logik (`MatchResultWindow.xaml.cs`)

**Neue Methoden:**

#### `ParseAndDisplayHubStatistics()`
```csharp
private void ParseAndDisplayHubStatistics()
{
    // Prüft ob Notes-Feld Hub-Daten enthält
    // Parst JSON mit System.Text.Json
    // Ruft DisplayHubStatistics() auf wenn erfolgreich
}
```

**Erkennungslogik:**
- Prüft ob Notes-Feld mit `{"type":"tournament-match-updated"` beginnt
- Verwendet JSON-Deserialisierung mit fehlertoleranten Optionen
- Zeigt Section nur an, wenn valide Daten vorhanden sind

#### `DisplayHubStatistics(HubMatchResultData hubData)`
```csharp
private void DisplayHubStatistics(HubMatchResultData hubData)
{
    // Befüllt UI-Elemente mit geparsten Daten
    // Formatiert Dauer-Anzeige (Stunden/Minuten/Sekunden)
    // Zeigt High Finish Details als Liste
}
```

**Features:**
- Dynamische UI-Element-Suche mit `FindName()`
- Intelligente Dauer-Formatierung
- Optional: High Finish Scores als kommagetrennteiste

## Verwendung

### Automatische Erkennung

Wenn ein Match über das Hub beendet wird, wird die komplette Hub-Response im Notes-Feld gespeichert. Beim Öffnen des Match Result Dialogs:

1. **Prüfung:** System erkennt automatisch Hub-Daten im Notes-Feld
2. **Parsing:** JSON wird deserialisiert und validiert
3. **Anzeige:** Statistics Section wird mit Daten gefüllt und sichtbar gemacht

### Beispiel Notes-Feld Inhalt

```json
{
  "type": "tournament-match-updated",
  "tournamentId": "TOURNAMENT_Barksen-Masters",
  "statistics": {
    "player1": {
      "name": "kljsadk",
      "average": 109.1,
      "scores180": 1,
      "highFinishes": 0,
      "checkouts": 1,
      "totalThrows": 27,
      "totalScore": 1342
    },
    "player2": {
      "name": "jsakdjl",
      "average": 104.4,
      "scores180": 4,
      "highFinishes": 3,
      "highFinishScores": [
        {"finish": 121, "darts": [60,57,4], ...},
        ...
      ],
      "checkouts": 4,
      "totalThrows": 33,
      "totalScore": 1504
    },
    "match": {
      "duration": 303415,
      "format": "301 Double-Out",
      "totalThrows": 60,
      "startTime": "2025-11-12T20:10:13.143Z",
      "endTime": "2025-11-12T20:15:16.558Z"
    }
  },
  "matchUpdate": {
    "result": {
      "dartScoringResult": {
        "player1Stats": { ... },
        "player2Stats": { ... },
        "submittedVia": "DartScoringAdvanced",
        "version": "1.2.0"
      }
    }
  }
}
```

### UI-Darstellung

Wenn Hub-Daten erkannt werden:

```
┌─────────────────────────────────────────────┐
│ 📊 Dart Scoring Statistiken                │
├─────────────────────────────────────────────┤
│                                             │
│  kljsadk              VS          jsakdjl   │
│  ┌────────────────┐          ┌────────────┐│
│  │ ⌀ Avg:   109.1 │          │ ⌀ Avg: 104.4││
│  │ 🎯 180s:     1 │          │ 🎯 180s:   4││
│  │ 🏆 HF:       0 │          │ 🏆 HF: 3(121)││
│  │ ✓ CO:        1 │          │ ✓ CO:      4││
│  └────────────────┘          └────────────┘│
│                                             │
│  Match Information:                         │
│  ⏱️ Dauer: 5m 3s                           │
│  🎮 Format: 301 Double-Out                 │
│  📡 Eingabe via: DartScoringAdvanced       │
└─────────────────────────────────────────────┘
```

## Technische Details

### JSON Deserialisierung

**Verwendete Optionen:**
```csharp
var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,  // Groß-/Kleinschreibung ignorieren
    AllowTrailingCommas = true,          // Trailing Commas erlauben
    ReadCommentHandling = JsonCommentHandling.Skip  // Kommentare überspringen
};
```

### Fehlerbehandlung

Das System ist robust gegen:
- ✅ Fehlende Hub-Daten (Section bleibt versteckt)
- ✅ Ungültiges JSON (Exception Handling)
- ✅ Teilweise Daten (Null-Checks für alle Felder)
- ✅ Alte Matches ohne Hub-Daten (automatische Erkennung)

### Performance

- **Lazy Loading:** Statistiken werden nur beim Öffnen des Dialogs geparst
- **Kein Network-Call:** Alle Daten sind bereits im Notes-Feld vorhanden
- **Minimale UI-Updates:** Nur betroffene TextBlocks werden aktualisiert

## Zukünftige Erweiterungen

### Mögliche Verbesserungen:

1. **Leg-by-Leg Details**
   - Detaillierte Anzeige jedes einzelnen Legs
   - Leg-Average Verläufe als Chart

2. **Export-Funktionalität**
   - Statistiken als PDF exportieren
   - Excel/CSV Export für Analysen

3. **Historische Vergleiche**
   - Vergleich mit früheren Matches des Spielers
   - Performance-Trends anzeigen

4. **Erweiterte Metriken**
   - First 9 Average
   - Checkout-Percentage
   - Tons (100+, 140+, 180)

## Kompatibilität

### Versionen:
- ✅ .NET 9
- ✅ System.Text.Json
- ✅ Tournament Hub v1.0+

### Backwards Compatibility:
- ✅ Alte Matches ohne Hub-Daten: Section bleibt versteckt
- ✅ Manuelle Notizen: Werden normal angezeigt, keine Statistics Section
- ✅ Bestehende Match Result Dialogs: Funktionieren weiterhin wie gewohnt

## Testing

### Test-Szenarien:

1. **Match mit Hub-Daten:**
   - ✅ Statistics Section wird angezeigt
   - ✅ Alle Statistiken sind korrekt befüllt
   - ✅ Dauer ist formatiert (5m 3s statt 303415ms)

2. **Match ohne Hub-Daten:**
   - ✅ Statistics Section ist versteckt
   - ✅ Notizen werden normal angezeigt
   - ✅ Keine Fehler in Console

3. **Match mit ungültigem JSON:**
   - ✅ Statistics Section ist versteckt
   - ✅ Fehler wird geloggt aber nicht dem User angezeigt
   - ✅ Dialog funktioniert normal weiter

4. **Match mit teilweisen Daten:**
   - ✅ Verfügbare Daten werden angezeigt
   - ✅ Fehlende Felder zeigen "-" oder werden ausgelassen
   - ✅ Keine Null-Reference Exceptions

## Debug-Output

Das System gibt detaillierte Debug-Informationen aus:

```
ℹ️ [MatchResultWindow] No hub statistics data in Notes field
🔍 [MatchResultWindow] Found hub statistics data, parsing...
✅ [MatchResultWindow] Hub statistics displayed successfully
⚠️ [MatchResultWindow] Hub data parsed but statistics missing
❌ [MatchResultWindow] Error parsing hub statistics: ...
```

## Zusammenfassung

Mit dieser Implementation können Hub Match Results nun:
- ✅ Automatisch erkannt werden
- ✅ Strukturiert angezeigt werden
- ✅ Detaillierte Statistiken bereitstellen
- ✅ Nahtlos in bestehenden Dialog integriert werden

Der User muss **nichts** manuell tun - die Statistiken erscheinen automatisch, wenn Hub-Daten vorhanden sind!
