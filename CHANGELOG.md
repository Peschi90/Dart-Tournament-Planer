## v0.1.13

### 🎯 New Features
- **🆔 Custom Tournament-ID**: Users can set custom Tournament-IDs or generate them automatically
  - Optional ID input field in Hub registration dialog
  - 🔄 Generate button for quick ID creation
  - ID validation and sanitization (alphanumeric characters, `_`, `-` only)
  - Automatic `TOURNAMENT_` prefix if not present
  - Persistent storage of Tournament-ID in `TournamentData`
  - Reuse of saved IDs on reconnect for persistent QR codes

### 🌐 Hub Integration Improvements
- **Tournament-ID Persistence**: Tournament-ID managed via `TournamentManagementService`
  - Correct Tournament-ID propagation in all dialog paths (Print, Overview, Match entry)
  - Auto-load of saved Tournament-ID during Hub registration
  - QR codes remain valid after connection interruptions
- **✨ NEW: Live-Match-Updates**: Real-time match progress tracking from Tournament Hub
  - Match-Start Events: Automatic detection when a match begins
  - Leg-Completed Events: Updates after each finished leg with detailed statistics
  - Match-Progress Events: Optional live updates during ongoing legs
  - Live-Status indicators (🔴 LIVE) in UI
  - Leg-Counter display (e.g. "Leg 2/5")
  - Detailed leg statistics (winner, darts, duration, average, checkout)
  - Automatic UI synchronization without data persistence
  - Full internationalization (DE/EN)
- **🔄 Automatic WebSocket Reconnect**: Robust connection recovery after server outages
  - Continuous reconnect attempts every 5-10 seconds until server is back online
  - Automatic tournament re-registration with preserved Tournament-ID
  - Full tournament data synchronization after reconnect (matches, game rules, players)
  - Duplicate reconnect prevention with smart scheduling
  - No manual intervention required - seamless reconnection
- **📊 Enhanced Connection Status Display**: Detailed WebSocket and tournament status indicators
  - Separate states for WebSocket connection and tournament registration
  - Status bar shows: Disconnected / WebSocket Ready / Tournament Registered
  - Visual indicators (🔴 Red / 🟢 Green / 🟡 Yellow) for connection state
  - Preserved Tournament-ID display during disconnection for reconnect info
  - Real-time status updates in both status bar and debug console
  - Clear distinction between connection loss and registration status

### 🔧 Bug Fixes
- **Tournament-ID in QR Codes**: Complete integration of Tournament-ID in all QR code generations
  - ✅ Print Dialog (all 4 methods)
  - ✅ Tournament Overview Window
  - ✅ Match Result Windows (Group phase, Finals, K.O.)
  - ✅ Tournament Tree (Winner & Loser Bracket)
  - ✅ EditMatchResult (TournamentTab)
- **Hub Registration Dialog**: 
  - Dialog height increased from 350px to 420px (all content fully visible)
  - Complete translation of all info texts (German & English)
  - TextBlocks dynamically translated instead of static texts
- **WebSocket Connection Management**:
  - Fixed NullReferenceException in CloseAsync method
  - Robust exception handling for ObjectDisposedException
  - Safe disposal of CancellationTokenSource
  - Prevented crash on disconnect
- **Status Bar Display**:
  - Fixed status bar remaining green after server disconnect
  - Eliminated duplicate event handlers causing incorrect status display
  - Status now correctly shows "Disconnected" when connection is lost
  - Tournament-ID preserved and displayed during reconnection attempts

### 🌍 Localization
- **Hub Registration Dialog Translations**: 13 new translation keys added
  - German translations in `GermanHubLanguageProvider`
  - English translations in `EnglishHubLanguageProvider`
  - Dynamic UI translation for all dialog elements
  - Tooltips and buttons fully translated

### 🏗️ Technical Improvements
- **HubIntegrationService**: Extended with custom Tournament-ID parameter
- **LicensedHubService**: Custom ID support with retry persistence
- **TournamentDataSyncService**: Enhanced game rules synchronization
  - Round-specific game rules for Winner/Loser Brackets
- **New UI Components**:
  - `HubRegistrationDialog.xaml` - Modern dialog with theme support
  - `HubRegistrationDialog.cs` - With translation support and ID management
