# Tournament Hub - Round Robin Finals Rules Synchronisation

## Übersicht

Die Dart Tournament Planer Desktop-Anwendung unterstützt jetzt **rundenspezifische Regeln für Round Robin Finals**. Diese Dokumentation beschreibt die notwendigen Anpassungen am Tournament Hub Server, um diese Funktionalität vollständig zu unterstützen.

## Änderungen in der Desktop-Anwendung

### 1. Neue Datenstruktur: `RoundRobinFinalsRound` Enum
```csharp
public enum RoundRobinFinalsRound
{
    Finals  // Finalrunde im Round Robin Modus
}
```

### 2. Erweiterte GameRules Synchronisation

Finals-Matches werden nun mit **rundenspezifischen Regeln** synchronisiert:

```json
{
  "id": "uuid-match-123",
  "matchType": "Finals",
  "gameRulesId": "1_Finals",
  "gameRulesUsed": {
    "id": "1_Finals",
    "name": "Platin Finalrunde",
    "gamePoints": 501,
    "startingScore": 501,
    "gameMode": "Points501",
    "finishMode": "DoubleOut",
    "doubleOut": true,
    "singleOut": false,
    "setsToWin": 0,
    "legsToWin": 5,
    "legsPerSet": 3,
    "maxSets": 5,
    "maxLegsPerSet": 3,
    "playWithSets": false,
    "classId": 1,
    "className": "Platin",
    "matchType": "Finals",
    "isDefault": false,
    "dartScoringReady": true,
    "gameTypeString": "501",
    "finishTypeString": "DoubleOut",
    "finalsSpecific": {
      "isFinalsMatch": true,
      "usesRoundRobinFinalsRules": true,
      "escalated": false,
      "roundRobinFinalsRound": "Finals"
    }
  }
}
```

## Notwendige Hub-Server-Anpassungen

### Option 1: Keine Änderungen erforderlich (Empfohlen)

Der Hub-Server muss **KEINE** Änderungen vornehmen, wenn:

1. **Die `gameRulesUsed` Struktur bereits korrekt verarbeitet wird**
   - Der Hub sollte die `gameRulesUsed` Objekte bereits speichern und an Clients weitergeben
   - Die neuen Felder (`finalsSpecific`) sind optional und müssen nicht explizit verarbeitet werden

2. **Die Dart-Scoring Seite dynamisch GameRules lädt**
   - Die `dart-scoring.html` Seite sollte die `gameRulesUsed` vom Match-Objekt verwenden
   - Keine Hardcoding von Regeln auf Basis von `matchType`

### Überprüfung der aktuellen Implementation

Prüfen Sie in Ihrem Hub-Server:

#### 1. Match-Endpunkt (`GET /api/matches/:uuid`)

```javascript
// ? KORREKT: gameRulesUsed wird mit dem Match zurückgegeben
{
  "match": {
    "id": "uuid-123",
    "matchType": "Finals",
    "gameRulesUsed": { /* komplettes gameRulesUsed Objekt */ },
    "player1": "Spieler A",
    "player2": "Spieler B",
    ...
  }
}

// ? FALSCH: gameRulesUsed fehlt oder wird gefiltert
{
  "match": {
    "id": "uuid-123",
    "matchType": "Finals",
    // gameRulesUsed fehlt!
    ...
  }
}
```

#### 2. Dart-Scoring Seite JavaScript

```javascript
// ? KORREKT: Verwendet gameRulesUsed vom Match
const gameRules = matchData.gameRulesUsed || matchData.gameRules;
const setsToWin = gameRules.setsToWin;
const legsToWin = gameRules.legsToWin;
const playWithSets = gameRules.playWithSets;

// ? FALSCH: Hardcoded Regeln basierend auf matchType
if (matchData.matchType === "Finals") {
    // NICHT so! Dies ignoriert rundenspezifische Regeln
    setsToWin = 3; // Hardcoded
    legsToWin = 5; // Hardcoded
}
```

### Option 2: Erweiterte Validierung (Optional)

Wenn Sie explizite Validierung für Finals-Regeln implementieren möchten:

#### Server-seitige Validierung (Node.js/Express Beispiel)

```javascript
// tournament-hub/routes/matches.js

function validateFinalsRules(match) {
    if (match.matchType === 'Finals' && match.gameRulesUsed) {
        const rules = match.gameRulesUsed;
        
        // Validiere dass Finals-spezifische Regeln vorhanden sind
        if (!rules.finalsSpecific) {
            console.warn(`[VALIDATION] Finals match ${match.id} missing finalsSpecific metadata`);
            // Optional: Setze Defaults
            rules.finalsSpecific = {
                isFinalsMatch: true,
                usesRoundRobinFinalsRules: true,
                escalated: false,
                roundRobinFinalsRound: "Finals"
            };
        }
        
        // Validiere Regelwerte
        if (rules.playWithSets) {
            if (rules.setsToWin === undefined || rules.setsToWin < 0) {
                throw new Error('Invalid setsToWin for Finals match with sets');
            }
        } else {
            if (rules.legsToWin === undefined || rules.legsToWin < 1) {
                throw new Error('Invalid legsToWin for Finals match without sets');
            }
        }
    }
}

// In Match-Update Handler
router.post('/api/matches/:uuid/update', async (req, res) => {
    const match = await getMatch(req.params.uuid);
    
    // Validiere Finals-Regeln
    validateFinalsRules(match);
    
    // ... rest of update logic
});
```

