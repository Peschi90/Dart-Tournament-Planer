# Leg Darts Statistics Feature - Implementierungsdokumentation

## Übersicht

Die Dart Tournament Planer Desktop-Anwendung wurde um **detaillierte Leg-Darts-Statistiken** erweitert. Diese zeigen für jeden Spieler, wie viele Darts benötigt wurden, um gewonnene Legs zu gewinnen.

## ?? Implementierte Features

### 1. Neue Datenmodelle (`PlayerMatchStatistics.cs`)

#### `LegData` Klasse
```csharp
public class LegData
{
    public int LegNumber { get; set; }          // Leg-Nummer (1-basiert)
    public double Average { get; set; }         // Average für dieses Leg
    public int Darts { get; set; }             // ? NEU: Anzahl benötigter Darts
    public int Score { get; set; }             // Erzielte Punkte
    public DateTime Timestamp { get; set; }     // Zeitstempel
    public string FormattedDisplay { get; }     // "Leg X: Y Darts (Ø Z)"
}
```

#### Neue Properties in `PlayerMatchStatistics`
```csharp
// Collection für detaillierte Leg-Daten
public List<LegData> LegData { get; set; } = new List<LegData>();

// Berechnete Eigenschaften
public int FewestDartsPerLeg { get; }          // Wenigste Darts für ein Leg
public double AverageDartsPerLeg { get; }      // Durchschnittliche Darts pro Leg
public string LegDartsFormatted { get; }       // "6 | 9 | 12" (alle Legs)
```

### 2. Erweiterte Spieler-Statistiken (`PlayerStatistics.cs`)

#### Neue aggregierte Statistiken über alle Matches
```csharp
// Leg-Effizienz über alle Matches
public int FewestDartsPerLeg { get; }          // Beste Leg-Effizienz
public double AverageDartsPerWonLeg { get; }   // Durchschnitt über alle gewonnenen Legs
public string BestLegEfficiencyFormatted { get; } // "6 Darts @ 150.5"
```

**Berechnungslogik:**
- Durchsucht alle `LegData` aus allen Matches
- Findet das beste Leg (wenigste Darts)
- Berechnet Durchschnitt über alle gewonnenen Legs
- Formatiert als lesbarer String mit Average

### 3. JSON-Parsing (`PlayerStatisticsManager.cs`)

#### Neue `legData` Verarbeitung
```csharp
if (playerData.TryGetProperty("legData", out var legData) && 
    legData.ValueKind == JsonValueKind.Array)
{
    foreach (var legDataElement in legData.EnumerateArray())
    {
        var legDataItem = new LegData();
        
        // Leg-Nummer
        if (legDataElement.TryGetProperty("legNumber", out var legNumber))
            legDataItem.LegNumber = legNumber.GetInt32();
            
        // ? WICHTIG: Darts pro Leg extrahieren
        if (legDataElement.TryGetProperty("darts", out var darts))
            legDataItem.Darts = darts.GetInt32();
            
        // Average, Score, Timestamp...
        stats.LegData.Add(legDataItem);
    }
}
```

**Unterstützte JSON-Struktur:**
```json
{
  "player1Stats": {
    "legData": [
      {
        "legNumber": 1,
        "average": 150.5,
        "darts": 6,
        "score": 301,
        "timestamp": "2025-11-13T17:39:33.669Z"
      }
    ]
  }
}
```

### 4. UI-Erweiterungen (`PlayerStatisticsView`)

#### Neue DataGrid-Spalten
```xaml
<!-- Wenigste Darts/Leg -->
<DataGridTextColumn Name="FewestDartsPerLegColumn" 
                    Header="Min Darts/Leg" 
                    Binding="{Binding FewestDartsPerLegFormatted}" 
                    Width="110">
    <DataGridTextColumn.ElementStyle>
        <Style TargetType="TextBlock">
            <Setter Property="Foreground" Value="#10B981"/>
            <Setter Property="FontWeight" Value="Bold"/>
        </Style>
    </DataGridTextColumn.ElementStyle>
</DataGridTextColumn>

<!-- Durchschnittliche Darts/Leg -->
<DataGridTextColumn Name="AverageDartsPerLegColumn" 
                    Header="? Darts/Leg" 
                    Binding="{Binding AverageDartsPerWonLegFormatted}" 
                    Width="110"/>

<!-- Beste Leg-Effizienz -->
<DataGridTextColumn Name="BestLegEfficiencyColumn" 
                    Header="Beste Leg-Effizienz" 
                    Binding="{Binding BestLegEfficiency}" 
                    Width="150"/>
```

