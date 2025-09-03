/**
 * Dart Scoring Main Module
 * Main application logic and initialization
 */
class DartScoringMain {
    constructor() {
        // Check if all required classes are available
        if (typeof DartScoringCore === 'undefined') {
            throw new Error('DartScoringCore is not loaded');
        }
        if (typeof DartScoringUI === 'undefined') {
            throw new Error('DartScoringUI is not loaded');
        }
        if (typeof DartScoringStats === 'undefined') {
            throw new Error('DartScoringStats is not loaded');
        }
        if (typeof DartScoringSubmission === 'undefined') {
            throw new Error('DartScoringSubmission is not loaded');
        }
        if (typeof DartScoringCache === 'undefined') {
            throw new Error('DartScoringCache is not loaded');
        }
        if (typeof DartScoringCacheUI === 'undefined') {
            throw new Error('DartScoringCacheUI is not loaded');
        }

        this.core = new DartScoringCore();
        this.ui = new DartScoringUI();
        this.stats = new DartScoringStats(this.core, this.ui);
        this.submission = new DartScoringSubmission(this.core, this.ui, this.stats); // ✅ NEU: WebSocket Submission
        
        // ✅ NEU: Cache System
        this.cache = new DartScoringCache(this.core);
        this.cacheUI = new DartScoringCacheUI(this.core, this.ui, this.cache);

        console.log('🚀 [DART-MAIN] Dart Scoring Main initialized with caching system');
    }

    /**
     * Initialize the application
     */
    async initialize() {
        try {
            console.log('🔄 [DART-MAIN] Starting application initialization...');

            // Get URL parameters
            const urlParams = new URLSearchParams(window.location.search);
            const tournamentId = urlParams.get('tournament') || urlParams.get('t');
            const matchId = urlParams.get('match') || urlParams.get('m');

            console.log('📋 [DART-MAIN] URL parameters:', { tournamentId, matchId });

            if (!tournamentId || !matchId) {
                throw new Error('Tournament ID and Match ID are required');
            }

            // Initialize core with match data
            const coreInitialized = await this.core.initialize(matchId, tournamentId);
            if (!coreInitialized) {
                throw new Error('Failed to initialize dart scoring core');
            }

            // ✅ NEU: Prüfe automatisch auf gespeicherten State und lade wenn verfügbar
            const autoLoadResult = await this.handleAutoLoad();

            // Initialize UI with core
            this.ui.initialize(this.core);

            // ✅ NEU: Initialize Cache UI
            this.cacheUI.initialize();

            // Start statistics tracking
            this.stats.startTracking();

            // ✅ NEU: Initialize WebSocket submission system
            await this.initializeSubmissionSystem();

            // Update match display
            this.ui.updateMatchDisplay();

            // Setup enhanced match result submission
            this.setupEnhancedSubmission();

            // ✅ NEU: Setup match completion cleanup
            this.setupMatchCompletionCleanup();

            // Show auto-load result
            this.showAutoLoadResult(autoLoadResult);

            console.log('✅ [DART-MAIN] Application initialized successfully with caching system');
            return true;

        } catch (error) {
            console.error('❌ [DART-MAIN] Failed to initialize application:', error);
            this.showError('Fehler beim Laden des Spiels: ' + error.message);
            return false;
        }
    }

    /**
     * Initialize WebSocket submission system
     */
    async initializeSubmissionSystem() {
        try {
            console.log('📤 [DART-MAIN] Initializing WebSocket submission system...');

            const initialized = await this.submission.initialize();

            if (initialized) {
                console.log('✅ [DART-MAIN] WebSocket submission system ready');

                // Show connection status
                if (this.ui) {
                    this.ui.showMessage('✅ WebSocket-Verbindung für erweiterte Übertragung bereit', 'success');
                }
            } else {
                console.warn('⚠️ [DART-MAIN] WebSocket submission not available, using fallback');

                if (this.ui) {
                    this.ui.showMessage('⚠️ WebSocket-Verbindung nicht verfügbar - verwende Standard-Übertragung', 'warning');
                }
            }

        } catch (error) {
            console.error('❌ [DART-MAIN] Failed to initialize submission system:', error);

            if (this.ui) {
                this.ui.showMessage('⚠️ Erweiterte Übertragung nicht verfügbar', 'warning');
            }
        }
    }

