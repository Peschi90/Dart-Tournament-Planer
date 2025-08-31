/**
 * Dart Scoring Modals Module
 * Handles modal dialogs and overlays
 */
class DartScoringModals {
    constructor(ui) {
        this.ui = ui;
        this.activeModals = new Set();

        console.log('💬 [DART-MODALS] Dart Scoring Modals initialized');
    }

    /**
     * Show a confirmation dialog
     */
    showConfirmation(title, message, onConfirm, onCancel) {
        const modal = this.createModal('confirmation', {
            title: title,
            message: message,
            buttons: [{
                    text: 'Bestätigen',
                    style: 'primary',
                    callback: () => {
                        this.hideModal(modal);
                        if (onConfirm) onConfirm();
                    }
                },
                {
                    text: 'Abbrechen',
                    style: 'secondary',
                    callback: () => {
                        this.hideModal(modal);
                        if (onCancel) onCancel();
                    }
                }
            ]
        });

        this.showModal(modal);
        return modal;
    }

    /**
     * Show an information dialog
     */
    showInfo(title, message, onClose) {
        const modal = this.createModal('info', {
            title: title,
            message: message,
            buttons: [{
                text: 'OK',
                style: 'primary',
                callback: () => {
                    this.hideModal(modal);
                    if (onClose) onClose();
                }
            }]
        });

        this.showModal(modal);
        return modal;
    }

    /**
     * Show an error dialog
     */
    showError(title, message, onClose) {
        const modal = this.createModal('error', {
            title: title,
            message: message,
            buttons: [{
                text: 'OK',
                style: 'secondary',
                callback: () => {
                    this.hideModal(modal);
                    if (onClose) onClose();
                }
            }]
        });

        this.showModal(modal);
        return modal;
    }

    /**
     * Create a modal element
     */
    createModal(type, options) {
        const modalId = `modal-${type}-${Date.now()}`;

        const overlay = document.createElement('div');
        overlay.className = 'modal-overlay';
        overlay.id = modalId;

        const modal = document.createElement('div');
        modal.className = `modal modal-${type}`;

        // Title
        const title = document.createElement('h2');
        title.textContent = options.title;
        modal.appendChild(title);

        // Message
        const message = document.createElement('div');
        message.innerHTML = options.message;
        modal.appendChild(message);

        // Buttons
        if (options.buttons && options.buttons.length > 0) {
            const buttonContainer = document.createElement('div');
            buttonContainer.className = 'modal-buttons';

            options.buttons.forEach(buttonConfig => {
                const button = document.createElement('button');
                button.className = `btn btn-${buttonConfig.style}`;
                button.textContent = buttonConfig.text;
                button.addEventListener('click', buttonConfig.callback);
                buttonContainer.appendChild(button);
            });

            modal.appendChild(buttonContainer);
        }

        overlay.appendChild(modal);

        // Close on overlay click
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                this.hideModal(overlay);
            }
        });

        return overlay;
    }

    /**
     * Show a modal
     */
    showModal(modal) {
        document.body.appendChild(modal);
        this.activeModals.add(modal);

        // Fade in animation
        requestAnimationFrame(() => {
            modal.style.opacity = '0';
            modal.style.transition = 'opacity 0.3s ease';
            requestAnimationFrame(() => {
                modal.style.opacity = '1';
            });
        });

        console.log('💬 [DART-MODALS] Modal shown:', modal.id);
    }

    /**
     * Hide a modal
     */
    hideModal(modal) {
        if (!modal || !this.activeModals.has(modal)) return;

        modal.style.transition = 'opacity 0.3s ease';
        modal.style.opacity = '0';

        setTimeout(() => {
            if (modal.parentNode) {
                modal.parentNode.removeChild(modal);
            }
            this.activeModals.delete(modal);
        }, 300);

        console.log('💬 [DART-MODALS] Modal hidden:', modal.id);
    }

    /**
     * Hide all modals
     */
    hideAllModals() {
        this.activeModals.forEach(modal => {
            this.hideModal(modal);
        });
    }

    /**
     * Check if any modal is open
     */
    hasActiveModals() {
        return this.activeModals.size > 0;
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringModals;
} else {
    window.DartScoringModals = DartScoringModals;
}

console.log('💬 [DART-MODALS] Dart Scoring Modals module loaded');