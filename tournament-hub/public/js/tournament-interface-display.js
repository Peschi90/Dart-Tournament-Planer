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
        const location = tournament.location || tournament.Location || t('tournamentInterface.tournament.unknownLocation');
        const description = tournament.description || tournament.Description || '';
        const descriptionText = description ? ` • ${description}` : '';

        // Tournament Statistics
        const totalMatches = tournament.totalMatches || (tournament.matches && tournament.matches.length) || (window.matches && window.matches.length) || 0;
        const totalPlayers = tournament.totalPlayers || tournament.playerCount || 0;
        const classCount = (tournament.classes && tournament.classes.length) || (window.tournamentClasses && window.tournamentClasses.length) || 0;

        console.log('📊 Tournament stats:', { totalMatches, totalPlayers, classCount });

        const tournamentIdDisplay = tournament.id || window.tournamentId || 'Unknown';

        metaElement.innerHTML = `
            <span data-i18n="tournamentInterface.messages.tournamentId">Tournament-ID</span>: <code style="background: rgba(0,0,0,0.1); padding: 2px 6px; border-radius: 4px; font-family: monospace;">${tournamentIdDisplay}</code> 
            • 📍 ${location}${descriptionText}
            <br><small style="color: #666;">
                👥 ${totalPlayers} <span data-i18n="tournamentInterface.tournament.players">Spieler</span> • 🎮 ${totalMatches} <span data-i18n="tournamentInterface.tournament.matches">Matches</span> • 📚 ${classCount} <span data-i18n="tournamentInterface.tournament.classes">Klassen</span>
            </small>
        `;

        // Apply i18n translations to the newly created content
        if (window.applyTranslationsToElement) {
            window.applyTranslationsToElement(metaElement);
        } else if (window.i18n && window.i18n.applyTranslations) {
            window.i18n.applyTranslations();
        }

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

    console.log('🎮 displayMatches called with:', (matches && matches.length) || 0, 'matches');
    console.log('📊 Sample match data:', matches && matches[0]);

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

    // 🌐 Apply translations to the newly generated match cards
    if (window.applyTranslationsToElement) {
        window.applyTranslationsToElement(container);
    }

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
                <strong>${t('tournamentInterface.messages.loadingMatchesError')}</strong><br>
                ${errorMessage}<br>
                <button onclick="loadMatches()" style="margin-top: 15px; padding: 10px 20px; background: #667eea; color: white; border: none; border-radius: 8px; cursor: pointer;" data-i18n="tournamentInterface.matches.retryButton">
                    🔄 Erneut versuchen
                </button>
                <button onclick="debugMatches()" style="margin-top: 10px; margin-left: 10px; padding: 10px 20px; background: #ff6b6b; color: white; border: none; border-radius: 8px; cursor: pointer;" data-i18n="tournamentInterface.matches.debugButton">
                    🔧 Debug
                </button>
            </div>
        `;
    } else {
        container.innerHTML = `
            <div class="no-matches">
                <span class="icon">🎯</span>
                <strong>${t('tournamentInterface.matches.noMatches')}</strong><br>
                <small>${t('tournamentInterface.messages.noMatchesDescription')}</small>
                <small>${t('tournamentInterface.messages.syncDescription')}</small><br>
                <button onclick="loadMatches()" style="margin-top: 15px; padding: 10px 20px; background: #667eea; color: white; border: none; border-radius: 8px; cursor: pointer;" data-i18n="tournamentInterface.matches.refreshButton">
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

    // 🌐 Apply translations to the newly generated content
    if (window.applyTranslationsToElement) {
        window.applyTranslationsToElement(container);
    }
}

