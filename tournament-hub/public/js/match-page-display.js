/**
 * Match Page Display Module
 * Handles the visual display and UI updates for match pages
 */
class MatchPageDisplay {
    constructor() {
        this.currentMatch = null;
        this.currentGameRules = null;
        
        console.log('🎨 [MATCH-DISPLAY] Match Page Display initialized');
    }

    /**
     * Update the entire display with new match and game rules data
     */
    updateDisplay(matchData, gameRules) {
        try {
            console.log('🔄 [MATCH-DISPLAY] Updating display with new data');
            
            this.currentMatch = matchData;
            this.currentGameRules = gameRules;

            // Update header information
            this.updateHeader();
            
            // Update main match area
            this.updateMainMatchArea();
            
            // Update sidebar sections
            this.updateGameRulesSection();
            this.updateMatchInfoSection();
            
            console.log('✅ [MATCH-DISPLAY] Display updated successfully');
        } catch (error) {
            console.error('🚫 [MATCH-DISPLAY] Error updating display:', error);
        }
    }

    /**
     * Update header information
     */
    updateHeader() {
        const titleElement = document.getElementById('matchTitle');
        const metaElement = document.getElementById('matchMeta');

        if (titleElement && this.currentMatch) {
            const player1 = this.currentMatch.player1 || 'Spieler 1';
            const player2 = this.currentMatch.player2 || 'Spieler 2';
            titleElement.textContent = `🎯 ${player1} vs ${player2}`;
        }

        if (metaElement && this.currentMatch) {
            const status = this.getMatchStatusText(this.currentMatch.status);
            const group = this.currentMatch.group ? ` • Gruppe: ${this.currentMatch.group}` : '';
            const round = this.currentMatch.round ? ` • Runde: ${this.currentMatch.round}` : '';
            metaElement.textContent = `Status: ${status}${group}${round}`;
        }
    }

    /**
     * Update main match area with score display and result form
     */
    updateMainMatchArea() {
        const mainArea = document.getElementById('mainMatchArea');
        if (!mainArea || !this.currentMatch) return;

        // ✅ KORRIGIERT: Strengere Status-Erkennung - nur bei echten Results als finished markieren
        const hasValidResult = this.hasValidMatchResult();
        const isExplicitlyFinished = this.currentMatch.status === 'finished' || 
                                   this.currentMatch.status === 'Finished' ||
                                   this.currentMatch.status === 'completed' ||
                                   this.currentMatch.status === 'Completed';

        // ✅ WICHTIG: Match ist nur dann finished wenn BEIDE Bedingungen erfüllt sind
        const isFinished = isExplicitlyFinished && hasValidResult;

        console.log('🔄 [MATCH-DISPLAY] Main area update:', {
            status: this.currentMatch.status,
            isExplicitlyFinished,
            hasValidResult,
            isFinished,
            player1Sets: this.currentMatch.player1Sets,
            player2Sets: this.currentMatch.player2Sets,
            player1Legs: this.currentMatch.player1Legs,
            player2Legs: this.currentMatch.player2Legs
        });

        let html = `
            <div class="match-header">
                <h2 class="match-title">Match #${this.currentMatch.id || 'N/A'}</h2>
                <div class="match-status-badge ${this.getStatusClass(this.currentMatch.status)}">
                    ${this.getMatchStatusText(this.currentMatch.status)}
                </div>
            </div>

            <div class="players-section">
                ${this.renderPlayerScoreCard(this.currentMatch.player1, this.getPlayerSets(1), this.getPlayerLegs(1), '1')}
                <div class="vs-divider">VS</div>
                ${this.renderPlayerScoreCard(this.currentMatch.player2, this.getPlayerSets(2), this.getPlayerLegs(2), '2')}
            </div>

            ${isFinished ? this.renderMatchResult() : this.renderResultForm()}
        `;

        mainArea.innerHTML = html;

        // Add event listeners for the result form if it exists and match is not finished
        if (!isFinished) {
            this.setupResultFormHandlers();
        }
    }

