// Socket.IO Event Handlers Module
// Handles Socket.IO events and room management

class SocketIOHandlers {
    constructor(io, tournamentRegistry, matchService, websocketHandlers) {
        this.io = io;
        this.tournamentRegistry = tournamentRegistry;
        this.matchService = matchService;
        this.websocketHandlers = websocketHandlers;
        this.setupEventHandlers();
    }

    setupEventHandlers() {
        this.io.on('connection', (socket) => {
            const clientIP = socket.handshake.headers['x-forwarded-for'] || socket.handshake.address || 'unknown';
            console.log(`?? [Socket.IO] Client connected: ${socket.id} from ${clientIP}`);
            
            // Tournament subscription
            socket.on('subscribe-tournament', (tournamentId) => {
                this.handleTournamentSubscription(socket, tournamentId, clientIP);
            });
            
            // Tournament unsubscription
            socket.on('unsubscribe-tournament', (tournamentId) => {
                this.handleTournamentUnsubscription(socket, tournamentId);
            });
            
            // Join tournament (for tournament interface)
            socket.on('join-tournament', (data) => {
                this.handleJoinTournament(socket, data);
            });
            
            // Join match room (for individual match pages)
            socket.on('join-match-room', (data) => {
                this.handleJoinMatchRoom(socket, data);
            });
            
            // Get match data (for match pages)
            socket.on('get-match-data', (data) => {
                this.handleGetMatchData(socket, data);
            });
            
            // Get tournament matches
            socket.on('get-tournament-matches', (data) => {
                this.handleGetTournamentMatches(socket, data);
            });
            
            // Planner registration
            socket.on('register-planner', (data) => {
                this.handlePlannerRegistration(socket, data);
            });
            
            // Match result submission
            socket.on('submit-match-result', async (data) => {
                await this.handleMatchResultSubmission(socket, data);
            });
            
            // Match result submission with acknowledgment (for match pages)
            socket.on('submit-match-result', async (data, callback) => {
                await this.handleMatchResultSubmissionWithCallback(socket, data, callback);
            });
            
            // Heartbeat
            socket.on('heartbeat', (data) => {
                this.handleHeartbeat(socket, data);
            });
            
            // Disconnection
            socket.on('disconnect', (reason) => {
                this.handleDisconnection(socket, reason);
            });
        });
    }

    handleJoinTournament(socket, data) {
        const { tournamentId } = data;
        console.log(`?? [Socket.IO] Join tournament request: ${tournamentId} from ${socket.id}`);
        
        const tournament = this.tournamentRegistry.getTournament(tournamentId);
        
        if (!tournament) {
            console.log(`? [Socket.IO] Tournament not found: ${tournamentId}`);
            socket.emit('tournament-joined', {
                success: false,
                error: `Tournament ${tournamentId} not found`
            });
            return;
        }
        
        // Join the tournament room
        socket.join(`tournament-${tournamentId}`);
        
        // Store tournament ID on socket
        socket.tournamentId = tournamentId;
        
        // Get matches for this tournament
        const matches = tournament.matches || [];
        const classes = tournament.classes || [];
        const gameRules = tournament.gameRules || [];
        
        console.log(`? [Socket.IO] Client joined tournament ${tournamentId}: ${matches.length} matches, ${classes.length} classes`);
        
        socket.emit('tournament-joined', {
            success: true,
            tournament: {
                id: tournament.id,
                name: tournament.name,
                description: tournament.description,
                location: tournament.location,
                classes: classes,
                gameRules: gameRules,
                totalMatches: matches.length,
                totalPlayers: tournament.totalPlayers || 0
            },
            matches: matches
        });
    }

    handleGetTournamentMatches(socket, data) {
        const { tournamentId, classId } = data;
        console.log(`?? [Socket.IO] Get tournament matches: ${tournamentId}, class: ${classId || 'All'}`);
        
        const tournament = this.tournamentRegistry.getTournament(tournamentId);
        
        if (!tournament) {
            socket.emit('tournament-matches', {
                success: false,
                error: `Tournament ${tournamentId} not found`
            });
            return;
        }
        
        let matches = tournament.matches || [];
        
        // Filter by class if specified
        if (classId) {
            matches = matches.filter(match => 
                (match.classId || match.ClassId) == classId
            );
        }
        
        console.log(`?? [Socket.IO] Sending ${matches.length} matches for tournament ${tournamentId}`);
        
        socket.emit('tournament-matches', {
            success: true,
            tournamentId: tournamentId,
            classId: classId || 'all',
            matches: matches,
            total: matches.length
        });
    }

