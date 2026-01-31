# Unity Status Report

**Component**: Unity Game Host (All Game Logic)
**Last Updated**: 2026-01-30
**Status**: ⚠️ **CODE COMPLETE - SCENE SETUP REQUIRED**

> 💡 **Note**: See `GAME_MODES_DESIGN.md` for extended game mode ideas (Quick Draw, Tug-o-War, etc.) - NOT yet implemented, design phase only.

---

## ✅ What Has Been Done

### C# Scripts Implementation (100% Complete)

#### Core Systems:
1. ✅ `Assets/Scripts/NetworkManager.cs` (145 lines)
   - SocketIOUnity integration
   - Connection to relay server
   - Event subscription system (OnPlayerJoined, OnPlayerLeft, OnTapReceived)
   - Message parsing with error handling
   - Event emission (game_state, eliminated, mask_assigned)
   - Singleton pattern

2. ✅ `Assets/Scripts/PlayerManager.cs` (120 lines)
   - Tracks up to 60 players
   - Unique mask ID assignment (1-60)
   - Player alive/dead state management
   - Elimination logic with reason tracking
   - Tap count tracking for bonus rounds
   - Player lookup by ID or mask ID
   - Singleton pattern

3. ✅ `Assets/Scripts/GameManager.cs` (130 lines)
   - Game state machine (Lobby → Playing → BonusRound → GameOver)
   - Round progression logic
   - SPACE key to start game
   - Bonus round triggering (every 5 rounds)
   - Winner detection (last player standing)
   - Game state broadcasting
   - Round completion handling
   - Debug key (N) to skip rounds

4. ✅ `Assets/Scripts/RoundController.cs` (110 lines)
   - Main round logic
   - Random mask selection (3-5 per round)
   - Tap tracking during round
   - Timer countdown (5 seconds default)
   - Elimination evaluation (wrong tap, missed tap)
   - Round completion notification

5. ✅ `Assets/Scripts/MaskManager.cs` (140 lines)
   - Mask sprite loading system
   - Placeholder sprite generation (60 colored circles)
   - Mask display on screen
   - Horizontal layout arrangement
   - Mask clearing between rounds
   - Support for loading real sprites from Resources/Masks/
   - Singleton pattern

6. ✅ `Assets/Scripts/UIManager.cs` (130 lines)
   - QR code panel (lobby)
   - Player count display (alive/total)
   - Round number display
   - Round timer display
   - Bonus round title panel
   - Winner/Game Over screen
   - Panel show/hide logic
   - Singleton pattern

#### Bonus Round Systems:
7. ✅ `Assets/Scripts/BonusRounds/SprintRound.cs` (90 lines)
   - Tap counter bonus round
   - 100 taps in 10 seconds challenge
   - Per-player tap tracking
   - Elimination of players under threshold
   - Timer countdown

8. ✅ `Assets/Scripts/BonusRounds/ReactionRound.cs` (130 lines)
   - Reaction time challenge
   - WAIT → GO signal sequence
   - Random delay (2-5 seconds)
   - Fastest 50% survive
   - Early tap elimination
   - Reaction time measurement in milliseconds

9. ✅ `Assets/Scripts/BonusRounds/PrecisionRound.cs` (115 lines)
   - Timing precision challenge
   - Tap at exactly 5.0 seconds
   - Deviation calculation
   - Closest 50% survive
   - First tap only (no retries)

### Documentation:
- ✅ `CLAUDE.md` - Unity-specific context
- ✅ `GAME_MODES_DESIGN.md` - Extended game modes design (v2.0 ideas)
- ✅ `DEBUG_CONTROLS_PLAN.md` - Debug tools for testing without full playtest
- ✅ `QR_CODE_IN_UNITY.md` - Generate QR code in Unity (auto-matches server URL)
- ✅ `Packages/manifest.json` - Package dependencies

### Folder Structure:
```
✅ Assets/
    ✅ Scripts/           (all 9 scripts created)
    ✅ Scripts/BonusRounds/  (3 bonus scripts)
    ✅ Resources/Masks/   (folder exists, awaiting sprites)
    ✅ Prefabs/          (folder exists, needs MaskDisplay prefab)
    ⚠️  Scenes/           (folder exists, needs MainGame.unity scene)
```

---

## ⏳ What Remains

