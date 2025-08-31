/**
 * Dart Scoring Statistics Module
 * Handles statistics collection and submission for dart matches
 */
class DartScoringStats {
    constructor(core, ui) {
        this.core = core;
        this.ui = ui;
        this.statistics = {
            player1: {
                maximums: [], // 180er
                highFinishes: [], // Finishes = 100
                score26: [], // Geworfene 26-Punkte-Würfe
                totalThrows: 0,
                totalScore: 0,
                legs: 0,
                sets: 0,
                average: 0,
                checkouts: [],
                // 🆕 NEU: Leg-spezifische Averages
                legAverages: [], // Array mit Average pro Leg
                currentLegThrows: 0,
                currentLegScore: 0
            },
            player2: {
                maximums: [], // 180er
                highFinishes: [], // Finishes = 100
                score26: [], // Geworfene 26-Punkte-Würfe
                totalThrows: 0,
                totalScore: 0,
                legs: 0,
                sets: 0,
                average: 0,
                checkouts: [],
                // 🆕 NEU: Leg-spezifische Averages
                legAverages: [], // Array mit Average pro Leg
                currentLegThrows: 0,
                currentLegScore: 0
            },
            matchStatistics: {
                totalLegs: 0,
                totalSets: 0,
                matchDuration: 0,
                startTime: null,
                endTime: null
            }
        };

        this.isTracking = false;

        console.log('📊 [DART-STATS] Statistics module initialized');
    }

    /**
     * Start tracking statistics for match
     */
    startTracking() {
        this.isTracking = true;
        this.statistics.matchStatistics.startTime = new Date();

        // Hook into throw processing to collect stats
        this.setupStatsHooks();

        console.log('🔄 [DART-STATS] Statistics tracking started');
    }

    /**
     * Setup hooks to collect statistics during gameplay
     */
    setupStatsHooks() {
        // Store original processThrow method
        const originalProcessThrow = this.core.processThrow.bind(this.core);

        // Override processThrow to collect statistics
        this.core.processThrow = (dart1, dart2, dart3) => {
            const result = originalProcessThrow(dart1, dart2, dart3);

            if (result.success && this.isTracking) {
                this.collectThrowStatistics(dart1, dart2, dart3, result);
            }

            return result;
        };

        // 🆕 NEU: Hook into startNewLeg to reset leg stats
        const originalStartNewLeg = this.core.startNewLeg.bind(this.core);

        this.core.startNewLeg = () => {
            const result = originalStartNewLeg();

            if (result.success && this.isTracking) {
                this.resetLegStatsForAllPlayers();
            }

            return result;
        };
    }

