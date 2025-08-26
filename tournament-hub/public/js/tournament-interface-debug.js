// tournament-interface-debug.js - Debug and testing functions

function debugMatches() {
    console.log('🔧 ===== COMPREHENSIVE DEBUG: Tournament Interface State =====');
    console.log('🔧 Current URL:', window.location.href);
    console.log('🔧 Tournament ID:', window.tournamentId);
    console.log('🔧 Current Tournament:', window.currentTournament);
    console.log('🔧 Matches Count:', window.matches?.length || 0);
    console.log('🔧 Tournament Classes Count:', window.tournamentClasses?.length || 0);
    console.log('🔧 Game Rules Count:', window.gameRules?.length || 0);
    console.log('🔧 Current Class ID:', window.currentClassId);
    console.log('🔧 Socket Connected:', window.socket?.connected);
    console.log('🔧 Socket ID:', window.socket?.id);
    
    // Test DOM elements
    console.log('🔧 DOM Elements Check:');
    const elements = [
        'tournamentName', 'tournamentMeta', 'connectionIndicator', 
        'connectionText', 'classSelector', 'classSelect', 'loadClassMatches', 'matchContainer'
    ];
    
    elements.forEach(id => {
        const element = document.getElementById(id);
        console.log(`   ${id}: ${element ? '✅' : '❌'} ${element ? 'Found' : 'Missing'}`);
    });
    
    // Test API endpoints
    console.log('🔧 Testing API endpoints...');
    testApiEndpoints();
    
    // Show current data
    if (window.matches && window.matches.length > 0) {
        console.log('🔧 Sample Match Data:', window.matches[0]);
    }
    
    if (window.tournamentClasses && window.tournamentClasses.length > 0) {
        console.log('🔧 Sample Class Data:', window.tournamentClasses[0]);
    }
    
    if (window.gameRules && window.gameRules.length > 0) {
        console.log('🔧 Sample Game Rules:', window.gameRules[0]);
    }
    
    // Test notification
    showNotification('🔧 Debug-Informationen in der Browser-Konsole geloggt', 'info');
    
    console.log('🔧 ===== DEBUG COMPLETE =====');
}

async function testApiEndpoints() {
    const endpoints = [
        `/api/tournaments/${window.tournamentId}`,
        `/api/tournaments/${window.tournamentId}/matches`,
        `/api/tournaments/${window.tournamentId}/classes`
    ];
    
    for (const endpoint of endpoints) {
        try {
            console.log(`🧪 Testing endpoint: ${endpoint}`);
            const response = await fetch(endpoint);
            console.log(`   Status: ${response.status} ${response.statusText}`);
            
            if (response.ok) {
                const data = await response.json();
                console.log(`   Data type: ${Array.isArray(data) ? 'Array' : typeof data}`);
                console.log(`   Data size: ${Array.isArray(data) ? data.length : Object.keys(data || {}).length} items/properties`);
            } else {
                const errorText = await response.text();
                console.log(`   Error: ${errorText}`);
            }
        } catch (error) {
            console.log(`   Exception: ${error.message}`);
        }
    }
}

// Window-Level Match Data Validation
window.validateMatchData = function() {
    console.log('🔍 [VALIDATION] Starting comprehensive match data validation...');
    
    if (!window.matches || window.matches.length === 0) {
        console.warn('⚠️ [VALIDATION] No matches available for validation');
        return;
    }
    
    let validationIssues = 0;
    
    window.matches.forEach((match, index) => {
        const matchId = match.matchId || match.id || match.Id || `Unknown-${index}`;
        console.log(`🔍 [VALIDATION] Validating match ${index + 1}/${window.matches.length}: ID=${matchId}`);
        
        // Validate required fields
        const requiredFields = [
            { key: 'classId', alternatives: ['ClassId'], name: 'Class ID' },
            { key: 'className', alternatives: ['ClassName'], name: 'Class Name' },
            { key: 'player1', alternatives: ['Player1'], name: 'Player 1' },
            { key: 'player2', alternatives: ['Player2'], name: 'Player 2' }
        ];
        
        requiredFields.forEach(field => {
            let value = match[field.key];
            if (!value) {
                field.alternatives.forEach(alt => {
                    if (!value && match[alt]) value = match[alt];
                });
            }
            
            if (!value) {
                console.warn(`⚠️ [VALIDATION] Match ${matchId}: Missing ${field.name}`);
                validationIssues++;
            }
        });
        
        // Match-Type specific validation
        const matchType = match.matchType || match.MatchType || 'Group';
        if (matchType === 'Group') {
            const groupName = match.groupName || match.GroupName;
            if (!groupName) {
                console.warn(`⚠️ [VALIDATION] Match ${matchId}: Group match missing group name`);
                validationIssues++;
            }
        }
        
        console.log(`✅ [VALIDATION] Match ${matchId} validation complete`);
    });
    
    if (validationIssues === 0) {
        console.log(`✅ [VALIDATION] All ${window.matches.length} matches passed validation!`);
        showNotification(`✅ Alle ${window.matches.length} Matches sind korrekt validiert`, 'success');
    } else {
        console.warn(`⚠️ [VALIDATION] Found ${validationIssues} validation issues in ${window.matches.length} matches`);
        showNotification(`⚠️ ${validationIssues} Validierungsprobleme gefunden - siehe Konsole`, 'warning');
    }
    
    return { totalMatches: window.matches.length, issues: validationIssues };
};

// Global debug functions
window.debugTournament = debugMatches;
window.testApis = testApiEndpoints;
window.reloadData = function() { if (window.loadTournamentData) window.loadTournamentData(); };
window.reloadMatches = function() { if (window.loadMatches) window.loadMatches(); };
window.showState = function() {
    return {
        tournamentId: window.tournamentId,
        currentTournament: window.currentTournament,
        matches: window.matches?.length || 0,
        tournamentClasses: window.tournamentClasses?.length || 0,
        gameRules: window.gameRules?.length || 0,
        currentClassId: window.currentClassId,
        socketConnected: window.socket?.connected,
        socketId: window.socket?.id
    };
};