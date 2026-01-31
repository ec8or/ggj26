# Debug Controls Plan

**Status**: 📋 Planning Phase - NOT YET IMPLEMENTED
**Last Updated**: 2026-01-30
**Purpose**: Debug tools for rapid testing without full playtest

---

## Overview

**Problem**: Testing gameplay requires:
- Multiple phone connections
- Full round completion
- Waiting for specific modes
- Can't easily test edge cases

**Solution**: Debug controls to:
- Simulate players instantly
- Jump to specific modes
- Trigger eliminations manually
- Test UI states without gameplay

---

## Debug Mode Activation

### Option A: Keyboard Toggle
```
Press F12 to toggle debug mode
  ↓
Debug overlay appears
Press F12 again to hide
```

### Option B: Inspector Toggle
```csharp
[SerializeField] private bool debugModeEnabled = false;
```

### Option C: Build Define
```csharp
#if UNITY_EDITOR || DEBUG_BUILD
    // Debug controls active
#endif
```

**Recommendation**: Option A (F12 toggle) + Option C (only in editor/debug builds)

---

## Debug Overlay UI

### Layout:
```
┌──────────────────────────────────────┐
│  DEBUG CONTROLS [F12 to hide]       │
├──────────────────────────────────────┤
│                                      │
│  PLAYERS:                            │
│  [+1] [+5] [+10] [+20] [Clear All]  │
│  Current: 0                          │
│                                      │
│  GAME STATE:                         │
│  [Start Game] [Next Round] [End]    │
│  Round: 0 | Alive: 0/0              │
│                                      │
│  LAUNCH MODE:                        │
│  [Main] [Sprint] [Reaction]         │
│  [Precision] [Quick Draw]           │
│  [Tug-o-War] [Tap If NOT]          │
│                                      │
│  ELIMINATIONS:                       │
│  [Kill Random] [Kill Half]          │
│  [Kill Category] [Select...]        │
│                                      │
│  EVENTS:                             │
│  [Switch Masks] [Demasked]          │
│                                      │
│  UI TESTS:                           │
│  [Title Card] [Winner] [Masks]      │
│                                      │
│  CHEATS:                             │
│  [God Mode] [Slow Mo] [Speed Up]   │
│                                      │
└──────────────────────────────────────┘
```

---

## Player Management

### Add Virtual Players

**Keyboard Shortcuts**:
- `1` - Add 1 player
- `5` - Add 5 players
- `0` (zero) - Add 10 players
- `Shift+0` - Add 20 players

**Button Actions**:
```csharp
void AddVirtualPlayer()
{
    string playerId = $"DEBUG_{System.Guid.NewGuid()}";
    PlayerManager.Instance.AddPlayer(playerId); // Simulates player_joined
}

void AddMultiplePlayers(int count)
{
    for (int i = 0; i < count; i++)
    {
        AddVirtualPlayer();
        // Small delay to avoid overwhelming system
        await Task.Delay(50);
    }
}
```

**Virtual Player Behavior**:
- Assigned mask ID like real players
- Appear in player count
- Do NOT auto-tap (unless God Mode)
- Can be eliminated
- No network connection (local only)

---

### Remove Players

**Keyboard Shortcuts**:
- `Backspace` - Remove 1 random player
- `Shift+Backspace` - Remove all players

**Button Actions**:
```csharp
void RemoveRandomPlayer()
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
    if (alivePlayers.Count > 0)
    {
        var randomPlayer = alivePlayers[Random.Range(0, alivePlayers.Count)];
        PlayerManager.Instance.EliminatePlayer(randomPlayer.Id, "debug_removed");
    }
}

void RemoveAllPlayers()
{
    var allPlayers = PlayerManager.Instance.GetAllPlayers();
    foreach (var player in allPlayers)
    {
        PlayerManager.Instance.RemovePlayer(player.Id);
    }
}
```

---

### Player Presets

**Quick Setup Buttons**:
- `Small Game` - 10 players (2 per category)
- `Medium Game` - 30 players (5 per category)
- `Large Game` - 60 players (10 per category)
- `Unbalanced` - 20 Luchadors, 5 Animals, 3 Tech

