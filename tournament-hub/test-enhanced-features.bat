@echo off
echo.
echo ========================================
echo   Match-Page und Auto-Refresh Test
echo ========================================
echo.

echo ?? Starting Enhanced Tournament Hub with UUID and Auto-Refresh...
cd /d "%~dp0"

REM Kill any existing node processes
taskkill /F /IM node.exe >nul 2>&1

REM Start Tournament Hub
start "Tournament Hub Server" /MIN cmd /c "node server.js"

REM Wait for server to start
echo Waiting for server to initialize...
timeout /t 5 >nul

echo.
echo ?? Testing Enhanced Features...

REM Test 1: Basic functionality
echo.
echo ?? Test 1: Basic API Health
curl -s "http://localhost:9443/api/health" | jq . 2>nul || echo ? Server not responding

REM Test 2: Tournament interface with UUID support
echo.
echo ?? Test 2: Opening Enhanced Tournament Interface
start "" "http://localhost:9443/uuid-test.html"

REM Test 3: Dashboard for tournament management
echo.
echo ?? Test 3: Opening Dashboard
start "" "http://localhost:9443/"

echo.
echo ========================================
echo   Enhanced System Test Started!
echo ========================================
echo.
echo ?? Test Features:
echo - UUID System Test: http://localhost:9443/uuid-test.html
echo - Dashboard: http://localhost:9443
echo - Match-Page öffnen: Button in Match-Cards
echo - Auto-Refresh: Nach Match-Result-Submit
echo.
echo ?? Test Steps:
echo 1. Start Dart Tournament Planer (.NET)
echo 2. Create a tournament with matches
echo 3. Register tournament with Hub
echo 4. Test match pages from tournament interface
echo 5. Submit match results and verify auto-refresh
echo 6. Test UUID vs numeric ID compatibility
echo.
echo Press any key to continue...
pause >nul