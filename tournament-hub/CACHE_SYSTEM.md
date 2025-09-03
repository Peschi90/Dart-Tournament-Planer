# 💾 Dart Scoring Cache System

Das Dart Scoring Cache System bietet automatisches Speichern und Wiederherstellen von Spielständen, damit Benutzer keine Daten verlieren, wenn sie versehentlich die Seite schließen oder von einem anderen Gerät aus zugreifen möchten.

## 🚀 Features

### Automatisches Speichern
- **Auto-Save alle 10 Sekunden** während des Spiels
- **Sofortiges Speichern nach jedem Wurf** (mit 2-Sekunden-Verzögerung)
- **Server-seitige Persistierung** sowohl im Memory-Cache als auch auf der Festplatte
- **Geräteübergreifende Synchronisation**

### Automatisches Laden
- **Automatische Wiederherstellung** beim Neuladen der Seite
- **Smart-Loading**: Lädt automatisch wenn möglich, zeigt sonst manuellen Button
- **Alters-Validierung**: Verwirft zu alte Spielstände (24h für aktive, 7 Tage für persistierte)

### Benutzeroberfläche
- **Status-Indikator** im Header zeigt Cache-Status an
- **Manueller Restore-Button** wenn automatisches Laden fehlschlägt
- **Eleganter Dialog** mit Spielstand-Informationen
- **Responsive Design** für alle Bildschirmgrößen

## 🏗️ Architektur

```
📁 Cache System Components
├── 🛠️ Server-Side (API Routes)
│   └── routes/match-state.js - REST API für Cache-Operations
├── 💾 Client-Side Core
│   └── js/dart-scoring-cache.js - Cache-Logik und Auto-Save
├── 🎨 User Interface
│   └── js/dart-scoring-cache-ui.js - UI-Komponenten und Dialoge
└── 🔄 Integration
    └── js/dart-scoring-main.js - Erweitert um Cache-Funktionalität
```

## 📡 API Endpoints

### Cache Management
```http
POST   /api/match-state/{tournamentId}/{matchId}/save   # Spielstand speichern
GET    /api/match-state/{tournamentId}/{matchId}/load   # Spielstand laden  
GET    /api/match-state/{tournamentId}/{matchId}/check  # State-Existenz prüfen
DELETE /api/match-state/{tournamentId}/{matchId}/clear  # Spielstand löschen
GET    /api/match-state/stats                          # Cache-Statistiken
```

## 💾 Speicher-Strategie

### Dual-Layer Caching
1. **Memory Cache**: Schneller Zugriff für aktive Spiele
2. **Disk Cache**: Persistente Speicherung für Ausfallsicherheit

### Auto-Cleanup
- **Aktive States**: Verfallen nach 24 Stunden ohne Aktivität
- **Persistente States**: Verfallen nach 7 Tagen
- **Match-Completion**: Automatisches Löschen nach erfolgreichem Match-Ende

## 🎮 Benutzer-Experience

### Automatischer Workflow
1. **Spieler öffnet Dart Scoring**
2. **System prüft automatisch auf gespeicherte Daten**
3. **Falls vorhanden**: Automatische Wiederherstellung
4. **Falls Auto-Load fehlschlägt**: Manueller Button wird angezeigt
5. **Während des Spiels**: Kontinuierliches Auto-Save
6. **Nach Match-Ende**: Automatische Cache-Bereinigung

### Fallback-Strategie
```
Auto-Load erfolgreich ✅
    ↓
Spiel fortsetzten mit wiederhergestelltem State
    
Auto-Load fehlgeschlagen ⚠️
    ↓
Zeige manuellen Restore-Button
    ↓
User-Entscheidung: Wiederherstellen oder Neu starten
```

## 🔧 Entwickler-Tools

### Debug-Funktionen
```javascript
// Cache-Status prüfen
window.debugDartScoring.getCacheStatus()

// Cache manuell prüfen
window.debugDartScoring.checkCache()

// State manuell speichern
window.debugDartScoring.saveState()

// State manuell laden  
window.debugDartScoring.loadState()

// Cache löschen
window.debugDartScoring.clearCache()

// Vollständige Debug-Info
window.debugDartScoring.getDebugInfo()
```

### Monitoring
```javascript
// Cache-Statistiken
fetch('/api/match-state/stats')
  .then(r => r.json())
  .then(console.log)
```

## ⚙️ Konfiguration

### Cache-Einstellungen
```javascript
// In DartScoringCache Konstruktor
this.autoSaveInterval = 10000;      // 10 Sekunden
this.saveOnThrowDelay = 2000;       // 2 Sekunden nach Wurf
```

### Server-Einstellungen
```javascript
// In routes/match-state.js
const maxAge = 24 * 60 * 60 * 1000; // 24 Stunden für aktive States
const diskMaxAge = 7 * 24 * 60 * 60 * 1000; // 7 Tage für persistierte States
```

## 🛡️ Fehlerbehandlung

### Robustheit
- **Network Failures**: Graceful Fallbacks ohne Spielunterbrechung
- **Server Errors**: Lokale Fortsetzung mit Retry-Mechanismus  
- **Corrupt Data**: Validation und automatische Bereinigung
- **Browser Compatibility**: Progressive Enhancement

### User Feedback
- **Success**: ✅ Grüner Status-Indikator
- **In Progress**: 💾 Blauer Speicher-Indikator
- **Error**: ⚠️ Roter Fehler-Indikator mit Fallback
- **Disabled**: ❌ Grauer deaktiviert-Status

## 🚀 Performance

### Optimierungen
- **Change Detection**: Speichert nur bei tatsächlichen Änderungen
- **Throttling**: Begrenzte Speicher-Frequenz verhindert Server-Overload  
- **Compression**: JSON-Daten werden effizient serialisiert
- **Memory Management**: Automatische Bereinigung alter Daten

### Skalierung
- **Multi-Device Support**: Ein Match kann von verschiedenen Geräten aus zugegriffen werden
- **Concurrent Safety**: Race-Condition-sichere Speicher-Operations
- **Load Balancing**: Stateless API-Design für horizontale Skalierung

## 🔮 Zukunft

### Planned Features
- **Cloud Backup**: Integration mit externen Cloud-Speichern
- **Version History**: Mehrere Restore-Punkte pro Match
- **Offline Support**: Service Worker für echtes Offline-Caching
- **Analytics**: Detaillierte Nutzungsstatistiken des Cache-Systems

---

**💾 Das Cache-System sorgt dafür, dass kein Dart-Spiel mehr verloren geht! 🎯**
