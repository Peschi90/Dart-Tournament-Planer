// Tournament Hub - Web Interface Debug Script
// Fügen Sie diesen Code in die Browser-Konsole ein, um Class-ID Debugging zu aktivieren

console.log('?? Tournament Hub Class-ID Debug aktiviert');

// Override der submitResult Funktion mit erweiterten Logs
const originalSubmitResult = window.submitResult;
window.submitResult = function(matchId) {
    console.log(`?? [DEBUG] ===== ENHANCED SUBMIT RESULT DEBUG =====`);
    console.log(`?? [DEBUG] Match ID: ${matchId}`);
    
    // Find the match
    const match = matches.find(m => (m.matchId || m.id) == matchId);
    if (!match) {
        console.error(`? [DEBUG] Match ${matchId} not found`);
        return;
    }
    
    console.log(`?? [DEBUG] Original match data:`, {
        matchId: match.matchId || match.id,
        classId: match.classId || match.ClassId,
        className: match.className || match.ClassName,
        player1: match.player1 || match.Player1,
        player2: match.player2 || match.Player2,
        matchType: match.matchType || match.MatchType
    });
    
    // Class extraction logic
    const classId = match.classId || match.ClassId || 1;
    const className = match.className || match.ClassName || 
                    tournamentClasses.find(c => c.id == classId)?.name || 
                    `Klasse ${classId}`;
    
    console.log(`?? [DEBUG] Class extraction result:`, {
        extractedClassId: classId,
        extractedClassName: className,
        isCorrect: className !== 'Platin' || classId !== 1,
        expectedForSilber: className === 'Silber' && classId === 3
    });
    
    // Form values
    const p1Sets = parseInt(document.getElementById(`p1Sets_${matchId}`)?.value) || 0;
    const p2Sets = parseInt(document.getElementById(`p2Sets_${matchId}`)?.value) || 0;
    const p1Legs = parseInt(document.getElementById(`p1Legs_${matchId}`)?.value) || 0;
    const p2Legs = parseInt(document.getElementById(`p2Legs_${matchId}`)?.value) || 0;
    const notes = document.getElementById(`notes_${matchId}`)?.value?.trim() || '';
    
    console.log(`?? [DEBUG] Form values:`, {
        p1Sets, p2Sets, p1Legs, p2Legs, notes
    });
    
    const result = {
        matchId: matchId,
        player1Sets: p1Sets,
        player1Legs: p1Legs,
        player2Sets: p2Sets,
        player2Legs: p2Legs,
        notes: notes,
        status: 'Finished',
        submittedAt: new Date().toISOString(),
        classId: classId,
        className: className
    };
    
    const socketMessage = {
        tournamentId: tournamentId,
        matchId: matchId,
        result: result,
        classId: classId,
        className: className
    };
    
    console.log(`?? [DEBUG] Socket message to be sent:`, JSON.stringify(socketMessage, null, 2));
    console.log(`?? [DEBUG] Class verification:`, {
        messageClassId: socketMessage.classId,
        messageClassName: socketMessage.className,
        resultClassId: socketMessage.result.classId,
        resultClassName: socketMessage.result.className,
        allMatch: socketMessage.classId === socketMessage.result.classId &&
                  socketMessage.className === socketMessage.result.className
    });
    
    // Call original function
    originalSubmitResult(matchId);
    
    console.log(`?? [DEBUG] ===== SUBMIT RESULT DEBUG COMPLETE =====`);
};

// WebSocket Message Debugging
const originalEmit = socket?.emit;
if (socket && originalEmit) {
    socket.emit = function(event, data) {
        if (event === 'submit-match-result') {
            console.log(`?? [DEBUG] WebSocket EMIT - submit-match-result:`, {
                event: event,
                tournamentId: data.tournamentId,
                matchId: data.matchId,
                classId: data.classId,
                className: data.className,
                resultClassId: data.result?.classId,
                resultClassName: data.result?.className
            });
        }
        return originalEmit.call(this, event, data);
    };
}

// Enhanced notification for Class-ID verification
const originalShowNotification = window.showNotification;
window.showNotification = function(message, type) {
    console.log(`?? [DEBUG] Notification: ${type.toUpperCase()} - ${message}`);
    originalShowNotification(message, type);
};

console.log('? Class-ID Debug aktiviert! Testen Sie jetzt ein Silber-Match.');
console.log('?? Erwartung: Silber-Match sollte als Silber (ID: 3) übertragen werden');
console.log('? Problem: Wenn Platin (ID: 1) erscheint, ist Class-ID verloren gegangen');

// Debug Info anzeigen
console.log('?? Debug Info:', {
    tournamentId: tournamentId,
    currentMatches: matches?.length || 0,
    tournamentClasses: tournamentClasses?.length || 0,
    socketConnected: socket?.connected || false
});