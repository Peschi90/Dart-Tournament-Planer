// tournament-interface-api.js - API and WebSocket communication

// Global variables
let socket = null;
let tournamentId = null;
let currentTournament = null;
let matches = [];
let tournamentClasses = [];
let gameRules = [];
let currentClassId = null;

// Make variables globally accessible
window.socket = socket;
window.tournamentId = tournamentId;
window.currentTournament = currentTournament;
window.matches = matches;
window.tournamentClasses = tournamentClasses;
window.gameRules = gameRules;
window.currentClassId = currentClassId;

// Socket.IO Initialisierung
function initializeSocket() {
    console.log('🔌 Initializing Socket.IO connection...');
    
    try {
        socket = io();
        window.socket = socket;
        
        console.log('🔌 Socket.IO connection attempt started');
        
        // Connection Events
        socket.on('connect', function() {
            console.log('✅ Socket.IO connected:', socket.id);
            updateConnectionStatus(true);
            
            if (tournamentId) {
                console.log('🎯 Joining tournament:', tournamentId);
                socket.emit('joinTournament', { tournamentId: tournamentId });
            }
        });
        
        socket.on('disconnect', function() {
            console.log('❌ Socket.IO disconnected');
            updateConnectionStatus(false);
        });
        
        socket.on('connect_error', function(error) {
            console.error('❌ Socket.IO connection error:', error);
            updateConnectionStatus(false);
        });
        
        // Tournament Events
        socket.on('tournament-joined', function(data) {
            console.log('🎯 Tournament joined:', data);
            if (data.success && data.tournament) {
                updateTournamentInfo(data.tournament);
            }
        });
        
        socket.on('tournament-data', function(data) {
            console.log('📊 Tournament data received via Socket.IO:', data);
            
            try {
                if (data.tournament) {
                    console.log('🏆 Updating tournament info from Socket.IO');
                    currentTournament = data.tournament;
                    window.currentTournament = currentTournament;
                    updateTournamentInfo(data.tournament);
                }
                
                if (data.matches) {
                    console.log('🎮 Updating matches from Socket.IO:', data.matches.length, 'matches');
                    matches = data.matches;
                    window.matches = matches;
                    displayMatches(data.matches);
                }
                
                if (data.tournamentClasses) {
                    console.log('📚 Updating tournament classes from Socket.IO:', data.tournamentClasses.length, 'classes');
                    tournamentClasses = data.tournamentClasses;
                    window.tournamentClasses = tournamentClasses;
                    updateClassSelector(data.tournamentClasses);
                }
                
                if (data.gameRules) {
                    console.log('🎮 Updating game rules from Socket.IO:', data.gameRules.length, 'rules');
                    gameRules = data.gameRules;
                    window.gameRules = gameRules;
                }
                
                console.log('✅ Socket.IO tournament data processed successfully');
            } catch (error) {
                console.error('❌ Error processing Socket.IO tournament data:', error);
            }
        });
        
        socket.on('matches-updated', function(data) {
            console.log('🎮 Matches updated via Socket.IO:', data);
            
            try {
                if (data.matches && Array.isArray(data.matches)) {
                    matches = data.matches;
                    window.matches = matches;
                    displayMatches(matches);
                    console.log('✅ Matches updated from Socket.IO successfully');
                } else {
                    console.warn('⚠️ Invalid matches data from Socket.IO:', data);
                }
            } catch (error) {
                console.error('❌ Error processing matches update:', error);
            }
        });
        
        socket.on('result-submitted', function(data) {
            console.log('✅ Result submitted confirmation:', data);
            
            try {
                const message = data.matchId ? 
                    `✅ Ergebnis für Match ${data.matchId} erfolgreich übertragen!` : 
                    '✅ Match-Ergebnis erfolgreich übertragen!';
                showNotification(message, 'success');
                
                setTimeout(() => {
                    console.log('🔄 Reloading matches after successful submission');
                    loadMatches();
                }, 1000);
            } catch (error) {
                console.error('❌ Error processing result submission confirmation:', error);
            }
        });
        
        socket.on('error', function(data) {
            console.error('❌ Socket.IO error:', data);
            const errorMessage = data.error || data.message || 'Unbekannter Socket.IO Fehler';
            showNotification(`❌ Socket.IO Fehler: ${errorMessage}`, 'error');
        });
        
        console.log('🔌 Socket.IO event listeners registered');
        
    } catch (error) {
        console.error('❌ Error initializing Socket.IO:', error);
        updateConnectionStatus(false);
        
        console.log('🔄 Falling back to REST API...');
        loadTournamentData();
    }
}

