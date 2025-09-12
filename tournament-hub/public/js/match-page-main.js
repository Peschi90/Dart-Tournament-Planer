/**
 * Match Page Main Module
 * Main entry point and orchestrator for match pages
 */
class MatchPageMain {
    constructor() {
        this.isInitialized = false;
        this.pageLoadTime = Date.now();

        console.log('🚀 [MATCH-MAIN] Match Page Main initialized');
    }

    /**
     * Get translation helper
     */
    t(key, params = {}) {
        if (window.i18nManager) {
            return window.i18nManager.t(key, params);
        } else if (typeof t !== 'undefined') {
            return t(key, params);
        }
        return key; // Fallback if i18n not available
    }

    /**
     * Initialize the entire match page application
     */
    async initialize() {
        if (this.isInitialized) {
            console.warn('⚠️ [MATCH-MAIN] Already initialized');
            return;
        }

        try {
            console.log('🔄 [MATCH-MAIN] Starting match page initialization...');

            // Check if all required modules are loaded (with retry)
            const dependenciesReady = await this.waitForDependencies();
            if (!dependenciesReady) {
                throw new Error('Required modules not loaded after retries');
            }

            // Initialize core functionality
            const coreInitialized = await window.matchPageCore.initialize();
            if (!coreInitialized) {
                throw new Error('Core initialization failed');
            }

            // Setup global event listeners
            this.setupGlobalEventListeners();

            // Setup error handling
            this.setupErrorHandling();

            // Setup page visibility handling
            this.setupVisibilityHandling();

            // Mark as initialized
            this.isInitialized = true;

            const initTime = Date.now() - this.pageLoadTime;
            console.log(`✅ [MATCH-MAIN] Match page initialized successfully in ${initTime}ms`);

            // Optional: Perform initial health check
            this.performInitialHealthCheck();

        } catch (error) {
            console.error('🚫 [MATCH-MAIN] Initialization failed:', error);
            this.handleInitializationError(error);
        }
    }

    /**
     * Wait for dependencies with retry logic
     */
    async waitForDependencies(maxRetries = 5, delayMs = 200) {
        for (let attempt = 1; attempt <= maxRetries; attempt++) {
            console.log(`🔍 [MATCH-MAIN] Checking dependencies (attempt ${attempt}/${maxRetries})`);

            if (this.checkDependencies()) {
                console.log('✅ [MATCH-MAIN] All dependencies are ready');
                return true;
            }

            if (attempt < maxRetries) {
                console.log(`⏳ [MATCH-MAIN] Dependencies not ready, waiting ${delayMs}ms...`);
                await new Promise(resolve => setTimeout(resolve, delayMs));
                delayMs *= 1.5; // Exponential backoff
            }
        }

        console.error('❌ [MATCH-MAIN] Dependencies still not ready after all retries');
        return false;
    }

    /**
     * Check if all required dependencies are available
     */
    checkDependencies() {
        const requiredModules = [
            'matchPageCore',
            'matchPageDisplay',
            'matchPageScoring',
            'matchPageAPI',
            'i18nManager'
        ];

        console.log('🔍 [MATCH-MAIN] Checking dependencies...');

        // Log current state of all window objects
        console.log('📋 [MATCH-MAIN] Current window objects:', {
            matchPageCore: !!window.matchPageCore,
            matchPageDisplay: !!window.matchPageDisplay,
            matchPageScoring: !!window.matchPageScoring,
            matchPageAPI: !!window.matchPageAPI,
            i18nManager: !!window.i18nManager,
            globalT: !!window.t
        });

        const missingModules = [];
        const availableModules = [];

        requiredModules.forEach(module => {
            if (!window[module]) {
                missingModules.push(module);
            } else {
                availableModules.push(module);
                console.log(`✅ [MATCH-MAIN] ${module} is available`);
            }
        });

        if (missingModules.length > 0) {
            console.error('🚫 [MATCH-MAIN] Missing required modules:', missingModules);
            console.log('✅ [MATCH-MAIN] Available modules:', availableModules);
            return false;
        }

        console.log('✅ [MATCH-MAIN] All required modules loaded');
        return true;
    }

