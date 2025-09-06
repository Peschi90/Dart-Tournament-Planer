/**
 * Match Page API Module
 * Handles API communication for match pages
 */
class MatchPageAPI {
    constructor() {
        this.apiBaseUrl = '/api';
        this.requestTimeout = 10000; // 10 seconds

        console.log('🌐 [MATCH-API] Match Page API initialized');
    }

    /**
     * ✅ NEUE VEREINFACHTE API: Get match data with only match ID
     */
    async getMatchDataSimplified(matchId) {
        try {
            console.log(`🚀 [MATCH-API] Fetching match data via simplified API: ${matchId}`);
            console.log('🔍 [MATCH-API] Match ID type:', typeof matchId, 'value:', matchId);

            const response = await this.makeRequest(
                `${this.apiBaseUrl}/match/${matchId}`,
                'GET'
            );

            if (response.success) {
                console.log('✅ [MATCH-API] Match data fetched successfully via simplified API');

                // Log simplified API response details
                if (response.match) {
                    console.log('🔍 [MATCH-API] Simplified API match identification:');
                    console.log('   Primary ID (returned as id):', response.match.id);
                    console.log('   UUID:', response.match.uniqueId || 'none');
                    console.log('   Numeric ID:', response.match.matchId || 'none');
                    console.log('   Tournament ID:', response.match.tournamentId);

                    if (response.meta && response.meta.matchIdentification) {
                        console.log('🔍 [MATCH-API] Simplified API meta identification:');
                        console.log('   Requested ID:', response.meta.matchIdentification.requestedId);
                        console.log('   Found UUID:', response.meta.matchIdentification.uniqueId || 'none');
                        console.log('   Found Numeric:', response.meta.matchIdentification.numericId || 'none');
                        console.log('   Access Method:', response.meta.matchIdentification.accessMethod);
                        console.log('   Tournament Found:', response.meta.matchIdentification.tournamentFound);
                        console.log('   API Version:', response.meta.simplified ? 'simplified' : 'legacy');
                    }
                }

                return response;
            } else {
                throw new Error(response.message || 'Failed to fetch match data via simplified API');
            }
        } catch (error) {
            console.error('❌ [MATCH-API] Error fetching match data via simplified API:', error);
            throw error;
        }
    }