// REST API Fallback für Tournament Daten
async function loadTournamentData() {
    try {
        console.log('📡 Loading tournament data via REST API...');
        
        const response = await fetch(`/api/tournaments/${tournamentId}`);
        if (response.ok) {
            const apiResponse = await response.json();
            console.log('📊 Tournament data loaded:', apiResponse);
            
            // KORRIGIERT: Handle API response structure - data is nested in apiResponse.data
            let data = apiResponse;
            if (apiResponse.data) {
                console.log('📊 Using nested data structure from API response');
                data = apiResponse.data;
            }
            
            if (data) {
                // Verarbeite Tournament-Information
                if (data.tournament) {
                    console.log('🏆 Processing tournament info:', data.tournament);
                    currentTournament = data.tournament;
                    window.currentTournament = currentTournament;
                    updateTournamentInfo(data.tournament);
                } else if (data.name || data.id) {
                    console.log('🏆 Using direct tournament data as fallback');
                    currentTournament = data;
                    window.currentTournament = currentTournament;
                    updateTournamentInfo(data);
                } else {
                    console.log('🏆 Creating fallback tournament info');
                    currentTournament = {
                        id: tournamentId,
                        name: `Tournament ${tournamentId}`,
                        location: 'Unbekannt',
                        description: 'Automatisch geladen'
                    };
                    window.currentTournament = currentTournament;
                    updateTournamentInfo(currentTournament);
                }
                
                // Verarbeite Matches
                if (data.matches && Array.isArray(data.matches)) {
                    console.log('🎮 Processing matches:', data.matches.length, 'matches');
                    matches = data.matches;
                    window.matches = matches;
                    displayMatches(data.matches);
                } else if (Array.isArray(data)) {
                    console.log('🎮 Using data as direct matches array');
                    matches = data;
                    window.matches = matches;
                    displayMatches(data);
                } else {
                    console.warn('⚠️ No matches found in data, trying to load matches separately...');
                    // Load matches via separate endpoint
                    await loadMatches();
                }
                
                // Verarbeite Tournament Classes
                if (data.tournamentClasses && Array.isArray(data.tournamentClasses)) {
                    console.log('📚 Processing tournament classes:', data.tournamentClasses.length, 'classes');
                    tournamentClasses = data.tournamentClasses;
                    window.tournamentClasses = tournamentClasses;
                    updateClassSelector(data.tournamentClasses);
                } else if (data.classes && Array.isArray(data.classes)) {
                    console.log('📚 Using classes property as fallback');
                    tournamentClasses = data.classes;
                    window.tournamentClasses = tournamentClasses;
                    updateClassSelector(data.classes);
                } else {
                    console.warn('⚠️ No tournament classes found in data, trying to load classes separately...');
                    // Load classes via separate endpoint
                    await loadTournamentClasses();
                }
                
                // Verarbeite Game Rules
                if (data.gameRules && Array.isArray(data.gameRules)) {
                    console.log('🎮 Processing game rules:', data.gameRules.length, 'rules');
                    gameRules = data.gameRules;
                    window.gameRules = gameRules;
                } else {
                    console.warn('⚠️ No game rules found in data');
                    gameRules = [];
                    window.gameRules = gameRules;
                }
                
                console.log('✅ Tournament data processing complete');
                console.log(`📊 Final state: Tournament=${!!currentTournament}, Matches=${matches.length}, Classes=${tournamentClasses.length}, Rules=${gameRules.length}`);
                
            } else {
                console.error('❌ Empty response data');
                displayNoMatches(`Leere Antwort vom Server`);
            }
        } else {
            console.error('❌ Failed to load tournament data:', response.status, response.statusText);
            const errorText = await response.text();
            console.error('❌ Error response body:', errorText);
            displayNoMatches(`Server Fehler ${response.status}: ${response.statusText}`);
        }
    } catch (error) {
        console.error('❌ Error loading tournament data:', error);
        console.error('❌ Error stack:', error.stack);
        displayNoMatches(`Netzwerkfehler: ${error.message}`);
    }
}

// NEW FUNCTION: Load tournament classes separately
async function loadTournamentClasses() {
    try {
        console.log('📚 Loading tournament classes via REST API...');
        
        const response = await fetch(`/api/tournaments/${tournamentId}/classes`);
        if (response.ok) {
            const apiResponse = await response.json();
            console.log('📚 Tournament classes loaded:', apiResponse);
            
            let data = apiResponse.data || apiResponse;
            
            if (data && Array.isArray(data)) {
                tournamentClasses = data;
                window.tournamentClasses = tournamentClasses;
                updateClassSelector(data);
                console.log(`✅ Successfully loaded ${data.length} tournament classes`);
            } else {
                console.warn('⚠️ No tournament classes in response');
            }
        } else {
            console.error('❌ Failed to load tournament classes:', response.statusText);
        }
    } catch (error) {
        console.error('❌ Error loading tournament classes:', error);
    }
}

// Matches via REST API laden
async function loadMatches() {
    try {
        console.log('🎮 Loading matches via REST API...');
        
        let url = `/api/tournaments/${tournamentId}/matches`;
        if (currentClassId) {
            url += `?classId=${currentClassId}`;
        }
        
        const response = await fetch(url);
        if (response.ok) {
            const apiResponse = await response.json();
            console.log('🎮 Matches loaded:', apiResponse);
            
            // KORRIGIERT: Handle API response structure - check for nested data
            let data = apiResponse.data || apiResponse;
            
            if (data && Array.isArray(data)) {
                matches = data;
                window.matches = matches;
                displayMatches(matches);
                console.log(`✅ Successfully loaded ${data.length} matches`);
            } else if (data && data.matches && Array.isArray(data.matches)) {
                matches = data.matches;
                window.matches = matches;
                displayMatches(data.matches);
                console.log(`✅ Successfully loaded ${data.matches.length} matches from data.matches`);
            } else {
                console.warn('⚠️ No matches found in API response:', data);
                displayNoMatches('Keine Matches in der API-Antwort gefunden');
            }
        } else {
            console.error('❌ Failed to load matches:', response.status, response.statusText);
            const errorText = await response.text();
            console.error('❌ Error response body:', errorText);
            displayNoMatches(`API Fehler ${response.status}: ${response.statusText}`);
        }
    } catch (error) {
        console.error('❌ Error loading matches:', error);
        displayNoMatches(`Netzwerkfehler: ${error.message}`);
    }
}

// Make functions globally accessible
window.loadTournamentData = loadTournamentData;
window.loadMatches = loadMatches;
window.loadTournamentClasses = loadTournamentClasses;