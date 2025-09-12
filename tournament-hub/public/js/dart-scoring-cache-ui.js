/**
 * Dart Scoring Cache UI
 * Handles user interface for cache functionality
 */
class DartScoringCacheUI {
    constructor(core, ui, cache) {
        this.core = core;
        this.ui = ui;
        this.cache = cache;
        
        this.restoreButton = null;
        this.resetButton = null;
        this.statusIndicator = null;        console.log('🎨 [DART-CACHE-UI] Cache UI initialized');
    }

    /**
     * 🚀 Initialisiere Cache UI
     */
    initialize() {
        this.createStatusIndicator();
        this.setupEventListeners();
        console.log('✅ [DART-CACHE-UI] Cache UI ready');
    }

    /**
     * 📊 Erstelle Status-Indikator
     */
    createStatusIndicator() {
        // Status-Indikator in der Header-Area hinzufügen
        const headerControls = document.querySelector('.header-controls');
        if (!headerControls) return;

        // Container für Cache-Status
        const cacheStatusContainer = document.createElement('div');
        cacheStatusContainer.id = 'cacheStatusContainer';
        cacheStatusContainer.style.cssText = `
            display: flex;
            align-items: center;
            gap: 8px;
            margin-right: 10px;
        `;

        // Status-Indikator
        this.statusIndicator = document.createElement('div');
        this.statusIndicator.id = 'cacheStatusIndicator';
        this.statusIndicator.style.cssText = `
            display: flex;
            align-items: center;
            gap: 6px;
            padding: 4px 8px;
            border-radius: 6px;
            font-size: 0.8em;
            font-weight: 600;
            background: rgba(255, 255, 255, 0.1);
            color: white;
            border: 1px solid rgba(255, 255, 255, 0.2);
            transition: all 0.2s ease;
        `;

        // Restore-Button (initial versteckt)
        this.restoreButton = document.createElement('button');
        this.restoreButton.id = 'cacheRestoreButton';
        this.restoreButton.innerHTML = '🔄 Wiederherstellen';
        this.restoreButton.style.cssText = `
            padding: 6px 12px;
            border: none;
            border-radius: 6px;
            background: #f39c12;
            color: white;
            font-weight: 600;
            font-size: 0.9em;
            cursor: pointer;
            transition: all 0.2s ease;
            display: none;
        `;

        this.restoreButton.addEventListener('mouseenter', () => {
            this.restoreButton.style.background = '#e67e22';
            this.restoreButton.style.transform = 'translateY(-1px)';
        });

        this.restoreButton.addEventListener('mouseleave', () => {
            this.restoreButton.style.background = '#f39c12';
            this.restoreButton.style.transform = 'translateY(0)';
        });

        // Reset-Button (immer sichtbar während des Spiels)
        this.resetButton = document.createElement('button');
        this.resetButton.id = 'cacheResetButton';
        this.resetButton.innerHTML = '🔄 Match zurücksetzen';
        this.resetButton.style.cssText = `
            padding: 6px 12px;
            border: none;
            border-radius: 6px;
            background: #e53e3e;
            color: white;
            font-weight: 600;
            font-size: 0.9em;
            cursor: pointer;
            transition: all 0.2s ease;
            margin-left: 8px;
        `;

        this.resetButton.addEventListener('mouseenter', () => {
            this.resetButton.style.background = '#c53030';
            this.resetButton.style.transform = 'translateY(-1px)';
        });

        this.resetButton.addEventListener('mouseleave', () => {
            this.resetButton.style.background = '#e53e3e';
            this.resetButton.style.transform = 'translateY(0)';
        });

        // Zusammenbauen
        cacheStatusContainer.appendChild(this.statusIndicator);
        cacheStatusContainer.appendChild(this.restoreButton);
        cacheStatusContainer.appendChild(this.resetButton);

        // Als erstes Element in header-controls einfügen
        headerControls.insertBefore(cacheStatusContainer, headerControls.firstChild);

        this.updateStatusIndicator('idle');
    }

