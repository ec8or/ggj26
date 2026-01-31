# Game Modes Design Document

**Status**: 💡 Design Phase - NOT YET IMPLEMENTED
**Last Updated**: 2026-01-30
**Purpose**: Extended game mode ideas and architectural considerations

---

## Current Implementation (v1.0)

### Existing Flow:
```
Lobby
  ↓ (SPACE pressed)
Playing (Snap Rounds) ←──┐
  ↓ (every 5 rounds)      │
BonusRound               │
  ↓ (cycle back) ─────────┘
  ↓ (1 player left)
GameOver
```

### Existing Modes:
1. **Snap Round**: Show 3-5 masks, tap if yours is shown
2. **Sprint Bonus**: Tap 100x in 10 seconds
3. **Reaction Bonus**: Fastest 50% after GO signal
4. **Precision Bonus**: Tap at exactly 5.0 seconds

---

## Proposed New Game Modes

### 1. Quick Draw
**Type**: Speed Elimination Round

**Setup**:
- 2-4 masks randomly selected
- All players see "READY..." screen
- Random delay (1-3 seconds)
- "GO!" signal appears

**Win Condition**:
- First to tap survive
- Last 1-2 players to tap are eliminated

**Lose Conditions**:
- **Hard**: Being in bottom 1-2 tappers
- **Time**: 3 second time limit after GO signal
  - Anyone not tapped after 3s = eliminated

**Mobile Display**:
- READY screen (wait for GO)
- GO signal (tap NOW!)
- Either: "SAFE!" or "TOO SLOW!"

**Unity Display**:
- Title card: "QUICK DRAW"
- Show the 2-4 selected masks
- Countdown/timer
- Tap order visualization (optional)

**Variation Ideas**:
- Show masks AFTER GO (even more reaction-based)
- Increase mask count as players reduce
- Fake-out signals (RED = wait, GREEN = tap)

---

### 2. Tug-o-War
**Type**: Category Battle Round

**Setup**:
- Remaining players grouped by mask category
- Example: 8 Luchadors vs 6 Animals vs 5 Tech
- 10-15 second round
- Each tap adds +1 to category score

**Win Condition**:
- Category with MOST total taps wins
- All players in winning category(s) survive
- All players in losing category(s) eliminated

**Lose Conditions**:
- **Hard**: Being in lowest-scoring category
- **Time**: 15 second time limit
  - Categories ranked by accumulated taps

**Mobile Display**:
- "TAP FOR YOUR CATEGORY!"
- Real-time tap counter (your taps)
- Category leaderboard (optional)

**Unity Display**:
- Title card: "TUG-O-WAR"
- Category icons/names displayed
- Live scoreboard showing category totals
- Bar chart racing visualization

**Variation Ideas**:
- Team eliminations (bottom 2 categories)
- Proportional elimination (each category loses bottom 20%)
- Require minimum taps per player (no freeloading)

**Balancing Notes**:
- Could be unfair if categories unbalanced
- Solution: Normalize by player count (avg taps per category)
- Or: Only trigger when 3+ players per category

---

### 3. Tap If You're NOT on Screen
**Type**: Reverse Logic Round

**Setup**:
- Triggered when ≤ 20 players remain
- Unity shows 1-10 random masks on screen
- Players whose masks are NOT shown must tap
- Players whose masks ARE shown must NOT tap

