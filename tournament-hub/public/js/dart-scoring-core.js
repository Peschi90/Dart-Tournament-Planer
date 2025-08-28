/**
 * Dart Scoring Core Module
 * Handles dart game logic, scoring calculations, and game state management
 */
class DartScoringCore {
    constructor() {
        this.matchData = null;
        this.gameRules = null;
        this.gameState = {
            currentPlayer: 1,
            player1: {
                score: 501,
                legs: 0,
                sets: 0,
                throws: [],
                totalThrows: 0,
                totalScore: 0
            },
            player2: {
                score: 501,
                legs: 0,
                sets: 0,
                throws: [],
                totalThrows: 0,
                totalScore: 0
            },
            currentLeg: 1,
            currentSet: 1,
            isGameFinished: false,
            throwHistory: []
        };
        
        this.socket = null;
        
        console.log('🎯 [DART-CORE] Dart Scoring Core initialized');
    }

    /**
     * Initialize dart scoring with match data
     */
    async initialize(matchId, tournamentId) {
        try {
            console.log('🔄 [DART-CORE] Initializing dart scoring...', { matchId, tournamentId });
            
            // Connect to Socket.IO
            this.socket = io();
            this.setupSocketListeners();
            
            // Load match data
            await this.loadMatchData(matchId, tournamentId);
            
            // Initialize game state based on rules
            this.initializeGameState();
            
            console.log('✅ [DART-CORE] Dart scoring initialized successfully');
            return true;
        } catch (error) {
            console.error('❌ [DART-CORE] Failed to initialize dart scoring:', error);
            return false;
        }
    }

    /**
     * Load match data from server
     */
    async loadMatchData(matchId, tournamentId) {
        try {
            const response = await fetch(`/api/match/${tournamentId}/${matchId}?uuid=true`);
            
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            
            const data = await response.json();
            
            if (!data.success) {
                throw new Error(data.message || 'Failed to load match data');
            }
            
            this.matchData = data.match;
            this.gameRules = data.gameRules;
            
            console.log('📄 [DART-CORE] Match data loaded:', {
                match: this.matchData.displayName,
                gameMode: this.gameRules.gameMode,
                legsToWin: this.gameRules.legsToWin,
                setsToWin: this.gameRules.setsToWin,
                doubleOut: this.gameRules.doubleOut
            });
            
        } catch (error) {
            console.error('❌ [DART-CORE] Failed to load match data:', error);
            throw error;
        }
    }

    /**
     * Initialize game state based on game rules
     */
    initializeGameState() {
        const startingScore = this.getStartingScore();
        
        this.gameState.player1.score = startingScore;
        this.gameState.player2.score = startingScore;
        
        // ✅ NEU: Tracking für Leg- und Set-Startspieler
        this.gameState.legStartPlayer = 1; // Wer das aktuelle Leg gestartet hat
        this.gameState.setStartPlayer = 1; // Wer das aktuelle Set gestartet hat
        this.gameState.currentPlayer = 1;
        
        console.log('🎮 [DART-CORE] Game state initialized:', {
            startingScore,
            gameMode: this.gameRules?.gameMode,
            doubleOut: this.gameRules?.doubleOut,
            legStartPlayer: this.gameState.legStartPlayer,
            setStartPlayer: this.gameState.setStartPlayer
        });
    }

    /**
     * Get starting score based on game mode
     */
    getStartingScore() {
        if (!this.gameRules) return 501;
        
        switch (this.gameRules.gameMode) {
            case 'Game301': return 301;
            case 'Game401': return 401;
            case 'Game501': return 501;
            case 'Game701': return 701;
            case 'Game1001': return 1001;
            default: return 501;
        }
    }