    handleTournamentSubscription(socket, tournamentId, clientIP) {
        console.log(`?? [Socket.IO] Client ${socket.id} subscribing to tournament: ${tournamentId}`);
        
        socket.join(`tournament-${tournamentId}`);
        
        const tournament = this.tournamentRegistry.getTournament(tournamentId);
        
        const confirmationData = {
            tournamentId,
            message: 'Successfully subscribed to tournament updates',
            timestamp: new Date().toISOString(),
            tournament: tournament ? {
                id: tournament.id,
                name: tournament.name,
                classes: tournament.classes,
                gameRules: tournament.gameRules,
                matches: tournament.matches
            } : null
        };
        
        socket.emit('subscription-confirmed', confirmationData);
        console.log(`? [Socket.IO] Client ${socket.id} subscribed to tournament-${tournamentId}`);
    }

    handleTournamentUnsubscription(socket, tournamentId) {
        console.log(`?? [Socket.IO] Client ${socket.id} unsubscribing from tournament: ${tournamentId}`);
        socket.leave(`tournament-${tournamentId}`);
        
        socket.emit('unsubscription-confirmed', {
            tournamentId,
            message: 'Successfully unsubscribed from tournament updates',
            timestamp: new Date().toISOString()
        });
    }

    handlePlannerRegistration(socket, data) {
        try {
            console.log(`?? [Socket.IO] Tournament Planner registration from ${socket.id}`);
            
            const { tournamentId, plannerInfo } = data;
            
            socket.tournamentId = tournamentId;
            socket.isPlannerClient = true;
            socket.plannerInfo = plannerInfo;
            
            socket.join(`tournament-${tournamentId}`);
            socket.join(`planner-${tournamentId}`);
            
            const confirmationData = {
                success: true,
                tournamentId,
                message: 'Tournament Planner successfully registered for updates',
                timestamp: new Date().toISOString()
            };
            
            socket.emit('planner-registration-confirmed', confirmationData);
            console.log(`? [Socket.IO] Tournament Planner registered for ${tournamentId}`);
            
        } catch (error) {
            console.error(`? [Socket.IO] Planner registration error:`, error);
            socket.emit('planner-registration-error', {
                success: false,
                error: error.message
            });
        }
    }

