/**
 * Debug-Skript für Match-Page URL-Parameter-Extraktion
 * Testausgaben für verschiedene URL-Formate
 */

console.log('?? [DEBUG] Testing Match-Page URL Parameter Extraction');

// Test verschiedene URL-Formate
const testUrls = [
    'http://localhost:9443/match-page.html?tournament=TOURNAMENT_123&match=uuid-abc-123',
    'http://localhost:9443/match-page.html?tournament=TOURNAMENT_123&match=456',
    'http://localhost:9443/match-page.html?tournamentId=TOURNAMENT_123&matchId=uuid-def-456',
    'http://localhost:9443/match/TOURNAMENT_123/uuid-abc-123',
    'http://localhost:9443/match/TOURNAMENT_123/789',
    'http://localhost:9443/tournament/TOURNAMENT_123?match=uuid-ghi-789'
];

function testUrlExtraction(testUrl) {
    console.log(`\n?? Testing URL: ${testUrl}`);
    
    // Simuliere URL
    const url = new URL(testUrl);
    const urlParams = new URLSearchParams(url.search);
    const pathParts = url.pathname.split('/');
    
    console.log('?? Path Parts:', pathParts);
    console.log('?? Search Params:', Object.fromEntries(urlParams));
    
    // Test extraction logic
    // Priorität 1: Query-Parameter (?tournament=ID&match=ID)
    const queryTournamentId = urlParams.get('tournament') || urlParams.get('tournamentId');
    const queryMatchId = urlParams.get('match') || urlParams.get('matchId');
    
    if (queryTournamentId && queryMatchId) {
        console.log('? Query parameters found:', { tournamentId: queryTournamentId, matchId: queryMatchId });
        return { tournamentId: queryTournamentId, matchId: queryMatchId, method: 'query' };
    }
    
    // Priorität 2: Path-Parameter (/match/tournamentId/matchId)
    if (pathParts.length >= 4 && pathParts[1] === 'match') {
        console.log('? Path parameters found:', { tournamentId: pathParts[2], matchId: pathParts[3] });
        return { tournamentId: pathParts[2], matchId: pathParts[3], method: 'path' };
    }
    
    // Priorität 3: Legacy format (/tournament/tournamentId + query match)
    if (pathParts.length >= 3 && pathParts[1] === 'tournament') {
        const legacyTournamentId = pathParts[2];
        const legacyMatchId = urlParams.get('match') || urlParams.get('matchId');
        
        if (legacyTournamentId && legacyMatchId) {
            console.log('? Legacy format found:', { tournamentId: legacyTournamentId, matchId: legacyMatchId });
            return { tournamentId: legacyTournamentId, matchId: legacyMatchId, method: 'legacy' };
        }
    }
    
    console.log('? No valid parameters found');
    return {};
}

// Teste alle URLs
testUrls.forEach(testUrl => {
    const result = testUrlExtraction(testUrl);
    
    if (result.tournamentId && result.matchId) {
        const isUUID = result.matchId.includes('-') || result.matchId.length > 10;
        console.log(`?? Result: Tournament=${result.tournamentId}, Match=${result.matchId} (${isUUID ? 'UUID' : 'Numeric'}), Method=${result.method}`);
    }
    
    console.log('?'.repeat(80));
});

console.log('\n?? [DEBUG] URL Parameter Extraction Test Complete');