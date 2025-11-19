# 🎯 Dart Tournament Planner

**[English](#english)** | **[Deutsch](#deutsch)**

---

<a name="english"></a>
## 🇬🇧 English Version

A modern WPF application for managing dart tournaments with professional features for tournament organizers.

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![C#](https://img.shields.io/badge/C%23-13.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)
![Version](https://img.shields.io/badge/Version-0.1.13-brightgreen)

### 🏆 Core Features

#### 🎮 Tournament Management
- **Multiple Tournament Classes**: Manage up to 4 different classes (Platinum, Gold, Silver, Bronze)
- **Flexible Group Phase**: Round-robin system with unlimited groups
- **Knockout System**: Single or double elimination with winner/loser brackets
- **Final Rounds**: Round-robin finals for qualified players
- **Auto-Save System**: Configurable automatic saving with adjustable intervals
- **Professional Workflows**: Simplified tournament creation and management
- **Bye System**: Automatic bye assignment for odd player counts

#### ⚡ **PowerScoring System** (NEW!)
Intelligent player seeding based on performance data - perfect for fair tournament distribution!

- **Scoring Sessions**: Create scoring sessions with customizable rules
  - 1x3 throws: Quick assessment (3 darts)
  - 8x3 throws: Standard evaluation (24 darts)
  - 10x3 throws: Detailed evaluation (30 darts)
  - 15x3 throws: Very detailed evaluation (45 darts)
- **Remote Scoring**: QR code integration for remote score entry via Tournament Hub
- **Player Ranking**: Automatic ranking based on total and average scores
- **Intelligent Group Distribution**: Smart player distribution across tournament classes
  - Support for 5 classes (Platinum, Gold, Silver, Bronze, Iron)
  - Configurable groups per class (1-4 groups)
  - Configurable players per group (2-6 players)
- **Distribution Modes**: 
  - ⚖️ **Balanced**: Even distribution by ranking
  - 🐍 **Snake Draft**: 1-2-3-4-4-3-2-1 zigzag pattern for balanced groups
  - 🔝 **Top-Heavy**: Strongest players grouped first
  - 🎲 **Random**: Random distribution
- **Advanced Settings**: 
  - Class-specific group and player counts
  - Skip classes functionality
  - Individual player limits per class
  - Distribution preview before confirmation
- **Tournament Creation**: 
  - Create new tournament directly from PowerScoring distribution
  - Automatic tournament data migration
  - Existing tournament backup before creation
  - Seamless UI transition to tournament view
- **Session Management**: 
  - Session persistence (auto-save/auto-load)
  - Tournament-ID integration for Hub synchronization
  - QR code generation for all players
  - Live scoring updates via WebSocket

#### 📊 **Extended Player Statistics**
- **Match Efficiency**: Display fastest match duration (MM:SS format)
- **Throw Efficiency**: Track fewest darts per match
- **Detailed Performance Data**: High finish details with darts breakdown
- **180 Tracking**: Complete maximum score tracking
- **Checkout Statistics**: Count and details of all successful checkouts
- **Leg Averages**: Track individual leg performance
- **Score Analysis**: Track 26+ scores and performance trends
- **Dedicated Statistics Tab**: Separate display for each tournament class

#### 🖨️ **Professional Print System**
- **Tournament Statistics Printing**: Comprehensive tournament reports with detailed statistics
- **Print Dialog**: User-friendly interface for selecting print contents
- **Print Preview**: Real-time preview of documents before printing
- **Flexible Options**: Print individual groups, complete tournaments, or specific phases
- **Professional Layout**: Formatted reports with tables, rankings, and match results
- **Multi-Phase Support**: Separate printing for group phase, finals, and knockout rounds
- **License-driven Features**: Extended print functions with premium license

#### 🌐 **Tournament Hub Integration**
- **Real-time Synchronization**: WebSocket-based live tournament data sync
- **Custom Tournament-ID**: Set custom IDs or generate them automatically
  - Optional ID input field with validation
  - 🔄 Generate button for quick ID creation
  - Persistent storage and QR code integration
- **Multi-Device Access**: Access tournaments from different devices
- **Live Match Updates**: Automatic real-time match result updates
  - Match-Start Events with live indicators (🔴 LIVE)
  - Leg-Completed Events with detailed statistics
  - Match-Progress Events for ongoing updates
  - Leg counter display (e.g., "Leg 2/5")
- **Join URL System**: Easy tournament access sharing
- **Automatic WebSocket Reconnect**: Robust connection recovery
  - Continuous reconnect attempts until server is back
  - Automatic tournament re-registration
  - Full data synchronization after reconnect
- **Enhanced Status Display**: Detailed connection indicators
  - Visual indicators (🔴 Red / 🟢 Green / 🟡 Yellow)
  - Three-tier status: Connection / Registration / Sync
  - Debug console for connection diagnostics

#### 🔑 **License System**
- **Core Features**: All basic functions are free
- **Premium Features**: Extended features through licensing
  - 📈 **Extended Statistics**: Detailed player analyses
  - 🌐 **Tournament Hub Premium**: Enhanced hub features
  - ⚡ **PowerScoring**: Intelligent player seeding system
  - 🖨️ **Enhanced Printing**: Professional print layouts
  - 📊 **Tournament Overview Premium**: Extended presentation modes
- **License Management**: Easy activation, status display, and management
- **Offline Support**: License validation without internet connection

#### 🎨 **Theme System**
- **Light/Dark Mode**: Complete theme support with automatic persistence
- **One-Click Toggle**: Switch between light and dark modes instantly
- **Consistent Design**: Uniform theme application across all UI elements
- **Theme Persistence**: Settings saved and restored on app restart
- **PowerScoring Dark Mode**: Full dark mode support for all PowerScoring dialogs

#### 🌍 **Extended Localization**
- **Modular Architecture**: Language providers for easy extension
- **Comprehensive Coverage**: 500+ translated interface elements
- **Context-Aware**: Sport-specific and tournament-specific translations
- **Real-time Switch**: Language change without app restart
- **PowerScoring Support**: Complete translations for all PowerScoring features

##### Supported Languages
- 🇩🇪 **German** (Complete translation with 500+ keys)
- 🇬🇧 **English** (Complete translation with tournament terminology)

#### ⚡ **Match Management**
- **Automatic Match Generation**: Round-robin matches created automatically
- **Flexible Game Rules**: 301, 401, or 501 points with single/double out
- **Set System**: Configurable sets and legs with detailed validation
- **Round-specific Rules**: Different rules for quarterfinals, semifinals, finals
- **Result Validation**: Advanced match result validation with conflict detection
- **WebSocket Integration**: Direct match updates via Tournament Hub

#### 🎭 **User Experience**
- **Professional Start**: Animated splash screen with progress indicators
- **Modern UI**: Intuitive WPF interface with professional design
- **Tournament Overview**: Full-screen presentation mode with auto-cycling
- **Auto-Update System**: Automatic update check with GitHub integration
- **Bug Report System**: Integrated error reporting with system information
- **Loading Animations**: Professional loading animations and progress displays

### 💾 Data Management
- **JSON Storage**: Human-readable tournament data in JSON format
- **Version Control**: Data structure versioning for compatibility
- **Backup System**: Automatic backup creation on save
- **Export/Import**: Complete tournament data portability
- **Auto-Save**: Intelligent automatic saving on changes

### 🔄 Update System
- **GitHub Integration**: Automatic check of GitHub releases
- **Background Check**: Unobtrusive update detection on startup
- **Professional UI**: Integrated update dialog with changelog
- **One-Click Updates**: Automated download and installation
- **Release Notes**: Detailed changelog display with markdown support

### 🐛 Error Handling & Support
- **Integrated Bug Reporting**: Detailed bug report forms
- **System Information**: Automatic inclusion of system information
- **Debug Console**: Extended debug tools for development and support
- **Error Recovery**: Robust error handling and recovery mechanisms

---

### 🔧 System Requirements

- **Operating System**: Windows 10 or higher
- **.NET Runtime**: .NET 9.0 Runtime
- **Architecture**: x64 or x86
- **Memory**: Minimum 512 MB RAM
- **Storage**: 50 MB free space
- **Printer**: Optional - for print functionality
- **Internet**: Optional - for Hub integration and updates

---

### 📦 Installation

#### Automatic Installation (Recommended)
1. Download the latest `Setup-DartTournamentPlaner-v0.1.13.exe` from [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
2. Run the installer (administrator rights may be required)
3. Follow the installation wizard
4. Start the application via desktop shortcut or start menu

#### Manual Installation
1. Download the latest ZIP archive from [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
2. Extract to your desired folder
3. Run `DartTournamentPlaner.exe`

> **Note**: The application automatically checks for updates on startup.

---

### 🚀 Quick Start

#### Creating Your First Tournament
1. **Select Tournament Class**: Choose from Platinum, Gold, Silver, or Bronze
2. **Add Groups**: Click **"Add Group"** to create tournament groups
3. **Add Players**: Add players to each group (minimum 2 per group)
4. **Configure Rules**: Use **"Configure Rules"** for game parameters
5. **Generate Matches**: Click **"Generate Matches"** for automatic round-robin creation
6. **Enter Results**: Click on matches to enter results
7. **Progress Phases**: Use **"Start Next Phase"** when group phase is complete

#### Using PowerScoring for Player Seeding
1. **Open PowerScoring**: Go to **PowerScoring** menu → **Open PowerScoring**
2. **Select Scoring Rule**: Choose from 1x3, 8x3, 10x3, or 15x3 throws
3. **Add Players**: Enter all player names
4. **Start Scoring**: Begin the scoring session
5. **Enter Scores**: Input scores manually or use QR codes for remote entry
6. **Complete Scoring**: Finish scoring and view rankings
7. **Configure Distribution**: 
   - Select tournament classes (Platinum, Gold, Silver, Bronze, Iron)
   - Configure groups per class (1-4)
   - Set players per group (2-6)
   - Choose distribution mode (Balanced, Snake Draft, Top-Heavy, Random)
8. **Create Tournament**: Generate tournament directly from distribution

#### Using Tournament Hub
1. **Register**: Go to **Tournament Hub** → **Register with Hub**
2. **Custom ID** (Optional): Set a custom Tournament-ID or let it auto-generate
3. **Share URL**: The join URL is automatically copied to clipboard
4. **Live Updates**: Match results are synchronized automatically
5. **Multi-Device**: Access from different devices via join URL

---

### 📋 Advanced Features

#### ⚡ **PowerScoring Distribution Modes**
- **Balanced (⚖️)**: Players evenly distributed across groups based on ranking
  - Best for competitive balance
  - Ensures each group has similar skill levels
- **Snake Draft (🐍)**: 1-2-3-4-4-3-2-1 pattern
  - Zigzag distribution for balanced groups
  - Ideal for league-style tournaments
- **Top-Heavy (🔝)**: Strongest players grouped first
  - Group 1 gets strongest players, then Group 2, etc.
  - Good for tiered tournament structures
- **Random (🎲)**: Random distribution
  - Players randomly distributed to groups
  - Useful for casual tournaments

#### 🖨️ **Professional Print System**
- **Tournament Statistics**: Complete tournament reports with all phases
- **Group Reports**: Individual group rankings and match results
- **Finals Documentation**: Finals round participants and results
- **Knockout Brackets**: Winner and loser bracket visualization
- **Participant Lists**: Comprehensive player listings
- **Custom Titles**: User-defined titles and subtitles for reports

#### 🌐 **Tournament Hub System**
- **WebSocket Connection**: Real-time communication with Tournament Hub
- **Automatic Synchronization**: Live updates of match results
- **QR Code Generation**: Easy access via QR codes
- **Multi-User Support**: Multiple users can participate simultaneously
- **Robust Connection**: Automatic reconnection on connection errors
- **Custom Tournament-IDs**: Persistent IDs for consistent QR codes

---

### 🛠️ Development

#### Technical Stack
- **Framework**: .NET 9.0 with C# 13.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM pattern with service-oriented design
- **Dependencies**: 
  - `Newtonsoft.Json` (13.0.3) for data serialization
  - `Microsoft.VisualBasic` (10.3.0) for input dialogs
  - `QRCoder` (1.6.0) for QR code generation
  - `System.Management` (9.0.0) for system information


### 📈 Version History

See [CHANGELOG.md](CHANGELOG.md) for detailed version history.

**Current Version: v0.1.13**
- ⚡ PowerScoring System with intelligent player seeding
- 🎨 Complete dark mode support for all components
- 🆔 Custom Tournament-ID with persistence
- 🔄 Enhanced WebSocket reconnection
- 📊 Live match updates with detailed statistics
- 🌍 500+ translation keys (DE/EN)

---

### 🤝 Contributing

We welcome contributions! Here's how you can help:

1. **Fork** the repository
2. **Clone** your fork locally
3. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
4. **Make** your changes
5. **Test** thoroughly
6. **Commit** your changes (`git commit -m 'Add AmazingFeature'`)
7. **Push** to your branch (`git push origin feature/AmazingFeature`)
8. **Create** a Pull Request

#### Areas for Contribution
- **New Languages**: Support for additional languages via ILanguageProvider
- **Print Features**: Extended print layouts and options
- **Tournament Formats**: Additional tournament structures
- **UI Improvements**: Enhanced usability features
- **Bug Fixes**: Error fixes and stability improvements
- **Documentation**: Help content and user guides
- **PowerScoring**: Distribution algorithms and scoring rules

---

### 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

### 💝 Support the Project

#### Financial Support
- **In-App Donations**: Use the integrated donation dialog (**Help** → **Donate**)
- **One-Time Donations**: [PayPal](https://www.paypal.com/paypalme/I3ull3t)

#### Non-Financial Support
- ⭐ **Star** the repository on GitHub
- 🐛 **Report** bugs and suggest improvements
- 📢 **Share** with your dart community
- 📝 **Write** reviews and tutorials
- 🌍 **Help** with translations

---

### 📞 Contact & Links

- **GitHub Repository**: [Peschi90/Dart-Turnament-Planer](https://github.com/Peschi90/Dart-Turnament-Planer)
- **Releases**: [Latest Downloads](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
- **Issues**: [GitHub Issues](https://github.com/Peschi90/Dart-Turnament-Planer/issues)
- **Developer**: [@Peschi90](https://github.com/Peschi90)
- **Email**: m@peschi.info

---

*Developed with ❤️ for the Dart Community*

**"Perfect tournaments start with perfect planning - analyze them intelligently!"** 🎯📊

---

<a name="deutsch"></a>
## 🇩🇪 Deutsche Version

Eine moderne WPF-Anwendung für die Verwaltung von Dart-Turnieren mit professionellen Features für Turnierorganisatoren.

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![C#](https://img.shields.io/badge/C%23-13.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)
![Version](https://img.shields.io/badge/Version-0.1.13-brightgreen)

### 🏆 Kern-Features

#### 🎮 Turnier-Management
- **Multiple Turnierklassen**: Verwaltung von bis zu 4 verschiedenen Klassen (Platin, Gold, Silber, Bronze)
- **Flexible Gruppenphase**: Round-Robin-System mit unbegrenzten Gruppen
- **Knockout-System**: Einzel- oder Doppel-Eliminierung mit Winner/Loser Bracket
- **Finalrunden**: Round-Robin-Finals für qualifizierte Spieler
- **Auto-Save-System**: Konfigurierbare automatische Speicherung
- **Professionelle Workflows**: Vereinfachte Turniererstellung und -verwaltung
- **Bye-System**: Automatische Bye-Zuweisung bei ungerader Spieleranzahl

#### ⚡ **PowerScoring System** (NEU!)
Intelligente Spieler-Einteilung basierend auf Leistungsdaten - perfekt für faire Turnierverteilung!

- **Scoring-Sitzungen**: Erstellen Sie Scoring-Sitzungen mit anpassbaren Regeln
  - 1x3 Würfe: Schnelle Einschätzung (3 Darts)
  - 8x3 Würfe: Standard-Bewertung (24 Darts)
  - 10x3 Würfe: Ausführliche Bewertung (30 Darts)
  - 15x3 Würfe: Sehr detaillierte Bewertung (45 Darts)
- **Remote Scoring**: QR-Code-Integration für Remote-Score-Eingabe via Tournament Hub
- **Spieler-Ranking**: Automatisches Ranking basierend auf Gesamt- und Durchschnitts-Scores
- **Intelligente Gruppenverteilung**: Smart Player-Verteilung über Turnierklassen
  - Unterstützung für 5 Klassen (Platin, Gold, Silber, Bronze, Eisen)
  - Konfigurierbare Gruppen pro Klasse (1-4 Gruppen)
  - Konfigurierbare Spieler pro Gruppe (2-6 Spieler)
- **Verteilungsmodi**: 
  - ⚖️ **Balanced**: Gleichmäßige Verteilung nach Ranking
  - 🐍 **Snake Draft**: 1-2-3-4-4-3-2-1 Zickzack-Muster für ausgeglichene Gruppen
  - 🔝 **Top-Heavy**: Stärkste Spieler zuerst gruppiert
  - 🎲 **Random**: Zufällige Verteilung
- **Erweiterte Einstellungen**: 
  - Klassen-spezifische Gruppen- und Spieleranzahl
  - Klassen überspringen
  - Individuelle Spieler-Limits pro Klasse
  - Verteilungs-Vorschau vor Bestätigung
- **Turnier-Erstellung**: 
  - Neues Turnier direkt aus PowerScoring-Verteilung erstellen
  - Automatische Turnierdaten-Migration
  - Backup des bestehenden Turniers vor Erstellung
  - Nahtloser UI-Übergang zur Turnieransicht
- **Sitzungs-Verwaltung**: 
  - Sitzungs-Persistenz (Auto-Save/Auto-Load)
  - Turnier-ID-Integration für Hub-Synchronisation
  - QR-Code-Generierung für alle Spieler
  - Live-Scoring-Updates via WebSocket

#### 📊 **Erweiterte Spieler-Statistiken**
- **Match-Effizienz**: Anzeige der schnellsten Spieldauer (MM:SS Format)
- **Wurf-Effizienz**: Tracking der wenigsten Würfe pro Match
- **Detaillierte Performance-Daten**: High Finish Details mit Darts-Aufschlüsselung
- **180er-Tracking**: Vollständige Maximum-Score-Verfolgung
- **Checkout-Statistiken**: Anzahl und Details aller erfolgreichen Checkouts
- **Leg-Averages**: Verfolgung individueller Leg-Performance
- **Score-Analyse**: Tracking von 26er-Scores und Performance-Trends
- **Dedizierter Statistiken-Tab**: Separate Anzeige für jede Turnierklasse

#### 🖨️ **Professionelles Druck-System**
- **Turnier-Statistiken-Druck**: Umfassende Turnierberichte mit detaillierten Statistiken
- **Druck-Dialog**: Benutzerfreundliche Oberfläche zur Auswahl der Druckinhalte
- **Druckvorschau**: Echtzeit-Vorschau von Dokumenten vor dem Druck
- **Flexible Optionen**: Druck einzelner Gruppen, kompletter Turniere oder spezifischer Phasen
- **Professionelles Layout**: Formatierte Berichte mit Tabellen, Ranglisten und Match-Ergebnissen
- **Multi-Phasen-Support**: Separate Druckmöglichkeiten für Gruppenphase, Finals und Knockout
- **Lizenz-gesteuerte Features**: Erweiterte Druckfunktionen mit Premium-Lizenz

#### 🌐 **Tournament Hub Integration**
- **Echtzeit-Synchronisation**: WebSocket-basierte Live-Synchronisation von Turnierdaten
- **Custom Tournament-ID**: Eigene IDs setzen oder automatisch generieren
  - Optionales ID-Eingabefeld mit Validierung
  - 🔄 Generieren-Button für schnelle ID-Erstellung
  - Persistente Speicherung und QR-Code-Integration
- **Multi-Device-Zugang**: Zugriff auf Turniere von verschiedenen Geräten
- **Live Match-Updates**: Automatische Echtzeit-Match-Ergebnis-Updates
  - Match-Start-Events mit Live-Indikatoren (🔴 LIVE)
  - Leg-Abgeschlossen-Events mit detaillierten Statistiken
  - Match-Fortschritt-Events für laufende Updates
  - Leg-Counter-Anzeige (z.B. "Leg 2/5")
- **Join-URL-System**: Einfaches Teilen von Turnier-Zugängen
- **Automatische WebSocket-Wiederverbindung**: Robuste Verbindungswiederherstellung
  - Kontinuierliche Wiederverbindungsversuche bis Server zurück
  - Automatische Turnier-Neuregistrierung
  - Vollständige Datensynchronisation nach Wiederverbindung
- **Erweiterte Status-Anzeige**: Detaillierte Verbindungsindikatoren
  - Visuelle Indikatoren (🔴 Rot / 🟢 Grün / 🟡 Gelb)
  - Drei-Stufen-Status: Verbindung / Registrierung / Sync
  - Debug-Konsole für Verbindungsdiagnose

#### 🔑 **Lizenz-System**
- **Core Features**: Alle Basisfunktionen sind kostenlos
- **Premium Features**: Erweiterte Funktionen durch Lizenzierung
  - 📈 **Erweiterte Statistiken**: Detaillierte Spieler-Analysen
  - 🌐 **Tournament Hub Premium**: Erweiterte Hub-Features
  - ⚡ **PowerScoring**: Intelligentes Spieler-Seeding-System
  - 🖨️ **Enhanced Printing**: Professionelle Druck-Layouts
  - 📊 **Tournament Overview Premium**: Erweiterte Präsentationsmodi
- **Lizenz-Verwaltung**: Einfache Aktivierung, Status-Anzeige und Verwaltung
- **Offline-Unterstützung**: Lizenzvalidierung ohne Internetverbindung

#### 🎨 **Theme-System**
- **Light/Dark Mode**: Vollständige Theme-Unterstützung mit automatischer Persistenz
- **Ein-Klick-Toggle**: Sofortiger Wechsel zwischen hellen und dunklen Modi
- **Konsistentes Design**: Einheitliche Theme-Anwendung über alle UI-Elemente
- **Theme-Persistenz**: Einstellungen gespeichert und beim App-Start wiederhergestellt
- **PowerScoring Dark Mode**: Vollständige Dark-Mode-Unterstützung für alle PowerScoring-Dialoge

#### 🌍 **Erweiterte Lokalisierung**
- **Modulare Architektur**: Sprachprovider für einfache Erweiterung
- **Umfassende Abdeckung**: 500+ übersetzte Interface-Elemente
- **Kontextbewusst**: Sport-spezifische und turnier-spezifische Übersetzungen
- **Echtzeit-Wechsel**: Sprachwechsel ohne App-Neustart
- **PowerScoring-Unterstützung**: Vollständige Übersetzungen für alle PowerScoring-Features

##### Unterstützte Sprachen
- 🇩🇪 **Deutsch** (Vollständige Übersetzung mit 500+ Schlüsseln)
- 🇬🇧 **English** (Vollständige Übersetzung mit Turnier-Terminologie)

#### ⚡ **Match-Verwaltung**
- **Automatische Match-Generierung**: Round-Robin-Matches werden automatisch erstellt
- **Flexible Spielregeln**: 301, 401 oder 501 Punkte mit Single/Double Out
- **Set-System**: Konfigurierbare Sets und Legs mit detaillierter Validierung
- **Runden-spezifische Regeln**: Verschiedene Regeln für Viertelfinale, Halbfinale, Finale
- **Ergebnis-Validierung**: Erweiterte Match-Ergebnis-Validierung mit Konflikt-Erkennung
- **WebSocket-Integration**: Direkte Match-Updates via Tournament Hub

#### 🎭 **Benutzerfreundlichkeit**
- **Professioneller Start**: Animierter Splash Screen mit Fortschrittsanzeigen
- **Moderne UI**: Intuitive WPF-Oberfläche mit professionellem Design
- **Turnierübersicht**: Vollbild-Präsentationsmodus mit Auto-Cycling
- **Auto-Update-System**: Automatische Update-Prüfung mit GitHub-Integration
- **Bug-Report-System**: Integrierte Fehlerberichterstattung mit Systeminformationen
- **Lade-Animationen**: Professionelle Ladeanimationen und Fortschrittsanzeigen

### 💾 Daten-Management
- **JSON-Storage**: Menschenlesbare Turnierdaten im JSON-Format
- **Versions-Kontrolle**: Datenstruktur-Versionierung für Kompatibilität
- **Backup-System**: Automatische Backup-Erstellung beim Speichern
- **Export/Import**: Vollständige Turnierdaten-Portabilität
- **Auto-Save**: Intelligente automatische Speicherung bei Änderungen

### 🔄 Update-System
- **GitHub-Integration**: Automatische Prüfung von GitHub Releases
- **Background-Prüfung**: Unaufdringliche Update-Erkennung beim Start
- **Professionelle UI**: Integrierter Update-Dialog mit Changelog
- **Ein-Klick-Updates**: Automatisierter Download und Installation
- **Release Notes**: Detaillierte Changelog-Anzeige mit Markdown-Support

### 🐛 Fehlerbehandlung & Support
- **Integrierte Bug-Berichterstattung**: Detaillierte Bug-Report-Formulare
- **System-Informationen**: Automatische Einbindung von System-Informationen
- **Debug-Konsole**: Erweiterte Debug-Tools für Entwicklung und Support
- **Error-Recovery**: Robuste Fehlerbehandlung und Wiederherstellungsmechanismen

---

### 🔧 Systemanforderungen

- **Betriebssystem**: Windows 10 oder höher
- **.NET Runtime**: .NET 9.0 Runtime
- **Architektur**: x64 oder x86
- **Arbeitsspeicher**: Mindestens 512 MB RAM
- **Speicherplatz**: 50 MB freier Speicherplatz
- **Drucker**: Optional - für Druckfunktionalität
- **Internet**: Optional - für Hub-Integration und Updates

---

### 📦 Installation

#### Automatische Installation (Empfohlen)
1. Laden Sie die neueste `Setup-DartTournamentPlaner-v0.1.13.exe` von [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases) herunter
2. Führen Sie das Installationsprogramm aus (Administrator-Rechte können erforderlich sein)
3. Folgen Sie dem Installations-Assistenten
4. Starten Sie die Anwendung über die Desktop-Verknüpfung oder das Startmenü

#### Manuelle Installation
1. Laden Sie das neueste ZIP-Archiv von [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases) herunter
2. Extrahieren Sie es in Ihren gewünschten Ordner
3. Führen Sie `DartTournamentPlaner.exe` aus

> **Hinweis**: Die Anwendung prüft beim Start automatisch auf Updates.

---

### 🚀 Schnellstart

#### Ihr erstes Turnier erstellen
1. **Turnierklasse wählen**: Wählen Sie aus Platin, Gold, Silber oder Bronze
2. **Gruppen hinzufügen**: Klicken Sie auf **"Gruppe hinzufügen"**
3. **Spieler hinzufügen**: Fügen Sie Spieler zu jeder Gruppe hinzu (mindestens 2 pro Gruppe)
4. **Regeln konfigurieren**: Verwenden Sie **"Regeln konfigurieren"**
5. **Matches generieren**: Klicken Sie auf **"Matches generieren"**
6. **Ergebnisse eingeben**: Klicken Sie auf Matches um Ergebnisse einzugeben
7. **Phasen fortsetzen**: Verwenden Sie **"Nächste Phase starten"**

#### PowerScoring für Spieler-Seeding verwenden
1. **PowerScoring öffnen**: Gehen Sie zu **PowerScoring** → **PowerScoring öffnen**
2. **Scoring-Regel wählen**: Wählen Sie aus 1x3, 8x3, 10x3 oder 15x3 Würfen
3. **Spieler hinzufügen**: Geben Sie alle Spielernamen ein
4. **Scoring starten**: Beginnen Sie die Scoring-Sitzung
5. **Scores eingeben**: Geben Sie Scores manuell ein oder verwenden Sie QR-Codes
6. **Scoring abschließen**: Beenden Sie das Scoring und sehen Sie die Rankings
7. **Verteilung konfigurieren**: 
   - Wählen Sie Turnierklassen (Platin, Gold, Silber, Bronze, Eisen)
   - Konfigurieren Sie Gruppen pro Klasse (1-4)
   - Setzen Sie Spieler pro Gruppe (2-6)
   - Wählen Sie Verteilungsmodus (Balanced, Snake Draft, Top-Heavy, Random)
8. **Turnier erstellen**: Generieren Sie Turnier direkt aus der Verteilung

#### Tournament Hub verwenden
1. **Registrieren**: Gehen Sie zu **Tournament Hub** → **Bei Hub registrieren**
2. **Custom ID** (Optional): Setzen Sie eine eigene Turnier-ID oder lassen Sie sie automatisch generieren
3. **URL teilen**: Die Join-URL wird automatisch in die Zwischenablage kopiert
4. **Live-Updates**: Match-Ergebnisse werden automatisch synchronisiert
5. **Multi-Device**: Zugriff von verschiedenen Geräten über Join-URL

---

### 📋 Erweiterte Features

#### ⚡ **PowerScoring Verteilungsmodi**
- **Balanced (⚖️)**: Spieler gleichmäßig auf Gruppen verteilt basierend auf Ranking
  - Am besten für Wettbewerbsbalance
  - Stellt sicher, dass jede Gruppe ähnliche Skill-Level hat
- **Snake Draft (🐍)**: 1-2-3-4-4-3-2-1 Muster
  - Zickzack-Verteilung für ausgeglichene Gruppen
  - Ideal für Liga-ähnliche Turniere
- **Top-Heavy (🔝)**: Stärkste Spieler zuerst gruppiert
  - Gruppe 1 erhält stärkste Spieler, dann Gruppe 2, etc.
  - Gut für gestaffelte Turnierstrukturen
- **Random (🎲)**: Zufällige Verteilung
  - Spieler zufällig auf Gruppen verteilt
  - Nützlich für Casual-Turniere

#### 🖨️ **Professionelles Druck-System**
- **Turnier-Statistiken**: Vollständige Turnierberichte mit allen Phasen
- **Gruppen-Berichte**: Individuelle Gruppen-Ranglisten und Match-Ergebnisse
- **Finals-Dokumentation**: Finals-Runde Teilnehmer und Ergebnisse
- **Knockout-Brackets**: Winner- und Loser-Bracket-Visualisierung
- **Teilnehmer-Listen**: Umfassende Spieler-Auflistungen
- **Anpassbare Titel**: Benutzerdefinierte Titel und Untertitel für Berichte

#### 🌐 **Tournament Hub System**
- **WebSocket-Verbindung**: Echtzeit-Kommunikation mit Tournament Hub
- **Automatische Synchronisation**: Live-Updates von Match-Ergebnissen
- **QR-Code-Generation**: Einfacher Zugang über QR-Codes
- **Multi-User-Support**: Mehrere Benutzer können gleichzeitig teilnehmen
- **Robuste Verbindung**: Automatische Wiederverbindung bei Verbindungsfehlern
- **Custom Tournament-IDs**: Persistente IDs für konsistente QR-Codes

---

### 🛠️ Entwicklung

#### Technischer Stack
- **Framework**: .NET 9.0 mit C# 13.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Architektur**: MVVM-Pattern mit Service-orientiertem Design
- **Abhängigkeiten**: 
  - `Newtonsoft.Json` (13.0.3) für Datenserialisierung
  - `Microsoft.VisualBasic` (10.3.0) für Input-Dialoge
  - `QRCoder` (1.6.0) für QR-Code-Generierung
  - `System.Management` (9.0.0) für Systeminformationen

### 📈 Versionshistorie

Siehe [CHANGELOG.md](CHANGELOG.md) für detaillierte Versionshistorie.

**Aktuelle Version: v0.1.13**
- ⚡ PowerScoring-System mit intelligentem Spieler-Seeding
- 🎨 Vollständige Dark-Mode-Unterstützung für alle Komponenten
- 🆔 Custom Tournament-ID mit Persistenz
- 🔄 Verbesserte WebSocket-Wiederverbindung
- 📊 Live-Match-Updates mit detaillierten Statistiken
- 🌍 500+ Übersetzungsschlüssel (DE/EN)

---

### 🤝 Beitragen

Wir begrüßen Beiträge! So können Sie helfen:

1. **Fork** das Repository
2. **Klonen** Sie Ihren Fork lokal
3. **Erstellen** Sie einen Feature-Branch (`git checkout -b feature/AmazingFeature`)
4. **Machen** Sie Ihre Änderungen
5. **Testen** Sie gründlich
6. **Committen** Sie Ihre Änderungen (`git commit -m 'Add AmazingFeature'`)
7. **Pushen** Sie zu Ihrem Branch (`git push origin feature/AmazingFeature`)
8. **Erstellen** Sie einen Pull Request

#### Bereiche für Beiträge
- **Neue Sprachen**: Support für zusätzliche Sprachen über ILanguageProvider
- **Druck-Features**: Erweiterte Druck-Layouts und Optionen
- **Turnier-Formate**: Zusätzliche Turnier-Strukturen
- **UI-Verbesserungen**: Erweiterte Benutzerfreundlichkeits-Features
- **Bug-Fixes**: Fehlerbehebung und Stabilitätsverbesserungen
- **Dokumentation**: Hilfe-Inhalte und Benutzerhandbücher
- **PowerScoring**: Verteilungs-Algorithmen und Scoring-Regeln

---

### 📄 Lizenz

Dieses Projekt ist unter der **MIT-Lizenz** lizenziert - siehe die [LICENSE](LICENSE) Datei für Details.

---

### 💝 Projekt unterstützen

#### Finanzielle Unterstützung
- **In-App-Spenden**: Verwenden Sie den integrierten Spenden-Dialog (**Hilfe** → **Spenden**)
- **Einmalige Spenden**: [PayPal](https://www.paypal.com/paypalme/I3ull3t)

#### Nicht-finanzielle Unterstützung
- ⭐ **Bewerten** Sie das Repository auf GitHub
- 🐛 **Melden** Sie Bugs und schlagen Sie Verbesserungen vor
- 📢 **Teilen** Sie mit Ihrer Dart-Community
- 📝 **Schreiben** Sie Bewertungen und Tutorials
- 🌍 **Helfen** Sie bei Übersetzungen

---

### 📞 Kontakt & Links

- **GitHub Repository**: [Peschi90/Dart-Turnament-Planer](https://github.com/Peschi90/Dart-Turnament-Planer)
- **Releases**: [Neueste Downloads](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
- **Issues**: [GitHub Issues](https://github.com/Peschi90/Dart-Turnament-Planer/issues)
- **Entwickler**: [@Peschi90](https://github.com/Peschi90)
- **E-Mail**: m@peschi.info

---

*Entwickelt mit ❤️ für die Dart Community*

**"Perfekte Turniere beginnen mit perfekter Planung - analysiere sie intelligent!"** 🎯📊

