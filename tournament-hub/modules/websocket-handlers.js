// WebSocket Handler Module
// Handles WebSocket message processing and client management

class WebSocketHandlers {
    constructor(tournamentRegistry, matchService, io) {
        this.tournamentRegistry = tournamentRegistry;
        this.matchService = matchService;
        this.io = io;
        this.websocketClients = new Map();
    }

    // ========================
    // WebSocket Message Handlers
    // ========================

    async handleWebSocketMessage(ws, clientId, message) {
        const client = this.websocketClients.get(clientId);
        if (!client) return;
        
        console.log(`🔄 [WebSocket-Direct] ===== PROCESSING MESSAGE =====`);
        console.log(`🔄 [WebSocket-Direct] Client ID: ${clientId}`);
        console.log(`🔄 [WebSocket-Direct] Raw Message:`, JSON.stringify(message, null, 2));
        
        // Behandle sowohl direkte Messages als auch Messages mit 'data' Wrapper
        const actualMessage = message.data || message;
        const messageType = actualMessage.type || message.type;
        
        console.log(`🎯 [WebSocket-Direct] Extracted message type: ${messageType}`);
        
        try {
            switch (messageType) {
                case 'subscribe-tournament':
                    let tournamentId;
                    if (typeof actualMessage.data === 'string') {
                        tournamentId = actualMessage.data;
                    } else {
                        tournamentId = actualMessage.tournamentId || actualMessage.data?.tournamentId;
                    }
                    this.handleTournamentSubscription(ws, clientId, tournamentId);
                    break;
                    
                case 'register-planner':
                    const plannerTournamentId = actualMessage.tournamentId || actualMessage.data?.tournamentId;
                    const plannerInfo = actualMessage.plannerInfo || actualMessage.data?.plannerInfo;
                    this.handlePlannerRegistration(ws, clientId, plannerTournamentId, plannerInfo);
                    break;
                    
                case 'submit-match-result':
                    this.handleMatchResultSubmission(ws, clientId, actualMessage);
                    break;
                    
                case 'heartbeat':
                    this.handleHeartbeat(ws, clientId);
                    break;
                    
                case 'match-update-acknowledged':
                    this.handleMatchUpdateAcknowledgment(ws, clientId, message.data || message);
                    break;
                    
                case 'match-update-error':
                    this.handleMatchUpdateError(ws, clientId, message.data || message);
                    break;
                    
                default:
                    console.log(`❓ [WebSocket-Direct] Unknown message type: ${messageType}`);
                    ws.send(JSON.stringify({
                        type: 'error',
                        error: `Unknown message type: ${messageType}`,
                        receivedMessage: message
                    }));
            }
        } catch (error) {
            console.error(`❌ [WebSocket-Direct] Error handling message type ${messageType}:`, error);
            ws.send(JSON.stringify({
                type: 'error',
                error: `Error processing ${messageType}: ${error.message}`,
                timestamp: new Date().toISOString()
            }));
        }
        
        console.log(`🔄 [WebSocket-Direct] ===== MESSAGE PROCESSING COMPLETE =====`);
    }

