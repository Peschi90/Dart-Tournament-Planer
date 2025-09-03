/**
 * Match State API Routes
 * Provides server-side caching for dart scoring states
 */
const express = require('express');
const router = express.Router();
const fs = require('fs').promises;
const path = require('path');

// In-Memory Cache für bessere Performance
const matchStateCache = new Map();

// Stelle sicher, dass das Data-Verzeichnis existiert
const DATA_DIR = path.join(__dirname, '..', 'data', 'match-states');

async function ensureDataDir() {
    try {
        await fs.mkdir(DATA_DIR, { recursive: true });
    } catch (error) {
        console.error('❌ [MATCH-STATE] Failed to create data directory:', error);
    }
}

// Initialisiere Data-Verzeichnis beim Start
ensureDataDir();

/**
 * Hilfsfunktionen für File-basierte Persistierung
 */
function getStateFilePath(tournamentId, matchId) {
    return path.join(DATA_DIR, `${tournamentId}_${matchId}.json`);
}

async function saveStateToDisk(tournamentId, matchId, gameState) {
    try {
        const filePath = getStateFilePath(tournamentId, matchId);
        const stateData = {
            ...gameState,
            savedToDisk: new Date().toISOString()
        };

        await fs.writeFile(filePath, JSON.stringify(stateData, null, 2));
        console.log(`💾 [MATCH-STATE] Saved to disk: ${filePath}`);
        return true;
    } catch (error) {
        console.error('❌ [MATCH-STATE] Failed to save to disk:', error);
        return false;
    }
}

async function loadStateFromDisk(tournamentId, matchId) {
    try {
        const filePath = getStateFilePath(tournamentId, matchId);
        const data = await fs.readFile(filePath, 'utf8');
        const gameState = JSON.parse(data);
        
        // Prüfe Alter der Datei
        const maxAge = 7 * 24 * 60 * 60 * 1000; // 7 Tage
        const stateAge = Date.now() - new Date(gameState.savedToDisk || gameState.lastUpdated).getTime();
        
        if (stateAge > maxAge) {
            console.log(`🕒 [MATCH-STATE] State too old (${Math.floor(stateAge / (24 * 60 * 60 * 1000))} days), deleting`);
            await deleteStateFromDisk(tournamentId, matchId);
            return null;
        }
        
        console.log(`📥 [MATCH-STATE] Loaded from disk: ${filePath}`);
        return gameState;
    } catch (error) {
        if (error.code !== 'ENOENT') {
            console.error('❌ [MATCH-STATE] Failed to load from disk:', error);
        }
        return null;
    }
}

async function deleteStateFromDisk(tournamentId, matchId) {
    try {
        const filePath = getStateFilePath(tournamentId, matchId);
        await fs.unlink(filePath);
        console.log(`🗑️ [MATCH-STATE] Deleted from disk: ${filePath}`);
        return true;
    } catch (error) {
        if (error.code !== 'ENOENT') {
            console.error('❌ [MATCH-STATE] Failed to delete from disk:', error);
        }
        return false;
    }
}

/**
 * POST /api/match-state/:tournamentId/:matchId/save
 * Speichere den aktuellen Spielstand
 */
router.post('/:tournamentId/:matchId/save', async (req, res) => {
    try {
        const { tournamentId, matchId } = req.params;
        const gameState = req.body;

        console.log(`💾 [MATCH-STATE] Saving state for ${tournamentId}/${matchId}`);

        // Validierung der eingehenden Daten
        if (!gameState || typeof gameState !== 'object') {
            return res.status(400).json({
                success: false,
                message: 'Invalid game state data'
            });
        }

        const cacheKey = `${tournamentId}_${matchId}`;
        
        // Füge Metadata hinzu
        const stateData = {
            ...gameState,
            lastUpdated: new Date().toISOString(),
            tournamentId,
            matchId,
            version: '1.0.0',
            autoSaved: true
        };

        // Speichere in Memory-Cache
        matchStateCache.set(cacheKey, stateData);

        // Speichere auch auf Festplatte für Persistierung
        const diskSaved = await saveStateToDisk(tournamentId, matchId, stateData);

        console.log(`💾 [MATCH-STATE] State saved:`, {
            memoryCache: true,
            diskCache: diskSaved,
            currentPlayer: gameState.gameState?.currentPlayer,
            currentLeg: gameState.gameState?.currentLeg,
            player1Score: gameState.gameState?.player1?.score,
            player2Score: gameState.gameState?.player2?.score,
            throwHistoryLength: gameState.gameState?.throwHistory?.length || 0
        });

        res.json({
            success: true,
            message: 'Match state saved successfully',
            lastUpdated: stateData.lastUpdated,
            savedTo: {
                memory: true,
                disk: diskSaved
            }
        });

    } catch (error) {
        console.error('❌ [MATCH-STATE] Error saving state:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to save match state',
            error: error.message
        });
    }
});

/**
 * GET /api/match-state/:tournamentId/:matchId/load
 * Lade den gespeicherten Spielstand
 */