    /**
     * Collect statistics from a throw
     */
    collectThrowStatistics(dart1, dart2, dart3, throwResult) {
        const throwTotal = dart1 + dart2 + dart3;
        const playerNumber = throwResult.bustedPlayer || throwResult.winner || this.getPreviousPlayer();
        const playerStats = playerNumber === 1 ? this.statistics.player1 : this.statistics.player2;

        console.log('📈 [DART-STATS] Collecting stats for player', playerNumber, 'throw:', [dart1, dart2, dart3], 'total:', throwTotal);

        // 🔧 NEU: Berechne die tatsächlich geworfenen Darts für korrekte Average-Berechnung
        const actualDartsThrown = this.countActualDartsThrown(dart1, dart2, dart3);

        // Update basic stats - 🔧 KORRIGIERT: Match Average = alle Punkte / alle geworfenen Darts * 3
        playerStats.totalThrows += actualDartsThrown;
        playerStats.totalScore += throwTotal;

        // 🎯 NEU: Match Average = (Gesamtpunkte / Gesamtdarts) * 3
        // Beispiel: 300 Punkte mit 6 Darts = (300/6)*3 = 150 Average
        const totalDartsThrown = playerStats.totalThrows;
        if (totalDartsThrown > 0) {
            playerStats.average = (playerStats.totalScore / totalDartsThrown) * 3;
        }

        // 🆕 NEU: Update Leg-spezifische Statistiken
        playerStats.currentLegThrows += actualDartsThrown;
        playerStats.currentLegScore += throwTotal;

        // Check for 180 (Maximum)
        if (throwTotal === 180) {
            playerStats.maximums.push({
                darts: [dart1, dart2, dart3],
                total: 180,
                timestamp: new Date()
            });
            console.log('🎯 [DART-STATS] 180 recorded for player', playerNumber);
        }

        // 🔧 KORRIGIERT: Check for 26 score (unabhängig vom Checkout)
        if (throwTotal === 26) {
            playerStats.score26.push({
                darts: [dart1, dart2, dart3],
                total: 26,
                timestamp: new Date()
            });
            console.log('💯 [DART-STATS] 26 score recorded for player', playerNumber, 'darts:', [dart1, dart2, dart3]);
        }

        // 🔧 KORRIGIERT: Check for high finish (100 and over checkout)
        if (throwResult.type === 'leg_won' && throwTotal >= 100) {
            playerStats.highFinishes.push({
                finish: throwTotal,
                darts: [dart1, dart2, dart3],
                remainingScore: throwTotal,
                timestamp: new Date()
            });
            console.log('🏆 [DART-STATS] High finish (>=100) recorded for player', playerNumber, 'finish:', throwTotal);
        }

        // Record all checkouts and berechne Leg Average
        if (throwResult.type === 'leg_won') {
            playerStats.checkouts.push({
                finish: throwTotal,
                darts: [dart1, dart2, dart3],
                doubleOut: throwResult.doubleOut || false,
                timestamp: new Date()
            });

            // 🎯 NEU: Berechne und speichere Leg Average beim Leg-Ende
            this.finalizeLegAverage(playerStats);
        }

        // Update legs and sets from game state
        if (this.core.gameState) {
            this.statistics.player1.legs = this.core.gameState.player1.legs;
            this.statistics.player1.sets = this.core.gameState.player1.sets;
            this.statistics.player2.legs = this.core.gameState.player2.legs;
            this.statistics.player2.sets = this.core.gameState.player2.sets;
        }

        console.log('📊 [DART-STATS] Updated stats for player', playerNumber, ':', {
            totalScore: playerStats.totalScore,
            totalThrows: playerStats.totalThrows,
            matchAverage: playerStats.average.toFixed(2),
            currentLegScore: playerStats.currentLegScore,
            currentLegThrows: playerStats.currentLegThrows,
            legAverages: playerStats.legAverages
        });
    }

    /**
     * 🔢 NEU: Zähle tatsächlich geworfene Darts (nicht immer 3)
     */
    countActualDartsThrown(dart1, dart2, dart3) {
        let count = 0;

        // Zähle jeden Dart der >= 0 ist (Miss = 0 ist ein geworfener Dart)
        if (dart1 !== null && dart1 !== undefined && dart1 >= 0) count++;
        if (dart2 !== null && dart2 !== undefined && dart2 >= 0) count++;
        if (dart3 !== null && dart3 !== undefined && dart3 >= 0) count++;

        // Falls keine Darts gezählt wurden aber processThrow aufgerufen wurde, mindestens 1
        return Math.max(count, 1);
    }

    /**
     * 🎯 NEU: Finalisiere Leg Average und reset current leg stats
     */
    finalizeLegAverage(playerStats) {
        if (playerStats.currentLegThrows > 0) {
            // Berechne Leg Average: (Leg-Punkte / Leg-Darts) * 3
            const legAverage = (playerStats.currentLegScore / playerStats.currentLegThrows) * 3;

            playerStats.legAverages.push({
                legNumber: playerStats.legAverages.length + 1,
                average: Math.round(legAverage * 10) / 10, // Auf 1 Dezimalstelle runden
                score: playerStats.currentLegScore,
                throws: playerStats.currentLegThrows,
                timestamp: new Date()
            });

            console.log('📈 [DART-STATS] Leg average finalized:', {
                leg: playerStats.legAverages.length,
                average: legAverage.toFixed(1),
                score: playerStats.currentLegScore,
                throws: playerStats.currentLegThrows
            });
        }

        // Reset current leg stats
        playerStats.currentLegThrows = 0;
        playerStats.currentLegScore = 0;
    }