    /**
     * Process a dart throw
     */
    processThrow(dart1, dart2, dart3) {
        try {
            // Validate throw
            const validation = this.validateThrow(dart1, dart2, dart3);
            if (!validation.valid) {
                return validation;
            }

            const throwScore = dart1 + dart2 + dart3;
            const currentPlayer = this.getCurrentPlayer();
            const newScore = currentPlayer.score - throwScore;
            
            // ✅ ERWEITERT: Bestimme aktuellen Spieler VOR Änderungen
            const throwingPlayerNumber = this.gameState.currentPlayer;

            console.log('🎯 [DART-CORE] Processing throw:', {
                player: throwingPlayerNumber,
                darts: [dart1, dart2, dart3],
                total: throwScore,
                oldScore: currentPlayer.score,
                newScore
            });

            // Bestimme welcher Dart der letzte war (für Double-Out)
            let lastDartScore = 0;
            if (dart3 > 0) lastDartScore = dart3;
            else if (dart2 > 0) lastDartScore = dart2;
            else if (dart1 > 0) lastDartScore = dart1;

            // Check for bust
            if (this.isBust(newScore, lastDartScore)) {
                console.log('💥 [DART-CORE] Bust! Score reset to previous value for player', throwingPlayerNumber);
                
                // Record bust throw
                const throwEntry = {
                    player: throwingPlayerNumber, // ✅ KORRIGIERT: Spieler VOR Wechsel
                    darts: [dart1, dart2, dart3],
                    total: throwScore,
                    previousScore: currentPlayer.score,
                    newScore: currentPlayer.score, // Score stays the same
                    isBust: true,
                    lastDart: lastDartScore,
                    timestamp: new Date()
                };
                
                this.gameState.throwHistory.unshift(throwEntry);
                currentPlayer.throws.push(throwEntry);
                
                // Switch player NACH Bust-Recording
                this.switchPlayer();
                
                return {
                    success: true,
                    type: 'bust',
                    message: 'Überworfen! Nächster Spieler ist dran.',
                    bustedPlayer: throwingPlayerNumber, // ✅ NEU: Info über überworfenen Spieler
                    gameState: this.gameState
                };
            }

            // Check for finish
            if (newScore === 0) {
                console.log('🎉 [DART-CORE] Leg finished by player', throwingPlayerNumber);
                
                // Update score
                currentPlayer.score = newScore;
                
                // Record winning throw
                const throwEntry = {
                    player: throwingPlayerNumber,
                    darts: [dart1, dart2, dart3],
                    total: throwScore,
                    previousScore: currentPlayer.score + throwScore,
                    newScore: 0,
                    isWinning: true,
                    lastDart: lastDartScore,
                    doubleOut: this.gameRules?.doubleOut && this.isValidDouble(lastDartScore),
                    timestamp: new Date()
                };
                
                this.gameState.throwHistory.unshift(throwEntry);
                currentPlayer.throws.push(throwEntry);
                currentPlayer.totalThrows += this.countDartsUsed(dart1, dart2, dart3);
                currentPlayer.totalScore += throwScore;
                
                // Award leg
                currentPlayer.legs++;
                
                const result = this.checkGameCompletion();
                
                return {
                    success: true,
                    type: 'leg_won',
                    message: `${this.getPlayerName(throwingPlayerNumber)} gewinnt das Leg!`,
                    winner: throwingPlayerNumber, // ✅ NEU: Explizite Winner-Info
                    gameState: this.gameState,
                    gameResult: result
                };
            }

            // Normal throw - update score
            currentPlayer.score = newScore;
            
            // Record throw
            const throwEntry = {
                player: throwingPlayerNumber,
                darts: [dart1, dart2, dart3],
                total: throwScore,
                previousScore: currentPlayer.score + throwScore,
                newScore: currentPlayer.score,
                lastDart: lastDartScore,
                timestamp: new Date()
            };
            
            this.gameState.throwHistory.unshift(throwEntry);
            currentPlayer.throws.push(throwEntry);
            currentPlayer.totalThrows += this.countDartsUsed(dart1, dart2, dart3);
            currentPlayer.totalScore += throwScore;
            
            // Switch player
            this.switchPlayer();
            
            return {
                success: true,
                type: 'normal',
                message: 'Wurf registriert',
                gameState: this.gameState
            };
            
        } catch (error) {
            console.error('❌ [DART-CORE] Error processing throw:', error);
            return {
                success: false,
                message: 'Fehler beim Verarbeiten des Wurfs'
            };
        }
    }

