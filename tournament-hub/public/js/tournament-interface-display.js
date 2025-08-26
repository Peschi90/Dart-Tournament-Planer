// tournament-interface-display.js - Display and UI functions

function updateTournamentInfo(tournament) {
    console.log('📋 ===== UPDATING TOURNAMENT INFO =====');
    console.log('📋 Tournament data received:', tournament);
    
    try {
        const nameElement = document.getElementById('tournamentName');
        const metaElement = document.getElementById('tournamentMeta');
        
        if (!nameElement) {
            console.error('❌ Tournament name element not found!');
            return;
        }
        
        if (!metaElement) {
            console.error('❌ Tournament meta element not found!');
            return;
        }
        
        // Robuste Tournament-Name Extraktion
        let tournamentName = 'Tournament';
        if (tournament.name) {
            tournamentName = tournament.name;
            console.log('✅ Using tournament.name:', tournamentName);
        } else if (tournament.Name) {
            tournamentName = tournament.Name;
            console.log('✅ Using tournament.Name:', tournamentName);
        } else if (tournament.id) {
            tournamentName = `Tournament ${tournament.id}`;
            console.log('⚠️ Using fallback name with ID:', tournamentName);
        } else {
            tournamentName = `Tournament ${window.tournamentId}`;
            console.log('⚠️ Using fallback name with global ID:', tournamentName);
        }
        
        nameElement.textContent = `🎯 ${tournamentName}`;
        console.log('✅ Tournament name updated');
        
        // Robuste Meta-Information
        const location = tournament.location || tournament.Location || 'Unbekannter Ort';
        const description = tournament.description || tournament.Description || '';
        const descriptionText = description ? ` • ${description}` : '';
        
        // Tournament Statistics
        const totalMatches = tournament.totalMatches || tournament.matches?.length || window.matches?.length || 0;
        const totalPlayers = tournament.totalPlayers || tournament.playerCount || 0;
        const classCount = tournament.classes?.length || window.tournamentClasses?.length || 0;
        
        console.log('📊 Tournament stats:', { totalMatches, totalPlayers, classCount });
        
        const tournamentIdDisplay = tournament.id || window.tournamentId || 'Unknown';
        
        metaElement.innerHTML = `
            Tournament-ID: <code style="background: rgba(0,0,0,0.1); padding: 2px 6px; border-radius: 4px; font-family: monospace;">${tournamentIdDisplay}</code> 
            • 📍 ${location}${descriptionText}
            <br><small style="color: #666;">
                👥 ${totalPlayers} Spieler • 🎮 ${totalMatches} Matches • 📚 ${classCount} Klassen
            </small>
        `;
        
        console.log('✅ Tournament meta information updated');
        console.log('📋 ===== TOURNAMENT INFO UPDATE COMPLETE =====');
        
    } catch (error) {
        console.error('❌ Error updating tournament info:', error);
        console.error('❌ Error stack:', error.stack);
        
        // Fallback update
        try {
            const nameElement = document.getElementById('tournamentName');
            if (nameElement) {
                nameElement.textContent = `🎯 Tournament ${window.tournamentId}`;
            }
        } catch (fallbackError) {
            console.error('❌ Fallback update also failed:', fallbackError);
        }
    }
}

function displayMatches(matches) {
    const container = document.getElementById('matchContainer');
    
    console.log('🎮 displayMatches called with:', matches?.length, 'matches');
    console.log('📊 Sample match data:', matches?.[0]);
    
    if (!matches || matches.length === 0) {
        displayNoMatches();
        return;
    }

    // Automatische Datenvalidierung beim Anzeigen
    console.log(`🔍 [AUTO-VALIDATION] Performing automatic data integrity check...`);
    let validationWarnings = 0;
    matches.forEach((match, index) => {
        const matchId = match.matchId || match.id || match.Id || `Unknown-${index}`;
        const classId = match.classId || match.ClassId;
        const className = match.className || match.ClassName;
        const groupName = match.groupName || match.GroupName;
        
        if (!classId) {
            console.warn(`⚠️ [VALIDATION] Match ${matchId}: Missing class ID`);
            validationWarnings++;
        }
        if (!className) {
            console.warn(`⚠️ [VALIDATION] Match ${matchId}: Missing class name`);
            validationWarnings++;
        }
        if (!groupName && match.matchType !== 'Finals' && match.MatchType !== 'Finals') {
            console.warn(`⚠️ [VALIDATION] Match ${matchId}: Missing group name (type: ${match.matchType || match.MatchType || 'Unknown'})`);
            validationWarnings++;
        }
    });
    
    if (validationWarnings > 0) {
        console.warn(`⚠️ [AUTO-VALIDATION] Found ${validationWarnings} data integrity warnings`);
        showNotification(`⚠️ ${validationWarnings} Datenwarnungen gefunden - siehe Konsole für Details`, 'warning');
    } else {
        console.log(`✅ [AUTO-VALIDATION] All match data appears complete`);
    }

    // Group matches by status
    const getMatchStatus = (match) => {
        const status = match.status || match.Status || 'NotStarted';
        return status.toLowerCase();
    };
    
    const pending = matches.filter(m => {
        const status = getMatchStatus(m);
        return status === 'notstarted' || status === 'pending';
    });
    
    const inProgress = matches.filter(m => {
        const status = getMatchStatus(m);
        return status === 'inprogress' || status === 'active';
    });
    
    const finished = matches.filter(m => {
        const status = getMatchStatus(m);
        return status === 'finished' || status === 'completed';
    });

    console.log(`📊 Match distribution: ${pending.length} pending, ${inProgress.length} in progress, ${finished.length} finished`);

    let html = '<div class="match-grid">';

    // Show in-progress matches first, then pending, then finished
    const orderedMatches = [...inProgress, ...pending, ...finished];

    orderedMatches.forEach((match, index) => {
        console.log(`🎮 Processing match ${index + 1}:`, {
            matchId: match.matchId || match.id || match.Id,
            player1: match.player1 || match.Player1,
            player2: match.player2 || match.Player2,
            status: match.status || match.Status,
            matchType: match.matchType || match.MatchType || 'Group',
            classId: match.classId || match.ClassId || 'MISSING',
            className: match.className || match.ClassName || 'MISSING',
            groupName: match.groupName || match.GroupName || 'MISSING'
        });
        html += createMatchCard(match);
    });

    html += '</div>';
    container.innerHTML = html;
    
    console.log(`✅ Successfully displayed ${orderedMatches.length} matches`);
    
    if (validationWarnings > 0) {
        const warningDiv = document.createElement('div');
        warningDiv.style.cssText = `
            background: #fefcbf;
            color: #d69e2e;
            border: 2px solid #f6e05e;
            padding: 12px;
            border-radius: 8px;
            margin: 20px 0;
            font-size: 0.9em;
            text-align: center;
        `;
        warningDiv.innerHTML = `
            ⚠️ <strong>${validationWarnings} Datenintegritäts-Warnungen</strong> gefunden<br>
            <small>Öffnen Sie die Browser-Konsole für Details oder verwenden Sie <code>validateMatchData()</code></small>
        `;
        container.insertBefore(warningDiv, container.firstChild);
    }
}