- **Tournament-ID Propagation**: Correct passing via `TournamentManagementService.GetTournamentData()`
  - All print methods corrected
  - All match dialog paths corrected
  - TournamentTreeRenderer corrected
- **WebSocket Connection Architecture**:
  - New `HubConnectionState` enum with 5 distinct states (Disconnected, WebSocketReady, TournamentRegistered, Connecting, Error)
  - `ScheduleReconnect` method with duplicate prevention and smart scheduling
  - Separate tracking of WebSocket connection and tournament registration status
  - Event-driven architecture with `TournamentNeedsResync` event for automatic data synchronization
  - 4-step reconnect process: HTTP re-registration → WebSocket re-subscription → data sync → timer restart
- **Connection Status Management**:
  - Deprecated legacy `OnHubStatusChanged` handler in favor of detailed state handler
  - `UpdateHubStatusDetailed` method with comprehensive state visualization
  - Real-time status updates via `HubConnectionStateChanged` event
  - Comprehensive logging for all connection state changes

### 🎨 UI/UX Improvements
- **Hub Registration Dialog**:
  - Optimal dialog size (500x420px)
  - Modern animations and hover effects
  - Info box with helpful tips and notes
  - Generate button with icon (🔄)
  - Theme support (Light/Dark Mode)
  - No more cut-off content
- **Status Bar Enhancements**:
  - Color-coded connection indicators (Red/Green/Yellow)
  - Three-tier status display: Connection / Registration / Sync status
  - Tournament-ID preserved during disconnection for user awareness
  - Real-time status updates without page refresh
  - Clickable status area for debug console access
- **Debug Console Improvements**:
  - Detailed logging of all reconnect attempts
  - Step-by-step reconnection process visualization
  - WebSocket state tracking with timestamps
  - Tournament data sync progress indicators
  - Color-coded log messages for easy debugging

### 🚀 Performance & Stability
- **Reconnect Reliability**: Continuous retry mechanism until connection is restored
- **Memory Management**: Proper disposal of WebSocket and cancellation tokens
- **Exception Handling**: Comprehensive try-catch blocks preventing crashes
- **Resource Cleanup**: Safe cleanup of timers, connections, and event handlers
- **Thread Safety**: Dispatcher-based UI updates for thread-safe operations
- **State Management**: Robust tracking of connection and registration states

### 📚 Documentation
- **Code Comments**: Extensive documentation of reconnect logic and state management
- **Debug Logging**: Detailed trace output for all connection operations
- **State Transitions**: Clear documentation of connection state changes
- **Error Handling**: Comprehensive error logging and recovery documentation

## v0.1.12
- moved
	- Tournament Hub out of the repository.

- bugfixes
	- Fixed some bugs in the license system.
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
	- Progress bar for update to new version implemented. 
- bugfixes
	- Fixed some translation bugs.

## v0.1.10
- bugfixes
	- Fixed bug for license request email.

## v0.1.9
- bugfixes
	- Improved sharpness and overall clarity.

## v0.1.8

### 🎯 New Features
- **🔑 Complete License System**: Comprehensive feature management with Core/Premium features, offline validation and license management
- **🎨 Dark/Light Theme System**: Full theme switching with persistent settings and one-click toggle
- **📊 Extended Player Statistics**: New statistics columns "Fastest Match" and "Fewest Darts" with detailed performance analysis
- **🌐 Tournament Hub Integration**: Enhanced real-time synchronization with WebSocket connections and QR code support
- **📱 Professional Startup**: Animated splash screen with progress indicators and modern animations
- **🎭 Statistics Tab**: New dedicated tab for detailed player statistics in each tournament class

### 🔑 License System Features
- **Premium Feature Management**: Granular control over advanced features
- **Offline License Validation**: Functionality even without internet connection
- **License Dialogs**: User-friendly activation, status display and management
- **Feature-driven UI**: Dynamic display based on license status
- **Security Features**: Hardware ID-based licensing with activation limits

### 🎨 Theme System
- **Light/Dark Mode Toggle**: Complete theme switching via menu button
- **Consistent Theme Application**: Uniform display across all UI elements
- **Theme Persistence**: Storage of theme selection between app starts
- **Modern Color Palettes**: Professional color schemes for both modes
- **Real-time Theme Switching**: Theme change without application restart

