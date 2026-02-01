# Pass/Fail Overlay Implementation

## Assets Copied ✅
- `icon-tick.png` (24KB) - Green checkmark
- `icon-cross.png` (11KB) - Red X
- Location: `/unity/Assets/Resources/`

## Implementation Plan

### Option 1: Add Overlays to Mask Prefab (RECOMMENDED)

#### Step 1: Update Mask Prefab in Unity Editor
1. Open your mask prefab (the one referenced in MaskManager)
2. Add two child Image components:
   - Right-click prefab → UI → Image → Name: "TickOverlay"
   - Right-click prefab → UI → Image → Name: "CrossOverlay"

3. Configure each overlay:
   - **Anchors**: Center (0.5, 0.5)
   - **Position**: (0, 0, 0)
   - **Size**: Slightly larger than mask or same size (e.g., 450x450 if mask is 400x400)
   - **Active**: Unchecked (start inactive)
   - **Sprite**: Assign icon-tick.png or icon-cross.png from Resources
   - **Preserve Aspect**: Checked
   - **Raycast Target**: Unchecked (not interactive)

4. Hierarchy should look like:
```
MaskPrefab (Image)
├─ TickOverlay (Image) [inactive]
└─ CrossOverlay (Image) [inactive]
```

#### Step 2: Update MaskManager.cs

Add these methods to MaskManager:

```csharp
// Show tick overlay on a specific mask (survived/passed)
public void ShowTickOverlay(int maskId, float duration = 2.5f)
{
    if (!maskIdToDisplay.ContainsKey(maskId)) return;

    GameObject maskObj = maskIdToDisplay[maskId];
    Transform tickOverlay = maskObj.transform.Find("TickOverlay");

    if (tickOverlay != null)
    {
        tickOverlay.gameObject.SetActive(true);
        StartCoroutine(HideOverlayAfterDelay(tickOverlay.gameObject, duration));
    }
    else
    {
        Debug.LogWarning($"⚠️ TickOverlay not found on mask {maskId}");
    }
}

// Show cross overlay on a specific mask (eliminated/failed)
public void ShowCrossOverlay(int maskId, float duration = 2.5f)
{
    if (!maskIdToDisplay.ContainsKey(maskId)) return;

    GameObject maskObj = maskIdToDisplay[maskId];
    Transform crossOverlay = maskObj.transform.Find("CrossOverlay");

    if (crossOverlay != null)
    {
        crossOverlay.gameObject.SetActive(true);
        StartCoroutine(HideOverlayAfterDelay(crossOverlay.gameObject, duration));
    }
    else
    {
        Debug.LogWarning($"⚠️ CrossOverlay not found on mask {maskId}");
    }
}

// Hide overlay after duration
private System.Collections.IEnumerator HideOverlayAfterDelay(GameObject overlay, float delay)
{
    yield return new WaitForSeconds(delay);
    overlay.SetActive(false);
}

// Show all ticks (for showing survivors)
public void ShowTickOnAllDisplayedMasks(float duration = 2.5f)
{
    foreach (var kvp in maskIdToDisplay)
    {
        ShowTickOverlay(kvp.Key, duration);
    }
}

// Clear all overlays immediately
public void ClearAllOverlays()
{
    foreach (var kvp in maskIdToDisplay)
    {
        GameObject maskObj = kvp.Value;

        Transform tickOverlay = maskObj.transform.Find("TickOverlay");
        if (tickOverlay != null) tickOverlay.gameObject.SetActive(false);

        Transform crossOverlay = maskObj.transform.Find("CrossOverlay");
        if (crossOverlay != null) crossOverlay.gameObject.SetActive(false);
    }
}
```

#### Step 3: Usage in Round Controllers

**In Snap Round (RoundController.cs):**
```csharp
void EvaluateRound()
{
    // ... existing elimination logic ...

    // Show feedback
    foreach (var player in alivePlayers)
    {
        if (player.tappedCorrectly)
        {
            MaskManager.Instance.ShowTickOverlay(player.MaskId, 2.5f);
        }
        else
        {
            MaskManager.Instance.ShowCrossOverlay(player.MaskId, 2.5f);
            PlayerManager.Instance.EliminatePlayer(player.Id, "missed_tap");
        }
    }

    // Wait for overlays to be visible before clearing
    yield return new WaitForSeconds(3f);
    MaskManager.Instance.ClearMasks();
}
```

