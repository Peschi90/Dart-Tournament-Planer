/**
 * Dart Scoring WebSocket Submission Module
 * Handles enhanced match result submission with statistics via WebSocket
 */
class DartScoringSubmission {
    constructor(core, ui, stats) {
        this.core = core;
        this.ui = ui;
        this.stats = stats;
        this.socket = null;
        this.isConnected = false;
        this.submissionInProgress = false;
        this.acknowledgmentReceived = false;
        this.acknowledgmentTimeout = null;

        console.log('📤 [DART-SUBMISSION] Dart Scoring Submission module initialized');
    }

    /**
     * Initialize WebSocket connection for submission
     */
    async initialize() {
        try {
            console.log('🔌 [DART-SUBMISSION] Initializing WebSocket connection...');

            // Verwende bereits bestehende Socket-Verbindung vom Core
            if (this.core.socket && this.core.socket.connected) {
                this.socket = this.core.socket;
                this.isConnected = true;
                this.setupSubmissionHandlers();
                console.log('✅ [DART-SUBMISSION] Using existing socket connection');
                return true;
            }

            // Fallback: Erstelle neue Socket-Verbindung
            this.socket = io('/', {
                transports: ['websocket', 'polling'],
                timeout: 10000,
                forceNew: false
            });

            await this.waitForConnection();
            this.setupSubmissionHandlers();

            console.log('✅ [DART-SUBMISSION] WebSocket connection established');
            return true;

        } catch (error) {
            console.error('❌ [DART-SUBMISSION] Failed to initialize WebSocket:', error);
            return false;
        }
    }

    /**
     * Wait for socket connection
     */
    waitForConnection() {
        return new Promise((resolve, reject) => {
            const timeout = setTimeout(() => {
                reject(new Error('Connection timeout'));
            }, 10000);

            this.socket.on('connect', () => {
                clearTimeout(timeout);
                this.isConnected = true;
                console.log('✅ [DART-SUBMISSION] Socket connected for submission');
                resolve();
            });

            this.socket.on('connect_error', (error) => {
                clearTimeout(timeout);
                this.isConnected = false;
                reject(error);
            });
        });
    }

    /**
     * Setup WebSocket event handlers for submission
     */
    setupSubmissionHandlers() {
        // Acknowledgment for match result submission
        this.socket.on('result-submitted', (data) => {
            console.log('✅ [DART-SUBMISSION] Match result acknowledgment received:', data);
            this.handleSubmissionAcknowledgment(data);
        });

        // Error handling
        this.socket.on('match-result-error', (error) => {
            console.error('❌ [DART-SUBMISSION] Match result error received:', error);
            this.handleSubmissionError(error);
        });

        // Match page specific events
        this.socket.on('match-result-submitted', (data) => {
            console.log('📝 [DART-SUBMISSION] Match page submission response:', data);
            this.handleSubmissionAcknowledgment(data);
        });

        // Connection status events
        this.socket.on('disconnect', () => {
            this.isConnected = false;
            console.warn('⚠️ [DART-SUBMISSION] Socket disconnected');
        });

        this.socket.on('reconnect', () => {
            this.isConnected = true;
            console.log('🔄 [DART-SUBMISSION] Socket reconnected');
        });

        console.log('🎧 [DART-SUBMISSION] Submission handlers setup complete');
    }

