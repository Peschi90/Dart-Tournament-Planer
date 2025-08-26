# ?? Bidirectional Tournament Hub Communication

## ?? Overview
This document describes the bidirectional communication system between the Tournament Planner and Tournament Hub, allowing match results entered via the web interface to automatically update the Tournament Planner.

## ??? Architecture

```
Tournament Planner ?--? Tournament Hub ?--? Web Interface
     (WPF App)           (Node.js)        (Browser)
        ?                     ?               ?
   Match Updates      Result Storage    Result Input
   Auto-Refresh       Broadcasting      User Interface
```

## ?? Communication Flow

### 1. **Tournament Registration**
```
Tournament Planner ? Tournament Hub
- Register tournament with unique ID
- Establish communication channel
- Initialize polling for updates
```

### 2. **Match Synchronization** 
```
Tournament Planner ? Tournament Hub
- Sync all matches with complete data
- Include GameRules and class information
- Store matches in Hub registry
```

### 3. **Result Submission (Web Interface)**
```
User (Web) ? Tournament Hub ? Tournament Planner
- User enters result via web interface
- Hub stores result and broadcasts update
- Tournament Planner receives and applies update
```

### 4. **Automatic Updates**
```
Tournament Hub ? Tournament Planner
- Polling every 5 seconds for changes
- Process only recently updated matches
- Update local match data and UI
```

## ?? Implementation Details

### Tournament Planner Side

#### Enhanced TournamentHubService
```csharp
// Initialize bidirectional communication
await _tournamentHubService.InitializeWebSocketConnectionAsync(tournamentId);

// Subscribe to Hub updates
_tournamentHubService.OnMatchResultReceivedFromHub += OnHubMatchResultReceived;

// Process incoming updates
private void OnHubMatchResultReceived(HubMatchUpdateEventArgs e)
{
    // Find match in tournament classes
    // Update match with Hub data
    // Trigger UI refresh
    // Mark as changed for auto-save
}
```

#### Match Update Processing
```csharp
private bool UpdateMatchWithHubData(Match match, HubMatchUpdateEventArgs hubData)
{
    // Check for actual changes
    // Update sets, legs, status
    // Determine winner
    // Set end time if finished
    return hasChanges;
}
```

### Tournament Hub Side

#### Match Result Endpoints
```javascript
// Submit match result from web interface
POST /api/matches/:tournamentId/:matchId/result
{
  "player1Sets": 3,
  "player2Sets": 1,
  "player1Legs": 9,
  "player2Legs": 6,
  "status": "Finished",
  "notes": "Web interface submission"
}

// Get match details
GET /api/matches/:tournamentId/:matchId
```

#### Real-time Updates
```javascript
// Broadcast to Tournament Planner
io.emit('result-submitted-to-planner', {
  tournamentId: tournamentId,
  matchId: matchId,
  result: updatedMatch,
  classId: updatedMatch.classId,
  timestamp: new Date()
});

// Broadcast to web clients
io.to(`tournament_${tournamentId}`).emit('match-result-updated', {
  tournamentId: tournamentId,
  matchId: matchId,
  result: updatedMatch
});
```

### Web Interface Side

#### Enhanced Result Submission
```javascript
// Submit with game rules context
const result = {
    matchId: matchId,
    player1Sets: playWithSets ? p1Sets : 0,
    player1Legs: p1Legs,
    player2Sets: playWithSets ? p2Sets : 0,
    player2Legs: p2Legs,
    status: 'Finished',
    playWithSets: playWithSets,
    gameRulesUsed: {
        name: gameRule.name,
        playWithSets: playWithSets,
        setsToWin: gameRule.setsToWin,
        legsToWin: gameRule.legsToWin
    }
};

// Submit via WebSocket or REST API
socket.emit('submit-match-result', {
    tournamentId: tournamentId,
    matchId: matchId,
    result: result
});
```

## ?? Data Flow Diagram