    /**
     * ✅ KORRIGIERT: Strengere Validierung für echte Match-Results
     */
    hasValidMatchResult() {
        // Check für sinnvolle Ergebnis-Daten (nicht nur 0-0)
        const player1Sets = this.getPlayerSets(1) || 0;
        const player2Sets = this.getPlayerSets(2) || 0;
        const player1Legs = this.getPlayerLegs(1) || 0;
        const player2Legs = this.getPlayerLegs(2) || 0;

        const playWithSets = this.currentGameRules?.playWithSets || false;

        // Bei Sets-Spiel: Mindestens ein Spieler muss Sets > 0 haben
        if (playWithSets) {
            const hasSetsResult = player1Sets > 0 || player2Sets > 0;
            const hasLegsResult = player1Legs > 0 || player2Legs > 0;
            
            console.log('🔍 [MATCH-DISPLAY] Sets validation:', {
                playWithSets, hasSetsResult, hasLegsResult,
                sets: `${player1Sets}-${player2Sets}`, legs: `${player1Legs}-${player2Legs}`
            });
            
            return hasSetsResult && hasLegsResult; // Beide müssen vorhanden sein
        } 
        // Bei Legs-Only: Mindestens ein Spieler muss Legs > 0 haben
        else {
            const hasLegsResult = player1Legs > 0 || player2Legs > 0;
            
            console.log('🔍 [MATCH-DISPLAY] Legs validation:', {
                playWithSets, hasLegsResult,
                legs: `${player1Legs}-${player2Legs}`
            });
            
            return hasLegsResult;
        }
    }

    /**
     * ✅ DEPRECATED: Alte hasMatchResult Methode - ersetzt durch hasValidMatchResult
     * Wird nur noch für Legacy-Kompatibilität behalten
     */
    hasMatchResult() {
        return this.hasValidMatchResult();
    }

    /**
     * Get player sets from various data sources
     */
    getPlayerSets(playerNumber) {
        const playerField = `player${playerNumber}Sets`;
        
        // Priorität 1: Direkte Player-Felder im Match
        if (this.currentMatch[playerField] !== undefined && this.currentMatch[playerField] !== null) {
            return this.currentMatch[playerField];
        }
        
        // Priorität 2: Result-Objekt (Legacy-Format)
        if (this.currentMatch.result) {
            const resultField = playerNumber === 1 ? 'sets1' : 'sets2';
            if (this.currentMatch.result[resultField] !== undefined && this.currentMatch.result[resultField] !== null) {
                return this.currentMatch.result[resultField];
            }
            
            // Alternative result field names
            const playerResultField = `player${playerNumber}Sets`;
            if (this.currentMatch.result[playerResultField] !== undefined && this.currentMatch.result[playerResultField] !== null) {
                return this.currentMatch.result[playerResultField];
            }
        }
        
        // ✅ KORRIGIERT: Nur null zurückgeben wenn kein gültiges Result vorhanden
        return null;
    }

    /**
     * Get player legs from various data sources
     */
    getPlayerLegs(playerNumber) {
        const playerField = `player${playerNumber}Legs`;
        
        // Priorität 1: Direkte Player-Felder im Match
        if (this.currentMatch[playerField] !== undefined && this.currentMatch[playerField] !== null) {
            return this.currentMatch[playerField];
        }
        
        // Priorität 2: Result-Objekt (Legacy-Format)
        if (this.currentMatch.result) {
            const resultField = playerNumber === 1 ? 'legs1' : 'legs2';
            if (this.currentMatch.result[resultField] !== undefined && this.currentMatch.result[resultField] !== null) {
                return this.currentMatch.result[resultField];
            }
            
            // Alternative result field names
            const playerResultField = `player${playerNumber}Legs`;
            if (this.currentMatch.result[playerResultField] !== undefined && this.currentMatch.result[playerResultField] !== null) {
                return this.currentMatch.result[playerResultField];
            }
        }
        
        // ✅ KORRIGIERT: Nur null zurückgeben wenn kein gültiges Result vorhanden
        return null;
    }

