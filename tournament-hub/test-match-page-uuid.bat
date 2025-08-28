@echo off
echo.
echo ========================================
echo   UUID Match-Page Test System
echo ========================================
echo.

echo ?? Testing Match-Page with UUID Support...
cd /d "%~dp0"

REM Kill any existing node processes
taskkill /F /IM node.exe >nul 2>&1

REM Start Tournament Hub
start "Tournament Hub Server" /MIN cmd /c "node server.js"

REM Wait for server to start
echo Waiting for server to initialize...
timeout /t 5 >nul

echo.
echo ?? Testing Match-Page UUID Features...

REM Test 1: Basic functionality
echo.
echo ?? Test 1: API Health Check
curl -s "http://localhost:9443/api/health" | jq . 2>nul || echo ? Server not responding

REM Test 2: Open main interface
echo.
echo ?? Test 2: Opening Enhanced Tournament Interface
start "" "http://localhost:9443/"

REM Test 3: Open UUID test interface
echo.
echo ?? Test 3: Opening UUID Test Interface
start "" "http://localhost:9443/uuid-test.html"

echo.
echo ========================================
echo   UUID Match-Page Test Started!
echo ========================================
echo.
echo ?? Test Features:
echo - Dashboard: http://localhost:9443/
echo - UUID Test Interface: http://localhost:9443/uuid-test.html  
echo - Match-Page mit UUID: http://localhost:9443/match-page.html?tournament=ID&match=UUID
echo - Match-Page mit Numeric: http://localhost:9443/match-page.html?tournament=ID&match=123
echo.
echo ?? Test Steps:
echo 1. Start Dart Tournament Planer (.NET) und erstelle Tournament
echo 2. Registriere Tournament mit Hub (bekommt UUIDs für alle Matches)
echo 3. Im Tournament Interface: Klicke "?? Match-Seite öffnen" Button
echo 4. Teste sowohl UUID als auch numerische IDs
echo 5. Verifiziere Match-Result-Submissions funktionieren
echo.
echo ?? URL-Formate die unterstützt werden:
echo   - /match-page.html?tournament=TOURNAMENT_ID^&match=UUID
echo   - /match-page.html?tournament=TOURNAMENT_ID^&match=123
echo   - /match-page.html?tournamentId=ID^&matchId=UUID
echo.
echo ?? UUID vs Numeric ID Testing:
echo   - Beide Formate sollten zur selben Match-Seite führen
echo   - UUID wird bevorzugt wenn verfügbar
echo   - Auto-Refresh nach Result-Submit sollte funktionieren
echo.
echo Press any key to continue...
pause >nul