    /**
     * Setup global event listeners
     */
    setupGlobalEventListeners() {
        // Handle page reload/refresh
        window.addEventListener('beforeunload', (event) => {
            this.handleBeforeUnload(event);
        });

        // Handle browser back/forward navigation
        window.addEventListener('popstate', (event) => {
            this.handlePopState(event);
        });

        // Handle keyboard shortcuts
        window.addEventListener('keydown', (event) => {
            this.handleKeyboardShortcuts(event);
        });

        // Handle focus/blur for real-time updates
        window.addEventListener('focus', () => {
            this.handlePageFocus();
        });

        window.addEventListener('blur', () => {
            this.handlePageBlur();
        });

        // Handle online/offline status
        window.addEventListener('online', () => {
            this.handleOnlineStatus(true);
        });

        window.addEventListener('offline', () => {
            this.handleOnlineStatus(false);
        });

        console.log('✅ [MATCH-MAIN] Global event listeners setup complete');
    }

    /**
     * Setup error handling
     */
    setupErrorHandling() {
        // Global error handler
        window.addEventListener('error', (event) => {
            console.error('🚫 [MATCH-MAIN] Global error:', event.error);
            this.logError('Global Error', event.error, {
                filename: event.filename,
                lineno: event.lineno,
                colno: event.colno
            });
        });

        // Unhandled promise rejections
        window.addEventListener('unhandledrejection', (event) => {
            console.error('🚫 [MATCH-MAIN] Unhandled promise rejection:', event.reason);
            this.logError('Unhandled Promise Rejection', event.reason);
            event.preventDefault(); // Prevent default browser handling
        });

        console.log('✅ [MATCH-MAIN] Error handling setup complete');
    }

