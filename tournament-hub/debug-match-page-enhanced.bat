@echo off
echo.
echo ========================================
echo   Enhanced Match-Page Debug Test
echo ========================================
echo.

echo ?? Starting Enhanced Match-Page Debug...
cd /d "%~dp0"

REM Kill any existing node processes
taskkill /F /IM node.exe >nul 2>&1

REM Start Tournament Hub
start "Tournament Hub Server" /MIN cmd /c "node server.js"

REM Wait for server to start
echo Waiting for server to initialize...
timeout /t 5 >nul

echo.
echo ?? Testing Enhanced Match-Page System...

REM Test 1: Basic server health
echo.
echo ?? Test 1: API Health Check
curl -s "http://localhost:9443/api/health" 2>nul && echo ? API responding || echo ? API not responding

REM Test 2: Check JavaScript files
echo.
echo ?? Test 2: Checking JavaScript Module Availability
echo Core Module:
curl -s -I "http://localhost:9443/js/match-page-core.js" | findstr "200" >nul && echo ? match-page-core.js OK || echo ? match-page-core.js missing
echo Display Module:  
curl -s -I "http://localhost:9443/js/match-page-display.js" | findstr "200" >nul && echo ? match-page-display.js OK || echo ? match-page-display.js missing
echo Scoring Module:
curl -s -I "http://localhost:9443/js/match-page-scoring.js" | findstr "200" >nul && echo ? match-page-scoring.js OK || echo ? match-page-scoring.js missing
echo API Module:
curl -s -I "http://localhost:9443/js/match-page-api.js" | findstr "200" >nul && echo ? match-page-api.js OK || echo ? match-page-api.js missing
echo Main Module:
curl -s -I "http://localhost:9443/js/match-page-main.js" | findstr "200" >nul && echo ? match-page-main.js OK || echo ? match-page-main.js missing

REM Test 3: Check match-page.html
echo.
echo ?? Test 3: Checking Match-Page HTML
curl -s -I "http://localhost:9443/match-page.html" | findstr "200" >nul && echo ? match-page.html accessible || echo ? match-page.html not accessible

REM Test 4: Check Socket.IO
echo.
echo ?? Test 4: Checking Socket.IO
curl -s -I "http://localhost:9443/socket.io/socket.io.js" | findstr "200" >nul && echo ? Socket.IO accessible || echo ? Socket.IO not accessible

REM Test 5: Open test match page
echo.
echo ?? Test 5: Opening Test Match-Page
start "" "http://localhost:9443/match-page.html?tournament=DEBUG_TEST&match=UUID_TEST_123"

echo.
echo ========================================
echo   Enhanced Debug Test Complete!
echo ========================================
echo.
echo ?? What to check in Browser Developer Tools:
echo.
echo 1. Console Tab - Expected Messages:
echo    ? ?? [MATCH-CORE] Match Page Core initialized
echo    ? ?? [MATCH-DISPLAY] Match Page Display initialized
echo    ? ?? [MATCH-SCORING] Match Page Scoring initialized  
echo    ? ?? [MATCH-API] Match Page API initialized
echo    ? ?? [MATCH-MAIN] Match Page Main initialized
echo    ? ? [MATCH-MAIN] All dependencies are ready
echo    ? ?? [MATCH-MAIN] Match page initialized successfully
echo.
echo 2. Network Tab - Check for 404 errors
echo    All JavaScript files should load successfully
echo.
echo 3. Console Commands to Test:
echo    console.log('Testing modules:', {
echo      core: !!window.matchPageCore,
echo      display: !!window.matchPageDisplay, 
echo      scoring: !!window.matchPageScoring,
echo      api: !!window.matchPageAPI,
echo      main: !!window.matchPageMain
echo    });
echo.
echo 4. Expected Behavior:
echo    - Page should show "Lade Match-Daten..." initially
echo    - Should attempt to load match data via API
echo    - Should show connection status
echo    - No "Required modules not loaded" error
echo.
echo ?? If still getting "Missing required modules" error:
echo    - Check that all 5 JavaScript files load without 404 errors
echo    - Verify each creates its window.matchPage* global variable
echo    - Try hard refresh (Ctrl+F5) to clear cache
echo.
echo Press any key to continue...
pause >nul