```csharp
void SetupSmallGame()
{
    RemoveAllPlayers();
    AddMultiplePlayers(10);
    Debug.Log("Setup: Small game (10 players)");
}

void SetupUnbalancedGame()
{
    RemoveAllPlayers();
    // Add players with specific mask categories
    for (int i = 0; i < 20; i++) AddPlayerWithCategory("Luchador");
    for (int i = 0; i < 5; i++) AddPlayerWithCategory("Animal");
    for (int i = 0; i < 3; i++) AddPlayerWithCategory("Tech");
}
```

---

## Game State Control

### Skip to Game Start

**Keyboard Shortcut**: `Space` (same as normal, but works anytime in debug mode)

**Action**:
- If in Lobby: Start game normally
- If in Playing: Do nothing (already playing)
- If in GameOver: Restart game

```csharp
void DebugStartGame()
{
    if (GameManager.Instance.CurrentState == GameState.Lobby)
    {
        GameManager.Instance.StartGame();
    }
    else if (GameManager.Instance.CurrentState == GameState.GameOver)
    {
        GameManager.Instance.RestartGame();
    }
}
```

---

### Skip to Next Round

**Keyboard Shortcut**: `N` (already implemented!)

**Action**:
- End current mode immediately
- Skip to next round
- No eliminations (unless forced)

```csharp
void DebugNextRound()
{
    if (currentMode != null)
    {
        currentMode.ForceEndMode();
    }
    GameManager.Instance.NextRound();
}
```

---

### End Game

**Keyboard Shortcut**: `E`

**Action**:
- Eliminate all but 1 player
- Trigger winner screen
- Return to lobby

```csharp
void DebugEndGame()
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

    // Keep first player alive, eliminate rest
    for (int i = 1; i < alivePlayers.Count; i++)
    {
        PlayerManager.Instance.EliminatePlayer(alivePlayers[i].Id, "debug_end");
    }

    GameManager.Instance.CheckWinCondition();
}
```

---

## Mode Launcher

### Launch Specific Mode

**Keyboard Shortcuts**:
- `F1` - Main Round (Show Masks)
- `F2` - Sprint Round
- `F3` - Reaction Round
- `F4` - Precision Round
- `F5` - Quick Draw (if implemented)
- `F6` - Tug-o-War (if implemented)
- `F7` - Tap If NOT (if implemented)

**Button Actions**:
```csharp
void LaunchMode(string modeName)
{
    // End current mode if active
    if (currentMode != null)
    {
        currentMode.ForceEndMode();
    }

    // Find and launch requested mode
    GameMode mode = FindModeByName(modeName);
    if (mode != null)
    {
        Debug.Log($"DEBUG: Launching {modeName}");
        GameManager.Instance.StartMode(mode);
    }
    else
    {
        Debug.LogWarning($"Mode not found: {modeName}");
    }
}
```

**Usage**:
- Press F1-F7 to instantly launch mode
- Current round interrupted
- No eliminations from previous mode
- Useful for testing mode-specific UI/logic

---

### Mode Parameters Override

**UI**: Sliders/fields to override mode settings

```
┌──────────────────────────────────┐
│  MODE PARAMETERS:                │
│                                  │
│  Round Duration:  [5.0] seconds  │
│  Min Masks:       [3]            │
│  Max Masks:       [5]            │
│  Sprint Taps:     [100]          │
│  Reaction Window: [3.0] seconds  │
│                                  │
│  [Apply] [Reset to Defaults]    │
└──────────────────────────────────┘
```

```csharp
void OverrideRoundParameters()
{
    if (currentMode is RoundController rc)
    {
        rc.roundDuration = debugRoundDuration;
        rc.minMasks = debugMinMasks;
        rc.maxMasks = debugMaxMasks;
    }
}
```

**Use Case**: Test edge cases without changing code
- Very short rounds (1 second)
- Very long rounds (30 seconds)
- Show all masks (60 masks)
- Impossible sprint target (1000 taps)

