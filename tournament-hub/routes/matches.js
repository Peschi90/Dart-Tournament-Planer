const express = require('express');
const router = express.Router();

/**
 * Match Routes
 * Handles all match-related API endpoints
 */

/**
 * Get tournament matches
 * GET /api/matches/:tournamentId
 */
router.get('/:tournamentId', async (req, res) => {
    try {
        const { matchService } = req.app.locals;
        const { tournamentId } = req.params;
        const { status } = req.query;

        let matches = await matchService.getTournamentMatches(tournamentId);

        // Filter by status if provided
        if (status) {
            matches = matches.filter(m => 
                m.status === status || 
                (status === 'pending' && (m.status === 'NotStarted' || (!m.status && !m.winner))) ||
                (status === 'completed' && (m.status === 'Finished' || m.winner))
            );
        }

        res.json({
            success: true,
            data: {
                tournamentId: tournamentId,
                matches: matches,
                totalMatches: matches.length
            }
        });
    } catch (error) {
        console.error('? Get tournament matches error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Submit match result
 * POST /api/matches/:tournamentId/:matchId/result
 */
router.post('/:tournamentId/:matchId/result', async (req, res) => {
    try {
        const { matchService, io } = req.app.locals;
        const { tournamentId, matchId } = req.params;
        const matchResult = req.body;

        console.log(`?? Match result submission: ${tournamentId}/${matchId}`);

        // Validate result data
        if (!matchResult || typeof matchResult !== 'object') {
            return res.status(400).json({
                success: false,
                error: 'Invalid match result data'
            });
        }

        const success = await matchService.submitMatchResult(tournamentId, matchId, matchResult);

        if (success) {
            // Broadcast to tournament room
            io.to(`tournament_${tournamentId}`).emit('match-result-updated', {
                tournamentId,
                matchId,
                result: matchResult,
                timestamp: new Date()
            });

            res.json({
                success: true,
                message: 'Match result submitted successfully',
                data: {
                    tournamentId,
                    matchId,
                    submittedAt: new Date()
                }
            });
        } else {
            res.status(400).json({
                success: false,
                error: 'Failed to submit match result'
            });
        }
    } catch (error) {
        console.error('? Submit match result error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Get pending matches for tournament
 * GET /api/matches/:tournamentId/pending
 */
router.get('/:tournamentId/pending', async (req, res) => {
    try {
        const { matchService } = req.app.locals;
        const { tournamentId } = req.params;

        const pendingMatches = await matchService.getPendingMatches(tournamentId);

        res.json({
            success: true,
            data: {
                tournamentId: tournamentId,
                matches: pendingMatches,
                totalPending: pendingMatches.length
            }
        });
    } catch (error) {
        console.error('? Get pending matches error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

/**
 * Get all pending matches across all tournaments
 * GET /api/matches/pending
 */
router.get('/pending', async (req, res) => {
    try {
        const { tournamentRegistry, matchService } = req.app.locals;

        const allPendingMatches = [];
        const tournaments = tournamentRegistry.getAllTournaments();

        for (const tournament of tournaments) {
            try {
                const pendingMatches = await matchService.getPendingMatches(tournament.id);
                allPendingMatches.push({
                    tournamentId: tournament.id,
                    tournamentName: tournament.name,
                    matches: pendingMatches
                });
            } catch (error) {
                console.warn(`?? Error getting pending matches for tournament ${tournament.id}:`, error.message);
            }
        }

        const totalPending = allPendingMatches.reduce((sum, t) => sum + t.matches.length, 0);

        res.json({
            success: true,
            data: {
                tournaments: allPendingMatches,
                totalPendingMatches: totalPending
            }
        });
    } catch (error) {
        console.error('? Get all pending matches error:', error.message);
        res.status(500).json({
            success: false,
            error: 'Internal server error'
        });
    }
});

module.exports = router;