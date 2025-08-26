// tournament-interface-main.js - Main initialization and event handlers

// Get tournament ID from URL
const pathParts = window.location.pathname.split('/');
if (pathParts.length >= 3 && pathParts[1] === 'tournament') {
    window.tournamentId = pathParts[2];
    tournamentId = window.tournamentId;
}

if (!tournamentId) {
    window.location.href = '/join';
}

// Event-Handler für Klassen-Auswahl
document.addEventListener('DOMContentLoaded', function() {
    console.log('🎯 Tournament Interface loading for:', tournamentId);
    console.log('🔍 Current URL:', window.location.href);
    console.log('🔍 Tournament ID extracted:', tournamentId);
    
    // Check if all required elements exist
    const requiredElements = [
        'tournamentName', 'tournamentMeta', 'connectionIndicator', 
        'connectionText', 'classSelector', 'classSelect', 'loadClassMatches', 'matchContainer'
    ];
    
    const missingElements = requiredElements.filter(id => !document.getElementById(id));
    if (missingElements.length > 0) {
        console.error('❌ Missing required elements:', missingElements);
    } else {
        console.log('✅ All required DOM elements found');
    }
    
    // Initialize connections
    initializeSocket();
    
    // Load data with delay to ensure DOM is ready
    setTimeout(() => {
        console.log('🔄 Starting tournament data loading...');
        loadTournamentData();
    }, 100);
    
    // Klassen-Event-Handler
    const loadClassButton = document.getElementById('loadClassMatches');
    const classSelect = document.getElementById('classSelect');
    
    if (loadClassButton) {
        loadClassButton.addEventListener('click', function() {
            window.currentClassId = classSelect.value || null;
            currentClassId = window.currentClassId;
            console.log('🎯 Loading matches for class:', currentClassId || 'All');
            loadMatches();
        });
    } else {
        console.warn('⚠️ Load class button not found');
    }
    
    if (classSelect) {
        classSelect.addEventListener('change', function() {
            window.currentClassId = this.value || null;
            currentClassId = window.currentClassId;
            console.log('📚 Class changed to:', currentClassId || 'All');
            // Auto-load on change
            loadMatches();
        });
    } else {
        console.warn('⚠️ Class select element not found');
    }
    
    console.log('✅ Tournament Interface initialization complete');
});

