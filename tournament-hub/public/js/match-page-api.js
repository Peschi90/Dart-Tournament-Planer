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
     * Get match data from API
     */
    async getMatchData(tournamentId, matchId) {
        try {
            console.log(`📡 [MATCH-API] Fetching match data: ${tournamentId}/${matchId}`);
            
            const response = await this.makeRequest(
                `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}`,
                'GET'
            );
            
            if (response.success) {
                console.log('✅ [MATCH-API] Match data fetched successfully');
                return response;
            } else {
                throw new Error(response.message || 'Failed to fetch match data');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error fetching match data:', error);
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
            
            const endpoint = matchId ? 
                `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}/rules` :
                `${this.apiBaseUrl}/tournaments/${tournamentId}/rules`;
            
            const response = await this.makeRequest(endpoint, 'GET');
            
            if (response.success) {
                console.log('✅ [MATCH-API] Game rules fetched successfully');
                return response;
            } else {
                throw new Error(response.message || 'Failed to fetch game rules');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error fetching game rules:', error);
            throw error;
        }
    }

    /**
     * Submit match result via API
     */
    async submitMatchResult(tournamentId, matchId, resultData) {
        try {
            console.log(`📤 [MATCH-API] Submitting match result: ${tournamentId}/${matchId}`);
            
            const response = await this.makeRequest(
                `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}/result`,
                'POST',
                resultData
            );
            
            if (response.success) {
                console.log('✅ [MATCH-API] Match result submitted successfully');
                return response;
            } else {
                throw new Error(response.message || 'Failed to submit match result');
            }
        } catch (error) {
            console.error('🚫 [MATCH-API] Error submitting match result:', error);
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
            console.log(`🔐 [MATCH-API] Validating match access: ${tournamentId}/${matchId}`);
            
            const response = await this.makeRequest(
                `${this.apiBaseUrl}/tournaments/${tournamentId}/matches/${matchId}/access`,
                'GET'
            );
            
            return response.success === true;
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