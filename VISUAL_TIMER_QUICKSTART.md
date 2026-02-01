# Visual Round Timer - Quick Implementation

## Problem
Rounds auto-progress after timer expires, but there's no visual indicator on screen. Host and audience don't know when round will end.

## Solution
Add a simple visual timer that depletes as round time runs out.

## Option 1: Radial Fill Circle (Recommended)

### Unity Setup
1. Create UI Image in Canvas
2. Set Image Type: **Filled**
3. Fill Method: **Radial 360**
4. Fill Origin: **Top**
5. Clockwise: **Yes**
6. Fill Amount: **1** (starts full)

### Position
- Top-right corner (small, ~80x80px)
- Or center-top near title
- Keep it consistent across all rounds

### Styling
- White circle outline
- Fill color:
  - Green when > 50% time left
  - Yellow when 25-50% left
  - Red when < 25% left
- Optional: Add text inside showing seconds "8s"

### Code (UIManager.cs)
```csharp
[Header("Round Timer")]
public Image timerFillImage;
public TextMeshProUGUI timerText; // Optional

public void UpdateRoundTimer(float timeRemaining, float totalTime)
{
    if (timerFillImage == null) return;

    float fillAmount = timeRemaining / totalTime;
    timerFillImage.fillAmount = fillAmount;

    // Optional: Color shift
    if (fillAmount > 0.5f)
        timerFillImage.color = Color.green;
    else if (fillAmount > 0.25f)
        timerFillImage.color = Color.yellow;
    else
        timerFillImage.color = Color.red;

    // Optional: Show seconds
    if (timerText != null)
    {
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString() + "s";
    }
}

public void ShowTimer()
{
    if (timerFillImage != null)
        timerFillImage.gameObject.SetActive(true);
}

public void HideTimer()
{
    if (timerFillImage != null)
        timerFillImage.gameObject.SetActive(false);
}
```

### Usage in Rounds
```csharp
// In any round's Update() method
void Update()
{
    if (!active) return;

    timer -= Time.deltaTime;

    // Update visual timer
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateRoundTimer(timer, roundDuration);
    }

    if (timer <= 0)
    {
        EvaluateRound();
    }
}

// When round starts
void StartRound()
{
    // ...
    if (UIManager.Instance != null)
    {
        UIManager.Instance.ShowTimer();
    }
}

// When round ends
void EndRound()
{
    // ...
    if (UIManager.Instance != null)
    {
        UIManager.Instance.HideTimer();
    }
}
```

## Option 2: Simple Progress Bar

### Unity Setup
1. Create UI Panel (background)
2. Create UI Image inside (foreground bar)
3. Anchor foreground to left, stretch vertically
4. Adjust width via RectTransform

### Code
```csharp
public RectTransform timerBar;
public float timerBarMaxWidth = 400f;

public void UpdateRoundTimer(float timeRemaining, float totalTime)
{
    if (timerBar == null) return;

    float width = (timeRemaining / totalTime) * timerBarMaxWidth;
    timerBar.sizeDelta = new Vector2(width, timerBar.sizeDelta.y);
}
```

## Which Rounds Need Timer?

- ✅ **Snap Round** (each loop has duration)
- ✅ **Sprint Round** (10 seconds)
- ✅ **Timing Round** (10 seconds)
- ✅ **Reaction Round** (each red/green cycle has duration)
- ✅ **Chaos Round** (display duration, ~4 seconds)
- ❌ **Title Screens** (wait for SPACE, no auto-advance)

## Testing
1. Start any round with bots
2. Verify timer fills/depletes correctly
3. Check color changes (if implemented)
4. Verify it hides when round ends

## Quick Win
This is a 15-30 minute task with big visual clarity payoff!
