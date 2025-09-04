/**
 * Dart Scoring Cache Manager
 * Handles automatic saving and loading of game states
 */
class DartScoringCache {
    constructor(core) {
        this.core = core;
        this.isEnabled = true;
        this.autoSaveInterval = 10000; // 10 Sekunden
        this.saveOnThrowDelay = 2000; // 2 Sekunden nach Wurf

        this.autoSaveTimer = null;
        this.saveOnThrowTimer = null;
        this.lastSaveState = null;
        this.hasCachedState = false;

        this.saveInProgress = false;
        this.lastSaveTime = null;

        console.log('💾 [DART-CACHE] Cache Manager initialized');
    }

    /**
     * 🔍 Prüfe ob ein gespeicherter State existiert
     */
    async checkForCachedState() {
        if (!this.core.matchData) return false;

        try {
            const { tournamentId, uniqueId, matchId } = this.core.matchData;
            const actualMatchId = uniqueId || matchId;

            const response = await fetch(`/api/match-state/${tournamentId}/${actualMatchId}/check`);
            const result = await response.json();

            this.hasCachedState = result.success && result.hasState;

            console.log(`🔍 [DART-CACHE] Cache check:`, {
                tournamentId,
                matchId: actualMatchId,
                hasState: this.hasCachedState,
                sources: result.sources
            });

            return this.hasCachedState;

        } catch (error) {
            console.warn('⚠️ [DART-CACHE] Failed to check cache:', error);
            return false;
        }
    }

    /**
     * 📥 Automatisches Laden beim Start
     */
    async autoLoadOnStart() {
        console.log('🚀 [DART-CACHE] Starting auto-load process...');

        try {
            const hasCached = await this.checkForCachedState();

            if (!hasCached) {
                console.log('📭 [DART-CACHE] No cached state found - starting fresh');
                this.startAutoSave();
                return { loaded: false, reason: 'no_cache' };
            }

            // Versuche automatisches Laden
            const loadResult = await this.loadCachedState();

            if (loadResult.success) {
                console.log('✅ [DART-CACHE] Auto-loaded successfully');
                this.startAutoSave();
                return { loaded: true, method: 'auto', data: loadResult };
            } else {
                console.warn('⚠️ [DART-CACHE] Auto-load failed:', loadResult.message);
                this.startAutoSave();
                return { loaded: false, reason: 'load_failed', error: loadResult.message };
            }

        } catch (error) {
            console.error('❌ [DART-CACHE] Auto-load error:', error);
            this.startAutoSave();
            return { loaded: false, reason: 'error', error: error.message };
        }
    }

    /**
     * 📥 Lade gespeicherten Spielstand
     */
    async loadCachedState() {
        if (!this.core.matchData) {
            return { success: false, message: 'Match data not loaded' };
        }

        try {
            const { tournamentId, uniqueId, matchId } = this.core.matchData;
            const actualMatchId = uniqueId || matchId;

            const response = await fetch(`/api/match-state/${tournamentId}/${actualMatchId}/load`);
            const result = await response.json();

            if (!result.success || !result.hasState) {
                return { success: false, message: result.message || 'No state found' };
            }

            const cachedData = result.gameState;

            // Validate cached data
            if (!cachedData.gameState || !cachedData.gameRules || !cachedData.matchData) {
                return { success: false, message: 'Invalid cached data structure' };
            }

            // Restore state to core
            this.core.gameState = cachedData.gameState;
            this.core.gameRules = cachedData.gameRules;

            // Update lastSaveState to prevent immediate save
            this.lastSaveState = JSON.stringify(this.core.gameState);

            console.log('📥 [DART-CACHE] State restored:', {
                currentPlayer: this.core.gameState.currentPlayer,
                currentLeg: this.core.gameState.currentLeg,
                player1Score: this.core.gameState.player1 && this.core.gameState.player1.score,
                player2Score: this.core.gameState.player2 && this.core.gameState.player2.score,
                throwHistoryLength: (this.core.gameState.throwHistory && this.core.gameState.throwHistory.length) || 0,
                lastUpdated: result.lastUpdated,
                age: Math.floor(result.age / (60 * 1000)) + ' minutes'
            });

            return {
                success: true,
                message: 'Spielstand erfolgreich wiederhergestellt',
                lastUpdated: result.lastUpdated,
                age: result.age
            };

        } catch (error) {
            console.error('❌ [DART-CACHE] Failed to load cached state:', error);
            return { success: false, message: error.message };
        }
    }

