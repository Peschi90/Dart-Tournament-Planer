// tournament-interface-match-card.js - Match card creation and handling

function createMatchCard(match) {
    const canSubmitResult = match.status !== 'Finished' && match.status !== 'finished';
    
    // Robuste Extraktion aller Match-spezifischen Daten
    console.log(`🎯 [CREATE_CARD] Creating match card for:`, {
        matchId: match.matchId || match.id || match.Id,
        classId: match.classId || match.ClassId,
        className: match.className || match.ClassName,
        groupName: match.groupName || match.GroupName,
        matchType: match.matchType || match.MatchType || 'Group',
        player1: match.player1 || match.Player1,
        player2: match.player2 || match.Player2
    });
    
    // Handle different player name formats
    let player1Name = 'Spieler 1';
    let player2Name = 'Spieler 2';
    
    if (match.player1) {
        if (typeof match.player1 === 'string') {
            player1Name = match.player1;
        } else if (match.player1.name) {
            player1Name = match.player1.name;
        } else if (match.player1.Name) {
            player1Name = match.player1.Name;
        }
    } else if (match.Player1) {
        if (typeof match.Player1 === 'string') {
            player1Name = match.Player1;
        } else if (match.Player1.Name) {
            player1Name = match.Player1.Name;
        }
    }
    
    if (match.player2) {
        if (typeof match.player2 === 'string') {
            player2Name = match.player2;
        } else if (match.player2.name) {
            player2Name = match.player2.name;
        } else if (match.player2.Name) {
            player2Name = match.player2.Name;
        }
    } else if (match.Player2) {
        if (typeof match.Player2 === 'string') {
            player2Name = match.Player2;
        } else if (match.Player2.Name) {
            player2Name = match.Player2.Name;
        }
    }
    
    const matchId = match.matchId || match.id || match.Id || 'Unknown';
    const p1Sets = match.player1Sets || match.Player1Sets || 0;
    const p2Sets = match.player2Sets || match.Player2Sets || 0;
    const p1Legs = match.player1Legs || match.Player1Legs || 0;
    const p2Legs = match.player2Legs || match.Player2Legs || 0;
    const notes = match.notes || match.Notes || '';
    const status = match.status || match.Status || 'NotStarted';
    const matchType = match.matchType || match.MatchType || 'Group';
    
    const classId = match.classId || match.ClassId || 1;
    
    let className = match.className || match.ClassName || null;
    if (!className) {
        const foundClass = window.tournamentClasses.find(c => c.id == classId);
        className = foundClass?.name || `Klasse ${classId}`;
    }
    
    const groupName = match.groupName || match.GroupName || null;
    const groupId = match.groupId || match.GroupId || null;
    
    // 🎮 ERWEITERTE GAME RULES VERARBEITUNG FÜR ALLE MATCH-TYPES
    let gameRule = getMatchSpecificGameRules(match, matchType, classId, className);
    
    // Extrahiere Game Rule Properties mit Fallbacks
    const playWithSets = gameRule.playWithSets !== false;
    const setsToWin = gameRule.setsToWin || 3;
    const legsToWin = gameRule.legsToWin || 3;
    const maxSets = gameRule.maxSets || 5;
    const maxLegs = gameRule.maxLegsPerSet || gameRule.legsPerSet || 5;
    const gamePoints = gameRule.gamePoints || 501;
    const gameMode = gameRule.gameMode || 'Standard';
    const finishMode = gameRule.finishMode || 'DoubleOut';
    
    console.log(`🎮 [CREATE_CARD] Match ${matchId} Game Rules:`, {
        name: gameRule.name,
        matchType,
        playWithSets,
        gamePoints,
        setsToWin,
        legsToWin,
        finishMode
    });
    
    // Eindeutige ID für Match-Card
    const uniqueCardId = `match_${matchId}_class_${classId}_type_${matchType.replace(/[^a-zA-Z0-9]/g, '')}_group_${groupId || 'none'}_${player1Name.replace(/\s+/g, '')}_${player2Name.replace(/\s+/g, '')}`;
    
    // Match-Type Display
    const getMatchTypeDisplay = (type) => {
        const matchTypeIndicator = {
            'Group': '🔸 Gruppe',
            'Finals': '🏆 Finale',
            'Knockout-WB-Best64': '⚡ K.O. Beste 64',
            'Knockout-WB-Best32': '⚡ K.O. Beste 32', 
            'Knockout-WB-Best16': '⚡ K.O. Beste 16',
            'Knockout-WB-Quarterfinal': '⚡ K.O. Viertelfinale',
            'Knockout-WB-Semifinal': '⚡ K.O. Halbfinale',
            'Knockout-WB-Final': '🏆 K.O. Finale',
            'Knockout-WB-GrandFinal': '🏆 K.O. Grand Final',
            'Knockout-LB-LoserRound1': '🔄 K.O. Loser Runde 1',
            'Knockout-LB-LoserRound2': '🔄 K.O. Loser Runde 2',
            'Knockout-LB-LoserRound3': '🔄 K.O. Loser Runde 3',
            'Knockout-LB-LoserRound4': '🔄 K.O. Loser Runde 4',
            'Knockout-LB-LoserRound5': '🔄 K.O. Loser Runde 5',
            'Knockout-LB-LoserRound6': '🔄 K.O. Loser Runde 6',
            'Knockout-LB-LoserFinal': '🔄 K.O. Loser Final'
        };
        
        if (matchTypeIndicator[type]) {
            return matchTypeIndicator[type];
        }
        
        if (type.startsWith('Knockout-WB')) {
            if (type.includes('Final')) return '🏆 K.O. Finale';
            return '⚡ K.O. Winner Bracket';
        }
        
        if (type.startsWith('Knockout-LB')) {
            if (type.includes('LoserFinal')) return '🔄 K.O. Loser Final';
            return '🔄 K.O. Loser Bracket';
        }
        
        if (type === 'Finals') return '🏆 Finalrunde';
        if (type === 'Group') return '🔸 Gruppe';
        
        return `🎯 ${type.replace('Knockout-', 'K.O. ').replace('WB', 'Winner').replace('LB', 'Loser')}`;
    };
    
    const classColors = {
        1: 'linear-gradient(135deg, #8B5CF6 0%, #A78BFA 100%)',
        2: 'linear-gradient(135deg, #F59E0B 0%, #FCD34D 100%)',
        3: 'linear-gradient(135deg, #6B7280 0%, #9CA3AF 100%)',
        4: 'linear-gradient(135deg, #92400E 0%, #D97706 100%)'
    };
    
    const classColor = classColors[classId] || 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';

    // 🎯 ERWEITERTE GAME RULES ANZEIGE MIT MATCH-TYPE SPEZIFISCHEN INFORMATIONEN
    const gameRulesDisplay = createGameRulesDisplay(gameRule, matchType, className);

    return `
        <div class="match-card" 
             id="${uniqueCardId}"
             data-match-id="${matchId}" 
             data-class-id="${classId}" 
             data-class-name="${className}" 
             data-group-name="${groupName || ''}" 
             data-group-id="${groupId || ''}"
             data-match-type="${matchType}"
             data-player1="${player1Name}"
             data-player2="${player2Name}"
             data-unique-card-id="${uniqueCardId}"
             data-game-rules='${JSON.stringify(gameRule).replace(/'/g, "&apos;")}'>
            <div class="match-header">
                <div class="match-id">Match ${matchId}</div>
                <div class="match-status status-${status.toLowerCase()}">
                    ${getStatusText(status)}
                </div>
            </div>
            
            <div style="text-align: center; margin-bottom: 15px; padding: 12px; background: ${classColor}; color: white; border-radius: 8px; font-size: 1em; font-weight: bold; box-shadow: 0 4px 12px rgba(0,0,0,0.2); position: relative; overflow: hidden;">
                <div style="position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: rgba(255,255,255,0.1); backdrop-filter: blur(1px);"></div>
                <div style="position: relative; z-index: 1;">
                    🏆 ${className}
                    <div style="font-size: 0.85em; margin-top: 4px; opacity: 0.95;">
                        ${getMatchTypeDisplay(matchType)}
                    </div>
                    ${groupName ? `<div style="font-size: 0.8em; margin-top: 2px; opacity: 0.9;">📋 ${groupName}</div>` : ''}
                </div>
            </div>
            
            ${gameRulesDisplay}
            
            <div class="players">
                <div class="player-row">
                    <div class="player-name">${player1Name}</div>
                    <div class="player-scores">
                        ${playWithSets ? `
                            <div class="score-item">
                                <span class="score-label">Sets</span>
                                <span class="score-value">${p1Sets}</span>
                            </div>
                        ` : '' }
                        <div class="score-item">
                            <span class="score-label">Legs</span>
                            <span class="score-value">${p1Legs}</span>
                        </div>
                    </div>
                </div>
                <div class="player-row">
                    <div class="player-name">${player2Name}</div>
                    <div class="player-scores">
                        ${playWithSets ? `
                            <div class="score-item">
                                <span class="score-label">Sets</span>
                                <span class="score-value">${p2Sets}</span>
                            </div>
                        ` : '' }
                        <div class="score-item">
                            <span class="score-label">Legs</span>
                            <span class="score-value">${p2Legs}</span>
                        </div>
                    </div>
                </div>
            </div>

            ${canSubmitResult ? `
                <div class="result-form">
                    <h4 style="margin-bottom: 15px; color: #4a5568;">🎯 Ergebnis eingeben für: ${gameRule.name}</h4>
                    
                    ${playWithSets ? `
                        <div class="form-row">
                            <div class="input-group">
                                <label>${player1Name} - Sets (max ${maxSets}, benötigt: ${setsToWin})</label>
                                <input type="number" min="0" max="${maxSets}" value="${p1Sets}" 
                                       id="p1Sets_${uniqueCardId}" onchange="validateMatchResult('${uniqueCardId}')"
                                       title="Sets für ${player1Name} (${setsToWin} Sets zum Sieg)">
                            </div>
                            <div class="input-group">
                                <label>${player2Name} - Sets (max ${maxSets}, benötigt: ${setsToWin})</label>
                                <input type="number" min="0" max="${maxSets}" value="${p2Sets}" 
                                       id="p2Sets_${uniqueCardId}" onchange="validateMatchResult('${uniqueCardId}')"
                                       title="Sets für ${player2Name} (${setsToWin} Sets zum Sieg)">
                            </div>
                        </div>
                    ` : `
                        <div style="background: #fff3cd; color: #856404; padding: 12px; border-radius: 8px; margin-bottom: 15px; font-size: 0.9em; border: 1px solid #ffeaa7;">
                            <strong>📊 NUR LEGS-MODUS:</strong> Dieses Match verwendet die Regel "${gameRule.name}" - nur Legs werden gezählt (keine Sets)
                        </div>
                    `}

                    <div class="form-row">
                        <div class="input-group">
                            <label>${player1Name} - Legs${playWithSets ? ` (max ${maxLegs}/Set)` : ` (benötigt: ${legsToWin})`}</label>
                            <input type="number" min="0" max="${playWithSets ? maxLegs : 99}" value="${p1Legs}" 
                                   id="p1Legs_${uniqueCardId}" onchange="validateMatchResult('${uniqueCardId}')"
                                   title="Legs für ${player1Name} (${legsToWin} Legs zum Sieg)">
                        </div>
                        <div class="input-group">
                            <label>${player2Name} - Legs${playWithSets ? ` (max ${maxLegs}/Set)` : ` (benötigt: ${legsToWin})`}</label>
                            <input type="number" min="0" max="${playWithSets ? maxLegs : 99}" value="${p2Legs}" 
                                   id="p2Legs_${uniqueCardId}" onchange="validateMatchResult('${uniqueCardId}')"
                                   title="Legs für ${player2Name} (${legsToWin} Legs zum Sieg)">
                        </div>
                    </div>

                    <div class="input-group">
                        <label>Notizen (optional)</label>
                        <textarea rows="2" placeholder="Match-Details, besondere Ereignisse..." 
                                 id="notes_${uniqueCardId}">${notes}</textarea>
                    </div>

                    <div id="validationMessage_${uniqueCardId}" style="display: none; padding: 12px; margin: 10px 0; border-radius: 8px; font-size: 0.9em; font-weight: bold;"></div>

                    <button class="submit-button" onclick="submitResultFromCard('${uniqueCardId}')" 
                            id="submitBtn_${uniqueCardId}">
                        🎯 Ergebnis übertragen (${gameRule.name})
                    </button>

                    <div class="message" id="message_${uniqueCardId}"></div>
                </div>
            ` : `
                <div style="text-align: center; padding: 20px; background: #f0f8ff; border-radius: 10px; margin-top: 15px; border: 2px solid #e6f3ff;">
                    <strong style="color: #2b6cb0; font-size: 1.1em;">✅ Match abgeschlossen</strong>
                    <div style="margin-top: 8px; color: #4a90b8; font-size: 0.9em;">
                        Regeln: ${gameRule.name} • Klasse: ${className}
                    </div>
                    ${notes ? `<p style="margin-top: 12px; font-size: 0.9em; color: #666; padding: 8px; background: rgba(255,255,255,0.7); border-radius: 6px;">"${notes}"</p>` : '' }
                </div>
            `}
        </div>
    `;
}