    /**
     * Render a player score card
     */
    renderPlayerScoreCard(playerName, sets, legs, playerNumber) {
        const displayName = playerName || `Spieler ${playerNumber}`;
        
        // ✅ KORRIGIERT: Bessere Anzeige-Logik für Sets und Legs
        const playWithSets = this.currentGameRules?.playWithSets || false;
        
        // Zeige nur Werte an, wenn sie wirklich existieren (nicht null)
        const displaySets = (sets !== null && sets !== undefined) ? sets : '-';
        const displayLegs = (legs !== null && legs !== undefined) ? legs : '-' ;

        // ✅ ERWEITERT: Debugging-Info für Score-Cards
        console.log(`🃏 [MATCH-DISPLAY] Rendering player ${playerNumber} card:`, {
            playerName: displayName,
            sets: sets,
            legs: legs,
            displaySets: displaySets,
            displayLegs: displayLegs,
            playWithSets: playWithSets
        });

        return `
            <div class="player-card player-${playerNumber}">
                <div class="player-name">${displayName}</div>
                <div class="player-scores">
                    ${playWithSets ? `
                        <div class="score-item">
                            <span class="score-label">Sets</span>
                            <span class="score-value">${displaySets}</span>
                        </div>
                    ` : ''}
                    <div class="score-item">
                        <span class="score-label">Legs</span>
                        <span class="score-value">${displayLegs}</span>
                    </div>
                </div>
            </div>
        `;
    }

    /**
     * Render match result (for finished matches)
     */
    renderMatchResult() {
        // ✅ KORRIGIERT: Verwende die verbesserten Getter-Methoden für Sets und Legs
        const player1Sets = this.getPlayerSets(1);
        const player2Sets = this.getPlayerSets(2);
        const player1Legs = this.getPlayerLegs(1);
        const player2Legs = this.getPlayerLegs(2);
        
        const winner = this.currentMatch.winner || this.determineWinner(player1Sets, player2Sets, player1Legs, player2Legs);
        const playWithSets = this.currentGameRules?.playWithSets || false;
        
        // ✅ ERWEITERT: Detailliertes Logging für Debugging
        console.log('🏆 [MATCH-DISPLAY] Rendering match result:', {
            player1Sets, player2Sets, player1Legs, player2Legs,
            winner, playWithSets,
            matchStatus: this.currentMatch.status,
            hasResult: this.hasMatchResult()
        });
        
        return `
            <div class="match-result-section">
                <h3>🏆 Endergebnis</h3>
                <div class="final-result">
                    <div class="result-display">
                        ${playWithSets ? `Sets: ${player1Sets || 0} - ${player2Sets || 0}<br>` : ''}

                        Legs: ${player1Legs || 0} - ${player2Legs || 0}
                    </div>
                    ${winner ? `<div class="winner-announcement">🎉 Gewinner: ${winner}</div>` : ''}

                </div>
                ${this.currentMatch.notes || this.currentMatch.result?.notes ? `
                    <div class="match-notes">
                        <strong>Notizen:</strong> ${this.currentMatch.notes || this.currentMatch.result?.notes}
                    </div>
                ` : '' }
                <div class="match-completed-badge">
                    ✅ Match abgeschlossen
                </div>
                <div class="match-result-actions">
                    <button onclick="window.history.back()" class="secondary-button">
                        ← Zurück zur Übersicht
                    </button>
                </div>
            </div>
        `;
    }

    /**
     * Determine winner based on scores
     */
    determineWinner(player1Sets, player2Sets, player1Legs, player2Legs) {
        const playWithSets = this.currentGameRules?.playWithSets || false;
        
        if (playWithSets) {
            // Bei Sets: Wer mehr Sets hat, gewinnt
            if (player1Sets > player2Sets) {
                return this.currentMatch.player1;
            } else if (player2Sets > player1Sets) {
                return this.currentMatch.player2;
            }
        } else {
            // Bei Legs Only: Wer mehr Legs hat, gewinnt
            if (player1Legs > player2Legs) {
                return this.currentMatch.player1;
            } else if (player2Legs > player1Legs) {
                return this.currentMatch.player2;
            }
        }
        
        return null; // Unentschieden oder nicht bestimmbar
    }

