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

