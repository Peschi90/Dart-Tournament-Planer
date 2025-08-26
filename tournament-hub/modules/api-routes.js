// REST API Routes Module
// Handles all HTTP API endpoints

const express = require('express');
const router = express.Router();

function createApiRoutes(tournamentRegistry, matchService, socketIOHandlers, io) {
    
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

    // Submit match result
    router.post('/matches/:tournamentId/:matchId/result', async (req, res) => {
        try {
            const { tournamentId, matchId } = req.params;
            const result = req.body;
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            
            console.log(`?? [API] Match result submission: Tournament ${tournamentId}, Match ${matchId} from ${clientIP}`);
            
            const success = await matchService.submitMatchResult(tournamentId, matchId, result);
            
            if (success && socketIOHandlers) {
                socketIOHandlers.broadcastMatchUpdate(tournamentId, matchId, result);
                
                console.log(`? [API] Match result submitted and broadcasted: ${tournamentId}/${matchId}`);
                res.json({
                    success: true,
                    message: 'Match result submitted successfully',
                    data: {
                        tournamentId,
                        matchId,
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
            console.error(`? [API] Match result submission error:`, error);
            res.status(500).json({
                success: false,
                message: 'Failed to submit match result',
                error: error.message
            });
        }
    });

    // Tournament list
    router.get('/tournaments', async (req, res) => {
        try {
            const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
            console.log(`?? [API] Tournaments list requested from ${clientIP}`);
            
            const tournaments = tournamentRegistry.getAllTournaments();
            const activeTournaments = tournamentRegistry.getActiveTournaments();
            
            const response = {
                success: true,
                data: {
                    tournaments: tournaments.map(t => ({
                        id: t.id,
                        name: t.name,
                        description: t.description,
                        status: t.status,
                        registeredAt: t.registeredAt,
                        lastHeartbeat: t.lastHeartbeat,
                        connectedClients: t.connectedClients || 0,
                        totalMatches: t.totalMatches || 0,
                        activeMatches: t.activeMatches || 0,
                        classes: t.classes?.length || 0,
                        gameRules: t.gameRules?.length || 0,
                        location: t.location
                    })),
                    statistics: {
                        total: tournaments.length,
                        active: activeTournaments.length,
                        totalConnections: io ? io.engine.clientsCount : 0
                    }
                },
                meta: {
                    requestedAt: new Date().toISOString(),
                    server: 'dtp.i3ull3t.de:9443'
                }
            };
            
            console.log(`? [API] Tournaments list: ${tournaments.length} total, ${activeTournaments.length} active`);
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

    // Tournament details
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
            
            const response = {
                success: true,
                data: {
                    tournament: {
                        id: tournament.id,
                        name: tournament.name,
                        description: tournament.description,
                        status: tournament.status,
                        registeredAt: tournament.registeredAt,
                        lastHeartbeat: tournament.lastHeartbeat,
                        lastUpdate: tournament.lastUpdate,
                        connectedClients: tournament.connectedClients || 0,
                        totalMatches: tournament.totalMatches || 0,
                        activeMatches: tournament.activeMatches || 0,
                        totalPlayers: tournament.totalPlayers || 0,
                        location: tournament.location,
                        classes: tournament.classes || [],
                        gameRules: tournament.gameRules || [],
                        matches: tournament.matches || [],
                        metadata: tournament.metadata || {}
                    }
                },
                meta: {
                    requestedAt: new Date().toISOString(),
                    server: 'dtp.i3ull3t.de:9443'
                }
            };
            
            console.log(`? [API] Tournament details: ${tournament.name} (${tournament.matches?.length || 0} matches)`);
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
            
            console.log(`?? [API] Returning ${matches.length} matches`);
            
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

    return router;
}

module.exports = createApiRoutes;