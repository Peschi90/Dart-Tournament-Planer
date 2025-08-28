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
     * Extract URL parameters from query string or path with UUID support
     */
    extractUrlParameters() {
        const urlParams = new URLSearchParams(window.location.search);
        const pathParts = window.location.pathname.split('/');
        
        // 🔑 ERWEITERT: UUID-Unterstützung bei Parameter-Extraktion
        
        // Priorität 1: Query-Parameter (?tournament=ID&match=ID)
        const queryTournamentId = urlParams.get('tournament') || urlParams.get('tournamentId');
        const queryMatchId = urlParams.get('match') || urlParams.get('matchId');
        const uuidHint = urlParams.get('uuid') === 'true';
        
        if (queryTournamentId && queryMatchId) {
            console.log('🔍 [MATCH-CORE] Using query parameters');
            console.log('🆔 [MATCH-CORE] UUID hint from URL:', uuidHint);
            console.log('🔍 [MATCH-CORE] Match ID format:', typeof queryMatchId, 'length:', queryMatchId.length, 'contains hyphens:', queryMatchId.includes('-'));
            
            // Erkenne UUID-Format (typisch: 8-4-4-4-12 Zeichen mit Bindestrichen)
            const isLikelyUuid = queryMatchId.length >= 32 && queryMatchId.includes('-');
            
            console.log('🆔 [MATCH-CORE] Match ID analysis:', {
                value: queryMatchId,
                likelyUuid: isLikelyUuid,
                uuidHint: uuidHint,
                finalAssessment: isLikelyUuid || uuidHint
            });
            
            return {
                tournamentId: queryTournamentId,
                matchId: queryMatchId,
                isUuid: isLikelyUuid || uuidHint
            };
        }
        
        // Priorität 2: Path-Parameter (/match/tournamentId/matchId)
        if (pathParts.length >= 4 && pathParts[1] === 'match') {
            console.log('🔍 [MATCH-CORE] Using path parameters');
            const pathMatchId = pathParts[3];
            const isLikelyUuid = pathMatchId.length >= 32 && pathMatchId.includes('-');
            
            console.log('🆔 [MATCH-CORE] Path match ID analysis:', {
                value: pathMatchId,
                likelyUuid: isLikelyUuid
            });
            
            return {
                tournamentId: pathParts[2],
                matchId: pathMatchId,
                isUuid: isLikelyUuid
            };
        }
        
        // Priorität 3: Legacy path format (/tournament/tournamentId -> dann Query-Parameter)
        if (pathParts.length >= 3 && pathParts[1] === 'tournament') {
            const legacyTournamentId = pathParts[2];
            const legacyMatchId = urlParams.get('match') || urlParams.get('matchId');
            
            if (legacyTournamentId && legacyMatchId) {
                console.log('🔍 [MATCH-CORE] Using legacy format');
                const isLikelyUuid = legacyMatchId.length >= 32 && legacyMatchId.includes('-');
                
                return {
                    tournamentId: legacyTournamentId,
                    matchId: legacyMatchId,
                    isUuid: isLikelyUuid || uuidHint
                };
            }
        }
        
        console.warn('⚠️ [MATCH-CORE] No valid URL parameters found');
        return {};
    }

    /**
     * Initialize the match page with tournament and match IDs with UUID support
     */
    async initialize() {
        try {
            console.log('🚀 [MATCH-CORE] Initializing match page with UUID support...');
            
            // Extract URL parameters (enhanced with UUID detection)
            const urlParams = this.extractUrlParameters();
            console.log('📋 [MATCH-CORE] URL Parameters:', urlParams);
            
            if (!urlParams.tournamentId || !urlParams.matchId) {
                throw new Error('Ungültige Match-URL: Tournament ID oder Match ID fehlen');
            }

            this.tournamentId = urlParams.tournamentId;
            this.matchId = urlParams.matchId;
            this.urlIndicatesUuid = urlParams.isUuid || false;

            console.log(`🎯 [MATCH-CORE] Tournament: ${this.tournamentId}, Match: ${this.matchId}`);
            console.log(`🆔 [MATCH-CORE] URL indicates UUID: ${this.urlIndicatesUuid}`);
            
            // Update page title
            document.title = `Match ${this.matchId} - Tournament Hub`;
            
            // Setup back button
            this.setupBackButton();
            
            // Initialize socket connection first (non-blocking)
            this.initializeSocket();

            // 🔑 ERWEITERT: Load match data first to get complete match information including UUID
            await this.loadMatchData();
            
            // Load tournament info
            await this.loadTournamentInfo();
            
            // Load game rules
            await this.loadGameRules();
            
            // Initialize form handlers
            this.initializeFormHandlers();
            
            // Start periodic updates
            this.startPeriodicUpdates();
            
            console.log('🎉 [MATCH-CORE] Match page initialization complete!');
            this.showInfo('Match-Seite erfolgreich geladen', 'success');
            
            // 🆔 ERWEITERT: Zeige UUID-Status in der UI
            if (this.matchUniqueId) {
                console.log('✅ [MATCH-CORE] Match page running with UUID support');
                this.showInfo(`✅ UUID-System aktiv (${this.matchUniqueId.substring(0, 8)}...)`, 'success');
            } else if (this.urlIndicatesUuid) {
                console.warn('⚠️ [MATCH-CORE] URL indicated UUID but none found in match data');
                this.showInfo('⚠️ UUID erwartet aber nicht gefunden', 'warning');
            }
            
            return true;
            
        } catch (error) {
            console.error('🚫 [MATCH-CORE] Initialization error:', error);
            this.showError(`Initialisierungsfehler: ${error.message}`);
            return false;
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
            
            // ✅ ERWEITERT: Add comprehensive match identification to result
            const enhancedResultData = {
                ...resultData,
                // ✅ KORRIGIERT: Setze explizit den Status auf "Finished"
                status: 'Finished',
                // 🔑 PRIMÄRE UUID-IDENTIFIKATION (preferred for all operations)
                uniqueId: this.matchUniqueId,                           // UUID (primary)
                matchIdentification: this.getMatchIdentification(),     // All ID types
                hubIdentifier: this.matchData?.hubIdentifier,           // Hub-specific ID
                
                // Enhanced metadata for Hub/Planner integration
                submittedVia: 'Match-Page-Web-Interface',
                submittedAt: new Date().toISOString(),
                matchType: this.matchData?.matchType || 'Unknown',
                bracketType: this.matchData?.bracketType || null,
                round: this.matchData?.round || null,
                position: this.matchData?.position || null,
                
                // Class information
                classId: this.matchData?.classId || 1,
                className: this.matchData?.className || 'Unknown Class',
                
                // Group information (for proper identification)
                groupId: this.matchData?.groupId || null,
                groupName: this.matchData?.groupName || null,
                
                // 🎯 UUID-System Metadata
                uuidSystem: {
                    enabled: true,
                    version: "2.0",
                    submissionMethod: this.matchUniqueId ? "uuid" : "numericId",
                    preferredId: this.getPreferredMatchId(),
                    allKnownIds: {
                        uuid: this.matchUniqueId || null,
                        numericId: this.matchNumericId || null,
                        requestedId: this.matchId,
                        hubIdentifier: this.matchData?.hubIdentifier || null
                    }
                }
            };

            console.log('🔍 [MATCH-CORE] Enhanced result data with UUID system:', enhancedResultData);
            
            // 🎯 WICHTIG: Use preferred match ID (UUID if available)
            const submitMatchId = this.getPreferredMatchId();
            console.log(`📤 [MATCH-CORE] Submitting to match ID: ${submitMatchId} (preferred method: ${enhancedResultData.uuidSystem.submissionMethod})`);

            // Submit using preferred identification method
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

            // 🎯 ERWEITERT: Log match identification confirmation from server
            if (response.data) {
                console.log('🔍 [MATCH-CORE] Server confirmed match identification:');
                console.log(`   UUID: ${response.data.uniqueId || 'none'}`);
                console.log(`   Numeric ID: ${response.data.numericMatchId || 'none'}`);
                console.log(`   Hub Identifier: ${response.data.hubIdentifier || 'none'}`);
                console.log(`   Submitted to: ${response.data.matchId}`);
                console.log(`   Access method: ${response.data.accessMethod || 'unknown'}`);
            }

            // Show enhanced success message
            this.showInfo(`Match-Ergebnis erfolgreich übertragen! (ID: ${submitMatchId})`, 'success');
            
            // ✅ KORRIGIERT: Aktualisiere Match Data sofort mit submitted Result
            console.log('🔄 [MATCH-CORE] Updating local match data with submitted result...');
            
            // Update local match data immediately for instant UI feedback
            if (this.matchData) {
                this.matchData.status = 'Finished';
                this.matchData.player1Sets = enhancedResultData.player1Sets || 0;
                this.matchData.player2Sets = enhancedResultData.player2Sets || 0;
                this.matchData.player1Legs = enhancedResultData.player1Legs || 0;
                this.matchData.player2Legs = enhancedResultData.player2Legs || 0;
                this.matchData.notes = enhancedResultData.notes || '';
                this.matchData.endTime = new Date().toISOString();
                this.matchData.lastUpdated = new Date().toISOString();
                
                // Update display immediately
                if (window.matchPageDisplay) {
                    console.log('🎨 [MATCH-CORE] Triggering immediate display update...');
                    window.matchPageDisplay.updateDisplay(this.matchData, this.gameRules);
                }
            }
            
            // Also reload from server for confirmation (async)
            setTimeout(async () => {
                try {
                    console.log('🔄 [MATCH-CORE] Reloading match data from server for confirmation...');
                    await this.loadMatchData();
                } catch (error) {
                    console.warn('⚠️ [MATCH-CORE] Failed to reload match data (non-fatal):', error);
                }
            }, 2000);

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
            console.log(`🔍 [MATCH-CORE] Tournament: ${this.tournamentId}, Match: ${this.matchId}`);
            
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
                
                // HINZUGEFÜGT: Logge welche ID verwendet wurde für die Anfrage
                if (this.matchId === this.matchUniqueId) {
                    console.log('✅ [MATCH-CORE] Match was accessed via UUID');
                } else if (this.matchId == this.matchNumericId) {
                    console.log('✅ [MATCH-CORE] Match was accessed via numeric ID');
                } else {
                    console.log('⚠️ [MATCH-CORE] Match ID format unknown or converted');
                }
            }

            // Extract meta information for debugging
            if (response.meta && response.meta.matchIdentification) {
                const metaId = response.meta.matchIdentification;
                console.log('🔍 [MATCH-CORE] API Meta identification:', metaId);
            }

            console.log('✅ [MATCH-CORE] Match data loaded successfully');
            console.log('🎮 [MATCH-CORE] Match details:', {
                preferredId: this.getPreferredMatchId(),
                player1: this.matchData.player1,
                player2: this.matchData.player2,
                status: this.matchData.status,
                className: this.matchData.className || 'Unknown',
                matchType: this.matchData.matchType || 'Unknown'
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

    /**
     * Load tournament information
     */
    async loadTournamentInfo() {
        try {
            console.log('🏆 [MATCH-CORE] Loading tournament information...');
            
            const response = await window.matchPageAPI.getTournamentInfo(this.tournamentId);
            
            if (response.success && response.data && response.data.tournament) {
                this.tournamentData = response.data.tournament;
                console.log('✅ [MATCH-CORE] Tournament info loaded:', this.tournamentData.name);
                
                // Update page title with tournament name
                if (this.tournamentData.name) {
                    document.title = `Match ${this.matchId} - ${this.tournamentData.name}`;
                }
                
                // Update display
                if (typeof window.matchPageDisplay !== 'undefined') {
                    window.matchPageDisplay.updateTournamentDisplay(this.tournamentData);
                }
            } else {
                console.warn('⚠️ [MATCH-CORE] Tournament info not available or invalid response');
            }
            
        } catch (error) {
            console.warn('⚠️ [MATCH-CORE] Error loading tournament info (non-fatal):', error);
        }
    }

    /**
     * Load game rules for this match
     */
    async loadGameRules() {
        try {
            console.log('🎮 [MATCH-CORE] Loading game rules...');
            
            // ✅ KORRIGIERT: Prüfe zuerst, ob Game Rules bereits aus Match-Data vorhanden sind
            if (this.gameRules) {
                console.log('✅ [MATCH-CORE] Game rules already loaded from match data:', this.gameRules.name || 'Default');
                
                // Update display mit bereits geladenen Rules
                if (typeof window.matchPageDisplay !== 'undefined') {
                    window.matchPageDisplay.updateGameRulesDisplay(this.gameRules);
                }
                return;
            }
            
            // Fallback: Versuche Game Rules von API zu laden (nur wenn nicht bereits vorhanden)
            console.log('🔍 [MATCH-CORE] Game rules not in match data, trying to load from tournament...');
            
            try {
                // Verwende Tournament-Level Game Rules als Fallback
                const response = await window.matchPageAPI.getGameRules(this.tournamentId, null);
                
                if (response.success && response.data) {
                    // Finde passende Game Rules für diesen Match
                    const tournamentGameRules = Array.isArray(response.data) ? response.data : [response.data];
                    
                    // Versuche Match-spezifische Rules zu finden
                    let matchGameRules = tournamentGameRules.find(gr => 
                        (gr.classId || gr.ClassId) === this.matchData?.classId &&
                        (gr.matchType || 'Group') === (this.matchData?.matchType || 'Group')
                    );
                    
                    // Fallback: Class-basierte Rules
                    if (!matchGameRules) {
                        matchGameRules = tournamentGameRules.find(gr => 
                            (gr.classId || gr.ClassId) === this.matchData?.classId
                        );
                    }
                    
                    // Final fallback: Erste verfügbare Rules
                    if (!matchGameRules && tournamentGameRules.length > 0) {
                        matchGameRules = tournamentGameRules[0];
                    }
                    
                    if (matchGameRules) {
                        this.gameRules = matchGameRules;
                        console.log('✅ [MATCH-CORE] Game rules loaded from tournament:', this.gameRules.name || 'Default');
                    }
                } else {
                    console.log('ℹ️ [MATCH-CORE] No tournament-level game rules found');
                }
            } catch (apiError) {
                console.warn('⚠️ [MATCH-CORE] Could not load tournament game rules:', apiError.message);
            }
            
            // Erstelle Default Game Rules wenn immer noch keine vorhanden
            if (!this.gameRules) {
                console.log('🔧 [MATCH-CORE] Creating default game rules');
                this.gameRules = this.createDefaultGameRules();
            }
            
            // Update display
            if (typeof window.matchPageDisplay !== 'undefined') {
                window.matchPageDisplay.updateGameRulesDisplay(this.gameRules);
            }
            
            console.log('✅ [MATCH-CORE] Game rules finalized:', {
                name: this.gameRules.name || 'Default',
                gameMode: this.gameRules.gameMode || 'Standard',
                gamePoints: this.gameRules.gamePoints || 501,
                finishMode: this.gameRules.finishMode || 'DoubleOut',
                playWithSets: this.gameRules.playWithSets || false
            });
            
        } catch (error) {
            console.warn('⚠️ [MATCH-CORE] Error loading game rules (non-fatal), using defaults:', error);
            
            // Erstelle Default Game Rules als finaler Fallback
            if (!this.gameRules) {
                this.gameRules = this.createDefaultGameRules();
                console.log('🔧 [MATCH-CORE] Using default game rules after error');
                
                // Update display mit Default Rules
                if (typeof window.matchPageDisplay !== 'undefined') {
                    window.matchPageDisplay.updateGameRulesDisplay(this.gameRules);
                }
            }
        }
    }

    /**
     * Create default game rules when none are available
     */
    createDefaultGameRules() {
        return {
            id: 'default',
            name: 'Standard Dart Regeln',
            gamePoints: 501,
            gameMode: 'Standard',
            finishMode: 'DoubleOut',
            playWithSets: true,
            setsToWin: 3,
            legsToWin: 3,
            legsPerSet: 5,
            maxThrowsPerLeg: null,
            checkoutMode: 'Any',
            allowBigFinish: true,
            description: 'Standard 501 Double-Out Regeln'
        };
    }

    /**
     * Initialize form handlers for result submission
     */
    initializeFormHandlers() {
        console.log('📝 [MATCH-CORE] Initializing form handlers...');
        
        // Match result form submission
        const resultForm = document.getElementById('matchResultForm');
        if (resultForm) {
            resultForm.addEventListener('submit', this.handleResultSubmission.bind(this));
        }
        
        // Real-time validation on input changes
        const inputs = resultForm?.querySelectorAll('input[type="number"]');
        inputs?.forEach(input => {
            input.addEventListener('input', this.validateForm.bind(this));
        });
        
        console.log('✅ [MATCH-CORE] Form handlers initialized');
    }

    /**
     * Handle result form submission
     */
    async handleResultSubmission(event) {
        event.preventDefault();
        
        try {
            console.log('📤 [MATCH-CORE] Handling result submission...');
            
            // ✅ KORRIGIERT: Verwende die korrekten Feldnamen für die API
            const formData = new FormData(event.target);
            const resultData = {
                // 🎯 KORRIGIERT: Verwende die gleichen Feldnamen wie Tournament-Interface
                player1Sets: parseInt(formData.get('sets1')) || 0,
                player2Sets: parseInt(formData.get('sets2')) || 0,
                player1Legs: parseInt(formData.get('legs1')) || 0,
                player2Legs: parseInt(formData.get('legs2')) || 0,
                notes: formData.get('notes') || '',
                status: 'Finished',
                // UUID-Informationen hinzufügen
                matchIdentification: this.getMatchIdentification(),
                matchType: this.matchData?.matchType || 'Unknown',
                className: this.matchData?.className || 'Unknown'
            };
            
            console.log('📊 [MATCH-CORE] Submitting result:', resultData);
            
            // Disable submit button
            const submitButton = event.target.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = true;
                submitButton.textContent = 'Übertrage...';
            }
            
            await this.submitMatchResult(resultData);
            
            // Show success message
            this.showInfo('Ergebnis erfolgreich übertragen!', 'success');
            
        } catch (error) {
            console.error('❌ [MATCH-CORE] Error submitting result:', error);
            this.showError(`Fehler: ${error.message}`);
        } finally {
            // Re-enable submit button
            const submitButton = document.querySelector('button[type="submit"]');
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.textContent = 'Ergebnis übertragen';
            }
        }
    }

    /**
     * Validate form inputs
     */
    validateForm() {
        // Basic validation logic
        console.log('🔍 [MATCH-CORE] Validating form...');
        // Implementation can be added based on game rules
    }

    /**
     * Start periodic updates to keep match data fresh
     */
    startPeriodicUpdates() {
        console.log('🔄 [MATCH-CORE] Starting periodic updates...');
        
        // Update every 30 seconds
        this.updateInterval = setInterval(() => {
            if (this.isSocketConnected()) {
                this.requestMatchData();
            } else {
                // Fallback: reload via API
                this.loadMatchData().catch(err => {
                    console.warn('⚠️ [MATCH-CORE] Periodic update failed:', err);
                });
            }
        }, 30000);
    }

    /**
     * Show info message
     */
    showInfo(message, type = 'info') {
        console.log(`ℹ️ [MATCH-CORE] Info: ${message}`);
        
        // Create or update info display
        let infoDisplay = document.getElementById('infoDisplay');
        if (!infoDisplay) {
            infoDisplay = document.createElement('div');
            infoDisplay.id = 'infoDisplay';
            infoDisplay.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9999;
                padding: 15px;
                border-radius: 8px;
                color: white;
                font-weight: bold;
                max-width: 300px;
                box-shadow: 0 4px 12px rgba(0,0,0,0.3);
            `;
            document.body.appendChild(infoDisplay);
        }
        
        // Set message and styling based on type
        infoDisplay.textContent = message;
        switch (type) {
            case 'success':
                infoDisplay.style.backgroundColor = '#28a745';
                break;
            case 'error':
                infoDisplay.style.backgroundColor = '#dc3545';
                break;
            case 'warning':
                infoDisplay.style.backgroundColor = '#ffc107';
                infoDisplay.style.color = '#000';
                break;
            default:
                infoDisplay.style.backgroundColor = '#17a2b8';
        }
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            if (infoDisplay && infoDisplay.parentNode) {
                infoDisplay.parentNode.removeChild(infoDisplay);
            }
        }, 5000);
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
        let tournamentId = window.currentTournamentId;
        
        // Fallback: Tournament ID aus URL extrahieren
        if (!tournamentId) {
            const urlParams = new URLSearchParams(window.location.search);
            tournamentId = urlParams.get('tournament') || urlParams.get('tournamentId');
        }
        
        // Weitere Fallback: Tournament ID aus URL-Path extrahieren
        if (!tournamentId) {
            const pathParts = window.location.pathname.split('/');
            if (pathParts.length >= 3 && pathParts[1] === 'tournament') {
                tournamentId = pathParts[2];
            }
        }
        
        // Fallback: Tournament ID aus aktuellem Tournament ermitteln
        if (!tournamentId && window.currentTournament) {
            tournamentId = window.currentTournament.id;
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
        
        // KORRIGIERT: Verwende Query-Parameter Format für match-page.html (nicht /match/ path)
        const matchPageUrl = `/match-page.html?tournament=${tournamentId}&match=${matchId}`;
        
        console.log(`🔗 [GLOBAL] Preparing to open: ${matchPageUrl}`);
        
        // Frage Benutzer nach Öffnungsmethode
        const openInNewTab = confirm(
            `Match-Seite für Match ${matchId} öffnen:\n\n` +
            '✅ OK = In neuem Tab öffnen\n' +
            '❌ Abbrechen = In aktuellem Tab öffnen'
        );
        
        if (openInNewTab) {
            // Open in new tab
            const newWindow = window.open(matchPageUrl, '_blank', 'width=1200,height=800');
            if (newWindow) {
                console.log(`🔗 [GLOBAL] Opened match page in new tab: ${matchPageUrl}`);
            } else {
                // Fallback if popup blocked
                console.warn('⚠️ [GLOBAL] Popup blocked, asking user for permission');
                if (confirm('Popup wurde blockiert. In aktuellem Tab öffnen?')) {
                    window.location.href = matchPageUrl;
                }
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