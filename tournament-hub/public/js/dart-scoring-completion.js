/**
 * 🏁 Dart Scoring Match Completion Module
 * Handles the match completion display and navigation
 * 
 * @author Dart Tournament System
 * @version 1.0.0
 */

class DartScoringCompletion {
    constructor() {
        this.completionContainer = null;
        this.completionStyle = null;
    }

    /**
     * Show match completed message with success animation
     */
    showMatchCompletedMessage() {
        try {
            console.log('💬 [DART-COMPLETION] Showing match completed message');

            // Hide game interface if it exists
            const gameContainer = document.getElementById('game-container');
            if (gameContainer) {
                gameContainer.classList.add('hidden');
            }

            // Create completion message container
            this.createCompletionContainer();
            this.addCompletionStyles();
            this.setupEventListeners();

            // Add to page
            document.body.appendChild(this.completionContainer);

            console.log('✅ [DART-COMPLETION] Match completion message shown');

        } catch (error) {
            console.error('❌ [DART-COMPLETION] Error showing completion message:', error);
            // Fallback: show simple alert
            alert('🏁 Match beendet!\n\nDas Ergebnis wurde erfolgreich übertragen.\nDie Seite kann nun geschlossen werden.');
        }
    }

    /**
     * Create the completion container HTML
     */
    createCompletionContainer() {
        this.completionContainer = document.createElement('div');
        this.completionContainer.id = 'match-completion-container';
        this.completionContainer.className = 'completion-container';
        this.completionContainer.innerHTML = `
            <div class="completion-content">
                <div class="completion-header">
                    <h2>🏁 Match beendet</h2>
                    <p class="completion-subtitle">Das Ergebnis wurde erfolgreich übertragen</p>
                </div>
                
                <div class="completion-message">
                    <div class="success-icon">👍</div>
                    <h3>Übertragung erfolgreich!</h3>
                    <p>Das Match-Ergebnis und alle Statistiken wurden erfolgreich zum Tournament Hub übertragen.</p>
                </div>

                <div class="completion-actions">
                    <p><strong>Die Seite kann nun geschlossen werden.</strong></p>
                    <small>Oder nutzen Sie den Button unten, um zum Tournament zurückzukehren.</small>
                </div>

                <div class="completion-buttons">
                    <button id="close-window-btn" class="btn btn-primary">
                        🚪 Fenster schließen
                    </button>
                    <button id="back-to-tournament-btn" class="btn btn-secondary">
                        🔙 Zurück zum Tournament
                    </button>
                </div>
            </div>
        `;
    }