---

## Elimination Controls

### Manual Elimination

**Keyboard Shortcuts**:
- `K` - Kill random player
- `Shift+K` - Kill half the players
- `Ctrl+K` - Kill specific category (cycles: Hannya → Luchador → Theatre → Animal → Sports → Tech)

**Button Actions**:
```csharp
void KillRandomPlayer()
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
    if (alivePlayers.Count > 0)
    {
        var victim = alivePlayers[Random.Range(0, alivePlayers.Count)];
        PlayerManager.Instance.EliminatePlayer(victim.Id, "debug_killed");
        Debug.Log($"DEBUG: Killed Mask #{victim.MaskId}");
    }
}

void KillHalfPlayers()
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
    int halfCount = Mathf.CeilToInt(alivePlayers.Count / 2f);

    var shuffled = alivePlayers.OrderBy(x => Random.value).ToList();
    for (int i = 0; i < halfCount; i++)
    {
        PlayerManager.Instance.EliminatePlayer(shuffled[i].Id, "debug_half");
    }

    Debug.Log($"DEBUG: Killed {halfCount} players");
}

void KillCategory(string categoryName)
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
    int killed = 0;

    foreach (var player in alivePlayers)
    {
        if (GetMaskCategory(player.MaskId) == categoryName)
        {
            PlayerManager.Instance.EliminatePlayer(player.Id, "debug_category");
            killed++;
        }
    }

    Debug.Log($"DEBUG: Killed {killed} {categoryName} players");
}
```

---

### Select Players to Eliminate

**UI**: Clickable mask grid

```
┌────────────────────────────────────┐
│  SELECT PLAYERS TO ELIMINATE:      │
│                                    │
│  [X] Mask #1   [ ] Mask #5        │
│  [ ] Mask #12  [X] Mask #18       │
│  [X] Mask #23  [ ] Mask #34       │
│  ...                               │
│                                    │
│  [Eliminate Selected (3)]          │
└────────────────────────────────────┘
```

```csharp
List<string> selectedPlayerIds = new List<string>();

void TogglePlayerSelection(string playerId)
{
    if (selectedPlayerIds.Contains(playerId))
        selectedPlayerIds.Remove(playerId);
    else
        selectedPlayerIds.Add(playerId);
}

void EliminateSelected()
{
    foreach (var playerId in selectedPlayerIds)
    {
        PlayerManager.Instance.EliminatePlayer(playerId, "debug_selected");
    }

    Debug.Log($"DEBUG: Eliminated {selectedPlayerIds.Count} selected players");
    selectedPlayerIds.Clear();
}
```

---

## Event Triggers

### Random Event Testing

**Keyboard Shortcuts**:
- `M` - Switch Masks event
- `B` - Demasked (Blind) event

**Button Actions**:
```csharp
void TriggerSwitchMasks()
{
    Debug.Log("DEBUG: Triggering Switch Masks event");
    EventManager.Instance.TriggerSwitchMasksEvent();
}

void TriggerDemasked()
{
    Debug.Log("DEBUG: Triggering Demasked event");
    EventManager.Instance.TriggerDemaskedEvent(3); // 3 rounds
}
```

**Event Parameters**:
```
┌──────────────────────────────────┐
│  EVENT: Demasked                 │
│                                  │
│  Duration: [3] rounds            │
│  Affect: [All] / [Random 50%]   │
│                                  │
│  [Trigger Event]                 │
└──────────────────────────────────┘
```

---

## UI State Testing

### Test Individual UI Elements

**Keyboard Shortcuts**:
- `T` - Show test title card
- `W` - Show winner screen
- `R` - Show remaining masks screen

**Button Actions**:
```csharp
void TestTitleCard()
{
    UIManager.Instance.ShowTitleCard(
        "TEST MODE",
        "This is a test title card!",
        2f
    );
}

void TestWinnerScreen()
{
    UIManager.Instance.ShowWinner(42); // Test with Mask #42
}

void TestRemainingMasks()
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
    MaskManager.Instance.DisplayRemainingMasks(alivePlayers);
}
```

