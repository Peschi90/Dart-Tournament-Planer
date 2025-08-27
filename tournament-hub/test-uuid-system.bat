@echo off
echo.
echo ========================================
echo    UUID-System Test Script
echo ========================================
echo.

echo ?? Starting Tournament Hub with UUID Support...
cd /d "%~dp0"

REM Start Tournament Hub
start "Tournament Hub" cmd /k "cd /d \"%~dp0\" && node server.js"

timeout /t 3 >nul

echo.
echo ?? Testing UUID System Components...

REM Test 1: API Health Check
echo.
echo ?? Test 1: API Health Check
curl -s -X GET "http://localhost:9443/api/health" | jq .

REM Test 2: Tournament List (should work now)
echo.
echo ?? Test 2: Tournament List
curl -s -X GET "http://localhost:9443/api/tournaments" | jq .

REM Test 3: WebSocket Info
echo.
echo ?? Test 3: WebSocket Information
curl -s -X GET "http://localhost:9443/api/websocket/info" | jq .

echo.
echo ?? UUID System Status Check Complete!
echo.
echo Next Steps:
echo 1. Start Dart Tournament Planner (.NET)
echo 2. Create a tournament with matches
echo 3. Register tournament with Hub
echo 4. Test match pages with UUIDs
echo.

pause