    async handleMatchResultSubmission(socket, data) {
        try {
            console.log(`?? [Socket.IO] ===== MATCH RESULT SUBMISSION =====`);
            console.log(`?? [Socket.IO] Client: ${socket.id}`);
            console.log(`?? [Socket.IO] Data:`, JSON.stringify(data, null, 2));
            
            const { tournamentId, matchId, result, classId, className } = data;
            
            // KORRIGIERT: Class-Information aus verschiedenen Quellen extrahieren
            let finalClassId = classId || result?.classId || 1;
            let finalClassName = className || result?.className || 'Unbekannte Klasse';
            
            // ERWEITERT: Wenn Class-Info fehlt, aus Tournament-Daten holen
            if ((!finalClassId || finalClassId === 1) && !finalClassName.includes('Klasse')) {
                const tournament = this.tournamentRegistry.getTournament(tournamentId);
                if (tournament && tournament.matches) {
                    const originalMatch = tournament.matches.find(m => 
                        String(m.id) === String(matchId) || 
                        String(m.matchId) === String(matchId)
                    );
                    
                    if (originalMatch) {
                        finalClassId = originalMatch.classId || originalMatch.ClassId || finalClassId;
                        finalClassName = originalMatch.className || originalMatch.ClassName || finalClassName;
                        
                        console.log(`?? [Socket.IO] Found match in tournament data: Class ${finalClassName} (ID: ${finalClassId})`);
                    }
                }
            }
            
            // KORRIGIERT: Erweitere Result-Objekt mit Class-Information
            const enhancedResult = {
                ...result,
                classId: finalClassId,
                className: finalClassName,
                submittedVia: 'Socket.IO',
                submittedAt: new Date().toISOString()
            };
            
            console.log(`?? [Socket.IO] Enhanced result with class info:`, {
                originalClassId: classId,
                originalClassName: className,
                resultClassId: result?.classId,
                resultClassName: result?.className,
                finalClassId: finalClassId,
                finalClassName: finalClassName
            });
            
            const success = await this.matchService.submitMatchResult(tournamentId, matchId, enhancedResult);
            
            if (success) {
                const broadcastData = {
                    type: 'match-result-update',
                    tournamentId,
                    matchId,
                    result: {
                        ...enhancedResult,
                        updatedAt: new Date().toISOString(),
                        source: 'socket-io'
                    },
                    timestamp: new Date().toISOString(),
                    // KORRIGIERT: Class-Information auf Top-Level
                    classId: finalClassId,
                    className: finalClassName,
                    matchResultHighlight: true
                };
                
                console.log(`?? [Socket.IO] Broadcasting match update for ${finalClassName} (Class ID: ${finalClassId})`);
                
                // Broadcast to tournament subscribers
                this.io.to(`tournament-${tournamentId}`).emit('tournament-match-updated', broadcastData);
                this.io.to(`tournament-${tournamentId}`).emit('match-result-updated', broadcastData);
                
                // Broadcast to specific match room (for match pages)
                const matchRoom = `match_${tournamentId}_${matchId}`;
                this.io.to(matchRoom).emit('match-updated', {
                    success: true,
                    match: {
                        ...enhancedResult,
                        id: matchId,
                        status: 'finished',
                        updatedAt: new Date().toISOString()
                    },
                    timestamp: new Date().toISOString()
                });
                
                console.log(`?? [Socket.IO] Match update sent to room: ${matchRoom}`);
                
                // Special broadcast to planner clients with class information
                const plannerBroadcastData = {
                    ...broadcastData,
                    plannerSpecific: true,
                    requiresUIUpdate: true,
                    classId: finalClassId,
                    className: finalClassName
                };
                
                this.io.to(`planner-${tournamentId}`).emit('planner-match-updated', plannerBroadcastData);
                
                // Also broadcast via direct WebSocket if available
                if (this.websocketHandlers) {
                    // KORRIGIERT: Übergebe erweiterte Result-Daten mit Class-Info
                    this.websocketHandlers.broadcastMatchUpdate(tournamentId, matchId, enhancedResult);
                }
                
                // Confirm to submitter with class information
                const confirmationData = {
                    success: true,
                    tournamentId,
                    matchId,
                    classId: finalClassId,
                    className: finalClassName,
                    message: `Match result submitted and broadcasted successfully for ${finalClassName}`,
                    timestamp: new Date().toISOString()
                };
                
                socket.emit('result-submitted', confirmationData);
                
                console.log(`? [Socket.IO] Match result submitted and broadcasted: ${tournamentId}/${matchId} in ${finalClassName}`);
                
                // Return result for callback handling
                return {
                    success: true,
                    message: `Match result submitted successfully for ${finalClassName}`,
                    data: confirmationData
                };
                
            } else {
                const errorData = {
                    success: false,
                    tournamentId,
                    matchId,
                    error: 'Failed to submit match result'
                };
                
                socket.emit('match-result-error', errorData);
                
                return {
                    success: false,
                    message: 'Failed to submit match result',
                    data: errorData
                };
            }
        } catch (error) {
            console.error(`? [Socket.IO] Error in match result submission:`, error);
            
            const errorData = {
                success: false,
                error: error.message,
                timestamp: new Date().toISOString()
            };
            
            socket.emit('match-result-error', errorData);
            
            return {
                success: false,
                message: error.message,
                data: errorData
            };
        }
    }

