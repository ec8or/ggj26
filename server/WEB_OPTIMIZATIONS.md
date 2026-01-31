# Mobile Client Optimizations (TODO)

**Status**: Notes added to code, not yet implemented
**Priority**: Medium (nice-to-have for polish)
**Files**: `server/public/app.js`, `server/public/style.css`

---

## 1. Tap Throttling for Sprint Round (Bandwidth Optimization)

**Problem:**
- Sprint round: 100 taps per player
- 40 players = 4,000 tap messages in 30 seconds
- Causes network congestion

**Solution:**
Broadcast only every 5th tap:
```javascript
let tapCount = 0;

tapBtn.addEventListener('click', () => {
  tapCount++;

  // Only send every 5 taps (or first tap)
  if (tapCount % 5 === 0 || tapCount === 1) {
    socket.emit('tap', { playerId, timestamp: Date.now() });
  }

  // ... rest of tap handling
});
```

**Result:**
- Reduces messages from 4,000 to 800 (80% reduction!)
- Unity can interpolate between updates
- Still feels responsive

---

## 2. Sprint Round Victory UI

**Feature:**
Show "YOU WIN!" message when player reaches 100 taps in Sprint round.

**Requirements:**
1. Track current round type from `game_state` events
2. Track local tap count
3. At 100 taps during Sprint round:
   - Hide tap button
   - Show victory overlay
   - Optional: Confetti/celebration animation

**Implementation:**
```javascript
let currentRoundType = 'main'; // Track from game_state

socket.on('game_state', (data) => {
  // Detect Sprint round (needs Unity to send roundType)
  if (data.phase === 'bonusround') {
    currentRoundType = 'sprint'; // Or parse from message
  }
});

tapBtn.addEventListener('click', () => {
  tapCount++;

  // Check for Sprint victory
  if (currentRoundType === 'sprint' && tapCount >= 100) {
    tapBtn.classList.add('hidden');
    showVictoryMessage(); // Create this function
  }
});

function showVictoryMessage() {
  // Create overlay div
  // Add "YOU WIN!" text
  // Add celebration effects
}
```

**UI Suggestions:**
- Green overlay with "🏆 YOU WIN! 🏆"
- Show final tap count: "100/100 taps!"
- Confetti animation (can use CSS or canvas)
- Keep visible until round ends

---

## 3. Optional: Sprint Progress Display

**Feature:**
Show tap counter during Sprint round: "42/100 taps"

**Location:**
- Replace connection status during Sprint
- Or add below mask image
- Large, visible font

**Benefits:**
- Player knows their progress
- Reduces anxiety ("Am I tapping enough?")
- Creates urgency as time runs out

---

## Implementation Order:

1. ✅ Add TODO comments to code (DONE)
2. ⏸️ Test bandwidth impact during playtesting
3. ⏸️ If network lag occurs, implement tap throttling first
4. ⏸️ Add victory UI for polish
5. ⏸️ Add progress counter if time permits

---

## Testing Notes:

- Test with 30+ players tapping simultaneously
- Monitor network tab in browser dev tools
- Check if server logs show message backlog
- Unity should handle missed updates gracefully

---

**Do NOT implement yet** - game works fine as-is. These are polish features for post-jam or if bandwidth becomes an issue during testing.