**Win Condition**:
- Correctly follow rule (tap if NOT shown, don't tap if shown)

**Lose Conditions**:
- **Hard**: Wrong action (tapped when shown, or didn't tap when hidden)
- **Time**: 5 second time limit
  - Anyone whose mask NOT shown who didn't tap = eliminated

**Mobile Display**:
- Player always sees their own mask
- Instruction: "TAP IF YOUR MASK ISN'T ON THE BIG SCREEN!"
- (Player must look at Unity display to know)

**Unity Display**:
- Title card: "TAP IF YOU'RE NOT ON SCREEN!"
- Shows 1-10 random masks prominently
- Timer counting down
- Eliminated players flash red

**Variation Ideas**:
- Increase masks shown over time during round
- Show different masks every 2 seconds (constant changes)
- "Tap if you ARE on screen" (flip logic for confusion)

**Balancing Notes**:
- Could be complex for players to understand
- Clear tutorial/title card essential
- Maybe trigger only once per game

---

## Random Events (Modifiers)

### Event 1: Switch Masks
**Trigger**: Randomly between rounds (10% chance?)

**Effect**:
- All alive players randomly reassigned new mask IDs
- Server sends new mask_assigned events
- Mobile phones update to show new mask
- Players now look for different mask

**Duration**: Permanent (until next switch event or end of game)

**Implementation Notes**:
- PlayerManager shuffles mask assignments
- NetworkManager emits new mask_assigned to all
- Could cause confusion but adds chaos/fun

**Variations**:
- Swap pairs (2 players trade masks)
- Category shuffle (all Luchadors swap with each other)
- Temporary (switch back after 3 rounds)

---

### Event 2: Demasked (Blind Mode)
**Trigger**: Randomly between rounds (5% chance?)

**Effect**:
- Mobile phones do NOT show player's mask for 1-3 rounds
- Phone shows: "???" or blank/static
- Players must remember their mask or guess
- Unity still shows all masks normally

**Duration**: 1-3 rounds, then masks reappear

**Implementation Notes**:
- Server sends "demasked" event to all phones
- Mobile app hides mask display
- After N rounds, server sends "unmasked" event
- Players who forgot their mask likely eliminated

**Variations**:
- Partial blind (only some players demasked)
- Everyone blind (Unity also hides masks)
- Scrambled mask (shows wrong mask on phone)

---

## Architectural Considerations

### Current Architecture (v1.0):
```
GameManager (state machine)
  ├── RoundController (main rounds)
  ├── SprintRound (bonus)
  ├── ReactionRound (bonus)
  └── PrecisionRound (bonus)
```

**Pros**:
- Simple state machine
- Each mode is isolated script
- Easy to understand

**Cons**:
- Hardcoded bonus interval (every 5 rounds)
- No randomization of modes
- Difficult to add many new modes

---

### Proposed Architecture (v2.0): Mode Pool System

```
GameManager (orchestrator)
  ├── ModeSelector (random mode picker)
  ├── Modes/
  │   ├── ShowMasksMode (current main round)
  │   ├── QuickDrawMode
  │   ├── TugOWarMode
  │   ├── TapIfNotOnScreenMode
  │   ├── SprintMode
  │   ├── ReactionMode
  │   └── PrecisionMode
  └── EventManager (random events)
```

**Implementation**:
```csharp
public abstract class GameMode : MonoBehaviour
{
    public abstract string ModeName { get; }
    public abstract string ModeDescription { get; }
    public abstract void StartMode();
    public abstract void UpdateMode();
    public abstract void EvaluateMode();
}

public class ModeSelector
{
    private List<GameMode> availableModes;
    private Dictionary<GameMode, int> modeWeights;

    public GameMode SelectRandomMode()
    {
        // Weighted random selection
        // Can exclude recently played modes
        // Can favor certain modes based on player count
    }
}
```

**Pros**:
- Easy to add new modes (just inherit GameMode)
- Random selection keeps game fresh
- Can weight modes by player count/game state
- Modular and extensible

**Cons**:
- More complex architecture
- Requires refactoring existing code
- Testing more difficult (random outcomes)

---

### Separate Scenes vs Single Scene?

#### Option A: Single Scene (Current)
**Pros**:
- Faster transitions (no scene loading)
- Shared state/references
- Simpler debugging

**Cons**:
- All UI elements in one scene (cluttered)
- All modes loaded in memory
- Potential reference complexity

#### Option B: Separate Scenes per Mode
**Pros**:
- Clean separation of concerns
- Only relevant UI loaded
- Easier to design mode-specific UI

**Cons**:
- Scene loading time (1-2 seconds)
- Must pass state between scenes (PlayerManager, etc.)
- More complex scene management

#### Option C: Hybrid - Main Scene + Additive Scenes
**Pros**:
- Core systems stay loaded (PlayerManager, NetworkManager)
- Mode-specific scenes load additively
- No state passing needed (DontDestroyOnLoad singletons)

**Cons**:
- Slightly more complex
- Must manage active scene switching

**Recommendation**: Option C (Hybrid) for v2.0
- Keep current architecture for MVP
- If adding 5+ more modes, refactor to additive scenes

---

## Universal Round Flow (v2.0)

### New Standard Flow:
```
1. Check Win Condition
   ↓ (if >1 player alive)
2. Show Remaining Masks Screen
   ↓ (display all alive players, 3 seconds)
3. Select Next Mode
   ↓ (random or weighted selection)
4. Show Title Card
   ↓ ("QUICK DRAW", "TUG-O-WAR", etc., 2 seconds)
5. Execute Mode
   ↓ (mode-specific gameplay)
6. Evaluate & Eliminate
   ↓ (apply lose conditions)
7. Show Elimination Screen
   ↓ (killed X players, 2 seconds)
8. Loop to Step 1
```

### Implementation:
```csharp
void CheckWinCondition()
{
    if (PlayerManager.GetAliveCount() <= 1)
    {
        EndGame();
    }
    else
    {
        ShowRemainingMasks();
    }
}

void ShowRemainingMasks()
{
    // Display all alive masks on screen
    // 3 second pause
    Invoke(nameof(SelectNextMode), 3f);
}

void SelectNextMode()
{
    GameMode nextMode = modeSelector.SelectRandomMode();
    ShowTitleCard(nextMode.ModeName, nextMode.ModeDescription);
}

void ShowTitleCard(string title, string description)
{
    // Display title card UI
    // 2 second pause
    Invoke(nameof(ExecuteMode), 2f);
}

void ExecuteMode()
{
    currentMode.StartMode();
}
```

---

## Dual Elimination System

### Hard Lose Condition:
**Definition**: Absolute failure state
**Examples**:
- Being last to tap in Quick Draw
- Being in losing category in Tug-o-War
- Tapping when mask IS shown (Tap If NOT on Screen)
- Missing 100 tap threshold in Sprint

**Timing**: Evaluated immediately when condition met

### Time Limit Condition:
**Definition**: Failure to complete by deadline
**Examples**:
- Not tapping within 5 seconds
- Not reaching tap threshold before timer expires
- Not making decision before countdown ends

**Timing**: Evaluated when timer reaches 0

### Implementation:
```csharp
public abstract class GameMode
{
    protected float timeLimit;
    protected float timeRemaining;

    protected abstract void CheckHardLoseConditions();
    protected abstract void CheckTimeLimitConditions();

    void Update()
    {
        if (!modeActive) return;

        // Check hard conditions continuously
        CheckHardLoseConditions();

        // Count down timer
        timeRemaining -= Time.deltaTime;
        UIManager.UpdateTimer(timeRemaining);

        // Check time limit
        if (timeRemaining <= 0)
        {
            CheckTimeLimitConditions();
            EvaluateMode();
        }
    }
}
```

**Example - Quick Draw**:
```csharp
void CheckHardLoseConditions()
{
    // No hard condition until GO signal
    if (!goSignalFired) return;

    // Check if enough players tapped
    if (tapOrder.Count >= requiredTapCount)
    {
        // Last players who didn't tap = eliminated
        EliminateSlowPlayers();
        EvaluateMode();
    }
}

void CheckTimeLimitConditions()
{
    // Anyone who didn't tap by time limit = eliminated
    foreach (var player in PlayerManager.GetAlivePlayers())
    {
        if (!playersTapped.Contains(player.Id))
        {
            PlayerManager.EliminatePlayer(player.Id, "too_slow");
        }
    }
}
```

---

## Mode Selection Logic

### Factors to Consider:
1. **Player Count**
   - >30 players: Favor elimination-heavy modes
   - 10-30 players: Balanced mix
   - <10 players: Favor skill-based modes

2. **Game Length**
   - Early game (rounds 1-5): Simpler modes
   - Mid game (rounds 6-15): All modes available
   - Late game (rounds 16+): High-stakes modes

3. **Recent History**
   - Don't repeat same mode 2x in a row
   - Try to cycle through all modes before repeating

4. **Category Balance**
   - Only trigger Tug-o-War if 3+ categories represented
   - Only trigger Tap If NOT on Screen if ≤20 players

### Weighted Selection Example:
```csharp
Dictionary<GameMode, int> GetModeWeights()
{
    int playerCount = PlayerManager.GetAliveCount();
    int roundNumber = GameManager.CurrentRound;

    var weights = new Dictionary<GameMode, int>();

    // ShowMasks (main mode) - always available
    weights[showMasksMode] = 10;

    // Quick Draw - favor in early/mid game
    weights[quickDrawMode] = roundNumber < 10 ? 8 : 5;

    // Tug-o-War - only if balanced categories
    if (HasBalancedCategories())
        weights[tugOWarMode] = 6;

    // Tap If NOT - only late game
    if (playerCount <= 20)
        weights[tapIfNotMode] = 7;

    // Sprint/Reaction/Precision - standard bonuses
    weights[sprintMode] = 5;
    weights[reactionMode] = 5;
    weights[precisionMode] = 5;

    // Reduce weight if played recently
    foreach (var mode in recentModes)
    {
        if (weights.ContainsKey(mode))
            weights[mode] /= 2;
    }

    return weights;
}
```

---

## UI/UX Considerations

### Title Cards:
Every mode needs clear title card with:
- Mode name (large, bold)
- Quick instruction (1 sentence)
- Icon/visual indicator
- 2-3 second display time

**Examples**:
```
┌─────────────────────┐
│   QUICK DRAW        │
│                     │
│  Tap FAST after GO! │
│  Last 2 OUT!        │
│                     │
│      [⚡icon]        │
└─────────────────────┘

┌─────────────────────┐
│   TUG-O-WAR         │
│                     │
│ Tap for your team!  │
│ Lowest team OUT!    │
│                     │
│   [🤜💥🤛icon]      │
└─────────────────────┘
```

### Remaining Masks Screen:
Between rounds, show:
- All alive masks in grid layout
- Player count prominently
- Round number
- Brief pause for dramatic effect

### Mobile Instructions:
- Mode-specific instructions on phone
- "WAIT..." vs "TAP NOW!" states
- Clear feedback ("SAFE!", "ELIMINATED!")
- Visual cues match Unity display

---

## Balancing Considerations

### Elimination Rate:
- **Early game**: 20-30% per round (rapid reduction)
- **Mid game**: 10-20% per round (steady)
- **Late game**: 5-10% per round (tension building)

**Mode difficulty scaling**:
- Increase time pressure as rounds progress
- Increase elimination percentage as player count drops
- Add more complex modes in late game

### Category Balance (Tug-o-War):
**Problem**: Unfair if one category has many more players

**Solution Options**:
1. **Normalize by count**: Score = total taps / player count
2. **Cap advantage**: Max 2x multiplier for smaller teams
3. **Proportional elimination**: Each category loses bottom 20%
4. **Handicap**: Smaller teams get head start

**Recommendation**: Normalize by count (most fair)

### Mode Frequency:
Suggested distribution over 20 rounds:
- ShowMasks: 40% (8 rounds)
- Quick Draw: 15% (3 rounds)
- Tug-o-War: 10% (2 rounds)
- Tap If NOT: 5% (1 round)
- Sprint/Reaction/Precision: 30% (6 rounds)

---

## Testing Checklist (When Implemented)

### Per Mode:
- [ ] Hard lose condition works correctly
- [ ] Time limit condition works correctly
- [ ] Mobile UI shows correct instructions
- [ ] Unity UI shows correct title card
- [ ] Eliminations sent to correct players
- [ ] Edge cases handled (all players fail, all pass, etc.)

### Mode Selector:
- [ ] Weighted selection distributes fairly
- [ ] No repeated modes back-to-back
- [ ] Player count filters work
- [ ] Recent history tracking works

### Flow:
- [ ] Win condition checked after each mode
- [ ] Remaining masks screen displays
- [ ] Title cards show before each mode
- [ ] Transitions smooth (no jarring jumps)

### Events:
- [ ] Switch Masks reassigns correctly
- [ ] Demasked hides masks on phones
- [ ] Events don't break gameplay
- [ ] Events unwind properly

---

## Implementation Priority

### Phase 1 (Current - MVP):
✅ ShowMasks (main round)
✅ Sprint bonus
✅ Reaction bonus
✅ Precision bonus

### Phase 2 (Next - Mode Variety):
- [ ] Refactor to GameMode base class
- [ ] Implement ModeSelector
- [ ] Add Quick Draw mode
- [ ] Add Remaining Masks screen
- [ ] Add Title Cards

### Phase 3 (Polish - More Modes):
- [ ] Add Tug-o-War mode
- [ ] Add Tap If NOT mode
- [ ] Implement weighted selection
- [ ] Add mode history tracking

### Phase 4 (Stretch - Events):
- [ ] Add EventManager system
- [ ] Implement Switch Masks event
- [ ] Implement Demasked event
- [ ] Add event probability tuning

---

## Open Questions

1. **Scene architecture**: Single scene sufficient or need additive scenes?
2. **Mode selection**: Pure random or weighted by game state?
3. **Event timing**: Between rounds or mid-round?
4. **Tug-o-War balancing**: Normalize or proportional elimination?
5. **Player confusion**: Too many different modes?
6. **Tutorial**: How to teach 7+ different modes quickly?
7. **Mode exhaustion**: How many rounds before patterns get stale?

---

## Notes for Future Development

### If Implementing v2.0:
1. Create `GameMode.cs` abstract base class
2. Refactor existing modes to inherit from GameMode
3. Create `ModeSelector.cs` with weighted random selection
4. Add `RemainingMasksScreen` UI
5. Add `TitleCard` UI system
6. Implement new modes one at a time (test thoroughly)
7. Add event system last (most complex)

### Backward Compatibility:
- Keep v1.0 architecture working
- Add v2.0 as optional toggle in GameManager
- Allow switching between fixed rotation and random selection

### Performance:
- Mode switching should be instantaneous (no loading)
- UI transitions should be smooth (use DOTween or Animation)
- Avoid garbage collection spikes during mode changes

---

**Status**: Design document complete - awaiting decision to implement
**Next Step**: Discuss with team, decide on Phase 2 timeline
**Estimated Work**: Phase 2 ~8 hours, Phase 3 ~6 hours, Phase 4 ~4 hours

---

**Last Updated**: 2026-01-30
**Document Version**: 1.0
**Implementation Version**: Still on v1.0 (basic modes only)