### Critical - Manual Unity Editor Setup Required:

Unity scenes, GameObjects, and Inspector references **cannot be created programmatically**. The following must be done manually in Unity Editor:

#### 1. Project Setup (5 min):
- [ ] Open project in Unity Hub
- [ ] Install SocketIOUnity package via Package Manager
  - URL: `https://github.com/itisnajim/SocketIOUnity.git`
- [ ] Import TextMeshPro essentials
- [ ] Verify all scripts compile without errors

#### 2. Scene Creation (10 min):
- [ ] Create new scene: `Assets/Scenes/MainGame.unity`
- [ ] Set up Camera for 1920x1080 resolution
- [ ] Create GameManager GameObject
- [ ] Add all 9 scripts to GameManager
- [ ] Configure NetworkManager server URL

#### 3. UI Canvas Setup (20 min):
- [ ] Create UI Canvas (Scale With Screen Size: 1920x1080)
- [ ] Create QRCodePanel with InstructionsText
- [ ] Create HUD elements (PlayerCountText, RoundNumberText, RoundTimerText)
- [ ] Create MaskDisplayContainer (empty GameObject)
- [ ] Create BonusRoundPanel with BonusRoundTitleText
- [ ] Create WinnerPanel with WinnerText

#### 4. Prefab Creation (5 min):
- [ ] Create MaskDisplay prefab (UI Image, 400x400)
- [ ] Save to Assets/Prefabs/
- [ ] Remove from scene

#### 5. Inspector Linking (10 min):
- [ ] Link MaskManager → Mask Display Container
- [ ] Link MaskManager → Mask Prefab
- [ ] Link UIManager → All UI elements (9 fields)
- [ ] Verify no "None (Script)" or "Missing" references

**Total Estimated Time**: 45-60 minutes

---

## 🚀 Next Steps

### Step 1: Open Unity Project
1. Launch Unity Hub
2. Click "Open" or "Add"
3. Navigate to: `/Users/nils/Dev/GGJ26/unity`
4. Wait for import (may take 2-5 minutes first time)

### Step 2: Install SocketIOUnity Package
1. Window → Package Manager
2. Click "+" (top-left)
3. Select "Add package from git URL"
4. Enter: `https://github.com/itisnajim/SocketIOUnity.git`
5. Click "Add"
6. Wait for installation

**Verify**: Package Manager shows "SocketIOUnity" in list

### Step 3: Import TextMeshPro
1. Window → TextMeshPro → Import TMP Essential Resources
2. Click "Import"

### Step 4: Check Script Compilation
1. Window → General → Console
2. Look for errors (red messages)
3. Scripts should compile without errors

**If "SocketIOUnity not found"**:
- Verify package installed (Step 2)
- Try: Assets → Reimport All
- Restart Unity

### Step 5: Follow Detailed Setup Checklist
Open and follow: `/Users/nils/Dev/GGJ26/UNITY_SETUP_CHECKLIST.md`

This file has step-by-step instructions with checkboxes for:
- Scene creation
- GameObject setup
- UI element creation
- Inspector linking

### Step 6: Configure Server Connection
1. Select GameManager in Hierarchy
2. Find NetworkManager component
3. Set "Server Url" field:
   - Testing locally: `http://localhost:3000`
   - On network: `http://[SERVER_IP]:3000`

**Get server IP from**: Server terminal when you run `npm start`

### Step 7: First Test
1. Ensure server is running
2. Press Play in Unity
3. Check Console for:
   - "🔌 Connecting to server..."
   - "✅ Connected to server"

### Step 8: Test with Mobile Phone
1. Connect 1 phone to server via QR code
2. Unity Console should show:
   - "👤 Player joined: [id]"
   - "✅ Player [id] assigned Mask #1"
3. Tap button on phone
4. Unity Console should show:
   - "👆 Tap from [id]"

### Step 9: Test Gameplay
1. Connect 3-5 phones
2. Press SPACE in Unity
3. QR code panel should hide
4. Masks should appear on screen
5. Players tap when they see their mask
6. Check eliminations work correctly

---

## 🐛 Troubleshooting

### Problem: Scripts won't compile
**Solutions**:
- Install SocketIOUnity package (Step 2)
- Install TextMeshPro (Step 3)
- Check Unity version (need 2022.3 LTS or newer)
- Window → Package Manager → Refresh