    /**
     * Add completion styles to document
     */
    addCompletionStyles() {
        this.completionStyle = document.createElement('style');
        this.completionStyle.textContent = `
            .completion-container {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 10000;
                animation: fadeIn 0.5s ease-in;
            }

            .completion-content {
                background: white;
                padding: 40px;
                border-radius: 20px;
                box-shadow: 0 20px 40px rgba(0,0,0,0.1);
                text-align: center;
                max-width: 500px;
                width: 90%;
                animation: slideInUp 0.6s ease-out;
            }

            .completion-header h2 {
                color: #2d3748;
                margin-bottom: 10px;
                font-size: 2.2em;
            }

            .completion-subtitle {
                color: #718096;
                margin-bottom: 30px;
                font-size: 1.1em;
            }

            .completion-message {
                background: #f7fafc;
                padding: 30px;
                border-radius: 15px;
                margin-bottom: 30px;
            }

            .success-icon {
                font-size: 4em;
                margin-bottom: 20px;
                animation: bounce 1s infinite;
            }

            .completion-message h3 {
                color: #38a169;
                margin-bottom: 15px;
                font-size: 1.5em;
            }

            .completion-message p {
                color: #4a5568;
                line-height: 1.6;
            }

            .completion-actions {
                margin-bottom: 30px;
            }

            .completion-actions p {
                color: #2d3748;
                font-size: 1.2em;
                margin-bottom: 10px;
            }

            .completion-actions small {
                color: #718096;
            }

            .completion-buttons {
                display: flex;
                gap: 15px;
                justify-content: center;
                flex-wrap: wrap;
            }

            .completion-buttons .btn {
                padding: 15px 30px;
                border: none;
                border-radius: 10px;
                font-size: 1.1em;
                font-weight: 600;
                cursor: pointer;
                transition: all 0.3s ease;
                min-width: 180px;
            }

            .btn-primary {
                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                color: white;
            }

            .btn-primary:hover {
                transform: translateY(-2px);
                box-shadow: 0 10px 20px rgba(0,0,0,0.1);
            }

            .btn-secondary {
                background: #e2e8f0;
                color: #4a5568;
            }

            .btn-secondary:hover {
                background: #cbd5e0;
                transform: translateY(-2px);
            }

            @keyframes fadeIn {
                from { opacity: 0; }
                to { opacity: 1; }
            }

            @keyframes slideInUp {
                from {
                    transform: translateY(30px);
                    opacity: 0;
                }
                to {
                    transform: translateY(0);
                    opacity: 1;
                }
            }

            @keyframes bounce {
                0%, 20%, 53%, 80%, 100% {
                    transform: translate3d(0,0,0);
                }
                40%, 43% {
                    transform: translate3d(0, -20px, 0);
                }
                70% {
                    transform: translate3d(0, -10px, 0);
                }
                90% {
                    transform: translate3d(0, -4px, 0);
                }
            }

            @media (max-width: 600px) {
                .completion-content {
                    padding: 30px 20px;
                    margin: 20px;
                }
                
                .completion-buttons {
                    flex-direction: column;
                    align-items: center;
                }
                
                .completion-buttons .btn {
                    width: 100%;
                    max-width: 280px;
                }
            }
        `;
        document.head.appendChild(this.completionStyle);
    }

    /**
     * Setup event listeners for completion buttons
     */
    setupEventListeners() {
        // Use event delegation to handle button clicks
        this.completionContainer.addEventListener('click', (e) => {
            if (e.target.id === 'close-window-btn') {
                this.handleCloseWindow();
            } else if (e.target.id === 'back-to-tournament-btn') {
                this.handleBackToTournament();
            }
        });
    }

    /**
     * Handle close window button click
     */
    handleCloseWindow() {
        console.log('🚪 [DART-COMPLETION] User requested window close');
        window.close();
    }

    /**
     * Handle back to tournament button click
     */
    handleBackToTournament() {
        console.log('🔙 [DART-COMPLETION] User requested back to tournament');
        this.navigateBackToTournament();
    }

    /**
     * Navigate back to tournament interface
     */
    navigateBackToTournament() {
        const urlParams = new URLSearchParams(window.location.search);
        const tournamentId = urlParams.get('tournament') || urlParams.get('t');

        if (tournamentId) {
            const tournamentUrl = `/tournament-interface.html?tournament=${tournamentId}`;
            console.log(`🔗 [DART-COMPLETION] Redirecting to: ${tournamentUrl}`);
            window.location.href = tournamentUrl;
        } else {
            console.warn('⚠️ [DART-COMPLETION] No tournament ID found, going to dashboard');
            window.location.href = '/dashboard.html';
        }
    }

    /**
     * Cleanup method to remove completion display
     */
    cleanup() {
        if (this.completionContainer && this.completionContainer.parentNode) {
            this.completionContainer.parentNode.removeChild(this.completionContainer);
            this.completionContainer = null;
        }

        if (this.completionStyle && this.completionStyle.parentNode) {
            this.completionStyle.parentNode.removeChild(this.completionStyle);
            this.completionStyle = null;
        }

        console.log('🧹 [DART-COMPLETION] Completion module cleaned up');
    }

    /**
     * Hide completion display
     */
    hide() {
        if (this.completionContainer) {
            this.completionContainer.style.display = 'none';
        }
    }

    /**
     * Show completion display (if already created)
     */
    show() {
        if (this.completionContainer) {
            this.completionContainer.style.display = 'flex';
        }
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringCompletion;
} else {
    window.DartScoringCompletion = DartScoringCompletion;
}