    /**
     * Setup enhanced match result submission
     */
    setupEnhancedSubmission() {
        // ✅ NEU: Override core's submitMatchResult with enhanced WebSocket submission
        const originalSubmitMatchResult = this.core.submitMatchResult.bind(this.core);

        this.core.submitMatchResult = async() => {
            console.log('📤 [DART-MAIN] Using enhanced WebSocket submission with statistics');

            try {
                // Prüfe ob WebSocket Submission möglich ist
                const submissionStatus = this.submission.getSubmissionStatus();

                if (submissionStatus.canSubmit && submissionStatus.isConnected) {
                    console.log('🌐 [DART-MAIN] Using WebSocket submission with enhanced statistics');

                    // Show progress
                    this.submission.showSubmissionProgress('📤 Übertrage Ergebnis mit Statistiken...');

                    // Use enhanced WebSocket submission
                    const result = await this.submission.submitEnhancedMatchResult();

                    if (result.success) {
                        // Handle successful submission (includes navigation back)
                        await this.submission.handleSuccessfulSubmission(result);
                        return result;
                    } else {
                        // WebSocket failed, try fallback
                        console.warn('⚠️ [DART-MAIN] WebSocket submission failed, trying standard fallback');
                        this.submission.showSubmissionProgress('⚠️ WebSocket fehlgeschlagen, verwende Standard-Übertragung...');

                        // Use statistics-enhanced fallback
                        return await this.stats.submitMatchResult();
                    }
                } else {
                    // Use statistics-enhanced fallback
                    console.log('📊 [DART-MAIN] WebSocket not available, using enhanced statistics fallback');
                    this.submission.showSubmissionProgress('📊 Verwende erweiterte Statistik-Übertragung...');

                    const result = await this.stats.submitMatchResult();

                    if (result.success) {
                        // ✅ NEU: Show completion message instead of navigate back
                        this.ui.showMatchCompletedMessage();
                    }

                    return result;
                }

            } catch (error) {
                console.error('❌ [DART-MAIN] Enhanced submission failed, trying final fallback:', error);

                // Final fallback to original method
                this.submission.showSubmissionProgress('❌ Erweiterte Übertragung fehlgeschlagen, verwende Standard-Methode...');

                try {
                    return await originalSubmitMatchResult();
                } catch (fallbackError) {
                    console.error('❌ [DART-MAIN] All submission methods failed:', fallbackError);
                    return {
                        success: false,
                        message: `Alle Übertragungsmethoden fehlgeschlagen: ${fallbackError.message}`
                    };
                }
            }
        };

        console.log('🔄 [DART-MAIN] Enhanced WebSocket submission system activated');
    }

    /**
     * Handle automatic loading on start
     */
    async handleAutoLoad() {
        console.log('🔄 [DART-MAIN] Handling auto-load...');

        try {
            // Check if cached state exists
            const hasCached = await this.cache.checkForCachedState();

            if (!hasCached) {
                console.log('📭 [DART-MAIN] No cached state found - starting fresh game');
                this.cache.startAutoSave();
                this.cacheUI.updateRestoreButton(false);
                return { loaded: false, reason: 'no_cache' };
            }

            console.log('🔍 [DART-MAIN] Cached state found - attempting auto-load');

            // Try to load automatically
            const loadResult = await this.cache.loadCachedState();

            if (loadResult.success) {
                console.log('✅ [DART-MAIN] Auto-load successful');
                this.cache.startAutoSave();
                this.cacheUI.updateRestoreButton(false);
                return { 
                    loaded: true, 
                    method: 'auto', 
                    age: loadResult.age,
                    lastUpdated: loadResult.lastUpdated 
                };
            } else {
                console.warn('⚠️ [DART-MAIN] Auto-load failed, showing manual button');
                this.cache.startAutoSave();
                this.cacheUI.updateRestoreButton(true, true);
                return { loaded: false, reason: 'auto_load_failed', error: loadResult.message };
            }

        } catch (error) {
            console.error('❌ [DART-MAIN] Auto-load error:', error);
            this.cache.startAutoSave();
            this.cacheUI.updateRestoreButton(true, true);
            return { loaded: false, reason: 'error', error: error.message };
        }
    }

    /**
     * Setup cleanup when match is completed
     */
    setupMatchCompletionCleanup() {
        // Hook in das submitMatchResult um Cache zu löschen
        const originalSubmit = this.core.submitMatchResult.bind(this.core);
        
        this.core.submitMatchResult = async (...args) => {
            try {
                const result = await originalSubmit(...args);
                
                // Wenn erfolgreich übertragen, lösche Cache
                if (result.success) {
                    console.log('🗑️ [DART-MAIN] Match completed successfully, clearing cache');
                    await this.cache.clearCachedState();
                    this.cacheUI.updateRestoreButton(false);
                }
                
                return result;
            } catch (error) {
                throw error;
            }
        };
    }

