/**
 * Dart Scoring Keyboard Module
 * Handles keyboard input and shortcuts for dart scoring
 */
class DartScoringKeyboard {
    constructor(ui) {
        this.ui = ui;
        this.isEnabled = true;

        console.log('⌨️ [DART-KEYBOARD] Dart Scoring Keyboard initialized');
    }

    /**
     * Initialize keyboard shortcuts
     */
    initialize() {
        this.setupKeyboardShortcuts();
        console.log('⌨️ [DART-KEYBOARD] Keyboard shortcuts enabled');
    }

    /**
     * Setup keyboard event listeners
     */
    setupKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            if (!this.isEnabled) return;

            // Ignore if typing in input fields
            if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;

            // Prevent default for handled keys
            const handledKeys = [
                'Enter', 'Backspace', 'Delete',
                '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
                'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p',
                'd', 'D', 'T', 's', 'S', 'b', 'B', 'm', 'M'
            ];

            if (handledKeys.includes(e.key)) {
                e.preventDefault();
            }

            this.handleKeyPress(e);
        });
    }

    /**
     * Handle individual key presses
     */
    handleKeyPress(e) {
        // Handle number keys (1-9, 0 for 10)
        if (e.key >= '1' && e.key <= '9') {
            const number = parseInt(e.key);
            this.ui.handleNumberInput(number);
        } else if (e.key === '0') {
            this.ui.handleNumberInput(10);
        }

        // Handle number keys for 11-20 (QWERTY row)
        const numberMap = {
            'q': 11,
            'w': 12,
            'e': 13,
            'r': 14,
            't': 15,
            'y': 16,
            'u': 17,
            'i': 18,
            'o': 19,
            'p': 20
        };

        if (numberMap[e.key.toLowerCase()]) {
            this.ui.handleNumberInput(numberMap[e.key.toLowerCase()]);
        }

        // Handle multipliers (both uppercase and lowercase)
        switch (e.key) {
            case 'S':
            case 's': // Single
                this.ui.handleMultiplierInput(1);
                break;
            case 'D':
            case 'd': // Double
                this.ui.handleMultiplierInput(2);
                break;
            case 'T':
            case 't': // Triple
                this.ui.handleMultiplierInput(3);
                break;
        }

        // Handle special targets
        switch (e.key) {
            case 'b': // Bull (lowercase)
                this.ui.handleSpecialInput('bull');
                break;
            case 'B': // Bullseye (Shift+B)
                this.ui.handleSpecialInput('bullseye');
                break;
            case 'm':
            case 'M': // Miss
                this.ui.handleSpecialInput('miss');
                break;
        }

        // Handle control keys
        switch (e.key) {
            case 'Enter':
                if (this.ui.elements.confirmThrow && !this.ui.elements.confirmThrow.disabled) {
                    this.ui.handleConfirmThrow();
                }
                break;
            case 'Backspace':
                if (this.ui.handleUndoLastDart) {
                    this.ui.handleUndoLastDart();
                }
                break;
            case 'Delete':
                if (this.ui.handleUndoThrow) {
                    this.ui.handleUndoThrow();
                }
                break;
        }
    }

    /**
     * Enable keyboard shortcuts
     */
    enable() {
        this.isEnabled = true;
        console.log('⌨️ [DART-KEYBOARD] Keyboard shortcuts enabled');
    }

    /**
     * Disable keyboard shortcuts
     */
    disable() {
        this.isEnabled = false;
        console.log('⌨️ [DART-KEYBOARD] Keyboard shortcuts disabled');
    }

    /**
     * Get keyboard help text
     */
    getHelpText() {
        return `
        📋 Keyboard Shortcuts:
        Numbers: 1-9, 0(=10), Q-P(=11-20)
        Multipliers: S(Single), D(Double), T(Triple)
        Special: b(Bull), Shift+B(Bullseye), m(Miss)
        Controls: Enter(Confirm), Backspace(Undo Dart), Delete(Undo Throw)
        `;
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringKeyboard;
} else {
    window.DartScoringKeyboard = DartScoringKeyboard;
}

console.log('⌨️ [DART-KEYBOARD] Dart Scoring Keyboard module loaded');