    /**
     * 🔄 NEU: Reset leg stats for new leg (auch bei anderen Spielern)
     */
    resetLegStatsForAllPlayers() {
        this.statistics.player1.currentLegThrows = 0;
        this.statistics.player1.currentLegScore = 0;
        this.statistics.player2.currentLegThrows = 0;
        this.statistics.player2.currentLegScore = 0;

        console.log('🔄 [DART-STATS] Leg stats reset for both players (new leg)');
    }

    /**
     * Get the player who made the previous throw (before switch)
     */
    getPreviousPlayer() {
        // Since processThrow switches players, the "other" player made the throw
        return this.core.gameState.currentPlayer === 1 ? 2 : 1;
    }

    /**
     * Finalize statistics when match ends
     */
    finalizeStatistics() {
        this.statistics.matchStatistics.endTime = new Date();

        if (this.statistics.matchStatistics.startTime) {
            this.statistics.matchStatistics.matchDuration =
                this.statistics.matchStatistics.endTime - this.statistics.matchStatistics.startTime;
        }

        this.statistics.matchStatistics.totalLegs =
            this.statistics.player1.legs + this.statistics.player2.legs;

        this.statistics.matchStatistics.totalSets =
            this.statistics.player1.sets + this.statistics.player2.sets;

        console.log('🏁 [DART-STATS] Statistics finalized:', this.statistics);
    }

    /**
     * Generate match result object for submission
     */
    generateMatchResult() {
        this.finalizeStatistics();

        const matchData = this.core.matchData;
        const gameState = this.core.gameState;
        const gameRules = this.core.gameRules;

        // 🔧 KORRIGIERT: Robuste Game Rules Analyse
        // Verschiedene Namenskonventionen berücksichtigen
        const playWithSets = this.determinePlayWithSets(gameRules);
        const legsToWin = this.determineLegsToWin(gameRules);
        const setsToWin = this.determineSetsToWin(gameRules, playWithSets);

        console.log('🎲 [DART-STATS] Analyzed Game Rules:', {
            originalRules: gameRules,
            playWithSets,
            legsToWin,
            setsToWin,
            player1Stats: { legs: gameState.player1.legs, sets: gameState.player1.sets },
            player2Stats: { legs: gameState.player2.legs, sets: gameState.player2.sets }
        });

        // Gewinner-Bestimmung basierend auf analysiertem Format
        let player1Won = false;
        if (playWithSets) {
            // Sets-basierte Gewinner-Bestimmung
            player1Won = gameState.player1.sets > gameState.player2.sets;
            console.log('🎯 [DART-STATS] Sets mode - P1:', gameState.player1.sets, 'vs P2:', gameState.player2.sets);
        } else {
            // Legs-basierte Gewinner-Bestimmung
            player1Won = gameState.player1.legs > gameState.player2.legs;
            console.log('🎯 [DART-STATS] Legs mode - P1:', gameState.player1.legs, 'vs P2:', gameState.player2.legs);
        }

        const winner = player1Won ? 1 : 2;

        const matchResult = {
            // Basic match result (compatible with existing system)
            player1Sets: gameState.player1.sets,
            player2Sets: gameState.player2.sets,
            player1Legs: gameState.player1.legs,
            player2Legs: gameState.player2.legs,
            winner: winner === 1 ? matchData.player1.name : matchData.player2.name,
            winnerPlayerNumber: winner,

            // Enhanced dart scoring specific data
            dartScoringResult: {
                // Player Statistics
                player1Stats: {
                    name: matchData.player1.name,
                    average: Math.round(this.statistics.player1.average * 10) / 10,
                    legs: this.statistics.player1.legs,
                    sets: this.statistics.player1.sets,
                    totalThrows: this.statistics.player1.totalThrows,
                    totalScore: this.statistics.player1.totalScore,
                    maximums: this.statistics.player1.maximums.length,
                    maximumDetails: this.statistics.player1.maximums,
                    highFinishes: this.statistics.player1.highFinishes.length,
                    highFinishDetails: this.statistics.player1.highFinishes,
                    score26Count: this.statistics.player1.score26.length,
                    score26Details: this.statistics.player1.score26,
                    checkouts: this.statistics.player1.checkouts.length,
                    checkoutDetails: this.statistics.player1.checkouts,
                    // 🆕 NEU: Leg-spezifische Averages
                    legAverages: this.statistics.player1.legAverages,
                    legAveragesCount: this.statistics.player1.legAverages.length,
                    averageLegAverage: this.calculateAverageLegAverage(this.statistics.player1.legAverages)
                },
                player2Stats: {
                    name: matchData.player2.name,
                    average: Math.round(this.statistics.player2.average * 10) / 10,
                    legs: this.statistics.player2.legs,
                    sets: this.statistics.player2.sets,
                    totalThrows: this.statistics.player2.totalThrows,
                    totalScore: this.statistics.player2.totalScore,
                    maximums: this.statistics.player2.maximums.length,
                    maximumDetails: this.statistics.player2.maximums,
                    highFinishes: this.statistics.player2.highFinishes.length,
                    highFinishDetails: this.statistics.player2.highFinishes,
                    score26Count: this.statistics.player2.score26.length,
                    score26Details: this.statistics.player2.score26,
                    checkouts: this.statistics.player2.checkouts.length,
                    checkoutDetails: this.statistics.player2.checkouts,
                    // 🆕 NEU: Leg-spezifische Averages
                    legAverages: this.statistics.player2.legAverages,
                    legAveragesCount: this.statistics.player2.legAverages.length,
                    averageLegAverage: this.calculateAverageLegAverage(this.statistics.player2.legAverages)
                },

                // 🔧 KORRIGIERT: Normalisierte Game Rules
                gameRules: {
                    gameMode: (gameRules && gameRules.gameMode) || 'Game501',
                    startingScore: this.core.getStartingScore(),
                    legsToWin: legsToWin,
                    setsToWin: playWithSets ? setsToWin : 0, // 0 wenn keine Sets
                    playWithSets: playWithSets,
                    usesSets: playWithSets, // Alias für Kompatibilität
                    doubleOut: this.parseDoubleOut(gameRules)
                },

                // Match Statistics
                matchDuration: this.statistics.matchStatistics.matchDuration,
                totalLegs: this.statistics.matchStatistics.totalLegs,
                totalSets: this.statistics.matchStatistics.totalSets,
                startTime: (this.statistics.matchStatistics.startTime && this.statistics.matchStatistics.startTime.toISOString) ? this.statistics.matchStatistics.startTime.toISOString() : null,
                endTime: (this.statistics.matchStatistics.endTime && this.statistics.matchStatistics.endTime.toISOString) ? this.statistics.matchStatistics.endTime.toISOString() : null,

                // Technical Info
                submittedVia: 'DartScoringAdvanced',
                submissionTimestamp: new Date().toISOString(),
                version: '1.2.0' // Version erhöht für robuste Game Rules
            },

            // Legacy compatibility
            notes: this.generateMatchNotes(playWithSets, legsToWin, setsToWin),
            submittedVia: 'DartScoringAdvanced',
            timestamp: new Date().toISOString()
        };

        console.log('📊 [DART-STATS] Generated enhanced match result with robust game rules:', matchResult);
        return matchResult;
    }