    /**
     * Check if a dart score is a valid double
     */
    isValidDouble(dartScore) {
        // Valid doubles: D1-D20 (2,4,6...40) and Double-Bull (50)
        const validDoubles = [2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 50];
        return validDoubles.includes(dartScore);
    }

    /**
     * Validate dart throw
     */
    validateThrow(dart1, dart2, dart3) {
        // Convert to numbers
        dart1 = parseInt(dart1) || 0;
        dart2 = parseInt(dart2) || 0;
        dart3 = parseInt(dart3) || 0;

        // Check individual dart scores
        if (dart1 < 0 || dart1 > 60) {
            return { valid: false, message: '1. Dart muss zwischen 0 und 60 sein' };
        }
        if (dart2 < 0 || dart2 > 60) {
            return { valid: false, message: '2. Dart muss zwischen 0 und 60 sein' };
        }
        if (dart3 < 0 || dart3 > 60) {
            return { valid: false, message: '3. Dart muss zwischen 0 und 60 sein' };
        }

        const total = dart1 + dart2 + dart3;
        if (total > 180) {
            return { valid: false, message: 'Maximaler Wurf ist 180 Punkte' };
        }

        return { valid: true };
    }

    /**
     * Check if score is bust
     */
    isBust(newScore, lastDart) {
        if (newScore < 0) return true;
        
        // Double out rule - NEU: Erweiterte Double-Out-Logik
        if (this.gameRules?.doubleOut && newScore === 0) {
            // Prüfe ob der letzte Dart ein gültiges Double war
            // Vereinfachte Logik: Gerade Zahlen 2-40 oder Bullseye (50) gelten als Double
            const validDoubles = [2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 50];
            
            if (!validDoubles.includes(lastDart)) {
                console.log(`🚫 [DART-CORE] Double-Out required but last dart ${lastDart} is not a valid double`);
                return true; // Bust - kein gültiges Double
            } else {
                console.log(`✅ [DART-CORE] Valid double finish with ${lastDart}`);
            }
        }
        
        // Can't finish on 1 with double out
        if (this.gameRules?.doubleOut && newScore === 1) {
            console.log(`🚫 [DART-CORE] Can't finish on 1 with double-out rule`);
            return true;
        }
        
        return false;
    }

    /**
     * Count how many darts were actually used - ✅ KORRIGIERT für Average
     */
    countDartsUsed(dart1, dart2, dart3) {
        // ✅ NEU: Für Average-Berechnung zählen wir immer alle 3 Darts eines Wurfs
        // auch wenn weniger geworfen wurden (wie im echten Dart)
        let actualDartsThrown = 0;
        
        // Zähle tatsächlich geworfene Darts
        if (dart1 > 0 || dart1 === 0) actualDartsThrown++; // 0 = Miss zählt als geworfener Dart
        if (dart2 > 0 || dart2 === 0) actualDartsThrown++;
        if (dart3 > 0 || dart3 === 0) actualDartsThrown++;
        
        // Für Average: Immer 3 Darts pro Wurf zählen (Standard im Dart-Sport)
        const dartsForAverage = 3;
        
        console.log(`🎯 [DART-CORE] Darts thrown: ${actualDartsThrown}, counted for average: ${dartsForAverage} from [${dart1}, ${dart2}, ${dart3}]`);
        return dartsForAverage;
    }

    /**
     * Switch to next player
     */
    switchPlayer() {
        this.gameState.currentPlayer = this.gameState.currentPlayer === 1 ? 2 : 1;
        console.log('🔄 [DART-CORE] Switched to player', this.gameState.currentPlayer);
    }

    /**
     * Get current player object
     */
    getCurrentPlayer() {
        return this.gameState.currentPlayer === 1 ? this.gameState.player1 : this.gameState.player2;
    }

