/**
 * Match Page Core Module
 * Handles core functionality for individual match pages
 */
class MatchPageCore {
    constructor() {
        this.tournamentId = null;
        this.matchId = null;
        this.socket = null;
        this.matchData = null;
        this.gameRules = null;
        this.isConnected = false;
        this.connectionRetries = 0;
        this.maxRetries = 5;
        
        console.log('🎯 [MATCH-CORE] Match Page Core initialized');
    }

    /**
     * Initialize the match page with tournament and match IDs
     */
    initialize() {
        try {
            // Extract tournament and match IDs from URL
            const pathParts = window.location.pathname.split('/');
            this.tournamentId = pathParts[2];
            this.matchId = pathParts[3];

            if (!this.tournamentId || !this.matchId) {
                this.showError('Ungültige Match-URL');
                return false;
            }

            console.log(`🎯 [MATCH-CORE] Initializing match page for tournament ${this.tournamentId}, match ${this.matchId}`);
            
            // Setup back button
            this.setupBackButton();
            
            // Initialize socket connection
            this.initializeSocket();
            
            return true;
        } catch (error) {
            console.error('🚫 [MATCH-CORE] Error during initialization:', error);
            this.showError('Fehler beim Initialisieren der Match-Seite');
            return false;
        }
    }

    /**
     * Initialize match page with URL parameters
     */
    async initializeMatchPage() {
        try {
            console.log('🚀 [MATCH-CORE] Starting match page initialization...');
            
            // Extract tournament and match ID from URL
            const urlParams = this.extractUrlParameters();
            console.log('📋 [MATCH-CORE] URL Parameters:', urlParams);
            
            if (!urlParams.tournamentId || !urlParams.matchId) {
                throw new Error('Tournament ID oder Match ID fehlen in der URL');
            }

            // Store the IDs (matchId kann UUID oder numerische ID sein)
            this.tournamentId = urlParams.tournamentId;
            this.matchId = urlParams.matchId; // UUID oder numerische ID
            this.matchUniqueId = null; // Wird später gesetzt wenn UUID verfügbar
            this.matchNumericId = null; // Wird später gesetzt
            
            console.log(`🎯 [MATCH-CORE] Tournament: ${this.tournamentId}, Match: ${this.matchId}`);

            // Update page title
            document.title = `Match ${this.matchId} - Tournament Hub`;

            // Validate access first
            const hasAccess = await window.matchPageAPI.validateMatchAccess(
                this.tournamentId, 
                this.matchId
            );

            if (!hasAccess) {
                throw new Error('Zugriff auf dieses Match nicht möglich');
            }

            console.log('✅ [MATCH-CORE] Match access validated');

            // Load match data first to get complete match information including UUID
            await this.loadMatchData();
            
            // Load tournament info
            await this.loadTournamentInfo();
            
            // Load game rules
            await this.loadGameRules();
            
            // Initialize Socket.IO connection
            if (typeof window.matchPageWebSocket !== 'undefined') {
                await window.matchPageWebSocket.initializeConnection(
                    this.tournamentId,
                    this.getPreferredMatchId(), // Verwende UUID wenn verfügbar
                    'match-page'
                );
            }
            
            // Initialize form handlers
            this.initializeFormHandlers();
            
            // Start periodic updates
            this.startPeriodicUpdates();
            
            console.log('🎉 [MATCH-CORE] Match page initialization complete!');
            this.showInfo('Match-Seite erfolgreich geladen', 'success');
            
        } catch (error) {
            console.error('🚫 [MATCH-CORE] Initialization error:', error);
            this.showError(`Initialisierungsfehler: ${error.message}`);
            throw error;
        }
    }

    /**
     * Get preferred match ID (UUID if available, otherwise numeric ID)
     */
    getPreferredMatchId() {
        return this.matchUniqueId || this.matchId;
    }

    /**
     * Get match identification info
     */
    getMatchIdentification() {
        return {
            requestedId: this.matchId,
            uniqueId: this.matchUniqueId,
            numericId: this.matchNumericId,
            preferredId: this.getPreferredMatchId()
        };
    }

    /**
     * Setup back button functionality
     */
    setupBackButton() {
        const backButton = document.getElementById('backButton');
        if (backButton && this.tournamentId) {
            backButton.href = `/tournament/${this.tournamentId}`;
        }
    }