// Submit result from card with unique ID
function submitResultFromCard(uniqueCardId) {
    console.log(`🎯 [CARD_SUBMIT] Submitting result from card: ${uniqueCardId}`);
    
    // Finde die Match-Card
    const cardElement = document.getElementById(uniqueCardId);
    if (!cardElement) {
        console.error(`❌ [CARD_SUBMIT] Card element not found: ${uniqueCardId}`);
        showNotification(`❌ Fehler: Match-Card nicht gefunden!`, 'error');
        return;
    }
    
    // Extrahiere Match-Daten aus data-Attributen
    const cardData = {
        matchId: cardElement.dataset.matchId,
        classId: parseInt(cardElement.dataset.classId),
        className: cardElement.dataset.className,
        groupId: cardElement.dataset.groupId || null,
        groupName: cardElement.dataset.groupName || null,
        matchType: cardElement.dataset.matchType || 'Group',
        player1Name: cardElement.dataset.player1,
        player2Name: cardElement.dataset.player2,
        uniqueCardId: uniqueCardId
    };
    
    // 🎮 ERWEITERT: Extrahiere Game Rules direkt aus der Card
    let cardGameRules = null;
    try {
        const gameRulesData = cardElement.dataset.gameRules;
        if (gameRulesData) {
            cardGameRules = JSON.parse(gameRulesData.replace(/&apos;/g, "'"));
            console.log(`🎮 [CARD_SUBMIT] Game rules extracted from card:`, cardGameRules);
        }
    } catch (error) {
        console.warn(`⚠️ [CARD_SUBMIT] Could not parse game rules from card:`, error);
    }
    
    console.log(`📊 [CARD_SUBMIT] Card data extracted:`, cardData);
    
    // Finde entsprechendes Match-Objekt
    const match = window.matches.find(m => {
        const mId = m.matchId || m.id || m.Id;
        const mClassId = m.classId || m.ClassId;
        const mMatchType = m.matchType || m.MatchType || 'Group';
        return mId == cardData.matchId && 
               mClassId == cardData.classId && 
               mMatchType === cardData.matchType;
    });
    
    if (!match) {
        console.error(`❌ [CARD_SUBMIT] No matching match object found for card data:`, cardData);
        showNotification(`❌ Fehler: Match-Daten inkonsistent!`, 'error');
        return;
    }
    
    // 🎮 ERWEITERT: Bestimme Game Rules mit Priorität auf Card-Daten
    let gameRule = null;
    
    // 1. Priorität: Game Rules aus der Card (bereits verarbeitet)
    if (cardGameRules) {
        gameRule = cardGameRules;
        console.log(`🎮 [CARD_SUBMIT] Using card-specific game rules:`, gameRule);
    }
    // 2. Priorität: Direct match rules
    else if (match.gameRules || match.GameRules) {
        gameRule = match.gameRules || match.GameRules;
        console.log(`🎮 [CARD_SUBMIT] Using direct match game rules:`, gameRule);
    }
    // 3. Priorität: Ermittle Match-spezifische Regeln
    else {
        gameRule = getMatchSpecificGameRules(match, cardData.matchType, cardData.classId, cardData.className);
        console.log(`🎮 [CARD_SUBMIT] Using determined match-specific game rules:`, gameRule);
    }
    
    const playWithSets = gameRule.playWithSets !== false;
    
    // Extrahiere Eingabewerte
    let p1Sets = 0, p2Sets = 0;
    const p1Legs = parseInt(document.getElementById(`p1Legs_${uniqueCardId}`).value) || 0;
    const p2Legs = parseInt(document.getElementById(`p2Legs_${uniqueCardId}`).value) || 0;
    
    if (playWithSets) {
        p1Sets = parseInt(document.getElementById(`p1Sets_${uniqueCardId}`).value) || 0;
        p2Sets = parseInt(document.getElementById(`p2Sets_${uniqueCardId}`).value) || 0;
    }
    
    const notes = document.getElementById(`notes_${uniqueCardId}`).value.trim();

    console.log(`📊 [CARD_SUBMIT] Input values from card ${uniqueCardId}:`);
    console.log(`   Game Rule: "${gameRule.name}" (Type: ${cardData.matchType})`);
    console.log(`   Sets: ${p1Sets}-${p2Sets} (playWithSets: ${playWithSets})`);
    console.log(`   Legs: ${p1Legs}-${p2Legs}`);
    console.log(`   Notes: "${notes}"`);

    // 🎮 ERWEITERT: Result-Objekt mit vollständigen Game Rules Informationen
    const result = {
        matchId: cardData.matchId,
        player1Sets: p1Sets,
        player1Legs: p1Legs,
        player2Sets: p2Sets,
        player2Legs: p2Legs,
        notes: notes,
        status: 'Finished',
        submittedAt: new Date().toISOString(),
        playWithSets: playWithSets,
        classId: cardData.classId,
        className: cardData.className,
        groupId: cardData.groupId,
        groupName: cardData.groupName,
        matchType: cardData.matchType,
        player1Name: cardData.player1Name,
        player2Name: cardData.player2Name,
        // 🎮 ERWEITERT: Game Rules Information für Server-Side Processing
        gameRules: {
            name: gameRule.name,
            gamePoints: gameRule.gamePoints || 501,
            gameMode: gameRule.gameMode || 'Standard',
            finishMode: gameRule.finishMode || 'DoubleOut',
            playWithSets: playWithSets,
            setsToWin: gameRule.setsToWin || 3,
            legsToWin: gameRule.legsToWin || 3,
            legsPerSet: gameRule.legsPerSet || 5,
            maxSets: gameRule.maxSets || 5,
            maxLegsPerSet: gameRule.maxLegsPerSet || 5
        }
    };

    console.log(`📊 [CARD_SUBMIT] ===== FINAL RESULT WITH MATCH-SPECIFIC GAME RULES =====`);
    console.log(`   🎯 Match: "${cardData.player1Name}" vs "${cardData.player2Name}"`);
    console.log(`   🎮 Match Type: ${cardData.matchType}`);
    console.log(`   🎲 Game Rules: "${gameRule.name}" (${playWithSets ? 'mit Sets' : 'nur Legs'})`);
    console.log(`   📚 CARD Class: "${cardData.className}" (ID: ${cardData.classId})`);
    console.log(`   📋 CARD Group: "${cardData.groupName || 'No Group'}" (ID: ${cardData.groupId})`);
    console.log(`   🆔 Card ID: ${uniqueCardId}`);
    console.log(`   📊 Score: ${playWithSets ? `Sets ${p1Sets}-${p2Sets}, ` : ''}Legs ${p1Legs}-${p2Legs}`);
    
    // Submit Result
    const submitBtn = document.getElementById(`submitBtn_${uniqueCardId}`);
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<div class="loading-spinner"></div> Übertrage...';
    }

    // Submit via WebSocket
    if (window.socket && window.socket.connected) {
        const socketMessage = {
            tournamentId: tournamentId,
            matchId: cardData.matchId,
            result: result,
            classId: cardData.classId,
            className: cardData.className,
            groupId: cardData.groupId,
            groupName: cardData.groupName,
            matchType: cardData.matchType,
            submittedFromCard: uniqueCardId,
            // 🎮 ERWEITERT: Game Rules für Server-Side Validation
            gameRules: result.gameRules
        };
        
        console.log(`📡 [CARD_SUBMIT] Sending WebSocket message with GAME-RULES DATA:`, socketMessage);
        window.socket.emit('submit-match-result', socketMessage);
        
        console.log(`✅ [CARD_SUBMIT] Result sent with MATCH-TYPE-SPECIFIC GAME RULES!`);
        
        updateMatchDeliveryStatus(cardData.matchId, 'pending');
        showNotification(`🔄 Match ${cardData.matchId} wird übertragen (${gameRule.name})...`, 'info');
        
    } else {
        console.log('⚠️ WebSocket not available, using REST API fallback');
        updateMatchDeliveryStatus(cardData.matchId, 'pending');
        submitResultViaAPI(cardData.matchId, result);
    }

    // Re-enable button after timeout
    setTimeout(() => {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = `🎯 Ergebnis übertragen (${gameRule.name})`;
        }
    }, 5000);
    
    console.log(`📊 [CARD_SUBMIT] ===== CARD-SPECIFIC SUBMIT WITH GAME RULES COMPLETE =====`);
}

// Make functions globally accessible
window.submitResultFromCard = submitResultFromCard;