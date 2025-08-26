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
                socket.emit('result-submitted', {
                    success: true,
                    tournamentId,
                    matchId,
                    classId: finalClassId,
                    className: finalClassName,
                    message: `Match result submitted and broadcasted successfully for ${finalClassName}`
                });
                
                console.log(`? [Socket.IO] Match result submitted and broadcasted: ${tournamentId}/${matchId} in ${finalClassName}`);
                
            } else {
                socket.emit('match-result-error', {
                    success: false,
                    tournamentId,
                    matchId,
                    error: 'Failed to submit match result'
                });
            }
        } catch (error) {
            console.error(`? [Socket.IO] Error in match result submission:`, error);
            socket.emit('match-result-error', {
                success: false,
                error: error.message
            });
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

    // Helper methods for broadcasting
    broadcastMatchUpdate(tournamentId, matchId, result) {
        console.log(`?? [Socket.IO] ===== BROADCASTING MATCH UPDATE =====`);
        console.log(`?? [Socket.IO] Tournament: ${tournamentId}, Match: ${matchId}`);
        
        // KORRIGIERT: Hole Class-Information aus Tournament Registry falls nicht in result vorhanden
        let classId = result?.classId || 1;
        let className = result?.className || 'Unbekannte Klasse';
        
        // Fallback: Aus Tournament-Daten holen falls nicht vorhanden
        if (!result?.classId && !result?.className) {
            const tournament = this.tournamentRegistry.getTournament(tournamentId);
            if (tournament && tournament.matches) {
                const originalMatch = tournament.matches.find(m => 
                    String(m.id) === String(matchId) || 
                    String(m.matchId) === String(matchId)
                );
                
                if (originalMatch) {
                    classId = originalMatch.classId || originalMatch.ClassId || classId;
                    className = originalMatch.className || originalMatch.ClassName || className;
                    
                    console.log(`?? [Socket.IO] Retrieved class info from tournament data: ${className} (ID: ${classId})`);
                }
            }
        }
        
        const broadcastData = {
            type: 'match-result-update',
            tournamentId,
            matchId,
            result: {
                ...result,
                classId: classId,
                className: className
            },
            timestamp: new Date().toISOString(),
            source: 'socket-io',
            // KORRIGIERT: Class-Information auf Top-Level
            classId: classId,
            className: className,
            matchResultHighlight: true
        };
        
        console.log(`?? [Socket.IO] Broadcasting for class: ${className} (ID: ${classId})`);
        
        this.io.to(`tournament-${tournamentId}`).emit('match-updated', broadcastData);
        this.io.to(`tournament-${tournamentId}`).emit('match-result-updated', broadcastData);
        
        console.log(`?? [Socket.IO] Match update broadcasted to tournament-${tournamentId} for ${className}`);
        console.log(`?? [Socket.IO] ===== BROADCASTING COMPLETE =====`);
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