    /**
     * Setup page visibility handling
     */
    setupVisibilityHandling() {
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                console.log('👁️ [MATCH-MAIN] Page hidden - reducing updates');
                this.handlePageHidden();
            } else {
                console.log('👁️ [MATCH-MAIN] Page visible - resuming updates');
                this.handlePageVisible();
            }
        });
    }

    /**
     * Handle before page unload
     */
    handleBeforeUnload(event) {
        console.log('🔄 [MATCH-MAIN] Page unloading...');

        // Cleanup resources
        if (window.matchPageCore) {
            window.matchPageCore.cleanup();
        }

        if (window.matchPageScoring) {
            window.matchPageScoring.cleanup();
        }

        // Note: Modern browsers ignore custom messages
        // event.returnValue = 'Are you sure you want to leave this match page?';
    }

    /**
     * Handle browser navigation
     */
    handlePopState(event) {
        console.log('🔙 [MATCH-MAIN] Navigation event:', event.state);
        // Handle back button if needed
    }

    /**
     * Handle keyboard shortcuts
     */
    handleKeyboardShortcuts(event) {
        // Ctrl/Cmd + R: Refresh data
        if ((event.ctrlKey || event.metaKey) && event.key === 'r') {
            event.preventDefault();
            console.log('🔄 [MATCH-MAIN] Manual refresh requested');
            this.refreshMatchData();
            return;
        }

        // Escape: Clear any active modals or forms
        if (event.key === 'Escape') {
            console.log('⏹️ [MATCH-MAIN] Escape key pressed');
            this.handleEscapeKey();
            return;
        }

        // F5: Full page refresh
        if (event.key === 'F5') {
            console.log('🔄 [MATCH-MAIN] F5 refresh');
            // Let default behavior occur
            return;
        }

        // Future: Add more shortcuts for scoring interface
        // Space: Quick score entry
        // Numbers 1-20: Dart board input
        // Ctrl+Z: Undo last action
    }

    /**
     * Handle page focus
     */
    handlePageFocus() {
        console.log('🎯 [MATCH-MAIN] Page focused - checking for updates');

        // Request fresh data when page regains focus
        if (window.matchPageCore && window.matchPageCore.isSocketConnected()) {
            window.matchPageCore.requestMatchData();
        }
    }

    /**
     * Handle page blur
     */
    handlePageBlur() {
        console.log('😴 [MATCH-MAIN] Page blurred - reducing activity');
        // Could pause non-critical updates here
    }

    /**
     * Handle page hidden
     */
    handlePageHidden() {
        // Reduce update frequency or pause non-critical operations
        console.log('👻 [MATCH-MAIN] Page hidden');
    }

    /**
     * Handle page visible
     */
    handlePageVisible() {
        // Resume normal operations and sync data
        console.log('👀 [MATCH-MAIN] Page visible');
        this.refreshMatchData();
    }

    /**
     * Handle online/offline status
     */
    handleOnlineStatus(isOnline) {
        console.log(`🌐 [MATCH-MAIN] Network status: ${isOnline ? 'online' : 'offline'}`);

        if (isOnline) {
            // Attempt to reconnect and sync data
            setTimeout(() => {
                if (window.matchPageCore && !window.matchPageCore.isSocketConnected()) {
                    window.matchPageCore.initializeSocket();
                }
            }, 1000);
        }

        // Update UI to reflect connection status
        this.updateConnectionStatusUI(isOnline);
    }

    /**
     * Update connection status in UI
     */
    updateConnectionStatusUI(isOnline) {
        const statusElement = document.getElementById('connectionText');
        if (statusElement) {
            if (isOnline) {
                statusElement.textContent = this.t('matchPage.connection.connected');
                statusElement.setAttribute('data-i18n', 'matchPage.connection.connected');
            } else {
                statusElement.textContent = this.t('matchPage.connection.disconnected');
                statusElement.setAttribute('data-i18n', 'matchPage.connection.disconnected');
            }
        }
    }

    /**
     * Handle escape key press
     */
    handleEscapeKey() {
        // Close any open modals, forms, or overlays
        const activeModals = document.querySelectorAll('.modal.active, .overlay.active');
        activeModals.forEach(modal => {
            modal.classList.remove('active');
        });

        // Clear any active form focus
        const activeElement = document.activeElement;
        if (activeElement && activeElement.tagName !== 'BODY') {
            activeElement.blur();
        }
    }

    /**
     * Refresh match data
     */
    async refreshMatchData() {
        try {
            if (window.matchPageCore && window.matchPageCore.isSocketConnected()) {
                console.log('🔄 [MATCH-MAIN] Refreshing match data...');
                window.matchPageCore.requestMatchData();
            } else {
                console.warn('⚠️ [MATCH-MAIN] Cannot refresh - not connected');
            }
        } catch (error) {
            console.error('🚫 [MATCH-MAIN] Error refreshing match data:', error);
        }
    }

    /**
     * Perform initial health check
     */
    async performInitialHealthCheck() {
        try {
            console.log('🏥 [MATCH-MAIN] Performing initial health check...');

            const apiHealthy = await window.matchPageAPI.healthCheck();
            const socketHealthy = await window.matchPageAPI.testSocketConnection();

            console.log(`📊 [MATCH-MAIN] Health check results: API=${apiHealthy}, Socket=${socketHealthy}`);

            if (!apiHealthy && !socketHealthy) {
                console.warn('⚠️ [MATCH-MAIN] Both API and Socket connections failed');
                this.showConnectionWarning();
            }
        } catch (error) {
            console.error('🚫 [MATCH-MAIN] Health check error:', error);
        }
    }

    /**
     * Show connection warning to user
     */
    showConnectionWarning() {
        const warningHTML = `
            <div id="connectionWarning" class="connection-warning">
                ⚠️ <strong>${this.t('matchPage.connection.error')}</strong><br>
                ${this.t('matchPage.errors.connectionFailed')}
                <button onclick="this.parentElement.style.display='none'">✖</button>
            </div>
        `;

        document.body.insertAdjacentHTML('afterbegin', warningHTML);

        // Auto-hide after 10 seconds
        setTimeout(() => {
            const warning = document.getElementById('connectionWarning');
            if (warning) {
                warning.style.display = 'none';
            }
        }, 10000);
    }

    /**
     * Handle initialization error
     */
    handleInitializationError(error) {
        console.error('🚫 [MATCH-MAIN] Handling initialization error:', error);

        const errorContainer = document.getElementById('loadingContainer');
        if (errorContainer) {
            errorContainer.innerHTML = `
                <span class="icon">❌</span>
                <strong>Initialisierungsfehler</strong><br>
                ${error.message}<br><br>
                <button onclick="location.reload()" class="retry-button">
                    🔄 Seite neu laden
                </button>
            `;
        }
    }

    /**
     * Log error for debugging
     */
    logError(type, error, context = {}) {
        const errorLog = {
            timestamp: new Date().toISOString(),
            type: type,
            error: {
                message: (error && error.message) || 'Unknown error',
                stack: error && error.stack,
                name: error && error.name
            },
            context: context,
            url: window.location.href,
            userAgent: navigator.userAgent,
            sessionTime: Date.now() - this.pageLoadTime
        };

        // Store in local storage for debugging (keep last 10 errors)
        try {
            const existingLogs = JSON.parse(localStorage.getItem('matchPageErrors') || '[]');
            existingLogs.push(errorLog);

            // Keep only last 10 errors
            const recentLogs = existingLogs.slice(-10);
            localStorage.setItem('matchPageErrors', JSON.stringify(recentLogs));
        } catch (storageError) {
            console.error('Could not store error log:', storageError);
        }

        console.error(`📝 [MATCH-MAIN] ${type}:`, errorLog);
    }

    /**
     * Get diagnostic information
     */
    getDiagnosticInfo() {
        return {
            initialized: this.isInitialized,
            pageLoadTime: this.pageLoadTime,
            sessionTime: Date.now() - this.pageLoadTime,
            modules: {
                core: !!window.matchPageCore,
                display: !!window.matchPageDisplay,
                scoring: !!window.matchPageScoring,
                api: !!window.matchPageAPI
            },
            connection: {
                online: navigator.onLine,
                socketConnected: (window.matchPageCore && window.matchPageCore.isSocketConnected && window.matchPageCore.isSocketConnected()) || false
            },
            tournament: {
                id: (window.matchPageCore && window.matchPageCore.tournamentId) || null,
                matchId: (window.matchPageCore && window.matchPageCore.matchId) || null
            },
            errors: JSON.parse(localStorage.getItem('matchPageErrors') || '[]')
        };
    }

    /**
     * Export diagnostic data for support
     */
    exportDiagnostics() {
        const diagnostics = this.getDiagnosticInfo();
        const dataStr = JSON.stringify(diagnostics, null, 2);
        const dataBlob = new Blob([dataStr], { type: 'application/json' });

        const link = document.createElement('a');
        link.href = URL.createObjectURL(dataBlob);
        link.download = `match-page-diagnostics-${Date.now()}.json`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        console.log('📊 [MATCH-MAIN] Diagnostics exported');
    }
}

