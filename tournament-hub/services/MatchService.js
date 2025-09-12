/**
 * Match Service
 * Handles match-related operations and result processing
 */
class MatchService {
    constructor(tournamentRegistry) {
        this.tournamentRegistry = tournamentRegistry;
        this.pendingResults = new Map(); // Store pending results
        this.processingQueue = [];
    }

    /**
     * Submit match result with WebSocket broadcasting
     * @param {string} tournamentId - Tournament ID
     * @param {string} matchId - Match ID (kann UUID oder numerische ID sein)
     * @param {Object} result - Match result data
     * @returns {Promise<boolean>} Success status
     */
    async submitMatchResult(tournamentId, matchId, result) {
        try {
            console.log(`?? [MatchService] ===== MATCH RESULT SUBMISSION =====`);
            console.log(`?? [MatchService] Tournament: ${tournamentId}`);
            console.log(`?? [MatchService] Match: ${matchId} (UUID or numeric ID)`);
            console.log(`?? [MatchService] Class Analysis:`);
            console.log(`   Result classId: ${result.classId}`);
            console.log(`   Result className: ${result.className}`);
            console.log(`   GameRules classId: ${result.gameRulesUsed?.classId}`);
            console.log(`   GameRules className: ${result.gameRulesUsed?.className}`);
            console.log(`?? [MatchService] Full result:`, JSON.stringify(result, null, 2));

            if (!tournamentId || !matchId || !result) {
                throw new Error('Tournament ID, Match ID, and result are required');
            }

            const tournament = this.tournamentRegistry.getTournament(tournamentId);
            if (!tournament) {
                throw new Error(`Tournament not found: ${tournamentId}`);
            }

            console.log(`?? [MatchService] Tournament found: ${tournament.name || tournament.id}`);
            console.log(`?? [MatchService] Current matches: ${tournament.matches ? tournament.matches.length : 0}`);

            // Validate result data with enhanced game rules validation
            const validationResult = this.validateMatchResultWithGameRules(result);
            if (!validationResult.valid) {
                console.error(`? [MatchService] Validation failed: ${validationResult.error}`);
                throw new Error(`Invalid match result: ${validationResult.error}`);
            }

            console.log(`? [MatchService] Match result validation passed`);

            // ERWEITERT: UUID-bewusste Match-Suche
            const matches = tournament.matches || [];
            const matchIndex = matches.findIndex(m =>
                // Priorisiere UniqueId (UUID)
                (m.uniqueId && m.uniqueId === matchId) ||
                // Fallback auf numerische IDs
                (m.id === matchId || m.matchId === matchId ||
                    String(m.id) === String(matchId) ||
                    String(m.matchId) === String(matchId))
            );

            if (matchIndex === -1) {
                console.error(`? [MatchService] Match not found in tournament: ${matchId}`);
                console.log(`?? [MatchService] Available matches:`, matches.map(m => ({
                    uniqueId: m.uniqueId || 'none',
                    id: m.id || m.matchId,
                    classId: m.classId,
                    className: m.className,
                    player1: m.player1,
                    player2: m.player2
                })));
                throw new Error(`Match not found: ${matchId}`);
            }

            const originalMatch = matches[matchIndex];
            console.log(`?? [MatchService] Found match:`, {
                uniqueId: originalMatch.uniqueId || 'none',
                id: originalMatch.id || originalMatch.matchId,
                classId: originalMatch.classId,
                className: originalMatch.className,
                player1: originalMatch.player1,
                player2: originalMatch.player2,
                matchType: originalMatch.matchType || 'Unknown'
            });

            // KORRIGIERT: Preserve original class information and enhance with result data
            const finalClassId = result.classId || originalMatch.classId || originalMatch.ClassId || 1;
            const finalClassName = result.className || originalMatch.className || originalMatch.ClassName || 'Unbekannte Klasse';

            console.log(`?? [MatchService] Class information resolution:`);
            console.log(`   Original match classId: ${originalMatch.classId}`);
            console.log(`   Original match className: ${originalMatch.className}`);
            console.log(`   Result classId: ${result.classId}`);
            console.log(`   Result className: ${result.className}`);
            console.log(`   FINAL classId: ${finalClassId}`);
            console.log(`   FINAL className: ${finalClassName}`);

            // Update the match with result including preserved class information and UUID
            const updatedMatch = {
                ...originalMatch,
                // Preserve UUID if available
                uniqueId: originalMatch.uniqueId,
                // Results
                player1Sets: parseInt(result.player1Sets) || 0,
                player1Legs: parseInt(result.player1Legs) || 0,
                player2Sets: parseInt(result.player2Sets) || 0,
                player2Legs: parseInt(result.player2Legs) || 0,
                status: result.status || 'Finished',
                notes: result.notes || '',
                endTime: new Date().toISOString(),
                lastUpdated: new Date().toISOString(),
                submittedBy: 'Hub Interface',
                // KORRIGIERT: Preserve class information
                classId: finalClassId,
                className: finalClassName,
                // Enhanced metadata with UUID support
                submissionSource: result.submissionSource || 'unknown',
                submissionTimestamp: result.submissionTimestamp || new Date().toISOString(),
                matchIdentification: {
                    uniqueId: originalMatch.uniqueId,
                    numericId: originalMatch.id || originalMatch.matchId,
                    requestedId: matchId,
                    matchType: originalMatch.matchType || 'Unknown'
                },
                gameRulesUsed: {
                    ...result.gameRulesUsed,
                    classId: finalClassId,
                    className: finalClassName
                }
            };

            // Replace the match in the array
            matches[matchIndex] = updatedMatch;
            tournament.matches = matches;
            tournament.lastUpdate = new Date();

            console.log(`? [MatchService] Match updated successfully:`);
            console.log(`   UUID: ${updatedMatch.uniqueId || 'none'}`);
            console.log(`   Numeric ID: ${updatedMatch.id || updatedMatch.matchId}`);
            console.log(`   Sets: ${updatedMatch.player1Sets}-${updatedMatch.player2Sets}`);
            console.log(`   Legs: ${updatedMatch.player1Legs}-${updatedMatch.player2Legs}`);
            console.log(`   Status: ${updatedMatch.status}`);
            console.log(`   Class: ${updatedMatch.className} (ID: ${updatedMatch.classId})`);
            console.log(`   Game Rules: ${updatedMatch.gameRulesUsed ? updatedMatch.gameRulesUsed.name || 'Default' : 'None'}`);

            // Process the result for queue handling (Tournament Planner API forwarding)
            // Verwende UUID wenn verf�gbar, sonst numerische ID
            const forwardMatchId = originalMatch.uniqueId || matchId;
            const processedResult = {
                matchId: forwardMatchId,
                uniqueId: originalMatch.uniqueId,
                numericMatchId: originalMatch.id || originalMatch.matchId,
                tournamentId: tournamentId,
                ...result,
                // KORRIGIERT: Ensure class information is preserved
                classId: finalClassId,
                className: finalClassName,
                submittedAt: new Date(),
                processed: false
            };

            // Store pending result with enhanced key
            const resultKey = originalMatch.uniqueId ?
                `${tournamentId}_${originalMatch.uniqueId}` :
                `${tournamentId}_${matchId}`;
            this.pendingResults.set(resultKey, processedResult);

            // Add to processing queue for Tournament Planner API
            this.processingQueue.push({
                tournamentId,
                matchId: forwardMatchId,
                uniqueId: originalMatch.uniqueId,
                numericMatchId: originalMatch.id || originalMatch.matchId,
                result: processedResult,
                attempts: 0,
                maxAttempts: 3
            });

            // Process queue (forward to Tournament Planner API)
            try {
                await this.processResultQueue();
                console.log(`?? [MatchService] Result queue processed successfully`);
            } catch (queueError) {
                console.error(`?? [MatchService] Queue processing error (non-fatal):`, queueError.message);
                // Don't fail the main operation due to queue errors
            }

            console.log(`?? [MatchService] Match result submitted successfully for ${finalClassName} (Class ID: ${finalClassId})`);
            console.log(`?? [MatchService] ===== MATCH RESULT SUBMISSION COMPLETE =====`);

            // Return success - WebSocket broadcasting is handled by server.js
            return true;

        } catch (error) {
            console.error('? [MatchService] submitMatchResult error:', error.message);
            console.error('Stack trace:', error.stack);
            throw error; // Re-throw to let the caller handle it
        }
    }

