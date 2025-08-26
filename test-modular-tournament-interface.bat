@echo off
echo ===== MODULARE TOURNAMENT INTERFACE GEFIXT =====
echo.

echo ?? [MODULAR] Starting modular Tournament Interface...
cd tournament-hub
start "Tournament Hub Server" cmd /c "node server.js & pause"
timeout /t 3 >nul

echo ?? [MODULAR] Starting Tournament Planner API...
cd ..\DartTournamentPlaner.API
start "Tournament Planner API" cmd /c "dotnet run --urls=http://localhost:5000 & pause"
timeout /t 5 >nul

echo ?? [MODULAR] Opening modular tournament interface...
start http://localhost:3000/tournament/TOURNAMENT_TEST_MODULAR

echo ?? [MODULAR] Opening browser console for testing...
timeout /t 2 >nul
start http://localhost:3000/dashboard.html

echo.
echo ===== MODULARISIERUNG ERFOLGREICH =====
echo.
echo ? HTML-Datei entschlackt:
echo    - Nur noch CSS und HTML-Struktur
echo    - JavaScript in separate Dateien ausgelagert
echo    - Übersichtliche Dateigröße
echo.
echo ??? JavaScript Module:
echo    /js/tournament-interface-core.js       - Core-Funktionen
echo    /js/tournament-interface-display.js    - UI-Display-Logik
echo    /js/tournament-interface-debug.js      - Debug-Tools
echo    /js/tournament-interface-match-card.js - Match-Card-Generation
echo    /js/tournament-interface-api.js        - API & WebSocket
echo    /js/tournament-interface-main.js       - Hauptinitialisierung
echo.
echo ?? Alle Funktionen wiederhergestellt:
echo    ? Socket.IO Integration
echo    ? REST API Fallback
echo    ? Tournament-Info Updates
echo    ? Match-Anzeige mit Validierung
echo    ? Class-Selector
echo    ? Match-Card-Generation
echo    ? Result-Submission
echo    ? Debug-Tools
echo    ? Validation-System
echo    ? Notification-System
echo.
echo ?? Browser Console Debug Commands:
echo    debugTournament()     - Umfassende Diagnose
echo    testApis()           - API Endpoint Tests
echo    reloadData()         - Tournament-Daten neu laden
echo    showState()          - Aktueller Zustand
echo    validateMatchData()  - Datenvalidierung
echo.
echo ?? Vorteile der Modularisierung:
echo    - Wartbarer und übersichtlicher Code
echo    - Keine Probleme beim Bearbeiten großer HTML-Dateien
echo    - Bessere Trennung der Funktionalitäten
echo    - Einfachere Entwicklung und Debugging
echo.
pause