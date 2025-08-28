// tournament-interface-core.js - Core functions for tournament interface

// NEUE HILFSFUNKTIONEN für Match-Type spezifische Behandlung
function getGameRulesSuffixByMatchType(matchType) {
    if (matchType.startsWith('Knockout-WB')) return 'Winner Bracket Regeln';
    if (matchType.startsWith('Knockout-LB')) return 'Loser Bracket Regeln';
    if (matchType === 'Finals') return 'Finalrunden Regeln';
    return 'Standard Regeln';
}

function getMatchTypeDescription(matchType) {
    const descriptions = {
        'Group': 'Gruppen',
        'Finals': 'Finalrunden',
        'Knockout-WB-Best64': 'Winner Bracket Beste 64',
        'Knockout-WB-Best32': 'Winner Bracket Beste 32',
        'Knockout-WB-Best16': 'Winner Bracket Beste 16',
        'Knockout-WB-Quarterfinal': 'Winner Bracket Viertelfinale',
        'Knockout-WB-Semifinal': 'Winner Bracket Halbfinale',
        'Knockout-WB-Final': 'Winner Bracket Finale',
        'Knockout-WB-GrandFinal': 'Winner Bracket Grand Final',
        'Knockout-LB-LoserRound1': 'Loser Bracket Runde 1',
        'Knockout-LB-LoserRound2': 'Loser Bracket Runde 2',
        'Knockout-LB-LoserRound3': 'Loser Bracket Runde 3',
        'Knockout-LB-LoserRound4': 'Loser Bracket Runde 4',
        'Knockout-LB-LoserRound5': 'Loser Bracket Runde 5',
        'Knockout-LB-LoserRound6': 'Loser Bracket Runde 6',
        'Knockout-LB-LoserFinal': 'Loser Bracket Final'
    };
    
    return descriptions[matchType] || matchType.replace('Knockout-', 'K.O. ').replace('WB', 'Winner').replace('LB', 'Loser');
}

// Match-Delivery Status aktualisieren
function updateMatchDeliveryStatus(matchId, status) {
    console.log(`📊 Updating match ${matchId} delivery status to: ${status}`);
    
    const matchCards = document.querySelectorAll(`[data-match-id="${matchId}"]`);
    
    matchCards.forEach(card => {
        const statusElement = card.querySelector('.match-status');
        if (statusElement) {
            if (status === 'pending') {
                statusElement.innerHTML = '🔄 Übertrage...';
                statusElement.className = 'match-status delivery-status';
            } else if (status === 'success') {
                statusElement.innerHTML = '✅ Übertragen';
                statusElement.className = 'match-status status-delivered';
                
                // ERWEITERT: Aktualisiere UI nach erfolgreichem Submit
                setTimeout(() => {
                    statusElement.innerHTML = getStatusText('Finished');
                    statusElement.className = 'match-status status-finished';
                    
                    // Verstecke Result-Form und zeige "Abgeschlossen"
                    const resultForm = card.querySelector('.result-form');
                    if (resultForm) {
                        resultForm.style.display = 'none';
                        
                        // Erstelle und zeige "Match abgeschlossen" Anzeige
                        const completedDiv = document.createElement('div');
                        completedDiv.innerHTML = `
                            <div style="text-align: center; padding: 20px; background: #f0f8ff; border-radius: 10px; margin-top: 15px; border: 2px solid #e6f3ff;">
                                <strong style="color: #2b6cb0; font-size: 1.1em;">✅ Match abgeschlossen</strong>
                                <div style="margin-top: 8px; color: #4a90b8; font-size: 0.9em;">
                                    Ergebnis erfolgreich übertragen
                                </div>
                                <button class="match-page-button" onclick="openMatchPage('${matchId}')" 
                                        style="margin-top: 10px;" title="Zur Einzel-Match-Seite wechseln">
                                    📄 Match-Seite öffnen
                                </button>
                            </div>
                        `;
                        resultForm.parentNode.insertBefore(completedDiv, resultForm.nextSibling);
                    }
                }, 2000);
                
                // HINZUGEFÜGT: Automatisches Laden aktueller Daten nach Erfolg
                setTimeout(() => {
                    console.log('🔄 [UPDATE] Auto-refreshing match data after successful submission');
                    refreshTournamentData();
                }, 3000);
                
            } else if (status === 'error') {
                statusElement.innerHTML = '❌ Fehler';
                statusElement.className = 'match-status status-error';
                
                setTimeout(() => {
                    statusElement.innerHTML = getStatusText('NotStarted');
                    statusElement.className = 'match-status status-notstarted';
                }, 5000);
            }
        }
    });
}

