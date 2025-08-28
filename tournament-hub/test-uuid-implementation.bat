@echo off
echo.
echo ========================================
echo   UUID-System Implementation Test
echo ========================================
echo.

echo ?? Testing UUID System Implementation...
cd /d "%~dp0"

REM Kill any existing node processes
taskkill /F /IM node.exe >nul 2>&1

REM Start Tournament Hub
start "Tournament Hub Server" /MIN cmd /c "node server.js"

REM Wait for server to start
echo Waiting for server to initialize...
timeout /t 5 >nul

echo.
echo ?? Testing UUID System...

REM Test 1: API Health Check
echo.
echo ?? Test 1: API Health Check
curl -s "http://localhost:9443/api/health" | jq . 2>nul || echo ? Server not responding

REM Test 2: Open UUID Test Interface
echo.
echo ?? Test 2: Opening UUID Test Interface
start "" "http://localhost:9443/uuid-test.html"

echo.
echo ========================================
echo   UUID System Test Complete!
echo ========================================
echo.
echo ?? Was das UUID-System macht:
echo.
echo 1. ?? UUID-Generierung:
echo    - Jedes Match bekommt automatisch eine eindeutige UUID
echo    - Format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
echo    - UUIDs sind global eindeutig (keine Duplikate möglich)
echo.
echo 2. ?? Dart Planer (.NET):
echo    - Match.UniqueId Property hinzugefügt
echo    - KnockoutMatch.UniqueId Property hinzugefügt
echo    - Automatische UUID-Generierung bei Match-Erstellung
echo    - Hub-Integration sendet UUIDs mit allen Matches
echo.
echo 3. ?? Tournament Hub (Node.js):
echo    - UUID-aware Match-Suche (UUID hat Priorität)
echo    - Fallback auf numerische IDs für Kompatibilität
echo    - API-Endpoints unterstützen beide ID-Typen
echo    - Match-Pages können mit UUID oder numerischer ID aufgerufen werden
echo.
echo 4. ?? Web Interface:
echo    - Match-Seiten verwenden bevorzugt UUIDs
echo    - URLs: /match-page.html?tournament=ID^&match=UUID
echo    - Eindeutige Identifikation auch bei vielen Matches
echo.
echo ?? Nächste Schritte:
echo 1. Dart Tournament Planer starten
echo 2. Tournament erstellen mit Matches
echo 3. Tournament an Hub senden
echo 4. Im UUID-Test-Interface prüfen ob UUIDs vorhanden sind
echo 5. Match-Seiten mit UUIDs öffnen
echo.
echo ?? Erwartete JSON-Struktur (nach Hub-Sync):
echo   "matches": [
echo     {
echo       "id": 1,
echo       "matchId": 1,
echo       "uniqueId": "550e8400-e29b-41d4-a716-446655440000",
echo       "player1": "Player A",
echo       "player2": "Player B",
echo       ...
echo     }
echo   ]
echo.
echo Press any key to continue...
pause >nul