    /**
     * 🔄 Update Status-Indikator
     */
    updateStatusIndicator(status, details = {}) {
        if (!this.statusIndicator) return;

        const statusConfig = {
            idle: {
                icon: '💾',
                text: 'Auto-Save',
                color: 'rgba(104, 211, 145, 0.8)', // Grün
                border: 'rgba(104, 211, 145, 0.4)'
            },
            saving: {
                icon: '💾',
                text: 'Speichert...',
                color: 'rgba(66, 153, 225, 0.8)', // Blau
                border: 'rgba(66, 153, 225, 0.4)'
            },
            saved: {
                icon: '✅',
                text: 'Gespeichert',
                color: 'rgba(104, 211, 145, 0.8)', // Grün
                border: 'rgba(104, 211, 145, 0.4)'
            },
            error: {
                icon: '⚠️',
                text: 'Fehler',
                color: 'rgba(245, 101, 101, 0.8)', // Rot
                border: 'rgba(245, 101, 101, 0.4)'
            },
            disabled: {
                icon: '❌',
                text: 'Deaktiviert',
                color: 'rgba(160, 174, 192, 0.8)', // Grau
                border: 'rgba(160, 174, 192, 0.4)'
            }
        };

        const config = statusConfig[status] || statusConfig.idle;

        this.statusIndicator.innerHTML = `
            <span style="font-size: 1.1em;">${config.icon}</span>
            <span>${config.text}</span>
        `;

        this.statusIndicator.style.background = config.color;
        this.statusIndicator.style.borderColor = config.border;

        // Auto-hide success status
        if (status === 'saved') {
            setTimeout(() => {
                this.updateStatusIndicator('idle');
            }, 2000);
        }
    }

    /**
     * 🔄 Zeige/Verstecke Restore-Button
     */
    updateRestoreButton(show, canRestore = false) {
        if (!this.restoreButton) return;

        if (show && canRestore) {
            this.restoreButton.style.display = 'inline-flex';
            this.restoreButton.disabled = false;
            this.restoreButton.style.opacity = '1';
            this.restoreButton.style.cursor = 'pointer';
        } else if (show && !canRestore) {
            this.restoreButton.style.display = 'inline-flex';
            this.restoreButton.disabled = true;
            this.restoreButton.style.opacity = '0.5';
            this.restoreButton.style.cursor = 'not-allowed';
            this.restoreButton.innerHTML = '🔄 Kein Spielstand';
        } else {
            this.restoreButton.style.display = 'none';
        }
    }