    /**
     * Bestimme ob Sets gespielt werden (robuste Erkennung)
     */
    determinePlayWithSets(gameRules) {
        if (!gameRules) return false;

        // Direkte Boolean-Werte
        if (typeof gameRules.playWithSets === 'boolean') return gameRules.playWithSets;
        if (typeof gameRules.usesSets === 'boolean') return gameRules.usesSets;

        // Indirekte Erkennung über setsToWin
        if (typeof gameRules.setsToWin === 'number' && gameRules.setsToWin > 1) return true;

        // String-Werte
        if (gameRules.playWithSets === 'true') return true;
        if (gameRules.usesSets === 'true') return true;

        console.log('⚙️ [DART-STATS] determinePlayWithSets: Default to false for rules:', gameRules);
        return false;
    }

    /**
     * Bestimme Legs to Win (robuste Erkennung)
     */
    determineLegsToWin(gameRules) {
        if (!gameRules) return 2;

        // Verschiedene Namenskonventionen
        if (typeof gameRules.legsToWinSet === 'number' && gameRules.legsToWinSet > 0) {
            return gameRules.legsToWinSet;
        }

        if (typeof gameRules.legsToWin === 'number' && gameRules.legsToWin > 0) {
            return gameRules.legsToWin;
        }

        if (typeof gameRules.legsPerSet === 'number' && gameRules.legsPerSet > 0) {
            return gameRules.legsPerSet;
        }

        // String-Parsing
        if (typeof gameRules.legsToWin === 'string') {
            const parsed = parseInt(gameRules.legsToWin);
            if (!isNaN(parsed) && parsed > 0) return parsed;
        }

        console.log('⚙️ [DART-STATS] determineLegsToWin: Default to 2 for rules:', gameRules);
        return 2; // Fallback
    }

