// Tournament Hub Routes
const express = require('express');
const router = express.Router();

/**
 * Tournament Routes
 * Handles all tournament-related API endpoints
 */
module.exports = (tournamentHubService) => {
  
  /**
   * Register a new tournament
   * POST /api/tournaments/register
   */
  router.post('/register', async (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const tournamentData = req.body;
      
      console.log('🎯 Tournament registration request received');
      console.log('📦 Request body:', JSON.stringify(tournamentData, null, 2));
      
      // Validate required fields
      if (!tournamentData.tournamentId || !tournamentData.name) {
        console.log('❌ Missing required fields:', { 
          tournamentId: tournamentData.tournamentId, 
          name: tournamentData.name 
        });
        return res.status(400).json({
          success: false,
          error: 'Tournament ID and name are required'
        });
      }

      console.log(`🎯 Registering tournament: ${tournamentData.tournamentId} - ${tournamentData.name}`);

      // Register tournament using TournamentRegistry (not TournamentHubService)
      const result = tournamentRegistry.registerTournament(tournamentData);

      console.log('📊 Registration result:', JSON.stringify(result, null, 2));

      if (result.success) {
        console.log(`✅ Tournament successfully registered: ${result.tournament.id}`);
        
        res.json({
          success: true,
          message: 'Tournament registered successfully',
          data: {
            tournamentId: result.tournament.id,
            hubEndpoint: result.hubEndpoint,
            joinUrl: result.joinUrl,
            websocketUrl: result.websocketUrl,
            registeredAt: result.registeredAt
          }
        });
      } else {
        console.log('❌ Registration failed:', result.error);
        res.status(400).json({
          success: false,
          error: result.error
        });
      }
    } catch (error) {
      console.error('❌ Tournament registration error:', error.message);
      console.error('Stack trace:', error.stack);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Get tournament information
   * GET /api/tournaments/:id
   */
  router.get('/:id', (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const { id } = req.params;
      
      const tournament = tournamentRegistry.getTournament(id);
      
      if (tournament) {
        res.json({
          success: true,
          data: {
            id: tournament.id,
            name: tournament.name,
            description: tournament.description,
            location: tournament.location,
            status: tournament.status,
            registeredAt: tournament.registeredAt,
            lastHeartbeat: tournament.lastHeartbeat,
            connectedClients: tournament.connectedClients || 0,
            totalPlayers: tournament.totalPlayers || 0,
            activeMatches: tournament.activeMatches || 0
          }
        });
      } else {
        res.status(404).json({
          success: false,
          error: 'Tournament not found'
        });
      }
    } catch (error) {
      console.error('❌ Get tournament error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Update tournament heartbeat
   * POST /api/tournaments/:id/heartbeat
   */
  router.post('/:id/heartbeat', (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const { id } = req.params;
      const heartbeatData = req.body;
      
      console.log(`💓 Heartbeat received for tournament: ${id}`);
      
      const success = tournamentRegistry.updateHeartbeat(id, heartbeatData);
      
      if (success) {
        res.json({
          success: true,
          message: 'Heartbeat updated successfully',
          timestamp: new Date()
        });
      } else {
        res.status(404).json({
          success: false,
          error: 'Tournament not found'
        });
      }
    } catch (error) {
      console.error('❌ Heartbeat error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Synchronize tournament matches
   * POST /api/tournaments/:id/sync-matches
   */
  router.post('/:id/sync-matches', (req, res) => {
    try {
      const { tournamentRegistry, io } = req.app.locals;
      const { id } = req.params;
      const { matches } = req.body;
      
      console.log(`🔄 Match sync request for tournament: ${id} (${matches ? matches.length : 0} matches)`);
      
      const success = tournamentRegistry.syncMatches(id, matches);
      
      if (success) {
        // Broadcast match updates to connected clients
        io.to(`tournament_${id}`).emit('matches-synced', {
          tournamentId: id,
          matchCount: matches ? matches.length : 0,
          timestamp: new Date()
        });

        res.json({
          success: true,
          message: 'Matches synchronized successfully',
          syncedMatches: matches ? matches.length : 0
        });
      } else {
        res.status(404).json({
          success: false,
          error: 'Tournament not found'
        });
      }
    } catch (error) {
      console.error('❌ Match sync error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Synchronize complete tournament with classes (full sync)
   * POST /api/tournaments/:id/sync-full
   */
  router.post('/:id/sync-full', (req, res) => {
    try {
      const { tournamentRegistry, io } = req.app.locals;
      const { id } = req.params;
      const { name, classes, matches, gameRules, syncedAt } = req.body;
      
      console.log(`🎯 Full tournament sync for ${id}: ${matches?.length || 0} matches, ${classes?.length || 0} classes, ${gameRules?.length || 0} game rules`);
      console.log('📚 Classes:', classes?.map(c => `${c.name} (ID: ${c.id}, ${c.playerCount || 0} players, ${c.matchCount || 0} matches)`));
      console.log('🎮 Game Rules:', gameRules?.map(gr => `${gr.name} (${gr.gamePoints} points, ${gr.setsToWin} sets)`));
      
      // Update tournament with class and game rules information
      const tournament = tournamentRegistry.getTournament(id);
      if (tournament) {
        // Update tournament classes with enhanced information
        if (classes && classes.length > 0) {
          tournament.classes = classes.map(cls => ({
            id: cls.id,
            name: cls.name,
            playerCount: cls.playerCount || 0,
            groupCount: cls.groupCount || 0,
            matchCount: cls.matchCount || 0,
            currentPhase: cls.currentPhase || 'GroupPhase',
            gameRules: cls.gameRules || null
          }));
          console.log(`✅ Updated tournament classes: ${classes.map(c => c.name).join(', ')}`);
        }
        
        // Update tournament game rules
        if (gameRules && gameRules.length > 0) {
          tournament.gameRules = gameRules.map(gr => ({
            id: gr.id,
            name: gr.name,
            gamePoints: gr.gamePoints || 501,
            setsToWin: gr.setsToWin || 3,
            legsToWin: gr.legsToWin || 3,
            legsPerSet: gr.legsPerSet || 5,
            maxSets: gr.maxSets || 5,
            maxLegsPerSet: gr.maxLegsPerSet || 5,
            classId: gr.classId || null,
            className: gr.className || null,
            description: gr.description || `${gr.name} - ${gr.gamePoints} Punkte`
          }));
          console.log(`✅ Updated tournament game rules: ${gameRules.map(gr => gr.name).join(', ')}`);
        }
        
        // Update tournament name if provided
        if (name) {
          tournament.name = name;
        }
        
        // Update last sync time
        tournament.lastSync = new Date(syncedAt || new Date());
        tournament.lastHeartbeat = new Date();
      }
      
      // Sync matches with class information (this also handles gameRulesId linking)
      const success = tournamentRegistry.syncMatches(id, matches);
      
      if (success) {
        // Broadcast enhanced sync update to connected clients
        io.to(`tournament_${id}`).emit('tournament-synced', {
          tournamentId: id,
          classCount: classes?.length || 0,
          matchCount: matches?.length || 0,
          gameRulesCount: gameRules?.length || 0,
          timestamp: new Date()
        });

        res.json({
          success: true,
          message: `Full tournament sync completed: ${matches?.length || 0} matches, ${classes?.length || 0} classes, ${gameRules?.length || 0} game rules`,
          data: {
            syncedMatches: matches?.length || 0,
            syncedClasses: classes?.length || 0,
            syncedGameRules: gameRules?.length || 0,
            classNames: classes?.map(c => c.name) || [],
            gameRuleNames: gameRules?.map(gr => gr.name) || []
          }
        });
        
        console.log(`✅ Full tournament sync completed for ${id} - ${gameRules?.length || 0} game rules included`);
      } else {
        res.status(404).json({
          success: false,
          error: 'Tournament not found'
        });
      }
    } catch (error) {
      console.error('❌ Full tournament sync error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Get all tournaments (for dashboard)
   * GET /api/tournaments
   */
  router.get('/', (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const { status, search } = req.query;

      let tournaments = tournamentRegistry.getAllTournaments();

      // Filter by status if provided
      if (status) {
        tournaments = tournaments.filter(t => t.status === status);
      }

      // Search by name or location if provided
      if (search) {
        const searchLower = search.toLowerCase();
        tournaments = tournaments.filter(t => 
          t.name.toLowerCase().includes(searchLower) ||
          t.location.toLowerCase().includes(searchLower)
        );
      }

      // Map to safe public data
      const publicTournaments = tournaments.map(t => ({
        id: t.id,
        name: t.name,
        description: t.description,
        location: t.location,
        status: t.status,
        registeredAt: t.registeredAt,
        lastHeartbeat: t.lastHeartbeat,
        connectedClients: t.connectedClients || 0,
        totalPlayers: t.totalPlayers || 0,
        activeMatches: t.activeMatches || 0
      }));

      res.json({
        success: true,
        data: publicTournaments,
        total: publicTournaments.length
      });
    } catch (error) {
      console.error('❌ Get tournaments error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Get tournament classes
   * GET /api/tournaments/:id/classes
   */
  router.get('/:id/classes', (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const { id } = req.params;

      console.log(`📚 Fetching classes for tournament: ${id}`);

      const classes = tournamentRegistry.getTournamentClasses(id);
      
      console.log(`✅ Found ${classes.length} classes for tournament ${id}`);
      
      res.json({
        success: true,
        data: classes,
        tournamentId: id,
        total: classes.length
      });
    } catch (error) {
      console.error('❌ Get tournament classes error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Get tournament game rules
   * GET /api/tournaments/:id/gamerules
   */
  router.get('/:id/gamerules', (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const { id } = req.params;

      console.log(`🎮 Fetching game rules for tournament: ${id}`);

      const gameRules = tournamentRegistry.getTournamentGameRules(id);
      
      console.log(`✅ Found ${gameRules.length} game rules for tournament ${id}`);
      
      res.json({
        success: true,
        data: gameRules,
        tournamentId: id,
        total: gameRules.length
      });
    } catch (error) {
      console.error('❌ Get tournament game rules error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Get tournament matches (supports class filtering)
   * GET /api/tournaments/:id/matches?classId=1
   */
  router.get('/:id/matches', (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const { id } = req.params;
      const { classId } = req.query;

      console.log(`🎯 Fetching matches for tournament: ${id}${classId ? `, class: ${classId}` : ''}`);
      
      const matches = tournamentRegistry.getTournamentMatches(id, classId);
      
      console.log(`✅ Found ${matches.length} matches for tournament ${id}${classId ? ` (class ${classId})` : ''}`);
      
      res.json({
        success: true,
        data: matches,
        tournamentId: id,
        classId: classId || null,
        total: matches.length
      });
    } catch (error) {
      console.error('❌ Get tournament matches error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Set current class for tournament
   * POST /api/tournaments/:id/class
   */
  router.post('/:id/class', (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const { id } = req.params;
      const { classId } = req.body;
      
      console.log(`🎯 Setting current class for tournament ${id}: ${classId}`);
      
      const success = tournamentRegistry.setCurrentClass(id, classId);
      
      if (success) {
        res.json({
          success: true,
          message: 'Current class set successfully',
          tournamentId: id,
          classId: parseInt(classId)
        });
      } else {
        res.status(404).json({
          success: false,
          error: 'Tournament not found'
        });
      }
    } catch (error) {
      console.error('❌ Set current class error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Unregister tournament
   * DELETE /api/tournaments/:id
   */
  router.delete('/:id', (req, res) => {
    try {
      const { tournamentRegistry, io } = req.app.locals;
      const { id } = req.params;

      console.log(`📴 Tournament unregistration request: ${id}`);

      const success = tournamentRegistry.unregisterTournament(id);
      
      if (success) {
        // Notify connected clients
        io.to(`tournament_${id}`).emit('tournament-unregistered', {
          tournamentId: id,
          message: 'Tournament has been unregistered',
          timestamp: new Date()
        });
        
        res.json({
          success: true,
          message: 'Tournament unregistered successfully'
        });
      } else {
        res.status(404).json({
          success: false,
          error: 'Tournament not found'
        });
      }
    } catch (error) {
      console.error('❌ Tournament unregistration error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Submit match result
   * POST /api/matches/:tournamentId/:matchId/result
   */
  router.post('/matches/:tournamentId/:matchId/result', (req, res) => {
    try {
      const { tournamentRegistry, io } = req.app.locals;
      const { tournamentId, matchId } = req.params;
      const resultData = req.body;
      
      console.log(`🎯 Match result submission: Tournament ${tournamentId}, Match ${matchId}`);
      console.log('📊 Result data:', JSON.stringify(resultData, null, 2));
      
      const tournament = tournamentRegistry.getTournament(tournamentId);
      if (!tournament) {
        return res.status(404).json({
          success: false,
          error: 'Tournament not found'
        });
      }
      
      // Find and update the match
      const matches = tournament.matches || [];
      const matchIndex = matches.findIndex(m => m.matchId == matchId);
      
      if (matchIndex === -1) {
        return res.status(404).json({
          success: false,
          error: 'Match not found'
        });
      }
      
      // Update match with result
      const updatedMatch = {
        ...matches[matchIndex],
        player1Sets: resultData.player1Sets || 0,
        player1Legs: resultData.player1Legs || 0,
        player2Sets: resultData.player2Sets || 0,
        player2Legs: resultData.player2Legs || 0,
        status: resultData.status || 'Finished',
        notes: resultData.notes || '',
        endTime: new Date().toISOString(),
        lastUpdated: new Date().toISOString(), // WICHTIG: lastUpdated setzen
        submittedBy: 'Hub Interface',
        gameRulesUsed: resultData.gameRulesUsed || null
      };
      
      matches[matchIndex] = updatedMatch;
      tournament.matches = matches;
      tournament.lastUpdate = new Date();
      
      console.log(`✅ Match result stored with lastUpdated: ${updatedMatch.lastUpdated}`);
      
      // Broadcast result update to all connected clients
      io.to(`tournament_${tournamentId}`).emit('match-result-updated', {
        tournamentId: tournamentId,
        matchId: matchId,
        result: updatedMatch,
        timestamp: new Date()
      });
      
      // WICHTIG: Sende an Tournament Planner über dediziertes Event
      io.emit('result-submitted-to-planner', {
        tournamentId: tournamentId,
        matchId: matchId,
        result: updatedMatch,
        classId: updatedMatch.classId,
        timestamp: new Date(),
        source: 'Hub-REST-API'
      });
      
      console.log(`✅ Match result updated successfully and broadcast to Tournament Planner: Match ${matchId} in Tournament ${tournamentId}`);
      
      res.json({
        success: true,
        message: 'Match result updated successfully',
        data: updatedMatch
      });
      
    } catch (error) {
      console.error('❌ Match result submission error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  /**
   * Get match details
   * GET /api/matches/:tournamentId/:matchId
   */
  router.get('/matches/:tournamentId/:matchId', (req, res) => {
    try {
      const { tournamentRegistry } = req.app.locals;
      const { tournamentId, matchId } = req.params;
      
      console.log(`🎯 Getting match details: Tournament ${tournamentId}, Match ${matchId}`);
      
      const tournament = tournamentRegistry.getTournament(tournamentId);
      if (!tournament) {
        return res.status(404).json({
          success: false,
          error: 'Tournament not found'
        });
      }
      
      const matches = tournament.matches || [];
      const match = matches.find(m => m.matchId == matchId);
      
      if (!match) {
        return res.status(404).json({
          success: false,
          error: 'Match not found'
        });
      }
      
      res.json({
        success: true,
        data: match
      });
      
    } catch (error) {
      console.error('❌ Get match details error:', error.message);
      res.status(500).json({
        success: false,
        error: 'Internal server error'
      });
    }
  });

  return router;
};