## Testing & Verification

### Test-Szenario 1: Finals ohne Sets

**Desktop-Anwendung konfiguriert:**
- Round Robin Finals: 0 Sets, 5 Legs

**Erwartete Hub-Synchronisation:**
```json
{
  "gameRulesUsed": {
    "setsToWin": 0,
    "legsToWin": 5,
    "playWithSets": false,
    "matchType": "Finals"
  }
}
```

**Dart-Scoring Seite sollte zeigen:**
- Keine Sets-Spalten
- "First to 5 Legs" anzeigen
- Legs-basierte Eingabe ermöglichen

### Test-Szenario 2: Finals mit Sets

**Desktop-Anwendung konfiguriert:**
- Round Robin Finals: 2 Sets, 3 Legs, 3 Legs per Set

**Erwartete Hub-Synchronisation:**
```json
{
  "gameRulesUsed": {
    "setsToWin": 2,
    "legsToWin": 3,
    "legsPerSet": 3,
    "playWithSets": true,
    "matchType": "Finals"
  }
}
```

**Dart-Scoring Seite sollte zeigen:**
- Sets-Spalten anzeigen
- "First to 2 Sets (3 Legs per Set)" anzeigen
- Sets- und Legs-basierte Eingabe ermöglichen

## Häufige Probleme und Lösungen

### Problem 1: Finals-Matches verwenden falsche Regeln

**Symptom:** Dart-Scoring Seite zeigt immer die gleichen Regeln für Finals, unabhängig von der Konfiguration

**Ursache:** Hub-Server filtert `gameRulesUsed` Objekt heraus oder Dart-Scoring Seite ignoriert es

**Lösung:**
1. Prüfen Sie den Match-Endpunkt Response
2. Stellen Sie sicher, dass `gameRulesUsed` vollständig zurückgegeben wird
3. Aktualisieren Sie die Dart-Scoring Seite, um `gameRulesUsed` zu verwenden

### Problem 2: Alte Matches haben keine finalsSpecific Daten

**Symptom:** Bereits existierende Finals-Matches haben keine `finalsSpecific` Metadaten

**Ursache:** Matches wurden vor dem Update synchronisiert

**Lösung:** Migration Script (Optional)

```javascript
// migration-add-finals-metadata.js
async function migrateFinalsMatches() {
    const finalsMatches = await db.matches.find({ matchType: 'Finals' });
    
    for (const match of finalsMatches) {
        if (match.gameRulesUsed && !match.gameRulesUsed.finalsSpecific) {
            match.gameRulesUsed.finalsSpecific = {
                isFinalsMatch: true,
                usesRoundRobinFinalsRules: true,
                escalated: false,
                roundRobinFinalsRound: "Finals"
            };
            
            await db.matches.update({ _id: match._id }, { $set: { gameRulesUsed: match.gameRulesUsed } });
            console.log(`Migrated Finals match ${match.id}`);
        }
    }
    
    console.log(`Migration completed: ${finalsMatches.length} matches processed`);
}
```

## API-Kompatibilität

### Bestehende Endpoints bleiben unverändert

Alle bestehenden API-Endpoints funktionieren weiterhin:
- `GET /api/matches/:uuid` - Gibt Match mit `gameRulesUsed` zurück
- `POST /api/matches/:uuid/update` - Akzeptiert Match-Updates
- `POST /api/tournaments/:id/sync-full` - Synchronisiert alle Matches

### Neue optionale Felder

Die folgenden Felder sind **optional** und müssen nicht verarbeitet werden:
- `gameRulesUsed.finalsSpecific` - Metadaten für Finals-Matches
- `gameRulesUsed.finalsSpecific.usesRoundRobinFinalsRules` - Flag für rundenspezifische Regeln
- `gameRulesUsed.finalsSpecific.roundRobinFinalsRound` - Name der Finals-Runde

## Zusammenfassung

### ? Was funktioniert bereits (keine Änderungen erforderlich):

1. **Basis-Synchronisation**: Finals-Matches werden mit `gameRulesUsed` synchronisiert
2. **Match-Updates**: WebSocket-Updates für Finals-Matches funktionieren
3. **Dart-Scoring Integration**: Wenn dynamisch `gameRulesUsed` geladen wird

### ?? Was geprüft werden sollte:

1. **Match-Endpunkt**: Gibt `gameRulesUsed` vollständig zurück?
2. **Dart-Scoring Seite**: Verwendet sie `gameRulesUsed` statt Hardcoding?
3. **WebSocket-Updates**: Werden `gameRulesUsed` bei Match-Updates übertragen?

### ?? Optionale Erweiterungen:

1. **Validierung**: Server-seitige Validierung der Finals-Regeln
2. **Migration**: Script für existierende Finals-Matches
3. **Dokumentation**: API-Docs um `finalsSpecific` erweitern

## Support & Kontakt

Bei Fragen oder Problemen:
1. Prüfen Sie die Console-Logs der Dart-Scoring Seite
2. Überprüfen Sie die Network-Requests zum Hub-Server
3. Aktivieren Sie Debug-Modus in der Desktop-Anwendung (Hub Debug Window)

---

**Version:** 1.0  
**Datum:** 2024  
**Kompatibilität:** Tournament Hub v1.0+, Dart Tournament Planer v1.0+