    /**
     * Get player name
     */
    getPlayerName(playerNumber) {
        if (!this.matchData) return `Spieler ${playerNumber}`;
        
        if (playerNumber === 1) {
            return this.matchData.player1?.name || 'Spieler 1';
        } else {
            return this.matchData.player2?.name || 'Spieler 2';
        }
    }

    /**
     * Calculate player average - ✅ KORRIGIERT: Pro Leg basierend
     */
    getPlayerAverage(playerNumber) {
        const player = playerNumber === 1 ? this.gameState.player1 : this.gameState.player2;
        
        if (player.totalThrows === 0) return 0;
        
        // ✅ NEU: Average-Berechnung: Gesamtscore / (Anzahl Darts / 3) 
        // Beispiel: 300 Punkte mit 6 Darts = 300 / (6/3) = 300/2 = 150 Average
        const dartsPerTurn = 3;
        const turns = player.totalThrows / dartsPerTurn;
        
        if (turns === 0) return 0;
        
        const average = player.totalScore / turns;
        
        console.log(`📊 [DART-CORE] Player ${playerNumber} average: ${player.totalScore} points / ${turns} turns (${player.totalThrows} darts) = ${average.toFixed(1)}`);
        
        return Math.round(average * 10) / 10; // Runde auf 1 Dezimalstelle
    }

    /**
     * Undo last throw
     */
    undoLastThrow() {
        try {
            if (this.gameState.throwHistory.length === 0) {
                return {
                    success: false,
                    message: 'Keine Würfe zum Rückgängigmachen vorhanden'
                };
            }

            const lastThrow = this.gameState.throwHistory[0];
            console.log('↺ [DART-CORE] Undoing throw:', lastThrow);

            // Remove from history
            this.gameState.throwHistory.shift();
            
            // Get the player who made the throw
            const player = lastThrow.player === 1 ? this.gameState.player1 : this.gameState.player2;
            
            // Remove from player's throws
            player.throws.pop();
            
            // Restore score
            player.score = lastThrow.previousScore;
            
            // Restore statistics
            player.totalScore -= lastThrow.total;
            player.totalThrows -= this.countDartsUsed(...lastThrow.darts);
            
            // Handle leg restoration if it was a winning throw
            if (lastThrow.isWinning) {
                player.legs--;
            }
            
            // Switch back to the player who made the throw
            this.gameState.currentPlayer = lastThrow.player;
            
            return {
                success: true,
                message: 'Letzter Wurf rückgängig gemacht',
                gameState: this.gameState
            };
            
        } catch (error) {
            console.error('❌ [DART-CORE] Error undoing throw:', error);
            return {
                success: false,
                message: 'Fehler beim Rückgängigmachen'
            };
        }
    }

    /**
     * Start new leg
     */
    startNewLeg() {
        console.log('🆕 [DART-CORE] Starting new leg');
        
        const startingScore = this.getStartingScore();
        
        // Reset scores
        this.gameState.player1.score = startingScore;
        this.gameState.player2.score = startingScore;
        
        // ✅ NEU: Anwurf-Wechsel für neues Leg
        // Der Spieler der das letzte Leg NICHT gestartet hat, startet das neue Leg
        this.gameState.legStartPlayer = this.gameState.legStartPlayer === 1 ? 2 : 1;
        this.gameState.currentPlayer = this.gameState.legStartPlayer;
        
        // Increment leg counter
        this.gameState.currentLeg++;
        
        // Clear throw history for this leg (keep overall history)
        this.gameState.throwHistory = [];
        
        console.log('🔄 [DART-CORE] New leg - start player switched:', {
            legStartPlayer: this.gameState.legStartPlayer,
            currentPlayer: this.gameState.currentPlayer
        });
        
        return {
            success: true,
            message: `Neues Leg gestartet - ${this.getPlayerName(this.gameState.currentPlayer)} hat Anwurf`,
            gameState: this.gameState
        };
    }