    /**
     * Get tournament matches
     * @param {string} tournamentId - Tournament ID
     * @returns {Promise<Array>} Array of matches
     */
    async getTournamentMatches(tournamentId) {
        try {
            if (!tournamentId) {
                throw new Error('Tournament ID is required');
            }

            const tournament = this.tournamentRegistry.getTournament(tournamentId);
            if (!tournament) {
                console.warn(`? Tournament not found: ${tournamentId}`);
                return [];
            }

            const matches = tournament.matches || [];
            console.log(`?? Retrieved ${matches.length} matches for tournament: ${tournamentId}`);

            return matches;
        } catch (error) {
            console.error('? Get matches error:', error.message);
            return [];
        }
    }

    /**
     * Get specific match
     * @param {string} tournamentId - Tournament ID
     * @param {string} matchId - Match ID (kann UUID oder numerische ID sein)
     * @returns {Promise<Object|null>} Match data or null
     */
    async getMatch(tournamentId, matchId) {
        try {
            if (!tournamentId || !matchId) {
                throw new Error('Tournament ID and Match ID are required');
            }

            const matches = await this.getTournamentMatches(tournamentId);

            // ERWEITERT: UUID-bewusste Match-Suche
            const match = matches.find(m =>
                // Priorisiere UniqueId (UUID)
                (m.uniqueId && m.uniqueId === matchId) ||
                // Fallback auf numerische IDs
                (m.id === matchId || m.matchId === matchId)
            );

            if (match) {
                console.log(`?? Retrieved match: ${tournamentId}/${matchId} (UUID: ${match.uniqueId || 'none'})`);
                return match;
            }

            console.warn(`?? Match not found: ${tournamentId}/${matchId}`);
            return null;
        } catch (error) {
            console.error('? Get match error:', error.message);
            return null;
        }
    }