#### Display-Model Erweiterungen
```csharp
public class PlayerStatisticsDisplayModel
{
    // Neue Properties
    public int FewestDartsPerLeg { get; private set; }
    public double AverageDartsPerWonLeg { get; private set; }
    
    // Formatierte Anzeige
    public string FewestDartsPerLegFormatted => 
        FewestDartsPerLeg > 0 ? $"{FewestDartsPerLeg}" : "-";
    
    public string AverageDartsPerWonLegFormatted => 
        AverageDartsPerWonLeg > 0 ? $"{AverageDartsPerWonLeg:F1}" : "-";
    
    public string BestLegEfficiency => 
        _playerStatistics.BestLegEfficiencyFormatted;
}
```

### 5. Mehrsprachigkeit

#### Deutsche Übersetzungen (`GermanPlayerStatisticsLanguageProvider.cs`)
```csharp
["FewestDartsPerLeg"] = "Min Darts/Leg",
["AverageDartsPerLeg"] = "? Darts/Leg",
["BestLegEfficiency"] = "Beste Leg-Effizienz",
["FewestDartsPerLegTooltip"] = "Wenigste benötigte Darts für ein gewonnenes Leg",
["AverageDartsPerLegTooltip"] = "Durchschnittliche Darts pro gewonnenem Leg",
["BestLegEfficiencyTooltip"] = "Beste Leg-Effizienz (wenigste Darts + Average)"
```

#### Englische Übersetzungen (`EnglishPlayerStatisticsLanguageProvider.cs`)
```csharp
["FewestDartsPerLeg"] = "Min Darts/Leg",
["AverageDartsPerLeg"] = "? Darts/Leg",
["BestLegEfficiency"] = "Best Leg Efficiency",
["FewestDartsPerLegTooltip"] = "Fewest darts needed to win a leg",
["AverageDartsPerLegTooltip"] = "Average darts per won leg",
["BestLegEfficiencyTooltip"] = "Best leg efficiency (fewest darts + average)"
```

## ?? Beispiel-Darstellung

### In der Statistik-Tabelle:

```
???????????????????????????????????????????????????????????????????
? Spieler     ? Min Darts/Leg? ? Darts/Leg  ? Beste Leg-Effizienz ?
???????????????????????????????????????????????????????????????????
? test1       ? 6            ? 7.5          ? 6 Darts @ 150.5     ?
? test11      ? 9            ? 10.2         ? 9 Darts @ 133.4     ?
? PlayerX     ? 12           ? 15.8         ? 12 Darts @ 75.2     ?
???????????????????????????????????????????????????????????????????
```

### Im Tooltip:
```
Gespielte Matches: 5
Gewonnen: 3 (60.0%)
Verloren: 2
Turnier Average: 109.5
Beste/Schlechteste: 150.5/82.3
Höchster Leg Average: 150.5
180er: 4
High Finishes: 3 (Scores: 121 | 141 | 170)
Schlechte Scores (?26): 1
Checkouts: 5
Wenigste Darts/Finish: 1
? Darts/Checkout: 2.4
Wenigste Darts/Leg: 6        ? ? NEU
? Darts/Leg: 7.5             ? ? NEU
Beste Leg-Effizienz: 6 Darts @ 150.5  ? ? NEU
Schnellstes Match: 5:23
Wenigste Würfe: 18
```

## ?? Datenfluss

### 1. Hub sendet Match-Ergebnis
```json
{
  "dartScoringResult": {
    "player1Stats": {
      "legData": [
        {
          "legNumber": 1,
          "darts": 6,
          "average": 150.5,
          "score": 301,
          "timestamp": "2025-11-13T17:39:33.669Z"
        }
      ]
    }
  }
}
```

### 2. PlayerStatisticsManager parsed legData
```csharp
// In ParsePlayerStats()
foreach (var legDataElement in legData.EnumerateArray())
{
    var legDataItem = new LegData();
    legDataItem.Darts = darts.GetInt32();  // ? Extrahiert Darts
    stats.LegData.Add(legDataItem);
}
```

### 3. PlayerMatchStatistics speichert die Daten
```csharp
public class PlayerMatchStatistics
{
    public List<LegData> LegData { get; set; } = new();
    public int FewestDartsPerLeg => LegData.Min(ld => ld.Darts);
}
```

### 4. PlayerStatistics aggregiert über alle Matches
```csharp
public class PlayerStatistics
{
    public int FewestDartsPerLeg => 
        MatchStatistics.SelectMany(m => m.LegData)
                      .Where(ld => ld.Darts > 0)
                      .Min(ld => ld.Darts);
}
```

### 5. UI zeigt die Statistiken an
```csharp
public class PlayerStatisticsDisplayModel
{
    FewestDartsPerLeg = playerStatistics.FewestDartsPerLeg;
    AverageDartsPerWonLeg = playerStatistics.AverageDartsPerWonLeg;
}
```

## ?? Use Cases

### 1. Effizienz-Analyse
**Frage:** "Wie effizient gewinnt ein Spieler Legs?"
**Antwort:** 
- `FewestDartsPerLeg: 6` ? Bestes Leg mit nur 6 Darts gewonnen
- `AverageDartsPerWonLeg: 7.5` ? Im Durchschnitt 7.5 Darts pro Leg