    /**
     * Initialize Socket.IO connection
     */
    initializeSocket() {
        try {
            console.log('🔌 [MATCH-CORE] Connecting to Socket.IO server...');
            
            this.socket = io('/', {
                transports: ['websocket', 'polling'],
                timeout: 10000,
                forceNew: false
            });

            this.setupSocketHandlers();
        } catch (error) {
            console.error('🚫 [MATCH-CORE] Failed to initialize socket:', error);
            this.showConnectionError();
        }
    }

    /**
     * Setup socket event handlers
     */
    setupSocketHandlers() {
        this.socket.on('connect', () => {
            console.log('✅ [MATCH-CORE] Connected to Socket.IO server');
            this.isConnected = true;
            this.connectionRetries = 0;
            this.updateConnectionStatus(true);
            this.joinMatchRoom();
        });

        this.socket.on('disconnect', (reason) => {
            console.log(`❌ [MATCH-CORE] Disconnected from server: ${reason}`);
            this.isConnected = false;
            this.updateConnectionStatus(false);
        });

        this.socket.on('connect_error', (error) => {
            console.error('🚫 [MATCH-CORE] Connection error:', error);
            this.connectionRetries++;
            this.updateConnectionStatus(false);
            
            if (this.connectionRetries >= this.maxRetries) {
                this.showConnectionError();
            }
        });

        this.socket.on('reconnect', (attemptNumber) => {
            console.log(`🔄 [MATCH-CORE] Reconnected after ${attemptNumber} attempts`);
            this.joinMatchRoom();
        });

        // Match-specific events
        this.socket.on('match-data', (data) => {
            console.log('📥 [MATCH-CORE] Received match data:', data);
            this.handleMatchData(data);
        });

        this.socket.on('match-updated', (data) => {
            console.log('🔄 [MATCH-CORE] Match updated:', data);
            this.handleMatchUpdate(data);
        });

        this.socket.on('game-rules-updated', (data) => {
            console.log('📋 [MATCH-CORE] Game rules updated:', data);
            this.handleGameRulesUpdate(data);
        });

        this.socket.on('error', (error) => {
            console.error('❌ [MATCH-CORE] Socket error:', error);
            this.showError(`Socket-Fehler: ${error.message || error}`);
        });
    }

    /**
     * Join the specific match room
     */
    joinMatchRoom() {
        if (!this.socket || !this.isConnected) {
            console.warn('⚠️ [MATCH-CORE] Cannot join room - not connected');
            return;
        }

        const roomData = {
            tournamentId: this.tournamentId,
            matchId: this.matchId,
            type: 'match-page'
        };

        console.log('🚪 [MATCH-CORE] Joining match room:', roomData);
        this.socket.emit('join-match-room', roomData);
        
        // Request initial match data
        this.requestMatchData();
    }

    /**
     * Request match data from server
     */
    requestMatchData() {
        console.log('📡 [MATCH-CORE] Requesting match data...');
        this.socket.emit('get-match-data', {
            tournamentId: this.tournamentId,
            matchId: this.matchId
        });
    }

    /**
     * Handle received match data
     */
    handleMatchData(data) {
        try {
            if (data.success) {
                this.matchData = data.match;
                this.gameRules = data.gameRules;
                console.log('✅ [MATCH-CORE] Match data processed successfully');
                
                // Trigger display update
                if (window.matchPageDisplay) {
                    window.matchPageDisplay.updateDisplay(this.matchData, this.gameRules);
                }
                
                this.hideLoading();
            } else {
                console.error('❌ [MATCH-CORE] Failed to load match data:', data.message);
                this.showError(data.message || 'Fehler beim Laden der Match-Daten');
            }
        } catch (error) {
            console.error('🚫 [MATCH-CORE] Error processing match data:', error);
            this.showError('Fehler beim Verarbeiten der Match-Daten');
        }
    }

    /**
     * Handle match updates
     */
    handleMatchUpdate(data) {
        try {
            if (data.success && data.match) {
                console.log('🔄 [MATCH-CORE] Processing match update...');
                this.matchData = data.match;
                
                // Update display
                if (window.matchPageDisplay) {
                    window.matchPageDisplay.handleMatchUpdate(this.matchData);
                }
            }
        } catch (error) {
            console.error('🚫 [MATCH-CORE] Error processing match update:', error);
        }
    }