function displayNoMatches(errorMessage = null) {
    const container = document.getElementById('matchContainer');
    
    if (errorMessage) {
        container.innerHTML = `
            <div class="no-matches">
                <span class="icon">❌</span>
                <strong>Fehler beim Laden der Matches</strong><br>
                ${errorMessage}<br>
                <button onclick="loadMatches()" style="margin-top: 15px; padding: 10px 20px; background: #667eea; color: white; border: none; border-radius: 8px; cursor: pointer;">
                    🔄 Erneut versuchen
                </button>
                <button onclick="debugMatches()" style="margin-top: 10px; margin-left: 10px; padding: 10px 20px; background: #ff6b6b; color: white; border: none; border-radius: 8px; cursor: pointer;">
                    🔧 Debug
                </button>
            </div>
        `;
    } else {
        container.innerHTML = `
            <div class="no-matches">
                <span class="icon">🎯</span>
                <strong>Keine Matches verfügbar</strong><br>
                <small>Matches werden vom Tournament Planner synchronisiert...</small><br>
                <button onclick="loadMatches()" style="margin-top: 15px; padding: 10px 20px; background: #667eea; color: white; border: none; border-radius: 8px; cursor: pointer;">
                    🔄 Aktualisieren
                </button>
                <button onclick="debugMatches()" style="margin-top: 10px; margin-left: 10px; padding: 10px 20px; background: #28a745; color: white; border: none; border-radius: 8px; cursor: pointer;">
                    🔧 Debug Test
                </button>
                <div style="margin-top: 15px; font-size: 0.9em; color: rgba(255,255,255,0.8);">
                    Tournament ID: <code style="background: rgba(255,255,255,0.2); padding: 2px 6px; border-radius: 4px;">${window.tournamentId}</code>
                </div>
            </div>
        `;
    }
}

function updateClassSelector(classes) {
    const classSelector = document.getElementById('classSelector');
    const classSelect = document.getElementById('classSelect');
    
    if (!classSelector || !classSelect) return;
    
    console.log('📚 Updating class selector with:', classes);
    
    // Clear existing options (except "All")
    classSelect.innerHTML = '<option value="">Alle Klassen</option>';
    
    if (classes && classes.length > 0) {
        classes.forEach(cls => {
            const option = document.createElement('option');
            option.value = cls.id;
            option.textContent = `${cls.name} (${cls.playerCount || 0} Spieler)`;
            classSelect.appendChild(option);
        });
        
        // Show class selector
        classSelector.style.display = 'block';
    } else {
        classSelector.style.display = 'none';
    }
}

function showNotification(message, type = 'info') {
    console.log(`📢 [${type.toUpperCase()}] ${message}`);
    
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 12px 20px;
        border-radius: 8px;
        color: white;
        font-weight: bold;
        z-index: 10000;
        max-width: 300px;
        word-wrap: break-word;
        animation: slideIn 0.3s ease-out;
    `;
    
    switch (type) {
        case 'success':
            notification.style.background = 'linear-gradient(135deg, #48bb78 0%, #38a169 100%)';
            break;
        case 'error':
            notification.style.background = 'linear-gradient(135deg, #f56565 0%, #e53e3e 100%)';
            break;
        case 'warning':
            notification.style.background = 'linear-gradient(135deg, #ed8936 0%, #dd6b20 100%)';
            break;
        default:
            notification.style.background = 'linear-gradient(135deg, #4299e1 0%, #3182ce 100%)';
    }
    
    notification.textContent = message;
    document.body.appendChild(notification);
    
    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 5000);
}

function updateConnectionStatus(connected) {
    const indicator = document.getElementById('connectionIndicator');
    const text = document.getElementById('connectionText');
    
    if (connected) {
        indicator.classList.add('connected');
        text.textContent = 'Verbunden';
    } else {
        indicator.classList.remove('connected');
        text.textContent = 'Getrennt';
    }
}