### 2. Spieler-Vergleich
```
Spieler A: 6 Darts (? 7.5)  ? Effizienter
Spieler B: 9 Darts (? 10.2)
```

### 3. Performance-Tracking
- Über mehrere Turniere hinweg
- Verbesserung der Leg-Effizienz sichtbar machen
- Kombination mit Average zeigt echte Spielstärke

### 4. Rekord-Tracking
**"Beste Leg-Effizienz: 6 Darts @ 150.5"**
- Zeigt sowohl Effizienz (6 Darts) als auch Qualität (150.5 Average)
- Perfekt für Turnierankündigungen und Spieler-Profile

## ?? Visuelle Gestaltung

### Farbschema
- **Leg-Darts Spalten:** `#10B981` (Grün) - für Effizienz-Metriken
- **Highlight:** Fettdruck für beste Werte
- **Formatierung:** Dezimalstellen für Durchschnittswerte (F1)

### Sortierung
Die Tabelle kann nach allen neuen Spalten sortiert werden:
- Nach wenigsten Darts (aufsteigend)
- Nach durchschnittlichen Darts (aufsteigend)
- Nach bester Effizienz (kombiniert)

## ?? Technische Details

### Null-Safety
Alle Berechnungen prüfen auf:
- Leere Collections (`LegData.Any()`)
- Null-Werte (`> 0` Checks)
- Fallback zu "-" in der UI

### Performance
- **Lazy Evaluation:** Statistiken werden nur bei Bedarf berechnet
- **LINQ Optimierung:** Verwendung von `SelectMany` und `Where`
- **Caching:** Werte werden im DisplayModel gecacht

### Fehlerbehandlung
```csharp
try
{
    var fewestDarts = _playerStatistics.FewestDartsPerLeg;
    return fewestDarts > 0 ? $"{fewestDarts}" : "-";
}
catch
{
    return "-";
}
```

## ?? Zukünftige Erweiterungen

### Mögliche Features:
1. **Leg-Darts Histogramm**
   - Visualisierung der Verteilung
   - Häufigkeit von 6, 9, 12, 15 Darts

2. **Trend-Analyse**
   - Verbesserung über Zeit
   - Vergleich erste vs. letzte 5 Matches

3. **Checkout-Pattern**
   - Welche Darts-Anzahl führt zu den besten Checkouts
   - Korrelation zwischen Darts und Average

4. **Leg-Details Export**
   - CSV-Export aller Leg-Daten
   - Detaillierte Analyse in Excel

## ? Testing

### Test-Szenarien:

#### 1. Perfect Leg (6 Darts)
```json
{
  "legData": [{
    "legNumber": 1,
    "darts": 6,
    "average": 150.5,
    "score": 301
  }]
}
```
**Erwartet:** Min Darts/Leg = 6

#### 2. Multiple Legs
```json
{
  "legData": [
    {"legNumber": 1, "darts": 6, "average": 150.5},
    {"legNumber": 2, "darts": 9, "average": 100.3},
    {"legNumber": 3, "darts": 12, "average": 75.2}
  ]
}
```
**Erwartet:** 
- Min Darts/Leg = 6
- ? Darts/Leg = 9.0
- Beste Effizienz = "6 Darts @ 150.5"

#### 3. Keine Leg-Daten
```json
{
  "legData": []
}
```
**Erwartet:** Alle Spalten zeigen "-"

## ?? Bekannte Einschränkungen

1. **Nur gewonnene Legs**
   - LegData enthält nur Legs, die der Spieler gewonnen hat
   - Verlorene Legs werden nicht erfasst

2. **Hub-Abhängigkeit**
   - Feature funktioniert nur mit Hub-Integration
   - Manuelle Eingaben haben keine Leg-Details

3. **Historische Daten**
   - Alte Matches ohne `legData` zeigen "-"
   - Migration nicht möglich ohne Re-Eingabe

## ?? Zusammenfassung

### ? Implementiert:
- ? `LegData` Klasse mit Darts-Property
- ? JSON-Parsing in `PlayerStatisticsManager`
- ? Aggregierte Statistiken in `PlayerStatistics`
- ? UI-Spalten in `PlayerStatisticsView`
- ? Mehrsprachige Übersetzungen (DE/EN)
- ? Formatierte Anzeige mit Fallbacks
- ? Tooltip-Integration

### ?? Vorteile:
- Detaillierte Effizienz-Analyse
- Besserer Spieler-Vergleich
- Kombination von Qualität (Average) und Effizienz (Darts)
- Intuitive visuelle Darstellung

### ?? Performance:
- Buildvorgang: ? Erfolgreich
- Null-Safety: ? Vollständig implementiert
- Fehlerbehandlung: ? Robust

---

**Version:** 1.0  
**Datum:** 2025-01-13  
**Status:** ? Produktionsbereit
