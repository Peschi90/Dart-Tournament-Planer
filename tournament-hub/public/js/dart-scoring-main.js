/**
 * Dart Scoring Main Module
 * Main application logic and initialization
 */
class DartScoringMain {
    constructor() {
        this.core = new DartScoringCore();
        this.ui = new DartScoringUI();
        this.stats = new DartScoringStats(this.core, this.ui);
        this.submission = new DartScoringSubmission(this.core, this.ui, this.stats); // ✅ NEU: WebSocket Submission

        console.log('🚀 [DART-MAIN] Dart Scoring Main initialized with enhanced submission');
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

            // Initialize UI with core
            this.ui.initialize(this.core);

            // Start statistics tracking
            this.stats.startTracking();

            // ✅ NEU: Initialize WebSocket submission system
            await this.initializeSubmissionSystem();

            // Update match display
            this.ui.updateMatchDisplay();

            // Setup enhanced match result submission
            this.setupEnhancedSubmission();

            console.log('✅ [DART-MAIN] Application initialized successfully with WebSocket submission');
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
     * Show error message to user
     */
    showError(message) {
        // Create error display
        const errorContainer = document.createElement('div');
        errorContainer.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: white;
            padding: 30px;
            border-radius: 15px;
            box-shadow: 0 20px 40px rgba(0,0,0,0.3);
            text-align: center;
            z-index: 9999;
            max-width: 500px;
            margin: 20px;
        `;

        errorContainer.innerHTML = `
            <h2 style="color: #e53e3e; margin-bottom: 15px;">❌ Fehler</h2>
            <p style="color: #4a5568; margin-bottom: 20px;">${message}</p>
            <button onclick="history.back()" style="
                background: #4299e1;
                color: white;
                border: none;
                padding: 10px 20px;
                border-radius: 8px;
                cursor: pointer;
                font-weight: 600;
            ">Zurück</button>
        `;

        document.body.appendChild(errorContainer);
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

    try {
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
                triggerSubmission: () => window.dartScoringApp.triggerManualSubmission(),
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
                }
            };

            console.log('🔧 [DART-MAIN] Enhanced debug functions available:');
            console.log('   window.debugDartScoring.getStats()');
            console.log('   window.debugDartScoring.getSubmissionStatus()');
            console.log('   window.debugDartScoring.triggerSubmission()');
            console.log('   window.debugDartScoring.forceSubmit() // ⚠️ Debugging only');
            console.log('   window.debugDartScoring.setGameFinished(true/false) // ⚠️ Testing only');
            console.log('   window.debugDartScoring.getDebugInfo() // Complete state');
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

console.log('🎯 [DART-MAIN] Enhanced Dart Scoring Main module loaded');