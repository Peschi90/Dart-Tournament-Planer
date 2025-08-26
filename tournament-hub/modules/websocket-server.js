// WebSocket Server Setup Module
// Handles WebSocket server creation and client management

const WebSocket = require('ws');
const https = require('https');
const fs = require('fs');

class WebSocketServer {
    constructor(HTTPS_ENABLED, SSL_KEY_PATH, SSL_CERT_PATH, HOST, websocketHandlers) {
        this.HTTPS_ENABLED = HTTPS_ENABLED;
        this.SSL_KEY_PATH = SSL_KEY_PATH;
        this.SSL_CERT_PATH = SSL_CERT_PATH;
        this.HOST = HOST;
        this.websocketHandlers = websocketHandlers;
        this.wss = null;
        
        this.setupWebSocketServer();
    }

    setupWebSocketServer() {
        console.log('?? Setting up WebSocket server for Tournament Planner integration...');
        
        try {
            if (this.HTTPS_ENABLED && fs.existsSync(this.SSL_KEY_PATH) && fs.existsSync(this.SSL_CERT_PATH)) {
                // SSL WebSocket Server
                const httpsOptions = {
                    key: fs.readFileSync(this.SSL_KEY_PATH),
                    cert: fs.readFileSync(this.SSL_CERT_PATH)
                };
                
                const httpsServer = https.createServer(httpsOptions);
                
                this.wss = new WebSocket.Server({ 
                    server: httpsServer,
                    path: '/ws'
                });
                
                httpsServer.listen(9444, this.HOST, () => {
                    console.log('?? SSL WebSocket server started on port 9444');
                    console.log('?? WebSocket endpoint: wss://dtp.i3ull3t.de:9444/ws');
                });
                
            } else {
                // HTTP WebSocket Server
                console.log('?? SSL certificates not found, using HTTP WebSocket');
                
                this.wss = new WebSocket.Server({ 
                    port: 9445,
                    path: '/ws',
                    host: this.HOST
                });
                
                console.log('?? HTTP WebSocket server started on port 9445');
                console.log('?? WebSocket endpoint: ws://dtp.i3ull3t.de:9445/ws');
            }
        } catch (error) {
            console.error('? Error creating WebSocket server:', error.message);
            console.log('?? Creating fallback HTTP WebSocket server...');
            
            this.wss = new WebSocket.Server({ 
                port: 9445,
                path: '/ws',
                host: this.HOST
            });
            
            console.log('?? Fallback HTTP WebSocket server started on port 9445');
        }

        this.setupEventHandlers();
    }

    setupEventHandlers() {
        this.wss.on('connection', (ws, request) => {
            const clientIP = request.headers['x-forwarded-for'] || request.socket.remoteAddress || 'unknown';
            const clientId = Math.random().toString(36).substr(2, 9);
            
            this.websocketHandlers.addClient(clientId, ws, clientIP);
            
            // Send welcome message
            ws.send(JSON.stringify({
                type: 'welcome',
                clientId: clientId,
                message: 'Connected to Tournament Hub WebSocket',
                timestamp: new Date().toISOString(),
                keepAliveInterval: 30000
            }));
            
            // Setup keep-alive
            const keepAliveInterval = setInterval(() => {
                if (ws.readyState === WebSocket.OPEN) {
                    try {
                        ws.ping();
                    } catch (error) {
                        console.error(`? [WebSocket] Error sending ping to ${clientId}:`, error);
                        clearInterval(keepAliveInterval);
                        this.websocketHandlers.removeClient(clientId);
                    }
                } else {
                    clearInterval(keepAliveInterval);
                    this.websocketHandlers.removeClient(clientId);
                }
            }, 30000);
            
            ws.on('pong', () => {
                const client = this.websocketHandlers.getClient(clientId);
                if (client) {
                    client.lastHeartbeat = new Date();
                }
            });
            
            ws.on('message', async (data) => {
                try {
                    const message = JSON.parse(data.toString());
                    
                    // Update last activity
                    const client = this.websocketHandlers.getClient(clientId);
                    if (client) {
                        client.lastHeartbeat = new Date();
                    }
                    
                    await this.websocketHandlers.handleWebSocketMessage(ws, clientId, message);
                } catch (error) {
                    console.error(`? [WebSocket] Error parsing message from ${clientId}:`, error);
                    ws.send(JSON.stringify({
                        type: 'error',
                        error: 'Invalid message format'
                    }));
                }
            });

            ws.on('close', () => {
                clearInterval(keepAliveInterval);
                this.websocketHandlers.removeClient(clientId);
            });

            ws.on('error', (error) => {
                console.error(`? [WebSocket] Error from client ${clientId}:`, error);
                clearInterval(keepAliveInterval);
                this.websocketHandlers.removeClient(clientId);
            });
        });
    }
}

module.exports = WebSocketServer;