// NEUE FUNKTION: Tournament-Daten nach Match-Update refreshen
async function refreshTournamentData() {
    try {
        console.log('🔄 [REFRESH] Refreshing tournament data after match update...');
        
        if (!window.tournamentId) {
            console.warn('⚠️ [REFRESH] No tournament ID available for refresh');
            return;
        }
        
        // Zeige Loading-Indicator
        showRefreshIndicator(true);
        
        // Hole aktuelle Tournament-Daten
        await loadTournamentData();
        
        // Hole aktuelle Match-Daten
        await loadMatches();
        
        // Aktualisiere Display
        if (typeof window.tournamentInterfaceDisplay !== 'undefined') {
            window.tournamentInterfaceDisplay.updateTournamentDisplay(window.currentTournament);
            window.tournamentInterfaceDisplay.displayMatches(window.currentMatches);
        }
        
        console.log('✅ [REFRESH] Tournament data refreshed successfully');
        showNotification('✅ Tournament-Daten aktualisiert', 'success');
        
    } catch (error) {
        console.error('❌ [REFRESH] Error refreshing tournament data:', error);
        showNotification('⚠️ Fehler beim Aktualisieren der Daten', 'warning');
    } finally {
        showRefreshIndicator(false);
    }
}

// NEUE FUNKTION: Refresh-Indicator anzeigen
function showRefreshIndicator(show) {
    let indicator = document.getElementById('refresh-indicator');
    
    if (show) {
        if (!indicator) {
            indicator = document.createElement('div');
            indicator.id = 'refresh-indicator';
            indicator.innerHTML = `
                <div style="position: fixed; top: 20px; right: 20px; z-index: 9999; background: rgba(255,255,255,0.95); border: 2px solid #4299e1; border-radius: 10px; padding: 15px 20px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); display: flex; align-items: center; gap: 10px;">
                    <div style="width: 20px; height: 20px; border: 2px solid #4299e1; border-top: 2px solid transparent; border-radius: 50%; animation: spin 1s linear infinite;"></div>
                    <span style="color: #4299e1; font-weight: bold;">Aktualisiere...</span>
                </div>
                <style>
                    @keyframes spin {
                        0% { transform: rotate(0deg); }
                        100% { transform: rotate(360deg); }
                    }
                </style>
            `;
            document.body.appendChild(indicator);
        }
        indicator.style.display = 'block';
    } else {
        if (indicator) {
            indicator.style.display = 'none';
            // Entferne nach kurzer Verzögerung
            setTimeout(() => {
                if (indicator && indicator.parentNode) {
                    indicator.parentNode.removeChild(indicator);
                }
            }, 500);
        }
    }
}

// ERWEITERT: Fallback API submission mit Auto-Refresh
async function submitResultViaAPI(matchId, result) {
    try {
        console.log('📡 Submitting result via REST API...');
        
        const response = await fetch(`/api/tournaments/${window.tournamentId}/matches/${matchId}/result`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(result)
        });
        
        if (response.ok) {
            const data = await response.json();
            console.log('✅ Result submitted via API:', data);
            updateMatchDeliveryStatus(matchId, 'success');
            showNotification(`✅ Match ${matchId} Ergebnis erfolgreich übertragen!`, 'success');
            
            // ERWEITERT: Intelligentes Refresh mit Retry-Logic
            setTimeout(async () => {
                try {
                    await refreshTournamentData();
                } catch (refreshError) {
                    console.warn('⚠️ [API] Refresh after API submission failed, retrying...', refreshError);
                    setTimeout(() => refreshTournamentData(), 2000);
                }
            }, 1000);
        } else {
            console.error('❌ API submission failed:', response.statusText);
            updateMatchDeliveryStatus(matchId, 'error');
            showNotification(`❌ Fehler beim Übertragen von Match ${matchId}`, 'error');
        }
    } catch (error) {
        console.error('❌ Error submitting via API:', error);
        updateMatchDeliveryStatus(matchId, 'error');
        showNotification(`❌ Netzwerkfehler: ${error.message}`, 'error');
    }
}