    /**
     * Render result submission form based on game rules
     */
    renderResultForm() {
        // ✅ KORRIGIERT: Form basiert jetzt auf Game Rules
        const playWithSets = this.currentGameRules?.playWithSets || false;
        const setsToWin = this.currentGameRules?.setsToWin || 3;
        const legsToWin = this.currentGameRules?.legsToWin || 3;
        
        console.log('🎮 [MATCH-DISPLAY] Rendering form with game rules:', {
            playWithSets,
            setsToWin,
            legsToWin
        });
        
        return `
            <div class="result-form-section">
                <h3>📊 Ergebnis eingeben</h3>
                ${playWithSets ? `
                    <div class="game-rules-info">
                        <span class="rules-hint">📋 Spiel-Modus: Sets (Best of ${setsToWin}, Legs pro Set: ${legsToWin})</span>
                    </div>
                ` : `
                    <div class="game-rules-info">
                        <span class="rules-hint">📋 Spiel-Modus: Legs Only (Best of ${legsToWin})</span>
                    </div>
                `}
                <form id="matchResultForm" class="result-form">
                    ${playWithSets ? `
                        <div class="form-section">
                            <h4>Sets</h4>
                            <div class="form-row">
                                <div class="input-group">
                                    <label for="sets1">${this.currentMatch.player1 || 'Spieler 1'} - Sets:</label>
                                    <input type="number" id="sets1" name="sets1" min="0" max="${setsToWin}" value="0" required>
                                </div>
                                <div class="input-group">
                                    <label for="sets2">${this.currentMatch.player2 || 'Spieler 2'} - Sets:</label>
                                    <input type="number" id="sets2" name="sets2" min="0" max="${setsToWin}" value="0" required>
                                </div>
                            </div>
                        </div>
                    ` : ''}
                    
                    <div class="form-section">
                        <h4>${playWithSets ? 'Legs (Gesamt)' : 'Legs'}</h4>
                        <div class="form-row">
                            <div class="input-group">
                                <label for="legs1">${this.currentMatch.player1 || 'Spieler 1'} - Legs:</label>
                                <input type="number" id="legs1" name="legs1" min="0" max="99" value="0" required>
                            </div>
                            <div class="input-group">
                                <label for="legs2">${this.currentMatch.player2 || 'Spieler 2'} - Legs:</label>
                                <input type="number" id="legs2" name="legs2" min="0" max="99" value="0" required>
                            </div>
                        </div>
                    </div>
                    
                    <div class="form-row">
                        <div class="input-group full-width">
                            <label for="notes">Notizen (optional):</label>
                            <textarea id="notes" name="notes" rows="3" placeholder="Zusätzliche Informationen zum Match..."></textarea>
                        </div>
                    </div>
                    
                    <button type="submit" class="submit-button" id="submitResultBtn">
                        📤 Ergebnis übertragen
                    </button>
                    
                    <div id="resultMessage" class="message"></div>
                </form>
            </div>
        `;
    }

