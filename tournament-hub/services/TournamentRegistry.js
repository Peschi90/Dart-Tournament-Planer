const { v4: uuidv4 } = require('uuid');

/**
 * Tournament Registry Service
 * Manages all registered tournaments and their state
 */
class TournamentRegistry {
    constructor() {
        this.tournaments = new Map();
        this.statistics = {
            activeTournaments: 0,
            totalConnections: 0,
            totalMatchesProcessed: 0,
            dailyMatchesProcessed: 0,
            lastResetDate: new Date().toDateString()
        };
    }

    /**
     * Register a new tournament with classes and game rules
     * @param {Object} tournamentData - Tournament registration data
     * @returns {Object} Registration result
     */
    registerTournament(tournamentData) {
        try {
            const {
                tournamentId,
                name,
                description,
                apiEndpoint,
                location,
                apiKey,
                classes,
                gameRules,
                totalPlayers,
                startTime,
                metadata
            } = tournamentData;

            if (!tournamentId || !name) {
                throw new Error('Tournament ID and name are required');
            }

            const tournament = {
                id: tournamentId,
                name: name,
                description: description || '',
                apiEndpoint: apiEndpoint || null,
                location: location || 'Unknown',
                apiKey: apiKey || null,
                registeredAt: new Date(),
                lastHeartbeat: new Date(),
                status: 'active',
                connectedClients: 0,
                matchesProcessed: 0,
                matches: [],

                // Enhanced class support with default Dart Tournament classes
                classes: classes || [
                    { id: 1, name: 'Platin', playerCount: 0, groupCount: 0, matchCount: 0 },
                    { id: 2, name: 'Gold', playerCount: 0, groupCount: 0, matchCount: 0 },
                    { id: 3, name: 'Silber', playerCount: 0, groupCount: 0, matchCount: 0 },
                    { id: 4, name: 'Bronze', playerCount: 0, groupCount: 0, matchCount: 0 }
                ],
                gameRules: gameRules || [{
                    id: 1,
                    name: 'Standard 501',
                    gamePoints: 501,
                    setsToWin: 3,
                    legsToWin: 3,
                    legsPerSet: 5,
                    maxSets: 5,
                    maxLegsPerSet: 5
                }],
                totalPlayers: totalPlayers || 0,
                startTime: startTime ? new Date(startTime) : new Date(),
                metadata: metadata || {},

                // Klassen-spezifische Daten
                currentClassId: null, // Aktuell ausgewählte Klasse
                matchesByClass: new Map(), // Matches gruppiert nach Klassen
            };

            this.tournaments.set(tournamentId, tournament);
            this.updateStatistics();

            console.log(`✅ Tournament registered: ${tournamentId} - ${name}`);
            console.log(`📚 Classes: ${tournament.classes.length}, GameRules: ${tournament.gameRules.length}`);
            console.log(`🎮 Game Rules: ${tournament.gameRules.map(gr => gr.name).join(', ')}`);

            return {
                success: true,
                tournament: tournament,
                hubEndpoint: process.env.BASE_URL || 'http://localhost:3000',
                joinUrl: `${process.env.BASE_URL || 'http://localhost:3000'}/tournament/${tournamentId}`,
                websocketUrl: `${process.env.BASE_URL || 'http://localhost:3000'}`,
                registeredAt: tournament.registeredAt
            };
        } catch (error) {
            console.error('Error registering tournament:', error.message);
            return {
                success: false,
                error: error.message
            };
        }
    }