    // Enhanced match result submission with callback (for match pages)
    async handleMatchResultSubmissionWithCallback(socket, data, callback) {
        try {
            console.log(`?? [Socket.IO] Match result submission with callback from ${socket.id}`);
            console.log(`?? [Socket.IO] Data:`, data);
            
            const result = await this.handleMatchResultSubmission(socket, data);
            
            // Call the callback function if provided
            if (typeof callback === 'function') {
                callback({
                    success: result.success || false,
                    message: result.message,
                    data: result.data,
                    timestamp: new Date().toISOString()
                });
            }
            
            // Also emit specific event for match pages
            socket.emit('match-result-submitted', {
                success: result.success || false,
                message: result.message,
                data: result.data,
                timestamp: new Date().toISOString()
            });
            
        } catch (error) {
            console.error(`? [Socket.IO] Error in match result submission with callback:`, error);
            
            const errorResponse = {
                success: false,
                message: error.message,
                timestamp: new Date().toISOString()
            };
            
            if (typeof callback === 'function') {
                callback(errorResponse);
            }
            
            socket.emit('match-result-submitted', errorResponse);
        }
    }

    handleHeartbeat(socket, data) {
        const heartbeatResponse = {
            timestamp: new Date().toISOString(),
            message: 'Heartbeat acknowledged'
        };
        
        if (socket.isPlannerClient) {
            heartbeatResponse.plannerStatus = 'connected';
            heartbeatResponse.tournamentId = socket.tournamentId;
        }
        
        socket.emit('heartbeat-ack', heartbeatResponse);
    }

    handleDisconnection(socket, reason) {
        console.log(`?? [Socket.IO] Client disconnected: ${socket.id}, Reason: ${reason}`);
        
        if (socket.isPlannerClient) {
            console.log(`?? [Socket.IO] Tournament Planner disconnected from ${socket.tournamentId}`);
            
            if (socket.tournamentId) {
                this.io.to(`tournament-${socket.tournamentId}`).emit('planner-disconnected', {
                    tournamentId: socket.tournamentId,
                    timestamp: new Date().toISOString(),
                    reason: reason
                });
            }
        }
    }