    /**
     * Setup event handlers for result form
     */
    setupResultFormHandlers() {
        const form = document.getElementById('matchResultForm');
        if (!form) return;

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            await this.handleResultSubmission(form);
        });

        // Add real-time validation
        const inputs = form.querySelectorAll('input[type="number"]');
        inputs.forEach(input => {
            input.addEventListener('input', () => this.validateFormInput(input));
        });
    }

    /**
     * Handle result form submission
     */
    async handleResultSubmission(form) {
        try {
            const submitBtn = document.getElementById('submitResultBtn');
            const messageDiv = document.getElementById('resultMessage');
            
            // Show loading state
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<div class="loading-spinner"></div> Übertrage...';
            }

            // ✅ KORRIGIERT: Berücksichtige playWithSets bei der Datensammlung
            const playWithSets = this.currentGameRules?.playWithSets || false;
            
            // Collect form data
            const formData = new FormData(form);
            let resultData = {
                player1Legs: parseInt(formData.get('legs1')) || 0,
                player2Legs: parseInt(formData.get('legs2')) || 0,
                notes: formData.get('notes') || ''
            };
            
            // Sets nur hinzufügen wenn playWithSets = true
            if (playWithSets) {
                resultData.player1Sets = parseInt(formData.get('sets1')) || 0;
                resultData.player2Sets = parseInt(formData.get('sets2')) || 0;
            } else {
                // Sets auf 0 setzen wenn nicht mit Sets gespielt wird
                resultData.player1Sets = 0;
                resultData.player2Sets = 0;
            }

            console.log('📤 [MATCH-DISPLAY] Submitting result:', resultData);
            console.log('🎮 [MATCH-DISPLAY] Game rules context:', {
                playWithSets,
                setsRequired: playWithSets
            });

            // Validate data based on game rules
            const validation = this.validateResultData(resultData, playWithSets);
            if (!validation.valid) {
                throw new Error(validation.message);
            }

            // Submit via core module
            const response = await window.matchPageCore.submitMatchResult(resultData);
            
            // Show success message
            this.showMessage(messageDiv, 'success', '✅ Ergebnis erfolgreich übertragen!');
            
            // The display will be updated automatically when the match-updated event is received
            
        } catch (error) {
            console.error('🚫 [MATCH-DISPLAY] Error submitting result:', error);
            
            const messageDiv = document.getElementById('resultMessage');
            this.showMessage(messageDiv, 'error', `❌ Fehler: ${error.message}`);
            
            // Reset submit button
            const submitBtn = document.getElementById('submitResultBtn');
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = '📤 Ergebnis übertragen';
            }
        }
    }

    /**
     * Validate result data based on game rules
     */
    validateResultData(data, playWithSets = false) {
        console.log('🔍 [MATCH-DISPLAY] Validating result data:', { data, playWithSets });
        
        // Negative Werte prüfen
        if (data.player1Legs < 0 || data.player2Legs < 0) {
            return { valid: false, message: 'Negative Leg-Werte sind nicht erlaubt' };
        }
        
        if (playWithSets && (data.player1Sets < 0 || data.player2Sets < 0)) {
            return { valid: false, message: 'Negative Set-Werte sind nicht erlaubt' };
        }

        // Mindestens ein gültiges Ergebnis erforderlich
        if (playWithSets) {
            // Bei Sets: Mindestens ein Set muss gespielt worden sein
            if (data.player1Sets === 0 && data.player2Sets === 0) {
                return { valid: false, message: 'Mindestens ein Set muss gespielt worden sein' };
            }
            
            // Sets-Winner Validation
            if (data.player1Sets === data.player2Sets && data.player1Sets > 0) {
                return { valid: false, message: 'Unentschieden bei Sets ist nicht erlaubt' };
            }
        } else {
            // Bei Legs Only: Mindestens ein Leg muss gespielt worden sein
            if (data.player1Legs === 0 && data.player2Legs === 0) {
                return { valid: false, message: 'Mindestens ein Leg muss gespielt worden sein' };
            }
            
            // Legs Winner Validation (nur bei Legs-only)
            if (data.player1Legs === data.player2Legs && data.player1Legs > 0) {
                return { valid: false, message: 'Unentschieden bei Legs ist nicht erlaubt' };
            }
        }

        // Game Rules Validierung
        const setsToWin = this.currentGameRules?.setsToWin || 3;
        const legsToWin = this.currentGameRules?.legsToWin || 3;
        
        if (playWithSets) {
            // Bei Sets: Ein Spieler muss die erforderliche Anzahl Sets erreicht haben
            if (data.player1Sets < setsToWin && data.player2Sets < setsToWin) {
                return { 
                    valid: false, 
                    message: `Mindestens ein Spieler muss ${setsToWin} Sets erreichen` 
                };
            }
            
            // Maximale Sets prüfen
            if (data.player1Sets > setsToWin || data.player2Sets > setsToWin) {
                return { 
                    valid: false, 
                    message: `Maximum ${setsToWin} Sets erlaubt` 
                };
            }
        } else {
            // Bei Legs Only: Ein Spieler muss die erforderliche Anzahl Legs erreicht haben
            if (data.player1Legs < legsToWin && data.player2Legs < legsToWin) {
                return { 
                    valid: false, 
                    message: `Mindestens ein Spieler muss ${legsToWin} Legs erreichen` 
                };
            }
        }

        return { valid: true };
    }

    /**
     * Validate individual form input
     */
    validateFormInput(input) {
        const value = parseInt(input.value);
        
        if (isNaN(value) || value < 0) {
            input.style.borderColor = '#e53e3e';
        } else {
            input.style.borderColor = '#e2e8f0';
        }
    }

    /**
     * Update game rules section in sidebar
     */
    updateGameRulesSection() {
        const section = document.getElementById('gameRulesSection');
        if (!section) return;

        const rules = this.currentGameRules || {};
        
        // ✅ ERWEITERT: Verbesserte Game Rules Anzeige mit mehr Details
        const gamePoints = rules.gamePoints || 501;
        const gameMode = rules.gameMode || 'Standard';
        const finishMode = this.formatFinishMode(rules.finishMode || 'DoubleOut');
        const legsToWin = rules.legsToWin || 'Standard';
        const setsToWin = rules.setsToWin || 'Standard';
        const legsPerSet = rules.legsPerSet || 'Standard';
        
        section.innerHTML = `
            <h3>📋 Spielregeln</h3>
            <div class="rules-grid">
                <div class="rule-item">
                    <span class="rule-label">Spiel-Punkte:</span>
                    <span class="rule-value">${gamePoints}</span>
                </div>
                <div class="rule-item">
                    <span class="rule-label">Spielmodus:</span>
                    <span class="rule-value">${gameMode}</span>
                </div>
                <div class="rule-item">
                    <span class="rule-label">Finish-Modus:</span>
                    <span class="rule-value">${finishMode}</span>
                </div>
                <div class="rule-item">
                    <span class="rule-label">Legs zum Sieg:</span>
                    <span class="rule-value">${legsToWin}</span>
                </div>
                ${rules.playWithSets ? `
                    <div class="rule-item">
                        <span class="rule-label">Sets zum Sieg:</span>
                        <span class="rule-value">${setsToWin}</span>
                    </div>
                    <div class="rule-item">
                        <span class="rule-label">Legs pro Set:</span>
                        <span class="rule-value">${legsPerSet}</span>
                    </div>
                ` : ''}
                ${rules.maxThrowsPerLeg ? `
                    <div class="rule-item">
                        <span class="rule-label">Max. Würfe:</span>
                        <span class="rule-value">${rules.maxThrowsPerLeg}</span>
                    </div>
                ` : ''}
                ${rules.checkoutMode && rules.checkoutMode !== 'Any' ? `
                    <div class="rule-item">
                        <span class="rule-label">Checkout-Modus:</span>
                        <span class="rule-value">${this.formatCheckoutMode(rules.checkoutMode)}</span>
                    </div>
                ` : ''}
            </div>
            ${rules.description ? `
                <div class="rules-description">
                    <strong>Beschreibung:</strong><br>
                    ${rules.description}
                </div>
            ` : ''}
        `;
    }

    /**
     * Format finish mode for display
     */
    formatFinishMode(finishMode) {
        const modes = {
            'DoubleOut': 'Double Out',
            'SingleOut': 'Single Out',
            'MasterOut': 'Master Out',
            'StraightOut': 'Straight Out'
        };
        return modes[finishMode] || finishMode;
    }

    /**
     * Format checkout mode for display
     */
    formatCheckoutMode(checkoutMode) {
        const modes = {
            'Any': 'Beliebig',
            'Double': 'Double',
            'Triple': 'Triple',
            'Bull': 'Bull',
            'DoubleOrBull': 'Double oder Bull'
        };
        return modes[checkoutMode] || checkoutMode;
    }

    /**
     * Update match info section in sidebar
     */
    updateMatchInfoSection() {
        const section = document.getElementById('matchInfoSection');
        if (!section || !this.currentMatch) return;

        const startTime = this.currentMatch.startTime ? 
            new Date(this.currentMatch.startTime).toLocaleString('de-DE') : 'Nicht gestartet';
        const endTime = this.currentMatch.endTime ? 
            new Date(this.currentMatch.endTime).toLocaleString('de-DE') : '-' ;

        section.innerHTML = `
            <h3>ℹ️ Match-Info</h3>
            <div class="info-grid">
                <div class="info-item">
                    <span class="info-label">Match-ID:</span>
                    <span class="info-value">${this.currentMatch.id || 'N/A'}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Status:</span>
                    <span class="info-value status-${this.currentMatch.status}">
                        ${this.getMatchStatusText(this.currentMatch.status)}
                    </span>
                </div>
                ${this.currentMatch.group ? `
                    <div class="info-item">
                        <span class="info-label">Gruppe:</span>
                        <span class="info-value">${this.currentMatch.group}</span>
                    </div>
                ` : ''}
                ${this.currentMatch.round ? `
                    <div class="info-item">
                        <span class="info-label">Runde:</span>
                        <span class="info-value">${this.currentMatch.round}</span>
                    </div>
                ` : ''}
                <div class="info-item">
                    <span class="info-label">Gestartet:</span>
                    <span class="info-value">${startTime}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Beendet:</span>
                    <span class="info-value">${endTime}</span>
                </div>
            </div>
        `;
    }

    /**
     * Handle match update events
     */
    handleMatchUpdate(matchData) {
        console.log('🔄 [MATCH-DISPLAY] Handling match update');
        this.updateDisplay(matchData, this.currentGameRules);
    }

    /**
     * Handle game rules update events
     */
    handleGameRulesUpdate(gameRules) {
        console.log('📋 [MATCH-DISPLAY] Handling game rules update');
        this.currentGameRules = gameRules;
        this.updateGameRulesSection();
    }

    /**
     * Update match display specifically (alias for updateDisplay)
     * This function is called from match-page-core.js
     */
    updateMatchDisplay(matchData) {
        console.log('🎨 [MATCH-DISPLAY] Updating match display specifically');
        this.updateDisplay(matchData, this.currentGameRules);
    }

    /**
     * Update tournament display
     */
    updateTournamentDisplay(tournamentData) {
        console.log('🏆 [MATCH-DISPLAY] Updating tournament display');
        
        // Update page title and meta information
        const titleElement = document.getElementById('matchTitle');
        const metaElement = document.getElementById('matchMeta');
        
        if (titleElement && tournamentData && tournamentData.name) {
            const currentTitle = titleElement.textContent;
            if (currentTitle.includes('Lade Match')) {
                titleElement.textContent = `🎯 ${tournamentData.name} - Match`;
            }
        }
    }

    /**
     * Update game rules display specifically
     */
    updateGameRulesDisplay(gameRules) {
        console.log('📋 [MATCH-DISPLAY] Updating game rules display speziell');
        this.currentGameRules = gameRules;
        this.updateGameRulesSection();
    }

    /**
     * Get match status text in German
     */
    getMatchStatusText(status) {
        const statusMap = {
            'pending': 'Ausstehend',
            'notstarted': 'Nicht gestartet',
            'inprogress': 'Läuft',
            'finished': 'Beendet',
            'bye': 'Freilos'
        };
        return statusMap[status] || 'Unbekannt';
    }

    /**
     * Get CSS class for status
     */
    getStatusClass(status) {
        return `status-${status}`;
    }

    /**
     * Show message in message div
     */
    showMessage(element, type, message) {
        if (!element) return;
        
        element.className = `message ${type}`;
        element.textContent = message;
        element.style.display = 'block';
        
        // Hide after 5 seconds for success messages
        if (type === 'success') {
            setTimeout(() => {
                element.style.display = 'none';
            }, 5000);
        }
    }
}