    /**
     * 🔄 LEGACY API: Get match data from API (backward compatible)
     */
    async getMatchData(tournamentId, matchId) {
        try {
            console.log(`📡 [MATCH-API] Fetching match data with UUID support: ${tournamentId}/${matchId}`);
            console.log('🔍 [MATCH-API] Match ID type:', typeof matchId, 'value:', matchId);

            const response = await this.makeRequest(
                `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}`,
                'GET'
            );

            if (response.success) {
                console.log('✅ [MATCH-API] Match data fetched successfully');

                // 🎯 ERWEITERT: Log UUID information from server response
                if (response.match) {
                    console.log('🔍 [MATCH-API] Server match identification:');
                    console.log('   Primary ID (returned as id):', response.match.id);
                    console.log('   UUID:', response.match.uniqueId || 'none');
                    console.log('   Numeric ID:', response.match.matchId || 'none');
                    console.log('   Hub Identifier:', response.match.hubIdentifier || 'none');

                    // Validate match identification
                    if (response.meta && response.meta.matchIdentification) {
                        console.log('🔍 [MATCH-API] Server meta identification:');
                        console.log('   Requested ID:', response.meta.matchIdentification.requestedId);
                        console.log('   Found UUID:', response.meta.matchIdentification.uniqueId || 'none');
                        console.log('   Found Numeric:', response.meta.matchIdentification.numericId || 'none');
                        console.log('   Access Method:', response.meta.matchIdentification.accessMethod);
                    }
                }

                return response;
            } else {
                throw new Error(response.message || 'Failed to fetch match data');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error fetching match data:', error);

            // 🎯 ERWEITERT: Enhanced error handling for UUID-related issues
            if (error.message.includes('404') || error.message.includes('not found')) {
                console.error('❌ [MATCH-API] Match not found - possible ID format issue:');
                console.error('   Requested Match ID:', matchId);
                console.error('   ID Type:', typeof matchId);
                console.error('   Tournament ID:', tournamentId);
            }

            throw error;
        }
    }

    /**
     * Get tournament information
     */
    async getTournamentInfo(tournamentId) {
        try {
            console.log(`📡 [MATCH-API] Fetching tournament info: ${tournamentId}`);

            const response = await this.makeRequest(
                `${this.apiBaseUrl}/tournaments/${tournamentId}`,
                'GET'
            );

            if (response.success) {
                console.log('✅ [MATCH-API] Tournament info fetched successfully');
                return response;
            } else {
                throw new Error(response.message || 'Failed to fetch tournament info');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error fetching tournament info:', error);
            throw error;
        }
    }

    /**
     * Get game rules for the match
     */
    async getGameRules(tournamentId, matchId = null) {
        try {
            console.log(`📡 [MATCH-API] Fetching game rules: ${tournamentId}${matchId ? '/' + matchId : ''}`);

            // ✅ KORRIGIERT: Verwende Tournament-Level Game Rules Route (nicht Match-spezifisch)
            // Die Match-spezifische Route existiert nicht, daher verwenden wir Tournament-Level Rules
            const endpoint = `${this.apiBaseUrl}/tournaments/${tournamentId}/gamerules`;

            const response = await this.makeRequest(endpoint, 'GET');

            if (response.success) {
                console.log('✅ [MATCH-API] Game rules fetched successfully');
                return response;
            } else {
                throw new Error(response.message || 'Failed to fetch game rules');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error fetching game rules:', error);

            // ✅ ERWEITERT: Graceful fallback for missing game rules
            if (error.message.includes('404') || error.message.includes('not found')) {
                console.log('ℹ️ [MATCH-API] Game rules not found at tournament level, using fallback');
                return {
                    success: true,
                    data: null, // Signal that no specific rules were found
                    message: 'No tournament-specific game rules found'
                };
            }

            throw error;
        }
    }

    /**
     * Submit match result via API
     */
    async submitMatchResult(tournamentId, matchId, resultData) {
        try {
            console.log(`📤 [MATCH-API] Submitting match result: ${tournamentId ? tournamentId + '/' : ''}${matchId}`);
            console.log('🔍 [MATCH-API] Original match ID type:', typeof matchId, 'value:', matchId);

            // 🎯 ERWEITERT: Log UUID system information
            if (resultData.uuidSystem) {
                console.log('🆔 [MATCH-API] UUID System Info:');
                console.log('   Version:', resultData.uuidSystem.version);
                console.log('   Submission Method:', resultData.uuidSystem.submissionMethod);
                console.log('   Preferred ID:', resultData.uuidSystem.preferredId);
                console.log('   All Known IDs:', resultData.uuidSystem.allKnownIds);
            }

            // 🎯 WICHTIG: Enhanced result data with all identification methods
            const enhancedResultData = {
                ...resultData,
                // Server-side match identification help
                matchIdentificationContext: {
                    originalRequestId: matchId,
                    uuidProvided: resultData.uniqueId ? true : false,
                    numericIdProvided: resultData.matchIdentification && resultData.matchIdentification.numericId ? true : false,
                    submissionTimestamp: new Date().toISOString(),
                    userAgent: navigator.userAgent,
                    source: 'match-page-web-interface'
                }
            };

            let response;

            // Versuche die vereinfachte API, wenn nur UUID vorhanden ist
            if (!tournamentId || tournamentId === 'uuid-only') {
                console.log('🎯 [MATCH-API] Using simplified API for result submission');
                try {
                    response = await this.makeRequest(
                        `${this.apiBaseUrl}/match/${matchId}/result`,
                        'POST',
                        enhancedResultData
                    );
                } catch (error) {
                    console.log('⚠️ [MATCH-API] Simplified API failed, falling back to legacy API');
                    throw error; // Rethrow to trigger fallback
                }
            } else {
                // Legacy API call
                response = await this.makeRequest(
                    `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}/result`,
                    'POST',
                    enhancedResultData
                );
            }

            if (response.success) {
                console.log('✅ [MATCH-API] Match result submitted successfully');

                // 🎯 ERWEITERT: Log server's match identification response
                if (response.data) {
                    console.log('🔍 [MATCH-API] Server identification response:');
                    console.log('   Match ID used:', response.data.matchId || 'not provided');
                    console.log('   UUID confirmed:', response.data.uniqueId || 'not provided');
                    console.log('   Numeric ID confirmed:', response.data.numericMatchId || 'not provided');
                    console.log('   Hub Identifier:', response.data.hubIdentifier || 'not provided');
                    console.log('   Access Method:', response.data.accessMethod || 'unknown');

                    // Validate that server found the correct match
                    if (resultData.uniqueId && response.data.uniqueId &&
                        resultData.uniqueId !== response.data.uniqueId) {
                        console.warn('⚠️ [MATCH-API] UUID mismatch between request and response!');
                        console.warn('   Sent UUID:', resultData.uniqueId);
                        console.warn('   Server UUID:', response.data.uniqueId);
                    }
                }

                return response;
            } else {
                throw new Error(response.message || 'Failed to submit match result');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error submitting match result:', error);

            // 🎯 ERWEITERT: Enhanced error logging for UUID-related issues
            if (error.message.includes('not found')) {
                console.error('❌ [MATCH-API] Match not found - possible UUID/ID mismatch:');
                console.error('   Submitted Match ID:', matchId);
                console.error('   UUID in data:', resultData.uniqueId || 'none');
                console.error('   Numeric ID in data:', resultData.matchIdentification && resultData.matchIdentification.numericId || 'none');
            }

            throw error;
        }
    }

    /**
     * Get match statistics
     * Future implementation for detailed match stats
     */
    async getMatchStatistics(tournamentId, matchId) {
        try {
            console.log(`📊 [MATCH-API] Fetching match statistics: ${tournamentId}/${matchId}`);

            const response = await this.makeRequest(
                `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}/statistics`,
                'GET'
            );

            if (response.success) {
                console.log('✅ [MATCH-API] Match statistics fetched successfully');
                return response;
            } else {
                // This is expected if statistics endpoint is not yet implemented
                console.log('ℹ️ [MATCH-API] Match statistics not available yet');
                return {
                    success: false,
                    message: 'Match statistics not yet implemented',
                    data: null
                };
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error fetching match statistics:', error);
            // Don't throw error for non-critical statistics
            return {
                success: false,
                message: error.message,
                data: null
            };
        }
    }

    /**
     * Submit dart throw data (future implementation)
     */
    async submitDartThrow(tournamentId, matchId, throwData) {
        try {
            console.log(`🎯 [MATCH-API] Submitting dart throw: ${tournamentId}/${matchId}`);

            const response = await this.makeRequest(
                `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}/throws`,
                'POST',
                throwData
            );

            if (response.success) {
                console.log('✅ [MATCH-API] Dart throw submitted successfully');
                return response;
            } else {
                throw new Error(response.message || 'Failed to submit dart throw');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error submitting dart throw:', error);
            // For now, return a future implementation message
            return {
                success: false,
                message: 'Dart throw submission will be available in a future version'
            };
        }
    }

    /**
     * Get live match updates
     */
    async getMatchUpdates(tournamentId, matchId, lastUpdateTimestamp = null) {
        try {
            console.log(`🔄 [MATCH-API] Fetching match updates: ${tournamentId}/${matchId}`);

            let endpoint = `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}/updates`;
            if (lastUpdateTimestamp) {
                endpoint += `?since=${lastUpdateTimestamp}`;
            }

            const response = await this.makeRequest(endpoint, 'GET');

            if (response.success) {
                console.log('✅ [MATCH-API] Match updates fetched successfully');
                return response;
            } else {
                throw new Error(response.message || 'Failed to fetch match updates');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error fetching match updates:', error);
            // Return empty updates for graceful handling
            return {
                success: true,
                data: {
                    updates: [],
                    lastUpdate: null
                }
            };
        }
    }

    /**
     * Validate match access
     */
    async validateMatchAccess(tournamentId, matchId) {
        try {
            console.log(`🔐 [MATCH-API] Validating match access with UUID support: ${tournamentId}/${matchId}`);

            const response = await this.makeRequest(
                `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}/access`,
                'GET'
            );

            if (response.success && response.hasAccess) {
                console.log('✅ [MATCH-API] Match access validated');

                // 🎯 ERWEITERT: Log match identification from access validation
                if (response.match) {
                    console.log('🔍 [MATCH-API] Access validation match info:');
                    console.log('   Validated ID:', response.match.id);
                    console.log('   UUID:', response.match.uniqueId || 'none');
                    console.log('   Numeric ID:', response.match.numericId || 'none');
                    console.log('   Access Method:', response.match.accessedVia);
                }

                return true;
            }

            return false;
        } catch (error) {
            console.error('🚫 [MATCH-API] Error validating match access:', error);
            return false;
        }
    }

    /**
     * Make HTTP request with timeout and error handling
     */
    async makeRequest(url, method = 'GET', data = null) {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), this.requestTimeout);

        try {
            const options = {
                method,
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                signal: controller.signal
            };

            if (data && method !== 'GET') {
                options.body = JSON.stringify(data);
            }

            console.log(`🌐 [MATCH-API] Making ${method} request to: ${url}`);

            const response = await fetch(url, options);
            clearTimeout(timeoutId);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const result = await response.json();
            return result;

        } catch (error) {
            clearTimeout(timeoutId);

            if (error.name === 'AbortError') {
                throw new Error('Request timeout');
            }

            console.error(`🚫 [MATCH-API] Request failed (${method} ${url}):`, error);
            throw error;
        }
    }

    /**
     * Health check for API connectivity
     */
    async healthCheck() {
        try {
            console.log('🏥 [MATCH-API] Performing health check...');

            const response = await this.makeRequest(`${this.apiBaseUrl}/health`, 'GET');

            if (response.success || response.status === 'healthy') {
                console.log('✅ [MATCH-API] API health check passed');
                return true;
            } else {
                console.warn('⚠️ [MATCH-API] API health check failed:', response);
                return false;
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] API health check error:', error);
            return false;
        }
    }

    /**
     * Get API status information
     */
    async getAPIStatus() {
        try {
            const response = await this.makeRequest(`${this.apiBaseUrl}/status`, 'GET');
            return response;
        } catch (error) {
            return {
                success: false,
                message: 'API not available',
                error: error.message
            };
        }
    }

    /**
     * Test socket connectivity
     */
    testSocketConnection() {
        return new Promise((resolve) => {
            try {
                const testSocket = io('/', {
                    transports: ['websocket', 'polling'],
                    timeout: 5000,
                    forceNew: true
                });

                testSocket.on('connect', () => {
                    console.log('✅ [MATCH-API] Socket connection test passed');
                    testSocket.disconnect();
                    resolve(true);
                });

                testSocket.on('connect_error', (error) => {
                    console.error('🚫 [MATCH-API] Socket connection test failed:', error);
                    testSocket.disconnect();
                    resolve(false);
                });

                setTimeout(() => {
                    if (testSocket.connected) {
                        testSocket.disconnect();
                    }
                    resolve(false);
                }, 5000);
            } catch (error) {
                console.error('🚫 [MATCH-API] Socket test error:', error);
                resolve(false);
            }
        });
    }

    /**
     * Format API error for user display
     */
    formatErrorMessage(error) {
        if (error.message) {
            if (error.message.includes('timeout')) {
                return 'Zeitüberschreitung - Bitte versuchen Sie es erneut';
            } else if (error.message.includes('Network')) {
                return 'Netzwerkfehler - Bitte Verbindung prüfen';
            } else if (error.message.includes('404')) {
                return 'Match nicht gefunden';
            } else if (error.message.includes('403')) {
                return 'Zugriff verweigert';
            } else if (error.message.includes('500')) {
                return 'Server-Fehler - Bitte später versuchen';
            }
        }

        return error.message || 'Unbekannter API-Fehler';
    }

    /**
     * Retry failed request with exponential backoff
     */
    async retryRequest(requestFunction, maxRetries = 3, baseDelay = 1000) {
        let lastError;

        for (let attempt = 1; attempt <= maxRetries; attempt++) {
            try {
                console.log(`🔄 [MATCH-API] Attempt ${attempt}/${maxRetries}`);
                return await requestFunction();
            } catch (error) {
                lastError = error;

                if (attempt < maxRetries) {
                    const delay = baseDelay * Math.pow(2, attempt - 1);
                    console.log(`⏳ [MATCH-API] Retrying in ${delay}ms...`);
                    await new Promise(resolve => setTimeout(resolve, delay));
                }
            }
        }

        throw lastError;
    }
}

// Create global instance
window.matchPageAPI = new MatchPageAPI();

console.log('🌐 [MATCH-API] Match Page API module loaded');