**In Sprint Round (SprintRound.cs):**
```csharp
void EvaluateSprint()
{
    var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

    foreach (var player in alivePlayers)
    {
        int tapCount = GetPlayerTapCount(player.Id);

        if (tapCount >= 100)
        {
            MaskManager.Instance.ShowTickOverlay(player.MaskId, 2.5f);
        }
        else
        {
            MaskManager.Instance.ShowCrossOverlay(player.MaskId, 2.5f);
            PlayerManager.Instance.EliminatePlayer(player.Id, "failed_sprint");
        }
    }

    yield return new WaitForSeconds(3f);
    // Continue to next round...
}
```

**In Reaction Round (ReactionRound.cs):**
```csharp
void OnPlayerEliminated(string playerId)
{
    var player = PlayerManager.Instance.GetPlayer(playerId);
    if (player != null)
    {
        MaskManager.Instance.ShowCrossOverlay(player.MaskId, 2.5f);
        // Animate mask off screen after overlay shows
        StartCoroutine(RemoveMaskAfterDelay(player.MaskId, 3f));
    }
}

void OnPlayerSurvived(string playerId)
{
    var player = PlayerManager.Instance.GetPlayer(playerId);
    if (player != null)
    {
        MaskManager.Instance.ShowTickOverlay(player.MaskId, 1.5f);
    }
}
```

### Option 2: Dynamic Overlay Creation (Alternative)

If you don't want to modify the prefab, you can create overlays dynamically:

```csharp
public void ShowTickOverlay(int maskId, float duration = 2.5f)
{
    if (!maskIdToDisplay.ContainsKey(maskId)) return;

    GameObject maskObj = maskIdToDisplay[maskId];

    // Create overlay as child
    GameObject overlay = new GameObject("TickOverlay");
    overlay.transform.SetParent(maskObj.transform, false);

    Image img = overlay.AddComponent<Image>();
    img.sprite = Resources.Load<Sprite>("icon-tick");
    img.raycastTarget = false;

    RectTransform rt = overlay.GetComponent<RectTransform>();
    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.one;
    rt.sizeDelta = Vector2.zero;
    rt.anchoredPosition = Vector2.zero;

    StartCoroutine(DestroyOverlayAfterDelay(overlay, duration));
}

private System.Collections.IEnumerator DestroyOverlayAfterDelay(GameObject overlay, float delay)
{
    yield return new WaitForSeconds(delay);
    Destroy(overlay);
}
```

## Visual Enhancement (Optional)

Add fade-in animation to overlays:

```csharp
// In ShowTickOverlay/ShowCrossOverlay, after activating:
Image overlayImg = tickOverlay.GetComponent<Image>();
if (overlayImg != null)
{
    StartCoroutine(FadeInOverlay(overlayImg, 0.3f));
}

private System.Collections.IEnumerator FadeInOverlay(Image img, float duration)
{
    Color c = img.color;
    c.a = 0f;
    img.color = c;

    float elapsed = 0f;
    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        c.a = Mathf.Lerp(0f, 1f, elapsed / duration);
        img.color = c;
        yield return null;
    }

    c.a = 1f;
    img.color = c;
}
```

## Testing

1. Start any round with test bots
2. When round evaluates, check:
   - ✅ Tick appears on survived masks
   - ❌ Cross appears on eliminated masks
   - Overlays visible for ~2.5 seconds
   - Overlays disappear before next round starts
3. Verify overlays are centered and properly sized on masks

## Summary

**Recommended approach:** Add TickOverlay and CrossOverlay as inactive children to your mask prefab. This is cleaner, more performant, and easier to style in the Unity editor.

**Duration:** 2-3 seconds is good - long enough to see, short enough to keep pacing tight.

**Integration:** Call `ShowTickOverlay()` or `ShowCrossOverlay()` after round evaluation, before clearing masks.
