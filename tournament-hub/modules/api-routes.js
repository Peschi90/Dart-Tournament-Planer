// REST API Routes Module
// Handles all HTTP API endpoints

const express = require('express');
const router = express.Router();

function createApiRoutes(tournamentRegistry, matchService, socketIOHandlers, io, websocketHandlers) {
    
    // Health check
    router.get('/health', (req, res) => {
        const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
        console.log(`?? [API] Health check requested from ${clientIP}`);
        
        const healthInfo = {
            status: 'OK',
            server: 'dtp.i3ull3t.de:9443',
            timestamp: new Date().toISOString(),
            websocket: {
                enabled: true,
                connectedClients: io ? io.engine.clientsCount : 0,
                protocol: 'wss'
            },
            ssl: true,
            environment: process.env.NODE_ENV || 'production',
            nodeVersion: process.version,
            uptime: process.uptime(),
            packageVersion: require('../package.json').version
        };
        
        res.json(healthInfo);
    });

    // API Status endpoint (needed for dashboard)
    router.get('/status', (req, res) => {
        const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
        console.log(`?? [API] Status endpoint requested from ${clientIP}`);
        
        const stats = tournamentRegistry.getStatistics();
        const status = {
            status: 'OK',
            server: 'Tournament Hub',
            version: '1.2.0',
            environment: process.env.NODE_ENV || 'production',
            timestamp: new Date().toISOString(),
            uptime: Math.floor(process.uptime()),
            
            // Server info
            serverInfo: {
                domain: 'dtp.i3ull3t.de',
                port: 9443,
                protocol: 'https',
                ssl: true
            },
            
            // WebSocket info
            websocket: {
                enabled: true,
                connectedClients: io ? io.engine.clientsCount : 0,
                protocol: 'wss'
            },
            
            // Tournament statistics
            tournaments: stats,
            
            // System info
            system: {
                nodeVersion: process.version,
                platform: process.platform,
                arch: process.arch,
                memoryUsage: process.memoryUsage()
            }
        };
        
        res.json(status);
    });

    // Statistics endpoint
    router.get('/statistics', (req, res) => {
        const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
        console.log(`?? [API] Statistics requested from ${clientIP}`);
        
        const stats = tournamentRegistry.getStatistics();
        const response = {
            success: true,
            data: {
                ...stats,
                websocket: {
                    connectedClients: io ? io.engine.clientsCount : 0,
                    rooms: io ? Object.keys(io.sockets.adapter.rooms).length : 0
                },
                server: {
                    uptime: Math.floor(process.uptime()),
                    memoryUsage: process.memoryUsage(),
                    nodeVersion: process.version
                }
            },
            meta: {
                requestedAt: new Date().toISOString(),
                server: 'dtp.i3ull3t.de:9443'
            }
        };
        
        res.json(response);
    });

    // WebSocket info endpoint
    router.get('/websocket/info', (req, res) => {
        const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
        console.log(`?? [API] WebSocket info requested from ${clientIP}`);
        
        res.json({
            success: true,
            websocket: {
                // Socket.IO WebSocket (Standard)
                socketIO: {
                    enabled: true,
                    connectedClients: io ? io.engine.clientsCount : 0,
                    protocol: 'wss',
                    url: 'wss://dtp.i3ull3t.de:9443',
                    events: [
                        'subscribe-tournament',
                        'unsubscribe-tournament', 
                        'register-planner',
                        'submit-match-result',
                        'heartbeat'
                    ]
                },
                // Direkter WebSocket (Tournament Planner)
                direct: {
                    enabled: true,
                    protocol: 'wss',
                    url: 'wss://dtp.i3ull3t.de:9444/ws',
                    ssl: true,
                    port: 9444,
                    events: [
                        'subscribe-tournament',
                        'register-planner',
                        'heartbeat'
                    ]
                },
                rooms: io ? Object.keys(io.sockets.adapter.rooms) : [],
                server: 'dtp.i3ull3t.de:9443'
            },
            ssl: {
                enabled: true,
                certificatesFound: true
            },
            environment: 'production'
        });
    });

    // Tournament registration
    router.post('/tournaments/register', async (req, res) => {
        try {
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            console.log(`?? [API] Tournament registration from ${clientIP}:`, req.body.tournamentId);
            
            const { tournamentId, name, description, apiEndpoint, location, apiKey, classes, gameRules, totalPlayers } = req.body;
            
            if (!tournamentId || !name) {
                return res.status(400).json({
                    success: false,
                    message: 'Tournament ID and name are required'
                });
            }

            const registrationData = {
                tournamentId,
                name,
                description,
                apiEndpoint,
                location,
                apiKey,
                classes: classes || [],
                gameRules: gameRules || [],
                totalPlayers: totalPlayers || 0,
                registeredAt: new Date().toISOString(),
                lastHeartbeat: new Date().toISOString(),
                matches: [],
                metadata: {}
            };

            const registrationResult = tournamentRegistry.registerTournament(registrationData);
            
            if (!registrationResult.success) {
                return res.status(400).json({
                    success: false,
                    message: registrationResult.error || 'Tournament registration failed'
                });
            }
            
            const baseUrl = `https://dtp.i3ull3t.de:9443`;
            const joinUrl = `${baseUrl}/join/${tournamentId}`;
            const websocketUrl = `wss://dtp.i3ull3t.de:9443`;

            const response = {
                success: true,
                message: 'Tournament registered successfully',
                data: {
                    tournamentId,
                    hubEndpoint: baseUrl,
                    joinUrl,
                    websocketUrl,
                    registeredAt: registrationData.registeredAt,
                    server: 'dtp.i3ull3t.de:9443',
                    classes: classes?.length || 0,
                    gameRules: gameRules?.length || 0
                }
            };
            
            console.log(`? [API] Tournament registered: ${tournamentId} - Join: ${joinUrl}`);
            
            // Broadcast tournament registration via WebSocket
            if (io) {
                io.emit('tournament-registered', {
                    tournamentId,
                    name,
                    timestamp: new Date().toISOString()
                });
            }
            
            res.json(response);
            
        } catch (error) {
            console.error(`? [API] Tournament registration error:`, error);
            res.status(500).json({
                success: false,
                message: 'Internal server error',
                error: error.message
            });
        }
    });

    // Tournament sync
    router.post('/tournaments/:tournamentId/sync-full', async (req, res) => {
        try {
            const { tournamentId } = req.params;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            console.log(`?? [API] Full tournament sync: ${tournamentId} from ${clientIP}`);
            
            const tournamentData = req.body;
            
            // ERWEITERT: Detailliertes Logging für Game Rules
            console.log(`?? [API] Tournament sync data for ${tournamentId}:`);
            console.log(`   ?? Classes: ${tournamentData.classes?.length || 0}`);
            console.log(`   ?? Game Rules: ${tournamentData.gameRules?.length || 0}`);
            console.log(`   ?? Matches: ${tournamentData.matches?.length || 0}`);
            
            const updated = tournamentRegistry.updateTournament(tournamentId, tournamentData);
            
            if (updated && socketIOHandlers) {
                socketIOHandlers.broadcastTournamentUpdate(tournamentId, {
                    type: 'full-sync',
                    classes: tournamentData.classes?.length || 0,
                    matches: tournamentData.matches?.length || 0,
                    gameRules: tournamentData.gameRules?.length || 0,
                    gameRulesNames: tournamentData.gameRules?.map(gr => gr.name || `Rule ${gr.id}`) || []
                });
                
                console.log(`? [API] Tournament synced successfully: ${tournamentId}`);
                console.log(`?? [API] Game Rules applied: ${tournamentData.gameRules?.length || 0} rules`);
            }
            
            res.json({
                success: updated,
                message: updated ? 'Tournament synced successfully' : 'Failed to sync tournament',
                data: {
                    tournamentId,
                    syncedAt: new Date().toISOString(),
                    websocketBroadcast: updated,
                    server: 'dtp.i3ull3t.de:9443',
                    gameRulesSynced: tournamentData.gameRules?.length || 0,
                    matchesWithRules: tournamentData.matches?.filter(m => m.gameRulesUsed || m.gameRulesId).length || 0
                }
            });
            
        } catch (error) {
            console.error(`? [API] Tournament sync error:`, error);
            res.status(500).json({
                success: false,
                message: 'Sync failed',
                error: error.message
            });
        }
    });

    // Tournament list endpoint (for dashboard)
    router.get('/tournaments', async (req, res) => {
        try {
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            console.log(`?? [API] Tournaments list requested from ${clientIP}`);
            
            const tournaments = tournamentRegistry.getAllTournaments();
            const activeTournaments = tournamentRegistry.getActiveTournaments();
            
            console.log(`? [API] Returning ${tournaments.length} tournaments (${activeTournaments.length} active)`);
            
            const response = {
                success: true,
                data: {
                    tournaments: tournaments.map(t => ({
                        id: t.id,
                        name: t.name,
                        description: t.description,
                        status: t.status || 'active',
                        registeredAt: t.registeredAt,
                        lastHeartbeat: t.lastHeartbeat,
                        lastUpdate: t.lastUpdate,
                        connectedClients: t.connectedClients || 0,
                        totalMatches: (t.matches || []).length,
                        activeMatches: (t.matches || []).filter(m => 
                            (m.status || '').toLowerCase() === 'inprogress' || 
                            (m.Status || '').toLowerCase() === 'inprogress'
                        ).length,
                        classes: (t.classes || []).length,
                        gameRules: (t.gameRules || []).length,
                        totalPlayers: t.totalPlayers || 0,
                        location: t.location || '',
                        // Tournament interface URLs
                        joinUrl: `https://dtp.i3ull3t.de:9443/join/${t.id}`,
                        tournamentUrl: `https://dtp.i3ull3t.de:9443/tournament/${t.id}`
                    })),
                    statistics: {
                        total: tournaments.length,
                        active: activeTournaments.length,
                        totalConnections: io ? io.engine.clientsCount : 0,
                        totalMatches: tournaments.reduce((sum, t) => sum + (t.matches || []).length, 0),
                        totalPlayers: tournaments.reduce((sum, t) => sum + (t.totalPlayers || 0), 0)
                    }
                },
                meta: {
                    requestedAt: new Date().toISOString(),
                    server: 'dtp.i3ull3t.de:9443'
                }
            };
            
            res.json(response);
            
        } catch (error) {
            console.error(`? [API] Tournaments list error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to retrieve tournaments',
                error: error.message
            });
        }
    });

    // Tournament details endpoint
    router.get('/tournaments/:tournamentId', async (req, res) => {
        try {
            const { tournamentId } = req.params;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            console.log(`?? [API] Tournament details requested: ${tournamentId} from ${clientIP}`);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            console.log(`? [API] Returning tournament details: ${tournament.name}`);
            
            const response = {
                success: true,
                data: {
                    tournament: {
                        id: tournament.id,
                        name: tournament.name,
                        description: tournament.description,
                        status: tournament.status || 'active',
                        registeredAt: tournament.registeredAt,
                        lastHeartbeat: tournament.lastHeartbeat,
                        lastUpdate: tournament.lastUpdate,
                        connectedClients: tournament.connectedClients || 0,
                        totalMatches: (tournament.matches || []).length,
                        activeMatches: (tournament.matches || []).filter(m => 
                            (m.status || '').toLowerCase() === 'inprogress' || 
                            (m.Status || '').toLowerCase() === 'inprogress'
                        ).length,
                        totalPlayers: tournament.totalPlayers || 0,
                        location: tournament.location || '',
                        classes: tournament.classes || [],
                        gameRules: tournament.gameRules || [],
                        matches: tournament.matches || [],
                        metadata: tournament.metadata || {},
                        // URLs
                        joinUrl: `https://dtp.i3ull3t.de:9443/join/${tournament.id}`,
                        tournamentUrl: `https://dtp.i3ull3t.de:9443/tournament/${tournament.id}`
                    }
                },
                meta: {
                    requestedAt: new Date().toISOString(),
                    server: 'dtp.i3ull3t.de:9443'
                }
            };
            
            res.json(response);
            
        } catch (error) {
            console.error(`? [API] Tournament details error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to retrieve tournament details',
                error: error.message
            });
        }
    });

    // Tournament classes endpoint
    router.get('/tournaments/:tournamentId/classes', async (req, res) => {
        try {
            const { tournamentId } = req.params;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            console.log(`?? [API] Tournament classes requested: ${tournamentId} from ${clientIP}`);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            const classes = tournament.classes || [];
            const matches = tournament.matches || [];
            const classesWithStats = classes.map(cls => ({
                ...cls,
                matchCount: matches.filter(m => (m.classId || m.ClassId) == cls.id).length,
                playerCount: cls.playerCount || 0,
                groupCount: cls.groupCount || 0
            }));
            
            console.log(`? [API] Returning ${classesWithStats.length} classes with stats`);
            
            res.json({
                success: true,
                data: classesWithStats,
                meta: {
                    tournamentId,
                    totalClasses: classesWithStats.length,
                    requestedAt: new Date().toISOString(),
                    server: 'dtp.i3ull3t.de:9443'
                }
            });
            
        } catch (error) {
            console.error(`? [API] Tournament classes error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to retrieve tournament classes',
                error: error.message
            });
        }
    });

    // Tournament game rules endpoint
    router.get('/tournaments/:tournamentId/gamerules', async (req, res) => {
        try {
            const { tournamentId } = req.params;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            console.log(`?? [API] Tournament game rules requested: ${tournamentId} from ${clientIP}`);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            const gameRules = tournament.gameRules || [];
            
            console.log(`? [API] Returning ${gameRules.length} game rules`);
            
            res.json({
                success: true,
                data: gameRules,
                meta: {
                    tournamentId,
                    totalGameRules: gameRules.length,
                    requestedAt: new Date().toISOString(),
                    server: 'dtp.i3ull3t.de:9443'
                }
            });
            
        } catch (error) {
            console.error(`? [API] Tournament game rules error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to retrieve tournament game rules',
                error: error.message
            });
        }
    });

    // Tournament matches with class filtering
    router.get('/tournaments/:tournamentId/matches', async (req, res) => {
        try {
            const { tournamentId } = req.params;
            const { classId } = req.query;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            
            console.log(`?? [API] Tournament matches requested: ${tournamentId}${classId ? ` for class ${classId}` : ' (all classes)'} from ${clientIP}`);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            let matches = tournament.matches || [];
            
            if (classId) {
                matches = matches.filter(match => 
                    (match.classId || match.ClassId) == classId
                );
                console.log(`?? [API] Filtered to ${matches.length} matches for class ${classId}`);
            }
            
            console.log(`? [API] Returning ${matches.length} matches`);
            
            res.json({
                success: true,
                data: matches,
                meta: {
                    tournamentId,
                    classId: classId || 'all',
                    totalMatches: matches.length,
                    requestedAt: new Date().toISOString(),
                    server: 'dtp.i3ull3t.de:9443'
                }
            });
            
        } catch (error) {
            console.error(`? [API] Tournament matches error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to retrieve tournament matches',
                error: error.message
            });
        }
    });

    // Individual match data endpoint
    router.get('/tournaments/:tournamentId/matches/:matchId', async (req, res) => {
        try {
            const { tournamentId, matchId } = req.params;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            
            console.log(`?? [API] Individual match data requested: ${tournamentId}/${matchId} from ${clientIP}`);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            // UUID-aware match search
            const match = findMatchByIdOrUUID(tournament.matches, matchId);
            
            if (!match) {
                return res.status(404).json({
                    success: false,
                    message: `Match ${matchId} not found`,
                    availableMatches: tournament.matches?.map(m => ({
                        id: m.id || m.matchId,
                        uniqueId: m.uniqueId,
                        player1: m.player1,
                        player2: m.player2
                    })) || []
                });
            }
            
            // Find game rules for this match
            let gameRules = null;
            if (match.gameRulesUsed) {
                gameRules = match.gameRulesUsed;
            } else if (tournament.gameRules) {
                gameRules = tournament.gameRules.find(gr => 
                    (gr.classId || gr.ClassId) === match.classId &&
                    (gr.matchType || 'Group') === (match.matchType || 'Group')
                ) || tournament.gameRules.find(gr => 
                    (gr.classId || gr.ClassId) === match.classId
                ) || tournament.gameRules[0];
            }
            
            // Enhanced match response with UUID support
            const responseMatch = {
                // Primary identification (prefer UUID)
                id: match.uniqueId || match.matchId || match.id,
                matchId: match.matchId || match.id,
                uniqueId: match.uniqueId || null,
                
                // Match data
                player1: match.player1,
                player2: match.player2,
                player1Sets: match.player1Sets || 0,
                player2Sets: match.player2Sets || 0,
                player1Legs: match.player1Legs || 0,
                player2Legs: match.player2Legs || 0,
                status: match.status,
                winner: match.winner,
                notes: match.notes || '',
                
                // Classification
                classId: match.classId,
                className: match.className,
                matchType: match.matchType || 'Group',
                groupId: match.groupId,
                groupName: match.groupName,
                
                // KO-specific (if applicable)
                bracketType: match.bracketType || null,
                round: match.round || null,
                position: match.position || null,
                
                // Game rules integration
                gameRulesId: match.gameRulesId,
                gameRulesUsed: match.gameRulesUsed,
                
                // Timestamps
                syncedAt: match.syncedAt,
                tournamentId: tournamentId
            };
            
            res.json({
                success: true,
                match: responseMatch,
                gameRules: gameRules,
                tournament: {
                    id: tournament.id,
                    name: tournament.name,
                    description: tournament.description
                },
                meta: {
                    requestedAt: new Date().toISOString(),
                    server: `${req.hostname}:${process.env.PORT || 9443}`,
                    matchIdentification: {
                        requestedId: matchId,
                        uniqueId: match.uniqueId || null,
                        numericId: match.matchId || match.id,
                        accessMethod: matchId === match.uniqueId ? 'UUID' : 'numericId'
                    }
                }
            });
            
        } catch (error) {
            console.error(`? [API] Tournament details error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to retrieve tournament details',
                error: error.message
            });
        }
    });

    // Match game rules endpoint
    router.get('/tournaments/:tournamentId/matches/:matchId/rules', async (req, res) => {
        try {
            const { tournamentId, matchId } = req.params;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            
            console.log(`?? [API] Match game rules requested: ${tournamentId}/${matchId} from ${clientIP}`);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            const matches = tournament.matches || [];
            const match = matches.find(m => 
                (m.matchId || m.id || m.Id) == matchId
            );
            
            if (!match) {
                return res.status(404).json({
                    success: false,
                    message: `Match not found: ${matchId}`
                });
            }
            
            const gameRules = tournament.gameRules || [];
            let matchGameRules = null;
            
            // Find game rules for this match
            if (match.gameRulesUsed) {
                matchGameRules = match.gameRulesUsed;
            } else if (match.gameRulesId || match.GameRulesId) {
                matchGameRules = gameRules.find(gr => 
                    (gr.id || gr.Id) == (match.gameRulesId || match.GameRulesId)
                );
            } else {
                // Fallback: use class-based rules
                const classId = match.classId || match.ClassId;
                matchGameRules = gameRules.find(gr => 
                    (gr.classId || gr.ClassId) == classId
                );
            }
            
            if (!matchGameRules) {
                // Create default rules
                matchGameRules = {
                    id: `default_${match.classId || match.ClassId || 1}`,
                    name: 'Standard Rules',
                    gamePoints: 501,
                    gameMode: 'Standard',
                    finishMode: 'DoubleOut',
                    playWithSets: true,
                    setsToWin: 3,
                    legsToWin: 3,
                    legsPerSet: 5
                };
            }
            
            console.log(`? [API] Returning game rules for match ${matchId}: ${matchGameRules.name || 'Default'}`);
            
            res.json({
                success: true,
                data: matchGameRules,
                meta: {
                    matchId,
                    tournamentId,
                    source: match.gameRulesUsed ? 'match-specific' : 'class-default',
                    requestedAt: new Date().toISOString(),
                    server: 'dtp.i3ull3t.de:9443'
                }
            });
            
        } catch (error) {
            console.error(`? [API] Match game rules error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to retrieve match game rules',
                error: error.message
            });
        }
    });

    // Submit match result (enhanced for match page)
    router.post('/tournaments/:tournamentId/matches/:matchId/result', async (req, res) => {
        try {
            const { tournamentId, matchId } = req.params;
            const result = req.body;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            
            console.log(`📤 [API] Match result submission: Tournament ${tournamentId}, Match ${matchId} from ${clientIP}`);
            console.log(`📊 [API] Result data:`, result);
            
            // ERWEITERT: UUID-bewusste Match-Suche
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            const matches = tournament.matches || [];
            
            // Finde Match über UUID oder numerische ID
            const match = matches.find(m => 
                // Priorisiere UniqueId (UUID)
                (m.uniqueId && m.uniqueId === matchId) ||
                // Fallback auf numerische IDs
                (m.matchId || m.id || m.Id) == matchId
            );
            
            if (!match) {
                return res.status(404).json({
                    success: false,
                    message: `Match not found: ${matchId} in tournament ${tournamentId}`
                });
            }
            
            // Erweitere Result mit Match-Identifikationsdaten
            const enhancedResult = {
                ...result,
                // Match-Identifikation
                uniqueId: match.uniqueId,
                matchId: match.matchId || match.id || match.Id,
                submittedVia: 'Hub-API',
                submittedAt: new Date().toISOString(),
                matchType: match.matchType || 'Unknown',
                bracketType: match.bracketType || null,
                // Class-Information
                classId: result.classId || match.classId || 1,
                className: result.className || match.className || 'Unknown Class'
            };
            
            // Verwende UUID für Match Service wenn verfügbar
            const submitMatchId = match.uniqueId || matchId;
            const success = await matchService.submitMatchResult(tournamentId, submitMatchId, enhancedResult);
            
            if (success && socketIOHandlers) {
                // Broadcast mit erweiterten Match-Daten
                const broadcastData = {
                    type: 'match-result-update',
                    tournamentId,
                    matchId: submitMatchId,
                    uniqueId: match.uniqueId,
                    numericMatchId: match.matchId || match.id || match.Id,
                    result: enhancedResult,
                    timestamp: new Date().toISOString(),
                    matchType: match.matchType || 'Unknown',
                    bracketType: match.bracketType || null,
                    classId: enhancedResult.classId,
                    className: enhancedResult.className
                };
                
                // Broadcast to tournament interface
                socketIOHandlers.broadcastTournamentUpdate(tournamentId, broadcastData);
                
                // 🚨 KORRIGIERT: Auch WebSocket-Direct Broadcasting aufrufen!
                if (websocketHandlers) {
                    console.log(`📡 [API] Triggering WebSocket-Direct broadcast for Tournament Planner`);
                    websocketHandlers.broadcastMatchUpdate(tournamentId, submitMatchId, enhancedResult);
                }
                
                // Broadcast to specific match room (beide ID-Typen)
                if (io) {
                    // UUID-basierte Räume
                    if (match.uniqueId) {
                        const uuidMatchRoom = `match_${tournamentId}_${match.uniqueId}`;
                        io.to(uuidMatchRoom).emit('match-updated', {
                            success: true,
                            match: enhancedResult,
                            uniqueId: match.uniqueId,
                            updatedAt: new Date().toISOString()
                        });
                        console.log(`📤 [API] Match update broadcasted to UUID room: ${uuidMatchRoom}`);
                    }
                    
                    // Legacy numerische ID-Räume
                    const numericMatchRoom = `match_${tournamentId}_${match.matchId || match.id || match.Id}`;
                    io.to(numericMatchRoom).emit('match-updated', {
                        success: true,
                        match: enhancedResult,
                        matchId: match.matchId || match.id || match.Id,
                        updatedAt: new Date().toISOString()
                    });
                    console.log(`📤 [API] Match update broadcasted to numeric room: ${numericMatchRoom}`);
                }
                
                console.log(`✅ [API] Match result submitted and broadcasted: ${tournamentId}/${submitMatchId} (UUID: ${match.uniqueId || 'none'})`);
                
                res.json({
                    success: true,
                    message: 'Match result submitted successfully',
                    data: {
                        tournamentId,
                        matchId: submitMatchId,
                        uniqueId: match.uniqueId,
                        numericMatchId: match.matchId || match.id || match.Id,
                        submittedAt: new Date().toISOString(),
                        websocketBroadcast: true,
                        server: 'dtp.i3ull3t.de:9443'
                    }
                });
            } else {
                res.status(400).json({
                    success: false,
                    message: 'Failed to submit match result'
                });
            }
            
        } catch (error) {
            console.error(`❌ [API] Match result submission error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to submit match result',
                error: error.message
            });
        }
    });

    // Submit dart scoring match result
    router.post('/match/:tournamentId/:matchId/result', async (req, res) => {
        try {
            const { tournamentId, matchId } = req.params;
            const result = req.body;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            
            console.log(`🎯 [API] Dart scoring result submission: Tournament ${tournamentId}, Match ${matchId} from ${clientIP}`);
            console.log(`📊 [API] Dart scoring result:`, result);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            const matches = tournament.matches || [];
            
            // Find match via UUID or numeric ID
            const match = matches.find(m => 
                // Prioritize UniqueId (UUID)
                (m.uniqueId && m.uniqueId === matchId) ||
                // Fallback to numeric IDs
                (m.matchId || m.id || m.Id) == matchId
            );
            
            if (!match) {
                return res.status(404).json({
                    success: false,
                    message: `Match not found: ${matchId} in tournament ${tournamentId}`
                });
            }
            
            // Enhanced result with dart scoring metadata
            const enhancedResult = {
                ...result,
                // Match identification
                uniqueId: match.uniqueId,
                matchId: match.matchId || match.id || match.Id,
                submittedVia: 'Dart-Scoring',
                submittedAt: new Date().toISOString(),
                matchType: match.matchType || 'Unknown',
                bracketType: match.bracketType || null,
                // Class information
                classId: result.classId || match.classId || 1,
                className: result.className || match.className || 'Unknown Class',
                // Mark as finished
                status: 'finished'
            };
            
            // Use UUID for Match Service if available
            const submitMatchId = match.uniqueId || matchId;
            const success = await matchService.submitMatchResult(tournamentId, submitMatchId, enhancedResult);
            
            if (success && socketIOHandlers) {
                // Broadcast with enhanced match data
                const broadcastData = {
                    type: 'dart-scoring-result',
                    tournamentId,
                    matchId: submitMatchId,
                    uniqueId: match.uniqueId,
                    numericMatchId: match.matchId || match.id || match.Id,
                    result: enhancedResult,
                    timestamp: new Date().toISOString(),
                    matchType: match.matchType || 'Unknown',
                    bracketType: match.bracketType || null,
                    classId: enhancedResult.classId,
                    className: enhancedResult.className
                };
                
                // Broadcast to tournament interface
                socketIOHandlers.broadcastTournamentUpdate(tournamentId, broadcastData);
                
                // Auch WebSocket-Direct Broadcasting auslösen
                if (websocketHandlers) {
                    console.log(`📡 [API] Triggering WebSocket-Direct broadcast for dart scoring result`);
                    websocketHandlers.broadcastMatchUpdate(tournamentId, submitMatchId, enhancedResult);
                }
                
                // Broadcast to specific match room
                if (io) {
                    // UUID-based rooms
                    if (match.uniqueId) {
                        const uuidMatchRoom = `match_${tournamentId}_${match.uniqueId}`;
                        io.to(uuidMatchRoom).emit('match-updated', {
                            success: true,
                            match: enhancedResult,
                            uniqueId: match.uniqueId,
                            source: 'dart-scoring',
                            updatedAt: new Date().toISOString()
                        });
                        console.log(`📤 [API] Dart scoring result broadcasted to UUID room: ${uuidMatchRoom}`);
                    }
                    
                    // Legacy numeric ID rooms
                    const numericMatchRoom = `match_${tournamentId}_${match.matchId || match.id || match.Id}`;
                    io.to(numericMatchRoom).emit('match-updated', {
                        success: true,
                        match: enhancedResult,
                        matchId: match.matchId || match.id || match.Id,
                        source: 'dart-scoring',
                        updatedAt: new Date().toISOString()
                    });
                    console.log(`📤 [API] Dart scoring result broadcasted to numeric room: ${numericMatchRoom}`);
                }
                
                console.log(`✅ [API] Dart scoring result submitted and broadcasted: ${tournamentId}/${submitMatchId}`);
                
                res.json({
                    success: true,
                    message: 'Dart scoring result submitted successfully',
                    data: {
                        tournamentId,
                        matchId: submitMatchId,
                        uniqueId: match.uniqueId,
                        numericMatchId: match.matchId || match.id || match.Id,
                        submittedAt: new Date().toISOString(),
                        websocketBroadcast: true,
                        server: 'dtp.i3ull3t.de:9443'
                    }
                });
            } else {
                res.status(400).json({
                    success: false,
                    message: 'Failed to submit dart scoring result'
                });
            }
            
        } catch (error) {
            console.error(`❌ [API] Dart scoring result submission error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to submit dart scoring result',
                error: error.message
            });
        }
    });

    // Match access validation endpoint
    router.get('/tournaments/:tournamentId/matches/:matchId/access', async (req, res) => {
        try {
            const { tournamentId, matchId } = req.params;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            
            console.log(`?? [API] Match access validation: ${tournamentId}/${matchId} from ${clientIP}`);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            // UUID-aware match search
            const match = findMatchByIdOrUUID(tournament.matches, matchId);
            
            if (!match) {
                return res.status(404).json({
                    success: false,
                    message: `Match ${matchId} not found`,
                    searchedFor: matchId,
                    availableMatches: tournament.matches?.length || 0
                });
            }
            
            res.json({
                success: true,
                hasAccess: true,
                match: {
                    id: match.uniqueId || match.matchId || match.id,
                    uniqueId: match.uniqueId,
                    numericId: match.matchId || match.id,
                    tournamentId: tournamentId,
                    accessedVia: matchId === match.uniqueId ? 'UUID' : 'numericId'
                },
                message: `Access granted to match ${matchId}`
            });
        } catch (error) {
            console.error('?? [API] Match access error:', error);
            res.status(500).json({
                success: false,
                message: 'Internal server error during match access validation'
            });
        }
    });

    // Individual match data endpoint with UUID support for dart scoring
    router.get('/match/:tournamentId/:matchId', async (req, res) => {
        try {
            const { tournamentId, matchId } = req.params;
            const { uuid } = req.query;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            
            console.log(`🎯 [API] Match data for dart scoring: ${tournamentId}/${matchId}${uuid ? ' (UUID mode)' : ''} from ${clientIP}`);
            
            const tournament = tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                return res.status(404).json({
                    success: false,
                    message: `Tournament not found: ${tournamentId}`
                });
            }
            
            // UUID-aware match search
            const match = findMatchByIdOrUUID(tournament.matches, matchId);
            
            if (!match) {
                return res.status(404).json({
                    success: false,
                    message: `Match ${matchId} not found`,
                    availableMatches: tournament.matches?.map(m => ({
                        id: m.id || m.matchId,
                        uniqueId: m.uniqueId,
                        player1: m.player1,
                        player2: m.player2
                    })) || []
                });
            }
            
            // Find game rules for this match
            let gameRules = null;
            if (match.gameRulesUsed) {
                gameRules = match.gameRulesUsed;
                console.log(`🎮 [API] Using match-specific game rules: ${gameRules.name || 'Unnamed'}`);
            } else if (tournament.gameRules) {
                // Try to find rules by class and match type
                gameRules = tournament.gameRules.find(gr => {
                    const classMatch = (gr.classId || gr.ClassId) === match.classId;
                    const typeMatch = (gr.matchType || 'Group') === (match.matchType || 'Group');
                    return classMatch && typeMatch;
                });
                
                // Fallback to any rules for this class
                if (!gameRules) {
                    gameRules = tournament.gameRules.find(gr => 
                        (gr.classId || gr.ClassId) === match.classId
                    );
                }
                
                // Final fallback to first available rules
                if (!gameRules && tournament.gameRules.length > 0) {
                    gameRules = tournament.gameRules[0];
                }
                
                console.log(`🎮 [API] Using tournament game rules: ${gameRules?.name || 'Default'}`);
            }
            
            // Create default game rules if none found
            if (!gameRules) {
                gameRules = {
                    id: `default_${match.classId || 1}`,
                    name: 'Standard Rules',
                    gamePoints: 501,
                    gameMode: 'Game501',
                    finishMode: 'DoubleOut',
                    playWithSets: true,
                    setsToWin: 3,
                    legsToWin: 3,
                    legsPerSet: 5,
                    doubleOut: true
                };
                console.log(`🎮 [API] Using default game rules for match ${matchId}`);
            }
            
            // Enhanced match response for dart scoring
            const responseMatch = {
                // Primary identification (prefer UUID)
                id: match.uniqueId || match.matchId || match.id,
                matchId: match.matchId || match.id,
                uniqueId: match.uniqueId || null,
                
                // Tournament info
                tournamentId: tournamentId,
                
                // Display name for dart scoring
                displayName: `${match.player1 || 'Spieler 1'} vs ${match.player2 || 'Spieler 2'}`,
                
                // Match data
                player1: { 
                    name: match.player1 || 'Spieler 1',
                    id: match.player1Id || 1
                },
                player2: { 
                    name: match.player2 || 'Spieler 2',
                    id: match.player2Id || 2
                },
                player1Sets: match.player1Sets || 0,
                player2Sets: match.player2Sets || 0,
                player1Legs: match.player1Legs || 0,
                player2Legs: match.player2Legs || 0,
                status: match.status || 'notstarted',
                winner: match.winner,
                notes: match.notes || '',
                
                // Classification
                classId: match.classId,
                className: match.className,
                matchType: match.matchType || 'Group',
                groupId: match.groupId,
                groupName: match.groupName,
                
                // KO-specific (if applicable)
                bracketType: match.bracketType || null,
                round: match.round || null,
                position: match.position || null,
                
                // Game rules integration
                gameRulesId: match.gameRulesId,
                gameRulesUsed: match.gameRulesUsed,
                
                // Timestamps
                syncedAt: match.syncedAt,
                startTime: match.startTime,
                endTime: match.endTime
            };
            
            console.log(`✅ [API] Returning match data for dart scoring: ${responseMatch.displayName}`);
            
            res.json({
                success: true,
                match: responseMatch,
                gameRules: gameRules,
                tournament: {
                    id: tournament.id,
                    name: tournament.name,
                    description: tournament.description
                },
                meta: {
                    requestedAt: new Date().toISOString(),
                    server: `dtp.i3ull3t.de:9443`,
                    matchIdentification: {
                        requestedId: matchId,
                        uniqueId: match.uniqueId || null,
                        numericId: match.matchId || match.id,
                        accessMethod: matchId === match.uniqueId ? 'UUID' : 'numericId'
                    },
                    uuidMode: uuid === 'true'
                }
            });
            
        } catch (error) {
            console.error('❌ [API] Get match data for dart scoring error:', error);
            res.status(500).json({
                success: false,
                message: 'Internal server error',
                error: error.message
            });
        }
    });

    /**
     * UUID-aware match search function
     * Searches for a match by UUID first, then by numeric ID
     */
    function findMatchByIdOrUUID(matches, searchId) {
        if (!matches || !Array.isArray(matches)) {
            return null;
        }
        
        console.log(`?? [API] Searching for match with ID: ${searchId}`);
        console.log(`?? [API] Available matches: ${matches.length}`);
        
        // Priority 1: Search by UUID (exact match)
        const byUuid = matches.find(m => m.uniqueId === searchId);
        if (byUuid) {
            console.log(`? [API] Match found by UUID: ${searchId}`);
            return byUuid;
        }
        
        // Priority 2: Search by numeric ID (convert to number for comparison)
        const searchNumeric = parseInt(searchId);
        if (!isNaN(searchNumeric)) {
            const byNumeric = matches.find(m => {
                const mId = parseInt(m.matchId || m.id || m.Id);
                return mId === searchNumeric;
            });
            
            if (byNumeric) {
                console.log(`? [API] Match found by numeric ID: ${searchId} -> ${byNumeric.matchId || byNumeric.id}`);
                return byNumeric;
            }
        }
        
        // Priority 3: String-based ID search (fallback)
        const byStringId = matches.find(m => {
            const mId = String(m.matchId || m.id || m.Id);
            return mId === String(searchId);
        });
        
        if (byStringId) {
            console.log(`? [API] Match found by string ID: ${searchId}`);
            return byStringId;
        }
        
        console.log(`? [API] Match not found for ID: ${searchId}`);
        console.log('?? [API] Available match IDs:', matches.map(m => ({
            numeric: m.matchId || m.id,
            uuid: m.uniqueId,
            player1: m.player1,
            player2: m.player2
        })));
        
        return null;
    }

    return router;
}

module.exports = createApiRoutes;