    /**
     * Submit enhanced match result with statistics
     */
    async submitEnhancedMatchResult() {
        if (this.submissionInProgress) {
            console.warn('⚠️ [DART-SUBMISSION] Submission already in progress');
            return {
                success: false,
                message: 'Übertragung läuft bereits'
            };
        }

        try {
            console.log('📤 [DART-SUBMISSION] Starting enhanced match result submission...');
            this.submissionInProgress = true;
            this.acknowledgmentReceived = false;

            // Prüfe Verbindung
            if (!this.isConnected || !this.socket || !this.socket.connected) {
                console.warn('⚠️ [DART-SUBMISSION] No WebSocket connection, attempting to reconnect...');
                const connected = await this.initialize();
                if (!connected) {
                    throw new Error('WebSocket-Verbindung konnte nicht hergestellt werden');
                }
            }

            // Prüfe ob Match beendet ist
            if (!this.core.gameState.isGameFinished) {
                throw new Error('Match ist noch nicht beendet');
            }

            // Generiere erweiterte Match-Ergebnisse mit Statistiken
            const enhancedMatchResult = this.generateEnhancedMatchResult();

            console.log('📊 [DART-SUBMISSION] Generated enhanced match result:', enhancedMatchResult);

            // ✅ KORRIGIERT: Sende nur das dartScoringResult-Objekt direkt
            const submissionData = {
                tournamentId: this.core.matchData.tournamentId,
                matchId: this.core.matchData.uniqueId || this.core.matchData.matchId,
                classId: this.core.matchData.classId || 1,
                className: this.core.matchData.className || 'Unknown',

                // ✅ NEU: Sende das dartScoringResult direkt als Hauptinhalt
                dartScoringResult: enhancedMatchResult.dartScoringResult,

                // Standard Match Result für Kompatibilität
                result: {
                    player1Sets: enhancedMatchResult.player1Sets,
                    player2Sets: enhancedMatchResult.player2Sets,
                    player1Legs: enhancedMatchResult.player1Legs,
                    player2Legs: enhancedMatchResult.player2Legs,
                    winner: enhancedMatchResult.winner,
                    winnerPlayerNumber: enhancedMatchResult.winnerPlayerNumber,
                    notes: enhancedMatchResult.notes,
                    submittedVia: enhancedMatchResult.submittedVia,
                    timestamp: enhancedMatchResult.timestamp
                },

                // Enhanced submission metadata
                submissionType: 'enhanced-dart-scoring',
                hasStatistics: true,
                submissionTimestamp: new Date().toISOString(),

                // Match identification für bessere Verarbeitung
                matchIdentification: {
                    uniqueId: this.core.matchData.uniqueId,
                    numericId: this.core.matchData.matchId || this.core.matchData.id,
                    matchType: this.core.matchData.matchType || 'Unknown'
                }
            };

            console.log('📡 [DART-SUBMISSION] Sending enhanced result via WebSocket...');
            console.log('📊 [DART-SUBMISSION] dartScoringResult structure:', enhancedMatchResult.dartScoringResult);

            // Sende mit Promise-basiertem Ansatz und Timeout
            const result = await this.sendWithAcknowledgment(submissionData);

            console.log('✅ [DART-SUBMISSION] Enhanced match result submitted successfully');

            return {
                success: true,
                message: 'Match-Ergebnis mit Statistiken erfolgreich übertragen',
                data: result,
                statistics: this.stats.getCurrentStatistics()
            };

        } catch (error) {
            console.error('❌ [DART-SUBMISSION] Error submitting enhanced match result:', error);
            return {
                success: false,
                message: `Fehler beim Übertragen: ${error.message}`,
                error: error
            };
        } finally {
            this.submissionInProgress = false;
        }
    }

    /**
     * Send data with acknowledgment handling
     */
    sendWithAcknowledgment(submissionData) {
        return new Promise((resolve, reject) => {
            console.log('📨 [DART-SUBMISSION] Sending data and waiting for acknowledgment...');

            // Setup timeout for acknowledgment
            this.acknowledgmentTimeout = setTimeout(() => {
                console.warn('⏰ [DART-SUBMISSION] Acknowledgment timeout');
                reject(new Error('Timeout: Keine Bestätigung vom Server erhalten'));
            }, 15000); // 15 second timeout

            // Store promise handlers for acknowledgment handling
            this.acknowledgmentResolve = resolve;
            this.acknowledgmentReject = reject;

            // Send via Socket.IO with callback
            this.socket.emit('submit-match-result', submissionData, (response) => {
                console.log('📩 [DART-SUBMISSION] Callback response received:', response);
                this.handleCallbackResponse(response);
            });

            console.log('📤 [DART-SUBMISSION] Data sent, waiting for confirmation...');
        });
    }

    /**
     * Handle callback response from socket
     */
    handleCallbackResponse(response) {
        if (this.acknowledgmentTimeout) {
            clearTimeout(this.acknowledgmentTimeout);
            this.acknowledgmentTimeout = null;
        }

        if (response && response.success) {
            console.log('✅ [DART-SUBMISSION] Callback confirmed successful submission');

            if (this.acknowledgmentResolve) {
                this.acknowledgmentResolve(response);
                this.acknowledgmentResolve = null;
                this.acknowledgmentReject = null;
            }
        } else {
            console.error('❌ [DART-SUBMISSION] Callback indicated failure:', response);

            if (this.acknowledgmentReject) {
                this.acknowledgmentReject(new Error(response ? .message || 'Server callback indicated failure'));
                this.acknowledgmentResolve = null;
                this.acknowledgmentReject = null;
            }
        }
    }

