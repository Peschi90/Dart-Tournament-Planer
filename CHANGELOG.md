## v0.1.12
- moved
	- tournament Hub out of the Repo.

- bugfixes
	- fixed some bugs in the license system.
	- **Reset Buttons Functionality**: Fixed reset buttons behavior to preserve match structure
		- Reset KO Phase button now only resets KO phase data (preserves group phase results)
		- Reset Finals button now only resets finals phase data (preserves group phase results)
		- Reset Matches button now context-aware (works in Group, KO, and Finals phases)

- improvements
	- **Landscape Format Support**: Automatic landscape format (1122.5 x 793.7px) when QR codes are available
	- **Larger Font Sizes**: Doubled font sizes (22pt headers, 20pt cells) for better readability
	- **Multi-Page Support**: Automatic creation of continuation pages for many matches
		- Intelligent calculation of matches per page based on available space
		- Page numbering for overflow pages ("Page 2", "Page 3", etc.)
		- Support for group phase, finals and knockout brackets
	- **QR Code Optimization**: 90x90px QR codes in 110px wide columns
	- **Optimized Table Layouts**: 
		- Widened match number column (60px instead of 40px)
		- Adjusted row height (95px) for optimal space utilization
		- 5 matches with QR codes fit on one page
	- Better code organization and reusability
	- **Context-Aware Match Reset**: 
		- Reset Matches button now intelligently resets only match results based on current phase
		- Preserves match generation and tournament structure
		- Works across Group Phase, KO Phase, and Finals Phase
		- Button activation logic improved for all tournament phases
	- **Tournament Overview Auto-Scroll**: 
		- Auto-scroll feature now synchronized with auto-cycle timing
		- ✅ **Intelligent Scroll Strategies**:
			- **Small Content (<10px)**: Delayed single-scroll at 50% of cycle time for optimal visibility
			- **Large Content (≥10px)**: Smooth animated scrolling over entire cycle duration with easing

## v0.1.11
- adds
	- Progressbar for update to new version implemented. 
- Bugfixes
	- fixed some translation bugs.

## v0.1.10
- Bugfixes
	- fixed bug for license request Mail.

## v0.1.9
- Bugfixes
	- improve sharpness and overall clearance.

## v0.1.8

### 🎯 Neue Features
- **🔑 Vollständiges Lizenz-System**: Umfassendes Feature-Management mit Core/Premium-Features, Offline-Validierung und Lizenz-Verwaltung
- **🎨 Dark/Light Theme-System**: Vollständiger Theme-Wechsel mit persistenten Einstellungen und Ein-Klick-Toggle
- **📊 Erweiterte Spieler-Statistiken**: Neue Statistik-Spalten "Schnellstes Match" und "Wenigste Würfe" mit detaillierter Performance-Analyse
- **🌐 Tournament Hub Integration**: Verbesserte Echtzeit-Synchronisation mit WebSocket-Verbindungen und QR-Code-Unterstützung
- **📱 Professioneller Startup**: Animierter Splash Screen mit Fortschrittsanzeigen und modernen Animationen
- **🎭 Statistiken-Tab**: Neuer dedizierter Tab für detaillierte Spieler-Statistiken in jeder Turnierklasse

### 🔑 Lizenz-System Features
- **Premium-Feature-Management**: Granulare Kontrolle über erweiterte Funktionen
- **Offline-Lizenz-Validierung**: Funktionalität auch ohne Internetverbindung
- **Lizenz-Dialoge**: Benutzerfreundliche Aktivierung, Status-Anzeige und Verwaltung
- **Feature-gesteuerte UI**: Dynamische Anzeige basierend auf Lizenz-Status
- **Sicherheits-Features**: Hardware-ID-basierte Lizenzierung mit Aktivierungslimits

### 🎨 Theme-System
- **Light/Dark Mode Toggle**: Vollständiger Theme-Wechsel über Menü-Button
- **Konsistente Theme-Anwendung**: Einheitliche Darstellung über alle UI-Elemente
- **Theme-Persistierung**: Speicherung der Theme-Auswahl zwischen App-Starts
- **Moderne Farbpaletten**: Professionelle Farb-Schemata für beide Modi
- **Echtzeit-Theme-Wechsel**: Theme-Änderung ohne Anwendungsneustart

### 🌐 Hub-Integration Verbesserungen
- **WebSocket-Statistik-Integration**: Vollständige Extraktion und Verarbeitung von Dart-Statistiken aus WebSocket-Nachrichten
- **QR-Code-Support**: QRCoder-Integration für einfachen Tournament-Zugang
- **Erweiterte Match-Daten**: Verarbeitung von `dartScoringResult` für detaillierte Spielanalysen
- **Automatische Synchronisation**: Echtzeit-Updates der Spieler-Statistiken bei Match-Abschluss
- **Debug-Konsole**: Globale Debug-Konsole für Hub-Verbindungsdiagnose
- **Fallback-Mechanismen**: Robuste Verarbeitung bei verschiedenen Datenformaten

