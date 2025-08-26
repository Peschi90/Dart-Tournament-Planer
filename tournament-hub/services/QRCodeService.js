const QRCode = require('qrcode');

/**
 * QR Code Service
 * Generates QR codes for tournament join URLs and other purposes
 */
class QRCodeService {
    constructor() { 
        this.defaultOptions = {
            width: 200,
            margin: 2,
            color: {
                dark: '#000000',
                light: '#FFFFFF'
            }, 
            errorCorrectionLevel: 'M'
        };
    }

    /**
     * Generate QR code from text
     * @param {string} text - Text to encode in QR code
     * @param {Object} options - QR code options
     * @returns {Promise<Object>} QR code generation result
     */
    async generateQRCode(text, options = {}) {
        try {
            if (!text || typeof text !== 'string') {
                throw new Error('Text is required and must be a string');
            }

            const qrOptions = { ...this.defaultOptions, ...options };
            const qrCodeDataURL = await QRCode.toDataURL(text, qrOptions);
            
            console.log(`?? QR Code generated for: ${text.substring(0, 50)}...`);
            
            return {
                success: true,
                dataURL: qrCodeDataURL,
                text: text,
                options: qrOptions
            };
        } catch (error) {
            console.error('? QR Code generation error:', error.message);
            return {
                success: false,
                error: error.message
            };
        }
    }

    /**
     * Generate QR code for tournament join URL
     * @param {string} tournamentId - Tournament ID
     * @param {string} baseUrl - Base URL of the hub
     * @param {Object} options - QR code options
     * @returns {Promise<Object>} QR code generation result
     */
    async generateTournamentQR(tournamentId, baseUrl, options = {}) {
        try {
            if (!tournamentId) {
                throw new Error('Tournament ID is required');
            }

            const joinUrl = `${baseUrl || 'http://localhost:3000'}/join/${tournamentId}`;
            
            const qrResult = await this.generateQRCode(joinUrl, {
                ...options,
                width: options.width || 300 // Larger for tournament QRs
            });

            if (qrResult.success) {
                console.log(`?? Tournament QR Code generated for: ${tournamentId}`);
                return {
                    ...qrResult,
                    tournamentId: tournamentId,
                    joinUrl: joinUrl
                };
            }

            return qrResult;
        } catch (error) {
            console.error('? Tournament QR generation error:', error.message);
            return {
                success: false,
                error: error.message
            };
        }
    }

    /**
     * Generate QR code as SVG format
     * @param {string} text - Text to encode
     * @param {Object} options - SVG options
     * @returns {Promise<Object>} SVG QR code result
     */
    async generateSVGQRCode(text, options = {}) {
        try {
            if (!text || typeof text !== 'string') {
                throw new Error('Text is required and must be a string');
            }

            const svgOptions = {
                width: options.width || 200,
                margin: options.margin || 2,
                color: {
                    dark: options.darkColor || '#000000',
                    light: options.lightColor || '#FFFFFF'
                }
            };

            const svgString = await QRCode.toString(text, {
                type: 'svg',
                ...svgOptions
            });

            console.log(`?? SVG QR Code generated for: ${text.substring(0, 50)}...`);
            
            return {
                success: true,
                svg: svgString,
                text: text,
                options: svgOptions
            };
        } catch (error) {
            console.error('? SVG QR Code generation error:', error.message);
            return {
                success: false,
                error: error.message
            };
        }
    }

    /**
     * Generate QR code with custom logo/overlay
     * @param {string} text - Text to encode
     * @param {string} logoPath - Path to logo image
     * @param {Object} options - QR code options
     * @returns {Promise<Object>} QR code with logo result
     */
    async generateQRCodeWithLogo(text, logoPath, options = {}) {
        try {
            // For now, generate standard QR code
            // Logo overlay would require additional image processing
            const qrResult = await this.generateQRCode(text, {
                ...options,
                errorCorrectionLevel: 'H' // High error correction for logo overlay
            });

            if (qrResult.success) {
                return {
                    ...qrResult,
                    logoPath: logoPath,
                    note: 'Logo overlay not yet implemented'
                };
            }

            return qrResult;
        } catch (error) {
            console.error('? QR Code with logo error:', error.message);
            return {
                success: false,
                error: error.message
            };
        }
    }

