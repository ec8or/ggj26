# WebSocket Messaging Patterns

**Component**: Server Communication Architecture
**Last Updated**: 2026-01-30

---

## Overview

The server supports **two messaging patterns**:

1. **Broadcast to All** - Send to everyone (game state updates)
2. **Targeted to One** - Send to specific player (elimination, mask assignment)

---

## Pattern 1: Broadcast to All Players

### When to Use:
- Game state changes everyone needs to know
- Round start announcements
- Player count updates
- Phase transitions

### Server Code:
```javascript
// Unity sends to server
socket.on('game_state', (data) => {
  io.emit('game_state', data);  // ← Broadcasts to ALL connected clients
});
```

### Unity Code:
```csharp
// Send to everyone
NetworkManager.Instance.EmitGameState(
    "playing",           // phase
    23,                  // playersAlive
    45,                  // totalPlayers
    5,                   // roundNumber
    "sprint"             // roundType
);
```

### Mobile Receives:
```javascript
// ALL phones receive this
socket.on('game_state', (data) => {
  console.log('Game state:', data);
  // data = { phase: 'playing', playersAlive: 23, totalPlayers: 45, roundNumber: 5, roundType: 'sprint' }
});
```

### Current Broadcast Events:
- `game_state` - Game phase and player counts

---

## Pattern 2: Targeted to Specific Player

### When to Use:
- Player eliminated (only that player needs to know)
- Mask assignment (each player gets their own mask)
- Personal notifications
- Private feedback

### Server Code:
```javascript
// Unity sends elimination for specific player
socket.on('eliminated', (data) => {
  // Find the socket ID for this player ID
  const targetSocket = Array.from(players.entries())
    .find(([sid, pid]) => pid === data.playerId);

  if (targetSocket) {
    // Send ONLY to that specific player's socket
    io.to(targetSocket[0]).emit('eliminated', data);  // ← Targeted send
  }
});
```

### How Targeting Works:

```
Players Map (server tracks this):
┌─────────────────────────────────┐
│ Socket ID      → Player ID      │
├─────────────────────────────────┤
│ "abc123xyz"    → "player-001"   │
│ "def456uvw"    → "player-002"   │
│ "ghi789rst"    → "player-003"   │
└─────────────────────────────────┘

Unity says: "Eliminate player-002"
Server looks up: socket ID "def456uvw"
Server sends ONLY to: socket "def456uvw"
```

### Unity Code:
```csharp
// Eliminate specific player
PlayerManager.Instance.EliminatePlayer(
    "player-abc123",    // playerId
    "wrong_tap"         // reason
);

// This calls NetworkManager:
NetworkManager.Instance.EmitEliminated(
    playerId,           // "player-abc123"
    reason,             // "wrong_tap"
    maskId,             // 42
    playersRemaining    // 22
);
```

### Mobile Receives (Only Eliminated Player):
```javascript
// ONLY the eliminated player receives this
socket.on('eliminated', (data) => {
  isEliminated = true;
  tapBtn.classList.add('hidden');
  eliminatedReason.textContent = getReason(data.reason);
  eliminatedEl.classList.remove('hidden');

  // Triple vibrate
  if (navigator.vibrate) {
    navigator.vibrate([200, 100, 200]);
  }
});
```

### Current Targeted Events:
- `eliminated` - Send to eliminated player only
- `mask_assigned` - Send to newly joined player only

---

## Comparison Table

| Event Type | Send Method | Recipients | Example |
|------------|-------------|------------|---------|
| **Broadcast** | `io.emit()` | ALL players | Game state update |
| **Targeted** | `io.to(socketId).emit()` | ONE player | Elimination notice |

---

## Implementation Details

### Server Tracking:

The server maintains a map of socket IDs to player IDs:

```javascript
let players = new Map(); // socket.id → playerId

// When player connects
socket.on('identify', (data) => {
  if (data.type === 'player') {
    players.set(socket.id, data.playerId);  // Store mapping
  }
});

// When player disconnects
socket.on('disconnect', () => {
  if (players.has(socket.id)) {
    players.delete(socket.id);  // Remove mapping
  }
});
```

### Finding Target Socket:

```javascript
// Unity sends playerId, server needs socketId
const targetSocket = Array.from(players.entries())
  .find(([socketId, playerId]) => playerId === data.playerId);

if (targetSocket) {
  const socketId = targetSocket[0];  // Extract socket ID
  io.to(socketId).emit('eliminated', data);  // Send to that socket
}
```

---

## Message Flow Examples

### Example 1: Player Elimination

```
Unity                Server                 Mobile (Player #23)      All Other Mobiles
  |                     |                           |                        |
  |--eliminated-------->|                           |                        |
  |  playerId: "p23"    |                           |                        |
  |  reason: "wrong"    |                           |                        |
  |                     |---eliminated------------->|                        |
  |                     |  (ONLY to p23's socket)   |                        |
  |                     |                           |                        |
  |                     |                           |--Shows "ELIMINATED"    |
  |                     |                           |--Vibrates phone        |
  |                     |                           |                        |
  |                     |                    (Other phones unchanged)        |
```

### Example 2: Round Start (Broadcast)

```
Unity                Server                 All Mobiles
  |                     |                        |
  |--game_state-------->|                        |
  |  phase: "playing"   |                        |
  |  round: 5           |                        |
  |                     |---game_state---------->|
  |                     |  (to EVERYONE)         |
  |                     |                        |
  |                     |                 (All phones update UI)
```

### Example 3: Mask Assignment

```
Unity                Server                 Mobile (New Player)      Existing Players
  |                     |                           |                        |
  |<-player_joined------|<-identify-----------------|                        |
  |                     |                           |                        |
  |--mask_assigned----->|                           |                        |
  |  playerId: "new"    |                           |                        |
  |  maskId: 42         |                           |                        |
  |                     |---mask_assigned---------->|                        |
  |                     |  (ONLY to new player)     |                        |
  |                     |                           |                        |
  |                     |                           |--Shows "Mask #42"      |
  |                     |                    (Others don't get this message) |
```

---

## Adding New Message Types

### To Add Broadcast Message:

**Server (server.js)**:
```javascript
socket.on('new_broadcast_event', (data) => {
  io.emit('new_broadcast_event', data);  // Send to everyone
});
```

**Unity**:
```csharp
socket.Emit("new_broadcast_event", new { /* data */ });
```

**Mobile**:
```javascript
socket.on('new_broadcast_event', (data) => {
  // All phones receive and handle
});
```

---

### To Add Targeted Message:

**Server (server.js)**:
```javascript
socket.on('new_targeted_event', (data) => {
  const targetSocket = Array.from(players.entries())
    .find(([sid, pid]) => pid === data.playerId);

  if (targetSocket) {
    io.to(targetSocket[0]).emit('new_targeted_event', data);  // Send to one
  }
});
```

**Unity**:
```csharp
socket.Emit("new_targeted_event", new { playerId = "abc123", /* data */ });
```

**Mobile**:
```javascript
socket.on('new_targeted_event', (data) => {
  // Only the targeted phone receives this
});
```

---

## Performance Considerations

### Broadcast Messages:
- **Cost**: O(n) where n = number of connected clients
- **Bandwidth**: Message sent n times
- **Use For**: Infrequent updates (game state, round start)
- **Avoid For**: High-frequency updates (every tap)

### Targeted Messages:
- **Cost**: O(1) lookup + 1 send
- **Bandwidth**: Message sent once
- **Use For**: Player-specific events (elimination, rewards)
- **Avoid For**: Events everyone needs to know

---

## Common Patterns

### Pattern: "Everyone Except One"

**Use Case**: Announce elimination to ALL players for kill feed

