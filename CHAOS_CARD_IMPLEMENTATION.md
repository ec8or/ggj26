# Chaos Card Implementation

## What It Does
Before an Advanced Snap round, randomize all player masks. This resets player familiarity and adds drama - everyone gets a NEW mask to learn!

## When to Trigger
- Before the first Advanced Snap round
- Or periodically (e.g., every 2-3 rounds after halfway point)
- Good timing: When ~20-30 players remain

## Implementation Steps

### 1. GameManager.cs - Add Chaos Logic
```csharp
// Add method to trigger chaos
public void TriggerChaosCard()
{
    Debug.Log("🃏 CHAOS CARD ACTIVATED!");

    // Show title screen
    if (UIManager.Instance != null)
    {
        UIManager.Instance.ShowRoundTitle("🃏 CHAOS CARD 🃏\n\nEveryone gets a NEW mask!");
    }

    // Randomize all masks
    PlayerManager.Instance.RandomizeAllMasks();

    // Wait 3-5 seconds for players to see new masks
    StartCoroutine(ChaosCardDelay());
}

IEnumerator ChaosCardDelay()
{
    yield return new WaitForSeconds(4f); // Give players time to see new mask

    // Hide title
    if (UIManager.Instance != null)
    {
        UIManager.Instance.HideRoundTitle();
    }

    // Continue to next round (probably Advanced Snap)
    OnRoundComplete();
}
```

### 2. PlayerManager.cs - Add Randomize Method
```csharp
public void RandomizeAllMasks()
{
    var alivePlayers = GetAlivePlayers();
    var availableMasks = MaskManager.Instance.GetAvailableMaskIds();

    foreach (var player in alivePlayers)
    {
        // Assign random mask (can be same as before or different)
        int newMaskId = availableMasks[Random.Range(0, availableMasks.Count)];
        player.MaskId = newMaskId;

        Debug.Log($"🔀 Player {player.Id} reassigned to Mask #{newMaskId}");

        // Send new mask to mobile client
        NetworkManager.Instance.EmitMaskAssignment(player.Id, newMaskId);
    }
}
```

### 3. MaskManager.cs - Add Helper Method (if needed)
```csharp
public List<int> GetAvailableMaskIds()
{
    List<int> ids = new List<int>();
    for (int i = 0; i < loadedMasks.Count; i++)
    {
        ids.Add(i);
    }
    return ids;
}
```

### 4. NetworkManager.cs - Ensure Mask Re-assignment Works
Check that `EmitMaskAssignment()` already exists and sends to mobile clients:
```csharp
public void EmitMaskAssignment(string playerId, int maskId)
{
    socket.Emit("mask_assigned", new {
        playerId = playerId,
        maskId = maskId,
        maskUrl = $"/masks/mask_{maskId}.png"
    });
}
```

### 5. Game Flow - When to Call It
In `GameManager.cs`, add logic in your round sequencer:
```csharp
// Example: Trigger before first Advanced Snap
if (roundNumber == 5 && GetAliveCount() >= 20) // Adjust round number as needed
{
    TriggerChaosCard();
    return; // ChaosCardDelay() will call OnRoundComplete() when done
}
```

Or add it as a manual trigger:
```csharp
void Update()
{
    // Press 'C' key to manually trigger chaos for testing
    if (Input.GetKeyDown(KeyCode.C))
    {
        TriggerChaosCard();
    }
}
```

## Mobile Client - Should Already Work
The mobile client's `socket.on('mask_assigned')` handler should automatically:
1. Receive the new maskId
2. Load the new mask image
3. Update the tap button with new mask

Check `server/public/app.js` line ~50 to verify this works.

## Testing
1. Start server + bots: `node test-bot.js 40 http://localhost:3000`
2. In Unity, press 'C' key (or let it trigger naturally)
3. Verify:
   - Title screen shows "CHAOS CARD"
   - All player masks get randomized in PlayerManager
   - Mobile clients receive new mask assignments
   - After 4 seconds, game continues to next round

## Visual Enhancement (Optional)
- Add screen shake when chaos triggers
- Particle effects / confetti
- Play a dramatic sound effect (if you add audio)

## Notes
- Players CAN get the same mask they had before (it's random!)
- Make sure to give 3-5 seconds for players to SEE their new mask
- This is most dramatic when ~20-30 players remain
- Consider triggering it only once per game for maximum impact
