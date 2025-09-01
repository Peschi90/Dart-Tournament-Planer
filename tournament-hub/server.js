// Tournament Hub Server - Production Configuration (Modular)
// Main server file with modular architecture

const path = require('path');
const fs = require('fs');

// Validate package.json before continuing
try {
    const packagePath = path.join(__dirname, 'package.json');
    const packageData = fs.readFileSync(packagePath, 'utf8');
    const packageJson = JSON.parse(packageData);
    console.log(`📦 Package: ${packageJson.name} v${packageJson.version}`);
} catch (error) {
    console.error('❌ Invalid package.json configuration:');
    console.error('   Error:', error.message);
    console.error('   Please run: ./fix-package-config.sh');
    process.exit(1);
}

const express = require('express');
const http = require('http');
const https = require('https');
const socketIo = require('socket.io');
const cors = require('cors');
const rateLimit = require('express-rate-limit');

// Import services with error handling
let TournamentRegistry, MatchService, QRCodeService;
try {
    TournamentRegistry = require('./services/TournamentRegistry');
    MatchService = require('./services/MatchService');
    QRCodeService = require('./services/QRCodeService');
    console.log('✅ Services loaded successfully');
} catch (error) {
    console.error('❌ Error loading services:', error.message);
    console.error('   Please check services directory structure');
    process.exit(1);
}

// Import modular components
const WebSocketHandlers = require('./modules/websocket-handlers');
const SocketIOHandlers = require('./modules/socketio-handlers');
const createApiRoutes = require('./modules/api-routes');
const WebSocketServer = require('./modules/websocket-server');

// Create Express app
const app = express();

// Initialize services
const tournamentRegistry = new TournamentRegistry();
const matchService = new MatchService(tournamentRegistry);
const qrCodeService = new QRCodeService();

// Environment configuration for ONLINE PRODUCTION
const PORT = process.env.PORT || 9443;
const HTTP_PORT = process.env.HTTP_PORT || 3000;
const HOST = process.env.HOST || '0.0.0.0';
const HTTPS_ENABLED = process.env.HTTPS_ENABLED !== 'false';
const SSL_KEY_PATH = process.env.SSL_KEY_PATH || '/etc/ssl/private/server.key';
const SSL_CERT_PATH = process.env.SSL_CERT_PATH || '/etc/ssl/certs/server.crt';

console.log(`🔧 Server Configuration:`);
console.log(`   Environment: ${process.env.NODE_ENV || 'development'}`);
console.log(`   HTTPS: ${HTTPS_ENABLED}`);
console.log(`   Primary Port: ${PORT}`);
console.log(`   HTTP Port: ${HTTP_PORT}`);
console.log(`   Host: ${HOST}`);

// Middleware
app.use(cors({
    origin: ["https://dtp.i3ull3t.de", "http://dtp.i3ull3t.de", "https://dtp.i3ull3t.de:9443", "http://localhost:5000"],
    credentials: true,
    methods: ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
    allowedHeaders: ["Content-Type", "Authorization", "X-Requested-With"]
}));

app.use(express.json({ limit: '50mb' }));
app.use(express.static(path.join(__dirname, 'public')));

// Enhanced rate limiting for production
const limiter = rateLimit({
    windowMs: 15 * 60 * 1000, // 15 minutes
    max: 1000, // Limit each IP to 1000 requests per windowMs
    message: {
        error: 'Too many requests from this IP',
        retryAfter: '15 minutes'
    },
    standardHeaders: true,
    legacyHeaders: false
});
app.use('/api/', limiter);

// Create server (HTTP or HTTPS based on configuration)
let server;
let wsProtocol = 'ws';

if (HTTPS_ENABLED) {
    // HTTPS Server for production
    try {
        if (fs.existsSync(SSL_KEY_PATH) && fs.existsSync(SSL_CERT_PATH)) {
            const httpsOptions = {
                key: fs.readFileSync(SSL_KEY_PATH),
                cert: fs.readFileSync(SSL_CERT_PATH)
            };
            server = https.createServer(httpsOptions, app);
            wsProtocol = 'wss';
            console.log('🔒 HTTPS server configured with SSL certificates');
        } else {
            console.log('⚠️ SSL certificates not found, falling back to HTTP');
            server = http.createServer(app);
        }
    } catch (error) {
        console.error('❌ Error loading SSL certificates:', error.message);
        console.log('⚠️ Falling back to HTTP server');
        server = http.createServer(app);
    }
} else {
    // HTTP Server for development
    server = http.createServer(app);
    console.log('🔓 HTTP server configured');
}

// Initialize Socket.IO with production configuration
const io = socketIo(server, {
    cors: {
        origin: ["https://dtp.i3ull3t.de", "http://dtp.i3ull3t.de", "https://dtp.i3ull3t.de:9443", "http://localhost:5000"],
        methods: ["GET", "POST"],
        allowedHeaders: ["*"],
        credentials: true
    },
    transports: ['websocket', 'polling'],
    allowEIO3: true,
    pingTimeout: 60000,
    pingInterval: 25000
});

// Initialize modular components
console.log('🔌 Setting up modular WebSocket and Socket.IO handlers...');

const websocketHandlers = new WebSocketHandlers(tournamentRegistry, matchService, io);
const socketIOHandlers = new SocketIOHandlers(io, tournamentRegistry, matchService, websocketHandlers);
const websocketServer = new WebSocketServer(HTTPS_ENABLED, SSL_KEY_PATH, SSL_CERT_PATH, HOST, websocketHandlers);