    /**
     * Bestimme Sets to Win (robuste Erkennung)
     */
    determineSetsToWin(gameRules, playWithSets) {
        if (!gameRules || !playWithSets) return 0;

        if (typeof gameRules.setsToWin === 'number' && gameRules.setsToWin > 0) {
            return gameRules.setsToWin;
        }

        // String-Parsing
        if (typeof gameRules.setsToWin === 'string') {
            const parsed = parseInt(gameRules.setsToWin);
            if (!isNaN(parsed) && parsed > 0) return parsed;
        }

        console.log('⚙️ [DART-STATS] determineSetsToWin: Default to 1 for rules:', gameRules);
        return 1; // Fallback wenn Sets gespielt werden
    }

    /**
     * Parse Double Out Rule (robuste Erkennung)
     */
    parseDoubleOut(gameRules) {
        if (!gameRules) return false;

        if (typeof gameRules.doubleOut === 'boolean') return gameRules.doubleOut;
        if (gameRules.doubleOut === 'true') return true;
        if (gameRules.doubleOut === 'false') return false;

        // Auch finishMode berücksichtigen
        if (gameRules.finishMode === 'DoubleOut') return true;
        if (gameRules.finishMode === 'SingleOut') return false;

        return false; // Fallback
    }

    /**
     * 📊 NEU: Berechne durchschnittlichen Leg Average
     */
    calculateAverageLegAverage(legAverages) {
        if (!legAverages || legAverages.length === 0) return 0;

        const totalAverage = legAverages.reduce((sum, leg) => sum + leg.average, 0);
        return Math.round((totalAverage / legAverages.length) * 10) / 10;
    }

    /**
     * Generate human-readable match notes
     */
    generateMatchNotes(playWithSets, legsToWin, setsToWin) {
        const p1Stats = this.statistics.player1;
        const p2Stats = this.statistics.player2;
        const gameRules = this.core.gameRules;

        let notes = [];

        // 🔧 KORRIGIERT: Verwende übergebene Parameter statt erneute Analyse
        let format;
        if (playWithSets) {
            format = `${p1Stats.sets}-${p2Stats.sets} Sets (${p1Stats.legs}-${p2Stats.legs} Legs total)`;
        } else {
            format = `${p1Stats.legs}-${p2Stats.legs} Legs only`;
        }
        notes.push(`Result: ${format}`);

        // Averages
        notes.push(`Averages: ${this.core.getPlayerName(1)} ${p1Stats.average.toFixed(1)}, ${this.core.getPlayerName(2)} ${p2Stats.average.toFixed(1)}`);

        // 180s
        const total180s = p1Stats.maximums.length + p2Stats.maximums.length;
        if (total180s > 0) {
            notes.push(`180s: ${this.core.getPlayerName(1)} ${p1Stats.maximums.length}, ${this.core.getPlayerName(2)} ${p2Stats.maximums.length}`);
        }

        // High finishes (>=100)
        const totalHighFinishes = p1Stats.highFinishes.length + p2Stats.highFinishes.length;
        if (totalHighFinishes > 0) {
            notes.push(`High Finishes (>=100): ${this.core.getPlayerName(1)} ${p1Stats.highFinishes.length}, ${this.core.getPlayerName(2)} ${p2Stats.highFinishes.length}`);
        }

        // 26 Scores
        const total26Scores = p1Stats.score26.length + p2Stats.score26.length;
        if (total26Scores > 0) {
            notes.push(`26 Scores: ${this.core.getPlayerName(1)} ${p1Stats.score26.length}, ${this.core.getPlayerName(2)} ${p2Stats.score26.length}`);
        }

        // Match duration
        if (this.statistics.matchStatistics.matchDuration) {
            const minutes = Math.floor(this.statistics.matchStatistics.matchDuration / (1000 * 60));
            notes.push(`Duration: ${minutes} minutes`);
        }

        // 🔧 KORRIGIERT: Game Rules Info mit übergebenen Parametern
        const startingScore = this.core.getStartingScore();
        const doubleOutInfo = this.parseDoubleOut(gameRules) ? ' Double-Out' : ' Single-Out';
        const formatInfo = playWithSets ?
            ` (First to ${setsToWin} Sets)` :
            ` (First to ${legsToWin} Legs)`;

        notes.push(`Format: ${startingScore}${doubleOutInfo}${formatInfo}`);

        notes.push('Submitted via Advanced Dart Scoring');

        return notes.join(' � ');
    }