    /**
     * Update tournament heartbeat
     * @param {string} tournamentId - Tournament ID
     * @param {Object} heartbeatData - Heartbeat data
     * @returns {boolean} Success status
     */
    updateHeartbeat(tournamentId, heartbeatData) {
        try {
            const tournament = this.tournaments.get(tournamentId);
            if (!tournament) {
                console.warn(`? Heartbeat for unknown tournament: ${tournamentId}`);
                return false;
            }

            tournament.lastHeartbeat = new Date();
            tournament.status = heartbeatData.status || 'active';

            if (heartbeatData.activeMatches !== undefined) {
                tournament.activeMatches = heartbeatData.activeMatches;
            }

            if (heartbeatData.totalPlayers !== undefined) {
                tournament.totalPlayers = heartbeatData.totalPlayers;
            }

            // Update metadata if provided
            if (heartbeatData.metadata) {
                tournament.metadata = {...tournament.metadata, ...heartbeatData.metadata };
            }

            console.log(`?? Heartbeat updated for tournament: ${tournamentId}`);
            return true;
        } catch (error) {
            console.error('Error updating heartbeat:', error.message);
            return false;
        }
    }

    /**
     * Synchronize matches for a tournament (now supports classes)
     * @param {string} tournamentId - Tournament ID
     * @param {Array} matches - Array of match data
     * @param {string} classId - Optional: specific class ID
     * @returns {boolean} Success status
     */
    syncMatches(tournamentId, matches, classId = null) {
            try {
                const tournament = this.tournaments.get(tournamentId);
                if (!tournament) {
                    console.warn(`⚠️ Match sync for unknown tournament: ${tournamentId}`);
                    return false;
                }

                // Store the matches with detailed logging and class support
                if (matches && Array.isArray(matches)) {
                    const processedMatches = matches.map(match => ({
                        ...match,
                        syncedAt: new Date(),
                        // Ensure consistent field names and proper class handling
                        matchId: match.matchId || match.id || (match.Match && match.Match.Id),
                        classId: match.classId || match.ClassId || classId || 1,
                        className: match.className || match.ClassName || this.getClassNameById(match.classId || match.ClassId || classId),
                        status: match.status || match.Status || 'NotStarted',
                        player1: match.player1 || (match.Player1 && match.Player1.Name) || match.Player1 || 'Player 1',
                        player2: match.player2 || (match.Player2 && match.Player2.Name) || match.Player2 || 'Player 2',
                        player1Sets: match.player1Sets || match.Player1Sets || 0,
                        player2Sets: match.player2Sets || match.Player2Sets || 0,
                        player1Legs: match.player1Legs || match.Player1Legs || 0,
                        player2Legs: match.player2Legs || match.Player2Legs || 0,
                        notes: match.notes || match.Notes || '',
                        matchType: match.matchType || match.MatchType || 'Group',
                        groupName: match.groupName || match.GroupName || '',

                        // GameRules-bezogene Felder
                        gameRulesId: match.gameRulesId || match.GameRulesId || 1,
                        currentScore1: match.currentScore1 || match.CurrentScore1 || null,
                        currentScore2: match.currentScore2 || match.CurrentScore2 || null,
                    }));

                    // Store matches both globally and by class
                    tournament.matches = processedMatches;

                    // Initialize matchesByClass if needed
                    if (!tournament.matchesByClass) {
                        tournament.matchesByClass = new Map();
                    }

                    // Group matches by class
                    const matchesByClass = new Map();
                    processedMatches.forEach(match => {
                        const classId = match.classId || 1;
                        if (!matchesByClass.has(classId)) {
                            matchesByClass.set(classId, []);
                        }
                        matchesByClass.get(classId).push(match);
                    });

                    tournament.matchesByClass = matchesByClass;

                    console.log(`📊 Match sync details for ${tournamentId}:`);
                    console.log(`   📦 Total matches: ${matches.length}`);
                    console.log(`   📚 Classes with matches: ${matchesByClass.size}`);
                    console.log(`   🎮 Match IDs: [${processedMatches.map(m => m.matchId).join(', ')}]`);
                    console.log(`   📋 Match statuses: [${processedMatches.map(m => m.status).join(', ')}]`);
                    console.log(`   👥 Players sample: ${processedMatches.slice(0, 3).map(m => `${m.player1} vs ${m.player2}`).join(', ')}`);
                
                // Log class distribution with names
                for (const [classId, classMatches] of matchesByClass) {
                    const className = this.getClassNameById(classId);
                    console.log(`   📚 ${className} (ID: ${classId}): ${classMatches.length} matches`);
                }
                
            } else {
                tournament.matches = [];
                tournament.matchesByClass = new Map();
                console.log(`⚠️ Empty or invalid matches array for tournament: ${tournamentId}`);
            }

            tournament.lastMatchSync = new Date();
            tournament.activeMatches = tournament.matches.filter(m => m.status === 'InProgress').length;
            tournament.totalMatches = tournament.matches.length;
            
            // Update processed count (cumulative and daily)
            const newMatches = matches ? matches.length : 0;
            this.statistics.totalMatchesProcessed += newMatches;
            
            // Check if we need to reset daily counter
            const today = new Date().toDateString();
            if (this.statistics.lastResetDate !== today) {
                this.statistics.dailyMatchesProcessed = 0;
                this.statistics.lastResetDate = today;
                console.log(`🔄 Daily match counter reset for new day: ${today}`);
            }
            
            this.statistics.dailyMatchesProcessed += newMatches;

            console.log(`✅ Matches synced for tournament: ${tournamentId} (${newMatches} matches)`);
            return true;
        } catch (error) {
            console.error('❌ Error syncing matches:', error.message);
            console.error('Stack trace:', error.stack);
            return false;
        }
    }