// Setup API routes
const apiRoutes = createApiRoutes(tournamentRegistry, matchService, socketIOHandlers, io, websocketHandlers);
app.use('/api', apiRoutes);

// Static routes
app.get('/', (req, res) => {
    const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
    console.log(`🏠 [API] Root access from ${clientIP}`);
    res.sendFile(path.join(__dirname, 'public', 'dashboard.html'));
});

app.get('/join/:tournamentId', (req, res) => {
    const { tournamentId } = req.params;
    const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
    console.log(`🎮 [API] Join tournament page: ${tournamentId} from ${clientIP}`);
    res.sendFile(path.join(__dirname, 'public', 'join-tournament.html'));
});

app.get('/tournament/:tournamentId', (req, res) => {
    const { tournamentId } = req.params;
    const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
    console.log(`🏆 [API] Tournament interface: ${tournamentId} from ${clientIP}`);
    res.sendFile(path.join(__dirname, 'public', 'tournament-interface.html'));
});

app.get('/match/:tournamentId/:matchId', (req, res) => {
    const { tournamentId, matchId } = req.params;
    const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
    console.log(`🎯 [API] Match page: ${matchId} in tournament ${tournamentId} from ${clientIP}`);
    res.sendFile(path.join(__dirname, 'public', 'match-page.html'));
});

app.get('/dashboard.html', (req, res) => {
    const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
    console.log(`📊 [API] Dashboard accessed from ${clientIP}`);
    res.sendFile(path.join(__dirname, 'public', 'dashboard.html'));
});

// Error handling middleware
app.use((error, req, res, next) => {
    console.error(`❌ [API] Unhandled error:`, error);
    res.status(500).json({
        success: false,
        message: 'Internal server error',
        timestamp: new Date().toISOString()
    });
});

// Graceful shutdown handling
function gracefulShutdown(signal) {
    console.log(`🔄 [PROD] Received ${signal}, shutting down gracefully`);
    server.close(() => {
        console.log('🔴 [PROD] Server closed');
        process.exit(0);
    });
}

process.on('SIGTERM', () => gracefulShutdown('SIGTERM'));
process.on('SIGINT', () => gracefulShutdown('SIGINT'));

// Start server with enhanced error handling
server.listen(PORT, HOST, () => {
    const protocol = HTTPS_ENABLED ? 'HTTPS' : 'HTTP';
    const wsProtocolDisplay = HTTPS_ENABLED ? 'WSS' : 'WS';

    console.log(`🚀 Tournament Hub PRODUCTION Server started (Modular Architecture)`);
    console.log(`📍 ${protocol} Server: ${protocol.toLowerCase()}://dtp.i3ull3t.de:${PORT}`);
    console.log(`🔌 WebSocket Server: ${wsProtocolDisplay.toLowerCase()}://dtp.i3ull3t.de:${PORT}`);
    console.log(`🌐 Dashboard: https://dtp.i3ull3t.de:${PORT}/dashboard.html`);
    console.log(`📱 Join Interface: https://dtp.i3ull3t.de:${PORT}/join/[tournament-id]`);
    console.log(`📊 Health Check: https://dtp.i3ull3t.de:${PORT}/api/health`);
    console.log(`✅ PRODUCTION Server is ready for online access!`);
    console.log(`🌍 Server binding to ${HOST}:${PORT} for external access`);

    // Start HTTP redirect server after main server is running
    startHttpRedirectServer();
});

server.on('error', (error) => {
    console.error('❌ [PROD] Server error:', error);
    if (error.code === 'EADDRINUSE') {
        console.error(`❌ Port ${PORT} is already in use`);
        process.exit(1);
    }
});

// HTTP redirect server function
function startHttpRedirectServer() {
    setTimeout(() => {
        if (HTTPS_ENABLED && HTTP_PORT !== PORT && !process.env.DISABLE_HTTP_REDIRECT) {
            console.log(`🔄 [HTTP-REDIRECT] Starting HTTP redirect server on port ${HTTP_PORT}...`);

            const httpApp = express();
            httpApp.get('*', (req, res) => {
                const httpsUrl = `https://dtp.i3ull3t.de:${PORT}${req.url}`;
                const clientIP = req.headers['x-forwarded-for'] || req.connection.remoteAddress || 'unknown';
                console.log(`🔄 [HTTP-REDIRECT] Redirecting ${clientIP} to ${httpsUrl}`);
                res.redirect(301, httpsUrl);
            });

            const httpServer = http.createServer(httpApp);

            httpServer.on('error', (error) => {
                if (error.code === 'EADDRINUSE') {
                    console.error(`❌ [HTTP-REDIRECT] Port ${HTTP_PORT} is already in use`);
                } else {
                    console.error(`❌ [HTTP-REDIRECT] Server error:`, error);
                }
            });

            httpServer.listen(HTTP_PORT, HOST, () => {
                console.log(`🔄 HTTP Redirect Server: http://dtp.i3ull3t.de:${HTTP_PORT} -> https://dtp.i3ull3t.de:${PORT}`);
            });
        } else if (process.env.DISABLE_HTTP_REDIRECT) {
            console.log(`🚫 [HTTP-REDIRECT] HTTP redirect server disabled`);
        } else {
            console.log(`ℹ️ [HTTP-REDIRECT] HTTP redirect not needed`);
        }
    }, 2000);
}