### 📊 Statistik-Features
- **Match-Effizienz**: Anzeige der schnellsten Spieldauer (MM:SS Format)
- **Wurf-Effizienz**: Tracking der wenigsten Würfe pro Match
- **Detaillierte Daten**: High Finish Details mit Darts-Aufschlüsselung
- **Leg-Averages**: Verfolgung individueller Leg-Performance
- **Checkout-Statistiken**: Anzahl und Details aller erfolgreichen Checkouts
- **180er-Tracking**: Vollständige Verfolgung aller Maximum-Scores
- **Score-Analyse**: Tracking von 26er-Scores und Performance-Trends

### 🔧 Verbesserungen
- **UI-Button-Aktivierung**: Reset-Buttons sind nicht mehr fälschlicherweise ausgegraut
- **Match-Statistik-Verarbeitung**: Erweiterte Extraktion von 180ern, 26er-Scores, High Finishes und Checkouts
- **Lokalisierung**: Neue deutsche und englische Übersetzungen für alle neuen Features
- **Performance**: Optimierte Statistik-Berechnung und UI-Updates
- **Debug-Ausgaben**: Erweiterte Logging-Funktionalität für bessere Fehlerdiagnose
- **Splash Screen**: Professionelle Startup-Erfahrung mit Ladeanimationen und Status-Updates

### 🏗️ Technische Verbesserungen
- **Manager-Klassen**: Aufgeteilte UI-Logik in spezialisierte Manager (TournamentTabUIManager, PlayerStatisticsManager, TranslationManager)
- **ThemeService**: Dedizierte Service-Klasse für Theme-Verwaltung
- **LicenseFeatureService**: Umfassendes Lizenz-Feature-Management
- **Event-Handling**: Verbesserte Event-Delegation für bessere Wartbarkeit
- **Code-Organisation**: Klarere Trennung von Verantwortlichkeiten
- **Typsicherheit**: Enhanced null-safety und Error-Handling
- **.NET 9.0 Upgrade**: Aktualisierung auf die neueste .NET-Version mit C# 13.0

### 📱 Benutzerfreundlichkeit
- **Animierter Startup**: Professioneller Splash Screen mit Fortschrittsbalken und Status-Updates
- **Theme-Toggle**: Ein-Klick-Wechsel zwischen Light- und Dark-Mode
- **Formatierung**: Zeitangaben in benutzerfreundlichem MM:SS Format
- **Tooltips**: Erweiterte Hilfetexte für neue Statistik-Felder und Features
- **Sortierung**: Verbesserte Sortierungsmöglichkeiten für Statistik-Tabellen
- **Lizenz-Management**: Intuitive Lizenz-Aktivierung und -verwaltung

### 🔄 API & Integration
- **QRCoder-Package**: Neue Abhängigkeit für QR-Code-Generierung (v1.6.0)
- **System.Management**: Neue Abhängigkeit für Hardware-ID-Generierung (v9.0.0)
- **WebSocket-Protokoll**: Erweiterte Unterstützung für Tournament Hub WebSocket-Messages
- **JSON-Parsing**: Robuste Verarbeitung komplexer Match-Daten-Strukturen und Lizenz-Validierung
- **Backward-Compatibility**: Unterstützung für bestehende und neue Datenformate
- **Error-Recovery**: Verbesserte Wiederherstellung bei Verbindungsfehlern

### 🐛 Fehlerbehebungen
- **Reset-Button-Problem**: Tournament Reset-Buttons funktionieren wieder korrekt
- **Statistik-Speicherung**: Korrekte Persistierung in tournament-data.json
- **UI-Synchronisation**: Verbesserte Aktualisierung der Benutzeroberfläche
- **Null-Reference-Behandlung**: Robustere Fehlerbehandlung bei fehlenden Daten
- **Theme-Konsistenz**: Korrekte Theme-Anwendung über alle UI-Komponenten
- **Lizenz-Validierung**: Robuste Offline-/Online-Lizenz-Überprüfung

### 📚 Dokumentation
- **Code-Kommentare**: Erweiterte Dokumentation der neuen Features
- **Debug-Logging**: Detaillierte Trace-Ausgaben für Entwicklung und Wartung
- **Lokalisierungs-Keys**: Vollständige Übersetzungsschlüssel für alle UI-Elemente
- **License-Integration**: Umfassende Dokumentation des Lizenz-Systems
- **Theme-System**: Dokumentation des Theme-Wechsel-Mechanismus

### 🎁 Premium Features (Lizenz erforderlich)
- **📈 Erweiterte Statistiken**: Detaillierte Spieler-Performance-Analyse mit Hub-Integration
- **🌐 Tournament Hub Premium**: Erweiterte Hub-Verbindungsfeatures
- **🖨️ Enhanced Printing**: Professionelle Drucklayouts und erweiterte Optionen
- **📊 Tournament Overview Premium**: Erweiterte Präsentationsmodi und Features

### 🚀 Performance & Stabilität
- **Async/Await-Pattern**: Durchgängige Verwendung asynchroner Programmierung
- **Memory Management**: Verbesserte Ressourcenverwaltung und Garbage Collection
- **Exception Handling**: Robuste Fehlerbehandlung mit Benutzer-Feedback
- **UI Responsiveness**: Verbesserte UI-Reaktionsfähigkeit durch Background-Threading
- **Startup Performance**: Optimierte Anwendungs-Startzeit mit Splash Screen


## v0.0.0