    /**
     * 💾 Speichere aktuellen Spielstand
     */
    async saveCurrentState() {
        if (!this.core.matchData || this.core.gameState.isGameFinished || this.saveInProgress) {
            return { success: false, message: 'Cannot save at this time' };
        }

        try {
            this.saveInProgress = true;

            const currentStateHash = JSON.stringify(this.core.gameState);

            // Speichere nur wenn sich etwas geändert hat
            if (currentStateHash === this.lastSaveState) {
                this.saveInProgress = false;
                return { success: true, message: 'No changes to save' };
            }

            const { tournamentId, uniqueId, matchId } = this.core.matchData;
            const actualMatchId = uniqueId || matchId;

            const stateData = {
                gameState: this.core.gameState,
                gameRules: this.core.gameRules,
                matchData: this.core.matchData,
                version: '1.0.0',
                savedAt: new Date().toISOString()
            };

            const response = await fetch(`/api/match-state/${tournamentId}/${actualMatchId}/save`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(stateData)
            });

            const result = await response.json();

            if (result.success) {
                this.lastSaveState = currentStateHash;
                this.lastSaveTime = new Date();

                console.log('💾 [DART-CACHE] State saved successfully to:', result.savedTo);

                return {
                    success: true,
                    message: 'Spielstand gespeichert',
                    savedTo: result.savedTo,
                    lastUpdated: result.lastUpdated
                };
            } else {
                throw new Error(result.message);
            }

        } catch (error) {
            console.warn('⚠️ [DART-CACHE] Failed to save state:', error);
            return { success: false, message: error.message };
        } finally {
            this.saveInProgress = false;
        }
    }

    /**
     * 🚀 Start Auto-Save Funktionalität
     */
    startAutoSave() {
        if (!this.isEnabled || this.autoSaveTimer) return;

        console.log('🚀 [DART-CACHE] Starting auto-save every', this.autoSaveInterval / 1000, 'seconds');

        // Regelmäßiges Speichern
        this.autoSaveTimer = setInterval(() => {
            this.saveCurrentState();
        }, this.autoSaveInterval);

        // Speichern nach Würfen
        this.enableSaveOnThrow();

        console.log('✅ [DART-CACHE] Auto-save active');
    }

    /**
     * 🛑 Stop Auto-Save
     */
    stopAutoSave() {
        if (this.autoSaveTimer) {
            clearInterval(this.autoSaveTimer);
            this.autoSaveTimer = null;
        }

        if (this.saveOnThrowTimer) {
            clearTimeout(this.saveOnThrowTimer);
            this.saveOnThrowTimer = null;
        }

        console.log('🛑 [DART-CACHE] Auto-save stopped');
    }

    /**
     * 💾 Enable save after each throw
     */
    enableSaveOnThrow() {
        // Hook in das processThrow der Core
        const originalProcessThrow = this.core.processThrow.bind(this.core);

        this.core.processThrow = (...args) => {
            const result = originalProcessThrow(...args);

            // Auto-save nach erfolgreichem Wurf (mit Delay)
            if (result.success && !this.core.gameState.isGameFinished) {
                // Cancel existing timer
                if (this.saveOnThrowTimer) {
                    clearTimeout(this.saveOnThrowTimer);
                }

                // Save with delay
                this.saveOnThrowTimer = setTimeout(() => {
                    this.saveCurrentState();
                }, this.saveOnThrowDelay);
            }

            return result;
        };

        console.log('🎯 [DART-CACHE] Save-on-throw enabled');
    }

    /**
     * 🗑️ Lösche gespeicherten State (nach Match-Ende)
     */
    async clearCachedState() {
        if (!this.core.matchData) return;

        try {
            this.stopAutoSave(); // Stop auto-save first

            const { tournamentId, uniqueId, matchId } = this.core.matchData;
            const actualMatchId = uniqueId || matchId;

            const response = await fetch(`/api/match-state/${tournamentId}/${actualMatchId}/clear`, {
                method: 'DELETE'
            });

            const result = await response.json();

            console.log('🗑️ [DART-CACHE] Cached state cleared:', result.clearedFrom);

            this.lastSaveState = null;
            this.hasCachedState = false;

            return result;

        } catch (error) {
            console.warn('⚠️ [DART-CACHE] Failed to clear cached state:', error);
            return { success: false, message: error.message };
        }
    }

    /**
     * 📊 Get cache status info
     */
    getCacheStatus() {
        return {
            isEnabled: this.isEnabled,
            autoSaveActive: !!this.autoSaveTimer,
            hasCachedState: this.hasCachedState,
            lastSaveTime: this.lastSaveTime,
            saveInProgress: this.saveInProgress,
            autoSaveInterval: this.autoSaveInterval,
            saveOnThrowDelay: this.saveOnThrowDelay
        };
    }

    /**
     * 🔧 Enable/Disable caching
     */
    setEnabled(enabled) {
        this.isEnabled = enabled;

        if (enabled) {
            this.startAutoSave();
            console.log('✅ [DART-CACHE] Caching enabled');
        } else {
            this.stopAutoSave();
            console.log('❌ [DART-CACHE] Caching disabled');
        }
    }

    /**
     * 🧹 Cleanup
     */
    cleanup() {
        this.stopAutoSave();
        this.lastSaveState = null;
        console.log('🧹 [DART-CACHE] Cache manager cleaned up');
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringCache;
} else {
    window.DartScoringCache = DartScoringCache;
}

console.log('💾 [DART-CACHE] Dart Scoring Cache module loaded');