// 🎮 NEUE FUNKTION: Match-spezifische Game Rules ermitteln
function getMatchSpecificGameRules(match, matchType, classId, className) {
    console.log(`🎮 [GAME_RULES] Getting game rules for match type: ${matchType}, class: ${classId}`);
    
    // 1. Direkt vom Match (höchste Priorität)
    if (match.gameRules || match.GameRules) {
        const rules = match.gameRules || match.GameRules;
        console.log(`🎮 [GAME_RULES] Using direct match rules:`, rules);
        return rules;
    }
    
    // 2. Match-Type spezifische Regeln aus globalen Game Rules
    if (window.gameRules && window.gameRules.length > 0) {
        // Suche nach rundenspezifischen Regeln
        let matchSpecificRule = findRoundSpecificGameRules(window.gameRules, matchType, classId);
        
        if (matchSpecificRule) {
            console.log(`🎮 [GAME_RULES] Using round-specific rules for ${matchType}:`, matchSpecificRule);
            return matchSpecificRule;
        }
        
        // Fallback: Klassen-Standard-Regeln
        const classRule = window.gameRules.find(gr => 
            (gr.classId || gr.ClassId) === classId || 
            (gr.id || gr.Id) === classId
        );
        
        if (classRule) {
            console.log(`🎮 [GAME_RULES] Using class default rules:`, classRule);
            return classRule;
        }
        
        // Fallback: Erste verfügbare Regel
        const fallbackRule = window.gameRules[0];
        console.log(`🎮 [GAME_RULES] Using fallback rule:`, fallbackRule);
        return fallbackRule;
    }
    
    // 3. Erstelle intelligente Default-Regeln basierend auf Match-Type
    const intelligentDefaults = createIntelligentDefaultGameRules(matchType, classId, className);
    console.log(`🎮 [GAME_RULES] Using intelligent defaults for ${matchType}:`, intelligentDefaults);
    return intelligentDefaults;
}