### Problem: "SocketIOUnity not found"
**Solutions**:
- Verify in Package Manager list
- Try: Edit → Preferences → External Tools → Regenerate project files
- Restart Unity
- Manual install: Clone repo to Assets/Plugins/

### Problem: "Newtonsoft.Json not found"
**Solution**:
- SocketIOUnity includes this automatically
- If missing: Window → Package Manager → Add "com.unity.nuget.newtonsoft-json"

### Problem: Cannot connect to server
**Solutions**:
- Verify server running: `cd ../server && npm start`
- Check NetworkManager "Server Url" matches server IP
- Check firewall settings
- Try `http://localhost:3000` first

### Problem: No tap events received
**Solutions**:
- Verify Console shows "✅ Connected to server"
- Check server logs show taps
- Verify phone shows "Connected!" (green)
- Check NetworkManager event subscription in Start()

### Problem: NullReferenceException in UI
**Solutions**:
- Check all UIManager Inspector fields are linked
- Verify TextMeshPro objects exist in scene
- Check MaskManager has Container and Prefab linked
- Look for "None (Game Object)" in Inspector

### Problem: Masks don't appear
**Solutions**:
- Check MaskManager has "Mask Display Container" assigned
- Check MaskManager has "Mask Prefab" assigned
- Verify "Use Placeholder Sprites" is checked
- Check Console for sprite creation logs

### Problem: GameManager not starting game
**Solutions**:
- Check GameManager has all 9 scripts attached
- Verify no missing references
- Check Console for errors
- Try pressing SPACE in Play mode

---

## 🎯 Testing Checklist

### Basic Connection Test:
- [ ] Unity connects to server
- [ ] 1 phone connects
- [ ] Unity logs "Player joined"
- [ ] Phone assigned Mask #1
- [ ] Tap event received in Unity

### Gameplay Test:
- [ ] Press SPACE starts game
- [ ] QR code panel hides
- [ ] 3-5 masks appear on screen
- [ ] Round timer counts down
- [ ] Players eliminated correctly (wrong tap)
- [ ] Players eliminated correctly (missed tap)
- [ ] Game progresses to next round

### Multi-Player Test:
- [ ] 5+ phones connect
- [ ] Each gets unique Mask ID
- [ ] All taps received
- [ ] Multiple eliminations in one round
- [ ] Player count updates correctly

### Bonus Round Test:
- [ ] Round 5 triggers Sprint Round
- [ ] Sprint Round UI shows
- [ ] Tap counting works
- [ ] Players eliminated for <100 taps
- [ ] Round 10 triggers Reaction Round
- [ ] Reaction Round shows WAIT → GO
- [ ] Early taps eliminated
- [ ] Fastest 50% survive
- [ ] Round 15 triggers Precision Round
- [ ] Precision timing measured
- [ ] Closest 50% survive

### Win Condition Test:
- [ ] Game continues until 1 player left
- [ ] Winner screen displays
- [ ] Winner shows correct Mask ID
- [ ] Game ends properly

### Performance Test:
- [ ] 30+ players (use test-bot.js)
- [ ] 60 FPS maintained
- [ ] No lag or stuttering
- [ ] No memory leaks over 20 rounds

---

## 📊 Performance Expectations

### Target Metrics:
- **Frame Rate**: 60 FPS constant
- **Memory**: <2GB RAM
- **Update Time**: <10ms per frame
- **Player Count**: 60 concurrent

### Optimization Tips:
1. Use object pooling for mask displays (if needed)
2. Disable vsync: Edit → Project Settings → Quality → VSync Count = Don't Sync
3. Profile with Unity Profiler: Window → Analysis → Profiler
4. Reduce roundDuration if performance drops

---

## 🎨 Asset Requirements (Designer)

### Mask Sprites (Not Yet Created):
**Status**: Using placeholder colored circles

**Needed**:
- 60 unique mask sprites
- Format: PNG with transparency
- Size: 512x512px
- Naming: `mask_001.png` to `mask_060.png`
- Location: `Assets/Resources/Masks/`

**When ready**:
1. Place sprites in `Assets/Resources/Masks/`
2. Select GameManager → MaskManager component
3. Uncheck "Use Placeholder Sprites"
4. Sprites will auto-load from Resources folder

