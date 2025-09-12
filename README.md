# 🎯 Dart Tournament Planner

Eine moderne WPF-Anwendung für die Verwaltung von Dart-Turnieren mit professionellen Features für Turnierorganisatoren.

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![C#](https://img.shields.io/badge/C%23-13.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)
![Version](https://img.shields.io/badge/Version-0.1.8-brightgreen)

## 🏆 Features

### 🎮 Turnier-Management
- **Multiple Turnierklassen**: Verwaltung von bis zu 4 verschiedenen Klassen (Platinum, Gold, Silver, Bronze)
- **Flexible Gruppenphase**: Round-Robin-System mit unbegrenzten Gruppen
- **Knockout-System**: Einzel- oder Doppel-Eliminierung mit Winner/Loser Bracket
- **Finalrunden**: Round-Robin-Finals für qualifizierte Spieler
- **Auto-Save-System**: Konfigurierbare automatische Speicherung mit anpassbaren Intervallen
- **Professionelle Workflows**: Vereinfachte Turniererstellung und -verwaltung
- **Bye-System**: Automatische Bye-Zuweisung bei ungerader Spieleranzahl

### 📊 **NEU: Erweiterte Spieler-Statistiken**
- **Match-Effizienz**: Anzeige der schnellsten Spieldauer (MM:SS Format)
- **Wurf-Effizienz**: Tracking der wenigsten Würfe pro Match
- **Detaillierte Performance-Daten**: High Finish Details mit Darts-Aufschlüsselung
- **180er-Tracking**: Verfolgung aller Maximum-Scores
- **Checkout-Statistiken**: Anzahl und Details aller erfolgreichen Checkouts
- **Leg-Averages**: Verfolgung individueller Leg-Performance
- **Score-Analyse**: Tracking von 26er-Scores und schlechten Würfen
- **Dedizierter Statistiken-Tab**: Separate Anzeige für jede Turnierklasse

### 🖨️ **Professionelles Druck-System**
- **Turnier-Statistiken-Druck**: Umfassende Turnierberichte mit detaillierten Statistiken
- **Druck-Dialog**: Benutzerfreundliche Oberfläche zur Auswahl der Druckinhalte
- **Druckvorschau**: Echtzeit-Vorschau von Dokumenten vor dem Druck
- **Flexible Optionen**: Druck einzelner Gruppen, kompletter Turniere oder spezifischer Phasen
- **Professionelles Layout**: Formatierte Berichte mit Tabellen, Ranglisten und Match-Ergebnissen
- **Multi-Phasen-Support**: Separate Druckmöglichkeiten für Gruppenphase, Finals und Knockout-Runden
- **Lizenz-gesteuerte Features**: Erweiterte Druckfunktionen mit Premium-Lizenz

### 🌐 **Tournament Hub Integration**
- **Echtzeit-Synchronisation**: WebSocket-basierte Live-Synchronisation von Turnierdaten
- **Multi-Device-Zugang**: Zugriff auf Turniere von verschiedenen Geräten
- **Live Match-Updates**: Automatische Aktualisierung von Match-Ergebnissen in Echtzeit
- **Join-URL-System**: Einfaches Teilen von Turnier-Zugängen
- **Hub-Status-Anzeige**: Visueller Status der Hub-Verbindung
- **Automatische Wiederverbindung**: Robuste WebSocket-Verbindung mit Auto-Reconnect
- **Debug-Konsole**: Erweiterte Debugging-Tools für Hub-Verbindungen

### 🔑 **Lizenz-System**
- **Core Features**: Alle grundlegenden Funktionen sind kostenlos verfügbar
- **Premium Features**: Erweiterte Funktionen durch Lizenzierung
  - 📈 **Erweiterte Statistiken**: Detaillierte Spieler-Analysen und Performance-Tracking
  - 🌐 **Tournament Hub**: Premium Hub-Verbindungsfeatures
  - 🖨️ **Enhanced Printing**: Professionelle Drucklayouts und erweiterte Optionen
  - 📊 **Tournament Overview**: Premium Präsentationsmodus
- **Lizenz-Verwaltung**: Einfache Aktivierung, Status-Anzeige und Verwaltung
- **Offline-Unterstützung**: Lizenzvalidierung auch ohne Internetverbindung
- **Flexible Aktivierung**: Support für verschiedene Lizenztypen und Aktivierungsmodelle

### 🎨 **Theme-System**
- **Light/Dark Mode**: Vollständige Theme-Unterstützung
- **Automatischer Theme-Wechsel**: Toggle zwischen hellen und dunklen Modi
- **Konsistentes Design**: Einheitliche Theme-Anwendung über alle UI-Elemente
- **Persistente Einstellungen**: Theme-Auswahl wird gespeichert und beim Start wiederhergestellt

### 🌍 **Erweiterte Lokalisierung**
- **Modulare Architektur**: Sprachprovider für einfache Erweiterung
- **Umfassende Abdeckung**: 400+ übersetzte Interface-Elemente
- **Kontextbewusst**: Sport-spezifische und turnier-spezifische Übersetzungen
- **Dynamischer Inhalt**: Versions-bewusste und kontext-sensitive Übersetzungen
- **Einfache Erweiterung**: Neue Sprachen über ILanguageProvider Interface hinzufügen
- **Echtzeit-Wechsel**: Sprachwechsel ohne Anwendungsneustart

#### Unterstützte Sprachen
- 🇩🇪 **Deutsch** (Vollständige Übersetzung mit 400+ Übersetzungen)
- 🇬🇧 **English** (Complete translation with tournament terminology)

### ⚡ **Match-Verwaltung**
- **Automatische Match-Generierung**: Round-Robin-Matches werden automatisch erstellt
- **Flexible Spielregeln**: 301, 401 oder 501 Punkte mit Single/Double Out
- **Set-System**: Konfigurierbare Sets und Legs mit detaillierter Validierung
- **Runden-spezifische Regeln**: Verschiedene Regeln für Viertelfinale, Halbfinale, Finale, etc.
- **Ergebnis-Validierung**: Erweiterte Match-Ergebnis-Validierung mit Konflikt-Erkennung
- **WebSocket-Integration**: Direkte Match-Updates über Tournament Hub

### 🎭 **Benutzerfreundlichkeit**
- **Professioneller Start**: Animierter Splash Screen mit Fortschrittsanzeigen
- **Moderne UI**: Intuitive WPF-Oberfläche mit professionellem Design
- **Turnierübersicht**: Vollbild-Präsentationsmodus mit Auto-Cycling
- **Auto-Update-System**: Automatische Update-Prüfung mit GitHub-Integration
- **Bug-Report-System**: Integrierte Fehlerberichterstattung mit Systeminformationen
- **Loading-Animationen**: Professionelle Ladeanimationen und Fortschrittsanzeigen

### 💾 **Daten-Management**
- **JSON-Storage**: Menschenlesbare Turnierdaten im JSON-Format
- **Versions-Kontrolle**: Datenstruktur-Versionierung für Kompatibilität
- **Backup-System**: Automatische Backup-Erstellung beim Speichern
- **Export/Import**: Vollständige Turnierdaten-Portabilität
- **Auto-Save**: Intelligente automatische Speicherung bei Änderungen

### 🔄 **Update-System**
- **GitHub-Integration**: Automatische Prüfung von GitHub Releases
- **Background-Prüfung**: Unaufdringliche Update-Erkennung beim Start
- **Professionelle UI**: Integrierter Update-Dialog mit Changelog
- **Ein-Klick-Updates**: Automatisierter Download und Installation
- **Release Notes**: Detaillierte Changelog-Anzeige mit Markdown-Support
- **Versions-Management**: Intelligenter Versionsvergleich und Rollback-Schutz

### 🐛 **Fehlerbehandlung & Support**
- **Integrierte Bug-Berichterstattung**: Detaillierte Bug-Report-Formulare
- **System-Informationen**: Automatische Einbindung von System-Informationen
- **Debug-Konsole**: Erweiterte Debug-Tools für Entwicklung und Support
- **Error-Recovery**: Robuste Fehlerbehandlung und Wiederherstellungsmechanismen

## 🔧 Systemanforderungen

- **Betriebssystem**: Windows 10 oder höher
- **.NET Runtime**: .NET 9.0 Runtime
- **Architektur**: x64 oder x86
- **Arbeitsspeicher**: Mindestens 512 MB RAM
- **Speicherplatz**: 50 MB freier Speicherplatz
- **Drucker**: Optional - für Druckfunktionalität
- **Internet**: Optional - für Hub-Integration und Updates

## 📦 Installation

### Automatische Installation (Empfohlen)
1. Laden Sie die neueste `Setup-DartTournamentPlaner-v0.1.8.exe` von [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases) herunter
2. Führen Sie das Installationsprogramm aus (Administrator-Rechte können erforderlich sein)
3. Folgen Sie dem Installations-Assistenten
4. Starten Sie die Anwendung über die Desktop-Verknüpfung oder das Startmenü

### Manuelle Installation
1. Laden Sie das neueste ZIP-Archiv von [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases) herunter
2. Extrahieren Sie es in Ihren gewünschten Ordner
3. Führen Sie `DartTournamentPlaner.exe` aus

> **Hinweis**: Die Anwendung prüft beim Start automatisch auf Updates und bietet nahtlose Update-Installation.

## 🚀 Schnellstart

### Ihr erstes Turnier erstellen
1. **Turnierklasse wählen**: Wählen Sie aus Platinum, Gold, Silver oder Bronze
2. **Gruppen hinzufügen**: Klicken Sie auf **"Gruppe hinzufügen"** um Turniergruppen zu erstellen
3. **Spieler hinzufügen**: Fügen Sie Spieler zu jeder Gruppe hinzu (mindestens 2 pro Gruppe)
4. **Regeln konfigurieren**: Verwenden Sie **"Regeln konfigurieren"** für Spielparameter
5. **Matches generieren**: Klicken Sie auf **"Matches generieren"** für automatische Round-Robin-Erstellung
6. **Ergebnisse eingeben**: Klicken Sie auf Matches um Ergebnisse einzugeben
7. **Phasen fortsetzen**: Verwenden Sie **"Nächste Phase starten"** wenn die Gruppenphase abgeschlossen ist

### 🖨️ **Turnierberichte drucken**
1. **Druckmenü öffnen**: Gehen Sie zu **Datei** → **Drucken** oder verwenden Sie Strg+P
2. **Inhalte auswählen**: Wählen Sie was gedruckt werden soll (Gruppen, Finals, Knockout)
3. **Optionen konfigurieren**: Wählen Sie spezifische Gruppen oder Turnierphasen
4. **Vorschau**: Überprüfen Sie die Druckvorschau vor dem Drucken
5. **Drucken**: Erstellen Sie professionelle Turnierberichte

### 🌐 **Tournament Hub verwenden**
1. **Registrieren**: Gehen Sie zu **Tournament Hub** → **Mit Hub registrieren**
2. **URL teilen**: Die Join-URL wird automatisch in die Zwischenablage kopiert
3. **Live-Updates**: Match-Ergebnisse werden automatisch synchronisiert
4. **Multi-Device**: Zugriff von verschiedenen Geräten über die Join-URL

### Turnierphasen
1. **Gruppenphase**: Round-Robin innerhalb jeder Gruppe
2. **Finals/Knockout**: Basierend auf Ihrer Konfiguration:
   - **Nur Gruppenphase**: Turnier endet nach den Gruppen
   - **Finalrunde**: Top-Spieler kämpfen in Round-Robin
   - **Knockout-System**: Einzel- oder Doppel-Eliminierung

## 📋 Erweiterte Features

### 📊 **Spieler-Statistiken**
- **Match-Effizienz**: Verfolgung der schnellsten Spieldauer
- **Wurf-Performance**: Analyse der Wurf-Effizienz pro Match
- **High Finish Tracking**: Detaillierte Aufzeichnung aller High Finishes
- **180er-Statistiken**: Vollständige Maximum-Score-Verfolgung
- **Checkout-Analyse**: Erfolgreiche Checkout-Statistiken
- **Performance-Trends**: Langzeit-Performance-Analyse

### 🖨️ **Professionelles Drucksystem**
- **Turnier-Statistiken**: Komplette Turnierberichte mit allen Phasen
- **Gruppen-Berichte**: Individuelle Gruppen-Ranglisten und Match-Ergebnisse
- **Finals-Dokumentation**: Finals-Runde Teilnehmer und Ergebnisse
- **Knockout-Brackets**: Winner- und Loser-Bracket-Visualisierung
- **Teilnehmer-Listen**: Umfassende Spieler-Auflistungen
- **Anpassbare Titel**: Benutzerdefinierte Titel und Untertitel für Berichte

### 🌐 **Tournament Hub System**
- **WebSocket-Verbindung**: Echtzeit-Kommunikation mit Tournament Hub
- **Automatische Synchronisation**: Live-Updates von Match-Ergebnissen
- **QR-Code-Generation**: Einfacher Zugang über QR-Codes
- **Multi-User-Support**: Mehrere Benutzer können gleichzeitig teilnehmen
- **Robust Connection**: Automatische Wiederverbindung bei Verbindungsfehlern

### 🔑 **Lizenz-Management**
- **Feature-Kontrolle**: Granulare Kontrolle über Premium-Features
- **Offline-Validierung**: Funktioniert auch ohne Internetverbindung
- **Lizenz-Status**: Detaillierte Anzeige des aktuellen Lizenz-Status
- **Einfache Aktivierung**: Benutzerfreundliche Lizenz-Aktivierung
- **Support-Integration**: Direkte Links zu Support und Lizenzkauf

### 🎨 **Theme-Anpassung**
- **Vollständige Theme-Unterstützung**: Konsistente Darstellung über alle UI-Elemente
- **Theme-Toggle**: Schneller Wechsel zwischen Light- und Dark-Mode
- **Persistente Einstellungen**: Theme-Präferenzen werden gespeichert
- **Moderne Farbpaletten**: Professionelle Farbschemata für beide Modi

## 🌍 Internationalisierung

Die Anwendung verfügt über ein vollständig überarbeitetes Lokalisierungssystem:

### Unterstützte Sprachen
- 🇩🇪 **Deutsch** (Standard mit 400+ Übersetzungen)
- 🇬🇧 **English** (Vollständige Übersetzung mit Turnier-Terminologie)

### Sprachsystem-Features
- **Modulares Design**: Separate Sprachprovider-Dateien für Wartbarkeit
- **Echtzeit-Wechsel**: Sprachwechsel ohne Anwendungsneustart
- **Kontext-spezifisch**: Turnier- und sport-spezifische Terminologie
- **Druckunterstützung**: Vollständige Übersetzungsunterstützung für Druckdokumente
- **Dynamischer Inhalt**: Versions-bewusste AboutText und dynamische Inhalte

**Sprache ändern**: Einstellungen → Sprache → Gewünschte Sprache auswählen

### Für Entwickler: Neue Sprachen hinzufügen
```csharp
// Neuen Sprachprovider erstellen
public class FrenchLanguageProvider : ILanguageProvider
{
    public string LanguageCode => "fr";
    public string DisplayName => "Français";
    public Dictionary<string, string> GetTranslations() { /* translations */ }
}
```

## 🛠️ Entwicklung

### Technischer Stack
- **Framework**: .NET 9.0 mit C# 13.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Architektur**: MVVM-Pattern mit Service-orientiertem Design
- **Abhängigkeiten**: 
  - `Newtonsoft.Json` (13.0.3) für Datenserialisierung
  - `Microsoft.VisualBasic` (10.3.0) für Input-Dialoge
  - `QRCoder` (1.6.0) für QR-Code-Generierung
  - `System.Management` (9.0.0) für Systeminformationen
- **Lokalisierung**: Modulares ILanguageProvider-System
- **Auto-Update**: GitHub Releases API Integration
- **Drucken**: WPF Document/FlowDocument-System
- **Theme-System**: ResourceDictionary-basierte Theme-Implementierung

### Build-Anforderungen
- **Visual Studio 2022** (17.8 oder höher) oder **Visual Studio Code**
- **.NET 9.0 SDK**
- **Windows 10/11** für Entwicklung und Tests

### Aus Quellcode erstellen
```bash
# Repository klonen
git clone https://github.com/Peschi90/Dart-Turnament-Planer.git

# Zum Projekt-Verzeichnis navigieren
cd Dart-Turnament-Planer

# Abhängigkeiten wiederherstellen
dotnet restore

# Projekt erstellen
dotnet build --configuration Release

# Anwendung ausführen
dotnet run --project DartTournamentPlaner
```

### Projektstruktur
```
DartTournamentPlaner/
├── Models/              # Datenmodelle und Business-Entitäten
│   ├── Match.cs         # Match-Verwaltung
│   ├── Player.cs        # Spieler-Informationen
│   ├── Group.cs         # Gruppen-Verwaltung
│   ├── TournamentClass.cs # Turnierklassen-Struktur
│   ├── KnockoutMatch.cs # Knockout-spezifische Matches
│   ├── Statistics/      # Statistik-Modelle
│   └── License/         # Lizenz-System Modelle
├── Services/            # Business-Logic Services
│   ├── LocalizationService.cs # Multi-Sprachen-Support
│   ├── ConfigService.cs # Anwendungskonfiguration
│   ├── DataService.cs   # Datenpersistierung
│   ├── UpdateService.cs # Automatische Updates
│   ├── PrintService.cs  # Drucksystem
│   ├── ThemeService.cs  # Theme-Verwaltung
│   ├── License/         # Lizenz-Services
│   ├── Statistics/      # Statistik-Services
│   ├── HubWebSocket/    # WebSocket-Services
│   └── Languages/       # Sprachprovider-Dateien
├── Views/              # UI-Dialoge und Fenster
│   ├── MainWindow.xaml  # Hauptanwendungsfenster
│   ├── TournamentOverviewWindow.xaml # Präsentationsmodus
│   ├── TournamentPrintDialog.xaml # Druck-Dialog
│   ├── StartupSplashWindow.xaml # Start-Bildschirm
│   ├── SettingsWindow.xaml # Einstellungen
│   └── License/         # Lizenz-Dialoge
├── Controls/           # Benutzerdefinierte WPF-Controls
│   ├── TournamentTab.xaml # Haupt-Turnier-Interface
│   ├── PlayerStatisticsView.xaml # Spieler-Statistiken
│   └── LoadingSpinner.xaml # Lade-Animationen
├── Helpers/            # Utility-Klassen
│   ├── PrintHelper.cs   # Druck-Utilities
│   ├── TournamentDialogHelper.cs
│   ├── MainWindowUIHelper.cs
│   └── TournamentTabUIManager.cs
├── Themes/             # Theme-Ressourcen
│   ├── LightTheme.xaml  # Helles Theme
│   └── DarkTheme.xaml   # Dunkles Theme
└── Assets/             # Bilder, Icons und Ressourcen
```

## 📈 Versionshistorie

### Aktuell: v0.1.8 (Neueste) - Erweiterte Statistiken & Hub-Integration
- 📊 **NEU**: Erweiterte Spieler-Statistiken mit "Schnellstes Match" und "Wenigste Würfe"
- 🌐 **VERBESSERT**: WebSocket-Statistik-Integration mit vollständiger Datenextraktion
- 🎯 **NEU**: Tournament Hub Integration mit Echtzeit-Synchronisation
- 🎭 **NEU**: Statistiken-Tab für detaillierte Spieler-Analysen
- 🐛 **BEHOBEN**: Reset-Button-Probleme und UI-Synchronisation
- 🏗️ **VERBESSERT**: Manager-Klassen für bessere Code-Organisation

### Früher: v0.1.7 - Print System & Lokalisierung
- 🖨️ **NEU**: Professionelles Drucksystem mit umfassenden Turnierberichten
- 🌍 **VERBESSERT**: Modulares Lokalisierungssystem mit separaten Sprachprovidern
- 💝 **NEU**: Spendensystem mit GitHub Sponsors Integration
- 🎨 **VERBESSERT**: Professionelle Starterfahrung mit animiertem Splash Screen
- 📋 **ERWEITERT**: 400+ Übersetzungsschlüssel mit kontext-spezifischen Begriffen

### Geplant: v1.0.0
- 🎨 **Theme-Anpassung**: Erweiterte Theme-Optionen und Farbschemata
- 📱 **Responsive Design**: Verbesserte UI für verschiedene Bildschirmgrößen
- 🔗 **Online-Turnier-Integration**: Cloud-basierte Turnier-Synchronisation
- 📧 **E-Mail-Benachrichtigungen**: Automatische Turnier-Einladungen und Updates
- 🏆 **Achievement-System**: Spieler-Achievements und Ranking-System
- 📊 **Erweiterte Analytics**: Detaillierte Turnier-Analysen und Berichte

## 🤝 Beitragen

Wir begrüßen Beiträge! So können Sie helfen:

### Erste Schritte
1. **Fork** das Repository
2. **Klonen** Sie Ihren Fork lokal
3. **Erstellen** Sie einen Feature-Branch (`git checkout -b feature/AmazingFeature`)
4. **Machen** Sie Ihre Änderungen
5. **Testen** Sie gründlich
6. **Committen** Sie Ihre Änderungen (`git commit -m 'Add AmazingFeature'`)
7. **Pushen** Sie zu Ihrem Branch (`git push origin feature/AmazingFeature`)
8. **Erstellen** Sie einen Pull Request

### Entwicklungsrichtlinien
- Befolgen Sie bestehenden Code-Stil und Konventionen
- Fügen Sie angemessene Kommentare und Dokumentation hinzu
- Schließen Sie Unit-Tests für neue Features ein
- Aktualisieren Sie Übersetzungen für neue UI-Elemente
- Testen Sie auf verschiedenen Bildschirmauflösungen und Windows-Versionen

### Bereiche für Beiträge
- **Neue Sprachen**: Support für zusätzliche Sprachen über ILanguageProvider
- **Druck-Features**: Erweiterte Druck-Layouts und Optionen
- **Turnier-Formate**: Zusätzliche Turnier-Strukturen
- **UI-Verbesserungen**: Erweiterte Benutzerfreundlichkeits-Features
- **Bug-Fixes**: Fehlerbehebung und Stabilitätsverbesserungen
- **Dokumentation**: Hilfe-Inhalte und Benutzerhandbücher
- **Statistik-Features**: Erweiterte Analysen und Berichte
- **Theme-Entwicklung**: Neue Themes und Anpassungsoptionen

## 📄 Lizenz

Dieses Projekt ist unter der **MIT-Lizenz** lizenziert - siehe die [LICENSE](LICENSE) Datei für Details.

### Was das bedeutet:
- ✅ **Kommerzielle Nutzung** erlaubt
- ✅ **Modifikation** erlaubt
- ✅ **Verteilung** erlaubt
- ✅ **Private Nutzung** erlaubt
- ❗ **Keine Garantie** bereitgestellt
- ❗ **Lizenz- und Copyright-Hinweis** erforderlich

## 💝 Projekt unterstützen

Sie lieben den Dart Tournament Planner? So können Sie die Entwicklung unterstützen:

### 💰 Finanzielle Unterstützung
- **In-App-Spenden**: Verwenden Sie den integrierten Spenden-Dialog (**Hilfe** → **Spenden**)
- **GitHub Sponsors**: [Auf GitHub sponsern](https://github.com/sponsors/Peschi90)
- **Einmalige Spenden**: [PayPal](https://www.paypal.com/paypalme/I3ull3t)
- **Professioneller Support**: Individuelle Entwicklung und Enterprise-Features

### Nicht-finanzielle Unterstützung
- ⭐ **Bewerten** Sie das Repository auf GitHub
- 🐛 **Melden** Sie Bugs und schlagen Sie Verbesserungen vor
- 📢 **Teilen** Sie mit Ihrer Dart-Community
- 📝 **Schreiben** Sie Bewertungen und Tutorials
- 🌍 **Helfen** Sie bei Übersetzungen für zusätzliche Sprachen
- 🖨️ **Testen** Sie Druckfunktionalität und geben Sie Feedback
- 📊 **Testen** Sie Statistik-Features und melden Sie Probleme

### Unternehmens-Support
Für Unternehmen, die diese Software verwenden:
- 🏢 **Unternehmens-Lizenzierung**: Kontakt für kommerzielle Support-Optionen
- 🤝 **Partnerschaftsmöglichkeiten**: Zusammenarbeit bei Turnieren und Events
- 📊 **Individuelle Features**: Gesponserte Entwicklung spezifischer Anforderungen
- 🖨️ **Professionelles Drucken**: Individuelle Druck-Layouts und Branding

## 📞 Kontakt & Links

### Offizielle Links
- **GitHub Repository**: [Peschi90/Dart-Turnament-Planer](https://github.com/Peschi90/Dart-Turnament-Planer)
- **Releases**: [Neueste Downloads](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
- **Issues & Bug Reports**: [GitHub Issues](https://github.com/Peschi90/Dart-Turnament-Planer/issues)
- **Diskussionen**: [GitHub Discussions](https://github.com/Peschi90/Dart-Turnament-Planer/discussions)

### Entwickler-Kontakt
- **GitHub**: [@Peschi90](https://github.com/Peschi90)
- **E-Mail**: m@peschi.info

### Community
- 🎯 **Dart Community**: Teilen Sie Ihre Turniere und Erfahrungen
- 💬 **Feature Requests**: Schlagen Sie neue Features über GitHub Issues vor
- 📖 **Dokumentation**: Helfen Sie bei der Verbesserung von Benutzerhandbüchern und Tutorials
- 🖨️ **Druck-Templates**: Teilen Sie individuelle Druck-Layouts und Designs
- 📊 **Statistik-Feedback**: Teilen Sie Ihre Erfahrungen mit den Statistik-Features

---

*Entwickelt mit ❤️ für die Dart Community*

**"Perfekte Turniere beginnen mit perfekter Planung - analysiere sie intelligent!"** 🎯📊
