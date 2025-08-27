// KO Phase Game Rules Debug Script
// Paste this into browser console to debug game rules

console.log('?? ===== KO PHASE GAME RULES DEBUG =====');

// 1. Check if game rules are loaded
console.log('?? Total Game Rules loaded:', window.gameRules?.length || 0);

if (window.gameRules && window.gameRules.length > 0) {
    // Group by match type
    const rulesByType = {};
    window.gameRules.forEach(rule => {
        const type = rule.matchType || 'Unknown';
        if (!rulesByType[type]) rulesByType[type] = [];
        rulesByType[type].push(rule);
    });
    
    console.log('?? Game Rules by Match Type:');
    Object.keys(rulesByType).forEach(type => {
        console.log(`  ${type}: ${rulesByType[type].length} rules`);
        rulesByType[type].forEach(rule => {
            console.log(`    - ${rule.name} (ID: ${rule.id}, Sets: ${rule.setsToWin}, Legs: ${rule.legsToWin})`);
        });
    });
    
    // Check for KO-specific rules
    const koRules = window.gameRules.filter(gr => 
        gr.matchType?.includes('Knockout') || 
        (gr.id || '').toString().includes('_WB_') || 
        (gr.id || '').toString().includes('_LB_')
    );
    
    console.log(`?? KO-Specific Rules found: ${koRules.length}`);
    koRules.forEach(rule => {
        console.log(`  ? ${rule.name} (${rule.id}): ${rule.setsToWin} Sets, ${rule.legsToWin} Legs`);
    });
    
} else {
    console.log('? No game rules loaded! Check if tournament data is synced.');
}

// 2. Check matches
console.log('\n?? Match Analysis:');
console.log('?? Total Matches loaded:', window.matches?.length || 0);

if (window.matches && window.matches.length > 0) {
    const matchesByType = {};
    window.matches.forEach(match => {
        const type = match.matchType || 'Unknown';
        if (!matchesByType[type]) matchesByType[type] = [];
        matchesByType[type].push(match);
    });
    
    console.log('?? Matches by Type:');
    Object.keys(matchesByType).forEach(type => {
        console.log(`  ${type}: ${matchesByType[type].length} matches`);
        
        // Check first match of each type for game rules
        const firstMatch = matchesByType[type][0];
        if (firstMatch) {
            const hasDirectRules = firstMatch.gameRulesUsed || firstMatch.gameRules;
            const gameRulesId = firstMatch.gameRulesId || firstMatch.GameRulesId;
            
            console.log(`    First match (${firstMatch.matchId || firstMatch.id}):`);
            console.log(`      Direct Rules: ${hasDirectRules ? '? Yes' : '? No'}`);
            console.log(`      Game Rules ID: ${gameRulesId || 'None'}`);
            
            if (hasDirectRules) {
                const rules = firstMatch.gameRulesUsed || firstMatch.gameRules;
                console.log(`      Rule Name: ${rules.name}`);
                console.log(`      Sets/Legs: ${rules.setsToWin}/${rules.legsToWin}`);
            }
        }
    });
    
    // Test KO match rule resolution
    const koMatches = window.matches.filter(m => 
        m.matchType?.startsWith('Knockout-') || 
        (m.matchType || '').includes('WB') || 
        (m.matchType || '').includes('LB')
    );
    
    console.log(`\n?? KO Matches Analysis: ${koMatches.length} matches`);
    
    koMatches.slice(0, 3).forEach((match, index) => {
        console.log(`\n?? KO Match ${index + 1}:`);
        console.log(`  Match ID: ${match.matchId || match.id}`);
        console.log(`  Match Type: ${match.matchType}`);
        console.log(`  Class: ${match.className} (ID: ${match.classId})`);
        console.log(`  Game Rules ID: ${match.gameRulesId}`);
        
        // Test rule resolution
        try {
            const resolvedRules = getMatchSpecificGameRules(match, match.matchType, match.classId, match.className);
            console.log(`  ? Resolved Rules: ${resolvedRules.name}`);
            console.log(`    Sets/Legs: ${resolvedRules.setsToWin}/${resolvedRules.legsToWin}`);
            console.log(`    Max Sets: ${resolvedRules.maxSets}`);
            console.log(`    Play With Sets: ${resolvedRules.playWithSets}`);
        } catch (error) {
            console.log(`  ? Rule Resolution Error: ${error.message}`);
        }
    });
    
} else {
    console.log('? No matches loaded!');
}

// 3. Test specific patterns
console.log('\n?? Testing Rule Matching Patterns:');

const testPatterns = [
    { matchType: 'Knockout-WB-Semifinal', classId: 2, expectedId: '2_WB_Semifinal' },
    { matchType: 'Knockout-WB-Final', classId: 2, expectedId: '2_WB_Final' },
    { matchType: 'Knockout-LB-LoserRound1', classId: 2, expectedId: '2_LB_LoserRound1' },
    { matchType: 'Finals', classId: 2, expectedId: '2_Finals' }
];

testPatterns.forEach(test => {
    if (typeof findPreciseMatchTypeGameRules === 'function' && window.gameRules) {
        const found = findPreciseMatchTypeGameRules(
            window.gameRules, 
            test.matchType, 
            test.classId, 
            { gameRulesId: test.expectedId }
        );
        
        console.log(`${test.matchType} (Class ${test.classId}): ${found ? 
            `? Found: ${found.name}` : 
            `? Not found (expected ID: ${test.expectedId})`}`);
    }
});

// 4. Manual test helper
console.log('\n?? Manual Test Helpers:');
console.log('// Add test KO rule:');
console.log('window.gameRules = window.gameRules || [];');
console.log('window.gameRules.push({');
console.log('  id: "2_WB_Semifinal",');
console.log('  name: "Manual Test KO Rule",');
console.log('  setsToWin: 5,');
console.log('  legsToWin: 5,');
console.log('  matchType: "Knockout-WB-Semifinal",');
console.log('  classId: 2,');
console.log('  playWithSets: true');
console.log('});');

console.log('\n// Test rule matching:');
console.log('const testMatch = window.matches.find(m => m.matchType?.includes("Knockout"));');
console.log('if (testMatch) {');
console.log('  const rules = getMatchSpecificGameRules(testMatch, testMatch.matchType, testMatch.classId, testMatch.className);');
console.log('  console.log("Test Result:", rules);');
console.log('}');

console.log('\n?? ===== DEBUG COMPLETE =====');