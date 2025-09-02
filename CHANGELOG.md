## v0.1.8

### 🎯 Neue Features
- **Erweiterte Spieler-Statistiken**: Neue Statistik-Spalten "Schnellstes Match" und "Wenigste Würfe" hinzugefügt
- **WebSocket-Statistik-Integration**: Vollständige Extraktion und Verarbeitung von Dart-Statistiken aus WebSocket-Nachrichten
- **Tournament Hub Integration**: Verbesserte Echtzeit-Synchronisation mit Tournament Hub über WebSocket-Verbindungen
- **Statistiken-Tab**: Neuer dedizierter Tab für detaillierte Spieler-Statistiken in jeder Turnierklasse

### 🔧 Verbesserungen
- **UI-Button-Aktivierung**: Reset-Buttons sind nicht mehr fälschlicherweise ausgegraut
- **Match-Statistik-Verarbeitung**: Erweiterte Extraktion von 180ern, 26er-Scores, High Finishes und Checkouts
- **Lokalisierung**: Neue deutsche und englische Übersetzungen für alle Statistik-Features
- **Performance**: Optimierte Statistik-Berechnung und UI-Updates
- **Debug-Ausgaben**: Erweiterte Logging-Funktionalität für bessere Fehlerdiagnose

### 📊 Statistik-Features
- **Match-Effizienz**: Anzeige der schnellsten Spieldauer (MM:SS Format)
- **Wurf-Effizienz**: Tracking der wenigsten Würfe pro Match
- **Detaillierte Daten**: High Finish Details mit Darts-Aufschlüsselung
- **Leg-Averages**: Verfolgung individueller Leg-Performance
- **Checkout-Statistiken**: Anzahl und Details aller erfolgreichen Checkouts

### 🌐 Hub-Integration
- **Direkte WebSocket-Statistiken**: Extraktion aus `statistics`-Sektion der WebSocket-Nachrichten
- **Erweiterte Match-Daten**: Verarbeitung von `dartScoringResult` für detaillierte Spielanalysen
- **Automatische Synchronisation**: Echtzeit-Updates der Spieler-Statistiken bei Match-Abschluss
- **Fallback-Mechanismen**: Robuste Verarbeitung bei verschiedenen Datenformaten

### 🐛 Fehlerbehebungen
- **Reset-Button-Problem**: Tournament Reset-Buttons funktionieren wieder korrekt
- **Statistik-Speicherung**: Korrekte Persistierung in tournament-data.json
- **UI-Synchronisation**: Verbesserte Aktualisierung der Benutzeroberfläche
- **Null-Reference-Behandlung**: Robustere Fehlerbehandlung bei fehlenden Daten

### 🏗️ Technische Verbesserungen
- **Manager-Klassen**: Aufgeteilte UI-Logik in spezialisierte Manager (TournamentTabUIManager, PlayerStatisticsManager)
- **Event-Handling**: Verbesserte Event-Delegation für bessere Wartbarkeit
- **Code-Organisation**: Klarere Trennung von Verantwortlichkeiten
- **Typsicherheit**: Enhanced null-safety und Error-Handling

### 📱 Benutzerfreundlichkeit
- **Formatierung**: Zeitangaben in benutzerfreundlichem MM:SS Format
- **Tooltips**: Erweiterte Hilfetexte für neue Statistik-Felder
- **Sortierung**: Verbesserte Sortierungsmöglichkeiten für Statistik-Tabellen

### 🔄 API & Integration
- **WebSocket-Protokoll**: Erweiterte Unterstützung für Tournament Hub WebSocket-Messages
- **JSON-Parsing**: Robuste Verarbeitung komplexer Match-Daten-Strukturen
- **Backward-Compatibility**: Unterstützung für bestehende und neue Datenformate
- **Error-Recovery**: Verbesserte Wiederherstellung bei Verbindungsfehlern

### 📚 Dokumentation
- **Code-Kommentare**: Erweiterte Dokumentation der neuen Statistik-Features
- **Debug-Logging**: Detaillierte Trace-Ausgaben für Entwicklung und Wartung
- **Lokalisierungs-Keys**: Vollständige Übersetzungsschlüssel für alle UI-Elemente


## v0.0.0