// 🎯 NEUE FUNKTION: Rundenspezifische Game Rules finden
function findRoundSpecificGameRules(gameRules, matchType, classId) {
    // KO-Phase spezifische Regelsuche
    if (matchType.startsWith('Knockout-')) {
        const roundName = extractRoundFromMatchType(matchType);
        
        // Suche nach expliziten Rundenregeln
        const roundSpecific = gameRules.find(gr => {
            const ruleName = (gr.name || '').toLowerCase();
            const roundKey = roundName.toLowerCase();
            return (gr.classId || gr.ClassId) === classId && 
                   (ruleName.includes(roundKey) || 
                    ruleName.includes(matchType.toLowerCase()) ||
                    (gr.matchType && gr.matchType === matchType));
        });
        
        if (roundSpecific) return roundSpecific;
        
        // Bracket-spezifische Regeln (Winner vs Loser Bracket)
        if (matchType.includes('-WB-')) {
            const winnerBracketRule = gameRules.find(gr => 
                (gr.classId || gr.ClassId) === classId && 
                ((gr.name || '').toLowerCase().includes('winner') || 
                 (gr.name || '').toLowerCase().includes('gewinner'))
            );
            if (winnerBracketRule) return winnerBracketRule;
        }
        
        if (matchType.includes('-LB-')) {
            const loserBracketRule = gameRules.find(gr => 
                (gr.classId || gr.ClassId) === classId && 
                ((gr.name || '').toLowerCase().includes('loser') || 
                 (gr.name || '').toLowerCase().includes('verlierer'))
            );
            if (loserBracketRule) return loserBracketRule;
        }
    }
    
    // Finals-spezifische Regeln
    if (matchType === 'Finals') {
        const finalsRule = gameRules.find(gr => 
            (gr.classId || gr.ClassId) === classId && 
            ((gr.name || '').toLowerCase().includes('final') || 
             (gr.name || '').toLowerCase().includes('finale'))
        );
        if (finalsRule) return finalsRule;
    }
    
    return null;
}