// Match-Validation
function validateMatchResult(uniqueCardId) {
    console.log(`🔍 Validating match result for card: ${uniqueCardId}`);
    
    const cardElement = document.getElementById(uniqueCardId);
    if (!cardElement) return;
    
    // 🎮 ERWEITERT: Extrahiere Game Rules aus der Card für präzise Validation
    let gameRules = null;
    try {
        const gameRulesData = cardElement.dataset.gameRules;
        if (gameRulesData) {
            gameRules = JSON.parse(gameRulesData.replace(/&apos;/g, "'"));
        }
    } catch (error) {
        console.warn(`⚠️ Could not parse game rules for validation:`, error);
    }
    
    const p1SetsInput = document.getElementById(`p1Sets_${uniqueCardId}`);
    const p2SetsInput = document.getElementById(`p2Sets_${uniqueCardId}`);
    const p1LegsInput = document.getElementById(`p1Legs_${uniqueCardId}`);
    const p2LegsInput = document.getElementById(`p2Legs_${uniqueCardId}`);
    const validationMessage = document.getElementById(`validationMessage_${uniqueCardId}`);
    const submitBtn = document.getElementById(`submitBtn_${uniqueCardId}`);
    
    if (!validationMessage || !submitBtn) return;
    
    // Extrahiere Game Rules Parameter für Validation
    const playWithSets = gameRules ? (gameRules.playWithSets !== false) : true;
    const setsToWin = gameRules ? (gameRules.setsToWin || 3) : 3;
    const legsToWin = gameRules ? (gameRules.legsToWin || 3) : 3;
    const maxSets = gameRules ? (gameRules.maxSets || 5) : 5;
    const maxLegs = gameRules ? (gameRules.maxLegsPerSet || 5) : 5;
    
    let isValid = true;
    let message = '';
    
    // Basic validation
    const p1Sets = parseInt(p1SetsInput?.value || '0');
    const p2Sets = parseInt(p2SetsInput?.value || '0');
    const p1Legs = parseInt(p1LegsInput?.value || '0');
    const p2Legs = parseInt(p2LegsInput?.value || '0');
    
    console.log(`🔍 Validating with game rules:`, {
        playWithSets,
        setsToWin,
        legsToWin,
        maxSets,
        maxLegs,
        currentScore: { p1Sets, p2Sets, p1Legs, p2Legs }
    });
    
    // Check for negative values
    if (p1Sets < 0 || p2Sets < 0 || p1Legs < 0 || p2Legs < 0) {
        isValid = false;
        message = '❌ Negative Werte sind nicht erlaubt';
    }
    
    // Check if both players have 0 everything
    else if (p1Sets === 0 && p2Sets === 0 && p1Legs === 0 && p2Legs === 0) {
        isValid = false;
        message = '⚠️ Bitte geben Sie ein Ergebnis ein';
    }
    
    // 🎮 ERWEITERT: Game Rules spezifische Validierung
    else if (playWithSets) {
        // Sets-basierte Validierung
        if (p1Sets > maxSets || p2Sets > maxSets) {
            isValid = false;
            message = `❌ Maximum ${maxSets} Sets erlaubt`;
        }
        else if (p1Legs > maxLegs || p2Legs > maxLegs) {
            isValid = false;
            message = `❌ Maximum ${maxLegs} Legs pro Set erlaubt`;
        }
        // Validiere Gewinner-Logik für Sets
        else if (p1Sets >= setsToWin && p2Sets >= setsToWin) {
            isValid = false;
            message = `❌ Beide Spieler können nicht ${setsToWin}+ Sets haben`;
        }
        // Spiel muss abgeschlossen sein wenn ein Spieler genug Sets hat
        else if ((p1Sets >= setsToWin || p2Sets >= setsToWin) && (p1Sets + p2Sets === 0)) {
            isValid = false;
            message = `⚠️ Spiel ist noch nicht beendet (${setsToWin} Sets zum Sieg)`;
        }
        // Check if match is actually finished
        else if (p1Sets < setsToWin && p2Sets < setsToWin && (p1Sets > 0 || p2Sets > 0 || p1Legs > 0 || p2Legs > 0)) {
            // OK - Spiel läuft noch oder ist unentschieden erlaubt
        }
    }
    else {
        // Nur-Legs Validierung
        if (p1Legs >= legsToWin && p2Legs >= legsToWin) {
            isValid = false;
            message = `❌ Beide Spieler können nicht ${legsToWin}+ Legs haben`;
        }
        else if (p1Legs < legsToWin && p2Legs < legsToWin && (p1Legs > 0 || p2Legs > 0)) {
            // OK - Spiel läuft noch
        }
    }
    
    // 🎮 ERWEITERT: Match-Type spezifische Validierung
    if (isValid && gameRules && gameRules.name) {
        const matchType = cardElement.dataset.matchType || 'Group';
        
        // KO-Spiele müssen einen eindeutigen Gewinner haben
        if (matchType.startsWith('Knockout-') && isValid) {
            if (playWithSets) {
                if (p1Sets === p2Sets && p1Sets > 0) {
                    isValid = false;
                    message = '⚔️ KO-Spiele können nicht unentschieden enden';
                }
            } else {
                if (p1Legs === p2Legs && p1Legs > 0) {
                    isValid = false;
                    message = '⚔️ KO-Spiele können nicht unentschieden enden';
                }
            }
        }
    }
    
    if (isValid) {
        validationMessage.style.display = 'none';
        submitBtn.disabled = false;
    } else {
        validationMessage.textContent = message;
        validationMessage.style.display = 'block';
        validationMessage.style.background = '#fed7d7';
        validationMessage.style.color = '#c53030';
        submitBtn.disabled = true;
    }
    
    return isValid;
}

function getStatusText(status) {
    const statusTexts = {
        'NotStarted': 'Ausstehend',
        'InProgress': 'Läuft', 
        'Finished': 'Beendet',
        'notstarted': 'Ausstehend',
        'inprogress': 'Läuft',
        'finished': 'Beendet'
    };
    return statusTexts[status] || status;
}