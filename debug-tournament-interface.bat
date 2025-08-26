@echo off
echo ===== TOURNAMENT INTERFACE DEBUG & DIAGNOSE =====
echo.

echo ?? [DEBUG] Starting Tournament Hub server with enhanced logging...
cd tournament-hub
start "Tournament Hub Server" cmd /c "echo Starting Tournament Hub Server... && node server.js"
timeout /t 3 >nul

echo ?? [DEBUG] Starting Tournament Planner API with verbose logging...
cd ..\DartTournamentPlaner.API
start "Tournament Planner API" cmd /c "echo Starting Tournament Planner API... && dotnet run --urls=http://localhost:5000"
timeout /t 5 >nul

echo ?? [DEBUG] Opening tournament interface with debug tools...
start http://localhost:3000/tournament/TOURNAMENT_TEST_DEBUG

echo ?? [DEBUG] Opening browser console for debugging...
timeout /t 2 >nul
start http://localhost:3000/dashboard.html

echo.
echo ===== DEBUG INSTRUCTIONS =====
echo.
echo ?? Browser Console Debug Commands:
echo    debugTournament()     - Comprehensive tournament state debug
echo    testApis()           - Test all API endpoints
echo    reloadData()         - Reload tournament data
echo    reloadMatches()      - Reload matches only
echo    showState()          - Show current state summary
echo    validateMatchData()  - Validate match data integrity
echo.
echo ?? Debug Steps:
echo 1. Open Browser Developer Tools (F12)
echo 2. Go to Console tab
echo 3. Run: debugTournament()
echo 4. Check for errors in the console output
echo 5. Run: testApis() to test API connectivity
echo 6. Check Network tab for failed requests
echo.
echo ?? Look for these issues:
echo    - API endpoint errors (404, 500, etc.)
echo    - JSON parsing errors
echo    - Missing DOM elements
echo    - Socket.IO connection problems
echo    - Empty or malformed tournament data
echo.
echo ?? Check Server Logs:
echo    - Tournament Hub Server window for Node.js logs
echo    - Tournament Planner API window for .NET logs
echo    - Look for tournament synchronization messages
echo.
echo ?? Common Issues:
echo    - Tournament not synchronized from WPF app
echo    - API endpoints returning empty data
echo    - CORS issues blocking requests
echo    - Tournament ID mismatch
echo    - Missing tournament classes or game rules
echo.
echo ?? If issues persist:
echo    1. Check if Tournament Planner WPF is running
echo    2. Verify tournament is synced to hub
echo    3. Check tournament ID in URL matches synced tournament
echo    4. Restart both servers and try again
echo.
pause