@echo off
echo.
echo ========================================
echo   Match-Page Module Loading Debug Test
echo ========================================
echo.

echo ?? Testing Match-Page Module Loading...
cd /d "%~dp0"

REM Kill any existing node processes
taskkill /F /IM node.exe >nul 2>&1

REM Start Tournament Hub
start "Tournament Hub Server" /MIN cmd /c "node server.js"

REM Wait for server to start
echo Waiting for server to initialize...
timeout /t 5 >nul

echo.
echo ?? Testing Module Loading...

REM Test 1: Basic server health
echo.
echo ?? Test 1: API Health Check
curl -s "http://localhost:9443/api/health" | jq . 2>nul || echo ? Server not responding

REM Test 2: Check if all JS files are accessible
echo.
echo ?? Test 2: Checking Match-Page JavaScript Files
curl -s -I "http://localhost:9443/js/match-page-core.js" | findstr "200" >nul && echo ? match-page-core.js accessible || echo ? match-page-core.js not accessible
curl -s -I "http://localhost:9443/js/match-page-display.js" | findstr "200" >nul && echo ? match-page-display.js accessible || echo ? match-page-display.js not accessible
curl -s -I "http://localhost:9443/js/match-page-scoring.js" | findstr "200" >nul && echo ? match-page-scoring.js accessible || echo ? match-page-scoring.js not accessible
curl -s -I "http://localhost:9443/js/match-page-api.js" | findstr "200" >nul && echo ? match-page-api.js accessible || echo ? match-page-api.js not accessible
curl -s -I "http://localhost:9443/js/match-page-main.js" | findstr "200" >nul && echo ? match-page-main.js accessible || echo ? match-page-main.js not accessible

REM Test 3: Open match-page with debug parameters
echo.
echo ?? Test 3: Opening Match-Page with Debug Info
start "" "http://localhost:9443/match-page.html?tournament=DEBUG_TEST&match=DEBUG_MATCH"

echo.
echo ?? Debug Instructions:
echo 1. Press F12 in browser to open Developer Tools
echo 2. Check Console tab for module loading messages
echo 3. Look for any 404 errors in Network tab
echo 4. Verify all window.matchPage* objects exist
echo.
echo ?? Expected Console Output:
echo   - ?? [MATCH-CORE] Match Page Core initialized
echo   - ?? [MATCH-DISPLAY] Match Page Display initialized  
echo   - ?? [MATCH-SCORING] Match Page Scoring initialized
echo   - ?? [MATCH-API] Match Page API initialized
echo   - ?? [MATCH-MAIN] Match Page Main initialized
echo   - ? [MATCH-MAIN] All required modules loaded
echo.
echo Press any key to continue...
pause >nul