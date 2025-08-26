/**
 * Database Service
 * Handles database operations and connections
 */
class DatabaseService {
    constructor() {
        this.initialized = false;
        this.connection = null;
        this.dbPath = process.env.DATABASE_URL || 'tournaments.db'; 
    }
     
    /**
     * Initialize database connection and create tables if needed
     * @returns {Promise<boolean>} Success status
     */
    async initialize() {
        try {
            // For now, we'll use in-memory storage
            // In production, this would connect to SQLite or other database
            this.connection = {
                tournaments: new Map(),
                matches: new Map(),
                sessions: new Map()
            };
            
            console.log('?? Database service initialized (in-memory storage)');
            this.initialized = true;
            return true;
        } catch (error) {
            console.error('? Database initialization error:', error.message);
            this.initialized = false;
            return false;
        }
    }

    /**
     * Close database connections
     * @returns {Promise<boolean>} Success status
     */
    async close() {
        try {
            if (this.connection) {
                // Clear in-memory data
                this.connection.tournaments.clear();
                this.connection.matches.clear();
                this.connection.sessions.clear();
                this.connection = null;
            }
            
            console.log('?? Database service closed');
            this.initialized = false;
            return true;
        } catch (error) {
            console.error('? Database close error:', error.message);
            return false;
        }
    }

    /**
     * Check if database is initialized
     * @returns {boolean} Initialization status
     */
    isInitialized() {
        return this.initialized;
    }

    /**
     * Save tournament data to database
     * @param {string} tournamentId - Tournament ID
     * @param {Object} tournamentData - Tournament data to save
     * @returns {Promise<boolean>} Success status
     */
    async saveTournament(tournamentId, tournamentData) {
        try {
            if (!this.initialized || !this.connection) {
                throw new Error('Database not initialized');
            }

            const dataToSave = {
                ...tournamentData,
                savedAt: new Date(),
                updatedAt: new Date()
            };

            this.connection.tournaments.set(tournamentId, dataToSave);
            console.log(`?? Tournament data saved: ${tournamentId}`);
            return true;
        } catch (error) {
            console.error('? Save tournament error:', error.message);
            return false;
        }
    }

    /**
     * Load tournament data from database
     * @param {string} tournamentId - Tournament ID
     * @returns {Promise<Object|null>} Tournament data or null
     */
    async loadTournament(tournamentId) {
        try {
            if (!this.initialized || !this.connection) {
                throw new Error('Database not initialized');
            }

            const data = this.connection.tournaments.get(tournamentId);
            if (data) {
                console.log(`?? Tournament data loaded: ${tournamentId}`);
                return data;
            }
            
            return null;
        } catch (error) {
            console.error('? Load tournament error:', error.message);
            return null;
        }
    }

    /**
     * Get database statistics
     * @returns {Promise<Object>} Database statistics
     */
    async getStatistics() {
        try {
            if (!this.initialized || !this.connection) {
                return {
                    initialized: false,
                    tournaments: 0,
                    matches: 0,
                    sessions: 0
                };
            }

            return {
                initialized: true,
                tournaments: this.connection.tournaments.size,
                matches: this.connection.matches.size,
                sessions: this.connection.sessions.size,
                storageType: 'in-memory'
            };
        } catch (error) {
            console.error('? Get statistics error:', error.message);
            return {
                initialized: false,
                error: error.message
            };
        }
    }
}

module.exports = DatabaseService;