### 🌐 Hub Integration Improvements
- **WebSocket Statistics Integration**: Complete extraction and processing of dart statistics from WebSocket messages
- **QR Code Support**: QRCoder integration for easy tournament access
- **Extended Match Data**: Processing of `dartScoringResult` for detailed game analysis
- **Automatic Synchronization**: Real-time updates of player statistics on match completion
- **Debug Console**: Global debug console for hub connection diagnostics
- **Fallback Mechanisms**: Robust processing for different data formats

### 📊 Statistics Features
- **Match Efficiency**: Display of fastest match duration (MM:SS format)
- **Throw Efficiency**: Tracking of fewest darts per match
- **Detailed Data**: High Finish details with darts breakdown
- **Leg Averages**: Tracking of individual leg performance
- **Checkout Statistics**: Count and details of all successful checkouts
- **180 Tracking**: Complete tracking of all maximum scores
- **Score Analysis**: Tracking of 26+ scores and performance trends

### 🔧 Improvements
- **UI Button Activation**: Reset buttons are no longer incorrectly grayed out
- **Match Statistics Processing**: Extended extraction of 180s, 26+ scores, high finishes and checkouts
- **Localization**: New German and English translations for all new features
- **Performance**: Optimized statistics calculation and UI updates
- **Debug Output**: Extended logging functionality for better error diagnosis
- **Splash Screen**: Professional startup experience with loading animations and status updates

### 🏗️ Technical Improvements
- **Manager Classes**: Split UI logic into specialized managers (TournamentTabUIManager, PlayerStatisticsManager, TranslationManager)
- **ThemeService**: Dedicated service class for theme management
- **LicenseFeatureService**: Comprehensive license feature management
- **Event Handling**: Improved event delegation for better maintainability
- **Code Organization**: Clearer separation of responsibilities
- **Type Safety**: Enhanced null-safety and error handling
- **.NET 9.0 Upgrade**: Update to the latest .NET version with C# 13.0

### 📱 User Experience
- **Animated Startup**: Professional splash screen with progress bar and status updates
- **Theme Toggle**: One-click switch between Light and Dark mode
- **Formatting**: Time displays in user-friendly MM:SS format
- **Tooltips**: Extended help texts for new statistics fields and features
- **Sorting**: Improved sorting options for statistics tables
- **License Management**: Intuitive license activation and management

### 🔄 API & Integration
- **QRCoder Package**: New dependency for QR code generation (v1.6.0)
- **System.Management**: New dependency for hardware ID generation (v9.0.0)
- **WebSocket Protocol**: Extended support for Tournament Hub WebSocket messages
- **JSON Parsing**: Robust processing of complex match data structures and license validation
- **Backward Compatibility**: Support for existing and new data formats
- **Error Recovery**: Improved recovery from connection errors

### 🐛 Bug Fixes
- **Reset Button Problem**: Tournament reset buttons work correctly again
- **Statistics Storage**: Correct persistence in tournament-data.json
- **UI Synchronization**: Improved user interface updates
- **Null Reference Handling**: More robust error handling for missing data
- **Theme Consistency**: Correct theme application across all UI components
- **License Validation**: Robust offline/online license verification

### 📚 Documentation
- **Code Comments**: Extended documentation of new features
- **Debug Logging**: Detailed trace output for development and maintenance
- **Localization Keys**: Complete translation keys for all UI elements
- **License Integration**: Comprehensive documentation of the license system
- **Theme System**: Documentation of the theme switching mechanism

### 🎁 Premium Features (License required)
- **📈 Extended Statistics**: Detailed player performance analysis with hub integration
- **🌐 Tournament Hub Premium**: Extended hub connection features
- **🖨️ Enhanced Printing**: Professional print layouts and extended options
- **📊 Tournament Overview Premium**: Extended presentation modes and features

### 🚀 Performance & Stability
- **Async/Await Pattern**: Consistent use of asynchronous programming
- **Memory Management**: Improved resource management and garbage collection
- **Exception Handling**: Robust error handling with user feedback
- **UI Responsiveness**: Improved UI responsiveness through background threading
- **Startup Performance**: Optimized application startup time with splash screen


## v0.0.0