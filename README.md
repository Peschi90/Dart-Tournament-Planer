# 🎯 Dart Tournament Planner

A modern WPF application for managing dart tournaments with professional features for tournament organizers.

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![C#](https://img.shields.io/badge/C%23-13.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)
![Version](https://img.shields.io/badge/Version-0.1.7-brightgreen)

## 🏆 Features

### Tournament Management
- **Multiple Tournament Classes**: Manage up to 4 different classes (Platinum, Gold, Silver, Bronze)
- **Flexible Group Phase**: Round-Robin system with unlimited groups
- **Knockout System**: Single or Double Elimination with Winner/Loser Bracket
- **Finals Rounds**: Round-Robin finals for qualified players
- **Auto-Save System**: Configurable automatic saving with customizable intervals
- **Professional Workflows**: Streamlined tournament creation and management

### 🖨️ **NEW: Professional Print System**
- **Tournament Statistics Printing**: Comprehensive tournament reports with detailed statistics
- **Print Dialog**: User-friendly interface for selecting print content
- **Print Preview**: Real-time preview of documents before printing
- **Flexible Options**: Print individual groups, complete tournaments, or specific phases
- **Professional Layout**: Formatted reports with tables, standings, and match results
- **Multi-Phase Support**: Print Group Phase, Finals, and Knockout brackets separately

### Match Management
- **Automatic Match Generation**: Round-Robin matches are created automatically
- **Flexible Game Rules**: 301, 401, or 501 points with Single/Double Out
- **Set System**: Configurable sets and legs with detailed validation
- **Round-Specific Rules**: Different rules for quarterfinals, semifinals, finals, etc.
- **Bye System**: Automatic bye assignment for odd number of players
- **Result Validation**: Advanced match result validation with conflict detection

### 🌍 **IMPROVED: Multi-Language System**
- **Modular Localization**: Refactored language system with separate provider files
- **Dynamic Language Support**: Easy addition of new languages via ILanguageProvider interface
- **400+ Translation Keys**: Comprehensive translation coverage
- **German & English**: Complete translations with context-specific terms
- **Real-Time Switching**: Dynamic language switching without restart
- **Version-Aware Content**: Dynamic content that adapts to current application version

### User Experience
- **Professional Startup**: Animated splash screen with progress indicators
- **Modern UI**: Intuitive WPF interface with professional design
- **Tournament Overview**: Full-screen presentation mode with auto-cycling
- **Auto-Update System**: Automatic update checking with GitHub integration
- **Bug Report System**: Integrated error reporting with system information

### 💝 **NEW: Support & Donation System**
- **Donation Integration**: Built-in support for project development
- **GitHub Sponsors**: Direct links to sponsorship platforms
- **Multiple Support Options**: Various ways to contribute to the project

### Professional Features
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Data Validation**: Advanced tournament data validation and integrity checks
- **Performance Optimized**: Efficient resource usage and fast loading times
- **Backup System**: Automatic data backup and recovery mechanisms

## 🔧 System Requirements

- **Operating System**: Windows 10 or higher
- **.NET Runtime**: .NET 9.0 Runtime
- **Architecture**: x64 or x86
- **Memory**: Minimum 512 MB RAM
- **Storage**: 50 MB free space
- **Printer**: Optional - for print functionality

## 📦 Installation

### Automatic Installation (Recommended)
1. Download the latest `Setup-DartTournamentPlaner-v0.1.7.exe` from [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
2. Run the installer (Administrator rights may be required)
3. Follow the installation wizard
4. Launch the application from the desktop shortcut or Start menu

### Manual Installation
1. Download the latest ZIP archive from [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
2. Extract to your desired folder
3. Run `DartTournamentPlaner.exe`

> **Note**: The application automatically checks for updates on startup and provides seamless update installation.

## 🚀 Quick Start

### Creating Your First Tournament
1. **Select Tournament Class**: Choose from Platinum, Gold, Silver, or Bronze
2. **Add Groups**: Click **"Add Group"** to create tournament groups
3. **Add Players**: Add players to each group (minimum 2 per group)
4. **Configure Rules**: Use **"Configure Rules"** to set game parameters
5. **Generate Matches**: Click **"Generate Matches"** for automatic Round-Robin creation
6. **Enter Results**: Click on matches to enter results
7. **Advance Phases**: Use **"Start Next Phase"** when group phase is complete

### 🖨️ **NEW: Printing Tournament Reports**
1. **Access Print Menu**: Go to **File** → **Print** or use Ctrl+P
2. **Select Content**: Choose what to print (Groups, Finals, Knockout)
3. **Configure Options**: Select specific groups or tournament phases
4. **Preview**: Review the print preview before printing
5. **Print**: Generate professional tournament reports

### Tournament Phases
1. **Group Phase**: Round-Robin within each group
2. **Finals/Knockout**: Based on your configuration:
   - **Group Phase Only**: Tournament ends after groups
   - **Finals Round**: Top players compete in Round-Robin
   - **Knockout System**: Single or Double Elimination brackets

## 📋 Advanced Features

### 🖨️ Professional Printing System
- **Tournament Statistics**: Complete tournament reports with all phases
- **Group Reports**: Individual group standings and match results  
- **Finals Documentation**: Finals round participants and results
- **Knockout Brackets**: Winner and Loser bracket visualization
- **Participant Lists**: Comprehensive player listings
- **Custom Titles**: Add custom titles and subtitles to reports

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

### 🌍 Enhanced Localization
- **Modular Architecture**: Language providers for easy extension
- **Comprehensive Coverage**: 400+ translated interface elements
- **Context-Aware**: Sport-specific and tournament-specific translations
- **Dynamic Content**: Version-aware and context-sensitive translations
- **Easy Extension**: Add new languages through ILanguageProvider interface

## 🌍 Internationalization

The application features a completely refactored localization system:

### Supported Languages
- 🇩🇪 **German** (Deutsch) - Default language with 400+ translations
- 🇬🇧 **English** - Complete translation with tournament terminology

### Language System Features
- **Modular Design**: Separate language provider files for maintainability
- **Real-Time Switching**: Change languages without application restart
- **Context-Specific**: Tournament and sport-specific terminology
- **Print Support**: Full translation support for print documents
- **Dynamic Content**: Version-aware AboutText and dynamic content

**Change Language**: Settings → Language → Select preferred language

### For Developers: Adding New Languages
```csharp
// Create new language provider
public class FrenchLanguageProvider : ILanguageProvider
{
    public string LanguageCode => "fr";
    public string DisplayName => "Français";
    public Dictionary<string, string> GetTranslations() { /* translations */ }
}
```

## 💾 Data Management

### Automatic Saving
- **Auto-Save Options**: Enable/disable automatic saving
- **Configurable Intervals**: 1-60 minutes between saves
- **Change Detection**: Smart saving only when changes are detected
- **Status Indicators**: Visual feedback for save status
- **Backup Creation**: Automatic backup on save operations

### Data Format
- **JSON Storage**: Human-readable tournament data format
- **Version Control**: Data structure versioning for compatibility
- **Backup System**: Automatic backup creation on save
- **Export/Import**: Full tournament data portability

## 🔄 Update System

### Automatic Updates
- **GitHub Integration**: Automatic checking of GitHub releases
- **Background Checking**: Non-intrusive update detection during startup
- **Professional UI**: Integrated update dialog with changelog
- **One-Click Updates**: Automated download and installation
- **Release Notes**: Detailed changelog display with Markdown support
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
- **Email**: m@peschi.info

## 🛠️ Development

### Technical Stack
- **Framework**: .NET 9.0 with C# 13.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM pattern with Service-oriented design
- **Dependencies**: 
  - `Newtonsoft.Json` (13.0.3) for data serialization
  - `Microsoft.VisualBasic` (10.3.0) for input dialogs
- **Localization**: Modular ILanguageProvider system
- **Auto-Update**: GitHub Releases API integration
- **Printing**: WPF Document/FlowDocument system

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
│   ├── UpdateService.cs # Automatic updates
│   ├── PrintService.cs  # Print system
│   └── Languages/       # Language provider files
│       ├── ILanguageProvider.cs
│       ├── GermanLanguageProvider.cs
│       └── EnglishLanguageProvider.cs
├── Views/              # UI dialogs and windows
│   ├── MainWindow.xaml  # Main application window
│   ├── TournamentOverviewWindow.xaml # Presentation mode
│   ├── TournamentPrintDialog.xaml # Print dialog
│   ├── UpdateDialog.cs  # Update management
│   ├── DonationDialog.xaml.cs # Donation support
│   ├── StartupSplashWindow.xaml # Startup screen
│   └── BugReportDialog.xaml.cs # Bug reporting
├── Controls/           # Custom WPF controls
│   ├── TournamentTab.xaml # Main tournament interface
│   └── LoadingSpinner.xaml # Loading animations
├── Helpers/            # Utility classes
│   ├── PrintHelper.cs   # Print utilities
│   ├── TournamentDialogHelper.cs
│   └── TournamentUIHelper.cs
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
- **New Languages**: Add support for additional languages via ILanguageProvider
- **Print Features**: Enhanced print layouts and options
- **Tournament Formats**: Additional tournament structures
- **UI Improvements**: Enhanced user experience features
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

### 💰 ### Financial Support
- **In-App Donations**: Use the integrated donation dialog (**Help** → **Donate**)
- **GitHub Sponsors**: [Sponsor on GitHub](https://github.com/sponsors/Peschi90)
- **One-time Donations**: [PayPal](https://www.paypal.com/paypalme/I3ull3t)
- **Professional Support**: Custom development and enterprise features

### Non-Financial Support
- ⭐ **Star** the repository on GitHub
- 🐛 **Report bugs** and suggest improvements
- 📢 **Share** with your dart community
- 📝 **Write reviews** and tutorials
- 🌍 **Help with translations** for additional languages
- 🖨️ **Test print functionality** and provide feedback

### Corporate Support
For businesses using this software:
- 🏢 **Corporate Licensing**: Contact for commercial support options
- 🤝 **Partnership Opportunities**: Collaboration on tournaments and events
- 📊 **Custom Features**: Sponsored development of specific requirements
- 🖨️ **Professional Printing**: Custom print layouts and branding

## 📞 Contact & Links

### Official Links
- **GitHub Repository**: [Peschi90/Dart-Turnament-Planer](https://github.com/Peschi90/Dart-Turnament-Planer)
- **Releases**: [Latest Downloads](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
- **Issues & Bug Reports**: [GitHub Issues](https://github.com/Peschi90/Dart-Turnament-Planer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Peschi90/Dart-Turnament-Planer/discussions)

### Developer Contact
- **GitHub**: [@Peschi90](https://github.com/Peschi90)
- **Email**: m@peschi.info

### Community
- 🎯 **Dart Community**: Share your tournaments and experiences
- 💬 **Feature Requests**: Suggest new features via GitHub Issues
- 📖 **Documentation**: Help improve user guides and tutorials
- 🖨️ **Print Templates**: Share custom print layouts and designs

---

## 📈 Version History

### Current: v0.1.7 (Latest) - Print System & Localization Refactoring
- 🖨️ **NEW**: Professional Print System with comprehensive tournament reports
- 🌍 **IMPROVED**: Modular localization system with separate language providers
- 💝 **NEW**: Donation system with GitHub Sponsors integration
- 🎨 **IMPROVED**: Professional startup experience with animated splash screen
- 📋 **Enhanced**: 400+ translation keys with context-specific terms
- 🐛 **Fixed**: Various stability improvements and bug fixes

### Previous: v0.1.0
- ✨ Initial public release
- 🏆 Complete tournament management system
- 🎮 Group phase with Round-Robin support
- ⚔️ Knockout system (Single/Double Elimination)
- 📺 Tournament overview with presentation mode
- 🌍 Multi-language support (German/English)
- 🔄 Automatic update system
- 💾 Auto-save functionality

### Upcoming: v1.0.0 (Planned)
- 📊 Advanced statistics and analytics dashboard
- 🎨 Theme customization and dark mode
- 📱 Responsive design improvements
- 🔗 Online tournament integration
- 📧 Email notifications and tournament invitations
- 🏆 Achievement and ranking system

---

*Developed with ❤️ for the Dart Community*

**"Perfect tournaments start with perfect planning - print them beautifully!"** 🎯🖨️
