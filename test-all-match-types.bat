@echo off
echo ===== TESTING ALL MATCH TYPES INTEGRATION =====
echo.

echo ?? [BUILD] Building DartTournamentPlaner with extended match types support...
dotnet build DartTournamentPlaner\DartTournamentPlaner.csproj --configuration Release --verbosity quiet
if %ERRORLEVEL% neq 0 (
    echo ? [ERROR] DartTournamentPlaner build failed!
    pause
    exit /b 1
)

echo ?? [BUILD] Building DartTournamentPlaner.API with enhanced hub services...
dotnet build DartTournamentPlaner.API\DartTournamentPlaner.API.csproj --configuration Release --verbosity quiet
if %ERRORLEVEL% neq 0 (
    echo ? [ERROR] DartTournamentPlaner.API build failed!
    pause
    exit /b 1
)

echo ? [BUILD] All projects built successfully!
echo.

echo ?? [TEST] Starting Tournament Hub server...
cd tournament-hub
start "Tournament Hub Server" cmd /c "node server.js & pause"
timeout /t 3 >nul

echo ?? [TEST] Starting DartTournamentPlaner.API...
cd ..\DartTournamentPlaner.API
start "Tournament Planner API" cmd /c "dotnet run --urls=http://localhost:5000 & pause"
timeout /t 5 >nul

echo ?? [TEST] Opening test interfaces...
start http://localhost:3000/dashboard.html
timeout /t 2 >nul
start http://localhost:5000/swagger
timeout /t 2 >nul

echo ?? [TEST] Opening browser for match type testing...
start http://localhost:3000/join

echo.
echo ===== ALL MATCH TYPES TEST SETUP COMPLETE =====
echo.
echo ?? Test Instructions:
echo 1. Use the Tournament Planner WPF application to create tournaments
echo 2. Create tournaments with different phases:
echo    - Group Phase matches (?? Gruppe)
echo    - Finals matches (?? Finalrunde)  
echo    - Winner Bracket matches (? K.O. Winner)
echo    - Loser Bracket matches (?? K.O. Loser)
echo.
echo 3. Test match result submissions for each match type
echo 4. Verify proper display and processing of different tournament phases
echo.
echo Open browser tabs:
echo - Dashboard: http://localhost:3000/dashboard.html
echo - API Docs: http://localhost:5000/swagger
echo - Join Interface: http://localhost:3000/join
echo.
echo ? All match types (Group, Finals, Winner Bracket, Loser Bracket) are now supported!
echo.
pause