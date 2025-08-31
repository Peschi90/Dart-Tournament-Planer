/**
 * Dart Scoring Animations Module
 * Handles animations and visual effects
 */
class DartScoringAnimations {
    constructor(ui) {
        this.ui = ui;
        this.activeAnimations = new Map();

        console.log('🎬 [DART-ANIMATIONS] Dart Scoring Animations initialized');
    }

    /**
     * Animate dart entry
     */
    animateDartEntry(dartElement, value) {
        if (!dartElement) return;

        // Store original styles
        const originalTransform = dartElement.style.transform;
        const originalBoxShadow = dartElement.style.boxShadow;

        // Animate entry
        dartElement.style.transition = 'all 0.3s ease';
        dartElement.style.transform = 'scale(1.1)';
        dartElement.style.boxShadow = '0 0 15px rgba(66, 153, 225, 0.5)';

        setTimeout(() => {
            dartElement.style.transform = originalTransform;
            dartElement.style.boxShadow = originalBoxShadow;
        }, 300);
    }

    /**
     * Animate throw total update
     */
    animateThrowTotal(totalElement, value) {
        if (!totalElement) return;

        totalElement.style.transition = 'all 0.2s ease';
        totalElement.style.transform = 'scale(1.15)';

        // Color animation based on value
        if (value >= 140) {
            totalElement.style.color = '#38a169'; // Green for excellent
        } else if (value >= 100) {
            totalElement.style.color = '#4299e1'; // Blue for good
        }

        setTimeout(() => {
            totalElement.style.transform = 'scale(1)';
        }, 200);
    }

    /**
     * Animate score update
     */
    animateScoreUpdate(scoreElement, newScore, previousScore) {
        if (!scoreElement) return;

        const difference = previousScore - newScore;

        // Create floating animation element
        const floatingScore = document.createElement('div');
        floatingScore.textContent = `-${difference}`;
        floatingScore.style.cssText = `
            position: absolute;
            top: 0;
            right: 0;
            color: #38a169;
            font-weight: bold;
            font-size: 1.2em;
            pointer-events: none;
            z-index: 1000;
            animation: floatUp 2s ease-out forwards;
        `;

        scoreElement.parentElement.style.position = 'relative';
        scoreElement.parentElement.appendChild(floatingScore);

        // Update main score with animation
        scoreElement.style.transition = 'all 0.3s ease';
        scoreElement.style.transform = 'scale(1.1)';
        scoreElement.textContent = newScore;

        setTimeout(() => {
            scoreElement.style.transform = 'scale(1)';
        }, 300);

        // Remove floating element
        setTimeout(() => {
            if (floatingScore.parentElement) {
                floatingScore.parentElement.removeChild(floatingScore);
            }
        }, 2000);
    }

    /**
     * Animate player switch
     */
    animatePlayerSwitch(newActivePlayer, previousActivePlayer) {
        if (newActivePlayer) {
            newActivePlayer.style.transition = 'all 0.4s ease';
            newActivePlayer.style.transform = 'scale(1.02)';

            setTimeout(() => {
                newActivePlayer.style.transform = 'scale(1)';
            }, 400);
        }

        if (previousActivePlayer) {
            previousActivePlayer.style.transition = 'all 0.4s ease';
            previousActivePlayer.style.opacity = '0.7';

            setTimeout(() => {
                previousActivePlayer.style.opacity = '1';
            }, 400);
        }
    }

    /**
     * Animate bust indication
     */
    animateBust(playerElement) {
        if (!playerElement) return;

        playerElement.style.transition = 'all 0.1s ease';

        // Shake animation
        const shakeFrames = [
            'translateX(0)',
            'translateX(-10px)',
            'translateX(10px)',
            'translateX(-5px)',
            'translateX(5px)',
            'translateX(0)'
        ];

        let frameIndex = 0;
        const shakeInterval = setInterval(() => {
            playerElement.style.transform = shakeFrames[frameIndex];
            frameIndex++;

            if (frameIndex >= shakeFrames.length) {
                clearInterval(shakeInterval);
            }
        }, 50);

        // Red flash
        const originalBackground = playerElement.style.backgroundColor;
        playerElement.style.backgroundColor = 'rgba(229, 62, 62, 0.1)';

        setTimeout(() => {
            playerElement.style.backgroundColor = originalBackground;
        }, 300);
    }

    /**
     * Animate checkout/win
     */
    animateCheckout(playerElement, checkoutValue) {
        if (!playerElement) return;

        // Golden glow effect
        playerElement.style.transition = 'all 0.5s ease';
        playerElement.style.boxShadow = '0 0 30px rgba(255, 215, 0, 0.8)';
        playerElement.style.backgroundColor = 'rgba(255, 215, 0, 0.1)';

        // Create checkout value popup
        const checkoutPopup = document.createElement('div');
        checkoutPopup.textContent = `🎯 CHECKOUT ${checkoutValue}!`;
        checkoutPopup.style.cssText = `
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(45deg, #ffd700, #ffed4e);
            color: #2d3748;
            padding: 10px 20px;
            border-radius: 25px;
            font-weight: bold;
            font-size: 1.2em;
            box-shadow: 0 10px 25px rgba(255, 215, 0, 0.5);
            pointer-events: none;
            z-index: 1500;
            animation: checkoutPulse 2s ease-out;
        `;

        playerElement.style.position = 'relative';
        playerElement.appendChild(checkoutPopup);

        // Clean up after animation
        setTimeout(() => {
            playerElement.style.boxShadow = '';
            playerElement.style.backgroundColor = '';

            if (checkoutPopup.parentElement) {
                checkoutPopup.parentElement.removeChild(checkoutPopup);
            }
        }, 2000);
    }

