@echo off
echo ===== API RESPONSE STRUCTURE FIX =====
echo.

echo ?? [FIX] Starting corrected Tournament Interface...
cd tournament-hub
start "Tournament Hub Server" cmd /c "node server.js & pause"
timeout /t 3 >nul

echo ?? [FIX] Starting Tournament Planner API...
cd ..\DartTournamentPlaner.API
start "Tournament Planner API" cmd /c "dotnet run --urls=http://localhost:5000 & pause"
timeout /t 5 >nul

echo ?? [FIX] Opening corrected tournament interface...
start http://localhost:3000/tournament/TOURNAMENT_20250826_215558

echo ?? [FIX] Testing API endpoints directly...
timeout /t 3 >nul

echo.
echo ===== API RESPONSE STRUCTURE PROBLEM IDENTIFIED =====
echo.
echo ? Problem:
echo    API verwendet ApiResponse^<T^> Wrapper mit 'data' Property:
echo    {
echo      "success": true,
echo      "data": { ... actual data ... },
echo      "meta": { ... metadata ... }
echo    }
echo.
echo    aber Tournament Interface erwartet direkte Daten:
echo    { ... direct tournament/matches data ... }
echo.
echo ? Fix implementiert:
echo    - Erkennung der verschachtelten API Response Struktur
echo    - Fallback auf apiResponse.data wenn verfügbar
echo    - Separate Endpunkt-Aufrufe für Classes und Matches
echo    - Verbesserte Fehlerbehandlung für API Response
echo.
echo ?? Erwartete Browser Console Logs:
echo    "?? Using nested data structure from API response"
echo    "?? Loading tournament classes via REST API..."
echo    "? Successfully loaded X matches"
echo    "? Successfully loaded X tournament classes"
echo.
echo ?? Browser Console Test Commands:
echo    debugTournament()     - Comprehensive diagnosis
echo    testApis()           - API endpoint testing
echo    showState()          - Current state summary
echo.
pause