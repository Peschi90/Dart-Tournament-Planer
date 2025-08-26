@echo off
echo ===== MATCH-TYPE SPECIFIC GAME RULES ENHANCEMENT =====
echo.

echo ?? [GAME_RULES] Starting enhanced Tournament Interface with match-specific game rules...
cd tournament-hub
start "Tournament Hub Server" cmd /c "node server.js & pause"
timeout /t 3 >nul

echo ?? [GAME_RULES] Starting Tournament Planner API...
cd ..\DartTournamentPlaner.API
start "Tournament Planner API" cmd /c "dotnet run --urls=http://localhost:5000 & pause"
timeout /t 5 >nul

echo ?? [GAME_RULES] Opening enhanced tournament interface...
start http://localhost:3000/tournament/TOURNAMENT_20250826_215558

echo ?? [GAME_RULES] Testing enhanced game rules...
timeout /t 3 >nul

echo.
echo ===== MATCH-TYPE SPECIFIC GAME RULES ENHANCEMENT =====
echo.
echo ? Implementierte Verbesserungen:
echo.
echo ?? Match-spezifische Game Rules Erkennung:
echo    - Direkte Match Game Rules (höchste Priorität)
echo    - Rundenspezifische Regeln für KO-Phasen
echo    - Winner vs Loser Bracket unterschiedliche Regeln
echo    - Finals-spezifische Regeln
echo    - Intelligente Standard-Regeln basierend auf Match-Type
echo.
echo ?? KO-Phase spezifische Regeln:
echo    - Knockout-WB-Best64: 2 Sets, 3 Legs (schnell)
echo    - Knockout-WB-Best32: 2 Sets, 3 Legs
echo    - Knockout-WB-Best16: 3 Sets, 3 Legs
echo    - Knockout-WB-Quarterfinal: 3 Sets, 3 Legs
echo    - Knockout-WB-Semifinal: 3 Sets, 4 Legs (länger)
echo    - Knockout-WB-Final: 4 Sets, 4 Legs (noch länger)
echo    - Knockout-WB-GrandFinal: 5 Sets, 5 Legs (längste)
echo.
echo ?? Loser Bracket Regeln:
echo    - Knockout-LB-*: Generell kürzere Spiele (2 Sets, 3 Legs)
echo    - Knockout-LB-LoserFinal: 3 Sets, 4 Legs (wichtiges Spiel)
echo.
echo ?? Finals und Gruppen:
echo    - Group: Standard 3 Sets, 3 Legs
echo    - Finals: Standard 3 Sets, 3 Legs (Round Robin)
echo.
echo ?? Visuelle Verbesserungen:
echo    - Match-Type spezifische Farben in Game Rules Display
echo    - Grün für Winner Bracket
echo    - Orange für Loser Bracket
echo    - Gelb für Finals
echo    - Blau für Gruppen
echo.
echo ?? Erweiterte Validierung:
echo    - Game Rules spezifische Input-Validierung
echo    - KO-Spiele müssen eindeutigen Gewinner haben
echo    - Sets/Legs Limits basierend auf aktuellen Regeln
echo    - Match-Type spezifische Validation Rules
echo.
echo ?? Browser Console Debug Commands:
echo    debugTournament()     - Comprehensive diagnosis
echo    testApis()           - API endpoint testing
echo    validateMatchData()  - Enhanced match data validation
echo.
echo ?? Erwartete Verbesserungen:
echo    - Jede Match-Card zeigt spezifische Game Rules
echo    - KO-Runden haben angemessene Spieldauer
echo    - Finals sind länger als frühe KO-Runden
echo    - Loser Bracket ist schneller als Winner Bracket
echo    - Validation passt sich an aktuelle Regeln an
echo.
pause