    /**
     * Handle submission acknowledgment
     */
    handleSubmissionAcknowledgment(data) {
        if (this.acknowledgmentReceived) {
            console.log('📬 [DART-SUBMISSION] Duplicate acknowledgment received (ignoring)');
            return;
        }

        this.acknowledgmentReceived = true;

        if (this.acknowledgmentTimeout) {
            clearTimeout(this.acknowledgmentTimeout);
            this.acknowledgmentTimeout = null;
        }

        console.log('✅ [DART-SUBMISSION] Submission acknowledgment processed:', data.message);

        // Resolve promise if callback approach didn't work
        if (this.acknowledgmentResolve && data.success) {
            this.acknowledgmentResolve(data);
            this.acknowledgmentResolve = null;
            this.acknowledgmentReject = null;
        }
    }

    /**
     * Handle submission error
     */
    handleSubmissionError(error) {
        if (this.acknowledgmentTimeout) {
            clearTimeout(this.acknowledgmentTimeout);
            this.acknowledgmentTimeout = null;
        }

        console.error('❌ [DART-SUBMISSION] Submission error handled:', error);

        if (this.acknowledgmentReject) {
            this.acknowledgmentReject(new Error(error.error || error.message || 'Unknown submission error'));
            this.acknowledgmentResolve = null;
            this.acknowledgmentReject = null;
        }
    }

    /**
     * Generate enhanced match result with statistics
     */
    generateEnhancedMatchResult() {
        // Nutze das Statistics-System für erweiterte Daten
        const baseMatchResult = this.stats.generateMatchResult();

        // Erweitere mit zusätzlichen Dart Scoring spezifischen Daten
        const enhancedResult = {
            ...baseMatchResult,

            // Enhanced submission metadata
            submissionMetadata: {
                submittedVia: 'AdvancedDartScoring',
                submissionType: 'enhanced-websocket',
                version: '2.0.0',
                submissionTimestamp: new Date().toISOString(),

                // Match tracking info
                matchDuration: this.stats.statistics.matchStatistics.matchDuration,
                totalThrows: this.stats.statistics.player1.totalThrows + this.stats.statistics.player2.totalThrows,

                // Quality metrics
                hasDetailedStats: true,
                has180s: (this.stats.statistics.player1.maximums.length + this.stats.statistics.player2.maximums.length) > 0,
                hasHighFinishes: (this.stats.statistics.player1.highFinishes.length + this.stats.statistics.player2.highFinishes.length) > 0,
                has26Scores: (this.stats.statistics.player1.score26.length + this.stats.statistics.player2.score26.length) > 0
            },

            // Technical WebSocket info
            webSocketSubmission: {
                enabled: true,
                socketId: this.socket ? .id,
                connectionType: this.socket ? .io ? .engine ? .transport ? .name || 'unknown',
                submissionMethod: 'enhanced-dart-scoring-websocket',
                reliability: 'high-with-acknowledgment'
            },

            // Tournament Planner compatibility
            plannerCompatibility: {
                version: '2.0.0',
                supportsEnhancedStats: true,
                requiresUIRefresh: true,
                classId: this.core.matchData.classId || 1,
                className: this.core.matchData.className || 'Unknown'
            }
        };

        return enhancedResult;
    }

    /**
     * Show submission progress in UI
     */
    showSubmissionProgress(message) {
        if (this.ui) {
            this.ui.showMessage(message, 'info');
        }
        console.log(`📊 [DART-SUBMISSION] Progress: ${message}`);
    }