    /**
     * Check if game/match is completed
     */
    checkGameCompletion() {
        const currentPlayer = this.getCurrentPlayer();
        
        // ✅ KORRIGIERT: Verwende tatsächliche Game Rules statt fest codierte Werte
        const legsToWin = this.gameRules?.legsToWinSet || this.gameRules?.legsToWin || 2;
        const setsToWin = this.gameRules?.setsToWin || 1;
        
        console.log('🔍 [DART-CORE] Checking game completion:', {
            currentPlayerLegs: currentPlayer.legs,
            legsToWin: legsToWin,
            currentPlayerSets: currentPlayer.sets,
            setsToWin: setsToWin,
            gameRules: this.gameRules
        });

        // Check if player won the set
        if (currentPlayer.legs >= legsToWin) {
            currentPlayer.sets++;
            
            // Set-Anwurf-Logik
            this.gameState.setStartPlayer = this.gameState.setStartPlayer === 1 ? 2 : 1;
            this.gameState.legStartPlayer = this.gameState.setStartPlayer;
            
            // Reset legs for new set
            const otherPlayer = currentPlayer === this.gameState.player1 ? this.gameState.player2 : this.gameState.player1;
            currentPlayer.legs = 0;
            otherPlayer.legs = 0;
            this.gameState.currentSet++;
            
            console.log('🏆 [DART-CORE] Set won by player', this.gameState.currentPlayer);
            
            // Check if player won the match
            if (currentPlayer.sets >= setsToWin) {
                this.gameState.isGameFinished = true;
                console.log('🥇 [DART-CORE] Match won by player', this.gameState.currentPlayer);
                
                return {
                    type: 'match_won',
                    winner: this.gameState.currentPlayer,
                    winnerName: this.getPlayerName(this.gameState.currentPlayer)
                };
            }
            
            return {
                type: 'set_won',
                winner: this.gameState.currentPlayer,
                winnerName: this.getPlayerName(this.gameState.currentPlayer),
                newSetStartPlayer: this.gameState.setStartPlayer
            };
        }

        // ✅ NEU: Prüfe Match-Ende wenn nur ein Set gespielt wird (First to X Legs)
        if (setsToWin === 1 && currentPlayer.legs >= legsToWin) {
            this.gameState.isGameFinished = true;
            console.log('🥇 [DART-CORE] Match won by player', this.gameState.currentPlayer, '(First to', legsToWin, 'legs)');
            
            return {
                type: 'match_won',
                winner: this.gameState.currentPlayer,
                winnerName: this.getPlayerName(this.gameState.currentPlayer)
            };
        }

        return null;
    }

    /**
     * Get possible finishes for a given score
     */
    getPossibleFinishes(score) {
        if (score > 170 || score < 2) return [];
        
        const finishes = [];
        
        // Common finish combinations
        const commonFinishes = {
            170: ['T20-T20-Bull'],
            167: ['T20-T19-Bull'],
            164: ['T20-T18-Bull', 'T19-T19-Bull'],
            161: ['T20-T17-Bull'],
            160: ['T20-T20-D20'],
            158: ['T20-T20-D19'],
            157: ['T20-T19-D20'],
            156: ['T20-T20-D18'],
            155: ['T20-T19-D19'],
            154: ['T20-T18-D20'],
            153: ['T20-T19-D18'],
            152: ['T20-T20-D16'],
            151: ['T20-T17-D20'],
            150: ['T20-T18-D18'],
            149: ['T20-T19-D16'],
            148: ['T20-T20-D14'],
            147: ['T20-T17-D18'],
            146: ['T20-T18-D16'],
            145: ['T20-T19-D14'],
            144: ['T20-T20-D12'],
            143: ['T20-T17-D16'],
            142: ['T20-T18-D14'],
            141: ['T20-T19-D12'],
            140: ['T20-T20-D10'],
            139: ['T20-T17-D14'],
            138: ['T20-T18-D12'],
            137: ['T20-T19-D10'],
            136: ['T20-T20-D8'],
            135: ['T20-T17-D12'],
            134: ['T20-T18-D10'],
            133: ['T20-T19-D8'],
            132: ['T20-T20-D6'],
            131: ['T20-T17-D10'],
            130: ['T20-T18-D8'],
            129: ['T19-T16-D12'],
            128: ['T18-T14-D16'],
            127: ['T20-T17-D8'],
            126: ['T19-T17-D8'],
            125: ['T18-T19-D8'],
            124: ['T20-T16-D8'],
            123: ['T19-T16-D9'],
            122: ['T18-T20-D4'],
            121: ['T17-T18-D8'],
            120: ['T20-S20-D20'],
            110: ['T20-S18-D16'],
            100: ['T20-D20'],
            90: ['T18-D18'],
            80: ['T20-D10'],
            70: ['T18-D8'],
            60: ['S20-D20'],
            50: ['S18-D16'],
            40: ['D20'],
            32: ['D16'],
            24: ['D12'],
            16: ['D8'],
            8: ['D4'],
            4: ['D2'],
            2: ['D1']
        };

        if (commonFinishes[score]) {
            finishes.push(...commonFinishes[score]);
        }

        // Add simple double finishes for even numbers
        if (score % 2 === 0 && score <= 40) {
            const double = score / 2;
            if (double <= 20) {
                finishes.push(`D${double}`);
            }
        }

        return finishes.slice(0, 3); // Show max 3 options
    }

