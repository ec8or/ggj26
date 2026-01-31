# Hey Rich! 👋

Thanks for jumping in to save the day! Here's what you need to know:

---

## What We're Building

**Mask Royale** - 40-player battle royale where the audience ARE the players.

**Format**: 5-minute presentation
- 1 min intro/rules
- 2-4 min gameplay (ideally 2 rounds, minimum 1 round)
- Keep it ON RAILS - no complex choices, just pure chaos

**How it works**: Everyone scans QR code → gets assigned a mask → when your mask appears on screen, TAP YOUR PHONE → don't tap when it's not your mask → last player standing wins!

---

## Architecture (The TL;DR)

```
Mobile Phone (HTML/JS)  →  Node.js Server (relay)  →  Unity (ALL game logic)
        ↓                          ↓                         ↓
   Sends taps              Forwards messages          Decides everything
   Shows mask              No validation              Runs rounds
   Vibrates                Dumb pipe                  Eliminates players
```

**Key point**: Unity is the authority. Server just relays Socket.io messages. Client is thin - tap button + mask image.

**Events**:
- Phone → Server → Unity: `tap` events
- Unity → Server → Phone: `mask_assigned`, `eliminated`, `game_state`

---

## Quick Start

**Prerequisites**: Make sure you have Node.js installed (`node --version` should return v16+)

1. **Start Server**:
   ```bash
   cd server
   npm install    # Install dependencies (first time only)
   npm start      # Start the server
   ```
   Server will show QR code in terminal and run at `http://localhost:3000`

2. **Start Unity** (do this BEFORE connecting clients):
   - Open `unity/` folder in Unity 2022.3 LTS
   - Open `MainGame` scene
   - Press Play
   - Unity Console should show "✅ Connected to server"

3. **Test with Browser Tab** (easiest way to test):
   - Open browser to `http://localhost:3000`
   - You'll see your mask and a big TAP button
   - Unity Console should show "👤 Player joined" and "✅ Player assigned Mask #X"
   - Open more tabs to simulate multiple players

4. **Play a Round**:
   - Press SPACE in Unity to start game
   - Masks appear on Unity screen
   - Tap in browser when you see your mask
   - Don't tap when your mask isn't shown
   - Last player standing wins!

5. **Test with Real Phones** (when ready):
   - Use ngrok for network access: `ngrok http 3000`
   - Update `NetworkManager.serverUrl` in Unity Inspector with ngrok URL
   - Scan QR code or manually visit URL on phones

---

## Unity Code Setup

### Scene Structure
`Assets/Scenes/MainGame.unity` - One massive UI Canvas with a bunch of panels. Yeah, it's messy, but it works. You'll figure it out.

### Scripts Overview (`Assets/Scripts/`)

**Core Singletons** (all on GameManager GameObject):

1. **`NetworkManager.cs`**
   - SocketIOUnity connection to server
   - Events: `OnPlayerJoined`, `OnPlayerLeft`, `OnTapReceived`
   - Emits: `mask_assigned`, `game_state`, `eliminated`
   - Thread-safe event queue for main thread processing