    /**
     * Helper: Get class name by ID
     */
    getClassNameById(classId) {
        const classNames = {
            1: 'Platin',
            2: 'Gold', 
            3: 'Silber',
            4: 'Bronze'
        };
        return classNames[classId] || `Klasse ${classId}`;
    }

    /**
     * Get a specific tournament
     * @param {string} tournamentId - Tournament ID
     * @returns {Object|null} Tournament data or null
     */
    getTournament(tournamentId) {
        return this.tournaments.get(tournamentId) || null;
    }

    /**
     * Get all tournaments
     * @returns {Array} Array of all tournaments
     */
    getAllTournaments() {
        const tournaments = Array.from(this.tournaments.values());
        console.log(`?? getAllTournaments called: Found ${tournaments.length} tournaments`);
        tournaments.forEach(t => {
            console.log(`   - ${t.id}: ${t.name} (${t.status}, clients: ${t.connectedClients || 0})`);
        });
        return tournaments;
    }

    /**
     * Get active tournaments (heartbeat within last 5 minutes)
     * @returns {Array} Array of active tournaments
     */
    getActiveTournaments() {
        const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
        return Array.from(this.tournaments.values()).filter(
            tournament => tournament.lastHeartbeat > fiveMinutesAgo
        );
    }

    /**
     * Unregister a tournament
     * @param {string} tournamentId - Tournament ID
     * @returns {boolean} Success status
     */
    unregisterTournament(tournamentId) {
        try {
            const tournament = this.tournaments.get(tournamentId);
            if (!tournament) {
                console.warn(`? Unregister unknown tournament: ${tournamentId}`);
                return false;
            }

            this.tournaments.delete(tournamentId);
            this.updateStatistics();

            console.log(`?? Tournament unregistered: ${tournamentId} - ${tournament.name}`);
            return true;
        } catch (error) {
            console.error('Error unregistering tournament:', error.message);
            return false;
        }
    }

    /**
     * Update client count for a tournament
     * @param {string} tournamentId - Tournament ID
     * @param {number} count - Client count
     */
    updateClientCount(tournamentId, count) {
        const tournament = this.tournaments.get(tournamentId);
        if (tournament) {
            tournament.connectedClients = count;
            this.updateStatistics();
        }
    }

    /**
     * Clean up inactive tournaments (no heartbeat for 10 minutes)
     */
    cleanupInactiveTournaments() {
        const tenMinutesAgo = new Date(Date.now() - 10 * 60 * 1000);
        const toRemove = [];

        for (const [id, tournament] of this.tournaments.entries()) {
            if (tournament.lastHeartbeat < tenMinutesAgo) {
                toRemove.push(id);
            }
        }

        for (const id of toRemove) {
            console.log(`?? Cleaning up inactive tournament: ${id}`);
            this.tournaments.delete(id);
        }

        if (toRemove.length > 0) {
            this.updateStatistics();
        }

        return toRemove.length;
    }

