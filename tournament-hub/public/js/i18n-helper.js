/**
 * i18n Helper Functions for Tournament Hub JavaScript Modules
 * Provides translation functions for use in existing JS modules
 */

// Wait for i18n to be available
function waitForI18n() {
    return new Promise((resolve) => {
        if (window.i18n) {
            resolve(window.i18n);
        } else {
            const checkI18n = setInterval(() => {
                if (window.i18n) {
                    clearInterval(checkI18n);
                    resolve(window.i18n);
                }
            }, 100);
        }
    });
}

// Translation helper for JavaScript modules
const I18nHelper = {
    // Get translation with fallback
    t: (key, fallback = null, params = {}) => {
        if (window.i18n) {
            return window.i18n.t(key, params);
        }
        return fallback || key;
    },

    // Get current language
    getCurrentLanguage: () => {
        if (window.i18n) {
            return window.i18n.getCurrentLanguage();
        }
        return 'de'; // fallback
    },

    // Format relative time
    formatRelativeTime: (timestamp) => {
        if (window.i18n) {
            return window.i18n.formatRelativeTime(timestamp);
        }
        // Fallback implementation
        if (!timestamp) return 'Never';
        const now = new Date();
        const time = new Date(timestamp);
        const diffMs = now - time;
        const diffMins = Math.floor(diffMs / 60000);

        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins} min ago`;
        if (diffMins < 1440) return `${Math.floor(diffMins / 60)} hrs ago`;
        return time.toLocaleDateString();
    },

    // Format uptime
    formatUptime: (seconds) => {
        if (window.i18n) {
            return window.i18n.formatUptime(seconds);
        }
        // Fallback implementation
        const hours = Math.floor(seconds / 3600);
        const minutes = Math.floor((seconds % 3600) / 60);
        const secs = seconds % 60;

        if (hours > 0) {
            return `${hours}h ${minutes}m`;
        } else if (minutes > 0) {
            return `${minutes}m ${secs}s`;
        } else {
            return `${secs}s`;
        }
    },

    // Apply translations to dynamically created content
    applyTranslations: (container = document) => {
        if (window.i18n) {
            // Find elements with data-i18n in the container
            const elements = container.querySelectorAll('[data-i18n]');
            elements.forEach(element => {
                const key = element.getAttribute('data-i18n');
                const params = I18nHelper.parseDataParams(element.getAttribute('data-i18n-params'));
                const target = element.getAttribute('data-i18n-target') || 'textContent';

                const translation = window.i18n.t(key, params);

                if (target === 'placeholder') {
                    element.placeholder = translation;
                } else if (target === 'title') {
                    element.title = translation;
                } else if (target === 'value') {
                    element.value = translation;
                } else if (target === 'innerHTML') {
                    element.innerHTML = translation;
                } else {
                    element.textContent = translation;
                }
            });
        }
    },

    // Parse data-i18n-params attribute
    parseDataParams: (paramsString) => {
        if (!paramsString) return {};

        try {
            return JSON.parse(paramsString);
        } catch (error) {
            console.warn('Failed to parse data-i18n-params:', paramsString);
            return {};
        }
    },

    // Create translatable element
    createElement: (tag, translationKey, params = {}, attributes = {}) => {
        const element = document.createElement(tag);
        element.setAttribute('data-i18n', translationKey);

        if (Object.keys(params).length > 0) {
            element.setAttribute('data-i18n-params', JSON.stringify(params));
        }

        Object.keys(attributes).forEach(attr => {
            element.setAttribute(attr, attributes[attr]);
        });

        // Set initial text
        element.textContent = I18nHelper.t(translationKey, translationKey, params);

        return element;
    },

    // Common translations for frequently used terms
    common: {
        loading: () => I18nHelper.t('common.loading', 'Loading...'),
        error: () => I18nHelper.t('common.error', 'Error'),
        success: () => I18nHelper.t('common.success', 'Success'),
        cancel: () => I18nHelper.t('common.cancel', 'Cancel'),
        confirm: () => I18nHelper.t('common.confirm', 'Confirm'),
        save: () => I18nHelper.t('common.save', 'Save'),
        back: () => I18nHelper.t('common.back', 'Back'),
        refresh: () => I18nHelper.t('common.refresh', 'Refresh'),
    },

    // Dashboard specific translations
    dashboard: {
        connectionStatus: (status) => I18nHelper.t(`dashboard.connection.${status}`, status),
        tournamentStatus: (isActive) => I18nHelper.t(`dashboard.tournaments.status.${isActive ? 'active' : 'inactive'}`, isActive ? 'Active' : 'Inactive'),
    },

    // Dart scoring specific translations
    dartScoring: {
        playerName: (number) => I18nHelper.t(`dartScoring.game.players.player${number}`, `Player ${number}`),
        gameStatus: (type) => I18nHelper.t(`dartScoring.game.status.${type}`, type),
        dartControls: (action) => I18nHelper.t(`dartScoring.game.controls.${action}`, action),
    },

    // Match page specific translations
    matchPage: {
        loading: (type) => I18nHelper.t(`matchPage.loading.${type}`, 'Loading...'),
        connection: (status) => I18nHelper.t(`matchPage.connection.${status}`, status),
    }
};

/**
 * Apply translations to all elements with data-i18n attributes within a specific element
 */
function applyTranslationsToElement(rootElement = document) {
    if (!window.i18n) {
        console.warn('🌐 applyTranslationsToElement called but i18n not available');
        return;
    }

    const elements = rootElement.querySelectorAll('[data-i18n]');
    let translatedCount = 0;

    elements.forEach(element => {
        const key = element.getAttribute('data-i18n');
        const params = parseDataParams(element.getAttribute('data-i18n-params'));

        const translation = window.i18n.t(key, params);

        // Only update if translation is different from key (avoid showing raw keys)
        if (translation && translation !== key) {
            // Update element content
            if (element.tagName.toLowerCase() === 'input') {
                if (element.type === 'text' || element.type === 'password') {
                    element.placeholder = translation;
                } else {
                    element.value = translation;
                }
            } else {
                element.textContent = translation;
            }
            translatedCount++;
        }
    });

    console.log(`🌐 Applied translations to ${translatedCount}/${elements.length} elements in`, rootElement);
}

/**
 * Parse data-i18n-params attribute
 */
function parseDataParams(paramsStr) {
    if (!paramsStr) return {};

    try {
        return JSON.parse(paramsStr);
    } catch (error) {
        console.warn('🌐 Failed to parse data-i18n-params:', paramsStr);
        return {};
    }
}

// Make available globally
window.I18nHelper = I18nHelper;
window.applyTranslationsToElement = applyTranslationsToElement;

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', async() => {
        await waitForI18n();
        console.log('🌐 I18n Helper initialized');
    });
} else {
    waitForI18n().then(() => {
        console.log('🌐 I18n Helper initialized');
    });
}