    /**
     * Submit match result with enhanced statistics
     */
    async submitMatchResult() {
        if (!this.core.gameState.isGameFinished) {
            return {
                success: false,
                message: 'Match ist noch nicht beendet'
            };
        }

        try {
            const matchResult = this.generateMatchResult();

            console.log('📤 [DART-STATS] Submitting enhanced match result:', matchResult);

            // Submit via REST API (same endpoint as regular match result)
            const response = await fetch(`/api/match/${this.core.matchData.tournamentId}/${this.core.matchData.uniqueId || this.core.matchData.matchId}/result`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(matchResult)
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();

            if (!data.success) {
                throw new Error(data.message || 'Failed to submit result');
            }

            // Also send via Socket.IO for real-time updates
            if (this.core.socket) {
                this.core.socket.emit('submitMatchResult', {
                    tournamentId: this.core.matchData.tournamentId,
                    matchId: this.core.matchData.uniqueId || this.core.matchData.matchId,
                    ...matchResult
                });
            }

            console.log('✅ [DART-STATS] Enhanced match result submitted successfully');

            return {
                success: true,
                message: 'Match-Ergebnis erfolgreich übermittelt',
                statistics: this.statistics
            };

        } catch (error) {
            console.error('❌ [DART-STATS] Error submitting enhanced match result:', error);
            return {
                success: false,
                message: `Fehler beim Übermitteln: ${error.message}`
            };
        }
    }

    /**
     * Get current statistics summary for display
     */
    getCurrentStatistics() {
        return {
            player1: {
                name: this.core.getPlayerName(1),
                average: this.statistics.player1.average.toFixed(1),
                maximums: this.statistics.player1.maximums.length,
                highFinishes: this.statistics.player1.highFinishes.length,
                score26: this.statistics.player1.score26.length,
                checkouts: this.statistics.player1.checkouts.length
            },
            player2: {
                name: this.core.getPlayerName(2),
                average: this.statistics.player2.average.toFixed(1),
                maximums: this.statistics.player2.maximums.length,
                highFinishes: this.statistics.player2.highFinishes.length,
                score26: this.statistics.player2.score26.length,
                checkouts: this.statistics.player2.checkouts.length
            }
        };
    }

    /**
     * Stop tracking statistics
     */
    stopTracking() {
        this.isTracking = false;
        this.finalizeStatistics();
        console.log('🛑 [DART-STATS] Statistics tracking stopped');
    }

    /**
     * Reset statistics for new match
     */
    reset() {
        this.statistics = {
            player1: {
                maximums: [],
                highFinishes: [],
                score26: [],
                totalThrows: 0,
                totalScore: 0,
                legs: 0,
                sets: 0,
                average: 0,
                checkouts: [],
                // ? NEU: Leg-spezifische Averages
                legAverages: [],
                currentLegThrows: 0,
                currentLegScore: 0
            },
            player2: {
                maximums: [],
                highFinishes: [],
                score26: [],
                totalThrows: 0,
                totalScore: 0,
                legs: 0,
                sets: 0,
                average: 0,
                checkouts: [],
                // ? NEU: Leg-spezifische Averages
                legAverages: [],
                currentLegThrows: 0,
                currentLegScore: 0
            },
            matchStatistics: {
                totalLegs: 0,
                totalSets: 0,
                matchDuration: 0,
                startTime: null,
                endTime: null
            }
        };

        this.isTracking = false;
        console.log('🔄 [DART-STATS] Statistics reset');
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringStats;
} else {
    window.DartScoringStats = DartScoringStats;
}

console.log('📊 [DART-STATS] Dart Scoring Statistics module loaded');