    /**
     * Handle game rules updates
     */
    handleGameRulesUpdate(data) {
        try {
            if (data.success && data.gameRules) {
                console.log('📋 [MATCH-CORE] Processing game rules update...');
                this.gameRules = data.gameRules;
                
                // Update display
                if (window.matchPageDisplay) {
                    window.matchPageDisplay.handleGameRulesUpdate(this.gameRules);
                }
            }
        } catch (error) {
            console.error('🚫 [MATCH-CORE] Error processing game rules update:', error);
        }
    }

    /**
     * Update connection status indicator
     */
    updateConnectionStatus(connected) {
        const indicator = document.getElementById('connectionIndicator');
        const text = document.getElementById('connectionText');
        
        if (indicator) {
            indicator.classList.toggle('connected', connected);
        }
        
        if (text) {
            text.textContent = connected ? 'Verbunden' : 'Getrennt';
        }
    }

    /**
     * Hide loading screen and show content
     */
    hideLoading() {
        const loadingContainer = document.getElementById('loadingContainer');
        const matchContainer = document.getElementById('matchContainer');
        
        if (loadingContainer) {
            loadingContainer.classList.add('hidden');
        }
        
        if (matchContainer) {
            matchContainer.classList.remove('hidden');
            matchContainer.classList.add('fade-in');
        }
    }

    /**
     * Show error message
     */
    showError(message) {
        const loadingContainer = document.getElementById('loadingContainer');
        if (loadingContainer) {
            loadingContainer.innerHTML = `
                <span class="icon">❌</span>
                <strong>Fehler</strong><br>
                ${message}
            `;
        }
        
        console.error('🚫 [MATCH-CORE] Error displayed:', message);
    }

    /**
     * Show connection error
     */
    showConnectionError() {
        this.showError('Verbindung zum Server fehlgeschlagen. Bitte Seite neu laden.');
    }

    /**
     * Submit match result
     */
    async submitMatchResult(resultData) {
        try {
            console.log('📤 [MATCH-CORE] Submitting match result...');
            console.log('📊 [MATCH-CORE] Result data:', resultData);
            
            // Add match identification information to result
            const enhancedResultData = {
                ...resultData,
                // Match identification
                matchIdentification: this.getMatchIdentification(),
                // Enhanced metadata
                submittedVia: 'Match-Page',
                submittedAt: new Date().toISOString(),
                matchType: this.matchData?.matchType || 'Unknown',
                bracketType: this.matchData?.bracketType || null,
                // Class information
                classId: this.matchData?.classId || 1,
                className: this.matchData?.className || 'Unknown Class'
            };

            console.log('🔍 [MATCH-CORE] Enhanced result data:', enhancedResultData);
            
            // Use preferred match ID (UUID if available)
            const submitMatchId = this.getPreferredMatchId();
            console.log(`📤 [MATCH-CORE] Submitting to match ID: ${submitMatchId} (preferred)`);

            const response = await window.matchPageAPI.submitMatchResult(
                this.tournamentId,
                submitMatchId,
                enhancedResultData
            );

            if (!response.success) {
                throw new Error(response.message || 'Fehler beim Übertragen des Ergebnisses');
            }

            console.log('✅ [MATCH-CORE] Match result submitted successfully');
            console.log('📋 [MATCH-CORE] Server response:', response);

            // Log match identification confirmation from server
            if (response.data) {
                console.log('🔍 [MATCH-CORE] Server confirmed match identification:');
                console.log(`   UUID: ${response.data.uniqueId || 'none'}`);
                console.log(`   Numeric ID: ${response.data.numericMatchId || 'none'}`);
                console.log(`   Submitted to: ${response.data.matchId}`);
            }

            // Show success message
            this.showInfo('Match-Ergebnis erfolgreich übertragen!', 'success');
            
            // Trigger reload of match data to show updated state
            setTimeout(() => {
                this.loadMatchData();
            }, 1000);

            return response;

        } catch (error) {
            console.error('🚫 [MATCH-CORE] Error submitting match result:', error);
            this.showError(`Fehler beim Übertragen des Ergebnisses: ${error.message}`);
            throw error;
        }
    }

    /**
     * Get current match data
     */
    getMatchData() {
        return this.matchData;
    }

    /**
     * Get current game rules
     */
    getGameRules() {
        return this.gameRules;
    }

    /**
     * Check if socket is connected
     */
    isSocketConnected() {
        return this.isConnected && this.socket && this.socket.connected;
    }

    /**
     * Cleanup resources
     */
    cleanup() {
        if (this.socket) {
            this.socket.disconnect();
            this.socket = null;
        }
        console.log('🧹 [MATCH-CORE] Resources cleaned up');
    }