---

### UI Inspector Mode

**Keyboard Shortcut**: `I`

**Action**: Toggle UI debug info overlay

```
┌──────────────────────────────────┐
│  UI DEBUG INFO:                  │
│                                  │
│  Current State: Playing          │
│  Round Number: 5                 │
│  Players Alive: 23/45            │
│  Current Mode: SprintRound       │
│  Mode Time Remaining: 7.3s       │
│  Masks Displayed: 4              │
│  Active Panels: RoundTimer       │
│                                  │
└──────────────────────────────────┘
```

```csharp
void DrawUIDebugInfo()
{
    GUILayout.BeginArea(new Rect(10, 400, 300, 200));
    GUILayout.Label("UI DEBUG INFO:");
    GUILayout.Label($"State: {GameManager.Instance.CurrentState}");
    GUILayout.Label($"Round: {GameManager.Instance.CurrentRound}");
    GUILayout.Label($"Players: {PlayerManager.Instance.GetAliveCount()}/{PlayerManager.Instance.GetTotalCount()}");
    GUILayout.Label($"Mode: {currentMode?.GetType().Name ?? "None"}");
    GUILayout.Label($"Time: {currentMode?.TimeRemaining:F1}s");
    GUILayout.EndArea();
}
```

---

## Cheat Codes

### God Mode

**Keyboard Shortcut**: `G`

**Effect**:
- Virtual players auto-tap correctly every round
- Never eliminated
- Useful for testing late-game scenarios

```csharp
bool godModeEnabled = false;

void ToggleGodMode()
{
    godModeEnabled = !godModeEnabled;
    Debug.Log($"DEBUG: God Mode {(godModeEnabled ? "ON" : "OFF")}");
}

// In RoundController evaluation:
void EvaluateRound()
{
    if (DebugManager.Instance.GodModeEnabled)
    {
        // Skip eliminations
        Debug.Log("DEBUG: God Mode - No eliminations");
        return;
    }

    // Normal evaluation...
}
```

---

### Time Scale Control

**Keyboard Shortcuts**:
- `[` - Slow down time (0.5x)
- `]` - Speed up time (2x)
- `\` - Reset time (1x)

**Effect**:
- Slow motion for debugging animations
- Fast forward for long rounds

```csharp
void SetTimeScale(float scale)
{
    Time.timeScale = scale;
    Debug.Log($"DEBUG: Time scale set to {scale}x");
}

void Update()
{
    if (Input.GetKeyDown(KeyCode.LeftBracket))
        SetTimeScale(0.5f);

    if (Input.GetKeyDown(KeyCode.RightBracket))
        SetTimeScale(2f);

    if (Input.GetKeyDown(KeyCode.Backslash))
        SetTimeScale(1f);
}
```

---

### Instant Win

**Keyboard Shortcut**: `Shift+W`

**Action**:
- Eliminate all players except one random survivor
- Show winner screen immediately

```csharp
void DebugInstantWin()
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
    if (alivePlayers.Count == 0) return;

    var winner = alivePlayers[Random.Range(0, alivePlayers.Count)];

    foreach (var player in alivePlayers)
    {
        if (player.Id != winner.Id)
        {
            PlayerManager.Instance.EliminatePlayer(player.Id, "debug_instant_win");
        }
    }

    GameManager.Instance.EndGame();
}
```

---

## Network Testing

### Simulate Tap Events

**Keyboard Shortcut**: `Numpad 1-9` - Simulate tap from player 1-9

**Action**:
```csharp
void SimulateTap(int playerIndex)
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
    if (playerIndex < alivePlayers.Count)
    {
        var player = alivePlayers[playerIndex];
        var tapData = new TapData
        {
            PlayerId = player.Id,
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };

        NetworkManager.Instance.OnTapReceived?.Invoke(tapData);
        Debug.Log($"DEBUG: Simulated tap from Mask #{player.MaskId}");
    }
}
```

---

### Simulate Disconnection

**Keyboard Shortcut**: `D` - Disconnect random player

**Action**:
```csharp
void SimulateDisconnection()
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
    if (alivePlayers.Count > 0)
    {
        var player = alivePlayers[Random.Range(0, alivePlayers.Count)];
        NetworkManager.Instance.OnPlayerLeft?.Invoke(player.Id);
        Debug.Log($"DEBUG: Simulated disconnection of Mask #{player.MaskId}");
    }
}
```

---

## Logging & Analytics

### Debug Log Levels

**Keyboard Shortcut**: `L` - Cycle log verbosity

**Levels**:
1. **Silent** - No debug logs
2. **Errors Only** - Only errors/warnings
3. **Info** - Game state changes
4. **Verbose** - Everything (taps, eliminations, etc.)

```csharp
public enum DebugLogLevel { Silent, ErrorsOnly, Info, Verbose }
private DebugLogLevel currentLogLevel = DebugLogLevel.Info;

