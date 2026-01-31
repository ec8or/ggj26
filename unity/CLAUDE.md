# Unity - Game Host & Logic

## Your Role

You are implementing the **game logic and display** for a 50-60 player game jam game. ALL game logic lives in Unity - the server only relays messages.

## Scope

**IN SCOPE**:
- Connect to Socket.io server (via SocketIOUnity package)
- Track 60 players (alive/dead, mask IDs)
- Run Snap rounds (show masks, evaluate taps, eliminate)
- Run bonus rounds (sprint, reaction, precision)
- Display UI (QR code, kill feed, timers, player count)
- Load and display 60 mask sprites

**OUT OF SCOPE**:
- Server-side logic (that's in Node.js)
- Mobile client UI (that's in HTML/CSS/JS)
- Authentication/accounts
- Audio/music (no time for this)

## Files to Create

1. `/Assets/Scripts/NetworkManager.cs` - Socket.io connection
2. `/Assets/Scripts/GameManager.cs` - State machine (Lobby → Playing → GameOver)
3. `/Assets/Scripts/PlayerManager.cs` - Track 60 players
4. `/Assets/Scripts/RoundController.cs` - Snap round logic
5. `/Assets/Scripts/MaskManager.cs` - Load/display masks
6. `/Assets/Scripts/UIManager.cs` - QR, kill feed, timers
7. `/Assets/Scripts/BonusRounds/SprintRound.cs` - Tap counter bonus
8. `/Assets/Scripts/BonusRounds/ReactionRound.cs` - GO signal bonus
9. `/Assets/Scripts/BonusRounds/PrecisionRound.cs` - Timing bonus

## Critical Requirements

### Unity Settings:
- Resolution: 1920x1080 (Full HD)
- Target: 60 FPS
- Build: Standalone (Windows/Mac)

### Mask Sprites:
- 60 unique PNGs at 512x512px
- Naming: `mask_001.png` to `mask_060.png`
- Location: `/Assets/Resources/Masks/`
- Load with: `Resources.LoadAll<Sprite>("Masks")`

### Game Logic:
- Snap round: Show 3-5 random masks, eliminate wrong/missing taps
- Sprint round: Tap 100+ times in 10 seconds
- Reaction round: Fastest 50% after GO signal survive
- Precision round: Closest to 5.0 seconds survive

## Testing

1. Start server (coordinate with server agent)
2. Press Play in Unity
3. Verify "Connected to server" in Console
4. Connect 5 phones via QR code
5. Press SPACE to start game
6. Verify eliminations work correctly

## Communication Protocol

### Events to Send to Server
```csharp
// Identify as Unity host
{ type = "unity" }

// Assign mask to player
{ type = "mask_assigned", playerId, maskId }

// Send game state
{ type = "game_state", phase, playersAlive, totalPlayers, roundNumber }

// Send elimination
{ type = "eliminated", playerId, reason, maskId }
```

### Events to Receive from Server
```csharp
// Player joined
{ playerId, socketId }

// Player left
{ playerId }

// Tap received
{ playerId, timestamp }
```

## Timeline

- Hours 0-6: Connection + player tracking
- Hours 6-18: Core game loop (Snap rounds)
- Hours 18-24: Bonus rounds
- Ongoing: Polish (tweens, kill feed, masks)

## Dependencies

- Unity 2022.3 LTS or newer
- SocketIOUnity: `https://github.com/itisnajim/SocketIOUnity.git`
- TextMeshPro (built-in)