// Additional CSS for main module UI elements
const mainStyles = `
    <style>
        .connection-warning {
            position: fixed;
            top: 20px;
            left: 50%;
            transform: translateX(-50%);
            background: #fed7d7;
            color: #c53030;
            padding: 15px 20px;
            border-radius: 8px;
            border: 1px solid #feb2b2;
            z-index: 9999;
            text-align: center;
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
            animation: slideDown 0.3s ease;
        }

        .connection-warning button {
            background: none;
            border: none;
            color: #c53030;
            font-weight: bold;
            cursor: pointer;
            margin-left: 10px;
            padding: 2px 6px;
            border-radius: 3px;
        }

        .connection-warning button:hover {
            background: rgba(197, 48, 48, 0.1);
        }

        .retry-button {
            background: #667eea;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 8px;
            cursor: pointer;
            font-size: 16px;
            margin-top: 10px;
            transition: all 0.3s ease;
        }

        .retry-button:hover {
            background: #5a67d8;
            transform: translateY(-1px);
        }

        @keyframes slideDown {
            from {
                opacity: 0;
                transform: translateX(-50%) translateY(-20px);
            }
            to {
                opacity: 1;
                transform: translateX(-50%) translateY(0);
            }
        }

        /* Debug info display (for development) */
        .debug-info {
            position: fixed;
            bottom: 10px;
            right: 10px;
            background: rgba(0,0,0,0.8);
            color: white;
            padding: 10px;
            border-radius: 5px;
            font-family: monospace;
            font-size: 12px;
            z-index: 999;
            display: none;
        }

        .debug-info.show {
            display: block;
        }

        /* Mobile optimizations */
        @media (max-width: 768px) {
            .connection-warning {
                left: 20px;
                right: 20px;
                transform: none;
                font-size: 14px;
            }
        }
    </style>
`;

// Inject main styles
document.head.insertAdjacentHTML('beforeend', mainStyles);

// Create global instance
window.matchPageMain = new MatchPageMain();

// Auto-initialize when DOM is ready
// Remove automatic initialization - will be started from HTML after i18n is ready
// document.addEventListener('DOMContentLoaded', () => {
//     console.log('📄 [MATCH-MAIN] DOM ready - initializing match page');
//     
//     // Add delay to ensure all modules are loaded
//     setTimeout(() => {
//         console.log('⏰ [MATCH-MAIN] Starting delayed initialization to ensure all modules are loaded');
//         window.matchPageMain.initialize();
//     }, 100); // Short delay to let all scripts finish loading
// });

// Expose global debug function for development
window.getMatchPageDiagnostics = () => {
    return window.matchPageMain.getDiagnosticInfo();
};

window.exportMatchPageDiagnostics = () => {
    window.matchPageMain.exportDiagnostics();
};

console.log('🚀 [MATCH-MAIN] Match Page Main module loaded');