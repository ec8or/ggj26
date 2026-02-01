# Updated Game Flow - Final Day Changes

## Key Changes

### 1. Snap Rounds - Internal Loops
- Snap rounds now have **5 loops** within a single round
- Each loop shows different mask combinations (2-5 masks)
- No return to title screen between loops
- Better pacing, less interruption
- Single title screen → 5 elimination cycles → results

### 2. Reaction Round - Final Death Match
**NEW DESIGN:** Continuous elimination until 1 winner!

**How it works:**
- All remaining masks visible on screen
- Cycle: RED light (WAIT) → GREEN light (GO!) → repeat
- Eliminate on:
  - Tap during RED = disqualified (too eager)
  - Last to tap on GREEN = eliminated (too slow)
- Masks knocked off as players die
- Continue until 1 mask remains = WINNER 👑

**Visual:**
- Red/green lights from Disa
- Masks fade/disappear on elimination
- Victory celebration for last standing

### 3. Manual Round Control
**Keyboard shortcuts for host control:**
- `1` = Snap Round
- `2` = Sprint Round
- `3` = Reaction Round (death match)
- `4` = Timing/Precision Round
- `5` = Chaos Round
- `SPACE` = Advance current round/screen

**Why:** Adapt presentation on the fly based on:
- Time remaining
- Player count
- Crowd energy
- How rounds are playing

## Revised Round Sequence (Suggested)

### Opening (40 players → ~30)
- Snap Round (5 loops)
- Sprint Round
- Snap Round (5 loops)

### Mid-Game (~30 → ~15)
- Chaos Card! (randomize masks)
- Advanced Snap (5 loops, more masks)
- Timing Round
- Sprint Round

### End Game (~15 → 1)
- Advanced Snap (5 loops)
- Timing Round
- **Reaction Death Match** (continuous until winner)

## Host Controls Summary
- `SPACE`: Advance/Start rounds
- `1-5`: Manually trigger specific round types
- `C`: Chaos Card (if needed outside sequence)

## Asset Requirements
- 40 masks total (mask_0.png to mask_39.png)
- Red light (light_red.png)
- Green light (light_green.png)
- Pass/fail overlays (tick_green.png, cross_red.png)
- Background (optional)

## Testing
Use bots to test:
```bash
node test-bot.js 40 http://localhost:3000 5
```

Try different round sequences with manual triggers to find best flow!
