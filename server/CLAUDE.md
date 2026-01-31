# Server - Relay Server for Crowd Control

## Your Role

You are implementing the **relay server** for a 50-60 player game jam game. Your ONLY job is to forward messages between mobile clients and Unity. **DO NOT implement game logic**.

## Scope

**IN SCOPE**:
- Socket.io relay server (forward tap events)
- Serve mobile client HTML/CSS/JS
- Generate QR code for mobile joining
- Track connected clients
- Forward game state updates to mobile

**OUT OF SCOPE**:
- Game logic (that's in Unity)
- Player elimination logic
- Round management
- Score tracking
- Authentication/accounts

## Files to Create

1. `/server/server.js` - Main relay server
2. `/server/package.json` - Dependencies (express, socket.io, qrcode)
3. `/server/public/index.html` - Mobile controller UI
4. `/server/public/style.css` - Anti-zoom, anti-refresh CSS
5. `/server/public/app.js` - Socket.io client

## Critical Requirements

### Mobile Client Must Have:
- Large tap button (60-80% of screen)
- Anti-zoom CSS: `user-scalable=no`
- Anti-refresh: `beforeunload` event
- Haptic feedback: `navigator.vibrate(50)` on tap
- Connection status indicator
- Eliminated screen (red overlay)

### Server Must:
- Generate QR code with LOCAL IP (not localhost)
- Forward ALL events (don't filter or modify)
- Handle 60 concurrent connections
- Log all events for debugging

## Testing

1. Start server: `npm start`
2. Scan QR code with 3+ phones
3. Tap button on phone
4. Verify server logs show tap events
5. Verify Unity receives events (coordinate with Unity agent)

## Communication Protocol

### Events Received from Mobile
```javascript
// Player identification
{ type: 'player', playerId: 'abc123' }

// Tap event
{ playerId: 'abc123', timestamp: 1234567890 }
```

### Events Received from Unity
```javascript
// Mask assignment
{ type: 'mask_assigned', playerId: 'abc123', maskId: 42, maskUrl: '...' }

// Game state
{ type: 'game_state', phase: 'lobby', playersAlive: 45, totalPlayers: 58 }

// Elimination
{ type: 'eliminated', playerId: 'abc123', reason: 'wrong_tap' }
```

## Timeline

- Hours 0-4: Basic relay server
- Hours 4-8: Mobile client UI
- Hours 8-12: Polish and optimization

## Dependencies

```bash
cd /Users/nils/Dev/GGJ26/server
npm install
```

Dependencies: express (^4.18.2), socket.io (^4.6.0), qrcode (^1.5.3)