    /**
     * Setup Socket.IO listeners
     */
    setupSocketListeners() {
        if (!this.socket) return;
        
        this.socket.on('connect', () => {
            console.log('🔌 [DART-CORE] Connected to tournament hub');
        });
        
        this.socket.on('disconnect', () => {
            console.log('🔌 [DART-CORE] Disconnected from tournament hub');
        });
        
        // Listen for external match updates
        this.socket.on('matchUpdated', (data) => {
            console.log('📡 [DART-CORE] External match update received:', data);
            // Handle external updates if needed
        });
    }

    /**
     * Send match result to server
     */
    async submitMatchResult() {
        if (!this.gameState.isGameFinished) {
            return {
                success: false,
                message: 'Match ist noch nicht beendet'
            };
        }

        try {
            const winner = this.gameState.player1.sets > this.gameState.player2.sets ? 1 : 2;
            const matchResult = {
                player1Sets: this.gameState.player1.sets,
                player2Sets: this.gameState.player2.sets,
                player1Legs: this.gameState.player1.legs,
                player2Legs: this.gameState.player2.legs,
                winner: winner === 1 ? this.matchData.player1.name : this.matchData.player2.name,
                notes: `Dart-Scoring Result: ${this.gameState.player1.sets}-${this.gameState.player2.sets} Sets, ${this.gameState.player1.legs}-${this.gameState.player2.legs} Legs (total)`,
                submittedVia: 'DartScoring',
                timestamp: new Date().toISOString()
            };

            console.log('📤 [DART-CORE] Submitting match result:', matchResult);

            // Submit via REST API
            const response = await fetch(`/api/match/${this.matchData.tournamentId}/${this.matchData.uniqueId || this.matchData.matchId}/result`, {
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
            if (this.socket) {
                this.socket.emit('submitMatchResult', {
                    tournamentId: this.matchData.tournamentId,
                    matchId: this.matchData.uniqueId || this.matchData.matchId,
                    ...matchResult
                });
            }

            console.log('✅ [DART-CORE] Match result submitted successfully');

            return {
                success: true,
                message: 'Match-Ergebnis erfolgreich übermittelt'
            };

        } catch (error) {
            console.error('❌ [DART-CORE] Error submitting match result:', error);
            return {
                success: false,
                message: `Fehler beim Übermitteln: ${error.message}`
            };
        }
    }

    /**
     * Get current game state
     */
    getGameState() {
        return {
            ...this.gameState,
            matchData: this.matchData,
            gameRules: this.gameRules
        };
    }

    /**
     * Cleanup resources
     */
    cleanup() {
        if (this.socket) {
            this.socket.disconnect();
            this.socket = null;
        }
        
        console.log('🧹 [DART-CORE] Dart scoring cleaned up');
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringCore;
} else {
    window.DartScoringCore = DartScoringCore;
}

console.log('🎯 [DART-CORE] Dart Scoring Core module loaded');