void DebugLog(string message, DebugLogLevel level)
{
    if (currentLogLevel >= level)
    {
        Debug.Log($"[DEBUG] {message}");
    }
}
```

---

### Performance Stats

**Keyboard Shortcut**: `P` - Toggle performance overlay

**Display**:
```
┌──────────────────────────┐
│  PERFORMANCE:            │
│  FPS: 60.0               │
│  Frame Time: 16.6ms      │
│  Memory: 342 MB          │
│  Players: 23             │
│  Network Events: 145/s   │
└──────────────────────────┘
```

```csharp
void DrawPerformanceStats()
{
    float fps = 1f / Time.deltaTime;
    float frameTime = Time.deltaTime * 1000f;
    long memory = System.GC.GetTotalMemory(false) / 1024 / 1024;

    GUILayout.BeginArea(new Rect(Screen.width - 210, 10, 200, 150));
    GUILayout.Label("PERFORMANCE:");
    GUILayout.Label($"FPS: {fps:F1}");
    GUILayout.Label($"Frame: {frameTime:F1}ms");
    GUILayout.Label($"Memory: {memory} MB");
    GUILayout.Label($"Players: {PlayerManager.Instance.GetAliveCount()}");
    GUILayout.EndArea();
}
```

---

## Saved Debug States

### Save Current State

**Keyboard Shortcut**: `Ctrl+S` - Save current game state

**Saves**:
- Player count and mask assignments
- Round number
- Game state
- Current mode

```csharp
[System.Serializable]
public class DebugSaveState
{
    public int roundNumber;
    public string gameState;
    public List<PlayerSnapshot> players;
}

void SaveDebugState(string name)
{
    var state = new DebugSaveState
    {
        roundNumber = GameManager.Instance.CurrentRound,
        gameState = GameManager.Instance.CurrentState.ToString(),
        players = PlayerManager.Instance.GetAllPlayers().Select(p => new PlayerSnapshot(p)).ToList()
    };

    string json = JsonUtility.ToJson(state, true);
    File.WriteAllText($"DebugStates/{name}.json", json);
    Debug.Log($"DEBUG: Saved state '{name}'");
}
```

---

### Load Saved State

**Keyboard Shortcut**: `Ctrl+L` - Load saved game state

**Action**:
- Clear current players
- Recreate players from save
- Set round number
- Restore game state

```csharp
void LoadDebugState(string name)
{
    string path = $"DebugStates/{name}.json";
    if (!File.Exists(path))
    {
        Debug.LogWarning($"DEBUG: State '{name}' not found");
        return;
    }

    string json = File.ReadAllText(path);
    var state = JsonUtility.FromJson<DebugSaveState>(json);

    // Restore state
    PlayerManager.Instance.RemoveAllPlayers();
    foreach (var playerSnap in state.players)
    {
        PlayerManager.Instance.RestorePlayer(playerSnap);
    }

    GameManager.Instance.CurrentRound = state.roundNumber;
    Debug.Log($"DEBUG: Loaded state '{name}'");
}
```

---

## Debug Scenarios

### Preset Test Scenarios

**Button Shortcuts**:
- `Early Game` - Round 1, 45 players, no eliminations yet
- `Mid Game` - Round 8, 20 players, mixed categories
- `Late Game` - Round 15, 5 players, tense finale
- `Final Two` - 2 players remaining, next round wins

```csharp
void SetupEarlyGameScenario()
{
    RemoveAllPlayers();
    AddMultiplePlayers(45);
    GameManager.Instance.CurrentRound = 1;
    GameManager.Instance.StartGame();
    Debug.Log("DEBUG: Early game scenario loaded");
}