    /**
     * Get pending matches for a tournament
     * @param {string} tournamentId - Tournament ID
     * @returns {Promise<Array>} Array of pending matches
     */
    async getPendingMatches(tournamentId) {
        try {
            const matches = await this.getTournamentMatches(tournamentId);
            const pendingMatches = matches.filter(match =>
                match.status === 'NotStarted' ||
                match.status === 'pending' ||
                (!match.status && !match.winner)
            );

            console.log(`? Found ${pendingMatches.length} pending matches for tournament: ${tournamentId}`);
            return pendingMatches;
        } catch (error) {
            console.error('? Get pending matches error:', error.message);
            return [];
        }
    }

    /**
     * Get completed matches for a tournament
     * @param {string} tournamentId - Tournament ID
     * @returns {Promise<Array>} Array of completed matches
     */
    async getCompletedMatches(tournamentId) {
        try {
            const matches = await this.getTournamentMatches(tournamentId);
            const completedMatches = matches.filter(match =>
                match.status === 'Finished' ||
                match.status === 'completed' ||
                match.winner
            );

            console.log(`? Found ${completedMatches.length} completed matches for tournament: ${tournamentId}`);
            return completedMatches;
        } catch (error) {
            console.error('? Get completed matches error:', error.message);
            return [];
        }
    }

