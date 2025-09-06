/**
 * Dart Scoring UI Module
 * Handles user interface updates and interactions
 */
class DartScoringUI {
    constructor() {
        this.core = null;
        this.elements = {};
        this.isInitialized = false;
        this.keyboard = null; // 🆕 NEU: Keyboard-Instanz
        this.animations = null; // 🆕 NEU: Animations-Instanz

        console.log('🎯 [DART-UI] Dart Scoring UI initialized');
    }

    /**
     * Initialize UI with core instance
     */
    initialize(core) {
        this.core = core;
        this.cacheElements();
        this.setupEventListeners();

        // 🆕 NEU: Initialize keyboard module
        this.keyboard = new DartScoringKeyboard(this);
        this.keyboard.initialize();

        // 🆕 NEU: Initialize animations module
        if (typeof DartScoringAnimations !== 'undefined') {
            this.animations = new DartScoringAnimations(this);
            this.animations.initialize();
        } else {
            console.warn('⚠️ [DART-UI] DartScoringAnimations module not available');
        }

        // 🏁 NEU: Initialize completion module
        if (typeof DartScoringCompletion !== 'undefined') {
            this.completion = new DartScoringCompletion();
            console.log('🏁 [DART-UI] Completion module initialized');
        } else {
            console.warn('⚠️ [DART-UI] DartScoringCompletion not found - completion handling disabled');
        }

        this.isInitialized = true;

        console.log('🎯 [DART-UI] UI initialized with core, keyboard, animations and completion modules');
    }

    /**
     * Cache DOM elements for better performance
     */
    cacheElements() {
        this.elements = {
            // Header elements
            gameTitle: document.getElementById('gameTitle'),
            gameMeta: document.getElementById('gameMeta'),
            backToMatch: document.getElementById('backToMatch'),
            backToTournament: document.getElementById('backToTournament'),

            // Game status - NEUE KOMPAKTE ELEMENTE
            gameStatus: document.getElementById('gameStatus'),
            currentLegDisplay: document.getElementById('currentLegDisplay'),
            currentSetDisplay: document.getElementById('currentSetDisplay'),
            currentSetContainer: document.getElementById('currentSetContainer'),
            gameModeDisplay: document.getElementById('gameModeDisplay'),

            // Player sections
            player1Section: document.getElementById('player1Section'),
            player1Name: document.getElementById('player1Name'),
            player1Score: document.getElementById('player1Score'),
            player1Legs: document.getElementById('player1Legs'),
            player1Sets: document.getElementById('player1Sets'),
            player1Average: document.getElementById('player1Average'),
            player1Finishes: document.getElementById('player1Finishes'),
            player1FinishOptions: document.getElementById('player1FinishOptions'),

            player2Section: document.getElementById('player2Section'),
            player2Name: document.getElementById('player2Name'),
            player2Score: document.getElementById('player2Score'),
            player2Legs: document.getElementById('player2Legs'),
            player2Sets: document.getElementById('player2Sets'),
            player2Average: document.getElementById('player2Average'),
            player2Finishes: document.getElementById('player2Finishes'),
            player2FinishOptions: document.getElementById('player2FinishOptions'),

            // Dart keypad elements - NEUE STRUKTUR mit beiden Spielern
            player1Display: document.getElementById('player1Display'),
            player1DisplayName: document.getElementById('player1DisplayName'),
            player1DisplayScore: document.getElementById('player1DisplayScore'),
            player2Display: document.getElementById('player2Display'),
            player2DisplayName: document.getElementById('player2DisplayName'),
            player2DisplayScore: document.getElementById('player2DisplayScore'),
            dart1Result: document.getElementById('dart1Result'),
            dart2Result: document.getElementById('dart2Result'),
            dart3Result: document.getElementById('dart3Result'),
            throwTotal: document.getElementById('throwTotal'),

            // Control buttons
            confirmThrow: document.getElementById('confirmThrow'),
            undoLastDart: document.getElementById('undoLastDart'),
            undoThrow: document.getElementById('undoThrow'),

            // History
            historyList: document.getElementById('historyList'),

            // BUSTED Animation
            bustedAnimation: document.getElementById('bustedAnimation'),
            bustedMessage: document.getElementById('bustedMessage'),
            bustedDetails: document.getElementById('bustedDetails'),

            // Win Animation
            winAnimation: document.getElementById('winAnimation'),
            winMessage: document.getElementById('winMessage'),
            winDetails: document.getElementById('winDetails'),
            continueFromWin: document.getElementById('continueFromWin'),

            // Modals
            victoryModal: document.getElementById('victoryModal'),
            victoryMessage: document.getElementById('victoryMessage'),
            newLegBtn: document.getElementById('newLegBtn'),
            finishMatchBtn: document.getElementById('finishMatchBtn'),

            // 📊 NEU: Match Statistics Modal Elements
            matchStatsModal: document.getElementById('matchStatsModal'),
            matchStatsContent: document.getElementById('matchStatsContent'),
            submitMatchResultBtn: document.getElementById('submitMatchResultBtn'),

            // 🏁 NEU: Match Finished Display Elements
            matchFinishedDisplay: document.getElementById('matchFinishedDisplay'),
            backToTournamentFinal: document.getElementById('backToTournamentFinal'),

            // Containers
            loadingContainer: document.getElementById('loadingContainer'),
            gameContainer: document.getElementById('gameContainer')
        };

        // Cache keypad elements - NEUE UNIFIED STRUKTUR
        this.keypadElements = {
            numberBtns: document.querySelectorAll('.number-btn'),
            multiplierBtns: document.querySelectorAll('.multiplier-btn'),
            specialBtns: document.querySelectorAll('.special-btn'),
            allKeypadBtns: document.querySelectorAll('.keypad-btn-unified')
        };
    }

    /**
     * Setup event listeners
     */
    setupEventListeners() {
        // Keypad event listeners
        this.setupKeypadListeners();

        // Control buttons
        this.elements.confirmThrow.addEventListener('click', () => this.handleConfirmThrow());
        this.elements.undoLastDart.addEventListener('click', () => this.handleUndoLastDart());
        this.elements.undoThrow.addEventListener('click', () => this.handleUndoThrow());

        // Modal buttons
        this.elements.newLegBtn.addEventListener('click', () => this.handleNewLeg());
        this.elements.finishMatchBtn.addEventListener('click', () => this.handleFinishMatch());

        // 📊 NEU: Match Statistics Modal
        this.elements.submitMatchResultBtn.addEventListener('click', () => this.handleSubmitMatchResult());
        this.elements.backToTournamentFinal.addEventListener('click', () => this.navigateBackToTournament());

        // Win Animation
        this.elements.continueFromWin.addEventListener('click', () => this.handleContinueFromWin());

        // Navigation
        this.setupNavigation();

        console.log('✅ [DART-UI] Event listeners setup complete');
    }

