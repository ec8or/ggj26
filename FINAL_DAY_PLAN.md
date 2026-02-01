# FINAL DAY PLAN - Mask Royale
**Last Day of GGJ26** 🚀

---

## Timeline

### Morning - At Home (2 hours)
- **Nils**: Debug tools, core fixes, mobile polish
- **Disa**: 1 hour of masks (target: 10 more masks = 20 total)

### Afternoon - On Site (2-3 hours with Rich)
- Polish, test, UI integration, juice
- Get remaining masks from Disa
- Full playtest with 40 debug players
- Final tweaks and balance

---

## ✅ What's Working

All core game loops are functional:
- ✅ **LOBBY**: QR code, masks appear as players join
- ✅ **SNAP ROUND**: Shows masks, detects taps, eliminates correctly
- ✅ **SPRINT ROUND**: Tap counting works
- ✅ **REACTION ROUND**: WAIT → GO logic functional
- ✅ **TIMING ROUND**: Precision timing measurement works

---

## 🎯 Critical Path (Must Have)

### 1. Visual Feedback System (HIGH PRIORITY)

**Problem**: Players eliminated but no clear visual indication on who's out/safe.

**Solution - Pass/Fail Overlays**:
- ✅ **Tick overlay** on masks that passed (green checkmark)
- ❌ **Cross/death overlay** on masks that failed (red X)
- Keep overlays on screen for 2-3 seconds before clearing
- Failed masks could shrink/fade or move to a "graveyard" area

**Implementation**:
```
When round evaluates:
1. Show tick on safe players
2. Show cross on eliminated players
3. Wait 2-3 seconds
4. Clear masks and move to next round
```

**Assets needed from Disa**:
- Tick/checkmark graphic (green)
- Cross/X graphic (red)
- Optional: Skull icon for eliminated

---

### 2. Title/Instruction Screens (HIGH PRIORITY)

**Before each round type, show:**
- Round name (big text)
- Brief instructions (1 sentence)
- Players remaining count
- "Press SPACE to start"

**Examples**:
```
┌─────────────────────────────┐
│      🎭 SNAP ROUND 🎭      │
│                             │
│ Tap when you see your mask! │
│                             │
│   Players Remaining: 30     │
│                             │
│   [Press SPACE to start]    │
└─────────────────────────────┘
```

**Screens needed**:
- SNAP: "Tap when you see your mask!"
- SPRINT: "Tap 100 times in 10 seconds!"
- REACTION: "Wait for GO, then tap FAST!"
- CHAOS: "Everyone gets a NEW mask!"
- ADVANCED SNAP: "More masks = harder to spot yours!"
- TIMING: "Tap at EXACTLY 5 seconds!"

**Implementation**:
- Create title panel in Unity
- Show before each round starts
- Press SPACE advances from title → round
- Update player count dynamically

---

### 3. Timing Round Polish (MEDIUM PRIORITY)

**Current state**: Works, but needs visual cues.

**Add**:
- 🔴 **RED light** (0-2 seconds) - "WAIT"
- 🟡 **YELLOW light** (2-4 seconds) - "GET READY"
- 🟢 **GREEN light** (at 5 seconds) - "TAP NOW!"
- Show timer counting up: "0.5s... 1.2s... 3.8s..."
- Show pass/fail after: "You tapped at 4.8s - TOO EARLY! ❌" or "5.1s - CLOSE! ✅"

**Assets needed**:
- Traffic light sprites (red/yellow/green circles)

---

### 4. Debug Tools (CRITICAL FOR TESTING)

**Need**: Simulate 40 players without 40 phones.

**Nils to implement**:
- Script to spawn 40 fake browser sessions
- Auto-connect to localhost:3000
- Each gets unique player ID and mask
- Can trigger auto-taps on command
- Button to "tap all" or "tap random 50%"

**Use case**:
```bash
cd server
node debug-clients.js 40  # Spawns 40 fake players
```

Unity console shows: "40 players connected"

---

## 🎨 Assets from Disa

### Must Have:
1. **10 more masks** (gets us to 20 total) - 1 hour
2. **Pass/Fail overlays**:
   - Green tick/checkmark
   - Red X/cross
   - Optional: Skull for death
3. **Main background** for Unity screen
4. **Traffic lights** for Timing round (red/yellow/green)

### Nice to Have:
5. UI boxes/frames for title screens
6. Any other polish (buttons, borders, etc.)

---

## 🎮 Final Round - Two Options

### Option A: Simple (START WITH THIS) ⭐

**When ~5 players remain**:
- One final REACTION round
- Show ALL remaining masks on screen
- As players tap, tick them off one by one
- Visual feedback: mask gets ✅ and timestamp
- Fastest player wins!

**Why**: Easy to implement, dramatic finish, clear winner.

**Implementation**:
```
1. Detect when <= 5 players remain
2. Show title: "FINAL ROUND - REACTION!"
3. Display all remaining masks
4. WAIT → GO signal
5. As taps come in, show tick + reaction time
6. First to tap = WINNER
```

---

### Option B: Public Vote (NICE-TO-HAVE) 💭

**When exactly 2 players remain**:

1. Show both final masks on screen
2. Send to both phones: "Why should you win?"
3. 20-second text input timer
4. Display both responses on screen
5. Audience votes (show hands, or use another system)
6. Winner declared!

**Why**: More interactive, funny, involves the whole crowd.

**Complexity**:
- Need text input on mobile client
- Need vote collection system (or manual judge decision)
- Need UI to display text responses