    /**
     * Generate batch QR codes for multiple tournaments
     * @param {Array} tournaments - Array of tournament objects
     * @param {string} baseUrl - Base URL
     * @param {Object} options - QR code options
     * @returns {Promise<Array>} Array of QR code results
     */
    async generateBatchTournamentQRs(tournaments, baseUrl, options = {}) {
        try {
            if (!Array.isArray(tournaments)) {
                throw new Error('Tournaments must be an array');
            }

            const results = [];
            
            for (const tournament of tournaments) {
                const qrResult = await this.generateTournamentQR(
                    tournament.id || tournament.tournamentId,
                    baseUrl,
                    {
                        ...options,
                        // Add tournament name as metadata
                        tournamentName: tournament.name
                    }
                );
                
                results.push({
                    tournament: tournament,
                    qrCode: qrResult
                });
            }

            console.log(`?? Batch QR generation completed: ${results.length} QR codes`);
            
            return {
                success: true,
                results: results,
                count: results.length
            };
        } catch (error) {
            console.error('? Batch QR generation error:', error.message);
            return {
                success: false,
                error: error.message
            };
        }
    }

    /**
     * Validate QR code text
     * @param {string} text - Text to validate
     * @returns {Object} Validation result
     */
    validateQRText(text) {
        try {
            if (!text) {
                return { valid: false, error: 'Text is required' };
            }

            if (typeof text !== 'string') {
                return { valid: false, error: 'Text must be a string' };
            }

            if (text.length > 2953) {
                return { valid: false, error: 'Text is too long for QR code (max 2953 characters)' };
            }

            // Check if it's a valid URL
            const isUrl = text.startsWith('http://') || text.startsWith('https://');
            
            return {
                valid: true,
                isUrl: isUrl,
                length: text.length
            };
        } catch (error) {
            return {
                valid: false,
                error: error.message
            };
        }
    }

    /**
     * Get QR code statistics
     * @returns {Object} Service statistics
     */
    getStatistics() {
        return {
            service: 'QRCodeService',
            version: '1.0.0',
            supportedFormats: ['PNG (Data URL)', 'SVG'],
            maxDataLength: 2953,
            defaultErrorCorrection: 'M',
            features: [
                'Tournament QR generation',
                'Batch QR generation',
                'SVG output',
                'Custom styling',
                'Text validation'
            ]
        };
    }

    /**
     * Generate QR code for match interface
     * @param {string} tournamentId - Tournament ID
     * @param {string} matchId - Match ID
     * @param {string} baseUrl - Base URL
     * @param {Object} options - QR code options
     * @returns {Promise<Object>} Match QR code result
     */
    async generateMatchQR(tournamentId, matchId, baseUrl, options = {}) {
        try {
            if (!tournamentId || !matchId) {
                throw new Error('Tournament ID and Match ID are required');
            }

            const matchUrl = `${baseUrl || 'http://localhost:3000'}/match/${tournamentId}/${matchId}`;
            
            const qrResult = await this.generateQRCode(matchUrl, options);

            if (qrResult.success) {
                console.log(`?? Match QR Code generated: ${tournamentId}/${matchId}`);
                return {
                    ...qrResult,
                    tournamentId: tournamentId,
                    matchId: matchId,
                    matchUrl: matchUrl
                };
            }

            return qrResult;
        } catch (error) {
            console.error('? Match QR generation error:', error.message);
            return {
                success: false,
                error: error.message
            };
        }
    }
}

module.exports = QRCodeService;