    /**
     * 💬 Zeige Reset-Dialog
     */
    async showResetDialog() {
        return new Promise((resolve) => {
            const modal = document.createElement('div');
            modal.className = 'modal-overlay';
            modal.id = 'resetModal';
            
            modal.innerHTML = `
                <div class="modal" style="max-width: 600px;">
                    <h2>🔄 Match zurücksetzen</h2>
                    <div style="margin: 25px 0; text-align: left; line-height: 1.6;">
                        <div style="background: #fff5f5; padding: 20px; border-radius: 10px; margin-bottom: 20px; border-left: 4px solid #fed7d7;">
                            <p style="margin-bottom: 15px; color: #742a2a;">
                                <strong>⚠️ Achtung: Diese Aktion kann nicht rückgängig gemacht werden!</strong>
                            </p>
                            <p style="color: #4a5568; margin-bottom: 15px;">
                                Das Match wird vollständig zurückgesetzt und alle Spielfortschritte gehen verloren:
                            </p>
                            <ul style="margin: 15px 0 15px 20px; color: #4a5568;">
                                <li>✗ Alle Würfe und Scores werden gelöscht</li>
                                <li>✗ Leg- und Set-Fortschritt wird zurückgesetzt</li>
                                <li>✗ Wurf-Historie wird geleert</li>
                                <li>✗ Gespeicherte Cache-Daten werden entfernt</li>
                            </ul>
                        </div>
                        <div style="background: #f7fafc; padding: 15px; border-radius: 8px; margin-bottom: 20px;">
                            <h4 style="color: #2d3748; margin-bottom: 10px;">Wann sollten Sie das Match zurücksetzen?</h4>
                            <div style="margin-left: 15px; color: #4a5568;">
                                <p style="margin: 8px 0;">• Falsches Match wurde versehentlich gestartet</p>
                                <p style="margin: 8px 0;">• Falsche Spieler wurden eingegeben</p>
                                <p style="margin: 8px 0;">• Spiel soll komplett neu gestartet werden</p>
                            </div>
                        </div>
                        <div style="background: #edf2f7; padding: 15px; border-radius: 8px;">
                            <p style="color: #4a5568; font-size: 0.9em; margin: 0;">
                                💡 <strong>Alternative:</strong> Wenn Sie nur den aktuellen Wurf korrigieren möchten, verwenden Sie die "Wurf rückgängig" Funktion im Spiel.
                            </p>
                        </div>
                    </div>
                    <div class="modal-buttons">
                        <button class="btn-secondary" id="resetCancel" style="font-size: 1.1em; padding: 12px 24px;">
                            ❌ Abbrechen
                        </button>
                        <button class="btn-primary" id="resetConfirm" style="font-size: 1.1em; padding: 12px 24px; background: #e53e3e; border-color: #c53030;">
                            🔄 Match zurücksetzen
                        </button>
                    </div>
                </div>
            `;

            document.body.appendChild(modal);

            const cleanup = () => {
                document.body.removeChild(modal);
            };

            document.getElementById('resetCancel').onclick = () => {
                cleanup();
                resolve(false);
            };

            document.getElementById('resetConfirm').onclick = () => {
                cleanup();
                resolve(true);
            };

            // ESC-Taste Handling
            const escHandler = (e) => {
                if (e.key === 'Escape') {
                    cleanup();
                    document.removeEventListener('keydown', escHandler);
                    resolve(false);
                }
            };
            document.addEventListener('keydown', escHandler);
        });
    }

    /**
     * 💬 Zeige Reset-Erfolg-Nachricht
     */
    showResetSuccess() {
        if (this.ui && this.ui.showMessage) {
            this.ui.showMessage('🔄 Match erfolgreich zurückgesetzt!', 'success', 3000);
        }
    }

    /**
     * 💬 Zeige Reset-Fehler-Nachricht
     */
    showResetError(error) {
        if (this.ui && this.ui.showMessage) {
            this.ui.showMessage(`❌ Reset fehlgeschlagen: ${error}`, 'error', 5000);
        }
    }