---

## 🔧 Configuration Options

### RoundController Settings:
```csharp
[SerializeField] private float roundDuration = 5f;  // Seconds to respond
[SerializeField] private int minMasks = 3;          // Min masks shown
[SerializeField] private int maxMasks = 5;          // Max masks shown
```

### GameManager Settings:
```csharp
[SerializeField] private int minPlayersToStart = 2;       // Min to start
[SerializeField] private int bonusRoundInterval = 5;      // Bonus every N rounds
```

### SprintRound Settings:
```csharp
[SerializeField] private int requiredTaps = 100;          // Taps needed
[SerializeField] private float roundDuration = 10f;       // Time limit
```

### ReactionRound Settings:
```csharp
[SerializeField] private float minWaitTime = 2f;          // Min delay before GO
[SerializeField] private float maxWaitTime = 5f;          // Max delay before GO
[SerializeField] private float reactionWindow = 3f;       // Time to respond
```

### PrecisionRound Settings:
```csharp
[SerializeField] private float targetTime = 5.0f;         // Target tap time
[SerializeField] private float roundDuration = 10f;       // Total time limit
```

Adjust these values in Inspector to tune difficulty.

---

## 📝 Architecture Notes

### Unity's Role:
Unity is the **game authority** - it runs ALL game logic:
- ✅ Decides which masks to show
- ✅ Evaluates all taps (correct/incorrect)
- ✅ Eliminates players
- ✅ Manages game state
- ✅ Sends updates to server

Server just relays messages (no validation).

### State Flow:
```
Lobby (waiting for players)
   ↓ [SPACE pressed]
Playing (main rounds)
   ↓ [every 5 rounds]
BonusRound (sprint/reaction/precision)
   ↓ [cycle back]
Playing
   ↓ [1 player left]
GameOver (winner declared)
```

### Event Flow:
```
Unity                Server              Mobile
  |                     |                   |
  |<--player_joined-----|<----connect------|
  |                     |                   |
  |---mask_assigned---->|--mask_assigned-->|
  |                     |                   |
  |<------tap-----------|<------tap--------|
  |                     |                   |
  |---eliminated------->|---eliminated---->|
```

---

## ✅ Completion Checklist

Before marking Unity as "done":

- [ ] Project opens without errors
- [ ] SocketIOUnity installed
- [ ] All scripts compile
- [ ] MainGame scene created
- [ ] GameManager GameObject exists with all 9 scripts
- [ ] UI Canvas created with all elements
- [ ] MaskDisplay prefab created
- [ ] All Inspector references linked
- [ ] No "None" or "Missing" references
- [ ] Press Play - no errors
- [ ] Connects to server successfully
- [ ] Receives player join events
- [ ] Receives tap events
- [ ] SPACE starts game
- [ ] Masks appear on screen
- [ ] Eliminations work correctly
- [ ] Bonus rounds trigger
- [ ] Winner screen appears
- [ ] 60 FPS with 30+ players

---

## 📞 Communication with Server

### Events Unity Receives:
- `player_joined` - {playerId, socketId}
- `player_left` - {playerId}
- `tap` - {playerId, timestamp}

### Events Unity Sends:
- `identify` - {type: "unity"}
- `mask_assigned` - {playerId, maskId}
- `game_state` - {phase, playersAlive, totalPlayers, roundNumber}
- `eliminated` - {playerId, reason, maskId, playersRemaining}

Coordinate with server developer to verify message format matches.

---

## 🎯 Success Criteria

### Minimum (MVP):
- ✅ Code written
- [ ] Scene created
- [ ] Game starts
- [ ] 10 players work
- [ ] Main rounds work

### Target:
- [ ] 60 players supported
- [ ] All bonus rounds work
- [ ] 60 FPS maintained
- [ ] No crashes
- [ ] Winner declared correctly

### Stretch:
- [ ] Real mask artwork
- [ ] Visual effects (tweens, particles)
- [ ] Kill feed system
- [ ] Sound effects

---

**Status**: ⚠️ Code complete, scene setup required
**Confidence**: 85% (depends on manual setup)
**Blockers**: Unity Editor setup (cannot be automated)

**Next Action**: Open project in Unity Hub and follow UNITY_SETUP_CHECKLIST.md