```
???????????????????    ???????????????????    ???????????????????
? Tournament      ?????? Tournament Hub  ?????? Web Interface   ?
? Planner (WPF)   ?    ? (Node.js)       ?    ? (Browser)       ?
???????????????????    ???????????????????    ???????????????????
?• Match Storage  ?    ?• Result Storage ?    ?• Result Input   ?
?• UI Updates     ?    ?• Broadcasting   ?    ?• Real-time UI   ?
?• Auto-Save      ?    ?• Polling API    ?    ?• WebSocket      ?
?• Polling (5s)   ?    ?• WebSocket Hub  ?    ?• Validation     ?
???????????????????    ???????????????????    ???????????????????
         ?                       ?                       ?
   ???????????????        ???????????????        ???????????????
   ? Local Match ?        ? Hub Match   ?        ? User Input  ?
   ? Objects     ?        ? Registry    ?        ? Form        ?
   ???????????????        ???????????????        ???????????????
```

## ?? Update Scenarios

### Scenario 1: Web Result Entry
1. **User enters result** in web interface
2. **Web submits** via WebSocket/REST to Hub
3. **Hub stores result** in tournament registry
4. **Hub broadcasts** update to all clients
5. **Tournament Planner polls** Hub for updates
6. **Tournament Planner updates** local matches
7. **UI refreshes** automatically
8. **Auto-save triggers** if enabled

### Scenario 2: Tournament Planner Changes
1. **User modifies match** in Tournament Planner
2. **Tournament Planner syncs** to Hub via API
3. **Hub updates** match registry
4. **Web interface** receives real-time update
5. **Web UI refreshes** match display

## ?? Configuration

### Polling Interval
```csharp
// Tournament Planner - configurable polling interval
private readonly int _hubPollingIntervalMs = 5000; // 5 seconds
```

### Hub URL Configuration
```csharp
// Production Hub URL
HubUrl = "https://dtp.i3ull3t.de:9443";

// Local development
HubUrl = "http://localhost:3000";
```

### WebSocket Settings
```javascript
// Socket.IO configuration
const io = io({
    transports: ['websocket', 'polling'],
    timeout: 5000,
    reconnectionAttempts: 3
});
```

## ?? Testing

### Automated Tests
Use `test-bidirectional-hub.bat` to verify:
- ? Tournament registration and sync
- ? Web interface result entry
- ? Tournament Planner update reception
- ? UI refresh and auto-save triggering

### Manual Testing Checklist
1. ? Start Tournament Planner with Hub registration
2. ? Open web interface for tournament
3. ? Enter match result via web
4. ? Verify Tournament Planner updates automatically
5. ? Check UI refresh and change marking
6. ? Verify auto-save functionality

## ?? Benefits

1. **Real-time Synchronization**: Changes appear immediately
2. **User Flexibility**: Results can be entered from anywhere
3. **Data Integrity**: Single source of truth in Hub
4. **Automatic UI Updates**: No manual refresh needed
5. **Reliable Communication**: Polling ensures updates aren't missed
6. **Game Rules Aware**: Respects class-specific rules

## ?? Future Enhancements

- Replace polling with true WebSocket bidirectional communication
- Add conflict resolution for simultaneous edits
- Implement offline sync capabilities
- Add user authentication for result submissions
- Enhanced error handling and retry logic

## ?? API Reference

### Tournament Hub Endpoints
- `POST /api/tournaments/register` - Register tournament
- `POST /api/tournaments/:id/sync-full` - Full tournament sync
- `POST /api/matches/:tournamentId/:matchId/result` - Submit result
- `GET /api/matches/:tournamentId/:matchId` - Get match details
- `GET /api/tournaments/:id/matches` - Get all matches

### WebSocket Events
- `submit-match-result` - Submit result from web
- `match-result-updated` - Broadcast result update
- `result-submitted-to-planner` - Notify Tournament Planner
- `tournament-synced` - Tournament data synchronized

## ? Status: FULLY IMPLEMENTED

The bidirectional communication system is now complete and ready for testing!