    /**
     * Enhanced match result validation with game rules support
     * @param {Object} result - Match result to validate
     * @returns {Object} Validation result
     */
    validateMatchResultWithGameRules(result) {
        try {
            console.log(`?? [API] Validating match result with game rules:`, JSON.stringify(result, null, 2));

            if (!result || typeof result !== 'object') {
                return { valid: false, error: 'Result must be an object' };
            }

            // Validate numeric fields - but allow 0 values
            const numericFields = ['player1Sets', 'player2Sets', 'player1Legs', 'player2Legs'];
            for (const field of numericFields) {
                if (result[field] !== undefined && result[field] !== null) {
                    const value = Number(result[field]);
                    if (isNaN(value) || value < 0) {
                        console.error(`? [API] Validation failed: ${field} must be a non-negative number, got: ${value}`);
                        return { valid: false, error: `${field} must be a non-negative number` };
                    }
                }
            }

            // Convert to numbers for validation
            const p1Sets = Number(result.player1Sets) || 0;
            const p2Sets = Number(result.player2Sets) || 0;
            const p1Legs = Number(result.player1Legs) || 0;
            const p2Legs = Number(result.player2Legs) || 0;

            console.log(`?? [API] Parsed values: Sets ${p1Sets}-${p2Sets}, Legs ${p1Legs}-${p2Legs}`);

            // Check if there's a winner - more flexible validation
            const hasSetWinner = p1Sets > 0 || p2Sets > 0;
            const hasLegWinner = p1Legs > 0 || p2Legs > 0;
            const isExplicitlyFinished = result.status === 'Finished';

            console.log(`?? [API] Winner validation:`);
            console.log(`   Has set winner: ${hasSetWinner} (${p1Sets} vs ${p2Sets})`);
            console.log(`   Has leg winner: ${hasLegWinner} (${p1Legs} vs ${p2Legs})`);
            console.log(`   Explicitly finished: ${isExplicitlyFinished}`);

            // Accept the match if any of these conditions are met:
            if (!hasSetWinner && !hasLegWinner && !isExplicitlyFinished) {
                console.error(`? [API] Validation failed: No clear winner - no sets, no legs, and not marked as finished`);
                return {
                    valid: false,
                    error: 'Match result must have a winner (either sets > 0, legs > 0, or status = Finished)'
                };
            }

            // Enhanced game rules validation
            if (result.gameRulesUsed && typeof result.gameRulesUsed === 'object') {
                const gameRules = result.gameRulesUsed;
                console.log(`?? [API] Game rules found:`, gameRules);

                // Validate sets against game rules
                if (gameRules.playWithSets && gameRules.setsToWin) {
                    const maxSets = Number(gameRules.setsToWin) || 3;
                    if (p1Sets > maxSets || p2Sets > maxSets) {
                        console.error(`? [API] Validation failed: Sets exceed maximum (${maxSets})`);
                        return {
                            valid: false,
                            error: `Sets cannot exceed maximum of ${maxSets} based on game rules`
                        };
                    }

                    // Check if match is properly finished based on sets
                    if ((p1Sets >= maxSets || p2Sets >= maxSets) && Math.abs(p1Sets - p2Sets) < 1) {
                        console.warn(`?? [API] Match may not be properly finished - winner should have ${maxSets} sets`);
                    }
                }

                // Validate legs against game rules  
                if (gameRules.legsToWin) {
                    const maxLegs = Number(gameRules.legsToWin) * (Math.max(p1Sets, p2Sets) || 1) * 2; // Rough estimation
                    if (p1Legs > maxLegs || p2Legs > maxLegs) {
                        console.warn(`?? [API] Legs count seems high compared to game rules (${maxLegs} estimated max)`);
                    }
                }

                console.log(`? [API] Game rules validation passed for: ${gameRules.name || 'Unknown Rules'}`);
            } else {
                console.log(`?? [API] No game rules provided - using basic validation only`);
            }

            console.log(`? [API] Match result validation passed`);
            return { valid: true };

        } catch (error) {
            console.error(`? [API] Validation error:`, error);
            return { valid: false, error: error.message };
        }
    }

    /**
     * Legacy validation method - kept for compatibility
     */
    validateMatchResult(result) {
        return this.validateMatchResultWithGameRules(result);
    }

    /**
     * Get tournament information
     * @param {string} tournamentId - Tournament ID
     * @returns {Object} Tournament information
     */
    getTournamentInfo(tournamentId) {
        const tournament = this.tournamentRegistry.getTournament(tournamentId);
        if (!tournament) {
            throw new Error(`Tournament not found: ${tournamentId}`);
        }
        return tournament;
    }

    /**
     * Process the result queue (forward results to Tournament Planner API)
     * @returns {Promise<void>}
     */
    async processResultQueue() {
        try {
            if (this.processingQueue.length === 0) {
                return;
            }

            console.log(`?? Processing ${this.processingQueue.length} pending results...`);

            const processed = [];
            const failed = [];

            for (const queueItem of this.processingQueue) {
                try {
                    const success = await this.forwardResultToTournamentPlanner(
                        queueItem.tournamentId,
                        queueItem.matchId,
                        queueItem.result
                    );

                    if (success) {
                        // Mark as processed
                        const resultKey = `${queueItem.tournamentId}_${queueItem.matchId}`;
                        const pendingResult = this.pendingResults.get(resultKey);
                        if (pendingResult) {
                            pendingResult.processed = true;
                            pendingResult.processedAt = new Date();
                        }

                        processed.push(queueItem);
                        console.log(`? Result forwarded: ${queueItem.tournamentId}/${queueItem.matchId}`);
                    } else {
                        queueItem.attempts++;
                        if (queueItem.attempts >= queueItem.maxAttempts) {
                            failed.push(queueItem);
                            console.error(`? Result forwarding failed after ${queueItem.attempts} attempts: ${queueItem.tournamentId}/${queueItem.matchId}`);
                        } else {
                            console.warn(`?? Result forwarding failed, will retry: ${queueItem.tournamentId}/${queueItem.matchId} (attempt ${queueItem.attempts})`);
                        }
                    }
                } catch (error) {
                    queueItem.attempts++;
                    console.error(`? Error processing result: ${error.message}`);

                    if (queueItem.attempts >= queueItem.maxAttempts) {
                        failed.push(queueItem);
                    }
                }
            }

            // Remove processed and failed items from queue
            this.processingQueue = this.processingQueue.filter(item =>
                !processed.includes(item) && !failed.includes(item)
            );

            if (processed.length > 0) {
                console.log(`? Successfully processed ${processed.length} results`);
            }

            if (failed.length > 0) {
                console.error(`? Failed to process ${failed.length} results after maximum attempts`);
            }
        } catch (error) {
            console.error('? Process queue error:', error.message);
        }
    }

