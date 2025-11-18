# Leg Darts Statistics - Debugging Guide

## Problem

Die neuen Spalten in der Statistik-Tabelle (`Min Darts/Leg`, `? Darts/Leg`, `Beste Leg-Effizienz`) werden nicht mit Daten gefüllt.

## Behobene Issues

### 1. **JSON-Parsing Priorität** ?
**Problem:** Das System verwendete `ExtractDirectWebSocketStatistics` als erste Priorität, die nur die `statistics`-Section parsed, **nicht** die detaillierten `dartScoringResult` mit den `legData`.

**Lösung:** Reihenfolge geändert in:
1. ? **PRIORITÄT 1:** `ExtractEnhancedDartStatistics` (enthält `legData`!)
2. **PRIORITÄT 2:** `ExtractDirectWebSocketStatistics`
3. **PRIORITÄT 3:** `ExtractTopLevelPlayerStatistics`
4. **PRIORITÄT 4:** `ExtractSimplePlayerStatistics`

### 2. **Won-Property fehlte** ?
**Problem:** Das JSON enthält `"won": true/false` pro Leg, aber die `LegData` Klasse hatte diese Property nicht.

**Lösung:** 
- `Won` Property zu `LegData` hinzugefügt
- Parsing im `PlayerStatisticsManager` erweitert
- Nur gewonnene Legs (`ld.Won == true`) werden für Statistiken gezählt

### 3. **Filterung auf gewonnene Legs** ?
**Problem:** Berechnungen zählten alle Legs, nicht nur gewonnene.

**Lösung:** Alle LINQ-Queries filtern jetzt mit `.Where(ld => ld.Won)`:
```csharp
// PlayerStatistics.cs
var allLegData = MatchStatistics
    .SelectMany(m => m.LegData)
    .Where(ld => ld.Darts > 0 && ld.Won); // ? Nur gewonnene Legs
```

## Erwartete JSON-Struktur

```json
{
  "result": {
    "dartScoringResult": {
      "player1Stats": {
        "name": "Jonas",
        "average": 119.4,
        "legData": [
          {
            "legNumber": 1,
            "average": 150.5,
            "darts": 6,
            "score": 301,
            "won": true,
            "timestamp": "2025-11-13T21:44:53.510Z"
          }
        ]
      },
      "player2Stats": {
        "name": "Peter",
        "legData": [
          {
            "legNumber": 1,
            "darts": 9,
            "won": true
          }
        ]
      }
    }
  }
}
```

## Debug-Ausgaben

### Erfolgreiche Verarbeitung:
```
[STATS] Processing match update for class Bronze
[STATS] Found dartScoringResult in result
[STATS] Extracted Player1: Jonas, Avg: 119.4, LegData Count: 1
[STATS] Leg 1: 6 darts, Ø 150.5, Won: True
[STATS] Extracted Player2: Peter, Avg: 115.2, LegData Count: 1
[STATS] Leg 1: 9 darts, Ø 100.3, Won: True
[STATS] Processing enhanced dart statistics (with legData) for Jonas vs Peter
```

### Wenn keine legData vorhanden:
```
[STATS] No dartScoringResult found in JSON structure
[STATS] Processing direct WebSocket statistics for Jonas vs Peter
```

## Verifikation

### 1. Prüfen Sie die Debug-Console
Beim Speichern eines Match-Ergebnisses sollten Sie sehen:
```
[STATS] Processing enhanced dart statistics (with legData) for [Spieler1] vs [Spieler2]
[STATS] Leg X: Y darts, Ø Z, Won: True/False
```

### 2. Prüfen Sie die Statistik-Tabelle
Nach dem Neuladen der Statistiken sollten die Spalten gefüllt sein:
```
Min Darts/Leg: 6
? Darts/Leg: 7.5
Beste Leg-Effizienz: 6 Darts @ 150.5
```

### 3. Tooltip prüfen
Der Tooltip sollte zeigen:
```
Wenigste Darts/Leg: 6
? Darts/Leg: 7.5
Beste Leg-Effizienz: 6 Darts @ 150.5
```

## Testing

### Test-Fall 1: Match mit gewonnenem Leg
**Input JSON:**
```json
{
  "player1Stats": {
    "legData": [
      {"legNumber": 1, "darts": 6, "average": 150.5, "won": true}
    ]
  }
}
```

**Erwartete Ausgabe:**
- Min Darts/Leg: `6`
- ? Darts/Leg: `6.0`
- Beste Leg-Effizienz: `6 Darts @ 150.5`

### Test-Fall 2: Match mit gewonnenem und verlorenem Leg
**Input JSON:**
```json
{
  "player1Stats": {
    "legData": [
      {"legNumber": 1, "darts": 6, "average": 150.5, "won": true},
      {"legNumber": 2, "darts": 9, "average": 100.3, "won": false}
    ]
  }
}
```

**Erwartete Ausgabe:**
- Min Darts/Leg: `6` (nur gewonnenes Leg gezählt!)
- ? Darts/Leg: `6.0` (nur gewonnenes Leg gezählt!)
- Beste Leg-Effizienz: `6 Darts @ 150.5`

### Test-Fall 3: Multiple gewonnene Legs
**Input JSON:**
```json
{
  "player1Stats": {
    "legData": [
      {"legNumber": 1, "darts": 6, "average": 150.5, "won": true},
      {"legNumber": 2, "darts": 9, "average": 100.3, "won": true},
      {"legNumber": 3, "darts": 12, "average": 75.2, "won": true}
    ]
  }
}
```

**Erwartete Ausgabe:**
- Min Darts/Leg: `6`
- ? Darts/Leg: `9.0` ((6+9+12)/3)
- Beste Leg-Effizienz: `6 Darts @ 150.5`

## Häufige Fehler

### ? Spalten zeigen "-"
**Ursache:** `legData` wird nicht geparst
**Lösung:** 
1. Prüfen Sie Debug-Output: Wird `ExtractEnhancedDartStatistics` verwendet?
2. Wenn nein: JSON-Struktur prüfen (siehe oben)
3. Sicherstellen, dass `result.dartScoringResult` existiert

### ? Nur einige Spieler haben Daten
**Ursache:** Alte Matches ohne `legData`
**Lösung:** Normal! Nur neue Matches (nach diesem Update) haben `legData`

### ? Berechnungen falsch
**Ursache:** `won: false` Legs werden mitgezählt
**Lösung:** ? Bereits behoben - Filter auf `.Where(ld => ld.Won)`

## Änderungshistorie

### v1.1 (Aktuell)
- ? JSON-Parsing Priorität geändert
- ? `Won` Property hinzugefügt
- ? Filterung auf gewonnene Legs
- ? Debug-Logging verbessert

### v1.0 (Initial)
- Basic `legData` Parsing
- Ohne `Won` Property
- Falsche Parsing-Reihenfolge

## Nächste Schritte

1. **Testen Sie mit einem neuen Match:**
   - Spielen Sie ein Match über das Hub
   - Prüfen Sie die Debug-Console
   - Verifizieren Sie die Statistik-Tabelle

2. **Bei Problemen:**
   - Kopieren Sie die Debug-Console-Ausgabe
   - Kopieren Sie das JSON aus der WebSocket-Nachricht
   - Öffnen Sie ein Issue mit beiden Informationen

3. **Dokumentation:**
   - Aktualisieren Sie CHANGELOG.md
   - Fügen Sie Screenshots zur Dokumentation hinzu

---

**Status:** ? Alle Issues behoben, bereit zum Testen
**Datum:** 2025-01-13
**Version:** 1.1