    /**
     * Handle successful submission - show completion message (no redirect)
     */
    async handleSuccessfulSubmission(result) {
        try {
            console.log('🎉 [DART-SUBMISSION] Handling successful submission...');

            // Show success message
            if (this.ui) {
                this.ui.showMessage('✅ Match-Ergebnis erfolgreich übertragen!', 'success');

                // Show statistics summary
                const stats = this.stats.getCurrentStatistics();
                let statsMessage = `📊 Statistiken: ${stats.player1.name} ${stats.player1.average} vs ${stats.player2.name} ${stats.player2.average}`;

                if (stats.player1.maximums + stats.player2.maximums > 0) {
                    statsMessage += ` | 180er: ${stats.player1.maximums + stats.player2.maximums}`;
                }

                if (stats.player1.highFinishes + stats.player2.highFinishes > 0) {
                    statsMessage += ` | High Finishes: ${stats.player1.highFinishes + stats.player2.highFinishes}`;
                }

                this.ui.showMessage(statsMessage, 'success');

                // ✅ NEU: Zeige Match-beendet Nachricht anstatt Weiterleitung
                this.ui.showMatchCompletedMessage();
            }

            // Stop statistics tracking
            this.stats.stopTracking();

            console.log('✅ [DART-SUBMISSION] Match completed successfully - no redirect');

        } catch (error) {
            console.error('❌ [DART-SUBMISSION] Error in post-submission handling:', error);
        }
    }

    /**
     * Navigate back to tournament interface
     */
    navigateBackToTournament() {
        try {
            console.log('🔄 [DART-SUBMISSION] Navigating back to tournament interface...');

            const urlParams = new URLSearchParams(window.location.search);
            const tournamentId = urlParams.get('tournament') || urlParams.get('t');

            if (tournamentId) {
                const tournamentUrl = `/tournament-interface.html?tournament=${tournamentId}`;
                console.log(`🔗 [DART-SUBMISSION] Redirecting to: ${tournamentUrl}`);
                window.location.href = tournamentUrl;
            } else {
                console.warn('⚠️ [DART-SUBMISSION] No tournament ID found, going to dashboard');
                window.location.href = '/dashboard.html';
            }

        } catch (error) {
            console.error('❌ [DART-SUBMISSION] Error navigating back:', error);
            // Fallback: go to dashboard
            window.location.href = '/dashboard.html';
        }
    }

    /**
     * Utility delay function
     */
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    /**
     * Check if submission is possible
     */
    canSubmit() {
        if (!this.core.gameState.isGameFinished) {
            return { canSubmit: false, reason: 'Match ist noch nicht beendet' };
        }

        if (!this.isConnected) {
            return { canSubmit: false, reason: 'Keine WebSocket-Verbindung' };
        }

        if (this.submissionInProgress) {
            return { canSubmit: false, reason: 'Übertragung läuft bereits' };
        }

        return { canSubmit: true };
    }

    /**
     * ✅ NEU: Force submission for debugging (ignores isGameFinished check)
     */
    canForceSubmit() {
        if (!this.isConnected) {
            return { canSubmit: false, reason: 'Keine WebSocket-Verbindung' };
        }

        if (this.submissionInProgress) {
            return { canSubmit: false, reason: 'Übertragung läuft bereits' };
        }

        return { canSubmit: true };
    }