    /**
     * Update internal statistics
     */
    updateStatistics() {
        this.statistics.activeTournaments = this.tournaments.size;
        this.statistics.totalConnections = Array.from(this.tournaments.values())
            .reduce((sum, tournament) => sum + (tournament.connectedClients || 0), 0);
    }

    /**
     * Increment daily match counter (for individual match submissions)
     */
    incrementDailyMatchCounter() {
        // Check if we need to reset daily counter
        const today = new Date().toDateString();
        if (this.statistics.lastResetDate !== today) {
            this.statistics.dailyMatchesProcessed = 0;
            this.statistics.lastResetDate = today;
            console.log(`🔄 Daily match counter reset for new day: ${today}`);
        }
        
        this.statistics.dailyMatchesProcessed += 1;
        this.statistics.totalMatchesProcessed += 1;
        
        console.log(`📊 Match counters updated - Daily: ${this.statistics.dailyMatchesProcessed}, Total: ${this.statistics.totalMatchesProcessed}`);
    }

    /**
     * Get current statistics
     * @returns {Object} Statistics object
     */
    getStatistics() {
        this.updateStatistics();
        
        // Check if we need to reset daily counter
        const today = new Date().toDateString();
        if (this.statistics.lastResetDate !== today) {
            this.statistics.dailyMatchesProcessed = 0;
            this.statistics.lastResetDate = today;
            console.log(`🔄 Daily match counter reset for new day: ${today}`);
        }
        
        const stats = {
            ...this.statistics,
            totalTournaments: this.tournaments.size,
            activeTournaments: this.getActiveTournaments().length
        };
        
        console.log(`📊 Statistics requested:`, stats);
        return stats;
    }

    /**
     * Get matches for a specific tournament and class
     * @param {string} tournamentId - Tournament ID
     * @param {string} classId - Class ID (optional)
     * @returns {Array} Array of matches
     */
    getTournamentMatches(tournamentId, classId = null) {
        const tournament = this.tournaments.get(tournamentId);
        if (!tournament) {
            console.log(`⚠️ [REGISTRY] Tournament not found: ${tournamentId}`);
            return [];
        }
        
        console.log(`📊 [REGISTRY] Getting matches for tournament ${tournamentId}, class: ${classId || 'All'}`);
        
        let matches = tournament.matches || [];
        
        if (classId && tournament.matchesByClass) {
            const classMatches = tournament.matchesByClass.get(parseInt(classId)) || [];
            console.log(`📚 [REGISTRY] Found ${classMatches.length} matches for class ${classId} from matchesByClass`);
            return classMatches;
        }
        
        // Filter by class if specified
        if (classId) {
            matches = matches.filter(match => 
                match.classId && match.classId.toString() === classId.toString()
            );
            console.log(`📚 [REGISTRY] Filtered to ${matches.length} matches for class ${classId}`);
        }
        
        // Sort by match ID for consistent ordering
        matches.sort((a, b) => {
            const aId = parseInt(a.matchId) || 0;
            const bId = parseInt(b.matchId) || 0;
            return aId - bId;
        });
        
        console.log(`✅ [REGISTRY] Returning ${matches.length} matches for tournament ${tournamentId}`);
        return matches;
    }

    /**
     * Get tournament classes
     * @param {string} tournamentId - Tournament ID
     * @returns {Array} Array of classes
     */
    getTournamentClasses(tournamentId) {
        const tournament = this.tournaments.get(tournamentId);
        return tournament ? (tournament.classes || []) : [];
    }

    /**
     * Get game rules for tournament
     * @param {string} tournamentId - Tournament ID
     * @returns {Array} Array of game rules
     */
    getTournamentGameRules(tournamentId) {
        const tournament = this.tournaments.get(tournamentId);
        return tournament ? (tournament.gameRules || []) : [];
    }