    /**
     * Forward match result to Tournament Planner API
     * @param {string} tournamentId - Tournament ID
     * @param {string} matchId - Match ID
     * @param {Object} result - Match result
     * @returns {Promise<boolean>} Success status
     */
    async forwardResultToTournamentPlanner(tournamentId, matchId, result) {
        try {
            const tournament = this.tournamentRegistry.getTournament(tournamentId);
            if (!tournament || !tournament.apiEndpoint) {
                console.warn(`?? No API endpoint for tournament: ${tournamentId}`);
                return false;
            }

            // Use axios for HTTP requests
            const axios = require('axios');

            const response = await axios.put(
                `${tournament.apiEndpoint}/api/matches/${matchId}/result`,
                result, {
                    timeout: 5000,
                    headers: {
                        'Content-Type': 'application/json',
                        'User-Agent': 'Tournament-Hub/1.0'
                    }
                }
            );

            if (response.status === 200 || response.status === 204) {
                console.log(`? Result forwarded to Tournament Planner: ${tournamentId}/${matchId}`);
                return true;
            } else {
                console.warn(`?? Unexpected response status: ${response.status}`);
                return false;
            }
        } catch (error) {
            if (error.code === 'ECONNREFUSED' || error.code === 'ETIMEDOUT') {
                console.warn(`?? Tournament Planner API not reachable: ${error.message}`);
            } else {
                console.error(`? Error forwarding result: ${error.message}`);
            }
            return false;
        }
    }

    /**
     * Get match statistics for a tournament
     * @param {string} tournamentId - Tournament ID
     * @returns {Promise<Object>} Match statistics
     */
    async getMatchStatistics(tournamentId) {
        try {
            const matches = await this.getTournamentMatches(tournamentId);
            const pending = await this.getPendingMatches(tournamentId);
            const completed = await this.getCompletedMatches(tournamentId);
            const inProgress = matches.filter(m =>
                m.status === 'InProgress' || m.status === 'active'
            );

            return {
                total: matches.length,
                pending: pending.length,
                completed: completed.length,
                inProgress: inProgress.length,
                completionRate: matches.length > 0 ?
                    Math.round((completed.length / matches.length) * 100) : 0
            };
        } catch (error) {
            console.error('? Get match statistics error:', error.message);
            return {
                total: 0,
                pending: 0,
                completed: 0,
                inProgress: 0,
                completionRate: 0
            };
        }
    }

    /**
     * Get service statistics
     * @returns {Object} Service statistics
     */
    getStatistics() {
        return {
            service: 'MatchService',
            version: '1.0.0',
            pendingResults: this.pendingResults.size,
            processingQueueLength: this.processingQueue.length,
            features: [
                'Match result submission',
                'Result validation',
                'Automatic forwarding to Tournament Planner',
                'Retry mechanism',
                'Match statistics'
            ]
        };
    }

    /**
     * Clear old pending results (cleanup method)
     * @param {number} maxAgeHours - Maximum age in hours
     * @returns {number} Number of cleared results
     */
    clearOldPendingResults(maxAgeHours = 24) {
        try {
            const cutoffTime = new Date(Date.now() - (maxAgeHours * 60 * 60 * 1000));
            let cleared = 0;

            for (const [key, result] of this.pendingResults.entries()) {
                if (result.submittedAt < cutoffTime && result.processed) {
                    this.pendingResults.delete(key);
                    cleared++;
                }
            }

            if (cleared > 0) {
                console.log(`?? Cleared ${cleared} old pending results`);
            }

            return cleared;
        } catch (error) {
            console.error('? Clear old results error:', error.message);
            return 0;
        }
    }
}

module.exports = MatchService;