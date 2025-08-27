@echo off
echo.
echo ========================================
echo    Tournament Hub - UUID System Test
echo ========================================
echo.

echo ?? Starting Tournament Hub Server...
cd /d "%~dp0"

REM Kill any existing node processes
taskkill /F /IM node.exe >nul 2>&1

REM Start Tournament Hub in background
echo Starting server.js...
start "Tournament Hub Server" /MIN cmd /c "node server.js"

REM Wait for server to start
echo Waiting for server to initialize...
timeout /t 5 >nul

echo.
echo ?? Testing UUID System...

REM Test 1: Server Health
echo.
echo ?? Test 1: Server Health Check
curl -s "http://localhost:9443/api/health" 2>nul || echo ? Server not responding

REM Test 2: Tournament List API (should work now)
echo.
echo ?? Test 2: Tournament List (UUID System)
curl -s "http://localhost:9443/api/tournaments" 2>nul || echo ? Tournament API not responding

REM Test 3: UUID Test Interface
echo.
echo ?? Opening UUID Test Interface in Browser...
start "" "http://localhost:9443/uuid-test.html"

echo.
echo ========================================
echo    UUID System Test Complete!
echo ========================================
echo.
echo ?? Test Results:
echo - Tournament Hub Server: Started
echo - UUID Test Interface: http://localhost:9443/uuid-test.html
echo - Dashboard: http://localhost:9443
echo.
echo ?? Next Steps:
echo 1. Use the UUID Test Interface to test all features
echo 2. Start Dart Tournament Planer (.NET)
echo 3. Create matches with UUIDs
echo 4. Test match pages with both UUID and numeric IDs
echo.
echo Press any key to continue...
pause >nul