    /**
     * 💬 Zeige Restore-Dialog
     */
    async showRestoreDialog(cacheInfo = {}) {
        return new Promise((resolve) => {
            const modal = document.createElement('div');
            modal.className = 'modal-overlay';
            modal.id = 'restoreModal';

            const ageText = cacheInfo.age ? this.formatAge(cacheInfo.age) : 'unbekannt';
            const lastUpdated = cacheInfo.lastUpdated ?
                new Date(cacheInfo.lastUpdated).toLocaleString('de-DE') : 'unbekannt';

            modal.innerHTML = `
                <div class="modal" style="max-width: 600px;">
                    <h2>🔄 Gespeicherter Spielstand gefunden</h2>
                    <div style="margin: 25px 0; text-align: left; line-height: 1.6;">
                        <div style="background: #f7fafc; padding: 20px; border-radius: 10px; margin-bottom: 20px;">
                            <p style="margin-bottom: 15px;">
                                <strong>Es wurde ein automatisch gespeicherter Spielstand für dieses Match gefunden.</strong>
                            </p>
                            <div style="display: grid; grid-template-columns: auto 1fr; gap: 10px 15px; font-size: 0.9em; color: #4a5568;">
                                <span><strong>Gespeichert:</strong></span>
                                <span>${lastUpdated}</span>
                                <span><strong>Alter:</strong></span>
                                <span>${ageText}</span>
                            </div>
                        </div>
                        <div style="background: #edf2f7; padding: 15px; border-radius: 8px; margin-bottom: 20px;">
                            <h4 style="color: #2d3748; margin-bottom: 10px;">Was möchten Sie tun?</h4>
                            <div style="margin-left: 15px;">
                                <p style="margin: 8px 0;"><strong>🔄 Wiederherstellen:</strong> Den gespeicherten Spielstand laden und weiterspielen</p>
                                <p style="margin: 8px 0;"><strong>🆕 Neu starten:</strong> Ein neues Spiel beginnen (der alte Spielstand wird gelöscht)</p>
                            </div>
                        </div>
                        <div style="background: #fff5f5; padding: 15px; border-radius: 8px; border-left: 4px solid #fed7d7;">
                            <p style="color: #4a5568; font-size: 0.9em; margin: 0;">
                                💡 <strong>Hinweis:</strong> Der Spielstand wird automatisch geräteübergreifend synchronisiert und kann von jedem Gerät aus wiederhergestellt werden.
                            </p>
                        </div>
                    </div>
                    <div class="modal-buttons">
                        <button class="btn-primary" id="restoreYes" style="font-size: 1.1em; padding: 12px 24px;">
                            🔄 Spielstand wiederherstellen
                        </button>
                        <button class="btn-secondary" id="restoreNo" style="font-size: 1.1em; padding: 12px 24px;">
                            🆕 Neues Spiel starten
                        </button>
                    </div>
                </div>
            `;

            document.body.appendChild(modal);

            const cleanup = () => {
                document.body.removeChild(modal);
            };

            document.getElementById('restoreYes').onclick = () => {
                cleanup();
                resolve(true);
            };

            document.getElementById('restoreNo').onclick = () => {
                cleanup();
                resolve(false);
            };

            // ESC-Taste Handling
            const escHandler = (e) => {
                if (e.key === 'Escape') {
                    cleanup();
                    document.removeEventListener('keydown', escHandler);
                    resolve(false);
                }
            };
            document.addEventListener('keydown', escHandler);
        });
    }

    /**
     * 💬 Zeige Restore-Erfolg-Nachricht
     */
    showRestoreSuccess(details = {}) {
        if (this.ui && this.ui.showMessage) {
            const ageText = details.age ? this.formatAge(details.age) : '';
            const message = `🔄 Spielstand wiederhergestellt! ${ageText}`;
            this.ui.showMessage(message, 'success', 3000);
        }
    }

    /**
     * 💬 Zeige Restore-Fehler-Nachricht
     */
    showRestoreError(error) {
        if (this.ui && this.ui.showMessage) {
            this.ui.showMessage(`❌ Wiederherstellung fehlgeschlagen: ${error}`, 'error', 5000);
        }
    }