function updateClassSelector(classes) {
    const classSelector = document.getElementById('classSelector');
    const classSelect = document.getElementById('classSelect');

    if (!classSelector || !classSelect) return;

    console.log('📚 Updating class selector with:', classes);

    // Clear existing options (except "All")
    classSelect.innerHTML = `<option value="" data-i18n="tournamentInterface.classes.allClasses">${t('tournamentInterface.classes.allClasses')}</option>`;

    if (classes && classes.length > 0) {
        classes.forEach(cls => {
            const option = document.createElement('option');
            // Support both naming conventions
            const classId = cls.classId || cls.ClassId || cls.id || cls.Id;
            const className = cls.className || cls.ClassName || cls.name || cls.Name;
            const playerCount = cls.playerCount || cls.PlayerCount || 0;

            option.value = classId;
            option.textContent = `${className} (${playerCount} ${t('tournamentInterface.classes.players')})`;
            classSelect.appendChild(option);

            console.log('📚 Added class option:', { classId, className, playerCount });
        });

        // Show class selector
        classSelector.style.display = 'block';
        console.log('✅ Class selector updated with', classes.length, 'classes');
    } else {
        classSelector.style.display = 'none';
        console.log('⚠️ No classes provided, hiding class selector');
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

/**
 * Get translation helper
 */
function t(key, params = {}) {
    if (window.i18n && window.i18n.t) {
        return window.i18n.t(key, params);
    }
    if (window.I18nManager && window.I18nManager.t) {
        return window.I18nManager.t(key, params);
    }
    console.warn(`🌐 t() called but i18n not available for key: ${key}`);
    return key; // Fallback if i18n not available
}

function updateConnectionStatus(connected) {
    const indicator = document.getElementById('connectionIndicator');
    const text = document.getElementById('connectionText');

    if (connected) {
        indicator.classList.add('connected');
        text.textContent = t('tournamentInterface.connection.connected');
    } else {
        indicator.classList.remove('connected');
        text.textContent = t('tournamentInterface.connection.disconnected');
    }
}

/**
 * 🌐 Refresh all dynamic translations in the Tournament Interface
 * This function is called when the language changes to ensure
 * all dynamic content is properly translated
 */
function refreshDisplayTranslations() {
    console.log('🌐 [TOURNAMENT] Refreshing display translations...');

    try {
        // 1. Update connection status
        const indicator = document.getElementById('connectionIndicator');
        const text = document.getElementById('connectionText');
        if (indicator && text) {
            if (indicator.classList.contains('connected')) {
                text.textContent = t('tournamentInterface.connection.connected');
            } else {
                text.textContent = t('tournamentInterface.connection.disconnected');
            }
        }

        // 2. Update tournament meta info if available
        if (window.currentTournament) {
            updateTournamentInfo(window.currentTournament);
        }

        // 3. Update class selector if available
        const classSelect = document.getElementById('classSelect');
        if (classSelect && window.tournamentClasses) {
            const currentValue = classSelect.value;
            // Clear options
            classSelect.innerHTML = `<option value="" data-i18n="tournamentInterface.classes.allClasses">${t('tournamentInterface.classes.allClasses')}</option>`;

            // Re-add class options
            window.tournamentClasses.forEach(cls => {
                const option = document.createElement('option');
                // Support both naming conventions
                const classId = cls.classId || cls.ClassId || cls.id || cls.Id;
                const className = cls.className || cls.ClassName || cls.name || cls.Name;
                const playerCount = cls.playerCount || cls.PlayerCount || 0;

                option.value = classId;
                option.textContent = `${className} (${playerCount} ${t('tournamentInterface.classes.players')})`;
                classSelect.appendChild(option);
            });

            // Restore selection
            classSelect.value = currentValue;
        }

        // 4. Update match cards - regenerate all match cards with new translations
        if (window.matches && window.matches.length > 0) {
            console.log('🌐 [TOURNAMENT] Regenerating match cards with new translations...');
            displayMatches(window.matches);
        }

        // 5. Update any notification that might be visible
        // (notifications auto-translate on next showing)

        console.log('✅ [TOURNAMENT] Display translations refreshed');

    } catch (error) {
        console.error('❌ [TOURNAMENT] Error refreshing display translations:', error);
    }
}

// Make function globally available
window.refreshDisplayTranslations = refreshDisplayTranslations;