void SetupLateGameScenario()
{
    RemoveAllPlayers();
    AddMultiplePlayers(5);
    GameManager.Instance.CurrentRound = 15;
    GameManager.Instance.StartGame();
    Debug.Log("DEBUG: Late game scenario loaded");
}

void SetupFinalTwoScenario()
{
    RemoveAllPlayers();
    AddMultiplePlayers(2);
    GameManager.Instance.CurrentRound = 20;
    GameManager.Instance.StartGame();
    Debug.Log("DEBUG: Final two scenario loaded");
}
```

---

## Configuration File

### Debug Config JSON

**Location**: `/StreamingAssets/debug_config.json`

```json
{
  "enabled": true,
  "keyboard_shortcuts_enabled": true,
  "overlay_visible_on_start": false,
  "god_mode_default": false,
  "default_time_scale": 1.0,
  "quick_player_counts": [5, 10, 30, 60],
  "log_level": "Info",
  "saved_states": [
    "early_game",
    "mid_game",
    "late_game",
    "final_two"
  ]
}
```

**Loading**:
```csharp
void LoadDebugConfig()
{
    string path = Path.Combine(Application.streamingAssetsPath, "debug_config.json");
    if (File.Exists(path))
    {
        string json = File.ReadAllText(path);
        debugConfig = JsonUtility.FromJson<DebugConfig>(json);
        Debug.Log("DEBUG: Config loaded");
    }
}
```

---

## Implementation Structure

### Recommended File Structure:
```
/Assets/Scripts/Debug/
├── DebugManager.cs           (Main debug controller)
├── DebugOverlayUI.cs         (UI rendering)
├── DebugPlayerManager.cs     (Virtual player creation)
├── DebugModeController.cs    (Mode launching)
├── DebugStateManager.cs      (Save/load states)
└── DebugConfig.cs            (Config data structure)
```

### DebugManager.cs (Singleton):
```csharp
public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    public bool DebugModeEnabled { get; private set; }
    public bool GodModeEnabled { get; private set; }
    public DebugLogLevel LogLevel { get; private set; }

    private DebugOverlayUI overlayUI;
    private DebugPlayerManager playerManager;
    private DebugModeController modeController;

    void Awake()
    {
        #if !UNITY_EDITOR && !DEBUG_BUILD
            Destroy(gameObject);
            return;
        #endif

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDebugSystems();
    }

    void Update()
    {
        HandleKeyboardShortcuts();
    }
}
```

---

## Keyboard Shortcuts Summary

### Players:
- `1` - Add 1 player
- `5` - Add 5 players
- `0` - Add 10 players
- `Shift+0` - Add 20 players
- `Backspace` - Remove 1 player
- `Shift+Backspace` - Remove all players

### Game Control:
- `Space` - Start/restart game
- `N` - Next round
- `E` - End game

### Modes:
- `F1` - Main Round
- `F2` - Sprint
- `F3` - Reaction
- `F4` - Precision
- `F5` - Quick Draw
- `F6` - Tug-o-War
- `F7` - Tap If NOT

### Eliminations:
- `K` - Kill random
- `Shift+K` - Kill half
- `Ctrl+K` - Kill category

### Events:
- `M` - Switch Masks
- `B` - Blind/Demasked

### UI Testing:
- `T` - Title card
- `W` - Winner screen
- `R` - Remaining masks
- `I` - UI inspector

### Cheats:
- `G` - God mode
- `[` - Slow mo (0.5x)
- `]` - Speed up (2x)
- `\` - Normal speed (1x)
- `Shift+W` - Instant win

### Misc:
- `F12` - Toggle debug overlay
- `L` - Cycle log level
- `P` - Performance stats
- `D` - Simulate disconnect
- `Ctrl+S` - Save state
- `Ctrl+L` - Load state

---

## Testing Workflow Examples

### Example 1: Test New Mode UI
```
1. Press F12 (open debug overlay)
2. Click [+10] (add 10 players)
3. Press F5 (launch Quick Draw mode)
4. Observe title card and UI
5. Press N (skip to next round)
6. Repeat for other modes
```

### Example 2: Test Late Game Scenario
```
1. Press F12
2. Click "Late Game" preset (5 players, round 15)
3. Press Space (start game)
4. Press F3 (Reaction Round)
5. Press K (kill random player)
6. Observe with 4 players
7. Press Shift+W (instant win)
```

### Example 3: Test Elimination Edge Cases
```
1. Add 30 players
2. Start game
3. Press Ctrl+K multiple times (kill by category)
4. When 1 category left, test Tug-o-War
5. Observe behavior with unbalanced categories
```

### Example 4: Test UI States
```
1. Add 20 players
2. Press T (test title card)
3. Press W (test winner screen)
4. Press R (test remaining masks)
5. Press I (toggle UI inspector)
6. Verify all UI elements display correctly
```

---

## Safety & Build Configurations

### Development Build:
```csharp
#if UNITY_EDITOR || DEBUG_BUILD
    // All debug features enabled