    /**
     * Show result of auto-load
     */
    showAutoLoadResult(result) {
        if (result.loaded) {
            const ageText = result.age ? this.cacheUI.formatAge(result.age) : '';
            this.ui.showMessage(`🔄 Spielstand automatisch wiederhergestellt! ${ageText}`, 'success', 4000);
        } else if (result.reason === 'auto_load_failed') {
            this.ui.showMessage('💡 Gespeicherter Spielstand verfügbar - Button zum Wiederherstellen anzeigen', 'info', 3000);
        }
    }

    /**
     * Get current match statistics
     */
    getCurrentStatistics() {
        return this.stats.getCurrentStatistics();
    }

    /**
     * Get submission status
     */
    getSubmissionStatus() {
        return this.submission.getSubmissionStatus();
    }

    /**
     * Get cache status
     */
    getCacheStatus() {
        return this.cache.getCacheStatus();
    }

    /**
     * Manual submission trigger (for debugging)
     */
    async triggerManualSubmission() {
        try {
            console.log('🔧 [DART-MAIN] Manual submission triggered');

            if (!this.core.gameState.isGameFinished) {
                throw new Error('Match ist noch nicht beendet');
            }

            return await this.core.submitMatchResult();

        } catch (error) {
            console.error('❌ [DART-MAIN] Manual submission failed:', error);
            return {
                success: false,
                message: error.message
            };
        }
    }

    /**
     * Cleanup resources
     */
    cleanup() {
        this.stats.stopTracking();
        this.submission.cleanup();
        this.cache.cleanup();
        this.cacheUI.cleanup();
        this.core.cleanup();
        console.log('🧹 [DART-MAIN] Application cleaned up');
    }
}

// Global error handler
window.addEventListener('unhandledrejection', (event) => {
    console.error('🚨 [DART-MAIN] Unhandled promise rejection:', event.reason);
    event.preventDefault();
});