2. **`PlayerManager.cs`**
   - Tracks up to 60 players (we're using ~40)
   - Assigns unique mask IDs (0-59)
   - `EliminatePlayer(playerId, reason)` - call this to kill someone
   - `GetAlivePlayers()` - returns List<Player>

3. **`GameManager.cs`**
   - State machine: `Lobby → Playing → GameOver`
   - Has `roundSequence[]` array defining round order
   - Press SPACE to start game
   - Calls `NextRound()` after each round completes

4. **`RoundController.cs`**
   - **Snap round** (the core game mode)
   - Shows 3-5 random masks
   - 5 second timer
   - Eliminates: wrong tap (tapped but mask not active) OR missed tap (didn't tap but mask was active)

5. **`MaskManager.cs`**
   - Loads sprites from `Resources/Masks/mask_0.png` to `mask_59.png`
   - `DisplayMasks(List<int> maskIds)` - shows masks on screen
   - `ClearMasks()` - removes them
   - Has placeholder colored circles for missing masks

6. **`UIManager.cs`**
   - Shows/hides panels
   - Updates timer, player count, etc.
   - All TextMeshPro references linked in Inspector

**Bonus Round Scripts** (`Assets/Scripts/BonusRounds/`):

7. **`SprintRound.cs`** - Tap 100 times in 10 seconds
8. **`ReactionRound.cs`** - Wait for GO signal, fastest 50% survive
9. **`PrecisionRound.cs`** - Tap at exactly 5.0 seconds
10. **`AdvancedRound.cs`** - Advanced Snap (shows MORE masks)

### Conventions
- All managers are singletons with `Instance` property
- Use events for decoupling (e.g., `NetworkManager.OnTapReceived += HandleTap`)
- Round classes have `Start[RoundType]Round()` and evaluate in their own Update loop
- Call `GameManager.Instance.NextRound()` when round completes
- Elimination reasons: `"wrong_tap"`, `"missed_tap"`, `"too_slow"`, `"failed_bonus"`

### Round Sequence
Defined in `GameManager.roundSequence[]`:
```csharp
Snap → Sprint → Reaction → [CHAOS CARD] → Advanced Snap → FINAL
```

**Chaos Card**: Right before Advanced Snap, we randomize all remaining player masks! This adds unpredictability - players get reassigned to new masks, keeping everyone on their toes.

You can edit this array in Inspector to change the flow.

---

## To-Do List

### **General** - Core Features Needed

#### Title/Instruction Screens
- **Show before each round type** (Snap, Sprint, Reaction, Advanced Snap)
- Display round name + brief instructions
- Examples:
  - "SNAP! - Tap when you see your mask"
  - "SPRINT! - Tap 100 times in 10 seconds"
  - "REACTION! - Wait for GO signal, then tap fast!"
  - "CHAOS! - Everyone gets a new mask!"
  - "ADVANCED SNAP! - More masks = harder to spot yours"
- **Press SPACE to advance** from instruction screen to actual round
- Keeps presentation on rails, gives host control of pacing

### **Nils** - Phone UI & Juice
- Mobile web client polish
- Haptic feedback improvements
- Maybe some GFX from Disa

### **Disa** - Art & Design
- **AS MANY MASKS AS POSSIBLE** (targeting 40 minimum)
- Background artwork
- UI elements (keep text/icons minimal!)
- Traffic light graphics for Reaction/Precision rounds

### **Rich** (You!) - Unity Polish & Balance

#### Layout & Presentation
- **All round types need layout grids** (Snap, Advanced, Precision, Sprint, Reaction)
- Make masks display **as large as possible** on screen
- Grid arrangements that scale with mask count (3-5 for Snap, more for Advanced)

#### Animations
- Small bobbing animation on masks (idle bounce)
- Maybe scale-in when masks appear
- Maybe pulse/glow on active masks

#### Scene Organization
- **Clean up MainGame scene** - it's a bit chaotic right now
- Organize UI elements into logical holders/containers
- No need to split into multiple scenes, just better hierarchy
- Group related panels together (e.g., all round-specific UI under holders)
- Makes it easier to find things and toggle visibility

#### New Feature: Chaos Card
- **Right before Advanced Snap**: Randomize all remaining player masks
- Show big "CHAOS!" animation/text on screen
- Emit new mask assignments to all players
- Give players 3-5 seconds to see their new mask before Advanced Snap begins
- This keeps players on their toes and adds unpredictability

#### Minor Fixes
- **Precision Round needs traffic lights** (Red → Yellow → Green) so players know when to start counting to 5 seconds
- Reaction Round already has WAIT → GO logic, but could use visual polish

#### Balance (Critical!)
**Target**: 40 players → 30 → 15 → 8 → 2 → FINAL

**⚠️ NOTE: These numbers are NOT final! Adjust based on playtesting.**

Work backwards:
- Round 1 (Snap): Eliminate ~10 players (40→30)
- Round 2 (Sprint): Eliminate ~15 players (30→15)
- Round 3 (Reaction): Eliminate ~7 players (15→8)
- **[CHAOS CARD]** - Randomize all masks!
- Round 4 (Advanced Snap): Eliminate ~6 players (8→2)
- FINAL: Top 2 face off (or public vote? TBD)

Tune these values:
- `RoundController.minMasks / maxMasks` - how many shown (3-5 for Snap, more for Advanced)
- `RoundController.roundDuration` - time pressure (currently 5s)
- `SprintRound.requiredTaps` - currently 100, maybe too hard?
- `ReactionRound` - currently top 50% survive
- `AdvancedRound` - shows MORE masks (harder to spot yours)

**Goal**: Predictable eliminations. Each round should cut down players smoothly to create drama.

#### Reset System
- If we have time for 2 rounds in the presentation, add a "Reset Game" function
- Clear all players, return to Lobby, ready for round 2

---

## Files You'll Probably Touch

- `GameManager.cs` - round sequence, balance tweaks
- `RoundController.cs` - main round logic and timing
- `MaskManager.cs` - layout/grid arrangement
- `SprintRound.cs`, `ReactionRound.cs`, `PrecisionRound.cs` - bonus round balance
- `UIManager.cs` - if you add traffic lights or other UI
- Unity Scene - positioning, animations, visual polish

---

## Documentation

- `/unity/STATUS.md` - What's done, what remains
- `/unity/CLAUDE.md` - Unity context and architecture
- `/server/STATUS.md` - Server status
- `/DESIGNER_BRIEF.md` - Mask categories and specs

---

## Notes

- We have 10 real masks so far (mask_0 to mask_9), targeting 40 total
- Placeholder colored circles fill in the gaps
- Server is at `http://localhost:3000` (or ngrok URL for phones)
- Everything is Socket.io WebSockets
- Mobile client is in `server/public/` (HTML/CSS/JS)

---

You got this! If anything's unclear, the code is pretty well commented. The scene is a bit chaotic but all the scripts are clean.

Let's make this thing SHINE! ✨

— Nils & Claude