    /**
     * Set current class for tournament
     * @param {string} tournamentId - Tournament ID
     * @param {string} classId - Class ID
     * @returns {boolean} Success status
     */
    setCurrentClass(tournamentId, classId) {
        const tournament = this.tournaments.get(tournamentId);
        if (!tournament) return false;
        
        tournament.currentClassId = parseInt(classId);
        console.log(`🎯 Set current class for ${tournamentId}: ${classId}`);
        return true;
    }

    /**
     * Update tournament data (full sync)
     * @param {string} tournamentId - Tournament ID
     * @param {Object} tournamentData - Tournament data to update
     * @returns {boolean} Success status
     */
    updateTournament(tournamentId, tournamentData) {
        try {
            console.log(`🔄 [Registry] updateTournament called for: ${tournamentId}`);
            console.log(`📊 [Registry] Update data keys: ${Object.keys(tournamentData || {}).join(', ')}`);
            
            const tournament = this.tournaments.get(tournamentId);
            if (!tournament) {
                console.warn(`⚠️ [Registry] Tournament not found for update: ${tournamentId}`);
                return false;
            }

            // Update basic tournament information
            if (tournamentData.name) tournament.name = tournamentData.name;
            if (tournamentData.description) tournament.description = tournamentData.description;
            if (tournamentData.totalPlayers !== undefined) tournament.totalPlayers = tournamentData.totalPlayers;
            
            // Update classes if provided
            if (tournamentData.classes && Array.isArray(tournamentData.classes)) {
                tournament.classes = tournamentData.classes;
                console.log(`📚 [Registry] Updated classes: ${tournament.classes.length} classes`);
            }
            
            // Update game rules if provided
            if (tournamentData.gameRules && Array.isArray(tournamentData.gameRules)) {
                tournament.gameRules = tournamentData.gameRules;
                console.log(`🎮 [Registry] Updated game rules: ${tournament.gameRules.length} rules`);
            }
            
            // Update matches if provided
            if (tournamentData.matches && Array.isArray(tournamentData.matches)) {
                const processedMatches = tournamentData.matches.map(match => ({
                    ...match,
                    syncedAt: new Date(),
                    matchId: match.matchId || match.id,
                    classId: match.classId || 1,
                    className: match.className || this.getClassNameById(match.classId || 1),
                    status: match.status || 'NotStarted',
                    player1: match.player1 || 'Player 1',
                    player2: match.player2 || 'Player 2',
                    player1Sets: match.player1Sets || 0,
                    player2Sets: match.player2Sets || 0,
                    player1Legs: match.player1Legs || 0,
                    player2Legs: match.player2Legs || 0,
                    gameRulesUsed: match.gameRulesUsed || null
                }));
                
                tournament.matches = processedMatches;
                
                // Group matches by class
                const matchesByClass = new Map();
                processedMatches.forEach(match => {
                    const classId = match.classId || 1;
                    if (!matchesByClass.has(classId)) {
                        matchesByClass.set(classId, []);
                    }
                    matchesByClass.get(classId).push(match);
                });
                
                tournament.matchesByClass = matchesByClass;
                tournament.totalMatches = processedMatches.length;
                tournament.activeMatches = processedMatches.filter(m => m.status === 'InProgress').length;
                
                console.log(`📊 [Registry] Updated matches: ${processedMatches.length} total`);
                console.log(`📚 [Registry] Classes with matches: ${matchesByClass.size}`);
            }
            
            // Update metadata if provided
            if (tournamentData.metadata) {
                tournament.metadata = { ...tournament.metadata, ...tournamentData.metadata };
            }
            
            // Update timestamps
            tournament.lastUpdate = new Date();
            tournament.lastHeartbeat = new Date();
            
            // Update statistics
            this.updateStatistics();
            
            console.log(`✅ [Registry] Tournament updated successfully: ${tournamentId}`);
            return true;
            
        } catch (error) {
            console.error(`❌ [Registry] Error updating tournament ${tournamentId}:`, error.message);
            console.error(`❌ [Registry] Stack trace:`, error.stack);
            return false;
        }
    }
}

module.exports = TournamentRegistry;