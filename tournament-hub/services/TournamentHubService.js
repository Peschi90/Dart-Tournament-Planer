class TournamentHubService {
  constructor(db, io, tournamentRegistry) {
    this.db = db;
    this.io = io;
    this.tournamentRegistry = tournamentRegistry;
    this.tournaments = new Map(); // tournamentId -> tournament info
    this.matches = new Map(); // tournamentId -> Map(matchId -> match data)
  }

  async initialize() {
    // Load existing tournaments from database
    try {
      const existingTournaments = await this.db.getAllTournaments();
      for (const tournament of existingTournaments) {
        this.tournaments.set(tournament.tournamentId, tournament);
        
        // Load matches for each tournament
        const matches = await this.db.getMatchesForTournament(tournament.tournamentId);
        const matchMap = new Map();
        for (const match of matches) {
          matchMap.set(match.matchId, match);
        }
        this.matches.set(tournament.tournamentId, matchMap);
      }
      console.log(`?? Loaded ${existingTournaments.length} tournaments from database`); 
    } catch (error) {
      console.warn('Warning: Could not load existing tournaments:', error.message);
    }
  }

  /**
   * Registriert ein neues Tournament beim Hub
   */
  async registerTournament(tournamentData) {
    const {
      tournamentId,
      name,
      description,
      apiEndpoint,
      location,
      apiKey
    } = tournamentData;

    // Prüfe ob Tournament bereits existiert
    if (this.tournaments.has(tournamentId)) {
      const existing = this.tournaments.get(tournamentId);
      existing.lastHeartbeat = new Date();
      existing.isOnline = true;
      existing.apiEndpoint = apiEndpoint; // Update endpoint if changed
      
      await this.db.updateTournament(tournamentId, existing);
      return existing;
    }

    // Erstelle neues Tournament
    const tournament = {
      tournamentId,
      name,
      description: description || '',
      apiEndpoint,
      location: location || '',
      apiKey: apiKey || '',
      registeredAt: new Date(),
      lastHeartbeat: new Date(),
      isOnline: true,
      status: 'active',
      activeMatches: 0,
      totalPlayers: 0
    };

    this.tournaments.set(tournamentId, tournament);
    this.matches.set(tournamentId, new Map());

    // Save to database
    await this.db.saveTournament(tournament);

    return tournament;
  }

  /**
   * Aktualisiert das Heartbeat eines Tournaments
   */
  async updateHeartbeat(tournamentId, data) {
    const tournament = this.tournaments.get(tournamentId);
    if (!tournament) {
      throw new Error('Tournament not found');
    }

    tournament.lastHeartbeat = new Date();
    tournament.status = data.status || 'active';
    tournament.activeMatches = data.activeMatches || 0;
    tournament.totalPlayers = data.totalPlayers || 0;
    tournament.isOnline = true;

    await this.db.updateTournament(tournamentId, tournament);
    return tournament;
  }

  /**
   * Synchronisiert Matches für ein Tournament
   */
  async syncMatches(tournamentId, matches) {
    if (!this.tournaments.has(tournamentId)) {
      throw new Error('Tournament not found');
    }

    const tournamentMatches = this.matches.get(tournamentId) || new Map();
    let syncedCount = 0;
    let updatedCount = 0;
    let newCount = 0;

    for (const match of matches) {
      const matchId = match.id || match.matchId;
      const existingMatch = tournamentMatches.get(matchId);

      const matchData = {
        matchId: matchId,
        tournamentId: tournamentId,
        player1: match.player1 || null,
        player2: match.player2 || null,
        player1Sets: match.player1Sets || 0,
        player2Sets: match.player2Sets || 0,
        player1Legs: match.player1Legs || 0,
        player2Legs: match.player2Legs || 0,
        status: match.status || 'NotStarted',
        winner: match.winner || null,
        startTime: match.startTime || null,
        endTime: match.endTime || null,
        notes: match.notes || '',
        classId: match.classId || null,
        className: match.className || null,
        matchType: match.matchType || 'Group',
        // KORRIGIERT: Group-Informationen hinzufügen
        groupId: match.groupId || null,
        groupName: match.groupName || null,
        // Game Rules Information
        gameRulesId: match.gameRulesId || null,
        gameRulesUsed: match.gameRulesUsed || null,
        lastUpdated: new Date()
      };

      if (existingMatch) {
        // Update existing match
        tournamentMatches.set(matchId, { ...existingMatch, ...matchData });
        updatedCount++;
      } else {
        // Add new match
        tournamentMatches.set(matchId, matchData);
        newCount++;
      }

      // Save to database
      await this.db.saveMatch(matchData);
      syncedCount++;
    }

    this.matches.set(tournamentId, tournamentMatches);

    // Update tournament last sync time
    const tournament = this.tournaments.get(tournamentId);
    tournament.lastSync = new Date();
    await this.db.updateTournament(tournamentId, tournament);

    return {
      syncedCount,
      updatedCount,
      newCount,
      totalMatches: tournamentMatches.size
    };
  }

  /**
   * Synchronisiert ein komplettes Tournament mit Klassen-Informationen
   */
  async syncFullTournament(tournamentData) {
    const { tournamentId, name, classes, matches, gameRules, syncedAt } = tournamentData;

    if (!this.tournaments.has(tournamentId)) {
      throw new Error('Tournament not found');
    }

    // Update tournament with class and game rules information
    const tournament = this.tournaments.get(tournamentId);
    tournament.name = name;
    tournament.classes = classes || [];
    // ERWEITERT: Game Rules hinzufügen
    tournament.gameRules = gameRules || [];
    tournament.lastSync = new Date(syncedAt);
    tournament.lastHeartbeat = new Date();

    console.log(`🎮 [TOURNAMENT-SYNC] Full sync for ${tournamentId}:`);
    console.log(`   Name: ${name}`);
    console.log(`   Classes: ${classes?.length || 0}`);
    console.log(`   Matches: ${matches?.length || 0}`);
    console.log(`   Game Rules: ${gameRules?.length || 0}`);

    // Sync matches with class and group information
    const result = await this.syncMatches(tournamentId, matches);

    await this.db.updateTournament(tournamentId, tournament);

    console.log(`✅ [TOURNAMENT-SYNC] Tournament ${tournamentId} synced successfully:`, result);

    return {
      ...result,
      classCount: classes?.length || 0,
      gameRulesCount: gameRules?.length || 0
    };
  }

  /**
   * Holt Matches für ein Tournament, optional gefiltert nach Klasse
   */
  async getMatches(tournamentId, statusFilter = null, classId = null) {
    const tournamentMatches = this.matches.get(tournamentId);
    if (!tournamentMatches) {
      return [];
    }

    let matches = Array.from(tournamentMatches.values());

    // Filter by status if specified
    if (statusFilter) {
      matches = matches.filter(match => match.status === statusFilter);
    }

    // Filter by class if specified
    if (classId) {
      matches = matches.filter(match => 
        match.classId && match.classId.toString() === classId.toString()
      );
    }

    // Sort by match ID for consistent ordering
    matches.sort((a, b) => {
      const aId = parseInt(a.matchId) || 0;
      const bId = parseInt(b.matchId) || 0;
      return aId - bId;
    });

    return matches;
  }

  /**
   * Holt alle Game Rules für ein Tournament
   */
  async getTournamentGameRules(tournamentId) {
    const tournament = this.tournaments.get(tournamentId);
    if (!tournament || !tournament.gameRules) {
      console.log(`⚠️ [GAME-RULES] No game rules found for tournament ${tournamentId}`);
      return [];
    }

    console.log(`✅ [GAME-RULES] Found ${tournament.gameRules.length} game rules for tournament ${tournamentId}`);
    return tournament.gameRules;
  }

  /**
   * Holt alle Klassen für ein Tournament
   */
  async getTournamentClasses(tournamentId) {
    const tournament = this.tournaments.get(tournamentId);
    if (!tournament || !tournament.classes) {
      return [];
    }

    return tournament.classes;
  }

  /**
   * Holt ein spezifisches Tournament mit Klassen-Informationen
   */
  async getTournament(tournamentId) {
    const tournament = this.tournaments.get(tournamentId);
    if (!tournament) {
      return null;
    }

    const tournamentMatches = this.matches.get(tournamentId) || new Map();
    const classes = tournament.classes || [];

    // Calculate stats per class
    const classStats = classes.map(cls => {
      const classMatches = Array.from(tournamentMatches.values())
        .filter(match => match.classId === cls.id);
      
      return {
        ...cls,
        matchCount: classMatches.length,
        activeMatches: classMatches.filter(m => m.status === 'InProgress').length,
        finishedMatches: classMatches.filter(m => m.status === 'Finished').length
      };
    });

    return {
      ...tournament,
      joinUrl: `${process.env.BASE_URL || 'http://localhost:3000'}/join/${tournamentId}`,
      matchCount: tournamentMatches.size,
      classes: classStats
    };
  }

  /**
   * Entregistriert ein Tournament
   */
  async unregisterTournament(tournamentId) {
    const tournament = this.tournaments.get(tournamentId);
    if (tournament) {
      tournament.isOnline = false;
      tournament.lastHeartbeat = new Date();
      await this.db.updateTournament(tournamentId, tournament);
    }

    return true;
  }

  /**
   * Bereinigt offline Tournaments
   */
  async cleanupOfflineTournaments(hoursOld = 24) {
    const cutoffTime = new Date(Date.now() - (hoursOld * 60 * 60 * 1000));
    let cleanedCount = 0;

    for (const [tournamentId, tournament] of this.tournaments) {
      if (!tournament.isOnline && tournament.lastHeartbeat < cutoffTime) {
        this.tournaments.delete(tournamentId);
        this.matches.delete(tournamentId);
        await this.db.deleteTournament(tournamentId);
        cleanedCount++;
      }
    }

    console.log(`?? Cleaned up ${cleanedCount} offline tournaments`);
    return cleanedCount;
  }

  /**
   * Cleanup bei Server-Shutdown
   */
  async cleanup() {
    // Mark all tournaments as offline
    for (const [tournamentId, tournament] of this.tournaments) {
      tournament.isOnline = false;
      tournament.lastHeartbeat = new Date();
      await this.db.updateTournament(tournamentId, tournament);
    }

    console.log(`?? Marked ${this.tournaments.size} tournaments as offline`); 
  }

  /**
   * Statistiken für Status-Endpoint
   */
  getStatistics() {
    const onlineCount = Array.from(this.tournaments.values())
      .filter(t => t.isOnline).length;
    
    const totalMatches = Array.from(this.matches.values())
      .reduce((sum, matches) => sum + matches.size, 0);

    const activeMatches = Array.from(this.matches.values())
      .reduce((sum, matches) => {
        return sum + Array.from(matches.values())
          .filter(m => m.status === 'InProgress').length;
      }, 0);

    return {
      totalTournaments: this.tournaments.size,
      onlineTournaments: onlineCount,
      totalMatches,
      activeMatches
    };
  }
}

module.exports = TournamentHubService;