#endif
```

### Release Build:
```csharp
#if !UNITY_EDITOR && !DEBUG_BUILD
    // Completely disable debug manager
    // No keyboard shortcuts active
    // No performance overhead
#endif
```

### Build Settings:
```
Development Build: Debug features ON
Release Build:     Debug features OFF (compiled out)
```

---

## Priority Implementation Order

### Phase 1 (Essential - 2 hours):
- [ ] F12 toggle overlay
- [ ] Add/remove players (1, 5, 10 buttons)
- [ ] Mode launcher (F1-F4 for existing modes)
- [ ] Next round (N key)
- [ ] Basic UI testing (T, W, R keys)

### Phase 2 (Useful - 1 hour):
- [ ] Kill controls (K, Shift+K)
- [ ] God mode (G key)
- [ ] Time scale ([ ] \ keys)
- [ ] Performance stats (P key)
- [ ] UI inspector (I key)

### Phase 3 (Polish - 1 hour):
- [ ] Player presets (Small/Medium/Large game)
- [ ] Test scenarios (Early/Mid/Late game)
- [ ] Save/load states
- [ ] Debug config file

### Phase 4 (Advanced - 2 hours):
- [ ] Select players UI (clickable grid)
- [ ] Event triggers (M, B keys)
- [ ] Network simulation
- [ ] Log level control

---

## Open Questions

1. Should debug overlay be always visible or toggle-only?
2. Should virtual players have auto-tap behavior option?
3. Should debug saves be persistent between sessions?
4. Need visual feedback for keyboard shortcuts? (toast notifications?)
5. Should debug overlay work in release builds? (probably no)

---

## Notes for Implementation

### When Implementing:
1. Create separate assembly definition for debug code
2. Use conditional compilation to exclude from release builds
3. Keep debug code isolated from game logic
4. Make all debug methods non-intrusive (don't break normal flow)
5. Add tooltips/help text to debug UI
6. Test that release builds fully exclude debug code

### Dependencies:
- Requires PlayerManager, GameManager, ModeSelector (v2.0)
- UI toolkit or IMGUI for debug overlay
- JsonUtility for save/load states

### Performance:
- Debug overlay should have minimal impact (<1ms/frame)
- Virtual players should not create network traffic
- Disable debug systems completely in release builds

---

**Status**: Design complete - ready for implementation when needed
**Estimated Implementation Time**: 6-8 hours total
**Priority**: Phase 1 (essential) should be implemented early for testing efficiency

---

**Last Updated**: 2026-01-30
**Document Version**: 1.0