    handleTournamentSubscription(ws, clientId, tournamentId) {
        const client = this.websocketClients.get(clientId);
        if (!client || !tournamentId) {
            console.log(`❌ [WebSocket-Direct] Tournament subscription failed - missing client or tournamentId`);
            return;
        }
        
        console.log(`🎯 [WebSocket-Direct] Client ${clientId} subscribing to tournament: ${tournamentId}`);
        
        client.tournamentId = tournamentId;
        
        const tournament = this.tournamentRegistry.getTournament(tournamentId);
        
        const responseData = {
            type: 'subscription-confirmed',
            tournamentId: tournamentId,
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
        
        ws.send(JSON.stringify(responseData));
        
        console.log(`✅ [WebSocket-Direct] Tournament subscription confirmed for ${clientId}`);
    }

    handlePlannerRegistration(ws, clientId, tournamentId, plannerInfo) {
        const client = this.websocketClients.get(clientId);
        if (!client) return;
        
        console.log(`🏁 [WebSocket-Direct] Planner registration for client ${clientId}`);
        
        client.tournamentId = tournamentId;
        client.isPlannerClient = true;
        client.plannerInfo = plannerInfo;
        
        const responseData = {
            type: 'planner-registration-confirmed',
            success: true,
            tournamentId: tournamentId,
            message: 'Tournament Planner registration successful',
            timestamp: new Date().toISOString()
        };
        
        ws.send(JSON.stringify(responseData));
        
        console.log(`✅ [WebSocket-Direct] Planner registration confirmed for ${clientId}`);
    }

    handleMatchResultSubmission(ws, clientId, message) {
        const client = this.websocketClients.get(clientId);
        if (!client) return;
        
        console.log(`🎯 [WebSocket-Direct] ===== MATCH RESULT SUBMISSION =====`);
        console.log(`🎯 [WebSocket-Direct] Client: ${clientId}`);
        console.log(`🎯 [WebSocket-Direct] Raw message:`, JSON.stringify(message, null, 2));
        
        const { tournamentId, matchId, result } = message;
        
        if (!tournamentId || !matchId || !result) {
            ws.send(JSON.stringify({
                type: 'match-result-error',
                error: 'Missing required data: tournamentId, matchId, or result',
                timestamp: new Date().toISOString()
            }));
            return;
        }
        
        // KORRIGIERT: Extrahiere Class-Information aus Message und Result
        console.log(`📊 [WebSocket-Direct] Class-ID Analysis:`);
        console.log(`   Message classId: ${message.classId}`);
        console.log(`   Message className: ${message.className}`);
        console.log(`   Result classId: ${result.classId}`);
        console.log(`   Result className: ${result.className}`);
        console.log(`   GameRules classId: ${result.gameRulesUsed?.classId}`);
        console.log(`   GameRules className: ${result.gameRulesUsed?.className}`);
        
        // ERWEITERT: Vollständige Class-Information für Tournament Planner
        const enhancedResult = {
            ...result,
            // Class-Information aus verschiedenen Quellen
            classId: message.classId || result.classId || result.gameRulesUsed?.classId || 1,
            className: message.className || result.className || result.gameRulesUsed?.className || 'Unbekannte Klasse',
            // Zusätzliche Metadaten
            submissionSource: 'websocket-direct',
            submissionTimestamp: new Date().toISOString(),
            originalMessageClassId: message.classId,
            originalMessageClassName: message.className
        };
        
        console.log(`📋 [WebSocket-Direct] Enhanced result for Tournament Planner:`);
        console.log(`   Final classId: ${enhancedResult.classId}`);
        console.log(`   Final className: ${enhancedResult.className}`);
        console.log(`   Enhanced result:`, JSON.stringify(enhancedResult, null, 2));
        
        // Process match result
        this.matchService.submitMatchResult(tournamentId, matchId, enhancedResult)
            .then(success => {
                if (success) {
                    ws.send(JSON.stringify({
                        type: 'match-result-submitted',
                        success: true,
                        tournamentId,
                        matchId,
                        classId: enhancedResult.classId,
                        className: enhancedResult.className,
                        message: `Match result submitted successfully for ${enhancedResult.className}`,
                        timestamp: new Date().toISOString()
                    }));
                    
                    console.log(`✅ [WebSocket-Direct] Match result submitted for ${enhancedResult.className} (ID: ${enhancedResult.classId})`);
                    
                    // Broadcast to other clients with enhanced class information
                    this.broadcastMatchUpdate(tournamentId, matchId, enhancedResult);
                } else {
                    ws.send(JSON.stringify({
                        type: 'match-result-error',
                        success: false,
                        error: 'Failed to submit match result',
                        timestamp: new Date().toISOString()
                    }));
                }
            })
            .catch(error => {
                console.error(`❌ [WebSocket-Direct] Error in match result submission:`, error);
                ws.send(JSON.stringify({
                    type: 'match-result-error',
                    success: false,
                    error: error.message,
                    classId: enhancedResult.classId,
                    className: enhancedResult.className,
                    timestamp: new Date().toISOString()
                }));
            });
            
        console.log(`🎯 [WebSocket-Direct] ===== MATCH RESULT SUBMISSION COMPLETE =====`);
    }

    handleHeartbeat(ws, clientId) {
        const client = this.websocketClients.get(clientId);
        if (!client) return;
        
        client.lastHeartbeat = new Date();
        
        const responseData = {
            type: 'heartbeat-ack',
            clientId: clientId,
            timestamp: new Date().toISOString(),
            message: 'Heartbeat acknowledged'
        };
        
        ws.send(JSON.stringify(responseData));
    }

    handleMatchUpdateAcknowledgment(ws, clientId, message) {
        console.log(`📬 [WebSocket-Direct] Client ${clientId} acknowledged match update`);
        
        const client = this.websocketClients.get(clientId);
        if (!client) return;
        
        if (!client.matchUpdateStats) {
            client.matchUpdateStats = {
                totalReceived: 0,
                lastAcknowledged: null,
                errors: 0
            };
        }
        
        client.matchUpdateStats.totalReceived++;
        client.matchUpdateStats.lastAcknowledged = new Date();
    }

    handleMatchUpdateError(ws, clientId, message) {
        console.log(`❌ [WebSocket-Direct] Client ${clientId} reported match update error`);
        
        const client = this.websocketClients.get(clientId);
        if (!client) return;
        
        if (!client.matchUpdateStats) {
            client.matchUpdateStats = {
                totalReceived: 0,
                lastAcknowledged: null,
                errors: 0
            };
        }
        
        client.matchUpdateStats.errors++;
    }

    // ========================
    // Client Management
    // ========================

    addClient(clientId, ws, clientIP) {
        this.websocketClients.set(clientId, {
            ws: ws,
            clientId: clientId,
            ip: clientIP,
            connectedAt: new Date(),
            lastHeartbeat: new Date(),
            tournamentId: null,
            isPlannerClient: false
        });
        
        console.log(`🔌 [WebSocket-Direct] Client connected: ${clientId} from ${clientIP}`);
    }

    removeClient(clientId) {
        this.websocketClients.delete(clientId);
        console.log(`🔌 [WebSocket-Direct] Client disconnected: ${clientId}`);
    }

    getClient(clientId) {
        return this.websocketClients.get(clientId);
    }

    // ========================
    // Broadcasting
    // ========================

    broadcastMatchUpdate(tournamentId, matchId, result) {
        console.log(`📡 [WebSocket-Direct] ===== BROADCASTING MATCH UPDATE =====`);
        console.log(`📡 [WebSocket-Direct] Tournament: ${tournamentId}, Match: ${matchId}`);
        console.log(`📡 [WebSocket-Direct] Result data:`, JSON.stringify(result, null, 2));
        
        // KORRIGIERT: Hole Match-Informationen aus Tournament Registry für korrekte Class-ID
        const tournament = this.tournamentRegistry.getTournament(tournamentId);
        if (!tournament) {
            console.error(`❌ [WebSocket-Direct] Tournament not found for broadcasting: ${tournamentId}`);
            return;
        }
        
        // Finde das Match in den Tournament-Daten um Class-ID zu erhalten
        const matches = tournament.matches || [];
        const originalMatch = matches.find(m => 
            String(m.id) === String(matchId) || 
            String(m.matchId) === String(matchId)
        );
        
        if (!originalMatch) {
            console.error(`❌ [WebSocket-Direct] Match not found in tournament data: ${matchId}`);
            console.log(`📋 [WebSocket-Direct] Available matches: ${matches.map(m => `${m.id}/${m.matchId} (Class: ${m.classId})`).join(', ')}`);
            return;
        }
        
        // KORRIGIERT: Extrahiere korrekte Class-Information aus ursprünglichem Match
        const classId = originalMatch.classId || originalMatch.ClassId || result.classId || 1;
        const className = originalMatch.className || originalMatch.ClassName || result.className || 'Unbekannte Klasse';
        
        console.log(`📚 [WebSocket-Direct] Match class information:`, {
            originalClassId: originalMatch.classId,
            originalClassName: originalMatch.className,
            resultClassId: result.classId,
            resultClassName: result.className,
            finalClassId: classId,
            finalClassName: className
        });
        
        const matchUpdate = {
            tournamentId,
            matchId,
            result: {
                ...result,
                submittedAt: new Date().toISOString(),
                source: 'websocket-direct',
                // KORRIGIERT: Class-Information aus ursprünglichem Match hinzufügen
                classId: classId,
                className: className,
                // Match-Details für bessere Debugging
                player1Name: originalMatch.player1 || originalMatch.Player1 || result.player1Name || 'Spieler 1',
                player2Name: originalMatch.player2 || originalMatch.Player2 || result.player2Name || 'Spieler 2',
                matchType: originalMatch.matchType || originalMatch.MatchType || result.matchType || 'Group'
            },
            timestamp: new Date().toISOString(),
            // KORRIGIERT: Class-Information auf Top-Level für bessere Verarbeitung
            classId: classId,
            className: className
        };
        
        console.log(`📊 [WebSocket-Direct] Enhanced match update:`, JSON.stringify(matchUpdate, null, 2));
        
        // Broadcast to WebSocket clients
        this.websocketClients.forEach((client, clientId) => {
            if (client.tournamentId === tournamentId && client.ws.readyState === 1) {
                try {
                    const message = {
                        type: 'tournament-match-updated',
                        tournamentId: tournamentId,
                        matchUpdate: matchUpdate,
                        timestamp: new Date().toISOString(),
                        source: 'websocket-direct',
                        // KORRIGIERT: Match result highlight für Tournament Planner
                        matchResultHighlight: true,
                        // KORRIGIERT: Class-Information direkt auf Message-Level
                        classId: classId,
                        className: className
                    };
                    
                    client.ws.send(JSON.stringify(message));
                    console.log(`📤 [WebSocket-Direct] Message sent to client ${clientId} with Class: ${className} (ID: ${classId})`);
                } catch (error) {
                    console.error(`❌ [WebSocket-Direct] Error sending to client ${clientId}:`, error);
                    this.removeClient(clientId);
                }
            }
        });
        
        // Also broadcast via Socket.IO with enhanced class information
        const socketIOMessage = {
            type: 'match-result-update',
            tournamentId,
            matchId,
            result: matchUpdate.result,
            timestamp: new Date().toISOString(),
            source: 'websocket-direct-bridge',
            // KORRIGIERT: Class-Information für Socket.IO
            classId: classId,
            className: className,
            matchResultHighlight: true
        };
        
        this.io.to(`tournament-${tournamentId}`).emit('tournament-match-updated', socketIOMessage);
        
        console.log(`📡 [WebSocket-Direct] Broadcasting complete for Match ${matchId} in ${className} (Class ID: ${classId})`);
        console.log(`📡 [WebSocket-Direct] ===== BROADCASTING COMPLETE =====`);
    }
}

module.exports = WebSocketHandlers;