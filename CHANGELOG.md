## v0.1.7 (2025-08-21) - Print System & Localization Refactoring

### 🖨️ **NEW: Professional Print System**
- **Tournament Statistics Printing**: Comprehensive printing functionality with detailed tournament reports
- **Print Dialog**: User-friendly dialog for selecting print content (Group Phase, Finals, Knockout)
- **Print Preview**: Live preview of print content before printing
- **Flexible Print Options**: Print individual groups, complete tournaments, or specific phases
- **Professional Layout**: Formatted tournament reports with tables, standings, and match results

### 🌍 **IMPROVED: Localization System Refactoring**
- **Modular Language System**: Refactored to use separate language provider files
- **ILanguageProvider Interface**: Clean architecture for adding new languages
- **GermanLanguageProvider & EnglishLanguageProvider**: Separate files for each language
- **Enhanced Maintainability**: Easier to manage and extend translations
- **Dynamic Content Support**: Version-aware AboutText generation
- **400+ Translation Keys**: Comprehensive translation coverage including print system

### 💝 **NEW: Donation System**
- **Donation Dialog**: Integrated donation support for project development
- **GitHub Sponsors Integration**: Direct links to sponsorship platforms
- **Support Options**: Multiple ways to support the project financially

### 🎨 **IMPROVED: Startup Experience**
- **Professional Splash Screen**: Animated startup screen with progress indicators
- **Loading Animations**: Smooth loading spinners throughout the application
- **Progress Feedback**: Real-time feedback during application initialization
- **Update Check Integration**: Seamless update checking during startup

### 🔧 **TECHNICAL IMPROVEMENTS**
- **Code Architecture**: Improved separation of concerns with language providers
- **Print Service**: Comprehensive printing infrastructure with error handling
- **UI Enhancements**: Better user experience with loading indicators
- **Error Handling**: Enhanced error handling and user feedback
- **Performance**: Optimized loading times and resource usage

### 🌟 **UI/UX Enhancements**
- **Modern Design**: Updated UI elements with professional styling
- **Better Accessibility**: Improved tooltips and help content
- **Enhanced Navigation**: Streamlined user interactions
- **Visual Feedback**: Loading states and progress indicators

### 🐛 **Bug Fixes**
- Fixed localization refresh mechanisms
- Improved error handling in print operations  
- Enhanced stability of splash screen animations
- Resolved memory leaks in language provider loading

### 📋 **Translation Updates**
- Extended German translations with print-specific terms
- Enhanced English translation coverage
- Added context-specific translations for UI elements
- Improved consistency across all translated content

---
## v0.1.6
- added possibly to Print Tournament statistics
- bugfix final round stats where not open at the start
- bugfix tournament overview window was not movable
## v0.1.1
- Initial release of the project.
- generate Tournaments for
  - `Groupphase only`
  - `Round Robin`
  - `KO System`
- Tournament overview Window to display all classes and their matches.
  - `automatic tab switch to let it run continiously`
## v0.1.0 (Previous Release)
- Initial release of the project
- Generate Tournaments for:
  - `Group phase only`
  - `Round Robin Finals` 
  - `KO System` (Single/Double Elimination)
- Tournament overview Window to display all classes and their matches
  - `Automatic tab switching for continuous display`
- Multi-language support (German/English)
- Auto-save functionality
- Bug report system
- Auto-update mechanism

## v0.0.0
- Development version
