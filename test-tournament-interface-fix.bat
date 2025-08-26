@echo off
echo ===== TESTING TOURNAMENT INTERFACE FIXES =====
echo.

echo ?? [TEST] Starting Tournament Hub server...
cd tournament-hub
start "Tournament Hub Server" cmd /c "node server.js & pause"
timeout /t 3 >nul

echo ?? [TEST] Starting DartTournamentPlaner.API...
cd ..\DartTournamentPlaner.API
start "Tournament Planner API" cmd /c "dotnet run --urls=http://localhost:5000 & pause"
timeout /t 5 >nul

echo ?? [TEST] Opening tournament interface for testing...
start http://localhost:3000/join

echo ?? [TEST] Opening browser console for debugging...
start http://localhost:3000/dashboard.html

echo.
echo ===== TOURNAMENT INTERFACE TEST INSTRUCTIONS =====
echo.
echo ?? Browser Console Tests:
echo 1. Öffnen Sie die Browser-Entwicklertools (F12)
echo 2. Prüfen Sie, ob "initializeSocket is not defined" Fehler verschwunden sind
echo 3. Schauen Sie nach Socket.IO Verbindungsnachrichten
echo 4. Testen Sie Match-Ergebnis-Eingaben
echo.
echo ?? Funktionalitätstests:
echo 1. Verbindung zu Tournament Hub
echo 2. Tournament-Daten laden
echo 3. Match-Cards anzeigen
echo 4. Klassen-Auswahl funktioniert
echo 5. Match-Ergebnis-Eingabe
echo 6. WebSocket vs. REST API Fallback
echo.
echo ?? Debug-Funktionen:
echo - Konsolen-Befehl: validateMatchData()
echo - Debug-Button in der UI
echo - Notification-System
echo.
echo ? Erwartete Korrekturen:
echo - ? "initializeSocket is not defined" Fehler behoben
echo - ? Socket.IO Verbindung funktioniert
echo - ? REST API Fallback verfügbar
echo - ? Match-Type spezifische Behandlung
echo - ? Erweiterte Validierung und Debugging
echo.
pause