    /**
     * Animate maximum (180)
     */
    animateMaximum(throwTotalElement) {
        if (!throwTotalElement) return;

        // Rainbow effect for 180
        throwTotalElement.style.background = 'linear-gradient(45deg, #ff0000, #ff7f00, #ffff00, #00ff00, #0000ff, #4b0082, #9400d3)';
        throwTotalElement.style.backgroundSize = '400% 400%';
        throwTotalElement.style.animation = 'rainbowShift 2s ease-in-out';
        throwTotalElement.style.color = 'white';
        throwTotalElement.style.fontWeight = 'bold';
        throwTotalElement.style.textShadow = '2px 2px 4px rgba(0,0,0,0.5)';

        // Create 180 popup
        const maximumPopup = document.createElement('div');
        maximumPopup.textContent = '🎯 MAXIMUM 180! 🎯';
        maximumPopup.style.cssText = `
            position: fixed;
            top: 20%;
            left: 50%;
            transform: translateX(-50%);
            background: linear-gradient(45deg, #ff6b6b, #ffa500);
            color: white;
            padding: 15px 30px;
            border-radius: 15px;
            font-weight: bold;
            font-size: 1.5em;
            box-shadow: 0 15px 35px rgba(255, 107, 107, 0.5);
            pointer-events: none;
            z-index: 2000;
            animation: maximumBounce 3s ease-out;
        `;

        document.body.appendChild(maximumPopup);

        // Clean up
        setTimeout(() => {
            throwTotalElement.style.background = '';
            throwTotalElement.style.backgroundSize = '';
            throwTotalElement.style.animation = '';
            throwTotalElement.style.color = '';
            throwTotalElement.style.fontWeight = '';
            throwTotalElement.style.textShadow = '';

            if (maximumPopup.parentElement) {
                maximumPopup.parentElement.removeChild(maximumPopup);
            }
        }, 3000);
    }

    /**
     * Initialize CSS animations
     */
    initializeCSS() {
        if (document.getElementById('dart-animations-css')) return;

        const style = document.createElement('style');
        style.id = 'dart-animations-css';
        style.textContent = `
            @keyframes floatUp {
                0% {
                    opacity: 1;
                    transform: translateY(0);
                }
                100% {
                    opacity: 0;
                    transform: translateY(-50px);
                }
            }
            
            @keyframes checkoutPulse {
                0% {
                    transform: translate(-50%, -50%) scale(0.8);
                    opacity: 0;
                }
                20% {
                    transform: translate(-50%, -50%) scale(1.2);
                    opacity: 1;
                }
                80% {
                    transform: translate(-50%, -50%) scale(1);
                    opacity: 1;
                }
                100% {
                    transform: translate(-50%, -50%) scale(0.8);
                    opacity: 0;
                }
            }
            
            @keyframes rainbowShift {
                0% { background-position: 0% 50%; }
                50% { background-position: 100% 50%; }
                100% { background-position: 0% 50%; }
            }
            
            @keyframes maximumBounce {
                0% {
                    transform: translateX(-50%) scale(0) rotate(-5deg);
                    opacity: 0;
                }
                20% {
                    transform: translateX(-50%) scale(1.2) rotate(2deg);
                    opacity: 1;
                }
                40% {
                    transform: translateX(-50%) scale(0.9) rotate(-1deg);
                }
                60% {
                    transform: translateX(-50%) scale(1.1) rotate(1deg);
                }
                80% {
                    transform: translateX(-50%) scale(1) rotate(0deg);
                }
                90% {
                    transform: translateX(-50%) scale(1) rotate(0deg);
                    opacity: 1;
                }
                100% {
                    transform: translateX(-50%) scale(0.8) rotate(0deg);
                    opacity: 0;
                }
            }
        `;

        document.head.appendChild(style);
        console.log('🎬 [DART-ANIMATIONS] CSS animations initialized');
    }

    /**
     * Initialize animations
     */
    initialize() {
        this.initializeCSS();
        console.log('🎬 [DART-ANIMATIONS] Animations system ready');
    }

    /**
     * Clean up animations
     */
    cleanup() {
        this.activeAnimations.clear();
        console.log('🎬 [DART-ANIMATIONS] Animations cleaned up');
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DartScoringAnimations;
} else {
    window.DartScoringAnimations = DartScoringAnimations;
}

console.log('🎬 [DART-ANIMATIONS] Dart Scoring Animations module loaded');