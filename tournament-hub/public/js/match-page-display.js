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

        const isFinished = this.currentMatch.status === 'finished';
        const hasResult = this.currentMatch.result && 
                         (this.currentMatch.result.sets1 !== null || 
                          this.currentMatch.result.legs1 !== null);

        let html = `
            <div class="match-header">
                <h2 class="match-title">Match #${this.currentMatch.id || 'N/A'}</h2>
                <div class="match-status-badge ${this.getStatusClass(this.currentMatch.status)}">
                    ${this.getMatchStatusText(this.currentMatch.status)}
                </div>
            </div>

            <div class="players-section">
                ${this.renderPlayerScoreCard(this.currentMatch.player1, this.currentMatch.result?.sets1, this.currentMatch.result?.legs1, '1')}
                <div class="vs-divider">VS</div>
                ${this.renderPlayerScoreCard(this.currentMatch.player2, this.currentMatch.result?.sets2, this.currentMatch.result?.legs2, '2')}
            </div>

            ${isFinished ? this.renderMatchResult() : this.renderResultForm()}
        `;

        mainArea.innerHTML = html;

        // Add event listeners for the result form if it exists
        if (!isFinished) {
            this.setupResultFormHandlers();
        }
    }

    /**
     * Render a player score card
     */
    renderPlayerScoreCard(playerName, sets, legs, playerNumber) {
        const displayName = playerName || `Spieler ${playerNumber}`;
        const displaySets = sets !== null && sets !== undefined ? sets : '-';
        const displayLegs = legs !== null && legs !== undefined ? legs : '-';

        return `
            <div class="player-card player-${playerNumber}">
                <div class="player-name">${displayName}</div>
                <div class="player-scores">
                    <div class="score-item">
                        <span class="score-label">Sets</span>
                        <span class="score-value">${displaySets}</span>
                    </div>
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
        const result = this.currentMatch.result;
        const winner = this.currentMatch.winner;
        
        return `
            <div class="match-result-section">
                <h3>🏆 Endergebnis</h3>
                <div class="final-result">
                    <div class="result-display">
                        Sets: ${result?.sets1 || 0} - ${result?.sets2 || 0}<br>
                        Legs: ${result?.legs1 || 0} - ${result?.legs2 || 0}
                    </div>
                    ${winner ? `<div class="winner-announcement">🎉 Gewinner: ${winner}</div>` : ''}
                </div>
                ${result?.notes ? `<div class="match-notes"><strong>Notizen:</strong> ${result.notes}</div>` : ''}
                <div class="match-completed-badge">
                    ✅ Match abgeschlossen
                </div>
            </div>
        `;
    }

    /**
     * Render result submission form
     */
    renderResultForm() {
        return `
            <div class="result-form-section">
                <h3>📊 Ergebnis eingeben</h3>
                <form id="matchResultForm" class="result-form">
                    <div class="form-row">
                        <div class="input-group">
                            <label for="sets1">${this.currentMatch.player1 || 'Spieler 1'} - Sets:</label>
                            <input type="number" id="sets1" name="sets1" min="0" max="99" required>
                        </div>
                        <div class="input-group">
                            <label for="sets2">${this.currentMatch.player2 || 'Spieler 2'} - Sets:</label>
                            <input type="number" id="sets2" name="sets2" min="0" max="99" required>
                        </div>
                    </div>
                    
                    <div class="form-row">
                        <div class="input-group">
                            <label for="legs1">${this.currentMatch.player1 || 'Spieler 1'} - Legs:</label>
                            <input type="number" id="legs1" name="legs1" min="0" max="99" required>
                        </div>
                        <div class="input-group">
                            <label for="legs2">${this.currentMatch.player2 || 'Spieler 2'} - Legs:</label>
                            <input type="number" id="legs2" name="legs2" min="0" max="99" required>
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

            // Collect form data
            const formData = new FormData(form);
            const resultData = {
                sets1: parseInt(formData.get('sets1')) || 0,
                sets2: parseInt(formData.get('sets2')) || 0,
                legs1: parseInt(formData.get('legs1')) || 0,
                legs2: parseInt(formData.get('legs2')) || 0,
                notes: formData.get('notes') || ''
            };

            console.log('📤 [MATCH-DISPLAY] Submitting result:', resultData);

            // Validate data
            const validation = this.validateResultData(resultData);
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
     * Validate result data
     */
    validateResultData(data) {
        if (data.sets1 < 0 || data.sets2 < 0 || data.legs1 < 0 || data.legs2 < 0) {
            return { valid: false, message: 'Negative Werte sind nicht erlaubt' };
        }

        if (data.sets1 === 0 && data.sets2 === 0 && data.legs1 === 0 && data.legs2 === 0) {
            return { valid: false, message: 'Ein gültiges Ergebnis ist erforderlich' };
        }

        // Check if both players have won (both have more than 0)
        const player1Won = data.sets1 > data.sets2 || (data.sets1 === data.sets2 && data.legs1 > data.legs2);
        const player2Won = data.sets2 > data.sets1 || (data.sets2 === data.sets1 && data.legs2 > data.legs1);
        
        if (!player1Won && !player2Won && (data.sets1 !== data.sets2 || data.legs1 !== data.legs2)) {
            return { valid: false, message: 'Unentschieden sind nur bei gleichen Sets und Legs möglich' };
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
        
        section.innerHTML = `
            <h3>📋 Spielregeln</h3>
            <div class="rules-grid">
                <div class="rule-item">
                    <span class="rule-label">Spielmodus:</span>
                    <span class="rule-value">${rules.gameMode || 'Standard'}</span>
                </div>
                <div class="rule-item">
                    <span class="rule-label">Finish-Modus:</span>
                    <span class="rule-value">${rules.finishMode || 'Single Out'}</span>
                </div>
                <div class="rule-item">
                    <span class="rule-label">Legs zum Sieg:</span>
                    <span class="rule-value">${rules.legsToWin || 'Standard'}</span>
                </div>
                ${rules.playWithSets ? `
                    <div class="rule-item">
                        <span class="rule-label">Sets zum Sieg:</span>
                        <span class="rule-value">${rules.setsToWin || 'Standard'}</span>
                    </div>
                    <div class="rule-item">
                        <span class="rule-label">Legs pro Set:</span>
                        <span class="rule-value">${rules.legsPerSet || 'Standard'}</span>
                    </div>
                ` : ''}
            </div>
        `;
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
            new Date(this.currentMatch.endTime).toLocaleString('de-DE') : '-';

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