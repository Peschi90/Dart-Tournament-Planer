# 🎯 Dart Tournament Planner

A modern WPF application for managing dart tournaments with professional features for tournament organizers.

![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![C#](https://img.shields.io/badge/C%23-13.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)

## 🏆 Features

### Tournament Management
- **Multiple Tournament Classes**: Manage up to 4 different classes (Platinum, Gold, Silver, Bronze)
- **Flexible Group Phase**: Round-Robin system with unlimited groups
- **Knockout System**: Single or Double Elimination with Winner/Loser Bracket
- **Finals Rounds**: Round-Robin finals for qualified players

### Match Management
- **Automatic Match Generation**: Round-Robin matches are created automatically
- **Flexible Game Rules**: 301, 401, or 501 points with Single/Double Out
- **Set System**: Configurable sets and legs
- **Round-Specific Rules**: Different rules for different tournament rounds

### User Experience
- **Multilingual**: German and English support
- **Modern UI**: Intuitive WPF interface
- **Auto-Save**: Automatic tournament data saving
- **Tournament Overview**: Full-screen mode for presentations
- **Bug Report System**: Integrated error reporting

## 🔧 System Requirements

- **Operating System**: Windows 10 or higher
- **.NET**: .NET 9.0 Runtime
- **Architecture**: x64 or x86

## 📦 Installation

1. Download the latest version from [Releases](https://github.com/Peschi90/Dart-Turnament-Planer/releases)
2. Extract the ZIP file to your desired folder
3. Run `DartTournamentPlaner.exe`

> **Note**: You may need to install the .NET 9.0 Runtime if it's not already on your system.

## 🚀 Quick Start

### Creating a New Tournament
1. Select a tournament class (Platinum, Gold, Silver, Bronze)
2. Click **"Add Group"** to create groups
3. Add players to the groups
4. Configure game rules via **"Configure Rules"**
5. Set the post-group phase mode

### Managing Matches
1. Click **"Generate Matches"** for Round-Robin matches
2. Click on a match to enter the result
3. The standings table updates automatically
4. Use **"Start Next Phase"** to advance to knockout phase

## 📋 Features in Detail

### Group Phase
- Round-Robin system within each group
- Automatic standings calculation
- Minimum 2 players per group required
- Bye system for odd number of players

### Knockout System
- **Single Elimination**: Classic knockout system
- **Double Elimination**: Winner + Loser Bracket
- Automatic tournament tree generation
- Configurable qualification from group phase

### Game Rules
- **Game Modes**: 301, 401, 501 points
- **Finish Modes**: Single Out, Double Out
- **Set System**: Configurable sets and legs per set
- **Round Rules**: Different rules for quarterfinals, semifinals, etc.

### Tournament Overview
- Full-screen mode for presentations
- Automatic cycling between classes
- Perfect for projectors and second monitors
- Configurable display duration

## 🌍 Internationalization

The application supports:
- 🇩🇪 German (Default)
- 🇬🇧 English

Change language: **Settings** → **Language**

## 💾 Data Management

### Auto-Save
- Automatic saving can be enabled in settings
- Configurable save intervals
- Status indicator for unsaved changes

### Data Format
- Tournament data is stored in JSON format
- Import/Export via File menu
- Data structure versioning

## 🐛 Bug Reporting

For issues or feature requests:
1. Use the integrated Bug Report dialog (**Help** → **Report Bug**)
2. Or create a [GitHub Issue](https://github.com/Peschi90/Dart-Turnament-Planer/issues)

## 🛠️ Development

### Technical Details
- **Framework**: .NET 9.0, C# 13.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Architecture**: MVVM pattern with Services
- **Localization**: Dictionary-based system

### Build
```
# Clone repository
git clone https://github.com/Peschi90/Dart-Turnament-Planer.git

# Navigate to project directory
cd Dart-Turnament-Planer

# Build project
dotnet build

# Run application
dotnet run
```

### Project Structure
```
DartTournamentPlaner/
├── Models/           # Data models
├── Services/         # Business logic services
├── Views/           # UI dialogs and windows
├── Controls/        # Custom WPF controls
└── Resources/       # Resources and assets
```

## 🤝 Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Create a Pull Request

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) file for details.

## 💝 Support

Do you like the Dart Tournament Planner? Support the development:
- ⭐ Give the project a star on GitHub
- 🐛 Report bugs or suggest features
- 💰 [Donate via the integrated dialog](https://github.com/sponsors/Peschi90)

## 📞 Contact

- **GitHub**: [Peschi90](https://github.com/Peschi90)
- **Issues**: [GitHub Issues](https://github.com/Peschi90/Dart-Turnament-Planer/issues)


---

*Developed with ❤️ for the Dart Community*