    /**
     * Load match data from API
     */
    async loadMatchData() {
        try {
            console.log('📡 [MATCH-CORE] Loading match data...');
            
            const response = await window.matchPageAPI.getMatchData(
                this.tournamentId,
                this.matchId
            );

            if (!response.success) {
                throw new Error(response.message || 'Fehler beim Laden der Match-Daten');
            }

            // Store match data including UUID information
            this.matchData = response.match;
            this.gameRules = response.gameRules;

            // ERWEITERT: Extrahiere UUID-Informationen aus der Antwort
            if (response.match) {
                this.matchUniqueId = response.match.uniqueId || null;
                this.matchNumericId = response.match.matchId || response.match.id || null;
                
                console.log('🔍 [MATCH-CORE] Match identification extracted:');
                console.log(`   Requested ID: ${this.matchId}`);
                console.log(`   UUID: ${this.matchUniqueId || 'none'}`);
                console.log(`   Numeric ID: ${this.matchNumericId || 'none'}`);
                console.log(`   Match Type: ${response.match.matchType || 'Unknown'}`);
                console.log(`   Bracket Type: ${response.match.bracketType || 'none'}`);
            }

            // Extract meta information for debugging
            if (response.meta && response.meta.matchIdentification) {
                const metaId = response.meta.matchIdentification;
                console.log('🔍 [MATCH-CORE] API Meta identification:', metaId);
            }

            console.log('✅ [MATCH-CORE] Match data loaded successfully');
            console.log('🎮 [MATCH-CORE] Match details:', {
                id: this.getPreferredMatchId(),
                player1: this.matchData.player1,
                player2: this.matchData.player2,
                status: this.matchData.status,
                className: this.matchData.className || 'Unknown'
            });

            // Update display
            if (typeof window.matchPageDisplay !== 'undefined') {
                window.matchPageDisplay.updateMatchDisplay(this.matchData);
            }

            return this.matchData;

        } catch (error) {
            console.error('🚫 [MATCH-CORE] Error loading match data:', error);
            this.showError(`Fehler beim Laden der Match-Daten: ${error.message}`);
            throw error;
        }
    }
}

// Create global instance
window.matchPageCore = new MatchPageCore();

console.log('📦 [MATCH-CORE] Match Page Core module loaded');

/**
 * Global function to open match page
 * Called from tournament interface match cards
 */
function openMatchPage(matchId) {
    try {
        console.log(`📄 [GLOBAL] Opening match page for match ${matchId}`);
        
        // Get tournament ID from current URL or global variable
        const pathParts = window.location.pathname.split('/');
        let tournamentId = null;
        
        if (pathParts.length >= 3 && pathParts[1] === 'tournament') {
            tournamentId = pathParts[2];
        } else if (window.currentTournamentId) {
            tournamentId = window.currentTournamentId;
        }
        
        if (!tournamentId) {
            console.error('❌ [GLOBAL] Cannot open match page - tournament ID not found');
            alert('Fehler: Turnier-ID nicht gefunden. Bitte Seite neu laden.');
            return;
        }
        
        if (!matchId) {
            console.error('❌ [GLOBAL] Cannot open match page - match ID is missing');
            alert('Fehler: Match-ID fehlt.');
            return;
        }
        
        // Construct match page URL
        const matchPageUrl = `/match/${tournamentId}/${matchId}`;
        
        // Ask user if they want to open in new tab or same tab
        const openInNewTab = confirm(
            'Match-Seite öffnen:\n\n' +
            '✅ OK = In neuem Tab öffnen\n' +
            '❌ Abbrechen = In aktuellem Tab öffnen'
        );
        
        if (openInNewTab) {
            // Open in new tab
            const newWindow = window.open(matchPageUrl, '_blank');
            if (newWindow) {
                console.log(`🔗 [GLOBAL] Opened match page in new tab: ${matchPageUrl}`);
            } else {
                // Fallback if popup blocked
                console.warn('⚠️ [GLOBAL] Popup blocked, using current tab');
                window.location.href = matchPageUrl;
            }
        } else {
            // Open in current tab
            console.log(`🔗 [GLOBAL] Opening match page in current tab: ${matchPageUrl}`);
            window.location.href = matchPageUrl;
        }
        
    } catch (error) {
        console.error('❌ [GLOBAL] Error opening match page:', error);
        alert(`Fehler beim Öffnen der Match-Seite: ${error.message}`);
    }
}