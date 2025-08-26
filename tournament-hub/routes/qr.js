const express = require('express');
const router = express.Router();

/**
 * QR Code Routes
 * Handles QR code generation for tournaments and matches
 */

/**
 * Generate QR code for tournament join
 * POST /api/qr/tournament
 */
router.post('/tournament', async (req, res) => {
    try {
        const { qrService, tournamentRegistry } = req.app.locals;
        const { tournamentId, options } = req.body;

        if (!tournamentId) {
            return res.status(400).json({
                success: false,
                error: 'Tournament ID is required'
            });
        }

        // Verify tournament exists
        const tournament = tournamentRegistry.getTournament(tournamentId);
        if (!tournament) {
            return res.status(404).json({
                success: false,
                error: 'Tournament not found'
            });
        }

        const baseUrl = process.env.BASE_URL || `http://localhost:${process.env.PORT || 3000}`;
        const qrResult = await qrService.generateTournamentQR(tournamentId, baseUrl, options);

        if (qrResult.success) {
            res.json({
                success: true,
                message: 'Tournament QR code generated successfully', 
                data: {
                    tournamentId: qrResult.tournamentId,
                    joinUrl: qrResult.joinUrl,
                    qrCode: qrResult.dataURL,
                    tournament: {
                        name: tournament.name,
                        description: tournament.description
                    }
                }
            });
        } else {
            res.status(500).json({
                success: false,
                error: qrResult.error
            });
        }
    } catch (error) {
        console.error('? Generate tournament QR error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Generate QR code for match interface
 * POST /api/qr/match
 */
router.post('/match', async (req, res) => {
    try {
        const { qrService, tournamentRegistry } = req.app.locals;
        const { tournamentId, matchId, options } = req.body;

        if (!tournamentId || !matchId) {
            return res.status(400).json({
                success: false,
                error: 'Tournament ID and Match ID are required'
            });
        }

        // Verify tournament exists
        const tournament = tournamentRegistry.getTournament(tournamentId);
        if (!tournament) {
            return res.status(404).json({
                success: false,
                error: 'Tournament not found'
            });
        }

        const baseUrl = process.env.BASE_URL || `http://localhost:${process.env.PORT || 3000}`;
        const qrResult = await qrService.generateMatchQR(tournamentId, matchId, baseUrl, options);

        if (qrResult.success) {
            res.json({
                success: true,
                message: 'Match QR code generated successfully',
                data: {
                    tournamentId: qrResult.tournamentId,
                    matchId: qrResult.matchId,
                    matchUrl: qrResult.matchUrl,
                    qrCode: qrResult.dataURL,
                    tournament: {
                        name: tournament.name,
                        description: tournament.description
                    }
                }
            });
        } else {
            res.status(500).json({
                success: false,
                error: qrResult.error
            });
        }
    } catch (error) {
        console.error('? Generate match QR error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Generate custom QR code
 * POST /api/qr/generate
 */
router.post('/generate', async (req, res) => {
    try {
        const { qrService } = req.app.locals;
        const { text, options } = req.body;

        if (!text) {
            return res.status(400).json({
                success: false,
                error: 'Text is required'
            });
        }

        // Validate text
        const validation = qrService.validateQRText(text);
        if (!validation.valid) {
            return res.status(400).json({
                success: false,
                error: validation.error
            });
        }

        const qrResult = await qrService.generateQRCode(text, options);

        if (qrResult.success) {
            res.json({
                success: true,
                message: 'QR code generated successfully',
                data: {
                    text: qrResult.text,
                    qrCode: qrResult.dataURL,
                    isUrl: validation.isUrl,
                    textLength: validation.length
                }
            });
        } else {
            res.status(500).json({
                success: false,
                error: qrResult.error
            });
        }
    } catch (error) {
        console.error('? Generate QR error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Generate SVG QR code
 * POST /api/qr/svg
 */
router.post('/svg', async (req, res) => {
    try {
        const { qrService } = req.app.locals;
        const { text, options } = req.body;

        if (!text) {
            return res.status(400).json({
                success: false,
                error: 'Text is required'
            });
        }

        const qrResult = await qrService.generateSVGQRCode(text, options);

        if (qrResult.success) {
            res.setHeader('Content-Type', 'application/json');
            res.json({
                success: true,
                message: 'SVG QR code generated successfully',
                data: {
                    text: qrResult.text,
                    svg: qrResult.svg
                }
            });
        } else {
            res.status(500).json({
                success: false,
                error: qrResult.error
            });
        }
    } catch (error) {
        console.error('? Generate SVG QR error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Generate batch QR codes for multiple tournaments
 * POST /api/qr/batch
 */
router.post('/batch', async (req, res) => {
    try {
        const { qrService, tournamentRegistry } = req.app.locals;
        const { tournamentIds, options } = req.body;

        if (!Array.isArray(tournamentIds) || tournamentIds.length === 0) {
            return res.status(400).json({
                success: false,
                error: 'Tournament IDs array is required'
            });
        }

        // Get tournament data
        const tournaments = tournamentIds.map(id => {
            const tournament = tournamentRegistry.getTournament(id);
            if (!tournament) {
                return null;
            }
            return {
                id: tournament.id,
                name: tournament.name,
                description: tournament.description
            };
        }).filter(t => t !== null);

        if (tournaments.length === 0) {
            return res.status(404).json({
                success: false,
                error: 'No valid tournaments found'
            });
        }

        const baseUrl = process.env.BASE_URL || `http://localhost:${process.env.PORT || 3000}`;
        const batchResult = await qrService.generateBatchTournamentQRs(tournaments, baseUrl, options);

        if (batchResult.success) {
            res.json({
                success: true,
                message: `Batch QR codes generated successfully`,
                data: {
                    results: batchResult.results,
                    totalGenerated: batchResult.count
                }
            });
        } else {
            res.status(500).json({
                success: false,
                error: batchResult.error
            });
        }
    } catch (error) {
        console.error('? Generate batch QR error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Validate text for QR code generation
 * POST /api/qr/validate
 */
router.post('/validate', (req, res) => {
    try {
        const { qrService } = req.app.locals;
        const { text } = req.body;

        if (!text) {
            return res.status(400).json({
                success: false,
                error: 'Text is required'
            });
        }

        const validation = qrService.validateQRText(text);

        res.json({
            success: true,
            data: {
                valid: validation.valid,
                error: validation.error || null,
                isUrl: validation.isUrl || false,
                length: validation.length || 0,
                maxLength: 2953
            }
        });
    } catch (error) {
        console.error('? Validate QR text error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Get QR service statistics
 * GET /api/qr/statistics
 */
router.get('/statistics', (req, res) => {
    try {
        const { qrService } = req.app.locals;
        const stats = qrService.getStatistics();

        res.json({
            success: true,
            data: stats
        });
    } catch (error) {
        console.error('? Get QR statistics error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

module.exports = router;