// 🎮 NEUE FUNKTION: Runde aus Match-Type extrahieren
function extractRoundFromMatchType(matchType) {
    if (matchType.includes('Best64')) return 'Best64';
    if (matchType.includes('Best32')) return 'Best32';
    if (matchType.includes('Best16')) return 'Best16';
    if (matchType.includes('Quarterfinal')) return 'Quarterfinal';
    if (matchType.includes('Semifinal')) return 'Semifinal';
    if (matchType.includes('GrandFinal')) return 'GrandFinal';
    if (matchType.includes('Final')) return 'Final';
    if (matchType.includes('LoserRound')) {
        const match = matchType.match(/LoserRound(\d+)/);
        return match ? `LoserRound${match[1]}` : 'LoserRound';
    }
    return 'Standard';
}

// 🎲 NEUE FUNKTION: Intelligente Standard-Game-Rules erstellen
function createIntelligentDefaultGameRules(matchType, classId, className) {
    const baseRules = {
        id: `${classId}_${matchType}`,
        classId: classId,
        className: className,
        gamePoints: 501,
        gameMode: 'Standard',
        finishMode: 'DoubleOut',
        playWithSets: true,
        maxSets: 5,
        maxLegsPerSet: 5
    };
    
    // Match-Type spezifische Anpassungen
    if (matchType === 'Group') {
        return {
            ...baseRules,
            name: `${className} Gruppenphase`,
            setsToWin: 3,
            legsToWin: 3,
            legsPerSet: 5
        };
    }
    
    if (matchType === 'Finals') {
        return {
            ...baseRules,
            name: `${className} Finalrunde`,
            setsToWin: 3,
            legsToWin: 3,
            legsPerSet: 5
        };
    }
    
    // KO Winner Bracket - je höher die Runde, desto mehr Sets/Legs
    if (matchType.startsWith('Knockout-WB-')) {
        if (matchType.includes('GrandFinal')) {
            return {
                ...baseRules,
                name: `${className} Grand Final`,
                setsToWin: 5,
                legsToWin: 5,
                legsPerSet: 7
            };
        }
        if (matchType.includes('Final')) {
            return {
                ...baseRules,
                name: `${className} KO Finale`,
                setsToWin: 4,
                legsToWin: 4,
                legsPerSet: 6
            };
        }
        if (matchType.includes('Semifinal')) {
            return {
                ...baseRules,
                name: `${className} KO Halbfinale`,
                setsToWin: 3,
                legsToWin: 4,
                legsPerSet: 5
            };
        }
        if (matchType.includes('Quarterfinal')) {
            return {
                ...baseRules,
                name: `${className} KO Viertelfinale`,
                setsToWin: 3,
                legsToWin: 3,
                legsPerSet: 5
            };
        }
        // Best of Rounds
        if (matchType.includes('Best16')) {
            return {
                ...baseRules,
                name: `${className} KO Beste 16`,
                setsToWin: 3,
                legsToWin: 3,
                legsPerSet: 5
            };
        }
        if (matchType.includes('Best32')) {
            return {
                ...baseRules,
                name: `${className} KO Beste 32`,
                setsToWin: 2,
                legsToWin: 3,
                legsPerSet: 4
            };
        }
        if (matchType.includes('Best64')) {
            return {
                ...baseRules,
                name: `${className} KO Beste 64`,
                setsToWin: 2,
                legsToWin: 3,
                legsPerSet: 3
            };
        }
        
        return {
            ...baseRules,
            name: `${className} KO Winner`,
            setsToWin: 3,
            legsToWin: 3,
            legsPerSet: 5
        };
    }
    
    // KO Loser Bracket - meist kürzere Spiele
    if (matchType.startsWith('Knockout-LB-')) {
        if (matchType.includes('LoserFinal')) {
            return {
                ...baseRules,
                name: `${className} Loser Final`,
                setsToWin: 3,
                legsToWin: 4,
                legsPerSet: 5
            };
        }
        
        return {
            ...baseRules,
            name: `${className} KO Loser`,
            setsToWin: 2,
            legsToWin: 3,
            legsPerSet: 4
        };
    }
    
    // Fallback
    return {
        ...baseRules,
        name: `${className} Standard`,
        setsToWin: 3,
        legsToWin: 3,
        legsPerSet: 5
    };
}