    /**
     * ✅ NEU: Force submit for testing (ignores game finished state)
     */
    async forceSubmitEnhancedMatchResult() {
        console.warn('🔧 [DART-SUBMISSION] FORCE SUBMIT - Ignoring game finished check!');

        if (this.submissionInProgress) {
            console.warn('⚠️ [DART-SUBMISSION] Submission already in progress');
            return {
                success: false,
                message: 'Übertragung läuft bereits'
            };
        }

        try {
            console.log('🔧 [DART-SUBMISSION] Starting FORCED enhanced match result submission...');
            this.submissionInProgress = true;
            this.acknowledgmentReceived = false;

            // Prüfe Verbindung (aber nicht Game State)
            if (!this.isConnected || !this.socket || !this.socket.connected) {
                console.warn('⚠️ [DART-SUBMISSION] No WebSocket connection, attempting to reconnect...');
                const connected = await this.initialize();
                if (!connected) {
                    throw new Error('WebSocket-Verbindung konnte nicht hergestellt werden');
                }
            }

            // ✅ NEU: FORCE - Ignoriere isGameFinished Check
            console.warn('🔧 [DART-SUBMISSION] FORCING submission trotz Spielstatus:', {
                isGameFinished: this.core.gameState.isGameFinished,
                player1Legs: this.core.gameState.player1.legs,
                player2Legs: this.core.gameState.player2.legs,
                player1Sets: this.core.gameState.player1.sets,
                player2Sets: this.core.gameState.player2.sets
            });

            // Generiere erweiterte Match-Ergebnisse mit Statistiken
            const enhancedMatchResult = this.generateEnhancedMatchResult();

            console.log('📊 [DART-SUBMISSION] Generated FORCED enhanced match result:', enhancedMatchResult);

            // ✅ KORRIGIERT: Sende nur das dartScoringResult-Objekt direkt (auch bei Force)
            const submissionData = {
                tournamentId: this.core.matchData.tournamentId,
                matchId: this.core.matchData.uniqueId || this.core.matchData.matchId,
                classId: this.core.matchData.classId || 1,
                className: this.core.matchData.className || 'Unknown',

                // ✅ NEU: Sende das dartScoringResult direkt als Hauptinhalt
                dartScoringResult: enhancedMatchResult.dartScoringResult,

                // Standard Match Result für Kompatibilität
                result: {
                    player1Sets: enhancedMatchResult.player1Sets,
                    player2Sets: enhancedMatchResult.player2Sets,
                    player1Legs: enhancedMatchResult.player1Legs,
                    player2Legs: enhancedMatchResult.player2Legs,
                    winner: enhancedMatchResult.winner,
                    winnerPlayerNumber: enhancedMatchResult.winnerPlayerNumber,
                    notes: enhancedMatchResult.notes,
                    submittedVia: enhancedMatchResult.submittedVia,
                    timestamp: enhancedMatchResult.timestamp
                },

                // Enhanced submission metadata
                submissionType: 'enhanced-dart-scoring-FORCED',
                hasStatistics: true,
                submissionTimestamp: new Date().toISOString(),

                // Match identification für bessere Verarbeitung
                matchIdentification: {
                    uniqueId: this.core.matchData.uniqueId,
                    numericId: this.core.matchData.matchId || this.core.matchData.id,
                    matchType: this.core.matchData.matchType || 'Unknown'
                },

                // Force flag
                forcedSubmission: true
            };

            console.log('📡 [DART-SUBMISSION] Sending FORCED enhanced result via WebSocket...');
            console.log('📊 [DART-SUBMISSION] FORCED dartScoringResult structure:', enhancedMatchResult.dartScoringResult);

            // Sende mit Promise-basiertem Ansatz und Timeout
            const result = await this.sendWithAcknowledgment(submissionData);

            console.log('✅ [DART-SUBMISSION] FORCED enhanced match result submitted successfully');

            return {
                success: true,
                message: 'FORCED Match-Ergebnis mit Statistiken erfolgreich übertragen',
                data: result,
                statistics: this.stats.getCurrentStatistics(),
                wasForced: true
            };

        } catch (error) {
            console.error('❌ [DART-SUBMISSION] Error in FORCED submission:', error);
            return {
                success: false,
                message: `Fehler beim Übertragen (FORCED): ${error.message}`,
                error: error
            };
        } finally {
            this.submissionInProgress = false;
        }
    }

    /**
     * Get submission status
     */
    getSubmissionStatus() {
        return {
            isConnected: this.isConnected,
            submissionInProgress: this.submissionInProgress,
            acknowledgmentReceived: this.acknowledgmentReceived,
            socketId: this.socket ? .id,
            canSubmit: this.canSubmit().canSubmit
        };
    }

    /**
     * Cleanup resources
     */
    cleanup() {
        if (this.acknowledgmentTimeout) {
            clearTimeout(this.acknowledgmentTimeout);
            this.acknowledgmentTimeout = null;
        }

        // Don't disconnect core socket - only clean up handlers
        if (this.socket && this.socket !== this.core.socket) {
            this.socket.disconnect();
        }

        this.socket = null;
        this.isConnected = false;
        this.submissionInProgress = false;
        this.acknowledgmentReceived = false;
        this.acknowledgmentResolve = null;
        this.acknowledgmentReject = null;

        console.log('🧹 [DART-SUBMISSION] Submission module cleaned up');
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringSubmission;
} else {
    window.DartScoringSubmission = DartScoringSubmission;
}

console.log('📤 [DART-SUBMISSION] Dart Scoring Submission module loaded');