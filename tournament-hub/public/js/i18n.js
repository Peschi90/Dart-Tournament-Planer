/**
 * Internationalization (i18n) System for Tournament Hub
 * Supports automatic language detection and manual language switching
 */

class I18nManager {
    constructor() {
        this.currentLanguage = 'de'; // Default language
        this.supportedLanguages = ['de', 'en'];
        this.translations = {};
        this.fallbackLanguage = 'de';
        this.languageChangeListeners = [];
        this.isInitialized = false;

        // Storage key for user preference
        this.storageKey = 'tournament-hub-language';

        console.log('🌐 I18n Manager initialized');
        this.detectLanguage();
    }

    /**
     * Detect user's preferred language
     */
    detectLanguage() {
        // 1. Check localStorage for saved preference
        const savedLanguage = localStorage.getItem(this.storageKey);
        if (savedLanguage && savedLanguage !== 'auto' && this.supportedLanguages.includes(savedLanguage)) {
            this.currentLanguage = savedLanguage;
            console.log(`🌐 Using saved language: ${savedLanguage}`);
            return;
        }

        // 2. Check browser language
        const browserLanguage = navigator.language || navigator.languages[0];
        const langCode = browserLanguage.split('-')[0].toLowerCase();

        console.log(`🌐 Browser language detected: ${browserLanguage} (code: ${langCode})`);

        if (this.supportedLanguages.includes(langCode)) {
            this.currentLanguage = langCode;
            console.log(`🌐 Using browser language: ${langCode}`);
        } else {
            console.log(`🌐 Browser language ${langCode} not supported, using fallback: ${this.fallbackLanguage}`);
            this.currentLanguage = this.fallbackLanguage;
        }
    }

    /**
     * Load translations for given language
     */
    async loadTranslations(language) {
        if (!this.supportedLanguages.includes(language)) {
            console.error(`🌐 Unsupported language: ${language}`);
            return false;
        }

        try {
            console.log(`🌐 Loading translations for ${language}...`);
            const response = await fetch(`/i18n/${language}.json`);

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const translations = await response.json();
            this.translations[language] = translations;
            console.log(`✅ Translations for ${language} loaded successfully`);
            return true;
        } catch (error) {
            console.error(`❌ Failed to load translations for ${language}:`, error);
            return false;
        }
    }

    /**
     * Get translated text for given key
     */
    t(key, params = {}) {
        // Try current language first
        let result = this.getNestedTranslation(this.translations[this.currentLanguage], key);

        // Fallback to fallback language
        if (!result && this.currentLanguage !== this.fallbackLanguage) {
            result = this.getNestedTranslation(this.translations[this.fallbackLanguage], key);
        }

        // Final fallback to key itself
        if (!result) {
            console.warn(`🌐 Missing translation for key: ${key}`);
            return key;
        }

        // Replace variables {{variable}}
        Object.keys(params).forEach(paramKey => {
            const regex = new RegExp(`{{${paramKey}}}`, 'g');
            result = result.replace(regex, params[paramKey]);
        });

        return result;
    }

    /**
     * Get nested translation value from object
     */
    getNestedTranslation(obj, key) {
        if (!obj) return null;

        const keys = key.split('.');
        let current = obj;

        for (const k of keys) {
            if (current[k] === undefined) {
                return null;
            }
            current = current[k];
        }

        return current;
    }

    /**
     * Change language and reload translations
     */
    async changeLanguage(language) {
        if (language === 'auto') {
            // Remove saved preference and re-detect
            localStorage.removeItem(this.storageKey);
            this.detectLanguage();
            language = this.currentLanguage;
        } else if (!this.supportedLanguages.includes(language)) {
            console.error(`🌐 Unsupported language: ${language}`);
            return false;
        }

        console.log(`🌐 Changing language to: ${language}`);
        this.currentLanguage = language;

        // Save to localStorage (unless it's auto-detected)
        localStorage.setItem(this.storageKey, language);

        // Load translations if not already loaded
        if (!this.translations[language]) {
            await this.loadTranslations(language);
        }

        // Update document language
        document.documentElement.lang = language;

        // Apply translations to all existing elements
        this.applyTranslations();

        // Update the language selector if it exists
        this.updateLanguageSelector();

        // Notify listeners
        this.notifyLanguageChange(language);

        console.log(`✅ Language changed to: ${language} and translations reapplied`);
        return true;
    }

    /**
     * Initialize i18n system
     */
    async init() {
        console.log('🌐 Initializing i18n system...');

        // Load translations for current language
        await this.loadTranslations(this.currentLanguage);

        // Load fallback language if different
        if (this.currentLanguage !== this.fallbackLanguage) {
            await this.loadTranslations(this.fallbackLanguage);
        }

        // Set document language
        document.documentElement.lang = this.currentLanguage;

        // Setup language selector event listener
        this.setupLanguageSelector();

        this.isInitialized = true;
        console.log(`✅ i18n system initialized with language: ${this.currentLanguage}`);
        return true;
    }