router.get('/:tournamentId/:matchId/load', async (req, res) => {
    try {
        const { tournamentId, matchId } = req.params;
        const cacheKey = `${tournamentId}_${matchId}`;

        console.log(`📥 [MATCH-STATE] Loading state for ${tournamentId}/${matchId}`);

        // Prüfe zuerst Memory-Cache
        let cachedState = matchStateCache.get(cacheKey);

        // Falls nicht im Memory-Cache, prüfe Festplatte
        if (!cachedState) {
            cachedState = await loadStateFromDisk(tournamentId, matchId);
            
            // Falls von Festplatte geladen, in Memory-Cache speichern
            if (cachedState) {
                matchStateCache.set(cacheKey, cachedState);
                console.log(`📂 [MATCH-STATE] Restored from disk to memory cache`);
            }
        }

        if (!cachedState) {
            console.log(`📭 [MATCH-STATE] No cached state found for ${tournamentId}/${matchId}`);
            return res.json({
                success: false,
                message: 'No cached state found',
                hasState: false
            });
        }

        // Prüfe Alter des States
        const maxAge = 24 * 60 * 60 * 1000; // 24 Stunden für aktive States
        const stateAge = Date.now() - new Date(cachedState.lastUpdated).getTime();

        if (stateAge > maxAge) {
            console.log(`🕒 [MATCH-STATE] State expired (${Math.floor(stateAge / (60 * 60 * 1000))} hours old)`);
            
            // Lösche abgelaufenen State
            matchStateCache.delete(cacheKey);
            await deleteStateFromDisk(tournamentId, matchId);
            
            return res.json({
                success: false,
                message: 'Cached state expired',
                hasState: false
            });
        }

        console.log(`📥 [MATCH-STATE] State loaded successfully:`, {
            age: Math.floor(stateAge / (60 * 1000)), // Minuten
            currentPlayer: cachedState.gameState?.currentPlayer,
            currentLeg: cachedState.gameState?.currentLeg,
            throwHistoryLength: cachedState.gameState?.throwHistory?.length || 0
        });

        res.json({
            success: true,
            message: 'Match state loaded successfully',
            hasState: true,
            gameState: cachedState,
            lastUpdated: cachedState.lastUpdated,
            age: stateAge
        });

    } catch (error) {
        console.error('❌ [MATCH-STATE] Error loading state:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to load match state',
            error: error.message
        });
    }
});

/**
 * GET /api/match-state/:tournamentId/:matchId/check
 * Prüfe ob ein gespeicherter Spielstand existiert (ohne ihn zu laden)
 */
router.get('/:tournamentId/:matchId/check', async (req, res) => {
    try {
        const { tournamentId, matchId } = req.params;
        const cacheKey = `${tournamentId}_${matchId}`;

        // Prüfe Memory-Cache
        let hasMemoryState = matchStateCache.has(cacheKey);
        let hasDiskState = false;

        // Prüfe Festplatte
        if (!hasMemoryState) {
            try {
                const filePath = getStateFilePath(tournamentId, matchId);
                await fs.access(filePath);
                hasDiskState = true;
            } catch {
                hasDiskState = false;
            }
        }

        const hasState = hasMemoryState || hasDiskState;

        console.log(`🔍 [MATCH-STATE] State check for ${tournamentId}/${matchId}:`, {
            memory: hasMemoryState,
            disk: hasDiskState,
            hasState
        });

        res.json({
            success: true,
            hasState,
            sources: {
                memory: hasMemoryState,
                disk: hasDiskState
            }
        });

    } catch (error) {
        console.error('❌ [MATCH-STATE] Error checking state:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to check match state',
            error: error.message
        });
    }
});

/**
 * DELETE /api/match-state/:tournamentId/:matchId/clear
 * Lösche den gespeicherten Spielstand (nach Match-Ende)
 */
router.delete('/:tournamentId/:matchId/clear', async (req, res) => {
    try {
        const { tournamentId, matchId } = req.params;
        const cacheKey = `${tournamentId}_${matchId}`;

        console.log(`🗑️ [MATCH-STATE] Clearing state for ${tournamentId}/${matchId}`);

        // Lösche aus Memory-Cache
        const memoryDeleted = matchStateCache.delete(cacheKey);

        // Lösche von Festplatte
        const diskDeleted = await deleteStateFromDisk(tournamentId, matchId);

        console.log(`🗑️ [MATCH-STATE] State cleared:`, {
            memory: memoryDeleted,
            disk: diskDeleted
        });

        res.json({
            success: true,
            message: 'Match state cleared successfully',
            clearedFrom: {
                memory: memoryDeleted,
                disk: diskDeleted
            }
        });

    } catch (error) {
        console.error('❌ [MATCH-STATE] Error clearing state:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to clear match state',
            error: error.message
        });
    }
});

/**
 * GET /api/match-state/stats
 * Statistiken über gespeicherte States (für Debugging)
 */
router.get('/stats', (req, res) => {
    try {
        const memoryCacheSize = matchStateCache.size;
        const memoryCacheKeys = Array.from(matchStateCache.keys());

        console.log(`📊 [MATCH-STATE] Cache stats requested`);

        res.json({
            success: true,
            stats: {
                memoryCacheSize,
                memoryCacheKeys,
                dataDirectory: DATA_DIR
            }
        });

    } catch (error) {
        console.error('❌ [MATCH-STATE] Error getting stats:', error);
        res.status(500).json({
            success: false,
            message: 'Failed to get cache stats',
            error: error.message
        });
    }
});

module.exports = router;
