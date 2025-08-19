# 🎯 Dart Tournament Planner

A modern WPF application for managing dart tournaments with professional features for tournament organizers.

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![C#](https://img.shields.io/badge/C%23-13.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)
![Version](https://img.shields.io/badge/Version-0.1.0-brightgreen)

## 🏆 Features

### Tournament Management
- **Multiple Tournament Classes**: Manage up to 4 different classes (Platinum, Gold, Silver, Bronze)
- **Flexible Group Phase**: Round-Robin system with unlimited groups
- **Knockout System**: Single or Double Elimination with Winner/Loser Bracket
- **Finals Rounds**: Round-Robin finals for qualified players
- **Auto-Save System**: Configurable automatic saving with customizable intervals

### Match Management
- **Automatic Match Generation**: Round-Robin matches are created automatically
- **Flexible Game Rules**: 301, 401, or 501 points with Single/Double Out
- **Set System**: Configurable sets and legs with detailed validation
- **Round-Specific Rules**: Different rules for quarterfinals, semifinals, finals, etc.
- **Bye System**: Automatic bye assignment for odd number of players

### User Experience
- **Multilingual Support**: German and English with dynamic language switching
- **Modern UI**: Intuitive WPF interface with professional design
- **Tournament Overview**: Full-screen presentation mode with auto-cycling
- **Startup Splash Screen**: Professional loading screen with progress indicators
- **Auto-Update System**: Automatic update checking with GitHub integration
- **Bug Report System**: Integrated error reporting with system information

### Professional Features
- **Loading Animations**: Smooth loading spinners for better user experience
- **Donation Support**: Built-in support system for project development
- **Advanced Localization**: Complete translation system with over 400 localized strings
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Data Validation**: Advanced match result validation with conflict detection

## 🔧 System Requirements

- **Operating System**: Windows 10 or higher
- **.NET Runtime**: .NET 9.0 Runtime
- **Architecture**: x64 or x86
- **Memory**: Minimum 512 MB RAM
- **Storage**: 50 MB free space

## 📦 Installation

### Automatic Installation (Recommended)
1. Download the latest `Setup-DartTournamentPlaner-v0.1.0.exe` from [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
2. Run the installer (Administrator rights may be required)
3. Follow the installation wizard
4. Launch the application from the desktop shortcut or Start menu

### Manual Installation
1. Download the latest ZIP archive from [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
2. Extract to your desired folder
3. Run `DartTournamentPlaner.exe`

> **Note**: The application will automatically check for updates on startup and notify you when new versions are available.

## 🚀 Quick Start

### Creating Your First Tournament
1. **Select Tournament Class**: Choose from Platinum, Gold, Silver, or Bronze
2. **Add Groups**: Click **"Add Group"** to create tournament groups
3. **Add Players**: Add players to each group (minimum 2 per group)
4. **Configure Rules**: Use **"Configure Rules"** to set game parameters
5. **Generate Matches**: Click **"Generate Matches"** for automatic Round-Robin creation
6. **Enter Results**: Click on matches to enter results
7. **Advance Phases**: Use **"Start Next Phase"** when group phase is complete

### Tournament Phases
1. **Group Phase**: Round-Robin within each group
2. **Finals/Knockout**: Based on your configuration:
   - **Group Phase Only**: Tournament ends after groups
   - **Finals Round**: Top players compete in Round-Robin
   - **Knockout System**: Single or Double Elimination brackets

## 📋 Advanced Features

### Knockout System
- **Single Elimination**: Traditional knockout tournament
- **Double Elimination**: Winner Bracket + Loser Bracket system
- **Automatic Seeding**: Based on group phase performance
- **Bye Management**: Automatic bye assignment and management
- **Tree Visualization**: Visual tournament bracket display

### Game Rules Configuration
- **Game Modes**: 301, 401, 501 points starting score
- **Finish Modes**: Single Out or Double Out finishing
- **Set System**: Best-of-X sets with configurable legs per set
- **Round-Specific Rules**: Custom rules for different tournament stages
- **Validation System**: Automatic result validation and error checking

### Tournament Overview System
- **Full-Screen Mode**: Perfect for projectors and presentations
- **Auto-Cycling**: Automatic switching between classes and phases
- **Configurable Timing**: Customizable display intervals
- **Multi-Monitor Support**: Ideal for dual-screen setups
- **Real-Time Updates**: Live standings and match results

### Professional Tools
- **Auto-Save**: Configurable automatic saving (1-60 minute intervals)
- **Data Export/Import**: JSON-based tournament data management
- **Comprehensive Logging**: Detailed debug information for troubleshooting
- **Update System**: Automatic GitHub-based update checking
- **Bug Reporting**: Integrated system information and error reporting

## 🌍 Internationalization

The application supports complete localization:
- 🇩🇪 **German** (Deutsch) - Default language
- 🇬🇧 **English** - Full translation available

**Change Language**: Settings → Language → Select preferred language

All UI elements, messages, and help content are fully translated with over 400 localized strings.

## 💾 Data Management

### Automatic Saving
- **Auto-Save Options**: Enable/disable automatic saving
- **Configurable Intervals**: 1-60 minutes between saves
- **Change Detection**: Smart saving only when changes are detected
- **Status Indicators**: Visual feedback for save status

### Data Format
- **JSON Storage**: Human-readable tournament data format
- **Version Control**: Data structure versioning for compatibility
- **Backup System**: Automatic backup creation on save
- **Export/Import**: Full tournament data portability

## 🔄 Update System

### Automatic Updates
- **GitHub Integration**: Automatic checking of GitHub releases
- **Background Checking**: Non-intrusive update detection
- **One-Click Updates**: Automated download and installation
- **Release Notes**: Detailed changelog display
- **Version Management**: Smart version comparison and rollback protection

### Manual Updates
If automatic updates fail, you can always:
1. Visit the [Releases page](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
2. Download the latest setup file
3. Run the installer to update

## 🐛 Bug Reporting & Support

### Integrated Bug Reporting
1. Use **Help** → **Report Bug** in the application
2. Fill out the detailed bug report form
3. System information is automatically included
4. Submit via email or GitHub Issues

### Manual Reporting
- **GitHub Issues**: [Create an issue](https://github.com/Peschi90/Dart-Turnament-Planer/issues)

## 🛠️ Development

### Technical Stack
- **Framework**: .NET 9.0 with C# 13.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM pattern with Service-oriented design
- **Dependencies**: 
  - `Newtonsoft.Json` (13.0.3) for data serialization
  - `Microsoft.VisualBasic` (10.3.0) for input dialogs
- **Localization**: Dictionary-based translation system
- **Auto-Update**: GitHub Releases API integration

### Build Requirements
- **Visual Studio 2022** (17.8 or later) or **Visual Studio Code**
- **.NET 9.0 SDK**
- **Windows 10/11** for development and testing

### Building from Source
```bash
# Clone the repository
git clone https://github.com/Peschi90/Dart-Turnament-Planer.git

# Navigate to project directory
cd Dart-Turnament-Planer

# Restore dependencies
dotnet restore

# Build the project
dotnet build --configuration Release

# Run the application
dotnet run --project DartTournamentPlaner
```

### Project Structure
```
DartTournamentPlaner/
├── Models/              # Data models and business entities
│   ├── Match.cs         # Match management
│   ├── Player.cs        # Player information
│   ├── Group.cs         # Group management
│   ├── TournamentClass.cs # Tournament class structure
│   └── KnockoutMatch.cs # Knockout-specific matches
├── Services/            # Business logic services
│   ├── LocalizationService.cs # Multi-language support
│   ├── ConfigService.cs # Application configuration
│   ├── DataService.cs   # Data persistence
│   └── UpdateService.cs # Automatic updates
├── Views/              # UI dialogs and windows
│   ├── MainWindow.xaml  # Main application window
│   ├── TournamentOverviewWindow.xaml # Presentation mode
│   ├── UpdateDialog.cs  # Update management
│   └── BugReportDialog.xaml.cs # Bug reporting
├── Controls/           # Custom WPF controls
│   ├── TournamentTab.xaml # Main tournament interface
│   └── LoadingSpinner.xaml # Loading animations
├── Helpers/            # Utility classes
└── Assets/             # Images, icons, and resources
```

## 🤝 Contributing

We welcome contributions! Here's how you can help:

### Getting Started
1. **Fork** the repository
2. **Clone** your fork locally
3. **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
4. **Make** your changes
5. **Test** thoroughly
6. **Commit** your changes (`git commit -m 'Add AmazingFeature'`)
7. **Push** to your branch (`git push origin feature/AmazingFeature`)
8. **Create** a Pull Request

### Development Guidelines
- Follow existing code style and conventions
- Add appropriate comments and documentation
- Include unit tests for new features
- Update translations for new UI elements
- Test on different screen resolutions and Windows versions

### Areas for Contribution
- **New Tournament Formats**: Additional tournament structures
- **UI Improvements**: Enhanced user experience features
- **Localization**: Additional language support
- **Bug Fixes**: Issue resolution and stability improvements
- **Documentation**: Help content and user guides

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

### What this means:
- ✅ **Commercial use** allowed
- ✅ **Modification** allowed
- ✅ **Distribution** allowed
- ✅ **Private use** allowed
- ❗ **No warranty** provided
- ❗ **License and copyright notice** required

## 💝 Support the Project

Love the Dart Tournament Planner? Here's how you can support its development:

### Financial Support
- 💰 **Donate**: Use the integrated donation dialog (**Help** → **Donate**)
- 💰 **GitHub Sponsors**: [Sponsor on GitHub](https://github.com/sponsors/Peschi90)
- 💰 **One-time Donation**: [PayPal](https://www.paypal.com/paypalme/I3ull3t)

### Non-Financial Support
- ⭐ **Star** the repository on GitHub
- 🐛 **Report bugs** and suggest improvements
- 📢 **Share** with your dart community
- 📝 **Write reviews** and tutorials
- 🌍 **Help with translations** for additional languages

### Corporate Support
For businesses using this software:
- 🏢 **Corporate Licensing**: Contact for commercial support options
- 🤝 **Partnership Opportunities**: Collaboration on tournaments and events
- 📊 **Custom Features**: Sponsored development of specific requirements

## 📞 Contact & Links

### Official Links
- **GitHub Repository**: [Peschi90/Dart-Turnament-Planer](https://github.com/Peschi90/Dart-Turnament-Planer)
- **Releases**: [Latest Downloads](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
- **Issues & Bug Reports**: [GitHub Issues](https://github.com/Peschi90/Dart-Turnament-Planer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Peschi90/Dart-Turnament-Planer/discussions)

### Developer Contact
- **GitHub**: [@Peschi90](https://github.com/Peschi90)

### Community
- 🎯 **Dart Community**: Share your tournaments and experiences
- 💬 **Feature Requests**: Suggest new features via GitHub Issues
- 📖 **Documentation**: Help improve user guides and tutorials

---

## 📈 Version History

### Current: v0.1.0 (Latest)
- ✨ Initial public release
- 🏆 Complete tournament management system
- 🎮 Group phase with Round-Robin support
- ⚔️ Knockout system (Single/Double Elimination)
- 📺 Tournament overview with presentation mode
- 🌍 Multi-language support (German/English)
- 🔄 Automatic update system
- 💾 Auto-save functionality
---

*Developed with ❤️ for the Dart Community*

**"Make every dart count, organize every tournament perfectly!"** 🎯