    /**
     * Setup language selector event listener
     */
    setupLanguageSelector() {
        const select = document.getElementById('languageSelect');
        if (select) {
            // Set current value
            select.value = this.currentLanguage;

            // Add event listener
            select.addEventListener('change', async(e) => {
                const newLanguage = e.target.value;
                console.log(`🌐 Language selector changed to: ${newLanguage}`);
                await this.changeLanguage(newLanguage);
            });

            console.log('🌐 Language selector event listener setup');
        }
    }

    /**
     * Update language selector to reflect current language
     */
    updateLanguageSelector() {
        const select = document.getElementById('languageSelect');
        if (select) {
            select.value = this.currentLanguage;
        }
    }

    /**
     * Register listener for language changes
     */
    onLanguageChange(callback) {
        this.languageChangeListeners.push(callback);
    }

    /**
     * Notify all listeners about language change
     */
    notifyLanguageChange(language) {
        this.languageChangeListeners.forEach(callback => {
            try {
                callback(language);
            } catch (error) {
                console.error('🌐 Error in language change listener:', error);
            }
        });
    }

    /**
     * Get current language
     */
    getCurrentLanguage() {
        return this.currentLanguage;
    }

    /**
     * Get supported languages
     */
    getSupportedLanguages() {
        return [...this.supportedLanguages];
    }

    /**
     * Apply translations to DOM elements with data-i18n attributes
     */
    applyTranslations() {
        console.log('🌐 Applying translations to DOM...');

        // Find all elements with data-i18n attribute
        const elements = document.querySelectorAll('[data-i18n]');

        let translatedCount = 0;
        elements.forEach(element => {
            const key = element.getAttribute('data-i18n');
            const params = this.parseDataParams(element.getAttribute('data-i18n-params'));

            const translation = this.t(key, params);

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

        console.log(`✅ Applied translations to ${translatedCount}/${elements.length} elements`);
    }

    /**
     * Parse data-i18n-params attribute
     */
    parseDataParams(paramsStr) {
        if (!paramsStr) return {};

        try {
            return JSON.parse(paramsStr);
        } catch (error) {
            console.warn('🌐 Failed to parse data-i18n-params:', paramsStr);
            return {};
        }
    }

    /**
     * Create language selector UI element
     */
    createLanguageSelector(containerId) {
        const container = document.getElementById(containerId);
        if (!container) {
            console.warn(`🌐 Language selector container "${containerId}" not found`);
            return;
        }

        // Create language selector HTML
        const selectorHtml = `
            <div class="language-selector" style="position: fixed; top: 1rem; right: 4rem; z-index: 1000; background: rgba(255, 255, 255, 0.95); backdrop-filter: blur(10px); border-radius: 8px; padding: 0.5rem; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid rgba(255, 255, 255, 0.2);">
                <label for="languageSelect" style="font-size: 0.875rem; color: #4a5568; margin-right: 0.5rem;" data-i18n="language.select">Sprache wählen:</label>
                <select id="languageSelect" style="border: 1px solid #e2e8f0; border-radius: 4px; padding: 0.25rem 0.5rem; font-size: 0.875rem; background: white; color: #4a5568;">
                    <option value="auto" data-i18n="language.auto">Automatisch (Browser-Sprache)</option>
                    <option value="de" data-i18n="language.german">Deutsch</option>
                    <option value="en" data-i18n="language.english">English</option>
                </select>
            </div>
        `;

        container.innerHTML = selectorHtml;

        // Setup event listener for the new selector
        this.setupLanguageSelector();

        // Apply translations to the newly created elements
        this.applyTranslations();

        console.log('🌐 Language selector created and setup');
    }

    /**
     * Format relative time (used in dashboard)
     */
    formatRelativeTime(date) {
        const now = new Date();
        const time = new Date(date);
        const diffMs = now - time;
        const diffMins = Math.floor(diffMs / 60000);

        if (diffMins < 1) return this.t('dashboard.tournaments.labels.justNow');
        if (diffMins < 60) return this.t('dashboard.tournaments.labels.minutesAgo', { minutes: diffMins });
        if (diffMins < 1440) return this.t('dashboard.tournaments.labels.hoursAgo', { hours: Math.floor(diffMins / 60) });

        return time.toLocaleDateString(this.currentLanguage === 'de' ? 'de-DE' : 'en-US');
    }

    /**
     * Format uptime (used in dashboard) 
     */
    formatUptime(seconds) {
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
    }
}

// Create global instance
const i18n = new I18nManager();

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        i18n.init().then(() => {
            i18n.applyTranslations();
        });
    });
} else {
    i18n.init().then(() => {
        i18n.applyTranslations();
    });
}

// Make available globally
window.i18n = i18n;
window.I18nManager = i18n; // Also expose as I18nManager for compatibility
window.i18nManager = i18n; // Also expose as i18nManager for match page compatibility