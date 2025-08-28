/**
 * Dart Scoring Main Module
 * Main entry point for the dart scoring application
 */
class DartScoringMain {
    constructor() {
        this.core = new DartScoringCore();
        this.ui = new DartScoringUI();
        this.isInitialized = false;
        
        console.log('🚀 [DART-MAIN] Dart Scoring Main initialized');
    }

    /**
     * Initialize the dart scoring application
     */
    async initialize() {
        try {
            console.log('🔄 [DART-MAIN] Starting application initialization...');
            
            // Get URL parameters
            const params = this.getUrlParameters();
            
            if (!params.tournamentId || !params.matchId) {
                throw new Error('Tournament ID or Match ID missing from URL');
            }

            console.log('📋 [DART-MAIN] URL parameters:', params);

            // Initialize core with match data
            const coreInitialized = await this.core.initialize(params.matchId, params.tournamentId);
            
            if (!coreInitialized) {
                throw new Error('Failed to initialize dart scoring core');
            }

            // Initialize UI with core
            this.ui.initialize(this.core);

            // Update UI with match data
            this.ui.updateMatchDisplay();
            this.ui.updatePlayerDisplays();
            this.ui.updateThrowHistory();

            // Complete UI initialization
            this.ui.initializeComplete();

            this.isInitialized = true;
            console.log('✅ [DART-MAIN] Application initialization complete');

        } catch (error) {
            console.error('❌ [DART-MAIN] Failed to initialize application:', error);
            this.showError(`Fehler beim Laden: ${error.message}`);
        }
    }

    /**
     * Get URL parameters
     */
    getUrlParameters() {
        const urlParams = new URLSearchParams(window.location.search);
        
        return {
            tournamentId: urlParams.get('tournament') || urlParams.get('t'),
            matchId: urlParams.get('match') || urlParams.get('m'),
            uuid: urlParams.get('uuid') === 'true'
        };
    }

    /**
     * Show error message
     */
    showError(message) {
        const container = document.getElementById('loadingContainer');
        if (container) {
            container.innerHTML = `
                <div style="text-align: center; color: #e53e3e; padding: 40px;">
                    <div style="font-size: 3em; margin-bottom: 20px;">⚠️</div>
                    <h2>Fehler beim Laden</h2>
                    <p>${message}</p>
                    <button onclick="location.reload()" style="
                        background: #4299e1; 
                        color: white; 
                        border: none; 
                        padding: 10px 20px; 
                        border-radius: 8px; 
                        cursor: pointer;
                        margin-top: 20px;
                    ">
                        Neu laden
                    </button>
                </div>
            `;
        }
    }

    /**
     * Handle visibility change (tab switching)
     */
    handleVisibilityChange() {
        if (document.hidden) {
            console.log('📱 [DART-MAIN] App hidden - pausing updates');
        } else {
            console.log('📱 [DART-MAIN] App visible - resuming updates');
            if (this.isInitialized) {
                // Refresh display when tab becomes visible
                this.ui.updatePlayerDisplays();
                this.ui.updateThrowHistory();
            }
        }
    }

    /**
     * Handle page unload
     */
    handlePageUnload() {
        console.log('🔄 [DART-MAIN] Page unloading - cleaning up...');
        
        if (this.core) {
            this.core.cleanup();
        }
    }

    /**
     * Setup global event listeners
     */
    setupGlobalEventListeners() {
        // Handle visibility changes (tab switching)
        document.addEventListener('visibilitychange', () => {
            this.handleVisibilityChange();
        });

        // Handle page unload
        window.addEventListener('beforeunload', () => {
            this.handlePageUnload();
        });

        // Handle online/offline status
        window.addEventListener('online', () => {
            console.log('🌐 [DART-MAIN] Back online');
            // Could reconnect socket here if needed
        });

        window.addEventListener('offline', () => {
            console.log('📱 [DART-MAIN] Gone offline');
            // Could show offline indicator here
        });
    }

    /**
     * Setup development helpers (only in dev mode)
     */
    setupDevHelpers() {
        // Only enable in development (check for localhost or specific dev domains)
        if (location.hostname === 'localhost' || location.hostname === '127.0.0.1') {
            
            // Global access to core and UI for debugging
            window.dartCore = this.core;
            window.dartUI = this.ui;
            
            // Development shortcuts
            window.devHelpers = {
                // Quick finish leg for testing
                finishLeg: (player = 1) => {
                    const targetPlayer = player === 1 ? this.core.gameState.player1 : this.core.gameState.player2;
                    const score = targetPlayer.score;
                    
                    // Set current player
                    this.core.gameState.currentPlayer = player;
                    
                    // Simulate finishing throw
                    if (score <= 60) {
                        const result = this.core.processThrow(score, 0, 0);
                        this.ui.updatePlayerDisplays();
                        this.ui.updateThrowHistory();
                        
                        if (result.type === 'leg_won') {
                            this.ui.showVictoryModal(result);
                        }
                    }
                },
                
                // Reset current leg
                resetLeg: () => {
                    this.core.initializeGameState();
                    this.ui.updatePlayerDisplays();
                    this.ui.updateThrowHistory();
                },
                
                // Add random throw
                randomThrow: () => {
                    const darts = [
                        Math.floor(Math.random() * 61),
                        Math.floor(Math.random() * 61),
                        Math.floor(Math.random() * 61)
                    ];
                    
                    const total = darts.reduce((a, b) => a + b, 0);
                    if (total <= 180) {
                        const result = this.core.processThrow(...darts);
                        this.ui.updatePlayerDisplays();
                        this.ui.updateThrowHistory();
                        
                        if (result.type === 'leg_won') {
                            this.ui.showVictoryModal(result);
                        }
                    }
                },
                
                // Show current game state
                showState: () => {
                    console.table(this.core.gameState);
                }
            };
            
            console.log('🛠️ [DART-MAIN] Development helpers enabled');
            console.log('Available commands: window.devHelpers');
        }
    }
}

// Initialize when DOM is ready
let dartScoringApp;

document.addEventListener('DOMContentLoaded', async () => {
    console.log('📄 [DART-MAIN] DOM loaded, initializing app...');
    
    // Create main app instance
    dartScoringApp = new DartScoringMain();
    
    // Setup global event listeners
    dartScoringApp.setupGlobalEventListeners();
    
    // Setup development helpers
    dartScoringApp.setupDevHelpers();
    
    // Initialize the application
    await dartScoringApp.initialize();
});

// Handle errors globally
window.addEventListener('error', (event) => {
    console.error('🚨 [DART-MAIN] Global error:', event.error);
    
    // Show user-friendly error message
    if (dartScoringApp && !dartScoringApp.isInitialized) {
        dartScoringApp.showError('Ein unerwarteter Fehler ist aufgetreten.');
    }
});

// Handle unhandled promise rejections
window.addEventListener('unhandledrejection', (event) => {
    console.error('🚨 [DART-MAIN] Unhandled promise rejection:', event.reason);
    
    // Prevent the default handling
    event.preventDefault();
    
    // Show user-friendly error message
    if (dartScoringApp && !dartScoringApp.isInitialized) {
        dartScoringApp.showError('Verbindungsfehler. Bitte versuchen Sie es erneut.');
    }
});

console.log('🎯 [DART-MAIN] Dart Scoring Main module loaded');