```javascript
socket.on('player_eliminated', (data) => {
  // Send to eliminated player (targeted)
  const victimSocket = findSocket(data.playerId);
  if (victimSocket) {
    io.to(victimSocket).emit('eliminated', { reason: data.reason });
  }

  // Send to everyone else (broadcast)
  io.emit('kill_feed', {
    playerId: data.playerId,
    maskId: data.maskId,
    reason: data.reason
  });
});
```

**Mobile Handling**:
```javascript
// Eliminated player
socket.on('eliminated', (data) => {
  showEliminatedScreen();
});

// All players (including eliminated)
socket.on('kill_feed', (data) => {
  if (!isEliminated) {
    addKillFeedEntry(data.maskId, data.reason);
  }
});
```

---

### Pattern: "Broadcast with Personal Context"

**Use Case**: Show round results with personal placement

```javascript
// Unity sends targeted placement + broadcast results
socket.on('round_end', (data) => {
  // Send personal placement to each player
  data.placements.forEach(placement => {
    const playerSocket = findSocket(placement.playerId);
    if (playerSocket) {
      io.to(playerSocket).emit('your_placement', {
        rank: placement.rank,
        score: placement.score
      });
    }
  });

  // Send full leaderboard to everyone
  io.emit('leaderboard', {
    placements: data.placements
  });
});
```

---

## Debugging Tips

### Check if Message Reaches Server:
```javascript
// In server.js, add logging
socket.on('any_event', (data) => {
  console.log('Received:', data);
  // ... handle event
});
```

### Check if Message Sent to Client:
```javascript
// In server.js
io.to(socketId).emit('event', data);
console.log(`Sent 'event' to ${socketId}:`, data);
```

### Check if Mobile Receives:
```javascript
// In mobile app.js
socket.on('event', (data) => {
  console.log('Received event:', data);
});
```

### Common Issues:

**Problem**: Targeted message not received
- Check `players` map has correct socket ID
- Verify player ID matches exactly
- Check socket is still connected

**Problem**: Broadcast message not received
- Check socket connection active
- Verify event name matches exactly (case-sensitive)
- Check Unity socket connected (`IsConnected`)

---

## Security Note

**Server is a Dumb Relay** - It does NOT validate:
- ❌ Player eligibility (Unity decides who can be eliminated)
- ❌ Event authenticity (Unity is trusted source)
- ❌ Data correctness (Unity provides correct player IDs)

The server trusts Unity completely. All game logic and validation happens in Unity.

---

## Future Enhancements

### Room-Based Targeting:
For multiple simultaneous games:

```javascript
// Assign players to rooms
socket.join(`game_${gameId}`);

// Broadcast to room only
io.to(`game_${gameId}`).emit('event', data);

// Targeted within room
io.to(socketId).emit('event', data);
```

### Batch Targeted Messages:
For eliminating multiple players at once:

```javascript
socket.on('batch_eliminate', (data) => {
  data.playerIds.forEach(playerId => {
    const targetSocket = findSocket(playerId);
    if (targetSocket) {
      io.to(targetSocket).emit('eliminated', {
        playerId,
        reason: data.reason
      });
    }
  });
});
```

---

## Summary

| Scenario | Pattern | Code |
|----------|---------|------|
| Game starts | Broadcast | `io.emit('game_state', {...})` |
| Player eliminated | Targeted | `io.to(socketId).emit('eliminated', {...})` |
| Round countdown | Broadcast | `io.emit('round_timer', {...})` |
| Mask assigned | Targeted | `io.to(socketId).emit('mask_assigned', {...})` |
| Winner declared | Broadcast | `io.emit('winner', {...})` |
| Personal reward | Targeted | `io.to(socketId).emit('reward', {...})` |

**Golden Rule**:
- Everyone needs to know? → **Broadcast** (`io.emit`)
- One person needs to know? → **Targeted** (`io.to(socketId).emit`)

---

**Last Updated**: 2026-01-30
**Current Implementation**: Both patterns fully functional