**Decision**: Implement Option A first. If time permits, add Option B.

---

## 📋 Task Breakdown

### **Nils (Morning - 2 hours)**

#### Debug Tools (30 min)
- [ ] Create `server/debug-clients.js`
- [ ] Spawns N fake players with unique IDs
- [ ] Auto-connects to server
- [ ] Commands: "tap all", "tap random %"

#### Visual Feedback (45 min)
- [ ] Add tick/cross overlays to MaskManager
- [ ] 2-3 second delay before clearing masks
- [ ] Animate overlays (fade in, scale)

#### Title Screens (30 min)
- [ ] Create TitleScreen panel in Unity
- [ ] Show round name + instructions
- [ ] Show players remaining count
- [ ] SPACE to advance logic

#### Mobile Polish (15 min)
- [ ] Test haptic feedback
- [ ] Ensure no zoom/refresh issues
- [ ] Connection status always visible

---

### **Disa (Morning - 1 hour)**

- [ ] **10 more masks** (priority #1)
- [ ] Green tick/checkmark overlay
- [ ] Red X/cross overlay
- [ ] Main background for Unity screen
- [ ] Traffic light sprites (red/yellow/green)

**File format**: PNG, transparent background, 512x512px
**Naming**: `mask_10.png` to `mask_19.png`
**Delivery**: Drop in `/unity/Assets/Resources/Masks/`

---

### **Rich (Afternoon - 2-3 hours with team)**

#### Scene Organization (30 min)
- [ ] Clean up MainGame scene hierarchy
- [ ] Group UI into logical holders
- [ ] Easier to navigate and find things

#### Layout & Grids (45 min)
- [ ] Snap round: 3-5 masks, large and clear
- [ ] Advanced Snap: More masks, tighter grid
- [ ] Sprint: Show tap count prominently
- [ ] All rounds: Maximize mask size on screen

#### Animations & Juice (30 min)
- [ ] Bobbing animation on masks (idle)
- [ ] Scale-in when masks appear
- [ ] Tick/cross fade-in animations
- [ ] Screen shake on eliminations (optional)

#### Chaos Card Implementation (30 min)
- [ ] Randomize all player masks before Advanced Snap
- [ ] Show "CHAOS!" title screen
- [ ] Emit new mask assignments to all players
- [ ] Give 3-5 seconds to see new mask

#### Final Round (30 min)
- [ ] Implement Option A (Reaction final with all masks shown)
- [ ] Show ticks as players tap in
- [ ] Display winner with their mask

#### Integration & Polish (30 min)
- [ ] Integrate Disa's assets (masks, overlays, backgrounds)
- [ ] Test all rounds with debug clients
- [ ] Balance tuning (elimination rates)
- [ ] Bug fixes

---

## 🧪 Testing Checklist

### With Debug Clients (40 fake players):

- [ ] LOBBY: All 40 masks appear
- [ ] SNAP: Correct eliminations (wrong tap, missed tap)
- [ ] SPRINT: Tap counting accurate, eliminates < 100 taps
- [ ] REACTION: WAIT → GO works, fastest 50% survive
- [ ] CHAOS: All masks randomize correctly
- [ ] ADVANCED SNAP: More masks displayed, harder
- [ ] TIMING: Traffic lights work, precision measured
- [ ] FINAL: Last ~5 players, reaction round, clear winner

### Visual Feedback:
- [ ] Tick appears on safe masks
- [ ] Cross appears on eliminated masks
- [ ] Overlays visible for 2-3 seconds
- [ ] Masks clear after feedback shown

### Title Screens:
- [ ] Show before each round type
- [ ] Player count updates correctly
- [ ] SPACE advances from title → round
- [ ] Instructions clear and readable

---

## ⚠️ Known Issues to Watch

1. **Socket.io disconnections**: Test stability with 40+ connections
2. **Mobile zoom/refresh**: Ensure CSS prevents accidental actions
3. **Timing sync**: Make sure all clients see rounds at same time
4. **Performance**: 60 FPS with 40+ masks on screen

---

## 🎯 Success Criteria

### Minimum (Must Ship):
- ✅ 20 masks (10 real + 10 placeholders OK if needed)
- ✅ All 4 main rounds work (Snap, Sprint, Reaction, Timing)
- ✅ Visual feedback (tick/cross) on pass/fail
- ✅ Title screens before each round
- ✅ Final round determines winner
- ✅ Runs stable with 40 players
- ✅ 5-minute presentation flows smoothly

### Target (Polish):
- ✅ 20+ real masks
- ✅ Background artwork
- ✅ Animations (bobbing, scale-in)
- ✅ Chaos Card works
- ✅ Advanced Snap round
- ✅ Traffic lights in Timing round
- ✅ Clean, organized scene

### Stretch (If Time):
- Public vote final round (Option B)
- Kill feed showing eliminations
- Sound effects
- Particle effects
- More masks (30+)

---

## 📝 Notes

- **Presentation is 5 minutes total**: 1 min intro, 2-4 min gameplay
- **Keep it ON RAILS**: Host controls pacing with SPACE key
- **Balance for drama**: Eliminate smoothly, not all at once
- **Test early, test often**: Use debug clients constantly
- **Don't over-engineer**: Ship a polished MVP, not a complex mess

---

## 🚀 Let's Go!

**Morning focus**: Debug tools + visual feedback + 10 more masks
**Afternoon focus**: Integration + polish + full playtest
**Goal**: Blow the crowd away with Mask Royale! 🎭👑

Good luck team! ✨

— Nils, Rich, Disa & Claude