    /**
     * Setup keypad event listeners
     */
    setupKeypadListeners() {
        // Number buttons (1-20)
        this.keypadElements.numberBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const number = parseInt(btn.dataset.number);
                this.handleNumberInput(number);
            });
        });

        // Multiplier buttons (Single, Double, Triple)
        this.keypadElements.multiplierBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const multiplier = parseInt(btn.dataset.multiplier);
                this.handleMultiplierInput(multiplier);
            });
        });

        // Special buttons (Bull, Bullseye, Miss)
        this.keypadElements.specialBtns.forEach(btn => {
            btn.addEventListener('click', () => {
                const special = btn.dataset.special;
                this.handleSpecialInput(special);
            });
        });

        console.log('⌨️ [DART-UI] Keypad listeners setup complete');
    }

    /**
     * Initialize keypad state
     */
    initializeKeypad() {
        this.keypadState = {
            currentDart: 1, // 1, 2, or 3
            darts: [null, null, null], // Store each dart result
            selectedMultiplier: 1, // Default to single
            awaitingNumber: false,
            throwComplete: false
        };

        // Set default multiplier to single
        this.setActiveMultiplier(1);
        this.updateKeypadState();

        console.log('🔢 [DART-UI] Keypad initialized');
    }

    /**
     * Handle number input (1-20)
     */
    handleNumberInput(number) {
        // Verhindere Input nach Leg/Match-Ende
        if (this.core && this.core.gameState && this.core.gameState.isGameFinished) {
            return;
        }

        if (this.keypadState.currentDart > 3 || this.keypadState.throwComplete) {
            return;
        }

        // Verhindere Input wenn Keypad deaktiviert
        if (this.keypadElements.numberBtns[0] && this.keypadElements.numberBtns[0].classList.contains('disabled')) {
            return;
        }

        const multiplier = this.keypadState.selectedMultiplier;
        const score = number * multiplier;

        // Validate maximum score per dart (60)
        if (score > 60) {
            this.showMessage('Maximaler Wert pro Dart ist 60!', 'warning');
            return;
        }

        const dartResult = {
            number: number,
            multiplier: multiplier,
            score: score,
            display: this.formatDartDisplay(number, multiplier)
        };

        this.addDartResult(dartResult);
    }

    /**
     * Handle multiplier input (Single=1, Double=2, Triple=3)
     */
    handleMultiplierInput(multiplier) {
        // Verhindere Multiplier-Änderung nach Leg-Ende
        if (this.core && this.core.gameState && this.core.gameState.isGameFinished) {
            return;
        }

        // Verhindere Multiplier-Eingabe wenn Keypad deaktiviert
        if (this.keypadElements.multiplierBtns[0] && this.keypadElements.multiplierBtns[0].classList.contains('disabled')) {
            return;
        }

        this.keypadState.selectedMultiplier = multiplier;
        this.setActiveMultiplier(multiplier);
        console.log(`🎯 [DART-UI] Multiplier selected: ${this.getMultiplierName(multiplier)}`);
    }

    /**
     * Handle special input (Bull, Bullseye, Miss)
     */
    handleSpecialInput(special) {
        // Verhindere Input nach Leg/Match-Ende
        if (this.core && this.core.gameState && this.core.gameState.isGameFinished) {
            return;
        }

        if (this.keypadState.currentDart > 3 || this.keypadState.throwComplete) {
            return;
        }

        // Verhindere Input wenn Keypad deaktiviert
        if (this.keypadElements.specialBtns[0] && this.keypadElements.specialBtns[0].classList.contains('disabled')) {
            return;
        }

        let dartResult;

        switch (special) {
            case 'bull':
                dartResult = {
                    number: 25,
                    multiplier: 1,
                    score: 25,
                    display: 'Bull',
                    special: true
                };
                break;
            case 'bullseye':
                dartResult = {
                    number: 25,
                    multiplier: 2,
                    score: 50,
                    display: 'D-Bull',
                    special: true
                };
                break;
            case 'miss':
                dartResult = {
                    number: 0,
                    multiplier: 0,
                    score: 0,
                    display: 'Miss',
                    special: true
                };
                break;
        }

        this.addDartResult(dartResult);
    }

    /**
     * Add dart result to current throw
     */
    addDartResult(dartResult) {
        const dartIndex = this.keypadState.currentDart - 1;
        this.keypadState.darts[dartIndex] = dartResult;

        // Update UI
        this.updateDartDisplay(dartIndex, dartResult);
        this.updateThrowTotal();

        // 🔧 ERWEITERT: Prüfe BUST nach JEDEM Dart (nicht nur am Ende)
        if (this.isBustWithCurrentDarts()) {
            console.log('💥 [DART-UI] BUST detected after dart', dartIndex + 1);

            // Sofort BUST-Animation zeigen
            this.keypadState.throwComplete = true;
            this.disableKeypad();

            setTimeout(() => {
                this.handleEarlyBust();
            }, 300); // Kurze Verzögerung damit User den Dart sieht

            return; // Stoppe weitere Verarbeitung
        }

        // Move to next dart
        this.keypadState.currentDart++;

        // Reset multiplier nach jedem Dart
        this.keypadState.selectedMultiplier = 1;
        this.setActiveMultiplier(1);

        // Check if throw is complete OR if fewer darts needed
        const isThrowComplete = this.keypadState.currentDart > 3;
        const canFinishEarly = this.canFinishWithCurrentDarts();

        // Enable confirm button after each dart
        const dartsEntered = this.keypadState.darts.filter(dart => dart !== null).length;
        if (dartsEntered > 0) {
            this.elements.confirmThrow.disabled = false;
        }

        if (isThrowComplete || canFinishEarly) {
            this.keypadState.throwComplete = true;
            this.disableKeypad();

            // Automatischer Wurf nach 3 Darts oder bei Finish
            if (canFinishEarly) {
                setTimeout(() => {
                    this.handleAutoFinish();
                }, 500);
            } else if (isThrowComplete) {
                setTimeout(() => {
                    this.handleConfirmThrow();
                }, 1000);
            }
        }

        this.updateKeypadState();

        console.log(`🎯 [DART-UI] Dart ${dartIndex + 1} added:`, dartResult);
    }

    /**
     * Handle automatic finish check for early checkout
     */
    handleAutoFinish() {
        if (!this.canFinishWithCurrentDarts()) {
            // Prüfe auch auf BUST bei früher Eingabe
            if (this.isBustWithCurrentDarts()) {
                this.handleEarlyBust();
                return;
            }
            return;
        }

        const dart1 = (this.keypadState.darts[0] && this.keypadState.darts[0].score) || 0;
        const dart2 = (this.keypadState.darts[1] && this.keypadState.darts[1].score) || 0;
        const dart3 = (this.keypadState.darts[2] && this.keypadState.darts[2].score) || 0;

        const result = this.core.processThrow(dart1, dart2, dart3);

        if (result.success && result.type === 'leg_won') {
            this.resetKeypad();
            this.updatePlayerDisplays();
            this.updateThrowHistory();
            this.updateGameStatus();

            this.disableAllInputs();
            this.showWinAnimation(result);

            setTimeout(() => {
                this.hideWinAnimation();
                this.showVictoryModal(result);
            }, 3000);

            console.log('🎯 [DART-UI] Auto-finish detected and processed');
        }
    }

    /**
     * Check if player can finish with current darts
     */
    canFinishWithCurrentDarts() {
        if (!this.core || !this.core.gameState) return false;

        const currentPlayer = this.core.gameState.currentPlayer;
        const playerData = currentPlayer === 1 ? this.core.gameState.player1 : this.core.gameState.player2;

        // Calculate current throw total
        const throwTotal = this.keypadState.darts
            .filter(dart => dart !== null)
            .reduce((sum, dart) => sum + dart.score, 0);

        const newScore = playerData.score - throwTotal;

        // Prüfe ob Finish möglich und regelkonform
        if (newScore === 0) {
            const lastDart = this.getLastThrownDart();

            if (!lastDart) return false;

            // Prüfe Double-Out-Regel
            if (this.core.gameRules && this.core.gameRules.doubleOut) {
                const isValidDouble = this.core.isValidDouble(lastDart.score);
                console.log(`🔍 [DART-UI] Double-Out check: ${lastDart.display} (${lastDart.score}) is valid double: ${isValidDouble}`);
                return isValidDouble;
            }

            // Single-Out: Jeder Dart außer Miss ist gültig
            return lastDart.score > 0;
        }

        return false;
    }

    /**
     * Check if player will bust with current darts
     */
    isBustWithCurrentDarts() {
        if (!this.core || !this.core.gameState) return false;

        const currentPlayer = this.core.gameState.currentPlayer;
        const playerData = currentPlayer === 1 ? this.core.gameState.player1 : this.core.gameState.player2;

        // Calculate current throw total
        const throwTotal = this.keypadState.darts
            .filter(dart => dart !== null)
            .reduce((sum, dart) => sum + dart.score, 0);

        const newScore = playerData.score - throwTotal;

        // Prüfe auf Bust (Überwurf oder unmöglicher Finish bei Double-Out)
        if (newScore < 0) {
            console.log(`💥 [DART-UI] Early BUST detected: ${playerData.score} - ${throwTotal} = ${newScore}`);
            return true;
        }

        // Prüfe Double-Out Regel für Score 1
        if (this.core.gameRules && this.core.gameRules.doubleOut && newScore === 1) {
            console.log(`💥 [DART-UI] Early BUST detected: Can't finish on 1 with double-out`);
            return true;
        }

        return false;
    }

    /**
     * Handle early bust detection
     */
    handleEarlyBust() {
        // 🔧 KORRIGIERT: Bestimme Spieler VOR processThrow
        const bustedPlayerNumber = this.core.gameState.currentPlayer;
        const bustedPlayerName = this.core.getPlayerName(bustedPlayerNumber);

        const dart1 = (this.keypadState.darts[0] && this.keypadState.darts[0].score) || 0;
        const dart2 = (this.keypadState.darts[1] && this.keypadState.darts[1].score) || 0;
        const dart3 = (this.keypadState.darts[2] && this.keypadState.darts[2].score) || 0;

        console.log('💥 [DART-UI] Processing early BUST for player', bustedPlayerNumber, 'with darts:', [dart1, dart2, dart3]);

        const result = this.core.processThrow(dart1, dart2, dart3);

        if (result.success && result.type === 'bust') {
            this.resetKeypad();
            this.updatePlayerDisplays();
            this.updateThrowHistory();
            this.updateGameStatus();

            // 🔧 KORRIGIERT: Zeige BUSTED Animation mit korrektem Spieler
            const animationResult = {
                ...result,
                bustedPlayer: bustedPlayerNumber,
                bustedPlayerName: bustedPlayerName
            };

            this.showBustedAnimationForPlayer(bustedPlayerNumber, bustedPlayerName);

            setTimeout(() => {
                this.hideBustedAnimation();
            }, 2500);

            console.log('💥 [DART-UI] Early BUST processed for player:', bustedPlayerName);
        }
    }

    /**
     * Show BUSTED animation for specific player
     */
    showBustedAnimationForPlayer(playerNumber, playerName) {
        const playerElement = playerNumber === 1 ? this.elements.player1Section : this.elements.player2Section;

        // 🎬 NEU: Use animations module for bust animation
        if (this.animations) {
            this.animations.animateBust(playerElement);
        }

        this.elements.bustedMessage.textContent = '💥 BUSTED! 💥';
        this.elements.bustedDetails.innerHTML = `<strong>${playerName}</strong> hat sich überworfen!<br>Nächster Spieler ist dran.`;

        this.elements.bustedAnimation.classList.remove('hidden');

        // Sound-Effekt für Bust
        this.playBustedSound();

        console.log('💥 [DART-UI] BUSTED animation displayed for player:', playerName, 'Number:', playerNumber);
    }

    /**
     * Update dart display element
     */
    updateDartDisplay(dartIndex, dartResult) {
        const displayElement = [
            this.elements.dart1Result,
            this.elements.dart2Result,
            this.elements.dart3Result
        ][dartIndex];

        if (displayElement) {
            displayElement.textContent = dartResult.display;
            displayElement.classList.add('entered');
            displayElement.classList.remove('active');

            // 🎬 NEU: Use animations module for dart entry animation
            if (this.animations) {
                this.animations.animateDartEntry(displayElement, dartResult.score);
            }
        }

        // Set next dart as active
        if (dartIndex + 1 < 3) {
            const nextElement = [
                this.elements.dart1Result,
                this.elements.dart2Result,
                this.elements.dart3Result
            ][dartIndex + 1];

            if (nextElement) {
                nextElement.classList.add('active');
            }
        }
    }

    /**
     * Update throw total display
     */
    updateThrowTotal() {
        const total = this.keypadState.darts
            .filter(dart => dart !== null)
            .reduce((sum, dart) => sum + dart.score, 0);

        this.elements.throwTotal.textContent = total;

        // 🎬 NEU: Use animations module for throw total animation
        if (this.animations) {
            this.animations.animateThrowTotal(this.elements.throwTotal, total);

            // 🎬 Special animation for maximum (180)
            if (total === 180) {
                this.animations.animateMaximum(this.elements.throwTotal);
            }
        }

        // Color coding for total
        if (total > 180) {
            this.elements.throwTotal.style.color = '#e53e3e'; // Red for invalid
        } else if (total >= 140) {
            this.elements.throwTotal.style.color = '#38a169'; // Green for excellent
        } else if (total >= 100) {
            this.elements.throwTotal.style.color = '#4299e1'; // Blue for good
        } else {
            this.elements.throwTotal.style.color = '#2d3748'; // Default
        }
    }

    /**
     * Set active multiplier visual feedback
     */
    setActiveMultiplier(multiplier) {
        this.keypadElements.multiplierBtns.forEach(btn => {
            btn.classList.remove('active');
            if (parseInt(btn.dataset.multiplier) === multiplier) {
                btn.classList.add('active');
            }
        });
    }

    /**
     * Update keypad state and UI
     */
    updateKeypadState() {
        // Update current player display in both player headers
        if (this.core && this.core.gameState) {
            const player1Data = this.core.gameState.player1;
            const player2Data = this.core.gameState.player2;

            // Update both player displays in header
            this.elements.player1DisplayName.textContent = this.core.getPlayerName(1);
            this.elements.player1DisplayScore.textContent = player1Data.score;
            this.elements.player2DisplayName.textContent = this.core.getPlayerName(2);
            this.elements.player2DisplayScore.textContent = player2Data.score;

            // Set active/inactive states for player displays
            if (this.core.gameState.currentPlayer === 1) {
                this.elements.player1Display.classList.add('active');
                this.elements.player1Display.classList.remove('inactive');
                this.elements.player2Display.classList.add('inactive');
                this.elements.player2Display.classList.remove('active');
            } else {
                this.elements.player2Display.classList.add('active');
                this.elements.player2Display.classList.remove('inactive');
                this.elements.player1Display.classList.add('inactive');
                this.elements.player1Display.classList.remove('active');
            }
        }

        // Update dart result active states
        const dartElements = [
            this.elements.dart1Result,
            this.elements.dart2Result,
            this.elements.dart3Result
        ];

        dartElements.forEach((el, index) => {
            el.classList.remove('active');
            if (index === this.keypadState.currentDart - 1 && !this.keypadState.throwComplete) {
                el.classList.add('active');
            }
        });

        // Enable/disable control buttons
        this.elements.undoLastDart.disabled = this.keypadState.currentDart === 1;
    }

    /**
     * Update game status display - kompakte Anzeige
     */
    updateGameStatus() {
        if (!this.core || !this.core.gameState) return;

        const gameState = this.core.gameState;
        const gameRules = this.core.gameRules;

        // Update Leg display
        this.elements.currentLegDisplay.textContent = gameState.currentLeg;

        // 🔧 KORRIGIERT: Bessere Set-Display Logik für Round Robin Finals
        // Update Set display - nur anzeigen wenn tatsächlich Sets gespielt werden
        const setsToWin = gameRules.setsToWin || 0;
        const legsToWin = gameRules.legsToWinSet || gameRules.legsToWin || 2;

        // � KORRIGIERT: Prüfe playWithSets Flag für Round Robin Finals
        // Das playWithSets Flag ist entscheidend, nicht nur setsToWin!
        const isReallySetsBased = (gameRules.playWithSets === true) && (setsToWin > 1);

        console.log('🎮 [DART-UI] Set display logic:', {
            setsToWin,
            legsToWin,
            playWithSets: gameRules.playWithSets,
            isReallySetsBased,
            matchType: this.core.matchData ? (this.core.matchData.matchType || this.core.matchData.type) : 'unknown'
        });

        if (isReallySetsBased) {
            this.elements.currentSetDisplay.textContent = gameState.currentSet;
            this.elements.currentSetContainer.style.display = 'block';
        } else {
            this.elements.currentSetContainer.style.display = 'none';
        }

        // Update game mode display
        const startingScore = this.core.getStartingScore();
        let modeText = startingScore.toString();

        if (gameRules.doubleOut) {
            modeText += ' D.Out';
        }

        this.elements.gameModeDisplay.textContent = modeText;
    }

    /**
     * Disable keypad when throw is complete
     */
    disableKeypad() {
        this.keypadElements.numberBtns.forEach(btn => {
            btn.classList.add('disabled');
        });
        this.keypadElements.specialBtns.forEach(btn => {
            btn.classList.add('disabled');
        });
    }

    /**
     * Enable keypad for new throw
     */
    enableKeypad() {
        this.keypadElements.numberBtns.forEach(btn => {
            btn.classList.remove('disabled');
        });
        this.keypadElements.specialBtns.forEach(btn => {
            btn.classList.remove('disabled');
        });
    }

    /**
     * Reset keypad for new throw
     */
    resetKeypad() {
        this.keypadState = {
            currentDart: 1,
            darts: [null, null, null],
            selectedMultiplier: 1,
            awaitingNumber: false,
            throwComplete: false
        };

        // Reset UI
        [this.elements.dart1Result, this.elements.dart2Result, this.elements.dart3Result]
        .forEach(el => {
            el.textContent = '-';
            el.classList.remove('entered', 'active');
        });

        this.elements.dart1Result.classList.add('active');
        this.elements.throwTotal.textContent = '0';
        this.elements.throwTotal.style.color = '#2d3748';
        this.elements.confirmThrow.disabled = true;

        this.setActiveMultiplier(1);
        this.enableKeypad();
        this.updateKeypadState();

        console.log('🔄 [DART-UI] Keypad reset for new throw');
    }

    /**
     * Disable all inputs when leg/match is finished
     */
    disableAllInputs() {
        this.disableKeypad();

        this.elements.confirmThrow.disabled = true;
        this.elements.undoLastDart.disabled = true;
        this.elements.undoThrow.disabled = true;

        this.keypadElements.multiplierBtns.forEach(btn => {
            btn.classList.add('disabled');
        });

        // 🆕 NEU: Disable keyboard module
        this.disableKeyboard();

        console.log('🚫 [DART-UI] All inputs disabled');
    }

    /**
     * Enable all inputs for new leg
     */
    enableAllInputs() {
        this.enableKeypad();

        this.keypadElements.multiplierBtns.forEach(btn => {
            btn.classList.remove('disabled');
        });

        // 🆕 NEU: Enable keyboard module
        this.enableKeyboard();

        console.log('✅ [DART-UI] All inputs enabled for new leg');
    }

    /**
     * Format dart display text
     */
    formatDartDisplay(number, multiplier) {
        if (number === 0) return 'Miss';
        if (number === 25 && multiplier === 1) return 'Bull';
        if (number === 25 && multiplier === 2) return 'D-Bull';

        const multiplierPrefix = multiplier === 2 ? 'D' : multiplier === 3 ? 'T' : '';
        return `${multiplierPrefix}${number}`;
    }

    /**
     * Get multiplier name for display
     */
    getMultiplierName(multiplier) {
        switch (multiplier) {
            case 1:
                return 'Single';
            case 2:
                return 'Double';
            case 3:
                return 'Triple';
            default:
                return 'Unknown';
        }
    }

    /**
     * Setup navigation event listeners
     */
    setupNavigation() {
        const urlParams = new URLSearchParams(window.location.search);
        const tournamentId = urlParams.get('tournament') || urlParams.get('t');
        const matchId = urlParams.get('match') || urlParams.get('m');
        const isUuidSystem = urlParams.get('uuid') === 'true';

        // Back to match page
        this.elements.backToMatch.addEventListener('click', (e) => {
            e.preventDefault();
            
            let matchUrl;
            
            // Prefer simplified URL for UUID matches when no tournament or tournament=null
            if (isUuidSystem && (!tournamentId || tournamentId === 'null' || tournamentId === null)) {
                // Use new simplified match URL format
                matchUrl = `/match/${matchId}`;
                console.log('🚀 [DART-SCORING] Using simplified match URL (UUID-only):', matchUrl);
            } else if (tournamentId && matchId) {
                // Use legacy format with tournament
                matchUrl = `/match-page.html?tournament=${tournamentId}&match=${matchId}&uuid=true`;
                console.log('🔄 [DART-SCORING] Using legacy match URL:', matchUrl);
            } else if (matchId) {
                // Fallback: match ID only
                matchUrl = `/match/${matchId}`;
                console.log('🎯 [DART-SCORING] Using fallback simplified match URL:', matchUrl);
            } else {
                console.error('❌ [DART-SCORING] No match ID available for navigation');
                alert('Fehler: Match-ID nicht verfügbar');
                return;
            }
            
            window.location.href = matchUrl;
        });

        // Back to tournament
        this.elements.backToTournament.addEventListener('click', (e) => {
            e.preventDefault();
            if (tournamentId && tournamentId !== 'null' && tournamentId !== null) {
                window.location.href = `/tournament-interface.html?tournament=${tournamentId}`;
            } else {
                console.error('❌ [DART-SCORING] No tournament ID available for navigation');
                alert('Fehler: Tournament-ID nicht verfügbar');
            }
        });
    }

    /**
     * Update match display with loaded data
     */
    updateMatchDisplay() {
            if (!this.core || !this.core.matchData) return;

            const match = this.core.matchData;
            const gameState = this.core.gameState;
            const gameRules = this.core.gameRules;

            // 🔧 ERWEITERTE DEBUGGING: Zeige alle verfügbaren Game Rules Information
            console.log('🎮 [DART-UI] Game Rules Debug Info:', {
                'Core gameRules': this.core.gameRules,
                'Match gameRules': match.gameRules || match.GameRules || match.gameRulesUsed,
                'Match Type': match.matchType || match.type,
                'Match Class': match.classId || match.class,
                'Match Name': match.displayName || match.name,
                'Raw Match Data': match
            });

            // 🚨 KORRIGIERT: Bessere Game Rules Erkennung für Round Robin Finals
            let effectiveGameRules = gameRules;

            // Prüfe ob Match spezifische Rules vorhanden sind, die anders sind
            if (match.gameRules || match.GameRules || match.gameRulesUsed) {
                const matchSpecificRules = match.gameRules || match.GameRules || match.gameRulesUsed;

                // Vergleiche die Rules
                const coreRulesStr = JSON.stringify(gameRules);
                const matchRulesStr = JSON.stringify(matchSpecificRules);

                if (coreRulesStr !== matchRulesStr) {
                    console.warn('🚨 [DART-UI] Game Rules Mismatch detected!');
                    console.log('🎮 [DART-UI] Core loaded rules:', gameRules);
                    console.log('🎮 [DART-UI] Match specific rules:', matchSpecificRules);

                    // Verwende Match-spezifische Rules wenn verfügbar
                    effectiveGameRules = matchSpecificRules;
                    console.log('✅ [DART-UI] Using match-specific game rules instead of core rules');
                }
            }

            // 🔧 ERWEITERT: Match-Regeln in der Meta-Anzeige mit korrigierten Rules
            const legsToWin = effectiveGameRules.legsToWinSet || effectiveGameRules.legsToWin || 2;
            const setsToWin = effectiveGameRules.setsToWin || 1;
            const startingScore = this.getStartingScoreFromRules(effectiveGameRules);
            const doubleOut = effectiveGameRules.doubleOut ? ' D.Out' : '';

            // � KORRIGIERT: Prüfe playWithSets Flag für Round Robin Finals
            const isReallySetsBased = (effectiveGameRules.playWithSets === true) && (setsToWin > 1);

            let rulesText = '';
            if (isReallySetsBased) {
                // Best of Sets Format: "Best of 5 Sets • First to 3 Legs per Set"
                const totalSets = (setsToWin * 2) - 1; // Best of X berechnen
                rulesText = `Best of ${totalSets} Sets • First to ${legsToWin} Legs`;
            } else {
                // 🔧 KORRIGIERT: Nur Legs Format für Round Robin Finals: "First to 3 Legs"
                rulesText = `First to ${legsToWin} Legs`;
            }

            // Update header
            this.elements.gameTitle.textContent = `🎯 ${match.displayName}`;
            this.elements.gameMeta.textContent = `${startingScore}${doubleOut} • ${rulesText} • Leg ${gameState.currentLeg}${isReallySetsBased ? ` • Set ${gameState.currentSet}` : ''}`;

        // Update player names
        this.elements.player1Name.textContent = match.player1.name || 'Spieler 1';
        this.elements.player2Name.textContent = match.player2.name || 'Spieler 2';

        // Initialize keypad
        this.initializeKeypad();

        // Update game status
        this.updateGameStatus();

        // Show game container
        this.showGame();

        console.log('📊 [DART-UI] Match display updated with effective rules:', {
            rulesText,
            effectiveGameRules,
            startingScore,
            legsToWin,
            setsToWin,
            playWithSets: effectiveGameRules.playWithSets,
            isReallySetsBased,
            doubleOut: effectiveGameRules.doubleOut
        });
    }

    /**
     * 🆕 NEU: Get starting score from specific game rules
     */
    getStartingScoreFromRules(gameRules) {
        if (!gameRules) return 501;

        switch (gameRules.gameMode) {
            case 'Game301':
                return 301;
            case 'Game401':
                return 401;
            case 'Game501':
                return 501;
            case 'Game701':
                return 701;
            case 'Game1001':
                return 1001;
            default:
                return 501;
        }
    }

    /**
     * Update all player displays
     */
    updatePlayerDisplays() {
        if (!this.core) return;

        const gameState = this.core.gameState;

        // Store previous scores for animations
        const previousPlayer1Score = this.elements.player1Score.textContent ? parseInt(this.elements.player1Score.textContent) : gameState.player1.score;
        const previousPlayer2Score = this.elements.player2Score.textContent ? parseInt(this.elements.player2Score.textContent) : gameState.player2.score;

        // Update scores and stats in side panels
        this.elements.player1Score.textContent = gameState.player1.score;
        this.elements.player1Legs.textContent = gameState.player1.legs;
        this.elements.player1Sets.textContent = gameState.player1.sets;
        this.elements.player1Average.textContent = this.core.getPlayerAverage(1).toFixed(1);

        this.elements.player2Score.textContent = gameState.player2.score;
        this.elements.player2Legs.textContent = gameState.player2.legs;
        this.elements.player2Sets.textContent = gameState.player2.sets;
        this.elements.player2Average.textContent = this.core.getPlayerAverage(2).toFixed(1);

        // 🎬 NEU: Animate score changes
        if (this.animations) {
            if (gameState.player1.score !== previousPlayer1Score) {
                this.animations.animateScoreUpdate(this.elements.player1Score, gameState.player1.score, previousPlayer1Score);
                this.animations.animateScoreUpdate(this.elements.player1DisplayScore, gameState.player1.score, previousPlayer1Score);
            }
            if (gameState.player2.score !== previousPlayer2Score) {
                this.animations.animateScoreUpdate(this.elements.player2Score, gameState.player2.score, previousPlayer2Score);
                this.animations.animateScoreUpdate(this.elements.player2DisplayScore, gameState.player2.score, previousPlayer2Score);
            }
        }

        // Update active player highlighting in side panels
        const previousActivePlayer = document.querySelector('.player-section.active');
        const newActivePlayer = gameState.currentPlayer === 1 ? this.elements.player1Section : this.elements.player2Section;

        this.elements.player1Section.classList.toggle('active', gameState.currentPlayer === 1);
        this.elements.player2Section.classList.toggle('active', gameState.currentPlayer === 2);

        // 🎬 NEU: Animate player switch
        if (this.animations && previousActivePlayer !== newActivePlayer) {
            this.animations.animatePlayerSwitch(newActivePlayer, previousActivePlayer);
        }

        // Update finish suggestions
        this.updateFinishSuggestions();

        // Update both player displays in header
        this.elements.player1DisplayName.textContent = this.core.getPlayerName(1);
        this.elements.player1DisplayScore.textContent = gameState.player1.score;
        this.elements.player2DisplayName.textContent = this.core.getPlayerName(2);
        this.elements.player2DisplayScore.textContent = gameState.player2.score;

        // Set active/inactive states for player displays
        if (gameState.currentPlayer === 1) {
            this.elements.player1Display.classList.add('active');
            this.elements.player1Display.classList.remove('inactive');
            this.elements.player2Display.classList.add('inactive');
            this.elements.player2Display.classList.remove('active');
        } else {
            this.elements.player2Display.classList.add('active');
            this.elements.player2Display.classList.remove('inactive');
            this.elements.player1Display.classList.add('inactive');
            this.elements.player1Display.classList.remove('active');
        }

        console.log('👥 [DART-UI] Player displays updated');
    }

    /**
     * Update finish suggestions for both players
     */
    updateFinishSuggestions() {
        const gameState = this.core.gameState;

        // Player 1 finishes
        const p1Finishes = this.core.getPossibleFinishes(gameState.player1.score);
        if (p1Finishes.length > 0 && gameState.player1.score <= 170) {
            this.elements.player1FinishOptions.innerHTML = p1Finishes
                .map(finish => `<div class="finish-option">${finish}</div>`)
                .join('');
            this.elements.player1Finishes.classList.remove('hidden');
        } else {
            this.elements.player1Finishes.classList.add('hidden');
        }

        // Player 2 finishes
        const p2Finishes = this.core.getPossibleFinishes(gameState.player2.score);
        if (p2Finishes.length > 0 && gameState.player2.score <= 170) {
            this.elements.player2FinishOptions.innerHTML = p2Finishes
                .map(finish => `<div class="finish-option">${finish}</div>`)
                .join('');
            this.elements.player2Finishes.classList.remove('hidden');
        } else {
            this.elements.player2Finishes.classList.add('hidden');
        }
    }

    /**
     * Handle throw confirmation
     */
    handleConfirmThrow() {
        // Erlaube Bestätigung auch mit weniger als 3 Darts
        const dartsThrown = this.keypadState.darts.filter(dart => dart !== null).length;

        if (dartsThrown === 0) {
            this.showMessage('Mindestens einen Dart eingeben', 'warning');
            return;
        }

        // Extract scores from darts
        const dart1 = (this.keypadState.darts[0] && this.keypadState.darts[0].score) || 0;
        const dart2 = (this.keypadState.darts[1] && this.keypadState.darts[1].score) || 0;
        const dart3 = (this.keypadState.darts[2] && this.keypadState.darts[2].score) || 0;

        console.log(`🎯 [DART-UI] Confirming throw with ${dartsThrown} darts:`, [dart1, dart2, dart3]);

        const result = this.core.processThrow(dart1, dart2, dart3);

        if (result.success) {
            this.resetKeypad();
            this.updatePlayerDisplays();
            this.updateThrowHistory();
            this.updateGameStatus();

            if (result.type === 'leg_won') {
                this.disableAllInputs();
                this.showWinAnimation(result);

                setTimeout(() => {
                    this.hideWinAnimation();
                    this.showVictoryModal(result);
                }, 3000);

            } else if (result.type === 'bust') {
                // Zeige BUSTED Animation
                this.showBustedAnimation(result);

                // Nach 2.5 Sekunden Animation ausblenden und weiter
                setTimeout(() => {
                    this.hideBustedAnimation();
                }, 2500);
            } else {
                this.showMessage(result.message, 'success');
            }
        } else {
            this.showMessage(result.message, 'error');
        }
    }

    /**
     * Show win animation for checkout
     */
    showWinAnimation(result) {
        const playerName = this.core.getPlayerName(this.core.gameState.currentPlayer);
        const lastDart = this.getLastThrownDart();
        const isDoubleOut = this.core.gameRules.doubleOut;
        const playerElement = this.core.gameState.currentPlayer === 1 ? this.elements.player1Section : this.elements.player2Section;

        // 🎬 NEU: Use animations module for checkout animation
        if (this.animations && lastDart) {
            this.animations.animateCheckout(playerElement, lastDart.score);
        }

        this.elements.winMessage.textContent = '🏆 CHECKOUT! 🏆';

        let detailsHtml = `<strong>${playerName}</strong> gewinnt das Leg!<br>`;
        if (lastDart && lastDart.display !== 'Miss') {
            detailsHtml += `Mit ${lastDart.display}`;
            if (isDoubleOut && lastDart.multiplier === 2) {
                detailsHtml += ` (Double-Out!)`;
            }
        }

        this.elements.winDetails.innerHTML = detailsHtml;
        this.elements.winAnimation.classList.remove('hidden');

        this.playWinSound();

        console.log('🏆 [DART-UI] Win animation displayed');
    }

    /**
     * Hide win animation
     */
    hideWinAnimation() {
        this.elements.winAnimation.classList.add('hidden');
        console.log('🙈 [DART-UI] Win animation hidden');
    }

    /**
     * Handle continue from win animation
     */
    handleContinueFromWin() {
        this.hideWinAnimation();
        const lastResult = { type: 'leg_won' };
        this.showVictoryModal(lastResult);
    }

    /**
     * Show BUSTED animation for overshot
     */
    showBustedAnimation(result) {
        // 🔧 KORRIGIERT: Bestimme Spieler VOR Spielerwechsel
        // Der Spieler der sich überworfen hat ist noch der aktuelle Spieler
        // (processThrow wechselt erst NACH der BUST-Verarbeitung)
        const bustedPlayerNumber = this.core.gameState.currentPlayer;
        const bustedPlayerName = this.core.getPlayerName(bustedPlayerNumber);
        const playerElement = bustedPlayerNumber === 1 ? this.elements.player1Section : this.elements.player2Section;

        // 🎬 NEU: Use animations module for bust animation
        if (this.animations) {
            this.animations.animateBust(playerElement);
        }

        this.elements.bustedMessage.textContent = '💥 BUSTED! 💥';
        this.elements.bustedDetails.innerHTML = `<strong>${bustedPlayerName}</strong> hat sich überworfen!<br>Nächster Spieler ist dran.`;

        this.elements.bustedAnimation.classList.remove('hidden');

        // Sound-Effekt für Bust
        this.playBustedSound();

        console.log('💥 [DART-UI] BUSTED animation displayed for player:', bustedPlayerName, 'Player Number:', bustedPlayerNumber);
    }

    /**
     * Hide BUSTED animation
     */
    hideBustedAnimation() {
        this.elements.bustedAnimation.classList.add('hidden');
        console.log('🙈 [DART-UI] BUSTED animation hidden');
    }

    /**
     * Play BUSTED sound effect
     */
    playBustedSound() {
        try {
            const audioContext = new(window.AudioContext || window.webkit.AudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            // Tieferer, härterer Ton für BUSTED
            oscillator.frequency.setValueAtTime(150, audioContext.currentTime);
            oscillator.frequency.exponentialRampToValueAtTime(80, audioContext.currentTime + 0.8);

            gainNode.gain.setValueAtTime(0.4, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.8);

            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 0.8);
        } catch (error) {
            console.log('🔇 [DART-UI] Audio not supported');
        }
    }

    /**
     * Get the last thrown dart from current throw
     */
    getLastThrownDart() {
        const darts = this.keypadState.darts;
        for (let i = darts.length - 1; i >= 0; i--) {
            if (darts[i] !== null) {
                return darts[i];
            }
        }
        return null;
    }

    /**
     * Play win sound effect
     */
    playWinSound() {
        try {
            const audioContext = new(window.AudioContext || window.webkit.AudioContext)();
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            oscillator.frequency.setValueAtTime(440, audioContext.currentTime);
            oscillator.frequency.exponentialRampToValueAtTime(880, audioContext.currentTime + 0.5);

            gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 1);

            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + 1);
        } catch (error) {
            console.log('🔇 [DART-UI] Audio not supported');
        }
    }

    /**
     * Handle undo last dart
     */
    handleUndoLastDart() {
        if (this.keypadState.currentDart === 1) {
            return;
        }

        // Remove last dart
        const lastDartIndex = this.keypadState.throwComplete ? 2 : this.keypadState.currentDart - 2;
        this.keypadState.darts[lastDartIndex] = null;

        // Update UI
        const dartElement = [
            this.elements.dart1Result,
            this.elements.dart2Result,
            this.elements.dart3Result
        ][lastDartIndex];

        if (dartElement) {
            dartElement.textContent = '-';
            dartElement.classList.remove('entered', 'active');
        }

        // Update state
        this.keypadState.currentDart = lastDartIndex + 1;
        this.keypadState.throwComplete = false;

        // Re-enable keypad and update UI
        this.enableKeypad();
        this.elements.confirmThrow.disabled = true;
        this.updateKeypadState();
        this.updateThrowTotal();

        console.log(`⏪ [DART-UI] Undid dart ${lastDartIndex + 1}`);
    }

    /**
     * Handle undo entire throw
     */
    handleUndoThrow() {
        const result = this.core.undoLastThrow();

        if (result.success) {
            this.updatePlayerDisplays();
            this.updateThrowHistory();
            this.updateGameStatus();
            this.showMessage(result.message, 'success');
        } else {
            this.showMessage(result.message, 'error');
        }
    }

    /**
     * Handle new leg start - 🔧 GEÄNDERT: Behandle Match-Ende anders
     */
    handleNewLeg() {
        // 🆕 NEU: Prüfe ob Match beendet ist und zeige Statistiken statt neues Leg zu starten
        if (this.core.gameState.isGameFinished) {
            console.log('🏁 [DART-UI] Match is finished - showing statistics instead of new leg');
            this.handleFinishMatch(); // Zeigt Statistik-Modal
            return;
        }

        // Normaler neues Leg Ablauf
        const result = this.core.startNewLeg();

        if (result.success) {
            this.hideVictoryModal();
            this.enableAllInputs();
            this.resetKeypad();

            this.updatePlayerDisplays();
            this.updateThrowHistory();
            this.updateGameStatus();
            this.showMessage(result.message, 'success');

            console.log('🆕 [DART-UI] New leg started - inputs re-enabled');
        }
    }

    /**
     * Handle match finish - 🔧 GEÄNDERT: Zeige Statistik-Modal statt direkte Übertragung
     */
    async handleFinishMatch() {
        try {
            console.log('🏁 [DART-UI] Match finish requested - showing match statistics...');

            // Verstecke Victory Modal
            this.hideVictoryModal();

            // Zeige Match-Statistik-Modal mit Absenden-Button
            this.showMatchStatisticsModal();

        } catch (error) {
            console.error('❌ [DART-UI] Error showing match statistics:', error);
            this.showMessage(`❌ Fehler beim Anzeigen der Statistiken: ${error.message}`, 'error');
        }
    }

    /**
     * 📤 NEU: Handle submit match result from statistics modal
     */
    async handleSubmitMatchResult() {
        try {
            console.log('📤 [DART-UI] Submitting match result from statistics modal...');

            // Disable the button to prevent double-clicks
            this.elements.submitMatchResultBtn.disabled = true;
            this.elements.submitMatchResultBtn.textContent = 'Übertrage...';

            // Show submission progress
            this.showMessage('📤 Übertrage Match-Ergebnis mit Statistiken...', 'info');

            // Use enhanced submission system
            const result = await this.core.submitMatchResult();

            if (result.success) {
                // Hide statistics modal
                this.hideMatchStatisticsModal();

                // Show success and final completion
                this.showMatchFinishedDisplay();

                console.log('✅ [DART-UI] Match result submitted successfully from statistics modal');

            } else {
                // Re-enable button on failure
                this.elements.submitMatchResultBtn.disabled = false;
                this.elements.submitMatchResultBtn.textContent = '📤 Ergebnis absenden';

                this.showMessage(`❌ Fehler: ${result.message}`, 'error');
            }

        } catch (error) {
            console.error('❌ [DART-UI] Error submitting from statistics modal:', error);

            // Re-enable button on error
            this.elements.submitMatchResultBtn.disabled = false;
            this.elements.submitMatchResultBtn.textContent = '📤 Ergebnis absenden';

            this.showMessage(`❌ Fehler beim Übertragen: ${error.message}`, 'error');
        }
    }

    /**
     * Update throw history display
     */
    updateThrowHistory() {
            const gameState = this.core.gameState;
            const history = gameState.throwHistory.slice(0, 20);

            this.elements.historyList.innerHTML = history.map(entry => {
                        const playerName = this.core.getPlayerName(entry.player);

                        let entryClass = '';
                        let statusIcon = '';

                        if (entry.isWinning) {
                            entryClass = 'style="background: #c6f6d5;"';
                            statusIcon = '🏆';
                        } else if (entry.isBust) {
                            entryClass = 'style="background: #fed7d7;"';
                            statusIcon = '💥';
                        }

                        return `
                <div class="history-entry" ${entryClass}>
                    <div class="history-player">${statusIcon} ${playerName}</div>
                    <div class="history-throws">
                        ${entry.darts.map(dart => `<span class="history-throw">${dart || 0}</span>`).join('')}
                    </div>
                    <div class="history-total">${entry.total}</div>
                </div>
            `;
        }).join('');
    }

    /**
     * Show victory modal
     */
    showVictoryModal(result) {
        const playerName = this.core.getPlayerName(this.core.gameState.currentPlayer);
        
        // 🔧 KORRIGIERT: Prüfe playWithSets Flag für korrekte Message-Anzeige
        const isReallySetsBased = (this.core.gameRules.playWithSets === true) && (this.core.gameRules.setsToWin > 1);
        
        let message = `<strong>${playerName}</strong> gewinnt das Leg!`;

        if (result.gameResult) {
            if (result.gameResult.type === 'set_won') {
                // 🔧 NEU: Zeige nur Set-Information bei echten Sets-basierten Matches
                if (isReallySetsBased) {
                    message += `<br><br>🏆 <strong>Set gewonnen!</strong>`;
                    const newStartPlayerName = this.core.getPlayerName(result.gameResult.newSetStartPlayer);
                    message += `<br><small>Nächstes Set startet: <strong>${newStartPlayerName}</strong></small>`;
                } else {
                    // Für Legs-only Matches (z.B. Round Robin Finals): Keine Set-Information
                    console.log('🎯 [DART-UI] Legs-only match - skipping set information in victory modal');
                }
                
            } else if (result.gameResult.type === 'match_won') {
                message += `<br><br>🎯 <strong>Match gewonnen!</strong>`;
                
                // 🔧 KORRIGIERT: Prüfe isGameFinished Flag für korrekte Button-Anzeige
                if (this.core.gameState.isGameFinished) {
                    // 🆕 NEU: Bei Match-Ende zeige "Statistiken anzeigen" statt direkter Submit
                    this.elements.newLegBtn.textContent = 'Statistiken anzeigen';
                    this.elements.finishMatchBtn.style.display = 'none';
                    
                    console.log('📊 [DART-UI] Match is finished - showing "Statistiken anzeigen" button');
                } else {
                    // Fallback: Zeige beide Buttons wenn Flag nicht gesetzt
                    this.elements.newLegBtn.textContent = 'Neues Leg starten';
                    this.elements.finishMatchBtn.style.display = 'block';
                    
                    console.warn('⚠️ [DART-UI] Match won but isGameFinished=false - showing both buttons');
                }
                
                // Zeige Match-Statistiken bei Match-Ende
                message += this.generateMatchStatsSummary();
                
                // Add submission status info
                if (window.dartScoringApp && window.dartScoringApp.getSubmissionStatus) {
                    const submissionStatus = window.dartScoringApp.getSubmissionStatus();
                    
                    if (submissionStatus.isConnected) {
                        message += '<br><br><div style="color: #28a745; font-size: 0.9em;">🔗 WebSocket-Verbindung bereit für erweiterte Übertragung</div>';
                    } else {
                        message += '<br><br><div style="color: #ffc107; font-size: 0.9em;">⚠️ Standard-Übertragung wird verwendet</div>';
                    }
                }
            }
        } else {
            const nextStartPlayerName = this.core.getPlayerName(this.core.gameState.legStartPlayer === 1 ? 2 : 1);
            message += `<br><small>Nächstes Leg startet: <strong>${nextStartPlayerName}</strong></small>`;
        }

        this.elements.victoryMessage.innerHTML = message;
        this.elements.victoryModal.classList.remove('hidden');
    }

    /**
     * Generate match statistics summary for display
     */
    generateMatchStatsSummary() {
        if (!window.dartScoringApp || !window.dartScoringApp.stats) return '';
        
        const stats = window.dartScoringApp.getCurrentStatistics();
        
        let summary = '<br><br><div style="text-align: left; background: #f7fafc; padding: 15px; border-radius: 8px; margin-top: 15px;">';
        summary += '<strong>📊 Match-Statistiken:</strong><br><br>';
        
        // Player 1 Stats
        summary += `<strong>${stats.player1.name}:</strong><br>`;
        summary += `📊 Average: ${stats.player1.average}<br>`;
        if (stats.player1.maximums > 0) summary += `🎯 180er: ${stats.player1.maximums}<br>`;
        if (stats.player1.highFinishes > 0) summary += `🔥 High Finishes (≥100): ${stats.player1.highFinishes}<br>`;
        if (stats.player1.score26 > 0) summary += `⭐ 26er Scores: ${stats.player1.score26}<br>`;
        summary += `✅ Checkouts: ${stats.player1.checkouts}<br><br>`;
        
        // Player 2 Stats
        summary += `<strong>${stats.player2.name}:</strong><br>`;
        summary += `📊 Average: ${stats.player2.average}<br>`;
        if (stats.player2.maximums > 0) summary += `🎯 180er: ${stats.player2.maximums}<br>`;
        if (stats.player2.highFinishes > 0) summary += `🔥 High Finishes (≥100): ${stats.player2.highFinishes}<br>`;
        if (stats.player2.score26 > 0) summary += `⭐ 26er Scores: ${stats.player2.score26}<br>`;
        summary += `✅ Checkouts: ${stats.player2.checkouts}<br>`;
        
        summary += '</div>';
        
        return summary;
    }

    /**
     * Hide victory modal
     */
    hideVictoryModal() {
        this.elements.victoryModal.classList.add('hidden');
        this.elements.newLegBtn.textContent = 'Neues Leg starten';
        this.elements.finishMatchBtn.style.display = 'block';
    }

    /**
     * 📊 NEU: Show match statistics modal
     */
    showMatchStatisticsModal() {
        try {
            console.log('📊 [DART-UI] Showing match statistics modal...');
            
            // Generate statistics content
            const statsContent = this.generateMatchStatisticsContent();
            this.elements.matchStatsContent.innerHTML = statsContent;
            
            // Reset submit button
            this.elements.submitMatchResultBtn.disabled = false;
            this.elements.submitMatchResultBtn.textContent = '📤 Ergebnis absenden';
            
            // Show modal
            this.elements.matchStatsModal.classList.remove('hidden');
            
            console.log('✅ [DART-UI] Match statistics modal displayed');
            
        } catch (error) {
            console.error('❌ [DART-UI] Error showing match statistics modal:', error);
            this.showMessage(`❌ Fehler beim Anzeigen der Statistiken: ${error.message}`, 'error');
        }
    }

    /**
     * ❌ NEU: Hide match statistics modal
     */
    hideMatchStatisticsModal() {
        this.elements.matchStatsModal.classList.add('hidden');
    }

    /**
     * 🏁 NEU: Show match finished display
     */
    showMatchFinishedDisplay() {
        this.elements.matchFinishedDisplay.classList.remove('hidden');
    }

    /**
     * 📊 NEU: Generate match statistics content
     */
    generateMatchStatisticsContent() {
        if (!window.dartScoringApp || !window.dartScoringApp.stats) {
            return '<p>Statistiken nicht verfügbar</p>';
        }
        
        try {
            const stats = window.dartScoringApp.getCurrentStatistics();
            const fullStats = window.dartScoringApp.stats.statistics;
            
            // Determine winner
            const gameState = this.core.gameState;
            let winner = null;
            let winnerName = '';
            
            if (gameState.player1.sets > gameState.player2.sets) {
                winner = 1;
                winnerName = stats.player1.name;
            } else if (gameState.player2.sets > gameState.player1.sets) {
                winner = 2;
                winnerName = stats.player2.name;
            } else if (gameState.player1.legs > gameState.player2.legs) {
                winner = 1;
                winnerName = stats.player1.name;
            } else if (gameState.player2.legs > gameState.player1.legs) {
                winner = 2;
                winnerName = stats.player2.name;
            }

            let content = `
                <div class="match-summary">
                    <h3>🏆 ${winnerName} gewinnt das Match!</h3>
                    <p><strong>Ergebnis:</strong> ${gameState.player1.legs}-${gameState.player2.legs} Legs`;
            
            if (gameState.player1.sets > 0 || gameState.player2.sets > 0) {
                content += `, ${gameState.player1.sets}-${gameState.player2.sets} Sets`;
            }
            
            content += '</p></div>';

            content += '<div class="match-stats-grid">';
            
            // Player 1 Stats
            content += `
                <div class="player-stats-section ${winner === 1 ? 'winner-highlight' : ''}">
                    <h3>${winner === 1 ? '🏆 ' : ''}${stats.player1.name}</h3>
                    <div class="stats-row">
                        <span class="stats-label">Match Average:</span>
                        <span class="stats-value">${stats.player1.average}</span>
                    </div>`;

            // Show leg averages if available
            if (fullStats.player1.legAverages && fullStats.player1.legAverages.length > 0) {
                const avgLegAvg = fullStats.player1.legAverages.reduce((sum, leg) => sum + leg.average, 0) / fullStats.player1.legAverages.length;
                content += `
                    <div class="stats-row">
                        <span class="stats-label">? Leg Average:</span>
                        <span class="stats-value">${avgLegAvg.toFixed(1)}</span>
                    </div>
                    <div class="stats-row">
                        <span class="stats-label">Legs gespielt:</span>
                        <span class="stats-value">${fullStats.player1.legAverages.length}</span>
                    </div>`;
            }

            content += `
                    <div class="stats-row">
                        <span class="stats-label">Legs gewonnen:</span>
                        <span class="stats-value">${gameState.player1.legs}</span>
                    </div>`;

            if (gameState.player1.sets > 0 || gameState.player2.sets > 0) {
                content += `
                    <div class="stats-row">
                        <span class="stats-label">Sets gewonnen:</span>
                        <span class="stats-value">${gameState.player1.sets}</span>
                    </div>`;
            }

            if (stats.player1.maximums > 0) {
                content += `
                    <div class="stats-row">
                        <span class="stats-label">180er:</span>
                        <span class="stats-value">${stats.player1.maximums}</span>
                    </div>`;
            }

            if (stats.player1.highFinishes > 0) {
                content += `
                    <div class="stats-row">
                        <span class="stats-label">High Finishes (=100):</span>
                        <span class="stats-value">${stats.player1.highFinishes}</span>
                    </div>`;
            }

            if (stats.player1.score26 > 0) {
                content += `
                    <div class="stats-row">
                        <span class="stats-label">26er Scores:</span>
                        <span class="stats-value">${stats.player1.score26}</span>
                    </div>`;
            }

            content += `
                    <div class="stats-row">
                        <span class="stats-label">Checkouts:</span>
                        <span class="stats-value">${stats.player1.checkouts}</span>
                    </div>
                </div>`;

            // Player 2 Stats
            content += `
                <div class="player-stats-section ${winner === 2 ? 'winner-highlight' : ''}">
                    <h3>${winner === 2 ? '🏆 ' : ''}${stats.player2.name}</h3>
                    <div class="stats-row">
                        <span class="stats-label">Match Average:</span>
                        <span class="stats-value">${stats.player2.average}</span>
                    </div>`;

            // Show leg averages if available
            if (fullStats.player2.legAverages && fullStats.player2.legAverages.length > 0) {
                const avgLegAvg = fullStats.player2.legAverages.reduce((sum, leg) => sum + leg.average, 0) / fullStats.player2.legAverages.length;
                content += `
                    <div class="stats-row">
                        <span class="stats-label">? Leg Average:</span>
                        <span class="stats-value">${avgLegAvg.toFixed(1)}</span>
                    </div>
                    <div class="stats-row">
                        <span class="stats-label">Legs gespielt:</span>
                        <span class="stats-value">${fullStats.player2.legAverages.length}</span>
                    </div>`;
            }

            content += `
                    <div class="stats-row">
                        <span class="stats-label">Legs gewonnen:</span>
                        <span class="stats-value">${gameState.player2.legs}</span>
                    </div>`;

            if (gameState.player1.sets > 0 || gameState.player2.sets > 0) {
                content += `
                    <div class="stats-row">
                        <span class="stats-label">Sets gewonnen:</span>
                        <span class="stats-value">${gameState.player2.sets}</span>
                    </div>`;
            }

            if (stats.player2.maximums > 0) {
                content += `
                    <div class="stats-row">
                        <span class="stats-label">180er:</span>
                        <span class="stats-value">${stats.player2.maximums}</span>
                    </div>`;
            }

            if (stats.player2.highFinishes > 0) {
                content += `
                    <div class="stats-row">
                        <span class="stats-label">High Finishes (=100):</span>
                        <span class="stats-value">${stats.player2.highFinishes}</span>
                    </div>`;
            }

            if (stats.player2.score26 > 0) {
                content += `
                    <div class="stats-row">
                        <span class="stats-label">26er Scores:</span>
                        <span class="stats-value">${stats.player2.score26}</span>
                    </div>`;
            }

            content += `
                    <div class="stats-row">
                        <span class="stats-label">Checkouts:</span>
                        <span class="stats-value">${stats.player2.checkouts}</span>
                    </div>
                </div>
            </div>`;

            // Match overview
            const totalThrows = fullStats.player1.totalThrows + fullStats.player2.totalThrows;
            const totalMaximums = stats.player1.maximums + stats.player2.maximums;
            const totalHighFinishes = stats.player1.highFinishes + stats.player2.highFinishes;
            const total26Scores = stats.player1.score26 + stats.player2.score26;
            
            content += `
                <div style="margin-top: 20px; text-align: center; color: #4a5568;">
                    <p><strong>Match-Übersicht:</strong></p>
                    <p>Gesamte Darts geworfen: ${totalThrows}`;
            
            if (totalMaximums > 0) content += ` • 180er: ${totalMaximums}`;
            if (totalHighFinishes > 0) content += ` • High Finishes: ${totalHighFinishes}`;
            if (total26Scores > 0) content += ` • 26er Scores: ${total26Scores}`;
            
            content += '</p></div>';

            return content;
            
        } catch (error) {
            console.error('? [DART-UI] Error generating statistics content:', error);
            return '<p>? Fehler beim Laden der Statistiken</p>';
        }
    }

    /**
     * ? NEU: Navigate back to tournament
     */
    navigateBackToTournament() {
        const urlParams = new URLSearchParams(window.location.search);
        const tournamentId = urlParams.get('tournament') || urlParams.get('t');
        
        if (tournamentId) {
            const tournamentUrl = `/tournament-interface.html?tournament=${tournamentId}`;
            console.log(`🔗 [DART-UI] Redirecting to: ${tournamentUrl}`);
            window.location.href = tournamentUrl;
        } else {
            console.warn('⚠️ [DART-UI] No tournament ID found, going to dashboard');
            window.location.href = '/dashboard.html';
        }
    }

    /**
     * Show temporary message
     */
    showMessage(text, type = 'success') {
        const message = document.createElement('div');
        message.className = `message ${type}`;
        message.textContent = text;
        
        this.elements.gameStatus.parentNode.insertBefore(message, this.elements.gameStatus.nextSibling);
        
        setTimeout(() => {
            if (message.parentNode) {
                message.parentNode.removeChild(message);
            }
        }, 3000);
    }

    /**
     * 🏁 NEU: Show match completed message - delegated to completion module
     */
    showMatchCompletedMessage() {
        if (this.completion) {
            this.completion.showMatchCompletedMessage();
        } else {
            // Fallback if completion module not available
            console.warn('⚠️ [DART-UI] Completion module not available - showing fallback');
            alert('🏁 Match beendet!\n\nDas Ergebnis wurde erfolgreich übertragen.\nDie Seite kann nun geschlossen werden.');
        }
    }

    /**
     * Show loading state
     */
    showLoading() {
        this.elements.loadingContainer.classList.remove('hidden');
        this.elements.gameContainer.classList.add('hidden');
    }

    /**
     * Show game interface
     */
    showGame() {
        this.elements.loadingContainer.classList.add('hidden');
        this.elements.gameContainer.classList.remove('hidden');
        this.elements.gameContainer.classList.add('fade-in');
        
        // Initialize keypad when game is shown
        if (this.keypadState) {
            this.resetKeypad();
        }
        
        console.log('🎮 [DART-UI] Game interface shown with keypad ready');
    }

    /**
     * Handle keyboard shortcuts
     */
    /**
     * 📝 GEÄNDERT: Delegate keyboard functionality to separate module
     */
    setupKeyboardShortcuts() {
        // Keyboard functionality is now handled by the DartScoringKeyboard module
        // which is initialized in the initialize() method
        console.log('⌨️ [DART-UI] Keyboard shortcuts delegated to DartScoringKeyboard module');
    }

    /**
     * ? NEU: Keyboard control methods for external keyboard module
     */
    enableKeyboard() {
        if (this.keyboard) {
            this.keyboard.enable();
        }
    }

    disableKeyboard() {
        if (this.keyboard) {
            this.keyboard.disable();
        }
    }

    getKeyboardHelpText() {
        if (this.keyboard) {
            return this.keyboard.getHelpText();
        }
        return '';
    }

    /**
     * Setup responsive behavior
     */
    setupResponsive() {
               let resizeTimer;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(() => {
                this.updatePlayerDisplays();
            }, 250);
        });
    }

    /**
     * Initialize UI fully
     */
    initializeComplete() {
        this.setupKeyboardShortcuts(); // Delegate to keyboard module
        this.setupResponsive();
        
        // ? NEU: Log keyboard help for debugging
        if (this.keyboard) {
            console.log('❓ [DART-UI] Available keyboard shortcuts:', this.getKeyboardHelpText());
        }
        
        console.log('✅ [DART-UI] UI initialization complete with keyboard and animations modules');
    }

    /**
     * 🧹 NEU: Cleanup method for animations, keyboard and completion modules
     */
    cleanup() {
        if (this.animations) {
            this.animations.cleanup();
        }
        
        if (this.keyboard) {
            this.keyboard.cleanup();
        }

        if (this.completion) {
            this.completion.cleanup();
        }
        
        console.log('🧹 [DART-UI] UI cleaned up');
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringUI;
} else {
    window.DartScoringUI = DartScoringUI;
}

console.log('📱 [DART-UI] Dart Scoring UI module loaded');