// 🎨 NEUE FUNKTION: Game Rules Display erstellen
function createGameRulesDisplay(gameRule, matchType, className) {
    const playWithSets = gameRule.playWithSets !== false;
    const gamePoints = gameRule.gamePoints || 501;
    const gameMode = gameRule.gameMode || 'Standard';
    const finishMode = gameRule.finishMode || 'DoubleOut';
    const setsToWin = gameRule.setsToWin || 3;
    const legsToWin = gameRule.legsToWin || 3;
    
    // Match-Type spezifische Styling
    let backgroundColor = '#f0f8ff';
    let borderColor = '#b3d9ff';
    let textColor = '#2b6cb0';
    
    if (matchType.startsWith('Knockout-WB-')) {
        backgroundColor = '#f0fff4';
        borderColor = '#9ae6b4';
        textColor = '#22543d';
    } else if (matchType.startsWith('Knockout-LB-')) {
        backgroundColor = '#fffaf0';
        borderColor = '#fbd38d';
        textColor = '#c05621';
    } else if (matchType === 'Finals') {
        backgroundColor = '#fef5e7';
        borderColor = '#f6e05e';
        textColor = '#d69e2e';
    }
    
    return `
        <div style="text-align: center; margin-bottom: 15px; padding: 15px; background: linear-gradient(135deg, ${backgroundColor} 0%, ${backgroundColor}dd 100%); border: 2px solid ${borderColor}; border-radius: 10px; font-size: 0.95em; color: ${textColor}; box-shadow: 0 3px 10px rgba(0,0,0,0.1);">
            <div style="display: flex; align-items: center; justify-content: center; gap: 8px; margin-bottom: 8px;">
                🎮 <strong style="font-size: 1.1em;">${gameRule.name || 'Standard Regeln'}</strong>
                <span style="background: ${textColor}; color: white; padding: 3px 10px; border-radius: 15px; font-size: 0.8em; font-weight: bold;">${className}</span>
            </div>
            <div style="font-size: 0.9em; color: ${textColor}aa; display: flex; justify-content: center; gap: 15px; flex-wrap: wrap; margin-bottom: 6px;">
                <span title="Spielmodus & Punkte">🎯 ${gamePoints} ${gameMode}</span>
                ${playWithSets ? `<span title="Sets zum Sieg" style="font-weight: bold;">🏆 ${setsToWin} Sets</span>` : '' }
                <span title="Legs zum Sieg" style="font-weight: bold;">📊 ${legsToWin} Legs</span>
                <span title="Finish-Modus">🏁 ${finishMode}</span>
            </div>
            ${!playWithSets ? `<div style="font-size: 0.85em; color: #e68900; margin-top: 6px; font-weight: bold; padding: 4px 8px; background: rgba(230,137,0,0.1); border-radius: 6px;">⚠️ NUR LEGS (keine Sets)</div>` : '' }
            ${matchType.startsWith('Knockout-') ? `<div style="font-size: 0.8em; margin-top: 6px; opacity: 0.8;">⚔️ Knockout-Spezial-Regeln</div>` : '' }
        </div>
    `;
}