    // Join match room for individual match pages
    handleJoinMatchRoom(socket, data) {
        try {
            const { tournamentId, matchId, type } = data;
            console.log(`?? [Socket.IO] Join match room request: ${tournamentId}/${matchId} type: ${type} from ${socket.id}`);
            
            const tournament = this.tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                console.log(`? [Socket.IO] Tournament not found for match room: ${tournamentId}`);
                socket.emit('match-room-error', {
                    success: false,
                    error: `Tournament ${tournamentId} not found`
                });
                return;
            }
            
            const matches = tournament.matches || [];
            
            // ERWEITERT: UUID-bewusste Match-Suche
            const match = matches.find(m => 
                // Priorisiere UniqueId (UUID)
                (m.uniqueId && m.uniqueId === matchId) ||
                // Fallback auf numerische IDs
                (m.matchId || m.id || m.Id) == matchId
            );
            
            if (!match) {
                console.log(`? [Socket.IO] Match not found: ${matchId} in tournament ${tournamentId}`);
                socket.emit('match-room-error', {
                    success: false,
                    error: `Match ${matchId} not found`
                });
                return;
            }
            
            // Join match-specific rooms (beide ID-Typen für Compatibility)
            const rooms = [];
            
            // UUID-basierte Räume (bevorzugt)
            if (match.uniqueId) {
                const uuidMatchRoom = `match_${tournamentId}_${match.uniqueId}`;
                socket.join(uuidMatchRoom);
                rooms.push(uuidMatchRoom);
            }
            
            // Legacy numerische ID-Räume (Backwards-Compatibility)
            const numericMatchRoom = `match_${tournamentId}_${match.matchId || match.id || match.Id}`;
            socket.join(numericMatchRoom);
            rooms.push(numericMatchRoom);
            
            // Also join tournament room for updates
            socket.join(`tournament-${tournamentId}`);
            
            // Store match info on socket
            socket.matchId = matchId;
            socket.matchUniqueId = match.uniqueId;
            socket.matchNumericId = match.matchId || match.id || match.Id;
            socket.tournamentId = tournamentId;
            socket.isMatchPageClient = true;
            socket.matchPageType = type || 'match-page';
            
            console.log(`? [Socket.IO] Client joined match rooms: ${rooms.join(', ')}`);
            console.log(`?? [Socket.IO] Match identification - UUID: ${match.uniqueId || 'none'}, Numeric: ${match.matchId || match.id || match.Id}`);
            
            socket.emit('match-room-joined', {
                success: true,
                matchRooms: rooms,
                primaryRoom: match.uniqueId ? `match_${tournamentId}_${match.uniqueId}` : numericMatchRoom,
                tournamentId,
                matchId,
                matchIdentification: {
                    uniqueId: match.uniqueId,
                    numericId: match.matchId || match.id || match.Id,
                    matchType: match.matchType || 'Unknown'
                },
                timestamp: new Date().toISOString()
            });
            
        } catch (error) {
            console.error(`? [Socket.IO] Error joining match room:`, error);
            socket.emit('match-room-error', {
                success: false,
                error: error.message
            });
        }
    }

    // Get match data for individual match pages
    handleGetMatchData(socket, data) {
        try {
            const { tournamentId, matchId } = data;
            console.log(`?? [Socket.IO] Get match data request: ${tournamentId}/${matchId} from ${socket.id}`);
            
            const tournament = this.tournamentRegistry.getTournament(tournamentId);
            
            if (!tournament) {
                socket.emit('match-data', {
                    success: false,
                    error: `Tournament ${tournamentId} not found`
                });
                return;
            }
            
            const matches = tournament.matches || [];
            
            // ERWEITERT: UUID-bewusste Match-Suche
            const match = matches.find(m => 
                // Priorisiere UniqueId (UUID)
                (m.uniqueId && m.uniqueId === matchId) ||
                // Fallback auf numerische IDs
                (m.matchId || m.id || m.Id) == matchId
            );
            
            if (!match) {
                socket.emit('match-data', {
                    success: false,
                    error: `Match ${matchId} not found`
                });
                return;
            }
            
            // Find game rules for this match
            const gameRules = tournament.gameRules || [];
            let matchGameRules = null;
            
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
                const classId = match.classId || match.ClassId || 1;
                const className = match.className || match.ClassName || `Klasse ${classId}`;
                
                matchGameRules = {
                    id: `default_${classId}`,
                    name: `${className} Standard`,
                    gamePoints: 501,
                    gameMode: 'Standard',
                    finishMode: 'DoubleOut',
                    playWithSets: true,
                    setsToWin: 3,
                    legsToWin: 3,
                    legsPerSet: 5,
                    classId: classId
                };
            }
            
            // Erweitere Match-Objekt mit UUID-Informationen
            const enrichedMatch = {
                ...match,
                // Eindeutige Identifikation
                id: match.uniqueId || match.matchId || match.id || match.Id,
                uniqueId: match.uniqueId,
                matchId: match.matchId || match.id || match.Id,
                tournamentId: tournamentId,
                tournamentName: tournament.name,
                // Zusätzliche Match-Informationen
                matchType: match.matchType || 'Unknown',
                bracketType: match.bracketType || null,
                round: match.round || null,
                position: match.position || null
            };
            
            console.log(`? [Socket.IO] Sending match data: ${enrichedMatch.id} (UUID: ${match.uniqueId || 'none'}) with ${matchGameRules.name || 'default'} rules`);
            
            socket.emit('match-data', {
                success: true,
                match: enrichedMatch,
                gameRules: matchGameRules,
                tournament: {
                    id: tournament.id,
                    name: tournament.name,
                    description: tournament.description
                },
                matchIdentification: {
                    requestedId: matchId,
                    uniqueId: match.uniqueId,
                    numericId: match.matchId || match.id || match.Id,
                    matchType: match.matchType || 'Unknown'
                },
                timestamp: new Date().toISOString()
            });
            
        } catch (error) {
            console.error(`? [Socket.IO] Error getting match data:`, error);
            socket.emit('match-data', {
                success: false,
                error: error.message
            });
        }
    }

    broadcastTournamentUpdate(tournamentId, updateData) {
        const broadcastData = {
            type: 'tournament-update',
            tournamentId,
            updateData,
            timestamp: new Date().toISOString(),
            source: 'socket-io'
        };
        
        this.io.to(`tournament-${tournamentId}`).emit('tournament-updated', broadcastData);
        this.io.to(`tournament-${tournamentId}`).emit('matches-synced', broadcastData);
        console.log(`?? [Socket.IO] Tournament update broadcasted to tournament-${tournamentId}`);
    }
}

module.exports = SocketIOHandlers;