// Initialize application when DOM is loaded
document.addEventListener('DOMContentLoaded', async() => {
    console.log('📄 [DART-MAIN] DOM loaded, initializing enhanced dart scoring app...');

    // Wait a bit for all scripts to be fully loaded
    await new Promise(resolve => setTimeout(resolve, 100));

    try {
        // Check if all required dependencies are loaded
        const requiredClasses = ['DartScoringCore', 'DartScoringUI', 'DartScoringStats', 'DartScoringSubmission', 'DartScoringCache', 'DartScoringCacheUI'];
        const missingClasses = requiredClasses.filter(className => typeof window[className] === 'undefined');

        if (missingClasses.length > 0) {
            throw new Error(`Missing required classes: ${missingClasses.join(', ')}`);
        }

        window.dartScoringApp = new DartScoringMain();
        const success = await window.dartScoringApp.initialize();

        if (success) {
            console.log('🎉 [DART-MAIN] Enhanced dart scoring app initialized successfully!');

            // Debug information in console
            console.log('🔍 [DART-MAIN] Debug Info:');
            console.log('   Statistics:', window.dartScoringApp.getCurrentStatistics());
            console.log('   Submission Status:', window.dartScoringApp.getSubmissionStatus());

            // Global debug functions
            window.debugDartScoring = {
                getStats: () => window.dartScoringApp.getCurrentStatistics(),
                getSubmissionStatus: () => window.dartScoringApp.getSubmissionStatus(),
                getCacheStatus: () => window.dartScoringApp.getCacheStatus(),
                triggerSubmission: () => window.dartScoringApp.triggerManualSubmission(),
                // ✅ NEU: Cache Debug Functions
                checkCache: () => window.dartScoringApp.cache.checkForCachedState(),
                saveState: () => window.dartScoringApp.cache.saveCurrentState(),
                loadState: () => window.dartScoringApp.cache.loadCachedState(),
                clearCache: () => window.dartScoringApp.cache.clearCachedState(),
                // ✅ NEU: Force Submit für Debugging
                forceSubmit: async() => {
                    console.warn('🔧 [DEBUG] Force submitting match result...');
                    try {
                        const result = await window.dartScoringApp.submission.forceSubmitEnhancedMatchResult();
                        console.log('🔧 [DEBUG] Force submit result:', result);
                        return result;
                    } catch (error) {
                        console.error('❌ [DEBUG] Force submit failed:', error);
                        return { success: false, error: error.message };
                    }
                },
                // ✅ NEU: Set game finished für Testing
                setGameFinished: (finished = true) => {
                    console.warn('🔧 [DEBUG] Setting isGameFinished to:', finished);
                    window.dartScoringApp.core.gameState.isGameFinished = finished;
                    console.log('🔧 [DEBUG] New game state:', window.dartScoringApp.core.gameState.isGameFinished);
                    return window.dartScoringApp.core.gameState.isGameFinished;
                },
                // ✅ NEU: Complete Debug Info
                getDebugInfo: () => {
                    return {
                        stats: window.dartScoringApp.getCurrentStatistics(),
                        submissionStatus: window.dartScoringApp.getSubmissionStatus(),
                        cacheStatus: window.dartScoringApp.getCacheStatus(),
                        gameState: {
                            isGameFinished: window.dartScoringApp.core.gameState.isGameFinished,
                            currentPlayer: window.dartScoringApp.core.gameState.currentPlayer,
                            player1: {
                                legs: window.dartScoringApp.core.gameState.player1.legs,
                                sets: window.dartScoringApp.core.gameState.player1.sets,
                                score: window.dartScoringApp.core.gameState.player1.score
                            },
                            player2: {
                                legs: window.dartScoringApp.core.gameState.player2.legs,
                                sets: window.dartScoringApp.core.gameState.player2.sets,
                                score: window.dartScoringApp.core.gameState.player2.score
                            }
                        },
                        matchData: window.dartScoringApp.core.matchData,
                        gameRules: window.dartScoringApp.core.gameRules
                    };
                },
                // 🆕 NEU: Game Rules Debug Function
                debugGameRules: () => {
                    const core = window.dartScoringApp.core;
                    const match = core.matchData;
                    const coreRules = core.gameRules;

                    console.log('🎮 [DEBUG] Complete Game Rules Analysis:');
                    console.log('🎮 [DEBUG] Core gameRules:', coreRules);
                    console.log('🎮 [DEBUG] Match.gameRules:', match.gameRules);
                    console.log('🎮 [DEBUG] Match.GameRules:', match.GameRules);
                    console.log('🎮 [DEBUG] Match.gameRulesUsed:', match.gameRulesUsed);
                    console.log('🎮 [DEBUG] Match Type:', match.matchType || match.type);
                    console.log('🎮 [DEBUG] Match Class:', match.classId || match.class);
                    console.log('🎮 [DEBUG] Full Match Data:', match);

                    // Test Rule Application
                    const legsToWin = coreRules.legsToWinSet || coreRules.legsToWin || 2;
                    const setsToWin = coreRules.setsToWin || 1;
                    const startingScore = core.getStartingScore();
                    const doubleOut = coreRules.doubleOut;

                    console.log('🎮 [DEBUG] Applied Rules:', {
                        legsToWin,
                        setsToWin,
                        startingScore,
                        doubleOut,
                        gameMode: coreRules.gameMode
                    });

                    return {
                        coreRules,
                        matchRules: {
                            gameRules: match.gameRules,
                            GameRules: match.GameRules,
                            gameRulesUsed: match.gameRulesUsed
                        },
                        matchInfo: {
                            matchType: match.matchType || match.type,
                            classId: match.classId || match.class,
                            displayName: match.displayName || match.name
                        },
                        appliedRules: {
                            legsToWin,
                            setsToWin,
                            startingScore,
                            doubleOut,
                            gameMode: coreRules.gameMode
                        }
                    };
                }
            };

            console.log('🔧 [DART-MAIN] Enhanced debug functions available:');
            console.log('   window.debugDartScoring.getStats()');
            console.log('   window.debugDartScoring.getSubmissionStatus()');
            console.log('   window.debugDartScoring.getCacheStatus() // ✅ NEU');
            console.log('   window.debugDartScoring.checkCache() // ✅ NEU');
            console.log('   window.debugDartScoring.saveState() // ✅ NEU');
            console.log('   window.debugDartScoring.loadState() // ✅ NEU');
            console.log('   window.debugDartScoring.clearCache() // ✅ NEU');
            console.log('   window.debugDartScoring.triggerSubmission()');
            console.log('   window.debugDartScoring.forceSubmit() // ⚠️ Debugging only');
            console.log('   window.debugDartScoring.setGameFinished(true/false) // ⚠️ Testing only');
            console.log('   window.debugDartScoring.getDebugInfo() // Complete state');
            console.log('   window.debugDartScoring.debugGameRules() // 🆕 Game Rules Analysis');
        }
    } catch (error) {
        console.error('❌ [DART-MAIN] Failed to start enhanced app:', error);
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (window.dartScoringApp) {
        window.dartScoringApp.cleanup();
    }
});

console.log('🎯 [DART-MAIN] Enhanced Dart Scoring Main module with caching loaded');