    /**
     * 📄 Event Listeners setup
     */
    setupEventListeners() {
        // Restore-Button Click
        if (this.restoreButton) {
            this.restoreButton.addEventListener('click', async() => {
                console.log('🔄 [DART-CACHE-UI] Manual restore triggered');

                this.restoreButton.disabled = true;
                this.restoreButton.innerHTML = '🔄 Lädt...';

                try {
                    // Prüfe zuerst ob State existiert
                    const hasCache = await this.cache.checkForCachedState();

                    if (!hasCache) {
                        this.showRestoreError('Kein gespeicherter Spielstand gefunden');
                        return;
                    }

                    // Zeige Dialog
                    const shouldRestore = await this.showRestoreDialog();

                    if (shouldRestore) {
                        const result = await this.cache.loadCachedState();

                        if (result.success) {
                            // Update UI
                            this.ui.updateDisplay();
                            this.showRestoreSuccess({ age: result.age });
                            this.updateRestoreButton(false);
                        } else {
                            this.showRestoreError(result.message);
                        }
                    } else {
                        // User wählte "Neu starten" - lösche Cache
                        await this.cache.clearCachedState();
                        this.updateRestoreButton(false);

                        if (this.ui && this.ui.showMessage) {
                            this.ui.showMessage('🆕 Neues Spiel gestartet', 'info', 2000);
                        }
                    }

                } catch (error) {
                    console.error('❌ [DART-CACHE-UI] Manual restore failed:', error);
                    this.showRestoreError(error.message);
                } finally {
                    this.restoreButton.disabled = false;
                    this.restoreButton.innerHTML = '🔄 Wiederherstellen';
                }
            });
        }

        // Reset-Button Click
        if (this.resetButton) {
            this.resetButton.addEventListener('click', async () => {
                console.log('🔄 [DART-CACHE-UI] Reset match triggered');
                
                // Zeige Bestätigungs-Dialog
                const shouldReset = await this.showResetDialog();
                
                if (!shouldReset) {
                    console.log('🔄 [DART-CACHE-UI] Reset cancelled by user');
                    return;
                }

                this.resetButton.disabled = true;
                this.resetButton.innerHTML = '🔄 Setzt zurück...';
                
                try {
                    const result = await this.cache.resetMatchToOriginal();
                    
                    if (result.success) {
                        // Update UI komplett
                        this.ui.updateDisplay();
                        this.showResetSuccess();
                        this.updateRestoreButton(false);
                        this.updateStatusIndicator('idle');
                    } else {
                        this.showResetError(result.message);
                    }

                } catch (error) {
                    console.error('❌ [DART-CACHE-UI] Reset failed:', error);
                    this.showResetError(error.message);
                } finally {
                    this.resetButton.disabled = false;
                    this.resetButton.innerHTML = '🔄 Match zurücksetzen';
                }
            });
        }

        // Listen to cache events
        if (this.cache) {
            // Override cache save method to update UI
            const originalSave = this.cache.saveCurrentState.bind(this.cache);
            this.cache.saveCurrentState = async(...args) => {
                this.updateStatusIndicator('saving');

                const result = await originalSave(...args);

                if (result.success) {
                    this.updateStatusIndicator('saved');
                } else {
                    this.updateStatusIndicator('error');
                }

                return result;
            };
        }
    }

    /**
     * 🕒 Format age für Display
     */
    formatAge(ageMs) {
        const seconds = Math.floor(ageMs / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);

        if (days > 0) {
            return `vor ${days} Tag${days !== 1 ? 'en' : ''}`;
        } else if (hours > 0) {
            return `vor ${hours} Stunde${hours !== 1 ? 'n' : ''}`;
        } else if (minutes > 0) {
            return `vor ${minutes} Minute${minutes !== 1 ? 'n' : ''}`;
        } else {
            return `vor ${seconds} Sekunde${seconds !== 1 ? 'n' : ''}`;
        }
    }

    /**
     * 🔧 Update UI basierend auf Cache-Status
     */
    updateCacheUI() {
        const status = this.cache.getCacheStatus();

        if (!status.isEnabled) {
            this.updateStatusIndicator('disabled');
            this.updateRestoreButton(false);
        } else if (status.autoSaveActive) {
            this.updateStatusIndicator('idle');
        }
    }

    /**
     * 🧹 Cleanup
     */
    cleanup() {
        // Remove event listeners and DOM elements
        if (this.statusIndicator && this.statusIndicator.parentNode) {
            this.statusIndicator.parentNode.removeChild(this.statusIndicator);
        }

        if (this.restoreButton && this.restoreButton.parentNode) {
            this.restoreButton.parentNode.removeChild(this.restoreButton);
        }

        if (this.resetButton && this.resetButton.parentNode) {
            this.resetButton.parentNode.removeChild(this.resetButton);
        }

        console.log('🧹 [DART-CACHE-UI] Cache UI cleaned up');
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringCacheUI;
} else {
    window.DartScoringCacheUI = DartScoringCacheUI;
}

console.log('🎨 [DART-CACHE-UI] Dart Scoring Cache UI module loaded');