// Additional CSS for the display components
const additionalStyles = `
    <style>
        .match-title {
            color: #2d3748;
            margin-bottom: 20px;
            font-size: 1.5em;
        }

        .match-status-badge {
            padding: 6px 12px;
            border-radius: 20px;
            font-size: 0.85em;
            font-weight: bold;
            text-transform: uppercase;
        }

        .status-pending, .status-notstarted {
            background: #fed7d7;
            color: #c53030;
        }

        .status-inprogress {
            background: #fefcbf;
            color: #d69e2e;
        }

        .status-finished {
            background: #c6f6d5;
            color: #38a169;
        }

        .status-bye {
            background: #e2e8f0;
            color: #4a5568;
        }

        .players-section {
            display: grid;
            grid-template-columns: 1fr auto 1fr;
            gap: 20px;
            align-items: center;
            margin-bottom: 30px;
        }

        .player-card {
            background: #f7fafc;
            border-radius: 12px;
            padding: 20px;
            text-align: center;
        }

        .player-name {
            font-size: 1.2em;
            font-weight: bold;
            color: #2d3748;
            margin-bottom: 15px;
        }

        .player-scores {
            display: flex;
            gap: 20px;
            justify-content: center;
        }

        .score-item {
            text-align: center;
        }

        .score-label {
            font-size: 0.8em;
            color: #718096;
            display: block;
        }

        .score-value {
            font-size: 2em;
            font-weight: bold;
            color: #5a67d8;
        }

        .vs-divider {
            font-size: 1.5em;
            font-weight: bold;
            color: #718096;
            text-align: center;
        }

        .result-form-section, .match-result-section {
            background: #f7fafc;
            border-radius: 12px;
            padding: 25px;
            margin-top: 20px;
        }

        .result-form-section h3, .match-result-section h3 {
            margin-bottom: 20px;
            color: #2d3748;
        }

        .game-rules-info {
            background: #e6fffa;
            border-left: 4px solid #38b2ac;
            padding: 12px 16px;
            margin-bottom: 20px;
            border-radius: 6px;
        }

        .rules-hint {
            color: #234e52;
            font-size: 0.9em;
            font-weight: 600;
        }

        .form-section {
            margin-bottom: 25px;
        }

        .form-section h4 {
            color: #4a5568;
            margin-bottom: 15px;
            font-size: 1.1em;
            border-bottom: 1px solid #e2e8f0;
            padding-bottom: 8px;
        }

        .form-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
            margin-bottom: 15px;
        }

        .input-group.full-width {
            grid-column: 1 / -1;
        }

        .input-group label {
            font-size: 0.9em;
            color: #4a5568;
            margin-bottom: 5px;
            font-weight: bold;
            display: block;
        }

        .input-group input, .input-group textarea {
            padding: 10px;
            border: 2px solid #e2e8f0;
            border-radius: 8px;
            font-size: 16px;
            transition: border-color 0.3s ease;
            width: 100%;
        }

        .input-group input:focus, .input-group textarea:focus {
            outline: none;
            border-color: #667eea;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
        }

        .submit-button {
            width: 100%;
            background: linear-gradient(135deg, #48bb78 0%, #38a169 100%);
            color: white;
            border: none;
            padding: 15px;
            border-radius: 10px;
            font-size: 16px;
            font-weight: bold;
            cursor: pointer;
            transition: all 0.3s ease;
            margin-top: 15px;
        }

        .submit-button:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 8px 25px rgba(72, 187, 120, 0.4);
        }

        .submit-button:disabled {
            background: #cbd5e0;
            cursor: not-allowed;
            transform: none;
            box-shadow: none;
        }

        .message {
            padding: 12px;
            border-radius: 8px;
            margin-top: 15px;
            display: none;
        }

        .message.success {
            background: #c6f6d5;
            color: #22543d;
            border: 1px solid #9ae6b4;
        }

        .message.error {
            background: #fed7d7;
            color: #c53030;
            border: 1px solid #feb2b2;
        }

        .rules-grid, .info-grid {
            display: grid;
            gap: 12px;
        }

        .rule-item, .info-item {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 0;
            border-bottom: 1px solid #e2e8f0;
        }

        .rule-item:last-child, .info-item:last-child {
            border-bottom: none;
        }

        .rule-label, .info-label {
            font-weight: bold;
            color: #4a5568;
        }

        .rule-value, .info-value {
            color: #2d3748;
        }

        .final-result {
            background: white;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            margin: 20px 0;
        }

        .result-display {
            font-size: 1.2em;
            font-weight: bold;
            color: #2d3748;
            margin-bottom: 10px;
        }

        .winner-announcement {
            color: #38a169;
            font-size: 1.1em;
            font-weight: bold;
        }

        .match-notes {
            background: #edf2f7;
            padding: 15px;
            border-radius: 8px;
            margin: 15px 0;
            font-style: italic;
        }

        .match-completed-badge {
            background: #38a169;
            color: white;
            padding: 10px 20px;
            border-radius: 25px;
            text-align: center;
            font-weight: bold;
            margin-top: 15px;
        }

        .match-result-actions {
            margin-top: 20px;
            text-align: center;
        }

        .secondary-button {
            background: linear-gradient(135deg, #6c757d 0%, #495057 100%);
            color: white;
            border: none;
            padding: 12px 25px;
            border-radius: 8px;
            cursor: pointer;
            font-size: 16px;
            font-weight: 600;
            transition: all 0.3s ease;
        }

        .secondary-button:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 25px rgba(108, 117, 125, 0.4);
        }

        .rules-description {
            background: #edf2f7;
            padding: 12px;
            border-radius: 8px;
            margin-top: 15px;
            font-size: 0.9em;
            line-height: 1.4;
            color: #4a5568;
        }

        .rules-description strong {
            color: #2d3748;
        }

        @media (max-width: 768px) {
            .players-section {
                grid-template-columns: 1fr;
                gap: 15px;
            }

            .vs-divider {
                order: -1;
            }

            .form-row {
                grid-template-columns: 1fr;
            }

            .player-scores {
                gap: 15px;
            }

            .score-value {
                font-size: 1.5em;
            }
        }
    </style>
`;

// Inject additional styles
document.head.insertAdjacentHTML('beforeend', additionalStyles);

// Create global instance
window.matchPageDisplay = new MatchPageDisplay();

console.log('🎨 [MATCH-DISPLAY] Match Page Display module loaded');