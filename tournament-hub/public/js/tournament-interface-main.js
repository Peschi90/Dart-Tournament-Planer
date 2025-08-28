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

// Submit result from card with unique ID and enhanced UUID support
function submitResultFromCard(uniqueCardId) {
    console.log(`🎯 [CARD_SUBMIT] Submitting result from card: ${uniqueCardId}`);
    
    // Finde die Match-Card
    const cardElement = document.getElementById(uniqueCardId);
    if (!cardElement) {
        console.error(`❌ [CARD_SUBMIT] Card element not found: ${uniqueCardId}`);
        showNotification(`❌ Fehler: Match-Card nicht gefunden!`, 'error');
        return;
    }
    
    // 🔑 ERWEITERT: Extrahiere Match-Daten inklusive UUID aus data-Attributen
    const cardData = {
        matchId: cardElement.dataset.matchId,
        matchUuid: cardElement.dataset.matchUuid || null,
        primaryMatchId: cardElement.dataset.primaryMatchId || cardElement.dataset.matchId,
        hasUuid: cardElement.dataset.hasUuid === 'true',
        classId: parseInt(cardElement.dataset.classId),
        className: cardElement.dataset.className,
        groupId: cardElement.dataset.groupId || null,
        groupName: cardElement.dataset.groupName || null,
        matchType: cardElement.dataset.matchType || 'Group',
        player1Name: cardElement.dataset.player1,
        player2Name: cardElement.dataset.player2,
        uniqueCardId: uniqueCardId
    };
    
    console.log(`🔑 [CARD_SUBMIT] Card UUID information:`, {
        matchId: cardData.matchId,
        matchUuid: cardData.matchUuid,
        primaryMatchId: cardData.primaryMatchId,
        hasUuid: cardData.hasUuid,
        finalIdentifier: cardData.hasUuid ? cardData.matchUuid : cardData.matchId
    });
    
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
    
    // 🔑 ERWEITERT: Finde entsprechendes Match-Objekt mit UUID-Priorität
    const match = window.matches.find(m => {
        // Priorität 1: UUID-basierte Suche
        if (cardData.hasUuid && cardData.matchUuid && (m.uniqueId || m.UniqueId)) {
            const matchUuid = m.uniqueId || m.UniqueId;
            if (matchUuid === cardData.matchUuid) {
                console.log(`✅ [CARD_SUBMIT] Match found by UUID: ${cardData.matchUuid}`);
                return true;
            }
        }
        
        // Priorität 2: Numerische ID-basierte Suche
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
    
    console.log(`✅ [CARD_SUBMIT] Match object found:`, {
        matchId: match.matchId || match.id,
        uniqueId: match.uniqueId || match.UniqueId,
        matchType: match.matchType || match.MatchType,
        player1: match.player1 || match.Player1,
        player2: match.player2 || match.Player2
    });
    
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

    // 🔑 ERWEITERT: Result-Objekt mit vollständigen UUID- und Game Rules-Informationen
    const result = {
        // 🔑 PRIMÄRE UUID-IDENTIFIKATION (für Match-Submission)
        matchId: cardData.hasUuid ? cardData.matchUuid : cardData.matchId,    // Verwende UUID wenn verfügbar
        uniqueId: cardData.matchUuid,                                         // UUID explizit
        numericMatchId: cardData.matchId,                                     // Numerische ID für Kompatibilität
        
        // Match-Ergebnis
        player1Sets: p1Sets,
        player1Legs: p1Legs,
        player2Sets: p2Sets,
        player2Legs: p2Legs,
        notes: notes,
        status: 'Finished',
        submittedAt: new Date().toISOString(),
        playWithSets: playWithSets,
        
        // Match-Klassifizierung
        classId: cardData.classId,
        className: cardData.className,
        groupId: cardData.groupId,
        groupName: cardData.groupName,
        matchType: cardData.matchType,
        player1Name: cardData.player1Name,
        player2Name: cardData.player2Name,
        
        // 🔑 UUID-System Metadata
        matchIdentification: {
            requestedId: cardData.matchId,
            uniqueId: cardData.matchUuid,
            numericId: cardData.matchId,
            preferredId: cardData.hasUuid ? cardData.matchUuid : cardData.matchId,
            submissionMethod: cardData.hasUuid ? 'uuid' : 'numericId',
            hasValidUuid: cardData.hasUuid
        },
        
        // 🎯 UUID-System Information for Hub
        uuidSystem: {
            enabled: true,
            version: "2.0",
            submissionMethod: cardData.hasUuid ? "uuid" : "numericId",
            preferredId: cardData.hasUuid ? cardData.matchUuid : cardData.matchId,
            allKnownIds: {
                uuid: cardData.matchUuid || null,
                numericId: cardData.matchId,
                primaryId: cardData.primaryMatchId,
                hubIdentifier: match.hubIdentifier || null
            }
        },
        
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

    console.log(`📊 [CARD_SUBMIT] ===== FINAL RESULT WITH UUID & GAME RULES =====`);
    console.log(`   🎯 Match: "${cardData.player1Name}" vs "${cardData.player2Name}"`);
    console.log(`   🆔 Match ID: ${result.matchId} (method: ${result.uuidSystem.submissionMethod})`);
    console.log(`   🔑 UUID: ${result.uniqueId || 'none'}`);
    console.log(`   🔢 Numeric ID: ${result.numericMatchId}`);
    console.log(`   🎮 Match Type: ${cardData.matchType}`);
    console.log(`   🎲 Game Rules: "${gameRule.name}" (${playWithSets ? 'mit Sets' : 'nur Legs'})`);
    console.log(`   📚 Class: "${cardData.className}" (ID: ${cardData.classId})`);
    console.log(`   📋 Group: "${cardData.groupName || 'No Group'}" (ID: ${cardData.groupId})`);
    console.log(`   🆔 Card ID: ${uniqueCardId}`);
    console.log(`   📊 Score: ${playWithSets ? `Sets ${p1Sets}-${p2Sets}, ` : ''}Legs ${p1Legs}-${p2Legs}`);
    
    // Submit Result
    const submitBtn = document.getElementById(`submitBtn_${uniqueCardId}`);
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<div class="loading-spinner"></div> Übertrage...';
    }

    // 🔑 ERWEITERT: Submit via WebSocket mit UUID-Support
    if (window.socket && window.socket.connected) {
        const socketMessage = {
            tournamentId: tournamentId,
            matchId: result.matchId,                    // Verwende preferred ID (UUID wenn verfügbar)
            uniqueId: result.uniqueId,                  // UUID explizit für Server
            numericMatchId: result.numericMatchId,      // Numerische ID für Kompatibilität
            result: result,
            classId: cardData.classId,
            className: cardData.className,
            groupId: cardData.groupId,
            groupName: cardData.groupName,
            matchType: cardData.matchType,
            submittedFromCard: uniqueCardId,
            
            // 🎯 UUID-System Information for Hub Processing
            uuidSystem: result.uuidSystem,
            matchIdentification: result.matchIdentification,
            
            // 🎮 ERWEITERT: Game Rules für Server-Side Validation
            gameRules: result.gameRules
        };
        
        console.log(`📡 [CARD_SUBMIT] Sending WebSocket message with UUID & GAME-RULES DATA:`, socketMessage);
        console.log(`🔑 [CARD_SUBMIT] Using ${result.uuidSystem.submissionMethod} submission method with ID: ${result.matchId}`);
        
        window.socket.emit('submit-match-result', socketMessage);
        
        console.log(`✅ [CARD_SUBMIT] Result sent with UUID SYSTEM & MATCH-TYPE-SPECIFIC GAME RULES!`);
        
        // Verwende die preferred ID für Status-Updates (UUID wenn verfügbar)
        const statusUpdateId = result.matchId;
        updateMatchDeliveryStatus(statusUpdateId, 'pending');
        showNotification(`🔄 Match ${statusUpdateId.substring(0, 8)}${result.uniqueId ? '... (UUID)' : ''} wird übertragen (${gameRule.name})...`, 'info');
        
        // ERWEITERT: Setup auto-refresh für Socket.IO Erfolgs-Event mit UUID-Support
        setupSocketAutoRefresh(statusUpdateId, result.uniqueId);
        
    } else {
        console.log('⚠️ WebSocket not available, using REST API fallback');
        
        // Für REST API auch preferred ID verwenden
        const apiSubmissionId = result.matchId;
        updateMatchDeliveryStatus(apiSubmissionId, 'pending');
        submitResultViaAPI(apiSubmissionId, result);
    }

    // Re-enable button after timeout
    setTimeout(() => {
        if (submitBtn) {
            submitBtn.disabled = false;
            submitBtn.innerHTML = `🎯 Ergebnis übertragen (${gameRule.name})`;
        }
    }, 5000);
    
    console.log(`📊 [CARD_SUBMIT] ===== CARD-SPECIFIC SUBMIT WITH UUID & GAME RULES COMPLETE =====`);
}

// 🔑 ERWEITERTE FUNKTION: Setup Auto-Refresh für Socket.IO Events mit UUID-Support
function setupSocketAutoRefresh(matchId, matchUuid = null) {
    if (!window.socket) return;
    
    console.log(`🔄 [AUTO_REFRESH] Setting up UUID-aware auto-refresh for match: ${matchId} (UUID: ${matchUuid || 'none'})`);
    
    // Temporärer Event-Listener für diesen spezifischen Match (unterstützt beide ID-Typen)
    const refreshHandler = (data) => {
        console.log(`🔄 [AUTO_REFRESH] Received result-submitted event:`, data);
        
        // Prüfe ob das Event zu unserem Match gehört (UUID oder numerische ID)
        const eventMatchesOurMatch = (
            (data.matchId && data.matchId === matchId) ||
            (data.uniqueId && matchUuid && data.uniqueId === matchUuid) ||
            (data.numericMatchId && data.numericMatchId == matchId)
        );
        
        if (eventMatchesOurMatch && data.success) {
            console.log(`✅ [AUTO_REFRESH] Match ${matchId} submitted successfully - triggering refresh`);
            console.log(`🔑 [AUTO_REFRESH] Match identification confirmed:`, {
                eventMatchId: data.matchId,
                eventUniqueId: data.uniqueId,
                eventNumericId: data.numericMatchId,
                ourMatchId: matchId,
                ourUuid: matchUuid
            });
            
            // Triggere Auto-Refresh nach kurzer Verzögerung
            setTimeout(async () => {
                try {
                    await refreshTournamentData();
                    const displayId = matchUuid ? `${matchUuid.substring(0, 8)}... (UUID)` : matchId;
                    showNotification(`✅ Match ${displayId} erfolgreich aktualisiert`, 'success');
                } catch (error) {
                    console.error('❌ [AUTO_REFRESH] Error during auto-refresh:', error);
                    showNotification('⚠️ Daten konnten nicht aktualisiert werden', 'warning');
                }
            }, 2000);
            
            // Entferne Event-Listener nach erfolgreichem Refresh
            window.socket.off('result-submitted', refreshHandler);
        } else if (eventMatchesOurMatch && !data.success) {
            console.warn(`⚠️ [AUTO_REFRESH] Match ${matchId} submission failed:`, data);
        }
    };
    
    // Event-Listener für diesen Match hinzufügen
    window.socket.on('result-submitted', refreshHandler);
    
    // Backup: Tournament-level update events mit UUID-Support
    const tournamentUpdateHandler = (data) => {
        const eventMatchesOurMatch = (
            data.type === 'match-result-update' && (
                (data.matchId && data.matchId === matchId) ||
                (data.uniqueId && matchUuid && data.uniqueId === matchUuid) ||
                (data.result?.matchId && data.result.matchId === matchId) ||
                (data.result?.uniqueId && matchUuid && data.result.uniqueId === matchUuid)
            )
        );
        
        if (eventMatchesOurMatch) {
            console.log(`🔄 [AUTO_REFRESH] Tournament update for match ${matchId} (UUID: ${matchUuid || 'none'}) - refreshing`);
            
            setTimeout(() => refreshTournamentData(), 1500);
            
            // Entferne Handler nach Update
            window.socket.off('tournament-match-updated', tournamentUpdateHandler);
        }
    };
    
    window.socket.on('tournament-match-updated', tournamentUpdateHandler);
    
    // Cleanup nach Timeout
    setTimeout(() => {
        window.socket.off('result-submitted', refreshHandler);
        window.socket.off('tournament-match-updated', tournamentUpdateHandler);
        console.log(`🔄 [AUTO_REFRESH] Cleaned up UUID-aware event listeners for match: ${matchId} (UUID: ${matchUuid || 'none'})`);
    }, 30000); // 30 Sekunden Timeout
}